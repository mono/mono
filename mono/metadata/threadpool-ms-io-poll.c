
#define POLL_NEVENTS 1024

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
	guint i;

	poll_fds_size = 1;
	poll_fds_capacity = POLL_NEVENTS;
	poll_fds = g_new0 (mono_pollfd, poll_fds_capacity);

	POLL_INIT_FD (poll_fds, wakeup_pipe_fd, MONO_POLLIN);
	for (i = 1; i < poll_fds_capacity; ++i)
		POLL_INIT_FD (poll_fds + i, -1, 0);

	return TRUE;
}

static void
poll_cleanup (void)
{
	g_free (poll_fds);
}

static inline gint
poll_mark_bad_fds (mono_pollfd *poll_fds, gint poll_fds_size)
{
	gint i;
	gint ret;
	gint ready = 0;
	mono_pollfd *poll_fd;

	for (i = 0; i < poll_fds_size; i++) {
		poll_fd = poll_fds + i;
		if (poll_fd->fd == -1)
			continue;

		ret = mono_poll (poll_fd, 1, 0);
		if (ret == 1)
			ready++;
		if (ret == -1) {
#if !defined(HOST_WIN32)
			if (errno == EBADF)
#else
			if (WSAGetLastError () == WSAEBADF)
#endif
			{
				poll_fd->revents |= MONO_POLLNVAL;
				ready++;
			}
		}
	}

	return ready;
}

static void
poll_update_add (ThreadPoolIOUpdate *update)
{
	gboolean found = FALSE;
	gint j, k;

	for (j = 1; j < poll_fds_size; ++j) {
		mono_pollfd *poll_fd = poll_fds + j;
		if (poll_fd->fd == update->fd) {
			found = TRUE;
			break;
		}
	}

	if (!found) {
		for (j = 1; j < poll_fds_capacity; ++j) {
			mono_pollfd *poll_fd = poll_fds + j;
			if (poll_fd->fd == -1)
				break;
		}
	}

	if (j == poll_fds_capacity) {
		poll_fds_capacity += POLL_NEVENTS;
		poll_fds = g_renew (mono_pollfd, poll_fds, poll_fds_capacity);
		for (k = j; k < poll_fds_capacity; ++k)
			POLL_INIT_FD (poll_fds + k, -1, 0);
	}

	POLL_INIT_FD (poll_fds + j, update->fd, update->events);

	if (j >= poll_fds_size)
		poll_fds_size = j + 1;
}

static gint
poll_event_wait (void)
{
	gint ready;

	ready = mono_poll (poll_fds, poll_fds_size, -1);
	if (ready == -1) {
		/*
		 * Apart from EINTR, we only check EBADF, for the rest:
		 *  EINVAL: mono_poll() 'protects' us from descriptor
		 *      numbers above the limit if using select() by marking
		 *      then as MONO_POLLERR.  If a system poll() is being
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
			check_for_interruption_critical ();
			ready = 0;
			break;
#if !defined(HOST_WIN32)
		case EBADF:
#else
		case WSAEBADF:
#endif
			ready = poll_mark_bad_fds (poll_fds, poll_fds_size);
			break;
		default:
#if !defined(HOST_WIN32)
			g_warning ("poll_event_wait: mono_poll () failed, error (%d) %s", errno, g_strerror (errno));
#else
			g_warning ("poll_event_wait: mono_poll () failed, error (%d)\n", WSAGetLastError ());
#endif
			break;
		}
	}

	return ready;
}

static inline gint
poll_event_fd_at (guint i)
{
	return poll_fds [i].fd;
}

static gint
poll_event_max (void)
{
	return poll_fds_size;
}

static gboolean
poll_event_create_sockares_at (guint i, gint fd, MonoMList **list)
{
	mono_pollfd *poll_fd;

	g_assert (list);

	poll_fd = &poll_fds [i];
	g_assert (poll_fd);

	g_assert (fd == poll_fd->fd);

	if (fd == -1 || poll_fd->revents == 0)
		return FALSE;

	if (*list && (poll_fd->revents & (MONO_POLLIN | MONO_POLLERR | MONO_POLLHUP | MONO_POLLNVAL)) != 0) {
		MonoSocketAsyncResult *io_event = get_sockares_for_event (list, MONO_POLLIN);
		if (io_event)
			mono_threadpool_ms_enqueue_work_item (((MonoObject*) io_event)->vtable->domain, (MonoObject*) io_event);
	}
	if (*list && (poll_fd->revents & (MONO_POLLOUT | MONO_POLLERR | MONO_POLLHUP | MONO_POLLNVAL)) != 0) {
		MonoSocketAsyncResult *io_event = get_sockares_for_event (list, MONO_POLLOUT);
		if (io_event)
			mono_threadpool_ms_enqueue_work_item (((MonoObject*) io_event)->vtable->domain, (MonoObject*) io_event);
	}

	if (*list)
		poll_fd->events = get_events (*list);
	else
		POLL_INIT_FD (poll_fd, -1, 0);

	return TRUE;
}

static ThreadPoolIOBackend backend_poll = {
	.init = poll_init,
	.cleanup = poll_cleanup,
	.update_add = poll_update_add,
	.event_wait = poll_event_wait,
	.event_max = poll_event_max,
	.event_fd_at = poll_event_fd_at,
	.event_create_sockares_at = poll_event_create_sockares_at,
};
