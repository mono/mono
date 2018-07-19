/*
 * Wrapper functions for <sys/xattr.h> (or <attr/xattr.h>) and <sys/extattr.h>
 *
 * Authors:
 *   Daniel Drake (dsd@gentoo.org)
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2005 Daniel Drake
 * Copyright (C) 2006 Jonathan Pryor
 */

#include <config.h>

//If we're compiling to API level < 16 this won't be available
#if defined (HOST_ANDROID) && __ANDROID_API__ < 16
#define ANDROID_NO_XATTR
#endif

#if (defined(HAVE_SYS_XATTR_H) || defined(HAVE_ATTR_ATTR_H) || defined(HAVE_SYS_EXTATTR_H)) && !defined (ANDROID_NO_XATTR)

#include <sys/types.h>

/*
 * Where available, we prefer to use the libc implementation of the xattr
 * syscalls. However, we also support using libattr for this on systems where
 * libc does not provide this (e.g. glibc-2.2 and older)
 * (configure-time magic is used to select which library to link to)
 */
#ifdef HAVE_SYS_XATTR_H
// libc
#include <sys/xattr.h>
#define EA_UNIX
#elif HAVE_ATTR_ATTR_H
// libattr
#include <attr/xattr.h>
#define EA_UNIX
#endif /* HAVE_SYS_XATTR_H */

#ifdef HAVE_SYS_EXTATTR_H
#include <sys/extattr.h>
#include <sys/uio.h>
#define EA_BSD
#endif

#include <unistd.h>
#include <fcntl.h>
#include <errno.h>
#include <string.h>
#include <stdlib.h>

#include "map.h"
#include "mph.h"

/*
 * Linux provides extended attributes through the <sys/xattr.h> API.
 * Any file or link can have attributes assigned to it (provided that they are
 * supported by the backing filesystem). Each attribute has to be placed in a
 * namespace, of which "user" is the most common. Namespaces are specified as
 * a prefix to the attribute name, proceeded by a '.' (e.g. user.myattribute)
 *
 * FreeBSD provides extended attributes through the <sys/extattr.h> API.
 * Behaviour is very similar to Linux EA's, but the namespace is specified
 * through an enum-style parameter rather than as a prefix to an attribute
 * name. There are also differences in the behaviour of the "list attributes"
 * system calls.
 *
 * This file merges the two implementations into a single API for use by the
 * Mono.Unix.Syscall.*xattr methods. No matter which OS you are on, things
 * should "just work" the same as anywhere else.
 *
 * The API provided here leans more towards the Linux implementation. Attribute
 * namespaces are provided as prefixes to the attribute name (followed by '.').
 * There is no limit to the namespaces accepted by the Linux side of this
 * implementation, but you are obviously limited to the ones available to you
 * on the system.
 * FreeBSD namespaces have to be converted from the textual prefix into their
 * relevant number so that they can be used in the FreeBSD system calls.
 * This means that the only namespaces available are the ones known by in this
 * file (see bsd_extattr_namespaces). However, you can also specify the
 * numericalnamespace index yourself, by using an attribute name such as
 * "5.myattr".
 * (this will obviously fail on Linux, your code will no longer be 'portable')
 *
 * Linux {,l,f}setxattr calls have a flags parameter which allow you to control
 * what should happen if an attribute with the same name does (or doesn't)
 * already exist. The 'flags' parameter is available here, but because FreeBSD
 * does not support this kind of refinement, it will fail on FreeBSD if you
 * specify anything other than XATTR_AUTO (XATTR_AUTO will create the attribute
 * if it doesn't already exist, and overwrite the existing attribute if it
 * already set).
 * 
 * For usage and behaviour information, see the monodoc documentation on the
 * Mono.Unix.Syscall class.
 */

G_BEGIN_DECLS

//
// HELPER FUNCTIONS
//

#ifdef EA_BSD

struct BsdNamespaceInfo {
	const char *name;
	int value;
};

