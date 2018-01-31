/**
 * \file
 */

#ifndef __MONO_PATH_H
#define __MONO_PATH_H

#include <glib.h>
#include <mono/utils/mono-publib.h>

MONO_BEGIN_DECLS

MONO_API gchar *mono_path_resolve_symlinks (const char *path);
MONO_API gchar *mono_path_canonicalize (const char *path);

MONO_END_DECLS

#endif /* __MONO_PATH_H */

