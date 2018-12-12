/**
 * \file
 * Culture-sensitive handling
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
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include <config.h>
#include <glib.h>
#include <string.h>

#include <mono/metadata/class-init.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/object.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/monitor.h>
#include <mono/metadata/locales.h>
#include <mono/metadata/culture-info.h>
#include <mono/metadata/culture-info-tables.h>
#include <mono/utils/bsearch.h>

#ifndef DISABLE_NORMALIZATION
#include <mono/metadata/normalization-tables.h>
#endif

#include <locale.h>
#if defined(__APPLE__)
#include <CoreFoundation/CoreFoundation.h>
#endif
#include "icall-decl.h"

static gint32
string_invariant_compare_char (gunichar2 c1, gunichar2 c2, gint32 options);

static gint32 string_invariant_compare_char (gunichar2 c1, gunichar2 c2,
					     gint32 options);

static const CultureInfoEntry* culture_info_entry_from_lcid (int lcid);

static const CultureInfoEntry*
culture_info_entry_from_lcid (int lcid);

/* Lazy class loading functions */
static GENERATE_GET_CLASS_WITH_CACHE (culture_info, "System.Globalization", "CultureInfo")

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
	
	return strcmp (aa, idx2string (bb->name));
}

static MonoArrayHandle
create_group_sizes_array (const gint *gs, gint ml, MonoError *error)
{
	HANDLE_FUNCTION_ENTER ();

	MonoArrayHandle ret = NULL_HANDLE_ARRAY;
	int i = 0;
	int len = 0;

	for (i = 0; i < ml; i++) {
		if (gs [i] == -1)
			break;
		len++;
	}
	
	ret = mono_array_new_cached_handle (mono_domain_get (),
				     mono_get_int32_class (), len, error);
	goto_if_nok (error, return_null);

	for(i = 0; i < len; i++)
		MONO_HANDLE_ARRAY_SETVAL (ret, gint32, i, gs [i]);

	goto exit;
return_null:
	ret = NULL_HANDLE_ARRAY;
exit:
	HANDLE_FUNCTION_RETURN_REF (MonoArray, ret);
}

static MonoArrayHandle
create_names_array_idx (const guint16 *names, int ml, MonoError *error)
{
	HANDLE_FUNCTION_ENTER ();

	MonoArrayHandle ret = NULL_HANDLE_ARRAY;
	MonoDomain *domain = mono_domain_get ();
	MonoStringHandle s = MONO_HANDLE_NEW (MonoString, NULL);
	int i = 0;

	if (names == NULL)
		goto return_null;

	ret = mono_array_new_cached_handle (domain, mono_get_string_class (), ml, error);
	goto_if_nok (error, return_null);

	for (i = 0; i < ml; ++i) {
		mono_string_new_utf8z_assign (s, domain, dtidx2string (names [i]), error);
		goto_if_nok (error, return_null);
		MONO_HANDLE_ARRAY_SETREF (ret, i, s);
	}

	goto exit;
return_null:
	ret = NULL_HANDLE_ARRAY;
exit:
	HANDLE_FUNCTION_RETURN_REF (MonoArray, ret);
}

static MonoArrayHandle
create_names_array_idx_dynamic (const guint16 *names, int ml, MonoError *error)
{
	HANDLE_FUNCTION_ENTER ();

	MonoStringHandle s = MONO_HANDLE_NEW (MonoString, NULL);
	MonoArrayHandle ret = NULL_HANDLE_ARRAY;
	MonoDomain *domain = mono_domain_get ();
	int i = 0;
	int len = 0;

	if (names == NULL)
		goto return_null;

	for (i = 0; i < ml; i++) {
		if (names [i] == 0)
			break;
		len++;
	}

	ret = mono_array_new_cached_handle (domain, mono_get_string_class (), len, error);
	goto_if_nok (error, return_null);

	for(i = 0; i < len; i++) {
		mono_string_new_utf8z_assign (s, domain, pattern2string (names [i]), error);
		goto_if_nok (error, return_null);
		MONO_HANDLE_ARRAY_SETREF (ret, i, s);
	}
	goto exit;
return_null:
	ret = NULL_HANDLE_ARRAY;
exit:
	HANDLE_FUNCTION_RETURN_REF (MonoArray, ret);
}

