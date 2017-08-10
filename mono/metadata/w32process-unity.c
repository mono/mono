#include <mono/metadata/w32process.h>

#if defined(PLATFORM_UNITY) && defined(UNITY_USE_PLATFORM_STUBS)

void
mono_w32process_init (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

void
mono_w32process_cleanup (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

gpointer
ves_icall_System_Diagnostics_Process_GetProcess_internal (guint32 pid)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

MonoBoolean
ves_icall_System_Diagnostics_Process_ShellExecuteEx_internal (MonoW32ProcessStartInfo *proc_start_info, MonoW32ProcessInfo *process_info)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

MonoBoolean
ves_icall_System_Diagnostics_Process_CreateProcess_internal (MonoW32ProcessStartInfo *proc_start_info, gpointer stdin_handle,
								 gpointer stdout_handle, gpointer stderr_handle, MonoW32ProcessInfo *process_info)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

MonoArray *
ves_icall_System_Diagnostics_Process_GetProcesses_internal (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_CloseProcess (gpointer handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_TerminateProcess (gpointer handle, gint32 exitcode)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_GetExitCodeProcess (gpointer handle, gint32 *exitcode)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_GetProcessWorkingSetSize (gpointer handle, gsize *min, gsize *max)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_SetProcessWorkingSetSize (gpointer handle, gsize min, gsize max)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gint32
ves_icall_Microsoft_Win32_NativeMethods_GetPriorityClass (gpointer handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_SetPriorityClass (gpointer handle, gint32 priorityClass)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_GetProcessTimes (gpointer handle, gint64 *creationtime, gint64 *exittime, gint64 *kerneltime, gint64 *usertime)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gpointer
ves_icall_Microsoft_Win32_NativeMethods_GetCurrentProcess (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

MonoString *
ves_icall_System_Diagnostics_Process_ProcessName_internal (gpointer process)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

MonoArray *
ves_icall_System_Diagnostics_Process_GetModules_internal (MonoObject *this_obj, gpointer process)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

#endif /* PLATFORM_UNITY && UNITY_USE_PLATFORM_STUBS */
