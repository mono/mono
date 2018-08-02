/**
 * \file
 */

#ifndef __MONO_UTILS_DL_WINDOWS_H__
#define __MONO_UTILS_DL_WINDOWS_H__

#include <config.h>
#include <glib.h>

#ifdef HOST_WIN32
#include "mono/utils/mono-dl.h"

G_BEGIN_DECLS

void*
mono_dl_lookup_symbol_in_process (const char *symbol_name);

G_END_DECLS

#endif /* HOST_WIN32 */
#endif /* __MONO_UTILS_DL_WINDOWS_H__ */

