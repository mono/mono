/*
 * mono-semaphore.h:  Definitions for generic semaphore usage
 *
 * Author:
 *	Geoff Norton  <gnorton@novell.com>
 *
 * (C) 2009 Novell, Inc.
 */

#ifndef _MONO_SEMAPHORE_H_
#define _MONO_SEMAPHORE_H_

#include <config.h>
#include <glib.h>

#define MONO_HAS_SEMAPHORES 1

#if defined(USE_MACH_SEMA)

#include <mach/mach_init.h>
#include <mach/task.h>
#include <mach/semaphore.h>

typedef semaphore_t MonoSemType;

#elif defined(HAVE_SEMAPHORE_H) && !defined(HOST_WIN32)

#include <semaphore.h>

typedef sem_t MonoSemType;

#else

#include <winsock2.h>
#include <windows.h>

typedef HANDLE MonoSemType;

#endif

G_BEGIN_DECLS

int
mono_sem_init (MonoSemType *sem, int value);

int
mono_sem_destroy (MonoSemType *sem);

int
mono_sem_wait (MonoSemType *sem, gboolean alertable);

int
mono_sem_timedwait (MonoSemType *sem, guint32 timeout_ms, gboolean alertable);

int
mono_sem_post (MonoSemType *sem);

G_END_DECLS

#endif /* _MONO_SEMAPHORE_H_ */
