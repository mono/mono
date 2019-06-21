/**
 * \file
 * Low-level TLS support
 *
 * Thread local variables that are accessed both from native and managed code
 * are defined here and should be accessed only through this APIs
 *
 * Copyright 2013 Xamarin, Inc (http://www.xamarin.com)
 */

#if !__cplusplus
#error This file must be compiled as C++, with the -xc++ or -TP switches, but also should remain with .c extension.
#endif

#include <mono/utils/mach-support.h>

#include "mono-tls.h"

// On all platforms, use C++11 thread_local.
// Wrap this in an extern "C" API for C clients, or macros for C++ clients (or inline functions).
// On some platforms, C++11 thread_local might not be fast, gcc might just
// emulate and ues TlsGetValue or pthread_getspecific, but this is a portable (configuration-free)
// syntax for systems where it is fast, and the situation should only get better.
//
// FIXME Fast TLS for Windows.
/*
 * Certain platforms will support fast tls only when using one of the thread local
 * storage backends. By default this is __thread if we have MONO_KEYWORD_THREAD defined.
 *
 * By default all platforms will call into these native getters whenever they need
 * to get a tls value. On certain platforms we can try to be faster than this and
 * avoid the call. We call this fast tls and each platform defines its own way to
 * achieve this. For this, a platform has to define MONO_ARCH_HAVE_INLINED_TLS,
 * and provide alternative getters/setters for a MonoTlsKey. In order to have fast
 * getter/setters, the platform has to declare a way to fetch an internal offset
 * (MONO_THREAD_VAR_OFFSET) which is stored here, and in the arch specific file
 * probe the system to see if we can use the offset initialized here. If these
 * run-time checks don't succeed we just use the fallbacks.
 *
 * In case we would wish to provide fast inlined tls for aot code, we would need
 * to be sure that, at run-time, these two platform checks would never fail
 * otherwise the tls getter/setters that we emitted would not work. Normally,
 * there is little incentive to support this since tls access is most common in
 * wrappers and managed allocators, both of which are not aot-ed by default.
 * So far, we never supported inlined fast tls on full-aot systems.
 */

/* Runtime offset detection */
#if defined(TARGET_AMD64) && !defined(TARGET_MACH) && !defined(HOST_WIN32) /* __thread likely not tested on mac/win */

#if defined(PIC)
// This only works if libmono is linked into the application
#define MONO_THREAD_VAR_OFFSET(var,offset) do { guint64 foo;  __asm ("movq " #var "@GOTTPOFF(%%rip), %0" : "=r" (foo)); offset = foo; } while (0)
#else
#define MONO_THREAD_VAR_OFFSET(var,offset) do { guint64 foo;  __asm ("movq $" #var "@TPOFF, %0" : "=r" (foo)); offset = foo; } while (0)
#endif

#elif defined(TARGET_X86) && !defined(TARGET_MACH) && !defined(HOST_WIN32) && defined(__GNUC__)

#if defined(PIC)
#define MONO_THREAD_VAR_OFFSET(var,offset) do { int tmp; __asm ("call 1f; 1: popl %0; addl $_GLOBAL_OFFSET_TABLE_+[.-1b], %0; movl " #var "@gotntpoff(%0), %1" : "=r" (tmp), "=r" (offset)); } while (0)
#else
#define MONO_THREAD_VAR_OFFSET(var,offset) __asm ("movl $" #var "@ntpoff, %0" : "=r" (offset))
#endif

#elif defined(TARGET_ARM64) && !defined(PIC)

#define MONO_THREAD_VAR_OFFSET(var,offset) \
	__asm ( "mov %0, #0\n add %0, %0, #:tprel_hi12:" #var "\n add %0, %0, #:tprel_lo12_nc:" #var "\n" \
		: "=r" (offset))

#elif defined(TARGET_ARM) && defined(__ARM_EABI__) && !defined(PIC)

#define MONO_THREAD_VAR_OFFSET(var,offset) __asm ("     ldr     %0, 1f; b 2f; 1: .word " #var "(tpoff); 2:" : "=r" (offset))

#elif defined(TARGET_S390X)
# if defined(__PIC__)
#  if !defined(__PIE__)
// This only works if libmono is linked into the application
#   define MONO_THREAD_VAR_OFFSET(var,offset) do { guint64 foo;  				\
						void *x = &var;					\
						__asm__ ("ear   %%r1,%%a0\n"			\
							 "sllg  %%r1,%%r1,32\n"			\
							 "ear   %%r1,%%a1\n"			\
							 "lgr   %0,%1\n"			\
							 "sgr   %0,%%r1\n"			\
							: "=r" (foo) : "r" (x)			\
							: "1", "cc");				\
						offset = foo; } while (0)
#  elif __PIE__ == 1
#   define MONO_THREAD_VAR_OFFSET(var,offset) do { guint64 foo;  					\
						__asm__ ("lg	%0," #var "@GOTNTPOFF(%%r12)\n\t"	\
							 : "=r" (foo));					\
						offset = foo; } while (0)
#  elif __PIE__ == 2
#   define MONO_THREAD_VAR_OFFSET(var,offset) do { guint64 foo;  				\
						__asm__ ("larl	%%r1," #var "@INDNTPOFF\n\t"	\
							 "lg	%0,0(%%r1)\n\t"			\
							 : "=r" (foo) :				\
							 : "1", "cc");				\
						offset = foo; } while (0)
