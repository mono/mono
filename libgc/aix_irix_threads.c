/* 
 * Copyright (c) 1991-1995 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1996-1999 by Silicon Graphics.  All rights reserved.
 * Copyright (c) 1999-2003 by Hewlett-Packard Company. All rights reserved.
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
/*
 * Support code for Irix (>=6.2) Pthreads and for AIX pthreads.
 * This relies on properties
 * not guaranteed by the Pthread standard.  It may or may not be portable
 * to other implementations.
 *
 * Note that there is a lot of code duplication between this file and
 * (pthread_support.c, pthread_stop_world.c).  They should be merged.
 * Pthread_support.c should be directly usable.
 *
 * Please avoid adding new ports here; use the generic pthread support
 * as a base instead.
 */

# if defined(GC_IRIX_THREADS) || defined(GC_AIX_THREADS)

# include "private/gc_priv.h"
# include <pthread.h>
# include <assert.h>
# include <semaphore.h>
# include <time.h>
# include <errno.h>
# include <unistd.h>
# include <sys/mman.h>
# include <sys/time.h>

#undef pthread_create
#undef pthread_sigmask
#undef pthread_join

#if defined(GC_IRIX_THREADS) && !defined(MUTEX_RECURSIVE_NP)
#define MUTEX_RECURSIVE_NP PTHREAD_MUTEX_RECURSIVE
#endif

void GC_thr_init();

#if 0
void GC_print_sig_mask()
{
    sigset_t blocked;
    int i;

    if (pthread_sigmask(SIG_BLOCK, NULL, &blocked) != 0)
    	ABORT("pthread_sigmask");
    GC_printf0("Blocked: ");
    for (i = 1; i <= MAXSIG; i++) {
        if (sigismember(&blocked, i)) { GC_printf1("%ld ",(long) i); }
    }
    GC_printf0("\n");
}
#endif

/* We use the allocation lock to protect thread-related data structures. */

/* The set of all known threads.  We intercept thread creation and 	*/
/* joins.  We never actually create detached threads.  We allocate all 	*/
/* new thread stacks ourselves.  These allow us to maintain this	*/
/* data structure.							*/
/* Protected by GC_thr_lock.						*/
/* Some of this should be declared volatile, but that's incosnsistent	*/
/* with some library routine declarations.  		 		*/
typedef struct GC_Thread_Rep {
    struct GC_Thread_Rep * next;  /* More recently allocated threads	*/
				  /* with a given pthread id come 	*/
				  /* first.  (All but the first are	*/
				  /* guaranteed to be dead, but we may  */
				  /* not yet have registered the join.) */
    pthread_t id;
    word stop;
#	define NOT_STOPPED 0
#	define PLEASE_STOP 1
#	define STOPPED 2
    word flags;
#	define FINISHED 1   	/* Thread has exited.	*/
#	define DETACHED 2	/* Thread is intended to be detached.	*/
    ptr_t stack_cold;		/* cold end of the stack		*/
    ptr_t stack_hot;  		/* Valid only when stopped. */
				/* But must be within stack region at	*/
				/* all times.				*/
    void * status;		/* Used only to avoid premature 	*/
				/* reclamation of any data it might 	*/
				/* reference.				*/
} * GC_thread;

GC_thread GC_lookup_thread(pthread_t id);

/*
 * The only way to suspend threads given the pthread interface is to send
 * signals.  Unfortunately, this means we have to reserve
 * a signal, and intercept client calls to change the signal mask.
 */
#if 0 /* DOB: 6.1 */
# if defined(GC_AIX_THREADS)
#   define SIG_SUSPEND SIGUSR1
# else
#   define SIG_SUSPEND (SIGRTMIN + 6)
# endif
#endif

pthread_mutex_t GC_suspend_lock = PTHREAD_MUTEX_INITIALIZER;
				/* Number of threads stopped so far	*/
pthread_cond_t GC_suspend_ack_cv = PTHREAD_COND_INITIALIZER;
pthread_cond_t GC_continue_cv = PTHREAD_COND_INITIALIZER;

