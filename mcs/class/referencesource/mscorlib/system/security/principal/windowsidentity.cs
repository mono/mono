// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// WindowsIdentity.cs
//
// Representation of a process/thread token.
//

namespace System.Security.Principal
{
    using System.Diagnostics.Contracts;    
    using System.Reflection;
    using System.Runtime.CompilerServices;
    #if FEATURE_CORRUPTING_EXCEPTIONS
    using System.Runtime.ExceptionServices;
    #endif // FEATURE_CORRUPTING_EXCEPTIONS
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Versioning;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;

#if !FEATURE_CORECLR
    using System.Security.Claims;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Globalization;
#endif

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public enum WindowsAccountType {
        Normal      = 0,
        Guest       = 1,
        System      = 2,
        Anonymous   = 3
    }

    // Keep in sync with vm\comprincipal.h
    internal enum WinSecurityContext {
        Thread = 1, // OpenAsSelf = false
        Process = 2, // OpenAsSelf = true
        Both = 3 // OpenAsSelf = true, then OpenAsSelf = false
    }

    internal enum ImpersonationQueryResult {
        Impersonated    = 0,    // current thread is impersonated
        NotImpersonated = 1,    // current thread is not impersonated
        Failed          = 2     // failed to query 
    }

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
#if !FEATURE_CORECLR
    public class WindowsIdentity : ClaimsIdentity, ISerializable, IDeserializationCallback, IDisposable {
#else
    public class WindowsIdentity : IIdentity, ISerializable, IDeserializationCallback, IDisposable {
#endif
        [System.Security.SecurityCritical] // auto-generated
        static SafeAccessTokenHandle s_invalidTokenHandle = SafeAccessTokenHandle.InvalidHandle; 
        private string m_name = null;
        private SecurityIdentifier m_owner = null;
        private SecurityIdentifier m_user = null;
        private object m_groups = null;
        [System.Security.SecurityCritical] // auto-generated
        private SafeAccessTokenHandle m_safeTokenHandle = SafeAccessTokenHandle.InvalidHandle;
        private string m_authType = null;
        private int m_isAuthenticated = -1;
        private volatile TokenImpersonationLevel m_impersonationLevel;
        private volatile bool m_impersonationLevelInitialized;
        private static RuntimeConstructorInfo s_specialSerializationCtor;

#if !FEATURE_CORECLR

        [NonSerialized]
        public new const string DefaultIssuer = @"AD AUTHORITY";
        
        [NonSerialized]
        string m_issuerName = DefaultIssuer;
        
        [NonSerialized]
        private object m_claimsIntiailizedLock = new object();
        
        [NonSerialized]
        volatile bool m_claimsInitialized;

        [NonSerialized]
        List<Claim> m_deviceClaims;

        [NonSerialized]
        List<Claim> m_userClaims;

#endif

        //
        // Constructors.
        //
        
        [System.Security.SecuritySafeCritical]  // auto-generated
        static WindowsIdentity()
        {
            s_specialSerializationCtor = typeof(WindowsIdentity).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(SerializationInfo) },
                null) as RuntimeConstructorInfo;
        }

        [System.Security.SecurityCritical]  // auto-generated
#if !FEATURE_CORECLR
        private WindowsIdentity ()
            : base( null, null, null, ClaimTypes.Name, ClaimTypes.GroupSid ) {}
#else
        private WindowsIdentity () {}
#endif

        [System.Security.SecurityCritical]  // auto-generated
        internal WindowsIdentity (SafeAccessTokenHandle safeTokenHandle) : this (safeTokenHandle.DangerousGetHandle(), null, -1) {
            GC.KeepAlive(safeTokenHandle);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public WindowsIdentity (IntPtr userToken) : this (userToken, null, -1) {}

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public WindowsIdentity (IntPtr userToken, string type) : this (userToken, type, -1) {}

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public WindowsIdentity (IntPtr userToken, string type, WindowsAccountType acctType) : this (userToken, type, -1) {}

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public WindowsIdentity (IntPtr userToken, string type, WindowsAccountType acctType, bool isAuthenticated) 
            : this (userToken, type, isAuthenticated ? 1 : 0) {}

        [System.Security.SecurityCritical]  // auto-generated


        private WindowsIdentity (IntPtr userToken, string authType, int isAuthenticated )
#if !FEATURE_CORECLR
            : base(null, null, null, ClaimTypes.Name, ClaimTypes.GroupSid) 
#endif
        {
            CreateFromToken(userToken);
            m_authType = authType;
            m_isAuthenticated = isAuthenticated;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private void CreateFromToken (IntPtr userToken) {
            if (userToken == IntPtr.Zero)
                throw new ArgumentException(Environment.GetResourceString("Argument_TokenZero"));
            Contract.EndContractBlock();

            // Find out if the specified token is a valid.
            uint dwLength = (uint) Marshal.SizeOf(typeof(uint));
            bool result = Win32Native.GetTokenInformation(userToken, (uint) TokenInformationClass.TokenType,
                                                          SafeLocalAllocHandle.InvalidHandle, 0, out dwLength);
            if (Marshal.GetLastWin32Error() == Win32Native.ERROR_INVALID_HANDLE)
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidImpersonationToken"));

            if (!Win32Native.DuplicateHandle(Win32Native.GetCurrentProcess(),
                                             userToken,
                                             Win32Native.GetCurrentProcess(),
                                             ref m_safeTokenHandle,
                                             0,
                                             true,
                                             Win32Native.DUPLICATE_SAME_ACCESS))
                throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public WindowsIdentity (string sUserPrincipalName) : this (sUserPrincipalName, null) {}

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]


        public WindowsIdentity (string sUserPrincipalName, string type )
#if !FEATURE_CORECLR
            : base( null, null, null, ClaimTypes.Name, ClaimTypes.GroupSid )
#endif
        {
            KerbS4ULogon(sUserPrincipalName, ref m_safeTokenHandle);
        }

        //
        // We cannot make sure the token will stay alive
        // until it is being deserialized in another AppDomain. We do not have a way to capture 
        // the state of a token (just a pointer to kernel memory) and re-construct it later
        // and even if we did (via calling NtQueryInformationToken and relying on the token undocumented
        // format), constructing a token requires TCB privilege. We need to address the "serializable" 
        // nature of WindowsIdentity since it is not obvious that can be achieved at all.
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.SerializationFormatter )]
        public WindowsIdentity (SerializationInfo info, StreamingContext context) : this(info)
        {
        }

        // This is a copy of the serialization constructor above but without the
        // security demand that's slow and breaks partial trust scenarios
        // without an expensive assert in place in the remoting code. Instead we
        // special case this class and call the private constructor directly
        // (changing the demand above is considered a breaking change, even
        // though nobody else should have been using a serialization constructor
        // directly).
        [System.Security.SecurityCritical]  // auto-generated
        private WindowsIdentity(SerializationInfo info)
#if !FEATURE_CORECLR
            : base(info)
#endif
        {

            #if !FEATURE_CORECLR
            m_claimsInitialized = false;
            #endif

            IntPtr userToken = (IntPtr) info.GetValue("m_userToken", typeof(IntPtr));
            if (userToken != IntPtr.Zero)
                CreateFromToken(userToken);
        }

