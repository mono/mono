/* 
 * Copyright 1988, 1989 Hans-J. Boehm, Alan J. Demers
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1996-1999 by Silicon Graphics.  All rights reserved.
 * Copyright (c) 1999 by Hewlett-Packard Company. All rights reserved.
 *
 *
 * THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
 * OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
 *
 * Permission is hereby granted to use or copy this program
 * for any purpose,  provided the above notices are retained on all copies.
 * Permission to modify the code and to distribute modified code is granted,
 * provided the above notices are retained, and a notice that the code was
 * modified is included with the above copyright notice.
 */

#ifndef GC_LOCKS_H
#define GC_LOCKS_H

/*
 * Mutual exclusion between allocator/collector routines.
 * Needed if there is more than one allocator thread.
 * FASTLOCK() is assumed to try to acquire the lock in a cheap and
 * dirty way that is acceptable for a few instructions, e.g. by
 * inhibiting preemption.  This is assumed to have succeeded only
 * if a subsequent call to FASTLOCK_SUCCEEDED() returns TRUE.
 * FASTUNLOCK() is called whether or not FASTLOCK_SUCCEEDED().
 * If signals cannot be tolerated with the FASTLOCK held, then
 * FASTLOCK should disable signals.  The code executed under
 * FASTLOCK is otherwise immune to interruption, provided it is
 * not restarted.
 * DCL_LOCK_STATE declares any local variables needed by LOCK and UNLOCK
 * and/or DISABLE_SIGNALS and ENABLE_SIGNALS and/or FASTLOCK.
 * (There is currently no equivalent for FASTLOCK.)
 *
 * In the PARALLEL_MARK case, we also need to define a number of
 * other inline finctions here:
 *   GC_bool GC_compare_and_exchange( volatile GC_word *addr,
 *   				      GC_word old, GC_word new )
 *   GC_word GC_atomic_add( volatile GC_word *addr, GC_word how_much )
 *   void GC_memory_barrier( )
 *   
 */  
# ifdef THREADS
   void GC_noop1 GC_PROTO((word));
#  ifdef PCR_OBSOLETE	/* Faster, but broken with multiple lwp's	*/
#    include  "th/PCR_Th.h"
#    include  "th/PCR_ThCrSec.h"
     extern struct PCR_Th_MLRep GC_allocate_ml;
#    define DCL_LOCK_STATE  PCR_sigset_t GC_old_sig_mask
#    define LOCK() PCR_Th_ML_Acquire(&GC_allocate_ml) 
#    define UNLOCK() PCR_Th_ML_Release(&GC_allocate_ml)
#    define UNLOCK() PCR_Th_ML_Release(&GC_allocate_ml)
#    define FASTLOCK() PCR_ThCrSec_EnterSys()
     /* Here we cheat (a lot): */
#        define FASTLOCK_SUCCEEDED() (*(int *)(&GC_allocate_ml) == 0)
		/* TRUE if nobody currently holds the lock */
#    define FASTUNLOCK() PCR_ThCrSec_ExitSys()
#  endif
#  ifdef PCR
#    include <base/PCR_Base.h>
#    include <th/PCR_Th.h>
     extern PCR_Th_ML GC_allocate_ml;
#    define DCL_LOCK_STATE \
	 PCR_ERes GC_fastLockRes; PCR_sigset_t GC_old_sig_mask
#    define LOCK() PCR_Th_ML_Acquire(&GC_allocate_ml)
#    define UNLOCK() PCR_Th_ML_Release(&GC_allocate_ml)
#    define FASTLOCK() (GC_fastLockRes = PCR_Th_ML_Try(&GC_allocate_ml))
#    define FASTLOCK_SUCCEEDED() (GC_fastLockRes == PCR_ERes_okay)
#    define FASTUNLOCK()  {\
        if( FASTLOCK_SUCCEEDED() ) PCR_Th_ML_Release(&GC_allocate_ml); }
#  endif
#  ifdef SRC_M3
     extern GC_word RT0u__inCritical;
#    define LOCK() RT0u__inCritical++
#    define UNLOCK() RT0u__inCritical--
#  endif
#  ifdef GC_SOLARIS_THREADS
#    include <thread.h>
#    include <signal.h>
     extern mutex_t GC_allocate_ml;
