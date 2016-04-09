/*
 * wait.c:  wait for handles to become signalled
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002-2006 Novell, Inc.
 */

#include <config.h>
#include <glib.h>
#include <string.h>
#include <errno.h>

#include <mono/io-layer/wapi.h>
#include <mono/io-layer/handles-private.h>
#include <mono/io-layer/wapi-private.h>
#include <mono/io-layer/io-trace.h>
#include <mono/utils/mono-logger-internals.h>
#include <mono/utils/mono-time.h>

static gboolean own_if_signalled(gpointer handle)
{
	gboolean ret = FALSE;
	
	if (_WAPI_SHARED_HANDLE (_wapi_handle_type (handle))) {
		if (_wapi_handle_trylock_shared_handles () == EBUSY) {
			return (FALSE);
		}
	}
	
	if (_wapi_handle_issignalled (handle)) {
		_wapi_handle_ops_own (handle);
		ret = TRUE;
	}

	if (_WAPI_SHARED_HANDLE (_wapi_handle_type (handle))) {
		_wapi_handle_unlock_shared_handles ();
	}

	return(ret);
}

static gboolean own_if_owned(gpointer handle)
{
	gboolean ret = FALSE;
	
	if (_WAPI_SHARED_HANDLE (_wapi_handle_type (handle))) {
		if (_wapi_handle_trylock_shared_handles () == EBUSY) {
			return (FALSE);
		}
	}
	
	if (_wapi_handle_ops_isowned (handle)) {
		_wapi_handle_ops_own (handle);
		ret = TRUE;
	}

	if (_WAPI_SHARED_HANDLE (_wapi_handle_type (handle))) {
		_wapi_handle_unlock_shared_handles ();
	}

	return(ret);
}

/**
 * WaitForSingleObjectEx:
 * @handle: an object to wait for
 * @timeout: the maximum time in milliseconds to wait for
 * @alertable: if TRUE, the wait can be interrupted by an APC call
 *
 * This function returns when either @handle is signalled, or @timeout
 * ms elapses.  If @timeout is zero, the object's state is tested and
 * the function returns immediately.  If @timeout is %INFINITE, the
 * function waits forever.
 *
 * Return value: %WAIT_ABANDONED - @handle is a mutex that was not
 * released by the owning thread when it exited.  Ownership of the
 * mutex object is granted to the calling thread and the mutex is set
 * to nonsignalled.  %WAIT_OBJECT_0 - The state of @handle is
 * signalled.  %WAIT_TIMEOUT - The @timeout interval elapsed and
 * @handle's state is still not signalled.  %WAIT_FAILED - an error
 * occurred. %WAIT_IO_COMPLETION - the wait was ended by an APC.
 */
