/*
 * sgen-gc.c: Simple generational GC.
 *
 * Author:
 * 	Paolo Molaro (lupus@ximian.com)
 *  Rodrigo Kumpera (kumpera@gmail.com)
 *
 * Copyright 2005-2011 Novell, Inc (http://www.novell.com)
 * Copyright 2011 Xamarin Inc (http://www.xamarin.com)
 *
 * Thread start/stop adapted from Boehm's GC:
 * Copyright (c) 1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1996 by Silicon Graphics.  All rights reserved.
 * Copyright (c) 1998 by Fergus Henderson.  All rights reserved.
 * Copyright (c) 2000-2004 by Hewlett-Packard Company.  All rights reserved.
 * Copyright 2001-2003 Ximian, Inc
 * Copyright 2003-2010 Novell, Inc.
 * Copyright 2011 Xamarin, Inc.
 * Copyright (C) 2012 Xamarin Inc
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
 *
 * Important: allocation provides always zeroed memory, having to do
 * a memset after allocation is deadly for performance.
 * Memory usage at startup is currently as follows:
 * 64 KB pinned space
 * 64 KB internal space
 * size of nursery
 * We should provide a small memory config with half the sizes
 *
 * We currently try to make as few mono assumptions as possible:
 * 1) 2-word header with no GC pointers in it (first vtable, second to store the
 *    forwarding ptr)
 * 2) gc descriptor is the second word in the vtable (first word in the class)
 * 3) 8 byte alignment is the minimum and enough (not true for special structures (SIMD), FIXME)
 * 4) there is a function to get an object's size and the number of
 *    elements in an array.
 * 5) we know the special way bounds are allocated for complex arrays
 * 6) we know about proxies and how to treat them when domains are unloaded
 *
 * Always try to keep stack usage to a minimum: no recursive behaviour
 * and no large stack allocs.
 *
 * General description.
 * Objects are initially allocated in a nursery using a fast bump-pointer technique.
 * When the nursery is full we start a nursery collection: this is performed with a
 * copying GC.
 * When the old generation is full we start a copying GC of the old generation as well:
 * this will be changed to mark&sweep with copying when fragmentation becomes to severe
 * in the future.  Maybe we'll even do both during the same collection like IMMIX.
 *
 * The things that complicate this description are:
 * *) pinned objects: we can't move them so we need to keep track of them
 * *) no precise info of the thread stacks and registers: we need to be able to
 *    quickly find the objects that may be referenced conservatively and pin them
 *    (this makes the first issues more important)
 * *) large objects are too expensive to be dealt with using copying GC: we handle them
 *    with mark/sweep during major collections
 * *) some objects need to not move even if they are small (interned strings, Type handles):
 *    we use mark/sweep for them, too: they are not allocated in the nursery, but inside
 *    PinnedChunks regions
 */

/*
 * TODO:

 *) we could have a function pointer in MonoClass to implement
  customized write barriers for value types

 *) investigate the stuff needed to advance a thread to a GC-safe
  point (single-stepping, read from unmapped memory etc) and implement it.
  This would enable us to inline allocations and write barriers, for example,
  or at least parts of them, like the write barrier checks.
  We may need this also for handling precise info on stacks, even simple things
  as having uninitialized data on the stack and having to wait for the prolog
  to zero it. Not an issue for the last frame that we scan conservatively.
  We could always not trust the value in the slots anyway.

 *) modify the jit to save info about references in stack locations:
  this can be done just for locals as a start, so that at least
  part of the stack is handled precisely.

 *) test/fix endianess issues

 *) Implement a card table as the write barrier instead of remembered
    sets?  Card tables are not easy to implement with our current
    memory layout.  We have several different kinds of major heap
    objects: Small objects in regular blocks, small objects in pinned
    chunks and LOS objects.  If we just have a pointer we have no way
    to tell which kind of object it points into, therefore we cannot
    know where its card table is.  The least we have to do to make
    this happen is to get rid of write barriers for indirect stores.
    (See next item)

 *) Get rid of write barriers for indirect stores.  We can do this by
    telling the GC to wbarrier-register an object once we do an ldloca
    or ldelema on it, and to unregister it once it's not used anymore
    (it can only travel downwards on the stack).  The problem with
    unregistering is that it needs to happen eventually no matter
    what, even if exceptions are thrown, the thread aborts, etc.
    Rodrigo suggested that we could do only the registering part and
    let the collector find out (pessimistically) when it's safe to
    unregister, namely when the stack pointer of the thread that
    registered the object is higher than it was when the registering
    happened.  This might make for a good first implementation to get
    some data on performance.

 *) Some sort of blacklist support?  Blacklists is a concept from the
    Boehm GC: if during a conservative scan we find pointers to an
    area which we might use as heap, we mark that area as unusable, so
    pointer retention by random pinning pointers is reduced.

 *) experiment with max small object size (very small right now - 2kb,
    because it's tied to the max freelist size)

  *) add an option to mmap the whole heap in one chunk: it makes for many
     simplifications in the checks (put the nursery at the top and just use a single
     check for inclusion/exclusion): the issue this has is that on 32 bit systems it's
     not flexible (too much of the address space may be used by default or we can't
     increase the heap as needed) and we'd need a race-free mechanism to return memory
     back to the system (mprotect(PROT_NONE) will still keep the memory allocated if it
     was written to, munmap is needed, but the following mmap may not find the same segment
     free...)

 *) memzero the major fragments after restarting the world and optionally a smaller
    chunk at a time

 *) investigate having fragment zeroing threads

 *) separate locks for finalization and other minor stuff to reduce
    lock contention

 *) try a different copying order to improve memory locality

 *) a thread abort after a store but before the write barrier will
    prevent the write barrier from executing

 *) specialized dynamically generated markers/copiers

 *) Dynamically adjust TLAB size to the number of threads.  If we have
    too many threads that do allocation, we might need smaller TLABs,
    and we might get better performance with larger TLABs if we only
    have a handful of threads.  We could sum up the space left in all
    assigned TLABs and if that's more than some percentage of the
    nursery size, reduce the TLAB size.

 *) Explore placing unreachable objects on unused nursery memory.
	Instead of memset'ng a region to zero, place an int[] covering it.
	A good place to start is add_nursery_frag. The tricky thing here is
	placing those objects atomically outside of a collection.

 *) Allocation should use asymmetric Dekker synchronization:
 	http://blogs.oracle.com/dave/resource/Asymmetric-Dekker-Synchronization.txt
	This should help weak consistency archs.
 */
#include "config.h"
#ifdef HAVE_SGEN_GC

#ifdef __MACH__
#undef _XOPEN_SOURCE
#define _XOPEN_SOURCE
#define _DARWIN_C_SOURCE
#endif

#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif
#ifdef HAVE_PTHREAD_H
#include <pthread.h>
#endif
#ifdef HAVE_PTHREAD_NP_H
#include <pthread_np.h>
#endif
#ifdef HAVE_SEMAPHORE_H
#include <semaphore.h>
#endif
#include <stdio.h>
#include <string.h>
#include <signal.h>
#include <errno.h>
#include <assert.h>

#include "metadata/sgen-gc.h"
#include "metadata/metadata-internals.h"
#include "metadata/class-internals.h"
#include "metadata/gc-internal.h"
#include "metadata/object-internals.h"
#include "metadata/threads.h"
#include "metadata/sgen-cardtable.h"
#include "metadata/sgen-protocol.h"
#include "metadata/sgen-archdep.h"
#include "metadata/sgen-bridge.h"
#include "metadata/sgen-memory-governor.h"
#include "metadata/sgen-hash-table.h"
#include "metadata/mono-gc.h"
#include "metadata/method-builder.h"
#include "metadata/profiler-private.h"
#include "metadata/monitor.h"
#include "metadata/mempool-internals.h"
#include "metadata/marshal.h"
#include "metadata/runtime.h"
#include "metadata/sgen-cardtable.h"
#include "metadata/sgen-pinning.h"
#include "metadata/sgen-workers.h"
#include "metadata/sgen-layout-stats.h"
#include "utils/mono-mmap.h"
#include "utils/mono-time.h"
#include "utils/mono-semaphore.h"
#include "utils/mono-counters.h"
#include "utils/mono-proclib.h"
#include "utils/mono-memory-model.h"
#include "utils/mono-logger-internal.h"
#include "utils/dtrace.h"

#include <mono/utils/mono-logger-internal.h>
#include <mono/utils/memcheck.h>

#if defined(__MACH__)
#include "utils/mach-support.h"
#endif

#define OPDEF(a,b,c,d,e,f,g,h,i,j) \
	a = i,

enum {
#include "mono/cil/opcode.def"
	CEE_LAST
};

#undef OPDEF

#undef pthread_create
#undef pthread_join
#undef pthread_detach

/*
 * ######################################################################
 * ########  Types and constants used by the GC.
 * ######################################################################
 */

/* 0 means not initialized, 1 is initialized, -1 means in progress */
static int gc_initialized = 0;
/* If set, check if we need to do something every X allocations */
gboolean has_per_allocation_action;
/* If set, do a heap check every X allocation */
guint32 verify_before_allocs = 0;
/* If set, do a minor collection before every X allocation */
guint32 collect_before_allocs = 0;
/* If set, do a whole heap check before each collection */
static gboolean whole_heap_check_before_collection = FALSE;
/* If set, do a heap consistency check before each minor collection */
static gboolean consistency_check_at_minor_collection = FALSE;
/* If set, do a mod union consistency check before each finishing collection pause */
static gboolean mod_union_consistency_check = FALSE;
/* If set, check whether mark bits are consistent after major collections */
static gboolean check_mark_bits_after_major_collection = FALSE;
/* If set, check that all nursery objects are pinned/not pinned, depending on context */
static gboolean check_nursery_objects_pinned = FALSE;
/* If set, do a few checks when the concurrent collector is used */
static gboolean do_concurrent_checks = FALSE;
/* If set, check that there are no references to the domain left at domain unload */
static gboolean xdomain_checks = FALSE;
/* If not null, dump the heap after each collection into this file */
static FILE *heap_dump_file = NULL;
/* If set, mark stacks conservatively, even if precise marking is possible */
static gboolean conservative_stack_mark = FALSE;
/* If set, do a plausibility check on the scan_starts before and after
   each collection */
static gboolean do_scan_starts_check = FALSE;
/*
 * If the major collector is concurrent and this is FALSE, we will
 * never initiate a synchronous major collection, unless requested via
 * GC.Collect().
 */
static gboolean allow_synchronous_major = TRUE;
static gboolean nursery_collection_is_parallel = FALSE;
static gboolean disable_minor_collections = FALSE;
static gboolean disable_major_collections = FALSE;
gboolean do_pin_stats = FALSE;
static gboolean do_verify_nursery = FALSE;
static gboolean do_dump_nursery_content = FALSE;

#ifdef HEAVY_STATISTICS
long long stat_objects_alloced_degraded = 0;
long long stat_bytes_alloced_degraded = 0;

long long stat_copy_object_called_nursery = 0;
long long stat_objects_copied_nursery = 0;
long long stat_copy_object_called_major = 0;
long long stat_objects_copied_major = 0;

long long stat_scan_object_called_nursery = 0;
long long stat_scan_object_called_major = 0;

long long stat_slots_allocated_in_vain;

long long stat_nursery_copy_object_failed_from_space = 0;
long long stat_nursery_copy_object_failed_forwarded = 0;
long long stat_nursery_copy_object_failed_pinned = 0;
long long stat_nursery_copy_object_failed_to_space = 0;

static int stat_wbarrier_add_to_global_remset = 0;
static int stat_wbarrier_set_field = 0;
static int stat_wbarrier_set_arrayref = 0;
static int stat_wbarrier_arrayref_copy = 0;
static int stat_wbarrier_generic_store = 0;
static int stat_wbarrier_generic_store_atomic = 0;
static int stat_wbarrier_set_root = 0;
static int stat_wbarrier_value_copy = 0;
static int stat_wbarrier_object_copy = 0;
#endif

static long long stat_pinned_objects = 0;

static long long time_minor_pre_collection_fragment_clear = 0;
static long long time_minor_pinning = 0;
static long long time_minor_scan_remsets = 0;
static long long time_minor_scan_pinned = 0;
static long long time_minor_scan_registered_roots = 0;
static long long time_minor_scan_thread_data = 0;
static long long time_minor_finish_gray_stack = 0;
static long long time_minor_fragment_creation = 0;

static long long time_major_pre_collection_fragment_clear = 0;
static long long time_major_pinning = 0;
static long long time_major_scan_pinned = 0;
static long long time_major_scan_registered_roots = 0;
static long long time_major_scan_thread_data = 0;
static long long time_major_scan_alloc_pinned = 0;
static long long time_major_scan_finalized = 0;
static long long time_major_scan_big_objects = 0;
static long long time_major_finish_gray_stack = 0;
static long long time_major_free_bigobjs = 0;
static long long time_major_los_sweep = 0;
static long long time_major_sweep = 0;
static long long time_major_fragment_creation = 0;

int gc_debug_level = 0;
FILE* gc_debug_file;

static MonoGCFinalizerCallbacks fin_callbacks;

/*
void
mono_gc_flush_info (void)
{
	fflush (gc_debug_file);
}
*/

#define TV_DECLARE SGEN_TV_DECLARE
#define TV_GETTIME SGEN_TV_GETTIME
#define TV_ELAPSED SGEN_TV_ELAPSED
#define TV_ELAPSED_MS SGEN_TV_ELAPSED_MS

SGEN_TV_DECLARE (sgen_init_timestamp);

#define ALIGN_TO(val,align) ((((guint64)val) + ((align) - 1)) & ~((align) - 1))

NurseryClearPolicy nursery_clear_policy = CLEAR_AT_TLAB_CREATION;

#define object_is_forwarded	SGEN_OBJECT_IS_FORWARDED
#define object_is_pinned	SGEN_OBJECT_IS_PINNED
#define pin_object		SGEN_PIN_OBJECT
#define unpin_object		SGEN_UNPIN_OBJECT

#define ptr_in_nursery sgen_ptr_in_nursery

#define LOAD_VTABLE	SGEN_LOAD_VTABLE

static const char*
safe_name (void* obj)
{
	MonoVTable *vt = (MonoVTable*)LOAD_VTABLE (obj);
	return vt->klass->name;
}

#define safe_object_get_size	sgen_safe_object_get_size

const char*
sgen_safe_name (void* obj)
{
	return safe_name (obj);
}

/*
 * ######################################################################
 * ########  Global data.
 * ######################################################################
 */
LOCK_DECLARE (gc_mutex);
gboolean sgen_try_free_some_memory;

#define SCAN_START_SIZE	SGEN_SCAN_START_SIZE

static mword pagesize = 4096;
size_t degraded_mode = 0;

static mword bytes_pinned_from_failed_allocation = 0;

GCMemSection *nursery_section = NULL;
static mword lowest_heap_address = ~(mword)0;
static mword highest_heap_address = 0;

LOCK_DECLARE (sgen_interruption_mutex);
static LOCK_DECLARE (pin_queue_mutex);

#define LOCK_PIN_QUEUE mono_mutex_lock (&pin_queue_mutex)
#define UNLOCK_PIN_QUEUE mono_mutex_unlock (&pin_queue_mutex)

typedef struct _FinalizeReadyEntry FinalizeReadyEntry;
struct _FinalizeReadyEntry {
	FinalizeReadyEntry *next;
	void *object;
};

typedef struct _EphemeronLinkNode EphemeronLinkNode;

struct _EphemeronLinkNode {
	EphemeronLinkNode *next;
	char *array;
};

typedef struct {
       void *key;
       void *value;
} Ephemeron;

int current_collection_generation = -1;
volatile gboolean concurrent_collection_in_progress = FALSE;

/* objects that are ready to be finalized */
static FinalizeReadyEntry *fin_ready_list = NULL;
static FinalizeReadyEntry *critical_fin_list = NULL;

static EphemeronLinkNode *ephemeron_list;

/* registered roots: the key to the hash is the root start address */
/* 
 * Different kinds of roots are kept separate to speed up pin_from_roots () for example.
 */
SgenHashTable roots_hash [ROOT_TYPE_NUM] = {
	SGEN_HASH_TABLE_INIT (INTERNAL_MEM_ROOTS_TABLE, INTERNAL_MEM_ROOT_RECORD, sizeof (RootRecord), mono_aligned_addr_hash, NULL),
	SGEN_HASH_TABLE_INIT (INTERNAL_MEM_ROOTS_TABLE, INTERNAL_MEM_ROOT_RECORD, sizeof (RootRecord), mono_aligned_addr_hash, NULL),
	SGEN_HASH_TABLE_INIT (INTERNAL_MEM_ROOTS_TABLE, INTERNAL_MEM_ROOT_RECORD, sizeof (RootRecord), mono_aligned_addr_hash, NULL)
};
static mword roots_size = 0; /* amount of memory in the root set */

#define GC_ROOT_NUM 32
typedef struct {
	int count;		/* must be the first field */
	void *objects [GC_ROOT_NUM];
	int root_types [GC_ROOT_NUM];
	uintptr_t extra_info [GC_ROOT_NUM];
} GCRootReport;

static void
notify_gc_roots (GCRootReport *report)
{
	if (!report->count)
		return;
	mono_profiler_gc_roots (report->count, report->objects, report->root_types, report->extra_info);
	report->count = 0;
}

static void
add_profile_gc_root (GCRootReport *report, void *object, int rtype, uintptr_t extra_info)
{
	if (report->count == GC_ROOT_NUM)
		notify_gc_roots (report);
	report->objects [report->count] = object;
	report->root_types [report->count] = rtype;
	report->extra_info [report->count++] = (uintptr_t)((MonoVTable*)LOAD_VTABLE (object))->klass;
}

MonoNativeTlsKey thread_info_key;

#ifdef HAVE_KW_THREAD
__thread SgenThreadInfo *sgen_thread_info;
__thread char *stack_end;
#endif

/* The size of a TLAB */
/* The bigger the value, the less often we have to go to the slow path to allocate a new 
 * one, but the more space is wasted by threads not allocating much memory.
 * FIXME: Tune this.
 * FIXME: Make this self-tuning for each thread.
 */
guint32 tlab_size = (1024 * 4);

#define MAX_SMALL_OBJ_SIZE	SGEN_MAX_SMALL_OBJ_SIZE

/* Functions supplied by the runtime to be called by the GC */
static MonoGCCallbacks gc_callbacks;

#define ALLOC_ALIGN		SGEN_ALLOC_ALIGN
#define ALLOC_ALIGN_BITS	SGEN_ALLOC_ALIGN_BITS

#define ALIGN_UP		SGEN_ALIGN_UP

#define MOVED_OBJECTS_NUM 64
static void *moved_objects [MOVED_OBJECTS_NUM];
static int moved_objects_idx = 0;

/* Vtable of the objects used to fill out nursery fragments before a collection */
static MonoVTable *array_fill_vtable;

#ifdef SGEN_DEBUG_INTERNAL_ALLOC
MonoNativeThreadId main_gc_thread = NULL;
#endif

/*Object was pinned during the current collection*/
static mword objects_pinned;

/*
 * ######################################################################
 * ########  Macros and function declarations.
 * ######################################################################
 */

inline static void*
align_pointer (void *ptr)
{
	mword p = (mword)ptr;
	p += sizeof (gpointer) - 1;
	p &= ~ (sizeof (gpointer) - 1);
	return (void*)p;
}

typedef SgenGrayQueue GrayQueue;

/* forward declarations */
static void scan_thread_data (void *start_nursery, void *end_nursery, gboolean precise, GrayQueue *queue);
static void scan_from_registered_roots (char *addr_start, char *addr_end, int root_type, ScanCopyContext ctx);
static void scan_finalizer_entries (FinalizeReadyEntry *list, ScanCopyContext ctx);
static void report_finalizer_roots (void);
static void report_registered_roots (void);

static void pin_from_roots (void *start_nursery, void *end_nursery, GrayQueue *queue);
static int pin_objects_from_addresses (GCMemSection *section, void **start, void **end, void *start_nursery, void *end_nursery, ScanCopyContext ctx);
static void finish_gray_stack (int generation, GrayQueue *queue);

void mono_gc_scan_for_specific_ref (MonoObject *key, gboolean precise);


static void init_stats (void);

static int mark_ephemerons_in_range (ScanCopyContext ctx);
static void clear_unreachable_ephemerons (ScanCopyContext ctx);
static void null_ephemerons_for_domain (MonoDomain *domain);

static gboolean major_update_or_finish_concurrent_collection (gboolean force_finish);

SgenObjectOperations current_object_ops;
SgenMajorCollector major_collector;
SgenMinorCollector sgen_minor_collector;
static GrayQueue gray_queue;

static SgenRemeberedSet remset;

/* The gray queue to use from the main collection thread. */
#define WORKERS_DISTRIBUTE_GRAY_QUEUE	(&gray_queue)

/*
 * The gray queue a worker job must use.  If we're not parallel or
 * concurrent, we use the main gray queue.
 */
static SgenGrayQueue*
sgen_workers_get_job_gray_queue (WorkerData *worker_data)
{
	return worker_data ? &worker_data->private_gray_queue : WORKERS_DISTRIBUTE_GRAY_QUEUE;
}

static void
gray_queue_redirect (SgenGrayQueue *queue)
{
	gboolean wake = FALSE;


	for (;;) {
		GrayQueueSection *section = sgen_gray_object_dequeue_section (queue);
		if (!section)
			break;
		sgen_section_gray_queue_enqueue (queue->alloc_prepare_data, section);
		wake = TRUE;
	}

	if (wake) {
		g_assert (concurrent_collection_in_progress ||
				(current_collection_generation == GENERATION_OLD && major_collector.is_parallel));
		if (sgen_workers_have_started ()) {
			sgen_workers_wake_up_all ();
		} else {
			if (concurrent_collection_in_progress)
				g_assert (current_collection_generation == -1);
		}
	}
}

void
sgen_scan_area_with_callback (char *start, char *end, IterateObjectCallbackFunc callback, void *data, gboolean allow_flags)
{
	while (start < end) {
		size_t size;
		char *obj;

		if (!*(void**)start) {
			start += sizeof (void*); /* should be ALLOC_ALIGN, really */
			continue;
		}

		if (allow_flags) {
			if (!(obj = SGEN_OBJECT_IS_FORWARDED (start)))
				obj = start;
		} else {
			obj = start;
		}

		size = ALIGN_UP (safe_object_get_size ((MonoObject*)obj));

		if ((MonoVTable*)SGEN_LOAD_VTABLE (obj) != array_fill_vtable)
			callback (obj, size, data);

		start += size;
	}
}

static gboolean
need_remove_object_for_domain (char *start, MonoDomain *domain)
{
	if (mono_object_domain (start) == domain) {
		SGEN_LOG (4, "Need to cleanup object %p", start);
		binary_protocol_cleanup (start, (gpointer)LOAD_VTABLE (start), safe_object_get_size ((MonoObject*)start));
		return TRUE;
	}
	return FALSE;
}

static void
process_object_for_domain_clearing (char *start, MonoDomain *domain)
{
	GCVTable *vt = (GCVTable*)LOAD_VTABLE (start);
	if (vt->klass == mono_defaults.internal_thread_class)
		g_assert (mono_object_domain (start) == mono_get_root_domain ());
	/* The object could be a proxy for an object in the domain
	   we're deleting. */
#ifndef DISABLE_REMOTING
	if (mono_defaults.real_proxy_class->supertypes && mono_class_has_parent_fast (vt->klass, mono_defaults.real_proxy_class)) {
		MonoObject *server = ((MonoRealProxy*)start)->unwrapped_server;

		/* The server could already have been zeroed out, so
		   we need to check for that, too. */
		if (server && (!LOAD_VTABLE (server) || mono_object_domain (server) == domain)) {
			SGEN_LOG (4, "Cleaning up remote pointer in %p to object %p", start, server);
			((MonoRealProxy*)start)->unwrapped_server = NULL;
		}
	}
#endif
}