#    define LOCK() mutex_lock(&GC_allocate_ml);
#    define UNLOCK() mutex_unlock(&GC_allocate_ml);
#  endif

/* Try to define GC_TEST_AND_SET and a matching GC_CLEAR for spin lock	*/
/* acquisition and release.  We need this for correct operation of the	*/
/* incremental GC.							*/
#  ifdef __GNUC__
#    if defined(I386)
       inline static int GC_test_and_set(volatile unsigned int *addr) {
	  int oldval;
	  /* Note: the "xchg" instruction does not need a "lock" prefix */
	  __asm__ __volatile__("xchgl %0, %1"
		: "=r"(oldval), "=m"(*(addr))
		: "0"(1), "m"(*(addr)) : "memory");
	  return oldval;
       }
#      define GC_TEST_AND_SET_DEFINED
#    endif
#    if defined(IA64)
#      if defined(__INTEL_COMPILER)
#        include <ia64intrin.h>
#      endif
       inline static int GC_test_and_set(volatile unsigned int *addr) {
	  long oldval, n = 1;
#	ifndef __INTEL_COMPILER
	  __asm__ __volatile__("xchg4 %0=%1,%2"
		: "=r"(oldval), "=m"(*addr)
		: "r"(n), "1"(*addr) : "memory");
#	else
	  oldval = _InterlockedExchange(addr, n);
#	endif
	  return oldval;
       }
#      define GC_TEST_AND_SET_DEFINED
       /* Should this handle post-increment addressing?? */
       inline static void GC_clear(volatile unsigned int *addr) {
#	ifndef __INTEL_COMPILER
	 __asm__ __volatile__("st4.rel %0=r0" : "=m" (*addr) : : "memory");
#	else
	// there is no st4 but I can use xchg I hope
	 _InterlockedExchange(addr, 0);
#	endif
       }
#      define GC_CLEAR_DEFINED
#    endif
#    ifdef SPARC
       inline static int GC_test_and_set(volatile unsigned int *addr) {
	 int oldval;

	 __asm__ __volatile__("ldstub %1,%0"
	 : "=r"(oldval), "=m"(*addr)
	 : "m"(*addr) : "memory");
	 return oldval;
       }
#      define GC_TEST_AND_SET_DEFINED
#    endif
#    ifdef M68K
       /* Contributed by Tony Mantler.  I'm not sure how well it was	*/
       /* tested.							*/
       inline static int GC_test_and_set(volatile unsigned int *addr) {
          char oldval; /* this must be no longer than 8 bits */

          /* The return value is semi-phony. */
          /* 'tas' sets bit 7 while the return */
          /* value pretends bit 0 was set */
          __asm__ __volatile__(
                 "tas %1@; sne %0; negb %0"
                 : "=d" (oldval)
                 : "a" (addr) : "memory");
          return oldval;
       }
#      define GC_TEST_AND_SET_DEFINED
#    endif
#    if defined(POWERPC)
        inline static int GC_test_and_set(volatile unsigned int *addr) {
          int oldval;
          int temp = 1; /* locked value */

          __asm__ __volatile__(
               "1:\tlwarx %0,0,%3\n"   /* load and reserve               */
               "\tcmpwi %0, 0\n"       /* if load is                     */
               "\tbne 2f\n"            /*   non-zero, return already set */
               "\tstwcx. %2,0,%1\n"    /* else store conditional         */
               "\tbne- 1b\n"           /* retry if lost reservation      */
               "\tsync\n"              /* import barrier                 */
               "2:\t\n"                /* oldval is zero if we set       */
              : "=&r"(oldval), "=p"(addr)
              : "r"(temp), "1"(addr)
              : "cr0","memory");
          return oldval;
        }
#       define GC_TEST_AND_SET_DEFINED
        inline static void GC_clear(volatile unsigned int *addr) {
	  __asm__ __volatile__("eieio" : : : "memory");
          *(addr) = 0;
        }
