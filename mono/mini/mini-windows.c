/**
 * \file
 * POSIX signal handling support for Mono.
 *
 * Authors:
 *   Mono Team (mono-list@lists.ximian.com)
 *
 * Copyright 2001-2003 Ximian, Inc.
 * Copyright 2003-2008 Ximian, Inc.
 *
 * See LICENSE for licensing information.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#include <config.h>
#include <signal.h>
#include <math.h>
#include <conio.h>
#include <assert.h>

#include <mono/metadata/coree.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/loader.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/class.h>
#include <mono/metadata/object.h>
#include <mono/metadata/tokentype.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/threads.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/profiler-private.h>
#include <mono/metadata/mono-config.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/gc-internals.h>
#include <mono/metadata/threads-types.h>
#include <mono/metadata/verify.h>
#include <mono/metadata/verify-internals.h>
#include <mono/metadata/mempool-internals.h>
#include <mono/metadata/attach.h>
#include <mono/utils/mono-math.h>
#include <mono/utils/mono-compiler.h>
#include <mono/utils/mono-counters.h>
#include <mono/utils/mono-logger-internals.h>
#include <mono/utils/mono-mmap.h>
#include <mono/utils/dtrace.h>

#include "mini.h"
#include "mini-runtime.h"
#include "mini-windows.h"
#include <string.h>
#include <ctype.h>
#include "trace.h"
#include "version.h"

#include "jit-icalls.h"

#if G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT)
#include <mmsystem.h>
#endif

#define MONO_HANDLER_DELIMITER ','
#define MONO_HANDLER_DELIMITER_LEN G_N_ELEMENTS(MONO_HANDLER_DELIMITER)-1

#define MONO_HANDLER_ATEXIT_WAIT_KEYPRESS "atexit-waitkeypress"
#define MONO_HANDLER_ATEXIT_WAIT_KEYPRESS_LEN G_N_ELEMENTS(MONO_HANDLER_ATEXIT_WAIT_KEYPRESS)-1

// Typedefs used to setup handler table.
typedef void (*handler)(void);

typedef struct {
	const char * cmd;
	const int cmd_len;
	handler handler;
} HandlerItem;

#if G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT)
/**
* atexit_wait_keypress:
*
* This function is installed as an atexit function making sure that the console is not terminated before the end user has a chance to read the result.
* This can be handy in debug scenarios (running from within the debugger) since an exit of the process will close the console window
* without giving the end user a chance to look at the output before closed.
*/
static void
atexit_wait_keypress (void)
{

	fflush (stdin);

	printf ("Press any key to continue . . . ");
	fflush (stdout);

	_getch ();

	return;
}

/**
* install_atexit_wait_keypress:
*
* This function installs the wait keypress exit handler.
*/
static void
install_atexit_wait_keypress (void)
{
	atexit (atexit_wait_keypress);
	return;
}

#else

/**
* install_atexit_wait_keypress:
*
* Not supported on WINAPI family.
*/
static void
install_atexit_wait_keypress (void)
{
	return;
}

#endif /* G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT) */

// Table describing handlers that can be installed at process startup. Adding a new handler can be done by adding a new item to the table together with an install handler function.
const HandlerItem g_handler_items[] = { { MONO_HANDLER_ATEXIT_WAIT_KEYPRESS, MONO_HANDLER_ATEXIT_WAIT_KEYPRESS_LEN, install_atexit_wait_keypress },
					{ NULL, 0, NULL } };

/**
 * get_handler_arg_len:
 * @handlers: Get length of next handler.
 *
 * This function calculates the length of next handler included in argument.
 *
 * Returns: The length of next handler, if available.
 */
static size_t
get_next_handler_arg_len (const char *handlers)
{
	assert (handlers != NULL);

	size_t current_len = 0;
	const char *handler = strchr (handlers, MONO_HANDLER_DELIMITER);
	if (handler != NULL) {
		// Get length of next handler arg.
		current_len = (handler - handlers);
	} else {
		// Consume rest as length of next handler arg.
		current_len = strlen (handlers);
	}

	return current_len;
}

