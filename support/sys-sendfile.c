/*
 * <sys/sendfile.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#include <sys/sendfile.h>
#include <errno.h>

#include "mph.h"

G_BEGIN_DECLS

mph_ssize_t
Mono_Posix_Syscall_sendfile (int out_fd, int in_fd, mph_off_t *offset, mph_size_t count)
{
	off_t _offset;
	ssize_t r;
	mph_return_if_off_t_overflow (*offset);

	_offset = *offset;

	r = sendfile (out_fd, in_fd, &_offset, (size_t) count);

	*offset = _offset;

	return r;
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
