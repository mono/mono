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
// This ordering is mimiced in MONO_JIT_ICALLS and will be in mono_create_tls_get.
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

G_EXTERN_C void mono_tls_init_gc_keys (void);
G_EXTERN_C void mono_tls_init_runtime_keys (void);
G_EXTERN_C gint32 mono_tls_get_tls_offset (MonoTlsKey key);

typedef gpointer (*MonoTlsGetter)(void);

G_EXTERN_C MonoTlsGetter mono_tls_get_tls_getter (MonoTlsKey key);

G_EXTERN_C MonoInternalThread *mono_tls_get_thread (void);
G_EXTERN_C MonoJitTlsData     *mono_tls_get_jit_tls (void);
G_EXTERN_C MonoDomain *mono_tls_get_domain (void);
G_EXTERN_C SgenThreadInfo     *mono_tls_get_sgen_thread_info (void);
G_EXTERN_C MonoLMF           **mono_tls_get_lmf_addr (void);

G_EXTERN_C void mono_tls_set_thread 	   (MonoInternalThread *value);
G_EXTERN_C void mono_tls_set_jit_tls 	   (MonoJitTlsData     *value);
G_EXTERN_C void mono_tls_set_domain 	   (MonoDomain         *value);
G_EXTERN_C void mono_tls_set_sgen_thread_info (SgenThreadInfo     *value);
G_EXTERN_C void mono_tls_set_lmf_addr 	   (MonoLMF           **value);

#if __cplusplus

/* tls attribute */
#if HAVE_TLS_MODEL_ATTR

#if defined(__PIC__) && !defined(PIC)
/*
 * Must be compiling -fPIE, for executables.  Build PIC
 * but with initial-exec.
 * http://bugs.gentoo.org/show_bug.cgi?id=165547
 */
#define PIC
#define PIC_INITIAL_EXEC
#endif

/*
 * Define this if you want a faster libmono, which cannot be loaded dynamically as a
 * module.
 */
//#define PIC_INITIAL_EXEC

#if defined(PIC)

#ifdef PIC_INITIAL_EXEC
#define MONO_TLS_FAST __attribute__ ((__tls_model__("initial-exec")))
#else
#if defined (__powerpc__)
/* local dynamic requires a call to __tls_get_addr to look up the
   TLS block address via the Dynamic Thread Vector. In this case Thread
   Pointer relative offsets can't be used as this modules TLS was
   allocated separately (none contiguoiusly) from the initial TLS
   block.

   For now we will disable this. */
#define MONO_TLS_FAST
#else
#define MONO_TLS_FAST __attribute__ ((__tls_model__("local-dynamic")))
#endif
#endif

#else

#define MONO_TLS_FAST __attribute__ ((__tls_model__("local-exec")))

#endif

#else
#define MONO_TLS_FAST /* nothing */
#endif

// Tls variables and macros to get/set them.
extern thread_local MonoInternalThread *mono_tls_thread MONO_TLS_FAST;
extern thread_local MonoJitTlsData     *mono_tls_jit_tls MONO_TLS_FAST;
extern thread_local MonoDomain         *mono_tls_domain MONO_TLS_FAST;
extern thread_local SgenThreadInfo     *mono_tls_sgen_thread_info MONO_TLS_FAST;
extern thread_local MonoLMF           **mono_tls_lmf_addr MONO_TLS_FAST;

#define mono_tls_get_thread()                   (mono_tls_thread)
#define mono_tls_get_jit_tls()                  (mono_tls_jit_tls)
#define mono_tls_get_domain()                   (mono_tls_domain)
#define mono_tls_get_sgen_thread_info()         (mono_tls_sgen_thread_info)
#define mono_tls_get_lmf_addr()                 (mono_tls_lmf_addr)

#define mono_tls_set_thread(value)              (mono_tls_thread = (value))
#define mono_tls_set_jit_tls(value)             (mono_tls_jit_tls = (value))
#define mono_tls_set_domain(value)              (mono_tls_domain = (value))
#define mono_tls_set_sgen_thread_info(value)    (mono_tls_sgen_thread_info = (value))
#define mono_tls_set_lmf_addr(value)            (mono_tls_lmf_addr = (value))

#endif

#endif /* __MONO_TLS_H__ */
