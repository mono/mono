#include "private/pthread_support.h"

# if defined(GC_DARWIN_THREADS)

#include <AvailabilityMacros.h>
#include "mono/utils/mono-compiler.h"

#ifdef MONO_DEBUGGER_SUPPORTED
#include "include/libgc-mono-debugger.h"
#endif

/* From "Inside Mac OS X - Mach-O Runtime Architecture" published by Apple
   Page 49:
   "The space beneath the stack pointer, where a new stack frame would normally
   be allocated, is called the red zone. This area as shown in Figure 3-2 may
   be used for any purpose as long as a new stack frame does not need to be
   added to the stack."
   
   Page 50: "If a leaf procedure's red zone usage would exceed 224 bytes, then
   it must set up a stack frame just like routines that call other routines."
*/
#ifdef POWERPC
# if CPP_WORDSZ == 32
#   define PPC_RED_ZONE_SIZE 224
# elif CPP_WORDSZ == 64
#   define PPC_RED_ZONE_SIZE 320
# endif
#endif

typedef struct StackFrame {
  unsigned long	savedSP;
  unsigned long	savedCR;
  unsigned long	savedLR;
  unsigned long	reserved[2];
  unsigned long	savedRTOC;
} StackFrame;

unsigned long FindTopOfStack(unsigned int stack_start) {
  StackFrame	*frame;
  
  if (stack_start == 0) {
# ifdef POWERPC
#   if CPP_WORDSZ == 32
      __asm__ volatile("lwz	%0,0(r1)" : "=r" (frame));
#   else
      __asm__ volatile("ldz	%0,0(r1)" : "=r" (frame));
#   endif
# endif
  } else {
    frame = (StackFrame *)stack_start;
  }

# ifdef DEBUG_THREADS
    /* GC_printf1("FindTopOfStack start at sp = %p\n", frame); */
# endif
  do {
    if (frame->savedSP == 0) break;
    		/* if there are no more stack frames, stop */

    frame = (StackFrame*)frame->savedSP;

    /* we do these next two checks after going to the next frame
       because the LR for the first stack frame in the loop
       is not set up on purpose, so we shouldn't check it. */
    if ((frame->savedLR & ~3) == 0) break; /* if the next LR is bogus, stop */
    if ((~(frame->savedLR) & ~3) == 0) break; /* ditto */
  } while (1); 

# ifdef DEBUG_THREADS
    /* GC_printf1("FindTopOfStack finish at sp = %p\n", frame); */
# endif

  return (unsigned long)frame;
}	

