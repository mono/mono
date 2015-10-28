/*
 * sgen-fin-weak-hash.c: Finalizers and weak links.
 *
 * Author:
 * 	Paolo Molaro (lupus@ximian.com)
 *  Rodrigo Kumpera (kumpera@gmail.com)
 *
 * Copyright 2005-2011 Novell, Inc (http://www.novell.com)
 * Copyright 2011 Xamarin Inc (http://www.xamarin.com)
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
 */

#include "config.h"
#ifdef HAVE_SGEN_GC

#include "mono/sgen/sgen-gc.h"
#include "mono/sgen/sgen-gray.h"
#include "mono/sgen/sgen-protocol.h"
#include "mono/sgen/sgen-pointer-queue.h"
#include "mono/sgen/sgen-client.h"
#include "mono/sgen/gc-internal-agnostic.h"
#include "mono/utils/mono-membar.h"
// FIXME: remove!
#ifndef SGEN_WITHOUT_MONO
#include "mono/metadata/gc-internal.h"
#endif

#define ptr_in_nursery sgen_ptr_in_nursery

typedef SgenGrayQueue GrayQueue;

static int no_finalize = 0;

/*
 * The finalizable hash has the object as the key, the 
 * disappearing_link hash, has the link address as key.
 *
 * Copyright 2011 Xamarin Inc.
 */

#define TAG_MASK ((mword)0x1)

static inline GCObject*
tagged_object_get_object (GCObject *object)
{
	return (GCObject*)(((mword)object) & ~TAG_MASK);
}

static inline int
tagged_object_get_tag (GCObject *object)
{
	return ((mword)object) & TAG_MASK;
}

static inline GCObject*
tagged_object_apply (void *object, int tag_bits)
{
       return (GCObject*)((mword)object | (mword)tag_bits);
}

static int
tagged_object_hash (GCObject *o)
{
	return sgen_aligned_addr_hash (tagged_object_get_object (o));
}

static gboolean
tagged_object_equals (GCObject *a, GCObject *b)
{
	return tagged_object_get_object (a) == tagged_object_get_object (b);
}

static SgenHashTable minor_finalizable_hash = SGEN_HASH_TABLE_INIT (INTERNAL_MEM_FIN_TABLE, INTERNAL_MEM_FINALIZE_ENTRY, 0, (GHashFunc)tagged_object_hash, (GEqualFunc)tagged_object_equals);
static SgenHashTable major_finalizable_hash = SGEN_HASH_TABLE_INIT (INTERNAL_MEM_FIN_TABLE, INTERNAL_MEM_FINALIZE_ENTRY, 0, (GHashFunc)tagged_object_hash, (GEqualFunc)tagged_object_equals);

static SgenHashTable*
get_finalize_entry_hash_table (int generation)
{
	switch (generation) {
	case GENERATION_NURSERY: return &minor_finalizable_hash;
	case GENERATION_OLD: return &major_finalizable_hash;
	default: g_assert_not_reached ();
	}
}

#define BRIDGE_OBJECT_MARKED 0x1

/* LOCKING: requires that the GC lock is held */
void
sgen_mark_bridge_object (GCObject *obj)
{
	SgenHashTable *hash_table = get_finalize_entry_hash_table (ptr_in_nursery (obj) ? GENERATION_NURSERY : GENERATION_OLD);

	sgen_hash_table_set_key (hash_table, obj, tagged_object_apply (obj, BRIDGE_OBJECT_MARKED));
}

/* LOCKING: requires that the GC lock is held */
void
sgen_collect_bridge_objects (int generation, ScanCopyContext ctx)
{
	CopyOrMarkObjectFunc copy_func = ctx.ops->copy_or_mark_object;
	GrayQueue *queue = ctx.queue;
	SgenHashTable *hash_table = get_finalize_entry_hash_table (generation);
	GCObject *object;
	gpointer dummy G_GNUC_UNUSED;
	GCObject *copy;
	SgenPointerQueue moved_fin_objects;

	sgen_pointer_queue_init (&moved_fin_objects, INTERNAL_MEM_TEMPORARY);

	if (no_finalize)
		return;

	SGEN_HASH_TABLE_FOREACH (hash_table, object, dummy) {
		int tag = tagged_object_get_tag (object);
		object = tagged_object_get_object (object);

		/* Bridge code told us to ignore this one */
		if (tag == BRIDGE_OBJECT_MARKED)
			continue;

		/* Object is a bridge object and major heap says it's dead  */
		if (major_collector.is_object_live (object))
			continue;

		/* Nursery says the object is dead. */
		if (!sgen_gc_is_object_ready_for_finalization (object))
			continue;

		if (!sgen_client_bridge_is_bridge_object (object))
			continue;

		copy = object;
		copy_func (&copy, queue);

		sgen_client_bridge_register_finalized_object (copy);
		
		if (hash_table == &minor_finalizable_hash && !ptr_in_nursery (copy)) {
			/* remove from the list */
			SGEN_HASH_TABLE_FOREACH_REMOVE (TRUE);

			/* insert it into the major hash */
			sgen_hash_table_replace (&major_finalizable_hash, tagged_object_apply (copy, tag), NULL, NULL);

			SGEN_LOG (5, "Promoting finalization of object %p (%s) (was at %p) to major table", copy, sgen_client_vtable_get_name (SGEN_LOAD_VTABLE (copy)), object);

			continue;
		} else if (copy != object) {
			/* update pointer */
			SGEN_HASH_TABLE_FOREACH_REMOVE (TRUE);

			/* register for reinsertion */
			sgen_pointer_queue_add (&moved_fin_objects, tagged_object_apply (copy, tag));

			SGEN_LOG (5, "Updating object for finalization: %p (%s) (was at %p)", copy, sgen_client_vtable_get_name (SGEN_LOAD_VTABLE (copy)), object);

			continue;
		}
	} SGEN_HASH_TABLE_FOREACH_END;

	while (!sgen_pointer_queue_is_empty (&moved_fin_objects)) {
		sgen_hash_table_replace (hash_table, sgen_pointer_queue_pop (&moved_fin_objects), NULL, NULL);
	}

	sgen_pointer_queue_free (&moved_fin_objects);
}


