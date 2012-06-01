/* 
 * Copyright 1988, 1989 Hans-J. Boehm, Alan J. Demers
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1996-1999 by Silicon Graphics.  All rights reserved.
 * Copyright (c) 1999-2001 by Hewlett-Packard Company. All rights reserved.
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
 

# ifndef GC_PRIVATE_H
# define GC_PRIVATE_H

#if defined(mips) && defined(SYSTYPE_BSD) && defined(sony_news)
    /* sony RISC NEWS, NEWSOS 4 */
#   define BSD_TIME
/*    typedef long ptrdiff_t;   -- necessary on some really old systems	*/
#endif

#if defined(mips) && defined(SYSTYPE_BSD43)
    /* MIPS RISCOS 4 */
#   define BSD_TIME
#endif

#ifdef DGUX
#   include <sys/types.h>
#   include <sys/time.h>
#   include <sys/resource.h>
#endif /* DGUX */

#ifdef BSD_TIME
#   include <sys/types.h>
#   include <sys/time.h>
#   include <sys/resource.h>
#endif /* BSD_TIME */

# ifndef _GC_H
#   include "../gc.h"
# endif

# ifndef GC_MARK_H
#   include "../gc_mark.h"
# endif

typedef GC_word word;
typedef GC_signed_word signed_word;

typedef int GC_bool;
# define TRUE 1
# define FALSE 0

typedef char * ptr_t;	/* A generic pointer to which we can add	*/
			/* byte displacements.				*/
			/* Preferably identical to caddr_t, if it 	*/
			/* exists.					*/
			
# ifndef GCCONFIG_H
#   include "gcconfig.h"
# endif

# ifndef HEADERS_H
#   include "gc_hdrs.h"
# endif

#if defined(__STDC__)
#   include <stdlib.h>
#   if !(defined( sony_news ) )
#       include <stddef.h>
#   endif
#   define VOLATILE volatile
#else
#   ifdef MSWIN32
#   	include <stdlib.h>
#   endif
#   define VOLATILE
#endif

#if defined(__GNUC__) && (__GNUC__ > 2) && defined(__OPTIMIZE__)
/* This doesn't work in some earlier gcc versions */
# define EXPECT(expr, outcome) __builtin_expect(expr,outcome)
  /* Equivalent to (expr), but predict that usually (expr)==outcome. */
#else
# define EXPECT(expr, outcome) (expr)
#endif

# ifndef GC_LOCKS_H
#   include "gc_locks.h"
# endif

# ifdef STACK_GROWS_DOWN
#   define COOLER_THAN >
#   define HOTTER_THAN <
#   define MAKE_COOLER(x,y) if ((word)(x)+(y) > (word)(x)) {(x) += (y);} \
			    else {(x) = (word)ONES;}
#   define MAKE_HOTTER(x,y) (x) -= (y)
# else
#   define COOLER_THAN <
#   define HOTTER_THAN >
#   define MAKE_COOLER(x,y) if ((word)(x)-(y) < (word)(x)) {(x) -= (y);} else {(x) = 0;}
#   define MAKE_HOTTER(x,y) (x) += (y)
# endif

#if defined(AMIGA) && defined(__SASC)
#   define GC_FAR __far
#else
#   define GC_FAR
#endif


/*********************************/
/*                               */
/* Definitions for conservative  */
/* collector                     */
/*                               */
/*********************************/

/*********************************/
/*                               */
/* Easily changeable parameters  */
/*                               */
/*********************************/

/* #define STUBBORN_ALLOC */
		    /* Enable stubborm allocation, and thus a limited	*/
		    /* form of incremental collection w/o dirty bits.	*/

/* #define ALL_INTERIOR_POINTERS */
		    /* Forces all pointers into the interior of an 	*/
		    /* object to be considered valid.  Also causes the	*/
		    /* sizes of all objects to be inflated by at least 	*/
		    /* one byte.  This should suffice to guarantee	*/
		    /* that in the presence of a compiler that does	*/
		    /* not perform garbage-collector-unsafe		*/
		    /* optimizations, all portable, strictly ANSI	*/
		    /* conforming C programs should be safely usable	*/
		    /* with malloc replaced by GC_malloc and free	*/
		    /* calls removed.  There are several disadvantages: */
		    /* 1. There are probably no interesting, portable,	*/
		    /*    strictly ANSI	conforming C programs.		*/
		    /* 2. This option makes it hard for the collector	*/
		    /*    to allocate space that is not ``pointed to''  */
		    /*    by integers, etc.  Under SunOS 4.X with a 	*/
		    /*    statically linked libc, we empiricaly		*/
		    /*    observed that it would be difficult to 	*/
		    /*	  allocate individual objects larger than 100K.	*/
		    /* 	  Even if only smaller objects are allocated,	*/
		    /*    more swap space is likely to be needed.       */
		    /*    Fortunately, much of this will never be	*/
		    /*    touched.					*/
		    /* If you can easily avoid using this option, do.	*/
		    /* If not, try to keep individual objects small.	*/
		    /* This is now really controlled at startup,	*/
		    /* through GC_all_interior_pointers.		*/
		    
#define PRINTSTATS  /* Print garbage collection statistics          	*/
		    /* For less verbose output, undefine in reclaim.c 	*/

#define PRINTTIMES  /* Print the amount of time consumed by each garbage   */
		    /* collection.                                         */

#define PRINTBLOCKS /* Print object sizes associated with heap blocks,     */
		    /* whether the objects are atomic or composite, and    */
		    /* whether or not the block was found to be empty      */
		    /* during the reclaim phase.  Typically generates       */
		    /* about one screenful per garbage collection.         */
#undef PRINTBLOCKS

#ifdef SILENT
#  ifdef PRINTSTATS
#    undef PRINTSTATS
#  endif
#  ifdef PRINTTIMES
#    undef PRINTTIMES
#  endif
#  ifdef PRINTNBLOCKS
#    undef PRINTNBLOCKS
#  endif
#endif

#if defined(PRINTSTATS) && !defined(GATHERSTATS)
#   define GATHERSTATS
#endif

#if defined(PRINTSTATS) || !defined(SMALL_CONFIG)
#   define CONDPRINT  /* Print some things if GC_print_stats is set */
#endif

#define GC_INVOKE_FINALIZERS() GC_notify_or_invoke_finalizers()

#define MERGE_SIZES /* Round up some object sizes, so that fewer distinct */
		    /* free lists are actually maintained.  This applies  */
		    /* only to the top level routines in misc.c, not to   */
		    /* user generated code that calls GC_allocobj and     */
		    /* GC_allocaobj directly.                             */
		    /* Slows down average programs slightly.  May however */
		    /* substantially reduce fragmentation if allocation   */
		    /* request sizes are widely scattered.                */
		    /* May save significant amounts of space for obj_map  */
		    /* entries.						  */

#if defined(USE_MARK_BYTES) && !defined(ALIGN_DOUBLE)
#  define ALIGN_DOUBLE
   /* We use one byte for every 2 words, which doesn't allow for	*/
   /* odd numbered words to have mark bits.				*/
#endif

#if defined(GC_GCJ_SUPPORT) && ALIGNMENT < 8 && !defined(ALIGN_DOUBLE)
   /* GCJ's Hashtable synchronization code requires 64-bit alignment.  */
#  define ALIGN_DOUBLE
#endif

/* ALIGN_DOUBLE requires MERGE_SIZES at present. */
# if defined(ALIGN_DOUBLE) && !defined(MERGE_SIZES)
#   define MERGE_SIZES
# endif

#if !defined(DONT_ADD_BYTE_AT_END)
# define EXTRA_BYTES GC_all_interior_pointers
#else
# define EXTRA_BYTES 0
#endif


# ifndef LARGE_CONFIG
#   define MINHINCR 16	 /* Minimum heap increment, in blocks of HBLKSIZE  */
			 /* Must be multiple of largest page size.	   */
#   define MAXHINCR 2048 /* Maximum heap increment, in blocks              */
# else
#   define MINHINCR 64
#   define MAXHINCR 4096
# endif

# define TIME_LIMIT 50	   /* We try to keep pause times from exceeding	 */
			   /* this by much. In milliseconds.		 */

# define BL_LIMIT GC_black_list_spacing
			   /* If we need a block of N bytes, and we have */
			   /* a block of N + BL_LIMIT bytes available, 	 */
			   /* and N > BL_LIMIT,				 */
			   /* but all possible positions in it are 	 */
			   /* blacklisted, we just use it anyway (and	 */
			   /* print a warning, if warnings are enabled). */
			   /* This risks subsequently leaking the block	 */
			   /* due to a false reference.  But not using	 */
			   /* the block risks unreasonable immediate	 */
			   /* heap growth.				 */

/*********************************/
/*                               */
/* Stack saving for debugging	 */
/*                               */
/*********************************/

#ifdef NEED_CALLINFO
    struct callinfo {
	word ci_pc;  	/* Caller, not callee, pc	*/
#	if NARGS > 0
	    word ci_arg[NARGS];	/* bit-wise complement to avoid retention */
#	endif
#	if defined(ALIGN_DOUBLE) && (NFRAMES * (NARGS + 1)) % 2 == 1
	    /* Likely alignment problem. */
	    word ci_dummy;
#	endif
    };
#endif

#ifdef SAVE_CALL_CHAIN

/* Fill in the pc and argument information for up to NFRAMES of my	*/
/* callers.  Ignore my frame and my callers frame.			*/
void GC_save_callers GC_PROTO((struct callinfo info[NFRAMES]));
  
void GC_print_callers GC_PROTO((struct callinfo info[NFRAMES]));

#endif


/*********************************/
/*                               */
/* OS interface routines	 */
/*                               */
/*********************************/

#ifdef BSD_TIME
#   undef CLOCK_TYPE
#   undef GET_TIME
#   undef MS_TIME_DIFF
#   define CLOCK_TYPE struct timeval
#   define GET_TIME(x) { struct rusage rusage; \
			 getrusage (RUSAGE_SELF,  &rusage); \
			 x = rusage.ru_utime; }
#   define MS_TIME_DIFF(a,b) ((double) (a.tv_sec - b.tv_sec) * 1000.0 \
                               + (double) (a.tv_usec - b.tv_usec) / 1000.0)
#else /* !BSD_TIME */
# if defined(MSWIN32) || defined(MSWINCE)
#   include <windows.h>
#   include <winbase.h>
#   define CLOCK_TYPE DWORD
#   define GET_TIME(x) x = GetTickCount()
#   define MS_TIME_DIFF(a,b) ((long)((a)-(b)))
# else /* !MSWIN32, !MSWINCE, !BSD_TIME */
#   include <time.h>
#   if !defined(__STDC__) && defined(SPARC) && defined(SUNOS4)
      clock_t clock();	/* Not in time.h, where it belongs	*/
