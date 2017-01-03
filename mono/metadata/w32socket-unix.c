/*
 * w32socket-unix.c: Unix specific socket code.
 *
 * Copyright 2016 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include <config.h>
#include <glib.h>

#include <string.h>
#include <stdlib.h>
#include <sys/socket.h>
#ifdef HAVE_SYS_IOCTL_H
#include <sys/ioctl.h>
#endif
#include <netinet/in.h>
#include <netinet/tcp.h>
#ifdef HAVE_NETDB_H
#include <netdb.h>
#endif
#include <arpa/inet.h>
#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif
#include <errno.h>

#include <fcntl.h>

#include <sys/types.h>

#include "w32socket.h"
#include "w32socket-internals.h"
#include "w32handle.h"
#include "utils/mono-logger-internals.h"
#include "utils/mono-poll.h"

static guint32 in_cleanup = 0;

static void
socket_close (gpointer handle, gpointer data)
{
	int ret;
	MonoW32HandleSocket *socket_handle = (MonoW32HandleSocket *)data;
	MonoThreadInfo *info = mono_thread_info_current ();

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: closing socket handle %p", __func__, handle);

	/* Shutdown the socket for reading, to interrupt any potential
	 * receives that may be blocking for data.  See bug 75705. */
	shutdown (GPOINTER_TO_UINT (handle), SHUT_RD);

	do {
		ret = close (GPOINTER_TO_UINT(handle));
	} while (ret == -1 && errno == EINTR &&
		 !mono_thread_info_is_interrupt_state (info));

	if (ret == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: close error: %s", __func__, strerror (errno));
		errnum = errno_to_WSA (errnum, __func__);
		if (!in_cleanup)
			mono_w32socket_set_last_error (errnum);
	}

	if (!in_cleanup)
		socket_handle->saved_error = 0;
}

static void
socket_details (gpointer data)
{
	/* FIXME: do something */
}

static const gchar*
socket_typename (void)
{
	return "Socket";
}

static gsize
socket_typesize (void)
{
	return sizeof (MonoW32HandleSocket);
}

static MonoW32HandleOps ops = {
	socket_close,    /* close */
	NULL,            /* signal */
	NULL,            /* own */
	NULL,            /* is_owned */
	NULL,            /* special_wait */
	NULL,            /* prewait */
	socket_details,  /* details */
	socket_typename, /* typename */
	socket_typesize, /* typesize */
};

void
mono_w32socket_initialize (void)
{
	mono_w32handle_register_ops (MONO_W32HANDLE_SOCKET, &ops);
}

static gboolean
cleanup_close (gpointer handle, gpointer data, gpointer user_data)
{
	if (mono_w32handle_get_type (handle) == MONO_W32HANDLE_SOCKET)
		mono_w32handle_force_close (handle, data);

	return FALSE;
}

void
mono_w32socket_cleanup (void)
{
	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: cleaning up", __func__);

	in_cleanup = 1;
	mono_w32handle_foreach (cleanup_close, NULL);
	in_cleanup = 0;
}

static SOCKET
_wapi_accept(SOCKET sock, struct sockaddr *addr, socklen_t *addrlen)
{
	gpointer handle = GUINT_TO_POINTER (sock);
	gpointer new_handle;
	MonoW32HandleSocket *socket_handle;
	MonoW32HandleSocket new_socket_handle = {0};
	SOCKET new_fd;
	MonoThreadInfo *info = mono_thread_info_current ();

	if (addr != NULL && *addrlen < sizeof(struct sockaddr)) {
		mono_w32socket_set_last_error (WSAEFAULT);
		return INVALID_SOCKET;
	}

	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return INVALID_SOCKET;
	}

	if (!mono_w32handle_lookup (handle, MONO_W32HANDLE_SOCKET, (gpointer *)&socket_handle)) {
		g_warning ("%s: error looking up socket handle %p",
			   __func__, handle);
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return INVALID_SOCKET;
	}

	do {
		new_fd = accept (sock, addr, addrlen);
	} while (new_fd == -1 && errno == EINTR &&
		 !mono_thread_info_is_interrupt_state (info));

	if (new_fd == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: accept error: %s", __func__, strerror(errno));

		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return INVALID_SOCKET;
	}

	if (new_fd >= mono_w32handle_fd_reserve) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: File descriptor is too big", __func__);

		mono_w32socket_set_last_error (WSASYSCALLFAILURE);

		close (new_fd);

		return INVALID_SOCKET;
	}

	new_socket_handle.domain = socket_handle->domain;
	new_socket_handle.type = socket_handle->type;
	new_socket_handle.protocol = socket_handle->protocol;
	new_socket_handle.still_readable = 1;

	new_handle = mono_w32handle_new_fd (MONO_W32HANDLE_SOCKET, new_fd,
					  &new_socket_handle);
	if(new_handle == INVALID_HANDLE_VALUE) {
		g_warning ("%s: error creating socket handle", __func__);
		mono_w32socket_set_last_error (ERROR_GEN_FAILURE);
		return INVALID_SOCKET;
	}

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: returning newly accepted socket handle %p with",
		   __func__, new_handle);

	return new_fd;
}