/* LOCKING: requires that the GC lock is held */
void
sgen_finalize_in_range (int generation, ScanCopyContext ctx)
{
	CopyOrMarkObjectFunc copy_func = ctx.ops->copy_or_mark_object;
	GrayQueue *queue = ctx.queue;
	SgenHashTable *hash_table = get_finalize_entry_hash_table (generation);
	GCObject *object;
	gpointer dummy G_GNUC_UNUSED;
	SgenPointerQueue moved_fin_objects;

	sgen_pointer_queue_init (&moved_fin_objects, INTERNAL_MEM_TEMPORARY);

	if (no_finalize)
		return;
	SGEN_HASH_TABLE_FOREACH (hash_table, object, dummy) {
		int tag = tagged_object_get_tag (object);
		object = tagged_object_get_object (object);
		if (!major_collector.is_object_live (object)) {
			gboolean is_fin_ready = sgen_gc_is_object_ready_for_finalization (object);
			GCObject *copy = object;
			copy_func (&copy, queue);
			if (is_fin_ready) {
				/* remove and put in fin_ready_list */
				SGEN_HASH_TABLE_FOREACH_REMOVE (TRUE);
				sgen_queue_finalization_entry (copy);
				/* Make it survive */
				SGEN_LOG (5, "Queueing object for finalization: %p (%s) (was at %p) (%d)", copy, sgen_client_vtable_get_name (SGEN_LOAD_VTABLE (copy)), object, sgen_hash_table_num_entries (hash_table));
				continue;
			} else {
				if (hash_table == &minor_finalizable_hash && !ptr_in_nursery (copy)) {
					/* remove from the list */
					SGEN_HASH_TABLE_FOREACH_REMOVE (TRUE);

					/* insert it into the major hash */
					sgen_hash_table_replace (&major_finalizable_hash, tagged_object_apply (copy, tag), NULL, NULL);

					SGEN_LOG (5, "Promoting finalization of object %p (%s) (was at %p) to major table", copy, sgen_client_vtable_get_name (SGEN_LOAD_VTABLE (copy)), object);

					continue;
				} else if (copy != object) {
					/* update pointer */
					SGEN_HASH_TABLE_FOREACH_REMOVE (TRUE);

					/* register for reinsertion */
					sgen_pointer_queue_add (&moved_fin_objects, tagged_object_apply (copy, tag));

					SGEN_LOG (5, "Updating object for finalization: %p (%s) (was at %p)", copy, sgen_client_vtable_get_name (SGEN_LOAD_VTABLE (copy)), object);

					continue;
				}
			}
		}
	} SGEN_HASH_TABLE_FOREACH_END;

	while (!sgen_pointer_queue_is_empty (&moved_fin_objects)) {
		sgen_hash_table_replace (hash_table, sgen_pointer_queue_pop (&moved_fin_objects), NULL, NULL);
	}

	sgen_pointer_queue_free (&moved_fin_objects);
}

/* LOCKING: requires that the GC lock is held */
static void
register_for_finalization (GCObject *obj, void *user_data, int generation)
{
	SgenHashTable *hash_table = get_finalize_entry_hash_table (generation);

	if (no_finalize)
		return;

	if (user_data) {
		if (sgen_hash_table_replace (hash_table, obj, NULL, NULL)) {
			GCVTable vt = SGEN_LOAD_VTABLE_UNCHECKED (obj);
			SGEN_LOG (5, "Added finalizer for object: %p (%s) (%d) to %s table", obj, sgen_client_vtable_get_name (vt), hash_table->num_entries, sgen_generation_name (generation));
		}
	} else {
		if (sgen_hash_table_remove (hash_table, obj, NULL)) {
			GCVTable vt = SGEN_LOAD_VTABLE_UNCHECKED (obj);
			SGEN_LOG (5, "Removed finalizer for object: %p (%s) (%d)", obj, sgen_client_vtable_get_name (vt), hash_table->num_entries);
		}
	}
}

/*
 * We're using (mostly) non-locking staging queues for finalizers and weak links to speed
 * up registering them.  Otherwise we'd have to take the GC lock.
 *
 * The queues are arrays of `StageEntry`, plus a `next_entry` index.  Threads add entries to
 * the queue via `add_stage_entry()` in a linear fashion until it fills up, in which case
 * `process_stage_entries()` is called to drain it.  A garbage collection will also drain
 * the queues via the same function.  That implies that `add_stage_entry()`, since it
 * doesn't take a lock, must be able to run concurrently with `process_stage_entries()`,
 * though it doesn't have to make progress while the queue is drained.  In fact, once it
 * detects that the queue is being drained, it blocks until the draining is done.
 *
 * The protocol must guarantee that entries in the queue are causally ordered, otherwise two
 * entries for the same location might get switched, resulting in the earlier one being
 * committed and the later one ignored.
 *
 * `next_entry` is the index of the next entry to be filled, or `-1` if the queue is
 * currently being drained.  Each entry has a state:
 *
 * `STAGE_ENTRY_FREE`: The entry is free.  Its data fields must be `NULL`.
 *
 * `STAGE_ENTRY_BUSY`: The entry is currently being filled in.
 *
 * `STAGE_ENTRY_USED`: The entry is completely filled in and must be processed in the next
 * draining round.
 *
 * `STAGE_ENTRY_INVALID`: The entry was busy during queue draining and therefore
 * invalidated.  Entries that are `BUSY` can obviously not be processed during a drain, but
 * we can't leave them in place because new entries might be inserted before them, including
 * from the same thread, violating causality.  An alternative would be not to reset
 * `next_entry` to `0` after a drain, but to the index of the last `BUSY` entry plus one,
 * but that can potentially waste the whole queue.
 *
 * State transitions:
 *
 * | from    | to      | filler? | drainer? |
 * +---------+---------+---------+----------+
 * | FREE    | BUSY    | X       |          |
 * | BUSY    | FREE    | X       |          |
 * | BUSY    | USED    | X       |          |
 * | BUSY    | INVALID |         | X        |
 * | USED    | FREE    |         | X        |
 * | INVALID | FREE    | X       |          |
 *
 * `next_entry` can be incremented either by the filler thread that set the corresponding
 * entry to `BUSY`, or by another filler thread that's trying to get a `FREE` slot.  If that
 * other thread wasn't allowed to increment, it would block on the first filler thread.
 *
 * An entry's state, once it's set from `FREE` to `BUSY` by a filler thread, can only be
 * changed by that same thread or by the drained.  The drainer can only set a `BUSY` thread
 * to `INVALID`, so it needs to be set to `FREE` again by the original filler thread.
 */

