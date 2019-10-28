/**
 * \file
 * Time utility functions.
 * Author: Paolo Molaro (<lupus@ximian.com>)
 * Copyright (C) 2008 Novell, Inc.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include <config.h>
#include <stdlib.h>
#include <stdio.h>

#ifdef HAVE_SYS_TIME_H
#include <sys/time.h>
#endif

#include <utils/mono-time.h>


#define MTICKS_PER_SEC (10 * 1000 * 1000)

gint64
mono_msec_ticks (void)
{
	return mono_100ns_ticks () / 10 / 1000;
}

#ifdef HOST_WIN32
#include <windows.h>

#ifndef _MSC_VER
/* we get "error: implicit declaration of function 'GetTickCount64'" */
WINBASEAPI ULONGLONG WINAPI GetTickCount64(void);
#endif

gint64
mono_msec_boottime (void)
{
	/* GetTickCount () is reportedly monotonic */
	return GetTickCount64 ();
}

/* Returns the number of 100ns ticks from unspecified time: this should be monotonic */
gint64
mono_100ns_ticks (void)
{
	static LARGE_INTEGER freq;
	static UINT64 start_time;
	UINT64 cur_time;
	LARGE_INTEGER value;

	if (!freq.QuadPart) {
		if (!QueryPerformanceFrequency (&freq))
			return mono_100ns_datetime ();
		QueryPerformanceCounter (&value);
		start_time = value.QuadPart;
	}
	QueryPerformanceCounter (&value);
	cur_time = value.QuadPart;
	/* we use unsigned numbers and return the difference to avoid overflows */
	return (cur_time - start_time) * (double)MTICKS_PER_SEC / freq.QuadPart;
}

/* Returns the number of 100ns ticks since Jan 1, 1601, UTC timezone */
gint64
mono_100ns_datetime (void)
{
	ULARGE_INTEGER ft;

	if (sizeof(ft) != sizeof(FILETIME))
		g_assert_not_reached ();

	GetSystemTimeAsFileTime ((FILETIME*) &ft);
	return ft.QuadPart;
}

#else


#if defined (HAVE_SYS_PARAM_H)
#include <sys/param.h>
#endif
#if defined(HAVE_SYS_SYSCTL_H)
#include <sys/sysctl.h>
#endif

#if defined(HOST_DARWIN)
#include <mach/mach.h>
#include <mach/mach_time.h>
#endif

#include <time.h>

/* a made up uptime of 300 seconds */
#define MADEUP_BOOT_TIME (300 * MTICKS_PER_SEC)

#if defined(ANDROID)
#include <math.h>
/*  CLOCK_MONOTONIC is the most reliable clock type on Android. However, it
    does not tick when the device is sleeping (case 867885, case 1037712).
    CLOCK_BOOTTIME includes that time, but is very unreliable. Some older
 
    devices had this time ticking back or jumping back and forth (case 970945)
    To fix this issue we combine both clocks to produce a CLOCK_MONOTONIC-based
    clock that ticks even when the device is disabled.
*/
gint64 android_get_time_since_startup(double current_monotonic_time, double current_boottime_time)
{
	static double monotonic_start_time = -HUGE_VAL;
	static double boottime_start_time = -HUGE_VAL;
	static double boottime_adjustment = 0;
	static int broken_boottime = 0;
	static double broken_boottime_detection_hysteresis = 0.001;
	static double adjustment_hysteresis_when_bootime_good = 0.001;
	static double adjustment_hysteresis_when_bootime_broken = 8;

	if (monotonic_start_time == -HUGE_VAL)
		monotonic_start_time = current_monotonic_time;
	if (boottime_start_time == -HUGE_VAL)
		boottime_start_time = current_boottime_time;
	double monotonicSinceStart = current_monotonic_time - monotonic_start_time;
	double boottimeSinceStart = current_boottime_time - boottime_start_time;
	/*  In theory, boottime can only go faster than monotonic, so whenever we detect
		this condition we assume that device was asleep and we must adjust the returned
		time by the amount of time that the boottime jumped forwards.
		In the real world, boottime can go slower than monotonic or even backwards.
		We work around this by only taking into account the total difference between
		boottime and monotonic times and only adjusting monotonic time when this difference
		increases.
		There's also a problem that on some devices the boottime continuously jumps
		forwards and backwards by ~4 seconds. This means that a naive implementation would
		often do more than one time jump after device sleeps, depending on which part
		of the jump "cycle" we landed. We work around this by introducing hysteresis of
		hysteresisSeconds seconds and adjusting monotonic time only when this adjustment
		changes by more than hysteresisSeconds amount, but only on broken devices.
		On devices with broken CLOCK_BOOTTIME behaviour this would ignore device sleeps of
		hysteresisSeconds or less, which is small compromise to make.
	*/
	if (boottimeSinceStart - monotonicSinceStart < -broken_boottime_detection_hysteresis)
		broken_boottime = 1;
	double hysteresisSeconds = broken_boottime ? adjustment_hysteresis_when_bootime_broken : adjustment_hysteresis_when_bootime_good;
	if (boottimeSinceStart - monotonicSinceStart > boottime_adjustment + hysteresisSeconds)
		boottime_adjustment = boottimeSinceStart - monotonicSinceStart;
	return (gint64)(monotonicSinceStart + boottime_adjustment);
}
#endif

