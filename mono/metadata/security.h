/**
 * \file
 * Security internal calls
 *
 * Author:
 *	Sebastien Pouliot  <sebastien@ximian.com>
 *
 * (C) 2004 Novell (http://www.novell.com)
 */


#ifndef _MONO_METADATA_SECURITY_H_
#define _MONO_METADATA_SECURITY_H_

#include <glib.h>
#include <mono/metadata/object.h>
#include <mono/metadata/object-internals.h>
#include <mono/utils/mono-compiler.h>
#include <mono/utils/mono-error.h>
#include <mono/utils/mono-publib.h>
#include <mono/metadata/icalls.h>

G_BEGIN_DECLS

/* System.Environment */
ICALL_EXPORT
MonoStringHandle
ves_icall_System_Environment_get_UserName (MonoError *error);


/* System.Security.Principal.WindowsIdentity */
gpointer
mono_security_principal_windows_identity_get_current_token (MonoError *error);

MonoArray*
ves_icall_System_Security_Principal_WindowsIdentity_GetRoles (gpointer token);

gpointer
ves_icall_System_Security_Principal_WindowsIdentity_GetCurrentToken (MonoError *error);

MonoStringHandle
ves_icall_System_Security_Principal_WindowsIdentity_GetTokenName (gpointer token, MonoError *error);

gpointer
ves_icall_System_Security_Principal_WindowsIdentity_GetUserToken (MonoStringHandle username, MonoError *error);

MonoArray*
ves_icall_System_Security_Principal_WindowsIdentity_GetRoles (gpointer token);

gpointer
ves_icall_System_Security_Principal_WindowsIdentity_GetCurrentToken (MonoError *error);

MonoStringHandle
ves_icall_System_Security_Principal_WindowsIdentity_GetTokenName (gpointer token, MonoError *error);

gpointer
ves_icall_System_Security_Principal_WindowsIdentity_GetUserToken (MonoStringHandle username, MonoError *error);

/* System.Security.Principal.WindowsImpersonationContext */
gboolean
ves_icall_System_Security_Principal_WindowsImpersonationContext_CloseToken (gpointer token, MonoError *error);

gpointer
ves_icall_System_Security_Principal_WindowsImpersonationContext_DuplicateToken (gpointer token, MonoError *error);

gboolean
ves_icall_System_Security_Principal_WindowsImpersonationContext_SetCurrentToken (gpointer token, MonoError *error);

gboolean
ves_icall_System_Security_Principal_WindowsImpersonationContext_RevertToSelf (MonoError *error);


/* System.Security.Principal.WindowsPrincipal */
gboolean
ves_icall_System_Security_Principal_WindowsPrincipal_IsMemberOfGroupId (gpointer user, gpointer group, MonoError *error);

gboolean
ves_icall_System_Security_Principal_WindowsPrincipal_IsMemberOfGroupName (gpointer user, const gchar *group, MonoError *error);

/* Mono.Security.Cryptography.KeyPairPersistance */
MonoBoolean
ves_icall_Mono_Security_Cryptography_KeyPairPersistence_CanSecure (const gunichar2 *root, MonoError *error);

MonoBoolean
ves_icall_Mono_Security_Cryptography_KeyPairPersistence_IsMachineProtected (const gunichar2 *path, MonoError *error);

MonoBoolean
ves_icall_Mono_Security_Cryptography_KeyPairPersistence_IsUserProtected (const gunichar2 *path, MonoError *error);

MonoBoolean
ves_icall_Mono_Security_Cryptography_KeyPairPersistence_ProtectMachine (const gunichar2 *path, MonoError *error);

MonoBoolean
ves_icall_Mono_Security_Cryptography_KeyPairPersistence_ProtectUser (const gunichar2 *path, MonoError *error);


/* System.Security.Policy.Evidence */
MonoBoolean
ves_icall_System_Security_Policy_Evidence_IsAuthenticodePresent (MonoReflectionAssemblyHandle refass, MonoError *error);

/* System.Security.SecureString */
void
ves_icall_System_Security_SecureString_DecryptInternal (MonoArray *data, MonoObject *scope);

void
ves_icall_System_Security_SecureString_EncryptInternal (MonoArray *data, MonoObject *scope);

void
mono_invoke_protected_memory_method (MonoArray *data, MonoObject *scope, gboolean encrypt, MonoError *error);

G_END_DECLS

#endif /* _MONO_METADATA_SECURITY_H_ */
