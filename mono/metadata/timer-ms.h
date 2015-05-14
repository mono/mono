#ifndef _MONO_TIMER_MS_H_
#define _MONO_TIMER_MS_H_

#include <glib.h>

#include "class-internals.h"
#include "metadata.h"
#include "object.h"

MonoSafeHandle*
ves_icall_System_Threading_TimerQueue_CreateAppDomainTimer (guint due_time);

MonoBoolean
ves_icall_System_Threading_TimerQueue_ChangeAppDomainTimer (MonoSafeHandle *handle, guint due_time);

MonoBoolean
ves_icall_System_Threading_TimerQueue_DeleteAppDomainTimer (gpointer handle);

#endif /* _MONO_TIMER_MS_H_ */
