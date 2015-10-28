/*
 * sgen-client-mono.h: Mono's client definitions for SGen.
 *
 * Copyright (C) 2014 Xamarin Inc
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Library General Public
 * License 2.0 as published by the Free Software Foundation;
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Library General Public License for more details.
 *
 * You should have received a copy of the GNU Library General Public
 * License 2.0 along with this library; if not, write to the Free
 * Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 */

#ifdef SGEN_DEFINE_OBJECT_VTABLE

#include "sgen/sgen-archdep.h"
#include "utils/mono-threads.h"
#include "utils/mono-mmap.h"
#include "metadata/object-internals.h"

typedef MonoObject GCObject;
typedef MonoVTable* GCVTable;

static inline GCVTable
SGEN_LOAD_VTABLE_UNCHECKED (GCObject *obj)
{
	return obj->vtable;
}

static inline SgenDescriptor
sgen_vtable_get_descriptor (GCVTable vtable)
{
	return (SgenDescriptor)vtable->gc_descr;
}

typedef struct _SgenClientThreadInfo SgenClientThreadInfo;
struct _SgenClientThreadInfo {
	MonoThreadInfo info;

	/*
	 * `skip` is set to TRUE when STW fails to suspend a thread, most probably because
	 * the underlying thread is dead.
	*/
	gboolean skip, suspend_done;
	volatile int in_critical_region;

	gpointer stopped_ip;	/* only valid if the thread is stopped */
	MonoDomain *stopped_domain; /* dsto */

	/*
	This is set the argument of mono_gc_set_skip_thread.

	A thread that knowingly holds no managed state can call this
	function around blocking loops to reduce the GC burden by not
	been scanned.
	*/
	gboolean gc_disabled;

#ifdef SGEN_POSIX_STW
	/* This is -1 until the first suspend. */
	int signal;
	/* FIXME: kill this, we only use signals on systems that have rt-posix, which doesn't have issues with duplicates. */
	unsigned int stop_count; /* to catch duplicate signals. */
#endif

	gpointer runtime_data;

	void *stack_end;
	void *stack_start;
	void *stack_start_limit;

	/*FIXME pretty please finish killing ARCH_NUM_REGS */
#ifdef USE_MONO_CTX
	MonoContext ctx;		/* ditto */
#else
	gpointer regs[ARCH_NUM_REGS];	    /* ditto */
#endif
};

#else

#include "metadata/profiler-private.h"
#include "utils/dtrace.h"
#include "utils/mono-counters.h"
#include "utils/mono-logger-internal.h"
#include "utils/mono-time.h"
#include "utils/mono-semaphore.h"
#include "metadata/sgen-bridge-internal.h"

extern void mono_sgen_register_moved_object (void *obj, void *destination);
extern void mono_sgen_gc_event_moves (void);

extern void mono_sgen_init_stw (void);

enum {
	INTERNAL_MEM_EPHEMERON_LINK = INTERNAL_MEM_FIRST_CLIENT,
	INTERNAL_MEM_MAX
};

static inline mword
sgen_mono_array_size (GCVTable vtable, MonoArray *array, mword *bounds_size, mword descr)
{
	mword size, size_without_bounds;
	int element_size;

	if ((descr & DESC_TYPE_MASK) == DESC_TYPE_VECTOR)
		element_size = ((descr) >> VECTOR_ELSIZE_SHIFT) & MAX_ELEMENT_SIZE;
	else
		element_size = vtable->klass->sizes.element_size;

	size_without_bounds = size = sizeof (MonoArray) + element_size * mono_array_length_fast (array);

	if (G_UNLIKELY (array->bounds)) {
		size += sizeof (mono_array_size_t) - 1;
		size &= ~(sizeof (mono_array_size_t) - 1);
		size += sizeof (MonoArrayBounds) * vtable->klass->rank;
	}

	if (bounds_size)
		*bounds_size = size - size_without_bounds;
	return size;
}

