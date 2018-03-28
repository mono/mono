//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    public sealed class IssuedTokenClientCredential
    {
        SecurityKeyEntropyMode defaultKeyEntropyMode = AcceleratedTokenProvider.defaultKeyEntropyMode;
        KeyedByTypeCollection<IEndpointBehavior> localIssuerChannelBehaviors;
        Dictionary<Uri, KeyedByTypeCollection<IEndpointBehavior>> issuerChannelBehaviors;
        bool cacheIssuedTokens = SpnegoTokenProvider.defaultClientCacheTokens;
        TimeSpan maxIssuedTokenCachingTime = SpnegoTokenProvider.DefaultClientMaxTokenCachingTime;
        EndpointAddress localIssuerAddress;
        Binding localIssuerBinding;
        int issuedTokenRenewalThresholdPercentage = AcceleratedTokenProvider.defaultServiceTokenValidityThresholdPercentage;
        bool isReadOnly;

        internal IssuedTokenClientCredential()
        {
        }

        internal IssuedTokenClientCredential(IssuedTokenClientCredential other)
        {
            this.defaultKeyEntropyMode = other.defaultKeyEntropyMode;
            this.cacheIssuedTokens = other.cacheIssuedTokens;
            this.issuedTokenRenewalThresholdPercentage = other.issuedTokenRenewalThresholdPercentage;
            this.maxIssuedTokenCachingTime = other.maxIssuedTokenCachingTime;
            this.localIssuerAddress = other.localIssuerAddress;
            this.localIssuerBinding = (other.localIssuerBinding != null) ? new CustomBinding(other.localIssuerBinding) : null;
            if (other.localIssuerChannelBehaviors != null)
                this.localIssuerChannelBehaviors = GetBehaviorCollection(other.localIssuerChannelBehaviors);
            if (other.issuerChannelBehaviors != null)
            {
                this.issuerChannelBehaviors = new Dictionary<Uri, KeyedByTypeCollection<IEndpointBehavior>>();
                foreach (Uri uri in other.issuerChannelBehaviors.Keys)
                {
                    this.issuerChannelBehaviors.Add(uri, GetBehaviorCollection(other.issuerChannelBehaviors[uri]));
                }
            }
            this.isReadOnly = other.isReadOnly;
        }

        public EndpointAddress LocalIssuerAddress
        {
            get
            {
                return this.localIssuerAddress;
            }
            set
            {
                ThrowIfImmutable();
                this.localIssuerAddress = value;
            }
        }

        public Binding LocalIssuerBinding 
        {
            get
            {
                return this.localIssuerBinding;
            }
            set
            {
                ThrowIfImmutable();
                this.localIssuerBinding = value;
            }
        }

        public SecurityKeyEntropyMode DefaultKeyEntropyMode
        {
            get
            {
                return this.defaultKeyEntropyMode;
            }
            set
            {
                SecurityKeyEntropyModeHelper.Validate(value);
                ThrowIfImmutable();
                this.defaultKeyEntropyMode = value;
            }
        }

        public bool CacheIssuedTokens
        {
            get
            {
                return this.cacheIssuedTokens;
            }
            set
            {
                ThrowIfImmutable();
                this.cacheIssuedTokens = value;
            }
        }

        public int IssuedTokenRenewalThresholdPercentage 
        {
            get
            {
                return this.issuedTokenRenewalThresholdPercentage;
            }
            set
            {
                ThrowIfImmutable();
                this.issuedTokenRenewalThresholdPercentage = value;
            }
        }

        public Dictionary<Uri, KeyedByTypeCollection<IEndpointBehavior>> IssuerChannelBehaviors
        {
            get
            {
                if (this.issuerChannelBehaviors == null)
                    this.issuerChannelBehaviors = new Dictionary<Uri, KeyedByTypeCollection<IEndpointBehavior>>();
                return this.issuerChannelBehaviors;
            }
        }

        public KeyedByTypeCollection<IEndpointBehavior> LocalIssuerChannelBehaviors
        {
            get
            {
                if (this.localIssuerChannelBehaviors == null)
                    this.localIssuerChannelBehaviors = new KeyedByTypeCollection<IEndpointBehavior>();
                return this.localIssuerChannelBehaviors;
            }
        }

        public TimeSpan MaxIssuedTokenCachingTime
        {
            get
            {
                return this.maxIssuedTokenCachingTime;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                ThrowIfImmutable();
                this.maxIssuedTokenCachingTime = value;
            }
        }

        KeyedByTypeCollection<IEndpointBehavior> GetBehaviorCollection(KeyedByTypeCollection<IEndpointBehavior> behaviors)
        {
            KeyedByTypeCollection<IEndpointBehavior> result = new KeyedByTypeCollection<IEndpointBehavior>();
            foreach (IEndpointBehavior behavior in behaviors)
            {
                result.Add(behavior);
            }
            return result;
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            }
        }
    }
}
