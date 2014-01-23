/*
 * <signal.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *   Jonathan Pryor (jpryor@novell.com)
 *   Tim Jenks (tim.jenks@realtimeworlds.com)
 *
 * Copyright (C) 2004-2005 Jonathan Pryor
 * Copyright (C) 2008 Novell, Inc.
 */

#include <signal.h>

#include "map.h"
#include "mph.h"

#ifndef HOST_WIN32
#include <sys/time.h>
#include <sys/types.h>
#if defined(__APPLE__)
#include "fakepoll.h"
#else
#include <poll.h>
#endif
#include <unistd.h>
#include <stdlib.h>
#include <string.h>
#include <pthread.h>
#include <mono/utils/atomic.h>
#include <mono/metadata/appdomain.h>
#endif

G_BEGIN_DECLS

typedef void (*mph_sighandler_t)(int);
typedef struct Mono_Unix_UnixSignal_SignalInfo signal_info;

#ifndef HOST_WIN32
static int count_handlers (int signum);
#endif

void*
Mono_Posix_Stdlib_SIG_DFL (void)
{
	return SIG_DFL;
}

void*
Mono_Posix_Stdlib_SIG_ERR (void)
{
	return SIG_ERR;
}

void*
Mono_Posix_Stdlib_SIG_IGN (void)
{
	return SIG_IGN;
}

void
Mono_Posix_Stdlib_InvokeSignalHandler (int signum, void *handler)
{
	mph_sighandler_t _h = (mph_sighandler_t) handler;
	_h (signum);
}

int Mono_Posix_SIGRTMIN (void)
{
#ifdef SIGRTMIN
	return SIGRTMIN;
#else /* def SIGRTMIN */
	return -1;
#endif /* ndef SIGRTMIN */
}

int Mono_Posix_SIGRTMAX (void)
{
#ifdef SIGRTMAX
	return SIGRTMAX;
#else /* def SIGRTMAX */
	return -1;
#endif /* ndef SIGRTMAX */
}

int Mono_Posix_FromRealTimeSignum (int offset, int *r)
{
	if (NULL == r) {
		errno = EINVAL;
		return -1;
	}
	*r = 0;
#if defined (SIGRTMIN) && defined (SIGRTMAX)
	if ((offset < 0) || (SIGRTMIN > SIGRTMAX - offset)) {
		errno = EINVAL;
		return -1;
	}
	*r = SIGRTMIN+offset;
	return 0;
#else /* defined (SIGRTMIN) && defined (SIGRTMAX) */
# ifdef ENOSYS
	errno = ENOSYS;
# endif /* ENOSYS */
	return -1;
#endif /* defined (SIGRTMIN) && defined (SIGRTMAX) */
}

#ifndef HOST_WIN32

#ifndef WAPI_NO_ATOMIC_ASM
	#define mph_int_get(p)     InterlockedExchangeAdd ((p), 0)
	#define mph_int_inc(p)     InterlockedIncrement ((p))
	#define mph_int_dec_test(p)     (InterlockedDecrement ((p)) == 0)
	#define mph_int_set(p,o,n) InterlockedExchange ((p), (n))
#elif GLIB_CHECK_VERSION(2,4,0)
	#define mph_int_get(p) g_atomic_int_get ((p))
 	#define mph_int_inc(p) do {g_atomic_int_inc ((p));} while (0)
	#define mph_int_dec_test(p) g_atomic_int_dec_and_test ((p))
	#define mph_int_set(p,o,n) do {                                 \
		while (!g_atomic_int_compare_and_exchange ((p), (o), (n))) {} \
	} while (0)
#else
	#define mph_int_get(p) (*(p))
	#define mph_int_inc(p) do { (*(p))++; } while (0)
	#define mph_int_dec_test(p) (--(*(p)) == 0)
	#define mph_int_set(p,o,n) do { *(p) = n; } while (0)
#endif

