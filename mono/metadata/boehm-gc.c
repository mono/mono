/*
 * boehm-gc.c: GC implementation using either the installed or included Boehm GC.
 *
 * Copyright 2001-2003 Ximian, Inc (http://www.ximian.com)
 * Copyright 2004-2011 Novell, Inc (http://www.novell.com)
 * Copyright 2011-2012 Xamarin, Inc (http://www.xamarin.com)
 */

#include "config.h"

#include <string.h>

#define GC_I_HIDE_POINTERS
#include <mono/metadata/gc-internal.h>
#include <mono/metadata/mono-gc.h>
#include <mono/metadata/profiler-private.h>
#include <mono/metadata/class-internals.h>
#include <mono/metadata/method-builder.h>
#include <mono/metadata/opcodes.h>
#include <mono/metadata/domain-internals.h>
#include <mono/metadata/metadata-internals.h>
#include <mono/metadata/marshal.h>
#include <mono/metadata/runtime.h>
#include <mono/metadata/sgen-toggleref.h>
#include <mono/utils/atomic.h>
#include <mono/utils/mono-logger-internal.h>
#include <mono/utils/mono-memory-model.h>
#include <mono/utils/mono-time.h>
#include <mono/utils/mono-threads.h>
#include <mono/utils/dtrace.h>
#include <mono/utils/gc_wrapper.h>
#include <mono/utils/mono-mutex.h>
#include <mono/utils/mono-counters.h>

#if HAVE_BOEHM_GC

#undef TRUE
#undef FALSE
#define THREAD_LOCAL_ALLOC 1
#include "private/pthread_support.h"

#if defined(PLATFORM_MACOSX) && defined(HAVE_PTHREAD_GET_STACKADDR_NP)
void *pthread_get_stackaddr_np(pthread_t);
#endif

#define GC_NO_DESCRIPTOR ((gpointer)(0 | GC_DS_LENGTH))
/*Boehm max heap cannot be smaller than 16MB*/
#define MIN_BOEHM_MAX_HEAP_SIZE_IN_MB 16
#define MIN_BOEHM_MAX_HEAP_SIZE (MIN_BOEHM_MAX_HEAP_SIZE_IN_MB << 20)

static gboolean gc_initialized = FALSE;
static mono_mutex_t mono_gc_lock;

static void*
boehm_thread_register (MonoThreadInfo* info, void *baseptr);
static void
boehm_thread_unregister (MonoThreadInfo *p);
static void
register_test_toggleref_callback (void);

#define BOEHM_GC_BIT_FINALIZER_AWARE 1
static MonoGCFinalizerCallbacks fin_callbacks;

static void
mono_gc_warning (char *msg, GC_word arg)
{
	mono_trace (G_LOG_LEVEL_WARNING, MONO_TRACE_GC, msg, (unsigned long)arg);
}

