//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security.Tokens;

    class AsymmetricSecurityProtocolFactory : MessageSecurityProtocolFactory
    {
        SecurityTokenParameters cryptoTokenParameters;
        SecurityTokenParameters asymmetricTokenParameters;

        SecurityTokenProvider recipientAsymmetricTokenProvider;
        ReadOnlyCollection<SecurityTokenResolver> recipientOutOfBandTokenResolverList;
        SecurityTokenAuthenticator recipientCryptoTokenAuthenticator;        

        bool allowSerializedSigningTokenOnReply;
        
        public AsymmetricSecurityProtocolFactory()
            : base()
        {
        }

        internal AsymmetricSecurityProtocolFactory(AsymmetricSecurityProtocolFactory factory)
            : base(factory)
        {
            this.allowSerializedSigningTokenOnReply = factory.allowSerializedSigningTokenOnReply;
        }

        public bool AllowSerializedSigningTokenOnReply
        {
            get
            {
                return this.allowSerializedSigningTokenOnReply;
            }
            set
            {
                ThrowIfImmutable();
                this.allowSerializedSigningTokenOnReply = value;
            }
        }

        public SecurityTokenParameters AsymmetricTokenParameters
        {
            get
            {
                return this.asymmetricTokenParameters;
            }
            set
            {
                ThrowIfImmutable();
                this.asymmetricTokenParameters = value;
            }
        }

        public SecurityTokenProvider RecipientAsymmetricTokenProvider
        {
            get
            {
                this.CommunicationObject.ThrowIfNotOpened();
                return this.recipientAsymmetricTokenProvider;
            }
        }
        
        public SecurityTokenAuthenticator RecipientCryptoTokenAuthenticator
        {
            get
            {
                this.CommunicationObject.ThrowIfNotOpened();
                return this.recipientCryptoTokenAuthenticator;
            }
        }

        public ReadOnlyCollection<SecurityTokenResolver> RecipientOutOfBandTokenResolverList
        {
            get
            {
                this.CommunicationObject.ThrowIfNotOpened();
                return this.recipientOutOfBandTokenResolverList;
            }
        }


        public SecurityTokenParameters CryptoTokenParameters
        {
            get
            {
                return this.cryptoTokenParameters;
            }
            set
            {
                ThrowIfImmutable();
                this.cryptoTokenParameters = value;
            }
        }

        bool RequiresAsymmetricTokenProviderForForwardDirection
        {
            get
            {
                return ((this.ActAsInitiator && this.ApplyConfidentiality) || (!this.ActAsInitiator && this.RequireConfidentiality));
            }
        }

        bool RequiresAsymmetricTokenProviderForReturnDirection
        {
            get
            {
                return ((this.ActAsInitiator && this.RequireIntegrity) || (!this.ActAsInitiator && this.ApplyIntegrity));
            }
        }

        public override EndpointIdentity GetIdentityOfSelf()
        {
            if (this.SecurityTokenManager is IEndpointIdentityProvider && this.AsymmetricTokenParameters != null)
            {
                SecurityTokenRequirement requirement = CreateRecipientSecurityTokenRequirement();
                this.AsymmetricTokenParameters.InitializeSecurityTokenRequirement(requirement);
                return ((IEndpointIdentityProvider)this.SecurityTokenManager).GetIdentityOfSelf(requirement);
            }
            else
            {
                return base.GetIdentityOfSelf();
            }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(Collection<ISecurityContextSecurityTokenCache>))
            {
                Collection<ISecurityContextSecurityTokenCache> result = base.GetProperty<Collection<ISecurityContextSecurityTokenCache>>();
                if (this.recipientCryptoTokenAuthenticator is ISecurityContextSecurityTokenCacheProvider)
                {
                    result.Add(((ISecurityContextSecurityTokenCacheProvider)this.recipientCryptoTokenAuthenticator).TokenCache);
                }
                return (T) (object) (result);
            }
            else
            {
                return base.GetProperty<T>();
            }
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (!this.ActAsInitiator)
            {
                if (this.recipientAsymmetricTokenProvider != null)
                {
                    SecurityUtils.CloseTokenProviderIfRequired(this.recipientAsymmetricTokenProvider, timeoutHelper.RemainingTime());
                }
                if (this.recipientCryptoTokenAuthenticator != null)
                {
                    SecurityUtils.CloseTokenAuthenticatorIfRequired(this.recipientCryptoTokenAuthenticator, timeoutHelper.RemainingTime());
                }
            }
            base.OnClose(timeoutHelper.RemainingTime());
        }

        public override void OnAbort()
        {
            if (!this.ActAsInitiator)
            {
                if (this.recipientAsymmetricTokenProvider != null)
                {
                    SecurityUtils.AbortTokenProviderIfRequired(this.recipientAsymmetricTokenProvider);
                }
                if (this.recipientCryptoTokenAuthenticator != null)
                {
                    SecurityUtils.AbortTokenAuthenticatorIfRequired(this.recipientCryptoTokenAuthenticator);
                }
            }
            base.OnAbort();
        }

        protected override SecurityProtocol OnCreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, TimeSpan timeout)
        {
            return new AsymmetricSecurityProtocol(this, target, via);
        }

        public override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());

            // open forward direction
            if (this.ActAsInitiator)
            {
                if (this.ApplyIntegrity)
                {
                    if (this.CryptoTokenParameters == null)
                    {
                        OnPropertySettingsError("CryptoTokenParameters", true);
                    }
                    if (this.CryptoTokenParameters.RequireDerivedKeys)
                    {
                        this.ExpectKeyDerivation = true;
                    }
                }
            }
            else
            {
                if (this.CryptoTokenParameters == null)
                {
                    OnPropertySettingsError("CryptoTokenParameters", true);
                }
                if (this.CryptoTokenParameters.RequireDerivedKeys)
                {
                    this.ExpectKeyDerivation = true;
                }
                SecurityTokenResolver resolver = null;
                if (this.RequireIntegrity)
                {
                    RecipientServiceModelSecurityTokenRequirement requirement = CreateRecipientSecurityTokenRequirement();
                    this.CryptoTokenParameters.InitializeSecurityTokenRequirement(requirement);
                    requirement.KeyUsage = SecurityKeyUsage.Signature;
                    requirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Input;
                    this.recipientCryptoTokenAuthenticator = this.SecurityTokenManager.CreateSecurityTokenAuthenticator(requirement, out resolver);
                     Open("RecipientCryptoTokenAuthenticator", true, this.recipientCryptoTokenAuthenticator, timeoutHelper.RemainingTime());
                }
                if (resolver != null)
                {
                    Collection<SecurityTokenResolver> tmp = new Collection<SecurityTokenResolver>();
                    tmp.Add(resolver);
                    this.recipientOutOfBandTokenResolverList = new ReadOnlyCollection<SecurityTokenResolver>(tmp);
                }
                else
                {
                    this.recipientOutOfBandTokenResolverList = EmptyReadOnlyCollection<SecurityTokenResolver>.Instance;
                }
            }

            if (this.RequiresAsymmetricTokenProviderForForwardDirection || this.RequiresAsymmetricTokenProviderForReturnDirection)
            {
                if (this.AsymmetricTokenParameters == null)
                {
                    OnPropertySettingsError("AsymmetricTokenParameters", this.RequiresAsymmetricTokenProviderForForwardDirection);
                }
                else if (this.AsymmetricTokenParameters.RequireDerivedKeys)
                {
                    this.ExpectKeyDerivation = true;
                }
                if (!this.ActAsInitiator)
                {
                    RecipientServiceModelSecurityTokenRequirement requirement = CreateRecipientSecurityTokenRequirement();
                    this.AsymmetricTokenParameters.InitializeSecurityTokenRequirement(requirement);
                    requirement.KeyUsage = (this.RequiresAsymmetricTokenProviderForForwardDirection) ? SecurityKeyUsage.Exchange : SecurityKeyUsage.Signature;
                    requirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = (this.RequiresAsymmetricTokenProviderForForwardDirection) ? MessageDirection.Input : MessageDirection.Output;
                    this.recipientAsymmetricTokenProvider = this.SecurityTokenManager.CreateSecurityTokenProvider(requirement);
                    Open("RecipientAsymmetricTokenProvider", this.RequiresAsymmetricTokenProviderForForwardDirection, this.recipientAsymmetricTokenProvider, timeoutHelper.RemainingTime());
                }
            }

            if (this.ActAsInitiator && this.AllowSerializedSigningTokenOnReply && this.IdentityVerifier == null)
            {
                OnPropertySettingsError("IdentityVerifier", false);
            }
        }
    }
}
