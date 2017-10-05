/**
 * \file
 * Atomic operations
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 * Copyright 2012 Xamarin Inc
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#ifndef _WAPI_ATOMIC_H_
#define _WAPI_ATOMIC_H_

#include "config.h"
#include <glib.h>
#include <mono/utils/mono-membar.h>

/*
The current Nexus 7 arm-v7a fails with:
F/MonoDroid( 1568): shared runtime initialization error: Cannot load library: reloc_library[1285]:    37 cannot locate '__sync_val_compare_and_swap_8'

Apple targets have historically being problematic, xcode 4.6 would miscompile the intrinsic.
*/

#define TO_INTERLOCKED_INT8_ARGP(ptr) ((volatile gint8 *)(ptr))
#define TO_INTERLOCKED_INT8_ARG(arg) ((gint8)(arg))

#define TO_INTERLOCKED_INT16_ARGP(ptr) ((volatile gint16 *)(ptr))
#define TO_INTERLOCKED_INT16_ARG(arg) ((gint16)(arg))

#define TO_INTERLOCKED_INT32_ARGP(ptr) ((volatile gint32 *)(ptr))
#define TO_INTERLOCKED_INT32_ARG(arg) ((gint32)(arg))

#define TO_INTERLOCKED_INT64_ARGP(ptr) ((volatile gint64 *)(ptr))
#define TO_INTERLOCKED_INT64_ARG(arg) ((gint64)(arg))

#define TO_INTERLOCKED_POINTER_ARGP(ptr) ((volatile gpointer *)(ptr))
#define TO_INTERLOCKED_POINTER_ARG(arg) ((gpointer)(arg))

/* On Windows, we always use the functions provided by the Windows API. */
/* NOTE, there are some variations in Win32 interlocked signatures compared to the once defined in atomic.h */
/* This cause a lot of build warnings when using MSVC with warning level 4. Redefined the interlocked methods */
/* to use same signature on Windows as on other platforms to reduce warnings. The redefined version will also make */
/* sure we use compiler intrinsic for interlocked methods. */

#if defined(__WIN32__) || defined(_WIN32)

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <windows.h>

//gint32 InterlockedExchangeAdd (volatile gint32 *dest, gint32 add);
static inline gint32 mono_win32_interlocked_exchange_add (volatile gint32 *dest, gint32 add)
{
	return _InterlockedExchangeAdd ((LONG volatile *)dest, (LONG)add);
}

#undef InterlockedExchangeAdd
#define InterlockedExchangeAdd mono_win32_interlocked_exchange_add

//gint64 InterlockedExchangeAdd64 (volatile gint64 *dest, gint64 add);
static inline gint64 mono_win32_interlocked_exchange_add64 (volatile gint64 *dest, gint64 add)
{
	return _InterlockedExchangeAdd64 ((LONG64 volatile *)dest, (LONG64)add);
}

#undef InterlockedExchangeAdd64
#define InterlockedExchangeAdd64 mono_win32_interlocked_exchange_add64

//gint32 InterlockedCompareExchange (volatile gint32 *dest, gint32 exch, gint32 comp);
static inline gint32 mono_win32_interlocked_compare_exchange (volatile gint32 *dest, gint32 exch, gint32 comp)
{
	return _InterlockedCompareExchange ((LONG volatile *)dest, (LONG)exch, (LONG)comp);
}

#undef InterlockedCompareExchange
#define InterlockedCompareExchange mono_win32_interlocked_compare_exchange

//gint64 InterlockedCompareExchange64 (volatile gint64 *dest, gint64 exch, gint64 comp);
static inline gint64 mono_win32_interlocked_compare_exchange64 (volatile gint64 *dest, gint64 exch, gint64 comp)
{
	return _InterlockedCompareExchange64 ((LONG64 volatile *)dest, (LONG64)exch, (LONG64)comp);
}

#undef InterlockedCompareExchange64
#define InterlockedCompareExchange64 mono_win32_interlocked_compare_exchange64

/* mingw is missing InterlockedCompareExchange64 () from winbase.h */
#if HAVE_DECL_INTERLOCKEDCOMPAREEXCHANGE64==0
static inline gint64 InterlockedCompareExchange64 (volatile gint64 *dest, gint64 exch, gint64 comp)
{
	return __sync_val_compare_and_swap (dest, comp, exch);
}
#endif

//gpointer InterlockedCompareExchangePointer (volatile gpointer *dest, gpointer exch, gpointer comp);
static inline gpointer mono_win32_interlocked_compare_exchange_pointer (volatile gpointer *dest, gpointer exch, gpointer comp)
{
	return _InterlockedCompareExchangePointer ((PVOID volatile *)dest, (PVOID)exch, (PVOID)comp);
}

