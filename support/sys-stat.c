/*
 * <sys/stat.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
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

struct Mono_Posix_Syscall_Stat {
	/* dev_t */     mph_dev_t     st_dev;     /* device */
	/* ino_t */     mph_ino_t     st_ino;     /* inode */
	/* mode_t */    guint32       st_mode;    /* protection */
	                guint32       _padding_;  /* structure padding */
	/* nlink_t */   mph_nlink_t   st_nlink;   /* number of hard links */
	/* uid_t */     mph_uid_t     st_uid;     /* user ID of owner */
	/* gid_t */     mph_gid_t     st_gid;     /* group ID of owner */
	/* dev_t */     mph_dev_t     st_rdev;    /* device type (if inode device) */
	/* off_t */     mph_off_t     st_size;    /* total size, in bytes */
	/* blksize_t */ mph_blksize_t st_blksize; /* blocksize for filesystem I/O */
	/* blkcnt_t */  mph_blkcnt_t  st_blocks;  /* number of blocks allocated */

	/* st_atime, st_mtime, and st_ctime are macros (!), so use a slightly
	 * different name to appease CPP */

	/* time_t */    mph_time_t    st_atime_;  /* time of last access */
	/* time_t */    mph_time_t    st_mtime_;  /* time of last modification */
	/* time_t */    mph_time_t    st_ctime_;  /* time of last status change */
};

static int
copy_stat (struct Mono_Posix_Syscall_Stat *to, struct stat *from)
{
	if (Mono_Posix_ToFilePermissions (from->st_mode, &to->st_mode) == -1)
		return -1;
	to->st_dev      = from->st_dev;
	to->st_ino      = from->st_ino;
	to->st_nlink    = from->st_nlink;
	to->st_uid      = from->st_uid;
	to->st_gid      = from->st_gid;
	to->st_rdev     = from->st_rdev;
	to->st_size     = from->st_size;
	to->st_blksize  = from->st_blksize;
	to->st_blocks   = from->st_blocks;
	to->st_atime_   = from->st_atime;
	to->st_mtime_   = from->st_mtime;
	to->st_ctime_   = from->st_ctime;
	return 0;
}

gint32
Mono_Posix_Syscall_stat (const char *file_name, struct Mono_Posix_Syscall_Stat *buf)
{
	int r;
	struct stat _buf;

	if (buf == NULL) {
		errno = EFAULT;
		return -1;
	}
	r = stat (file_name, &_buf);
	if (r != -1 && copy_stat (buf, &_buf) == -1)
		r = -1;
	return r;
}

gint32
Mono_Posix_Syscall_fstat (int filedes, struct Mono_Posix_Syscall_Stat *buf)
{
	int r;
	struct stat _buf;

	if (buf == NULL) {
		errno = EFAULT;
		return -1;
	}
	r = fstat (filedes, &_buf);
	if (r != -1 && copy_stat (buf, &_buf) == -1)
		r = -1;
	return r;
}

gint32
Mono_Posix_Syscall_lstat (const char *file_name, struct Mono_Posix_Syscall_Stat *buf)
{
	int r;
	struct stat _buf;

	if (buf == NULL) {
		errno = EFAULT;
		return -1;
	}
	r = lstat (file_name, &_buf);
	if (r != -1 && copy_stat (buf, &_buf) == -1)
		r = -1;
	return r;
}

gint32
Mono_Posix_Syscall_mknod (const char *pathname, guint32 mode, mph_dev_t dev)
{
	if (Mono_Posix_FromFilePermissions (mode, &mode) == -1)
		return -1;
	return mknod (pathname, mode, dev);
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