#define SGEN_CLIENT_OBJECT_HEADER_SIZE		(sizeof (GCObject))
#define SGEN_CLIENT_MINIMUM_OBJECT_SIZE		SGEN_CLIENT_OBJECT_HEADER_SIZE

static mword /*__attribute__((noinline)) not sure if this hint is a good idea*/
sgen_client_slow_object_get_size (GCVTable vtable, GCObject* o)
{
	MonoClass *klass = ((MonoVTable*)vtable)->klass;

	/*
	 * We depend on mono_string_length_fast and
	 * mono_array_length_fast not using the object's vtable.
	 */
	if (klass == mono_defaults.string_class) {
		return G_STRUCT_OFFSET (MonoString, chars) + 2 * mono_string_length_fast ((MonoString*) o) + 2;
	} else if (klass->rank) {
		return sgen_mono_array_size (vtable, (MonoArray*)o, NULL, 0);
	} else {
		/* from a created object: the class must be inited already */
		return klass->instance_size;
	}
}

/*
 * This function can be called on an object whose first word, the
 * vtable field, is not intact.  This is necessary for the parallel
 * collector.
 */
static MONO_NEVER_INLINE mword
sgen_client_par_object_get_size (GCVTable vtable, GCObject* o)
{
	SgenDescriptor descr = sgen_vtable_get_descriptor (vtable);
	mword type = descr & DESC_TYPE_MASK;

	if (type == DESC_TYPE_RUN_LENGTH || type == DESC_TYPE_SMALL_PTRFREE) {
		mword size = descr & 0xfff8;
		SGEN_ASSERT (9, size >= sizeof (MonoObject), "Run length object size to small");
		return size;
	} else if (descr == SGEN_DESC_STRING) {
		return G_STRUCT_OFFSET (MonoString, chars) + 2 * mono_string_length_fast ((MonoString*) o) + 2;
	} else if (type == DESC_TYPE_VECTOR) {
		return sgen_mono_array_size (vtable, (MonoArray*)o, NULL, descr);
	}

	return sgen_client_slow_object_get_size (vtable, o);
}

static MONO_ALWAYS_INLINE size_t G_GNUC_UNUSED
sgen_client_array_element_size (GCVTable gc_vtable)
{
	MonoVTable *vt = (MonoVTable*)gc_vtable;
	return mono_array_element_size (vt->klass);
}

static MONO_ALWAYS_INLINE G_GNUC_UNUSED char*
sgen_client_array_data_start (GCObject *obj)
{
	return (char*)(obj) +  G_STRUCT_OFFSET (MonoArray, vector);
}

static MONO_ALWAYS_INLINE size_t G_GNUC_UNUSED
sgen_client_array_length (GCObject *obj)
{
	return mono_array_length_fast ((MonoArray*)obj);
}

static MONO_ALWAYS_INLINE gboolean G_GNUC_UNUSED
sgen_client_object_is_array_fill (GCObject *o)
{
	return ((MonoObject*)o)->synchronisation == GINT_TO_POINTER (-1);
}

static MONO_ALWAYS_INLINE void G_GNUC_UNUSED
sgen_client_pre_copy_checks (char *destination, GCVTable gc_vtable, void *obj, mword objsize)
{
	MonoVTable *vt = (MonoVTable*)gc_vtable;
	SGEN_ASSERT (9, vt->klass->inited, "vtable %p for class %s:%s was not initialized", vt, vt->klass->name_space, vt->klass->name);
}

