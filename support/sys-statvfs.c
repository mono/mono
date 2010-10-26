
/*
 * <sys/sendfile.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004-2006 Jonathan Pryor
 */

#include <errno.h>

#include <string.h>

#include "mph.h"
#include "map.h"

#ifdef HAVE_PATHCONF_H
#include <pathconf.h>
#endif

#ifdef HAVE_SYS_STATVFS_H
#include <sys/statvfs.h>
#elif defined (HAVE_STATFS) || defined (HAVE_FSTATFS)

#if !defined(PLATFORM_ANDROID)
#include <sys/stat.h>  //lucas had to change this for 10.3 compatibility. Apparently on some linux machines stat is defined in sys/vfs.h, but on 10.6, which is our current mono build platform, it's in sys/stat.h
#else
#include <sys/vfs.h>  // yes, but on android stat is _still_ in sys/vfs.h...
#endif

#endif /* def HAVE_SYS_STATVFS_H */

#ifdef HAVE_GETFSSTAT
#include <sys/param.h>
#include <sys/ucred.h>
#include <sys/mount.h>
#include <unistd.h>     /* for pathconf */
#endif /* def HAVE_GETFSSTAT */

G_BEGIN_DECLS

#ifdef HAVE_SYS_STATVFS_H
int
Mono_Posix_ToStatvfs (void *_from, struct Mono_Posix_Statvfs *to)
{
	struct statvfs *from = _from;

	to->f_bsize   = from->f_bsize;
	to->f_frsize  = from->f_frsize;
	to->f_blocks  = from->f_blocks;
	to->f_bfree   = from->f_bfree;
	to->f_bavail  = from->f_bavail;
	to->f_files   = from->f_files;
	to->f_ffree   = from->f_ffree;
	to->f_favail  = from->f_favail;
	to->f_fsid    = from->f_fsid;
	to->f_namemax =	from->f_namemax;

	if (Mono_Posix_ToMountFlags (from->f_flag, &to->f_flag) != 0)
		return -1;

	return 0;
}

int
Mono_Posix_FromStatvfs (struct Mono_Posix_Statvfs *from, void *_to)
{
	struct statvfs *to = _to;
	guint64 flag;

	to->f_bsize   = from->f_bsize;
	to->f_frsize  = from->f_frsize;
	to->f_blocks  = from->f_blocks;
	to->f_bfree   = from->f_bfree;
	to->f_bavail  = from->f_bavail;
	to->f_files   = from->f_files;
	to->f_ffree   = from->f_ffree;
	to->f_favail  = from->f_favail;
	to->f_fsid    = from->f_fsid;
	to->f_namemax =	from->f_namemax;

	if (Mono_Posix_FromMountFlags (from->f_flag, &flag) != 0)
		return -1;
	to->f_flag = flag;

	return 0;
}
#endif /* ndef HAVE_SYS_STATVFS_H */

/*
 * System V-compatible definitions
 */

#ifdef HAVE_STATVFS
gint32
Mono_Posix_Syscall_statvfs (const char *path, struct Mono_Posix_Statvfs *buf)
{
	struct statvfs s;
	int r;

	if (buf == NULL) {
		errno = EFAULT;
		return -1;
	}

	if ((r = statvfs (path, &s)) == 0)
		r = Mono_Posix_ToStatvfs (&s, buf);

	return r;
}
#endif /* ndef HAVA_STATVFS */

#ifdef HAVE_FSTATVFS
gint32
Mono_Posix_Syscall_fstatvfs (gint32 fd, struct Mono_Posix_Statvfs *buf)
{
	struct statvfs s;
	int r;

	if (buf == NULL) {
		errno = EFAULT;
		return -1;
	}

	if ((r = fstatvfs (fd, &s)) == 0)
		r = Mono_Posix_ToStatvfs (&s, buf);

	return r;
}
#endif /* ndef HAVA_FSTATVFS */

