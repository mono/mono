/*
 * <stdio.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004-2005 Jonathan Pryor
 */

#include <stdio.h>
#include <stdlib.h>

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

gint32
Mono_Posix_Stdlib__IOFBF (void)
{
	return _IOFBF;
}

gint32
Mono_Posix_Stdlib__IOLBF (void)
{
	return _IOLBF;
}

gint32
Mono_Posix_Stdlib__IONBF (void)
{
	return _IONBF;
}

gint32
Mono_Posix_Stdlib_BUFSIZ (void)
{
	return BUFSIZ;
}

gint32
Mono_Posix_Stdlib_EOF (void)
{
	return EOF;
}

gint32
Mono_Posix_Stdlib_FOPEN_MAX (void)
{
	return FOPEN_MAX;
}

gint32
Mono_Posix_Stdlib_FILENAME_MAX (void)
{
	return FILENAME_MAX;
}

gint32
Mono_Posix_Stdlib_L_tmpnam (void)
{
	return L_tmpnam;
}

FILE*
Mono_Posix_Stdlib_stdin (void)
{
	return stdin;
}

FILE*
Mono_Posix_Stdlib_stdout (void)
{
	return stdout;
}

FILE*
Mono_Posix_Stdlib_stderr (void)
{
	return stderr;
}

gint32
Mono_Posix_Stdlib_TMP_MAX (void)
{
	return TMP_MAX;
}

gint32
Mono_Posix_Stdlib_setvbuf (FILE *stream, char *buf, int mode, mph_size_t size)
{
	mph_return_if_size_t_overflow (size);
	return setvbuf (stream, buf, mode, (size_t) size);
}

gint32
Mono_Posix_Stdlib_fseek (FILE* stream, gint64 offset, int origin)
{
	mph_return_if_long_overflow (offset);

	return fseek (stream, offset, origin);
}

gint64
Mono_Posix_Stdlib_ftell (FILE* stream)
{
	return ftell (stream);
}

fpos_t*
Mono_Posix_Stdlib_CreateFilePosition (void)
{
	fpos_t* pos = malloc (sizeof(fpos_t));
	return pos;
}

gint32
Mono_Posix_Stdlib_fgetpos (FILE* stream, fpos_t *pos)
{
	return fgetpos (stream, pos);
}

gint32
Mono_Posix_Stdlib_fsetpos (FILE* stream, fpos_t *pos)
{
	return fsetpos (stream, pos);
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
