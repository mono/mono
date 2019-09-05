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
#if defined(HAVE_POLL_H)
#include <poll.h>
#elif defined(HAVE_SYS_POLL_H)
#include <sys/poll.h>
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

// Atomicity rules: Fields of signal_info read or written by the signal handler
// (see UnixSignal.cs) should be read and written using atomic functions.
// (For simplicity, we're protecting some things we don't strictly need to.)

// Because we are in MonoPosixHelper, we are banned from linking mono.
// We can still use atomic.h because that's all inline functions--
// unless WAPI_NO_ATOMIC_ASM is defined, in which case atomic.h calls linked functions.
#ifndef WAPI_NO_ATOMIC_ASM
	#define mph_int_get(p)     mono_atomic_fetch_add_i32 ((p), 0)
	#define mph_int_inc(p)     mono_atomic_inc_i32 ((p))
	#define mph_int_dec_test(p)     (mono_atomic_dec_i32 ((p)) == 0)
	#define mph_int_set(p,n) mono_atomic_xchg_i32 ((p), (n))
	// Pointer, original, new
	#define mph_int_test_and_set(p,o,n) (o == mono_atomic_cas_i32 ((p), (n), (o)))
#elif GLIB_CHECK_VERSION(2,4,0)
	#define mph_int_get(p) g_atomic_int_get ((p))
 	#define mph_int_inc(p) do {g_atomic_int_inc ((p));} while (0)
	#define mph_int_dec_test(p) g_atomic_int_dec_and_test ((p))
	#define mph_int_set(p,n) g_atomic_int_set ((p),(n))
	#define mph_int_test_and_set(p,o,n) g_atomic_int_compare_and_exchange ((p), (o), (n))
#else
	#error "GLIB 2.4 required because building without ASM atomics"
#endif

#if HAVE_PSIGNAL

/* 
 * HACK: similar to the mkdtemp one in glib; turns out gcc "helpfully"
 * shadows system headers with "fixed" versions that omit functions...
 * in any case, psignal is another victim of poor GNU decisions. Even
 * then, we may have to do this anyways, as psignal, while present in
 * libc, isn't in PASE headers - so do it anyways
 */
#if defined(_AIX)
extern void psignal(int, const char *);
#endif

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

static int
keep_trying (int r)
{
	return r == -1 && errno == EINTR;
}

// This tiny ad-hoc read/write lock is needed because of the very specific
// synchronization needed between default_handler and teardown_pipes:
// - Many default_handlers can be running at once
// - The signals_mutex already ensures only one teardown_pipes runs at once
// - If teardown_pipes starts while a default_handler is ongoing, it must block
// - If default_handler starts while a teardown_pipes is ongoing, it must *not* block
// Locks are implemented as ints.

// The lock is split into a teardown bit and a handler count (sign bit unused).
// There is a teardown running or waiting to run if the teardown bit is set.
// There is a handler running if the handler count is nonzero.
#define PIPELOCK_TEARDOWN_BIT (  (int)0x40000000 )
#define PIPELOCK_COUNT_MASK   (~((int)0xC0000000))
#define PIPELOCK_GET_COUNT(x)      ((x) & PIPELOCK_COUNT_MASK)
#define PIPELOCK_INCR_COUNT(x, by) (((x) & PIPELOCK_TEARDOWN_BIT) | (PIPELOCK_GET_COUNT (PIPELOCK_GET_COUNT (x) + (by))))

static void
acquire_pipelock_teardown (int *lock)
{
	int lockvalue_draining;
	// First mark that a teardown is occurring, so handlers will stop entering the lock.
	while (1) {
		int lockvalue = mph_int_get (lock);
		lockvalue_draining = lockvalue | PIPELOCK_TEARDOWN_BIT;
		if (mph_int_test_and_set (lock, lockvalue, lockvalue_draining))
			break;
	}
	// Now wait for all handlers to complete.
	while (1) {
		if (0 == PIPELOCK_GET_COUNT (lockvalue_draining))
			break; // We now hold the lock.
		// Handler is still running, spin until it completes.
		sched_yield (); // We can call this because !defined(HOST_WIN32)
		lockvalue_draining = mph_int_get (lock);
	}
}

