/*
 * tramp-mips.c: JIT trampoline code for MIPS
 *
 * Authors:
 *    Mark Mason (mason@broadcom.com)
 *
 * Based on tramp-ppc.c by:
 *   Dietmar Maurer (dietmar@ximian.com)
 *   Paolo Molaro (lupus@ximian.com)
 *   Carlos Valiente <yo@virutass.net>
 *
 * (C) 2006 Broadcom
 * (C) 2001 Ximian, Inc.
 */

#include <config.h>
#include <glib.h>

#include <mono/metadata/appdomain.h>
#include <mono/metadata/marshal.h>
#include <mono/metadata/tabledefs.h>
#include <mono/arch/mips/mips-codegen.h>

#include "mini.h"
#include "mini-mips.h"

static guint8* nullified_class_init_trampoline;

/*
 * get_unbox_trampoline:
 * @m: method pointer
 * @addr: pointer to native code for @m
 *
 * when value type methods are called through the vtable we need to unbox the
 * this argument. This method returns a pointer to a trampoline which does
 * unboxing before calling the method
 */
gpointer
mono_arch_get_unbox_trampoline (MonoMethod *m, gpointer addr)
{
	guint8 *code, *start;
	int this_pos = mips_a0;
	MonoDomain *domain = mono_domain_get ();

	if (MONO_TYPE_ISSTRUCT (mono_method_signature (m)->ret))
		this_pos = mips_a1;
	    
	start = code = mono_domain_code_reserve (domain, 20);

	mips_load (code, mips_t9, addr);
	mips_addiu (code, this_pos, this_pos, sizeof (MonoObject));
	mips_jr (code, mips_t9);
	mips_nop (code);

	mono_arch_flush_icache (start, code - start);
	g_assert ((code - start) <= 20);
	/*g_print ("unbox trampoline at %d for %s:%s\n", this_pos, m->klass->name, m->name);
	g_print ("unbox code is at %p for method at %p\n", start, addr);*/

	return start;
}

void
mono_arch_patch_callsite (guint8 *method_start, guint8 *orig_code, guint8 *addr)
{
	guint32 *code = (guint32*)orig_code;

	/* Locate the address of the method-specific trampoline.
	The call using the vtable slot that took the processing flow to
	'arch_create_jit_trampoline' looks something like one of these:

		jal	XXXXYYYY
		nop

		lui	t9, XXXX
		addiu	t9, YYYY
		jalr	t9
		nop

	On entry, 'code' points just after one of the above sequences.
	*/
	
	/* The jal case */
	if ((code[-2] >> 26) == 0x03) {
		//g_print ("direct patching\n");
		mips_patch ((code-2), (gsize)addr);
		return;
	}
	/* Look for the jalr */
	if ((code[-2] & 0xfc1f003f) == 0x00000009) {
		/* The lui / addiu / jalr case */
		if ((code [-4] >> 26) == 0x0f && (code [-3] >> 26) == 0x09
		    && (code [-2] >> 26) == 0) {
			mips_patch ((code-4), (gsize)addr);
			return;
		}
	}
	g_print("error: bad patch at 0x%08x\n", code);
	g_assert_not_reached ();
}

void
mono_arch_patch_plt_entry (guint8 *code, gpointer *got, mgreg_t *regs, guint8 *addr)
{
	g_assert_not_reached ();
}

/* Stack size for trampoline function 
 * MIPS_MINIMAL_STACK_SIZE + 16 (args + alignment to mips_magic_trampoline)
 * + MonoLMF + 14 fp regs + 13 gregs + alignment
 * #define STACK (MIPS_MINIMAL_STACK_SIZE + 4 * sizeof (gulong) + sizeof (MonoLMF) + 14 * sizeof (double) + 13 * (sizeof (gulong)))
 * STACK would be 444 for 32 bit darwin
 */

#define STACK (4*IREG_SIZE + 8 + sizeof(MonoLMF) + 32)


