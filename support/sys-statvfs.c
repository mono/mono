/*
 * <sys/sendfile.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#include <errno.h>

#include <string.h>

#include "mph.h"

#ifdef HAVE_SYS_STATVFS_H
#include <sys/statvfs.h>
#endif /* def HAVE_SYS_STATVFS_H */

#ifdef HAVE_GETFSSTAT
#include <sys/param.h>
#include <sys/ucred.h>
#include <sys/mount.h>
#include <unistd.h>     /* for pathconf */
#endif /* def HAVE_GETFSSTAT */

G_BEGIN_DECLS

struct Mono_Posix_Statvfs {
	guint64         f_bsize;    /* file system block size */
	guint64         f_frsize;   /* fragment size */
	mph_fsblkcnt_t  f_blocks;   /* size of fs in f_frsize units */
	mph_fsblkcnt_t  f_bfree;    /* # free blocks */
	mph_fsblkcnt_t  f_bavail;   /* # free blocks for non-root */
	mph_fsfilcnt_t  f_files;    /* # inodes */
	mph_fsfilcnt_t  f_ffree;    /* # free inodes */
	mph_fsfilcnt_t  f_favail;   /* # free inodes for non-root */
	guint64         f_fsid;     /* file system id */
	guint64         f_flag;     /* mount flags */
	guint64         f_namemax;  /* maximum filename length */
};

#ifdef HAVE_SYS_STATVFS_H
static void
copy_statvfs (struct Mono_Posix_Statvfs *to, struct statvfs *from)
{
  to->f_bsize   = from->f_bsize;
  to->f_frsize  = from->f_frsize;
  to->f_blocks  = from->f_blocks;
  to->f_bfree   = from->f_bfree;
  to->f_bavail  = from->f_bavail;
  to->f_files   = from->f_files;
  to->f_ffree   = from->f_ffree;
  to->f_favail  = from->f_favail;
  to->f_fsid    = from->f_fsid;
  to->f_flag    = from->f_flag;
  to->f_namemax =	from->f_namemax;
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
		copy_statvfs (buf, &s);

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
		copy_statvfs (buf, &s);

	return r;
}
#endif /* ndef HAVA_FSTATVFS */

/*
 * BSD-compatible definitions.
 *
 * Linux also provides these, but are deprecated in favor of (f)statvfs.
 */

#if (defined (HAVE_STATFS) || defined (HAVE_FSTATFS)) && !defined (HAVE_STATVFS)
static void
copy_statfs (struct Mono_Posix_Statvfs *to, struct statfs *from)
{
  to->f_bsize   = from->f_bsize;
  to->f_frsize  = from->f_bsize;
  to->f_blocks  = from->f_blocks;
  to->f_bfree   = from->f_bfree;
  to->f_bavail  = from->f_bavail;
  to->f_files   = from->f_files;
  to->f_ffree   = from->f_ffree;
  to->f_favail  = from->f_ffree; /* OSX doesn't have f_avail */
  to->f_flag    = from->f_flags;
	// from->f_fsid is an int32[2], to->f_fsid is a uint64, 
	// so this shouldn't lose anything.
	memcpy (&to->f_fsid, from->f_fsid, sizeof(to->f_fsid));
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

	if ((r = statfs (path, &s)) == 0) {
		copy_statfs (buf, &s);
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

	if ((r = fstatfs (fd, &s)) == 0) {
		copy_statfs (buf, &s);
		set_fnamemax (fd, buf);
	}

	return r;
}
#endif /* !def HAVE_FSTATVFS && def HAVE_STATFS */

G_END_DECLS

/*
 * vim: noexpandtab
 */
