/**
 * \file
 */

#ifndef __MONO_UTILS_DL_WINDOWS_H__
#define __MONO_UTILS_DL_WINDOWS_H__

#ifdef HOST_WIN32

#include <config.h>
#include <glib.h>
#include "mono/utils/mono-dl.h"

void*
mono_dl_lookup_symbol_in_process (const char *symbol_name);

#endif /* HOST_WIN32 */
#endif /* __MONO_UTILS_DL_WINDOWS_H__ */
