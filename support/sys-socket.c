/*
 * <sys/socket.h> wrapper functions.
 *
 * Authors:
 *   Steffen Kiess (s-kiess@web.de)
 *
 * Copyright (C) 2015 Steffen Kiess
 */

#include <sys/socket.h>
#include <netinet/in.h>
#include <sys/un.h>

#include <stddef.h>

#include "map.h"
#include "mph.h"
#include "sys-uio.h"

G_BEGIN_DECLS

int
Mono_Posix_Sockaddr_offsetof_sockaddr_sa_data (void)
{
	return offsetof (struct sockaddr, sa_data);
}

int
Mono_Posix_Sockaddr_sizeof_sockaddr_storage (void)
{
#ifdef HAVE_STRUCT_SOCKADDR_STORAGE
	return sizeof (struct sockaddr_storage);
#else
	return -1;
#endif
}

int
Mono_Posix_Cmsghdr_getsize (void)
{
	return sizeof (struct cmsghdr);
}

int
Mono_Posix_SockaddrIn_getsize (void)
{
#ifdef HAVE_STRUCT_SOCKADDR_IN
	return sizeof (struct sockaddr_in);
#else
	return -1;
#endif
}

int
Mono_Posix_SockaddrIn6_getsize (void)
{
#ifdef HAVE_STRUCT_SOCKADDR_IN6
	return sizeof (struct sockaddr_in6);
#else
	return -1;
#endif
}

int
Mono_Posix_SockaddrUn_get_path_offset (void)
{
#ifdef HAVE_STRUCT_SOCKADDR_UN
	return offsetof (struct sockaddr_un, sun_path);
#else
	return -1;
#endif
}

int
Mono_Posix_FromInAddr (struct Mono_Posix_InAddr* source, void* destination)
{
	memcpy (&((struct in_addr*)destination)->s_addr, &source->addr, 4);
	return 0;
}

int
Mono_Posix_ToInAddr (void* source, struct Mono_Posix_InAddr* destination)
{
	memcpy (&destination->addr, &((struct in_addr*)source)->s_addr, 4);
	return 0;
}

int
Mono_Posix_FromIn6Addr (struct Mono_Posix_In6Addr* source, void* destination)
{
	memcpy (&((struct in6_addr*)destination)->s6_addr, &source->addr0, 16);
	return 0;
}

int
Mono_Posix_ToIn6Addr (void* source, struct Mono_Posix_In6Addr* destination)
{
	memcpy (&destination->addr0, &((struct in6_addr*)source)->s6_addr, 16);
	return 0;
}


int
Mono_Posix_Syscall_socketpair (int domain, int type, int protocol, int* sv0, int* sv1)
{
	int filedes[2] = {-1, -1};
	int r;

	r = socketpair (domain, type, protocol, filedes);

	*sv0 = filedes[0];
	*sv1 = filedes[1];
	return r;
}

int
Mono_Posix_Syscall_getsockopt (int socket, int level, int option_name, void* option_value, gint64* option_len)
{
	socklen_t len;
	int r;

	mph_return_if_socklen_t_overflow (*option_len);

	len = *option_len;

	r = getsockopt (socket, level, option_name, option_value, &len);

	*option_len = len;

	return r;
}

int
Mono_Posix_Syscall_getsockopt_timeval (int socket, int level, int option_name, struct Mono_Posix_Timeval* option_value)
{
	struct timeval tv;
	int r;
	socklen_t size;

	size = sizeof (struct timeval);
	r = getsockopt (socket, level, option_name, &tv, &size);

	if (r != -1 && size == sizeof (struct timeval)) {
		if (Mono_Posix_ToTimeval (&tv, option_value) != 0)
			return -1;
	} else {
		memset (option_value, 0, sizeof (struct Mono_Posix_Timeval));
		if (r != -1)
			errno = EINVAL;
	}

	return r;
}

int
Mono_Posix_Syscall_getsockopt_linger (int socket, int level, int option_name, struct Mono_Posix_Linger* option_value)
{
	struct linger ling;
	int r;
	socklen_t size;

	size = sizeof (struct linger);
	r = getsockopt (socket, level, option_name, &ling, &size);

	if (r != -1 && size == sizeof (struct linger)) {
		if (Mono_Posix_ToLinger (&ling, option_value) != 0)
			return -1;
	} else {
		memset (option_value, 0, sizeof (struct Mono_Posix_Linger));
		if (r != -1)
			errno = EINVAL;
	}

	return r;
}

