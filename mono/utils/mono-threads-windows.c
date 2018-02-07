/**
 * \file
 * Low-level threading, windows version
 *
 * Author:
 *	Rodrigo Kumpera (kumpera@gmail.com)
 *
 * (C) 2011 Novell, Inc
 */

#include <mono/utils/mono-threads.h>

#if defined(USE_WINDOWS_BACKEND)

#include <mono/utils/mono-compiler.h>
#include <mono/utils/mono-threads-debug.h>
#include <mono/utils/mono-os-wait.h>
#include <limits.h>

void
mono_threads_suspend_init (void)
{
}

gboolean
mono_threads_suspend_begin_async_suspend (MonoThreadInfo *info, gboolean interrupt_kernel)
{
	DWORD id = mono_thread_info_get_tid (info);
	HANDLE handle;
	DWORD result;

	handle = OpenThread (THREAD_ALL_ACCESS, FALSE, id);
	g_assert (handle);

	result = SuspendThread (handle);
	THREADS_SUSPEND_DEBUG ("SUSPEND %p -> %d\n", (void*)id, ret);
	if (result == (DWORD)-1) {
		CloseHandle (handle);
		return FALSE;
	}

	/* Suspend logic assumes thread is really suspended before continuing below. Surprisingly SuspendThread */
	/* is just an async request to the scheduler, meaning that the suspended thread can continue to run */
	/* user mode code until scheduler gets around and process the request. This will cause a thread state race */
	/* in mono's thread state machine implementation on Windows. By requesting a threads context after issuing a */
	/* suspended request, this will wait until thread is suspended and thread context has been collected */
	/* and returned. */
	CONTEXT context;
	context.ContextFlags = CONTEXT_INTEGER | CONTEXT_CONTROL;
	if (!GetThreadContext (handle, &context)) {
		CloseHandle (handle);
		return FALSE;
	}

	/* We're in the middle of a self-suspend, resume and register */
	if (!mono_threads_transition_finish_async_suspend (info)) {
		mono_threads_add_to_pending_operation_set (info);
		result = ResumeThread (handle);
		g_assert (result == 1);
		CloseHandle (handle);
		THREADS_SUSPEND_DEBUG ("FAILSAFE RESUME/1 %p -> %d\n", (void*)id, 0);
		//XXX interrupt_kernel doesn't make sense in this case as the target is not in a syscall
		return TRUE;
	}
	info->suspend_can_continue = mono_threads_get_runtime_callbacks ()->thread_state_init_from_handle (&info->thread_saved_state [ASYNC_SUSPEND_STATE_INDEX], info, &context);
	THREADS_SUSPEND_DEBUG ("thread state %p -> %d\n", (void*)id, res);
	if (info->suspend_can_continue) {
		if (interrupt_kernel)
			mono_win32_interrupt_wait (info, handle, id);
	} else {
		THREADS_SUSPEND_DEBUG ("FAILSAFE RESUME/2 %p -> %d\n", (void*)info->native_handle, 0);
	}

	CloseHandle (handle);
	return TRUE;
}

gboolean
mono_threads_suspend_check_suspend_result (MonoThreadInfo *info)
{
	return info->suspend_can_continue;
}



void
mono_threads_suspend_abort_syscall (MonoThreadInfo *info)
{
	DWORD id = mono_thread_info_get_tid (info);
	HANDLE handle;

	handle = OpenThread (THREAD_ALL_ACCESS, FALSE, id);
	g_assert (handle);

	mono_win32_abort_wait (info, handle, id);

	CloseHandle (handle);
}