SOCKET
mono_w32socket_accept (SOCKET sock, struct sockaddr *addr, socklen_t *addrlen, gboolean blocking)
{
	return _wapi_accept (sock, addr, addrlen);
}

static int
_wapi_connect(SOCKET sock, const struct sockaddr *serv_addr, socklen_t addrlen)
{
	gpointer handle = GUINT_TO_POINTER (sock);
	MonoW32HandleSocket *socket_handle;
	gint errnum;
	MonoThreadInfo *info = mono_thread_info_current ();

	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

	if (connect (sock, serv_addr, addrlen) == -1) {
		mono_pollfd fds;
		int so_error;
		socklen_t len;

		errnum = errno;

		if (errno != EINTR) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: connect error: %s", __func__,
				   strerror (errnum));

			errnum = errno_to_WSA (errnum, __func__);
			if (errnum == WSAEINPROGRESS)
				errnum = WSAEWOULDBLOCK; /* see bug #73053 */

			mono_w32socket_set_last_error (errnum);

			/*
			 * On solaris x86 getsockopt (SO_ERROR) is not set after
			 * connect () fails so we need to save this error.
			 *
			 * But don't do this for EWOULDBLOCK (bug 317315)
			 */
			if (errnum != WSAEWOULDBLOCK) {
				if (!mono_w32handle_lookup (handle, MONO_W32HANDLE_SOCKET, (gpointer *)&socket_handle)) {
					/* ECONNRESET means the socket was closed by another thread */
					/* Async close on mac raises ECONNABORTED. */
					if (errnum != WSAECONNRESET && errnum != WSAENETDOWN)
						g_warning ("%s: error looking up socket handle %p (error %d)", __func__, handle, errnum);
				} else {
					socket_handle->saved_error = errnum;
				}
			}
			return SOCKET_ERROR;
		}

		fds.fd = sock;
		fds.events = MONO_POLLOUT;
		while (mono_poll (&fds, 1, -1) == -1 &&
		       !mono_thread_info_is_interrupt_state (info)) {
			if (errno != EINTR) {
				errnum = errno_to_WSA (errno, __func__);

				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: connect poll error: %s",
					   __func__, strerror (errno));

				mono_w32socket_set_last_error (errnum);
				return SOCKET_ERROR;
			}
		}

		len = sizeof(so_error);
		if (getsockopt (sock, SOL_SOCKET, SO_ERROR, &so_error,
				&len) == -1) {
			errnum = errno_to_WSA (errno, __func__);

			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: connect getsockopt error: %s",
				   __func__, strerror (errno));

			mono_w32socket_set_last_error (errnum);
			return SOCKET_ERROR;
		}

		if (so_error != 0) {
			errnum = errno_to_WSA (so_error, __func__);

			/* Need to save this socket error */
			if (!mono_w32handle_lookup (handle, MONO_W32HANDLE_SOCKET, (gpointer *)&socket_handle)) {
				g_warning ("%s: error looking up socket handle %p", __func__, handle);
			} else {
				socket_handle->saved_error = errnum;
			}

			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: connect getsockopt returned error: %s",
				   __func__, strerror (so_error));

			mono_w32socket_set_last_error (errnum);
			return SOCKET_ERROR;
		}
	}

	return 0;
}

int
mono_w32socket_connect (SOCKET s, const struct sockaddr *name, int namelen, gboolean blocking)
{
	return _wapi_connect (s, name, namelen);
}

static int
_wapi_recvfrom(SOCKET sock, void *buf, size_t len, int recv_flags, struct sockaddr *from, socklen_t *fromlen);

