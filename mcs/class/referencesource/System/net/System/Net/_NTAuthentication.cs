//------------------------------------------------------------------------------
// <copyright file="_NTAuthentication.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using System.Threading;
    using System.Globalization;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Net.Security;

    // #define ISC_REQ_DELEGATE                0x00000001
    // #define ISC_REQ_MUTUAL_AUTH             0x00000002
    // #define ISC_REQ_REPLAY_DETECT           0x00000004
    // #define ISC_REQ_SEQUENCE_DETECT         0x00000008
    // #define ISC_REQ_CONFIDENTIALITY         0x00000010
    // #define ISC_REQ_USE_SESSION_KEY         0x00000020
    // #define ISC_REQ_PROMPT_FOR_CREDS        0x00000040
    // #define ISC_REQ_USE_SUPPLIED_CREDS      0x00000080
    // #define ISC_REQ_ALLOCATE_MEMORY         0x00000100
    // #define ISC_REQ_USE_DCE_STYLE           0x00000200
    // #define ISC_REQ_DATAGRAM                0x00000400
    // #define ISC_REQ_CONNECTION              0x00000800
    // #define ISC_REQ_CALL_LEVEL              0x00001000
    // #define ISC_REQ_FRAGMENT_SUPPLIED       0x00002000
    // #define ISC_REQ_EXTENDED_ERROR          0x00004000
    // #define ISC_REQ_STREAM                  0x00008000
    // #define ISC_REQ_INTEGRITY               0x00010000
    // #define ISC_REQ_IDENTIFY                0x00020000
    // #define ISC_REQ_NULL_SESSION            0x00040000
    // #define ISC_REQ_MANUAL_CRED_VALIDATION  0x00080000
    // #define ISC_REQ_RESERVED1               0x00100000
    // #define ISC_REQ_FRAGMENT_TO_FIT         0x00200000
    // #define ISC_REQ_HTTP                    0x10000000
    // Win7 SP1 +
    // #define ISC_REQ_UNVERIFIED_TARGET_NAME  0x20000000  

    // #define ASC_REQ_DELEGATE                0x00000001
    // #define ASC_REQ_MUTUAL_AUTH             0x00000002
    // #define ASC_REQ_REPLAY_DETECT           0x00000004
    // #define ASC_REQ_SEQUENCE_DETECT         0x00000008
    // #define ASC_REQ_CONFIDENTIALITY         0x00000010
    // #define ASC_REQ_USE_SESSION_KEY         0x00000020
    // #define ASC_REQ_ALLOCATE_MEMORY         0x00000100
    // #define ASC_REQ_USE_DCE_STYLE           0x00000200
    // #define ASC_REQ_DATAGRAM                0x00000400
    // #define ASC_REQ_CONNECTION              0x00000800
    // #define ASC_REQ_CALL_LEVEL              0x00001000
    // #define ASC_REQ_EXTENDED_ERROR          0x00008000
    // #define ASC_REQ_STREAM                  0x00010000
    // #define ASC_REQ_INTEGRITY               0x00020000
    // #define ASC_REQ_LICENSING               0x00040000
    // #define ASC_REQ_IDENTIFY                0x00080000
    // #define ASC_REQ_ALLOW_NULL_SESSION      0x00100000
    // #define ASC_REQ_ALLOW_NON_USER_LOGONS   0x00200000
    // #define ASC_REQ_ALLOW_CONTEXT_REPLAY    0x00400000
    // #define ASC_REQ_FRAGMENT_TO_FIT         0x00800000
    // #define ASC_REQ_FRAGMENT_SUPPLIED       0x00002000
    // #define ASC_REQ_NO_TOKEN                0x01000000
    // #define ASC_REQ_HTTP                    0x10000000

    [Flags]
    internal enum ContextFlags {
        Zero            = 0,
        // The server in the transport application can
        // build new security contexts impersonating the
        // client that will be accepted by other servers
        // as the client's contexts.
        Delegate        = 0x00000001,
        // The communicating parties must authenticate
        // their identities to each other. Without MutualAuth,
        // the client authenticates its identity to the server.
        // With MutualAuth, the server also must authenticate
        // its identity to the client.
        MutualAuth      = 0x00000002,
        // The security package detects replayed packets and
        // notifies the caller if a packet has been replayed.
        // The use of this flag implies all of the conditions
        // specified by the Integrity flag.
        ReplayDetect    = 0x00000004,
        // The context must be allowed to detect out-of-order
        // delivery of packets later through the message support
        // functions. Use of this flag implies all of the
        // conditions specified by the Integrity flag.
        SequenceDetect  = 0x00000008,
        // The context must protect data while in transit.
        // Confidentiality is supported for NTLM with Microsoft
        // Windows NT version 4.0, SP4 and later and with the
        // Kerberos protocol in Microsoft Windows 2000 and later.
        Confidentiality = 0x00000010,
        UseSessionKey   = 0x00000020,
        AllocateMemory  = 0x00000100,

        // Connection semantics must be used.
        Connection      = 0x00000800,

        // Client applications requiring extended error messages specify the
        // ISC_REQ_EXTENDED_ERROR flag when calling the InitializeSecurityContext
        // Server applications requiring extended error messages set
        // the ASC_REQ_EXTENDED_ERROR flag when calling AcceptSecurityContext.
        InitExtendedError    = 0x00004000,
        AcceptExtendedError  = 0x00008000,
        // A transport application requests stream semantics
        // by setting the ISC_REQ_STREAM and ASC_REQ_STREAM
        // flags in the calls to the InitializeSecurityContext
        // and AcceptSecurityContext functions
        InitStream          = 0x00008000,
        AcceptStream        = 0x00010000,
        // Buffer integrity can be verified; however, replayed
        // and out-of-sequence messages will not be detected
        InitIntegrity       = 0x00010000,       // ISC_REQ_INTEGRITY
        AcceptIntegrity     = 0x00020000,       // ASC_REQ_INTEGRITY
        
        InitManualCredValidation    = 0x00080000,   // ISC_REQ_MANUAL_CRED_VALIDATION
        InitUseSuppliedCreds        = 0x00000080,   // ISC_REQ_USE_SUPPLIED_CREDS
        InitIdentify                = 0x00020000,   // ISC_REQ_IDENTIFY
        AcceptIdentify              = 0x00080000,   // ASC_REQ_IDENTIFY

        ProxyBindings               = 0x04000000,   // ASC_REQ_PROXY_BINDINGS
        AllowMissingBindings        = 0x10000000,   // ASC_REQ_ALLOW_MISSING_BINDINGS

        UnverifiedTargetName        = 0x20000000,   // ISC_REQ_UNVERIFIED_TARGET_NAME
    }

    internal class NTAuthentication {

        static private int s_UniqueGroupId = 1;
        static private ContextCallback s_InitializeCallback = new ContextCallback(InitializeCallback);

        private bool m_IsServer;

        private SafeFreeCredentials m_CredentialsHandle;
        private SafeDeleteContext   m_SecurityContext;
        private string m_Spn;
        private string m_ClientSpecifiedSpn;

        private int m_TokenSize;
        private ContextFlags m_RequestedContextFlags;
        private ContextFlags m_ContextFlags;
        private string m_UniqueUserId;

        private bool m_IsCompleted;
        private string m_ProtocolName;
        private SecSizes m_Sizes;
        private string m_LastProtocolName;
        private string m_Package;

        private ChannelBinding m_ChannelBinding;

        //
        // Properties
        //
        internal string UniqueUserId {
            get {
                return m_UniqueUserId;
            }
        }

        // The semantic of this propoerty is "Don't call me again".
        // It can be completed either with success or error
        // The latest case is signalled by IsValidContext==false
        internal bool IsCompleted {
            get {
                return m_IsCompleted;
            }
        }

        internal bool IsValidContext {
            get {
                return !(m_SecurityContext == null || m_SecurityContext.IsInvalid);
            }
        }

        internal string AssociatedName {
            get {
                if (!(IsValidContext && IsCompleted))
                    throw new Win32Exception((int)SecurityStatus.InvalidHandle);

                string name = SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPIAuth, m_SecurityContext, ContextAttribute.Names) as string;
                GlobalLog.Print("NTAuthentication: The context is associated with [" + name + "]");
                return name;
            }
        }

        internal bool IsConfidentialityFlag {
            get {
                return (m_ContextFlags & ContextFlags.Confidentiality) != 0;
            }
        }

        internal bool IsIntegrityFlag {
            get {
                return (m_ContextFlags & (m_IsServer?ContextFlags.AcceptIntegrity:ContextFlags.InitIntegrity)) != 0;
            }
        }

        internal bool IsMutualAuthFlag {
            get {
                return (m_ContextFlags & ContextFlags.MutualAuth) != 0;
            }
        }

        internal bool IsDelegationFlag {
            get {
                return (m_ContextFlags & ContextFlags.Delegate) != 0;
            }
        }

        internal bool IsIdentifyFlag {
            get {
                return (m_ContextFlags & (m_IsServer?ContextFlags.AcceptIdentify:ContextFlags.InitIdentify)) != 0;
            }
        }

        internal string Spn {
            get {
                return m_Spn;
            }
        }

        internal string ClientSpecifiedSpn {
            get {
                if (m_ClientSpecifiedSpn == null) {
                    m_ClientSpecifiedSpn = GetClientSpecifiedSpn();
                }
                return m_ClientSpecifiedSpn;
            }
        }

        internal bool OSSupportsExtendedProtection {
            get {
                GlobalLog.Assert(IsCompleted && IsValidContext, "NTAuthentication#{0}::OSSupportsExtendedProtection|The context is not completed or invalid.", ValidationHelper.HashString(this));

                int errorCode;
                SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPIAuth, m_SecurityContext, 
                    ContextAttribute.ClientSpecifiedSpn, out errorCode);

                // We consider any error other than Unsupported to mean that the underlying OS
                // supports extended protection.  Most likely it will be TargetUnknown.
                return ((SecurityStatus)errorCode != SecurityStatus.Unsupported);
            }
        }

        //
        // True indicates this instance is for Server and will use AcceptSecurityContext SSPI API
        //
        internal bool IsServer {
            get {
                return m_IsServer;
            }
        }

        //
        internal bool IsKerberos
        {
            get {
                if (m_LastProtocolName  == null)
                    m_LastProtocolName = ProtocolName;

                return (object) m_LastProtocolName == (object) NegotiationInfoClass.Kerberos;
            }
        }
        internal bool IsNTLM
        {
            get {
                if (m_LastProtocolName  == null)
                    m_LastProtocolName = ProtocolName;

                return (object) m_LastProtocolName == (object) NegotiationInfoClass.NTLM;
            }
        }

        internal string Package
        {
            get
            {
                return m_Package;
            }
        }

        internal string ProtocolName {
            get {
                // NB: May return string.Empty if the auth is not done yet or failed
                if (m_ProtocolName==null)
                {
                    NegotiationInfoClass negotiationInfo = null;

                    if (IsValidContext)
                    {
                        negotiationInfo = SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPIAuth, m_SecurityContext, ContextAttribute.NegotiationInfo) as NegotiationInfoClass;
                        if (IsCompleted) {
                            if (negotiationInfo != null)
                            {
                                //cache it only when it's completed
                                m_ProtocolName = negotiationInfo.AuthenticationPackage;
                            }
                        }
                    }
                    return negotiationInfo == null? string.Empty: negotiationInfo.AuthenticationPackage;
                }
                return m_ProtocolName;
            }
        }

        internal SecSizes Sizes {
            get {
                GlobalLog.Assert(IsCompleted && IsValidContext, "NTAuthentication#{0}::MaxDataSize|The context is not completed or invalid.", ValidationHelper.HashString(this));
                if (m_Sizes == null) {
                    m_Sizes = SSPIWrapper.QueryContextAttributes(
                                  GlobalSSPI.SSPIAuth,
                                  m_SecurityContext,
                                  ContextAttribute.Sizes
                                  ) as SecSizes;
                }
                return m_Sizes;
            }
        }

        internal ChannelBinding ChannelBinding
        {
            get { return m_ChannelBinding; }
        }

        //
        // .Ctors
        //

        //
        // Use only for client HTTP authentication
        //
        internal NTAuthentication(string package, NetworkCredential networkCredential, SpnToken spnToken, 
                WebRequest request, ChannelBinding channelBinding) :
            this(false, package, networkCredential, spnToken.Spn, GetHttpContextFlags(request, spnToken.IsTrusted), 
                request.GetWritingContext(), channelBinding)
        {
            //
            //  In order to prevent a race condition where one request could
            //  steal a connection from another request, before a handshake is
            //  complete, we create a new Group for each authentication request.
            //
            if (package == NtlmClient.AuthType || package == NegotiateClient.AuthType) {
                m_UniqueUserId = (Interlocked.Increment(ref s_UniqueGroupId)).ToString(NumberFormatInfo.InvariantInfo) + m_UniqueUserId;
            }
        }
        //
        private static ContextFlags GetHttpContextFlags(WebRequest request, bool trustedSpn)
        {
            ContextFlags contextFlags = ContextFlags.Connection;

            if (request.ImpersonationLevel == TokenImpersonationLevel.Anonymous)
                throw new NotSupportedException(SR.GetString(SR.net_auth_no_anonymous_support));
            else if(request.ImpersonationLevel == TokenImpersonationLevel.Identification)
                contextFlags |= ContextFlags.InitIdentify;
            else if(request.ImpersonationLevel == TokenImpersonationLevel.Delegation)
                contextFlags |= ContextFlags.Delegate;

            if (request.AuthenticationLevel == AuthenticationLevel.MutualAuthRequested || request.AuthenticationLevel == AuthenticationLevel.MutualAuthRequired)
                contextFlags |= ContextFlags.MutualAuth;

            // CBT: If the SPN came from an untrusted source we should tell the server by setting this flag
            if (!trustedSpn && ComNetOS.IsWin7Sp1orLater)
                contextFlags |= ContextFlags.UnverifiedTargetName;

            return contextFlags;
        }

        //
        // This constructor is for a general (non-HTTP) authentication handshake using SSPI
        // Works for both client and server sides.
        //
        // Security: we may need to impersonate on user behalf as to temporarily restore original thread token.
        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlPrincipal)]
        internal NTAuthentication(bool isServer, string package, NetworkCredential credential, string spn, ContextFlags requestedContextFlags, ContextAwareResult context, ChannelBinding channelBinding)
        {
            //
            // check if we're using DefaultCredentials
            //
            if (credential is SystemNetworkCredential)
            {
                // 
#if DEBUG
                GlobalLog.Assert(context == null || context.IdentityRequested, "NTAuthentication#{0}::.ctor|Authentication required when it wasn't expected.  (Maybe Credentials was changed on another thread?)", ValidationHelper.HashString(this));
#endif

                WindowsIdentity w = context == null ? null : context.Identity;
                try
                {
                    IDisposable ctx = w == null ? null : w.Impersonate();
                    if (ctx != null)
                    {
                        using (ctx)
                        {
                            Initialize(isServer, package, credential, spn, requestedContextFlags, channelBinding);
                        }
                    }
                    else
                    {
                        ExecutionContext x = context == null ? null : context.ContextCopy;
                        if (x == null)
                        {
                            Initialize(isServer, package, credential, spn, requestedContextFlags, channelBinding);
                        }
                        else
                        {
                            ExecutionContext.Run(x, s_InitializeCallback, new InitializeCallbackContext(this, isServer, package, credential, spn, requestedContextFlags, channelBinding));
                        }
                    }
                }
                catch
                {
                    // Prevent the impersonation from leaking to upstack exception filters.
                    throw;
                }
            }
            else
            {
                Initialize(isServer, package, credential, spn, requestedContextFlags, channelBinding);
            }
        }

        //
        // This overload does not attmept to impersonate because the caller either did it already or the original thread context is still preserved
        //
        internal NTAuthentication(bool isServer, string package, NetworkCredential credential, string spn, ContextFlags requestedContextFlags, ChannelBinding channelBinding) {
            Initialize(isServer, package, credential, spn, requestedContextFlags, channelBinding);
        }

        //
        // This overload always uses the default credentials for the process.
        //
        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.ControlPrincipal)]
        internal NTAuthentication(bool isServer, string package, string spn, ContextFlags requestedContextFlags, ChannelBinding channelBinding)
        {
            try
            {
                using (WindowsIdentity.Impersonate(IntPtr.Zero))
                {
                    Initialize(isServer, package, SystemNetworkCredential.defaultCredential, spn, requestedContextFlags, channelBinding);
                }
            }
            catch
            {
                // Avoid exception filter attacks.
                throw;
            }
        }

        private class InitializeCallbackContext
        {
            internal InitializeCallbackContext(NTAuthentication thisPtr, bool isServer, string package, NetworkCredential credential, string spn, ContextFlags requestedContextFlags, ChannelBinding channelBinding)
            {
                this.thisPtr = thisPtr;
                this.isServer = isServer;
                this.package = package;
                this.credential = credential;
                this.spn = spn;
                this.requestedContextFlags = requestedContextFlags;
                this.channelBinding = channelBinding;
            }

            internal readonly NTAuthentication thisPtr;
            internal readonly bool isServer;
            internal readonly string package;
            internal readonly NetworkCredential credential;
            internal readonly string spn;
            internal readonly ContextFlags requestedContextFlags;
            internal readonly ChannelBinding channelBinding;
        }

        private static void InitializeCallback(object state)
        {
            InitializeCallbackContext context = (InitializeCallbackContext)state;
            context.thisPtr.Initialize(context.isServer, context.package, context.credential, context.spn, context.requestedContextFlags, context.channelBinding);
        }

        //
        private void Initialize(bool isServer, string package, NetworkCredential credential, string spn, ContextFlags requestedContextFlags, ChannelBinding channelBinding) {
            GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::.ctor() package:" + ValidationHelper.ToString(package) + " spn:" + ValidationHelper.ToString(spn) + " flags :" + requestedContextFlags.ToString());
            m_TokenSize = SSPIWrapper.GetVerifyPackageInfo(GlobalSSPI.SSPIAuth, package, true).MaxToken;
            m_IsServer = isServer;
            m_Spn = spn;
            m_SecurityContext = null;
            m_RequestedContextFlags = requestedContextFlags;
            m_Package = package;
            m_ChannelBinding = channelBinding;

            GlobalLog.Print("Peer SPN-> '" + m_Spn + "'");
            //
            // check if we're using DefaultCredentials
            //
            if (credential is SystemNetworkCredential)
            {
                GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::.ctor(): using DefaultCredentials");
                m_CredentialsHandle = SSPIWrapper.AcquireDefaultCredential(
                                                    GlobalSSPI.SSPIAuth,
                                                    package,
                                                    (m_IsServer? CredentialUse.Inbound: CredentialUse.Outbound));
                m_UniqueUserId = "/S"; // save off for unique connection marking ONLY used by HTTP client
            }
            else if (ComNetOS.IsWin7orLater)
            {
                unsafe
                {
                    SafeSspiAuthDataHandle authData = null;
                    try
                    {
                        SecurityStatus result = UnsafeNclNativeMethods.SspiHelper.SspiEncodeStringsAsAuthIdentity(
                            credential.InternalGetUserName(), credential.InternalGetDomain(),
                            credential.InternalGetPassword(), out authData);

                        if (result != SecurityStatus.OK)
                        {
                            if (Logging.On) Logging.PrintError(Logging.Web, SR.GetString(SR.net_log_operation_failed_with_error, "SspiEncodeStringsAsAuthIdentity()", String.Format(CultureInfo.CurrentCulture, "0x{0:X}", (int)result)));
                            throw new Win32Exception((int)result);
                        }

                        m_CredentialsHandle = SSPIWrapper.AcquireCredentialsHandle(GlobalSSPI.SSPIAuth,
                            package, (m_IsServer ? CredentialUse.Inbound : CredentialUse.Outbound), ref authData);
                    }
                    finally
                    {
                        if (authData != null)
                        {
                            authData.Close();
                        }
                    }
                }
            }
            else
            {

                //
                // we're not using DefaultCredentials, we need a
                // AuthIdentity struct to contain credentials
                // SECREVIEW:
                // we'll save username/domain in temp strings, to avoid decrypting multiple times.
                // password is only used once
                //
                string username = credential.InternalGetUserName();

                string domain = credential.InternalGetDomain();
                // ATTN:
                // NetworkCredential class does not differentiate between null and "" but SSPI packages treat these cases differently
                // For NTLM we want to keep "" for Wdigest.Dll we should use null.
                AuthIdentity authIdentity = new AuthIdentity(username, credential.InternalGetPassword(), (object)package == (object)NegotiationInfoClass.WDigest && (domain == null || domain.Length == 0)? null: domain);

                m_UniqueUserId = domain + "/" + username + "/U"; // save off for unique connection marking ONLY used by HTTP client

                GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::.ctor(): using authIdentity:" + authIdentity.ToString());

                m_CredentialsHandle = SSPIWrapper.AcquireCredentialsHandle(
                                                    GlobalSSPI.SSPIAuth,
                                                    package,
                                                    (m_IsServer? CredentialUse.Inbound: CredentialUse.Outbound),
                                                    ref authIdentity
                                                    );
            }
        }

        //
        // Methods
        //


        // This will return an client token when conducted authentication on server side'
        // This token can be used ofr impersanation
        // We use it to create a WindowsIdentity and hand it out to the server app.
        internal SafeCloseHandle GetContextToken(out SecurityStatus status)
        {
            GlobalLog.Assert(IsCompleted && IsValidContext, "NTAuthentication#{0}::GetContextToken|Should be called only when completed with success, currently is not!", ValidationHelper.HashString(this));
            GlobalLog.Assert(IsServer, "NTAuthentication#{0}::GetContextToken|The method must not be called by the client side!", ValidationHelper.HashString(this));

            if (!IsValidContext) {
                throw new Win32Exception((int)SecurityStatus.InvalidHandle);
            }


            SafeCloseHandle token = null;
            status = (SecurityStatus) SSPIWrapper.QuerySecurityContextToken(
                GlobalSSPI.SSPIAuth,
                m_SecurityContext,
                out token);

            return token;
        }

        internal SafeCloseHandle GetContextToken()
        {
            SecurityStatus status;
            SafeCloseHandle token = GetContextToken(out status);
            if (status != SecurityStatus.OK) {
                throw new Win32Exception((int)status);
            }
            return token;
        }

        internal void CloseContext()
        {
            if (m_SecurityContext != null && !m_SecurityContext.IsClosed)
                m_SecurityContext.Close();
        }

        //
        // NTAuth::GetOutgoingBlob()
        // Created:   12-01-1999: L.M.
        // Description:
        // Accepts a base64 encoded incoming security blob and returns
        // a base 64 encoded outgoing security blob
        //
        // This method is for HttpWebRequest usage only as it has semantic bound to it
        internal string GetOutgoingBlob(string incomingBlob) {
            GlobalLog.Enter("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingBlob", incomingBlob);
            byte[] decodedIncomingBlob = null;
            if (incomingBlob != null && incomingBlob.Length > 0) {
                decodedIncomingBlob = Convert.FromBase64String(incomingBlob);
            }
            byte[] decodedOutgoingBlob = null;

            if ((IsValidContext || IsCompleted) && decodedIncomingBlob == null) {
                // we tried auth previously, now we got a null blob, we're done. this happens
                // with Kerberos & valid credentials on the domain but no ACLs on the resource
                GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingBlob() null blob AND m_SecurityContext#" + ValidationHelper.HashString(m_SecurityContext) + "::Handle:[0x" + m_SecurityContext.ToString() + "]");
                m_IsCompleted = true;
            }
            else {
                SecurityStatus statusCode;
#if TRAVE
                try {
#endif
                    decodedOutgoingBlob = GetOutgoingBlob(decodedIncomingBlob, true, out statusCode);
#if TRAVE
                } catch (Exception exception) {
                    if (NclUtilities.IsFatal(exception)) throw;

                    GlobalLog.LeaveException("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingBlob", exception);
                    throw;
                }
#endif
            }

            string outgoingBlob = null;
            if (decodedOutgoingBlob != null && decodedOutgoingBlob.Length > 0) {
                outgoingBlob = Convert.ToBase64String(decodedOutgoingBlob);
            }

            //This is only for HttpWebRequest that does not need security context anymore
            if (IsCompleted)
            {
                string name = ProtocolName; // cache the only info needed from a completed context before closing it
                CloseContext();
            }
            GlobalLog.Leave("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingBlob", outgoingBlob);
            return outgoingBlob;
        }

        // NTAuth::GetOutgoingBlob()
        // Created:   12-01-1999: L.M.
        // Description:
        // Accepts an incoming binary security blob  and returns
        // an outgoing binary security blob
        internal byte[] GetOutgoingBlob(byte[] incomingBlob, bool throwOnError, out SecurityStatus statusCode)
        {
            GlobalLog.Enter("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingBlob", ((incomingBlob == null) ? "0" : incomingBlob.Length.ToString(NumberFormatInfo.InvariantInfo)) + " bytes");

            List<SecurityBuffer> list = new List<SecurityBuffer>(2);
            
            if (incomingBlob != null) {
                list.Add(new SecurityBuffer(incomingBlob, BufferType.Token));
            }
            if (m_ChannelBinding != null) {
                list.Add(new SecurityBuffer(m_ChannelBinding));
            }

            SecurityBuffer[] inSecurityBufferArray = null;
            if (list.Count > 0)
            {
                inSecurityBufferArray = list.ToArray();
            }

            SecurityBuffer outSecurityBuffer = new SecurityBuffer(m_TokenSize, BufferType.Token);

            bool firstTime = m_SecurityContext == null;
            try {
                if (!m_IsServer) {
                    // client session
                    statusCode = (SecurityStatus)SSPIWrapper.InitializeSecurityContext(
                        GlobalSSPI.SSPIAuth,
                        m_CredentialsHandle,
                        ref m_SecurityContext,
                        m_Spn,
                        m_RequestedContextFlags,
                        Endianness.Network,
                        inSecurityBufferArray,
                        outSecurityBuffer,
                        ref m_ContextFlags);

                    GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingBlob() SSPIWrapper.InitializeSecurityContext() returns statusCode:0x" + ((int) statusCode).ToString("x8", NumberFormatInfo.InvariantInfo) + " (" + statusCode.ToString() + ")");

                    if (statusCode == SecurityStatus.CompleteNeeded)
                    {
                        SecurityBuffer[] inSecurityBuffers = new SecurityBuffer[1];
                        inSecurityBuffers[0] = outSecurityBuffer;

                        statusCode = (SecurityStatus) SSPIWrapper.CompleteAuthToken(
                            GlobalSSPI.SSPIAuth,
                            ref m_SecurityContext,
                            inSecurityBuffers );

                        GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingDigestBlob() SSPIWrapper.CompleteAuthToken() returns statusCode:0x" + ((int) statusCode).ToString("x8", NumberFormatInfo.InvariantInfo) + " (" + statusCode.ToString() + ")");
                        outSecurityBuffer.token = null;
                    }
                }
                else {
                    // server session
                    statusCode = (SecurityStatus)SSPIWrapper.AcceptSecurityContext(
                        GlobalSSPI.SSPIAuth,
                        m_CredentialsHandle,
                        ref m_SecurityContext,
                        m_RequestedContextFlags,
                        Endianness.Network,
                        inSecurityBufferArray,
                        outSecurityBuffer,
                        ref m_ContextFlags);

                    GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingBlob() SSPIWrapper.AcceptSecurityContext() returns statusCode:0x" + ((int) statusCode).ToString("x8", NumberFormatInfo.InvariantInfo) + " (" + statusCode.ToString() + ")");
                }
            }
            finally {
                //
                // Assuming the ISC or ASC has referenced the credential on the first successful call,
                // we want to decrement the effective ref count by "disposing" it.
                // The real dispose will happen when the security context is closed.
                // Note if the first call was not successfull the handle is physically destroyed here
                //
              if (firstTime && m_CredentialsHandle != null)
                  m_CredentialsHandle.Close();
            }


          if (((int) statusCode & unchecked((int) 0x80000000)) != 0)
          {
                CloseContext();
                m_IsCompleted = true;
                if (throwOnError) {
                    Win32Exception exception = new Win32Exception((int) statusCode);
                    GlobalLog.Leave("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingBlob", "Win32Exception:" + exception);
                    throw exception;
                }
                GlobalLog.Leave("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingBlob", "null statusCode:0x" + ((int) statusCode).ToString("x8", NumberFormatInfo.InvariantInfo) + " (" + statusCode.ToString() + ")");
                return null;
            }
            else if (firstTime && m_CredentialsHandle != null)
            {
                // cache until it is pushed out by newly incoming handles
                SSPIHandleCache.CacheCredential(m_CredentialsHandle);
            }

            // the return value from SSPI will tell us correctly if the
            // handshake is over or not: http://msdn.microsoft.com/library/psdk/secspi/sspiref_67p0.htm
            // we also have to consider the case in which SSPI formed a new context, in this case we're done as well.
            if (statusCode == SecurityStatus.OK)
            {
                // we're sucessfully done
                GlobalLog.Assert(statusCode == SecurityStatus.OK, "NTAuthentication#{0}::GetOutgoingBlob()|statusCode:[0x{1:x8}] ({2}) m_SecurityContext#{3}::Handle:[{4}] [STATUS != OK]", ValidationHelper.HashString(this), (int)statusCode, statusCode, ValidationHelper.HashString(m_SecurityContext), ValidationHelper.ToString(m_SecurityContext));
                m_IsCompleted = true;
            }
            else {
                // we need to continue
                GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingBlob() need continue statusCode:[0x" + ((int) statusCode).ToString("x8", NumberFormatInfo.InvariantInfo) + "] (" + statusCode.ToString() + ") m_SecurityContext#" + ValidationHelper.HashString(m_SecurityContext) + "::Handle:" + ValidationHelper.ToString(m_SecurityContext) + "]");
            }
//            GlobalLog.Print("out token = " + outSecurityBuffer.ToString());
//            GlobalLog.Dump(outSecurityBuffer.token);
            GlobalLog.Leave("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingBlob", "IsCompleted:" + IsCompleted.ToString());
            return outSecurityBuffer.token;
        }

        // for Server side (IIS 6.0) see: \\netindex\Sources\inetsrv\iis\iisrearc\iisplus\ulw3\digestprovider.cxx
        // for Client side (HTTP.SYS) see: \\netindex\Sources\net\http\sys\ucauth.c
        internal string GetOutgoingDigestBlob(string incomingBlob, string requestMethod, string requestedUri, string realm, bool isClientPreAuth, bool throwOnError, out SecurityStatus statusCode)
        {
            GlobalLog.Enter("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingDigestBlob", incomingBlob);

            // second time call with 3 incoming buffers to select HTTP client.
            // we should get back a SecurityStatus.OK and a non null outgoingBlob.
            SecurityBuffer[] inSecurityBuffers = null;
            SecurityBuffer outSecurityBuffer = new SecurityBuffer(m_TokenSize, isClientPreAuth ? BufferType.Parameters : BufferType.Token);

            bool firstTime = m_SecurityContext == null;
            try {
                if (!m_IsServer) {
                    // client session

                    if (!isClientPreAuth) {

                        if (incomingBlob != null) 
                        {
                            List<SecurityBuffer> list = new List<SecurityBuffer>(5);

                            list.Add(new SecurityBuffer(WebHeaderCollection.HeaderEncoding.GetBytes(incomingBlob), BufferType.Token));
                            list.Add(new SecurityBuffer(WebHeaderCollection.HeaderEncoding.GetBytes(requestMethod), BufferType.Parameters));
                            list.Add(new SecurityBuffer(null, BufferType.Parameters));
                            list.Add(new SecurityBuffer(Encoding.Unicode.GetBytes(m_Spn), BufferType.TargetHost));

                            if (m_ChannelBinding != null) {
                                list.Add(new SecurityBuffer(m_ChannelBinding));
                            }

                            inSecurityBuffers = list.ToArray();
                        }

                        statusCode = (SecurityStatus) SSPIWrapper.InitializeSecurityContext(
                            GlobalSSPI.SSPIAuth,
                            m_CredentialsHandle,
                            ref m_SecurityContext,
                            requestedUri, // this must match the Uri in the HTTP status line for the current request
                            m_RequestedContextFlags,
                            Endianness.Network,
                            inSecurityBuffers,
                            outSecurityBuffer,
                            ref m_ContextFlags );

                        GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingDigestBlob() SSPIWrapper.InitializeSecurityContext() returns statusCode:0x" + ((int) statusCode).ToString("x8", NumberFormatInfo.InvariantInfo) + " (" + statusCode.ToString() + ")");
                    }
                    else {
#if WDIGEST_PREAUTH
                        inSecurityBuffers = new SecurityBuffer[] {
                            new SecurityBuffer(null, BufferType.Token),
                            new SecurityBuffer(WebHeaderCollection.HeaderEncoding.GetBytes(requestMethod), BufferType.Parameters),
                            new SecurityBuffer(WebHeaderCollection.HeaderEncoding.GetBytes(requestedUri), BufferType.Parameters),
                            new SecurityBuffer(null, BufferType.Parameters),
                            outSecurityBuffer,
                        };

                        statusCode = (SecurityStatus) SSPIWrapper.MakeSignature(GlobalSSPI.SSPIAuth, m_SecurityContext, inSecurityBuffers, 0);

                        GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingDigestBlob() SSPIWrapper.MakeSignature() returns statusCode:0x" + ((int) statusCode).ToString("x8", NumberFormatInfo.InvariantInfo) + " (" + statusCode.ToString() + ")");
#else
                        statusCode = SecurityStatus.OK;
                        GlobalLog.Assert("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingDigestBlob()", "Invalid code path.");
#endif
                    }
                }
                else {
                    // server session
                    List<SecurityBuffer> list = new List<SecurityBuffer>(6);
                    
                    list.Add(incomingBlob == null ? new SecurityBuffer(0, BufferType.Token) : new SecurityBuffer(WebHeaderCollection.HeaderEncoding.GetBytes(incomingBlob), BufferType.Token));
                    list.Add(requestMethod == null ? new SecurityBuffer(0, BufferType.Parameters) : new SecurityBuffer(WebHeaderCollection.HeaderEncoding.GetBytes(requestMethod), BufferType.Parameters));
                    list.Add(requestedUri == null ? new SecurityBuffer(0, BufferType.Parameters) : new SecurityBuffer(WebHeaderCollection.HeaderEncoding.GetBytes(requestedUri), BufferType.Parameters));
                    list.Add(new SecurityBuffer(0, BufferType.Parameters));
                    list.Add(realm == null ? new SecurityBuffer(0, BufferType.Parameters) : new SecurityBuffer(Encoding.Unicode.GetBytes(realm), BufferType.Parameters));

                    if (m_ChannelBinding != null) {
                        list.Add(new SecurityBuffer(m_ChannelBinding));
                    }

                    inSecurityBuffers = list.ToArray();

                    statusCode = (SecurityStatus) SSPIWrapper.AcceptSecurityContext(
                        GlobalSSPI.SSPIAuth,
                        m_CredentialsHandle,
                        ref m_SecurityContext,
                        m_RequestedContextFlags,
                        Endianness.Network,
                        inSecurityBuffers,
                        outSecurityBuffer,
                        ref m_ContextFlags );

                    GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingDigestBlob() SSPIWrapper.AcceptSecurityContext() returns statusCode:0x" + ((int)statusCode).ToString("x8", NumberFormatInfo.InvariantInfo) + " (" + statusCode.ToString() + ")");

                    if (statusCode == SecurityStatus.CompleteNeeded)
                    {
                        inSecurityBuffers[4] = outSecurityBuffer;

                        statusCode = (SecurityStatus) SSPIWrapper.CompleteAuthToken(
                                GlobalSSPI.SSPIAuth,
                                ref m_SecurityContext,
                                inSecurityBuffers );

                        GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingDigestBlob() SSPIWrapper.CompleteAuthToken() returns statusCode:0x" + ((int)statusCode).ToString("x8", NumberFormatInfo.InvariantInfo) + " (" + statusCode.ToString() + ")");

                        outSecurityBuffer.token = null;
                    }
                }
            }
            finally {
                //
                // Assuming the ISC or ASC has referenced the credential on the first successful call,
                // we want to decrement the effective ref count by "disposing" it.
                // The real dispose will happen when the security context is closed.
                // Note if the first call was not successfull the handle is physically destroyed here
                //
              if (firstTime && m_CredentialsHandle != null)
                  m_CredentialsHandle.Close();
            }


            if (((int) statusCode & unchecked((int) 0x80000000)) != 0)
            {
                CloseContext();
                if (throwOnError) {
                    Win32Exception exception = new Win32Exception((int) statusCode);
                    GlobalLog.Leave("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingDigestBlob", "Win32Exception:" + exception);
                    throw exception;
                }
                GlobalLog.Leave("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingDigestBlob", "null statusCode:0x" + ((int) statusCode).ToString("x8", NumberFormatInfo.InvariantInfo) + " (" + statusCode.ToString() + ")");
                return null;
            }
            else if (firstTime && m_CredentialsHandle != null)
            {
                // cache until it is pushed out by newly incoming handles
                SSPIHandleCache.CacheCredential(m_CredentialsHandle);
            }


            // the return value from SSPI will tell us correctly if the
            // handshake is over or not: http://msdn.microsoft.com/library/psdk/secspi/sspiref_67p0.htm
            if (statusCode == SecurityStatus.OK)
            {
                // we're done, cleanup
                m_IsCompleted = true;
            }
            else {
                // we need to continue
                GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingDigestBlob() need continue statusCode:[0x" + ((int) statusCode).ToString("x8", NumberFormatInfo.InvariantInfo) + "] (" + statusCode.ToString() + ") m_SecurityContext#" + ValidationHelper.HashString(m_SecurityContext) + "::Handle:" + ValidationHelper.ToString(m_SecurityContext) + "]");
            }
            GlobalLog.Print("out token = " + outSecurityBuffer.ToString());
            GlobalLog.Dump(outSecurityBuffer.token);
            GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingDigestBlob() IsCompleted:" + IsCompleted.ToString());

            byte[] decodedOutgoingBlob = outSecurityBuffer.token;
            string outgoingBlob = null;
            if (decodedOutgoingBlob!=null && decodedOutgoingBlob.Length>0) {
                outgoingBlob = WebHeaderCollection.HeaderEncoding.GetString(decodedOutgoingBlob, 0, outSecurityBuffer.size);
            }
            GlobalLog.Leave("NTAuthentication#" + ValidationHelper.HashString(this) + "::GetOutgoingDigestBlob", outgoingBlob);
            return outgoingBlob;
        }

        internal int Encrypt(byte[] buffer, int offset, int count, ref byte[] output, uint sequenceNumber) {
            SecSizes sizes = Sizes;

            try
            {
                int maxCount = checked(Int32.MaxValue - 4 - sizes.BlockSize - sizes.SecurityTrailer);

                if (count > maxCount || count < 0)
                {
                    throw new ArgumentOutOfRangeException("count", SR.GetString(SR.net_io_out_range, maxCount));
                }
            }
            catch(Exception e)
            {
                if (!NclUtilities.IsFatal(e)){
                    GlobalLog.Assert(false, "NTAuthentication#" + ValidationHelper.HashString(this) + "::Encrypt", "Arguments out of range.");
                }
                throw;
            }

            int resultSize = count + sizes.SecurityTrailer + sizes.BlockSize;
            if (output == null || output.Length < resultSize+4)
            {
                output = new byte[resultSize+4];
            }

            // make a copy of user data for in-place encryption
            Buffer.BlockCopy(buffer, offset, output, 4 + sizes.SecurityTrailer, count);

            // prepare buffers TOKEN(signautre), DATA and Padding
            SecurityBuffer[] securityBuffer = new SecurityBuffer[3];
            securityBuffer[0] = new SecurityBuffer(output, 4, sizes.SecurityTrailer, BufferType.Token);
            securityBuffer[1] = new SecurityBuffer(output, 4 + sizes.SecurityTrailer, count, BufferType.Data);
            securityBuffer[2] = new SecurityBuffer(output, 4 + sizes.SecurityTrailer + count, sizes.BlockSize, BufferType.Padding);

            int errorCode;
            if (IsConfidentialityFlag)
            {
                errorCode = SSPIWrapper.EncryptMessage(GlobalSSPI.SSPIAuth, m_SecurityContext, securityBuffer, sequenceNumber);
            }
            else
            {
                if (IsNTLM)
                    securityBuffer[1].type |= BufferType.ReadOnlyFlag;
                errorCode = SSPIWrapper.MakeSignature(GlobalSSPI.SSPIAuth, m_SecurityContext, securityBuffer, 0);
            }


            if (errorCode != 0) {
                GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::Encrypt() throw Error = " + errorCode.ToString("x", NumberFormatInfo.InvariantInfo));
                throw new Win32Exception(errorCode);
            }

            // Compacting the result...
            resultSize = securityBuffer[0].size;
            bool forceCopy = false;
            if (resultSize != sizes.SecurityTrailer)
            {
                forceCopy = true;
                Buffer.BlockCopy(output, securityBuffer[1].offset, output, 4 + resultSize, securityBuffer[1].size);
            }

            resultSize += securityBuffer[1].size;
            if (securityBuffer[2].size != 0 && (forceCopy || resultSize != (count + sizes.SecurityTrailer)))
                Buffer.BlockCopy(output, securityBuffer[2].offset, output, 4 + resultSize, securityBuffer[2].size);

            resultSize += securityBuffer[2].size;

            unchecked {
                output[0] = (byte)((resultSize) & 0xFF);
                output[1] = (byte)(((resultSize)>>8) & 0xFF);
                output[2] = (byte)(((resultSize)>>16) & 0xFF);
                output[3] = (byte)(((resultSize)>>24) & 0xFF);
            }
            return resultSize+4;
        }

        internal int Decrypt(byte[] payload, int offset, int count, out int newOffset, uint expectedSeqNumber)
        {
            if (offset < 0 || offset > (payload == null ? 0 : payload.Length))
            {
                GlobalLog.Assert(false, "NTAuthentication#" + ValidationHelper.HashString(this) + "::Decrypt", "Argument 'offset' out of range.");
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || count > (payload == null ? 0 : payload.Length - offset))
            {
                GlobalLog.Assert(false, "NTAuthentication#" + ValidationHelper.HashString(this) + "::Decrypt", "Argument 'count' out of range.");
                throw new ArgumentOutOfRangeException("count");
            }

            if (IsNTLM)
                return DecryptNtlm(payload, offset, count, out newOffset, expectedSeqNumber);

            //
            // Kerberos and up
            //

            SecurityBuffer[] securityBuffer = new SecurityBuffer[2];
            securityBuffer[0] = new SecurityBuffer(payload, offset, count, BufferType.Stream);
            securityBuffer[1] = new SecurityBuffer(0, BufferType.Data);

            int errorCode;
            if (IsConfidentialityFlag)
            {
                errorCode = SSPIWrapper.DecryptMessage(GlobalSSPI.SSPIAuth, m_SecurityContext, securityBuffer, expectedSeqNumber);
            }
            else
            {
                errorCode = SSPIWrapper.VerifySignature(GlobalSSPI.SSPIAuth, m_SecurityContext, securityBuffer, expectedSeqNumber);
            }

            if (errorCode != 0)
            {
                GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::Decrypt() throw Error = " + errorCode.ToString("x", NumberFormatInfo.InvariantInfo));
                throw new Win32Exception(errorCode);
            }

            if (securityBuffer[1].type != BufferType.Data)
                throw new InternalException();

            newOffset = securityBuffer[1].offset;
            return securityBuffer[1].size;

        }

        private string GetClientSpecifiedSpn()
        {
            GlobalLog.Assert(IsValidContext && IsCompleted, "NTAuthentication: Trying to get the client SPN before handshaking is done!");

            string spn = SSPIWrapper.QueryContextAttributes(GlobalSSPI.SSPIAuth, m_SecurityContext,
                ContextAttribute.ClientSpecifiedSpn) as string;

            GlobalLog.Print("NTAuthentication: The client specified SPN is [" + spn + "]");
            return spn;
        }
        
        //
        private int DecryptNtlm(byte[] payload, int offset, int count, out int newOffset, uint expectedSeqNumber)
        {
            // For the most part the arguments are verified in Encrypt().
            if (count < 16)
            {
                GlobalLog.Assert(false, "NTAuthentication#" + ValidationHelper.HashString(this) + "::DecryptNtlm", "Argument 'count' out of range.");
                throw new ArgumentOutOfRangeException("count");
            }

            SecurityBuffer[] securityBuffer = new SecurityBuffer[2];
            securityBuffer[0] = new SecurityBuffer(payload, offset, 16, BufferType.Token);
            securityBuffer[1] = new SecurityBuffer(payload, offset + 16, count-16, BufferType.Data);

            int errorCode;
            BufferType realDataType = BufferType.Data;

            if (IsConfidentialityFlag)
            {
                errorCode = SSPIWrapper.DecryptMessage(GlobalSSPI.SSPIAuth, m_SecurityContext, securityBuffer, expectedSeqNumber);
            }
            else
            {
                realDataType |= BufferType.ReadOnlyFlag;
                securityBuffer[1].type = realDataType;
                errorCode = SSPIWrapper.VerifySignature(GlobalSSPI.SSPIAuth, m_SecurityContext, securityBuffer, expectedSeqNumber);
            }

            if (errorCode != 0)
            {
                GlobalLog.Print("NTAuthentication#" + ValidationHelper.HashString(this) + "::Decrypt() throw Error = " + errorCode.ToString("x", NumberFormatInfo.InvariantInfo));
                throw new Win32Exception(errorCode);
            }

            if (securityBuffer[1].type != realDataType)
                throw new InternalException();

            newOffset = securityBuffer[1].offset;
            return securityBuffer[1].size;
        }
        
        //
        // VerifySignature
        // 
        // Adapted from Decrypt method above as a more generic message 
        // signature verify method for SMTP AUTH GSSAPI (SASL). 
        // Decrypt method, used NegotiateStream, couldn't be used due 
        // to special cases for NTLM.
        // 
        // See SmtpNegotiateAuthenticationModule class for caller.
        // 
        internal int VerifySignature(byte[] buffer, int offset, int count) {

            // validate offset within length
            if (offset < 0 || offset > (buffer == null ? 0 : buffer.Length)) {
                GlobalLog.Assert(
                            false, 
                            "NTAuthentication#" + 
                            ValidationHelper.HashString(this) + 
                            "::VerifySignature", 
                            "Argument 'offset' out of range.");
                throw new ArgumentOutOfRangeException("offset");
            }
            
            // validate count within offset and end of buffer
            if (count < 0 || 
                count > (buffer == null ? 0 : buffer.Length - offset)) {
                GlobalLog.Assert(
                            false, 
                            "NTAuthentication#" + 
                            ValidationHelper.HashString(this) + 
                            "::VerifySignature", 
                            "Argument 'count' out of range.");
                throw new ArgumentOutOfRangeException("count");
            }

            // setup security buffers for ssp call
            // one points at signed data
            // two will receive payload if signature is valid
            SecurityBuffer[] securityBuffer = new SecurityBuffer[2];
            securityBuffer[0] = 
                new SecurityBuffer(buffer, offset, count, BufferType.Stream);
            securityBuffer[1] = new SecurityBuffer(0, BufferType.Data);

            // call SSP function
            int errorCode = SSPIWrapper.VerifySignature(
                                GlobalSSPI.SSPIAuth, 
                                m_SecurityContext, 
                                securityBuffer, 
                                0);

            // throw if error
            if (errorCode != 0)
            {
                GlobalLog.Print(
                            "NTAuthentication#" + 
                            ValidationHelper.HashString(this) + 
                            "::VerifySignature() threw Error = " + 
                            errorCode.ToString("x", 
                                NumberFormatInfo.InvariantInfo));
                throw new Win32Exception(errorCode);
            }

            // not sure why this is here - retained from Encrypt code above
            if (securityBuffer[1].type != BufferType.Data)
                throw new InternalException();

            // return validated payload size 
            return securityBuffer[1].size;
        }
        
        //
        // MakeSignature
        // 
        // Adapted from Encrypt method above as a more generic message 
        // signing method for SMTP AUTH GSSAPI (SASL). 
        // Encrypt method, used for NegotiateStream, put size at head of
        // message.  Don't need that
        // 
        // See SmtpNegotiateAuthenticationModule class for caller.
        // 
        internal int MakeSignature(
                        byte[] buffer, 
                        int offset, 
                        int count, 
                        ref byte[] output) {
            SecSizes sizes = Sizes;


            // alloc new output buffer if not supplied or too small
            int resultSize = count + sizes.MaxSignature;
            if (output == null || output.Length < resultSize)
            {
                output = new byte[resultSize];
            }
          
            // make a copy of user data for in-place encryption
            Buffer.BlockCopy(buffer, offset, output, sizes.MaxSignature, count);
            
            // setup security buffers for ssp call
            SecurityBuffer[] securityBuffer = new SecurityBuffer[2];
            securityBuffer[0] = new SecurityBuffer(output, 0, sizes.MaxSignature, BufferType.Token);
            securityBuffer[1] = new SecurityBuffer(output, sizes.MaxSignature, count, BufferType.Data);

            // call SSP Function
            int errorCode = SSPIWrapper.MakeSignature(
                                GlobalSSPI.SSPIAuth, 
                                m_SecurityContext, 
                                securityBuffer, 
                                0);

            // throw if error
            if (errorCode != 0) {
                GlobalLog.Print(
                    "NTAuthentication#" + 
                    ValidationHelper.HashString(this) + 
                    "::Encrypt() throw Error = " + 
                    errorCode.ToString("x", NumberFormatInfo.InvariantInfo));
                throw new Win32Exception(errorCode);
            }

            // return signed size
            return securityBuffer[0].size + securityBuffer[1].size;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
    internal struct AuthIdentity {
        // see SEC_WINNT_AUTH_IDENTITY_W
        internal string UserName;
        internal int UserNameLength;
        internal string Domain;
        internal int DomainLength;
        internal string Password;
        internal int PasswordLength;
        internal int Flags;

        internal AuthIdentity(string userName, string password, string domain) {
            UserName = userName;
            UserNameLength = userName==null ? 0 : userName.Length;
            Password = password;
            PasswordLength = password==null ? 0 : password.Length;
            Domain = domain;
            DomainLength = domain==null ? 0 : domain.Length;
            // Flags are 2 for Unicode and 1 for ANSI. We use 2 on NT and 1 on Win9x.
            Flags = 2;
        }
        public override string ToString() {
            return ValidationHelper.ToString(Domain) + "\\" + ValidationHelper.ToString(UserName);
        }
    }

}
