/**
 * \file
 * mach support for PPC
 *
 * Authors:
 *   Sergey Fedorov (vital.had@gmail.com)
 *
 * (C) 2023 Xamarin, Inc.
 */

/* The code below assumes 10.5.x or 10.6.x (10A190 or Rosetta).
 * Tiger will likely need some changes, if at all can be supported.
 */

#include <config.h>

#if defined(__MACH__)
#include <stdint.h>
#include <glib.h>
#include <pthread.h>
#include "utils/mono-sigcontext.h"
#include "utils/mono-compiler.h"
#include "mach-support.h"

// For reg numbers
#include <mono/arch/ppc/ppc-codegen.h>

int
mono_mach_arch_get_mcontext_size ()
{
	return sizeof (struct __darwin_mcontext);
}

void
mono_mach_arch_thread_states_to_mcontext (thread_state_t state, thread_state_t fpstate, void *context)
{
	ppc_thread_state_t *arch_state = (ppc_thread_state_t *) state;
	ppc_float_state_t *arch_fpstate = (ppc_float_state_t *) fpstate;
	struct __darwin_mcontext *ctx = (struct __darwin_mcontext *) context;

	ctx->__ss = *arch_state;
	ctx->__fs = *arch_fpstate;
}

void
mono_mach_arch_mcontext_to_thread_states (void *context, thread_state_t state, thread_state_t fpstate)
{
	ppc_thread_state_t *arch_state = (ppc_thread_state_t *) state;
	ppc_float_state_t *arch_fpstate = (ppc_float_state_t *) fpstate;
	struct __darwin_mcontext *ctx = (struct __darwin_mcontext *) context;

	*arch_state = ctx->__ss;
	*arch_fpstate = ctx->__fs;
}

void
mono_mach_arch_thread_states_to_mono_context (thread_state_t state, thread_state_t fpstate, MonoContext *context)
{
	ppc_thread_state_t *arch_state = (ppc_thread_state_t *) state;
	ppc_float_state_t *arch_fpstate = (ppc_float_state_t *) fpstate;
	context->sc_ir = arch_state->__srr0;
	context->sc_sp = arch_state->__r1;
	context->regs[ppc_r0] = arch_state->__r0;
	context->regs[ppc_r1] = arch_state->__r1;
	context->regs[ppc_r2] = arch_state->__r2;
	context->regs[ppc_r3] = arch_state->__r3;
	context->regs[ppc_r4] = arch_state->__r4;
	context->regs[ppc_r5] = arch_state->__r5;
	context->regs[ppc_r6] = arch_state->__r6;
	context->regs[ppc_r7] = arch_state->__r7;
	context->regs[ppc_r8] = arch_state->__r8;
	context->regs[ppc_r9] = arch_state->__r9;
	context->regs[ppc_r10] = arch_state->__r10;
	context->regs[ppc_r11] = arch_state->__r11;
	context->regs[ppc_r12] = arch_state->__r12;
	context->regs[ppc_r13] = arch_state->__r13;
	context->regs[ppc_r14] = arch_state->__r14;
	context->regs[ppc_r15] = arch_state->__r15;
	context->regs[ppc_r16] = arch_state->__r16;
	context->regs[ppc_r17] = arch_state->__r17;
	context->regs[ppc_r18] = arch_state->__r18;
	context->regs[ppc_r19] = arch_state->__r19;
	context->regs[ppc_r20] = arch_state->__r20;
	context->regs[ppc_r21] = arch_state->__r21;
	context->regs[ppc_r22] = arch_state->__r22;
	context->regs[ppc_r23] = arch_state->__r23;
	context->regs[ppc_r24] = arch_state->__r24;
	context->regs[ppc_r25] = arch_state->__r25;
	context->regs[ppc_r26] = arch_state->__r26;
	context->regs[ppc_r27] = arch_state->__r27;
	context->regs[ppc_r28] = arch_state->__r28;
	context->regs[ppc_r29] = arch_state->__r29;
	context->regs[ppc_r30] = arch_state->__r30;
	context->regs[ppc_r31] = arch_state->__r31;
	for (int i = 0; i < 32; ++i)
		context->fregs [i] = arch_fpstate->__fpregs [i];
}

int
mono_mach_arch_get_thread_state_size ()
{
	return sizeof (ppc_thread_state_t);
}

int
mono_mach_arch_get_thread_fpstate_size ()
{
	return sizeof (ppc_float_state_t);
}

kern_return_t
mono_mach_arch_get_thread_states (thread_port_t thread, thread_state_t state, mach_msg_type_number_t *count, thread_state_t fpstate, mach_msg_type_number_t *fpcount)
{
	ppc_thread_state_t *arch_state = (ppc_thread_state_t *) state;
	ppc_float_state_t *arch_fpstate = (ppc_float_state_t *) fpstate;
	kern_return_t ret;

	*count = PPC_THREAD_STATE_COUNT;
	*fpcount = PPC_FLOAT_STATE_COUNT;

	ret = thread_get_state (thread, PPC_THREAD_STATE, (thread_state_t) arch_state, count);
	if (ret != KERN_SUCCESS)
		return ret;
	ret = thread_get_state (thread, PPC_FLOAT_STATE, (thread_state_t) arch_fpstate, fpcount);
	return ret;
}

kern_return_t
mono_mach_arch_set_thread_states (thread_port_t thread, thread_state_t state, mach_msg_type_number_t count, thread_state_t fpstate, mach_msg_type_number_t fpcount)
{
	kern_return_t ret;
	return thread_set_state (thread, PPC_THREAD_STATE, state, count);
		if (ret != KERN_SUCCESS)
		return ret;
	ret = thread_set_state (thread, PPC_FLOAT_STATE, fpstate, fpcount);
	return ret;
}

#endif
