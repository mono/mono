/**
 * \file
 * GC implementation using CoreCLR GC
 *
 * Copyright 2019 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include "config.h"

#define THREAD_INFO_TYPE CoreClrThreadInfo

struct _CoreClrThreadInfo;

typedef struct _CoreClrThreadInfo CoreClrThreadInfo;

#include <glib.h>

#include "sgen/sgen-archdep.h"

#include <mono/metadata/mono-gc.h>
#include <mono/metadata/gc-internals.h>
#include <mono/metadata/class-init.h>
#include <mono/metadata/runtime.h>
#include <mono/metadata/w32handle.h>
#include <mono/metadata/abi-details.h>
#include <mono/utils/atomic.h>
#include <mono/utils/mono-threads.h>
#include <mono/utils/mono-counters.h>
#include <mono/metadata/null-gc-handles.h>
#include "coregc-mono.h"

struct _CoreClrThreadInfo {
	MonoThreadInfo info;
	gboolean skip, suspend_done;

	void *stack_end;
	void *stack_start;
	void *stack_start_limit;

	MonoContext ctx;
};

static gboolean gc_inited = FALSE;

G_BEGIN_DECLS

HRESULT GC_Initialize(IGCToCLR* clrToGC, IGCHeap** gcHeap, IGCHandleManager** gcHandleManager, GcDacVars* gcDacVars);

static IGCHeap *pGCHeap;
static IGCHandleManager *pGCHandleManager;

static void
mono_init_coreclr_gc (void)
{
	//
	// Initialize GC heap
	//
	GcDacVars dacVars;
	if (GC_Initialize(nullptr, &pGCHeap, &pGCHandleManager, &dacVars) != S_OK)
		g_assert_not_reached ();

	if (FAILED(pGCHeap->Initialize()))
		g_assert_not_reached ();

	//
	// Initialize handle manager
	//
	if (!pGCHandleManager->Initialize())
		g_assert_not_reached ();
}

void
mono_gc_base_init (void)
{
	if (gc_inited)
		return;

	mono_counters_init ();

#ifndef HOST_WIN32
	mono_w32handle_init ();
#endif

	mono_thread_callbacks_init ();
	mono_thread_info_init (sizeof (THREAD_INFO_TYPE));

	mono_init_coreclr_gc ();

	gc_inited = TRUE;
}

void
mono_gc_base_cleanup (void)
{
}

void
mono_gc_init_icalls (void)
{
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

guint64
mono_gc_get_allocated_bytes_for_current_thread (void)
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
	g_assert_not_reached ();
}

int64_t
mono_gc_get_heap_size (void)
{
	g_assert_not_reached ();
}

gboolean
mono_gc_is_gc_thread (void)
{
	return TRUE;
}

int
mono_gc_walk_heap (int flags, MonoGCReferences callback, void *data)
{
	g_assert_not_reached ();
}

gboolean
mono_object_is_alive (MonoObject* o)
{
	g_assert_not_reached ();
}

int
mono_gc_register_root (char *start, size_t size, MonoGCDescriptor descr, MonoGCRootSource source, void *key, const char *msg)
{
	g_assert_not_reached ();
}

int
mono_gc_register_root_wbarrier (char *start, size_t size, MonoGCDescriptor descr, MonoGCRootSource source, void *key, const char *msg)
{
	g_assert_not_reached ();
}

void
mono_gc_deregister_root (char* addr)
{
}

typedef struct {
	uint16_t m_componentSize;
	uint16_t m_flags;
	uint32_t m_baseSize;
} mono_gc_descr;

typedef union {
	mono_gc_descr struct_gc_descr;
	gpointer ptr_gc_descr;
} mono_gc_descr_union;

#define MTFlag_ContainsPointers     0x0100
#define MTFlag_HasCriticalFinalizer 0x0800
#define MTFlag_HasFinalizer         0x0010
#define MTFlag_IsArray              0x0008
#define MTFlag_Collectible          0x1000
#define MTFlag_HasComponentSize     0x8000

void*
mono_gc_make_descr_for_object (gpointer klass, gsize *bitmap, int numbits, size_t obj_size)
{
	MonoClass *casted_class = (MonoClass*) klass;
	mono_gc_descr_union gc_descr;
	gc_descr.struct_gc_descr.m_componentSize = 0; // not array or string
	gc_descr.struct_gc_descr.m_flags = MTFlag_ContainsPointers;
	if (casted_class->has_finalize)
		gc_descr.struct_gc_descr.m_flags |= MTFlag_HasFinalizer;
	gc_descr.struct_gc_descr.m_baseSize = obj_size + 8;
	return gc_descr.ptr_gc_descr;
}

void*
mono_gc_make_descr_for_string (gsize *bitmap, int numbits)
{
	mono_gc_descr_union gc_descr;
	gc_descr.struct_gc_descr.m_componentSize = 2;
	gc_descr.struct_gc_descr.m_flags = MTFlag_ContainsPointers | MTFlag_IsArray | MTFlag_HasComponentSize;
	gc_descr.struct_gc_descr.m_baseSize = sizeof (MonoString) + 8;
	return gc_descr.ptr_gc_descr;
}

void*
mono_gc_make_descr_for_array (int vector, gsize *elem_bitmap, int numbits, size_t elem_size)
{
	mono_gc_descr_union gc_descr;
	gc_descr.struct_gc_descr.m_componentSize = elem_size;
	gc_descr.struct_gc_descr.m_flags = MTFlag_ContainsPointers | MTFlag_IsArray | MTFlag_HasComponentSize;
	gc_descr.struct_gc_descr.m_baseSize = sizeof (MonoArray) + 8;
	return gc_descr.ptr_gc_descr;
}

void*
mono_gc_make_descr_from_bitmap (gsize *bitmap, int numbits)
{
	g_assert_not_reached ();
}

void*
mono_gc_make_vector_descr (void)
{
	g_assert_not_reached ();
}

void*
mono_gc_make_root_descr_all_refs (int numbits)
{
	g_assert_not_reached ();
	return NULL;
}

MonoObject*
mono_gc_alloc_fixed (size_t size, void *descr, MonoGCRootSource source, void *key, const char *msg)
{
	g_assert_not_reached ();
}

MonoObject*
mono_gc_alloc_fixed_no_descriptor (size_t size, MonoGCRootSource source, void *key, const char *msg)
{
	g_assert_not_reached ();
}

void
mono_gc_free_fixed (void* addr)
{
	g_assert_not_reached ();
}

MonoObject*
mono_gc_alloc_obj (MonoVTable *vtable, size_t size)
{
	g_assert_not_reached ();
}

MonoArray*
mono_gc_alloc_vector (MonoVTable *vtable, size_t size, uintptr_t max_length)
{
	g_assert_not_reached ();
}

MonoArray*
mono_gc_alloc_array (MonoVTable *vtable, size_t size, uintptr_t max_length, uintptr_t bounds_size)
{
	g_assert_not_reached ();
}

MonoString*
mono_gc_alloc_string (MonoVTable *vtable, size_t size, gint32 len)
{
	g_assert_not_reached ();
}

MonoObject*
mono_gc_alloc_mature (MonoVTable *vtable, size_t size)
{
	g_assert_not_reached ();
}

MonoObject*
mono_gc_alloc_pinned_obj (MonoVTable *vtable, size_t size)
{
	g_assert_not_reached ();
}

void
mono_gc_wbarrier_set_field_internal (MonoObject *obj, gpointer field_ptr, MonoObject* value)
{
	g_assert_not_reached ();
}

void
mono_gc_wbarrier_set_arrayref_internal (MonoArray *arr, gpointer slot_ptr, MonoObject* value)
{
	g_assert_not_reached ();
}

void
mono_gc_wbarrier_arrayref_copy_internal (gpointer dest_ptr, gconstpointer src_ptr, int count)
{
	g_assert_not_reached ();
}

void
mono_gc_wbarrier_generic_store_internal (void volatile* ptr, MonoObject* value)
{
	g_assert_not_reached ();
}

void
mono_gc_wbarrier_generic_store_atomic_internal (gpointer ptr, MonoObject *value)
{
	g_assert_not_reached ();
}

void
mono_gc_wbarrier_generic_nostore_internal (gpointer ptr)
{
	g_assert_not_reached ();
}

void
mono_gc_wbarrier_value_copy_internal (gpointer dest, gconstpointer src, int count, MonoClass *klass)
{
	g_assert_not_reached ();
}

static int
get_size_for_vtable (gpointer vtable, gpointer o)
{
	MonoClass *klass = ((MonoVTable*)vtable)->klass;

	// FIXME: use gc desc for fast path

	/*
	 * We depend on mono_string_length_fast and
	 * mono_array_length_internal not using the object's vtable.
	 */
	if (klass == mono_defaults.string_class) {
		return MONO_SIZEOF_MONO_STRING + 2 * mono_string_length_fast ((MonoString*) o) + 2;
	} else if (m_class_get_rank (klass)) {
		g_error ("niy");
		// return sgen_mono_array_size (vtable, (MonoArray*)o, NULL, 0);
	} else {
		/* from a created object: the class must be inited already */
		return m_class_get_instance_size (klass);
	}
}

