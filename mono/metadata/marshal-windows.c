/**
 * \file
 * Windows marshal support.
 *
 * Copyright 2016 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#include <config.h>
#include <glib.h>

#if defined(HOST_WIN32)
#include <winsock2.h>
#include <windows.h>
#include <objbase.h>
#include "mono/metadata/marshal-windows-internals.h"

#if G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT)
void*
mono_marshal_alloc_hglobal (size_t size)
{
	return GlobalAlloc (GMEM_FIXED, size);
}

gpointer
mono_marshal_realloc_hglobal (gpointer ptr, size_t size)
{
	return GlobalReAlloc (ptr, size, GMEM_MOVEABLE);
}

void
mono_marshal_free_hglobal (gpointer ptr)
{
	GlobalFree (ptr);
}
#endif /* G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT) */

void*
mono_marshal_alloc_co_task_mem (size_t size)
{
	return CoTaskMemAlloc (size);
}

void
mono_marshal_free_co_task_mem (void *ptr)
{
	CoTaskMemFree (ptr);
}

gpointer
mono_marshal_realloc_co_task_mem (gpointer ptr, size_t size)
{
	return CoTaskMemRealloc (ptr, size);
}

char*
ves_icall_System_Runtime_InteropServices_Marshal_StringToHGlobalAnsi (const gunichar2 *s, int length, MonoError *error)
{
	// FIXME pass mono_utf16_to_utf8 an allocator to avoid double alloc/copy.

	char* tres = mono_utf16_to_utf8 (s, length, error);
	return_val_if_nok (error, NULL);
	if (!tres)
		return tres;

	/*
	 * mono_utf16_to_utf8() returns a memory area at least as large as length,
	 * even if it contains NULL characters. The copy we allocate here has to be equally
	 * large.
	 */
	size_t len = MAX (strlen (tres) + 1, length);
	char* ret = ves_icall_System_Runtime_InteropServices_Marshal_AllocHGlobal (len, error);
	if (ret)
		memcpy (ret, tres, len);
	g_free (tres);
	return ret;
}

gunichar2*
ves_icall_System_Runtime_InteropServices_Marshal_StringToHGlobalUni (const gunichar2 *s, int length, MonoError *error)
{
	if (!s)
		return NULL;

	gsize const len = ((gsize)length + 1) * 2;
	gunichar2 *res = ves_icall_System_Runtime_InteropServices_Marshal_AllocHGlobal (len, error);
	if (res) {
		memcpy (res, s, length * 2);
		res [length] = 0;
	}
	return res;
}

gpointer
mono_string_to_utf8str_handle (MonoStringHandle s, MonoError *error)
{
	char *as, *tmp;
	glong len;
	GError *gerror = NULL;

	if (MONO_HANDLE_IS_NULL (s))
		return NULL;

	if (!mono_string_handle_length (s)) {
		as = CoTaskMemAlloc (1);
		g_assert (as);
		as [0] = '\0';
		return as;
	}

	// FIXME pass g_utf16_to_utf8 an allocator to avoid double alloc/copy.

	uint32_t gchandle = 0;
	tmp = g_utf16_to_utf8 (mono_string_handle_pin_chars (s, &gchandle), mono_string_handle_length (s), NULL, &len, &gerror);
	mono_gchandle_free (gchandle);
	if (gerror) {
		mono_error_set_argument (error, "string", gerror->message);
		g_error_free (gerror);
		return NULL;
	} else {
		as = CoTaskMemAlloc (len + 1);
		g_assert (as);
		memcpy (as, tmp, len + 1);
		g_free (tmp);
		return as;
	}
}

/* This is a JIT icall, it sets the pending exception and returns NULL on error. */
gpointer
mono_string_to_utf8str (MonoString *s_raw)
{
	HANDLE_FUNCTION_ENTER ();
	ERROR_DECL (error);
	MONO_HANDLE_DCL (MonoString, s);
	gpointer result = mono_string_to_utf8str_handle (s, error);
	mono_error_set_pending_exception (error);
	HANDLE_FUNCTION_RETURN_VAL (result);
}

#endif /* HOST_WIN32 */
