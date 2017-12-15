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

void*
ves_icall_System_Diagnostics_PerformanceCounter_GetImpl (const gunichar2* category,
	gint32 category_length, const gunichar2* counter, gint32 counter_length,
	const gunichar2* instance, gint32 instance_length, int *type, MonoBoolean *custom,
	MonoError *error);

MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounter_GetSample (void *impl, MonoBoolean only_value,
	MonoCounterSample *sample, MonoError *error);

gint64
ves_icall_System_Diagnostics_PerformanceCounter_UpdateValue (void *impl, MonoBoolean do_incr,
	gint64 value, MonoError *error);

void
ves_icall_System_Diagnostics_PerformanceCounter_FreeData (void *impl, MonoError *error);

/* Category icalls */
MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounterCategory_CategoryDelete (const gunichar2* name,
	gint32 name_length, MonoError *error);

MonoStringHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_CategoryHelpInternal (
	const gunichar2* category, gint32 category_length, MonoError *error);

MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounterCategory_CounterCategoryExists (
	const gunichar2* counter, gint32 counter_length, const gunichar2* category,
	gint32 category_length, MonoError *error);

MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounterCategory_Create (const gunichar2* category,
	gint32 category_length, const gunichar2* help, gint32 help_length, int type,
	MonoArrayHandle items, MonoError *error);

MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounterCategory_InstanceExistsInternal (
	const gunichar2* instance, gint32 instance_length, const gunichar2* category,
	gint32 category_length, MonoError *error);
		
MonoArrayHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_GetCategoryNames (MonoError *error);

MonoArrayHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_GetCounterNames (const gunichar2* category,
	gint32 category_length, MonoError *error);

MonoArrayHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_GetInstanceNames (
	const gunichar2* category, gint32 category_length, MonoError *error);

typedef gboolean (*PerfCounterEnumCallback) (char *category_name, char *name, unsigned char type, gint64 value, gpointer user_data);
MONO_API void mono_perfcounter_foreach (PerfCounterEnumCallback cb, gpointer user_data);

#endif /* __MONO_PERFCOUNTERS_H__ */
