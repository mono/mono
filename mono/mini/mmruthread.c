// COPYRIGHT 2012, Mike Rieker, Beverly, MA, USA, mrieker@nii.net
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#include <errno.h>
#include <stdbool.h>
#include <sys/syscall.h>
#include <unistd.h>

#include "config.h"
#include "mini.h"
#include "../metadata/gc-internal.h"
#include "../utils/mono-mmap.h"

#include "mmruthread.h"

//#define PRINTF printf
static inline void PRINTF (char const *fmt, ...) { }


// Porting:
//   add your target to this list
#define SUPPORTED_TARGETS (defined(TARGET_AMD64) || defined(TARGET_X86))

// Porting:
//   define your target's memory page size
#if defined(TARGET_AMD64) || defined(TARGET_X86)
#define PAGE_SIZE 4096
#define GUARD_SIZE (2*PAGE_SIZE)
#define MINST_SIZE (2*PAGE_SIZE)
#endif

typedef struct FreeStackElement FreeStackElement;
typedef struct MMRUThread MMRUThread;

struct FreeStackElement {
	FreeStackElement *nextStack;
	gulong stackSize;
};

struct MMRUThread {
	MonoObject *uThreadObj;	// MMRUThread object pointer
	guint8 *microStkAddr;	// bottom of uthread's allocated stack (page aligned)
	guint8 *macroStkAddr;	// saved original thread's stack address
	MonoLMF microLMF;	// top-level microthread LMF struct (valid while microthread active)
	MonoLMF *macroLMFPtr;	// inner macrothread LMF struct pointer (valid while microthread busy)
	MonoLMF *microLMFPtr;	// inner microthread LMF struct pointer (valid while microthread suspended)
	MonoException *(*entry) (MonoObject *uThreadObj);  // MainEx() entrypoint
	gulong microStkSize;	// size of microthread's stack (page aligned) not including guard
	gulong macroStkSize;	// saved macrothread's stack size
	_Bool volatile busy;	// false: idle; true: stack in use
	_Bool active;		// start() has been called, and suspend(exit=true) hasn't been called

	// Porting:
	//   List all registers here that the ABI expects to be preserved across calls
	//   but that cannot be specified as modified in an asm(...) statement.
	//   There has to be two locations for each such register,
	//      one for the macro thread stack, ie, the one mono gives you to start with
	//      one for the micro thread stack, ie, the one these routines create
#if defined(TARGET_AMD64) || defined(TARGET_X86)
	gulong macBP, macSP;	// macrothread's frame and stack pointer
	gulong micBP, micSP;	// microthread's frame and stack pointer
#endif
#if defined(TARGET_X86)
	gulong macBX, micBX;	// macrothread and microthread PIC pointers
#endif
};

// Porting:
//   Test-and-set 'mmrUThread->busy'
//     returns whether or not 'mmrUThread->busy' was busy
//     then atomically forces 'mmrUThread->busy' true regardless
//     also barrier off reads and writes
//   Clear 'mmrUThread->busy'
//     also barrier off reads and writes
#if defined(TARGET_AMD64) || defined(TARGET_X86)
static inline _Bool TSUTHREADBUSY(MMRUThread *mmrUThread)
{
	_Bool busy = true;
	asm volatile ("xchg %1,%0" : "+m" (mmrUThread->busy), "+r" (busy));
	return busy;
}
static inline void CLRUTHREADBUSY(MMRUThread *mmrUThread)
{
	guint32 eax = 0;
	asm volatile ("cpuid" : "+a" (eax) : : "ebx", "ecx", "edx");
	mmrUThread->busy = false;
}
#endif

static FreeStackElement *freeStackList = NULL;
static pthread_mutex_t freeStackMutex = PTHREAD_MUTEX_INITIALIZER;
static gulong numStackBytes = 0;

#if defined(TARGET_AMD64)
#define ASMCALLATTR __attribute__((used))            // (RDI,RSI), ie, usual calling standard
#endif
#if defined(TARGET_X86)
#define ASMCALLATTR __attribute__((fastcall, used))  // (ECX,EDX)
#endif
#if !defined(ASMCALLATTR)
#define ASMCALLATTR
#endif


static MonoException *mmruthread_ctor    (MonoObject *uThreadObj, gulong stackSize, MMRUThread **mmrUThread_r);
static MonoException *mmruthread_dtor    (MMRUThread *mmrUThread);
static gulong         mmruthread_stackleft (void);
static MonoException *mmruthread_start   (MMRUThread *mmrUThread);
static ASMCALLATTR void CallIt (MMRUThread *mmrUThread);
static MonoException *mmruthread_suspend (MMRUThread *mmrUThread, MonoException *except, _Bool exit);
static MonoException *mmruthread_resume  (MMRUThread *mmrUThread, MonoException *except);
static int            mmruthread_active  (MMRUThread *mmrUThread);
static void AllocStack (MMRUThread *mmrUThread);
static void FreeStack  (MMRUThread *mmrUThread);
static ASMCALLATTR void SetMicroStk (MMRUThread *mmrUThread);
static ASMCALLATTR void SetMacroStk (MMRUThread *mmrUThread);
static void GetCurrentStackBounds (guint8 **base, gulong *size);
static void SetCurrentStackBounds (guint8 *base, gulong size);


