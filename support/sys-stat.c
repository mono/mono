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
#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif
#include <fcntl.h>
#include <errno.h>

#include "mph.h" /* Don't remove or move after map.h! Works around issues with Android SDK unified headers */
#include "map.h"

G_BEGIN_DECLS

int
Mono_Posix_FromStat (struct Mono_Posix_Stat *from, void *_to)
{
	struct stat *to = _to;
	memset (to, 0, sizeof(*to));

	to->st_dev         = from->st_dev;
	to->st_ino         = from->st_ino;

	unsigned int to_st_mode;
	if (Mono_Posix_FromFilePermissions (from->st_mode, &to_st_mode) != 0) {
		return -1;
	}

	to->st_mode        = to_st_mode;
	to->st_nlink       = from->st_nlink;
	to->st_uid         = from->st_uid;
	to->st_gid         = from->st_gid;
	to->st_rdev        = from->st_rdev;
	to->st_size        = from->st_size;
#ifndef HOST_WIN32
	to->st_blksize     = from->st_blksize;
	to->st_blocks      = from->st_blocks;
#endif
	to->st_atime       = from->st_atime_;
	to->st_mtime       = from->st_mtime_;
	to->st_ctime       = from->st_ctime_;
#if HAVE_STRUCT_STAT_ST_ATIMESPEC
	to->st_atimespec.tv_sec = from->st_atime_;
	to->st_atimespec.tv_nsec = from->st_atime_nsec;
	to->st_mtimespec.tv_sec = from->st_mtime_;
	to->st_mtimespec.tv_nsec = from->st_mtime_nsec;
	to->st_ctimespec.tv_sec = from->st_ctime_;
	to->st_ctimespec.tv_nsec = from->st_ctime_nsec;
#else
#    ifdef HAVE_STRUCT_STAT_ST_ATIM
	to->st_atim.tv_nsec = from->st_atime_nsec;
#    endif
#    ifdef HAVE_STRUCT_STAT_ST_MTIM
	to->st_mtim.tv_nsec = from->st_mtime_nsec;
#    endif
#    ifdef HAVE_STRUCT_STAT_ST_CTIM
	to->st_ctim.tv_nsec = from->st_ctime_nsec;
#    endif
#endif
	return 0;
}

int
Mono_Posix_ToStat (void *_from, struct Mono_Posix_Stat *to)
{
	struct stat *from = _from;
	memset (to, 0, sizeof(*to));

	to->st_dev        = from->st_dev;
	to->st_ino        = from->st_ino;
	if (Mono_Posix_ToFilePermissions (from->st_mode, &to->st_mode) != 0) {
		return -1;
	}
	to->st_nlink      = from->st_nlink;
	to->st_uid        = from->st_uid;
	to->st_gid        = from->st_gid;
	to->st_rdev       = from->st_rdev;
	to->st_size       = from->st_size;
#ifndef HOST_WIN32
	to->st_blksize    = from->st_blksize;
	to->st_blocks     = from->st_blocks;
#endif
	to->st_atime_     = from->st_atime;
	to->st_mtime_     = from->st_mtime;
	to->st_ctime_     = from->st_ctime;
#if HAVE_STRUCT_STAT_ST_ATIMESPEC
	to->st_atime_nsec = from->st_atimespec.tv_nsec;
	to->st_mtime_nsec = from->st_mtimespec.tv_nsec;
	to->st_ctime_nsec = from->st_ctimespec.tv_nsec;
#else
#    ifdef HAVE_STRUCT_STAT_ST_ATIM
	to->st_atime_nsec = from->st_atim.tv_nsec;
#    endif
#    ifdef HAVE_STRUCT_STAT_ST_MTIM
	to->st_mtime_nsec = from->st_mtim.tv_nsec;
#    endif
#    ifdef HAVE_STRUCT_STAT_ST_CTIM
	to->st_ctime_nsec = from->st_ctim.tv_nsec;
#    endif
#endif
	return 0;
}

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

#ifndef HOST_WIN32
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
#endif

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

#ifndef HOST_WIN32
gint32
Mono_Posix_Syscall_mknod (const char *pathname, guint32 mode, mph_dev_t dev)
{
	if (Mono_Posix_FromFilePermissions (mode, &mode) == -1)
		return -1;
	return mknod (pathname, mode, dev);
}
#endif

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

#if defined(HAVE_FUTIMENS) || defined(HAVE_UTIMENSAT)
static struct timespec*
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
#endif

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