MonoBoolean
ves_icall_System_Globalization_CalendarData_fill_calendar_data (MonoCalendarDataHandle this_obj,
	const gunichar2 *name, int name_length, gint32 calendar_index, MonoError *error)
{
	const DateTimeFormatEntry *dfe;
	const CultureInfoNameEntry *ne;
	const CultureInfoEntry *ci;
	MonoDomain *domain = mono_domain_get ();

	char *n = mono_utf16_to_utf8 (name, name_length, error);
	return_val_if_nok (error, FALSE);
	ne = (const CultureInfoNameEntry *)mono_binary_search (n, culture_name_entries, NUM_CULTURE_ENTRIES,
			sizeof (CultureInfoNameEntry), culture_name_locator);
	g_free (n);
	if (ne == NULL)
		return FALSE;

	ci = &culture_entries [ne->culture_entry_index];
	dfe = &datetime_format_entries [ci->datetime_format_index];

	MonoStringHandle native_name = mono_string_new_handle (domain, idx2string (ci->nativename), error);
	return_val_if_nok (error, FALSE);
	MONO_HANDLE_SET (this_obj, NativeName, native_name);
	MonoArrayHandle short_date_patterns = create_names_array_idx_dynamic (dfe->short_date_patterns,
									 NUM_SHORT_DATE_PATTERNS, error);
	return_val_if_nok (error, FALSE);
	MONO_HANDLE_SET (this_obj, ShortDatePatterns, short_date_patterns);
	MonoArrayHandle year_month_patterns =create_names_array_idx_dynamic (dfe->year_month_patterns,
									NUM_YEAR_MONTH_PATTERNS, error);
	return_val_if_nok (error, FALSE);
	MONO_HANDLE_SET (this_obj, YearMonthPatterns, year_month_patterns);

	MonoArrayHandle long_date_patterns = create_names_array_idx_dynamic (dfe->long_date_patterns,
									NUM_LONG_DATE_PATTERNS, error);
	return_val_if_nok (error, FALSE);
	MONO_HANDLE_SET (this_obj, LongDatePatterns, long_date_patterns);

	MonoStringHandle month_day_pattern = mono_string_new_handle (domain, pattern2string (dfe->month_day_pattern), error);
	return_val_if_nok (error, FALSE);
	MONO_HANDLE_SET (this_obj, MonthDayPattern, month_day_pattern);

	MonoArrayHandle day_names = create_names_array_idx (dfe->day_names, NUM_DAYS, error);
	return_val_if_nok (error, FALSE);
	MONO_HANDLE_SET (this_obj, DayNames, day_names);

	MonoArrayHandle abbr_day_names = create_names_array_idx (dfe->abbreviated_day_names,
							    NUM_DAYS, error);
	return_val_if_nok (error, FALSE);
	MONO_HANDLE_SET (this_obj, AbbreviatedDayNames, abbr_day_names);

	MonoArrayHandle ss_day_names = create_names_array_idx (dfe->shortest_day_names, NUM_DAYS, error);
	return_val_if_nok (error, FALSE);
	MONO_HANDLE_SET (this_obj, SuperShortDayNames, ss_day_names);

	MonoArrayHandle month_names = create_names_array_idx (dfe->month_names, NUM_MONTHS, error);
	return_val_if_nok (error, FALSE);
	MONO_HANDLE_SET (this_obj, MonthNames, month_names);

	MonoArrayHandle abbr_mon_names = create_names_array_idx (dfe->abbreviated_month_names,
							    NUM_MONTHS, error);
	return_val_if_nok (error, FALSE);
	MONO_HANDLE_SET (this_obj, AbbreviatedMonthNames, abbr_mon_names);
	
	MonoArrayHandle gen_month_names = create_names_array_idx (dfe->month_genitive_names, NUM_MONTHS, error);
	return_val_if_nok (error, FALSE);
	MONO_HANDLE_SET (this_obj, GenitiveMonthNames, gen_month_names);

	MonoArrayHandle gen_abbr_mon_names = create_names_array_idx (dfe->abbreviated_month_genitive_names, NUM_MONTHS, error);
	return_val_if_nok (error, FALSE);
	MONO_HANDLE_SET (this_obj, GenitiveAbbreviatedMonthNames, gen_abbr_mon_names);

	return TRUE;
}

