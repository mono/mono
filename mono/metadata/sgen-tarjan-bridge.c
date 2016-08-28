/*
 * sgen-tarjan-bridge.c: Tarjan-based bridge implementation.
 *
 * Copyright 2011 Novell, Inc (http://www.novell.com)
 * Copyright 2014 Xamarin Inc (http://www.xamarin.com)
 *
 *
 * Copyright 2001-2003 Ximian, Inc
 * Copyright 2003-2010 Novell, Inc.
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include "config.h"

#ifdef HAVE_SGEN_GC

#include <stdlib.h>

#include "sgen/sgen-gc.h"
#include "sgen-bridge-internals.h"
#include "sgen/sgen-hash-table.h"
#include "sgen/sgen-qsort.h"
#include "tabledefs.h"
#include "utils/mono-logger-internals.h"

#define OPTIMIZATION_FORWARD
#define OPTIMIZATION_SINGLETON_DYN_ARRAY
#include "sgen-dynarray.h"

/*
 * See comments in sgen-bridge.h
 *
 * This bridge implementation is based on the tarjan algorithm for strongly
 * connected components. It has two elements:
 *
 *   - Standard tarjan SCC algorithm to convert graph to SCC forest
 *
 *   - "Colors": We reduce the SCC forest to bridged-SCCs-only by using a
 *     "color" algorithm devised by Kumpera. Consider the set of bridged SCCs
 *     which is reachable from a given object. We call each such unique set a
 *     "color". We compute the set of colors and which colors contain links to
 *     which colors. The color graph then becomes the reduced SCC graph.
 */

static void
enable_accounting (void)
{
	// bridge_accounting_enabled = TRUE;
	// hash_table = (SgenHashTable)SGEN_HASH_TABLE_INIT (INTERNAL_MEM_BRIDGE_HASH_TABLE, INTERNAL_MEM_BRIDGE_HASH_TABLE_ENTRY, sizeof (HashEntryWithAccounting), mono_aligned_addr_hash, NULL);
}

// Is this class bridged or not, and should its dependencies be scanned or not?
// The result of this callback will be cached for use by is_opaque_object later.
static MonoGCBridgeObjectKind
class_kind (MonoClass *klass)
{
	MonoGCBridgeObjectKind res = bridge_callbacks.bridge_class_kind (klass);

	/* If it's a bridge, nothing we can do about it. */
	if (res == GC_BRIDGE_TRANSPARENT_BRIDGE_CLASS || res == GC_BRIDGE_OPAQUE_BRIDGE_CLASS)
		return res;

	/* Non bridge classes with no pointers will never point to a bridge, so we can savely ignore them. */
	if (!klass->has_references) {
		SGEN_LOG (6, "class %s is opaque\n", klass->name);
		return GC_BRIDGE_OPAQUE_CLASS;
	}

	/* Some arrays can be ignored */
	if (klass->rank == 1) {
		MonoClass *elem_class = klass->element_class;

		/* FIXME the bridge check can be quite expensive, cache it at the class level. */
		/* An array of a sealed type that is not a bridge will never get to a bridge */
		if ((elem_class->flags & TYPE_ATTRIBUTE_SEALED) && !elem_class->has_references && !bridge_callbacks.bridge_class_kind (elem_class)) {
			SGEN_LOG (6, "class %s is opaque\n", klass->name);
			return GC_BRIDGE_OPAQUE_CLASS;
		}
	}

	return GC_BRIDGE_TRANSPARENT_CLASS;
}

//enable usage logging
// #define DUMP_GRAPH 1

// ScanData state
enum {
	INITIAL,
	SCANNED,
	FINISHED_ON_STACK,
	FINISHED_OFF_STACK
};

/*
Optimizations:
	We can split this data structure in two, those with bridges and those without
*/
typedef struct {
    // Colors (ColorDatas) linked to by objects with this color
	DynPtrArray other_colors;
    // Bridge objects (GCObjects) held by objects with this color
	DynPtrArray bridges;
	int api_index    : 31;
	unsigned visited : 1;
} ColorData;

// Represents one managed object. Equivalent of new/old bridge "HashEntry"
typedef struct _ScanData {
	// FIXME this can be eliminated; if we have a ScanData we generally looked it up from its GCObject
	GCObject *obj;
	// We use the sgen lock_word in GCObject to store a pointer to the ScanData. Cache the original here to restore later:
	mword lock_word;

	ColorData *color;
	// Tarjan algorithm index (order visited)
	int index;
	// Tarjan index of lowest-index object known reachable from here
	int low_index : 27;

	// See "ScanData state" enum above
	unsigned state : 2;
	unsigned is_bridge : 1;
	// Similar to lock_word, we use these bits in the GCObject as scratch space then restore them when done
	unsigned obj_state : 2;

#ifdef OPTIMIZATION_FORWARD
	struct _ScanData *forwarded_to;
#endif
} ScanData;