#if defined(CLOCK_MONOTONIC)
static gint64
get_posix_time_for_class(int clock_class)
{
	struct timespec tspec;
	static struct timespec tspec_freq = {0};
	static int can_use_clock = 0;
	if (!tspec_freq.tv_nsec) {
		can_use_clock = clock_getres (clock_class, &tspec_freq) == 0;
		/*printf ("resolution: %lu.%lu\n", tspec_freq.tv_sec, tspec_freq.tv_nsec);*/
	}
	if (can_use_clock) {
		if (clock_gettime (clock_class, &tspec) == 0) {
			/*printf ("time: %lu.%lu\n", tspec.tv_sec, tspec.tv_nsec); */
			return ((gint64)tspec.tv_sec * MTICKS_PER_SEC + tspec.tv_nsec / 100);
		}
	}
}
#endif
 
static gint64
get_boot_time (void)
{
#if defined (HAVE_SYS_PARAM_H) && defined (KERN_BOOTTIME)
	int mib [2];
	size_t size;
	time_t now;
	struct timeval boottime;

	(void)time(&now);

	mib [0] = CTL_KERN;
	mib [1] = KERN_BOOTTIME;

	size = sizeof(boottime);

	if (sysctl(mib, 2, &boottime, &size, NULL, 0) != -1)
		return (gint64)((now - boottime.tv_sec) * MTICKS_PER_SEC);
#else
	FILE *uptime = fopen ("/proc/uptime", "r");
	if (uptime) {
		double upt;
		if (fscanf (uptime, "%lf", &upt) == 1) {
			gint64 now = mono_100ns_datetime ();
			fclose (uptime);
			return now - (gint64)(upt * MTICKS_PER_SEC);
		}
		fclose (uptime);
	}
#endif
	return (gint64)MADEUP_BOOT_TIME;
}

/* Returns the number of milliseconds from boot time: this should be monotonic */
gint64
mono_msec_boottime (void)
{
#if defined(ANDROID)
	return get_posix_time_for_class(CLOCK_BOOTTIME);
#else
	static gint64 boot_time = 0;
	gint64 now;
	if (!boot_time)
		boot_time = get_boot_time ();
	now = mono_100ns_datetime ();
	/*printf ("now: %llu (boot: %llu) ticks: %llu\n", (gint64)now, (gint64)boot_time, (gint64)(now - boot_time));*/
	return (now - boot_time)/10000;
#endif
}

/* Returns the number of 100ns ticks from unspecified time: this should be monotonic */
gint64
mono_100ns_ticks (void)
{
	struct timeval tv;
#if defined(HOST_DARWIN)
	/* http://developer.apple.com/library/mac/#qa/qa1398/_index.html */
	static mach_timebase_info_data_t timebase;
	guint64 now = mach_absolute_time ();
	if (timebase.denom == 0) {
		mach_timebase_info (&timebase);
		timebase.denom *= 100; /* we return 100ns ticks */
	}
	return now * timebase.numer / timebase.denom;
#elif defined(CLOCK_MONOTONIC)
#if defined(ANDROID)
	return android_get_time_since_startup(get_posix_time_for_class(CLOCK_BOOTTIME), get_posix_time_for_class(CLOCK_MONOTONIC));
#else
	return get_posix_time_for_class(CLOCK_MONOTONIC);
#endif
#endif
	if (gettimeofday (&tv, NULL) == 0)
		return ((gint64)tv.tv_sec * 1000000 + tv.tv_usec) * 10;
	return 0;
}

/*
 * Magic number to convert unix epoch start to windows epoch start
 * Jan 1, 1970 into a value which is relative to Jan 1, 1601.
 */
#define EPOCH_ADJUST    ((guint64)11644473600LL)

/* Returns the number of 100ns ticks since 1/1/1601, UTC timezone */
gint64
mono_100ns_datetime (void)
{
	struct timeval tv;
	if (gettimeofday (&tv, NULL) == 0)
		return mono_100ns_datetime_from_timeval (tv);
	return 0;
}

gint64
mono_100ns_datetime_from_timeval (struct timeval tv)
{
	return (((gint64)tv.tv_sec + EPOCH_ADJUST) * 1000000 + tv.tv_usec) * 10;
}

#endif

