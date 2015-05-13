/*
 * null-gc.c: GC implementation using malloc: will leak everything, just for testing.
 *
 * Copyright 2001-2003 Ximian, Inc (http://www.ximian.com)
 * Copyright 2004-2011 Novell, Inc (http://www.novell.com)
 * Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
 */

#include "config.h"
#include <glib.h>
#include <mono/metadata/mono-gc.h>
#include <mono/metadata/gc-internal.h>
#include <mono/metadata/runtime.h>
#include <mono/utils/atomic.h>
#include <mono/utils/mono-threads.h>
#include <mono/utils/mono-counters.h>

#ifdef HAVE_NULL_GC

void
mono_gc_base_init (void)
{
	MonoThreadInfoCallbacks cb;
	int dummy;

	mono_counters_init ();

	memset (&cb, 0, sizeof (cb));
	/* TODO: This casts away an incompatible pointer type warning in the same
	         manner that boehm-gc does it. This is probably worth investigating
	         more carefully. */
	cb.mono_method_is_critical = (gpointer)mono_runtime_is_critical_method;

	mono_threads_init (&cb, sizeof (MonoThreadInfo));

	mono_thread_info_attach (&dummy);
}

void
mono_gc_collect (int generation)
{
}

int
mono_gc_max_generation (void)
{
	return 0;
}

int
mono_gc_get_generation  (MonoObject *object)
{
	return 0;
}

int
mono_gc_collection_count (int generation)
{
	return 0;
}

void
mono_gc_add_memory_pressure (gint64 value)
{
}

/* maybe track the size, not important, though */
int64_t
mono_gc_get_used_size (void)
{
	return 1024*1024;
}

int64_t
mono_gc_get_heap_size (void)
{
	return 2*1024*1024;
}

gboolean
mono_gc_is_gc_thread (void)
{
	return TRUE;
}

gboolean
mono_gc_register_thread (void *baseptr)
{
	return TRUE;
}

int
mono_gc_walk_heap (int flags, MonoGCReferences callback, void *data)
{
	return 1;
}

gboolean
mono_object_is_alive (MonoObject* o)
{
	return TRUE;
}

void
mono_gc_enable_events (void)
{
}

int
mono_gc_register_root (char *start, size_t size, void *descr)
{
	return TRUE;
}

void
mono_gc_deregister_root (char* addr)
{
}

void
mono_gc_weak_link_add (void **link_addr, MonoObject *obj, gboolean track)
{
	*link_addr = obj;
}

void
mono_gc_weak_link_remove (void **link_addr, gboolean track)
{
	*link_addr = NULL;
}

MonoObject*
mono_gc_weak_link_get (void **link_addr)
{
	return *link_addr;
}

void*
mono_gc_make_descr_for_string (gsize *bitmap, int numbits)
{
	return NULL;
}

void*
mono_gc_make_descr_for_object (gsize *bitmap, int numbits, size_t obj_size)
{
	return NULL;
}

void*
mono_gc_make_descr_for_array (int vector, gsize *elem_bitmap, int numbits, size_t elem_size)
{
	return NULL;
}

void*
mono_gc_make_descr_from_bitmap (gsize *bitmap, int numbits)
{
	return NULL;
}

void*
mono_gc_make_root_descr_all_refs (int numbits)
{
	return NULL;
}

void*
mono_gc_alloc_fixed (size_t size, void *descr)
{
	return g_malloc0 (size);
}

void
mono_gc_free_fixed (void* addr)
{
	g_free (addr);
}

void
mono_gc_wbarrier_set_field (MonoObject *obj, gpointer field_ptr, MonoObject* value)
{
	*(void**)field_ptr = value;
}

void
mono_gc_wbarrier_set_arrayref (MonoArray *arr, gpointer slot_ptr, MonoObject* value)
{
	*(void**)slot_ptr = value;
}

void
mono_gc_wbarrier_arrayref_copy (gpointer dest_ptr, gpointer src_ptr, int count)
{
	mono_gc_memmove_aligned (dest_ptr, src_ptr, count * sizeof (gpointer));
}

void
mono_gc_wbarrier_generic_store (gpointer ptr, MonoObject* value)
{
	*(void**)ptr = value;
}

void
mono_gc_wbarrier_generic_store_atomic (gpointer ptr, MonoObject *value)
{
	InterlockedWritePointer (ptr, value);
}

void
mono_gc_wbarrier_generic_nostore (gpointer ptr)
{
}

void
mono_gc_wbarrier_value_copy (gpointer dest, gpointer src, int count, MonoClass *klass)
{
	mono_gc_memmove_atomic (dest, src, count * mono_class_value_size (klass, NULL));
}