void GC_suspend_handler(int sig)
{
    int dummy;
    GC_thread me;
    sigset_t all_sigs;
    sigset_t old_sigs;
    int i;

    if (sig != SIG_SUSPEND) ABORT("Bad signal in suspend_handler");
    me = GC_lookup_thread(pthread_self());
    /* The lookup here is safe, since I'm doing this on behalf  */
    /* of a thread which holds the allocation lock in order	*/
    /* to stop the world.  Thus concurrent modification of the	*/
    /* data structure is impossible.				*/
    if (PLEASE_STOP != me -> stop) {
	/* Misdirected signal.	*/
	pthread_mutex_unlock(&GC_suspend_lock);
	return;
    }
    pthread_mutex_lock(&GC_suspend_lock);
    me -> stack_hot = (ptr_t)(&dummy);
    me -> stop = STOPPED;
    pthread_cond_signal(&GC_suspend_ack_cv);
    pthread_cond_wait(&GC_continue_cv, &GC_suspend_lock);
    pthread_mutex_unlock(&GC_suspend_lock);
    /* GC_printf1("Continuing 0x%x\n", pthread_self()); */
}


GC_bool GC_thr_initialized = FALSE;


# define THREAD_TABLE_SZ 128	/* Must be power of 2	*/
volatile GC_thread GC_threads[THREAD_TABLE_SZ];

void GC_push_thread_structures GC_PROTO((void))
{
    GC_push_all((ptr_t)(GC_threads), (ptr_t)(GC_threads)+sizeof(GC_threads));
}

/* Add a thread to GC_threads.  We assume it wasn't already there.	*/
/* Caller holds allocation lock.					*/
GC_thread GC_new_thread(pthread_t id)
{
    int hv = ((word)id) % THREAD_TABLE_SZ;
    GC_thread result;
    static struct GC_Thread_Rep first_thread;
    static GC_bool first_thread_used = FALSE;
    
    GC_ASSERT(I_HOLD_LOCK());
    if (!first_thread_used) {
    	result = &first_thread;
    	first_thread_used = TRUE;
    	/* Dont acquire allocation lock, since we may already hold it. */
    } else {
        result = (struct GC_Thread_Rep *)
        	 GC_generic_malloc_inner(sizeof(struct GC_Thread_Rep), NORMAL);
    }
    if (result == 0) return(0);
    result -> id = id;
    result -> next = GC_threads[hv];
    GC_threads[hv] = result;
    /* result -> flags = 0;     */
    /* result -> stop = 0;	*/
    return(result);
}

/* Delete a thread from GC_threads.  We assume it is there.	*/
/* (The code intentionally traps if it wasn't.)			*/
/* Caller holds allocation lock.				*/
/* We explicitly pass in the GC_thread we're looking for, since */
/* if a thread has been joined, but we have not yet		*/
/* been notified, then there may be more than one thread 	*/
/* in the table with the same pthread id.			*/
/* This is OK, but we need a way to delete a specific one.	*/
void GC_delete_gc_thread(pthread_t id, GC_thread gc_id)
{
    int hv = ((word)id) % THREAD_TABLE_SZ;
    register GC_thread p = GC_threads[hv];
    register GC_thread prev = 0;

    GC_ASSERT(I_HOLD_LOCK());
    while (p != gc_id) {
        prev = p;
        p = p -> next;
    }
    if (prev == 0) {
        GC_threads[hv] = p -> next;
    } else {
        prev -> next = p -> next;
    }
}

/* Return a GC_thread corresponding to a given thread_t.	*/
/* Returns 0 if it's not there.					*/
/* Caller holds  allocation lock or otherwise inhibits 		*/
/* updates.							*/
/* If there is more than one thread with the given id we 	*/
/* return the most recent one.					*/
GC_thread GC_lookup_thread(pthread_t id)
{
    int hv = ((word)id) % THREAD_TABLE_SZ;
    register GC_thread p = GC_threads[hv];
    
    /* I either hold the lock, or i'm being called from the stop-the-world
     * handler. */
#if defined(GC_AIX_THREADS)
    GC_ASSERT(I_HOLD_LOCK()); /* no stop-the-world handler needed on AIX */
#endif
    while (p != 0 && !pthread_equal(p -> id, id)) p = p -> next;
    return(p);
}

