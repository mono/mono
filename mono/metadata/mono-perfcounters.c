/**
 * \file
 *
 * Performance counters support.
 *
 * Author: Paolo Molaro (lupus@ximian.com)
 *
 * Copyright 2008-2009 Novell, Inc (http://www.novell.com)
 * 2011 Xamarin, Inc
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include "config.h"
#include <time.h>
#include <string.h>
#include <stdlib.h>
#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif
#if defined (__OpenBSD__)
#include <sys/param.h>
#endif
#ifdef HAVE_SYS_TYPES_H
#include <sys/types.h>
#endif
#ifdef HAVE_SYS_TIME_H
#include <sys/time.h>
#endif
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
#include "metadata/mono-perfcounters.h"
#include "metadata/appdomain.h"
#include "metadata/object-internals.h"
/* for mono_stats */
#include "metadata/class-internals.h"
#include "utils/mono-time.h"
#include "utils/mono-mmap.h"
#include "utils/mono-proclib.h"
#include "utils/mono-networkinterfaces.h"
#include "utils/mono-error-internals.h"
#include "utils/atomic.h"
#include "utils/unlocked.h"

// FIXME merge with https://github.com/mono/mono/pull/6247/files
// Remove these macros and functions.
#define mono_string_handle_to_utf8 fixme_mono_string_handle_to_utf8
#define mono_unwrapped_string_to_utf8 fixme_mono_unwrapped_string_to_utf8

// FIXME much cleanup in this file

static char *
mono_unwrapped_string_to_utf8 (MonoUnwrappedString s, MonoError *error)
{
	GError *gerror = 0;
	long written = 0;

	char *utf8 = g_utf16_to_utf8 (s.chars, s.length, 0, &written, &gerror);

	if (gerror) {
		mono_error_set_argument (error, "string", "%s", gerror->message);
		g_error_free (gerror);
		g_free (utf8);
		return NULL;
	}

	if (s.length > written) {
		// g_utf16_to_utf8 may not be able to complete the conversion (e.g. NULL values were found, #335488)
		// allocate the total length, copy the part of the string that has been converted, and zero the rest
		char *zero_padded = (char *)g_malloc (s.length);
		memcpy (zero_padded, utf8, written);
		memset (zero_padded, 0, s.length - written);
		g_free (utf8);
		return zero_padded;
	}

	return utf8;
}

static char *
mono_string_handle_to_utf8 (MonoStringHandle s, MonoError *error)
{
	MonoUnwrappedString t = mono_unwrap_string_handle (s);

	char * const utf8 = mono_unwrapped_string_to_utf8 (t, error);

	mono_unwrapped_string_cleanup (&t);

	return utf8;
}

// FIXME Make these case insensitive?
// FIXME? foo_equal_bar vs. foo_bar_equal
#define perfcounter_utf16_equal_ascii g_utf16_ascii_equal
#define perfcounter_utf16_equal_asciiz g_utf16_asciiz_equal
#define perfcounter_string_handle_equal_ascii mono_string_handle_equal_ascii

/* map of CounterSample.cs */
struct _MonoCounterSample {
	gint64 rawValue;
	gint64 baseValue;
	gint64 counterFrequency;
	gint64 systemFrequency;
	gint64 timeStamp;
	gint64 timeStamp100nSec;
	gint64 counterTimeStamp;
	int counterType;
};

#ifndef DISABLE_PERFCOUNTERS
/* map of PerformanceCounterType.cs */
enum {
	NumberOfItemsHEX32=0x00000000,
	NumberOfItemsHEX64=0x00000100,
	NumberOfItems32=0x00010000,
	NumberOfItems64=0x00010100,
	CounterDelta32=0x00400400,
	CounterDelta64=0x00400500,
	SampleCounter=0x00410400,
	CountPerTimeInterval32=0x00450400,
	CountPerTimeInterval64=0x00450500,
	RateOfCountsPerSecond32=0x10410400,
	RateOfCountsPerSecond64=0x10410500,
	RawFraction=0x20020400,
	CounterTimer=0x20410500,
	Timer100Ns=0x20510500,
	SampleFraction=0x20C20400,
	CounterTimerInverse=0x21410500,
	Timer100NsInverse=0x21510500,
	CounterMultiTimer=0x22410500,
	CounterMultiTimer100Ns=0x22510500,
	CounterMultiTimerInverse=0x23410500,
	CounterMultiTimer100NsInverse=0x23510500,
	AverageTimer32=0x30020400,
	ElapsedTime=0x30240500,
	AverageCount64=0x40020500,
	SampleBase=0x40030401,
	AverageBase=0x40030402,
	RawBase=0x40030403,
	CounterMultiBase=0x42030500
};

/* maps a small integer type to the counter types above */
static const int
simple_type_to_type [] = {
	NumberOfItemsHEX32, NumberOfItemsHEX64,
	NumberOfItems32, NumberOfItems64,
	CounterDelta32, CounterDelta64,
	SampleCounter, CountPerTimeInterval32,
	CountPerTimeInterval64, RateOfCountsPerSecond32,
	RateOfCountsPerSecond64, RawFraction,
	CounterTimer, Timer100Ns,
	SampleFraction, CounterTimerInverse,
	Timer100NsInverse, CounterMultiTimer,
	CounterMultiTimer100Ns, CounterMultiTimerInverse,
	CounterMultiTimer100NsInverse, AverageTimer32,
	ElapsedTime, AverageCount64,
	SampleBase, AverageBase,
	RawBase, CounterMultiBase
};

enum {
	SingleInstance,
	MultiInstance,
	CatTypeUnknown = -1
};

enum {
	ProcessInstance,
	ThreadInstance,
	CPUInstance,
	MonoInstance,
	NetworkInterfaceInstance,
	CustomInstance
};

#define PERFCTR_CAT(id,name,help,type,inst,first_counter) CATEGORY_ ## id,
#define PERFCTR_COUNTER(id,name,help,type,field)
enum {
#include "mono-perfcounters-def.h"
	NUM_CATEGORIES
};

#undef PERFCTR_CAT
#undef PERFCTR_COUNTER
#define PERFCTR_CAT(id,name,help,type,inst,first_counter) CATEGORY_START_ ## id = -1,
#define PERFCTR_COUNTER(id,name,help,type,field) COUNTER_ ## id,
/* each counter is assigned an id starting from 0 inside the category */
enum {
#include "mono-perfcounters-def.h"
	END_COUNTERS
};

#undef PERFCTR_CAT
#undef PERFCTR_COUNTER
#define PERFCTR_CAT(id,name,help,type,inst,first_counter)
#define PERFCTR_COUNTER(id,name,help,type,field) CCOUNTER_ ## id,
/* this is used just to count the number of counters */
enum {
#include "mono-perfcounters-def.h"
	NUM_COUNTERS
};

static mono_mutex_t perfctr_mutex;
#define perfctr_lock() mono_os_mutex_lock (&perfctr_mutex)
#define perfctr_unlock() mono_os_mutex_unlock (&perfctr_mutex)

typedef struct {
	char reserved [16];
	int size;
	unsigned short counters_start;
	unsigned short counters_size;
	unsigned short data_start;
	MonoPerfCounters counters;
	char data [1];
} MonoSharedArea;

/*
  binary format of custom counters in shared memory, starting from MonoSharedArea* + data_start;
  basic stanza:
  struct stanza_header {
  	byte stanza_type; // FTYPE_*
  	byte other_info;
  	ushort stanza_length; // includeas header
  	... data ...
  }

// strings are utf8
// perfcat and perfinstance are 4-bytes aligned
struct perfcat {
	byte typeidx;
	byte categorytype;
	ushort length; // includes the counters
	ushort num_counters;
	ushort counters_data_size;
	int num_instances;
	char name[]; // null terminated
	char help[]; // null terminated
	// perfcounters follow
	{
		byte countertype;
		char name[]; // null terminated
		char help[]; // null terminated
	}
	0-byte
};

struct perfinstance {
	byte typeidx;
	byte data_offset; // offset of counters from beginning of struct
	ushort length;
	uint category_offset; // offset of category in the shared area
	char name[]; // null terminated
	// data follows: this is always 8-byte aligned
};

*/

enum {
	FTYPE_CATEGORY = 'C',
	FTYPE_DELETED = 'D',
	FTYPE_PREDEF_INSTANCE = 'P', // an instance of a predef counter
	FTYPE_INSTANCE = 'I',
	FTYPE_DIRTY = 'd',
	FTYPE_END = 0
};

typedef struct {
	unsigned char ftype;
	unsigned char extra;
	unsigned short size;
} SharedHeader;

typedef struct {
	SharedHeader header;
	unsigned short num_counters;
	unsigned short counters_data_size;
	int num_instances;
	/* variable length data follows */
	char name [1];
	// string name
	// string help
	// SharedCounter counters_info [num_counters]
} SharedCategory;

typedef struct {
	SharedHeader header;
	unsigned int category_offset;
	/* variable length data follows */
	char instance_name [1];
	// string name
} SharedInstance;

typedef struct {
	unsigned char type;
	guint8 seq_num;
	/* variable length data follows */
	char name [1];
	// string name
	// string help
} SharedCounter;

