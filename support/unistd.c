/*
 * <unistd.h> wrapper functions.
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
#include <limits.h>
#include <string.h>     /* for swab(3) on Mac OS X */

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

mph_off_t
Mono_Posix_Syscall_lseek (gint32 fd, mph_off_t offset, gint32 whence)
{
	short _whence;
	mph_return_if_off_t_overflow (offset);
	if (Mono_Posix_FromSeekFlags (whence, &_whence) == -1)
		return -1;
	whence = _whence;

	return lseek (fd, offset, whence);
}

mph_ssize_t
Mono_Posix_Syscall_read (gint32 fd, void *buf, mph_size_t count)
{
	mph_return_if_size_t_overflow (count);
	return read (fd, buf, (size_t) count);
}

mph_ssize_t
Mono_Posix_Syscall_write (gint32 fd, const void *buf, mph_size_t count)
{
	mph_return_if_size_t_overflow (count);
	return write (fd, buf, (size_t) count);
}

mph_ssize_t
Mono_Posix_Syscall_pread (gint32 fd, void *buf, mph_size_t count, mph_off_t offset)
{
	mph_return_if_size_t_overflow (count);
	mph_return_if_off_t_overflow (offset);

	return pread (fd, buf, (size_t) count, (off_t) offset);
}

mph_ssize_t
Mono_Posix_Syscall_pwrite (gint32 fd, const void *buf, mph_size_t count, mph_off_t offset)
{
	mph_return_if_size_t_overflow (count);
	mph_return_if_off_t_overflow (offset);

	return pwrite (fd, buf, (size_t) count, (off_t) offset);
}

gint32
Mono_Posix_Syscall_pipe (gint32 *reading, gint32 *writing)
{
	int filedes[2] = {-1, -1};
	int r;

	if (reading == NULL || writing == NULL) {
		errno = EFAULT;
		return -1;
	}

	r = pipe (filedes);

	*reading = filedes[0];
	*writing = filedes[1];
	return r;
}

char*
Mono_Posix_Syscall_getcwd (char *buf, mph_size_t size)
{
	mph_return_val_if_size_t_overflow (size, NULL);
	return getcwd (buf, (size_t) size);
}

gint64
Mono_Posix_Syscall_fpathconf (int filedes, int name)
{
	if (Mono_Posix_FromPathConf (name, &name) == -1)
		return -1;
	return fpathconf (filedes, name);
}

gint64
Mono_Posix_Syscall_pathconf (char *path, int name)
{
	if (Mono_Posix_FromPathConf (name, &name) == -1)
		return -1;
	return pathconf (path, name);
}

gint64
Mono_Posix_Syscall_sysconf (int name)
{
	if (Mono_Posix_FromSysConf (name, &name) == -1)
		return -1;
	return sysconf (name);
}

gint64
Mono_Posix_Syscall_confstr (int name, char *buf, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);
	if (Mono_Posix_FromConfStr (name, &name) == -1)
		return -1;
	return confstr (name, buf, (size_t) len);
}

#ifdef HAVE_TTYNAME_R
gint32
Mono_Posix_Syscall_ttyname_r (int fd, char *buf, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);
	return ttyname_r (fd, buf, (size_t) len);
}
#endif /* ndef HAVE_TTYNAME_R */

gint32
Mono_Posix_Syscall_readlink (const char *path, char *buf, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);
	return readlink (path, buf, (size_t) len);
}

gint32
Mono_Posix_Syscall_getlogin_r (char *buf, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);
	return getlogin_r (buf, (size_t) len);
}

gint32
Mono_Posix_Syscall_gethostname (char *buf, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);
	return gethostname (buf, (size_t) len);
}

gint32
Mono_Posix_Syscall_sethostname (const char *name, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);
	return sethostname (name, (size_t) len);
}

gint64
Mono_Posix_Syscall_gethostid (void)
{
	return gethostid ();
}

gint32
Mono_Posix_Syscall_sethostid (gint64 hostid)
{
	mph_return_if_long_overflow (hostid);
#ifdef MPH_ON_BSD
	sethostid ((long) hostid);
	return 0;
#else
	return sethostid ((long) hostid);
#endif
}

gint32
Mono_Posix_Syscall_getdomainname (char *name, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);
	return getdomainname (name, (size_t) len);
}

gint32
Mono_Posix_Syscall_setdomainname (const char *name, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);
	return setdomainname (name, (size_t) len);
}

gint32
Mono_Posix_Syscall_truncate (const char *path, mph_off_t length)
{
	mph_return_if_off_t_overflow (length);
	return truncate (path, (off_t) length);
}

gint32
Mono_Posix_Syscall_ftruncate (int fd, mph_off_t length)
{
	mph_return_if_off_t_overflow (length);
	return ftruncate (fd, (off_t) length);
}

gint32
Mono_Posix_Syscall_lockf (int fd, int cmd, mph_off_t len)
{
	mph_return_if_off_t_overflow (len);
	if (Mono_Posix_FromLockFlags (cmd, &cmd) == -1)
		return -1;
	return lockf (fd, cmd, (off_t) len);
}

void
Mono_Posix_Syscall_swab (void *from, void *to, mph_ssize_t n)
{
	if (mph_have_long_overflow (n))
		return;
	swab (from, to, (ssize_t) n);
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
