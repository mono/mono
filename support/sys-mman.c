/*
 * <sys/mman.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004-2006 Jonathan Pryor
 */

#define _XOPEN_SOURCE 600

#ifdef PLATFORM_MACOSX
/* For mincore () */
#define _DARWIN_C_SOURCE
#endif
#ifdef __FreeBSD__
/* For mincore () */
#define __BSD_VISIBLE 1
#endif

#include <sys/types.h>
#include <sys/mman.h>
#include <errno.h>

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

void*
Mono_Posix_Syscall_mmap (void *start, mph_size_t length, int prot, int flags, 
		int fd, mph_off_t offset)
{
	int _prot, _flags;

	mph_return_val_if_size_t_overflow (length, MAP_FAILED);
	mph_return_val_if_off_t_overflow (offset, MAP_FAILED);

	if (Mono_Posix_FromMmapProts (prot, &_prot) == -1)
		return MAP_FAILED;
	if (Mono_Posix_FromMmapFlags (flags, &_flags) == -1)
		return MAP_FAILED;

	return mmap (start, (size_t) length, _prot, _flags, fd, (off_t) offset);
}

int
Mono_Posix_Syscall_munmap (void *start, mph_size_t length)
{
	mph_return_if_size_t_overflow (length);

	return munmap (start, (size_t) length);
}

int
Mono_Posix_Syscall_mprotect (void *start, mph_size_t len, int prot)
{
	int _prot;
	mph_return_if_size_t_overflow (len);

	if (Mono_Posix_FromMmapProts (prot, &_prot) == -1)
		return -1;

	return mprotect (start, (size_t) len, _prot);
}

int
Mono_Posix_Syscall_msync (void *start, mph_size_t len, int flags)
{
	int _flags;
	mph_return_if_size_t_overflow (len);

	if (Mono_Posix_FromMsyncFlags (flags, &_flags) == -1)
		return -1;

	return msync (start, (size_t) len, _flags);
}

int
Mono_Posix_Syscall_mlock (void *start, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);

	return mlock (start, (size_t) len);
}

int
Mono_Posix_Syscall_munlock (void *start, mph_size_t len)
{
	mph_return_if_size_t_overflow (len);

	return munlock (start, (size_t) len);
}

#ifdef HAVE_MREMAP
void*
Mono_Posix_Syscall_mremap (void *old_address, mph_size_t old_size, 
		mph_size_t new_size, guint64 flags)
{
	guint64 _flags;

	mph_return_val_if_size_t_overflow (old_size, MAP_FAILED);
	mph_return_val_if_size_t_overflow (new_size, MAP_FAILED);

	if (Mono_Posix_FromMremapFlags (flags, &_flags) == -1)
		return MAP_FAILED;

	return mremap (old_address, (size_t) old_size, (size_t) new_size,
			(unsigned long) _flags);
}
#endif /* def HAVE_MREMAP */

int
Mono_Posix_Syscall_mincore (void *start, mph_size_t length, unsigned char *vec)
{
	mph_return_if_size_t_overflow (length);

	return mincore (start, (size_t) length, (void*)vec);
}

#ifdef HAVE_POSIX_MADVISE
gint32
Mono_Posix_Syscall_posix_madvise (void *addr, mph_size_t len, gint32 advice)
{
	mph_return_if_size_t_overflow (len);

	if (Mono_Posix_FromPosixMadviseAdvice (advice, &advice) == -1)
		return -1;

	return posix_madvise (addr, (size_t) len, advice);
}
#endif /* def HAVE_POSIX_MADVISE */

#ifdef HAVE_REMAP_FILE_PAGES
int
Mono_Posix_Syscall_remap_file_pages (void *start, mph_size_t size, 
		int prot, mph_ssize_t pgoff, int flags)
{
	int _prot, _flags;

	mph_return_if_size_t_overflow (size);
	mph_return_if_ssize_t_overflow (pgoff);

	if (Mono_Posix_FromMmapProts (prot, &_prot) == -1)
		return -1;
	if (Mono_Posix_FromMmapFlags (flags, &_flags) == -1)
		return -1;

	return remap_file_pages (start, (size_t) size, _prot, (ssize_t) pgoff, _flags);
}
#endif /* def HAVE_REMAP_FILE_PAGES */

G_END_DECLS

/*
 * vim: noexpandtab
 */