typedef struct {
	const char *name;
	const char *help;
	guint name_length;
	guint help_length;
	unsigned char id;
	signed int type : 2;
	unsigned int instance_type : 6;
	short first_counter;
} CategoryDesc;

typedef struct {
	const char *name;
	const char *help;
	guint name_length;
	guint help_length;
	short id;
	unsigned short offset; // offset inside MonoPerfCounters
	int type;
} CounterDesc;

#undef PERFCTR_CAT
#undef PERFCTR_COUNTER
#define PERFCTR_CAT(id,name,help,type,inst,first_counter) {name, help, sizeof (name) - 1, \
	sizeof (help) - 1, CATEGORY_ ## id, type, inst ## Instance, CCOUNTER_ ## first_counter},
#define PERFCTR_COUNTER(id,name,help,type,field)
static const CategoryDesc
predef_categories [] = {
#include "mono-perfcounters-def.h"
	{NULL, NULL, 0, 0, NUM_CATEGORIES, -1, 0, NUM_COUNTERS}
};

#undef PERFCTR_CAT
#undef PERFCTR_COUNTER
#define PERFCTR_CAT(id,name,help,type,inst,first_counter)
#define PERFCTR_COUNTER(id,name,help,type,field) {name, help, sizeof (name) - 1, \
	sizeof (help) - 1, COUNTER_ ## id, G_STRUCT_OFFSET (MonoPerfCounters, field), type},
static const CounterDesc
predef_counters [] = {
#include "mono-perfcounters-def.h"
	{NULL, NULL, 0, 0, -1, 0, 0}
};

/*
 * We have several different classes of counters:
 * *) system counters
 * *) runtime counters
 * *) remote counters
 * *) user-defined counters
 * *) windows counters (the implementation on windows will use this)
 *
 * To easily handle the differences we create a vtable for each class that contains the
 * function pointers with the actual implementation to access the counters.
 */
typedef struct _ImplVtable ImplVtable;

typedef MonoBoolean (*SampleFunc) (ImplVtable *vtable, MonoBoolean only_value, MonoCounterSample* sample);
typedef gint64 (*UpdateFunc) (ImplVtable *vtable, MonoBoolean do_incr, gint64 value);
typedef void (*CleanupFunc) (ImplVtable *vtable);

struct _ImplVtable {
	void *arg;
	SampleFunc sample;
	UpdateFunc update;
	CleanupFunc cleanup;
};

typedef struct {
	int id;
	char *name;
} NetworkVtableArg;

typedef struct {
	ImplVtable vtable;
	MonoPerfCounters *counters;
	int pid;
} PredefVtable;

typedef struct {
	ImplVtable vtable;
	SharedInstance *instance_desc;
	SharedCounter *counter_desc;
} CustomVTable;

static ImplVtable*
create_vtable (void *arg, SampleFunc sample, UpdateFunc update)
{
	ImplVtable *vtable = g_new0 (ImplVtable, 1);
	vtable->arg = arg;
	vtable->sample = sample;
	vtable->update = update;
	return vtable;
}

MonoPerfCounters *mono_perfcounters = NULL;
static MonoSharedArea *shared_area = NULL;

typedef struct {
	void *sarea;
	int refcount;
} ExternalSArea;

/* maps a pid to a ExternalSArea pointer */
static GHashTable *pid_to_shared_area = NULL;

static MonoSharedArea *
load_sarea_for_pid (int pid)
{
	ExternalSArea *data;
	MonoSharedArea *area = NULL;

	perfctr_lock ();
	if (pid_to_shared_area == NULL)
		pid_to_shared_area = g_hash_table_new (NULL, NULL);
	data = (ExternalSArea *)g_hash_table_lookup (pid_to_shared_area, GINT_TO_POINTER (pid));
	if (!data) {
		area = (MonoSharedArea *)mono_shared_area_for_pid (GINT_TO_POINTER (pid));
		if (area) {
			data = g_new (ExternalSArea, 1);
			data->sarea = area;
			data->refcount = 1;
			g_hash_table_insert (pid_to_shared_area, GINT_TO_POINTER (pid), data);
		}
	} else {
		area = (MonoSharedArea *)data->sarea;
		data->refcount ++;
	}
	perfctr_unlock ();
	return area;
}

static void
unref_pid_unlocked (int pid)
{
	ExternalSArea *data;
	data = (ExternalSArea *)g_hash_table_lookup (pid_to_shared_area, GINT_TO_POINTER (pid));
	if (data) {
		data->refcount--;
		if (!data->refcount) {
			g_hash_table_remove (pid_to_shared_area, GINT_TO_POINTER (pid));
			mono_shared_area_unload (data->sarea);
			g_free (data);
		}
	}
}

static void
predef_cleanup (ImplVtable *vtable)
{
	PredefVtable *vt = (PredefVtable*)vtable;
	/* ExternalSArea *data; */
	
	perfctr_lock ();
	if (!pid_to_shared_area) {
		perfctr_unlock ();
		return;
	}
	unref_pid_unlocked (vt->pid);
	perfctr_unlock ();
}

static guint64
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

static guint64
mono_determine_physical_ram_available_size (void)
{
#if defined (TARGET_WIN32)
	MEMORYSTATUSEX memstat;

	memstat.dwLength = sizeof (memstat);
	GlobalMemoryStatusEx (&memstat);
	return (guint64)memstat.ullAvailPhys;

#elif defined (__NetBSD__)
	struct vmtotal vm_total;
	guint64 page_size;
	int mib[2];
	size_t len;

	mib[0] = CTL_VM;
	mib[1] = VM_METER;

	len = sizeof (vm_total);
	sysctl (mib, 2, &vm_total, &len, NULL, 0);

	mib[0] = CTL_HW;
	mib[1] = HW_PAGESIZE;

	len = sizeof (page_size);
	sysctl (mib, 2, &page_size, &len, NULL, 0);

	return ((guint64) vm_total.t_free * page_size) / 1024;
#elif defined (__APPLE__)
	mach_msg_type_number_t count = HOST_VM_INFO_COUNT;
	mach_port_t host = mach_host_self();
	vm_size_t page_size;
	vm_statistics_data_t vmstat;
	kern_return_t ret;
	do {
		ret = host_statistics(host, HOST_VM_INFO, (host_info_t)&vmstat, &count);
	} while (ret == KERN_ABORTED);

	if (ret != KERN_SUCCESS) {
		g_warning ("Mono was unable to retrieve memory usage!");
		return 0;
	}

	host_page_size(host, &page_size);
	return (guint64) vmstat.free_count * page_size;

#elif defined (HAVE_SYSCONF)
	guint64 page_size = 0, num_pages = 0;

	/* sysconf works on most *NIX operating systems, if your system doesn't have it or if it
	 * reports invalid values, please add your OS specific code below. */
#ifdef _SC_PAGESIZE
	page_size = (guint64)sysconf (_SC_PAGESIZE);
#endif

#ifdef _SC_AVPHYS_PAGES
	num_pages = (guint64)sysconf (_SC_AVPHYS_PAGES);
#endif

	if (!page_size || !num_pages) {
		g_warning ("Your operating system's sysconf (3) function doesn't correctly report physical memory size!");
		return 0;
	}

	return page_size * num_pages;
#else
	return 0;
#endif
}

void
mono_perfcounters_init (void)
{
	int d_offset = G_STRUCT_OFFSET (MonoSharedArea, data);
	d_offset += 7;
	d_offset &= ~7;

	mono_os_mutex_init_recursive (&perfctr_mutex);

	shared_area = (MonoSharedArea *)mono_shared_area ();
	shared_area->counters_start = G_STRUCT_OFFSET (MonoSharedArea, counters);
	shared_area->counters_size = sizeof (MonoPerfCounters);
	shared_area->data_start = d_offset;
	shared_area->size = 4096;
	mono_perfcounters = &shared_area->counters;
}

static int
perfctr_type_compress (int type)
{
	int i;
	for (i = 0; i < G_N_ELEMENTS (simple_type_to_type); ++i) {
		if (simple_type_to_type [i] == type)
			return i;
	}
	/* NumberOfItems32 */
	return 2;
}

static SharedHeader*
shared_data_reserve_room (int size, int ftype)
{
	SharedHeader* header;
	unsigned char *p = (unsigned char *)shared_area + shared_area->data_start;
	unsigned char *end = (unsigned char *)shared_area + shared_area->size;

	size += 7;
	size &= ~7;
	while (p < end) {
		unsigned short *next;
		if (*p == FTYPE_END) {
			if (size < (end - p))
				goto res;
			return NULL;
		}
		if (p + 4 > end)
			return NULL;
		next = (unsigned short*)(p + 2);
		if (*p == FTYPE_DELETED) {
			/* we reuse only if it's the same size */
			if (*next == size) {
				goto res;
			}
		}
		p += *next;
	}
	return NULL;

res:
	header = (SharedHeader*)p;
	header->ftype = ftype;
	header->extra = 0; /* data_offset could overflow here, so we leave this field unused */
	header->size = size;

	return header;
}

typedef gboolean (*SharedFunc) (SharedHeader *header, void *data);

