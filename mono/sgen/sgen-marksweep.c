/*
 * sgen-marksweep.c: The Mark & Sweep major collector.
 *
 * Author:
 * 	Mark Probst <mark.probst@gmail.com>
 *
 * Copyright 2009-2010 Novell, Inc.
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

#include <math.h>
#include <errno.h>
#include <string.h>
#include <stdlib.h>

#include "mono/sgen/sgen-gc.h"
#include "mono/sgen/sgen-protocol.h"
#include "mono/sgen/sgen-cardtable.h"
#include "mono/sgen/sgen-memory-governor.h"
#include "mono/sgen/sgen-layout-stats.h"
#include "mono/sgen/sgen-pointer-queue.h"
#include "mono/sgen/sgen-pinning.h"
#include "mono/sgen/sgen-workers.h"
#include "mono/sgen/sgen-thread-pool.h"
#include "mono/sgen/sgen-client.h"
#include "mono/utils/mono-membar.h"

#if defined(ARCH_MIN_MS_BLOCK_SIZE) && defined(ARCH_MIN_MS_BLOCK_SIZE_SHIFT)
#define MS_BLOCK_SIZE	ARCH_MIN_MS_BLOCK_SIZE
#define MS_BLOCK_SIZE_SHIFT	ARCH_MIN_MS_BLOCK_SIZE_SHIFT
#else
#define MS_BLOCK_SIZE_SHIFT     14      /* INT FASTENABLE */
#define MS_BLOCK_SIZE           (1 << MS_BLOCK_SIZE_SHIFT)
#endif
#define MAJOR_SECTION_SIZE	MS_BLOCK_SIZE
#define CARDS_PER_BLOCK (MS_BLOCK_SIZE / CARD_SIZE_IN_BYTES)

/*
 * Don't allocate single blocks, but alloc a contingent of this many
 * blocks in one swoop.  This must be a power of two.
 */
#define MS_BLOCK_ALLOC_NUM	32

/*
 * Number of bytes before the first object in a block.  At the start
 * of a block is the MSBlockHeader, then opional padding, then come
 * the objects, so this must be >= sizeof (MSBlockHeader).
 */
#define MS_BLOCK_SKIP	((sizeof (MSBlockHeader) + 15) & ~15)

#define MS_BLOCK_FREE	(MS_BLOCK_SIZE - MS_BLOCK_SKIP)

#define MS_NUM_MARK_WORDS	((MS_BLOCK_SIZE / SGEN_ALLOC_ALIGN + sizeof (mword) * 8 - 1) / (sizeof (mword) * 8))

/*
 * Blocks progress from one state to the next:
 *
 * SWEPT           The block is fully swept.  It might or might not be in
 *                 a free list.
 *
 * MARKING         The block might or might not contain live objects.  If
 *                 we're in between an initial collection pause and the
 *                 finishing pause, the block might or might not be in a
 *                 free list.
 *
 * CHECKING        The sweep thread is investigating the block to determine
 *                 whether or not it contains live objects.  The block is
 *                 not in a free list.
 *
 * NEED_SWEEPING   The block contains live objects but has not yet been
 *                 swept.  It also contains free slots.  It is in a block
 *                 free list.
 *
 * SWEEPING        The block is being swept.  It might be in a free list.
 */

enum {
	BLOCK_STATE_SWEPT,
	BLOCK_STATE_MARKING,
	BLOCK_STATE_CHECKING,
	BLOCK_STATE_NEED_SWEEPING,
	BLOCK_STATE_SWEEPING
};

typedef struct _MSBlockInfo MSBlockInfo;
struct _MSBlockInfo {
	guint16 obj_size;
	/*
	 * FIXME: Do we even need this? It's only used during sweep and might be worth
	 * recalculating to save the space.
	 */
	guint16 obj_size_index;
	/* FIXME: Reduce this - it only needs a byte. */
	volatile gint32 state;
	unsigned int pinned : 1;
	unsigned int has_references : 1;
	unsigned int has_pinned : 1;	/* means cannot evacuate */
	unsigned int is_to_space : 1;
	void ** volatile free_list;
	MSBlockInfo * volatile next_free;
	guint8 * volatile cardtable_mod_union;
	mword mark_words [MS_NUM_MARK_WORDS];
};

#define MS_BLOCK_FOR_BLOCK_INFO(b)	((char*)(b))

#define MS_BLOCK_OBJ(b,i)		((GCObject *)(MS_BLOCK_FOR_BLOCK_INFO(b) + MS_BLOCK_SKIP + (b)->obj_size * (i)))
#define MS_BLOCK_OBJ_FOR_SIZE(b,i,obj_size)		(MS_BLOCK_FOR_BLOCK_INFO(b) + MS_BLOCK_SKIP + (obj_size) * (i))
#define MS_BLOCK_DATA_FOR_OBJ(o)	((char*)((mword)(o) & ~(mword)(MS_BLOCK_SIZE - 1)))

typedef struct {
	MSBlockInfo info;
} MSBlockHeader;

#define MS_BLOCK_FOR_OBJ(o)		(&((MSBlockHeader*)MS_BLOCK_DATA_FOR_OBJ ((o)))->info)

/* object index will always be small */
#define MS_BLOCK_OBJ_INDEX(o,b)	((int)(((char*)(o) - (MS_BLOCK_FOR_BLOCK_INFO(b) + MS_BLOCK_SKIP)) / (b)->obj_size))

//casting to int is fine since blocks are 32k
#define MS_CALC_MARK_BIT(w,b,o) 	do {				\
		int i = ((int)((char*)(o) - MS_BLOCK_DATA_FOR_OBJ ((o)))) >> SGEN_ALLOC_ALIGN_BITS; \
		if (sizeof (mword) == 4) {				\
			(w) = i >> 5;					\
			(b) = i & 31;					\
		} else {						\
			(w) = i >> 6;					\
			(b) = i & 63;					\
		}							\
	} while (0)

#define MS_MARK_BIT(bl,w,b)	((bl)->mark_words [(w)] & (ONE_P << (b)))
#define MS_SET_MARK_BIT(bl,w,b)	((bl)->mark_words [(w)] |= (ONE_P << (b)))

#define MS_OBJ_ALLOCED(o,b)	(*(void**)(o) && (*(char**)(o) < MS_BLOCK_FOR_BLOCK_INFO (b) || *(char**)(o) >= MS_BLOCK_FOR_BLOCK_INFO (b) + MS_BLOCK_SIZE))

#define MS_BLOCK_OBJ_SIZE_FACTOR	(pow (2.0, 1.0 / 3))

/*
 * This way we can lookup block object size indexes for sizes up to
 * 256 bytes with a single load.
 */
#define MS_NUM_FAST_BLOCK_OBJ_SIZE_INDEXES	32

static int *block_obj_sizes;
static int num_block_obj_sizes;
static int fast_block_obj_size_indexes [MS_NUM_FAST_BLOCK_OBJ_SIZE_INDEXES];

#define MS_BLOCK_FLAG_PINNED	1
#define MS_BLOCK_FLAG_REFS	2

#define MS_BLOCK_TYPE_MAX	4

static gboolean *evacuate_block_obj_sizes;
static float evacuation_threshold = 0.666f;

static gboolean lazy_sweep = FALSE;

enum {
	SWEEP_STATE_SWEPT,
	SWEEP_STATE_NEED_SWEEPING,
	SWEEP_STATE_SWEEPING,
	SWEEP_STATE_SWEEPING_AND_ITERATING,
	SWEEP_STATE_COMPACTING
};

static volatile int sweep_state = SWEEP_STATE_SWEPT;

static gboolean concurrent_mark;
static gboolean concurrent_sweep = TRUE;

#define BLOCK_IS_TAGGED_HAS_REFERENCES(bl)	SGEN_POINTER_IS_TAGGED_1 ((bl))
#define BLOCK_TAG_HAS_REFERENCES(bl)		SGEN_POINTER_TAG_1 ((bl))

#define BLOCK_IS_TAGGED_CHECKING(bl)		SGEN_POINTER_IS_TAGGED_2 ((bl))
#define BLOCK_TAG_CHECKING(bl)			SGEN_POINTER_TAG_2 ((bl))

#define BLOCK_UNTAG(bl)				((MSBlockInfo *)SGEN_POINTER_UNTAG_12 ((bl)))

#define BLOCK_TAG(bl)				((bl)->has_references ? BLOCK_TAG_HAS_REFERENCES ((bl)) : (bl))

/* all allocated blocks in the system */
static SgenPointerQueue allocated_blocks;

/* non-allocated block free-list */
static void *empty_blocks = NULL;
static size_t num_empty_blocks = 0;

