/*
 * <fstab.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004-2005 Jonathan Pryor
 */

#include <errno.h>
#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <stddef.h>

#include "mph.h"

#ifdef HAVE_FSTAB_H
#include <fstab.h>
#endif /* def HAVE_FSTAB_H */

#ifdef HAVE_SYS_VFSTAB_H
#include <sys/vfstab.h>
#endif /* def HAVE_SYS_VFSTAB_H */

G_BEGIN_DECLS

struct Mono_Posix_Syscall__Fstab {
	char  *fs_spec;     /* block device name */
	char  *fs_file;     /* mount point */
	char  *fs_vfstype;	/* filesystem type */
	char  *fs_mntops;   /* mount options */
	char  *fs_type;     /* rw/rq/ro/sw/xx option */
	int    fs_freq;     /* dump frequency, in days */
	int    fs_passno;   /* pass number on parallel dump */

	char  *_fs_buf_;
};

#ifdef HAVE_FSTAB_H

typedef struct fstab mph_fstab;

static const size_t
fstab_offsets[] = {
	offsetof (struct fstab, fs_spec),
	offsetof (struct fstab, fs_file),
	offsetof (struct fstab, fs_vfstype),
	offsetof (struct fstab, fs_mntops),
	offsetof (struct fstab, fs_type)
};

static const size_t
mph_fstab_offsets[] = {
	offsetof (struct Mono_Posix_Syscall__Fstab, fs_spec),
	offsetof (struct Mono_Posix_Syscall__Fstab, fs_file),
	offsetof (struct Mono_Posix_Syscall__Fstab, fs_vfstype),
	offsetof (struct Mono_Posix_Syscall__Fstab, fs_mntops),
	offsetof (struct Mono_Posix_Syscall__Fstab, fs_type)
};

/*
 * Copy the native `fstab' structure to it's managed representation.
 *
 * To minimize separate mallocs, all the strings are allocated within the same
 * memory block (stored in _fs_buf_).
 */
static int
copy_fstab (struct Mono_Posix_Syscall__Fstab *to, struct fstab *from)
{
	char *buf;
	buf = _mph_copy_structure_strings (to, mph_fstab_offsets,
			from, fstab_offsets, sizeof(fstab_offsets)/sizeof(fstab_offsets[0]));

	to->fs_freq   = from->fs_freq;
	to->fs_passno = from->fs_passno;

	to->_fs_buf_ = buf;
	if (buf == NULL) {
		return -1;
	}

	return 0;
}

#endif /* def HAVE_FSTAB_H */

#ifdef HAVE_SYS_VFSTAB_H

/* 
 * Solaris doesn't provide <fstab.h> but has equivalent functionality in
 * <sys/fstab.h> via getvfsent(3C) and company.
 */

typedef struct vfstab mph_fstab;

static const size_t
vfstab_offsets[] = {
	offsetof (struct vfstab, vfs_special),
	offsetof (struct vfstab, vfs_mountp),
	offsetof (struct vfstab, vfs_fstype),
	offsetof (struct vfstab, vfs_mntopts)
};

static const size_t
mph_fstab_offsets[] = {
	offsetof (struct Mono_Posix_Syscall__Fstab, fs_spec),
	offsetof (struct Mono_Posix_Syscall__Fstab, fs_file),
	offsetof (struct Mono_Posix_Syscall__Fstab, fs_vfstype),
	offsetof (struct Mono_Posix_Syscall__Fstab, fs_mntops)
};

/*
 * Copy the native `vfstab' structure to it's managed representation.
 *
 * To minimize separate mallocs, all the strings are allocated within the same
 * memory block (stored in _fs_buf_).
 */
static int
copy_fstab (struct Mono_Posix_Syscall__Fstab *to, struct vfstab *from)
{
	char *buf;
	buf = _mph_copy_structure_strings (to, mph_fstab_offsets,
			from, vfstab_offsets, sizeof(vfstab_offsets)/sizeof(vfstab_offsets[0]));

	to->fs_type   = NULL;
	to->fs_freq   = -1;
	to->fs_passno = -1;

	to->_fs_buf_ = buf;
	if (buf == NULL) {
		return -1;
	}

	return 0;
}

/*
 * Implement Linux/BSD getfsent(3) in terms of Solaris getvfsent(3C)...
 */
static FILE*
etc_fstab;

static int
setfsent (void)
{
	etc_fstab = fopen ("/etc/vfstab", "r");
	if (etc_fstab != NULL)
		return 1;
	return 0;
}

static void
endfsent (void)
{
	fclose (etc_fstab);
	etc_fstab = NULL;
}

static struct vfstab
cur_vfstab_entry;

static struct vfstab*
getfsent (void)
{
	int r;
	r = getvfsent (etc_fstab, &cur_vfstab_entry);
	if (r == 0)
		return &cur_vfstab_entry;
	return NULL;
}

static struct vfstab*
getfsfile (const char *mount_point)
{
	int r;
	int close = 0;
	if (etc_fstab == 0) {
		close = 1;
		if (setfsfile() != 1)
			return NULL;
	}
	rewind (etc_fstab);
	r = getvfsfile (etc_fstab, &cur_vfstab_entry, (char*) mount_point);
	if (close)
		endfsent ();
	if (r == 0)
		return &cur_vfstab_entry;
	return NULL;
}

static struct vfstab*
getfsspec (const char *special_file)
{
	int r;
	int close = 0;
	if (etc_fstab == 0) {
		close = 1;
		if (setfsfile() != 1)
			return NULL;
	}
	rewind (etc_fstab);
	r = getvfsspec (etc_fstab, &cur_vfstab_entry, (char*) special_file);
	if (close)
		endfsent ();
	if (r == 0)
		return &cur_vfstab_entry;
	return NULL;
}

#endif /* def HAVE_SYS_VFSTAB_H */

#if defined (HAVE_FSTAB_H) || defined (HAVE_SYS_VFSTAB_H)

void
Mono_Posix_Syscall_endfsent ()
{
	endfsent ();
}

gint32
Mono_Posix_Syscall_getfsent (struct Mono_Posix_Syscall__Fstab *fsbuf)
{
	mph_fstab *fs;

	if (fsbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	fs = getfsent ();
	if (fs == NULL)
		return -1;

	if (copy_fstab (fsbuf, fs) == -1) {
		errno = ENOMEM;
		return -1;
	}
	return 0;
}

gint32
Mono_Posix_Syscall_getfsfile (const char *mount_point, 
		struct Mono_Posix_Syscall__Fstab *fsbuf)
{
	mph_fstab *fs;

	if (fsbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	fs = getfsfile (mount_point);
	if (fs == NULL)
		return -1;

	if (copy_fstab (fsbuf, fs) == -1) {
		errno = ENOMEM;
		return -1;
	}
	return 0;
}

gint32
Mono_Posix_Syscall_getfsspec (const char *special_file, 
		struct Mono_Posix_Syscall__Fstab *fsbuf)
{
	mph_fstab *fs;

	if (fsbuf == NULL) {
		errno = EFAULT;
		return -1;
	}

	fs = getfsspec (special_file);
	if (fs == NULL)
		return -1;

	if (copy_fstab (fsbuf, fs) == -1) {
		errno = ENOMEM;
		return -1;
	}
	return 0;
}

gint32
Mono_Posix_Syscall_setfsent (void)
{
	return setfsent ();
}

#endif /* def HAVE_FSTAB_H || def HAVE_SYS_VFSTAB_H */

G_END_DECLS

/*
 * vim: noexpandtab
 */
