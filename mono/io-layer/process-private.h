/*
 * process-private.h: Private definitions for process handles
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002-2006 Novell, Inc.
 */

#ifndef _WAPI_PROCESS_PRIVATE_H_
#define _WAPI_PROCESS_PRIVATE_H_

#include <config.h>
#include <glib.h>

#include <mono/utils/mono-os-semaphore.h>

/* There doesn't seem to be a defined symbol for this */
#define _WAPI_PROCESS_CURRENT (gpointer)0xFFFFFFFF

void _wapi_processes_init (void);
extern gpointer _wapi_process_duplicate (void);
extern void wapi_processes_cleanup (void);

#endif /* _WAPI_PROCESS_PRIVATE_H_ */
