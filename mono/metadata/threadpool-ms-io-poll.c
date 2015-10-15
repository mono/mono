
#include "utils/mono-poll.h"

typedef struct {
	mono_pollfd *fds;
	gint fds_capacity;
	gint fds_size;
} ThreadPoolIOBackendPoll;

static inline void
POLL_INIT_FD (mono_pollfd *poll_fd, gint fd, gint events)
{
	poll_fd->fd = fd;
	poll_fd->events = events;
	poll_fd->revents = 0;
}

static gpointer
poll_init (gpointer wakeup_pipe_handle)
{
	ThreadPoolIOBackendPoll *backend_poll;
	gint wakeup_pipe_fd;

	backend_poll = g_new0 (ThreadPoolIOBackendPoll, 1);
	g_assert (backend_poll);

	mono_trace (G_LOG_LEVEL_INFO, MONO_TRACE_IO_THREADPOOL,
		"[%p][poll] init", backend_poll);

	backend_poll->fds_size = 1;
	backend_poll->fds_capacity = 64;

	backend_poll->fds = g_new0 (mono_pollfd, backend_poll->fds_capacity);
	g_assert (backend_poll->fds);

	wakeup_pipe_fd = GPOINTER_TO_INT (wakeup_pipe_handle);
	g_assert (wakeup_pipe_fd >= 0);

	POLL_INIT_FD (&backend_poll->fds [0], wakeup_pipe_fd, MONO_POLLIN);

	return backend_poll;
}

static void
poll_cleanup (gpointer backend)
{
	ThreadPoolIOBackendPoll *backend_poll;

	backend_poll = (ThreadPoolIOBackendPoll*) backend;
	g_assert (backend_poll);

	g_free (backend_poll->fds);
	g_free (backend_poll);
}

static void
poll_add_handle (gpointer backend, gpointer handle, gint16 operations, gboolean is_new)
{
	ThreadPoolIOBackendPoll *backend_poll;
	gint fd;
	gint i;
	gint poll_event;

	backend_poll = (ThreadPoolIOBackendPoll*) backend;
	g_assert (backend_poll);

	fd = GPOINTER_TO_INT (handle);
	g_assert (fd >= 0);

	g_assert ((operations & ~(OPERATION_READ | OPERATION_WRITE)) == 0);

	g_assert (backend_poll->fds_size <= backend_poll->fds_capacity);

	poll_event = 0;
	if (operations & OPERATION_READ)
		poll_event |= MONO_POLLIN;
	if (operations & OPERATION_WRITE)
		poll_event |= MONO_POLLOUT;

	for (i = 0; i < backend_poll->fds_size; ++i) {
		if (backend_poll->fds [i].fd == fd) {
			if (is_new)
				g_error ("[%p][poll] add: handle %d already present", backend_poll, fd);
			POLL_INIT_FD (&backend_poll->fds [i], fd, poll_event);
			return;
		}
	}

	if (!is_new)
		g_error ("[%p][poll] add: could not find existing handle %d", backend_poll, fd);

	for (i = 0; i < backend_poll->fds_size; ++i) {
		if (backend_poll->fds [i].fd == -1) {
			POLL_INIT_FD (&backend_poll->fds [i], fd, poll_event);
			return;
		}
	}

	backend_poll->fds_size += 1;

	if (backend_poll->fds_size > backend_poll->fds_capacity) {
		backend_poll->fds_capacity *= 2;
		g_assert (backend_poll->fds_size <= backend_poll->fds_capacity);

		backend_poll->fds = g_renew (mono_pollfd, backend_poll->fds, backend_poll->fds_capacity);
	}

	POLL_INIT_FD (&backend_poll->fds [backend_poll->fds_size - 1], fd, poll_event);
}

static void
poll_remove_handle (gpointer backend, gpointer handle)
{
	ThreadPoolIOBackendPoll *backend_poll;
	gint fd;
	gint i;

	backend_poll = (ThreadPoolIOBackendPoll*) backend;
	g_assert (backend_poll);

	fd = GPOINTER_TO_INT (handle);
	g_assert (fd >= 0);

	for (i = 0; i < backend_poll->fds_size; ++i) {
		if (backend_poll->fds [i].fd == fd) {
			POLL_INIT_FD (&backend_poll->fds [i], -1, 0);
			break;
		}
	}

	/* if we don't find the fd in poll_fds,
	 * it means we try to delete it twice */
	if (i == backend_poll->fds_size)
		g_error ("[%p][poll] remove: trying to remove handle %d twice", backend_poll, fd);

	/* if we find it again, it means we added
	 * it twice */
	for (; i < backend_poll->fds_size; ++i) {
		if (backend_poll->fds [i].fd == fd)
			g_error ("[%p][poll] remove: handle %d is present twice", backend_poll, fd);
	}

	/* reduce the value of poll_fds_size so we
	 * do not keep it too big */
	while (backend_poll->fds_size > 1 && backend_poll->fds [backend_poll->fds_size - 1].fd == -1)
		backend_poll->fds_size -= 1;
}

