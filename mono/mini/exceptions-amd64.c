/**
 * \file
 * exception support for AMD64
 *
 * Authors:
 *   Dietmar Maurer (dietmar@ximian.com)
 *   Johan Lorensson (lateralusx.github@gmail.com)
 *
 * (C) 2001 Ximian, Inc.
 * Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include <config.h>

// Secret password to unlock wcscat_s on mxe, must happen before string.h included
#ifdef __MINGW32__
#define MINGW_HAS_SECURE_API 1
#endif

#include <glib.h>
#include <string.h>
#include <signal.h>
#ifdef HAVE_UCONTEXT_H
#include <ucontext.h>
#endif

#include <mono/arch/amd64/amd64-codegen.h>
#include <mono/metadata/abi-details.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/threads.h>
#include <mono/metadata/threads-types.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/gc-internals.h>
#include <mono/metadata/mono-debug.h>
#include <mono/utils/mono-mmap.h>
#include <mono/utils/mono-state.h>
#include <mono/utils/w32subset.h>

#include "mini.h"
#include "mini-amd64.h"
#include "mini-runtime.h"
#include "aot-runtime.h"
#include "tasklets.h"
#include "mono/utils/mono-tls-inline.h"

#ifndef DISABLE_JIT
/*
 * mono_arch_get_restore_context:
 *
 * Returns a pointer to a method which restores a previously saved sigcontext.
 */
gpointer
mono_arch_get_restore_context (MonoTrampInfo **info, gboolean aot)
{
	guint8 *start = NULL;
	guint8 *code;
	MonoJumpInfo *ji = NULL;
	GSList *unwind_ops = NULL;
	int i, gregs_offset;

	/* restore_contect (MonoContext *ctx) */

	const int size = 256;

	start = code = (guint8 *)mono_global_codeman_reserve (size);

	amd64_mov_reg_reg (code, AMD64_R11, AMD64_ARG_REG1, 8);

	/* Restore all registers except %rip and %r11 */
	gregs_offset = MONO_STRUCT_OFFSET (MonoContext, gregs);
	for (i = 0; i < AMD64_NREG; ++i) {
		if (i != AMD64_RIP && i != AMD64_RSP && i != AMD64_R8 && i != AMD64_R9 && i != AMD64_R10 && i != AMD64_R11)
			amd64_mov_reg_membase (code, i, AMD64_R11, gregs_offset + (i * 8), 8);
	}

	/*
	 * The context resides on the stack, in the stack frame of the
	 * caller of this function.  The stack pointer that we need to
	 * restore is potentially many stack frames higher up, so the
	 * distance between them can easily be more than the red zone
	 * size.  Hence the stack pointer can be restored only after
	 * we have finished loading everything from the context.
	 */
	amd64_mov_reg_membase (code, AMD64_R8, AMD64_R11,  gregs_offset + (AMD64_RSP * 8), 8);
	amd64_mov_reg_membase (code, AMD64_R11, AMD64_R11,  gregs_offset + (AMD64_RIP * 8), 8);
	amd64_mov_reg_reg (code, AMD64_RSP, AMD64_R8, 8);

	/* jump to the saved IP */
	amd64_jump_reg (code, AMD64_R11);

	g_assertf ((code - start) <= size, "%d %d", (int)(code - start), size);

	mono_arch_flush_icache (start, code - start);
	MONO_PROFILER_RAISE (jit_code_buffer, (start, code - start, MONO_PROFILER_CODE_BUFFER_EXCEPTION_HANDLING, NULL));

	if (info)
		*info = mono_tramp_info_create ("restore_context", start, code - start, ji, unwind_ops);

	return start;
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
	guint8 *start;
	int i, gregs_offset;
	guint8 *code;
	guint32 pos;
	MonoJumpInfo *ji = NULL;
	GSList *unwind_ops = NULL;
	const int kMaxCodeSize = 128;

	start = code = (guint8 *)mono_global_codeman_reserve (kMaxCodeSize);

	/* call_filter (MonoContext *ctx, unsigned long eip) */
	code = start;

	/* Alloc new frame */
	amd64_push_reg (code, AMD64_RBP);
	amd64_mov_reg_reg (code, AMD64_RBP, AMD64_RSP, 8);

	/* Save callee saved regs */
	pos = 0;
	for (i = 0; i < AMD64_NREG; ++i)
		if (AMD64_IS_CALLEE_SAVED_REG (i)) {
			amd64_push_reg (code, i);
			pos += 8;
		}

	/* Save EBP */
	pos += 8;
	amd64_push_reg (code, AMD64_RBP);

	/* Make stack misaligned, the call will make it aligned again */
	if (! (pos & 8))
		amd64_alu_reg_imm (code, X86_SUB, AMD64_RSP, 8);

	gregs_offset = MONO_STRUCT_OFFSET (MonoContext, gregs);

	/* set new EBP */
	amd64_mov_reg_membase (code, AMD64_RBP, AMD64_ARG_REG1, gregs_offset + (AMD64_RBP * 8), 8);
	/* load callee saved regs */
	for (i = 0; i < AMD64_NREG; ++i) {
		if (AMD64_IS_CALLEE_SAVED_REG (i) && i != AMD64_RBP)
			amd64_mov_reg_membase (code, i, AMD64_ARG_REG1, gregs_offset + (i * 8), 8);
	}
	/* load exc register */
	amd64_mov_reg_membase (code, AMD64_RAX, AMD64_ARG_REG1,  gregs_offset + (AMD64_RAX * 8), 8);

	/* call the handler */
	amd64_call_reg (code, AMD64_ARG_REG2);

	if (! (pos & 8))
		amd64_alu_reg_imm (code, X86_ADD, AMD64_RSP, 8);

	/* restore RBP */
	amd64_pop_reg (code, AMD64_RBP);

	/* Restore callee saved regs */
	for (i = AMD64_NREG; i >= 0; --i)
		if (AMD64_IS_CALLEE_SAVED_REG (i))
			amd64_pop_reg (code, i);

#if TARGET_WIN32
	amd64_lea_membase (code, AMD64_RSP, AMD64_RBP, 0);
	amd64_pop_reg (code, AMD64_RBP);
#else
	amd64_leave (code);
#endif
	amd64_ret (code);

	g_assertf ((code - start) <= kMaxCodeSize, "%d %d", (int)(code - start), kMaxCodeSize);

	mono_arch_flush_icache (start, code - start);
	MONO_PROFILER_RAISE (jit_code_buffer, (start, code - start, MONO_PROFILER_CODE_BUFFER_EXCEPTION_HANDLING, NULL));

	if (info)
		*info = mono_tramp_info_create ("call_filter", start, code - start, ji, unwind_ops);

	return start;
}
#endif /* !DISABLE_JIT */

