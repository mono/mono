/*
 * locales.h: Culture-sensitive handling
 *
 * Authors:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2003 Ximian, Inc.
 */

#ifndef _MONO_METADATA_LOCALES_H_
#define _MONO_METADATA_LOCALES_H_

#include <glib.h>

#include <mono/metadata/object.h>

typedef struct _MonoCalendarData MonoCalendarData;
typedef struct _MonoCompareInfo MonoCompareInfo;
typedef struct _MonoCultureData MonoCultureData;
typedef struct _MonoCultureInfo MonoCultureInfo;
typedef struct _MonoDateTimeFormatInfo MonoDateTimeFormatInfo;
typedef struct _MonoNumberFormatInfo MonoNumberFormatInfo;
typedef struct _MonoRegionInfo MonoRegionInfo;
typedef struct _MonoSortKey MonoSortKey;

MonoBoolean
ves_icall_System_Globalization_CalendarData_fill_calendar_data (MonoCalendarData *this_obj, MonoString *name, gint32 calendar_index);

void
ves_icall_System_Globalization_CultureData_fill_culture_data (MonoCultureData *this_obj, gint32 datetime_index);

void
ves_icall_System_Globalization_CultureData_fill_number_data (MonoNumberFormatInfo* number, gint32 number_index);

void
ves_icall_System_Globalization_CultureInfo_construct_internal_locale (MonoCultureInfo *this_obj, MonoString *locale);

MonoString*
ves_icall_System_Globalization_CultureInfo_get_current_locale_name (void);

MonoBoolean
ves_icall_System_Globalization_CultureInfo_construct_internal_locale_from_lcid (MonoCultureInfo *this_obj, gint lcid);

MonoBoolean
ves_icall_System_Globalization_CultureInfo_construct_internal_locale_from_name (MonoCultureInfo *this_obj, MonoString *name);

MonoArray*
ves_icall_System_Globalization_CultureInfo_internal_get_cultures (MonoBoolean neutral, MonoBoolean specific, MonoBoolean installed);

int
ves_icall_System_Globalization_CompareInfo_internal_compare (MonoCompareInfo *this_obj, MonoString *str1, gint32 off1, gint32 len1, MonoString *str2, gint32 off2, gint32 len2, gint32 options);

MonoBoolean
ves_icall_System_Globalization_RegionInfo_construct_internal_region_from_lcid (MonoRegionInfo *this_obj, gint lcid);

MonoBoolean
ves_icall_System_Globalization_RegionInfo_construct_internal_region_from_name (MonoRegionInfo *this_obj, MonoString *name);

void
ves_icall_System_Globalization_CompareInfo_assign_sortkey (MonoCompareInfo *this_obj, MonoSortKey *key, MonoString *source, gint32 options);

int
ves_icall_System_Globalization_CompareInfo_internal_index (MonoCompareInfo *this_obj, MonoString *source, gint32 sindex, gint32 count, MonoString *value, gint32 options, MonoBoolean first);

int
ves_icall_System_Globalization_CompareInfo_internal_index_char (MonoCompareInfo *this_obj, MonoString *source, gint32 sindex, gint32 count, gunichar2 value, gint32 options, MonoBoolean first);

int
ves_icall_System_Threading_Thread_current_lcid (void);

void
ves_icall_System_Text_Normalization_load_normalization_resource (guint8 **argProps, guint8** argMappedChars, guint8** argCharMapIndex, guint8** argHelperIndex, guint8** argMapIdxToComposite, guint8** argCombiningClass);

#endif /* _MONO_METADATA_FILEIO_H_ */
