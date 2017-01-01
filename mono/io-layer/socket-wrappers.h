/*
 * Special header file to be included only in selected C files.
 * We need to use the _wapi_ equivalents of the socket API when
 * working with io-layer handles. On windows we define the wrappers to use
 * the normal win32 functions.
 */

#include <config.h>
#ifdef HAVE_SYS_SELECT_H
#  include <sys/select.h>
#endif
#ifdef HAVE_SYS_TIME_H
#  include <sys/time.h>
#endif
#ifdef HAVE_SYS_SOCKET_H
#  include <sys/socket.h>
#endif

#ifndef HAVE_SOCKLEN_T
#define socklen_t int
#endif

#ifdef HOST_WIN32
#define _wapi_bind bind 
#define _wapi_getpeername getpeername 
#define _wapi_getsockname getsockname 
#define _wapi_getsockopt getsockopt 
#define _wapi_listen listen 
#define _wapi_setsockopt setsockopt 
#define _wapi_shutdown shutdown 
#define _wapi_socket WSASocket 
#define _wapi_select select 

/* No need to wrap FD_ZERO because it doesnt involve file
 * descriptors
*/
#define _wapi_FD_CLR FD_CLR
#define _wapi_FD_ISSET FD_ISSET
#define _wapi_FD_SET FD_SET

#define _wapi_cleanup_networking() ;
#else

#define WSA_FLAG_OVERLAPPED           0x01

extern int _wapi_bind(guint32 handle, struct sockaddr *my_addr,
		      socklen_t addrlen);
extern int _wapi_getpeername(guint32 handle, struct sockaddr *name,
			     socklen_t *namelen);
extern int _wapi_getsockname(guint32 handle, struct sockaddr *name,
			     socklen_t *namelen);
extern int _wapi_getsockopt(guint32 handle, int level, int optname,
			    void *optval, socklen_t *optlen);
extern int _wapi_listen(guint32 handle, int backlog);
extern int _wapi_setsockopt(guint32 handle, int level, int optname,
			    const void *optval, socklen_t optlen);
extern int _wapi_shutdown(guint32 handle, int how);
extern guint32 _wapi_socket(int domain, int type, int protocol, void *unused,
			    guint32 unused2, guint32 flags);

#ifdef HAVE_SYS_SELECT_H
extern int _wapi_select(int nfds, fd_set *readfds, fd_set *writefds,
			fd_set *exceptfds, struct timeval *timeout);

extern void _wapi_FD_CLR(guint32 handle, fd_set *set);
extern int _wapi_FD_ISSET(guint32 handle, fd_set *set);
extern void _wapi_FD_SET(guint32 handle, fd_set *set);
#endif

extern void _wapi_cleanup_networking (void);
#endif /* HOST_WIN32 */

