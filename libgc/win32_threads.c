#if defined(GC_WIN32_THREADS) 

#include "private/gc_priv.h"
#include <windows.h>

#ifdef CYGWIN32
# include <errno.h>

 /* Cygwin-specific forward decls */
# undef pthread_create 
# undef pthread_sigmask 
# undef pthread_join 
# undef dlopen 

# define DEBUG_CYGWIN_THREADS 0

  void * GC_start_routine(void * arg);
  void GC_thread_exit_proc(void *arg);

#endif

/* The type of the first argument to InterlockedExchange.	*/
/* Documented to be LONG volatile *, but at least gcc likes 	*/
/* this better.							*/
typedef LONG * IE_t;

#ifndef MAX_THREADS
# define MAX_THREADS 256
    /* FIXME:							*/
    /* Things may get quite slow for large numbers of threads,	*/
    /* since we look them up with sequential search.		*/
#endif

GC_bool GC_thr_initialized = FALSE;

DWORD GC_main_thread = 0;

struct GC_thread_Rep {
  LONG in_use; /* Updated without lock.	*/
  			/* We assert that unused 	*/
  			/* entries have invalid ids of	*/
  			/* zero and zero stack fields.  */
  DWORD id;
  HANDLE handle;
  ptr_t stack_base;	/* The cold end of the stack.   */
			/* 0 ==> entry not valid.	*/
			/* !in_use ==> stack_base == 0	*/
  GC_bool suspended;

# ifdef CYGWIN32
    void *status; /* hold exit value until join in case it's a pointer */
    pthread_t pthread_id;
    short flags;		/* Protected by GC lock.	*/
#	define FINISHED 1   	/* Thread has exited.	*/
#	define DETACHED 2	/* Thread is intended to be detached.	*/
# endif
};

typedef volatile struct GC_thread_Rep * GC_thread;

/*
 * We generally assume that volatile ==> memory ordering, at least among
 * volatiles.
 */

volatile GC_bool GC_please_stop = FALSE;

volatile struct GC_thread_Rep thread_table[MAX_THREADS];

volatile LONG GC_max_thread_index = 0; /* Largest index in thread_table	*/
				       /* that was ever used.		*/

extern LONG WINAPI GC_write_fault_handler(struct _EXCEPTION_POINTERS *exc_info);

/*
 * This may be called from DllMain, and hence operates under unusual
 * constraints.
 */