static void
foreach_shared_item_in_area (unsigned char *p, unsigned char *end, SharedFunc func, void *data)
{
	while (p < end) {
		unsigned short *next;
		if (p + 4 > end)
			return;
		next = (unsigned short*)(p + 2);
		if (!func ((SharedHeader*)p, data))
			return;
		if (*p == FTYPE_END)
			return;
		p += *next;
	}
}

static void
foreach_shared_item (SharedFunc func, void *data)
{
	unsigned char *p = (unsigned char *)shared_area + shared_area->data_start;
	unsigned char *end = (unsigned char *)shared_area + shared_area->size;

	foreach_shared_item_in_area (p, end, func, data);
}

typedef struct {
	const gunichar2 *name;
	SharedCategory *cat;
} CatSearch;

static gboolean
category_search (SharedHeader *header, void *data)
{
	CatSearch *search = (CatSearch *)data;
	if (header->ftype == FTYPE_CATEGORY) {
		SharedCategory *cat = (SharedCategory*)header;
		if (perfcounter_utf16_equal_asciiz (search->name, cat->name)) {
			search->cat = cat;
			return FALSE;
		}
	}
	return TRUE;
}

static SharedCategory*
find_custom_category (MonoUnwrappedString name)
{
	CatSearch search;
	search.name = name.chars;
	search.cat = NULL;
	foreach_shared_item (category_search, &search);
	return search.cat;
}

static gboolean
category_collect (SharedHeader *header, void *data)
{
	GSList **list = (GSList **)data;
	if (header->ftype == FTYPE_CATEGORY) {
		*list = g_slist_prepend (*list, header);
	}
	return TRUE;
}

static GSList*
get_custom_categories (void) {
	GSList *list = NULL;
	foreach_shared_item (category_collect, &list);
	return list;
}

static SharedCounter*
custom_category_counters (SharedCategory* cat)
{
	char *help = cat->name + strlen (cat->name) + 1;
	return (SharedCounter*)(help + strlen (help) + 1);
}

static SharedCounter*
next_custom_category_counter (SharedCounter* counter)
{
	char *help = counter->name + strlen (counter->name) + 1;
	return (SharedCounter*)(help + strlen (help) + 1);
}

static SharedCounter*
find_custom_counter (SharedCategory* cat, MonoUnwrappedString name)
{
	int i;
	SharedCounter *counter = custom_category_counters (cat);
	for (i = 0; i < cat->num_counters; ++i) {
		if (perfcounter_utf16_equal_asciiz (name.chars, counter->name))
			return counter;
		counter = next_custom_category_counter (counter);
	}
	return NULL;
}

typedef struct {
	unsigned int cat_offset;
	SharedCategory* cat;
	char *name;
	SharedInstance* result;
	GSList *list;
} InstanceSearch;

static gboolean
instance_search (SharedHeader *header, void *data)
{
	InstanceSearch *search = (InstanceSearch *)data;
	if (header->ftype == FTYPE_INSTANCE) {
		SharedInstance *ins = (SharedInstance*)header;
		if (search->cat_offset == ins->category_offset) {
			if (search->name) {
				if (strcmp (search->name, ins->instance_name) == 0) {
					search->result = ins;
					return FALSE;
				}
			} else {
				search->list = g_slist_prepend (search->list, ins);
			}
		}
	}
	return TRUE;
}

static SharedInstance*
find_custom_instance (SharedCategory* cat, char *name)
{
	InstanceSearch search;
	search.cat_offset = (char*)cat - (char*)shared_area;
	search.cat = cat;
	search.name = name;
	search.list = NULL;
	search.result = NULL;
	foreach_shared_item (instance_search, &search);
	return search.result;
}

static GSList*
get_custom_instances_list (SharedCategory* cat)
{
	InstanceSearch search;
	search.cat_offset = (char*)cat - (char*)shared_area;
	search.cat = cat;
	search.name = NULL;
	search.list = NULL;
	search.result = NULL;
	foreach_shared_item (instance_search, &search);
	return search.list;
}

static char*
custom_category_help (SharedCategory* cat)
{
	return cat->name + strlen (cat->name) + 1;
}

static const CounterDesc*
get_counter_in_category (const CategoryDesc *desc, MonoUnwrappedString counter)
{
	const CounterDesc *cdesc = &predef_counters [desc->first_counter];
	const CounterDesc *end = &predef_counters [desc [1].first_counter];
	for (; cdesc < end; ++cdesc) {
		if (perfcounter_utf16_equal_ascii (counter.chars, counter.length, cdesc->name, cdesc->name_length))
			return cdesc;
	}
	return NULL;
}

/* fill the info in sample (except the raw value) */
static void
fill_sample (MonoCounterSample *sample)
{
	sample->timeStamp = mono_100ns_ticks ();
	sample->timeStamp100nSec = sample->timeStamp;
	sample->counterTimeStamp = sample->timeStamp;
	sample->counterFrequency = 10000000;
	sample->systemFrequency = 10000000;
	// the real basevalue needs to be get from a different counter...
	sample->baseValue = 0;
}

static int
id_from_string (const gchar *id_str, gboolean is_process)
{
	int id = -1;
	if (strcmp("", id_str) != 0) {
		char *end;
		id = strtol (id_str, &end, 0);
		if (end == id_str && !is_process)
			id = -1;
	}
	return id;
}

static MonoBoolean
get_cpu_counter (ImplVtable *vtable, MonoBoolean only_value, MonoCounterSample *sample)
{
	MonoProcessError error;
	int id = GPOINTER_TO_INT (vtable->arg);
	int pid = id >> 5;
	id &= 0x1f;
	if (!only_value) {
		fill_sample (sample);
		sample->baseValue = 1;
	}
	sample->counterType = predef_counters [predef_categories [CATEGORY_CPU].first_counter + id].type;
	switch (id) {
	case COUNTER_CPU_USER_TIME:
		sample->rawValue = mono_cpu_get_data (pid, MONO_CPU_USER_TIME, &error);
		return TRUE;
	case COUNTER_CPU_PRIV_TIME:
		sample->rawValue = mono_cpu_get_data (pid, MONO_CPU_PRIV_TIME, &error);
		return TRUE;
	case COUNTER_CPU_INTR_TIME:
		sample->rawValue = mono_cpu_get_data (pid, MONO_CPU_INTR_TIME, &error);
		return TRUE;
	case COUNTER_CPU_DCP_TIME:
		sample->rawValue = mono_cpu_get_data (pid, MONO_CPU_DCP_TIME, &error);
		return TRUE;
	case COUNTER_CPU_PROC_TIME:
		sample->rawValue = mono_cpu_get_data (pid, MONO_CPU_IDLE_TIME, &error);
		return TRUE;
	}
	return FALSE;
}

static void*
cpu_get_impl (MonoUnwrappedString counter, const gchar* instance, int *type, MonoBoolean *custom)
{
	int id = id_from_string (instance, FALSE) << 5;
	const CounterDesc *cdesc;
	*custom = FALSE;
	/* increase the shift above and the mask also in the implementation functions */
	//g_assert (32 > desc [1].first_counter - desc->first_counter);
	if ((cdesc = get_counter_in_category (&predef_categories [CATEGORY_CPU], counter))) {
		*type = cdesc->type;
		return create_vtable (GINT_TO_POINTER (id | cdesc->id), get_cpu_counter, NULL);
	}
	return NULL;
}

static MonoBoolean
get_network_counter (ImplVtable *vtable, MonoBoolean only_value, MonoCounterSample *sample)
{
	MonoNetworkError error = MONO_NETWORK_ERROR_OTHER;
	NetworkVtableArg *narg = (NetworkVtableArg*) vtable->arg;
	if (!only_value) {
		fill_sample (sample);
	}

	sample->counterType = predef_counters [predef_categories [CATEGORY_NETWORK].first_counter + narg->id].type;
	switch (narg->id) {
	case COUNTER_NETWORK_BYTESRECSEC:
		sample->rawValue = mono_network_get_data (narg->name, MONO_NETWORK_BYTESREC, &error);
		break;
	case COUNTER_NETWORK_BYTESSENTSEC:
		sample->rawValue = mono_network_get_data (narg->name, MONO_NETWORK_BYTESSENT, &error);
		break;
	case COUNTER_NETWORK_BYTESTOTALSEC:
		sample->rawValue = mono_network_get_data (narg->name, MONO_NETWORK_BYTESTOTAL, &error);
		break;
	}

	if (error == MONO_NETWORK_ERROR_NONE)
		return TRUE;
	else
		return FALSE;
}

static void
network_cleanup (ImplVtable *vtable)
{
	NetworkVtableArg *narg;

	if (vtable == NULL)
		return;

	narg = (NetworkVtableArg *)vtable->arg;
	if (narg == NULL)
		return;

	g_free (narg->name);
	narg->name = NULL;
	g_free (narg);
	vtable->arg = NULL;
}

