//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{

    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Net;
    using System.Runtime;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    using SafeFreeCredentials = System.IdentityModel.SafeFreeCredentials;
    using System.ServiceModel.Description;

    class SpnegoTokenProvider : SspiNegotiationTokenProvider
    {
        TokenImpersonationLevel allowedImpersonationLevel = TokenImpersonationLevel.Identification;
        ICredentials clientCredential;
        IdentityVerifier identityVerifier = IdentityVerifier.CreateDefault();
        bool allowNtlm = true;
        bool authenticateServer = true;
        SafeFreeCredentials credentialsHandle;
        bool ownCredentialsHandle = false;
        bool interactiveNegoExLogonEnabled = true;

        public SpnegoTokenProvider(SafeFreeCredentials credentialsHandle)
            : this(credentialsHandle, null)
        { }

        public SpnegoTokenProvider(SafeFreeCredentials credentialsHandle, SecurityBindingElement securityBindingElement)
            : base(securityBindingElement)
        {
            this.credentialsHandle = credentialsHandle;
        }

        // settings
        public IdentityVerifier IdentityVerifier
        {
            get
            {
                return this.identityVerifier;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.identityVerifier = value;
            }
        }

        public TokenImpersonationLevel AllowedImpersonationLevel
        {
            get
            {
                return this.allowedImpersonationLevel;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();

                TokenImpersonationLevelHelper.Validate(value);
                if (value == TokenImpersonationLevel.None)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value",
                        String.Format(CultureInfo.InvariantCulture, SR.GetString(SR.SpnegoImpersonationLevelCannotBeSetToNone))));
                }
                this.allowedImpersonationLevel = value;
            }
        }

        public ICredentials ClientCredential
        {
            get
            {
                return this.clientCredential;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.clientCredential = value;
            }
        }

        public bool AllowNtlm
        {
            get
            {
                return this.allowNtlm;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.allowNtlm = value;
            }
        }

        public bool AuthenticateServer
        {
            get
            {
                return this.authenticateServer;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.authenticateServer = value;
            }
        }

        public bool InteractiveNegoExLogonEnabled
        {
            get
            {
                return this.interactiveNegoExLogonEnabled;
            }
            set
            {
                this.interactiveNegoExLogonEnabled = value;
            }
        }

        // overrides
        public override XmlDictionaryString NegotiationValueType
        {
            get
            {
                return XD.TrustApr2004Dictionary.SpnegoValueTypeUri;
            }
        }

        public override void OnOpening()
        {
            bool osIsGreaterThanXP = SecurityUtils.IsOsGreaterThanXP();

            base.OnOpening();
            if (this.credentialsHandle == null)
            {
                string packageName;
                if (!this.allowNtlm && !osIsGreaterThanXP)
                {
                    packageName = "Kerberos";
                }
                else
                {
                    packageName = "Negotiate";
                }

                NetworkCredential credential = null;
                if (this.clientCredential != null)
                {
                    credential = this.clientCredential.GetCredential(this.TargetAddress.Uri, packageName);
                }

                // if OS is less than 2k3 !NTLM is not supported, Windows SE 142400
                if (!this.allowNtlm && osIsGreaterThanXP)
                {
                    this.credentialsHandle = SecurityUtils.GetCredentialsHandle(packageName, credential, false, "!NTLM");
                }
                else
                {
                    this.credentialsHandle = SecurityUtils.GetCredentialsHandle(packageName, credential, false);
                }

                this.ownCredentialsHandle = true;
            }
        }

        public override void OnClose(TimeSpan timeout)
        {
            base.OnClose(timeout);
            FreeCredentialsHandle();
        }

        public override void OnAbort()
        {
            base.OnAbort();
            FreeCredentialsHandle();
        }

        void FreeCredentialsHandle()
        {
            if (this.credentialsHandle != null)
            {
                if (this.ownCredentialsHandle)
                {
                    this.credentialsHandle.Close();
                }
                this.credentialsHandle = null;
            }
        }

        protected override bool CreateNegotiationStateCompletesSynchronously(EndpointAddress target, Uri via)
        {
            return true;
        }

        protected override IAsyncResult BeginCreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout, AsyncCallback callback, object state)
        {
            SspiNegotiationTokenProviderState sspiState = this.CreateNegotiationState(target, via, timeout);
            return new CompletedAsyncResult<SspiNegotiationTokenProviderState>(sspiState, callback, state);
        }

        protected override SspiNegotiationTokenProviderState EndCreateNegotiationState(IAsyncResult result)
        {
            return CompletedAsyncResult<SspiNegotiationTokenProviderState>.End(result);
        }

        protected override SspiNegotiationTokenProviderState CreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout)
        {
            EnsureEndpointAddressDoesNotRequireEncryption(target);

            EndpointIdentity identity = null;
            if (this.identityVerifier == null)
            {
                identity = target.Identity;
            }
            else
            {
                this.identityVerifier.TryGetIdentity(target, out identity);
            }

            string spn;
            if (this.AuthenticateServer || !this.AllowNtlm)
            {
                spn = SecurityUtils.GetSpnFromIdentity(identity, target);
            }
            else
            {
                // if an SPN or UPN identity is configured (for example, in mixed mode SSPI), then 
                // use that identity for Negotiate
                Claim identityClaim = identity.IdentityClaim;
                if (identityClaim != null && (identityClaim.ClaimType == ClaimTypes.Spn || identityClaim.ClaimType == ClaimTypes.Upn))
                {
                    spn = identityClaim.Resource.ToString();
                }
                else
                {
                    spn = "host/" + target.Uri.DnsSafeHost;
                }
            }

            string packageName;
            if (!this.allowNtlm && !SecurityUtils.IsOsGreaterThanXP())
            {
                packageName = "Kerberos";
            }
            else
            {
                packageName = "Negotiate";
            }

            WindowsSspiNegotiation sspiNegotiation = new WindowsSspiNegotiation(packageName, this.credentialsHandle,
                this.AllowedImpersonationLevel, spn, true, this.InteractiveNegoExLogonEnabled, this.allowNtlm);
            return new SspiNegotiationTokenProviderState(sspiNegotiation);
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateSspiNegotiation(ISspiNegotiation sspiNegotiation)
        {
            WindowsSspiNegotiation windowsNegotiation = (WindowsSspiNegotiation)sspiNegotiation;
            if (windowsNegotiation.IsValidContext == false)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.InvalidSspiNegotiation)));
            }
            if (this.AuthenticateServer && windowsNegotiation.IsMutualAuthFlag == false)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.CannotAuthenticateServer)));
            }
            SecurityTraceRecordHelper.TraceClientSpnego(windowsNegotiation);

            return SecurityUtils.CreatePrincipalNameAuthorizationPolicies(windowsNegotiation.ServicePrincipalName);
        }
    }
}
