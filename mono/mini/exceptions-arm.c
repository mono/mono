/*
 * exceptions-arm.c: exception support for ARM
 *
 * Authors:
 *   Dietmar Maurer (dietmar@ximian.com)
 *   Paolo Molaro (lupus@ximian.com)
 *
 * (C) 2001 Ximian, Inc.
 */

#include <config.h>
#include <glib.h>
#include <signal.h>
#include <string.h>
#ifdef HAVE_ASM_SIGCONTEXT_H
#include <asm/sigcontext.h>
#endif  /* def HAVE_ASM_SIGCONTEXT_H */
#ifdef HAVE_UCONTEXT_H
#include <ucontext.h>
#endif  /* def HAVE_UCONTEXT_H */

#include <mono/arch/arm/arm-codegen.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/threads.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/mono-debug.h>

#include "mini.h"
#include "mini-arm.h"
#include "mono/utils/mono-sigcontext.h"

/*
 * arch_get_restore_context:
 *
 * Returns a pointer to a method which restores a previously saved sigcontext.
 * The first argument in r0 is the pointer to the context.
 */
gpointer
mono_arch_get_restore_context (MonoTrampInfo **info, gboolean aot)
{
	guint8 *code;
	guint8 *start;
	int ctx_reg;
	MonoJumpInfo *ji = NULL;
	GSList *unwind_ops = NULL;

	start = code = mono_global_codeman_reserve (128);

	/* 
	 * Move things to their proper place so we can restore all the registers with
	 * one instruction.
	 */

	ctx_reg = ARMREG_R0;

	/* move eip to PC */
	ARM_LDR_IMM (code, ARMREG_IP, ctx_reg, G_STRUCT_OFFSET (MonoContext, eip));
	ARM_STR_IMM (code, ARMREG_IP, ctx_reg, G_STRUCT_OFFSET (MonoContext, regs) + (ARMREG_PC * 4));
	/* move sp to SP */
	ARM_LDR_IMM (code, ARMREG_IP, ctx_reg, G_STRUCT_OFFSET (MonoContext, esp));
	ARM_STR_IMM (code, ARMREG_IP, ctx_reg, G_STRUCT_OFFSET (MonoContext, regs) + (ARMREG_SP * 4));

	/* restore everything */
	ARM_ADD_REG_IMM8 (code, ARMREG_IP, ctx_reg, G_STRUCT_OFFSET(MonoContext, regs));
	ARM_LDM (code, ARMREG_IP, 0xffff);

	/* never reached */
	ARM_DBRK (code);

	g_assert ((code - start) < 128);

	mono_arch_flush_icache (start, code - start);

	if (info)
		*info = mono_tramp_info_create (g_strdup_printf ("restore_context"), start, code - start, ji, unwind_ops);

	return start;
}

/*
 * arch_get_call_filter:
 *
 * Returns a pointer to a method which calls an exception filter. We
 * also use this function to call finally handlers (we pass NULL as 
 * @exc object in this case).
 */
gpointer
mono_arch_get_call_filter (MonoTrampInfo **info, gboolean aot)
{
	guint8 *code;
	guint8* start;
	int ctx_reg;
	MonoJumpInfo *ji = NULL;
	GSList *unwind_ops = NULL;

	/* call_filter (MonoContext *ctx, unsigned long eip, gpointer exc) */
	start = code = mono_global_codeman_reserve (320);

	/* save all the regs on the stack */
	ARM_MOV_REG_REG (code, ARMREG_IP, ARMREG_SP);
	ARM_PUSH (code, MONO_ARM_REGSAVE_MASK);

	/* restore all the regs from ctx (in r0), but not sp, the stack pointer */
	ctx_reg = ARMREG_R0;
	ARM_LDR_IMM (code, ARMREG_IP, ctx_reg, G_STRUCT_OFFSET (MonoContext, eip));
	ARM_ADD_REG_IMM8 (code, ARMREG_LR, ctx_reg, G_STRUCT_OFFSET(MonoContext, regs) + (4 * 4));
	ARM_LDM (code, ARMREG_LR, MONO_ARM_REGSAVE_MASK);
	/* call handler at eip (r1) and set the first arg with the exception (r2) */
	ARM_MOV_REG_REG (code, ARMREG_R0, ARMREG_R2);
	ARM_MOV_REG_REG (code, ARMREG_LR, ARMREG_PC);
	ARM_MOV_REG_REG (code, ARMREG_PC, ARMREG_R1);

	/* epilog */
	ARM_POP_NWB (code, 0xff0 | ((1 << ARMREG_SP) | (1 << ARMREG_PC)));

	g_assert ((code - start) < 320);

	mono_arch_flush_icache (start, code - start);

	if (info)
		*info = mono_tramp_info_create (g_strdup_printf ("call_filter"), start, code - start, ji, unwind_ops);

	return start;
}

