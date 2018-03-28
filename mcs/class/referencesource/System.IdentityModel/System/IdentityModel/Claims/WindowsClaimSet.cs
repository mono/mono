//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Claims
{
    using System.IdentityModel.Policy;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;

    public class WindowsClaimSet : ClaimSet, IIdentityInfo, IDisposable
    {
        internal const bool DefaultIncludeWindowsGroups = true;
        WindowsIdentity windowsIdentity;
        DateTime expirationTime;
        bool includeWindowsGroups;
        IList<Claim> claims;
        GroupSidClaimCollection groups;
        bool disposed = false;
        string authenticationType;

        public WindowsClaimSet(WindowsIdentity windowsIdentity)
            : this(windowsIdentity, DefaultIncludeWindowsGroups)
        {
        }

        public WindowsClaimSet(WindowsIdentity windowsIdentity, bool includeWindowsGroups)
            : this(windowsIdentity, includeWindowsGroups, DateTime.UtcNow.AddHours(10))
        {
        }

        public WindowsClaimSet(WindowsIdentity windowsIdentity, DateTime expirationTime)
            : this(windowsIdentity, DefaultIncludeWindowsGroups, expirationTime)
        {
        }

        public WindowsClaimSet(WindowsIdentity windowsIdentity, bool includeWindowsGroups, DateTime expirationTime)
            : this(windowsIdentity, null, includeWindowsGroups, expirationTime, true)
        {
        }

        public WindowsClaimSet(WindowsIdentity windowsIdentity, string authenticationType, bool includeWindowsGroups, DateTime expirationTime)
            : this( windowsIdentity, authenticationType, includeWindowsGroups, expirationTime, true )
        {
        }

        internal WindowsClaimSet(WindowsIdentity windowsIdentity, string authenticationType, bool includeWindowsGroups, bool clone)
            : this( windowsIdentity, authenticationType, includeWindowsGroups, DateTime.UtcNow.AddHours( 10 ), clone )
        {
        }

        internal WindowsClaimSet(WindowsIdentity windowsIdentity, string authenticationType, bool includeWindowsGroups, DateTime expirationTime, bool clone)
        {
            if (windowsIdentity == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("windowsIdentity");

            this.windowsIdentity = clone ? SecurityUtils.CloneWindowsIdentityIfNecessary(windowsIdentity,  authenticationType) : windowsIdentity;
            this.includeWindowsGroups = includeWindowsGroups;
            this.expirationTime = expirationTime;
            this.authenticationType = authenticationType;
        }

        WindowsClaimSet(WindowsClaimSet from)
            : this(from.WindowsIdentity, from.authenticationType, from.includeWindowsGroups, from.expirationTime, true)
        {
        }

        public override Claim this[int index]
        {
            get
            {
                ThrowIfDisposed();
                EnsureClaims();
                return this.claims[index];
            }
        }

        public override int Count
        {
            get
            {
                ThrowIfDisposed();
                EnsureClaims();
                return this.claims.Count;
            }
        }

        IIdentity IIdentityInfo.Identity
        {
            get
            {
                ThrowIfDisposed();
                return this.windowsIdentity;
            }
        }

        public WindowsIdentity WindowsIdentity
        {
            get
            {
                ThrowIfDisposed();
                return this.windowsIdentity;
            }
        }
       
        public override ClaimSet Issuer
        {
            get { return ClaimSet.Windows; }
        }

        public DateTime ExpirationTime
        {
            get { return this.expirationTime; }
        }

        GroupSidClaimCollection Groups
        {
            get
            {
                if (this.groups == null)
                {
                    this.groups = new GroupSidClaimCollection(this.windowsIdentity);
                }
                return this.groups;
            }
        }

        internal WindowsClaimSet Clone()
        {
            ThrowIfDisposed();
            return new WindowsClaimSet(this);
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.windowsIdentity.Dispose();
            }
        }

        IList<Claim> InitializeClaimsCore()
        {
            if (this.windowsIdentity.Token == IntPtr.Zero)
                return new List<Claim>();

            List<Claim> claims = new List<Claim>(3);
            claims.Add(new Claim(ClaimTypes.Sid, this.windowsIdentity.User, Rights.Identity));
            Claim claim;
            if (TryCreateWindowsSidClaim(this.windowsIdentity, out claim))
            {
                claims.Add(claim);
            }
            claims.Add(Claim.CreateNameClaim(this.windowsIdentity.Name));
            if (this.includeWindowsGroups)
            {
                claims.AddRange(this.Groups);
            }
            return claims;
        }

        void EnsureClaims()
        {
            if (this.claims != null)
                return;

            this.claims = InitializeClaimsCore();
        }

        void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().FullName));
            }
        }

        static bool SupportedClaimType(string claimType)
        {
            return claimType == null ||
                ClaimTypes.Sid == claimType ||
                ClaimTypes.DenyOnlySid == claimType ||
                ClaimTypes.Name == claimType;
        }

        // Note: null string represents any.
        public override IEnumerable<Claim> FindClaims(string claimType, string right)
        {
            ThrowIfDisposed();
            if (!SupportedClaimType(claimType) || !ClaimSet.SupportedRight(right))
            {
                yield break;
            }
            else if (this.claims == null && (ClaimTypes.Sid == claimType || ClaimTypes.DenyOnlySid == claimType))
            {
                if (ClaimTypes.Sid == claimType)
                {
                    if (right == null || Rights.Identity == right)
                    {
                        yield return new Claim(ClaimTypes.Sid, this.windowsIdentity.User, Rights.Identity);
                    }
                }

                if (right == null || Rights.PossessProperty == right)
                {
                    Claim sid;
                    if (TryCreateWindowsSidClaim(this.windowsIdentity, out sid))
                    {
                        if (claimType == sid.ClaimType)
                        {
                            yield return sid;
                        }
                    }
                }

                if (this.includeWindowsGroups && (right == null || Rights.PossessProperty == right))
                {
                    for (int i = 0; i < this.Groups.Count; ++i)
                    {
                        Claim sid = this.Groups[i];
                        if (claimType == sid.ClaimType)
                        {
                            yield return sid;
                        }
                    }
                }
            }
            else
            {
                EnsureClaims();

                bool anyClaimType = (claimType == null);
                bool anyRight = (right == null);

                for (int i = 0; i < this.claims.Count; ++i)
                {
                    Claim claim = this.claims[i];
                    if ((claim != null) &&
                        (anyClaimType || claimType == claim.ClaimType) &&
                        (anyRight || right == claim.Right))
                    {
                        yield return claim;
                    }
                }
            }
        }

        public override IEnumerator<Claim> GetEnumerator()
        {
            ThrowIfDisposed();
            EnsureClaims();
            return this.claims.GetEnumerator();
        }

        public override string ToString()
        {
            return this.disposed ? base.ToString() : SecurityUtils.ClaimSetToString(this);
        }

        class GroupSidClaimCollection : Collection<Claim>
        {
            // Copy from System\Security\Principal\WindowsIdentity.cs
            [Fx.Tag.SecurityNote(Critical = "Uses critical type SafeHGlobalHandle.",
                Safe = "Performs a Demand for full trust.")]
            [SecuritySafeCritical]
            [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
            public GroupSidClaimCollection(WindowsIdentity windowsIdentity)
            {
                if (windowsIdentity.Token != IntPtr.Zero)
                {
                    SafeHGlobalHandle safeAllocHandle = SafeHGlobalHandle.InvalidHandle;
                    try
                    {
                        uint dwLength;
                        safeAllocHandle = GetTokenInformation(windowsIdentity.Token, TokenInformationClass.TokenGroups, out dwLength);
                        int count = Marshal.ReadInt32(safeAllocHandle.DangerousGetHandle());
                        IntPtr pSidAndAttributes = new IntPtr((long)safeAllocHandle.DangerousGetHandle() + (long)Marshal.OffsetOf(typeof(TOKEN_GROUPS), "Groups"));
                        for (int i = 0; i < count; ++i)
                        {
                            SID_AND_ATTRIBUTES group = (SID_AND_ATTRIBUTES)Marshal.PtrToStructure(pSidAndAttributes, typeof(SID_AND_ATTRIBUTES));
                            uint mask = NativeMethods.SE_GROUP_ENABLED | NativeMethods.SE_GROUP_LOGON_ID | NativeMethods.SE_GROUP_USE_FOR_DENY_ONLY;
                            if ((group.Attributes & mask) == NativeMethods.SE_GROUP_ENABLED)
                            {
                                base.Add(Claim.CreateWindowsSidClaim(new SecurityIdentifier(group.Sid)));
                            }
                            else if ((group.Attributes & mask) == NativeMethods.SE_GROUP_USE_FOR_DENY_ONLY)
                            {
                                base.Add(Claim.CreateDenyOnlyWindowsSidClaim(new SecurityIdentifier(group.Sid)));
                            }
                            pSidAndAttributes = new IntPtr((long)pSidAndAttributes + SID_AND_ATTRIBUTES.SizeOf);
                        }
                    }
                    finally
                    {
                        safeAllocHandle.Close();
                    }
                }
            }
        }

        // Copy from System\Security\Principal\WindowsIdentity.cs
        [Fx.Tag.SecurityNote(Critical = "Uses critical type SafeHGlobalHandle.",
            Safe = "Performs a Demand for full trust.")]
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        static SafeHGlobalHandle GetTokenInformation(IntPtr tokenHandle, TokenInformationClass tokenInformationClass, out uint dwLength)
        {
            SafeHGlobalHandle safeAllocHandle = SafeHGlobalHandle.InvalidHandle;
            dwLength = (uint)Marshal.SizeOf(typeof(uint));
            bool result = NativeMethods.GetTokenInformation(tokenHandle,
                                                          (uint)tokenInformationClass,
                                                          safeAllocHandle,
                                                          0,
                                                          out dwLength);
            int dwErrorCode = Marshal.GetLastWin32Error();
            switch (dwErrorCode)
            {
                case NativeMethods.ERROR_BAD_LENGTH:
                // special case for TokenSessionId. Falling through
                case NativeMethods.ERROR_INSUFFICIENT_BUFFER:
                    safeAllocHandle = SafeHGlobalHandle.AllocHGlobal(dwLength);
                    result = NativeMethods.GetTokenInformation(tokenHandle,
                                                             (uint)tokenInformationClass,
                                                             safeAllocHandle,
                                                             dwLength,
                                                             out dwLength);
                    dwErrorCode = Marshal.GetLastWin32Error();
                    if (!result)
                    {
                        safeAllocHandle.Close();
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(dwErrorCode));
                    }
                    break;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(dwErrorCode));
            }
            return safeAllocHandle;
        }

        [Fx.Tag.SecurityNote(Critical = "Uses critical type SafeHGlobalHandle.",
            Safe = "Performs a Demand for full trust.")]
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        static bool TryCreateWindowsSidClaim(WindowsIdentity windowsIdentity, out Claim claim)
        {
            SafeHGlobalHandle safeAllocHandle = SafeHGlobalHandle.InvalidHandle;
            try
            {
                uint dwLength;
                safeAllocHandle = GetTokenInformation(windowsIdentity.Token, TokenInformationClass.TokenUser, out dwLength);
                SID_AND_ATTRIBUTES user = (SID_AND_ATTRIBUTES)Marshal.PtrToStructure(safeAllocHandle.DangerousGetHandle(), typeof(SID_AND_ATTRIBUTES));
                uint mask = NativeMethods.SE_GROUP_USE_FOR_DENY_ONLY;
                if (user.Attributes == 0)
                {
                    claim = Claim.CreateWindowsSidClaim(new SecurityIdentifier(user.Sid));
                    return true;
                }
                else if ((user.Attributes & mask) == NativeMethods.SE_GROUP_USE_FOR_DENY_ONLY)
                {
                    claim = Claim.CreateDenyOnlyWindowsSidClaim(new SecurityIdentifier(user.Sid));
                    return true;
                }
            }
            finally
            {
                safeAllocHandle.Close();
            }
            claim = null;
            return false;
        }
    }
}
