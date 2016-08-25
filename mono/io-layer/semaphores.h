/*
 * semaphores.h:  Semaphore handles
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 */

#ifndef _WAPI_SEMAPHORES_H_
#define _WAPI_SEMAPHORES_H_

#include <glib.h>

G_BEGIN_DECLS

#include "mono/metadata/w32handle-namespace.h"

struct _WapiHandle_sem
{
	guint32 val;
	gint32 max;
};

struct _WapiHandle_namedsem
{
	struct _WapiHandle_sem s;
	MonoW32HandleNamespace sharedns;
};

extern gboolean ReleaseSemaphore(gpointer handle, gint32 count,
				 gint32 *prevcount);
extern gpointer OpenSemaphore (guint32 access, gboolean inherit,
			       const gunichar2 *name);

G_END_DECLS
#endif /* _WAPI_SEMAPHORES_H_ */
