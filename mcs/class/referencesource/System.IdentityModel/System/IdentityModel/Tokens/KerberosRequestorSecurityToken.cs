//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.Security.Authentication.ExtendedProtection;
    using System.IdentityModel.Diagnostics;

    public class KerberosRequestorSecurityToken : SecurityToken
    {
        string id;
        byte[] apreq;
        readonly string servicePrincipalName;
        SymmetricSecurityKey symmetricSecurityKey;
        ReadOnlyCollection<SecurityKey> securityKeys;
        DateTime effectiveTime;
        DateTime expirationTime;

        public KerberosRequestorSecurityToken(string servicePrincipalName)
            : this(servicePrincipalName, TokenImpersonationLevel.Impersonation, null, SecurityUniqueId.Create().Value, null)
        {
        }

        public KerberosRequestorSecurityToken(string servicePrincipalName, TokenImpersonationLevel tokenImpersonationLevel, NetworkCredential networkCredential, string id)
            : this(servicePrincipalName, tokenImpersonationLevel, networkCredential, id, null, null)
        {
        }

        internal KerberosRequestorSecurityToken(string servicePrincipalName, TokenImpersonationLevel tokenImpersonationLevel, NetworkCredential networkCredential, string id, ChannelBinding channelBinding)
            : this(servicePrincipalName, tokenImpersonationLevel, networkCredential, id, null, channelBinding)
        {
        }

        internal KerberosRequestorSecurityToken(string servicePrincipalName, TokenImpersonationLevel tokenImpersonationLevel, NetworkCredential networkCredential, string id, SafeFreeCredentials credentialsHandle, ChannelBinding channelBinding)
        {
            if (servicePrincipalName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("servicePrincipalName");
            if (tokenImpersonationLevel != TokenImpersonationLevel.Identification && tokenImpersonationLevel != TokenImpersonationLevel.Impersonation)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenImpersonationLevel",
                    SR.GetString(SR.ImpersonationLevelNotSupported, tokenImpersonationLevel)));
            }
            if (id == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("id");

            this.servicePrincipalName = servicePrincipalName;
            if (networkCredential != null && networkCredential != CredentialCache.DefaultNetworkCredentials)
            {
                if (string.IsNullOrEmpty(networkCredential.UserName))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.ProvidedNetworkCredentialsForKerberosHasInvalidUserName));
                }
                // Note: we don't check the domain, since Lsa accepts
                // FQ userName.
            }
            this.id = id;
            try
            {
                Initialize(tokenImpersonationLevel, networkCredential, credentialsHandle, channelBinding);
            }
            catch (Win32Exception e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.UnableToCreateKerberosCredentials), e));
            }
            catch (SecurityTokenException ste)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.UnableToCreateKerberosCredentials), ste));
            }
        }

        public override string Id
        {
            get { return this.id; }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                if (this.securityKeys == null)
                {
                    List<SecurityKey> temp = new List<SecurityKey>(1);
                    temp.Add(this.symmetricSecurityKey);
                    this.securityKeys = temp.AsReadOnly();
                }
                return this.securityKeys;
            }
        }

        public override DateTime ValidFrom
        {
            get { return this.effectiveTime; }
        }

        public override DateTime ValidTo
        {
            get { return this.expirationTime; }
        }

        public string ServicePrincipalName
        {
            get { return this.servicePrincipalName; }
        }

        public SymmetricSecurityKey SecurityKey
        {
            get
            {
                return this.symmetricSecurityKey;
            }
        }

        public byte[] GetRequest()
        {
            return SecurityUtils.CloneBuffer(this.apreq);
        }

        void Initialize(TokenImpersonationLevel tokenImpersonationLevel, NetworkCredential networkCredential, SafeFreeCredentials credentialsHandle, ChannelBinding channelBinding)
        {
            bool ownCredentialsHandle = false;
            SafeDeleteContext securityContext = null;

            try
            {
                if (credentialsHandle == null)
                {
                    if (networkCredential == null || networkCredential == CredentialCache.DefaultNetworkCredentials)
                    {
                        credentialsHandle = SspiWrapper.AcquireDefaultCredential("Kerberos", CredentialUse.Outbound);
                    }
                    else
                    {
                        AuthIdentityEx authIdentity = new AuthIdentityEx(networkCredential.UserName, networkCredential.Password, networkCredential.Domain);
                        credentialsHandle = SspiWrapper.AcquireCredentialsHandle("Kerberos", CredentialUse.Outbound, ref authIdentity);
                    }
                    ownCredentialsHandle = true;
                }

                SspiContextFlags fContextReq = SspiContextFlags.AllocateMemory
                                             | SspiContextFlags.Confidentiality
                                             | SspiContextFlags.ReplayDetect
                                             | SspiContextFlags.SequenceDetect;


                // we only accept Identity or Impersonation (Impersonation is default).
                if (tokenImpersonationLevel == TokenImpersonationLevel.Identification)
                {
                    fContextReq |= SspiContextFlags.InitIdentify;
                }

                SspiContextFlags contextFlags = SspiContextFlags.Zero;
                SecurityBuffer inSecurityBuffer = null;
                if (channelBinding != null)
                {
                    inSecurityBuffer = new SecurityBuffer(channelBinding);
                }
                SecurityBuffer outSecurityBuffer = new SecurityBuffer(0, BufferType.Token);

                int statusCode = SspiWrapper.InitializeSecurityContext(
                                    credentialsHandle,
                                    ref securityContext,
                                    this.servicePrincipalName,
                                    fContextReq,
                                    Endianness.Native,
                                    inSecurityBuffer,
                                    outSecurityBuffer,
                                    ref contextFlags);

                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    SecurityTraceRecordHelper.TraceChannelBindingInformation(null, false, channelBinding);
                }

                if (statusCode != (int)SecurityStatus.OK)
                {
                    if (statusCode == (int)SecurityStatus.ContinueNeeded)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new SecurityTokenException(SR.GetString(SR.KerberosMultilegsNotSupported), new Win32Exception(statusCode)));
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new SecurityTokenException(SR.GetString(SR.FailInitializeSecurityContext), new Win32Exception(statusCode)));
                    }
                }

