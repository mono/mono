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
    using System.ServiceModel.Security.Tokens;

    class SymmetricSecurityProtocolFactory : MessageSecurityProtocolFactory
    {
        // server side per-listener objects
        SecurityTokenAuthenticator recipientSymmetricTokenAuthenticator;
        SecurityTokenProvider recipientAsymmetricTokenProvider;
        ReadOnlyCollection<SecurityTokenResolver> recipientOutOfBandTokenResolverList;
        SecurityTokenParameters tokenParameters;
        SecurityTokenParameters protectionTokenParameters;

        public SymmetricSecurityProtocolFactory()
            : base()
        {
        }

        internal SymmetricSecurityProtocolFactory(MessageSecurityProtocolFactory factory)
            : base(factory)
        {
        }

        public SecurityTokenParameters SecurityTokenParameters
        {
            get
            {
                return this.tokenParameters;
            }
            set
            {
                ThrowIfImmutable();
                this.tokenParameters = value;
            }
        }

        public SecurityTokenProvider RecipientAsymmetricTokenProvider
        {
            get
            {
                return this.recipientAsymmetricTokenProvider;
            }
        }

        public SecurityTokenAuthenticator RecipientSymmetricTokenAuthenticator
        {
            get
            {
                return this.recipientSymmetricTokenAuthenticator;
            }
        }

        public ReadOnlyCollection<SecurityTokenResolver> RecipientOutOfBandTokenResolverList
        {
            get
            {
                return this.recipientOutOfBandTokenResolverList;
            }
        }

        public override EndpointIdentity GetIdentityOfSelf()
        {
            EndpointIdentity identity = null;
            if (this.SecurityTokenManager is IEndpointIdentityProvider)
            {
                SecurityTokenRequirement requirement = CreateRecipientSecurityTokenRequirement();
                this.SecurityTokenParameters.InitializeSecurityTokenRequirement(requirement);
                identity = ((IEndpointIdentityProvider)this.SecurityTokenManager).GetIdentityOfSelf(requirement);
            }
            else
            {
                identity = base.GetIdentityOfSelf();
            }
            return identity;
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(Collection<ISecurityContextSecurityTokenCache>))
            {
                Collection<ISecurityContextSecurityTokenCache> result = base.GetProperty<Collection<ISecurityContextSecurityTokenCache>>();
                if (this.recipientSymmetricTokenAuthenticator is ISecurityContextSecurityTokenCacheProvider)
                {
                    result.Add(((ISecurityContextSecurityTokenCacheProvider)this.recipientSymmetricTokenAuthenticator).TokenCache);
                }
                return (T)(object)(result);
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
                if (this.recipientSymmetricTokenAuthenticator != null)
                {
                    SecurityUtils.CloseTokenAuthenticatorIfRequired(this.recipientSymmetricTokenAuthenticator, timeoutHelper.RemainingTime());
                }
                if (this.recipientAsymmetricTokenProvider != null)
                {
                    SecurityUtils.CloseTokenProviderIfRequired(this.recipientAsymmetricTokenProvider, timeoutHelper.RemainingTime());
                }
            }
            base.OnClose(timeoutHelper.RemainingTime());
        }

        public override void OnAbort()
        {
            if (!this.ActAsInitiator)
            {
                if (this.recipientSymmetricTokenAuthenticator != null)
                {
                    SecurityUtils.AbortTokenAuthenticatorIfRequired(this.recipientSymmetricTokenAuthenticator);
                }
                if (this.recipientAsymmetricTokenProvider != null)
                {
                    SecurityUtils.AbortTokenProviderIfRequired(this.recipientAsymmetricTokenProvider);
                }
            }
            base.OnAbort();
        }

        protected override SecurityProtocol OnCreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, TimeSpan timeout)
        {
            return new SymmetricSecurityProtocol(this, target, via);
        }

        RecipientServiceModelSecurityTokenRequirement CreateRecipientTokenRequirement()
        {
            RecipientServiceModelSecurityTokenRequirement requirement = CreateRecipientSecurityTokenRequirement();
            this.SecurityTokenParameters.InitializeSecurityTokenRequirement(requirement);
            requirement.KeyUsage = (this.SecurityTokenParameters.HasAsymmetricKey) ? SecurityKeyUsage.Exchange : SecurityKeyUsage.Signature;
            return requirement;
        }

        public override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());

            if (this.tokenParameters == null)
            {
                OnPropertySettingsError("SecurityTokenParameters", true);
            }

            if (!this.ActAsInitiator)
            {
                SecurityTokenRequirement recipientTokenRequirement = CreateRecipientTokenRequirement();
                SecurityTokenResolver resolver = null;
                if (this.SecurityTokenParameters.HasAsymmetricKey)
                {
                    this.recipientAsymmetricTokenProvider = this.SecurityTokenManager.CreateSecurityTokenProvider(recipientTokenRequirement);
                }
                else
                {
                    this.recipientSymmetricTokenAuthenticator = this.SecurityTokenManager.CreateSecurityTokenAuthenticator(recipientTokenRequirement, out resolver);
                }
                if (this.RecipientSymmetricTokenAuthenticator != null && this.RecipientAsymmetricTokenProvider != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.OnlyOneOfEncryptedKeyOrSymmetricBindingCanBeSelected)));
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

                if (this.RecipientAsymmetricTokenProvider != null)
                {
                    Open("RecipientAsymmetricTokenProvider", true, this.RecipientAsymmetricTokenProvider, timeoutHelper.RemainingTime());
                }
                else
                {
                    Open("RecipientSymmetricTokenAuthenticator", true, this.RecipientSymmetricTokenAuthenticator, timeoutHelper.RemainingTime());
                }
            }
            if (this.tokenParameters.RequireDerivedKeys)
            {
                this.ExpectKeyDerivation = true;
            }
            if (this.tokenParameters.HasAsymmetricKey)
            {
                this.protectionTokenParameters = new WrappedKeySecurityTokenParameters();
                this.protectionTokenParameters.RequireDerivedKeys = this.SecurityTokenParameters.RequireDerivedKeys;
            }
            else
            {
                this.protectionTokenParameters = this.tokenParameters;
            }
        }

        internal SecurityTokenParameters GetProtectionTokenParameters()
        {
            return this.protectionTokenParameters;
        }
    }
}
