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
 */
/* Boehm, September 14, 1994 4:44 pm PDT */

# if defined(GC_SOLARIS_THREADS) || defined(GC_SOLARIS_PTHREADS)

# include "private/gc_priv.h"
# include "private/solaris_threads.h"
# include <thread.h>
# include <synch.h>
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

#ifdef HANDLE_FORK
  --> Not yet supported.  Try porting the code from linux_threads.c.
#endif

/*
 * This is the default size of the LWP arrays. If there are more LWPs
 * than this when a stop-the-world GC happens, set_max_lwps will be
 * called to cope.
 * This must be higher than the number of LWPs at startup time.
 * The threads library creates a thread early on, so the min. is 3
 */
# define DEFAULT_MAX_LWPS	4

#undef thr_join
#undef thr_create
#undef thr_suspend
#undef thr_continue

cond_t GC_prom_join_cv;		/* Broadcast when any thread terminates	*/
cond_t GC_create_cv;		/* Signalled when a new undetached	*/
				/* thread starts.			*/
				

#ifdef MMAP_STACKS
static int GC_zfd;
#endif /* MMAP_STACKS */

/* We use the allocation lock to protect thread-related data structures. */

/* We stop the world using /proc primitives.  This makes some	*/
/* minimal assumptions about the threads implementation.	*/
/* We don't play by the rules, since the rules make this	*/
/* impossible (as of Solaris 2.3).  Also note that as of	*/
/* Solaris 2.3 the various thread and lwp suspension		*/
/* primitives failed to stop threads by the time the request	*/
/* is completed.						*/


static sigset_t old_mask;

/* Sleep for n milliseconds, n < 1000	*/
void GC_msec_sleep(int n)
{
    struct timespec ts;
                            
    ts.tv_sec = 0;
    ts.tv_nsec = 1000000*n;
    if (syscall(SYS_nanosleep, &ts, 0) < 0) {
	ABORT("nanosleep failed");
    }
}
/* Turn off preemption;  gross but effective.  		*/
/* Caller has allocation lock.				*/
/* Actually this is not needed under Solaris 2.3 and	*/
/* 2.4, but hopefully that'll change.			*/
void preempt_off()
{
    sigset_t set;

    (void)sigfillset(&set);
    sigdelset(&set, SIGABRT);
    syscall(SYS_sigprocmask, SIG_SETMASK, &set, &old_mask);
}

void preempt_on()
{
    syscall(SYS_sigprocmask, SIG_SETMASK, &old_mask, NULL);
}

int GC_main_proc_fd = -1;


struct lwp_cache_entry {
    lwpid_t lc_id;
    int lc_descr;	/* /proc file descriptor.	*/
}  GC_lwp_cache_default[DEFAULT_MAX_LWPS];

static int max_lwps = DEFAULT_MAX_LWPS;
static struct lwp_cache_entry *GC_lwp_cache = GC_lwp_cache_default;

static prgregset_t GC_lwp_registers_default[DEFAULT_MAX_LWPS];
static prgregset_t *GC_lwp_registers = GC_lwp_registers_default;

/* Return a file descriptor for the /proc entry corresponding	*/
/* to the given lwp.  The file descriptor may be stale if the	*/
/* lwp exited and a new one was forked.				*/
static int open_lwp(lwpid_t id)
{
    int result;
    static int next_victim = 0;
    register int i;
    
    for (i = 0; i < max_lwps; i++) {
    	if (GC_lwp_cache[i].lc_id == id) return(GC_lwp_cache[i].lc_descr);
    }
    result = syscall(SYS_ioctl, GC_main_proc_fd, PIOCOPENLWP, &id);
    /*
     * If PIOCOPENLWP fails, try closing fds in the cache until it succeeds.
     */
    if (result < 0 && errno == EMFILE) {
	    for (i = 0; i < max_lwps; i++) {
		if (GC_lwp_cache[i].lc_id != 0) {
        		(void)syscall(SYS_close, GC_lwp_cache[i].lc_descr);
			result = syscall(SYS_ioctl, GC_main_proc_fd, PIOCOPENLWP, &id);
			if (result >= 0 || (result < 0 && errno != EMFILE))
				break;
		}
	    }
    }
    if (result < 0) {
	if (errno == EMFILE) {
		ABORT("Too many open files");
	}
        return(-1) /* exited? */;
    }
    if (GC_lwp_cache[next_victim].lc_id != 0)
        (void)syscall(SYS_close, GC_lwp_cache[next_victim].lc_descr);
    GC_lwp_cache[next_victim].lc_id = id;
    GC_lwp_cache[next_victim].lc_descr = result;
    if (++next_victim >= max_lwps)
	next_victim = 0;
    return(result);
}

