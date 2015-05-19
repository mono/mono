/*
 * exceptions-ppc.c: exception support for PowerPC
 *
 * Authors:
 *   Dietmar Maurer (dietmar@ximian.com)
 *   Paolo Molaro (lupus@ximian.com)
 *   Andreas Faerber <andreas.faerber@web.de>
 *
 * (C) 2001 Ximian, Inc.
 * (C) 2007-2008 Andreas Faerber
 */

#include <config.h>
#include <glib.h>
#include <signal.h>
#include <string.h>
#include <stddef.h>
#if HAVE_UCONTEXT_H
#include <ucontext.h>
#endif

#include <mono/arch/ppc/ppc-codegen.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/threads.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/mono-debug.h>

#include "mini.h"
#include "mini-ppc.h"

/*

struct sigcontext {
    int      sc_onstack;     // sigstack state to restore 
    int      sc_mask;        // signal mask to restore 
    int      sc_ir;          // pc 
    int      sc_psw;         // processor status word 
    int      sc_sp;          // stack pointer if sc_regs == NULL 
    void    *sc_regs;        // (kernel private) saved state 
};

struct ucontext {
        int             uc_onstack;
        sigset_t        uc_sigmask;     // signal mask used by this context 
        stack_t         uc_stack;       // stack used by this context 
        struct ucontext *uc_link;       // pointer to resuming context 
        size_t          uc_mcsize;      // size of the machine context passed in 
        mcontext_t      uc_mcontext;    // machine specific context 
};

typedef struct ppc_exception_state {
        unsigned long dar;      // Fault registers for coredump 
        unsigned long dsisr;
        unsigned long exception;// number of powerpc exception taken 
        unsigned long pad0;     // align to 16 bytes 

        unsigned long pad1[4];  // space in PCB "just in case" 
} ppc_exception_state_t;

typedef struct ppc_vector_state {
        unsigned long   save_vr[32][4];
        unsigned long   save_vscr[4];
        unsigned int    save_pad5[4];
        unsigned int    save_vrvalid;                   // VRs that have been saved 
        unsigned int    save_pad6[7];
} ppc_vector_state_t;

typedef struct ppc_float_state {
        double  fpregs[32];

        unsigned int fpscr_pad; // fpscr is 64 bits, 32 bits of rubbish 
        unsigned int fpscr;     // floating point status register 
} ppc_float_state_t;

typedef struct ppc_thread_state {
        unsigned int srr0;      // Instruction address register (PC) 
        unsigned int srr1;      // Machine state register (supervisor) 
        unsigned int r0;
        unsigned int r1;
        unsigned int r2;
	... 
        unsigned int r31;
        unsigned int cr;        // Condition register 
        unsigned int xer;       // User's integer exception register 
        unsigned int lr;        // Link register 
        unsigned int ctr;       // Count register 
        unsigned int mq;        // MQ register (601 only) 

        unsigned int vrsave;    // Vector Save Register 
} ppc_thread_state_t;

struct mcontext {
        ppc_exception_state_t   es;
        ppc_thread_state_t      ss;
        ppc_float_state_t       fs;
        ppc_vector_state_t      vs;
};

typedef struct mcontext  * mcontext_t;

Linux/PPC instead has:
struct sigcontext {
        unsigned long   _unused[4];
        int             signal;
        unsigned long   handler;
        unsigned long   oldmask;
        struct pt_regs  *regs;
};
struct pt_regs {
        unsigned long gpr[32];
        unsigned long nip;
        unsigned long msr;
        unsigned long orig_gpr3;        // Used for restarting system calls 
        unsigned long ctr;
        unsigned long link;
        unsigned long xer;
        unsigned long ccr;
        unsigned long mq;               // 601 only (not used at present) 
                                        // Used on APUS to hold IPL value. 
        unsigned long trap;             // Reason for being here 
        // N.B. for critical exceptions on 4xx, the dar and dsisr
        // fields are overloaded to hold srr0 and srr1. 
        unsigned long dar;              // Fault registers 
        unsigned long dsisr;            // on 4xx/Book-E used for ESR 
        unsigned long result;           // Result of a system call 
};
struct mcontext {
        elf_gregset_t   mc_gregs;
        elf_fpregset_t  mc_fregs;
        unsigned long   mc_pad[2];
        elf_vrregset_t  mc_vregs __attribute__((__aligned__(16)));
};

struct ucontext {
        unsigned long    uc_flags;
        struct ucontext *uc_link;
        stack_t          uc_stack;
        int              uc_pad[7];
        struct mcontext *uc_regs;       // points to uc_mcontext field 
        sigset_t         uc_sigmask;
        // glibc has 1024-bit signal masks, ours are 64-bit 
        int              uc_maskext[30];
        int              uc_pad2[3];
        struct mcontext  uc_mcontext;
};

#define ELF_NGREG       48      // includes nip, msr, lr, etc. 
#define ELF_NFPREG      33      // includes fpscr 

// General registers 
typedef unsigned long elf_greg_t;
typedef elf_greg_t elf_gregset_t[ELF_NGREG];

// Floating point registers 
typedef double elf_fpreg_t;
typedef elf_fpreg_t elf_fpregset_t[ELF_NFPREG];


*/


