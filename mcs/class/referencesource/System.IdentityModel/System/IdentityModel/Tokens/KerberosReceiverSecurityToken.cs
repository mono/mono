//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;
    using System.IdentityModel.Diagnostics;

    public class KerberosReceiverSecurityToken : WindowsSecurityToken
    {
        string id;
        byte[] request;
        SymmetricSecurityKey symmetricSecurityKey = null;
        ReadOnlyCollection<SecurityKey> securityKeys = null;
        bool isAuthenticated = false;
        string valueTypeUri = null;
        ChannelBinding channelBinding;
        ExtendedProtectionPolicy extendedProtectionPolicy;

        public KerberosReceiverSecurityToken(byte[] request)
            : this(request, SecurityUniqueId.Create().Value)
        { }

        public KerberosReceiverSecurityToken(byte[] request, string id)
            : this(request, id, true, null)
        {
        }

        public KerberosReceiverSecurityToken(byte[] request, string id, string valueTypeUri)
            : this(request, id, true, valueTypeUri)
        {
        }

        internal KerberosReceiverSecurityToken( byte[] request, string id, bool doAuthenticate, string valueTypeUri )
            : this(request, id, doAuthenticate, valueTypeUri, null, null)
        { }

        internal KerberosReceiverSecurityToken( 
                                byte[] request, 
                                string id, 
                                bool doAuthenticate, 
                                string valueTypeUri, 
                                ChannelBinding channelBinding, 
                                ExtendedProtectionPolicy extendedProtectionPolicy )
        {
            if (request == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("request"));
            if (id == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("id"));

            this.id = id;
            this.request = request;
            this.valueTypeUri = valueTypeUri;
            this.channelBinding = channelBinding;
            this.extendedProtectionPolicy = extendedProtectionPolicy;

            if (doAuthenticate)
            {
                Initialize(null, channelBinding, extendedProtectionPolicy);
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                if (this.securityKeys == null)
                {
                    List<SecurityKey> temp = new List<SecurityKey>(1);
                    temp.Add(this.SecurityKey);
                    this.securityKeys = temp.AsReadOnly();
                }
                return this.securityKeys;
            }
        }

        public SymmetricSecurityKey SecurityKey
        {
            get 
            {
                if (!this.isAuthenticated)
                {
                    Initialize(null, this.channelBinding, this.extendedProtectionPolicy);
                }
                return this.symmetricSecurityKey; 
            }
        }

        public override DateTime ValidFrom
        {
            get
            {
                if (!this.isAuthenticated)
                {
                    Initialize(null, this.channelBinding, this.extendedProtectionPolicy);
                }
                return base.ValidFrom;
            }
        }

        public override DateTime ValidTo
        {
            get
            {
                if (!this.isAuthenticated)
                {
                    Initialize(null, this.channelBinding, this.extendedProtectionPolicy);
                }
                return base.ValidTo;
            }
        }

        public override WindowsIdentity WindowsIdentity
        {
            get
            {
                ThrowIfDisposed();
                if (!this.isAuthenticated)
                {
                    Initialize(null, this.channelBinding, this.extendedProtectionPolicy);
                }
                return base.WindowsIdentity;
            }
        }

        /// <summary>
        /// The Uri that defines the ValueType of the kerberos blob.
        /// </summary>
        public string ValueTypeUri
        {
            get
            {
                return valueTypeUri;
            }
        }

        public byte[] GetRequest()
        {
            return SecurityUtils.CloneBuffer(this.request);
        }

        // This internal API is not thread-safe.  It is acceptable since ..
        // 1) From public OM, Initialize happens at ctor time.
        // 2) From internal OM (Sfx), Initialize happens right after ctor (single thread env).
        //    i.e. ReadToken and then AuthenticateToken.
        internal void Initialize( SafeFreeCredentials credentialsHandle, ChannelBinding channelBinding, ExtendedProtectionPolicy extendedProtectionPolicy )
        {
            if (this.isAuthenticated)
            {
                return;
            }
            bool ownCredentialsHandle = false;
            SafeDeleteContext securityContext = null;
            SafeCloseHandle tokenHandle = null;

#if RECOMPUTEGSS
            int tokenSize = DEREncoding.TokenSize(this.request.Length);
            byte[] rawRequest = new byte[tokenSize];
            int offset = 0;
            int len = this.request.Length;
            DEREncoding.MakeTokenHeader(this.request.Length, rawRequest, ref offset, ref len);
            System.Buffer.BlockCopy(this.request, 0, rawRequest, offset, this.request.Length);
#else
            byte[] rawRequest = this.request;
#endif

            try
            {
                if (credentialsHandle == null)
                {
                    credentialsHandle = SspiWrapper.AcquireDefaultCredential("Kerberos", CredentialUse.Inbound);
                    ownCredentialsHandle = true;
                }

                SspiContextFlags fContextReq = SspiContextFlags.AllocateMemory | SspiContextFlags.Confidentiality
                                             | SspiContextFlags.Confidentiality
                                             | SspiContextFlags.ReplayDetect 
                                             | SspiContextFlags.SequenceDetect;

                ExtendedProtectionPolicyHelper policyHelper = new ExtendedProtectionPolicyHelper(channelBinding, extendedProtectionPolicy);

                if (policyHelper.PolicyEnforcement == PolicyEnforcement.Always && policyHelper.ChannelBinding == null && policyHelper.ProtectionScenario != ProtectionScenario.TrustedProxy)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SecurityChannelBindingMissing)));
                }

                if (policyHelper.PolicyEnforcement == PolicyEnforcement.WhenSupported)
                {
                    fContextReq |= SspiContextFlags.ChannelBindingAllowMissingBindings;
                }

                if (policyHelper.ProtectionScenario == ProtectionScenario.TrustedProxy)
                {
                    fContextReq |= SspiContextFlags.ChannelBindingProxyBindings;
                }

                SspiContextFlags contextFlags = SspiContextFlags.Zero;
                SecurityBuffer outSecurityBuffer = new SecurityBuffer(0, BufferType.Token);

                List<SecurityBuffer> list = new List<SecurityBuffer>(2);
                list.Add(new SecurityBuffer(rawRequest, BufferType.Token));

                if (policyHelper.ShouldAddChannelBindingToASC())
                {
                    list.Add(new SecurityBuffer(policyHelper.ChannelBinding));
                }

                SecurityBuffer[] inSecurityBuffer = null;
                if (list.Count > 0)
                {
                    inSecurityBuffer = list.ToArray();
                }

                int statusCode = SspiWrapper.AcceptSecurityContext(credentialsHandle,
                    ref securityContext,
                    fContextReq,
                    Endianness.Native,
                    inSecurityBuffer,
                    outSecurityBuffer,
                    ref contextFlags);


                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    SecurityTraceRecordHelper.TraceChannelBindingInformation(policyHelper, true, channelBinding);
                }

                if (statusCode != (int)SecurityStatus.OK)
                {
                    if (statusCode == (int)SecurityStatus.ContinueNeeded)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new SecurityTokenException(SR.GetString(SR.KerberosMultilegsNotSupported), new Win32Exception(statusCode)));
                    }
                    else if (statusCode == (int)SecurityStatus.OutOfMemory)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new SecurityTokenException(SR.GetString(SR.KerberosApReqInvalidOrOutOfMemory), new Win32Exception(statusCode)));
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new SecurityTokenException(SR.GetString(SR.FailAcceptSecurityContext), new Win32Exception(statusCode)));
                    }
                }

                // Expiration
                LifeSpan lifeSpan = (LifeSpan)SspiWrapper.QueryContextAttributes(securityContext, ContextAttribute.Lifespan);
                DateTime effectiveTime = lifeSpan.EffectiveTimeUtc;
                DateTime expirationTime = lifeSpan.ExpiryTimeUtc;

                // SessionKey
                SecuritySessionKeyClass sessionKey = (SecuritySessionKeyClass)SspiWrapper.QueryContextAttributes(securityContext, ContextAttribute.SessionKey);
                this.symmetricSecurityKey = new InMemorySymmetricSecurityKey(sessionKey.SessionKey);

                // WindowsSecurityToken
                statusCode = SspiWrapper.QuerySecurityContextToken(securityContext, out tokenHandle);
                if (statusCode != (int)SecurityStatus.OK)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(statusCode));
                }

                WindowsIdentity windowsIdentity = new WindowsIdentity( tokenHandle.DangerousGetHandle(), SecurityUtils.AuthTypeKerberos);
                Initialize(this.id, SecurityUtils.AuthTypeKerberos, effectiveTime, expirationTime, windowsIdentity, false);

                // Authenticated
                this.isAuthenticated = true;
            }
            finally
            {
                if (tokenHandle != null)
                    tokenHandle.Close();

                if (securityContext != null)
                    securityContext.Close();

                if (ownCredentialsHandle && credentialsHandle != null)
                    credentialsHandle.Close();
            }
        }

        public override bool CanCreateKeyIdentifierClause<T>()
        {
            if (typeof(T) == typeof(KerberosTicketHashKeyIdentifierClause))
                return true;

            return base.CanCreateKeyIdentifierClause<T>();
        }

        public override T CreateKeyIdentifierClause<T>()
        {
            if (typeof(T) == typeof(KerberosTicketHashKeyIdentifierClause))
                return new KerberosTicketHashKeyIdentifierClause(CryptoHelper.ComputeHash(this.request), false, null, 0) as T;

            return base.CreateKeyIdentifierClause<T>();
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            KerberosTicketHashKeyIdentifierClause kerbKeyIdentifierClause = keyIdentifierClause as KerberosTicketHashKeyIdentifierClause;
            if (kerbKeyIdentifierClause != null)
                return kerbKeyIdentifierClause.Matches(CryptoHelper.ComputeHash(this.request));

            return base.MatchesKeyIdentifierClause(keyIdentifierClause);
        }
    }
}