static GC_thread GC_new_thread(void) {
  int i;
  /* It appears to be unsafe to acquire a lock here, since this	*/
  /* code is apparently not preeemptible on some systems.	*/
  /* (This is based on complaints, not on Microsoft's official	*/
  /* documentation, which says this should perform "only simple	*/
  /* initialization tasks".)					*/
  /* Hence we make do with nonblocking synchronization.		*/

  /* The following should be a noop according to the win32	*/
  /* documentation.  There is empirical evidence that it	*/
  /* isn't.		- HB					*/
# if defined(MPROTECT_VDB)
   if (GC_incremental) SetUnhandledExceptionFilter(GC_write_fault_handler);
# endif
                /* cast away volatile qualifier */
  for (i = 0; InterlockedExchange((IE_t)&thread_table[i].in_use,1) != 0; i++) {
    /* Compare-and-swap would make this cleaner, but that's not 	*/
    /* supported before Windows 98 and NT 4.0.  In Windows 2000,	*/
    /* InterlockedExchange is supposed to be replaced by		*/
    /* InterlockedExchangePointer, but that's not really what I		*/
    /* want here.							*/
    if (i == MAX_THREADS - 1)
      ABORT("too many threads");
  }
  /* Update GC_max_thread_index if necessary.  The following is safe,	*/
  /* and unlike CompareExchange-based solutions seems to work on all	*/
  /* Windows95 and later platforms.					*/
  /* Unfortunately, GC_max_thread_index may be temporarily out of 	*/
  /* bounds, so readers have to compensate.				*/
  while (i > GC_max_thread_index) {
    InterlockedIncrement((IE_t)&GC_max_thread_index);
  }
  if (GC_max_thread_index >= MAX_THREADS) {
    /* We overshot due to simultaneous increments.	*/
    /* Setting it to MAX_THREADS-1 is always safe.	*/
    GC_max_thread_index = MAX_THREADS - 1;
  }
  
# ifdef CYGWIN32
    thread_table[i].pthread_id = pthread_self();
# endif
  if (!DuplicateHandle(GetCurrentProcess(),
	               GetCurrentThread(),
		       GetCurrentProcess(),
		       (HANDLE*)&thread_table[i].handle,
		       0,
		       0,
		       DUPLICATE_SAME_ACCESS)) {
	DWORD last_error = GetLastError();
	GC_printf1("Last error code: %lx\n", last_error);
	ABORT("DuplicateHandle failed");
  }
  thread_table[i].stack_base = GC_get_stack_base();
  /* Up until this point, GC_push_all_stacks considers this thread	*/
  /* invalid.								*/
  if (thread_table[i].stack_base == NULL) 
    ABORT("Failed to find stack base in GC_new_thread");
  /* Up until this point, this entry is viewed as reserved but invalid	*/
  /* by GC_delete_thread.						*/
  thread_table[i].id = GetCurrentThreadId();
  /* If this thread is being created while we are trying to stop	*/
  /* the world, wait here.  Hopefully this can't happen on any	*/
  /* systems that don't allow us to block here.			*/
  while (GC_please_stop) Sleep(20);
  return thread_table + i;
}

/*
 * GC_max_thread_index may temporarily be larger than MAX_THREADS.
 * To avoid subscript errors, we check on access.
 */
#ifdef __GNUC__
__inline__
#endif
LONG GC_get_max_thread_index()
{
  LONG my_max = GC_max_thread_index;

  if (my_max >= MAX_THREADS) return MAX_THREADS-1;
  return my_max;
}

/* This is intended to be lock-free, though that			*/
/* assumes that the CloseHandle becomes visible before the 		*/
/* in_use assignment.							*/
static void GC_delete_gc_thread(GC_thread thr)
{
    CloseHandle(thr->handle);
      /* cast away volatile qualifier */
    thr->stack_base = 0;
    thr->id = 0;
#   ifdef CYGWIN32
      thr->pthread_id = 0;
#   endif /* CYGWIN32 */
    thr->in_use = FALSE;
}

static void GC_delete_thread(DWORD thread_id) {
  int i;
  LONG my_max = GC_get_max_thread_index();

  for (i = 0;
       i <= my_max &&
       (!thread_table[i].in_use || thread_table[i].id != thread_id);
       /* Must still be in_use, since nobody else can store our thread_id. */
       i++) {}
  if (i > my_max) {
    WARN("Removing nonexisiting thread %ld\n", (GC_word)thread_id);
  } else {
    GC_delete_gc_thread(thread_table+i);
  }
}


#ifdef CYGWIN32

/* Return a GC_thread corresponding to a given pthread_t.	*/
/* Returns 0 if it's not there.					*/
/* We assume that this is only called for pthread ids that	*/
/* have not yet terminated or are still joinable.		*/
static GC_thread GC_lookup_thread(pthread_t id)
{
  int i;
  LONG my_max = GC_get_max_thread_index();

  for (i = 0;
       i <= my_max &&
       (!thread_table[i].in_use || thread_table[i].pthread_id != id
	|| !thread_table[i].in_use);
       /* Must still be in_use, since nobody else can store our thread_id. */
       i++);
  if (i > my_max) return 0;
  return thread_table + i;
}

#endif /* CYGWIN32 */