void
mono_arm_throw_exception (MonoObject *exc, unsigned long eip, unsigned long esp, gulong *int_regs, gdouble *fp_regs)
{
	static void (*restore_context) (MonoContext *);
	MonoContext ctx;
	gboolean rethrow = eip & 1;

	if (!restore_context)
		restore_context = mono_get_restore_context ();

	eip &= ~1; /* clear the optional rethrow bit */
	/* adjust eip so that it point into the call instruction */
	eip -= 4;

	/*printf ("stack in throw: %p\n", esp);*/
	MONO_CONTEXT_SET_BP (&ctx, int_regs [ARMREG_FP - 4]);
	MONO_CONTEXT_SET_SP (&ctx, esp);
	MONO_CONTEXT_SET_IP (&ctx, eip);
	memcpy (((guint8*)&ctx.regs) + (4 * 4), int_regs, sizeof (gulong) * 8);
	/* memcpy (&ctx.fregs, fp_regs, sizeof (double) * MONO_SAVED_FREGS); */

	if (mono_object_isinst (exc, mono_defaults.exception_class)) {
		MonoException *mono_ex = (MonoException*)exc;
		if (!rethrow)
			mono_ex->stack_trace = NULL;
	}
	mono_handle_exception (&ctx, exc, (gpointer)(eip + 4), FALSE);
	restore_context (&ctx);
	g_assert_not_reached ();
}

void
mono_arm_throw_exception_by_token (guint32 type_token, unsigned long eip, unsigned long esp, gulong *int_regs, gdouble *fp_regs)
{
	mono_arm_throw_exception ((MonoObject*)mono_exception_from_token (mono_defaults.corlib, type_token), eip, esp, int_regs, fp_regs);
}

static void
mono_arm_resume_unwind (guint32 dummy1, unsigned long eip, unsigned long esp, gulong *int_regs, gdouble *fp_regs)
{
	MonoContext ctx;

	eip &= ~1; /* clear the optional rethrow bit */
	/* adjust eip so that it point into the call instruction */
	eip -= 4;

	MONO_CONTEXT_SET_BP (&ctx, int_regs [ARMREG_FP - 4]);
	MONO_CONTEXT_SET_SP (&ctx, esp);
	MONO_CONTEXT_SET_IP (&ctx, eip);
	memcpy (((guint8*)&ctx.regs) + (4 * 4), int_regs, sizeof (gulong) * 8);

	mono_resume_unwind (&ctx);
}

/**
 * get_throw_trampoline:
 *
 * Returns a function pointer which can be used to raise 
 * exceptions. The returned function has the following 
 * signature: void (*func) (MonoException *exc); or
 * void (*func) (guint32 ex_token, guint8* ip);
 *
 */