#define STAGE_ENTRY_FREE	0
#define STAGE_ENTRY_BUSY	1
#define STAGE_ENTRY_USED	2
#define STAGE_ENTRY_INVALID	3

typedef struct {
	volatile gint32 state;
	GCObject *obj;
	void *user_data;
} StageEntry;

#define NUM_FIN_STAGE_ENTRIES	1024

static volatile gint32 next_fin_stage_entry = 0;
static StageEntry fin_stage_entries [NUM_FIN_STAGE_ENTRIES];

/*
 * This is used to lock the stage when processing is forced, i.e. when it's triggered by a
 * garbage collection.  In that case, the world is already stopped and there's only one
 * thread operating on the queue.
 */
static void
lock_stage_for_processing (volatile gint32 *next_entry)
{
	*next_entry = -1;
}

/*
 * When processing is triggered by an overflow, we don't want to take the GC lock
 * immediately, and then set `next_index` to `-1`, because another thread might have drained
 * the queue in the mean time.  Instead, we make sure the overflow is still there, we
 * atomically set `next_index`, and only once that happened do we take the GC lock.
 */
static gboolean
try_lock_stage_for_processing (int num_entries, volatile gint32 *next_entry)
{
	gint32 old = *next_entry;
	if (old < num_entries)
		return FALSE;
	return InterlockedCompareExchange (next_entry, -1, old) == old;
}

/* LOCKING: requires that the GC lock is held */
static void
process_stage_entries (int num_entries, volatile gint32 *next_entry, StageEntry *entries, void (*process_func) (GCObject*, void*, int))
{
	int i;

	/*
	 * This can happen if after setting `next_index` to `-1` in
	 * `try_lock_stage_for_processing()`, a GC was triggered, which then drained the
	 * queue and reset `next_entry`.
	 *
	 * We have the GC lock now, so if it's still `-1`, we can't be interrupted by a GC.
	 */
	if (*next_entry != -1)
		return;

	for (i = 0; i < num_entries; ++i) {
		gint32 state;

	retry:
		state = entries [i].state;

		switch (state) {
		case STAGE_ENTRY_FREE:
		case STAGE_ENTRY_INVALID:
			continue;
		case STAGE_ENTRY_BUSY:
			/* BUSY -> INVALID */
			/*
			 * This must be done atomically, because the filler thread can set
			 * the entry to `USED`, in which case we must process it, so we must
			 * detect that eventuality.
			 */
			if (InterlockedCompareExchange (&entries [i].state, STAGE_ENTRY_INVALID, STAGE_ENTRY_BUSY) != STAGE_ENTRY_BUSY)
				goto retry;
			continue;
		case STAGE_ENTRY_USED:
			break;
		default:
			SGEN_ASSERT (0, FALSE, "Invalid stage entry state");
			break;
		}

		/* state is USED */

		process_func (entries [i].obj, entries [i].user_data, i);

		entries [i].obj = NULL;
		entries [i].user_data = NULL;

		mono_memory_write_barrier ();

		/* USED -> FREE */
		/*
		 * This transition only happens here, so we don't have to do it atomically.
		 */
		entries [i].state = STAGE_ENTRY_FREE;
	}

	mono_memory_write_barrier ();

	*next_entry = 0;
}

#ifdef HEAVY_STATISTICS
static guint64 stat_overflow_abort = 0;
static guint64 stat_wait_for_processing = 0;
static guint64 stat_increment_other_thread = 0;
static guint64 stat_index_decremented = 0;
static guint64 stat_entry_invalidated = 0;
static guint64 stat_success = 0;
#endif

static int
add_stage_entry (int num_entries, volatile gint32 *next_entry, StageEntry *entries, GCObject *obj, void *user_data)
{
	gint32 index, new_next_entry, old_next_entry;
	gint32 previous_state;

 retry:
	for (;;) {
		index = *next_entry;
		if (index >= num_entries) {
			HEAVY_STAT (++stat_overflow_abort);
			return -1;
		}
		if (index < 0) {
			/*
			 * Backed-off waiting is way more efficient than even using a
			 * dedicated lock for this.
			 */
			while ((index = *next_entry) < 0) {
				/*
				 * This seems like a good value.  Determined by timing
				 * sgen-weakref-stress.exe.
				 */
				g_usleep (200);
				HEAVY_STAT (++stat_wait_for_processing);
			}
			continue;
		}
		/* FREE -> BUSY */
		if (entries [index].state != STAGE_ENTRY_FREE ||
				InterlockedCompareExchange (&entries [index].state, STAGE_ENTRY_BUSY, STAGE_ENTRY_FREE) != STAGE_ENTRY_FREE) {
			/*
			 * If we can't get the entry it must be because another thread got
			 * it first.  We don't want to wait for that thread to increment
			 * `next_entry`, so we try to do it ourselves.  Whether we succeed
			 * or not, we start over.
			 */
			if (*next_entry == index) {
				InterlockedCompareExchange (next_entry, index + 1, index);
				//g_print ("tried increment for other thread\n");
				HEAVY_STAT (++stat_increment_other_thread);
			}
			continue;
		}
		/* state is BUSY now */
		mono_memory_write_barrier ();
		/*
		 * Incrementing `next_entry` must happen after setting the state to `BUSY`.
		 * If it were the other way around, it would be possible that after a filler
		 * incremented the index, other threads fill up the queue, the queue is
		 * drained, the original filler finally fills in the slot, but `next_entry`
		 * ends up at the start of the queue, and new entries are written in the
		 * queue in front of, not behind, the original filler's entry.
		 *
		 * We don't actually require that the CAS succeeds, but we do require that
		 * the value of `next_entry` is not lower than our index.  Since the drainer
		 * sets it to `-1`, that also takes care of the case that the drainer is
		 * currently running.
		 */
		old_next_entry = InterlockedCompareExchange (next_entry, index + 1, index);
		if (old_next_entry < index) {
			/* BUSY -> FREE */
			/* INVALID -> FREE */
			/*
			 * The state might still be `BUSY`, or the drainer could have set it
			 * to `INVALID`.  In either case, there's no point in CASing.  Set
			 * it to `FREE` and start over.
			 */
			entries [index].state = STAGE_ENTRY_FREE;
			HEAVY_STAT (++stat_index_decremented);
			continue;
		}
		break;
	}

	SGEN_ASSERT (0, index >= 0 && index < num_entries, "Invalid index");

	entries [index].obj = obj;
	entries [index].user_data = user_data;

	mono_memory_write_barrier ();

	new_next_entry = *next_entry;
	mono_memory_read_barrier ();
	/* BUSY -> USED */
	/*
	 * A `BUSY` entry will either still be `BUSY` or the drainer will have set it to
	 * `INVALID`.  In the former case, we set it to `USED` and we're finished.  In the
	 * latter case, we reset it to `FREE` and start over.
	 */
	previous_state = InterlockedCompareExchange (&entries [index].state, STAGE_ENTRY_USED, STAGE_ENTRY_BUSY);
	if (previous_state == STAGE_ENTRY_BUSY) {
		SGEN_ASSERT (0, new_next_entry >= index || new_next_entry < 0, "Invalid next entry index - as long as we're busy, other thread can only increment or invalidate it");
		HEAVY_STAT (++stat_success);
		return index;
	}

	SGEN_ASSERT (0, previous_state == STAGE_ENTRY_INVALID, "Invalid state transition - other thread can only make busy state invalid");
	entries [index].obj = NULL;
	entries [index].user_data = NULL;
	mono_memory_write_barrier ();
	/* INVALID -> FREE */
	entries [index].state = STAGE_ENTRY_FREE;

	HEAVY_STAT (++stat_entry_invalidated);

	goto retry;
}

