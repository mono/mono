/**
 * \file
 * System.Diagnostics.Process support
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 */

#ifndef _MONO_METADATA_W32PROCESS_H_
#define _MONO_METADATA_W32PROCESS_H_

#include <config.h>
#include <glib.h>

#if HAVE_SYS_TYPES_H
#include <sys/types.h>
#endif

#include <mono/metadata/object.h>

G_BEGIN_DECLS

typedef enum {
	MONO_W32PROCESS_PRIORITY_CLASS_NORMAL       = 0x0020,
	MONO_W32PROCESS_PRIORITY_CLASS_IDLE         = 0x0040,
	MONO_W32PROCESS_PRIORITY_CLASS_HIGH         = 0x0080,
	MONO_W32PROCESS_PRIORITY_CLASS_REALTIME     = 0x0100,
	MONO_W32PROCESS_PRIORITY_CLASS_BELOW_NORMAL = 0x4000,
	MONO_W32PROCESS_PRIORITY_CLASS_ABOVE_NORMAL = 0x8000,
} MonoW32ProcessPriorityClass;

// Output of CreateProcess and ShellExecute,
typedef struct _MonoW32ProcessInfo
{
	gpointer process_handle;
	guint32 pid; /* Contains mono_w32error_get_last () on failure */
	MonoArray *env_variables;
	MonoString *username;
	MonoString *domain;
	gpointer password; /* BSTR from SecureString in 2.0 profile */
	MonoBoolean load_user_profile;
} MonoW32ProcessInfo;

// Input to CreateProcess and ShellExecute.
typedef struct _MonoW32ProcessStartInfo
{
	MonoString *filename;
	MonoString *arguments;
	MonoString *working_directory;
	MonoString *verb;
	guint32 window_style;
	MonoBoolean error_dialog;
	gpointer error_dialog_parent_handle;
	MonoBoolean use_shell_execute;
	MonoString *username;
	MonoString *domain;
	MonoObject *password; /* SecureString in 2.0 profile, dummy in 1.x */
	MonoString *password_in_clear_text;
	MonoBoolean load_user_profile;
	MonoBoolean redirect_standard_input;
	MonoBoolean redirect_standard_output;
	MonoBoolean redirect_standard_error;
	MonoObject *encoding_stdout;
	MonoObject *encoding_stderr;
	MonoBoolean create_no_window;
	MonoObject *weak_parent_process;
	MonoObject *envVars;
} MonoW32ProcessStartInfo;

void
mono_w32process_init (void);

void
mono_w32process_cleanup (void);

void
mono_w32process_signal_finished (void);

#ifndef HOST_WIN32

void
mono_w32process_set_cli_launcher (gchar *path);

gchar*
mono_w32process_get_path (pid_t pid);

#endif

gpointer
ves_icall_System_Diagnostics_Process_GetProcess_internal (guint32 pid);

MonoArray*
ves_icall_System_Diagnostics_Process_GetProcesses_internal (void);

MonoArray*
ves_icall_System_Diagnostics_Process_GetModules_internal (MonoObject *this_obj, gpointer process);

void
ves_icall_System_Diagnostics_FileVersionInfo_GetVersionInfo_internal (MonoObject *this_obj, MonoString *filename);

