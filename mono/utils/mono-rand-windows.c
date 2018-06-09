/**
 * \file
 * Windows rand support for Mono.
 *
 * Copyright 2016 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
*/
#include <config.h>
#include <glib.h>
#ifdef HOST_WIN32
#include "mono-error.h"
#include "mono-error-internals.h"
#include "mono-rand.h"
#include <windows.h>
#include "mono/utils/mono-rand-windows-internals.h"
#include <brypt.h>

#ifndef BCRYPT_USE_SYSTEM_PREFERRED_RNG
#define BCRYPT_USE_SYSTEM_PREFERRED_RNG (2)
#endif

long
mono_rand_win_gen (guchar *buffer, size_t buffer_size)
{
	while (buffer_size > 0) {
		ULONG const size = (ULONG)MIN (buffer_size, MAXULONG);
		long const status = BCryptGenRandom (NULL, buffer, size, BCRYPT_USE_SYSTEM_PREFERRED_RNG);
		if (status < 0)
			return status;
		buffer += size;
		buffer_size -= size;
	}
	return 0;
}

/**
 * mono_rand_open:
 *
 * Returns: True if random source is global, false if mono_rand_init can be called repeatedly to get randomness instances.
 *
 * Initializes entire RNG system. Must be called once per process before calling mono_rand_init.
 */
gboolean
mono_rand_open (void)
{
	return TRUE;
}

/**
 * mono_rand_init:
 * \param seed A string containing seed data
 * \param seed_size Length of seed string
 * Initializes an RNG client.
 * \returns On success, a non-NULL handle which can be used to fetch random data from \c mono_rand_try_get_bytes. On failure, NULL.
 */
gpointer
mono_rand_init (guchar *seed, gint seed_size)
{
	return (gpointer)"BCryptGenRandom"; // NULL will be interpreted as failure; return arbitrary nonzero pointer
}

/**
 * mono_rand_try_get_bytes:
 * \param handle A pointer to an RNG handle. Handle is set to NULL on failure.
 * \param buffer A buffer into which to write random data.
 * \param buffer_size Number of bytes to write into buffer.
 * \param error Set on error.
 * Extracts bytes from an RNG handle.
 * \returns FALSE on failure and sets \p error, TRUE on success.
 */
gboolean
mono_rand_try_get_bytes (gpointer *handle, guchar *buffer, gint buffer_size, MonoError *error)
{
	g_assert (buffer || !buffer_size);

	error_init (error);

	long const status = mono_rand_win_gen (buffer, buffer_size);
	if (status >= 0)
		return TRUE;
	mono_error_set_execution_engine (error, "Failed to gen random bytes (%ld)", status);
	*handle = 0;
	return FALSE;
}

/**
 * mono_rand_close:
 * \param handle An RNG handle.
 * Releases an RNG handle.
 */
void
mono_rand_close (gpointer handle)
{
}
#endif /* HOST_WIN32 */