#define FOREACH_BLOCK_NO_LOCK_CONDITION(cond,bl) {			\
	size_t __index;							\
	SGEN_ASSERT (0, (cond) && !sweep_in_progress (), "Can't iterate blocks while the world is running or sweep is in progress."); \
	for (__index = 0; __index < allocated_blocks.next_slot; ++__index) { \
		(bl) = BLOCK_UNTAG (allocated_blocks.data [__index]);
#define FOREACH_BLOCK_NO_LOCK(bl)					\
	FOREACH_BLOCK_NO_LOCK_CONDITION(sgen_is_world_stopped (), bl)
#define FOREACH_BLOCK_HAS_REFERENCES_NO_LOCK(bl,hr) {			\
	size_t __index;							\
	SGEN_ASSERT (0, sgen_is_world_stopped () && !sweep_in_progress (), "Can't iterate blocks while the world is running or sweep is in progress."); \
	for (__index = 0; __index < allocated_blocks.next_slot; ++__index) { \
		(bl) = (MSBlockInfo *)allocated_blocks.data [__index];			\
		(hr) = BLOCK_IS_TAGGED_HAS_REFERENCES ((bl));		\
		(bl) = BLOCK_UNTAG ((bl));
#define END_FOREACH_BLOCK_NO_LOCK	} }

static volatile size_t num_major_sections = 0;
/*
 * One free block list for each block object size.  We add and remove blocks from these
 * lists lock-free via CAS.
 *
 * Blocks accessed/removed from `free_block_lists`:
 *   from the mutator (with GC lock held)
 *   in nursery collections
 *   in non-concurrent major collections
 *   in the finishing pause of concurrent major collections (whole list is cleared)
 *
 * Blocks added to `free_block_lists`:
 *   in the sweeping thread
 *   during nursery collections
 *   from domain clearing (with the world stopped and no sweeping happening)
 *
 * The only item of those that doesn't require the GC lock is the sweep thread.  The sweep
 * thread only ever adds blocks to the free list, so the ABA problem can't occur.
 */
static MSBlockInfo * volatile *free_block_lists [MS_BLOCK_TYPE_MAX];

static guint64 stat_major_blocks_alloced = 0;
static guint64 stat_major_blocks_freed = 0;
static guint64 stat_major_blocks_lazy_swept = 0;

#if SIZEOF_VOID_P != 8
static guint64 stat_major_blocks_freed_ideal = 0;
static guint64 stat_major_blocks_freed_less_ideal = 0;
static guint64 stat_major_blocks_freed_individual = 0;
static guint64 stat_major_blocks_alloced_less_ideal = 0;
#endif

#ifdef SGEN_COUNT_NUMBER_OF_MAJOR_OBJECTS_MARKED
static guint64 num_major_objects_marked = 0;
#define INC_NUM_MAJOR_OBJECTS_MARKED()	(++num_major_objects_marked)
#else
#define INC_NUM_MAJOR_OBJECTS_MARKED()
#endif

#ifdef SGEN_HEAVY_BINARY_PROTOCOL
static mono_mutex_t scanned_objects_list_lock;
static SgenPointerQueue scanned_objects_list;

static void
add_scanned_object (void *ptr)
{
	if (!binary_protocol_is_enabled ())
		return;

	mono_os_mutex_lock (&scanned_objects_list_lock);
	sgen_pointer_queue_add (&scanned_objects_list, ptr);
	mono_os_mutex_unlock (&scanned_objects_list_lock);
}
#endif

static gboolean sweep_block (MSBlockInfo *block);

static int
ms_find_block_obj_size_index (size_t size)
{
	int i;
	SGEN_ASSERT (9, size <= SGEN_MAX_SMALL_OBJ_SIZE, "size %zd is bigger than max small object size %d", size, SGEN_MAX_SMALL_OBJ_SIZE);
	for (i = 0; i < num_block_obj_sizes; ++i)
		if (block_obj_sizes [i] >= size)
			return i;
	g_error ("no object of size %zd\n", size);
	return -1;
}

#define FREE_BLOCKS_FROM(lists,p,r)	(lists [((p) ? MS_BLOCK_FLAG_PINNED : 0) | ((r) ? MS_BLOCK_FLAG_REFS : 0)])
#define FREE_BLOCKS(p,r)		(FREE_BLOCKS_FROM (free_block_lists, (p), (r)))

#define MS_BLOCK_OBJ_SIZE_INDEX(s)				\
	(((s)+7)>>3 < MS_NUM_FAST_BLOCK_OBJ_SIZE_INDEXES ?	\
	 fast_block_obj_size_indexes [((s)+7)>>3] :		\
	 ms_find_block_obj_size_index ((s)))

static void*
major_alloc_heap (mword nursery_size, mword nursery_align, int the_nursery_bits)
{
	char *start;
	if (nursery_align)
		start = (char *)sgen_alloc_os_memory_aligned (nursery_size, nursery_align, (SgenAllocFlags)(SGEN_ALLOC_HEAP | SGEN_ALLOC_ACTIVATE), "nursery");
	else
		start = (char *)sgen_alloc_os_memory (nursery_size, (SgenAllocFlags)(SGEN_ALLOC_HEAP | SGEN_ALLOC_ACTIVATE), "nursery");

	return start;
}

static void
update_heap_boundaries_for_block (MSBlockInfo *block)
{
	sgen_update_heap_boundaries ((mword)MS_BLOCK_FOR_BLOCK_INFO (block), (mword)MS_BLOCK_FOR_BLOCK_INFO (block) + MS_BLOCK_SIZE);
}

/*
 * Thread safe
 */
static void*
ms_get_empty_block (void)
{
	char *p;
	int i;
	void *block, *empty, *next;

 retry:
	if (!empty_blocks) {
		/*
		 * We try allocating MS_BLOCK_ALLOC_NUM blocks first.  If that's
		 * unsuccessful, we halve the number of blocks and try again, until we're at
		 * 1.  If that doesn't work, either, we assert.
		 */
		int alloc_num = MS_BLOCK_ALLOC_NUM;
		for (;;) {
			p = (char *)sgen_alloc_os_memory_aligned (MS_BLOCK_SIZE * alloc_num, MS_BLOCK_SIZE,
				(SgenAllocFlags)(SGEN_ALLOC_HEAP | SGEN_ALLOC_ACTIVATE),
				alloc_num == 1 ? "major heap section" : NULL);
			if (p)
				break;
			alloc_num >>= 1;
		}

		for (i = 0; i < alloc_num; ++i) {
			block = p;
			/*
			 * We do the free list update one after the
			 * other so that other threads can use the new
			 * blocks as quickly as possible.
			 */
			do {
				empty = empty_blocks;
				*(void**)block = empty;
			} while (SGEN_CAS_PTR ((gpointer*)&empty_blocks, block, empty) != empty);
			p += MS_BLOCK_SIZE;
		}

		SGEN_ATOMIC_ADD_P (num_empty_blocks, alloc_num);

		stat_major_blocks_alloced += alloc_num;
#if SIZEOF_VOID_P != 8
		if (alloc_num != MS_BLOCK_ALLOC_NUM)
			stat_major_blocks_alloced_less_ideal += alloc_num;
#endif
	}

	do {
		empty = empty_blocks;
		if (!empty)
			goto retry;
		block = empty;
		next = *(void**)block;
	} while (SGEN_CAS_PTR (&empty_blocks, next, empty) != empty);

	SGEN_ATOMIC_ADD_P (num_empty_blocks, -1);

	*(void**)block = NULL;

	g_assert (!((mword)block & (MS_BLOCK_SIZE - 1)));

	return block;
}

/*
 * This doesn't actually free a block immediately, but enqueues it into the `empty_blocks`
 * list, where it will either be freed later on, or reused in nursery collections.
 */
static void
ms_free_block (void *block)
{
	void *empty;

	sgen_memgov_release_space (MS_BLOCK_SIZE, SPACE_MAJOR);
	memset (block, 0, MS_BLOCK_SIZE);

	do {
		empty = empty_blocks;
		*(void**)block = empty;
	} while (SGEN_CAS_PTR (&empty_blocks, block, empty) != empty);

	SGEN_ATOMIC_ADD_P (num_empty_blocks, 1);

	binary_protocol_block_free (block, MS_BLOCK_SIZE);
}

static gboolean
sweep_in_progress (void)
{
	int state = sweep_state;
	return state == SWEEP_STATE_SWEEPING ||
		state == SWEEP_STATE_SWEEPING_AND_ITERATING ||
		state == SWEEP_STATE_COMPACTING;
}

static inline gboolean
block_is_swept_or_marking (MSBlockInfo *block)
{
	gint32 state = block->state;
	return state == BLOCK_STATE_SWEPT || state == BLOCK_STATE_MARKING;
}

//#define MARKSWEEP_CONSISTENCY_CHECK

#ifdef MARKSWEEP_CONSISTENCY_CHECK
static void
check_block_free_list (MSBlockInfo *block, int size, gboolean pinned)
{
	SGEN_ASSERT (0, !sweep_in_progress (), "Can't examine allocated blocks during sweep");
	for (; block; block = block->next_free) {
		SGEN_ASSERT (0, block->state != BLOCK_STATE_CHECKING, "Can't have a block we're checking in a free list.");
		g_assert (block->obj_size == size);
		g_assert ((pinned && block->pinned) || (!pinned && !block->pinned));

		/* blocks in the free lists must have at least
		   one free slot */
		g_assert (block->free_list);

		/* the block must be in the allocated_blocks array */
		g_assert (sgen_pointer_queue_find (&allocated_blocks, BLOCK_TAG (block)) != (size_t)-1);
	}
}

static void
check_empty_blocks (void)
{
	void *p;
	size_t i = 0;
	for (p = empty_blocks; p; p = *(void**)p)
		++i;
	g_assert (i == num_empty_blocks);
}

static void
consistency_check (void)
{
	MSBlockInfo *block;
	int i;

	/* check all blocks */
	FOREACH_BLOCK_NO_LOCK (block) {
		int count = MS_BLOCK_FREE / block->obj_size;
		int num_free = 0;
		void **free;

		/* count number of free slots */
		for (i = 0; i < count; ++i) {
			void **obj = (void**) MS_BLOCK_OBJ (block, i);
			if (!MS_OBJ_ALLOCED (obj, block))
				++num_free;
		}

		/* check free list */
		for (free = block->free_list; free; free = (void**)*free) {
			g_assert (MS_BLOCK_FOR_OBJ (free) == block);
			--num_free;
		}
		g_assert (num_free == 0);

		/* check all mark words are zero */
		if (!sgen_concurrent_collection_in_progress () && block_is_swept_or_marking (block)) {
			for (i = 0; i < MS_NUM_MARK_WORDS; ++i)
				g_assert (block->mark_words [i] == 0);
		}
	} END_FOREACH_BLOCK_NO_LOCK;

	/* check free blocks */
	for (i = 0; i < num_block_obj_sizes; ++i) {
		int j;
		for (j = 0; j < MS_BLOCK_TYPE_MAX; ++j)
			check_block_free_list (free_block_lists [j][i], block_obj_sizes [i], j & MS_BLOCK_FLAG_PINNED);
	}

	check_empty_blocks ();
}
#endif

static void
add_free_block (MSBlockInfo * volatile *free_blocks, int size_index, MSBlockInfo *block)
{
	MSBlockInfo *old;
	do {
		block->next_free = old = free_blocks [size_index];
	} while (SGEN_CAS_PTR ((volatile gpointer *)&free_blocks [size_index], block, old) != old);
}

static void major_finish_sweep_checking (void);

static gboolean
ms_alloc_block (int size_index, gboolean pinned, gboolean has_references)
{
	int size = block_obj_sizes [size_index];
	int count = MS_BLOCK_FREE / size;
	MSBlockInfo *info;
	MSBlockInfo * volatile * free_blocks = FREE_BLOCKS (pinned, has_references);
	char *obj_start;
	int i;

	if (!sgen_memgov_try_alloc_space (MS_BLOCK_SIZE, SPACE_MAJOR))
		return FALSE;

	info = (MSBlockInfo*)ms_get_empty_block ();

	SGEN_ASSERT (9, count >= 2, "block with %d objects, it must hold at least 2", count);

	info->obj_size = size;
	info->obj_size_index = size_index;
	info->pinned = pinned;
	info->has_references = has_references;
	info->has_pinned = pinned;
	/*
	 * Blocks that are to-space are not evacuated from.  During an major collection
	 * blocks are allocated for two reasons: evacuating objects from the nursery and
	 * evacuating them from major blocks marked for evacuation.  In both cases we don't
	 * want further evacuation. We also don't want to evacuate objects allocated during
	 * the concurrent mark since it would add pointless stress on the finishing pause.
	 */
	info->is_to_space = (sgen_get_current_collection_generation () == GENERATION_OLD) || sgen_concurrent_collection_in_progress ();
	info->state = info->is_to_space ? BLOCK_STATE_MARKING : BLOCK_STATE_SWEPT;
	SGEN_ASSERT (6, !sweep_in_progress () || info->state == BLOCK_STATE_SWEPT, "How do we add a new block to be swept while sweeping?");
	info->cardtable_mod_union = NULL;

	update_heap_boundaries_for_block (info);

	binary_protocol_block_alloc (info, MS_BLOCK_SIZE);

	/* build free list */
	obj_start = MS_BLOCK_FOR_BLOCK_INFO (info) + MS_BLOCK_SKIP;
	info->free_list = (void**)obj_start;
	/* we're skipping the last one - it must be nulled */
	for (i = 0; i < count - 1; ++i) {
		char *next_obj_start = obj_start + size;
		*(void**)obj_start = next_obj_start;
		obj_start = next_obj_start;
	}
	/* the last one */
	*(void**)obj_start = NULL;

	add_free_block (free_blocks, size_index, info);

	/*
	 * Adding to the allocated_blocks array is racy with the removal of nulls when
	 * sweeping. We wait for sweep to finish to avoid that.
	 *
	 * The memory barrier here and in `sweep_job_func()` are required because we need
	 * `allocated_blocks` synchronized between this and the sweep thread.
	 */
	major_finish_sweep_checking ();
	mono_memory_barrier ();

	sgen_pointer_queue_add (&allocated_blocks, BLOCK_TAG (info));

	SGEN_ATOMIC_ADD_P (num_major_sections, 1);
	return TRUE;
}

static gboolean
ptr_is_from_pinned_alloc (char *ptr)
{
	MSBlockInfo *block;

	FOREACH_BLOCK_NO_LOCK (block) {
		if (ptr >= MS_BLOCK_FOR_BLOCK_INFO (block) && ptr <= MS_BLOCK_FOR_BLOCK_INFO (block) + MS_BLOCK_SIZE)
			return block->pinned;
	} END_FOREACH_BLOCK_NO_LOCK;
	return FALSE;
}

static void
ensure_can_access_block_free_list (MSBlockInfo *block)
{
 retry:
	for (;;) {
		switch (block->state) {
		case BLOCK_STATE_SWEPT:
		case BLOCK_STATE_MARKING:
			return;
		case BLOCK_STATE_CHECKING:
			SGEN_ASSERT (0, FALSE, "How did we get a block that's being checked from a free list?");
			break;
		case BLOCK_STATE_NEED_SWEEPING:
			if (sweep_block (block))
				++stat_major_blocks_lazy_swept;
			break;
		case BLOCK_STATE_SWEEPING:
			/* FIXME: do this more elegantly */
			g_usleep (100);
			goto retry;
		default:
			SGEN_ASSERT (0, FALSE, "Illegal block state");
			break;
		}
	}
}

static void*
unlink_slot_from_free_list_uncontested (MSBlockInfo * volatile *free_blocks, int size_index)
{
	MSBlockInfo *block, *next_free_block;
	void *obj, *next_free_slot;

 retry:
	block = free_blocks [size_index];
	SGEN_ASSERT (9, block, "no free block to unlink from free_blocks %p size_index %d", free_blocks, size_index);

	ensure_can_access_block_free_list (block);

	obj = block->free_list;
	SGEN_ASSERT (6, obj, "block %p in free list had no available object to alloc from", block);

	next_free_slot = *(void**)obj;
	if (next_free_slot) {
		block->free_list = (gpointer *)next_free_slot;
		return obj;
	}

	next_free_block = block->next_free;
	if (SGEN_CAS_PTR ((volatile gpointer *)&free_blocks [size_index], next_free_block, block) != block)
		goto retry;

	block->free_list = NULL;
	block->next_free = NULL;

	return obj;
}

static GCObject*
alloc_obj (GCVTable vtable, size_t size, gboolean pinned, gboolean has_references)
{
	int size_index = MS_BLOCK_OBJ_SIZE_INDEX (size);
	MSBlockInfo * volatile * free_blocks = FREE_BLOCKS (pinned, has_references);
	void *obj;

	if (!free_blocks [size_index]) {
		if (G_UNLIKELY (!ms_alloc_block (size_index, pinned, has_references)))
			return NULL;
	}

	obj = unlink_slot_from_free_list_uncontested (free_blocks, size_index);

	/* FIXME: assumes object layout */
	*(GCVTable*)obj = vtable;

	return (GCObject *)obj;
}

static GCObject*
major_alloc_object (GCVTable vtable, size_t size, gboolean has_references)
{
	return alloc_obj (vtable, size, FALSE, has_references);
}

/*
 * We're not freeing the block if it's empty.  We leave that work for
 * the next major collection.
 *
 * This is just called from the domain clearing code, which runs in a
 * single thread and has the GC lock, so we don't need an extra lock.
 */
static void
free_object (GCObject *obj, size_t size, gboolean pinned)
{
	MSBlockInfo *block = MS_BLOCK_FOR_OBJ (obj);
	int word, bit;
	gboolean in_free_list;

	SGEN_ASSERT (9, sweep_state == SWEEP_STATE_SWEPT, "Should have waited for sweep to free objects.");

	ensure_can_access_block_free_list (block);
	SGEN_ASSERT (9, (pinned && block->pinned) || (!pinned && !block->pinned), "free-object pinning mixup object %p pinned %d block %p pinned %d", obj, pinned, block, block->pinned);
	SGEN_ASSERT (9, MS_OBJ_ALLOCED (obj, block), "object %p is already free", obj);
	MS_CALC_MARK_BIT (word, bit, obj);
	SGEN_ASSERT (9, !MS_MARK_BIT (block, word, bit), "object %p has mark bit set", obj);

	memset (obj, 0, size);

	in_free_list = !!block->free_list;
	*(void**)obj = block->free_list;
	block->free_list = (void**)obj;

	if (!in_free_list) {
		MSBlockInfo * volatile *free_blocks = FREE_BLOCKS (pinned, block->has_references);
		int size_index = MS_BLOCK_OBJ_SIZE_INDEX (size);
		SGEN_ASSERT (9, !block->next_free, "block %p doesn't have a free-list of object but belongs to a free-list of blocks", block);
		add_free_block (free_blocks, size_index, block);
	}
}

static void
major_free_non_pinned_object (GCObject *obj, size_t size)
{
	free_object (obj, size, FALSE);
}

/* size is a multiple of SGEN_ALLOC_ALIGN */
static GCObject*
major_alloc_small_pinned_obj (GCVTable vtable, size_t size, gboolean has_references)
{
	void *res;

	res = alloc_obj (vtable, size, TRUE, has_references);
	 /*If we failed to alloc memory, we better try releasing memory
	  *as pinned alloc is requested by the runtime.
	  */
	 if (!res) {
		sgen_perform_collection (0, GENERATION_OLD, "pinned alloc failure", TRUE);
		res = alloc_obj (vtable, size, TRUE, has_references);
	 }
	 return (GCObject *)res;
}

static void
free_pinned_object (GCObject *obj, size_t size)
{
	free_object (obj, size, TRUE);
}

/*
 * size is already rounded up and we hold the GC lock.
 */
static GCObject*
major_alloc_degraded (GCVTable vtable, size_t size)
{
	GCObject *obj;

	obj = alloc_obj (vtable, size, FALSE, SGEN_VTABLE_HAS_REFERENCES (vtable));
	if (G_LIKELY (obj)) {
		HEAVY_STAT (++stat_objects_alloced_degraded);
		HEAVY_STAT (stat_bytes_alloced_degraded += size);
	}
	return obj;
}

/*
 * obj is some object.  If it's not in the major heap (i.e. if it's in
 * the nursery or LOS), return FALSE.  Otherwise return whether it's
 * been marked or copied.
 */
static gboolean
major_is_object_live (GCObject *obj)
{
	MSBlockInfo *block;
	int word, bit;
	mword objsize;

	if (sgen_ptr_in_nursery (obj))
		return FALSE;

	objsize = SGEN_ALIGN_UP (sgen_safe_object_get_size (obj));

	/* LOS */
	if (objsize > SGEN_MAX_SMALL_OBJ_SIZE)
		return FALSE;

	/* now we know it's in a major block */
	block = MS_BLOCK_FOR_OBJ (obj);
	SGEN_ASSERT (9, !block->pinned, "block %p is pinned, BTW why is this bad?", block);
	MS_CALC_MARK_BIT (word, bit, obj);
	return MS_MARK_BIT (block, word, bit) ? TRUE : FALSE;
}

static gboolean
major_ptr_is_in_non_pinned_space (char *ptr, char **start)
{
	MSBlockInfo *block;

	FOREACH_BLOCK_NO_LOCK (block) {
		if (ptr >= MS_BLOCK_FOR_BLOCK_INFO (block) && ptr <= MS_BLOCK_FOR_BLOCK_INFO (block) + MS_BLOCK_SIZE) {
			int count = MS_BLOCK_FREE / block->obj_size;
			int i;

			*start = NULL;
			for (i = 0; i <= count; ++i) {
				if (ptr >= (char*)MS_BLOCK_OBJ (block, i) && ptr < (char*)MS_BLOCK_OBJ (block, i + 1)) {
					*start = (char *)MS_BLOCK_OBJ (block, i);
					break;
				}
			}
			return !block->pinned;
		}
	} END_FOREACH_BLOCK_NO_LOCK;
	return FALSE;
}

static gboolean
try_set_sweep_state (int new_, int expected)
{
	int old = SGEN_CAS (&sweep_state, new_, expected);
	return old == expected;
}

static void
set_sweep_state (int new_, int expected)
{
	gboolean success = try_set_sweep_state (new_, expected);
	SGEN_ASSERT (0, success, "Could not set sweep state.");
}

static gboolean ensure_block_is_checked_for_sweeping (int block_index, gboolean wait, gboolean *have_checked);

static SgenThreadPoolJob * volatile sweep_job;

static void
major_finish_sweep_checking (void)
{
	int block_index;
	SgenThreadPoolJob *job;

 retry:
	switch (sweep_state) {
	case SWEEP_STATE_SWEPT:
	case SWEEP_STATE_NEED_SWEEPING:
		return;
	case SWEEP_STATE_SWEEPING:
		if (try_set_sweep_state (SWEEP_STATE_SWEEPING_AND_ITERATING, SWEEP_STATE_SWEEPING))
			break;
		goto retry;
	case SWEEP_STATE_SWEEPING_AND_ITERATING:
		SGEN_ASSERT (0, FALSE, "Is there another minor collection running?");
		goto retry;
	case SWEEP_STATE_COMPACTING:
		goto wait;
	default:
		SGEN_ASSERT (0, FALSE, "Invalid sweep state.");
		break;
	}

	/*
	 * We're running with the world stopped and the only other thread doing work is the
	 * sweep thread, which doesn't add blocks to the array, so we can safely access
	 * `next_slot`.
	 */
	for (block_index = 0; block_index < allocated_blocks.next_slot; ++block_index)
		ensure_block_is_checked_for_sweeping (block_index, FALSE, NULL);

	set_sweep_state (SWEEP_STATE_SWEEPING, SWEEP_STATE_SWEEPING_AND_ITERATING);

 wait:
	job = sweep_job;
	if (job)
		sgen_thread_pool_job_wait (job);
	SGEN_ASSERT (0, !sweep_job, "Why did the sweep job not null itself?");
	SGEN_ASSERT (0, sweep_state == SWEEP_STATE_SWEPT, "How is the sweep job done but we're not swept?");
}

static void
major_iterate_objects (IterateObjectsFlags flags, IterateObjectCallbackFunc callback, void *data)
{
	gboolean sweep = flags & ITERATE_OBJECTS_SWEEP;
	gboolean non_pinned = flags & ITERATE_OBJECTS_NON_PINNED;
	gboolean pinned = flags & ITERATE_OBJECTS_PINNED;
	MSBlockInfo *block;

	major_finish_sweep_checking ();
	FOREACH_BLOCK_NO_LOCK (block) {
		int count = MS_BLOCK_FREE / block->obj_size;
		int i;

		if (block->pinned && !pinned)
			continue;
		if (!block->pinned && !non_pinned)
			continue;
		if (sweep && lazy_sweep) {
			sweep_block (block);
			SGEN_ASSERT (6, block->state == BLOCK_STATE_SWEPT, "Block must be swept after sweeping");
		}

		for (i = 0; i < count; ++i) {
			void **obj = (void**) MS_BLOCK_OBJ (block, i);
			/*
			 * We've finished sweep checking, but if we're sweeping lazily and
			 * the flags don't require us to sweep, the block might still need
			 * sweeping.  In that case, we need to consult the mark bits to tell
			 * us whether an object slot is live.
			 */
			if (!block_is_swept_or_marking (block)) {
				int word, bit;
				SGEN_ASSERT (6, !sweep && block->state == BLOCK_STATE_NEED_SWEEPING, "Has sweeping not finished?");
				MS_CALC_MARK_BIT (word, bit, obj);
				if (!MS_MARK_BIT (block, word, bit))
					continue;
			}
			if (MS_OBJ_ALLOCED (obj, block))
				callback ((GCObject*)obj, block->obj_size, data);
		}
	} END_FOREACH_BLOCK_NO_LOCK;
}

static gboolean
major_is_valid_object (char *object)
{
	MSBlockInfo *block;

	FOREACH_BLOCK_NO_LOCK (block) {
		int idx;
		char *obj;

		if ((MS_BLOCK_FOR_BLOCK_INFO (block) > object) || ((MS_BLOCK_FOR_BLOCK_INFO (block) + MS_BLOCK_SIZE) <= object))
			continue;

		idx = MS_BLOCK_OBJ_INDEX (object, block);
		obj = (char*)MS_BLOCK_OBJ (block, idx);
		if (obj != object)
			return FALSE;
		return MS_OBJ_ALLOCED (obj, block);
	} END_FOREACH_BLOCK_NO_LOCK;

	return FALSE;
}


static GCVTable
major_describe_pointer (char *ptr)
{
	MSBlockInfo *block;

	FOREACH_BLOCK_NO_LOCK (block) {
		int idx;
		char *obj;
		gboolean live;
		GCVTable vtable;
		int w, b;
		gboolean marked;

		if ((MS_BLOCK_FOR_BLOCK_INFO (block) > ptr) || ((MS_BLOCK_FOR_BLOCK_INFO (block) + MS_BLOCK_SIZE) <= ptr))
			continue;

		SGEN_LOG (0, "major-ptr (block %p sz %d pin %d ref %d)\n",
			MS_BLOCK_FOR_BLOCK_INFO (block), block->obj_size, block->pinned, block->has_references);

		idx = MS_BLOCK_OBJ_INDEX (ptr, block);
		obj = (char*)MS_BLOCK_OBJ (block, idx);
		live = MS_OBJ_ALLOCED (obj, block);
		vtable = live ? SGEN_LOAD_VTABLE ((GCObject*)obj) : NULL;

		MS_CALC_MARK_BIT (w, b, obj);
		marked = MS_MARK_BIT (block, w, b);

		if (obj == ptr) {
			SGEN_LOG (0, "\t(");
			if (live)
				SGEN_LOG (0, "object");
			else
				SGEN_LOG (0, "dead-object");
		} else {
			if (live)
				SGEN_LOG (0, "interior-ptr offset %zd", ptr - obj);
			else
				SGEN_LOG (0, "dead-interior-ptr offset %zd", ptr - obj);
		}

		SGEN_LOG (0, " marked %d)\n", marked ? 1 : 0);

		return vtable;
	} END_FOREACH_BLOCK_NO_LOCK;

	return NULL;
}

static void
major_check_scan_starts (void)
{
}

static void
major_dump_heap (FILE *heap_dump_file)
{
	MSBlockInfo *block;
	int *slots_available = (int *)alloca (sizeof (int) * num_block_obj_sizes);
	int *slots_used = (int *)alloca (sizeof (int) * num_block_obj_sizes);
	int i;

	for (i = 0; i < num_block_obj_sizes; ++i)
		slots_available [i] = slots_used [i] = 0;

	FOREACH_BLOCK_NO_LOCK (block) {
		int index = ms_find_block_obj_size_index (block->obj_size);
		int count = MS_BLOCK_FREE / block->obj_size;

		slots_available [index] += count;
		for (i = 0; i < count; ++i) {
			if (MS_OBJ_ALLOCED (MS_BLOCK_OBJ (block, i), block))
				++slots_used [index];
		}
	} END_FOREACH_BLOCK_NO_LOCK;

	fprintf (heap_dump_file, "<occupancies>\n");
	for (i = 0; i < num_block_obj_sizes; ++i) {
		fprintf (heap_dump_file, "<occupancy size=\"%d\" available=\"%d\" used=\"%d\" />\n",
				block_obj_sizes [i], slots_available [i], slots_used [i]);
	}
	fprintf (heap_dump_file, "</occupancies>\n");

	FOREACH_BLOCK_NO_LOCK (block) {
		int count = MS_BLOCK_FREE / block->obj_size;
		int i;
		int start = -1;

		fprintf (heap_dump_file, "<section type=\"%s\" size=\"%zu\">\n", "old", (size_t)MS_BLOCK_FREE);

		for (i = 0; i <= count; ++i) {
			if ((i < count) && MS_OBJ_ALLOCED (MS_BLOCK_OBJ (block, i), block)) {
				if (start < 0)
					start = i;
			} else {
				if (start >= 0) {
					sgen_dump_occupied ((char *)MS_BLOCK_OBJ (block, start), (char *)MS_BLOCK_OBJ (block, i), MS_BLOCK_FOR_BLOCK_INFO (block));
					start = -1;
				}
			}
		}

		fprintf (heap_dump_file, "</section>\n");
	} END_FOREACH_BLOCK_NO_LOCK;
}

static guint8*
get_cardtable_mod_union_for_block (MSBlockInfo *block, gboolean allocate)
{
	guint8 *mod_union = block->cardtable_mod_union;
	guint8 *other;
	if (mod_union)
		return mod_union;
	else if (!allocate)
		return NULL;
	mod_union = sgen_card_table_alloc_mod_union (MS_BLOCK_FOR_BLOCK_INFO (block), MS_BLOCK_SIZE);
	other = (guint8 *)SGEN_CAS_PTR ((gpointer*)&block->cardtable_mod_union, mod_union, NULL);
	if (!other) {
		SGEN_ASSERT (0, block->cardtable_mod_union == mod_union, "Why did CAS not replace?");
		return mod_union;
	}
	sgen_card_table_free_mod_union (mod_union, MS_BLOCK_FOR_BLOCK_INFO (block), MS_BLOCK_SIZE);
	return other;
}

static inline guint8*
major_get_cardtable_mod_union_for_reference (char *ptr)
{
	MSBlockInfo *block = MS_BLOCK_FOR_OBJ (ptr);
	size_t offset = sgen_card_table_get_card_offset (ptr, (char*)sgen_card_table_align_pointer (MS_BLOCK_FOR_BLOCK_INFO (block)));
	guint8 *mod_union = get_cardtable_mod_union_for_block (block, TRUE);
	SGEN_ASSERT (0, mod_union, "FIXME: optionally allocate the mod union if it's not here and CAS it in.");
	return &mod_union [offset];
}

/*
 * Mark the mod-union card for `ptr`, which must be a reference within the object `obj`.
 */
static void
mark_mod_union_card (GCObject *obj, void **ptr, GCObject *value_obj)
{
	int type = sgen_obj_get_descriptor (obj) & DESC_TYPE_MASK;
	if (sgen_safe_object_is_small (obj, type)) {
		guint8 *card_byte = major_get_cardtable_mod_union_for_reference ((char*)ptr);
		SGEN_ASSERT (0, MS_BLOCK_FOR_OBJ (obj) == MS_BLOCK_FOR_OBJ (ptr), "How can an object and a reference inside it not be in the same block?");
		*card_byte = 1;
	} else {
		sgen_los_mark_mod_union_card (obj, ptr);
	}

	binary_protocol_mod_union_remset (obj, ptr, value_obj, SGEN_LOAD_VTABLE (value_obj));
}

static inline gboolean
major_block_is_evacuating (MSBlockInfo *block)
{
	if (evacuate_block_obj_sizes [block->obj_size_index] &&
			!block->has_pinned &&
			!block->is_to_space)
		return TRUE;
	return FALSE;
}

#define LOAD_VTABLE	SGEN_LOAD_VTABLE

#define MS_MARK_OBJECT_AND_ENQUEUE_CHECKED(obj,desc,block,queue) do {	\
		int __word, __bit;					\
		MS_CALC_MARK_BIT (__word, __bit, (obj));		\
		if (!MS_MARK_BIT ((block), __word, __bit) && MS_OBJ_ALLOCED ((obj), (block))) {	\
			MS_SET_MARK_BIT ((block), __word, __bit);	\
			if (sgen_gc_descr_has_references (desc))			\
				GRAY_OBJECT_ENQUEUE ((queue), (obj), (desc)); \
			binary_protocol_mark ((obj), (gpointer)LOAD_VTABLE ((obj)), sgen_safe_object_get_size ((obj))); \
			INC_NUM_MAJOR_OBJECTS_MARKED ();		\
		}							\
	} while (0)
#define MS_MARK_OBJECT_AND_ENQUEUE(obj,desc,block,queue) do {		\
		int __word, __bit;					\
		MS_CALC_MARK_BIT (__word, __bit, (obj));		\
		SGEN_ASSERT (9, MS_OBJ_ALLOCED ((obj), (block)), "object %p not allocated", obj); \
		if (!MS_MARK_BIT ((block), __word, __bit)) {		\
			MS_SET_MARK_BIT ((block), __word, __bit);	\
			if (sgen_gc_descr_has_references (desc))			\
				GRAY_OBJECT_ENQUEUE ((queue), (obj), (desc)); \
			binary_protocol_mark ((obj), (gpointer)LOAD_VTABLE ((obj)), sgen_safe_object_get_size ((obj))); \
			INC_NUM_MAJOR_OBJECTS_MARKED ();		\
		}							\
	} while (0)

