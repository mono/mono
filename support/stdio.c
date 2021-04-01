/*
 * <stdio.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004-2006 Jonathan Pryor
 */

#include <stdarg.h>
#include <stdio.h>
#include <stdlib.h>

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

gint32
Mono_Posix_Syscall_L_ctermid (void)
{
#ifndef HOST_WIN32
	return L_ctermid;
#else
	return -1;
#endif
}

gint32
Mono_Posix_Syscall_L_cuserid (void)
{
#if defined(__APPLE__) || defined (__OpenBSD__) || defined (HOST_WIN32)
	return -1;
#else
	return L_cuserid;
#endif
}

mph_size_t
Mono_Posix_Stdlib_fread (unsigned char *ptr, mph_size_t size, mph_size_t nmemb, void *stream)
{
	mph_return_if_size_t_overflow (size);
	mph_return_if_size_t_overflow (nmemb);

	return fread (ptr, (size_t) size, (size_t) nmemb, (FILE*) stream);
}

mph_size_t
Mono_Posix_Stdlib_fwrite (unsigned char *ptr, mph_size_t size, mph_size_t nmemb, void *stream)
{
	mph_return_if_size_t_overflow (size);
	mph_return_if_size_t_overflow (nmemb);

	size_t ret = fwrite (ptr, (size_t) size, (size_t) nmemb, (FILE*) stream);
#ifdef HOST_WIN32
	// Workaround for a particular weirdness on Windows triggered by the
	// StdioFileStreamTest.Write() test method. The test writes 15 bytes to a
	// file, then rewinds the file pointer and reads the same bytes. It then
	// writes 15 additional bytes to the file. This second write fails on
	// Windows with 0 returned from fwrite(). Calling fseek() followed by a retry
	// of fwrite() like we do here fixes the issue.
	if (ret != nmemb)
	{
		fseek (stream, 0, SEEK_CUR);
		ret = fwrite (ptr + (ret * nmemb), (size_t) size, (size_t) nmemb - ret, (FILE*) stream);
	}
#endif
	return ret;
}

#ifdef HAVE_VSNPRINTF
gint32
Mono_Posix_Stdlib_snprintf (char *s, mph_size_t n, char *format, ...);
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
#endif /* def HAVE_VSNPRINTF */

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

void*
Mono_Posix_Stdlib_stdin (void)
{
	return stdin;
}

void*
Mono_Posix_Stdlib_stdout (void)
{
	return stdout;
}

void*
Mono_Posix_Stdlib_stderr (void)
{
	return stderr;
}

gint32
Mono_Posix_Stdlib_TMP_MAX (void)
{
	return TMP_MAX;
}

void*
Mono_Posix_Stdlib_tmpfile (void)
{
	return tmpfile ();
}

gint32
Mono_Posix_Stdlib_setvbuf (void* stream, void *buf, int mode, mph_size_t size)
{
	mph_return_if_size_t_overflow (size);
	return setvbuf (stream, (char *) buf, mode, (size_t) size);
}

int 
Mono_Posix_Stdlib_setbuf (void* stream, void* buf)
{
	setbuf (stream, buf);
	return 0;
}

void*
Mono_Posix_Stdlib_fopen (const char* path, const char* mode)
{
	return fopen (path, mode);
}

void*
Mono_Posix_Stdlib_freopen (const char* path, const char* mode, void *stream)
{
	return freopen (path, mode, stream);
}

gint32
Mono_Posix_Stdlib_fprintf (void* stream, const char* format, const char* message)
{
	return fprintf (stream, format, message);
}

gint32
Mono_Posix_Stdlib_fgetc (void* stream)
{
	return fgetc (stream);
}

void*
Mono_Posix_Stdlib_fgets (char* str, gint32 size, void* stream)
{
	return fgets (str, size, stream);
}

gint32
Mono_Posix_Stdlib_fputc (gint32 c, void* stream)
{
	return fputc (c, stream);
}

gint32
Mono_Posix_Stdlib_fputs (const char* s, void* stream)
{
	return fputs (s, stream);
}

gint32
Mono_Posix_Stdlib_fclose (void* stream)
{
	return fclose (stream);
}

gint32
Mono_Posix_Stdlib_fflush (void* stream)
{
	return fflush (stream);
}

gint32
Mono_Posix_Stdlib_fseek (void* stream, gint64 offset, int origin)
{
	mph_return_if_long_overflow (offset);

	return fseek (stream, offset, origin);
}

gint64
Mono_Posix_Stdlib_ftell (void* stream)
{
	return ftell (stream);
}

void*
Mono_Posix_Stdlib_CreateFilePosition (void)
{
	fpos_t* pos = malloc (sizeof(fpos_t));
	return pos;
}

gint32
Mono_Posix_Stdlib_fgetpos (void* stream, void *pos)
{
	return fgetpos (stream, (fpos_t*) pos);
}

gint32
Mono_Posix_Stdlib_fsetpos (void* stream, void *pos)
{
	return fsetpos (stream, (fpos_t*) pos);
}

int
Mono_Posix_Stdlib_rewind (void* stream)
{
	do {
		rewind (stream);
	} while (errno == EINTR);
	mph_return_if_val_in_list5(errno, EAGAIN, EBADF, EFBIG, EINVAL, EIO);
	mph_return_if_val_in_list5(errno, ENOSPC, ENXIO, EOVERFLOW, EPIPE, ESPIPE);
	return 0;
}

int
Mono_Posix_Stdlib_clearerr (void* stream)
{
	clearerr (((FILE*) stream));
	return 0;
}

gint32
Mono_Posix_Stdlib_ungetc (gint32 c, void* stream)
{
	return ungetc (c, stream);
}

gint32
Mono_Posix_Stdlib_feof (void* stream)
{
	return feof (((FILE*) stream));
}

gint32
Mono_Posix_Stdlib_ferror (void* stream)
{
	return ferror (((FILE*) stream));
}

int
Mono_Posix_Stdlib_perror (const char* s, int err)
{
	errno = err;
	perror (s);
	return 0;
}

#define MPH_FPOS_LENGTH (sizeof(fpos_t)*2)

int
Mono_Posix_Stdlib_DumpFilePosition (char *dest, void *pos, gint32 len)
{
	char *destp;
	unsigned char *posp, *pose;

	if (dest == NULL)
		return MPH_FPOS_LENGTH;

	if (pos == NULL || len <= 0) {
		errno = EINVAL;
		return -1;
	}

	posp = (unsigned char*) pos;
	pose = posp + sizeof(fpos_t);
	destp = dest;

	for ( ; posp < pose && len > 1; destp += 2, ++posp, len -= 2) {
		sprintf (destp, "%02X", *posp);
	}

	if (len)
		dest[MPH_FPOS_LENGTH] = '\0';

	return destp - dest;
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