#if REMOVEGSS
                //
                // ... and strip GSS-framing from it
                //
                int offset = 0;
                int len = outSecurityBuffer.token.Length;
                DEREncoding.VerifyTokenHeader(outSecurityBuffer.token, ref offset, ref len);
                this.apreq = SecurityUtils.CloneBuffer(outSecurityBuffer.token, offset, len);
#else
                this.apreq = outSecurityBuffer.token;
#endif

                // Expiration
                LifeSpan lifeSpan = (LifeSpan)SspiWrapper.QueryContextAttributes(securityContext, ContextAttribute.Lifespan);
                this.effectiveTime = lifeSpan.EffectiveTimeUtc;
                this.expirationTime = lifeSpan.ExpiryTimeUtc;

                // SessionKey
                SecuritySessionKeyClass sessionKey = (SecuritySessionKeyClass)SspiWrapper.QueryContextAttributes(securityContext, ContextAttribute.SessionKey);
                this.symmetricSecurityKey = new InMemorySymmetricSecurityKey(sessionKey.SessionKey);
            }
            finally
            {
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
                return new KerberosTicketHashKeyIdentifierClause(CryptoHelper.ComputeHash(this.apreq), false, null, 0) as T;

            return base.CreateKeyIdentifierClause<T>();
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            KerberosTicketHashKeyIdentifierClause kerbKeyIdentifierClause = keyIdentifierClause as KerberosTicketHashKeyIdentifierClause;
            if (kerbKeyIdentifierClause != null)
                return kerbKeyIdentifierClause.Matches(CryptoHelper.ComputeHash(this.apreq));

            return base.MatchesKeyIdentifierClause(keyIdentifierClause);
        }
    }
}