static void uncache_lwp(lwpid_t id)
{
    register int i;
    
    for (i = 0; i < max_lwps; i++) {
    	if (GC_lwp_cache[i].lc_id == id) {
    	    (void)syscall(SYS_close, GC_lwp_cache[id].lc_descr);
    	    GC_lwp_cache[i].lc_id = 0;
    	    break;
    	}
    }
}
	/* Sequence of current lwp ids	*/
static lwpid_t GC_current_ids_default[DEFAULT_MAX_LWPS + 1];
static lwpid_t *GC_current_ids = GC_current_ids_default;

	/* Temporary used below (can be big if large number of LWPs) */
static lwpid_t last_ids_default[DEFAULT_MAX_LWPS + 1];
static lwpid_t *last_ids = last_ids_default;


#define ROUNDUP(n)    WORDS_TO_BYTES(ROUNDED_UP_WORDS(n))

static void set_max_lwps(GC_word n)
{
    char *mem;
    char *oldmem;
    int required_bytes = ROUNDUP(n * sizeof(struct lwp_cache_entry))
	+ ROUNDUP(n * sizeof(prgregset_t))
	+ ROUNDUP((n + 1) * sizeof(lwpid_t))
	+ ROUNDUP((n + 1) * sizeof(lwpid_t));

    GC_expand_hp_inner(divHBLKSZ((word)required_bytes));
    oldmem = mem = GC_scratch_alloc(required_bytes);
    if (0 == mem) ABORT("No space for lwp data structures");

    /*
     * We can either flush the old lwp cache or copy it over. Do the latter.
     */
    memcpy(mem, GC_lwp_cache, max_lwps * sizeof(struct lwp_cache_entry));
    GC_lwp_cache = (struct lwp_cache_entry*)mem;
    mem += ROUNDUP(n * sizeof(struct lwp_cache_entry));

    BZERO(GC_lwp_registers, max_lwps * sizeof(GC_lwp_registers[0]));
    GC_lwp_registers = (prgregset_t *)mem;
    mem += ROUNDUP(n * sizeof(prgregset_t));


    GC_current_ids = (lwpid_t *)mem;
    mem += ROUNDUP((n + 1) * sizeof(lwpid_t));

    last_ids = (lwpid_t *)mem;
    mem += ROUNDUP((n + 1)* sizeof(lwpid_t));

    if (mem > oldmem + required_bytes)
	ABORT("set_max_lwps buffer overflow");

    max_lwps = n;
}


