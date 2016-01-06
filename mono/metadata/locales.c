/*
 * locales.c: Culture-sensitive handling
 *
 * Authors:
 *	Dick Porter (dick@ximian.com)
 *	Mohammad DAMT (mdamt@cdl2000.com)
 *	Marek Safar (marek.safar@gmail.com)
 *
 * Copyright 2003 Ximian, Inc (http://www.ximian.com)
 * Copyright 2004-2009 Novell, Inc (http://www.novell.com)
 * (C) 2003 PT Cakram Datalingga Duaribu  http://www.cdl2000.com
 * Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
 */

#include <config.h>
#include <glib.h>
#include <string.h>

#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/object.h>
#include <mono/metadata/object-internals.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/monitor.h>
#include <mono/metadata/locales.h>
#include <mono/metadata/culture-info.h>
#include <mono/metadata/culture-info-tables.h>
#include <mono/metadata/handle.h>
#include <mono/utils/bsearch.h>

#ifndef DISABLE_NORMALIZATION
#include <mono/metadata/normalization-tables.h>
#endif

#include <locale.h>
#if defined(__APPLE__)
#include <CoreFoundation/CoreFoundation.h>
#endif

#undef DEBUG

enum {
	CompareOptions_None = 0x00,
	CompareOptions_IgnoreCase = 0x01,
	CompareOptions_IgnoreNonSpace = 0x02,
	CompareOptions_IgnoreSymbols = 0x04,
	CompareOptions_IgnoreKanaType = 0x08,
	CompareOptions_IgnoreWidth = 0x10,
	CompareOptions_StringSort = 0x20000000,
	CompareOptions_Ordinal = 0x40000000,
};

struct _MonoCalendarData {
	MonoObject obj;
	MonoString *NativeName;
	MonoArray *ShortDatePatterns;
	MonoArray *YearMonthPatterns;
	MonoArray *LongDatePatterns;
	MonoString *MonthDayPattern;

	MonoArray *EraNames;
	MonoArray *AbbreviatedEraNames;
	MonoArray *AbbreviatedEnglishEraNames;
	MonoArray *DayNames;
	MonoArray *AbbreviatedDayNames;
	MonoArray *SuperShortDayNames;
	MonoArray *MonthNames;
	MonoArray *AbbreviatedMonthNames;
	MonoArray *GenitiveMonthNames;
	MonoArray *GenitiveAbbreviatedMonthNames;
};

struct _MonoCompareInfo {
	MonoObject obj;
	gint32 lcid;
	MonoString *icu_name;
	gpointer ICU_collator;
};

struct _MonoCultureData {
	MonoObject obj;
	MonoString *AMDesignator;
	MonoString *PMDesignator;
	MonoString *TimeSeparator;
	MonoArray *LongTimePatterns;
	MonoArray *ShortTimePatterns;
	guint32 FirstDayOfWeek;
	guint32 CalendarWeekRule;
};

struct _MonoCultureInfo {
	MonoObject obj;
	MonoBoolean is_read_only;
	gint32 lcid;
	gint32 parent_lcid;
	gint32 datetime_index;
	gint32 number_index;
	gint32 calendar_type;
	MonoBoolean use_user_override;
	MonoNumberFormatInfo *number_format;
	MonoDateTimeFormatInfo *datetime_format;
	MonoObject *textinfo;
	MonoString *name;
	MonoString *englishname;
	MonoString *nativename;
	MonoString *iso3lang;
	MonoString *iso2lang;
	MonoString *win3lang;
	MonoString *territory;
	MonoArray *native_calendar_names;
	MonoCompareInfo *compareinfo;
	const void* text_info_data;
};

struct _MonoDateTimeFormatInfo {
	MonoObject obj;
	MonoBoolean readOnly;
	MonoString *AMDesignator;
	MonoString *PMDesignator;
	MonoString *DateSeparator;
	MonoString *TimeSeparator;
	MonoString *ShortDatePattern;
	MonoString *LongDatePattern;
	MonoString *ShortTimePattern;
	MonoString *LongTimePattern;
	MonoString *MonthDayPattern;
	MonoString *YearMonthPattern;
	guint32 FirstDayOfWeek;
	guint32 CalendarWeekRule;
	MonoArray *AbbreviatedDayNames;
	MonoArray *DayNames;
	MonoArray *MonthNames;
	MonoArray *GenitiveMonthNames;
	MonoArray *AbbreviatedMonthNames;
	MonoArray *GenitiveAbbreviatedMonthNames;
	MonoArray *ShortDatePatterns;
	MonoArray *LongDatePatterns;
	MonoArray *ShortTimePatterns;
	MonoArray *LongTimePatterns;
	MonoArray *MonthDayPatterns;
	MonoArray *YearMonthPatterns;
	MonoArray *ShortestDayNames;
};

