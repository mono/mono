/*
 * <signal.h> wrapper functions.
 */

#include <signal.h>

#include "mph.h"

G_BEGIN_DECLS

typedef void (*mph_sighandler_t)(int);

mph_sighandler_t
Mono_Posix_Stdlib_SIG_DFL (void)
{
	return SIG_DFL;
}

mph_sighandler_t
Mono_Posix_Stdlib_SIG_ERR (void)
{
	return SIG_ERR;
}

mph_sighandler_t
Mono_Posix_Stdlib_SIG_IGN (void)
{
	return SIG_IGN;
}

void
Mono_Posix_Stdlib_InvokeSignalHandler (int signum, mph_sighandler_t handler)
{
	handler (signum);
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
