// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// WindowsPrincipal.cs
//
// Group membership checks.
//

namespace System.Security.Principal
{
    using System.Diagnostics.Contracts;
    using System.Runtime.InteropServices;
#if !FEATURE_CORECLR
    using System.Runtime.Serialization;
    using System.Security.Claims;
    using System.Collections.Generic;
#endif
    using System.Security.Permissions;
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using Hashtable = System.Collections.Hashtable;


    [Serializable]
    [ComVisible(true)]
    public enum WindowsBuiltInRole {
        Administrator   = 0x220,
        User            = 0x221,
        Guest           = 0x222,
        PowerUser       = 0x223,
        AccountOperator = 0x224,
        SystemOperator  = 0x225,
        PrintOperator   = 0x226,
        BackupOperator  = 0x227,
        Replicator      = 0x228
    }

    [Serializable]
    [HostProtection(SecurityInfrastructure=true)]
    [ComVisible(true)]

#if !FEATURE_CORECLR
    public class WindowsPrincipal : ClaimsPrincipal {
#else
    public class WindowsPrincipal : IPrincipal {
#endif
        private WindowsIdentity m_identity = null;

        // Following 3 fields are present purely for serialization compatability with Everett: not used in Whidbey        
#pragma warning disable 169
        private String[] m_roles;
        private Hashtable m_rolesTable;
        private bool m_rolesLoaded;
#pragma warning restore 169

        //
        // Constructors.
        //

        private WindowsPrincipal () {}

        public WindowsPrincipal (WindowsIdentity ntIdentity) 

#if !FEATURE_CORECLR
            : base (ntIdentity) 
#endif
        {
            if (ntIdentity == null)
                throw new ArgumentNullException("ntIdentity");
            Contract.EndContractBlock();

            m_identity = ntIdentity;
        }

#if !FEATURE_CORECLR
        [OnDeserialized()]
        [SecuritySafeCritical]
        private void OnDeserializedMethod(StreamingContext context)
        {
            // Here it the matrix of possible serializations
            //
            // Version From  |  Version To | ClaimsIdentities | Roles
            // ============     ==========   ================   ========================================================
            // 4.0               4.5         None               We always need to add a ClaimsIdentity
            //
            // 4.5               4.5         Yes                There should be a ClaimsIdentity

            ClaimsIdentity firstNonNullIdentity = null;
            foreach (var identity in base.Identities)
            {
                if (identity != null)
                {
                    firstNonNullIdentity = identity;
                    break;
                }
            }

            if (firstNonNullIdentity == null)
            {
                base.AddIdentity(m_identity);
            }
        }
#endif //!FEATURE_CORECLR

        //
        // Properties.
        //
#if !FEATURE_CORECLR
        public override IIdentity Identity {
#else
        public virtual IIdentity Identity {
#endif
            get {
                return m_identity;
            }
        }

        //
        // Public methods.
        //

        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, ControlPrincipal = true)]
#if !FEATURE_CORECLR
        public override bool IsInRole (string role) {
#else
        public virtual bool IsInRole (string role) {
#endif
            if (role == null || role.Length == 0)
                return false;

            NTAccount ntAccount = new NTAccount(role);
            IdentityReferenceCollection source = new IdentityReferenceCollection(1);
            source.Add(ntAccount);
            IdentityReferenceCollection target = NTAccount.Translate(source, typeof(SecurityIdentifier), false);

            SecurityIdentifier sid = target[0] as SecurityIdentifier;

#if !FEATURE_CORECLR
            if (sid != null) {
                if ( IsInRole(sid) ) {
                    return true;
                }
            }

            // possible that identity has other role claims that match
            return base.IsInRole(role);
#else
            if (sid == null)
                return false;

            return IsInRole(sid);
#endif
        }

#if !FEATURE_CORECLR
        // <summary
        // Returns all of the claims from all of the identities that are windows user claims
        // found in the NT token.
        // </summary>
        public virtual IEnumerable<Claim> UserClaims
        {
            get
            {
                foreach (ClaimsIdentity identity in Identities)
                {
                    WindowsIdentity wi = identity as WindowsIdentity;
                    if ( wi!=null)
                    {
                        foreach (Claim claim in wi.UserClaims)
                        {
                            yield return claim;
                        }
                    }   
                }
            }
        }

        // <summary
        // Returns all of the claims from all of the identities that are windows device claims
        // found in the NT token.
        // </summary>
        public virtual IEnumerable<Claim> DeviceClaims
        {
            get
            {
                foreach (ClaimsIdentity identity in Identities)
                {
                    WindowsIdentity wi = identity as WindowsIdentity;
                    if (wi != null)
                    {
                        foreach (Claim claim in wi.DeviceClaims)
                        {
                            yield return claim;
                        }
                    }
                }
            }
        }
#endif
        public virtual bool IsInRole (WindowsBuiltInRole role) {
            if (role < WindowsBuiltInRole.Administrator || role > WindowsBuiltInRole.Replicator)
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", (int)role), "role");
            Contract.EndContractBlock();

            return IsInRole((int) role);
        }

        public virtual bool IsInRole (int rid) {
            SecurityIdentifier sid = new SecurityIdentifier(IdentifierAuthority.NTAuthority, 
                                                            new int[] {Win32Native.SECURITY_BUILTIN_DOMAIN_RID, rid});

            return IsInRole(sid);
        }

        // This methods (with a SID parameter) is more general than the 2 overloads that accept a WindowsBuiltInRole or
        // a rid (as an int). It is also better from a performance standpoint than the overload that accepts a string.
        // The aformentioned overloads remain in this class since we do not want to introduce a
        // breaking change. However, this method should be used in all new applications and we should document this.

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ComVisible(false)]
        public virtual bool IsInRole (SecurityIdentifier sid) {
            if (sid == null)
                throw new ArgumentNullException("sid");
            Contract.EndContractBlock();

            // special case the anonymous identity.
            if (m_identity.TokenHandle.IsInvalid)
                return false;

            // CheckTokenMembership expects an impersonation token
            SafeTokenHandle token = SafeTokenHandle.InvalidHandle;
            if (m_identity.ImpersonationLevel == TokenImpersonationLevel.None) {
                if (!Win32Native.DuplicateTokenEx(m_identity.TokenHandle,
                                                  (uint) TokenAccessLevels.Query,
                                                  IntPtr.Zero,
                                                  (uint) TokenImpersonationLevel.Identification,
                                                  (uint) TokenType.TokenImpersonation,
                                                  ref token))
                    throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
            }

            bool isMember = false;
            // CheckTokenMembership will check if the SID is both present and enabled in the access token.
            if (!Win32Native.CheckTokenMembership((m_identity.ImpersonationLevel != TokenImpersonationLevel.None ? m_identity.TokenHandle : token),
                                                  sid.BinaryForm,
                                                  ref isMember))
                throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));

            token.Dispose();
            return isMember;
        }
    }
}