static void
pin_major_object (GCObject *obj, SgenGrayQueue *queue)
{
	MSBlockInfo *block;

	if (concurrent_mark)
		g_assert_not_reached ();

	block = MS_BLOCK_FOR_OBJ (obj);
	block->has_pinned = TRUE;
	MS_MARK_OBJECT_AND_ENQUEUE (obj, sgen_obj_get_descriptor (obj), block, queue);
}

#include "sgen-major-copy-object.h"

static long long
major_get_and_reset_num_major_objects_marked (void)
{
#ifdef SGEN_COUNT_NUMBER_OF_MAJOR_OBJECTS_MARKED
	long long num = num_major_objects_marked;
	num_major_objects_marked = 0;
	return num;
#else
	return 0;
#endif
}

#define PREFETCH_CARDS		1	/* BOOL FASTENABLE */
#if !PREFETCH_CARDS
#undef PREFETCH_CARDS
#endif

/* gcc 4.2.1 from xcode4 crashes on sgen_card_table_get_card_address () when this is enabled */
#if defined(PLATFORM_MACOSX)
#define GCC_VERSION (__GNUC__ * 10000 \
                               + __GNUC_MINOR__ * 100 \
                               + __GNUC_PATCHLEVEL__)
#if GCC_VERSION <= 40300
#undef PREFETCH_CARDS
#endif
#endif

