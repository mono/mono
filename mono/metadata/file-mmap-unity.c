
#include <config.h>
#include <glib.h>

#include <mono/metadata/file-mmap.h>
#include "MemoryMappedFile-c-api.h"
#include "File-c-api.h"

typedef struct {
	void *address;
	size_t length;
} MmapInstance;

enum {
	BAD_CAPACITY_FOR_FILE_BACKED = 1,
	CAPACITY_SMALLER_THAN_FILE_SIZE,
	FILE_NOT_FOUND,
	FILE_ALREADY_EXISTS,
	PATH_TOO_LONG,
	COULD_NOT_OPEN,
	CAPACITY_MUST_BE_POSITIVE,
	INVALID_FILE_MODE,
	COULD_NOT_MAP_MEMORY,
	ACCESS_DENIED,
	CAPACITY_LARGER_THAN_LOGICAL_ADDRESS_SPACE
};

#ifndef HOST_WIN32

typedef struct {
	int kind;
	int ref_count;
	size_t capacity;
	char *name;
	int fd;
} MmapHandle;

#endif

void mono_mmap_close (void *mmap_handle)
{
	/* Not Supported in UnityPAL */
	g_assert_not_reached();
}

void mono_mmap_configure_inheritability (void *mmap_handle, gboolean inheritability)
{
	/* Not Supported in UnityPAL */
	g_assert_not_reached();
}

void mono_mmap_flush (void *mmap_handle)
{
	/* Not Supported in UnityPAL */
	g_assert_not_reached();
}

void *mono_mmap_open_file (MonoString *string, int mode, MonoString *mapName, gint64 *capacity, int access, int options, int *error)
{
	/* Not Supported in UnityPAL */
	g_assert_not_reached();
	return NULL;
}

void *mono_mmap_open_handle (void *handle, MonoString *mapName, gint64 *capacity, int access, int options, int *error)
{
	/* Not Supported in UnityPAL */
	g_assert_not_reached();
	return NULL;
}

int mono_mmap_map (void *handle, gint64 offset, gint64 *size, int access, void **mmap_handle, void **base_address)
{
	/* We are dropping access parameter, UnityPAL does not support */
	g_assert (handle);

	MmapInstance *h = g_malloc0 (sizeof (MmapInstance));
	h->length = *size;

 #ifdef HOST_WIN32
	h->address = UnityPalMemoryMappedFileMapWithParams((UnityPalFileHandle*) handle, (size_t) *size,  (size_t) offset);
 #else 
    MmapHandle *fh = (MmapHandle *)handle; 
	h->address = UnityPalMemoryMappedFileMapWithFileDescriptor(fh->fd, (size_t) *size,  (size_t) offset);
 #endif

	if (h->address) 
	{
		*mmap_handle = h;
		*base_address = (char*) h->address + offset;
		return 0;
	} 
	else 
	{
		g_free (h);
		return COULD_NOT_MAP_MEMORY;
	}
}

gboolean
mono_mmap_unmap (void *mmap_handle)
{
	g_assert (mmap_handle);

	MmapInstance *h = (MmapInstance *)mmap_handle;

	UnityPalMemoryMappedFileUnmapWithParams(h->address, h->length);

	g_free (h);

	/* UnityPAL does not give any indication of success or failure of an unmap, forced
       to always return true */

	return TRUE;
}
