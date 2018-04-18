/*
 * <sys/sendfile.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#include <config.h>

#include <sys/types.h>
#include <errno.h>

#include "mono/utils/mono-compiler.h"
#include "map.h"
#include "mph.h"

#ifdef HAVE_SYS_SENDFILE_H
#include <sys/sendfile.h>
#endif /* ndef HAVE_SYS_SENDFILE_H */

G_BEGIN_DECLS

#ifdef HAVE_SENDFILE
mph_ssize_t
Mono_Posix_Syscall_sendfile (int out_fd, int in_fd, mph_off_t *offset, mph_size_t count)
{
	off_t _offset;
	ssize_t r;
	mph_return_if_off_t_overflow (*offset);

	_offset = *offset;

#if defined(HOST_DARWIN) || defined(HOST_BSD)
	/* The BSD version has 6 arguments */
	g_assert_not_reached ();
#else
	r = sendfile (out_fd, in_fd, &_offset, (size_t) count);
#endif

	*offset = _offset;

	return r;
}
#endif /* ndef HAVE_SENDFILE */

G_END_DECLS

/*
 * vim: noexpandtab
 */