/*
 * Garbage collector interface.
 * These pointers get filled in with whatever --gc=<whatever> was selected.
 */
static void (*mmruthread_lockgc)   (void);              // block garbage collection so we can call any of ...
                                                        // ... mmruthread_{unlkgc,addroot,remroot,setstack}
static void (*mmruthread_unlkgc)   (void);              // allow garbage collection
static void (*mmruthread_addroot)  (char *b, char *e);  // start scanning [b,e) for pointers to things
static void (*mmruthread_remroot)  (char *b, char *e);  // stop scanning [b,e) for pointers to things
static void (*mmruthread_setstack) (void *end);         // this is the top of the current thread's stack now

#ifdef HAVE_BOEHM_GC
#include "../../libgc/include/mmruthread_gc_boehm.h"
#endif

#ifdef HAVE_NULL_GC
static void mmruthread_null_lockgc   (void)
{
}
static void mmruthread_null_unlkgc   (void)
{
}
static void mmruthread_null_addroot  (char *b, char *e)
{
}
static void mmruthread_null_remroot  (char *b, char *e)
{
}
static void mmruthread_null_setstack (void *end)
{
}
#endif


/**
 * @brief One-time initialization routine.
 */
void
mmruthread_init (void)
{
#ifdef HAVE_BOEHM_GC
	if (strcmp (mono_gc_get_gc_name (), "boehm") == 0) {
		mmruthread_lockgc   = mmruthread_boehm_lockgc;
		mmruthread_unlkgc   = mmruthread_boehm_unlkgc;
		mmruthread_addroot  = mmruthread_boehm_addroot;
		mmruthread_remroot  = mmruthread_boehm_remroot;
		mmruthread_setstack = mmruthread_boehm_setstack;
	}
#endif

#ifdef HAVE_NULL_GC
	if (strcmp (mono_gc_get_gc_name (), "null") == 0) {
		mmruthread_lockgc   = mmruthread_null_lockgc;
		mmruthread_unlkgc   = mmruthread_null_unlkgc;
		mmruthread_addroot  = mmruthread_null_addroot;
		mmruthread_remroot  = mmruthread_null_remroot;
		mmruthread_setstack = mmruthread_null_setstack;
	}
#endif

#if SUPPORTED_TARGETS
	mono_add_internal_call ("Mono.Tasklets.MMRUThread::active",    mmruthread_active);
	mono_add_internal_call ("Mono.Tasklets.MMRUThread::ctor",      mmruthread_ctor);
	mono_add_internal_call ("Mono.Tasklets.MMRUThread::dtor",      mmruthread_dtor);
	mono_add_internal_call ("Mono.Tasklets.MMRUThread::resume",    mmruthread_resume);
	mono_add_internal_call ("Mono.Tasklets.MMRUThread::StackLeft", mmruthread_stackleft);
	mono_add_internal_call ("Mono.Tasklets.MMRUThread::start",     mmruthread_start);
	mono_add_internal_call ("Mono.Tasklets.MMRUThread::suspend",   mmruthread_suspend);
#endif
}


/**
 * @brief MMRUThread object just created, allocate corresponding stack and MMRUThread struct.
 * @param uThreadObj = points to MMRUThread object
 * @param stackSize = 0: use current thread's stack size
 *                 else: make stack this size
 * @param mmrUThread_r = where to return MMRUThread struct pointer
 */
static MonoException *
mmruthread_ctor (MonoObject *uThreadObj, gulong stackSize, MMRUThread **mmrUThread_r)
{
	MMRUThread *mmrUThread;

	*mmrUThread_r = NULL;

	/*
	 * Make sure we have an implemented GC scheme.
	 */
	if (mmruthread_lockgc == NULL) {
		char const *gcname = mono_gc_get_gc_name ();
		char *msg = alloca (strlen (gcname) + 40);
		sprintf (msg, "MMRUThread not implemented for gc %s", gcname);
		return mono_get_exception_not_implemented (msg);
	}

	/*
	 * If no stack size given, get current stack size.
	 */
	if (stackSize == 0) {
		guint8 *staddr;
		gulong stsize;

		GetCurrentStackBounds (&staddr, &stsize);
		stackSize = stsize;
	}

	/*
	 * Got to have some room for a stack.
	 */
	if (stackSize < MINST_SIZE) {
		return mono_get_exception_argument ("stackSize", "stack size too small");
	}

	/*
	 * Allocate internal struct to keep track of stack.
	 */
	mmrUThread = g_malloc (sizeof *mmrUThread);
	if (mmrUThread == NULL) {
		return mono_get_exception_out_of_memory ();
	}
	memset (mmrUThread, 0, sizeof *mmrUThread);
	mmrUThread->microStkSize = (stackSize + PAGE_SIZE - 1) & -PAGE_SIZE;
	mmrUThread->uThreadObj   = uThreadObj;

	PRINTF ("mmruthread_ctor*: obj=%lX, str=%lX\n", (gulong)uThreadObj, (gulong)mmrUThread);

	*mmrUThread_r = mmrUThread;
	return NULL;
}


/**
 * @brief destructor - free off stack area and MMRUThread struct
 * @param mmrUThread = points to MMRUThread struct
 */
