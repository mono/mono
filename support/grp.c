/*
 * <grp.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#include <sys/types.h>
#include <sys/param.h>
#include <grp.h>
#include <stdio.h>
#include <stdlib.h>
#include <errno.h>
#include <string.h>
#include <unistd.h>	/* for setgroups on Mac OS X */

#include "mph.h"

G_BEGIN_DECLS

struct Mono_Posix_Syscall__Group {
	/* string */  char     *gr_name;
	/* string */  char     *gr_passwd;
	/* gid_t  */  mph_gid_t gr_gid;
	/* int    */  int       _gr_nmem_;
	/* string */  char    **gr_mem;
	/* string */  char     *_gr_buf_;  /* holds all but gr_mem */
};

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
	char *cur;

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
	to->gr_mem = (char **) malloc (sizeof(char*)*(count+1));
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
		to->gr_mem[i] = strcpy (cur, from->gr_mem[i]);
		cur += (strlen (from->gr_mem[i])+1);
	}
	to->gr_mem[i] = NULL;

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
	struct group **gbufp)
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
	} while ((r = getgrnam_r (name, &_grbuf, buf, buflen, gbufp)) && 
			recheck_range (r));

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
	struct group **gbufp)
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
	} while ((r = getgrgid_r (gid, &_grbuf, buf, buflen, gbufp)) && 
			recheck_range (r));

	if (r == 0 && copy_group (gbuf, &_grbuf) == -1)
		r = errno = ENOMEM;
	free (buf);

	return r;
}
#endif /* ndef HAVE_GETGRGID_R */

gint32
Mono_Posix_Syscall_getgrent (struct Mono_Posix_Syscall__Group *grbuf)
{
	struct group *gr;

	if (grbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	gr = getgrent ();
	if (gr == NULL)
		return -1;

	if (copy_group (grbuf, gr) == -1) {
		errno = ENOMEM;
		return -1;
	}
	return 0;
}

#ifdef HAVE_FGETGRENT
gint32
Mono_Posix_Syscall_fgetgrent (FILE *stream, struct Mono_Posix_Syscall__Group *grbuf)
{
	struct group *gr;

	if (grbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	gr = fgetgrent (stream);
	if (gr == NULL)
		return -1;

	if (copy_group (grbuf, gr) == -1) {
		errno = ENOMEM;
		return -1;
	}
	return 0;
}
#endif /* ndef HAVE_FGETGRENT */

gint32
Mono_Posix_Syscall_setgroups (mph_size_t size, mph_gid_t *list)
{
	mph_return_if_size_t_overflow (size);
	return setgroups ((size_t) size, list);
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
