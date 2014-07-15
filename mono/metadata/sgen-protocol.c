/*
 * sgen-protocol.c: Binary protocol of internal activity, to aid
 * debugging.
 *
 * Copyright 2001-2003 Ximian, Inc
 * Copyright 2003-2010 Novell, Inc.
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

#ifdef HAVE_SGEN_GC

#include "config.h"
#include "sgen-gc.h"
#include "sgen-protocol.h"
#include "sgen-memory-governor.h"
#include "utils/mono-mmap.h"
#include "utils/mono-threads.h"

/* If not null, dump binary protocol to this file */
static FILE *binary_protocol_file = NULL;

/* We set this to -1 to indicate an exclusive lock */
static volatile int binary_protocol_use_count = 0;

#define BINARY_PROTOCOL_BUFFER_SIZE	(65536 - 2 * 8)

typedef struct _BinaryProtocolBuffer BinaryProtocolBuffer;
struct _BinaryProtocolBuffer {
	BinaryProtocolBuffer * volatile next;
	volatile int index;
	unsigned char buffer [BINARY_PROTOCOL_BUFFER_SIZE];
};

static BinaryProtocolBuffer * volatile binary_protocol_buffers = NULL;

void
binary_protocol_init (const char *filename)
{
	binary_protocol_file = fopen (filename, "w");
}

gboolean
binary_protocol_is_enabled (void)
{
	return binary_protocol_file != NULL;
}

static gboolean
try_lock_exclusive (void)
{
	do {
		if (binary_protocol_use_count)
			return FALSE;
	} while (InterlockedCompareExchange (&binary_protocol_use_count, -1, 0) != 0);
	mono_memory_barrier ();
	return TRUE;
}

static void
unlock_exclusive (void)
{
	mono_memory_barrier ();
	SGEN_ASSERT (0, binary_protocol_use_count == -1, "Exclusively locked count must be -1");
	if (InterlockedCompareExchange (&binary_protocol_use_count, 0, -1) != -1)
		SGEN_ASSERT (0, FALSE, "Somebody messed with the exclusive lock");
}

static void
lock_recursive (void)
{
	int old_count;
	do {
	retry:
		old_count = binary_protocol_use_count;
		if (old_count < 0) {
			/* Exclusively locked - retry */
			/* FIXME: short back-off */
			goto retry;
		}
	} while (InterlockedCompareExchange (&binary_protocol_use_count, old_count + 1, old_count) != old_count);
	mono_memory_barrier ();
}

static void
unlock_recursive (void)
{
	int old_count;
	mono_memory_barrier ();
	do {
		old_count = binary_protocol_use_count;
		SGEN_ASSERT (0, old_count > 0, "Locked use count must be at least 1");
	} while (InterlockedCompareExchange (&binary_protocol_use_count, old_count - 1, old_count) != old_count);
}

static void
binary_protocol_flush_buffer (BinaryProtocolBuffer *buffer)
{
	g_assert (buffer->index > 0);
	fwrite (buffer->buffer, 1, buffer->index, binary_protocol_file);

	sgen_free_os_memory (buffer, sizeof (BinaryProtocolBuffer), SGEN_ALLOC_INTERNAL);
}

void
binary_protocol_flush_buffers (gboolean force)
{
	int num_buffers = 0, i;
	BinaryProtocolBuffer *buf;
	BinaryProtocolBuffer **bufs;

	if (!binary_protocol_file)
		return;

	if (!force && !try_lock_exclusive ())
		return;

	for (buf = binary_protocol_buffers; buf != NULL; buf = buf->next)
		++num_buffers;
	bufs = sgen_alloc_internal_dynamic (num_buffers * sizeof (BinaryProtocolBuffer*), INTERNAL_MEM_BINARY_PROTOCOL, TRUE);
	for (buf = binary_protocol_buffers, i = 0; buf != NULL; buf = buf->next, i++)
		bufs [i] = buf;
	SGEN_ASSERT (0, i == num_buffers, "Binary protocol buffer count error");

	binary_protocol_buffers = NULL;

	for (i = num_buffers - 1; i >= 0; --i)
		binary_protocol_flush_buffer (bufs [i]);

	sgen_free_internal_dynamic (buf, num_buffers * sizeof (BinaryProtocolBuffer*), INTERNAL_MEM_BINARY_PROTOCOL);

	if (!force)
		unlock_exclusive ();

	fflush (binary_protocol_file);
}