#undef InterlockedCompareExchangePointer
#define InterlockedCompareExchangePointer mono_win32_interlocked_compare_exchange_pointer

//gint32 InterlockedAdd (volatile gint32 *dest, gint32 add);
static inline gint32 mono_win32_interlocked_add (volatile gint32 *dest, gint32 add)
{
	return _InterlockedAdd ((LONG volatile *)dest, (LONG)add);
}

#undef InterlockedAdd
#define InterlockedAdd mono_win32_interlocked_add

/* mingw is missing InterlockedAdd () from winbase.h */
#if HAVE_DECL_INTERLOCKEDADD==0
static inline gint32 InterlockedAdd (volatile gint32 *dest, gint32 add)
{
	return __sync_add_and_fetch (dest, add);
}
#endif

//gint64 InterlockedAdd64 (volatile gint64 *dest, gint64 add);
static inline gint64 mono_win32_interlocked_add64 (volatile gint64 *dest, gint64 add)
{
	return _InterlockedAdd64 ((LONG64 volatile *)dest, (LONG64)add);
}

#undef InterlockedAdd64
#define InterlockedAdd64 mono_win32_interlocked_add64

/* mingw is missing InterlockedAdd64 () from winbase.h */
#if HAVE_DECL_INTERLOCKEDADD64==0
static inline gint64 InterlockedAdd64 (volatile gint64 *dest, gint64 add)
{
	return __sync_add_and_fetch (dest, add);
}
#endif

//gint32 InterlockedIncrement (volatile gint32 *dest);
static inline gint32 mono_win32_interlocked_increment (volatile gint32 *dest)
{
	return _InterlockedIncrement ((LONG volatile *)dest);
}

#undef InterlockedIncrement
#define InterlockedIncrement mono_win32_interlocked_increment

//gint64 InterlockedIncrement64 (volatile gint64 *dest);
static inline gint64 mono_win32_interlocked_increment64 (volatile gint64 *dest)
{
	return _InterlockedIncrement64 ((LONG64 volatile *)dest);
}

#undef InterlockedIncrement64
#define InterlockedIncrement64 mono_win32_interlocked_increment64

/* mingw is missing InterlockedIncrement64 () from winbase.h */
#if HAVE_DECL_INTERLOCKEDINCREMENT64==0
static inline gint64 InterlockedIncrement64 (volatile gint64 *dest)
{
	return __sync_add_and_fetch (dest, 1);
}
#endif

//gint32 InterlockedDecrement (volatile gint32 *dest);
static inline gint32 mono_win32_interlocked_decrement (volatile gint32 *dest)
{
	return _InterlockedDecrement ((LONG volatile *)dest);
}

#undef InterlockedDecrement
#define InterlockedDecrement mono_win32_interlocked_decrement

//gint64 InterlockedDecrement64 (volatile gint64 *dest);
static inline gint64 mono_win32_interlocked_decrement64 (volatile gint64 *dest)
{
	return _InterlockedDecrement64 ((LONG64 volatile *)dest);
}

#undef InterlockedDecrement64
#define InterlockedDecrement64 mono_win32_interlocked_decrement64

/* mingw is missing InterlockedDecrement64 () from winbase.h */
#if HAVE_DECL_INTERLOCKEDDECREMENT64==0
static inline gint64 InterlockedDecrement64 (volatile gint64 *dest)
{
	return __sync_sub_and_fetch (dest, 1);
}
#endif

//gint32 InterlockedExchange (volatile gint32 *dest, gint32 exch);
static inline gint32 mono_win32_interlocked_exchange (volatile gint32 *dest, gint32 exch)
{
	return _InterlockedExchange ((LONG volatile *)dest, (LONG)exch);
}

#undef InterlockedExchange
#define InterlockedExchange mono_win32_interlocked_exchange

//gint64 InterlockedExchange64 (volatile gint64 *dest, gint64 exch);
static inline gint64 mono_win32_interlocked_exchange64 (volatile gint64 *dest, gint64 exch)
{
	return _InterlockedExchange64 ((LONG64 volatile *)dest, (LONG64)exch);
}

#undef InterlockedExchange64
#define InterlockedExchange64 mono_win32_interlocked_exchange64

/* mingw is missing InterlockedExchange64 () from winbase.h */
#if HAVE_DECL_INTERLOCKEDEXCHANGE64==0
static inline gint64 InterlockedExchange64 (volatile gint64 *val, gint64 new_val)
{
	gint64 old_val;
	do {
		old_val = *val;
	} while (InterlockedCompareExchange64 (val, new_val, old_val) != old_val);
	return old_val;
}
#endif