struct _MonoNumberFormatInfo {
	MonoObject obj;
	MonoArray *numberGroupSizes;
	MonoArray *currencyGroupSizes;
	MonoArray *percentGroupSizes;
	MonoString *positiveSign;
	MonoString *negativeSign;
	MonoString *numberDecimalSeparator;
	MonoString *numberGroupSeparator;
	MonoString *currencyGroupSeparator;
	MonoString *currencyDecimalSeparator;
	MonoString *currencySymbol;
	MonoString *ansiCurrencySymbol;	/* unused */
	MonoString *naNSymbol;
	MonoString *positiveInfinitySymbol;
	MonoString *negativeInfinitySymbol;
	MonoString *percentDecimalSeparator;
	MonoString *percentGroupSeparator;
	MonoString *percentSymbol;
	MonoString *perMilleSymbol;
	MonoString *nativeDigits; /* unused */
	gint32 dataItem; /* unused */
	guint32 numberDecimalDigits;
	gint32 currencyDecimalDigits;
	gint32 currencyPositivePattern;
	gint32 currencyNegativePattern;
	gint32 numberNegativePattern;
	gint32 percentPositivePattern;
	gint32 percentNegativePattern;
	gint32 percentDecimalDigits;
};

struct _MonoRegionInfo {
	MonoObject obj;
	gint32 geo_id;
	MonoString *iso2name;
	MonoString *iso3name;
	MonoString *win3name;
	MonoString *english_name;
	MonoString *native_name;
	MonoString *currency_symbol;
	MonoString *iso_currency_symbol;
	MonoString *currency_english_name;
	MonoString *currency_native_name;
};

struct _MonoSortKey {
	MonoObject obj;
	MonoString *str;
	gint32 options;
	MonoArray *key;
	gint32 lcid;
};

MONO_HANDLE_TYPE_DECL (MonoCalendarData);
MONO_HANDLE_TYPE_DECL (MonoCompareInfo);
MONO_HANDLE_TYPE_DECL (MonoCultureData);
MONO_HANDLE_TYPE_DECL (MonoCultureInfo);
MONO_HANDLE_TYPE_DECL (MonoNumberFormatInfo);
MONO_HANDLE_TYPE_DECL (MonoRegionInfo);
MONO_HANDLE_TYPE_DECL (MonoSortKey);

static int
culture_lcid_locator (const void *a, const void *b)
{
	const int *lcid = (const int *)a;
	const CultureInfoEntry *bb = (const CultureInfoEntry *)b;

	return *lcid - bb->lcid;
}

static int
culture_name_locator (const void *a, const void *b)
{
	const char *aa = (const char *)a;
	const CultureInfoNameEntry *bb = (const CultureInfoNameEntry *)b;
	int ret;
	
	ret = strcmp (aa, idx2string (bb->name));

	return ret;
}

static int
region_name_locator (const void *a, const void *b)
{
	const char *aa = (const char *)a;
	const RegionInfoNameEntry *bb = (const RegionInfoNameEntry *)b;
	int ret;
	
	ret = strcmp (aa, idx2string (bb->name));

	return ret;
}

static MONO_HANDLE_TYPE (MonoArray)
create_group_sizes_array (const gint *gs, gint ml, MonoError *error)
{
	MONO_HANDLE_TYPE (MonoArray) ret;
	gsize i, len = 0;

	for (i = 0; i < ml; i++) {
		if (gs [i] == -1)
			break;
		len++;
	}

	ret = mono_handle_array_new (mono_domain_get (), mono_get_int32_class (), len, error);
	if (!mono_error_ok (error))
		goto done;

	for (i = 0; i < len; i++)
		MONO_HANDLE_ARRAY_SET (ret, gint32, i, gs [i]);

done:
	return ret;
}

static MONO_HANDLE_TYPE (MonoArray)
create_names_array_idx (const guint16 *names, gsize ml, MonoError *error)
{
	MONO_HANDLE_TYPE (MonoArray) ret;
	MonoDomain *domain;
	gsize i;

	if (names == NULL)
		return MONO_HANDLE_NEW (MonoArray, NULL);

	domain = mono_domain_get ();

	ret = mono_handle_array_new (domain, mono_get_string_class (), ml, error);
	if (!mono_error_ok (error))
		goto done;

	MONO_HANDLE_ARENA_PUSH ();

	for (i = 0; i < ml; i++) {
		MONO_HANDLE_ARRAY_SETREF (ret, i, mono_handle_string_new (domain, idx2string (names [i]), error));
		if (!mono_error_ok (error))
			break;
	}

	MONO_HANDLE_ARENA_POP;

done:
	return ret;
}

static MONO_HANDLE_TYPE (MonoArray)
create_names_array_idx_dynamic (const guint16 *names, gsize ml, MonoError *error)
{
	MONO_HANDLE_TYPE (MonoArray) ret;
	MonoDomain *domain;
	gsize i, len = 0;

	if (names == NULL)
		return MONO_HANDLE_NEW (MonoArray, NULL);

	domain = mono_domain_get ();

	for (i = 0; i < ml; i++) {
		if (names [i] == 0)
			break;
		len++;
	}

	ret = mono_handle_array_new (domain, mono_get_string_class (), len, error);
	if (!mono_error_ok (error))
		goto done;

	MONO_HANDLE_ARENA_PUSH ();

	for (i = 0; i < len; i++) {
		MONO_HANDLE_ARRAY_SETREF (ret, i, mono_handle_string_new (domain, idx2string (names [i]), error));
		if (!mono_error_ok (error))
			break;
	}

	MONO_HANDLE_ARENA_POP;

done:
	return ret;
}

