/*
 * <stdio.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#include <stdio.h>

#include "mph.h"

G_BEGIN_DECLS

gint32
Mono_Posix_Syscall_L_ctermid (void)
{
	return L_ctermid;
}

gint32
Mono_Posix_Syscall_L_cuserid (void)
{
	return L_cuserid;
}

mph_size_t
Mono_Posix_Stdlib_fread (void *ptr, mph_size_t size, mph_size_t nmemb, FILE *stream)
{
	mph_return_if_size_t_overflow (size);
	mph_return_if_size_t_overflow (nmemb);

	return fread (ptr, (size_t) size, (size_t) nmemb, stream);
}

mph_size_t
Mono_Posix_Stdlib_fwrite (const void *ptr, mph_size_t size, mph_size_t nmemb, FILE *stream)
{
	mph_return_if_size_t_overflow (size);
	mph_return_if_size_t_overflow (nmemb);

	return fwrite (ptr, (size_t) size, (size_t) nmemb, stream);
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