#   endif
#   if defined(FREEBSD) && !defined(CLOCKS_PER_SEC)
#     include <machine/limits.h>
#     define CLOCKS_PER_SEC CLK_TCK
#   endif
#   if !defined(CLOCKS_PER_SEC)
#     define CLOCKS_PER_SEC 1000000
/*
 * This is technically a bug in the implementation.  ANSI requires that
 * CLOCKS_PER_SEC be defined.  But at least under SunOS4.1.1, it isn't.
 * Also note that the combination of ANSI C and POSIX is incredibly gross
 * here. The type clock_t is used by both clock() and times().  But on
 * some machines these use different notions of a clock tick,  CLOCKS_PER_SEC
 * seems to apply only to clock.  Hence we use it here.  On many machines,
 * including SunOS, clock actually uses units of microseconds (which are
 * not really clock ticks).
 */
#   endif
#   define CLOCK_TYPE clock_t
#   define GET_TIME(x) x = clock()
#   define MS_TIME_DIFF(a,b) ((unsigned long) \
		(1000.0*(double)((a)-(b))/(double)CLOCKS_PER_SEC))
# endif /* !MSWIN32 */
#endif /* !BSD_TIME */

/* We use bzero and bcopy internally.  They may not be available.	*/
# if defined(SPARC) && defined(SUNOS4)
#   define BCOPY_EXISTS
# endif
# if defined(M68K) && defined(AMIGA)
#   define BCOPY_EXISTS
# endif
# if defined(M68K) && defined(NEXT)
#   define BCOPY_EXISTS
# endif
# if defined(VAX)
#   define BCOPY_EXISTS
# endif
# if defined(AMIGA)
#   include <string.h>
#   define BCOPY_EXISTS
# endif
# if defined(DARWIN)
#   include <string.h>
#   define BCOPY_EXISTS
# endif

# ifndef BCOPY_EXISTS
#   include <string.h>
#   define BCOPY(x,y,n) memcpy(y, x, (size_t)(n))
#   define BZERO(x,n)  memset(x, 0, (size_t)(n))
# else
#   define BCOPY(x,y,n) bcopy((char *)(x),(char *)(y),(int)(n))
#   define BZERO(x,n) bzero((char *)(x),(int)(n))
# endif

#if defined(DARWIN)
#	if defined(POWERPC)
#		define GC_MACH_THREAD_STATE_FLAVOR PPC_THREAD_STATE
#	elif defined(I386)
#		define GC_MACH_THREAD_STATE_FLAVOR i386_THREAD_STATE
#	elif defined(X86_64)
#		define GC_MACH_THREAD_STATE_FLAVOR x86_THREAD_STATE64
#	else
#		define GC_MACH_THREAD_STATE_FLAVOR MACHINE_THREAD_STATE
#	endif
#endif

/* Delay any interrupts or signals that may abort this thread.  Data	*/
/* structures are in a consistent state outside this pair of calls.	*/
/* ANSI C allows both to be empty (though the standard isn't very	*/
/* clear on that point).  Standard malloc implementations are usually	*/
/* neither interruptable nor thread-safe, and thus correspond to	*/
/* empty definitions.							*/
/* It probably doesn't make any sense to declare these to be nonempty	*/
/* if the code is being optimized, since signal safety relies on some	*/
/* ordering constraints that are typically not obeyed by optimizing	*/
/* compilers.								*/
# ifdef PCR
#   define DISABLE_SIGNALS() \
		 PCR_Th_SetSigMask(PCR_allSigsBlocked,&GC_old_sig_mask)
#   define ENABLE_SIGNALS() \
		PCR_Th_SetSigMask(&GC_old_sig_mask, NIL)
# else
#   if defined(THREADS) || defined(AMIGA)  \
	|| defined(MSWIN32) || defined(MSWINCE) || defined(MACOS) \
	|| defined(DJGPP) || defined(NO_SIGNALS) 
			/* Also useful for debugging.		*/
	/* Should probably use thr_sigsetmask for GC_SOLARIS_THREADS. */
#     define DISABLE_SIGNALS()
#     define ENABLE_SIGNALS()
#   else
#     define DISABLE_SIGNALS() GC_disable_signals()
	void GC_disable_signals(void);
#     define ENABLE_SIGNALS() GC_enable_signals()
	void GC_enable_signals(void);
#   endif
# endif

/*
 * Stop and restart mutator threads.
 */
# ifdef PCR
#     include "th/PCR_ThCtl.h"
#     define STOP_WORLD() \
 	PCR_ThCtl_SetExclusiveMode(PCR_ThCtl_ExclusiveMode_stopNormal, \
 				   PCR_allSigsBlocked, \
 				   PCR_waitForever)
#     define START_WORLD() \
	PCR_ThCtl_SetExclusiveMode(PCR_ThCtl_ExclusiveMode_null, \
 				   PCR_allSigsBlocked, \
 				   PCR_waitForever);
# else
#   if defined(GC_SOLARIS_THREADS) || defined(GC_WIN32_THREADS) \
	|| defined(GC_PTHREADS)
      void GC_stop_world(void);
      void GC_start_world(void);
#     define STOP_WORLD() GC_stop_world()
#     define START_WORLD() GC_start_world()
#   else
#     define STOP_WORLD()
#     define START_WORLD()
#   endif
# endif

/* Abandon ship */
# ifdef PCR
#   define ABORT(s) PCR_Base_Panic(s)
# else
#   ifdef SMALL_CONFIG
#	define ABORT(msg) abort();
#   else
	GC_API void GC_abort GC_PROTO((GC_CONST char * msg));
#       define ABORT(msg) GC_abort(msg);
#   endif
# endif

/* Exit abnormally, but without making a mess (e.g. out of memory) */
# ifdef PCR
#   define EXIT() PCR_Base_Exit(1,PCR_waitForever)
# else
#   define EXIT() (void)exit(1)
# endif

/* Print warning message, e.g. almost out of memory.	*/
# define WARN(msg,arg) (*GC_current_warn_proc)("GC Warning: " msg, (GC_word)(arg))
extern GC_warn_proc GC_current_warn_proc;

/* Get environment entry */
#if !defined(NO_GETENV)
#   if defined(EMPTY_GETENV_RESULTS)
	/* Workaround for a reputed Wine bug.	*/
	static inline char * fixed_getenv(const char *name)
	{
	  char * tmp = getenv(name);
	  if (tmp == 0 || strlen(tmp) == 0)
	    return 0;
	  return tmp;
	}
#       define GETENV(name) fixed_getenv(name)
#   else
#       define GETENV(name) getenv(name)
#   endif
#else
#   define GETENV(name) 0
#endif

/*********************************/
/*                               */
/* Word-size-dependent defines   */
/*                               */
/*********************************/

#if CPP_WORDSZ == 32
#  define WORDS_TO_BYTES(x)   ((x)<<2)
#  define BYTES_TO_WORDS(x)   ((x)>>2)
#  define LOGWL               ((word)5)    /* log[2] of CPP_WORDSZ */
#  define modWORDSZ(n) ((n) & 0x1f)        /* n mod size of word	    */
#  if ALIGNMENT != 4
#	define UNALIGNED
#  endif
#endif

#if CPP_WORDSZ == 64
#  define WORDS_TO_BYTES(x)   ((x)<<3)
#  define BYTES_TO_WORDS(x)   ((x)>>3)
#  define LOGWL               ((word)6)    /* log[2] of CPP_WORDSZ */
#  define modWORDSZ(n) ((n) & 0x3f)        /* n mod size of word	    */
#  if ALIGNMENT != 8
#	define UNALIGNED
#  endif
#endif

#define WORDSZ ((word)CPP_WORDSZ)
#define SIGNB  ((word)1 << (WORDSZ-1))
#define BYTES_PER_WORD      ((word)(sizeof (word)))
#define ONES                ((word)(signed_word)(-1))
#define divWORDSZ(n) ((n) >> LOGWL)	   /* divide n by size of word      */

/*********************/
/*                   */
/*  Size Parameters  */
/*                   */
/*********************/

/*  heap block size, bytes. Should be power of 2 */

#ifndef HBLKSIZE
# ifdef SMALL_CONFIG
#   define CPP_LOG_HBLKSIZE 10
# else
#   if (CPP_WORDSZ == 32) || (defined(HPUX) && defined(HP_PA))
      /* HPUX/PA seems to use 4K pages with the 64 bit ABI */
#     define CPP_LOG_HBLKSIZE 12
#   else
#     define CPP_LOG_HBLKSIZE 13
#   endif
# endif
#else
# if HBLKSIZE == 512
#   define CPP_LOG_HBLKSIZE 9
# endif
# if HBLKSIZE == 1024
#   define CPP_LOG_HBLKSIZE 10
# endif
# if HBLKSIZE == 2048
#   define CPP_LOG_HBLKSIZE 11
# endif
# if HBLKSIZE == 4096
#   define CPP_LOG_HBLKSIZE 12
# endif
# if HBLKSIZE == 8192
#   define CPP_LOG_HBLKSIZE 13
# endif
# if HBLKSIZE == 16384
#   define CPP_LOG_HBLKSIZE 14
# endif
# ifndef CPP_LOG_HBLKSIZE
    --> fix HBLKSIZE
# endif
# undef HBLKSIZE
#endif
# define CPP_HBLKSIZE (1 << CPP_LOG_HBLKSIZE)
# define LOG_HBLKSIZE   ((word)CPP_LOG_HBLKSIZE)
# define HBLKSIZE ((word)CPP_HBLKSIZE)


/*  max size objects supported by freelist (larger objects may be   */
/*  allocated, but less efficiently)                                */

#define CPP_MAXOBJBYTES (CPP_HBLKSIZE/2)
#define MAXOBJBYTES ((word)CPP_MAXOBJBYTES)
#define CPP_MAXOBJSZ    BYTES_TO_WORDS(CPP_MAXOBJBYTES)
#define MAXOBJSZ ((word)CPP_MAXOBJSZ)
		
# define divHBLKSZ(n) ((n) >> LOG_HBLKSIZE)

# define HBLK_PTR_DIFF(p,q) divHBLKSZ((ptr_t)p - (ptr_t)q)
	/* Equivalent to subtracting 2 hblk pointers.	*/
	/* We do it this way because a compiler should	*/
	/* find it hard to use an integer division	*/
	/* instead of a shift.  The bundled SunOS 4.1	*/
	/* o.w. sometimes pessimizes the subtraction to	*/
	/* involve a call to .div.			*/
 
# define modHBLKSZ(n) ((n) & (HBLKSIZE-1))
 
