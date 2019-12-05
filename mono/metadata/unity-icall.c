/**
 * \file
 * Unity icall support for Mono.
 *
 * Copyright 2016 Unity
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
*/
#include <config.h>
#include <glib.h>
#include "mono/utils/mono-compiler.h"
#include <mono/metadata/class-internals.h>
#include <mono/metadata/domain-internals.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/metadata-internals.h>
#include <mono/metadata/object.h>
#include <mono/metadata/object-internals.h>

#ifdef HAVE_SYS_TIME_H
#include <sys/time.h>
#endif

/*
 * Magic number to convert a time which is relative to
 * Jan 1, 1970 into a value which is relative to Jan 1, 0001.
 */
#define	EPOCH_ADJUST	((guint64)62135596800LL)

/*
 * Magic number to convert FILETIME base Jan 1, 1601 to DateTime - base Jan, 1, 0001
 */
#define FILETIME_ADJUST ((guint64)504911232000000000LL)

#ifdef PLATFORM_WIN32
/* convert a SYSTEMTIME which is of the form "last thursday in october" to a real date */
static void
convert_to_absolute_date(SYSTEMTIME *date)
{
#define IS_LEAP(y) ((y % 4) == 0 && ((y % 100) != 0 || (y % 400) == 0))
	static int days_in_month[] = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
	static int leap_days_in_month[] = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
	/* from the calendar FAQ */
	int a = (14 - date->wMonth) / 12;
	int y = date->wYear - a;
	int m = date->wMonth + 12 * a - 2;
	int d = (1 + y + y/4 - y/100 + y/400 + (31*m)/12) % 7;

	/* d is now the day of the week for the first of the month (0 == Sunday) */

	int day_of_week = date->wDayOfWeek;

	/* set day_in_month to the first day in the month which falls on day_of_week */    
	int day_in_month = 1 + (day_of_week - d);
	if (day_in_month <= 0)
		day_in_month += 7;

	/* wDay is 1 for first weekday in month, 2 for 2nd ... 5 means last - so work that out allowing for days in the month */
	date->wDay = day_in_month + (date->wDay - 1) * 7;
	if (date->wDay > (IS_LEAP(date->wYear) ? leap_days_in_month[date->wMonth - 1] : days_in_month[date->wMonth - 1]))
		date->wDay -= 7;
}
#endif

#ifndef PLATFORM_WIN32
/*
 * Return's the offset from GMT of a local time.
 * 
 *  tm is a local time
 *  t  is the same local time as seconds.
 */
static int 
gmt_offset(struct tm *tm, time_t t)
{
#if defined (HAVE_TM_GMTOFF)
	return tm->tm_gmtoff;
#else
	struct tm g;
	time_t t2;
	g = *gmtime(&t);
	g.tm_isdst = tm->tm_isdst;
	t2 = mktime(&g);
	return (int)difftime(t, t2);
#endif
}
#endif
/*
 * This is heavily based on zdump.c from glibc 2.2.
 *
 *  * data[0]:  start of daylight saving time (in DateTime ticks).
 *  * data[1]:  end of daylight saving time (in DateTime ticks).
 *  * data[2]:  utcoffset (in TimeSpan ticks).
 *  * data[3]:  additional offset when daylight saving (in TimeSpan ticks).
 *  * name[0]:  name of this timezone when not daylight saving.
 *  * name[1]:  name of this timezone when daylight saving.
 *
 *  FIXME: This only works with "standard" Unix dates (years between 1900 and 2100) while
 *         the class library allows years between 1 and 9999.
 *
 *  Returns true on success and zero on failure.
 */