guint32 WaitForSingleObjectEx(gpointer handle, guint32 timeout,
			      gboolean alertable)
{
	guint32 ret, waited;
	int thr_ret;
	gboolean apc_pending = FALSE;
	gpointer current_thread = wapi_get_current_thread_handle ();
	gint64 now, end;
	
	if (current_thread == NULL) {
		SetLastError (ERROR_INVALID_HANDLE);
		return(WAIT_FAILED);
	}

	if (handle == _WAPI_THREAD_CURRENT) {
		handle = wapi_get_current_thread_handle ();
		if (handle == NULL) {
			SetLastError (ERROR_INVALID_HANDLE);
			return(WAIT_FAILED);
		}
	}

	if ((GPOINTER_TO_UINT (handle) & _WAPI_PROCESS_UNHANDLED) == _WAPI_PROCESS_UNHANDLED) {
		SetLastError (ERROR_INVALID_HANDLE);
		return(WAIT_FAILED);
	}
	
	if (_wapi_handle_test_capabilities (handle,
					    WAPI_HANDLE_CAP_WAIT) == FALSE) {
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: handle %p can't be waited for", __func__,
			   handle);

		return(WAIT_FAILED);
	}

	_wapi_handle_ops_prewait (handle);
	
	if (_wapi_handle_test_capabilities (handle, WAPI_HANDLE_CAP_SPECIAL_WAIT) == TRUE) {
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: handle %p has special wait", __func__, handle);

		ret = _wapi_handle_ops_special_wait (handle, timeout, alertable);
	
		if (alertable && _wapi_thread_cur_apc_pending ())
			ret = WAIT_IO_COMPLETION;

		return ret;
	}
	
	
	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: locking handle %p", __func__, handle);

	thr_ret = _wapi_handle_lock_handle (handle);
	g_assert (thr_ret == 0);

	if (_wapi_handle_test_capabilities (handle,
					    WAPI_HANDLE_CAP_OWN) == TRUE) {
		if (own_if_owned (handle) == TRUE) {
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: handle %p already owned", __func__,
				   handle);
			ret = WAIT_OBJECT_0;
			goto done;
		}
	}

	if (own_if_signalled (handle) == TRUE) {
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: handle %p already signalled", __func__,
			   handle);

		ret=WAIT_OBJECT_0;
		goto done;
	}

	if (timeout == 0) {
		ret = WAIT_TIMEOUT;
		goto done;
	}
	
	if (timeout != INFINITE)
		end = mono_100ns_ticks () + timeout * 1000 * 10;

	do {
		/* Check before waiting on the condition, just in case
		 */
		_wapi_handle_ops_prewait (handle);

		if (own_if_signalled (handle)) {
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: handle %p signalled", __func__,
				   handle);

			ret = WAIT_OBJECT_0;
			goto done;
		}

		if (timeout == INFINITE) {
			waited = _wapi_handle_timedwait_signal_handle (handle, INFINITE, alertable, FALSE, &apc_pending);
		} else {
			now = mono_100ns_ticks ();
			if (end < now) {
				ret = WAIT_TIMEOUT;
				goto done;
			}

			waited = _wapi_handle_timedwait_signal_handle (handle, (end - now) / 10 / 1000, alertable, FALSE, &apc_pending);
		}

		if(waited==0 && !apc_pending) {
			/* Condition was signalled, so hopefully
			 * handle is signalled now.  (It might not be
			 * if someone else got in before us.)
			 */
			if (own_if_signalled (handle)) {
				MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: handle %p signalled", __func__,
					   handle);

				ret=WAIT_OBJECT_0;
				goto done;
			}
		
			/* Better luck next time */
		}
	} while(waited == 0 && !apc_pending);

	/* Timeout or other error */
	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: wait on handle %p error: %s", __func__, handle,
		   strerror (waited));

	ret = apc_pending ? WAIT_IO_COMPLETION : WAIT_TIMEOUT;

done:

	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: unlocking handle %p", __func__, handle);
	
	thr_ret = _wapi_handle_unlock_handle (handle);
	g_assert (thr_ret == 0);

	return(ret);
}

guint32 WaitForSingleObject(gpointer handle, guint32 timeout)
{
	return WaitForSingleObjectEx (handle, timeout, FALSE);
}


/**
 * SignalObjectAndWait:
 * @signal_handle: An object to signal
 * @wait: An object to wait for
 * @timeout: The maximum time in milliseconds to wait for
 * @alertable: Specifies whether the function returnes when the system
 * queues an I/O completion routine or an APC for the calling thread.
 *
 * Atomically signals @signal and waits for @wait to become signalled,
 * or @timeout ms elapses.  If @timeout is zero, the object's state is
 * tested and the function returns immediately.  If @timeout is
 * %INFINITE, the function waits forever.
 *
 * @signal can be a semaphore, mutex or event object.
 *
 * If @alertable is %TRUE and the system queues an I/O completion
 * routine or an APC for the calling thread, the function returns and
 * the thread calls the completion routine or APC function.  If
 * %FALSE, the function does not return, and the thread does not call
 * the completion routine or APC function.  A completion routine is
 * queued when the ReadFileEx() or WriteFileEx() function in which it
 * was specified has completed.  The calling thread is the thread that
 * initiated the read or write operation.  An APC is queued when
 * QueueUserAPC() is called.  Currently completion routines and APC
 * functions are not supported.
 *
 * Return value: %WAIT_ABANDONED - @wait is a mutex that was not
 * released by the owning thread when it exited.  Ownershop of the
 * mutex object is granted to the calling thread and the mutex is set
 * to nonsignalled.  %WAIT_IO_COMPLETION - the wait was ended by one
 * or more user-mode asynchronous procedure calls queued to the
 * thread.  %WAIT_OBJECT_0 - The state of @wait is signalled.
 * %WAIT_TIMEOUT - The @timeout interval elapsed and @wait's state is
 * still not signalled.  %WAIT_FAILED - an error occurred.
 */