#define restore_regs_from_context(ctx_reg,ip_reg,tmp_reg) do {	\
		int reg;	\
		ppc_ldptr (code, ip_reg, G_STRUCT_OFFSET (MonoContext, sc_ir), ctx_reg);	\
		ppc_load_multiple_regs (code, ppc_r13, G_STRUCT_OFFSET (MonoContext, regs), ctx_reg);	\
		for (reg = 0; reg < MONO_SAVED_FREGS; ++reg) {	\
			ppc_lfd (code, (14 + reg),	\
				G_STRUCT_OFFSET(MonoContext, fregs) + reg * sizeof (gdouble), ctx_reg);	\
		}	\
	} while (0)

/* nothing to do */
#define setup_context(ctx)

#ifdef PPC_USES_FUNCTION_DESCRIPTOR
guint8*
mono_ppc_create_pre_code_ftnptr (guint8 *code)
{
	MonoPPCFunctionDescriptor *ftnptr = (MonoPPCFunctionDescriptor*)code;

	code += sizeof (MonoPPCFunctionDescriptor);
	ftnptr->code = code;
	ftnptr->toc = NULL;
	ftnptr->env = NULL;

	return code;
}
#endif

/*
 * arch_get_restore_context:
 *
 * Returns a pointer to a method which restores a previously saved sigcontext.
 * The first argument in r3 is the pointer to the context.
 */
gpointer
mono_arch_get_restore_context (MonoTrampInfo **info, gboolean aot)
{
	guint8 *start, *code;
	int size = MONO_PPC_32_64_CASE (128, 172) + PPC_FTNPTR_SIZE;
	MonoJumpInfo *ji = NULL;
	GSList *unwind_ops = NULL;

	code = start = mono_global_codeman_reserve (size);
	if (!aot)
		code = mono_ppc_create_pre_code_ftnptr (code);
	restore_regs_from_context (ppc_r3, ppc_r4, ppc_r5);
	/* restore also the stack pointer */
	ppc_ldptr (code, ppc_sp, G_STRUCT_OFFSET (MonoContext, sc_sp), ppc_r3);
	//ppc_break (code);
	/* jump to the saved IP */
	ppc_mtctr (code, ppc_r4);
	ppc_bcctr (code, PPC_BR_ALWAYS, 0);
	/* never reached */
	ppc_break (code);

	g_assert ((code - start) <= size);
	mono_arch_flush_icache (start, code - start);
	mono_profiler_code_buffer_new (start, code - start, MONO_PROFILER_CODE_BUFFER_EXCEPTION_HANDLING, NULL);

	if (info)
		*info = mono_tramp_info_create ("restore_context", start, code - start, ji, unwind_ops);

	return start;
}