void
ves_icall_System_Globalization_CultureData_fill_culture_data (MonoCultureDataHandle this_obj,
	gint32 datetime_index, MonoError *error)
{
	g_assert (datetime_index >= 0);
	MonoDomain *domain = mono_domain_get ();

	const DateTimeFormatEntry *dfe = &datetime_format_entries [datetime_index];

	MonoStringHandle tmp_str = MONO_HANDLE_NEW (MonoString, NULL);

#define SET_STR(field, field2) do {						\
		mono_string_new_utf8z_assign (tmp_str, domain, idx2string (dfe->field2), error); \
		return_if_nok (error); 						\
		MONO_HANDLE_SET (this_obj, field, tmp_str);			\
	} while (0)

	SET_STR (AMDesignator, am_designator);
	SET_STR (PMDesignator, pm_designator);
	SET_STR (TimeSeparator, time_separator);
#undef SET_STR

	MonoArrayHandle long_time_patterns = create_names_array_idx_dynamic (dfe->long_time_patterns,
									NUM_LONG_TIME_PATTERNS, error);
	return_if_nok (error);
	MONO_HANDLE_SET (this_obj, LongTimePatterns, long_time_patterns);

	MonoArrayHandle short_time_patterns = create_names_array_idx_dynamic (dfe->short_time_patterns,
									 NUM_SHORT_TIME_PATTERNS, error);
	return_if_nok (error);
	MONO_HANDLE_SET (this_obj, ShortTimePatterns, short_time_patterns);

	MONO_HANDLE_SETVAL (this_obj, FirstDayOfWeek, guint32, dfe->first_day_of_week);
	MONO_HANDLE_SETVAL (this_obj, CalendarWeekRule, guint32, dfe->calendar_week_rule);
}