static gpointer 
get_throw_trampoline (int size, gboolean corlib, gboolean rethrow, gboolean llvm, gboolean resume_unwind, const char *tramp_name, MonoTrampInfo **info, gboolean aot)
{
	guint8 *start;
	guint8 *code;
	MonoJumpInfo *ji = NULL;
	GSList *unwind_ops = NULL;

	code = start = mono_global_codeman_reserve (size);

	mono_add_unwind_op_def_cfa (unwind_ops, code, start, ARMREG_SP, 0);

	/* save all the regs on the stack */
	ARM_MOV_REG_REG (code, ARMREG_IP, ARMREG_SP);
	ARM_PUSH (code, MONO_ARM_REGSAVE_MASK);

	mono_add_unwind_op_def_cfa (unwind_ops, code, start, ARMREG_SP, 10 * 4);
	mono_add_unwind_op_offset (unwind_ops, code, start, ARMREG_LR, -4);

	/* call throw_exception (exc, ip, sp, int_regs, fp_regs) */
	/* caller sp */
	ARM_ADD_REG_IMM8 (code, ARMREG_R2, ARMREG_SP, 10 * 4); /* 10 saved regs */
	/* exc is already in place in r0 */
	if (corlib) {
		/* The caller ip is already in R1 */
		if (llvm)
			/* Negate the ip adjustment done in mono_arm_throw_exception */
			ARM_ADD_REG_IMM8 (code, ARMREG_R1, ARMREG_R1, 4);
	} else {
		ARM_MOV_REG_REG (code, ARMREG_R1, ARMREG_LR); /* caller ip */
	}
	/* FIXME: pointer to the saved fp regs */
	/*pos = alloc_size - sizeof (double) * MONO_SAVED_FREGS;
	ppc_addi (code, ppc_r7, ppc_sp, pos);*/
	/* pointer to the saved int regs */
	ARM_MOV_REG_REG (code, ARMREG_R3, ARMREG_SP); /* the pushed regs */
	/* we encode rethrow in the ip, so we avoid args on the stack */
	ARM_ORR_REG_IMM8 (code, ARMREG_R1, ARMREG_R1, rethrow);

	if (aot) {
		ji = mono_patch_info_list_prepend (ji, code - start, MONO_PATCH_INFO_JIT_ICALL_ADDR, corlib ? "mono_arm_throw_exception_by_token" : "mono_arm_throw_exception");
		ARM_LDR_IMM (code, ARMREG_IP, ARMREG_PC, 0);
		ARM_B (code, 0);
		*(gpointer*)(gpointer)code = NULL;
		code += 4;
		ARM_LDR_REG_REG (code, ARMREG_IP, ARMREG_PC, ARMREG_IP);
	} else {
		code = mono_arm_emit_load_imm (code, ARMREG_IP, GPOINTER_TO_UINT (resume_unwind ? (gpointer)mono_arm_resume_unwind : (corlib ? (gpointer)mono_arm_throw_exception_by_token : (gpointer)mono_arm_throw_exception)));
	}
	ARM_MOV_REG_REG (code, ARMREG_LR, ARMREG_PC);
	ARM_MOV_REG_REG (code, ARMREG_PC, ARMREG_IP);
	/* we should never reach this breakpoint */
	ARM_DBRK (code);
	g_assert ((code - start) < size);
	mono_arch_flush_icache (start, code - start);

	if (info)
		*info = mono_tramp_info_create (g_strdup_printf (tramp_name), start, code - start, ji, unwind_ops);

	return start;
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
	return get_throw_trampoline (132, FALSE, FALSE, FALSE, FALSE, "throw_exception", info, aot);
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
	return get_throw_trampoline (132, FALSE, TRUE, FALSE, FALSE, "rethrow_exception", info, aot);
}

/**
 * mono_arch_get_throw_corlib_exception:
 *
 * Returns a function pointer which can be used to raise 
 * corlib exceptions. The returned function has the following 
 * signature: void (*func) (guint32 ex_token, guint32 offset); 
 * Here, offset is the offset which needs to be substracted from the caller IP 
 * to get the IP of the throw. Passing the offset has the advantage that it 
 * needs no relocations in the caller.
 * On ARM, the ip is passed instead of an offset.
 */
gpointer 
mono_arch_get_throw_corlib_exception (MonoTrampInfo **info, gboolean aot)
{
	return get_throw_trampoline (168, TRUE, FALSE, FALSE, FALSE, "throw_corlib_exception", info, aot);
}	

void
mono_arch_exceptions_init (void)
{
	guint8 *tramp;

	if (mono_aot_only) {
	} else {
		/* LLVM uses the normal trampolines, but with a different name */
		tramp = get_throw_trampoline (168, TRUE, FALSE, FALSE, FALSE, "llvm_throw_corlib_exception_trampoline", NULL, FALSE);
		mono_register_jit_icall (tramp, "llvm_throw_corlib_exception_trampoline", NULL, TRUE);

		tramp = get_throw_trampoline (168, TRUE, FALSE, TRUE, FALSE, "llvm_throw_corlib_exception_abs_trampoline", NULL, FALSE);
		mono_register_jit_icall (tramp, "llvm_throw_corlib_exception_abs_trampoline", NULL, TRUE);

		tramp = get_throw_trampoline (168, FALSE, FALSE, FALSE, TRUE, "llvm_resume_unwind_trampoline", NULL, FALSE);
		mono_register_jit_icall (tramp, "llvm_resume_unwind_trampoline", NULL, TRUE);
	}
}