#define SAVED_REGS_LENGTH		(sizeof (gdouble) * MONO_SAVED_FREGS + sizeof (gpointer) * MONO_SAVED_GREGS)
#define ALIGN_STACK_FRAME_SIZE(s)	(((s) + MONO_ARCH_FRAME_ALIGNMENT - 1) & ~(MONO_ARCH_FRAME_ALIGNMENT - 1))
/* The 64 bytes here are for outgoing arguments and a bit of spare.
   We don't use it all, but it doesn't hurt. */
#define REG_SAVE_STACK_FRAME_SIZE	(ALIGN_STACK_FRAME_SIZE (SAVED_REGS_LENGTH + PPC_MINIMAL_STACK_SIZE + 64))

static guint8*
emit_save_saved_regs (guint8 *code, int pos)
{
	int i;

	for (i = 31; i >= 14; --i) {
		pos -= sizeof (gdouble);
		ppc_stfd (code, i, pos, ppc_sp);
	}
	pos -= sizeof (gpointer) * MONO_SAVED_GREGS;
	ppc_store_multiple_regs (code, ppc_r13, pos, ppc_sp);

	return code;
}

/*
 * mono_arch_get_call_filter:
 *
 * Returns a pointer to a method which calls an exception filter. We
 * also use this function to call finally handlers (we pass NULL as 
 * @exc object in this case).
 */
gpointer
mono_arch_get_call_filter (MonoTrampInfo **info, gboolean aot)
{
	guint8 *start, *code;
	int alloc_size, pos, i;
	int size = MONO_PPC_32_64_CASE (320, 500) + PPC_FTNPTR_SIZE;
	MonoJumpInfo *ji = NULL;
	GSList *unwind_ops = NULL;

	/* call_filter (MonoContext *ctx, unsigned long eip, gpointer exc) */
	code = start = mono_global_codeman_reserve (size);
	if (!aot)
		code = mono_ppc_create_pre_code_ftnptr (code);

	/* store ret addr */
	ppc_mflr (code, ppc_r0);
	ppc_stptr (code, ppc_r0, PPC_RET_ADDR_OFFSET, ppc_sp);

	alloc_size = REG_SAVE_STACK_FRAME_SIZE;

	/* allocate stack frame and set link from sp in ctx */
	g_assert ((alloc_size & (MONO_ARCH_FRAME_ALIGNMENT-1)) == 0);
	ppc_ldptr (code, ppc_r0, G_STRUCT_OFFSET (MonoContext, sc_sp), ppc_r3);
	ppc_ldptr_indexed (code, ppc_r0, ppc_r0, ppc_r0);
	ppc_stptr_update (code, ppc_r0, -alloc_size, ppc_sp);

	code = emit_save_saved_regs (code, alloc_size);

	/* restore all the regs from ctx (in r3), but not r1, the stack pointer */
	restore_regs_from_context (ppc_r3, ppc_r6, ppc_r7);
	/* call handler at eip (r4) and set the first arg with the exception (r5) */
	ppc_mtctr (code, ppc_r4);
	ppc_mr (code, ppc_r3, ppc_r5);
	ppc_bcctrl (code, PPC_BR_ALWAYS, 0);

	/* epilog */
	ppc_ldptr (code, ppc_r0, alloc_size + PPC_RET_ADDR_OFFSET, ppc_sp);
	ppc_mtlr (code, ppc_r0);

	/* restore all the regs from the stack */
	pos = alloc_size;
	for (i = 31; i >= 14; --i) {
		pos -= sizeof (gdouble);
		ppc_lfd (code, i, pos, ppc_sp);
	}
	pos -= sizeof (gpointer) * MONO_SAVED_GREGS;
	ppc_load_multiple_regs (code, ppc_r13, pos, ppc_sp);

	ppc_addic (code, ppc_sp, ppc_sp, alloc_size);
	ppc_blr (code);

	g_assert ((code - start) < size);
	mono_arch_flush_icache (start, code - start);
	mono_profiler_code_buffer_new (start, code - start, MONO_PROFILER_CODE_BUFFER_EXCEPTION_HANDLING, NULL);

	if (info)
		*info = mono_tramp_info_create ("call_filter", start, code - start, ji, unwind_ops);

	return start;
}