guint32 SignalObjectAndWait(gpointer signal_handle, gpointer wait,
			    guint32 timeout, gboolean alertable)
{
	guint32 ret = 0, waited;
	int thr_ret;
	gboolean apc_pending = FALSE;
	gpointer current_thread = wapi_get_current_thread_handle ();
	gint64 now, end;
	
	if (current_thread == NULL) {
		SetLastError (ERROR_INVALID_HANDLE);
		return(WAIT_FAILED);
	}

	if (signal_handle == _WAPI_THREAD_CURRENT) {
		signal_handle = wapi_get_current_thread_handle ();
		if (signal_handle == NULL) {
			SetLastError (ERROR_INVALID_HANDLE);
			return(WAIT_FAILED);
		}
	}

	if (wait == _WAPI_THREAD_CURRENT) {
		wait = wapi_get_current_thread_handle ();
		if (wait == NULL) {
			SetLastError (ERROR_INVALID_HANDLE);
			return(WAIT_FAILED);
		}
	}

	if ((GPOINTER_TO_UINT (signal_handle) & _WAPI_PROCESS_UNHANDLED) == _WAPI_PROCESS_UNHANDLED) {
		SetLastError (ERROR_INVALID_HANDLE);
		return(WAIT_FAILED);
	}

	if ((GPOINTER_TO_UINT (wait) & _WAPI_PROCESS_UNHANDLED) == _WAPI_PROCESS_UNHANDLED) {
		SetLastError (ERROR_INVALID_HANDLE);
		return(WAIT_FAILED);
	}
	
	if (_wapi_handle_test_capabilities (signal_handle,
					    WAPI_HANDLE_CAP_SIGNAL)==FALSE) {
		return(WAIT_FAILED);
	}
	
	if (_wapi_handle_test_capabilities (wait,
					    WAPI_HANDLE_CAP_WAIT)==FALSE) {
		return(WAIT_FAILED);
	}

	_wapi_handle_ops_prewait (wait);
	
	if (_wapi_handle_test_capabilities (wait, WAPI_HANDLE_CAP_SPECIAL_WAIT) == TRUE) {
		g_warning ("%s: handle %p has special wait, implement me!!",
			   __func__, wait);

		return (WAIT_FAILED);
	}

	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: locking handle %p", __func__, wait);

	thr_ret = _wapi_handle_lock_handle (wait);
	g_assert (thr_ret == 0);

	_wapi_handle_ops_signal (signal_handle);

	if (_wapi_handle_test_capabilities (wait, WAPI_HANDLE_CAP_OWN)==TRUE) {
		if (own_if_owned (wait)) {
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: handle %p already owned", __func__,
				   wait);
			ret = WAIT_OBJECT_0;
			goto done;
		}
	}

	if (own_if_signalled (wait)) {
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: handle %p already signalled", __func__, wait);

		ret = WAIT_OBJECT_0;
		goto done;
	}

	if (timeout != INFINITE)
		end = mono_100ns_ticks () + timeout * 1000 * 10;

	do {
		/* Check before waiting on the condition, just in case
		 */
		_wapi_handle_ops_prewait (wait);
	
		if (own_if_signalled (wait)) {
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: handle %p signalled", __func__, wait);

			ret = WAIT_OBJECT_0;
			goto done;
		}

		if (timeout == INFINITE) {
			waited = _wapi_handle_timedwait_signal_handle (wait, INFINITE, alertable, FALSE, &apc_pending);
		} else {
			now = mono_100ns_ticks ();
			if (end < now) {
				ret = WAIT_TIMEOUT;
				goto done;
			}

			waited = _wapi_handle_timedwait_signal_handle (wait, (end - now) / 10 / 1000, alertable, FALSE, &apc_pending);
		}

		if (waited==0 && !apc_pending) {
			/* Condition was signalled, so hopefully
			 * handle is signalled now.  (It might not be
			 * if someone else got in before us.)
			 */
			if (own_if_signalled (wait)) {
				MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: handle %p signalled", __func__,
					   wait);

				ret = WAIT_OBJECT_0;
				goto done;
			}
		
			/* Better luck next time */
		}
	} while(waited == 0 && !apc_pending);

	/* Timeout or other error */
	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: wait on handle %p error: %s", __func__, wait, strerror (ret));

	ret = apc_pending ? WAIT_IO_COMPLETION : WAIT_TIMEOUT;

done:

	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: unlocking handle %p", __func__, wait);

	thr_ret = _wapi_handle_unlock_handle (wait);
	g_assert (thr_ret == 0);

	return(ret);
}

static gboolean test_and_own (guint32 numobjects, gpointer *handles,
			      gboolean waitall, guint32 *count,
			      guint32 *lowest)
{
	gboolean done;
	int i;
	
	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: locking handles", __func__);
	
	done = _wapi_handle_count_signalled_handles (numobjects, handles,
						     waitall, count, lowest);
	if (done == TRUE) {
		if (waitall == TRUE) {
			for (i = 0; i < numobjects; i++) {
				own_if_signalled (handles[i]);
			}
		} else {
			own_if_signalled (handles[*lowest]);
		}
	}
	
	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: unlocking handles", __func__);

	_wapi_handle_unlock_handles (numobjects, handles);

	return(done);
}