#if TARGET_SIZEOF_VOID_P == 8
#define card_byte_shift     11
#else
#define card_byte_shift     10
#endif

#define card_byte(addr) (((size_t)(addr)) >> card_byte_shift)

extern "C" guint32* g_gc_card_table;
extern "C" guint8* g_gc_lowest_address;
extern "C" guint8* g_gc_highest_address;

static void
coregc_mark_card_table (gpointer addr)
{
    if (((guint8 *) addr < g_gc_lowest_address) || ((guint8 *) addr >= g_gc_highest_address))
        return;

    // volatile is used here to prevent fetch of g_card_table from being reordered
    // with g_lowest/highest_address check above. See comments in StompWriteBarrier
    guint8* cardByte = (guint8 *)*(volatile guint8 **)(&g_gc_card_table) + card_byte((guint8 *)addr);
	*cardByte = 0xff;
}

#ifndef MAX
#define MAX(a,b) (((a)>(b)) ? (a) : (b))
#endif

static int
number_of_cards (gpointer start, int size)
{
	gpointer end = (guint8 *) start + MAX (1, size) - 1;
	return ((intptr_t) end >> card_byte_shift) - ((intptr_t) start >> card_byte_shift) + 1;
}

void
mono_gc_wbarrier_object_copy_internal (MonoObject* obj, MonoObject *src)
{
	size_t size = get_size_for_vtable (obj->vtable, obj);

	// TLAB_ACCESS_INIT;
	// ENTER_CRITICAL_REGION;

	mono_gc_memmove_aligned ((char *) obj + COREGC_CLIENT_OBJECT_HEADER_SIZE, (char *) src + COREGC_CLIENT_OBJECT_HEADER_SIZE, size - COREGC_CLIENT_OBJECT_HEADER_SIZE);

	int num_cards = number_of_cards (obj, size);
#if 0
	memset (obj, 0xff, num_cards);
#else
	for (int i = 0; i < size; i++)
		coregc_mark_card_table ((char *) obj + i);
#endif

	// EXIT_CRITICAL_REGION;
}

