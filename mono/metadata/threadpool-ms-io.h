#ifndef _MONO_THREADPOOL_MS_IO_H_
#define _MONO_THREADPOOL_MS_IO_H_

#include <config.h>
#include <glib.h>

#include <mono/metadata/object-internals.h>

typedef struct _ThreadPoolIOBackendEvent ThreadPoolIOBackendEvent;

gpointer
ves_icall_System_IOSelector_BackendInitialize (gpointer wakeup_pipe_handle);

void
ves_icall_System_IOSelector_BackendCleanup (gpointer backend);

void
ves_icall_System_IOSelector_BackendAddHandle (gpointer backend, gpointer handle, gint32 operations, MonoBoolean is_new);

void
ves_icall_System_IOSelector_BackendRemoveHandle (gpointer backend, gpointer handle);

void
ves_icall_System_IOSelector_BackendPoll (gpointer backend, MonoArray *events_array);

#endif /* _MONO_THREADPOOL_MS_IO_H_ */