static gboolean
clear_domain_process_object (char *obj, MonoDomain *domain)
{
	gboolean remove;

	process_object_for_domain_clearing (obj, domain);
	remove = need_remove_object_for_domain (obj, domain);

	if (remove && ((MonoObject*)obj)->synchronisation) {
		void **dislink = mono_monitor_get_object_monitor_weak_link ((MonoObject*)obj);
		if (dislink)
			sgen_register_disappearing_link (NULL, dislink, FALSE, TRUE);
	}

	return remove;
}

static void
clear_domain_process_minor_object_callback (char *obj, size_t size, MonoDomain *domain)
{
	if (clear_domain_process_object (obj, domain))
		memset (obj, 0, size);
}

static void
clear_domain_process_major_object_callback (char *obj, size_t size, MonoDomain *domain)
{
	clear_domain_process_object (obj, domain);
}

static void
clear_domain_free_major_non_pinned_object_callback (char *obj, size_t size, MonoDomain *domain)
{
	if (need_remove_object_for_domain (obj, domain))
		major_collector.free_non_pinned_object (obj, size);
}

static void
clear_domain_free_major_pinned_object_callback (char *obj, size_t size, MonoDomain *domain)
{
	if (need_remove_object_for_domain (obj, domain))
		major_collector.free_pinned_object (obj, size);
}

/*
 * When appdomains are unloaded we can easily remove objects that have finalizers,
 * but all the others could still be present in random places on the heap.
 * We need a sweep to get rid of them even though it's going to be costly
 * with big heaps.
 * The reason we need to remove them is because we access the vtable and class
 * structures to know the object size and the reference bitmap: once the domain is
 * unloaded the point to random memory.
 */
void
mono_gc_clear_domain (MonoDomain * domain)
{
	LOSObject *bigobj, *prev;
	int i;

	LOCK_GC;

	binary_protocol_domain_unload_begin (domain);

	sgen_stop_world (0);

	if (concurrent_collection_in_progress)
		sgen_perform_collection (0, GENERATION_OLD, "clear domain", TRUE);
	g_assert (!concurrent_collection_in_progress);

	sgen_process_fin_stage_entries ();
	sgen_process_dislink_stage_entries ();

	sgen_clear_nursery_fragments ();

	if (xdomain_checks && domain != mono_get_root_domain ()) {
		sgen_scan_for_registered_roots_in_domain (domain, ROOT_TYPE_NORMAL);
		sgen_scan_for_registered_roots_in_domain (domain, ROOT_TYPE_WBARRIER);
		sgen_check_for_xdomain_refs ();
	}

	/*Ephemerons and dislinks must be processed before LOS since they might end up pointing
	to memory returned to the OS.*/
	null_ephemerons_for_domain (domain);

	for (i = GENERATION_NURSERY; i < GENERATION_MAX; ++i)
		sgen_null_links_for_domain (domain, i);

	for (i = GENERATION_NURSERY; i < GENERATION_MAX; ++i)
		sgen_remove_finalizers_for_domain (domain, i);

	sgen_scan_area_with_callback (nursery_section->data, nursery_section->end_data,
			(IterateObjectCallbackFunc)clear_domain_process_minor_object_callback, domain, FALSE);

	/* We need two passes over major and large objects because
	   freeing such objects might give their memory back to the OS
	   (in the case of large objects) or obliterate its vtable
	   (pinned objects with major-copying or pinned and non-pinned
	   objects with major-mark&sweep), but we might need to
	   dereference a pointer from an object to another object if
	   the first object is a proxy. */
	major_collector.iterate_objects (ITERATE_OBJECTS_SWEEP_ALL, (IterateObjectCallbackFunc)clear_domain_process_major_object_callback, domain);
	for (bigobj = los_object_list; bigobj; bigobj = bigobj->next)
		clear_domain_process_object (bigobj->data, domain);

	prev = NULL;
	for (bigobj = los_object_list; bigobj;) {
		if (need_remove_object_for_domain (bigobj->data, domain)) {
			LOSObject *to_free = bigobj;
			if (prev)
				prev->next = bigobj->next;
			else
				los_object_list = bigobj->next;
			bigobj = bigobj->next;
			SGEN_LOG (4, "Freeing large object %p", bigobj->data);
			sgen_los_free_object (to_free);
			continue;
		}
		prev = bigobj;
		bigobj = bigobj->next;
	}
	major_collector.iterate_objects (ITERATE_OBJECTS_SWEEP_NON_PINNED, (IterateObjectCallbackFunc)clear_domain_free_major_non_pinned_object_callback, domain);
	major_collector.iterate_objects (ITERATE_OBJECTS_SWEEP_PINNED, (IterateObjectCallbackFunc)clear_domain_free_major_pinned_object_callback, domain);

	if (domain == mono_get_root_domain ()) {
		if (G_UNLIKELY (do_pin_stats))
			sgen_pin_stats_print_class_stats ();
		sgen_object_layout_dump (stdout);
	}

	sgen_restart_world (0, NULL);

	binary_protocol_domain_unload_end (domain);

	UNLOCK_GC;
}

/*
 * sgen_add_to_global_remset:
 *
 *   The global remset contains locations which point into newspace after
 * a minor collection. This can happen if the objects they point to are pinned.
 *
 * LOCKING: If called from a parallel collector, the global remset
 * lock must be held.  For serial collectors that is not necessary.
 */
void
sgen_add_to_global_remset (gpointer ptr, gpointer obj)
{
	SGEN_ASSERT (5, sgen_ptr_in_nursery (obj), "Target pointer of global remset must be in the nursery");

	HEAVY_STAT (++stat_wbarrier_add_to_global_remset);

	if (!major_collector.is_concurrent) {
		SGEN_ASSERT (5, current_collection_generation != -1, "Global remsets can only be added during collections");
	} else {
		if (current_collection_generation == -1)
			SGEN_ASSERT (5, sgen_concurrent_collection_in_progress (), "Global remsets outside of collection pauses can only be added by the concurrent collector");
	}

	if (!object_is_pinned (obj))
		SGEN_ASSERT (5, sgen_minor_collector.is_split || sgen_concurrent_collection_in_progress (), "Non-pinned objects can only remain in nursery if it is a split nursery");
	else if (sgen_cement_lookup_or_register (obj))
		return;

	remset.record_pointer (ptr);

	if (G_UNLIKELY (do_pin_stats))
		sgen_pin_stats_register_global_remset (obj);

	SGEN_LOG (8, "Adding global remset for %p", ptr);
	binary_protocol_global_remset (ptr, obj, (gpointer)SGEN_LOAD_VTABLE (obj));


#ifdef ENABLE_DTRACE
	if (G_UNLIKELY (MONO_GC_GLOBAL_REMSET_ADD_ENABLED ())) {
		MonoVTable *vt = (MonoVTable*)LOAD_VTABLE (obj);
		MONO_GC_GLOBAL_REMSET_ADD ((mword)ptr, (mword)obj, sgen_safe_object_get_size (obj),
				vt->klass->name_space, vt->klass->name);
	}
#endif
}

/*
 * sgen_drain_gray_stack:
 *
 *   Scan objects in the gray stack until the stack is empty. This should be called
 * frequently after each object is copied, to achieve better locality and cache
 * usage.
 */
gboolean
sgen_drain_gray_stack (int max_objs, ScanCopyContext ctx)
{
	char *obj;
	ScanObjectFunc scan_func = ctx.scan_func;
	GrayQueue *queue = ctx.queue;

	if (max_objs == -1) {
		for (;;) {
			GRAY_OBJECT_DEQUEUE (queue, &obj);
			if (!obj)
				return TRUE;
			SGEN_LOG (9, "Precise gray object scan %p (%s)", obj, safe_name (obj));
			scan_func (obj, queue);
		}
	} else {
		int i;

		do {
			for (i = 0; i != max_objs; ++i) {
				GRAY_OBJECT_DEQUEUE (queue, &obj);
				if (!obj)
					return TRUE;
				SGEN_LOG (9, "Precise gray object scan %p (%s)", obj, safe_name (obj));
				scan_func (obj, queue);
			}
		} while (max_objs < 0);
		return FALSE;
	}
}

/*
 * Addresses from start to end are already sorted. This function finds
 * the object header for each address and pins the object. The
 * addresses must be inside the passed section.  The (start of the)
 * address array is overwritten with the addresses of the actually
 * pinned objects.  Return the number of pinned objects.
 */
static int
pin_objects_from_addresses (GCMemSection *section, void **start, void **end, void *start_nursery, void *end_nursery, ScanCopyContext ctx)
{
	void *last = NULL;
	int count = 0;
	void *search_start;
	void *last_pinned_obj = NULL;
	size_t last_pinned_obj_size = 0;
	void *addr;
	size_t idx;
	void **definitely_pinned = start;
	ScanObjectFunc scan_func = ctx.scan_func;
	SgenGrayQueue *queue = ctx.queue;

	sgen_nursery_allocator_prepare_for_pinning ();

	while (start < end) {
		gboolean found;

		addr = *start;

		SGEN_ASSERT (0, addr >= start_nursery && addr < end_nursery, "Potential pinning address out of range");
		SGEN_ASSERT (0, addr >= last, "Pin queue not sorted");

		if (addr == last) {
			++start;
			continue;
		}

		SGEN_LOG (5, "Considering pinning addr %p", addr);
		/* multiple pointers to the same object */
		if (addr >= last_pinned_obj && (char*)addr < (char*)last_pinned_obj + last_pinned_obj_size) {
			start++;
			continue;
		}

		/*
		 * Find the closest scan start <= addr.  We might search backward in the
		 * scan_starts array because entries might be NULL.  In the worst case we
		 * start at start_nursery.
		 */
		idx = ((char*)addr - (char*)section->data) / SCAN_START_SIZE;
		SGEN_ASSERT (0, idx < section->num_scan_start, "Scan start index out of range");
		search_start = (void*)section->scan_starts [idx];
		if (!search_start || search_start > addr) {
			while (idx) {
				--idx;
				search_start = section->scan_starts [idx];
				if (search_start && search_start <= addr)
					break;
			}
			if (!search_start || search_start > addr)
				search_start = start_nursery;
		}

		/*
		 * If the last object we pinned is closer than the scan start we found,
		 * start searching after that pinned object instead.
		 */
		if (search_start < last_pinned_obj)
			search_start = (char*)last_pinned_obj + last_pinned_obj_size;

		/*
		 * Now addr should be in an object a short distance from search_start.
		 *
		 * search_start must point to zeroed mem or point to an object.
		 */
		found = FALSE;
		do {
			size_t obj_size;

			/* Skip zeros. */
			if (!*(void**)search_start) {
				search_start = (void*)ALIGN_UP ((mword)search_start + sizeof (gpointer));
				/* The loop condition makes sure we don't overrun addr. */
				continue;
			}

			obj_size = ALIGN_UP (safe_object_get_size ((MonoObject*)search_start));

			if (addr >= search_start && (char*)addr < (char*)search_start + obj_size) {
				/* This is the object we're looking for. */
				last_pinned_obj = search_start;
				last_pinned_obj_size = obj_size;
				found = TRUE;
				break;
			}

			/* Skip to the next object */
			search_start = (void*)((char*)search_start + obj_size);
		} while (search_start <= addr);

		/* We've searched past the address we were looking for. */
		if (!found)
			goto next_pin_queue_entry;

		/*
		 * If this is a dummy array marking the beginning of a nursery
		 * fragment, we don't process it.
		 */
		if (((MonoObject*)search_start)->synchronisation == GINT_TO_POINTER (-1))
			goto next_pin_queue_entry;

		/*
		 * Finally - pin the object!
		 */
		if (scan_func) {
			scan_func (last_pinned_obj, queue);
		} else {
			SGEN_LOG (4, "Pinned object %p, vtable %p (%s), count %d\n",
					last_pinned_obj, *(void**)last_pinned_obj, safe_name (last_pinned_obj), count);
			binary_protocol_pin (last_pinned_obj,
					(gpointer)LOAD_VTABLE (last_pinned_obj),
					safe_object_get_size (last_pinned_obj));

#ifdef ENABLE_DTRACE
			if (G_UNLIKELY (MONO_GC_OBJ_PINNED_ENABLED ())) {
				int gen = sgen_ptr_in_nursery (last_pinned_obj) ? GENERATION_NURSERY : GENERATION_OLD;
				MonoVTable *vt = (MonoVTable*)LOAD_VTABLE (last_pinned_obj);
				MONO_GC_OBJ_PINNED ((mword)last_pinned_obj,
						sgen_safe_object_get_size (last_pinned_obj),
						vt->klass->name_space, vt->klass->name, gen);
			}
#endif

			pin_object (last_pinned_obj);
			GRAY_OBJECT_ENQUEUE (queue, last_pinned_obj);
			if (G_UNLIKELY (do_pin_stats))
				sgen_pin_stats_register_object (last_pinned_obj, last_pinned_obj_size);
			definitely_pinned [count] = last_pinned_obj;
			count++;
		}

		/*
		 * We either pinned the correct object or we ignored the addr because it
		 * points to unused zeroed memory.
		 */
	next_pin_queue_entry:
		last = addr;
		++start;
	}
	//printf ("effective pinned: %d (at the end: %d)\n", count, (char*)end_nursery - (char*)last);
	if (mono_profiler_get_events () & MONO_PROFILE_GC_ROOTS) {
		GCRootReport report;
		report.count = 0;
		for (idx = 0; idx < count; ++idx)
			add_profile_gc_root (&report, definitely_pinned [idx], MONO_PROFILE_GC_ROOT_PINNING | MONO_PROFILE_GC_ROOT_MISC, 0);
		notify_gc_roots (&report);
	}
	stat_pinned_objects += count;
	return count;
}

void
sgen_pin_objects_in_section (GCMemSection *section, ScanCopyContext ctx)
{
	size_t num_entries = section->pin_queue_num_entries;
	if (num_entries) {
		void **start = section->pin_queue_start;
		size_t reduced_to;
		reduced_to = pin_objects_from_addresses (section, start, start + num_entries,
				section->data, section->next_data, ctx);
		section->pin_queue_num_entries = reduced_to;
		if (!reduced_to)
			section->pin_queue_start = NULL;
	}
}


void
sgen_pin_object (void *object, GrayQueue *queue)
{
	g_assert (!concurrent_collection_in_progress);

	if (sgen_collection_is_parallel ()) {
		LOCK_PIN_QUEUE;
		/*object arrives pinned*/
		sgen_pin_stage_ptr (object);
		++objects_pinned ;
		UNLOCK_PIN_QUEUE;
	} else {
		SGEN_PIN_OBJECT (object);
		sgen_pin_stage_ptr (object);
		++objects_pinned;
		if (G_UNLIKELY (do_pin_stats))
			sgen_pin_stats_register_object (object, safe_object_get_size (object));
	}
	GRAY_OBJECT_ENQUEUE (queue, object);
	binary_protocol_pin (object, (gpointer)LOAD_VTABLE (object), safe_object_get_size (object));

#ifdef ENABLE_DTRACE
	if (G_UNLIKELY (MONO_GC_OBJ_PINNED_ENABLED ())) {
		int gen = sgen_ptr_in_nursery (object) ? GENERATION_NURSERY : GENERATION_OLD;
		MonoVTable *vt = (MonoVTable*)LOAD_VTABLE (object);
		MONO_GC_OBJ_PINNED ((mword)object, sgen_safe_object_get_size (object), vt->klass->name_space, vt->klass->name, gen);
	}
#endif
}

void
sgen_parallel_pin_or_update (void **ptr, void *obj, MonoVTable *vt, SgenGrayQueue *queue)
{
	for (;;) {
		mword vtable_word;
		gboolean major_pinned = FALSE;

		if (sgen_ptr_in_nursery (obj)) {
			if (SGEN_CAS_PTR (obj, (void*)((mword)vt | SGEN_PINNED_BIT), vt) == vt) {
				sgen_pin_object (obj, queue);
				break;
			}
		} else {
			major_collector.pin_major_object (obj, queue);
			major_pinned = TRUE;
		}

		vtable_word = *(mword*)obj;
		/*someone else forwarded it, update the pointer and bail out*/
		if (vtable_word & SGEN_FORWARDED_BIT) {
			*ptr = (void*)(vtable_word & ~SGEN_VTABLE_BITS_MASK);
			break;
		}

		/*someone pinned it, nothing to do.*/
		if (vtable_word & SGEN_PINNED_BIT || major_pinned)
			break;
	}
}

/* Sort the addresses in array in increasing order.
 * Done using a by-the book heap sort. Which has decent and stable performance, is pretty cache efficient.
 */
void
sgen_sort_addresses (void **array, size_t size)
{
	size_t i;
	void *tmp;

	for (i = 1; i < size; ++i) {
		size_t child = i;
		while (child > 0) {
			size_t parent = (child - 1) / 2;

			if (array [parent] >= array [child])
				break;

			tmp = array [parent];
			array [parent] = array [child];
			array [child] = tmp;

			child = parent;
		}
	}

	for (i = size - 1; i > 0; --i) {
		size_t end, root;
		tmp = array [i];
		array [i] = array [0];
		array [0] = tmp;

		end = i - 1;
		root = 0;

		while (root * 2 + 1 <= end) {
			size_t child = root * 2 + 1;

			if (child < end && array [child] < array [child + 1])
				++child;
			if (array [root] >= array [child])
				break;

			tmp = array [root];
			array [root] = array [child];
			array [child] = tmp;

			root = child;
		}
	}
}

/* 
 * Scan the memory between start and end and queue values which could be pointers
 * to the area between start_nursery and end_nursery for later consideration.
 * Typically used for thread stacks.
 */
static void
conservatively_pin_objects_from (void **start, void **end, void *start_nursery, void *end_nursery, int pin_type)
{
	int count = 0;

#ifdef VALGRIND_MAKE_MEM_DEFINED_IF_ADDRESSABLE
	VALGRIND_MAKE_MEM_DEFINED_IF_ADDRESSABLE (start, (char*)end - (char*)start);
#endif

	while (start < end) {
		if (*start >= start_nursery && *start < end_nursery) {
			/*
			 * *start can point to the middle of an object
			 * note: should we handle pointing at the end of an object?
			 * pinning in C# code disallows pointing at the end of an object
			 * but there is some small chance that an optimizing C compiler
			 * may keep the only reference to an object by pointing
			 * at the end of it. We ignore this small chance for now.
			 * Pointers to the end of an object are indistinguishable
			 * from pointers to the start of the next object in memory
			 * so if we allow that we'd need to pin two objects...
			 * We queue the pointer in an array, the
			 * array will then be sorted and uniqued. This way
			 * we can coalesce several pinning pointers and it should
			 * be faster since we'd do a memory scan with increasing
			 * addresses. Note: we can align the address to the allocation
			 * alignment, so the unique process is more effective.
			 */
			mword addr = (mword)*start;
			addr &= ~(ALLOC_ALIGN - 1);
			if (addr >= (mword)start_nursery && addr < (mword)end_nursery) {
				SGEN_LOG (6, "Pinning address %p from %p", (void*)addr, start);
				sgen_pin_stage_ptr ((void*)addr);
				count++;
			}
			if (G_UNLIKELY (do_pin_stats)) { 
				if (ptr_in_nursery ((void*)addr))
					sgen_pin_stats_register_address ((char*)addr, pin_type);
			}
		}
		start++;
	}
	if (count)
		SGEN_LOG (7, "found %d potential pinned heap pointers", count);
}

/*
 * The first thing we do in a collection is to identify pinned objects.
 * This function considers all the areas of memory that need to be
 * conservatively scanned.
 */
static void
pin_from_roots (void *start_nursery, void *end_nursery, GrayQueue *queue)
{
	void **start_root;
	RootRecord *root;
	SGEN_LOG (2, "Scanning pinned roots (%d bytes, %d/%d entries)", (int)roots_size, roots_hash [ROOT_TYPE_NORMAL].num_entries, roots_hash [ROOT_TYPE_PINNED].num_entries);
	/* objects pinned from the API are inside these roots */
	SGEN_HASH_TABLE_FOREACH (&roots_hash [ROOT_TYPE_PINNED], start_root, root) {
		SGEN_LOG (6, "Pinned roots %p-%p", start_root, root->end_root);
		conservatively_pin_objects_from (start_root, (void**)root->end_root, start_nursery, end_nursery, PIN_TYPE_OTHER);
	} SGEN_HASH_TABLE_FOREACH_END;
	/* now deal with the thread stacks
	 * in the future we should be able to conservatively scan only:
	 * *) the cpu registers
	 * *) the unmanaged stack frames
	 * *) the _last_ managed stack frame
	 * *) pointers slots in managed frames
	 */
	scan_thread_data (start_nursery, end_nursery, FALSE, queue);
}

static void
unpin_objects_from_queue (SgenGrayQueue *queue)
{
	for (;;) {
		char *addr;
		GRAY_OBJECT_DEQUEUE (queue, &addr);
		if (!addr)
			break;
		g_assert (SGEN_OBJECT_IS_PINNED (addr));
		SGEN_UNPIN_OBJECT (addr);
	}
}

typedef struct {
	CopyOrMarkObjectFunc func;
	GrayQueue *queue;
} UserCopyOrMarkData;

static void
single_arg_user_copy_or_mark (void **obj, void *gc_data)
{
	UserCopyOrMarkData *data = gc_data;

	data->func (obj, data->queue);
}

/*
 * The memory area from start_root to end_root contains pointers to objects.
 * Their position is precisely described by @desc (this means that the pointer
 * can be either NULL or the pointer to the start of an object).
 * This functions copies them to to_space updates them.
 *
 * This function is not thread-safe!
 */
static void
precisely_scan_objects_from (void** start_root, void** end_root, char* n_start, char *n_end, mword desc, ScanCopyContext ctx)
{
	CopyOrMarkObjectFunc copy_func = ctx.copy_func;
	SgenGrayQueue *queue = ctx.queue;

	switch (desc & ROOT_DESC_TYPE_MASK) {
	case ROOT_DESC_BITMAP:
		desc >>= ROOT_DESC_TYPE_SHIFT;
		while (desc) {
			if ((desc & 1) && *start_root) {
				copy_func (start_root, queue);
				SGEN_LOG (9, "Overwrote root at %p with %p", start_root, *start_root);
				sgen_drain_gray_stack (-1, ctx);
			}
			desc >>= 1;
			start_root++;
		}
		return;
	case ROOT_DESC_COMPLEX: {
		gsize *bitmap_data = sgen_get_complex_descriptor_bitmap (desc);
		gsize bwords = (*bitmap_data) - 1;
		void **start_run = start_root;
		bitmap_data++;
		while (bwords-- > 0) {
			gsize bmap = *bitmap_data++;
			void **objptr = start_run;
			while (bmap) {
				if ((bmap & 1) && *objptr) {
					copy_func (objptr, queue);
					SGEN_LOG (9, "Overwrote root at %p with %p", objptr, *objptr);
					sgen_drain_gray_stack (-1, ctx);
				}
				bmap >>= 1;
				++objptr;
			}
			start_run += GC_BITS_PER_WORD;
		}
		break;
	}
	case ROOT_DESC_USER: {
		UserCopyOrMarkData data = { copy_func, queue };
		MonoGCRootMarkFunc marker = sgen_get_user_descriptor_func (desc);
		marker (start_root, single_arg_user_copy_or_mark, &data);
		break;
	}
	case ROOT_DESC_RUN_LEN:
		g_assert_not_reached ();
	default:
		g_assert_not_reached ();
	}
}

static void
reset_heap_boundaries (void)
{
	lowest_heap_address = ~(mword)0;
	highest_heap_address = 0;
}

void
sgen_update_heap_boundaries (mword low, mword high)
{
	mword old;

	do {
		old = lowest_heap_address;
		if (low >= old)
			break;
	} while (SGEN_CAS_PTR ((gpointer*)&lowest_heap_address, (gpointer)low, (gpointer)old) != (gpointer)old);

	do {
		old = highest_heap_address;
		if (high <= old)
			break;
	} while (SGEN_CAS_PTR ((gpointer*)&highest_heap_address, (gpointer)high, (gpointer)old) != (gpointer)old);
}

/*
 * Allocate and setup the data structures needed to be able to allocate objects
 * in the nursery. The nursery is stored in nursery_section.
 */
