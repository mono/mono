/*
 * <unistd.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004-2006 Jonathan Pryor
 */

#include <config.h>

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
#if defined(_AIX)
#include <netdb.h> /* for get/setdomainname */
/*
 * Yet more stuff in libc that isn't in headers.
 * Python does the same thing we do here.
 * see: bugs.python.org/issue18259
 */
extern int sethostname(const char *, size_t);
extern int sethostid(long);
#endif

#include "mph.h" /* Don't remove or move after map.h! Works around issues with Android SDK unified headers */
#include "map.h"

G_BEGIN_DECLS

mph_off_t
Mono_Posix_Syscall_lseek (gint32 fd, mph_off_t offset, gint32 whence)
{
	mph_return_if_off_t_overflow (offset);

	return lseek (fd, offset, whence);
}

mph_ssize_t
Mono_Posix_Syscall_read (gint32 fd, void *buf, mph_size_t count)
{
	mph_return_if_size_t_overflow (count);
	return read (fd, buf, (size_t) count);
}

mph_ssize_t
Mono_Posix_Syscall_write (gint32 fd, void *buf, mph_size_t count)
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
Mono_Posix_Syscall_pwrite (gint32 fd, void *buf, mph_size_t count, mph_off_t offset)
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

void*
Mono_Posix_Syscall_getcwd (char *buf, mph_size_t size)
{
	mph_return_val_if_size_t_overflow (size, NULL);
	return getcwd (buf, (size_t) size);
}

gint64
Mono_Posix_Syscall_fpathconf (int filedes, int name, int defaultError)
{
	errno = defaultError;
	if (Mono_Posix_FromPathconfName (name, &name) == -1)
		return -1;
	return fpathconf (filedes, name);
}

gint64
Mono_Posix_Syscall_pathconf (const char *path, int name, int defaultError)
{
	errno = defaultError;
	if (Mono_Posix_FromPathconfName (name, &name) == -1)
		return -1;
	return pathconf (path, name);
}

gint64
Mono_Posix_Syscall_sysconf (int name, int defaultError)
{
	errno = defaultError;
	if (Mono_Posix_FromSysconfName (name, &name) == -1)
		return -1;
	return sysconf (name);
}

#if HAVE_CONFSTR
mph_size_t
Mono_Posix_Syscall_confstr (int name, char *buf, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);
	if (Mono_Posix_FromConfstrName (name, &name) == -1)
		return -1;
	return confstr (name, buf, (size_t) len);
}
#endif  /* def HAVE_CONFSTR */

#ifdef HAVE_TTYNAME_R
gint32
Mono_Posix_Syscall_ttyname_r (int fd, char *buf, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);
	return ttyname_r (fd, buf, (size_t) len);
}
#endif /* ndef HAVE_TTYNAME_R */

gint64
Mono_Posix_Syscall_readlink (const char *path, unsigned char *buf, mph_size_t len)
{
	gint64 r;
	mph_return_if_size_t_overflow (len);
	r = readlink (path, (char*) buf, (size_t) len);
	if (r >= 0 && r < len)
		buf [r] = '\0';
	return r;
}

#ifdef HAVE_READLINKAT
gint64
Mono_Posix_Syscall_readlinkat (int dirfd, const char *path, unsigned char *buf, mph_size_t len)
{
	gint64 r;
	mph_return_if_size_t_overflow (len);
	r = readlinkat (dirfd, path, (char*) buf, (size_t) len);
	if (r >= 0 && r < len)
		buf [r] = '\0';
	return r;
}
#endif /* def HAVE_READLINKAT */

#if HAVE_GETLOGIN_R
gint32
Mono_Posix_Syscall_getlogin_r (char *buf, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);
	return getlogin_r (buf, (size_t) len);
}
#endif  /* def HAVE_GETLOGIN_R */

gint32
Mono_Posix_Syscall_gethostname (char *buf, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);
	return gethostname (buf, (size_t) len);
}

#if HAVE_SETHOSTNAME
gint32
Mono_Posix_Syscall_sethostname (const char *name, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);
	return sethostname (name, (size_t) len);
}
#endif  /* def HAVE_SETHOSTNAME */

#if HAVE_GETHOSTID
gint64
Mono_Posix_Syscall_gethostid (void)
{
	return gethostid ();
}
#endif  /* def HAVE_GETHOSTID */

#ifdef HAVE_SETHOSTID
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
#endif /* def HAVE_SETHOSTID */

#ifdef HAVE_GETDOMAINNAME
gint32
Mono_Posix_Syscall_getdomainname (char *name, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);
	return getdomainname (name, (size_t) len);
}
#endif /* def HAVE_GETDOMAINNAME */

#ifdef HAVE_SETDOMAINNAME
gint32
Mono_Posix_Syscall_setdomainname (const char *name, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);
	return setdomainname (name, (size_t) len);
}
#endif /* def HAVE_SETDOMAINNAME */

/* Android implements truncate, but doesn't declare it.
 * Result is a warning during compilation, so skip it.
 */
#ifndef HOST_ANDROID
gint32
Mono_Posix_Syscall_truncate (const char *path, mph_off_t length)
{
	mph_return_if_off_t_overflow (length);
	return truncate (path, (off_t) length);
}
#endif

gint32
Mono_Posix_Syscall_ftruncate (int fd, mph_off_t length)
{
	mph_return_if_off_t_overflow (length);
	return ftruncate (fd, (off_t) length);
}

#if HAVE_LOCKF
gint32
Mono_Posix_Syscall_lockf (int fd, int cmd, mph_off_t len)
{
	mph_return_if_off_t_overflow (len);
	if (Mono_Posix_FromLockfCommand (cmd, &cmd) == -1)
		return -1;
	return lockf (fd, cmd, (off_t) len);
}
#endif  /* def HAVE_LOCKF */

#if HAVE_SWAB
int
Mono_Posix_Syscall_swab (void *from, void *to, mph_ssize_t n)
{
	if (mph_have_long_overflow (n))
		return -1;
	swab (from, to, (ssize_t) n);
	return 0;
}
#endif  /* def HAVE_SWAB */

#if HAVE_SETUSERSHELL
int
Mono_Posix_Syscall_setusershell (void)
{
	setusershell ();
	return 0;
}
#endif  /* def HAVE_SETUSERSHELL */

#if HAVE_ENDUSERSHELL
int
Mono_Posix_Syscall_endusershell (void)
{
	endusershell ();
	return 0;
}
#endif  /* def HAVE_ENDUSERSHELL */

int
Mono_Posix_Syscall_sync (void)
{
	sync ();
	return 0;
}


G_END_DECLS

/*
 * vim: noexpandtab
 */
