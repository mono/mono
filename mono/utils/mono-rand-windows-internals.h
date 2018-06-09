/**
 * \file
 */

#ifndef _MONO_UTILS_RAND_WINDOWS_H_
#define _MONO_UTILS_RAND_WINDOWS_H_

#include <config.h>
#include <glib.h>

#ifdef HOST_WIN32

long
mono_rand_win_gen (guchar *buffer, size_t buffer_size);

#endif /* HOST_WIN32 */
#endif /* _MONO_UTILS_RAND_WINDOWS_H_ */
