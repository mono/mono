/*
 * <signal.h> wrapper functions.
 */

#include <signal.h>

#include "mph.h"

G_BEGIN_DECLS

sighandler_t
Mono_Posix_Stdlib_SIG_DFL ()
{
	return SIG_DFL;
}

sighandler_t
Mono_Posix_Stdlib_SIG_ERR ()
{
	return SIG_ERR;
}

sighandler_t
Mono_Posix_Stdlib_SIG_IGN ()
{
	return SIG_IGN;
}

void
Mono_Posix_Stdlib_InvokeSignalHandler (int signum, sighandler_t handler)
{
	handler (signum);
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
