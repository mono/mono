/*
 * w32mutex-unix.c: Runtime support for managed Mutex on Unix
 *
 * Author:
 *	Ludovic Henry (luhenry@microsoft.com)
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include "w32mutex.h"

#include "mono/io-layer/io-layer.h"
#include "mono/io-layer/mutex-private.h"
#include "mono/utils/mono-logger-internals.h"

static gpointer mutex_handle_create (struct _WapiHandle_mutex *mutex_handle, MonoW32HandleType type, gboolean owned)
{
	gpointer handle;
	int thr_ret;

	mutex_handle->tid = 0;
	mutex_handle->recursion = 0;

	handle = mono_w32handle_new (type, mutex_handle);
	if (handle == INVALID_HANDLE_VALUE) {
		g_warning ("%s: error creating %s handle",
			__func__, mono_w32handle_ops_typename (type));
		SetLastError (ERROR_GEN_FAILURE);
		return NULL;
	}

	thr_ret = mono_w32handle_lock_handle (handle);
	g_assert (thr_ret == 0);

	if (owned)
		mono_w32handle_ops_own (handle);
	else
		mono_w32handle_set_signal_state (handle, TRUE, FALSE);

	thr_ret = mono_w32handle_unlock_handle (handle);
	g_assert (thr_ret == 0);

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: created %s handle %p",
		__func__, mono_w32handle_ops_typename (type), handle);

	return handle;
}

static gpointer mutex_create (gboolean owned)
{
	struct _WapiHandle_mutex mutex_handle;
	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: creating %s handle",
		__func__, mono_w32handle_ops_typename (MONO_W32HANDLE_MUTEX));
	return mutex_handle_create (&mutex_handle, MONO_W32HANDLE_MUTEX, owned);
}

static gpointer namedmutex_create (gboolean owned, const gunichar2 *name)
{
	gpointer handle;
	gchar *utf8_name;
	int thr_ret;

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: creating %s handle",
		__func__, mono_w32handle_ops_typename (MONO_W32HANDLE_NAMEDMUTEX));

	/* w32 seems to guarantee that opening named objects can't race each other */
	thr_ret = wapi_namespace_lock ();
	g_assert (thr_ret == 0);

	utf8_name = g_utf16_to_utf8 (name, -1, NULL, NULL, NULL);

	handle = wapi_search_handle_namespace (MONO_W32HANDLE_NAMEDMUTEX, utf8_name);
	if (handle == INVALID_HANDLE_VALUE) {
		/* The name has already been used for a different object. */
		handle = NULL;
		SetLastError (ERROR_INVALID_HANDLE);
	} else if (handle) {
		/* Not an error, but this is how the caller is informed that the mutex wasn't freshly created */
		SetLastError (ERROR_ALREADY_EXISTS);

		/* this is used as creating a new handle */
		mono_w32handle_ref (handle);
	} else {
		/* A new named mutex */
		struct _WapiHandle_namedmutex namedmutex_handle;

		strncpy (&namedmutex_handle.sharedns.name [0], utf8_name, MAX_PATH);
		namedmutex_handle.sharedns.name [MAX_PATH] = '\0';

		handle = mutex_handle_create ((struct _WapiHandle_mutex*) &namedmutex_handle, MONO_W32HANDLE_NAMEDMUTEX, owned);
	}

	g_free (utf8_name);

	thr_ret = wapi_namespace_unlock (NULL);
	g_assert (thr_ret == 0);

	return handle;
}

gpointer
ves_icall_System_Threading_Mutex_CreateMutex_internal (MonoBoolean owned, MonoString *name, MonoBoolean *created)
{
	gpointer mutex;

	*created = TRUE;

	/* Need to blow away any old errors here, because code tests
	 * for ERROR_ALREADY_EXISTS on success (!) to see if a mutex
	 * was freshly created */
	SetLastError (ERROR_SUCCESS);

	if (!name) {
		mutex = mutex_create (owned);
	} else {
		mutex = namedmutex_create (owned, mono_string_chars (name));

		if (GetLastError () == ERROR_ALREADY_EXISTS)
			*created = FALSE;
	}

	return mutex;
}

MonoBoolean
ves_icall_System_Threading_Mutex_ReleaseMutex_internal (gpointer handle)
{
	return ReleaseMutex (handle);
}

gpointer
ves_icall_System_Threading_Mutex_OpenMutex_internal (MonoString *name, gint32 rights, gint32 *error)
{
	HANDLE ret;

	*error = ERROR_SUCCESS;

	ret = OpenMutex (rights, FALSE, mono_string_chars (name));
	if (!ret)
		*error = GetLastError ();

	return ret;
}
