/*
 * sockets.c:  Socket handles
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 */

#include <config.h>

#ifndef DISABLE_SOCKETS

#include <glib.h>
#include <pthread.h>
#include <errno.h>
#include <string.h>
#include <sys/types.h>
#include <sys/socket.h>
#ifdef HAVE_SYS_UIO_H
#  include <sys/uio.h>
#endif
#ifdef HAVE_SYS_IOCTL_H
#  include <sys/ioctl.h>
#endif
#ifdef HAVE_SYS_FILIO_H
#include <sys/filio.h>     /* defines FIONBIO and FIONREAD */
#endif
#ifdef HAVE_SYS_SOCKIO_H
#include <sys/sockio.h>    /* defines SIOCATMARK */
#endif
#include <unistd.h>
#include <fcntl.h>

#ifndef HAVE_MSG_NOSIGNAL
#include <signal.h>
#endif

#include <mono/io-layer/wapi.h>
#include <mono/io-layer/wapi-private.h>
#include <mono/io-layer/socket-wrappers.h>
#include <mono/io-layer/io-trace.h>
#include <mono/utils/mono-poll.h>
#include <mono/utils/mono-once.h>
#include <mono/utils/mono-logger-internals.h>
#include <mono/metadata/w32handle.h>
#include <mono/metadata/w32socket-internals.h>

#include <netinet/in.h>
#include <netinet/tcp.h>
#include <arpa/inet.h>
#ifdef HAVE_SYS_SENDFILE_H
#include <sys/sendfile.h>
#endif
#ifdef HAVE_NETDB_H
#include <netdb.h>
#endif

#define WSAID_DISCONNECTEX {0x7fda2e11,0x8630,0x436f,{0xa0, 0x31, 0xf5, 0x36, 0xa6, 0xee, 0xc1, 0x57}}
#define WSAID_TRANSMITFILE {0xb5367df0,0xcbac,0x11cf,{0x95,0xca,0x00,0x80,0x5f,0x48,0xa1,0x92}}

static guint32 in_cleanup = 0;

static void socket_close (gpointer handle, gpointer data);
static void socket_details (gpointer data);
static const gchar* socket_typename (void);
static gsize socket_typesize (void);

static MonoW32HandleOps _wapi_socket_ops = {
	socket_close,		/* close */
	NULL,			/* signal */
	NULL,			/* own */
	NULL,			/* is_owned */
	NULL,			/* special_wait */
	NULL,			/* prewait */
	socket_details,	/* details */
	socket_typename,	/* typename */
	socket_typesize,	/* typesize */
};

void
_wapi_socket_init (void)
{
	mono_w32handle_register_ops (MONO_W32HANDLE_SOCKET, &_wapi_socket_ops);
}

static void socket_close (gpointer handle, gpointer data)
{
	int ret;
	MonoW32HandleSocket *socket_handle = (MonoW32HandleSocket *)data;
	MonoThreadInfo *info = mono_thread_info_current ();

	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: closing socket handle %p", __func__, handle);

	/* Shutdown the socket for reading, to interrupt any potential
	 * receives that may be blocking for data.  See bug 75705.
	 */
	shutdown (GPOINTER_TO_UINT (handle), SHUT_RD);
	
	do {
		ret = close (GPOINTER_TO_UINT(handle));
	} while (ret == -1 && errno == EINTR &&
		 !mono_thread_info_is_interrupt_state (info));
	
	if (ret == -1) {
		gint errnum = errno;
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: close error: %s", __func__, strerror (errno));
		errnum = errno_to_WSA (errnum, __func__);
		if (!in_cleanup)
			WSASetLastError (errnum);
	}

	if (!in_cleanup)
		socket_handle->saved_error = 0;
}

static void socket_details (gpointer data)
{
	/* FIXME: do something */
}

static const gchar* socket_typename (void)
{
	return "Socket";
}

static gsize socket_typesize (void)
{
	return sizeof (MonoW32HandleSocket);
}

static gboolean
cleanup_close (gpointer handle, gpointer data, gpointer user_data)
{
	if (mono_w32handle_get_type (handle) == MONO_W32HANDLE_SOCKET)
		mono_w32handle_force_close (handle, data);

	return FALSE;
}

