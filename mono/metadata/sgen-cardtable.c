/*
 * sgen-cardtable.c: Card table implementation for sgen
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
#include "metadata/sgen-cardtable.h"
#include "metadata/sgen-memory-governor.h"
#include "metadata/sgen-protocol.h"
#include "metadata/sgen-layout-stats.h"
#include "utils/mono-counters.h"
#include "utils/mono-time.h"
#include "utils/mono-memory-model.h"

//#define CARDTABLE_STATS

#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif
#ifdef HAVE_SYS_MMAN_H
#include <sys/mman.h>
#endif
#include <sys/types.h>

#define ARRAY_OBJ_INDEX(ptr,array,elem_size) (((char*)(ptr) - ((char*)(array) + G_STRUCT_OFFSET (MonoArray, vector))) / (elem_size))

guint8 *sgen_cardtable;

static gboolean need_mod_union;

#ifdef HEAVY_STATISTICS
guint64 marked_cards;
guint64 scanned_cards;
guint64 scanned_objects;
guint64 remarked_cards;

static guint64 los_marked_cards;
static guint64 large_objects;
static guint64 bloby_objects;
static guint64 los_array_cards;
static guint64 los_array_remsets;

#endif
static guint64 major_card_scan_time;
static guint64 los_card_scan_time;

static guint64 last_major_scan_time;
static guint64 last_los_scan_time;

static void sgen_card_tables_collect_stats (gboolean begin);


/*WARNING: This function returns the number of cards regardless of overflow in case of overlapping cards.*/
static mword
cards_in_range (mword address, mword size)
{
	mword end = address + MAX (1, size) - 1;
	return (end >> CARD_BITS) - (address >> CARD_BITS) + 1;
}

static void
sgen_card_table_wbarrier_set_field (MonoObject *obj, gpointer field_ptr, MonoObject* value)
{
	*(void**)field_ptr = value;
	if (need_mod_union || sgen_ptr_in_nursery (value))
		sgen_card_table_mark_address ((mword)field_ptr);
	sgen_dummy_use (value);
}

static void
sgen_card_table_wbarrier_set_arrayref (MonoArray *arr, gpointer slot_ptr, MonoObject* value)
{
	*(void**)slot_ptr = value;
	if (need_mod_union || sgen_ptr_in_nursery (value))
		sgen_card_table_mark_address ((mword)slot_ptr);
	sgen_dummy_use (value);	
}

static void
sgen_card_table_wbarrier_arrayref_copy (gpointer dest_ptr, gpointer src_ptr, int count)
{
	gpointer *dest = dest_ptr;
	gpointer *src = src_ptr;

	/*overlapping that required backward copying*/
	if (src < dest && (src + count) > dest) {
		gpointer *start = dest;
		dest += count - 1;
		src += count - 1;

		for (; dest >= start; --src, --dest) {
			gpointer value = *src;
			SGEN_UPDATE_REFERENCE_ALLOW_NULL (dest, value);
			if (need_mod_union || sgen_ptr_in_nursery (value))
				sgen_card_table_mark_address ((mword)dest);
			sgen_dummy_use (value);
		}
	} else {
		gpointer *end = dest + count;
		for (; dest < end; ++src, ++dest) {
			gpointer value = *src;
			SGEN_UPDATE_REFERENCE_ALLOW_NULL (dest, value);
			if (need_mod_union || sgen_ptr_in_nursery (value))
				sgen_card_table_mark_address ((mword)dest);
			sgen_dummy_use (value);
		}
	}	
}

static void
sgen_card_table_wbarrier_value_copy (gpointer dest, gpointer src, int count, MonoClass *klass)
{
	size_t element_size = mono_class_value_size (klass, NULL);
	size_t size = count * element_size;

#ifdef DISABLE_CRITICAL_REGION
	LOCK_GC;
#else
	TLAB_ACCESS_INIT;
	ENTER_CRITICAL_REGION;
#endif
	mono_gc_memmove_atomic (dest, src, size);
	sgen_card_table_mark_range ((mword)dest, size);
#ifdef DISABLE_CRITICAL_REGION
	UNLOCK_GC;
#else
	EXIT_CRITICAL_REGION;
#endif
}