void GC_push_thread_structures GC_PROTO((void))
{
    /* Unlike the other threads implementations, the thread table here	*/
    /* contains no pointers to the collectable heap.  Thus we have	*/
    /* no private structures we need to preserve.			*/
# ifdef CYGWIN32
  { int i; /* pthreads may keep a pointer in the thread exit value */
    LONG my_max = GC_get_max_thread_index();

    for (i = 0; i <= my_max; i++)
      if (thread_table[i].in_use)
	GC_push_all((ptr_t)&(thread_table[i].status),
                    (ptr_t)(&(thread_table[i].status)+1));
  }
# endif
}

void GC_stop_world()
{
  DWORD thread_id = GetCurrentThreadId();
  int i;

  if (!GC_thr_initialized) ABORT("GC_stop_world() called before GC_thr_init()");

  GC_please_stop = TRUE;
  for (i = 0; i <= GC_get_max_thread_index(); i++)
    if (thread_table[i].stack_base != 0
	&& thread_table[i].id != thread_id) {
#     ifdef MSWINCE
        /* SuspendThread will fail if thread is running kernel code */
	while (SuspendThread(thread_table[i].handle) == (DWORD)-1)
	  Sleep(10);
#     else
	/* Apparently the Windows 95 GetOpenFileName call creates	*/
	/* a thread that does not properly get cleaned up, and		*/
	/* SuspendThread on its descriptor may provoke a crash.		*/
	/* This reduces the probability of that event, though it still	*/
	/* appears there's a race here.					*/
	DWORD exitCode; 
	if (GetExitCodeThread(thread_table[i].handle,&exitCode) &&
            exitCode != STILL_ACTIVE) {
          thread_table[i].stack_base = 0; /* prevent stack from being pushed */
#         ifndef CYGWIN32
            /* this breaks pthread_join on Cygwin, which is guaranteed to  */
	    /* only see user pthreads 					   */
	    thread_table[i].in_use = FALSE;
	    CloseHandle(thread_table[i].handle);
#         endif
	  continue;
	}
	if (SuspendThread(thread_table[i].handle) == (DWORD)-1)
	  ABORT("SuspendThread failed");
#     endif
      thread_table[i].suspended = TRUE;
    }
}

void GC_start_world()
{
  DWORD thread_id = GetCurrentThreadId();
  int i;
  LONG my_max = GC_get_max_thread_index();

  for (i = 0; i <= my_max; i++)
    if (thread_table[i].stack_base != 0 && thread_table[i].suspended
	&& thread_table[i].id != thread_id) {
      if (ResumeThread(thread_table[i].handle) == (DWORD)-1)
	ABORT("ResumeThread failed");
      thread_table[i].suspended = FALSE;
    }
  GC_please_stop = FALSE;
}

# ifdef _MSC_VER
#   pragma warning(disable:4715)
# endif
ptr_t GC_current_stackbottom()
{
  DWORD thread_id = GetCurrentThreadId();
  int i;
  LONG my_max = GC_get_max_thread_index();

  for (i = 0; i <= my_max; i++)
    if (thread_table[i].stack_base && thread_table[i].id == thread_id)
      return thread_table[i].stack_base;
  ABORT("no thread table entry for current thread");
}
# ifdef _MSC_VER
#   pragma warning(default:4715)
# endif

# ifdef MSWINCE
    /* The VirtualQuery calls below won't work properly on WinCE, but	*/
    /* since each stack is restricted to an aligned 64K region of	*/
    /* virtual memory we can just take the next lowest multiple of 64K.	*/
#   define GC_get_stack_min(s) \
        ((ptr_t)(((DWORD)(s) - 1) & 0xFFFF0000))
# else
    static ptr_t GC_get_stack_min(ptr_t s)
    {
	ptr_t bottom;
	MEMORY_BASIC_INFORMATION info;
	VirtualQuery(s, &info, sizeof(info));
	do {
	    bottom = info.BaseAddress;
	    VirtualQuery(bottom - 1, &info, sizeof(info));
	} while ((info.Protect & PAGE_READWRITE)
		 && !(info.Protect & PAGE_GUARD));
	return(bottom);
    }
# endif

