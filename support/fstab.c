/*
 * <fstab.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#include <fstab.h>
#include <errno.h>
#include <string.h>
#include <stdio.h>
#include <stdlib.h>

#include "mph.h"

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
 * Copy the native `passwd' structure to it's managed representation.
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

gint32
Mono_Posix_Syscall_getfsent (struct Mono_Posix_Syscall__Fstab *fsbuf)
{
	struct fstab *fs;

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
	struct fstab *fs;

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
	struct fstab *fs;

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

G_END_DECLS

/*
 * vim: noexpandtab
 */