void
ves_icall_System_Globalization_CultureData_fill_number_data (MonoNumberFormatInfoHandle number,
	gint32 number_index, MonoError *error)
{
	g_assert (number_index >= 0);
	MonoDomain *domain = mono_domain_get ();

	const NumberFormatEntry *nfe = &number_format_entries [number_index];

	MONO_HANDLE_SETVAL (number, currencyDecimalDigits, gint32, nfe->currency_decimal_digits);

	MonoStringHandle tmp_str = MONO_HANDLE_NEW (MonoString, NULL);

#define SET_STR(field, field2) do {						\
		mono_string_new_utf8z_assign (tmp_str, domain, idx2string (nfe->field2), error); \
		return_if_nok (error); 						\
		MONO_HANDLE_SET (number, field, tmp_str);			\
	} while (0)

	SET_STR (currencyDecimalSeparator, currency_decimal_separator);
	SET_STR (currencyGroupSeparator, currency_group_separator);

	MonoArrayHandle currency_sizes_arr = create_group_sizes_array (nfe->currency_group_sizes,
								  GROUP_SIZE, error);
	return_if_nok (error);
	MONO_HANDLE_SET (number, currencyGroupSizes, currency_sizes_arr);
	MONO_HANDLE_SETVAL (number, currencyNegativePattern, gint32, nfe->currency_negative_pattern);
	MONO_HANDLE_SETVAL (number, currencyPositivePattern, gint32, nfe->currency_positive_pattern);

	SET_STR (currencySymbol, currency_symbol);
	SET_STR (naNSymbol, nan_symbol);
	SET_STR (negativeInfinitySymbol, negative_infinity_symbol);
	SET_STR (negativeSign, negative_sign);
	MONO_HANDLE_SETVAL (number, numberDecimalDigits, guint32, nfe->number_decimal_digits);
	SET_STR (numberDecimalSeparator, number_decimal_separator);
	SET_STR (numberGroupSeparator, number_group_separator);
	MonoArrayHandle number_sizes_arr = create_group_sizes_array (nfe->number_group_sizes,
								GROUP_SIZE, error);
	return_if_nok (error);
	MONO_HANDLE_SET (number, numberGroupSizes, number_sizes_arr);
	MONO_HANDLE_SETVAL (number, numberNegativePattern, gint32, nfe->number_negative_pattern);
	MONO_HANDLE_SETVAL (number, percentNegativePattern, gint32, nfe->percent_negative_pattern);
	MONO_HANDLE_SETVAL (number, percentPositivePattern, gint32, nfe->percent_positive_pattern);
	SET_STR (percentSymbol, percent_symbol);
	SET_STR (perMilleSymbol, per_mille_symbol);
	SET_STR (positiveInfinitySymbol, positive_infinity_symbol);
	SET_STR (positiveSign, positive_sign);
#undef SET_STR
}

static MonoBoolean
construct_culture (MonoCultureInfoHandle this_obj, const CultureInfoEntry *ci, MonoError *error)
{
	MonoDomain *domain = mono_domain_get ();
	error_init (error);

	MONO_HANDLE_SETVAL (this_obj, lcid, gint32, ci->lcid);

	MonoStringHandle tmp_str = MONO_HANDLE_NEW (MonoString, NULL);

#define SET_STR(field) do {						\
		mono_string_new_utf8z_assign (tmp_str, domain, idx2string (ci->field), error); \
		return_val_if_nok (error, FALSE);			\
		MONO_HANDLE_SET (this_obj, field, tmp_str);		\
	} while (0)

	SET_STR (name);
	SET_STR (englishname);
	SET_STR (nativename);
	SET_STR (win3lang);
	SET_STR (iso3lang);
	SET_STR (iso2lang);

	// It's null for neutral cultures
	if (ci->territory > 0)
		SET_STR (territory);

	MonoArrayHandle native_calendar_names = create_names_array_idx (ci->native_calendar_names, NUM_CALENDARS, error);
	return_val_if_nok (error, FALSE);
	MONO_HANDLE_SET (this_obj, native_calendar_names, native_calendar_names);
	MONO_HANDLE_SETVAL (this_obj, parent_lcid, gint32, ci->parent_lcid);
	MONO_HANDLE_SETVAL (this_obj, datetime_index, gint32, ci->datetime_format_index);
	MONO_HANDLE_SETVAL (this_obj, number_index, gint32, ci->number_format_index);
	MONO_HANDLE_SETVAL (this_obj, calendar_type, gint32, ci->calendar_type);
	MONO_HANDLE_SETVAL (this_obj, text_info_data, gconstpointer, &ci->text_info);
#undef SET_STR
	
	return TRUE;
}

