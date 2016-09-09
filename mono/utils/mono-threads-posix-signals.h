
#ifndef __MONO_THREADS_POSIX_SIGNALS_H__
#define __MONO_THREADS_POSIX_SIGNALS_H__

#include <config.h>
#include <glib.h>

#include "mono-threads.h"

#if defined(USE_POSIX_BACKEND)

int
mono_threads_posix_signal_search_alternative (int min_signal);

void
mono_threads_posix_init_signals (void);

gint
mono_threads_posix_get_suspend_signal (void);

gint
mono_threads_posix_get_restart_signal (void);

gint
mono_threads_posix_get_abort_signal (void);

#endif /* defined(USE_POSIX_BACKEND) */

#endif /* __MONO_THREADS_POSIX_SIGNALS_H__ */