static void
sgen_card_table_wbarrier_object_copy (MonoObject* obj, MonoObject *src)
{
	int size = mono_object_class (obj)->instance_size;

#ifdef DISABLE_CRITICAL_REGION
	LOCK_GC;
#else
	TLAB_ACCESS_INIT;
	ENTER_CRITICAL_REGION;
#endif
	mono_gc_memmove_aligned ((char*)obj + sizeof (MonoObject), (char*)src + sizeof (MonoObject),
			size - sizeof (MonoObject));
	sgen_card_table_mark_range ((mword)obj, size);
#ifdef DISABLE_CRITICAL_REGION
	UNLOCK_GC;
#else
	EXIT_CRITICAL_REGION;
#endif	
}

static void
sgen_card_table_wbarrier_generic_nostore (gpointer ptr)
{
	sgen_card_table_mark_address ((mword)ptr);	
}

#ifdef SGEN_HAVE_OVERLAPPING_CARDS

guint8 *sgen_shadow_cardtable;

#define SGEN_SHADOW_CARDTABLE_END (sgen_shadow_cardtable + CARD_COUNT_IN_BYTES)
#define SGEN_CARDTABLE_END (sgen_cardtable + CARD_COUNT_IN_BYTES)

static gboolean
sgen_card_table_region_begin_scanning (mword start, mword end)
{
	/*XXX this can be improved to work on words and have a single loop induction var */
	while (start <= end) {
		if (sgen_card_table_card_begin_scanning (start))
			return TRUE;
		start += CARD_SIZE_IN_BYTES;
	}
	return FALSE;
}

#else

static gboolean
sgen_card_table_region_begin_scanning (mword start, mword size)
{
	gboolean res = FALSE;
	guint8 *card = sgen_card_table_get_card_address (start);
	guint8 *end = card + cards_in_range (start, size);

	/*XXX this can be improved to work on words and have a branchless body */
	while (card != end) {
		if (*card++) {
			res = TRUE;
			break;
		}
	}

	memset (sgen_card_table_get_card_address (start), 0, size >> CARD_BITS);

	return res;
}

#endif

/*FIXME this assumes that major blocks are multiple of 4K which is pretty reasonable */
gboolean
sgen_card_table_get_card_data (guint8 *data_dest, mword address, mword cards)
{
	mword *start = (mword*)sgen_card_table_get_card_scan_address (address);
	mword *dest = (mword*)data_dest;
	mword *end = (mword*)(data_dest + cards);
	mword mask = 0;

	for (; dest < end; ++dest, ++start) {
		mword v = *start;
		*dest = v;
		mask |= v;

#ifndef SGEN_HAVE_OVERLAPPING_CARDS
		*start = 0;
#endif
	}

	return mask != 0;
}

void*
sgen_card_table_align_pointer (void *ptr)
{
	return (void*)((mword)ptr & ~(CARD_SIZE_IN_BYTES - 1));
}

void
sgen_card_table_mark_range (mword address, mword size)
{
	memset (sgen_card_table_get_card_address (address), 1, cards_in_range (address, size));
}

static gboolean
sgen_card_table_is_range_marked (guint8 *cards, mword address, mword size)
{
	guint8 *end = cards + cards_in_range (address, size);

	/*This is safe since this function is only called by code that only passes continuous card blocks*/
	while (cards != end) {
		if (*cards++)
			return TRUE;
	}
	return FALSE;

}

static void
sgen_card_table_record_pointer (gpointer address)
{
	*sgen_card_table_get_card_address ((mword)address) = 1;
}

static gboolean
sgen_card_table_find_address (char *addr)
{
	return sgen_card_table_address_is_marked ((mword)addr);
}

static gboolean
sgen_card_table_find_address_with_cards (char *cards_start, guint8 *cards, char *addr)
{
	cards_start = sgen_card_table_align_pointer (cards_start);
	return cards [(addr - cards_start) >> CARD_BITS];
}

static void
update_mod_union (guint8 *dest, gboolean init, guint8 *start_card, size_t num_cards)
{
	if (init) {
		memcpy (dest, start_card, num_cards);
	} else {
		int i;
		for (i = 0; i < num_cards; ++i)
			dest [i] |= start_card [i];
	}
}

static guint8*
alloc_mod_union (size_t num_cards)
{
	return sgen_alloc_internal_dynamic (num_cards, INTERNAL_MEM_CARDTABLE_MOD_UNION, TRUE);
}

