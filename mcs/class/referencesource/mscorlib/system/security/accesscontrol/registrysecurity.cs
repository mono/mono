// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  RegistrySecurity
**
**
** Purpose: Managed ACL wrapper for registry keys.
**
**
===========================================================*/

using System;
using System.Collections;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.IO;

namespace System.Security.AccessControl
{
    // We derived this enum from the definitions of KEY_READ and such from
    // winnt.h and from MSDN, plus some experimental validation with regedit.
    // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/sysinfo/base/registry_key_security_and_access_rights.asp
    [Flags]
    public enum RegistryRights
    {
        // No None field - An ACE with the value 0 cannot grant nor deny.
        QueryValues          = Win32Native.KEY_QUERY_VALUE,          // 0x0001 query the values of a registry key
        SetValue             = Win32Native.KEY_SET_VALUE,            // 0x0002 create, delete, or set a registry value
        CreateSubKey         = Win32Native.KEY_CREATE_SUB_KEY,       // 0x0004 required to create a subkey of a specific key
        EnumerateSubKeys     = Win32Native.KEY_ENUMERATE_SUB_KEYS,   // 0x0008 required to enumerate sub keys of a key
        Notify               = Win32Native.KEY_NOTIFY,               // 0x0010 needed to request change notifications
        CreateLink           = Win32Native.KEY_CREATE_LINK,          // 0x0020 reserved for system use
///
/// The Windows Kernel team agrees that it was a bad design to expose the WOW64_n options as permissions.
/// in the .NET Framework these options are exposed via the RegistryView enum
///
///        Reg64             = Win32Native.KEY_WOW64_64KEY,          // 0x0100 operate on the 64-bit registry view
///        Reg32             = Win32Native.KEY_WOW64_32KEY,          // 0x0200 operate on the 32-bit registry view
        ExecuteKey           = ReadKey,
        ReadKey              = Win32Native.STANDARD_RIGHTS_READ | QueryValues | EnumerateSubKeys | Notify,
        WriteKey             = Win32Native.STANDARD_RIGHTS_WRITE | SetValue | CreateSubKey,
        Delete               = 0x10000,
        ReadPermissions      = 0x20000,
        ChangePermissions    = 0x40000,
        TakeOwnership        = 0x80000,
        FullControl          = 0xF003F | Win32Native.STANDARD_RIGHTS_READ | Win32Native.STANDARD_RIGHTS_WRITE
    }


    public sealed class RegistryAccessRule : AccessRule
    {
        // Constructor for creating access rules for registry objects