// follow_forward needs to be called on anything that comes out of scan_stack or is
// recovered from an object reference. it does not need to be called on anything in
// state INITIAL (because objects are marked SCANNED on forward) or in the color stack.
static ScanData*
follow_forward (ScanData *entry)
{
#ifdef OPTIMIZATION_FORWARD
	while (entry->forwarded_to) {
		ScanData *next = entry->forwarded_to;
		if (next->forwarded_to)
			entry->forwarded_to = next->forwarded_to;
		entry = next;
	}
#endif
	return entry;
}


// Stacks of ScanData objects used for tarjan algorithm.
// The Tarjan algorithm is normally defined recursively; here scan_stack simulates the call stack of a recursive algorithm,
// and loop_stack is the stack structure used by the algorithm itself.
static DynPtrArray scan_stack, loop_stack;

// GCObjects on which register_finalized_object has been called
static DynPtrArray registered_bridges;

// ColorData objects
static DynPtrArray color_merge_array;

static int ignored_objects;
static int object_index;
static int num_colors_with_bridges;
static int xref_count;

static size_t setup_time, tarjan_time, scc_setup_time, gather_xref_time, xref_setup_time, cleanup_time;
static SgenBridgeProcessor *bridge_processor;

#define BUCKET_SIZE 8184

//ScanData buckets
#define NUM_SCAN_ENTRIES ((BUCKET_SIZE - SIZEOF_VOID_P * 2) / sizeof (ScanData))

typedef struct _ObjectBucket ObjectBucket;
struct _ObjectBucket {
	ObjectBucket *next;
	ScanData *next_data;
	ScanData data [NUM_SCAN_ENTRIES];
};

static ObjectBucket *root_object_bucket;
static ObjectBucket *cur_object_bucket;
static int object_data_count;

static ObjectBucket*
new_object_bucket (void)
{
	ObjectBucket *res = (ObjectBucket *)sgen_alloc_internal (INTERNAL_MEM_TARJAN_OBJ_BUCKET);
	res->next_data = &res->data [0];
	return res;
}

static void
object_alloc_init (void)
{
	root_object_bucket = cur_object_bucket = new_object_bucket ();
}

static ScanData*
alloc_object_data (void)
{
	ScanData *res;
retry:

	/* next_data points to the first free entry */
	res = cur_object_bucket->next_data;
	if (res >= &cur_object_bucket->data [NUM_SCAN_ENTRIES]) {
		ObjectBucket *b = new_object_bucket ();
		cur_object_bucket->next = b;
		cur_object_bucket = b;
		goto retry;
	}
	cur_object_bucket->next_data = res + 1;
	object_data_count++;
	return res;
}

static void
free_object_buckets (void)
{
	ObjectBucket *cur = root_object_bucket;

	object_data_count = 0;

	while (cur) {
		ObjectBucket *tmp = cur->next;
		sgen_free_internal (cur, INTERNAL_MEM_TARJAN_OBJ_BUCKET);
		cur = tmp;
	}

	root_object_bucket = cur_object_bucket = NULL;
}

//ColorData buckets
#define NUM_COLOR_ENTRIES ((BUCKET_SIZE - SIZEOF_VOID_P * 2) / sizeof (ColorData))

// Same as ObjectBucket except NUM_COLOR_ENTRIES and NUM_SCAN_ENTRIES differ
typedef struct _ColorBucket ColorBucket;
struct _ColorBucket {
	ColorBucket *next;
	ColorData *next_data;
	ColorData data [NUM_COLOR_ENTRIES];
};

static ColorBucket *root_color_bucket;
static ColorBucket *cur_color_bucket;
static int color_data_count;


static ColorBucket*
new_color_bucket (void)
{
	ColorBucket *res = (ColorBucket *)sgen_alloc_internal (INTERNAL_MEM_TARJAN_OBJ_BUCKET);
	res->next_data = &res->data [0];
	return res;
}

static void
color_alloc_init (void)
{
	root_color_bucket = cur_color_bucket = new_color_bucket ();
}

static ColorData*
alloc_color_data (void)
{
	ColorData *res;
retry:

	/* next_data points to the first free entry */
	res = cur_color_bucket->next_data;
	if (res >= &cur_color_bucket->data [NUM_COLOR_ENTRIES]) {
		ColorBucket *b = new_color_bucket ();
		cur_color_bucket->next = b;
		cur_color_bucket = b;
		goto retry;
	}
	cur_color_bucket->next_data = res + 1;
	color_data_count++;
	return res;
}

static void
free_color_buckets (void)
{
	ColorBucket *cur, *tmp;

	color_data_count = 0;

	for (cur = root_color_bucket; cur; cur = tmp) {
		ColorData *cd;
		for (cd = &cur->data [0]; cd < cur->next_data; ++cd) {
			dyn_array_ptr_uninit (&cd->other_colors);
			dyn_array_ptr_uninit (&cd->bridges);
		}
		tmp = cur->next;
		sgen_free_internal (cur, INTERNAL_MEM_TARJAN_OBJ_BUCKET);
	}
	root_color_bucket = cur_color_bucket = NULL;
}