static struct BsdNamespaceInfo bsd_extattr_namespaces[] = {
	{"user"         , EXTATTR_NAMESPACE_USER},
	{"system"       , EXTATTR_NAMESPACE_SYSTEM}
};

static int bsd_check_flags (gint32 flags)
{
	// BSD doesn't support flags, but always provides the same behaviour as
	// XATTR_AUTO. So we enforce that here.
	if (flags != Mono_Posix_XattrFlags_XATTR_AUTO) {
		errno = EINVAL;
		return -1;
	}
	return 0;
}

// On FreeBSD, we need to convert "user.blah" into namespace 1 and attribute
// name "blah", or maybe "6.blah" into namespace 6 attribute "blah"
static int
bsd_handle_nsprefix (const char *name, char **_name, int *namespace)
{
	int i;
	gchar **components = g_strsplit (name, ".", 2);

	// Find namespace number from textual representation
	for (i = 0; i < G_N_ELEMENTS(bsd_extattr_namespaces); i++)
		if (strcmp (bsd_extattr_namespaces[i].name, components[0]) == 0) {
			*namespace = bsd_extattr_namespaces[i].value;
			break;
		}

	if (*namespace == 0) {
		// Perhaps they specified the namespace number themselves..?
		char *endptr;
		*namespace = (int) strtol (components[0], &endptr, 10);
		if (*endptr != '\0')
			return -1;
	}

	*_name = g_strdup (components[1]);
	g_strfreev (components);
	return 0;
}

static void
init_attrlists (char *attrlists[])
{
	memset (attrlists, 0, G_N_ELEMENTS(bsd_extattr_namespaces) * sizeof(char*));
}

static void
free_attrlists (char *attrlists[])
{
	int i;
	for (i = 0; i < G_N_ELEMENTS(bsd_extattr_namespaces); i++)
		g_free (attrlists[i]);
}

// Counts the number of attributes in the result of a
// extattr_list_*() call. Note that the format of the data
// is: \3one\3two\6eleven where the leading charaters represent the length
// of the following attribute. (the description in the man-page is wrong)
static unsigned int
count_num_attrs (char *attrs, size_t size)
{
	size_t i = 0;
	unsigned int num_attrs = 0;

	if (!attrs || !size)
		return 0;

	while (i < size) {
		num_attrs++;
		i += attrs[i] + 1;
	}

	return num_attrs;
}

// Convert a BSD-style list buffer (see the description for count_num_attrs)
// into a Linux-style NULL-terminated list including namespace prefix.
static char
*bsd_convert_list (const char *nsprefix, const char *src, size_t size, char *dest)
{
	size_t i = 0;
	if (src == NULL || dest == NULL || size == 0)
		return NULL;

	while (i < size) {
		// Read length
		int attr_len = (int) src[i];
		int prefix_len = strlen (nsprefix);

		// Add namespace prefix
		strncpy (dest, nsprefix, prefix_len);
		dest[prefix_len] = '.';
		dest += prefix_len + 1;

		// Copy attribute
		memcpy(dest, src + ++i, attr_len);

		// NULL-terminate
		i += attr_len;
		dest[attr_len] = '\0';
		dest += attr_len + 1;
	}

	return dest;
}

// Combine all the lists of attributes that we know about into a single
// Linux-style buffer
static ssize_t
bsd_combine_lists (char *attrlists[], char *dest, size_t dest_size_needed, size_t dest_size)
{
	int i;
	if (!dest)
		return dest_size_needed;

	if (dest_size < dest_size_needed) {
		errno = ERANGE;
		return -1;
	}

	for (i = 0; i < G_N_ELEMENTS(bsd_extattr_namespaces); i++)
		if (attrlists[i])
			dest = bsd_convert_list (bsd_extattr_namespaces[i].name, attrlists[i], strlen (attrlists[i]), dest);

	return dest_size_needed;
}