#ifdef HEAVY_STATISTICS
static guint64 stat_optimized_copy;
static guint64 stat_optimized_copy_nursery;
static guint64 stat_optimized_copy_nursery_forwarded;
static guint64 stat_optimized_copy_nursery_pinned;
static guint64 stat_optimized_copy_major;
static guint64 stat_optimized_copy_major_small_fast;
static guint64 stat_optimized_copy_major_small_slow;
static guint64 stat_optimized_copy_major_large;
static guint64 stat_optimized_copy_major_forwarded;
static guint64 stat_optimized_copy_major_small_evacuate;
static guint64 stat_optimized_major_scan;
static guint64 stat_optimized_major_scan_no_refs;

static guint64 stat_drain_prefetch_fills;
static guint64 stat_drain_prefetch_fill_failures;
static guint64 stat_drain_loops;
#endif

#define COPY_OR_MARK_FUNCTION_NAME	major_copy_or_mark_object_no_evacuation
#define SCAN_OBJECT_FUNCTION_NAME	major_scan_object_no_evacuation
#define DRAIN_GRAY_STACK_FUNCTION_NAME	drain_gray_stack_no_evacuation
#include "sgen-marksweep-drain-gray-stack.h"

#define COPY_OR_MARK_WITH_EVACUATION
#define COPY_OR_MARK_FUNCTION_NAME	major_copy_or_mark_object_with_evacuation
#define SCAN_OBJECT_FUNCTION_NAME	major_scan_object_with_evacuation
#define DRAIN_GRAY_STACK_FUNCTION_NAME	drain_gray_stack_with_evacuation
#include "sgen-marksweep-drain-gray-stack.h"

#undef COPY_OR_MARK_WITH_EVACUATION
#define COPY_OR_MARK_CONCURRENT
#define COPY_OR_MARK_FUNCTION_NAME	major_copy_or_mark_object_concurrent_no_evacuation
#define SCAN_OBJECT_FUNCTION_NAME	major_scan_object_concurrent_no_evacuation
#define DRAIN_GRAY_STACK_FUNCTION_NAME	drain_gray_stack_concurrent_no_evacuation
#include "sgen-marksweep-drain-gray-stack.h"

#undef COPY_OR_MARK_CONCURRENT
#define COPY_OR_MARK_CONCURRENT_WITH_EVACUATION
#define COPY_OR_MARK_FUNCTION_NAME	major_copy_or_mark_object_concurrent_with_evacuation
#define SCAN_OBJECT_FUNCTION_NAME	major_scan_object_concurrent_with_evacuation
#define DRAIN_GRAY_STACK_FUNCTION_NAME	drain_gray_stack_concurrent_with_evacuation
#include "sgen-marksweep-drain-gray-stack.h"

static inline gboolean
major_is_evacuating (void)
{
	int i;
	for (i = 0; i < num_block_obj_sizes; ++i) {
		if (evacuate_block_obj_sizes [i]) {
			return TRUE;
		}
	}

	return FALSE;
}

static gboolean
drain_gray_stack (SgenGrayQueue *queue)
{
	if (major_is_evacuating ())
		return drain_gray_stack_with_evacuation (queue);
	else
		return drain_gray_stack_no_evacuation (queue);
}

static gboolean
drain_gray_stack_concurrent (SgenGrayQueue *queue)
{
	if (major_is_evacuating ())
		return drain_gray_stack_concurrent_with_evacuation (queue);
	else
		return drain_gray_stack_concurrent_no_evacuation (queue);
}