# define HBLKPTR(objptr) ((struct hblk *)(((word) (objptr)) & ~(HBLKSIZE-1)))

# define HBLKDISPL(objptr) (((word) (objptr)) & (HBLKSIZE-1))

/* Round up byte allocation requests to integral number of words, etc. */
# define ROUNDED_UP_WORDS(n) \
	BYTES_TO_WORDS((n) + (WORDS_TO_BYTES(1) - 1 + EXTRA_BYTES))
# ifdef ALIGN_DOUBLE
#       define ALIGNED_WORDS(n) \
	    (BYTES_TO_WORDS((n) + WORDS_TO_BYTES(2) - 1 + EXTRA_BYTES) & ~1)
# else
#       define ALIGNED_WORDS(n) ROUNDED_UP_WORDS(n)
# endif
# define SMALL_OBJ(bytes) ((bytes) <= (MAXOBJBYTES - EXTRA_BYTES))
# define ADD_SLOP(bytes) ((bytes) + EXTRA_BYTES)
# ifndef MIN_WORDS
    /* MIN_WORDS is the size of the smallest allocated object.	*/
    /* 1 and 2 are the only valid values.			*/
    /* 2 must be used if:					*/
    /* - GC_gcj_malloc can be used for objects of requested 	*/
    /*   size  smaller than 2 words, or				*/
    /* - USE_MARK_BYTES is defined.				*/
#   if defined(USE_MARK_BYTES) || defined(GC_GCJ_SUPPORT)
#     define MIN_WORDS 2   	/* Smallest allocated object.	*/
#   else
#     define MIN_WORDS 1
#   endif
# endif


/*
 * Hash table representation of sets of pages.  This assumes it is
 * OK to add spurious entries to sets.
 * Used by black-listing code, and perhaps by dirty bit maintenance code.
 */
 
# ifdef LARGE_CONFIG
#   define LOG_PHT_ENTRIES  20  /* Collisions likely at 1M blocks,	*/
				/* which is >= 4GB.  Each table takes	*/
				/* 128KB, some of which may never be	*/
				/* touched.				*/
# else
#   ifdef SMALL_CONFIG
#     define LOG_PHT_ENTRIES  14 /* Collisions are likely if heap grows	*/
				 /* to more than 16K hblks = 64MB.	*/
				 /* Each hash table occupies 2K bytes.   */
#   else /* default "medium" configuration */
#     define LOG_PHT_ENTRIES  16 /* Collisions are likely if heap grows	*/
				 /* to more than 64K hblks >= 256MB.	*/
				 /* Each hash table occupies 8K bytes.  */
				 /* Even for somewhat smaller heaps, 	*/
				 /* say half that, collisions may be an	*/
				 /* issue because we blacklist 		*/
				 /* addresses outside the heap.		*/
#   endif
# endif
# define PHT_ENTRIES ((word)1 << LOG_PHT_ENTRIES)
# define PHT_SIZE (PHT_ENTRIES >> LOGWL)
typedef word page_hash_table[PHT_SIZE];

# define PHT_HASH(addr) ((((word)(addr)) >> LOG_HBLKSIZE) & (PHT_ENTRIES - 1))

# define get_pht_entry_from_index(bl, index) \
		(((bl)[divWORDSZ(index)] >> modWORDSZ(index)) & 1)
# define set_pht_entry_from_index(bl, index) \
		(bl)[divWORDSZ(index)] |= (word)1 << modWORDSZ(index)
# define clear_pht_entry_from_index(bl, index) \
		(bl)[divWORDSZ(index)] &= ~((word)1 << modWORDSZ(index))
/* And a dumb but thread-safe version of set_pht_entry_from_index.	*/
/* This sets (many) extra bits.						*/
# define set_pht_entry_from_index_safe(bl, index) \
		(bl)[divWORDSZ(index)] = ONES
	


/********************************************/
/*                                          */
/*    H e a p   B l o c k s                 */
/*                                          */
/********************************************/

/*  heap block header */
#define HBLKMASK   (HBLKSIZE-1)

#define BITS_PER_HBLK (CPP_HBLKSIZE * 8)

#define MARK_BITS_PER_HBLK (BITS_PER_HBLK/CPP_WORDSZ)
	   /* upper bound                                    */
	   /* We allocate 1 bit/word, unless USE_MARK_BYTES  */
	   /* is defined.  Only the first word   	     */
	   /* in each object is actually marked.             */

# ifdef USE_MARK_BYTES
#   define MARK_BITS_SZ (MARK_BITS_PER_HBLK/2)
	/* Unlike the other case, this is in units of bytes.		*/
	/* We actually allocate only every second mark bit, since we	*/
	/* force all objects to be doubleword aligned.			*/
	/* However, each mark bit is allocated as a byte.		*/
# else
#   define MARK_BITS_SZ (MARK_BITS_PER_HBLK/CPP_WORDSZ)
# endif

/* We maintain layout maps for heap blocks containing objects of a given */
/* size.  Each entry in this map describes a byte offset and has the	 */
/* following type.							 */
typedef unsigned char map_entry_type;

struct hblkhdr {
    word hb_sz;  /* If in use, size in words, of objects in the block. */
		 /* if free, the size in bytes of the whole block      */
    struct hblk * hb_next; 	/* Link field for hblk free list	 */
    				/* and for lists of chunks waiting to be */
    				/* reclaimed.				 */
    struct hblk * hb_prev;	/* Backwards link for free list.	*/
    word hb_descr;   		/* object descriptor for marking.  See	*/
    				/* mark.h.				*/
    map_entry_type * hb_map;	
    			/* A pointer to a pointer validity map of the block. */
    		      	/* See GC_obj_map.				     */
    		     	/* Valid for all blocks with headers.		     */
    		     	/* Free blocks point to GC_invalid_map.		     */
    unsigned char hb_obj_kind;
    			 /* Kind of objects in the block.  Each kind 	*/
    			 /* identifies a mark procedure and a set of 	*/
    			 /* list headers.  Sometimes called regions.	*/
    unsigned char hb_flags;
#	define IGNORE_OFF_PAGE	1	/* Ignore pointers that do not	*/
					/* point to the first page of 	*/
					/* this object.			*/
#	define WAS_UNMAPPED 2	/* This is a free block, which has	*/
				/* been unmapped from the address 	*/
				/* space.				*/
				/* GC_remap must be invoked on it	*/
				/* before it can be reallocated.	*/
				/* Only set with USE_MUNMAP.		*/
    unsigned short hb_last_reclaimed;
    				/* Value of GC_gc_no when block was	*/
    				/* last allocated or swept. May wrap.   */
				/* For a free block, this is maintained */
				/* only for USE_MUNMAP, and indicates	*/
				/* when the header was allocated, or	*/
				/* when the size of the block last	*/
				/* changed.				*/
#   ifdef USE_MARK_BYTES
      union {
        char _hb_marks[MARK_BITS_SZ];
			    /* The i'th byte is 1 if the object 	*/
			    /* starting at word 2i is marked, 0 o.w.	*/
	word dummy;	/* Force word alignment of mark bytes. */
      } _mark_byte_union;
#     define hb_marks _mark_byte_union._hb_marks
#   else
      word hb_marks[MARK_BITS_SZ];
			    /* Bit i in the array refers to the             */
			    /* object starting at the ith word (header      */
			    /* INCLUDED) in the heap block.                 */
			    /* The lsb of word 0 is numbered 0.		    */
			    /* Unused bits are invalid, and are 	    */
			    /* occasionally set, e.g for uncollectable	    */
			    /* objects.					    */
#   endif /* !USE_MARK_BYTES */
};

/*  heap block body */

# define BODY_SZ (HBLKSIZE/sizeof(word))

struct hblk {
    word hb_body[BODY_SZ];
};

# define HBLK_IS_FREE(hdr) ((hdr) -> hb_map == GC_invalid_map)

# define OBJ_SZ_TO_BLOCKS(sz) \
    divHBLKSZ(WORDS_TO_BYTES(sz) + HBLKSIZE-1)
    /* Size of block (in units of HBLKSIZE) needed to hold objects of	*/
    /* given sz (in words).						*/

/* Object free list link */
# define obj_link(p) (*(ptr_t *)(p))

# define LOG_MAX_MARK_PROCS 6
# define MAX_MARK_PROCS (1 << LOG_MAX_MARK_PROCS)

/* Root sets.  Logically private to mark_rts.c.  But we don't want the	*/
/* tables scanned, so we put them here.					*/
/* MAX_ROOT_SETS is the maximum number of ranges that can be 	*/
/* registered as static roots. 					*/
# ifdef LARGE_CONFIG
#   define MAX_ROOT_SETS 4096
# else
#   ifdef PCR
#     define MAX_ROOT_SETS 1024
#   else
#     if defined(MSWIN32) || defined(MSWINCE)
#	define MAX_ROOT_SETS 1024
	    /* Under NT, we add only written pages, which can result 	*/
	    /* in many small root sets.					*/
#     else
#       define MAX_ROOT_SETS 1024
#     endif
#   endif
# endif

# define MAX_EXCLUSIONS (MAX_ROOT_SETS/4)
/* Maximum number of segments that can be excluded from root sets.	*/

/*
 * Data structure for excluded static roots.
 */
struct exclusion {
    ptr_t e_start;
    ptr_t e_end;
};

/* Data structure for list of root sets.				*/
/* We keep a hash table, so that we can filter out duplicate additions.	*/
/* Under Win32, we need to do a better job of filtering overlaps, so	*/
/* we resort to sequential search, and pay the price.			*/
struct roots {
	ptr_t r_start;
	ptr_t r_end;
#	if !defined(MSWIN32) && !defined(MSWINCE)
	  struct roots * r_next;
#	endif
	GC_bool r_tmp;
	  	/* Delete before registering new dynamic libraries */
};

#if !defined(MSWIN32) && !defined(MSWINCE)
    /* Size of hash table index to roots.	*/
#   define LOG_RT_SIZE 6
#   define RT_SIZE (1 << LOG_RT_SIZE) /* Power of 2, may be != MAX_ROOT_SETS */
#endif

/* Lists of all heap blocks and free lists	*/
/* as well as other random data structures	*/
/* that should not be scanned by the		*/
/* collector.					*/
/* These are grouped together in a struct	*/
/* so that they can be easily skipped by the	*/
/* GC_mark routine.				*/
/* The ordering is weird to make GC_malloc	*/
/* faster by keeping the important fields	*/
/* sufficiently close together that a		*/
/* single load of a base register will do.	*/
/* Scalars that could easily appear to		*/
/* be pointers are also put here.		*/
/* The main fields should precede any 		*/
/* conditionally included fields, so that	*/
/* gc_inl.h will work even if a different set	*/
/* of macros is defined when the client is	*/
/* compiled.					*/

