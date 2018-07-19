//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.IdentityModel.Claims;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.IdentityModel.Policy;
    using System.Security.Principal;
    using System.Security.Cryptography.X509Certificates;
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;
    using System.Net;
    using System.Diagnostics;

    class IssuanceTokenProviderState : IDisposable
    {
        bool isNegotiationCompleted = false;
        GenericXmlSecurityToken serviceToken;
        string context;
        EndpointAddress targetAddress;
        EndpointAddress remoteAddress;

        public IssuanceTokenProviderState() { }

        public bool IsNegotiationCompleted
        {
            get
            {
                return this.isNegotiationCompleted;
            }
        }

        public GenericXmlSecurityToken ServiceToken
        {
            get
            {
                CheckCompleted();
                return this.serviceToken;
            }
        }

        public EndpointAddress TargetAddress
        {
            get
            {
                return this.targetAddress;
            }
            set
            {
                this.targetAddress = value;
            }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return this.remoteAddress;
            }
            set
            {
                this.remoteAddress = value;
            }
        }

        public string Context
        {
            get
            {
                return this.context;
            }
            set
            {
                this.context = value;
            }
        }

        public virtual void Dispose() { }

        public void SetServiceToken(GenericXmlSecurityToken serviceToken)
        {
            if (this.IsNegotiationCompleted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NegotiationIsCompleted)));
            }
            this.serviceToken = serviceToken;
            this.isNegotiationCompleted = true;
        }

        void CheckCompleted()
        {
            if (!this.IsNegotiationCompleted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NegotiationIsNotCompleted)));
            }
        }
    }
}
