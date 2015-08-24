#include <mono/utils/mono-poll.h>

/* FIXME
Pass an array to poll with just the required FDs. This can be done by computing the array
on every call or by adjusting it on every call.

*/

static mono_pollfd *poll_fds;
static guint poll_fds_capacity;
static guint poll_fds_size;

static inline void
POLL_INIT_FD (mono_pollfd *poll_fd, gint fd, gint events)
{
	poll_fd->fd = fd;
	poll_fd->events = events;
	poll_fd->revents = 0;
}

static gboolean
poll_init (gint wakeup_pipe_fd)
{
	gint i;

	poll_fds_size = wakeup_pipe_fd + 1;
	poll_fds_capacity = 64;

	printf ("wakeup_fd %d\n", wakeup_pipe_fd);

	while (wakeup_pipe_fd >= poll_fds_capacity)
		poll_fds_capacity *= 4;

	poll_fds = g_new0 (mono_pollfd, poll_fds_capacity);

	for (i = 0; i < wakeup_pipe_fd; ++i)
		POLL_INIT_FD (&poll_fds [i], -1, 0);

	POLL_INIT_FD (&poll_fds [wakeup_pipe_fd], wakeup_pipe_fd, POLLIN);

	return TRUE;
}

static void
poll_cleanup (void)
{
	g_free (poll_fds);
}

static void
poll_register_fd (gint fd, gint events, gboolean is_new)
{
	gint i;
	mono_pollfd *poll_fd;

	g_assert (fd >= 0);
	g_assert (poll_fds_size <= poll_fds_capacity);
	printf ("register %d events %d is_new %d\n", fd, events, is_new);

	if (fd >= poll_fds_capacity) {
		do {
			poll_fds_capacity *= 4;
		} while (fd >= poll_fds_capacity);

		poll_fds = g_renew (mono_pollfd, poll_fds, poll_fds_capacity);
	}

	if (fd >= poll_fds_size) {
		for (i = poll_fds_size; i <= fd; ++i)
			POLL_INIT_FD (&poll_fds [i], -1, 0);

		poll_fds_size = fd + 1;
	}

	poll_fd = &poll_fds [fd];

	if (poll_fd->fd != -1) {
		g_assert (poll_fd->fd == fd);
		g_assert (!is_new);
	}

	POLL_INIT_FD (poll_fd, fd, ((events & EVENT_IN) ? POLLIN : 0) | ((events & EVENT_OUT) ? POLLOUT : 0));
}

static void
poll_remove_fd (gint fd)
{
	mono_pollfd *poll_fd;

	printf ("remove FD %d\n", fd);

	g_assert (fd >= 0);

	g_assert (fd < poll_fds_size);
	poll_fd = &poll_fds [fd];

	g_assert (poll_fd->fd == fd);
	POLL_INIT_FD (poll_fd, -1, 0);
}

static gint
poll_event_wait (void (*callback) (gint fd, gint events, gpointer user_data), gpointer user_data)
{
	gint i, ready;

	for (i = 0; i < poll_fds_size; ++i)
		poll_fds [i].revents = 0;

	mono_gc_set_skip_thread (TRUE);

	ready = mono_poll (poll_fds, poll_fds_size, -1);

	mono_gc_set_skip_thread (FALSE);

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
		switch (errno)
		{
		case EINTR:
		{
			mono_thread_internal_check_for_interruption_critical (mono_thread_internal_current ());
			ready = 0;
			break;
		}
		default:
			g_error ("poll_event_wait: mono_poll () failed, error (%d) %s", errno, g_strerror (errno));
			break;
		}
	}

	if (ready == -1)
		return -1;

	for (i = 0; i < poll_fds_size; ++i) {
		gint fd, events = 0;

		if (poll_fds [i].fd == -1)
			continue;
		if (poll_fds [i].revents == 0)
			continue;

		fd = poll_fds [i].fd;
		if (poll_fds [i].revents & (POLLIN | POLLERR | POLLHUP | POLLNVAL))
			events |= EVENT_IN;
		if (poll_fds [i].revents & (POLLOUT | POLLERR | POLLHUP | POLLNVAL))
			events |= EVENT_OUT;

		callback (fd, events, user_data);

		if (--ready == 0)
			break;
	}

	return 0;
}

static ThreadPoolIOBackend backend_poll = {
	.init = poll_init,
	.cleanup = poll_cleanup,
	.register_fd = poll_register_fd,
	.remove_fd = poll_remove_fd,
	.event_wait = poll_event_wait,
};