#if defined(GC_AIX_THREADS)
void GC_stop_world()
{
    pthread_t my_thread = pthread_self();
    register int i;
    register GC_thread p;
    register int result;
    struct timespec timeout;

    GC_ASSERT(I_HOLD_LOCK());
    for (i = 0; i < THREAD_TABLE_SZ; i++) {
      for (p = GC_threads[i]; p != 0; p = p -> next) {
        if (p -> id != my_thread) {
          pthread_suspend_np(p->id);
        }
      }
    }
    /* GC_printf1("World stopped 0x%x\n", pthread_self()); */
}

void GC_start_world()
{
    GC_thread p;
    unsigned i;
    pthread_t my_thread = pthread_self();

    /* GC_printf0("World starting\n"); */
    GC_ASSERT(I_HOLD_LOCK());
    for (i = 0; i < THREAD_TABLE_SZ; i++) {
      for (p = GC_threads[i]; p != 0; p = p -> next) {
        if (p -> id != my_thread) {
          pthread_continue_np(p->id);
        }
      }
    }
}

#else /* GC_AIX_THREADS */

/* Caller holds allocation lock.	*/
void GC_stop_world()
{
    pthread_t my_thread = pthread_self();
    register int i;
    register GC_thread p;
    register int result;
    struct timespec timeout;
    
    GC_ASSERT(I_HOLD_LOCK());
    for (i = 0; i < THREAD_TABLE_SZ; i++) {
      for (p = GC_threads[i]; p != 0; p = p -> next) {
        if (p -> id != my_thread) {
            if (p -> flags & FINISHED) {
		p -> stop = STOPPED;
		continue;
	    }
	    p -> stop = PLEASE_STOP;
            result = pthread_kill(p -> id, SIG_SUSPEND);
	    /* GC_printf1("Sent signal to 0x%x\n", p -> id); */
	    switch(result) {
                case ESRCH:
                    /* Not really there anymore.  Possible? */
                    p -> stop = STOPPED;
                    break;
                case 0:
                    break;
                default:
                    ABORT("pthread_kill failed");
            }
        }
      }
    }
    pthread_mutex_lock(&GC_suspend_lock);
    for (i = 0; i < THREAD_TABLE_SZ; i++) {
      for (p = GC_threads[i]; p != 0; p = p -> next) {
        while (p -> id != my_thread && p -> stop != STOPPED) {
	    clock_gettime(CLOCK_REALTIME, &timeout);
            timeout.tv_nsec += 50000000; /* 50 msecs */
            if (timeout.tv_nsec >= 1000000000) {
                timeout.tv_nsec -= 1000000000;
                ++timeout.tv_sec;
            }
            result = pthread_cond_timedwait(&GC_suspend_ack_cv,
					    &GC_suspend_lock,
                                            &timeout);
            if (result == ETIMEDOUT) {
                /* Signal was lost or misdirected.  Try again.      */
                /* Duplicate signals should be benign.              */
                result = pthread_kill(p -> id, SIG_SUSPEND);
	    }
	}
      }
    }
    pthread_mutex_unlock(&GC_suspend_lock);
    /* GC_printf1("World stopped 0x%x\n", pthread_self()); */
}

/* Caller holds allocation lock.	*/
void GC_start_world()
{
    GC_thread p;
    unsigned i;

    /* GC_printf0("World starting\n"); */
    GC_ASSERT(I_HOLD_LOCK());
    for (i = 0; i < THREAD_TABLE_SZ; i++) {
      for (p = GC_threads[i]; p != 0; p = p -> next) {
	p -> stop = NOT_STOPPED;
      }
    }
    pthread_mutex_lock(&GC_suspend_lock);
    /* All other threads are at pthread_cond_wait in signal handler.	*/
    /* Otherwise we couldn't have acquired the lock.			*/
    pthread_mutex_unlock(&GC_suspend_lock);
    pthread_cond_broadcast(&GC_continue_cv);
}

#endif /* GC_AIX_THREADS */