//gpointer InterlockedExchangePointer (volatile gpointer *dest, gpointer exch);
static inline gpointer mono_win32_interlocked_exchange_pointer (volatile gpointer *dest, gpointer exch)
{
	return _InterlockedExchangePointer ((PVOID volatile *)dest, exch);
}

#undef InterlockedExchangePointer
#define InterlockedExchangePointer mono_win32_interlocked_exchange_pointer

/* And now for some dirty hacks... The Windows API doesn't
 * provide any useful primitives for this (other than getting
 * into architecture-specific madness), so use CAS. */

static inline gint32 InterlockedRead(volatile gint32 *src)
{
	return InterlockedCompareExchange (src, 0, 0);
}

static inline gint64 InterlockedRead64(volatile gint64 *src)
{
	return InterlockedCompareExchange64 (src, 0, 0);
}

static inline gpointer InterlockedReadPointer(volatile gpointer *src)
{
	return InterlockedCompareExchangePointer (src, NULL, NULL);
}

static inline void InterlockedWrite(volatile gint32 *dst, gint32 val)
{
	InterlockedExchange (dst, val);
}

static inline void InterlockedWrite64(volatile gint64 *dst, gint64 val)
{
	InterlockedExchange64 (dst, val);
}

static inline void InterlockedWritePointer(volatile gpointer *dst, gpointer val)
{
	InterlockedExchangePointer (dst, val);
}

/* We can't even use CAS for these, so write them out
 * explicitly according to x86(_64) semantics... */

static inline gint8 InterlockedRead8(volatile gint8 *src)
{
	return *src;
}

static inline gint16 InterlockedRead16(volatile gint16 *src)
{
	return *src;
}

static inline void InterlockedWrite8(volatile gint8 *dst, gint8 val)
{
	*dst = val;
	mono_memory_barrier ();
}

static inline void InterlockedWrite16(volatile gint16 *dst, gint16 val)
{
	*dst = val;
	mono_memory_barrier ();
}

/* Prefer GCC atomic ops if the target supports it (see configure.ac). */
#elif defined(USE_GCC_ATOMIC_OPS)

/*
 * As of this comment (August 2016), all current Clang versions get atomic
 * intrinsics on ARM64 wrong. All GCC versions prior to 5.3.0 do, too. The bug
 * is the same: The compiler developers thought that the acq + rel barriers
 * that ARM64 load/store instructions can impose are sufficient to provide
 * sequential consistency semantics. This is not the case:
 *
 *     http://lists.infradead.org/pipermail/linux-arm-kernel/2014-February/229588.html
 *
 * We work around this bug by inserting full barriers around each atomic
 * intrinsic if we detect that we're built with a buggy compiler.
 */

#if defined (HOST_ARM64) && (defined (__clang__) || MONO_GNUC_VERSION < 50300)
#define WRAP_ATOMIC_INTRINSIC(INTRIN) \
	({ \
		mono_memory_barrier (); \
		__typeof__ (INTRIN) atomic_ret__ = (INTRIN); \
		mono_memory_barrier (); \
		atomic_ret__; \
	})

#define gcc_sync_val_compare_and_swap(a, b, c) WRAP_ATOMIC_INTRINSIC (__sync_val_compare_and_swap (a, b, c))
#define gcc_sync_add_and_fetch(a, b) WRAP_ATOMIC_INTRINSIC (__sync_add_and_fetch (a, b))
#define gcc_sync_sub_and_fetch(a, b) WRAP_ATOMIC_INTRINSIC (__sync_sub_and_fetch (a, b))
#define gcc_sync_fetch_and_add(a, b) WRAP_ATOMIC_INTRINSIC (__sync_fetch_and_add (a, b))
#else
#define gcc_sync_val_compare_and_swap(a, b, c) __sync_val_compare_and_swap (a, b, c)
#define gcc_sync_add_and_fetch(a, b) __sync_add_and_fetch (a, b)
#define gcc_sync_sub_and_fetch(a, b) __sync_sub_and_fetch (a, b)
#define gcc_sync_fetch_and_add(a, b) __sync_fetch_and_add (a, b)
#endif

static inline gint32 InterlockedCompareExchange(volatile gint32 *dest,
						gint32 exch, gint32 comp)
{
	return gcc_sync_val_compare_and_swap (dest, comp, exch);
}

static inline gpointer InterlockedCompareExchangePointer(volatile gpointer *dest, gpointer exch, gpointer comp)
{
	return gcc_sync_val_compare_and_swap (dest, comp, exch);
}

