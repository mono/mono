/*
 * sgen-memory-governor.c: When to schedule collections based on
 * memory usage.
 *
 * Author:
 * 	Rodrigo Kumpera (rkumpera@novell.com)
 *
 * Copyright 2001-2003 Ximian, Inc
 * Copyright 2003-2010 Novell, Inc.
 * Copyright 2011 Xamarin Inc (http://www.xamarin.com)
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

#include "metadata/sgen-gc.h"
#include "metadata/sgen-memory-governor.h"
#include "metadata/mono-gc.h"

#include "utils/mono-counters.h"
#include "utils/mono-mmap.h"
#include "utils/mono-logger-internal.h"
#include "utils/dtrace.h"

#define MIN_MINOR_COLLECTION_ALLOWANCE	((mword)(DEFAULT_NURSERY_SIZE * default_allowance_nursery_size_ratio))

/*Heap limits and allocation knobs*/
static mword max_heap_size = ((mword)0)- ((mword)1);
static mword soft_heap_limit = ((mword)0) - ((mword)1);

static double default_allowance_nursery_size_ratio = SGEN_DEFAULT_ALLOWANCE_NURSERY_SIZE_RATIO;
static double save_target_ratio = SGEN_DEFAULT_SAVE_TARGET_RATIO;

/**/
static mword allocated_heap;
static mword total_alloc = 0;

/* GC triggers. */

static gboolean debug_print_allowance = FALSE;


/* use this to tune when to do a major/minor collection */
static mword memory_pressure = 0;
static mword minor_collection_allowance;
static mword minor_collection_sections_alloced = 0;

static mword last_major_num_sections = 0;
static mword last_los_memory_usage = 0;

static gboolean need_calculate_minor_collection_allowance;

static mword last_collection_old_num_major_sections;
static mword last_collection_los_memory_usage = 0;
static mword last_collection_old_los_memory_usage;
static mword last_collection_los_memory_alloced;

static mword sgen_memgov_available_free_space (void);


static mword
double_to_mword_with_saturation (double value)
{
	if (value >= (double)MWORD_MAX_VALUE)
		return MWORD_MAX_VALUE;
	return (mword)value;
}

/* GC trigger heuristics. */

static void
sgen_memgov_try_calculate_minor_collection_allowance (gboolean overwrite)
{
	size_t num_major_sections, num_major_sections_saved;
	mword los_memory_saved, new_major, new_heap_size, save_target, allowance_target;

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

	save_target = (mword)((new_major + last_collection_los_memory_usage) * SGEN_DEFAULT_SAVE_TARGET_RATIO);

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
	allowance_target = double_to_mword_with_saturation ((double)save_target * (double)(minor_collection_sections_alloced * major_collector.section_size + last_collection_los_memory_alloced) / (double)(num_major_sections_saved * major_collector.section_size + los_memory_saved));

	minor_collection_allowance = MAX (MIN (allowance_target, num_major_sections * major_collector.section_size + los_memory_usage), MIN_MINOR_COLLECTION_ALLOWANCE);

	if (new_heap_size + minor_collection_allowance > soft_heap_limit) {
		if (new_heap_size > soft_heap_limit)
			minor_collection_allowance = MIN_MINOR_COLLECTION_ALLOWANCE;
		else
			minor_collection_allowance = MAX (soft_heap_limit - new_heap_size, MIN_MINOR_COLLECTION_ALLOWANCE);
	}

	if (debug_print_allowance) {
		mword old_major = last_collection_old_num_major_sections * major_collector.section_size;

		SGEN_LOG (1, "Before collection: %ld bytes (%ld major, %ld LOS)",
				  (long)(old_major + last_collection_old_los_memory_usage), (long)old_major, (long)last_collection_old_los_memory_usage);
		SGEN_LOG (1, "After collection: %ld bytes (%ld major, %ld LOS)",
				  (long)new_heap_size, (long)new_major, (long)last_collection_los_memory_usage);
		SGEN_LOG (1, "Allowance: %ld bytes", (long)minor_collection_allowance);
	}

	if (major_collector.have_computed_minor_collection_allowance)
		major_collector.have_computed_minor_collection_allowance ();

	need_calculate_minor_collection_allowance = FALSE;
}