        /// <internalonly/>
        [System.Security.SecurityCritical]  // auto-generated_required
        void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) {
#if !FEATURE_CORECLR
                base.GetObjectData(info, context);
#endif
            info.AddValue("m_userToken", m_safeTokenHandle.DangerousGetHandle());
        }

        /// <internalonly/>
        void IDeserializationCallback.OnDeserialization (Object sender) {}

        //
        // Factory methods.
        //

        
        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public static WindowsIdentity GetCurrent () {
           return GetCurrentInternal(TokenAccessLevels.MaximumAllowed, false);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public static WindowsIdentity GetCurrent (bool ifImpersonating) {
           return GetCurrentInternal(TokenAccessLevels.MaximumAllowed, ifImpersonating);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public static WindowsIdentity GetCurrent (TokenAccessLevels desiredAccess) {
           return GetCurrentInternal(desiredAccess, false);
        }

        // GetAnonymous() is used heavily in ASP.NET requests as a dummy identity to indicate
        // the request is anonymous. It does not represent a real process or thread token so
        // it cannot impersonate or do anything useful. Note this identity does not represent the
        // usual concept of an anonymous token, and the name is simply misleading but we cannot change it now.

        [System.Security.SecuritySafeCritical]  // auto-generated
        public static WindowsIdentity GetAnonymous () {
            return new WindowsIdentity();
        }

        //
        // Properties.
        //
        // this is defined 'override sealed' for back compat. Il generated is 'virtual final' and this needs to be the same.
#if !FEATURE_CORECLR
        public override sealed string AuthenticationType {
#else
        public string AuthenticationType {
#endif
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                // If this is an anonymous identity, return an empty string
                if (m_safeTokenHandle.IsInvalid)
                    return String.Empty;

                if (m_authType == null) {
                    Win32Native.LUID authId = GetLogonAuthId(m_safeTokenHandle);
                    if (authId.LowPart == Win32Native.ANONYMOUS_LOGON_LUID)
                        return String.Empty; // no authentication, just return an empty string

                    SafeLsaReturnBufferHandle pLogonSessionData = SafeLsaReturnBufferHandle.InvalidHandle;
                    try {
                        int status = Win32Native.LsaGetLogonSessionData(ref authId, ref pLogonSessionData);
                    if (status < 0) // non-negative numbers indicate success
                        throw GetExceptionFromNtStatus(status);

                        pLogonSessionData.Initialize((uint)Marshal.SizeOf(typeof(Win32Native.SECURITY_LOGON_SESSION_DATA)));

                        Win32Native.SECURITY_LOGON_SESSION_DATA logonSessionData = pLogonSessionData.Read<Win32Native.SECURITY_LOGON_SESSION_DATA>(0);
                        return Marshal.PtrToStringUni(logonSessionData.AuthenticationPackage.Buffer);
                    }
                    finally {
                        if (!pLogonSessionData.IsInvalid)
                            pLogonSessionData.Dispose();
                    }
                }

                return m_authType;
            }
        }


        [ComVisible(false)]
        public TokenImpersonationLevel ImpersonationLevel {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                // If two threads ---- here, they'll both set m_impersonationLevel to the same value,
                // which is ok.
                if (!m_impersonationLevelInitialized) {
                    TokenImpersonationLevel impersonationLevel = TokenImpersonationLevel.None;
                    // If this is an anonymous identity
                    if (m_safeTokenHandle.IsInvalid) {
                        impersonationLevel = TokenImpersonationLevel.Anonymous;
                    }
                    else {
                        TokenType tokenType = (TokenType)GetTokenInformation<int>(TokenInformationClass.TokenType);
                        if (tokenType == TokenType.TokenPrimary) {
                            impersonationLevel = TokenImpersonationLevel.None; // primary token;
                        }
                        else {
                            /// This is an impersonation token, get the impersonation level
                            int level = GetTokenInformation<int>(TokenInformationClass.TokenImpersonationLevel);
                            impersonationLevel = (TokenImpersonationLevel)level + 1;
                        }
                    }

                    m_impersonationLevel = impersonationLevel;
                    m_impersonationLevelInitialized = true;
                }

                return m_impersonationLevel;
            }
        }

#if !FEATURE_CORECLR
        public override bool IsAuthenticated {
#else
        public virtual bool IsAuthenticated {
#endif

            get {
                if (m_isAuthenticated == -1) {
                    // There is a known bug where this approach will not work correctly for domain guests (will return false
                    // instead of true). But this is a corner-case that is not very interesting.
#if !FEATURE_CORECLR
                    m_isAuthenticated = CheckNtTokenForSid(new SecurityIdentifier(IdentifierAuthority.NTAuthority,
                                                                    new int[] { Win32Native.SECURITY_AUTHENTICATED_USER_RID })) ? 1 : 0;
#else                    
                    WindowsPrincipal wp = new WindowsPrincipal(this);
                    SecurityIdentifier sid = new SecurityIdentifier(IdentifierAuthority.NTAuthority,
                                                                    new int[] {Win32Native.SECURITY_AUTHENTICATED_USER_RID});
                    m_isAuthenticated = wp.IsInRole(sid) ? 1 : 0;
#endif

                }

                return m_isAuthenticated == 1;
            }
        }

#if !FEATURE_CORECLR
        [System.Security.SecuritySafeCritical]
        [ComVisible(false)]
        bool CheckNtTokenForSid (SecurityIdentifier sid) {

            Contract.EndContractBlock();

            // special case the anonymous identity.
            if (m_safeTokenHandle.IsInvalid)
                return false;

            // CheckTokenMembership expects an impersonation token
            SafeAccessTokenHandle token = SafeAccessTokenHandle.InvalidHandle;
            TokenImpersonationLevel til = ImpersonationLevel;
            bool isMember = false;

            try {
                if (til == TokenImpersonationLevel.None) {
                    if (!Win32Native.DuplicateTokenEx(m_safeTokenHandle,
                                                      (uint) TokenAccessLevels.Query,
                                                      IntPtr.Zero,
                                                      (uint) TokenImpersonationLevel.Identification,
                                                      (uint) TokenType.TokenImpersonation,
                                                      ref token))
                        throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
                }

                
                // CheckTokenMembership will check if the SID is both present and enabled in the access token.
                if (!Win32Native.CheckTokenMembership((til != TokenImpersonationLevel.None ? m_safeTokenHandle : token),
                                                      sid.BinaryForm,
                                                      ref isMember))
                    throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
            }
            finally {
                if (token != SafeAccessTokenHandle.InvalidHandle) {
                    token.Dispose();
                }
            }

            return isMember;
        }
#endif

        //
        // IsGuest, IsSystem and IsAnonymous are maintained for compatibility reasons. It is always
        // possible to extract this same information from the User SID property and the new
        // (and more general) methods defined in the SID class (IsWellKnown, etc...).
        //

        public virtual bool IsGuest {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                // special case the anonymous identity.
                if (m_safeTokenHandle.IsInvalid)
                    return false;

#if !FEATURE_CORECLR
                return CheckNtTokenForSid(new SecurityIdentifier(IdentifierAuthority.NTAuthority,
                                                new int[] { Win32Native.SECURITY_BUILTIN_DOMAIN_RID, (int)WindowsBuiltInRole.Guest }));
#else
                WindowsPrincipal principal = new WindowsPrincipal(this);
                return principal.IsInRole(WindowsBuiltInRole.Guest);
#endif

            }
        }

        public virtual bool IsSystem {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                // special case the anonymous identity.
                if (m_safeTokenHandle.IsInvalid)
                    return false;
                SecurityIdentifier sid = new SecurityIdentifier(IdentifierAuthority.NTAuthority, 
                                                                new int[] {Win32Native.SECURITY_LOCAL_SYSTEM_RID});
                return (this.User == sid);
            }
        }

        public virtual bool IsAnonymous {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                // special case the anonymous identity.
                if (m_safeTokenHandle.IsInvalid)
                    return true;
                SecurityIdentifier sid = new SecurityIdentifier(IdentifierAuthority.NTAuthority, 
                                                                new int[] {Win32Native.SECURITY_ANONYMOUS_LOGON_RID});
                return (this.User == sid);
            }
        }

