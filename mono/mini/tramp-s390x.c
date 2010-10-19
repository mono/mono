/*------------------------------------------------------------------*/
/* 								    */
/* Name        - tramp-s390.c      			  	    */
/* 								    */
/* Function    - JIT trampoline code for S/390.                     */
/* 								    */
/* Name	       - Neale Ferguson (Neale.Ferguson@SoftwareAG-usa.com) */
/* 								    */
/* Date        - January, 2004					    */
/* 								    */
/* Derivation  - From exceptions-x86 & exceptions-ppc		    */
/* 	         Paolo Molaro (lupus@ximian.com) 		    */
/* 		 Dietmar Maurer (dietmar@ximian.com)		    */
/* 								    */
/* Copyright   - 2001 Ximian, Inc.				    */
/* 								    */
/*------------------------------------------------------------------*/

/*------------------------------------------------------------------*/
/*                 D e f i n e s                                    */
/*------------------------------------------------------------------*/

#define GR_SAVE_SIZE		4*sizeof(long)
#define FP_SAVE_SIZE		16*sizeof(double)
#define METHOD_SAVE_OFFSET	S390_MINIMAL_STACK_SIZE
#define CREATE_GR_OFFSET	METHOD_SAVE_OFFSET+8
#define CREATE_FP_OFFSET	CREATE_GR_OFFSET+GR_SAVE_SIZE
#define CREATE_LMF_OFFSET	CREATE_FP_OFFSET+FP_SAVE_SIZE
#define CREATE_STACK_SIZE	(CREATE_LMF_OFFSET+2*sizeof(long)+sizeof(MonoLMF))

/*------------------------------------------------------------------*/
/* Method-specific trampoline code fragment sizes		    */
/*------------------------------------------------------------------*/
#define SPECIFIC_TRAMPOLINE_SIZE	96

/*========================= End of Defines =========================*/

/*------------------------------------------------------------------*/
/*                 I n c l u d e s                                  */
/*------------------------------------------------------------------*/

#include <config.h>
#include <glib.h>
#include <string.h>

#include <mono/metadata/appdomain.h>
#include <mono/metadata/marshal.h>
#include <mono/metadata/tabledefs.h>
#include <mono/arch/s390x/s390x-codegen.h>

#include "mini.h"
#include "mini-s390x.h"

/*========================= End of Includes ========================*/

/*------------------------------------------------------------------*/
/*                 T y p e d e f s                                  */
/*------------------------------------------------------------------*/

/*========================= End of Typedefs ========================*/

/*------------------------------------------------------------------*/
/*                   P r o t o t y p e s                            */
/*------------------------------------------------------------------*/

/*========================= End of Prototypes ======================*/

/*------------------------------------------------------------------*/
/*                 G l o b a l   V a r i a b l e s                  */
/*------------------------------------------------------------------*/


/*====================== End of Global Variables ===================*/

/*------------------------------------------------------------------*/
/*                                                                  */
/* Name		- mono_arch_get_unbox_trampoline                    */
/*                                                                  */
/* Function	- Return a pointer to a trampoline which does the   */
/*		  unboxing before calling the method.		    */
/*                                                                  */
/*                When value type methods are called through the    */
/*		  vtable we need to unbox the 'this' argument.	    */
/*		                               		 	    */
/* Parameters   - method - Methd pointer			    */
/*		  addr   - Pointer to native code for method	    */
/*		                               		 	    */
/*------------------------------------------------------------------*/

gpointer
mono_arch_get_unbox_trampoline (MonoMethod *method, gpointer addr)
{
	guint8 *code, *start;
	int this_pos = s390_r2;
	MonoDomain *domain = mono_domain_get ();

	start = code = mono_domain_code_reserve (domain, 28);

	s390_basr (code, s390_r1, 0);
	s390_j	  (code, 6);
	s390_llong(code, addr);
	s390_lg   (code, s390_r1, 0, s390_r1, 4);
	s390_aghi (code, this_pos, sizeof(MonoObject));
	s390_br   (code, s390_r1);

	g_assert ((code - start) <= 28);

	mono_arch_flush_icache (start, code - start);

	return start;
}

/*========================= End of Function ========================*/

/*------------------------------------------------------------------*/
/*                                                                  */
/* Name		- mono_arch_patch_callsite                          */
/*                                                                  */
/* Function	- Patch a non-virtual callsite so it calls @addr.   */
/*                                                                  */
/*------------------------------------------------------------------*/

