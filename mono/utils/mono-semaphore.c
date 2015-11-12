/*
 * mono-semaphore.c: mono-semaphore functions
 *
 * Author:
 *	Gonzalo Paniagua Javier  <gonzalo@novell.com>
 *
 * (C) 2010 Novell, Inc.
 */

#include <config.h>

#include <errno.h>
#ifdef HAVE_SYS_TIME_H
#include <sys/time.h>
#endif
#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif

#include "mono-semaphore.h"

#ifndef NSEC_PER_SEC
#define NSEC_PER_SEC 1000 * 1000 * 1000
#endif

#if defined(USE_MACH_SEMA)

int
mono_sem_init (MonoSemType *sem, int value)
{
	return semaphore_create (current_task (), sem, SYNC_POLICY_FIFO, value) != KERN_SUCCESS ? -1 : 0;
}

int
mono_sem_destroy (MonoSemType *sem)
{
	return semaphore_destroy (current_task (), *sem) != KERN_SUCCESS ? -1 : 0;
}

int
mono_sem_wait (MonoSemType *sem, gboolean alertable)
{
	int res;

retry:
	res = semaphore_wait (*sem);
	g_assert (res != KERN_INVALID_ARGUMENT);

	if (res == KERN_ABORTED && !alertable)
		goto retry;

	return res != KERN_SUCCESS ? -1 : 0;
}

int
mono_sem_timedwait (MonoSemType *sem, guint32 timeout_ms, gboolean alertable)
{
	mach_timespec_t ts, copy;
	struct timeval start, current;
	int res = 0;

	if (timeout_ms == (guint32) 0xFFFFFFFF)
		return mono_sem_wait (sem, alertable);

	ts.tv_sec = timeout_ms / 1000;
	ts.tv_nsec = (timeout_ms % 1000) * 1000000;
	while (ts.tv_nsec >= NSEC_PER_SEC) {
		ts.tv_nsec -= NSEC_PER_SEC;
		ts.tv_sec++;
	}

	copy = ts;
	gettimeofday (&start, NULL);

retry:
	res = semaphore_timedwait (*sem, ts);
	g_assert (res != KERN_INVALID_ARGUMENT);

	if (res == KERN_ABORTED && !alertable) {
		ts = copy;

		gettimeofday (&current, NULL);
		ts.tv_sec -= (current.tv_sec - start.tv_sec);
		ts.tv_nsec -= (current.tv_usec - start.tv_usec) * 1000;
		if (ts.tv_nsec < 0) {
			if (ts.tv_sec <= 0) {
				ts.tv_nsec = 0;
			} else {
				ts.tv_sec--;
				ts.tv_nsec += NSEC_PER_SEC;
			}
		}
		if (ts.tv_sec < 0) {
			ts.tv_sec = 0;
			ts.tv_nsec = 0;
		}

		goto retry;
	}

	return res != KERN_SUCCESS ? -1 : 0;
}

int
mono_sem_post (MonoSemType *sem)
{
	int res;

	res = semaphore_signal (*sem);
	g_assert (res != KERN_INVALID_ARGUMENT);

	return res != KERN_SUCCESS ? -1 : 0;
}

#elif defined(HAVE_SEMAPHORE_H) && !defined(HOST_WIN32)

int
mono_sem_init (MonoSemType *sem, int value)
{
	return sem_init (sem, 0, value);
}

int
mono_sem_destroy (MonoSemType *sem)
{
	return sem_destroy (sem);
}

int
mono_sem_wait (MonoSemType *sem, gboolean alertable)
{
	int res;

retry:
	res = sem_wait (sem);
	if (res == -1)
		g_assert (errno != EINVAL);

	if (res == -1 && errno == EINTR && !alertable)
		goto retry:

	return res != 0 ? -1 : 0;
}

int
mono_sem_timedwait (MonoSemType *sem, guint32 timeout_ms, gboolean alertable)
{
	struct timespec ts, copy;
	struct timeval t;
	int res = 0;

	if (timeout_ms == 0) {
		res = sem_trywait (sem) != 0 ? -1 : 0;
		if (res == -1)
			g_assert (errno != EINVAL);

		return res != 0 ? -1 : 0;
	}

	if (timeout_ms == (guint32) 0xFFFFFFFF)
		return mono_sem_wait (sem, alertable);

	gettimeofday (&t, NULL);
	ts.tv_sec = timeout_ms / 1000 + t.tv_sec;
	ts.tv_nsec = (timeout_ms % 1000) * 1000000 + t.tv_usec * 1000;
	while (ts.tv_nsec >= NSEC_PER_SEC) {
		ts.tv_nsec -= NSEC_PER_SEC;
		ts.tv_sec++;
	}

	copy = ts;

retry:
#if defined(__native_client__) && defined(USE_NEWLIB)
	res = sem_trywait (sem);
#else
	res = sem_timedwait (sem, &ts)
#endif
	if (res == -1)
		g_assert (errno != EINVAL);

	if (res == -1 && errno == EINTR && !alertable) {
		ts = copy;
		goto retry;
	}

	return res != 0 ? -1 : 0;
}

int
mono_sem_post (MonoSemType *sem)
{
	int res;

	res = sem_post (sem);
	if (res == -1)
		g_assert (errno != EINVAL);

	return res;
}

#else

int
mono_sem_init (MonoSemType *sem, int value)
{
	*sem = CreateSemaphore (NULL, value, 0x7FFFFFFF, NULL);
	return *sem == NULL ? -1 : 0;
}

int
mono_sem_destroy (MonoSemType *sem)
{
	return !CloseHandle (*sem) ? -1 : 0;
}

int
mono_sem_wait (MonoSemType *sem, gboolean alertable)
{
	return mono_sem_timedwait (sem, INFINITE, alertable);
}

int
mono_sem_timedwait (MonoSemType *sem, guint32 timeout_ms, gboolean alertable)
{
	gboolean res;

retry:
	res = WaitForSingleObjectEx (*sem, timeout_ms, alertable);

	if (res == WAIT_IO_COMPLETION && !alertable)
		goto retry;

	return res != WAIT_OBJECT_0 ? -1 : 0;
}

int
mono_sem_post (MonoSemType *sem)
{
	return !ReleaseSemaphore (*sem, 1, NULL) ? -1 : 0;
}

#endif