static inline gint32 InterlockedAdd(volatile gint32 *dest, gint32 add)
{
	return gcc_sync_add_and_fetch (dest, add);
}

static inline gint32 InterlockedIncrement(volatile gint32 *val)
{
	return gcc_sync_add_and_fetch (val, 1);
}

static inline gint32 InterlockedDecrement(volatile gint32 *val)
{
	return gcc_sync_sub_and_fetch (val, 1);
}

static inline gint32 InterlockedExchange(volatile gint32 *val, gint32 new_val)
{
	gint32 old_val;
	do {
		old_val = *val;
	} while (gcc_sync_val_compare_and_swap (val, old_val, new_val) != old_val);
	return old_val;
}

static inline gpointer InterlockedExchangePointer(volatile gpointer *val,
						  gpointer new_val)
{
	gpointer old_val;
	do {
		old_val = *val;
	} while (gcc_sync_val_compare_and_swap (val, old_val, new_val) != old_val);
	return old_val;
}

static inline gint32 InterlockedExchangeAdd(volatile gint32 *val, gint32 add)
{
	return gcc_sync_fetch_and_add (val, add);
}

static inline gint8 InterlockedRead8(volatile gint8 *src)
{
	/* Kind of a hack, but GCC doesn't give us anything better, and it's
	 * certainly not as bad as using a CAS loop. */
	return gcc_sync_fetch_and_add (src, 0);
}

static inline gint16 InterlockedRead16(volatile gint16 *src)
{
	return gcc_sync_fetch_and_add (src, 0);
}

static inline gint32 InterlockedRead(volatile gint32 *src)
{
	return gcc_sync_fetch_and_add (src, 0);
}

static inline void InterlockedWrite8(volatile gint8 *dst, gint8 val)
{
	/* Nothing useful from GCC at all, so fall back to CAS. */
	gint8 old_val;
	do {
		old_val = *dst;
	} while (gcc_sync_val_compare_and_swap (dst, old_val, val) != old_val);
}

static inline void InterlockedWrite16(volatile gint16 *dst, gint16 val)
{
	gint16 old_val;
	do {
		old_val = *dst;
	} while (gcc_sync_val_compare_and_swap (dst, old_val, val) != old_val);
}

static inline void InterlockedWrite(volatile gint32 *dst, gint32 val)
{
	/* Nothing useful from GCC at all, so fall back to CAS. */
	gint32 old_val;
	do {
		old_val = *dst;
	} while (gcc_sync_val_compare_and_swap (dst, old_val, val) != old_val);
}

#if defined (TARGET_OSX) || defined (__arm__) || (defined (__mips__) && !defined (__mips64)) || (defined (__powerpc__) && !defined (__powerpc64__)) || (defined (__sparc__) && !defined (__arch64__))
#define BROKEN_64BIT_ATOMICS_INTRINSIC 1
#endif

#if !defined (BROKEN_64BIT_ATOMICS_INTRINSIC)

static inline gint64 InterlockedCompareExchange64(volatile gint64 *dest, gint64 exch, gint64 comp)
{
	return gcc_sync_val_compare_and_swap (dest, comp, exch);
}

static inline gint64 InterlockedAdd64(volatile gint64 *dest, gint64 add)
{
	return gcc_sync_add_and_fetch (dest, add);
}

static inline gint64 InterlockedIncrement64(volatile gint64 *val)
{
	return gcc_sync_add_and_fetch (val, 1);
}

static inline gint64 InterlockedDecrement64(volatile gint64 *val)
{
	return gcc_sync_sub_and_fetch (val, 1);
}

static inline gint64 InterlockedExchangeAdd64(volatile gint64 *val, gint64 add)
{
	return gcc_sync_fetch_and_add (val, add);
}

static inline gint64 InterlockedRead64(volatile gint64 *src)
{
	/* Kind of a hack, but GCC doesn't give us anything better. */
	return gcc_sync_fetch_and_add (src, 0);
}

#else

/* Implement 64-bit cmpxchg by hand or emulate it. */
extern gint64 InterlockedCompareExchange64(volatile gint64 *dest, gint64 exch, gint64 comp);

/* Implement all other 64-bit atomics in terms of a specialized CAS
 * in this case, since chances are that the other 64-bit atomic
 * intrinsics are broken too.
 */

static inline gint64 InterlockedExchangeAdd64(volatile gint64 *dest, gint64 add)
{
	gint64 old_val;
	do {
		old_val = *dest;
	} while (InterlockedCompareExchange64 (dest, old_val + add, old_val) != old_val);
	return old_val;
}

