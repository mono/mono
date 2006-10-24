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
