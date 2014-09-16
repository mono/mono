#ifndef __MONO_METADATA_MEMPROF_H__
#define __MONO_METADATA_MEMPROF_H__

#include <glib.h>
#include <mono/utils/mono-compiler.h>


typedef enum {
	MEMDOM_APPDOMAIN,
	MEMDOM_IMAGE,
	MEMDOM_IMAGE_SET,
	MEMDOM_MAX,
} MemoryDomainKind;

void mono_profiler_register_memory_domain (gpointer domain, MemoryDomainKind kind) MONO_INTERNAL;
void mono_profiler_free_memory_domain (gpointer domain) MONO_INTERNAL;

void mono_profiler_add_mempool_alloc (gpointer domain, gsize size, const char *tag) MONO_INTERNAL;

/*use this for large allocations that are not automtically tracked */
void mono_profiler_add_allocation (const char *tag, gsize size) MONO_INTERNAL;
void mono_profiler_remove_allocation (const char *tag, gsize size) MONO_INTERNAL;

void mono_profiler_valloc (gpointer address, size_t size, const char *tag) MONO_INTERNAL;
void mono_profiler_vfree (gpointer address, size_t size) MONO_INTERNAL;

void mono_memory_profiler_init (void) MONO_INTERNAL;

void mono_memprof_dump (void);

#endif