guint8*
sgen_card_table_update_mod_union_from_cards (guint8 *dest, guint8 *start_card, size_t num_cards)
{
	gboolean init = dest == NULL;

	if (init)
		dest = alloc_mod_union (num_cards);

	update_mod_union (dest, init, start_card, num_cards);

	return dest;
}

guint8*
sgen_card_table_update_mod_union (guint8 *dest, char *obj, mword obj_size, size_t *out_num_cards)
{
	guint8 *start_card = sgen_card_table_get_card_address ((mword)obj);
#ifndef SGEN_HAVE_OVERLAPPING_CARDS
	guint8 *end_card = sgen_card_table_get_card_address ((mword)obj + obj_size - 1) + 1;
#endif
	size_t num_cards;
	guint8 *result = NULL;

#ifdef SGEN_HAVE_OVERLAPPING_CARDS
	size_t rest;

	rest = num_cards = cards_in_range ((mword) obj, obj_size);

	while (start_card + rest > SGEN_CARDTABLE_END) {
		size_t count = SGEN_CARDTABLE_END - start_card;
		dest = sgen_card_table_update_mod_union_from_cards (dest, start_card, count);
		if (!result)
			result = dest;
		dest += count;
		rest -= count;
		start_card = sgen_cardtable;
	}
	num_cards = rest;
#else
	num_cards = end_card - start_card;
#endif

	dest = sgen_card_table_update_mod_union_from_cards (dest, start_card, num_cards);
	if (!result)
		result = dest;

	if (out_num_cards)
		*out_num_cards = num_cards;

	return result;
}

#ifdef SGEN_HAVE_OVERLAPPING_CARDS

static void
move_cards_to_shadow_table (mword start, mword size)
{
	guint8 *from = sgen_card_table_get_card_address (start);
	guint8 *to = sgen_card_table_get_shadow_card_address (start);
	size_t bytes = cards_in_range (start, size);

	if (bytes >= CARD_COUNT_IN_BYTES) {
		memcpy (sgen_shadow_cardtable, sgen_cardtable, CARD_COUNT_IN_BYTES);
	} else if (to + bytes > SGEN_SHADOW_CARDTABLE_END) {
		size_t first_chunk = SGEN_SHADOW_CARDTABLE_END - to;
		size_t second_chunk = MIN (CARD_COUNT_IN_BYTES, bytes) - first_chunk;

		memcpy (to, from, first_chunk);
		memcpy (sgen_shadow_cardtable, sgen_cardtable, second_chunk);
	} else {
		memcpy (to, from, bytes);
	}
}

static void
clear_cards (mword start, mword size)
{
	guint8 *addr = sgen_card_table_get_card_address (start);
	size_t bytes = cards_in_range (start, size);

	if (bytes >= CARD_COUNT_IN_BYTES) {
		memset (sgen_cardtable, 0, CARD_COUNT_IN_BYTES);
	} else if (addr + bytes > SGEN_CARDTABLE_END) {
		size_t first_chunk = SGEN_CARDTABLE_END - addr;

		memset (addr, 0, first_chunk);
		memset (sgen_cardtable, 0, bytes - first_chunk);
	} else {
		memset (addr, 0, bytes);
	}
}


#else

static void
clear_cards (mword start, mword size)
{
	memset (sgen_card_table_get_card_address (start), 0, cards_in_range (start, size));
}


#endif

static void
sgen_card_table_prepare_for_major_collection (void)
{
	/*XXX we could do this in 2 ways. using mincore or iterating over all sections/los objects */
	sgen_major_collector_iterate_live_block_ranges (clear_cards);
	sgen_los_iterate_live_block_ranges (clear_cards);
}

static void
sgen_card_table_finish_minor_collection (void)
{
	sgen_card_tables_collect_stats (FALSE);
}

static void
sgen_card_table_finish_scan_remsets (void *start_nursery, void *end_nursery, SgenGrayQueue *queue)
{
	SGEN_TV_DECLARE (atv);
	SGEN_TV_DECLARE (btv);

	sgen_card_tables_collect_stats (TRUE);

#ifdef SGEN_HAVE_OVERLAPPING_CARDS
	/*FIXME we should have a bit on each block/los object telling if the object have marked cards.*/
	/*First we copy*/
	sgen_major_collector_iterate_live_block_ranges (move_cards_to_shadow_table);
	sgen_los_iterate_live_block_ranges (move_cards_to_shadow_table);

	/*Then we clear*/
	sgen_card_table_prepare_for_major_collection ();
#endif
	SGEN_TV_GETTIME (atv);
	sgen_major_collector_scan_card_table (queue);
	SGEN_TV_GETTIME (btv);
	last_major_scan_time = SGEN_TV_ELAPSED (atv, btv); 
	major_card_scan_time += last_major_scan_time;
	sgen_los_scan_card_table (FALSE, queue);
	SGEN_TV_GETTIME (atv);
	last_los_scan_time = SGEN_TV_ELAPSED (btv, atv);
	los_card_scan_time += last_los_scan_time;
}