static void
alloc_nursery (void)
{
	GCMemSection *section;
	char *data;
	size_t scan_starts;
	size_t alloc_size;

	if (nursery_section)
		return;
	SGEN_LOG (2, "Allocating nursery size: %zu", (size_t)sgen_nursery_size);
	/* later we will alloc a larger area for the nursery but only activate
	 * what we need. The rest will be used as expansion if we have too many pinned
	 * objects in the existing nursery.
	 */
	/* FIXME: handle OOM */
	section = sgen_alloc_internal (INTERNAL_MEM_SECTION);

	alloc_size = sgen_nursery_size;

	/* If there isn't enough space even for the nursery we should simply abort. */
	g_assert (sgen_memgov_try_alloc_space (alloc_size, SPACE_NURSERY));

#ifdef SGEN_ALIGN_NURSERY
	data = major_collector.alloc_heap (alloc_size, alloc_size, DEFAULT_NURSERY_BITS);
#else
	data = major_collector.alloc_heap (alloc_size, 0, DEFAULT_NURSERY_BITS);
#endif
	sgen_update_heap_boundaries ((mword)data, (mword)(data + sgen_nursery_size));
	SGEN_LOG (4, "Expanding nursery size (%p-%p): %lu, total: %lu", data, data + alloc_size, (unsigned long)sgen_nursery_size, (unsigned long)mono_gc_get_heap_size ());
	section->data = section->next_data = data;
	section->size = alloc_size;
	section->end_data = data + sgen_nursery_size;
	scan_starts = (alloc_size + SCAN_START_SIZE - 1) / SCAN_START_SIZE;
	section->scan_starts = sgen_alloc_internal_dynamic (sizeof (char*) * scan_starts, INTERNAL_MEM_SCAN_STARTS, TRUE);
	section->num_scan_start = scan_starts;

	nursery_section = section;

	sgen_nursery_allocator_set_nursery_bounds (data, data + sgen_nursery_size);
}

void*
mono_gc_get_nursery (int *shift_bits, size_t *size)
{
	*size = sgen_nursery_size;
#ifdef SGEN_ALIGN_NURSERY
	*shift_bits = DEFAULT_NURSERY_BITS;
#else
	*shift_bits = -1;
#endif
	return sgen_get_nursery_start ();
}

void
mono_gc_set_current_thread_appdomain (MonoDomain *domain)
{
	SgenThreadInfo *info = mono_thread_info_current ();

	/* Could be called from sgen_thread_unregister () with a NULL info */
	if (domain) {
		g_assert (info);
		info->stopped_domain = domain;
	}
}

gboolean
mono_gc_precise_stack_mark_enabled (void)
{
	return !conservative_stack_mark;
}

FILE *
mono_gc_get_logfile (void)
{
	return gc_debug_file;
}

static void
report_finalizer_roots_list (FinalizeReadyEntry *list)
{
	GCRootReport report;
	FinalizeReadyEntry *fin;

	report.count = 0;
	for (fin = list; fin; fin = fin->next) {
		if (!fin->object)
			continue;
		add_profile_gc_root (&report, fin->object, MONO_PROFILE_GC_ROOT_FINALIZER, 0);
	}
	notify_gc_roots (&report);
}

static void
report_finalizer_roots (void)
{
	report_finalizer_roots_list (fin_ready_list);
	report_finalizer_roots_list (critical_fin_list);
}

static GCRootReport *root_report;

static void
single_arg_report_root (void **obj, void *gc_data)
{
	if (*obj)
		add_profile_gc_root (root_report, *obj, MONO_PROFILE_GC_ROOT_OTHER, 0);
}

static void
precisely_report_roots_from (GCRootReport *report, void** start_root, void** end_root, mword desc)
{
	switch (desc & ROOT_DESC_TYPE_MASK) {
	case ROOT_DESC_BITMAP:
		desc >>= ROOT_DESC_TYPE_SHIFT;
		while (desc) {
			if ((desc & 1) && *start_root) {
				add_profile_gc_root (report, *start_root, MONO_PROFILE_GC_ROOT_OTHER, 0);
			}
			desc >>= 1;
			start_root++;
		}
		return;
	case ROOT_DESC_COMPLEX: {
		gsize *bitmap_data = sgen_get_complex_descriptor_bitmap (desc);
		gsize bwords = (*bitmap_data) - 1;
		void **start_run = start_root;
		bitmap_data++;
		while (bwords-- > 0) {
			gsize bmap = *bitmap_data++;
			void **objptr = start_run;
			while (bmap) {
				if ((bmap & 1) && *objptr) {
					add_profile_gc_root (report, *objptr, MONO_PROFILE_GC_ROOT_OTHER, 0);
				}
				bmap >>= 1;
				++objptr;
			}
			start_run += GC_BITS_PER_WORD;
		}
		break;
	}
	case ROOT_DESC_USER: {
		MonoGCRootMarkFunc marker = sgen_get_user_descriptor_func (desc);
		root_report = report;
		marker (start_root, single_arg_report_root, NULL);
		break;
	}
	case ROOT_DESC_RUN_LEN:
		g_assert_not_reached ();
	default:
		g_assert_not_reached ();
	}
}

static void
report_registered_roots_by_type (int root_type)
{
	GCRootReport report;
	void **start_root;
	RootRecord *root;
	report.count = 0;
	SGEN_HASH_TABLE_FOREACH (&roots_hash [root_type], start_root, root) {
		SGEN_LOG (6, "Precise root scan %p-%p (desc: %p)", start_root, root->end_root, (void*)root->root_desc);
		precisely_report_roots_from (&report, start_root, (void**)root->end_root, root->root_desc);
	} SGEN_HASH_TABLE_FOREACH_END;
	notify_gc_roots (&report);
}

static void
report_registered_roots (void)
{
	report_registered_roots_by_type (ROOT_TYPE_NORMAL);
	report_registered_roots_by_type (ROOT_TYPE_WBARRIER);
}

static void
scan_finalizer_entries (FinalizeReadyEntry *list, ScanCopyContext ctx)
{
	CopyOrMarkObjectFunc copy_func = ctx.copy_func;
	SgenGrayQueue *queue = ctx.queue;
	FinalizeReadyEntry *fin;

	for (fin = list; fin; fin = fin->next) {
		if (!fin->object)
			continue;
		SGEN_LOG (5, "Scan of fin ready object: %p (%s)\n", fin->object, safe_name (fin->object));
		copy_func (&fin->object, queue);
	}
}

static const char*
generation_name (int generation)
{
	switch (generation) {
	case GENERATION_NURSERY: return "nursery";
	case GENERATION_OLD: return "old";
	default: g_assert_not_reached ();
	}
}

const char*
sgen_generation_name (int generation)
{
	return generation_name (generation);
}

SgenObjectOperations *
sgen_get_current_object_ops (void){
	return &current_object_ops;
}


static void
finish_gray_stack (int generation, GrayQueue *queue)
{
	TV_DECLARE (atv);
	TV_DECLARE (btv);
	int done_with_ephemerons, ephemeron_rounds = 0;
	CopyOrMarkObjectFunc copy_func = current_object_ops.copy_or_mark_object;
	ScanObjectFunc scan_func = current_object_ops.scan_object;
	ScanCopyContext ctx = { scan_func, copy_func, queue };
	char *start_addr = generation == GENERATION_NURSERY ? sgen_get_nursery_start () : NULL;
	char *end_addr = generation == GENERATION_NURSERY ? sgen_get_nursery_end () : (char*)-1;

	/*
	 * We copied all the reachable objects. Now it's the time to copy
	 * the objects that were not referenced by the roots, but by the copied objects.
	 * we built a stack of objects pointed to by gray_start: they are
	 * additional roots and we may add more items as we go.
	 * We loop until gray_start == gray_objects which means no more objects have
	 * been added. Note this is iterative: no recursion is involved.
	 * We need to walk the LO list as well in search of marked big objects
	 * (use a flag since this is needed only on major collections). We need to loop
	 * here as well, so keep a counter of marked LO (increasing it in copy_object).
	 *   To achieve better cache locality and cache usage, we drain the gray stack 
	 * frequently, after each object is copied, and just finish the work here.
	 */
	sgen_drain_gray_stack (-1, ctx);
	TV_GETTIME (atv);
	SGEN_LOG (2, "%s generation done", generation_name (generation));

	/*
	Reset bridge data, we might have lingering data from a previous collection if this is a major
	collection trigged by minor overflow.

	We must reset the gathered bridges since their original block might be evacuated due to major
	fragmentation in the meanwhile and the bridge code should not have to deal with that.
	*/
	if (sgen_need_bridge_processing ())
		sgen_bridge_reset_data ();

	/*
	 * Walk the ephemeron tables marking all values with reachable keys. This must be completely done
	 * before processing finalizable objects and non-tracking weak links to avoid finalizing/clearing
	 * objects that are in fact reachable.
	 */
	done_with_ephemerons = 0;
	do {
		done_with_ephemerons = mark_ephemerons_in_range (ctx);
		sgen_drain_gray_stack (-1, ctx);
		++ephemeron_rounds;
	} while (!done_with_ephemerons);

	sgen_mark_togglerefs (start_addr, end_addr, ctx);

	if (sgen_need_bridge_processing ()) {
		/*Make sure the gray stack is empty before we process bridge objects so we get liveness right*/
		sgen_drain_gray_stack (-1, ctx);
		sgen_collect_bridge_objects (generation, ctx);
		if (generation == GENERATION_OLD)
			sgen_collect_bridge_objects (GENERATION_NURSERY, ctx);

		/*
		Do the first bridge step here, as the collector liveness state will become useless after that.

		An important optimization is to only proccess the possibly dead part of the object graph and skip
		over all live objects as we transitively know everything they point must be alive too.

		The above invariant is completely wrong if we let the gray queue be drained and mark/copy everything.

		This has the unfortunate side effect of making overflow collections perform the first step twice, but
		given we now have heuristics that perform major GC in anticipation of minor overflows this should not
		be a big deal.
		*/
		sgen_bridge_processing_stw_step ();
	}

	/*
	Make sure we drain the gray stack before processing disappearing links and finalizers.
	If we don't make sure it is empty we might wrongly see a live object as dead.
	*/
	sgen_drain_gray_stack (-1, ctx);

	/*
	We must clear weak links that don't track resurrection before processing object ready for
	finalization so they can be cleared before that.
	*/
	sgen_null_link_in_range (generation, TRUE, ctx);
	if (generation == GENERATION_OLD)
		sgen_null_link_in_range (GENERATION_NURSERY, TRUE, ctx);


	/* walk the finalization queue and move also the objects that need to be
	 * finalized: use the finalized objects as new roots so the objects they depend
	 * on are also not reclaimed. As with the roots above, only objects in the nursery
	 * are marked/copied.
	 */
	sgen_finalize_in_range (generation, ctx);
	if (generation == GENERATION_OLD)
		sgen_finalize_in_range (GENERATION_NURSERY, ctx);
	/* drain the new stack that might have been created */
	SGEN_LOG (6, "Precise scan of gray area post fin");
	sgen_drain_gray_stack (-1, ctx);

	/*
	 * This must be done again after processing finalizable objects since CWL slots are cleared only after the key is finalized.
	 */
	done_with_ephemerons = 0;
	do {
		done_with_ephemerons = mark_ephemerons_in_range (ctx);
		sgen_drain_gray_stack (-1, ctx);
		++ephemeron_rounds;
	} while (!done_with_ephemerons);

	/*
	 * Clear ephemeron pairs with unreachable keys.
	 * We pass the copy func so we can figure out if an array was promoted or not.
	 */
	clear_unreachable_ephemerons (ctx);

	/*
	 * We clear togglerefs only after all possible chances of revival are done. 
	 * This is semantically more inline with what users expect and it allows for
	 * user finalizers to correctly interact with TR objects.
	*/
	sgen_clear_togglerefs (start_addr, end_addr, ctx);

	TV_GETTIME (btv);
	SGEN_LOG (2, "Finalize queue handling scan for %s generation: %d usecs %d ephemeron rounds", generation_name (generation), TV_ELAPSED (atv, btv), ephemeron_rounds);

	/*
	 * handle disappearing links
	 * Note we do this after checking the finalization queue because if an object
	 * survives (at least long enough to be finalized) we don't clear the link.
	 * This also deals with a possible issue with the monitor reclamation: with the Boehm
	 * GC a finalized object my lose the monitor because it is cleared before the finalizer is
	 * called.
	 */
	g_assert (sgen_gray_object_queue_is_empty (queue));
	for (;;) {
		sgen_null_link_in_range (generation, FALSE, ctx);
		if (generation == GENERATION_OLD)
			sgen_null_link_in_range (GENERATION_NURSERY, FALSE, ctx);
		if (sgen_gray_object_queue_is_empty (queue))
			break;
		sgen_drain_gray_stack (-1, ctx);
	}

	g_assert (sgen_gray_object_queue_is_empty (queue));
}

void
sgen_check_section_scan_starts (GCMemSection *section)
{
	size_t i;
	for (i = 0; i < section->num_scan_start; ++i) {
		if (section->scan_starts [i]) {
			mword size = safe_object_get_size ((MonoObject*) section->scan_starts [i]);
			g_assert (size >= sizeof (MonoObject) && size <= MAX_SMALL_OBJ_SIZE);
		}
	}
}

static void
check_scan_starts (void)
{
	if (!do_scan_starts_check)
		return;
	sgen_check_section_scan_starts (nursery_section);
	major_collector.check_scan_starts ();
}

static void
scan_from_registered_roots (char *addr_start, char *addr_end, int root_type, ScanCopyContext ctx)
{
	void **start_root;
	RootRecord *root;
	SGEN_HASH_TABLE_FOREACH (&roots_hash [root_type], start_root, root) {
		SGEN_LOG (6, "Precise root scan %p-%p (desc: %p)", start_root, root->end_root, (void*)root->root_desc);
		precisely_scan_objects_from (start_root, (void**)root->end_root, addr_start, addr_end, root->root_desc, ctx);
	} SGEN_HASH_TABLE_FOREACH_END;
}

void
sgen_dump_occupied (char *start, char *end, char *section_start)
{
	fprintf (heap_dump_file, "<occupied offset=\"%td\" size=\"%td\"/>\n", start - section_start, end - start);
}

void
sgen_dump_section (GCMemSection *section, const char *type)
{
	char *start = section->data;
	char *end = section->data + section->size;
	char *occ_start = NULL;
	GCVTable *vt;
	char *old_start = NULL;	/* just for debugging */

	fprintf (heap_dump_file, "<section type=\"%s\" size=\"%lu\">\n", type, (unsigned long)section->size);

	while (start < end) {
		guint size;
		MonoClass *class;

		if (!*(void**)start) {
			if (occ_start) {
				sgen_dump_occupied (occ_start, start, section->data);
				occ_start = NULL;
			}
			start += sizeof (void*); /* should be ALLOC_ALIGN, really */
			continue;
		}
		g_assert (start < section->next_data);

		if (!occ_start)
			occ_start = start;

		vt = (GCVTable*)LOAD_VTABLE (start);
		class = vt->klass;

		size = ALIGN_UP (safe_object_get_size ((MonoObject*) start));

		/*
		fprintf (heap_dump_file, "<object offset=\"%d\" class=\"%s.%s\" size=\"%d\"/>\n",
				start - section->data,
				vt->klass->name_space, vt->klass->name,
				size);
		*/

		old_start = start;
		start += size;
	}
	if (occ_start)
		sgen_dump_occupied (occ_start, start, section->data);

	fprintf (heap_dump_file, "</section>\n");
}

static void
dump_object (MonoObject *obj, gboolean dump_location)
{
	static char class_name [1024];

	MonoClass *class = mono_object_class (obj);
	int i, j;

	/*
	 * Python's XML parser is too stupid to parse angle brackets
	 * in strings, so we just ignore them;
	 */
	i = j = 0;
	while (class->name [i] && j < sizeof (class_name) - 1) {
		if (!strchr ("<>\"", class->name [i]))
			class_name [j++] = class->name [i];
		++i;
	}
	g_assert (j < sizeof (class_name));
	class_name [j] = 0;

	fprintf (heap_dump_file, "<object class=\"%s.%s\" size=\"%zd\"",
			class->name_space, class_name,
			safe_object_get_size (obj));
	if (dump_location) {
		const char *location;
		if (ptr_in_nursery (obj))
			location = "nursery";
		else if (safe_object_get_size (obj) <= MAX_SMALL_OBJ_SIZE)
			location = "major";
		else
			location = "LOS";
		fprintf (heap_dump_file, " location=\"%s\"", location);
	}
	fprintf (heap_dump_file, "/>\n");
}

static void
dump_heap (const char *type, int num, const char *reason)
{
	ObjectList *list;
	LOSObject *bigobj;

	fprintf (heap_dump_file, "<collection type=\"%s\" num=\"%d\"", type, num);
	if (reason)
		fprintf (heap_dump_file, " reason=\"%s\"", reason);
	fprintf (heap_dump_file, ">\n");
	fprintf (heap_dump_file, "<other-mem-usage type=\"mempools\" size=\"%ld\"/>\n", mono_mempool_get_bytes_allocated ());
	sgen_dump_internal_mem_usage (heap_dump_file);
	fprintf (heap_dump_file, "<pinned type=\"stack\" bytes=\"%zu\"/>\n", sgen_pin_stats_get_pinned_byte_count (PIN_TYPE_STACK));
	/* fprintf (heap_dump_file, "<pinned type=\"static-data\" bytes=\"%d\"/>\n", pinned_byte_counts [PIN_TYPE_STATIC_DATA]); */
	fprintf (heap_dump_file, "<pinned type=\"other\" bytes=\"%zu\"/>\n", sgen_pin_stats_get_pinned_byte_count (PIN_TYPE_OTHER));

	fprintf (heap_dump_file, "<pinned-objects>\n");
	for (list = sgen_pin_stats_get_object_list (); list; list = list->next)
		dump_object (list->obj, TRUE);
	fprintf (heap_dump_file, "</pinned-objects>\n");

	sgen_dump_section (nursery_section, "nursery");

	major_collector.dump_heap (heap_dump_file);

	fprintf (heap_dump_file, "<los>\n");
	for (bigobj = los_object_list; bigobj; bigobj = bigobj->next)
		dump_object ((MonoObject*)bigobj->data, FALSE);
	fprintf (heap_dump_file, "</los>\n");

	fprintf (heap_dump_file, "</collection>\n");
}

void
sgen_register_moved_object (void *obj, void *destination)
{
	g_assert (mono_profiler_events & MONO_PROFILE_GC_MOVES);

	/* FIXME: handle this for parallel collector */
	g_assert (!sgen_collection_is_parallel ());

	if (moved_objects_idx == MOVED_OBJECTS_NUM) {
		mono_profiler_gc_moves (moved_objects, moved_objects_idx);
		moved_objects_idx = 0;
	}
	moved_objects [moved_objects_idx++] = obj;
	moved_objects [moved_objects_idx++] = destination;
}

static void
init_stats (void)
{
	static gboolean inited = FALSE;

	if (inited)
		return;

	mono_counters_register ("Minor fragment clear", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_minor_pre_collection_fragment_clear);
	mono_counters_register ("Minor pinning", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_minor_pinning);
	mono_counters_register ("Minor scan remembered set", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_minor_scan_remsets);
	mono_counters_register ("Minor scan pinned", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_minor_scan_pinned);
	mono_counters_register ("Minor scan registered roots", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_minor_scan_registered_roots);
	mono_counters_register ("Minor scan thread data", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_minor_scan_thread_data);
	mono_counters_register ("Minor finish gray stack", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_minor_finish_gray_stack);
	mono_counters_register ("Minor fragment creation", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_minor_fragment_creation);

	mono_counters_register ("Major fragment clear", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_major_pre_collection_fragment_clear);
	mono_counters_register ("Major pinning", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_major_pinning);
	mono_counters_register ("Major scan pinned", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_major_scan_pinned);
	mono_counters_register ("Major scan registered roots", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_major_scan_registered_roots);
	mono_counters_register ("Major scan thread data", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_major_scan_thread_data);
	mono_counters_register ("Major scan alloc_pinned", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_major_scan_alloc_pinned);
	mono_counters_register ("Major scan finalized", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_major_scan_finalized);
	mono_counters_register ("Major scan big objects", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_major_scan_big_objects);
	mono_counters_register ("Major finish gray stack", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_major_finish_gray_stack);
	mono_counters_register ("Major free big objects", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_major_free_bigobjs);
	mono_counters_register ("Major LOS sweep", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_major_los_sweep);
	mono_counters_register ("Major sweep", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_major_sweep);
	mono_counters_register ("Major fragment creation", MONO_COUNTER_GC | MONO_COUNTER_LONG | MONO_COUNTER_TIME, &time_major_fragment_creation);

	mono_counters_register ("Number of pinned objects", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_pinned_objects);

#ifdef HEAVY_STATISTICS
	mono_counters_register ("WBarrier remember pointer", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_add_to_global_remset);
	mono_counters_register ("WBarrier set field", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_set_field);
	mono_counters_register ("WBarrier set arrayref", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_set_arrayref);
	mono_counters_register ("WBarrier arrayref copy", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_arrayref_copy);
	mono_counters_register ("WBarrier generic store called", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_generic_store);
	mono_counters_register ("WBarrier generic atomic store called", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_generic_store_atomic);
	mono_counters_register ("WBarrier set root", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_set_root);
	mono_counters_register ("WBarrier value copy", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_value_copy);
	mono_counters_register ("WBarrier object copy", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_object_copy);

	mono_counters_register ("# objects allocated degraded", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_objects_alloced_degraded);
	mono_counters_register ("bytes allocated degraded", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_bytes_alloced_degraded);

	mono_counters_register ("# copy_object() called (nursery)", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_copy_object_called_nursery);
	mono_counters_register ("# objects copied (nursery)", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_objects_copied_nursery);
	mono_counters_register ("# copy_object() called (major)", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_copy_object_called_major);
	mono_counters_register ("# objects copied (major)", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_objects_copied_major);

	mono_counters_register ("# scan_object() called (nursery)", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_scan_object_called_nursery);
	mono_counters_register ("# scan_object() called (major)", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_scan_object_called_major);

	mono_counters_register ("Slots allocated in vain", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_slots_allocated_in_vain);

	mono_counters_register ("# nursery copy_object() failed from space", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_nursery_copy_object_failed_from_space);
	mono_counters_register ("# nursery copy_object() failed forwarded", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_nursery_copy_object_failed_forwarded);
	mono_counters_register ("# nursery copy_object() failed pinned", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_nursery_copy_object_failed_pinned);
	mono_counters_register ("# nursery copy_object() failed to space", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_nursery_copy_object_failed_to_space);

	sgen_nursery_allocator_init_heavy_stats ();
	sgen_alloc_init_heavy_stats ();
#endif

	inited = TRUE;
}


static void
reset_pinned_from_failed_allocation (void)
{
	bytes_pinned_from_failed_allocation = 0;
}

void
sgen_set_pinned_from_failed_allocation (mword objsize)
{
	bytes_pinned_from_failed_allocation += objsize;
}

gboolean
sgen_collection_is_parallel (void)
{
	switch (current_collection_generation) {
	case GENERATION_NURSERY:
		return nursery_collection_is_parallel;
	case GENERATION_OLD:
		return major_collector.is_parallel;
	default:
		g_error ("Invalid current generation %d", current_collection_generation);
	}
}

gboolean
sgen_collection_is_concurrent (void)
{
	switch (current_collection_generation) {
	case GENERATION_NURSERY:
		return FALSE;
	case GENERATION_OLD:
		return concurrent_collection_in_progress;
	default:
		g_error ("Invalid current generation %d", current_collection_generation);
	}
}

gboolean
sgen_concurrent_collection_in_progress (void)
{
	return concurrent_collection_in_progress;
}

typedef struct
{
	char *heap_start;
	char *heap_end;
} FinishRememberedSetScanJobData;

static void
job_finish_remembered_set_scan (WorkerData *worker_data, void *job_data_untyped)
{
	FinishRememberedSetScanJobData *job_data = job_data_untyped;

	remset.finish_scan_remsets (job_data->heap_start, job_data->heap_end, sgen_workers_get_job_gray_queue (worker_data));
	sgen_free_internal_dynamic (job_data, sizeof (FinishRememberedSetScanJobData), INTERNAL_MEM_WORKER_JOB_DATA);
}