static ScanData*
create_data (GCObject *obj)
{
	mword *o = (mword*)obj;
	ScanData *res = alloc_object_data ();
	res->obj = obj;
	res->color = NULL;
	res->index = res->low_index = -1;
	res->state = INITIAL;
	res->is_bridge = FALSE;
	res->obj_state = o [0] & SGEN_VTABLE_BITS_MASK;
	res->lock_word = o [1];

	o [0] |= SGEN_VTABLE_BITS_MASK;
	o [1] = (mword)res;

#ifdef OPTIMIZATION_FORWARD
	res->forwarded_to = NULL;
#endif
	return res;
}

static ScanData*
find_data (GCObject *obj)
{
	ScanData *a = NULL;
	mword *o = (mword*)obj;
	if ((o [0] & SGEN_VTABLE_BITS_MASK) == SGEN_VTABLE_BITS_MASK)
		a = (ScanData*)o [1];
	return a;
}

static void
clear_after_processing (void)
{
	ObjectBucket *cur;

	for (cur = root_object_bucket; cur; cur = cur->next) {
		ScanData *sd;
		for (sd = &cur->data [0]; sd < cur->next_data; ++sd) {
			mword *o = (mword*)sd->obj;
			o [0] &= ~SGEN_VTABLE_BITS_MASK;
			o [0] |= sd->obj_state;
			o [1] = sd->lock_word;
		}
	}
}

static GCObject*
bridge_object_forward (GCObject *obj)
{
	GCObject *fwd;
	mword *o = (mword*)obj;
	if ((o [0] & SGEN_VTABLE_BITS_MASK) == SGEN_VTABLE_BITS_MASK)
		return obj;

	fwd = SGEN_OBJECT_IS_FORWARDED (obj);
	return fwd ? fwd : obj;
}

#ifdef DUMP_GRAPH
static const char*
safe_name_bridge (GCObject *obj)
{
	GCVTable vt = SGEN_LOAD_VTABLE (obj);
	return vt->klass->name;
}

static ScanData*
find_or_create_data (GCObject *obj)
{
	ScanData *entry = find_data (obj);
	if (!entry)
		entry = create_data (obj);
	return entry;
}
#endif

//----------
typedef struct {
	ColorData *color;
	int hash;
} HashEntry;

/*
We tried 2/32, 2/128, 4/32, 4/128, 6/128 and 8/128.

The performance cost between 4/128 and 8/128 is so small since cache movement happens completely in the same cacheline,
making the extra space pretty much free.

The cost between 32 and 128 itens is minimal too, it's mostly a fixed, setup cost.

Memory wise, 4/32 takes 512 and 8/128 takes 8k, so it's quite reasonable.
*/

#define ELEMENTS_PER_BUCKET 8
#define COLOR_CACHE_SIZE 128
static HashEntry merge_cache [COLOR_CACHE_SIZE][ELEMENTS_PER_BUCKET];

static int
mix_hash (size_t hash)
{
	return (int)(((hash * 215497) >> 16) ^ ((hash * 1823231) + hash));
}

static void
reset_cache (void)
{
	memset (merge_cache, 0, sizeof (merge_cache));
}


static gboolean
dyn_array_ptr_contains (DynPtrArray *da, void *x)
{
	int i;
	for (i = 0; i < dyn_array_ptr_size (da); ++i)
		if (dyn_array_ptr_get (da, i) == x)
			return TRUE;
	return FALSE;
}

static gboolean
match_colors (DynPtrArray *a, DynPtrArray *b)
{
	int i;
	if (dyn_array_ptr_size (a) != dyn_array_ptr_size (b))
		return FALSE;

	for (i = 0; i < dyn_array_ptr_size (a); ++i) {
		if (!dyn_array_ptr_contains (b, dyn_array_ptr_get (a, i)))
			return FALSE;
	}
	return TRUE;
}

static int cache_hits, cache_misses;

static ColorData*
find_in_cache (int *insert_index)
{
	HashEntry *bucket;
	int i, hash, size, index;

	size = dyn_array_ptr_size (&color_merge_array);
	/* Cache checking is very ineficient with a lot of elements*/
	if (size > 3)
		return NULL;

	hash = 0;
	for (i = 0 ; i < size; ++i)
		hash += mix_hash ((size_t)dyn_array_ptr_get (&color_merge_array, i));
	if (!hash)
		hash = 1;

	index = hash & (COLOR_CACHE_SIZE - 1);
	bucket = merge_cache [index];
	for (i = 0; i < ELEMENTS_PER_BUCKET; ++i) {
		if (bucket [i].hash != hash)
			continue;
		if (match_colors (&bucket [i].color->other_colors, &color_merge_array)) {
			++cache_hits;
			return bucket [i].color;
		}
	}

	//move elements to the back
	for (i = ELEMENTS_PER_BUCKET - 1; i > 0; --i)
		bucket [i] = bucket [i - 1];
	++cache_misses;
	*insert_index = index;
	bucket [0].hash = hash;
	return NULL;
}

