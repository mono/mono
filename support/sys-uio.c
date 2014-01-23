/*
 * <sys/uio.h> wrapper functions.
 *
 * Authors:
 *   Steffen Kiess (s-kiess@web.de)
 *
 * Copyright (C) 2012 Steffen Kiess
 */

#ifndef _GNU_SOURCE
#define _GNU_SOURCE
#endif /* ndef _GNU_SOURCE */

#include <sys/uio.h>

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

static struct iovec*
from_iovec (struct Mono_Posix_Iovec *iov, gint32 iovcnt)
{
	struct iovec* v;
	gint32 i;

	if (iovcnt < 0) {
		errno = EINVAL;
		return NULL;
	}

	v = malloc (iovcnt * sizeof (struct iovec));
	if (!v) {
		return NULL;
	}

	for (i = 0; i < iovcnt; i++) {
		if (Mono_Posix_FromIovec (&iov[i], &v[i]) != 0) {
			free (v);
			return NULL;
		}
	}

	return v;
}

#ifdef HAVE_READV
gint64
Mono_Posix_Syscall_readv (int dirfd, struct Mono_Posix_Iovec *iov, gint32 iovcnt)
{
	struct iovec* v;
	gint64 res;

	v = from_iovec (iov, iovcnt);
	if (!v) {
		return -1;
	}

	res = readv(dirfd, v, iovcnt);
	free (v);
	return res;
}
#endif /* def HAVE_READV */

#ifdef HAVE_WRITEV
gint64
Mono_Posix_Syscall_writev (int dirfd, struct Mono_Posix_Iovec *iov, gint32 iovcnt)
{
	struct iovec* v;
	gint64 res;

	v = from_iovec (iov, iovcnt);
	if (!v) {
		return -1;
	}

	res = writev (dirfd, v, iovcnt);
	free (v);
	return res;
}
#endif /* def HAVE_WRITEV */

#ifdef HAVE_PREADV
gint64
Mono_Posix_Syscall_preadv (int dirfd, struct Mono_Posix_Iovec *iov, gint32 iovcnt, gint64 off)
{
	struct iovec* v;
	gint64 res;

	mph_return_if_off_t_overflow (off);

	v = from_iovec (iov, iovcnt);
	if (!v) {
		return -1;
	}

	res = preadv (dirfd, v, iovcnt, (off_t) off);
	free (v);
	return res;
}
#endif /* def HAVE_PREADV */

#ifdef HAVE_PWRITEV
gint64
Mono_Posix_Syscall_pwritev (int dirfd, struct Mono_Posix_Iovec *iov, gint32 iovcnt, gint64 off)
{
	struct iovec* v;
	gint64 res;

	mph_return_if_off_t_overflow (off);

	v = from_iovec (iov, iovcnt);
	if (!v) {
		return -1;
	}

	res = pwritev (dirfd, v, iovcnt, (off_t) off);
	free (v);
	return res;
}
#endif /* def HAVE_PWRITEV */


/*
 * vim: noexpandtab
 */