#       define GC_CLEAR_DEFINED
#    endif
#    if defined(ALPHA) 
        inline static int GC_test_and_set(volatile unsigned int * addr)
        {
          unsigned long oldvalue;
          unsigned long temp;

          __asm__ __volatile__(
                             "1:     ldl_l %0,%1\n"
                             "       and %0,%3,%2\n"
                             "       bne %2,2f\n"
                             "       xor %0,%3,%0\n"
                             "       stl_c %0,%1\n"
#	ifdef __ELF__
                             "       beq %0,3f\n"
#	else
                             "       beq %0,1b\n"
#	endif
                             "       mb\n"
                             "2:\n"
#	ifdef __ELF__
                             ".section .text2,\"ax\"\n"
                             "3:     br 1b\n"
                             ".previous"
#	endif
                             :"=&r" (temp), "=m" (*addr), "=&r" (oldvalue)
                             :"Ir" (1), "m" (*addr)
			     :"memory");

          return oldvalue;
        }
#       define GC_TEST_AND_SET_DEFINED
        inline static void GC_clear(volatile unsigned int *addr) {
          __asm__ __volatile__("mb" : : : "memory");
          *(addr) = 0;
        }
#       define GC_CLEAR_DEFINED
#    endif /* ALPHA */
#    ifdef ARM32
        inline static int GC_test_and_set(volatile unsigned int *addr) {
          int oldval;
          /* SWP on ARM is very similar to XCHG on x86.  Doesn't lock the
           * bus because there are no SMP ARM machines.  If/when there are,
           * this code will likely need to be updated. */
          /* See linuxthreads/sysdeps/arm/pt-machine.h in glibc-2.1 */
          __asm__ __volatile__("swp %0, %1, [%2]"
      		  	     : "=r"(oldval)
      			     : "r"(1), "r"(addr)
			     : "memory");
          return oldval;
        }
#       define GC_TEST_AND_SET_DEFINED
#    endif /* ARM32 */
#    ifdef S390
       inline static int GC_test_and_set(volatile unsigned int *addr) {
         int ret;
         __asm__ __volatile__ (
          "     l     %0,0(%2)\n"
          "0:   cs    %0,%1,0(%2)\n"
          "     jl    0b"
          : "=&d" (ret)
          : "d" (1), "a" (addr)
          : "cc", "memory");
         return ret;
       }
#    endif
#  endif /* __GNUC__ */
#  if (defined(ALPHA) && !defined(__GNUC__))
#    ifndef OSF1
	--> We currently assume that if gcc is not used, we are
	--> running under Tru64.
#    endif
#    include <machine/builtins.h>
#    include <c_asm.h>
#    define GC_test_and_set(addr) __ATOMIC_EXCH_LONG(addr, 1)
#    define GC_TEST_AND_SET_DEFINED
#    define GC_clear(addr) { asm("mb"); *(volatile unsigned *)addr = 0; }
#    define GC_CLEAR_DEFINED
#  endif
#  if defined(MSWIN32)
#    define GC_test_and_set(addr) InterlockedExchange((LPLONG)addr,1)
#    define GC_TEST_AND_SET_DEFINED
#  endif
#  ifdef MIPS
#    ifdef LINUX
#      include <sys/tas.h>
#      define GC_test_and_set(addr) _test_and_set((int *) addr,1)
#      define GC_TEST_AND_SET_DEFINED
#    elif __mips < 3 || !(defined (_ABIN32) || defined(_ABI64)) \
	|| !defined(_COMPILER_VERSION) || _COMPILER_VERSION < 700
#	 ifdef __GNUC__
#          define GC_test_and_set(addr) _test_and_set((void *)addr,1)
#	 else
#          define GC_test_and_set(addr) test_and_set((void *)addr,1)
#	 endif
#    else
#	 define GC_test_and_set(addr) __test_and_set32((void *)addr,1)
#	 define GC_clear(addr) __lock_release(addr);
#	 define GC_CLEAR_DEFINED
#    endif
#    define GC_TEST_AND_SET_DEFINED
#  endif /* MIPS */
#  if defined(_AIX)
#    include <sys/atomic_op.h>
#    if (defined(_POWER) || defined(_POWERPC)) 
#      if defined(__GNUC__)  
         inline static void GC_memsync() {
           __asm__ __volatile__ ("sync" : : : "memory");
         }