void _wapi_cleanup_networking(void)
{
	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: cleaning up", __func__);

	in_cleanup = 1;
	mono_w32handle_foreach (cleanup_close, NULL);
	in_cleanup = 0;
}

void WSASetLastError(int error)
{
	SetLastError (error);
}

int WSAGetLastError(void)
{
	return(GetLastError ());
}

int closesocket(guint32 fd)
{
	gpointer handle = GUINT_TO_POINTER (fd);
	
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		WSASetLastError (WSAENOTSOCK);
		return(0);
	}
	
	mono_w32handle_unref (handle);
	return(0);
}

int _wapi_bind(guint32 fd, struct sockaddr *my_addr, socklen_t addrlen)
{
	gpointer handle = GUINT_TO_POINTER (fd);
	int ret;
	
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		WSASetLastError (WSAENOTSOCK);
		return(SOCKET_ERROR);
	}
	
	ret = bind (fd, my_addr, addrlen);
	if (ret == -1) {
		gint errnum = errno;
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: bind error: %s", __func__, strerror(errno));
		errnum = errno_to_WSA (errnum, __func__);
		WSASetLastError (errnum);
		
		return(SOCKET_ERROR);
	}
	return(ret);
}

int _wapi_getpeername(guint32 fd, struct sockaddr *name, socklen_t *namelen)
{
	gpointer handle = GUINT_TO_POINTER (fd);
	int ret;
	
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		WSASetLastError (WSAENOTSOCK);
		return(SOCKET_ERROR);
	}

	ret = getpeername (fd, name, namelen);
	if (ret == -1) {
		gint errnum = errno;
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: getpeername error: %s", __func__,
			   strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		WSASetLastError (errnum);

		return(SOCKET_ERROR);
	}
	
	return(ret);
}

int _wapi_getsockname(guint32 fd, struct sockaddr *name, socklen_t *namelen)
{
	gpointer handle = GUINT_TO_POINTER (fd);
	int ret;
	
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		WSASetLastError (WSAENOTSOCK);
		return(SOCKET_ERROR);
	}

	ret = getsockname (fd, name, namelen);
	if (ret == -1) {
		gint errnum = errno;
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: getsockname error: %s", __func__,
			   strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		WSASetLastError (errnum);

		return(SOCKET_ERROR);
	}
	
	return(ret);
}

int _wapi_getsockopt(guint32 fd, int level, int optname, void *optval,
		     socklen_t *optlen)
{
	gpointer handle = GUINT_TO_POINTER (fd);
	int ret;
	struct timeval tv;
	void *tmp_val;
	MonoW32HandleSocket *socket_handle;
	gboolean ok;
	
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		WSASetLastError (WSAENOTSOCK);
		return(SOCKET_ERROR);
	}

	tmp_val = optval;
	if (level == SOL_SOCKET &&
	    (optname == SO_RCVTIMEO || optname == SO_SNDTIMEO)) {
		tmp_val = &tv;
		*optlen = sizeof (tv);
	}

	ret = getsockopt (fd, level, optname, tmp_val, optlen);
	if (ret == -1) {
		gint errnum = errno;
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: getsockopt error: %s", __func__,
			   strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		WSASetLastError (errnum);
		
		return(SOCKET_ERROR);
	}

	if (level == SOL_SOCKET &&
	    (optname == SO_RCVTIMEO || optname == SO_SNDTIMEO)) {
		*((int *) optval)  = tv.tv_sec * 1000 + (tv.tv_usec / 1000);	// milli from micro
		*optlen = sizeof (int);
	}

	if (optname == SO_ERROR) {
		ok = mono_w32handle_lookup (handle, MONO_W32HANDLE_SOCKET,
					  (gpointer *)&socket_handle);
		if (ok == FALSE) {
			g_warning ("%s: error looking up socket handle %p",
				   __func__, handle);

			/* can't extract the last error */
			*((int *) optval) = errno_to_WSA (*((int *)optval),
							  __func__);
		} else {
			if (*((int *)optval) != 0) {
				*((int *) optval) = errno_to_WSA (*((int *)optval),
								  __func__);
				socket_handle->saved_error = *((int *)optval);
			} else {
				*((int *)optval) = socket_handle->saved_error;
			}
		}
	}
	
	return(ret);
}