struct _GC_arrays {
  word _heapsize;
  word _max_heapsize;
  word _requested_heapsize;	/* Heap size due to explicit expansion */
  ptr_t _last_heap_addr;
  ptr_t _prev_heap_addr;
  word _large_free_bytes;
	/* Total bytes contained in blocks on large object free */
	/* list.						*/
  word _large_allocd_bytes;
  	/* Total number of bytes in allocated large objects blocks.	*/
  	/* For the purposes of this counter and the next one only, a 	*/
  	/* large object is one that occupies a block of at least	*/
  	/* 2*HBLKSIZE.							*/
  word _max_large_allocd_bytes;
  	/* Maximum number of bytes that were ever allocated in		*/
  	/* large object blocks.  This is used to help decide when it	*/
  	/* is safe to split up a large block.				*/
  word _words_allocd_before_gc;
		/* Number of words allocated before this	*/
		/* collection cycle.				*/
# ifndef SEPARATE_GLOBALS
    word _words_allocd;
  	/* Number of words allocated during this collection cycle */
# endif
  word _words_wasted;
  	/* Number of words wasted due to internal fragmentation	*/
  	/* in large objects, or due to dropping blacklisted     */
	/* blocks, since last gc.  Approximate.                 */
  word _words_finalized;
  	/* Approximate number of words in objects (and headers)	*/
  	/* That became ready for finalization in the last 	*/
  	/* collection.						*/
  word _non_gc_bytes_at_gc;
  	/* Number of explicitly managed bytes of storage 	*/
  	/* at last collection.					*/
  word _mem_freed;
  	/* Number of explicitly deallocated words of memory	*/
  	/* since last collection.				*/
  word _finalizer_mem_freed;
  	/* Words of memory explicitly deallocated while 	*/
  	/* finalizers were running.  Used to approximate mem.	*/
  	/* explicitly deallocated by finalizers.		*/
  ptr_t _scratch_end_ptr;
  ptr_t _scratch_last_end_ptr;
	/* Used by headers.c, and can easily appear to point to	*/
	/* heap.						*/
  GC_mark_proc _mark_procs[MAX_MARK_PROCS];
  	/* Table of user-defined mark procedures.  There is	*/
	/* a small number of these, which can be referenced	*/
	/* by DS_PROC mark descriptors.  See gc_mark.h.		*/

# ifndef SEPARATE_GLOBALS
    ptr_t _objfreelist[MAXOBJSZ+1];
			  /* free list for objects */
    ptr_t _aobjfreelist[MAXOBJSZ+1];
			  /* free list for atomic objs 	*/
# endif

  ptr_t _uobjfreelist[MAXOBJSZ+1];
			  /* uncollectable but traced objs 	*/
			  /* objects on this and auobjfreelist  */
			  /* are always marked, except during   */
			  /* garbage collections.		*/
# ifdef ATOMIC_UNCOLLECTABLE
    ptr_t _auobjfreelist[MAXOBJSZ+1];
# endif
			  /* uncollectable but traced objs 	*/

# ifdef GATHERSTATS
    word _composite_in_use;
   		/* Number of words in accessible composite	*/
		/* objects.					*/
    word _atomic_in_use;
   		/* Number of words in accessible atomic		*/
		/* objects.					*/
# endif
# ifdef USE_MUNMAP
    word _unmapped_bytes;
# endif
# ifdef MERGE_SIZES
    unsigned _size_map[WORDS_TO_BYTES(MAXOBJSZ+1)];
    	/* Number of words to allocate for a given allocation request in */
    	/* bytes.							 */
# endif 

# ifdef STUBBORN_ALLOC
    ptr_t _sobjfreelist[MAXOBJSZ+1];
# endif
  			  /* free list for immutable objects	*/
  map_entry_type * _obj_map[MAXOBJSZ+1];
                       /* If not NIL, then a pointer to a map of valid  */
    		       /* object addresses. _obj_map[sz][i] is j if the	*/
    		       /* address block_start+i is a valid pointer      */
    		       /* to an object at block_start +			*/
 		       /* WORDS_TO_BYTES(BYTES_TO_WORDS(i) - j)		*/
  		       /* I.e. j is a word displacement from the	*/
  		       /* object beginning.				*/
  		       /* The entry is OBJ_INVALID if the corresponding	*/
  		       /* address is not a valid pointer.  It is 	*/
  		       /* OFFSET_TOO_BIG if the value j would be too 	*/
  		       /* large to fit in the entry.  (Note that the	*/
  		       /* size of these entries matters, both for 	*/
  		       /* space consumption and for cache utilization.)	*/
#   define OFFSET_TOO_BIG 0xfe
#   define OBJ_INVALID 0xff
#   define MAP_ENTRY(map, bytes) (map)[bytes]
#   define MAP_ENTRIES HBLKSIZE
#   define MAP_SIZE MAP_ENTRIES
#   define CPP_MAX_OFFSET (OFFSET_TOO_BIG - 1)	
#   define MAX_OFFSET ((word)CPP_MAX_OFFSET)
#   define INIT_MAP(map) memset((map), OBJ_INVALID, MAP_SIZE)
    /* The following are used only if GC_all_interior_ptrs != 0 */
# 	define VALID_OFFSET_SZ \
	  (CPP_MAX_OFFSET > WORDS_TO_BYTES(CPP_MAXOBJSZ)? \
	   CPP_MAX_OFFSET+1 \
	   : WORDS_TO_BYTES(CPP_MAXOBJSZ)+1)
  	char _valid_offsets[VALID_OFFSET_SZ];
				/* GC_valid_offsets[i] == TRUE ==> i 	*/
				/* is registered as a displacement.	*/
  	char _modws_valid_offsets[sizeof(word)];
				/* GC_valid_offsets[i] ==>		  */
				/* GC_modws_valid_offsets[i%sizeof(word)] */
#   define OFFSET_VALID(displ) \
	  (GC_all_interior_pointers || GC_valid_offsets[displ])
# ifdef STUBBORN_ALLOC
    page_hash_table _changed_pages;
        /* Stubborn object pages that were changes since last call to	*/
	/* GC_read_changed.						*/
    page_hash_table _prev_changed_pages;
        /* Stubborn object pages that were changes before last call to	*/
	/* GC_read_changed.						*/
# endif
# if defined(PROC_VDB) || defined(MPROTECT_VDB)
    page_hash_table _grungy_pages; /* Pages that were dirty at last 	   */
				     /* GC_read_dirty.			   */
# endif
# ifdef MPROTECT_VDB
    VOLATILE page_hash_table _dirty_pages;	
			/* Pages dirtied since last GC_read_dirty. */
# endif
# ifdef PROC_VDB
    page_hash_table _written_pages;	/* Pages ever dirtied	*/
# endif
# ifdef LARGE_CONFIG
#   if CPP_WORDSZ > 32
#     define MAX_HEAP_SECTS 4096 	/* overflows at roughly 64 GB	   */
#   else
#     define MAX_HEAP_SECTS 768		/* Separately added heap sections. */
#   endif
# else
#   ifdef SMALL_CONFIG
#     define MAX_HEAP_SECTS 128		/* Roughly 256MB (128*2048*1K)	*/
#   else
#     define MAX_HEAP_SECTS (384+128)		/* Roughly 4GB			*/
#   endif
# endif
  struct HeapSect {
      ptr_t hs_start; word hs_bytes;
  } _heap_sects[MAX_HEAP_SECTS];
# if defined(MSWIN32) || defined(MSWINCE)
    ptr_t _heap_bases[MAX_HEAP_SECTS];
    		/* Start address of memory regions obtained from kernel. */
# endif
# ifdef MSWINCE
    word _heap_lengths[MAX_HEAP_SECTS];
    		/* Commited lengths of memory regions obtained from kernel. */
# endif
  struct roots _static_roots[MAX_ROOT_SETS];
# if !defined(MSWIN32) && !defined(MSWINCE)
    struct roots * _root_index[RT_SIZE];
# endif
  struct exclusion _excl_table[MAX_EXCLUSIONS];
  /* Block header index; see gc_headers.h */
  bottom_index * _all_nils;
  bottom_index * _top_index [TOP_SZ];
#ifdef SAVE_CALL_CHAIN
  struct callinfo _last_stack[NFRAMES];	/* Stack at last garbage collection.*/
  					/* Useful for debugging	mysterious  */
  					/* object disappearances.	    */
  					/* In the multithreaded case, we    */
  					/* currently only save the calling  */
  					/* stack.			    */
#endif
};

GC_API GC_FAR struct _GC_arrays GC_arrays; 

# ifndef SEPARATE_GLOBALS
#   define GC_objfreelist GC_arrays._objfreelist
#   define GC_aobjfreelist GC_arrays._aobjfreelist
#   define GC_words_allocd GC_arrays._words_allocd
# endif
# define GC_uobjfreelist GC_arrays._uobjfreelist
# ifdef ATOMIC_UNCOLLECTABLE
#   define GC_auobjfreelist GC_arrays._auobjfreelist
# endif
# define GC_sobjfreelist GC_arrays._sobjfreelist
# define GC_valid_offsets GC_arrays._valid_offsets
# define GC_modws_valid_offsets GC_arrays._modws_valid_offsets
# ifdef STUBBORN_ALLOC
#    define GC_changed_pages GC_arrays._changed_pages
#    define GC_prev_changed_pages GC_arrays._prev_changed_pages
# endif
# define GC_obj_map GC_arrays._obj_map
# define GC_last_heap_addr GC_arrays._last_heap_addr
# define GC_prev_heap_addr GC_arrays._prev_heap_addr
# define GC_words_wasted GC_arrays._words_wasted
# define GC_large_free_bytes GC_arrays._large_free_bytes
# define GC_large_allocd_bytes GC_arrays._large_allocd_bytes
# define GC_max_large_allocd_bytes GC_arrays._max_large_allocd_bytes
# define GC_words_finalized GC_arrays._words_finalized
# define GC_non_gc_bytes_at_gc GC_arrays._non_gc_bytes_at_gc
# define GC_mem_freed GC_arrays._mem_freed
# define GC_finalizer_mem_freed GC_arrays._finalizer_mem_freed
# define GC_scratch_end_ptr GC_arrays._scratch_end_ptr
# define GC_scratch_last_end_ptr GC_arrays._scratch_last_end_ptr
# define GC_mark_procs GC_arrays._mark_procs
# define GC_heapsize GC_arrays._heapsize
# define GC_max_heapsize GC_arrays._max_heapsize
# define GC_requested_heapsize GC_arrays._requested_heapsize
# define GC_words_allocd_before_gc GC_arrays._words_allocd_before_gc
# define GC_heap_sects GC_arrays._heap_sects
# define GC_last_stack GC_arrays._last_stack
# ifdef USE_MUNMAP
#   define GC_unmapped_bytes GC_arrays._unmapped_bytes
# endif
# if defined(MSWIN32) || defined(MSWINCE)
#   define GC_heap_bases GC_arrays._heap_bases
# endif
# ifdef MSWINCE
#   define GC_heap_lengths GC_arrays._heap_lengths
# endif
# define GC_static_roots GC_arrays._static_roots
# define GC_root_index GC_arrays._root_index
# define GC_excl_table GC_arrays._excl_table
# define GC_all_nils GC_arrays._all_nils
# define GC_top_index GC_arrays._top_index
# if defined(PROC_VDB) || defined(MPROTECT_VDB)
#   define GC_grungy_pages GC_arrays._grungy_pages
# endif
# ifdef MPROTECT_VDB
#   define GC_dirty_pages GC_arrays._dirty_pages
# endif
# ifdef PROC_VDB
#   define GC_written_pages GC_arrays._written_pages
# endif
# ifdef GATHERSTATS
#   define GC_composite_in_use GC_arrays._composite_in_use
#   define GC_atomic_in_use GC_arrays._atomic_in_use
# endif
# ifdef MERGE_SIZES
#   define GC_size_map GC_arrays._size_map
# endif

