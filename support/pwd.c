/*
 * <pwd.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#include <pwd.h>
#include <errno.h>
#include <string.h>
#include <stdio.h>
#include <stdlib.h>

#include "mph.h"

G_BEGIN_DECLS

struct Mono_Posix_Syscall__Passwd {
	/* string */ char      *pw_name;
	/* string */ char      *pw_passwd;
	/* uid_t  */ mph_uid_t  pw_uid;
	/* gid_t  */ mph_gid_t  pw_gid;
	/* string */ char      *pw_gecos;
	/* string */ char      *pw_dir;
	/* string */ char      *pw_shell;
	/* string */ char      *_pw_buf_;
};

/*
 * Copy the native `passwd' structure to it's managed representation.
 *
 * To minimize separate mallocs, all the strings are allocated within the same
 * memory block (stored in _pw_buf_).
 */
static int
copy_passwd (struct Mono_Posix_Syscall__Passwd *to, struct passwd *from)
{
	enum {PW_NAME = 0, PW_PASSWD, PW_GECOS, PW_DIR, PW_SHELL, PW_LAST};
	size_t buflen, len[PW_LAST];
	/* bool */ unsigned char copy[PW_LAST] = {0};
	const char *source[PW_LAST];
	char **dest[PW_LAST];
	int i;
	char *cur;

	to->pw_uid    = from->pw_uid;
	to->pw_gid    = from->pw_gid;

	to->pw_name   = NULL;
	to->pw_passwd = NULL;
	to->pw_gecos  = NULL;
	to->pw_dir    = NULL;
	to->pw_shell  = NULL;
	to->_pw_buf_  = NULL;

	source[PW_NAME]   = from->pw_name;
	source[PW_PASSWD] = from->pw_passwd;
	source[PW_GECOS]  = from->pw_gecos;
	source[PW_DIR]    = from->pw_dir;
	source[PW_SHELL]  = from->pw_shell;

	dest[PW_NAME]   = &to->pw_name;
	dest[PW_PASSWD] = &to->pw_passwd;
	dest[PW_GECOS]  = &to->pw_gecos;
	dest[PW_DIR]    = &to->pw_dir;
	dest[PW_SHELL]  = &to->pw_shell;

	buflen = PW_LAST;

	/* over-rigorous checking for integer overflow */
	for (i = 0; i != PW_LAST; ++i) {
		len[i] = strlen (source[i]);
		if (len[i] < INT_MAX - buflen) {
			buflen += len[i];
			copy[i] = 1;
		}
	}

	cur = to->_pw_buf_ = (char*) malloc (buflen);
	if (cur == NULL) {
		return -1;
	}

	for (i = 0; i != PW_LAST; ++i) {
		if (copy[i]) {
			*dest[i] = strcpy (cur, source[i]);
			cur += (len[i] + 1);
		}
	}

	return 0;
}

gint32
Mono_Posix_Syscall_getpwnam (const char *name, struct Mono_Posix_Syscall__Passwd *pwbuf)
{
	struct passwd *pw;

	if (pwbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	pw = getpwnam (name);
	if (pw == NULL)
		return -1;

	if (copy_passwd (pwbuf, pw) == -1) {
		errno = ENOMEM;
		return -1;
	}
	return 0;
}

gint32
Mono_Posix_Syscall_getpwuid (mph_uid_t uid, struct Mono_Posix_Syscall__Passwd *pwbuf)
{
	struct passwd *pw;

	if (pwbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	errno = 0;
	pw = getpwuid (uid);
	if (pw == NULL) {
		return -1;
	}

	if (copy_passwd (pwbuf, pw) == -1) {
		errno = ENOMEM;
		return -1;
	}
	return 0;
}

gint32
Mono_Posix_Syscall_getpwnam_r (const char *name, 
	struct Mono_Posix_Syscall__Passwd *pwbuf,
	struct passwd **pwbufp)
{
	char *buf, *buf2;
	size_t buflen;
	int r;
	struct passwd _pwbuf;

	if (pwbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	buf = buf2 = NULL;
	buflen = 2;

	do {
		buf2 = realloc (buf, buflen *= 2);
		if (buf2 == NULL) {
			free (buf);
			errno = ENOMEM;
			return -1;
		}
		buf = buf2;
	} while ((r = getpwnam_r (name, &_pwbuf, buf, buflen, pwbufp)) && r == ERANGE);

	if (r == 0 && copy_passwd (pwbuf, &_pwbuf) == -1)
		r = errno = ENOMEM;
	free (buf);

	return r;
}

gint32
Mono_Posix_Syscall_getpwuid_r (mph_uid_t uid,
	struct Mono_Posix_Syscall__Passwd *pwbuf,
	struct passwd **pwbufp)
{
	char *buf, *buf2;
	size_t buflen;
	int r;
	struct passwd _pwbuf;

	if (pwbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	buf = buf2 = NULL;
	buflen = 2;

	do {
		buf2 = realloc (buf, buflen *= 2);
		if (buf2 == NULL) {
			free (buf);
			errno = ENOMEM;
			return -1;
		}
		buf = buf2;
	} while ((r = getpwuid_r (uid, &_pwbuf, buf, buflen, pwbufp)) && r == ERANGE);

	if (r == 0 && copy_passwd (pwbuf, &_pwbuf) == -1)
		r = errno = ENOMEM;
	free (buf);

	return r;
}

gint32
Mono_Posix_Syscall_getpwent (struct Mono_Posix_Syscall__Passwd *pwbuf)
{
	struct passwd *pw;

	if (pwbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	pw = getpwent ();
	if (pw == NULL)
		return -1;

	if (copy_passwd (pwbuf, pw) == -1) {
		errno = ENOMEM;
		return -1;
	}
	return 0;
}

#ifdef HAVE_FGETPWENT
gint32
Mono_Posix_Syscall_fgetpwent (FILE *stream, struct Mono_Posix_Syscall__Passwd *pwbuf)
{
	struct passwd *pw;

	if (pwbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	pw = fgetpwent (stream);
	if (pw == NULL)
		return -1;

	if (copy_passwd (pwbuf, pw) == -1) {
		errno = ENOMEM;
		return -1;
	}
	return 0;
}
#endif /* ndef FGETPWENT */

G_END_DECLS

/*
 * vim: noexpandtab
 */