static void
release_pipelock_teardown (int *lock)
{
	while (1) {
		int lockvalue = mph_int_get (lock);
		int lockvalue_new = lockvalue & ~PIPELOCK_TEARDOWN_BIT;
		// Technically this can't fail, because we hold both the pipelock and the mutex, but
		if (mph_int_test_and_set (lock, lockvalue, lockvalue_new))
			return;
	}
}

// Return 1 for success
static int
acquire_pipelock_handler (int *lock)
{
	while (1) {
		int lockvalue = mph_int_get (lock);
		if (lockvalue & PIPELOCK_TEARDOWN_BIT) // Final lock is being torn down
			return 0;
		int lockvalue_new = PIPELOCK_INCR_COUNT (lockvalue, 1);
		if (mph_int_test_and_set (lock, lockvalue, lockvalue_new))
			return 1;
	}
}

static void
release_pipelock_handler (int *lock)
{
	while (1) {
		int lockvalue = mph_int_get (lock);
		int lockvalue_new = PIPELOCK_INCR_COUNT (lockvalue, -1);
		if (mph_int_test_and_set (lock, lockvalue, lockvalue_new))
			return;
	}
}

// This handler is registered once for each UnixSignal object. A pipe is maintained
// for each one; Wait users read at one end of this pipe, and default_handler sends
// a write on the pipe for each signal received while the Wait is ongoing.
//
// Notice a fairly unlikely race condition exists here: Because we synchronize with
// pipe teardown, but not install/uninstall (in other words, we are only trying to
// protect against writing on a closed pipe) it is technically possible a full
// uninstall and then an install could complete after signum is checked but before
// the remaining instructions execute. In this unlikely case count could be
// incremented or a byte written on the wrong signal handler.
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

		if (!acquire_pipelock_handler (&h->pipelock))
			continue; // Teardown is occurring on this object, no one to send to.

		fd = mph_int_get (&h->write_fd);
		if (fd > 0) { // If any listener exists to write to
			int j,pipecounter;
			char c = signum; // (Value is meaningless)
			pipecounter = mph_int_get (&h->pipecnt); // Write one byte per pipe listener
			for (j = 0; j < pipecounter; ++j) {
				int r;
				do { r = write (fd, &c, 1); } while (keep_trying (r));
			}
		}
		release_pipelock_handler (&h->pipelock);
	}
}

static pthread_mutex_t signals_mutex = PTHREAD_MUTEX_INITIALIZER;

// A UnixSignal object is being constructed
void*
Mono_Unix_UnixSignal_install (int sig)
{
#if defined(HAVE_SIGNAL)
	int i;
	signal_info* h = NULL;        // signals[] slot to install to
	int have_handler = 0;         // Candidates for signal_info handler fields
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
			return NULL; // This is an rt signal with an existing handler. Bail out.
		}
	}
#endif /*defined (SIGRTMIN) && defined (SIGRTMAX)*/

	// Scan through signals list looking for (1) an unused spot (2) a usable value for handler
	for (i = 0; i < NUM_SIGNALS; ++i) {
		int just_installed = 0;
		// We're still looking for a signal_info spot, and this one is available:
		if (h == NULL && mph_int_get (&signals [i].signum) == 0) {
			h = &signals [i];
			h->handler = signal (sig, default_handler);
			if (h->handler == SIG_ERR) {
				h->handler = NULL;
				h = NULL;
				break;
			}
			else {
				just_installed = 1;
			}
		}
		// Check if this slot has a "usable" (not installed by this file) handler-to-restore-later:
		// (On the first signal to be installed, signals [i] will be == h when this happens.)
		if (!have_handler && (just_installed || mph_int_get (&signals [i].signum) == sig) &&
				signals [i].handler != default_handler) {
			have_handler = 1;
			handler = signals [i].handler;
		}
		if (h && have_handler) // We have everything we need
			break;
	}

	if (h) {
		// If we reached here without have_handler, this means that default_handler
		// was set as the signal handler before the first UnixSignal object was installed.
		g_assert (have_handler);

		// Overwrite the tenative handler we set a moment ago with a known-usable one
		h->handler = handler;
		h->have_handler = 1;

		mph_int_set (&h->count, 0);
		mph_int_set (&h->pipecnt, 0);
		mph_int_set (&h->signum, sig);
	}

	release_mutex (&signals_mutex);

	return h;
