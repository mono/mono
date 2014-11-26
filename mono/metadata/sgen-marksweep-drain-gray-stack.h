/*
 * sgen-marksweep-drain-gray-stack.h: The copy/mark and gray stack
 *     draining functions of the M&S major collector.
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

/*
 * COPY_OR_MARK_FUNCTION_NAME must be defined to be the function name of the copy/mark
 * function.
 *
 * DRAIN_GRAY_STACK_FUNCTION_NAME must be defined to be the function name of the gray stack
 * draining function.
 *
 * Define COPY_OR_MARK_WITH_EVACUATION to support evacuation.
 */

/* Returns whether the object is still in the nursery. */
static inline MONO_ALWAYS_INLINE gboolean
COPY_OR_MARK_FUNCTION_NAME (void **ptr, void *obj, SgenGrayQueue *queue)
{
#ifdef SGEN_MARK_ON_ENQUEUE
	MSBlockInfo *block;
#endif

#ifdef HEAVY_STATISTICS
	++stat_optimized_copy;
	{
		char *forwarded;
		mword desc;
		if ((forwarded = SGEN_OBJECT_IS_FORWARDED (obj)))
			desc = sgen_obj_get_descriptor_safe (forwarded);
		else
			desc = sgen_obj_get_descriptor_safe (obj);

		sgen_descriptor_count_copied_object (desc);
	}
#endif

	SGEN_ASSERT (9, obj, "null object from pointer %p", ptr);
	SGEN_ASSERT (9, current_collection_generation == GENERATION_OLD, "old gen parallel allocator called from a %d collection", current_collection_generation);

	if (sgen_ptr_in_nursery (obj)) {
#ifdef SGEN_MARK_ON_ENQUEUE
		int word, bit;
#endif
		char *forwarded, *old_obj;
		mword vtable_word = *(mword*)obj;

		HEAVY_STAT (++stat_optimized_copy_nursery);

		if (SGEN_VTABLE_IS_PINNED (vtable_word)) {
			HEAVY_STAT (++stat_optimized_copy_nursery_pinned);
			return TRUE;
		}
		if ((forwarded = SGEN_VTABLE_IS_FORWARDED (vtable_word))) {
			HEAVY_STAT (++stat_optimized_copy_nursery_forwarded);
			*ptr = forwarded;
			return sgen_ptr_in_nursery (forwarded);
		}

		/* An object in the nursery To Space has already been copied and grayed. Nothing to do. */
		if (sgen_nursery_is_to_space (obj))
			return TRUE;

#ifdef COPY_OR_MARK_WITH_EVACUATION
	do_copy_object:
#endif
		old_obj = obj;
		obj = copy_object_no_checks (obj, queue);
		SGEN_ASSERT (0, old_obj != obj, "Cannot handle copy object failure.");
		HEAVY_STAT (++stat_objects_copied_major);
		*ptr = obj;

		if (sgen_ptr_in_nursery (obj))
			return TRUE;

#ifdef SGEN_MARK_ON_ENQUEUE
		/*
		 * FIXME: See comment for copy_object_no_checks().  If
		 * we have that, we can let the allocation function
		 * give us the block info, too, and we won't have to
		 * re-fetch it.
		 *
		 * FIXME (2): We should rework this to avoid all those nursery checks.
		 */
		/*
		 * For the split nursery allocator the object might
		 * still be in the nursery despite having being
		 * promoted, in which case we can't mark it.
		 */
		block = MS_BLOCK_FOR_OBJ (obj);
		MS_CALC_MARK_BIT (word, bit, obj);
		SGEN_ASSERT (9, !MS_MARK_BIT (block, word, bit), "object %p already marked", obj);
		MS_SET_MARK_BIT (block, word, bit);
		binary_protocol_mark (obj, (gpointer)LOAD_VTABLE (obj), sgen_safe_object_get_size ((MonoObject*)obj));
#endif

		return FALSE;
	} else {
#ifdef SGEN_MARK_ON_ENQUEUE
		mword vtable_word = *(mword*)obj;
		mword desc = sgen_vtable_get_descriptor ((MonoVTable*)vtable_word);
		int type = desc & 7;

		HEAVY_STAT (++stat_optimized_copy_major);

#ifdef COPY_OR_MARK_WITH_EVACUATION
		{
			char *forwarded;
			if ((forwarded = SGEN_VTABLE_IS_FORWARDED (vtable_word))) {
				HEAVY_STAT (++stat_optimized_copy_major_forwarded);
				*ptr = forwarded;
				return FALSE;
			}
		}
#endif

		if (type <= DESC_TYPE_MAX_SMALL_OBJ || SGEN_ALIGN_UP (sgen_safe_object_get_size ((MonoObject*)obj)) <= SGEN_MAX_SMALL_OBJ_SIZE) {
#ifdef HEAVY_STATISTICS
			if (type <= DESC_TYPE_MAX_SMALL_OBJ)
				++stat_optimized_copy_major_small_fast;
			else
				++stat_optimized_copy_major_small_slow;
#endif

			block = MS_BLOCK_FOR_OBJ (obj);

#ifdef COPY_OR_MARK_WITH_EVACUATION
			{
				int size_index = block->obj_size_index;

				if (evacuate_block_obj_sizes [size_index] && !block->has_pinned) {
					HEAVY_STAT (++stat_optimized_copy_major_small_evacuate);
					if (block->is_to_space)
						return FALSE;
					goto do_copy_object;
				}
			}
#endif

			MS_MARK_OBJECT_AND_ENQUEUE (obj, desc, block, queue);
		} else {
			HEAVY_STAT (++stat_optimized_copy_major_large);

			if (sgen_los_object_is_pinned (obj))
				return FALSE;
			binary_protocol_pin (obj, (gpointer)SGEN_LOAD_VTABLE (obj), sgen_safe_object_get_size ((MonoObject*)obj));

			sgen_los_pin_object (obj);
			if (SGEN_OBJECT_HAS_REFERENCES (obj))
				GRAY_OBJECT_ENQUEUE (queue, obj, sgen_obj_get_descriptor (obj));
		}
		return FALSE;
#else
		GRAY_OBJECT_ENQUEUE (queue, obj, 0);
#endif
	}
	return FALSE;
}