/* LOCKING: requires that the GC lock is held */
static void
process_fin_stage_entry (GCObject *obj, void *user_data, int index)
{
	if (ptr_in_nursery (obj))
		register_for_finalization (obj, user_data, GENERATION_NURSERY);
	else
		register_for_finalization (obj, user_data, GENERATION_OLD);
}

/* LOCKING: requires that the GC lock is held */
void
sgen_process_fin_stage_entries (void)
{
	lock_stage_for_processing (&next_fin_stage_entry);
	process_stage_entries (NUM_FIN_STAGE_ENTRIES, &next_fin_stage_entry, fin_stage_entries, process_fin_stage_entry);
}

void
sgen_object_register_for_finalization (GCObject *obj, void *user_data)
{
	while (add_stage_entry (NUM_FIN_STAGE_ENTRIES, &next_fin_stage_entry, fin_stage_entries, obj, user_data) == -1) {
		if (try_lock_stage_for_processing (NUM_FIN_STAGE_ENTRIES, &next_fin_stage_entry)) {
			LOCK_GC;
			process_stage_entries (NUM_FIN_STAGE_ENTRIES, &next_fin_stage_entry, fin_stage_entries, process_fin_stage_entry);
			UNLOCK_GC;
		}
	}
}

/* LOCKING: requires that the GC lock is held */
static int
finalizers_with_predicate (SgenObjectPredicateFunc predicate, void *user_data, GCObject **out_array, int out_size, SgenHashTable *hash_table)
{
	GCObject *object;
	gpointer dummy G_GNUC_UNUSED;
	int count;

	if (no_finalize || !out_size || !out_array)
		return 0;
	count = 0;
	SGEN_HASH_TABLE_FOREACH (hash_table, object, dummy) {
		object = tagged_object_get_object (object);

		if (predicate (object, user_data)) {
			/* remove and put in out_array */
			SGEN_HASH_TABLE_FOREACH_REMOVE (TRUE);
			out_array [count ++] = object;
			SGEN_LOG (5, "Collecting object for finalization: %p (%s) (%d)", object, sgen_client_vtable_get_name (SGEN_LOAD_VTABLE (object)), sgen_hash_table_num_entries (hash_table));
			if (count == out_size)
				return count;
			continue;
		}
	} SGEN_HASH_TABLE_FOREACH_END;
	return count;
}

/**
 * sgen_gather_finalizers_if:
 * @predicate: predicate function
 * @user_data: predicate function data argument
 * @out_array: output array
 * @out_size: size of output array
 *
 * Store inside @out_array up to @out_size objects that match @predicate. Returns the number
 * of stored items. Can be called repeteadly until it returns 0.
 *
 * The items are removed from the finalizer data structure, so the caller is supposed
 * to finalize them.
 *
 * @out_array me be on the stack, or registered as a root, to allow the GC to know the
 * objects are still alive.
 */
int
sgen_gather_finalizers_if (SgenObjectPredicateFunc predicate, void *user_data, GCObject **out_array, int out_size)
{
	int result;

	LOCK_GC;
	sgen_process_fin_stage_entries ();
	result = finalizers_with_predicate (predicate, user_data, (GCObject**)out_array, out_size, &minor_finalizable_hash);
	if (result < out_size) {
		result += finalizers_with_predicate (predicate, user_data, (GCObject**)out_array + result, out_size - result,
			&major_finalizable_hash);
	}
	UNLOCK_GC;

	return result;
}

void
sgen_remove_finalizers_if (SgenObjectPredicateFunc predicate, void *user_data, int generation)
{
	SgenHashTable *hash_table = get_finalize_entry_hash_table (generation);
	GCObject *object;
	gpointer dummy G_GNUC_UNUSED;

	SGEN_HASH_TABLE_FOREACH (hash_table, object, dummy) {
		object = tagged_object_get_object (object);

		if (predicate (object, user_data)) {
			SGEN_HASH_TABLE_FOREACH_REMOVE (TRUE);
			continue;
		}
	} SGEN_HASH_TABLE_FOREACH_END;	
}

/* GC Handles */

#ifdef HEAVY_STATISTICS
static volatile guint64 stat_gc_handles_allocated = 0;
static volatile guint64 stat_gc_handles_max_allocated = 0;
#endif

#define BUCKETS (32 - MONO_GC_HANDLE_TYPE_SHIFT)
#define MIN_BUCKET_BITS (5)
#define MIN_BUCKET_SIZE (1 << MIN_BUCKET_BITS)