#ifdef DARWIN_DONT_PARSE_STACK
void GC_push_all_stacks() {
  int i;
  kern_return_t r;
  GC_thread p;
  pthread_t me;
  ptr_t lo, hi, altstack_lo, altstack_hi;
#if defined(POWERPC)
  ppc_thread_state_t state;
  mach_msg_type_number_t thread_state_count = PPC_THREAD_STATE_COUNT;
#elif defined(I386)
  i386_thread_state_t state;
  mach_msg_type_number_t thread_state_count = i386_THREAD_STATE_COUNT;
#elif defined(ARM)
  arm_thread_state_t state;
  mach_msg_type_number_t thread_state_count = ARM_THREAD_STATE_COUNT;
#elif defined(X86_64)
  x86_thread_state64_t state;
  mach_msg_type_number_t thread_state_count = x86_THREAD_STATE64_COUNT;
#else
# error FIXME for non-x86 || ppc architectures
  mach_msg_type_number_t thread_state_count = MACHINE_THREAD_STATE_COUNT;
#endif
  
  me = pthread_self();
  if (!GC_thr_initialized) GC_thr_init();
  
  for(i=0;i<THREAD_TABLE_SZ;i++) {
    for(p=GC_threads[i];p!=0;p=p->next) {
      if(p -> flags & FINISHED) continue;
      if(pthread_equal(p->id,me)) {
	lo = GC_approx_sp();
      } else {
	/* Get the thread state (registers, etc) */
	r = thread_get_state(
			     p->stop_info.mach_thread,
			     GC_MACH_THREAD_STATE_FLAVOR,
			     (natural_t*)&state,
			     &thread_state_count);
	if(r != KERN_SUCCESS) continue;
	
#if defined(I386)
#if defined (TARGET_IOS) || (MAC_OS_X_VERSION_MIN_REQUIRED >= MAC_OS_X_VERSION_10_5)

	lo = state.__esp;

	GC_push_one(state.__eax); 
	GC_push_one(state.__ebx); 
	GC_push_one(state.__ecx); 
	GC_push_one(state.__edx); 
	GC_push_one(state.__edi); 
	GC_push_one(state.__esi); 
	GC_push_one(state.__ebp); 
#else
	lo = state.esp;

	GC_push_one(state.eax); 
	GC_push_one(state.ebx); 
	GC_push_one(state.ecx); 
	GC_push_one(state.edx); 
	GC_push_one(state.edi); 
	GC_push_one(state.esi); 
	GC_push_one(state.ebp); 
#endif
#elif defined(X86_64)
          lo = state.__rsp;
          GC_push_one(state.__rax);
          GC_push_one(state.__rbx);
          GC_push_one(state.__rcx);
          GC_push_one(state.__rdx);
          GC_push_one(state.__rdi);
          GC_push_one(state.__rsi);
          GC_push_one(state.__rbp);
          GC_push_one(state.__r8);
          GC_push_one(state.__r9);
          GC_push_one(state.__r10);
          GC_push_one(state.__r11);
          GC_push_one(state.__r12);
          GC_push_one(state.__r13);
          GC_push_one(state.__r14);
          GC_push_one(state.__r15);
#elif defined(POWERPC)
#if defined(_STRUCT_PPC_EXCEPTION_STATE) && defined(__DARWIN_UNIX03)
	lo = (void*)(state.__r1 - PPC_RED_ZONE_SIZE);
        
	GC_push_one(state.__r0); 
	GC_push_one(state.__r2); 
	GC_push_one(state.__r3); 
	GC_push_one(state.__r4); 
	GC_push_one(state.__r5); 
	GC_push_one(state.__r6); 
	GC_push_one(state.__r7); 
	GC_push_one(state.__r8); 
	GC_push_one(state.__r9); 
	GC_push_one(state.__r10); 
	GC_push_one(state.__r11); 
	GC_push_one(state.__r12); 
	GC_push_one(state.__r13); 
	GC_push_one(state.__r14); 
	GC_push_one(state.__r15); 
	GC_push_one(state.__r16); 
	GC_push_one(state.__r17); 
	GC_push_one(state.__r18); 
	GC_push_one(state.__r19); 
	GC_push_one(state.__r20); 
	GC_push_one(state.__r21); 
	GC_push_one(state.__r22); 
	GC_push_one(state.__r23); 
	GC_push_one(state.__r24); 
	GC_push_one(state.__r25); 
	GC_push_one(state.__r26); 
	GC_push_one(state.__r27); 
	GC_push_one(state.__r28); 
	GC_push_one(state.__r29); 
	GC_push_one(state.__r30); 
	GC_push_one(state.__r31);
#else
	lo = (void*)(state.r1 - PPC_RED_ZONE_SIZE);
        
	GC_push_one(state.r0); 
	GC_push_one(state.r2); 
	GC_push_one(state.r3); 
	GC_push_one(state.r4); 
	GC_push_one(state.r5); 
	GC_push_one(state.r6); 
	GC_push_one(state.r7); 
	GC_push_one(state.r8); 
	GC_push_one(state.r9); 
	GC_push_one(state.r10); 
	GC_push_one(state.r11); 
	GC_push_one(state.r12); 
	GC_push_one(state.r13); 
	GC_push_one(state.r14); 
	GC_push_one(state.r15); 
	GC_push_one(state.r16); 
	GC_push_one(state.r17); 
	GC_push_one(state.r18); 
	GC_push_one(state.r19); 
	GC_push_one(state.r20); 
	GC_push_one(state.r21); 
	GC_push_one(state.r22); 
	GC_push_one(state.r23); 
	GC_push_one(state.r24); 
	GC_push_one(state.r25); 
	GC_push_one(state.r26); 
	GC_push_one(state.r27); 
	GC_push_one(state.r28); 
	GC_push_one(state.r29); 
	GC_push_one(state.r30); 
	GC_push_one(state.r31);
#endif
#elif defined(ARM)
        lo = (void*)state.__sp;

        GC_push_one(state.__r[0]);
        GC_push_one(state.__r[1]);
        GC_push_one(state.__r[2]);
        GC_push_one(state.__r[3]);
        GC_push_one(state.__r[4]);
        GC_push_one(state.__r[5]);
        GC_push_one(state.__r[6]);
        GC_push_one(state.__r[7]);
        GC_push_one(state.__r[8]);
        GC_push_one(state.__r[9]);
        GC_push_one(state.__r[10]);
        GC_push_one(state.__r[11]);
        GC_push_one(state.__r[12]);
        /* GC_push_one(state.__sp);  */
        GC_push_one(state.__lr);
        GC_push_one(state.__pc);
        GC_push_one(state.__cpsr);
#else
# error FIXME for non-x86 || ppc architectures
#endif
      } /* p != me */
      if(p->flags & MAIN_THREAD)
	hi = GC_stackbottom;
      else
	hi = p->stack_end;

	  if (p->altstack && lo >= p->altstack && lo <= p->altstack + p->altstack_size) {
		  altstack_lo = lo;
		  altstack_hi = p->altstack + p->altstack_size;
		  lo = (char*)p->stack;
		  hi = (char*)p->stack + p->stack_size;
	  }	else {
		  altstack_lo = NULL;
	  }

#if DEBUG_THREADS
      GC_printf3("Darwin: Stack for thread 0x%lx = [%lx,%lx)\n",
		 (unsigned long) p -> id,
		 (unsigned long) lo,
		 (unsigned long) hi
		 );
#endif
	  if (lo)
		  GC_push_all_stack(lo,hi);
	  if (altstack_lo)
		  GC_push_all_stack(altstack_lo,altstack_hi);
    } /* for(p=GC_threads[i]...) */
  } /* for(i=0;i<THREAD_TABLE_SZ...) */
}