void
mono_ppc_throw_exception (MonoObject *exc, unsigned long eip, unsigned long esp, mgreg_t *int_regs, gdouble *fp_regs, gboolean rethrow)
{
	MonoContext ctx;

	/* adjust eip so that it point into the call instruction */
	eip -= 4;

	setup_context (&ctx);

	/*printf ("stack in throw: %p\n", esp);*/
	MONO_CONTEXT_SET_BP (&ctx, esp);
	MONO_CONTEXT_SET_IP (&ctx, eip);
	memcpy (&ctx.regs, int_regs, sizeof (mgreg_t) * MONO_SAVED_GREGS);
	memcpy (&ctx.fregs, fp_regs, sizeof (double) * MONO_SAVED_FREGS);

	if (mono_object_isinst (exc, mono_defaults.exception_class)) {
		MonoException *mono_ex = (MonoException*)exc;
		if (!rethrow) {
			mono_ex->stack_trace = NULL;
			mono_ex->trace_ips = NULL;
		}
	}
	mono_handle_exception (&ctx, exc);
	mono_restore_context (&ctx);

	g_assert_not_reached ();
}

/**
 * arch_get_throw_exception_generic:
 *
 * Returns a function pointer which can be used to raise 
 * exceptions. The returned function has the following 
 * signature: void (*func) (MonoException *exc); or
 * void (*func) (guint32 ex_token, gpointer ip)
 *
 */
static gpointer
mono_arch_get_throw_exception_generic (int size, MonoTrampInfo **info, int corlib, gboolean rethrow, gboolean aot)
{
	guint8 *start, *code;
	int alloc_size, pos;
	MonoJumpInfo *ji = NULL;
	GSList *unwind_ops = NULL;

	code = start = mono_global_codeman_reserve (size);
	if (!aot)
		code = mono_ppc_create_pre_code_ftnptr (code);

	/* store ret addr */
	if (corlib)
		ppc_mr (code, ppc_r0, ppc_r4);
	else
		ppc_mflr (code, ppc_r0);
	ppc_stptr (code, ppc_r0, PPC_RET_ADDR_OFFSET, ppc_sp);

	alloc_size = REG_SAVE_STACK_FRAME_SIZE;

	g_assert ((alloc_size & (MONO_ARCH_FRAME_ALIGNMENT-1)) == 0);
	ppc_stptr_update (code, ppc_sp, -alloc_size, ppc_sp);

	code = emit_save_saved_regs (code, alloc_size);

	//ppc_break (code);
	if (corlib) {
		ppc_mr (code, ppc_r4, ppc_r3);

		if (aot) {
			code = mono_arch_emit_load_aotconst (start, code, &ji, MONO_PATCH_INFO_IMAGE, mono_defaults.corlib);
			ppc_mr (code, ppc_r3, ppc_r12);
			code = mono_arch_emit_load_aotconst (start, code, &ji, MONO_PATCH_INFO_JIT_ICALL_ADDR, "mono_exception_from_token");
#ifdef PPC_USES_FUNCTION_DESCRIPTOR
			ppc_ldptr (code, ppc_r2, sizeof (gpointer), ppc_r12);
			ppc_ldptr (code, ppc_r12, 0, ppc_r12);
#endif
			ppc_mtctr (code, ppc_r12);
			ppc_bcctrl (code, PPC_BR_ALWAYS, 0);
		} else {
			ppc_load (code, ppc_r3, (gulong)mono_defaults.corlib);
			ppc_load_func (code, PPC_CALL_REG, mono_exception_from_token);
			ppc_mtctr (code, PPC_CALL_REG);
			ppc_bcctrl (code, PPC_BR_ALWAYS, 0);
		}
	}

	/* call throw_exception (exc, ip, sp, int_regs, fp_regs) */
	/* caller sp */
	ppc_ldptr (code, ppc_r5, 0, ppc_sp);
	/* exc is already in place in r3 */
	if (corlib)
		ppc_ldptr (code, ppc_r4, PPC_RET_ADDR_OFFSET, ppc_r5);
	else
		ppc_mr (code, ppc_r4, ppc_r0); /* caller ip */
	/* pointer to the saved fp regs */
	pos = alloc_size - sizeof (gdouble) * MONO_SAVED_FREGS;
	ppc_addi (code, ppc_r7, ppc_sp, pos);
	/* pointer to the saved int regs */
	pos -= sizeof (gpointer) * MONO_SAVED_GREGS;
	ppc_addi (code, ppc_r6, ppc_sp, pos);
	ppc_li (code, ppc_r8, rethrow);

	if (aot) {
		// This can be called from runtime code, which can't guarantee that
		// r30 contains the got address.
		// So emit the got address loading code too
		code = mono_arch_emit_load_got_addr (start, code, NULL, &ji);
		code = mono_arch_emit_load_aotconst (start, code, &ji, MONO_PATCH_INFO_JIT_ICALL_ADDR, "mono_ppc_throw_exception");
#ifdef PPC_USES_FUNCTION_DESCRIPTOR
		ppc_ldptr (code, ppc_r2, sizeof (gpointer), ppc_r12);
		ppc_ldptr (code, ppc_r12, 0, ppc_r12);
#endif
		ppc_mtctr (code, ppc_r12);
		ppc_bcctrl (code, PPC_BR_ALWAYS, 0);
	} else {
		ppc_load_func (code, PPC_CALL_REG, mono_ppc_throw_exception);
		ppc_mtctr (code, PPC_CALL_REG);
		ppc_bcctrl (code, PPC_BR_ALWAYS, 0);
	}
	/* we should never reach this breakpoint */
	ppc_break (code);
	g_assert ((code - start) <= size);
	mono_arch_flush_icache (start, code - start);
	mono_profiler_code_buffer_new (start, code - start, MONO_PROFILER_CODE_BUFFER_EXCEPTION_HANDLING, NULL);

	if (info)
		*info = mono_tramp_info_create (corlib ? "throw_corlib_exception" : (rethrow ? "rethrow_exception" : "throw_exception"), start, code - start, ji, unwind_ops);

	return start;
}