/* We hold allocation lock.  Should do exactly the right thing if the	*/
/* world is stopped.  Should not fail if it isn't.			*/
void GC_push_all_stacks()
{
    register int i;
    register GC_thread p;
    register ptr_t hot, cold;
    pthread_t me = pthread_self();
    
    /* GC_init() should have been called before GC_push_all_stacks is
     * invoked, and GC_init calls GC_thr_init(), which sets
     * GC_thr_initialized. */
    GC_ASSERT(GC_thr_initialized);

    /* GC_printf1("Pushing stacks from thread 0x%x\n", me); */
    GC_ASSERT(I_HOLD_LOCK());
    for (i = 0; i < THREAD_TABLE_SZ; i++) {
      for (p = GC_threads[i]; p != 0; p = p -> next) {
        if (p -> flags & FINISHED) continue;
	cold = p->stack_cold;
	if (!cold) cold=GC_stackbottom; /* 0 indicates 'original stack' */
        if (pthread_equal(p -> id, me)) {
	    hot = GC_approx_sp();
	} else {
#        ifdef GC_AIX_THREADS
          /* AIX doesn't use signals to suspend, so we need to get an */
	  /* accurate hot stack pointer.			      */
	  /* See http://publib16.boulder.ibm.com/pseries/en_US/libs/basetrf1/pthread_getthrds_np.htm */
          pthread_t id = p -> id;
          struct __pthrdsinfo pinfo;
          int regbuf[64];
          int val = sizeof(regbuf);
          int retval = pthread_getthrds_np(&id, PTHRDSINFO_QUERY_ALL, &pinfo,
			  		   sizeof(pinfo), regbuf, &val);
          if (retval != 0) {
	    printf("ERROR: pthread_getthrds_np() failed in GC\n");
	    abort();
	  }
	  /* according to the AIX ABI, 
	     "the lowest possible valid stack address is 288 bytes (144 + 144)
	     less than the current value of the stack pointer.  Functions may
	     use this stack space as volatile storage which is not preserved
	     across function calls."
	     ftp://ftp.penguinppc64.org/pub/people/amodra/PPC-elf64abi.txt.gz
	  */
          hot = (ptr_t)(unsigned long)pinfo.__pi_ustk-288;
	  cold = (ptr_t)pinfo.__pi_stackend; /* more precise */
          /* push the registers too, because they won't be on stack */
          GC_push_all_eager((ptr_t)&pinfo.__pi_context,
			    (ptr_t)((&pinfo.__pi_context)+1));
          GC_push_all_eager((ptr_t)regbuf, ((ptr_t)regbuf)+val);
#	 else
              hot = p -> stack_hot;
#	 endif
	}
#	ifdef STACK_GROWS_UP
          GC_push_all_stack(cold, hot);
#	else
 /* printf("thread 0x%x: hot=0x%08x cold=0x%08x\n", p -> id, hot, cold); */
          GC_push_all_stack(hot, cold);
#	endif
      }
    }
}


/* We hold the allocation lock.	*/
void GC_thr_init()
{
    GC_thread t;
    struct sigaction act;

    if (GC_thr_initialized) return;
    GC_ASSERT(I_HOLD_LOCK());
    GC_thr_initialized = TRUE;
#ifndef GC_AIX_THREADS
    (void) sigaction(SIG_SUSPEND, 0, &act);
    if (act.sa_handler != SIG_DFL)
    	ABORT("Previously installed SIG_SUSPEND handler");
    /* Install handler.	*/
	act.sa_handler = GC_suspend_handler;
	act.sa_flags = SA_RESTART;
	(void) sigemptyset(&act.sa_mask);
        if (0 != sigaction(SIG_SUSPEND, &act, 0))
	    ABORT("Failed to install SIG_SUSPEND handler");
#endif
    /* Add the initial thread, so we can stop it.	*/
      t = GC_new_thread(pthread_self());
      /* use '0' to indicate GC_stackbottom, since GC_init() has not
       * completed by the time we are called (from GC_init_inner()) */
      t -> stack_cold = 0; /* the original stack. */
      t -> stack_hot = (ptr_t)(&t);
      t -> flags = DETACHED;
}