static gboolean
DRAIN_GRAY_STACK_FUNCTION_NAME (ScanCopyContext ctx)
{
	SgenGrayQueue *queue = ctx.queue;

	SGEN_ASSERT (0, ctx.scan_func == major_scan_object, "Wrong scan function");

#ifdef USE_PREFETCH_QUEUE
	HEAVY_STAT (++stat_drain_prefetch_fills);
	if (!sgen_gray_object_fill_prefetch (queue)) {
		HEAVY_STAT (++stat_drain_prefetch_fill_failures);
		return TRUE;
	}
#endif

	for (;;) {
		char *obj;
		mword desc;
		int type;

		HEAVY_STAT (++stat_drain_loops);

#ifdef USE_PREFETCH_QUEUE
		sgen_gray_object_dequeue_fast (queue, &obj, &desc);
		if (!obj) {
			HEAVY_STAT (++stat_drain_prefetch_fills);
			if (!sgen_gray_object_fill_prefetch (queue)) {
				HEAVY_STAT (++stat_drain_prefetch_fill_failures);
				return TRUE;
			}
			continue;
		}
#else
		GRAY_OBJECT_DEQUEUE (queue, &obj, &desc);
		if (!obj)
			return TRUE;
#endif

#ifndef SGEN_GRAY_QUEUE_HAVE_DESCRIPTORS
		desc = sgen_obj_get_descriptor_safe (obj);
#endif
		type = desc & 7;

#ifndef SGEN_MARK_ON_ENQUEUE
		HEAVY_STAT (++stat_optimized_major_mark);

		/* Mark object or, if already marked, don't process. */
		if (!sgen_ptr_in_nursery (obj)) {
			if (type <= DESC_TYPE_MAX_SMALL_OBJ || SGEN_ALIGN_UP (sgen_safe_object_get_size ((MonoObject*)obj)) <= SGEN_MAX_SMALL_OBJ_SIZE) {
				MSBlockInfo *block = MS_BLOCK_FOR_OBJ (obj);
				int __word, __bit;

				HEAVY_STAT (++stat_optimized_major_mark_small);

				MS_CALC_MARK_BIT (__word, __bit, (obj));
				if (MS_MARK_BIT ((block), __word, __bit))
					continue;
				MS_SET_MARK_BIT ((block), __word, __bit);
			} else {
				HEAVY_STAT (++stat_optimized_major_mark_large);

				if (sgen_los_object_is_pinned (obj))
					continue;
				sgen_los_pin_object (obj);
			}
		}
#endif

#ifdef HEAVY_STATISTICS
		++stat_optimized_major_scan;
		if (!sgen_gc_descr_has_references (desc))
			++stat_optimized_major_scan_no_refs;
#endif

		/* Now scan the object. */

#undef HANDLE_PTR
#define HANDLE_PTR(ptr,obj)	do {					\
			void *__old = *(ptr);				\
			if (__old) {					\
				gboolean __still_in_nursery = COPY_OR_MARK_FUNCTION_NAME ((ptr), __old, queue); \
				if (G_UNLIKELY (__still_in_nursery && !sgen_ptr_in_nursery ((ptr)) && !SGEN_OBJECT_IS_CEMENTED (*(ptr)))) { \
					void *__copy = *(ptr);		\
					sgen_add_to_global_remset ((ptr), __copy); \
				}					\
			}						\
		} while (0)

#ifdef DESCRIPTOR_FAST_PATH
		if (type == DESC_TYPE_LARGE_BITMAP) {
			OBJ_LARGE_BITMAP_FOREACH_PTR (desc, obj);
#ifdef HEAVY_STATISTICS
			sgen_descriptor_count_scanned_object (desc);
			++stat_optimized_major_scan_fast;
#endif
#ifdef SGEN_HEAVY_BINARY_PROTOCOL
			add_scanned_object (obj);
#endif
		} else
#endif
		{
			char *start = obj;
#ifdef HEAVY_STATISTICS
			++stat_optimized_major_scan_slow;
			sgen_descriptor_count_scanned_object (desc);
#endif
#ifdef SGEN_HEAVY_BINARY_PROTOCOL
			add_scanned_object (start);
#endif

#include "sgen-scan-object.h"
		}
	}
}

#undef COPY_OR_MARK_FUNCTION_NAME
#undef COPY_OR_MARK_WITH_EVACUATION
#undef DRAIN_GRAY_STACK_FUNCTION_NAME