static MonoException *
mmruthread_dtor (MMRUThread *mmrUThread)
{
	guint8 *stackAddr;
	gulong stackSize;

	/*
	 * Can't destroy it if its stack is in use somewhere.
	 * Then mark it busy so no one else can use its stack.
	 */
	if (TSUTHREADBUSY(mmrUThread)) {
		return mono_get_exception_argument ("mmrUThread", "MMRUThread busy");
	}

	/*
	 * Make sure the garbage collector is off it.
	 */
	stackAddr = mmrUThread->microStkAddr;
	stackSize = mmrUThread->microStkSize;

	if (mmrUThread->micSP != 0) {
		PRINTF ("mmruthread_dtor*: dereg %lX..%lX\n", mmrUThread->micSP, (gulong)stackAddr + stackSize - 1);
		g_assert (stackAddr != 0);
		mmruthread_remroot ((char *)mmrUThread->micSP, (char *)stackAddr + stackSize);
		mmrUThread->micSP = 0;
	}

	/*
	 * Poof!  This frees the stack and the mmrUThread struct with it.
	 */
	PRINTF ("mmruthread_dtor*: obj=%lX, str=%lX, stk=%lX..%lX\n", 
			(gulong)mmrUThread->uThreadObj, (gulong)mmrUThread, (gulong)stackAddr, (gulong)stackAddr + stackSize - 1);

	if (mmrUThread->active) {
		FreeStack (mmrUThread);
	}
	g_free (mmrUThread);

	return NULL;
}


/**
 * @brief how much stack is left (available)
 * @returns number of bytes remaining
 */
static gulong
mmruthread_stackleft (void)
{
	guint8 *base, *sp;
	gulong size;

	/*
	 * Get current stack bounds.
	 * This should work for either micro or macro threads.
	 */
	GetCurrentStackBounds (&base, &size);

	/*
	 * Return currentstackpointer - bottomofstack
	 */
#if defined(TARGET_AMD64)
	asm ("movq %%rsp,%0" : "=r" (sp));
#endif
#if defined(TARGET_X86)
	asm ("movl %%esp,%0" : "=r" (sp));
#endif
	return sp - base;
}


/**
 * @brief start uthread
 * @param mmrUThread = points to MMRUThread struct that is inactive or suspended
 *                     calls corresponding MainEx() routine
 * @returns NULL or exception pointer
 *          returned exception possible sources:
 *            1) generated by mmruthread_start code itself, eg, MainEx() not defined
 *            2) passed by MainEx() code to SuspendEx() or ExitEx()
 *            3) generated by mmruthread_suspend code itself, eg, no mem for stack gc slot
 */