/* Stop all lwps in process.  Assumes preemption is off.	*/
/* Caller has allocation lock (and any other locks he may	*/
/* need).							*/
static void stop_all_lwps()
{
    int lwp_fd;
    char buf[30];
    prstatus_t status;
    register int i;
    GC_bool changed;
    lwpid_t me = _lwp_self();

    if (GC_main_proc_fd == -1) {
    	sprintf(buf, "/proc/%d", getpid());
    	GC_main_proc_fd = syscall(SYS_open, buf, O_RDONLY);
        if (GC_main_proc_fd < 0) {
		if (errno == EMFILE)
			ABORT("/proc open failed: too many open files");
		GC_printf1("/proc open failed: errno %d", errno);
		abort();
        }
    }
    BZERO(GC_lwp_registers, sizeof (prgregset_t) * max_lwps);
    for (i = 0; i < max_lwps; i++)
	last_ids[i] = 0;
    for (;;) {
    if (syscall(SYS_ioctl, GC_main_proc_fd, PIOCSTATUS, &status) < 0)
    	ABORT("Main PIOCSTATUS failed");
    	if (status.pr_nlwp < 1)
    		ABORT("Invalid number of lwps returned by PIOCSTATUS");
    	if (status.pr_nlwp >= max_lwps) {
    		set_max_lwps(status.pr_nlwp*2 + 10);
		/*
		 * The data in the old GC_current_ids and
		 * GC_lwp_registers has been trashed. Cleaning out last_ids
		 * will make sure every LWP gets re-examined.
		 */
        	for (i = 0; i < max_lwps; i++)
			last_ids[i] = 0;
		continue;
    }
        if (syscall(SYS_ioctl, GC_main_proc_fd, PIOCLWPIDS, GC_current_ids) < 0)
            ABORT("PIOCLWPIDS failed");
        changed = FALSE;
        for (i = 0; GC_current_ids[i] != 0 && i < max_lwps; i++) {
            if (GC_current_ids[i] != last_ids[i]) {
                changed = TRUE;
                if (GC_current_ids[i] != me) {
		    /* PIOCSTOP doesn't work without a writable		*/
		    /* descriptor.  And that makes the process		*/
		    /* undebuggable.					*/
                    if (_lwp_suspend(GC_current_ids[i]) < 0) {
                        /* Could happen if the lwp exited */
                        uncache_lwp(GC_current_ids[i]);
                        GC_current_ids[i] = me; /* ignore */
                    }
                }
            }
        }
        /*
         * In the unlikely event something does a fork between the
	 * PIOCSTATUS and the PIOCLWPIDS. 
         */
        if (i >= max_lwps)
		continue;
        /* All lwps in GC_current_ids != me have been suspended.  Note	*/
        /* that _lwp_suspend is idempotent.				*/
        for (i = 0; GC_current_ids[i] != 0; i++) {
            if (GC_current_ids[i] != last_ids[i]) {
                if (GC_current_ids[i] != me) {
                    lwp_fd = open_lwp(GC_current_ids[i]);
		    if (lwp_fd == -1)
		    {
			    GC_current_ids[i] = me;
			    continue;
		    }
		    /* LWP should be stopped.  Empirically it sometimes	*/
		    /* isn't, and more frequently the PR_STOPPED flag	*/
		    /* is not set.  Wait for PR_STOPPED.		*/
                    if (syscall(SYS_ioctl, lwp_fd,
                                PIOCSTATUS, &status) < 0) {
			/* Possible if the descriptor was stale, or */
			/* we encountered the 2.3 _lwp_suspend bug. */
			uncache_lwp(GC_current_ids[i]);
                        GC_current_ids[i] = me; /* handle next time. */
                    } else {
                        while (!(status.pr_flags & PR_STOPPED)) {
                            GC_msec_sleep(1);
			    if (syscall(SYS_ioctl, lwp_fd,
				    	PIOCSTATUS, &status) < 0) {
                            	ABORT("Repeated PIOCSTATUS failed");
			    }
			    if (status.pr_flags & PR_STOPPED) break;
			    
			    GC_msec_sleep(20);
			    if (syscall(SYS_ioctl, lwp_fd,
				    	PIOCSTATUS, &status) < 0) {
                            	ABORT("Repeated PIOCSTATUS failed");
			    }
                        }
                        if (status.pr_who !=  GC_current_ids[i]) {
				/* can happen if thread was on death row */
				uncache_lwp(GC_current_ids[i]);
				GC_current_ids[i] = me; /* handle next time. */
				continue;	
                        }
                        /* Save registers where collector can */
			/* find them.			  */
			    BCOPY(status.pr_reg, GC_lwp_registers[i],
				  sizeof (prgregset_t));
                    }
                }
            }
        }
        if (!changed) break;
        for (i = 0; i < max_lwps; i++) last_ids[i] = GC_current_ids[i];
    }
}