static inline gint
poll_mark_bad_fds (mono_pollfd *poll_fds, gint poll_fds_size)
{
	gint i, ready = 0;

	for (i = 0; i < poll_fds_size; i++) {
		if (poll_fds [i].fd == -1)
			continue;

		switch (mono_poll (&poll_fds [i], 1, 0)) {
		case 1:
			ready++;
			break;
		case -1:
#if !defined(HOST_WIN32)
			if (errno == EBADF)
#else
			if (WSAGetLastError () == WSAEBADF)
#endif
			{
				poll_fds [i].revents |= MONO_POLLNVAL;
				ready++;
			}
			break;
		}
	}

	return ready;
}

static void
poll_poll (gpointer backend, ThreadPoolIOBackendEvent *events, gint nevents)
{
	ThreadPoolIOBackendPoll *backend_poll;
	gint i, j, ready;

	backend_poll = (ThreadPoolIOBackendPoll*) backend;
	g_assert (backend_poll);

	g_assert (events);
	g_assert (nevents > 0);

	for (i = 0; i < backend_poll->fds_size; ++i)
		backend_poll->fds [i].revents = 0;

	ready = mono_poll (backend_poll->fds, backend_poll->fds_size, -1);

	if (ready == -1) {
		/*
		 * Apart from EINTR, we only check EBADF, for the rest:
		 *  EINVAL: mono_poll() 'protects' us from descriptor
		 *      numbers above the limit if using select() by marking
		 *      then as POLLERR.  If a system poll() is being
		 *      used, the number of descriptor we're passing will not
		 *      be over sysconf(_SC_OPEN_MAX), as the error would have
		 *      happened when opening.
		 *
		 *  EFAULT: we own the memory pointed by pfds.
		 *  ENOMEM: we're doomed anyway
		 *
		 */
#if !defined(HOST_WIN32)
		switch (errno)
#else
		switch (WSAGetLastError ())
#endif
		{
#if !defined(HOST_WIN32)
		case EINTR:
#else
		case WSAEINTR:
#endif
		{
			mono_thread_internal_check_for_interruption_critical (mono_thread_internal_current ());
			ready = 0;
			break;
		}
#if !defined(HOST_WIN32)
		case EBADF:
#else
		case WSAEBADF:
#endif
		{
			ready = poll_mark_bad_fds (backend_poll->fds, backend_poll->fds_size);
			break;
		}
		default:
#if !defined(HOST_WIN32)
			g_error ("[%p][poll] poll: mono_poll () failed, error (%d) %s", backend_poll, errno, g_strerror (errno));
#else
			g_error ("[%p][poll] poll: mono_poll () failed, error (%d)\n", backend_poll, WSAGetLastError ());
#endif
			break;
		}
	}

	if (ready == 0)
		return;

	g_assert (ready > 0);

	for (i = 0, j = 0; i < backend_poll->fds_size; ++i) {
		mono_pollfd e = backend_poll->fds [i];

		if (e.fd == -1)
			continue;
		if (e.revents == 0)
			continue;

		events [j].handle = GINT_TO_POINTER (e.fd);
		if (e.revents & (MONO_POLLERR | MONO_POLLHUP | MONO_POLLNVAL)) {
			events [j].events = EVENT_IN | EVENT_OUT | EVENT_ERROR;
		} else {
			events [j].events = 0;
			if (e.revents & MONO_POLLIN)
				events [j].events |= EVENT_IN;
			if (e.revents & MONO_POLLOUT)
				events [j].events |= EVENT_OUT;
		}

		if (++j == nevents)
			break;
		if (--ready == 0)
			break;
	}
}

static ThreadPoolIOBackend backend_poll = {
	.init = poll_init,
	.cleanup = poll_cleanup,
	.add_handle = poll_add_handle,
	.remove_handle = poll_remove_handle,
	.poll = poll_poll,
};