/* 
 * mono_arch_find_jit_info:
 *
 * See exceptions-amd64.c for docs;
 */
gboolean
mono_arch_find_jit_info (MonoDomain *domain, MonoJitTlsData *jit_tls, 
							 MonoJitInfo *ji, MonoContext *ctx, 
							 MonoContext *new_ctx, MonoLMF **lmf, 
							 StackFrameInfo *frame)
{
	gpointer ip = MONO_CONTEXT_GET_IP (ctx);

	memset (frame, 0, sizeof (StackFrameInfo));
	frame->ji = ji;
	frame->managed = FALSE;

	*new_ctx = *ctx;

	if (ji != NULL) {
		int i;
		gssize regs [MONO_MAX_IREGS + 1];
		guint8 *cfa;
		guint32 unwind_info_len;
		guint8 *unwind_info;

		frame->type = FRAME_TYPE_MANAGED;

		if (!ji->method->wrapper_type || ji->method->wrapper_type == MONO_WRAPPER_DYNAMIC_METHOD)
			frame->managed = TRUE;

		if (ji->from_aot)
			unwind_info = mono_aot_get_unwind_info (ji, &unwind_info_len);
		else
			unwind_info = mono_get_cached_unwind_info (ji->used_regs, &unwind_info_len);

		for (i = 0; i < 16; ++i)
			regs [i] = new_ctx->regs [i];
		regs [ARMREG_SP] = new_ctx->esp;

		mono_unwind_frame (unwind_info, unwind_info_len, ji->code_start, 
						   (guint8*)ji->code_start + ji->code_size,
						   ip, regs, MONO_MAX_IREGS, &cfa);

		for (i = 0; i < 16; ++i)
			new_ctx->regs [i] = regs [i];
		new_ctx->eip = regs [ARMREG_LR];
		new_ctx->esp = (gsize)cfa;

		if (*lmf && (MONO_CONTEXT_GET_SP (ctx) >= (gpointer)(*lmf)->esp)) {
			/* remove any unused lmf */
			*lmf = (gpointer)(((gsize)(*lmf)->previous_lmf) & ~3);
		}

		/* we substract 1, so that the IP points into the call instruction */
		new_ctx->eip--;

		return TRUE;
	} else if (*lmf) {

		if (((gsize)(*lmf)->previous_lmf) & 2) {
			/* 
			 * This LMF entry is created by the soft debug code to mark transitions to
			 * managed code done during invokes.
			 */
			MonoLMFExt *ext = (MonoLMFExt*)(*lmf);

			g_assert (ext->debugger_invoke);

			memcpy (new_ctx, &ext->ctx, sizeof (MonoContext));

			*lmf = (gpointer)(((gsize)(*lmf)->previous_lmf) & ~3);

			frame->type = FRAME_TYPE_DEBUGGER_INVOKE;

			return TRUE;
		}

		frame->type = FRAME_TYPE_MANAGED_TO_NATIVE;
		
		if ((ji = mini_jit_info_table_find (domain, (gpointer)(*lmf)->eip, NULL))) {
			frame->ji = ji;
		} else {
			if (!(*lmf)->method)
				return FALSE;
			frame->method = (*lmf)->method;
		}

		/*
		 * The LMF is saved at the start of the method using:
		 * ARM_MOV_REG_REG (code, ARMREG_IP, ARMREG_SP)
		 * ARM_PUSH (code, 0x5ff0);
		 * So it stores the register state as it existed at the caller.
		 */
		memcpy (&new_ctx->regs [0], &(*lmf)->iregs [0], sizeof (gulong) * 13);
		/* SP is skipped */
		new_ctx->regs [ARMREG_LR] = (*lmf)->iregs [ARMREG_LR - 1];
		new_ctx->esp = (*lmf)->iregs [ARMREG_IP];
		new_ctx->eip = new_ctx->regs [ARMREG_LR];

		/* we substract 1, so that the IP points into the call instruction */
		new_ctx->eip--;

		*lmf = (gpointer)(((gsize)(*lmf)->previous_lmf) & ~3);

		return TRUE;
	}

	return FALSE;
}

