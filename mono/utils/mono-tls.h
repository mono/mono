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

/* TLS entries used by the runtime */
typedef enum {
	/* mono_thread_internal_current () */
	TLS_KEY_THREAD = 0,
	TLS_KEY_JIT_TLS = 1,
	/* mono_domain_get () */
	TLS_KEY_DOMAIN = 2,
	TLS_KEY_SGEN_THREAD_INFO = 3,
	TLS_KEY_LMF_ADDR = 4,
	TLS_KEY_NUM = 5
} MonoTlsKey;

#ifdef HOST_WIN32

#include <windows.h>

/*
* These APIs were added back in Windows SDK 14393. Let's redirect them to
* Fls* APIs on older SDKs just like Windows 8.1 headers do
*/
#if G_HAVE_API_SUPPORT(HAVE_UWP_WINAPI_SUPPORT)
#if WINDOWS_SDK_BUILD_VERSION < 14393
#define TlsAlloc() FlsAlloc(NULL)
#define TlsGetValue FlsGetValue
#define TlsSetValue FlsSetValue
#define TlsFree FlsFree
#endif
#endif

#define MonoNativeTlsKey DWORD
#define mono_native_tls_alloc(key,destructor) ((*(key) = TlsAlloc ()) != TLS_OUT_OF_INDEXES && destructor == NULL)
#define mono_native_tls_free TlsFree
#define mono_native_tls_set_value TlsSetValue
#define mono_native_tls_get_value(x) (TlsGetValue (x))

#else

#include <pthread.h>

#define MonoNativeTlsKey pthread_key_t
#define mono_native_tls_get_value(x) (pthread_getspecific (x))

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
G_BEGIN_DECLS // FIXMEcxx for monodis
void mono_tls_init_runtime_keys (void);
G_END_DECLS
void mono_tls_free_keys (void);
gint32 mono_tls_get_tls_offset (MonoTlsKey key);
gpointer mono_tls_get_tls_getter (MonoTlsKey key, gboolean name);
gpointer mono_tls_get_tls_setter (MonoTlsKey key, gboolean name);

struct _MonoInternalThread *mono_tls_get_thread (void);
struct  MonoJitTlsData     *mono_tls_get_jit_tls (void);
struct _MonoDomain         *mono_tls_get_domain (void);
struct _SgenThreadInfo     *mono_tls_get_sgen_thread_info (void);
struct  MonoLMF           **mono_tls_get_lmf_addr (void);

void mono_tls_set_thread (gpointer value);
void mono_tls_set_jit_tls (gpointer value);
void mono_tls_set_domain (gpointer value);
void mono_tls_set_sgen_thread_info (gpointer value);
void mono_tls_set_lmf_addr (gpointer value);

#endif /* __MONO_TLS_H__ */