void GC_push_all_stacks()
{
  DWORD thread_id = GetCurrentThreadId();
  GC_bool found_me = FALSE;
  int i;
  int dummy;
  ptr_t sp, stack_min;
  GC_thread thread;
  LONG my_max = GC_get_max_thread_index();
  
  for (i = 0; i <= my_max; i++) {
    thread = thread_table + i;
    if (thread -> in_use && thread -> stack_base) {
      if (thread -> id == thread_id) {
	sp = (ptr_t) &dummy;
	found_me = TRUE;
      } else {
        CONTEXT context;
        context.ContextFlags = CONTEXT_INTEGER|CONTEXT_CONTROL;
        if (!GetThreadContext(thread_table[i].handle, &context))
	  ABORT("GetThreadContext failed");

        /* Push all registers that might point into the heap.  Frame	*/
        /* pointer registers are included in case client code was	*/
        /* compiled with the 'omit frame pointer' optimisation.		*/
#       define PUSH1(reg) GC_push_one((word)context.reg)
#       define PUSH2(r1,r2) PUSH1(r1), PUSH1(r2)
#       define PUSH4(r1,r2,r3,r4) PUSH2(r1,r2), PUSH2(r3,r4)
#       if defined(I386)
          PUSH4(Edi,Esi,Ebx,Edx), PUSH2(Ecx,Eax), PUSH1(Ebp);
	  sp = (ptr_t)context.Esp;
#       elif defined(ARM32)
	  PUSH4(R0,R1,R2,R3),PUSH4(R4,R5,R6,R7),PUSH4(R8,R9,R10,R11),PUSH1(R12);
	  sp = (ptr_t)context.Sp;
#       elif defined(SHx)
	  PUSH4(R0,R1,R2,R3), PUSH4(R4,R5,R6,R7), PUSH4(R8,R9,R10,R11);
	  PUSH2(R12,R13), PUSH1(R14);
	  sp = (ptr_t)context.R15;
#       elif defined(MIPS)
	  PUSH4(IntAt,IntV0,IntV1,IntA0), PUSH4(IntA1,IntA2,IntA3,IntT0);
	  PUSH4(IntT1,IntT2,IntT3,IntT4), PUSH4(IntT5,IntT6,IntT7,IntS0);
	  PUSH4(IntS1,IntS2,IntS3,IntS4), PUSH4(IntS5,IntS6,IntS7,IntT8);
	  PUSH4(IntT9,IntK0,IntK1,IntS8);
	  sp = (ptr_t)context.IntSp;
#       elif defined(PPC)
	  PUSH4(Gpr0, Gpr3, Gpr4, Gpr5),  PUSH4(Gpr6, Gpr7, Gpr8, Gpr9);
	  PUSH4(Gpr10,Gpr11,Gpr12,Gpr14), PUSH4(Gpr15,Gpr16,Gpr17,Gpr18);
	  PUSH4(Gpr19,Gpr20,Gpr21,Gpr22), PUSH4(Gpr23,Gpr24,Gpr25,Gpr26);
	  PUSH4(Gpr27,Gpr28,Gpr29,Gpr30), PUSH1(Gpr31);
	  sp = (ptr_t)context.Gpr1;
#       elif defined(ALPHA)
	  PUSH4(IntV0,IntT0,IntT1,IntT2), PUSH4(IntT3,IntT4,IntT5,IntT6);
	  PUSH4(IntT7,IntS0,IntS1,IntS2), PUSH4(IntS3,IntS4,IntS5,IntFp);
	  PUSH4(IntA0,IntA1,IntA2,IntA3), PUSH4(IntA4,IntA5,IntT8,IntT9);
	  PUSH4(IntT10,IntT11,IntT12,IntAt);
	  sp = (ptr_t)context.IntSp;
#       else
#         error "architecture is not supported"
#       endif
      }

      stack_min = GC_get_stack_min(thread->stack_base);

      if (sp >= stack_min && sp < thread->stack_base)
        GC_push_all_stack(sp, thread->stack_base);
      else {
        WARN("Thread stack pointer 0x%lx out of range, pushing everything\n",
	     (unsigned long)sp);
        GC_push_all_stack(stack_min, thread->stack_base);
      }
    }
  }
  if (!found_me) ABORT("Collecting from unknown thread.");
}

