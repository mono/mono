/**
 * Author: Nathan Ricci (naricc@microsoft.com)
 * Copyright 2019 Microsoft Corp. 
 */


#include <mono/utils/determine_physical_ram_size.h>

#if defined (__APPLE__)
#include <mach/message.h>
#include <mach/mach_host.h>
#include <mach/host_info.h>
#include <sys/sysctl.h>
#endif
#if defined (__NetBSD__)
#include <sys/param.h>
#include <sys/sysctl.h>
#include <sys/vmmeter.h>
#endif

guint64
mono_determine_physical_ram_size (void)
{
#if defined (TARGET_WIN32)
	MEMORYSTATUSEX memstat;

	memstat.dwLength = sizeof (memstat);
	GlobalMemoryStatusEx (&memstat);
	return (guint64)memstat.ullTotalPhys;
#elif defined (__NetBSD__) || defined (__APPLE__)
#ifdef __NetBSD__
	unsigned long value;
#else
	guint64 value;
#endif
	int mib[2] = {
		CTL_HW,
#ifdef __NetBSD__
		HW_PHYSMEM64
#else
		HW_MEMSIZE
#endif
	};
	size_t size_sys = sizeof (value);

	sysctl (mib, 2, &value, &size_sys, NULL, 0);
	if (value == 0)
		return 134217728;

	return (guint64)value;
#elif defined (HAVE_SYSCONF)
	guint64 page_size = 0, num_pages = 0;

	/* sysconf works on most *NIX operating systems, if your system doesn't have it or if it
	 * reports invalid values, please add your OS specific code below. */
#ifdef _SC_PAGESIZE
	page_size = (guint64)sysconf (_SC_PAGESIZE);
#endif

#ifdef _SC_PHYS_PAGES
	num_pages = (guint64)sysconf (_SC_PHYS_PAGES);
#endif

	if (!page_size || !num_pages) {
		g_warning ("Your operating system's sysconf (3) function doesn't correctly report physical memory size!");
		return 134217728;
	}

	return page_size * num_pages;
#else
	return 134217728;
#endif
}