void
mono_gc_base_init (void)
{
	MonoThreadInfoCallbacks cb;
	const char *env;
	int dummy;

	if (gc_initialized)
		return;

	mono_counters_init ();

	/*
	 * Handle the case when we are called from a thread different from the main thread,
	 * confusing libgc.
	 * FIXME: Move this to libgc where it belongs.
	 *
	 * we used to do this only when running on valgrind,
	 * but it happens also in other setups.
	 */
#if defined(HAVE_PTHREAD_GETATTR_NP) && defined(HAVE_PTHREAD_ATTR_GETSTACK) && !defined(__native_client__)
	{
		size_t size;
		void *sstart;
		pthread_attr_t attr;
		pthread_getattr_np (pthread_self (), &attr);
		pthread_attr_getstack (&attr, &sstart, &size);
		pthread_attr_destroy (&attr); 
		/*g_print ("stackbottom pth is: %p\n", (char*)sstart + size);*/
#ifdef __ia64__
		/*
		 * The calculation above doesn't seem to work on ia64, also we need to set
		 * GC_register_stackbottom as well, but don't know how.
		 */
#else
		/* apparently with some linuxthreads implementations sstart can be NULL,
		 * fallback to the more imprecise method (bug# 78096).
		 */
		if (sstart) {
			GC_stackbottom = (char*)sstart + size;
		} else {
			int dummy;
			gsize stack_bottom = (gsize)&dummy;
			stack_bottom += 4095;
			stack_bottom &= ~4095;
			GC_stackbottom = (char*)stack_bottom;
		}
#endif
	}
#elif defined(HAVE_PTHREAD_GET_STACKSIZE_NP) && defined(HAVE_PTHREAD_GET_STACKADDR_NP)
		GC_stackbottom = (char*)pthread_get_stackaddr_np (pthread_self ());
#elif defined(__OpenBSD__)
#  include <pthread_np.h>
	{
		stack_t ss;
		int rslt;

		rslt = pthread_stackseg_np(pthread_self(), &ss);
		g_assert (rslt == 0);

		GC_stackbottom = (char*)ss.ss_sp;
	}
#elif defined(__native_client__)
	/* Do nothing, GC_stackbottom is set correctly in libgc */
#else
	{
		int dummy;
		gsize stack_bottom = (gsize)&dummy;
		stack_bottom += 4095;
		stack_bottom &= ~4095;
		/*g_print ("stackbottom is: %p\n", (char*)stack_bottom);*/
		GC_stackbottom = (char*)stack_bottom;
	}
#endif

#if !defined(PLATFORM_ANDROID)
	/* If GC_no_dls is set to true, GC_find_limit is not called. This causes a seg fault on Android. */
	GC_no_dls = TRUE;
#endif
	{
		if ((env = g_getenv ("MONO_GC_DEBUG"))) {
			char **opts = g_strsplit (env, ",", -1);
			for (char **ptr = opts; ptr && *ptr; ptr ++) {
				char *opt = *ptr;
				if (!strcmp (opt, "do-not-finalize")) {
					do_not_finalize = 1;
				} else if (!strcmp (opt, "log-finalizers")) {
					log_finalizers = 1;
				}
			}
		}
	}

	GC_init ();

	GC_oom_fn = mono_gc_out_of_memory;
	GC_set_warn_proc (mono_gc_warning);
	GC_finalize_on_demand = 1;
	GC_finalizer_notifier = mono_gc_finalize_notify;

	GC_init_gcj_malloc (5, NULL);

	if ((env = g_getenv ("MONO_GC_PARAMS"))) {
		char **ptr, **opts = g_strsplit (env, ",", -1);
		for (ptr = opts; *ptr; ++ptr) {
			char *opt = *ptr;
			if (g_str_has_prefix (opt, "max-heap-size=")) {
				size_t max_heap;

				opt = strchr (opt, '=') + 1;
				if (*opt && mono_gc_parse_environment_string_extract_number (opt, &max_heap)) {
					if (max_heap < MIN_BOEHM_MAX_HEAP_SIZE) {
						fprintf (stderr, "max-heap-size must be at least %dMb.\n", MIN_BOEHM_MAX_HEAP_SIZE_IN_MB);
						exit (1);
					}
					GC_set_max_heap_size (max_heap);
				} else {
					fprintf (stderr, "max-heap-size must be an integer.\n");
					exit (1);
				}
				continue;
			} else if (g_str_has_prefix (opt, "toggleref-test")) {
				register_test_toggleref_callback ();
				continue;
			} else {
				/* Could be a parameter for sgen */
				/*
				fprintf (stderr, "MONO_GC_PARAMS must be a comma-delimited list of one or more of the following:\n");
				fprintf (stderr, "  max-heap-size=N (where N is an integer, possibly with a k, m or a g suffix)\n");
				exit (1);
				*/
			}
		}
		g_strfreev (opts);
	}

	memset (&cb, 0, sizeof (cb));
	cb.thread_register = boehm_thread_register;
	cb.thread_unregister = boehm_thread_unregister;
	cb.mono_method_is_critical = (gpointer)mono_runtime_is_critical_method;

	mono_threads_init (&cb, sizeof (MonoThreadInfo));
	mono_mutex_init (&mono_gc_lock);

	mono_thread_info_attach (&dummy);

	mono_gc_enable_events ();
	gc_initialized = TRUE;
}

void
mono_gc_base_cleanup (void)
{
	GC_finalizer_notifier = NULL;
}

/**
 * mono_gc_collect:
 * @generation: GC generation identifier
 *
 * Perform a garbage collection for the given generation, higher numbers
 * mean usually older objects. Collecting a high-numbered generation
 * implies collecting also the lower-numbered generations.
 * The maximum value for @generation can be retrieved with a call to
 * mono_gc_max_generation(), so this function is usually called as:
 *
 * 	mono_gc_collect (mono_gc_max_generation ());
 */
void
mono_gc_collect (int generation)
{
#ifndef DISABLE_PERFCOUNTERS
	mono_perfcounters->gc_induced++;
#endif
	GC_gcollect ();
}

/**
 * mono_gc_max_generation:
 *
 * Get the maximum generation number used by the current garbage
 * collector. The value will be 0 for the Boehm collector, 1 or more
 * for the generational collectors.
 *
 * Returns: the maximum generation number.
 */
int
mono_gc_max_generation (void)
{
	return 0;
}

/**
 * mono_gc_get_generation:
 * @object: a managed object
 *
 * Get the garbage collector's generation that @object belongs to.
 * Use this has a hint only.
 *
 * Returns: a garbage collector generation number
 */
int
mono_gc_get_generation  (MonoObject *object)
{
	return 0;
}

/**
 * mono_gc_collection_count:
 * @generation: a GC generation number
 *
 * Get how many times a garbage collection has been performed
 * for the given @generation number.
 *
 * Returns: the number of garbage collections
 */
int
mono_gc_collection_count (int generation)
{
	return GC_gc_no;
}

/**
 * mono_gc_add_memory_pressure:
 * @value: amount of bytes
 *
 * Adjust the garbage collector's view of how many bytes of memory
 * are indirectly referenced by managed objects (for example unmanaged
 * memory holding image or other binary data).
 * This is a hint only to the garbage collector algorithm.
 * Note that negative amounts of @value will decrease the memory
 * pressure.
 */
void
mono_gc_add_memory_pressure (gint64 value)
{
}

/**
 * mono_gc_get_used_size:
 *
 * Get the approximate amount of memory used by managed objects.
 *
 * Returns: the amount of memory used in bytes
 */
int64_t
mono_gc_get_used_size (void)
{
	return GC_get_heap_size () - GC_get_free_bytes ();
}

/**
 * mono_gc_get_heap_size:
 *
 * Get the amount of memory used by the garbage collector.
 *
 * Returns: the size of the heap in bytes
 */
int64_t
mono_gc_get_heap_size (void)
{
	return GC_get_heap_size ();
}

gboolean
mono_gc_is_gc_thread (void)
{
	return GC_thread_is_registered ();
}

extern int GC_thread_register_foreign (void *base_addr);