#  endif
# else
#  define MONO_THREAD_VAR_OFFSET(var,offset) do { guint64 foo;  			\
						__asm__ ("basr  %%r1,0\n\t"		\
							 "j     0f\n\t"			\
							 ".quad " #var "@NTPOFF\n"	\
							 "0:\n\t"			\
							 "lg    %0,4(%%r1)\n\t"		\
							: "=r" (foo) : : "1");		\
						offset = foo; } while (0)
# endif

#elif defined (TARGET_RISCV) && !defined (PIC)

#define MONO_THREAD_VAR_OFFSET(var, offset) \
	do { \
		guint32 temp; \
		__asm__ ( \
			"lui %0, %%tprel_hi(" #var ")\n" \
			"add %0, %0, tp, %%tprel_add(" #var ")\n" \
			"addi %0, %0, %%tprel_lo(" #var ")\n" \
			: "=r" (temp) \
		); \
		offset = temp; \
	} while (0)

#else

#define MONO_THREAD_VAR_OFFSET(var,offset) (offset) = -1

#endif

/* Tls variables for each MonoTlsKey */
thread_local MonoInternalThread *mono_tls_thread MONO_TLS_FAST;
thread_local MonoJitTlsData     *mono_tls_jit_tls MONO_TLS_FAST;
thread_local MonoDomain         *mono_tls_domain MONO_TLS_FAST;
thread_local SgenThreadInfo     *mono_tls_sgen_thread_info MONO_TLS_FAST;
thread_local MonoLMF           **mono_tls_lmf_addr MONO_TLS_FAST;

static gint32 tls_offsets [TLS_KEY_NUM];

#define MONO_TLS_GET_VALUE(tls_var,tls_key) (tls_var)
#define MONO_TLS_SET_VALUE(tls_var,tls_key,value) (tls_var = value)

void
mono_tls_init_gc_keys (void)
{
	MONO_THREAD_VAR_OFFSET (mono_tls_sgen_thread_info, tls_offsets [TLS_KEY_SGEN_THREAD_INFO]);
}

void
mono_tls_init_runtime_keys (void)
{
	MONO_THREAD_VAR_OFFSET (mono_tls_thread, tls_offsets [TLS_KEY_THREAD]);
	MONO_THREAD_VAR_OFFSET (mono_tls_jit_tls, tls_offsets [TLS_KEY_JIT_TLS]);
	MONO_THREAD_VAR_OFFSET (mono_tls_domain, tls_offsets [TLS_KEY_DOMAIN]);
	MONO_THREAD_VAR_OFFSET (mono_tls_lmf_addr, tls_offsets [TLS_KEY_LMF_ADDR]);
}

/*
 * Gets the tls offset associated with the key. This offset is set at key
 * initialization (at runtime). Certain targets can implement computing
 * this offset and using it at runtime for fast inlined tls access.
 */
gint32
mono_tls_get_tls_offset (MonoTlsKey key)
{
	g_assert (tls_offsets [key]);
	return tls_offsets [key];
}

/*
 * Returns the getter (gpointer (*)(void)) for the mono tls key.
 * Managed code will always get the value by calling this getter.
 */
MonoTlsGetter
mono_tls_get_tls_getter (MonoTlsKey key)
{
	switch (key) {
	case TLS_KEY_THREAD:
		return (MonoTlsGetter)mono_tls_get_thread;
	case TLS_KEY_JIT_TLS:
		return (MonoTlsGetter)mono_tls_get_jit_tls;
	case TLS_KEY_DOMAIN:
		return (MonoTlsGetter)mono_tls_get_domain;
	case TLS_KEY_SGEN_THREAD_INFO:
		return (MonoTlsGetter)mono_tls_get_sgen_thread_info;
	case TLS_KEY_LMF_ADDR:
		return (MonoTlsGetter)mono_tls_get_lmf_addr;
	}
	g_assert_not_reached ();
	return NULL;
}

// Provide functions for all the macros, for C to use.

// Implement function by invoking macro with same name.
#define FUNCTION_FOR_MACRO(type, name, ret, sig_types, sig_names) type (name) sig_types { ret name sig_names; }

#define TLS_GETTER(type, name) FUNCTION_FOR_MACRO (type*, name, return, (void), ())
#define TLS_SETTER(type, name) FUNCTION_FOR_MACRO (void, name, , (type* value), (value))

#define TLS_FUNCTIONS(type, name) TLS_GETTER (type, mono_tls_get_ ## name) TLS_SETTER (type, mono_tls_set_ ## name)

// For search:
//  mono_tls_get_thread
//  mono_tls_set_thread
//  mono_tls_get_jit_tls
//  mono_tls_set_jit_tls
//  mono_tls_get_domain
//  mono_tls_set_domain
//  mono_tls_get_sgen_thread_info
//  mono_tls_set_sgen_thread_info
//  mono_tls_get_lmf_addr
//  mono_tls_set_lmf_addr
TLS_FUNCTIONS (MonoInternalThread, thread)
TLS_FUNCTIONS (MonoJitTlsData, jit_tls)
TLS_FUNCTIONS (MonoDomain, domain)
TLS_FUNCTIONS (SgenThreadInfo, sgen_thread_info)
TLS_FUNCTIONS (MonoLMF*, lmf_addr)
