// ==++==
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
// CryptoKeySecurity.cs
//

using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Security.AccessControl {
    [Flags]
    public enum CryptoKeyRights {
        ReadData                     = 0x00000001,
        WriteData                    = 0x00000002,
        ReadExtendedAttributes       = 0x00000008,
        WriteExtendedAttributes      = 0x00000010,
        ReadAttributes               = 0x00000080,
        WriteAttributes              = 0x00000100,
        Delete                       = 0x00010000,
        ReadPermissions              = 0x00020000,
        ChangePermissions            = 0x00040000,
        TakeOwnership                = 0x00080000,
        Synchronize                  = 0x00100000,
        FullControl                  = 0x001F019B,
        GenericAll                   = 0x10000000,
        GenericExecute               = 0x20000000,
        GenericWrite                 = 0x40000000,
        GenericRead                  = unchecked((int) 0x80000000)
    }

    public sealed class CryptoKeyAccessRule : AccessRule {
        public CryptoKeyAccessRule (IdentityReference identity, CryptoKeyRights cryptoKeyRights, AccessControlType type)
                    : this (identity,
                            AccessMaskFromRights(cryptoKeyRights, type),
                            false,
                            InheritanceFlags.None,
                            PropagationFlags.None,
                            type) {
        }

        public CryptoKeyAccessRule (string identity, CryptoKeyRights cryptoKeyRights, AccessControlType type)
                    : this (new NTAccount(identity),
                            AccessMaskFromRights(cryptoKeyRights, type),
                            false,
                            InheritanceFlags.None,
                            PropagationFlags.None,
                            type) {
        }

        private CryptoKeyAccessRule (IdentityReference identity, int accessMask, bool isInherited,
                                     InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
                    : base (identity,
                            accessMask,
                            isInherited,
                            inheritanceFlags,
                            propagationFlags,
                            type) {
        }

        public CryptoKeyRights CryptoKeyRights {
            get { return RightsFromAccessMask(base.AccessMask); }
        }

        // Acl's on files have a Synchronize bit, and CreateFile always
        // asks for it. So for allows, let's always include this bit,
        // and for denies, let's never include this bit unless we're denying
        // full control. This is the right thing for users, even if it does
        // make the model look asymmetrical from a purist point of view.
        // Also, crypto key containers are just files in the end, so
        // this tweak to the access rights for files makes sense here.
        private static int AccessMaskFromRights (CryptoKeyRights cryptoKeyRights, AccessControlType controlType) {
            if (controlType == AccessControlType.Allow) {
                cryptoKeyRights |= CryptoKeyRights.Synchronize;
            }
            else if (controlType == AccessControlType.Deny) {
                if (cryptoKeyRights != CryptoKeyRights.FullControl)
                    cryptoKeyRights &= ~CryptoKeyRights.Synchronize;
            }
            else {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidEnumValue", controlType, "controlType"), "controlType");
            }

            return (int) cryptoKeyRights;
        }

        internal static CryptoKeyRights RightsFromAccessMask(int accessMask) {
            return (CryptoKeyRights) accessMask;
        }
    }


    public sealed class CryptoKeyAuditRule : AuditRule {
        public CryptoKeyAuditRule (IdentityReference identity, CryptoKeyRights cryptoKeyRights, AuditFlags flags)
                    : this (identity,
                            AccessMaskFromRights(cryptoKeyRights),
                            false,
                            InheritanceFlags.None,
                            PropagationFlags.None,
                            flags) {
        }

        public CryptoKeyAuditRule (string identity, CryptoKeyRights cryptoKeyRights, AuditFlags flags)
                    : this (new NTAccount(identity),
                            AccessMaskFromRights(cryptoKeyRights),
                            false,
                            InheritanceFlags.None,
                            PropagationFlags.None,
                            flags) {
        }

        private CryptoKeyAuditRule (IdentityReference identity, int accessMask, bool isInherited,
                                    InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
                    : base (identity,
                            accessMask,
                            isInherited,
                            inheritanceFlags,
                            propagationFlags,
                            flags) {
        }

        public CryptoKeyRights CryptoKeyRights {
            get { return RightsFromAccessMask(base.AccessMask); }
        }

        private static int AccessMaskFromRights (CryptoKeyRights cryptoKeyRights) {
            return (int) cryptoKeyRights;
        }

        internal static CryptoKeyRights RightsFromAccessMask(int accessMask) {
            return (CryptoKeyRights) accessMask;
        }
    }


    public sealed class CryptoKeySecurity : NativeObjectSecurity {
        private const ResourceType s_ResourceType = ResourceType.FileObject;

        public CryptoKeySecurity () : base(false, s_ResourceType) {}
        [System.Security.SecuritySafeCritical]  // auto-generated
        public CryptoKeySecurity (CommonSecurityDescriptor securityDescriptor) : base(s_ResourceType, securityDescriptor) {}

        public sealed override AccessRule AccessRuleFactory (IdentityReference identityReference,
                                                                int accessMask,
                                                                bool isInherited,
                                                                InheritanceFlags inheritanceFlags,
                                                                PropagationFlags propagationFlags,
                                                                AccessControlType type) {
            return new CryptoKeyAccessRule(
                identityReference,
                CryptoKeyAccessRule.RightsFromAccessMask(accessMask),
                type);
        }

        public sealed override AuditRule AuditRuleFactory (IdentityReference identityReference,
                                                              int accessMask,
                                                              bool isInherited,
                                                              InheritanceFlags inheritanceFlags,
                                                              PropagationFlags propagationFlags,
                                                              AuditFlags flags) {
            return new CryptoKeyAuditRule(
                identityReference,
                CryptoKeyAuditRule.RightsFromAccessMask(accessMask),
                flags);
        }

        public void AddAccessRule (CryptoKeyAccessRule rule) {
            base.AddAccessRule(rule);
        }

        public void SetAccessRule (CryptoKeyAccessRule rule) {
            base.SetAccessRule(rule);
        }

        public void ResetAccessRule (CryptoKeyAccessRule rule) {
            base.ResetAccessRule(rule);
        }

        public bool RemoveAccessRule (CryptoKeyAccessRule rule) {
            return base.RemoveAccessRule(rule);
        }

        public void RemoveAccessRuleAll (CryptoKeyAccessRule rule) {
            base.RemoveAccessRuleAll(rule);
        }

        public void RemoveAccessRuleSpecific (CryptoKeyAccessRule rule) {
            base.RemoveAccessRuleSpecific(rule);
        }

        public void AddAuditRule (CryptoKeyAuditRule rule) {
            base.AddAuditRule(rule);
        }

        public void SetAuditRule (CryptoKeyAuditRule rule) {
            base.SetAuditRule(rule);
        }

        public bool RemoveAuditRule (CryptoKeyAuditRule rule) {
            return base.RemoveAuditRule(rule);
        }

        public void RemoveAuditRuleAll (CryptoKeyAuditRule rule) {
            base.RemoveAuditRuleAll(rule);
        }

        public void RemoveAuditRuleSpecific (CryptoKeyAuditRule rule) {
            base.RemoveAuditRuleSpecific(rule);
        }
        
        #region some overrides
        public override Type AccessRightType
        {
            get { return typeof(System.Security.AccessControl.CryptoKeyRights); }
        }
        
        public override Type AccessRuleType
        {
            get { return typeof(System.Security.AccessControl.CryptoKeyAccessRule); }
        }
        
        public override Type AuditRuleType
        {
            get { return typeof(System.Security.AccessControl.CryptoKeyAuditRule); }
        }
        #endregion

        internal AccessControlSections ChangedAccessControlSections {
            [System.Security.SecurityCritical]  // auto-generated
            get {
                AccessControlSections changedSections = AccessControlSections.None;

                bool readLockAcquired = false;

                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try { }
                    finally {
                        ReadLock();
                        readLockAcquired = true;
                    }

                    if (AccessRulesModified)
                        changedSections |= AccessControlSections.Access;
                    if (AuditRulesModified)
                        changedSections |= AccessControlSections.Audit;
                    if (GroupModified)
                        changedSections |= AccessControlSections.Group;
                    if (OwnerModified)
                        changedSections |= AccessControlSections.Owner;
                }
                finally {
                    if (readLockAcquired) {
                        ReadUnlock();
                    }
                }

                return changedSections;
            }
        }
    }
}