#include "sgen-marksweep-scan-object-concurrent.h"

static void
major_copy_or_mark_object_canonical (GCObject **ptr, SgenGrayQueue *queue)
{
	major_copy_or_mark_object_with_evacuation (ptr, *ptr, queue);
}

static void
major_copy_or_mark_object_concurrent_canonical (GCObject **ptr, SgenGrayQueue *queue)
{
	major_copy_or_mark_object_concurrent_with_evacuation (ptr, *ptr, queue);
}

static void
major_copy_or_mark_object_concurrent_finish_canonical (GCObject **ptr, SgenGrayQueue *queue)
{
	major_copy_or_mark_object_with_evacuation (ptr, *ptr, queue);
}

static void
mark_pinned_objects_in_block (MSBlockInfo *block, size_t first_entry, size_t last_entry, SgenGrayQueue *queue)
{
	void **entry, **end;
	int last_index = -1;

	if (first_entry == last_entry)
		return;

	block->has_pinned = TRUE;

	entry = sgen_pinning_get_entry (first_entry);
	end = sgen_pinning_get_entry (last_entry);

	for (; entry < end; ++entry) {
		int index = MS_BLOCK_OBJ_INDEX (*entry, block);
		GCObject *obj;
		SGEN_ASSERT (9, index >= 0 && index < MS_BLOCK_FREE / block->obj_size, "invalid object %p index %d max-index %d", *entry, index, (int)(MS_BLOCK_FREE / block->obj_size));
		if (index == last_index)
			continue;
		obj = MS_BLOCK_OBJ (block, index);
		MS_MARK_OBJECT_AND_ENQUEUE_CHECKED (obj, sgen_obj_get_descriptor (obj), block, queue);
		last_index = index;
	}
}

static inline void
sweep_block_for_size (MSBlockInfo *block, int count, int obj_size)
{
	int obj_index;

	for (obj_index = 0; obj_index < count; ++obj_index) {
		int word, bit;
		void *obj = MS_BLOCK_OBJ_FOR_SIZE (block, obj_index, obj_size);

		MS_CALC_MARK_BIT (word, bit, obj);
		if (MS_MARK_BIT (block, word, bit)) {
			SGEN_ASSERT (9, MS_OBJ_ALLOCED (obj, block), "object %p not allocated", obj);
		} else {
			/* an unmarked object */
			if (MS_OBJ_ALLOCED (obj, block)) {
				/*
				 * FIXME: Merge consecutive
				 * slots for lower reporting
				 * overhead.  Maybe memset
				 * will also benefit?
				 */
				binary_protocol_empty (obj, obj_size);
				memset (obj, 0, obj_size);
			}
			*(void**)obj = block->free_list;
			block->free_list = (void **)obj;
		}
	}
}

static inline gboolean
try_set_block_state (MSBlockInfo *block, gint32 new_state, gint32 expected_state)
{
	gint32 old_state = SGEN_CAS (&block->state, new_state, expected_state);
	gboolean success = old_state == expected_state;
	if (success)
		binary_protocol_block_set_state (block, MS_BLOCK_SIZE, old_state, new_state);
	return success;
}

static inline void
set_block_state (MSBlockInfo *block, gint32 new_state, gint32 expected_state)
{
	SGEN_ASSERT (6, block->state == expected_state, "Block state incorrect before set");
	block->state = new_state;
}

/*
 * If `block` needs sweeping, sweep it and return TRUE.  Otherwise return FALSE.
 *
 * Sweeping means iterating through the block's slots and building the free-list from the
 * unmarked ones.  They will also be zeroed.  The mark bits will be reset.
 */
static gboolean
sweep_block (MSBlockInfo *block)
{
	int count;
	void *reversed = NULL;

 retry:
	switch (block->state) {
	case BLOCK_STATE_SWEPT:
		return FALSE;
	case BLOCK_STATE_MARKING:
	case BLOCK_STATE_CHECKING:
		SGEN_ASSERT (0, FALSE, "How did we get to sweep a block that's being marked or being checked?");
		goto retry;
	case BLOCK_STATE_SWEEPING:
		/* FIXME: Do this more elegantly */
		g_usleep (100);
		goto retry;
	case BLOCK_STATE_NEED_SWEEPING:
		if (!try_set_block_state (block, BLOCK_STATE_SWEEPING, BLOCK_STATE_NEED_SWEEPING))
			goto retry;
		break;
	default:
		SGEN_ASSERT (0, FALSE, "Illegal block state");
	}

	SGEN_ASSERT (6, block->state == BLOCK_STATE_SWEEPING, "How did we get here without setting state to sweeping?");

	count = MS_BLOCK_FREE / block->obj_size;

	block->free_list = NULL;

	/* Use inline instances specialized to constant sizes, this allows the compiler to replace the memset calls with inline code */
	// FIXME: Add more sizes
	switch (block->obj_size) {
	case 16:
		sweep_block_for_size (block, count, 16);
		break;
	default:
		sweep_block_for_size (block, count, block->obj_size);
		break;
	}

	/* reset mark bits */
	memset (block->mark_words, 0, sizeof (mword) * MS_NUM_MARK_WORDS);

	/* Reverse free list so that it's in address order */
	reversed = NULL;
	while (block->free_list) {
		void *next = *(void**)block->free_list;
		*(void**)block->free_list = reversed;
		reversed = block->free_list;
		block->free_list = (void **)next;
	}
	block->free_list = (void **)reversed;

	mono_memory_write_barrier ();

	set_block_state (block, BLOCK_STATE_SWEPT, BLOCK_STATE_SWEEPING);

	return TRUE;
}

static inline int
bitcount (mword d)
{
	int count = 0;

#ifdef __GNUC__
	if (sizeof (mword) == 8)
		count += __builtin_popcountll (d);
	else
		count += __builtin_popcount (d);
#else
	while (d) {
		count ++;
		d &= (d - 1);
	}
#endif
	return count;
}

/* statistics for evacuation */
static size_t *sweep_slots_available;
static size_t *sweep_slots_used;
static size_t *sweep_num_blocks;

static volatile size_t num_major_sections_before_sweep;
static volatile size_t num_major_sections_freed_in_sweep;

static void
sweep_start (void)
{
	int i;

	for (i = 0; i < num_block_obj_sizes; ++i)
		sweep_slots_available [i] = sweep_slots_used [i] = sweep_num_blocks [i] = 0;

	/* clear all the free lists */
	for (i = 0; i < MS_BLOCK_TYPE_MAX; ++i) {
		MSBlockInfo * volatile *free_blocks = free_block_lists [i];
		int j;
		for (j = 0; j < num_block_obj_sizes; ++j)
			free_blocks [j] = NULL;
	}
}

static void sweep_finish (void);

/*
 * If `wait` is TRUE and the block is currently being checked, this function will wait until
 * the checking has finished.
 *
 * Returns whether the block is still there.  If `wait` is FALSE, the return value will not
 * be correct, i.e. must not be used.
 */
static gboolean
ensure_block_is_checked_for_sweeping (int block_index, gboolean wait, gboolean *have_checked)
{
	int count;
	gboolean have_live = FALSE;
	gboolean have_free = FALSE;
	int nused = 0;
	int block_state;
	int i;
	void *tagged_block;
	MSBlockInfo *block;

	SGEN_ASSERT (6, sweep_in_progress (), "Why do we call this function if there's no sweep in progress?");

	if (have_checked)
		*have_checked = FALSE;

 retry:
	tagged_block = *(void * volatile *)&allocated_blocks.data [block_index];
	if (!tagged_block)
		return FALSE;

	if (BLOCK_IS_TAGGED_CHECKING (tagged_block)) {
		if (!wait)
			return FALSE;
		/* FIXME: do this more elegantly */
		g_usleep (100);
		goto retry;
	}

	if (SGEN_CAS_PTR (&allocated_blocks.data [block_index], BLOCK_TAG_CHECKING (tagged_block), tagged_block) != tagged_block)
		goto retry;

	block = BLOCK_UNTAG (tagged_block);
	block_state = block->state;

	if (!sweep_in_progress ()) {
		SGEN_ASSERT (6, block_state != BLOCK_STATE_SWEEPING && block_state != BLOCK_STATE_CHECKING, "Invalid block state.");
		if (!lazy_sweep)
			SGEN_ASSERT (6, block_state != BLOCK_STATE_NEED_SWEEPING, "Invalid block state.");
	}

	switch (block_state) {
	case BLOCK_STATE_SWEPT:
	case BLOCK_STATE_NEED_SWEEPING:
	case BLOCK_STATE_SWEEPING:
		goto done;
	case BLOCK_STATE_MARKING:
		break;
	case BLOCK_STATE_CHECKING:
		SGEN_ASSERT (0, FALSE, "We set the CHECKING bit - how can the stage be CHECKING?");
		goto done;
	default:
		SGEN_ASSERT (0, FALSE, "Illegal block state");
		break;
	}

	SGEN_ASSERT (6, block->state == BLOCK_STATE_MARKING, "When we sweep all blocks must start out marking.");
	set_block_state (block, BLOCK_STATE_CHECKING, BLOCK_STATE_MARKING);

	if (have_checked)
		*have_checked = TRUE;

	block->has_pinned = block->pinned;

	block->is_to_space = FALSE;

	count = MS_BLOCK_FREE / block->obj_size;

	if (block->cardtable_mod_union) {
		sgen_card_table_free_mod_union (block->cardtable_mod_union, MS_BLOCK_FOR_BLOCK_INFO (block), MS_BLOCK_SIZE);
		block->cardtable_mod_union = NULL;
	}

	/* Count marked objects in the block */
	for (i = 0; i < MS_NUM_MARK_WORDS; ++i)
		nused += bitcount (block->mark_words [i]);

	if (nused)
		have_live = TRUE;
	if (nused < count)
		have_free = TRUE;

	if (have_live) {
		int obj_size_index = block->obj_size_index;
		gboolean has_pinned = block->has_pinned;

		set_block_state (block, BLOCK_STATE_NEED_SWEEPING, BLOCK_STATE_CHECKING);

		/*
		 * FIXME: Go straight to SWEPT if there are no free slots.  We need
		 * to set the free slot list to NULL, though, and maybe update some
		 * statistics.
		 */
		if (!lazy_sweep)
			sweep_block (block);

		if (!has_pinned) {
			++sweep_num_blocks [obj_size_index];
			sweep_slots_used [obj_size_index] += nused;
			sweep_slots_available [obj_size_index] += count;
		}

		/*
		 * If there are free slots in the block, add
		 * the block to the corresponding free list.
		 */
		if (have_free) {
			MSBlockInfo * volatile *free_blocks = FREE_BLOCKS (block->pinned, block->has_references);

			if (!lazy_sweep)
				SGEN_ASSERT (6, block->free_list, "How do we not have a free list when there are free slots?");

			add_free_block (free_blocks, obj_size_index, block);
		}

		/* FIXME: Do we need the heap boundaries while we do nursery collections? */
		update_heap_boundaries_for_block (block);
	} else {
		/*
		 * Blocks without live objects are removed from the
		 * block list and freed.
		 */
		SGEN_ASSERT (6, block_index < allocated_blocks.next_slot, "How did the number of blocks shrink?");
		SGEN_ASSERT (6, allocated_blocks.data [block_index] == BLOCK_TAG_CHECKING (tagged_block), "How did the block move?");

		binary_protocol_empty (MS_BLOCK_OBJ (block, 0), (char*)MS_BLOCK_OBJ (block, count) - (char*)MS_BLOCK_OBJ (block, 0));
		ms_free_block (block);

		SGEN_ATOMIC_ADD_P (num_major_sections, -1);

		tagged_block = NULL;
	}

 done:
	allocated_blocks.data [block_index] = tagged_block;
	return !!tagged_block;
}