/* Restart all lwps in process.  Assumes preemption is off.	*/
static void restart_all_lwps()
{
    int lwp_fd;
    register int i;
    GC_bool changed;
    lwpid_t me = _lwp_self();
#   define PARANOID

    for (i = 0; GC_current_ids[i] != 0; i++) {
#	ifdef PARANOID
	  if (GC_current_ids[i] != me) {
	    int lwp_fd = open_lwp(GC_current_ids[i]);
	    prstatus_t status;
	    
	    if (lwp_fd < 0) ABORT("open_lwp failed");
	    if (syscall(SYS_ioctl, lwp_fd,
			PIOCSTATUS, &status) < 0) {
                ABORT("PIOCSTATUS failed in restart_all_lwps");
	    }
	    if (memcmp(status.pr_reg, GC_lwp_registers[i],
		       sizeof (prgregset_t)) != 0) {
		    int j;

		    for(j = 0; j < NPRGREG; j++)
		    {
			    GC_printf3("%i: %x -> %x\n", j,
				       GC_lwp_registers[i][j],
				       status.pr_reg[j]);
		    }
		ABORT("Register contents changed");
	    }
	    if (!status.pr_flags & PR_STOPPED) {
	    	ABORT("lwp no longer stopped");
	    }
#ifdef SPARC
	    {
		    gwindows_t windows;
	      if (syscall(SYS_ioctl, lwp_fd,
			PIOCGWIN, &windows) < 0) {
                ABORT("PIOCSTATUS failed in restart_all_lwps");
	      }
	      if (windows.wbcnt > 0) ABORT("unsaved register windows");
	    }
#endif
	  }
#	endif /* PARANOID */
	if (GC_current_ids[i] == me) continue;
        if (_lwp_continue(GC_current_ids[i]) < 0) {
            ABORT("Failed to restart lwp");
        }
    }
    if (i >= max_lwps) ABORT("Too many lwps");
}

GC_bool GC_multithreaded = 0;

void GC_stop_world()
{
    preempt_off();
    if (GC_multithreaded)
        stop_all_lwps();
}

void GC_start_world()
{
    if (GC_multithreaded)
        restart_all_lwps();
    preempt_on();
}

void GC_thr_init(void);

GC_bool GC_thr_initialized = FALSE;

size_t GC_min_stack_sz;


/*
 * stack_head is stored at the top of free stacks
 */
struct stack_head {
	struct stack_head	*next;
	ptr_t			base;
	thread_t		owner;
};

# define N_FREE_LISTS 25
struct stack_head *GC_stack_free_lists[N_FREE_LISTS] = { 0 };
		/* GC_stack_free_lists[i] is free list for stacks of 	*/
		/* size GC_min_stack_sz*2**i.				*/
		/* Free lists are linked through stack_head stored	*/			/* at top of stack.					*/