/**
 * install_custom_handler:
 * @handlers: Handlers included in --handler argument, example "atexit-waitkeypress,someothercmd,yetanothercmd".
 * @handler_arg_len: Output, length of consumed handler.
 *
 * This function installs the next handler included in @handlers parameter.
 *
 * Returns: TRUE on successful install, FALSE on failure or unrecognized handler.
 */
static gboolean
install_custom_handler (const char *handlers, size_t *handler_arg_len)
{
	gboolean result = FALSE;

	assert (handlers != NULL);
	assert (handler_arg_len);

	*handler_arg_len = get_next_handler_arg_len (handlers);
	for (int current_item = 0; current_item < G_N_ELEMENTS (g_handler_items); ++current_item) {
		const HandlerItem * handler_item = &g_handler_items [current_item];

		if (handler_item->cmd == NULL)
			continue;

		if (*handler_arg_len == handler_item->cmd_len && strncmp (handlers, handler_item->cmd, *handler_arg_len) == 0) {
			assert (handler_item->handler != NULL);
			handler_item->handler ();
			result = TRUE;
			break;
		}
	}
	return result;
}

void
mono_runtime_install_handlers (void)
{
#ifndef MONO_CROSS_COMPILE
	win32_seh_init();
	win32_seh_set_handler(SIGFPE, mono_sigfpe_signal_handler);
	win32_seh_set_handler(SIGILL, mono_sigill_signal_handler);
	win32_seh_set_handler(SIGSEGV, mono_sigsegv_signal_handler);
	if (mini_get_debug_options ()->handle_sigint)
		win32_seh_set_handler(SIGINT, mono_sigint_signal_handler);
#endif
}

gboolean
mono_runtime_install_custom_handlers (const char *handlers)
{
	gboolean result = FALSE;

	assert (handlers != NULL);
	while (*handlers != '\0') {
		size_t handler_arg_len = 0;

		result = install_custom_handler (handlers, &handler_arg_len);
		handlers += handler_arg_len;

		if (*handlers == MONO_HANDLER_DELIMITER)
			handlers++;
		if (!result)
			break;
	}

	return result;
}

void
mono_runtime_install_custom_handlers_usage (void)
{
	fprintf (stdout,
		 "Custom Handlers:\n"
		 "   --handlers=HANDLERS            Enable handler support, HANDLERS is a comma\n"
		 "                                  separated list of available handlers to install.\n"
		 "\n"
#if G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT)
		 "HANDLERS is composed of:\n"
		 "    atexit-waitkeypress           Install an atexit handler waiting for a keypress\n"
		 "                                  before exiting process.\n");
#else
		 "No handlers supported on current platform.\n");
#endif /* G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT) */
}

void
mono_runtime_cleanup_handlers (void)
{
#ifndef MONO_CROSS_COMPILE
	win32_seh_cleanup();
#endif
}

void
mono_init_native_crash_info (void)
{
	return;
}

void
mono_cleanup_native_crash_info (void)
{
	return;
}

#if G_HAVE_API_SUPPORT (HAVE_CLASSIC_WINAPI_SUPPORT | HAVE_UWP_WINAPI_SUPPORT)
/* mono_chain_signal:
 *
 *   Call the original signal handler for the signal given by the arguments, which
 * should be the same as for a signal handler. Returns TRUE if the original handler
 * was called, false otherwise.
 */
gboolean
MONO_SIG_HANDLER_SIGNATURE (mono_chain_signal)
{
	/* Set to FALSE to indicate that vectored exception handling should continue to look for handler */
	MONO_SIG_HANDLER_GET_INFO ()->handled = FALSE;
	return TRUE;
}

#ifndef MONO_CROSS_COMPILE
void
mono_dump_native_crash_info (const char *signal, MonoContext *mctx, MONO_SIG_HANDLER_INFO_TYPE *info)
{
	//TBD
}