void GC_get_next_stack(char *start, char **lo, char **hi)
{
    int i;
#   define ADDR_LIMIT (char *)(-1L)
    char * current_min = ADDR_LIMIT;
    LONG my_max = GC_get_max_thread_index();
  
    for (i = 0; i <= my_max; i++) {
    	char * s = (char *)thread_table[i].stack_base;

	if (0 != s && s > start && s < current_min) {
	    current_min = s;
	}
    }
    *hi = current_min;
    if (current_min == ADDR_LIMIT) {
    	*lo = ADDR_LIMIT;
	return;
    }
    *lo = GC_get_stack_min(current_min);
    if (*lo < start) *lo = start;
}

#if !defined(CYGWIN32)

#if !defined(MSWINCE) && defined(GC_DLL)

/* We register threads from DllMain */

GC_API HANDLE WINAPI GC_CreateThread(
    LPSECURITY_ATTRIBUTES lpThreadAttributes, 
    DWORD dwStackSize, LPTHREAD_START_ROUTINE lpStartAddress, 
    LPVOID lpParameter, DWORD dwCreationFlags, LPDWORD lpThreadId )
{
    return CreateThread(lpThreadAttributes, dwStackSize, lpStartAddress,
                        lpParameter, dwCreationFlags, lpThreadId);
}

#else /* defined(MSWINCE) || !defined(GC_DLL))  */

/* We have no DllMain to take care of new threads.  Thus we	*/
/* must properly intercept thread creation.			*/

typedef struct {
    LPTHREAD_START_ROUTINE start;
    LPVOID param;
} thread_args;

static DWORD WINAPI thread_start(LPVOID arg);

GC_API HANDLE WINAPI GC_CreateThread(
    LPSECURITY_ATTRIBUTES lpThreadAttributes, 
    DWORD dwStackSize, LPTHREAD_START_ROUTINE lpStartAddress, 
    LPVOID lpParameter, DWORD dwCreationFlags, LPDWORD lpThreadId )
{
    HANDLE thread_h = NULL;

    thread_args *args;

    if (!GC_is_initialized) GC_init();
    		/* make sure GC is initialized (i.e. main thread is attached) */
    
    args = GC_malloc_uncollectable(sizeof(thread_args)); 
	/* Handed off to and deallocated by child thread.	*/
    if (0 == args) {
	SetLastError(ERROR_NOT_ENOUGH_MEMORY);
        return NULL;
    }

    /* set up thread arguments */
    	args -> start = lpStartAddress;
    	args -> param = lpParameter;

    thread_h = CreateThread(lpThreadAttributes,
    			    dwStackSize, thread_start,
    			    args, dwCreationFlags,
    			    lpThreadId);

    return thread_h;
}

static DWORD WINAPI thread_start(LPVOID arg)
{
    DWORD ret = 0;
    thread_args *args = (thread_args *)arg;

    GC_new_thread();

    /* Clear the thread entry even if we exit with an exception.	*/
    /* This is probably pointless, since an uncaught exception is	*/
    /* supposed to result in the process being killed.			*/
#ifndef __GNUC__
    __try {
#endif /* __GNUC__ */
	ret = args->start (args->param);
#ifndef __GNUC__
    } __finally {
#endif /* __GNUC__ */
	GC_free(args);
	GC_delete_thread(GetCurrentThreadId());
#ifndef __GNUC__
    }
#endif /* __GNUC__ */

    return ret;
}
#endif /* !defined(MSWINCE) && !(defined(__MINGW32__) && !defined(_DLL))  */

#endif /* !CYGWIN32 */

#ifdef MSWINCE