static ColorData*
new_color (gboolean force_new)
{
	int i = -1;
	ColorData *cd;
	/* XXX Try to find an equal one and return it */
	if (!force_new) {
		cd = find_in_cache (&i);
		if (cd)
			return cd;
	}

	cd = alloc_color_data ();
	cd->api_index = -1;
	dyn_array_ptr_set_all (&cd->other_colors, &color_merge_array);
	/* if i >= 0, it means we prepared a given slot to receive the new color */
	if (i >= 0)
		merge_cache [i][0].color = cd;

	return cd;
}


static void
register_bridge_object (GCObject *obj)
{
	create_data (obj)->is_bridge = TRUE;
}

static gboolean
is_opaque_object (GCObject *obj)
{
	MonoVTable *vt = SGEN_LOAD_VTABLE (obj);
	if ((vt->gc_bits & SGEN_GC_BIT_BRIDGE_OPAQUE_OBJECT) == SGEN_GC_BIT_BRIDGE_OPAQUE_OBJECT) {
		SGEN_LOG (6, "ignoring %s\n", vt->klass->name);
		++ignored_objects;
		return TRUE;
	}
	return FALSE;
}

// Called during DFS; visits one child. Decides if it is a candidate to be scanned, and if it is a candidate for forwarding.
static void
initial_scan_check (ScanData *parent, GCObject *obj, ScanData **push_obj, ScanData **forward_obj)
{
	ScanData *data;
	obj = bridge_object_forward (obj);

#if DUMP_GRAPH
	printf ("\t= pushing %p %s -> ", obj, safe_name_bridge (obj));
#endif

	/* Reference we can ignore */
	if (parent->obj == obj) {
#if DUMP_GRAPH
		printf ("self-reference\n");
#endif
		return;
	}

	/* Object types we can ignore */
	if (is_opaque_object (obj)) {
#if DUMP_GRAPH
		printf ("opaque\n");
#endif
		return;
	}

	data = find_data (obj);

	/* Already marked - XXX must be done this way as the bridge themselves are alive. */
	if (data && data->state != INITIAL) {
#if DUMP_GRAPH
		printf ("already marked\n");
#endif

		// Notice we only follow forward for non-initial objects (since forwarding is added on first scan)
		*forward_obj = data;

		return;
	}

	/* We only care about dead objects */
	if (!data && sgen_object_is_live (obj)) {
#if DUMP_GRAPH
		printf ("alive\n");
#endif
		return;
	}

#if DUMP_GRAPH
	printf ("pushed!\n");
#endif

	if (!data)
		data = create_data (obj);
	g_assert (data->state == INITIAL);
	g_assert (data->index == -1);

	*forward_obj = data;
	*push_obj = data;
}

static void
mark_object_scanned (ScanData *data)
{
	data->low_index = data->index = object_index++;
	dyn_array_ptr_push (&scan_stack, data);
	dyn_array_ptr_push (&loop_stack, data);
}

// The target of the scan gets pushed onto the stack first.
// However, when using OPTIMIZATION_FORWARD, we want to avoid pushing the object
// if it has exactly one forwardable reference. Therefore we wait to push the
// scan target until we know this criteria has failed.
// initial_scan_one returns as arguments a forwardable argument (if any) and
// a pushable object (if any). pushable objects are always forwardable.
static void
initial_scan_one (ScanData *data, GCObject *obj, ScanData **first_push, ScanData **first_forward, gboolean *forward_check_finished)
{
	ScanData *to_push = NULL, *to_forward = NULL;
	initial_scan_check(data, obj, &to_push, &to_forward);
	if (*forward_check_finished) {
		if (to_push) {
			dyn_array_ptr_push (&scan_stack, to_push);
		}
	} else if (to_forward) {
		if (*first_forward) {
			mark_object_scanned(data);
			*forward_check_finished = TRUE;

			if (*first_push)
				dyn_array_ptr_push (&scan_stack, *first_push);
			if (to_push)
				dyn_array_ptr_push (&scan_stack, to_push);
		} else {
			*first_push = to_push;
			*first_forward = to_forward;
		}
	}
}

#undef HANDLE_PTR
#define HANDLE_PTR(ptr,obj)	do {					\
		GCObject *dst = (GCObject*)*(ptr);			\
		if (dst)                                    \
			initial_scan_one (data, dst, &first_push, &first_forward, &forward_check_finished ); \
	} while (0)