#else /* !DARWIN_DONT_PARSE_STACK; Use FindTopOfStack() */

void GC_push_all_stacks() {
    int i;
	task_t my_task;
    kern_return_t r;
    mach_port_t me;
    ptr_t lo, hi;
    thread_act_array_t act_list = 0;
    mach_msg_type_number_t listcount = 0;

    me = mach_thread_self();
    if (!GC_thr_initialized) GC_thr_init();
    
	my_task = current_task();
    r = task_threads(my_task, &act_list, &listcount);
    if(r != KERN_SUCCESS) ABORT("task_threads failed");
    for(i = 0; i < listcount; i++) {
      thread_act_t thread = act_list[i];
      if (thread == me) {
	lo = GC_approx_sp();
	hi = (ptr_t)FindTopOfStack(0);
      } else {
#     if defined(POWERPC)
#      if CPP_WORDSZ == 32
	ppc_thread_state_t info;
#      else
	ppc_thread_state64_t info;
#      endif
	mach_msg_type_number_t outCount = THREAD_STATE_MAX;
	r = thread_get_state(thread, GC_MACH_THREAD_STATE_FLAVOR,
			     (natural_t *)&info, &outCount);
	if(r != KERN_SUCCESS) continue;

#if defined(_STRUCT_PPC_EXCEPTION_STATE)
	lo = (void*)(info.__r1 - PPC_RED_ZONE_SIZE);
	hi = (ptr_t)FindTopOfStack(info.__r1);

	GC_push_one(info.__r0); 
	GC_push_one(info.__r2); 
	GC_push_one(info.__r3); 
	GC_push_one(info.__r4); 
	GC_push_one(info.__r5); 
	GC_push_one(info.__r6); 
	GC_push_one(info.__r7); 
	GC_push_one(info.__r8); 
	GC_push_one(info.__r9); 
	GC_push_one(info.__r10); 
	GC_push_one(info.__r11); 
	GC_push_one(info.__r12); 
	GC_push_one(info.__r13); 
	GC_push_one(info.__r14); 
	GC_push_one(info.__r15); 
	GC_push_one(info.__r16); 
	GC_push_one(info.__r17); 
	GC_push_one(info.__r18); 
	GC_push_one(info.__r19); 
	GC_push_one(info.__r20); 
	GC_push_one(info.__r21); 
	GC_push_one(info.__r22); 
	GC_push_one(info.__r23); 
	GC_push_one(info.__r24); 
	GC_push_one(info.__r25); 
	GC_push_one(info.__r26); 
	GC_push_one(info.__r27); 
	GC_push_one(info.__r28); 
	GC_push_one(info.__r29); 
	GC_push_one(info.__r30); 
	GC_push_one(info.__r31);
#else
	lo = (void*)(info.r1 - PPC_RED_ZONE_SIZE);
	hi = (ptr_t)FindTopOfStack(info.r1);

	GC_push_one(info.r0); 
	GC_push_one(info.r2); 
	GC_push_one(info.r3); 
	GC_push_one(info.r4); 
	GC_push_one(info.r5); 
	GC_push_one(info.r6); 
	GC_push_one(info.r7); 
	GC_push_one(info.r8); 
	GC_push_one(info.r9); 
	GC_push_one(info.r10); 
	GC_push_one(info.r11); 
	GC_push_one(info.r12); 
	GC_push_one(info.r13); 
	GC_push_one(info.r14); 
	GC_push_one(info.r15); 
	GC_push_one(info.r16); 
	GC_push_one(info.r17); 
	GC_push_one(info.r18); 
	GC_push_one(info.r19); 
	GC_push_one(info.r20); 
	GC_push_one(info.r21); 
	GC_push_one(info.r22); 
	GC_push_one(info.r23); 
	GC_push_one(info.r24); 
	GC_push_one(info.r25); 
	GC_push_one(info.r26); 
	GC_push_one(info.r27); 
	GC_push_one(info.r28); 
	GC_push_one(info.r29); 
	GC_push_one(info.r30); 
	GC_push_one(info.r31);
#endif
#      elif defined(I386) /* !POWERPC */
	/* FIXME: Remove after testing:	*/
	WARN("This is completely untested and likely will not work\n", 0);
	i386_thread_state_t info;
	mach_msg_type_number_t outCount = THREAD_STATE_MAX;
	r = thread_get_state(thread, GC_MACH_THREAD_STATE_FLAVOR,
			     (natural_t *)&info, &outCount);
	if(r != KERN_SUCCESS) continue;

#if defined (TARGET_IOS) || (MAC_OS_X_VERSION_MIN_REQUIRED >= MAC_OS_X_VERSION_10_5)
	lo = (void*)info.__esp;
	hi = (ptr_t)FindTopOfStack(info.__esp);

	GC_push_one(info.__eax); 
	GC_push_one(info.__ebx); 
	GC_push_one(info.__ecx); 
	GC_push_one(info.__edx); 
	GC_push_one(info.__edi); 
	GC_push_one(info.__esi); 
	GC_push_one(info.__ebp);
	/* GC_push_one(info.__esp);  */
	GC_push_one(info.__ss); 
	GC_push_one(info.__eip); 
	GC_push_one(info.__cs); 
	GC_push_one(info.__ds); 
	GC_push_one(info.__es); 
	GC_push_one(info.__fs); 
	GC_push_one(info.__gs); 
#else
	lo = (void*)info.esp;
	hi = (ptr_t)FindTopOfStack(info.esp);

	GC_push_one(info.eax); 
	GC_push_one(info.ebx); 
	GC_push_one(info.ecx); 
	GC_push_one(info.edx); 
	GC_push_one(info.edi); 
	GC_push_one(info.esi); 
	GC_push_one(info.ebp);
	/* GC_push_one(info.esp);  */
	GC_push_one(info.ss); 
	GC_push_one(info.eip); 
	GC_push_one(info.cs); 
	GC_push_one(info.ds); 
	GC_push_one(info.es); 
	GC_push_one(info.fs); 
	GC_push_one(info.gs); 
#endif
#      elif defined(ARM) /* !I386 */
	arm_thread_state_t info;
	mach_msg_type_number_t outCount = THREAD_STATE_MAX;
	r = thread_get_state(thread, GC_MACH_THREAD_STATE_FLAVOR,
			     (natural_t *)&info, &outCount);
	if(r != KERN_SUCCESS) continue;

	lo = (void*)info.__sp;
	hi = (ptr_t)FindTopOfStack(info.__sp);

	GC_push_one(info.__r[0]); 
	GC_push_one(info.__r[1]); 
	GC_push_one(info.__r[2]); 
	GC_push_one(info.__r[3]); 
	GC_push_one(info.__r[4]); 
	GC_push_one(info.__r[5]); 
	GC_push_one(info.__r[6]); 
	GC_push_one(info.__r[7]); 
	GC_push_one(info.__r[8]); 
	GC_push_one(info.__r[9]); 
	GC_push_one(info.__r[10]); 
	GC_push_one(info.__r[11]); 
	GC_push_one(info.__r[12]); 
	/* GC_push_one(info.__sp);  */
	GC_push_one(info.__lr); 
	GC_push_one(info.__pc); 
	GC_push_one(info.__cpsr); 
#      endif /* !ARM */
      }
#     if DEBUG_THREADS
       GC_printf3("Darwin: Stack for thread 0x%lx = [%lx,%lx)\n",
		  (unsigned long) thread,
		  (unsigned long) lo,
		  (unsigned long) hi
		 );
#     endif
      GC_push_all_stack(lo, hi);
	  mach_port_deallocate(my_task, thread);
    } /* for(p=GC_threads[i]...) */
    vm_deallocate(my_task, (vm_address_t)act_list, sizeof(thread_t) * listcount);
	mach_port_deallocate(my_task, me);
}
#endif /* !DARWIN_DONT_PARSE_STACK */