        public RegistryAccessRule(IdentityReference identity, RegistryRights registryRights, AccessControlType type) 
            : this(identity, (int) registryRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public RegistryAccessRule(String identity, RegistryRights registryRights, AccessControlType type) 
            : this(new NTAccount(identity), (int) registryRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public RegistryAccessRule(IdentityReference identity, RegistryRights registryRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
            : this(identity, (int) registryRights, false, inheritanceFlags, propagationFlags, type)
        {
        }

        public RegistryAccessRule(string identity, RegistryRights registryRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
            : this(new NTAccount(identity), (int) registryRights, false, inheritanceFlags, propagationFlags, type)
        {
        }

        //
        // Internal constructor to be called by public constructors
        // and the access rule factory methods of {File|Folder}Security
        //
        internal RegistryAccessRule(
            IdentityReference identity,
            int accessMask,
            bool isInherited,
            InheritanceFlags inheritanceFlags,
            PropagationFlags propagationFlags,
            AccessControlType type )
            : base(
                identity,
                accessMask,
                isInherited,
                inheritanceFlags,
                propagationFlags,
                type )
        {
        }

        public RegistryRights RegistryRights { 
            get { return (RegistryRights) base.AccessMask; }
        }
    }


    public sealed class RegistryAuditRule : AuditRule
    {
        public RegistryAuditRule(IdentityReference identity, RegistryRights registryRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
            : this(identity, (int) registryRights, false, inheritanceFlags, propagationFlags, flags)
        {
        }

        public RegistryAuditRule(string identity, RegistryRights registryRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
            : this(new NTAccount(identity), (int) registryRights, false, inheritanceFlags, propagationFlags, flags)
        {
        }

        internal RegistryAuditRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
            : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, flags)
        {
        }
        
        public RegistryRights RegistryRights { 
            get { return (RegistryRights) base.AccessMask; }
        }
    }


    public sealed class RegistrySecurity : NativeObjectSecurity
    {
        public RegistrySecurity()
            : base(true, ResourceType.RegistryKey)
        {
        }

        /*
        // The name of registry key must start with a predefined string,
        // like CLASSES_ROOT, CURRENT_USER, MACHINE, and USERS.  See
        // MSDN's help for SetNamedSecurityInfo for details.
        [SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        internal RegistrySecurity(String name, AccessControlSections includeSections)
            : base(true, ResourceType.RegistryKey, HKeyNameToWindowsName(name), includeSections)
        {
            new RegistryPermission(RegistryPermissionAccess.NoAccess, AccessControlActions.View, name).Demand();
        }
        */

        [System.Security.SecurityCritical]  // auto-generated
        [SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        internal RegistrySecurity(SafeRegistryHandle hKey, String name, AccessControlSections includeSections)
            : base(true, ResourceType.RegistryKey, hKey, includeSections, _HandleErrorCode, null )
        {
            new RegistryPermission(RegistryPermissionAccess.NoAccess, AccessControlActions.View, name).Demand();
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static Exception _HandleErrorCode(int errorCode, string name, SafeHandle handle, object context)
        {
            System.Exception exception = null;
            
            switch (errorCode) {
            case Win32Native.ERROR_FILE_NOT_FOUND:
                exception = new IOException(Environment.GetResourceString("Arg_RegKeyNotFound", errorCode));
                break;

            case Win32Native.ERROR_INVALID_NAME:
                exception = new ArgumentException(Environment.GetResourceString("Arg_RegInvalidKeyName", "name"));
                break;

            case Win32Native.ERROR_INVALID_HANDLE:
                exception = new ArgumentException(Environment.GetResourceString("AccessControl_InvalidHandle"));
                break;

            default:
                break;
            }

            return exception;
        }

        public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new RegistryAccessRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, type);
        }

        public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new RegistryAuditRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, flags);
        }

        internal AccessControlSections GetAccessControlSectionsFromChanges()
        {
            AccessControlSections persistRules = AccessControlSections.None;
            if (AccessRulesModified)
                persistRules = AccessControlSections.Access;
            if (AuditRulesModified)
                persistRules |= AccessControlSections.Audit;
            if (OwnerModified)
                persistRules |= AccessControlSections.Owner;
            if (GroupModified)
                persistRules |= AccessControlSections.Group;
            return persistRules;
        }

        /*
        // See SetNamedSecurityInfo docs - we must start strings
        // with names like CURRENT_USER, MACHINE, CLASSES_ROOT, etc.  
        // (Look at SE_OBJECT_TYPE, then the docs for SE_REGISTRY_KEY)
        internal static String HKeyNameToWindowsName(String keyName)
        {
            if (keyName.StartsWith("HKEY_")) {
                if (keyName.Equals("HKEY_LOCAL_MACHINE"))
                    return "MACHINE";
                return keyName.Substring(5);
            }
            return keyName;
        }

        [SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        internal void Persist(String keyName)
        {
            new RegistryPermission(RegistryPermissionAccess.NoAccess, AccessControlActions.Change, keyName).Demand();

            AccessControlSections persistRules = GetAccessControlSectionsFromChanges();
            if (persistRules == AccessControlSections.None)
                return;  // Don't need to persist anything.

            String windowsKeyName = HKeyNameToWindowsName(keyName);
            base.Persist(windowsKeyName, persistRules);
            OwnerModified = GroupModified = AuditRulesModified = AccessRulesModified = false;
        }
        */

        [System.Security.SecurityCritical]  // auto-generated
        [SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        internal void Persist(SafeRegistryHandle hKey, String keyName)
        {
            new RegistryPermission(RegistryPermissionAccess.NoAccess, AccessControlActions.Change, keyName).Demand();

            WriteLock();

            try
            {
                AccessControlSections persistRules = GetAccessControlSectionsFromChanges();
                if (persistRules == AccessControlSections.None)
                    return;  // Don't need to persist anything.

                base.Persist(hKey, persistRules);
                OwnerModified = GroupModified = AuditRulesModified = AccessRulesModified = false;
            }
            finally
            {
                WriteUnlock();
            }
        }

        public void AddAccessRule(RegistryAccessRule rule)
        {
            base.AddAccessRule(rule);
        }

        public void SetAccessRule(RegistryAccessRule rule)
        {
            base.SetAccessRule(rule);
        }

        public void ResetAccessRule(RegistryAccessRule rule)
        {
            base.ResetAccessRule(rule);
        }

        public bool RemoveAccessRule(RegistryAccessRule rule)
        {
            return base.RemoveAccessRule(rule);
        }

        public void RemoveAccessRuleAll(RegistryAccessRule rule)
        {
            base.RemoveAccessRuleAll(rule);
        }

        public void RemoveAccessRuleSpecific(RegistryAccessRule rule)
        {
            base.RemoveAccessRuleSpecific(rule);
        }
                
        public void AddAuditRule(RegistryAuditRule rule)
        {
            base.AddAuditRule(rule);
        }

        public void SetAuditRule(RegistryAuditRule rule)
        {
            base.SetAuditRule(rule);
        }

        public bool RemoveAuditRule(RegistryAuditRule rule)
        {
            return base.RemoveAuditRule(rule);
        }

        public void RemoveAuditRuleAll(RegistryAuditRule rule)
        {
            base.RemoveAuditRuleAll(rule);
        }

        public void RemoveAuditRuleSpecific(RegistryAuditRule rule)
        {
            base.RemoveAuditRuleSpecific(rule);
        }

        public override Type AccessRightType
        {
            get { return typeof(RegistryRights); }
        }
        
        public override Type AccessRuleType
        {
            get { return typeof(RegistryAccessRule); }
        }
        
        public override Type AuditRuleType
        {
            get { return typeof(RegistryAuditRule); }
        }
    }
}