# define beginGC_arrays ((ptr_t)(&GC_arrays))
# define endGC_arrays (((ptr_t)(&GC_arrays)) + (sizeof GC_arrays))

#define USED_HEAP_SIZE (GC_heapsize - GC_large_free_bytes)

/* Object kinds: */
# define MAXOBJKINDS 16

extern struct obj_kind {
   ptr_t *ok_freelist;	/* Array of free listheaders for this kind of object */
   			/* Point either to GC_arrays or to storage allocated */
   			/* with GC_scratch_alloc.			     */
   struct hblk **ok_reclaim_list;
   			/* List headers for lists of blocks waiting to be */
   			/* swept.					  */
   word ok_descriptor;  /* Descriptor template for objects in this	*/
   			/* block.					*/
   GC_bool ok_relocate_descr;
   			/* Add object size in bytes to descriptor 	*/
   			/* template to obtain descriptor.  Otherwise	*/
   			/* template is used as is.			*/
   GC_bool ok_init;   /* Clear objects before putting them on the free list. */
} GC_obj_kinds[MAXOBJKINDS];

# define beginGC_obj_kinds ((ptr_t)(&GC_obj_kinds))
# define endGC_obj_kinds (beginGC_obj_kinds + (sizeof GC_obj_kinds))

/* Variables that used to be in GC_arrays, but need to be accessed by 	*/
/* inline allocation code.  If they were in GC_arrays, the inlined 	*/
/* allocation code would include GC_arrays offsets (as it did), which	*/
/* introduce maintenance problems.					*/

#ifdef SEPARATE_GLOBALS
  word GC_words_allocd;
  	/* Number of words allocated during this collection cycle */
  ptr_t GC_objfreelist[MAXOBJSZ+1];
			  /* free list for NORMAL objects */
# define beginGC_objfreelist ((ptr_t)(&GC_objfreelist))
# define endGC_objfreelist (beginGC_objfreelist + sizeof(GC_objfreelist))

  ptr_t GC_aobjfreelist[MAXOBJSZ+1];
			  /* free list for atomic (PTRFREE) objs 	*/
# define beginGC_aobjfreelist ((ptr_t)(&GC_aobjfreelist))
# define endGC_aobjfreelist (beginGC_aobjfreelist + sizeof(GC_aobjfreelist))
#endif

/* Predefined kinds: */
# define PTRFREE 0
# define NORMAL  1
# define UNCOLLECTABLE 2
# ifdef ATOMIC_UNCOLLECTABLE
#   define AUNCOLLECTABLE 3
#   define STUBBORN 4
#   define IS_UNCOLLECTABLE(k) (((k) & ~1) == UNCOLLECTABLE)
# else
#   define STUBBORN 3
#   define IS_UNCOLLECTABLE(k) ((k) == UNCOLLECTABLE)
# endif

extern int GC_n_kinds;

GC_API word GC_fo_entries;

extern word GC_n_heap_sects;	/* Number of separately added heap	*/
				/* sections.				*/

extern word GC_page_size;

# if defined(MSWIN32) || defined(MSWINCE)
  struct _SYSTEM_INFO;
  extern struct _SYSTEM_INFO GC_sysinfo;
  extern word GC_n_heap_bases;	/* See GC_heap_bases.	*/
# endif

extern word GC_total_stack_black_listed;
			/* Number of bytes on stack blacklist. 	*/

extern word GC_black_list_spacing;
			/* Average number of bytes between blacklisted	*/
			/* blocks. Approximate.				*/
			/* Counts only blocks that are 			*/
			/* "stack-blacklisted", i.e. that are 		*/
			/* problematic in the interior of an object.	*/

extern map_entry_type * GC_invalid_map;
			/* Pointer to the nowhere valid hblk map */
			/* Blocks pointing to this map are free. */

extern struct hblk * GC_hblkfreelist[];
				/* List of completely empty heap blocks	*/
				/* Linked through hb_next field of 	*/
				/* header structure associated with	*/
				/* block.				*/

extern GC_bool GC_objects_are_marked;	/* There are marked objects in  */
					/* the heap.			*/

#ifndef SMALL_CONFIG
  extern GC_bool GC_incremental;
			/* Using incremental/generational collection. */
# define TRUE_INCREMENTAL \
	(GC_incremental && GC_time_limit != GC_TIME_UNLIMITED)
	/* True incremental, not just generational, mode */
#else
# define GC_incremental FALSE
			/* Hopefully allow optimizer to remove some code. */
# define TRUE_INCREMENTAL FALSE
#endif

extern GC_bool GC_dirty_maintained;
				/* Dirty bits are being maintained, 	*/
				/* either for incremental collection,	*/
				/* or to limit the root set.		*/

extern word GC_root_size;	/* Total size of registered root sections */

extern GC_bool GC_debugging_started;	/* GC_debug_malloc has been called. */ 

extern long GC_large_alloc_warn_interval;
	/* Interval between unsuppressed warnings.	*/

extern long GC_large_alloc_warn_suppressed;
	/* Number of warnings suppressed so far.	*/

#ifdef THREADS
  extern GC_bool GC_world_stopped;
#endif

/* Operations */
# ifndef abs
#   define abs(x)  ((x) < 0? (-(x)) : (x))
# endif


/*  Marks are in a reserved area in                          */
/*  each heap block.  Each word has one mark bit associated  */
/*  with it. Only those corresponding to the beginning of an */
/*  object are used.                                         */

/* Set mark bit correctly, even if mark bits may be concurrently 	*/
/* accessed.								*/
#ifdef PARALLEL_MARK
# define OR_WORD(addr, bits) \
	{ word old; \
	  do { \
	    old = *((volatile word *)addr); \
	  } while (!GC_compare_and_exchange((addr), old, old | (bits))); \
	}
# define OR_WORD_EXIT_IF_SET(addr, bits, exit_label) \
	{ word old; \
	  word my_bits = (bits); \
	  do { \
	    old = *((volatile word *)addr); \
	    if (old & my_bits) goto exit_label; \
	  } while (!GC_compare_and_exchange((addr), old, old | my_bits)); \
	}
#else
# define OR_WORD(addr, bits) *(addr) |= (bits)
# define OR_WORD_EXIT_IF_SET(addr, bits, exit_label) \
	{ \
	  word old = *(addr); \
	  word my_bits = (bits); \
	  if (old & my_bits) goto exit_label; \
	  *(addr) = (old | my_bits); \
	}
#endif

/* Mark bit operations */

/*
 * Retrieve, set, clear the mark bit corresponding
 * to the nth word in a given heap block.
 *
 * (Recall that bit n corresponds to object beginning at word n
 * relative to the beginning of the block, including unused words)
 */

#ifdef USE_MARK_BYTES
# define mark_bit_from_hdr(hhdr,n) ((hhdr)->hb_marks[(n) >> 1])
# define set_mark_bit_from_hdr(hhdr,n) ((hhdr)->hb_marks[(n)>>1]) = 1
# define clear_mark_bit_from_hdr(hhdr,n) ((hhdr)->hb_marks[(n)>>1]) = 0
#else /* !USE_MARK_BYTES */
# define mark_bit_from_hdr(hhdr,n) (((hhdr)->hb_marks[divWORDSZ(n)] \
			    >> (modWORDSZ(n))) & (word)1)
# define set_mark_bit_from_hdr(hhdr,n) \
			    OR_WORD((hhdr)->hb_marks+divWORDSZ(n), \
				    (word)1 << modWORDSZ(n))
# define clear_mark_bit_from_hdr(hhdr,n) (hhdr)->hb_marks[divWORDSZ(n)] \
				&= ~((word)1 << modWORDSZ(n))
#endif /* !USE_MARK_BYTES */

/* Important internal collector routines */

ptr_t GC_approx_sp GC_PROTO((void));
  
GC_bool GC_should_collect GC_PROTO((void));
  
void GC_apply_to_all_blocks GC_PROTO(( \
    void (*fn) GC_PROTO((struct hblk *h, word client_data)), \
    word client_data));
  			/* Invoke fn(hbp, client_data) for each 	*/
  			/* allocated heap block.			*/
struct hblk * GC_next_used_block GC_PROTO((struct hblk * h));
  			/* Return first in-use block >= h	*/
struct hblk * GC_prev_block GC_PROTO((struct hblk * h));
  			/* Return last block <= h.  Returned block	*/
  			/* is managed by GC, but may or may not be in	*/
			/* use.						*/
void GC_mark_init GC_PROTO((void));
void GC_clear_marks GC_PROTO((void));	/* Clear mark bits for all heap objects. */
void GC_invalidate_mark_state GC_PROTO((void));
					/* Tell the marker that	marked 	   */
  					/* objects may point to	unmarked   */
  					/* ones, and roots may point to	   */
  					/* unmarked objects.		   */
  					/* Reset mark stack.		   */
GC_bool GC_mark_stack_empty GC_PROTO((void));
GC_bool GC_mark_some GC_PROTO((ptr_t cold_gc_frame));
  			/* Perform about one pages worth of marking	*/
  			/* work of whatever kind is needed.  Returns	*/
  			/* quickly if no collection is in progress.	*/
  			/* Return TRUE if mark phase finished.		*/
void GC_initiate_gc GC_PROTO((void));
				/* initiate collection.			*/
  				/* If the mark state is invalid, this	*/
  				/* becomes full colleection.  Otherwise */
  				/* it's partial.			*/