static int
_wapi_recv (SOCKET sock, void *buf, size_t len, int recv_flags)
{
	return _wapi_recvfrom (sock, buf, len, recv_flags, NULL, 0);
}

int
mono_w32socket_recv (SOCKET s, char *buf, int len, int flags, gboolean blocking)
{
	return _wapi_recv (s, buf, len, flags);
}

static int
_wapi_recvfrom(SOCKET sock, void *buf, size_t len, int recv_flags, struct sockaddr *from, socklen_t *fromlen)
{
	gpointer handle = GUINT_TO_POINTER (sock);
	MonoW32HandleSocket *socket_handle;
	int ret;
	MonoThreadInfo *info = mono_thread_info_current ();

	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

	do {
		ret = recvfrom (sock, buf, len, recv_flags, from, fromlen);
	} while (ret == -1 && errno == EINTR &&
		 !mono_thread_info_is_interrupt_state (info));

	if (ret == 0 && len > 0) {
		/* According to the Linux man page, recvfrom only
		 * returns 0 when the socket has been shut down
		 * cleanly.  Turn this into an EINTR to simulate win32
		 * behaviour of returning EINTR when a socket is
		 * closed while the recvfrom is blocking (we use a
		 * shutdown() in socket_close() to trigger this.) See
		 * bug 75705.
		 */
		/* Distinguish between the socket being shut down at
		 * the local or remote ends, and reads that request 0
		 * bytes to be read
		 */

		/* If this returns FALSE, it means the socket has been
		 * closed locally.  If it returns TRUE, but
		 * still_readable != 1 then shutdown
		 * (SHUT_RD|SHUT_RDWR) has been called locally.
		 */
		if (!mono_w32handle_lookup (handle, MONO_W32HANDLE_SOCKET, (gpointer *)&socket_handle)
			 || socket_handle->still_readable != 1)
		{
			ret = -1;
			errno = EINTR;
		}
	}

	if (ret == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: recv error: %s", __func__, strerror(errno));

		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return SOCKET_ERROR;
	}
	return ret;
}

int
mono_w32socket_recvfrom (SOCKET s, char *buf, int len, int flags, struct sockaddr *from, socklen_t *fromlen, gboolean blocking)
{
	return _wapi_recvfrom (s, buf, len, flags, from, fromlen);
}

static void
wsabuf_to_msghdr (WSABUF *buffers, guint32 count, struct msghdr *hdr)
{
	guint32 i;

	memset (hdr, 0, sizeof (struct msghdr));
	hdr->msg_iovlen = count;
	hdr->msg_iov = g_new0 (struct iovec, count);
	for (i = 0; i < count; i++) {
		hdr->msg_iov [i].iov_base = buffers [i].buf;
		hdr->msg_iov [i].iov_len  = buffers [i].len;
	}
}

static void
msghdr_iov_free (struct msghdr *hdr)
{
	g_free (hdr->msg_iov);
}

static int
_wapi_recvmsg(SOCKET sock, struct msghdr *msg, int recv_flags)
{
	gpointer handle = GUINT_TO_POINTER (sock);
	MonoW32HandleSocket *socket_handle;
	int ret;
	MonoThreadInfo *info = mono_thread_info_current ();

	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

	do {
		ret = recvmsg (sock, msg, recv_flags);
	} while (ret == -1 && errno == EINTR &&
		 !mono_thread_info_is_interrupt_state (info));

	if (ret == 0) {
		/* see _wapi_recvfrom */
		if (!mono_w32handle_lookup (handle, MONO_W32HANDLE_SOCKET, (gpointer *)&socket_handle)
			 || socket_handle->still_readable != 1)
		{
			ret = -1;
			errno = EINTR;
		}
	}

	if (ret == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: recvmsg error: %s", __func__, strerror(errno));

		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return SOCKET_ERROR;
	}
	return ret;
}

static int
WSARecv (SOCKET sock, WSABUF *buffers, guint32 count, guint32 *received, guint32 *flags, gpointer overlapped, gpointer complete)
{
	int ret;
	struct msghdr hdr;

	g_assert (overlapped == NULL);
	g_assert (complete == NULL);

	wsabuf_to_msghdr (buffers, count, &hdr);
	ret = _wapi_recvmsg (sock, &hdr, *flags);
	msghdr_iov_free (&hdr);

	if(ret == SOCKET_ERROR) {
		return ret;
	}

	*received = ret;
	*flags = hdr.msg_flags;

	return 0;
}