MonoBoolean
ves_icall_System_Globalization_CalendarData_fill_calendar_data (MonoCalendarData *this_obj, MonoString *name, gint32 calendar_index)
{
	MonoBoolean ret;

	MONO_HANDLE_ARENA_PUSH ();

	MONO_HANDLE_TYPE (MonoCalendarData) this_obj_handle = MONO_HANDLE_NEW (MonoCalendarData, this_obj);
	MONO_HANDLE_TYPE (MonoString) name_handle = MONO_HANDLE_NEW (MonoString, name);

	MonoError error;
	gchar *name_utf8;
	const CultureInfoNameEntry *name_entry;

	ret = TRUE;

	name_utf8 = mono_handle_string_to_utf8 (name_handle, &error);
	g_assert (mono_error_ok (&error));

	name_entry = mono_binary_search (name_utf8, culture_name_entries, NUM_CULTURE_ENTRIES, sizeof (CultureInfoNameEntry), culture_name_locator);

	g_free (name_utf8);

	if (!name_entry) {
		ret = FALSE;
	} else {
		MonoDomain *domain;
		const DateTimeFormatEntry *dfe;
		const CultureInfoEntry *ci;

		domain = mono_domain_get ();

		ci = &culture_entries [name_entry->culture_entry_index];
		dfe = &datetime_format_entries [ci->datetime_format_index];

		MONO_HANDLE_SETREF (this_obj_handle, NativeName, mono_handle_string_new (domain, idx2string (ci->nativename), &error));
		g_assert (mono_error_ok (&error));

		MONO_HANDLE_SETREF (this_obj_handle, ShortDatePatterns, create_names_array_idx_dynamic (dfe->short_date_patterns, NUM_SHORT_DATE_PATTERNS, &error));
		g_assert (mono_error_ok (&error));

		MONO_HANDLE_SETREF (this_obj_handle, YearMonthPatterns, create_names_array_idx_dynamic (dfe->year_month_patterns, NUM_YEAR_MONTH_PATTERNS, &error));
		g_assert (mono_error_ok (&error));

		MONO_HANDLE_SETREF (this_obj_handle, LongDatePatterns, create_names_array_idx_dynamic (dfe->long_date_patterns, NUM_LONG_DATE_PATTERNS, &error));
		g_assert (mono_error_ok (&error));

		MONO_HANDLE_SETREF (this_obj_handle, MonthDayPattern, mono_handle_string_new (domain, idx2string (dfe->month_day_pattern), &error));
		g_assert (mono_error_ok (&error));

		MONO_HANDLE_SETREF (this_obj_handle, DayNames, create_names_array_idx (dfe->day_names, NUM_DAYS, &error));
		g_assert (mono_error_ok (&error));

		MONO_HANDLE_SETREF (this_obj_handle, AbbreviatedDayNames, create_names_array_idx (dfe->abbreviated_day_names, NUM_DAYS, &error));
		g_assert (mono_error_ok (&error));

		MONO_HANDLE_SETREF (this_obj_handle, SuperShortDayNames, create_names_array_idx (dfe->shortest_day_names, NUM_DAYS, &error));
		g_assert (mono_error_ok (&error));

		MONO_HANDLE_SETREF (this_obj_handle, MonthNames, create_names_array_idx (dfe->month_names, NUM_MONTHS, &error));
		g_assert (mono_error_ok (&error));

		MONO_HANDLE_SETREF (this_obj_handle, AbbreviatedMonthNames, create_names_array_idx (dfe->abbreviated_month_names, NUM_MONTHS, &error));
		g_assert (mono_error_ok (&error));

		MONO_HANDLE_SETREF (this_obj_handle, GenitiveMonthNames, create_names_array_idx (dfe->month_genitive_names, NUM_MONTHS, &error));
		g_assert (mono_error_ok (&error));

		MONO_HANDLE_SETREF (this_obj_handle, GenitiveAbbreviatedMonthNames, create_names_array_idx (dfe->abbreviated_month_genitive_names, NUM_MONTHS, &error));
		g_assert (mono_error_ok (&error));
	}

	MONO_HANDLE_ARENA_POP;

	return ret;
}

