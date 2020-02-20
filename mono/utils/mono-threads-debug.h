/**
 * \file
 */

#ifndef __MONO_UTILS_MONO_THREADS_DEBUG_H__
#define __MONO_UTILS_MONO_THREADS_DEBUG_H__

#include <config.h>
#include <glib.h>

void
mono_debug_print_to_memory (const char*, ...);

#if 0
#define THREADS_DEBUG(...)
#else
#define THREADS_DEBUG mono_debug_print_to_memory
#endif

#if 0
#define THREADS_STW_DEBUG(...)
#else
#define THREADS_STW_DEBUG mono_debug_print_to_memory
#endif

#if 0
#define THREADS_SUSPEND_DEBUG(...)
#else
#define THREADS_SUSPEND_DEBUG mono_debug_print_to_memory
#endif

#if 0
#define THREADS_STATE_MACHINE_DEBUG(...)
#else
#define THREADS_STATE_MACHINE_DEBUG_ENABLED
#define THREADS_STATE_MACHINE_DEBUG mono_debug_print_to_memory
#endif

#if 1
#define THREADS_INTERRUPT_DEBUG(...)
#else
#define THREADS_INTERRUPT_DEBUG mono_debug_print_to_memory
#endif

#endif /* __MONO_UTILS_MONO_THREADS_DEBUG_H__ */