#      else
#        ifndef inline
#          define inline __inline
#        endif
#        pragma mc_func GC_memsync { \
           "7c0004ac" /* sync (same opcode used for dcs)*/ \
         }
#      endif
#    else 
#    error dont know how to memsync
#    endif
     inline static int GC_test_and_set(volatile unsigned int * addr) {
          int oldvalue = 0;
          if (compare_and_swap((void *)addr, &oldvalue, 1)) {
            GC_memsync();
            return 0;
          } else return 1;
     }
#    define GC_TEST_AND_SET_DEFINED
     inline static void GC_clear(volatile unsigned int *addr) {
          GC_memsync();
          *(addr) = 0;
     }
#    define GC_CLEAR_DEFINED

#  endif
#  if 0 /* defined(HP_PA) */
     /* The official recommendation seems to be to not use ldcw from	*/
     /* user mode.  Since multithreaded incremental collection doesn't	*/
     /* work anyway on HP_PA, this shouldn't be a major loss.		*/

     /* "set" means 0 and "clear" means 1 here.		*/
#    define GC_test_and_set(addr) !GC_test_and_clear(addr);
#    define GC_TEST_AND_SET_DEFINED
#    define GC_clear(addr) GC_noop1((word)(addr)); *(volatile unsigned int *)addr = 1;
	/* The above needs a memory barrier! */
#    define GC_CLEAR_DEFINED
#  endif
#  if defined(GC_TEST_AND_SET_DEFINED) && !defined(GC_CLEAR_DEFINED)
#    ifdef __GNUC__
       inline static void GC_clear(volatile unsigned int *addr) {
         /* Try to discourage gcc from moving anything past this. */
         __asm__ __volatile__(" " : : : "memory");
         *(addr) = 0;
       }
#    else
	    /* The function call in the following should prevent the	*/
	    /* compiler from moving assignments to below the UNLOCK.	*/
#      define GC_clear(addr) GC_noop1((word)(addr)); \
			     *((volatile unsigned int *)(addr)) = 0;
#    endif
#    define GC_CLEAR_DEFINED
#  endif /* !GC_CLEAR_DEFINED */

#  if !defined(GC_TEST_AND_SET_DEFINED)
#    define USE_PTHREAD_LOCKS
#  endif

#  if defined(GC_PTHREADS) && !defined(GC_SOLARIS_THREADS) \
      && !defined(GC_IRIX_THREADS) && !defined(GC_WIN32_THREADS)
#    define NO_THREAD (pthread_t)(-1)
#    include <pthread.h>
#    if defined(PARALLEL_MARK) 
      /* We need compare-and-swap to update mark bits, where it's	*/
      /* performance critical.  If USE_MARK_BYTES is defined, it is	*/
      /* no longer needed for this purpose.  However we use it in	*/
      /* either case to implement atomic fetch-and-add, though that's	*/
      /* less performance critical, and could perhaps be done with	*/
      /* a lock.							*/
#     if defined(GENERIC_COMPARE_AND_SWAP)
	/* Probably not useful, except for debugging.	*/
	/* We do use GENERIC_COMPARE_AND_SWAP on PA_RISC, but we 	*/
	/* minimize its use.						*/
	extern pthread_mutex_t GC_compare_and_swap_lock;

	/* Note that if GC_word updates are not atomic, a concurrent 	*/
	/* reader should acquire GC_compare_and_swap_lock.  On 		*/
	/* currently supported platforms, such updates are atomic.	*/
	extern GC_bool GC_compare_and_exchange(volatile GC_word *addr,
					       GC_word old, GC_word new_val);
#     endif /* GENERIC_COMPARE_AND_SWAP */
#     if defined(I386)
#      if !defined(GENERIC_COMPARE_AND_SWAP)
         /* Returns TRUE if the comparison succeeded. */
         inline static GC_bool GC_compare_and_exchange(volatile GC_word *addr,
		  				       GC_word old,
						       GC_word new_val) 
         {
	   char result;
	   __asm__ __volatile__("lock; cmpxchgl %2, %0; setz %1"
	    	: "+m"(*(addr)), "=r"(result)
		: "r" (new_val), "a"(old) : "memory");
	   return (GC_bool) result;
         }