void
mono_arch_patch_callsite (guint8 *method_start, guint8 *orig_code, guint8 *addr)
{
	gint32 displace;
	unsigned short opcode;

	opcode = *((unsigned short *) (orig_code - 6));
	if (opcode == 0xc0e5) {
		/* This is the 'brasl' instruction */
		orig_code    -= 4;
		displace = ((gssize) addr - (gssize) (orig_code - 2)) / 2;
		s390_patch_rel (orig_code, displace);
		mono_arch_flush_icache (orig_code, 4);
	} else {
		/* This should be a 'lg %r14,4(%r13)' then a 'basr r14, r14' instruction */
		g_assert (orig_code [-8] == 0xe3);
		g_assert (orig_code [-7] == 0xe0);
		g_assert (orig_code [-6] == 0xd0);
		g_assert (orig_code [-5] == 0x04);
		g_assert (orig_code [-4] == 0x00);
		g_assert (orig_code [-3] == 0x04);
		opcode = *((unsigned short*) (orig_code - 2));
		g_assert (opcode == 0x0dee);

		/* The call address is stored in the 8 bytes preceeding the basr instruction */
		s390_patch_addr(orig_code - 16, (gssize)addr);
		mono_arch_flush_icache (orig_code - 16, 8);
	}
}

/*========================= End of Function ========================*/

/*------------------------------------------------------------------*/
/*                                                                  */
/* Name		- mono_arch_patch_plt_entry.                        */
/*                                                                  */
/* Function	- Patch a PLT entry - unused as yet.                */
/*                                                                  */
/*------------------------------------------------------------------*/

void
mono_arch_patch_plt_entry (guint8 *code, gpointer *got, mgreg_t *regs, guint8 *addr)
{
	g_assert_not_reached ();
}

/*========================= End of Function ========================*/

/*------------------------------------------------------------------*/
/*                                                                  */
/* Name		- mono_arch_nullify_class_init_trampoline           */
/*                                                                  */
/* Function	- Nullify a call which calls a class init trampoline*/
/*                                                                  */
/*------------------------------------------------------------------*/

void
mono_arch_nullify_class_init_trampoline (guint8 *code, mgreg_t *regs)
{
	char patch[2] = {0x07, 0x00};

	code = code - 2;

	memcpy(code, patch, sizeof(patch));
}

/*========================= End of Function ========================*/

/*------------------------------------------------------------------*/
/*                                                                  */
/* Name		- mono_arch_nullify_plt_entry			    */
/*                                                                  */
/* Function	- Nullify a PLT entry call.			    */
/*                                                                  */
/*------------------------------------------------------------------*/

void
mono_arch_nullify_plt_entry (guint8 *code, mgreg_t *regs)
{
	g_assert_not_reached ();
}

/*========================= End of Function ========================*/

/*------------------------------------------------------------------*/
/*                                                                  */
/* Name		- mono_arch_create_trampoline_code                  */
/*                                                                  */
/* Function	- Create the designated type of trampoline according*/
/*                to the 'tramp_type' parameter.                    */
/*                                                                  */
/*------------------------------------------------------------------*/

