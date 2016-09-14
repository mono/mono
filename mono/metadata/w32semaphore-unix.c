/*
 * w32semaphore-unix.c: Runtime support for managed Semaphore on Unix
 *
 * Author:
 *	Ludovic Henry (luhenry@microsoft.com)
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include "w32semaphore.h"

#include "w32handle-namespace.h"
#include "mono/io-layer/io-layer.h"
#include "mono/utils/mono-logger-internals.h"
#include "mono/utils/w32handle.h"

typedef struct {
	guint32 val;
	gint32 max;
} MonoW32HandleSemaphore;

struct MonoW32HandleNamedSemaphore {
	MonoW32HandleSemaphore s;
	MonoW32HandleNamespace sharedns;
};

static gboolean sem_handle_own (gpointer handle, MonoW32HandleType type)
{
	MonoW32HandleSemaphore *sem_handle;

	if (!mono_w32handle_lookup (handle, type, (gpointer *)&sem_handle)) {
		g_warning ("%s: error looking up %s handle %p",
			__func__, mono_w32handle_ops_typename (type), handle);
		return FALSE;
	}

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: owning %s handle %p",
		__func__, mono_w32handle_ops_typename (type), handle);

	sem_handle->val--;

	if (sem_handle->val == 0)
		mono_w32handle_set_signal_state (handle, FALSE, FALSE);

	return TRUE;
}

static void sema_signal(gpointer handle)
{
	ves_icall_System_Threading_Semaphore_ReleaseSemaphore_internal(handle, 1, NULL);
}

static gboolean sema_own (gpointer handle)
{
	return sem_handle_own (handle, MONO_W32HANDLE_SEM);
}

static void namedsema_signal (gpointer handle)
{
	ves_icall_System_Threading_Semaphore_ReleaseSemaphore_internal (handle, 1, NULL);
}

/* NB, always called with the shared handle lock held */
static gboolean namedsema_own (gpointer handle)
{
	return sem_handle_own (handle, MONO_W32HANDLE_NAMEDSEM);
}

static void sema_details (gpointer data)
{
	MonoW32HandleSemaphore *sem = (MonoW32HandleSemaphore *)data;
	g_print ("val: %5u, max: %5d", sem->val, sem->max);
}

static void namedsema_details (gpointer data)
{
	MonoW32HandleNamedSemaphore *namedsem = (MonoW32HandleNamedSemaphore *)data;
	g_print ("val: %5u, max: %5d, name: \"%s\"", namedsem->s.val, namedsem->s.max, namedsem->sharedns.name);
}

static const gchar* sema_typename (void)
{
	return "Semaphore";
}

static gsize sema_typesize (void)
{
	return sizeof (MonoW32HandleSemaphore);
}

static const gchar* namedsema_typename (void)
{
	return "N.Semaphore";
}

static gsize namedsema_typesize (void)
{
	return sizeof (MonoW32HandleNamedSemaphore);
}

void
mono_w32semaphore_init (void)
{
	static MonoW32HandleOps sem_ops = {
		NULL,			/* close */
		sema_signal,		/* signal */
		sema_own,		/* own */
		NULL,			/* is_owned */
		NULL,			/* special_wait */
		NULL,			/* prewait */
		sema_details,	/* details */
		sema_typename,	/* typename */
		sema_typesize,	/* typesize */
	};

	static MonoW32HandleOps namedsem_ops = {
		NULL,			/* close */
		namedsema_signal,	/* signal */
		namedsema_own,		/* own */
		NULL,			/* is_owned */
		NULL,			/* special_wait */
		NULL,			/* prewait */
		namedsema_details,	/* details */
		namedsema_typename,	/* typename */
		namedsema_typesize,	/* typesize */
	};

	mono_w32handle_register_ops (MONO_W32HANDLE_SEM,      &sem_ops);
	mono_w32handle_register_ops (MONO_W32HANDLE_NAMEDSEM, &namedsem_ops);

	mono_w32handle_register_capabilities (MONO_W32HANDLE_SEM,
		(MonoW32HandleCapability)(MONO_W32HANDLE_CAP_WAIT | MONO_W32HANDLE_CAP_SIGNAL));
	mono_w32handle_register_capabilities (MONO_W32HANDLE_NAMEDSEM,
		(MonoW32HandleCapability)(MONO_W32HANDLE_CAP_WAIT | MONO_W32HANDLE_CAP_SIGNAL));
}