static void
sweep_job_func (void *thread_data_untyped, SgenThreadPoolJob *job)
{
	int block_index;
	int num_blocks = num_major_sections_before_sweep;

	SGEN_ASSERT (0, sweep_in_progress (), "Sweep thread called with wrong state");
	SGEN_ASSERT (0, num_blocks <= allocated_blocks.next_slot, "How did we lose blocks?");

	/*
	 * We traverse the block array from high to low.  Nursery collections will have to
	 * cooperate with the sweep thread to finish sweeping, and they will traverse from
	 * low to high, to avoid constantly colliding on the same blocks.
	 */
	for (block_index = num_blocks - 1; block_index >= 0; --block_index) {
		gboolean have_checked;

		/*
		 * The block might have been freed by another thread doing some checking
		 * work.
		 */
		if (!ensure_block_is_checked_for_sweeping (block_index, TRUE, &have_checked))
			++num_major_sections_freed_in_sweep;
	}

	while (!try_set_sweep_state (SWEEP_STATE_COMPACTING, SWEEP_STATE_SWEEPING)) {
		/*
		 * The main GC thread is currently iterating over the block array to help us
		 * finish the sweep.  We have already finished, but we don't want to mess up
		 * that iteration, so we just wait for it.
		 */
		g_usleep (100);
	}

	if (SGEN_MAX_ASSERT_LEVEL >= 6) {
		for (block_index = num_blocks; block_index < allocated_blocks.next_slot; ++block_index) {
			MSBlockInfo *block = BLOCK_UNTAG (allocated_blocks.data [block_index]);
			SGEN_ASSERT (6, block && block->state == BLOCK_STATE_SWEPT, "How did a new block to be swept get added while swept?");
		}
	}

	sgen_pointer_queue_remove_nulls (&allocated_blocks);
	mono_memory_barrier ();

	sweep_finish ();

	sweep_job = NULL;
}

static void
sweep_finish (void)
{
	int i;

	for (i = 0; i < num_block_obj_sizes; ++i) {
		float usage = (float)sweep_slots_used [i] / (float)sweep_slots_available [i];
		if (sweep_num_blocks [i] > 5 && usage < evacuation_threshold) {
			evacuate_block_obj_sizes [i] = TRUE;
			/*
			g_print ("slot size %d - %d of %d used\n",
					block_obj_sizes [i], slots_used [i], slots_available [i]);
			*/
		} else {
			evacuate_block_obj_sizes [i] = FALSE;
		}
	}

	set_sweep_state (SWEEP_STATE_SWEPT, SWEEP_STATE_COMPACTING);
}

static void
major_sweep (void)
{
	set_sweep_state (SWEEP_STATE_SWEEPING, SWEEP_STATE_NEED_SWEEPING);

	sweep_start ();

	SGEN_ASSERT (0, num_major_sections == allocated_blocks.next_slot, "We don't know how many blocks we have?");

	num_major_sections_before_sweep = num_major_sections;
	num_major_sections_freed_in_sweep = 0;

	SGEN_ASSERT (0, !sweep_job, "We haven't finished the last sweep?");
	if (concurrent_sweep) {
		sweep_job = sgen_thread_pool_job_alloc ("sweep", sweep_job_func, sizeof (SgenThreadPoolJob));
		sgen_thread_pool_job_enqueue (sweep_job);
	} else {
		sweep_job_func (NULL, NULL);
	}
}

static gboolean
major_have_swept (void)
{
	return sweep_state == SWEEP_STATE_SWEPT;
}

static int count_pinned_ref;
static int count_pinned_nonref;
static int count_nonpinned_ref;
static int count_nonpinned_nonref;

static void
count_nonpinned_callback (GCObject *obj, size_t size, void *data)
{
	GCVTable vtable = LOAD_VTABLE (obj);

	if (SGEN_VTABLE_HAS_REFERENCES (vtable))
		++count_nonpinned_ref;
	else
		++count_nonpinned_nonref;
}

static void
count_pinned_callback (GCObject *obj, size_t size, void *data)
{
	GCVTable vtable = LOAD_VTABLE (obj);

	if (SGEN_VTABLE_HAS_REFERENCES (vtable))
		++count_pinned_ref;
	else
		++count_pinned_nonref;
}

static G_GNUC_UNUSED void
count_ref_nonref_objs (void)
{
	int total;

	count_pinned_ref = 0;
	count_pinned_nonref = 0;
	count_nonpinned_ref = 0;
	count_nonpinned_nonref = 0;

	major_iterate_objects (ITERATE_OBJECTS_SWEEP_NON_PINNED, count_nonpinned_callback, NULL);
	major_iterate_objects (ITERATE_OBJECTS_SWEEP_PINNED, count_pinned_callback, NULL);

	total = count_pinned_nonref + count_nonpinned_nonref + count_pinned_ref + count_nonpinned_ref;

	g_print ("ref: %d pinned %d non-pinned   non-ref: %d pinned %d non-pinned  --  %.1f\n",
			count_pinned_ref, count_nonpinned_ref,
			count_pinned_nonref, count_nonpinned_nonref,
			(count_pinned_nonref + count_nonpinned_nonref) * 100.0 / total);
}

static int
ms_calculate_block_obj_sizes (double factor, int *arr)
{
	double target_size;
	int num_sizes = 0;
	int last_size = 0;

	/*
	 * Have every possible slot size starting with the minimal
	 * object size up to and including four times that size.  Then
	 * proceed by increasing geometrically with the given factor.
	 */

	for (int size = SGEN_CLIENT_MINIMUM_OBJECT_SIZE; size <= 4 * SGEN_CLIENT_MINIMUM_OBJECT_SIZE; size += SGEN_ALLOC_ALIGN) {
		if (arr)
			arr [num_sizes] = size;
		++num_sizes;
		last_size = size;
	}
	target_size = (double)last_size;

	do {
		int target_count = (int)floor (MS_BLOCK_FREE / target_size);
		int size = MIN ((MS_BLOCK_FREE / target_count) & ~(SGEN_ALLOC_ALIGN - 1), SGEN_MAX_SMALL_OBJ_SIZE);

		if (size != last_size) {
			if (arr)
				arr [num_sizes] = size;
			++num_sizes;
			last_size = size;
		}

		target_size *= factor;
	} while (last_size < SGEN_MAX_SMALL_OBJ_SIZE);

	return num_sizes;
}

/* only valid during minor collections */
static mword old_num_major_sections;

static void
major_start_nursery_collection (void)
{
#ifdef MARKSWEEP_CONSISTENCY_CHECK
	consistency_check ();
#endif

	old_num_major_sections = num_major_sections;
}

static void
major_finish_nursery_collection (void)
{
#ifdef MARKSWEEP_CONSISTENCY_CHECK
	consistency_check ();
#endif
}

static void
major_start_major_collection (void)
{
	MSBlockInfo *block;
	int i;

	major_finish_sweep_checking ();

	/*
	 * Clear the free lists for block sizes where we do evacuation.  For those block
	 * sizes we will have to allocate new blocks.
	 */
	for (i = 0; i < num_block_obj_sizes; ++i) {
		if (!evacuate_block_obj_sizes [i])
			continue;

		binary_protocol_evacuating_blocks (block_obj_sizes [i]);

		free_block_lists [0][i] = NULL;
		free_block_lists [MS_BLOCK_FLAG_REFS][i] = NULL;
	}

	if (lazy_sweep)
		binary_protocol_sweep_begin (GENERATION_OLD, TRUE);

	/* Sweep all unswept blocks and set them to MARKING */
	FOREACH_BLOCK_NO_LOCK (block) {
		if (lazy_sweep)
			sweep_block (block);
		SGEN_ASSERT (0, block->state == BLOCK_STATE_SWEPT, "All blocks must be swept when we're pinning.");
		set_block_state (block, BLOCK_STATE_MARKING, BLOCK_STATE_SWEPT);
	} END_FOREACH_BLOCK_NO_LOCK;

	if (lazy_sweep)
		binary_protocol_sweep_end (GENERATION_OLD, TRUE);

	set_sweep_state (SWEEP_STATE_NEED_SWEEPING, SWEEP_STATE_SWEPT);
}

static void
major_finish_major_collection (ScannedObjectCounts *counts)
{
#ifdef SGEN_HEAVY_BINARY_PROTOCOL
	if (binary_protocol_is_enabled ()) {
		counts->num_scanned_objects = scanned_objects_list.next_slot;

		sgen_pointer_queue_sort_uniq (&scanned_objects_list);
		counts->num_unique_scanned_objects = scanned_objects_list.next_slot;

		sgen_pointer_queue_clear (&scanned_objects_list);
	}
#endif
}

#if SIZEOF_VOID_P != 8
static int
compare_pointers (const void *va, const void *vb) {
	char *a = *(char**)va, *b = *(char**)vb;
	if (a < b)
		return -1;
	if (a > b)
		return 1;
	return 0;
}
#endif

/*
 * This is called with sweep completed and the world stopped.
 */
static void
major_free_swept_blocks (size_t allowance)
{
	/* FIXME: This is probably too much.  It's assuming all objects are small. */
	size_t section_reserve = allowance / MS_BLOCK_SIZE;

	SGEN_ASSERT (0, sweep_state == SWEEP_STATE_SWEPT, "Sweeping must have finished before freeing blocks");

#ifdef TARGET_WIN32
		/*
		 * sgen_free_os_memory () asserts in mono_vfree () because windows doesn't like freeing the middle of
		 * a VirtualAlloc ()-ed block.
		 */
		return;
#endif

#if SIZEOF_VOID_P != 8
	{
		int i, num_empty_blocks_orig, num_blocks, arr_length;
		void *block;
		void **empty_block_arr;
		void **rebuild_next;

		if (num_empty_blocks <= section_reserve)
			return;
		SGEN_ASSERT (0, num_empty_blocks > 0, "section reserve can't be negative");

		num_empty_blocks_orig = num_empty_blocks;
		empty_block_arr = (void**)sgen_alloc_internal_dynamic (sizeof (void*) * num_empty_blocks_orig,
				INTERNAL_MEM_MS_BLOCK_INFO_SORT, FALSE);
		if (!empty_block_arr)
			goto fallback;

		i = 0;
		for (block = empty_blocks; block; block = *(void**)block)
			empty_block_arr [i++] = block;
		SGEN_ASSERT (0, i == num_empty_blocks, "empty block count wrong");

		sgen_qsort (empty_block_arr, num_empty_blocks, sizeof (void*), compare_pointers);

		/*
		 * We iterate over the free blocks, trying to find MS_BLOCK_ALLOC_NUM
		 * contiguous ones.  If we do, we free them.  If that's not enough to get to
		 * section_reserve, we halve the number of contiguous blocks we're looking
		 * for and have another go, until we're done with looking for pairs of
		 * blocks, at which point we give up and go to the fallback.
		 */
		arr_length = num_empty_blocks_orig;
		num_blocks = MS_BLOCK_ALLOC_NUM;
		while (num_empty_blocks > section_reserve && num_blocks > 1) {
			int first = -1;
			int dest = 0;

			dest = 0;
			for (i = 0; i < arr_length; ++i) {
				int d = dest;
				void *block = empty_block_arr [i];
				SGEN_ASSERT (6, block, "we're not shifting correctly");
				if (i != dest) {
					empty_block_arr [dest] = block;
					/*
					 * This is not strictly necessary, but we're
					 * cautious.
					 */
					empty_block_arr [i] = NULL;
				}
				++dest;

				if (first < 0) {
					first = d;
					continue;
				}

				SGEN_ASSERT (6, first >= 0 && d > first, "algorithm is wrong");

				if ((char*)block != ((char*)empty_block_arr [d-1]) + MS_BLOCK_SIZE) {
					first = d;
					continue;
				}

				if (d + 1 - first == num_blocks) {
					/*
					 * We found num_blocks contiguous blocks.  Free them
					 * and null their array entries.  As an optimization
					 * we could, instead of nulling the entries, shift
					 * the following entries over to the left, while
					 * we're iterating.
					 */
					int j;
					sgen_free_os_memory (empty_block_arr [first], MS_BLOCK_SIZE * num_blocks, SGEN_ALLOC_HEAP);
					for (j = first; j <= d; ++j)
						empty_block_arr [j] = NULL;
					dest = first;
					first = -1;

					num_empty_blocks -= num_blocks;

					stat_major_blocks_freed += num_blocks;
					if (num_blocks == MS_BLOCK_ALLOC_NUM)
						stat_major_blocks_freed_ideal += num_blocks;
					else
						stat_major_blocks_freed_less_ideal += num_blocks;

				}
			}

			SGEN_ASSERT (6, dest <= i && dest <= arr_length, "array length is off");
			arr_length = dest;
			SGEN_ASSERT (6, arr_length == num_empty_blocks, "array length is off");

			num_blocks >>= 1;
		}

		/* rebuild empty_blocks free list */
		rebuild_next = (void**)&empty_blocks;
		for (i = 0; i < arr_length; ++i) {
			void *block = empty_block_arr [i];
			SGEN_ASSERT (6, block, "we're missing blocks");
			*rebuild_next = block;
			rebuild_next = (void**)block;
		}
		*rebuild_next = NULL;

		/* free array */
		sgen_free_internal_dynamic (empty_block_arr, sizeof (void*) * num_empty_blocks_orig, INTERNAL_MEM_MS_BLOCK_INFO_SORT);
	}

	SGEN_ASSERT (0, num_empty_blocks >= 0, "we freed more blocks than we had in the first place?");

 fallback:
	/*
	 * This is our threshold.  If there's not more empty than used blocks, we won't
	 * release uncontiguous blocks, in fear of fragmenting the address space.
	 */
	if (num_empty_blocks <= num_major_sections)
		return;
#endif

	while (num_empty_blocks > section_reserve) {
		void *next = *(void**)empty_blocks;
		sgen_free_os_memory (empty_blocks, MS_BLOCK_SIZE, SGEN_ALLOC_HEAP);
		empty_blocks = next;
		/*
		 * Needs not be atomic because this is running
		 * single-threaded.
		 */
		--num_empty_blocks;

		++stat_major_blocks_freed;
#if SIZEOF_VOID_P != 8
		++stat_major_blocks_freed_individual;
#endif
	}
}