guchar*
mono_arch_create_generic_trampoline (MonoTrampolineType tramp_type, MonoTrampInfo **info, gboolean aot)
{
	guint8 *buf, *tramp, *code;
	int i, offset, lmfOffset;

	g_assert (!aot);
	if (info)
		*info = NULL;

	/* Now we'll create in 'buf' the S/390 trampoline code. This
	   is the trampoline code common to all methods  */
		
	code = buf = mono_global_codeman_reserve(512);
		
	/*-----------------------------------------------------------
	  STEP 0: First create a non-standard function prologue with a
	  stack size big enough to save our registers.
	  -----------------------------------------------------------*/
		
	s390_stmg (buf, s390_r6, s390_r15, STK_BASE, S390_REG_SAVE_OFFSET);
	s390_lgr  (buf, s390_r11, s390_r15);
	s390_aghi (buf, STK_BASE, -CREATE_STACK_SIZE);
	s390_stg  (buf, s390_r11, 0, STK_BASE, 0);
	s390_stg  (buf, s390_r1, 0, STK_BASE, METHOD_SAVE_OFFSET);
	s390_stmg (buf, s390_r2, s390_r5, STK_BASE, CREATE_GR_OFFSET);

	/* Save the FP registers */
	offset = CREATE_FP_OFFSET;
	for (i = s390_f0; i <= s390_f15; ++i) {
		s390_std  (buf, i, 0, STK_BASE, offset);
		offset += 8;
	}

	/*----------------------------------------------------------
	  STEP 1: call 'mono_get_lmf_addr()' to get the address of our
	  LMF. We'll need to restore it after the call to
	  's390_magic_trampoline' and before the call to the native
	  method.
	  ----------------------------------------------------------*/
				
	s390_basr (buf, s390_r13, 0);
	s390_j	  (buf, 6);
	s390_llong(buf, mono_get_lmf_addr);
	s390_lg   (buf, s390_r1, 0, s390_r13, 4);
	s390_basr (buf, s390_r14, s390_r1);

	/*---------------------------------------------------------------*/
	/* we build the MonoLMF structure on the stack - see mini-s390.h */
	/* Keep in sync with the code in mono_arch_emit_prolog 		 */
	/*---------------------------------------------------------------*/
	lmfOffset = CREATE_STACK_SIZE - sizeof(MonoLMF);
											
	s390_lgr   (buf, s390_r13, STK_BASE);
	s390_aghi  (buf, s390_r13, lmfOffset);	
											
	/*---------------------------------------------------------------*/	
	/* Set lmf.lmf_addr = jit_tls->lmf				 */	
	/*---------------------------------------------------------------*/	
	s390_stg   (buf, s390_r2, 0, s390_r13, 				
			    G_STRUCT_OFFSET(MonoLMF, lmf_addr));			
											
	/*---------------------------------------------------------------*/	
	/* Get current lmf						 */	
	/*---------------------------------------------------------------*/	
	s390_lg    (buf, s390_r0, 0, s390_r2, 0);				
											
	/*---------------------------------------------------------------*/	
	/* Set our lmf as the current lmf				 */	
	/*---------------------------------------------------------------*/	
	s390_stg   (buf, s390_r13, 0, s390_r2, 0);				
											
	/*---------------------------------------------------------------*/	
	/* Have our lmf.previous_lmf point to the last lmf		 */	
	/*---------------------------------------------------------------*/	
	s390_stg   (buf, s390_r0, 0, s390_r13, 				
			    G_STRUCT_OFFSET(MonoLMF, previous_lmf));			
											
	/*---------------------------------------------------------------*/	
	/* save method info						 */	
	/*---------------------------------------------------------------*/	
	s390_lg    (buf, s390_r1, 0, STK_BASE, METHOD_SAVE_OFFSET);
	s390_stg   (buf, s390_r1, 0, s390_r13, 				
			    G_STRUCT_OFFSET(MonoLMF, method));				
									
	/*---------------------------------------------------------------*/	
	/* save the current SP						 */	
	/*---------------------------------------------------------------*/	
	s390_lg    (buf, s390_r1, 0, STK_BASE, 0);
	s390_stg   (buf, s390_r1, 0, s390_r13, G_STRUCT_OFFSET(MonoLMF, ebp));	
									
	/*---------------------------------------------------------------*/	
	/* save the current IP						 */	
	/*---------------------------------------------------------------*/	
	if (tramp_type == MONO_TRAMPOLINE_JUMP) {
		s390_lghi  (buf, s390_r1, 0);
	} else {
		s390_lg    (buf, s390_r1, 0, s390_r1, S390_RET_ADDR_OFFSET);
		//			s390_la    (buf, s390_r1, 0, s390_r1, 0);
	}
	s390_stg   (buf, s390_r1, 0, s390_r13, G_STRUCT_OFFSET(MonoLMF, eip));	
											
	/*---------------------------------------------------------------*/	
	/* Save general and floating point registers			 */	
	/*---------------------------------------------------------------*/	
	s390_mvc   (buf, 4*sizeof(gulong), s390_r13, G_STRUCT_OFFSET(MonoLMF, gregs[2]), 
		    STK_BASE, CREATE_GR_OFFSET);
	s390_mvc   (buf, 10*sizeof(gulong), s390_r13, G_STRUCT_OFFSET(MonoLMF, gregs[6]), 
		    s390_r11, S390_REG_SAVE_OFFSET);

	/* Simply copy fpregs already saved above			 */
	s390_mvc   (buf, 16*sizeof(double), s390_r13, G_STRUCT_OFFSET(MonoLMF, fregs[0]),
		    STK_BASE, CREATE_FP_OFFSET);

	/*---------------------------------------------------------------*/
	/* STEP 2: call the C trampoline function                        */
	/*---------------------------------------------------------------*/
				
	/* Set arguments */

	/* Arg 1: mgreg_t *regs. We pass sp instead */
	s390_la  (buf, s390_r2, 0, STK_BASE, CREATE_STACK_SIZE);
		
	/* Arg 2: code (next address to the instruction that called us) */
	if (tramp_type == MONO_TRAMPOLINE_JUMP) {
		s390_lghi (buf, s390_r3, 0);
	} else {
		s390_lg   (buf, s390_r3, 0, s390_r11, S390_RET_ADDR_OFFSET);
	}

	/* Arg 3: MonoMethod *method. It was put in r1 by the
	   method-specific trampoline code, and then saved before the call
	   to mono_get_lmf_addr()'. */
	s390_lg  (buf, s390_r4, 0, STK_BASE, METHOD_SAVE_OFFSET);

	/* Arg 4: trampoline address. Ignore for now */
		
	/* Calculate call address and call the C trampoline. Return value will be in r2 */
	s390_basr (buf, s390_r13, 0);
	s390_j	  (buf, 6);
	tramp = (guint8*)mono_get_trampoline_func (tramp_type);
	s390_llong (buf, tramp);
	s390_lg   (buf, s390_r1, 0, s390_r13, 4);
	s390_basr (buf, s390_r14, s390_r1);
		
	/* OK, code address is now on r2. Move it to r1, so that we
	   can restore r2 and use it from r1 later */
	s390_lgr  (buf, s390_r1, s390_r2);

	/*----------------------------------------------------------
	  STEP 3: Restore the LMF
	  ----------------------------------------------------------*/
	restoreLMF(buf, STK_BASE, CREATE_STACK_SIZE);
	
	/*----------------------------------------------------------
	  STEP 4: call the compiled method
	  ----------------------------------------------------------*/
		
	/* Restore registers */

	s390_lmg  (buf, s390_r2, s390_r5, STK_BASE, CREATE_GR_OFFSET);
		
	/* Restore the FP registers */
	offset = CREATE_FP_OFFSET;
	for (i = s390_f0; i <= s390_f15; ++i) {
		s390_ld  (buf, i, 0, STK_BASE, offset);
		offset += 8;
	}

	/* Restore stack pointer and jump to the code -
	   R14 contains the return address to our caller */
	s390_lgr  (buf, STK_BASE, s390_r11);
	s390_lmg  (buf, s390_r6, s390_r14, STK_BASE, S390_REG_SAVE_OFFSET);

	if (tramp_type == MONO_TRAMPOLINE_CLASS_INIT || tramp_type == MONO_TRAMPOLINE_GENERIC_CLASS_INIT)
		s390_br (buf, s390_r14);
	else
		s390_br (buf, s390_r1);

	/* Flush instruction cache, since we've generated code */
	mono_arch_flush_icache (code, buf - code);
	
	/* Sanity check */
	g_assert ((buf - code) <= 512);

	return code;
}