/* 
 * The first few arguments are dummy, to force the other arguments to be passed on
 * the stack, this avoids overwriting the argument registers in the throw trampoline.
 */
void
mono_amd64_throw_exception (guint64 dummy1, guint64 dummy2, guint64 dummy3, guint64 dummy4,
							guint64 dummy5, guint64 dummy6,
							MonoContext *mctx, MonoObject *exc, gboolean rethrow, gboolean preserve_ips)
{
	ERROR_DECL (error);
	MonoContext ctx;

	/* mctx is on the caller's stack */
	memcpy (&ctx, mctx, sizeof (MonoContext));

	if (mono_object_isinst_checked (exc, mono_defaults.exception_class, error)) {
		MonoException *mono_ex = (MonoException*)exc;
		if (!rethrow && !mono_ex->caught_in_unmanaged) {
			mono_ex->stack_trace = NULL;
			mono_ex->trace_ips = NULL;
		} else if (preserve_ips) {
			mono_ex->caught_in_unmanaged = TRUE;
		}
	}
	mono_error_assert_ok (error);

	/* adjust eip so that it point into the call instruction */
	ctx.gregs [AMD64_RIP] --;

	mono_handle_exception (&ctx, exc);
	mono_restore_context (&ctx);
	g_assert_not_reached ();
}

void
mono_amd64_throw_corlib_exception (guint64 dummy1, guint64 dummy2, guint64 dummy3, guint64 dummy4,
								   guint64 dummy5, guint64 dummy6,
								   MonoContext *mctx, guint32 ex_token_index, gint64 pc_offset)
{
	guint32 ex_token = MONO_TOKEN_TYPE_DEF | ex_token_index;
	MonoException *ex;

	ex = mono_exception_from_token (m_class_get_image (mono_defaults.exception_class), ex_token);

	mctx->gregs [AMD64_RIP] -= pc_offset;

	/* Negate the ip adjustment done in mono_amd64_throw_exception () */
	mctx->gregs [AMD64_RIP] += 1;

	mono_amd64_throw_exception (dummy1, dummy2, dummy3, dummy4, dummy5, dummy6, mctx, (MonoObject*)ex, FALSE, FALSE);
}

void
mono_amd64_resume_unwind (guint64 dummy1, guint64 dummy2, guint64 dummy3, guint64 dummy4,
						  guint64 dummy5, guint64 dummy6,
						  MonoContext *mctx, guint32 dummy7, gint64 dummy8)
{
	/* Only the register parameters are valid */
	MonoContext ctx;

	/* mctx is on the caller's stack */
	memcpy (&ctx, mctx, sizeof (MonoContext));

	mono_resume_unwind (&ctx);
}

#ifndef DISABLE_JIT
/*
 * get_throw_trampoline:
 *
 *  Generate a call to mono_amd64_throw_exception/
 * mono_amd64_throw_corlib_exception.
 */