gboolean
mono_gc_is_critical_method (MonoMethod *method)
{
	g_assert_not_reached ();
}

gpointer
mono_gc_thread_attach (THREAD_INFO_TYPE * info)
{
	g_assert_not_reached ();
}

void
mono_gc_thread_detach_with_lock (THREAD_INFO_TYPE *p)
{
	g_assert_not_reached ();
}

gboolean
mono_gc_thread_in_critical_region (THREAD_INFO_TYPE *info)
{
	g_assert_not_reached ();
}

int
mono_gc_get_aligned_size_for_allocator (int size)
{
	g_assert_not_reached ();
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
mono_gc_get_managed_allocator_by_type (int atype, ManagedAllocatorVariant variant)
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
	return "coregc";
}

void
mono_gc_clear_domain (MonoDomain *domain)
{
}

void
mono_gc_suspend_finalizers (void)
{
	g_assert_not_reached ();
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
	return NULL;
}

void*
mono_gc_invoke_with_gc_lock (MonoGCLockedCallbackFunc func, void *data)
{
	g_assert_not_reached ();
}

char*
mono_gc_get_description (void)
{
	return g_strdup ("coregc");
}

void
mono_gc_set_desktop_mode (void)
{
	g_assert_not_reached ();
}

gboolean
mono_gc_is_moving (void)
{
	return TRUE;
}