typedef struct
{
	CopyOrMarkObjectFunc copy_or_mark_func;
	ScanObjectFunc scan_func;
	char *heap_start;
	char *heap_end;
	int root_type;
} ScanFromRegisteredRootsJobData;

static void
job_scan_from_registered_roots (WorkerData *worker_data, void *job_data_untyped)
{
	ScanFromRegisteredRootsJobData *job_data = job_data_untyped;
	ScanCopyContext ctx = { job_data->scan_func, job_data->copy_or_mark_func,
		sgen_workers_get_job_gray_queue (worker_data) };

	scan_from_registered_roots (job_data->heap_start, job_data->heap_end, job_data->root_type, ctx);
	sgen_free_internal_dynamic (job_data, sizeof (ScanFromRegisteredRootsJobData), INTERNAL_MEM_WORKER_JOB_DATA);
}

typedef struct
{
	char *heap_start;
	char *heap_end;
} ScanThreadDataJobData;

static void
job_scan_thread_data (WorkerData *worker_data, void *job_data_untyped)
{
	ScanThreadDataJobData *job_data = job_data_untyped;

	scan_thread_data (job_data->heap_start, job_data->heap_end, TRUE,
			sgen_workers_get_job_gray_queue (worker_data));
	sgen_free_internal_dynamic (job_data, sizeof (ScanThreadDataJobData), INTERNAL_MEM_WORKER_JOB_DATA);
}

typedef struct
{
	FinalizeReadyEntry *list;
} ScanFinalizerEntriesJobData;

static void
job_scan_finalizer_entries (WorkerData *worker_data, void *job_data_untyped)
{
	ScanFinalizerEntriesJobData *job_data = job_data_untyped;
	ScanCopyContext ctx = { NULL, current_object_ops.copy_or_mark_object, sgen_workers_get_job_gray_queue (worker_data) };

	scan_finalizer_entries (job_data->list, ctx);
	sgen_free_internal_dynamic (job_data, sizeof (ScanFinalizerEntriesJobData), INTERNAL_MEM_WORKER_JOB_DATA);
}

static void
job_scan_major_mod_union_cardtable (WorkerData *worker_data, void *job_data_untyped)
{
	g_assert (concurrent_collection_in_progress);
	major_collector.scan_card_table (TRUE, sgen_workers_get_job_gray_queue (worker_data));
}

static void
job_scan_los_mod_union_cardtable (WorkerData *worker_data, void *job_data_untyped)
{
	g_assert (concurrent_collection_in_progress);
	sgen_los_scan_card_table (TRUE, sgen_workers_get_job_gray_queue (worker_data));
}

static void
verify_scan_starts (char *start, char *end)
{
	size_t i;

	for (i = 0; i < nursery_section->num_scan_start; ++i) {
		char *addr = nursery_section->scan_starts [i];
		if (addr > start && addr < end)
			SGEN_LOG (1, "NFC-BAD SCAN START [%zu] %p for obj [%p %p]", i, addr, start, end);
	}
}

static void
verify_nursery (void)
{
	char *start, *end, *cur, *hole_start;

	if (!do_verify_nursery)
		return;

	/*This cleans up unused fragments */
	sgen_nursery_allocator_prepare_for_pinning ();

	hole_start = start = cur = sgen_get_nursery_start ();
	end = sgen_get_nursery_end ();

	while (cur < end) {
		size_t ss, size;

		if (!*(void**)cur) {
			cur += sizeof (void*);
			continue;
		}

		if (object_is_forwarded (cur))
			SGEN_LOG (1, "FORWARDED OBJ %p", cur);
		else if (object_is_pinned (cur))
			SGEN_LOG (1, "PINNED OBJ %p", cur);

		ss = safe_object_get_size ((MonoObject*)cur);
		size = ALIGN_UP (safe_object_get_size ((MonoObject*)cur));
		verify_scan_starts (cur, cur + size);
		if (do_dump_nursery_content) {
			if (cur > hole_start)
				SGEN_LOG (1, "HOLE [%p %p %d]", hole_start, cur, (int)(cur - hole_start));
			SGEN_LOG (1, "OBJ  [%p %p %d %d %s %d]", cur, cur + size, (int)size, (int)ss, sgen_safe_name ((MonoObject*)cur), (gpointer)LOAD_VTABLE (cur) == sgen_get_array_fill_vtable ());
		}
		cur += size;
		hole_start = cur;
	}
}

/*
 * Checks that no objects in the nursery are fowarded or pinned.  This
 * is a precondition to restarting the mutator while doing a
 * concurrent collection.  Note that we don't clear fragments because
 * we depend on that having happened earlier.
 */
static void
check_nursery_is_clean (void)
{
	char *start, *end, *cur;

	start = cur = sgen_get_nursery_start ();
	end = sgen_get_nursery_end ();

	while (cur < end) {
		size_t ss, size;

		if (!*(void**)cur) {
			cur += sizeof (void*);
			continue;
		}

		g_assert (!object_is_forwarded (cur));
		g_assert (!object_is_pinned (cur));

		ss = safe_object_get_size ((MonoObject*)cur);
		size = ALIGN_UP (safe_object_get_size ((MonoObject*)cur));
		verify_scan_starts (cur, cur + size);

		cur += size;
	}
}

static void
init_gray_queue (void)
{
	if (sgen_collection_is_parallel () || sgen_collection_is_concurrent ()) {
		sgen_workers_init_distribute_gray_queue ();
		sgen_gray_object_queue_init_with_alloc_prepare (&gray_queue, NULL,
				gray_queue_redirect, sgen_workers_get_distribute_section_gray_queue ());
	} else {
		sgen_gray_object_queue_init (&gray_queue, NULL);
	}
}

static void
pin_stage_object_callback (char *obj, size_t size, void *data)
{
	sgen_pin_stage_ptr (obj);
	/* FIXME: do pin stats if enabled */
}

/*
 * Collect objects in the nursery.  Returns whether to trigger a major
 * collection.
 */
static gboolean
collect_nursery (SgenGrayQueue *unpin_queue, gboolean finish_up_concurrent_mark)
{
	gboolean needs_major;
	size_t max_garbage_amount;
	char *nursery_next;
	FinishRememberedSetScanJobData *frssjd;
	ScanFromRegisteredRootsJobData *scrrjd_normal, *scrrjd_wbarrier;
	ScanFinalizerEntriesJobData *sfejd_fin_ready, *sfejd_critical_fin;
	ScanThreadDataJobData *stdjd;
	mword fragment_total;
	ScanCopyContext ctx;
	TV_DECLARE (all_atv);
	TV_DECLARE (all_btv);
	TV_DECLARE (atv);
	TV_DECLARE (btv);

	if (disable_minor_collections)
		return TRUE;

	MONO_GC_BEGIN (GENERATION_NURSERY);
	binary_protocol_collection_begin (gc_stats.minor_gc_count, GENERATION_NURSERY);

	verify_nursery ();

#ifndef DISABLE_PERFCOUNTERS
	mono_perfcounters->gc_collections0++;
#endif

	current_collection_generation = GENERATION_NURSERY;
	if (sgen_collection_is_parallel ())
		current_object_ops = sgen_minor_collector.parallel_ops;
	else
		current_object_ops = sgen_minor_collector.serial_ops;
	
	reset_pinned_from_failed_allocation ();

	check_scan_starts ();

	sgen_nursery_alloc_prepare_for_minor ();

	degraded_mode = 0;
	objects_pinned = 0;
	nursery_next = sgen_nursery_alloc_get_upper_alloc_bound ();
	/* FIXME: optimize later to use the higher address where an object can be present */
	nursery_next = MAX (nursery_next, sgen_get_nursery_end ());

	SGEN_LOG (1, "Start nursery collection %d %p-%p, size: %d", gc_stats.minor_gc_count, sgen_get_nursery_start (), nursery_next, (int)(nursery_next - sgen_get_nursery_start ()));
	max_garbage_amount = nursery_next - sgen_get_nursery_start ();
	g_assert (nursery_section->size >= max_garbage_amount);

	/* world must be stopped already */
	TV_GETTIME (all_atv);
	atv = all_atv;

	TV_GETTIME (btv);
	time_minor_pre_collection_fragment_clear += TV_ELAPSED (atv, btv);

	if (xdomain_checks) {
		sgen_clear_nursery_fragments ();
		sgen_check_for_xdomain_refs ();
	}

	nursery_section->next_data = nursery_next;

	major_collector.start_nursery_collection ();

	sgen_memgov_minor_collection_start ();

	init_gray_queue ();

	gc_stats.minor_gc_count ++;

	MONO_GC_CHECKPOINT_1 (GENERATION_NURSERY);

	sgen_process_fin_stage_entries ();
	sgen_process_dislink_stage_entries ();

	MONO_GC_CHECKPOINT_2 (GENERATION_NURSERY);

	/* pin from pinned handles */
	sgen_init_pinning ();
	mono_profiler_gc_event (MONO_GC_EVENT_MARK_START, 0);
	pin_from_roots (sgen_get_nursery_start (), nursery_next, WORKERS_DISTRIBUTE_GRAY_QUEUE);
	/* pin cemented objects */
	sgen_cement_iterate (pin_stage_object_callback, NULL);
	/* identify pinned objects */
	sgen_optimize_pin_queue (0);
	sgen_pinning_setup_section (nursery_section);
	ctx.scan_func = NULL;
	ctx.copy_func = NULL;
	ctx.queue = WORKERS_DISTRIBUTE_GRAY_QUEUE;
	sgen_pin_objects_in_section (nursery_section, ctx);
	sgen_pinning_trim_queue_to_section (nursery_section);

	TV_GETTIME (atv);
	time_minor_pinning += TV_ELAPSED (btv, atv);
	SGEN_LOG (2, "Finding pinned pointers: %zd in %d usecs", sgen_get_pinned_count (), TV_ELAPSED (btv, atv));
	SGEN_LOG (4, "Start scan with %zd pinned objects", sgen_get_pinned_count ());

	MONO_GC_CHECKPOINT_3 (GENERATION_NURSERY);

	if (whole_heap_check_before_collection) {
		sgen_clear_nursery_fragments ();
		sgen_check_whole_heap (finish_up_concurrent_mark);
	}
	if (consistency_check_at_minor_collection)
		sgen_check_consistency ();

	sgen_workers_start_all_workers ();
	sgen_workers_start_marking ();

	frssjd = sgen_alloc_internal_dynamic (sizeof (FinishRememberedSetScanJobData), INTERNAL_MEM_WORKER_JOB_DATA, TRUE);
	frssjd->heap_start = sgen_get_nursery_start ();
	frssjd->heap_end = nursery_next;
	sgen_workers_enqueue_job (job_finish_remembered_set_scan, frssjd);

	/* we don't have complete write barrier yet, so we scan all the old generation sections */
	TV_GETTIME (btv);
	time_minor_scan_remsets += TV_ELAPSED (atv, btv);
	SGEN_LOG (2, "Old generation scan: %d usecs", TV_ELAPSED (atv, btv));

	MONO_GC_CHECKPOINT_4 (GENERATION_NURSERY);

	if (!sgen_collection_is_parallel ()) {
		ctx.scan_func = current_object_ops.scan_object;
		ctx.copy_func = NULL;
		ctx.queue = &gray_queue;
		sgen_drain_gray_stack (-1, ctx);
	}

	if (mono_profiler_get_events () & MONO_PROFILE_GC_ROOTS)
		report_registered_roots ();
	if (mono_profiler_get_events () & MONO_PROFILE_GC_ROOTS)
		report_finalizer_roots ();
	TV_GETTIME (atv);
	time_minor_scan_pinned += TV_ELAPSED (btv, atv);

	MONO_GC_CHECKPOINT_5 (GENERATION_NURSERY);

	/* registered roots, this includes static fields */
	scrrjd_normal = sgen_alloc_internal_dynamic (sizeof (ScanFromRegisteredRootsJobData), INTERNAL_MEM_WORKER_JOB_DATA, TRUE);
	scrrjd_normal->copy_or_mark_func = current_object_ops.copy_or_mark_object;
	scrrjd_normal->scan_func = current_object_ops.scan_object;
	scrrjd_normal->heap_start = sgen_get_nursery_start ();
	scrrjd_normal->heap_end = nursery_next;
	scrrjd_normal->root_type = ROOT_TYPE_NORMAL;
	sgen_workers_enqueue_job (job_scan_from_registered_roots, scrrjd_normal);

	scrrjd_wbarrier = sgen_alloc_internal_dynamic (sizeof (ScanFromRegisteredRootsJobData), INTERNAL_MEM_WORKER_JOB_DATA, TRUE);
	scrrjd_wbarrier->copy_or_mark_func = current_object_ops.copy_or_mark_object;
	scrrjd_wbarrier->scan_func = current_object_ops.scan_object;
	scrrjd_wbarrier->heap_start = sgen_get_nursery_start ();
	scrrjd_wbarrier->heap_end = nursery_next;
	scrrjd_wbarrier->root_type = ROOT_TYPE_WBARRIER;
	sgen_workers_enqueue_job (job_scan_from_registered_roots, scrrjd_wbarrier);

	TV_GETTIME (btv);
	time_minor_scan_registered_roots += TV_ELAPSED (atv, btv);

	MONO_GC_CHECKPOINT_6 (GENERATION_NURSERY);

	/* thread data */
	stdjd = sgen_alloc_internal_dynamic (sizeof (ScanThreadDataJobData), INTERNAL_MEM_WORKER_JOB_DATA, TRUE);
	stdjd->heap_start = sgen_get_nursery_start ();
	stdjd->heap_end = nursery_next;
	sgen_workers_enqueue_job (job_scan_thread_data, stdjd);

	TV_GETTIME (atv);
	time_minor_scan_thread_data += TV_ELAPSED (btv, atv);
	btv = atv;

	MONO_GC_CHECKPOINT_7 (GENERATION_NURSERY);

	g_assert (!sgen_collection_is_parallel () && !sgen_collection_is_concurrent ());

	if (sgen_collection_is_parallel () || sgen_collection_is_concurrent ())
		g_assert (sgen_gray_object_queue_is_empty (&gray_queue));

	/* Scan the list of objects ready for finalization. If */
	sfejd_fin_ready = sgen_alloc_internal_dynamic (sizeof (ScanFinalizerEntriesJobData), INTERNAL_MEM_WORKER_JOB_DATA, TRUE);
	sfejd_fin_ready->list = fin_ready_list;
	sgen_workers_enqueue_job (job_scan_finalizer_entries, sfejd_fin_ready);

	sfejd_critical_fin = sgen_alloc_internal_dynamic (sizeof (ScanFinalizerEntriesJobData), INTERNAL_MEM_WORKER_JOB_DATA, TRUE);
	sfejd_critical_fin->list = critical_fin_list;
	sgen_workers_enqueue_job (job_scan_finalizer_entries, sfejd_critical_fin);

	MONO_GC_CHECKPOINT_8 (GENERATION_NURSERY);

	finish_gray_stack (GENERATION_NURSERY, &gray_queue);
	TV_GETTIME (atv);
	time_minor_finish_gray_stack += TV_ELAPSED (btv, atv);
	mono_profiler_gc_event (MONO_GC_EVENT_MARK_END, 0);

	MONO_GC_CHECKPOINT_9 (GENERATION_NURSERY);

	/*
	 * The (single-threaded) finalization code might have done
	 * some copying/marking so we can only reset the GC thread's
	 * worker data here instead of earlier when we joined the
	 * workers.
	 */
	sgen_workers_reset_data ();

	if (objects_pinned) {
		sgen_optimize_pin_queue (0);
		sgen_pinning_setup_section (nursery_section);
	}

	/* walk the pin_queue, build up the fragment list of free memory, unmark
	 * pinned objects as we go, memzero() the empty fragments so they are ready for the
	 * next allocations.
	 */
	mono_profiler_gc_event (MONO_GC_EVENT_RECLAIM_START, 0);
	fragment_total = sgen_build_nursery_fragments (nursery_section,
			nursery_section->pin_queue_start, nursery_section->pin_queue_num_entries,
			unpin_queue);
	if (!fragment_total)
		degraded_mode = 1;

	/* Clear TLABs for all threads */
	sgen_clear_tlabs ();

	mono_profiler_gc_event (MONO_GC_EVENT_RECLAIM_END, 0);
	TV_GETTIME (btv);
	time_minor_fragment_creation += TV_ELAPSED (atv, btv);
	SGEN_LOG (2, "Fragment creation: %d usecs, %lu bytes available", TV_ELAPSED (atv, btv), (unsigned long)fragment_total);

	if (consistency_check_at_minor_collection)
		sgen_check_major_refs ();

	major_collector.finish_nursery_collection ();

	TV_GETTIME (all_btv);
	gc_stats.minor_gc_time += TV_ELAPSED (all_atv, all_btv);

	if (heap_dump_file)
		dump_heap ("minor", gc_stats.minor_gc_count - 1, NULL);

	/* prepare the pin queue for the next collection */
	sgen_finish_pinning ();
	if (fin_ready_list || critical_fin_list) {
		SGEN_LOG (4, "Finalizer-thread wakeup: ready %d", num_ready_finalizers);
		mono_gc_finalize_notify ();
	}
	sgen_pin_stats_reset ();
	/* clear cemented hash */
	sgen_cement_clear_below_threshold ();

	g_assert (sgen_gray_object_queue_is_empty (&gray_queue));

	remset.finish_minor_collection ();

	check_scan_starts ();

	binary_protocol_flush_buffers (FALSE);

	sgen_memgov_minor_collection_end ();

	/*objects are late pinned because of lack of memory, so a major is a good call*/
	needs_major = objects_pinned > 0;
	current_collection_generation = -1;
	objects_pinned = 0;

	MONO_GC_END (GENERATION_NURSERY);
	binary_protocol_collection_end (gc_stats.minor_gc_count - 1, GENERATION_NURSERY);

	if (check_nursery_objects_pinned && !sgen_minor_collector.is_split)
		sgen_check_nursery_objects_pinned (unpin_queue != NULL);

	return needs_major;
}

static void
scan_nursery_objects_callback (char *obj, size_t size, ScanCopyContext *ctx)
{
	ctx->scan_func (obj, ctx->queue);
}

static void
scan_nursery_objects (ScanCopyContext ctx)
{
	sgen_scan_area_with_callback (nursery_section->data, nursery_section->end_data,
			(IterateObjectCallbackFunc)scan_nursery_objects_callback, (void*)&ctx, FALSE);
}

