/*
 * mono-membar.h: Memory barrier inline functions
 *
 * Author:
 *	Mark Probst (mark.probst@gmail.com)
 *
 * (C) 2007 Novell, Inc
 */

#ifndef _MONO_UTILS_MONO_MEMBAR_H_
#define _MONO_UTILS_MONO_MEMBAR_H_

#include <config.h>

#include <glib.h>

#include "mono-arch.h"

#if MONO_ARCH == MONO_ARCH_X86_64
#ifndef _MSC_VER
static inline void mono_memory_barrier (void)
{
	__asm__ __volatile__ ("mfence" : : : "memory");
}

static inline void mono_memory_read_barrier (void)
{
	__asm__ __volatile__ ("lfence" : : : "memory");
}

static inline void mono_memory_write_barrier (void)
{
	__asm__ __volatile__ ("sfence" : : : "memory");
}
#else
#include <intrin.h>

static inline void mono_memory_barrier (void)
{
	_ReadWriteBarrier ();
}

static inline void mono_memory_read_barrier (void)
{
	_ReadBarrier ();
}

static inline void mono_memory_write_barrier (void)
{
	_WriteBarrier ();
}
#endif
#elif MONO_ARCH == MONO_ARCH_X86
#ifndef _MSC_VER
static inline void mono_memory_barrier (void)
{
	__asm__ __volatile__ ("lock; addl $0,0(%%esp)" : : : "memory");
}

static inline void mono_memory_read_barrier (void)
{
	mono_memory_barrier ();
}

static inline void mono_memory_write_barrier (void)
{
	mono_memory_barrier ();
}
#else
#include <intrin.h>

static inline void mono_memory_barrier (void)
{
	_ReadWriteBarrier ();
}

static inline void mono_memory_read_barrier (void)
{
	_ReadBarrier ();
}

static inline void mono_memory_write_barrier (void)
{
	_WriteBarrier ();
}
#endif
#elif defined(MONO_ARCH_IS_SPARC)
static inline void mono_memory_barrier (void)
{
	__asm__ __volatile__ ("membar	#LoadLoad | #LoadStore | #StoreStore | #StoreLoad" : : : "memory");
}

static inline void mono_memory_read_barrier (void)
{
	__asm__ __volatile__ ("membar	#LoadLoad" : : : "memory");
}

static inline void mono_memory_write_barrier (void)
{
	__asm__ __volatile__ ("membar	#StoreStore" : : : "memory");
}
#elif defined(MONO_ARCH_IS_S390)
static inline void mono_memory_barrier (void)
{
	__asm__ __volatile__ ("bcr 15,0" : : : "memory");
}

static inline void mono_memory_read_barrier (void)
{
	mono_memory_barrier ();
}

static inline void mono_memory_write_barrier (void)
{
	mono_memory_barrier ();
}
#elif defined(MONO_ARCH_IS_PPC)
static inline void mono_memory_barrier (void)
{
	__asm__ __volatile__ ("sync" : : : "memory");
}

static inline void mono_memory_read_barrier (void)
{
	mono_memory_barrier ();
}

static inline void mono_memory_write_barrier (void)
{
	__asm__ __volatile__ ("eieio" : : : "memory");
}

#elif MONO_ARCH == MONO_ARCH_ARM
static inline void mono_memory_barrier (void)
{
#ifdef HAVE_ARMV6
	__asm__ __volatile__ ("mcr p15, 0, %0, c7, c10, 5" : : "r" (0) : "memory");
#endif
}

static inline void mono_memory_read_barrier (void)
{
	mono_memory_barrier ();
}

static inline void mono_memory_write_barrier (void)
{
	mono_memory_barrier ();
}
#elif MONO_ARCH == MONO_ARCH_IA_64
static inline void mono_memory_barrier (void)
{
	__asm__ __volatile__ ("mf" : : : "memory");
}

static inline void mono_memory_read_barrier (void)
{
	mono_memory_barrier ();
}

static inline void mono_memory_write_barrier (void)
{
	mono_memory_barrier ();
}
#elif MONO_ARCH == MONO_ARCH_ALPHA
static inline void mono_memory_barrier (void)
{
        __asm__ __volatile__ ("mb" : : : "memory");
}

static inline void mono_memory_read_barrier (void)
{
        mono_memory_barrier ();
}

static inline void mono_memory_write_barrier (void)
{
        mono_memory_barrier ();
}
#elif defined(MONO_ARCH_IS_MIPS)
static inline void mono_memory_barrier (void)
{
        __asm__ __volatile__ ("" : : : "memory");
}

static inline void mono_memory_read_barrier (void)
{
        mono_memory_barrier ();
}

static inline void mono_memory_write_barrier (void)
{
        mono_memory_barrier ();
}
#elif defined(MONO_CROSS_COMPILE)
static inline void mono_memory_barrier (void)
{
}

static inline void mono_memory_read_barrier (void)
{
}

static inline void mono_memory_write_barrier (void)
{
}
#endif

#endif	/* _MONO_UTILS_MONO_MEMBAR_H_ */
