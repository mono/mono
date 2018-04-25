//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.IdentityModel.Claims;
    using System.ServiceModel;
    using System.IdentityModel.Policy;
    using System.Security.Principal;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.Net;
    using System.Diagnostics;

    class SspiNegotiationTokenProviderState : IssuanceTokenProviderState
    {
        ISspiNegotiation sspiNegotiation;
        HashAlgorithm negotiationDigest;

        public SspiNegotiationTokenProviderState(ISspiNegotiation sspiNegotiation)
            : base()
        {
            if (sspiNegotiation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sspiNegotiation");
            }
            this.sspiNegotiation = sspiNegotiation;
            this.negotiationDigest = CryptoHelper.NewSha1HashAlgorithm();
        }

        public ISspiNegotiation SspiNegotiation
        {
            get
            {
                return this.sspiNegotiation;
            }
        }

        internal HashAlgorithm NegotiationDigest
        {
            get
            {
                return this.negotiationDigest;
            }
        }

        public override void Dispose()
        {
            try
            {
                if (this.sspiNegotiation != null)
                {
                    this.sspiNegotiation.Dispose();
                    this.sspiNegotiation = null;

                    ((IDisposable)this.negotiationDigest).Dispose();
                    this.negotiationDigest = null;
                }
            }
            finally
            {
                base.Dispose();
            }
        }
    }
}
