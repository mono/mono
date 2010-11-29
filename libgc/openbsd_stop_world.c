#include "private/pthread_support.h"

/* derived from pthread_stop_world.c */

# if defined(GC_OPENBSD_THREADS)

#define THREAD_EQUAL(id1, id2) pthread_equal(id1, id2)

/* We hold allocation lock.  Should do exactly the right thing if the	*/
/* world is stopped.  Should not fail if it isn't.			*/
void GC_push_all_stacks()
{
    GC_bool found_me = FALSE;
    size_t nthreads = 0;
    int i;
    GC_thread p;
    ptr_t lo, hi;
    pthread_t me = pthread_self();
    
    if (!GC_thr_initialized) GC_thr_init();
#   if DEBUG_THREADS
        GC_printf("Pushing stacks from thread 0x%x\n", (unsigned) me);
#   endif
    for (i = 0; i < THREAD_TABLE_SZ; i++) {
      for (p = GC_threads[i]; p != 0; p = p -> next) {
        if (p -> flags & FINISHED) continue;
	++nthreads;
        if (THREAD_EQUAL(p -> id, me)) {
#  	    ifdef SPARC
	        lo = (ptr_t)GC_save_regs_in_stack();
#  	    else
 	        lo = GC_approx_sp();
#           endif
	    found_me = TRUE;
	} else {
	    lo = p -> stop_info.stack_ptr;
	}
        if ((p -> flags & MAIN_THREAD) == 0) {
	    hi = p -> stack_end;
        } else {
            /* The original stack. */
            hi = GC_stackbottom;
        }
#	if DEBUG_THREADS
            GC_printf("Stack for thread 0x%x = [%p,%p)\n",
    	              (unsigned)(p -> id), lo, hi);
#	endif
	if (0 == lo) ABORT("GC_push_all_stacks: sp not set!\n");
#       ifdef STACK_GROWS_UP
	  /* We got them backwards! */
          GC_push_all_stack(hi, lo);
#       else
          GC_push_all_stack(lo, hi);
#	endif
      }
    }
    if (!found_me && !GC_in_thread_creation)
      ABORT("Collecting from unknown thread.");
}

/* We hold the allocation lock.  Suspend all threads that might	*/
/* still be running. */ 
void GC_suspend_all()
{
    int i;
    GC_thread p;
    int result;
    pthread_t my_thread = pthread_self();
    
    for (i = 0; i < THREAD_TABLE_SZ; i++) {
      for (p = GC_threads[i]; p != 0; p = p -> next) {
        if (!THREAD_EQUAL(p -> id, my_thread)) {
            if (p -> flags & FINISHED) continue;
	    if (p -> thread_blocked) /* Will wait */ continue;
#	    if DEBUG_THREADS
	      GC_printf("Suspending thread 0x%x\n",
			(unsigned)(p -> id));
#	    endif
        
            if (pthread_suspend_np(p -> id) != 0)
              ABORT("pthread_suspend_np failed");

	   /*
	    * This will only work for userland pthreads. It will
	    * fail badly on rthreads. Perhaps we should consider
	    * a pthread_sp_np() function that returns the stack
	    * pointer for a suspended thread and implement in
	    * both pthreads and rthreads.
	    */
	   p -> stop_info.stack_ptr = *(ptr_t*)((char *)p -> id + UTHREAD_SP_OFFSET);
        }
      }
    }
}

void GC_stop_world()
{
    int i;

    GC_ASSERT(I_HOLD_LOCK());
#   if DEBUG_THREADS
      GC_printf("Stopping the world from 0x%x\n", (unsigned)pthread_self());
#   endif
       
    /* Make sure all free list construction has stopped before we start. */
    /* No new construction can start, since free list construction is	*/
    /* required to acquire and release the GC lock before it starts,	*/
    /* and we have the lock.						*/
#   ifdef PARALLEL_MARK
      GC_acquire_mark_lock();
      GC_ASSERT(GC_fl_builder_count == 0);
      /* We should have previously waited for it to become zero. */
#   endif /* PARALLEL_MARK */

    GC_suspend_all();

#   ifdef PARALLEL_MARK
      GC_release_mark_lock();
#   endif
    #if DEBUG_THREADS
      GC_printf("World stopped from 0x%x\n", (unsigned)pthread_self());
    #endif
}

/* Caller holds allocation lock, and has held it continuously since	*/
/* the world stopped.							*/
void GC_start_world()
{
    pthread_t my_thread = pthread_self();
    register int i;
    register GC_thread p;
    register int result;

#   if DEBUG_THREADS
      GC_printf("World starting\n");
#   endif

    for (i = 0; i < THREAD_TABLE_SZ; i++) {
      for (p = GC_threads[i]; p != 0; p = p -> next) {
        if (!THREAD_EQUAL(p -> id, my_thread)) {
            if (p -> flags & FINISHED) continue;
	    if (p -> thread_blocked) continue;
	    #if DEBUG_THREADS
	      GC_printf("Resuming thread 0x%x\n",
			(unsigned)(p -> id));
	    #endif
        
            if (pthread_resume_np(p -> id) != 0)
              ABORT("pthread_kill failed");
        }
      }
    }
#    if DEBUG_THREADS
      GC_printf("World started\n");
#    endif
}

void GC_stop_init() {
}

#endif
