/*
 * <sys/socket.h> wrapper functions.
 *
 * Authors:
 *   Steffen Kiess (s-kiess@web.de)
 *
 * Copyright (C) 2015 Steffen Kiess
 */

#include <sys/socket.h>
#include <sys/time.h>
#include <netinet/in.h>
#include <sys/un.h>

#include <stddef.h>

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

int
Mono_Posix_Syscall_socketpair (int domain, int type, int protocol, int* socket1, int* socket2)
{
	int filedes[2] = {-1, -1};
	int r;

	r = socketpair (domain, type, protocol, filedes);

	*socket1 = filedes[0];
	*socket2 = filedes[1];
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