// dfs () function's queue-children-of-object operation.
static void
initial_scan (ScanData *data)
{
	GCObject *obj = data->obj;
	char *start = (char*)obj;
	mword desc = sgen_obj_get_descriptor_safe (obj);

	gboolean forward_check_finished = FALSE;
	ScanData *first_push = NULL, *first_forward = NULL;

	// Not forwarding, do not defer this
#ifdef OPTIMIZATION_FORWARD
	if (data->is_bridge)
#endif
	{
		mark_object_scanned (data);
		forward_check_finished = TRUE;
	}

#if DUMP_GRAPH
	printf ("+scanning %s (%p) index %d color %p\n", safe_name_bridge (data->obj), data->obj, data->index, data->color);
#endif

	#include "sgen/sgen-scan-object.h"

	/*
	 * We can remove non-bridge objects with a single outgoing
	 * link (tested by push_all) by forwarding links going to it.
	 *
	 * This is the first time we've encountered this object, so
	 * no links to it have yet been added.  We'll keep it that
	 * way by setting the forward pointer, and instead of
	 * continuing processing this object, we start over with the
	 * object it points to.
	 */
#ifdef OPTIMIZATION_FORWARD
	if (!forward_check_finished) {
		if (first_forward) {
			ScanData *true_forward = follow_forward(first_forward);
			// Don't allow loops. If this is a self-reference, pretend it isn't even there.
			// Worth investigating: new-bridge didn't require this for OPTIMIZATION_FORWARD. Why does tarjan-bridge?
			if (true_forward == data)
				first_forward = NULL;
			else
				first_forward = true_forward;
		}

		if (first_forward) {
			data->forwarded_to = first_forward;

			// The object forwarded to was a candidate for scanning, make sure that happens next
			if (first_push)
				dyn_array_ptr_push (&scan_stack, first_forward);

#if DUMP_GRAPH
			printf ("+forwarding %s (%p obj %p) to %s (%p obj %p)%s\n", safe_name_bridge (data->obj), data, data->obj, safe_name_bridge (first_forward->obj), first_forward, first_forward->obj, data->is_bridge?" (is bridge)":"");
#endif
		} else {
			// This is not a forward, but a leaf
			mark_object_scanned (data);
		}
	}
#endif
}


static void
compute_low_index (ScanData *data, GCObject *obj)
{
	ScanData *other;
	ColorData *cd;

	obj = bridge_object_forward (obj);
	other = find_data (obj);
	if (other)
		other = follow_forward (other);

#if DUMP_GRAPH
	printf ("\tcompute low %p ->%p (%s) %p (%d / %d)\n", data->obj, obj, safe_name_bridge (obj), other, other ? other->index : -2, other ? other->low_index : -2);
#endif
	if (!other)
		return;

	g_assert (other->state != INITIAL);

	if ((other->state == SCANNED || other->state == FINISHED_ON_STACK) && data->low_index > other->low_index)
		data->low_index = other->low_index;

	/* Compute the low color */
	if (other->color == NULL)
		return;

	cd = other->color;
	if (!cd->visited) {
		dyn_array_ptr_add (&color_merge_array, other->color);
		cd->visited = TRUE;
	}
}

#undef HANDLE_PTR
#define HANDLE_PTR(ptr,obj)	do {					\
		GCObject *dst = (GCObject*)*(ptr);			\
		if (dst) compute_low_index (data, dst); 			\
	} while (0)

static void
compute_low (ScanData *data)
{
	GCObject *obj = data->obj;
	char *start = (char*)obj;
	mword desc = sgen_obj_get_descriptor_safe (obj);

	#include "sgen/sgen-scan-object.h"
}

static ColorData*
reduce_color (void)
{
	ColorData *color = NULL;
	int size = dyn_array_ptr_size (&color_merge_array);

	if (size == 0)
		color = NULL;
	else if (size == 1) {
		color = (ColorData *)dyn_array_ptr_get (&color_merge_array, 0);
	} else
		color = new_color (FALSE);

	return color;
}

static void
create_scc (ScanData *data)
{
	int i;
	gboolean found = FALSE;
	gboolean found_bridge = FALSE;
	ColorData *color_data = NULL;

	for (i = dyn_array_ptr_size (&loop_stack) - 1; i >= 0; --i) {
		ScanData *other = (ScanData *)dyn_array_ptr_get (&loop_stack, i);
		found_bridge |= other->is_bridge;
		if (found_bridge || other == data)
			break;
	}

#if DUMP_GRAPH
	printf ("|SCC rooted in %s (%p) has bridge %d\n", safe_name_bridge (data->obj), data->obj, found_bridge);
	printf ("\tpoints-to-colors: ");
	for (int i = 0; i < dyn_array_ptr_size (&color_merge_array); ++i)
		printf ("%p ", dyn_array_ptr_get (&color_merge_array, i));
	printf ("\n");

	printf ("loop stack: ");
	for (int i = 0; i < dyn_array_ptr_size (&loop_stack); ++i) {
		ScanData *other = dyn_array_ptr_get (&loop_stack, i);
		printf ("(%d/%d)", other->index, other->low_index);
	}
	printf ("\n");
#endif

	if (found_bridge) {
		color_data = new_color (TRUE);
		++num_colors_with_bridges;
	} else {
		color_data = reduce_color ();
	}

	while (dyn_array_ptr_size (&loop_stack) > 0) {
		ScanData *other = (ScanData *)dyn_array_ptr_pop (&loop_stack);

#if DUMP_GRAPH
		printf ("\tmember %s (%p) index %d low-index %d color %p state %d\n", safe_name_bridge (other->obj), other->obj, other->index, other->low_index, other->color, other->state);
#endif

#ifdef OPTIMIZATION_FORWARD
		g_assert(!other->forwarded_to);
#endif

		other->color = color_data;
		switch (other->state) {
		case FINISHED_ON_STACK:
			other->state = FINISHED_OFF_STACK;
			break;
		case FINISHED_OFF_STACK:
			break;
		default:
			g_error ("Invalid state when building SCC %d", other->state);
		}

		if (other->is_bridge)
			dyn_array_ptr_add (&color_data->bridges, other->obj);

		if (other == data) {
			found = TRUE;
			break;
		}
	}
	g_assert (found);

	for (i = 0; i < dyn_array_ptr_size (&color_merge_array); ++i) {
		ColorData *cd  = (ColorData *)dyn_array_ptr_get (&color_merge_array, i);
		g_assert (cd->visited);
		cd->visited = FALSE;
	}
	dyn_array_ptr_empty (&color_merge_array);
	found_bridge = FALSE;
}

