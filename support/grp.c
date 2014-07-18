/*
 * <grp.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004-2005 Jonathan Pryor
 */

#include <sys/types.h>
#ifdef HAVE_SYS_PARAM_H
#include <sys/param.h>
#endif
#include <grp.h>
#include <stdio.h>
#include <stdlib.h>
#include <errno.h>
#include <string.h>
#include <unistd.h>	/* for setgroups on Mac OS X */

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

static void
count_members (char **gr_mem, int *count, size_t *mem)
{
	char *cur;
	*count = 0;

	// ensure that later (*mem)+1 doesn't result in integer overflow
	if (*mem > INT_MAX - 1)
		return;

	for (cur = *gr_mem; cur != NULL; cur = *++gr_mem) {
		size_t len;
		len = strlen (cur);

		if (!(len < INT_MAX - ((*mem) + 1)))
			break;

		++(*count);
		*mem += (len + 1);
	}
}

static int
copy_group (struct Mono_Posix_Syscall__Group *to, struct group *from)
{
	size_t nlen, plen, buflen;
	int i, count;
	char *cur, **to_mem;

	to->gr_gid    = from->gr_gid;

	to->gr_name   = NULL;
	to->gr_passwd = NULL;
	to->gr_mem    = NULL;
	to->_gr_buf_  = NULL;

	nlen = strlen (from->gr_name);
	plen = strlen (from->gr_passwd);

	buflen = 2;

	if (!(nlen < INT_MAX - buflen))
		return -1;
	buflen += nlen;

	if (!(plen < INT_MAX - buflen))
		return -1;
	buflen += plen;

	count = 0;
	count_members (from->gr_mem, &count, &buflen);

	to->_gr_nmem_ = count;
	cur = to->_gr_buf_ = (char*) malloc (buflen);
	to_mem = to->gr_mem = malloc (sizeof(char*)*(count+1));
	if (to->_gr_buf_ == NULL || to->gr_mem == NULL) {
		free (to->_gr_buf_);
		free (to->gr_mem);
		return -1;
	}

	to->gr_name = strcpy (cur, from->gr_name);
	cur += (nlen + 1);
	to->gr_passwd = strcpy (cur, from->gr_passwd);
	cur += (plen + 1);

	for (i = 0; i != count; ++i) {
		to_mem [i] = strcpy (cur, from->gr_mem[i]);
		cur += (strlen (from->gr_mem[i])+1);
	}
	to_mem [i] = NULL;

	return 0;
}

gint32
Mono_Posix_Syscall_getgrnam (const char *name, struct Mono_Posix_Syscall__Group *gbuf)
{
	struct group *_gbuf;

	if (gbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	errno = 0;
	_gbuf = getgrnam (name);
	if (_gbuf == NULL)
		return -1;

	if (copy_group (gbuf, _gbuf) == -1) {
		errno = ENOMEM;
		return -1;
	}
	return 0;
}

gint32
Mono_Posix_Syscall_getgrgid (mph_gid_t gid, struct Mono_Posix_Syscall__Group *gbuf)
{
	struct group *_gbuf;

	if (gbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	errno = 0;
	_gbuf = getgrgid (gid);
	if (_gbuf == NULL)
		return -1;

	if (copy_group (gbuf, _gbuf) == -1) {
		errno = ENOMEM;
		return -1;
	}
	return 0;
}

#ifdef HAVE_GETGRNAM_R
gint32
Mono_Posix_Syscall_getgrnam_r (const char *name, 
	struct Mono_Posix_Syscall__Group *gbuf,
	void **gbufp)
{
	char *buf, *buf2;
	size_t buflen;
	int r;
	struct group _grbuf;

	if (gbuf == NULL) {
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
	} while ((r = getgrnam_r (name, &_grbuf, buf, buflen, (struct group**) gbufp)) && 
			recheck_range (r));

	/* On Solaris, this function returns 0 even if the entry was not found */
	if (r == 0 && !(*gbufp))
		r = errno = ENOENT;

	if (r == 0 && copy_group (gbuf, &_grbuf) == -1)
		r = errno = ENOMEM;
	free (buf);

	return r;
}
#endif /* ndef HAVE_GETGRNAM_R */

#ifdef HAVE_GETGRGID_R
gint32
Mono_Posix_Syscall_getgrgid_r (mph_gid_t gid,
	struct Mono_Posix_Syscall__Group *gbuf,
	void **gbufp)
{
	char *buf, *buf2;
	size_t buflen;
	int r;
	struct group _grbuf;

	if (gbuf == NULL) {
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
	} while ((r = getgrgid_r (gid, &_grbuf, buf, buflen, (struct group**) gbufp)) && 
			recheck_range (r));

	/* On Solaris, this function returns 0 even if the entry was not found */
	if (r == 0 && !(*gbufp))
		r = errno = ENOENT;

	if (r == 0 && copy_group (gbuf, &_grbuf) == -1)
		r = errno = ENOMEM;
	free (buf);

	return r;
}
#endif /* ndef HAVE_GETGRGID_R */

#if HAVE_GETGRENT
gint32
Mono_Posix_Syscall_getgrent (struct Mono_Posix_Syscall__Group *grbuf)
{
	struct group *gr;

	if (grbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	errno = 0;
	gr = getgrent ();
	if (gr == NULL)
		return -1;

	if (copy_group (grbuf, gr) == -1) {
		errno = ENOMEM;
		return -1;
	}
	return 0;
}
#endif  /* def HAVE_GETGRENT */

#ifdef HAVE_FGETGRENT
gint32
Mono_Posix_Syscall_fgetgrent (void *stream, struct Mono_Posix_Syscall__Group *grbuf)
{
	struct group *gr;

	if (grbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	errno = 0;
	gr = fgetgrent ((FILE*) stream);
	if (gr == NULL)
		return -1;

	if (copy_group (grbuf, gr) == -1) {
		errno = ENOMEM;
		return -1;
	}
	return 0;
}
#endif /* ndef HAVE_FGETGRENT */

#if HAVE_SETGROUPS
gint32
Mono_Posix_Syscall_setgroups (mph_size_t size, mph_gid_t *list)
{
	mph_return_if_size_t_overflow (size);
	return setgroups ((size_t) size, list);
}
#endif  /* def HAVE_SETGROUPS */

#if HAVE_SETGRENT
int
Mono_Posix_Syscall_setgrent (void)
{
	errno = 0;
	do {
		setgrent ();
	} while (errno == EINTR);
	mph_return_if_val_in_list5(errno, EIO, EMFILE, ENFILE, ENOMEM, ERANGE);
	return 0;
}
#endif  /* def HAVE_SETGRENT */

#if HAVE_ENDGRENT
int
Mono_Posix_Syscall_endgrent (void)
{
	endgrent();
	return 0;
}
#endif  /* def HAVE_ENDGRENT */


G_END_DECLS

/*
 * vim: noexpandtab
 */
