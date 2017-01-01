/*
 * sockets.h:  Socket handles
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 */

#ifndef _WAPI_SOCKETS_H_
#define _WAPI_SOCKETS_H_

#include "mono/io-layer/wapi.h"

G_BEGIN_DECLS

#define INVALID_SOCKET (guint32)(~0)
#define SOCKET_ERROR -1

typedef struct
{
	guint32 Data1;
	guint16 Data2;
	guint16 Data3;
	guint8 Data4[8];
} WapiGuid;

#define TF_DISCONNECT 0x01
#define TF_REUSE_SOCKET 0x02

/* If we need to support more WSAIoctl commands then define these
 * using the bitfield flags method
 */
#define SIO_GET_EXTENSION_FUNCTION_POINTER 0xC8000006
#define SIO_KEEPALIVE_VALS 0x98000004

extern void WSASetLastError(int error);
extern int WSAGetLastError(void);
extern int closesocket(guint32 handle);

extern int ioctlsocket(guint32 handle, unsigned long command, gpointer arg);
extern int WSAIoctl (guint32 handle, gint32 command,
		     gchar *input, gint i_len,
		     gchar *output, gint o_len, glong *written,
		     void *unused1, void *unused2);

void
_wapi_socket_init (void);
G_END_DECLS
#endif /* _WAPI_SOCKETS_H_ */