static void
dfs (void)
{
	g_assert (dyn_array_ptr_size (&scan_stack) == 1);
	g_assert (dyn_array_ptr_size (&loop_stack) == 0);

	dyn_array_ptr_empty (&color_merge_array);

	while (dyn_array_ptr_size (&scan_stack) > 0) {
		ScanData *data = (ScanData *)dyn_array_ptr_pop (&scan_stack);

		data = follow_forward(data);

		/**
		 * Ignore finished objects on stack, they happen due to loops. For example:
		 * A -> C
		 * A -> B
		 * B -> C
		 * C -> A
		 *
		 * We start scanning from A and push C before B. So, after the first iteration, the scan stack will have: A C B.
		 * We then visit B, which will find C in its initial state and push again.
		 * Finally after finish with C and B, the stack will be left with "A C" and at this point C should be ignored.
         *
         * The above explains FINISHED_ON_STACK, to explain FINISHED_OFF_STACK, consider if the root was D, which pointed
		 * to A and C. A is processed first, leaving C on stack after that in the mentioned state.
		 */
		if (data->state == FINISHED_ON_STACK || data->state == FINISHED_OFF_STACK)
			continue;

		if (data->state == INITIAL) {
			g_assert (data->index == -1);
			g_assert (data->low_index == -1);

			data->state = SCANNED;

			initial_scan (data);

		} else {
			g_assert (data->state == SCANNED);
			data->state = FINISHED_ON_STACK;

#if DUMP_GRAPH
			printf ("-finishing %s (%p) index %d low-index %d color %p\n", safe_name_bridge (data->obj), data->obj, data->index, data->low_index, data->color);
#endif

			/* Compute low index */
			compute_low (data);

#if DUMP_GRAPH
			printf ("-finished %s (%p) index %d low-index %d color %p\n", safe_name_bridge (data->obj), data->obj, data->index, data->low_index, data->color);
#endif
			//SCC root
			if (data->index == data->low_index)
				create_scc (data);
		}
	}
}

static void
register_finalized_object (GCObject *obj)
{
	g_assert (sgen_need_bridge_processing ());
	dyn_array_ptr_push (&registered_bridges, obj);
}

static void
reset_data (void)
{
	dyn_array_ptr_empty (&registered_bridges);
}

static void
cleanup (void)
{
	dyn_array_ptr_empty (&scan_stack);
	dyn_array_ptr_empty (&loop_stack);
	dyn_array_ptr_empty (&registered_bridges);
	free_object_buckets ();
	free_color_buckets ();
	reset_cache ();
	object_index = 0;
	num_colors_with_bridges = 0;
}

#ifdef DUMP_GRAPH
static void
dump_color_table (const char *why, gboolean do_index)
{
	ColorBucket *cur;
	int i = 0, j;
	printf ("colors%s:\n", why);

	for (cur = root_color_bucket; cur; cur = cur->next, ++i) {
		ColorData *cd;
		for (cd = &cur->data [0]; cd < cur->next_data; ++cd) {
			if (do_index)
				printf ("\t%d(%d):", i, cd->api_index);
			else
				printf ("\t%d: ", i);
			for (j = 0; j < dyn_array_ptr_size (&cd->other_colors); ++j) {
				printf ("%p ", dyn_array_ptr_get (&cd->other_colors, j));
			}
			if (dyn_array_ptr_size (&cd->bridges)) {
				printf (" bridges: ");
				for (j = 0; j < dyn_array_ptr_size (&cd->bridges); ++j) {
					GCObject *obj = dyn_array_ptr_get (&cd->bridges, j);
					ScanData *data = find_or_create_data (obj);
					printf ("%d ", data->index);
				}
			}
			printf ("\n");
		}
	}

}
#endif