void GC_push_all GC_PROTO((ptr_t bottom, ptr_t top));
				/* Push everything in a range 		*/
  				/* onto mark stack.			*/
void GC_push_selected GC_PROTO(( \
    ptr_t bottom, \
    ptr_t top, \
    int (*dirty_fn) GC_PROTO((struct hblk *h)), \
    void (*push_fn) GC_PROTO((ptr_t bottom, ptr_t top)) ));
				  /* Push all pages h in [b,t) s.t. 	*/
				  /* select_fn(h) != 0 onto mark stack. */
#ifndef SMALL_CONFIG
  void GC_push_conditional GC_PROTO((ptr_t b, ptr_t t, GC_bool all));
#else
# define GC_push_conditional(b, t, all) GC_push_all(b, t)
#endif
                                /* Do either of the above, depending	*/
				/* on the third arg.			*/
void GC_push_all_stack GC_PROTO((ptr_t b, ptr_t t));
				    /* As above, but consider		*/
				    /*  interior pointers as valid  	*/
void GC_push_all_eager GC_PROTO((ptr_t b, ptr_t t));
				    /* Same as GC_push_all_stack, but   */
				    /* ensures that stack is scanned	*/
				    /* immediately, not just scheduled  */
				    /* for scanning.			*/
#ifndef THREADS
  void GC_push_all_stack_partially_eager GC_PROTO(( \
      ptr_t bottom, ptr_t top, ptr_t cold_gc_frame ));
			/* Similar to GC_push_all_eager, but only the	*/
			/* part hotter than cold_gc_frame is scanned	*/
			/* immediately.  Needed to ensure that callee-	*/
			/* save registers are not missed.		*/
#else
  /* In the threads case, we push part of the current thread stack	*/
  /* with GC_push_all_eager when we push the registers.  This gets the  */
  /* callee-save registers that may disappear.  The remainder of the	*/
  /* stacks are scheduled for scanning in *GC_push_other_roots, which	*/
  /* is thread-package-specific.					*/
#endif
void GC_push_current_stack GC_PROTO((ptr_t cold_gc_frame));
  			/* Push enough of the current stack eagerly to	*/
  			/* ensure that callee-save registers saved in	*/
  			/* GC frames are scanned.			*/
  			/* In the non-threads case, schedule entire	*/
  			/* stack for scanning.				*/
void GC_push_roots GC_PROTO((GC_bool all, ptr_t cold_gc_frame));
  			/* Push all or dirty roots.	*/
extern void (*GC_push_other_roots) GC_PROTO((void));
  			/* Push system or application specific roots	*/
  			/* onto the mark stack.  In some environments	*/
  			/* (e.g. threads environments) this is		*/
  			/* predfined to be non-zero.  A client supplied */
  			/* replacement should also call the original	*/
  			/* function.					*/
extern void GC_push_gc_structures GC_PROTO((void));
			/* Push GC internal roots.  These are normally	*/
			/* included in the static data segment, and 	*/
			/* Thus implicitly pushed.  But we must do this	*/
			/* explicitly if normal root processing is 	*/
			/* disabled.  Calls the following:		*/
	extern void GC_push_finalizer_structures GC_PROTO((void));
	extern void GC_push_stubborn_structures GC_PROTO((void));
#	ifdef THREADS
	  extern void GC_push_thread_structures GC_PROTO((void));
#	endif
extern void (*GC_start_call_back) GC_PROTO((void));
  			/* Called at start of full collections.		*/
  			/* Not called if 0.  Called with allocation 	*/
  			/* lock held.					*/
  			/* 0 by default.				*/
# if defined(USE_GENERIC_PUSH_REGS)
  void GC_generic_push_regs GC_PROTO((ptr_t cold_gc_frame));
# else
  void GC_push_regs GC_PROTO((void));
# endif
# if defined(SPARC) || defined(IA64)
  /* Cause all stacked registers to be saved in memory.  Return a	*/
  /* pointer to the top of the corresponding memory stack.		*/
  word GC_save_regs_in_stack GC_PROTO((void));
# endif
			/* Push register contents onto mark stack.	*/
  			/* If NURSERY is defined, the default push	*/
  			/* action can be overridden with GC_push_proc	*/

# ifdef NURSERY
    extern void (*GC_push_proc)(ptr_t);
# endif
# if defined(MSWIN32) || defined(MSWINCE)
  void __cdecl GC_push_one GC_PROTO((word p));
# else
  void GC_push_one GC_PROTO((word p));
			      /* If p points to an object, mark it    */
                              /* and push contents on the mark stack  */
  			      /* Pointer recognition test always      */
  			      /* accepts interior pointers, i.e. this */
  			      /* is appropriate for pointers found on */
  			      /* stack.				      */
# endif
# if defined(PRINT_BLACK_LIST) || defined(KEEP_BACK_PTRS)
  void GC_mark_and_push_stack GC_PROTO((word p, ptr_t source));
				/* Ditto, omits plausibility test	*/
# else
  void GC_mark_and_push_stack GC_PROTO((word p));
# endif
void GC_push_marked GC_PROTO((struct hblk * h, hdr * hhdr));
		/* Push contents of all marked objects in h onto	*/
		/* mark stack.						*/
#ifdef SMALL_CONFIG
# define GC_push_next_marked_dirty(h) GC_push_next_marked(h)
#else
  struct hblk * GC_push_next_marked_dirty GC_PROTO((struct hblk * h));
		/* Invoke GC_push_marked on next dirty block above h.	*/
		/* Return a pointer just past the end of this block.	*/
#endif /* !SMALL_CONFIG */
struct hblk * GC_push_next_marked GC_PROTO((struct hblk * h));
  		/* Ditto, but also mark from clean pages.	*/
struct hblk * GC_push_next_marked_uncollectable GC_PROTO((struct hblk * h));
  		/* Ditto, but mark only from uncollectable pages.	*/
GC_bool GC_stopped_mark GC_PROTO((GC_stop_func stop_func));
 			/* Stop world and mark from all roots	*/
  			/* and rescuers.			*/
void GC_clear_hdr_marks GC_PROTO((hdr * hhdr));
				    /* Clear the mark bits in a header */
void GC_set_hdr_marks GC_PROTO((hdr * hhdr));
 				    /* Set the mark bits in a header */
void GC_set_fl_marks GC_PROTO((ptr_t p));
				    /* Set all mark bits associated with */
				    /* a free list.			 */
void GC_add_roots_inner GC_PROTO((char * b, char * e, GC_bool tmp));
void GC_remove_roots_inner GC_PROTO((char * b, char * e));
GC_bool GC_is_static_root GC_PROTO((ptr_t p));
  		/* Is the address p in one of the registered static	*/
  		/* root sections?					*/
# if defined(MSWIN32) || defined(_WIN32_WCE_EMULATION)
GC_bool GC_is_tmp_root GC_PROTO((ptr_t p));
		/* Is the address p in one of the temporary static	*/
		/* root sections?					*/
# endif
void GC_register_dynamic_libraries GC_PROTO((void));
  		/* Add dynamic library data sections to the root set. */

GC_bool GC_register_main_static_data GC_PROTO((void));
		/* We need to register the main data segment.  Returns	*/
		/* TRUE unless this is done implicitly as part of	*/
		/* dynamic library registration.			*/
  
/* Machine dependent startup routines */
ptr_t GC_get_stack_base GC_PROTO((void));	/* Cold end of stack */
#ifdef IA64
  ptr_t GC_get_register_stack_base GC_PROTO((void));
  					/* Cold end of register stack.	*/
#endif
void GC_register_data_segments GC_PROTO((void));
  
/* Black listing: */
void GC_bl_init GC_PROTO((void));
# ifdef PRINT_BLACK_LIST
      void GC_add_to_black_list_normal GC_PROTO((word p, ptr_t source));
			/* Register bits as a possible future false	*/
			/* reference from the heap or static data	*/
#     define GC_ADD_TO_BLACK_LIST_NORMAL(bits, source) \
      		if (GC_all_interior_pointers) { \
		  GC_add_to_black_list_stack(bits, (ptr_t)(source)); \
		} else { \
  		  GC_add_to_black_list_normal(bits, (ptr_t)(source)); \
		}
# else
      void GC_add_to_black_list_normal GC_PROTO((word p));
#     define GC_ADD_TO_BLACK_LIST_NORMAL(bits, source) \
      		if (GC_all_interior_pointers) { \
		  GC_add_to_black_list_stack(bits); \
		} else { \
  		  GC_add_to_black_list_normal(bits); \
		}
# endif

# ifdef PRINT_BLACK_LIST
    void GC_add_to_black_list_stack GC_PROTO((word p, ptr_t source));
# else
    void GC_add_to_black_list_stack GC_PROTO((word p));
# endif
struct hblk * GC_is_black_listed GC_PROTO((struct hblk * h, word len));
  			/* If there are likely to be false references	*/
  			/* to a block starting at h of the indicated    */
  			/* length, then return the next plausible	*/
  			/* starting location for h that might avoid	*/
  			/* these false references.			*/
void GC_promote_black_lists GC_PROTO((void));
  			/* Declare an end to a black listing phase.	*/
void GC_unpromote_black_lists GC_PROTO((void));
  			/* Approximately undo the effect of the above.	*/
  			/* This actually loses some information, but	*/
  			/* only in a reasonably safe way.		*/
word GC_number_stack_black_listed GC_PROTO(( \
	struct hblk *start, struct hblk *endp1));
  			/* Return the number of (stack) blacklisted	*/
  			/* blocks in the range for statistical		*/
  			/* purposes.					*/
  		 	
ptr_t GC_scratch_alloc GC_PROTO((word bytes));
  				/* GC internal memory allocation for	*/
  				/* small objects.  Deallocation is not  */
  				/* possible.				*/
  	
/* Heap block layout maps: */			
void GC_invalidate_map GC_PROTO((hdr * hhdr));
  				/* Remove the object map associated	*/
  				/* with the block.  This identifies	*/
  				/* the block as invalid to the mark	*/
  				/* routines.				*/
GC_bool GC_add_map_entry GC_PROTO((word sz));
  				/* Add a heap block map for objects of	*/
  				/* size sz to obj_map.			*/
  				/* Return FALSE on failure.		*/
void GC_register_displacement_inner GC_PROTO((word offset));
  				/* Version of GC_register_displacement	*/
  				/* that assumes lock is already held	*/
  				/* and signals are already disabled.	*/
  
/*  hblk allocation: */		
void GC_new_hblk GC_PROTO((word size_in_words, int kind));
  				/* Allocate a new heap block, and build */
  				/* a free list in it.			*/				

