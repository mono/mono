/*
 * <errno.h> wrapper functions.
 */

/* to get XPG's strerror_r declaration */
#undef _GNU_SOURCE
#undef _XOPEN_SOURCE
#define _XOPEN_SOURCE 600

#include <errno.h>
#include <string.h>
#include "mph.h"
#include <stdio.h>

G_BEGIN_DECLS

void
Mono_Posix_Syscall_SetLastError (int error_number)
{
	errno = error_number;
}

#ifdef HAVE_STRERROR_R
gint32
Mono_Posix_Syscall_strerror_r (int errnum, char *buf, mph_size_t n)
{
	mph_return_if_size_t_overflow (n);
	return strerror_r (errnum, buf, (size_t) n);
}
#endif /* def HAVE_STRERROR_R */

G_END_DECLS

/*
 * vim: noexpandtab
 */
