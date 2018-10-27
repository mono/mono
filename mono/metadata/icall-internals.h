/**
 * \file
 * Copyright 2016 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#ifndef __MONO_METADATA_ICALL_INTERNALS_H__
#define __MONO_METADATA_ICALL_INTERNALS_H__

#include <config.h>
#include <glib.h>
#include <mono/metadata/object-internals.h>

void*
mono_lookup_internal_call_full (MonoMethod *method, gboolean warn_on_missing, mono_bool *uses_handles, mono_bool *foreign);

MONO_PAL_API void
mono_add_internal_call_with_flags (const char *name, const void* method, gboolean cooperative);

MONO_PROFILER_API void
mono_add_internal_call_internal (const char *name, gconstpointer method);

#ifdef __cplusplus

#include <type_traits>

template <typename T>
inline typename std::enable_if<std::is_function<T>::value ||
			       std::is_function<typename std::remove_pointer<T>::type>::value >::type
mono_add_internal_call_with_flags (const char *name, T method, gboolean cooperative)
{
	return mono_add_internal_call_with_flags (name, (const void*)method, cooperative);
}

template <typename T>
inline typename std::enable_if<std::is_function<T>::value ||
			       std::is_function<typename std::remove_pointer<T>::type>::value >::type
mono_add_internal_call_internal (const char *name, T method)
{
	return mono_add_internal_call_internal (name, (const void*)method);
}

#endif // __cplusplus

#endif /* __MONO_METADATA_ICALL_INTERNALS_H__ */
