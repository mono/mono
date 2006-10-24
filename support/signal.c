/*
 * <signal.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004-2005 Jonathan Pryor
 */

#include <signal.h>

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

typedef void (*mph_sighandler_t)(int);

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

#ifndef PLATFORM_WIN32
int
Mono_Posix_Syscall_psignal (int sig, const char* s)
{
	errno = 0;
	psignal (sig, s);
	return errno == 0 ? 0 : -1;
}
#endif /* ndef PLATFORM_WIN32 */


G_END_DECLS

/*
 * vim: noexpandtab
 */