gboolean
mono_gc_is_disabled (void)
{
	return FALSE;
}

void
mono_gc_wbarrier_range_copy (gpointer _dest, gconstpointer _src, int size)
{
	g_assert_not_reached ();
}

MonoRangeCopyFunction
mono_gc_get_range_copy_func (void)
{
	return &mono_gc_wbarrier_range_copy;
}

guint8*
mono_gc_get_card_table (int *shift_bits, gpointer *card_mask)
{
	g_assert_not_reached ();
	return NULL;
}

guint8*
mono_gc_get_target_card_table (int *shift_bits, target_mgreg_t *card_mask)
{
	g_assert_not_reached ();
}

gboolean
mono_gc_card_table_nursery_check (void)
{
	g_assert_not_reached ();
	return TRUE;
}

void
mono_gc_register_obj_with_weak_fields (void *obj)
{
	g_assert_not_reached ();
}

void*
mono_gc_get_nursery (int *shift_bits, size_t *size)
{
	g_assert_not_reached ();
	return NULL;
}

gboolean
mono_gc_precise_stack_mark_enabled (void)
{
	g_assert_not_reached ();
	return FALSE;
}

FILE *
mono_gc_get_logfile (void)
{
	return NULL;
}

void
mono_gc_params_set (const char* options)
{
}

