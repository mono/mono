
/* This is just a hack so we can call mono_w32mutex_abandon
 * from mono/utils/mono-threads-posix.c, without importing
 * the whole object.h
 * In the best of all world, mutex owning + disowning + abandoning
 * should be done in metadata/ */

#ifndef _MONO_METADATA_W32MUTEX_UTILS_H_
#define _MONO_METADATA_W32MUTEX_UTILS_H_

#include <config.h>
#include <glib.h>

#include "mono/utils/mono-threads.h"
#include "mono/io-layer/wapi.h"

void
mono_w32mutex_abandon (gpointer handle, MonoNativeThreadId tid);

typedef struct MonoW32HandleNamedMutex MonoW32HandleNamedMutex;

WapiSharedNamespace*
mono_w32mutex_get_namespace (MonoW32HandleNamedMutex *mutex);

#endif /* _MONO_METADATA_W32MUTEX_UTILS_H_ */