/**
 * mono_arch_get_rethrow_exception:
 *
 * Returns a function pointer which can be used to rethrow 
 * exceptions. The returned function has the following 
 * signature: void (*func) (MonoException *exc); 
 *
 */
gpointer
mono_arch_get_rethrow_exception (MonoTrampInfo **info, gboolean aot)
{
	int size = MONO_PPC_32_64_CASE (132, 224) + PPC_FTNPTR_SIZE;

	if (aot)
		size += 64;
	return mono_arch_get_throw_exception_generic (size, info, FALSE, TRUE, aot);
}

/**
 * arch_get_throw_exception:
 *
 * Returns a function pointer which can be used to raise 
 * exceptions. The returned function has the following 
 * signature: void (*func) (MonoException *exc); 
 * For example to raise an arithmetic exception you can use:
 *
 * x86_push_imm (code, mono_get_exception_arithmetic ()); 
 * x86_call_code (code, arch_get_throw_exception ()); 
 *
 */
gpointer
mono_arch_get_throw_exception (MonoTrampInfo **info, gboolean aot)
{
	int size = MONO_PPC_32_64_CASE (132, 224) + PPC_FTNPTR_SIZE;

	if (aot)
		size += 64;
	return mono_arch_get_throw_exception_generic (size, info, FALSE, FALSE, aot);
}

/**
 * mono_arch_get_throw_corlib_exception:
 *
 * Returns a function pointer which can be used to raise 
 * corlib exceptions. The returned function has the following 
 * signature: void (*func) (guint32 ex_token, guint32 offset); 
 * On PPC, we pass the ip instead of the offset
 */
gpointer
mono_arch_get_throw_corlib_exception (MonoTrampInfo **info, gboolean aot)
{
	int size = MONO_PPC_32_64_CASE (168, 304) + PPC_FTNPTR_SIZE;

	if (aot)
		size += 64;
	return mono_arch_get_throw_exception_generic (size, info, TRUE, FALSE, aot);
}

