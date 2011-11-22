/*
 * sgen-gc.c: Simple generational GC.
 *
 * Author:
 * 	Paolo Molaro (lupus@ximian.com)
 *  Rodrigo Kumpera (kumpera@gmail.com)
 *
 * Copyright 2005-2011 Novell, Inc (http://www.novell.com)
 * Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
 *
 * Thread start/stop adapted from Boehm's GC:
 * Copyright (c) 1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1996 by Silicon Graphics.  All rights reserved.
 * Copyright (c) 1998 by Fergus Henderson.  All rights reserved.
 * Copyright (c) 2000-2004 by Hewlett-Packard Company.  All rights reserved.
 *
 * THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
 * OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
 *
 * Permission is hereby granted to use or copy this program
 * for any purpose,  provided the above notices are retained on all copies.
 * Permission to modify the code and to distribute modified code is granted,
 * provided the above notices are retained, and a notice that the code was
 * modified is included with the above copyright notice.
 *
 *
 * Copyright 2001-2003 Ximian, Inc
 * Copyright 2003-2010 Novell, Inc.
 * Copyright 2011 Xamarin, Inc.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
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

#include <unistd.h>
#include <stdio.h>
#include <string.h>
#include <semaphore.h>
#include <signal.h>
#include <errno.h>
#include <assert.h>
#ifdef __MACH__
#undef _XOPEN_SOURCE
#endif
#include <pthread.h>
#ifdef __MACH__
#define _XOPEN_SOURCE
#endif

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
#include "metadata/mono-gc.h"
#include "metadata/method-builder.h"
#include "metadata/profiler-private.h"
#include "metadata/monitor.h"
#include "metadata/threadpool-internals.h"
#include "metadata/mempool-internals.h"
#include "metadata/marshal.h"
#include "metadata/runtime.h"
#include "utils/mono-mmap.h"
#include "utils/mono-time.h"
#include "utils/mono-semaphore.h"
#include "utils/mono-counters.h"
#include "utils/mono-proclib.h"
#include "utils/mono-memory-model.h"
#include "utils/mono-logger-internal.h"

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
static gint32 gc_initialized = 0;
/* If set, do a minor collection before every X allocation */
static guint32 collect_before_allocs = 0;
/* If set, do a heap consistency check before each minor collection */
static gboolean consistency_check_at_minor_collection = FALSE;
/* If set, check that there are no references to the domain left at domain unload */
static gboolean xdomain_checks = FALSE;
/* If not null, dump the heap after each collection into this file */
static FILE *heap_dump_file = NULL;
/* If set, mark stacks conservatively, even if precise marking is possible */
static gboolean conservative_stack_mark = FALSE;
/* If set, do a plausibility check on the scan_starts before and after
   each collection */
static gboolean do_scan_starts_check = FALSE;
static gboolean nursery_collection_is_parallel = FALSE;
static gboolean disable_minor_collections = FALSE;
static gboolean disable_major_collections = FALSE;
static gboolean do_pin_stats = FALSE;
static gboolean do_verify_nursery = FALSE;
static gboolean do_dump_nursery_content = FALSE;

#ifdef HEAVY_STATISTICS
static long long stat_objects_alloced = 0;
static long long stat_bytes_alloced = 0;
long long stat_objects_alloced_degraded = 0;
long long stat_bytes_alloced_degraded = 0;
static long long stat_bytes_alloced_los = 0;

long long stat_copy_object_called_nursery = 0;
long long stat_objects_copied_nursery = 0;
long long stat_copy_object_called_major = 0;
long long stat_objects_copied_major = 0;

long long stat_scan_object_called_nursery = 0;
long long stat_scan_object_called_major = 0;

long long stat_nursery_copy_object_failed_from_space = 0;
long long stat_nursery_copy_object_failed_forwarded = 0;
long long stat_nursery_copy_object_failed_pinned = 0;

static long long stat_store_remsets = 0;
static long long stat_store_remsets_unique = 0;
static long long stat_saved_remsets_1 = 0;
static long long stat_saved_remsets_2 = 0;
static long long stat_local_remsets_processed = 0;
static long long stat_global_remsets_added = 0;
static long long stat_global_remsets_readded = 0;
static long long stat_global_remsets_processed = 0;
static long long stat_global_remsets_discarded = 0;

static int stat_wbarrier_set_field = 0;
static int stat_wbarrier_set_arrayref = 0;
static int stat_wbarrier_arrayref_copy = 0;
static int stat_wbarrier_generic_store = 0;
static int stat_wbarrier_generic_store_remset = 0;
static int stat_wbarrier_set_root = 0;
static int stat_wbarrier_value_copy = 0;
static int stat_wbarrier_object_copy = 0;
#endif

static long long stat_pinned_objects = 0;

static long long time_minor_pre_collection_fragment_clear = 0;
static long long time_minor_pinning = 0;
static long long time_minor_scan_remsets = 0;
static long long time_minor_scan_card_table = 0;
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
static gboolean debug_print_allowance = FALSE;

/*
void
mono_gc_flush_info (void)
{
	fflush (gc_debug_file);
}
*/

/*
 * Define this to allow the user to change the nursery size by
 * specifying its value in the MONO_GC_PARAMS environmental
 * variable. See mono_gc_base_init for details.
 */
#define USER_CONFIG 1

#define TV_DECLARE SGEN_TV_DECLARE
#define TV_GETTIME SGEN_TV_GETTIME
#define TV_ELAPSED SGEN_TV_ELAPSED
#define TV_ELAPSED_MS SGEN_TV_ELAPSED_MS

#define ALIGN_TO(val,align) ((((guint64)val) + ((align) - 1)) & ~((align) - 1))

static NurseryClearPolicy nursery_clear_policy = CLEAR_AT_TLAB_CREATION;

/* the runtime can register areas of memory as roots: we keep two lists of roots,
 * a pinned root set for conservatively scanned roots and a normal one for
 * precisely scanned roots (currently implemented as a single list).
 */
typedef struct _RootRecord RootRecord;
struct _RootRecord {
	char *end_root;
	mword root_desc;
};

/*
 * We're never actually using the first element.  It's always set to
 * NULL to simplify the elimination of consecutive duplicate
 * entries.
 */
#define STORE_REMSET_BUFFER_SIZE	1023

typedef struct _GenericStoreRememberedSet GenericStoreRememberedSet;
struct _GenericStoreRememberedSet {
	GenericStoreRememberedSet *next;
	/* We need one entry less because the first entry of store
	   remset buffers is always a dummy and we don't copy it. */
	gpointer data [STORE_REMSET_BUFFER_SIZE - 1];
};

/* we have 4 possible values in the low 2 bits */
enum {
	REMSET_LOCATION, /* just a pointer to the exact location */
	REMSET_RANGE,    /* range of pointer fields */
	REMSET_OBJECT,   /* mark all the object for scanning */
	REMSET_VTYPE,    /* a valuetype array described by a gc descriptor, a count and a size */
	REMSET_TYPE_MASK = 0x3
};

#ifdef HAVE_KW_THREAD
static __thread RememberedSet *remembered_set MONO_TLS_FAST;
#endif
static MonoNativeTlsKey remembered_set_key;
static RememberedSet *global_remset;
static RememberedSet *freed_thread_remsets;
static GenericStoreRememberedSet *generic_store_remsets = NULL;

/*A two slots cache for recently inserted remsets */
static gpointer global_remset_cache [2];

/* FIXME: later choose a size that takes into account the RememberedSet struct
 * and doesn't waste any alloc paddin space.
 */
#define DEFAULT_REMSET_SIZE 1024
static RememberedSet* alloc_remset (int size, gpointer id, gboolean global);

#define object_is_forwarded	SGEN_OBJECT_IS_FORWARDED
#define object_is_pinned	SGEN_OBJECT_IS_PINNED
#define pin_object		SGEN_PIN_OBJECT
#define unpin_object		SGEN_UNPIN_OBJECT

#define ptr_in_nursery(p)	(SGEN_PTR_IN_NURSERY ((p), DEFAULT_NURSERY_BITS, nursery_start, nursery_end))

#define LOAD_VTABLE	SGEN_LOAD_VTABLE

static const char*
safe_name (void* obj)
{
	MonoVTable *vt = (MonoVTable*)LOAD_VTABLE (obj);
	return vt->klass->name;
}

#define safe_object_get_size	mono_sgen_safe_object_get_size

const char*
mono_sgen_safe_name (void* obj)
{
	return safe_name (obj);
}

/*
 * ######################################################################
 * ########  Global data.
 * ######################################################################
 */
static LOCK_DECLARE (gc_mutex);
static int gc_disabled = 0;
static int num_minor_gcs = 0;
static int num_major_gcs = 0;

static gboolean use_cardtable;

#ifdef USER_CONFIG

/* good sizes are 512KB-1MB: larger ones increase a lot memzeroing time */
#define DEFAULT_NURSERY_SIZE (default_nursery_size)
static int default_nursery_size = (1 << 22);
#ifdef SGEN_ALIGN_NURSERY
/* The number of trailing 0 bits in DEFAULT_NURSERY_SIZE */
#define DEFAULT_NURSERY_BITS (default_nursery_bits)
static int default_nursery_bits = 22;
#endif

#else

#define DEFAULT_NURSERY_SIZE (4*1024*1024)
#ifdef SGEN_ALIGN_NURSERY
#define DEFAULT_NURSERY_BITS 22
#endif

#endif

#ifndef SGEN_ALIGN_NURSERY
#define DEFAULT_NURSERY_BITS -1
#endif

#define MIN_MINOR_COLLECTION_ALLOWANCE	(DEFAULT_NURSERY_SIZE * 4)

#define SCAN_START_SIZE	SGEN_SCAN_START_SIZE

static mword pagesize = 4096;
static mword nursery_size;
static int degraded_mode = 0;

static mword bytes_pinned_from_failed_allocation = 0;

static mword total_alloc = 0;
/* use this to tune when to do a major/minor collection */
static mword memory_pressure = 0;
static mword minor_collection_allowance;
static int minor_collection_sections_alloced = 0;


/* GC Logging stats */
static int last_major_num_sections = 0;
static int last_los_memory_usage = 0;
static gboolean major_collection_happened = FALSE;

static GCMemSection *nursery_section = NULL;
static mword lowest_heap_address = ~(mword)0;
static mword highest_heap_address = 0;

static LOCK_DECLARE (interruption_mutex);
static LOCK_DECLARE (global_remset_mutex);
static LOCK_DECLARE (pin_queue_mutex);

#define LOCK_GLOBAL_REMSET mono_mutex_lock (&global_remset_mutex)
#define UNLOCK_GLOBAL_REMSET mono_mutex_unlock (&global_remset_mutex)

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

/*
 * The link pointer is hidden by negating each bit.  We use the lowest
 * bit of the link (before negation) to store whether it needs
 * resurrection tracking.
 */
#define HIDE_POINTER(p,t)	((gpointer)(~((gulong)(p)|((t)?1:0))))
#define REVEAL_POINTER(p)	((gpointer)((~(gulong)(p))&~3L))

/* objects that are ready to be finalized */
static FinalizeReadyEntry *fin_ready_list = NULL;
static FinalizeReadyEntry *critical_fin_list = NULL;

static EphemeronLinkNode *ephemeron_list;

static int num_ready_finalizers = 0;
static int no_finalize = 0;

enum {
	ROOT_TYPE_NORMAL = 0, /* "normal" roots */
	ROOT_TYPE_PINNED = 1, /* roots without a GC descriptor */
	ROOT_TYPE_WBARRIER = 2, /* roots with a write barrier */
	ROOT_TYPE_NUM
};

/* registered roots: the key to the hash is the root start address */
/* 
 * Different kinds of roots are kept separate to speed up pin_from_roots () for example.
 */
static SgenHashTable roots_hash [ROOT_TYPE_NUM] = {
	SGEN_HASH_TABLE_INIT (INTERNAL_MEM_ROOTS_TABLE, INTERNAL_MEM_ROOT_RECORD, sizeof (RootRecord), mono_aligned_addr_hash, NULL),
	SGEN_HASH_TABLE_INIT (INTERNAL_MEM_ROOTS_TABLE, INTERNAL_MEM_ROOT_RECORD, sizeof (RootRecord), mono_aligned_addr_hash, NULL),
	SGEN_HASH_TABLE_INIT (INTERNAL_MEM_ROOTS_TABLE, INTERNAL_MEM_ROOT_RECORD, sizeof (RootRecord), mono_aligned_addr_hash, NULL)
};
static mword roots_size = 0; /* amount of memory in the root set */

#define GC_ROOT_NUM 32
typedef struct {
	int count;
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

/* 
 * The current allocation cursors
 * We allocate objects in the nursery.
 * The nursery is the area between nursery_start and nursery_end.
 * Allocation is done from a Thread Local Allocation Buffer (TLAB). TLABs are allocated
 * from nursery fragments.
 * tlab_next is the pointer to the space inside the TLAB where the next object will 
 * be allocated.
 * tlab_temp_end is the pointer to the end of the temporary space reserved for
 * the allocation: it allows us to set the scan starts at reasonable intervals.
 * tlab_real_end points to the end of the TLAB.
 * nursery_frag_real_end points to the end of the currently used nursery fragment.
 * nursery_first_pinned_start points to the start of the first pinned object in the nursery
 * nursery_last_pinned_end points to the end of the last pinned object in the nursery
 * At the next allocation, the area of the nursery where objects can be present is
 * between MIN(nursery_first_pinned_start, first_fragment_start) and
 * MAX(nursery_last_pinned_end, nursery_frag_real_end)
 */
static char *nursery_start = NULL;
static char *nursery_end = NULL;
static char *nursery_alloc_bound = NULL;

#ifdef HAVE_KW_THREAD
#define TLAB_ACCESS_INIT
#define TLAB_START	tlab_start
#define TLAB_NEXT	tlab_next
#define TLAB_TEMP_END	tlab_temp_end
#define TLAB_REAL_END	tlab_real_end
#define REMEMBERED_SET	remembered_set
#define STORE_REMSET_BUFFER	store_remset_buffer
#define STORE_REMSET_BUFFER_INDEX	store_remset_buffer_index
#define IN_CRITICAL_REGION thread_info->in_critical_region
#else
static MonoNativeTlsKey thread_info_key;
#define TLAB_ACCESS_INIT	SgenThreadInfo *__thread_info__ = mono_native_tls_get_value (thread_info_key)
#define TLAB_START	(__thread_info__->tlab_start)
#define TLAB_NEXT	(__thread_info__->tlab_next)
#define TLAB_TEMP_END	(__thread_info__->tlab_temp_end)
#define TLAB_REAL_END	(__thread_info__->tlab_real_end)
#define REMEMBERED_SET	(__thread_info__->remset)
#define STORE_REMSET_BUFFER	(__thread_info__->store_remset_buffer)
#define STORE_REMSET_BUFFER_INDEX	(__thread_info__->store_remset_buffer_index)
#define IN_CRITICAL_REGION (__thread_info__->in_critical_region)
#endif

#ifndef DISABLE_CRITICAL_REGION

/* Enter must be visible before anything is done in the critical region. */
#define ENTER_CRITICAL_REGION do { mono_atomic_store_acquire (&IN_CRITICAL_REGION, 1); } while (0)

/* Exit must make sure all critical regions stores are visible before it signal the end of the region. 
 * We don't need to emit a full barrier since we
 */
#define EXIT_CRITICAL_REGION  do { mono_atomic_store_release (&IN_CRITICAL_REGION, 0); } while (0)


#endif

/*
 * FIXME: What is faster, a TLS variable pointing to a structure, or separate TLS 
 * variables for next+temp_end ?
 */
#ifdef HAVE_KW_THREAD
static __thread SgenThreadInfo *thread_info;
static __thread char *tlab_start;
static __thread char *tlab_next;
static __thread char *tlab_temp_end;
static __thread char *tlab_real_end;
static __thread gpointer *store_remset_buffer;
static __thread long store_remset_buffer_index;
/* Used by the managed allocator/wbarrier */
static __thread char **tlab_next_addr;
static __thread char *stack_end;
static __thread long *store_remset_buffer_index_addr;
#endif

/* The size of a TLAB */
/* The bigger the value, the less often we have to go to the slow path to allocate a new 
 * one, but the more space is wasted by threads not allocating much memory.
 * FIXME: Tune this.
 * FIXME: Make this self-tuning for each thread.
 */
static guint32 tlab_size = (1024 * 4);

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

/*
 * ######################################################################
 * ########  Heap size accounting
 * ######################################################################
 */
/*heap limits*/
static mword max_heap_size = ((mword)0)- ((mword)1);
static mword soft_heap_limit = ((mword)0) - ((mword)1);
static mword allocated_heap;

/*Object was pinned during the current collection*/
static mword objects_pinned;

void
mono_sgen_release_space (mword size, int space)
{
	allocated_heap -= size;
}

static size_t
available_free_space (void)
{
	return max_heap_size - MIN (allocated_heap, max_heap_size);
}

gboolean
mono_sgen_try_alloc_space (mword size, int space)
{
	if (available_free_space () < size)
		return FALSE;

	allocated_heap += size;
	mono_runtime_resource_check_limit (MONO_RESOURCE_GC_HEAP, allocated_heap);
	return TRUE;
}

static void
init_heap_size_limits (glong max_heap, glong soft_limit)
{
	if (soft_limit)
		soft_heap_limit = soft_limit;

	if (max_heap == 0)
		return;

	if (max_heap < soft_limit) {
		fprintf (stderr, "max-heap-size must be at least as large as soft-heap-limit.\n");
		exit (1);
	}

	if (max_heap < nursery_size * 4) {
		fprintf (stderr, "max-heap-size must be at least 4 times larger than nursery size.\n");
		exit (1);
	}
	max_heap_size = max_heap - nursery_size;
}

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
static int stop_world (int generation);
static int restart_world (int generation);
static void scan_thread_data (void *start_nursery, void *end_nursery, gboolean precise, GrayQueue *queue);
static void scan_from_global_remsets (void *start_nursery, void *end_nursery, GrayQueue *queue);
static void scan_from_remsets (void *start_nursery, void *end_nursery, GrayQueue *queue);
static void scan_from_registered_roots (CopyOrMarkObjectFunc copy_func, char *addr_start, char *addr_end, int root_type, GrayQueue *queue);
static void scan_finalizer_entries (CopyOrMarkObjectFunc copy_func, FinalizeReadyEntry *list, GrayQueue *queue);
static void report_finalizer_roots (void);
static void report_registered_roots (void);
static void find_pinning_ref_from_thread (char *obj, size_t size);
static void update_current_thread_stack (void *start);
static void collect_bridge_objects (CopyOrMarkObjectFunc copy_func, char *start, char *end, int generation, GrayQueue *queue);
static void finalize_in_range (CopyOrMarkObjectFunc copy_func, char *start, char *end, int generation, GrayQueue *queue);
static void process_fin_stage_entries (void);
static void null_link_in_range (CopyOrMarkObjectFunc copy_func, char *start, char *end, int generation, gboolean before_finalization, GrayQueue *queue);
static void null_links_for_domain (MonoDomain *domain, int generation);
static void process_dislink_stage_entries (void);

static void pin_from_roots (void *start_nursery, void *end_nursery, GrayQueue *queue);
static int pin_objects_from_addresses (GCMemSection *section, void **start, void **end, void *start_nursery, void *end_nursery, GrayQueue *queue);
static void optimize_pin_queue (int start_slot);
static void clear_remsets (void);
static void clear_tlabs (void);
static void sort_addresses (void **array, int size);
static gboolean drain_gray_stack (GrayQueue *queue, int max_objs);
static void finish_gray_stack (char *start_addr, char *end_addr, int generation, GrayQueue *queue);
static gboolean need_major_collection (mword space_needed);
static void major_collection (const char *reason);

static gboolean collection_is_parallel (void);

static void mono_gc_register_disappearing_link (MonoObject *obj, void **link, gboolean track, gboolean in_gc);
static gboolean mono_gc_is_critical_method (MonoMethod *method);

void describe_ptr (char *ptr);
void check_object (char *start);

static void check_consistency (void);
static void check_major_refs (void);
static void check_scan_starts (void);
static void check_for_xdomain_refs (void);
static void dump_heap (const char *type, int num, const char *reason);

void mono_gc_scan_for_specific_ref (MonoObject *key, gboolean precise);

static void init_stats (void);

static int mark_ephemerons_in_range (CopyOrMarkObjectFunc copy_func, char *start, char *end, GrayQueue *queue);
static void clear_unreachable_ephemerons (CopyOrMarkObjectFunc copy_func, char *start, char *end, GrayQueue *queue);
static void null_ephemerons_for_domain (MonoDomain *domain);

SgenMajorCollector major_collector;

#include "sgen-pinning.c"
#include "sgen-pinning-stats.c"
#include "sgen-gray.c"
#include "sgen-workers.c"
#include "sgen-cardtable.c"

/* Root bitmap descriptors are simpler: the lower three bits describe the type
 * and we either have 30/62 bitmap bits or nibble-based run-length,
 * or a complex descriptor, or a user defined marker function.
 */
enum {
	ROOT_DESC_CONSERVATIVE, /* 0, so matches NULL value */
	ROOT_DESC_BITMAP,
	ROOT_DESC_RUN_LEN, 
	ROOT_DESC_COMPLEX,
	ROOT_DESC_USER,
	ROOT_DESC_TYPE_MASK = 0x7,
	ROOT_DESC_TYPE_SHIFT = 3,
};

#define MAKE_ROOT_DESC(type,val) ((type) | ((val) << ROOT_DESC_TYPE_SHIFT))

#define MAX_USER_DESCRIPTORS 16

static gsize* complex_descriptors = NULL;
static int complex_descriptors_size = 0;
static int complex_descriptors_next = 0;
static MonoGCRootMarkFunc user_descriptors [MAX_USER_DESCRIPTORS];
static int user_descriptors_next = 0;

static int
alloc_complex_descriptor (gsize *bitmap, int numbits)
{
	int nwords, res, i;

	numbits = ALIGN_TO (numbits, GC_BITS_PER_WORD);
	nwords = numbits / GC_BITS_PER_WORD + 1;

	LOCK_GC;
	res = complex_descriptors_next;
	/* linear search, so we don't have duplicates with domain load/unload
	 * this should not be performance critical or we'd have bigger issues
	 * (the number and size of complex descriptors should be small).
	 */
	for (i = 0; i < complex_descriptors_next; ) {
		if (complex_descriptors [i] == nwords) {
			int j, found = TRUE;
			for (j = 0; j < nwords - 1; ++j) {
				if (complex_descriptors [i + 1 + j] != bitmap [j]) {
					found = FALSE;
					break;
				}
			}
			if (found) {
				UNLOCK_GC;
				return i;
			}
		}
		i += complex_descriptors [i];
	}
	if (complex_descriptors_next + nwords > complex_descriptors_size) {
		int new_size = complex_descriptors_size * 2 + nwords;
		complex_descriptors = g_realloc (complex_descriptors, new_size * sizeof (gsize));
		complex_descriptors_size = new_size;
	}
	DEBUG (6, fprintf (gc_debug_file, "Complex descriptor %d, size: %d (total desc memory: %d)\n", res, nwords, complex_descriptors_size));
	complex_descriptors_next += nwords;
	complex_descriptors [res] = nwords;
	for (i = 0; i < nwords - 1; ++i) {
		complex_descriptors [res + 1 + i] = bitmap [i];
		DEBUG (6, fprintf (gc_debug_file, "\tvalue: %p\n", (void*)complex_descriptors [res + 1 + i]));
	}
	UNLOCK_GC;
	return res;
}

gsize*
mono_sgen_get_complex_descriptor (mword desc)
{
	return complex_descriptors + (desc >> LOW_TYPE_BITS);
}

/*
 * Descriptor builders.
 */
void*
mono_gc_make_descr_for_string (gsize *bitmap, int numbits)
{
	return (void*) DESC_TYPE_RUN_LENGTH;
}

void*
mono_gc_make_descr_for_object (gsize *bitmap, int numbits, size_t obj_size)
{
	int first_set = -1, num_set = 0, last_set = -1, i;
	mword desc = 0;
	size_t stored_size = obj_size;
	for (i = 0; i < numbits; ++i) {
		if (bitmap [i / GC_BITS_PER_WORD] & ((gsize)1 << (i % GC_BITS_PER_WORD))) {
			if (first_set < 0)
				first_set = i;
			last_set = i;
			num_set++;
		}
	}
	/*
	 * We don't encode the size of types that don't contain
	 * references because they might not be aligned, i.e. the
	 * bottom two bits might be set, which would clash with the
	 * bits we need to encode the descriptor type.  Since we don't
	 * use the encoded size to skip objects, other than for
	 * processing remsets, in which case only the positions of
	 * references are relevant, this is not a problem.
	 */
	if (first_set < 0)
		return (void*)DESC_TYPE_RUN_LENGTH;
	g_assert (!(stored_size & 0x3));
	if (stored_size <= MAX_SMALL_OBJ_SIZE) {
		/* check run-length encoding first: one byte offset, one byte number of pointers
		 * on 64 bit archs, we can have 3 runs, just one on 32.
		 * It may be better to use nibbles.
		 */
		if (first_set < 0) {
			desc = DESC_TYPE_RUN_LENGTH | (stored_size << 1);
			DEBUG (6, fprintf (gc_debug_file, "Ptrfree descriptor %p, size: %zd\n", (void*)desc, stored_size));
			return (void*) desc;
		} else if (first_set < 256 && num_set < 256 && (first_set + num_set == last_set + 1)) {
			desc = DESC_TYPE_RUN_LENGTH | (stored_size << 1) | (first_set << 16) | (num_set << 24);
			DEBUG (6, fprintf (gc_debug_file, "Runlen descriptor %p, size: %zd, first set: %d, num set: %d\n", (void*)desc, stored_size, first_set, num_set));
			return (void*) desc;
		}
	}
	/* we know the 2-word header is ptr-free */
	if (last_set < LARGE_BITMAP_SIZE + OBJECT_HEADER_WORDS) {
		desc = DESC_TYPE_LARGE_BITMAP | ((*bitmap >> OBJECT_HEADER_WORDS) << LOW_TYPE_BITS);
		DEBUG (6, fprintf (gc_debug_file, "Largebitmap descriptor %p, size: %zd, last set: %d\n", (void*)desc, stored_size, last_set));
		return (void*) desc;
	}
	/* it's a complex object ... */
	desc = DESC_TYPE_COMPLEX | (alloc_complex_descriptor (bitmap, last_set + 1) << LOW_TYPE_BITS);
	return (void*) desc;
}

/* If the array holds references, numbits == 1 and the first bit is set in elem_bitmap */
void*
mono_gc_make_descr_for_array (int vector, gsize *elem_bitmap, int numbits, size_t elem_size)
{
	int first_set = -1, num_set = 0, last_set = -1, i;
	mword desc = vector? DESC_TYPE_VECTOR: DESC_TYPE_ARRAY;
	for (i = 0; i < numbits; ++i) {
		if (elem_bitmap [i / GC_BITS_PER_WORD] & ((gsize)1 << (i % GC_BITS_PER_WORD))) {
			if (first_set < 0)
				first_set = i;
			last_set = i;
			num_set++;
		}
	}
	/* See comment at the definition of DESC_TYPE_RUN_LENGTH. */
	if (first_set < 0)
		return (void*)DESC_TYPE_RUN_LENGTH;
	if (elem_size <= MAX_ELEMENT_SIZE) {
		desc |= elem_size << VECTOR_ELSIZE_SHIFT;
		if (!num_set) {
			return (void*)(desc | VECTOR_SUBTYPE_PTRFREE);
		}
		/* Note: we also handle structs with just ref fields */
		if (num_set * sizeof (gpointer) == elem_size) {
			return (void*)(desc | VECTOR_SUBTYPE_REFS | ((gssize)(-1) << 16));
		}
		/* FIXME: try run-len first */
		/* Note: we can't skip the object header here, because it's not present */
		if (last_set <= SMALL_BITMAP_SIZE) {
			return (void*)(desc | VECTOR_SUBTYPE_BITMAP | (*elem_bitmap << 16));
		}
	}
	/* it's am array of complex structs ... */
	desc = DESC_TYPE_COMPLEX_ARR;
	desc |= alloc_complex_descriptor (elem_bitmap, last_set + 1) << LOW_TYPE_BITS;
	return (void*) desc;
}

/* Return the bitmap encoded by a descriptor */
gsize*
mono_gc_get_bitmap_for_descr (void *descr, int *numbits)
{
	mword d = (mword)descr;
	gsize *bitmap;

	switch (d & 0x7) {
	case DESC_TYPE_RUN_LENGTH: {		
		int first_set = (d >> 16) & 0xff;
		int num_set = (d >> 24) & 0xff;
		int i;

		bitmap = g_new0 (gsize, (first_set + num_set + 7) / 8);

		for (i = first_set; i < first_set + num_set; ++i)
			bitmap [i / GC_BITS_PER_WORD] |= ((gsize)1 << (i % GC_BITS_PER_WORD));

		*numbits = first_set + num_set;

		return bitmap;
	}
	case DESC_TYPE_LARGE_BITMAP: {
		gsize bmap = (d >> LOW_TYPE_BITS) << OBJECT_HEADER_WORDS;

		bitmap = g_new0 (gsize, 1);
		bitmap [0] = bmap;
		*numbits = 0;
		while (bmap) {
			(*numbits) ++;
			bmap >>= 1;
		}
		return bitmap;
	}
	default:
		g_assert_not_reached ();
	}
}

static gboolean
is_xdomain_ref_allowed (gpointer *ptr, char *obj, MonoDomain *domain)
{
	MonoObject *o = (MonoObject*)(obj);
	MonoObject *ref = (MonoObject*)*(ptr);
	int offset = (char*)(ptr) - (char*)o;

	if (o->vtable->klass == mono_defaults.thread_class && offset == G_STRUCT_OFFSET (MonoThread, internal_thread))
		return TRUE;
	if (o->vtable->klass == mono_defaults.internal_thread_class && offset == G_STRUCT_OFFSET (MonoInternalThread, current_appcontext))
		return TRUE;
	if (mono_class_has_parent (o->vtable->klass, mono_defaults.real_proxy_class) &&
			offset == G_STRUCT_OFFSET (MonoRealProxy, unwrapped_server))
		return TRUE;
	/* Thread.cached_culture_info */
	if (!strcmp (ref->vtable->klass->name_space, "System.Globalization") &&
			!strcmp (ref->vtable->klass->name, "CultureInfo") &&
			!strcmp(o->vtable->klass->name_space, "System") &&
			!strcmp(o->vtable->klass->name, "Object[]"))
		return TRUE;
	/*
	 *  at System.IO.MemoryStream.InternalConstructor (byte[],int,int,bool,bool) [0x0004d] in /home/schani/Work/novell/trunk/mcs/class/corlib/System.IO/MemoryStream.cs:121
	 * at System.IO.MemoryStream..ctor (byte[]) [0x00017] in /home/schani/Work/novell/trunk/mcs/class/corlib/System.IO/MemoryStream.cs:81
	 * at (wrapper remoting-invoke-with-check) System.IO.MemoryStream..ctor (byte[]) <IL 0x00020, 0xffffffff>
	 * at System.Runtime.Remoting.Messaging.CADMethodCallMessage.GetArguments () [0x0000d] in /home/schani/Work/novell/trunk/mcs/class/corlib/System.Runtime.Remoting.Messaging/CADMessages.cs:327
	 * at System.Runtime.Remoting.Messaging.MethodCall..ctor (System.Runtime.Remoting.Messaging.CADMethodCallMessage) [0x00017] in /home/schani/Work/novell/trunk/mcs/class/corlib/System.Runtime.Remoting.Messaging/MethodCall.cs:87
	 * at System.AppDomain.ProcessMessageInDomain (byte[],System.Runtime.Remoting.Messaging.CADMethodCallMessage,byte[]&,System.Runtime.Remoting.Messaging.CADMethodReturnMessage&) [0x00018] in /home/schani/Work/novell/trunk/mcs/class/corlib/System/AppDomain.cs:1213
	 * at (wrapper remoting-invoke-with-check) System.AppDomain.ProcessMessageInDomain (byte[],System.Runtime.Remoting.Messaging.CADMethodCallMessage,byte[]&,System.Runtime.Remoting.Messaging.CADMethodReturnMessage&) <IL 0x0003d, 0xffffffff>
	 * at System.Runtime.Remoting.Channels.CrossAppDomainSink.ProcessMessageInDomain (byte[],System.Runtime.Remoting.Messaging.CADMethodCallMessage) [0x00008] in /home/schani/Work/novell/trunk/mcs/class/corlib/System.Runtime.Remoting.Channels/CrossAppDomainChannel.cs:198
	 * at (wrapper runtime-invoke) object.runtime_invoke_CrossAppDomainSink/ProcessMessageRes_object_object (object,intptr,intptr,intptr) <IL 0x0004c, 0xffffffff>
	 */
	if (!strcmp (ref->vtable->klass->name_space, "System") &&
			!strcmp (ref->vtable->klass->name, "Byte[]") &&
			!strcmp (o->vtable->klass->name_space, "System.IO") &&
			!strcmp (o->vtable->klass->name, "MemoryStream"))
		return TRUE;
	/* append_job() in threadpool.c */
	if (!strcmp (ref->vtable->klass->name_space, "System.Runtime.Remoting.Messaging") &&
			!strcmp (ref->vtable->klass->name, "AsyncResult") &&
			!strcmp (o->vtable->klass->name_space, "System") &&
			!strcmp (o->vtable->klass->name, "Object[]") &&
			mono_thread_pool_is_queue_array ((MonoArray*) o))
		return TRUE;
	return FALSE;
}

static void
check_reference_for_xdomain (gpointer *ptr, char *obj, MonoDomain *domain)
{
	MonoObject *o = (MonoObject*)(obj);
	MonoObject *ref = (MonoObject*)*(ptr);
	int offset = (char*)(ptr) - (char*)o;
	MonoClass *class;
	MonoClassField *field;
	char *str;

	if (!ref || ref->vtable->domain == domain)
		return;
	if (is_xdomain_ref_allowed (ptr, obj, domain))
		return;

	field = NULL;
	for (class = o->vtable->klass; class; class = class->parent) {
		int i;

		for (i = 0; i < class->field.count; ++i) {
			if (class->fields[i].offset == offset) {
				field = &class->fields[i];
				break;
			}
		}
		if (field)
			break;
	}

	if (ref->vtable->klass == mono_defaults.string_class)
		str = mono_string_to_utf8 ((MonoString*)ref);
	else
		str = NULL;
	g_print ("xdomain reference in %p (%s.%s) at offset %d (%s) to %p (%s.%s) (%s)  -  pointed to by:\n",
			o, o->vtable->klass->name_space, o->vtable->klass->name,
			offset, field ? field->name : "",
			ref, ref->vtable->klass->name_space, ref->vtable->klass->name, str ? str : "");
	mono_gc_scan_for_specific_ref (o, TRUE);
	if (str)
		g_free (str);
}

#undef HANDLE_PTR
#define HANDLE_PTR(ptr,obj)	check_reference_for_xdomain ((ptr), (obj), domain)

static void
scan_object_for_xdomain_refs (char *start, mword size, void *data)
{
	MonoDomain *domain = ((MonoObject*)start)->vtable->domain;

	#include "sgen-scan-object.h"
}

static gboolean scan_object_for_specific_ref_precise = TRUE;

#undef HANDLE_PTR
#define HANDLE_PTR(ptr,obj) do {		\
	if ((MonoObject*)*(ptr) == key) {	\
	g_print ("found ref to %p in object %p (%s) at offset %td\n",	\
			key, (obj), safe_name ((obj)), ((char*)(ptr) - (char*)(obj))); \
	}								\
	} while (0)

