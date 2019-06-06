#ifndef _MONO_STRENC_INTERNALS_H_
#define _MONO_STRENC_INTERNALS_H_

#include <glib.h>

gchar *mono_unicode_to_external_error (const gunichar2 *uni, GError **err);

#endif /* _MONO_STRENC_INTERNALS_H_ */
