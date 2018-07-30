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
#ifndef __MONO_UTILS_SAFE_HANDLE__
#define __MONO_UTILS_SAFE_HANDLE__

#include "mono/utils/mono-coop-mutex.h"

typedef union {
	gint64 mem_start;
	struct {
			gboolean freeing : 1; // Either 0x1 or 0x0
			gint64 waiters : (sizeof (gint64) * 8) - 1;
	}; 
} MonoSafeHandleNativeState;

typedef struct {
	MonoCoopMutex *synch_cs;
	MonoSafeHandleNativeState lock_flags;
} MonoSafeHandleNative;

gboolean
mono_safe_handle_try_lock (MonoSafeHandleNative *handle);

void
mono_safe_handle_lock (MonoSafeHandleNative *handle);

void
mono_safe_handle_unlock (MonoSafeHandleNative *handle);

void
mono_safe_handle_free (MonoSafeHandleNative *handle);

#endif // MONO_UTILS_SAFE_HANDLE
