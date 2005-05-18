/*
 * <sys/xattr.h> wrapper functions.
 *
 * Authors:
 *   Daniel Drake (dsd@gentoo.org)
 *
 * Copyright (C) 2005 Daniel Drake
 */

#include <config.h>

#ifdef HAVE_SYS_XATTR_H

#include <sys/types.h>
#include <sys/xattr.h>
#include <unistd.h>
#include <fcntl.h>
#include <errno.h>

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

gint32
Mono_Posix_Syscall_setxattr (const char *path, const char *name, void *value, mph_size_t size, gint32 flags)
{
	int _flags;
	mph_return_if_size_t_overflow (size);

	if (Mono_Posix_FromXattrFlags (flags, &_flags) == -1)
		return -1;

#if __APPLE__
	return setxattr (path, name, value, size, 0, _flags);
#else
	return setxattr (path, name, value, size, _flags);
#endif
}

#if !__APPLE__
gint32
Mono_Posix_Syscall_lsetxattr (const char *path, const char *name, void *value, mph_size_t size, gint32 flags)
{
	int _flags;
	mph_return_if_size_t_overflow (size);

	if (Mono_Posix_FromXattrFlags (flags, &_flags) == -1)
		return -1;

	return lsetxattr (path, name, value, size, _flags);
}
#endif

gint32
Mono_Posix_Syscall_fsetxattr (int fd, const char *name, void *value, mph_size_t size, gint32 flags)
{
	int _flags;
	mph_return_if_size_t_overflow (size);

	if (Mono_Posix_FromXattrFlags (flags, &_flags) == -1)
		return -1;

#if __APPLE__
	return fsetxattr (fd, name, value, (size_t) size, 0, _flags);
#else
	return fsetxattr (fd, name, value, (size_t) size, _flags);
#endif

}

mph_ssize_t
Mono_Posix_Syscall_getxattr (const char *path, const char *name, void *value, mph_size_t size)
{
	mph_return_if_size_t_overflow (size);
#if __APPLE__
	return getxattr (path, name, value, (size_t) size, 0, 0);
#else
	return getxattr (path, name, value, (size_t) size);
#endif
}

#if !__APPLE__
mph_ssize_t
Mono_Posix_Syscall_lgetxattr (const char *path, const char *name, void *value, mph_size_t size)
{
	mph_return_if_size_t_overflow (size);
	return lgetxattr (path, name, value, (size_t) size);
}
#endif

mph_ssize_t
Mono_Posix_Syscall_fgetxattr (int fd, const char *name, void *value, mph_size_t size)
{
	mph_return_if_size_t_overflow (size);
#if __APPLE__
	return fgetxattr (fd, name, value, (size_t) size, 0, 0);
#else
	return fgetxattr (fd, name, value, (size_t) size);
#endif
}

mph_ssize_t
Mono_Posix_Syscall_listxattr (const char *path, void *list, mph_size_t size)
{
	mph_return_if_size_t_overflow (size);
#if __APPLE__
	return listxattr (path, (char *) list, (size_t) size, 0);
#else
	return listxattr (path, (char *) list, (size_t) size);
#endif
}

#if !__APPLE__
mph_ssize_t
Mono_Posix_Syscall_llistxattr (const char *path, void *list, mph_size_t size)
{
	mph_return_if_size_t_overflow (size);
	return llistxattr (path, (char *) list, (size_t) size);
}
#endif

mph_ssize_t
Mono_Posix_Syscall_flistxattr (int fd, void *list, mph_size_t size)
{
	mph_return_if_size_t_overflow (size);
#if __APPLE__
	return flistxattr (fd, (char *) list, (size_t) size, 0);
#else
	return flistxattr (fd, (char *) list, (size_t) size);
#endif
}

G_END_DECLS

#endif /* def HAVE_ATTR_XATTR_H */

/*
 * vim: noexpandtab
 */