static void*
network_get_impl (MonoUnwrappedString counter, const gchar* instance, int *type, MonoBoolean *custom)
{
	const CounterDesc *cdesc;
	NetworkVtableArg *narg;
	ImplVtable *vtable;
	char *instance_name;

	*custom = FALSE;
	if ((cdesc = get_counter_in_category (&predef_categories [CATEGORY_NETWORK], counter))) {
		instance_name = g_strdup (instance);
		narg = g_new0 (NetworkVtableArg, 1);
		narg->id = cdesc->id;
		narg->name = instance_name;
		*type = cdesc->type;
		vtable = create_vtable (narg, get_network_counter, NULL);
		vtable->cleanup = network_cleanup;
		return vtable;
	}
	return NULL;
}

static MonoBoolean
get_process_counter (ImplVtable *vtable, MonoBoolean only_value, MonoCounterSample *sample)
{
	int id = GPOINTER_TO_INT (vtable->arg);
	int pid = id >> 5;
	if (pid < 0)
		return FALSE;
	id &= 0x1f;
	if (!only_value) {
		fill_sample (sample);
		sample->baseValue = 1;
	}
	sample->counterType = predef_counters [predef_categories [CATEGORY_PROC].first_counter + id].type;
	switch (id) {
	case COUNTER_PROC_USER_TIME:
		sample->rawValue = mono_process_get_data (GINT_TO_POINTER (pid), MONO_PROCESS_USER_TIME);
		return TRUE;
	case COUNTER_PROC_PRIV_TIME:
		sample->rawValue = mono_process_get_data (GINT_TO_POINTER (pid), MONO_PROCESS_SYSTEM_TIME);
		return TRUE;
	case COUNTER_PROC_PROC_TIME:
		sample->rawValue = mono_process_get_data (GINT_TO_POINTER (pid), MONO_PROCESS_TOTAL_TIME);
		return TRUE;
	case COUNTER_PROC_THREADS:
		sample->rawValue = mono_process_get_data (GINT_TO_POINTER (pid), MONO_PROCESS_NUM_THREADS);
		return TRUE;
	case COUNTER_PROC_VBYTES:
		sample->rawValue = mono_process_get_data (GINT_TO_POINTER (pid), MONO_PROCESS_VIRTUAL_BYTES);
		return TRUE;
	case COUNTER_PROC_WSET:
		sample->rawValue = mono_process_get_data (GINT_TO_POINTER (pid), MONO_PROCESS_WORKING_SET);
		return TRUE;
	case COUNTER_PROC_PBYTES:
		sample->rawValue = mono_process_get_data (GINT_TO_POINTER (pid), MONO_PROCESS_PRIVATE_BYTES);
		return TRUE;
	}
	return FALSE;
}

static void*
process_get_impl (MonoUnwrappedString counter, const gchar* instance, int *type, MonoBoolean *custom)
{
	int id = id_from_string (instance, TRUE) << 5;
	const CounterDesc *cdesc;
	*custom = FALSE;
	/* increase the shift above and the mask also in the implementation functions */
	//g_assert (32 > desc [1].first_counter - desc->first_counter);
	if ((cdesc = get_counter_in_category (&predef_categories [CATEGORY_PROC], counter))) {
		*type = cdesc->type;
		return create_vtable (GINT_TO_POINTER (id | cdesc->id), get_process_counter, NULL);
	}
	return NULL;
}

static MonoBoolean
mono_mem_counter (ImplVtable *vtable, MonoBoolean only_value, MonoCounterSample *sample)
{
	int id = GPOINTER_TO_INT (vtable->arg);
	if (!only_value) {
		fill_sample (sample);
		sample->baseValue = 1;
	}
	sample->counterType = predef_counters [predef_categories [CATEGORY_MONO_MEM].first_counter + id].type;
	switch (id) {
	case COUNTER_MEM_NUM_OBJECTS:
		sample->rawValue = 0;
		return TRUE;
	case COUNTER_MEM_PHYS_TOTAL:
		sample->rawValue = mono_determine_physical_ram_size ();
		return TRUE;
	case COUNTER_MEM_PHYS_AVAILABLE:
		sample->rawValue = mono_determine_physical_ram_available_size ();
		return TRUE;
	}
	return FALSE;
}

static void*
mono_mem_get_impl (MonoUnwrappedString counter, const gchar* instance, int *type, MonoBoolean *custom)
{
	const CounterDesc *cdesc;
	*custom = FALSE;
	if ((cdesc = get_counter_in_category (&predef_categories [CATEGORY_MONO_MEM], counter))) {
		*type = cdesc->type;
		return create_vtable (GINT_TO_POINTER ((gint) cdesc->id), mono_mem_counter, NULL);
	}
	return NULL;
}

static MonoBoolean
predef_readonly_counter (ImplVtable *vtable, MonoBoolean only_value, MonoCounterSample *sample)
{
	PredefVtable *vt = (PredefVtable *)vtable;
	const CounterDesc *desc;
	int cat_id = GPOINTER_TO_INT (vtable->arg);
	int id = cat_id >> 16;
	cat_id &= 0xffff;
	if (!only_value) {
		fill_sample (sample);
		sample->baseValue = 1;
	}
	desc = &predef_counters [predef_categories [cat_id].first_counter + id];
	sample->counterType = desc->type;
	/* FIXME: check that the offset fits inside imported counters */
	/*g_print ("loading %s at %d\n", desc->name, desc->offset);*/
	sample->rawValue = *(guint32*)((char*)vt->counters + desc->offset);
	return TRUE;
}

static ImplVtable*
predef_vtable (void *arg, const gchar *pids)
{
	MonoSharedArea *area;
	PredefVtable *vtable;
	int pid;

	pid = atoi (pids);
	area = load_sarea_for_pid (pid);
	if (!area)
		return NULL;

	vtable = g_new (PredefVtable, 1);
	vtable->vtable.arg = arg;
	vtable->vtable.sample = predef_readonly_counter;
	vtable->vtable.cleanup = predef_cleanup;
	vtable->counters = (MonoPerfCounters*)((char*)area + area->counters_start);
	vtable->pid = pid;

	return (ImplVtable*)vtable;
}

/* consider storing the pointer directly in vtable->arg, so the runtime overhead is lower:
 * this needs some way to set sample->counterType as well, though.
 */
static MonoBoolean
predef_writable_counter (ImplVtable *vtable, MonoBoolean only_value, MonoCounterSample *sample)
{
	int cat_id = GPOINTER_TO_INT (vtable->arg);
	int id = cat_id >> 16;
	cat_id &= 0xffff;
	if (!only_value) {
		fill_sample (sample);
		sample->baseValue = 1;
	}
	sample->counterType = predef_counters [predef_categories [cat_id].first_counter + id].type;
	switch (cat_id) {
	case CATEGORY_EXC:
		switch (id) {
		case COUNTER_EXC_THROWN:
			sample->rawValue = mono_atomic_load_i32 (&mono_perfcounters->exceptions_thrown);
			return TRUE;
		}
		break;
	case CATEGORY_ASPNET:
		switch (id) {
		case COUNTER_ASPNET_REQ_Q:
			sample->rawValue = mono_atomic_load_i32 (&mono_perfcounters->aspnet_requests_queued);
			return TRUE;
		case COUNTER_ASPNET_REQ_TOTAL:
			sample->rawValue = mono_atomic_load_i32 (&mono_perfcounters->aspnet_requests);
			return TRUE;
		}
		break;
	case CATEGORY_THREADPOOL:
		switch (id) {
		case COUNTER_THREADPOOL_WORKITEMS:
			sample->rawValue = mono_atomic_load_i64 (&mono_perfcounters->threadpool_workitems);
			return TRUE;
		case COUNTER_THREADPOOL_IOWORKITEMS:
			sample->rawValue = mono_atomic_load_i64 (&mono_perfcounters->threadpool_ioworkitems);
			return TRUE;
		case COUNTER_THREADPOOL_THREADS:
			sample->rawValue = mono_atomic_load_i32 (&mono_perfcounters->threadpool_threads);
			return TRUE;
		case COUNTER_THREADPOOL_IOTHREADS:
			sample->rawValue = mono_atomic_load_i32 (&mono_perfcounters->threadpool_iothreads);
			return TRUE;
		}
		break;
	case CATEGORY_JIT:
		switch (id) {
		case COUNTER_JIT_BYTES:
			sample->rawValue = mono_atomic_load_i32 (&mono_perfcounters->jit_bytes);
			return TRUE;
		case COUNTER_JIT_METHODS:
			sample->rawValue = mono_atomic_load_i32 (&mono_perfcounters->jit_methods);
			return TRUE;
		case COUNTER_JIT_TIME:
			sample->rawValue = mono_atomic_load_i32 (&mono_perfcounters->jit_time);
			return TRUE;
		case COUNTER_JIT_BYTES_PSEC:
			sample->rawValue = mono_atomic_load_i32 (&mono_perfcounters->jit_bytes);
			return TRUE;
		case COUNTER_JIT_FAILURES:
			sample->rawValue = mono_atomic_load_i32 (&mono_perfcounters->jit_failures);
			return TRUE;
		}
		break;
	}
	return FALSE;
}