int
mono_w32socket_recvbuffers (SOCKET s, WSABUF *lpBuffers, guint32 dwBufferCount, guint32 *lpNumberOfBytesRecvd, guint32 *lpFlags, gpointer lpOverlapped, gpointer lpCompletionRoutine, gboolean blocking)
{
	return WSARecv (s, lpBuffers, dwBufferCount,lpNumberOfBytesRecvd,lpFlags, lpOverlapped, lpCompletionRoutine);
}

static int
_wapi_send (SOCKET sock, const void *msg, size_t len, int send_flags)
{
	gpointer handle = GUINT_TO_POINTER (sock);
	int ret;
	MonoThreadInfo *info = mono_thread_info_current ();

	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

	do {
		ret = send (sock, msg, len, send_flags);
	} while (ret == -1 && errno == EINTR &&
		 !mono_thread_info_is_interrupt_state (info));

	if (ret == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: send error: %s", __func__, strerror (errno));

#ifdef O_NONBLOCK
		/* At least linux returns EAGAIN/EWOULDBLOCK when the timeout has been set on
		 * a blocking socket. See bug #599488 */
		if (errnum == EAGAIN) {
			ret = fcntl (sock, F_GETFL, 0);
			if (ret != -1 && (ret & O_NONBLOCK) == 0)
				errnum = ETIMEDOUT;
		}
#endif /* O_NONBLOCK */
		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return SOCKET_ERROR;
	}
	return ret;
}

int
mono_w32socket_send (SOCKET s, char *buf, int len, int flags, gboolean blocking)
{
	return _wapi_send (s, buf, len, flags);
}

static int
_wapi_sendto (SOCKET sock, const void *msg, size_t len, int send_flags, const struct sockaddr *to, socklen_t tolen)
{
	gpointer handle = GUINT_TO_POINTER (sock);
	int ret;
	MonoThreadInfo *info = mono_thread_info_current ();

	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

	do {
		ret = sendto (sock, msg, len, send_flags, to, tolen);
	} while (ret == -1 && errno == EINTR &&
		 !mono_thread_info_is_interrupt_state (info));

	if (ret == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: send error: %s", __func__, strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return SOCKET_ERROR;
	}
	return ret;
}

int
mono_w32socket_sendto (SOCKET s, const char *buf, int len, int flags, const struct sockaddr *to, int tolen, gboolean blocking)
{
	return _wapi_sendto (s, buf, len, flags, to, tolen);
}

static int
_wapi_sendmsg (SOCKET sock,  const struct msghdr *msg, int send_flags)
{
	gpointer handle = GUINT_TO_POINTER (sock);
	int ret;
	MonoThreadInfo *info = mono_thread_info_current ();

	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

	do {
		ret = sendmsg (sock, msg, send_flags);
	} while (ret == -1 && errno == EINTR &&
		 !mono_thread_info_is_interrupt_state (info));

	if (ret == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: sendmsg error: %s", __func__, strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return SOCKET_ERROR;
	}
	return ret;
}

static int
WSASend (SOCKET sock, WSABUF *buffers, guint32 count, guint32 *sent, guint32 flags, gpointer overlapped, gpointer complete)
{
	int ret;
	struct msghdr hdr;

	g_assert (overlapped == NULL);
	g_assert (complete == NULL);

	wsabuf_to_msghdr (buffers, count, &hdr);
	ret = _wapi_sendmsg (sock, &hdr, flags);
	msghdr_iov_free (&hdr);

	if(ret == SOCKET_ERROR)
		return ret;

	*sent = ret;
	return 0;
}

int
mono_w32socket_sendbuffers (SOCKET s, WSABUF *lpBuffers, guint32 dwBufferCount, guint32 *lpNumberOfBytesRecvd, guint32 lpFlags, gpointer lpOverlapped, gpointer lpCompletionRoutine, gboolean blocking)
{
	return WSASend (s, lpBuffers, dwBufferCount, lpNumberOfBytesRecvd, lpFlags, lpOverlapped, lpCompletionRoutine);
}

