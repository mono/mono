/*
 * sgen-internal.c: Internal lock-free memory allocator.
 *
 * Copyright (C) 2012 Xamarin Inc
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include "config.h"

#ifdef HAVE_SGEN_GC

#include <string.h>

#include "mono/sgen/sgen-gc.h"
#include "mono/utils/lock-free-alloc.h"
#include "mono/sgen/sgen-memory-governor.h"
#include "mono/sgen/sgen-client.h"

/* keep each size a multiple of ALLOC_ALIGN */
#if SIZEOF_VOID_P == 4
static const int allocator_sizes [] = {
	   8,   16,   24,   32,   40,   48,   64,   80,
	  96,  128,  160,  192,  224,  248,  296,  320,
	 384,  448,  504,  528,  584,  680,  816, 1088,
	1360, 2044, 2336, 2728, 3272, 4092, 5456, 8188 };
#else
static const int allocator_sizes [] = {
	   8,   16,   24,   32,   40,   48,   64,   80,
	  96,  128,  160,  192,  224,  248,  320,  328,
	 384,  448,  528,  584,  680,  816, 1016, 1088,
	1360, 2040, 2336, 2728, 3272, 4088, 5456, 8184 };
#endif

#define NUM_ALLOCATORS	(sizeof (allocator_sizes) / sizeof (int))

static int allocator_block_sizes [NUM_ALLOCATORS];

static MonoLockFreeAllocSizeClass size_classes [NUM_ALLOCATORS];
static MonoLockFreeAllocator allocators [NUM_ALLOCATORS];

#ifdef HEAVY_STATISTICS
static int allocator_sizes_stats [NUM_ALLOCATORS];
#endif

static size_t
block_size (size_t slot_size)
{
	static int pagesize = -1;

	int size;

	if (pagesize == -1)
		pagesize = mono_pagesize ();

	for (size = pagesize; size < LOCK_FREE_ALLOC_SB_MAX_SIZE; size <<= 1) {
		if (slot_size * 2 <= LOCK_FREE_ALLOC_SB_USABLE_SIZE (size))
			return size;
	}
	return LOCK_FREE_ALLOC_SB_MAX_SIZE;
}

/*
 * Find the allocator index for memory chunks that can contain @size
 * objects.
 */
static int
index_for_size (size_t size)
{
	int slot;
	/* do a binary search or lookup table later. */
	for (slot = 0; slot < NUM_ALLOCATORS; ++slot) {
		if (allocator_sizes [slot] >= size)
			return slot;
	}
	g_assert_not_reached ();
	return -1;
}

/*
 * Allocator indexes for the fixed INTERNAL_MEM_XXX types.  -1 if that
 * type is dynamic.
 */
static int fixed_type_allocator_indexes [INTERNAL_MEM_MAX];

void
sgen_register_fixed_internal_mem_type (int type, size_t size)
{
	int slot;

	g_assert (type >= 0 && type < INTERNAL_MEM_MAX);
	g_assert (size <= allocator_sizes [NUM_ALLOCATORS - 1]);

	slot = index_for_size (size);
	g_assert (slot >= 0);

	if (fixed_type_allocator_indexes [type] == -1)
		fixed_type_allocator_indexes [type] = slot;
	else {
		if (fixed_type_allocator_indexes [type] != slot)
			g_error ("Invalid double registration of type %d old slot %d new slot %d", type, fixed_type_allocator_indexes [type], slot);
	}
}

