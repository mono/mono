#ifdef GC_SOLARIS_THREADS

/* The set of all known threads.  We intercept thread creation and     */
/* joins.  We never actually create detached threads.  We allocate all */
/* new thread stacks ourselves.  These allow us to maintain this       */
/* data structure.                                                     */
/* Protected by GC_thr_lock.                                           */
/* Some of this should be declared volatile, but that's incosnsistent  */
/* with some library routine declarations.  In particular, the 	       */
/* definition of cond_t doesn't mention volatile!                      */
  typedef struct GC_Thread_Rep {
    struct GC_Thread_Rep * next;
    thread_t id;
    word flags;
#      define FINISHED 1       /* Thread has exited.   */
#      define DETACHED 2       /* Thread is intended to be detached.   */
#      define CLIENT_OWNS_STACK        4
                               /* Stack was supplied by client.        */
#      define SUSPNDED 8       /* Currently suspended.			*/
    			       /* SUSPENDED is used insystem header.	*/
    ptr_t stack;
    size_t stack_size;
    cond_t join_cv;
    void * status;
  } * GC_thread;
  extern GC_thread GC_new_thread(thread_t id);

  extern GC_bool GC_thr_initialized;
  extern volatile GC_thread GC_threads[];
  extern size_t GC_min_stack_sz;
  extern size_t GC_page_sz;
  extern void GC_thr_init(void);
  extern ptr_t GC_stack_alloc(size_t * stack_size);
  extern void GC_stack_free(ptr_t stack, size_t size);

# endif /* GC_SOLARIS_THREADS */