#define SF_BUFFER_SIZE	16384
static gint
wapi_sendfile (SOCKET sock, gpointer file_handle, guint32 bytes_to_write, guint32 bytes_per_send, guint32 flags)
{
	MonoThreadInfo *info = mono_thread_info_current ();
#if defined(HAVE_SENDFILE) && (defined(__linux__) || defined(DARWIN))
	gint file = GPOINTER_TO_INT (file_handle);
	gint n;
	gint errnum;
	gssize res;
	struct stat statbuf;

	n = fstat (file, &statbuf);
	if (n == -1) {
		errnum = errno;
		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);
		return SOCKET_ERROR;
	}
	do {
#ifdef __linux__
		res = sendfile (sock, file, NULL, statbuf.st_size);
#elif defined(DARWIN)
		/* TODO: header/tail could be sent in the 5th argument */
		/* TODO: Might not send the entire file for non-blocking sockets */
		res = sendfile (file, sock, 0, &statbuf.st_size, NULL, 0);
#endif
	} while (res != -1 && errno == EINTR && !mono_thread_info_is_interrupt_state (info));
	if (res == -1) {
		errnum = errno;
		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);
		return SOCKET_ERROR;
	}
#else
	/* Default implementation */
	gint file = GPOINTER_TO_INT (file_handle);
	gchar *buffer;
	gint n;

	buffer = g_malloc (SF_BUFFER_SIZE);
	do {
		do {
			n = read (file, buffer, SF_BUFFER_SIZE);
		} while (n == -1 && errno == EINTR && !mono_thread_info_is_interrupt_state (info));
		if (n == -1)
			break;
		if (n == 0) {
			g_free (buffer);
			return 0; /* We're done reading */
		}
		do {
			n = send (sock, buffer, n, 0); /* short sends? enclose this in a loop? */
		} while (n == -1 && errno == EINTR && !mono_thread_info_is_interrupt_state (info));
	} while (n != -1 && errno == EINTR && !mono_thread_info_is_interrupt_state (info));

	if (n == -1) {
		gint errnum = errno;
		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);
		g_free (buffer);
		return SOCKET_ERROR;
	}
	g_free (buffer);
#endif
	return 0;
}

static gboolean
TransmitFile (SOCKET sock, gpointer file, guint32 bytes_to_write, guint32 bytes_per_send, OVERLAPPED *ol,
		TRANSMIT_FILE_BUFFERS *buffers, guint32 flags)
{
	gpointer handle = GUINT_TO_POINTER (sock);
	gint ret;

	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return FALSE;
	}

	/* Write the header */
	if (buffers != NULL && buffers->Head != NULL && buffers->HeadLength > 0) {
		ret = _wapi_send (sock, buffers->Head, buffers->HeadLength, 0);
		if (ret == SOCKET_ERROR)
			return FALSE;
	}

	ret = wapi_sendfile (sock, file, bytes_to_write, bytes_per_send, flags);
	if (ret == SOCKET_ERROR)
		return FALSE;

	/* Write the tail */
	if (buffers != NULL && buffers->Tail != NULL && buffers->TailLength > 0) {
		ret = _wapi_send (sock, buffers->Tail, buffers->TailLength, 0);
		if (ret == SOCKET_ERROR)
			return FALSE;
	}

	if ((flags & TF_DISCONNECT) == TF_DISCONNECT)
		CloseHandle (handle);

	return TRUE;
}

BOOL
mono_w32socket_transmit_file (SOCKET hSocket, gpointer hFile, guint32 nNumberOfBytesToWrite, guint32 nNumberOfBytesPerSend, OVERLAPPED *lpOverlapped, TRANSMIT_FILE_BUFFERS *lpTransmitBuffers, guint32 dwReserved, gboolean blocking)
{
	return TransmitFile (hSocket, hFile, nNumberOfBytesToWrite, nNumberOfBytesPerSend, lpOverlapped, lpTransmitBuffers, dwReserved);
}