static mph_ssize_t
bsd_listxattr (const char *path, void *list, mph_size_t size)
{
	size_t full_size = 0;
	int i;
	char *attrlists[G_N_ELEMENTS(bsd_extattr_namespaces)];

	init_attrlists (attrlists);
	for (i = 0; i < G_N_ELEMENTS(bsd_extattr_namespaces); i++) {
		size_t buf_size;
		int num_attrs;

		buf_size = (size_t) extattr_list_file (path, i + 1, NULL, 0);
		if (buf_size == -1)
			continue;

		attrlists[i] = g_malloc0 (buf_size + 1);
		buf_size = (size_t) extattr_list_file (path, i + 1, attrlists[i], buf_size);
		if (buf_size == -1)
			continue;

		num_attrs = count_num_attrs(attrlists[i], buf_size);
		full_size += buf_size + (num_attrs * (strlen (bsd_extattr_namespaces[i].name) + 1));
	}

	full_size = bsd_combine_lists (attrlists, (char *) list, full_size, size);
	free_attrlists (attrlists);
	return full_size;
}

static mph_ssize_t
bsd_llistxattr (const char *path, void *list, mph_size_t size)
{
	size_t full_size = 0;
	int i;
	char *attrlists[G_N_ELEMENTS(bsd_extattr_namespaces)];

	init_attrlists (attrlists);
	for (i = 0; i < G_N_ELEMENTS(bsd_extattr_namespaces); i++) {
		size_t buf_size;
		int num_attrs;

		buf_size = (size_t) extattr_list_link (path, i + 1, NULL, 0);
		if (buf_size == -1)
			continue;

		attrlists[i] = g_malloc0 (buf_size + 1);
		buf_size = (size_t) extattr_list_link (path, i + 1, attrlists[i], buf_size);
		if (buf_size == -1)
			continue;

		num_attrs = count_num_attrs(attrlists[i], buf_size);
		full_size += buf_size + (num_attrs * (strlen (bsd_extattr_namespaces[i].name) + 1));
	}

	full_size = bsd_combine_lists (attrlists, (char *) list, full_size, size);
	free_attrlists (attrlists);
	return full_size;
}

static mph_ssize_t
bsd_flistxattr (int fd, void *list, mph_size_t size)
{
	size_t full_size = 0;
	int i;
	char *attrlists[G_N_ELEMENTS(bsd_extattr_namespaces)];

	init_attrlists (attrlists);
	for (i = 0; i < G_N_ELEMENTS(bsd_extattr_namespaces); i++) {
		size_t buf_size;
		int num_attrs;

		buf_size = (size_t) extattr_list_fd (fd, i + 1, NULL, 0);
		if (buf_size == -1)
			continue;

		attrlists[i] = g_malloc0 (buf_size + 1);
		buf_size = (size_t) extattr_list_fd (fd, i + 1, attrlists[i], buf_size);
		if (buf_size == -1)
			continue;

		num_attrs = count_num_attrs(attrlists[i], buf_size);
		full_size += buf_size + (num_attrs * (strlen (bsd_extattr_namespaces[i].name) + 1));
	}

	full_size = bsd_combine_lists (attrlists, (char *) list, full_size, size);
	free_attrlists (attrlists);
	return full_size;
}

#endif /* EA_BSD */

//
// THE PROVIDED API
//

gint32
Mono_Posix_Syscall_setxattr (const char *path, const char *name, unsigned char *value, mph_size_t size, gint32 flags)
{
	gint32 ret;
	mph_return_if_size_t_overflow (size);

#ifdef EA_UNIX
	{
		int _flags;
		if (Mono_Posix_FromXattrFlags (flags, &_flags) == -1)
			return -1;
#if __APPLE__
		ret = setxattr (path, name, value, (size_t) size, 0, _flags);
#else /* __APPLE__ */
		ret = setxattr (path, name, value, (size_t) size, _flags);
#endif /* __APPLE__ */
	}
#else /* EA_UNIX */
	{
		char *_name;
		int namespace;
		if (bsd_check_flags (flags) == -1)
			return -1;
		if (bsd_handle_nsprefix (name, &_name, &namespace) == -1)
			return -1;
		ret = extattr_set_file (path, namespace, _name, value, (size_t) size);
		g_free (_name);
	}
#endif /* EA_UNIX */

	return ret;
}