gboolean
mono_threads_suspend_begin_async_resume (MonoThreadInfo *info)
{
	DWORD id = mono_thread_info_get_tid (info);
	HANDLE handle;
	DWORD result;

	handle = OpenThread (THREAD_ALL_ACCESS, FALSE, id);
	g_assert (handle);

	if (info->async_target) {
		MonoContext ctx;
		CONTEXT context;
		gboolean res;

		ctx = info->thread_saved_state [ASYNC_SUSPEND_STATE_INDEX].ctx;
		mono_threads_get_runtime_callbacks ()->setup_async_callback (&ctx, info->async_target, info->user_data);
		info->async_target = info->user_data = NULL;

		context.ContextFlags = CONTEXT_INTEGER | CONTEXT_CONTROL;

		if (!GetThreadContext (handle, &context)) {
			CloseHandle (handle);
			return FALSE;
		}

		g_assert (context.ContextFlags & CONTEXT_INTEGER);
		g_assert (context.ContextFlags & CONTEXT_CONTROL);

		mono_monoctx_to_sigctx (&ctx, &context);

		context.ContextFlags = CONTEXT_INTEGER | CONTEXT_CONTROL;
		res = SetThreadContext (handle, &context);
		if (!res) {
			CloseHandle (handle);
			return FALSE;
		}
	}

	result = ResumeThread (handle);
	CloseHandle (handle);

	return result != (DWORD)-1;
}


void
mono_threads_suspend_register (MonoThreadInfo *info)
{
}

void
mono_threads_suspend_free (MonoThreadInfo *info)
{
}

void
mono_threads_suspend_init_signals (void)
{
}

gint
mono_threads_suspend_search_alternative_signal (void)
{
	g_assert_not_reached ();
}

gint
mono_threads_suspend_get_suspend_signal (void)
{
	return -1;
}

gint
mono_threads_suspend_get_restart_signal (void)
{
	return -1;
}

gint
mono_threads_suspend_get_abort_signal (void)
{
	return -1;
}

#endif

#if defined (HOST_WIN32)

gboolean
mono_thread_platform_create_thread (MonoThreadStart thread_fn, gpointer thread_data, gsize* const stack_size, MonoNativeThreadId *tid)
{
	HANDLE result;
	DWORD thread_id;

	result = CreateThread (NULL, stack_size ? *stack_size : 0, (LPTHREAD_START_ROUTINE) thread_fn, thread_data, 0, &thread_id);
	if (!result)
		return FALSE;

	/* A new handle is open when attaching
	 * the thread, so we don't need this one */
	CloseHandle (result);

	if (tid)
		*tid = thread_id;

	if (stack_size) {
		// TOOD: Use VirtualQuery to get correct value 
		// http://stackoverflow.com/questions/2480095/thread-stack-size-on-windows-visual-c
		*stack_size = 2 * 1024 * 1024;
	}

	return TRUE;
}


MonoNativeThreadId
mono_native_thread_id_get (void)
{
	return GetCurrentThreadId ();
}

gboolean
mono_native_thread_id_equals (MonoNativeThreadId id1, MonoNativeThreadId id2)
{
	return id1 == id2;
}

gboolean
mono_native_thread_create (MonoNativeThreadId *tid, gpointer func, gpointer arg)
{
	return CreateThread (NULL, 0, (func), (arg), 0, (tid)) != NULL;
}

gboolean
mono_native_thread_join_handle (HANDLE thread_handle, gboolean close_handle)
{
	DWORD res = WaitForSingleObject (thread_handle, INFINITE);

	if (close_handle)
		CloseHandle (thread_handle);

	return res != WAIT_FAILED;
}

gboolean
mono_native_thread_join (MonoNativeThreadId tid)
{
	HANDLE handle;

	if (!(handle = OpenThread (SYNCHRONIZE, TRUE, tid)))
		return FALSE;

	return mono_native_thread_join_handle (handle, TRUE);
}

#if HAVE_DECL___READFSDWORD==0
static MONO_ALWAYS_INLINE unsigned long long
__readfsdword (unsigned long offset)
{
	unsigned long value;
	//	__asm__("movl %%fs:%a[offset], %k[value]" : [value] "=q" (value) : [offset] "irm" (offset));
   __asm__ volatile ("movl    %%fs:%1,%0"
     : "=r" (value) ,"=m" ((*(volatile long *) offset)));
	return value;
}
#endif

