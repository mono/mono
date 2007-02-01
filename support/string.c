/*
 * <string.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2005 Jonathan Pryor
 */

#include <string.h>

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

guint64
Mono_Posix_Stdlib_strlen (void* p)
{
	return strlen ((const char*) p);
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