#if !__APPLE__
gint32
Mono_Posix_Syscall_lsetxattr (const char *path, const char *name, unsigned char *value, mph_size_t size, gint32 flags)
{
	gint32 ret;
	mph_return_if_size_t_overflow (size);

#ifdef EA_UNIX
	{
		int _flags;
		if (Mono_Posix_FromXattrFlags (flags, &_flags) == -1)
			return -1;
		ret = lsetxattr (path, name, value, size, _flags);
	}
#else /* EA_UNIX */
	{
		char *_name;
		int namespace;
		if (bsd_check_flags (flags) == -1)
			return -1;
		if (bsd_handle_nsprefix (name, &_name, &namespace) == -1)
			return -1;
		ret = extattr_set_link (path, namespace, _name, value, (size_t) size);
		g_free (_name);
	}
#endif /* EA_UNIX */

	return ret;
}
#endif /* !__APPLE__ */

gint32
Mono_Posix_Syscall_fsetxattr (int fd, const char *name, unsigned char *value, mph_size_t size, gint32 flags)
{
	gint32 ret;
	mph_return_if_size_t_overflow (size);

#ifdef EA_UNIX
	{
		int _flags;
		if (Mono_Posix_FromXattrFlags (flags, &_flags) == -1)
			return -1;
#if __APPLE__
		ret = fsetxattr (fd, name, value, (size_t) size, 0, _flags);
#else /* __APPLE__ */
		ret = fsetxattr (fd, name, value, (size_t) size, _flags);
#endif /* __APPLE__ */
	}
#else /* EA_UNIX */
	{
		char *_name;
		int namespace;
		if (bsd_check_flags (flags) == -1)
			return -1;
		if (bsd_handle_nsprefix (name, &_name, &namespace) == -1)
			return -1;
		ret = extattr_set_fd (fd, namespace, _name, value, (size_t) size);
		g_free (_name);
	}
#endif /* EA_UNIX */

	return ret;
}

mph_ssize_t
Mono_Posix_Syscall_getxattr (const char *path, const char *name, unsigned char *value, mph_size_t size)
{
	mph_ssize_t ret;
	mph_return_if_size_t_overflow (size);

#ifdef EA_UNIX
#if __APPLE__
	ret = getxattr (path, name, value, (size_t) size, 0, 0);
#else /* __APPLE__ */
	ret = getxattr (path, name, value, (size_t) size);
#endif /* __APPLE__ */
#else /* EA_UNIX */
	{
		char *_name;
		int namespace;
		if (bsd_handle_nsprefix (name, &_name, &namespace) == -1)
			return -1;
		ret = extattr_get_file (path, namespace, _name, value, (size_t) size);
		g_free (_name);
	}
#endif /* EA_UNIX */

	return ret;
}

#if !__APPLE__
mph_ssize_t
Mono_Posix_Syscall_lgetxattr (const char *path, const char *name, unsigned char *value, mph_size_t size)
{
	mph_ssize_t ret;
	mph_return_if_size_t_overflow (size);

#ifdef EA_UNIX
	ret = lgetxattr (path, name, value, (size_t) size);
#else /* EA_UNIX */
	{
		char *_name;
		int namespace;
		if (bsd_handle_nsprefix (name, &_name, &namespace) == -1)
			return -1;
		ret = extattr_get_link (path, namespace, _name, value, (size_t) size);
		g_free (_name);
	}
#endif /* EA_UNIX */

	return ret;
}
#endif /* !__APPLE__ */

mph_ssize_t
Mono_Posix_Syscall_fgetxattr (int fd, const char *name, unsigned char *value, mph_size_t size)
{
	mph_ssize_t ret;
	mph_return_if_size_t_overflow (size);

#ifdef EA_UNIX
#if __APPLE__
	ret = fgetxattr (fd, name, value, (size_t) size, 0, 0);
#else /* __APPLE__ */
	ret = fgetxattr (fd, name, value, (size_t) size);
#endif /* __APPLE__ */
#else /* EA_UNIX */
	{
		char *_name;
		int namespace;
		if (bsd_handle_nsprefix (name, &_name, &namespace) == -1)
			return -1;
		ret = extattr_get_fd (fd, namespace, _name, value, (size_t) size);
		g_free (_name);
	}
#endif /* EA_UNIX */

	return ret;
}

