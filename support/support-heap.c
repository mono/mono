/*
 * Emulates the Heap* routines.
 *
 * Authors:
 *   Gonzalo Paniagua (gonzalo@ximian.com)
 *   Miguel de Icaza  (miguel@novell.com)
 *
 * (C) 2005 Novell, Inc.
 *
 */
#include <glib.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include "supportw.h"

gpointer HeapAlloc            (gpointer unused1, gint32 unused2, gint32 nbytes);
gpointer HeapCreate           (gint32 flags, gint32 initial_size, gint32 max_size);
gboolean HeapSetInformation   (gpointer handle, gpointer heap_info_class,
			       gpointer heap_info, gint32 head_info_length);

gboolean HeapQueryInformation (gpointer handle, gpointer heap_info_class,
			       gpointer heap_info, gint32 head_info_length, gint32 *ret_length);

gpointer HeapAlloc            (gpointer handle, gint32 flags, gint32 nbytes);
gpointer HeapReAlloc          (gpointer handle, gint32 flags, gpointer mem, gint32 nbytes);
gint32   HeapSize             (gpointer handle, gint32 flags, gpointer mem);
gboolean HeapFree             (gpointer handle, gint32 flags, gpointer mem);
gboolean HeapValidate         (gpointer handle, gpointer mem);
gboolean HeapDestroy          (gpointer handle);

typedef struct _HeapInfo {
	gint32 flags;
	gint32 initial_size;
	gint32 max_size;
	GHashTable *hash;
} HeapInfo;

/* Some initial value for the process heap */
HeapInfo *process_heap;

static GHashTable *heaps;

gpointer
HeapCreate (gint32 flags, gint32 initial_size, gint32 max_size)
{
	HeapInfo *hi;
	
	if (heaps == NULL)
		heaps = g_hash_table_new (g_direct_hash, g_direct_equal);

	if (flags != 0)
		g_warning ("Flags for HeapCreate are the unsupported value non-zero");
	
	hi = g_new (HeapInfo, 1);
	hi->flags = flags;
	hi->initial_size = initial_size;
	hi->max_size = max_size;
	hi->hash = g_hash_table_new (g_direct_hash, g_direct_equal);
	
	g_hash_table_insert (heaps, hi, hi);
	
	return hi;
}

gboolean
HeapSetInformation (gpointer handle, gpointer heap_info_class, gpointer heap_info,
		    gint32 head_info_length)
{
	return TRUE;
}

gboolean
HeapQueryInformation (gpointer handle, gpointer heap_info_class, gpointer heap_info,
			gint32 head_info_length, gint32 *ret_length)
{
	*ret_length = 0;
	return TRUE;
}

gpointer
HeapAlloc (gpointer handle, gint32 flags, gint32 nbytes)
{
	HeapInfo *heap = (HeapInfo *) handle;
	void *ptr;
	
	ptr = g_malloc0 (nbytes);

	g_hash_table_insert (heap->hash, ptr, GINT_TO_POINTER (nbytes));
	
	return ptr;
}

gpointer
HeapReAlloc (gpointer handle, gint32 flags, gpointer mem, gint32 nbytes)
{
	HeapInfo *heap = (HeapInfo *) handle;
	void *ptr;
	
	g_hash_table_remove (heap->hash, mem);
	ptr = g_realloc (mem, nbytes);
	g_hash_table_insert (heap->hash, ptr, GINT_TO_POINTER (nbytes));

	return ptr;
}

gint32
HeapSize (gpointer handle, gint32 flags, gpointer mem)
{
	HeapInfo *heap = (HeapInfo *) handle;

	gint32 size = GPOINTER_TO_INT (g_hash_table_lookup (heap->hash, mem));

	return size;
}

gboolean
HeapFree (gpointer handle, gint32 flags, gpointer mem)
{
	HeapInfo *heap = (HeapInfo *) handle;

	g_hash_table_remove (heap->hash, GINT_TO_POINTER (mem));
	g_free (mem);
	
	return TRUE;
}

gboolean
HeapValidate (gpointer handle, gpointer mem)
{
	return TRUE;
}

static void
free_handles (gpointer key, gpointer value, gpointer user_data)
{
	g_free (key);
}

gboolean
HeapDestroy (gpointer handle)
{
	HeapInfo *heap = (HeapInfo *) handle;

	/* Failure is zero */
	if (handle == process_heap)
		return 0;
	
	g_hash_table_foreach (heap->hash, free_handles, NULL);
	g_hash_table_destroy (heap->hash);
	
	g_hash_table_remove (heaps, handle);
	g_free (heap);

	return 1;
}

gpointer GetProcessHeap (void);

gpointer 
GetProcessHeap (void)
{
	if (process_heap == NULL){
		process_heap = g_new (HeapInfo, 1);
		process_heap->flags = 0;
		process_heap->initial_size = 1024;
		process_heap->max_size = 1024*1024*1024;
		
	}
	return process_heap;
}
/* end Heap* functions */
