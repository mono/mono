
#include "wapi.h"

#include "mono/utils/mono-lazy-init.h"
#include "mono/metadata/w32handle.h"

/**
 * CloseHandle:
 * @handle: The handle to release
 *
 * Closes and invalidates @handle, releasing any resources it
 * consumes.  When the last handle to a temporary or non-persistent
 * object is closed, that object can be deleted.  Closing the same
 * handle twice is an error.
 *
 * Return value: %TRUE on success, %FALSE otherwise.
 */
gboolean CloseHandle(gpointer handle)
{
	if (handle == INVALID_HANDLE_VALUE){
		SetLastError (ERROR_INVALID_PARAMETER);
		return FALSE;
	}
	if (handle == (gpointer)0 && mono_w32handle_get_type (handle) != MONO_W32HANDLE_CONSOLE) {
		/* Problem: because we map file descriptors to the
		 * same-numbered handle we can't tell the difference
		 * between a bogus handle and the handle to stdin.
		 * Assume that it's the console handle if that handle
		 * exists...
		 */
		SetLastError (ERROR_INVALID_PARAMETER);
		return FALSE;
	}

	mono_w32handle_unref (handle);
	return TRUE;
}