gboolean
sgen_need_major_collection (mword space_needed)
{
	mword los_alloced;
	if (sgen_concurrent_collection_in_progress ())
		return FALSE;
	los_alloced = los_memory_usage - MIN (last_collection_los_memory_usage, los_memory_usage);
	return (space_needed > sgen_memgov_available_free_space ()) ||
		minor_collection_sections_alloced * major_collector.section_size + los_alloced > minor_collection_allowance;
}

void
sgen_memgov_minor_collection_start (void)
{
	sgen_memgov_try_calculate_minor_collection_allowance (FALSE);
}

void
sgen_memgov_minor_collection_end (void)
{
}

void
sgen_memgov_major_collection_start (void)
{
	last_collection_old_num_major_sections = sgen_get_major_collector ()->get_num_major_sections ();

	/*
	 * A domain could have been freed, resulting in
	 * los_memory_usage being less than last_collection_los_memory_usage.
	 */
	last_collection_los_memory_alloced = los_memory_usage - MIN (last_collection_los_memory_usage, los_memory_usage);
	last_collection_old_los_memory_usage = los_memory_usage;

	need_calculate_minor_collection_allowance = TRUE;
}

void
sgen_memgov_major_collection_end (void)
{
	sgen_memgov_try_calculate_minor_collection_allowance (TRUE);

	minor_collection_sections_alloced = 0;
	last_collection_los_memory_usage = los_memory_usage;
}

void
sgen_memgov_collection_start (int generation)
{
	last_major_num_sections = major_collector.get_num_major_sections ();
	last_los_memory_usage = los_memory_usage;
}

static void
log_timming (GGTimingInfo *info)
{
	//unsigned long stw_time, unsigned long bridge_time, gboolean is_overflow
	mword num_major_sections = major_collector.get_num_major_sections ();
	char full_timing_buff [1024];
	full_timing_buff [0] = '\0';

	if (!info->is_overflow)
	        sprintf (full_timing_buff, "total %.2fms, bridge %.2fms", info->stw_time / 10000.0f, (int)info->bridge_time / 10000.0f);
	if (info->generation == GENERATION_OLD)
	        mono_trace (G_LOG_LEVEL_INFO, MONO_TRACE_GC, "GC_MAJOR%s: (%s) pause %.2fms, %s major %dK/%dK los %dK/%dK",
	                info->is_overflow ? "_OVERFLOW" : "",
	                info->reason ? info->reason : "",
	                (int)info->total_time / 10000.0f,
	                full_timing_buff,
	                major_collector.section_size * num_major_sections / 1024,
	                major_collector.section_size * last_major_num_sections / 1024,
	                los_memory_usage / 1024,
	                last_los_memory_usage / 1024);
	else
	        mono_trace (G_LOG_LEVEL_INFO, MONO_TRACE_GC, "GC_MINOR%s: (%s) pause %.2fms, %s promoted %dK major %dK los %dK",
	        		info->is_overflow ? "_OVERFLOW" : "",
	                info->reason ? info->reason : "",
	                (int)info->total_time / 10000.0f,
	                full_timing_buff,
	                (num_major_sections - last_major_num_sections) * major_collector.section_size / 1024,
	                major_collector.section_size * num_major_sections / 1024,
	                los_memory_usage / 1024);       
}

void
sgen_memgov_collection_end (int generation, GGTimingInfo* info, int info_count)
{
	int i;
	for (i = 0; i < info_count; ++i) {
		if (info[i].generation != -1)
			log_timming (&info [i]);
	}
}

void
sgen_register_major_sections_alloced (size_t num_sections)
{
	minor_collection_sections_alloced += num_sections;
}

mword
sgen_get_minor_collection_allowance (void)
{
	return minor_collection_allowance;
}

/* Memory pressure API */

/* Negative value to remove */
void
mono_gc_add_memory_pressure (gint64 value)
{
	/* FIXME: Use interlocked functions */
	LOCK_GC;
	memory_pressure += value;
	UNLOCK_GC;
}


/*
Global GC memory tracking.
This tracks the total usage of memory by the GC. This includes
managed and unmanaged memory.
*/

static unsigned long
prot_flags_for_activate (int activate)
{
	unsigned long prot_flags = activate? MONO_MMAP_READ|MONO_MMAP_WRITE: MONO_MMAP_NONE;
	return prot_flags | MONO_MMAP_PRIVATE | MONO_MMAP_ANON;
}

void
sgen_assert_memory_alloc (void *ptr, size_t requested_size, const char *assert_description)
{
	if (ptr || !assert_description)
		return;
	fprintf (stderr, "Error: Garbage collector could not allocate %zu bytes of memory for %s.\n", requested_size, assert_description);
	exit (1);
}

