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

#include "map.h"

G_BEGIN_DECLS

gint32
Mono_Posix_Syscall_mount (const char *source, const char *target, 
		const char *filesystemtype, guint64 mountflags, const void *data)
{
	if (Mono_Posix_FromMountFlags (mountflags, &mountflags) == -1)
		return -1;

	return mount (source, target, filesystemtype, (unsigned long) mountflags, data);
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