void
mono_gc_wbarrier_object_copy (MonoObject* obj, MonoObject *src)
{
	/* do not copy the sync state */
	mono_gc_memmove_aligned ((char*)obj + sizeof (MonoObject), (char*)src + sizeof (MonoObject),
			mono_object_class (obj)->instance_size - sizeof (MonoObject));
}

gboolean
mono_gc_is_critical_method (MonoMethod *method)
{
	return FALSE;
}

int
mono_gc_get_aligned_size_for_allocator (int size)
{
	return size;
}

MonoMethod*
mono_gc_get_managed_allocator (MonoClass *klass, gboolean for_box, gboolean known_instance_size)
{
	return NULL;
}

MonoMethod*
mono_gc_get_managed_array_allocator (MonoClass *klass)
{
	return NULL;
}

MonoMethod*
mono_gc_get_managed_allocator_by_type (int atype)
{
	return NULL;
}

guint32
mono_gc_get_managed_allocator_types (void)
{
	return 0;
}

const char *
mono_gc_get_gc_name (void)
{
	return "null";
}

void
mono_gc_add_weak_track_handle (MonoObject *obj, guint32 gchandle)
{
}

void
mono_gc_change_weak_track_handle (MonoObject *old_obj, MonoObject *obj, guint32 gchandle)
{
}

void
mono_gc_remove_weak_track_handle (guint32 gchandle)
{
}

GSList*
mono_gc_remove_weak_track_object (MonoDomain *domain, MonoObject *obj)
{
	return NULL;
}

void
mono_gc_clear_domain (MonoDomain *domain)
{
}

int
mono_gc_get_suspend_signal (void)
{
	return -1;
}

int
mono_gc_get_restart_signal (void)
{
	return -1;
}

MonoMethod*
mono_gc_get_specific_write_barrier (gboolean is_concurrent)
{
	g_assert_not_reached ();
	return NULL;
}

MonoMethod*
mono_gc_get_write_barrier (void)
{
	g_assert_not_reached ();
	return NULL;
}

void*
mono_gc_invoke_with_gc_lock (MonoGCLockedCallbackFunc func, void *data)
{
	return func (data);
}

char*
mono_gc_get_description (void)
{
	return g_strdup (DEFAULT_GC_NAME);
}

void
mono_gc_set_desktop_mode (void)
{
}

gboolean
mono_gc_is_moving (void)
{
	return FALSE;
}

gboolean
mono_gc_is_disabled (void)
{
	return FALSE;
}

void
mono_gc_wbarrier_value_copy_bitmap (gpointer _dest, gpointer _src, int size, unsigned bitmap)
{
	g_assert_not_reached ();
}

guint8*
mono_gc_get_card_table (int *shift_bits, gpointer *card_mask)
{
	g_assert_not_reached ();
	return NULL;
}

gboolean
mono_gc_card_table_nursery_check (void)
{
	g_assert_not_reached ();
	return TRUE;
}

void*
mono_gc_get_nursery (int *shift_bits, size_t *size)
{
	return NULL;
}

void
mono_gc_set_current_thread_appdomain (MonoDomain *domain)
{
}

gboolean
mono_gc_precise_stack_mark_enabled (void)
{
	return FALSE;
}

FILE *
mono_gc_get_logfile (void)
{
	return NULL;
}

void
mono_gc_conservatively_scan_area (void *start, void *end)
{
	g_assert_not_reached ();
}

void *
mono_gc_scan_object (void *obj, void *gc_data)
{
	g_assert_not_reached ();
	return NULL;
}

gsize*
mono_gc_get_bitmap_for_descr (void *descr, int *numbits)
{
	g_assert_not_reached ();
	return NULL;
}

void
mono_gc_set_gc_callbacks (MonoGCCallbacks *callbacks)
{
}

void
mono_gc_set_stack_end (void *stack_end)
{
}

int
mono_gc_get_los_limit (void)
{
	return G_MAXINT;
}

gboolean
mono_gc_user_markers_supported (void)
{
	return FALSE;
}

void *
mono_gc_make_root_descr_user (MonoGCRootMarkFunc marker)
{
	g_assert_not_reached ();
	return NULL;
}

#ifndef HOST_WIN32

void mono_gc_set_skip_thread (gboolean value)
{
}
#endif

#ifdef HOST_WIN32
BOOL APIENTRY mono_gc_dllmain (HMODULE module_handle, DWORD reason, LPVOID reserved)
{
	return TRUE;
}
#endif

guint
mono_gc_get_vtable_bits (MonoClass *class)
{
	return 0;
}

void
mono_gc_register_altstack (gpointer stack, gint32 stack_size, gpointer altstack, gint32 altstack_size)
{
}

gboolean
mono_gc_set_allow_synchronous_major (gboolean flag)
{
	return TRUE;
}

#endif