int _wapi_listen(guint32 fd, int backlog)
{
	gpointer handle = GUINT_TO_POINTER (fd);
	int ret;
	
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		WSASetLastError (WSAENOTSOCK);
		return(SOCKET_ERROR);
	}
	
	ret = listen (fd, backlog);
	if (ret == -1) {
		gint errnum = errno;
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: listen error: %s", __func__, strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		WSASetLastError (errnum);

		return(SOCKET_ERROR);
	}

	return(0);
}

int _wapi_setsockopt(guint32 fd, int level, int optname,
		     const void *optval, socklen_t optlen)
{
	gpointer handle = GUINT_TO_POINTER (fd);
	int ret;
	const void *tmp_val;
#if defined (__linux__)
	/* This has its address taken so it cannot be moved to the if block which uses it */
	int bufsize = 0;
#endif
	struct timeval tv;
	
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		WSASetLastError (WSAENOTSOCK);
		return(SOCKET_ERROR);
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
		
	ret = setsockopt (fd, level, optname, tmp_val, optlen);
	if (ret == -1) {
		gint errnum = errno;
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: setsockopt error: %s", __func__,
			   strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		WSASetLastError (errnum);
		
		return(SOCKET_ERROR);
	}

#if defined (SO_REUSEPORT)
	/* BSD's and MacOS X multicast sockets also need SO_REUSEPORT when SO_REUSEADDR is requested.  */
	if (level == SOL_SOCKET && optname == SO_REUSEADDR) {
		int type;
		socklen_t type_len = sizeof (type);

		if (!getsockopt (fd, level, SO_TYPE, &type, &type_len)) {
			if (type == SOCK_DGRAM || type == SOCK_STREAM)
				setsockopt (fd, level, SO_REUSEPORT, tmp_val, optlen);
		}
	}
#endif
	
	return(ret);
}

int _wapi_shutdown(guint32 fd, int how)
{
	MonoW32HandleSocket *socket_handle;
	gboolean ok;
	gpointer handle = GUINT_TO_POINTER (fd);
	int ret;

	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		WSASetLastError (WSAENOTSOCK);
		return(SOCKET_ERROR);
	}

	if (how == SHUT_RD ||
	    how == SHUT_RDWR) {
		ok = mono_w32handle_lookup (handle, MONO_W32HANDLE_SOCKET,
					  (gpointer *)&socket_handle);
		if (ok == FALSE) {
			g_warning ("%s: error looking up socket handle %p",
				   __func__, handle);
			WSASetLastError (WSAENOTSOCK);
			return(SOCKET_ERROR);
		}
		
		socket_handle->still_readable = 0;
	}
	
	ret = shutdown (fd, how);
	if (ret == -1) {
		gint errnum = errno;
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: shutdown error: %s", __func__,
			   strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		WSASetLastError (errnum);
		
		return(SOCKET_ERROR);
	}
	
	return(ret);
}