static void
major_pin_objects (SgenGrayQueue *queue)
{
	MSBlockInfo *block;

	FOREACH_BLOCK_NO_LOCK (block) {
		size_t first_entry, last_entry;
		SGEN_ASSERT (6, block_is_swept_or_marking (block), "All blocks must be swept when we're pinning.");
		sgen_find_optimized_pin_queue_area (MS_BLOCK_FOR_BLOCK_INFO (block) + MS_BLOCK_SKIP, MS_BLOCK_FOR_BLOCK_INFO (block) + MS_BLOCK_SIZE,
				&first_entry, &last_entry);
		mark_pinned_objects_in_block (block, first_entry, last_entry, queue);
	} END_FOREACH_BLOCK_NO_LOCK;
}

static void
major_init_to_space (void)
{
}

static void
major_report_pinned_memory_usage (void)
{
	g_assert_not_reached ();
}

static gint64
major_get_used_size (void)
{
	gint64 size = 0;
	MSBlockInfo *block;

	/*
	 * We're holding the GC lock, but the sweep thread might be running.  Make sure it's
	 * finished, then we can iterate over the block array.
	 */
	major_finish_sweep_checking ();

	FOREACH_BLOCK_NO_LOCK_CONDITION (TRUE, block) {
		int count = MS_BLOCK_FREE / block->obj_size;
		void **iter;
		size += count * block->obj_size;
		for (iter = block->free_list; iter; iter = (void**)*iter)
			size -= block->obj_size;
	} END_FOREACH_BLOCK_NO_LOCK;

	return size;
}

/* FIXME: return number of bytes, not of sections */
static size_t
get_num_major_sections (void)
{
	return num_major_sections;
}

/*
 * Returns the number of bytes in blocks that were present when the last sweep was
 * initiated, and were not freed during the sweep.  They are the basis for calculating the
 * allowance.
 */
static size_t
get_bytes_survived_last_sweep (void)
{
	SGEN_ASSERT (0, sweep_state == SWEEP_STATE_SWEPT, "Can only query unswept sections after sweep");
	return (num_major_sections_before_sweep - num_major_sections_freed_in_sweep) * MS_BLOCK_SIZE;
}

static gboolean
major_handle_gc_param (const char *opt)
{
	if (g_str_has_prefix (opt, "evacuation-threshold=")) {
		const char *arg = strchr (opt, '=') + 1;
		int percentage = atoi (arg);
		if (percentage < 0 || percentage > 100) {
			fprintf (stderr, "evacuation-threshold must be an integer in the range 0-100.\n");
			exit (1);
		}
		evacuation_threshold = (float)percentage / 100.0f;
		return TRUE;
	} else if (!strcmp (opt, "lazy-sweep")) {
		lazy_sweep = TRUE;
		return TRUE;
	} else if (!strcmp (opt, "no-lazy-sweep")) {
		lazy_sweep = FALSE;
		return TRUE;
	} else if (!strcmp (opt, "concurrent-sweep")) {
		concurrent_sweep = TRUE;
		return TRUE;
	} else if (!strcmp (opt, "no-concurrent-sweep")) {
		concurrent_sweep = FALSE;
		return TRUE;
	}

	return FALSE;
}

static void
major_print_gc_param_usage (void)
{
	fprintf (stderr,
			""
			"  evacuation-threshold=P (where P is a percentage, an integer in 0-100)\n"
			"  (no-)lazy-sweep\n"
			"  (no-)concurrent-sweep\n"
			);
}

/*
 * This callback is used to clear cards, move cards to the shadow table and do counting.
 */
static void
major_iterate_live_block_ranges (sgen_cardtable_block_callback callback)
{
	MSBlockInfo *block;
	gboolean has_references;

	major_finish_sweep_checking ();
	FOREACH_BLOCK_HAS_REFERENCES_NO_LOCK (block, has_references) {
		if (has_references)
			callback ((mword)MS_BLOCK_FOR_BLOCK_INFO (block), MS_BLOCK_SIZE);
	} END_FOREACH_BLOCK_NO_LOCK;
}

#ifdef HEAVY_STATISTICS
extern guint64 marked_cards;
extern guint64 scanned_cards;
extern guint64 scanned_objects;
extern guint64 remarked_cards;
#endif

#define CARD_WORDS_PER_BLOCK (CARDS_PER_BLOCK / SIZEOF_VOID_P)
/*
 * MS blocks are 16K aligned.
 * Cardtables are 4K aligned, at least.
 * This means that the cardtable of a given block is 32 bytes aligned.
 */
static guint8*
initial_skip_card (guint8 *card_data)
{
	mword *cards = (mword*)card_data;
	mword card;
	int i;
	for (i = 0; i < CARD_WORDS_PER_BLOCK; ++i) {
		card = cards [i];
		if (card)
			break;
	}

	if (i == CARD_WORDS_PER_BLOCK)
		return card_data + CARDS_PER_BLOCK;

#if defined(__i386__) && defined(__GNUC__)
	return card_data + i * 4 +  (__builtin_ffs (card) - 1) / 8;
#elif defined(__x86_64__) && defined(__GNUC__)
	return card_data + i * 8 +  (__builtin_ffsll (card) - 1) / 8;
#elif defined(__s390x__) && defined(__GNUC__)
	return card_data + i * 8 +  (__builtin_ffsll (GUINT64_TO_LE(card)) - 1) / 8;
#else
	for (i = i * SIZEOF_VOID_P; i < CARDS_PER_BLOCK; ++i) {
		if (card_data [i])
			return &card_data [i];
	}
	return card_data;
#endif
}

#define MS_BLOCK_OBJ_INDEX_FAST(o,b,os)	(((char*)(o) - ((b) + MS_BLOCK_SKIP)) / (os))
#define MS_BLOCK_OBJ_FAST(b,os,i)			((b) + MS_BLOCK_SKIP + (os) * (i))
#define MS_OBJ_ALLOCED_FAST(o,b)		(*(void**)(o) && (*(char**)(o) < (b) || *(char**)(o) >= (b) + MS_BLOCK_SIZE))

static void
scan_card_table_for_block (MSBlockInfo *block, gboolean mod_union, ScanCopyContext ctx)
{
	SgenGrayQueue *queue = ctx.queue;
	ScanObjectFunc scan_func = ctx.ops->scan_object;
#ifndef SGEN_HAVE_OVERLAPPING_CARDS
	guint8 cards_copy [CARDS_PER_BLOCK];
#endif
	gboolean small_objects;
	int block_obj_size;
	char *block_start;
	guint8 *card_data, *card_base;
	guint8 *card_data_end;
	char *scan_front = NULL;

	block_obj_size = block->obj_size;
	small_objects = block_obj_size < CARD_SIZE_IN_BYTES;

	block_start = MS_BLOCK_FOR_BLOCK_INFO (block);

	/*
	 * This is safe in face of card aliasing for the following reason:
	 *
	 * Major blocks are 16k aligned, or 32 cards aligned.
	 * Cards aliasing happens in powers of two, so as long as major blocks are aligned to their
	 * sizes, they won't overflow the cardtable overlap modulus.
	 */
	if (mod_union) {
		card_data = card_base = block->cardtable_mod_union;
		/*
		 * This happens when the nursery collection that precedes finishing
		 * the concurrent collection allocates new major blocks.
		 */
		if (!card_data)
			return;
	} else {
#ifdef SGEN_HAVE_OVERLAPPING_CARDS
		card_data = card_base = sgen_card_table_get_card_scan_address ((mword)block_start);
#else
		if (!sgen_card_table_get_card_data (cards_copy, (mword)block_start, CARDS_PER_BLOCK))
			return;
		card_data = card_base = cards_copy;
#endif
	}
	card_data_end = card_data + CARDS_PER_BLOCK;

	card_data += MS_BLOCK_SKIP >> CARD_BITS;

	card_data = initial_skip_card (card_data);
	while (card_data < card_data_end) {
		size_t card_index, first_object_index;
		char *start;
		char *end;
		char *first_obj, *obj;

		HEAVY_STAT (++scanned_cards);

		if (!*card_data) {
			++card_data;
			continue;
		}

		card_index = card_data - card_base;
		start = (char*)(block_start + card_index * CARD_SIZE_IN_BYTES);
		end = start + CARD_SIZE_IN_BYTES;

		if (!block_is_swept_or_marking (block))
			sweep_block (block);

		HEAVY_STAT (++marked_cards);

		if (small_objects)
			sgen_card_table_prepare_card_for_scanning (card_data);

		/*
		 * If the card we're looking at starts at or in the block header, we
		 * must start at the first object in the block, without calculating
		 * the index of the object we're hypothetically starting at, because
		 * it would be negative.
		 */
		if (card_index <= (MS_BLOCK_SKIP >> CARD_BITS))
			first_object_index = 0;
		else
			first_object_index = MS_BLOCK_OBJ_INDEX_FAST (start, block_start, block_obj_size);

		obj = first_obj = (char*)MS_BLOCK_OBJ_FAST (block_start, block_obj_size, first_object_index);

		binary_protocol_card_scan (first_obj, end - first_obj);

		while (obj < end) {
			if (obj < scan_front || !MS_OBJ_ALLOCED_FAST (obj, block_start))
				goto next_object;

			if (mod_union) {
				/* FIXME: do this more efficiently */
				int w, b;
				MS_CALC_MARK_BIT (w, b, obj);
				if (!MS_MARK_BIT (block, w, b))
					goto next_object;
			}

			GCObject *object = (GCObject*)obj;

			if (small_objects) {
				HEAVY_STAT (++scanned_objects);
				scan_func (object, sgen_obj_get_descriptor (object), queue);
			} else {
				size_t offset = sgen_card_table_get_card_offset (obj, block_start);
				sgen_cardtable_scan_object (object, block_obj_size, card_base + offset, mod_union, ctx);
			}
		next_object:
			obj += block_obj_size;
			g_assert (scan_front <= obj);
			scan_front = obj;
		}

		HEAVY_STAT (if (*card_data) ++remarked_cards);

		if (small_objects)
			++card_data;
		else
			card_data = card_base + sgen_card_table_get_card_offset (obj, block_start);
	}
}