static MonoBoolean
construct_region (MonoRegionInfoHandle this_obj, const RegionInfoEntry *ri, MonoError *error)
{
	error_init (error);
	MonoDomain *domain = mono_domain_get ();
	MonoStringHandle tmp_str = MONO_HANDLE_NEW (MonoString, NULL);

#define SET_STR(field) do {						\
		tmp_str = mono_string_new_utf8z_assign (tmp_str, domain, idx2string (ri->field), error); \
		return_val_if_nok (error, FALSE);			\
		MONO_HANDLE_SET (this_obj, field, tmp_str);		\
	} while (0)

	MONO_HANDLE_SETVAL (this_obj, geo_id, gint32, ri->geo_id);

	SET_STR (iso2name);
	SET_STR (iso3name);
	SET_STR (win3name);
	SET_STR (english_name);
	SET_STR (native_name);
	SET_STR (currency_symbol);
	SET_STR (iso_currency_symbol);
	SET_STR (currency_english_name);
	SET_STR (currency_native_name);
	
#undef SET_STR

	return TRUE;
}

static const CultureInfoEntry*
culture_info_entry_from_lcid (int lcid)
{
	const CultureInfoEntry *ci;

	ci = (const CultureInfoEntry *)mono_binary_search (&lcid, culture_entries, NUM_CULTURE_ENTRIES, sizeof (CultureInfoEntry), culture_lcid_locator);

	return ci;
}

#if defined (__APPLE__)
static gchar*
get_darwin_locale (void)
{
	static gchar *cached_locale = NULL;
	gchar *darwin_locale = NULL;
	CFLocaleRef locale = NULL;
	CFStringRef locale_language = NULL;
	CFStringRef locale_country = NULL;
	CFStringRef locale_script = NULL;
	CFStringRef locale_cfstr = NULL;
	CFIndex bytes_converted;
	CFIndex bytes_written;
	CFIndex len;
	int i;

	if (cached_locale != NULL)
		return g_strdup (cached_locale);

	locale = CFLocaleCopyCurrent ();

	if (locale) {
		locale_language = (CFStringRef)CFLocaleGetValue (locale, kCFLocaleLanguageCode);
		if (locale_language != NULL && CFStringGetBytes(locale_language, CFRangeMake (0, CFStringGetLength (locale_language)), kCFStringEncodingMacRoman, 0, FALSE, NULL, 0, &bytes_converted) > 0) {
			len = bytes_converted + 1;

			locale_country = (CFStringRef)CFLocaleGetValue (locale, kCFLocaleCountryCode);
			if (locale_country != NULL && CFStringGetBytes (locale_country, CFRangeMake (0, CFStringGetLength (locale_country)), kCFStringEncodingMacRoman, 0, FALSE, NULL, 0, &bytes_converted) > 0) {
				len += bytes_converted + 1;

				locale_script = (CFStringRef)CFLocaleGetValue (locale, kCFLocaleScriptCode);
				if (locale_script != NULL && CFStringGetBytes (locale_script, CFRangeMake (0, CFStringGetLength (locale_script)), kCFStringEncodingMacRoman, 0, FALSE, NULL, 0, &bytes_converted) > 0) {
					len += bytes_converted + 1;
				}

				darwin_locale = (char *) g_malloc (len + 1);
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
				darwin_locale = (char *) g_malloc (len);
				if (!CFStringGetCString (locale_cfstr, darwin_locale, len, kCFStringEncodingMacRoman)) {
					g_free (darwin_locale);
					CFRelease (locale);
					cached_locale = NULL;
					return NULL;
				}

				for (i = 0; i < strlen (darwin_locale); i++)
					if (darwin_locale [i] == '_')
						darwin_locale [i] = '-';
			}			
		}

		CFRelease (locale);
	}

	mono_memory_barrier ();
	cached_locale = darwin_locale;
	return g_strdup (cached_locale);
}
#endif

