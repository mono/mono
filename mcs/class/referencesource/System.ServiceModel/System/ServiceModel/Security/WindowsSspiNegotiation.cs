//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Globalization;
    using System.ComponentModel;
    using System.Security.Principal;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.Security.Authentication.ExtendedProtection;
    using IMD = System.IdentityModel.Diagnostics;

    using DiagnosticUtility = System.ServiceModel.DiagnosticUtility;
    using SR = System.ServiceModel.SR;

    internal sealed class WindowsSspiNegotiation : ISspiNegotiation
    {
        const int DefaultMaxPromptAttempts = 1;
        SspiContextFlags contextFlags;
        SafeFreeCredentials credentialsHandle;
        bool disposed = false;
        bool doMutualAuth;
        TokenImpersonationLevel impersonationLevel;
        bool isCompleted;
        bool isServer;
        LifeSpan lifespan;
        string protocolName;
        SafeDeleteContext securityContext;
        string servicePrincipalName;
        SecSizes sizes;
        Object syncObject = new Object();
        int tokenSize;
        bool interactiveNegoLogonEnabled = true;
        string clientPackageName;
        bool saveClientCredentialsOnSspiUi = true;
        bool allowNtlm;
        int MaxPromptAttempts = 0;

        /// <summary>
        /// Client side ctor
        /// </summary>
        internal WindowsSspiNegotiation(string package, SafeFreeCredentials credentialsHandle, TokenImpersonationLevel impersonationLevel, string servicePrincipalName, bool doMutualAuth, bool interactiveLogonEnabled, bool ntlmEnabled)
            : this(false, package, credentialsHandle, impersonationLevel, servicePrincipalName, doMutualAuth, interactiveLogonEnabled, ntlmEnabled)
        { }

        /// <summary>
        /// Server side ctor
        /// </summary>
        internal WindowsSspiNegotiation(string package, SafeFreeCredentials credentialsHandle, string defaultServiceBinding)
            : this(true, package, credentialsHandle, TokenImpersonationLevel.Delegation, defaultServiceBinding, false, false, true)
        { }

        WindowsSspiNegotiation(bool isServer, string package, SafeFreeCredentials credentialsHandle, TokenImpersonationLevel impersonationLevel, string servicePrincipalName, bool doMutualAuth, bool interactiveLogonEnabled, bool ntlmEnabled)
        {
            this.tokenSize = SspiWrapper.GetVerifyPackageInfo(package).MaxToken;
            this.isServer = isServer;
            this.servicePrincipalName = servicePrincipalName;
            this.securityContext = null;
            if (isServer)
            {
                this.impersonationLevel = TokenImpersonationLevel.Delegation;
                this.doMutualAuth = false;
            }
            else
            {
                this.impersonationLevel = impersonationLevel;
                this.doMutualAuth = doMutualAuth;
                this.interactiveNegoLogonEnabled = interactiveLogonEnabled;
                this.clientPackageName = package;
                this.allowNtlm = ntlmEnabled;
            }
            this.credentialsHandle = credentialsHandle;
        }

        public DateTime ExpirationTimeUtc
        {
            get
            {
                ThrowIfDisposed();
                if (this.LifeSpan == null)
                {
                    return SecurityUtils.MaxUtcDateTime;
                }
                else
                {
                    return this.LifeSpan.ExpiryTimeUtc;
                }
            }
        }

        public bool IsCompleted
        {
            get
            {
                ThrowIfDisposed();
                return this.isCompleted;
            }
        }

        public bool IsDelegationFlag
        {
            get
            {
                ThrowIfDisposed();
                return (this.contextFlags & SspiContextFlags.Delegate) != 0;
            }
        }

        public bool IsIdentifyFlag
        {
            get
            {
                ThrowIfDisposed();
                return (this.contextFlags & (this.isServer ? SspiContextFlags.AcceptIdentify : SspiContextFlags.InitIdentify)) != 0;
            }
        }

        public bool IsMutualAuthFlag
        {
            get
            {
                ThrowIfDisposed();
                return (this.contextFlags & SspiContextFlags.MutualAuth) != 0;
            }
        }

        public bool IsValidContext
        {
            get
            {
                return (this.securityContext != null && this.securityContext.IsInvalid == false);
            }
        }

        public string KeyEncryptionAlgorithm
        {
            get
            {
                return SecurityAlgorithms.WindowsSspiKeyWrap;
            }
        }

        public LifeSpan LifeSpan
        {
            get
            {
                ThrowIfDisposed();
                if (this.lifespan == null)
                {
                    LifeSpan tmpLifeSpan = (LifeSpan)SspiWrapper.QueryContextAttributes(this.securityContext, ContextAttribute.Lifespan);

                    if (IsCompleted)
                    {
                        // cache it only when it's completed
                        this.lifespan = tmpLifeSpan;
                    }

                    return tmpLifeSpan;
                }

                return this.lifespan;
            }
        }

        public string ProtocolName
        {
            get
            {
                ThrowIfDisposed();
                if (this.protocolName == null)
                {
                    NegotiationInfoClass negotiationInfo = SspiWrapper.QueryContextAttributes(this.securityContext, ContextAttribute.NegotiationInfo) as NegotiationInfoClass;

                    if (IsCompleted)
                    {
                        // cache it only when it's completed
                        this.protocolName = negotiationInfo.AuthenticationPackage;
                    }

                    return negotiationInfo.AuthenticationPackage;
                }

                return this.protocolName;
            }
        }

        public string ServicePrincipalName
        {
            get
            {
                ThrowIfDisposed();
                return this.servicePrincipalName;
            }
        }

        SecSizes SecuritySizes
        {
            get
            {
                ThrowIfDisposed();
                if (this.sizes == null)
                {
                    SecSizes tmpSizes = (SecSizes)SspiWrapper.QueryContextAttributes(this.securityContext, ContextAttribute.Sizes);

                    if (IsCompleted)
                    {
                        // cache it only when it's completed
                        this.sizes = tmpSizes;
                    }

                    return tmpSizes;
                }

                return this.sizes;
            }
        }

        public string GetRemoteIdentityName()
        {
            if (!this.isServer)
            {
                return this.servicePrincipalName;
            }

            if (IsValidContext)
            {
                using (SafeCloseHandle contextToken = GetContextToken())
                {
                    using (WindowsIdentity windowsIdentity = new WindowsIdentity(contextToken.DangerousGetHandle(), this.ProtocolName))
                    {
                        return windowsIdentity.Name;
                    }
                }
            }
            return String.Empty;
        }

        public byte[] Decrypt(byte[] encryptedContent)
        {
            if (encryptedContent == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encryptedContent");
            ThrowIfDisposed();

            SecurityBuffer[] securityBuffer = new SecurityBuffer[2];
            securityBuffer[0] = new SecurityBuffer(encryptedContent, 0, encryptedContent.Length, BufferType.Stream);
            securityBuffer[1] = new SecurityBuffer(0, BufferType.Data);
            int errorCode = SspiWrapper.DecryptMessage(this.securityContext, securityBuffer, 0, true);
            if (errorCode != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
            }

            for (int i = 0; i < securityBuffer.Length; ++i)
            {
                if (securityBuffer[i].type == BufferType.Data)
                {
                    return securityBuffer[i].token;
                }
            }
            OnBadData();
            return null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public byte[] Encrypt(byte[] input)
        {
            if (input == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("input");
            ThrowIfDisposed();
            SecurityBuffer[] securityBuffer = new SecurityBuffer[3];

            byte[] tokenBuffer = DiagnosticUtility.Utility.AllocateByteArray(SecuritySizes.SecurityTrailer);
            securityBuffer[0] = new SecurityBuffer(tokenBuffer, 0, tokenBuffer.Length, BufferType.Token);
            byte[] dataBuffer = DiagnosticUtility.Utility.AllocateByteArray(input.Length);
            Buffer.BlockCopy(input, 0, dataBuffer, 0, input.Length);
            securityBuffer[1] = new SecurityBuffer(dataBuffer, 0, dataBuffer.Length, BufferType.Data);
            byte[] paddingBuffer = DiagnosticUtility.Utility.AllocateByteArray(SecuritySizes.BlockSize);
            securityBuffer[2] = new SecurityBuffer(paddingBuffer, 0, paddingBuffer.Length, BufferType.Padding);

            int errorCode = SspiWrapper.EncryptMessage(this.securityContext, securityBuffer, 0);
            if (errorCode != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
            }

            int tokenLen = 0;
            int paddingLen = 0;
            for (int i = 0; i < securityBuffer.Length; ++i)
            {
                if (securityBuffer[i].type == BufferType.Token)
                    tokenLen = securityBuffer[i].size;
                else if (securityBuffer[i].type == BufferType.Padding)
                    paddingLen = securityBuffer[i].size;
            }
            byte[] encryptedData = DiagnosticUtility.Utility.AllocateByteArray(checked(tokenLen + dataBuffer.Length + paddingLen));

            Buffer.BlockCopy(tokenBuffer, 0, encryptedData, 0, tokenLen);
            Buffer.BlockCopy(dataBuffer, 0, encryptedData, tokenLen, dataBuffer.Length);
            Buffer.BlockCopy(paddingBuffer, 0, encryptedData, tokenLen + dataBuffer.Length, paddingLen);

            return encryptedData;
        }

        public byte[] GetOutgoingBlob(byte[] incomingBlob, ChannelBinding channelbinding, ExtendedProtectionPolicy protectionPolicy)
        {
            ThrowIfDisposed();
            int statusCode = 0;

            // use the confidentiality option to ensure we can encrypt messages
            SspiContextFlags requestedFlags = SspiContextFlags.Confidentiality
                                            | SspiContextFlags.ReplayDetect
                                            | SspiContextFlags.SequenceDetect;

            if (this.doMutualAuth)
            {
                requestedFlags |= SspiContextFlags.MutualAuth;
            }

            if (this.impersonationLevel == TokenImpersonationLevel.Delegation)
            {
                requestedFlags |= SspiContextFlags.Delegate;
            }
            else if (this.isServer == false && this.impersonationLevel == TokenImpersonationLevel.Identification)
            {
                requestedFlags |= SspiContextFlags.InitIdentify;
            }
            else if (this.isServer == false && this.impersonationLevel == TokenImpersonationLevel.Anonymous)
            {
                requestedFlags |= SspiContextFlags.InitAnonymous;
            }

            ExtendedProtectionPolicyHelper policyHelper = new ExtendedProtectionPolicyHelper(channelbinding, protectionPolicy);

            if (isServer)
            {
                if (policyHelper.PolicyEnforcement == PolicyEnforcement.Always && policyHelper.ChannelBinding == null && policyHelper.ProtectionScenario != ProtectionScenario.TrustedProxy)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SecurityChannelBindingMissing)));
                }

                if (policyHelper.PolicyEnforcement == PolicyEnforcement.WhenSupported)
                {
                    requestedFlags |= SspiContextFlags.ChannelBindingAllowMissingBindings;
                }

                if (policyHelper.ProtectionScenario == ProtectionScenario.TrustedProxy)
                {
                    requestedFlags |= SspiContextFlags.ChannelBindingProxyBindings;
                }
            }

            List<SecurityBuffer> list = new List<SecurityBuffer>(2);

            if (incomingBlob != null)
            {
                list.Add(new SecurityBuffer(incomingBlob, BufferType.Token));
            }

            // when deciding if the channel binding should be added to the security buffer
            // it is necessary to differentiate between  client and server.
            // Server rules were added to policyHelper as they are shared with Kerb and I want them consistent
            // Client adds if not null.
            if (this.isServer)
            {
                if (policyHelper.ShouldAddChannelBindingToASC())
                {
                    list.Add(new SecurityBuffer(policyHelper.ChannelBinding));
                }
            }
            else
            {
                if (policyHelper.ChannelBinding != null)
                {
                    list.Add(new SecurityBuffer(policyHelper.ChannelBinding));
                }
            }

            SecurityBuffer[] inSecurityBuffer = null;
            if (list.Count > 0)
            {
                inSecurityBuffer = list.ToArray();
            }

            SecurityBuffer outSecurityBuffer = new SecurityBuffer(this.tokenSize, BufferType.Token);

            if (!this.isServer)
            {
                //client session
                statusCode = SspiWrapper.InitializeSecurityContext(this.credentialsHandle,
                                                                    ref this.securityContext,
                                                                    this.servicePrincipalName,
                                                                    requestedFlags,
                                                                    Endianness.Network,
                                                                    inSecurityBuffer,
                                                                    outSecurityBuffer,
                                                                    ref this.contextFlags);
            }
            else
            {
                // server session
                //This check is to save an unnecessary ASC call.
                bool isServerSecurityContextNull = this.securityContext == null;
                SspiContextFlags serverContextFlags = this.contextFlags;

                statusCode = SspiWrapper.AcceptSecurityContext(this.credentialsHandle,
                                                                ref this.securityContext,
                                                                requestedFlags,
                                                                Endianness.Network,
                                                                inSecurityBuffer,
                                                                outSecurityBuffer,
                                                                ref this.contextFlags);

                if (statusCode == (int)SecurityStatus.InvalidToken && !isServerSecurityContextNull)
                {
                    // Call again into ASC after deleting the Securitycontext. If this securitycontext is not deleted 
                    // then when the client sends NTLM blob the service will treat it as Nego2blob and will fail to authenticate the client.
                    this.contextFlags = serverContextFlags;
                    CloseContext();
                    statusCode = SspiWrapper.AcceptSecurityContext(this.credentialsHandle,
                                                                    ref this.securityContext,
                                                                    requestedFlags,
                                                                    Endianness.Network,
                                                                    inSecurityBuffer,
                                                                    outSecurityBuffer,
                                                                    ref this.contextFlags);
                }
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                IMD.SecurityTraceRecordHelper.TraceChannelBindingInformation(policyHelper, this.isServer, channelbinding);
            }

            if ((statusCode & unchecked((int)0x80000000)) != 0)
            {
                if (!this.isServer
                    && this.interactiveNegoLogonEnabled
                    && SecurityUtils.IsOSGreaterThanOrEqualToWin7()
                    && SspiWrapper.IsSspiPromptingNeeded((uint)statusCode)
                    && SspiWrapper.IsNegotiateExPackagePresent())
                {
                    // If we have prompted enough number of times (DefaultMaxPromptAttempts) with wrong credentials, then we do not prompt again and throw. 
                    if (MaxPromptAttempts >= DefaultMaxPromptAttempts)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(statusCode, SR.GetString(SR.InvalidClientCredentials)));
                    }

                    IntPtr ppAuthIdentity = IntPtr.Zero;
                    uint errorCode = SspiWrapper.SspiPromptForCredential(this.servicePrincipalName, this.clientPackageName, out ppAuthIdentity, ref this.saveClientCredentialsOnSspiUi);
                    if (errorCode == (uint)CredentialStatus.Success)
                    {
                        IntPtr ppNewAuthIdentity = IntPtr.Zero;

                        if (!this.allowNtlm)
                        {
                            // When Ntlm is  explicitly disabled we don't want the collected 
                            //creds from the Kerb/NTLM tile to be used for NTLM auth.

                            uint status = UnsafeNativeMethods.SspiExcludePackage(ppAuthIdentity, "NTLM", out ppNewAuthIdentity);
                        }
                        else
                        {
                            ppNewAuthIdentity = ppAuthIdentity;
                        }

                        this.credentialsHandle = SspiWrapper.AcquireCredentialsHandle(this.clientPackageName, CredentialUse.Outbound, ref ppNewAuthIdentity);

                        if (IntPtr.Zero != ppNewAuthIdentity)
                        {
                            UnsafeNativeMethods.SspiFreeAuthIdentity(ppNewAuthIdentity);
                        }

                        CloseContext();

                        MaxPromptAttempts++;
                        return this.GetOutgoingBlob(null, channelbinding, protectionPolicy);
                    }
                    else
                    {
                        // Call into SspiPromptForCredential had an error. Time to throw.
                        if (IntPtr.Zero != ppAuthIdentity)
                        {
                            UnsafeNativeMethods.SspiFreeAuthIdentity(ppAuthIdentity);
                        }

                        CloseContext();
                        this.isCompleted = true;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception((int)errorCode, SR.GetString(SR.SspiErrorOrInvalidClientCredentials)));
                    }
                }

                CloseContext();
                this.isCompleted = true;
                if (!this.isServer && (statusCode == (int)SecurityStatus.TargetUnknown
                    || statusCode == (int)SecurityStatus.WrongPrincipal))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(statusCode, SR.GetString(SR.IncorrectSpnOrUpnSpecified, this.servicePrincipalName)));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(statusCode, SR.GetString(SR.InvalidSspiNegotiation)));
                }
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                if (this.isServer)
                {
                    SecurityTraceRecordHelper.TraceServiceOutgoingSpnego(this);
                }
                else
                {
                    SecurityTraceRecordHelper.TraceClientOutgoingSpnego(this);
                }
            }

            if (statusCode == (int)SecurityStatus.OK)
            {
                // we're done
                this.isCompleted = true;

                // These must all be true to check service binding
                // 1. we are the service (listener)
                // 2. caller is not anonymous
                // 3. protocol is not Kerberos
                // 4. policy is set to check service binding
                // 
                if (isServer && ((this.contextFlags & SspiContextFlags.AcceptAnonymous) == 0) && (string.Compare(this.ProtocolName, NegotiationInfoClass.Kerberos, StringComparison.OrdinalIgnoreCase) != 0) && policyHelper.ShouldCheckServiceBinding)
                {
                    // in the server case the servicePrincipalName is the defaultServiceBinding
                   
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        string serviceBindingNameSentByClient;
                        SspiWrapper.QuerySpecifiedTarget(securityContext, out serviceBindingNameSentByClient);
                        IMD.SecurityTraceRecordHelper.TraceServiceNameBindingOnServer( serviceBindingNameSentByClient, this.servicePrincipalName, policyHelper.ServiceNameCollection);
                    }
                    
                    policyHelper.CheckServiceBinding(this.securityContext, this.servicePrincipalName);
                }
            }
            else
            {
                // we need to continue
            }

            return outSecurityBuffer.token;
        }

        public void ImpersonateContext()
        {
            ThrowIfDisposed();
            if (!IsValidContext)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception((int)SecurityStatus.InvalidHandle));
            }

            SspiWrapper.ImpersonateSecurityContext(this.securityContext);
        }

        internal void CloseContext()
        {
            ThrowIfDisposed();
            try
            {
                if (this.securityContext != null)
                {
                    this.securityContext.Close();
                }
            }
            finally
            {
                this.securityContext = null;
            }
        }

        private void Dispose(bool disposing)
        {
            lock (this.syncObject)
            {
                if (this.disposed == false)
                {
                    if (disposing)
                    {
                        this.CloseContext();
                    }

                    // set to null any references that aren't finalizable
                    this.protocolName = null;
                    this.servicePrincipalName = null;
                    this.sizes = null;
                    this.disposed = true;
                }
            }
        }

        internal SafeCloseHandle GetContextToken()
        {
            if (!IsValidContext)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception((int)SecurityStatus.InvalidHandle));
            }

            SafeCloseHandle token;
            SecurityStatus status = (SecurityStatus)SspiWrapper.QuerySecurityContextToken(this.securityContext, out token);
            if (status != SecurityStatus.OK)
            {
                Utility.CloseInvalidOutSafeHandle(token);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception((int)status));
            }
            return token;
        }

        void OnBadData()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.BadData)));
        }

        void ThrowIfDisposed()
        {
            lock (this.syncObject)
            {
                if (this.disposed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(null));
                }
            }
        }
    }
}
