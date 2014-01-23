/*
 * threads.h:  Thread handles
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 */

#ifndef _WAPI_THREADS_H_
#define _WAPI_THREADS_H_

#include <glib.h>

#include <mono/io-layer/handles.h>
#include <mono/io-layer/io.h>
#include <mono/io-layer/status.h>
#include <mono/io-layer/processes.h>
#include <mono/io-layer/access.h>

G_BEGIN_DECLS

#define TLS_MINIMUM_AVAILABLE 64
#define TLS_OUT_OF_INDEXES 0xFFFFFFFF

#define STILL_ACTIVE STATUS_PENDING


#define THREAD_TERMINATE		0x0001
#define THREAD_SUSPEND_RESUME		0x0002
#define THREAD_GET_CONTEXT		0x0008
#define THREAD_SET_CONTEXT		0x0010
#define THREAD_SET_INFORMATION		0x0020
#define THREAD_QUERY_INFORMATION	0x0040
#define THREAD_SET_THREAD_TOKEN		0x0080
#define THREAD_IMPERSONATE		0x0100
#define THREAD_DIRECT_IMPERSONATION	0x0200
#define THREAD_ALL_ACCESS		(STANDARD_RIGHTS_REQUIRED|SYNCHRONIZE|0x3ff)

typedef guint32 (*WapiThreadStart)(gpointer);
typedef guint32 (*WapiApcProc)(gpointer);

extern gpointer OpenThread (guint32 access, gboolean inherit, gsize tid); /* NB tid is 32bit in MS API */
extern void ExitThread(guint32 exitcode) G_GNUC_NORETURN;
extern gboolean GetExitCodeThread(gpointer handle, guint32 *exitcode);
extern gsize GetCurrentThreadId(void); /* NB return is 32bit in MS API */
extern gpointer GetCurrentThread(void);
extern void Sleep(guint32 ms);
extern guint32 SleepEx(guint32 ms, gboolean alertable);

/* Kludge alert! Making this visible outside io-layer is broken, but I
 * can't find any w32 call that will let me do this.
 */
extern void _wapi_thread_signal_self (guint32 exitstatus);

void wapi_thread_interrupt_self (void);
void wapi_interrupt_thread (gpointer handle);
void wapi_clear_interruption (void);
gboolean wapi_thread_set_wait_handle (gpointer handle);
void wapi_thread_clear_wait_handle (gpointer handle);
void wapi_self_interrupt (void);

gpointer wapi_prepare_interrupt_thread (gpointer thread_handle);
void wapi_finish_interrupt_thread (gpointer wait_handle);


char* wapi_current_thread_desc (void);

gpointer wapi_create_thread_handle (void);
void wapi_thread_set_exit_code (guint32 exitstatus, gpointer handle);

G_END_DECLS
#endif /* _WAPI_THREADS_H_ */