/*
 * BSD-compatible definitions.
 *
 * Linux also provides these, but are deprecated in favor of (f)statvfs.
 */

#if (defined (HAVE_STATFS) || defined (HAVE_FSTATFS)) && !defined (HAVE_STATVFS)
int
Mono_Posix_ToStatvfs (void *_from, struct Mono_Posix_Statvfs *to)
{
	struct statfs *from = _from;

	to->f_bsize   = from->f_bsize;
	to->f_frsize  = from->f_bsize;
	to->f_blocks  = from->f_blocks;
	to->f_bfree   = from->f_bfree;
	to->f_bavail  = from->f_bavail;
	to->f_files   = from->f_files;
	to->f_ffree   = from->f_ffree;
	to->f_favail  = from->f_ffree; /* OSX doesn't have f_avail */

	// from->f_fsid is an int32[2], to->f_fsid is a uint64, 
	// so this shouldn't lose anything.
	memcpy (&to->f_fsid, &from->f_fsid, sizeof(to->f_fsid));

#if HAVE_STRUCT_STATFS_F_FLAGS
	if (Mono_Posix_ToMountFlags (from->f_flags, &to->f_flag) != 0)
		return -1;
#endif  /* def HAVE_STRUCT_STATFS_F_FLAGS */

	return 0;
}

int
Mono_Posix_FromStatvfs (struct Mono_Posix_Statvfs *from, void *_to)
{
	struct statfs *to = _to;
	guint64 flag;

	to->f_bsize   = from->f_bsize;
	to->f_blocks  = from->f_blocks;
	to->f_bfree   = from->f_bfree;
	to->f_bavail  = from->f_bavail;
	to->f_files   = from->f_files;
	to->f_ffree   = from->f_ffree;

	// from->f_fsid is an int32[2], to->f_fsid is a uint64, 
	// so this shouldn't lose anything.
	memcpy (&to->f_fsid, &from->f_fsid, sizeof(to->f_fsid));

#if HAVE_STRUCT_STATFS_F_FLAGS
	if (Mono_Posix_FromMountFlags (from->f_flag, &flag) != 0)
		return -1;
	to->f_flags = flag;
#endif  /* def HAVE_STRUCT_STATFS_F_FLAGS */

	return 0;
}

static void
set_namemax (const char *path, struct Mono_Posix_Statvfs *buf)
{
  buf->f_namemax = pathconf (path, _PC_NAME_MAX);
}

static void
set_fnamemax (int fd, struct Mono_Posix_Statvfs *buf)
{
  buf->f_namemax = fpathconf (fd, _PC_NAME_MAX);
}
#endif /* (def HAVE_STATFS || def HAVE_FSTATFS) && !def HAVE_STATVFS */

#if !defined (HAVE_STATVFS) && defined (HAVE_STATFS)
gint32
Mono_Posix_Syscall_statvfs (const char *path, struct Mono_Posix_Statvfs *buf)
{
	struct statfs s;
	int r;

	if (buf == NULL) {
		errno = EFAULT;
		return -1;
	}

	if ((r = statfs (path, &s)) == 0 &&
			(r = Mono_Posix_ToStatvfs (&s, buf)) == 0) {
		set_namemax (path, buf);
	}

	return r;
}
#endif /* !def HAVE_STATVFS && def HAVE_STATFS */

#if !defined (HAVE_STATVFS) && defined (HAVE_STATFS)
gint32
Mono_Posix_Syscall_fstatvfs (gint32 fd, struct Mono_Posix_Statvfs *buf)
{
	struct statfs s;
	int r;

	if (buf == NULL) {
		errno = EFAULT;
		return -1;
	}

	if ((r = fstatfs (fd, &s)) == 0 &&
			(r = Mono_Posix_ToStatvfs (&s, buf)) == 0) {
		set_fnamemax (fd, buf);
	}

	return r;
}
#endif /* !def HAVE_FSTATVFS && def HAVE_STATFS */

G_END_DECLS

/*
 * vim: noexpandtab
 */