static void
major_copy_or_mark_from_roots (size_t *old_next_pin_slot, gboolean finish_up_concurrent_mark, gboolean scan_mod_union)
{
	LOSObject *bigobj;
	TV_DECLARE (atv);
	TV_DECLARE (btv);
	/* FIXME: only use these values for the precise scan
	 * note that to_space pointers should be excluded anyway...
	 */
	char *heap_start = NULL;
	char *heap_end = (char*)-1;
	gboolean profile_roots = mono_profiler_get_events () & MONO_PROFILE_GC_ROOTS;
	GCRootReport root_report = { 0 };
	ScanFromRegisteredRootsJobData *scrrjd_normal, *scrrjd_wbarrier;
	ScanThreadDataJobData *stdjd;
	ScanFinalizerEntriesJobData *sfejd_fin_ready, *sfejd_critical_fin;
	ScanCopyContext ctx;

	if (concurrent_collection_in_progress) {
		/*This cleans up unused fragments */
		sgen_nursery_allocator_prepare_for_pinning ();

		if (do_concurrent_checks)
			check_nursery_is_clean ();
	} else {
		/* The concurrent collector doesn't touch the nursery. */
		sgen_nursery_alloc_prepare_for_major ();
	}

	init_gray_queue ();

	TV_GETTIME (atv);

	/* Pinning depends on this */
	sgen_clear_nursery_fragments ();

	if (whole_heap_check_before_collection)
		sgen_check_whole_heap (finish_up_concurrent_mark);

	TV_GETTIME (btv);
	time_major_pre_collection_fragment_clear += TV_ELAPSED (atv, btv);

	if (!sgen_collection_is_concurrent ())
		nursery_section->next_data = sgen_get_nursery_end ();
	/* we should also coalesce scanning from sections close to each other
	 * and deal with pointers outside of the sections later.
	 */

	objects_pinned = 0;
	*major_collector.have_swept = FALSE;

	if (xdomain_checks) {
		sgen_clear_nursery_fragments ();
		sgen_check_for_xdomain_refs ();
	}

	if (!concurrent_collection_in_progress) {
		/* Remsets are not useful for a major collection */
		remset.prepare_for_major_collection ();
	}

	sgen_process_fin_stage_entries ();
	sgen_process_dislink_stage_entries ();

	TV_GETTIME (atv);
	sgen_init_pinning ();
	SGEN_LOG (6, "Collecting pinned addresses");
	pin_from_roots ((void*)lowest_heap_address, (void*)highest_heap_address, WORKERS_DISTRIBUTE_GRAY_QUEUE);

	if (!concurrent_collection_in_progress || finish_up_concurrent_mark) {
		if (major_collector.is_concurrent) {
			/*
			 * The concurrent major collector cannot evict
			 * yet, so we need to pin cemented objects to
			 * not break some asserts.
			 *
			 * FIXME: We could evict now!
			 */
			sgen_cement_iterate (pin_stage_object_callback, NULL);
		}

		if (!concurrent_collection_in_progress)
			sgen_cement_reset ();
	}

	sgen_optimize_pin_queue (0);

	/*
	 * The concurrent collector doesn't move objects, neither on
	 * the major heap nor in the nursery, so we can mark even
	 * before pinning has finished.  For the non-concurrent
	 * collector we start the workers after pinning.
	 */
	if (concurrent_collection_in_progress) {
		sgen_workers_start_all_workers ();
		sgen_workers_start_marking ();
	}

	/*
	 * pin_queue now contains all candidate pointers, sorted and
	 * uniqued.  We must do two passes now to figure out which
	 * objects are pinned.
	 *
	 * The first is to find within the pin_queue the area for each
	 * section.  This requires that the pin_queue be sorted.  We
	 * also process the LOS objects and pinned chunks here.
	 *
	 * The second, destructive, pass is to reduce the section
	 * areas to pointers to the actually pinned objects.
	 */
	SGEN_LOG (6, "Pinning from sections");
	/* first pass for the sections */
	sgen_find_section_pin_queue_start_end (nursery_section);
	major_collector.find_pin_queue_start_ends (WORKERS_DISTRIBUTE_GRAY_QUEUE);
	/* identify possible pointers to the insize of large objects */
	SGEN_LOG (6, "Pinning from large objects");
	for (bigobj = los_object_list; bigobj; bigobj = bigobj->next) {
		size_t dummy;
		if (sgen_find_optimized_pin_queue_area (bigobj->data, (char*)bigobj->data + sgen_los_object_size (bigobj), &dummy)) {
			binary_protocol_pin (bigobj->data, (gpointer)LOAD_VTABLE (bigobj->data), safe_object_get_size (((MonoObject*)(bigobj->data))));

#ifdef ENABLE_DTRACE
			if (G_UNLIKELY (MONO_GC_OBJ_PINNED_ENABLED ())) {
				MonoVTable *vt = (MonoVTable*)LOAD_VTABLE (bigobj->data);
				MONO_GC_OBJ_PINNED ((mword)bigobj->data, sgen_safe_object_get_size ((MonoObject*)bigobj->data), vt->klass->name_space, vt->klass->name, GENERATION_OLD);
			}
#endif

			if (sgen_los_object_is_pinned (bigobj->data)) {
				g_assert (finish_up_concurrent_mark);
				continue;
			}
			sgen_los_pin_object (bigobj->data);
			if (SGEN_OBJECT_HAS_REFERENCES (bigobj->data))
				GRAY_OBJECT_ENQUEUE (WORKERS_DISTRIBUTE_GRAY_QUEUE, bigobj->data);
			if (G_UNLIKELY (do_pin_stats))
				sgen_pin_stats_register_object ((char*) bigobj->data, safe_object_get_size ((MonoObject*) bigobj->data));
			SGEN_LOG (6, "Marked large object %p (%s) size: %lu from roots", bigobj->data, safe_name (bigobj->data), (unsigned long)sgen_los_object_size (bigobj));

			if (profile_roots)
				add_profile_gc_root (&root_report, bigobj->data, MONO_PROFILE_GC_ROOT_PINNING | MONO_PROFILE_GC_ROOT_MISC, 0);
		}
	}
	if (profile_roots)
		notify_gc_roots (&root_report);
	/* second pass for the sections */
	ctx.scan_func = concurrent_collection_in_progress ? current_object_ops.scan_object : NULL;
	ctx.copy_func = NULL;
	ctx.queue = WORKERS_DISTRIBUTE_GRAY_QUEUE;

	/*
	 * Concurrent mark never follows references into the nursery.
	 * In the start and finish pauses we must scan live nursery
	 * objects, though.  We could simply scan all nursery objects,
	 * but that would be conservative.  The easiest way is to do a
	 * nursery collection, which copies all live nursery objects
	 * (except pinned ones, with the simple nursery) to the major
	 * heap.  Scanning the mod union table later will then scan
	 * those promoted objects, provided they're reachable.  Pinned
	 * objects in the nursery - which we can trivially find in the
	 * pinning queue - are treated as roots in the mark pauses.
	 *
	 * The split nursery complicates the latter part because
	 * non-pinned objects can survive in the nursery.  That's why
	 * we need to do a full front-to-back scan of the nursery,
	 * marking all objects.
	 *
	 * Non-concurrent mark evacuates from the nursery, so it's
	 * sufficient to just scan pinned nursery objects.
	 */
	if (concurrent_collection_in_progress && sgen_minor_collector.is_split) {
		scan_nursery_objects (ctx);
	} else {
		sgen_pin_objects_in_section (nursery_section, ctx);
		if (check_nursery_objects_pinned && !sgen_minor_collector.is_split)
			sgen_check_nursery_objects_pinned (!concurrent_collection_in_progress || finish_up_concurrent_mark);
	}

	major_collector.pin_objects (WORKERS_DISTRIBUTE_GRAY_QUEUE);
	if (old_next_pin_slot)
		*old_next_pin_slot = sgen_get_pinned_count ();

	TV_GETTIME (btv);
	time_major_pinning += TV_ELAPSED (atv, btv);
	SGEN_LOG (2, "Finding pinned pointers: %zd in %d usecs", sgen_get_pinned_count (), TV_ELAPSED (atv, btv));
	SGEN_LOG (4, "Start scan with %zd pinned objects", sgen_get_pinned_count ());

	major_collector.init_to_space ();

#ifdef SGEN_DEBUG_INTERNAL_ALLOC
	main_gc_thread = mono_native_thread_self ();
#endif

	if (!concurrent_collection_in_progress && major_collector.is_parallel) {
		sgen_workers_start_all_workers ();
		sgen_workers_start_marking ();
	}

	if (mono_profiler_get_events () & MONO_PROFILE_GC_ROOTS)
		report_registered_roots ();
	TV_GETTIME (atv);
	time_major_scan_pinned += TV_ELAPSED (btv, atv);

	/* registered roots, this includes static fields */
	scrrjd_normal = sgen_alloc_internal_dynamic (sizeof (ScanFromRegisteredRootsJobData), INTERNAL_MEM_WORKER_JOB_DATA, TRUE);
	scrrjd_normal->copy_or_mark_func = current_object_ops.copy_or_mark_object;
	scrrjd_normal->scan_func = current_object_ops.scan_object;
	scrrjd_normal->heap_start = heap_start;
	scrrjd_normal->heap_end = heap_end;
	scrrjd_normal->root_type = ROOT_TYPE_NORMAL;
	sgen_workers_enqueue_job (job_scan_from_registered_roots, scrrjd_normal);

	scrrjd_wbarrier = sgen_alloc_internal_dynamic (sizeof (ScanFromRegisteredRootsJobData), INTERNAL_MEM_WORKER_JOB_DATA, TRUE);
	scrrjd_wbarrier->copy_or_mark_func = current_object_ops.copy_or_mark_object;
	scrrjd_wbarrier->scan_func = current_object_ops.scan_object;
	scrrjd_wbarrier->heap_start = heap_start;
	scrrjd_wbarrier->heap_end = heap_end;
	scrrjd_wbarrier->root_type = ROOT_TYPE_WBARRIER;
	sgen_workers_enqueue_job (job_scan_from_registered_roots, scrrjd_wbarrier);

	TV_GETTIME (btv);
	time_major_scan_registered_roots += TV_ELAPSED (atv, btv);

	/* Threads */
	stdjd = sgen_alloc_internal_dynamic (sizeof (ScanThreadDataJobData), INTERNAL_MEM_WORKER_JOB_DATA, TRUE);
	stdjd->heap_start = heap_start;
	stdjd->heap_end = heap_end;
	sgen_workers_enqueue_job (job_scan_thread_data, stdjd);

	TV_GETTIME (atv);
	time_major_scan_thread_data += TV_ELAPSED (btv, atv);

	TV_GETTIME (btv);
	time_major_scan_alloc_pinned += TV_ELAPSED (atv, btv);

	if (mono_profiler_get_events () & MONO_PROFILE_GC_ROOTS)
		report_finalizer_roots ();

	/* scan the list of objects ready for finalization */
	sfejd_fin_ready = sgen_alloc_internal_dynamic (sizeof (ScanFinalizerEntriesJobData), INTERNAL_MEM_WORKER_JOB_DATA, TRUE);
	sfejd_fin_ready->list = fin_ready_list;
	sgen_workers_enqueue_job (job_scan_finalizer_entries, sfejd_fin_ready);

	sfejd_critical_fin = sgen_alloc_internal_dynamic (sizeof (ScanFinalizerEntriesJobData), INTERNAL_MEM_WORKER_JOB_DATA, TRUE);
	sfejd_critical_fin->list = critical_fin_list;
	sgen_workers_enqueue_job (job_scan_finalizer_entries, sfejd_critical_fin);

	if (scan_mod_union) {
		g_assert (finish_up_concurrent_mark);

		/* Mod union card table */
		sgen_workers_enqueue_job (job_scan_major_mod_union_cardtable, NULL);
		sgen_workers_enqueue_job (job_scan_los_mod_union_cardtable, NULL);
	}

	TV_GETTIME (atv);
	time_major_scan_finalized += TV_ELAPSED (btv, atv);
	SGEN_LOG (2, "Root scan: %d usecs", TV_ELAPSED (btv, atv));

	TV_GETTIME (btv);
	time_major_scan_big_objects += TV_ELAPSED (atv, btv);

	if (concurrent_collection_in_progress) {
		/* prepare the pin queue for the next collection */
		sgen_finish_pinning ();

		sgen_pin_stats_reset ();

		if (do_concurrent_checks)
			check_nursery_is_clean ();
	}
}

static void
major_start_collection (gboolean concurrent, size_t *old_next_pin_slot)
{
	MONO_GC_BEGIN (GENERATION_OLD);
	binary_protocol_collection_begin (gc_stats.major_gc_count, GENERATION_OLD);

	current_collection_generation = GENERATION_OLD;
#ifndef DISABLE_PERFCOUNTERS
	mono_perfcounters->gc_collections1++;
#endif

	g_assert (sgen_section_gray_queue_is_empty (sgen_workers_get_distribute_section_gray_queue ()));

	if (concurrent) {
		g_assert (major_collector.is_concurrent);
		concurrent_collection_in_progress = TRUE;

		sgen_cement_concurrent_start ();

		current_object_ops = major_collector.major_concurrent_ops;
	} else {
		current_object_ops = major_collector.major_ops;
	}

	reset_pinned_from_failed_allocation ();

	sgen_memgov_major_collection_start ();

	//count_ref_nonref_objs ();
	//consistency_check ();

	check_scan_starts ();

	degraded_mode = 0;
	SGEN_LOG (1, "Start major collection %d", gc_stats.major_gc_count);
	gc_stats.major_gc_count ++;

	if (major_collector.start_major_collection)
		major_collector.start_major_collection ();

	major_copy_or_mark_from_roots (old_next_pin_slot, FALSE, FALSE);
}

static void
wait_for_workers_to_finish (void)
{
	while (!sgen_workers_all_done ())
		g_usleep (200);
}

static void
join_workers (void)
{
	if (concurrent_collection_in_progress || major_collector.is_parallel) {
		gray_queue_redirect (&gray_queue);
		sgen_workers_join ();
	}

	g_assert (sgen_gray_object_queue_is_empty (&gray_queue));

#ifdef SGEN_DEBUG_INTERNAL_ALLOC
	main_gc_thread = NULL;
#endif
}

static void
major_finish_collection (const char *reason, size_t old_next_pin_slot, gboolean scan_mod_union)
{
	LOSObject *bigobj, *prevbo;
	TV_DECLARE (atv);
	TV_DECLARE (btv);

	TV_GETTIME (btv);

	if (concurrent_collection_in_progress || major_collector.is_parallel)
		join_workers ();

	if (concurrent_collection_in_progress) {
		current_object_ops = major_collector.major_concurrent_ops;

		major_copy_or_mark_from_roots (NULL, TRUE, scan_mod_union);
		join_workers ();

		g_assert (sgen_gray_object_queue_is_empty (&gray_queue));

		if (do_concurrent_checks)
			check_nursery_is_clean ();
	} else {
		current_object_ops = major_collector.major_ops;
	}

	/*
	 * The workers have stopped so we need to finish gray queue
	 * work that might result from finalization in the main GC
	 * thread.  Redirection must therefore be turned off.
	 */
	sgen_gray_object_queue_disable_alloc_prepare (&gray_queue);
	g_assert (sgen_section_gray_queue_is_empty (sgen_workers_get_distribute_section_gray_queue ()));

	/* all the objects in the heap */
	finish_gray_stack (GENERATION_OLD, &gray_queue);
	TV_GETTIME (atv);
	time_major_finish_gray_stack += TV_ELAPSED (btv, atv);

	/*
	 * The (single-threaded) finalization code might have done
	 * some copying/marking so we can only reset the GC thread's
	 * worker data here instead of earlier when we joined the
	 * workers.
	 */
	sgen_workers_reset_data ();

	if (objects_pinned) {
		g_assert (!concurrent_collection_in_progress);

		/*This is slow, but we just OOM'd*/
		sgen_pin_queue_clear_discarded_entries (nursery_section, old_next_pin_slot);
		sgen_optimize_pin_queue (0);
		sgen_find_section_pin_queue_start_end (nursery_section);
		objects_pinned = 0;
	}

	reset_heap_boundaries ();
	sgen_update_heap_boundaries ((mword)sgen_get_nursery_start (), (mword)sgen_get_nursery_end ());

	if (check_mark_bits_after_major_collection)
		sgen_check_major_heap_marked ();

	MONO_GC_SWEEP_BEGIN (GENERATION_OLD, !major_collector.sweeps_lazily);

	/* sweep the big objects list */
	prevbo = NULL;
	for (bigobj = los_object_list; bigobj;) {
		g_assert (!object_is_pinned (bigobj->data));
		if (sgen_los_object_is_pinned (bigobj->data)) {
			sgen_los_unpin_object (bigobj->data);
			sgen_update_heap_boundaries ((mword)bigobj->data, (mword)bigobj->data + sgen_los_object_size (bigobj));
		} else {
			LOSObject *to_free;
			/* not referenced anywhere, so we can free it */
			if (prevbo)
				prevbo->next = bigobj->next;
			else
				los_object_list = bigobj->next;
			to_free = bigobj;
			bigobj = bigobj->next;
			sgen_los_free_object (to_free);
			continue;
		}
		prevbo = bigobj;
		bigobj = bigobj->next;
	}

	TV_GETTIME (btv);
	time_major_free_bigobjs += TV_ELAPSED (atv, btv);

	sgen_los_sweep ();

	TV_GETTIME (atv);
	time_major_los_sweep += TV_ELAPSED (btv, atv);

	major_collector.sweep ();

	MONO_GC_SWEEP_END (GENERATION_OLD, !major_collector.sweeps_lazily);

	TV_GETTIME (btv);
	time_major_sweep += TV_ELAPSED (atv, btv);

	if (!concurrent_collection_in_progress) {
		/* walk the pin_queue, build up the fragment list of free memory, unmark
		 * pinned objects as we go, memzero() the empty fragments so they are ready for the
		 * next allocations.
		 */
		if (!sgen_build_nursery_fragments (nursery_section, nursery_section->pin_queue_start, nursery_section->pin_queue_num_entries, NULL))
			degraded_mode = 1;

		/* prepare the pin queue for the next collection */
		sgen_finish_pinning ();

		/* Clear TLABs for all threads */
		sgen_clear_tlabs ();

		sgen_pin_stats_reset ();
	}

	if (concurrent_collection_in_progress)
		sgen_cement_concurrent_finish ();
	sgen_cement_clear_below_threshold ();

	TV_GETTIME (atv);
	time_major_fragment_creation += TV_ELAPSED (btv, atv);

	if (heap_dump_file)
		dump_heap ("major", gc_stats.major_gc_count - 1, reason);

	if (fin_ready_list || critical_fin_list) {
		SGEN_LOG (4, "Finalizer-thread wakeup: ready %d", num_ready_finalizers);
		mono_gc_finalize_notify ();
	}

	g_assert (sgen_gray_object_queue_is_empty (&gray_queue));

	sgen_memgov_major_collection_end ();
	current_collection_generation = -1;

	major_collector.finish_major_collection ();

	g_assert (sgen_section_gray_queue_is_empty (sgen_workers_get_distribute_section_gray_queue ()));

	if (concurrent_collection_in_progress)
		concurrent_collection_in_progress = FALSE;

	check_scan_starts ();

	binary_protocol_flush_buffers (FALSE);

	//consistency_check ();

	MONO_GC_END (GENERATION_OLD);
	binary_protocol_collection_end (gc_stats.major_gc_count - 1, GENERATION_OLD);
}

static gboolean
major_do_collection (const char *reason)
{
	TV_DECLARE (all_atv);
	TV_DECLARE (all_btv);
	size_t old_next_pin_slot;

	if (disable_major_collections)
		return FALSE;

	if (major_collector.get_and_reset_num_major_objects_marked) {
		long long num_marked = major_collector.get_and_reset_num_major_objects_marked ();
		g_assert (!num_marked);
	}

	/* world must be stopped already */
	TV_GETTIME (all_atv);

	major_start_collection (FALSE, &old_next_pin_slot);
	major_finish_collection (reason, old_next_pin_slot, FALSE);

	TV_GETTIME (all_btv);
	gc_stats.major_gc_time += TV_ELAPSED (all_atv, all_btv);

	/* FIXME: also report this to the user, preferably in gc-end. */
	if (major_collector.get_and_reset_num_major_objects_marked)
		major_collector.get_and_reset_num_major_objects_marked ();

	return bytes_pinned_from_failed_allocation > 0;
}

static void
major_start_concurrent_collection (const char *reason)
{
	long long num_objects_marked;

	if (disable_major_collections)
		return;

	num_objects_marked = major_collector.get_and_reset_num_major_objects_marked ();
	g_assert (num_objects_marked == 0);

	MONO_GC_CONCURRENT_START_BEGIN (GENERATION_OLD);
	binary_protocol_concurrent_start ();

	// FIXME: store reason and pass it when finishing
	major_start_collection (TRUE, NULL);

	gray_queue_redirect (&gray_queue);
	sgen_workers_wait_for_jobs ();

	num_objects_marked = major_collector.get_and_reset_num_major_objects_marked ();
	MONO_GC_CONCURRENT_START_END (GENERATION_OLD, num_objects_marked);

	current_collection_generation = -1;
}

static gboolean
major_update_or_finish_concurrent_collection (gboolean force_finish)
{
	SgenGrayQueue unpin_queue;
	memset (&unpin_queue, 0, sizeof (unpin_queue));

	MONO_GC_CONCURRENT_UPDATE_FINISH_BEGIN (GENERATION_OLD, major_collector.get_and_reset_num_major_objects_marked ());
	binary_protocol_concurrent_update_finish ();

	g_assert (sgen_gray_object_queue_is_empty (&gray_queue));

	if (!force_finish && !sgen_workers_all_done ()) {
		major_collector.update_cardtable_mod_union ();
		sgen_los_update_cardtable_mod_union ();

		MONO_GC_CONCURRENT_UPDATE_END (GENERATION_OLD, major_collector.get_and_reset_num_major_objects_marked ());
		return FALSE;
	}

	/*
	 * The major collector can add global remsets which are processed in the finishing
	 * nursery collection, below.  That implies that the workers must have finished
	 * marking before the nursery collection is allowed to run, otherwise we might miss
	 * some remsets.
	 */
	wait_for_workers_to_finish ();

	major_collector.update_cardtable_mod_union ();
	sgen_los_update_cardtable_mod_union ();

	collect_nursery (&unpin_queue, TRUE);

	if (mod_union_consistency_check)
		sgen_check_mod_union_consistency ();

	current_collection_generation = GENERATION_OLD;
	major_finish_collection ("finishing", -1, TRUE);

	if (whole_heap_check_before_collection)
		sgen_check_whole_heap (FALSE);

	unpin_objects_from_queue (&unpin_queue);
	sgen_gray_object_queue_deinit (&unpin_queue);

	MONO_GC_CONCURRENT_FINISH_END (GENERATION_OLD, major_collector.get_and_reset_num_major_objects_marked ());

	current_collection_generation = -1;

	return TRUE;
}

/*
 * Ensure an allocation request for @size will succeed by freeing enough memory.
 *
 * LOCKING: The GC lock MUST be held.
 */
void
sgen_ensure_free_space (size_t size)
{
	int generation_to_collect = -1;
	const char *reason = NULL;


	if (size > SGEN_MAX_SMALL_OBJ_SIZE) {
		if (sgen_need_major_collection (size)) {
			reason = "LOS overflow";
			generation_to_collect = GENERATION_OLD;
		}
	} else {
		if (degraded_mode) {
			if (sgen_need_major_collection (size)) {
				reason = "Degraded mode overflow";
				generation_to_collect = GENERATION_OLD;
			}
		} else if (sgen_need_major_collection (size)) {
			reason = "Minor allowance";
			generation_to_collect = GENERATION_OLD;
		} else {
			generation_to_collect = GENERATION_NURSERY;
			reason = "Nursery full";                        
		}
	}

	if (generation_to_collect == -1) {
		if (concurrent_collection_in_progress && sgen_workers_all_done ()) {
			generation_to_collect = GENERATION_OLD;
			reason = "Finish concurrent collection";
		}
	}

	if (generation_to_collect == -1)
		return;
	sgen_perform_collection (size, generation_to_collect, reason, FALSE);
}

/*
 * LOCKING: Assumes the GC lock is held.
 */
void
sgen_perform_collection (size_t requested_size, int generation_to_collect, const char *reason, gboolean wait_to_finish)
{
	TV_DECLARE (gc_end);
	GGTimingInfo infos [2];
	int overflow_generation_to_collect = -1;
	int oldest_generation_collected = generation_to_collect;
	const char *overflow_reason = NULL;

	MONO_GC_REQUESTED (generation_to_collect, requested_size, wait_to_finish ? 1 : 0);
	if (wait_to_finish)
		binary_protocol_collection_force (generation_to_collect);

	g_assert (generation_to_collect == GENERATION_NURSERY || generation_to_collect == GENERATION_OLD);

	memset (infos, 0, sizeof (infos));
	mono_profiler_gc_event (MONO_GC_EVENT_START, generation_to_collect);

	infos [0].generation = generation_to_collect;
	infos [0].reason = reason;
	infos [0].is_overflow = FALSE;
	TV_GETTIME (infos [0].total_time);
	infos [1].generation = -1;

	sgen_stop_world (generation_to_collect);

	if (concurrent_collection_in_progress) {
		if (major_update_or_finish_concurrent_collection (wait_to_finish && generation_to_collect == GENERATION_OLD)) {
			oldest_generation_collected = GENERATION_OLD;
			goto done;
		}
		if (generation_to_collect == GENERATION_OLD)
			goto done;
	} else {
		if (generation_to_collect == GENERATION_OLD &&
				allow_synchronous_major &&
				major_collector.want_synchronous_collection &&
				*major_collector.want_synchronous_collection) {
			wait_to_finish = TRUE;
		}
	}

	//FIXME extract overflow reason
	if (generation_to_collect == GENERATION_NURSERY) {
		if (collect_nursery (NULL, FALSE)) {
			overflow_generation_to_collect = GENERATION_OLD;
			overflow_reason = "Minor overflow";
		}
	} else {
		if (major_collector.is_concurrent) {
			g_assert (!concurrent_collection_in_progress);
			if (!wait_to_finish)
				collect_nursery (NULL, FALSE);
		}

		if (major_collector.is_concurrent && !wait_to_finish) {
			major_start_concurrent_collection (reason);
			// FIXME: set infos[0] properly
			goto done;
		} else {
			if (major_do_collection (reason)) {
				overflow_generation_to_collect = GENERATION_NURSERY;
				overflow_reason = "Excessive pinning";
			}
		}
	}

	TV_GETTIME (gc_end);
	infos [0].total_time = SGEN_TV_ELAPSED (infos [0].total_time, gc_end);


	if (!major_collector.is_concurrent && overflow_generation_to_collect != -1) {
		mono_profiler_gc_event (MONO_GC_EVENT_START, overflow_generation_to_collect);
		infos [1].generation = overflow_generation_to_collect;
		infos [1].reason = overflow_reason;
		infos [1].is_overflow = TRUE;
		infos [1].total_time = gc_end;

		if (overflow_generation_to_collect == GENERATION_NURSERY)
			collect_nursery (NULL, FALSE);
		else
			major_do_collection (overflow_reason);

		TV_GETTIME (gc_end);
		infos [1].total_time = SGEN_TV_ELAPSED (infos [1].total_time, gc_end);

		/* keep events symmetric */
		mono_profiler_gc_event (MONO_GC_EVENT_END, overflow_generation_to_collect);

		oldest_generation_collected = MAX (oldest_generation_collected, overflow_generation_to_collect);
	}

	SGEN_LOG (2, "Heap size: %lu, LOS size: %lu", (unsigned long)mono_gc_get_heap_size (), (unsigned long)los_memory_usage);

	/* this also sets the proper pointers for the next allocation */
	if (generation_to_collect == GENERATION_NURSERY && !sgen_can_alloc_size (requested_size)) {
		/* TypeBuilder and MonoMethod are killing mcs with fragmentation */
		SGEN_LOG (1, "nursery collection didn't find enough room for %zd alloc (%zd pinned)", requested_size, sgen_get_pinned_count ());
		sgen_dump_pin_queue ();
		degraded_mode = 1;
	}

 done:
	g_assert (sgen_gray_object_queue_is_empty (&gray_queue));

	sgen_restart_world (oldest_generation_collected, infos);

	mono_profiler_gc_event (MONO_GC_EVENT_END, generation_to_collect);
}

/*
 * ######################################################################
 * ########  Memory allocation from the OS
 * ######################################################################
 * This section of code deals with getting memory from the OS and
 * allocating memory for GC-internal data structures.
 * Internal memory can be handled with a freelist for small objects.
 */

/*
 * Debug reporting.
 */
G_GNUC_UNUSED static void
report_internal_mem_usage (void)
{
	printf ("Internal memory usage:\n");
	sgen_report_internal_mem_usage ();
	printf ("Pinned memory usage:\n");
	major_collector.report_pinned_memory_usage ();
}

/*
 * ######################################################################
 * ########  Finalization support
 * ######################################################################
 */

static inline gboolean
sgen_major_is_object_alive (void *object)
{
	mword objsize;

	/* Oldgen objects can be pinned and forwarded too */
	if (SGEN_OBJECT_IS_PINNED (object) || SGEN_OBJECT_IS_FORWARDED (object))
		return TRUE;

	/*
	 * FIXME: major_collector.is_object_live() also calculates the
	 * size.  Avoid the double calculation.
	 */
	objsize = SGEN_ALIGN_UP (sgen_safe_object_get_size ((MonoObject*)object));
	if (objsize > SGEN_MAX_SMALL_OBJ_SIZE)
		return sgen_los_object_is_pinned (object);

	return major_collector.is_object_live (object);
}

