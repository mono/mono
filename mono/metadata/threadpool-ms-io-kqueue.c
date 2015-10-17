
#if defined(HAVE_KQUEUE)

#include <sys/types.h>
#include <sys/event.h>
#include <sys/time.h>

#if defined(HOST_WIN32)
/* We assume that kqueue is not available on windows */
#error
#endif

#define KQUEUE_NEVENTS 128

typedef struct {
	gint fd;
} ThreadPoolIOBackendKqueue;

static gpointer
kqueue_init (gpointer wakeup_pipe_handle)
{
	ThreadPoolIOBackendKqueue *backend_kqueue;
	gint wakeup_pipe_fd;
	struct kevent event;

	backend_kqueue = g_new0 (ThreadPoolIOBackendKqueue, 1);
	g_assert (backend_kqueue);

	mono_trace (G_LOG_LEVEL_INFO, MONO_TRACE_IO_THREADPOOL,
		"[%p][kqueue] init", backend_kqueue);

	backend_kqueue->fd = kqueue ();
	if (backend_kqueue->fd == -1)
		g_error ("[%p][kqueue] init: kqueue () failed, error (%d) %s", backend_kqueue, errno, g_strerror (errno));

	wakeup_pipe_fd = GPOINTER_TO_INT (wakeup_pipe_handle);
	g_assert (wakeup_pipe_fd >= 0);

	EV_SET (&event, wakeup_pipe_fd, EVFILT_READ, EV_ADD | EV_ENABLE, 0, 0, 0);
	if (kevent (backend_kqueue->fd, &event, 1, NULL, 0, NULL) == -1)
		g_error ("[%p][kqueue] init: kevent () failed, error (%d) %s", backend_kqueue, errno, g_strerror (errno));

	return backend_kqueue;
}

static void
kqueue_cleanup (gpointer backend)
{
	ThreadPoolIOBackendKqueue *backend_kqueue;

	backend_kqueue = backend;
	g_assert (backend_kqueue);

	close (backend_kqueue->fd);
	g_free (backend_kqueue);
}

static void
kqueue_add_handle (gpointer backend, gpointer handle, gint16 operations, gboolean is_new)
{
	ThreadPoolIOBackendKqueue *backend_kqueue;
	gint fd;
	struct kevent event;

	backend_kqueue = backend;
	g_assert (backend_kqueue);

	fd = GPOINTER_TO_INT (handle);
	g_assert (fd >= 0);

	if (operations & OPERATION_READ) {
		EV_SET (&event, fd, EVFILT_READ, EV_ADD | EV_ENABLE, 0, 0, 0);
		if (kevent (backend_kqueue->fd, &event, 1, NULL, 0, NULL) == -1)
			g_error ("[%p][kqueue] add: kevent(%d,read,enable) failed, error (%d) %s", backend_kqueue, fd, errno, g_strerror (errno));
	} else {
		if (!is_new) {
			EV_SET (&event, fd, EVFILT_READ, EV_ADD | EV_DISABLE, 0, 0, 0);
			if (kevent (backend_kqueue->fd, &event, 1, NULL, 0, NULL) == -1)
				g_error ("[%p][kqueue] add: kevent(%d,read,disable) failed, error (%d) %s", backend_kqueue, fd, errno, g_strerror (errno));
		}
	}

	if (operations & OPERATION_WRITE) {
		EV_SET (&event, fd, EVFILT_WRITE, EV_ADD | EV_ENABLE, 0, 0, 0);
		if (kevent (backend_kqueue->fd, &event, 1, NULL, 0, NULL) == -1)
			g_error ("[%p][kqueue] add: kevent(%d,write,enable) failed, error (%d) %s", backend_kqueue, fd, errno, g_strerror (errno));
	} else {
		if (!is_new) {
			EV_SET (&event, fd, EVFILT_WRITE, EV_ADD | EV_DISABLE, 0, 0, 0);
			if (kevent (backend_kqueue->fd, &event, 1, NULL, 0, NULL) == -1) {
				switch (errno) {
				case EPIPE:
					mono_trace (G_LOG_LEVEL_CRITICAL, MONO_TRACE_IO_THREADPOOL,
						"[%p][kqueue] add: kevent(%d,write,disable) failed, error (%d) %s", backend_kqueue, fd, errno, g_strerror (errno));
					break;
				default:
					g_error ("[%p][kqueue] add: kevent(%d,write,disable) failed, error (%d) %s", backend_kqueue, fd, errno, g_strerror (errno));
				}
			}
		}
	}
}

