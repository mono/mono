/* 
 * Copyright (c) 1994 by Xerox Corporation.  All rights reserved.
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
 * Support code for Solaris threads.  Provides functionality we wish Sun
 * had provided.  Relies on some information we probably shouldn't rely on.
 * Modified by Peter C. for Solaris Posix Threads.
 */

# if defined(GC_SOLARIS_PTHREADS)
# include "private/gc_priv.h"
# include <pthread.h>
# include <thread.h>
# include <signal.h>
# include <fcntl.h>
# include <sys/types.h>
# include <sys/mman.h>
# include <sys/time.h>
# include <sys/resource.h>
# include <sys/stat.h>
# include <sys/syscall.h>
# include <sys/procfs.h>
# include <sys/lwp.h>
# include <sys/reg.h>
# define _CLASSIC_XOPEN_TYPES
# include <unistd.h>
# include <errno.h>
# include "private/solaris_threads.h"
# include <stdio.h>

#undef pthread_join
#undef pthread_create

pthread_cond_t GC_prom_join_cv;		/* Broadcast when any thread terminates	*/
pthread_cond_t GC_create_cv;		/* Signalled when a new undetached	*/
				/* thread starts.			*/
				
extern GC_bool GC_multithreaded;

/* We use the allocation lock to protect thread-related data structures. */

/* We stop the world using /proc primitives.  This makes some	*/
/* minimal assumptions about the threads implementation.	*/
/* We don't play by the rules, since the rules make this	*/
/* impossible (as of Solaris 2.3).  Also note that as of	*/
/* Solaris 2.3 the various thread and lwp suspension		*/
/* primitives failed to stop threads by the time the request	*/
/* is completed.						*/



int GC_pthread_join(pthread_t wait_for, void **status)
{
	return GC_thr_join((thread_t)wait_for, NULL, status);
}


int
GC_pthread_create(pthread_t *new_thread,
          const pthread_attr_t *attr_in,
          void * (*thread_execp)(void *), void *arg)
{
    int result;
    GC_thread t;
    pthread_t my_new_thread;
    pthread_attr_t  attr;
    word my_flags = 0;
    int  flag;
    void * stack = 0;
    size_t stack_size = 0;
    int    n;
    struct sched_param schedparam;
   
    (void)pthread_attr_init(&attr);
    if (attr_in != 0) {
	(void)pthread_attr_getstacksize(attr_in, &stack_size);
	(void)pthread_attr_getstackaddr(attr_in, &stack);
    }

    LOCK();
    if (!GC_is_initialized) {
	    GC_init_inner();
    }
    GC_multithreaded++;
	    
    if (stack == 0) {
     	if (stack_size == 0)
		stack_size = 1048576;
			  /* ^-- 1 MB (this was GC_min_stack_sz, but that
			   * violates the pthread_create documentation which
			   * says the default value if none is supplied is
			   * 1MB) */
	else
		stack_size += thr_min_stack();

     	stack = (void *)GC_stack_alloc(&stack_size);
     	if (stack == 0) {
	    GC_multithreaded--;
     	    UNLOCK();
	    errno = ENOMEM;
     	    return -1;
     	}
    } else {
    	my_flags |= CLIENT_OWNS_STACK;
    }
    (void)pthread_attr_setstacksize(&attr, stack_size);
    (void)pthread_attr_setstackaddr(&attr, stack);
    if (attr_in != 0) {
	(void)pthread_attr_getscope(attr_in, &n);
	(void)pthread_attr_setscope(&attr, n);
	(void)pthread_attr_getschedparam(attr_in, &schedparam);
	(void)pthread_attr_setschedparam(&attr, &schedparam);
	(void)pthread_attr_getschedpolicy(attr_in, &n);
	(void)pthread_attr_setschedpolicy(&attr, n);
	(void)pthread_attr_getinheritsched(attr_in, &n);
	(void)pthread_attr_setinheritsched(&attr, n);

	(void)pthread_attr_getdetachstate(attr_in, &flag);
	if (flag == PTHREAD_CREATE_DETACHED) {
		my_flags |= DETACHED;
	}
	(void)pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_JOINABLE);
    }
    /*
     * thr_create can call malloc(), which if redirected will
     * attempt to acquire the allocation lock.
     * Unlock here to prevent deadlock.
     */


#if 0
#ifdef I386
    UNLOCK();
#endif
#endif
    result = 
	    pthread_create(&my_new_thread, &attr, thread_execp, arg);
#if 0
#ifdef I386
    LOCK();
#endif
#endif
    if (result == 0) {
        t = GC_new_thread(my_new_thread);
        t -> flags = my_flags;
        if (!(my_flags & DETACHED)) cond_init(&(t->join_cv), USYNC_THREAD, 0);
        t -> stack = stack;
        t -> stack_size = stack_size;
        if (new_thread != 0) *new_thread = my_new_thread;
        pthread_cond_signal(&GC_create_cv);
    } else {
	    if (!(my_flags & CLIENT_OWNS_STACK)) {
		    GC_stack_free(stack, stack_size);
	    }        
	    GC_multithreaded--;
    }
    UNLOCK();
    pthread_attr_destroy(&attr);
    return(result);
}

# else

#ifndef LINT
  int GC_no_sunOS_pthreads;
#endif

# endif /* GC_SOLARIS_PTHREADS */