static BinaryProtocolBuffer*
binary_protocol_get_buffer (int length)
{
	BinaryProtocolBuffer *buffer, *new_buffer;

 retry:
	buffer = binary_protocol_buffers;
	if (buffer && buffer->index + length <= BINARY_PROTOCOL_BUFFER_SIZE)
		return buffer;

	new_buffer = sgen_alloc_os_memory (sizeof (BinaryProtocolBuffer), SGEN_ALLOC_INTERNAL | SGEN_ALLOC_ACTIVATE, "debugging memory");
	new_buffer->next = buffer;
	new_buffer->index = 0;

	if (InterlockedCompareExchangePointer ((void**)&binary_protocol_buffers, new_buffer, buffer) != buffer) {
		sgen_free_os_memory (new_buffer, sizeof (BinaryProtocolBuffer), SGEN_ALLOC_INTERNAL);
		goto retry;
	}

	return new_buffer;
}


static void
protocol_entry (unsigned char type, gpointer data, int size)
{
	int index;
	BinaryProtocolBuffer *buffer;

	if (!binary_protocol_file)
		return;

	if (sgen_is_worker_thread (mono_native_thread_id_get ()))
		type |= 0x80;

	lock_recursive ();

 retry:
	buffer = binary_protocol_get_buffer (size + 1);
 retry_same_buffer:
	index = buffer->index;
	if (index + 1 + size > BINARY_PROTOCOL_BUFFER_SIZE)
		goto retry;

	if (InterlockedCompareExchange (&buffer->index, index + 1 + size, index) != index)
		goto retry_same_buffer;

	/* FIXME: if we're interrupted at this point, we have a buffer
	   entry that contains random data. */

	buffer->buffer [index++] = type;
	memcpy (buffer->buffer + index, data, size);
	index += size;

	g_assert (index <= BINARY_PROTOCOL_BUFFER_SIZE);

	unlock_recursive ();
}

void
binary_protocol_collection_force (int generation)
{
	SGenProtocolCollectionForce entry = { generation };
	binary_protocol_flush_buffers (FALSE);
	protocol_entry (SGEN_PROTOCOL_COLLECTION_FORCE, &entry, sizeof (SGenProtocolCollectionForce));
}

void
binary_protocol_collection_begin (int index, int generation)
{
	SGenProtocolCollection entry = { index, generation };
	binary_protocol_flush_buffers (FALSE);
	protocol_entry (SGEN_PROTOCOL_COLLECTION_BEGIN, &entry, sizeof (SGenProtocolCollection));
}

void
binary_protocol_collection_end (int index, int generation)
{
	SGenProtocolCollection entry = { index, generation };
	binary_protocol_flush_buffers (FALSE);
	protocol_entry (SGEN_PROTOCOL_COLLECTION_END, &entry, sizeof (SGenProtocolCollection));
}

void
binary_protocol_concurrent_start (void)
{
	protocol_entry (SGEN_PROTOCOL_CONCURRENT_START, NULL, 0);
}

void
binary_protocol_concurrent_update_finish (void)
{
	protocol_entry (SGEN_PROTOCOL_CONCURRENT_UPDATE_FINISH, NULL, 0);
}

void
binary_protocol_world_stopping (long long timestamp)
{
	SGenProtocolWorldStopping entry = { timestamp };
	protocol_entry (SGEN_PROTOCOL_WORLD_STOPPING, &entry, sizeof (SGenProtocolWorldStopping));
}

void
binary_protocol_world_stopped (long long timestamp, long long total_major_cards,
		long long marked_major_cards, long long total_los_cards, long long marked_los_cards)
{
	SGenProtocolWorldStopped entry = { timestamp, total_major_cards, marked_major_cards, total_los_cards, marked_los_cards };
	protocol_entry (SGEN_PROTOCOL_WORLD_STOPPED, &entry, sizeof (SGenProtocolWorldStopped));
}

void
binary_protocol_world_restarting (int generation, long long timestamp,
		long long total_major_cards, long long marked_major_cards, long long total_los_cards, long long marked_los_cards)
{
	SGenProtocolWorldRestarting entry = { generation, timestamp, total_major_cards, marked_major_cards, total_los_cards, marked_los_cards };
	protocol_entry (SGEN_PROTOCOL_WORLD_RESTARTING, &entry, sizeof (SGenProtocolWorldRestarting));
}