guint8*
mono_gc_get_card_table (int *shift_bits, gpointer *mask)
{
#ifndef MANAGED_WBARRIER
	return NULL;
#else
	if (!sgen_cardtable)
		return NULL;

	*shift_bits = CARD_BITS;
#ifdef SGEN_HAVE_OVERLAPPING_CARDS
	*mask = (gpointer)CARD_MASK;
#else
	*mask = NULL;
#endif

	return sgen_cardtable;
#endif
}

gboolean
mono_gc_card_table_nursery_check (void)
{
	return !major_collector.is_concurrent;
}

#if 0
void
sgen_card_table_dump_obj_card (char *object, size_t size, void *dummy)
{
	guint8 *start = sgen_card_table_get_card_scan_address (object);
	guint8 *end = start + cards_in_range (object, size);
	int cnt = 0;
	printf ("--obj %p %d cards [%p %p]--", object, size, start, end);
	for (; start < end; ++start) {
		if (cnt == 0)
			printf ("\n\t[%p] ", start);
		printf ("%x ", *start);
		++cnt;
		if (cnt == 8)
			cnt = 0;
	}
	printf ("\n");
}
#endif

#define MWORD_MASK (sizeof (mword) - 1)

static inline int
find_card_offset (mword card)
{
/*XXX Use assembly as this generates some pretty bad code */
#if defined(__i386__) && defined(__GNUC__)
	return  (__builtin_ffs (card) - 1) / 8;
#elif defined(__x86_64__) && defined(__GNUC__)
	return (__builtin_ffsll (card) - 1) / 8;
#elif defined(__s390x__)
	return (__builtin_ffsll (GUINT64_TO_LE(card)) - 1) / 8;
#else
	int i;
	guint8 *ptr = (guint8 *) &card;
	for (i = 0; i < sizeof (mword); ++i) {
		if (ptr[i])
			return i;
	}
	return 0;
#endif
}

static guint8*
find_next_card (guint8 *card_data, guint8 *end)
{
	mword *cards, *cards_end;
	mword card;

	while ((((mword)card_data) & MWORD_MASK) && card_data < end) {
		if (*card_data)
			return card_data;
		++card_data;
	}

	if (card_data == end)
		return end;

	cards = (mword*)card_data;
	cards_end = (mword*)((mword)end & ~MWORD_MASK);
	while (cards < cards_end) {
		card = *cards;
		if (card)
			return (guint8*)cards + find_card_offset (card);
		++cards;
	}

	card_data = (guint8*)cards_end;
	while (card_data < end) {
		if (*card_data)
			return card_data;
		++card_data;
	}

	return end;
}