#else
	g_error ("signal() is not supported by this platform");
	return 0;
#endif
}

static int
count_handlers (int signum)
{
	int i;
	int count = 0;
	for (i = 0; i < NUM_SIGNALS; ++i) {
		if (mph_int_get (&signals [i].signum) == signum)
			++count;
	}
	return count;
}

// A UnixSignal object is being Disposed
int
Mono_Unix_UnixSignal_uninstall (void* info)
{
#if defined(HAVE_SIGNAL)
	signal_info* h;
	int r = -1;

	if (acquire_mutex (&signals_mutex) == -1)
		return -1;

	h = info;

	if (h == NULL || h < signals || h > &signals [NUM_SIGNALS])
		errno = EINVAL;
	else {
		/* last UnixSignal -- we can unregister */
		int signum = mph_int_get (&h->signum);
		if (h->have_handler && count_handlers (signum) == 1) {
			mph_sighandler_t p = signal (signum, h->handler);
			if (p != SIG_ERR)
				r = 0;
			h->handler      = NULL;
			h->have_handler = 0;
		}
		mph_int_set (&h->signum, 0);
	}

	release_mutex (&signals_mutex);

	return r;
#else
	g_error ("signal() is not supported by this platform");
	return 0;
#endif
}

// Set up a signal_info to begin waiting for signal
static int
setup_pipes (signal_info** signals, int count, struct pollfd *fd_structs, int *currfd)
{
	int i;
	int r = 0;
	for (i = 0; i < count; ++i) {
		signal_info* h;
		int filedes[2];

		h = signals [i];

		if (mph_int_get (&h->pipecnt) == 0) { // First listener for this signal_info
			if ((r = pipe (filedes)) != 0) {
				break;
			}
			mph_int_set (&h->read_fd,  filedes [0]);
			mph_int_set (&h->write_fd, filedes [1]);
		}
		mph_int_inc (&h->pipecnt);
		fd_structs[*currfd].fd = mph_int_get (&h->read_fd);
		fd_structs[*currfd].events = POLLIN;
		++(*currfd); // count is verified less than NUM_SIGNALS by caller
	}
	return r;
}

// Cleanup a signal_info after waiting for signal
static void
teardown_pipes (signal_info** signals, int count)
{
	int i;
	for (i = 0; i < count; ++i) {
		signal_info* h = signals [i];

		if (mph_int_dec_test (&h->pipecnt)) { // Final listener for this signal_info
			acquire_pipelock_teardown (&h->pipelock);
			int read_fd = mph_int_get (&h->read_fd);
			int write_fd = mph_int_get (&h->write_fd);
			if (read_fd != 0)
				close (read_fd);
			if (write_fd != 0)
				close (write_fd);
			mph_int_set (&h->read_fd, 0);
			mph_int_set (&h->write_fd, 0);
			release_pipelock_teardown (&h->pipelock);
		}
	}
}

// Given pipes set up, wait for a byte to arrive on one of them
static int
wait_for_any (signal_info** signals, int count, int *currfd, struct pollfd* fd_structs, int timeout, Mono_Posix_RuntimeIsShuttingDown shutting_down)
{
	int r, idx;
	// Poll until one of this signal_info's pipes is ready to read.
	// Once a second, stop to check if the VM is shutting down.
	do {
		r = poll (fd_structs, count, timeout);
	} while (keep_trying (r) && !shutting_down ());

	idx = -1;
	if (r == 0)
		idx = timeout;
	else if (r > 0) { // The pipe[s] are ready to read.
		int i;
		for (i = 0; i < count; ++i) {
			signal_info* h = signals [i];
			if (fd_structs[i].revents & POLLIN) {
				int r;
				char c;
				do {
					r = read (mph_int_get (&h->read_fd), &c, 1);
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