gboolean
mono_gc_register_thread (void *baseptr)
{
	return mono_thread_info_attach (baseptr) != NULL;
}

static void*
boehm_thread_register (MonoThreadInfo* info, void *baseptr)
{
	if (mono_gc_is_gc_thread())
		return info;
#if !defined(HOST_WIN32)
	return GC_thread_register_foreign (baseptr) ? info : NULL;
#else
	return NULL;
#endif
}

static void
boehm_thread_unregister (MonoThreadInfo *p)
{
	MonoNativeThreadId tid;

	tid = mono_thread_info_get_tid (p);

	if (p->runtime_thread)
		mono_threads_add_joinable_thread ((gpointer)tid);
}

gboolean
mono_object_is_alive (MonoObject* o)
{
	return GC_is_marked ((gpointer)o);
}

int
mono_gc_walk_heap (int flags, MonoGCReferences callback, void *data)
{
	return 1;
}

static gint64 gc_start_time;

static void
on_gc_notification (GCEventType event)
{
	MonoGCEvent e = (MonoGCEvent)event;

	switch (e) {
	case MONO_GC_EVENT_PRE_STOP_WORLD:
		MONO_GC_WORLD_STOP_BEGIN ();
		mono_thread_info_suspend_lock ();
		break;

	case MONO_GC_EVENT_POST_STOP_WORLD:
		MONO_GC_WORLD_STOP_END ();
		break;

	case MONO_GC_EVENT_PRE_START_WORLD:
		MONO_GC_WORLD_RESTART_BEGIN (1);
		break;

	case MONO_GC_EVENT_POST_START_WORLD:
		MONO_GC_WORLD_RESTART_END (1);
		mono_thread_info_suspend_unlock ();
		break;

	case MONO_GC_EVENT_START:
		MONO_GC_BEGIN (1);
#ifndef DISABLE_PERFCOUNTERS
		if (mono_perfcounters)
			mono_perfcounters->gc_collections0++;
#endif
		gc_stats.major_gc_count ++;
		gc_start_time = mono_100ns_ticks ();
		break;

	case MONO_GC_EVENT_END:
		MONO_GC_END (1);
#if defined(ENABLE_DTRACE) && defined(__sun__)
		/* This works around a dtrace -G problem on Solaris.
		   Limit its actual use to when the probe is enabled. */
		if (MONO_GC_END_ENABLED ())
			sleep(0);
#endif

#ifndef DISABLE_PERFCOUNTERS
		if (mono_perfcounters) {
			guint64 heap_size = GC_get_heap_size ();
			guint64 used_size = heap_size - GC_get_free_bytes ();
			mono_perfcounters->gc_total_bytes = used_size;
			mono_perfcounters->gc_committed_bytes = heap_size;
			mono_perfcounters->gc_reserved_bytes = heap_size;
			mono_perfcounters->gc_gen0size = heap_size;
		}
#endif
		gc_stats.major_gc_time += mono_100ns_ticks () - gc_start_time;
		mono_trace_message (MONO_TRACE_GC, "gc took %d usecs", (mono_100ns_ticks () - gc_start_time) / 10);
		break;
	default:
		break;
	}

	mono_profiler_gc_event (e, 0);
}
 
static void
on_gc_heap_resize (size_t new_size)
{
	guint64 heap_size = GC_get_heap_size ();
#ifndef DISABLE_PERFCOUNTERS
	if (mono_perfcounters) {
		mono_perfcounters->gc_committed_bytes = heap_size;
		mono_perfcounters->gc_reserved_bytes = heap_size;
		mono_perfcounters->gc_gen0size = heap_size;
	}
#endif
	mono_profiler_gc_heap_resize (new_size);
}

void
mono_gc_enable_events (void)
{
	GC_notify_event = on_gc_notification;
	GC_on_heap_resize = on_gc_heap_resize;
}

static gboolean alloc_events = FALSE;

void
mono_gc_enable_alloc_events (void)
{
	alloc_events = TRUE;
}

int
mono_gc_register_root (char *start, size_t size, void *descr)
{
	/* for some strange reason, they want one extra byte on the end */
	GC_add_roots (start, start + size + 1);

	return TRUE;
}

void
mono_gc_deregister_root (char* addr)
{
#ifndef HOST_WIN32
	/* FIXME: libgc doesn't define this work win32 for some reason */
	/* FIXME: No size info */
	GC_remove_roots (addr, addr + sizeof (gpointer) + 1);
#endif
}

void
mono_gc_weak_link_add (void **link_addr, MonoObject *obj, gboolean track)
{
	/* libgc requires that we use HIDE_POINTER... */
	*link_addr = (void*)HIDE_POINTER (obj);
	if (track)
		GC_REGISTER_LONG_LINK (link_addr, obj);
	else
		GC_GENERAL_REGISTER_DISAPPEARING_LINK (link_addr, obj);
}

void
mono_gc_weak_link_remove (void **link_addr, gboolean track)
{
	if (track)
		GC_unregister_long_link (link_addr);
	else
		GC_unregister_disappearing_link (link_addr);
	*link_addr = NULL;
}

static gpointer
reveal_link (gpointer link_addr)
{
	void **link_a = link_addr;
	return REVEAL_POINTER (*link_a);
}

MonoObject*
mono_gc_weak_link_get (void **link_addr)
{
	MonoObject *obj = GC_call_with_alloc_lock (reveal_link, link_addr);
	if (obj == (MonoObject *) -1)
		return NULL;
	return obj;
}