static MonoException *
mmruthread_start (MMRUThread *mmrUThread)
{
	gpointer iterm, mainExCodeStart;
	MonoObject *uThreadObj;
	MonoClass *mainExRetClass, *uThreadObjClass;
	MonoException *except;
	MonoMethod *mainExMeth;
	MonoMethodSignature *mainExMethSig;
	MonoType *mainExRetType;

	g_assert (mmrUThread != NULL);

	uThreadObj = mmrUThread->uThreadObj;

	/*
	 * Find uThreadObj.MainEx().
	 */
	mainExMeth = NULL;
	for (uThreadObjClass = mono_object_class (uThreadObj); uThreadObjClass != NULL; uThreadObjClass = uThreadObjClass->parent) {
		iterm = NULL;
		while ((mainExMeth = mono_class_get_methods (uThreadObjClass, &iterm)) != NULL) {
			if (strcmp (mono_method_get_name (mainExMeth), "MainEx") != 0) continue;
			mainExMethSig = mono_method_signature (mainExMeth);
			if (mono_signature_get_param_count (mainExMethSig) == 0) break;
		}
		if (mainExMeth != NULL) break;
	}
	if (mainExMeth == NULL) {
		return mono_get_exception_argument ("uThreadObj", "no MainEx() method found");
	}

	/*
	 * Make sure uThreadObj.MainEx() returns System.Exception.
	 */
	mainExRetType = mono_signature_get_return_type (mainExMethSig);
	if (mono_type_get_type (mainExRetType) != MONO_TYPE_CLASS) goto badMainExRetType;
	mainExRetClass = mono_type_get_class (mainExRetType);
	if (mono_class_get_image (mainExRetClass) != mono_get_corlib ()) goto badMainExRetType;
	if (strcmp (mono_class_get_namespace (mainExRetClass), "System")    != 0) goto badMainExRetType;
	if (strcmp (mono_class_get_name      (mainExRetClass), "Exception") != 0) goto badMainExRetType;

	/*
	 * Compile uThreadObj.MainEx() so we have its start address.
	 */
	mainExCodeStart = mono_jit_compile_method (mainExMeth);
	if (mainExCodeStart == NULL) {
		return mono_get_exception_argument ("uThreadObj", "MainEx() compile failed");
	}

	/*
	 * Struct must not be in use anywhere and don't let anyone else use it.
	 */
	if (TSUTHREADBUSY(mmrUThread)) {
		return mono_get_exception_argument ("start", "MMRUThread already busy");
	}
	mmrUThread->entry = mainExCodeStart;

	/*
	 * Get an actual stack for microthread to run on.
	 */
	if (mmrUThread->microStkAddr == NULL) {
		AllocStack (mmrUThread);
		if (mmrUThread->microStkAddr == NULL) {
			fprintf (stderr, "mmruthread_start: no memory for %lu byte stack\n", mmrUThread->microStkSize);
			CLRUTHREADBUSY(mmrUThread);
			return mono_get_exception_out_of_memory ();
		}
	}

	/*
	 * Initialize microthread's LMF chain.
	 */
	mmrUThread->microLMFPtr = &mmrUThread->microLMF;

	/*
	 * Save boundaries of original stack so we can restore them when uthread next suspends or exits.
	 */
	GetCurrentStackBounds (&mmrUThread->macroStkAddr, &mmrUThread->macroStkSize);

	/*
	 * Call the given MainEx() routine on the microthread stack.
	 * Save current macrothread stack position so microthread can return to it.
	 *
	 * For AMD64, call stack is always:
	 *        RBX = MMRUThread struct pointer
	 *    -8(RSP) = return address
	 *
	 * For X86, call stack is always:
	 *    -4(ESP) = saved EBX
	 *    -8(ESP) = MMRUThread struct pointer
	 *   -12(ESP) = return address
	 * We have to save/restore EBX because it is the PIC base register.
	 *
	 * Porting:
	 *   1) Save macro thread's registers that the ABI requires be saved across calls but
	 *      that cannot be specified in the asm(...) statements modified register list,
	 *      such as the stack and frame pointers.
	 *   2) Call SetMicroStk() passing it mmrUThread
	 *   3) Call CallIt() passing it mmrUThread
	 *   4) Put CallIt()'s return value in except
	 */
	mmrUThread->active = true;

	mmruthread_lockgc ();
	{
#if defined(TARGET_AMD64)
		gulong rbxGetsWiped, rcxGetsWiped;

		asm volatile (
			"	movq	%%rbp,%c[macBP](%%rbx)	\n"	// save frame and stack pointers for 2nd half of mmruthread_start()
			"	movq	%%rsp,%c[macSP](%%rbx)	\n"
			"	xorl	%%ebp,%%ebp		\n"	// start out MainEx() with a null frame
			"	movq	%%rcx,%%rsp		\n"	// start its stack at end of allocated stack region
			"	movq	%%rbx,%%rdi		\n"	// tell garbage collector we are now on the uthread's stack
			"	call	SetMicroStk		\n"
			"	movq	%%rbx,%%rdi		\n"
			"	call	CallIt			\n"	// call MainEx().  rtn addr gets stuck at -8(%rbx).
			: "=a" (except),
			  "=b" (rbxGetsWiped),
			  "=c" (rcxGetsWiped)
			: "1" (mmrUThread),
			  "2" (mmrUThread->microStkAddr + mmrUThread->microStkSize),
			  [macBP] "i" (offsetof (MMRUThread, macBP)), 
			  [macSP] "i" (offsetof (MMRUThread, macSP))
			: "cc", "memory", "rdx", "rsi", "rdi", "r8", 
			  "r9", "r10", "r11", "r12", "r13", "r14", "r15");
#endif
#if defined(TARGET_X86)
		gulong ecxGetsWiped, esiGetsWiped;

		asm volatile (
			"	movl	%%ebp,%c[macBP](%%esi)	\n"	// save frame and stack pointers for 2nd half of mmruthread_start()
			"	movl	%%esp,%c[macSP](%%esi)	\n"
			"	movl	%%ebx,%c[macBX](%%esi)	\n"
			"	xorl	%%ebp,%%ebp		\n"	// start out MainEx() with a null frame
			"	movl	%%ecx,%%esp		\n"	// start its stack at end of allocated stack region
			"	movl	%%esi,%%ecx		\n"	// tell garbage collector we are now on the uthread's stack
			"	call	SetMicroStk		\n"
			"	movl	%%esi,%%ecx		\n"
			"	call	CallIt			\n"	// call MainEx().  rtn addr gets stuck at -8(%esi).
			: "=a" (except),
			  "=S" (esiGetsWiped),
			  "=c" (ecxGetsWiped)
			: "1" (mmrUThread),
			  "2" (mmrUThread->microStkAddr + mmrUThread->microStkSize),
			  [macBX] "i" (offsetof (MMRUThread, macBX)), 
			  [macBP] "i" (offsetof (MMRUThread, macBP)), 
			  [macSP] "i" (offsetof (MMRUThread, macSP))
			: "cc", "memory", "edx", "edi");
#endif
	}
	mmruthread_unlkgc ();

	/*
	 * If thread exited, free its stack.
	 */
	if (!mmrUThread->active) {
		FreeStack (mmrUThread);
	}

	return except;

badMainExRetType:
	return mono_get_exception_argument ("uThreadObj", "MainEx() doesn't return System.Exception");
}


/**
 * @brief wrapper for MainEx()
 */
static ASMCALLATTR void
CallIt (MMRUThread *mmrUThread)
{
	MonoException *except;

	mmruthread_unlkgc ();
	except = (*mmrUThread->entry) (mmrUThread->uThreadObj);
	mmruthread_suspend (mmrUThread, except, true);
	g_assert (0);	// we really can't return from here anyway because
			// ... we don't have any frame to return out to.
}


