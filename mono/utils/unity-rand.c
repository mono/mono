/*
 * mono-rand-unity.c: 
 *
 */


#include <glib.h>
#include "mono-error.h"
#include <Cryptography-c-api.h>

/**
 * mono_rand_open:
 *
 * Returns: True if random source is global, false if mono_rand_init can be called repeatedly to get randomness instances.
 *
 * Initializes entire RNG system. Must be called once per process before calling mono_rand_init.
 */
gboolean mono_rand_open (void)
{
    return UnityPalOpenCryptographyProvider();
}

/**
 * mono_rand_init:
 * @seed: A string containing seed data
 * @seed_size: Length of seed string
 *
 * Returns: On success, a non-NULL handle which can be used to fetch random data from mono_rand_try_get_bytes. On failure, NULL.
 *
 * Initializes an RNG client.
 */
gpointer
mono_rand_init (guchar *seed, gint seed_size)
{
    return UnityPalGetCryptographyProvider();
}

/**
 * mono_rand_try_get_bytes:
 * @handle: A pointer to an RNG handle. Handle is set to NULL on failure.
 * @buffer: A buffer into which to write random data.
 * @buffer_size: Number of bytes to write into buffer.
 * @error: Set on error.
 *
 * Returns: FALSE on failure and sets @error, TRUE on success.
 *
 * Extracts bytes from an RNG handle.
 */
gboolean
mono_rand_try_get_bytes (gpointer *handle, guchar *buffer, gint buffer_size, MonoError *error)
{
    mono_error_init (error);
    return UnityPalCryptographyFillBufferWithRandomBytes(*handle, buffer_size, buffer);
}

/**
 * mono_rand_close:
 * @handle: An RNG handle.
 * @buffer: A buffer into which to write random data.
 * @buffer_size: Number of bytes to write into buffer.
 *
 * Releases an RNG handle.
 */
void
mono_rand_close (gpointer handle)
{
    UnityPalReleaseCryptographyProvider(handle);
}


/**
 * mono_rand_try_get_uint32:
 * @handle: A pointer to an RNG handle. Handle is set to NULL on failure.
 * @val: A pointer to a 32-bit unsigned int, to which the result will be written.
 * @min: Result will be greater than or equal to this value.
 * @max: Result will be less than or equal to this value.
 *
 * Returns: FALSE on failure, TRUE on success.
 *
 * Extracts one 32-bit unsigned int from an RNG handle.
 */
gboolean
mono_rand_try_get_uint32 (gpointer *handle, guint32 *val, guint32 min, guint32 max, MonoError *error)
{
    g_assert (val);
    if (!mono_rand_try_get_bytes (handle, (guchar*) val, sizeof (guint32), error))
        return FALSE;

    double randomDouble = ((gdouble) *val) / ( ((double)G_MAXUINT32) + 1 ); // Range is [0,1)
    *val = (guint32) (randomDouble * (max - min + 1) + min);

    g_assert (*val >= min);
    g_assert (*val <= max);

    return TRUE;
}