void
sgen_cardtable_scan_object (char *obj, mword block_obj_size, guint8 *cards, gboolean mod_union, SgenGrayQueue *queue)
{
	MonoVTable *vt = (MonoVTable*)SGEN_LOAD_VTABLE (obj);
	MonoClass *klass = vt->klass;

	HEAVY_STAT (++large_objects);

	if (!SGEN_VTABLE_HAS_REFERENCES (vt)) {
		sgen_object_layout_scanned_bitmap (0);
		return;
	}

	if (vt->rank) {
		guint8 *card_data, *card_base;
		guint8 *card_data_end;
		char *obj_start = sgen_card_table_align_pointer (obj);
		mword obj_size = sgen_par_object_get_size (vt, (MonoObject*)obj);
		char *obj_end = obj + obj_size;
		size_t card_count;
		size_t extra_idx = 0;

		MonoArray *arr = (MonoArray*)obj;
		mword desc = (mword)klass->element_class->gc_descr;
		int elem_size = mono_array_element_size (klass);

#ifdef SGEN_HAVE_OVERLAPPING_CARDS
		guint8 *overflow_scan_end = NULL;
#endif

#ifdef SGEN_OBJECT_LAYOUT_STATISTICS
		if (klass->element_class->valuetype)
			sgen_object_layout_scanned_vtype_array ();
		else
			sgen_object_layout_scanned_ref_array ();
#endif

		if (cards)
			card_data = cards;
		else
			card_data = sgen_card_table_get_card_scan_address ((mword)obj);

		card_base = card_data;
		card_count = cards_in_range ((mword)obj, obj_size);
		card_data_end = card_data + card_count;


#ifdef SGEN_HAVE_OVERLAPPING_CARDS
		/*Check for overflow and if so, setup to scan in two steps*/
		if (!cards && card_data_end >= SGEN_SHADOW_CARDTABLE_END) {
			overflow_scan_end = sgen_shadow_cardtable + (card_data_end - SGEN_SHADOW_CARDTABLE_END);
			card_data_end = SGEN_SHADOW_CARDTABLE_END;
		}

LOOP_HEAD:
#endif

		card_data = find_next_card (card_data, card_data_end);
		for (; card_data < card_data_end; card_data = find_next_card (card_data + 1, card_data_end)) {
			size_t index;
			size_t idx = (card_data - card_base) + extra_idx;
			char *start = (char*)(obj_start + idx * CARD_SIZE_IN_BYTES);
			char *card_end = start + CARD_SIZE_IN_BYTES;
			char *first_elem, *elem;

			HEAVY_STAT (++los_marked_cards);

			if (!cards)
				sgen_card_table_prepare_card_for_scanning (card_data);

			card_end = MIN (card_end, obj_end);

			if (start <= (char*)arr->vector)
				index = 0;
			else
				index = ARRAY_OBJ_INDEX (start, obj, elem_size);

			elem = first_elem = (char*)mono_array_addr_with_size_fast ((MonoArray*)obj, elem_size, index);
			if (klass->element_class->valuetype) {
				ScanVTypeFunc scan_vtype_func = sgen_get_current_object_ops ()->scan_vtype;

				for (; elem < card_end; elem += elem_size)
					scan_vtype_func (elem, desc, queue BINARY_PROTOCOL_ARG (elem_size));
			} else {
				CopyOrMarkObjectFunc copy_func = sgen_get_current_object_ops ()->copy_or_mark_object;

				HEAVY_STAT (++los_array_cards);
				for (; elem < card_end; elem += SIZEOF_VOID_P) {
					gpointer new, old = *(gpointer*)elem;
					if ((mod_union && old) || G_UNLIKELY (sgen_ptr_in_nursery (old))) {
						HEAVY_STAT (++los_array_remsets);
						copy_func ((void**)elem, queue);
						new = *(gpointer*)elem;
						if (G_UNLIKELY (sgen_ptr_in_nursery (new)))
							sgen_add_to_global_remset (elem, new);
					}
				}
			}

			binary_protocol_card_scan (first_elem, elem - first_elem);
		}

#ifdef SGEN_HAVE_OVERLAPPING_CARDS
		if (overflow_scan_end) {
			extra_idx = card_data - card_base;
			card_base = card_data = sgen_shadow_cardtable;
			card_data_end = overflow_scan_end;
			overflow_scan_end = NULL;
			goto LOOP_HEAD;
		}
#endif

	} else {
		HEAVY_STAT (++bloby_objects);
		if (cards) {
			if (sgen_card_table_is_range_marked (cards, (mword)obj, block_obj_size))
				sgen_get_current_object_ops ()->scan_object (obj, sgen_obj_get_descriptor (obj), queue);
		} else if (sgen_card_table_region_begin_scanning ((mword)obj, block_obj_size)) {
			sgen_get_current_object_ops ()->scan_object (obj, sgen_obj_get_descriptor (obj), queue);
		}

		binary_protocol_card_scan (obj, sgen_safe_object_get_size ((MonoObject*)obj));
	}
}

#ifdef CARDTABLE_STATS

typedef struct {
	int total, marked, remarked, gc_marked;	
} card_stats;

static card_stats major_stats, los_stats;
static card_stats *cur_stats;

static void
count_marked_cards (mword start, mword size)
{
	mword end = start + size;
	while (start <= end) {
		guint8 card = *sgen_card_table_get_card_address (start);
		++cur_stats->total;
		if (card)
			++cur_stats->marked;
		if (card == 2)
			++cur_stats->gc_marked;
		start += CARD_SIZE_IN_BYTES;
	}
}