/*
 * mono_arch_find_jit_info:
 *
 * See exceptions-amd64.c for docs.
 */
gboolean
mono_arch_find_jit_info (MonoDomain *domain, MonoJitTlsData *jit_tls, 
							 MonoJitInfo *ji, MonoContext *ctx, 
							 MonoContext *new_ctx, MonoLMF **lmf,
							 mgreg_t **save_locations,
							 StackFrameInfo *frame)
{
	gpointer ip = MONO_CONTEXT_GET_IP (ctx);
	MonoPPCStackFrame *sframe;

	memset (frame, 0, sizeof (StackFrameInfo));
	frame->ji = ji;

	*new_ctx = *ctx;
	setup_context (new_ctx);

	if (ji != NULL) {
		int i;
		mgreg_t regs [ppc_lr + 1];
		guint8 *cfa;
		guint32 unwind_info_len;
		guint8 *unwind_info;

		frame->type = FRAME_TYPE_MANAGED;

		unwind_info = mono_jinfo_get_unwind_info (ji, &unwind_info_len);

		sframe = (MonoPPCStackFrame*)MONO_CONTEXT_GET_SP (ctx);
		MONO_CONTEXT_SET_BP (new_ctx, sframe->sp);
		if (jinfo_get_method (ji)->save_lmf) {
			/* sframe->sp points just past the end of the LMF */
			guint8 *lmf_addr = (guint8*)sframe->sp - sizeof (MonoLMF);
			memcpy (&new_ctx->fregs, lmf_addr + G_STRUCT_OFFSET (MonoLMF, fregs), sizeof (double) * MONO_SAVED_FREGS);
			memcpy (&new_ctx->regs, lmf_addr + G_STRUCT_OFFSET (MonoLMF, iregs), sizeof (mgreg_t) * MONO_SAVED_GREGS);
			/* the calling IP is in the parent frame */
			sframe = (MonoPPCStackFrame*)sframe->sp;
			/* we substract 4, so that the IP points into the call instruction */
			MONO_CONTEXT_SET_IP (new_ctx, sframe->lr - 4);
		} else {
			regs [ppc_lr] = ctx->sc_ir;
			regs [ppc_sp] = ctx->sc_sp;
			for (i = 0; i < MONO_SAVED_GREGS; ++i)
				regs [ppc_r13 + i] = ctx->regs [i];

			mono_unwind_frame (unwind_info, unwind_info_len, ji->code_start, 
							   (guint8*)ji->code_start + ji->code_size,
							   ip, NULL, regs, ppc_lr + 1,
							   save_locations, MONO_MAX_IREGS, &cfa);

			/* we substract 4, so that the IP points into the call instruction */
			MONO_CONTEXT_SET_IP (new_ctx, regs [ppc_lr] - 4);
			MONO_CONTEXT_SET_BP (new_ctx, cfa);

			for (i = 0; i < MONO_SAVED_GREGS; ++i)
				new_ctx->regs [i] = regs [ppc_r13 + i];
		}

		return TRUE;
	} else if (*lmf) {
		
		if ((ji = mini_jit_info_table_find (domain, (gpointer)(*lmf)->eip, NULL))) {
		} else {
			if (!(*lmf)->method)
				return FALSE;

			/* Trampoline lmf frame */
			frame->method = (*lmf)->method;
		}

		/*sframe = (MonoPPCStackFrame*)MONO_CONTEXT_GET_SP (ctx);
		MONO_CONTEXT_SET_BP (new_ctx, sframe->sp);
		MONO_CONTEXT_SET_IP (new_ctx, sframe->lr);*/
		MONO_CONTEXT_SET_BP (new_ctx, (*lmf)->ebp);
		MONO_CONTEXT_SET_IP (new_ctx, (*lmf)->eip);
		memcpy (&new_ctx->regs, (*lmf)->iregs, sizeof (mgreg_t) * MONO_SAVED_GREGS);
		memcpy (&new_ctx->fregs, (*lmf)->fregs, sizeof (double) * MONO_SAVED_FREGS);

		frame->ji = ji;
		frame->type = FRAME_TYPE_MANAGED_TO_NATIVE;

		/* FIXME: what about trampoline LMF frames?  see exceptions-x86.c */

		*lmf = (*lmf)->previous_lmf;

		return TRUE;
	}

	return FALSE;
}

