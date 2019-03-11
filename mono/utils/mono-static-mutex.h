/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
/**
 * \file
 * Initializers for PRIO_INHERIT mutexes
 * Based on mono-os-mutex.h
 *
 * Authors: Jeffrey Stedfast <fejj@ximian.com>
 *          Kurt Van Dijck <dev.kurt@vandijck-laurijssen.be>
 *
 * Copyright 2002 Ximian, Inc. (www.ximian.com)
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#ifndef __MONO_STATIC_MUTEX_H__
#define __MONO_STATIC_MUTEX_H__

#if !defined(HOST_WIN32)
#include <pthread.h>
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define st_error(FMT, ...) \
	({ \
		fprintf(stderr, (FMT), ##__VA_ARGS__); \
		exit(1); \
	 })

static inline int
mono_os_static_mutex_init_type(pthread_mutex_t *mutex, int type)
{
	int res;
	pthread_mutexattr_t attr;

	res = pthread_mutexattr_init (&attr);
	if (res)
		st_error ("%s: pthread_mutex_lock failed with \"%s\" (%d)", __func__, strerror (res), res);

	res = pthread_mutexattr_settype (&attr, type);
	if (res)
		st_error ("%s: pthread_mutex_lock failed with \"%s\" (%d)", __func__, strerror (res), res);

#ifdef PTHREAD_PRIO_INHERIT
	/* use PTHREAD_PRIO_INHERIT if possible */
	res = pthread_mutexattr_setprotocol (&attr, PTHREAD_PRIO_INHERIT);
	if (res)
		st_error ("%s: pthread_mutex_lock failed with \"%s\" (%d)", __func__, strerror (res), res);
#endif

	res = pthread_mutex_init (mutex, &attr);
	if (res)
		st_error ("%s: pthread_mutex_lock failed with \"%s\" (%d)", __func__, strerror (res), res);

	res = pthread_mutexattr_destroy (&attr);
	if (res)
		st_error ("%s: pthread_mutex_lock failed with \"%s\" (%d)", __func__, strerror (res), res);
}

/* The idea of this helper is to leave all static mutex initializers intact
 * and to run the initializer on systems that support PTHREAD_PRIO_INHERIT
 * so that those systems overwrite the static initializer during constructor.
 *
 * This serves only to initialize with PTHREAD_PRIO_INHERIT since I know
 * no static initializer for it
 */
static inline int
mono_os_static_mutex_init(pthread_mutex_t *mutex)
{
	mono_os_static_mutex_init_type(mutex, PTHREAD_MUTEX_DEFAULT);
}

/* the actual test whether to run the constructor.
 * It may become more fine-grained later on
 */
#define HAS_PTHREAD_PRIO_INHERIT	defined(PTHREAD_PRIO_INHERIT)

#endif /* !defined HOST_WIN32 */
#endif /* __MONO_STATIC_MUTEX_H__ */