/**
 * WaitForMultipleObjectsEx:
 * @numobjects: The number of objects in @handles. The maximum allowed
 * is %MAXIMUM_WAIT_OBJECTS.
 * @handles: An array of object handles.  Duplicates are not allowed.
 * @waitall: If %TRUE, this function waits until all of the handles
 * are signalled.  If %FALSE, this function returns when any object is
 * signalled.
 * @timeout: The maximum time in milliseconds to wait for.
 * @alertable: if TRUE, the wait can be interrupted by an APC call
 * 
 * This function returns when either one or more of @handles is
 * signalled, or @timeout ms elapses.  If @timeout is zero, the state
 * of each item of @handles is tested and the function returns
 * immediately.  If @timeout is %INFINITE, the function waits forever.
 *
 * Return value: %WAIT_OBJECT_0 to %WAIT_OBJECT_0 + @numobjects - 1 -
 * if @waitall is %TRUE, indicates that all objects are signalled.  If
 * @waitall is %FALSE, the return value minus %WAIT_OBJECT_0 indicates
 * the first index into @handles of the objects that are signalled.
 * %WAIT_ABANDONED_0 to %WAIT_ABANDONED_0 + @numobjects - 1 - if
 * @waitall is %TRUE, indicates that all objects are signalled, and at
 * least one object is an abandoned mutex object (See
 * WaitForSingleObject() for a description of abandoned mutexes.)  If
 * @waitall is %FALSE, the return value minus %WAIT_ABANDONED_0
 * indicates the first index into @handles of an abandoned mutex.
 * %WAIT_TIMEOUT - The @timeout interval elapsed and no objects in
 * @handles are signalled.  %WAIT_FAILED - an error occurred.
 * %WAIT_IO_COMPLETION - the wait was ended by an APC.
 */