/*========================= End of Function ========================*/

/*------------------------------------------------------------------*/
/*                                                                  */
/* Name		- mono_arch_create_specific_trampoline              */
/*                                                                  */
/* Function	- Creates the given kind of specific trampoline     */
/*                                                                  */
/*------------------------------------------------------------------*/

gpointer
mono_arch_create_specific_trampoline (gpointer arg1, MonoTrampolineType tramp_type, MonoDomain *domain, guint32 *code_len)
{
	guint8 *code, *buf, *tramp;
	gint32 displace;

	tramp = mono_get_trampoline_code (tramp_type);

	/*----------------------------------------------------------*/
	/* This is the method-specific part of the trampoline. Its  */
	/* purpose is to provide the generic part with the          */
	/* MonoMethod *method pointer. We'll use r1 to keep it.     */
	/*----------------------------------------------------------*/
	code = buf = mono_domain_code_reserve (domain, SPECIFIC_TRAMPOLINE_SIZE);

	s390_basr (buf, s390_r1, 0);
	s390_j	  (buf, 6);
	s390_llong(buf, arg1);
	s390_lg   (buf, s390_r1, 0, s390_r1, 4);
	displace = (tramp - buf) / 2;
	s390_jcl  (buf, S390_CC_UN, displace);

	/* Flush instruction cache, since we've generated code */
	mono_arch_flush_icache (code, buf - code);

	/* Sanity check */
	g_assert ((buf - code) <= SPECIFIC_TRAMPOLINE_SIZE);

	if (code_len)
		*code_len = buf - code;
	
	return code;
}	

/*========================= End of Function ========================*/

/*------------------------------------------------------------------*/
/*                                                                  */
/* Name		- mono_arch_create_rgctx_lazy_fetch_trampoline      */
/*                                                                  */
/* Function	- 						    */
/*                                                                  */
/*------------------------------------------------------------------*/

gpointer
mono_arch_create_rgctx_lazy_fetch_trampoline (guint32 slot, MonoTrampInfo **info, gboolean aot)
{
	/* FIXME: implement! */
	g_assert_not_reached ();
	return NULL;
}	

/*========================= End of Function ========================*/