void
mono_arch_sigctx_to_monoctx (void *sigctx, MonoContext *mctx)
{
#if BROKEN_LINUX
	g_assert_not_reached ();
#else
	arm_ucontext *my_uc = sigctx;

	mctx->eip = UCONTEXT_REG_PC (my_uc);
	mctx->esp = UCONTEXT_REG_SP (my_uc);
	memcpy (&mctx->regs, &UCONTEXT_REG_R0 (my_uc), sizeof (gulong) * 16);
#endif
}

void
mono_arch_monoctx_to_sigctx (MonoContext *mctx, void *ctx)
{
#if BROKEN_LINUX
	g_assert_not_reached ();
#else
	arm_ucontext *my_uc = ctx;

	UCONTEXT_REG_PC (my_uc) = mctx->eip;
	UCONTEXT_REG_SP (my_uc) = mctx->regs [ARMREG_FP];
	/* The upper registers are not guaranteed to be valid */
	memcpy (&UCONTEXT_REG_R0 (my_uc), &mctx->regs, sizeof (gulong) * 12);
#endif
}

/*
 * handle_exception:
 *
 *   Called by resuming from a signal handler.
 */
static void
handle_signal_exception (gpointer obj, gboolean test_only)
{
	MonoJitTlsData *jit_tls = TlsGetValue (mono_jit_tls_id);
	MonoContext ctx;
	static void (*restore_context) (MonoContext *);

	if (!restore_context)
		restore_context = mono_get_restore_context ();

	memcpy (&ctx, &jit_tls->ex_ctx, sizeof (MonoContext));

	mono_handle_exception (&ctx, obj, MONO_CONTEXT_GET_IP (&ctx), test_only);

	restore_context (&ctx);
}

/*
 * This is the function called from the signal handler
 */
gboolean
mono_arch_handle_exception (void *ctx, gpointer obj, gboolean test_only)
{
#if defined(MONO_CROSS_COMPILE)
	g_assert_not_reached ();
#elif defined(MONO_ARCH_USE_SIGACTION)
	arm_ucontext *sigctx = ctx;
	/*
	 * Handling the exception in the signal handler is problematic, since the original
	 * signal is disabled, and we could run arbitrary code though the debugger. So
	 * resume into the normal stack and do most work there if possible.
	 */
	MonoJitTlsData *jit_tls = TlsGetValue (mono_jit_tls_id);
	guint64 sp = UCONTEXT_REG_SP (sigctx);

	/* Pass the ctx parameter in TLS */
	mono_arch_sigctx_to_monoctx (sigctx, &jit_tls->ex_ctx);
	/* The others in registers */
	UCONTEXT_REG_R0 (sigctx) = (gsize)obj;
	UCONTEXT_REG_R1 (sigctx) = test_only;

	/* Allocate a stack frame */
	sp -= 16;
	UCONTEXT_REG_SP (sigctx) = sp;

	UCONTEXT_REG_PC (sigctx) = (gsize)handle_signal_exception;

	return TRUE;
#else
	MonoContext mctx;
	gboolean result;

	mono_arch_sigctx_to_monoctx (ctx, &mctx);

	result = mono_handle_exception (&mctx, obj, (gpointer)mctx.eip, test_only);
	/* restore the context so that returning from the signal handler will invoke
	 * the catch clause 
	 */
	mono_arch_monoctx_to_sigctx (&mctx, ctx);
	return result;
#endif
}

gpointer
mono_arch_ip_from_context (void *sigctx)
{
#if BROKEN_LINUX
	g_assert_not_reached ();
#else
	arm_ucontext *my_uc = sigctx;
	return (void*) UCONTEXT_REG_PC (my_uc);
#endif
}

gboolean
mono_arch_has_unwind_info (gconstpointer addr)
{
	return FALSE;
}