static MONO_ALWAYS_INLINE void G_GNUC_UNUSED
sgen_client_update_copied_object (char *destination, GCVTable gc_vtable, void *obj, mword objsize)
{
	MonoVTable *vt = (MonoVTable*)gc_vtable;
	if (G_UNLIKELY (vt->rank && ((MonoArray*)obj)->bounds)) {
		MonoArray *array = (MonoArray*)destination;
		array->bounds = (MonoArrayBounds*)((char*)destination + ((char*)((MonoArray*)obj)->bounds - (char*)obj));
		SGEN_LOG (9, "Array instance %p: size: %lu, rank: %d, length: %lu", array, (unsigned long)objsize, vt->rank, (unsigned long)mono_array_length (array));
	}

	if (G_UNLIKELY (mono_profiler_events & MONO_PROFILE_GC_MOVES))
		mono_sgen_register_moved_object (obj, destination);
}

#ifdef XDOMAIN_CHECKS_IN_WBARRIER
extern gboolean sgen_mono_xdomain_checks;

#define sgen_client_wbarrier_generic_nostore_check(ptr) do {		\
		/* FIXME: ptr_in_heap must be called with the GC lock held */ \
		if (sgen_mono_xdomain_checks && *(MonoObject**)ptr && ptr_in_heap (ptr)) { \
			char *start = find_object_for_ptr (ptr);	\
			MonoObject *value = *(MonoObject**)ptr;		\
			LOCK_GC;					\
			SGEN_ASSERT (0, start, "Write barrier outside an object?"); \
			if (start) {					\
				MonoObject *obj = (MonoObject*)start;	\
				if (obj->vtable->domain != value->vtable->domain) \
					SGEN_ASSERT (0, is_xdomain_ref_allowed (ptr, start, obj->vtable->domain), "Cross-domain ref not allowed"); \
			}						\
			UNLOCK_GC;					\
		}							\
	} while (0)
#else
#define sgen_client_wbarrier_generic_nostore_check(ptr)
#endif

static gboolean G_GNUC_UNUSED
sgen_client_object_has_critical_finalizer (GCObject *obj)
{
	MonoClass *class;

	if (!mono_defaults.critical_finalizer_object)
		return FALSE;

	class = SGEN_LOAD_VTABLE (obj)->klass;

	return mono_class_has_parent_fast (class, mono_defaults.critical_finalizer_object);
}

const char* sgen_client_vtable_get_namespace (GCVTable vtable);
const char* sgen_client_vtable_get_name (GCVTable vtable);

static gboolean G_GNUC_UNUSED
sgen_client_bridge_need_processing (void)
{
	return sgen_need_bridge_processing ();
}

static void G_GNUC_UNUSED
sgen_client_bridge_reset_data (void)
{
	sgen_bridge_reset_data ();
}

static void G_GNUC_UNUSED
sgen_client_bridge_processing_stw_step (void)
{
	sgen_bridge_processing_stw_step ();
}

static void G_GNUC_UNUSED
sgen_client_bridge_wait_for_processing (void)
{
	mono_gc_wait_for_bridge_processing ();
}

static void G_GNUC_UNUSED
sgen_client_bridge_processing_finish (int generation)
{
	sgen_bridge_processing_finish (generation);
}

static gboolean G_GNUC_UNUSED
sgen_client_bridge_is_bridge_object (GCObject *obj)
{
	return sgen_is_bridge_object (obj);
}