void
mono_gc_debug_set (const char* options)
{
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
	g_assert_not_reached ();
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
int
mono_gc_pthread_create (pthread_t *new_thread, const pthread_attr_t *attr, void *(*start_routine)(void *), void *arg)
{
	g_assert_not_reached ();
	return pthread_create (new_thread, attr, start_routine, arg);
}
#endif

void
mono_gc_skip_thread_changing (gboolean skip)
{
	g_assert_not_reached ();
	// No STW, nothing needs to be done.
}

void
mono_gc_skip_thread_changed (gboolean skip)
{
}

#ifdef HOST_WIN32
BOOL APIENTRY mono_gc_dllmain (HMODULE module_handle, DWORD reason, LPVOID reserved)
{
	return TRUE;
}
#endif

MonoVTable *
mono_gc_get_vtable (MonoObject *obj)
{
	g_assert_not_reached ();
	// No pointer tagging.
	return obj->vtable;
}

guint
mono_gc_get_vtable_bits (MonoClass *klass)
{
	g_assert_not_reached ();
	return 0;
}

void
mono_gc_register_altstack (gpointer stack, gint32 stack_size, gpointer altstack, gint32 altstack_size)
{
}

gboolean
mono_gc_is_null (void)
{
	return FALSE;
}

int
mono_gc_invoke_finalizers (void)
{
	return 0;
}

MonoBoolean
mono_gc_pending_finalizers (void)
{
	return FALSE;
}

gboolean
mono_gc_ephemeron_array_add (MonoObject *obj)
{
	return TRUE;
}

guint64 mono_gc_get_total_allocated_bytes (MonoBoolean precise)
{
	return 0;
}

MonoObject*
mono_gchandle_get_target_internal (guint32 gchandle)
{
	g_assert_not_reached ();
}

gboolean
mono_gchandle_is_in_domain (guint32 gchandle, MonoDomain *domain)
{
	g_assert_not_reached ();
}

guint32
mono_gchandle_new_internal (MonoObject *obj, gboolean pinned)
{
	g_assert_not_reached ();
}

guint32
mono_gchandle_new_weakref_internal (MonoObject* obj, gboolean track_resurrection)
{
	g_assert_not_reached ();
}

void
mono_gchandle_set_target (guint32 gchandle, MonoObject *obj)
{
	g_assert_not_reached ();
}

void
mono_gchandle_free_internal (guint32 gchandle)
{
	g_assert_not_reached ();
}

void
mono_gchandle_free_domain (MonoDomain *unloading)
{
	g_assert_not_reached ();
}

G_END_DECLS

// Interface which coregc library links against

#define THREADS_STW_DEBUG(...)

static gboolean
coreclr_is_thread_in_current_stw (THREAD_INFO_TYPE *info, int *reason)
{
	/*
	 * No need to check MONO_THREAD_INFO_FLAGS_NO_GC here as we rely on the
	 * FOREACH_THREAD_EXCLUDE macro to skip such threads for us.
	 */

	/*
	We have detected that this thread is failing/dying, ignore it.
	FIXME: can't we merge this with thread_is_dying?
	*/
	if (info->skip) {
		if (reason)
			*reason = 2;
		return FALSE;
	}

	/*
	Suspending the current thread will deadlock us, bad idea.
	*/
	if (info == mono_thread_info_current ()) {
		if (reason)
			*reason = 3;
		return FALSE;
	}

	/*
	We can't suspend the workers that will do all the heavy lifting.
	FIXME Use some state bit in SgenThreadInfo for this.
	*/
        #if 0
	if (sgen_thread_pool_is_thread_pool_thread (mono_thread_info_get_tid (info))) {
		if (reason)
			*reason = 4;
		return FALSE;
	}
        #endif

	/*
	The thread has signaled that it started to detach, ignore it.
	FIXME: can't we merge this with skip
	*/
	if (!mono_thread_info_is_live (info)) {
		if (reason)
			*reason = 5;
		return FALSE;
	}

	return TRUE;
}

static void
coregc_unified_suspend_stop_world (void)
{
        printf("coregc_unified_suspend_stop_world\n");
	int sleep_duration = -1;

	// we can't lead STW if we promised not to safepoint.
	g_assert (!mono_thread_info_will_not_safepoint (mono_thread_info_current ()));

	mono_threads_begin_global_suspend ();
	THREADS_STW_DEBUG ("[GC-STW-BEGIN][%p] *** BEGIN SUSPEND *** \n", mono_thread_info_get_tid (mono_thread_info_current ()));

	for (MonoThreadSuspendPhase phase = MONO_THREAD_SUSPEND_PHASE_INITIAL; phase < MONO_THREAD_SUSPEND_PHASE_COUNT; phase++) {
		gboolean need_next_phase = FALSE;
		FOREACH_THREAD_EXCLUDE (info, MONO_THREAD_INFO_FLAGS_NO_GC) {
			/* look at every thread in the first phase. */
			if (phase == MONO_THREAD_SUSPEND_PHASE_INITIAL) {
                                #if 1
				info->skip = FALSE;
				info->suspend_done = FALSE;
                                #endif
			} else {
				/* skip threads suspended by previous phase. */
				/* threads with info->skip set to TRUE will be skipped by coreclr_is_thread_in_current_stw. */
                                #if 1
				if (info->suspend_done)
					continue;
                                #endif
			}

			int reason;
			if (!coreclr_is_thread_in_current_stw(info, &reason)) {
				THREADS_STW_DEBUG ("[GC-STW-BEGIN-SUSPEND-%d] IGNORE thread %p skip %s reason %d\n", (int)phase, mono_thread_info_get_tid (info), info->skip ? "true" : "false", reason);
				continue;
			}

			switch (mono_thread_info_begin_suspend (info, phase)) {
			case MONO_THREAD_BEGIN_SUSPEND_SUSPENDED:
                                #if 1
				info->skip = FALSE;
                                #endif
				break;
			case MONO_THREAD_BEGIN_SUSPEND_SKIP:
                                #if 1
				info->skip = TRUE;
                                #endif
				break;
			case MONO_THREAD_BEGIN_SUSPEND_NEXT_PHASE:
				need_next_phase = TRUE;
				break;
			default:
				g_assert_not_reached ();
			}

			THREADS_STW_DEBUG ("[GC-STW-BEGIN-SUSPEND-%d] SUSPEND thread %p skip %s\n", (int)phase, mono_thread_info_get_tid (info), info->skip ? "true" : "false");
		} FOREACH_THREAD_END;

                #if 1
		mono_thread_info_current ()->suspend_done = TRUE;
                #endif
		mono_threads_wait_pending_operations ();

		if (!need_next_phase)
			break;
	}

	for (;;) {
		gint restart_counter = 0;

		FOREACH_THREAD_EXCLUDE (info, MONO_THREAD_INFO_FLAGS_NO_GC) {
			gint suspend_count;

			int reason = 0;
			if (info->suspend_done || !coreclr_is_thread_in_current_stw (info, &reason)) {
				THREADS_STW_DEBUG ("[GC-STW-RESTART] IGNORE RESUME thread %p not been processed done %d current %d reason %d\n", mono_thread_info_get_tid (info), info->suspend_done, !coreclr_is_thread_in_current_stw (info, NULL), reason);
				continue;
			}

			/*
			All threads that reach here are pristine suspended. This means the following:

			- We haven't accepted the previous suspend as good.
			- We haven't gave up on it for this STW (it's either bad or asked not to)
			*/
			if (!mono_thread_info_in_critical_location (info)) {
				info->suspend_done = TRUE;

				THREADS_STW_DEBUG ("[GC-STW-RESTART] DONE thread %p deemed fully suspended\n", mono_thread_info_get_tid (info));
				continue;
			}

			suspend_count = mono_thread_info_suspend_count (info);
			if (!(suspend_count == 1))
				g_error ("[%p] suspend_count = %d, but should be 1", mono_thread_info_get_tid (info), suspend_count);

			info->skip = !mono_thread_info_begin_pulse_resume_and_request_suspension (info);
			if (!info->skip)
				restart_counter += 1;

			THREADS_STW_DEBUG ("[GC-STW-RESTART] RESTART thread %p skip %s\n", mono_thread_info_get_tid (info), info->skip ? "true" : "false");
		} FOREACH_THREAD_END

		mono_threads_wait_pending_operations ();

		if (restart_counter == 0)
			break;

		if (sleep_duration < 0) {
			mono_thread_info_yield ();
			sleep_duration = 0;
		} else {
			g_usleep (sleep_duration);
			sleep_duration += 10;
		}

		FOREACH_THREAD_EXCLUDE (info, MONO_THREAD_INFO_FLAGS_NO_GC) {
			int reason = 0;
			if (info->suspend_done || !coreclr_is_thread_in_current_stw (info, &reason)) {
				THREADS_STW_DEBUG ("[GC-STW-RESTART] IGNORE SUSPEND thread %p not been processed done %d current %d reason %d\n", mono_thread_info_get_tid (info), info->suspend_done, !coreclr_is_thread_in_current_stw (info, NULL), reason);
				continue;
			}

			if (!mono_thread_info_is_running (info)) {
				THREADS_STW_DEBUG ("[GC-STW-RESTART] IGNORE SUSPEND thread %p not running\n", mono_thread_info_get_tid (info));
				continue;
			}

			switch (mono_thread_info_begin_suspend (info, MONO_THREAD_SUSPEND_PHASE_MOPUP)) {
			case MONO_THREAD_BEGIN_SUSPEND_SUSPENDED:
				info->skip = FALSE;
				break;
			case MONO_THREAD_BEGIN_SUSPEND_SKIP:
				info->skip = TRUE;
				break;
			case MONO_THREAD_BEGIN_SUSPEND_NEXT_PHASE:
				g_assert_not_reached ();
			default:
				g_assert_not_reached ();
			}

			THREADS_STW_DEBUG ("[GC-STW-RESTART] SUSPEND thread %p skip %s\n", mono_thread_info_get_tid (info), info->skip ? "true" : "false");
		} FOREACH_THREAD_END

		mono_threads_wait_pending_operations ();
	}

	FOREACH_THREAD_EXCLUDE (info, MONO_THREAD_INFO_FLAGS_NO_GC) {
		gpointer stopped_ip;

		int reason = 0;
		if (!coreclr_is_thread_in_current_stw (info, &reason)) {
			g_assert (!info->suspend_done || info == mono_thread_info_current ());

			THREADS_STW_DEBUG ("[GC-STW-SUSPEND-END] thread %p is NOT suspended, reason %d\n", mono_thread_info_get_tid (info), reason);
			continue;
		}

		g_assert (info->suspend_done);

		info->ctx = mono_thread_info_get_suspend_state (info)->ctx;

		/* Once we remove the old suspend code, we should move sgen to directly access the state in MonoThread */
		info->stack_start = (gpointer) ((char*)MONO_CONTEXT_GET_SP (&info->ctx) - REDZONE_SIZE);

		if (info->stack_start < info->info.stack_start_limit
			 || info->stack_start >= info->info.stack_end) {
			/*
			 * Thread context is in unhandled state, most likely because it is
			 * dying. We don't scan it.
			 * FIXME We should probably rework and check the valid flag instead.
			 */
			info->stack_start = NULL;
		}

		stopped_ip = (gpointer) (MONO_CONTEXT_GET_IP (&info->ctx));

                #if 0
		sgen_binary_protocol_thread_suspend ((gpointer) mono_thread_info_get_tid (info), stopped_ip);
                #endif

		THREADS_STW_DEBUG ("[GC-STW-SUSPEND-END] thread %p is suspended, stopped_ip = %p, stack = %p -> %p\n",
			mono_thread_info_get_tid (info), stopped_ip, info->stack_start, info->stack_start ? info->info.stack_end : NULL);
	} FOREACH_THREAD_END
}

static void
coreclr_unified_suspend_restart_world (void)
{
        printf("coreclr_unified_suspend_restart_world\n");
	THREADS_STW_DEBUG ("[GC-STW-END] *** BEGIN RESUME ***\n");
	FOREACH_THREAD_EXCLUDE (info, MONO_THREAD_INFO_FLAGS_NO_GC) {
		int reason = 0;
		if (coreclr_is_thread_in_current_stw (info, &reason)) {
			g_assert (mono_thread_info_begin_resume (info));
			THREADS_STW_DEBUG ("[GC-STW-RESUME-WORLD] RESUME thread %p\n", mono_thread_info_get_tid (info));

                        #if 0
			sgen_binary_protocol_thread_restart ((gpointer) mono_thread_info_get_tid (info));
                        #endif
		} else {
			THREADS_STW_DEBUG ("[GC-STW-RESUME-WORLD] IGNORE thread %p, reason %d\n", mono_thread_info_get_tid (info), reason);
		}
	} FOREACH_THREAD_END

	mono_threads_wait_pending_operations ();
	mono_threads_end_global_suspend ();
}

void GCToEEInterface::SuspendEE(SUSPEND_REASON reason)
{
	pGCHeap->SetGCInProgress(true);
        coregc_unified_suspend_stop_world();
}

void GCToEEInterface::RestartEE(bool bFinishedGC)
{
        coreclr_unified_suspend_restart_world();
	pGCHeap->SetGCInProgress(false);
}

void GCToEEInterface::GcScanRoots(promote_func* fn,  int condemned, int max_gen, ScanContext* sc)
{
}

void GCToEEInterface::GcStartWork(int condemned, int max_gen)
{
}

void GCToEEInterface::AfterGcScanRoots(int condemned, int max_gen, ScanContext* sc)
{
}

void GCToEEInterface::GcBeforeBGCSweepWork()
{
}

void GCToEEInterface::GcDone(int condemned)
{
}

bool GCToEEInterface::RefCountedHandleCallbacks(Object * pObject)
{
	return false;
}

bool GCToEEInterface::IsPreemptiveGCDisabled()
{
}

bool GCToEEInterface::EnablePreemptiveGC()
{
	return false;
}

void GCToEEInterface::DisablePreemptiveGC()
{
}

Thread* GCToEEInterface::GetThread()
{
	return NULL;
}

gc_alloc_context * GCToEEInterface::GetAllocContext()
{
	return NULL;
}

void GCToEEInterface::GcEnumAllocContexts (enum_alloc_context_func* fn, void* param)
{
}

uint8_t* GCToEEInterface::GetLoaderAllocatorObjectForGC(Object* pObject)
{
	return NULL;
}

void GCToEEInterface::SyncBlockCacheWeakPtrScan(HANDLESCANPROC /*scanProc*/, uintptr_t /*lp1*/, uintptr_t /*lp2*/)
{
}

void GCToEEInterface::SyncBlockCacheDemote(int /*max_gen*/)
{
}

void GCToEEInterface::SyncBlockCachePromotionsGranted(int /*max_gen*/)
{
}

void GCToEEInterface::DiagGCStart(int gen, bool isInduced)
{
}

void GCToEEInterface::DiagUpdateGenerationBounds()
{
}

void GCToEEInterface::DiagGCEnd(size_t index, int gen, int reason, bool fConcurrent)
{
}

void GCToEEInterface::DiagWalkFReachableObjects(void* gcContext)
{
}

void GCToEEInterface::DiagWalkSurvivors(void* gcContext, bool fCompacting)
{
}

void GCToEEInterface::DiagWalkLOHSurvivors(void* gcContext)
{
}

void GCToEEInterface::DiagWalkBGCSurvivors(void* gcContext)
{
}

void GCToEEInterface::StompWriteBarrier(WriteBarrierParameters* args)
{
}

void GCToEEInterface::EnableFinalization(bool foundFinalizers)
{
    // Signal to finalizer thread that there are objects to finalize
    // TODO: Implement for finalization
}

void GCToEEInterface::HandleFatalError(unsigned int exitCode)
{
	abort();
}

bool GCToEEInterface::EagerFinalized(Object* obj)
{
	return false;
}

bool GCToEEInterface::GetBooleanConfigValue(const char* key, bool* value)
{
    return false;
}

bool GCToEEInterface::GetIntConfigValue(const char* key, int64_t* value)
{
	return false;
}

bool GCToEEInterface::GetStringConfigValue(const char* key, const char** value)
{
	return false;
}

void GCToEEInterface::FreeStringConfigValue(const char *value)
{

}

bool GCToEEInterface::IsGCThread()
{
	return false;
}

bool GCToEEInterface::WasCurrentThreadCreatedByGC()
{
	return false;
}

/* Vtable of the objects used to fill out nursery fragments before a collection */
static MonoVTable *array_fill_vtable;

MethodTable* GCToEEInterface::GetFreeObjectMethodTable()
{
	if (!array_fill_vtable) {
		static char _vtable[sizeof(MonoVTable)+8];
		MonoVTable* vtable = (MonoVTable*) ALIGN_TO((size_t)_vtable, 8);
		gsize bmap;

		vtable->klass = mono_class_create_array_fill_type ();

		bmap = 0;
		mono_gc_descr_union desc;
		desc.ptr_gc_descr = mono_gc_make_descr_for_array (TRUE, &bmap, 0, 1);
		// Remove bounds from the reported size, since coreclr gc expects this object
		// to the size of ArrayBase
		desc.struct_gc_descr.m_baseSize -= 8;
		vtable->gc_descr = desc.ptr_gc_descr;
		vtable->rank = 1;

		array_fill_vtable = vtable;
	}
	return (MethodTable*)array_fill_vtable;
}

bool GCToEEInterface::CreateThread(void (*threadStart)(void*), void* arg, bool is_suspendable, const char* name)
{
	return false;
}

void GCToEEInterface::WalkAsyncPinnedForPromotion(Object* object, ScanContext* sc, promote_func* callback)
{
}

void GCToEEInterface::WalkAsyncPinned(Object* object, void* context, void (*callback)(Object*, Object*, void*))
{
}

uint32_t GCToEEInterface::GetTotalNumSizedRefHandles()
{
	return -1;
}

void GCToEEInterface::UpdateGCEventStatus(int publicLevel, int publicKeywords, int privateLevel, int privateKeywords)
{
}

inline bool GCToEEInterface::AnalyzeSurvivorsRequested(int condemnedGeneration)
{
	return false;
}

inline void GCToEEInterface::AnalyzeSurvivorsFinished(int condemnedGeneration)
{

}
