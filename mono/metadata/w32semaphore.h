
#ifndef _MONO_METADATA_W32SEMAPHORE_H_
#define _MONO_METADATA_W32SEMAPHORE_H_

#include <config.h>
#include <glib.h>

#include "object.h"

gpointer
ves_icall_System_Threading_Semaphore_CreateSemaphore_internal (gint32 initialCount, gint32 maximumCount, MonoString *name, gint32 *error);

MonoBoolean
ves_icall_System_Threading_Semaphore_ReleaseSemaphore_internal (gpointer handle, gint32 releaseCount, gint32 *prevcount);

gpointer
ves_icall_System_Threading_Semaphore_OpenSemaphore_internal (MonoString *name, gint32 rights, gint32 *error);

#endif /* _MONO_METADATA_W32SEMAPHORE_H_ */