void
binary_protocol_world_restarted (int generation, long long timestamp)
{
	SGenProtocolWorldRestarted entry = { generation, timestamp };
	protocol_entry (SGEN_PROTOCOL_WORLD_RESTARTED, &entry, sizeof (SGenProtocolWorldRestarted));
}

void
binary_protocol_thread_suspend (gpointer thread, gpointer stopped_ip)
{
	SGenProtocolThreadSuspend entry = { thread, stopped_ip };
	protocol_entry (SGEN_PROTOCOL_THREAD_SUSPEND, &entry, sizeof (SGenProtocolThreadSuspend));
}

void
binary_protocol_thread_restart (gpointer thread)
{
	SGenProtocolThreadRestart entry = { thread };
	protocol_entry (SGEN_PROTOCOL_THREAD_RESTART, &entry, sizeof (SGenProtocolThreadRestart));
}

void
binary_protocol_thread_register (gpointer thread)
{
	SGenProtocolThreadRegister entry = { thread };
	protocol_entry (SGEN_PROTOCOL_THREAD_REGISTER, &entry, sizeof (SGenProtocolThreadRegister));

}

void
binary_protocol_thread_unregister (gpointer thread)
{
	SGenProtocolThreadUnregister entry = { thread };
	protocol_entry (SGEN_PROTOCOL_THREAD_UNREGISTER, &entry, sizeof (SGenProtocolThreadUnregister));

}

void
binary_protocol_missing_remset (gpointer obj, gpointer obj_vtable, int offset, gpointer value, gpointer value_vtable, int value_pinned)
{
	SGenProtocolMissingRemset entry = { obj, obj_vtable, offset, value, value_vtable, value_pinned };
	protocol_entry (SGEN_PROTOCOL_MISSING_REMSET, &entry, sizeof (SGenProtocolMissingRemset));

}

void
binary_protocol_cement (gpointer obj, gpointer vtable, int size)
{
	SGenProtocolCement entry = { obj, vtable, size };
	protocol_entry (SGEN_PROTOCOL_CEMENT, &entry, sizeof (SGenProtocolCement));
}

void
binary_protocol_cement_reset (void)
{
	protocol_entry (SGEN_PROTOCOL_CEMENT_RESET, NULL, 0);
}

void
binary_protocol_domain_unload_begin (gpointer domain)
{
	SGenProtocolDomainUnload entry = { domain };
	protocol_entry (SGEN_PROTOCOL_DOMAIN_UNLOAD_BEGIN, &entry, sizeof (SGenProtocolDomainUnload));
}

void
binary_protocol_domain_unload_end (gpointer domain)
{
	SGenProtocolDomainUnload entry = { domain };
	protocol_entry (SGEN_PROTOCOL_DOMAIN_UNLOAD_END, &entry, sizeof (SGenProtocolDomainUnload));
}

#ifdef SGEN_HEAVY_BINARY_PROTOCOL
void
binary_protocol_alloc (gpointer obj, gpointer vtable, int size)
{
	SGenProtocolAlloc entry = { obj, vtable, size };
	protocol_entry (SGEN_PROTOCOL_ALLOC, &entry, sizeof (SGenProtocolAlloc));
}

void
binary_protocol_alloc_pinned (gpointer obj, gpointer vtable, int size)
{
	SGenProtocolAlloc entry = { obj, vtable, size };
	protocol_entry (SGEN_PROTOCOL_ALLOC_PINNED, &entry, sizeof (SGenProtocolAlloc));
}

void
binary_protocol_alloc_degraded (gpointer obj, gpointer vtable, int size)
{
	SGenProtocolAlloc entry = { obj, vtable, size };
	protocol_entry (SGEN_PROTOCOL_ALLOC_DEGRADED, &entry, sizeof (SGenProtocolAlloc));
}

void
binary_protocol_copy (gpointer from, gpointer to, gpointer vtable, int size)
{
	SGenProtocolCopy entry = { from, to, vtable, size };
	protocol_entry (SGEN_PROTOCOL_COPY, &entry, sizeof (SGenProtocolCopy));
}

void
binary_protocol_pin (gpointer obj, gpointer vtable, int size)
{
	SGenProtocolPin entry = { obj, vtable, size };
	protocol_entry (SGEN_PROTOCOL_PIN, &entry, sizeof (SGenProtocolPin));
}

