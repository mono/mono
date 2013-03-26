/*
 * <sys/stat.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004-2006 Jonathan Pryor
 */

#ifndef _GNU_SOURCE
#define _GNU_SOURCE
#endif /* ndef _GNU_SOURCE */

#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <fcntl.h>
#include <errno.h>

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

gint32
Mono_Posix_Syscall_stat (const char *file_name, struct Mono_Posix_Stat *buf)
{
	int r;
	struct stat _buf;

	if (buf == NULL) {
		errno = EFAULT;
		return -1;
	}
	r = stat (file_name, &_buf);
	if (r != -1 && Mono_Posix_ToStat (&_buf, buf) == -1)
		r = -1;
	return r;
}

gint32
Mono_Posix_Syscall_fstat (int filedes, struct Mono_Posix_Stat *buf)
{
	int r;
	struct stat _buf;

	if (buf == NULL) {
		errno = EFAULT;
		return -1;
	}
	r = fstat (filedes, &_buf);
	if (r != -1 && Mono_Posix_ToStat (&_buf, buf) == -1)
		r = -1;
	return r;
}

gint32
Mono_Posix_Syscall_lstat (const char *file_name, struct Mono_Posix_Stat *buf)
{
	int r;
	struct stat _buf;

	if (buf == NULL) {
		errno = EFAULT;
		return -1;
	}
	r = lstat (file_name, &_buf);
	if (r != -1 && Mono_Posix_ToStat (&_buf, buf) == -1)
		r = -1;
	return r;
}

#ifdef HAVE_FSTATAT
gint32
Mono_Posix_Syscall_fstatat (gint32 dirfd, const char *file_name, struct Mono_Posix_Stat *buf, gint32 flags)
{
	int r;
	struct stat _buf;

	if (Mono_Posix_FromAtFlags (flags, &flags) == -1)
		return -1;

	if (buf == NULL) {
		errno = EFAULT;
		return -1;
	}
	r = fstatat (dirfd, file_name, &_buf, flags);
	if (r != -1 && Mono_Posix_ToStat (&_buf, buf) == -1)
		r = -1;
	return r;
}
#endif

gint32
Mono_Posix_Syscall_mknod (const char *pathname, guint32 mode, mph_dev_t dev)
{
	if (Mono_Posix_FromFilePermissions (mode, &mode) == -1)
		return -1;
	return mknod (pathname, mode, dev);
}

#ifdef HAVE_MKNODAT
gint32
Mono_Posix_Syscall_mknodat (int dirfd, const char *pathname, guint32 mode, mph_dev_t dev)
{
	if (Mono_Posix_FromFilePermissions (mode, &mode) == -1)
		return -1;
	return mknodat (dirfd, pathname, mode, dev);
}
#endif

G_END_DECLS

gint64
Mono_Posix_Syscall_get_utime_now ()
{
#ifdef UTIME_NOW
	return UTIME_NOW;
#else
	return -1;
#endif
}

gint64
Mono_Posix_Syscall_get_utime_omit ()
{
#ifdef UTIME_OMIT
	return UTIME_OMIT;
#else
	return -1;
#endif
}

static inline struct timespec*
copy_utimens (struct timespec* to, struct Mono_Posix_Timespec *from)
{
	if (from) {
		to[0].tv_sec  = from[0].tv_sec;
		to[0].tv_nsec = from[0].tv_nsec;
		to[1].tv_sec  = from[1].tv_sec;
		to[1].tv_nsec = from[1].tv_nsec;
		return to;
	}

	return NULL;
}

#ifdef HAVE_FUTIMENS
gint32
Mono_Posix_Syscall_futimens(int fd, struct Mono_Posix_Timespec *tv)
{
	struct timespec _tv[2];
	struct timespec *ptv;

	ptv = copy_utimens (_tv, tv);

	return futimens (fd, ptv);
}
#endif /* def HAVE_FUTIMENS */

#ifdef HAVE_UTIMENSAT
gint32
Mono_Posix_Syscall_utimensat(int dirfd, const char *pathname, struct Mono_Posix_Timespec *tv, int flags)
{
	struct timespec _tv[2];
	struct timespec *ptv;

	ptv = copy_utimens (_tv, tv);

	return utimensat (dirfd, pathname, ptv, flags);
}
#endif /* def HAVE_UTIMENSAT */


/*
 * vim: noexpandtab
 */