int
Mono_Posix_Syscall_setsockopt (int socket, int level, int option_name, void* option_value, gint64 option_len)
{
	mph_return_if_socklen_t_overflow (option_len);

	return setsockopt (socket, level, option_name, option_value, option_len);
}

int
Mono_Posix_Syscall_setsockopt_timeval (int socket, int level, int option_name, struct Mono_Posix_Timeval* option_value)
{
	struct timeval tv;

	if (Mono_Posix_FromTimeval (option_value, &tv) != 0)
		return -1;

	return setsockopt (socket, level, option_name, &tv, sizeof (struct timeval));
}

int
Mono_Posix_Syscall_setsockopt_linger (int socket, int level, int option_name, struct Mono_Posix_Linger* option_value)
{
	struct linger ling;

	if (Mono_Posix_FromLinger (option_value, &ling) != 0)
		return -1;

	return setsockopt (socket, level, option_name, &ling, sizeof (struct linger));
}

int
Mono_Posix_Syscall_bind (int socket, unsigned char* addr, gint64 addrlen)
{
	mph_return_if_socklen_t_overflow (addrlen);

	return bind (socket, (struct sockaddr*) addr, addrlen);
}

int
Mono_Posix_Syscall_connect (int socket, unsigned char* addr, gint64 addrlen)
{
	mph_return_if_socklen_t_overflow (addrlen);

	return connect (socket, (struct sockaddr*) addr, addrlen);
}

int
Mono_Posix_Syscall_accept (int socket, unsigned char* addr, gint64* addrlen)
{
	socklen_t len;
	int r;

	mph_return_if_socklen_t_overflow (*addrlen);
	len = *addrlen;

	r = accept (socket, (struct sockaddr*) addr, &len);

	*addrlen = len;
	return r;
}

int
Mono_Posix_Syscall_accept4 (int socket, unsigned char* addr, gint64* addrlen, int flags)
{
	socklen_t len;
	int r;

	mph_return_if_socklen_t_overflow (*addrlen);
	len = *addrlen;

	r = accept4 (socket, (struct sockaddr*) addr, &len, flags);

	*addrlen = len;
	return r;
}

int
Mono_Posix_Syscall_getpeername (int socket, unsigned char* addr, gint64* addrlen)
{
	socklen_t len;
	int r;

	mph_return_if_socklen_t_overflow (*addrlen);
	len = *addrlen;

	r = getpeername (socket, (struct sockaddr*) addr, &len);

	*addrlen = len;
	return r;
}

int
Mono_Posix_Syscall_getsockname (int socket, unsigned char* addr, gint64* addrlen)
{
	socklen_t len;
	int r;

	mph_return_if_socklen_t_overflow (*addrlen);
	len = *addrlen;

	r = getsockname (socket, (struct sockaddr*) addr, &len);

	*addrlen = len;
	return r;
}

gint64
Mono_Posix_Syscall_recv (int socket, void* message, guint64 length, int flags)
{
	mph_return_if_size_t_overflow (length);

	return recv (socket, message, length, flags);
}

gint64
Mono_Posix_Syscall_send (int socket, void* message, guint64 length, int flags)
{
	mph_return_if_size_t_overflow (length);

	return send (socket, message, length, flags);
}

gint64
Mono_Posix_Syscall_recvfrom (int socket, void* buffer, guint64 length, int flags, unsigned char* addr, gint64* addrlen)
{
	socklen_t len;
	int r;

	mph_return_if_size_t_overflow (length);
	mph_return_if_socklen_t_overflow (*addrlen);
	len = *addrlen;

	r = recvfrom (socket, buffer, length, flags, (struct sockaddr*) addr, &len);

	*addrlen = len;
	return r;
}

gint64
Mono_Posix_Syscall_sendto (int socket, void* message, guint64 length, int flags, unsigned char* addr, gint64 addrlen)
{
	mph_return_if_size_t_overflow (length);
	mph_return_if_socklen_t_overflow (addrlen);

	return sendto (socket, message, length, flags, (struct sockaddr*) addr, addrlen);
}