#      endif /* !GENERIC_COMPARE_AND_SWAP */
       inline static void GC_memory_barrier()
       {
	 /* We believe the processor ensures at least processor	*/
	 /* consistent ordering.  Thus a compiler barrier	*/
	 /* should suffice.					*/
         __asm__ __volatile__("" : : : "memory");
       }
#     endif /* I386 */

#     if defined(POWERPC)
#      if !defined(GENERIC_COMPARE_AND_SWAP)
        /* Returns TRUE if the comparison succeeded. */
        inline static GC_bool GC_compare_and_exchange(volatile GC_word *addr,
            GC_word old, GC_word new_val) 
        {
            int result, dummy;
            __asm__ __volatile__(
                "1:\tlwarx %0,0,%5\n"
                  "\tcmpw %0,%4\n"
                  "\tbne  2f\n"
                  "\tstwcx. %3,0,%2\n"
                  "\tbne- 1b\n"
                  "\tsync\n"
                  "\tli %1, 1\n"
                  "\tb 3f\n"
                "2:\tli %1, 0\n"
                "3:\t\n"
                :  "=&r" (dummy), "=r" (result), "=p" (addr)
                :  "r" (new_val), "r" (old), "2"(addr)
                : "cr0","memory");
            return (GC_bool) result;
        }
#      endif /* !GENERIC_COMPARE_AND_SWAP */
        inline static void GC_memory_barrier()
        {
            __asm__ __volatile__("sync" : : : "memory");
        }
#     endif /* POWERPC */

#     if defined(IA64)
#      if !defined(GENERIC_COMPARE_AND_SWAP)
         inline static GC_bool GC_compare_and_exchange(volatile GC_word *addr,
						       GC_word old, GC_word new_val) 
	 {
	  unsigned long oldval;
	  __asm__ __volatile__("mov ar.ccv=%4 ;; cmpxchg8.rel %0=%1,%2,ar.ccv"
		: "=r"(oldval), "=m"(*addr)
		: "r"(new_val), "1"(*addr), "r"(old) : "memory");
	  return (oldval == old);
         }
#      endif /* !GENERIC_COMPARE_AND_SWAP */
#      if 0
	/* Shouldn't be needed; we use volatile stores instead. */
        inline static void GC_memory_barrier()
        {
          __asm__ __volatile__("mf" : : : "memory");
        }
#      endif /* 0 */
#     endif /* IA64 */
#     if defined(ALPHA)
#      if !defined(GENERIC_COMPARE_AND_SWAP)
#        if defined(__GNUC__)
           inline static GC_bool GC_compare_and_exchange(volatile GC_word *addr,
						         GC_word old, GC_word new_val) 
	   {
	     unsigned long was_equal;
             unsigned long temp;

             __asm__ __volatile__(
                             "1:     ldq_l %0,%1\n"
                             "       cmpeq %0,%4,%2\n"
			     "	     mov %3,%0\n"
                             "       beq %2,2f\n"
                             "       stq_c %0,%1\n"
                             "       beq %0,1b\n"
                             "2:\n"
                             "       mb\n"
                             :"=&r" (temp), "=m" (*addr), "=&r" (was_equal)
                             : "r" (new_val), "Ir" (old)
			     :"memory");
             return was_equal;
           }
#        else /* !__GNUC__ */
           inline static GC_bool GC_compare_and_exchange(volatile GC_word *addr,
						         GC_word old, GC_word new_val) 
	  {
	    return __CMP_STORE_QUAD(addr, old, new_val, addr);
          }
#        endif /* !__GNUC__ */
#      endif /* !GENERIC_COMPARE_AND_SWAP */
#      ifdef __GNUC__
         inline static void GC_memory_barrier()
         {
           __asm__ __volatile__("mb" : : : "memory");
         }
#      else
#	 define GC_memory_barrier() asm("mb")
#      endif /* !__GNUC__ */
#     endif /* ALPHA */
#     if defined(S390)
#      if !defined(GENERIC_COMPARE_AND_SWAP)
         inline static GC_bool GC_compare_and_exchange(volatile C_word *addr,
                                         GC_word old, GC_word new_val)
         {
           int retval;
           __asm__ __volatile__ (
#            ifndef __s390x__
               "     cs  %1,%2,0(%3)\n"
#            else
               "     csg %1,%2,0(%3)\n"
#            endif
             "     ipm %0\n"
             "     srl %0,28\n"
             : "=&d" (retval), "+d" (old)
             : "d" (new_val), "a" (addr)
             : "cc", "memory");
           return retval == 0;
         }