void
binary_protocol_mark (gpointer obj, gpointer vtable, int size)
{
	SGenProtocolMark entry = { obj, vtable, size };
	protocol_entry (SGEN_PROTOCOL_MARK, &entry, sizeof (SGenProtocolMark));
}

void
binary_protocol_scan_begin (gpointer obj, gpointer vtable, int size)
{
	SGenProtocolScanBegin entry = { obj, vtable, size };
	protocol_entry (SGEN_PROTOCOL_SCAN_BEGIN, &entry, sizeof (SGenProtocolScanBegin));
}

void
binary_protocol_scan_vtype_begin (gpointer obj, int size)
{
	SGenProtocolScanVTypeBegin entry = { obj, size };
	protocol_entry (SGEN_PROTOCOL_SCAN_VTYPE_BEGIN, &entry, sizeof (SGenProtocolScanVTypeBegin));
}

void
binary_protocol_wbarrier (gpointer ptr, gpointer value, gpointer value_vtable)
{
	SGenProtocolWBarrier entry = { ptr, value, value_vtable };
	protocol_entry (SGEN_PROTOCOL_WBARRIER, &entry, sizeof (SGenProtocolWBarrier));
}

void
binary_protocol_global_remset (gpointer ptr, gpointer value, gpointer value_vtable)
{
	SGenProtocolGlobalRemset entry = { ptr, value, value_vtable };
	protocol_entry (SGEN_PROTOCOL_GLOBAL_REMSET, &entry, sizeof (SGenProtocolGlobalRemset));
}

void
binary_protocol_ptr_update (gpointer ptr, gpointer old_value, gpointer new_value, gpointer vtable, int size)
{
	SGenProtocolPtrUpdate entry = { ptr, old_value, new_value, vtable, size };
	protocol_entry (SGEN_PROTOCOL_PTR_UPDATE, &entry, sizeof (SGenProtocolPtrUpdate));
}

void
binary_protocol_cleanup (gpointer ptr, gpointer vtable, int size)
{
	SGenProtocolCleanup entry = { ptr, vtable, size };
	protocol_entry (SGEN_PROTOCOL_CLEANUP, &entry, sizeof (SGenProtocolCleanup));
}

void
binary_protocol_empty (gpointer start, int size)
{
	SGenProtocolEmpty entry = { start, size };
	protocol_entry (SGEN_PROTOCOL_EMPTY, &entry, sizeof (SGenProtocolEmpty));
}

void
binary_protocol_card_scan (gpointer start, int size)
{
	SGenProtocolCardScan entry = { start, size };
	protocol_entry (SGEN_PROTOCOL_CARD_SCAN, &entry, sizeof (SGenProtocolCardScan));
}

void
binary_protocol_dislink_update (gpointer link, gpointer obj, int track, int staged)
{
	SGenProtocolDislinkUpdate entry = { link, obj, track, staged };
	protocol_entry (SGEN_PROTOCOL_DISLINK_UPDATE, &entry, sizeof (SGenProtocolDislinkUpdate));
}

void
binary_protocol_dislink_update_staged (gpointer link, gpointer obj, int track, int index)
{
	SGenProtocolDislinkUpdateStaged entry = { link, obj, track, index };
	protocol_entry (SGEN_PROTOCOL_DISLINK_UPDATE_STAGED, &entry, sizeof (SGenProtocolDislinkUpdateStaged));
}

void
binary_protocol_dislink_process_staged (gpointer link, gpointer obj, int index)
{
	SGenProtocolDislinkProcessStaged entry = { link, obj, index };
	protocol_entry (SGEN_PROTOCOL_DISLINK_PROCESS_STAGED, &entry, sizeof (SGenProtocolDislinkProcessStaged));
}

void
binary_protocol_gray_enqueue (gpointer queue, gpointer cursor, gpointer value)
{
	SGenProtocolGrayQueue entry = { queue, cursor, value };
	protocol_entry (SGEN_PROTOCOL_GRAY_ENQUEUE, &entry, sizeof (SGenProtocolGrayQueue));
}

void
binary_protocol_gray_dequeue (gpointer queue, gpointer cursor, gpointer value)
{
	SGenProtocolGrayQueue entry = { queue, cursor, value };
	protocol_entry (SGEN_PROTOCOL_GRAY_DEQUEUE, &entry, sizeof (SGenProtocolGrayQueue));
}
#endif

#endif /* HAVE_SGEN_GC */