static mach_port_t GC_mach_handler_thread;
static int GC_use_mach_handler_thread = 0;

#define SUSPEND_THREADS_SIZE 2048
static struct GC_mach_thread GC_mach_threads[SUSPEND_THREADS_SIZE];
static int GC_mach_threads_count;

void GC_stop_init() {
  int i;

  for (i = 0; i < SUSPEND_THREADS_SIZE; i++) {
    GC_mach_threads[i].thread = 0;
    GC_mach_threads[i].already_suspended = 0;
  }
  GC_mach_threads_count = 0;
}

/* returns true if there's a thread in act_list that wasn't in old_list */
int GC_suspend_thread_list(thread_act_array_t act_list, int count, 
			   thread_act_array_t old_list, int old_count) {
  mach_port_t my_thread = mach_thread_self();
  int i, j;

  int changed = 0;

  for(i = 0; i < count; i++) {
    thread_act_t thread = act_list[i];
#   if DEBUG_THREADS 
      GC_printf1("Attempting to suspend thread %p\n", thread);
#   endif
    /* find the current thread in the old list */
    int found = 0;
    for(j = 0; j < old_count; j++) {
      thread_act_t old_thread = old_list[j];
      if (old_thread == thread) {
	found = 1;
	break;
      }
    }
    if (!found) {
      /* add it to the GC_mach_threads list */
      GC_mach_threads[GC_mach_threads_count].thread = thread;
      /* default is not suspended */
      GC_mach_threads[GC_mach_threads_count].already_suspended = 0;
      changed = 1;
    }      

    if (thread != my_thread &&
	(!GC_use_mach_handler_thread
	 || (GC_use_mach_handler_thread
	     && GC_mach_handler_thread != thread))) {
      struct thread_basic_info info;
      mach_msg_type_number_t outCount = THREAD_INFO_MAX;
      kern_return_t kern_result = thread_info(thread, THREAD_BASIC_INFO,
				(thread_info_t)&info, &outCount);
      if(kern_result != KERN_SUCCESS) {
	/* the thread may have quit since the thread_threads () call 
	 * we mark already_suspended so it's not dealt with anymore later
	 */
        if (!found) {
	  GC_mach_threads[GC_mach_threads_count].already_suspended = TRUE;
    	  GC_mach_threads_count++;
	}
	continue;
      }
#     if DEBUG_THREADS
        GC_printf2("Thread state for 0x%lx = %d\n", thread, info.run_state);
#     endif
      if (!found) {
	GC_mach_threads[GC_mach_threads_count].already_suspended = info.suspend_count;
      }
      if (info.suspend_count) continue;
      
#     if DEBUG_THREADS
        GC_printf1("Suspending 0x%lx\n", thread);
#     endif
      /* Suspend the thread */
      kern_result = thread_suspend(thread);
      if(kern_result != KERN_SUCCESS) {
	/* the thread may have quit since the thread_threads () call 
	 * we mark already_suspended so it's not dealt with anymore later
	 */
        if (!found) {
	  GC_mach_threads[GC_mach_threads_count].already_suspended = TRUE;
    	  GC_mach_threads_count++;
	}
	continue;
      }
    } 
    if (!found) GC_mach_threads_count++;
  }
  
  mach_port_deallocate(current_task(), my_thread);
  return changed;
}


