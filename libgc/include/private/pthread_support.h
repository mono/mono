#ifndef GC_PTHREAD_SUPPORT_H
#define GC_PTHREAD_SUPPORT_H

# include "private/gc_priv.h"

# if defined(GC_PTHREADS) && !defined(GC_SOLARIS_THREADS) \
     && !defined(GC_IRIX_THREADS) && !defined(GC_WIN32_THREADS)
     
#if defined(GC_DARWIN_THREADS)
# include "private/darwin_stop_world.h"
#else
# include "private/pthread_stop_world.h"
#endif

/* We use the allocation lock to protect thread-related data structures. */

/* The set of all known threads.  We intercept thread creation and 	*/
/* joins.								*/
/* Protected by allocation/GC lock.					*/
/* Some of this should be declared volatile, but that's inconsistent	*/
/* with some library routine declarations.  		 		*/
typedef struct GC_Thread_Rep {
    struct GC_Thread_Rep * next;  /* More recently allocated threads	*/
				  /* with a given pthread id come 	*/
				  /* first.  (All but the first are	*/
				  /* guaranteed to be dead, but we may  */
				  /* not yet have registered the join.) */
    pthread_t id;
    /* Extra bookkeeping information the stopping code uses */
    struct thread_stop_info stop_info;
    
    short flags;
#	define FINISHED 1   	/* Thread has exited.	*/
#	define DETACHED 2	/* Thread is intended to be detached.	*/
#	define MAIN_THREAD 4	/* True for the original thread only.	*/
    short thread_blocked;	/* Protected by GC lock.		*/
    				/* Treated as a boolean value.  If set,	*/
    				/* thread will acquire GC lock before	*/
    				/* doing any pointer manipulations, and	*/
    				/* has set its sp value.  Thus it does	*/
    				/* not need to be sent a signal to stop	*/
    				/* it.					*/
    ptr_t stack_end;		/* Cold end of the stack.		*/
#   ifdef IA64
	ptr_t backing_store_end;
	ptr_t backing_store_ptr;
#   endif
    void * status;		/* The value returned from the thread.  */
    				/* Used only to avoid premature 	*/
				/* reclamation of any data it might 	*/
				/* reference.				*/
#   ifdef THREAD_LOCAL_ALLOC
#	if CPP_WORDSZ == 64 && defined(ALIGN_DOUBLE)
#	    define GRANULARITY 16
#	    define NFREELISTS 49
#	else
#	    define GRANULARITY 8
#	    define NFREELISTS 65
#	endif
	/* The ith free list corresponds to size i*GRANULARITY */
#	define INDEX_FROM_BYTES(n) ((ADD_SLOP(n) + GRANULARITY - 1)/GRANULARITY)
#	define BYTES_FROM_INDEX(i) ((i) * GRANULARITY - EXTRA_BYTES)
#	define SMALL_ENOUGH(bytes) (ADD_SLOP(bytes) <= \
				    (NFREELISTS-1)*GRANULARITY)
	ptr_t ptrfree_freelists[NFREELISTS];
	ptr_t normal_freelists[NFREELISTS];
#	ifdef GC_GCJ_SUPPORT
	  ptr_t gcj_freelists[NFREELISTS];
#	endif
		/* Free lists contain either a pointer or a small count */
		/* reflecting the number of granules allocated at that	*/
		/* size.						*/
		/* 0 ==> thread-local allocation in use, free list	*/
		/*       empty.						*/
		/* > 0, <= DIRECT_GRANULES ==> Using global allocation,	*/
		/*       too few objects of this size have been		*/
		/* 	 allocated by this thread.			*/
		/* >= HBLKSIZE  => pointer to nonempty free list.	*/
		/* > DIRECT_GRANULES, < HBLKSIZE ==> transition to	*/
		/*    local alloc, equivalent to 0.			*/
#	define DIRECT_GRANULES (HBLKSIZE/GRANULARITY)
		/* Don't use local free lists for up to this much 	*/
		/* allocation.						*/
#   endif
} * GC_thread;

# define THREAD_TABLE_SZ 128	/* Must be power of 2	*/
extern volatile GC_thread GC_threads[THREAD_TABLE_SZ];

extern GC_bool GC_thr_initialized;

GC_thread GC_lookup_thread(pthread_t id);

void GC_stop_init();

extern GC_bool GC_in_thread_creation;
	/* We may currently be in thread creation or destruction.	*/
	/* Only set to TRUE while allocation lock is held.		*/
	/* When set, it is OK to run GC from unknown thread.		*/

#endif /* GC_PTHREADS && !GC_SOLARIS_THREADS.... etc */
#endif /* GC_PTHREAD_SUPPORT_H */
