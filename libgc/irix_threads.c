/* 
 * Copyright (c) 1991-1995 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1996-1999 by Silicon Graphics.  All rights reserved.
 * Copyright (c) 1999 by Hewlett-Packard Company. All rights reserved.
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
 * Support code for Irix (>=6.2) Pthreads.  This relies on properties
 * not guaranteed by the Pthread standard.  It may or may not be portable
 * to other implementations.
 *
 * This now also includes an initial attempt at thread support for
 * HP/UX 11.
 *
 * Note that there is a lot of code duplication between linux_threads.c
 * and irix_threads.c; any changes made here may need to be reflected
 * there too.
 */

# if defined(GC_IRIX_THREADS)

# include "private/gc_priv.h"
# include <pthread.h>
# include <semaphore.h>
# include <time.h>
# include <errno.h>
# include <unistd.h>
# include <sys/mman.h>
# include <sys/time.h>

#undef pthread_create
#undef pthread_sigmask
#undef pthread_join
#undef pthread_detach

#ifdef HANDLE_FORK
  --> Not yet supported.  Try porting the code from linux_threads.c.
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
#	define CLIENT_OWNS_STACK	4
				/* Stack was supplied by client.	*/
    ptr_t stack;
    ptr_t stack_ptr;  		/* Valid only when stopped. */
				/* But must be within stack region at	*/
				/* all times.				*/
    size_t stack_size;		/* 0 for original thread.	*/
    void * status;		/* Used only to avoid premature 	*/
				/* reclamation of any data it might 	*/
				/* reference.				*/
} * GC_thread;

GC_thread GC_lookup_thread(pthread_t id);

/*
 * The only way to suspend threads given the pthread interface is to send
 * signals.  Unfortunately, this means we have to reserve
 * a signal, and intercept client calls to change the signal mask.
 * We use SIG_SUSPEND, defined in gc_priv.h.
 */

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
    me -> stack_ptr = (ptr_t)(&dummy);
    me -> stop = STOPPED;
    pthread_cond_signal(&GC_suspend_ack_cv);
    pthread_cond_wait(&GC_continue_cv, &GC_suspend_lock);
    pthread_mutex_unlock(&GC_suspend_lock);
    /* GC_printf1("Continuing 0x%x\n", pthread_self()); */
}


GC_bool GC_thr_initialized = FALSE;

size_t GC_min_stack_sz;

# define N_FREE_LISTS 25
ptr_t GC_stack_free_lists[N_FREE_LISTS] = { 0 };
		/* GC_stack_free_lists[i] is free list for stacks of 	*/
		/* size GC_min_stack_sz*2**i.				*/
		/* Free lists are linked through first word.		*/

/* Return a stack of size at least *stack_size.  *stack_size is	*/
/* replaced by the actual stack size.				*/
/* Caller holds allocation lock.				*/
ptr_t GC_stack_alloc(size_t * stack_size)
{
    register size_t requested_sz = *stack_size;
    register size_t search_sz = GC_min_stack_sz;
    register int index = 0;	/* = log2(search_sz/GC_min_stack_sz) */
    register ptr_t result;
    
    while (search_sz < requested_sz) {
        search_sz *= 2;
        index++;
    }
    if ((result = GC_stack_free_lists[index]) == 0
        && (result = GC_stack_free_lists[index+1]) != 0) {
        /* Try next size up. */
        search_sz *= 2; index++;
    }
    if (result != 0) {
        GC_stack_free_lists[index] = *(ptr_t *)result;
    } else {
        result = (ptr_t) GC_scratch_alloc(search_sz + 2*GC_page_size);
        result = (ptr_t)(((word)result + GC_page_size) & ~(GC_page_size - 1));
        /* Protect hottest page to detect overflow. */
#	ifdef STACK_GROWS_UP
          /* mprotect(result + search_sz, GC_page_size, PROT_NONE); */
#	else
          /* mprotect(result, GC_page_size, PROT_NONE); */
          result += GC_page_size;
#	endif
    }
    *stack_size = search_sz;
    return(result);
}

