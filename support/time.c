/*
 * <time.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#define _SVID_SOURCE
#include <time.h>
#include <errno.h>

#include "map.h"
#include "mph.h"
#include <glib.h>

G_BEGIN_DECLS

#if defined(HAVE_STRUCT_TIMESPEC) && _POSIX_C_SOURCE >= 199309L
int
Mono_Posix_Syscall_nanosleep (struct Mono_Posix_Timespec *req,
		struct Mono_Posix_Timespec *rem)
{
	struct timespec _req, _rem, *prem = NULL;
	int r;

	if (req == NULL) {
		errno = EFAULT;
		return -1;
	}

	if (Mono_Posix_FromTimespec (req, &_req) == -1)
		return -1;

	if (rem) {
		if (Mono_Posix_FromTimespec (rem, &_rem) == -1)
			return -1;
		prem = &_rem;
	}

	r = nanosleep (&_req, prem);

	if (rem && Mono_Posix_ToTimespec (prem, rem) == -1)
		return -1;

	return r;
}
#endif

#ifdef HAVE_STIME
/* AIX has stime in libc, but not at all in headers, so declare here */
#if defined(_AIX)
extern int stime(time_t);
#endif

gint32
Mono_Posix_Syscall_stime (mph_time_t *t)
{
	time_t _t;
	if (t == NULL) {
		errno = EFAULT;
		return -1;
	}
	mph_return_if_time_t_overflow (*t);
	_t = (time_t) *t;
	return stime (&_t);
}
#endif /* ndef HAVE_STIME */

mph_time_t
Mono_Posix_Syscall_time (mph_time_t *t)
{
	time_t _t, r;
	if (t == NULL) {
		errno = EFAULT;
		return -1;
	}

	mph_return_if_time_t_overflow (*t);

	_t = (time_t) *t;
	r = time (&_t);
	*t = _t;

	return r;
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