#if !FEATURE_CORECLR
        public override string Name {
#else
        public virtual string Name {
#endif
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                return GetName();
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        [DynamicSecurityMethodAttribute()]
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        internal String GetName()
        {
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            // special case the anonymous identity.
            if (m_safeTokenHandle.IsInvalid)
                return String.Empty;
            
            if (m_name == null) 
            {
                // revert thread impersonation for the duration of the call to get the name.
                using (SafeRevertToSelf(ref stackMark))  
                {
                    NTAccount ntAccount = this.User.Translate(typeof(NTAccount)) as NTAccount;
                    m_name = ntAccount.ToString();
                }
            }
            
            return m_name;
        }

        [ComVisible(false)]
        public SecurityIdentifier Owner {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                // special case the anonymous identity.
                if (m_safeTokenHandle.IsInvalid)
                    return null;

                if (m_owner == null) {
                    using (SafeLocalAllocHandle tokenOwner = GetTokenInformation(m_safeTokenHandle, TokenInformationClass.TokenOwner)) {
                        m_owner = new SecurityIdentifier(tokenOwner.Read<IntPtr>(0), true);
                    }
                }

                return m_owner;
            }
        }

        [ComVisible(false)]
        public SecurityIdentifier User {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                // special case the anonymous identity.
                if (m_safeTokenHandle.IsInvalid)
                    return null;

                if (m_user == null) {
                    using (SafeLocalAllocHandle tokenUser = GetTokenInformation(m_safeTokenHandle, TokenInformationClass.TokenUser)) {
                        m_user = new SecurityIdentifier(tokenUser.Read<IntPtr>(0), true);
                    }
                }

                return m_user;
            }
        }

        public IdentityReferenceCollection Groups {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                // special case the anonymous identity.
                if (m_safeTokenHandle.IsInvalid)
                    return null;

                if (m_groups == null) {
                    IdentityReferenceCollection groups = new IdentityReferenceCollection();
                    using (SafeLocalAllocHandle pGroups = GetTokenInformation(m_safeTokenHandle, TokenInformationClass.TokenGroups)) {

                        uint groupCount = pGroups.Read<uint>(0); 
                        // Work-around bug on WS03 that only populates the GroupCount field of TOKEN_GROUPS if the count is 0
                        // In that situation, attempting to read the entire TOKEN_GROUPS structure will lead to InsufficientBuffer exception 
                        // since the field is only 4 bytes long (uint only, for GroupCount), but we try to read more (including the pointer to GroupDetails).
                        if (groupCount != 0)
                        {

                            Win32Native.TOKEN_GROUPS tokenGroups = pGroups.Read<Win32Native.TOKEN_GROUPS>(0);
                            Win32Native.SID_AND_ATTRIBUTES[] groupDetails = new Win32Native.SID_AND_ATTRIBUTES[tokenGroups.GroupCount];

                            pGroups.ReadArray((uint)Marshal.OffsetOf(typeof(Win32Native.TOKEN_GROUPS), "Groups").ToInt32(),
                                              groupDetails,
                                              0,
                                              groupDetails.Length);

                            foreach (Win32Native.SID_AND_ATTRIBUTES group in groupDetails)
                            {
                                // Ignore disabled, logon ID, and deny-only groups.
                                uint mask = Win32Native.SE_GROUP_ENABLED | Win32Native.SE_GROUP_LOGON_ID | Win32Native.SE_GROUP_USE_FOR_DENY_ONLY;
                                if ((group.Attributes & mask) == Win32Native.SE_GROUP_ENABLED) {
                                    groups.Add(new SecurityIdentifier(group.Sid, true ));
                                }
                            }
                        }
                    }
                    Interlocked.CompareExchange(ref m_groups, groups, null);
                }

                return m_groups as IdentityReferenceCollection;
            }
        }

        //
        // Note this property does not duplicate the token. This is also the same as V1/Everett behaviour.
        //

        public virtual IntPtr Token {
            [System.Security.SecuritySafeCritical]  // auto-generated
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get {
                return m_safeTokenHandle.DangerousGetHandle();
            }
        }

        //
        // Public methods.
        //
        [SecuritySafeCritical]
        [DynamicSecurityMethodAttribute()]
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        public static void RunImpersonated(SafeAccessTokenHandle safeAccessTokenHandle, Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;

            WindowsIdentity wi = null;
            if (!safeAccessTokenHandle.IsInvalid)
                wi = new WindowsIdentity(safeAccessTokenHandle);

            using (WindowsImpersonationContext wiContext = SafeImpersonate(safeAccessTokenHandle, wi, ref stackMark))
            {
                action();
            }
        }

        [SecuritySafeCritical]
        [DynamicSecurityMethodAttribute()]
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        public static T RunImpersonated<T>(SafeAccessTokenHandle safeAccessTokenHandle, Func<T> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");

            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;

            WindowsIdentity wi = null;
            if (!safeAccessTokenHandle.IsInvalid)
                wi = new WindowsIdentity(safeAccessTokenHandle);

            T result = default(T);
            using (WindowsImpersonationContext wiContext = SafeImpersonate(safeAccessTokenHandle, wi, ref stackMark))
            {
                result = func();
            }

            return result;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [DynamicSecurityMethodAttribute()]
        [ResourceExposure(ResourceScope.Process)]  // Call from within a CER, or use a RunAsUser helper.
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        public virtual WindowsImpersonationContext Impersonate () 
        {
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return Impersonate(ref stackMark);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal | SecurityPermissionFlag.UnmanagedCode)]
        [DynamicSecurityMethodAttribute()]
        [ResourceExposure(ResourceScope.Process)]  // Call from within a CER, or use a RunAsUser helper.
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        public static WindowsImpersonationContext Impersonate (IntPtr userToken) 
        {
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            if (userToken == IntPtr.Zero)
                return SafeRevertToSelf(ref stackMark);

            WindowsIdentity wi = new WindowsIdentity(userToken, null, -1);
            return wi.Impersonate(ref stackMark);
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal WindowsImpersonationContext Impersonate (ref StackCrawlMark stackMark) {
            if (m_safeTokenHandle.IsInvalid)
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_AnonymousCannotImpersonate"));

            return SafeImpersonate(m_safeTokenHandle, this, ref stackMark);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ComVisible(false)]
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (m_safeTokenHandle != null && !m_safeTokenHandle.IsClosed)
                    m_safeTokenHandle.Dispose();
            }
            m_name = null;
            m_owner = null;
            m_user = null;
        }

        [ComVisible(false)]
        public void Dispose() {
            Dispose(true);
        }

        public SafeAccessTokenHandle AccessToken {
            [System.Security.SecurityCritical]  // auto-generated
            get {
                return m_safeTokenHandle;
            }
        }

        //
        // internal.
        //

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        internal static WindowsImpersonationContext SafeRevertToSelf(ref StackCrawlMark stackMark) 
        {
            return SafeImpersonate(s_invalidTokenHandle, null, ref stackMark);
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        internal static WindowsImpersonationContext SafeImpersonate (SafeAccessTokenHandle userToken, WindowsIdentity wi, ref StackCrawlMark stackMark) 
        {
            bool isImpersonating;
            int hr = 0;
            SafeAccessTokenHandle safeTokenHandle = GetCurrentToken(TokenAccessLevels.MaximumAllowed, false, out isImpersonating, out hr);
            if (safeTokenHandle == null || safeTokenHandle.IsInvalid)
                throw new SecurityException(Win32Native.GetMessage(hr));

            // Set the SafeAccessTokenHandle on the FSD:
            FrameSecurityDescriptor secObj = SecurityRuntime.GetSecurityObjectForFrame(ref stackMark, true);
            if (secObj == null)
            {
                // Security: REQ_SQ flag is missing. Bad compiler ?
                // This can happen when you create delegates over functions that need the REQ_SQ 
                throw new SecurityException(Environment.GetResourceString( "ExecutionEngine_MissingSecurityDescriptor" ) );
            }
                
            WindowsImpersonationContext context = new WindowsImpersonationContext(safeTokenHandle, GetCurrentThreadWI(), isImpersonating, secObj);

            if (userToken.IsInvalid) { // impersonating a zero token means clear the token on the thread
                hr = Win32.RevertToSelf();
                if (hr < 0)
                    Environment.FailFast(Win32Native.GetMessage(hr));
                // update identity on the thread
                UpdateThreadWI(wi);
                secObj.SetTokenHandles(safeTokenHandle, (wi == null?null:wi.AccessToken));
            } else {
                hr = Win32.RevertToSelf();
                if (hr < 0)
                        Environment.FailFast(Win32Native.GetMessage(hr));
                hr = Win32.ImpersonateLoggedOnUser(userToken);
                if (hr < 0) {
                    context.Undo();
                    throw new SecurityException(Environment.GetResourceString("Argument_ImpersonateUser"));
                }
                UpdateThreadWI(wi);
                secObj.SetTokenHandles(safeTokenHandle, (wi == null?null:wi.AccessToken));
            }

            return context;
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static WindowsIdentity GetCurrentThreadWI()
        {
            return SecurityContext.GetCurrentWI(Thread.CurrentThread.GetExecutionContextReader());
        }

        [SecurityCritical]
        internal static void UpdateThreadWI(WindowsIdentity wi)
        {
            // Set WI on Thread.CurrentThread.ExecutionContext.SecurityContext
            Thread currentThread = Thread.CurrentThread;
            if (currentThread.GetExecutionContextReader().SecurityContext.WindowsIdentity != wi)
            {
                ExecutionContext ec = currentThread.GetMutableExecutionContext(); 
                SecurityContext sc = ec.SecurityContext;
                if (wi != null && sc == null)
                {
                    // create a new security context on the thread
                    sc = new SecurityContext();
                    ec.SecurityContext = sc;
                }

                if (sc != null) // null-check needed here since we will not create an sc if wi is null
                {
                    sc.WindowsIdentity = wi;
                }
            }
        }


        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        internal static WindowsIdentity GetCurrentInternal (TokenAccessLevels desiredAccess, bool threadOnly) {
            int hr = 0;
            bool isImpersonating;
            SafeAccessTokenHandle safeTokenHandle = GetCurrentToken(desiredAccess, threadOnly, out isImpersonating, out hr);
            if (safeTokenHandle == null || safeTokenHandle.IsInvalid) {
                // either we wanted only ThreadToken - return null
                if (threadOnly && !isImpersonating)
                    return null;
                // or there was an error
                throw new SecurityException(Win32Native.GetMessage(hr));
            }
            WindowsIdentity wi = new WindowsIdentity();
            wi.m_safeTokenHandle.Dispose();
            wi.m_safeTokenHandle = safeTokenHandle;
            return wi;
        }

        internal static RuntimeConstructorInfo GetSpecialSerializationCtor()
        {
            return s_specialSerializationCtor;
        }

        //
        // private.
        //

        private static int GetHRForWin32Error (int dwLastError) {
            if ((dwLastError & 0x80000000) == 0x80000000)
                return dwLastError;
            else
                return (dwLastError & 0x0000FFFF) | unchecked((int)0x80070000);
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static Exception GetExceptionFromNtStatus (int status) {
            if ((uint) status == Win32Native.STATUS_ACCESS_DENIED)
                return new UnauthorizedAccessException();

            if ((uint) status == Win32Native.STATUS_INSUFFICIENT_RESOURCES || (uint) status == Win32Native.STATUS_NO_MEMORY)
                return new OutOfMemoryException();

            int win32ErrorCode = Win32Native.LsaNtStatusToWinError(status);
            return new SecurityException(Win32Native.GetMessage(win32ErrorCode));
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        private static SafeAccessTokenHandle GetCurrentToken(TokenAccessLevels desiredAccess, bool threadOnly, out bool isImpersonating, out int hr) {
            isImpersonating = true;
            SafeAccessTokenHandle safeTokenHandle = GetCurrentThreadToken(desiredAccess, out hr);
            if (safeTokenHandle == null && hr == GetHRForWin32Error(Win32Native.ERROR_NO_TOKEN)) {
                // No impersonation
                isImpersonating = false;
                if (!threadOnly)
                    safeTokenHandle = GetCurrentProcessToken(desiredAccess, out hr);
            }
            return safeTokenHandle;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        private static SafeAccessTokenHandle GetCurrentProcessToken (TokenAccessLevels desiredAccess, out int hr) {
            hr = 0;
            SafeAccessTokenHandle safeTokenHandle;
            if (!Win32Native.OpenProcessToken(Win32Native.GetCurrentProcess(), desiredAccess, out safeTokenHandle))
                hr = GetHRForWin32Error(Marshal.GetLastWin32Error());
            return safeTokenHandle;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        internal static SafeAccessTokenHandle GetCurrentThreadToken(TokenAccessLevels desiredAccess, out int hr) {
            SafeAccessTokenHandle safeTokenHandle;
            hr = Win32.OpenThreadToken(desiredAccess, WinSecurityContext.Both, out safeTokenHandle);
            return safeTokenHandle;
        }

        /// <summary>
        ///     Get a property from the current token
        /// </summary>
        [System.Security.SecurityCritical]  // auto-generated
        private T GetTokenInformation<T>(TokenInformationClass tokenInformationClass) where T : struct{
            Contract.Assert(!m_safeTokenHandle.IsInvalid && !m_safeTokenHandle.IsClosed, "!m_safeTokenHandle.IsInvalid && !m_safeTokenHandle.IsClosed");

            using (SafeLocalAllocHandle information = GetTokenInformation(m_safeTokenHandle, tokenInformationClass)) {
                Contract.Assert(information.ByteLength >= (ulong)Marshal.SizeOf(typeof(T)),
                                "information.ByteLength >= (ulong)Marshal.SizeOf(typeof(T))");

                return information.Read<T>(0);
            }
        }

        //
        // QueryImpersonation used to test if the current thread is impersonated.
        // This method doesn't return the thread token (WindowsIdentity).
        // Although GetCurrentInternal can be used to perform the same test but 
        // QueryImpersonation is optimized for the perf.
        // 
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        internal static ImpersonationQueryResult QueryImpersonation() {
            SafeAccessTokenHandle safeTokenHandle = null;
            int hr = Win32.OpenThreadToken(TokenAccessLevels.Query,  WinSecurityContext.Thread, out safeTokenHandle);
                        
            if (safeTokenHandle != null) {
                Contract.Assert(hr == 0, "[WindowsIdentity..QueryImpersonation] - hr == 0");
                safeTokenHandle.Close();
                return ImpersonationQueryResult.Impersonated;
            } 

            if (hr == GetHRForWin32Error(Win32Native.ERROR_ACCESS_DENIED)) {
                // thread is impersonated because the thread was there (and we failed to open it).
                return ImpersonationQueryResult.Impersonated;
            } 
            
            if (hr == GetHRForWin32Error(Win32Native.ERROR_NO_TOKEN)) {
                // definitely not impersonating
                return ImpersonationQueryResult.NotImpersonated;
            }
            
            // Unexpected failure.
            return ImpersonationQueryResult.Failed;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static Win32Native.LUID GetLogonAuthId (SafeAccessTokenHandle safeTokenHandle) {
            using (SafeLocalAllocHandle pStatistics = GetTokenInformation(safeTokenHandle, TokenInformationClass.TokenStatistics)) {
                Win32Native.TOKEN_STATISTICS statistics = pStatistics.Read<Win32Native.TOKEN_STATISTICS>(0);
            return statistics.AuthenticationId;
        }
        }

        [System.Security.SecurityCritical]
        private static SafeLocalAllocHandle GetTokenInformation (SafeAccessTokenHandle tokenHandle, TokenInformationClass tokenInformationClass) {
            SafeLocalAllocHandle safeLocalAllocHandle = SafeLocalAllocHandle.InvalidHandle;
            uint dwLength = (uint) Marshal.SizeOf(typeof(uint));
            bool result = Win32Native.GetTokenInformation(tokenHandle, 
                                                          (uint) tokenInformationClass, 
                                                          safeLocalAllocHandle, 
                                                          0, 
                                                          out dwLength);
            int dwErrorCode = Marshal.GetLastWin32Error();
            switch (dwErrorCode) {
            case Win32Native.ERROR_BAD_LENGTH:
                // special case for TokenSessionId. Falling through
            case Win32Native.ERROR_INSUFFICIENT_BUFFER:
                // ptrLength is an [In] param to LocalAlloc 
                UIntPtr ptrLength = new UIntPtr(dwLength);
                safeLocalAllocHandle.Dispose();
                safeLocalAllocHandle = Win32Native.LocalAlloc(Win32Native.LMEM_FIXED, ptrLength);
                if (safeLocalAllocHandle == null || safeLocalAllocHandle.IsInvalid) 
                    throw new OutOfMemoryException();
                safeLocalAllocHandle.Initialize(dwLength);

                result = Win32Native.GetTokenInformation(tokenHandle, 
                                                         (uint) tokenInformationClass, 
                                                         safeLocalAllocHandle, 
                                                         dwLength, 
                                                         out dwLength);
                if (!result)
                    throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
                break;
            case Win32Native.ERROR_INVALID_HANDLE:
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidImpersonationToken"));
            default:
                throw new SecurityException(Win32Native.GetMessage(dwErrorCode));
            }
            return safeLocalAllocHandle;
        }

        [System.Security.SecurityCritical]  // auto-generated
#if FEATURE_CORRUPTING_EXCEPTIONS
        [HandleProcessCorruptedStateExceptions] // 
#endif // FEATURE_CORRUPTING_EXCEPTIONS
        private unsafe static SafeAccessTokenHandle KerbS4ULogon (string upn, ref SafeAccessTokenHandle safeTokenHandle)
        {
            // source name
            byte[] sourceName = new byte[] { (byte)'C', (byte)'L', (byte)'R' }; // we set the source name to "CLR".

            // ptrLength is an [In] param to LocalAlloc 
            UIntPtr ptrLength = new UIntPtr((uint)(sourceName.Length + 1));

            using (SafeLocalAllocHandle pSourceName = Win32Native.LocalAlloc(Win32Native.LPTR, ptrLength)) {
                if (pSourceName == null || pSourceName.IsInvalid)
                    throw new OutOfMemoryException();

                pSourceName.Initialize((ulong)sourceName.Length + 1);
                pSourceName.WriteArray(0, sourceName, 0, sourceName.Length);
                Win32Native.UNICODE_INTPTR_STRING Name = new Win32Native.UNICODE_INTPTR_STRING(sourceName.Length,
                                                                                               pSourceName);

                int status;
                SafeLsaLogonProcessHandle logonHandle = SafeLsaLogonProcessHandle.InvalidHandle;
                SafeLsaReturnBufferHandle profile = SafeLsaReturnBufferHandle.InvalidHandle;
                try {
                    Privilege privilege = null;

                    RuntimeHelpers.PrepareConstrainedRegions();
                    // Try to get an impersonation token.
                    try {
                        // Try to enable the TCB privilege if possible
                        try {
                            privilege = new Privilege("SeTcbPrivilege");
                            privilege.Enable();
                        }
                        catch (PrivilegeNotHeldException) { }

                        IntPtr dummy = IntPtr.Zero;
                        status = Win32Native.LsaRegisterLogonProcess(ref Name, ref logonHandle, ref dummy);
                        if (Win32Native.ERROR_ACCESS_DENIED == Win32Native.LsaNtStatusToWinError(status)) {
                            // We don't have the Tcb privilege. The best we can hope for is to get an Identification token.
                            status = Win32Native.LsaConnectUntrusted(ref logonHandle);
                        }
                    }
                    catch {
                        // protect against exception filter-based luring attacks
                        if (privilege != null)
                            privilege.Revert();
                        throw;
                    }
                    finally {
                        if (privilege != null)
                            privilege.Revert();
                    }
                    if (status < 0) // non-negative numbers indicate success
                        throw GetExceptionFromNtStatus(status);

                    // package name ("Kerberos")
                    byte[] arrayPackageName = new byte[Win32Native.MICROSOFT_KERBEROS_NAME.Length + 1];
                    Encoding.ASCII.GetBytes(Win32Native.MICROSOFT_KERBEROS_NAME, 0, Win32Native.MICROSOFT_KERBEROS_NAME.Length, arrayPackageName, 0);

                    // ptrLength is an [In] param to LocalAlloc 
                    ptrLength = new UIntPtr((uint)arrayPackageName.Length);
                    using (SafeLocalAllocHandle pPackageName = Win32Native.LocalAlloc(Win32Native.LMEM_FIXED, ptrLength)) {
                        if (pPackageName == null || pPackageName.IsInvalid)
                            throw new OutOfMemoryException();
                        pPackageName.Initialize((ulong)(uint)arrayPackageName.Length);

                        pPackageName.WriteArray(0, arrayPackageName, 0, arrayPackageName.Length);
                        Win32Native.UNICODE_INTPTR_STRING PackageName = new Win32Native.UNICODE_INTPTR_STRING(Win32Native.MICROSOFT_KERBEROS_NAME.Length,
                                                                                                              pPackageName);
                        uint packageId = 0;
                        status = Win32Native.LsaLookupAuthenticationPackage(logonHandle, ref PackageName, ref packageId);
                        if (status < 0) // non-negative numbers indicate success
                            throw GetExceptionFromNtStatus(status);

                        // source context
                        Win32Native.TOKEN_SOURCE sourceContext = new Win32Native.TOKEN_SOURCE();
                        if (!Win32Native.AllocateLocallyUniqueId(ref sourceContext.SourceIdentifier))
                            throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
                        sourceContext.Name = new char[8];
                        sourceContext.Name[0] = 'C'; sourceContext.Name[1] = 'L'; sourceContext.Name[2] = 'R';

                        uint profileSize = 0;
                        Win32Native.LUID logonId = new Win32Native.LUID();
                        Win32Native.QUOTA_LIMITS quotas = new Win32Native.QUOTA_LIMITS();
                        int subStatus = 0;

                        //
                        // Build the KERB_S4U_LOGON structure.  Note that the LSA expects this entire
                        // structure to be contained within the same block of memory, so we need to allocate
                        // enough room for both the structure itself and the UPN string in a single buffer
                        // and do the marshalling into this buffer by hand.
                        //

                        byte[] upnBytes = Encoding.Unicode.GetBytes(upn);

                        Contract.Assert(Marshal.SizeOf(typeof(Win32Native.KERB_S4U_LOGON)) % IntPtr.Size == 0, "Potential allignment issue setting up S4U logon buffer");
                        uint logonInfoSize = (uint) (Marshal.SizeOf(typeof(Win32Native.KERB_S4U_LOGON)) + upnBytes.Length);
                        using (SafeLocalAllocHandle logonInfoBuffer = Win32Native.LocalAlloc(Win32Native.LPTR, new UIntPtr(logonInfoSize))) {
                            if (logonInfoBuffer == null || logonInfoBuffer.IsInvalid) {
                                throw new OutOfMemoryException();
                            }

                            logonInfoBuffer.Initialize((ulong)logonInfoSize);

                            // Write the UPN to the end of the serialized buffer
                            ulong upnOffset = (ulong)Marshal.SizeOf(typeof(Win32Native.KERB_S4U_LOGON));
                            logonInfoBuffer.WriteArray(upnOffset,
                                                       upnBytes,
                                                       0,
                                                       upnBytes.Length);

                            unsafe {
                                byte* pLogonInfoBuffer = null;

                                RuntimeHelpers.PrepareConstrainedRegions();
                                try {
                                    logonInfoBuffer.AcquirePointer(ref pLogonInfoBuffer);

                                    // Setup the KERB_S4U_LOGON structure
                                    Win32Native.KERB_S4U_LOGON logonInfo = new Win32Native.KERB_S4U_LOGON();
                                    logonInfo.MessageType = (uint)KerbLogonSubmitType.KerbS4ULogon;
                                    logonInfo.Flags = 0;

                                    // Point the ClientUpn at the UPN written at the end of this buffer
                                    logonInfo.ClientUpn = new Win32Native.UNICODE_INTPTR_STRING(upnBytes.Length,
                                                                                                new IntPtr(pLogonInfoBuffer + upnOffset));

                                    logonInfoBuffer.Write(0, logonInfo);

                                    // logon user
                                    status = Win32Native.LsaLogonUser(logonHandle,
                                                                      ref Name,
                                                                      (uint)SecurityLogonType.Network,
                                                                      packageId,
                                                                                          new IntPtr(pLogonInfoBuffer),
                                                                                          (uint)logonInfoBuffer.ByteLength,
                                                                      IntPtr.Zero,
                                                                      ref sourceContext,
                                                                      ref profile,
                                                                      ref profileSize,
                                                                      ref logonId,
                                                                      ref safeTokenHandle,
                                                                      ref quotas,
                                                                      ref subStatus);

                                    // If both status and substatus are < 0, substatus is preferred.
                                    if (status == Win32Native.STATUS_ACCOUNT_RESTRICTION && subStatus < 0)
                                        status = subStatus;
                                    if (status < 0) // non-negative numbers indicate success
                                        throw GetExceptionFromNtStatus(status);
                                    if (subStatus < 0) // non-negative numbers indicate success
                                        throw GetExceptionFromNtStatus(subStatus);
                                }
                                finally {
                                    if (pLogonInfoBuffer != null) {
                                        logonInfoBuffer.ReleasePointer();
                                    }
                                }
                            }
                        }

                        return safeTokenHandle;
                    }
                }
                finally {
                    if (!logonHandle.IsInvalid)
                        logonHandle.Dispose();
                    if (!profile.IsInvalid)
                        profile.Dispose();
                }
            }
        }
    
#if !FEATURE_CORECLR

        [SecuritySafeCritical]
        protected WindowsIdentity (WindowsIdentity identity)
            : base( identity, null, identity.m_authType, null, null, false )
        {
            if (identity == null)
                throw new ArgumentNullException("identity");

            Contract.EndContractBlock();

            bool mustDecrement = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (!identity.m_safeTokenHandle.IsInvalid && identity.m_safeTokenHandle != SafeAccessTokenHandle.InvalidHandle && identity.m_safeTokenHandle.DangerousGetHandle() != IntPtr.Zero)
                {
                    identity.m_safeTokenHandle.DangerousAddRef(ref mustDecrement);

                    if (!identity.m_safeTokenHandle.IsInvalid && identity.m_safeTokenHandle.DangerousGetHandle() != IntPtr.Zero)
                        CreateFromToken(identity.m_safeTokenHandle.DangerousGetHandle());

                    m_authType = identity.m_authType;
                    m_isAuthenticated = identity.m_isAuthenticated;
                }
            }
            finally
            {
                if (mustDecrement)
                    identity.m_safeTokenHandle.DangerousRelease();
            }
        }
        
        [SecurityCritical]
        internal IntPtr GetTokenInternal()
        {
            return m_safeTokenHandle.DangerousGetHandle();
        }

        [SecurityCritical]
        internal WindowsIdentity(ClaimsIdentity claimsIdentity, IntPtr userToken)
            : base(claimsIdentity)
        {
            if (userToken != IntPtr.Zero && userToken.ToInt64() > 0)
            {
                CreateFromToken(userToken);
            }
        }

        /// <summary>
        /// Returns a new instance of the base, used when serializing the WindowsIdentity.
        /// </summary>
        internal ClaimsIdentity CloneAsBase()
        {
            return base.Clone();
        }

        /// <summary>
        /// Returns a new instance of <see cref="WindowsIdentity"/> with values copied from this object.
        /// </summary>
        public override ClaimsIdentity Clone()
        {
            return new WindowsIdentity(this);
        }

        /// <summary>
        /// Gets the 'User Claims' from the NTToken that represents this identity
        /// </summary>
        public virtual IEnumerable<Claim> UserClaims
        {
            get
            {
                InitializeClaims();

                return m_userClaims.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the 'Device Claims' from the NTToken that represents the device the identity is using
        /// </summary>
        public virtual IEnumerable<Claim> DeviceClaims
        {
            get
            {
                InitializeClaims();

                return m_deviceClaims.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the claims as <see cref="IEnumerable{Claim}"/>, associated with this <see cref="WindowsIdentity"/>.
        /// Includes UserClaims and DeviceClaims.
        /// </summary>
        public override IEnumerable<Claim> Claims
        {
            get 
            {
                if (!m_claimsInitialized)
                {
                    InitializeClaims();
                }

                foreach (Claim claim in base.Claims)
                    yield return claim;

                foreach (Claim claim in m_userClaims)
                    yield return claim;

                foreach (Claim claim in m_deviceClaims)
                    yield return claim;
            }
        }

        /// <summary>
        /// Intenal method to initialize the claim collection.
        /// Lazy init is used so claims are not initialzed until needed
        /// </summary>
        [SecuritySafeCritical]
        void InitializeClaims()
        {
            if (!m_claimsInitialized)
            {
                lock (m_claimsIntiailizedLock)
                {
                    if (!m_claimsInitialized)
                    {
                        m_userClaims = new List<Claim>();
                        m_deviceClaims = new List<Claim>();

                        if (!String.IsNullOrEmpty(Name))
                        {
                            //
                            // Add the name claim only if the WindowsIdentity.Name is populated
                            // WindowsIdentity.Name will be null when it is the fake anonymous user
                            // with a token value of IntPtr.Zero
                            //
                            m_userClaims.Add(new Claim(NameClaimType, Name, ClaimValueTypes.String, m_issuerName, m_issuerName, this));
                        }

                        // primary sid
                        AddPrimarySidClaim(m_userClaims);

                        // group sids
                        AddGroupSidClaims(m_userClaims);

                        // The following TokenInformationClass's were part of the Win8 release
                        if (Environment.IsWindows8OrAbove)
                        {
                            // Device group sids
                            AddDeviceGroupSidClaims(m_deviceClaims, TokenInformationClass.TokenDeviceGroups);

                            // User token claims
                            AddTokenClaims(m_userClaims, TokenInformationClass.TokenUserClaimAttributes, ClaimTypes.WindowsUserClaim);

                            // Device token claims
                            AddTokenClaims(m_deviceClaims, TokenInformationClass.TokenDeviceClaimAttributes, ClaimTypes.WindowsDeviceClaim);
                        }

                        m_claimsInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a collection of SID claims that represent the DeviceSidGroups.
        /// </summary>
        /// this is SafeCritical as it accesss the NT token.
        [SecurityCritical]
        void AddDeviceGroupSidClaims(List<Claim> instanceClaims,  TokenInformationClass tokenInformationClass)
        {
            // special case the anonymous identity.
            if (m_safeTokenHandle.IsInvalid)
                return;

            SafeLocalAllocHandle safeAllocHandle =  SafeLocalAllocHandle.InvalidHandle;
            try
            {
                // Retrieve all group sids

                safeAllocHandle = GetTokenInformation(m_safeTokenHandle, tokenInformationClass);
                int count = Marshal.ReadInt32(safeAllocHandle.DangerousGetHandle());
                IntPtr pSidAndAttributes = new IntPtr((long)safeAllocHandle.DangerousGetHandle() + (long)Marshal.OffsetOf(typeof(Win32Native.TOKEN_GROUPS), "Groups"));
                string claimType = null;

                for (int i = 0; i < count; ++i)
                {
                    Win32Native.SID_AND_ATTRIBUTES group = (Win32Native.SID_AND_ATTRIBUTES)Marshal.PtrToStructure(pSidAndAttributes, typeof(Win32Native.SID_AND_ATTRIBUTES));
                    uint mask = Win32Native.SE_GROUP_ENABLED | Win32Native.SE_GROUP_LOGON_ID | Win32Native.SE_GROUP_USE_FOR_DENY_ONLY;
                    SecurityIdentifier groupSid = new SecurityIdentifier(group.Sid, true);
                    if ((group.Attributes & mask) == Win32Native.SE_GROUP_ENABLED)
                    {
                        claimType = ClaimTypes.WindowsDeviceGroup;
                        Claim claim = new Claim(claimType, groupSid.Value, ClaimValueTypes.String, m_issuerName, m_issuerName, this, ClaimTypes.WindowsSubAuthority, Convert.ToString(groupSid.IdentifierAuthority, CultureInfo.InvariantCulture));
                        claim.Properties.Add(claimType, "");
                        instanceClaims.Add(claim);
                    }
                    else if ((group.Attributes & mask) == Win32Native.SE_GROUP_USE_FOR_DENY_ONLY)
                    {
                        claimType = ClaimTypes.DenyOnlyWindowsDeviceGroup;
                        Claim claim = new Claim(claimType, groupSid.Value, ClaimValueTypes.String, m_issuerName, m_issuerName, this, ClaimTypes.WindowsSubAuthority, Convert.ToString(groupSid.IdentifierAuthority, CultureInfo.InvariantCulture));
                        claim.Properties.Add(claimType, "");
                        instanceClaims.Add(claim);
                    }

                    pSidAndAttributes = new IntPtr((long)pSidAndAttributes + Win32Native.SID_AND_ATTRIBUTES.SizeOf);
                }
            }
            finally
            {
                safeAllocHandle.Close();
            }
        }

        /// <summary>
        /// Creates a collection of SID claims that represent the users groups.
        /// </summary>
        /// this is SafeCritical as it accesss the NT token.
        [SecurityCritical]
        void AddGroupSidClaims(List<Claim> instanceClaims)
        {
            // special case the anonymous identity.
            if (m_safeTokenHandle.IsInvalid)
                return;

            SafeLocalAllocHandle safeAllocHandle =  SafeLocalAllocHandle.InvalidHandle;
            SafeLocalAllocHandle safeAllocHandlePrimaryGroup =  SafeLocalAllocHandle.InvalidHandle;
            try
            {
                // Retrieve the primary group sid
                safeAllocHandlePrimaryGroup = GetTokenInformation(m_safeTokenHandle, TokenInformationClass.TokenPrimaryGroup);
                Win32Native.TOKEN_PRIMARY_GROUP primaryGroup = (Win32Native.TOKEN_PRIMARY_GROUP)Marshal.PtrToStructure(safeAllocHandlePrimaryGroup.DangerousGetHandle(), typeof(Win32Native.TOKEN_PRIMARY_GROUP));
                SecurityIdentifier primaryGroupSid = new SecurityIdentifier(primaryGroup.PrimaryGroup, true);
                    
                // only add one primary group sid
                bool foundPrimaryGroupSid = false;

                // Retrieve all group sids, primary group sid is one of them
                safeAllocHandle = GetTokenInformation(m_safeTokenHandle, TokenInformationClass.TokenGroups);
                int count = Marshal.ReadInt32(safeAllocHandle.DangerousGetHandle());
                IntPtr pSidAndAttributes = new IntPtr((long)safeAllocHandle.DangerousGetHandle() + (long)Marshal.OffsetOf(typeof(Win32Native.TOKEN_GROUPS), "Groups"));
                for (int i = 0; i < count; ++i)
                {
                    Win32Native.SID_AND_ATTRIBUTES group = (Win32Native.SID_AND_ATTRIBUTES)Marshal.PtrToStructure(pSidAndAttributes, typeof(Win32Native.SID_AND_ATTRIBUTES));
                    uint mask = Win32Native.SE_GROUP_ENABLED | Win32Native.SE_GROUP_LOGON_ID | Win32Native.SE_GROUP_USE_FOR_DENY_ONLY;
                    SecurityIdentifier groupSid = new SecurityIdentifier(group.Sid, true);

                    if ((group.Attributes & mask) == Win32Native.SE_GROUP_ENABLED)
                    {
                        if (!foundPrimaryGroupSid && StringComparer.Ordinal.Equals( groupSid.Value, primaryGroupSid.Value))
                        {
                            instanceClaims.Add(new Claim(ClaimTypes.PrimaryGroupSid, groupSid.Value, ClaimValueTypes.String, m_issuerName, m_issuerName, this, ClaimTypes.WindowsSubAuthority, Convert.ToString(groupSid.IdentifierAuthority, CultureInfo.InvariantCulture)));
                            foundPrimaryGroupSid = true;
                        }
                        //Primary group sid generates both regular groupsid claim and primary groupsid claim
                        instanceClaims.Add(new Claim(ClaimTypes.GroupSid, groupSid.Value, ClaimValueTypes.String, m_issuerName, m_issuerName, this, ClaimTypes.WindowsSubAuthority, Convert.ToString(groupSid.IdentifierAuthority, CultureInfo.InvariantCulture)));

                    }
                    else if ((group.Attributes & mask) == Win32Native.SE_GROUP_USE_FOR_DENY_ONLY)
                    {
                        if (!foundPrimaryGroupSid && StringComparer.Ordinal.Equals( groupSid.Value, primaryGroupSid.Value))
                        {
                            instanceClaims.Add(new Claim(ClaimTypes.DenyOnlyPrimaryGroupSid, groupSid.Value, ClaimValueTypes.String, m_issuerName, m_issuerName, this, ClaimTypes.WindowsSubAuthority, Convert.ToString(groupSid.IdentifierAuthority, CultureInfo.InvariantCulture)));
                            foundPrimaryGroupSid = true;
                        }
                        //Primary group sid generates both regular groupsid claim and primary groupsid claim
                        instanceClaims.Add(new Claim(ClaimTypes.DenyOnlySid, groupSid.Value, ClaimValueTypes.String, m_issuerName, m_issuerName, this, ClaimTypes.WindowsSubAuthority, Convert.ToString(groupSid.IdentifierAuthority, CultureInfo.InvariantCulture)));
                    }
                    pSidAndAttributes = new IntPtr((long)pSidAndAttributes + Win32Native.SID_AND_ATTRIBUTES.SizeOf);
                }
            }
            finally
            {
                safeAllocHandle.Close();
                safeAllocHandlePrimaryGroup.Close();
            }
        }
        
        /// <summary>
        /// Creates a Windows SID Claim and adds to collection of claims.
        /// </summary>
        /// this is SafeCritical as it accesss the NT token.        
        [SecurityCritical]        
        void AddPrimarySidClaim(List<Claim> instanceClaims)
        {
            // special case the anonymous identity.
            if (m_safeTokenHandle.IsInvalid)
                return;

            SafeLocalAllocHandle safeAllocHandle = SafeLocalAllocHandle.InvalidHandle;
            try
            {
                safeAllocHandle = GetTokenInformation(m_safeTokenHandle, TokenInformationClass.TokenUser);
                Win32Native.SID_AND_ATTRIBUTES user = (Win32Native.SID_AND_ATTRIBUTES)Marshal.PtrToStructure(safeAllocHandle.DangerousGetHandle(), typeof(Win32Native.SID_AND_ATTRIBUTES));
                uint mask = Win32Native.SE_GROUP_USE_FOR_DENY_ONLY;
               
                SecurityIdentifier sid = new SecurityIdentifier(user.Sid, true);

                if (user.Attributes == 0)
                {
                    instanceClaims.Add(new Claim(ClaimTypes.PrimarySid, sid.Value, ClaimValueTypes.String, m_issuerName, m_issuerName, this, ClaimTypes.WindowsSubAuthority, Convert.ToString(sid.IdentifierAuthority, CultureInfo.InvariantCulture)));
                }
                else if ((user.Attributes & mask) == Win32Native.SE_GROUP_USE_FOR_DENY_ONLY)
                {
                    instanceClaims.Add(new Claim(ClaimTypes.DenyOnlyPrimarySid, sid.Value, ClaimValueTypes.String, m_issuerName, m_issuerName, this, ClaimTypes.WindowsSubAuthority, Convert.ToString(sid.IdentifierAuthority, CultureInfo.InvariantCulture)));
                }
            }
            finally
            {
                safeAllocHandle.Close();
            }
        }

        [SecurityCritical]
        void AddTokenClaims(List<Claim> instanceClaims, TokenInformationClass tokenInformationClass, string propertyValue)
        {
            // special case the anonymous identity.
            if (m_safeTokenHandle.IsInvalid)
                return;

            SafeLocalAllocHandle safeAllocHandle = SafeLocalAllocHandle.InvalidHandle;

            try
            {
                SafeLocalAllocHandle safeLocalAllocHandle = SafeLocalAllocHandle.InvalidHandle;
                safeAllocHandle = GetTokenInformation(m_safeTokenHandle, tokenInformationClass);

                Win32Native.CLAIM_SECURITY_ATTRIBUTES_INFORMATION claimAttributes = (Win32Native.CLAIM_SECURITY_ATTRIBUTES_INFORMATION)Marshal.PtrToStructure(safeAllocHandle.DangerousGetHandle(), typeof(Win32Native.CLAIM_SECURITY_ATTRIBUTES_INFORMATION));
                // An attribute represents a collection of claims.  Inside each attribute a claim can be multivalued, we create a claim for each value.
                // It is a ragged multi-dimentional array, where each cell can be of different lenghts.
                
                // index into array of claims.
                long offset = 0;
               
                for (int attribute = 0; attribute < claimAttributes.AttributeCount; attribute++)
                {
                    IntPtr pAttribute = new IntPtr(claimAttributes.Attribute.pAttributeV1.ToInt64() + offset);
                    Win32Native.CLAIM_SECURITY_ATTRIBUTE_V1 windowsClaim = (Win32Native.CLAIM_SECURITY_ATTRIBUTE_V1)Marshal.PtrToStructure(pAttribute, typeof(Win32Native.CLAIM_SECURITY_ATTRIBUTE_V1));                    
                    
                    // the switch was written this way, which appears to have multiple for loops, because each item in the ValueCount is of the same ValueType.  This saves the type check each item.
                    switch (windowsClaim.ValueType)
                    {
                        case Win32Native.CLAIM_SECURITY_ATTRIBUTE_TYPE_STRING:
                            IntPtr[] stringPointers = new IntPtr[windowsClaim.ValueCount];
                            Marshal.Copy(windowsClaim.Values.ppString, stringPointers, 0, (int)windowsClaim.ValueCount);

                            for (int item = 0; item < windowsClaim.ValueCount; item++)
                            {
                                instanceClaims.Add( new Claim(windowsClaim.Name, Marshal.PtrToStringAuto(stringPointers[item]), ClaimValueTypes.String, m_issuerName, m_issuerName, this, propertyValue, string.Empty));
                            }
                            break;

                        case Win32Native.CLAIM_SECURITY_ATTRIBUTE_TYPE_INT64:
                            Int64[] intValues = new Int64[windowsClaim.ValueCount];
                            Marshal.Copy(windowsClaim.Values.pInt64, intValues, 0, (int)windowsClaim.ValueCount);

                            for (int item = 0; item < windowsClaim.ValueCount; item++)
                            {
                                instanceClaims.Add(new Claim(windowsClaim.Name, Convert.ToString(intValues[item], CultureInfo.InvariantCulture), ClaimValueTypes.Integer64, m_issuerName, m_issuerName, this, propertyValue, string.Empty));
                            }
                            break;


                        case Win32Native.CLAIM_SECURITY_ATTRIBUTE_TYPE_UINT64:
                            Int64[] uintValues = new Int64[windowsClaim.ValueCount];
                            Marshal.Copy(windowsClaim.Values.pUint64, uintValues, 0, (int)windowsClaim.ValueCount);

                            for (int item = 0; item < windowsClaim.ValueCount; item++)
                            {
                                instanceClaims.Add( new Claim(windowsClaim.Name, Convert.ToString((UInt64)uintValues[item], CultureInfo.InvariantCulture), ClaimValueTypes.UInteger64, m_issuerName, m_issuerName, this, propertyValue, string.Empty));
                            }
                            break;

                        case Win32Native.CLAIM_SECURITY_ATTRIBUTE_TYPE_BOOLEAN:
                            Int64[] boolValues = new Int64[windowsClaim.ValueCount];
                            Marshal.Copy(windowsClaim.Values.pUint64, boolValues, 0, (int)windowsClaim.ValueCount);

                            for (int item = 0; item < windowsClaim.ValueCount; item++)
                            {
                                instanceClaims.Add(new Claim(windowsClaim.Name,
                                                  ((UInt64)boolValues[item] == 0 ? Convert.ToString(false, CultureInfo.InvariantCulture) : Convert.ToString(true, CultureInfo.InvariantCulture)),
                                                  ClaimValueTypes.Boolean, 
                                                  m_issuerName, 
                                                  m_issuerName, 
                                                  this, 
                                                  propertyValue, 
                                                  string.Empty));
                            }
                            break;


                        // These claim types are defined in the structure found in winnt.h, but I haven't received confirmation (may  2011) that they are supported and are not enabled.

                        //case Win32Native.CLAIM_SECURITY_ATTRIBUTE_TYPE_FQBN:
                        //    break;

                        //case Win32Native.CLAIM_SECURITY_ATTRIBUTE_TYPE_SID:
                        //    break;

                        //case Win32Native.CLAIM_SECURITY_ATTRIBUTE_TYPE_OCTET_STRING:
                        //    break;

                    }

                    offset += Marshal.SizeOf(windowsClaim);
                }
            }
            finally
            {
                safeAllocHandle.Close();
            }
        }

    }
#endif

    [Serializable]
    internal enum KerbLogonSubmitType : int {
        KerbInteractiveLogon = 2,
        KerbSmartCardLogon = 6,
        KerbWorkstationUnlockLogon = 7,
        KerbSmartCardUnlockLogon = 8,
        KerbProxyLogon = 9,
        KerbTicketLogon = 10,
        KerbTicketUnlockLogon = 11,
        KerbS4ULogon = 12
    }

    [Serializable]
    internal enum SecurityLogonType : int {
        Interactive = 2,
        Network,
        Batch,
        Service,
        Proxy,
        Unlock
    }

    [Serializable]
    internal enum TokenType : int {
        TokenPrimary = 1,
        TokenImpersonation
    }

    [Serializable]
    internal enum TokenInformationClass : int {
        TokenUser = 1,
        TokenGroups,
        TokenPrivileges,
        TokenOwner,
        TokenPrimaryGroup,
        TokenDefaultDacl,
        TokenSource,
        TokenType,
        TokenImpersonationLevel,
        TokenStatistics,
        TokenRestrictedSids,
        TokenSessionId,
        TokenGroupsAndPrivileges,
        TokenSessionReference,
        TokenSandBoxInert,
        TokenAuditPolicy,
        TokenOrigin,
        TokenElevationType,
        TokenLinkedToken,
        TokenElevation,
        TokenHasRestrictions,
        TokenAccessInformation,
        TokenVirtualizationAllowed,
        TokenVirtualizationEnabled,
        TokenIntegrityLevel,
        TokenUIAccess,
        TokenMandatoryPolicy,
        TokenLogonSid,
        TokenIsAppContainer,
        TokenCapabilities,
        TokenAppContainerSid,
        TokenAppContainerNumber,
        TokenUserClaimAttributes,
        TokenDeviceClaimAttributes,
        TokenRestrictedUserClaimAttributes,
        TokenRestrictedDeviceClaimAttributes,
        TokenDeviceGroups,
        TokenRestrictedDeviceGroups,
        MaxTokenInfoClass  // MaxTokenInfoClass should always be the last enum
    }
}