static void
count_remarked_cards (mword start, mword size)
{
	mword end = start + size;
	while (start <= end) {
		if (sgen_card_table_address_is_marked (start)) {
			++cur_stats->remarked;
			*sgen_card_table_get_card_address (start) = 2;
		}
		start += CARD_SIZE_IN_BYTES;
	}
}

#endif

static void
sgen_card_tables_collect_stats (gboolean begin)
{
#ifdef CARDTABLE_STATS
	if (begin) {
		memset (&major_stats, 0, sizeof (card_stats));
		memset (&los_stats, 0, sizeof (card_stats));
		cur_stats = &major_stats;
		sgen_major_collector_iterate_live_block_ranges (count_marked_cards);
		cur_stats = &los_stats;
		sgen_los_iterate_live_block_ranges (count_marked_cards);
	} else {
		cur_stats = &major_stats;
		sgen_major_collector_iterate_live_block_ranges (count_remarked_cards);
		cur_stats = &los_stats;
		sgen_los_iterate_live_block_ranges (count_remarked_cards);
		printf ("cards major (t %d m %d g %d r %d)  los (t %d m %d g %d r %d) major_scan %.2fms los_scan %.2fms\n", 
			major_stats.total, major_stats.marked, major_stats.gc_marked, major_stats.remarked,
			los_stats.total, los_stats.marked, los_stats.gc_marked, los_stats.remarked,
			last_major_scan_time / 10000.0f, last_los_scan_time / 10000.0f);
	}
#endif
}

void
sgen_card_table_init (SgenRemeberedSet *remset)
{
	sgen_cardtable = sgen_alloc_os_memory (CARD_COUNT_IN_BYTES, SGEN_ALLOC_INTERNAL | SGEN_ALLOC_ACTIVATE, "card table");

#ifdef SGEN_HAVE_OVERLAPPING_CARDS
	sgen_shadow_cardtable = sgen_alloc_os_memory (CARD_COUNT_IN_BYTES, SGEN_ALLOC_INTERNAL | SGEN_ALLOC_ACTIVATE, "shadow card table");
#endif

#ifdef HEAVY_STATISTICS
	mono_counters_register ("marked cards", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &marked_cards);
	mono_counters_register ("scanned cards", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &scanned_cards);
	mono_counters_register ("remarked cards", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &remarked_cards);

	mono_counters_register ("los marked cards", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &los_marked_cards);
	mono_counters_register ("los array cards scanned ", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &los_array_cards);
	mono_counters_register ("los array remsets", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &los_array_remsets);
	mono_counters_register ("cardtable scanned objects", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &scanned_objects);
	mono_counters_register ("cardtable large objects", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &large_objects);
	mono_counters_register ("cardtable bloby objects", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &bloby_objects);
#endif
	mono_counters_register ("cardtable major scan time", MONO_COUNTER_GC | MONO_COUNTER_ULONG | MONO_COUNTER_TIME, &major_card_scan_time);
	mono_counters_register ("cardtable los scan time", MONO_COUNTER_GC | MONO_COUNTER_ULONG | MONO_COUNTER_TIME, &los_card_scan_time);


	remset->wbarrier_set_field = sgen_card_table_wbarrier_set_field;
	remset->wbarrier_set_arrayref = sgen_card_table_wbarrier_set_arrayref;
	remset->wbarrier_arrayref_copy = sgen_card_table_wbarrier_arrayref_copy;
	remset->wbarrier_value_copy = sgen_card_table_wbarrier_value_copy;
	remset->wbarrier_object_copy = sgen_card_table_wbarrier_object_copy;
	remset->wbarrier_generic_nostore = sgen_card_table_wbarrier_generic_nostore;
	remset->record_pointer = sgen_card_table_record_pointer;

	remset->finish_scan_remsets = sgen_card_table_finish_scan_remsets;

	remset->finish_minor_collection = sgen_card_table_finish_minor_collection;
	remset->prepare_for_major_collection = sgen_card_table_prepare_for_major_collection;

	remset->find_address = sgen_card_table_find_address;
	remset->find_address_with_cards = sgen_card_table_find_address_with_cards;

	need_mod_union = sgen_get_major_collector ()->is_concurrent;
}

#endif /*HAVE_SGEN_GC*/