/*
 * If the object has been forwarded it means it's still referenced from a root. 
 * If it is pinned it's still alive as well.
 * A LOS object is only alive if we have pinned it.
 * Return TRUE if @obj is ready to be finalized.
 */
static inline gboolean
sgen_is_object_alive (void *object)
{
	if (ptr_in_nursery (object))
		return sgen_nursery_is_object_alive (object);

	return sgen_major_is_object_alive (object);
}

/*
 * This function returns true if @object is either alive or it belongs to the old gen
 * and we're currently doing a minor collection.
 */
static inline int
sgen_is_object_alive_for_current_gen (char *object)
{
	if (ptr_in_nursery (object))
		return sgen_nursery_is_object_alive (object);

	if (current_collection_generation == GENERATION_NURSERY)
		return TRUE;

	return sgen_major_is_object_alive (object);
}

/*
 * This function returns true if @object is either alive and belongs to the
 * current collection - major collections are full heap, so old gen objects
 * are never alive during a minor collection.
 */
static inline int
sgen_is_object_alive_and_on_current_collection (char *object)
{
	if (ptr_in_nursery (object))
		return sgen_nursery_is_object_alive (object);

	if (current_collection_generation == GENERATION_NURSERY)
		return FALSE;

	return sgen_major_is_object_alive (object);
}


gboolean
sgen_gc_is_object_ready_for_finalization (void *object)
{
	return !sgen_is_object_alive (object);
}

static gboolean
has_critical_finalizer (MonoObject *obj)
{
	MonoClass *class;

	if (!mono_defaults.critical_finalizer_object)
		return FALSE;

	class = ((MonoVTable*)LOAD_VTABLE (obj))->klass;

	return mono_class_has_parent_fast (class, mono_defaults.critical_finalizer_object);
}

static gboolean
is_finalization_aware (MonoObject *obj)
{
	MonoVTable *vt = ((MonoVTable*)LOAD_VTABLE (obj));
	return (vt->gc_bits & SGEN_GC_BIT_FINALIZER_AWARE) == SGEN_GC_BIT_FINALIZER_AWARE;
}

void
sgen_queue_finalization_entry (MonoObject *obj)
{
	FinalizeReadyEntry *entry = sgen_alloc_internal (INTERNAL_MEM_FINALIZE_READY_ENTRY);
	gboolean critical = has_critical_finalizer (obj);
	entry->object = obj;
	if (critical) {
		entry->next = critical_fin_list;
		critical_fin_list = entry;
	} else {
		entry->next = fin_ready_list;
		fin_ready_list = entry;
	}

	if (fin_callbacks.object_queued_for_finalization && is_finalization_aware (obj))
		fin_callbacks.object_queued_for_finalization (obj);

#ifdef ENABLE_DTRACE
	if (G_UNLIKELY (MONO_GC_FINALIZE_ENQUEUE_ENABLED ())) {
		int gen = sgen_ptr_in_nursery (obj) ? GENERATION_NURSERY : GENERATION_OLD;
		MonoVTable *vt = (MonoVTable*)LOAD_VTABLE (obj);
		MONO_GC_FINALIZE_ENQUEUE ((mword)obj, sgen_safe_object_get_size (obj),
				vt->klass->name_space, vt->klass->name, gen, critical);
	}
#endif
}

gboolean
sgen_object_is_live (void *obj)
{
	return sgen_is_object_alive_and_on_current_collection (obj);
}

/* LOCKING: requires that the GC lock is held */
static void
null_ephemerons_for_domain (MonoDomain *domain)
{
	EphemeronLinkNode *current = ephemeron_list, *prev = NULL;

	while (current) {
		MonoObject *object = (MonoObject*)current->array;

		if (object && !object->vtable) {
			EphemeronLinkNode *tmp = current;

			if (prev)
				prev->next = current->next;
			else
				ephemeron_list = current->next;

			current = current->next;
			sgen_free_internal (tmp, INTERNAL_MEM_EPHEMERON_LINK);
		} else {
			prev = current;
			current = current->next;
		}
	}
}

/* LOCKING: requires that the GC lock is held */
static void
clear_unreachable_ephemerons (ScanCopyContext ctx)
{
	CopyOrMarkObjectFunc copy_func = ctx.copy_func;
	GrayQueue *queue = ctx.queue;
	EphemeronLinkNode *current = ephemeron_list, *prev = NULL;
	MonoArray *array;
	Ephemeron *cur, *array_end;
	char *tombstone;

	while (current) {
		char *object = current->array;

		if (!sgen_is_object_alive_for_current_gen (object)) {
			EphemeronLinkNode *tmp = current;

			SGEN_LOG (5, "Dead Ephemeron array at %p", object);

			if (prev)
				prev->next = current->next;
			else
				ephemeron_list = current->next;

			current = current->next;
			sgen_free_internal (tmp, INTERNAL_MEM_EPHEMERON_LINK);

			continue;
		}

		copy_func ((void**)&object, queue);
		current->array = object;

		SGEN_LOG (5, "Clearing unreachable entries for ephemeron array at %p", object);

		array = (MonoArray*)object;
		cur = mono_array_addr (array, Ephemeron, 0);
		array_end = cur + mono_array_length_fast (array);
		tombstone = (char*)((MonoVTable*)LOAD_VTABLE (object))->domain->ephemeron_tombstone;

		for (; cur < array_end; ++cur) {
			char *key = (char*)cur->key;

			if (!key || key == tombstone)
				continue;

			SGEN_LOG (5, "[%td] key %p (%s) value %p (%s)", cur - mono_array_addr (array, Ephemeron, 0),
				key, sgen_is_object_alive_for_current_gen (key) ? "reachable" : "unreachable",
				cur->value, cur->value && sgen_is_object_alive_for_current_gen (cur->value) ? "reachable" : "unreachable");

			if (!sgen_is_object_alive_for_current_gen (key)) {
				cur->key = tombstone;
				cur->value = NULL;
				continue;
			}
		}
		prev = current;
		current = current->next;
	}
}

/*
LOCKING: requires that the GC lock is held

Limitations: We scan all ephemerons on every collection since the current design doesn't allow for a simple nursery/mature split.
*/
static int
mark_ephemerons_in_range (ScanCopyContext ctx)
{
	CopyOrMarkObjectFunc copy_func = ctx.copy_func;
	GrayQueue *queue = ctx.queue;
	int nothing_marked = 1;
	EphemeronLinkNode *current = ephemeron_list;
	MonoArray *array;
	Ephemeron *cur, *array_end;
	char *tombstone;

	for (current = ephemeron_list; current; current = current->next) {
		char *object = current->array;
		SGEN_LOG (5, "Ephemeron array at %p", object);

		/*It has to be alive*/
		if (!sgen_is_object_alive_for_current_gen (object)) {
			SGEN_LOG (5, "\tnot reachable");
			continue;
		}

		copy_func ((void**)&object, queue);

		array = (MonoArray*)object;
		cur = mono_array_addr (array, Ephemeron, 0);
		array_end = cur + mono_array_length_fast (array);
		tombstone = (char*)((MonoVTable*)LOAD_VTABLE (object))->domain->ephemeron_tombstone;

		for (; cur < array_end; ++cur) {
			char *key = cur->key;

			if (!key || key == tombstone)
				continue;

			SGEN_LOG (5, "[%td] key %p (%s) value %p (%s)", cur - mono_array_addr (array, Ephemeron, 0),
				key, sgen_is_object_alive_for_current_gen (key) ? "reachable" : "unreachable",
				cur->value, cur->value && sgen_is_object_alive_for_current_gen (cur->value) ? "reachable" : "unreachable");

			if (sgen_is_object_alive_for_current_gen (key)) {
				char *value = cur->value;

				copy_func ((void**)&cur->key, queue);
				if (value) {
					if (!sgen_is_object_alive_for_current_gen (value))
						nothing_marked = 0;
					copy_func ((void**)&cur->value, queue);
				}
			}
		}
	}

	SGEN_LOG (5, "Ephemeron run finished. Is it done %d", nothing_marked);
	return nothing_marked;
}

int
mono_gc_invoke_finalizers (void)
{
	FinalizeReadyEntry *entry = NULL;
	gboolean entry_is_critical = FALSE;
	int count = 0;
	void *obj;
	/* FIXME: batch to reduce lock contention */
	while (fin_ready_list || critical_fin_list) {
		LOCK_GC;

		if (entry) {
			FinalizeReadyEntry **list = entry_is_critical ? &critical_fin_list : &fin_ready_list;

			/* We have finalized entry in the last
			   interation, now we need to remove it from
			   the list. */
			if (*list == entry)
				*list = entry->next;
			else {
				FinalizeReadyEntry *e = *list;
				while (e->next != entry)
					e = e->next;
				e->next = entry->next;
			}
			sgen_free_internal (entry, INTERNAL_MEM_FINALIZE_READY_ENTRY);
			entry = NULL;
		}

		/* Now look for the first non-null entry. */
		for (entry = fin_ready_list; entry && !entry->object; entry = entry->next)
			;
		if (entry) {
			entry_is_critical = FALSE;
		} else {
			entry_is_critical = TRUE;
			for (entry = critical_fin_list; entry && !entry->object; entry = entry->next)
				;
		}

		if (entry) {
			g_assert (entry->object);
			num_ready_finalizers--;
			obj = entry->object;
			entry->object = NULL;
			SGEN_LOG (7, "Finalizing object %p (%s)", obj, safe_name (obj));
		}

		UNLOCK_GC;

		if (!entry)
			break;

		g_assert (entry->object == NULL);
		count++;
		/* the object is on the stack so it is pinned */
		/*g_print ("Calling finalizer for object: %p (%s)\n", entry->object, safe_name (entry->object));*/
		mono_gc_run_finalize (obj, NULL);
	}
	g_assert (!entry);
	return count;
}

gboolean
mono_gc_pending_finalizers (void)
{
	return fin_ready_list || critical_fin_list;
}

/*
 * ######################################################################
 * ########  registered roots support
 * ######################################################################
 */

/*
 * We do not coalesce roots.
 */
static int
mono_gc_register_root_inner (char *start, size_t size, void *descr, int root_type)
{
	RootRecord new_root;
	int i;
	LOCK_GC;
	for (i = 0; i < ROOT_TYPE_NUM; ++i) {
		RootRecord *root = sgen_hash_table_lookup (&roots_hash [i], start);
		/* we allow changing the size and the descriptor (for thread statics etc) */
		if (root) {
			size_t old_size = root->end_root - start;
			root->end_root = start + size;
			g_assert (((root->root_desc != 0) && (descr != NULL)) ||
					  ((root->root_desc == 0) && (descr == NULL)));
			root->root_desc = (mword)descr;
			roots_size += size;
			roots_size -= old_size;
			UNLOCK_GC;
			return TRUE;
		}
	}

	new_root.end_root = start + size;
	new_root.root_desc = (mword)descr;

	sgen_hash_table_replace (&roots_hash [root_type], start, &new_root, NULL);
	roots_size += size;

	SGEN_LOG (3, "Added root for range: %p-%p, descr: %p  (%d/%d bytes)", start, new_root.end_root, descr, (int)size, (int)roots_size);

	UNLOCK_GC;
	return TRUE;
}

int
mono_gc_register_root (char *start, size_t size, void *descr)
{
	return mono_gc_register_root_inner (start, size, descr, descr ? ROOT_TYPE_NORMAL : ROOT_TYPE_PINNED);
}

int
mono_gc_register_root_wbarrier (char *start, size_t size, void *descr)
{
	return mono_gc_register_root_inner (start, size, descr, ROOT_TYPE_WBARRIER);
}

void
mono_gc_deregister_root (char* addr)
{
	int root_type;
	RootRecord root;

	LOCK_GC;
	for (root_type = 0; root_type < ROOT_TYPE_NUM; ++root_type) {
		if (sgen_hash_table_remove (&roots_hash [root_type], addr, &root))
			roots_size -= (root.end_root - addr);
	}
	UNLOCK_GC;
}

/*
 * ######################################################################
 * ########  Thread handling (stop/start code)
 * ######################################################################
 */

unsigned int sgen_global_stop_count = 0;

int
sgen_get_current_collection_generation (void)
{
	return current_collection_generation;
}

void
mono_gc_set_gc_callbacks (MonoGCCallbacks *callbacks)
{
	gc_callbacks = *callbacks;
}

MonoGCCallbacks *
mono_gc_get_gc_callbacks ()
{
	return &gc_callbacks;
}

/* Variables holding start/end nursery so it won't have to be passed at every call */
static void *scan_area_arg_start, *scan_area_arg_end;

void
mono_gc_conservatively_scan_area (void *start, void *end)
{
	conservatively_pin_objects_from (start, end, scan_area_arg_start, scan_area_arg_end, PIN_TYPE_STACK);
}

void*
mono_gc_scan_object (void *obj, void *gc_data)
{
	UserCopyOrMarkData *data = gc_data;
	current_object_ops.copy_or_mark_object (&obj, data->queue);
	return obj;
}

/*
 * Mark from thread stacks and registers.
 */
static void
scan_thread_data (void *start_nursery, void *end_nursery, gboolean precise, GrayQueue *queue)
{
	SgenThreadInfo *info;

	scan_area_arg_start = start_nursery;
	scan_area_arg_end = end_nursery;

	FOREACH_THREAD (info) {
		if (info->skip) {
			SGEN_LOG (3, "Skipping dead thread %p, range: %p-%p, size: %td", info, info->stack_start, info->stack_end, (char*)info->stack_end - (char*)info->stack_start);
			continue;
		}
		if (info->gc_disabled) {
			SGEN_LOG (3, "GC disabled for thread %p, range: %p-%p, size: %td", info, info->stack_start, info->stack_end, (char*)info->stack_end - (char*)info->stack_start);
			continue;
		}
		if (mono_thread_info_run_state (info) != STATE_RUNNING) {
			SGEN_LOG (3, "Skipping non-running thread %p, range: %p-%p, size: %td (state %d)", info, info->stack_start, info->stack_end, (char*)info->stack_end - (char*)info->stack_start, mono_thread_info_run_state (info));
			continue;
		}
		SGEN_LOG (3, "Scanning thread %p, range: %p-%p, size: %td, pinned=%zd", info, info->stack_start, info->stack_end, (char*)info->stack_end - (char*)info->stack_start, sgen_get_pinned_count ());
		if (gc_callbacks.thread_mark_func && !conservative_stack_mark) {
			UserCopyOrMarkData data = { NULL, queue };
			gc_callbacks.thread_mark_func (info->runtime_data, info->stack_start, info->stack_end, precise, &data);
		} else if (!precise) {
			if (!conservative_stack_mark) {
				fprintf (stderr, "Precise stack mark not supported - disabling.\n");
				conservative_stack_mark = TRUE;
			}
			conservatively_pin_objects_from (info->stack_start, info->stack_end, start_nursery, end_nursery, PIN_TYPE_STACK);
		}

		if (!precise) {
#ifdef USE_MONO_CTX
			conservatively_pin_objects_from ((void**)&info->ctx, (void**)&info->ctx + ARCH_NUM_REGS,
				start_nursery, end_nursery, PIN_TYPE_STACK);
#else
			conservatively_pin_objects_from ((void**)&info->regs, (void**)&info->regs + ARCH_NUM_REGS,
					start_nursery, end_nursery, PIN_TYPE_STACK);
#endif
		}
	} END_FOREACH_THREAD
}

static gboolean
ptr_on_stack (void *ptr)
{
	gpointer stack_start = &stack_start;
	SgenThreadInfo *info = mono_thread_info_current ();

	if (ptr >= stack_start && ptr < (gpointer)info->stack_end)
		return TRUE;
	return FALSE;
}

static void*
sgen_thread_register (SgenThreadInfo* info, void *addr)
{
	size_t stsize = 0;
	guint8 *staddr = NULL;

#ifndef HAVE_KW_THREAD
	info->tlab_start = info->tlab_next = info->tlab_temp_end = info->tlab_real_end = NULL;

	g_assert (!mono_native_tls_get_value (thread_info_key));
	mono_native_tls_set_value (thread_info_key, info);
#else
	sgen_thread_info = info;
#endif

#ifdef SGEN_POSIX_STW
	info->stop_count = -1;
	info->signal = 0;
#endif
	info->skip = 0;
	info->stack_start = NULL;
	info->stopped_ip = NULL;
	info->stopped_domain = NULL;
#ifdef USE_MONO_CTX
	memset (&info->ctx, 0, sizeof (MonoContext));
#else
	memset (&info->regs, 0, sizeof (info->regs));
#endif

	sgen_init_tlab_info (info);

	binary_protocol_thread_register ((gpointer)mono_thread_info_get_tid (info));

	/* On win32, stack_start_limit should be 0, since the stack can grow dynamically */
#ifndef HOST_WIN32
	mono_thread_info_get_stack_bounds (&staddr, &stsize);
#endif
	if (staddr) {
		info->stack_start_limit = staddr;
		info->stack_end = staddr + stsize;
	} else {
		gsize stack_bottom = (gsize)addr;
		stack_bottom += 4095;
		stack_bottom &= ~4095;
		info->stack_end = (char*)stack_bottom;
	}

#ifdef HAVE_KW_THREAD
	stack_end = info->stack_end;
#endif

	SGEN_LOG (3, "registered thread %p (%p) stack end %p", info, (gpointer)mono_thread_info_get_tid (info), info->stack_end);

	if (gc_callbacks.thread_attach_func)
		info->runtime_data = gc_callbacks.thread_attach_func ();
	return info;
}

static void
sgen_thread_detach (SgenThreadInfo *p)
{
	/* If a delegate is passed to native code and invoked on a thread we dont
	 * know about, the jit will register it with mono_jit_thread_attach, but
	 * we have no way of knowing when that thread goes away.  SGen has a TSD
	 * so we assume that if the domain is still registered, we can detach
	 * the thread
	 */
	if (mono_domain_get ())
		mono_thread_detach_internal (mono_thread_internal_current ());
}

static void
sgen_thread_unregister (SgenThreadInfo *p)
{
	MonoNativeThreadId tid;

	tid = mono_thread_info_get_tid (p);
	binary_protocol_thread_unregister ((gpointer)tid);
	SGEN_LOG (3, "unregister thread %p (%p)", p, (gpointer)tid);

	if (p->info.runtime_thread)
		mono_threads_add_joinable_thread ((gpointer)tid);

	if (gc_callbacks.thread_detach_func) {
		gc_callbacks.thread_detach_func (p->runtime_data);
		p->runtime_data = NULL;
	}
}


static void
sgen_thread_attach (SgenThreadInfo *info)
{
	LOCK_GC;
	/*this is odd, can we get attached before the gc is inited?*/
	init_stats ();
	UNLOCK_GC;
	
	if (gc_callbacks.thread_attach_func && !info->runtime_data)
		info->runtime_data = gc_callbacks.thread_attach_func ();
}
gboolean
mono_gc_register_thread (void *baseptr)
{
	return mono_thread_info_attach (baseptr) != NULL;
}

/*
 * mono_gc_set_stack_end:
 *
 *   Set the end of the current threads stack to STACK_END. The stack space between 
 * STACK_END and the real end of the threads stack will not be scanned during collections.
 */
void
mono_gc_set_stack_end (void *stack_end)
{
	SgenThreadInfo *info;

	LOCK_GC;
	info = mono_thread_info_current ();
	if (info) {
		g_assert (stack_end < info->stack_end);
		info->stack_end = stack_end;
	}
	UNLOCK_GC;
}

#if USE_PTHREAD_INTERCEPT


int
mono_gc_pthread_create (pthread_t *new_thread, const pthread_attr_t *attr, void *(*start_routine)(void *), void *arg)
{
	return pthread_create (new_thread, attr, start_routine, arg);
}

int
mono_gc_pthread_join (pthread_t thread, void **retval)
{
	return pthread_join (thread, retval);
}

int
mono_gc_pthread_detach (pthread_t thread)
{
	return pthread_detach (thread);
}

void
mono_gc_pthread_exit (void *retval) 
{
	mono_thread_info_detach ();
	pthread_exit (retval);
	g_assert_not_reached ();
}

#endif /* USE_PTHREAD_INTERCEPT */

/*
 * ######################################################################
 * ########  Write barriers
 * ######################################################################
 */

/*
 * Note: the write barriers first do the needed GC work and then do the actual store:
 * this way the value is visible to the conservative GC scan after the write barrier
 * itself. If a GC interrupts the barrier in the middle, value will be kept alive by
 * the conservative scan, otherwise by the remembered set scan.
 */
void
mono_gc_wbarrier_set_field (MonoObject *obj, gpointer field_ptr, MonoObject* value)
{
	HEAVY_STAT (++stat_wbarrier_set_field);
	if (ptr_in_nursery (field_ptr)) {
		*(void**)field_ptr = value;
		return;
	}
	SGEN_LOG (8, "Adding remset at %p", field_ptr);
	if (value)
		binary_protocol_wbarrier (field_ptr, value, value->vtable);

	remset.wbarrier_set_field (obj, field_ptr, value);
}

void
mono_gc_wbarrier_set_arrayref (MonoArray *arr, gpointer slot_ptr, MonoObject* value)
{
	HEAVY_STAT (++stat_wbarrier_set_arrayref);
	if (ptr_in_nursery (slot_ptr)) {
		*(void**)slot_ptr = value;
		return;
	}
	SGEN_LOG (8, "Adding remset at %p", slot_ptr);
	if (value)
		binary_protocol_wbarrier (slot_ptr, value, value->vtable);

	remset.wbarrier_set_arrayref (arr, slot_ptr, value);
}

void
mono_gc_wbarrier_arrayref_copy (gpointer dest_ptr, gpointer src_ptr, int count)
{
	HEAVY_STAT (++stat_wbarrier_arrayref_copy);
	/*This check can be done without taking a lock since dest_ptr array is pinned*/
	if (ptr_in_nursery (dest_ptr) || count <= 0) {
		mono_gc_memmove_aligned (dest_ptr, src_ptr, count * sizeof (gpointer));
		return;
	}

#ifdef SGEN_HEAVY_BINARY_PROTOCOL
	if (binary_protocol_is_heavy_enabled ()) {
		int i;
		for (i = 0; i < count; ++i) {
			gpointer dest = (gpointer*)dest_ptr + i;
			gpointer obj = *((gpointer*)src_ptr + i);
			if (obj)
				binary_protocol_wbarrier (dest, obj, (gpointer)LOAD_VTABLE (obj));
		}
	}
#endif

	remset.wbarrier_arrayref_copy (dest_ptr, src_ptr, count);
}

static char *found_obj;

static void
find_object_for_ptr_callback (char *obj, size_t size, void *user_data)
{
	char *ptr = user_data;

	if (ptr >= obj && ptr < obj + size) {
		g_assert (!found_obj);
		found_obj = obj;
	}
}

/* for use in the debugger */
char* find_object_for_ptr (char *ptr);
char*
find_object_for_ptr (char *ptr)
{
	if (ptr >= nursery_section->data && ptr < nursery_section->end_data) {
		found_obj = NULL;
		sgen_scan_area_with_callback (nursery_section->data, nursery_section->end_data,
				find_object_for_ptr_callback, ptr, TRUE);
		if (found_obj)
			return found_obj;
	}

	found_obj = NULL;
	sgen_los_iterate_objects (find_object_for_ptr_callback, ptr);
	if (found_obj)
		return found_obj;

	/*
	 * Very inefficient, but this is debugging code, supposed to
	 * be called from gdb, so we don't care.
	 */
	found_obj = NULL;
	major_collector.iterate_objects (ITERATE_OBJECTS_SWEEP_ALL, find_object_for_ptr_callback, ptr);
	return found_obj;
}