static gint64
predef_writable_update (ImplVtable *vtable, MonoBoolean do_incr, gint64 value)
{
	gint32 *volatile ptr = NULL;
	gint64 *volatile ptr64 = NULL;
	int cat_id = GPOINTER_TO_INT (vtable->arg);
	int id = cat_id >> 16;
	cat_id &= 0xffff;
	switch (cat_id) {
	case CATEGORY_ASPNET:
		switch (id) {
		case COUNTER_ASPNET_REQ_Q: ptr = &mono_perfcounters->aspnet_requests_queued; break;
		case COUNTER_ASPNET_REQ_TOTAL: ptr = &mono_perfcounters->aspnet_requests; break;
		}
		break;
	case CATEGORY_THREADPOOL:
		switch (id) {
		case COUNTER_THREADPOOL_WORKITEMS: ptr64 = &mono_perfcounters->threadpool_workitems; break;
		case COUNTER_THREADPOOL_IOWORKITEMS: ptr64 = &mono_perfcounters->threadpool_ioworkitems; break;
		case COUNTER_THREADPOOL_THREADS: ptr = &mono_perfcounters->threadpool_threads; break;
		case COUNTER_THREADPOOL_IOTHREADS: ptr = &mono_perfcounters->threadpool_iothreads; break;
		}
		break;
	}
	if (ptr) {
		if (do_incr) {
			if (value == 1)
				return mono_atomic_inc_i32 (ptr);
			if (value == -1)
				return mono_atomic_dec_i32 (ptr);

			return mono_atomic_add_i32 (ptr, (gint32)value);
		}
		/* this can be non-atomic */
		*ptr = value;
		return value;
	} else if (ptr64) {
		if (do_incr) {
			if (value == 1)
				return UnlockedIncrement64 (ptr64); /* FIXME: use mono_atomic_inc_i64 () */
			if (value == -1)
				return UnlockedDecrement64 (ptr64); /* FIXME: use mono_atomic_dec_i64 () */

			return UnlockedAdd64 (ptr64, value); /* FIXME: use mono_atomic_add_i64 () */
		}
		/* this can be non-atomic */
		*ptr64 = value;
		return value;
	}
	return 0;
}

static void*
predef_writable_get_impl (int cat, MonoUnwrappedString counter, const gchar *instance, int *type, MonoBoolean *custom)
{
	const CounterDesc *cdesc;
	*custom = TRUE;
	if ((cdesc = get_counter_in_category (&predef_categories [cat], counter))) {
		*type = cdesc->type;
		if (instance == NULL || strcmp (instance, "") == 0)
			return create_vtable (GINT_TO_POINTER ((cdesc->id << 16) | cat), predef_writable_counter, predef_writable_update);
		else
			return predef_vtable (GINT_TO_POINTER ((cdesc->id << 16) | cat), instance);
	}
	return NULL;
}

static MonoBoolean
custom_writable_counter (ImplVtable *vtable, MonoBoolean only_value, MonoCounterSample *sample)
{
	CustomVTable *counter_data = (CustomVTable *)vtable;
	if (!only_value) {
		fill_sample (sample);
		sample->baseValue = 1;
	}
	sample->counterType = simple_type_to_type [counter_data->counter_desc->type];
	if (!vtable->arg)
		sample->rawValue = 0;
	else
		sample->rawValue = *(guint64*)vtable->arg;
	return TRUE;
}

static gint64
custom_writable_update (ImplVtable *vtable, MonoBoolean do_incr, gint64 value)
{
	/* FIXME: check writability */
	guint64 *ptr = (guint64 *)vtable->arg;
	if (ptr) {
		if (do_incr) {
			/* FIXME: we need to do this atomically */
			*ptr += value;
			return *ptr;
		}
		/* this can be non-atomic */
		*ptr = value;
		return value;
	}
	return 0;
}

static SharedInstance*
custom_get_instance (SharedCategory *cat, SharedCounter *scounter, char* name)
{
	SharedInstance* inst;
	char *p;
	int size;
	inst = find_custom_instance (cat, name);
	if (inst)
		return inst;
	size = sizeof (SharedInstance) + strlen (name);
	size += 7;
	size &= ~7;
	size += (sizeof (guint64) * cat->num_counters);
	perfctr_lock ();
	inst = (SharedInstance*) shared_data_reserve_room (size, FTYPE_INSTANCE);
	if (!inst) {
		perfctr_unlock ();
		return NULL;
	}
	inst->category_offset = (char*)cat - (char*)shared_area;
	cat->num_instances++;
	/* now copy the variable data */
	p = inst->instance_name;
	strcpy (p, name);
	p += strlen (name) + 1;
	perfctr_unlock ();

	return inst;
}

static ImplVtable*
custom_vtable (SharedCounter *scounter, SharedInstance* inst, char *data)
{
	CustomVTable* vtable;
	vtable = g_new0 (CustomVTable, 1);
	vtable->vtable.arg = data;
	vtable->vtable.sample = custom_writable_counter;
	vtable->vtable.update = custom_writable_update;
	vtable->instance_desc = inst;
	vtable->counter_desc = scounter;

	return (ImplVtable*)vtable;
}

static gpointer
custom_get_value_address (SharedCounter *scounter, SharedInstance* sinst)
{
	int offset = sizeof (SharedInstance) + strlen (sinst->instance_name);
	offset += 7;
	offset &= ~7;
	offset += scounter->seq_num * sizeof (guint64);
	return (char*)sinst + offset;
}

static void*
custom_get_impl (SharedCategory *cat, MonoUnwrappedString counter,
		MonoUnwrappedString instance, int *type, MonoError *error)
{
	SharedCounter *scounter;
	SharedInstance* inst;
	char *name;

	scounter = find_custom_counter (cat, counter);
	if (!scounter)
		return NULL;
	name = mono_unwrapped_string_to_utf8 (counter, error);
	return_val_if_nok (error, NULL);
	*type = simple_type_to_type [scounter->type];
	inst = custom_get_instance (cat, scounter, name);
	g_free (name);
	if (!inst)
		return NULL;
	return custom_vtable (scounter, inst, (char *)custom_get_value_address (scounter, inst));
}

static const CategoryDesc*
find_category (MonoUnwrappedString category)
{
	int i;
	const CategoryDesc* desc = predef_categories;
	for (i = 0; i < NUM_CATEGORIES; ++i) {
		if (perfcounter_utf16_equal_ascii (category.chars, category.length, desc->name, desc->name_length))
			return desc;
		++desc;
	}
	return NULL;
}

static void*
mono_perfcounter_get_impl (
		MonoUnwrappedString category, MonoUnwrappedString counter, MonoUnwrappedString instance,
		MonoStringHandle machine, int *type, MonoBoolean *custom, MonoError *error)
// separate function to minimize diff
{
<<<<<<< HEAD
	ERROR_DECL (error);
=======
>>>>>>> perfcounter icall work in progress
	const CategoryDesc *cdesc;
	void *result = NULL;

	/* no support for counters on other machines */
	if (!perfcounter_string_handle_equal_ascii (machine, ".", 1))
		return NULL;

	cdesc = find_category (category);
	if (!cdesc) {
		SharedCategory *scat = find_custom_category (category);
		if (!scat)
			return NULL;
		*custom = TRUE;
		result = custom_get_impl (scat, counter, instance, type, error);
<<<<<<< HEAD
		if (mono_error_set_pending_exception (error))
			return NULL;
		return result;
	}
	gchar *c_instance = mono_string_to_utf8_checked (instance, error);
	if (mono_error_set_pending_exception (error))
		return NULL;
=======
		return_val_if_nok (error, NULL);
		return result;
	}
	gchar *c_instance = mono_unwrapped_string_to_utf8 (instance, error);
	return_val_if_nok (error, NULL);
>>>>>>> perfcounter icall work in progress
	switch (cdesc->id) {
	case CATEGORY_CPU:
		result = cpu_get_impl (counter, c_instance, type, custom);
		break;
	case CATEGORY_PROC:
		result = process_get_impl (counter, c_instance, type, custom);
		break;
	case CATEGORY_MONO_MEM:
		result = mono_mem_get_impl (counter, c_instance, type, custom);
		break;
	case CATEGORY_NETWORK:
		result = network_get_impl (counter, c_instance, type, custom);
		break;
	case CATEGORY_JIT:
	case CATEGORY_EXC:
	case CATEGORY_GC:
	case CATEGORY_REMOTING:
	case CATEGORY_LOADING:
	case CATEGORY_THREAD:
	case CATEGORY_INTEROP:
	case CATEGORY_SECURITY:
	case CATEGORY_ASPNET:
	case CATEGORY_THREADPOOL:
		result = predef_writable_get_impl (cdesc->id, counter, c_instance, type, custom);
		break;
	}
	g_free (c_instance);
	return result;
}

void*
ves_icall_System_Diagnostics_PerformanceCounter_GetImpl (
	MonoStringHandle category_handle, MonoStringHandle counter_handle,
	MonoStringHandle instance_handle, MonoStringHandle machine, int *type,
	MonoBoolean *custom, MonoError *error)