#      endif
#     endif
#     if !defined(GENERIC_COMPARE_AND_SWAP)
        /* Returns the original value of *addr.	*/
        inline static GC_word GC_atomic_add(volatile GC_word *addr,
					    GC_word how_much)
        {
	  GC_word old;
	  do {
	    old = *addr;
	  } while (!GC_compare_and_exchange(addr, old, old+how_much));
          return old;
        }
#     else /* GENERIC_COMPARE_AND_SWAP */
	/* So long as a GC_word can be atomically updated, it should	*/
	/* be OK to read *addr without a lock.				*/
	extern GC_word GC_atomic_add(volatile GC_word *addr, GC_word how_much);
#     endif /* GENERIC_COMPARE_AND_SWAP */

#    endif /* PARALLEL_MARK */

#    if !defined(THREAD_LOCAL_ALLOC) && !defined(USE_PTHREAD_LOCKS)
      /* In the THREAD_LOCAL_ALLOC case, the allocation lock tends to	*/
      /* be held for long periods, if it is held at all.  Thus spinning	*/
      /* and sleeping for fixed periods are likely to result in 	*/
      /* significant wasted time.  We thus rely mostly on queued locks. */
#     define USE_SPIN_LOCK
      extern volatile unsigned int GC_allocate_lock;
      extern void GC_lock(void);
	/* Allocation lock holder.  Only set if acquired by client through */
	/* GC_call_with_alloc_lock.					   */
#     ifdef GC_ASSERTIONS
#        define LOCK() \
		{ if (GC_test_and_set(&GC_allocate_lock)) GC_lock(); \
		  SET_LOCK_HOLDER(); }
#        define UNLOCK() \
		{ GC_ASSERT(I_HOLD_LOCK()); UNSET_LOCK_HOLDER(); \
	          GC_clear(&GC_allocate_lock); }
#     else
#        define LOCK() \
		{ if (GC_test_and_set(&GC_allocate_lock)) GC_lock(); }
#        define UNLOCK() \
		GC_clear(&GC_allocate_lock)
#     endif /* !GC_ASSERTIONS */
#     if 0
	/* Another alternative for OSF1 might be:		*/
#       include <sys/mman.h>
        extern msemaphore GC_allocate_semaphore;
#       define LOCK() { if (msem_lock(&GC_allocate_semaphore, MSEM_IF_NOWAIT) \
 			    != 0) GC_lock(); else GC_allocate_lock = 1; }
        /* The following is INCORRECT, since the memory model is too weak. */
	/* Is this true?  Presumably msem_unlock has the right semantics?  */
	/*		- HB						   */
#       define UNLOCK() { GC_allocate_lock = 0; \
                          msem_unlock(&GC_allocate_semaphore, 0); }
#     endif /* 0 */
#    else /* THREAD_LOCAL_ALLOC  || USE_PTHREAD_LOCKS */
#      ifndef USE_PTHREAD_LOCKS
#        define USE_PTHREAD_LOCKS
#      endif
#    endif /* THREAD_LOCAL_ALLOC */
#   ifdef USE_PTHREAD_LOCKS
#      include <pthread.h>
       extern pthread_mutex_t GC_allocate_ml;
#      ifdef GC_ASSERTIONS
#        define LOCK() \
		{ GC_lock(); \
		  SET_LOCK_HOLDER(); }
#        define UNLOCK() \
		{ GC_ASSERT(I_HOLD_LOCK()); UNSET_LOCK_HOLDER(); \
	          pthread_mutex_unlock(&GC_allocate_ml); }
#      else /* !GC_ASSERTIONS */
#        if defined(NO_PTHREAD_TRYLOCK)
#          define LOCK() GC_lock();
#        else /* !defined(NO_PTHREAD_TRYLOCK) */
#        define LOCK() \
	   { if (0 != pthread_mutex_trylock(&GC_allocate_ml)) GC_lock(); }