void
ves_icall_System_Globalization_CultureData_fill_culture_data (MonoCultureData *this_obj, gint32 datetime_index)
{
	MONO_HANDLE_ARENA_PUSH ();

	MONO_HANDLE_TYPE (MonoCultureData) this_obj_handle = MONO_HANDLE_NEW (MonoCultureData, this_obj);

	MonoError error;
	MonoDomain *domain;
	const DateTimeFormatEntry *dfe;

	g_assert (datetime_index >= 0);
	dfe = &datetime_format_entries [datetime_index];

	domain = mono_domain_get ();

	MONO_HANDLE_SETREF (this_obj_handle, AMDesignator, mono_handle_string_new (domain, idx2string (dfe->am_designator), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, PMDesignator, mono_handle_string_new (domain, idx2string (dfe->pm_designator), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, TimeSeparator, mono_handle_string_new (domain, idx2string (dfe->time_separator), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, LongTimePatterns, create_names_array_idx_dynamic (dfe->long_time_patterns, NUM_LONG_TIME_PATTERNS, &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, ShortTimePatterns, create_names_array_idx_dynamic (dfe->short_time_patterns, NUM_SHORT_TIME_PATTERNS, &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SET (this_obj_handle, FirstDayOfWeek, dfe->first_day_of_week);

	MONO_HANDLE_SET (this_obj_handle, CalendarWeekRule, dfe->calendar_week_rule);

	MONO_HANDLE_ARENA_POP;
}

void
ves_icall_System_Globalization_CultureData_fill_number_data (MonoNumberFormatInfo* number, gint32 number_index)
{
	MONO_HANDLE_ARENA_PUSH ();

	MONO_HANDLE_TYPE (MonoNumberFormatInfo) number_handle = MONO_HANDLE_NEW (MonoNumberFormatInfo, number);

	MonoError error;
	MonoDomain *domain;
	const NumberFormatEntry *nfe;

	g_assert (number_index >= 0);
	nfe = &number_format_entries [number_index];

	domain = mono_domain_get ();

	MONO_HANDLE_SET (number_handle, currencyDecimalDigits, nfe->currency_decimal_digits);

	MONO_HANDLE_SETREF (number_handle, currencyDecimalSeparator, mono_handle_string_new (domain, idx2string (nfe->currency_decimal_separator), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (number_handle, currencyGroupSeparator, mono_handle_string_new (domain, idx2string (nfe->currency_group_separator), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (number_handle, currencyGroupSizes, create_group_sizes_array (nfe->currency_group_sizes, GROUP_SIZE, &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SET (number_handle, currencyNegativePattern, nfe->currency_negative_pattern);

	MONO_HANDLE_SET (number_handle, currencyPositivePattern, nfe->currency_positive_pattern);

	MONO_HANDLE_SETREF (number_handle, currencySymbol, mono_handle_string_new (domain, idx2string (nfe->currency_symbol), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (number_handle, naNSymbol, mono_handle_string_new (domain, idx2string (nfe->nan_symbol), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (number_handle, negativeInfinitySymbol, mono_handle_string_new (domain, idx2string (nfe->negative_infinity_symbol), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (number_handle, negativeSign, mono_handle_string_new (domain, idx2string (nfe->negative_sign), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SET (number_handle, numberDecimalDigits, nfe->number_decimal_digits);

	MONO_HANDLE_SETREF (number_handle, numberDecimalSeparator, mono_handle_string_new (domain, idx2string (nfe->number_decimal_separator), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (number_handle, numberGroupSeparator, mono_handle_string_new (domain, idx2string (nfe->number_group_separator), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (number_handle, numberGroupSizes, create_group_sizes_array (nfe->number_group_sizes, GROUP_SIZE, &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SET (number_handle, numberNegativePattern, nfe->number_negative_pattern);

	MONO_HANDLE_SET (number_handle, percentNegativePattern, nfe->percent_negative_pattern);

	MONO_HANDLE_SET (number_handle, percentPositivePattern, nfe->percent_positive_pattern);

	MONO_HANDLE_SETREF (number_handle, percentSymbol, mono_handle_string_new (domain, idx2string (nfe->percent_symbol), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (number_handle, perMilleSymbol, mono_handle_string_new (domain, idx2string (nfe->per_mille_symbol), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (number_handle, positiveInfinitySymbol, mono_handle_string_new (domain, idx2string (nfe->positive_infinity_symbol), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (number_handle, positiveSign, mono_handle_string_new (domain, idx2string (nfe->positive_sign), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_ARENA_POP;
}

static MonoBoolean
construct_culture (MONO_HANDLE_TYPE (MonoCultureInfo) this_obj_handle, const CultureInfoEntry *ci)
{
	MONO_HANDLE_ARENA_PUSH ();

	MonoError error;
	MonoDomain *domain;

	domain = mono_domain_get ();

	MONO_HANDLE_SET (this_obj_handle, lcid, ci->lcid);

	MONO_HANDLE_SETREF (this_obj_handle, name, mono_handle_string_new (domain, idx2string (ci->name), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, englishname, mono_handle_string_new (domain, idx2string (ci->englishname), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, nativename, mono_handle_string_new (domain, idx2string (ci->nativename), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, win3lang, mono_handle_string_new (domain, idx2string (ci->win3lang), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, iso3lang, mono_handle_string_new (domain, idx2string (ci->iso3lang), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, iso2lang, mono_handle_string_new (domain, idx2string (ci->iso2lang), &error));
	g_assert (mono_error_ok (&error));

	// It's null for neutral cultures
	if (ci->territory > 0) {
		MONO_HANDLE_SETREF (this_obj_handle, territory, mono_handle_string_new (domain, idx2string (ci->territory), &error));
		g_assert (mono_error_ok (&error));
	}

	MONO_HANDLE_SETREF (this_obj_handle, native_calendar_names, create_names_array_idx (ci->native_calendar_names, NUM_CALENDARS, &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SET (this_obj_handle, parent_lcid, ci->parent_lcid);

	MONO_HANDLE_SET (this_obj_handle, datetime_index, ci->datetime_format_index);

	MONO_HANDLE_SET (this_obj_handle, number_index, ci->number_format_index);

	MONO_HANDLE_SET (this_obj_handle, calendar_type, ci->calendar_type);

	MONO_HANDLE_SET (this_obj_handle, text_info_data, &ci->text_info);

	MONO_HANDLE_ARENA_POP;

	return TRUE;
}

static MonoBoolean
construct_region (MONO_HANDLE_TYPE (MonoRegionInfo) this_obj_handle, const RegionInfoEntry *ri)
{
	MONO_HANDLE_ARENA_PUSH ();

	MonoError error;
	MonoDomain *domain;

	domain = mono_domain_get ();

	MONO_HANDLE_SET (this_obj_handle, geo_id, ri->geo_id);

	MONO_HANDLE_SETREF (this_obj_handle, iso2name, mono_handle_string_new (domain, idx2string (ri->iso2name), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, iso3name, mono_handle_string_new (domain, idx2string (ri->iso3name), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, win3name, mono_handle_string_new (domain, idx2string (ri->win3name), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, english_name, mono_handle_string_new (domain, idx2string (ri->english_name), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, native_name, mono_handle_string_new (domain, idx2string (ri->native_name), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, currency_symbol, mono_handle_string_new (domain, idx2string (ri->currency_symbol), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, iso_currency_symbol, mono_handle_string_new (domain, idx2string (ri->iso_currency_symbol), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, currency_english_name, mono_handle_string_new (domain, idx2string (ri->currency_english_name), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_SETREF (this_obj_handle, currency_native_name, mono_handle_string_new (domain, idx2string (ri->currency_native_name), &error));
	g_assert (mono_error_ok (&error));

	MONO_HANDLE_ARENA_POP;

	return TRUE;
}

#if defined (__APPLE__)
static gchar*
get_darwin_locale (void)
{
	static gchar *darwin_locale = NULL;
	CFLocaleRef locale = NULL;
	CFStringRef locale_language = NULL;
	CFStringRef locale_country = NULL;
	CFStringRef locale_script = NULL;
	CFStringRef locale_cfstr = NULL;
	CFIndex bytes_converted;
	CFIndex bytes_written;
	CFIndex len;
	int i;

	if (darwin_locale != NULL)
		return g_strdup (darwin_locale);

	locale = CFLocaleCopyCurrent ();

	if (locale) {
		locale_language = CFLocaleGetValue (locale, kCFLocaleLanguageCode);
		if (locale_language != NULL && CFStringGetBytes(locale_language, CFRangeMake (0, CFStringGetLength (locale_language)), kCFStringEncodingMacRoman, 0, FALSE, NULL, 0, &bytes_converted) > 0) {
			len = bytes_converted + 1;

			locale_country = CFLocaleGetValue (locale, kCFLocaleCountryCode);
			if (locale_country != NULL && CFStringGetBytes (locale_country, CFRangeMake (0, CFStringGetLength (locale_country)), kCFStringEncodingMacRoman, 0, FALSE, NULL, 0, &bytes_converted) > 0) {
				len += bytes_converted + 1;

				locale_script = CFLocaleGetValue (locale, kCFLocaleScriptCode);
				if (locale_script != NULL && CFStringGetBytes (locale_script, CFRangeMake (0, CFStringGetLength (locale_script)), kCFStringEncodingMacRoman, 0, FALSE, NULL, 0, &bytes_converted) > 0) {
					len += bytes_converted + 1;
				}

				darwin_locale = (char *) malloc (len + 1);
				CFStringGetBytes (locale_language, CFRangeMake (0, CFStringGetLength (locale_language)), kCFStringEncodingMacRoman, 0, FALSE, (UInt8 *) darwin_locale, len, &bytes_converted);

				darwin_locale[bytes_converted] = '-';
				bytes_written = bytes_converted + 1;
				if (locale_script != NULL && CFStringGetBytes (locale_script, CFRangeMake (0, CFStringGetLength (locale_script)), kCFStringEncodingMacRoman, 0, FALSE, (UInt8 *) &darwin_locale[bytes_written], len - bytes_written, &bytes_converted) > 0) {
					darwin_locale[bytes_written + bytes_converted] = '-';
					bytes_written += bytes_converted + 1;
				}

				CFStringGetBytes (locale_country, CFRangeMake (0, CFStringGetLength (locale_country)), kCFStringEncodingMacRoman, 0, FALSE, (UInt8 *) &darwin_locale[bytes_written], len - bytes_written, &bytes_converted);
				darwin_locale[bytes_written + bytes_converted] = '\0';
			}
		}

		if (darwin_locale == NULL) {
			locale_cfstr = CFLocaleGetIdentifier (locale);

			if (locale_cfstr) {
				len = CFStringGetMaximumSizeForEncoding (CFStringGetLength (locale_cfstr), kCFStringEncodingMacRoman) + 1;
				darwin_locale = (char *) malloc (len);
				if (!CFStringGetCString (locale_cfstr, darwin_locale, len, kCFStringEncodingMacRoman)) {
					free (darwin_locale);
					CFRelease (locale);
					darwin_locale = NULL;
					return NULL;
				}

				for (i = 0; i < strlen (darwin_locale); i++)
					if (darwin_locale [i] == '_')
						darwin_locale [i] = '-';
			}			
		}

		CFRelease (locale);
	}

	return g_strdup (darwin_locale);
}
#endif

static char *
get_posix_locale (void)
{
	const char *locale;

	locale = g_getenv ("LC_ALL");
	if (locale == NULL) {
		locale = g_getenv ("LANG");
		if (locale == NULL)
			locale = setlocale (LC_ALL, NULL);
	}
	if (locale == NULL)
		return NULL;

	/* Skip English-only locale 'C' */
	if (strcmp (locale, "C") == 0)
		return NULL;

	return g_strdup (locale);
}


static gchar *
get_current_locale_name (void)
{
	char *locale;
	char *p, *ret;
		
#ifdef HOST_WIN32
	locale = g_win32_getlocale ();
#elif defined (__APPLE__)	
	locale = get_darwin_locale ();
	if (!locale)
		locale = get_posix_locale ();
#else
	locale = get_posix_locale ();
#endif

	if (locale == NULL)
		return NULL;

	p = strchr (locale, '.');
	if (p != NULL)
		*p = 0;
	p = strchr (locale, '@');
	if (p != NULL)
		*p = 0;
	p = strchr (locale, '_');
	if (p != NULL)
		*p = '-';

	ret = g_ascii_strdown (locale, -1);
	g_free (locale);

	return ret;
}

MonoString*
ves_icall_System_Globalization_CultureInfo_get_current_locale_name (void)
{
	MonoString* ret;

	MONO_HANDLE_ARENA_PUSH ();

	MONO_HANDLE_TYPE (MonoString) ret_handle;

	MonoError error;
	gchar *locale;

	locale = get_current_locale_name ();
	if (!locale) {
		ret_handle = MONO_HANDLE_NEW (MonoString, NULL);
	} else {
		ret_handle = mono_handle_string_new (mono_domain_get (), locale, &error);
		g_assert (mono_error_ok (&error));

		g_free (locale);
	}

	MONO_HANDLE_ARENA_POP_RETURN_UNSAFE (ret_handle, ret);

	return ret;
}

static const CultureInfoEntry*
culture_info_entry_from_lcid (int lcid)
{
	return mono_binary_search (&lcid, culture_entries, NUM_CULTURE_ENTRIES, sizeof (CultureInfoEntry), culture_lcid_locator);
}

MonoBoolean
ves_icall_System_Globalization_CultureInfo_construct_internal_locale_from_lcid (MonoCultureInfo *this_obj, gint lcid)
{
	MonoBoolean ret;

	MONO_HANDLE_ARENA_PUSH ();

	MONO_HANDLE_TYPE (MonoCultureInfo) this_obj_handle = MONO_HANDLE_NEW (MonoCultureInfo, this_obj);

	const CultureInfoEntry *ci;

	ci = culture_info_entry_from_lcid (lcid);
	if(ci == NULL)
		ret = FALSE;
	else
		ret = construct_culture (this_obj_handle, ci);

	MONO_HANDLE_ARENA_POP;

	return ret;
}

MonoBoolean
ves_icall_System_Globalization_CultureInfo_construct_internal_locale_from_name (MonoCultureInfo *this_obj, MonoString *name)
{
	MonoBoolean ret;

	MONO_HANDLE_ARENA_PUSH ();

	MONO_HANDLE_TYPE (MonoCultureInfo) this_obj_handle = MONO_HANDLE_NEW (MonoCultureInfo, this_obj);
	MONO_HANDLE_TYPE (MonoString) name_handle = MONO_HANDLE_NEW (MonoString, name);

	MonoError error;
	gchar *name_utf8;
	const CultureInfoNameEntry *name_entry;

	name_utf8 = mono_handle_string_to_utf8 (name_handle, &error);
	g_assert (mono_error_ok (&error));

	name_entry = mono_binary_search (name_utf8, culture_name_entries, NUM_CULTURE_ENTRIES, sizeof (CultureInfoNameEntry), culture_name_locator);

	g_free (name_utf8);

	ret = name_entry ? construct_culture (this_obj_handle, &culture_entries [name_entry->culture_entry_index]) : FALSE;

	MONO_HANDLE_ARENA_POP;

	return ret;
}

static const RegionInfoEntry*
region_info_entry_from_lcid (int lcid)
{
	const RegionInfoEntry *entry;
	const CultureInfoEntry *ne;

	ne = mono_binary_search (&lcid, culture_entries, NUM_CULTURE_ENTRIES, sizeof (CultureInfoEntry), culture_lcid_locator);

	if (ne == NULL)
		return FALSE;

	entry = &region_entries [ne->region_entry_index];

	return entry;
}

MonoBoolean
ves_icall_System_Globalization_RegionInfo_construct_internal_region_from_lcid (MonoRegionInfo *this_obj, gint lcid)
{
	MonoBoolean ret;

	MONO_HANDLE_ARENA_PUSH ();

	MONO_HANDLE_TYPE (MonoRegionInfo) this_obj_handle = MONO_HANDLE_NEW (MonoRegionInfo, this_obj);

	const RegionInfoEntry *ri;

	ri = region_info_entry_from_lcid (lcid);

	ret = ri ? construct_region (this_obj_handle, ri) : FALSE;

	MONO_HANDLE_ARENA_POP;

	return ret;
}

MonoBoolean
ves_icall_System_Globalization_RegionInfo_construct_internal_region_from_name (MonoRegionInfo *this_obj, MonoString *name)
{
	MonoBoolean ret;

	MONO_HANDLE_ARENA_PUSH ();

	MONO_HANDLE_TYPE (MonoRegionInfo) this_obj_handle = MONO_HANDLE_NEW (MonoRegionInfo, this_obj);
	MONO_HANDLE_TYPE (MonoString) name_handle = MONO_HANDLE_NEW (MonoString, name);

	MonoError error;
	gchar *name_utf8;
	const RegionInfoNameEntry *name_entry;

	name_utf8 = mono_handle_string_to_utf8 (name_handle, &error);
	g_assert (mono_error_ok (&error));

	name_entry = mono_binary_search (name_utf8, region_name_entries, NUM_REGION_ENTRIES, sizeof (RegionInfoNameEntry), region_name_locator);

	g_free (name_utf8);

	ret = name_entry ? construct_region (this_obj_handle, &region_entries [name_entry->region_entry_index]) : FALSE;

	MONO_HANDLE_ARENA_POP;

	return ret;
}

MonoArray*
ves_icall_System_Globalization_CultureInfo_internal_get_cultures (MonoBoolean neutral, MonoBoolean specific, MonoBoolean installed)
{
	MonoArray *ret;

	MONO_HANDLE_ARENA_PUSH ();

	MONO_HANDLE_TYPE (MonoArray) ret_handle;

	MonoError error;
	MonoClass *klass;
	MonoDomain *domain;
	gint i, len;
	gboolean is_neutral;
	const CultureInfoEntry *ci;

	domain = mono_domain_get ();

	len = 0;
	for (i = 0; i < NUM_CULTURE_ENTRIES; i++) {
		ci = &culture_entries [i];
		is_neutral = ci->territory == 0;
		if ((neutral && is_neutral) || (specific && !is_neutral))
			len++;
	}

	klass = mono_class_from_name (mono_get_corlib (), "System.Globalization", "CultureInfo");

	/* The InvariantCulture is not in culture_entries */
	/* We reserve the first slot in the array for it */
	if (neutral)
		len++;

	ret_handle = mono_handle_array_new (domain, klass, len, &error);
	g_assert (mono_error_ok (&error));

	if (len != 0) {
		gint last = 0;

		if (neutral)
			MONO_HANDLE_ARRAY_SETREF_NULL (ret_handle, last++);

		for (i = 0; i < NUM_CULTURE_ENTRIES; i++) {
			ci = &culture_entries [i];
			is_neutral = ci->territory == 0;
			if ((neutral && is_neutral) || (specific && !is_neutral)) {
				MONO_HANDLE_ARENA_PUSH ();

				MONO_HANDLE_TYPE (MonoCultureInfo) culture_handle;

				culture_handle = (MONO_HANDLE_TYPE (MonoCultureInfo)) mono_handle_object_new (domain, klass, &error);
				g_assert (mono_error_ok (&error));

				mono_handle_runtime_object_init ((MONO_HANDLE_TYPE (MonoObject)) culture_handle);

				construct_culture (culture_handle, ci);
				MONO_HANDLE_SET (culture_handle, use_user_override, TRUE);

				MONO_HANDLE_ARRAY_SETREF (ret_handle, last++, culture_handle);

				MONO_HANDLE_ARENA_POP;
			}
		}
	}

	MONO_HANDLE_ARENA_POP_RETURN_UNSAFE (ret_handle, ret);

	return ret;
}

static inline gint32
string_invariant_compare_char (gunichar2 c1, gunichar2 c2, gint32 options)
{
	gint32 result;

	/* Ordinal can not be mixed with other options, and must return the difference, not only -1, 0, 1 */
	if (options & CompareOptions_Ordinal) 
		return (gint32) c1 - c2;

	if (options & CompareOptions_IgnoreCase) {
		GUnicodeType c1type, c2type;

		c1type = g_unichar_type (c1);
		c2type = g_unichar_type (c2);

		result = (gint32) (c1type != G_UNICODE_LOWERCASE_LETTER ? g_unichar_tolower(c1) : c1) -
			(c2type != G_UNICODE_LOWERCASE_LETTER ? g_unichar_tolower(c2) : c2);
	} else {
		/*
		 * No options. Kana, symbol and spacing options don't
		 * apply to the invariant culture.
		 */

		/*
		 * FIXME: here we must use the information from c1type and c2type
		 * to find out the proper collation, even on the InvariantCulture, the
		 * sorting is not done by computing the unicode values, but their
		 * actual sort order.
		 */
		result = (gint32) c1 - c2;
	}

	return ((result < 0) ? -1 : (result > 0) ? 1 : 0);
}

int
ves_icall_System_Globalization_CompareInfo_internal_compare (MonoCompareInfo *this_obj, MonoString *str1, gint32 off1, gint32 len1, MonoString *str2, gint32 off2, gint32 len2, gint32 options)
{
	int ret;
	gint32 length;
	gint32 charcmp;
	gunichar2 *ustr1;
	gunichar2 *ustr2;
	gint32 pos;

	MONO_PREPARE_GC_CRITICAL_REGION;

	/* Do a normal ascii string compare, as we only know the
	 * invariant locale if we dont have ICU */

	ustr1 = mono_string_chars (str1) + off1;
	ustr2 = mono_string_chars (str2) + off2;

	length = len1 >= len2 ? len1 : len2;

	for (pos = 0; pos != length; pos++) {
		if (pos >= len1 || pos >= len2)
			break;

		charcmp = string_invariant_compare_char (ustr1 [pos], ustr2 [pos], options);
		if (charcmp != 0) {
			ret = charcmp;
			goto done;
		}
	}

	/* the lesser wins, so if we have looped until length we just
	 * need to check the last char */
	if (pos == length) {
		ret = string_invariant_compare_char (ustr1 [pos - 1], ustr2 [pos - 1], options);
		goto done;
	}

	/* Test if one of the strings has been compared to the end */
	if (pos >= len1 && pos >= len2) {
		ret = 0;
		goto done;
	} else if (pos >= len1) {
		ret = -1;
		goto done;
	} else if (pos >= len2) {
		ret = 1;
		goto done;
	}

	/* if not, check our last char only.. (can this happen?) */
	ret = string_invariant_compare_char (ustr1 [pos], ustr2 [pos], options);

done:
	MONO_FINISH_GC_CRITICAL_REGION;

	return ret;
}

void
ves_icall_System_Globalization_CompareInfo_assign_sortkey (MonoCompareInfo *this_obj, MonoSortKey *key, MonoString *source, gint32 options)
{
	MONO_HANDLE_ARENA_PUSH ();

	MONO_HANDLE_TYPE (MonoString) source_handle = MONO_HANDLE_NEW (MonoString, source);
	MONO_HANDLE_TYPE (MonoSortKey) key_handle = MONO_HANDLE_NEW (MonoSortKey, key);
	MONO_HANDLE_TYPE (MonoArray) array_handle;

	MonoError error;
	gint32 source_len, i;

	source_len = mono_handle_string_length (source_handle);

	array_handle = mono_handle_array_new (mono_domain_get (), mono_get_byte_class (), source_len, &error);
	g_assert (mono_error_ok (&error));

	MONO_PREPARE_GC_CRITICAL_REGION;

	gunichar2 *source_chars = mono_handle_string_chars (source_handle);

	for (i = 0; i < source_len; i++)
		MONO_HANDLE_ARRAY_SET (array_handle, guint8, i, source_chars [i]);

	MONO_FINISH_GC_CRITICAL_REGION;

	MONO_HANDLE_SETREF (key_handle, key, array_handle);

	MONO_HANDLE_ARENA_POP;
}

int
ves_icall_System_Globalization_CompareInfo_internal_index (MonoCompareInfo *this_obj, MonoString *source, gint32 sindex, gint32 count, MonoString *value, gint32 options, MonoBoolean first)
{
	gint ret;

	gint32 lencmpstr;
	gunichar2 *src;
	gunichar2 *cmpstr;
	gint32 pos, i;

	ret = -1;

	MONO_PREPARE_GC_CRITICAL_REGION;

	src = mono_string_chars (source);
	cmpstr = mono_string_chars (value);
	lencmpstr = mono_string_length (value);

	if (first) {
		count -= lencmpstr;
		for (pos = sindex; pos <= sindex + count; pos++) {
			for (i = 0; src [pos + i] == cmpstr [i]; ) {
				if (++i == lencmpstr) {
					ret = pos;
					break;
				}
			}
		}
	} else {
		for (pos = sindex - lencmpstr + 1; pos > sindex - count; pos--) {
			if (memcmp (src + pos, cmpstr, lencmpstr * sizeof (gunichar2)) == 0) {
				ret = pos;
				break;
			}
		}
	}

	MONO_FINISH_GC_CRITICAL_REGION;

	return ret;
}

int
ves_icall_System_Globalization_CompareInfo_internal_index_char (MonoCompareInfo *this_obj, MonoString *source, gint32 sindex, gint32 count, gunichar2 value, gint32 options, MonoBoolean first)
{
	gint ret;

	gint32 pos;
	gunichar2 *src;

	ret = -1;

	MONO_PREPARE_GC_CRITICAL_REGION;

	src = mono_string_chars (source);

	if (first) {
		for (pos = sindex; pos != count + sindex; pos++) {
			if (src [pos] == value) {
				ret = pos;
				break;
			}
		}
	} else {
		for (pos = sindex; pos > sindex - count; pos--) {
			if (src [pos] == value) {
				ret = pos;
				break;
			}
		}
	}

	MONO_FINISH_GC_CRITICAL_REGION;

	return ret;
}

int
ves_icall_System_Threading_Thread_current_lcid (void)
{
	/* Invariant */
	return 0x007F;
}

void
ves_icall_System_Text_Normalization_load_normalization_resource (guint8 **argProps, guint8 **argMappedChars, guint8 **argCharMapIndex, guint8 **argHelperIndex, guint8 **argMapIdxToComposite, guint8 **argCombiningClass)
{
#ifdef DISABLE_NORMALIZATION
	mono_set_pending_exception (mono_get_exception_not_supported ("This runtime has been compiled without string normalization support."));
#else
	*argProps = (guint8*)props;
	*argMappedChars = (guint8*) mappedChars;
	*argCharMapIndex = (guint8*) charMapIndex;
	*argHelperIndex = (guint8*) helperIndex;
	*argMapIdxToComposite = (guint8*) mapIdxToComposite;
	*argCombiningClass = (guint8*)combiningClass;
#endif
}