gpointer
mono_arch_get_vcall_slot (guint8 *code_ptr, mgreg_t *regs, int *displacement)
{
	char *o = NULL;
	char *vtable = NULL;
	int reg, offset = 0;
	guint32 base = 0;
	guint32 *code = (guint32*)code_ptr;
	char *sp;

	/* On MIPS, we are passed sp instead of the register array */
	sp = (char*)regs;

	//printf ("mips_magic_trampoline: 0x%08x @ 0x%0x\n", *(code-2), code-2);
	
	/* The jal case */
	if ((code[-2] >> 26) == 0x03)
		return NULL;

	/* Sanity check: look for the jalr */
	g_assert((code[-2] & 0xfc1f003f) == 0x00000009);

	reg = (code[-2] >> 21) & 0x1f;

	//printf ("mips_magic_trampoline: jalr @ 0x%0x, w/ reg %d\n", code-2, reg);

	/* The lui / addiu / jalr case */
	if ((code [-4] >> 26) == 0x0f && (code [-3] >> 26) == 0x09 && (code [-2] >> 26) == 0) {
		return NULL;
	}

	/* Probably a vtable lookup */

	/* Walk backwards to find 'lw reg,XX(base)' */
	for(; --code;) {
		guint32 mask = (0x3f << 26) | (0x1f << 16);
		guint32 match = (0x23 << 26) | (reg << 16);
		if((*code & mask) == match) {
			gint16 soff;
			gint reg_offset;

			/* lw reg,XX(base) */
			base = (*code >> 21) & 0x1f;
			soff = (*code & 0xffff);
			if (soff & 0x8000)
				soff |= 0xffff0000;
			offset = soff;
			if (1) {
				MonoLMF *lmf = (MonoLMF*)((char *)regs + 12*IREG_SIZE);
				g_assert (lmf->magic == MIPS_LMF_MAGIC2);
				o = (gpointer)lmf->iregs [base];
			}
			else {
				o = (gpointer) regs [base];
			}
			break;
		}
	}
	*displacement = offset;
	return o;
}

void
mono_arch_nullify_plt_entry (guint8 *code, mgreg_t *regs)
{
	if (mono_aot_only && !nullified_class_init_trampoline)
		nullified_class_init_trampoline = mono_aot_get_trampoline ("nullified_class_init_trampoline");

	mono_arch_patch_plt_entry (code, NULL, regs, nullified_class_init_trampoline);
}

void
mono_arch_nullify_class_init_trampoline (guint8 *code, mgreg_t *regs)
{
	guint32 *code32 = (guint32*)code;

	/* back up to the jal/jalr instruction */
	code32 -= 2;

	/* Check for jal/jalr -- and NOP it out */
	if ((((*code32)&0xfc000000) == 0x0c000000)
	    || (((*code32)&0xfc1f003f) == 0x00000009)) {
		mips_nop (code32);
		mono_arch_flush_icache ((gpointer)(code32 - 1), 4);
		return;
	}
	g_assert_not_reached ();
}

gpointer
mono_arch_get_nullified_class_init_trampoline (MonoTrampInfo **info)
{
	guint8 *buf, *code;

	code = buf = mono_global_codeman_reserve (16);

	mips_jr (code, mips_ra);
	mips_nop (code);

	mono_arch_flush_icache (buf, code - buf);

	if (info)
		*info = mono_tramp_info_create (g_strdup_printf ("nullified_class_init_trampoline"), buf, code - buf, NULL, NULL);

	return buf;
}

/*
 * Stack frame description when the generic trampoline is called.
 * caller frame
 * --------------------
 *  MonoLMF
 *  -------------------
 *  Saved FP registers 0-13
 *  -------------------
 *  Saved general registers 0-12
 *  -------------------
 *  param area for 3 args to mips_magic_trampoline
 *  -------------------
 *  linkage area
 *  -------------------
 */
