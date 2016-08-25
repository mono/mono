/*
 * mutexes.h: Mutex handles
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 */

#ifndef _WAPI_MUTEXES_H_
#define _WAPI_MUTEXES_H_

#include <glib.h>

#include <pthread.h>

G_BEGIN_DECLS

void
wapi_mutex_abandon (gpointer data, pid_t pid, pthread_t tid);

G_END_DECLS

#endif /* _WAPI_MUTEXES_H_ */