static const char*
description_for_type (int type)
{
	switch (type) {
	case INTERNAL_MEM_PIN_QUEUE: return "sgen:pin-queue";
	case INTERNAL_MEM_FRAGMENT: return "sgen:fragment";
	case INTERNAL_MEM_SECTION: return "sgen:section";
	case INTERNAL_MEM_SCAN_STARTS: return "sgen:scan-starts";
	case INTERNAL_MEM_FIN_TABLE: return "sgen:fin-table";
	case INTERNAL_MEM_FINALIZE_ENTRY: return "sgen:finalize-entry";
	case INTERNAL_MEM_FINALIZE_READY: return "sgen:finalize-ready";
	case INTERNAL_MEM_DISLINK_TABLE: return "sgen:dislink-table";
	case INTERNAL_MEM_DISLINK: return "sgen:dislink";
	case INTERNAL_MEM_ROOTS_TABLE: return "sgen:roots-table";
	case INTERNAL_MEM_ROOT_RECORD: return "sgen:root-record";
	case INTERNAL_MEM_STATISTICS: return "sgen:statistics";
	case INTERNAL_MEM_STAT_PINNED_CLASS: return "sgen:pinned-class";
	case INTERNAL_MEM_STAT_REMSET_CLASS: return "sgen:remset-class";
	case INTERNAL_MEM_GRAY_QUEUE: return "sgen:gray-queue";
	case INTERNAL_MEM_MS_TABLES: return "sgen:marksweep-tables";
	case INTERNAL_MEM_MS_BLOCK_INFO: return "sgen:marksweep-block-info";
	case INTERNAL_MEM_MS_BLOCK_INFO_SORT: return "sgen:marksweep-block-info-sort";
	case INTERNAL_MEM_WORKER_DATA: return "sgen:worker-data";
	case INTERNAL_MEM_THREAD_POOL_JOB: return "sgen:thread-pool-job";
	case INTERNAL_MEM_BRIDGE_DATA: return "sgen:bridge-data";
	case INTERNAL_MEM_OLD_BRIDGE_HASH_TABLE: return "sgen:old-bridge-hash-table";
	case INTERNAL_MEM_OLD_BRIDGE_HASH_TABLE_ENTRY: return "sgen:old-bridge-hash-table-entry";
	case INTERNAL_MEM_BRIDGE_HASH_TABLE: return "sgen:bridge-hash-table";
	case INTERNAL_MEM_BRIDGE_HASH_TABLE_ENTRY: return "sgen:bridge-hash-table-entry";
	case INTERNAL_MEM_TARJAN_BRIDGE_HASH_TABLE: return "sgen:tarjan-bridge-hash-table";
	case INTERNAL_MEM_TARJAN_BRIDGE_HASH_TABLE_ENTRY: return "sgen:tarjan-bridge-hash-table-entry";
	case INTERNAL_MEM_TARJAN_OBJ_BUCKET: return "sgen:tarjan-bridge-object-buckets";
	case INTERNAL_MEM_BRIDGE_ALIVE_HASH_TABLE: return "sgen:bridge-alive-hash-table";
	case INTERNAL_MEM_BRIDGE_ALIVE_HASH_TABLE_ENTRY: return "sgen:bridge-alive-hash-table-entry";
	case INTERNAL_MEM_BRIDGE_DEBUG: return "sgen:bridge-debug";
	case INTERNAL_MEM_TOGGLEREF_DATA: return "sgen:toggleref-data";
	case INTERNAL_MEM_CARDTABLE_MOD_UNION: return "sgen:cardtable-mod-union";
	case INTERNAL_MEM_BINARY_PROTOCOL: return "sgen:binary-protocol";
	case INTERNAL_MEM_TEMPORARY: return "sgen:temporary";
	case INTERNAL_MEM_LOG_ENTRY: return "sgen:log-entry";
	case INTERNAL_MEM_COMPLEX_DESCRIPTORS: return "sgen:complex-descriptors";
	default: {
		const char *description = sgen_client_description_for_internal_mem_type (type);
		SGEN_ASSERT (0, description, "Unknown internal mem type");
		return description;
	}
	}
}