guint32
ves_icall_System_CurrentSystemTimeZone_GetTimeZoneData (guint32 year, MonoArray **data, MonoArray **names, MonoBoolean *daylight_inverted)
{
	MonoError error;
#ifndef PLATFORM_WIN32
	MonoDomain *domain = mono_domain_get ();
	struct tm start, tt;
	time_t t;

	long int gmtoff_start;
	long int gmtoff;
	int is_transitioned = 0, day;
	char tzone [64];

	MONO_CHECK_ARG_NULL (data, FALSE);
	MONO_CHECK_ARG_NULL (names, FALSE);

	mono_gc_wbarrier_generic_store (data, (MonoObject*) mono_array_new_checked (domain, mono_defaults.int64_class, 4, &error));
	mono_gc_wbarrier_generic_store (names, (MonoObject*) mono_array_new_checked (domain, mono_defaults.string_class, 2, &error));

	/* 
	 * no info is better than crashing: we'll need our own tz data
	 * to make this work properly, anyway. The range is probably
	 * reduced to 1970 .. 2037 because that is what mktime is
	 * guaranteed to support (we get into an infinite loop
	 * otherwise).
	 */

	memset (&start, 0, sizeof (start));

	start.tm_mday = 1;
	start.tm_year = year-1900;

	t = mktime (&start);

	if ((year < 1970) || (year > 2037) || (t == -1)) {
		t = time (NULL);
		tt = *localtime (&t);
		strftime (tzone, sizeof (tzone), "%Z", &tt);
		mono_array_setref ((*names), 0, mono_string_new_checked (domain, tzone, &error));
		mono_array_setref ((*names), 1, mono_string_new_checked (domain, tzone, &error));
		*daylight_inverted = 0;
		return 1;
	}

	*daylight_inverted = start.tm_isdst;

	gmtoff = gmt_offset (&start, t);
	gmtoff_start = gmtoff;

	/* For each day of the year, calculate the tm_gmtoff. */
	for (day = 0; day < 365; day++) {

		t += 3600*24;
		tt = *localtime (&t);

		/* Daylight saving starts or ends here. */
		if (gmt_offset (&tt, t) != gmtoff) {
			struct tm tt1;
			time_t t1;

			/* Try to find the exact hour when daylight saving starts/ends. */
			t1 = t;
			do {
				t1 -= 3600;
				tt1 = *localtime (&t1);
			} while (gmt_offset (&tt1, t1) != gmtoff);

			/* Try to find the exact minute when daylight saving starts/ends. */
			do {
				t1 += 60;
				tt1 = *localtime (&t1);
			} while (gmt_offset (&tt1, t1) == gmtoff);
			t1+=gmtoff;
			strftime (tzone, sizeof (tzone), "%Z", &tt);
			
			/* Write data, if we're already in daylight saving, we're done. */
			if (is_transitioned) {
				if (!start.tm_isdst)
					mono_array_setref ((*names), 0, mono_string_new_checked (domain, tzone, &error));
				else
					mono_array_setref ((*names), 1, mono_string_new_checked (domain, tzone, &error));

				mono_array_set ((*data), gint64, 1, ((gint64)t1 + EPOCH_ADJUST) * 10000000L);
				return 1;
			} else {
				if (!start.tm_isdst)
					mono_array_setref ((*names), 1, mono_string_new_checked (domain, tzone, &error));
				else
					mono_array_setref ((*names), 0, mono_string_new_checked (domain, tzone, &error));

				mono_array_set ((*data), gint64, 0, ((gint64)t1 + EPOCH_ADJUST) * 10000000L);
				is_transitioned = 1;
			}

			/* This is only set once when we enter daylight saving. */
			if (*daylight_inverted == 0) {
				mono_array_set ((*data), gint64, 2, (gint64)gmtoff * 10000000L);
				mono_array_set ((*data), gint64, 3, (gint64)(gmt_offset (&tt, t) - gmtoff) * 10000000L);
			} else {
				mono_array_set ((*data), gint64, 2, (gint64)(gmtoff_start + (gmt_offset (&tt, t) - gmtoff)) * 10000000L);
				mono_array_set ((*data), gint64, 3, (gint64)(gmtoff - gmt_offset (&tt, t)) * 10000000L);
			}


			gmtoff = gmt_offset (&tt, t);
		}
	}

	if (!is_transitioned) {
		strftime (tzone, sizeof (tzone), "%Z", &tt);
		mono_array_setref ((*names), 0, mono_string_new_checked (domain, tzone, &error));
		mono_array_setref ((*names), 1, mono_string_new_checked (domain, tzone, &error));
		mono_array_set ((*data), gint64, 0, 0);
		mono_array_set ((*data), gint64, 1, 0);
		mono_array_set ((*data), gint64, 2, (gint64) gmtoff * 10000000L);
		mono_array_set ((*data), gint64, 3, 0);
		*daylight_inverted = 0;
	}

	return 1;
#else
	//On Windows, we should always load timezones in managed
	return 0;
#endif
}