/* Return a stack of size at least *stack_size.  *stack_size is	*/
/* replaced by the actual stack size.				*/
/* Caller holds allocation lock.				*/
ptr_t GC_stack_alloc(size_t * stack_size)
{
    register size_t requested_sz = *stack_size;
    register size_t search_sz = GC_min_stack_sz;
    register int index = 0;	/* = log2(search_sz/GC_min_stack_sz) */
    register ptr_t base;
    register struct stack_head *result;
    
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
        base =  GC_stack_free_lists[index]->base;
        GC_stack_free_lists[index] = GC_stack_free_lists[index]->next;
    } else {
#ifdef MMAP_STACKS
        base = (ptr_t)mmap(0, search_sz + GC_page_size,
			     PROT_READ|PROT_WRITE, MAP_PRIVATE |MAP_NORESERVE,
			     GC_zfd, 0);
	if (base == (ptr_t)-1)
	{
		*stack_size = 0;
		return NULL;
	}

	mprotect(base, GC_page_size, PROT_NONE);
	/* Should this use divHBLKSZ(search_sz + GC_page_size) ? -- cf */
	GC_is_fresh((struct hblk *)base, divHBLKSZ(search_sz));
	base += GC_page_size;

#else
        base = (ptr_t) GC_scratch_alloc(search_sz + 2*GC_page_size);
	if (base == NULL)
	{
		*stack_size = 0;
		return NULL;
	}

        base = (ptr_t)(((word)base + GC_page_size) & ~(GC_page_size - 1));
        /* Protect hottest page to detect overflow. */
#	ifdef SOLARIS23_MPROTECT_BUG_FIXED
            mprotect(base, GC_page_size, PROT_NONE);
#	endif
        GC_is_fresh((struct hblk *)base, divHBLKSZ(search_sz));

        base += GC_page_size;
#endif
    }
    *stack_size = search_sz;
    return(base);
}

/* Caller holds  allocationlock.					*/
void GC_stack_free(ptr_t stack, size_t size)
{
    register int index = 0;
    register size_t search_sz = GC_min_stack_sz;
    register struct stack_head *head;
    
#ifdef MMAP_STACKS
    /* Zero pointers */
    mmap(stack, size, PROT_READ|PROT_WRITE, MAP_PRIVATE|MAP_NORESERVE|MAP_FIXED,
	 GC_zfd, 0);
#endif
    while (search_sz < size) {
        search_sz *= 2;
        index++;
    }
    if (search_sz != size) ABORT("Bad stack size");

    head = (struct stack_head *)(stack + search_sz - sizeof(struct stack_head));
    head->next = GC_stack_free_lists[index];
    head->base = stack;
    GC_stack_free_lists[index] = head;
}

void GC_my_stack_limits();

/* Notify virtual dirty bit implementation that known empty parts of	*/
/* stacks do not contain useful data.					*/ 
/* Caller holds allocation lock.					*/
void GC_old_stacks_are_fresh()
{
/* No point in doing this for MMAP stacks - and pointers are zero'd out */
/* by the mmap in GC_stack_free */
#ifndef MMAP_STACKS
    register int i;
    register struct stack_head *s;
    register ptr_t p;
    register size_t sz;
    register struct hblk * h;
    int dummy;
    
    for (i = 0, sz= GC_min_stack_sz; i < N_FREE_LISTS;
         i++, sz *= 2) {
         for (s = GC_stack_free_lists[i]; s != 0; s = s->next) {
             p = s->base;
             h = (struct hblk *)(((word)p + HBLKSIZE-1) & ~(HBLKSIZE-1));
             if ((ptr_t)h == p) {
                 GC_is_fresh((struct hblk *)p, divHBLKSZ(sz));
             } else {
                 GC_is_fresh((struct hblk *)p, divHBLKSZ(sz) - 1);
                 BZERO(p, (ptr_t)h - p);
             }
         }
    }
#endif /* MMAP_STACKS */
    GC_my_stack_limits();
}

/* The set of all known threads.  We intercept thread creation and 	*/
/* joins.  We never actually create detached threads.  We allocate all 	*/
/* new thread stacks ourselves.  These allow us to maintain this	*/
/* data structure.							*/

# define THREAD_TABLE_SZ 128	/* Must be power of 2	*/
volatile GC_thread GC_threads[THREAD_TABLE_SZ];

void GC_push_thread_structures GC_PROTO((void))
{
    GC_push_all((ptr_t)(GC_threads), (ptr_t)(GC_threads)+sizeof(GC_threads));
}

/* Add a thread to GC_threads.  We assume it wasn't already there.	*/
/* Caller holds allocation lock.					*/
GC_thread GC_new_thread(thread_t id)
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
    /* result -> finished = 0; */
    (void) cond_init(&(result->join_cv), USYNC_THREAD, 0);
    return(result);
}