static gpointer
get_throw_trampoline (MonoTrampInfo **info, gboolean rethrow, gboolean corlib, gboolean llvm_abs, gboolean resume_unwind, const char *tramp_name, gboolean aot, gboolean preserve_ips)
{
	guint8* start;
	guint8 *code;
	MonoJumpInfo *ji = NULL;
	GSList *unwind_ops = NULL;
	int i, stack_size, arg_offsets [16], ctx_offset, regs_offset;
	const int kMaxCodeSize = 256;

#ifdef TARGET_WIN32
	const int dummy_stack_space = 6 * sizeof (target_mgreg_t);	/* Windows expects stack space allocated for all 6 dummy args. */
#else
	const int dummy_stack_space = 0;
#endif

	if (info)
		start = code = (guint8 *)mono_global_codeman_reserve (kMaxCodeSize + MONO_MAX_TRAMPOLINE_UNWINDINFO_SIZE);
	else
		start = code = (guint8 *)mono_global_codeman_reserve (kMaxCodeSize);

	/* The stack is unaligned on entry */
	stack_size = ALIGN_TO (sizeof (MonoContext) + 64 + dummy_stack_space, MONO_ARCH_FRAME_ALIGNMENT) + 8;

	code = start;

	if (info)
		unwind_ops = mono_arch_get_cie_program ();

	/* Alloc frame */
	amd64_alu_reg_imm (code, X86_SUB, AMD64_RSP, stack_size);
	if (info) {
		mono_add_unwind_op_def_cfa_offset (unwind_ops, code, start, stack_size + 8);
		mono_add_unwind_op_sp_alloc (unwind_ops, code, start, stack_size);
	}

	/*
	 * To hide linux/windows calling convention differences, we pass all arguments on
	 * the stack by passing 6 dummy values in registers.
	 */

	arg_offsets [0] = dummy_stack_space + 0;
	arg_offsets [1] = dummy_stack_space + sizeof (target_mgreg_t);
	arg_offsets [2] = dummy_stack_space + sizeof (target_mgreg_t) * 2;
	arg_offsets [3] = dummy_stack_space + sizeof (target_mgreg_t) * 3;
	ctx_offset = dummy_stack_space + sizeof (target_mgreg_t) * 4;
	regs_offset = ctx_offset + MONO_STRUCT_OFFSET (MonoContext, gregs);

	/* Save registers */
	for (i = 0; i < AMD64_NREG; ++i)
		if (i != AMD64_RSP)
			amd64_mov_membase_reg (code, AMD64_RSP, regs_offset + (i * sizeof (target_mgreg_t)), i, sizeof (target_mgreg_t));
	/* Save RSP */
	amd64_lea_membase (code, AMD64_RAX, AMD64_RSP, stack_size + sizeof (target_mgreg_t));
	amd64_mov_membase_reg (code, AMD64_RSP, regs_offset + (AMD64_RSP * sizeof (target_mgreg_t)), X86_EAX, sizeof (target_mgreg_t));
	/* Save IP */
	amd64_mov_reg_membase (code, AMD64_RAX, AMD64_RSP, stack_size, sizeof (target_mgreg_t));
	amd64_mov_membase_reg (code, AMD64_RSP, regs_offset + (AMD64_RIP * sizeof (target_mgreg_t)), AMD64_RAX, sizeof (target_mgreg_t));
	/* Set arg1 == ctx */
	amd64_lea_membase (code, AMD64_RAX, AMD64_RSP, ctx_offset);
	amd64_mov_membase_reg (code, AMD64_RSP, arg_offsets [0], AMD64_RAX, sizeof (target_mgreg_t));
	/* Set arg2 == exc/ex_token_index */
	if (resume_unwind)
		amd64_mov_membase_imm (code, AMD64_RSP, arg_offsets [1], 0, sizeof (target_mgreg_t));
	else
		amd64_mov_membase_reg (code, AMD64_RSP, arg_offsets [1], AMD64_ARG_REG1, sizeof (target_mgreg_t));
	/* Set arg3 == rethrow/pc offset */
	if (resume_unwind) {
		amd64_mov_membase_imm (code, AMD64_RSP, arg_offsets [2], 0, sizeof (target_mgreg_t));
	} else if (corlib) {
		if (llvm_abs)
			/*
			 * The caller doesn't pass in a pc/pc offset, instead we simply use the
			 * caller ip. Negate the pc adjustment done in mono_amd64_throw_corlib_exception ().
			 */
			amd64_mov_membase_imm (code, AMD64_RSP, arg_offsets [2], 1, sizeof  (target_mgreg_t));
		else
			amd64_mov_membase_reg (code, AMD64_RSP, arg_offsets [2], AMD64_ARG_REG2, sizeof (target_mgreg_t));
	} else {
		amd64_mov_membase_imm (code, AMD64_RSP, arg_offsets [2], rethrow, sizeof (target_mgreg_t));

		/* Set arg4 == preserve_ips */
		amd64_mov_membase_imm (code, AMD64_RSP, arg_offsets [3], preserve_ips, sizeof (target_mgreg_t));
	}

	if (aot) {
		MonoJitICallId icall_id;

		if (resume_unwind)
			icall_id = MONO_JIT_ICALL_mono_amd64_resume_unwind;
		else if (corlib)
			icall_id = MONO_JIT_ICALL_mono_amd64_throw_corlib_exception;
		else
			icall_id = MONO_JIT_ICALL_mono_amd64_throw_exception;
		ji = mono_patch_info_list_prepend (ji, code - start, MONO_PATCH_INFO_JIT_ICALL_ADDR, GUINT_TO_POINTER (icall_id));
		amd64_mov_reg_membase (code, AMD64_R11, AMD64_RIP, 0, 8);
	} else {
		amd64_mov_reg_imm (code, AMD64_R11, resume_unwind ? ((gpointer)mono_amd64_resume_unwind) : (corlib ? (gpointer)mono_amd64_throw_corlib_exception : (gpointer)mono_amd64_throw_exception));
	}
	amd64_call_reg (code, AMD64_R11);
	amd64_breakpoint (code);

	mono_arch_flush_icache (start, code - start);

	g_assertf ((code - start) <= kMaxCodeSize, "%d %d", (int)(code - start), kMaxCodeSize);
	g_assert_checked (mono_arch_unwindinfo_validate_size (unwind_ops, MONO_MAX_TRAMPOLINE_UNWINDINFO_SIZE));

	MONO_PROFILER_RAISE (jit_code_buffer, (start, code - start, MONO_PROFILER_CODE_BUFFER_EXCEPTION_HANDLING, NULL));

	if (info)
		*info = mono_tramp_info_create (tramp_name, start, code - start, ji, unwind_ops);

	return start;
}

/**
 * mono_arch_get_throw_exception:
 * \returns a function pointer which can be used to raise 
 * exceptions. The returned function has the following 
 * signature: void (*func) (MonoException *exc); 
 */
gpointer
mono_arch_get_throw_exception (MonoTrampInfo **info, gboolean aot)
{
	return get_throw_trampoline (info, FALSE, FALSE, FALSE, FALSE, "throw_exception", aot, FALSE);
}

gpointer 
mono_arch_get_rethrow_exception (MonoTrampInfo **info, gboolean aot)
{
	return get_throw_trampoline (info, TRUE, FALSE, FALSE, FALSE, "rethrow_exception", aot, FALSE);
}

