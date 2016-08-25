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

gpointer
ves_icall_System_Threading_Semaphore_CreateSemaphore_internal (gint32 initialCount, gint32 maximumCount, MonoString *name, gint32 *error)
{ 
	gpointer sem;

	sem = CreateSemaphore (NULL, initialCount, maximumCount, name ? mono_string_chars (name) : NULL);

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