void
mono_threads_platform_get_stack_bounds (guint8 **staddr, size_t *stsize)
{
	MEMORY_BASIC_INFORMATION meminfo;
#ifdef _WIN64
	/* win7 apis */
	NT_TIB* tib = (NT_TIB*)NtCurrentTeb();
	guint8 *stackTop = (guint8*)tib->StackBase;
	guint8 *stackBottom = (guint8*)tib->StackLimit;
#else
	/* http://en.wikipedia.org/wiki/Win32_Thread_Information_Block */
	void* tib = (void*)__readfsdword(0x18);
	guint8 *stackTop = (guint8*)*(int*)((char*)tib + 4);
	guint8 *stackBottom = (guint8*)*(int*)((char*)tib + 8);
#endif
	/*
	Windows stacks are expanded on demand, one page at time. The TIB reports
	only the currently allocated amount.
	VirtualQuery will return the actual limit for the bottom, which is what we want.
	*/
	if (VirtualQuery (&meminfo, &meminfo, sizeof (meminfo)) == sizeof (meminfo))
		stackBottom = MIN (stackBottom, (guint8*)meminfo.AllocationBase);

	*staddr = stackBottom;
	*stsize = stackTop - stackBottom;

}

#if SIZEOF_VOID_P == 4 && G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT)
typedef BOOL (WINAPI *LPFN_ISWOW64PROCESS) (HANDLE, PBOOL);
static gboolean is_wow64 = FALSE;
#endif

/* We do this at init time to avoid potential races with module opening */
void
mono_threads_platform_init (void)
{
#if SIZEOF_VOID_P == 4 && G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT)
	LPFN_ISWOW64PROCESS is_wow64_func = (LPFN_ISWOW64PROCESS) GetProcAddress (GetModuleHandle (TEXT ("kernel32")), "IsWow64Process");
	if (is_wow64_func)
		is_wow64_func (GetCurrentProcess (), &is_wow64);
#endif
}

/*
 * When running x86 process under x64 system syscalls are done through WoW64. This
 * needs to do a transition from x86 mode to x64 so it can syscall into the x64 system.
 * Apparently this transition invalidates the ESP that we would get from calling
 * GetThreadContext, so we would fail to scan parts of the thread stack. We attempt
 * to query whether the thread is in such a transition so we try to restart it later.
 * We check CONTEXT_EXCEPTION_ACTIVE for this, which is highly undocumented.
 *
 * A better understanding of this problem is that, wow64 processes do run
 * native 64 bit code, and their context does not fit in a 32bit context record.
 * And, then also, the 64code code maybe runs on a different stack.
 * There might be some desire to return the "last" 32bit context, prior to
 * entering 64bit mode, but apparently that does not work.
 *
 * CorCLR and Chakra can be seen to have similar code.
 * Cooperative suspend should fix this.
 */
gboolean
mono_threads_platform_in_critical_region (MonoNativeThreadId tid)
{
	gboolean ret = FALSE;
#if SIZEOF_VOID_P == 4 && G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT)
/* FIXME On cygwin these are not defined */
// FIXME get the values and define them here.
#if defined(CONTEXT_EXCEPTION_REQUEST) && defined(CONTEXT_EXCEPTION_REPORTING) && defined(CONTEXT_EXCEPTION_ACTIVE)
	if (is_wow64) {
		HANDLE handle = OpenThread (THREAD_ALL_ACCESS, FALSE, tid);
		if (handle) {
			CONTEXT context;
			ZeroMemory (&context, sizeof (CONTEXT));
			context.ContextFlags = CONTEXT_EXCEPTION_REQUEST;
			if (GetThreadContext (handle, &context)) {
				if ((context.ContextFlags & CONTEXT_EXCEPTION_REPORTING) &&
						(context.ContextFlags & CONTEXT_EXCEPTION_ACTIVE))
					ret = TRUE;
			}
			CloseHandle (handle);
		}
	}
#endif
#endif
	return ret;
}

gboolean
mono_threads_platform_yield (void)
{
	return SwitchToThread ();
}

void
mono_threads_platform_exit (gsize exit_code)
{
	ExitThread (exit_code);
}

int
mono_threads_get_max_stack_size (void)
{
	//FIXME
	return INT_MAX;
}

