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
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return(FALSE);
	}
	
	newsock = socket (socket_handle->domain, socket_handle->type,
			  socket_handle->protocol);
	if (newsock == -1) {
		gint errnum = errno;

		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: socket error: %s", __func__, strerror (errno));

		errnum = errno_to_WSA (errnum, __func__);
		mono_w32socket_set_last_error (errnum);
		
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
		mono_w32socket_set_last_error (errnum);
		
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
		mono_w32socket_set_last_error (WSAEINVAL);
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
		mono_w32socket_set_last_error (WSAENOTSOCK);
		return SOCKET_ERROR;
	}

	if (command == SIO_GET_EXTENSION_FUNCTION_POINTER) {
		int i = 0;
		WapiGuid *guid = (WapiGuid *)input;
		
		if (i_len < sizeof(WapiGuid)) {
			/* As far as I can tell, windows doesn't
			 * actually set an error here...
			 */
			mono_w32socket_set_last_error (WSAEINVAL);
			return(SOCKET_ERROR);
		}

		if (o_len < sizeof(gpointer)) {
			/* Or here... */
			mono_w32socket_set_last_error (WSAEINVAL);
			return(SOCKET_ERROR);
		}

		if (output == NULL) {
			/* Or here */
			mono_w32socket_set_last_error (WSAEINVAL);
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
		
		mono_w32socket_set_last_error (WSAEINVAL);
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
		mono_w32socket_set_last_error (errnum);
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

#endif /* ifndef DISABLE_SOCKETS */