/* Caller holds allocation lock.					*/
void GC_stack_free(ptr_t stack, size_t size)
{
    register int index = 0;
    register size_t search_sz = GC_min_stack_sz;
    
    while (search_sz < size) {
        search_sz *= 2;
        index++;
    }
    if (search_sz != size) ABORT("Bad stack size");
    *(ptr_t *)stack = GC_stack_free_lists[index];
    GC_stack_free_lists[index] = stack;
}



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
    
    if (!first_thread_used) {
    	result = &first_thread;
    	first_thread_used = TRUE;
    	/* Dont acquire allocation lock, since we may already hold it. */
    } else {
        result = (struct GC_Thread_Rep *)
        	 GC_INTERNAL_MALLOC(sizeof(struct GC_Thread_Rep), NORMAL);
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
void GC_delete_thread(pthread_t id)
{
    int hv = ((word)id) % THREAD_TABLE_SZ;
    register GC_thread p = GC_threads[hv];
    register GC_thread prev = 0;
    
    while (!pthread_equal(p -> id, id)) {
        prev = p;
        p = p -> next;
    }
    if (prev == 0) {
        GC_threads[hv] = p -> next;
    } else {
        prev -> next = p -> next;
    }
}

/* If a thread has been joined, but we have not yet		*/
/* been notified, then there may be more than one thread 	*/
/* in the table with the same pthread id.			*/
/* This is OK, but we need a way to delete a specific one.	*/
void GC_delete_gc_thread(pthread_t id, GC_thread gc_id)
{
    int hv = ((word)id) % THREAD_TABLE_SZ;
    register GC_thread p = GC_threads[hv];
    register GC_thread prev = 0;

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
    
    while (p != 0 && !pthread_equal(p -> id, id)) p = p -> next;
    return(p);
}


/* Caller holds allocation lock.	*/
void GC_stop_world()
{
    pthread_t my_thread = pthread_self();
    register int i;
    register GC_thread p;
    register int result;
    struct timespec timeout;
    
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

# ifdef MMAP_STACKS
--> not really supported yet.
int GC_is_thread_stack(ptr_t addr)
{
    register int i;
    register GC_thread p;

    for (i = 0; i < THREAD_TABLE_SZ; i++) {
      for (p = GC_threads[i]; p != 0; p = p -> next) {
        if (p -> stack_size != 0) {
            if (p -> stack <= addr &&
                addr < p -> stack + p -> stack_size)
                   return 1;
       }
      }
    }
    return 0;
}
# endif

/* We hold allocation lock.  Should do exactly the right thing if the	*/
/* world is stopped.  Should not fail if it isn't.			*/
void GC_push_all_stacks()
{
    register int i;
    register GC_thread p;
    register ptr_t sp = GC_approx_sp();
    register ptr_t hot, cold;
    pthread_t me = pthread_self();
    
    if (!GC_thr_initialized) GC_thr_init();
    /* GC_printf1("Pushing stacks from thread 0x%x\n", me); */
    for (i = 0; i < THREAD_TABLE_SZ; i++) {
      for (p = GC_threads[i]; p != 0; p = p -> next) {
        if (p -> flags & FINISHED) continue;
        if (pthread_equal(p -> id, me)) {
	    hot = GC_approx_sp();
	} else {
	    hot = p -> stack_ptr;
	}
        if (p -> stack_size != 0) {
#	  ifdef STACK_GROWS_UP
	    cold = p -> stack;
#	  else
            cold = p -> stack + p -> stack_size;
#	  endif
        } else {
            /* The original stack. */
            cold = GC_stackbottom;
        }
#	ifdef STACK_GROWS_UP
          GC_push_all_stack(cold, hot);
#	else
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
    GC_thr_initialized = TRUE;
    GC_min_stack_sz = HBLKSIZE;
    (void) sigaction(SIG_SUSPEND, 0, &act);
    if (act.sa_handler != SIG_DFL)
    	ABORT("Previously installed SIG_SUSPEND handler");
    /* Install handler.	*/
	act.sa_handler = GC_suspend_handler;
	act.sa_flags = SA_RESTART;
	(void) sigemptyset(&act.sa_mask);
        if (0 != sigaction(SIG_SUSPEND, &act, 0))
	    ABORT("Failed to install SIG_SUSPEND handler");
    /* Add the initial thread, so we can stop it.	*/
      t = GC_new_thread(pthread_self());
      t -> stack_size = 0;
      t -> stack_ptr = (ptr_t)(&t);
      t -> flags = DETACHED;
}

int GC_pthread_sigmask(int how, const sigset_t *set, sigset_t *oset)
{
    sigset_t fudged_set;
    
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
    ptr_t stack;
    size_t stack_size;
    sem_t registered;   	/* 1 ==> in our thread table, but 	*/
				/* parent hasn't yet noticed.		*/
};

void GC_thread_exit_proc(void *arg)
{
    GC_thread me;

    LOCK();
    me = GC_lookup_thread(pthread_self());
    if (me -> flags & DETACHED) {
    	GC_delete_thread(pthread_self());
    } else {
	me -> flags |= FINISHED;
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
    result = pthread_join(thread, retval);
    /* Some versions of the Irix pthreads library can erroneously 	*/
    /* return EINTR when the call succeeds.				*/
	if (EINTR == result) result = 0;
    if (result == 0) {
        LOCK();
        /* Here the pthread thread id may have been recycled. */
        GC_delete_gc_thread(thread, thread_gc_id);
        UNLOCK();
    }
    return result;
}

int GC_pthread_detach(pthread_t thread)
{
    int result;
    GC_thread thread_gc_id;
    
    LOCK();
    thread_gc_id = GC_lookup_thread(thread);
    UNLOCK();
    result = pthread_detach(thread);
    if (result == 0) {
      LOCK();
      thread_gc_id -> flags |= DETACHED;
      /* Here the pthread thread id may have been recycled. */
      if (thread_gc_id -> flags & FINISHED) {
        GC_delete_gc_thread(thread, thread_gc_id);
      }
      UNLOCK();
    }
    return result;
}

void * GC_start_routine(void * arg)
{
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
    me -> stack = si -> stack;
    me -> stack_size = si -> stack_size;
    me -> stack_ptr = (ptr_t)si -> stack + si -> stack_size - sizeof(word);
    UNLOCK();
    start = si -> start_routine;
    start_arg = si -> arg;
    sem_post(&(si -> registered));
    pthread_cleanup_push(GC_thread_exit_proc, 0);
    result = (*start)(start_arg);
    me -> status = result;
    me -> flags |= FINISHED;
    pthread_cleanup_pop(1);
	/* This involves acquiring the lock, ensuring that we can't exit */
	/* while a collection that thinks we're alive is trying to stop  */
	/* us.								 */
    return(result);
}

# define copy_attr(pa_ptr, source) *(pa_ptr) = *(source)

int
GC_pthread_create(pthread_t *new_thread,
		  const pthread_attr_t *attr,
                  void *(*start_routine)(void *), void *arg)
{
    int result;
    GC_thread t;
    void * stack;
    size_t stacksize;
    pthread_attr_t new_attr;
    int detachstate;
    word my_flags = 0;
    struct start_info * si = GC_malloc(sizeof(struct start_info)); 
	/* This is otherwise saved only in an area mmapped by the thread */
	/* library, which isn't visible to the collector.		 */

    if (0 == si) return(ENOMEM);
    if (0 != sem_init(&(si -> registered), 0, 0)) {
        ABORT("sem_init failed");
    }
    si -> start_routine = start_routine;
    si -> arg = arg;
    LOCK();
    if (!GC_is_initialized) GC_init();
    if (NULL == attr) {
        stack = 0;
	(void) pthread_attr_init(&new_attr);
    } else {
	copy_attr(&new_attr, attr);
	pthread_attr_getstackaddr(&new_attr, &stack);
    }
    pthread_attr_getstacksize(&new_attr, &stacksize);
    pthread_attr_getdetachstate(&new_attr, &detachstate);
    if (stacksize < GC_min_stack_sz) ABORT("Stack too small");
    if (0 == stack) {
     	stack = (void *)GC_stack_alloc(&stacksize);
     	if (0 == stack) {
     	    UNLOCK();
     	    return(ENOMEM);
     	}
	pthread_attr_setstackaddr(&new_attr, stack);
    } else {
    	my_flags |= CLIENT_OWNS_STACK;
    }
    if (PTHREAD_CREATE_DETACHED == detachstate) my_flags |= DETACHED;
    si -> flags = my_flags;
    si -> stack = stack;
    si -> stack_size = stacksize;
    result = pthread_create(new_thread, &new_attr, GC_start_routine, si);
    if (0 == new_thread && !(my_flags & CLIENT_OWNS_STACK)) {
      	GC_stack_free(stack, stacksize);
    }        
    UNLOCK();  
    /* Wait until child has been added to the thread table.		*/
    /* This also ensures that we hold onto si until the child is done	*/
    /* with it.  Thus it doesn't matter whether it is otherwise		*/
    /* visible to the collector.					*/
        while (0 != sem_wait(&(si -> registered))) {
	  if (errno != EINTR) {
	    GC_printf1("Sem_wait: errno = %ld\n", (unsigned long) errno);
	    ABORT("sem_wait failed");
	  }
	}
        sem_destroy(&(si -> registered));
    pthread_attr_destroy(&new_attr);  /* Probably unnecessary under Irix */
    return(result);
}

VOLATILE GC_bool GC_collecting = 0;
			/* A hint that we're in the collector and       */
                        /* holding the allocation lock for an           */
                        /* extended period.                             */

/* Reasonably fast spin locks.  Basically the same implementation */
/* as STL alloc.h.						  */

#define SLEEP_THRESHOLD 3

unsigned long GC_allocate_lock = 0;
# define GC_TRY_LOCK() !GC_test_and_set(&GC_allocate_lock)
# define GC_LOCK_TAKEN GC_allocate_lock

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

# else

#ifndef LINT
  int GC_no_Irix_threads;
#endif

# endif /* GC_IRIX_THREADS */