guint32 _wapi_socket(int domain, int type, int protocol, void *unused,
		     guint32 unused2, guint32 unused3)
{
	MonoW32HandleSocket socket_handle = {0};
	gpointer handle;
	int fd;
	
	socket_handle.domain = domain;
	socket_handle.type = type;
	socket_handle.protocol = protocol;
	socket_handle.still_readable = 1;
	
	fd = socket (domain, type, protocol);
	if (fd == -1 && domain == AF_INET && type == SOCK_RAW &&
	    protocol == 0) {
		/* Retry with protocol == 4 (see bug #54565) */
		// https://bugzilla.novell.com/show_bug.cgi?id=MONO54565
		socket_handle.protocol = 4;
		fd = socket (AF_INET, SOCK_RAW, 4);
	}
	
	if (fd == -1) {
		gint errnum = errno;
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: socket error: %s", __func__, strerror (errno));
		errnum = errno_to_WSA (errnum, __func__);
		WSASetLastError (errnum);

		return(INVALID_SOCKET);
	}

	if (fd >= mono_w32handle_fd_reserve) {
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: File descriptor is too big (%d >= %d)",
			   __func__, fd, mono_w32handle_fd_reserve);

		WSASetLastError (WSASYSCALLFAILURE);
		close (fd);
		
		return(INVALID_SOCKET);
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
	
		ret = setsockopt (fd, SOL_SOCKET, SO_REUSEADDR, &true_,
				  sizeof (true_));
		if (ret == -1) {
			int errnum = errno;

			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Error setting SO_REUSEADDR", __func__);
			
			errnum = errno_to_WSA (errnum, __func__);
			WSASetLastError (errnum);

			close (fd);

			return(INVALID_SOCKET);			
		}
	}
	
	
	handle = mono_w32handle_new_fd (MONO_W32HANDLE_SOCKET, fd, &socket_handle);
	if (handle == INVALID_HANDLE_VALUE) {
		g_warning ("%s: error creating socket handle", __func__);
		WSASetLastError (WSASYSCALLFAILURE);
		close (fd);
		return(INVALID_SOCKET);
	}

	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: returning socket handle %p", __func__, handle);

	return(fd);
}

static gboolean socket_disconnect (guint32 fd)
{
	MonoW32HandleSocket *socket_handle;
	gboolean ok;
	gpointer handle = GUINT_TO_POINTER (fd);
	int newsock, ret;
	
	ok = mono_w32handle_lookup (handle, MONO_W32HANDLE_SOCKET,
				  (gpointer *)&socket_handle);
	if (ok == FALSE) {
		g_warning ("%s: error looking up socket handle %p", __func__,
			   handle);
		WSASetLastError (WSAENOTSOCK);
		return(FALSE);
	}
	
	newsock = socket (socket_handle->domain, socket_handle->type,
			  socket_handle->protocol);
	if (newsock == -1) {
		gint errnum = errno;

		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: socket error: %s", __func__, strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		WSASetLastError (errnum);
		
		return(FALSE);
	}

	/* According to Stevens "Advanced Programming in the UNIX
	 * Environment: UNIX File I/O" dup2() is atomic so there
	 * should not be a race condition between the old fd being
	 * closed and the new socket fd being copied over
	 */
	do {
		ret = dup2 (newsock, fd);
	} while (ret == -1 && errno == EAGAIN);
	
	if (ret == -1) {
		gint errnum = errno;
		
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: dup2 error: %s", __func__, strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		WSASetLastError (errnum);
		
		return(FALSE);
	}

	close (newsock);
	
	return(TRUE);
}

static gboolean wapi_disconnectex (guint32 fd, WapiOverlapped *overlapped,
				   guint32 flags, guint32 reserved)
{
	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: called on socket %d!", __func__, fd);
	
	if (reserved != 0) {
		WSASetLastError (WSAEINVAL);
		return(FALSE);
	}

	/* We could check the socket type here and fail unless its
	 * SOCK_STREAM, SOCK_SEQPACKET or SOCK_RDM (according to msdn)
	 * if we really wanted to
	 */

	return(socket_disconnect (fd));
}

static struct 
{
	WapiGuid guid;
	gpointer func;
} extension_functions[] = {
	{WSAID_DISCONNECTEX, wapi_disconnectex},
	{WSAID_TRANSMITFILE, mono_w32socket_transmit_file},
	{{0}, NULL},
};