void
mono_post_native_crash_handler (const char *signal, MonoContext *mctx, MONO_SIG_HANDLER_INFO_TYPE *info, gboolean crash_chaining)
{
	if (!crash_chaining)
		abort ();
}
#endif /* !MONO_CROSS_COMPILE */
#endif /* G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT | HAVE_UWP_WINAPI_SUPPORT) */

#if G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT)
static MMRESULT	g_timer_event = 0;
static HANDLE g_timer_main_thread = INVALID_HANDLE_VALUE;

static VOID
thread_timer_expired (HANDLE thread)
{
	CONTEXT context;

	context.ContextFlags = CONTEXT_CONTROL;
	if (GetThreadContext (thread, &context)) {
		guchar *ip;

#ifdef _WIN64
		ip = (guchar *) context.Rip;
#else
		ip = (guchar *) context.Eip;
#endif

		MONO_PROFILER_RAISE (sample_hit, (ip, &context));
	}
}

static VOID CALLBACK
timer_event_proc (UINT uID, UINT uMsg, DWORD_PTR dwUser, DWORD_PTR dw1, DWORD_PTR dw2)
{
	thread_timer_expired ((HANDLE)dwUser);
}

static VOID
stop_profiler_timer_event (void)
{
	if (g_timer_event != 0) {

		timeKillEvent (g_timer_event);
		g_timer_event = 0;
	}

	if (g_timer_main_thread != INVALID_HANDLE_VALUE) {

		CloseHandle (g_timer_main_thread);
		g_timer_main_thread = INVALID_HANDLE_VALUE;
	}
}

static VOID
start_profiler_timer_event (void)
{
	g_return_if_fail (g_timer_main_thread == INVALID_HANDLE_VALUE && g_timer_event == 0);

	TIMECAPS timecaps;

	if (timeGetDevCaps (&timecaps, sizeof (timecaps)) != TIMERR_NOERROR)
		return;

	g_timer_main_thread = OpenThread (READ_CONTROL | THREAD_GET_CONTEXT, FALSE, GetCurrentThreadId ());
	if (g_timer_main_thread == NULL)
		return;

	if (timeBeginPeriod (1) != TIMERR_NOERROR)
		return;

	g_timer_event = timeSetEvent (1, 0, (LPTIMECALLBACK)timer_event_proc, (DWORD_PTR)g_timer_main_thread, TIME_PERIODIC | TIME_KILL_SYNCHRONOUS);
	if (g_timer_event == 0) {
		timeEndPeriod (1);
		return;
	}
}

void
mono_runtime_setup_stat_profiler (void)
{
	start_profiler_timer_event ();
	return;
}

void
mono_runtime_shutdown_stat_profiler (void)
{
	stop_profiler_timer_event ();
	return;
}

gboolean
mono_setup_thread_context(DWORD thread_id, MonoContext *mono_context)
{
	HANDLE handle;
	CONTEXT context;

	g_assert (thread_id != GetCurrentThreadId ());

	handle = OpenThread (THREAD_ALL_ACCESS, FALSE, thread_id);
	g_assert (handle);

	context.ContextFlags = CONTEXT_INTEGER | CONTEXT_CONTROL;

	if (!GetThreadContext (handle, &context)) {
		CloseHandle (handle);
		return FALSE;
	}

	g_assert (context.ContextFlags & CONTEXT_INTEGER);
	g_assert (context.ContextFlags & CONTEXT_CONTROL);

	memset (mono_context, 0, sizeof (MonoContext));
	mono_sigctx_to_monoctx (&context, mono_context);

	CloseHandle (handle);
	return TRUE;
}
#endif /* G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT) */

