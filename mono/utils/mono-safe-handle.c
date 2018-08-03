/**
 * \file
 * Support for synchronously freeing unmanaged memory subject to
 * concurrent refcount modification
 *
 * Author:
 *   Alexander Kyte (alkyte@microsoft.com)
 *
 * (C) 2018 Microsoft, Inc.
 *
 */

#include <config.h>
#include <glib.h>
#include "utils/mono-safe-handle.h"
#include "utils/atomic.h"

#define INTERNAL_HANDLE_TOMBSTONE (GINT_TO_POINTER (-1))

static void 
ensure_synch_cs_set (MonoSafeHandleNative *handle)
{
	MonoCoopMutex *synch_cs;

	if (handle->synch_cs != NULL) {
		return;
	}

	synch_cs = g_new0 (MonoCoopMutex, 1);
	mono_coop_mutex_init_recursive (synch_cs);

	gpointer ret = mono_atomic_cas_ptr ((gpointer *)&handle->synch_cs, synch_cs, NULL);

	if (ret != NULL) {
		/* Another handle must have installed this CS */
		/* Or the finalizer had to have run */
		mono_coop_mutex_destroy (synch_cs);
		g_free (synch_cs);
	}
}

static void
internal_handle_free (MonoSafeHandleNative *handle)
{
	MonoCoopMutex *synch_cs = handle->synch_cs;
	handle->synch_cs = INTERNAL_HANDLE_TOMBSTONE;
	mono_memory_barrier ();

	mono_coop_mutex_destroy (synch_cs);
	g_free (synch_cs);
}

static void
change_wait_queue (MonoSafeHandleNative *handle, int diff)
{
	g_assert (diff == 1 || diff == -1);

	mono_memory_barrier ();
	MonoSafeHandleNativeState flags;
	flags.mem_start = mono_atomic_load_i64 (&handle->lock_flags.mem_start);

	while (TRUE) {
		MonoSafeHandleNativeState old_flags = flags;
		flags.waiters += diff;
		MonoSafeHandleNativeState seen_old_flags;
		seen_old_flags.mem_start = mono_atomic_cas_i64 (&handle->lock_flags.mem_start, flags.mem_start, old_flags.mem_start);

		// flags was set.
		if (seen_old_flags.mem_start == old_flags.mem_start)
			break;
		else
			flags.mem_start = seen_old_flags.mem_start;
	}

	if (!handle->synch_cs)
		ensure_synch_cs_set (handle);
}

static void
decrement_wait_queue (MonoSafeHandleNative *handle)
{
	change_wait_queue (handle, -1);
}

static void
increment_wait_queue (MonoSafeHandleNative *handle)
{
	change_wait_queue (handle, 1);
}

static MonoCoopMutex *
get_lock (MonoSafeHandleNative *handle, gboolean assert_fine)
{
	increment_wait_queue (handle);

	MonoCoopMutex *ptr = (MonoCoopMutex *) mono_atomic_load_ptr ((volatile gpointer *) &handle->synch_cs);

	gboolean fine = ptr && (ptr != INTERNAL_HANDLE_TOMBSTONE);
	if (assert_fine && !fine)
		g_error ("Attempted to lock a freed lock without fallback");

	if (ptr == INTERNAL_HANDLE_TOMBSTONE)
		return NULL;
	else
		return ptr;
}

gboolean
mono_safe_handle_try_lock (MonoSafeHandleNative *handle)
{
	MonoCoopMutex *lock = get_lock (handle, FALSE);
	if (lock)
		mono_coop_mutex_lock (lock);

	return lock != NULL;
}

void
mono_safe_handle_lock (MonoSafeHandleNative *handle)
{
	mono_coop_mutex_lock (get_lock (handle, TRUE));
}

void
mono_safe_handle_free (MonoSafeHandleNative *handle)
{
	if (handle->synch_cs == NULL || handle->synch_cs == INTERNAL_HANDLE_TOMBSTONE)
		return;

	// FIXME: document expectations of callbacks
	// already disposed
	if (mono_safe_handle_try_lock (handle))
		return;

	mono_memory_barrier ();
	MonoSafeHandleNativeState flags;
	flags.mem_start = mono_atomic_load_i64 (&handle->lock_flags.mem_start);

	while (!flags.freeing) {
		MonoSafeHandleNativeState old_flags = flags;
		flags.freeing = TRUE;
		flags.mem_start = mono_atomic_cas_i64 (&handle->lock_flags.mem_start, flags.mem_start, old_flags.mem_start);
	}

	// If we started cleanup without any writers, finish the freeing
	// ourselves. Else the handle who unlocks the lock after the last pending 
	// wait is finished will do the freeing.
	mono_safe_handle_unlock (handle);
}

void
mono_safe_handle_unlock (MonoSafeHandleNative *handle)
{
	MonoCoopMutex *lock = (MonoCoopMutex *) mono_atomic_load_ptr ((volatile gpointer *) &handle->synch_cs);
	mono_coop_mutex_unlock (lock);

	// Now the lock can be freed
	decrement_wait_queue (handle);

	mono_memory_barrier ();
	MonoSafeHandleNativeState flags;
	flags.mem_start = mono_atomic_load_i64 (&handle->lock_flags.mem_start);

	if (flags.waiters == 0 && flags.freeing)
		internal_handle_free (handle);
}