void
mono_gc_wbarrier_generic_nostore (gpointer ptr)
{
	gpointer obj;

	HEAVY_STAT (++stat_wbarrier_generic_store);

#ifdef XDOMAIN_CHECKS_IN_WBARRIER
	/* FIXME: ptr_in_heap must be called with the GC lock held */
	if (xdomain_checks && *(MonoObject**)ptr && ptr_in_heap (ptr)) {
		char *start = find_object_for_ptr (ptr);
		MonoObject *value = *(MonoObject**)ptr;
		LOCK_GC;
		g_assert (start);
		if (start) {
			MonoObject *obj = (MonoObject*)start;
			if (obj->vtable->domain != value->vtable->domain)
				g_assert (is_xdomain_ref_allowed (ptr, start, obj->vtable->domain));
		}
		UNLOCK_GC;
	}
#endif

	obj = *(gpointer*)ptr;
	if (obj)
		binary_protocol_wbarrier (ptr, obj, (gpointer)LOAD_VTABLE (obj));

	if (ptr_in_nursery (ptr) || ptr_on_stack (ptr)) {
		SGEN_LOG (8, "Skipping remset at %p", ptr);
		return;
	}

	/*
	 * We need to record old->old pointer locations for the
	 * concurrent collector.
	 */
	if (!ptr_in_nursery (obj) && !concurrent_collection_in_progress) {
		SGEN_LOG (8, "Skipping remset at %p", ptr);
		return;
	}

	SGEN_LOG (8, "Adding remset at %p", ptr);

	remset.wbarrier_generic_nostore (ptr);
}

void
mono_gc_wbarrier_generic_store (gpointer ptr, MonoObject* value)
{
	SGEN_LOG (8, "Wbarrier store at %p to %p (%s)", ptr, value, value ? safe_name (value) : "null");
	*(void**)ptr = value;
	if (ptr_in_nursery (value))
		mono_gc_wbarrier_generic_nostore (ptr);
	sgen_dummy_use (value);
}

/* Same as mono_gc_wbarrier_generic_store () but performs the store
 * as an atomic operation with release semantics.
 */
void
mono_gc_wbarrier_generic_store_atomic (gpointer ptr, MonoObject *value)
{
	HEAVY_STAT (++stat_wbarrier_generic_store_atomic);

	SGEN_LOG (8, "Wbarrier atomic store at %p to %p (%s)", ptr, value, value ? safe_name (value) : "null");

	InterlockedWritePointer (ptr, value);

	if (ptr_in_nursery (value))
		mono_gc_wbarrier_generic_nostore (ptr);

	sgen_dummy_use (value);
}

void mono_gc_wbarrier_value_copy_bitmap (gpointer _dest, gpointer _src, int size, unsigned bitmap)
{
	mword *dest = _dest;
	mword *src = _src;

	while (size) {
		if (bitmap & 0x1)
			mono_gc_wbarrier_generic_store (dest, (MonoObject*)*src);
		else
			*dest = *src;
		++src;
		++dest;
		size -= SIZEOF_VOID_P;
		bitmap >>= 1;
	}
}

#ifdef SGEN_HEAVY_BINARY_PROTOCOL
#undef HANDLE_PTR
#define HANDLE_PTR(ptr,obj) do {					\
		gpointer o = *(gpointer*)(ptr);				\
		if ((o)) {						\
			gpointer d = ((char*)dest) + ((char*)(ptr) - (char*)(obj)); \
			binary_protocol_wbarrier (d, o, (gpointer) LOAD_VTABLE (o)); \
		}							\
	} while (0)

static void
scan_object_for_binary_protocol_copy_wbarrier (gpointer dest, char *start, mword desc)
{
#define SCAN_OBJECT_NOVTABLE
#include "sgen-scan-object.h"
}
#endif

void
mono_gc_wbarrier_value_copy (gpointer dest, gpointer src, int count, MonoClass *klass)
{
	HEAVY_STAT (++stat_wbarrier_value_copy);
	g_assert (klass->valuetype);

	SGEN_LOG (8, "Adding value remset at %p, count %d, descr %p for class %s (%p)", dest, count, klass->gc_descr, klass->name, klass);

	if (ptr_in_nursery (dest) || ptr_on_stack (dest) || !SGEN_CLASS_HAS_REFERENCES (klass)) {
		size_t element_size = mono_class_value_size (klass, NULL);
		size_t size = count * element_size;
		mono_gc_memmove_atomic (dest, src, size);		
		return;
	}

#ifdef SGEN_HEAVY_BINARY_PROTOCOL
	if (binary_protocol_is_heavy_enabled ()) {
		size_t element_size = mono_class_value_size (klass, NULL);
		int i;
		for (i = 0; i < count; ++i) {
			scan_object_for_binary_protocol_copy_wbarrier ((char*)dest + i * element_size,
					(char*)src + i * element_size - sizeof (MonoObject),
					(mword) klass->gc_descr);
		}
	}
#endif

	remset.wbarrier_value_copy (dest, src, count, klass);
}

/**
 * mono_gc_wbarrier_object_copy:
 *
 * Write barrier to call when obj is the result of a clone or copy of an object.
 */
void
mono_gc_wbarrier_object_copy (MonoObject* obj, MonoObject *src)
{
	int size;

	HEAVY_STAT (++stat_wbarrier_object_copy);

	if (ptr_in_nursery (obj) || ptr_on_stack (obj)) {
		size = mono_object_class (obj)->instance_size;
		mono_gc_memmove_aligned ((char*)obj + sizeof (MonoObject), (char*)src + sizeof (MonoObject),
				size - sizeof (MonoObject));
		return;	
	}

#ifdef SGEN_HEAVY_BINARY_PROTOCOL
	if (binary_protocol_is_heavy_enabled ())
		scan_object_for_binary_protocol_copy_wbarrier (obj, (char*)src, (mword) src->vtable->gc_descr);
#endif

	remset.wbarrier_object_copy (obj, src);
}


/*
 * ######################################################################
 * ########  Other mono public interface functions.
 * ######################################################################
 */

#define REFS_SIZE 128
typedef struct {
	void *data;
	MonoGCReferences callback;
	int flags;
	int count;
	int called;
	MonoObject *refs [REFS_SIZE];
	uintptr_t offsets [REFS_SIZE];
} HeapWalkInfo;

#undef HANDLE_PTR
#define HANDLE_PTR(ptr,obj)	do {	\
		if (*(ptr)) {	\
			if (hwi->count == REFS_SIZE) {	\
				hwi->callback ((MonoObject*)start, mono_object_class (start), hwi->called? 0: size, hwi->count, hwi->refs, hwi->offsets, hwi->data);	\
				hwi->count = 0;	\
				hwi->called = 1;	\
			}	\
			hwi->offsets [hwi->count] = (char*)(ptr)-(char*)start;	\
			hwi->refs [hwi->count++] = *(ptr);	\
		}	\
	} while (0)

static void
collect_references (HeapWalkInfo *hwi, char *start, size_t size)
{
#include "sgen-scan-object.h"
}

static void
walk_references (char *start, size_t size, void *data)
{
	HeapWalkInfo *hwi = data;
	hwi->called = 0;
	hwi->count = 0;
	collect_references (hwi, start, size);
	if (hwi->count || !hwi->called)
		hwi->callback ((MonoObject*)start, mono_object_class (start), hwi->called? 0: size, hwi->count, hwi->refs, hwi->offsets, hwi->data);
}

/**
 * mono_gc_walk_heap:
 * @flags: flags for future use
 * @callback: a function pointer called for each object in the heap
 * @data: a user data pointer that is passed to callback
 *
 * This function can be used to iterate over all the live objects in the heap:
 * for each object, @callback is invoked, providing info about the object's
 * location in memory, its class, its size and the objects it references.
 * For each referenced object it's offset from the object address is
 * reported in the offsets array.
 * The object references may be buffered, so the callback may be invoked
 * multiple times for the same object: in all but the first call, the size
 * argument will be zero.
 * Note that this function can be only called in the #MONO_GC_EVENT_PRE_START_WORLD
 * profiler event handler.
 *
 * Returns: a non-zero value if the GC doesn't support heap walking
 */
int
mono_gc_walk_heap (int flags, MonoGCReferences callback, void *data)
{
	HeapWalkInfo hwi;

	hwi.flags = flags;
	hwi.callback = callback;
	hwi.data = data;

	sgen_clear_nursery_fragments ();
	sgen_scan_area_with_callback (nursery_section->data, nursery_section->end_data, walk_references, &hwi, FALSE);

	major_collector.iterate_objects (ITERATE_OBJECTS_SWEEP_ALL, walk_references, &hwi);
	sgen_los_iterate_objects (walk_references, &hwi);

	return 0;
}

void
mono_gc_collect (int generation)
{
	LOCK_GC;
	if (generation > 1)
		generation = 1;
	sgen_perform_collection (0, generation, "user request", TRUE);
	UNLOCK_GC;
}

int
mono_gc_max_generation (void)
{
	return 1;
}

int
mono_gc_collection_count (int generation)
{
	if (generation == 0)
		return gc_stats.minor_gc_count;
	return gc_stats.major_gc_count;
}

int64_t
mono_gc_get_used_size (void)
{
	gint64 tot = 0;
	LOCK_GC;
	tot = los_memory_usage;
	tot += nursery_section->next_data - nursery_section->data;
	tot += major_collector.get_used_size ();
	/* FIXME: account for pinned objects */
	UNLOCK_GC;
	return tot;
}

int
mono_gc_get_los_limit (void)
{
	return MAX_SMALL_OBJ_SIZE;
}

gboolean
mono_gc_user_markers_supported (void)
{
	return TRUE;
}

gboolean
mono_object_is_alive (MonoObject* o)
{
	return TRUE;
}

int
mono_gc_get_generation (MonoObject *obj)
{
	if (ptr_in_nursery (obj))
		return 0;
	return 1;
}

void
mono_gc_enable_events (void)
{
}

void
mono_gc_weak_link_add (void **link_addr, MonoObject *obj, gboolean track)
{
	sgen_register_disappearing_link (obj, link_addr, track, FALSE);
}

void
mono_gc_weak_link_remove (void **link_addr, gboolean track)
{
	sgen_register_disappearing_link (NULL, link_addr, track, FALSE);
}

MonoObject*
mono_gc_weak_link_get (void **link_addr)
{
	void * volatile *link_addr_volatile;
	void *ptr;
	MonoObject *obj;
 retry:
	link_addr_volatile = link_addr;
	ptr = (void*)*link_addr_volatile;
	/*
	 * At this point we have a hidden pointer.  If the GC runs
	 * here, it will not recognize the hidden pointer as a
	 * reference, and if the object behind it is not referenced
	 * elsewhere, it will be freed.  Once the world is restarted
	 * we reveal the pointer, giving us a pointer to a freed
	 * object.  To make sure we don't return it, we load the
	 * hidden pointer again.  If it's still the same, we can be
	 * sure the object reference is valid.
	 */
	if (ptr)
		obj = (MonoObject*) REVEAL_POINTER (ptr);
	else
		return NULL;

	mono_memory_barrier ();

	/*
	 * During the second bridge processing step the world is
	 * running again.  That step processes all weak links once
	 * more to null those that refer to dead objects.  Before that
	 * is completed, those links must not be followed, so we
	 * conservatively wait for bridge processing when any weak
	 * link is dereferenced.
	 */
	if (G_UNLIKELY (bridge_processing_in_progress))
		mono_gc_wait_for_bridge_processing ();

	if ((void*)*link_addr_volatile != ptr)
		goto retry;

	return obj;
}

gboolean
mono_gc_ephemeron_array_add (MonoObject *obj)
{
	EphemeronLinkNode *node;

	LOCK_GC;

	node = sgen_alloc_internal (INTERNAL_MEM_EPHEMERON_LINK);
	if (!node) {
		UNLOCK_GC;
		return FALSE;
	}
	node->array = (char*)obj;
	node->next = ephemeron_list;
	ephemeron_list = node;

	SGEN_LOG (5, "Registered ephemeron array %p", obj);

	UNLOCK_GC;
	return TRUE;
}

gboolean
mono_gc_set_allow_synchronous_major (gboolean flag)
{
	if (!major_collector.is_concurrent)
		return flag;

	allow_synchronous_major = flag;
	return TRUE;
}

void*
mono_gc_invoke_with_gc_lock (MonoGCLockedCallbackFunc func, void *data)
{
	void *result;
	LOCK_INTERRUPTION;
	result = func (data);
	UNLOCK_INTERRUPTION;
	return result;
}

gboolean
mono_gc_is_gc_thread (void)
{
	gboolean result;
	LOCK_GC;
	result = mono_thread_info_current () != NULL;
	UNLOCK_GC;
	return result;
}

static gboolean
is_critical_method (MonoMethod *method)
{
	return mono_runtime_is_critical_method (method) || sgen_is_critical_method (method);
}

void
sgen_env_var_error (const char *env_var, const char *fallback, const char *description_format, ...)
{
	va_list ap;

	va_start (ap, description_format);

	fprintf (stderr, "Warning: In environment variable `%s': ", env_var);
	vfprintf (stderr, description_format, ap);
	if (fallback)
		fprintf (stderr, " - %s", fallback);
	fprintf (stderr, "\n");

	va_end (ap);
}

static gboolean
parse_double_in_interval (const char *env_var, const char *opt_name, const char *opt, double min, double max, double *result)
{
	char *endptr;
	double val = strtod (opt, &endptr);
	if (endptr == opt) {
		sgen_env_var_error (env_var, "Using default value.", "`%s` must be a number.", opt_name);
		return FALSE;
	}
	else if (val < min || val > max) {
		sgen_env_var_error (env_var, "Using default value.", "`%s` must be between %.2f - %.2f.", opt_name, min, max);
		return FALSE;
	}
	*result = val;
	return TRUE;
}

