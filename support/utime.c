/*
 * <stdio.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#include <sys/types.h>
#include <utime.h>

#include "mph.h"

G_BEGIN_DECLS

struct Mono_Posix_Utimbuf {
	/* time_t */ mph_time_t actime;   /* access time */
	/* time_t */ mph_time_t modtime;  /* modification time */
};

gint32
Mono_Posix_Syscall_utime (const char *filename, struct Mono_Posix_Utimbuf *buf)
{
	struct utimbuf _buf;
	struct utimbuf *pbuf = NULL;

	if (buf) {
		_buf.actime  = buf->actime;
		_buf.modtime = buf->modtime;
		pbuf = &_buf;
	}

	return utime (filename, pbuf);
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
