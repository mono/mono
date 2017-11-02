#include <config.h>
#include <glib.h>

#include <mono/utils/mono-compiler.h>

#ifdef HAVE_SYS_TIME_H
	#include <sys/time.h>
#endif

#include "Time-c-api.h"

gint64
mono_msec_ticks (void)
{
	return (gint64) UnityPalGetTicksMillisecondsMonotonic();
}

/* Returns the number of 100ns ticks from unspecified time: this should be monotonic */
gint64
mono_100ns_ticks (void)
{
	return (gint64) UnityPalGetTicks100NanosecondsMonotonic();
}

/* Returns the number of 100ns ticks since 1/1/1601, UTC timezone */
gint64
mono_100ns_datetime (void)
{
	return (gint64) UnityPalGetTicks100NanosecondsDateTime();
}

gint64
mono_msec_boottime (void)
{
	return (gint64) UnityPalGetTicksMillisecondsMonotonic();
}

#ifndef HOST_WIN32
gint64 mono_100ns_datetime_from_timeval (struct timeval tv)
{
	g_assert_not_reached();
	return 0;
}
#endif