/* Caller holds allocation lock.	*/
void GC_stop_world()
{
  int i, changes;
    GC_thread p;
	task_t my_task = current_task();
    mach_port_t my_thread = mach_thread_self();
    kern_return_t kern_result;
    thread_act_array_t act_list, prev_list;
    mach_msg_type_number_t listcount, prevcount;
    
    if (GC_notify_event)
        GC_notify_event (GC_EVENT_PRE_STOP_WORLD);

#   if DEBUG_THREADS
      GC_printf1("Stopping the world from 0x%lx\n", mach_thread_self());
#   endif

    /* clear out the mach threads list table */
    GC_stop_init(); 
       
    /* Make sure all free list construction has stopped before we start. */
    /* No new construction can start, since free list construction is	*/
    /* required to acquire and release the GC lock before it starts,	*/
    /* and we have the lock.						*/
#   ifdef PARALLEL_MARK
      GC_acquire_mark_lock();
      GC_ASSERT(GC_fl_builder_count == 0);
      /* We should have previously waited for it to become zero. */
#   endif /* PARALLEL_MARK */

      /* Loop stopping threads until you have gone over the whole list
	 twice without a new one appearing. thread_create() won't
	 return (and thus the thread stop) until the new thread
	 exists, so there is no window whereby you could stop a
	 thread, recognise it is stopped, but then have a new thread
	 it created before stopping show up later.
      */
      
      changes = 1;
      prev_list = NULL;
      prevcount = 0;
      do {
	int result;		  
	kern_result = task_threads(my_task, &act_list, &listcount);
	
	if(kern_result == KERN_SUCCESS) {	
		result = GC_suspend_thread_list(act_list, listcount,
										prev_list, prevcount);
		changes = result;
		
		if(prev_list != NULL) {
			for(i = 0; i < prevcount; i++)
				mach_port_deallocate(my_task, prev_list[i]);
			
			vm_deallocate(my_task, (vm_address_t)prev_list, sizeof(thread_t) * prevcount);
		}
		
		prev_list = act_list;
		prevcount = listcount;
	}		
      } while (changes);
     
	  for(i = 0; i < listcount; i++)
		  mach_port_deallocate(my_task, act_list[i]);
	  
	  vm_deallocate(my_task, (vm_address_t)act_list, sizeof(thread_t) * listcount);
	  
 
#   ifdef MPROTECT_VDB
      if(GC_incremental) {
        extern void GC_mprotect_stop();
        GC_mprotect_stop();
      }
#   endif
    
#   ifdef PARALLEL_MARK
      GC_release_mark_lock();
#   endif
    #if DEBUG_THREADS
      GC_printf1("World stopped from 0x%lx\n", my_thread);
    #endif
	  
	  mach_port_deallocate(my_task, my_thread);

    if (GC_notify_event)
        GC_notify_event (GC_EVENT_POST_STOP_WORLD);
}

