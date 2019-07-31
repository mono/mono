/*
 * <errno.h> wrapper functions.
 */

#include <errno.h>
#include <string.h>
#include "map.h"
#include "mph.h"
#include <stdio.h>

G_BEGIN_DECLS

int
Mono_Posix_Stdlib_GetLastError (void)
{
	return errno;
}

void
Mono_Posix_Stdlib_SetLastError (int error_number)
{
	errno = error_number;
}

#ifdef HAVE_STRERROR_R

/* 
 * There are two versions of strerror_r: 
 *  - the GNU version:  char *strerror_r (int errnum, char *buf, size_t n);
 *  - the XPG version:    int strerror_r (int errnum, char *buf, size_t n);
 *
 * Ideally I could stick with the XPG version, but we need to support 
 * Red Hat 9, which only supports the GNU version.
 *
 * Furthermore, I do NOT want to export the GNU version in Mono.Posix.dll, 
 * as that's supposed to contain *standard* function definitions (give or 
 * take a few GNU extensions).  Portability trumps all.
 *
 * Consequently, we export the functionality of the XPG version.  
 * Internally, we se the GNU version if _GNU_SOURCE is defined, otherwise 
 * we assume that the XPG version is present.
 */

#ifdef STRERROR_R_CHAR_P
#define mph_min(x,y) ((x) <= (y) ? (x) : (y))

/* If you pass an invalid errno value to glibc 2.3.2's strerror_r, you get
 * back the string "Unknown error" with the error value appended. */
static const char mph_unknown[] = "Unknown error ";

/*
 * Translate the GNU semantics to the XPG semantics.
 *
 * From reading the (RH9-using) GLibc 2.3.2 sysdeps/generic/_strerror.c, 
 * we can say the following:
 *   - If errnum is a valid error number, a pointer to a constant string is
 *     returned.  Thus, the prototype *lies* (it's not really a char*).
 *     `buf' is unchanged (WTF?).
 *   - If errnum is an *invalid* error number, an error message is copied
 *     into `buf' and `buf' is returned.  The error message returned is
 *     "Unknown error %i", where %i is the input errnum.
 *
 * Meanwhile, XPG always modifies `buf' if there's enough space, and either
 * returns 0 (success) or -1 (error) with errno = EINVAL (bad errnum) or
 * ERANGE (`buf' isn't big enough).  Also, GLibc 2.3.3 (which has the XPG
 * version) first checks the validity of errnum first, then does the copy.
 *
 * Assuming that the GNU implementation doesn't change much (ha!), we can
 * check for EINVAL by comparing the strerror_r return to `buf', OR by
 * comparing the return value to "Uknown error".  (This assumes that 
 * strerror_r will always only return the input buffer for errors.)
 *
 * Check for ERANGE by comparing the string length returned by strerror_r to
 * `n'.
 *
 * Then pray that this actually works...
 */
gint32
Mono_Posix_Syscall_strerror_r (int errnum, char *buf, mph_size_t n)
{
	char *r;
	char ebuf [sizeof(mph_unknown)];
	size_t len;
	size_t blen;

	mph_return_if_size_t_overflow (n);

	/* first, check for valid errnum */
#if HOST_ANDROID
	/* Android NDK defines _GNU_SOURCE but strerror_r follows the XSI semantics
	 * not the GNU one. XSI version returns an integer, as opposed to the GNU one
	 * which returns pointer to the buffer.
	 */
	if (strerror_r (errnum, ebuf, sizeof(ebuf)) == -1) {
		/* XSI strerror_r will return -1 if errno is set, but if we leave the value
		 * alone it breaks Mono.Posix StdioFileStream tests, so we'll ignore the value
		 * and set errno as below
		 */
		errno = EINVAL;
		return -1;
	}
	r = ebuf;
#else
	r = strerror_r (errnum, ebuf, sizeof(ebuf));
#endif
	if (!r) {
		errno = EINVAL;
		return -1;
	} 
	len = strlen (r);

	if (r == ebuf ||
			strncmp (r, mph_unknown, mph_min (len, sizeof(mph_unknown))) == 0) {
		errno = EINVAL;
		return -1;
	}

	/* valid errnum (we hope); is buffer big enough? */
	blen = (size_t) n;
	if ((len+1) > blen) {
		errno = ERANGE;
		return -1;
	}

	strncpy (buf, r, len);
	buf[len] = '\0';

	return 0;
}

#else /* !def STRERROR_R_CHAR_P */

gint32
Mono_Posix_Syscall_strerror_r (int errnum, char *buf, mph_size_t n)
{
	mph_return_if_size_t_overflow (n);
	return strerror_r (errnum, buf, (size_t) n);
}

#endif /* def STRERROR_R_CHAR_P */

#endif /* def HAVE_STRERROR_R */

G_END_DECLS

/*
 * vim: noexpandtab
 */
