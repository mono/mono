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
    using System.Runtime.Serialization;
    using System.Net;
    using System.Diagnostics;

    class SspiNegotiationTokenAuthenticatorState : NegotiationTokenAuthenticatorState
    {
        ISspiNegotiation sspiNegotiation;
        HashAlgorithm negotiationDigest;
        string context;
        int requestedKeySize;
        EndpointAddress appliesTo;
        DataContractSerializer appliesToSerializer;

        public SspiNegotiationTokenAuthenticatorState(ISspiNegotiation sspiNegotiation)
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

        internal int RequestedKeySize
        {
            get
            {
                return this.requestedKeySize;
            }
            set
            {
                this.requestedKeySize = value;
            }
        }

        internal HashAlgorithm NegotiationDigest
        {
            get
            {
                return this.negotiationDigest;
            }
        }

        internal string Context
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

        internal EndpointAddress AppliesTo
        {
            get
            {
                return this.appliesTo;
            }
            set
            {
                this.appliesTo = value;
            }
        }

        internal DataContractSerializer AppliesToSerializer
        {
            get
            {
                return this.appliesToSerializer;
            }
            set
            {
                this.appliesToSerializer = value;
            }
        }

        public override string GetRemoteIdentityName()
        {
            if (this.sspiNegotiation != null && !this.IsNegotiationCompleted)
            {
                return this.sspiNegotiation.GetRemoteIdentityName();
            }
            return base.GetRemoteIdentityName();
        }

        public override void Dispose()
        {
            try
            {
                lock (ThisLock)
                {
                    if (this.sspiNegotiation != null)
                    {
                        this.sspiNegotiation.Dispose();

                    }
                    if (this.negotiationDigest != null)
                    {
                        ((IDisposable)this.negotiationDigest).Dispose();
                    }
                }
            }
            finally
            {
                base.Dispose();
            }
        }
    }
}
