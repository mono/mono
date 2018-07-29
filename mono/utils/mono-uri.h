/**
 * \file
 */

#ifndef __MONO_URI_H
#define __MONO_URI_H
#include <glib.h>
#include <mono/utils/mono-publib.h>

MONO_BEGIN_DECLS

MONO_API gchar * mono_escape_uri_string (const gchar *string);

MONO_END_DECLS

#endif /* __MONO_URI_H */
