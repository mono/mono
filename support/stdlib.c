/*
 * <stdlib.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#include <stdlib.h>

#include "mph.h"

G_BEGIN_DECLS

void*
Mono_Posix_Stdlib_calloc (mph_size_t nmemb, mph_size_t size)
{
	if (mph_have_size_t_overflow(nmemb) || mph_have_size_t_overflow(size))
		return NULL;

	return calloc ((size_t) nmemb, (size_t) size);
}

void*
Mono_Posix_Stdlib_malloc (mph_size_t size)
{
	if (mph_have_size_t_overflow(size))
		return NULL;

	return malloc ((size_t) size);
}

void*
Mono_Posix_Stdlib_realloc (void* ptr, mph_size_t size)
{
	if (mph_have_size_t_overflow(size))
		return NULL;

	return realloc (ptr, (size_t) size);
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