ptr_t GC_build_fl GC_PROTO((struct hblk *h, word sz,
			   GC_bool clear,  ptr_t list));
				/* Build a free list for objects of 	*/
				/* size sz in block h.  Append list to	*/
				/* end of the free lists.  Possibly	*/
				/* clear objects on the list.  Normally	*/
				/* called by GC_new_hblk, but also	*/
				/* called explicitly without GC lock.	*/

struct hblk * GC_allochblk GC_PROTO(( \
	word size_in_words, int kind, unsigned flags));
				/* Allocate a heap block, inform	*/
				/* the marker that block is valid	*/
				/* for objects of indicated size.	*/

ptr_t GC_alloc_large GC_PROTO((word lw, int k, unsigned flags));
			/* Allocate a large block of size lw words.	*/
			/* The block is not cleared.			*/
			/* Flags is 0 or IGNORE_OFF_PAGE.		*/
			/* Calls GC_allchblk to do the actual 		*/
			/* allocation, but also triggers GC and/or	*/
			/* heap expansion as appropriate.		*/
			/* Does not update GC_words_allocd, but does	*/
			/* other accounting.				*/

ptr_t GC_alloc_large_and_clear GC_PROTO((word lw, int k, unsigned flags));
			/* As above, but clear block if appropriate	*/
			/* for kind k.					*/

void GC_freehblk GC_PROTO((struct hblk * p));
				/* Deallocate a heap block and mark it  */
  				/* as invalid.				*/
  				
/*  Misc GC: */
void GC_init_inner GC_PROTO((void));
GC_bool GC_expand_hp_inner GC_PROTO((word n));
void GC_start_reclaim GC_PROTO((int abort_if_found));
  				/* Restore unmarked objects to free	*/
  				/* lists, or (if abort_if_found is	*/
  				/* TRUE) report them.			*/
  				/* Sweeping of small object pages is	*/
  				/* largely deferred.			*/
void GC_continue_reclaim GC_PROTO((word sz, int kind));
  				/* Sweep pages of the given size and	*/
  				/* kind, as long as possible, and	*/
  				/* as long as the corr. free list is    */
  				/* empty.				*/
void GC_reclaim_or_delete_all GC_PROTO((void));
  				/* Arrange for all reclaim lists to be	*/
  				/* empty.  Judiciously choose between	*/
  				/* sweeping and discarding each page.	*/
GC_bool GC_reclaim_all GC_PROTO((GC_stop_func stop_func, GC_bool ignore_old));
  				/* Reclaim all blocks.  Abort (in a	*/
  				/* consistent state) if f returns TRUE. */
GC_bool GC_block_empty GC_PROTO((hdr * hhdr));
 				/* Block completely unmarked? 	*/
GC_bool GC_never_stop_func GC_PROTO((void));
				/* Returns FALSE.		*/
GC_bool GC_try_to_collect_inner GC_PROTO((GC_stop_func f));

				/* Collect; caller must have acquired	*/
				/* lock and disabled signals.		*/
				/* Collection is aborted if f returns	*/
				/* TRUE.  Returns TRUE if it completes	*/
				/* successfully.			*/
# define GC_gcollect_inner() \
	(void) GC_try_to_collect_inner(GC_never_stop_func)
void GC_finish_collection GC_PROTO((void));
 				/* Finish collection.  Mark bits are	*/
  				/* consistent and lock is still held.	*/
GC_bool GC_collect_or_expand GC_PROTO(( \
	word needed_blocks, GC_bool ignore_off_page));
  				/* Collect or expand heap in an attempt */
  				/* make the indicated number of free	*/
  				/* blocks available.  Should be called	*/
  				/* until the blocks are available or	*/
  				/* until it fails by returning FALSE.	*/

extern GC_bool GC_is_initialized;	/* GC_init() has been run.	*/

#if defined(MSWIN32) || defined(MSWINCE)
  void GC_deinit GC_PROTO((void));
                                /* Free any resources allocated by      */
                                /* GC_init                              */
#endif

void GC_collect_a_little_inner GC_PROTO((int n));
  				/* Do n units worth of garbage 		*/
  				/* collection work, if appropriate.	*/
  				/* A unit is an amount appropriate for  */
  				/* HBLKSIZE bytes of allocation.	*/
/* ptr_t GC_generic_malloc GC_PROTO((word lb, int k)); */
  				/* Allocate an object of the given	*/
  				/* kind.  By default, there are only	*/
  				/* a few kinds: composite(pointerfree), */
				/* atomic, uncollectable, etc.		*/
				/* We claim it's possible for clever	*/
				/* client code that understands GC	*/
				/* internals to add more, e.g. to	*/
				/* communicate object layout info	*/
				/* to the collector.			*/
				/* The actual decl is in gc_mark.h.	*/
ptr_t GC_generic_malloc_ignore_off_page GC_PROTO((size_t b, int k));
  				/* As above, but pointers past the 	*/
  				/* first page of the resulting object	*/
  				/* are ignored.				*/
ptr_t GC_generic_malloc_inner GC_PROTO((word lb, int k));
  				/* Ditto, but I already hold lock, etc.	*/
ptr_t GC_generic_malloc_words_small_inner GC_PROTO((word lw, int k));
				/* Analogous to the above, but assumes	*/
				/* a small object size, and bypasses	*/
				/* MERGE_SIZES mechanism.		*/
ptr_t GC_generic_malloc_words_small GC_PROTO((size_t lw, int k));
  				/* As above, but size in units of words */
  				/* Bypasses MERGE_SIZES.  Assumes	*/
  				/* words <= MAXOBJSZ.			*/
ptr_t GC_generic_malloc_inner_ignore_off_page GC_PROTO((size_t lb, int k));
  				/* Allocate an object, where		*/
  				/* the client guarantees that there	*/
  				/* will always be a pointer to the 	*/
  				/* beginning of the object while the	*/
  				/* object is live.			*/
ptr_t GC_allocobj GC_PROTO((word sz, int kind));
  				/* Make the indicated 			*/
  				/* free list nonempty, and return its	*/
  				/* head.				*/

void GC_free_inner(GC_PTR p);
  
void GC_init_headers GC_PROTO((void));
struct hblkhdr * GC_install_header GC_PROTO((struct hblk *h));
  				/* Install a header for block h.	*/
  				/* Return 0 on failure, or the header	*/
  				/* otherwise.				*/
GC_bool GC_install_counts GC_PROTO((struct hblk * h, word sz));
  				/* Set up forwarding counts for block	*/
  				/* h of size sz.			*/
  				/* Return FALSE on failure.		*/
void GC_remove_header GC_PROTO((struct hblk * h));
  				/* Remove the header for block h.	*/
void GC_remove_counts GC_PROTO((struct hblk * h, word sz));
  				/* Remove forwarding counts for h.	*/
hdr * GC_find_header GC_PROTO((ptr_t h)); /* Debugging only.		*/
  
void GC_finalize GC_PROTO((void));
 			/* Perform all indicated finalization actions	*/
  			/* on unmarked objects.				*/
  			/* Unreachable finalizable objects are enqueued	*/
  			/* for processing by GC_invoke_finalizers.	*/
  			/* Invoked with lock.				*/

void GC_notify_or_invoke_finalizers GC_PROTO((void));
			/* If GC_finalize_on_demand is not set, invoke	*/
			/* eligible finalizers. Otherwise:		*/
			/* Call *GC_finalizer_notifier if there are	*/
			/* finalizers to be run, and we haven't called	*/
			/* this procedure yet this GC cycle.		*/

GC_API GC_PTR GC_make_closure GC_PROTO((GC_finalization_proc fn, GC_PTR data));
GC_API void GC_debug_invoke_finalizer GC_PROTO((GC_PTR obj, GC_PTR data));
			/* Auxiliary fns to make finalization work	*/
			/* correctly with displaced pointers introduced	*/
			/* by the debugging allocators.			*/
  			
void GC_add_to_heap GC_PROTO((struct hblk *p, word bytes));
  			/* Add a HBLKSIZE aligned chunk to the heap.	*/
  
void GC_print_obj GC_PROTO((ptr_t p));
  			/* P points to somewhere inside an object with	*/
  			/* debugging info.  Print a human readable	*/
  			/* description of the object to stderr.		*/
extern void (*GC_check_heap) GC_PROTO((void));
  			/* Check that all objects in the heap with 	*/
  			/* debugging info are intact.  			*/
  			/* Add any that are not to GC_smashed list.	*/
extern void (*GC_print_all_smashed) GC_PROTO((void));
			/* Print GC_smashed if it's not empty.		*/
			/* Clear GC_smashed list.			*/
extern void GC_print_all_errors GC_PROTO((void));
			/* Print smashed and leaked objects, if any.	*/
			/* Clear the lists of such objects.		*/
extern void (*GC_print_heap_obj) GC_PROTO((ptr_t p));
  			/* If possible print s followed by a more	*/
  			/* detailed description of the object 		*/
  			/* referred to by p.				*/
#if defined(LINUX) && defined(__ELF__) && !defined(SMALL_CONFIG)
  void GC_print_address_map GC_PROTO((void));
  			/* Print an address map of the process.		*/
#endif

extern GC_bool GC_have_errors;  /* We saw a smashed or leaked object.	*/
				/* Call error printing routine 		*/
				/* occasionally.			*/
extern GC_bool GC_print_stats;	/* Produce at least some logging output	*/
				/* Set from environment variable.	*/

#ifndef NO_DEBUGGING
  extern GC_bool GC_dump_regularly;  /* Generate regular debugging dumps. */
# define COND_DUMP if (GC_dump_regularly) GC_dump();
#else
# define COND_DUMP
#endif

#ifdef KEEP_BACK_PTRS
  extern long GC_backtraces;
  void GC_generate_random_backtrace_no_gc(void);
#endif

extern GC_bool GC_print_back_height;

#ifdef MAKE_BACK_GRAPH
  void GC_print_back_graph_stats(void);
#endif

/* Macros used for collector internal allocation.	*/
/* These assume the collector lock is held.		*/
#ifdef DBG_HDRS_ALL
    extern GC_PTR GC_debug_generic_malloc_inner(size_t lb, int k);
    extern GC_PTR GC_debug_generic_malloc_inner_ignore_off_page(size_t lb,
								int k);
#   define GC_INTERNAL_MALLOC GC_debug_generic_malloc_inner
#   define GC_INTERNAL_MALLOC_IGNORE_OFF_PAGE \
		 GC_debug_generic_malloc_inner_ignore_off_page
#   ifdef THREADS
#       define GC_INTERNAL_FREE GC_debug_free_inner
#   else
#       define GC_INTERNAL_FREE GC_debug_free
#   endif
#else
#   define GC_INTERNAL_MALLOC GC_generic_malloc_inner
#   define GC_INTERNAL_MALLOC_IGNORE_OFF_PAGE \
		 GC_generic_malloc_inner_ignore_off_page