gboolean
mono_thread_state_init_from_handle (MonoThreadUnwindState *tctx, MonoThreadInfo *info, void *sigctx)
{
	tctx->valid = FALSE;
	tctx->unwind_data [MONO_UNWIND_DATA_DOMAIN] = NULL;
	tctx->unwind_data [MONO_UNWIND_DATA_LMF] = NULL;
	tctx->unwind_data [MONO_UNWIND_DATA_JIT_TLS] = NULL;

	if (sigctx == NULL) {
		DWORD id = mono_thread_info_get_tid (info);
		mono_setup_thread_context (id, &tctx->ctx);
	} else {
		g_assert (((CONTEXT *)sigctx)->ContextFlags & CONTEXT_INTEGER);
		g_assert (((CONTEXT *)sigctx)->ContextFlags & CONTEXT_CONTROL);
		mono_sigctx_to_monoctx (sigctx, &tctx->ctx);
	}

	/* mono_set_jit_tls () sets this */
	void *jit_tls = mono_thread_info_tls_get (info, TLS_KEY_JIT_TLS);
	/* SET_APPDOMAIN () sets this */
	void *domain = mono_thread_info_tls_get (info, TLS_KEY_DOMAIN);

	/*Thread already started to cleanup, can no longer capture unwind state*/
	if (!jit_tls || !domain)
		return FALSE;

	/*
	 * The current LMF address is kept in a separate TLS variable, and its hard to read its value without
	 * arch-specific code. But the address of the TLS variable is stored in another TLS variable which
	 * can be accessed through MonoThreadInfo.
	 */
	/* mono_set_lmf_addr () sets this */
	MonoLMF *lmf = NULL;
	MonoLMF **addr = (MonoLMF**)mono_thread_info_tls_get (info, TLS_KEY_LMF_ADDR);
	if (addr)
		lmf = *addr;

	tctx->unwind_data [MONO_UNWIND_DATA_DOMAIN] = domain;
	tctx->unwind_data [MONO_UNWIND_DATA_JIT_TLS] = jit_tls;
	tctx->unwind_data [MONO_UNWIND_DATA_LMF] = lmf;
	tctx->valid = TRUE;

	return TRUE;
}

BOOL
mono_win32_runtime_tls_callback (HMODULE module_handle, DWORD reason, LPVOID reserved, MonoWin32TLSCallbackType callback_type)
{
	if (!mono_win32_handle_tls_callback_type (callback_type))
		return TRUE;

	if (!mono_gc_dllmain (module_handle, reason, reserved))
		return FALSE;

	switch (reason)
	{
	case DLL_PROCESS_ATTACH:
		mono_install_runtime_load (mini_init);
		break;
	case DLL_PROCESS_DETACH:
		if (coree_module_handle)
			FreeLibrary (coree_module_handle);
		break;
	case DLL_THREAD_DETACH:
		mono_thread_info_detach ();
		break;

	}
	return TRUE;
}

#if HOST_WIN32 && defined (MONO_KEYWORD_THREAD)

