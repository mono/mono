/*
 * <stdio.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004-2005 Jonathan Pryor
 */

#include <stdarg.h>
#include <stdio.h>
#include <stdlib.h>

#include "mph.h"

G_BEGIN_DECLS

#ifndef PLATFORM_WIN32
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
#endif /* ndef PLATFORM_WIN32 */

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

#ifdef HAVE_VSNPRINTF
gint32
Mono_Posix_Stdlib_snprintf (char *s, mph_size_t n, char *format, ...)
{
	va_list ap;
	gint32 r;
	mph_return_if_size_t_overflow (n);

	va_start (ap, format);
	r = vsnprintf (s, (size_t) n, format, ap);
	va_end (ap);

	return r;
}
#endif /* def HAVE_SNPRINTF */

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

#define MPH_FPOS_LENGTH (sizeof(fpos_t)*2)

int
Mono_Posix_Stdlib_DumpFilePosition (char *dest, fpos_t *pos, gint32 len)
{
	char *destp;
	unsigned char *posp, *pose;
	int i;

	if (dest == NULL)
		return MPH_FPOS_LENGTH;

	if (pos == NULL || len <= 0) {
		errno = EINVAL;
		return -1;
	}

	posp = (unsigned char*) pos;
	pose = posp + sizeof(*pos);
	destp = dest;

	for ( ; posp < pose && len > 1; destp += 2, ++posp, len -= 2) {
		sprintf (destp, "%02X", *posp);
	}

	if (len)
		dest[MPH_FPOS_LENGTH] = '\0';

	return dest;
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