#if HAVE_PSIGNAL
int
Mono_Posix_Syscall_psignal (int sig, const char* s)
{
	errno = 0;
	psignal (sig, s);
	return errno == 0 ? 0 : -1;
}
#endif  /* def HAVE_PSIGNAL */

#define NUM_SIGNALS 64
static signal_info signals[NUM_SIGNALS];

static int acquire_mutex (pthread_mutex_t *mutex)
{
	int mr;
	while ((mr = pthread_mutex_lock (mutex)) == EAGAIN) {
		/* try to acquire again */
	}
	if ((mr != 0) && (mr != EDEADLK))  {
		errno = mr;
		return -1;
	}
	return 0;
}

static void release_mutex (pthread_mutex_t *mutex)
{
	int mr;
	while ((mr = pthread_mutex_unlock (mutex)) == EAGAIN) {
		/* try to release mutex again */
	}
}

static inline int
keep_trying (int r)
{
	return r == -1 && errno == EINTR;
}

static void
default_handler (int signum)
{
	int i;
	for (i = 0; i < NUM_SIGNALS; ++i) {
		int fd;
		signal_info* h = &signals [i];
		if (mph_int_get (&h->signum) != signum)
			continue;
		mph_int_inc (&h->count);
		fd = mph_int_get (&h->write_fd);
		if (fd > 0) {
			int j,pipecounter;
			char c = signum;
			pipecounter = mph_int_get (&h->pipecnt);
			for (j = 0; j < pipecounter; ++j) {
				int r;
				do { r = write (fd, &c, 1); } while (keep_trying (r));
				fsync (fd); /* force */
			}
		}
	}
}

static pthread_mutex_t signals_mutex = PTHREAD_MUTEX_INITIALIZER;

void*
Mono_Unix_UnixSignal_install (int sig)
{
	int i;
	signal_info* h = NULL; 
	int have_handler = 0;
	void* handler = NULL;

	if (acquire_mutex (&signals_mutex) == -1)
		return NULL;

#if defined (SIGRTMIN) && defined (SIGRTMAX)
	/*The runtime uses some rt signals for itself so it's important to not override them.*/
	if (sig >= SIGRTMIN && sig <= SIGRTMAX && count_handlers (sig) == 0) {
		struct sigaction sinfo;
		sigaction (sig, NULL, &sinfo);
		if (sinfo.sa_handler != SIG_DFL || (void*)sinfo.sa_sigaction != (void*)SIG_DFL) {
			pthread_mutex_unlock (&signals_mutex);
			errno = EADDRINUSE;
			return NULL;
		}
	}
#endif /*defined (SIGRTMIN) && defined (SIGRTMAX)*/

	for (i = 0; i < NUM_SIGNALS; ++i) {
		if (h == NULL && signals [i].signum == 0) {
			h = &signals [i];
			h->handler = signal (sig, default_handler);
			if (h->handler == SIG_ERR) {
				h->handler = NULL;
				h = NULL;
				break;
			}
			else {
				h->have_handler = 1;
			}
		}
		if (!have_handler && signals [i].signum == sig &&
				signals [i].handler != default_handler) {
			have_handler = 1;
			handler = signals [i].handler;
		}
		if (h && have_handler)
			break;
	}

	if (h && have_handler) {
		h->have_handler = 1;
		h->handler      = handler;
	}

	if (h) {
		mph_int_set (&h->count, h->count, 0);
		mph_int_set (&h->signum, h->signum, sig);
		mph_int_set (&h->pipecnt, h->pipecnt, 0);
	}

	release_mutex (&signals_mutex);

	return h;
}

static int
count_handlers (int signum)
{
	int i;
	int count = 0;
	for (i = 0; i < NUM_SIGNALS; ++i) {
		if (signals [i].signum == signum)
			++count;
	}
	return count;
}