guint32 WaitForMultipleObjectsEx(guint32 numobjects, gpointer *handles,
				 gboolean waitall, guint32 timeout,
				 gboolean alertable)
{
	gboolean duplicate = FALSE, bogustype = FALSE, done;
	guint32 count, lowest;
	guint i;
	guint32 ret;
	int thr_ret;
	gpointer current_thread = wapi_get_current_thread_handle ();
	guint32 retval;
	gboolean poll;
	gpointer sorted_handles [MAXIMUM_WAIT_OBJECTS];
	gboolean apc_pending = FALSE;
	gint64 now, end;
	
	if (current_thread == NULL) {
		SetLastError (ERROR_INVALID_HANDLE);
		return(WAIT_FAILED);
	}
	
	if (numobjects > MAXIMUM_WAIT_OBJECTS) {
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Too many handles: %d", __func__, numobjects);

		return(WAIT_FAILED);
	}
	
	if (numobjects == 1) {
		return WaitForSingleObjectEx (handles [0], timeout, alertable);
	}

	/* Check for duplicates */
	for (i = 0; i < numobjects; i++) {
		if (handles[i] == _WAPI_THREAD_CURRENT) {
			handles[i] = wapi_get_current_thread_handle ();
			
			if (handles[i] == NULL) {
				MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Handle %d bogus", __func__, i);

				bogustype = TRUE;
				break;
			}
		}

		if ((GPOINTER_TO_UINT (handles[i]) & _WAPI_PROCESS_UNHANDLED) == _WAPI_PROCESS_UNHANDLED) {
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Handle %d pseudo process", __func__,
				   i);

			bogustype = TRUE;
			break;
		}

		if (_wapi_handle_test_capabilities (handles[i], WAPI_HANDLE_CAP_WAIT) == FALSE) {
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Handle %p can't be waited for",
				   __func__, handles[i]);

			bogustype = TRUE;
			break;
		}

		sorted_handles [i] = handles [i];
		_wapi_handle_ops_prewait (handles[i]);
	}

	qsort (sorted_handles, numobjects, sizeof (gpointer), g_direct_equal);
	for (i = 1; i < numobjects; i++) {
		if (sorted_handles [i - 1] == sorted_handles [i]) {
			duplicate = TRUE;
			break;
		}
	}

	if (duplicate == TRUE) {
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Returning due to duplicates", __func__);

		return(WAIT_FAILED);
	}

	if (bogustype == TRUE) {
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Returning due to bogus type", __func__);

		return(WAIT_FAILED);
	}

	poll = FALSE;
	for (i = 0; i < numobjects; ++i)
		if (_wapi_handle_type (handles [i]) == WAPI_HANDLE_PROCESS || _WAPI_SHARED_HANDLE (_wapi_handle_type (handles[i]))) 
			/* Can't wait for a process handle + another handle without polling */
			poll = TRUE;

	done = test_and_own (numobjects, handles, waitall, &count, &lowest);
	if (done == TRUE) {
		return(WAIT_OBJECT_0+lowest);
	}
	
	if (timeout == 0) {
		return WAIT_TIMEOUT;
	}

	if (timeout != INFINITE)
		end = mono_100ns_ticks () + timeout * 1000 * 10;

	/* Have to wait for some or all handles to become signalled
	 */

	for (i = 0; i < numobjects; i++) {
		/* Add a reference, as we need to ensure the handle wont
		 * disappear from under us while we're waiting in the loop
		 * (not lock, as we don't want exclusive access here)
		 */
		_wapi_handle_ref (handles[i]);
	}

	while(1) {
		/* Prod all handles with prewait methods and
		 * special-wait handles that aren't already signalled
		 */
		for (i = 0; i < numobjects; i++) {
			_wapi_handle_ops_prewait (handles[i]);
		
			if (_wapi_handle_test_capabilities (handles[i], WAPI_HANDLE_CAP_SPECIAL_WAIT) == TRUE && _wapi_handle_issignalled (handles[i]) == FALSE) {
				_wapi_handle_ops_special_wait (handles[i], 0, alertable);
			}
		}
		
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: locking signal mutex", __func__);

		thr_ret = _wapi_handle_lock_signal_mutex ();
		g_assert (thr_ret == 0);

		/* Check the signalled state of handles inside the critical section */
		if (waitall) {
			done = TRUE;
			for (i = 0; i < numobjects; i++)
				if (!_wapi_handle_issignalled (handles [i]))
					done = FALSE;
		} else {
			done = FALSE;
			for (i = 0; i < numobjects; i++)
				if (_wapi_handle_issignalled (handles [i]))
					done = TRUE;
		}
		
		if (!done) {
			/* Enter the wait */
			if (timeout == INFINITE) {
				ret = _wapi_handle_timedwait_signal (INFINITE, poll, &apc_pending);
			} else {
				now = mono_100ns_ticks ();
				if (end < now) {
					ret = WAIT_TIMEOUT;
				} else {
					ret = _wapi_handle_timedwait_signal ((end - now) / 10 / 1000, poll, &apc_pending);
				}
			}
		} else {
			/* No need to wait */
			ret = 0;
		}

		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: unlocking signal mutex", __func__);

		thr_ret = _wapi_handle_unlock_signal_mutex (NULL);
		g_assert (thr_ret == 0);
		
		if (alertable && apc_pending) {
			retval = WAIT_IO_COMPLETION;
			break;
		}
	
		/* Check if everything is signalled, as we can't
		 * guarantee to notice a shared signal even if the
		 * wait timed out
		 */
		done = test_and_own (numobjects, handles, waitall,
				     &count, &lowest);
		if (done == TRUE) {
			retval = WAIT_OBJECT_0+lowest;
			break;
		} else if (ret != 0) {
			/* Didn't get all handles, and there was a
			 * timeout or other error
			 */
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: wait returned error: %s", __func__,
				   strerror (ret));

			if(ret==ETIMEDOUT) {
				retval = WAIT_TIMEOUT;
			} else {
				retval = WAIT_FAILED;
			}
			break;
		}
	}

	for (i = 0; i < numobjects; i++) {
		/* Unref everything we reffed above */
		_wapi_handle_unref (handles[i]);
	}

	return retval;
}

guint32 WaitForMultipleObjects(guint32 numobjects, gpointer *handles,
			       gboolean waitall, guint32 timeout)
{
	return WaitForMultipleObjectsEx(numobjects, handles, waitall, timeout, FALSE);
}

/**
 * WaitForInputIdle:
 * @handle: a handle to the process to wait for
 * @timeout: the maximum time in milliseconds to wait for
 *
 * This function returns when either @handle process is waiting
 * for input, or @timeout ms elapses.  If @timeout is zero, the
 * process state is tested and the function returns immediately.
 * If @timeout is %INFINITE, the function waits forever.
 *
 * Return value: 0 - @handle process is waiting for input.
 * %WAIT_TIMEOUT - The @timeout interval elapsed and
 * @handle process is not waiting for input.  %WAIT_FAILED - an error
 * occurred. 
 */
guint32 WaitForInputIdle(gpointer handle, guint32 timeout)
{
	/*TODO: Not implemented*/
	return WAIT_TIMEOUT;
}