/**
 * @brief suspend execution of current UThread, return to whoever started or resumed it last.
 * @param except = exception to return to the starter/resumer.
 * @param exit = false: uthread can be resumed, state changed to suspend
 *                true: uthread cannot be resumed, state changed to inactive
 * @returns NULL or exception pointer
 *          returned exception possible sources:
 *            1) generated by mmruthread_suspend code itself, eg, uthread not running
 *            2) as passed to ResumeEx()
 */
static MonoException *
mmruthread_suspend (MMRUThread *mmrUThread, MonoException *except, _Bool exit)
{
	g_assert (mmrUThread != NULL);
	g_assert (mmrUThread->busy);

	/*
	 * Don't let it be resumed.  Active() will indicate it has exited.
	 */
	if (exit) {
		mmrUThread->active = false;
		mmrUThread->micSP = 0;		// tell SetMacroStk() to tell garbage collector to forget about uthread stack
		mmrUThread->micBP = 0;
	}

	/*
	 * Save how to return to suspend point on microthread stack then
	 * jump to where it was started or resumed from on macrothread stack.
	 *
	 * Porting:
	 *   if (!exit) {
	 *     save current registers to microthread register save area in mmrUThread
	 *   }
	 *   swap return address on stack with 2f
	 *   load current registers from macrothread register save area in mmrUThread
	 *   SetMacroStack(mmrUThread)
	 *   get 'except' value loaded into return value register
	 *   jump to swapped return address = 
	 *       just past the 'call CallIt' in mmruthread_start() or 
	 *                 the '1:' in mmruthread_resume()
	 *  2:
	 */
	mmruthread_lockgc ();
	{
#if defined(TARGET_AMD64)
		gulong rcxGetsWiped;
		register gulong r13 asm ("r13") = (gulong)except;

		asm volatile (
			"	cmpb	$0,%%al			\n"
			"	jne	1f			\n"
			"	movq	%%rbp,%c[micBP](%%rbx)	\n"	// save frame and stack pointers for 2nd half of mmruthread_suspend()
			"	movq	%%rsp,%c[micSP](%%rbx)	\n"	// ... but only if not exiting so SetMacroStkBound will release stack if exiting
			"1:					\n"
			"	leaq	2f(%%rip),%%rax		\n"	// point to 2nd half of mmruthread_suspend()
			"	movq	-8(%%rcx),%%r12		\n"	// point to 2nd half of mmruthread_start() or mmruthread_resume()
			"	movq	%%rax,-8(%%rcx)		\n"
			"	movq	%c[macBP](%%rbx),%%rbp	\n"	// load frame and stack pointers for 2nd half of mmruthread_{start,resume}()
			"	movq	%c[macSP](%%rbx),%%rsp	\n"
			"	movq	%%rbx,%%rdi		\n"	// tell garbage collector we are now on the original stack
			"	call	SetMacroStk		\n"
			"	movq	%%r13,%%rax		\n"
			"	jmp	*%%r12			\n"	// jump to 2nd half of mmruthread_{start,resume}()
			"2:					\n"
			: "=a" (except),
			  "+r" (r13),
			  "=c" (rcxGetsWiped)
			: "b" (mmrUThread),
			  "0" (exit),
			  "2" (mmrUThread->microStkAddr + mmrUThread->microStkSize),
			  [micBP] "i" (offsetof (MMRUThread, micBP)), 
			  [micSP] "i" (offsetof (MMRUThread, micSP)), 
			  [macBP] "i" (offsetof (MMRUThread, macBP)), 
			  [macSP] "i" (offsetof (MMRUThread, macSP))
			: "cc", "memory", "rdx", "rsi", "rdi", 
			  "r8", "r9", "r10", "r11", "r12", "r14", "r15");
#endif
#if defined(TARGET_X86)
		gulong ecxGetsWiped;

		asm volatile (
			"	cmpb	$0,%%al			\n"
			"	jne	1f			\n"
			"	movl	%%ebp,%c[micBP](%%esi)	\n"	// save frame and stack pointers for 2nd half of mmruthread_suspend()
			"	movl	%%esp,%c[micSP](%%esi)	\n"	// ... but only if not exiting so SetMacroStkBound will release stack if exiting
			"	movl	%%ebx,%c[micBX](%%esi)	\n"
			"1:					\n"
			"	leal	2f,%%eax		\n"	// point to 2nd half of mmruthread_suspend()
			"	movl	-4(%%ecx),%%edx		\n"	// point to 2nd half of mmruthread_start() or mmruthread_resume()
			"	movl	%%eax,-4(%%ecx)		\n"
			"	movl	%c[macBP](%%esi),%%ebp	\n"	// load frame and stack pointers for 2nd half of mmruthread_{start,resume}()
			"	movl	%c[macSP](%%esi),%%esp	\n"
			"	movl	%c[macBX](%%esi),%%ebx	\n"
			"	pushl	%%edx			\n"
			"	movl	%%esi,%%ecx		\n"	// tell garbage collector we are now on the original stack
			"	call	SetMacroStk		\n"
			"	movl	%%edi,%%eax		\n"
			"	ret				\n"	// jump to 2nd half of mmruthread_{start,resume}()
			"2:					\n"
			: "=a" (except),
			  "=c" (ecxGetsWiped)
			: "S" (mmrUThread),
			  "D" (except),
			  "0" (exit),
			  "1" (mmrUThread->microStkAddr + mmrUThread->microStkSize),
			  [micBX] "i" (offsetof (MMRUThread, micBX)), 
			  [micBP] "i" (offsetof (MMRUThread, micBP)), 
			  [micSP] "i" (offsetof (MMRUThread, micSP)), 
			  [macBX] "i" (offsetof (MMRUThread, macBX)), 
			  [macBP] "i" (offsetof (MMRUThread, macBP)), 
			  [macSP] "i" (offsetof (MMRUThread, macSP))
			: "cc", "memory", "edx");
#endif
	}
	mmruthread_unlkgc ();

	return except;
}


