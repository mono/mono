/**
 * \file
 * Contains inline functions to explicitly mark data races that should not be changed.
 * This way, instruments like Clang's ThreadSanitizer can be told to ignore very specific instructions.
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#ifndef _RACING_H_
#define _RACING_H_

#include <glib.h>
#include <mono/utils/mono-compiler.h>

#if defined(__has_feature)
#if __has_feature(thread_sanitizer)
#define MONO_RACING_ATTRS MONO_NO_SANITIZE_THREAD static
#else
#define MONO_RACING_ATTRS MONO_ALWAYS_INLINE static inline
#endif
#else
#define MONO_RACING_ATTRS MONO_ALWAYS_INLINE static inline
#endif

MONO_RACING_ATTRS
gint32
RacingIncrement (gint32 *val)
{
	return ++*val;
}

MONO_RACING_ATTRS
gint64
RacingIncrement64 (gint64 *val)
{
	return ++*val;
}

MONO_RACING_ATTRS
gsize
RacingIncrementSize (gsize *val)
{
	return ++*val;
}

#endif /* _RACING_H_ */
