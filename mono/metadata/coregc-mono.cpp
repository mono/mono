/**
 * \file
 * GC implementation using CoreCLR GC
 *
 * Copyright 2019 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include "config.h"

#include <glib.h>
#include <mono/metadata/mono-gc.h>
#include <mono/metadata/gc-internals.h>
#include <mono/metadata/runtime.h>
#include <mono/metadata/w32handle.h>
#include <mono/metadata/abi-details.h>
#include <mono/utils/atomic.h>
#include <mono/utils/mono-threads.h>
#include <mono/utils/mono-counters.h>
#include <mono/metadata/null-gc-handles.h>
#include "coregc-mono.h"

static gboolean gc_inited = FALSE;

G_BEGIN_DECLS

extern "C" HRESULT GC_Initialize(IGCToCLR* clrToGC, IGCHeap** gcHeap, IGCHandleManager** gcHandleManager, GcDacVars* gcDacVars);

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
	mono_thread_info_init (sizeof (MonoThreadInfo));

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
mono_gc_register_root (char *start, size_t size, void *descr, MonoGCRootSource source, void *key, const char *msg)
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

void*
mono_gc_make_descr_for_string (gsize *bitmap, int numbits)
{
	g_assert_not_reached ();
}

void*
mono_gc_make_descr_for_object (gsize *bitmap, int numbits, size_t obj_size)
{
	g_assert_not_reached ();
}

void*
mono_gc_make_descr_for_array (int vector, gsize *elem_bitmap, int numbits, size_t elem_size)
{
	g_assert_not_reached ();
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

void
mono_gc_wbarrier_object_copy_internal (MonoObject* obj, MonoObject *src)
{
	g_assert_not_reached ();
}

gboolean
mono_gc_is_critical_method (MonoMethod *method)
{
	g_assert_not_reached ();
}

gpointer
mono_gc_thread_attach (MonoThreadInfo* info)
{
	g_assert_not_reached ();
}

void
mono_gc_thread_detach_with_lock (MonoThreadInfo *p)
{
	g_assert_not_reached ();
}

gboolean
mono_gc_thread_in_critical_region (MonoThreadInfo *info)
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
	g_assert_not_reached ();
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

void GCToEEInterface::SuspendEE(SUSPEND_REASON reason)
{
	pGCHeap->SetGCInProgress(true);
}

void GCToEEInterface::RestartEE(bool bFinishedGC)
{
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

MethodTable* GCToEEInterface::GetFreeObjectMethodTable()
{
	//
	// Initialize free object methodtable. The GC uses a special array-like methodtable as placeholder
	// for collected free space.
	//
	static MethodTable *freeObjectMT;
	if (!freeObjectMT) {
		freeObjectMT = (MethodTable *) g_malloc (sizeof (MethodTable));
		freeObjectMT->InitializeFreeObject();
	}
	return freeObjectMT;
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