#   ifdef THREADS
#       define GC_INTERNAL_FREE GC_free_inner
#   else
#       define GC_INTERNAL_FREE GC_free
#   endif
#endif

/* Memory unmapping: */
#ifdef USE_MUNMAP
  void GC_unmap_old(void);
  void GC_merge_unmapped(void);
  void GC_unmap(ptr_t start, word bytes);
  void GC_remap(ptr_t start, word bytes);
  void GC_unmap_gap(ptr_t start1, word bytes1, ptr_t start2, word bytes2);
#endif

/* Virtual dirty bit implementation:		*/
/* Each implementation exports the following:	*/
void GC_read_dirty GC_PROTO((void));
			/* Retrieve dirty bits.	*/
GC_bool GC_page_was_dirty GC_PROTO((struct hblk *h));
  			/* Read retrieved dirty bits.	*/
GC_bool GC_page_was_ever_dirty GC_PROTO((struct hblk *h));
  			/* Could the page contain valid heap pointers?	*/
void GC_is_fresh GC_PROTO((struct hblk *h, word n));
  			/* Assert the region currently contains no	*/
  			/* valid pointers.				*/
void GC_remove_protection GC_PROTO((struct hblk *h, word nblocks,
			   	    GC_bool pointerfree));
  			/* h is about to be writteni or allocated.  Ensure  */
			/* that it's not write protected by the virtual	    */
			/* dirty bit implementation.			    */
			
void GC_dirty_init GC_PROTO((void));
  
/* Slow/general mark bit manipulation: */
GC_API GC_bool GC_is_marked GC_PROTO((ptr_t p));
void GC_clear_mark_bit GC_PROTO((ptr_t p));
void GC_set_mark_bit GC_PROTO((ptr_t p));
  
/* Stubborn objects: */
void GC_read_changed GC_PROTO((void));	/* Analogous to GC_read_dirty */
GC_bool GC_page_was_changed GC_PROTO((struct hblk * h));
 				/* Analogous to GC_page_was_dirty */
void GC_clean_changing_list GC_PROTO((void));
 				/* Collect obsolete changing list entries */
void GC_stubborn_init GC_PROTO((void));
  
/* Debugging print routines: */
void GC_print_block_list GC_PROTO((void));
void GC_print_hblkfreelist GC_PROTO((void));
void GC_print_heap_sects GC_PROTO((void));
void GC_print_static_roots GC_PROTO((void));
void GC_print_finalization_stats GC_PROTO((void));
void GC_dump GC_PROTO((void));

#ifdef KEEP_BACK_PTRS
   void GC_store_back_pointer(ptr_t source, ptr_t dest);
   void GC_marked_for_finalization(ptr_t dest);
#  define GC_STORE_BACK_PTR(source, dest) GC_store_back_pointer(source, dest)
#  define GC_MARKED_FOR_FINALIZATION(dest) GC_marked_for_finalization(dest)
#else
#  define GC_STORE_BACK_PTR(source, dest) 
#  define GC_MARKED_FOR_FINALIZATION(dest)
#endif

/* Make arguments appear live to compiler */
# ifdef __WATCOMC__
    void GC_noop(void*, ...);
# else
#   ifdef __DMC__
      GC_API void GC_noop(...);
#   else
      GC_API void GC_noop(void*,...);
#   endif
# endif

void GC_noop1 GC_PROTO((word));

/* Logging and diagnostic output: 	*/
GC_API void GC_printf GC_PROTO((GC_CONST char * format, long, long, long, long, long, long));
			/* A version of printf that doesn't allocate,	*/
			/* is restricted to long arguments, and		*/
			/* (unfortunately) doesn't use varargs for	*/
			/* portability.  Restricted to 6 args and	*/
			/* 1K total output length.			*/
			/* (We use sprintf.  Hopefully that doesn't	*/
			/* allocate for long arguments.)  		*/
# define GC_printf0(f) GC_printf(f, 0l, 0l, 0l, 0l, 0l, 0l)
# define GC_printf1(f,a) GC_printf(f, (long)a, 0l, 0l, 0l, 0l, 0l)
# define GC_printf2(f,a,b) GC_printf(f, (long)a, (long)b, 0l, 0l, 0l, 0l)
# define GC_printf3(f,a,b,c) GC_printf(f, (long)a, (long)b, (long)c, 0l, 0l, 0l)
# define GC_printf4(f,a,b,c,d) GC_printf(f, (long)a, (long)b, (long)c, \
					    (long)d, 0l, 0l)
# define GC_printf5(f,a,b,c,d,e) GC_printf(f, (long)a, (long)b, (long)c, \
					      (long)d, (long)e, 0l)
# define GC_printf6(f,a,b,c,d,e,g) GC_printf(f, (long)a, (long)b, (long)c, \
						(long)d, (long)e, (long)g)

GC_API void GC_err_printf GC_PROTO((GC_CONST char * format, long, long, long, long, long, long));
# define GC_err_printf0(f) GC_err_puts(f)
# define GC_err_printf1(f,a) GC_err_printf(f, (long)a, 0l, 0l, 0l, 0l, 0l)
# define GC_err_printf2(f,a,b) GC_err_printf(f, (long)a, (long)b, 0l, 0l, 0l, 0l)
# define GC_err_printf3(f,a,b,c) GC_err_printf(f, (long)a, (long)b, (long)c, \
						  0l, 0l, 0l)
# define GC_err_printf4(f,a,b,c,d) GC_err_printf(f, (long)a, (long)b, \
						    (long)c, (long)d, 0l, 0l)
# define GC_err_printf5(f,a,b,c,d,e) GC_err_printf(f, (long)a, (long)b, \
						      (long)c, (long)d, \
						      (long)e, 0l)
# define GC_err_printf6(f,a,b,c,d,e,g) GC_err_printf(f, (long)a, (long)b, \
							(long)c, (long)d, \
							(long)e, (long)g)
			/* Ditto, writes to stderr.			*/
			
void GC_err_puts GC_PROTO((GC_CONST char *s));
			/* Write s to stderr, don't buffer, don't add	*/
			/* newlines, don't ...				*/

#if defined(LINUX) && !defined(SMALL_CONFIG)
  void GC_err_write GC_PROTO((GC_CONST char *buf, size_t len));
 			/* Write buf to stderr, don't buffer, don't add	*/
  			/* newlines, don't ...				*/
#endif


# ifdef GC_ASSERTIONS
#	define GC_ASSERT(expr) if(!(expr)) {\
		GC_err_printf2("Assertion failure: %s:%ld\n", \
				__FILE__, (unsigned long)__LINE__); \
		ABORT("assertion failure"); }
# else 
#	define GC_ASSERT(expr)
# endif

/* Check a compile time assertion at compile time.  The error	*/
/* message for failure is a bit baroque, but ...		*/
#if defined(mips) && !defined(__GNUC__)
/* DOB: MIPSPro C gets an internal error taking the sizeof an array type. 
   This code works correctly (ugliness is to avoid "unused var" warnings) */
# define GC_STATIC_ASSERT(expr) do { if (0) { char j[(expr)? 1 : -1]; j[0]='\0'; j[0]=j[0]; } } while(0)
#else
# define GC_STATIC_ASSERT(expr) sizeof(char[(expr)? 1 : -1])
#endif

# if defined(PARALLEL_MARK) || defined(THREAD_LOCAL_ALLOC)
    /* We need additional synchronization facilities from the thread	*/
    /* support.  We believe these are less performance critical		*/
    /* than the main garbage collector lock; standard pthreads-based	*/
    /* implementations should be sufficient.				*/

    /* The mark lock and condition variable.  If the GC lock is also 	*/
    /* acquired, the GC lock must be acquired first.  The mark lock is	*/
    /* used to both protect some variables used by the parallel		*/
    /* marker, and to protect GC_fl_builder_count, below.		*/
    /* GC_notify_all_marker() is called when				*/ 
    /* the state of the parallel marker changes				*/
    /* in some significant way (see gc_mark.h for details).  The	*/
    /* latter set of events includes incrementing GC_mark_no.		*/
    /* GC_notify_all_builder() is called when GC_fl_builder_count	*/
    /* reaches 0.							*/

     extern void GC_acquire_mark_lock(void);
     extern void GC_release_mark_lock(void);
     extern void GC_notify_all_builder(void);
     /* extern void GC_wait_builder(); */
     extern void GC_wait_for_reclaim(void);

     extern word GC_fl_builder_count;	/* Protected by mark lock.	*/
# endif /* PARALLEL_MARK || THREAD_LOCAL_ALLOC */
# ifdef PARALLEL_MARK
     extern void GC_notify_all_marker(void);
     extern void GC_wait_marker(void);
     extern word GC_mark_no;		/* Protected by mark lock.	*/

     extern void GC_help_marker(word my_mark_no);
		/* Try to help out parallel marker for mark cycle 	*/
		/* my_mark_no.  Returns if the mark cycle finishes or	*/
		/* was already done, or there was nothing to do for	*/
		/* some other reason.					*/
# endif /* PARALLEL_MARK */

# if defined(GC_PTHREADS) && !defined(GC_SOLARIS_THREADS)
  /* We define the thread suspension signal here, so that we can refer	*/
  /* to it in the dirty bit implementation, if necessary.  Ideally we	*/
  /* would allocate a (real-time ?) signal using the standard mechanism.*/
  /* unfortunately, there is no standard mechanism.  (There is one 	*/
  /* in Linux glibc, but it's not exported.)  Thus we continue to use	*/
  /* the same hard-coded signals we've always used.			*/
#  if !defined(SIG_SUSPEND)
#   if defined(GC_LINUX_THREADS) || defined(GC_DGUX386_THREADS) || defined(GC_NETBSD_THREADS)
#    if defined(SPARC) && !defined(SIGPWR)
       /* SPARC/Linux doesn't properly define SIGPWR in <signal.h>.
        * It is aliased to SIGLOST in asm/signal.h, though.		*/
#      define SIG_SUSPEND SIGLOST
#    elif defined(NACL)
#	define SIG_SUSPEND 0
#    else
       /* Linuxthreads itself uses SIGUSR1 and SIGUSR2.			*/
#      define SIG_SUSPEND SIGPWR
#    endif
#   else  /* !GC_LINUX_THREADS */
#     if defined(_SIGRTMIN)
#       define SIG_SUSPEND _SIGRTMIN + 6
#     else
#       define SIG_SUSPEND SIGRTMIN + 6
#     endif       
#   endif
#  endif /* !SIG_SUSPEND */
  
# endif

# endif /* GC_PRIVATE_H */
