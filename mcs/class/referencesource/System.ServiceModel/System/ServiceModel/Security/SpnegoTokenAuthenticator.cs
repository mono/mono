
//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Net;
    using System.Runtime;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    using SafeCloseHandle = System.IdentityModel.SafeCloseHandle;
    using SafeFreeCredentials = System.IdentityModel.SafeFreeCredentials;

    sealed class SpnegoTokenAuthenticator : SspiNegotiationTokenAuthenticator
    {
        bool extractGroupsForWindowsAccounts;
        NetworkCredential serverCredential;
        bool allowUnauthenticatedCallers;
        SafeFreeCredentials credentialsHandle;

        public SpnegoTokenAuthenticator()
            : base()
        {
            // empty
        }

        // settings        
        public bool ExtractGroupsForWindowsAccounts
        {
            get
            {
                return this.extractGroupsForWindowsAccounts;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.extractGroupsForWindowsAccounts = value;
            }
        }
        
        public NetworkCredential ServerCredential
        {
            get
            {
                return this.serverCredential;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.serverCredential = value;
            }
        }

        public bool AllowUnauthenticatedCallers
        {
            get
            {
                return this.allowUnauthenticatedCallers;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.allowUnauthenticatedCallers = value;
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
            base.OnOpening();
            if (this.credentialsHandle == null)
            {
                this.credentialsHandle = SecurityUtils.GetCredentialsHandle("Negotiate", this.serverCredential, true);
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
                this.credentialsHandle.Close();
                this.credentialsHandle = null;
            }
        }

        protected override SspiNegotiationTokenAuthenticatorState CreateSspiState(byte[] incomingBlob, string incomingValueTypeUri)
        {
            ISspiNegotiation windowsNegotiation = new WindowsSspiNegotiation("Negotiate", this.credentialsHandle, DefaultServiceBinding);
            return new SspiNegotiationTokenAuthenticatorState(windowsNegotiation);
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateSspiNegotiation(ISspiNegotiation sspiNegotiation)
        {
            WindowsSspiNegotiation windowsNegotiation = (WindowsSspiNegotiation)sspiNegotiation;
            if (windowsNegotiation.IsValidContext == false)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(SR.GetString(SR.InvalidSspiNegotiation)));
            }
            SecurityTraceRecordHelper.TraceServiceSpnego(windowsNegotiation);
            if (this.IsClientAnonymous)
            {
                return EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }
            using (SafeCloseHandle contextToken = windowsNegotiation.GetContextToken())
            {
                WindowsIdentity windowsIdentity = new WindowsIdentity(contextToken.DangerousGetHandle(), windowsNegotiation.ProtocolName);
                SecurityUtils.ValidateAnonymityConstraint(windowsIdentity, this.AllowUnauthenticatedCallers);

                List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
                WindowsClaimSet wic = new WindowsClaimSet( windowsIdentity, windowsNegotiation.ProtocolName, this.extractGroupsForWindowsAccounts, false );
                policies.Add(new System.IdentityModel.Policy.UnconditionalPolicy(wic, TimeoutHelper.Add(DateTime.UtcNow, base.ServiceTokenLifetime)));
                return policies.AsReadOnly();
            }
        }
    }
}
