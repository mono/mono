/*
 * w32semaphore-unix.c: Runtime support for managed Semaphore on Unix
 *
 * Author:
 *	Ludovic Henry (luhenry@microsoft.com)
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include "w32semaphore.h"

#include "mono/io-layer/io-layer.h"
#include "mono/utils/mono-logger-internals.h"
#include "mono/utils/w32handle.h"

static gpointer
sem_handle_create (struct _WapiHandle_sem *sem_handle, MonoW32HandleType type, gint32 initial, gint32 max)
{
	gpointer handle;
	int thr_ret;

	sem_handle->val = initial;
	sem_handle->max = max;

	handle = mono_w32handle_new (type, sem_handle);
	if (handle == INVALID_HANDLE_VALUE) {
		g_warning ("%s: error creating %s handle",
			__func__, mono_w32handle_ops_typename (type));
		SetLastError (ERROR_GEN_FAILURE);
		return NULL;
	}

	thr_ret = mono_w32handle_lock_handle (handle);
	g_assert (thr_ret == 0);

	if (initial != 0)
		mono_w32handle_set_signal_state (handle, TRUE, FALSE);

	thr_ret = mono_w32handle_unlock_handle (handle);
	g_assert (thr_ret == 0);

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: created %s handle %p",
		__func__, mono_w32handle_ops_typename (type), handle);

	return handle;
}

static gpointer
sem_create (gint32 initial, gint32 max)
{
	struct _WapiHandle_sem sem_handle;
	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: creating %s handle, initial %d max %d",
		__func__, mono_w32handle_ops_typename (MONO_W32HANDLE_SEM), initial, max);
	return sem_handle_create (&sem_handle, MONO_W32HANDLE_SEM, initial, max);
}

static gpointer
namedsem_create (gint32 initial, gint32 max, const gunichar2 *name)
{
	gpointer handle;
	gchar *utf8_name;
	int thr_ret;

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: creating %s handle, initial %d max %d name \"%s\"",
		__func__, mono_w32handle_ops_typename (MONO_W32HANDLE_NAMEDSEM), initial, max, name);

	/* w32 seems to guarantee that opening named objects can't race each other */
	thr_ret = wapi_namespace_lock ();
	g_assert (thr_ret == 0);

	utf8_name = g_utf16_to_utf8 (name, -1, NULL, NULL, NULL);

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Creating named sem name [%s] initial %d max %d", __func__, utf8_name, initial, max);

	handle = mono_w32handle_namespace_search_handle (MONO_W32HANDLE_NAMEDSEM, utf8_name);
	if (handle == INVALID_HANDLE_VALUE) {
		/* The name has already been used for a different object. */
		handle = NULL;
		SetLastError (ERROR_INVALID_HANDLE);
	} else if (handle) {
		/* Not an error, but this is how the caller is informed that the semaphore wasn't freshly created */
		SetLastError (ERROR_ALREADY_EXISTS);

		/* this is used as creating a new handle */
		mono_w32handle_ref (handle);
	} else {
		/* A new named semaphore */
		struct _WapiHandle_namedsem namedsem_handle;

		strncpy (&namedsem_handle.sharedns.name [0], utf8_name, MAX_PATH);
		namedsem_handle.sharedns.name [MAX_PATH] = '\0';

		handle = sem_handle_create ((struct _WapiHandle_sem*) &namedsem_handle, MONO_W32HANDLE_NAMEDSEM, initial, max);
	}

	g_free (utf8_name);

	thr_ret = wapi_namespace_unlock (NULL);
	g_assert (thr_ret == 0);

	return handle;
}

gpointer
ves_icall_System_Threading_Semaphore_CreateSemaphore_internal (gint32 initialCount, gint32 maximumCount, MonoString *name, gint32 *error)
{ 
	gpointer sem;

	if (maximumCount <= 0) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: maximumCount <= 0", __func__);

		*error = ERROR_INVALID_PARAMETER;
		return NULL;
	}

	if (initialCount > maximumCount || initialCount < 0) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: initialCount > maximumCount or < 0", __func__);

		*error = ERROR_INVALID_PARAMETER;
		return NULL;
	}

	/* Need to blow away any old errors here, because code tests
	 * for ERROR_ALREADY_EXISTS on success (!) to see if a
	 * semaphore was freshly created
	 */
	SetLastError (ERROR_SUCCESS);

	if (!name)
		sem = sem_create (initialCount, maximumCount);
	else
		sem = namedsem_create (initialCount, maximumCount, mono_string_chars (name));

	*error = GetLastError ();

	return sem;
}

MonoBoolean
ves_icall_System_Threading_Semaphore_ReleaseSemaphore_internal (gpointer handle, gint32 releaseCount, gint32 *prevcount)
{ 
	return ReleaseSemaphore (handle, releaseCount, prevcount);
}

gpointer
ves_icall_System_Threading_Semaphore_OpenSemaphore_internal (MonoString *name, gint32 rights, gint32 *error)
{
	gpointer sem;

	sem = OpenSemaphore (rights, FALSE, mono_string_chars (name));

	*error = GetLastError ();

	return sem;
}