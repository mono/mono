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

#include "mph.h"
#include <glib/gtypes.h>

G_BEGIN_DECLS

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
