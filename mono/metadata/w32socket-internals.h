/*
* w32socket-internals.h
*
* Copyright 2016 Microsoft
* Licensed under the MIT license. See LICENSE file in the project root for full license information.
*/
#ifndef __MONO_METADATA_W32SOCKET_INTERNALS_H__
#define __MONO_METADATA_W32SOCKET_INTERNALS_H__

#include <config.h>
#include <glib.h>
#include <mono/io-layer/io-layer.h>

#ifndef HOST_WIN32

typedef struct {
	guint32 len;
	gpointer buf;
} WSABUF;

typedef struct {
	guint32 Internal;
	guint32 InternalHigh;
	guint32 Offset;
	guint32 OffsetHigh;
	gpointer hEvent;
	gpointer handle1;
	gpointer handle2;
} OVERLAPPED;

typedef struct {
	gpointer Head;
	guint32 HeadLength;
	gpointer Tail;
	guint32 TailLength;
} TRANSMIT_FILE_BUFFERS;

typedef struct {
	guint32 Data1;
	guint16 Data2;
	guint16 Data3;
	guint8 Data4[8];
} GUID;

#endif

typedef struct {
	int domain;
	int type;
	int protocol;
	int saved_error;
	int still_readable;
} MonoW32HandleSocket;

SOCKET
mono_w32socket_accept (SOCKET s, struct sockaddr *addr, socklen_t *addrlen, gboolean blocking);

int
mono_w32socket_connect (SOCKET s, const struct sockaddr *name, int namelen, gboolean blocking);

int
mono_w32socket_recv (SOCKET s, char *buf, int len, int flags, gboolean blocking);

int
mono_w32socket_recvfrom (SOCKET s, char *buf, int len, int flags, struct sockaddr *from, socklen_t *fromlen, gboolean blocking);

int
mono_w32socket_recvbuffers (SOCKET s, WSABUF *lpBuffers, guint32 dwBufferCount, guint32 *lpNumberOfBytesRecvd, guint32 *lpFlags, gpointer lpOverlapped, gpointer lpCompletionRoutine, gboolean blocking);

int
mono_w32socket_send (SOCKET s, char *buf, int len, int flags, gboolean blocking);

int
mono_w32socket_sendto (SOCKET s, const char *buf, int len, int flags, const struct sockaddr *to, int tolen, gboolean blocking);

int
mono_w32socket_sendbuffers (SOCKET s, WSABUF *lpBuffers, guint32 dwBufferCount, guint32 *lpNumberOfBytesRecvd, guint32 lpFlags, gpointer lpOverlapped, gpointer lpCompletionRoutine, gboolean blocking);

#if G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT | HAVE_UWP_WINAPI_SUPPORT)

BOOL
mono_w32socket_transmit_file (SOCKET hSocket, gpointer hFile, guint32 nNumberOfBytesToWrite, guint32 nNumberOfBytesPerSend, OVERLAPPED *lpOverlapped, TRANSMIT_FILE_BUFFERS *lpTransmitBuffers, guint32 dwReserved, gboolean blocking);

#endif

gint
mono_w32socket_set_blocking (SOCKET socket, gboolean blocking);

gint
mono_w32socket_get_available (SOCKET socket, guint64 *amount);

void
mono_w32socket_set_last_error (gint32 error);

gint32
mono_w32socket_get_last_error (void);

gint32
mono_w32socket_convert_error (gint error, const gchar *func);

#endif // __MONO_METADATA_W32SOCKET_INTERNALS_H__
