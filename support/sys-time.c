/*
 * <sys/stat.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004-2006 Jonathan Pryor
 */

#include <sys/types.h>
#include <sys/time.h>
#include <string.h>
#ifdef __HAIKU__
#include <os/kernel/OS.h>
#endif

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

gint32
Mono_Posix_Syscall_gettimeofday (
	struct Mono_Posix_Timeval *tv,
	void *tz)
{
	struct timeval _tv;
	struct timezone _tz;
	int r;

	r = gettimeofday (&_tv, &_tz);

	if (r == 0) {
		if (tv) {
			tv->tv_sec  = _tv.tv_sec;
			tv->tv_usec = _tv.tv_usec;
		}
		if (tz) {
			struct Mono_Posix_Timezone *tz_ = (struct Mono_Posix_Timezone *) tz;
			tz_->tz_minuteswest = _tz.tz_minuteswest;
			tz_->tz_dsttime     = 0;
		}
	}

	return r;
}

gint32
Mono_Posix_Syscall_settimeofday (
	struct Mono_Posix_Timeval *tv,
	struct Mono_Posix_Timezone *tz)
{
	struct timeval _tv   = {0};
	struct timeval *ptv  = NULL;
	struct timezone _tz  = {0};
	struct timezone *ptz = NULL;
	int r;

	if (tv) {
		_tv.tv_sec  = tv->tv_sec;
		_tv.tv_usec = tv->tv_usec;
		ptv = &_tv;
	}
	if (tz) {
		_tz.tz_minuteswest = tz->tz_minuteswest;
		_tz.tz_dsttime = 0;
		ptz = &_tz;
	}

#ifdef __HAIKU__
	set_real_time_clock(ptv->tv_sec);
	r = 0;
#else
	r = settimeofday (ptv, ptz);
#endif

	return r;
}

static struct timeval*
copy_utimes (struct timeval* to, struct Mono_Posix_Timeval *from)
{
	if (from) {
		to[0].tv_sec  = from[0].tv_sec;
		to[0].tv_usec = from[0].tv_usec;
		to[1].tv_sec  = from[1].tv_sec;
		to[1].tv_usec = from[1].tv_usec;
		return to;
	}

	return NULL;
}

gint32
Mono_Posix_Syscall_utimes(const char *filename, struct Mono_Posix_Timeval *tv)
{
	struct timeval _tv[2];
	struct timeval *ptv;

	ptv = copy_utimes (_tv, tv);

	return utimes (filename, ptv);
}

#ifdef HAVE_LUTIMES
gint32
Mono_Posix_Syscall_lutimes(const char *filename, struct Mono_Posix_Timeval *tv)
{
	struct timeval _tv[2];
	struct timeval *ptv;

	ptv = copy_utimes (_tv, tv);

	return lutimes (filename, ptv);
}
#endif /* def HAVE_LUTIMES */

#if HAVE_FUTIMES
gint32
Mono_Posix_Syscall_futimes(int fd, struct Mono_Posix_Timeval *tv)
{
	struct timeval _tv[2];
	struct timeval *ptv;

	ptv = copy_utimes (_tv, tv);

	return futimes (fd, ptv);
}
#endif  /* def HAVE_FUTIMES */

G_END_DECLS

/*
 * vim: noexpandtab
 */