guchar*
mono_arch_create_generic_trampoline (MonoTrampolineType tramp_type, MonoTrampInfo **info, gboolean aot)
{
	guint8 *buf, *tramp, *code = NULL;
	int i, lmf;
	GSList *unwind_ops = NULL;
	MonoJumpInfo *ji = NULL;
	int max_code_len = 768;

	/* AOT not supported on MIPS yet */
	g_assert (!aot);

	/* Now we'll create in 'buf' the MIPS trampoline code. This
	   is the trampoline code common to all methods  */
		
	code = buf = mono_global_codeman_reserve (max_code_len);
		
	/* Allocate the stack frame, and save the return address */
	mips_addiu (code, mips_sp, mips_sp, -STACK);
	mips_sw (code, mips_ra, mips_sp, STACK + MIPS_RET_ADDR_OFFSET);

	/* we build the MonoLMF structure on the stack - see mini-mips.h */
	/* offset of MonoLMF from sp */
	lmf = STACK - sizeof (MonoLMF) - 8;

	for (i = 0; i < MONO_MAX_IREGS; i++)
		MIPS_SW (code, i, mips_sp, lmf + G_STRUCT_OFFSET (MonoLMF, iregs[i]));
	for (i = 0; i < MONO_MAX_FREGS; i++)
		MIPS_SWC1 (code, i, mips_sp, lmf + G_STRUCT_OFFSET (MonoLMF, fregs[i]));

	/* Set the magic number */
	mips_load_const (code, mips_at, MIPS_LMF_MAGIC2);
	mips_sw (code, mips_at, mips_sp, lmf + G_STRUCT_OFFSET(MonoLMF, magic));

	/* save method info (it was in t8) */
	mips_sw (code, mips_t8, mips_sp, lmf + G_STRUCT_OFFSET(MonoLMF, method));

	/* save frame pointer (caller fp) */
	MIPS_SW (code, mips_fp, mips_sp, lmf + G_STRUCT_OFFSET(MonoLMF, ebp));

	/* save the IP (caller ip) */
	if (tramp_type == MONO_TRAMPOLINE_JUMP) {
		mips_sw (code, mips_zero, mips_sp, lmf + G_STRUCT_OFFSET(MonoLMF, eip));
	} else {
		mips_sw (code, mips_ra, mips_sp, lmf + G_STRUCT_OFFSET(MonoLMF, eip));
	}

	/* jump to mono_get_lmf_addr here */
	mips_load (code, mips_t9, mono_get_lmf_addr);
	mips_jalr (code, mips_t9, mips_ra);
	mips_nop (code);

	/* v0 now points at the (MonoLMF **) for the current thread */

	/* new_lmf->lmf_addr = lmf_addr -- useful when unwinding */
	mips_sw (code, mips_v0, mips_sp, lmf + G_STRUCT_OFFSET(MonoLMF, lmf_addr));

	/* new_lmf->previous_lmf = *lmf_addr */
	mips_lw (code, mips_at, mips_v0, 0);
	mips_sw (code, mips_at, mips_sp, lmf + G_STRUCT_OFFSET(MonoLMF, previous_lmf));

	/* *(lmf_addr) = new_lmf */
	mips_addiu (code, mips_at, mips_sp, lmf);
	mips_sw (code, mips_at, mips_v0, 0);

	/*
	 * Now we're ready to call mips_magic_trampoline ().
	 */

	/* Arg 1: pointer to registers so that the magic trampoline can
	 * access what we saved above
	 */
	mips_addiu (code, mips_a0, mips_sp, lmf + G_STRUCT_OFFSET (MonoLMF, iregs[0]));

	/* Arg 2: code (next address to the instruction that called us) */
	if (tramp_type == MONO_TRAMPOLINE_JUMP) {
		mips_move (code, mips_a1, mips_zero);
	} else {
		mips_lw (code, mips_a1, mips_sp, STACK + MIPS_RET_ADDR_OFFSET);
	}

	/* Arg 3: MonoMethod *method. */
	mips_lw (code, mips_a2, mips_sp, lmf + G_STRUCT_OFFSET (MonoLMF, method));

	/* Arg 4: Trampoline */
	mips_move (code, mips_a3, mips_zero);
		
	/* Now go to the trampoline */
	tramp = (guint8*)mono_get_trampoline_func (tramp_type);
	mips_load (code, mips_t9, (guint32)tramp);
	mips_jalr (code, mips_t9, mips_ra);
	mips_nop (code);
		
	/* Code address is now in v0, move it to at */
	mips_move (code, mips_at, mips_v0);

	/*
	 * Now unwind the MonoLMF
	 */

	/* t0 = current_lmf->previous_lmf */
	mips_lw (code, mips_t0, mips_sp, lmf + G_STRUCT_OFFSET(MonoLMF, previous_lmf));
	/* t1 = lmf_addr */
	mips_lw (code, mips_t1, mips_sp, lmf + G_STRUCT_OFFSET(MonoLMF, lmf_addr));
	/* (*lmf_addr) = previous_lmf */
	mips_sw (code, mips_t0, mips_t1, 0);

	/* Restore the callee-saved & argument registers */
	for (i = 0; i < MONO_MAX_IREGS; i++) {
		if ((MONO_ARCH_CALLEE_SAVED_REGS | MONO_ARCH_CALLEE_REGS | MIPS_ARG_REGS) & (1 << i))
		    MIPS_LW (code, i, mips_sp, lmf + G_STRUCT_OFFSET (MonoLMF, iregs[i]));
	}
	for (i = 0; i < MONO_MAX_FREGS; i++)
		MIPS_LWC1 (code, i, mips_sp, lmf + G_STRUCT_OFFSET (MonoLMF, fregs[i]));

	/* Non-standard function epilogue. Instead of doing a proper
	 * return, we just jump to the compiled code.
	 */
	/* Restore ra & stack pointer, and jump to the code */

	mips_lw (code, mips_ra, mips_sp, STACK + MIPS_RET_ADDR_OFFSET);
	mips_addiu (code, mips_sp, mips_sp, STACK);
	if (tramp_type == MONO_TRAMPOLINE_CLASS_INIT)
		mips_jr (code, mips_ra);
	else
		mips_jr (code, mips_at);
	mips_nop (code);

	/* Flush instruction cache, since we've generated code */
	mono_arch_flush_icache (buf, code - buf);
	
	/* Sanity check */
	g_assert ((code - buf) <= max_code_len);

	if (tramp_type == MONO_TRAMPOLINE_CLASS_INIT)
		/* Initialize the nullified class init trampoline used in the AOT case */
		nullified_class_init_trampoline = mono_arch_get_nullified_class_init_trampoline (NULL);

	if (info)
		*info = mono_tramp_info_create (mono_get_generic_trampoline_name (tramp_type), buf, code - buf, ji, unwind_ops);

	return buf;
}