static gint64
step_timer (gint64 *timer)
{
	gint64 curtime, diff;

	SGEN_TV_GETTIME (curtime); 
	diff = SGEN_TV_ELAPSED (*timer, curtime);
	*timer = curtime;
	return diff;
}
static void
processing_stw_step (void)
{
	int i;
	int bridge_count;
	gint64 curtime;

	if (!dyn_array_ptr_size (&registered_bridges))
		return;

#if defined (DUMP_GRAPH)
	printf ("-----------------\n");
#endif

	SGEN_TV_GETTIME (curtime);

	object_alloc_init ();
	color_alloc_init ();

	bridge_count = dyn_array_ptr_size (&registered_bridges);
	for (i = 0; i < bridge_count ; ++i)
		register_bridge_object ((GCObject *)dyn_array_ptr_get (&registered_bridges, i));

	setup_time = step_timer (&curtime);

	for (i = 0; i < bridge_count; ++i) {
		ScanData *sd = find_data ((GCObject *)dyn_array_ptr_get (&registered_bridges, i));
		if (sd->state == INITIAL) {
			dyn_array_ptr_push (&scan_stack, sd);
			dfs ();
		} else {
			g_assert (sd->state == FINISHED_OFF_STACK);
		}
	}

	tarjan_time = step_timer (&curtime);

#if defined (DUMP_GRAPH)
	printf ("----summary----\n");
	printf ("bridges:\n");
	for (int i = 0; i < bridge_count; ++i) {
		ScanData *sd = find_data (dyn_array_ptr_get (&registered_bridges, i));
		printf ("\t%s (%p) index %d color %p\n", safe_name_bridge (sd->obj), sd->obj, sd->index, sd->color);
	}

	dump_color_table (" after tarjan", FALSE);
#endif

	clear_after_processing ();
}


static void
gather_xrefs (ColorData *color)
{
	int i;
	for (i = 0; i < dyn_array_ptr_size (&color->other_colors); ++i) {
		ColorData *src = (ColorData *)dyn_array_ptr_get (&color->other_colors, i);
		if (src->visited)
			continue;
		src->visited = TRUE;
		if (dyn_array_ptr_size (&src->bridges))
			dyn_array_ptr_add (&color_merge_array, src);
		else
			gather_xrefs (src);
	}
}

static void
reset_xrefs (ColorData *color)
{
	int i;
	for (i = 0; i < dyn_array_ptr_size (&color->other_colors); ++i) {
		ColorData *src = (ColorData *)dyn_array_ptr_get (&color->other_colors, i);
		if (!src->visited)
			continue;
		src->visited = FALSE;
		if (!dyn_array_ptr_size (&src->bridges))
			reset_xrefs (src);
	}
}

static void
processing_build_callback_data (int generation)
{
	int j, api_index;
	MonoGCBridgeSCC **api_sccs;
	MonoGCBridgeXRef *api_xrefs;
	gint64 curtime;
	ColorBucket *cur;

	g_assert (bridge_processor->num_sccs == 0 && bridge_processor->num_xrefs == 0);
	g_assert (!bridge_processor->api_sccs && !bridge_processor->api_xrefs);

	if (!dyn_array_ptr_size (&registered_bridges))
		return;

	SGEN_TV_GETTIME (curtime);

	/*create API objects */

#if defined (DUMP_GRAPH)
	printf ("***** API *****\n");
	printf ("number of SCCs %d\n", num_colors_with_bridges);
#endif

	/* This is a straightforward translation from colors to the bridge callback format. */
	api_sccs = (MonoGCBridgeSCC **)sgen_alloc_internal_dynamic (sizeof (MonoGCBridgeSCC*) * num_colors_with_bridges, INTERNAL_MEM_BRIDGE_DATA, TRUE);
	api_index = xref_count = 0;

	for (cur = root_color_bucket; cur; cur = cur->next) {
		ColorData *cd;
		for (cd = &cur->data [0]; cd < cur->next_data; ++cd) {
			int bridges = dyn_array_ptr_size (&cd->bridges);
			if (!bridges)
				continue;

			api_sccs [api_index] = (MonoGCBridgeSCC *)sgen_alloc_internal_dynamic (sizeof (MonoGCBridgeSCC) + sizeof (MonoObject*) * bridges, INTERNAL_MEM_BRIDGE_DATA, TRUE);
			api_sccs [api_index]->is_alive = FALSE;
			api_sccs [api_index]->num_objs = bridges;

			cd->api_index = api_index;

			for (j = 0; j < bridges; ++j)
				api_sccs [api_index]->objs [j] = (MonoObject *)dyn_array_ptr_get (&cd->bridges, j);
			api_index++;
		}
	}

	scc_setup_time = step_timer (&curtime);

	for (cur = root_color_bucket; cur; cur = cur->next) {
		ColorData *cd;
		for (cd = &cur->data [0]; cd < cur->next_data; ++cd) {
			int bridges = dyn_array_ptr_size (&cd->bridges);
			if (!bridges)
				continue;

			dyn_array_ptr_empty (&color_merge_array);
			gather_xrefs (cd);
			reset_xrefs (cd);
			dyn_array_ptr_set_all (&cd->other_colors, &color_merge_array);
			xref_count += dyn_array_ptr_size (&cd->other_colors);
		}
	}

	gather_xref_time = step_timer (&curtime);

#if defined (DUMP_GRAPH)
	printf ("TOTAL XREFS %d\n", xref_count);
	dump_color_table (" after xref pass", TRUE);
#endif

	api_xrefs = (MonoGCBridgeXRef *)sgen_alloc_internal_dynamic (sizeof (MonoGCBridgeXRef) * xref_count, INTERNAL_MEM_BRIDGE_DATA, TRUE);
	api_index = 0;
	for (cur = root_color_bucket; cur; cur = cur->next) {
		ColorData *src;
		for (src = &cur->data [0]; src < cur->next_data; ++src) {
			int bridges = dyn_array_ptr_size (&src->bridges);
			if (!bridges)
				continue;

			for (j = 0; j < dyn_array_ptr_size (&src->other_colors); ++j) {
				ColorData *dest = (ColorData *)dyn_array_ptr_get (&src->other_colors, j);
				g_assert (dyn_array_ptr_size (&dest->bridges)); /* We flattened the color graph, so this must never happen. */

				api_xrefs [api_index].src_scc_index = src->api_index;
				api_xrefs [api_index].dst_scc_index = dest->api_index;
				++api_index;
			}
		}
	}

	g_assert (xref_count == api_index);
	xref_setup_time = step_timer (&curtime);

#if defined (DUMP_GRAPH)
	printf ("---xrefs:\n");
	for (int i = 0; i < xref_count; ++i)
		printf ("\t%d -> %d\n", api_xrefs [i].src_scc_index, api_xrefs [i].dst_scc_index);
#endif

	//FIXME move half of the cleanup to before the bridge callback?
	bridge_processor->num_sccs = num_colors_with_bridges;
	bridge_processor->api_sccs = api_sccs;
	bridge_processor->num_xrefs = xref_count;
	bridge_processor->api_xrefs = api_xrefs;
}