static char *
get_posix_locale (void)
{
	char *locale;

	locale = g_getenv ("LC_ALL");
	if (locale == NULL) {
		locale = g_getenv ("LANG");
		if (locale == NULL) {
			char *static_locale = setlocale (LC_ALL, NULL);
			if (static_locale)
				locale = g_strdup (static_locale);
		}
	}
	if (locale == NULL)
		return NULL;

	/* Skip English-only locale 'C' */
	if (strcmp (locale, "C") == 0) {
		g_free (locale);
		return NULL;
	}

	return locale;
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

MonoStringHandle
ves_icall_System_Globalization_CultureInfo_get_current_locale_name (MonoError *error)
{
	char *locale = get_current_locale_name ();
	if (locale == NULL)
		return MONO_HANDLE_CAST (MonoString, NULL_HANDLE);

	MonoStringHandle ret = mono_string_new_handle (mono_domain_get (), locale, error);
	g_free (locale);

	return ret;
}

MonoBoolean
ves_icall_System_Globalization_CultureInfo_construct_internal_locale_from_lcid (MonoCultureInfoHandle this_obj,
		gint lcid, MonoError *error)
{
	const CultureInfoEntry *ci = culture_info_entry_from_lcid (lcid);
	return ci && construct_culture (this_obj, ci, error);
}

MonoBoolean
ves_icall_System_Globalization_CultureInfo_construct_internal_locale_from_name (MonoCultureInfoHandle this_obj,
		const gunichar2 *name, int name_length, MonoError *error)
{
	char *n = mono_utf16_to_utf8 (name, name_length, error);
	return_val_if_nok (error, FALSE);

	const CultureInfoNameEntry
	*ne = (const CultureInfoNameEntry *)mono_binary_search (n, culture_name_entries, NUM_CULTURE_ENTRIES,
			sizeof (CultureInfoNameEntry), culture_name_locator);
	g_free (n);
	return ne && construct_culture (this_obj, &culture_entries [ne->culture_entry_index], error);
}
/*
MonoBoolean
ves_icall_System_Globalization_CultureInfo_construct_internal_locale_from_specific_name (MonoCultureInfoHandle ci,
		const gunichar2 *name, int name_length, MonoError *error)
{
	char *locale = mono_utf16_to_utf8 (name, name_length, error);
	return_val_if_nok (error, FALSE);
	gboolean ret = construct_culture_from_specific_name (ci, locale);
	g_free (locale);

	return ret;
}
*/

MonoBoolean
ves_icall_System_Globalization_RegionInfo_construct_internal_region_from_name (MonoRegionInfoHandle this_obj,
		const gunichar2 *name, int name_length, MonoError *error)
{
	char *n = mono_utf16_to_utf8 (name, name_length, error);
	return_val_if_nok (error, FALSE);
	
	const RegionInfoNameEntry
	*ne = (const RegionInfoNameEntry *)mono_binary_search (n, region_name_entries, NUM_REGION_ENTRIES,
		sizeof (RegionInfoNameEntry), region_name_locator);

	g_free (n);
	return ne && construct_region (this_obj, &region_entries [ne->region_entry_index], error);
}

MonoArrayHandle
ves_icall_System_Globalization_CultureInfo_internal_get_cultures (MonoBoolean neutral,
		MonoBoolean specific, MonoBoolean installed, MonoError *error)
{
	MonoArrayHandle ret = NULL_HANDLE_ARRAY;
	MonoClass *klass;
	MonoCultureInfoHandle culture;
	MonoDomain *domain = mono_domain_get ();
	const CultureInfoEntry *ci;
	gint i;
	gboolean is_neutral;

	gint len = 0;
	for (i = 0; i < NUM_CULTURE_ENTRIES; i++) {
		ci = &culture_entries [i];
		is_neutral = ci->territory == 0;
		if ((neutral && is_neutral) || (specific && !is_neutral))
			len++;
	}

	klass = mono_class_get_culture_info_class ();

	/* The InvariantCulture is not in culture_entries */
	/* We reserve the first slot in the array for it */
	if (neutral)
		len++;

	ret = mono_array_new_handle (domain, klass, len, error);
	goto_if_nok (error, fail);

	if (len == 0)
		return ret;

	len = 0;
	if (neutral)
		MONO_HANDLE_ARRAY_SETREF (ret, len++, NULL_HANDLE);

	culture = MONO_HANDLE_NEW (MonoCultureInfo, NULL);

	for (i = 0; i < NUM_CULTURE_ENTRIES; i++) {
		ci = &culture_entries [i];
		is_neutral = ci->territory == 0;
		if ((neutral && is_neutral) || (specific && !is_neutral)) {
			mono_object_new_assign (MONO_HANDLE_CAST (MonoObject, culture), domain, klass, error);
			goto_if_nok (error, fail);
			mono_runtime_object_init_handle (MONO_HANDLE_CAST (MonoObject, culture), error);
			goto_if_nok (error, fail);
			if (!construct_culture (culture, ci, error))
				goto fail;
			MONO_HANDLE_SETVAL (culture, use_user_override, MonoBoolean, TRUE);
			MONO_HANDLE_ARRAY_SETREF (ret, len++, culture);
		}
	}

fail:
	return ret;
}

static gint32
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

gint32
ves_icall_System_Globalization_CompareInfo_internal_compare (const gunichar2 *ustr1, gint32 len1,
	const gunichar2 *ustr2, gint32 len2, gint32 options)
{
	/* Do a normal ascii string compare, as we only know the
	 * invariant locale if we dont have ICU
	 */

	/* c translation of C# code from old string.cs.. :) */
	const gint32 length = MAX (len1, len2);
	gint32 pos = 0;

	for (pos = 0; pos != length; pos++) {
		if (pos >= len1 || pos >= len2)
			break;
		const int charcmp = string_invariant_compare_char (ustr1 [pos], ustr2 [pos], options);
		if (charcmp)
			return charcmp;
	}

	/* the lesser wins, so if we have looped until length we just
	 * need to check the last char
	 */
	if (pos && pos == length)
		return string_invariant_compare_char(ustr1 [pos - 1], ustr2 [pos - 1], options);

	/* Test if one of the strings has been compared to the end */
	if (pos >= len1) {
		if (pos >= len2) {
			return 0;
		} else {
			return -1;
		}
	} else if (pos >= len2) {
		return 1;
	}

	/* if not, check our last char only.. (can this happen?) */
	return string_invariant_compare_char (ustr1 [pos], ustr2 [pos], options);
}

int
ves_icall_System_Globalization_CompareInfo_internal_index (const gunichar2 *src, gint32 sindex,
	gint32 count, const gunichar2 *cmpstr, int lencmpstr, MonoBoolean first)
{
	gint32 pos,i;
	
	if(first) {
		count -= lencmpstr;
		for(pos=sindex;pos <= sindex+count;pos++) {
			for(i=0;src[pos+i]==cmpstr[i];) {
				if(++i==lencmpstr) {
					return(pos);
				}
			}
		}
		
		return(-1);
	} else {
		for(pos=sindex-lencmpstr+1;pos>sindex-count;pos--) {
			if(memcmp (src+pos, cmpstr,
				   lencmpstr*sizeof(gunichar2))==0) {
				return(pos);
			}
		}
		
		return(-1);
	}
}

void ves_icall_System_Text_Normalization_load_normalization_resource (guint8 **argProps,
								      guint8 **argMappedChars,
								      guint8 **argCharMapIndex,
								      guint8 **argHelperIndex,
								      guint8 **argMapIdxToComposite,
								      guint8 **argCombiningClass,
								      MonoError *error)
{
#ifdef DISABLE_NORMALIZATION
	mono_error_set_not_supported (error, "This runtime has been compiled without string normalization support.");
#else
	*argProps = (guint8*)props;
	*argMappedChars = (guint8*) mappedChars;
	*argCharMapIndex = (guint8*) charMapIndex;
	*argHelperIndex = (guint8*) helperIndex;
	*argMapIdxToComposite = (guint8*) mapIdxToComposite;
	*argCombiningClass = (guint8*)combiningClass;
#endif
}
