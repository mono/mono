/*
 * threadpool-ms-io.c: Microsoft IO threadpool runtime support
 *
 * Author:
 *	Ludovic Henry (ludovic.henry@xamarin.com)
 *
 * Copyright 2015 Xamarin, Inc (http://www.xamarin.com)
 */

#include <config.h>

#ifndef DISABLE_SOCKETS

#include <glib.h>

#if defined(HOST_WIN32)
#include <windows.h>
#else
#include <errno.h>
#include <fcntl.h>
#endif

#include <mono/metadata/threadpool-ms-io.h>
#include <mono/utils/mono-lazy-init.h>
#include <mono/utils/mono-logger-internals.h>

/* Keep in sync with System.IOSelector:BackendEvent in mcs/class/System/System/IOSelector.cs */
struct _ThreadPoolIOBackendEvent {
	gpointer handle;
	gint16 events;
};

/* Keep in sync with System.IOSelector:BackendEventType in mcs/class/System/System/IOSelector.cs */
enum ThreadPoolIOBackendEventType {
	EVENT_IN     = 1 << 0,
	EVENT_OUT    = 1 << 1,
	EVENT_ERROR  = 1 << 2,
};

/* Keep in sync with System.IOOperation in mcs/class/System/System/IOSelector.cs */
enum ThreadPoolIOOperation {
	OPERATION_READ  = 1 << 0,
	OPERATION_WRITE = 1 << 1,
};

typedef struct {
	gpointer (*init)          (gpointer wakeup_pipe_handle);
	void     (*cleanup)       (gpointer backend);
	void     (*add_handle)    (gpointer backend, gpointer handle, gint16 operations, gboolean is_new);
	void     (*remove_handle) (gpointer backend, gpointer handle);
	void     (*poll)          (gpointer backend, ThreadPoolIOBackendEvent *events, gint nevents);
} ThreadPoolIOBackend;

#include "threadpool-ms-io-epoll.c"
#include "threadpool-ms-io-kqueue.c"
#include "threadpool-ms-io-poll.c"

static mono_lazy_init_t io_status = MONO_LAZY_INIT_STATUS_NOT_INITIALIZED;

static ThreadPoolIOBackend threadpool_io_backend;

static void
initialize (void)
{
	threadpool_io_backend = backend_poll;
	if (g_getenv ("MONO_ENABLE_AIO") != NULL) {
#if defined(HAVE_KQUEUE)
		threadpool_io_backend = backend_kqueue;
#elif defined(HAVE_EPOLL)
		threadpool_io_backend = backend_epoll;
#endif
	}
}

gpointer
ves_icall_System_IOSelector_BackendInitialize (gpointer wakeup_pipe_handle)
{
	mono_lazy_initialize (&io_status, initialize);

	return threadpool_io_backend.init (wakeup_pipe_handle);
}

void
ves_icall_System_IOSelector_BackendCleanup (gpointer backend)
{
	threadpool_io_backend.cleanup (backend);
}

void
ves_icall_System_IOSelector_BackendAddHandle (gpointer backend, gpointer handle, gint32 operations, MonoBoolean is_new)
{
	mono_trace (G_LOG_LEVEL_INFO, MONO_TRACE_IO_THREADPOOL,
		"[%p] add: handle %d operations %s | %s is_new %s", backend, GPOINTER_TO_INT (handle),
		(operations & OPERATION_READ) ? "RD" : "..", (operations & OPERATION_WRITE) ? "WR" : "..", is_new ? "T" : "F");

	threadpool_io_backend.add_handle (backend, handle, operations, is_new);
}

void
ves_icall_System_IOSelector_BackendRemoveHandle (gpointer backend, gpointer handle)
{
	mono_trace (G_LOG_LEVEL_INFO, MONO_TRACE_IO_THREADPOOL,
		"[%p] remove: handle %d", backend, GPOINTER_TO_INT (handle));

	threadpool_io_backend.remove_handle (backend, handle);
}

void
ves_icall_System_IOSelector_BackendPoll (gpointer backend, MonoArray *events_array)
{
	ThreadPoolIOBackendEvent *events;
	gint nevents;

	mono_trace (G_LOG_LEVEL_INFO, MONO_TRACE_IO_THREADPOOL,
		"[%p] poll", backend);

	nevents = mono_array_length (events_array);
	g_assert (nevents > 0);

	events = mono_array_addr (events_array, ThreadPoolIOBackendEvent, 0);
	g_assert (events);

	threadpool_io_backend.poll (backend, events, nevents);
}

#else

gpointer
ves_icall_System_IOSelector_BackendInitialize (gpointer wakeup_pipe_handle)
{
	g_assert_not_reached ();
}

void
ves_icall_System_IOSelector_BackendCleanup (gpointer backend)
{
	g_assert_not_reached ();
}

void
ves_icall_System_IOSelector_BackendAddHandle (gpointer backend, gpointer handle, gint16 operations, MonoBoolean is_new)
{
	g_assert_not_reached ();
}

void
ves_icall_System_IOSelector_BackendRemoveHandle (gpointer backend, gpointer handle)
{
	g_assert_not_reached ();
}

void
ves_icall_System_IOSelector_BackendPoll (gpointer backend, MonoArray *events)
{
	g_assert_not_reached ();
}

#endif