void*
sgen_alloc_internal_dynamic (size_t size, int type, gboolean assert_on_failure)
{
	int index;
	void *p;

	if (size > allocator_sizes [NUM_ALLOCATORS - 1]) {
		p = sgen_alloc_os_memory (size, (SgenAllocFlags)(SGEN_ALLOC_INTERNAL | SGEN_ALLOC_ACTIVATE), description_for_type (type));
	} else {
		index = index_for_size (size);

#ifdef HEAVY_STATISTICS
		++ allocator_sizes_stats [index];
#endif

		p = mono_lock_free_alloc (&allocators [index]);
		if (!p)
			sgen_assert_memory_alloc (NULL, size, description_for_type (type), TRUE);
		memset (p, 0, size);
	}
	return p;
}

void
sgen_free_internal_dynamic (void *addr, size_t size, int type)
{
	if (!addr)
		return;

	if (size > allocator_sizes [NUM_ALLOCATORS - 1])
		sgen_free_os_memory (addr, size, SGEN_ALLOC_INTERNAL);
	else
		mono_lock_free_free (addr, block_size (size));
}

void*
sgen_alloc_internal (int type)
{
	int index, size;
	void *p;

	index = fixed_type_allocator_indexes [type];
	g_assert (index >= 0 && index < NUM_ALLOCATORS);

#ifdef HEAVY_STATISTICS
	++ allocator_sizes_stats [index];
#endif

	size = allocator_sizes [index];

	p = mono_lock_free_alloc (&allocators [index]);
	memset (p, 0, size);

	return p;
}

void
sgen_free_internal (void *addr, int type)
{
	int index;

	if (!addr)
		return;

	index = fixed_type_allocator_indexes [type];
	g_assert (index >= 0 && index < NUM_ALLOCATORS);

	mono_lock_free_free (addr, allocator_block_sizes [index]);
}

void
sgen_dump_internal_mem_usage (FILE *heap_dump_file)
{
	/*
	int i;

	fprintf (heap_dump_file, "<other-mem-usage type=\"large-internal\" size=\"%lld\"/>\n", large_internal_bytes_alloced);
	fprintf (heap_dump_file, "<other-mem-usage type=\"pinned-chunks\" size=\"%lld\"/>\n", pinned_chunk_bytes_alloced);
	for (i = 0; i < INTERNAL_MEM_MAX; ++i) {
		fprintf (heap_dump_file, "<other-mem-usage type=\"%s\" size=\"%ld\"/>\n",
				description_for_type (i), unmanaged_allocator.small_internal_mem_bytes [i]);
	}
	*/
}

void
sgen_report_internal_mem_usage (void)
{
	int i G_GNUC_UNUSED;
#ifdef HEAVY_STATISTICS
	printf ("size -> # allocations\n");
	for (i = 0; i < NUM_ALLOCATORS; ++i)
		printf ("%d -> %d\n", allocator_sizes [i], allocator_sizes_stats [i]);
#endif
}

void
sgen_init_internal_allocator (void)
{
	int i, size;

	for (i = 0; i < INTERNAL_MEM_MAX; ++i)
		fixed_type_allocator_indexes [i] = -1;

	for (i = 0; i < NUM_ALLOCATORS; ++i) {
		allocator_block_sizes [i] = block_size (allocator_sizes [i]);
		mono_lock_free_allocator_init_size_class (&size_classes [i], allocator_sizes [i], allocator_block_sizes [i]);
		mono_lock_free_allocator_init_allocator (&allocators [i], &size_classes [i]);
	}

	for (size = mono_pagesize (); size <= LOCK_FREE_ALLOC_SB_MAX_SIZE; size <<= 1) {
		int max_size = LOCK_FREE_ALLOC_SB_USABLE_SIZE (size) / 2;
		/*
		 * we assert that allocator_sizes contains the biggest possible object size
		 * per block (4K => 4080 / 2 = 2040, 8k => 8176 / 2 = 4088, 16k => 16368 / 2 = 8184 on 64bits),
		 * so that we do not get different block sizes for sizes that should go to the same one
		 */
		g_assert (allocator_sizes [index_for_size (max_size)] == max_size);
	}
}

#endif