static inline gint64 InterlockedIncrement64(volatile gint64 *val)
{
	gint64 get, set;
	do {
		get = *val;
		set = get + 1;
	} while (InterlockedCompareExchange64 (val, set, get) != get);
	return set;
}

static inline gint64 InterlockedDecrement64(volatile gint64 *val)
{
	gint64 get, set;
	do {
		get = *val;
		set = get - 1;
	} while (InterlockedCompareExchange64 (val, set, get) != get);
	return set;
}

static inline gint64 InterlockedAdd64(volatile gint64 *dest, gint64 add)
{
	gint64 get, set;
	do {
		get = *dest;
		set = get + add;
	} while (InterlockedCompareExchange64 (dest, set, get) != get);
	return set;
}

static inline gint64 InterlockedRead64(volatile gint64 *src)
{
	return InterlockedCompareExchange64 (src, 0, 0);
}

#endif

static inline gpointer InterlockedReadPointer(volatile gpointer *src)
{
	return InterlockedCompareExchangePointer (src, NULL, NULL);
}

static inline void InterlockedWritePointer(volatile gpointer *dst, gpointer val)
{
	InterlockedExchangePointer (dst, val);
}

/* We always implement this in terms of a 64-bit cmpxchg since
 * GCC doesn't have an intrisic to model it anyway. */
static inline gint64 InterlockedExchange64(volatile gint64 *val, gint64 new_val)
{
	gint64 old_val;
	do {
		old_val = *val;
	} while (InterlockedCompareExchange64 (val, new_val, old_val) != old_val);
	return old_val;
}

static inline void InterlockedWrite64(volatile gint64 *dst, gint64 val)
{
	/* Nothing useful from GCC at all, so fall back to CAS. */
	InterlockedExchange64 (dst, val);
}

#else

#define WAPI_NO_ATOMIC_ASM

extern gint32 InterlockedCompareExchange(volatile gint32 *dest, gint32 exch, gint32 comp);
extern gint64 InterlockedCompareExchange64(volatile gint64 *dest, gint64 exch, gint64 comp);
extern gpointer InterlockedCompareExchangePointer(volatile gpointer *dest, gpointer exch, gpointer comp);
extern gint32 InterlockedAdd(volatile gint32 *dest, gint32 add);
extern gint64 InterlockedAdd64(volatile gint64 *dest, gint64 add);
extern gint32 InterlockedIncrement(volatile gint32 *dest);
extern gint64 InterlockedIncrement64(volatile gint64 *dest);
extern gint32 InterlockedDecrement(volatile gint32 *dest);
extern gint64 InterlockedDecrement64(volatile gint64 *dest);
extern gint32 InterlockedExchange(volatile gint32 *dest, gint32 exch);
extern gint64 InterlockedExchange64(volatile gint64 *dest, gint64 exch);
extern gpointer InterlockedExchangePointer(volatile gpointer *dest, gpointer exch);
extern gint32 InterlockedExchangeAdd(volatile gint32 *dest, gint32 add);
extern gint64 InterlockedExchangeAdd64(volatile gint64 *dest, gint64 add);
extern gint8 InterlockedRead8(volatile gint8 *src);
extern gint16 InterlockedRead16(volatile gint16 *src);
extern gint32 InterlockedRead(volatile gint32 *src);
extern gint64 InterlockedRead64(volatile gint64 *src);
extern gpointer InterlockedReadPointer(volatile gpointer *src);
extern void InterlockedWrite8(volatile gint8 *dst, gint8 val);
extern void InterlockedWrite16(volatile gint16 *dst, gint16 val);
extern void InterlockedWrite(volatile gint32 *dst, gint32 val);
extern void InterlockedWrite64(volatile gint64 *dst, gint64 val);
extern void InterlockedWritePointer(volatile gpointer *dst, gpointer val);

#endif

#if SIZEOF_VOID_P == 4
#define InterlockedAddP(p,add) InterlockedAdd ((volatile gint32*)p, (gint32)add)
#else
#define InterlockedAddP(p,add) InterlockedAdd64 ((volatile gint64*)p, (gint64)add)
#endif

/* The following functions cannot be found on any platform, and thus they can be declared without further existence checks */

static inline void
InterlockedWriteBool (volatile gboolean *dest, gboolean val)
{
	/* both, gboolean and gint32, are int32_t; the purpose of these casts is to make things explicit */
	InterlockedWrite ((volatile gint32 *)dest, (gint32)val);
}

#endif /* _WAPI_ATOMIC_H_ */