/*
 * A table of GC handle data, implementing a simple lock-free bitmap allocator.
 *
 * 'entries' is an array of pointers to buckets of increasing size. The first
 * bucket has size 'MIN_BUCKET_SIZE', and each bucket is twice the size of the
 * previous, i.e.:
 *
 *           |-------|-- MIN_BUCKET_SIZE
 *    [0] -> xxxxxxxx
 *    [1] -> xxxxxxxxxxxxxxxx
 *    [2] -> xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
 *    ...
 *
 * The size of the spine, 'BUCKETS', is chosen so that the maximum number of
 * entries is no less than the maximum index value of a GC handle.
 *
 * Each entry in a bucket is a pointer with two tag bits: if
 * 'GC_HANDLE_OCCUPIED' returns true for a slot, then the slot is occupied; if
 * so, then 'GC_HANDLE_VALID' gives whether the entry refers to a valid (1) or
 * NULL (0) object reference. If the reference is valid, then the pointer is an
 * object pointer. If the reference is NULL, and 'GC_HANDLE_TYPE_IS_WEAK' is
 * true for 'type', then the pointer is a domain pointer--this allows us to
 * retrieve the domain ID of an expired weak reference.
 *
 * Finally, 'slot_hint' denotes the position of the last allocation, so that the
 * whole array needn't be searched on every allocation.
 */

typedef struct {
	volatile gpointer *volatile entries [BUCKETS];
	volatile guint32 capacity;
	volatile guint32 slot_hint;
	guint8 type;
} HandleData;

static inline guint
bucket_size (guint index)
{
	return 1 << (index + MIN_BUCKET_BITS);
}

/* Computes floor(log2(index + MIN_BUCKET_SIZE)) - 1, giving the index
 * of the bucket containing a slot.
 */
static inline guint
index_bucket (guint index)
{
#ifdef __GNUC__
	return CHAR_BIT * sizeof (index) - __builtin_clz (index + MIN_BUCKET_SIZE) - 1 - MIN_BUCKET_BITS;
#else
	guint count = 0;
	index += MIN_BUCKET_SIZE;
	while (index) {
		++count;
		index >>= 1;
	}
	return count - 1 - MIN_BUCKET_BITS;
#endif
}

static inline void
bucketize (guint index, guint *bucket, guint *offset)
{
	*bucket = index_bucket (index);
	*offset = index - bucket_size (*bucket) + MIN_BUCKET_SIZE;
}

static inline gboolean
try_set_slot (volatile gpointer *slot, MonoObject *obj, gpointer old, GCHandleType type)
{
    if (obj)
		return InterlockedCompareExchangePointer (slot, MONO_GC_HANDLE_OBJECT_POINTER (obj, GC_HANDLE_TYPE_IS_WEAK (type)), old) == old;
    return InterlockedCompareExchangePointer (slot, MONO_GC_HANDLE_DOMAIN_POINTER (mono_domain_get (), GC_HANDLE_TYPE_IS_WEAK (type)), old) == old;
}

/* Try to claim a slot by setting its occupied bit. */
static inline gboolean
try_occupy_slot (HandleData *handles, guint bucket, guint offset, MonoObject *obj, gboolean track)
{
	volatile gpointer *link_addr = &(handles->entries [bucket] [offset]);
	if (MONO_GC_HANDLE_OCCUPIED (*link_addr))
		return FALSE;
	return try_set_slot (link_addr, obj, NULL, handles->type);
}

#define EMPTY_HANDLE_DATA(type) { { NULL }, 0, 0, (type) }

/* weak and weak-track arrays will be allocated in malloc memory 
 */
static HandleData gc_handles [] = {
	EMPTY_HANDLE_DATA (HANDLE_WEAK),
	EMPTY_HANDLE_DATA (HANDLE_WEAK_TRACK),
	EMPTY_HANDLE_DATA (HANDLE_NORMAL),
	EMPTY_HANDLE_DATA (HANDLE_PINNED)
};

static HandleData *
gc_handles_for_type (GCHandleType type)
{
	g_assert (type < HANDLE_TYPE_MAX);
	return &gc_handles [type];
}

/* This assumes that the world is stopped. */
void
sgen_mark_normal_gc_handles (void *addr, SgenUserMarkFunc mark_func, void *gc_data)
{
	HandleData *handles = gc_handles_for_type (HANDLE_NORMAL);
	size_t bucket, offset;
	const guint max_bucket = index_bucket (handles->capacity);
	for (bucket = 0; bucket < max_bucket; ++bucket) {
		volatile gpointer *entries = handles->entries [bucket];
		for (offset = 0; offset < bucket_size (bucket); ++offset) {
			volatile gpointer *entry = &entries [offset];
			gpointer hidden = *entry;
			gpointer revealed = MONO_GC_REVEAL_POINTER (hidden, FALSE);
			if (!MONO_GC_HANDLE_IS_OBJECT_POINTER (hidden))
				continue;
			mark_func ((MonoObject **)&revealed, gc_data);
			g_assert (revealed);
			*entry = MONO_GC_HANDLE_OBJECT_POINTER (revealed, FALSE);
		}
	}
}

static guint
handle_data_find_unset (HandleData *handles, guint32 begin, guint32 end)
{
	guint index;
	gint delta = begin < end ? +1 : -1;
	for (index = begin; index < end; index += delta) {
		guint bucket, offset;
		volatile gpointer *entries;
		bucketize (index, &bucket, &offset);
		entries = handles->entries [bucket];
		g_assert (entries);
		if (!MONO_GC_HANDLE_OCCUPIED (entries [offset]))
			return index;
	}
	return -1;
}