gpointer
mono_arch_ip_from_context (void *sigctx)
{
#ifdef MONO_CROSS_COMPILE
	g_assert_not_reached ();
#else
	os_ucontext *uc = sigctx;
	return (gpointer)UCONTEXT_REG_NIP(uc);
#endif
}

void
mono_ppc_set_func_into_sigctx (void *sigctx, void *func)
{
#ifdef MONO_CROSS_COMPILE
	g_assert_not_reached ();
#elif defined(PPC_USES_FUNCTION_DESCRIPTOR)
	/* Have to set both the ip and the TOC reg */
	os_ucontext *uc = sigctx;

	UCONTEXT_REG_NIP(uc) = ((gsize*)func) [0];
	UCONTEXT_REG_Rn (uc, 2) = ((gsize*)func)[1];
#else
	g_assert_not_reached ();
#endif
}

static void
altstack_handle_and_restore (void *sigctx, gpointer obj)
{
	MonoContext mctx;

	mono_sigctx_to_monoctx (sigctx, &mctx);
	mono_handle_exception (&mctx, obj);
	mono_restore_context (&mctx);
}

void
mono_arch_handle_altstack_exception (void *sigctx, MONO_SIG_HANDLER_INFO_TYPE *siginfo, gpointer fault_addr, gboolean stack_ovf)
{
#ifdef MONO_CROSS_COMPILE
	g_assert_not_reached ();
#else
#ifdef MONO_ARCH_USE_SIGACTION
	os_ucontext *uc = (ucontext_t*)sigctx;
	os_ucontext *uc_copy;
	MonoJitInfo *ji = mini_jit_info_table_find (mono_domain_get (), mono_arch_ip_from_context (sigctx), NULL);
	gpointer *sp;
	int frame_size;

	if (stack_ovf) {
		const char *method;
		/* we don't do much now, but we can warn the user with a useful message */
		fprintf (stderr, "Stack overflow: IP: %p, SP: %p\n", mono_arch_ip_from_context (sigctx), (gpointer)UCONTEXT_REG_Rn(uc, 1));
		if (ji && jinfo_get_method (ji))
			method = mono_method_full_name (jinfo_get_method (ji), TRUE);
		else
			method = "Unmanaged";
		fprintf (stderr, "At %s\n", method);
		abort ();
	}
	if (!ji)
		mono_handle_native_sigsegv (SIGSEGV, sigctx, siginfo);
	/* setup a call frame on the real stack so that control is returned there
	 * and exception handling can continue.
	 * The frame looks like:
	 *   ucontext struct
	 *   ...
	 * 224 is the size of the red zone
	 */
	frame_size = sizeof (ucontext_t) + sizeof (gpointer) * 16 + 224;
	frame_size += 15;
	frame_size &= ~15;
	sp = (gpointer)(UCONTEXT_REG_Rn(uc, 1) & ~15);
	sp = (gpointer)((char*)sp - frame_size);
	/* may need to adjust pointers in the new struct copy, depending on the OS */
	uc_copy = (ucontext_t*)(sp + 16);
	memcpy (uc_copy, uc, sizeof (os_ucontext));
#if defined(__linux__) && !defined(__mono_ppc64__)
	uc_copy->uc_mcontext.uc_regs = (gpointer)((char*)uc_copy + ((char*)uc->uc_mcontext.uc_regs - (char*)uc));
#endif
	g_assert (mono_arch_ip_from_context (uc) == mono_arch_ip_from_context (uc_copy));
	/* at the return form the signal handler execution starts in altstack_handle_and_restore() */
	UCONTEXT_REG_LNK(uc) = UCONTEXT_REG_NIP(uc);
#ifdef PPC_USES_FUNCTION_DESCRIPTOR
	{
		MonoPPCFunctionDescriptor *handler_ftnptr = (MonoPPCFunctionDescriptor*)altstack_handle_and_restore;

		UCONTEXT_REG_NIP(uc) = (gulong)handler_ftnptr->code;
		UCONTEXT_REG_Rn(uc, 2) = (gulong)handler_ftnptr->toc;
	}
#else
	UCONTEXT_REG_NIP(uc) = (unsigned long)altstack_handle_and_restore;
#endif
	UCONTEXT_REG_Rn(uc, 1) = (unsigned long)sp;
	UCONTEXT_REG_Rn(uc, PPC_FIRST_ARG_REG) = (unsigned long)(sp + 16);
	UCONTEXT_REG_Rn(uc, PPC_FIRST_ARG_REG + 1) = 0;
	UCONTEXT_REG_Rn(uc, PPC_FIRST_ARG_REG + 2) = 0;
#endif

#endif /* !MONO_CROSS_COMPILE */
}