int
Mono_Unix_UnixSignal_uninstall (void* info)
{
	signal_info* h;
	int r = -1;

	if (acquire_mutex (&signals_mutex) == -1)
		return -1;

	h = info;

	if (h == NULL || h < signals || h > &signals [NUM_SIGNALS])
		errno = EINVAL;
	else {
		/* last UnixSignal -- we can unregister */
		if (h->have_handler && count_handlers (h->signum) == 1) {
			mph_sighandler_t p = signal (h->signum, h->handler);
			if (p != SIG_ERR)
				r = 0;
			h->handler      = NULL;
			h->have_handler = 0;
		}
		h->signum = 0;
	}

	release_mutex (&signals_mutex);

	return r;
}

static int
setup_pipes (signal_info** signals, int count, struct pollfd *fd_structs, int *currfd)
{
	int i;
	int r = 0;
	for (i = 0; i < count; ++i) {
		signal_info* h;
		int filedes[2];

		h = signals [i];

		if (mph_int_get (&h->pipecnt) == 0) {
			if ((r = pipe (filedes)) != 0) {
				break;
			}
			h->read_fd  = filedes [0];
			h->write_fd = filedes [1];
		}
		mph_int_inc (&h->pipecnt);
		fd_structs[*currfd].fd = h->read_fd;
		fd_structs[*currfd].events = POLLIN;
		++(*currfd);
	}
	return r;
}

static void
teardown_pipes (signal_info** signals, int count)
{
	int i;
	for (i = 0; i < count; ++i) {
		signal_info* h = signals [i];

		if (mph_int_dec_test (&h->pipecnt)) {
			if (h->read_fd != 0)
				close (h->read_fd);
			if (h->write_fd != 0)
				close (h->write_fd);
			h->read_fd  = 0;
			h->write_fd = 0;
		}
	}
}

static int
wait_for_any (signal_info** signals, int count, int *currfd, struct pollfd* fd_structs, int timeout, Mono_Posix_RuntimeIsShuttingDown shutting_down)
{
	int r, idx;
	do {
		struct timeval tv;
		struct timeval *ptv = NULL;
		if (timeout != -1) {
			tv.tv_sec  = timeout / 1000;
			tv.tv_usec = (timeout % 1000)*1000;
			ptv = &tv;
		}
		r = poll (fd_structs, count, timeout);
	} while (keep_trying (r) && !shutting_down ());

	idx = -1;
	if (r == 0)
		idx = timeout;
	else if (r > 0) {
		int i;
		for (i = 0; i < count; ++i) {
			signal_info* h = signals [i];
			if (fd_structs[i].revents & POLLIN) {
				int r;
				char c;
				do {
					r = read (h->read_fd, &c, 1);
				} while (keep_trying (r) && !shutting_down ());
				if (idx == -1)
					idx = i;
			}
		}
	}

	return idx;
}

/*
 * returns: -1 on error:
 *          timeout on timeout
 *          index into _signals array of signal that was generated on success
 */
int
Mono_Unix_UnixSignal_WaitAny (void** _signals, int count, int timeout /* milliseconds */, Mono_Posix_RuntimeIsShuttingDown shutting_down)
{
	int r;
	int currfd = 0;
	struct pollfd fd_structs[NUM_SIGNALS];

	signal_info** signals = (signal_info**) _signals;

	if (count > NUM_SIGNALS)
		return -1;

	if (acquire_mutex (&signals_mutex) == -1)
		return -1;

	r = setup_pipes (signals, count, &fd_structs[0], &currfd);

	release_mutex (&signals_mutex);

	if (r == 0) {
		r = wait_for_any (signals, count, &currfd, &fd_structs[0], timeout, shutting_down);
	}

	if (acquire_mutex (&signals_mutex) == -1)
		return -1;

	teardown_pipes (signals, count);

	release_mutex (&signals_mutex);

	return r;
}

#endif /* ndef HOST_WIN32 */


G_END_DECLS

/*
 * vim: noexpandtab
 */