static void
major_scan_card_table (gboolean mod_union, ScanCopyContext ctx)
{
	MSBlockInfo *block;
	gboolean has_references;

	if (!concurrent_mark)
		g_assert (!mod_union);

	major_finish_sweep_checking ();
	binary_protocol_major_card_table_scan_start (sgen_timestamp (), mod_union);
	FOREACH_BLOCK_HAS_REFERENCES_NO_LOCK (block, has_references) {
#ifdef PREFETCH_CARDS
		int prefetch_index = __index + 6;
		if (prefetch_index < allocated_blocks.next_slot) {
			MSBlockInfo *prefetch_block = BLOCK_UNTAG (allocated_blocks.data [prefetch_index]);
			guint8 *prefetch_cards = sgen_card_table_get_card_scan_address ((mword)MS_BLOCK_FOR_BLOCK_INFO (prefetch_block));
			PREFETCH_READ (prefetch_block);
			PREFETCH_WRITE (prefetch_cards);
			PREFETCH_WRITE (prefetch_cards + 32);
                }
#endif

		if (!has_references)
			continue;

		scan_card_table_for_block (block, mod_union, ctx);
	} END_FOREACH_BLOCK_NO_LOCK;
	binary_protocol_major_card_table_scan_end (sgen_timestamp (), mod_union);
}

static void
major_count_cards (long long *num_total_cards, long long *num_marked_cards)
{
	MSBlockInfo *block;
	gboolean has_references;
	long long total_cards = 0;
	long long marked_cards = 0;

	if (sweep_in_progress ()) {
		*num_total_cards = -1;
		*num_marked_cards = -1;
		return;
	}

	FOREACH_BLOCK_HAS_REFERENCES_NO_LOCK (block, has_references) {
		guint8 *cards = sgen_card_table_get_card_scan_address ((mword) MS_BLOCK_FOR_BLOCK_INFO (block));
		int i;

		if (!has_references)
			continue;

		total_cards += CARDS_PER_BLOCK;
		for (i = 0; i < CARDS_PER_BLOCK; ++i) {
			if (cards [i])
				++marked_cards;
		}
	} END_FOREACH_BLOCK_NO_LOCK;

	*num_total_cards = total_cards;
	*num_marked_cards = marked_cards;
}

static void
update_cardtable_mod_union (void)
{
	MSBlockInfo *block;

	FOREACH_BLOCK_NO_LOCK (block) {
		size_t num_cards;
		guint8 *mod_union = get_cardtable_mod_union_for_block (block, TRUE);
		sgen_card_table_update_mod_union (mod_union, MS_BLOCK_FOR_BLOCK_INFO (block), MS_BLOCK_SIZE, &num_cards);
		SGEN_ASSERT (6, num_cards == CARDS_PER_BLOCK, "Number of cards calculation is wrong");
	} END_FOREACH_BLOCK_NO_LOCK;
}

#undef pthread_create

static void
post_param_init (SgenMajorCollector *collector)
{
	collector->sweeps_lazily = lazy_sweep;
	collector->needs_thread_pool = concurrent_mark || concurrent_sweep;
}

static void
sgen_marksweep_init_internal (SgenMajorCollector *collector, gboolean is_concurrent)
{
	int i;

	sgen_register_fixed_internal_mem_type (INTERNAL_MEM_MS_BLOCK_INFO, sizeof (MSBlockInfo));

	num_block_obj_sizes = ms_calculate_block_obj_sizes (MS_BLOCK_OBJ_SIZE_FACTOR, NULL);
	block_obj_sizes = (int *)sgen_alloc_internal_dynamic (sizeof (int) * num_block_obj_sizes, INTERNAL_MEM_MS_TABLES, TRUE);
	ms_calculate_block_obj_sizes (MS_BLOCK_OBJ_SIZE_FACTOR, block_obj_sizes);

	evacuate_block_obj_sizes = (gboolean *)sgen_alloc_internal_dynamic (sizeof (gboolean) * num_block_obj_sizes, INTERNAL_MEM_MS_TABLES, TRUE);
	for (i = 0; i < num_block_obj_sizes; ++i)
		evacuate_block_obj_sizes [i] = FALSE;

	sweep_slots_available = (size_t *)sgen_alloc_internal_dynamic (sizeof (size_t) * num_block_obj_sizes, INTERNAL_MEM_MS_TABLES, TRUE);
	sweep_slots_used = (size_t *)sgen_alloc_internal_dynamic (sizeof (size_t) * num_block_obj_sizes, INTERNAL_MEM_MS_TABLES, TRUE);
	sweep_num_blocks = (size_t *)sgen_alloc_internal_dynamic (sizeof (size_t) * num_block_obj_sizes, INTERNAL_MEM_MS_TABLES, TRUE);

	/*
	{
		int i;
		g_print ("block object sizes:\n");
		for (i = 0; i < num_block_obj_sizes; ++i)
			g_print ("%d\n", block_obj_sizes [i]);
	}
	*/

	for (i = 0; i < MS_BLOCK_TYPE_MAX; ++i)
		free_block_lists [i] = (MSBlockInfo *volatile *)sgen_alloc_internal_dynamic (sizeof (MSBlockInfo*) * num_block_obj_sizes, INTERNAL_MEM_MS_TABLES, TRUE);

	for (i = 0; i < MS_NUM_FAST_BLOCK_OBJ_SIZE_INDEXES; ++i)
		fast_block_obj_size_indexes [i] = ms_find_block_obj_size_index (i * 8);
	for (i = 0; i < MS_NUM_FAST_BLOCK_OBJ_SIZE_INDEXES * 8; ++i)
		g_assert (MS_BLOCK_OBJ_SIZE_INDEX (i) == ms_find_block_obj_size_index (i));

	mono_counters_register ("# major blocks allocated", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_major_blocks_alloced);
	mono_counters_register ("# major blocks freed", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_major_blocks_freed);
	mono_counters_register ("# major blocks lazy swept", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_major_blocks_lazy_swept);
#if SIZEOF_VOID_P != 8
	mono_counters_register ("# major blocks freed ideally", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_major_blocks_freed_ideal);
	mono_counters_register ("# major blocks freed less ideally", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_major_blocks_freed_less_ideal);
	mono_counters_register ("# major blocks freed individually", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_major_blocks_freed_individual);
	mono_counters_register ("# major blocks allocated less ideally", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_major_blocks_alloced_less_ideal);
#endif

	collector->section_size = MAJOR_SECTION_SIZE;

	concurrent_mark = is_concurrent;
	collector->is_concurrent = is_concurrent;
	collector->needs_thread_pool = is_concurrent || concurrent_sweep;
	collector->get_and_reset_num_major_objects_marked = major_get_and_reset_num_major_objects_marked;
	collector->supports_cardtable = TRUE;

	collector->alloc_heap = major_alloc_heap;
	collector->is_object_live = major_is_object_live;
	collector->alloc_small_pinned_obj = major_alloc_small_pinned_obj;
	collector->alloc_degraded = major_alloc_degraded;

	collector->alloc_object = major_alloc_object;
	collector->free_pinned_object = free_pinned_object;
	collector->iterate_objects = major_iterate_objects;
	collector->free_non_pinned_object = major_free_non_pinned_object;
	collector->pin_objects = major_pin_objects;
	collector->pin_major_object = pin_major_object;
	collector->scan_card_table = major_scan_card_table;
	collector->iterate_live_block_ranges = major_iterate_live_block_ranges;
	if (is_concurrent) {
		collector->update_cardtable_mod_union = update_cardtable_mod_union;
		collector->get_cardtable_mod_union_for_reference = major_get_cardtable_mod_union_for_reference;
	}
	collector->init_to_space = major_init_to_space;
	collector->sweep = major_sweep;
	collector->have_swept = major_have_swept;
	collector->finish_sweeping = major_finish_sweep_checking;
	collector->free_swept_blocks = major_free_swept_blocks;
	collector->check_scan_starts = major_check_scan_starts;
	collector->dump_heap = major_dump_heap;
	collector->get_used_size = major_get_used_size;
	collector->start_nursery_collection = major_start_nursery_collection;
	collector->finish_nursery_collection = major_finish_nursery_collection;
	collector->start_major_collection = major_start_major_collection;
	collector->finish_major_collection = major_finish_major_collection;
	collector->ptr_is_in_non_pinned_space = major_ptr_is_in_non_pinned_space;
	collector->ptr_is_from_pinned_alloc = ptr_is_from_pinned_alloc;
	collector->report_pinned_memory_usage = major_report_pinned_memory_usage;
	collector->get_num_major_sections = get_num_major_sections;
	collector->get_bytes_survived_last_sweep = get_bytes_survived_last_sweep;
	collector->handle_gc_param = major_handle_gc_param;
	collector->print_gc_param_usage = major_print_gc_param_usage;
	collector->post_param_init = post_param_init;
	collector->is_valid_object = major_is_valid_object;
	collector->describe_pointer = major_describe_pointer;
	collector->count_cards = major_count_cards;

	collector->major_ops_serial.copy_or_mark_object = major_copy_or_mark_object_canonical;
	collector->major_ops_serial.scan_object = major_scan_object_with_evacuation;
	collector->major_ops_serial.drain_gray_stack = drain_gray_stack;
	if (is_concurrent) {
		collector->major_ops_concurrent_start.copy_or_mark_object = major_copy_or_mark_object_concurrent_canonical;
		collector->major_ops_concurrent_start.scan_object = major_scan_object_concurrent_with_evacuation;
		collector->major_ops_concurrent_start.drain_gray_stack = drain_gray_stack_concurrent;

		collector->major_ops_concurrent_finish.copy_or_mark_object = major_copy_or_mark_object_concurrent_finish_canonical;
		collector->major_ops_concurrent_finish.scan_object = major_scan_object_with_evacuation;
		collector->major_ops_concurrent_finish.scan_vtype = major_scan_vtype_concurrent_finish;
		collector->major_ops_concurrent_finish.drain_gray_stack = drain_gray_stack;
	}

#ifdef HEAVY_STATISTICS
	mono_counters_register ("Optimized copy", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_optimized_copy);
	mono_counters_register ("Optimized copy nursery", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_optimized_copy_nursery);
	mono_counters_register ("Optimized copy nursery forwarded", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_optimized_copy_nursery_forwarded);
	mono_counters_register ("Optimized copy nursery pinned", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_optimized_copy_nursery_pinned);
	mono_counters_register ("Optimized copy major", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_optimized_copy_major);
	mono_counters_register ("Optimized copy major small fast", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_optimized_copy_major_small_fast);
	mono_counters_register ("Optimized copy major small slow", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_optimized_copy_major_small_slow);
	mono_counters_register ("Optimized copy major small evacuate", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_optimized_copy_major_small_evacuate);
	mono_counters_register ("Optimized copy major large", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_optimized_copy_major_large);
	mono_counters_register ("Optimized major scan", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_optimized_major_scan);
	mono_counters_register ("Optimized major scan no refs", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_optimized_major_scan_no_refs);

	mono_counters_register ("Gray stack drain loops", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_drain_loops);
	mono_counters_register ("Gray stack prefetch fills", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_drain_prefetch_fills);
	mono_counters_register ("Gray stack prefetch failures", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_drain_prefetch_fill_failures);
#endif

#ifdef SGEN_HEAVY_BINARY_PROTOCOL
	mono_os_mutex_init (&scanned_objects_list_lock);
#endif

	SGEN_ASSERT (0, SGEN_MAX_SMALL_OBJ_SIZE <= MS_BLOCK_FREE / 2, "MAX_SMALL_OBJ_SIZE must be at most MS_BLOCK_FREE / 2");

	/*cardtable requires major pages to be 8 cards aligned*/
	g_assert ((MS_BLOCK_SIZE % (8 * CARD_SIZE_IN_BYTES)) == 0);
}

void
sgen_marksweep_init (SgenMajorCollector *collector)
{
	sgen_marksweep_init_internal (collector, FALSE);
}

void
sgen_marksweep_conc_init (SgenMajorCollector *collector)
{
	sgen_marksweep_init_internal (collector, TRUE);
}

#endif