SOCKET
mono_w32socket_socket (int domain, int type, int protocol)
{
	MonoW32HandleSocket socket_handle = {0};
	gpointer handle;
	SOCKET sock;

	socket_handle.domain = domain;
	socket_handle.type = type;
	socket_handle.protocol = protocol;
	socket_handle.still_readable = 1;

	sock = socket (domain, type, protocol);
	if (sock == -1 && domain == AF_INET && type == SOCK_RAW &&
	    protocol == 0) {
		/* Retry with protocol == 4 (see bug #54565) */
		// https://bugzilla.novell.com/show_bug.cgi?id=MONO54565
		socket_handle.protocol = 4;
		sock = socket (AF_INET, SOCK_RAW, 4);
	}

	if (sock == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: socket error: %s", __func__, strerror (errno));
		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return INVALID_SOCKET;
	}

	if (sock >= mono_w32handle_fd_reserve) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: File descriptor is too big (%d >= %d)",
			   __func__, sock, mono_w32handle_fd_reserve);

		mono_w32socket_set_last_error (WSASYSCALLFAILURE);
		close (sock);

		return INVALID_SOCKET;
	}

	/* .net seems to set this by default for SOCK_STREAM, not for
	 * SOCK_DGRAM (see bug #36322)
	 * https://bugzilla.novell.com/show_bug.cgi?id=MONO36322
	 *
	 * It seems winsock has a rather different idea of what
	 * SO_REUSEADDR means.  If it's set, then a new socket can be
	 * bound over an existing listening socket.  There's a new
	 * windows-specific option called SO_EXCLUSIVEADDRUSE but
	 * using that means the socket MUST be closed properly, or a
	 * denial of service can occur.  Luckily for us, winsock
	 * behaves as though any other system would when SO_REUSEADDR
	 * is true, so we don't need to do anything else here.  See
	 * bug 53992.
	 * https://bugzilla.novell.com/show_bug.cgi?id=MONO53992
	 */
	{
		int ret, true_ = 1;

		ret = setsockopt (sock, SOL_SOCKET, SO_REUSEADDR, &true_, sizeof (true_));
		if (ret == -1) {
			int errnum = errno;

			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Error setting SO_REUSEADDR", __func__);

			errnum = errno_to_WSA (errnum, __func__);
			mono_w32socket_set_last_error (errnum);

			close (sock);

			return INVALID_SOCKET;
		}
	}


	handle = mono_w32handle_new_fd (MONO_W32HANDLE_SOCKET, sock, &socket_handle);
	if (handle == INVALID_HANDLE_VALUE) {
		g_warning ("%s: error creating socket handle", __func__);
		mono_w32socket_set_last_error (WSASYSCALLFAILURE);
		close (sock);
		return INVALID_SOCKET;
	}

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: returning socket handle %p", __func__, handle);

	return sock;
}

gint
mono_w32socket_bind (SOCKET sock, struct sockaddr *addr, socklen_t addrlen)
{
	gpointer handle;
	int ret;

	handle = GUINT_TO_POINTER (sock);
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

	ret = bind (sock, addr, addrlen);
	if (ret == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: bind error: %s", __func__, strerror(errno));
		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return SOCKET_ERROR;
	}

	return 0;
}

gint
mono_w32socket_getpeername (SOCKET sock, struct sockaddr *name, socklen_t *namelen)
{
	gpointer handle;
	gint ret;

	handle = GUINT_TO_POINTER (sock);
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

	ret = getpeername (sock, name, namelen);
	if (ret == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: getpeername error: %s", __func__, strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return SOCKET_ERROR;
	}

	return 0;
}

gint
mono_w32socket_getsockname (SOCKET sock, struct sockaddr *name, socklen_t *namelen)
{
	gpointer handle;
	gint ret;

	handle = GUINT_TO_POINTER (sock);
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

	ret = getsockname (sock, name, namelen);
	if (ret == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: getsockname error: %s", __func__, strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return SOCKET_ERROR;
	}

	return 0;
}

gint
mono_w32socket_getsockopt (SOCKET sock, gint level, gint optname, gpointer optval, socklen_t *optlen)
{
	gpointer handle;
	gint ret;
	struct timeval tv;
	gpointer tmp_val;
	MonoW32HandleSocket *socket_handle;

	handle = GUINT_TO_POINTER (sock);
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

	tmp_val = optval;
	if (level == SOL_SOCKET &&
	    (optname == SO_RCVTIMEO || optname == SO_SNDTIMEO)) {
		tmp_val = &tv;
		*optlen = sizeof (tv);
	}

	ret = getsockopt (sock, level, optname, tmp_val, optlen);
	if (ret == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: getsockopt error: %s", __func__, strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return SOCKET_ERROR;
	}

	if (level == SOL_SOCKET && (optname == SO_RCVTIMEO || optname == SO_SNDTIMEO)) {
		*((int *) optval) = tv.tv_sec * 1000 + (tv.tv_usec / 1000);	// milli from micro
		*optlen = sizeof (int);
	}

	if (optname == SO_ERROR) {
		if (!mono_w32handle_lookup (handle, MONO_W32HANDLE_SOCKET, (gpointer *)&socket_handle)) {
			g_warning ("%s: error looking up socket handle %p", __func__, handle);

			/* can't extract the last error */
			*((int *) optval) = errno_to_WSA (*((int *)optval), __func__);
		} else {
			if (*((int *)optval) != 0) {
				*((int *) optval) = errno_to_WSA (*((int *)optval), __func__);
				socket_handle->saved_error = *((int *)optval);
			} else {
				*((int *)optval) = socket_handle->saved_error;
			}
		}
	}

	return 0;
}