gpointer
mono_arch_create_specific_trampoline (gpointer arg1, MonoTrampolineType tramp_type, MonoDomain *domain, guint32 *code_len)
{
	guint8 *code, *buf, *tramp;

	tramp = mono_get_trampoline_code (tramp_type);

	code = buf = mono_domain_code_reserve (domain, 32);

	/* Prepare the jump to the generic trampoline code
	 * mono_arch_create_trampoline_code() knows we're putting this in t8
	 */
	mips_load (code, mips_t8, arg1);
	
	/* Now jump to the generic trampoline code */
	mips_load (code, mips_at, tramp);
	mips_jr (code, mips_at);
	mips_nop (code);

	/* Flush instruction cache, since we've generated code */
	mono_arch_flush_icache (buf, code - buf);

	g_assert ((code - buf) <= 32);

	if (code_len)
		*code_len = code - buf;

	return buf;
}

gpointer
mono_arch_create_rgctx_lazy_fetch_trampoline (guint32 slot, MonoTrampInfo **info, gboolean aot)
{
	/* FIXME: implement! */
	g_assert_not_reached ();
	return NULL;
}

gpointer
mono_arch_create_generic_class_init_trampoline (MonoTrampInfo **info, gboolean aot)
{
	/* FIXME: implement! */
	g_assert_not_reached ();
	return NULL;
}