#        endif
#        define UNLOCK() pthread_mutex_unlock(&GC_allocate_ml)
#      endif /* !GC_ASSERTIONS */
#   endif /* USE_PTHREAD_LOCKS */
#   define SET_LOCK_HOLDER() GC_lock_holder = pthread_self()
#   define UNSET_LOCK_HOLDER() GC_lock_holder = NO_THREAD
#   define I_HOLD_LOCK() (pthread_equal(GC_lock_holder, pthread_self()))
    extern VOLATILE GC_bool GC_collecting;
#   define ENTER_GC() GC_collecting = 1;
#   define EXIT_GC() GC_collecting = 0;
    extern void GC_lock(void);
    extern pthread_t GC_lock_holder;
#   ifdef GC_ASSERTIONS
      extern pthread_t GC_mark_lock_holder;
#   endif
#  endif /* GC_PTHREADS with linux_threads.c implementation */
#  if defined(GC_IRIX_THREADS)
#    include <pthread.h>
     /* This probably should never be included, but I can't test	*/
     /* on Irix anymore.						*/
#    include <mutex.h>

     extern volatile unsigned int GC_allocate_lock;
	/* This is not a mutex because mutexes that obey the (optional) 	*/
	/* POSIX scheduling rules are subject to convoys in high contention	*/
	/* applications.  This is basically a spin lock.			*/
     extern pthread_t GC_lock_holder;
     extern void GC_lock(void);
	/* Allocation lock holder.  Only set if acquired by client through */
	/* GC_call_with_alloc_lock.					   */
#    define SET_LOCK_HOLDER() GC_lock_holder = pthread_self()
#    define NO_THREAD (pthread_t)(-1)
#    define UNSET_LOCK_HOLDER() GC_lock_holder = NO_THREAD
#    define I_HOLD_LOCK() (pthread_equal(GC_lock_holder, pthread_self()))
#    define LOCK() { if (GC_test_and_set(&GC_allocate_lock)) GC_lock(); }
#    define UNLOCK() GC_clear(&GC_allocate_lock);
     extern VOLATILE GC_bool GC_collecting;
#    define ENTER_GC() \
		{ \
		    GC_collecting = 1; \
		}
#    define EXIT_GC() GC_collecting = 0;
#  endif /* GC_IRIX_THREADS */
#  if defined(GC_WIN32_THREADS)
#    if defined(GC_PTHREADS)
#      include <pthread.h>
       extern pthread_mutex_t GC_allocate_ml;
#      define LOCK()   pthread_mutex_lock(&GC_allocate_ml)
#      define UNLOCK() pthread_mutex_unlock(&GC_allocate_ml)
#    else
#      include <windows.h>
       GC_API CRITICAL_SECTION GC_allocate_ml;
#      define LOCK() EnterCriticalSection(&GC_allocate_ml);
#      define UNLOCK() LeaveCriticalSection(&GC_allocate_ml);
#    endif
#  endif
#  ifndef SET_LOCK_HOLDER
#      define SET_LOCK_HOLDER()
#      define UNSET_LOCK_HOLDER()
#      define I_HOLD_LOCK() FALSE
		/* Used on platforms were locks can be reacquired,	*/
		/* so it doesn't matter if we lie.			*/
#  endif
# else /* !THREADS */
#    define LOCK()
#    define UNLOCK()
# endif /* !THREADS */
# ifndef SET_LOCK_HOLDER
#   define SET_LOCK_HOLDER()
#   define UNSET_LOCK_HOLDER()
#   define I_HOLD_LOCK() FALSE
		/* Used on platforms were locks can be reacquired,	*/
		/* so it doesn't matter if we lie.			*/
# endif
# ifndef ENTER_GC
#   define ENTER_GC()
#   define EXIT_GC()
# endif

# ifndef DCL_LOCK_STATE
#   define DCL_LOCK_STATE
# endif
# ifndef FASTLOCK
#   define FASTLOCK() LOCK()
#   define FASTLOCK_SUCCEEDED() TRUE
#   define FASTUNLOCK() UNLOCK()
# endif

#endif /* GC_LOCKS_H */