static gpointer
sem_handle_create (MonoW32HandleSemaphore *sem_handle, MonoW32HandleType type, gint32 initial, gint32 max)
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
	MonoW32HandleSemaphore sem_handle;
	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: creating %s handle, initial %d max %d",
		__func__, mono_w32handle_ops_typename (MONO_W32HANDLE_SEM), initial, max);
	return sem_handle_create (&sem_handle, MONO_W32HANDLE_SEM, initial, max);
}

static gpointer
namedsem_create (gint32 initial, gint32 max, const gunichar2 *name)
{
	gpointer handle;
	gchar *utf8_name;

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: creating %s handle, initial %d max %d name \"%s\"",
		__func__, mono_w32handle_ops_typename (MONO_W32HANDLE_NAMEDSEM), initial, max, name);

	/* w32 seems to guarantee that opening named objects can't race each other */
	mono_w32handle_namespace_lock ();

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

		/* mono_w32handle_namespace_search_handle already adds a ref to the handle */
	} else {
		/* A new named semaphore */
		MonoW32HandleNamedSemaphore namedsem_handle;

		strncpy (&namedsem_handle.sharedns.name [0], utf8_name, MAX_PATH);
		namedsem_handle.sharedns.name [MAX_PATH] = '\0';

		handle = sem_handle_create ((MonoW32HandleSemaphore*) &namedsem_handle, MONO_W32HANDLE_NAMEDSEM, initial, max);
	}

	g_free (utf8_name);

	mono_w32handle_namespace_unlock ();

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
	MonoW32HandleType type;
	MonoW32HandleSemaphore *sem_handle;
	int thr_ret;
	MonoBoolean ret;

	if (!handle) {
		SetLastError (ERROR_INVALID_HANDLE);
		return FALSE;
	}

	switch (type = mono_w32handle_get_type (handle)) {
	case MONO_W32HANDLE_SEM:
	case MONO_W32HANDLE_NAMEDSEM:
		break;
	default:
		SetLastError (ERROR_INVALID_HANDLE);
		return FALSE;
	}

	if (!mono_w32handle_lookup (handle, type, (gpointer *)&sem_handle)) {
		g_warning ("%s: error looking up sem handle %p", __func__, handle);
		return FALSE;
	}

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: releasing %s handle %p",
		__func__, mono_w32handle_ops_typename (type), handle);

	thr_ret = mono_w32handle_lock_handle (handle);
	g_assert (thr_ret == 0);

	/* Do this before checking for count overflow, because overflowing
	 * max is a listed technique for finding the current value */
	if (prevcount)
		*prevcount = sem_handle->val;

	/* No idea why max is signed, but thats the spec :-( */
	if (sem_handle->val + releaseCount > (guint32)sem_handle->max) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: %s handle %p val %d count %d max %d, max value would be exceeded",
			__func__, mono_w32handle_ops_typename (type), handle, sem_handle->val, releaseCount, sem_handle->max);

		ret = FALSE;
	} else {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: %s handle %p val %d count %d max %d",
			__func__, mono_w32handle_ops_typename (type), handle, sem_handle->val, releaseCount, sem_handle->max);

		sem_handle->val += releaseCount;
		mono_w32handle_set_signal_state (handle, TRUE, TRUE);

		ret = TRUE;
	}

	thr_ret = mono_w32handle_unlock_handle (handle);
	g_assert (thr_ret == 0);

	return ret;
}

gpointer
ves_icall_System_Threading_Semaphore_OpenSemaphore_internal (MonoString *name, gint32 rights, gint32 *error)
{
	gpointer handle;
	gchar *utf8_name;

	*error = ERROR_SUCCESS;

	/* w32 seems to guarantee that opening named objects can't race each other */
	mono_w32handle_namespace_lock ();

	utf8_name = g_utf16_to_utf8 (mono_string_chars (name), -1, NULL, NULL, NULL);

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Opening named sem [%s]", __func__, utf8_name);

	handle = mono_w32handle_namespace_search_handle (MONO_W32HANDLE_NAMEDSEM, utf8_name);
	if (handle == INVALID_HANDLE_VALUE) {
		/* The name has already been used for a different object. */
		*error = ERROR_INVALID_HANDLE;
		goto cleanup;
	} else if (!handle) {
		/* This name doesn't exist */
		*error = ERROR_FILE_NOT_FOUND;
		goto cleanup;
	}

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: returning named sem handle %p", __func__, handle);

cleanup:
	g_free (utf8_name);

	mono_w32handle_namespace_unlock ();

	return handle;
}

MonoW32HandleNamespace*
mono_w32semaphore_get_namespace (MonoW32HandleNamedSemaphore *semaphore)
{
	return &semaphore->sharedns;
}