/**
 * @brief resume execution of given UThread, return to where it suspended.
 * @param except = exception to return to the suspender.
 * @returns NULL or exception pointer
 *          returned exception possible sources:
 *            1) generated by mmruthread_resume code itself, eg, uthread already running
 *            2) as resumed code passes to SuspendEx() or ExitEx()
 */
static MonoException *
mmruthread_resume (MMRUThread *mmrUThread, MonoException *except)
{
	g_assert (mmrUThread != NULL);

	/*
	 * Struct must not be in use anywhere and don't let anyone else use it.
	 */
	if (TSUTHREADBUSY(mmrUThread)) {
		return mono_get_exception_argument ("mmrUThread", "MMRUThread already busy");
	}

	/*
	 * Thread being resumed must be active so we have a stack and instructions to resume to.
	 */
	if (!mmrUThread->active) {
		CLRUTHREADBUSY(mmrUThread);
		return mono_get_exception_argument ("mmrUThread", "MMRUThread not active");
	}

	/*
	 * Save boundaries of original stack so we can restore them when uthread suspends or exits.
	 */
	GetCurrentStackBounds (&mmrUThread->macroStkAddr, &mmrUThread->macroStkSize);

	/*
	 * Save how to return to our caller on macrothread stack then 
	 * jump to where mmrUThread suspended on microthread stack.
	 *
	 * Saves original thread stack info in mmrUThread->macroR.P
	 * then loads uthread stack info from mmrUThread->microR.P.
	 *
	 * Porting:
	 *   save current registers to macrothread register save area in mmrUThread
	 *   swap return address on stack with 1f
	 *   load current registers from microthread register save area in mmrUThread
	 *   SetMicroStack(mmrUThread)
	 *   get 'except' value loaded in return value register
	 *   jump to swapped return address = at the 2: in mmruthread_suspend()
	 *  1:
	 */
	mmruthread_lockgc ();
	{
#if defined(TARGET_AMD64)
		gulong rbxGetsWiped, rcxGetsWiped;
		register gulong r13 asm ("r13") = (gulong)except;

		asm volatile (
			"	movq	%%rbp,%c[macBP](%%rbx)	\n"	// save resume frame and stack pointers
			"	movq	%%rsp,%c[macSP](%%rbx)	\n"
			"	leaq	1f(%%rip),%%rax		\n"	// point to 2nd half of mmruthread_resume()
			"	movq	-8(%%rcx),%%r12		\n"	// point to 2nd half of mmruthread_suspend()
			"	movq	%%rax,-8(%%rcx)		\n"	// save pointer to 2nd half of mmruthread_resume()
			"	movq	%c[micBP](%%rbx),%%rbp	\n"	// restore suspended frame and stack pointers
			"	movq	%c[micSP](%%rbx),%%rsp	\n"
			"	movq	%%rbx,%%rdi		\n"	// tell garbage collector we are now on the uthread's stack
			"	call	SetMicroStk		\n"
			"	movq	%%r13,%%rax		\n"
			"	jmp	*%%r12			\n"	// jump to 2nd half of mmruthread_suspend()
			"1:					\n"
			: "=a" (except),
			  "+r" (r13),
			  "=b" (rbxGetsWiped),
			  "=c" (rcxGetsWiped)
			: "2" (mmrUThread),
			  "3" (mmrUThread->microStkAddr + mmrUThread->microStkSize),
			  [micBP] "i" (offsetof (MMRUThread, micBP)), 
			  [micSP] "i" (offsetof (MMRUThread, micSP)), 
			  [macBP] "i" (offsetof (MMRUThread, macBP)), 
			  [macSP] "i" (offsetof (MMRUThread, macSP))
			: "cc", "memory", "rdx", "rsi", "rdi", 
			  "r8", "r9", "r10", "r11", "r12", "r14", "r15");
#endif
#if defined(TARGET_X86)
		gulong ecxGetsWiped, esiGetsWiped;

		asm volatile (
			"	movl	%%ebp,%c[macBP](%%esi)	\n"	// save resume frame and stack pointers
			"	movl	%%esp,%c[macSP](%%esi)	\n"
			"	movl	%%ebx,%c[macBX](%%esi)	\n"
			"	leal	1f,%%eax		\n"	// point to 2nd half of mmruthread_resume()
			"	movl	-4(%%ecx),%%edx		\n"	// point to 2nd half of mmruthread_suspend()
			"	movl	%%eax,-4(%%ecx)		\n"	// save pointer to 2nd half of mmruthread_resume()
			"	movl	%c[micBP](%%esi),%%ebp	\n"	// restore suspended frame and stack pointers
			"	movl	%c[micSP](%%esi),%%esp	\n"
			"	movl	%c[micBX](%%esi),%%ebx	\n"
			"	pushl	%%edx			\n"
			"	movl	%%esi,%%ecx		\n"	// tell garbage collector we are now on the uthread's stack
			"	call	SetMicroStk		\n"
			"	movl	%%edi,%%eax		\n"
			"	ret				\n"	// jump to 2nd half of mmruthread_suspend()
			"1:					\n"
			: "=a" (except),
			  "=S" (esiGetsWiped),
			  "=c" (ecxGetsWiped)
			: "1" (mmrUThread),
			  "2" (mmrUThread->microStkAddr + mmrUThread->microStkSize),
			  "D" (except),
			  [micBX] "i" (offsetof (MMRUThread, micBX)), 
			  [micBP] "i" (offsetof (MMRUThread, micBP)), 
			  [micSP] "i" (offsetof (MMRUThread, micSP)), 
			  [macBX] "i" (offsetof (MMRUThread, macBX)), 
			  [macBP] "i" (offsetof (MMRUThread, macBP)), 
			  [macSP] "i" (offsetof (MMRUThread, macSP))
			: "cc", "memory", "edx");
#endif
	}
	mmruthread_unlkgc ();

	/*
	 * If thread exited, free its stack.
	 */
	if (!mmrUThread->active) {
		FreeStack (mmrUThread);
	}

	return except;
}