void*
mono_gc_make_descr_for_string (gsize *bitmap, int numbits)
{
	return mono_gc_make_descr_from_bitmap (bitmap, numbits);
}

void*
mono_gc_make_descr_for_object (gsize *bitmap, int numbits, size_t obj_size)
{
	return mono_gc_make_descr_from_bitmap (bitmap, numbits);
}

void*
mono_gc_make_descr_for_array (int vector, gsize *elem_bitmap, int numbits, size_t elem_size)
{
	/* libgc has no usable support for arrays... */
	return GC_NO_DESCRIPTOR;
}

void*
mono_gc_make_descr_from_bitmap (gsize *bitmap, int numbits)
{
	/* It seems there are issues when the bitmap doesn't fit: play it safe */
	if (numbits >= 30)
		return GC_NO_DESCRIPTOR;
	else
		return (gpointer)GC_make_descriptor ((GC_bitmap)bitmap, numbits);
}

void*
mono_gc_make_root_descr_all_refs (int numbits)
{
	return NULL;
}

void*
mono_gc_alloc_fixed (size_t size, void *descr)
{
	/* To help track down typed allocation bugs */
	/*
	static int count;
	count ++;
	if (count == atoi (g_getenv ("COUNT2")))
		printf ("HIT!\n");
	if (count > atoi (g_getenv ("COUNT2")))
		return GC_MALLOC (size);
	*/

	if (descr)
		return GC_MALLOC_EXPLICITLY_TYPED (size, (GC_descr)descr);
	else
		return GC_MALLOC (size);
}

void
mono_gc_free_fixed (void* addr)
{
}

void *
mono_gc_alloc_obj (MonoVTable *vtable, size_t size)
{
	MonoObject *obj;

	if (!vtable->klass->has_references) {
		obj = GC_MALLOC_ATOMIC (size);

		obj->vtable = vtable;
		obj->synchronisation = NULL;

		memset ((char *) obj + sizeof (MonoObject), 0, size - sizeof (MonoObject));
	} else if (vtable->gc_descr != GC_NO_DESCRIPTOR) {
		obj = GC_GCJ_MALLOC (size, vtable);
	} else {
		obj = GC_MALLOC (size);

		obj->vtable = vtable;
	}

	if (G_UNLIKELY (alloc_events))
		mono_profiler_allocation (obj);

	return obj;
}

void *
mono_gc_alloc_vector (MonoVTable *vtable, size_t size, uintptr_t max_length)
{
	MonoArray *obj;

	if (!vtable->klass->has_references) {
		obj = GC_MALLOC_ATOMIC (size);

		obj->obj.vtable = vtable;
		obj->obj.synchronisation = NULL;

		memset ((char *) obj + sizeof (MonoObject), 0, size - sizeof (MonoObject));
	} else if (vtable->gc_descr != GC_NO_DESCRIPTOR) {
		obj = GC_GCJ_MALLOC (size, vtable);
	} else {
		obj = GC_MALLOC (size);

		obj->obj.vtable = vtable;
	}

	obj->max_length = max_length;

	if (G_UNLIKELY (alloc_events))
		mono_profiler_allocation (&obj->obj);

	return obj;
}

void *
mono_gc_alloc_array (MonoVTable *vtable, size_t size, uintptr_t max_length, uintptr_t bounds_size)
{
	MonoArray *obj;

	if (!vtable->klass->has_references) {
		obj = GC_MALLOC_ATOMIC (size);

		obj->obj.vtable = vtable;
		obj->obj.synchronisation = NULL;

		memset ((char *) obj + sizeof (MonoObject), 0, size - sizeof (MonoObject));
	} else if (vtable->gc_descr != GC_NO_DESCRIPTOR) {
		obj = GC_GCJ_MALLOC (size, vtable);
	} else {
		obj = GC_MALLOC (size);

		obj->obj.vtable = vtable;
	}

	obj->max_length = max_length;

	if (bounds_size)
		obj->bounds = (MonoArrayBounds *) ((char *) obj + size - bounds_size);

	if (G_UNLIKELY (alloc_events))
		mono_profiler_allocation (&obj->obj);

	return obj;
}

void *
mono_gc_alloc_string (MonoVTable *vtable, size_t size, gint32 len)
{
	MonoString *obj = GC_MALLOC_ATOMIC (size);

	obj->object.vtable = vtable;
	obj->object.synchronisation = NULL;
	obj->length = len;
	obj->chars [len] = 0;

	if (G_UNLIKELY (alloc_events))
		mono_profiler_allocation (&obj->object);

	return obj;
}

int
mono_gc_invoke_finalizers (void)
{
	/* There is a bug in GC_invoke_finalizer () in versions <= 6.2alpha4:
	 * the 'mem_freed' variable is not initialized when there are no
	 * objects to finalize, which leads to strange behavior later on.
	 * The check is necessary to work around that bug.
	 */
	if (GC_should_invoke_finalizers ())
		return GC_invoke_finalizers ();
	return 0;
}

