/**
 * \file
 */

#ifndef _MONO_METADATA_CULTURE_INFO_H_
#define _MONO_METADATA_CULTURE_INFO_H_ 1

#include <glib.h>
#include <mono/metadata/object.h>

#define NUM_DAYS 7
#define NUM_MONTHS 13
#define GROUP_SIZE 2
#define NUM_CALENDARS 4

#define NUM_SHORT_DATE_PATTERNS 14
#define NUM_LONG_DATE_PATTERNS 10
#define NUM_SHORT_TIME_PATTERNS 12
#define NUM_LONG_TIME_PATTERNS 9
#define NUM_YEAR_MONTH_PATTERNS 8

#define idx2string(idx) (locale_strings + (idx))
#define pattern2string(idx) (patterns + (idx))
#define dtidx2string(idx) (datetime_strings + (idx))

/* need to change this if the string data ends up to not fit in a 64KB array. */
typedef guint16 stridx_t;

typedef struct {
	const stridx_t month_day_pattern;
	const stridx_t am_designator;
	const stridx_t pm_designator;

	const stridx_t day_names [NUM_DAYS]; 
	const stridx_t abbreviated_day_names [NUM_DAYS];
	const stridx_t shortest_day_names [NUM_DAYS];
	const stridx_t month_names [NUM_MONTHS];
	const stridx_t month_genitive_names [NUM_MONTHS];
	const stridx_t abbreviated_month_names [NUM_MONTHS];
	const stridx_t abbreviated_month_genitive_names [NUM_MONTHS];

	const gint8 calendar_week_rule;
	const gint8 first_day_of_week;

	const stridx_t date_separator;
	const stridx_t time_separator;	

	const stridx_t short_date_patterns [NUM_SHORT_DATE_PATTERNS];
	const stridx_t long_date_patterns [NUM_LONG_DATE_PATTERNS];
	const stridx_t short_time_patterns [NUM_SHORT_TIME_PATTERNS];
	const stridx_t long_time_patterns [NUM_LONG_TIME_PATTERNS];
	const stridx_t year_month_patterns [NUM_YEAR_MONTH_PATTERNS];
} DateTimeFormatEntry;

typedef struct {
	// 12x ushort -- 6 ints
	stridx_t currency_decimal_separator;
	stridx_t currency_group_separator;
	stridx_t number_decimal_separator;
	stridx_t number_group_separator;

	stridx_t currency_symbol;
	stridx_t percent_symbol;
	stridx_t nan_symbol;
	stridx_t per_mille_symbol;
	stridx_t negative_infinity_symbol;
	stridx_t positive_infinity_symbol;

	stridx_t negative_sign;
	stridx_t positive_sign;

	// 7x gint8 -- FIXME expand to 8, or sort by size.
	// For this reason, copy the data to a simpler "managed" form.
	gint8 currency_negative_pattern;
	gint8 currency_positive_pattern;
	gint8 percent_negative_pattern;
	gint8 percent_positive_pattern;
	gint8 number_negative_pattern;

	gint8 currency_decimal_digits;
	gint8 number_decimal_digits;

	gint currency_group_sizes [2];
	gint number_group_sizes [2];
} NumberFormatEntry;

// Due to the questionable layout of NumberFormatEntry, in particular
// 7x byte, make something more guaranteed to match between native and managed.
struct NumberFormatEntryManaged {
	int currency_decimal_separator;
	int currency_group_separator;
	int number_decimal_separator;
	int number_group_separator;
	int currency_symbol;
	int percent_symbol;
	int nan_symbol;
	int per_mille_symbol;
	int negative_infinity_symbol;
	int positive_infinity_symbol;
	int negative_sign;
	int positive_sign;
	int currency_negative_pattern;
	int currency_positive_pattern;
	int percent_negative_pattern;
	int percent_positive_pattern;
	int number_negative_pattern;
	int currency_decimal_digits;
	int number_decimal_digits;
	int currency_group_sizes0;
	int currency_group_sizes1;
	int number_group_sizes0;
	int number_group_sizes1;
};

typedef struct {
	const gint ansi;
	const gint ebcdic;
	const gint mac;
	const gint oem;
	const MonoBoolean is_right_to_left;
	const char list_sep;
} TextInfoEntry;

typedef struct {
	const gint16 lcid;
	const gint16 parent_lcid;
	const gint16 calendar_type;
	const gint16 region_entry_index;
	const stridx_t name;
	const stridx_t englishname;
	const stridx_t nativename;
	const stridx_t win3lang;
	const stridx_t iso3lang;
	const stridx_t iso2lang;
	const stridx_t territory;
	const stridx_t native_calendar_names [NUM_CALENDARS];

	const gint16 datetime_format_index;
	const gint16 number_format_index;
	
	const TextInfoEntry text_info;
} CultureInfoEntry;

typedef struct {
	const stridx_t name;
	const gint16 culture_entry_index;
} CultureInfoNameEntry;

typedef struct {
	const gint16 geo_id;
	const stridx_t iso2name;
	const stridx_t iso3name;
	const stridx_t win3name;
	const stridx_t english_name;
	const stridx_t native_name;
	const stridx_t currency_symbol;
	const stridx_t iso_currency_symbol;
	const stridx_t currency_english_name;
	const stridx_t currency_native_name;
} RegionInfoEntry;

typedef struct {
	const stridx_t name;
	const gint16 region_entry_index;
} RegionInfoNameEntry;

#endif

