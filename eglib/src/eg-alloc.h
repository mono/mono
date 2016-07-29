#ifndef __EG_ALLOC_H__
#define __EG_ALLOC_H__

#include <glib.h>

typedef void (*EGAllocTagCallback)(void *address, size_t size, const char *tag);
void eg_mem_set_alloc_tag_callback (EGAllocTagCallback cb);

void eg_report_alloc (void *address, size_t size, const char *tag);

static inline void*
eg_malloc (size_t size, const char * tag)
{
	void *res = g_malloc (size);
	eg_report_alloc (res, size, tag);
	return res;
}

static inline void*
eg_malloc0 (size_t size, const char * tag)
{
	void *res = g_malloc0 (size);
	eg_report_alloc (res, size, tag);
	return res;
}

#define eg_new(type,size,tag)       ((type *) eg_malloc (sizeof (type)* (size), tag))
#define eg_new0(type,size,tag)       ((type *) eg_malloc0 (sizeof (type)* (size), tag))

#endif