static void
processing_after_callback (int generation)
{
	gint64 curtime;
	int bridge_count = dyn_array_ptr_size (&registered_bridges);
	int object_count = object_data_count;
	int color_count = color_data_count;
	int scc_count = num_colors_with_bridges;

	SGEN_TV_GETTIME (curtime);

	/* cleanup */
	cleanup ();

	cleanup_time = step_timer (&curtime);

	mono_trace (G_LOG_LEVEL_INFO, MONO_TRACE_GC, "GC_TAR_BRIDGE bridges %d objects %d colors %d ignored %d sccs %d xref %d cache %d/%d setup %.2fms tarjan %.2fms scc-setup %.2fms gather-xref %.2fms xref-setup %.2fms cleanup %.2fms",
		bridge_count, object_count, color_count,
		ignored_objects, scc_count, xref_count,
		cache_hits, cache_misses,
		setup_time / 10000.0f,
		tarjan_time / 10000.0f,
		scc_setup_time / 10000.0f,
		gather_xref_time / 10000.0f,
		xref_setup_time / 10000.0f,
		cleanup_time / 10000.0f);

	cache_hits = cache_misses = 0;
	ignored_objects = 0;
}

static void
describe_pointer (GCObject *obj)
{
	// HashEntry *entry;
	int i;

	for (i = 0; i < dyn_array_ptr_size (&registered_bridges); ++i) {
		if (obj == dyn_array_ptr_get (&registered_bridges, i)) {
			printf ("Pointer is a registered bridge object.\n");
			break;
		}
	}

	// entry = sgen_hash_table_lookup (&hash_table, obj);
	// if (!entry)
	// 	return;
	//
	// printf ("Bridge hash table entry %p:\n", entry);
	// printf ("  is bridge: %d\n", (int)entry->is_bridge);
	// printf ("  is visited: %d\n", (int)entry->is_visited);
}

void
sgen_tarjan_bridge_init (SgenBridgeProcessor *collector)
{
	collector->reset_data = reset_data;
	collector->processing_stw_step = processing_stw_step;
	collector->processing_build_callback_data = processing_build_callback_data;
	collector->processing_after_callback = processing_after_callback;
	collector->class_kind = class_kind;
	collector->register_finalized_object = register_finalized_object;
	collector->describe_pointer = describe_pointer;
	collector->enable_accounting = enable_accounting;
	// collector->set_dump_prefix = set_dump_prefix;

	sgen_register_fixed_internal_mem_type (INTERNAL_MEM_TARJAN_OBJ_BUCKET, BUCKET_SIZE);
	g_assert (sizeof (ObjectBucket) <= BUCKET_SIZE);
	g_assert (sizeof (ColorBucket) <= BUCKET_SIZE);
	bridge_processor = collector;
}

#endif
