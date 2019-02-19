/**
 * \file
 * Culture-sensitive handling
 *
 * Authors:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2003 Ximian, Inc.
 */

#ifndef _MONO_METADATA_LOCALES_H_
#define _MONO_METADATA_LOCALES_H_

#include <glib.h>

#include <mono/metadata/object-internals.h>
#include <mono/metadata/icalls.h>

/* This is a copy of System.Globalization.CompareOptions */
typedef enum {
	CompareOptions_None=0x00,
	CompareOptions_IgnoreCase=0x01,
	CompareOptions_IgnoreNonSpace=0x02,
	CompareOptions_IgnoreSymbols=0x04,
	CompareOptions_IgnoreKanaType=0x08,
	CompareOptions_IgnoreWidth=0x10,
	CompareOptions_StringSort=0x20000000,
	CompareOptions_Ordinal=0x40000000
} MonoCompareOptions;

ICALL_EXPORT
MonoBoolean ves_icall_System_Globalization_CalendarData_fill_calendar_data (MonoCalendarData *this_obj, MonoString *name, gint32 calendar_index);

ICALL_EXPORT
void ves_icall_System_Globalization_CultureData_fill_culture_data (MonoCultureData *this_obj, gint32 datetime_index);

ICALL_EXPORT
void ves_icall_System_Globalization_CultureData_fill_number_data (MonoNumberFormatInfo* number, gint32 number_index);

ICALL_EXPORT
void ves_icall_System_Globalization_CultureInfo_construct_internal_locale (MonoCultureInfo *this_obj, MonoString *locale);

ICALL_EXPORT
MonoBoolean ves_icall_System_Globalization_CultureInfo_construct_internal_locale_from_lcid (MonoCultureInfo *this_obj, gint lcid);

ICALL_EXPORT
MonoBoolean ves_icall_System_Globalization_CultureInfo_construct_internal_locale_from_name (MonoCultureInfo *this_obj, MonoString *name);

ICALL_EXPORT
MonoArray *ves_icall_System_Globalization_CultureInfo_internal_get_cultures (MonoBoolean neutral, MonoBoolean specific, MonoBoolean installed);

ICALL_EXPORT
void ves_icall_System_Globalization_CompareInfo_construct_compareinfo (MonoCompareInfo *comp, MonoString *locale);

ICALL_EXPORT int
ves_icall_System_Globalization_CompareInfo_internal_compare (const gunichar2 *str1, gint32 len1,
	const gunichar2 *str2, gint32 len2, gint32 options);

ICALL_EXPORT
void ves_icall_System_Globalization_CompareInfo_free_internal_collator (MonoCompareInfo *this_obj);

ICALL_EXPORT
MonoBoolean
ves_icall_System_Globalization_RegionInfo_construct_internal_region_from_name (MonoRegionInfo *this_obj,
 MonoString *name);

ICALL_EXPORT int
ves_icall_System_Globalization_CompareInfo_internal_index (const gunichar2 *source, gint32 sindex,
	gint32 count, const gunichar2 *value, int value_length, MonoBoolean first);

#define MONO_LOCALE_INVARIANT (0x007F)

#endif /* _MONO_METADATA_FILEIO_H_ */
