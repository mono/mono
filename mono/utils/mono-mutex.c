/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/*
 * mono-mutex.h: Portability wrappers around POSIX Mutexes
 *
 * Authors: Jeffrey Stedfast <fejj@ximian.com>
 *
 * Copyright 2002 Ximian, Inc. (www.ximian.com)
 */


#include <config.h>
#include <glib.h>

#include <stdlib.h>
#include <string.h>
#include <time.h>

#ifdef HAVE_SYS_TIME_H
#include <sys/time.h>
#endif

#include "mono-mutex.h"
#include "mono-lazy-init.h"

#if !defined(HOST_WIN32)

#include <errno.h>

int
mono_mutex_init (mono_mutex_t *mutex)
{
	return pthread_mutex_init (mutex, NULL);
}

int
mono_mutex_init_recursive (mono_mutex_t *mutex)
{
	int res;
	pthread_mutexattr_t attr;

	pthread_mutexattr_init (&attr);
	pthread_mutexattr_settype (&attr, PTHREAD_MUTEX_RECURSIVE);
	res = pthread_mutex_init (mutex, &attr);
	pthread_mutexattr_destroy (&attr);

	return res;
}

int
mono_mutex_destroy (mono_mutex_t *mutex)
{
	return pthread_mutex_destroy (mutex);
}

int
mono_mutex_lock (mono_mutex_t *mutex)
{
	int res;


	res = pthread_mutex_lock (mutex);
	g_assert (res != EINVAL);


	return res;
}

int
mono_mutex_trylock (mono_mutex_t *mutex)
{
	return pthread_mutex_trylock (mutex);
}

int
mono_mutex_unlock (mono_mutex_t *mutex)
{
	return pthread_mutex_unlock (mutex);
}

int
mono_cond_init (mono_cond_t *cond)
{
	return pthread_cond_init (cond, NULL);
}

int
mono_cond_destroy (mono_cond_t *cond)
{
	return pthread_cond_destroy (cond);
}

int
mono_cond_wait (mono_cond_t *cond, mono_mutex_t *mutex)
{
	int res;

	res = pthread_cond_wait (cond, mutex);
	g_assert (res != EINVAL);

	return res;
}

int
mono_cond_timedwait (mono_cond_t *cond, mono_mutex_t *mutex, struct timespec *timeout)
{
	int res;


	res = pthread_cond_timedwait (cond, mutex, timeout);
	g_assert (res != EINVAL);

	return res;
}

int
mono_cond_timedwait_ms (mono_cond_t *cond, mono_mutex_t *mutex, int timeout_ms)
{
	struct timeval tv;
	struct timespec ts;
	gint64 usecs;
	int res;

	/* ms = 10^-3, us = 10^-6, ns = 10^-9 */

	gettimeofday (&tv, NULL);
	tv.tv_sec += timeout_ms / 1000;
	usecs = tv.tv_usec + ((timeout_ms % 1000) * 1000);
	if (usecs >= 1000000) {
		usecs -= 1000000;
		tv.tv_sec ++;
	}
	ts.tv_sec = tv.tv_sec;
	ts.tv_nsec = usecs * 1000;

	res = pthread_cond_timedwait (cond, mutex, &ts);
	g_assert (res != EINVAL);

	return res;
}

int
mono_cond_signal (mono_cond_t *cond)
{
	return pthread_cond_signal (cond);
}

int
mono_cond_broadcast (mono_cond_t *cond)
{
	return pthread_cond_broadcast (cond);
}

#else /* !defined(HOST_WIN32) */

int
mono_mutex_init (mono_mutex_t *mutex)
{
	InitializeCriticalSection (mutex);
	return 0;
}

int
mono_mutex_init_recursive (mono_mutex_t *mutex)
{
	InitializeCriticalSection (mutex);
	return 0;
}

int
mono_mutex_destroy (mono_mutex_t *mutex)
{
	DeleteCriticalSection (mutex);
	return 0;
}

int
mono_mutex_lock (mono_mutex_t *mutex)
{
	EnterCriticalSection (mutex);
	return 0;
}

int
mono_mutex_trylock (mono_mutex_t *mutex)
{
	return TryEnterCriticalSection (mutex) != 0 ? 0 : 1;
}

int
mono_mutex_unlock (mono_mutex_t *mutex)
{
	LeaveCriticalSection (mutex));
	return 0;
}

int
mono_cond_init (mono_cond_t *cond)
{
	InitializeConditionVariable (cond);
	return 0;
}

int
mono_cond_destroy (mono_cond_t *cond)
{
	/* Beauty of win32 API: do not destroy it */
}

int
mono_cond_wait (mono_cond_t *cond, mono_mutex_t *mutex)
{
	int res;

	res = SleepConditionVariableCS (cond, mutex, INFINITE) ? 0 : 1;

	return res;
}

int
mono_cond_timedwait (mono_cond_t *cond, mono_mutex_t *mutex, struct timespec *timeout)
{
	g_assert_not_reached ();
}

int
mono_cond_timedwait_ms (mono_cond_t *cond, mono_mutex_t *mutex, int timeout_ms)
{
	int res;

	res = SleepConditionVariableCS (cond, mutex, timeout_ms) ? 0 : 1;

	return res;
}

int
mono_cond_signal (mono_cond_t *cond)
{
	WakeConditionVariable (cond);
	return 0;
}

int
mono_cond_broadcast (mono_cond_t *cond)
{
	WakeAllConditionVariable (cond);
	return 0;
}

#endif /* !defined(HOST_WIN32) */