/* Adds a bucket if necessary and possible. */
static void
handle_data_grow (HandleData *handles, guint32 old_capacity)
{
	const guint new_bucket = index_bucket (old_capacity);
	const guint32 growth = bucket_size (new_bucket);
	const guint32 new_capacity = old_capacity + growth;
	gpointer *entries;
	const size_t new_bucket_size = sizeof (**handles->entries) * growth;
	if (handles->capacity >= new_capacity)
		return;
	entries = g_malloc0 (new_bucket_size);
	if (handles->type == HANDLE_PINNED)
		sgen_register_root ((char *)entries, new_bucket_size, SGEN_DESCRIPTOR_NULL, ROOT_TYPE_PINNED, MONO_ROOT_SOURCE_GC_HANDLE, "pinned gc handles");
	if (InterlockedCompareExchangePointer ((volatile gpointer *)&handles->entries [new_bucket], entries, NULL) == NULL) {
		if (InterlockedCompareExchange ((volatile gint32 *)&handles->capacity, new_capacity, old_capacity) != old_capacity)
			g_assert_not_reached ();
		handles->slot_hint = old_capacity;
		mono_memory_write_barrier ();
		return;
	}
	/* Someone beat us to the allocation. */
	if (handles->type == HANDLE_PINNED)
		sgen_deregister_root ((char *)entries);
	g_free (entries);
}

static guint32
alloc_handle (HandleData *handles, MonoObject *obj, gboolean track)
{
	guint index;
	guint32 res;
	guint bucket, offset;
	guint32 capacity;
	guint32 slot_hint;
	if (!handles->capacity)
		handle_data_grow (handles, 0);
retry:
	capacity = handles->capacity;
	slot_hint = handles->slot_hint;
	index = handle_data_find_unset (handles, slot_hint, capacity);
	if (index == -1)
		index = handle_data_find_unset (handles, 0, slot_hint);
	if (index == -1) {
		handle_data_grow (handles, capacity);
		goto retry;
	}
	handles->slot_hint = index;
	bucketize (index, &bucket, &offset);
	if (!try_occupy_slot (handles, bucket, offset, obj, track))
		goto retry;
#ifdef HEAVY_STATISTICS
	InterlockedIncrement64 ((volatile gint64 *)&stat_gc_handles_allocated);
	if (stat_gc_handles_allocated > stat_gc_handles_max_allocated)
		stat_gc_handles_max_allocated = stat_gc_handles_allocated;
#endif
	if (obj && MONO_GC_HANDLE_TYPE_IS_WEAK (handles->type))
		binary_protocol_dislink_add ((gpointer)&handles->entries [bucket] [offset], obj, track);
	/* Ensure that a GC handle cannot be given to another thread without the slot having been set. */
	mono_memory_write_barrier ();
#ifndef DISABLE_PERFCOUNTERS
	mono_perfcounters->gc_num_handles++;
#endif
	res = MONO_GC_HANDLE (index, handles->type);
	mono_profiler_gc_handle (MONO_PROFILER_GC_HANDLE_CREATED, handles->type, res, obj);
	return res;
}

static gboolean
object_older_than (GCObject *object, int generation)
{
	return generation == GENERATION_NURSERY && !sgen_ptr_in_nursery (object);
}

/*
 * Maps a function over all GC handles.
 * This assumes that the world is stopped!
 */
static void
sgen_gchandle_iterate (GCHandleType handle_type, int max_generation, gpointer callback(gpointer, GCHandleType, int, gpointer), gpointer user)
{
	HandleData *handle_data = gc_handles_for_type (handle_type);
	size_t bucket, offset;
	guint max_bucket = index_bucket (handle_data->capacity);
	/* If a new bucket has been allocated, but the capacity has not yet been
	 * increased, nothing can yet have been allocated in the bucket because the
	 * world is stopped, so we shouldn't miss any handles during iteration.
	 */
	for (bucket = 0; bucket < max_bucket; ++bucket) {
		volatile gpointer *entries = handle_data->entries [bucket];
		for (offset = 0; offset < bucket_size (bucket); ++offset) {
			gpointer hidden = entries [offset];
			gpointer result;
			/* Table must contain no garbage pointers. */
			gboolean occupied = MONO_GC_HANDLE_OCCUPIED (hidden);
			g_assert (hidden ? occupied : !occupied);
			if (!occupied) // || !MONO_GC_HANDLE_VALID (hidden))
				continue;
			result = callback (hidden, handle_type, max_generation, user);
			if (result) {
				SGEN_ASSERT (0, MONO_GC_HANDLE_OCCUPIED (result), "Why did the callback return an unoccupied entry?");
				// FIXME: add the dislink_update protocol call here
			} else {
				HEAVY_STAT (InterlockedDecrement64 ((volatile gint64 *)&stat_gc_handles_allocated));
			}
			entries [offset] = result;
		}
	}
}

/**
 * mono_gchandle_new:
 * @obj: managed object to get a handle for
 * @pinned: whether the object should be pinned
 *
 * This returns a handle that wraps the object, this is used to keep a
 * reference to a managed object from the unmanaged world and preventing the
 * object from being disposed.
 * 
 * If @pinned is false the address of the object can not be obtained, if it is
 * true the address of the object can be obtained.  This will also pin the
 * object so it will not be possible by a moving garbage collector to move the
 * object. 
 * 
 * Returns: a handle that can be used to access the object from
 * unmanaged code.
 */
guint32
mono_gchandle_new (MonoObject *obj, gboolean pinned)
{
	return alloc_handle (gc_handles_for_type (pinned ? HANDLE_PINNED : HANDLE_NORMAL), obj, FALSE);
}

/**
 * mono_gchandle_new_weakref:
 * @obj: managed object to get a handle for
 * @pinned: whether the object should be pinned
 *
 * This returns a weak handle that wraps the object, this is used to
 * keep a reference to a managed object from the unmanaged world.
 * Unlike the mono_gchandle_new the object can be reclaimed by the
 * garbage collector.  In this case the value of the GCHandle will be
 * set to zero.
 * 
 * If @pinned is false the address of the object can not be obtained, if it is
 * true the address of the object can be obtained.  This will also pin the
 * object so it will not be possible by a moving garbage collector to move the
 * object. 
 * 
 * Returns: a handle that can be used to access the object from
 * unmanaged code.
 */
guint32
mono_gchandle_new_weakref (MonoObject *obj, gboolean track_resurrection)
{
	return alloc_handle (gc_handles_for_type (track_resurrection ? HANDLE_WEAK_TRACK : HANDLE_WEAK), obj, track_resurrection);
}