/*
 * handle_exception:
 *
 *   Called by resuming from a signal handler.
 */
static void
handle_signal_exception (gpointer obj)
{
	MonoJitTlsData *jit_tls = mono_native_tls_get_value (mono_jit_tls_id);
	MonoContext ctx;

	memcpy (&ctx, &jit_tls->ex_ctx, sizeof (MonoContext));

	mono_handle_exception (&ctx, obj);

	mono_restore_context (&ctx);
}

static void
setup_ucontext_return (void *uc, gpointer func)
{
#if !defined(MONO_CROSS_COMPILE)
	UCONTEXT_REG_LNK(uc) = UCONTEXT_REG_NIP(uc);
#ifdef PPC_USES_FUNCTION_DESCRIPTOR
	{
		MonoPPCFunctionDescriptor *handler_ftnptr = (MonoPPCFunctionDescriptor*)func;

		UCONTEXT_REG_NIP(uc) = (gulong)handler_ftnptr->code;
		UCONTEXT_REG_Rn(uc, 2) = (gulong)handler_ftnptr->toc;
	}
#else
	UCONTEXT_REG_NIP(uc) = (unsigned long)func;
#endif
#endif
}

gboolean
mono_arch_handle_exception (void *ctx, gpointer obj)
{
#if defined(MONO_ARCH_USE_SIGACTION) && defined(UCONTEXT_REG_Rn)
	/*
	 * Handling the exception in the signal handler is problematic, since the original
	 * signal is disabled, and we could run arbitrary code though the debugger. So
	 * resume into the normal stack and do most work there if possible.
	 */
	MonoJitTlsData *jit_tls = mono_native_tls_get_value (mono_jit_tls_id);
	mgreg_t sp;
	void *sigctx = ctx;
	int frame_size;
	void *uc = sigctx;

	/* Pass the ctx parameter in TLS */
	mono_sigctx_to_monoctx (sigctx, &jit_tls->ex_ctx);
	/* The others in registers */
	UCONTEXT_REG_Rn (sigctx, PPC_FIRST_ARG_REG) = (gsize)obj;

	/* Allocate a stack frame below the red zone */
	/* Similar to mono_arch_handle_altstack_exception () */
	frame_size = 224;
	frame_size += 15;
	frame_size &= ~15;
	sp = (mgreg_t)(UCONTEXT_REG_Rn(uc, 1) & ~15);
	sp = (mgreg_t)(sp - frame_size);
	UCONTEXT_REG_Rn(uc, 1) = (mgreg_t)sp;
	setup_ucontext_return (uc, handle_signal_exception);

	return TRUE;
#else
	MonoContext mctx;
	gboolean result;

	mono_sigctx_to_monoctx (ctx, &mctx);

	result = mono_handle_exception (&mctx, obj);
	/* restore the context so that returning from the signal handler will invoke
	 * the catch clause 
	 */
	mono_monoctx_to_sigctx (&mctx, ctx);
	return result;
#endif
}
