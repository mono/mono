/*
 * mach-support-arm.c: mach support for ARM
 *
 * Authors:
 *   Geoff Norton (gnorton@novell.com)
 *   Rodrigo Kumpera (kumpera@gmail.com)
 *
 * (C) 2010 Novell, Inc.
 * (C) 2011 Xamarin, Inc.
 */

#include <config.h>

#if defined(__MACH__)
#include <stdint.h>
#include <glib.h>
#include <pthread.h>
#include "utils/mono-sigcontext.h"
#include "utils/mono-compiler.h"
#include "mach-support.h"

/* _mcontext.h now defines __darwin_mcontext32, not __darwin_mcontext, starting with Xcode 5.1 */
#ifdef _STRUCT_MCONTEXT32
       #define __darwin_mcontext       __darwin_mcontext32
#endif

/* Known offsets used for TLS storage*/


static const int known_tls_offsets[] = {
	0x48, /*Found on iOS 6 */
	0xA4,
	0xA8,
};

#define TLS_PROBE_COUNT (sizeof (known_tls_offsets) / sizeof (int))

/* This is 2 slots less than the known low */
#define TLS_PROBE_LOW_WATERMARK 0x40
/* This is 24 slots above the know high, which is the same diff as the knowns high-low*/
#define TLS_PROBE_HIGH_WATERMARK 0x108

static int tls_vector_offset;

void *
mono_mach_arch_get_ip (thread_state_t state)
{
	/* Can't use unified_thread_state on !ARM64 since this has to compile on armv6 too */
	arm_unified_thread_state_t *arch_state = (arm_unified_thread_state_t *) state;

	return (void *) arch_state->ts_64.__pc;
}

void *
mono_mach_arch_get_sp (thread_state_t state)
{
	arm_unified_thread_state_t *arch_state = (arm_unified_thread_state_t *) state;

	return (void *) arch_state->ts_64.__sp;
}

int
mono_mach_arch_get_mcontext_size ()
{
	return sizeof (struct __darwin_mcontext64);
}

void
mono_mach_arch_thread_state_to_mcontext (thread_state_t state, void *context)
{
	arm_unified_thread_state_t *arch_state = (arm_unified_thread_state_t *) state;
	struct __darwin_mcontext64 *ctx = (struct __darwin_mcontext64 *) context;

	ctx->__ss = arch_state->ts_64;
}

void
mono_mach_arch_mcontext_to_thread_state (void *context, thread_state_t state)
{
	arm_unified_thread_state_t *arch_state = (arm_unified_thread_state_t *) state;
	struct __darwin_mcontext64 *ctx = (struct __darwin_mcontext64 *) context;

	arch_state->ts_64 = ctx->__ss;
}

int
mono_mach_arch_get_thread_state_size ()
{
	return sizeof (arm_unified_thread_state_t);
}

kern_return_t
mono_mach_arch_get_thread_state (thread_port_t thread, thread_state_t state, mach_msg_type_number_t *count)
{
	arm_unified_thread_state_t *arch_state = (arm_unified_thread_state_t *) state;
	kern_return_t ret;

	*count = ARM_UNIFIED_THREAD_STATE_COUNT;

	ret = thread_get_state (thread, ARM_UNIFIED_THREAD_STATE, (thread_state_t) arch_state, count);
	return ret;
}

kern_return_t
mono_mach_arch_set_thread_state (thread_port_t thread, thread_state_t state, mach_msg_type_number_t count)
{
	return thread_set_state (thread, ARM_UNIFIED_THREAD_STATE, state, count);
}

void *
mono_mach_get_tls_address_from_thread (pthread_t thread, pthread_key_t key)
{
	/* Mach stores TLS values in a hidden array inside the pthread_t structure
	 * They are keyed off a giant array from a known offset into the pointer. This value
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
	for (i = 0; i < TLS_PROBE_COUNT; ++i) {
		tls_vector_offset = known_tls_offsets [i];
		if (mono_mach_arch_get_tls_value_from_thread (pthread_self (), key) == canary)
			goto ok;
	}

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
