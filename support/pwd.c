/*
 * <pwd.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004-2005 Jonathan Pryor
 */

#include <pwd.h>
#include <errno.h>
#include <string.h>
#include <stdio.h>
#include <stdlib.h>

#include "mph.h" /* Don't remove or move after map.h! Works around issues with Android SDK unified headers */
#include "map.h"

G_BEGIN_DECLS

static const mph_string_offset_t
passwd_offsets[] = {
	MPH_STRING_OFFSET (struct passwd, pw_name,    MPH_STRING_OFFSET_PTR),
	MPH_STRING_OFFSET (struct passwd, pw_passwd,  MPH_STRING_OFFSET_PTR),
#if HAVE_STRUCT_PASSWD_PW_GECOS
	MPH_STRING_OFFSET (struct passwd, pw_gecos,   MPH_STRING_OFFSET_PTR),
#endif  /* def HAVE_STRUCT_PASSWD_PW_GECOS */
	MPH_STRING_OFFSET (struct passwd, pw_dir,     MPH_STRING_OFFSET_PTR),
	MPH_STRING_OFFSET (struct passwd, pw_shell,   MPH_STRING_OFFSET_PTR)
};

static const mph_string_offset_t
mph_passwd_offsets[] = {
	MPH_STRING_OFFSET (struct Mono_Posix_Syscall__Passwd, pw_name,    MPH_STRING_OFFSET_PTR),
	MPH_STRING_OFFSET (struct Mono_Posix_Syscall__Passwd, pw_passwd,  MPH_STRING_OFFSET_PTR),
#if HAVE_STRUCT_PASSWD_PW_GECOS
	MPH_STRING_OFFSET (struct Mono_Posix_Syscall__Passwd, pw_gecos,   MPH_STRING_OFFSET_PTR),
#endif  /* def HAVE_STRUCT_PASSWD_PW_GECOS */
	MPH_STRING_OFFSET (struct Mono_Posix_Syscall__Passwd, pw_dir,     MPH_STRING_OFFSET_PTR),
	MPH_STRING_OFFSET (struct Mono_Posix_Syscall__Passwd, pw_shell,   MPH_STRING_OFFSET_PTR)
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
	char *buf;
	buf = _mph_copy_structure_strings (to, mph_passwd_offsets,
			from, passwd_offsets, sizeof(passwd_offsets)/sizeof(passwd_offsets[0]));

	to->pw_uid    = from->pw_uid;
	to->pw_gid    = from->pw_gid;

	to->_pw_buf_ = buf;
	if (buf == NULL) {
		return -1;
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

	errno = 0;
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

#ifdef HAVE_GETPWNAM_R
gint32
Mono_Posix_Syscall_getpwnam_r (const char *name, 
	struct Mono_Posix_Syscall__Passwd *pwbuf,
	void **pwbufp)
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
		errno = 0;
	} while ((r = getpwnam_r (name, &_pwbuf, buf, buflen, (struct passwd**) pwbufp)) && 
			recheck_range (r));

	if (r == 0 && !(*pwbufp))
		/* On solaris, this function returns 0 even if the entry was not found */
		r = errno = ENOENT;

	if (r == 0 && copy_passwd (pwbuf, &_pwbuf) == -1)
		r = errno = ENOMEM;
	free (buf);

	return r;
}
#endif /* ndef HAVE_GETPWNAM_R */

#ifdef HAVE_GETPWUID_R
gint32
Mono_Posix_Syscall_getpwuid_r (mph_uid_t uid,
	struct Mono_Posix_Syscall__Passwd *pwbuf,
	void **pwbufp)
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
		errno = 0;
	} while ((r = getpwuid_r (uid, &_pwbuf, buf, buflen, (struct passwd**) pwbufp)) && 
			recheck_range (r));

	if (r == 0 && !(*pwbufp))
		/* On solaris, this function returns 0 even if the entry was not found */
		r = errno = ENOENT;

	if (r == 0 && copy_passwd (pwbuf, &_pwbuf) == -1)
		r = errno = ENOMEM;
	free (buf);

	return r;
}
#endif /* ndef HAVE_GETPWUID_R */

#if HAVE_GETPWENT
gint32
Mono_Posix_Syscall_getpwent (struct Mono_Posix_Syscall__Passwd *pwbuf)
{
	struct passwd *pw;

	if (pwbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	errno = 0;
	pw = getpwent ();
	if (pw == NULL)
		return -1;

	if (copy_passwd (pwbuf, pw) == -1) {
		errno = ENOMEM;
		return -1;
	}
	return 0;
}
#endif  /* def HAVE_GETPWENT */

#ifdef HAVE_FGETPWENT
gint32
Mono_Posix_Syscall_fgetpwent (void *stream, struct Mono_Posix_Syscall__Passwd *pwbuf)
{
	struct passwd *pw;

	if (pwbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	errno = 0;
	pw = fgetpwent ((FILE*) stream);
	if (pw == NULL)
		return -1;

	if (copy_passwd (pwbuf, pw) == -1) {
		errno = ENOMEM;
		return -1;
	}
	return 0;
}
#endif /* ndef HAVE_FGETPWENT */

#if HAVE_SETPWENT
int
Mono_Posix_Syscall_setpwent (void)
{
	errno = 0;
	do {
		setpwent ();
	} while (errno == EINTR);
	mph_return_if_val_in_list5(errno, EIO, EMFILE, ENFILE, ENOMEM, ERANGE);
	return 0;
}
#endif  /* def HAVE_SETPWENT */

#if HAVE_ENDPWENT
int
Mono_Posix_Syscall_endpwent (void)
{
	errno = 0;
	endpwent ();
	if (errno == EIO)
		return -1;
	return 0;
}
#endif  /* def HAVE_ENDPWENT */

G_END_DECLS

/*
 * vim: noexpandtab
 */