static void
kqueue_remove_handle (gpointer backend, gpointer handle)
{
	ThreadPoolIOBackendKqueue *backend_kqueue;
	gint fd;
	struct kevent event;

	backend_kqueue = backend;
	g_assert (backend_kqueue);

	fd = GPOINTER_TO_INT (handle);
	g_assert (fd >= 0);

	EV_SET (&event, fd, EVFILT_READ, EV_DELETE, 0, 0, 0);
	if (kevent (backend_kqueue->fd, &event, 1, NULL, 0, NULL) == -1) {
		switch (errno) {
		case ENOENT:
			mono_trace (G_LOG_LEVEL_CRITICAL, MONO_TRACE_IO_THREADPOOL,
				"[%p][kqueue] remove: kevent(%d,read,delete) failed, error (%d) %s", backend_kqueue, fd, errno, g_strerror (errno));
			break;
		default:
			g_error ("[%p][kqueue] remove: kevent(%d,read,delete) failed, error (%d) %s", backend_kqueue, fd, errno, g_strerror (errno));
		}
	}

	EV_SET (&event, fd, EVFILT_WRITE, EV_DELETE, 0, 0, 0);
	if (kevent (backend_kqueue->fd, &event, 1, NULL, 0, NULL) == -1) {
		switch (errno) {
		case ENOENT:
			mono_trace (G_LOG_LEVEL_CRITICAL, MONO_TRACE_IO_THREADPOOL,
				"[%p][kqueue] remove: kevent(%d,write,delete) failed, error (%d) %s", backend_kqueue, fd, errno, g_strerror (errno));
			break;
		default:
			g_error ("[%p][kqueue] remove: kevent(%d,write,delete) failed, error (%d) %s", backend_kqueue, fd, errno, g_strerror (errno));
		}
	}
}

static void
kqueue_poll (gpointer backend, ThreadPoolIOBackendEvent *events, gint nevents)
{
	ThreadPoolIOBackendKqueue *backend_kqueue;
	gint i, ready;
	struct kevent kqueue_events [KQUEUE_NEVENTS];

	backend_kqueue = backend;
	g_assert (backend_kqueue);

	g_assert (events);
	g_assert (nevents > 0);

	memset (kqueue_events, 0, sizeof (kqueue_events));

	ready = kevent (backend_kqueue->fd, NULL, 0, kqueue_events, MIN (KQUEUE_NEVENTS, nevents), NULL);

	if (ready == -1) {
		switch (errno) {
		case EINTR:
			mono_thread_internal_check_for_interruption_critical (mono_thread_internal_current ());
			ready = 0;
			break;
		default:
			g_error ("[%p][kqueue] poll: kevent () failed, error (%d) %s", backend_kqueue, errno, g_strerror (errno));
		}
	}

	if (ready == 0)
		return;

	g_assert (ready > 0);

	for (i = 0; i < MIN (ready, nevents); ++i) {
		struct kevent e = kqueue_events [i];

		events [i].handle = GINT_TO_POINTER (e.ident);
		if (e.flags & EV_ERROR) {
			events [i].events = EVENT_IN | EVENT_OUT | EVENT_ERROR;
		} else {
			events [i].events = 0;
			if (e.filter == EVFILT_READ)
				events [i].events |= EVENT_IN;
			if (e.filter == EVFILT_WRITE)
				events [i].events |= EVENT_OUT;
		}
	}
}

static ThreadPoolIOBackend backend_kqueue = {
	.init = kqueue_init,
	.cleanup = kqueue_cleanup,
	.add_handle = kqueue_add_handle,
	.remove_handle = kqueue_remove_handle,
	.poll = kqueue_poll,
};

#endif