MonoBoolean
ves_icall_System_Diagnostics_Process_ShellExecuteEx_internal (
// ProcessStartInfo
	MonoString *startInfo_filename,
	MonoString *startInfo_arguments,
	MonoString *startInfo_working_directory,
	MonoString *startInfo_verb,
	guint32 startInfo_window_style,
	MonoBoolean startInfo_error_dialog,
	gpointer startInfo_error_dialog_parent_handle,
	MonoBoolean startInfo_use_shell_execute,
	MonoString *startInfo_username,
	MonoString *startInfo_domain,
	MonoObject *startInfo_password, /* SecureString in 2.0 profile, dummy in 1.x */
	MonoString *startInfo_password_in_clear_text,
	MonoBoolean startInfo_load_user_profile,
	MonoBoolean startInfo_redirect_standard_input,
	MonoBoolean startInfo_redirect_standard_output,
	MonoBoolean startInfo_redirect_standard_error,
	MonoObject *startInfo_encoding_stdout,
	MonoObject *startInfo_encoding_stderr,
	MonoBoolean startInfo_create_no_window,
	MonoObject *startInfo_weak_parent_process,
	MonoObject *startInfo_envVars,
// ProcessInfo
	gpointer *procInfo_process_handle,
	guint32 *procInfo_pid, /* Contains mono_w32error_get_last () on failure */
	MonoArray **procInfo_env_variables,
	MonoString **procInfo_username,
	MonoString **procInfo_domain,
	gpointer *procInfo_password, /* BSTR from SecureString in 2.0 profile */
	MonoBoolean *procInfo_load_user_profile
	);

MonoBoolean
ves_icall_System_Diagnostics_Process_CreateProcess_internal (
	gpointer stdin_handle,
	gpointer stdout_handle,
	gpointer stderr_handle,
// ProcessStartInfo
	MonoString *startInfo_filename,
	MonoString *startInfo_arguments,
	MonoString *startInfo_working_directory,
	MonoString *startInfo_verb,
	guint32 startInfo_window_style,
	MonoBoolean startInfo_error_dialog,
	gpointer startInfo_error_dialog_parent_handle,
	MonoBoolean startInfo_use_shell_execute,
	MonoString *startInfo_username,
	MonoString *startInfo_domain,
	MonoObject *startInfo_password, /* SecureString in 2.0 profile, dummy in 1.x */
	MonoString *startInfo_password_in_clear_text,
	MonoBoolean startInfo_load_user_profile,
	MonoBoolean startInfo_redirect_standard_input,
	MonoBoolean startInfo_redirect_standard_output,
	MonoBoolean startInfo_redirect_standard_error,
	MonoObject *startInfo_encoding_stdout,
	MonoObject *startInfo_encoding_stderr,
	MonoBoolean startInfo_create_no_window,
	MonoObject *startInfo_weak_parent_process,
	MonoObject *startInfo_envVars,
// ProcessInfo
	gpointer *procInfo_process_handle,
	guint32 *procInfo_pid, /* Contains mono_w32error_get_last () on failure */
	MonoArray **procInfo_env_variables,
	MonoString **procInfo_username,
	MonoString **procInfo_domain,
	gpointer *procInfo_password, /* BSTR from SecureString in 2.0 profile */
	MonoBoolean *procInfo_load_user_profile
	);

MonoString*
ves_icall_System_Diagnostics_Process_ProcessName_internal (gpointer process);

gint64
ves_icall_System_Diagnostics_Process_GetProcessData (int pid, gint32 data_type, gint32 *error);

gpointer
ves_icall_Microsoft_Win32_NativeMethods_GetCurrentProcess (MonoError *error);

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_GetExitCodeProcess (gpointer handle, gint32 *exitcode, MonoError *error);

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_CloseProcess (gpointer handle, MonoError *error);

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_TerminateProcess (gpointer handle, gint32 exitcode, MonoError *error);

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_GetProcessWorkingSetSize (gpointer handle, gsize *min, gsize *max, MonoError *error);

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_SetProcessWorkingSetSize (gpointer handle, gsize min, gsize max, MonoError *error);

gint32
ves_icall_Microsoft_Win32_NativeMethods_GetPriorityClass (gpointer handle, MonoError *error);

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_SetPriorityClass (gpointer handle, gint32 priorityClass, MonoError *error);

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_GetProcessTimes (gpointer handle, gint64 *creationtime, gint64 *exittime, gint64 *kerneltime, gint64 *usertime, MonoError *error);

G_END_DECLS

#endif /* _MONO_METADATA_W32PROCESS_H_ */

