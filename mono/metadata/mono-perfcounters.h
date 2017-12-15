/**
 * \file
 */

#ifndef __MONO_PERFCOUNTERS_H__
#define __MONO_PERFCOUNTERS_H__

#include <glib.h>
#include <mono/metadata/object.h>
#include <mono/utils/mono-compiler.h>
#include <mono/metadata/handle.h>

typedef struct _MonoCounterSample MonoCounterSample;

void* ves_icall_System_Diagnostics_PerformanceCounter_GetImpl (
		MonoStringHandle category, MonoStringHandle counter, MonoStringHandle instance,
		MonoStringHandle machine, int *type, MonoBoolean *custom, MonoError *error);

MonoBoolean ves_icall_System_Diagnostics_PerformanceCounter_GetSample (void *impl,
		MonoBoolean only_value, MonoCounterSample *sample, MonoError *error);

gint64 ves_icall_System_Diagnostics_PerformanceCounter_UpdateValue (
	void *impl, MonoBoolean do_incr, gint64 value, MonoError *error);

void ves_icall_System_Diagnostics_PerformanceCounter_FreeData (void *impl, MonoError *error);

/* Category icalls */
MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounterCategory_CategoryDelete (
	MonoStringHandle name, MonoError *error);

MonoStringHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_CategoryHelpInternal (
		MonoStringHandle category, MonoStringHandle machine, MonoError *error);

MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounterCategory_CounterCategoryExists (
		MonoStringHandle counter, MonoStringHandle category, MonoStringHandle machine, MonoError *error);

MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounterCategory_Create (
		MonoStringHandle category, MonoStringHandle help, int type, MonoArrayHandle items, MonoError *error);

int
ves_icall_System_Diagnostics_PerformanceCounterCategory_InstanceExistsInternal (
		MonoStringHandle instance, MonoStringHandle category, MonoStringHandle machine, MonoError *error);
		
MonoArrayHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_GetCategoryNames (MonoStringHandle machine, MonoError *error);

MonoArrayHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_GetCounterNames (
	MonoStringHandle category, MonoStringHandle machine, MonoError *error);

MonoArrayHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_GetInstanceNames (
	MonoStringHandle category, MonoStringHandle machine, MonoError *error);

typedef gboolean (*PerfCounterEnumCallback) (char *category_name, char *name, unsigned char type, gint64 value, gpointer user_data);
MONO_API void mono_perfcounter_foreach (PerfCounterEnumCallback cb, gpointer user_data);

#endif /* __MONO_PERFCOUNTERS_H__ */