gint64
Mono_Posix_Syscall_recvmsg (int socket, struct Mono_Posix_Syscall__Msghdr* message, int flags)
{
	struct msghdr hdr;
	int r;

	memset (&hdr, 0, sizeof (struct msghdr));

	hdr.msg_name = message->msg_name;
	hdr.msg_namelen = message->msg_namelen;
	hdr.msg_iovlen = message->msg_iovlen;
	hdr.msg_control = message->msg_control;
	hdr.msg_controllen = message->msg_controllen;
			
	hdr.msg_iov = _mph_from_iovec_array (message->msg_iov, message->msg_iovlen);
	r = recvmsg (socket, &hdr, flags);
	free (hdr.msg_iov);

	message->msg_namelen = hdr.msg_namelen;
	message->msg_controllen = hdr.msg_controllen;
	message->msg_flags = hdr.msg_flags;

	return r;
}

gint64
Mono_Posix_Syscall_sendmsg (int socket, struct Mono_Posix_Syscall__Msghdr* message, int flags)
{
	struct msghdr hdr;
	int r;

	memset (&hdr, 0, sizeof (struct msghdr));

	hdr.msg_name = message->msg_name;
	hdr.msg_namelen = message->msg_namelen;
	hdr.msg_iovlen = message->msg_iovlen;
	hdr.msg_control = message->msg_control;
	hdr.msg_controllen = message->msg_controllen;

	hdr.msg_iov = _mph_from_iovec_array (message->msg_iov, message->msg_iovlen);
	r = sendmsg (socket, &hdr, flags);
	free (hdr.msg_iov);

	return r;
}

static inline void make_msghdr (struct msghdr* hdr, unsigned char* msg_control, gint64 msg_controllen)
{
	memset (hdr, 0, sizeof (struct msghdr));
	hdr->msg_control = msg_control;
	hdr->msg_controllen = msg_controllen;
}
static inline struct cmsghdr* from_offset (unsigned char* msg_control, gint64 offset)
{
	if (offset == -1)
		return NULL;
	return (struct cmsghdr*) (msg_control + offset);
}
static inline gint64 to_offset (unsigned char* msg_control, void* hdr)
{
	if (!hdr)
		return -1;
	return ((unsigned char*) hdr) - msg_control;
}

#ifdef CMSG_FIRSTHDR
gint64
Mono_Posix_Syscall_CMSG_FIRSTHDR (unsigned char* msg_control, gint64 msg_controllen)
{
	struct msghdr hdr;

	make_msghdr (&hdr, msg_control, msg_controllen);
	return to_offset (msg_control, CMSG_FIRSTHDR (&hdr));
}
#endif

#ifdef CMSG_NXTHDR
gint64
Mono_Posix_Syscall_CMSG_NXTHDR (unsigned char* msg_control, gint64 msg_controllen, gint64 cmsg)
{
	struct msghdr hdr;

	make_msghdr (&hdr, msg_control, msg_controllen);
	return to_offset (msg_control, CMSG_NXTHDR (&hdr, from_offset (msg_control, cmsg)));
}
#endif

#ifdef CMSG_DATA
gint64
Mono_Posix_Syscall_CMSG_DATA (unsigned char* msg_control, gint64 msg_controllen, gint64 cmsg)
{
	return to_offset (msg_control, CMSG_DATA (from_offset (msg_control, cmsg)));
}
#endif

#ifdef CMSG_ALIGN
guint64
Mono_Posix_Syscall_CMSG_ALIGN (guint64 length)
{
	return CMSG_ALIGN (length);	
}
#endif

#ifdef CMSG_SPACE
guint64
Mono_Posix_Syscall_CMSG_SPACE (guint64 length)
{
	return CMSG_SPACE (length);
}
#endif

#ifdef CMSG_LEN
guint64
Mono_Posix_Syscall_CMSG_LEN (guint64 length)
{
	return CMSG_LEN (length);
}
#endif

/*
 * vim: noexpandtab
 */

// vim: noexpandtab
// Local Variables: 
// tab-width: 4
// c-basic-offset: 4
// indent-tabs-mode: t
// End: 
