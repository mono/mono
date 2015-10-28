/*
 * mach-support-x86.c: mach support for x86
 *
 * Authors:
 *   Geoff Norton (gnorton@novell.com)
 *
 * (C) 2010 Novell, Inc.
 */

#include <config.h>

#if defined(__MACH__)
#include <stdint.h>
#include <glib.h>
#include <pthread.h>
#include "utils/mono-sigcontext.h"
#include "mach-support.h"

/* Known offsets used for TLS storage*/

/* All OSX versions up to 10.8 */
#define TLS_VECTOR_OFFSET_CATS 0x48
#define TLS_VECTOR_OFFSET_10_9 0xb0
#define TLS_VECTOR_OFFSET_10_11 0x100


/* This is 2 slots less than the known low */
#define TLS_PROBE_LOW_WATERMARK 0x40
/* This is 28 slots above the know high, which is more than the known high-low*/
#define TLS_PROBE_HIGH_WATERMARK 0x120


static int tls_vector_offset;

void *
mono_mach_arch_get_ip (thread_state_t state)
{
	x86_thread_state32_t *arch_state = (x86_thread_state32_t *) state;

	return (void *) arch_state->__eip;
}

void *
mono_mach_arch_get_sp (thread_state_t state)
{
	x86_thread_state32_t *arch_state = (x86_thread_state32_t *) state;

	return (void *) arch_state->__esp;
}

int
mono_mach_arch_get_mcontext_size ()
{
	return sizeof (struct __darwin_mcontext32);
}

void
mono_mach_arch_thread_state_to_mcontext (thread_state_t state, void *context)
{
	x86_thread_state32_t *arch_state = (x86_thread_state32_t *) state;
	struct __darwin_mcontext32 *ctx = (struct __darwin_mcontext32 *) context;

	ctx->__ss = *arch_state;
}

void
mono_mach_arch_mcontext_to_thread_state (void *context, thread_state_t state)
{
	x86_thread_state32_t *arch_state = (x86_thread_state32_t *) state;
	struct __darwin_mcontext32 *ctx = (struct __darwin_mcontext32 *) context;

	*arch_state = ctx->__ss;
}

void
mono_mach_arch_thread_state_to_mono_context (thread_state_t state, MonoContext *context)
{
	x86_thread_state32_t *arch_state = (x86_thread_state32_t *) state;
	context->eax = arch_state->__eax;
	context->ebx = arch_state->__ebx;
	context->ecx = arch_state->__ecx;
	context->edx = arch_state->__edx;
	context->ebp = arch_state->__ebp;
	context->esp = arch_state->__esp;
	context->esi = arch_state->__edi;
	context->edi = arch_state->__esi;
	context->eip = arch_state->__eip;
}


int
mono_mach_arch_get_thread_state_size ()
{
	return sizeof (x86_thread_state32_t);
}

kern_return_t
mono_mach_arch_get_thread_state (thread_port_t thread, thread_state_t state, mach_msg_type_number_t *count)
{
#if defined(HOST_WATCHOS)
	g_error ("thread_get_state() is not supported by this platform");
#else
	x86_thread_state32_t *arch_state = (x86_thread_state32_t *) state;
	kern_return_t ret;

	*count = x86_THREAD_STATE32_COUNT;
	ret = thread_get_state (thread, x86_THREAD_STATE32, (thread_state_t) arch_state, count);

	return ret;
#endif
}

kern_return_t
mono_mach_arch_set_thread_state (thread_port_t thread, thread_state_t state, mach_msg_type_number_t count)
{
#if defined(HOST_WATCHOS)
	g_error ("thread_set_state() is not supported by this platform");
#else
	return thread_set_state (thread, x86_THREAD_STATE32, state, count);
#endif	
}

void *
mono_mach_get_tls_address_from_thread (pthread_t thread, pthread_key_t key)
{
	/* OSX stores TLS values in a hidden array inside the pthread_t structure
	 * They are keyed off a giant array from a known offset into the pointer.  This value
	 * is baked into their pthread_getspecific implementation
	 */
	intptr_t *p = (intptr_t *) thread;
	intptr_t **tsd = (intptr_t **) ((char*)p + tls_vector_offset);
	g_assert (tls_vector_offset != -1);

	return (void *) &tsd [key];	
}

void *
mono_mach_arch_get_tls_value_from_thread (pthread_t thread, guint32 key)
{
	return *(void**)mono_mach_get_tls_address_from_thread (thread, key);
}

void
mono_mach_init (pthread_key_t key)
{
	int i;
	void *old_value = pthread_getspecific (key);
	void *canary = (void*)0xDEADBEEFu;

	pthread_key_create (&key, NULL);
	g_assert (old_value != canary);

	pthread_setspecific (key, canary);

	/*First we probe for cats*/
	tls_vector_offset = TLS_VECTOR_OFFSET_CATS;
	if (mono_mach_arch_get_tls_value_from_thread (pthread_self (), key) == canary)
		goto ok;

	tls_vector_offset = TLS_VECTOR_OFFSET_10_9;
	if (mono_mach_arch_get_tls_value_from_thread (pthread_self (), key) == canary)
		goto ok;

	tls_vector_offset = TLS_VECTOR_OFFSET_10_11;
	if (mono_mach_arch_get_tls_value_from_thread (pthread_self (), key) == canary)
		goto ok;

	/*Fallback to scanning a large range of offsets*/
	for (i = TLS_PROBE_LOW_WATERMARK; i <= TLS_PROBE_HIGH_WATERMARK; i += 4) {
		tls_vector_offset = i;
		if (mono_mach_arch_get_tls_value_from_thread (pthread_self (), key) == canary) {
			g_warning ("Found new TLS offset at %d", i);
			goto ok;
		}
	}

	tls_vector_offset = -1;
	g_warning ("could not discover the mach TLS offset");
ok:
	pthread_setspecific (key, old_value);
}

#endif