int
WSAIoctl (guint32 fd, gint32 command,
	  gchar *input, gint i_len,
	  gchar *output, gint o_len, glong *written,
	  void *unused1, void *unused2)
{
	gpointer handle = GUINT_TO_POINTER (fd);
	int ret;
	gchar *buffer = NULL;

	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		WSASetLastError (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

	if (command == SIO_GET_EXTENSION_FUNCTION_POINTER) {
		int i = 0;
		WapiGuid *guid = (WapiGuid *)input;
		
		if (i_len < sizeof(WapiGuid)) {
			/* As far as I can tell, windows doesn't
			 * actually set an error here...
			 */
			WSASetLastError (WSAEINVAL);
			return(SOCKET_ERROR);
		}

		if (o_len < sizeof(gpointer)) {
			/* Or here... */
			WSASetLastError (WSAEINVAL);
			return(SOCKET_ERROR);
		}

		if (output == NULL) {
			/* Or here */
			WSASetLastError (WSAEINVAL);
			return(SOCKET_ERROR);
		}
		
		while(extension_functions[i].func != NULL) {
			if (!memcmp (guid, &extension_functions[i].guid,
				     sizeof(WapiGuid))) {
				memcpy (output, &extension_functions[i].func,
					sizeof(gpointer));
				*written = sizeof(gpointer);
				return(0);
			}

			i++;
		}
		
		WSASetLastError (WSAEINVAL);
		return(SOCKET_ERROR);
	}

	if (command == SIO_KEEPALIVE_VALS) {
		uint32_t onoff;
		uint32_t keepalivetime;
		uint32_t keepaliveinterval;

		if (i_len < (3 * sizeof (uint32_t))) {
			WSASetLastError (WSAEINVAL);
			return SOCKET_ERROR;
		}
		memcpy (&onoff, input, sizeof (uint32_t));
		memcpy (&keepalivetime, input + sizeof (uint32_t), sizeof (uint32_t));
		memcpy (&keepaliveinterval, input + 2 * sizeof (uint32_t), sizeof (uint32_t));
		ret = setsockopt (fd, SOL_SOCKET, SO_KEEPALIVE, &onoff, sizeof (uint32_t));
		if (ret < 0) {
			gint errnum = errno;
			errnum = errno_to_WSA (errnum, __func__);
			WSASetLastError (errnum);
			return SOCKET_ERROR;
		}
		if (onoff != 0) {
#if defined(TCP_KEEPIDLE) && defined(TCP_KEEPINTVL)
			/* Values are in ms, but we need s */
			uint32_t rem;

			/* keepalivetime and keepaliveinterval are > 0 (checked in managed code) */
			rem = keepalivetime % 1000;
			keepalivetime /= 1000;
			if (keepalivetime == 0 || rem >= 500)
				keepalivetime++;
			ret = setsockopt (fd, IPPROTO_TCP, TCP_KEEPIDLE, &keepalivetime, sizeof (uint32_t));
			if (ret == 0) {
				rem = keepaliveinterval % 1000;
				keepaliveinterval /= 1000;
				if (keepaliveinterval == 0 || rem >= 500)
					keepaliveinterval++;
				ret = setsockopt (fd, IPPROTO_TCP, TCP_KEEPINTVL, &keepaliveinterval, sizeof (uint32_t));
			}
			if (ret != 0) {
				gint errnum = errno;
				errnum = errno_to_WSA (errnum, __func__);
				WSASetLastError (errnum);
				return SOCKET_ERROR;
			}
			return 0;
#endif
		}
		return 0;
	}

	if (i_len > 0) {
		buffer = (char *)g_memdup (input, i_len);
	}

	ret = ioctl (fd, command, buffer);
	if (ret == -1) {
		gint errnum = errno;
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: WSAIoctl error: %s", __func__,
			  strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		WSASetLastError (errnum);
		g_free (buffer);
		
		return(SOCKET_ERROR);
	}

	if (buffer == NULL) {
		*written = 0;
	} else {
		/* We just copy the buffer to the output. Some ioctls
		 * don't even output any data, but, well...
		 *
		 * NB windows returns WSAEFAULT if o_len is too small
		 */
		i_len = (i_len > o_len) ? o_len : i_len;

		if (i_len > 0 && output != NULL) {
			memcpy (output, buffer, i_len);
		}
		
		g_free (buffer);
		*written = i_len;
	}

	return(0);
}

#ifndef PLATFORM_PORT_PROVIDES_IOCTLSOCKET
int ioctlsocket(guint32 fd, unsigned long command, gpointer arg)
{
	gpointer handle = GUINT_TO_POINTER (fd);
	int ret;
	
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		WSASetLastError (WSAENOTSOCK);
		return(SOCKET_ERROR);
	}

	switch(command){
		case FIONBIO:
#ifdef O_NONBLOCK
			/* This works better than ioctl(...FIONBIO...) 
			 * on Linux (it causes connect to return
			 * EINPROGRESS, but the ioctl doesn't seem to)
			 */
			ret = fcntl(fd, F_GETFL, 0);
			if (ret != -1) {
				if (*(gboolean *)arg) {
					ret |= O_NONBLOCK;
				} else {
					ret &= ~O_NONBLOCK;
				}
				ret = fcntl(fd, F_SETFL, ret);
			}
			break;
#endif /* O_NONBLOCK */
			/* Unused in Mono */
		case SIOCATMARK:
			ret = ioctl (fd, command, arg);
			break;
			
		case FIONREAD:
		{
#if defined (PLATFORM_MACOSX)
			
			// ioctl (fd, FIONREAD, XXX) returns the size of
			// the UDP header as well on
			// Darwin.
			//
			// Use getsockopt SO_NREAD instead to get the
			// right values for TCP and UDP.
			// 
			// ai_canonname can be null in some cases on darwin, where the runtime assumes it will
			// be the value of the ip buffer.

			socklen_t optlen = sizeof (int);
			ret = getsockopt (fd, SOL_SOCKET, SO_NREAD, arg, &optlen);
#else
			ret = ioctl (fd, command, arg);
#endif
			break;
		}
		default:
			WSASetLastError (WSAEINVAL);
			return(SOCKET_ERROR);
	}

	if (ret == -1) {
		gint errnum = errno;
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: ioctl error: %s", __func__, strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		WSASetLastError (errnum);
		
		return(SOCKET_ERROR);
	}
	
	return(0);
}