/**
 * @brief say what the thread's state is if anyone cares
 * @returns 0: inactive (start not called yet or suspend(exit=true) has been called)
 *         -1: suspended (suspend(exit=false) has been called but not resumed)
 *          1: running (start or resume has been called but not suspended)
 */
static int
mmruthread_active (MMRUThread *mmrUThread)
{
	// busy trumps active, so if busy say it's busy
	// if not busy, then it's either idle or inactive
	return mmrUThread->busy ? 1 : (mmrUThread->active ? -1 : 0);
}


/**
 * @brief allocate stack for microthread
 * @param mmrUThread->microStkSize = number of bytes to allocate (page aligned)
 * @returns mmrUThread->microStkAddr == NULL: no memory
 *                                      else: bottom of stack area
 */
static void
AllocStack(MMRUThread *mmrUThread)
{
	FreeStackElement *freeStack, **lFreeStack;

	pthread_mutex_lock (&freeStackMutex);

	g_assert (mmrUThread->microStkAddr == NULL);

	for (lFreeStack = &freeStackList; (freeStack = *lFreeStack) != NULL; lFreeStack = &freeStack->nextStack) {
		if (freeStack->stackSize == mmrUThread->microStkSize) break;
	}
	if (freeStack != NULL) {
		*lFreeStack = freeStack->nextStack;
		mmrUThread->microStkAddr = (void *)freeStack;
		pthread_mutex_unlock (&freeStackMutex);
	} else {
		numStackBytes += mmrUThread->microStkSize;
		pthread_mutex_unlock (&freeStackMutex);

		mmrUThread->microStkAddr = mono_valloc (NULL, mmrUThread->microStkSize + GUARD_SIZE,
		                                        MONO_MMAP_READ|MONO_MMAP_WRITE|MONO_MMAP_PRIVATE|MONO_MMAP_ANON);
		if (mmrUThread->microStkAddr == NULL) {
			pthread_mutex_lock (&freeStackMutex);
			numStackBytes -= mmrUThread->microStkSize;
			pthread_mutex_unlock (&freeStackMutex);
			return;
		}
		mono_mprotect (mmrUThread->microStkAddr, GUARD_SIZE, MONO_MMAP_NONE);
		mmrUThread->microStkAddr += GUARD_SIZE;
	}
	PRINTF ("AllocStack*: str=%p, stk=%lX..%lX, size=%u\n", 
			mmrUThread, 
			(gulong)mmrUThread->microStkAddr, 
			(gulong)mmrUThread->microStkAddr + mmrUThread->microStkSize - 1, 
			mmrUThread->microStkSize);
}


/**
 * @brief Free stack from microthread as it has called exit or is being destroyed.
 */
static void
FreeStack (MMRUThread *mmrUThread)
{
	FreeStackElement *freeStack;

	pthread_mutex_lock (&freeStackMutex);

	PRINTF ("FreeStack*: str=%p, stk=%lX..%lX, size=%u\n", 
			mmrUThread, 
			(gulong)mmrUThread->microStkAddr, 
			(gulong)mmrUThread->microStkAddr + mmrUThread->microStkSize - 1, 
			mmrUThread->microStkSize);

	freeStack = (void *)mmrUThread->microStkAddr;
	mmrUThread->microStkAddr = NULL;

	g_assert (freeStack != NULL);       // we must have something to free
	g_assert (mmrUThread->micSP == 0);  // gargabe collector should be off the stack by now

	freeStack->nextStack = freeStackList;
	freeStack->stackSize = mmrUThread->microStkSize;

	freeStackList = freeStack;

	pthread_mutex_unlock (&freeStackMutex);
}