static void G_GNUC_UNUSED
sgen_client_bridge_register_finalized_object (GCObject *object)
{
	sgen_bridge_register_finalized_object (object);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_collection_requested (int generation, size_t requested_size, gboolean force)
{
	MONO_GC_REQUESTED (generation, requested_size, force);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_collection_begin (int minor_gc_count, int generation)
{
	MONO_GC_BEGIN (generation);

	mono_profiler_gc_event (MONO_GC_EVENT_START, generation);

#ifndef DISABLE_PERFCOUNTERS
	if (generation == GENERATION_NURSERY)
		mono_perfcounters->gc_collections0++;
	else
		mono_perfcounters->gc_collections1++;
#endif
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_collection_end (int minor_gc_count, int generation, long long num_objects_scanned, long long num_unique_objects_scanned)
{
	MONO_GC_END (generation);

	mono_profiler_gc_event (MONO_GC_EVENT_END, generation);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_concurrent_start (void)
{
	MONO_GC_CONCURRENT_START_BEGIN (GENERATION_OLD);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_concurrent_update (void)
{
	MONO_GC_CONCURRENT_UPDATE_FINISH_BEGIN (GENERATION_OLD, sgen_get_major_collector ()->get_and_reset_num_major_objects_marked ());
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_concurrent_finish (void)
{
	MONO_GC_CONCURRENT_UPDATE_FINISH_BEGIN (GENERATION_OLD, sgen_get_major_collector ()->get_and_reset_num_major_objects_marked ());
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_sweep_begin (int generation, int full_sweep)
{
	MONO_GC_SWEEP_BEGIN (generation, full_sweep);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_sweep_end (int generation, int full_sweep)
{
	MONO_GC_SWEEP_END (generation, full_sweep);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_world_stopping (int generation, long long timestamp, gpointer thread)
{
	MONO_GC_WORLD_STOP_BEGIN ();

	mono_profiler_gc_event (MONO_GC_EVENT_PRE_STOP_WORLD, generation);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_world_stopped (int generation, long long timestamp, long long total_major_cards, long long marked_major_cards, long long total_los_cards, long long marked_los_cards)
{
	MONO_GC_WORLD_STOP_END ();

	mono_profiler_gc_event (MONO_GC_EVENT_POST_STOP_WORLD, generation);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_world_restarting (int generation, long long timestamp, long long total_major_cards, long long marked_major_cards, long long total_los_cards, long long marked_los_cards)
{
	MONO_GC_WORLD_RESTART_BEGIN (generation);

	mono_profiler_gc_event (MONO_GC_EVENT_PRE_START_WORLD, generation);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_world_restarted (int generation, long long timestamp)
{
	MONO_GC_WORLD_RESTART_END (generation);

	mono_profiler_gc_event (MONO_GC_EVENT_POST_START_WORLD, generation);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_block_alloc (gpointer addr, size_t size)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_block_free (gpointer addr, size_t size)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_block_set_state (gpointer addr, size_t size, int old, int new)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_mark_start (int generation)
{
	mono_profiler_gc_event (MONO_GC_EVENT_MARK_START, generation);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_mark_end (int generation)
{
	mono_profiler_gc_event (MONO_GC_EVENT_MARK_END, generation);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_reclaim_start (int generation)
{
	mono_profiler_gc_event (MONO_GC_EVENT_RECLAIM_START, generation);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_reclaim_end (int generation)
{
	mono_profiler_gc_event (MONO_GC_EVENT_RECLAIM_END, generation);
}

static void
mono_binary_protocol_alloc_generic (gpointer obj, gpointer vtable, size_t size, gboolean pinned)
{
#ifdef ENABLE_DTRACE
	const char *namespace = sgen_client_vtable_get_namespace (vtable);
	const char *name = sgen_client_vtable_get_name (vtable);

	if (sgen_ptr_in_nursery (obj)) {
		if (G_UNLIKELY (MONO_GC_NURSERY_OBJ_ALLOC_ENABLED ()))
			MONO_GC_NURSERY_OBJ_ALLOC ((mword)obj, size, namespace, name);
	} else {
		if (size > SGEN_MAX_SMALL_OBJ_SIZE) {
			if (G_UNLIKELY (MONO_GC_MAJOR_OBJ_ALLOC_LARGE_ENABLED ()))
				MONO_GC_MAJOR_OBJ_ALLOC_LARGE ((mword)obj, size, namespace, name);
		} else if (pinned) {
			MONO_GC_MAJOR_OBJ_ALLOC_PINNED ((mword)obj, size, namespace, name);
		}
	}
#endif
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_alloc (gpointer obj, gpointer vtable, size_t size, gpointer provenance)
{
	mono_binary_protocol_alloc_generic (obj, vtable, size, FALSE);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_alloc_pinned (gpointer obj, gpointer vtable, size_t size, gpointer provenance)
{
	mono_binary_protocol_alloc_generic (obj, vtable, size, TRUE);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_alloc_degraded (gpointer obj, gpointer vtable, size_t size, gpointer provenance)
{
	MONO_GC_MAJOR_OBJ_ALLOC_DEGRADED ((mword)obj, size, sgen_client_vtable_get_namespace (vtable), sgen_client_vtable_get_name (vtable));
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_card_scan (gpointer start, size_t size)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_pin_stage (gpointer addr_ptr, gpointer addr)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_cement_stage (gpointer addr)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_pin (gpointer obj, gpointer vtable, size_t size)
{
#ifdef ENABLE_DTRACE
	if (G_UNLIKELY (MONO_GC_OBJ_PINNED_ENABLED ())) {
		int gen = sgen_ptr_in_nursery (obj) ? GENERATION_NURSERY : GENERATION_OLD;
		MONO_GC_OBJ_PINNED ((mword)obj,
				sgen_safe_object_get_size (obj),
				sgen_client_vtable_get_namespace (vtable), sgen_client_vtable_get_name (vtable), gen);
	}
#endif
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_mark (gpointer obj, gpointer vtable, size_t size)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_scan_begin (gpointer obj, gpointer vtable, size_t size)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_scan_vtype_begin (gpointer obj, size_t size)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_scan_process_reference (gpointer obj, gpointer ptr, gpointer value)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_scan_stack (gpointer thread, gpointer stack_start, gpointer stack_end, int skip_reason)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_wbarrier (gpointer ptr, gpointer value, gpointer value_vtable)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_cement (gpointer ptr, gpointer vtable, size_t size)
{
#ifdef ENABLE_DTRACE
	if (G_UNLIKELY (MONO_GC_OBJ_CEMENTED_ENABLED())) {
		MONO_GC_OBJ_CEMENTED ((mword)ptr, sgen_safe_object_get_size ((GCObject*)ptr),
				sgen_client_vtable_get_namespace (vtable), sgen_client_vtable_get_name (vtable));
	}
#endif
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_copy (gpointer from, gpointer to, gpointer vtable, size_t size)
{
#ifdef ENABLE_DTRACE
	if (G_UNLIKELY (MONO_GC_OBJ_MOVED_ENABLED ())) {
		int dest_gen = sgen_ptr_in_nursery (to) ? GENERATION_NURSERY : GENERATION_OLD;
		int src_gen = sgen_ptr_in_nursery (from) ? GENERATION_NURSERY : GENERATION_OLD;
		MONO_GC_OBJ_MOVED ((mword)to, (mword)from, dest_gen, src_gen, size, sgen_client_vtable_get_namespace (vtable), sgen_client_vtable_get_name (vtable));
	}
#endif
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_global_remset (gpointer ptr, gpointer value, gpointer value_vtable)
{
#ifdef ENABLE_DTRACE
	if (G_UNLIKELY (MONO_GC_GLOBAL_REMSET_ADD_ENABLED ())) {
		MONO_GC_GLOBAL_REMSET_ADD ((mword)ptr, (mword)value, sgen_safe_object_get_size (value),
				sgen_client_vtable_get_namespace (value_vtable), sgen_client_vtable_get_name (value_vtable));
	}
#endif
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_ptr_update (gpointer ptr, gpointer old_value, gpointer new_value, gpointer vtable, size_t size)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_cleanup (gpointer ptr, gpointer vtable, size_t size)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_dislink_update (gpointer link, gpointer obj, gboolean track)
{
#ifdef ENABLE_DTRACE
	if (MONO_GC_WEAK_UPDATE_ENABLED ()) {
		GCVTable vt = obj ? SGEN_LOAD_VTABLE (obj) : NULL;
		MONO_GC_WEAK_UPDATE ((mword)link,
				(mword)obj,
				obj ? (mword)sgen_safe_object_get_size (obj) : (mword)0,
				obj ? sgen_client_vtable_get_namespace (vt) : NULL,
				obj ? sgen_client_vtable_get_name (vt) : NULL,
				track ? 1 : 0);
	}
#endif
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_dislink_update_staged (gpointer link, gpointer obj, gboolean track, int index)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_dislink_process_staged (gpointer link, gpointer obj, int index)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_empty (gpointer start, size_t size)
{
	if (sgen_ptr_in_nursery (start))
		MONO_GC_NURSERY_SWEPT ((mword)start, size);
	else
		MONO_GC_MAJOR_SWEPT ((mword)start, size);
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_thread_suspend (gpointer thread, gpointer stopped_ip)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_thread_restart (gpointer thread)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_thread_register (gpointer thread)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_thread_unregister (gpointer thread)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_missing_remset (gpointer obj, gpointer obj_vtable, int offset, gpointer value, gpointer value_vtable, gboolean value_pinned)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_cement_reset (void)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_domain_unload_begin (gpointer domain)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_domain_unload_end (gpointer domain)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_gray_enqueue (gpointer queue, gpointer cursor, gpointer value)
{
}

static void G_GNUC_UNUSED
sgen_client_binary_protocol_gray_dequeue (gpointer queue, gpointer cursor, gpointer value)
{
}

int sgen_thread_handshake (BOOL suspend);
gboolean sgen_suspend_thread (SgenThreadInfo *info);
gboolean sgen_resume_thread (SgenThreadInfo *info);
void sgen_wait_for_suspend_ack (int count);

#ifdef HAVE_KW_THREAD
extern __thread SgenThreadInfo *sgen_thread_info;
#define TLAB_ACCESS_INIT
#define IN_CRITICAL_REGION sgen_thread_info->client_info.in_critical_region
#else
extern MonoNativeTlsKey thread_info_key;
#define TLAB_ACCESS_INIT	SgenThreadInfo *__thread_info__ = mono_native_tls_get_value (thread_info_key)
#define IN_CRITICAL_REGION (__thread_info__->client_info.in_critical_region)
#endif

#ifndef DISABLE_CRITICAL_REGION

#ifdef HAVE_KW_THREAD
#define IN_CRITICAL_REGION sgen_thread_info->client_info.in_critical_region
#else
#define IN_CRITICAL_REGION (__thread_info__->client_info.in_critical_region)
#endif

/* Enter must be visible before anything is done in the critical region. */
#define ENTER_CRITICAL_REGION do { mono_atomic_store_acquire (&IN_CRITICAL_REGION, 1); } while (0)

/* Exit must make sure all critical regions stores are visible before it signal the end of the region. 
 * We don't need to emit a full barrier since we
 */
#define EXIT_CRITICAL_REGION  do { mono_atomic_store_release (&IN_CRITICAL_REGION, 0); } while (0)

#endif

#define SGEN_TV_DECLARE(name) gint64 name
#define SGEN_TV_GETTIME(tv) tv = mono_100ns_ticks ()
#define SGEN_TV_ELAPSED(start,end) ((long)(end-start))

typedef MonoSemType SgenSemaphore;

#define SGEN_SEMAPHORE_INIT(sem,initial)	MONO_SEM_INIT ((sem), (initial))
#define SGEN_SEMAPHORE_POST(sem)		MONO_SEM_POST ((sem))
#define SGEN_SEMAPHORE_WAIT(sem)		MONO_SEM_WAIT ((sem))

gboolean sgen_has_critical_method (void);
gboolean sgen_is_critical_method (MonoMethod *method);

void sgen_set_use_managed_allocator (gboolean flag);
gboolean sgen_is_managed_allocator (MonoMethod *method);
gboolean sgen_has_managed_allocator (void);

void sgen_scan_for_registered_roots_in_domain (MonoDomain *domain, int root_type);
void sgen_null_links_for_domain (MonoDomain *domain);

#endif