static void
scan_object_for_specific_ref (char *start, MonoObject *key)
{
	char *forwarded;

	if ((forwarded = SGEN_OBJECT_IS_FORWARDED (start)))
		start = forwarded;

	if (scan_object_for_specific_ref_precise) {
		#include "sgen-scan-object.h"
	} else {
		mword *words = (mword*)start;
		size_t size = safe_object_get_size ((MonoObject*)start);
		int i;
		for (i = 0; i < size / sizeof (mword); ++i) {
			if (words [i] == (mword)key) {
				g_print ("found possible ref to %p in object %p (%s) at offset %td\n",
						key, start, safe_name (start), i * sizeof (mword));
			}
		}
	}
}

void
mono_sgen_scan_area_with_callback (char *start, char *end, IterateObjectCallbackFunc callback, void *data, gboolean allow_flags)
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

		callback (obj, size, data);

		start += size;
	}
}

static void
scan_object_for_specific_ref_callback (char *obj, size_t size, MonoObject *key)
{
	scan_object_for_specific_ref (obj, key);
}

static void
check_root_obj_specific_ref (RootRecord *root, MonoObject *key, MonoObject *obj)
{
	if (key != obj)
		return;
	g_print ("found ref to %p in root record %p\n", key, root);
}

static MonoObject *check_key = NULL;
static RootRecord *check_root = NULL;

static void
check_root_obj_specific_ref_from_marker (void **obj)
{
	check_root_obj_specific_ref (check_root, check_key, *obj);
}

static void
scan_roots_for_specific_ref (MonoObject *key, int root_type)
{
	void **start_root;
	RootRecord *root;
	check_key = key;

	SGEN_HASH_TABLE_FOREACH (&roots_hash [root_type], start_root, root) {
		mword desc = root->root_desc;

		check_root = root;

		switch (desc & ROOT_DESC_TYPE_MASK) {
		case ROOT_DESC_BITMAP:
			desc >>= ROOT_DESC_TYPE_SHIFT;
			while (desc) {
				if (desc & 1)
					check_root_obj_specific_ref (root, key, *start_root);
				desc >>= 1;
				start_root++;
			}
			return;
		case ROOT_DESC_COMPLEX: {
			gsize *bitmap_data = complex_descriptors + (desc >> ROOT_DESC_TYPE_SHIFT);
			int bwords = (*bitmap_data) - 1;
			void **start_run = start_root;
			bitmap_data++;
			while (bwords-- > 0) {
				gsize bmap = *bitmap_data++;
				void **objptr = start_run;
				while (bmap) {
					if (bmap & 1)
						check_root_obj_specific_ref (root, key, *objptr);
					bmap >>= 1;
					++objptr;
				}
				start_run += GC_BITS_PER_WORD;
			}
			break;
		}
		case ROOT_DESC_USER: {
			MonoGCRootMarkFunc marker = user_descriptors [desc >> ROOT_DESC_TYPE_SHIFT];
			marker (start_root, check_root_obj_specific_ref_from_marker);
			break;
		}
		case ROOT_DESC_RUN_LEN:
			g_assert_not_reached ();
		default:
			g_assert_not_reached ();
		}
	} SGEN_HASH_TABLE_FOREACH_END;

	check_key = NULL;
	check_root = NULL;
}

void
mono_gc_scan_for_specific_ref (MonoObject *key, gboolean precise)
{
	void **ptr;
	RootRecord *root;

	scan_object_for_specific_ref_precise = precise;

	mono_sgen_scan_area_with_callback (nursery_section->data, nursery_section->end_data,
			(IterateObjectCallbackFunc)scan_object_for_specific_ref_callback, key, TRUE);

	major_collector.iterate_objects (TRUE, TRUE, (IterateObjectCallbackFunc)scan_object_for_specific_ref_callback, key);

	mono_sgen_los_iterate_objects ((IterateObjectCallbackFunc)scan_object_for_specific_ref_callback, key);

	scan_roots_for_specific_ref (key, ROOT_TYPE_NORMAL);
	scan_roots_for_specific_ref (key, ROOT_TYPE_WBARRIER);

	SGEN_HASH_TABLE_FOREACH (&roots_hash [ROOT_TYPE_PINNED], ptr, root) {
		while (ptr < (void**)root->end_root) {
			check_root_obj_specific_ref (root, *ptr, key);
			++ptr;
		}
	} SGEN_HASH_TABLE_FOREACH_END;
}

static gboolean
need_remove_object_for_domain (char *start, MonoDomain *domain)
{
	if (mono_object_domain (start) == domain) {
		DEBUG (4, fprintf (gc_debug_file, "Need to cleanup object %p\n", start));
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
	if (mono_class_has_parent (vt->klass, mono_defaults.real_proxy_class)) {
		MonoObject *server = ((MonoRealProxy*)start)->unwrapped_server;

		/* The server could already have been zeroed out, so
		   we need to check for that, too. */
		if (server && (!LOAD_VTABLE (server) || mono_object_domain (server) == domain)) {
			DEBUG (4, fprintf (gc_debug_file, "Cleaning up remote pointer in %p to object %p\n",
					start, server));
			((MonoRealProxy*)start)->unwrapped_server = NULL;
		}
	}
}

static MonoDomain *check_domain = NULL;

static void
check_obj_not_in_domain (void **o)
{
	g_assert (((MonoObject*)(*o))->vtable->domain != check_domain);
}

static void
scan_for_registered_roots_in_domain (MonoDomain *domain, int root_type)
{
	void **start_root;
	RootRecord *root;
	check_domain = domain;
	SGEN_HASH_TABLE_FOREACH (&roots_hash [root_type], start_root, root) {
		mword desc = root->root_desc;

		/* The MonoDomain struct is allowed to hold
		   references to objects in its own domain. */
		if (start_root == (void**)domain)
			continue;

		switch (desc & ROOT_DESC_TYPE_MASK) {
		case ROOT_DESC_BITMAP:
			desc >>= ROOT_DESC_TYPE_SHIFT;
			while (desc) {
				if ((desc & 1) && *start_root)
					check_obj_not_in_domain (*start_root);
				desc >>= 1;
				start_root++;
			}
			break;
		case ROOT_DESC_COMPLEX: {
			gsize *bitmap_data = complex_descriptors + (desc >> ROOT_DESC_TYPE_SHIFT);
			int bwords = (*bitmap_data) - 1;
			void **start_run = start_root;
			bitmap_data++;
			while (bwords-- > 0) {
				gsize bmap = *bitmap_data++;
				void **objptr = start_run;
				while (bmap) {
					if ((bmap & 1) && *objptr)
						check_obj_not_in_domain (*objptr);
					bmap >>= 1;
					++objptr;
				}
				start_run += GC_BITS_PER_WORD;
			}
			break;
		}
		case ROOT_DESC_USER: {
			MonoGCRootMarkFunc marker = user_descriptors [desc >> ROOT_DESC_TYPE_SHIFT];
			marker (start_root, check_obj_not_in_domain);
			break;
		}
		case ROOT_DESC_RUN_LEN:
			g_assert_not_reached ();
		default:
			g_assert_not_reached ();
		}
	} SGEN_HASH_TABLE_FOREACH_END;

	check_domain = NULL;
}

static void
check_for_xdomain_refs (void)
{
	LOSObject *bigobj;

	mono_sgen_scan_area_with_callback (nursery_section->data, nursery_section->end_data,
			(IterateObjectCallbackFunc)scan_object_for_xdomain_refs, NULL, FALSE);

	major_collector.iterate_objects (TRUE, TRUE, (IterateObjectCallbackFunc)scan_object_for_xdomain_refs, NULL);

	for (bigobj = los_object_list; bigobj; bigobj = bigobj->next)
		scan_object_for_xdomain_refs (bigobj->data, bigobj->size, NULL);
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
			mono_gc_register_disappearing_link (NULL, dislink, FALSE, TRUE);
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

	process_fin_stage_entries ();
	process_dislink_stage_entries ();

	mono_sgen_clear_nursery_fragments ();

	if (xdomain_checks && domain != mono_get_root_domain ()) {
		scan_for_registered_roots_in_domain (domain, ROOT_TYPE_NORMAL);
		scan_for_registered_roots_in_domain (domain, ROOT_TYPE_WBARRIER);
		check_for_xdomain_refs ();
	}

	mono_sgen_scan_area_with_callback (nursery_section->data, nursery_section->end_data,
			(IterateObjectCallbackFunc)clear_domain_process_minor_object_callback, domain, FALSE);

	/*Ephemerons and dislinks must be processed before LOS since they might end up pointing
	to memory returned to the OS.*/
	null_ephemerons_for_domain (domain);

	for (i = GENERATION_NURSERY; i < GENERATION_MAX; ++i)
		null_links_for_domain (domain, i);

	/* We need two passes over major and large objects because
	   freeing such objects might give their memory back to the OS
	   (in the case of large objects) or obliterate its vtable
	   (pinned objects with major-copying or pinned and non-pinned
	   objects with major-mark&sweep), but we might need to
	   dereference a pointer from an object to another object if
	   the first object is a proxy. */
	major_collector.iterate_objects (TRUE, TRUE, (IterateObjectCallbackFunc)clear_domain_process_major_object_callback, domain);
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
			DEBUG (4, fprintf (gc_debug_file, "Freeing large object %p\n",
					bigobj->data));
			mono_sgen_los_free_object (to_free);
			continue;
		}
		prev = bigobj;
		bigobj = bigobj->next;
	}
	major_collector.iterate_objects (TRUE, FALSE, (IterateObjectCallbackFunc)clear_domain_free_major_non_pinned_object_callback, domain);
	major_collector.iterate_objects (FALSE, TRUE, (IterateObjectCallbackFunc)clear_domain_free_major_pinned_object_callback, domain);

	if (do_pin_stats && domain == mono_get_root_domain ())
		mono_sgen_pin_stats_print_class_stats ();

	UNLOCK_GC;
}

static void
global_remset_cache_clear (void)
{
	memset (global_remset_cache, 0, sizeof (global_remset_cache));
}

/*
 * Tries to check if a given remset location was already added to the global remset.
 * It can
 *
 * A 2 entry, LRU cache of recently saw location remsets.
 *
 * It's hand-coded instead of done using loops to reduce the number of memory references on cache hit.
 *
 * Returns TRUE is the element was added..
 */
static gboolean
global_remset_location_was_not_added (gpointer ptr)
{

	gpointer first = global_remset_cache [0], second;
	if (first == ptr) {
		HEAVY_STAT (++stat_global_remsets_discarded);
		return FALSE;
	}

	second = global_remset_cache [1];

	if (second == ptr) {
		/*Move the second to the front*/
		global_remset_cache [0] = second;
		global_remset_cache [1] = first;

		HEAVY_STAT (++stat_global_remsets_discarded);
		return FALSE;
	}

	global_remset_cache [0] = second;
	global_remset_cache [1] = ptr;
	return TRUE;
}

/*
 * mono_sgen_add_to_global_remset:
 *
 *   The global remset contains locations which point into newspace after
 * a minor collection. This can happen if the objects they point to are pinned.
 *
 * LOCKING: If called from a parallel collector, the global remset
 * lock must be held.  For serial collectors that is not necessary.
 */
void
mono_sgen_add_to_global_remset (gpointer ptr)
{
	RememberedSet *rs;
	gboolean lock = collection_is_parallel ();
	gpointer obj = *(gpointer*)ptr;

	if (use_cardtable) {
		sgen_card_table_mark_address ((mword)ptr);
		return;
	}

	g_assert (!ptr_in_nursery (ptr) && ptr_in_nursery (obj));

	if (lock)
		LOCK_GLOBAL_REMSET;

	if (!global_remset_location_was_not_added (ptr))
		goto done;

	if (do_pin_stats)
		mono_sgen_pin_stats_register_global_remset (obj);

	DEBUG (8, fprintf (gc_debug_file, "Adding global remset for %p\n", ptr));
	binary_protocol_global_remset (ptr, *(gpointer*)ptr, (gpointer)LOAD_VTABLE (obj));

	HEAVY_STAT (++stat_global_remsets_added);

	/* 
	 * FIXME: If an object remains pinned, we need to add it at every minor collection.
	 * To avoid uncontrolled growth of the global remset, only add each pointer once.
	 */
	if (global_remset->store_next + 3 < global_remset->end_set) {
		*(global_remset->store_next++) = (mword)ptr;
		goto done;
	}
	rs = alloc_remset (global_remset->end_set - global_remset->data, NULL, TRUE);
	rs->next = global_remset;
	global_remset = rs;
	*(global_remset->store_next++) = (mword)ptr;

	{
		int global_rs_size = 0;

		for (rs = global_remset; rs; rs = rs->next) {
			global_rs_size += rs->store_next - rs->data;
		}
		DEBUG (4, fprintf (gc_debug_file, "Global remset now has size %d\n", global_rs_size));
	}

 done:
	if (lock)
		UNLOCK_GLOBAL_REMSET;
}

/*
 * drain_gray_stack:
 *
 *   Scan objects in the gray stack until the stack is empty. This should be called
 * frequently after each object is copied, to achieve better locality and cache
 * usage.
 */
