
#if defined(HAVE_EPOLL)

#include <sys/epoll.h>

#if defined(HOST_WIN32)
/* We assume that epoll is not available on windows */
#error
#endif

#define EPOLL_NEVENTS 128

typedef struct {
	gint fd;
} ThreadPoolIOBackendEpoll;

static gpointer
epoll_init (gpointer wakeup_pipe_handle)
{
	ThreadPoolIOBackendEpoll *backend_epoll;
	gint wakeup_pipe_fd;
	struct epoll_event event;

	backend_epoll = g_new0 (ThreadPoolIOBackendEpoll, 1);
	g_assert (backend_epoll);

	mono_trace (G_LOG_LEVEL_INFO, MONO_TRACE_IO_THREADPOOL,
		"[%p][epoll] init", backend_epoll);

#ifdef EPOOL_CLOEXEC
	backend_epoll->fd = epoll_create1 (EPOLL_CLOEXEC);
#else
	backend_epoll->fd = epoll_create (256);
	fcntl (backend_epoll->fd, F_SETFD, FD_CLOEXEC);
#endif

	if (backend_epoll->fd == -1) {
#ifdef EPOOL_CLOEXEC
		g_error ("[%p][epoll] init: epoll_create1 () failed, error (%d) %s\n", backend_epoll, errno, g_strerror (errno));
#else
		g_error ("[%p][epoll] init: epoll_create () failed, error (%d) %s\n", backend_epoll, errno, g_strerror (errno));
#endif
	}

	wakeup_pipe_fd = GPOINTER_TO_INT (wakeup_pipe_handle);
	g_assert (wakeup_pipe_fd >= 0);

	event.events = EPOLLIN;
	event.data.fd = wakeup_pipe_fd;
	if (epoll_ctl (backend_epoll->fd, EPOLL_CTL_ADD, event.data.fd, &event) == -1)
		g_error ("[%p][epoll] init: epoll_ctl () failed, error (%d) %s", backend_epoll, errno, g_strerror (errno));

	return backend_epoll;
}

static void
epoll_cleanup (gpointer backend)
{
	ThreadPoolIOBackendEpoll *backend_epoll;

	backend_epoll = backend;
	g_assert (backend_epoll);

	close (backend_epoll->fd);
	g_free (backend_epoll);
}

static void
epoll_add_handle (gpointer backend, gpointer handle, gint16 operations, gboolean is_new)
{
	ThreadPoolIOBackendEpoll *backend_epoll;
	gint fd;
	struct epoll_event event;

	backend_epoll = backend;
	g_assert (backend_epoll);

	fd = GPOINTER_TO_INT (handle);
	g_assert (fd >= 0);

	event.data.fd = fd;
	event.events = 0;
	if (operations & OPERATION_READ)
		event.events |= EPOLLIN;
	if (operations & OPERATION_WRITE)
		event.events |= EPOLLOUT;

	if (epoll_ctl (backend_epoll->fd, is_new ? EPOLL_CTL_ADD : EPOLL_CTL_MOD, event.data.fd, &event) == -1)
		g_error ("[%p][epoll] add: epoll_ctl(%s,%d) failed, error (%d) %s", backend_epoll, is_new ? "add" : "mod", fd, errno, g_strerror (errno));
}

static void
epoll_remove_handle (gpointer backend, gpointer handle)
{
	ThreadPoolIOBackendEpoll *backend_epoll;
	gint fd;

	backend_epoll = backend;
	g_assert (backend_epoll);

	fd = GPOINTER_TO_INT (handle);
	g_assert (fd >= 0);

	if (epoll_ctl (backend_epoll->fd, EPOLL_CTL_DEL, fd, NULL) == -1)
		g_error ("[%p][epoll] remove: epoll_ctl(del,%d) failed, error (%d) %s", backend_epoll, fd, errno, g_strerror (errno));
}

static void
epoll_poll (gpointer backend, ThreadPoolIOBackendEvent *events, gint nevents)
{
	ThreadPoolIOBackendEpoll *backend_epoll;
	gint i, ready;
	struct epoll_event epoll_events [EPOLL_NEVENTS];

	backend_epoll = backend;
	g_assert (backend_epoll);

	g_assert (events);
	g_assert (nevents > 0);

	memset (epoll_events, 0, sizeof (epoll_events));

	ready = epoll_wait (backend_epoll->fd, epoll_events, MIN (EPOLL_NEVENTS, nevents), -1);

	if (ready == -1) {
		switch (errno) {
		case EINTR:
			mono_thread_internal_check_for_interruption_critical (mono_thread_internal_current ());
			ready = 0;
			break;
		default:
			g_error ("[%p][epoll] poll: epoll_wait () failed, error (%d) %s", backend_epoll, errno, g_strerror (errno));
		}
	}

	if (ready == 0)
		return;

	g_assert (ready > 0);

	for (i = 0; i < MIN (ready, nevents); ++i) {
		struct epoll_event e = epoll_events [i];

		events [i].handle = GINT_TO_POINTER (e.data.fd);
		if (e.events & (EPOLLERR | EPOLLHUP)) {
			events [i].events = EVENT_IN | EVENT_OUT | EVENT_ERROR;
		} else {
			events [i].events = 0;
			if (e.events & EPOLLIN)
				events [i].events |= EVENT_IN;
			if (e.events & EPOLLOUT)
				events [i].events |= EVENT_OUT;
		}
	}
}

static ThreadPoolIOBackend backend_epoll = {
	.init = epoll_init,
	.cleanup = epoll_cleanup,
	.add_handle = epoll_add_handle,
	.remove_handle = epoll_remove_handle,
	.poll = epoll_poll,
};

#endif