// previously mono_perfcounter_get_impl
{
	MonoUnwrappedString category = mono_unwrap_string_handle (category_handle);
	MonoUnwrappedString counter = mono_unwrap_string_handle (counter_handle);
	MonoUnwrappedString instance = mono_unwrap_string_handle (instance_handle);
	
	void * const result = mono_perfcounter_get_impl (category, counter,
		instance, machine, type, custom, error);

	mono_unwrapped_string_cleanup (&category);
	mono_unwrapped_string_cleanup (&instance);
	mono_unwrapped_string_cleanup (&counter);
	return result;
}

MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounter_GetSample (
	void *impl, MonoBoolean only_value, MonoCounterSample *sample, MonoError *error)
// previously mono_perfcounter_get_sample
{
	ImplVtable *vtable = (ImplVtable *)impl;
	if (vtable && vtable->sample)
		return vtable->sample (vtable, only_value, sample);
	return FALSE;
}

gint64
ves_icall_System_Diagnostics_PerformanceCounter_UpdateValue (
	void *impl, MonoBoolean do_incr, gint64 value, MonoError *error)
// previously mono_perfcounter_update_value
{
	ImplVtable *vtable = (ImplVtable *)impl;
	if (vtable && vtable->update)
		return vtable->update (vtable, do_incr, value);
	return 0;
}

void
ves_icall_System_Diagnostics_PerformanceCounter_FreeData (void *impl, MonoError *error)
// previously mono_perfcounter_free_data
{
	ImplVtable *vtable = (ImplVtable *)impl;
	if (vtable && vtable->cleanup)
		vtable->cleanup (vtable);
	g_free (impl);
}

/* Category icalls */

static MonoBoolean
mono_perfcounter_category_del (
	MonoUnwrappedString name, MonoError *error)
// FIXME merge with ves_icall_System_Diagnostics_PerformanceCounterCategory_CategoryDelete
{
	const CategoryDesc *cdesc;
	SharedCategory *cat;
	cdesc = find_category (name);
	/* can't delete a predefined category */
	if (cdesc)
		return FALSE;
	perfctr_lock ();
	cat = find_custom_category (name);
	/* FIXME: check the semantics, if deleting a category means also deleting the instances */
	if (!cat || cat->num_instances) {
		perfctr_unlock ();
		return FALSE;
	}
	cat->header.ftype = FTYPE_DELETED;
	perfctr_unlock ();
	return TRUE;
}

MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounterCategory_CategoryDelete (
	MonoStringHandle name_handle, MonoError *error)
{
	MonoUnwrappedString name = mono_unwrap_string_handle (name_handle);	
	MonoBoolean const result = mono_perfcounter_category_del (name, error);
 	mono_unwrapped_string_cleanup (&name);
	return result;
}

MonoStringHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_CategoryHelpInternal (
	MonoStringHandle category_handle, MonoStringHandle machine, MonoError *error)
// previously mono_perfcounter_category_help
{
	MONO_HANDLE_LOCAL_VARIABLE_INITIALIZED_NULL (MonoString, result);
	const CategoryDesc *cdesc;
	MonoUnwrappedString category = mono_unwrap_string_handle (category_handle);

	/* no support for counters on other machines */
	if (!perfcounter_string_handle_equal_ascii (machine, ".", 1))
		goto return_null;

	cdesc = find_category (category);
	if (!cdesc) {
		SharedCategory *scat = find_custom_category (category);
		if (!scat)
			goto return_null;
		result = mono_string_new_handle (mono_domain_get (), custom_category_help (scat), error);
		goto_if_nok (error, return_null);
		goto exit;
	}
	result = mono_string_new_handle_length (mono_domain_get (), cdesc->help, cdesc->help_length, error);
	goto_if_nok (error, return_null);
exit:
	mono_unwrapped_string_cleanup (&category);
	return result;

return_null:
	MONO_HANDLE_SET_NULL (MonoString, result);
	goto exit;
}

/*
 * Check if the category named @category exists on @machine. If @counter is not NULL, return
 * TRUE only if a counter with that name exists in the category.
 */
static MonoBoolean
mono_perfcounter_category_exists (
	MonoUnwrappedString counter, MonoUnwrappedString category, MonoStringHandle machine)
// FIXME merge with ves_icall_System_Diagnostics_PerformanceCounterCategory_CounterCategoryExists
{
	const CategoryDesc *cdesc;
	/* no support for counters on other machines */
	if (!perfcounter_string_handle_equal_ascii (machine, ".", 1))
		return FALSE;

	cdesc = find_category (category);
	if (!cdesc) {
		SharedCategory *scat = find_custom_category (category);
		if (!scat)
			return FALSE;
		/* counter is allowed to be null */
		if (!counter.chars)
			return TRUE;
		/* search through the custom category */
		return find_custom_counter (scat, counter) != NULL;
	}
	/* counter is allowed to be null */
	if (!counter.chars)
		return TRUE;
	if (get_counter_in_category (cdesc, counter))
		return TRUE;
	return FALSE;
}

MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounterCategory_CounterCategoryExists (
	MonoStringHandle counter_handle, MonoStringHandle category_handle, MonoStringHandle machine, MonoError *error)
{
	MonoUnwrappedString category = mono_unwrap_string_handle (category_handle);
	MonoUnwrappedString counter = mono_unwrap_string_handle (counter_handle);

	MonoBoolean const result = mono_perfcounter_category_exists (counter, category, machine);

	mono_unwrapped_string_cleanup (&counter);
	mono_unwrapped_string_cleanup (&category);
	return result;
}

/* C map of the type with the same name */
typedef struct {
	MonoObject object;
	MonoString *help;
	MonoString *name;
	int type;
} CounterCreationData;

TYPED_HANDLE_DECL (CounterCreationData);

static char*
append (char* cursor, const char* str)
{
	int const length = strlen (str) + 1;
	memcpy (cursor, str, length);
	return cursor + length;
}

static void
System_Diagnostics_PerformanceCounterCategory_Create_create_temp (
	MonoArrayHandle items, int i, char **counter_info, MonoError *error)
// Avoid creating co-op handles in loops -- move loop body into functions.
{
	HANDLE_FUNCTION_ENTER()

	MONO_HANDLE_LOCAL_VARIABLE_INITIALIZED_NULL (CounterCreationData, data);
	MONO_HANDLE_ARRAY_GETREF (data, items, i);
	// FIXME All that should really happen here is compute the size of the UTF8 conversion,
	// not actually allocate and copy the data.
	counter_info [i * 2] = mono_string_handle_to_utf8 (MONO_HANDLE_NEW_GET (MonoString, data, name), error);
	goto_if_nok (error, failure);
	counter_info [i * 2 + 1] = mono_string_handle_to_utf8 (MONO_HANDLE_NEW_GET (MonoString, data, help), error);
	goto_if_nok (error, failure);
failure:
	HANDLE_FUNCTION_RETURN ();
}

static char*
System_Diagnostics_PerformanceCounterCategory_Create_copy_temp_to_shared (
	MonoArrayHandle items, int i, char** counter_info, char* p)
// Avoid creating co-op handles in loops -- move loop body into functions.
{
	HANDLE_FUNCTION_ENTER()

	MONO_HANDLE_LOCAL_VARIABLE_INITIALIZED_NULL (CounterCreationData, data);
	MONO_HANDLE_ARRAY_GETREF (data, items, i);
	/* emit the SharedCounter structures */
	*p++ = perfctr_type_compress (MONO_HANDLE_GETVAL (data, type));
	*p++ = i;
	// FIXME This should be UTF8 conversion into supplied buffer, previously
	// sized correctly.
	p = append (p, counter_info [i * 2]);
	p = append (p, counter_info [i * 2 + 1]);

	HANDLE_FUNCTION_RETURN_VAL (p);
}

/*
 * Since we'll keep a copy of the category per-process, we should also make sure
 * categories with the same name are compatible.
 */
MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounterCategory_Create (
	MonoStringHandle category, MonoStringHandle help, int type, MonoArrayHandle items, MonoError *error)
{
// previously mono_perfcounter_create
// FIXME integer overflow throughout this function.
// FIXME This function unnecessarily builds up all the data
// into a temporary, and then copies it to shared memory.
// There is very little value in the temporary it is just extra code and cost.
// All the temporary does is avoid sizing utf8 data separate from conversion to utf8.
// Sizing and copying are roughly the same cost (strlen vs. strcpy), and this
// "optimization" costs heap.

	int result = FALSE;
	int i, size;
	int num_counters = mono_array_handle_length (items);
	int counters_data_size;
	char *name = NULL;
	char *chelp = NULL;
	char **counter_info = NULL;
	char *p;
	SharedCategory *cat;

	/* FIXME: ensure there isn't a category created already */
	name = mono_string_handle_to_utf8 (category, error);
	goto_if_nok (error, failure);

	chelp = mono_string_handle_to_utf8 (help, error);
	goto_if_nok (error, failure);
	
	// Build up all the data into a temporary and then copy
	// into shared memory. FIXME avoid the extra copying.

	counter_info = g_new0 (char*, num_counters * 2);
	/* calculate the size we need structure size + name/help + 2 0 string terminators */
	size = G_STRUCT_OFFSET (SharedCategory, name) + strlen (name) + strlen (chelp) + 2;
	for (i = 0; i < num_counters; ++i) {
		System_Diagnostics_PerformanceCounterCategory_Create_create_temp (
			items, i, counter_info, error);
		goto_if_nok (error, failure);
		size += sizeof (SharedCounter) + 1; /* 1 is for the help 0 terminator */
	}
	for (i = 0; i < num_counters * 2; ++i) {
		if (!counter_info [i])
			goto failure;
		size += strlen (counter_info [i]) + 1;
	}
	size += 7;
	size &= ~7;
	counters_data_size = num_counters * 8; /* optimize for size later */
	if (size > 65535)
		goto failure;
	perfctr_lock ();
	cat = (SharedCategory*) shared_data_reserve_room (size, FTYPE_CATEGORY);
	if (!cat) {
		perfctr_unlock ();
		goto failure;
	}
	cat->num_counters = num_counters;
	cat->counters_data_size = counters_data_size;
	/* now copy the variable data */
	p = append (cat->name, name);
	p = append (p, chelp);

	for (i = 0; i < num_counters; ++i)
		p = System_Diagnostics_PerformanceCounterCategory_Create_copy_temp_to_shared (
			items, i, counter_info, p);

	perfctr_unlock ();
	result = TRUE;
failure:
	// Data has been copied into shared location, always free local copy.
	if (counter_info) {
		for (i = 0; i < num_counters * 2; ++i) {
			g_free (counter_info [i]);
		}
		g_free (counter_info);
	}
	g_free (name);
	g_free (chelp);
	return result;
}

int
ves_icall_System_Diagnostics_PerformanceCounterCategory_InstanceExistsInternal (
	MonoStringHandle instance, MonoStringHandle category_handle, MonoStringHandle machine, MonoError *error)
// previously mono_perfcounter_instance_exist
{
	const CategoryDesc *cdesc;
	SharedInstance *sinst;
	char *name = NULL;
	int result = FALSE;
	MonoUnwrappedString category = mono_unwrap_string_handle (category_handle);
	/* no support for counters on other machines */
	/*FIXME: machine appears to be wrong
	if (!perfcounter_string_handle_equal_ascii (machine, ".", 1))
		goto return_false;*/
	cdesc = find_category (category);
	if (!cdesc) {
		SharedCategory *scat;
		scat = find_custom_category (category);
		if (!scat)
			goto return_false;
		name = mono_string_handle_to_utf8 (instance, error);
		goto_if_nok (error, return_false);
		sinst = find_custom_instance (scat, name);
		if (sinst)
			goto return_true;
	} else {
		/* FIXME: search instance */
	}
	goto return_false;
exit:
	g_free (name);
	mono_unwrapped_string_cleanup (&category);
	return result;
return_false:
	result = FALSE;
	goto exit;
return_true:
	result = TRUE;
	goto exit;
}

static void
mono_new_string_into_array (
	MonoDomain *domain, const char *str, size_t len, MonoArrayHandle array, size_t i, MonoError *error)
// Avoid creating co-op handles in loops -- move loop body into functions.
{
	HANDLE_FUNCTION_ENTER()

	MonoStringHandle strh = mono_string_new_handle_length (domain, str, len, error);
	goto_if_nok (error, exit);
	MONO_HANDLE_ARRAY_SETREF (array, i, strh);
exit:
	HANDLE_FUNCTION_RETURN ();
}

MonoArrayHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_GetCategoryNames (MonoStringHandle machine, MonoError *error)
// previously mono_perfcounter_category_names
{
	int i;
	MONO_HANDLE_LOCAL_VARIABLE_INITIALIZED_NULL (MonoArray, res);
	MonoDomain *domain = mono_domain_get ();
	GSList *custom_categories, *tmp;
	custom_categories = NULL;
	gboolean unlock = FALSE;

	/* no support for counters on other machines */
	if (!perfcounter_string_handle_equal_ascii (machine, ".", 1))
		goto return_empty;

	perfctr_lock ();
	unlock = TRUE;
	custom_categories = get_custom_categories ();
	res = mono_array_new_handle (domain, mono_get_string_class (), NUM_CATEGORIES + g_slist_length (custom_categories), error);
	goto_if_nok (error, return_null);

	for (i = 0; i < NUM_CATEGORIES; ++i) {
		const CategoryDesc *cdesc = &predef_categories [i];
		mono_new_string_into_array (domain, cdesc->name, cdesc->name_length, res, i, error);
		// Upon error, return partial results like old code.
		goto_if_nok (error, exit);
	}

	for (tmp = custom_categories; tmp; tmp = tmp->next) {
		SharedCategory *scat = (SharedCategory *)tmp->data;
		mono_new_string_into_array (domain, scat->name, strlen (scat->name), res, i, error);
		// Upon error, return partial results like old code.
		goto_if_nok (error, exit);
		i++;
	}

	goto exit;
	
return_null:
	MONO_HANDLE_SET_NULL (MonoArray, res);
	goto exit;
	
return_empty: // Return a zero sized array, not null, no error.
	res = mono_array_new_handle (domain, mono_get_string_class (), 0, error);
	goto exit;

exit:
	if (unlock)
		perfctr_unlock ();
	g_slist_free (custom_categories);
	return res;
}

MonoArrayHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_GetCounterNames (
	MonoStringHandle category_handle, MonoStringHandle machine, MonoError *error)
// previously mono_perfcounter_counter_names
{
	int i;
	SharedCategory *scat;
	const CategoryDesc *cdesc;
	MonoDomain *domain = mono_domain_get ();
	gboolean unlock = FALSE;

	MONO_HANDLE_LOCAL_VARIABLE_INITIALIZED_NULL (MonoArray, res);
	MonoUnwrappedString category = mono_unwrap_string_handle (category_handle);

	/* no support for counters on other machines */
	if (!perfcounter_string_handle_equal_ascii (machine, ".", 1))
		goto return_empty;

	cdesc = find_category (category);
	if (cdesc) {
		res = mono_array_new_handle (domain, mono_get_string_class (), cdesc [1].first_counter - cdesc->first_counter, error);
		goto_if_nok (error, return_null);
		for (i = cdesc->first_counter; i < cdesc [1].first_counter; ++i) {
			const CounterDesc *desc = &predef_counters [i];
			mono_new_string_into_array (domain, desc->name, desc->name_length, res, i, error);
			goto_if_nok (error, return_null);
		}
		goto exit;
	}

	perfctr_lock ();
	unlock = TRUE;
	scat = find_custom_category (category);
	if (scat) {
		SharedCounter *counter = custom_category_counters (scat);
		for (i = 0; i < scat->num_counters; ++i) {
			mono_new_string_into_array (domain, counter->name, strlen (counter->name), res, i, error);
			goto_if_nok (error, exit);
			counter = next_custom_category_counter (counter);
		}
	} else
		goto return_empty;
	
	goto exit;

return_null:
	MONO_HANDLE_SET_NULL (MonoArray, res);
	goto exit;
	
return_empty: // Return a zero sized array, not null, no error.
	res = mono_array_new_handle (domain, mono_get_string_class (), 0, error);
	goto exit;

exit:
	if (unlock)
		perfctr_unlock ();
	mono_unwrapped_string_cleanup (&category);
	return res;
}

static MonoArrayHandle
get_string_array (void **array, int count, gboolean is_process, MonoError *error)
{
	HANDLE_FUNCTION_ENTER()

	int i;
	MonoDomain *domain = mono_domain_get ();
	MonoArrayHandle res = mono_array_new_handle (domain, mono_get_string_class (), count, error);
	goto_if_nok (error, return_null);

	for (i = 0; i < count; ++i) {
		char buf [128];
		char *p;
		if (is_process) {
			char *pname = mono_process_get_name (array [i], buf, sizeof (buf));
			p = g_strdup_printf ("%d/%s", GPOINTER_TO_INT (array [i]), pname);
		} else {
			sprintf (buf, "%d", GPOINTER_TO_INT (array [i]));
			p = buf;
		}
		mono_new_string_into_array (domain, p, strlen (p), res, i, error);
		if (p != buf)
			g_free (p);
		goto_if_nok (error, return_null);
	}

	goto exit;

return_null:
	MONO_HANDLE_SET_NULL (MonoArray, res);
exit:
	HANDLE_FUNCTION_RETURN_REF (res);
}

static MonoArrayHandle
get_string_array_of_strings (void **array, int count, MonoError *error)
{
	HANDLE_FUNCTION_ENTER()

	int i;
	MonoDomain *domain = mono_domain_get ();
	MonoArrayHandle res = mono_array_new_handle (domain, mono_get_string_class (), count, error);
	goto_if_nok (error, return_null);

	for (i = 0; i < count; ++i) {
		char const * const p = (char*)array[i];
		mono_new_string_into_array (domain, p, strlen (p), res, i, error);
		goto_if_nok (error, return_null);
	}

	goto exit;
return_null:
	MONO_HANDLE_SET_NULL (MonoArray, res);
exit:
	HANDLE_FUNCTION_RETURN_REF (res);
}

