/**
 * \file
 */

#ifndef __MONO_UTILS_MONO_THREADS_DEBUG_H__
#define __MONO_UTILS_MONO_THREADS_DEBUG_H__

#include <config.h>
#include <glib.h>

/* Logging - enable them below if you need specific logging for the category you need */
#define MOSTLY_ASYNC_SAFE_FPRINTF(handle, ...) do { \
	g_async_safe_fprintf (handle, __VA_ARGS__); \
} while (0)

void
mono_debug_print_to_memory (const char*, ...);

void
THREADS_DEBUG (const char* a, gpointer b = 0, gpointer c = 0, gpointer d = 0);

void
THREADS_STW_DEBUG (const char* a, gpointer b = 0, gpointer c = 0, gpointer d = 0, gpointer e = 0);

void
THREADS_SUSPEND_DEBUG (const char* a, gpointer b = 0, gpointer c = 0, gpointer d = 0, gpointer e = 0, gpointer f = 0);

void
THREADS_STATE_MACHINE_DEBUG (const char* a, gpointer b = 0, gpointer c = 0, gpointer d = 0, gpointer e = 0,
			     gpointer f = 0, gpointer g = 0, gpointer h = 0, gpointer i = 0, gpointer j = 0);

#define THREADS_STATE_MACHINE_DEBUG_ENABLED

#if 1
#define THREADS_INTERRUPT_DEBUG(...)
#else
#define THREADS_INTERRUPT_DEBUG mono_debug_print_to_memory
#endif

#endif /* __MONO_UTILS_MONO_THREADS_DEBUG_H__ */