static void
ensure_weak_links_accessible (void)
{
	/*
	 * During the second bridge processing step the world is
	 * running again.  That step processes all weak links once
	 * more to null those that refer to dead objects.  Before that
	 * is completed, those links must not be followed, so we
	 * conservatively wait for bridge processing when any weak
	 * link is dereferenced.
	 */
	/* FIXME: A GC can occur after this check fails, in which case we
	 * should wait for bridge processing but would fail to do so.
	 */
	if (G_UNLIKELY (bridge_processing_in_progress))
		mono_gc_wait_for_bridge_processing ();
}

static MonoObject *
link_get (volatile gpointer *link_addr, gboolean is_weak)
{
	void *volatile *link_addr_volatile;
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
	if (ptr && MONO_GC_HANDLE_IS_OBJECT_POINTER (ptr))
		obj = (MonoObject *)MONO_GC_REVEAL_POINTER (ptr, is_weak);
	else
		return NULL;

	/* Note [dummy use]:
	 *
	 * If a GC happens here, obj needs to be on the stack or in a
	 * register, so we need to prevent this from being reordered
	 * wrt the check.
	 */
	mono_gc_dummy_use (obj);
	mono_memory_barrier ();

	if (is_weak)
		ensure_weak_links_accessible ();

	if ((void*)*link_addr_volatile != ptr)
		goto retry;

	return obj;
}

/**
 * mono_gchandle_get_target:
 * @gchandle: a GCHandle's handle.
 *
 * The handle was previously created by calling mono_gchandle_new or
 * mono_gchandle_new_weakref. 
 *
 * Returns a pointer to the MonoObject represented by the handle or
 * NULL for a collected object if using a weakref handle.
 */
MonoObject*
mono_gchandle_get_target (guint32 gchandle)
{
	guint index = MONO_GC_HANDLE_SLOT (gchandle);
	guint type = MONO_GC_HANDLE_TYPE (gchandle);
	HandleData *handles = gc_handles_for_type (type);
	guint bucket, offset;
	g_assert (index < handles->capacity);
	bucketize (index, &bucket, &offset);
	return link_get (&handles->entries [bucket] [offset], MONO_GC_HANDLE_TYPE_IS_WEAK (type));
}

void
sgen_gchandle_set_target (guint32 gchandle, GCObject *obj)
{
	guint index = MONO_GC_HANDLE_SLOT (gchandle);
	guint type = MONO_GC_HANDLE_TYPE (gchandle);
	HandleData *handles = gc_handles_for_type (type);
	gboolean track = handles->type == HANDLE_WEAK_TRACK;
	guint bucket, offset;
	gpointer slot;

	g_assert (index < handles->capacity);
	bucketize (index, &bucket, &offset);

retry:
	slot = handles->entries [bucket] [offset];
	g_assert (MONO_GC_HANDLE_OCCUPIED (slot));
	if (!try_set_slot (&handles->entries [bucket] [offset], obj, slot, MONO_GC_HANDLE_TYPE_IS_WEAK (handles->type)))
		goto retry;
	if (MONO_GC_HANDLE_IS_OBJECT_POINTER (slot))
		binary_protocol_dislink_remove ((gpointer)&handles->entries [bucket] [offset], track);
	if (obj)
		binary_protocol_dislink_add ((gpointer)&handles->entries [bucket] [offset], obj, track);
}

static MonoDomain *
mono_gchandle_slot_domain (volatile gpointer *slot_addr, gboolean is_weak)
{
	gpointer slot;
	MonoDomain *domain;
retry:
	slot = *slot_addr;
	if (!MONO_GC_HANDLE_OCCUPIED (slot))
		return NULL;
	if (MONO_GC_HANDLE_IS_OBJECT_POINTER (slot)) {
		MonoObject *obj = MONO_GC_REVEAL_POINTER (slot, is_weak);
		/* See note [dummy use]. */
		mono_gc_dummy_use (obj);
		if (*slot_addr != slot)
			goto retry;
		return mono_object_domain (obj);
	}
	domain = MONO_GC_REVEAL_POINTER (slot, is_weak);
	/* See note [dummy use]. */
	mono_gc_dummy_use (domain);
	if (*slot_addr != slot)
		goto retry;
	return domain;
}

static MonoDomain *
gchandle_domain (guint32 gchandle) {
	guint index = MONO_GC_HANDLE_SLOT (gchandle);
	guint type = MONO_GC_HANDLE_TYPE (gchandle);
	HandleData *handles = gc_handles_for_type (type);
	guint bucket, offset;
	if (index >= handles->capacity)
		return NULL;
	bucketize (index, &bucket, &offset);
	return mono_gchandle_slot_domain (&handles->entries [bucket] [offset], MONO_GC_HANDLE_TYPE_IS_WEAK (type));
}

/**
 * mono_gchandle_is_in_domain:
 * @gchandle: a GCHandle's handle.
 * @domain: An application domain.
 *
 * Returns: true if the object wrapped by the @gchandle belongs to the specific @domain.
 */
gboolean
mono_gchandle_is_in_domain (guint32 gchandle, MonoDomain *domain)
{
	return domain->domain_id == gchandle_domain (gchandle)->domain_id;
}

/**
 * mono_gchandle_free:
 * @gchandle: a GCHandle's handle.
 *
 * Frees the @gchandle handle.  If there are no outstanding
 * references, the garbage collector can reclaim the memory of the
 * object wrapped. 
 */
void
mono_gchandle_free (guint32 gchandle)
{
	guint index = MONO_GC_HANDLE_SLOT (gchandle);
	guint type = MONO_GC_HANDLE_TYPE (gchandle);
	HandleData *handles = gc_handles_for_type (type);
	guint bucket, offset;
	bucketize (index, &bucket, &offset);
	if (index < handles->capacity && MONO_GC_HANDLE_OCCUPIED (handles->entries [bucket] [offset])) {
		if (MONO_GC_HANDLE_TYPE_IS_WEAK (handles->type))
			binary_protocol_dislink_remove ((gpointer)&handles->entries [bucket] [offset], handles->type == HANDLE_WEAK_TRACK);
		handles->entries [bucket] [offset] = NULL;
		HEAVY_STAT (InterlockedDecrement64 ((volatile gint64 *)&stat_gc_handles_allocated));
	} else {
		/* print a warning? */
	}
#ifndef DISABLE_PERFCOUNTERS
	mono_perfcounters->gc_num_handles--;
#endif
	mono_profiler_gc_handle (MONO_PROFILER_GC_HANDLE_DESTROYED, handles->type, gchandle, NULL);
}