guint8*
mono_windows_emit_tls_get (guint8* code, int dreg, int tls_offset)
{
	// This is documented here:
	//
	// https://docs.microsoft.com/en-us/windows/win32/debug/pe-format
	//
	// Executable code accesses a static TLS data object through the following steps:
	//
	// At link time, the linker sets the Address of Index field of the TLS directory.
	//
	// This field points to a location where the program expects to receive the TLS index.
	//
	// The Microsoft run-time library facilitates this process by defining a memory image
	// of the TLS directory and giving it the special name "__tls_used" (Intel x86 platforms)
	// or "_tls_used" (other platforms).
	//
	// The linker looks for this memory image and uses the data there to create the TLS directory.
	//
	// Other compilers that support TLS and work with the Microsoft linker must use this same technique.
	//
	// When a thread is created,
	//	[jaykrell: Or when a .dll is loaded, after thread create, retroactively for existing threads.]
	// the loader communicates the address of the thread's TLS array by
	// placing the address of the thread environment block (TEB) in the FS register. A pointer to
	// the TLS array is at the offset of 0x2C from the beginning of TEB. This behavior is Intel x86-specific.
	//	[jaykrell: For Win64 it is 0x2C times 2. For AMD64 it is GS:. This is obvious from compiler output].
	//
	// The loader assigns the value of the TLS index to the place that was indicated by the Address of Index field.
	//
	// The executable code retrieves the TLS index and also the location of the TLS array.
	//
	// The code uses the TLS index and the TLS array location (multiplying the index by 4 [jaykrell: or 8]
	// and using it as an offset to the array) to get the address of the TLS data area for the given program
	// and module. Each thread has its own TLS data area, but this is transparent to the program,
	// which does not need to know how data is allocated for individual threads.
	//
	// An individual TLS data object is accessed as some fixed offset into the TLS data area.
	//
	// The TLS array is an array of addresses that the system maintains for each thread. Each address in
	// this array gives the location of TLS data for a given module (EXE or DLL) within the program.
	// The TLS index indicates which member of the array to use. The index is a number (meaningful only
	// to the system) that identifies the module.
	//
	// [End of documentation]
	//
	// The documentation is out of date on some details that do not matter.
	//   - It immediately prior contains a caveat that was fixed in Vista.
	//   - Exactly when the operating system does the allocation/free does not matter.
	//     That is, this works even if a thread is created before a .dll is loaded.
	//
	//   - The linker/compiler interface to the operating system remains unchanged and correct.
	//
	// For example:
	//
	// __declspec (thread) int a [10];
	//
	// __declspec (dllexport)
	// int f1 (void)
	// {
	//	return a [0] + a [1] + a [2];
	// }
	//
	// cl /O2 /Zi /MD /LD 1.c /link /incremental:no
	// link /dump /disasm 1.dll | more
	//
	// f1:
	// 0000000180001000: 8B 0D 3A 30 00 00  mov         ecx,dword ptr [_tls_index] ; ecx = _tls_index
	// 0000000180001006: 65 48 8B 04 25 58  mov         rax,qword ptr gs:[58h]     ; rax = Teb->TlsVector
	//	00 00 00
	// 000000018000100F: BA 08 00 00 00     mov         edx,8                      ; edx = tls_offset common subexpression (tls_offset of a)
	// 0000000180001014: 48 8B 04 C8        mov         rax,qword ptr [rax+rcx*8]  ; rax = Teb->TlsVector [_tls_index]
	// 0000000180001018: 8B 4C 10 08        mov         ecx,dword ptr [rax+rdx+8]  ; ecx = *(Teb->TlsVector [_tls_index] + 16)
	// 000000018000101C: 03 4C 10 04        add         ecx,dword ptr [rax+rdx+4]  ; ecx += *(Teb->TlsVector [_tls_index] + 12)
	// 0000000180001020: 03 0C 10           add         ecx,dword ptr [rax+rdx]    ; ecx += *(Teb->TlsVector [_tls_index] + 8)
	// 0000000180001023: 8B C1              mov         eax,ecx
	// 0000000180001025: C3                 ret
	//
	// Use dreg as the only temp.
	// _tls_index and tls_offset can be immediates in JIT.
	//
	// FIXME For AOT, we could export the offsets from mono runtime
	// and access [__imp__tls_index] and [__imp__tls_offset] (for each tls_offset)
	//
	// tls_offset is determined at mono link-time but indirection is still a good idea.
	// _tls_index is determined at load time and cannot be output to AOT.
	//
	const guint size = sizeof (char*); // documentation says "4" here

 #if TARGET_X86
	x86_prefix (code, X86_FS_PREFIX);

	// FIXME Enable x86/amd64 code sharing. The interfaces match.
#define amd64_mov_reg_mem       x86_mov_reg_mem
#define amd64_mov_reg_membase   x86_mov_reg_membase

#elif TARGET_AMD64
	x86_prefix (code, X86_GS_PREFIX);
#else
	#error unknown target
#endif
	amd64_mov_reg_mem (code, dreg, MONO_WINDOWS_TLS_VECTOR, size);     // dreg = Teb->TlsVector
	amd64_mov_reg_membase (code, dreg, dreg, _tls_index * size, size); // dreg = Teb->TlsVector [_tls_index]
	amd64_mov_reg_membase (code, dreg, dreg, tls_offset, size);        // dreg = *(Teb->TlsVector [_tls_index] + tls_offset)

	return code;
}

#endif