/*
 * Allocate a big chunk of memory from the OS (usually 64KB to several megabytes).
 * This must not require any lock.
 */
void*
sgen_alloc_os_memory (size_t size, SgenAllocFlags flags, const char *assert_description)
{
	void *ptr;

	g_assert (!(flags & ~(SGEN_ALLOC_HEAP | SGEN_ALLOC_ACTIVATE)));

	ptr = mono_valloc (0, size, prot_flags_for_activate (flags & SGEN_ALLOC_ACTIVATE));
	sgen_assert_memory_alloc (ptr, size, assert_description);
	if (ptr) {
		SGEN_ATOMIC_ADD_P (total_alloc, size);
		if (flags & SGEN_ALLOC_HEAP)
			MONO_GC_HEAP_ALLOC ((mword)ptr, size);
	}
	return ptr;
}

/* size must be a power of 2 */
void*
sgen_alloc_os_memory_aligned (size_t size, mword alignment, SgenAllocFlags flags, const char *assert_description)
{
	void *ptr;

	g_assert (!(flags & ~(SGEN_ALLOC_HEAP | SGEN_ALLOC_ACTIVATE)));

	ptr = mono_valloc_aligned (size, alignment, prot_flags_for_activate (flags & SGEN_ALLOC_ACTIVATE));
	sgen_assert_memory_alloc (ptr, size, assert_description);
	if (ptr) {
		SGEN_ATOMIC_ADD_P (total_alloc, size);
		if (flags & SGEN_ALLOC_HEAP)
			MONO_GC_HEAP_ALLOC ((mword)ptr, size);
	}
	return ptr;
}

/*
 * Free the memory returned by sgen_alloc_os_memory (), returning it to the OS.
 */
void
sgen_free_os_memory (void *addr, size_t size, SgenAllocFlags flags)
{
	g_assert (!(flags & ~SGEN_ALLOC_HEAP));

	mono_vfree (addr, size);
	SGEN_ATOMIC_ADD_P (total_alloc, -(gssize)size);
	if (flags & SGEN_ALLOC_HEAP)
		MONO_GC_HEAP_FREE ((mword)addr, size);
}

int64_t
mono_gc_get_heap_size (void)
{
	return total_alloc;
}


/*
Heap Sizing limits.
This limit the max size of the heap. It takes into account
only memory actively in use to hold heap objects and not
for other parts of the GC.
 */
static mword
sgen_memgov_available_free_space (void)
{
	return max_heap_size - MIN (allocated_heap, max_heap_size);
}

void
sgen_memgov_release_space (mword size, int space)
{
	SGEN_ATOMIC_ADD_P (allocated_heap, -(gssize)size);
}

gboolean
sgen_memgov_try_alloc_space (mword size, int space)
{
	if (sgen_memgov_available_free_space () < size)
		return FALSE;

	SGEN_ATOMIC_ADD_P (allocated_heap, size);
	mono_runtime_resource_check_limit (MONO_RESOURCE_GC_HEAP, allocated_heap);
	return TRUE;
}

void
sgen_memgov_init (size_t max_heap, size_t soft_limit, gboolean debug_allowance, double allowance_ratio, double save_target)
{
	if (soft_limit)
		soft_heap_limit = soft_limit;

	debug_print_allowance = debug_allowance;
	minor_collection_allowance = MIN_MINOR_COLLECTION_ALLOWANCE;

	if (max_heap == 0)
		return;

	if (max_heap < soft_limit) {
		sgen_env_var_error (MONO_GC_PARAMS_NAME, "Setting to minimum.", "`max-heap-size` must be at least as large as `soft-heap-limit`.");
		max_heap = soft_limit;
	}

	if (max_heap < sgen_nursery_size * 4) {
		sgen_env_var_error (MONO_GC_PARAMS_NAME, "Setting to minimum.", "`max-heap-size` must be at least 4 times as large as `nursery size`.");
		max_heap = sgen_nursery_size * 4;
	}
	max_heap_size = max_heap - sgen_nursery_size;

	if (allowance_ratio)
		default_allowance_nursery_size_ratio = allowance_ratio;

	if (save_target)
		save_target_ratio = save_target;
	minor_collection_allowance = MIN_MINOR_COLLECTION_ALLOWANCE;
}

#endif