int GC_pthread_sigmask(int how, const sigset_t *set, sigset_t *oset)
{
    sigset_t fudged_set;
    
#ifdef GC_AIX_THREADS
    return(pthread_sigmask(how, set, oset));
#endif

    if (set != NULL && (how == SIG_BLOCK || how == SIG_SETMASK)) {
        fudged_set = *set;
        sigdelset(&fudged_set, SIG_SUSPEND);
        set = &fudged_set;
    }
    return(pthread_sigmask(how, set, oset));
}

struct start_info {
    void *(*start_routine)(void *);
    void *arg;
    word flags;
    pthread_mutex_t registeredlock;
    pthread_cond_t registered;     
    int volatile registereddone;
};

void GC_thread_exit_proc(void *arg)
{
    GC_thread me;

    LOCK();
    me = GC_lookup_thread(pthread_self());
    me -> flags |= FINISHED;
    /* reclaim DETACHED thread right away; otherwise wait until join() */
    if (me -> flags & DETACHED) {
	GC_delete_gc_thread(pthread_self(), me);
    }
    UNLOCK();
}

int GC_pthread_join(pthread_t thread, void **retval)
{
    int result;
    GC_thread thread_gc_id;
    
    LOCK();
    thread_gc_id = GC_lookup_thread(thread);
    /* This is guaranteed to be the intended one, since the thread id	*/
    /* cant have been recycled by pthreads.				*/
    UNLOCK();
    GC_ASSERT(!(thread_gc_id->flags & DETACHED));
    result = pthread_join(thread, retval);
    /* Some versions of the Irix pthreads library can erroneously 	*/
    /* return EINTR when the call succeeds.				*/
	if (EINTR == result) result = 0;
    GC_ASSERT(thread_gc_id->flags & FINISHED);
    LOCK();
    /* Here the pthread thread id may have been recycled. */
    GC_delete_gc_thread(thread, thread_gc_id);
    UNLOCK();
    return result;
}

void * GC_start_routine(void * arg)
{
    int dummy;
    struct start_info * si = arg;
    void * result;
    GC_thread me;
    pthread_t my_pthread;
    void *(*start)(void *);
    void *start_arg;

    my_pthread = pthread_self();
    /* If a GC occurs before the thread is registered, that GC will	*/
    /* ignore this thread.  That's fine, since it will block trying to  */
    /* acquire the allocation lock, and won't yet hold interesting 	*/
    /* pointers.							*/
    LOCK();
    /* We register the thread here instead of in the parent, so that	*/
    /* we don't need to hold the allocation lock during pthread_create. */
    /* Holding the allocation lock there would make REDIRECT_MALLOC	*/
    /* impossible.  It probably still doesn't work, but we're a little  */
    /* closer ...							*/
    /* This unfortunately means that we have to be careful the parent	*/
    /* doesn't try to do a pthread_join before we're registered.	*/
    me = GC_new_thread(my_pthread);
    me -> flags = si -> flags;
    me -> stack_cold = (ptr_t) &dummy; /* this now the 'start of stack' */
    me -> stack_hot = me->stack_cold;/* this field should always be sensible */
    UNLOCK();
    start = si -> start_routine;
    start_arg = si -> arg;

    pthread_mutex_lock(&(si->registeredlock));
    si->registereddone = 1;
    pthread_cond_signal(&(si->registered));
    pthread_mutex_unlock(&(si->registeredlock));
    /* si went away as soon as we did this unlock */

    pthread_cleanup_push(GC_thread_exit_proc, 0);
    result = (*start)(start_arg);
    me -> status = result;
    pthread_cleanup_pop(1);
	/* This involves acquiring the lock, ensuring that we can't exit */
	/* while a collection that thinks we're alive is trying to stop  */
	/* us.								 */
    return(result);
}