/**
 * mono_gchandle_free_domain:
 * @unloading: domain that is unloading
 *
 * Function used internally to cleanup any GC handle for objects belonging
 * to the specified domain during appdomain unload.
 */
void
mono_gchandle_free_domain (MonoDomain *unloading)
{
	guint type;
	/* All non-pinned handle types. */
	for (type = HANDLE_TYPE_MIN; type < HANDLE_PINNED; ++type) {
		const gboolean is_weak = MONO_GC_HANDLE_TYPE_IS_WEAK (type);
		guint index;
		HandleData *handles = gc_handles_for_type (type);
		guint32 capacity = handles->capacity;
		for (index = 0; index < capacity; ++index) {
			guint bucket, offset;
			gpointer slot;
			bucketize (index, &bucket, &offset);
			MonoObject *obj = NULL;
			MonoDomain *domain;
			volatile gpointer *slot_addr = &handles->entries [bucket] [offset];
			/* NB: This should have the same behavior as mono_gchandle_slot_domain(). */
		retry:
			slot = *slot_addr;
			if (!MONO_GC_HANDLE_OCCUPIED (slot))
				continue;
			if (MONO_GC_HANDLE_IS_OBJECT_POINTER (slot)) {
				obj = MONO_GC_REVEAL_POINTER (slot, is_weak);
				if (*slot_addr != slot)
					goto retry;
				domain = mono_object_domain (obj);
			} else {
				domain = MONO_GC_REVEAL_POINTER (slot, is_weak);
			}
			if (unloading->domain_id == domain->domain_id) {
				if (MONO_GC_HANDLE_TYPE_IS_WEAK (type) && MONO_GC_REVEAL_POINTER (slot, is_weak))
					binary_protocol_dislink_remove ((gpointer)&handles->entries [bucket] [offset], handles->type == HANDLE_WEAK_TRACK);
				*slot_addr = NULL;
				HEAVY_STAT (InterlockedDecrement64 ((volatile gint64 *)&stat_gc_handles_allocated));
			}
			/* See note [dummy use]. */
			mono_gc_dummy_use (obj);
		}
	}

}

/*
 * Returns whether to remove the link from its hash.
 */
static gpointer
null_link_if_necessary (gpointer hidden, GCHandleType handle_type, int max_generation, gpointer user)
{
	const gboolean is_weak = GC_HANDLE_TYPE_IS_WEAK (handle_type);
	ScanCopyContext *ctx = (ScanCopyContext *)user;
	GCObject *obj;
	GCObject *copy;

	if (!MONO_GC_HANDLE_VALID (hidden))
		return hidden;

	obj = MONO_GC_REVEAL_POINTER (hidden, MONO_GC_HANDLE_TYPE_IS_WEAK (handle_type));
	SGEN_ASSERT (0, obj, "Why is the hidden pointer NULL?");

	if (object_older_than (obj, max_generation))
		return hidden;

	if (major_collector.is_object_live (obj))
		return hidden;

	/* Clear link if object is ready for finalization. This check may be redundant wrt is_object_live(). */
	if (sgen_gc_is_object_ready_for_finalization (obj))
		return MONO_GC_HANDLE_DOMAIN_POINTER (mono_object_domain (obj), is_weak);

	ctx->ops->copy_or_mark_object (&copy, ctx->queue);
	g_assert (copy);
	/* binary_protocol_dislink_update (hidden_entry, copy, handle_type == HANDLE_WEAK_TRACK); */

	copy = obj;
	ctx->ops->copy_or_mark_object (&copy, ctx->queue);
	SGEN_ASSERT (0, copy, "Why couldn't we copy the object?");
	/* Update link if object was moved. */
	return MONO_GC_HANDLE_OBJECT_POINTER (copy, is_weak);
}

/* LOCKING: requires that the GC lock is held */
void
sgen_null_link_in_range (int generation, ScanCopyContext ctx, gboolean track)
{
	sgen_gchandle_iterate (track ? HANDLE_WEAK_TRACK : HANDLE_WEAK, generation, null_link_if_necessary, &ctx);
}

typedef struct {
	SgenObjectPredicateFunc predicate;
	gpointer data;
} WeakLinkAlivePredicateClosure;

static gpointer
null_link_if (gpointer hidden, GCHandleType handle_type, int max_generation, gpointer user)
{
	/* Strictly speaking, function pointers are not guaranteed to have the same size as data pointers. */
	WeakLinkAlivePredicateClosure *closure = (WeakLinkAlivePredicateClosure *)user;
	GCObject *obj;

	if (!MONO_GC_HANDLE_VALID (hidden))
		return hidden;

	obj = MONO_GC_REVEAL_POINTER (hidden, MONO_GC_HANDLE_TYPE_IS_WEAK (handle_type));
	SGEN_ASSERT (0, obj, "Why is the hidden pointer NULL?");

	if (object_older_than (obj, max_generation))
		return hidden;

	if (closure->predicate (obj, closure->data))
		return NULL;

	return hidden;
}

/* LOCKING: requires that the GC lock is held */
void
sgen_null_links_if (SgenObjectPredicateFunc predicate, void *data, int generation, gboolean track)
{
	WeakLinkAlivePredicateClosure closure = { predicate, data };
	sgen_gchandle_iterate (track ? HANDLE_WEAK_TRACK : HANDLE_WEAK, generation, null_link_if, &closure);
}

void
sgen_init_fin_weak_hash (void)
{
#ifdef HEAVY_STATISTICS
	mono_counters_register ("FinWeak Successes", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_success);
	mono_counters_register ("FinWeak Overflow aborts", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_overflow_abort);
	mono_counters_register ("FinWeak Wait for processing", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_wait_for_processing);
	mono_counters_register ("FinWeak Increment other thread", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_increment_other_thread);
	mono_counters_register ("FinWeak Index decremented", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_index_decremented);
	mono_counters_register ("FinWeak Entry invalidated", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_entry_invalidated);

	mono_counters_register ("GC handles allocated", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_gc_handles_allocated);
	mono_counters_register ("max GC handles allocated", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_gc_handles_max_allocated);
#endif
}

#endif /* HAVE_SGEN_GC */