mph_ssize_t
Mono_Posix_Syscall_listxattr (const char *path, unsigned char *list, mph_size_t size)
{
	mph_return_if_size_t_overflow (size);

#ifdef EA_UNIX
#if __APPLE__
	return listxattr (path, (char*) list, (size_t) size, 0);
#else /* __APPLE__ */
	return listxattr (path, (char*) list, (size_t) size);
#endif /* __APPLE__ */
#else /* EA_UNIX */
	return bsd_listxattr (path, list, size);
#endif /* EA_UNIX */
}

#if !__APPLE__
mph_ssize_t
Mono_Posix_Syscall_llistxattr (const char *path, unsigned char *list, mph_size_t size)
{
	mph_return_if_size_t_overflow (size);

#ifdef EA_UNIX
	return llistxattr (path, (char*) list, (size_t) size);
#else /* EA_UNIX */
	return bsd_llistxattr (path, list, size);
#endif /* EA_UNIX */
}
#endif /* !__APPLE__ */

mph_ssize_t
Mono_Posix_Syscall_flistxattr (int fd, unsigned char *list, mph_size_t size)
{
	mph_return_if_size_t_overflow (size);

#ifdef EA_UNIX
#if __APPLE__
	return flistxattr (fd, (char*) list, (size_t) size, 0);
#else /* __APPLE__ */
	return flistxattr (fd, (char*) list, (size_t) size);
#endif /* __APPLE__ */
#else /* EA_UNIX */
	return bsd_flistxattr (fd, list, size);
#endif /* EA_UNIX */
}

gint32
Mono_Posix_Syscall_removexattr (const char *path, const char *name)
{
	gint32 ret;

#ifdef EA_UNIX
#if __APPLE__
	ret = removexattr (path, name, 0);
#else /* __APPLE__ */
	ret = removexattr (path, name);
#endif /* __APPLE__ */
#else /* EA_UNIX */
	{
		char *_name;
		int namespace;
		if (bsd_handle_nsprefix (name, &_name, &namespace) == -1)
			return -1;
		ret = extattr_delete_file (path, namespace, _name);
		g_free (_name);
	}
#endif /* EA_UNIX */

	return ret;
}

#if !__APPLE__
gint32
Mono_Posix_Syscall_lremovexattr (const char *path, const char *name)
{
	gint32 ret;

#ifdef EA_UNIX
	ret = lremovexattr (path, name);
#else /* EA_UNIX */
	{
		char *_name;
		int namespace;
		if (bsd_handle_nsprefix (name, &_name, &namespace) == -1)
			return -1;
		ret = extattr_delete_link (path, namespace, _name);
		g_free (_name);
	}
#endif /* EA_UNIX */

	return ret;
}
#endif /* !__APPLE__ */

gint32
Mono_Posix_Syscall_fremovexattr (int fd, const char *name)
{
	gint32 ret;

#ifdef EA_UNIX
#if __APPLE__
	ret = fremovexattr (fd, name, 0);
#else /* __APPLE__ */
	ret = fremovexattr (fd, name);
#endif /* __APPLE__ */
#else /* EA_UNIX */
	{
		char *_name;
		int namespace;
		if (bsd_handle_nsprefix (name, &_name, &namespace) == -1)
			return -1;
		ret = extattr_delete_fd (fd, namespace, _name);
		g_free (_name);
	}
#endif /* EA_UNIX */

	return ret;
}

G_END_DECLS

#endif /* HAVE_SYS_XATTR_H || HAVE_ATTR_ATTR_H || HAVE_SYS_EXTATTR_H */

/*
 * vim: noexpandtab
 */