int _wapi_select(int nfds G_GNUC_UNUSED, fd_set *readfds, fd_set *writefds,
		 fd_set *exceptfds, struct timeval *timeout)
{
	int ret, maxfd;
	MonoThreadInfo *info = mono_thread_info_current ();
	
	for (maxfd = FD_SETSIZE-1; maxfd >= 0; maxfd--) {
		if ((readfds && FD_ISSET (maxfd, readfds)) ||
		    (writefds && FD_ISSET (maxfd, writefds)) ||
		    (exceptfds && FD_ISSET (maxfd, exceptfds))) {
			break;
		}
	}

	if (maxfd == -1) {
		WSASetLastError (WSAEINVAL);
		return(SOCKET_ERROR);
	}

	do {
		ret = select(maxfd + 1, readfds, writefds, exceptfds,
			     timeout);
	} while (ret == -1 && errno == EINTR &&
		 !mono_thread_info_is_interrupt_state (info));

	if (ret == -1) {
		gint errnum = errno;
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: select error: %s", __func__, strerror (errno));
		errnum = errno_to_WSA (errnum, __func__);
		WSASetLastError (errnum);
		
		return(SOCKET_ERROR);
	}

	return(ret);
}

void _wapi_FD_CLR(guint32 fd, fd_set *set)
{
	gpointer handle = GUINT_TO_POINTER (fd);
	
	if (fd >= FD_SETSIZE) {
		WSASetLastError (WSAEINVAL);
		return;
	}
	
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		WSASetLastError (WSAENOTSOCK);
		return;
	}

	FD_CLR (fd, set);
}

int _wapi_FD_ISSET(guint32 fd, fd_set *set)
{
	gpointer handle = GUINT_TO_POINTER (fd);
	
	if (fd >= FD_SETSIZE) {
		WSASetLastError (WSAEINVAL);
		return(0);
	}
	
	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		WSASetLastError (WSAENOTSOCK);
		return(0);
	}

	return(FD_ISSET (fd, set));
}

void _wapi_FD_SET(guint32 fd, fd_set *set)
{
	gpointer handle = GUINT_TO_POINTER (fd);
	
	if (fd >= FD_SETSIZE) {
		WSASetLastError (WSAEINVAL);
		return;
	}

	if (mono_w32handle_get_type (handle) != MONO_W32HANDLE_SOCKET) {
		WSASetLastError (WSAENOTSOCK);
		return;
	}

	FD_SET (fd, set);
}
#endif

#endif /* ifndef DISABLE_SOCKETS */