gint
mono_w32socket_setsockopt (SOCKET sock, gint level, gint optname, const gpointer optval, socklen_t optlen)
{
	gpointer handle;
	gint ret;
	gpointer tmp_val;
#if defined (__linux__)
	/* This has its address taken so it cannot be moved to the if block which uses it */
	gint bufsize = 0;
#endif
	struct timeval tv;

	handle = GUINT_TO_POINTER (sock);
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

	tmp_val = optval;
	if (level == SOL_SOCKET &&
	    (optname == SO_RCVTIMEO || optname == SO_SNDTIMEO)) {
		int ms = *((int *) optval);
		tv.tv_sec = ms / 1000;
		tv.tv_usec = (ms % 1000) * 1000;	// micro from milli
		tmp_val = &tv;
		optlen = sizeof (tv);
	}
#if defined (__linux__)
	else if (level == SOL_SOCKET &&
		   (optname == SO_SNDBUF || optname == SO_RCVBUF)) {
		/* According to socket(7) the Linux kernel doubles the
		 * buffer sizes "to allow space for bookkeeping
		 * overhead."
		 */
		bufsize = *((int *) optval);

		bufsize /= 2;
		tmp_val = &bufsize;
	}
#endif

	ret = setsockopt (sock, level, optname, tmp_val, optlen);
	if (ret == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: setsockopt error: %s", __func__, strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return SOCKET_ERROR;
	}

#if defined (SO_REUSEPORT)
	/* BSD's and MacOS X multicast sockets also need SO_REUSEPORT when SO_REUSEADDR is requested.  */
	if (level == SOL_SOCKET && optname == SO_REUSEADDR) {
		int type;
		socklen_t type_len = sizeof (type);

		if (!getsockopt (sock, level, SO_TYPE, &type, &type_len)) {
			if (type == SOCK_DGRAM || type == SOCK_STREAM)
				setsockopt (sock, level, SO_REUSEPORT, tmp_val, optlen);
		}
	}
#endif

	return ret;
}

gint
mono_w32socket_listen (SOCKET sock, gint backlog)
{
	gpointer handle;
	gint ret;

	handle = GUINT_TO_POINTER (sock);
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

	ret = listen (sock, backlog);
	if (ret == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: listen error: %s", __func__, strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return SOCKET_ERROR;
	}

	return 0;
}

gint
mono_w32socket_shutdown (SOCKET sock, gint how)
{
	MonoW32HandleSocket *socket_handle;
	gpointer handle;
	gint ret;

	handle = GUINT_TO_POINTER (sock);
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

	if (how == SHUT_RD || how == SHUT_RDWR) {
		if (!mono_w32handle_lookup (handle, MONO_W32HANDLE_SOCKET, (gpointer *)&socket_handle)) {
			g_warning ("%s: error looking up socket handle %p", __func__, handle);
			mono_w32socket_set_last_error (WSAENOTSOCK);
			return SOCKET_ERROR;
		}

		socket_handle->still_readable = 0;
	}

	ret = shutdown (sock, how);
	if (ret == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: shutdown error: %s", __func__, strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return SOCKET_ERROR;
	}

	return ret;
}

#ifdef HAVE_SYS_SELECT_H

