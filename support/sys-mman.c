/*
 * <sys/mman.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#define _XOPEN_SOURCE 600

#include <sys/types.h>
#include <sys/mman.h>
#include <errno.h>

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

#ifdef HAVE_POSIX_MADVISE
gint32
Mono_Posix_Syscall_posix_madvise (void *addr, mph_size_t len, gint32 advice)
{
	mph_return_if_size_t_overflow (len);

	if (Mono_Posix_FromPosixMadviseAdvice (advice, &advice) == -1)
		return -1;

	return posix_madvise (addr, (size_t) len, advice);
}
#endif /* def HAVE_POSIX_MADVISE */

G_END_DECLS

/*
 * vim: noexpandtab
 */