gpointer 
mono_arch_get_rethrow_preserve_exception (MonoTrampInfo **info, gboolean aot)
{
	return get_throw_trampoline (info, TRUE, FALSE, FALSE, FALSE, "rethrow_preserve_exception", aot, TRUE);
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
 */
gpointer 
mono_arch_get_throw_corlib_exception (MonoTrampInfo **info, gboolean aot)
{
	return get_throw_trampoline (info, FALSE, TRUE, FALSE, FALSE, "throw_corlib_exception", aot, FALSE);
}
#endif /* !DISABLE_JIT */

/*
 * mono_arch_unwind_frame:
 *
 * This function is used to gather information from @ctx, and store it in @frame_info.
 * It unwinds one stack frame, and stores the resulting context into @new_ctx. @lmf
 * is modified if needed.
 * Returns TRUE on success, FALSE otherwise.
 */
gboolean
mono_arch_unwind_frame (MonoDomain *domain, MonoJitTlsData *jit_tls, 
							 MonoJitInfo *ji, MonoContext *ctx, 
							 MonoContext *new_ctx, MonoLMF **lmf,
							 host_mgreg_t **save_locations,
							 StackFrameInfo *frame)
{
	gpointer ip = MONO_CONTEXT_GET_IP (ctx);
	int i;

	memset (frame, 0, sizeof (StackFrameInfo));
	frame->ji = ji;

	*new_ctx = *ctx;

	if (ji != NULL) {
		host_mgreg_t regs [MONO_MAX_IREGS + 1];
		guint8 *cfa;
		guint32 unwind_info_len;
		guint8 *unwind_info;
		guint8 *epilog = NULL;

		if (ji->is_trampoline)
			frame->type = FRAME_TYPE_TRAMPOLINE;
		else
			frame->type = FRAME_TYPE_MANAGED;

		unwind_info = mono_jinfo_get_unwind_info (ji, &unwind_info_len);

		frame->unwind_info = unwind_info;
		frame->unwind_info_len = unwind_info_len;

		/*
		printf ("%s %p %p\n", ji->d.method->name, ji->code_start, ip);
		mono_print_unwind_info (unwind_info, unwind_info_len);
		*/
		/* LLVM compiled code doesn't have this info */
		if (ji->has_arch_eh_info)
			epilog = (guint8*)ji->code_start + ji->code_size - mono_jinfo_get_epilog_size (ji);
 
		for (i = 0; i < AMD64_NREG; ++i)
			regs [i] = new_ctx->gregs [i];

		gboolean success = mono_unwind_frame (unwind_info, unwind_info_len, (guint8 *)ji->code_start,
						   (guint8*)ji->code_start + ji->code_size,
						   (guint8 *)ip, epilog ? &epilog : NULL, regs, MONO_MAX_IREGS + 1,
						   save_locations, MONO_MAX_IREGS, &cfa);

		if (!success)
			return FALSE;

		for (i = 0; i < AMD64_NREG; ++i)
			new_ctx->gregs [i] = regs [i];
 
		/* The CFA becomes the new SP value */
		new_ctx->gregs [AMD64_RSP] = (host_mgreg_t)(gsize)cfa;

		/* Adjust IP */
		new_ctx->gregs [AMD64_RIP] --;

		return TRUE;
	} else if (*lmf) {
		guint64 rip;

		g_assert ((((guint64)(*lmf)->previous_lmf) & 2) == 0);

		if (((guint64)(*lmf)->previous_lmf) & 4) {
			MonoLMFTramp *ext = (MonoLMFTramp*)(*lmf);

			rip = (guint64)MONO_CONTEXT_GET_IP (ext->ctx);
		} else if ((*lmf)->rsp == 0) {
			/* Top LMF entry */
			return FALSE;
		} else {
			/* 
			 * The rsp field is set just before the call which transitioned to native 
			 * code. Obtain the rip from the stack.
			 */
			rip = *(guint64*)((*lmf)->rsp - sizeof(host_mgreg_t));
		}

		ji = mini_jit_info_table_find (domain, (char *)rip, NULL);
		/*
		 * FIXME: ji == NULL can happen when a managed-to-native wrapper is interrupted
		 * in the soft debugger suspend code, since (*lmf)->rsp no longer points to the
		 * return address.
		 */
		//g_assert (ji);
		if (!ji)
			return FALSE;

		frame->ji = ji;
		frame->type = FRAME_TYPE_MANAGED_TO_NATIVE;

		if (((guint64)(*lmf)->previous_lmf) & 4) {
			MonoLMFTramp *ext = (MonoLMFTramp*)(*lmf);

			/* Trampoline frame */
			for (i = 0; i < AMD64_NREG; ++i)
				new_ctx->gregs [i] = ext->ctx->gregs [i];
			/* Adjust IP */
			new_ctx->gregs [AMD64_RIP] --;
		} else {
			/*
			 * The registers saved in the LMF will be restored using the normal unwind info,
			 * when the wrapper frame is processed.
			 */
			/* Adjust IP */
			rip --;
			new_ctx->gregs [AMD64_RIP] = rip;
			new_ctx->gregs [AMD64_RSP] = (*lmf)->rsp;
			new_ctx->gregs [AMD64_RBP] = (*lmf)->rbp;
			for (i = 0; i < AMD64_NREG; ++i) {
				if (AMD64_IS_CALLEE_SAVED_REG (i) && i != AMD64_RBP)
					new_ctx->gregs [i] = 0;
			}
		}

		*lmf = (MonoLMF *)(((guint64)(*lmf)->previous_lmf) & ~7);

		return TRUE;
	}

	return FALSE;
}

/*
 * handle_exception:
 *
 *   Called by resuming from a signal handler.
 */
static void
handle_signal_exception (gpointer obj)
{
	MonoJitTlsData *jit_tls = mono_tls_get_jit_tls ();
	MonoContext ctx;

	memcpy (&ctx, &jit_tls->ex_ctx, sizeof (MonoContext));

	mono_handle_exception (&ctx, (MonoObject *)obj);

	mono_restore_context (&ctx);
}

void
mono_arch_setup_async_callback (MonoContext *ctx, void (*async_cb)(void *fun), gpointer user_data)
{
	guint64 sp = ctx->gregs [AMD64_RSP];

	ctx->gregs [AMD64_RDI] = (gsize)user_data;

	/* Allocate a stack frame below the red zone */
	sp -= 128;
	/* The stack should be unaligned */
	if ((sp % 16) == 0)
		sp -= 8;
#ifdef __linux__
	/* Preserve the call chain to prevent crashes in the libgcc unwinder (#15969) */
	*(guint64*)sp = ctx->gregs [AMD64_RIP];
#endif
	ctx->gregs [AMD64_RSP] = sp;
	ctx->gregs [AMD64_RIP] = (gsize)async_cb;
}

/**
 * mono_arch_handle_exception:
 * \param ctx saved processor state
 * \param obj the exception object
 */
gboolean
mono_arch_handle_exception (void *sigctx, gpointer obj)
{
#if defined(MONO_ARCH_USE_SIGACTION)
	MonoContext mctx;

	/*
	 * Handling the exception in the signal handler is problematic, since the original
	 * signal is disabled, and we could run arbitrary code though the debugger. So
	 * resume into the normal stack and do most work there if possible.
	 */
	MonoJitTlsData *jit_tls = mono_tls_get_jit_tls ();

	/* Pass the ctx parameter in TLS */
	mono_sigctx_to_monoctx (sigctx, &jit_tls->ex_ctx);

	mctx = jit_tls->ex_ctx;
	mono_arch_setup_async_callback (&mctx, handle_signal_exception, obj);
	mono_monoctx_to_sigctx (&mctx, sigctx);

	return TRUE;
#else
	MonoContext mctx;

	mono_sigctx_to_monoctx (sigctx, &mctx);

	mono_handle_exception (&mctx, obj);

	mono_monoctx_to_sigctx (&mctx, sigctx);

	return TRUE;
#endif
}

gpointer
mono_arch_ip_from_context (void *sigctx)
{
#if defined(MONO_ARCH_USE_SIGACTION)
	ucontext_t *ctx = (ucontext_t*)sigctx;

	return (gpointer)UCONTEXT_REG_RIP (ctx);
#elif defined(HOST_WIN32)
	return (gpointer)(((CONTEXT*)sigctx)->Rip);
#else
	MonoContext *ctx = (MonoContext*)sigctx;
	return (gpointer)ctx->gregs [AMD64_RIP];
#endif	
}

static MonoObject*
restore_soft_guard_pages (void)
{
	MonoJitTlsData *jit_tls = mono_tls_get_jit_tls ();
	if (jit_tls->stack_ovf_guard_base)
		mono_mprotect (jit_tls->stack_ovf_guard_base, jit_tls->stack_ovf_guard_size, MONO_MMAP_NONE);

	if (jit_tls->stack_ovf_pending) {
		MonoDomain *domain = mono_domain_get ();
		jit_tls->stack_ovf_pending = 0;
		return (MonoObject *) domain->stack_overflow_ex;
	}

	return NULL;
}

/* 
 * this function modifies mctx so that when it is restored, it
 * won't execcute starting at mctx.eip, but in a function that
 * will restore the protection on the soft-guard pages and return back to
 * continue at mctx.eip.
 */
static void
prepare_for_guard_pages (MonoContext *mctx)
{
	gpointer *sp;
	sp = (gpointer *)(mctx->gregs [AMD64_RSP]);
	sp -= 1;
	/* the return addr */
	sp [0] = (gpointer)(mctx->gregs [AMD64_RIP]);
	mctx->gregs [AMD64_RIP] = (guint64)restore_soft_guard_pages;
	mctx->gregs [AMD64_RSP] = (guint64)sp;
}

static void
altstack_handle_and_restore (MonoContext *ctx, MonoObject *obj, guint32 flags)
{
	MonoContext mctx;
	MonoJitInfo *ji = mini_jit_info_table_find (mono_domain_get (), MONO_CONTEXT_GET_IP (ctx), NULL);
	gboolean stack_ovf = (flags & 1) != 0;
	gboolean nullref = (flags & 2) != 0;

	if (!ji || (!stack_ovf && !nullref)) {
		if (mono_dump_start ())
			mono_handle_native_crash (mono_get_signame (SIGSEGV), ctx, NULL);
		// if couldn't dump or if mono_handle_native_crash returns, abort
		abort ();
	}

	mctx = *ctx;

	mono_handle_exception (&mctx, obj);
	if (stack_ovf) {
		MonoJitTlsData *jit_tls = mono_tls_get_jit_tls ();
		jit_tls->stack_ovf_pending = 1;
		prepare_for_guard_pages (&mctx);
	}
	mono_restore_context (&mctx);
}

void
mono_arch_handle_altstack_exception (void *sigctx, MONO_SIG_HANDLER_INFO_TYPE *siginfo, gpointer fault_addr, gboolean stack_ovf)
{
#if defined(MONO_ARCH_USE_SIGACTION)
	MonoException *exc = NULL;
	gpointer *sp;
	MonoJitTlsData *jit_tls = NULL;
	MonoContext *copied_ctx = NULL;
	gboolean nullref = TRUE;

	jit_tls = mono_tls_get_jit_tls ();
	g_assert (jit_tls);

	/* use TLS as temporary storage as we want to avoid
	 * (1) stack allocation on the application stack
	 * (2) calling malloc, because it is not async-safe
	 * (3) using a global storage, because this function is not reentrant
	 *
	 * tls->orig_ex_ctx is used by the stack walker, which shouldn't be running at this point.
	 */
	copied_ctx = &jit_tls->orig_ex_ctx;

	if (!mono_is_addr_implicit_null_check (fault_addr))
		nullref = FALSE;

	if (stack_ovf)
		exc = mono_domain_get ()->stack_overflow_ex;

	/* setup the call frame on the application stack so that control is
	 * returned there and exception handling can continue. we want the call
	 * frame to be minimal as possible, for example no argument passing that
	 * requires allocation on the stack, as this wouldn't be encoded in unwind
	 * information for the caller frame.
	 */
	sp = (gpointer *) ALIGN_DOWN_TO (UCONTEXT_REG_RSP (sigctx), 16);
	sp [-1] = (gpointer)UCONTEXT_REG_RIP (sigctx);
	mono_sigctx_to_monoctx (sigctx, copied_ctx);
	/* at the return from the signal handler execution starts in altstack_handle_and_restore() */
	UCONTEXT_REG_RIP (sigctx) = (unsigned long)altstack_handle_and_restore;
	UCONTEXT_REG_RSP (sigctx) = (unsigned long)(sp - 1);
	UCONTEXT_REG_RDI (sigctx) = (unsigned long)(copied_ctx);
	UCONTEXT_REG_RSI (sigctx) = (guint64)exc;
	UCONTEXT_REG_RDX (sigctx) = (stack_ovf ? 1 : 0) | (nullref ? 2 : 0);
#endif
}

#ifndef DISABLE_JIT
GSList*
mono_amd64_get_exception_trampolines (gboolean aot)
{
	MonoTrampInfo *info;
	GSList *tramps = NULL;

	// FIXME Macro to make one line per trampoline.

	/* LLVM needs different throw trampolines */
	get_throw_trampoline (&info, FALSE, TRUE, FALSE, FALSE, "llvm_throw_corlib_exception_trampoline", aot, FALSE);
	info->jit_icall_info = &mono_get_jit_icall_info ()->mono_llvm_throw_corlib_exception_trampoline;
	tramps = g_slist_prepend (tramps, info);

	get_throw_trampoline (&info, FALSE, TRUE, TRUE, FALSE, "llvm_throw_corlib_exception_abs_trampoline", aot, FALSE);
	info->jit_icall_info = &mono_get_jit_icall_info ()->mono_llvm_throw_corlib_exception_abs_trampoline;
	tramps = g_slist_prepend (tramps, info);

	get_throw_trampoline (&info, FALSE, TRUE, TRUE, TRUE, "llvm_resume_unwind_trampoline", aot, FALSE);
	info->jit_icall_info = &mono_get_jit_icall_info ()->mono_llvm_resume_unwind_trampoline;
	tramps = g_slist_prepend (tramps, info);

	return tramps;
}

#else

GSList*
mono_amd64_get_exception_trampolines (gboolean aot)
{
	g_assert_not_reached ();
	return NULL;
}

#endif /* !DISABLE_JIT */

void
mono_arch_exceptions_init (void)
{
	GSList *tramps, *l;
	gpointer tramp;

	if (mono_ee_features.use_aot_trampolines) {

		// FIXME Macro can make one line per trampoline here.
		tramp = mono_aot_get_trampoline ("llvm_throw_corlib_exception_trampoline");
		mono_register_jit_icall_info (&mono_get_jit_icall_info ()->mono_llvm_throw_corlib_exception_trampoline, tramp, "llvm_throw_corlib_exception_trampoline", NULL, TRUE, NULL);

		tramp = mono_aot_get_trampoline ("llvm_throw_corlib_exception_abs_trampoline");
		mono_register_jit_icall_info (&mono_get_jit_icall_info ()->mono_llvm_throw_corlib_exception_abs_trampoline, tramp, "llvm_throw_corlib_exception_abs_trampoline", NULL, TRUE, NULL);

		tramp = mono_aot_get_trampoline ("llvm_resume_unwind_trampoline");
		mono_register_jit_icall_info (&mono_get_jit_icall_info ()->mono_llvm_resume_unwind_trampoline, tramp, "llvm_resume_unwind_trampoline", NULL, TRUE, NULL);

	} else if (!mono_llvm_only) {
		/* Call this to avoid initialization races */
		tramps = mono_amd64_get_exception_trampolines (FALSE);
		for (l = tramps; l; l = l->next) {
			MonoTrampInfo *info = (MonoTrampInfo *)l->data;

			mono_register_jit_icall_info (info->jit_icall_info, info->code, g_strdup (info->name), NULL, TRUE, NULL);
			mono_tramp_info_register (info, NULL);
		}
		g_slist_free (tramps);
	}
}

// Implies defined(TARGET_WIN32)
#ifdef MONO_ARCH_HAVE_UNWIND_TABLE

static void
mono_arch_unwindinfo_create (gpointer* monoui)
{
	PUNWIND_INFO newunwindinfo;
	*monoui = newunwindinfo = g_new0 (UNWIND_INFO, 1);
	newunwindinfo->Version = 1;
}

void
mono_arch_unwindinfo_add_push_nonvol (PUNWIND_INFO unwindinfo, MonoUnwindOp *unwind_op)
{
	PUNWIND_CODE unwindcode;
	guchar codeindex;

	g_assert (unwindinfo != NULL);

	if (unwindinfo->CountOfCodes >= MONO_MAX_UNWIND_CODES)
		g_error ("Larger allocation needed for the unwind information.");

	codeindex = MONO_MAX_UNWIND_CODES - (++unwindinfo->CountOfCodes);
	unwindcode = &unwindinfo->UnwindCode [codeindex];
	unwindcode->UnwindOp = UWOP_PUSH_NONVOL;
	unwindcode->CodeOffset = (guchar)unwind_op->when;
	unwindcode->OpInfo = unwind_op->reg;

	if (unwindinfo->SizeOfProlog >= unwindcode->CodeOffset)
		g_error ("Adding unwind info in wrong order.");

	unwindinfo->SizeOfProlog = unwindcode->CodeOffset;
}

void
mono_arch_unwindinfo_add_set_fpreg (PUNWIND_INFO unwindinfo, MonoUnwindOp *unwind_op)
{
	PUNWIND_CODE unwindcode;
	guchar codeindex;

	g_assert (unwindinfo != NULL);

	if (unwindinfo->CountOfCodes + 1 >= MONO_MAX_UNWIND_CODES)
		g_error ("Larger allocation needed for the unwind information.");

	codeindex = MONO_MAX_UNWIND_CODES - (++unwindinfo->CountOfCodes);
	unwindcode = &unwindinfo->UnwindCode [codeindex];
	unwindcode->UnwindOp = UWOP_SET_FPREG;
	unwindcode->CodeOffset = (guchar)unwind_op->when;

	g_assert (unwind_op->val % 16 == 0);
	unwindinfo->FrameRegister = unwind_op->reg;
	unwindinfo->FrameOffset = unwind_op->val / 16;

	if (unwindinfo->SizeOfProlog >= unwindcode->CodeOffset)
		g_error ("Adding unwind info in wrong order.");

	unwindinfo->SizeOfProlog = unwindcode->CodeOffset;
}

void
mono_arch_unwindinfo_add_alloc_stack (PUNWIND_INFO unwindinfo, MonoUnwindOp *unwind_op)
{
	PUNWIND_CODE unwindcode;
	guchar codeindex;
	guchar codesneeded;
	guint size;

	g_assert (unwindinfo != NULL);

	size = unwind_op->val;

	if (size < 0x8)
		g_error ("Stack allocation must be equal to or greater than 0x8.");

	if (size <= 0x80)
		codesneeded = 1;
	else if (size <= 0x7FFF8)
		codesneeded = 2;
	else
		codesneeded = 3;

	if (unwindinfo->CountOfCodes + codesneeded > MONO_MAX_UNWIND_CODES)
		g_error ("Larger allocation needed for the unwind information.");

	codeindex = MONO_MAX_UNWIND_CODES - (unwindinfo->CountOfCodes += codesneeded);
	unwindcode = &unwindinfo->UnwindCode [codeindex];

	unwindcode->CodeOffset = (guchar)unwind_op->when;

	if (codesneeded == 1) {
		/*The size of the allocation is
		  (the number in the OpInfo member) times 8 plus 8*/
		unwindcode->UnwindOp = UWOP_ALLOC_SMALL;
		unwindcode->OpInfo = (size - 8)/8;
	}
	else {
		if (codesneeded == 3) {
			/*the unscaled size of the allocation is recorded
			  in the next two slots in little-endian format.
			  NOTE, unwind codes are allocated from end to beginning of list so
			  unwind code will have right execution order. List is sorted on CodeOffset
			  using descending sort order.*/
			unwindcode->UnwindOp = UWOP_ALLOC_LARGE;
			unwindcode->OpInfo = 1;
			*((unsigned int*)(&(unwindcode + 1)->FrameOffset)) = size;
		}
		else {
			/*the size of the allocation divided by 8
			  is recorded in the next slot.
			  NOTE, unwind codes are allocated from end to beginning of list so
			  unwind code will have right execution order. List is sorted on CodeOffset
			  using descending sort order.*/
			unwindcode->UnwindOp = UWOP_ALLOC_LARGE;
			unwindcode->OpInfo = 0;
			(unwindcode + 1)->FrameOffset = (gushort)(size/8);
		}
	}

	if (unwindinfo->SizeOfProlog >= unwindcode->CodeOffset)
		g_error ("Adding unwind info in wrong order.");

	unwindinfo->SizeOfProlog = unwindcode->CodeOffset;
}

static void
initialize_unwind_info_internal_ex (GSList *unwind_ops, PUNWIND_INFO unwindinfo)
{
	if (unwind_ops != NULL && unwindinfo != NULL) {
		MonoUnwindOp *unwind_op_data;
		gboolean sp_alloced = FALSE;
		gboolean fp_alloced = FALSE;

		// Replay collected unwind info and setup Windows format.
		for (GSList *l = unwind_ops; l; l = l->next) {
			unwind_op_data = (MonoUnwindOp *)l->data;
			switch (unwind_op_data->op) {
				case DW_CFA_offset : {
					// Pushes should go before SP/FP allocation to be compliant with Windows x64 ABI.
					// TODO: DW_CFA_offset can also be used to move saved regs into frame.
					if (unwind_op_data->reg != AMD64_RIP && sp_alloced == FALSE && fp_alloced == FALSE)
						mono_arch_unwindinfo_add_push_nonvol (unwindinfo, unwind_op_data);
					break;
				}
				case DW_CFA_mono_sp_alloc_info_win64 : {
					mono_arch_unwindinfo_add_alloc_stack (unwindinfo, unwind_op_data);
					sp_alloced = TRUE;
					break;
				}
				case DW_CFA_mono_fp_alloc_info_win64 : {
					mono_arch_unwindinfo_add_set_fpreg (unwindinfo, unwind_op_data);
					fp_alloced = TRUE;
					break;
				}
				default :
					break;
			}
		}
	}
}

static PUNWIND_INFO
initialize_unwind_info_internal (GSList *unwind_ops)
{
	PUNWIND_INFO unwindinfo;

	mono_arch_unwindinfo_create ((gpointer*)&unwindinfo);
	initialize_unwind_info_internal_ex (unwind_ops, unwindinfo);

	return unwindinfo;
}

guchar
mono_arch_unwindinfo_get_code_count (GSList *unwind_ops)
{
	UNWIND_INFO unwindinfo = {0};
	initialize_unwind_info_internal_ex (unwind_ops, &unwindinfo);
	return unwindinfo.CountOfCodes;
}

PUNWIND_INFO
mono_arch_unwindinfo_alloc_unwind_info (GSList *unwind_ops)
{
	if (!unwind_ops)
		return NULL;

	return initialize_unwind_info_internal (unwind_ops);
}

void
mono_arch_unwindinfo_free_unwind_info (PUNWIND_INFO unwind_info)
{
	g_free (unwind_info);
}

guint
mono_arch_unwindinfo_init_method_unwind_info (gpointer cfg)
{
	MonoCompile * current_cfg = (MonoCompile *)cfg;
	g_assert (current_cfg->arch.unwindinfo == NULL);
	current_cfg->arch.unwindinfo = initialize_unwind_info_internal (current_cfg->unwind_ops);
	return mono_arch_unwindinfo_get_size (((PUNWIND_INFO)(current_cfg->arch.unwindinfo))->CountOfCodes);
}

void
mono_arch_unwindinfo_install_method_unwind_info (PUNWIND_INFO *monoui, gpointer code, guint code_size)
{
	PUNWIND_INFO unwindinfo, targetinfo;
	guchar codecount;
	guint64 targetlocation;
	if (!*monoui)
		return;

	unwindinfo = *monoui;
	targetlocation = (guint64)&(((guchar*)code)[code_size]);
	targetinfo = (PUNWIND_INFO) ALIGN_TO(targetlocation, sizeof (host_mgreg_t));

	memcpy (targetinfo, unwindinfo, sizeof (UNWIND_INFO) - (sizeof (UNWIND_CODE) * MONO_MAX_UNWIND_CODES));

	codecount = unwindinfo->CountOfCodes;
	if (codecount) {
		memcpy (&targetinfo->UnwindCode [0], &unwindinfo->UnwindCode [MONO_MAX_UNWIND_CODES - codecount],
			sizeof (UNWIND_CODE) * codecount);
	}

#ifdef ENABLE_CHECKED_BUILD_UNWINDINFO
	if (codecount) {
		// Validate the order of unwind op codes in checked builds. Offset should be in descending order.
		// In first iteration previous == current, this is intended to handle UWOP_ALLOC_LARGE as first item.
		int previous = 0;
		for (int current = 0; current < codecount; current++) {
			g_assert_checked (targetinfo->UnwindCode [previous].CodeOffset >= targetinfo->UnwindCode [current].CodeOffset);
			previous = current;
			if (targetinfo->UnwindCode [current].UnwindOp == UWOP_ALLOC_LARGE) {
				if (targetinfo->UnwindCode [current].OpInfo == 0) {
					current++;
				} else {
					current += 2;
				}
			}
		}
	}
#endif /* ENABLE_CHECKED_BUILD_UNWINDINFO */

	mono_arch_unwindinfo_free_unwind_info (unwindinfo);
	*monoui = 0;

	// Register unwind info in table.
	mono_arch_unwindinfo_insert_rt_func_in_table (code, code_size);
}

void
mono_arch_unwindinfo_install_tramp_unwind_info (GSList *unwind_ops, gpointer code, guint code_size)
{
	PUNWIND_INFO unwindinfo = initialize_unwind_info_internal (unwind_ops);
	if (unwindinfo != NULL) {
		mono_arch_unwindinfo_install_method_unwind_info (&unwindinfo, code, code_size);
	}
}

RUNTIME_FUNCTION
mono_arch_unwindinfo_init(gpointer code, gsize code_offset, gsize code_size, gsize begin_range, gsize end_range) {

	RUNTIME_FUNCTION new_rt_func_data;
	new_rt_func_data.BeginAddress = code_offset;
	new_rt_func_data.EndAddress = code_offset + code_size;

	gsize aligned_unwind_data = ALIGN_TO(end_range, sizeof(host_mgreg_t));
	new_rt_func_data.UnwindData = aligned_unwind_data - begin_range;

	g_assert_checked(new_rt_func_data.UnwindData == ALIGN_TO(new_rt_func_data.EndAddress, sizeof(host_mgreg_t)));

	return new_rt_func_data;
}

#endif /* MONO_ARCH_HAVE_UNWIND_TABLE */

#if MONO_SUPPORT_TASKLETS && !defined(DISABLE_JIT) && !defined(ENABLE_NETCORE)
MonoContinuationRestore
mono_tasklets_arch_restore (void)
{
	static guint8* saved = NULL;
	guint8 *code, *start;
	int cont_reg = AMD64_R9; /* register usable on both call conventions */
	const int kMaxCodeSize = 64;

	if (saved)
		return (MonoContinuationRestore)saved;
	code = start = (guint8 *)mono_global_codeman_reserve (kMaxCodeSize);
	/* the signature is: restore (MonoContinuation *cont, int state, MonoLMF **lmf_addr) */
	/* cont is in AMD64_ARG_REG1 ($rcx or $rdi)
	 * state is in AMD64_ARG_REG2 ($rdx or $rsi)
	 * lmf_addr is in AMD64_ARG_REG3 ($r8 or $rdx)
	 * We move cont to cont_reg since we need both rcx and rdi for the copy
	 * state is moved to $rax so it's setup as the return value and we can overwrite $rsi
 	 */
	amd64_mov_reg_reg (code, cont_reg, MONO_AMD64_ARG_REG1, 8);
	amd64_mov_reg_reg (code, AMD64_RAX, MONO_AMD64_ARG_REG2, 8);
	/* setup the copy of the stack */
	amd64_mov_reg_membase (code, AMD64_RCX, cont_reg, MONO_STRUCT_OFFSET (MonoContinuation, stack_used_size), sizeof (int));
	amd64_shift_reg_imm (code, X86_SHR, AMD64_RCX, 3);
	x86_cld (code);
	amd64_mov_reg_membase (code, AMD64_RSI, cont_reg, MONO_STRUCT_OFFSET (MonoContinuation, saved_stack), sizeof (gpointer));
	amd64_mov_reg_membase (code, AMD64_RDI, cont_reg, MONO_STRUCT_OFFSET (MonoContinuation, return_sp), sizeof (gpointer));
	amd64_prefix (code, X86_REP_PREFIX);
	amd64_movsl (code);

	/* now restore the registers from the LMF */
	amd64_mov_reg_membase (code, AMD64_RCX, cont_reg, MONO_STRUCT_OFFSET (MonoContinuation, lmf), 8);
	amd64_mov_reg_membase (code, AMD64_RBP, AMD64_RCX, MONO_STRUCT_OFFSET (MonoLMF, rbp), 8);
	amd64_mov_reg_membase (code, AMD64_RSP, AMD64_RCX, MONO_STRUCT_OFFSET (MonoLMF, rsp), 8);

#ifdef WIN32
	amd64_mov_reg_reg (code, AMD64_R14, AMD64_ARG_REG3, 8);
#else
	amd64_mov_reg_reg (code, AMD64_R12, AMD64_ARG_REG3, 8);
#endif

	/* state is already in rax */
	amd64_jump_membase (code, cont_reg, MONO_STRUCT_OFFSET (MonoContinuation, return_ip));
	g_assertf ((code - start) <= kMaxCodeSize, "%d %d", (int)(code - start), kMaxCodeSize);

	mono_arch_flush_icache (start, code - start);
	MONO_PROFILER_RAISE (jit_code_buffer, (start, code - start, MONO_PROFILER_CODE_BUFFER_EXCEPTION_HANDLING, NULL));

	saved = start;
	return (MonoContinuationRestore)saved;
}
#endif /* MONO_SUPPORT_TASKLETS && !defined(DISABLE_JIT) && !defined(ENABLE_NETCORE) */

/*
 * mono_arch_setup_resume_sighandler_ctx:
 *
 *   Setup CTX so execution continues at FUNC.
 */
void
mono_arch_setup_resume_sighandler_ctx (MonoContext *ctx, gpointer func)
{
	/* 
	 * When resuming from a signal handler, the stack should be misaligned, just like right after
	 * a call.
	 */
	if ((((guint64)MONO_CONTEXT_GET_SP (ctx)) % 16) == 0)
		MONO_CONTEXT_SET_SP (ctx, (guint64)MONO_CONTEXT_GET_SP (ctx) - 8);
	MONO_CONTEXT_SET_IP (ctx, func);
}

#if (!MONO_SUPPORT_TASKLETS || defined(DISABLE_JIT)) && !defined(ENABLE_NETCORE)
MonoContinuationRestore
mono_tasklets_arch_restore (void)
{
	g_assert_not_reached ();
	return NULL;
}
#endif /* (!MONO_SUPPORT_TASKLETS || defined(DISABLE_JIT)) && !defined(ENABLE_NETCORE) */

void
mono_arch_undo_ip_adjustment (MonoContext *ctx)
{
	ctx->gregs [AMD64_RIP]++;
}

void
mono_arch_do_ip_adjustment (MonoContext *ctx)
{
	ctx->gregs [AMD64_RIP]--;
}
