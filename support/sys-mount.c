/*
 * <sys/mount.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#include <sys/mount.h>
#include <glib/gtypes.h>

#include "mph.h"
#include "map.h"

G_BEGIN_DECLS

gint32
Mono_Posix_Syscall_mount (const char *source, const char *target, 
		const char *filesystemtype, guint64 mountflags, void *data)
{
	if (Mono_Posix_FromMountFlags (mountflags, &mountflags) == -1)
		return -1;

#ifdef MPH_ON_BSD
	return mount (filesystemtype, target, mountflags, data);
#else
	return mount (source, target, filesystemtype, (unsigned long) mountflags, data);
#endif
}

gint32
Mono_Posix_Syscall_umount (const char *source)
{
#ifdef MPH_ON_BSD
	return unmount (source, MNT_FORCE);
#else
	return umount (source);
#endif
}

gint32
Mono_Posix_Syscall_umount2 (const char *source, gint32 flags)
{
#ifdef MPH_ON_BSD
	return unmount (source, flags);
#else
	return umount2 (source, flags);
#endif
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