static MonoArrayHandle
get_mono_instances (MonoError *error)
{
	HANDLE_FUNCTION_ENTER()

	int count = 64;
	int res;
	void **buf = NULL;
	MONO_HANDLE_LOCAL_VARIABLE_INITIALIZED_NULL (MonoArray, array);

	do {
		count *= 2;
		g_free (buf);
		buf = g_new (void*, count);
		res = mono_shared_area_instances (buf, count);
	} while (res == count);
	array = get_string_array (buf, res, TRUE, error);
	g_free (buf);
	HANDLE_FUNCTION_RETURN_REF (array)
}

static MonoArrayHandle
get_cpu_instances (MonoError *error)
{
	HANDLE_FUNCTION_ENTER()

	MONO_HANDLE_LOCAL_VARIABLE_INITIALIZED_NULL (MonoArray, array);
	int const count = mono_cpu_count () + 1; /* +1 for "_Total" */
	void ** const buf = g_new (void*, count);
	for (int i = 0; i < count; ++i)
		buf [i] = GINT_TO_POINTER (i - 1); /* -1 => _Total */
	array = get_string_array (buf, count, FALSE, error);
	g_free (buf);
	MonoStringHandle total = mono_string_new_handle (mono_domain_get (), "_Total", error);
	goto_if_nok (error, return_null);
	MONO_HANDLE_ARRAY_SETREF (array, 0, total);

	goto exit;
return_null:
	MONO_HANDLE_SET_NULL (MonoArray, array);
exit:
	HANDLE_FUNCTION_RETURN_REF (array)
}

static MonoArrayHandle
get_processes_instances (MonoError *error)
{
	HANDLE_FUNCTION_ENTER()

	MONO_HANDLE_LOCAL_VARIABLE_INITIALIZED_NULL (MonoArray, array);
	int count = 0;
	void **buf = mono_process_list (&count);
	if (!buf)
		array = get_string_array (NULL, 0, FALSE, error);
	else
		array = get_string_array (buf, count, TRUE, error);
	g_free (buf);
	HANDLE_FUNCTION_RETURN_REF (array);
}

static MonoArrayHandle
get_networkinterface_instances (MonoError *error)
{
	HANDLE_FUNCTION_ENTER()

	MONO_HANDLE_LOCAL_VARIABLE_INITIALIZED_NULL (MonoArray, array);
	int count = 0;
	void **buf = mono_networkinterface_list (&count);
	if (!buf)
		array = get_string_array_of_strings (NULL, 0, error);
	else
		array = get_string_array_of_strings (buf, count, error);
	g_strfreev ((char **) buf);
	HANDLE_FUNCTION_RETURN_REF (array);
}

static MonoArrayHandle
get_custom_instances (MonoUnwrappedString category, MonoError *error)
{
	HANDLE_FUNCTION_ENTER()

	SharedCategory *scat;
	MONO_HANDLE_LOCAL_VARIABLE_INITIALIZED_NULL (MonoArray, array);
	GSList *list = NULL;
	MonoDomain *domain = mono_domain_get ();

	scat = find_custom_category (category);
	if (scat) {
		GSList *list = get_custom_instances_list (scat);
		GSList *tmp;
		int i = 0;
		array = mono_array_new_handle (domain, mono_get_string_class (), g_slist_length (list), error);
		goto_if_nok (error, return_null);
		for (tmp = list; tmp; tmp = tmp->next) {
			SharedInstance *inst = (SharedInstance *)tmp->data;
			char *s = inst->instance_name;
			mono_new_string_into_array (domain, s, strlen (s), array, i, error);
			goto_if_nok (error, return_null);
			i++;
		}
		goto exit;
	}
	// Return a zero sized array, not null, no error.
	array = mono_array_new_handle (domain, mono_get_string_class (), 0, error);
	goto exit;
return_null:
	MONO_HANDLE_SET_NULL (MonoArray, array);
exit:
	g_slist_free (list);
	HANDLE_FUNCTION_RETURN_REF (array)
}

MonoArrayHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_GetInstanceNames (
	MonoStringHandle category_handle, MonoStringHandle machine, MonoError *error)
// previously mono_perfcounter_instance_names
{
	const CategoryDesc* cat;
	MONO_HANDLE_LOCAL_VARIABLE_INITIALIZED_NULL (MonoArray, result);
	MonoUnwrappedString category = mono_unwrap_string_handle (category_handle);

	if (!perfcounter_string_handle_equal_ascii (machine, ".", 1))
		goto return_empty;
	
	cat = find_category (category);
	if (!cat) {
		result = get_custom_instances (category, error);
		goto exit;
	}
	switch (cat->instance_type) {
	case MonoInstance:
		result = get_mono_instances (error);
		break;
	case CPUInstance:
		result = get_cpu_instances (error);
		break;
	case ProcessInstance:
		result = get_processes_instances (error);
		break;
	case NetworkInterfaceInstance:
		result = get_networkinterface_instances (error);
		break;
	case ThreadInstance:
	default:
		goto return_empty;
	}

	goto exit;

return_empty: // Return a zero sized array, not null, no error.
	result = mono_array_new_handle (mono_domain_get (), mono_get_string_class (), 0, error);
	goto exit;

exit:
	mono_unwrapped_string_cleanup (&category);
>>>>>>> perfcounter icall work in progress
	return result;
}

typedef struct {
	PerfCounterEnumCallback cb;
	void *data;
} PerfCounterForeachData;

static gboolean
mono_perfcounter_foreach_shared_item (SharedHeader *header, gpointer data)
{
	int i;
	char *p, *name;
	unsigned char type;
	void *addr;
	SharedCategory *cat;
	SharedCounter *counter;
	SharedInstance *inst;
	PerfCounterForeachData *foreach_data = (PerfCounterForeachData *)data;

	if (header->ftype == FTYPE_CATEGORY) {
		cat = (SharedCategory*)header;

		p = cat->name;
		p += strlen (p) + 1; /* skip category name */
		p += strlen (p) + 1; /* skip category help */

		for (i = 0; i < cat->num_counters; ++i) {
			counter = (SharedCounter*) p;
			type = (unsigned char)*p++;
			/* seq_num = (int)* */ p++;
			name = p;
			p += strlen (p) + 1;
			/* help = p; */
			p += strlen (p) + 1;

			inst = custom_get_instance (cat, counter, name);
			if (!inst)
				return FALSE;
			addr = custom_get_value_address (counter, inst);
			if (!foreach_data->cb (cat->name, name, type, addr ? *(gint64*)addr : 0, foreach_data->data))
				return FALSE;
		}
	}

	return TRUE;
}

void
mono_perfcounter_foreach (PerfCounterEnumCallback cb, gpointer data)
{
	PerfCounterForeachData foreach_data = { cb, data };

	perfctr_lock ();

	foreach_shared_item (mono_perfcounter_foreach_shared_item, &foreach_data);

	perfctr_unlock ();
}

#else

void*
ves_icall_System_Diagnostics_PerformanceCounter_GetImpl (
		MonoStringHandle category, MonoStringHandle counter, MonoStringHandle instance,
		MonoStringHandle machine, int *type, MonoBoolean *custom, MonoError *error)
{
	g_assert_not_reached ();
}

MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounter_GetSample (
	void *impl, MonoBoolean only_value, MonoCounterSample *sample, MonoError *error)
{
	g_assert_not_reached ();
}

gint64
ves_icall_System_Diagnostics_PerformanceCounter_UpdateValue (
	void *impl, MonoBoolean do_incr, gint64 value, MonoError *error)
{
	g_assert_not_reached ();
}

void
ves_icall_System_Diagnostics_PerformanceCounter_FreeData (void *impl, MonoError *error)
{
	g_assert_not_reached ();
}

/* Category icalls */
MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounterCategory_CategoryDelete (
	MonoStringHandle name, MonoError *error)
{
	g_assert_not_reached ();
}

MonoStringHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_CategoryHelpInternal (
	MonoStringHandle category, MonoStringHandle machine, MonoError *error)
{
	g_assert_not_reached ();
}

MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounterCategory_CounterCategoryExists (
	MonoStringHandle counter, MonoStringHandle category, MonoStringHandle machine, MonoError *error)
{
	g_assert_not_reached ();
}

MonoBoolean
ves_icall_System_Diagnostics_PerformanceCounterCategory_Create (
	MonoStringHandle category, MonoStringHandle help, int type, MonoArrayHandle items, MonoError *error)
{
	g_assert_not_reached ();
}

int
ves_icall_System_Diagnostics_PerformanceCounterCategory_InstanceExistsInternal (
	MonoStringHandle instance, MonoStringHandle category, MonoStringHandle machine, MonoError *error)
{
	g_assert_not_reached ();
}

MonoArrayHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_GetCategoryNames (
	MonoStringHandle machine, MonoError *error)
{
	g_assert_not_reached ();
}

MonoArrayHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_GetCounterNames (
	MonoStringHandle category, MonoStringHandle machine, MonoError *error)
{
	g_assert_not_reached ();
}

MonoArrayHandle
ves_icall_System_Diagnostics_PerformanceCounterCategory_GetInstanceNames (
	MonoStringHandle category, MonoStringHandle machine, MonoError *error)
{
	g_assert_not_reached ();
}

#endif