static gboolean
drain_gray_stack (GrayQueue *queue, int max_objs)
{
	char *obj;

	if (current_collection_generation == GENERATION_NURSERY) {
		ScanObjectFunc scan_func = mono_sgen_get_minor_scan_object ();

		for (;;) {
			GRAY_OBJECT_DEQUEUE (queue, obj);
			if (!obj)
				return TRUE;
			DEBUG (9, fprintf (gc_debug_file, "Precise gray object scan %p (%s)\n", obj, safe_name (obj)));
			scan_func (obj, queue);
		}
	} else {
		int i;

		if (collection_is_parallel () && queue == &workers_distribute_gray_queue)
			return TRUE;

		do {
			for (i = 0; i != max_objs; ++i) {
				GRAY_OBJECT_DEQUEUE (queue, obj);
				if (!obj)
					return TRUE;
				DEBUG (9, fprintf (gc_debug_file, "Precise gray object scan %p (%s)\n", obj, safe_name (obj)));
				major_collector.major_scan_object (obj, queue);
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
pin_objects_from_addresses (GCMemSection *section, void **start, void **end, void *start_nursery, void *end_nursery, GrayQueue *queue)
{
	void *last = NULL;
	int count = 0;
	void *search_start;
	void *last_obj = NULL;
	size_t last_obj_size = 0;
	void *addr;
	int idx;
	void **definitely_pinned = start;

	mono_sgen_nursery_allocator_prepare_for_pinning ();

	while (start < end) {
		addr = *start;
		/* the range check should be reduntant */
		if (addr != last && addr >= start_nursery && addr < end_nursery) {
			DEBUG (5, fprintf (gc_debug_file, "Considering pinning addr %p\n", addr));
			/* multiple pointers to the same object */
			if (addr >= last_obj && (char*)addr < (char*)last_obj + last_obj_size) {
				start++;
				continue;
			}
			idx = ((char*)addr - (char*)section->data) / SCAN_START_SIZE;
			g_assert (idx < section->num_scan_start);
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
			if (search_start < last_obj)
				search_start = (char*)last_obj + last_obj_size;
			/* now addr should be in an object a short distance from search_start
			 * Note that search_start must point to zeroed mem or point to an object.
			 */

			do {
				if (!*(void**)search_start) {
					/* Consistency check */
					/*
					for (frag = nursery_fragments; frag; frag = frag->next) {
						if (search_start >= frag->fragment_start && search_start < frag->fragment_end)
							g_assert_not_reached ();
					}
					*/

					search_start = (void*)ALIGN_UP ((mword)search_start + sizeof (gpointer));
					continue;
				}
				last_obj = search_start;
				last_obj_size = ALIGN_UP (safe_object_get_size ((MonoObject*)search_start));

				if (((MonoObject*)last_obj)->synchronisation == GINT_TO_POINTER (-1)) {
					/* Marks the beginning of a nursery fragment, skip */
				} else {
					DEBUG (8, fprintf (gc_debug_file, "Pinned try match %p (%s), size %zd\n", last_obj, safe_name (last_obj), last_obj_size));
					if (addr >= search_start && (char*)addr < (char*)last_obj + last_obj_size) {
						DEBUG (4, fprintf (gc_debug_file, "Pinned object %p, vtable %p (%s), count %d\n", search_start, *(void**)search_start, safe_name (search_start), count));
						binary_protocol_pin (search_start, (gpointer)LOAD_VTABLE (search_start), safe_object_get_size (search_start));
						pin_object (search_start);
						GRAY_OBJECT_ENQUEUE (queue, search_start);
						if (do_pin_stats)
							mono_sgen_pin_stats_register_object (search_start, last_obj_size);
						definitely_pinned [count] = search_start;
						count++;
						break;
					}
				}
				/* skip to the next object */
				search_start = (void*)((char*)search_start + last_obj_size);
			} while (search_start <= addr);
			/* we either pinned the correct object or we ignored the addr because
			 * it points to unused zeroed memory.
			 */
			last = addr;
		}
		start++;
	}
	//printf ("effective pinned: %d (at the end: %d)\n", count, (char*)end_nursery - (char*)last);
	if (mono_profiler_get_events () & MONO_PROFILE_GC_ROOTS) {
		GCRootReport report;
		report.count = 0;
		for (idx = 0; idx < count; ++idx)
			add_profile_gc_root (&report, definitely_pinned [idx], MONO_PROFILE_GC_ROOT_PINNING, 0);
		notify_gc_roots (&report);
	}
	stat_pinned_objects += count;
	return count;
}

void
mono_sgen_pin_objects_in_section (GCMemSection *section, GrayQueue *queue)
{
	int num_entries = section->pin_queue_num_entries;
	if (num_entries) {
		void **start = section->pin_queue_start;
		int reduced_to;
		reduced_to = pin_objects_from_addresses (section, start, start + num_entries,
				section->data, section->next_data, queue);
		section->pin_queue_num_entries = reduced_to;
		if (!reduced_to)
			section->pin_queue_start = NULL;
	}
}


void
mono_sgen_pin_object (void *object, GrayQueue *queue)
{
	if (collection_is_parallel ()) {
		LOCK_PIN_QUEUE;
		/*object arrives pinned*/
		pin_stage_ptr (object);
		++objects_pinned ;
		UNLOCK_PIN_QUEUE;
	} else {
		SGEN_PIN_OBJECT (object);
		pin_stage_ptr (object);
		++objects_pinned;
		if (do_pin_stats)
			mono_sgen_pin_stats_register_object (object, safe_object_get_size (object));
	}
	GRAY_OBJECT_ENQUEUE (queue, object);
	binary_protocol_pin (object, (gpointer)LOAD_VTABLE (object), safe_object_get_size (object));
}

/* Sort the addresses in array in increasing order.
 * Done using a by-the book heap sort. Which has decent and stable performance, is pretty cache efficient.
 */
static void
sort_addresses (void **array, int size)
{
	int i;
	void *tmp;

	for (i = 1; i < size; ++i) {
		int child = i;
		while (child > 0) {
			int parent = (child - 1) / 2;

			if (array [parent] >= array [child])
				break;

			tmp = array [parent];
			array [parent] = array [child];
			array [child] = tmp;

			child = parent;
		}
	}

	for (i = size - 1; i > 0; --i) {
		int end, root;
		tmp = array [i];
		array [i] = array [0];
		array [0] = tmp;

		end = i - 1;
		root = 0;

		while (root * 2 + 1 <= end) {
			int child = root * 2 + 1;

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

static G_GNUC_UNUSED void
print_nursery_gaps (void* start_nursery, void *end_nursery)
{
	int i;
	gpointer first = start_nursery;
	gpointer next;
	for (i = 0; i < next_pin_slot; ++i) {
		next = pin_queue [i];
		fprintf (gc_debug_file, "Nursery range: %p-%p, size: %td\n", first, next, (char*)next-(char*)first);
		first = next;
	}
	next = end_nursery;
	fprintf (gc_debug_file, "Nursery range: %p-%p, size: %td\n", first, next, (char*)next-(char*)first);
}

/* reduce the info in the pin queue, removing duplicate pointers and sorting them */
static void
optimize_pin_queue (int start_slot)
{
	void **start, **cur, **end;
	/* sort and uniq pin_queue: we just sort and we let the rest discard multiple values */
	/* it may be better to keep ranges of pinned memory instead of individually pinning objects */
	DEBUG (5, fprintf (gc_debug_file, "Sorting pin queue, size: %d\n", next_pin_slot));
	if ((next_pin_slot - start_slot) > 1)
		sort_addresses (pin_queue + start_slot, next_pin_slot - start_slot);
	start = cur = pin_queue + start_slot;
	end = pin_queue + next_pin_slot;
	while (cur < end) {
		*start = *cur++;
		while (*start == *cur && cur < end)
			cur++;
		start++;
	};
	next_pin_slot = start - pin_queue;
	DEBUG (5, fprintf (gc_debug_file, "Pin queue reduced to size: %d\n", next_pin_slot));
	//DEBUG (6, print_nursery_gaps (start_nursery, end_nursery));
	
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
			if (addr >= (mword)start_nursery && addr < (mword)end_nursery)
				pin_stage_ptr ((void*)addr);
			if (do_pin_stats && ptr_in_nursery (addr))
				pin_stats_register_address ((char*)addr, pin_type);
			DEBUG (6, if (count) fprintf (gc_debug_file, "Pinning address %p from %p\n", (void*)addr, start));
			count++;
		}
		start++;
	}
	DEBUG (7, if (count) fprintf (gc_debug_file, "found %d potential pinned heap pointers\n", count));
}

/*
 * Debugging function: find in the conservative roots where @obj is being pinned.
 */
static G_GNUC_UNUSED void
find_pinning_reference (char *obj, size_t size)
{
	char **start;
	RootRecord *root;
	char *endobj = obj + size;

	SGEN_HASH_TABLE_FOREACH (&roots_hash [ROOT_TYPE_NORMAL], start, root) {
		/* if desc is non-null it has precise info */
		if (!root->root_desc) {
			while (start < (char**)root->end_root) {
				if (*start >= obj && *start < endobj) {
					DEBUG (0, fprintf (gc_debug_file, "Object %p referenced in pinned roots %p-%p\n", obj, start, root->end_root));
				}
				start++;
			}
		}
	} SGEN_HASH_TABLE_FOREACH_END;

	find_pinning_ref_from_thread (obj, size);
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
	DEBUG (2, fprintf (gc_debug_file, "Scanning pinned roots (%d bytes, %d/%d entries)\n", (int)roots_size, roots_hash [ROOT_TYPE_NORMAL].num_entries, roots_hash [ROOT_TYPE_PINNED].num_entries));
	/* objects pinned from the API are inside these roots */
	SGEN_HASH_TABLE_FOREACH (&roots_hash [ROOT_TYPE_PINNED], start_root, root) {
		DEBUG (6, fprintf (gc_debug_file, "Pinned roots %p-%p\n", start_root, root->end_root));
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

	evacuate_pin_staging_area ();
}

typedef struct {
	CopyOrMarkObjectFunc func;
	GrayQueue *queue;
} UserCopyOrMarkData;

static MonoNativeTlsKey user_copy_or_mark_key;

static void
init_user_copy_or_mark_key (void)
{
	mono_native_tls_alloc (&user_copy_or_mark_key, NULL);
}

static void
set_user_copy_or_mark_data (UserCopyOrMarkData *data)
{
	mono_native_tls_set_value (user_copy_or_mark_key, data);
}

static void
single_arg_user_copy_or_mark (void **obj)
{
	UserCopyOrMarkData *data = mono_native_tls_get_value (user_copy_or_mark_key);

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
precisely_scan_objects_from (CopyOrMarkObjectFunc copy_func, void** start_root, void** end_root, char* n_start, char *n_end, mword desc, GrayQueue *queue)
{
	switch (desc & ROOT_DESC_TYPE_MASK) {
	case ROOT_DESC_BITMAP:
		desc >>= ROOT_DESC_TYPE_SHIFT;
		while (desc) {
			if ((desc & 1) && *start_root) {
				copy_func (start_root, queue);
				DEBUG (9, fprintf (gc_debug_file, "Overwrote root at %p with %p\n", start_root, *start_root));
				drain_gray_stack (queue, -1);
			}
			desc >>= 1;
			start_root++;
		}
		return;
	case ROOT_DESC_COMPLEX: {
		gsize *bitmap_data = complex_descriptors + (desc >> ROOT_DESC_TYPE_SHIFT);
		int bwords = (*bitmap_data) - 1;
		void **start_run = start_root;
		bitmap_data++;
		while (bwords-- > 0) {
			gsize bmap = *bitmap_data++;
			void **objptr = start_run;
			while (bmap) {
				if ((bmap & 1) && *objptr) {
					copy_func (objptr, queue);
					DEBUG (9, fprintf (gc_debug_file, "Overwrote root at %p with %p\n", objptr, *objptr));
					drain_gray_stack (queue, -1);
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
		MonoGCRootMarkFunc marker = user_descriptors [desc >> ROOT_DESC_TYPE_SHIFT];
		set_user_copy_or_mark_data (&data);
		marker (start_root, single_arg_user_copy_or_mark);
		set_user_copy_or_mark_data (NULL);
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
mono_sgen_update_heap_boundaries (mword low, mword high)
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

static unsigned long
prot_flags_for_activate (int activate)
{
	unsigned long prot_flags = activate? MONO_MMAP_READ|MONO_MMAP_WRITE: MONO_MMAP_NONE;
	return prot_flags | MONO_MMAP_PRIVATE | MONO_MMAP_ANON;
}

/*
 * Allocate a big chunk of memory from the OS (usually 64KB to several megabytes).
 * This must not require any lock.
 */
void*
mono_sgen_alloc_os_memory (size_t size, int activate)
{
	void *ptr = mono_valloc (0, size, prot_flags_for_activate (activate));
	if (ptr) {
		/* FIXME: CAS */
		total_alloc += size;
	}
	return ptr;
}

/* size must be a power of 2 */
void*
mono_sgen_alloc_os_memory_aligned (mword size, mword alignment, gboolean activate)
{
	void *ptr = mono_valloc_aligned (size, alignment, prot_flags_for_activate (activate));
	if (ptr) {
		/* FIXME: CAS */
		total_alloc += size;
	}
	return ptr;
}

/*
 * Free the memory returned by mono_sgen_alloc_os_memory (), returning it to the OS.
 */
void
mono_sgen_free_os_memory (void *addr, size_t size)
{
	mono_vfree (addr, size);
	/* FIXME: CAS */
	total_alloc -= size;
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
	int scan_starts;
	int alloc_size;

	if (nursery_section)
		return;
	DEBUG (2, fprintf (gc_debug_file, "Allocating nursery size: %lu\n", (unsigned long)nursery_size));
	/* later we will alloc a larger area for the nursery but only activate
	 * what we need. The rest will be used as expansion if we have too many pinned
	 * objects in the existing nursery.
	 */
	/* FIXME: handle OOM */
	section = mono_sgen_alloc_internal (INTERNAL_MEM_SECTION);

	g_assert (nursery_size == DEFAULT_NURSERY_SIZE);
	alloc_size = nursery_size;
#ifdef SGEN_ALIGN_NURSERY
	data = major_collector.alloc_heap (alloc_size, alloc_size, DEFAULT_NURSERY_BITS);
#else
	data = major_collector.alloc_heap (alloc_size, 0, DEFAULT_NURSERY_BITS);
#endif
	nursery_start = data;
	nursery_end = nursery_start + nursery_size;
	mono_sgen_update_heap_boundaries ((mword)nursery_start, (mword)nursery_end);
	DEBUG (4, fprintf (gc_debug_file, "Expanding nursery size (%p-%p): %lu, total: %lu\n", data, data + alloc_size, (unsigned long)nursery_size, (unsigned long)total_alloc));
	section->data = section->next_data = data;
	section->size = alloc_size;
	section->end_data = nursery_end;
	scan_starts = (alloc_size + SCAN_START_SIZE - 1) / SCAN_START_SIZE;
	section->scan_starts = mono_sgen_alloc_internal_dynamic (sizeof (char*) * scan_starts, INTERNAL_MEM_SCAN_STARTS);
	section->num_scan_start = scan_starts;
	section->block.role = MEMORY_ROLE_GEN0;
	section->block.next = NULL;

	nursery_section = section;

	mono_sgen_nursery_allocator_set_nursery_bounds (nursery_start, nursery_end);
}

void*
mono_gc_get_nursery (int *shift_bits, size_t *size)
{
	*size = nursery_size;
#ifdef SGEN_ALIGN_NURSERY
	*shift_bits = DEFAULT_NURSERY_BITS;
#else
	*shift_bits = -1;
#endif
	return nursery_start;
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
	return mono_sgen_get_logfile ();
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
single_arg_report_root (void **obj)
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
		gsize *bitmap_data = complex_descriptors + (desc >> ROOT_DESC_TYPE_SHIFT);
		int bwords = (*bitmap_data) - 1;
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
		MonoGCRootMarkFunc marker = user_descriptors [desc >> ROOT_DESC_TYPE_SHIFT];
		root_report = report;
		marker (start_root, single_arg_report_root);
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
		DEBUG (6, fprintf (gc_debug_file, "Precise root scan %p-%p (desc: %p)\n", start_root, root->end_root, (void*)root->root_desc));
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
scan_finalizer_entries (CopyOrMarkObjectFunc copy_func, FinalizeReadyEntry *list, GrayQueue *queue)
{
	FinalizeReadyEntry *fin;

	for (fin = list; fin; fin = fin->next) {
		if (!fin->object)
			continue;
		DEBUG (5, fprintf (gc_debug_file, "Scan of fin ready object: %p (%s)\n", fin->object, safe_name (fin->object)));
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

static MonoObject **finalized_array = NULL;
static int finalized_array_capacity = 0;
static int finalized_array_entries = 0;

static void
bridge_register_finalized_object (MonoObject *object)
{
	if (!finalized_array)
		return;

	if (finalized_array_entries >= finalized_array_capacity) {
		MonoObject **new_array;
		g_assert (finalized_array_entries == finalized_array_capacity);
		finalized_array_capacity *= 2;
		new_array = mono_sgen_alloc_internal_dynamic (sizeof (MonoObject*) * finalized_array_capacity, INTERNAL_MEM_BRIDGE_DATA);
		memcpy (new_array, finalized_array, sizeof (MonoObject*) * finalized_array_entries);
		mono_sgen_free_internal_dynamic (finalized_array, sizeof (MonoObject*) * finalized_array_entries, INTERNAL_MEM_BRIDGE_DATA);
		finalized_array = new_array;
	}
	finalized_array [finalized_array_entries++] = object;
}

static void
bridge_process (void)
{
	if (finalized_array_entries <= 0)
		return;

	g_assert (mono_sgen_need_bridge_processing ());
	mono_sgen_bridge_processing_finish (finalized_array_entries, finalized_array);

	finalized_array_entries = 0;
}

CopyOrMarkObjectFunc
mono_sgen_get_copy_object (void)
{
	if (current_collection_generation == GENERATION_NURSERY) {
		if (collection_is_parallel ())
			return major_collector.copy_object;
		else
			return major_collector.nopar_copy_object;
	} else {
		return major_collector.copy_or_mark_object;
	}
}

ScanObjectFunc
mono_sgen_get_minor_scan_object (void)
{
	g_assert (current_collection_generation == GENERATION_NURSERY);

	if (collection_is_parallel ())
		return major_collector.minor_scan_object;
	else
		return major_collector.nopar_minor_scan_object;
}

ScanVTypeFunc
mono_sgen_get_minor_scan_vtype (void)
{
	g_assert (current_collection_generation == GENERATION_NURSERY);

	if (collection_is_parallel ())
		return major_collector.minor_scan_vtype;
	else
		return major_collector.nopar_minor_scan_vtype;
}

static void
finish_gray_stack (char *start_addr, char *end_addr, int generation, GrayQueue *queue)
{
	TV_DECLARE (atv);
	TV_DECLARE (btv);
	int fin_ready;
	int done_with_ephemerons, ephemeron_rounds = 0;
	int num_loops;
	CopyOrMarkObjectFunc copy_func = mono_sgen_get_copy_object ();

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
	drain_gray_stack (queue, -1);
	TV_GETTIME (atv);
	DEBUG (2, fprintf (gc_debug_file, "%s generation done\n", generation_name (generation)));

	/*
	 * Walk the ephemeron tables marking all values with reachable keys. This must be completely done
	 * before processing finalizable objects or non-tracking weak hamdle to avoid finalizing/clearing
	 * objects that are in fact reachable.
	 */
	done_with_ephemerons = 0;
	do {
		done_with_ephemerons = mark_ephemerons_in_range (copy_func, start_addr, end_addr, queue);
		drain_gray_stack (queue, -1);
		++ephemeron_rounds;
	} while (!done_with_ephemerons);

	mono_sgen_scan_togglerefs (copy_func, start_addr, end_addr, queue);
	if (generation == GENERATION_OLD)
		mono_sgen_scan_togglerefs (copy_func, nursery_start, nursery_end, queue);

	if (mono_sgen_need_bridge_processing ()) {
		if (finalized_array == NULL) {
			finalized_array_capacity = 32;
			finalized_array = mono_sgen_alloc_internal_dynamic (sizeof (MonoObject*) * finalized_array_capacity, INTERNAL_MEM_BRIDGE_DATA);
		}
		finalized_array_entries = 0;		

		collect_bridge_objects (copy_func, start_addr, end_addr, generation, queue);
		if (generation == GENERATION_OLD)
			collect_bridge_objects (copy_func, nursery_start, nursery_end, GENERATION_NURSERY, queue);

		if (finalized_array_entries > 0)
			mono_sgen_bridge_processing_start (finalized_array_entries, finalized_array);
		drain_gray_stack (queue, -1);
	}

	/*
	We must clear weak links that don't track resurrection before processing object ready for
	finalization so they can be cleared before that.
	*/
	null_link_in_range (copy_func, start_addr, end_addr, generation, TRUE, queue);
	if (generation == GENERATION_OLD)
		null_link_in_range (copy_func, start_addr, end_addr, GENERATION_NURSERY, TRUE, queue);


	/* walk the finalization queue and move also the objects that need to be
	 * finalized: use the finalized objects as new roots so the objects they depend
	 * on are also not reclaimed. As with the roots above, only objects in the nursery
	 * are marked/copied.
	 * We need a loop here, since objects ready for finalizers may reference other objects
	 * that are fin-ready. Speedup with a flag?
	 */
	num_loops = 0;
	do {		
		fin_ready = num_ready_finalizers;
		finalize_in_range (copy_func, start_addr, end_addr, generation, queue);
		if (generation == GENERATION_OLD)
			finalize_in_range (copy_func, nursery_start, nursery_end, GENERATION_NURSERY, queue);

		if (fin_ready != num_ready_finalizers)
			++num_loops;

		/* drain the new stack that might have been created */
		DEBUG (6, fprintf (gc_debug_file, "Precise scan of gray area post fin\n"));
		drain_gray_stack (queue, -1);
	} while (fin_ready != num_ready_finalizers);

	if (mono_sgen_need_bridge_processing ())
		g_assert (num_loops <= 1);

	/*
	 * This must be done again after processing finalizable objects since CWL slots are cleared only after the key is finalized.
	 */
	done_with_ephemerons = 0;
	do {
		done_with_ephemerons = mark_ephemerons_in_range (copy_func, start_addr, end_addr, queue);
		drain_gray_stack (queue, -1);
		++ephemeron_rounds;
	} while (!done_with_ephemerons);

	/*
	 * Clear ephemeron pairs with unreachable keys.
	 * We pass the copy func so we can figure out if an array was promoted or not.
	 */
	clear_unreachable_ephemerons (copy_func, start_addr, end_addr, queue);

	TV_GETTIME (btv);
	DEBUG (2, fprintf (gc_debug_file, "Finalize queue handling scan for %s generation: %d usecs %d ephemeron roundss\n", generation_name (generation), TV_ELAPSED (atv, btv), ephemeron_rounds));

	/*
	 * handle disappearing links
	 * Note we do this after checking the finalization queue because if an object
	 * survives (at least long enough to be finalized) we don't clear the link.
	 * This also deals with a possible issue with the monitor reclamation: with the Boehm
	 * GC a finalized object my lose the monitor because it is cleared before the finalizer is
	 * called.
	 */
	g_assert (gray_object_queue_is_empty (queue));
	for (;;) {
		null_link_in_range (copy_func, start_addr, end_addr, generation, FALSE, queue);
		if (generation == GENERATION_OLD)
			null_link_in_range (copy_func, start_addr, end_addr, GENERATION_NURSERY, FALSE, queue);
		if (gray_object_queue_is_empty (queue))
			break;
		drain_gray_stack (queue, -1);
	}

	g_assert (gray_object_queue_is_empty (queue));
}

void
mono_sgen_check_section_scan_starts (GCMemSection *section)
{
	int i;
	for (i = 0; i < section->num_scan_start; ++i) {
		if (section->scan_starts [i]) {
			guint size = safe_object_get_size ((MonoObject*) section->scan_starts [i]);
			g_assert (size >= sizeof (MonoObject) && size <= MAX_SMALL_OBJ_SIZE);
		}
	}
}

static void
check_scan_starts (void)
{
	if (!do_scan_starts_check)
		return;
	mono_sgen_check_section_scan_starts (nursery_section);
	major_collector.check_scan_starts ();
}

static int last_num_pinned = 0;

static void
scan_from_registered_roots (CopyOrMarkObjectFunc copy_func, char *addr_start, char *addr_end, int root_type, GrayQueue *queue)
{
	void **start_root;
	RootRecord *root;
	SGEN_HASH_TABLE_FOREACH (&roots_hash [root_type], start_root, root) {
		DEBUG (6, fprintf (gc_debug_file, "Precise root scan %p-%p (desc: %p)\n", start_root, root->end_root, (void*)root->root_desc));
		precisely_scan_objects_from (copy_func, start_root, (void**)root->end_root, addr_start, addr_end, root->root_desc, queue);
	} SGEN_HASH_TABLE_FOREACH_END;
}

void
mono_sgen_dump_occupied (char *start, char *end, char *section_start)
{
	fprintf (heap_dump_file, "<occupied offset=\"%td\" size=\"%td\"/>\n", start - section_start, end - start);
}

void
mono_sgen_dump_section (GCMemSection *section, const char *type)
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
				mono_sgen_dump_occupied (occ_start, start, section->data);
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
		mono_sgen_dump_occupied (occ_start, start, section->data);

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

	fprintf (heap_dump_file, "<object class=\"%s.%s\" size=\"%d\"",
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
	mono_sgen_dump_internal_mem_usage (heap_dump_file);
	fprintf (heap_dump_file, "<pinned type=\"stack\" bytes=\"%zu\"/>\n", pinned_byte_counts [PIN_TYPE_STACK]);
	/* fprintf (heap_dump_file, "<pinned type=\"static-data\" bytes=\"%d\"/>\n", pinned_byte_counts [PIN_TYPE_STATIC_DATA]); */
	fprintf (heap_dump_file, "<pinned type=\"other\" bytes=\"%zu\"/>\n", pinned_byte_counts [PIN_TYPE_OTHER]);

	fprintf (heap_dump_file, "<pinned-objects>\n");
	for (list = pinned_objects; list; list = list->next)
		dump_object (list->obj, TRUE);
	fprintf (heap_dump_file, "</pinned-objects>\n");

	mono_sgen_dump_section (nursery_section, "nursery");

	major_collector.dump_heap (heap_dump_file);

	fprintf (heap_dump_file, "<los>\n");
	for (bigobj = los_object_list; bigobj; bigobj = bigobj->next)
		dump_object ((MonoObject*)bigobj->data, FALSE);
	fprintf (heap_dump_file, "</los>\n");

	fprintf (heap_dump_file, "</collection>\n");
}

void
mono_sgen_register_moved_object (void *obj, void *destination)
{
	g_assert (mono_profiler_events & MONO_PROFILE_GC_MOVES);

	/* FIXME: handle this for parallel collector */
	g_assert (!collection_is_parallel ());

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

	mono_counters_register ("Minor fragment clear", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_minor_pre_collection_fragment_clear);
	mono_counters_register ("Minor pinning", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_minor_pinning);
	mono_counters_register ("Minor scan remsets", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_minor_scan_remsets);
	mono_counters_register ("Minor scan cardtables", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_minor_scan_card_table);
	mono_counters_register ("Minor scan pinned", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_minor_scan_pinned);
	mono_counters_register ("Minor scan registered roots", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_minor_scan_registered_roots);
	mono_counters_register ("Minor scan thread data", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_minor_scan_thread_data);
	mono_counters_register ("Minor finish gray stack", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_minor_finish_gray_stack);
	mono_counters_register ("Minor fragment creation", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_minor_fragment_creation);

	mono_counters_register ("Major fragment clear", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_major_pre_collection_fragment_clear);
	mono_counters_register ("Major pinning", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_major_pinning);
	mono_counters_register ("Major scan pinned", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_major_scan_pinned);
	mono_counters_register ("Major scan registered roots", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_major_scan_registered_roots);
	mono_counters_register ("Major scan thread data", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_major_scan_thread_data);
	mono_counters_register ("Major scan alloc_pinned", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_major_scan_alloc_pinned);
	mono_counters_register ("Major scan finalized", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_major_scan_finalized);
	mono_counters_register ("Major scan big objects", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_major_scan_big_objects);
	mono_counters_register ("Major finish gray stack", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_major_finish_gray_stack);
	mono_counters_register ("Major free big objects", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_major_free_bigobjs);
	mono_counters_register ("Major LOS sweep", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_major_los_sweep);
	mono_counters_register ("Major sweep", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_major_sweep);
	mono_counters_register ("Major fragment creation", MONO_COUNTER_GC | MONO_COUNTER_LONG, &time_major_fragment_creation);

	mono_counters_register ("Number of pinned objects", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_pinned_objects);

#ifdef HEAVY_STATISTICS
	mono_counters_register ("WBarrier set field", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_set_field);
	mono_counters_register ("WBarrier set arrayref", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_set_arrayref);
	mono_counters_register ("WBarrier arrayref copy", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_arrayref_copy);
	mono_counters_register ("WBarrier generic store called", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_generic_store);
	mono_counters_register ("WBarrier generic store stored", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_generic_store_remset);
	mono_counters_register ("WBarrier set root", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_set_root);
	mono_counters_register ("WBarrier value copy", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_value_copy);
	mono_counters_register ("WBarrier object copy", MONO_COUNTER_GC | MONO_COUNTER_INT, &stat_wbarrier_object_copy);

	mono_counters_register ("# objects allocated", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_objects_alloced);
	mono_counters_register ("bytes allocated", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_bytes_alloced);
	mono_counters_register ("# objects allocated degraded", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_objects_alloced_degraded);
	mono_counters_register ("bytes allocated degraded", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_bytes_alloced_degraded);
	mono_counters_register ("bytes allocated in LOS", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_bytes_alloced_los);

	mono_counters_register ("# copy_object() called (nursery)", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_copy_object_called_nursery);
	mono_counters_register ("# objects copied (nursery)", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_objects_copied_nursery);
	mono_counters_register ("# copy_object() called (major)", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_copy_object_called_major);
	mono_counters_register ("# objects copied (major)", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_objects_copied_major);

	mono_counters_register ("# scan_object() called (nursery)", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_scan_object_called_nursery);
	mono_counters_register ("# scan_object() called (major)", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_scan_object_called_major);

	mono_counters_register ("# nursery copy_object() failed from space", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_nursery_copy_object_failed_from_space);
	mono_counters_register ("# nursery copy_object() failed forwarded", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_nursery_copy_object_failed_forwarded);
	mono_counters_register ("# nursery copy_object() failed pinned", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_nursery_copy_object_failed_pinned);

	mono_sgen_nursery_allocator_init_heavy_stats ();

	mono_counters_register ("Store remsets", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_store_remsets);
	mono_counters_register ("Unique store remsets", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_store_remsets_unique);
	mono_counters_register ("Saved remsets 1", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_saved_remsets_1);
	mono_counters_register ("Saved remsets 2", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_saved_remsets_2);
	mono_counters_register ("Non-global remsets processed", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_local_remsets_processed);
	mono_counters_register ("Global remsets added", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_global_remsets_added);
	mono_counters_register ("Global remsets re-added", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_global_remsets_readded);
	mono_counters_register ("Global remsets processed", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_global_remsets_processed);
	mono_counters_register ("Global remsets discarded", MONO_COUNTER_GC | MONO_COUNTER_LONG, &stat_global_remsets_discarded);
#endif

	inited = TRUE;
}

static gboolean need_calculate_minor_collection_allowance;

static int last_collection_old_num_major_sections;
static mword last_collection_los_memory_usage = 0;
static mword last_collection_old_los_memory_usage;
static mword last_collection_los_memory_alloced;

static void
reset_minor_collection_allowance (void)
{
	need_calculate_minor_collection_allowance = TRUE;
}

static void
try_calculate_minor_collection_allowance (gboolean overwrite)
{
	int num_major_sections, num_major_sections_saved, save_target, allowance_target;
	mword los_memory_saved, new_major, new_heap_size;

	if (overwrite)
		g_assert (need_calculate_minor_collection_allowance);

	if (!need_calculate_minor_collection_allowance)
		return;

	if (!*major_collector.have_swept) {
		if (overwrite)
			minor_collection_allowance = MIN_MINOR_COLLECTION_ALLOWANCE;
		return;
	}

	num_major_sections = major_collector.get_num_major_sections ();

	num_major_sections_saved = MAX (last_collection_old_num_major_sections - num_major_sections, 0);
	los_memory_saved = MAX (last_collection_old_los_memory_usage - last_collection_los_memory_usage, 1);

	new_major = num_major_sections * major_collector.section_size;
	new_heap_size = new_major + last_collection_los_memory_usage;

	/*
	 * FIXME: Why is save_target half the major memory plus half the
	 * LOS memory saved?  Shouldn't it be half the major memory
	 * saved plus half the LOS memory saved?  Or half the whole heap
	 * size?
	 */
	save_target = (new_major + los_memory_saved) / 2;

	/*
	 * We aim to allow the allocation of as many sections as is
	 * necessary to reclaim save_target sections in the next
	 * collection.  We assume the collection pattern won't change.
	 * In the last cycle, we had num_major_sections_saved for
	 * minor_collection_sections_alloced.  Assuming things won't
	 * change, this must be the same ratio as save_target for
	 * allowance_target, i.e.
	 *
	 *    num_major_sections_saved            save_target
	 * --------------------------------- == ----------------
	 * minor_collection_sections_alloced    allowance_target
	 *
	 * hence:
	 */
	allowance_target = (mword)((double)save_target * (double)(minor_collection_sections_alloced * major_collector.section_size + last_collection_los_memory_alloced) / (double)(num_major_sections_saved * major_collector.section_size + los_memory_saved));

	minor_collection_allowance = MAX (MIN (allowance_target, num_major_sections * major_collector.section_size + los_memory_usage), MIN_MINOR_COLLECTION_ALLOWANCE);

	if (new_heap_size + minor_collection_allowance > soft_heap_limit) {
		if (new_heap_size > soft_heap_limit)
			minor_collection_allowance = MIN_MINOR_COLLECTION_ALLOWANCE;
		else
			minor_collection_allowance = MAX (soft_heap_limit - new_heap_size, MIN_MINOR_COLLECTION_ALLOWANCE);
	}

	if (debug_print_allowance) {
		mword old_major = last_collection_old_num_major_sections * major_collector.section_size;

		fprintf (gc_debug_file, "Before collection: %ld bytes (%ld major, %ld LOS)\n",
				old_major + last_collection_old_los_memory_usage, old_major, last_collection_old_los_memory_usage);
		fprintf (gc_debug_file, "After collection: %ld bytes (%ld major, %ld LOS)\n",
				new_heap_size, new_major, last_collection_los_memory_usage);
		fprintf (gc_debug_file, "Allowance: %ld bytes\n", minor_collection_allowance);
	}

	if (major_collector.have_computed_minor_collection_allowance)
		major_collector.have_computed_minor_collection_allowance ();

	need_calculate_minor_collection_allowance = FALSE;
}

static gboolean
need_major_collection (mword space_needed)
{
	mword los_alloced = los_memory_usage - MIN (last_collection_los_memory_usage, los_memory_usage);
	return (space_needed > available_free_space ()) ||
		minor_collection_sections_alloced * major_collector.section_size + los_alloced > minor_collection_allowance;
}

gboolean
mono_sgen_need_major_collection (mword space_needed)
{
	return need_major_collection (space_needed);
}

static void
reset_pinned_from_failed_allocation (void)
{
	bytes_pinned_from_failed_allocation = 0;
}

void
mono_sgen_set_pinned_from_failed_allocation (mword objsize)
{
	bytes_pinned_from_failed_allocation += objsize;
}

static gboolean
collection_is_parallel (void)
{
	switch (current_collection_generation) {
	case GENERATION_NURSERY:
		return nursery_collection_is_parallel;
	case GENERATION_OLD:
		return major_collector.is_parallel;
	default:
		g_assert_not_reached ();
	}
}

gboolean
mono_sgen_nursery_collection_is_parallel (void)
{
	return nursery_collection_is_parallel;
}

static GrayQueue*
job_gray_queue (WorkerData *worker_data)
{
	return worker_data ? &worker_data->private_gray_queue : WORKERS_DISTRIBUTE_GRAY_QUEUE;
}

typedef struct
{
	char *heap_start;
	char *heap_end;
} ScanFromRemsetsJobData;

static void
job_scan_from_remsets (WorkerData *worker_data, void *job_data_untyped)
{
	ScanFromRemsetsJobData *job_data = job_data_untyped;

	scan_from_remsets (job_data->heap_start, job_data->heap_end, job_gray_queue (worker_data));
}

typedef struct
{
	CopyOrMarkObjectFunc func;
	char *heap_start;
	char *heap_end;
	int root_type;
} ScanFromRegisteredRootsJobData;

static void
job_scan_from_registered_roots (WorkerData *worker_data, void *job_data_untyped)
{
	ScanFromRegisteredRootsJobData *job_data = job_data_untyped;

	scan_from_registered_roots (job_data->func,
			job_data->heap_start, job_data->heap_end,
			job_data->root_type,
			job_gray_queue (worker_data));
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
			job_gray_queue (worker_data));
}

static void
verify_scan_starts (char *start, char *end)
{
	int i;

	for (i = 0; i < nursery_section->num_scan_start; ++i) {
		char *addr = nursery_section->scan_starts [i];
		if (addr > start && addr < end)
			fprintf (gc_debug_file, "NFC-BAD SCAN START [%d] %p for obj [%p %p]\n", i, addr, start, end);
	}
}

static void
verify_nursery (void)
{
	char *start, *end, *cur, *hole_start;

	if (!do_verify_nursery)
		return;

	/*This cleans up unused fragments */
	mono_sgen_nursery_allocator_prepare_for_pinning ();

	hole_start = start = cur = nursery_start;
	end = nursery_end;

	while (cur < end) {
		if (!*(void**)cur) {
			cur += sizeof (void*);
			continue;
		}

		if (object_is_forwarded (cur))
			fprintf (gc_debug_file, "FORWARDED OBJ %p\n", cur);
		else if (object_is_pinned (cur))
			fprintf (gc_debug_file, "PINNED OBJ %p\n", cur);

		size_t ss = safe_object_get_size ((MonoObject*)cur);
		size_t size = ALIGN_UP (safe_object_get_size ((MonoObject*)cur));
		verify_scan_starts (cur, cur + size);
		if (do_dump_nursery_content) {
			if (cur > hole_start)
				fprintf (gc_debug_file, "HOLE [%p %p %d]\n", hole_start, cur, cur - hole_start);
			fprintf (gc_debug_file, "OBJ  [%p %p %d %d %s %d]\n", cur, cur + size, size, ss, mono_sgen_safe_name ((MonoObject*)cur), LOAD_VTABLE (cur) == mono_sgen_get_array_fill_vtable ());
		}
		cur += size;
		hole_start = cur;
	}
	fflush (gc_debug_file);
}

/*
 * Collect objects in the nursery.  Returns whether to trigger a major
 * collection.
 */
static gboolean
collect_nursery (size_t requested_size)
{
	gboolean needs_major;
	size_t max_garbage_amount;
	char *nursery_next;
	ScanFromRemsetsJobData sfrjd;
	ScanFromRegisteredRootsJobData scrrjd_normal, scrrjd_wbarrier;
	ScanThreadDataJobData stdjd;
	mword fragment_total;
	TV_DECLARE (all_atv);
	TV_DECLARE (all_btv);
	TV_DECLARE (atv);
	TV_DECLARE (btv);

	if (disable_minor_collections)
		return TRUE;

	verify_nursery ();

	mono_perfcounters->gc_collections0++;

	current_collection_generation = GENERATION_NURSERY;

	reset_pinned_from_failed_allocation ();

	binary_protocol_collection (GENERATION_NURSERY);
	check_scan_starts ();

	degraded_mode = 0;
	objects_pinned = 0;
	nursery_next = mono_sgen_nursery_alloc_get_upper_alloc_bound ();
	/* FIXME: optimize later to use the higher address where an object can be present */
	nursery_next = MAX (nursery_next, nursery_end);

	nursery_alloc_bound = nursery_next;

	DEBUG (1, fprintf (gc_debug_file, "Start nursery collection %d %p-%p, size: %d\n", num_minor_gcs, nursery_start, nursery_next, (int)(nursery_next - nursery_start)));
	max_garbage_amount = nursery_next - nursery_start;
	g_assert (nursery_section->size >= max_garbage_amount);

	/* world must be stopped already */
	TV_GETTIME (all_atv);
	atv = all_atv;

	/* Pinning no longer depends on clearing all nursery fragments */
	mono_sgen_clear_current_nursery_fragment ();

	TV_GETTIME (btv);
	time_minor_pre_collection_fragment_clear += TV_ELAPSED_MS (atv, btv);

	if (xdomain_checks)
		check_for_xdomain_refs ();

	nursery_section->next_data = nursery_next;

	major_collector.start_nursery_collection ();

	try_calculate_minor_collection_allowance (FALSE);

	gray_object_queue_init (&gray_queue);
	workers_init_distribute_gray_queue ();

	num_minor_gcs++;
	mono_stats.minor_gc_count ++;

	global_remset_cache_clear ();

	process_fin_stage_entries ();
	process_dislink_stage_entries ();

	/* pin from pinned handles */
	init_pinning ();
	mono_profiler_gc_event (MONO_GC_EVENT_MARK_START, 0);
	pin_from_roots (nursery_start, nursery_next, WORKERS_DISTRIBUTE_GRAY_QUEUE);
	/* identify pinned objects */
	optimize_pin_queue (0);
	next_pin_slot = pin_objects_from_addresses (nursery_section, pin_queue, pin_queue + next_pin_slot, nursery_start, nursery_next, WORKERS_DISTRIBUTE_GRAY_QUEUE);
	nursery_section->pin_queue_start = pin_queue;
	nursery_section->pin_queue_num_entries = next_pin_slot;
	TV_GETTIME (atv);
	time_minor_pinning += TV_ELAPSED_MS (btv, atv);
	DEBUG (2, fprintf (gc_debug_file, "Finding pinned pointers: %d in %d usecs\n", next_pin_slot, TV_ELAPSED (btv, atv)));
	DEBUG (4, fprintf (gc_debug_file, "Start scan with %d pinned objects\n", next_pin_slot));

	if (consistency_check_at_minor_collection)
		check_consistency ();

	workers_start_all_workers ();

	/*
	 * Walk all the roots and copy the young objects to the old
	 * generation, starting from to_space.
	 *
	 * The global remsets must be processed before the workers start
	 * marking because they might add global remsets.
	 */
	scan_from_global_remsets (nursery_start, nursery_next, WORKERS_DISTRIBUTE_GRAY_QUEUE);

	workers_start_marking ();

	sfrjd.heap_start = nursery_start;
	sfrjd.heap_end = nursery_next;
	workers_enqueue_job (job_scan_from_remsets, &sfrjd);

	/* we don't have complete write barrier yet, so we scan all the old generation sections */
	TV_GETTIME (btv);
	time_minor_scan_remsets += TV_ELAPSED_MS (atv, btv);
	DEBUG (2, fprintf (gc_debug_file, "Old generation scan: %d usecs\n", TV_ELAPSED (atv, btv)));

	if (use_cardtable) {
		atv = btv;
		card_tables_collect_stats (TRUE);
		scan_from_card_tables (nursery_start, nursery_next, WORKERS_DISTRIBUTE_GRAY_QUEUE);
		TV_GETTIME (btv);
		time_minor_scan_card_table += TV_ELAPSED_MS (atv, btv);
	}

	if (!collection_is_parallel ())
		drain_gray_stack (&gray_queue, -1);

	if (mono_profiler_get_events () & MONO_PROFILE_GC_ROOTS)
		report_registered_roots ();
	if (mono_profiler_get_events () & MONO_PROFILE_GC_ROOTS)
		report_finalizer_roots ();
	TV_GETTIME (atv);
	time_minor_scan_pinned += TV_ELAPSED_MS (btv, atv);

	/* registered roots, this includes static fields */
	scrrjd_normal.func = collection_is_parallel () ? major_collector.copy_object : major_collector.nopar_copy_object;
	scrrjd_normal.heap_start = nursery_start;
	scrrjd_normal.heap_end = nursery_next;
	scrrjd_normal.root_type = ROOT_TYPE_NORMAL;
	workers_enqueue_job (job_scan_from_registered_roots, &scrrjd_normal);

	scrrjd_wbarrier.func = collection_is_parallel () ? major_collector.copy_object : major_collector.nopar_copy_object;
	scrrjd_wbarrier.heap_start = nursery_start;
	scrrjd_wbarrier.heap_end = nursery_next;
	scrrjd_wbarrier.root_type = ROOT_TYPE_WBARRIER;
	workers_enqueue_job (job_scan_from_registered_roots, &scrrjd_wbarrier);

	TV_GETTIME (btv);
	time_minor_scan_registered_roots += TV_ELAPSED_MS (atv, btv);

	/* thread data */
	stdjd.heap_start = nursery_start;
	stdjd.heap_end = nursery_next;
	workers_enqueue_job (job_scan_thread_data, &stdjd);

	TV_GETTIME (atv);
	time_minor_scan_thread_data += TV_ELAPSED_MS (btv, atv);
	btv = atv;

	if (collection_is_parallel ()) {
		while (!gray_object_queue_is_empty (WORKERS_DISTRIBUTE_GRAY_QUEUE)) {
			workers_distribute_gray_queue_sections ();
			g_usleep (1000);
		}
	}
	workers_join ();

	if (collection_is_parallel ())
		g_assert (gray_object_queue_is_empty (&gray_queue));

	finish_gray_stack (nursery_start, nursery_next, GENERATION_NURSERY, &gray_queue);
	TV_GETTIME (atv);
	time_minor_finish_gray_stack += TV_ELAPSED_MS (btv, atv);
	mono_profiler_gc_event (MONO_GC_EVENT_MARK_END, 0);

	/*
	 * The (single-threaded) finalization code might have done
	 * some copying/marking so we can only reset the GC thread's
	 * worker data here instead of earlier when we joined the
	 * workers.
	 */
	if (major_collector.reset_worker_data)
		major_collector.reset_worker_data (workers_gc_thread_data.major_collector_data);

	if (objects_pinned) {
		evacuate_pin_staging_area ();
		optimize_pin_queue (0);
		nursery_section->pin_queue_start = pin_queue;
		nursery_section->pin_queue_num_entries = next_pin_slot;
	}

	/* walk the pin_queue, build up the fragment list of free memory, unmark
	 * pinned objects as we go, memzero() the empty fragments so they are ready for the
	 * next allocations.
	 */
	mono_profiler_gc_event (MONO_GC_EVENT_RECLAIM_START, 0);
	fragment_total = mono_sgen_build_nursery_fragments (nursery_section, pin_queue, next_pin_slot);
	if (!fragment_total)
		degraded_mode = 1;

	/* Clear TLABs for all threads */
	clear_tlabs ();

	mono_profiler_gc_event (MONO_GC_EVENT_RECLAIM_END, 0);
	TV_GETTIME (btv);
	time_minor_fragment_creation += TV_ELAPSED_MS (atv, btv);
	DEBUG (2, fprintf (gc_debug_file, "Fragment creation: %d usecs, %lu bytes available\n", TV_ELAPSED (atv, btv), (unsigned long)fragment_total));

	if (consistency_check_at_minor_collection)
		check_major_refs ();

	major_collector.finish_nursery_collection ();

	TV_GETTIME (all_btv);
	mono_stats.minor_gc_time_usecs += TV_ELAPSED (all_atv, all_btv);

	if (heap_dump_file)
		dump_heap ("minor", num_minor_gcs - 1, NULL);

	/* prepare the pin queue for the next collection */
	last_num_pinned = next_pin_slot;
	next_pin_slot = 0;
	if (fin_ready_list || critical_fin_list) {
		DEBUG (4, fprintf (gc_debug_file, "Finalizer-thread wakeup: ready %d\n", num_ready_finalizers));
		mono_gc_finalize_notify ();
	}
	pin_stats_reset ();

	g_assert (gray_object_queue_is_empty (&gray_queue));

	if (use_cardtable)
		card_tables_collect_stats (FALSE);

	check_scan_starts ();

	binary_protocol_flush_buffers (FALSE);

	/*objects are late pinned because of lack of memory, so a major is a good call*/
	needs_major = need_major_collection (0) || objects_pinned;
	current_collection_generation = -1;
	objects_pinned = 0;

	return needs_major;
}

typedef struct
{
	FinalizeReadyEntry *list;
} ScanFinalizerEntriesJobData;

static void
job_scan_finalizer_entries (WorkerData *worker_data, void *job_data_untyped)
{
	ScanFinalizerEntriesJobData *job_data = job_data_untyped;

	scan_finalizer_entries (major_collector.copy_or_mark_object,
			job_data->list,
			job_gray_queue (worker_data));
}

static gboolean
major_do_collection (const char *reason)
{
	LOSObject *bigobj, *prevbo;
	TV_DECLARE (all_atv);
	TV_DECLARE (all_btv);
	TV_DECLARE (atv);
	TV_DECLARE (btv);
	/* FIXME: only use these values for the precise scan
	 * note that to_space pointers should be excluded anyway...
	 */
	char *heap_start = NULL;
	char *heap_end = (char*)-1;
	int old_next_pin_slot;
	ScanFromRegisteredRootsJobData scrrjd_normal, scrrjd_wbarrier;
	ScanThreadDataJobData stdjd;
	ScanFinalizerEntriesJobData sfejd_fin_ready, sfejd_critical_fin;

	mono_perfcounters->gc_collections1++;

	reset_pinned_from_failed_allocation ();

	last_collection_old_num_major_sections = major_collector.get_num_major_sections ();

	/*
	 * A domain could have been freed, resulting in
	 * los_memory_usage being less than last_collection_los_memory_usage.
	 */
	last_collection_los_memory_alloced = los_memory_usage - MIN (last_collection_los_memory_usage, los_memory_usage);
	last_collection_old_los_memory_usage = los_memory_usage;
	objects_pinned = 0;

	//count_ref_nonref_objs ();
	//consistency_check ();

	binary_protocol_collection (GENERATION_OLD);
	check_scan_starts ();
	gray_object_queue_init (&gray_queue);
	workers_init_distribute_gray_queue ();

	degraded_mode = 0;
	DEBUG (1, fprintf (gc_debug_file, "Start major collection %d\n", num_major_gcs));
	num_major_gcs++;
	mono_stats.major_gc_count ++;

	/* world must be stopped already */
	TV_GETTIME (all_atv);
	atv = all_atv;

	/* Pinning depends on this */
	mono_sgen_clear_nursery_fragments ();

	TV_GETTIME (btv);
	time_major_pre_collection_fragment_clear += TV_ELAPSED_MS (atv, btv);

	nursery_section->next_data = nursery_end;
	/* we should also coalesce scanning from sections close to each other
	 * and deal with pointers outside of the sections later.
	 */

	if (major_collector.start_major_collection)
		major_collector.start_major_collection ();

	*major_collector.have_swept = FALSE;
	reset_minor_collection_allowance ();

	if (xdomain_checks)
		check_for_xdomain_refs ();

	/* The remsets are not useful for a major collection */
	clear_remsets ();
	global_remset_cache_clear ();
	if (use_cardtable)
		card_table_clear ();

	process_fin_stage_entries ();
	process_dislink_stage_entries ();

	TV_GETTIME (atv);
	init_pinning ();
	DEBUG (6, fprintf (gc_debug_file, "Collecting pinned addresses\n"));
	pin_from_roots ((void*)lowest_heap_address, (void*)highest_heap_address, WORKERS_DISTRIBUTE_GRAY_QUEUE);
	optimize_pin_queue (0);

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
	DEBUG (6, fprintf (gc_debug_file, "Pinning from sections\n"));
	/* first pass for the sections */
	mono_sgen_find_section_pin_queue_start_end (nursery_section);
	major_collector.find_pin_queue_start_ends (WORKERS_DISTRIBUTE_GRAY_QUEUE);
	/* identify possible pointers to the insize of large objects */
	DEBUG (6, fprintf (gc_debug_file, "Pinning from large objects\n"));
	for (bigobj = los_object_list; bigobj; bigobj = bigobj->next) {
		int dummy;
		if (mono_sgen_find_optimized_pin_queue_area (bigobj->data, (char*)bigobj->data + bigobj->size, &dummy)) {
			binary_protocol_pin (bigobj->data, (gpointer)LOAD_VTABLE (bigobj->data), safe_object_get_size (bigobj->data));
			pin_object (bigobj->data);
			/* FIXME: only enqueue if object has references */
			GRAY_OBJECT_ENQUEUE (WORKERS_DISTRIBUTE_GRAY_QUEUE, bigobj->data);
			if (do_pin_stats)
				mono_sgen_pin_stats_register_object ((char*) bigobj->data, safe_object_get_size ((MonoObject*) bigobj->data));
			DEBUG (6, fprintf (gc_debug_file, "Marked large object %p (%s) size: %lu from roots\n", bigobj->data, safe_name (bigobj->data), (unsigned long)bigobj->size));
		}
	}
	/* second pass for the sections */
	mono_sgen_pin_objects_in_section (nursery_section, WORKERS_DISTRIBUTE_GRAY_QUEUE);
	major_collector.pin_objects (WORKERS_DISTRIBUTE_GRAY_QUEUE);
	old_next_pin_slot = next_pin_slot;

	TV_GETTIME (btv);
	time_major_pinning += TV_ELAPSED_MS (atv, btv);
	DEBUG (2, fprintf (gc_debug_file, "Finding pinned pointers: %d in %d usecs\n", next_pin_slot, TV_ELAPSED (atv, btv)));
	DEBUG (4, fprintf (gc_debug_file, "Start scan with %d pinned objects\n", next_pin_slot));

	major_collector.init_to_space ();

#ifdef SGEN_DEBUG_INTERNAL_ALLOC
	main_gc_thread = mono_native_thread_self ();
#endif

	workers_start_all_workers ();
	workers_start_marking ();

	if (mono_profiler_get_events () & MONO_PROFILE_GC_ROOTS)
		report_registered_roots ();
	TV_GETTIME (atv);
	time_major_scan_pinned += TV_ELAPSED_MS (btv, atv);

	/* registered roots, this includes static fields */
	scrrjd_normal.func = major_collector.copy_or_mark_object;
	scrrjd_normal.heap_start = heap_start;
	scrrjd_normal.heap_end = heap_end;
	scrrjd_normal.root_type = ROOT_TYPE_NORMAL;
	workers_enqueue_job (job_scan_from_registered_roots, &scrrjd_normal);

	scrrjd_wbarrier.func = major_collector.copy_or_mark_object;
	scrrjd_wbarrier.heap_start = heap_start;
	scrrjd_wbarrier.heap_end = heap_end;
	scrrjd_wbarrier.root_type = ROOT_TYPE_WBARRIER;
	workers_enqueue_job (job_scan_from_registered_roots, &scrrjd_wbarrier);

	TV_GETTIME (btv);
	time_major_scan_registered_roots += TV_ELAPSED_MS (atv, btv);

	/* Threads */
	stdjd.heap_start = heap_start;
	stdjd.heap_end = heap_end;
	workers_enqueue_job (job_scan_thread_data, &stdjd);

	TV_GETTIME (atv);
	time_major_scan_thread_data += TV_ELAPSED_MS (btv, atv);

	TV_GETTIME (btv);
	time_major_scan_alloc_pinned += TV_ELAPSED_MS (atv, btv);

	if (mono_profiler_get_events () & MONO_PROFILE_GC_ROOTS)
		report_finalizer_roots ();

	/* scan the list of objects ready for finalization */
	sfejd_fin_ready.list = fin_ready_list;
	workers_enqueue_job (job_scan_finalizer_entries, &sfejd_fin_ready);

	sfejd_critical_fin.list = critical_fin_list;
	workers_enqueue_job (job_scan_finalizer_entries, &sfejd_critical_fin);

	TV_GETTIME (atv);
	time_major_scan_finalized += TV_ELAPSED_MS (btv, atv);
	DEBUG (2, fprintf (gc_debug_file, "Root scan: %d usecs\n", TV_ELAPSED (btv, atv)));

	TV_GETTIME (btv);
	time_major_scan_big_objects += TV_ELAPSED_MS (atv, btv);

	if (major_collector.is_parallel) {
		while (!gray_object_queue_is_empty (WORKERS_DISTRIBUTE_GRAY_QUEUE)) {
			workers_distribute_gray_queue_sections ();
			g_usleep (1000);
		}
	}
	workers_join ();

#ifdef SGEN_DEBUG_INTERNAL_ALLOC
	main_gc_thread = NULL;
#endif

	if (major_collector.is_parallel)
		g_assert (gray_object_queue_is_empty (&gray_queue));

	/* all the objects in the heap */
	finish_gray_stack (heap_start, heap_end, GENERATION_OLD, &gray_queue);
	TV_GETTIME (atv);
	time_major_finish_gray_stack += TV_ELAPSED_MS (btv, atv);

	/*
	 * The (single-threaded) finalization code might have done
	 * some copying/marking so we can only reset the GC thread's
	 * worker data here instead of earlier when we joined the
	 * workers.
	 */
	if (major_collector.reset_worker_data)
		major_collector.reset_worker_data (workers_gc_thread_data.major_collector_data);

	if (objects_pinned) {
		/*This is slow, but we just OOM'd*/
		mono_sgen_pin_queue_clear_discarded_entries (nursery_section, old_next_pin_slot);
		evacuate_pin_staging_area ();
		optimize_pin_queue (0);
		mono_sgen_find_section_pin_queue_start_end (nursery_section);
		objects_pinned = 0;
	}

	reset_heap_boundaries ();
	mono_sgen_update_heap_boundaries ((mword)nursery_start, (mword)nursery_end);

	/* sweep the big objects list */
	prevbo = NULL;
	for (bigobj = los_object_list; bigobj;) {
		if (object_is_pinned (bigobj->data)) {
			unpin_object (bigobj->data);
			mono_sgen_update_heap_boundaries ((mword)bigobj->data, (mword)bigobj->data + bigobj->size);
		} else {
			LOSObject *to_free;
			/* not referenced anywhere, so we can free it */
			if (prevbo)
				prevbo->next = bigobj->next;
			else
				los_object_list = bigobj->next;
			to_free = bigobj;
			bigobj = bigobj->next;
			mono_sgen_los_free_object (to_free);
			continue;
		}
		prevbo = bigobj;
		bigobj = bigobj->next;
	}

	TV_GETTIME (btv);
	time_major_free_bigobjs += TV_ELAPSED_MS (atv, btv);

	mono_sgen_los_sweep ();

	TV_GETTIME (atv);
	time_major_los_sweep += TV_ELAPSED_MS (btv, atv);

	major_collector.sweep ();

	TV_GETTIME (btv);
	time_major_sweep += TV_ELAPSED_MS (atv, btv);

	/* walk the pin_queue, build up the fragment list of free memory, unmark
	 * pinned objects as we go, memzero() the empty fragments so they are ready for the
	 * next allocations.
	 */
	if (!mono_sgen_build_nursery_fragments (nursery_section, nursery_section->pin_queue_start, nursery_section->pin_queue_num_entries))
		degraded_mode = 1;

	/* Clear TLABs for all threads */
	clear_tlabs ();

	TV_GETTIME (atv);
	time_major_fragment_creation += TV_ELAPSED_MS (btv, atv);

	TV_GETTIME (all_btv);
	mono_stats.major_gc_time_usecs += TV_ELAPSED (all_atv, all_btv);

	if (heap_dump_file)
		dump_heap ("major", num_major_gcs - 1, reason);

	/* prepare the pin queue for the next collection */
	next_pin_slot = 0;
	if (fin_ready_list || critical_fin_list) {
		DEBUG (4, fprintf (gc_debug_file, "Finalizer-thread wakeup: ready %d\n", num_ready_finalizers));
		mono_gc_finalize_notify ();
	}
	pin_stats_reset ();

	g_assert (gray_object_queue_is_empty (&gray_queue));

	try_calculate_minor_collection_allowance (TRUE);

	minor_collection_sections_alloced = 0;
	last_collection_los_memory_usage = los_memory_usage;

	major_collector.finish_major_collection ();

	check_scan_starts ();

	binary_protocol_flush_buffers (FALSE);

	//consistency_check ();

	return bytes_pinned_from_failed_allocation > 0;
}

static void
major_collection (const char *reason)
{
	gboolean need_minor_collection;

	if (disable_major_collections) {
		collect_nursery (0);
		return;
	}

	major_collection_happened = TRUE;
	current_collection_generation = GENERATION_OLD;
	need_minor_collection = major_do_collection (reason);
	current_collection_generation = -1;

	if (need_minor_collection)
		collect_nursery (0);
}

void
sgen_collect_major_no_lock (const char *reason)
{
	gint64 gc_start_time;

	mono_profiler_gc_event (MONO_GC_EVENT_START, 1);
	gc_start_time = mono_100ns_ticks ();
	stop_world (1);
	major_collection (reason);
	restart_world (1);
	mono_trace_message (MONO_TRACE_GC, "major gc took %d usecs", (mono_100ns_ticks () - gc_start_time) / 10);
	mono_profiler_gc_event (MONO_GC_EVENT_END, 1);
}

/*
 * When deciding if it's better to collect or to expand, keep track
 * of how much garbage was reclaimed with the last collection: if it's too
 * little, expand.
 * This is called when we could not allocate a small object.
 */
static void __attribute__((noinline))
minor_collect_or_expand_inner (size_t size)
{
	int do_minor_collection = 1;

	g_assert (nursery_section);
	if (do_minor_collection) {
		gint64 total_gc_time, major_gc_time = 0;

		mono_profiler_gc_event (MONO_GC_EVENT_START, 0);
		total_gc_time = mono_100ns_ticks ();

		stop_world (0);
		if (collect_nursery (size)) {
			mono_profiler_gc_event (MONO_GC_EVENT_START, 1);
			major_gc_time = mono_100ns_ticks ();

			major_collection ("minor overflow");

			/* keep events symmetric */
			major_gc_time = mono_100ns_ticks () - major_gc_time;
			mono_profiler_gc_event (MONO_GC_EVENT_END, 1);
		}
		DEBUG (2, fprintf (gc_debug_file, "Heap size: %lu, LOS size: %lu\n", (unsigned long)total_alloc, (unsigned long)los_memory_usage));
		restart_world (0);

		total_gc_time = mono_100ns_ticks () - total_gc_time;
		if (major_gc_time)
			mono_trace_message (MONO_TRACE_GC, "overflow major gc took %d usecs minor gc took %d usecs", total_gc_time / 10, (total_gc_time - major_gc_time) / 10);
		else
			mono_trace_message (MONO_TRACE_GC, "minor gc took %d usecs", total_gc_time / 10);
		
		/* this also sets the proper pointers for the next allocation */
		if (!mono_sgen_can_alloc_size (size)) {
			int i;
			/* TypeBuilder and MonoMethod are killing mcs with fragmentation */
			DEBUG (1, fprintf (gc_debug_file, "nursery collection didn't find enough room for %zd alloc (%d pinned)\n", size, last_num_pinned));
			for (i = 0; i < last_num_pinned; ++i) {
				DEBUG (3, fprintf (gc_debug_file, "Bastard pinning obj %p (%s), size: %d\n", pin_queue [i], safe_name (pin_queue [i]), safe_object_get_size (pin_queue [i])));
			}
			degraded_mode = 1;
		}
		mono_profiler_gc_event (MONO_GC_EVENT_END, 0);
	}
	//report_internal_mem_usage ();
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
	mono_sgen_report_internal_mem_usage ();
	printf ("Pinned memory usage:\n");
	major_collector.report_pinned_memory_usage ();
}

/*
 * ######################################################################
 * ########  Object allocation
 * ######################################################################
 * This section of code deals with allocating memory for objects.
 * There are several ways:
 * *) allocate large objects
 * *) allocate normal objects
 * *) fast lock-free allocation
 * *) allocation of pinned objects
 */

static inline void
set_nursery_scan_start (char *p)
{
	int idx = (p - (char*)nursery_section->data) / SCAN_START_SIZE;
	nursery_section->scan_starts [idx] = p;
}

static void*
alloc_degraded (MonoVTable *vtable, size_t size, gboolean for_mature)
{
	static int last_major_gc_warned = -1;
	static int num_degraded = 0;

	if (!for_mature) {
		if (last_major_gc_warned < num_major_gcs) {
			++num_degraded;
			if (num_degraded == 1 || num_degraded == 3)
				fprintf (stderr, "Warning: Degraded allocation.  Consider increasing nursery-size if the warning persists.\n");
			else if (num_degraded == 10)
				fprintf (stderr, "Warning: Repeated degraded allocation.  Consider increasing nursery-size.\n");
			last_major_gc_warned = num_major_gcs;
		}
	}

	if (need_major_collection (0)) {
		gint64 gc_start_time;

		mono_profiler_gc_event (MONO_GC_EVENT_START, 1);
		gc_start_time = mono_100ns_ticks ();

		stop_world (1);
		major_collection ("degraded overflow");
		restart_world (1);

		mono_trace_message (MONO_TRACE_GC, "major gc took %d usecs", (mono_100ns_ticks () - gc_start_time) / 10);
		mono_profiler_gc_event (MONO_GC_EVENT_END, 1);
	}

	return major_collector.alloc_degraded (vtable, size);
}

/*
 * Provide a variant that takes just the vtable for small fixed-size objects.
 * The aligned size is already computed and stored in vt->gc_descr.
 * Note: every SCAN_START_SIZE or so we are given the chance to do some special
 * processing. We can keep track of where objects start, for example,
 * so when we scan the thread stacks for pinned objects, we can start
 * a search for the pinned object in SCAN_START_SIZE chunks.
 */
static void*
mono_gc_alloc_obj_nolock (MonoVTable *vtable, size_t size)
{
	/* FIXME: handle OOM */
	void **p;
	char *new_next;
	TLAB_ACCESS_INIT;

	HEAVY_STAT (++stat_objects_alloced);
	if (size <= MAX_SMALL_OBJ_SIZE)
		HEAVY_STAT (stat_bytes_alloced += size);
	else
		HEAVY_STAT (stat_bytes_alloced_los += size);

	size = ALIGN_UP (size);

	g_assert (vtable->gc_descr);

	if (G_UNLIKELY (collect_before_allocs)) {
		static int alloc_count;

		InterlockedIncrement (&alloc_count);
		if (((alloc_count % collect_before_allocs) == 0) && nursery_section) {
			gint64 gc_start_time;

			mono_profiler_gc_event (MONO_GC_EVENT_START, 0);
			gc_start_time = mono_100ns_ticks ();

			stop_world (0);
			collect_nursery (0);
			restart_world (0);

			mono_trace_message (MONO_TRACE_GC, "minor gc took %d usecs", (mono_100ns_ticks () - gc_start_time) / 10);
			mono_profiler_gc_event (MONO_GC_EVENT_END, 0);
			if (!degraded_mode && !mono_sgen_can_alloc_size (size) && size <= MAX_SMALL_OBJ_SIZE) {
				// FIXME:
				g_assert_not_reached ();
			}
		}
	}

	/*
	 * We must already have the lock here instead of after the
	 * fast path because we might be interrupted in the fast path
	 * (after confirming that new_next < TLAB_TEMP_END) by the GC,
	 * and we'll end up allocating an object in a fragment which
	 * no longer belongs to us.
	 *
	 * The managed allocator does not do this, but it's treated
	 * specially by the world-stopping code.
	 */

	if (size > MAX_SMALL_OBJ_SIZE) {
		p = mono_sgen_los_alloc_large_inner (vtable, size);
	} else {
		/* tlab_next and tlab_temp_end are TLS vars so accessing them might be expensive */

		p = (void**)TLAB_NEXT;
		/* FIXME: handle overflow */
		new_next = (char*)p + size;
		TLAB_NEXT = new_next;

		if (G_LIKELY (new_next < TLAB_TEMP_END)) {
			/* Fast path */

			/* 
			 * FIXME: We might need a memory barrier here so the change to tlab_next is 
			 * visible before the vtable store.
			 */

			DEBUG (6, fprintf (gc_debug_file, "Allocated object %p, vtable: %p (%s), size: %zd\n", p, vtable, vtable->klass->name, size));
			binary_protocol_alloc (p , vtable, size);
			g_assert (*p == NULL);
			mono_atomic_store_seq (p, vtable);

			return p;
		}

		/* Slow path */

		/* there are two cases: the object is too big or we run out of space in the TLAB */
		/* we also reach here when the thread does its first allocation after a minor 
		 * collection, since the tlab_ variables are initialized to NULL.
		 * there can be another case (from ORP), if we cooperate with the runtime a bit:
		 * objects that need finalizers can have the high bit set in their size
		 * so the above check fails and we can readily add the object to the queue.
		 * This avoids taking again the GC lock when registering, but this is moot when
		 * doing thread-local allocation, so it may not be a good idea.
		 */
		if (TLAB_NEXT >= TLAB_REAL_END) {
			int available_in_tlab;
			/* 
			 * Run out of space in the TLAB. When this happens, some amount of space
			 * remains in the TLAB, but not enough to satisfy the current allocation
			 * request. Currently, we retire the TLAB in all cases, later we could
			 * keep it if the remaining space is above a treshold, and satisfy the
			 * allocation directly from the nursery.
			 */
			TLAB_NEXT -= size;
			/* when running in degraded mode, we continue allocing that way
			 * for a while, to decrease the number of useless nursery collections.
			 */
			if (degraded_mode && degraded_mode < DEFAULT_NURSERY_SIZE) {
				p = alloc_degraded (vtable, size, FALSE);
				binary_protocol_alloc_degraded (p, vtable, size);
				return p;
			}

			available_in_tlab = TLAB_REAL_END - TLAB_NEXT;
			if (size > tlab_size || available_in_tlab > SGEN_MAX_NURSERY_WASTE) {
				/* Allocate directly from the nursery */
				do {
					p = mono_sgen_nursery_alloc (size);
					if (!p) {
						minor_collect_or_expand_inner (size);
						if (degraded_mode) {
							p = alloc_degraded (vtable, size, FALSE);
							binary_protocol_alloc_degraded (p, vtable, size);
							return p;
						} else {
							p = mono_sgen_nursery_alloc (size);
						}
					}
				} while (!p);
				if (!p) {
					// no space left
					g_assert (0);
				}

				if (nursery_clear_policy == CLEAR_AT_TLAB_CREATION) {
					memset (p, 0, size);
				}
			} else {
				int alloc_size = 0;
				if (TLAB_START)
					DEBUG (3, fprintf (gc_debug_file, "Retire TLAB: %p-%p [%ld]\n", TLAB_START, TLAB_REAL_END, (long)(TLAB_REAL_END - TLAB_NEXT - size)));
				mono_sgen_nursery_retire_region (p, available_in_tlab);

				do {
					p = mono_sgen_nursery_alloc_range (tlab_size, size, &alloc_size);
					if (!p) {
						minor_collect_or_expand_inner (tlab_size);
						if (degraded_mode) {
							p = alloc_degraded (vtable, size, FALSE);
							binary_protocol_alloc_degraded (p, vtable, size);
							return p;
						} else {
							p = mono_sgen_nursery_alloc_range (tlab_size, size, &alloc_size);
						}		
					}
				} while (!p);
					
				if (!p) {
					// no space left
					g_assert (0);
				}

				/* Allocate a new TLAB from the current nursery fragment */
				TLAB_START = (char*)p;
				TLAB_NEXT = TLAB_START;
				TLAB_REAL_END = TLAB_START + alloc_size;
				TLAB_TEMP_END = TLAB_START + MIN (SCAN_START_SIZE, alloc_size);

				if (nursery_clear_policy == CLEAR_AT_TLAB_CREATION) {
					memset (TLAB_START, 0, alloc_size);
				}

				/* Allocate from the TLAB */
				p = (void*)TLAB_NEXT;
				TLAB_NEXT += size;
				set_nursery_scan_start (p);
			}
		} else {
			/* Reached tlab_temp_end */

			/* record the scan start so we can find pinned objects more easily */
			set_nursery_scan_start (p);
			/* we just bump tlab_temp_end as well */
			TLAB_TEMP_END = MIN (TLAB_REAL_END, TLAB_NEXT + SCAN_START_SIZE);
			DEBUG (5, fprintf (gc_debug_file, "Expanding local alloc: %p-%p\n", TLAB_NEXT, TLAB_TEMP_END));
		}
	}

	if (G_LIKELY (p)) {
		DEBUG (6, fprintf (gc_debug_file, "Allocated object %p, vtable: %p (%s), size: %zd\n", p, vtable, vtable->klass->name, size));
		binary_protocol_alloc (p, vtable, size);
		mono_atomic_store_seq (p, vtable);
	}

	return p;
}

static void*
mono_gc_try_alloc_obj_nolock (MonoVTable *vtable, size_t size)
{
	void **p;
	char *new_next;
	TLAB_ACCESS_INIT;

	size = ALIGN_UP (size);

	g_assert (vtable->gc_descr);
	if (size > MAX_SMALL_OBJ_SIZE)
		return NULL;

	if (G_UNLIKELY (size > tlab_size)) {
		/* Allocate directly from the nursery */
		p = mono_sgen_nursery_alloc (size);
		if (!p)
			return NULL;

		/*FIXME we should use weak memory ops here. Should help specially on x86. */
		if (nursery_clear_policy == CLEAR_AT_TLAB_CREATION)
			memset (p, 0, size);
	} else {
		int available_in_tlab;
		char *real_end;
		/* tlab_next and tlab_temp_end are TLS vars so accessing them might be expensive */

		p = (void**)TLAB_NEXT;
		/* FIXME: handle overflow */
		new_next = (char*)p + size;

		real_end = TLAB_REAL_END;
		available_in_tlab = real_end - (char*)p;

		if (G_LIKELY (new_next < real_end)) {
			TLAB_NEXT = new_next;
		} else if (available_in_tlab > SGEN_MAX_NURSERY_WASTE) {
			/* Allocate directly from the nursery */
			p = mono_sgen_nursery_alloc (size);
			if (!p)
				return NULL;

			if (nursery_clear_policy == CLEAR_AT_TLAB_CREATION)
				memset (p, 0, size);			
		} else {
			int alloc_size = 0;

			mono_sgen_nursery_retire_region (p, available_in_tlab);
			new_next = mono_sgen_nursery_alloc_range (tlab_size, size, &alloc_size);
			p = (void**)new_next;
			if (!p)
				return NULL;

			TLAB_START = (char*)new_next;
			TLAB_NEXT = new_next + size;
			TLAB_REAL_END = new_next + alloc_size;
			TLAB_TEMP_END = new_next + MIN (SCAN_START_SIZE, alloc_size);

			if (nursery_clear_policy == CLEAR_AT_TLAB_CREATION)
				memset (new_next, 0, alloc_size);
			new_next += size;
		}

		/* Second case, we overflowed temp end */
		if (G_UNLIKELY (new_next >= TLAB_TEMP_END)) {
			set_nursery_scan_start (p);
			/* we just bump tlab_temp_end as well */
			TLAB_TEMP_END = MIN (TLAB_REAL_END, TLAB_NEXT + SCAN_START_SIZE);
			DEBUG (5, fprintf (gc_debug_file, "Expanding local alloc: %p-%p\n", TLAB_NEXT, TLAB_TEMP_END));		
		}
	}

	HEAVY_STAT (++stat_objects_alloced);
	HEAVY_STAT (stat_bytes_alloced += size);

	DEBUG (6, fprintf (gc_debug_file, "Allocated object %p, vtable: %p (%s), size: %zd\n", p, vtable, vtable->klass->name, size));
	binary_protocol_alloc (p, vtable, size);
	g_assert (*p == NULL); /* FIXME disable this in non debug builds */

	mono_atomic_store_seq (p, vtable);

	return p;
}

void*
mono_gc_alloc_obj (MonoVTable *vtable, size_t size)
{
	void *res;
#ifndef DISABLE_CRITICAL_REGION
	TLAB_ACCESS_INIT;
	ENTER_CRITICAL_REGION;
	res = mono_gc_try_alloc_obj_nolock (vtable, size);
	if (res) {
		EXIT_CRITICAL_REGION;
		return res;
	}
	EXIT_CRITICAL_REGION;
#endif
	LOCK_GC;
	res = mono_gc_alloc_obj_nolock (vtable, size);
	UNLOCK_GC;
	if (G_UNLIKELY (!res))
		return mono_gc_out_of_memory (size);
	return res;
}

void*
mono_gc_alloc_vector (MonoVTable *vtable, size_t size, uintptr_t max_length)
{
	MonoArray *arr;
#ifndef DISABLE_CRITICAL_REGION
	TLAB_ACCESS_INIT;
	ENTER_CRITICAL_REGION;
	arr = mono_gc_try_alloc_obj_nolock (vtable, size);
	if (arr) {
		/*This doesn't require fencing since EXIT_CRITICAL_REGION already does it for us*/
		arr->max_length = max_length;
		EXIT_CRITICAL_REGION;
		return arr;
	}
	EXIT_CRITICAL_REGION;
#endif

	LOCK_GC;

	arr = mono_gc_alloc_obj_nolock (vtable, size);
	if (G_UNLIKELY (!arr)) {
		UNLOCK_GC;
		return mono_gc_out_of_memory (size);
	}

	arr->max_length = max_length;

	UNLOCK_GC;

	return arr;
}

void*
mono_gc_alloc_array (MonoVTable *vtable, size_t size, uintptr_t max_length, uintptr_t bounds_size)
{
	MonoArray *arr;
	MonoArrayBounds *bounds;

	LOCK_GC;

	arr = mono_gc_alloc_obj_nolock (vtable, size);
	if (G_UNLIKELY (!arr)) {
		UNLOCK_GC;
		return mono_gc_out_of_memory (size);
	}

	arr->max_length = max_length;

	bounds = (MonoArrayBounds*)((char*)arr + size - bounds_size);
	arr->bounds = bounds;

	UNLOCK_GC;

	return arr;
}

void*
mono_gc_alloc_string (MonoVTable *vtable, size_t size, gint32 len)
{
	MonoString *str;
#ifndef DISABLE_CRITICAL_REGION
	TLAB_ACCESS_INIT;
	ENTER_CRITICAL_REGION;
	str = mono_gc_try_alloc_obj_nolock (vtable, size);
	if (str) {
		/*This doesn't require fencing since EXIT_CRITICAL_REGION already does it for us*/
		str->length = len;
		EXIT_CRITICAL_REGION;
		return str;
	}
	EXIT_CRITICAL_REGION;
#endif

	LOCK_GC;

	str = mono_gc_alloc_obj_nolock (vtable, size);
	if (G_UNLIKELY (!str)) {
		UNLOCK_GC;
		return mono_gc_out_of_memory (size);
	}

	str->length = len;

	UNLOCK_GC;

	return str;
}

/*
 * To be used for interned strings and possibly MonoThread, reflection handles.
 * We may want to explicitly free these objects.
 */
void*
mono_gc_alloc_pinned_obj (MonoVTable *vtable, size_t size)
{
	void **p;
	size = ALIGN_UP (size);
	LOCK_GC;

	if (size > MAX_SMALL_OBJ_SIZE) {
		/* large objects are always pinned anyway */
		p = mono_sgen_los_alloc_large_inner (vtable, size);
	} else {
		DEBUG (9, g_assert (vtable->klass->inited));
		p = major_collector.alloc_small_pinned_obj (size, SGEN_VTABLE_HAS_REFERENCES (vtable));
	}
	if (G_LIKELY (p)) {
		DEBUG (6, fprintf (gc_debug_file, "Allocated pinned object %p, vtable: %p (%s), size: %zd\n", p, vtable, vtable->klass->name, size));
		binary_protocol_alloc_pinned (p, vtable, size);
		mono_atomic_store_seq (p, vtable);
	}
	UNLOCK_GC;
	return p;
}

void*
mono_gc_alloc_mature (MonoVTable *vtable)
{
	void **res;
	size_t size = ALIGN_UP (vtable->klass->instance_size);
	LOCK_GC;
	res = alloc_degraded (vtable, size, TRUE);
	mono_atomic_store_seq (res, vtable);
	UNLOCK_GC;
	if (G_UNLIKELY (vtable->klass->has_finalize))
		mono_object_register_finalizer ((MonoObject*)res);

	return res;
}

/*
 * ######################################################################
 * ########  Finalization support
 * ######################################################################
 */

/*
 * this is valid for the nursery: if the object has been forwarded it means it's
 * still refrenced from a root. If it is pinned it's still alive as well.
 * Return TRUE if @obj is ready to be finalized.
 */
#define object_is_fin_ready(obj) (!object_is_pinned (obj) && !object_is_forwarded (obj))


gboolean
mono_sgen_gc_is_object_ready_for_finalization (void *object)
{
	return !major_collector.is_object_live (object) && object_is_fin_ready (object);
}

static gboolean
has_critical_finalizer (MonoObject *obj)
{
	MonoClass *class;

	if (!mono_defaults.critical_finalizer_object)
		return FALSE;

	class = ((MonoVTable*)LOAD_VTABLE (obj))->klass;

	return mono_class_has_parent (class, mono_defaults.critical_finalizer_object);
}

static void
queue_finalization_entry (MonoObject *obj) {
	FinalizeReadyEntry *entry = mono_sgen_alloc_internal (INTERNAL_MEM_FINALIZE_READY_ENTRY);
	entry->object = obj;
	if (has_critical_finalizer (obj)) {
		entry->next = critical_fin_list;
		critical_fin_list = entry;
	} else {
		entry->next = fin_ready_list;
		fin_ready_list = entry;
	}
}

static int
object_is_reachable (char *object, char *start, char *end)
{
	/*This happens for non nursery objects during minor collections. We just treat all objects as alive.*/
	if (object < start || object >= end)
		return TRUE;
	return !object_is_fin_ready (object) || major_collector.is_object_live (object);
}

#include "sgen-fin-weak-hash.c"

gboolean
mono_sgen_object_is_live (void *obj)
{
	if (ptr_in_nursery (obj))
		return object_is_pinned (obj);
	if (current_collection_generation == GENERATION_NURSERY)
		return FALSE;
	return major_collector.is_object_live (obj);
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
			mono_sgen_free_internal (tmp, INTERNAL_MEM_EPHEMERON_LINK);
		} else {
			prev = current;
			current = current->next;
		}
	}
}

/* LOCKING: requires that the GC lock is held */
static void
clear_unreachable_ephemerons (CopyOrMarkObjectFunc copy_func, char *start, char *end, GrayQueue *queue)
{
	int was_in_nursery, was_promoted;
	EphemeronLinkNode *current = ephemeron_list, *prev = NULL;
	MonoArray *array;
	Ephemeron *cur, *array_end;
	char *tombstone;

	while (current) {
		char *object = current->array;

		if (!object_is_reachable (object, start, end)) {
			EphemeronLinkNode *tmp = current;

			DEBUG (5, fprintf (gc_debug_file, "Dead Ephemeron array at %p\n", object));

			if (prev)
				prev->next = current->next;
			else
				ephemeron_list = current->next;

			current = current->next;
			mono_sgen_free_internal (tmp, INTERNAL_MEM_EPHEMERON_LINK);

			continue;
		}

		was_in_nursery = ptr_in_nursery (object);
		copy_func ((void**)&object, queue);
		current->array = object;

		/*The array was promoted, add global remsets for key/values left behind in nursery.*/
		was_promoted = was_in_nursery && !ptr_in_nursery (object);

		DEBUG (5, fprintf (gc_debug_file, "Clearing unreachable entries for ephemeron array at %p\n", object));

		array = (MonoArray*)object;
		cur = mono_array_addr (array, Ephemeron, 0);
		array_end = cur + mono_array_length_fast (array);
		tombstone = (char*)((MonoVTable*)LOAD_VTABLE (object))->domain->ephemeron_tombstone;

		for (; cur < array_end; ++cur) {
			char *key = (char*)cur->key;

			if (!key || key == tombstone)
				continue;

			DEBUG (5, fprintf (gc_debug_file, "[%td] key %p (%s) value %p (%s)\n", cur - mono_array_addr (array, Ephemeron, 0),
				key, object_is_reachable (key, start, end) ? "reachable" : "unreachable",
				cur->value, cur->value && object_is_reachable (cur->value, start, end) ? "reachable" : "unreachable"));

			if (!object_is_reachable (key, start, end)) {
				cur->key = tombstone;
				cur->value = NULL;
				continue;
			}

			if (was_promoted) {
				if (ptr_in_nursery (key)) {/*key was not promoted*/
					DEBUG (5, fprintf (gc_debug_file, "\tAdded remset to key %p\n", key));
					mono_sgen_add_to_global_remset (&cur->key);
				}
				if (ptr_in_nursery (cur->value)) {/*value was not promoted*/
					DEBUG (5, fprintf (gc_debug_file, "\tAdded remset to value %p\n", cur->value));
					mono_sgen_add_to_global_remset (&cur->value);
				}
			}
		}
		prev = current;
		current = current->next;
	}
}

/* LOCKING: requires that the GC lock is held */
static int
mark_ephemerons_in_range (CopyOrMarkObjectFunc copy_func, char *start, char *end, GrayQueue *queue)
{
	int nothing_marked = 1;
	EphemeronLinkNode *current = ephemeron_list;
	MonoArray *array;
	Ephemeron *cur, *array_end;
	char *tombstone;

	for (current = ephemeron_list; current; current = current->next) {
		char *object = current->array;
		DEBUG (5, fprintf (gc_debug_file, "Ephemeron array at %p\n", object));

		/*We ignore arrays in old gen during minor collections since all objects are promoted by the remset machinery.*/
		/*if (object < start || object >= end)
			continue;
                */

		/*It has to be alive*/
		if (!object_is_reachable (object, start, end)) {
			DEBUG (5, fprintf (gc_debug_file, "\tnot reachable\n"));
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

			DEBUG (5, fprintf (gc_debug_file, "[%td] key %p (%s) value %p (%s)\n", cur - mono_array_addr (array, Ephemeron, 0),
				key, object_is_reachable (key, start, end) ? "reachable" : "unreachable",
				cur->value, cur->value && object_is_reachable (cur->value, start, end) ? "reachable" : "unreachable"));

			if (object_is_reachable (key, start, end)) {
				char *value = cur->value;

				copy_func ((void**)&cur->key, queue);
				if (value) {
					if (!object_is_reachable (value, start, end))
						nothing_marked = 0;
					copy_func ((void**)&cur->value, queue);
				}
			}
		}
	}

	DEBUG (5, fprintf (gc_debug_file, "Ephemeron run finished. Is it done %d\n", nothing_marked));
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
			mono_sgen_free_internal (entry, INTERNAL_MEM_FINALIZE_READY_ENTRY);
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
			DEBUG (7, fprintf (gc_debug_file, "Finalizing object %p (%s)\n", obj, safe_name (obj)));
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

/* Negative value to remove */
void
mono_gc_add_memory_pressure (gint64 value)
{
	/* FIXME: Use interlocked functions */
	LOCK_GC;
	memory_pressure += value;
	UNLOCK_GC;
}

void
mono_sgen_register_major_sections_alloced (int num_sections)
{
	minor_collection_sections_alloced += num_sections;
}

mword
mono_sgen_get_minor_collection_allowance (void)
{
	return minor_collection_allowance;
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
		RootRecord *root = mono_sgen_hash_table_lookup (&roots_hash [i], start);
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

	mono_sgen_hash_table_replace (&roots_hash [root_type], start, &new_root);
	roots_size += size;

	DEBUG (3, fprintf (gc_debug_file, "Added root for range: %p-%p, descr: %p  (%d/%d bytes)\n", start, new_root.end_root, descr, (int)size, (int)roots_size));

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
		if (mono_sgen_hash_table_remove (&roots_hash [root_type], addr, &root))
			roots_size -= (root.end_root - addr);
	}
	UNLOCK_GC;
}

/*
 * ######################################################################
 * ########  Thread handling (stop/start code)
 * ######################################################################
 */

unsigned int mono_sgen_global_stop_count = 0;

#ifdef USE_MONO_CTX
static MonoContext cur_thread_ctx = {0};
#else
static mword cur_thread_regs [ARCH_NUM_REGS] = {0};
#endif

static void
update_current_thread_stack (void *start)
{
	int stack_guard = 0;
#ifndef USE_MONO_CTX
	void *ptr = cur_thread_regs;
#endif
	SgenThreadInfo *info = mono_thread_info_current ();
	
	info->stack_start = align_pointer (&stack_guard);
	g_assert (info->stack_start >= info->stack_start_limit && info->stack_start < info->stack_end);
#ifdef USE_MONO_CTX
	MONO_CONTEXT_GET_CURRENT (cur_thread_ctx);
	info->monoctx = &cur_thread_ctx;
#else
	ARCH_STORE_REGS (ptr);
	info->stopped_regs = ptr;
#endif
	if (gc_callbacks.thread_suspend_func)
		gc_callbacks.thread_suspend_func (info->runtime_data, NULL);
}

void
mono_sgen_fill_thread_info_for_suspend (SgenThreadInfo *info)
{
#ifdef HAVE_KW_THREAD
	/* update the remset info in the thread data structure */
	info->remset = remembered_set;
#endif
}

/*
 * Define this and use the "xdomain-checks" MONO_GC_DEBUG option to
 * have cross-domain checks in the write barrier.
 */
//#define XDOMAIN_CHECKS_IN_WBARRIER

#ifndef SGEN_BINARY_PROTOCOL
#ifndef HEAVY_STATISTICS
#define MANAGED_ALLOCATION
#ifndef XDOMAIN_CHECKS_IN_WBARRIER
#define MANAGED_WBARRIER
#endif
#endif
#endif

static gboolean
is_ip_in_managed_allocator (MonoDomain *domain, gpointer ip);

static int
restart_threads_until_none_in_managed_allocator (void)
{
	SgenThreadInfo *info;
	int num_threads_died = 0;
	int sleep_duration = -1;

	for (;;) {
		int restart_count = 0, restarted_count = 0;
		/* restart all threads that stopped in the
		   allocator */
		FOREACH_THREAD_SAFE (info) {
			gboolean result;
			if (info->skip)
				continue;
			if (!info->thread_is_dying && (!info->stack_start || info->in_critical_region ||
					is_ip_in_managed_allocator (info->stopped_domain, info->stopped_ip))) {
				binary_protocol_thread_restart ((gpointer)mono_thread_info_get_tid (info));
				result = mono_sgen_resume_thread (info);
				if (result) {
					++restart_count;
				} else {
					info->skip = 1;
				}
			} else {
				/* we set the stopped_ip to
				   NULL for threads which
				   we're not restarting so
				   that we can easily identify
				   the others */
				info->stopped_ip = NULL;
				info->stopped_domain = NULL;
			}
		} END_FOREACH_THREAD_SAFE
		/* if no threads were restarted, we're done */
		if (restart_count == 0)
			break;

		/* wait for the threads to signal their restart */
		mono_sgen_wait_for_suspend_ack (restart_count);

		if (sleep_duration < 0) {
#ifdef HOST_WIN32
			SwitchToThread ();
#else
			sched_yield ();
#endif
			sleep_duration = 0;
		} else {
			g_usleep (sleep_duration);
			sleep_duration += 10;
		}

		/* stop them again */
		FOREACH_THREAD (info) {
			gboolean result;
			if (info->skip || info->stopped_ip == NULL)
				continue;
			result = mono_sgen_suspend_thread (info);

			if (result) {
				++restarted_count;
			} else {
				info->skip = 1;
			}
		} END_FOREACH_THREAD
		/* some threads might have died */
		num_threads_died += restart_count - restarted_count;
		/* wait for the threads to signal their suspension
		   again */
		mono_sgen_wait_for_suspend_ack (restart_count);
	}

	return num_threads_died;
}

static void
acquire_gc_locks (void)
{
	LOCK_INTERRUPTION;
	mono_thread_info_suspend_lock ();
}

static void
release_gc_locks (void)
{
	mono_thread_info_suspend_unlock ();
	UNLOCK_INTERRUPTION;
}

static TV_DECLARE (stop_world_time);
static unsigned long max_pause_usec = 0;

/* LOCKING: assumes the GC lock is held */
static int
stop_world (int generation)
{
	int count;

	/*XXX this is the right stop, thought might not be the nicest place to put it*/
	mono_sgen_process_togglerefs ();

	mono_profiler_gc_event (MONO_GC_EVENT_PRE_STOP_WORLD, generation);
	acquire_gc_locks ();

	update_current_thread_stack (&count);

	mono_sgen_global_stop_count++;
	DEBUG (3, fprintf (gc_debug_file, "stopping world n %d from %p %p\n", mono_sgen_global_stop_count, mono_thread_info_current (), (gpointer)mono_native_thread_id_get ()));
	TV_GETTIME (stop_world_time);
	count = mono_sgen_thread_handshake (TRUE);
	count -= restart_threads_until_none_in_managed_allocator ();
	g_assert (count >= 0);
	DEBUG (3, fprintf (gc_debug_file, "world stopped %d thread(s)\n", count));
	mono_profiler_gc_event (MONO_GC_EVENT_POST_STOP_WORLD, generation);

	last_major_num_sections = major_collector.get_num_major_sections ();
	last_los_memory_usage = los_memory_usage;
	major_collection_happened = FALSE;
	return count;
}

/* LOCKING: assumes the GC lock is held */
static int
restart_world (int generation)
{
	int count, num_major_sections;
	SgenThreadInfo *info;
	TV_DECLARE (end_sw);
	TV_DECLARE (end_bridge);
	unsigned long usec, bridge_usec;

	/* notify the profiler of the leftovers */
	if (G_UNLIKELY (mono_profiler_events & MONO_PROFILE_GC_MOVES)) {
		if (moved_objects_idx) {
			mono_profiler_gc_moves (moved_objects, moved_objects_idx);
			moved_objects_idx = 0;
		}
	}
	mono_profiler_gc_event (MONO_GC_EVENT_PRE_START_WORLD, generation);
	FOREACH_THREAD (info) {
		info->stack_start = NULL;
#ifdef USE_MONO_CTX
		info->monoctx = NULL;
#else
		info->stopped_regs = NULL;
#endif
	} END_FOREACH_THREAD

	release_gc_locks ();

	count = mono_sgen_thread_handshake (FALSE);
	TV_GETTIME (end_sw);
	usec = TV_ELAPSED (stop_world_time, end_sw);
	max_pause_usec = MAX (usec, max_pause_usec);
	DEBUG (2, fprintf (gc_debug_file, "restarted %d thread(s) (pause time: %d usec, max: %d)\n", count, (int)usec, (int)max_pause_usec));
	mono_profiler_gc_event (MONO_GC_EVENT_POST_START_WORLD, generation);

	bridge_process ();

	TV_GETTIME (end_bridge);
	bridge_usec = TV_ELAPSED (end_sw, end_bridge);

	num_major_sections = major_collector.get_num_major_sections ();
	if (major_collection_happened)
		mono_trace (G_LOG_LEVEL_INFO, MONO_TRACE_GC, "GC_MAJOR: %s pause %.2fms, bridge %.2fms major %dK/%dK los %dK/%dK",
			generation ? "" : "(minor overflow)",
			(int)usec / 1000.0f, (int)bridge_usec / 1000.0f,
			major_collector.section_size * num_major_sections / 1024,
			major_collector.section_size * last_major_num_sections / 1024,
			los_memory_usage / 1024,
			last_los_memory_usage / 1024);
	else
		mono_trace (G_LOG_LEVEL_INFO, MONO_TRACE_GC, "GC_MINOR: pause %.2fms, bridge %.2fms promoted %dK major %dK los %dK",
			(int)usec / 1000.0f, (int)bridge_usec / 1000.0f,
			(num_major_sections - last_major_num_sections) * major_collector.section_size / 1024,
			major_collector.section_size * num_major_sections / 1024,
			los_memory_usage / 1024);

	return count;
}

int
mono_sgen_get_current_collection_generation (void)
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
mono_gc_scan_object (void *obj)
{
	UserCopyOrMarkData *data = mono_native_tls_get_value (user_copy_or_mark_key);

	if (current_collection_generation == GENERATION_NURSERY) {
		if (collection_is_parallel ())
			major_collector.copy_object (&obj, data->queue);
		else
			major_collector.nopar_copy_object (&obj, data->queue);
	} else {
		major_collector.copy_or_mark_object (&obj, data->queue);
	}
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
			DEBUG (3, fprintf (gc_debug_file, "Skipping dead thread %p, range: %p-%p, size: %td\n", info, info->stack_start, info->stack_end, (char*)info->stack_end - (char*)info->stack_start));
			continue;
		}
		DEBUG (3, fprintf (gc_debug_file, "Scanning thread %p, range: %p-%p, size: %ld, pinned=%d\n", info, info->stack_start, info->stack_end, (char*)info->stack_end - (char*)info->stack_start, next_pin_slot));
		if (!info->thread_is_dying) {
			if (gc_callbacks.thread_mark_func && !conservative_stack_mark) {
				UserCopyOrMarkData data = { NULL, queue };
				set_user_copy_or_mark_data (&data);
				gc_callbacks.thread_mark_func (info->runtime_data, info->stack_start, info->stack_end, precise);
				set_user_copy_or_mark_data (NULL);
			} else if (!precise) {
				conservatively_pin_objects_from (info->stack_start, info->stack_end, start_nursery, end_nursery, PIN_TYPE_STACK);
			}
		}

#ifdef USE_MONO_CTX
		if (!info->thread_is_dying && !precise)
			conservatively_pin_objects_from ((void**)info->monoctx, (void**)info->monoctx + ARCH_NUM_REGS,
				start_nursery, end_nursery, PIN_TYPE_STACK);
#else
		if (!info->thread_is_dying && !precise)
			conservatively_pin_objects_from (info->stopped_regs, info->stopped_regs + ARCH_NUM_REGS,
					start_nursery, end_nursery, PIN_TYPE_STACK);
#endif
	} END_FOREACH_THREAD
}

static void
find_pinning_ref_from_thread (char *obj, size_t size)
{
	int j;
	SgenThreadInfo *info;
	char *endobj = obj + size;

	FOREACH_THREAD (info) {
		char **start = (char**)info->stack_start;
		if (info->skip)
			continue;
		while (start < (char**)info->stack_end) {
			if (*start >= obj && *start < endobj) {
				DEBUG (0, fprintf (gc_debug_file, "Object %p referenced in thread %p (id %p) at %p, stack: %p-%p\n", obj, info, (gpointer)mono_thread_info_get_tid (info), start, info->stack_start, info->stack_end));
			}
			start++;
		}

		for (j = 0; j < ARCH_NUM_REGS; ++j) {
#ifdef USE_MONO_CTX
			mword w = ((mword*)info->monoctx) [j];
#else
			mword w = (mword)info->stopped_regs [j];
#endif

			if (w >= (mword)obj && w < (mword)obj + size)
				DEBUG (0, fprintf (gc_debug_file, "Object %p referenced in saved reg %d of thread %p (id %p)\n", obj, j, info, (gpointer)mono_thread_info_get_tid (info)));
		} END_FOREACH_THREAD
	}
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

static mword*
handle_remset (mword *p, void *start_nursery, void *end_nursery, gboolean global, GrayQueue *queue)
{
	void **ptr;
	mword count;
	mword desc;

	if (global)
		HEAVY_STAT (++stat_global_remsets_processed);
	else
		HEAVY_STAT (++stat_local_remsets_processed);

	/* FIXME: exclude stack locations */
	switch ((*p) & REMSET_TYPE_MASK) {
	case REMSET_LOCATION:
		ptr = (void**)(*p);
		//__builtin_prefetch (ptr);
		if (((void*)ptr < start_nursery || (void*)ptr >= end_nursery)) {
			gpointer old = *ptr;
			major_collector.copy_object (ptr, queue);
			DEBUG (9, fprintf (gc_debug_file, "Overwrote remset at %p with %p\n", ptr, *ptr));
			if (old)
				binary_protocol_ptr_update (ptr, old, *ptr, (gpointer)LOAD_VTABLE (*ptr), safe_object_get_size (*ptr));
			if (!global && *ptr >= start_nursery && *ptr < end_nursery) {
				/*
				 * If the object is pinned, each reference to it from nonpinned objects
				 * becomes part of the global remset, which can grow very large.
				 */
				DEBUG (9, fprintf (gc_debug_file, "Add to global remset because of pinning %p (%p %s)\n", ptr, *ptr, safe_name (*ptr)));
				mono_sgen_add_to_global_remset (ptr);
			}
		} else {
			DEBUG (9, fprintf (gc_debug_file, "Skipping remset at %p holding %p\n", ptr, *ptr));
		}
		return p + 1;
	case REMSET_RANGE:
		ptr = (void**)(*p & ~REMSET_TYPE_MASK);
		if (((void*)ptr >= start_nursery && (void*)ptr < end_nursery))
			return p + 2;
		count = p [1];
		while (count-- > 0) {
			major_collector.copy_object (ptr, queue);
			DEBUG (9, fprintf (gc_debug_file, "Overwrote remset at %p with %p (count: %d)\n", ptr, *ptr, (int)count));
			if (!global && *ptr >= start_nursery && *ptr < end_nursery)
				mono_sgen_add_to_global_remset (ptr);
			++ptr;
		}
		return p + 2;
	case REMSET_OBJECT:
		ptr = (void**)(*p & ~REMSET_TYPE_MASK);
		if (((void*)ptr >= start_nursery && (void*)ptr < end_nursery))
			return p + 1;
		mono_sgen_get_minor_scan_object () ((char*)ptr, queue);
		return p + 1;
	case REMSET_VTYPE: {
		ScanVTypeFunc scan_vtype = mono_sgen_get_minor_scan_vtype ();
		size_t skip_size;

		ptr = (void**)(*p & ~REMSET_TYPE_MASK);
		if (((void*)ptr >= start_nursery && (void*)ptr < end_nursery))
			return p + 4;
		desc = p [1];
		count = p [2];
		skip_size = p [3];
		while (count-- > 0) {
			scan_vtype ((char*)ptr, desc, queue);
			ptr = (void**)((char*)ptr + skip_size);
		}
		return p + 4;
	}
	default:
		g_assert_not_reached ();
	}
	return NULL;
}

#ifdef HEAVY_STATISTICS
static mword*
collect_store_remsets (RememberedSet *remset, mword *bumper)
{
	mword *p = remset->data;
	mword last = 0;
	mword last1 = 0;
	mword last2 = 0;

	while (p < remset->store_next) {
		switch ((*p) & REMSET_TYPE_MASK) {
		case REMSET_LOCATION:
			*bumper++ = *p;
			if (*p == last)
				++stat_saved_remsets_1;
			last = *p;
			if (*p == last1 || *p == last2) {
				++stat_saved_remsets_2;
			} else {
				last2 = last1;
				last1 = *p;
			}
			p += 1;
			break;
		case REMSET_RANGE:
			p += 2;
			break;
		case REMSET_OBJECT:
			p += 1;
			break;
		case REMSET_VTYPE:
			p += 4;
			break;
		default:
			g_assert_not_reached ();
		}
	}

	return bumper;
}

static void
remset_stats (void)
{
	RememberedSet *remset;
	int size = 0;
	SgenThreadInfo *info;
	int i;
	mword *addresses, *bumper, *p, *r;

	FOREACH_THREAD (info) {
		for (remset = info->remset; remset; remset = remset->next)
			size += remset->store_next - remset->data;
	} END_FOREACH_THREAD
	for (remset = freed_thread_remsets; remset; remset = remset->next)
		size += remset->store_next - remset->data;
	for (remset = global_remset; remset; remset = remset->next)
		size += remset->store_next - remset->data;

	bumper = addresses = mono_sgen_alloc_internal_dynamic (sizeof (mword) * size, INTERNAL_MEM_STATISTICS);

	FOREACH_THREAD (info) {
		for (remset = info->remset; remset; remset = remset->next)
			bumper = collect_store_remsets (remset, bumper);
	} END_FOREACH_THREAD
	for (remset = global_remset; remset; remset = remset->next)
		bumper = collect_store_remsets (remset, bumper);
	for (remset = freed_thread_remsets; remset; remset = remset->next)
		bumper = collect_store_remsets (remset, bumper);

	g_assert (bumper <= addresses + size);

	stat_store_remsets += bumper - addresses;

	sort_addresses ((void**)addresses, bumper - addresses);
	p = addresses;
	r = addresses + 1;
	while (r < bumper) {
		if (*r != *p)
			*++p = *r;
		++r;
	}

	stat_store_remsets_unique += p - addresses;

	mono_sgen_free_internal_dynamic (addresses, sizeof (mword) * size, INTERNAL_MEM_STATISTICS);
}
#endif

static void
clear_thread_store_remset_buffer (SgenThreadInfo *info)
{
	*info->store_remset_buffer_index_addr = 0;
	/* See the comment at the end of sgen_thread_unregister() */
	if (*info->store_remset_buffer_addr)
		memset (*info->store_remset_buffer_addr, 0, sizeof (gpointer) * STORE_REMSET_BUFFER_SIZE);
}

static size_t
remset_byte_size (RememberedSet *remset)
{
	return sizeof (RememberedSet) + (remset->end_set - remset->data) * sizeof (gpointer);
}

static void
scan_from_global_remsets (void *start_nursery, void *end_nursery, GrayQueue *queue)
{
	RememberedSet *remset;
	mword *p, *next_p, *store_pos;

	/* the global one */
	for (remset = global_remset; remset; remset = remset->next) {
		DEBUG (4, fprintf (gc_debug_file, "Scanning global remset range: %p-%p, size: %td\n", remset->data, remset->store_next, remset->store_next - remset->data));
		store_pos = remset->data;
		for (p = remset->data; p < remset->store_next; p = next_p) {
			void **ptr = (void**)p [0];

			/*Ignore previously processed remset.*/
			if (!global_remset_location_was_not_added (ptr)) {
				next_p = p + 1;
				continue;
			}

			next_p = handle_remset (p, start_nursery, end_nursery, TRUE, queue);

			/* 
			 * Clear global remsets of locations which no longer point to the 
			 * nursery. Otherwise, they could grow indefinitely between major 
			 * collections.
			 *
			 * Since all global remsets are location remsets, we don't need to unmask the pointer.
			 */
			if (ptr_in_nursery (*ptr)) {
				*store_pos ++ = p [0];
				HEAVY_STAT (++stat_global_remsets_readded);
			}
		}

		/* Truncate the remset */
		remset->store_next = store_pos;
	}
}

static void
scan_from_remsets (void *start_nursery, void *end_nursery, GrayQueue *queue)
{
	int i;
	SgenThreadInfo *info;
	RememberedSet *remset;
	GenericStoreRememberedSet *store_remset;
	mword *p;

#ifdef HEAVY_STATISTICS
	remset_stats ();
#endif

	/* the generic store ones */
	store_remset = generic_store_remsets;
	while (store_remset) {
		GenericStoreRememberedSet *next = store_remset->next;

		for (i = 0; i < STORE_REMSET_BUFFER_SIZE - 1; ++i) {
			gpointer addr = store_remset->data [i];
			if (addr)
				handle_remset ((mword*)&addr, start_nursery, end_nursery, FALSE, queue);
		}

		mono_sgen_free_internal (store_remset, INTERNAL_MEM_STORE_REMSET);

		store_remset = next;
	}
	generic_store_remsets = NULL;

	/* the per-thread ones */
	FOREACH_THREAD (info) {
		RememberedSet *next;
		int j;
		for (remset = info->remset; remset; remset = next) {
			DEBUG (4, fprintf (gc_debug_file, "Scanning remset for thread %p, range: %p-%p, size: %td\n", info, remset->data, remset->store_next, remset->store_next - remset->data));
			for (p = remset->data; p < remset->store_next;)
				p = handle_remset (p, start_nursery, end_nursery, FALSE, queue);
			remset->store_next = remset->data;
			next = remset->next;
			remset->next = NULL;
			if (remset != info->remset) {
				DEBUG (4, fprintf (gc_debug_file, "Freed remset at %p\n", remset->data));
				mono_sgen_free_internal_dynamic (remset, remset_byte_size (remset), INTERNAL_MEM_REMSET);
			}
		}
		for (j = 0; j < *info->store_remset_buffer_index_addr; ++j)
			handle_remset ((mword*)*info->store_remset_buffer_addr + j + 1, start_nursery, end_nursery, FALSE, queue);
		clear_thread_store_remset_buffer (info);
	} END_FOREACH_THREAD

	/* the freed thread ones */
	while (freed_thread_remsets) {
		RememberedSet *next;
		remset = freed_thread_remsets;
		DEBUG (4, fprintf (gc_debug_file, "Scanning remset for freed thread, range: %p-%p, size: %td\n", remset->data, remset->store_next, remset->store_next - remset->data));
		for (p = remset->data; p < remset->store_next;)
			p = handle_remset (p, start_nursery, end_nursery, FALSE, queue);
		next = remset->next;
		DEBUG (4, fprintf (gc_debug_file, "Freed remset at %p\n", remset->data));
		mono_sgen_free_internal_dynamic (remset, remset_byte_size (remset), INTERNAL_MEM_REMSET);
		freed_thread_remsets = next;
	}
}

/*
 * Clear the info in the remembered sets: we're doing a major collection, so
 * the per-thread ones are not needed and the global ones will be reconstructed
 * during the copy.
 */
static void
clear_remsets (void)
{
	SgenThreadInfo *info;
	RememberedSet *remset, *next;

	/* the global list */
	for (remset = global_remset; remset; remset = next) {
		remset->store_next = remset->data;
		next = remset->next;
		remset->next = NULL;
		if (remset != global_remset) {
			DEBUG (4, fprintf (gc_debug_file, "Freed remset at %p\n", remset->data));
			mono_sgen_free_internal_dynamic (remset, remset_byte_size (remset), INTERNAL_MEM_REMSET);
		}
	}
	/* the generic store ones */
	while (generic_store_remsets) {
		GenericStoreRememberedSet *gs_next = generic_store_remsets->next;
		mono_sgen_free_internal (generic_store_remsets, INTERNAL_MEM_STORE_REMSET);
		generic_store_remsets = gs_next;
	}
	/* the per-thread ones */
	FOREACH_THREAD (info) {
		for (remset = info->remset; remset; remset = next) {
			remset->store_next = remset->data;
			next = remset->next;
			remset->next = NULL;
			if (remset != info->remset) {
				DEBUG (3, fprintf (gc_debug_file, "Freed remset at %p\n", remset->data));
				mono_sgen_free_internal_dynamic (remset, remset_byte_size (remset), INTERNAL_MEM_REMSET);
			}
		}
		clear_thread_store_remset_buffer (info);
	} END_FOREACH_THREAD

	/* the freed thread ones */
	while (freed_thread_remsets) {
		next = freed_thread_remsets->next;
		DEBUG (4, fprintf (gc_debug_file, "Freed remset at %p\n", freed_thread_remsets->data));
		mono_sgen_free_internal_dynamic (freed_thread_remsets, remset_byte_size (freed_thread_remsets), INTERNAL_MEM_REMSET);
		freed_thread_remsets = next;
	}
}

/*
 * Clear the thread local TLAB variables for all threads.
 */
static void
clear_tlabs (void)
{
	SgenThreadInfo *info;

	FOREACH_THREAD (info) {
		/* A new TLAB will be allocated when the thread does its first allocation */
		*info->tlab_start_addr = NULL;
		*info->tlab_next_addr = NULL;
		*info->tlab_temp_end_addr = NULL;
		*info->tlab_real_end_addr = NULL;
	} END_FOREACH_THREAD
}

static void*
sgen_thread_register (SgenThreadInfo* info, void *addr)
{
#ifndef HAVE_KW_THREAD
	SgenThreadInfo *__thread_info__ = info;
#endif

	LOCK_GC;
#ifndef HAVE_KW_THREAD
	info->tlab_start = info->tlab_next = info->tlab_temp_end = info->tlab_real_end = NULL;

	g_assert (!mono_native_tls_get_value (thread_info_key));
	mono_native_tls_set_value (thread_info_key, info);
#else
	thread_info = info;
#endif

#if !defined(__MACH__)
	info->stop_count = -1;
	info->signal = 0;
#endif
	info->skip = 0;
	info->doing_handshake = FALSE;
	info->thread_is_dying = FALSE;
	info->stack_start = NULL;
	info->tlab_start_addr = &TLAB_START;
	info->tlab_next_addr = &TLAB_NEXT;
	info->tlab_temp_end_addr = &TLAB_TEMP_END;
	info->tlab_real_end_addr = &TLAB_REAL_END;
	info->store_remset_buffer_addr = &STORE_REMSET_BUFFER;
	info->store_remset_buffer_index_addr = &STORE_REMSET_BUFFER_INDEX;
	info->stopped_ip = NULL;
	info->stopped_domain = NULL;
#ifdef USE_MONO_CTX
	info->monoctx = NULL;
#else
	info->stopped_regs = NULL;
#endif

	binary_protocol_thread_register ((gpointer)mono_thread_info_get_tid (info));

#ifdef HAVE_KW_THREAD
	tlab_next_addr = &tlab_next;
	store_remset_buffer_index_addr = &store_remset_buffer_index;
#endif

#if defined(__MACH__)
	info->mach_port = mach_thread_self ();
#endif

	/* try to get it with attributes first */
#if defined(HAVE_PTHREAD_GETATTR_NP) && defined(HAVE_PTHREAD_ATTR_GETSTACK)
	{
		size_t size;
		void *sstart;
		pthread_attr_t attr;
		pthread_getattr_np (pthread_self (), &attr);
		pthread_attr_getstack (&attr, &sstart, &size);
		info->stack_start_limit = sstart;
		info->stack_end = (char*)sstart + size;
		pthread_attr_destroy (&attr);
	}
#elif defined(HAVE_PTHREAD_GET_STACKSIZE_NP) && defined(HAVE_PTHREAD_GET_STACKADDR_NP)
		 info->stack_end = (char*)pthread_get_stackaddr_np (pthread_self ());
		 info->stack_start_limit = (char*)info->stack_end - pthread_get_stacksize_np (pthread_self ());
#else
	{
		/* FIXME: we assume the stack grows down */
		gsize stack_bottom = (gsize)addr;
		stack_bottom += 4095;
		stack_bottom &= ~4095;
		info->stack_end = (char*)stack_bottom;
	}
#endif

#ifdef HAVE_KW_THREAD
	stack_end = info->stack_end;
#endif

	info->remset = alloc_remset (DEFAULT_REMSET_SIZE, info, FALSE);
	mono_native_tls_set_value (remembered_set_key, info->remset);
#ifdef HAVE_KW_THREAD
	remembered_set = info->remset;
#endif

	STORE_REMSET_BUFFER = mono_sgen_alloc_internal (INTERNAL_MEM_STORE_REMSET);
	STORE_REMSET_BUFFER_INDEX = 0;

	DEBUG (3, fprintf (gc_debug_file, "registered thread %p (%p) stack end %p\n", info, (gpointer)mono_thread_info_get_tid (info), info->stack_end));

	if (gc_callbacks.thread_attach_func)
		info->runtime_data = gc_callbacks.thread_attach_func ();

	UNLOCK_GC;
	return info;
}

static void
add_generic_store_remset_from_buffer (gpointer *buffer)
{
	GenericStoreRememberedSet *remset = mono_sgen_alloc_internal (INTERNAL_MEM_STORE_REMSET);
	memcpy (remset->data, buffer + 1, sizeof (gpointer) * (STORE_REMSET_BUFFER_SIZE - 1));
	remset->next = generic_store_remsets;
	generic_store_remsets = remset;
}

static void
sgen_thread_unregister (SgenThreadInfo *p)
{
	RememberedSet *rset;

	/* If a delegate is passed to native code and invoked on a thread we dont
	 * know about, the jit will register it with mono_jit_thread_attach, but
	 * we have no way of knowing when that thread goes away.  SGen has a TSD
	 * so we assume that if the domain is still registered, we can detach
	 * the thread
	 */
	if (mono_domain_get ())
		mono_thread_detach (mono_thread_current ());

	p->thread_is_dying = TRUE;

	/*
	There is a race condition between a thread finishing executing and been removed
	from the GC thread set.
	This happens on posix systems when TLS data is been cleaned-up, libpthread will
	set the thread_info slot to NULL before calling the cleanup function. This
	opens a window in which the thread is registered but has a NULL TLS.

	The suspend signal handler needs TLS data to know where to store thread state
	data or otherwise it will simply ignore the thread.

	This solution works because the thread doing STW will wait until all threads been
	suspended handshake back, so there is no race between the doing_hankshake test
	and the suspend_thread call.

	This is not required on systems that do synchronous STW as those can deal with
	the above race at suspend time.

	FIXME: I believe we could avoid this by using mono_thread_info_lookup when
	mono_thread_info_current returns NULL. Or fix mono_thread_info_lookup to do so.
	*/
#if (defined(__MACH__) && MONO_MACH_ARCH_SUPPORTED) || !defined(HAVE_PTHREAD_KILL)
	LOCK_GC;
#else
	while (!TRYLOCK_GC) {
		if (!mono_sgen_park_current_thread_if_doing_handshake (p))
			g_usleep (50);
	}
#endif

	binary_protocol_thread_unregister ((gpointer)mono_thread_info_get_tid (p));
	DEBUG (3, fprintf (gc_debug_file, "unregister thread %p (%p)\n", p, (gpointer)mono_thread_info_get_tid (p)));

#if defined(__MACH__)
	mach_port_deallocate (current_task (), p->mach_port);
#endif

	if (gc_callbacks.thread_detach_func) {
		gc_callbacks.thread_detach_func (p->runtime_data);
		p->runtime_data = NULL;
	}

	if (p->remset) {
		if (freed_thread_remsets) {
			for (rset = p->remset; rset->next; rset = rset->next)
				;
			rset->next = freed_thread_remsets;
			freed_thread_remsets = p->remset;
		} else {
			freed_thread_remsets = p->remset;
		}
	}
	if (*p->store_remset_buffer_index_addr)
		add_generic_store_remset_from_buffer (*p->store_remset_buffer_addr);
	mono_sgen_free_internal (*p->store_remset_buffer_addr, INTERNAL_MEM_STORE_REMSET);
	/*
	 * This is currently not strictly required, but we do it
	 * anyway in case we change thread unregistering:

	 * If the thread is removed from the thread list after
	 * unregistering (this is currently not the case), and a
	 * collection occurs, clear_remsets() would want to memset
	 * this buffer, which would either clobber memory or crash.
	 */
	*p->store_remset_buffer_addr = NULL;

	mono_threads_unregister_current_thread (p);
	UNLOCK_GC;
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
	pthread_exit (retval);
}

#endif /* USE_PTHREAD_INTERCEPT */

/*
 * ######################################################################
 * ########  Write barriers
 * ######################################################################
 */

/*
 * This causes the compile to extend the liveness of 'v' till the call to dummy_use
 */
static void
dummy_use (gpointer v) {
	__asm__ volatile ("" : "=r"(v) : "r"(v));
}


static RememberedSet*
alloc_remset (int size, gpointer id, gboolean global)
{
	RememberedSet* res = mono_sgen_alloc_internal_dynamic (sizeof (RememberedSet) + (size * sizeof (gpointer)), INTERNAL_MEM_REMSET);
	res->store_next = res->data;
	res->end_set = res->data + size;
	res->next = NULL;
	DEBUG (4, fprintf (gc_debug_file, "Allocated%s remset size %d at %p for %p\n", global ? " global" : "", size, res->data, id));
	return res;
}

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
	DEBUG (8, fprintf (gc_debug_file, "Adding remset at %p\n", field_ptr));
	if (value)
		binary_protocol_wbarrier (field_ptr, value, value->vtable);
	if (use_cardtable) {
		*(void**)field_ptr = value;
		if (ptr_in_nursery (value))
			sgen_card_table_mark_address ((mword)field_ptr);
		dummy_use (value);
	} else {
		RememberedSet *rs;
		TLAB_ACCESS_INIT;

		LOCK_GC;
		rs = REMEMBERED_SET;
		if (rs->store_next < rs->end_set) {
			*(rs->store_next++) = (mword)field_ptr;
			*(void**)field_ptr = value;
			UNLOCK_GC;
			return;
		}
		rs = alloc_remset (rs->end_set - rs->data, (void*)1, FALSE);
		rs->next = REMEMBERED_SET;
		REMEMBERED_SET = rs;
#ifdef HAVE_KW_THREAD
		mono_thread_info_current ()->remset = rs;
#endif
		*(rs->store_next++) = (mword)field_ptr;
		*(void**)field_ptr = value;
		UNLOCK_GC;
	}
}

void
mono_gc_wbarrier_set_arrayref (MonoArray *arr, gpointer slot_ptr, MonoObject* value)
{
	HEAVY_STAT (++stat_wbarrier_set_arrayref);
	if (ptr_in_nursery (slot_ptr)) {
		*(void**)slot_ptr = value;
		return;
	}
	DEBUG (8, fprintf (gc_debug_file, "Adding remset at %p\n", slot_ptr));
	if (value)
		binary_protocol_wbarrier (slot_ptr, value, value->vtable);
	if (use_cardtable) {
		*(void**)slot_ptr = value;
		if (ptr_in_nursery (value))
			sgen_card_table_mark_address ((mword)slot_ptr);
		dummy_use (value);
	} else {
		RememberedSet *rs;
		TLAB_ACCESS_INIT;

		LOCK_GC;
		rs = REMEMBERED_SET;
		if (rs->store_next < rs->end_set) {
			*(rs->store_next++) = (mword)slot_ptr;
			*(void**)slot_ptr = value;
			UNLOCK_GC;
			return;
		}
		rs = alloc_remset (rs->end_set - rs->data, (void*)1, FALSE);
		rs->next = REMEMBERED_SET;
		REMEMBERED_SET = rs;
#ifdef HAVE_KW_THREAD
		mono_thread_info_current ()->remset = rs;
#endif
		*(rs->store_next++) = (mword)slot_ptr;
		*(void**)slot_ptr = value;
		UNLOCK_GC;
	}
}

void
mono_gc_wbarrier_arrayref_copy (gpointer dest_ptr, gpointer src_ptr, int count)
{
	HEAVY_STAT (++stat_wbarrier_arrayref_copy);
	/*This check can be done without taking a lock since dest_ptr array is pinned*/
	if (ptr_in_nursery (dest_ptr) || count <= 0) {
		mono_gc_memmove (dest_ptr, src_ptr, count * sizeof (gpointer));
		return;
	}

#ifdef SGEN_BINARY_PROTOCOL
	{
		int i;
		for (i = 0; i < count; ++i) {
			gpointer dest = (gpointer*)dest_ptr + i;
			gpointer obj = *((gpointer*)src_ptr + i);
			if (obj)
				binary_protocol_wbarrier (dest, obj, (gpointer)LOAD_VTABLE (obj));
		}
	}
#endif

	if (use_cardtable) {
		gpointer *dest = dest_ptr;
		gpointer *src = src_ptr;

		/*overlapping that required backward copying*/
		if (src < dest && (src + count) > dest) {
			gpointer *start = dest;
			dest += count - 1;
			src += count - 1;

			for (; dest >= start; --src, --dest) {
				gpointer value = *src;
				*dest = value;
				if (ptr_in_nursery (value))
					sgen_card_table_mark_address ((mword)dest);
				dummy_use (value);
			}
		} else {
			gpointer *end = dest + count;
			for (; dest < end; ++src, ++dest) {
				gpointer value = *src;
				*dest = value;
				if (ptr_in_nursery (value))
					sgen_card_table_mark_address ((mword)dest);
				dummy_use (value);
			}
		}
	} else {
		RememberedSet *rs;
		TLAB_ACCESS_INIT;
		LOCK_GC;
		mono_gc_memmove (dest_ptr, src_ptr, count * sizeof (gpointer));

		rs = REMEMBERED_SET;
		DEBUG (8, fprintf (gc_debug_file, "Adding remset at %p, %d\n", dest_ptr, count));
		if (rs->store_next + 1 < rs->end_set) {
			*(rs->store_next++) = (mword)dest_ptr | REMSET_RANGE;
			*(rs->store_next++) = count;
			UNLOCK_GC;
			return;
		}
		rs = alloc_remset (rs->end_set - rs->data, (void*)1, FALSE);
		rs->next = REMEMBERED_SET;
		REMEMBERED_SET = rs;
#ifdef HAVE_KW_THREAD
		mono_thread_info_current ()->remset = rs;
#endif
		*(rs->store_next++) = (mword)dest_ptr | REMSET_RANGE;
		*(rs->store_next++) = count;

		UNLOCK_GC;
	}
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
		mono_sgen_scan_area_with_callback (nursery_section->data, nursery_section->end_data,
				find_object_for_ptr_callback, ptr, TRUE);
		if (found_obj)
			return found_obj;
	}

	found_obj = NULL;
	mono_sgen_los_iterate_objects (find_object_for_ptr_callback, ptr);
	if (found_obj)
		return found_obj;

	/*
	 * Very inefficient, but this is debugging code, supposed to
	 * be called from gdb, so we don't care.
	 */
	found_obj = NULL;
	major_collector.iterate_objects (TRUE, TRUE, find_object_for_ptr_callback, ptr);
	return found_obj;
}

static void
evacuate_remset_buffer (void)
{
	gpointer *buffer;
	TLAB_ACCESS_INIT;

	buffer = STORE_REMSET_BUFFER;

	add_generic_store_remset_from_buffer (buffer);
	memset (buffer, 0, sizeof (gpointer) * STORE_REMSET_BUFFER_SIZE);

	STORE_REMSET_BUFFER_INDEX = 0;
}

void
mono_gc_wbarrier_generic_nostore (gpointer ptr)
{
	gpointer *buffer;
	int index;
	TLAB_ACCESS_INIT;

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

	if (*(gpointer*)ptr)
		binary_protocol_wbarrier (ptr, *(gpointer*)ptr, (gpointer)LOAD_VTABLE (*(gpointer*)ptr));

	if (ptr_in_nursery (ptr) || ptr_on_stack (ptr) || !ptr_in_nursery (*(gpointer*)ptr)) {
		DEBUG (8, fprintf (gc_debug_file, "Skipping remset at %p\n", ptr));
		return;
	}

	if (use_cardtable) {
		if (ptr_in_nursery(*(gpointer*)ptr))
			sgen_card_table_mark_address ((mword)ptr);
		return;
	}

	LOCK_GC;

	buffer = STORE_REMSET_BUFFER;
	index = STORE_REMSET_BUFFER_INDEX;
	/* This simple optimization eliminates a sizable portion of
	   entries.  Comparing it to the last but one entry as well
	   doesn't eliminate significantly more entries. */
	if (buffer [index] == ptr) {
		UNLOCK_GC;
		return;
	}

	DEBUG (8, fprintf (gc_debug_file, "Adding remset at %p\n", ptr));
	HEAVY_STAT (++stat_wbarrier_generic_store_remset);

	++index;
	if (index >= STORE_REMSET_BUFFER_SIZE) {
		evacuate_remset_buffer ();
		index = STORE_REMSET_BUFFER_INDEX;
		g_assert (index == 0);
		++index;
	}
	buffer [index] = ptr;
	STORE_REMSET_BUFFER_INDEX = index;

	UNLOCK_GC;
}

void
mono_gc_wbarrier_generic_store (gpointer ptr, MonoObject* value)
{
	DEBUG (8, fprintf (gc_debug_file, "Wbarrier store at %p to %p (%s)\n", ptr, value, value ? safe_name (value) : "null"));
	*(void**)ptr = value;
	if (ptr_in_nursery (value))
		mono_gc_wbarrier_generic_nostore (ptr);
	dummy_use (value);
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

#ifdef SGEN_BINARY_PROTOCOL
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
	RememberedSet *rs;
	size_t element_size = mono_class_value_size (klass, NULL);
	size_t size = count * element_size;
	TLAB_ACCESS_INIT;
	HEAVY_STAT (++stat_wbarrier_value_copy);
	g_assert (klass->valuetype);
#ifdef SGEN_BINARY_PROTOCOL
	{
		int i;
		for (i = 0; i < count; ++i) {
			scan_object_for_binary_protocol_copy_wbarrier ((char*)dest + i * element_size,
					(char*)src + i * element_size - sizeof (MonoObject),
					(mword) klass->gc_descr);
		}
	}
#endif
	if (use_cardtable) {
#ifdef DISABLE_CRITICAL_REGION
		LOCK_GC;
#else
		ENTER_CRITICAL_REGION;
#endif
		mono_gc_memmove (dest, src, size);
		sgen_card_table_mark_range ((mword)dest, size);
#ifdef DISABLE_CRITICAL_REGION
		UNLOCK_GC;
#else
		EXIT_CRITICAL_REGION;
#endif
	} else {
		LOCK_GC;
		mono_gc_memmove (dest, src, size);
		rs = REMEMBERED_SET;
		if (ptr_in_nursery (dest) || ptr_on_stack (dest) || !SGEN_CLASS_HAS_REFERENCES (klass)) {
			UNLOCK_GC;
			return;
		}
		g_assert (klass->gc_descr_inited);
		DEBUG (8, fprintf (gc_debug_file, "Adding value remset at %p, count %d, descr %p for class %s (%p)\n", dest, count, klass->gc_descr, klass->name, klass));

		if (rs->store_next + 4 < rs->end_set) {
			*(rs->store_next++) = (mword)dest | REMSET_VTYPE;
			*(rs->store_next++) = (mword)klass->gc_descr;
			*(rs->store_next++) = (mword)count;
			*(rs->store_next++) = (mword)element_size;
			UNLOCK_GC;
			return;
		}
		rs = alloc_remset (rs->end_set - rs->data, (void*)1, FALSE);
		rs->next = REMEMBERED_SET;
		REMEMBERED_SET = rs;
#ifdef HAVE_KW_THREAD
		mono_thread_info_current ()->remset = rs;
#endif
		*(rs->store_next++) = (mword)dest | REMSET_VTYPE;
		*(rs->store_next++) = (mword)klass->gc_descr;
		*(rs->store_next++) = (mword)count;
		*(rs->store_next++) = (mword)element_size;
		UNLOCK_GC;
	}
}

/**
 * mono_gc_wbarrier_object_copy:
 *
 * Write barrier to call when obj is the result of a clone or copy of an object.
 */
void
mono_gc_wbarrier_object_copy (MonoObject* obj, MonoObject *src)
{
	RememberedSet *rs;
	int size;

	TLAB_ACCESS_INIT;
	HEAVY_STAT (++stat_wbarrier_object_copy);
	rs = REMEMBERED_SET;
	DEBUG (6, fprintf (gc_debug_file, "Adding object remset for %p\n", obj));
	size = mono_object_class (obj)->instance_size;
	LOCK_GC;
#ifdef SGEN_BINARY_PROTOCOL
	scan_object_for_binary_protocol_copy_wbarrier (obj, (char*)src, (mword) src->vtable->gc_descr);
#endif
	/* do not copy the sync state */
	mono_gc_memmove ((char*)obj + sizeof (MonoObject), (char*)src + sizeof (MonoObject),
			size - sizeof (MonoObject));
	if (ptr_in_nursery (obj) || ptr_on_stack (obj)) {
		UNLOCK_GC;
		return;
	}
	if (rs->store_next < rs->end_set) {
		*(rs->store_next++) = (mword)obj | REMSET_OBJECT;
		UNLOCK_GC;
		return;
	}
	rs = alloc_remset (rs->end_set - rs->data, (void*)1, FALSE);
	rs->next = REMEMBERED_SET;
	REMEMBERED_SET = rs;

#ifdef HAVE_KW_THREAD
	mono_thread_info_current ()->remset = rs;
#endif
	*(rs->store_next++) = (mword)obj | REMSET_OBJECT;
	UNLOCK_GC;
}

/*
 * ######################################################################
 * ########  Collector debugging
 * ######################################################################
 */

const char*descriptor_types [] = {
	"run_length",
	"small_bitmap",
	"string",
	"complex",
	"vector",
	"array",
	"large_bitmap",
	"complex_arr"
};

void
describe_ptr (char *ptr)
{
	MonoVTable *vtable;
	mword desc;
	int type;
	char *start;

	if (ptr_in_nursery (ptr)) {
		printf ("Pointer inside nursery.\n");
	} else {
		if (mono_sgen_ptr_is_in_los (ptr, &start)) {
			if (ptr == start)
				printf ("Pointer is the start of object %p in LOS space.\n", start);
			else
				printf ("Pointer is at offset 0x%x of object %p in LOS space.\n", (int)(ptr - start), start);
			ptr = start;
		} else if (major_collector.ptr_is_in_non_pinned_space (ptr)) {
			printf ("Pointer inside oldspace.\n");
		} else if (major_collector.obj_is_from_pinned_alloc (ptr)) {
			printf ("Pointer is inside a pinned chunk.\n");
		} else {
			printf ("Pointer unknown.\n");
			return;
		}
	}

	if (object_is_pinned (ptr))
		printf ("Object is pinned.\n");

	if (object_is_forwarded (ptr))
		printf ("Object is forwared.\n");

	// FIXME: Handle pointers to the inside of objects
	vtable = (MonoVTable*)LOAD_VTABLE (ptr);

	printf ("VTable: %p\n", vtable);
	if (vtable == NULL) {
		printf ("VTable is invalid (empty).\n");
		return;
	}
	if (ptr_in_nursery (vtable)) {
		printf ("VTable is invalid (points inside nursery).\n");
		return;
	}
	printf ("Class: %s\n", vtable->klass->name);

	desc = ((GCVTable*)vtable)->desc;
	printf ("Descriptor: %lx\n", (long)desc);

	type = desc & 0x7;
	printf ("Descriptor type: %d (%s)\n", type, descriptor_types [type]);
}

static mword*
find_in_remset_loc (mword *p, char *addr, gboolean *found)
{
	void **ptr;
	mword count, desc;
	size_t skip_size;

	switch ((*p) & REMSET_TYPE_MASK) {
	case REMSET_LOCATION:
		if (*p == (mword)addr)
			*found = TRUE;
		return p + 1;
	case REMSET_RANGE:
		ptr = (void**)(*p & ~REMSET_TYPE_MASK);
		count = p [1];
		if ((void**)addr >= ptr && (void**)addr < ptr + count)
			*found = TRUE;
		return p + 2;
	case REMSET_OBJECT:
		ptr = (void**)(*p & ~REMSET_TYPE_MASK);
		count = safe_object_get_size ((MonoObject*)ptr); 
		count = ALIGN_UP (count);
		count /= sizeof (mword);
		if ((void**)addr >= ptr && (void**)addr < ptr + count)
			*found = TRUE;
		return p + 1;
	case REMSET_VTYPE:
		ptr = (void**)(*p & ~REMSET_TYPE_MASK);
		desc = p [1];
		count = p [2];
		skip_size = p [3];

		/* The descriptor includes the size of MonoObject */
		skip_size -= sizeof (MonoObject);
		skip_size *= count;
		if ((void**)addr >= ptr && (void**)addr < ptr + (skip_size / sizeof (gpointer)))
			*found = TRUE;

		return p + 4;
	default:
		g_assert_not_reached ();
	}
	return NULL;
}

/*
 * Return whenever ADDR occurs in the remembered sets
 */
static gboolean
find_in_remsets (char *addr)
{
	int i;
	SgenThreadInfo *info;
	RememberedSet *remset;
	GenericStoreRememberedSet *store_remset;
	mword *p;
	gboolean found = FALSE;

	/* the global one */
	for (remset = global_remset; remset; remset = remset->next) {
		DEBUG (4, fprintf (gc_debug_file, "Scanning global remset range: %p-%p, size: %td\n", remset->data, remset->store_next, remset->store_next - remset->data));
		for (p = remset->data; p < remset->store_next;) {
			p = find_in_remset_loc (p, addr, &found);
			if (found)
				return TRUE;
		}
	}

	/* the generic store ones */
	for (store_remset = generic_store_remsets; store_remset; store_remset = store_remset->next) {
		for (i = 0; i < STORE_REMSET_BUFFER_SIZE - 1; ++i) {
			if (store_remset->data [i] == addr)
				return TRUE;
		}
	}

	/* the per-thread ones */
	FOREACH_THREAD (info) {
		int j;
		for (remset = info->remset; remset; remset = remset->next) {
			DEBUG (4, fprintf (gc_debug_file, "Scanning remset for thread %p, range: %p-%p, size: %td\n", info, remset->data, remset->store_next, remset->store_next - remset->data));
			for (p = remset->data; p < remset->store_next;) {
				p = find_in_remset_loc (p, addr, &found);
				if (found)
					return TRUE;
			}
		}
		for (j = 0; j < *info->store_remset_buffer_index_addr; ++j) {
			if ((*info->store_remset_buffer_addr) [j + 1] == addr)
				return TRUE;
		}
	} END_FOREACH_THREAD

	/* the freed thread ones */
	for (remset = freed_thread_remsets; remset; remset = remset->next) {
		DEBUG (4, fprintf (gc_debug_file, "Scanning remset for freed thread, range: %p-%p, size: %td\n", remset->data, remset->store_next, remset->store_next - remset->data));
		for (p = remset->data; p < remset->store_next;) {
			p = find_in_remset_loc (p, addr, &found);
			if (found)
				return TRUE;
		}
	}

	return FALSE;
}

static gboolean missing_remsets;

/*
 * We let a missing remset slide if the target object is pinned,
 * because the store might have happened but the remset not yet added,
 * but in that case the target must be pinned.  We might theoretically
 * miss some missing remsets this way, but it's very unlikely.
 */
#undef HANDLE_PTR
#define HANDLE_PTR(ptr,obj)	do {	\
		if (*(ptr) && (char*)*(ptr) >= nursery_start && (char*)*(ptr) < nursery_end) {	\
		if (!find_in_remsets ((char*)(ptr)) && (!use_cardtable || !sgen_card_table_address_is_marked ((mword)ptr))) { \
                fprintf (gc_debug_file, "Oldspace->newspace reference %p at offset %td in object %p (%s.%s) not found in remsets.\n", *(ptr), (char*)(ptr) - (char*)(obj), (obj), ((MonoObject*)(obj))->vtable->klass->name_space, ((MonoObject*)(obj))->vtable->klass->name); \
		binary_protocol_missing_remset ((obj), (gpointer)LOAD_VTABLE ((obj)), (char*)(ptr) - (char*)(obj), *(ptr), (gpointer)LOAD_VTABLE(*(ptr)), object_is_pinned (*(ptr))); \
		if (!object_is_pinned (*(ptr)))				\
			missing_remsets = TRUE;				\
            } \
        } \
	} while (0)

/*
 * Check that each object reference which points into the nursery can
 * be found in the remembered sets.
 */
static void
check_consistency_callback (char *start, size_t size, void *dummy)
{
	GCVTable *vt = (GCVTable*)LOAD_VTABLE (start);
	DEBUG (8, fprintf (gc_debug_file, "Scanning object %p, vtable: %p (%s)\n", start, vt, vt->klass->name));

#define SCAN_OBJECT_ACTION
#include "sgen-scan-object.h"
}

/*
 * Perform consistency check of the heap.
 *
 * Assumes the world is stopped.
 */
static void
check_consistency (void)
{
	// Need to add more checks

	missing_remsets = FALSE;

	DEBUG (1, fprintf (gc_debug_file, "Begin heap consistency check...\n"));

	// Check that oldspace->newspace pointers are registered with the collector
	major_collector.iterate_objects (TRUE, TRUE, (IterateObjectCallbackFunc)check_consistency_callback, NULL);

	mono_sgen_los_iterate_objects ((IterateObjectCallbackFunc)check_consistency_callback, NULL);

	DEBUG (1, fprintf (gc_debug_file, "Heap consistency check done.\n"));

	if (!binary_protocol_is_enabled ())
		g_assert (!missing_remsets);
}


#undef HANDLE_PTR
#define HANDLE_PTR(ptr,obj)	do {					\
		if (*(ptr) && !LOAD_VTABLE (*(ptr)))						\
			g_error ("Could not load vtable for obj %p slot %d (size %d)", obj, (char*)ptr - (char*)obj, safe_object_get_size ((MonoObject*)obj));		\
	} while (0)

static void
check_major_refs_callback (char *start, size_t size, void *dummy)
{
#define SCAN_OBJECT_ACTION
#include "sgen-scan-object.h"
}

static void
check_major_refs (void)
{
	major_collector.iterate_objects (TRUE, TRUE, (IterateObjectCallbackFunc)check_major_refs_callback, NULL);
	mono_sgen_los_iterate_objects ((IterateObjectCallbackFunc)check_major_refs_callback, NULL);
}

/* Check that the reference is valid */
#undef HANDLE_PTR
#define HANDLE_PTR(ptr,obj)	do {	\
		if (*(ptr)) {	\
			g_assert (safe_name (*(ptr)) != NULL);	\
		}	\
	} while (0)

/*
 * check_object:
 *
 *   Perform consistency check on an object. Currently we only check that the
 * reference fields are valid.
 */
void
check_object (char *start)
{
	if (!start)
		return;

#include "sgen-scan-object.h"
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

	mono_sgen_clear_nursery_fragments ();
	mono_sgen_scan_area_with_callback (nursery_section->data, nursery_section->end_data, walk_references, &hwi, FALSE);

	major_collector.iterate_objects (TRUE, TRUE, walk_references, &hwi);
	mono_sgen_los_iterate_objects (walk_references, &hwi);

	return 0;
}

void
mono_gc_collect (int generation)
{
	LOCK_GC;
	if (generation > 1)
		generation = 1;
	mono_profiler_gc_event (MONO_GC_EVENT_START, generation);
	stop_world (generation);
	if (generation == 0) {
		collect_nursery (0);
	} else {
		major_collection ("user request");
	}
	restart_world (generation);
	mono_profiler_gc_event (MONO_GC_EVENT_END, generation);
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
		return num_minor_gcs;
	return num_major_gcs;
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

int64_t
mono_gc_get_heap_size (void)
{
	return total_alloc;
}

void
mono_gc_disable (void)
{
	LOCK_GC;
	gc_disabled++;
	UNLOCK_GC;
}

void
mono_gc_enable (void)
{
	LOCK_GC;
	gc_disabled--;
	UNLOCK_GC;
}

int
mono_gc_get_los_limit (void)
{
	return MAX_SMALL_OBJ_SIZE;
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
	mono_gc_register_disappearing_link (obj, link_addr, track, FALSE);
}

void
mono_gc_weak_link_remove (void **link_addr)
{
	mono_gc_register_disappearing_link (NULL, link_addr, FALSE, FALSE);
}

MonoObject*
mono_gc_weak_link_get (void **link_addr)
{
	if (!*link_addr)
		return NULL;
	return (MonoObject*) REVEAL_POINTER (*link_addr);
}

gboolean
mono_gc_ephemeron_array_add (MonoObject *obj)
{
	EphemeronLinkNode *node;

	LOCK_GC;

	node = mono_sgen_alloc_internal (INTERNAL_MEM_EPHEMERON_LINK);
	if (!node) {
		UNLOCK_GC;
		return FALSE;
	}
	node->array = (char*)obj;
	node->next = ephemeron_list;
	ephemeron_list = node;

	DEBUG (5, fprintf (gc_debug_file, "Registered ephemeron array %p\n", obj));

	UNLOCK_GC;
	return TRUE;
}

void*
mono_gc_make_descr_from_bitmap (gsize *bitmap, int numbits)
{
	if (numbits == 0) {
		return (void*)MAKE_ROOT_DESC (ROOT_DESC_BITMAP, 0);
	} else if (numbits < ((sizeof (*bitmap) * 8) - ROOT_DESC_TYPE_SHIFT)) {
		return (void*)MAKE_ROOT_DESC (ROOT_DESC_BITMAP, bitmap [0]);
	} else {
		mword complex = alloc_complex_descriptor (bitmap, numbits);
		return (void*)MAKE_ROOT_DESC (ROOT_DESC_COMPLEX, complex);
	}
}

 static void *all_ref_root_descrs [32];

void*
mono_gc_make_root_descr_all_refs (int numbits)
{
	gsize *gc_bitmap;
	void *descr;
	int num_bytes = numbits / 8;

	if (numbits < 32 && all_ref_root_descrs [numbits])
		return all_ref_root_descrs [numbits];

	gc_bitmap = g_malloc0 (ALIGN_TO (ALIGN_TO (numbits, 8) + 1, sizeof (gsize)));
	memset (gc_bitmap, 0xff, num_bytes);
	if (numbits < ((sizeof (*gc_bitmap) * 8) - ROOT_DESC_TYPE_SHIFT)) 
		gc_bitmap[0] = GUINT64_TO_LE(gc_bitmap[0]);
	else if (numbits && num_bytes % (sizeof (*gc_bitmap)))
		gc_bitmap[num_bytes / 8] = GUINT64_TO_LE(gc_bitmap [num_bytes / 8]);
	if (numbits % 8)
		gc_bitmap [numbits / 8] = (1 << (numbits % 8)) - 1;
	descr = mono_gc_make_descr_from_bitmap (gc_bitmap, numbits);
	g_free (gc_bitmap);

	if (numbits < 32)
		all_ref_root_descrs [numbits] = descr;

	return descr;
}

void*
mono_gc_make_root_descr_user (MonoGCRootMarkFunc marker)
{
	void *descr;

	g_assert (user_descriptors_next < MAX_USER_DESCRIPTORS);
	descr = (void*)MAKE_ROOT_DESC (ROOT_DESC_USER, (mword)user_descriptors_next);
	user_descriptors [user_descriptors_next ++] = marker;

	return descr;
}

void*
mono_gc_alloc_fixed (size_t size, void *descr)
{
	/* FIXME: do a single allocation */
	void *res = calloc (1, size);
	if (!res)
		return NULL;
	if (!mono_gc_register_root (res, size, descr)) {
		free (res);
		res = NULL;
	}
	return res;
}

void
mono_gc_free_fixed (void* addr)
{
	mono_gc_deregister_root (addr);
	free (addr);
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
	return mono_runtime_is_critical_method (method) || mono_gc_is_critical_method (method);
}

void
mono_gc_base_init (void)
{
	MonoThreadInfoCallbacks cb;
	char *env;
	char **opts, **ptr;
	char *major_collector_opt = NULL;
	glong max_heap = 0;
	glong soft_limit = 0;
	int num_workers;
	int result;
	int dummy;

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

	LOCK_INIT (gc_mutex);

	pagesize = mono_pagesize ();
	gc_debug_file = stderr;

	cb.thread_register = sgen_thread_register;
	cb.thread_unregister = sgen_thread_unregister;
	cb.thread_attach = sgen_thread_attach;
	cb.mono_method_is_critical = (gpointer)is_critical_method;
#ifndef HOST_WIN32
	cb.mono_gc_pthread_create = (gpointer)mono_gc_pthread_create;
#endif

	mono_threads_init (&cb, sizeof (SgenThreadInfo));

	LOCK_INIT (interruption_mutex);
	LOCK_INIT (global_remset_mutex);
	LOCK_INIT (pin_queue_mutex);

	init_user_copy_or_mark_key ();

	if ((env = getenv ("MONO_GC_PARAMS"))) {
		opts = g_strsplit (env, ",", -1);
		for (ptr = opts; *ptr; ++ptr) {
			char *opt = *ptr;
			if (g_str_has_prefix (opt, "major=")) {
				opt = strchr (opt, '=') + 1;
				major_collector_opt = g_strdup (opt);
			}
		}
	} else {
		opts = NULL;
	}

	init_stats ();
	mono_sgen_init_internal_allocator ();
	mono_sgen_init_nursery_allocator ();

	mono_sgen_register_fixed_internal_mem_type (INTERNAL_MEM_SECTION, SGEN_SIZEOF_GC_MEM_SECTION);
	mono_sgen_register_fixed_internal_mem_type (INTERNAL_MEM_FINALIZE_READY_ENTRY, sizeof (FinalizeReadyEntry));
	mono_sgen_register_fixed_internal_mem_type (INTERNAL_MEM_GRAY_QUEUE, sizeof (GrayQueueSection));
	g_assert (sizeof (GenericStoreRememberedSet) == sizeof (gpointer) * STORE_REMSET_BUFFER_SIZE);
	mono_sgen_register_fixed_internal_mem_type (INTERNAL_MEM_STORE_REMSET, sizeof (GenericStoreRememberedSet));
	mono_sgen_register_fixed_internal_mem_type (INTERNAL_MEM_EPHEMERON_LINK, sizeof (EphemeronLinkNode));

	mono_native_tls_alloc (&remembered_set_key, NULL);

#ifndef HAVE_KW_THREAD
	mono_native_tls_alloc (&thread_info_key, NULL);
#endif

	/*
	 * This needs to happen before any internal allocations because
	 * it inits the small id which is required for hazard pointer
	 * operations.
	 */
	mono_sgen_os_init ();

	mono_thread_info_attach (&dummy);

	if (!major_collector_opt || !strcmp (major_collector_opt, "marksweep")) {
		mono_sgen_marksweep_init (&major_collector);
	} else if (!major_collector_opt || !strcmp (major_collector_opt, "marksweep-fixed")) {
		mono_sgen_marksweep_fixed_init (&major_collector);
	} else if (!major_collector_opt || !strcmp (major_collector_opt, "marksweep-par")) {
		mono_sgen_marksweep_par_init (&major_collector);
	} else if (!major_collector_opt || !strcmp (major_collector_opt, "marksweep-fixed-par")) {
		mono_sgen_marksweep_fixed_par_init (&major_collector);
	} else if (!strcmp (major_collector_opt, "copying")) {
		mono_sgen_copying_init (&major_collector);
	} else {
		fprintf (stderr, "Unknown major collector `%s'.\n", major_collector_opt);
		exit (1);
	}

#ifdef SGEN_HAVE_CARDTABLE
	use_cardtable = major_collector.supports_cardtable;
#else
	use_cardtable = FALSE;
#endif

	num_workers = mono_cpu_count ();
	g_assert (num_workers > 0);
	if (num_workers > 16)
		num_workers = 16;

	///* Keep this the default for now */
	conservative_stack_mark = TRUE;

	if (opts) {
		for (ptr = opts; *ptr; ++ptr) {
			char *opt = *ptr;
			if (g_str_has_prefix (opt, "major="))
				continue;
			if (g_str_has_prefix (opt, "wbarrier=")) {
				opt = strchr (opt, '=') + 1;
				if (strcmp (opt, "remset") == 0) {
					use_cardtable = FALSE;
				} else if (strcmp (opt, "cardtable") == 0) {
					if (!use_cardtable) {
						if (major_collector.supports_cardtable)
							fprintf (stderr, "The cardtable write barrier is not supported on this platform.\n");
						else
							fprintf (stderr, "The major collector does not support the cardtable write barrier.\n");
						exit (1);
					}
				}
				continue;
			}
			if (g_str_has_prefix (opt, "max-heap-size=")) {
				opt = strchr (opt, '=') + 1;
				if (*opt && mono_gc_parse_environment_string_extract_number (opt, &max_heap)) {
					if ((max_heap & (mono_pagesize () - 1))) {
						fprintf (stderr, "max-heap-size size must be a multiple of %d.\n", mono_pagesize ());
						exit (1);
					}
				} else {
					fprintf (stderr, "max-heap-size must be an integer.\n");
					exit (1);
				}
				continue;
			}
			if (g_str_has_prefix (opt, "soft-heap-limit=")) {
				opt = strchr (opt, '=') + 1;
				if (*opt && mono_gc_parse_environment_string_extract_number (opt, &soft_limit)) {
					if (soft_limit <= 0) {
						fprintf (stderr, "soft-heap-limit must be positive.\n");
						exit (1);
					}
				} else {
					fprintf (stderr, "soft-heap-limit must be an integer.\n");
					exit (1);
				}
				continue;
			}
			if (g_str_has_prefix (opt, "workers=")) {
				long val;
				char *endptr;
				if (!major_collector.is_parallel) {
					fprintf (stderr, "The workers= option can only be used for parallel collectors.");
					exit (1);
				}
				opt = strchr (opt, '=') + 1;
				val = strtol (opt, &endptr, 10);
				if (!*opt || *endptr) {
					fprintf (stderr, "Cannot parse the workers= option value.");
					exit (1);
				}
				if (val <= 0 || val > 16) {
					fprintf (stderr, "The number of workers must be in the range 1 to 16.");
					exit (1);
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
					fprintf (stderr, "Invalid value '%s' for stack-mark= option, possible values are: 'precise', 'conservative'.\n", opt);
					exit (1);
				}
				continue;
			}
#ifdef USER_CONFIG
			if (g_str_has_prefix (opt, "nursery-size=")) {
				long val;
				opt = strchr (opt, '=') + 1;
				if (*opt && mono_gc_parse_environment_string_extract_number (opt, &val)) {
					default_nursery_size = val;
#ifdef SGEN_ALIGN_NURSERY
					if ((val & (val - 1))) {
						fprintf (stderr, "The nursery size must be a power of two.\n");
						exit (1);
					}

					if (val < SGEN_MAX_NURSERY_WASTE) {
						fprintf (stderr, "The nursery size must be at least %d bytes.\n", SGEN_MAX_NURSERY_WASTE);
						exit (1);
					}

					default_nursery_bits = 0;
					while (1 << (++ default_nursery_bits) != default_nursery_size)
						;
#endif
				} else {
					fprintf (stderr, "nursery-size must be an integer.\n");
					exit (1);
				}
				continue;
			}
#endif
			if (!(major_collector.handle_gc_param && major_collector.handle_gc_param (opt))) {
				fprintf (stderr, "MONO_GC_PARAMS must be a comma-delimited list of one or more of the following:\n");
				fprintf (stderr, "  max-heap-size=N (where N is an integer, possibly with a k, m or a g suffix)\n");
				fprintf (stderr, "  soft-heap-limit=n (where N is an integer, possibly with a k, m or a g suffix)\n");
				fprintf (stderr, "  nursery-size=N (where N is an integer, possibly with a k, m or a g suffix)\n");
				fprintf (stderr, "  major=COLLECTOR (where COLLECTOR is `marksweep', `marksweep-par' or `copying')\n");
				fprintf (stderr, "  wbarrier=WBARRIER (where WBARRIER is `remset' or `cardtable')\n");
				fprintf (stderr, "  stack-mark=MARK-METHOD (where MARK-METHOD is 'precise' or 'conservative')\n");
				if (major_collector.print_gc_param_usage)
					major_collector.print_gc_param_usage ();
				exit (1);
			}
		}
		g_strfreev (opts);
	}

	if (major_collector.is_parallel)
		workers_init (num_workers);

	if (major_collector_opt)
		g_free (major_collector_opt);

	nursery_size = DEFAULT_NURSERY_SIZE;
	minor_collection_allowance = MIN_MINOR_COLLECTION_ALLOWANCE;
	init_heap_size_limits (max_heap, soft_limit);

	alloc_nursery ();

	if ((env = getenv ("MONO_GC_DEBUG"))) {
		opts = g_strsplit (env, ",", -1);
		for (ptr = opts; ptr && *ptr; ptr ++) {
			char *opt = *ptr;
			if (opt [0] >= '0' && opt [0] <= '9') {
				gc_debug_level = atoi (opt);
				opt++;
				if (opt [0] == ':')
					opt++;
				if (opt [0]) {
					char *rf = g_strdup_printf ("%s.%d", opt, getpid ());
					gc_debug_file = fopen (rf, "wb");
					if (!gc_debug_file)
						gc_debug_file = stderr;
					g_free (rf);
				}
			} else if (!strcmp (opt, "print-allowance")) {
				debug_print_allowance = TRUE;
			} else if (!strcmp (opt, "print-pinning")) {
				do_pin_stats = TRUE;
			} else if (!strcmp (opt, "collect-before-allocs")) {
				collect_before_allocs = 1;
			} else if (g_str_has_prefix (opt, "collect-before-allocs=")) {
				char *arg = strchr (opt, '=') + 1;
				collect_before_allocs = atoi (arg);
			} else if (!strcmp (opt, "check-at-minor-collections")) {
				consistency_check_at_minor_collection = TRUE;
				nursery_clear_policy = CLEAR_AT_GC;
			} else if (!strcmp (opt, "xdomain-checks")) {
				xdomain_checks = TRUE;
			} else if (!strcmp (opt, "clear-at-gc")) {
				nursery_clear_policy = CLEAR_AT_GC;
			} else if (!strcmp (opt, "clear-nursery-at-gc")) {
				nursery_clear_policy = CLEAR_AT_GC;
			} else if (!strcmp (opt, "check-scan-starts")) {
				do_scan_starts_check = TRUE;
			} else if (!strcmp (opt, "verify-nursery-at-minor-gc")) {
				do_verify_nursery = TRUE;
			} else if (!strcmp (opt, "dump-nursery-at-minor-gc")) {
				do_dump_nursery_content = TRUE;
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
#ifdef SGEN_BINARY_PROTOCOL
			} else if (g_str_has_prefix (opt, "binary-protocol=")) {
				char *filename = strchr (opt, '=') + 1;
				binary_protocol_init (filename);
				if (use_cardtable)
					fprintf (stderr, "Warning: Cardtable write barriers will not be binary-protocolled.\n");
#endif
			} else {
				fprintf (stderr, "Invalid format for the MONO_GC_DEBUG env variable: '%s'\n", env);
				fprintf (stderr, "The format is: MONO_GC_DEBUG=[l[:filename]|<option>]+ where l is a debug level 0-9.\n");
				fprintf (stderr, "Valid options are:\n");
				fprintf (stderr, "  collect-before-allocs[=<n>]\n");
				fprintf (stderr, "  check-at-minor-collections\n");
				fprintf (stderr, "  disable-minor\n");
				fprintf (stderr, "  disable-major\n");
				fprintf (stderr, "  xdomain-checks\n");
				fprintf (stderr, "  clear-at-gc\n");
				fprintf (stderr, "  print-allowance\n");
				fprintf (stderr, "  print-pinning\n");
				exit (1);
			}
		}
		g_strfreev (opts);
	}

	if (major_collector.is_parallel) {
		if (heap_dump_file) {
			fprintf (stderr, "Error: Cannot do heap dump with the parallel collector.\n");
			exit (1);
		}
		if (do_pin_stats) {
			fprintf (stderr, "Error: Cannot gather pinning statistics with the parallel collector.\n");
			exit (1);
		}
	}

	if (major_collector.post_param_init)
		major_collector.post_param_init ();

	global_remset = alloc_remset (1024, NULL, FALSE);
	global_remset->next = NULL;

	if (use_cardtable)
		card_table_init ();

	gc_initialized = 1;
}

enum {
	ATYPE_NORMAL,
	ATYPE_VECTOR,
	ATYPE_SMALL,
	ATYPE_NUM
};

#ifdef HAVE_KW_THREAD
#define EMIT_TLS_ACCESS(mb,dummy,offset)	do {	\
	mono_mb_emit_byte ((mb), MONO_CUSTOM_PREFIX);	\
	mono_mb_emit_byte ((mb), CEE_MONO_TLS);		\
	mono_mb_emit_i4 ((mb), (offset));		\
	} while (0)
#else

/* 
 * CEE_MONO_TLS requires the tls offset, not the key, so the code below only works on darwin,
 * where the two are the same.
 */
#if defined(__APPLE__) || defined (HOST_WIN32)
#define EMIT_TLS_ACCESS(mb,member,dummy)	do {	\
	mono_mb_emit_byte ((mb), MONO_CUSTOM_PREFIX);	\
	mono_mb_emit_byte ((mb), CEE_MONO_TLS);		\
	mono_mb_emit_i4 ((mb), thread_info_key);	\
	mono_mb_emit_icon ((mb), G_STRUCT_OFFSET (SgenThreadInfo, member));	\
	mono_mb_emit_byte ((mb), CEE_ADD);		\
	mono_mb_emit_byte ((mb), CEE_LDIND_I);		\
	} while (0)
#else
#define EMIT_TLS_ACCESS(mb,member,dummy)	do { g_error ("sgen is not supported when using --with-tls=pthread.\n"); } while (0)
#endif

#endif

#ifdef MANAGED_ALLOCATION
/* FIXME: Do this in the JIT, where specialized allocation sequences can be created
 * for each class. This is currently not easy to do, as it is hard to generate basic 
 * blocks + branches, but it is easy with the linear IL codebase.
 *
 * For this to work we'd need to solve the TLAB race, first.  Now we
 * require the allocator to be in a few known methods to make sure
 * that they are executed atomically via the restart mechanism.
 */
static MonoMethod*
create_allocator (int atype)
{
	int p_var, size_var;
	guint32 slowpath_branch, max_size_branch;
	MonoMethodBuilder *mb;
	MonoMethod *res;
	MonoMethodSignature *csig;
	static gboolean registered = FALSE;
	int tlab_next_addr_var, new_next_var;
	int num_params, i;
	const char *name = NULL;
	AllocatorWrapperInfo *info;

#ifdef HAVE_KW_THREAD
	int tlab_next_addr_offset = -1;
	int tlab_temp_end_offset = -1;

	MONO_THREAD_VAR_OFFSET (tlab_next_addr, tlab_next_addr_offset);
	MONO_THREAD_VAR_OFFSET (tlab_temp_end, tlab_temp_end_offset);

	g_assert (tlab_next_addr_offset != -1);
	g_assert (tlab_temp_end_offset != -1);
#endif

	if (!registered) {
		mono_register_jit_icall (mono_gc_alloc_obj, "mono_gc_alloc_obj", mono_create_icall_signature ("object ptr int"), FALSE);
		mono_register_jit_icall (mono_gc_alloc_vector, "mono_gc_alloc_vector", mono_create_icall_signature ("object ptr int int"), FALSE);
		registered = TRUE;
	}

	if (atype == ATYPE_SMALL) {
		num_params = 1;
		name = "AllocSmall";
	} else if (atype == ATYPE_NORMAL) {
		num_params = 1;
		name = "Alloc";
	} else if (atype == ATYPE_VECTOR) {
		num_params = 2;
		name = "AllocVector";
	} else {
		g_assert_not_reached ();
	}

	csig = mono_metadata_signature_alloc (mono_defaults.corlib, num_params);
	csig->ret = &mono_defaults.object_class->byval_arg;
	for (i = 0; i < num_params; ++i)
		csig->params [i] = &mono_defaults.int_class->byval_arg;

	mb = mono_mb_new (mono_defaults.object_class, name, MONO_WRAPPER_ALLOC);
	size_var = mono_mb_add_local (mb, &mono_defaults.int32_class->byval_arg);
	if (atype == ATYPE_NORMAL || atype == ATYPE_SMALL) {
		/* size = vtable->klass->instance_size; */
		mono_mb_emit_ldarg (mb, 0);
		mono_mb_emit_icon (mb, G_STRUCT_OFFSET (MonoVTable, klass));
		mono_mb_emit_byte (mb, CEE_ADD);
		mono_mb_emit_byte (mb, CEE_LDIND_I);
		mono_mb_emit_icon (mb, G_STRUCT_OFFSET (MonoClass, instance_size));
		mono_mb_emit_byte (mb, CEE_ADD);
		/* FIXME: assert instance_size stays a 4 byte integer */
		mono_mb_emit_byte (mb, CEE_LDIND_U4);
		mono_mb_emit_stloc (mb, size_var);
	} else if (atype == ATYPE_VECTOR) {
		MonoExceptionClause *clause;
		int pos, pos_leave;
		MonoClass *oom_exc_class;
		MonoMethod *ctor;

		/* n > 	MONO_ARRAY_MAX_INDEX -> OverflowException */
		mono_mb_emit_ldarg (mb, 1);
		mono_mb_emit_icon (mb, MONO_ARRAY_MAX_INDEX);
		pos = mono_mb_emit_short_branch (mb, CEE_BLE_UN_S);
		mono_mb_emit_exception (mb, "OverflowException", NULL);
		mono_mb_patch_short_branch (mb, pos);

		clause = mono_image_alloc0 (mono_defaults.corlib, sizeof (MonoExceptionClause));
		clause->try_offset = mono_mb_get_label (mb);

		/* vtable->klass->sizes.element_size */
		mono_mb_emit_ldarg (mb, 0);
		mono_mb_emit_icon (mb, G_STRUCT_OFFSET (MonoVTable, klass));
		mono_mb_emit_byte (mb, CEE_ADD);
		mono_mb_emit_byte (mb, CEE_LDIND_I);
		mono_mb_emit_icon (mb, G_STRUCT_OFFSET (MonoClass, sizes.element_size));
		mono_mb_emit_byte (mb, CEE_ADD);
		mono_mb_emit_byte (mb, CEE_LDIND_U4);

		/* * n */
		mono_mb_emit_ldarg (mb, 1);
		mono_mb_emit_byte (mb, CEE_MUL_OVF_UN);
		/* + sizeof (MonoArray) */
		mono_mb_emit_icon (mb, sizeof (MonoArray));
		mono_mb_emit_byte (mb, CEE_ADD_OVF_UN);
		mono_mb_emit_stloc (mb, size_var);

		pos_leave = mono_mb_emit_branch (mb, CEE_LEAVE);

		/* catch */
		clause->flags = MONO_EXCEPTION_CLAUSE_NONE;
		clause->try_len = mono_mb_get_pos (mb) - clause->try_offset;
		clause->data.catch_class = mono_class_from_name (mono_defaults.corlib,
				"System", "OverflowException");
		g_assert (clause->data.catch_class);
		clause->handler_offset = mono_mb_get_label (mb);

		oom_exc_class = mono_class_from_name (mono_defaults.corlib,
				"System", "OutOfMemoryException");
		g_assert (oom_exc_class);
		ctor = mono_class_get_method_from_name (oom_exc_class, ".ctor", 0);
		g_assert (ctor);

		mono_mb_emit_byte (mb, CEE_POP);
		mono_mb_emit_op (mb, CEE_NEWOBJ, ctor);
		mono_mb_emit_byte (mb, CEE_THROW);

		clause->handler_len = mono_mb_get_pos (mb) - clause->handler_offset;
		mono_mb_set_clauses (mb, 1, clause);
		mono_mb_patch_branch (mb, pos_leave);
		/* end catch */
	} else {
		g_assert_not_reached ();
	}

	/* size += ALLOC_ALIGN - 1; */
	mono_mb_emit_ldloc (mb, size_var);
	mono_mb_emit_icon (mb, ALLOC_ALIGN - 1);
	mono_mb_emit_byte (mb, CEE_ADD);
	/* size &= ~(ALLOC_ALIGN - 1); */
	mono_mb_emit_icon (mb, ~(ALLOC_ALIGN - 1));
	mono_mb_emit_byte (mb, CEE_AND);
	mono_mb_emit_stloc (mb, size_var);

	/* if (size > MAX_SMALL_OBJ_SIZE) goto slowpath */
	if (atype != ATYPE_SMALL) {
		mono_mb_emit_ldloc (mb, size_var);
		mono_mb_emit_icon (mb, MAX_SMALL_OBJ_SIZE);
		max_size_branch = mono_mb_emit_short_branch (mb, MONO_CEE_BGT_S);
	}

	/*
	 * We need to modify tlab_next, but the JIT only supports reading, so we read
	 * another tls var holding its address instead.
	 */

	/* tlab_next_addr (local) = tlab_next_addr (TLS var) */
	tlab_next_addr_var = mono_mb_add_local (mb, &mono_defaults.int_class->byval_arg);
	EMIT_TLS_ACCESS (mb, tlab_next_addr, tlab_next_addr_offset);
	mono_mb_emit_stloc (mb, tlab_next_addr_var);

	/* p = (void**)tlab_next; */
	p_var = mono_mb_add_local (mb, &mono_defaults.int_class->byval_arg);
	mono_mb_emit_ldloc (mb, tlab_next_addr_var);
	mono_mb_emit_byte (mb, CEE_LDIND_I);
	mono_mb_emit_stloc (mb, p_var);
	
	/* new_next = (char*)p + size; */
	new_next_var = mono_mb_add_local (mb, &mono_defaults.int_class->byval_arg);
	mono_mb_emit_ldloc (mb, p_var);
	mono_mb_emit_ldloc (mb, size_var);
	mono_mb_emit_byte (mb, CEE_CONV_I);
	mono_mb_emit_byte (mb, CEE_ADD);
	mono_mb_emit_stloc (mb, new_next_var);

	/* if (G_LIKELY (new_next < tlab_temp_end)) */
	mono_mb_emit_ldloc (mb, new_next_var);
	EMIT_TLS_ACCESS (mb, tlab_temp_end, tlab_temp_end_offset);
	slowpath_branch = mono_mb_emit_short_branch (mb, MONO_CEE_BLT_UN_S);

	/* Slowpath */
	if (atype != ATYPE_SMALL)
		mono_mb_patch_short_branch (mb, max_size_branch);

	mono_mb_emit_byte (mb, MONO_CUSTOM_PREFIX);
	mono_mb_emit_byte (mb, CEE_MONO_NOT_TAKEN);

	/* FIXME: mono_gc_alloc_obj takes a 'size_t' as an argument, not an int32 */
	mono_mb_emit_ldarg (mb, 0);
	mono_mb_emit_ldloc (mb, size_var);
	if (atype == ATYPE_NORMAL || atype == ATYPE_SMALL) {
		mono_mb_emit_icall (mb, mono_gc_alloc_obj);
	} else if (atype == ATYPE_VECTOR) {
		mono_mb_emit_ldarg (mb, 1);
		mono_mb_emit_icall (mb, mono_gc_alloc_vector);
	} else {
		g_assert_not_reached ();
	}
	mono_mb_emit_byte (mb, CEE_RET);

	/* Fastpath */
	mono_mb_patch_short_branch (mb, slowpath_branch);

	/* FIXME: Memory barrier */

	/* tlab_next = new_next */
	mono_mb_emit_ldloc (mb, tlab_next_addr_var);
	mono_mb_emit_ldloc (mb, new_next_var);
	mono_mb_emit_byte (mb, CEE_STIND_I);

	/*The tlab store must be visible before the the vtable store. This could be replaced with a DDS but doing it with IL would be tricky. */
	mono_mb_emit_byte ((mb), MONO_CUSTOM_PREFIX);
	mono_mb_emit_op (mb, CEE_MONO_MEMORY_BARRIER, StoreStoreBarrier);

	/* *p = vtable; */
	mono_mb_emit_ldloc (mb, p_var);
	mono_mb_emit_ldarg (mb, 0);
	mono_mb_emit_byte (mb, CEE_STIND_I);

	if (atype == ATYPE_VECTOR) {
		/* arr->max_length = max_length; */
		mono_mb_emit_ldloc (mb, p_var);
		mono_mb_emit_ldflda (mb, G_STRUCT_OFFSET (MonoArray, max_length));
		mono_mb_emit_ldarg (mb, 1);
		mono_mb_emit_byte (mb, CEE_STIND_I);
	}

	/*
	We must make sure both vtable and max_length are globaly visible before returning to managed land.
	*/
	mono_mb_emit_byte ((mb), MONO_CUSTOM_PREFIX);
	mono_mb_emit_op (mb, CEE_MONO_MEMORY_BARRIER, StoreStoreBarrier);

	/* return p */
	mono_mb_emit_ldloc (mb, p_var);
	mono_mb_emit_byte (mb, CEE_RET);

	res = mono_mb_create_method (mb, csig, 8);
	mono_mb_free (mb);
	mono_method_get_header (res)->init_locals = FALSE;

	info = mono_image_alloc0 (mono_defaults.corlib, sizeof (AllocatorWrapperInfo));
	info->gc_name = "sgen";
	info->alloc_type = atype;
	mono_marshal_set_wrapper_info (res, info);

	return res;
}
#endif

const char *
mono_gc_get_gc_name (void)
{
	return "sgen";
}

static MonoMethod* alloc_method_cache [ATYPE_NUM];
static MonoMethod *write_barrier_method;

static gboolean
mono_gc_is_critical_method (MonoMethod *method)
{
	int i;
	if (method == write_barrier_method)
		return TRUE;

	for (i = 0; i < ATYPE_NUM; ++i)
		if (method == alloc_method_cache [i])
			return TRUE;

	return FALSE;
}

static gboolean
is_ip_in_managed_allocator (MonoDomain *domain, gpointer ip)
{
	MonoJitInfo *ji;

	if (!mono_thread_internal_current ())
		/* Happens during thread attach */
		return FALSE;

	if (!ip || !domain)
		return FALSE;
	ji = mono_jit_info_table_find (domain, ip);
	if (!ji)
		return FALSE;

	return mono_gc_is_critical_method (ji->method);
}

/*
 * Generate an allocator method implementing the fast path of mono_gc_alloc_obj ().
 * The signature of the called method is:
 * 	object allocate (MonoVTable *vtable)
 */
MonoMethod*
mono_gc_get_managed_allocator (MonoVTable *vtable, gboolean for_box)
{
#ifdef MANAGED_ALLOCATION
	MonoClass *klass = vtable->klass;

#ifdef HAVE_KW_THREAD
	int tlab_next_offset = -1;
	int tlab_temp_end_offset = -1;
	MONO_THREAD_VAR_OFFSET (tlab_next, tlab_next_offset);
	MONO_THREAD_VAR_OFFSET (tlab_temp_end, tlab_temp_end_offset);

	if (tlab_next_offset == -1 || tlab_temp_end_offset == -1)
		return NULL;
#endif

	if (!mono_runtime_has_tls_get ())
		return NULL;
	if (klass->instance_size > tlab_size)
		return NULL;
	if (klass->has_finalize || klass->marshalbyref || (mono_profiler_get_events () & MONO_PROFILE_ALLOCATIONS))
		return NULL;
	if (klass->rank)
		return NULL;
	if (klass->byval_arg.type == MONO_TYPE_STRING)
		return NULL;
	if (collect_before_allocs)
		return NULL;

	if (ALIGN_TO (klass->instance_size, ALLOC_ALIGN) < MAX_SMALL_OBJ_SIZE)
		return mono_gc_get_managed_allocator_by_type (ATYPE_SMALL);
	else
		return mono_gc_get_managed_allocator_by_type (ATYPE_NORMAL);
#else
	return NULL;
#endif
}

MonoMethod*
mono_gc_get_managed_array_allocator (MonoVTable *vtable, int rank)
{
#ifdef MANAGED_ALLOCATION
	MonoClass *klass = vtable->klass;

#ifdef HAVE_KW_THREAD
	int tlab_next_offset = -1;
	int tlab_temp_end_offset = -1;
	MONO_THREAD_VAR_OFFSET (tlab_next, tlab_next_offset);
	MONO_THREAD_VAR_OFFSET (tlab_temp_end, tlab_temp_end_offset);

	if (tlab_next_offset == -1 || tlab_temp_end_offset == -1)
		return NULL;
#endif

	if (rank != 1)
		return NULL;
	if (!mono_runtime_has_tls_get ())
		return NULL;
	if (mono_profiler_get_events () & MONO_PROFILE_ALLOCATIONS)
		return NULL;
	if (collect_before_allocs)
		return NULL;
	g_assert (!mono_class_has_finalizer (klass) && !klass->marshalbyref);

	return mono_gc_get_managed_allocator_by_type (ATYPE_VECTOR);
#else
	return NULL;
#endif
}

MonoMethod*
mono_gc_get_managed_allocator_by_type (int atype)
{
#ifdef MANAGED_ALLOCATION
	MonoMethod *res;

	if (!mono_runtime_has_tls_get ())
		return NULL;

	mono_loader_lock ();
	res = alloc_method_cache [atype];
	if (!res)
		res = alloc_method_cache [atype] = create_allocator (atype);
	mono_loader_unlock ();
	return res;
#else
	return NULL;
#endif
}

guint32
mono_gc_get_managed_allocator_types (void)
{
	return ATYPE_NUM;
}

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
	mono_mb_emit_icon (mb, (mword)nursery_start >> DEFAULT_NURSERY_BITS);
	nursery_check_return_labels [0] = mono_mb_emit_branch (mb, CEE_BEQ);

	// if (!ptr_in_nursery (*ptr)) return;
	mono_mb_emit_ldarg (mb, 0);
	mono_mb_emit_byte (mb, CEE_LDIND_I);
	mono_mb_emit_icon (mb, DEFAULT_NURSERY_BITS);
	mono_mb_emit_byte (mb, CEE_SHR_UN);
	mono_mb_emit_icon (mb, (mword)nursery_start >> DEFAULT_NURSERY_BITS);
	nursery_check_return_labels [1] = mono_mb_emit_branch (mb, CEE_BNE_UN);
#else
	int label_continue1, label_continue2;
	int dereferenced_var;

	// if (ptr < (nursery_start)) goto continue;
	mono_mb_emit_ldarg (mb, 0);
	mono_mb_emit_ptr (mb, (gpointer) nursery_start);
	label_continue_1 = mono_mb_emit_branch (mb, CEE_BLT);

	// if (ptr >= nursery_end)) goto continue;
	mono_mb_emit_ldarg (mb, 0);
	mono_mb_emit_ptr (mb, (gpointer) nursery_end);
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

	// if (*ptr < nursery_start) return;
	mono_mb_emit_ldloc (mb, dereferenced_var);
	mono_mb_emit_ptr (mb, (gpointer) nursery_start);
	nursery_check_return_labels [1] = mono_mb_emit_branch (mb, CEE_BLT);

	// if (*ptr >= nursery_end) return;
	mono_mb_emit_ldloc (mb, dereferenced_var);
	mono_mb_emit_ptr (mb, (gpointer) nursery_end);
	nursery_check_return_labels [2] = mono_mb_emit_branch (mb, CEE_BGE);
#endif	
}

MonoMethod*
mono_gc_get_write_barrier (void)
{
	MonoMethod *res;
	MonoMethodBuilder *mb;
	MonoMethodSignature *sig;
#ifdef MANAGED_WBARRIER
	int i, nursery_check_labels [3];
	int label_no_wb_3, label_no_wb_4, label_need_wb, label_slow_path;
	int buffer_var, buffer_index_var, dummy_var;

#ifdef HAVE_KW_THREAD
	int stack_end_offset = -1, store_remset_buffer_offset = -1;
	int store_remset_buffer_index_offset = -1, store_remset_buffer_index_addr_offset = -1;

	MONO_THREAD_VAR_OFFSET (stack_end, stack_end_offset);
	g_assert (stack_end_offset != -1);
	MONO_THREAD_VAR_OFFSET (store_remset_buffer, store_remset_buffer_offset);
	g_assert (store_remset_buffer_offset != -1);
	MONO_THREAD_VAR_OFFSET (store_remset_buffer_index, store_remset_buffer_index_offset);
	g_assert (store_remset_buffer_index_offset != -1);
	MONO_THREAD_VAR_OFFSET (store_remset_buffer_index_addr, store_remset_buffer_index_addr_offset);
	g_assert (store_remset_buffer_index_addr_offset != -1);
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

#ifdef MANAGED_WBARRIER
	if (use_cardtable) {
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
	} else if (mono_runtime_has_tls_get ()) {
		emit_nursery_check (mb, nursery_check_labels);

		// if (ptr >= stack_end) goto need_wb;
		mono_mb_emit_ldarg (mb, 0);
		EMIT_TLS_ACCESS (mb, stack_end, stack_end_offset);
		label_need_wb = mono_mb_emit_branch (mb, CEE_BGE_UN);

		// if (ptr >= stack_start) return;
		dummy_var = mono_mb_add_local (mb, &mono_defaults.int_class->byval_arg);
		mono_mb_emit_ldarg (mb, 0);
		mono_mb_emit_ldloc_addr (mb, dummy_var);
		label_no_wb_3 = mono_mb_emit_branch (mb, CEE_BGE_UN);

		// need_wb:
		mono_mb_patch_branch (mb, label_need_wb);

		// buffer = STORE_REMSET_BUFFER;
		buffer_var = mono_mb_add_local (mb, &mono_defaults.int_class->byval_arg);
		EMIT_TLS_ACCESS (mb, store_remset_buffer, store_remset_buffer_offset);
		mono_mb_emit_stloc (mb, buffer_var);

		// buffer_index = STORE_REMSET_BUFFER_INDEX;
		buffer_index_var = mono_mb_add_local (mb, &mono_defaults.int_class->byval_arg);
		EMIT_TLS_ACCESS (mb, store_remset_buffer_index, store_remset_buffer_index_offset);
		mono_mb_emit_stloc (mb, buffer_index_var);

		// if (buffer [buffer_index] == ptr) return;
		mono_mb_emit_ldloc (mb, buffer_var);
		mono_mb_emit_ldloc (mb, buffer_index_var);
		g_assert (sizeof (gpointer) == 4 || sizeof (gpointer) == 8);
		mono_mb_emit_icon (mb, sizeof (gpointer) == 4 ? 2 : 3);
		mono_mb_emit_byte (mb, CEE_SHL);
		mono_mb_emit_byte (mb, CEE_ADD);
		mono_mb_emit_byte (mb, CEE_LDIND_I);
		mono_mb_emit_ldarg (mb, 0);
		label_no_wb_4 = mono_mb_emit_branch (mb, CEE_BEQ);

		// ++buffer_index;
		mono_mb_emit_ldloc (mb, buffer_index_var);
		mono_mb_emit_icon (mb, 1);
		mono_mb_emit_byte (mb, CEE_ADD);
		mono_mb_emit_stloc (mb, buffer_index_var);

		// if (buffer_index >= STORE_REMSET_BUFFER_SIZE) goto slow_path;
		mono_mb_emit_ldloc (mb, buffer_index_var);
		mono_mb_emit_icon (mb, STORE_REMSET_BUFFER_SIZE);
		label_slow_path = mono_mb_emit_branch (mb, CEE_BGE);

		// buffer [buffer_index] = ptr;
		mono_mb_emit_ldloc (mb, buffer_var);
		mono_mb_emit_ldloc (mb, buffer_index_var);
		g_assert (sizeof (gpointer) == 4 || sizeof (gpointer) == 8);
		mono_mb_emit_icon (mb, sizeof (gpointer) == 4 ? 2 : 3);
		mono_mb_emit_byte (mb, CEE_SHL);
		mono_mb_emit_byte (mb, CEE_ADD);
		mono_mb_emit_ldarg (mb, 0);
		mono_mb_emit_byte (mb, CEE_STIND_I);

		// STORE_REMSET_BUFFER_INDEX = buffer_index;
		EMIT_TLS_ACCESS (mb, store_remset_buffer_index_addr, store_remset_buffer_index_addr_offset);
		mono_mb_emit_ldloc (mb, buffer_index_var);
		mono_mb_emit_byte (mb, CEE_STIND_I);

		// return;
		for (i = 0; i < 3; ++i) {
			if (nursery_check_labels [i])
				mono_mb_patch_branch (mb, nursery_check_labels [i]);
		}
		mono_mb_patch_branch (mb, label_no_wb_3);
		mono_mb_patch_branch (mb, label_no_wb_4);
		mono_mb_emit_byte (mb, CEE_RET);

		// slow path
		mono_mb_patch_branch (mb, label_slow_path);

		mono_mb_emit_ldarg (mb, 0);
		mono_mb_emit_icall (mb, mono_gc_wbarrier_generic_nostore);
		mono_mb_emit_byte (mb, CEE_RET);
	} else
#endif
	{
		mono_mb_emit_ldarg (mb, 0);
		mono_mb_emit_icall (mb, mono_gc_wbarrier_generic_nostore);
		mono_mb_emit_byte (mb, CEE_RET);
	}

	res = mono_mb_create_method (mb, sig, 16);
	mono_mb_free (mb);

	mono_loader_lock ();
	if (write_barrier_method) {
		/* Already created */
		mono_free_method (res);
	} else {
		/* double-checked locking */
		mono_memory_barrier ();
		write_barrier_method = res;
	}
	mono_loader_unlock ();

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

void
mono_sgen_debug_printf (int level, const char *format, ...)
{
	va_list ap;

	if (level > gc_debug_level)
		return;

	va_start (ap, format);
	vfprintf (gc_debug_file, format, ap);
	va_end (ap);
}

FILE*
mono_sgen_get_logfile (void)
{
	return gc_debug_file;
}

#ifdef HOST_WIN32
BOOL APIENTRY mono_gc_dllmain (HMODULE module_handle, DWORD reason, LPVOID reserved)
{
	return TRUE;
}
#endif

NurseryClearPolicy
mono_sgen_get_nursery_clear_policy (void)
{
	return nursery_clear_policy;
}

MonoVTable*
mono_sgen_get_array_fill_vtable (void)
{
	if (!array_fill_vtable) {
		static MonoClass klass;
		static MonoVTable vtable;

		MonoDomain *domain = mono_get_root_domain ();
		g_assert (domain);

		klass.element_class = mono_defaults.byte_class;
		klass.rank = 1;
		klass.instance_size = sizeof (MonoArray);
		klass.sizes.element_size = 1;

		vtable.klass = &klass;
		vtable.gc_descr = NULL;
		vtable.rank = 1;

		array_fill_vtable = &vtable;
	}
	return array_fill_vtable;
}

void
mono_sgen_gc_lock (void)
{
	LOCK_GC;
}

void
mono_sgen_gc_unlock (void)
{
	UNLOCK_GC;
}

#endif /* HAVE_SGEN_GC */
