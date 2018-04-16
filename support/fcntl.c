/*
 * <fcntl.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004, 2006 Jonathan Pryor
 */

#ifndef _GNU_SOURCE
#define _GNU_SOURCE
#endif

#include <sys/types.h>
#include <sys/stat.h>
#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif
#include <fcntl.h>
#include <errno.h>
#ifdef HOST_WIN32
#include <corecrt_io.h>
#endif

#include "mph.h" /* Don't remove or move after map.h! Works around issues with Android SDK unified headers */
#include "map.h"

G_BEGIN_DECLS

#ifndef HOST_WIN32
gint32
Mono_Posix_Syscall_fcntl (gint32 fd, gint32 cmd)
{
	if (Mono_Posix_FromFcntlCommand (cmd, &cmd) == -1)
		return -1;
	return fcntl (fd, cmd);
}

gint32
Mono_Posix_Syscall_fcntl_arg_int (gint32 fd, gint32 cmd, int arg)
{
	if (Mono_Posix_FromFcntlCommand (cmd, &cmd) == -1)
		return -1;
	return fcntl (fd, cmd, arg);
}

gint32
Mono_Posix_Syscall_fcntl_arg_ptr (gint32 fd, gint32 cmd, void *arg)
{
	if (Mono_Posix_FromFcntlCommand (cmd, &cmd) == -1)
		return -1;
	return fcntl (fd, cmd, arg);
}

gint32
Mono_Posix_Syscall_fcntl_arg (gint32 fd, gint32 cmd, gint64 arg)
{
	long _arg;
	gint32 _cmd;

	mph_return_if_long_overflow (arg);

#ifdef F_NOTIFY
	if (cmd == F_NOTIFY) {
		int _argi;
		if (Mono_Posix_FromDirectoryNotifyFlags (arg, &_argi) == -1) {
			return -1;
		}
		_arg = _argi;
	}
	else
#endif
		_arg = (long) arg;

	if (Mono_Posix_FromFcntlCommand (cmd, &_cmd) == -1)
		return -1;
	return fcntl (fd, cmd, _arg);
}

gint32
Mono_Posix_Syscall_fcntl_lock (gint32 fd, gint32 cmd, struct Mono_Posix_Flock *lock)
{
	struct flock _lock;
	int r;

	if (lock == NULL) {
		errno = EFAULT;
		return -1;
	}

	if (Mono_Posix_FromFlock (lock, &_lock) == -1)
		return -1;

	if (Mono_Posix_FromFcntlCommand (cmd, &cmd) == -1)
		return -1;

	r = fcntl (fd, cmd, &_lock);

	if (Mono_Posix_ToFlock (&_lock, lock) == -1)
		return -1;

	return r;
}
#endif

gint32
Mono_Posix_Syscall_open (const char *pathname, gint32 flags)
{
	if (Mono_Posix_FromOpenFlags (flags, &flags) == -1)
		return -1;

	return open (pathname, flags);
}

gint32
Mono_Posix_Syscall_open_mode (const char *pathname, gint32 flags, guint32 mode)
{
	if (Mono_Posix_FromOpenFlags (flags, &flags) == -1)
		return -1;
	if (Mono_Posix_FromFilePermissions (mode, &mode) == -1)
		return -1;

	return open (pathname, flags, mode);
}

gint32
Mono_Posix_Syscall_get_at_fdcwd ()
{
#ifdef AT_FDCWD
	return AT_FDCWD;
#else
	return -1;
#endif
}

gint32
Mono_Posix_Syscall_creat (const char *pathname, guint32 mode)
{
	if (Mono_Posix_FromFilePermissions (mode, &mode) == -1)
		return -1;

	return creat (pathname, mode);
}

#ifdef HAVE_POSIX_FADVISE
gint32
Mono_Posix_Syscall_posix_fadvise (gint32 fd, mph_off_t offset, mph_off_t len, 
	gint32 advice)
{
	mph_return_if_off_t_overflow (offset);
	mph_return_if_off_t_overflow (len);

	if (Mono_Posix_FromPosixFadviseAdvice (advice, &advice) == -1)
		return -1;

	return posix_fadvise (fd, (off_t) offset, (off_t) len, advice);
}
#endif /* ndef HAVE_POSIX_FADVISE */

#ifdef HAVE_POSIX_FALLOCATE
gint32
Mono_Posix_Syscall_posix_fallocate (gint32 fd, mph_off_t offset, mph_size_t len)
{
	mph_return_if_off_t_overflow (offset);
	mph_return_if_size_t_overflow (len);

	return posix_fallocate (fd, (off_t) offset, (size_t) len);
}
#endif /* ndef HAVE_POSIX_FALLOCATE */

G_END_DECLS

/*
 * vim: noexpandtab
 */