/* Delete a thread from GC_threads.  We assume it is there.	*/
/* (The code intentionally traps if it wasn't.)			*/
/* Caller holds allocation lock.				*/
void GC_delete_thread(thread_t id)
{
    int hv = ((word)id) % THREAD_TABLE_SZ;
    register GC_thread p = GC_threads[hv];
    register GC_thread prev = 0;
    
    while (p -> id != id) {
        prev = p;
        p = p -> next;
    }
    if (prev == 0) {
        GC_threads[hv] = p -> next;
    } else {
        prev -> next = p -> next;
    }
}

/* Return the GC_thread correpsonding to a given thread_t.	*/
/* Returns 0 if it's not there.					*/
/* Caller holds  allocation lock.				*/
GC_thread GC_lookup_thread(thread_t id)
{
    int hv = ((word)id) % THREAD_TABLE_SZ;
    register GC_thread p = GC_threads[hv];
    
    while (p != 0 && p -> id != id) p = p -> next;
    return(p);
}

/* Solaris 2/Intel uses an initial stack size limit slightly bigger than the
   SPARC default of 8 MB.  Account for this to warn only if the user has
   raised the limit beyond the default.

   This is identical to DFLSSIZ defined in <sys/vm_machparam.h>.  This file
   is installed in /usr/platform/`uname -m`/include, which is not in the
   default include directory list, so copy the definition here.  */
#ifdef I386
# define MAX_ORIG_STACK_SIZE (8 * 1024 * 1024 + ((USRSTACK) & 0x3FFFFF))
#else
# define MAX_ORIG_STACK_SIZE (8 * 1024 * 1024)
#endif

word GC_get_orig_stack_size() {
    struct rlimit rl;
    static int warned = 0;
    int result;

    if (getrlimit(RLIMIT_STACK, &rl) != 0) ABORT("getrlimit failed");
    result = (word)rl.rlim_cur & ~(HBLKSIZE-1);
    if (result > MAX_ORIG_STACK_SIZE) {
	if (!warned) {
	    WARN("Large stack limit(%ld): only scanning 8 MB\n", result);
	    warned = 1;
	}
	result = MAX_ORIG_STACK_SIZE;
    }
    return result;
}

/* Notify dirty bit implementation of unused parts of my stack. */
/* Caller holds allocation lock.				*/
void GC_my_stack_limits()
{
    int dummy;
    register ptr_t hottest = (ptr_t)((word)(&dummy) & ~(HBLKSIZE-1));
    register GC_thread me = GC_lookup_thread(thr_self());
    register size_t stack_size = me -> stack_size;
    register ptr_t stack;
    
    if (stack_size == 0) {
      /* original thread */
        /* Empirically, what should be the stack page with lowest	*/
        /* address is actually inaccessible.				*/
        stack_size = GC_get_orig_stack_size() - GC_page_size;
        stack = GC_stackbottom - stack_size + GC_page_size;
    } else {
        stack = me -> stack;
    }
    if (stack > hottest || stack + stack_size < hottest) {
    	ABORT("sp out of bounds");
    }
    GC_is_fresh((struct hblk *)stack, divHBLKSZ(hottest - stack));
}


/* We hold allocation lock.  Should do exactly the right thing if the	*/
/* world is stopped.  Should not fail if it isn't.			*/
void GC_push_all_stacks()
{
    register int i;
    register GC_thread p;
    register ptr_t sp = GC_approx_sp();
    register ptr_t bottom, top;
    struct rlimit rl;
    
#   define PUSH(bottom,top) \
      if (GC_dirty_maintained) { \
	GC_push_selected((bottom), (top), GC_page_was_ever_dirty, \
		      GC_push_all_stack); \
      } else { \
        GC_push_all_stack((bottom), (top)); \
      }
    GC_push_all_stack((ptr_t)GC_lwp_registers,
		      (ptr_t)GC_lwp_registers
		      + max_lwps * sizeof(GC_lwp_registers[0]));
    for (i = 0; i < THREAD_TABLE_SZ; i++) {
      for (p = GC_threads[i]; p != 0; p = p -> next) {
        if (p -> stack_size != 0) {
            bottom = p -> stack;
            top = p -> stack + p -> stack_size;
        } else {
            /* The original stack. */
            bottom = GC_stackbottom - GC_get_orig_stack_size() + GC_page_size;
            top = GC_stackbottom;
        }
        if ((word)sp > (word)bottom && (word)sp < (word)top) bottom = sp;
        PUSH(bottom, top);
      }
    }
}