typedef struct {
    HINSTANCE hInstance;
    HINSTANCE hPrevInstance;
    LPWSTR lpCmdLine;
    int nShowCmd;
} main_thread_args;

DWORD WINAPI main_thread_start(LPVOID arg);

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance,
		   LPWSTR lpCmdLine, int nShowCmd)
{
    DWORD exit_code = 1;

    main_thread_args args = {
	hInstance, hPrevInstance, lpCmdLine, nShowCmd
    };
    HANDLE thread_h;
    DWORD thread_id;

    /* initialize everything */
    GC_init();

    /* start the main thread */
    thread_h = GC_CreateThread(
	NULL, 0, main_thread_start, &args, 0, &thread_id);

    if (thread_h != NULL)
    {
	WaitForSingleObject (thread_h, INFINITE);
	GetExitCodeThread (thread_h, &exit_code);
	CloseHandle (thread_h);
    }

    GC_deinit();
    DeleteCriticalSection(&GC_allocate_ml);

    return (int) exit_code;
}

DWORD WINAPI main_thread_start(LPVOID arg)
{
    main_thread_args * args = (main_thread_args *) arg;

    return (DWORD) GC_WinMain (args->hInstance, args->hPrevInstance,
			       args->lpCmdLine, args->nShowCmd);
}

# else /* !MSWINCE */

/* Called by GC_init() - we hold the allocation lock.	*/
void GC_thr_init() {
    if (GC_thr_initialized) return;
    GC_main_thread = GetCurrentThreadId();
    GC_thr_initialized = TRUE;

    /* Add the initial thread, so we can stop it.	*/
    GC_new_thread();
}

#ifdef CYGWIN32

struct start_info {
    void *(*start_routine)(void *);
    void *arg;
    GC_bool detached;
};

int GC_pthread_join(pthread_t pthread_id, void **retval) {
    int result;
    int i;
    GC_thread me;

#   if DEBUG_CYGWIN_THREADS
      GC_printf3("thread 0x%x(0x%x) is joining thread 0x%x.\n",
		 (int)pthread_self(), GetCurrentThreadId(), (int)pthread_id);
#   endif

    /* Thread being joined might not have registered itself yet. */
    /* After the join,thread id may have been recycled.		 */
    /* FIXME: It would be better if this worked more like	 */
    /* pthread_support.c.					 */

    while ((me = GC_lookup_thread(pthread_id)) == 0) Sleep(10);

    result = pthread_join(pthread_id, retval);

    GC_delete_gc_thread(me);

#   if DEBUG_CYGWIN_THREADS
      GC_printf3("thread 0x%x(0x%x) completed join with thread 0x%x.\n",
		 (int)pthread_self(), GetCurrentThreadId(), (int)pthread_id);
#   endif

    return result;
}

/* Cygwin-pthreads calls CreateThread internally, but it's not
 * easily interceptible by us..
 *   so intercept pthread_create instead
 */
int
GC_pthread_create(pthread_t *new_thread,
		  const pthread_attr_t *attr,
                  void *(*start_routine)(void *), void *arg) {
    int result;
    struct start_info * si;

    if (!GC_is_initialized) GC_init();
    		/* make sure GC is initialized (i.e. main thread is attached) */
    
    /* This is otherwise saved only in an area mmapped by the thread */
    /* library, which isn't visible to the collector.		 */
    si = GC_malloc_uncollectable(sizeof(struct start_info)); 
    if (0 == si) return(EAGAIN);

    si -> start_routine = start_routine;
    si -> arg = arg;
    if (attr != 0 &&
        pthread_attr_getdetachstate(attr, &si->detached)
	== PTHREAD_CREATE_DETACHED) {
      si->detached = TRUE;
    }

#   if DEBUG_CYGWIN_THREADS
      GC_printf2("About to create a thread from 0x%x(0x%x)\n",
		 (int)pthread_self(), GetCurrentThreadId);
#   endif
    result = pthread_create(new_thread, attr, GC_start_routine, si); 

    if (result) { /* failure */
      	GC_free(si);
    } 

    return(result);
}

