/*
 * <sys/stat.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#include <sys/types.h>
#include <sys/time.h>
#include <string.h>

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

struct Mono_Posix_Syscall_Timeval {
	/* time_t */      mph_time_t  tv_sec;   /* seconds */
	/* suseconds_t */ gint64      tv_usec;  /* microseconds */
};

struct Mono_Posix_Syscall_Timezone {
	int tz_minuteswest;  /* minutes W of Greenwich */
	int tz_dsttime;      /* ignored */
};

gint32
Mono_Posix_Syscall_gettimeofday (
	struct Mono_Posix_Syscall_Timeval *tv,
	struct Mono_Posix_Syscall_Timezone *tz)
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
			tz->tz_minuteswest = _tz.tz_minuteswest;
			tz->tz_dsttime     = 0;
		}
	}

	return r;
}

gint32
Mono_Posix_Syscall_settimeofday (
	const struct Mono_Posix_Syscall_Timeval *tv,
	const struct Mono_Posix_Syscall_Timezone *tz)
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

	r = settimeofday (ptv, ptz);

	return r;
}

gint32
Mono_Posix_Syscall_utimes (const char *filename,
	struct Mono_Posix_Syscall_Timeval *tv)
{
	struct timeval _tv;
	struct timeval *ptv = NULL;

	if (tv) {
		_tv.tv_sec  = tv->tv_sec;
		_tv.tv_usec = tv->tv_usec;
		ptv = &_tv;
	}

	return utimes (filename, ptv);
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