/**
 * @brief we just switched to uthread's stack
 *        so we must:
 *          1) if not first time switching, deregister uthread stack as an object with fixed limits to be scanned
 *          2) register original stack as a garbage-collectable object with fixed limits to be scanned
 *          3) set up uthread stack as the current garbage-collectable stack (variable lower limit)
 */
static ASMCALLATTR void
SetMicroStk (MMRUThread *mmrUThread)
{
	MonoLMF **lmfPtrPtr;

	/*
	 * Save macrothread's LMF pointer and restore microthread's LMF pointer.
	 */
	lmfPtrPtr = mono_get_lmf_addr ();
	mmrUThread->macroLMFPtr = *lmfPtrPtr;
	*lmfPtrPtr = mmrUThread->microLMFPtr;

	/*
	 * Deregister uthread stack as a garbage-collectable object as we will soon tell GC that it is current stack.
	 * We want GC to just check the dynamic limits, not whatever was last used, as we don't know how long it will be active.
	 */
	if (mmrUThread->micSP != 0) {
		PRINTF ("SetMicroStk*: dereg %lX..%lX\n", mmrUThread->micSP, (gulong)mmrUThread->microStkAddr + mmrUThread->microStkSize - 1);
		mmruthread_remroot ((char *)mmrUThread->micSP, (char *)mmrUThread->microStkAddr + mmrUThread->microStkSize);
	}

	/*
	 * Register original stack as a garbage-collectable object because it no longer is a stack that GC will see.
	 */
	PRINTF ("SetMicroStk*: reg %lX..%lX\n", mmrUThread->macSP, (gulong)mmrUThread->macroStkAddr + mmrUThread->macroStkSize - 1);
	mmruthread_addroot ((char *)mmrUThread->macSP, (char *)mmrUThread->macroStkAddr + mmrUThread->macroStkSize);

	/*
	 * Tell GC end of uthread stack.
	 */
	SetCurrentStackBounds (mmrUThread->microStkAddr, mmrUThread->microStkSize);
}


/**
 * @brief we just switched to original stack from uthread's stack
 *        so we must:
 *          1) deregister original stack as an object with fixed limits to be scanned
 *          2) if uthread not exiting, register uthread stack as a garbage-collectable object with fixed limits to be scanned
 *          3) set up original stack as the current garbage-collectable stack (variable lower limit)
 */
static ASMCALLATTR void
SetMacroStk (MMRUThread *mmrUThread)
{
	MonoLMF **lmfPtrPtr;

	/*
	 * Deregister original stack as a garbage-collectable object as we will soon tell GC that it is current stack.
	 * We want GC to just check the dynamic limits, not whatever was last used, as we don't know how long it will be active.
	 */
	PRINTF ("SetMacroStk*: dereg %lX..%lX\n", mmrUThread->macSP, (gulong)mmrUThread->macroStkAddr + mmrUThread->macroStkSize - 1);
	mmruthread_remroot ((char *)mmrUThread->macSP, (char *)mmrUThread->macroStkAddr + mmrUThread->macroStkSize);

	/*
	 * Register uthread stack as a garbage-collectable object because it no longer is a stack that GC will see.
	 * But don't bother if uthread is exiting because we don't care about any of its objects anymore.
	 */
	if (mmrUThread->micSP != 0) {
		PRINTF ("SetMicroStk*: reg %lX..%lX\n", mmrUThread->micSP, (gulong)mmrUThread->microStkAddr + mmrUThread->microStkSize - 1);
		mmruthread_addroot ((char *)mmrUThread->micSP, (char *)mmrUThread->microStkAddr + mmrUThread->microStkSize);
	}

	/*
	 * Tell GC end of original stack.
	 */
	SetCurrentStackBounds (mmrUThread->macroStkAddr, mmrUThread->macroStkSize);

	/*
	 * Save microthread's LMF pointer and restore macrothread's LMF pointer.
	 */
	lmfPtrPtr = mono_get_lmf_addr ();
	mmrUThread->microLMFPtr = *lmfPtrPtr;
	*lmfPtrPtr = mmrUThread->macroLMFPtr;

	/*
	 * Let someone activate mmrUThread now if they want as its micSP,micBP is valid
	 * and established as a gc root, and we don't need macroStk{Addr,Size} any more.
	 */
	CLRUTHREADBUSY(mmrUThread);
}


/**
 * @brief get current thread's stack address and size
 * @param base where to return lowest stack address
 * @param size where to return stack size in bytes
 */
static void
GetCurrentStackBounds (guint8 **base, gulong *size)
{
	MonoJitTlsData *tls;

	tls = mono_native_tls_get_value (mono_jit_tls_id);
	g_assert (tls->end_of_stack != NULL);                // these should always be filled in by now
	g_assert (tls->stack_size   != 0);
	*base = (guint8 *)tls->end_of_stack - tls->stack_size;
	*size = tls->stack_size;
}


/**
 * @brief Tell GC end of current thread's stack.  The TLS stuff is for exception handling.
 */
static void
SetCurrentStackBounds (guint8 *base, gulong size)
{
	MonoJitTlsData *tls;

	PRINTF ("SetCurrentStackBounds*: end %lX\n", (gulong)base + size);

	mmruthread_setstack (base + size);

	tls = mono_native_tls_get_value (mono_jit_tls_id);
	tls->end_of_stack = base + size;
	tls->stack_size   = size;
}