#if defined(_MSC_VER)
const DWORD MS_VC_EXCEPTION=0x406D1388;
#pragma pack(push,8)
typedef struct tagTHREADNAME_INFO
{
   DWORD dwType; // Must be 0x1000.
   LPCSTR szName; // Pointer to name (in user addr space).
   DWORD dwThreadID; // Thread ID (-1=caller thread).
  DWORD dwFlags; // Reserved for future use, must be zero.
} THREADNAME_INFO;
#pragma pack(pop)
#endif

void
mono_native_thread_set_name (MonoNativeThreadId tid, const char *name)
{
#if defined(_MSC_VER)
	/* http://msdn.microsoft.com/en-us/library/xcb2z8hs.aspx */
	THREADNAME_INFO info;
	info.dwType = 0x1000;
	info.szName = name;
	info.dwThreadID = tid;
	info.dwFlags = 0;
// FIXME The ability to talk to a debugger via RaiseException
// is compiler-portable, but the ability to __try/__except is maybe not.
// One could gate this with IsDebuggerPresent, however
// - The debugger can disappear right after it is called.
// - Which debugger? They do not likely all support it.
// You could isolate the call on a separate per-call thread and let the exception
// be unhandled, perhaps.
	__try {
		RaiseException( MS_VC_EXCEPTION, 0, sizeof(info)/sizeof(ULONG_PTR),       (ULONG_PTR*)&info );
	}
	__except(EXCEPTION_EXECUTE_HANDLER) {
	}
#endif
}

void
mono_native_thread_set_name_utf16 (MonoNativeThreadId tid, const gunichar2 *name)
{
// Bing: SetThreadDescription.
// https://msdn.microsoft.com/en-us/library/windows/desktop/mt774976(v=vs.85).aspx
// This mechanism has the following advantages over the older mechanism:
// - Supported by the kernel, not debuggers.
// - It works when debugger is not attached.
//   - Either before debugger is attached.
//   - Between detach and reattach.
//   - No debugger at all.
//   - This is arguably a memory footprint disadvantage.
// - There is an API to get the name, instead of being buried in the debugger.
// - ETW knows about it, so thread names appear in ETW traces.
// - Unicode (only) support.
// - Implemented by Mono team.
// - No exception used to implement it.
// - It works on handles instead of IDs, which is more idiomatic.
//
// It has the following disadvantage:
// - It is only present in newer versions of Windows -- requiring LoadLibrary/GetProcAddress.
// - The Xbox Win32 API is different, though the underlying kernel API is identical.
// - The Xbox360 API is another different.
//
// It is called "description" instead of name because SetThreadName is already taken.

// UWP does not allow LoadLibrary.
#if G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT)

	typedef HRESULT (WINAPI *PFN_SET_THREAD_DESCRIPTION) (HANDLE, PCWSTR);
	static PFN_SET_THREAD_DESCRIPTION setThreadDescription;
	static char sticky_failure;
	HMODULE kernel32;
	HANDLE threadHandle;
	
	// LOAD_LIBRARY_SEARCH_SYSTEM32 does not work on older systems,
	// but the API is not present on older systems either.
	//
	// LOAD_LIBRARY_SEARCH_SYSTEM32 is present on Windows 8 and newer,
	// and is available in a patch for Windows 7, that is often missing.
	
	if (sticky_failure)
		return;
	
	if (!setThreadDescription) {
		kernel32 = LoadLibraryExW(L"kernel32.dll", NULL, LOAD_LIBRARY_SEARCH_SYSTEM32);
		if (!kernel32)
			goto label_fail;
		setThreadDescription = (PFN_SET_THREAD_DESCRIPTION)GetProcAddress(Kernel32, "SetThreadDescription");
	}
	if (!setThreadDescription)
		goto label_fail;
	threadHandle = OpenThread (THREAD_SET_LIMITED_INFORMATION, FALSE, tid);
	if (!threadHandle)
		return; // not sticky
	setThreadDescription (threadHandle, name);
	CloseHandle (threadHandle);
	return;
label_fail:
	sticky_failure = TRUE;
	return;
#endif
}

#endif