int GC_is_thread_stack(ptr_t addr)
{
    register int i;
    register GC_thread p;
    register ptr_t bottom, top;
    
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

/* The only thread that ever really performs a thr_join.	*/
void * GC_thr_daemon(void * dummy)
{
    void *status;
    thread_t departed;
    register GC_thread t;
    register int i;
    register int result;
    
    for(;;) {
      start:
        result = thr_join((thread_t)0, &departed, &status);
    	LOCK();
    	if (result != 0) {
    	    /* No more threads; wait for create. */
    	    for (i = 0; i < THREAD_TABLE_SZ; i++) {
    	        for (t = GC_threads[i]; t != 0; t = t -> next) {
                    if (!(t -> flags & (DETACHED | FINISHED))) {
                      UNLOCK();
                      goto start; /* Thread started just before we */
                      		  /* acquired the lock.		   */
                    }
                }
            }
            cond_wait(&GC_create_cv, &GC_allocate_ml);
            UNLOCK();
    	} else {
    	    t = GC_lookup_thread(departed);
	    GC_multithreaded--;
    	    if (!(t -> flags & CLIENT_OWNS_STACK)) {
    	    	GC_stack_free(t -> stack, t -> stack_size);
    	    }
    	    if (t -> flags & DETACHED) {
    	    	GC_delete_thread(departed);
    	    } else {
    	        t -> status = status;
    	    	t -> flags |= FINISHED;
    	    	cond_signal(&(t -> join_cv));
    	    	cond_broadcast(&GC_prom_join_cv);
    	    }
    	    UNLOCK();
    	}
    }
}

/* We hold the allocation lock, or caller ensures that 2 instances	*/
/* cannot be invoked concurrently.					*/
void GC_thr_init(void)
{
    GC_thread t;
    thread_t tid;

    if (GC_thr_initialized)
	    return;
    GC_thr_initialized = TRUE;
    GC_min_stack_sz = ((thr_min_stack() + 32*1024 + HBLKSIZE-1)
    		       & ~(HBLKSIZE - 1));
#ifdef MMAP_STACKS
    GC_zfd = open("/dev/zero", O_RDONLY);
    if (GC_zfd == -1)
	    ABORT("Can't open /dev/zero");
#endif /* MMAP_STACKS */
    cond_init(&GC_prom_join_cv, USYNC_THREAD, 0);
    cond_init(&GC_create_cv, USYNC_THREAD, 0);
    /* Add the initial thread, so we can stop it.	*/
      t = GC_new_thread(thr_self());
      t -> stack_size = 0;
      t -> flags = DETACHED | CLIENT_OWNS_STACK;
    if (thr_create(0 /* stack */, 0 /* stack_size */, GC_thr_daemon,
    		   0 /* arg */, THR_DETACHED | THR_DAEMON,
    		   &tid /* thread_id */) != 0) {
    	ABORT("Cant fork daemon");
    }
    thr_setprio(tid, 126);
}

/* We acquire the allocation lock to prevent races with 	*/
/* stopping/starting world.					*/
/* This is no more correct than the underlying Solaris 2.X	*/
/* implementation.  Under 2.3 THIS IS BROKEN.			*/
int GC_thr_suspend(thread_t target_thread)
{
    GC_thread t;
    int result;
    
    LOCK();
    result = thr_suspend(target_thread);
    if (result == 0) {
    	t = GC_lookup_thread(target_thread);
    	if (t == 0) ABORT("thread unknown to GC");
        t -> flags |= SUSPNDED;
    }
    UNLOCK();
    return(result);
}

int GC_thr_continue(thread_t target_thread)
{
    GC_thread t;
    int result;
    
    LOCK();
    result = thr_continue(target_thread);
    if (result == 0) {
    	t = GC_lookup_thread(target_thread);
    	if (t == 0) ABORT("thread unknown to GC");
        t -> flags &= ~SUSPNDED;
    }
    UNLOCK();
    return(result);
}

int GC_thr_join(thread_t wait_for, thread_t *departed, void **status)
{
    register GC_thread t;
    int result = 0;
    
    LOCK();
    if (wait_for == 0) {
        register int i;
        register GC_bool thread_exists;
    
    	for (;;) {
    	  thread_exists = FALSE;
    	  for (i = 0; i < THREAD_TABLE_SZ; i++) {
    	    for (t = GC_threads[i]; t != 0; t = t -> next) {
              if (!(t -> flags & DETACHED)) {
                if (t -> flags & FINISHED) {
                  goto found;
                }
                thread_exists = TRUE;
              }
            }
          }
          if (!thread_exists) {
              result = ESRCH;
    	      goto out;
          }
          cond_wait(&GC_prom_join_cv, &GC_allocate_ml);
        }
    } else {
        t = GC_lookup_thread(wait_for);
    	if (t == 0 || t -> flags & DETACHED) {
    	    result = ESRCH;
    	    goto out;
    	}
    	if (wait_for == thr_self()) {
    	    result = EDEADLK;
    	    goto out;
    	}
    	while (!(t -> flags & FINISHED)) {
            cond_wait(&(t -> join_cv), &GC_allocate_ml);
    	}
    	
    }
  found:
    if (status) *status = t -> status;
    if (departed) *departed = t -> id;
    cond_destroy(&(t -> join_cv));
    GC_delete_thread(t -> id);
  out:
    UNLOCK();
    return(result);
}


int
GC_thr_create(void *stack_base, size_t stack_size,
              void *(*start_routine)(void *), void *arg, long flags,
              thread_t *new_thread)
{
    int result;
    GC_thread t;
    thread_t my_new_thread;
    word my_flags = 0;
    void * stack = stack_base;
   
    LOCK();
    if (!GC_is_initialized) GC_init_inner();
    GC_multithreaded++;
    if (stack == 0) {
     	if (stack_size == 0) stack_size = 1024*1024;
     	stack = (void *)GC_stack_alloc(&stack_size);
     	if (stack == 0) {
	    GC_multithreaded--;
     	    UNLOCK();
     	    return(ENOMEM);
     	}
    } else {
    	my_flags |= CLIENT_OWNS_STACK;
    }
    if (flags & THR_DETACHED) my_flags |= DETACHED;
    if (flags & THR_SUSPENDED) my_flags |= SUSPNDED;
    result = thr_create(stack, stack_size, start_routine,
   		        arg, flags & ~THR_DETACHED, &my_new_thread);
    if (result == 0) {
        t = GC_new_thread(my_new_thread);
        t -> flags = my_flags;
        if (!(my_flags & DETACHED)) cond_init(&(t -> join_cv), USYNC_THREAD, 0);
        t -> stack = stack;
        t -> stack_size = stack_size;
        if (new_thread != 0) *new_thread = my_new_thread;
        cond_signal(&GC_create_cv);
    } else {
	GC_multithreaded--;
        if (!(my_flags & CLIENT_OWNS_STACK)) {
      	    GC_stack_free(stack, stack_size);
	}
    }        
    UNLOCK();  
    return(result);
}

# else /* !GC_SOLARIS_THREADS */

#ifndef LINT
  int GC_no_sunOS_threads;
#endif
#endif
