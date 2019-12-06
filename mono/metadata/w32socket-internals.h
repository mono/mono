/**
 * \file
 *
 * Copyright 2016 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#ifndef __MONO_METADATA_W32SOCKET_INTERNALS_H__
#define __MONO_METADATA_W32SOCKET_INTERNALS_H__

#include <config.h>
#include <glib.h>

#ifdef HAVE_SYS_TIME_H
#include <sys/time.h>
#endif
#ifdef HAVE_SYS_SOCKET_H
#include <sys/socket.h>
#endif

#include <mono/utils/w32api.h>

#if !ENABLE_NETCORE

// MonoSocketAddress was already something else so add "Managed" to this name.
// Keep in sync with class System.Net.SocketAddress in mcs/class/referencesource/System/net/System/Net/SocketAddress.cs.
typedef struct MonoManagedSocketAddress {
	MonoObject	object;
	gint32		m_Size;
	MonoArray*	m_Buffer; // byte []
	MonoBoolean	m_changed;
	gint32		m_hash;
} MonoManagedSocketAddress;

TYPED_HANDLE_DECL (MonoManagedSocketAddress);

#if 0 // old

typedef struct MonoIPAddress {
	MonoObject		object;
	gint64			m_Address;
	MonoString*		m_ToString;
	gint32			m_Family;	// MonoAddressFamily
	MonoArray*		m_Numbers;	// ushort[]
	gint64			m_ScopeId;
	gint32			m_HashCode;
} MonoIPAddress;

#else // corefx

// Keep in sync with class System.Net.IPAddress in external/corefx/src/System.Net.Primitives/src/System/Net/IPAddress.cs.
typedef struct MonoIPAddress {
	MonoObject	object;
	guint32		_addressOrScopeId;
	MonoArray*	_numbers;	// ushort[]
	MonoString*	_toString;
	gint32		_hashCode;
} MonoIPAddress;

#endif

TYPED_HANDLE_DECL (MonoIPAddress);

// Keep in sync with System.Net.LingerOption in mcs/class/referencesource/System/net/System/Net/Sockets/LingerOption.cs.
typedef struct MonoLingerOption {
	MonoObject	object;
	MonoBoolean	enabled;
	gint32		lingerTime;
} MonoLingerOption;

TYPED_HANDLE_DECL (MonoLingerOption);

// Keep in sync with System.Net.MulticastOption in mcs/class/System/System.Net.Sockets/Socket.cs.
typedef struct MonoMulticastOption {
	MonoObject	object;
	MonoIPAddress*	group;
	MonoIPAddress*	localAddress;
	gint32		ifIndex;
} MonoMulticastOption;

TYPED_HANDLE_DECL (MonoMulticastOption);

// Keep in sync with System.Net.IPv6MulticastOption in mcs/class/referencesource/System/net/System/Net/Sockets/MulticastOption.cs.
typedef struct MonoIPv6MulticastOption {
	MonoObject	object;
	MonoIPAddress*	m_Group;
	gint64		m_Interface;
} MonoIPv6MulticastOption;

TYPED_HANDLE_DECL (MonoIPv6MulticastOption);

// Details of this type are not needed.
//
typedef struct MonoSemaphoreSlim MonoSemaphoreSlim;

// Details of this type are not needed.
//
typedef struct MonoEndPoint MonoEndPoint;

// Keep in sync with System.Net.Socket in mcs/class/System/System.Net.Sockets/Socket.cs.
typedef struct MonoSocket {
	MonoObject		object;
	MonoBoolean		is_closed;
	MonoBoolean		is_listening;
	MonoBoolean		useOverlappedIO;
	gint32			linger_timeout;
	gint32			addressFamily;	// enum AddressFamily
	gint32			socketType;	// enum SocketType
	gint32			protocolType;	// enum ProtocolType
	MonoSafeHandle*		m_Handle;
	MonoEndPoint*		seed_endpoint;
	MonoSemaphoreSlim*	ReadSem;
	MonoSemaphoreSlim*	WriteSem;
	MonoBoolean		is_blocking;
	MonoBoolean		is_bound;
	MonoBoolean		is_connected;
	gint32			m_IntCleanedUp;
	MonoBoolean		connect_in_progress;
} MonoSocket;

TYPED_HANDLE_DECL (MonoSocket);

#else

// Netcore does not use these types.

#endif // ENABLE_NETCORE

#ifndef HAVE_SOCKLEN_T
#define socklen_t int
#endif

#ifndef HOST_WIN32

#define TF_DISCONNECT 0x01
#define TF_REUSE_SOCKET 0x02

typedef struct {
	gpointer Head;
	guint32 HeadLength;
	gpointer Tail;
	guint32 TailLength;
} TRANSMIT_FILE_BUFFERS, *LPTRANSMIT_FILE_BUFFERS;

typedef struct {
	guint32 Data1;
	guint16 Data2;
	guint16 Data3;
	guint8 Data4[8];
} GUID;

typedef struct {
	guint32 Internal;
	guint32 InternalHigh;
	guint32 Offset;
	guint32 OffsetHigh;
	gpointer hEvent;
	gpointer handle1;
	gpointer handle2;
} OVERLAPPED;

#endif

void
mono_w32socket_initialize (void);

void
mono_w32socket_cleanup (void);

SOCKET
mono_w32socket_accept (SOCKET s, struct sockaddr *addr, socklen_t *addrlen, gboolean blocking);

int
mono_w32socket_connect (SOCKET s, const struct sockaddr *name, int namelen, gboolean blocking);

int
mono_w32socket_recv (SOCKET s, char *buf, int len, int flags, gboolean blocking);

int
mono_w32socket_recvfrom (SOCKET s, char *buf, int len, int flags, struct sockaddr *from, socklen_t *fromlen, gboolean blocking);

int
mono_w32socket_recvbuffers (SOCKET s, LPWSABUF lpBuffers, guint32 dwBufferCount, guint32 *lpNumberOfBytesRecvd, guint32 *lpFlags, gpointer lpOverlapped, gpointer lpCompletionRoutine, gboolean blocking);

int
mono_w32socket_send (SOCKET s, void *buf, int len, int flags, gboolean blocking);

int
mono_w32socket_sendto (SOCKET s, const char *buf, int len, int flags, const struct sockaddr *to, int tolen, gboolean blocking);

int
mono_w32socket_sendbuffers (SOCKET s, LPWSABUF lpBuffers, guint32 dwBufferCount, guint32 *lpNumberOfBytesRecvd, guint32 lpFlags, gpointer lpOverlapped, gpointer lpCompletionRoutine, gboolean blocking);

#if G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT | HAVE_UWP_WINAPI_SUPPORT)

BOOL
mono_w32socket_transmit_file (SOCKET hSocket, gpointer hFile, LPTRANSMIT_FILE_BUFFERS lpTransmitBuffers, guint32 dwReserved, gboolean blocking);

#endif

#ifndef HOST_WIN32

SOCKET
mono_w32socket_socket (int domain, int type, int protocol);

gint
mono_w32socket_bind (SOCKET sock, struct sockaddr *addr, socklen_t addrlen);

gint
mono_w32socket_getpeername (SOCKET sock, struct sockaddr *name, socklen_t *namelen);

gint
mono_w32socket_getsockname (SOCKET sock, struct sockaddr *name, socklen_t *namelen);

gint
mono_w32socket_getsockopt (SOCKET sock, gint level, gint optname, gpointer optval, socklen_t *optlen);

gint
mono_w32socket_setsockopt (SOCKET sock, gint level, gint optname, gconstpointer optval, socklen_t optlen);

gint
mono_w32socket_listen (SOCKET sock, gint backlog);

gint
mono_w32socket_shutdown (SOCKET sock, gint how);

gint
mono_w32socket_ioctl (SOCKET sock, gint32 command, gchar *input, gint inputlen, gchar *output, gint outputlen, glong *written);

gboolean
mono_w32socket_close (SOCKET sock);

#endif /* HOST_WIN32 */

gint
mono_w32socket_disconnect (SOCKET sock, gboolean reuse);

gint
mono_w32socket_set_blocking (SOCKET socket, gboolean blocking);

gint
mono_w32socket_get_available (SOCKET socket, guint64 *amount);

void
mono_w32socket_set_last_error (gint32 error);

gint32
mono_w32socket_get_last_error (void);

gint32
mono_w32socket_convert_error (gint error);

gboolean
mono_w32socket_duplicate (gpointer handle, gint32 targetProcessId, gpointer *duplicate_handle);

#endif // __MONO_METADATA_W32SOCKET_INTERNALS_H__
