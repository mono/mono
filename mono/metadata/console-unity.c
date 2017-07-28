#include <mono/metadata/console-io.h>

#if defined(PLATFORM_UNITY) && defined(UNITY_USE_PLATFORM_STUBS)

void
mono_console_init (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

void
mono_console_handle_async_ops (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

MonoBoolean
ves_icall_System_ConsoleDriver_Isatty (gpointer handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

MonoBoolean
ves_icall_System_ConsoleDriver_SetEcho (MonoBoolean want_echo)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

MonoBoolean
ves_icall_System_ConsoleDriver_SetBreak (MonoBoolean want_break)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gint32
ves_icall_System_ConsoleDriver_InternalKeyAvailable (gint32 timeout)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

MonoBoolean
ves_icall_System_ConsoleDriver_TtySetup (MonoString *keypad, MonoString *teardown, MonoArray **control_chars, int **size)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

#endif /* PLATFORM_UNITY && UNITY_USE_PLATFORM_STUBS */