gint
mono_w32socket_select (gint nfds, fd_set *readfds, fd_set *writefds, fd_set *exceptfds, struct timeval *timeout)
{
	gint ret, maxfd;
	MonoThreadInfo *info = mono_thread_info_current ();

	for (maxfd = FD_SETSIZE - 1; maxfd >= 0; maxfd--) {
		if ((readfds && FD_ISSET (maxfd, readfds)) ||
		    (writefds && FD_ISSET (maxfd, writefds)) ||
		    (exceptfds && FD_ISSET (maxfd, exceptfds))) {
			break;
		}
	}

	if (maxfd == -1) {
		mono_w32socket_set_last_error (WSAEINVAL);
		return SOCKET_ERROR;
	}

	do {
		ret = select(maxfd + 1, readfds, writefds, exceptfds, timeout);
	} while (ret == -1 && errno == EINTR &&
		 !mono_thread_info_is_interrupt_state (info));

	if (ret == -1) {
		gint errnum = errno;

		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: select error: %s", __func__, strerror (errno));
		errnum = errno_to_WSA (errnum, __func__);

		mono_w32socket_set_last_error (errnum);
		return SOCKET_ERROR;
	}

	return ret;
}

void
mono_w32socket_FD_CLR (SOCKET sock, fd_set *set)
{
	gpointer handle;

	if (sock >= FD_SETSIZE) {
		mono_w32socket_set_last_error (WSAEINVAL);
		return;
	}

	handle = GUINT_TO_POINTER (sock);
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return;
	}

	FD_CLR (sock, set);
}

gint
mono_w32socket_FD_ISSET (SOCKET sock, fd_set *set)
{
	gpointer handle;

	if (sock >= FD_SETSIZE) {
		mono_w32socket_set_last_error (WSAEINVAL);
		return 0;
	}

	handle = GUINT_TO_POINTER (sock);
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return 0;
	}

	return FD_ISSET (sock, set);
}

void
mono_w32socket_FD_SET (SOCKET sock, fd_set *set)
{
	gpointer handle;

	if (sock >= FD_SETSIZE) {
		mono_w32socket_set_last_error (WSAEINVAL);
		return;
	}

	handle = GUINT_TO_POINTER (sock);
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return;
	}

	FD_SET (sock, set);
}

#endif /* HAVE_SYS_SELECT_H */

gint
mono_w32socket_set_blocking (SOCKET socket, gboolean blocking)
{
	gint ret;
	gpointer handle;

	handle = GINT_TO_POINTER (socket);
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

#ifdef O_NONBLOCK
	/* This works better than ioctl(...FIONBIO...)
	 * on Linux (it causes connect to return
	 * EINPROGRESS, but the ioctl doesn't seem to) */
	ret = fcntl (socket, F_GETFL, 0);
	if (ret != -1)
		ret = fcntl (socket, F_SETFL, blocking ? (ret & (~O_NONBLOCK)) : (ret | (O_NONBLOCK)));
#endif /* O_NONBLOCK */

	if (ret == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: ioctl error: %s", __func__, strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return SOCKET_ERROR;
	}

	return 0;
}

gint
mono_w32socket_get_available (SOCKET socket, guint64 *amount)
{
	gint ret;
	gpointer handle;

	handle = GINT_TO_POINTER (socket);
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

#if defined (PLATFORM_MACOSX)
	// ioctl (socket, FIONREAD, XXX) returns the size of
	// the UDP header as well on Darwin.
	//
	// Use getsockopt SO_NREAD instead to get the
	// right values for TCP and UDP.
	//
	// ai_canonname can be null in some cases on darwin,
	// where the runtime assumes it will be the value of
	// the ip buffer.

	socklen_t optlen = sizeof (int);
	ret = getsockopt (socket, SOL_SOCKET, SO_NREAD, (gulong*) amount, &optlen);
#else
	ret = ioctl (socket, FIONREAD, (gulong*) amount);
#endif

	if (ret == -1) {
		gint errnum = errno;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: ioctl error: %s", __func__, strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);

		return SOCKET_ERROR;
	}

	return 0;
}

void
mono_w32socket_set_last_error (gint32 error)
{
	SetLastError (error);
}

gint32
mono_w32socket_get_last_error (void)
{
	return GetLastError ();
}

gint32
mono_w32socket_convert_error (gint error, const gchar *func)
{
	return errno_to_WSA (error, func);
}

gboolean
ves_icall_System_Net_Sockets_Socket_SupportPortReuse (MonoProtocolType proto)
{
#if defined (SO_REUSEPORT)
	return TRUE;
#else
#ifdef __linux__
	/* Linux always supports double binding for UDP, even on older kernels. */
	if (proto == ProtocolType_Udp)
		return TRUE;
#endif
	return FALSE;
#endif
}
