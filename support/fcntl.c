/*
 * <fcntl.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#ifndef _GNU_SOURCE
#define _GNU_SOURCE
#endif

#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <fcntl.h>
#include <errno.h>

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

struct Mono_Posix_Flock {
	gint16    l_type;
	gint16    l_whence;
	mph_off_t l_start;
	mph_off_t l_len;
	mph_pid_t l_pid;
};

gint32
Mono_Posix_Syscall_fcntl (gint32 fd, gint32 cmd)
{
	if (Mono_Posix_FromFcntlCommand (cmd, &cmd) == -1)
		return -1;
	return fcntl (fd, cmd);
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
#ifdef MPH_USE_64_API
	struct flock64 _lock;
#else
	struct flock _lock;
#endif
	int r;

	if (lock == NULL) {
		errno = EFAULT;
		return -1;
	}

	mph_return_if_off_t_overflow (lock->l_start);
	mph_return_if_off_t_overflow (lock->l_len);

	if (Mono_Posix_FromLockType (lock->l_type, &lock->l_type) == -1)
		return -1;
	_lock.l_type   = lock->l_type;
	_lock.l_whence = lock->l_whence;
	_lock.l_start  = lock->l_start;
	_lock.l_len    = lock->l_len;
	_lock.l_pid    = lock->l_pid;

	r = fcntl (fd, cmd, &_lock);

	if (Mono_Posix_ToLockType (_lock.l_type, &_lock.l_type) == -1)
		r = -1;
	lock->l_type   = _lock.l_type;
	lock->l_whence = _lock.l_whence;
	lock->l_start  = _lock.l_start;
	lock->l_len    = _lock.l_len;
	lock->l_pid    = _lock.l_pid;

	return r;
}

gint32
Mono_Posix_Syscall_open (const char *pathname, gint32 flags)
{
	if (Mono_Posix_FromOpenFlags (flags, &flags) == -1)
		return -1;
#ifdef MPH_USE_64_API
	return open64 (pathname, flags);
#else
	return open (pathname, flags);
#endif
}

gint32
Mono_Posix_Syscall_open_mode (const char *pathname, gint32 flags, guint32 mode)
{
	if (Mono_Posix_FromOpenFlags (flags, &flags) == -1)
		return -1;
	if (Mono_Posix_FromFilePermissions (mode, &mode) == -1)
		return -1;
#ifdef MPH_USE_64_API
	return open64 (pathname, flags, mode);
#else
	return open (pathname, flags, mode);
#endif
}

gint32
Mono_Posix_Syscall_creat (const char *pathname, guint32 mode)
{
	if (Mono_Posix_FromFilePermissions (mode, &mode) == -1)
		return -1;
#ifdef MPH_USE_64_API
	return creat64 (pathname, mode);
#else
	return creat (pathname, mode);
#endif
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

#ifdef MPH_USE_64_API
	return posix_fadvise64 (fd, offset, len, advice);
#else
	return posix_fadvise (fd, (off_t) offset, (off_t) len, advice);
#endif
}
#endif /* ndef HAVE_POSIX_FADVISE */

#ifdef HAVE_POSIX_FALLOCATE
gint32
Mono_Posix_Syscall_posix_fallocate (gint32 fd, mph_off_t offset, mph_size_t len)
{
	mph_return_if_off_t_overflow (offset);
	mph_return_if_size_t_overflow (len);

#ifdef MPH_USE_64_API
	return posix_fallocate64 (fd, offset, len);
#else
	return posix_fadvise (fd, (off_t) offset, (size_t) len);
#endif
}
#endif /* ndef HAVE_POSIX_FALLOCATE */

G_END_DECLS

/*
 * vim: noexpandtab
 */
