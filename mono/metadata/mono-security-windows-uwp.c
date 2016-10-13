/*
 * mono-security-windows-uwp.c: UWP security support for Mono.
 *
 * Copyright 2016 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
*/
#include <config.h>
#include <glib.h>

#if G_HAVE_API_SUPPORT(HAVE_UWP_WINAPI_SUPPORT)
#include <Windows.h>
#include "mono/metadata/mono-security-windows-internals.h"

gpointer
ves_icall_System_Security_Principal_WindowsIdentity_GetCurrentToken (void)
{
	MonoError mono_error;
	mono_error_init (&mono_error);

	g_unsupported_api ("OpenThreadToken, OpenProcessToken");

	mono_error_set_not_supported (&mono_error, G_UNSUPPORTED_API, "OpenThreadToken, OpenProcessToken");
	mono_error_set_pending_exception (&mono_error);

	SetLastError (ERROR_NOT_SUPPORTED);

	return NULL;
}

MonoArray*
ves_icall_System_Security_Principal_WindowsIdentity_GetRoles (gpointer token)
{
	MonoError mono_error;
	mono_error_init (&mono_error);

	g_unsupported_api ("GetTokenInformation");

	mono_error_set_not_supported (&mono_error, G_UNSUPPORTED_API, "GetTokenInformation");
	mono_error_set_pending_exception (&mono_error);

	SetLastError (ERROR_NOT_SUPPORTED);

	return NULL;
}

gpointer
ves_icall_System_Security_Principal_WindowsImpersonationContext_DuplicateToken (gpointer token)
{
	MonoError mono_error;
	mono_error_init (&mono_error);

	g_unsupported_api ("DuplicateToken");

	mono_error_set_not_supported (&mono_error, G_UNSUPPORTED_API, "DuplicateToken");
	mono_error_set_pending_exception (&mono_error);

	SetLastError (ERROR_NOT_SUPPORTED);

	return NULL;
}

gboolean
ves_icall_System_Security_Principal_WindowsImpersonationContext_SetCurrentToken (gpointer token)
{
	MonoError mono_error;
	mono_error_init (&mono_error);

	g_unsupported_api ("ImpersonateLoggedOnUser");

	mono_error_set_not_supported (&mono_error, G_UNSUPPORTED_API, "ImpersonateLoggedOnUser");
	mono_error_set_pending_exception (&mono_error);

	SetLastError (ERROR_NOT_SUPPORTED);

	return FALSE;
}

gboolean
ves_icall_System_Security_Principal_WindowsImpersonationContext_RevertToSelf (void)
{
	MonoError mono_error;
	mono_error_init (&mono_error);

	g_unsupported_api ("RevertToSelf");

	mono_error_set_not_supported(&mono_error, G_UNSUPPORTED_API, "RevertToSelf");
	mono_error_set_pending_exception (&mono_error);

	SetLastError (ERROR_NOT_SUPPORTED);

	return FALSE;
}

gint32
mono_security_win_get_token_name (gpointer token, gunichar2 ** uniname)
{
	MonoError mono_error;
	mono_error_init (&mono_error);

	g_unsupported_api ("GetTokenInformation");

	mono_error_set_not_supported (&mono_error, G_UNSUPPORTED_API, "GetTokenInformation");
	mono_error_set_pending_exception (&mono_error);

	SetLastError (ERROR_NOT_SUPPORTED);

	return 0;
}

gboolean
mono_security_win_is_machine_protected (gunichar2 *path)
{
	MonoError mono_error;
	mono_error_init (&mono_error);

	g_unsupported_api ("GetNamedSecurityInfo, LocalFree");

	mono_error_set_not_supported (&mono_error, G_UNSUPPORTED_API, "GetNamedSecurityInfo, LocalFree");
	mono_error_set_pending_exception (&mono_error);

	SetLastError (ERROR_NOT_SUPPORTED);

	return FALSE;
}

gboolean
mono_security_win_is_user_protected (gunichar2 *path)
{
	MonoError mono_error;
	mono_error_init (&mono_error);

	g_unsupported_api ("GetNamedSecurityInfo, LocalFree");

	mono_error_set_not_supported (&mono_error, G_UNSUPPORTED_API, "GetNamedSecurityInfo, LocalFree");
	mono_error_set_pending_exception (&mono_error);

	SetLastError (ERROR_NOT_SUPPORTED);

	return FALSE;
}

gboolean
mono_security_win_protect_machine (gunichar2 *path)
{
	MonoError mono_error;
	mono_error_init (&mono_error);

	g_unsupported_api ("BuildTrusteeWithSid, SetEntriesInAcl, SetNamedSecurityInfo, LocalFree, FreeSid");

	mono_error_set_not_supported (&mono_error, G_UNSUPPORTED_API, "BuildTrusteeWithSid, SetEntriesInAcl, SetNamedSecurityInfo, LocalFree, FreeSid");
	mono_error_set_pending_exception (&mono_error);

	SetLastError (ERROR_NOT_SUPPORTED);

	return FALSE;
}

gboolean
mono_security_win_protect_user (gunichar2 *path)
{
	MonoError mono_error;
	mono_error_init (&mono_error);

	g_unsupported_api ("BuildTrusteeWithSid, SetEntriesInAcl, SetNamedSecurityInfo, LocalFree");

	mono_error_set_not_supported (&mono_error, G_UNSUPPORTED_API, "BuildTrusteeWithSid, SetEntriesInAcl, SetNamedSecurityInfo, LocalFree");
	mono_error_set_pending_exception (&mono_error);

	SetLastError (ERROR_NOT_SUPPORTED);

	return FALSE;
}
#else /* G_HAVE_API_SUPPORT(HAVE_UWP_WINAPI_SUPPORT) */

#ifdef _MSC_VER
// Quiet Visual Studio linker warning, LNK4221, in cases when this source file intentional ends up empty.
void __mono_win32_mono_security_windows_uwp_quiet_lnk4221(void) {}
#endif
#endif /* G_HAVE_API_SUPPORT(HAVE_UWP_WINAPI_SUPPORT) */
