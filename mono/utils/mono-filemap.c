/*
 * mono-filemap.c: Unix/Windows implementation for filemap.
 *
 * Author:
 *   Paolo Molaro (lupus@ximian.com)
 *
 * Copyright 2008-2008 Novell, Inc.
 */

#include "config.h"

#if HAVE_SYS_STAT_H
#include <sys/stat.h>
#endif
#include <fcntl.h>
#include <string.h>
#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif
#include <stdlib.h>
#include <stdio.h>

#include "mono-mmap.h"

static MonoFileMapOpen     file_open_func     = 0;
static MonoFileMapSize     file_size_func     = 0;
static MonoFileMapFd       file_fd_func       = 0;
static MonoFileMapClose    file_close_func    = 0;
       MonoFileMapMap      file_map_func      = 0;
       MonoFileMapUnmap    file_unmap_func    = 0;

#if defined(ANDROID)
void mono_file_map_override(MonoFileMapOpen open_func, MonoFileMapSize size_func, MonoFileMapFd fd_func, MonoFileMapClose close_func, MonoFileMapMap map_func, MonoFileMapUnmap unmap_func)
{
	file_open_func     = open_func;
	file_size_func     = size_func;
	file_fd_func       = fd_func;
	file_close_func    = close_func;
	file_map_func      = map_func;
	file_unmap_func    = unmap_func;
}
#endif

MonoFileMap *
mono_file_map_open (const char* name)
{
	gunichar2* name_utf16;
	MonoFileMap* res;
	if (file_open_func) return file_open_func(name);
#ifdef PLATFORM_WIN32
	name_utf16 = g_utf8_to_utf16 (name, -1, NULL, NULL, NULL);
	res = (MonoFileMap *)_wfopen (name_utf16, L"rb");
	g_free (name_utf16);
#else
	res = (MonoFileMap *)fopen (name, "rb");
#endif

	return res;
}

guint64 
mono_file_map_size (MonoFileMap *fmap)
{
	struct stat stat_buf;
	if (file_size_func) return file_size_func(fmap);
	if (fstat (fileno ((FILE*)fmap), &stat_buf) < 0)
		return 0;
	return stat_buf.st_size;
}

int
mono_file_map_fd (MonoFileMap *fmap)
{
	if (file_fd_func) return file_fd_func(fmap);
	return fileno ((FILE*)fmap);
}

int 
mono_file_map_close (MonoFileMap *fmap)
{
	if (file_close_func) return file_close_func(fmap);
	return fclose ((FILE*)fmap);
}

#if !defined(HAVE_MMAP) && !defined (PLATFORM_WIN32)

static mono_file_map_alloc_fn alloc_fn = (mono_file_map_alloc_fn) malloc;
static mono_file_map_release_fn release_fn = (mono_file_map_release_fn) free;

void
mono_file_map_set_allocator (mono_file_map_alloc_fn alloc, mono_file_map_release_fn release)
{
	alloc_fn = alloc == NULL     ? (mono_file_map_alloc_fn) malloc : alloc;
	release_fn = release == NULL ? (mono_file_map_release_fn) free : release;
}

void *
mono_file_map (size_t length, int flags, int fd, guint64 offset, void **ret_handle)
{
	guint64 cur_offset;
	size_t bytes_read;
	void *ptr = (*alloc_fn) (length);
	if (!ptr)
		return NULL;
	cur_offset = lseek (fd, 0, SEEK_CUR);
	if (lseek (fd, offset, SEEK_SET) != offset) {
		free (ptr);
		return NULL;
	}
	bytes_read = read (fd, ptr, length);
	lseek (fd, cur_offset, SEEK_SET);
	*ret_handle = NULL;
	return ptr;
}

int
mono_file_unmap (void *addr, void *handle)
{
	(*release_fn) (addr);
	return 0;
}
#endif