/* Caller holds allocation lock, and has held it continuously since	*/
/* the world stopped.							*/
void GC_start_world()
{
  task_t my_task = current_task();
  mach_port_t my_thread = mach_thread_self();
  int i, j;
  GC_thread p;
  kern_return_t kern_result;
  thread_act_array_t act_list;
  mach_msg_type_number_t listcount;
  struct thread_basic_info info;
  mach_msg_type_number_t outCount = THREAD_INFO_MAX;

  if (GC_notify_event)
      GC_notify_event (GC_EVENT_PRE_START_WORLD);
  
#   if DEBUG_THREADS
      GC_printf0("World starting\n");
#   endif

#   ifdef MPROTECT_VDB
      if(GC_incremental) {
        extern void GC_mprotect_resume();
        GC_mprotect_resume();
      }
#   endif

    kern_result = task_threads(my_task, &act_list, &listcount);
    for(i = 0; i < listcount; i++) {
      thread_act_t thread = act_list[i];
      if (thread != my_thread &&
	  (!GC_use_mach_handler_thread ||
	   (GC_use_mach_handler_thread && GC_mach_handler_thread != thread))) {
	for(j = 0; j < GC_mach_threads_count; j++) {
	  if (thread == GC_mach_threads[j].thread) {
	    if (GC_mach_threads[j].already_suspended) {
#             if DEBUG_THREADS
	        GC_printf1("Not resuming already suspended thread %p\n", thread);
#             endif
	      continue;
	    }
	    kern_result = thread_info(thread, THREAD_BASIC_INFO,
				      (thread_info_t)&info, &outCount);
	    if(kern_result != KERN_SUCCESS) continue;
#           if DEBUG_THREADS
	      GC_printf2("Thread state for 0x%lx = %d\n", thread,
			 info.run_state);
	      GC_printf1("Resuming 0x%lx\n", thread);
#           endif
	    /* Resume the thread */
	    kern_result = thread_resume(thread);
	    if(kern_result != KERN_SUCCESS) continue;
	  } 
	}
      }
	  
	  mach_port_deallocate(my_task, thread);
    }
    vm_deallocate(my_task, (vm_address_t)act_list, sizeof(thread_t) * listcount);
	
	mach_port_deallocate(my_task, my_thread);

    if (GC_notify_event)
        GC_notify_event (GC_EVENT_POST_START_WORLD);

#   if DEBUG_THREADS
     GC_printf0("World started\n");
#   endif
}

void GC_darwin_register_mach_handler_thread(mach_port_t thread) {
  GC_mach_handler_thread = thread;
  GC_use_mach_handler_thread = 1;
}

#ifdef MONO_DEBUGGER_SUPPORTED
GCThreadFunctions *gc_thread_vtable = NULL;

void *
GC_mono_debugger_get_stack_ptr (void)
{
	GC_thread me;

	me = GC_lookup_thread (pthread_self ());
	return &me->stop_info.stack_ptr;
}
#endif

#endif
