/*
 * <signal.h> wrapper functions.
 */

#include <signal.h>

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

G_END_DECLS

/*
 * vim: noexpandtab
 */