int
GC_pthread_create(pthread_t *new_thread,
		  const pthread_attr_t *attr,
                  void *(*start_routine)(void *), void *arg)
{
    int result;
    GC_thread t;
    int detachstate;
    word my_flags = 0;
    struct start_info * si;
    	/* This is otherwise saved only in an area mmapped by the thread */
    	/* library, which isn't visible to the collector.		 */

    LOCK();
    /* GC_INTERNAL_MALLOC implicitly calls GC_init() if required */
    si = (struct start_info *)GC_INTERNAL_MALLOC(sizeof(struct start_info),
						 NORMAL);
    GC_ASSERT(GC_thr_initialized); /* initialized by GC_init() */
    UNLOCK();
    if (0 == si) return(ENOMEM);
    pthread_mutex_init(&(si->registeredlock), NULL);
    pthread_cond_init(&(si->registered),NULL);
    pthread_mutex_lock(&(si->registeredlock));
    si -> start_routine = start_routine;
    si -> arg = arg;

    pthread_attr_getdetachstate(attr, &detachstate);
    if (PTHREAD_CREATE_DETACHED == detachstate) my_flags |= DETACHED;
    si -> flags = my_flags;
    result = pthread_create(new_thread, attr, GC_start_routine, si); 

    /* Wait until child has been added to the thread table.		*/
    /* This also ensures that we hold onto si until the child is done	*/
    /* with it.  Thus it doesn't matter whether it is otherwise		*/
    /* visible to the collector.					*/

    if (0 == result) {
      si->registereddone = 0;
      while (!si->registereddone) 
        pthread_cond_wait(&(si->registered), &(si->registeredlock));
    }
    pthread_mutex_unlock(&(si->registeredlock));

    pthread_cond_destroy(&(si->registered));
    pthread_mutex_destroy(&(si->registeredlock));
    LOCK();
    GC_INTERNAL_FREE(si);
    UNLOCK();

    return(result);
}

/* For now we use the pthreads locking primitives on HP/UX */

VOLATILE GC_bool GC_collecting = 0; /* A hint that we're in the collector and       */
                        /* holding the allocation lock for an           */
                        /* extended period.                             */

/* Reasonably fast spin locks.  Basically the same implementation */
/* as STL alloc.h.						  */

#define SLEEP_THRESHOLD 3

volatile unsigned int GC_allocate_lock = 0;
#define GC_TRY_LOCK() !GC_test_and_set(&GC_allocate_lock)
#define GC_LOCK_TAKEN GC_allocate_lock

void GC_lock()
{
#   define low_spin_max 30  /* spin cycles if we suspect uniprocessor */
#   define high_spin_max 1000 /* spin cycles for multiprocessor */
    static unsigned spin_max = low_spin_max;
    unsigned my_spin_max;
    static unsigned last_spins = 0;
    unsigned my_last_spins;
    volatile unsigned junk;
#   define PAUSE junk *= junk; junk *= junk; junk *= junk; junk *= junk
    int i;

    if (GC_TRY_LOCK()) {
        return;
    }
    junk = 0;
    my_spin_max = spin_max;
    my_last_spins = last_spins;
    for (i = 0; i < my_spin_max; i++) {
        if (GC_collecting) goto yield;
        if (i < my_last_spins/2 || GC_LOCK_TAKEN) {
            PAUSE; 
            continue;
        }
        if (GC_TRY_LOCK()) {
	    /*
             * got it!
             * Spinning worked.  Thus we're probably not being scheduled
             * against the other process with which we were contending.
             * Thus it makes sense to spin longer the next time.
	     */
            last_spins = i;
            spin_max = high_spin_max;
            return;
        }
    }
    /* We are probably being scheduled against the other process.  Sleep. */
    spin_max = low_spin_max;
yield:
    for (i = 0;; ++i) {
        if (GC_TRY_LOCK()) {
            return;
        }
        if (i < SLEEP_THRESHOLD) {
            sched_yield();
	} else {
	    struct timespec ts;
	
	    if (i > 26) i = 26;
			/* Don't wait for more than about 60msecs, even	*/
			/* under extreme contention.			*/
	    ts.tv_sec = 0;
	    ts.tv_nsec = 1 << i;
	    nanosleep(&ts, 0);
	}
    }
}

# else  /* !GC_IRIX_THREADS && !GC_AIX_THREADS */

#ifndef LINT
  int GC_no_Irix_threads;
#endif

# endif /* IRIX_THREADS */

