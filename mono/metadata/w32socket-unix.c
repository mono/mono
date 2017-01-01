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

#include <sys/types.h>

#include "w32socket.h"
#include "w32socket-internals.h"
#include "w32handle.h"
#include "utils/mono-logger-internals.h"
#include "utils/mono-poll.h"


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
		closesocket (sock);

	return TRUE;
}

BOOL
mono_w32socket_transmit_file (SOCKET hSocket, gpointer hFile, guint32 nNumberOfBytesToWrite, guint32 nNumberOfBytesPerSend, OVERLAPPED *lpOverlapped, TRANSMIT_FILE_BUFFERS *lpTransmitBuffers, guint32 dwReserved, gboolean blocking)
{
	return TransmitFile (hSocket, hFile, nNumberOfBytesToWrite, nNumberOfBytesPerSend, lpOverlapped, lpTransmitBuffers, dwReserved);
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
