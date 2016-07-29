#ifndef __MONO_ALLOC_H__
#define __MONO_ALLOC_H__

#include <glib.h>
#include <mono/metadata/profiler-private.h>


static void*
m_malloc (size_t size, const char * tag)
{
	void *res = g_malloc (size);
	mono_profiler_malloc (res, size, tag);
	return res;
}

static void*
m_malloc0 (size_t size, const char * tag)
{
	void *res = g_malloc0 (size);
	mono_profiler_malloc (res, size, tag);
	return res;
}

#define m_new(type,size,tag)       ((type *) m_malloc (sizeof (type)* (size), tag))
#define m_new0(type,size,tag)       ((type *) m_malloc0 (sizeof (type)* (size), tag))


#endif
