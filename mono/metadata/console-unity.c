#include <mono/metadata/console-io.h>
#include "Console-c-api.h"
#include "File-c-api.h" /* required for IAtty */

#if defined(PLATFORM_UNITY)

void
mono_console_init (void)
{
}

void
mono_console_handle_async_ops (void)
{
}

MonoBoolean
ves_icall_System_ConsoleDriver_Isatty (gpointer handle)
{
	return UnityPalIsatty(handle);
}

MonoBoolean
ves_icall_System_ConsoleDriver_SetEcho (MonoBoolean want_echo)
{
	return UnityPalConsoleSetEcho(want_echo);
}

MonoBoolean
ves_icall_System_ConsoleDriver_SetBreak (MonoBoolean want_break)
{
	return UnityPalConsoleSetBreak(want_break);
}

gint32
ves_icall_System_ConsoleDriver_InternalKeyAvailable (gint32 timeout)
{
	return UnityPalConsoleInternalKeyAvailable(timeout);
}

MonoBoolean
ves_icall_System_ConsoleDriver_TtySetup (MonoString *keypad, MonoString *teardown, MonoArray **control_chars, int **size)
{
	return UnityPalConsoleTtySetup(keypad, teardown, control_chars, size);
}

#endif /* PLATFORM_UNITY */
