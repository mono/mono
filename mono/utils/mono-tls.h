/**
 * \file
 * Low-level TLS support
 *
 * Author:
 *	Rodrigo Kumpera (kumpera@gmail.com)
 *
 * Copyright 2011 Novell, Inc (http://www.novell.com)
 * Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#ifndef __MONO_TLS_H__
#define __MONO_TLS_H__

#include <config.h>
#include <glib.h>
#include <mono/utils/mono-forward-internal.h>

/* TLS entries used by the runtime */
// This ordering must match MONO_JIT_ICALLS and MONO_TLS_LIST (alphabetical).
typedef enum {
	TLS_KEY_DOMAIN		   = 0, // mono_domain_get ()
	TLS_KEY_JIT_TLS		   = 1,
	TLS_KEY_LMF_ADDR	   = 2,
	TLS_KEY_MARSHAL_LAST_ERROR = 3,
	TLS_KEY_SGEN_THREAD_INFO   = 4,
	TLS_KEY_THREAD		   = 5, // mono_thread_internal_current ()
	TLS_KEY_THREAD_SMALL_ID	   = 6,
	TLS_KEY_NUM		   = 7
} MonoTlsKey;

#if __cplusplus
g_static_assert (TLS_KEY_DOMAIN == 0);
#endif
// There are only JIT icalls to get TLS, not set TLS.
#define mono_get_tls_key_to_jit_icall_id(a)	((MonoJitICallId)((a) + MONO_JIT_ICALL_mono_tls_get_domain))

#ifdef HOST_WIN32

#include <windows.h>

// Some Windows SDKs define TLS to be FLS.
// FLS is a reasonable idea, but be consistent.
// And there is no __declspec (fiber).
#undef TlsAlloc
#undef TlsFree
#undef TlsGetValue
#undef TlsSetValue

#define MonoNativeTlsKey DWORD
#define mono_native_tls_alloc(key,destructor) ((*(key) = TlsAlloc ()) != TLS_OUT_OF_INDEXES && destructor == NULL)
#define mono_native_tls_free TlsFree
#define mono_native_tls_set_value TlsSetValue
#define mono_native_tls_get_value TlsGetValue

#else

#include <pthread.h>

#define MonoNativeTlsKey pthread_key_t
#define mono_native_tls_get_value pthread_getspecific

static inline int
mono_native_tls_alloc (MonoNativeTlsKey *key, void *destructor)
{
	return pthread_key_create (key, (void (*)(void*)) destructor) == 0;
}

static inline void
mono_native_tls_free (MonoNativeTlsKey key)
{
	pthread_key_delete (key);
}

static inline int
mono_native_tls_set_value (MonoNativeTlsKey key, gpointer value)
{
	return !pthread_setspecific (key, value);
}

#endif /* HOST_WIN32 */

void mono_tls_init_gc_keys (void);
void mono_tls_init_runtime_keys (void);
void mono_tls_free_keys (void);
gint32 mono_tls_get_tls_offset (MonoTlsKey key);

// For search despite token pasting:
//  mono_tls_get_thread
//  mono_tls_set_thread
//  mono_tls_thread
//
//  mono_tls_get_jit_tls
//  mono_tls_set_jit_tls
//  mono_tls_jit_tls
//
//  mono_tls_get_domain
//  mono_tls_set_domain
//  mono_tls_domain
//
//  mono_tls_get_sgen_thread_info
//  mono_tls_set_sgen_thread_info
//  mono_tls_sgen_thread_info
//
//  mono_tls_get_lmf_addr
//  mono_tls_set_lmf_addr
//  mono_tls_lmf_addr
//
//  thread_small_id
//  marshal_last_error
//
// This order must match MonoTlsKey (alphabetical)
////					| here
#define MONO_TLS_LIST					       /*init clean*/	\
	MONO_TLS (MonoDomain*,		domain,			TRUE, TRUE)	\
	MONO_TLS (MonoJitTlsData*,	jit_tls,		TRUE, TRUE)	\
	MONO_TLS (MonoLMF**,		lmf_addr,		TRUE, TRUE)	\
	MONO_TLS (int,			marshal_last_error,	FALSE, FALSE)	\
	MONO_TLS (SgenThreadInfo*,	sgen_thread_info,	FALSE, TRUE)	\
	MONO_TLS (MonoInternalThread*,	thread,			TRUE, TRUE)	\
	MONO_TLS (int,			thread_small_id,	FALSE, FALSE)	\

// Declare functions and keys.
// Some of the getters are icalls so are extern "C".

#undef MONO_TLS
#define MONO_TLS(type, name, init, cleanup)		\
	G_EXTERN_C type mono_tls_get_ ## name (void);	\
	void mono_tls_set_ ## name (type value);	\

	MONO_TLS_LIST

#ifndef MONO_KEYWORD_THREAD

#undef MONO_TLS
#define MONO_TLS(type, name, init, cleanup)		\
	extern MonoNativeTlsKey mono_tls_key_ ## name;	\

	MONO_TLS_LIST

#endif

#endif /* __MONO_TLS_H__ */
