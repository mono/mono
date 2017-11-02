#include "w32socket.h"
#include "w32socket-internals.h"

#if defined(PLATFORM_UNITY) && defined(UNITY_USE_PLATFORM_STUBS)

gboolean
ves_icall_System_Net_Sockets_Socket_SupportPortReuse (MonoProtocolType proto)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

MonoBoolean
ves_icall_System_Net_Dns_GetHostByName_internal (MonoString *host, MonoString **h_name, MonoArray **h_aliases, MonoArray **h_addr_list, gint32 hint)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

void
mono_w32socket_initialize (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

void
mono_w32socket_cleanup (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

SOCKET mono_w32socket_accept (SOCKET s, struct sockaddr *addr, socklen_t *addrlen, gboolean blocking)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return INVALID_SOCKET;
}

int mono_w32socket_connect (SOCKET s, const struct sockaddr *name, int namelen, gboolean blocking)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

int mono_w32socket_recv (SOCKET s, char *buf, int len, int flags, gboolean blocking)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

int mono_w32socket_recvfrom (SOCKET s, char *buf, int len, int flags, struct sockaddr *from, socklen_t *fromlen, gboolean blocking)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

int mono_w32socket_recvbuffers (SOCKET s, WSABUF *lpBuffers, guint32 dwBufferCount, guint32 *lpNumberOfBytesRecvd, guint32 *lpFlags, gpointer lpOverlapped, gpointer lpCompletionRoutine, gboolean blocking)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

int mono_w32socket_send (SOCKET s, char *buf, int len, int flags, gboolean blocking)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

int mono_w32socket_sendto (SOCKET s, const char *buf, int len, int flags, const struct sockaddr *to, int tolen, gboolean blocking)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

int mono_w32socket_sendbuffers (SOCKET s, WSABUF *lpBuffers, guint32 dwBufferCount, guint32 *lpNumberOfBytesRecvd, guint32 lpFlags, gpointer lpOverlapped, gpointer lpCompletionRoutine, gboolean blocking)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

BOOL mono_w32socket_transmit_file (SOCKET hSocket, gpointer hFile, TRANSMIT_FILE_BUFFERS *lpTransmitBuffers, guint32 dwReserved, gboolean blocking)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gint
mono_w32socket_disconnect (SOCKET sock, gboolean reuse)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

gint
mono_w32socket_set_blocking (SOCKET sock, gboolean blocking)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

gint
mono_w32socket_get_available (SOCKET sock, guint64 *amount)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

void
mono_w32socket_set_last_error (gint32 error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

gint32
mono_w32socket_get_last_error (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

gint32
mono_w32socket_convert_error (gint error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

gint
mono_w32socket_bind (SOCKET sock, struct sockaddr *addr, socklen_t addrlen)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

gint
mono_w32socket_getpeername (SOCKET sock, struct sockaddr *name, socklen_t *namelen)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

gint
mono_w32socket_getsockname (SOCKET sock, struct sockaddr *name, socklen_t *namelen)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

gint
mono_w32socket_getsockopt (SOCKET sock, gint level, gint optname, gpointer optval, socklen_t *optlen)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

gint
mono_w32socket_setsockopt (SOCKET sock, gint level, gint optname, const gpointer optval, socklen_t optlen)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

gint
mono_w32socket_listen (SOCKET sock, gint backlog)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

gint
mono_w32socket_shutdown (SOCKET sock, gint how)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

SOCKET
mono_w32socket_socket (int domain, int type, int protocol)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return INVALID_SOCKET;
}

gboolean
mono_w32socket_close (SOCKET sock)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

#endif /* PLATFORM_UNITY && UNITY_USE_PLATFORM_STUBS */