gboolean
mono_gc_pending_finalizers (void)
{
	return GC_should_invoke_finalizers ();
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

void
mono_gc_clear_domain (MonoDomain *domain)
{
}

int
mono_gc_get_suspend_signal (void)
{
	return GC_get_suspend_signal ();
}

int
mono_gc_get_restart_signal (void)
{
	return GC_get_restart_signal ();
}

#if defined(USE_COMPILER_TLS) && defined(__linux__) && (defined(__i386__) || defined(__x86_64__))
extern __thread MONO_TLS_FAST void* GC_thread_tls;
#include "metadata-internals.h"

static int
shift_amount (int v)
{
	int i = 0;
	while (!(v & (1 << i)))
		i++;
	return i;
}

enum {
	ATYPE_FREEPTR,
	ATYPE_FREEPTR_FOR_BOX,
	ATYPE_NORMAL,
	ATYPE_GCJ,
	ATYPE_STRING,
	ATYPE_NUM
};

static MonoMethod*
create_allocator (int atype, int tls_key, gboolean slowpath)
{
	int index_var, bytes_var, my_fl_var, my_entry_var;
	guint32 no_freelist_branch, not_small_enough_branch = 0;
	guint32 size_overflow_branch = 0;
	MonoMethodBuilder *mb;
	MonoMethod *res;
	MonoMethodSignature *csig;
	const char *name = NULL;
	AllocatorWrapperInfo *info;

	if (atype == ATYPE_FREEPTR) {
		name = slowpath ? "SlowAllocPtrfree" : "AllocPtrfree";
	} else if (atype == ATYPE_FREEPTR_FOR_BOX) {
		name = slowpath ? "SlowAllocPtrfreeBox" : "AllocPtrfreeBox";
	} else if (atype == ATYPE_NORMAL) {
		name = slowpath ? "SlowAlloc" : "Alloc";
	} else if (atype == ATYPE_GCJ) {
		name = slowpath ? "SlowAllocGcj" : "AllocGcj";
	} else if (atype == ATYPE_STRING) {
		name = slowpath ? "SlowAllocString" : "AllocString";
	} else {
		g_assert_not_reached ();
	}

	csig = mono_metadata_signature_alloc (mono_defaults.corlib, 2);

	if (atype == ATYPE_STRING) {
		csig->ret = &mono_defaults.string_class->byval_arg;
		csig->params [0] = &mono_defaults.int_class->byval_arg;
		csig->params [1] = &mono_defaults.int32_class->byval_arg;
	} else {
		csig->ret = &mono_defaults.object_class->byval_arg;
		csig->params [0] = &mono_defaults.int_class->byval_arg;
		csig->params [1] = &mono_defaults.int32_class->byval_arg;
	}

	mb = mono_mb_new (mono_defaults.object_class, name, MONO_WRAPPER_ALLOC);

	if (slowpath)
		goto always_slowpath;

	bytes_var = mono_mb_add_local (mb, &mono_defaults.int32_class->byval_arg);
	if (atype == ATYPE_STRING) {
		/* a string alloator method takes the args: (vtable, len) */
		/* bytes = (offsetof (MonoString, chars) + ((len + 1) * 2)); */
		mono_mb_emit_ldarg (mb, 1);
		mono_mb_emit_icon (mb, 1);
		mono_mb_emit_byte (mb, MONO_CEE_ADD);
		mono_mb_emit_icon (mb, 1);
		mono_mb_emit_byte (mb, MONO_CEE_SHL);
		// sizeof (MonoString) might include padding
		mono_mb_emit_icon (mb, G_STRUCT_OFFSET (MonoString, chars));
		mono_mb_emit_byte (mb, MONO_CEE_ADD);
		mono_mb_emit_stloc (mb, bytes_var);
	} else {
		mono_mb_emit_ldarg (mb, 1);
		mono_mb_emit_stloc (mb, bytes_var);
	}

	/* this is needed for strings/arrays only as the other big types are never allocated with this method */
	if (atype == ATYPE_STRING) {
		/* check for size */
		/* if (!SMALL_ENOUGH (bytes)) jump slow_path;*/
		mono_mb_emit_ldloc (mb, bytes_var);
		mono_mb_emit_icon (mb, (NFREELISTS-1) * GRANULARITY);
		not_small_enough_branch = mono_mb_emit_short_branch (mb, MONO_CEE_BGT_UN_S);
		/* check for overflow */
		mono_mb_emit_ldloc (mb, bytes_var);
		mono_mb_emit_icon (mb, sizeof (MonoString));
		size_overflow_branch = mono_mb_emit_short_branch (mb, MONO_CEE_BLE_UN_S);
	}

	/* int index = INDEX_FROM_BYTES(bytes); */
	index_var = mono_mb_add_local (mb, &mono_defaults.int32_class->byval_arg);
	
	mono_mb_emit_ldloc (mb, bytes_var);
	mono_mb_emit_icon (mb, GRANULARITY - 1);
	mono_mb_emit_byte (mb, MONO_CEE_ADD);
	mono_mb_emit_icon (mb, shift_amount (GRANULARITY));
	mono_mb_emit_byte (mb, MONO_CEE_SHR_UN);
	mono_mb_emit_icon (mb, shift_amount (sizeof (gpointer)));
	mono_mb_emit_byte (mb, MONO_CEE_SHL);
	/* index var is already adjusted into bytes */
	mono_mb_emit_stloc (mb, index_var);

	my_fl_var = mono_mb_add_local (mb, &mono_defaults.int_class->byval_arg);
	my_entry_var = mono_mb_add_local (mb, &mono_defaults.int_class->byval_arg);
	/* my_fl = ((GC_thread)tsd) -> ptrfree_freelists + index; */
	mono_mb_emit_byte (mb, MONO_CUSTOM_PREFIX);
	mono_mb_emit_byte (mb, 0x0D); /* CEE_MONO_TLS */
	mono_mb_emit_i4 (mb, tls_key);
	if (atype == ATYPE_FREEPTR || atype == ATYPE_FREEPTR_FOR_BOX || atype == ATYPE_STRING)
		mono_mb_emit_icon (mb, G_STRUCT_OFFSET (struct GC_Thread_Rep, ptrfree_freelists));
	else if (atype == ATYPE_NORMAL)
		mono_mb_emit_icon (mb, G_STRUCT_OFFSET (struct GC_Thread_Rep, normal_freelists));
	else if (atype == ATYPE_GCJ)
		mono_mb_emit_icon (mb, G_STRUCT_OFFSET (struct GC_Thread_Rep, gcj_freelists));
	else
		g_assert_not_reached ();
	mono_mb_emit_byte (mb, MONO_CEE_ADD);
	mono_mb_emit_ldloc (mb, index_var);
	mono_mb_emit_byte (mb, MONO_CEE_ADD);
	mono_mb_emit_stloc (mb, my_fl_var);

	/* my_entry = *my_fl; */
	mono_mb_emit_ldloc (mb, my_fl_var);
	mono_mb_emit_byte (mb, MONO_CEE_LDIND_I);
	mono_mb_emit_stloc (mb, my_entry_var);

	/* if (EXPECT((word)my_entry >= HBLKSIZE, 1)) { */
	mono_mb_emit_ldloc (mb, my_entry_var);
	mono_mb_emit_icon (mb, HBLKSIZE);
	no_freelist_branch = mono_mb_emit_short_branch (mb, MONO_CEE_BLT_UN_S);

	/* ptr_t next = obj_link(my_entry); *my_fl = next; */
	mono_mb_emit_ldloc (mb, my_fl_var);
	mono_mb_emit_ldloc (mb, my_entry_var);
	mono_mb_emit_byte (mb, MONO_CEE_LDIND_I);
	mono_mb_emit_byte (mb, MONO_CEE_STIND_I);

	/* set the vtable and clear the words in the object */
	mono_mb_emit_ldloc (mb, my_entry_var);
	mono_mb_emit_ldarg (mb, 0);
	mono_mb_emit_byte (mb, MONO_CEE_STIND_I);

	if (atype == ATYPE_FREEPTR) {
		int start_var, end_var, start_loop;
		/* end = my_entry + bytes; start = my_entry + sizeof (gpointer);
		 */
		start_var = mono_mb_add_local (mb, &mono_defaults.int_class->byval_arg);
		end_var = mono_mb_add_local (mb, &mono_defaults.int_class->byval_arg);
		mono_mb_emit_ldloc (mb, my_entry_var);
		mono_mb_emit_ldloc (mb, bytes_var);
		mono_mb_emit_byte (mb, MONO_CEE_ADD);
		mono_mb_emit_stloc (mb, end_var);
		mono_mb_emit_ldloc (mb, my_entry_var);
		mono_mb_emit_icon (mb, G_STRUCT_OFFSET (MonoObject, synchronisation));
		mono_mb_emit_byte (mb, MONO_CEE_ADD);
		mono_mb_emit_stloc (mb, start_var);
		/*
		 * do {
		 * 	*start++ = NULL;
		 * } while (start < end);
		 */
		start_loop = mono_mb_get_label (mb);
		mono_mb_emit_ldloc (mb, start_var);
		mono_mb_emit_icon (mb, 0);
		mono_mb_emit_byte (mb, MONO_CEE_STIND_I);
		mono_mb_emit_ldloc (mb, start_var);
		mono_mb_emit_icon (mb, sizeof (gpointer));
		mono_mb_emit_byte (mb, MONO_CEE_ADD);
		mono_mb_emit_stloc (mb, start_var);

		mono_mb_emit_ldloc (mb, start_var);
		mono_mb_emit_ldloc (mb, end_var);
		mono_mb_emit_byte (mb, MONO_CEE_BLT_UN_S);
		mono_mb_emit_byte (mb, start_loop - (mono_mb_get_label (mb) + 1));
	} else if (atype == ATYPE_FREEPTR_FOR_BOX || atype == ATYPE_STRING) {
		/* need to clear just the sync pointer */
		mono_mb_emit_ldloc (mb, my_entry_var);
		mono_mb_emit_icon (mb, G_STRUCT_OFFSET (MonoObject, synchronisation));
		mono_mb_emit_byte (mb, MONO_CEE_ADD);
		mono_mb_emit_icon (mb, 0);
		mono_mb_emit_byte (mb, MONO_CEE_STIND_I);
	}

	if (atype == ATYPE_STRING) {
		/* need to set length and clear the last char */
		/* s->length = len; */
		mono_mb_emit_ldloc (mb, my_entry_var);
		mono_mb_emit_icon (mb, G_STRUCT_OFFSET (MonoString, length));
		mono_mb_emit_byte (mb, MONO_CEE_ADD);
		mono_mb_emit_ldarg (mb, 1);
		mono_mb_emit_byte (mb, MONO_CEE_STIND_I4);
		/* s->chars [len] = 0; */
		mono_mb_emit_ldloc (mb, my_entry_var);
		mono_mb_emit_ldloc (mb, bytes_var);
		mono_mb_emit_icon (mb, 2);
		mono_mb_emit_byte (mb, MONO_CEE_SUB);
		mono_mb_emit_byte (mb, MONO_CEE_ADD);
		mono_mb_emit_icon (mb, 0);
		mono_mb_emit_byte (mb, MONO_CEE_STIND_I2);
	}

	/* return my_entry; */
	mono_mb_emit_ldloc (mb, my_entry_var);
	mono_mb_emit_byte (mb, MONO_CEE_RET);
	
	mono_mb_patch_short_branch (mb, no_freelist_branch);
	if (not_small_enough_branch > 0)
		mono_mb_patch_short_branch (mb, not_small_enough_branch);
	if (size_overflow_branch > 0)
		mono_mb_patch_short_branch (mb, size_overflow_branch);

	/* the slow path: we just call back into the runtime */
 always_slowpath:
	if (atype == ATYPE_STRING) {
		mono_mb_emit_ldarg (mb, 1);
		mono_mb_emit_icall (mb, mono_string_alloc);
	} else {
		mono_mb_emit_ldarg (mb, 0);
		mono_mb_emit_icall (mb, mono_object_new_specific);
	}

	mono_mb_emit_byte (mb, MONO_CEE_RET);

	res = mono_mb_create_method (mb, csig, 8);
	mono_mb_free (mb);
	mono_method_get_header (res)->init_locals = FALSE;

	info = mono_image_alloc0 (mono_defaults.corlib, sizeof (AllocatorWrapperInfo));
	info->gc_name = "boehm";
	info->alloc_type = atype;
	mono_marshal_set_wrapper_info (res, info);

	return res;
}

static MonoMethod* alloc_method_cache [ATYPE_NUM];
static MonoMethod* slowpath_alloc_method_cache [ATYPE_NUM];

static G_GNUC_UNUSED gboolean
mono_gc_is_critical_method (MonoMethod *method)
{
	int i;

	for (i = 0; i < ATYPE_NUM; ++i)
		if (method == alloc_method_cache [i] || method == slowpath_alloc_method_cache [i])
			return TRUE;

	return FALSE;
}

/*
 * If possible, generate a managed method that can quickly allocate objects in class
 * @klass. The method will typically have an thread-local inline allocation sequence.
 * The signature of the called method is:
 * 	object allocate (MonoVTable *vtable)
 * Some of the logic here is similar to mono_class_get_allocation_ftn () i object.c,
 * keep in sync.
 * The thread local alloc logic is taken from libgc/pthread_support.c.
 */

MonoMethod*
mono_gc_get_managed_allocator (MonoClass *klass, gboolean for_box, gboolean known_instance_size)
{
	int offset = -1;
	int atype;
	MONO_THREAD_VAR_OFFSET (GC_thread_tls, offset);

	/*g_print ("thread tls: %d\n", offset);*/
	if (offset == -1)
		return NULL;
	if (!SMALL_ENOUGH (klass->instance_size))
		return NULL;
	if (mono_class_has_finalizer (klass) || mono_class_is_marshalbyref (klass) || (mono_profiler_get_events () & MONO_PROFILE_ALLOCATIONS))
		return NULL;
	if (klass->rank)
		return NULL;
	if (mono_class_is_open_constructed_type (&klass->byval_arg))
		return NULL;
	if (klass->byval_arg.type == MONO_TYPE_STRING) {
		atype = ATYPE_STRING;
	} else if (!klass->has_references) {
		if (for_box)
			atype = ATYPE_FREEPTR_FOR_BOX;
		else
			atype = ATYPE_FREEPTR;
	} else {
		return NULL;
		/*
		 * disabled because we currently do a runtime choice anyway, to
		 * deal with multiple appdomains.
		if (vtable->gc_descr != GC_NO_DESCRIPTOR)
			atype = ATYPE_GCJ;
		else
			atype = ATYPE_NORMAL;
		*/
	}
	return mono_gc_get_managed_allocator_by_type (atype, FALSE);
}

MonoMethod*
mono_gc_get_managed_array_allocator (MonoClass *klass)
{
	return NULL;
}

/**
 * mono_gc_get_managed_allocator_by_type:
 *
 *   Return a managed allocator method corresponding to allocator type ATYPE.
 */
MonoMethod*
mono_gc_get_managed_allocator_by_type (int atype, gboolean slowpath)
{
	int offset = -1;
	MonoMethod *res;
	MonoMethod **cache = slowpath ? slowpath_alloc_method_cache : alloc_method_cache;
	MONO_THREAD_VAR_OFFSET (GC_thread_tls, offset);

	mono_tls_key_set_offset (TLS_KEY_BOEHM_GC_THREAD, offset);

	res = cache [atype];
	if (res)
		return res;

	res = create_allocator (atype, TLS_KEY_BOEHM_GC_THREAD, slowpath);
	mono_mutex_lock (&mono_gc_lock);
	if (cache [atype]) {
		mono_free_method (res);
		res = cache [atype];
	} else {
		mono_memory_barrier ();
		cache [atype] = res;
	}
	mono_mutex_unlock (&mono_gc_lock);
	return res;
}

guint32
mono_gc_get_managed_allocator_types (void)
{
	return ATYPE_NUM;
}

MonoMethod*
mono_gc_get_write_barrier (void)
{
	g_assert_not_reached ();
	return NULL;
}

#else

static G_GNUC_UNUSED gboolean
mono_gc_is_critical_method (MonoMethod *method)
{
	return FALSE;
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
mono_gc_get_managed_allocator_by_type (int atype, gboolean slowpath)
{
	return NULL;
}

guint32
mono_gc_get_managed_allocator_types (void)
{
	return 0;
}

MonoMethod*
mono_gc_get_write_barrier (void)
{
	g_assert_not_reached ();
	return NULL;
}

#endif

MonoMethod*
mono_gc_get_specific_write_barrier (gboolean is_concurrent)
{
	g_assert_not_reached ();
	return NULL;
}

int
mono_gc_get_aligned_size_for_allocator (int size)
{
	return size;
}

const char *
mono_gc_get_gc_name (void)
{
	return "boehm";
}

void*
mono_gc_invoke_with_gc_lock (MonoGCLockedCallbackFunc func, void *data)
{
	return GC_call_with_alloc_lock (func, data);
}

char*
mono_gc_get_description (void)
{
	return g_strdup (DEFAULT_GC_NAME);
}

void
mono_gc_set_desktop_mode (void)
{
	GC_dont_expand = 1;
}

gboolean
mono_gc_is_moving (void)
{
	return FALSE;
}

gboolean
mono_gc_is_disabled (void)
{
	if (GC_dont_gc || g_getenv ("GC_DONT_GC"))
		return TRUE;
	else
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

void mono_gc_set_skip_thread (gboolean value)
{
}

void
mono_gc_register_for_finalization (MonoObject *obj, void *user_data)
{
	guint offset = 0;

#ifndef GC_DEBUG
	/* This assertion is not valid when GC_DEBUG is defined */
	g_assert (GC_base (obj) == (char*)obj - offset);
#endif

	GC_REGISTER_FINALIZER_NO_ORDER ((char*)obj - offset, user_data, GUINT_TO_POINTER (offset), NULL, NULL);
}

#ifndef HOST_WIN32
int
mono_gc_pthread_create (pthread_t *new_thread, const pthread_attr_t *attr, void *(*start_routine)(void *), void *arg)
{
	/* it is being replaced by GC_pthread_create on some
	 * platforms, see libgc/include/gc_pthread_redirects.h */
	return pthread_create (new_thread, attr, start_routine, arg);
}
#endif

#ifdef HOST_WIN32
BOOL APIENTRY mono_gc_dllmain (HMODULE module_handle, DWORD reason, LPVOID reserved)
{
	return GC_DllMain (module_handle, reason, reserved);
}
#endif

guint
mono_gc_get_vtable_bits (MonoClass *class)
{
	if (fin_callbacks.is_class_finalization_aware) {
		if (fin_callbacks.is_class_finalization_aware (class))
			return BOEHM_GC_BIT_FINALIZER_AWARE;
	}
	return 0;
}

/*
 * mono_gc_register_altstack:
 *
 *   Register the dimensions of the normal stack and altstack with the collector.
 * Currently, STACK/STACK_SIZE is only used when the thread is suspended while it is on an altstack.
 */
void
mono_gc_register_altstack (gpointer stack, gint32 stack_size, gpointer altstack, gint32 altstack_size)
{
	GC_register_altstack (stack, stack_size, altstack, altstack_size);
}

int
mono_gc_get_los_limit (void)
{
	return G_MAXINT;
}

void
mono_gc_set_string_length (MonoString *str, gint32 new_length)
{
	mono_unichar2 *new_end = str->chars + new_length;
	
	/* zero the discarded string. This null-delimits the string and allows 
	 * the space to be reclaimed by SGen. */
	 
	memset (new_end, 0, (str->length - new_length + 1) * sizeof (mono_unichar2));
	str->length = new_length;
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

gboolean
mono_gc_set_allow_synchronous_major (gboolean flag)
{
	return flag;
}
/* Toggleref support */

void
mono_gc_toggleref_add (MonoObject *object, mono_bool strong_ref)
{
	GC_toggleref_add ((GC_PTR)object, (int)strong_ref);
}

void
mono_gc_toggleref_register_callback (MonoToggleRefStatus (*proccess_toggleref) (MonoObject *obj))
{
	GC_toggleref_register_callback ((int (*) (GC_PTR obj)) proccess_toggleref);
}

/* Test support code */

static MonoToggleRefStatus
test_toggleref_callback (MonoObject *obj)
{
	static MonoClassField *mono_toggleref_test_field;
	int status = MONO_TOGGLE_REF_DROP;

	if (!mono_toggleref_test_field) {
		mono_toggleref_test_field = mono_class_get_field_from_name (mono_object_get_class (obj), "__test");
		g_assert (mono_toggleref_test_field);
	}

	mono_field_get_value (obj, mono_toggleref_test_field, &status);
	printf ("toggleref-cb obj %d\n", status);
	return status;
}

static void
register_test_toggleref_callback (void)
{
	mono_gc_toggleref_register_callback (test_toggleref_callback);
}

static gboolean
is_finalization_aware (MonoObject *obj)
{
	MonoVTable *vt = obj->vtable;
	return (vt->gc_bits & BOEHM_GC_BIT_FINALIZER_AWARE) == BOEHM_GC_BIT_FINALIZER_AWARE;
}

static void
fin_notifier (MonoObject *obj)
{
	if (is_finalization_aware (obj))
		fin_callbacks.object_queued_for_finalization (obj);
}

void
mono_gc_register_finalizer_callbacks (MonoGCFinalizerCallbacks *callbacks)
{
	if (callbacks->version != MONO_GC_FINALIZER_EXTENSION_VERSION)
		g_error ("Invalid finalizer callback version. Expected %d but got %d\n", MONO_GC_FINALIZER_EXTENSION_VERSION, callbacks->version);

	fin_callbacks = *callbacks;

	GC_set_finalizer_notify_proc ((void (*) (GC_PTR))fin_notifier);
}

gboolean
mono_gc_is_null (void)
{
	return FALSE;
}

#endif /* no Boehm GC */
