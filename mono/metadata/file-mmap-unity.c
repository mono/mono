
#include <config.h>
#include <glib.h>

#include <mono/metadata/file-mmap.h>
#include "MemoryMappedFile-c-api.h"

typedef struct {
	void *address;
	size_t length;
} MmapInstance;



void mono_mmap_close (void *mmap_handle)
{

}

void mono_mmap_configure_inheritability (void *mmap_handle, gboolean inheritability)
{

}

void mono_mmap_flush (void *mmap_handle)
{

}

void *mono_mmap_open_file (MonoString *string, int mode, MonoString *mapName, gint64 *capacity, int access, int options, int *error)
{
	return NULL;
}

void *mono_mmap_open_handle (void *handle, MonoString *mapName, gint64 *capacity, int access, int options, int *error)
{
	return NULL;
}

int mono_mmap_map (void *handle, gint64 offset, gint64 *size, int access, void **mmap_handle, void **base_address)
{
	return 0;
}

gboolean
mono_mmap_unmap (void *mmap_handle)
{
	g_assert (mmap_handle);

	MmapInstance *h = (MmapInstance *)mmap_handle;

	UnityPalMemoryMappedFileUnmapWithParams(h->address, h->length);

	g_free (h);

	/* libil2cpp does not give any indication of success or failure of an unmap, forced
       to always return true */

	return TRUE;
}