void * GC_start_routine(void * arg)
{
    struct start_info * si = arg;
    void * result;
    void *(*start)(void *);
    void *start_arg;
    pthread_t pthread_id;
    GC_thread me;
    GC_bool detached;
    int i;

#   if DEBUG_CYGWIN_THREADS
      GC_printf2("thread 0x%x(0x%x) starting...\n",(int)pthread_self(),
		      				   GetCurrentThreadId());
#   endif

    /* If a GC occurs before the thread is registered, that GC will	*/
    /* ignore this thread.  That's fine, since it will block trying to  */
    /* acquire the allocation lock, and won't yet hold interesting 	*/
    /* pointers.							*/
    LOCK();
    /* We register the thread here instead of in the parent, so that	*/
    /* we don't need to hold the allocation lock during pthread_create. */
    me = GC_new_thread();
    UNLOCK();

    start = si -> start_routine;
    start_arg = si -> arg;
    if (si-> detached) me -> flags |= DETACHED;
    me -> pthread_id = pthread_id = pthread_self();

    GC_free(si); /* was allocated uncollectable */

    pthread_cleanup_push(GC_thread_exit_proc, (void *)me);
    result = (*start)(start_arg);
    me -> status = result;
    pthread_cleanup_pop(0);

#   if DEBUG_CYGWIN_THREADS
      GC_printf2("thread 0x%x(0x%x) returned from start routine.\n",
		 (int)pthread_self(),GetCurrentThreadId());
#   endif

    return(result);
}

void GC_thread_exit_proc(void *arg)
{
    GC_thread me = (GC_thread)arg;
    int i;

#   if DEBUG_CYGWIN_THREADS
      GC_printf2("thread 0x%x(0x%x) called pthread_exit().\n",
		 (int)pthread_self(),GetCurrentThreadId());
#   endif

    LOCK();
    if (me -> flags & DETACHED) {
      GC_delete_thread(GetCurrentThreadId());
    } else {
      /* deallocate it as part of join */
      me -> flags |= FINISHED;
    }
    UNLOCK();
}

/* nothing required here... */
int GC_pthread_sigmask(int how, const sigset_t *set, sigset_t *oset) {
  return pthread_sigmask(how, set, oset);
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
        GC_delete_gc_thread(thread_gc_id);
      }
      UNLOCK();
    }
    return result;
}

#else /* !CYGWIN32 */

/*
 * We avoid acquiring locks here, since this doesn't seem to be preemptable.
 * Pontus Rydin suggests wrapping the thread start routine instead.
 */
#ifdef GC_DLL
BOOL WINAPI DllMain(HINSTANCE inst, ULONG reason, LPVOID reserved)
{
  switch (reason) {
  case DLL_PROCESS_ATTACH:
    GC_init();	/* Force initialization before thread attach.	*/
    /* fall through */
  case DLL_THREAD_ATTACH:
    GC_ASSERT(GC_thr_initialized);
    if (GC_main_thread != GetCurrentThreadId()) {
        GC_new_thread();
    } /* o.w. we already did it during GC_thr_init(), called by GC_init() */
    break;

  case DLL_THREAD_DETACH:
    GC_delete_thread(GetCurrentThreadId());
    break;

  case DLL_PROCESS_DETACH:
    {
      int i;

      LOCK();
      for (i = 0; i <= GC_get_max_thread_index(); ++i)
      {
          if (thread_table[i].in_use)
	    GC_delete_gc_thread(thread_table + i);
      }
      UNLOCK();

      GC_deinit();
      DeleteCriticalSection(&GC_allocate_ml);
    }
    break;

  }
  return TRUE;
}
#endif /* GC_DLL */
#endif /* !CYGWIN32 */

# endif /* !MSWINCE */

#endif /* GC_WIN32_THREADS */