void
mono_gc_base_init (void)
{
	MonoThreadInfoCallbacks cb;
	const char *env;
	char **opts, **ptr;
	char *major_collector_opt = NULL;
	char *minor_collector_opt = NULL;
	size_t max_heap = 0;
	size_t soft_limit = 0;
	int num_workers;
	int result;
	int dummy;
	gboolean debug_print_allowance = FALSE;
	double allowance_ratio = 0, save_target = 0;
	gboolean have_split_nursery = FALSE;
	gboolean cement_enabled = TRUE;

	do {
		result = InterlockedCompareExchange (&gc_initialized, -1, 0);
		switch (result) {
		case 1:
			/* already inited */
			return;
		case -1:
			/* being inited by another thread */
			g_usleep (1000);
			break;
		case 0:
			/* we will init it */
			break;
		default:
			g_assert_not_reached ();
		}
	} while (result != 0);

	SGEN_TV_GETTIME (sgen_init_timestamp);

	LOCK_INIT (gc_mutex);

	pagesize = mono_pagesize ();
	gc_debug_file = stderr;

	cb.thread_register = sgen_thread_register;
	cb.thread_detach = sgen_thread_detach;
	cb.thread_unregister = sgen_thread_unregister;
	cb.thread_attach = sgen_thread_attach;
	cb.mono_method_is_critical = (gpointer)is_critical_method;
#ifndef HOST_WIN32
	cb.thread_exit = mono_gc_pthread_exit;
	cb.mono_gc_pthread_create = (gpointer)mono_gc_pthread_create;
#endif

	mono_threads_init (&cb, sizeof (SgenThreadInfo));

	LOCK_INIT (sgen_interruption_mutex);
	LOCK_INIT (pin_queue_mutex);

	if ((env = g_getenv (MONO_GC_PARAMS_NAME))) {
		opts = g_strsplit (env, ",", -1);
		for (ptr = opts; *ptr; ++ptr) {
			char *opt = *ptr;
			if (g_str_has_prefix (opt, "major=")) {
				opt = strchr (opt, '=') + 1;
				major_collector_opt = g_strdup (opt);
			} else if (g_str_has_prefix (opt, "minor=")) {
				opt = strchr (opt, '=') + 1;
				minor_collector_opt = g_strdup (opt);
			}
		}
	} else {
		opts = NULL;
	}

	init_stats ();
	sgen_init_internal_allocator ();
	sgen_init_nursery_allocator ();
	sgen_init_fin_weak_hash ();
	sgen_init_stw ();
	sgen_init_hash_table ();

	sgen_register_fixed_internal_mem_type (INTERNAL_MEM_SECTION, SGEN_SIZEOF_GC_MEM_SECTION);
	sgen_register_fixed_internal_mem_type (INTERNAL_MEM_FINALIZE_READY_ENTRY, sizeof (FinalizeReadyEntry));
	sgen_register_fixed_internal_mem_type (INTERNAL_MEM_GRAY_QUEUE, sizeof (GrayQueueSection));
	sgen_register_fixed_internal_mem_type (INTERNAL_MEM_EPHEMERON_LINK, sizeof (EphemeronLinkNode));

#ifndef HAVE_KW_THREAD
	mono_native_tls_alloc (&thread_info_key, NULL);
#if defined(__APPLE__) || defined (HOST_WIN32)
	/* 
	 * CEE_MONO_TLS requires the tls offset, not the key, so the code below only works on darwin,
	 * where the two are the same.
	 */
	mono_tls_key_set_offset (TLS_KEY_SGEN_THREAD_INFO, thread_info_key);
#endif
#else
	{
		int tls_offset = -1;
		MONO_THREAD_VAR_OFFSET (sgen_thread_info, tls_offset);
		mono_tls_key_set_offset (TLS_KEY_SGEN_THREAD_INFO, tls_offset);
	}
#endif

	/*
	 * This needs to happen before any internal allocations because
	 * it inits the small id which is required for hazard pointer
	 * operations.
	 */
	sgen_os_init ();

	mono_thread_info_attach (&dummy);

	if (!minor_collector_opt) {
		sgen_simple_nursery_init (&sgen_minor_collector);
	} else {
		if (!strcmp (minor_collector_opt, "simple")) {
		use_simple_nursery:
			sgen_simple_nursery_init (&sgen_minor_collector);
		} else if (!strcmp (minor_collector_opt, "split")) {
			sgen_split_nursery_init (&sgen_minor_collector);
			have_split_nursery = TRUE;
		} else {
			sgen_env_var_error (MONO_GC_PARAMS_NAME, "Using `simple` instead.", "Unknown minor collector `%s'.", minor_collector_opt);
			goto use_simple_nursery;
		}
	}

	if (!major_collector_opt || !strcmp (major_collector_opt, "marksweep")) {
	use_marksweep_major:
		sgen_marksweep_init (&major_collector);
	} else if (!major_collector_opt || !strcmp (major_collector_opt, "marksweep-fixed")) {
		sgen_marksweep_fixed_init (&major_collector);
	} else if (!major_collector_opt || !strcmp (major_collector_opt, "marksweep-par")) {
		sgen_marksweep_par_init (&major_collector);
	} else if (!major_collector_opt || !strcmp (major_collector_opt, "marksweep-fixed-par")) {
		sgen_marksweep_fixed_par_init (&major_collector);
	} else if (!major_collector_opt || !strcmp (major_collector_opt, "marksweep-conc")) {
		sgen_marksweep_conc_init (&major_collector);
	} else {
		sgen_env_var_error (MONO_GC_PARAMS_NAME, "Using `marksweep` instead.", "Unknown major collector `%s'.", major_collector_opt);
		goto use_marksweep_major;
	}

	if (have_split_nursery && major_collector.is_parallel) {
		sgen_env_var_error (MONO_GC_PARAMS_NAME, "Disabling split minor collector.", "`minor=split` is not supported with the parallel collector yet.");
		have_split_nursery = FALSE;
	}

	num_workers = mono_cpu_count ();
	g_assert (num_workers > 0);
	if (num_workers > 16)
		num_workers = 16;

	///* Keep this the default for now */
	/* Precise marking is broken on all supported targets. Disable until fixed. */
	conservative_stack_mark = TRUE;

	sgen_nursery_size = DEFAULT_NURSERY_SIZE;

	if (opts) {
		gboolean usage_printed = FALSE;

		for (ptr = opts; *ptr; ++ptr) {
			char *opt = *ptr;
			if (!strcmp (opt, ""))
				continue;
			if (g_str_has_prefix (opt, "major="))
				continue;
			if (g_str_has_prefix (opt, "minor="))
				continue;
			if (g_str_has_prefix (opt, "max-heap-size=")) {
				size_t max_heap_candidate = 0;
				opt = strchr (opt, '=') + 1;
				if (*opt && mono_gc_parse_environment_string_extract_number (opt, &max_heap_candidate)) {
					max_heap = (max_heap_candidate + mono_pagesize () - 1) & ~(size_t)(mono_pagesize () - 1);
					if (max_heap != max_heap_candidate)
						sgen_env_var_error (MONO_GC_PARAMS_NAME, "Rounding up.", "`max-heap-size` size must be a multiple of %d.", mono_pagesize ());
				} else {
					sgen_env_var_error (MONO_GC_PARAMS_NAME, NULL, "`max-heap-size` must be an integer.");
				}
				continue;
			}
			if (g_str_has_prefix (opt, "soft-heap-limit=")) {
				opt = strchr (opt, '=') + 1;
				if (*opt && mono_gc_parse_environment_string_extract_number (opt, &soft_limit)) {
					if (soft_limit <= 0) {
						sgen_env_var_error (MONO_GC_PARAMS_NAME, NULL, "`soft-heap-limit` must be positive.");
						soft_limit = 0;
					}
				} else {
					sgen_env_var_error (MONO_GC_PARAMS_NAME, NULL, "`soft-heap-limit` must be an integer.");
				}
				continue;
			}
			if (g_str_has_prefix (opt, "workers=")) {
				long val;
				char *endptr;
				if (!major_collector.is_parallel) {
					sgen_env_var_error (MONO_GC_PARAMS_NAME, "Ignoring.", "The `workers` option can only be used for parallel collectors.");
					continue;
				}
				opt = strchr (opt, '=') + 1;
				val = strtol (opt, &endptr, 10);
				if (!*opt || *endptr) {
					sgen_env_var_error (MONO_GC_PARAMS_NAME, "Ignoring.", "Cannot parse the `workers` option value.");
					continue;
				}
				if (val <= 0 || val > 16) {
					sgen_env_var_error (MONO_GC_PARAMS_NAME, "Using default value.", "The number of `workers` must be in the range 1 to 16.");
					continue;
				}
				num_workers = (int)val;
				continue;
			}
			if (g_str_has_prefix (opt, "stack-mark=")) {
				opt = strchr (opt, '=') + 1;
				if (!strcmp (opt, "precise")) {
					conservative_stack_mark = FALSE;
				} else if (!strcmp (opt, "conservative")) {
					conservative_stack_mark = TRUE;
				} else {
					sgen_env_var_error (MONO_GC_PARAMS_NAME, conservative_stack_mark ? "Using `conservative`." : "Using `precise`.",
							"Invalid value `%s` for `stack-mark` option, possible values are: `precise`, `conservative`.", opt);
				}
				continue;
			}
			if (g_str_has_prefix (opt, "bridge-implementation=")) {
				opt = strchr (opt, '=') + 1;
				sgen_set_bridge_implementation (opt);
				continue;
			}
			if (g_str_has_prefix (opt, "toggleref-test")) {
				sgen_register_test_toggleref_callback ();
				continue;
			}

#ifdef USER_CONFIG
			if (g_str_has_prefix (opt, "nursery-size=")) {
				size_t val;
				opt = strchr (opt, '=') + 1;
				if (*opt && mono_gc_parse_environment_string_extract_number (opt, &val)) {
#ifdef SGEN_ALIGN_NURSERY
					if ((val & (val - 1))) {
						sgen_env_var_error (MONO_GC_PARAMS_NAME, "Using default value.", "`nursery-size` must be a power of two.");
						continue;
					}

					if (val < SGEN_MAX_NURSERY_WASTE) {
						sgen_env_var_error (MONO_GC_PARAMS_NAME, "Using default value.",
								"`nursery-size` must be at least %d bytes.", SGEN_MAX_NURSERY_WASTE);
						continue;
					}

					sgen_nursery_size = val;
					sgen_nursery_bits = 0;
					while (ONE_P << (++ sgen_nursery_bits) != sgen_nursery_size)
						;
#else
					sgen_nursery_size = val;
#endif
				} else {
					sgen_env_var_error (MONO_GC_PARAMS_NAME, "Using default value.", "`nursery-size` must be an integer.");
					continue;
				}
				continue;
			}
#endif
			if (g_str_has_prefix (opt, "save-target-ratio=")) {
				double val;
				opt = strchr (opt, '=') + 1;
				if (parse_double_in_interval (MONO_GC_PARAMS_NAME, "save-target-ratio", opt,
						SGEN_MIN_SAVE_TARGET_RATIO, SGEN_MAX_SAVE_TARGET_RATIO, &val)) {
					save_target = val;
				}
				continue;
			}
			if (g_str_has_prefix (opt, "default-allowance-ratio=")) {
				double val;
				opt = strchr (opt, '=') + 1;
				if (parse_double_in_interval (MONO_GC_PARAMS_NAME, "default-allowance-ratio", opt,
						SGEN_MIN_ALLOWANCE_NURSERY_SIZE_RATIO, SGEN_MIN_ALLOWANCE_NURSERY_SIZE_RATIO, &val)) {
					allowance_ratio = val;
				}
				continue;
			}
			if (g_str_has_prefix (opt, "allow-synchronous-major=")) {
				if (!major_collector.is_concurrent) {
					sgen_env_var_error (MONO_GC_PARAMS_NAME, "Ignoring.", "`allow-synchronous-major` is only valid for the concurrent major collector.");
					continue;
				}

				opt = strchr (opt, '=') + 1;

				if (!strcmp (opt, "yes")) {
					allow_synchronous_major = TRUE;
				} else if (!strcmp (opt, "no")) {
					allow_synchronous_major = FALSE;
				} else {
					sgen_env_var_error (MONO_GC_PARAMS_NAME, "Using default value.", "`allow-synchronous-major` must be either `yes' or `no'.");
					continue;
				}
			}

			if (!strcmp (opt, "cementing")) {
				if (major_collector.is_parallel) {
					sgen_env_var_error (MONO_GC_PARAMS_NAME, "Ignoring.", "`cementing` is not supported for the parallel major collector.");
					continue;
				}
				cement_enabled = TRUE;
				continue;
			}
			if (!strcmp (opt, "no-cementing")) {
				cement_enabled = FALSE;
				continue;
			}

			if (major_collector.handle_gc_param && major_collector.handle_gc_param (opt))
				continue;

			if (sgen_minor_collector.handle_gc_param && sgen_minor_collector.handle_gc_param (opt))
				continue;

			sgen_env_var_error (MONO_GC_PARAMS_NAME, "Ignoring.", "Unknown option `%s`.", opt);

			if (usage_printed)
				continue;

			fprintf (stderr, "\n%s must be a comma-delimited list of one or more of the following:\n", MONO_GC_PARAMS_NAME);
			fprintf (stderr, "  max-heap-size=N (where N is an integer, possibly with a k, m or a g suffix)\n");
			fprintf (stderr, "  soft-heap-limit=n (where N is an integer, possibly with a k, m or a g suffix)\n");
			fprintf (stderr, "  nursery-size=N (where N is an integer, possibly with a k, m or a g suffix)\n");
			fprintf (stderr, "  major=COLLECTOR (where COLLECTOR is `marksweep', `marksweep-conc', `marksweep-par', 'marksweep-fixed' or 'marksweep-fixed-par')\n");
			fprintf (stderr, "  minor=COLLECTOR (where COLLECTOR is `simple' or `split')\n");
			fprintf (stderr, "  wbarrier=WBARRIER (where WBARRIER is `remset' or `cardtable')\n");
			fprintf (stderr, "  stack-mark=MARK-METHOD (where MARK-METHOD is 'precise' or 'conservative')\n");
			fprintf (stderr, "  [no-]cementing\n");
			if (major_collector.is_concurrent)
				fprintf (stderr, "  allow-synchronous-major=FLAG (where FLAG is `yes' or `no')\n");
			if (major_collector.print_gc_param_usage)
				major_collector.print_gc_param_usage ();
			if (sgen_minor_collector.print_gc_param_usage)
				sgen_minor_collector.print_gc_param_usage ();
			fprintf (stderr, " Experimental options:\n");
			fprintf (stderr, "  save-target-ratio=R (where R must be between %.2f - %.2f).\n", SGEN_MIN_SAVE_TARGET_RATIO, SGEN_MAX_SAVE_TARGET_RATIO);
			fprintf (stderr, "  default-allowance-ratio=R (where R must be between %.2f - %.2f).\n", SGEN_MIN_ALLOWANCE_NURSERY_SIZE_RATIO, SGEN_MAX_ALLOWANCE_NURSERY_SIZE_RATIO);
			fprintf (stderr, "\n");

			usage_printed = TRUE;
		}
		g_strfreev (opts);
	}

	if (major_collector.is_parallel) {
		cement_enabled = FALSE;
		sgen_workers_init (num_workers);
	} else if (major_collector.is_concurrent) {
		sgen_workers_init (1);
	}

	if (major_collector_opt)
		g_free (major_collector_opt);

	if (minor_collector_opt)
		g_free (minor_collector_opt);

	alloc_nursery ();

	sgen_cement_init (cement_enabled);

	if ((env = g_getenv (MONO_GC_DEBUG_NAME))) {
		gboolean usage_printed = FALSE;

		opts = g_strsplit (env, ",", -1);
		for (ptr = opts; ptr && *ptr; ptr ++) {
			char *opt = *ptr;
			if (!strcmp (opt, ""))
				continue;
			if (opt [0] >= '0' && opt [0] <= '9') {
				gc_debug_level = atoi (opt);
				opt++;
				if (opt [0] == ':')
					opt++;
				if (opt [0]) {
#ifdef HOST_WIN32
					char *rf = g_strdup_printf ("%s.%d", opt, GetCurrentProcessId ());
#else
					char *rf = g_strdup_printf ("%s.%d", opt, getpid ());
#endif
					gc_debug_file = fopen (rf, "wb");
					if (!gc_debug_file)
						gc_debug_file = stderr;
					g_free (rf);
				}
			} else if (!strcmp (opt, "print-allowance")) {
				debug_print_allowance = TRUE;
			} else if (!strcmp (opt, "print-pinning")) {
				do_pin_stats = TRUE;
			} else if (!strcmp (opt, "verify-before-allocs")) {
				verify_before_allocs = 1;
				has_per_allocation_action = TRUE;
			} else if (g_str_has_prefix (opt, "verify-before-allocs=")) {
				char *arg = strchr (opt, '=') + 1;
				verify_before_allocs = atoi (arg);
				has_per_allocation_action = TRUE;
			} else if (!strcmp (opt, "collect-before-allocs")) {
				collect_before_allocs = 1;
				has_per_allocation_action = TRUE;
			} else if (g_str_has_prefix (opt, "collect-before-allocs=")) {
				char *arg = strchr (opt, '=') + 1;
				has_per_allocation_action = TRUE;
				collect_before_allocs = atoi (arg);
			} else if (!strcmp (opt, "verify-before-collections")) {
				whole_heap_check_before_collection = TRUE;
			} else if (!strcmp (opt, "check-at-minor-collections")) {
				consistency_check_at_minor_collection = TRUE;
				nursery_clear_policy = CLEAR_AT_GC;
			} else if (!strcmp (opt, "mod-union-consistency-check")) {
				if (!major_collector.is_concurrent) {
					sgen_env_var_error (MONO_GC_DEBUG_NAME, "Ignoring.", "`mod-union-consistency-check` only works with concurrent major collector.");
					continue;
				}
				mod_union_consistency_check = TRUE;
			} else if (!strcmp (opt, "check-mark-bits")) {
				check_mark_bits_after_major_collection = TRUE;
			} else if (!strcmp (opt, "check-nursery-pinned")) {
				check_nursery_objects_pinned = TRUE;
			} else if (!strcmp (opt, "xdomain-checks")) {
				xdomain_checks = TRUE;
			} else if (!strcmp (opt, "clear-at-gc")) {
				nursery_clear_policy = CLEAR_AT_GC;
			} else if (!strcmp (opt, "clear-nursery-at-gc")) {
				nursery_clear_policy = CLEAR_AT_GC;
			} else if (!strcmp (opt, "clear-at-tlab-creation")) {
				nursery_clear_policy = CLEAR_AT_TLAB_CREATION;
			} else if (!strcmp (opt, "debug-clear-at-tlab-creation")) {
				nursery_clear_policy = CLEAR_AT_TLAB_CREATION_DEBUG;
			} else if (!strcmp (opt, "check-scan-starts")) {
				do_scan_starts_check = TRUE;
			} else if (!strcmp (opt, "verify-nursery-at-minor-gc")) {
				do_verify_nursery = TRUE;
			} else if (!strcmp (opt, "check-concurrent")) {
				if (!major_collector.is_concurrent) {
					sgen_env_var_error (MONO_GC_DEBUG_NAME, "Ignoring.", "`check-concurrent` only works with concurrent major collectors.");
					continue;
				}
				do_concurrent_checks = TRUE;
			} else if (!strcmp (opt, "dump-nursery-at-minor-gc")) {
				do_dump_nursery_content = TRUE;
			} else if (!strcmp (opt, "no-managed-allocator")) {
				sgen_set_use_managed_allocator (FALSE);
			} else if (!strcmp (opt, "disable-minor")) {
				disable_minor_collections = TRUE;
			} else if (!strcmp (opt, "disable-major")) {
				disable_major_collections = TRUE;
			} else if (g_str_has_prefix (opt, "heap-dump=")) {
				char *filename = strchr (opt, '=') + 1;
				nursery_clear_policy = CLEAR_AT_GC;
				heap_dump_file = fopen (filename, "w");
				if (heap_dump_file) {
					fprintf (heap_dump_file, "<sgen-dump>\n");
					do_pin_stats = TRUE;
				}
			} else if (g_str_has_prefix (opt, "binary-protocol=")) {
				char *filename = strchr (opt, '=') + 1;
				char *colon = strrchr (filename, ':');
				size_t limit = -1;
				if (colon) {
					if (!mono_gc_parse_environment_string_extract_number (colon + 1, &limit)) {
						sgen_env_var_error (MONO_GC_DEBUG_NAME, "Ignoring limit.", "Binary protocol file size limit must be an integer.");
						limit = -1;
					}
					*colon = '\0';
				}
				binary_protocol_init (filename, (long long)limit);
			} else if (!sgen_bridge_handle_gc_debug (opt)) {
				sgen_env_var_error (MONO_GC_DEBUG_NAME, "Ignoring.", "Unknown option `%s`.", opt);

				if (usage_printed)
					continue;

				fprintf (stderr, "\n%s must be of the format [<l>[:<filename>]|<option>]+ where <l> is a debug level 0-9.\n", MONO_GC_DEBUG_NAME);
				fprintf (stderr, "Valid <option>s are:\n");
				fprintf (stderr, "  collect-before-allocs[=<n>]\n");
				fprintf (stderr, "  verify-before-allocs[=<n>]\n");
				fprintf (stderr, "  check-at-minor-collections\n");
				fprintf (stderr, "  check-mark-bits\n");
				fprintf (stderr, "  check-nursery-pinned\n");
				fprintf (stderr, "  verify-before-collections\n");
				fprintf (stderr, "  verify-nursery-at-minor-gc\n");
				fprintf (stderr, "  dump-nursery-at-minor-gc\n");
				fprintf (stderr, "  disable-minor\n");
				fprintf (stderr, "  disable-major\n");
				fprintf (stderr, "  xdomain-checks\n");
				fprintf (stderr, "  check-concurrent\n");
				fprintf (stderr, "  clear-[nursery-]at-gc\n");
				fprintf (stderr, "  clear-at-tlab-creation\n");
				fprintf (stderr, "  debug-clear-at-tlab-creation\n");
				fprintf (stderr, "  check-scan-starts\n");
				fprintf (stderr, "  no-managed-allocator\n");
				fprintf (stderr, "  print-allowance\n");
				fprintf (stderr, "  print-pinning\n");
				fprintf (stderr, "  heap-dump=<filename>\n");
				fprintf (stderr, "  binary-protocol=<filename>[:<file-size-limit>]\n");
				sgen_bridge_print_gc_debug_usage ();
				fprintf (stderr, "\n");

				usage_printed = TRUE;
			}
		}
		g_strfreev (opts);
	}

	if (major_collector.is_parallel) {
		if (heap_dump_file) {
			sgen_env_var_error (MONO_GC_DEBUG_NAME, "Disabling.", "Cannot do `heap-dump` with the parallel collector.");
			fclose (heap_dump_file);
			heap_dump_file = NULL;
		}
		if (do_pin_stats) {
			sgen_env_var_error (MONO_GC_DEBUG_NAME, "Disabling.", "`print-pinning` is not supported with the parallel collector.");
			do_pin_stats = FALSE;
		}
	}

	if (major_collector.post_param_init)
		major_collector.post_param_init (&major_collector);

	sgen_memgov_init (max_heap, soft_limit, debug_print_allowance, allowance_ratio, save_target);

	memset (&remset, 0, sizeof (remset));

	sgen_card_table_init (&remset);

	gc_initialized = 1;
}

const char *
mono_gc_get_gc_name (void)
{
	return "sgen";
}

static MonoMethod *write_barrier_method;

gboolean
sgen_is_critical_method (MonoMethod *method)
{
	return (method == write_barrier_method || sgen_is_managed_allocator (method));
}

gboolean
sgen_has_critical_method (void)
{
	return write_barrier_method || sgen_has_managed_allocator ();
}

#ifndef DISABLE_JIT

static void
emit_nursery_check (MonoMethodBuilder *mb, int *nursery_check_return_labels)
{
	memset (nursery_check_return_labels, 0, sizeof (int) * 3);
#ifdef SGEN_ALIGN_NURSERY
	// if (ptr_in_nursery (ptr)) return;
	/*
	 * Masking out the bits might be faster, but we would have to use 64 bit
	 * immediates, which might be slower.
	 */
	mono_mb_emit_ldarg (mb, 0);
	mono_mb_emit_icon (mb, DEFAULT_NURSERY_BITS);
	mono_mb_emit_byte (mb, CEE_SHR_UN);
	mono_mb_emit_ptr (mb, (gpointer)((mword)sgen_get_nursery_start () >> DEFAULT_NURSERY_BITS));
	nursery_check_return_labels [0] = mono_mb_emit_branch (mb, CEE_BEQ);

	if (!major_collector.is_concurrent) {
		// if (!ptr_in_nursery (*ptr)) return;
		mono_mb_emit_ldarg (mb, 0);
		mono_mb_emit_byte (mb, CEE_LDIND_I);
		mono_mb_emit_icon (mb, DEFAULT_NURSERY_BITS);
		mono_mb_emit_byte (mb, CEE_SHR_UN);
		mono_mb_emit_ptr (mb, (gpointer)((mword)sgen_get_nursery_start () >> DEFAULT_NURSERY_BITS));
		nursery_check_return_labels [1] = mono_mb_emit_branch (mb, CEE_BNE_UN);
	}
#else
	int label_continue1, label_continue2;
	int dereferenced_var;

	// if (ptr < (sgen_get_nursery_start ())) goto continue;
	mono_mb_emit_ldarg (mb, 0);
	mono_mb_emit_ptr (mb, (gpointer) sgen_get_nursery_start ());
	label_continue_1 = mono_mb_emit_branch (mb, CEE_BLT);

	// if (ptr >= sgen_get_nursery_end ())) goto continue;
	mono_mb_emit_ldarg (mb, 0);
	mono_mb_emit_ptr (mb, (gpointer) sgen_get_nursery_end ());
	label_continue_2 = mono_mb_emit_branch (mb, CEE_BGE);

	// Otherwise return
	nursery_check_return_labels [0] = mono_mb_emit_branch (mb, CEE_BR);

	// continue:
	mono_mb_patch_branch (mb, label_continue_1);
	mono_mb_patch_branch (mb, label_continue_2);

	// Dereference and store in local var
	dereferenced_var = mono_mb_add_local (mb, &mono_defaults.int_class->byval_arg);
	mono_mb_emit_ldarg (mb, 0);
	mono_mb_emit_byte (mb, CEE_LDIND_I);
	mono_mb_emit_stloc (mb, dereferenced_var);

	if (!major_collector.is_concurrent) {
		// if (*ptr < sgen_get_nursery_start ()) return;
		mono_mb_emit_ldloc (mb, dereferenced_var);
		mono_mb_emit_ptr (mb, (gpointer) sgen_get_nursery_start ());
		nursery_check_return_labels [1] = mono_mb_emit_branch (mb, CEE_BLT);

		// if (*ptr >= sgen_get_nursery_end ()) return;
		mono_mb_emit_ldloc (mb, dereferenced_var);
		mono_mb_emit_ptr (mb, (gpointer) sgen_get_nursery_end ());
		nursery_check_return_labels [2] = mono_mb_emit_branch (mb, CEE_BGE);
	}
#endif	
}
#endif

MonoMethod*
mono_gc_get_write_barrier (void)
{
	MonoMethod *res;
	MonoMethodBuilder *mb;
	MonoMethodSignature *sig;
#ifdef MANAGED_WBARRIER
	int i, nursery_check_labels [3];

#ifdef HAVE_KW_THREAD
	int stack_end_offset = -1;

	MONO_THREAD_VAR_OFFSET (stack_end, stack_end_offset);
	g_assert (stack_end_offset != -1);
#endif
#endif

	// FIXME: Maybe create a separate version for ctors (the branch would be
	// correctly predicted more times)
	if (write_barrier_method)
		return write_barrier_method;

	/* Create the IL version of mono_gc_barrier_generic_store () */
	sig = mono_metadata_signature_alloc (mono_defaults.corlib, 1);
	sig->ret = &mono_defaults.void_class->byval_arg;
	sig->params [0] = &mono_defaults.int_class->byval_arg;

	mb = mono_mb_new (mono_defaults.object_class, "wbarrier", MONO_WRAPPER_WRITE_BARRIER);

#ifndef DISABLE_JIT
#ifdef MANAGED_WBARRIER
	emit_nursery_check (mb, nursery_check_labels);
	/*
	addr = sgen_cardtable + ((address >> CARD_BITS) & CARD_MASK)
	*addr = 1;

	sgen_cardtable:
		LDC_PTR sgen_cardtable

	address >> CARD_BITS
		LDARG_0
		LDC_I4 CARD_BITS
		SHR_UN
	if (SGEN_HAVE_OVERLAPPING_CARDS) {
		LDC_PTR card_table_mask
		AND
	}
	AND
	ldc_i4_1
	stind_i1
	*/
	mono_mb_emit_ptr (mb, sgen_cardtable);
	mono_mb_emit_ldarg (mb, 0);
	mono_mb_emit_icon (mb, CARD_BITS);
	mono_mb_emit_byte (mb, CEE_SHR_UN);
#ifdef SGEN_HAVE_OVERLAPPING_CARDS
	mono_mb_emit_ptr (mb, (gpointer)CARD_MASK);
	mono_mb_emit_byte (mb, CEE_AND);
#endif
	mono_mb_emit_byte (mb, CEE_ADD);
	mono_mb_emit_icon (mb, 1);
	mono_mb_emit_byte (mb, CEE_STIND_I1);

	// return;
	for (i = 0; i < 3; ++i) {
		if (nursery_check_labels [i])
			mono_mb_patch_branch (mb, nursery_check_labels [i]);
	}
	mono_mb_emit_byte (mb, CEE_RET);
#else
	mono_mb_emit_ldarg (mb, 0);
	mono_mb_emit_icall (mb, mono_gc_wbarrier_generic_nostore);
	mono_mb_emit_byte (mb, CEE_RET);
#endif
#endif
	res = mono_mb_create_method (mb, sig, 16);
	mono_mb_free (mb);

	LOCK_GC;
	if (write_barrier_method) {
		/* Already created */
		mono_free_method (res);
	} else {
		/* double-checked locking */
		mono_memory_barrier ();
		write_barrier_method = res;
	}
	UNLOCK_GC;

	return write_barrier_method;
}

char*
mono_gc_get_description (void)
{
	return g_strdup ("sgen");
}

void
mono_gc_set_desktop_mode (void)
{
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

#ifdef HOST_WIN32
BOOL APIENTRY mono_gc_dllmain (HMODULE module_handle, DWORD reason, LPVOID reserved)
{
	return TRUE;
}
#endif

NurseryClearPolicy
sgen_get_nursery_clear_policy (void)
{
	return nursery_clear_policy;
}

MonoVTable*
sgen_get_array_fill_vtable (void)
{
	if (!array_fill_vtable) {
		static MonoClass klass;
		static MonoVTable vtable;
		gsize bmap;

		MonoDomain *domain = mono_get_root_domain ();
		g_assert (domain);

		klass.element_class = mono_defaults.byte_class;
		klass.rank = 1;
		klass.instance_size = sizeof (MonoArray);
		klass.sizes.element_size = 1;
		klass.name = "array_filler_type";

		vtable.klass = &klass;
		bmap = 0;
		vtable.gc_descr = mono_gc_make_descr_for_array (TRUE, &bmap, 0, 1);
		vtable.rank = 1;

		array_fill_vtable = &vtable;
	}
	return array_fill_vtable;
}

void
sgen_gc_lock (void)
{
	LOCK_GC;
}

void
sgen_gc_unlock (void)
{
	gboolean try_free = sgen_try_free_some_memory;
	sgen_try_free_some_memory = FALSE;
	mono_mutex_unlock (&gc_mutex);
	MONO_GC_UNLOCKED ();
	if (try_free)
		mono_thread_hazardous_try_free_some ();
}

void
sgen_major_collector_iterate_live_block_ranges (sgen_cardtable_block_callback callback)
{
	major_collector.iterate_live_block_ranges (callback);
}

void
sgen_major_collector_scan_card_table (SgenGrayQueue *queue)
{
	major_collector.scan_card_table (FALSE, queue);
}

SgenMajorCollector*
sgen_get_major_collector (void)
{
	return &major_collector;
}

void mono_gc_set_skip_thread (gboolean skip)
{
	SgenThreadInfo *info = mono_thread_info_current ();

	LOCK_GC;
	info->gc_disabled = skip;
	UNLOCK_GC;
}

SgenRemeberedSet*
sgen_get_remset (void)
{
	return &remset;
}

guint
mono_gc_get_vtable_bits (MonoClass *class)
{
	guint res = 0;
	/* FIXME move this to the bridge code */
	if (sgen_need_bridge_processing ()) {
		switch (sgen_bridge_class_kind (class)) {
		case GC_BRIDGE_TRANSPARENT_BRIDGE_CLASS:
		case GC_BRIDGE_OPAQUE_BRIDGE_CLASS:
			res = SGEN_GC_BIT_BRIDGE_OBJECT;
			break;
		case GC_BRIDGE_OPAQUE_CLASS:
			res = SGEN_GC_BIT_BRIDGE_OPAQUE_OBJECT;
			break;
		}
	}
	if (fin_callbacks.is_class_finalization_aware) {
		if (fin_callbacks.is_class_finalization_aware (class))
			res |= SGEN_GC_BIT_FINALIZER_AWARE;
	}
	return res;
}

void
mono_gc_register_altstack (gpointer stack, gint32 stack_size, gpointer altstack, gint32 altstack_size)
{
	// FIXME:
}


void
sgen_check_whole_heap_stw (void)
{
	sgen_stop_world (0);
	sgen_clear_nursery_fragments ();
	sgen_check_whole_heap (FALSE);
	sgen_restart_world (0, NULL);
}

void
sgen_gc_event_moves (void)
{
	if (moved_objects_idx) {
		mono_profiler_gc_moves (moved_objects, moved_objects_idx);
		moved_objects_idx = 0;
	}
}

gint64
sgen_timestamp (void)
{
	SGEN_TV_DECLARE (timestamp);
	SGEN_TV_GETTIME (timestamp);
	return SGEN_TV_ELAPSED (sgen_init_timestamp, timestamp);
}

void
mono_gc_register_finalizer_callbacks (MonoGCFinalizerCallbacks *callbacks)
{
	if (callbacks->version != MONO_GC_FINALIZER_EXTENSION_VERSION)
		g_error ("Invalid finalizer callback version. Expected %d but got %d\n", MONO_GC_FINALIZER_EXTENSION_VERSION, callbacks->version);

	fin_callbacks = *callbacks;
}

#endif /* HAVE_SGEN_GC */
