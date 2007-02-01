/*
 * <stdio.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004-2006 Jonathan Pryor
 */

#include <sys/types.h>
#include <utime.h>

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

gint32
Mono_Posix_Syscall_utime (const char *filename, struct Mono_Posix_Utimbuf *buf, 
		int use_buf)
{
	struct utimbuf _buf;
	struct utimbuf *pbuf = NULL;

	if (buf && use_buf) {
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
