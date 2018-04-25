//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
//
// Presharp uses the c# pragma mechanism to supress its warnings.
// These are not recognised by the base compiler so we need to explictly
// disable the following warnings. See http://winweb/cse/Tools/PREsharp/userguide/default.asp 
// for details. 
//
#pragma warning disable 1634, 1691      // unknown message, unknown pragma

namespace System.ServiceModel.Security
{
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security.Tokens;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Xml;
    using System.IO;
    using System.Text;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;


    //
    // Summary:
    //  delegate definition for invokation of the GetToken method.
    //
    delegate SecurityToken GetInfoCardTokenCallback(bool requiresInfoCard, CardSpacePolicyElement[] chain, SecurityTokenSerializer tokenSerializer);

    static class InfoCardHelper
    {
        const string WSIdentityNamespace = @"http://schemas.microsoft.com/ws/2005/05/identity";
        const string IsManagedElementName = @"IsManaged";
        static Uri selfIssuerUri;

        // Summary:
        //  If interactive support is requested and an IssuedSecurityTokenParameters is specified this method 
        //  will return an instance of an InfoCardTokenProvider. 
        //  Otherwise this method defers to the base implementation.
        //
        // Parameters
        //  parameters  - The security token parameters associated with this ChannelFactory.
        //
        // Note
        //  The target and issuer information will not be available in this call
        //
        public static bool TryCreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement, ClientCredentialsSecurityTokenManager clientCredentialsTokenManager, out SecurityTokenProvider provider)
        {
            if (tokenRequirement == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
            if (clientCredentialsTokenManager == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("clientCredentialsTokenManager");

            provider = null;

            if (!clientCredentialsTokenManager.ClientCredentials.SupportInteractive
                || (null != clientCredentialsTokenManager.ClientCredentials.IssuedToken.LocalIssuerAddress && null != clientCredentialsTokenManager.ClientCredentials.IssuedToken.LocalIssuerBinding)
                || !clientCredentialsTokenManager.IsIssuedSecurityTokenRequirement(tokenRequirement)
                )
            {
                //IDT.TraceDebug("ICARDTOKPROV: Non Issued SecurityToken requirement submitted to InfoCardClientCredentialsSecurityTokenManager:\n{0}", tokenRequirement);
                //IDT.TraceDebug("ICARDTOKPROV: Defering to the base class to create the token provider");
            }
            else
            {
                ChannelParameterCollection channelParameter;
                InfoCardChannelParameter infocardChannelParameter = null;
                if (tokenRequirement.TryGetProperty<ChannelParameterCollection>(ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty, out channelParameter))
                {
                    foreach (object obj in channelParameter)
                    {
                        if (obj is InfoCardChannelParameter)
                        {
                            infocardChannelParameter = (InfoCardChannelParameter)obj;
                            break;
                        }
                    }
                }

                if (null == infocardChannelParameter || !infocardChannelParameter.RequiresInfoCard)
                {
                    return false;
                }

                EndpointAddress target = tokenRequirement.GetProperty<EndpointAddress>(ServiceModelSecurityTokenRequirement.TargetAddressProperty);
                IssuedSecurityTokenParameters issuedTokenParameters = tokenRequirement.GetProperty<IssuedSecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty);

                Uri privacyNoticeLink;
                if (!tokenRequirement.TryGetProperty<Uri>(ServiceModelSecurityTokenRequirement.PrivacyNoticeUriProperty, out privacyNoticeLink))
                {
                    privacyNoticeLink = null;
                }

                int privacyNoticeVersion;
                if (!tokenRequirement.TryGetProperty<int>(ServiceModelSecurityTokenRequirement.PrivacyNoticeVersionProperty, out privacyNoticeVersion))
                {
                    privacyNoticeVersion = 0;
                }
                //
                // This analysis of this chain indicates that interactive support will be required
                // The InternalClientCredentials class handles that.
                //
                provider = CreateTokenProviderForNextLeg(tokenRequirement, target, issuedTokenParameters.IssuerAddress, infocardChannelParameter.RelyingPartyIssuer, clientCredentialsTokenManager, infocardChannelParameter);
            }

            return provider != null;
        }


        public static bool IsInfocardRequired(Binding binding, ClientCredentials clientCreds, SecurityTokenManager clientCredentialsTokenManager, EndpointAddress target, out CardSpacePolicyElement[] infocardChain, out Uri relyingPartyIssuer)
        {
            infocardChain = null;
            bool requiresInfoCard = false;
            relyingPartyIssuer = null;
            if (!clientCreds.SupportInteractive
                || (null != clientCreds.IssuedToken.LocalIssuerAddress && null != clientCreds.IssuedToken.LocalIssuerBinding)
                )
            {
                return false;
            }
            IssuedSecurityTokenParameters parameters = TryGetNextStsIssuedTokenParameters(binding);
            if (null != parameters)
            {

                Uri privacyNotice;
                int privacyVersion;
                GetPrivacyNoticeLinkFromIssuerBinding(binding, out privacyNotice, out privacyVersion);

                PolicyElement[] policyChain = GetPolicyChain(target, binding, parameters, privacyNotice, privacyVersion, clientCredentialsTokenManager);
                relyingPartyIssuer = null;
                if (null != policyChain)
                {
                    requiresInfoCard = RequiresInfoCard(policyChain, out relyingPartyIssuer);
                }
                if (requiresInfoCard)
                {
                    infocardChain = new CardSpacePolicyElement[policyChain.Length];
                    for (int i = 0; i < policyChain.Length; i++)
                    {
                        infocardChain[i] = policyChain[i].ToCardSpacePolicyElement();
                    }

                }
            }
            return requiresInfoCard;
        }

        static Uri SelfIssuerUri
        {
            get
            {
                if (selfIssuerUri == null)

                    selfIssuerUri = new Uri(WSIdentityNamespace + "/issuer/self");
                return selfIssuerUri;
            }
        }

        static PolicyElement[] GetPolicyChain(EndpointAddress target, Binding outerBinding, IssuedSecurityTokenParameters parameters, Uri firstPrivacyNoticeLink, int firstPrivacyNoticeVersion, SecurityTokenManager clientCredentialsTokenManager)
        {
            EndpointAddress nextTarget = target;
            IssuedSecurityTokenParameters nextParameters = parameters;

            List<PolicyElement> chain = new List<PolicyElement>();

            Uri privacyNoticeLink = firstPrivacyNoticeLink;
            int privacyNoticeVersion = firstPrivacyNoticeVersion;
            bool isManagedIssuer = false;


            //
            // this is the binding to the final STS in the chain. Start from here and walk the 
            // chain backwards to the 1st STS in the chain
            //
            while (null != nextParameters)
            {
                MessageSecurityVersion bindingSecurityVersion = null;
                if (nextParameters.IssuerBinding == null)
                {
                    bindingSecurityVersion = GetBindingSecurityVersionOrDefault(outerBinding);
                }
                else
                {
                    bindingSecurityVersion = GetBindingSecurityVersionOrDefault(nextParameters.IssuerBinding);
                }
                chain.Add(new PolicyElement(nextTarget,
                 nextParameters.IssuerAddress,
                 nextParameters.CreateRequestParameters(bindingSecurityVersion, clientCredentialsTokenManager.CreateSecurityTokenSerializer(bindingSecurityVersion.SecurityTokenVersion)),
                 privacyNoticeLink,
                 privacyNoticeVersion,
                 isManagedIssuer,
                 nextParameters.IssuerBinding));

                isManagedIssuer = IsReferralToManagedIssuer(nextParameters.IssuerBinding);
                GetPrivacyNoticeLinkFromIssuerBinding(nextParameters.IssuerBinding, out privacyNoticeLink, out privacyNoticeVersion);
                nextTarget = nextParameters.IssuerAddress;
                outerBinding = nextParameters.IssuerBinding;
                nextParameters = TryGetNextStsIssuedTokenParameters(nextParameters.IssuerBinding);
            }

            //
            // Add the last element for the MCIP case
            //
            if (isManagedIssuer)
            {
                chain.Add(new PolicyElement(nextTarget,
                            null,
                            null,
                            privacyNoticeLink,
                            privacyNoticeVersion,
                            isManagedIssuer,
                            null));
            }
            return chain.ToArray();
        }

        //
        // Summary:
        //  Checks the policy chain to determine which target and issuer in the chain the InfoCard system should be invoked with.
        //
        //  Cases:
        //         
        //    i     Frame(n-1)          Frame(n)                            RP Index
        //    -     ----------          --------                             ---------
        //    1     empty               Self/Anon/null                       n 
        //    2     empty               mcip                                 error
        //    3     empty               anything other than self/anon        federated token provider
        //                              null/mcip
        //    4     any issuer          mcip                                 n-1
        //    5     any issuer          Self/Anon/null                       n   
        //    6     any issuer          anything other than self/anon        federated token provider
        //                              null/mcip
        //   
        //      
        // Parameters
        //  relyingPartyIssuer  - The output Uri of the issuer of the relying party requiring 
        //                        interactive support.
        //    
        // Returns:
        //  true    - A policy frame was identified to require interactive support from the infocard system.
        //  false   - Interactive support not required
        //
        static bool RequiresInfoCard(PolicyElement[] chain, out Uri relyingPartyIssuer)
        {
            relyingPartyIssuer = null;

            //IDT.TraceDebug("ICARDTOKPROV: Attempting to identify the relying party requiring infocard support.");

            if (0 == chain.Length)
            {
                //IDT.TraceDebug("ICARDTOKPROV: Zero length policy chain, returning false.");
                return false;
            }

            int n = chain.Length - 1;
            int relyingPartyIndex = -1;
            bool infocardSupportRequired = false;

            //
            // Handle a chain which contains only one element - cases 1 and 2
            //
            if (1 == chain.Length)
            {
                if (null == chain[n].Issuer
                    || chain[n].Issuer.IsAnonymous
                    || SelfIssuerUri.Equals(chain[n].Issuer.Uri)
                    || (null != chain[n].Issuer && null == chain[n].Binding))
                {
                    //IDT.TraceDebug("ICARDTOKPROV: Policy frame n was selected (case 1).");

                    //
                    // Case 1: Return frame n 
                    //
                    relyingPartyIndex = n;
                    infocardSupportRequired = true;
                }
                //
                // Case 2: Asert if mcip is at the bottom of the chain and is the only element
                //
                else if (chain[n].IsManagedIssuer)
                {
                    Fx.Assert("MCIP was found at the bottom of the chain when the chain length was 1");
                }
                else
                {
                    //
                    // Case 3: Do nothing so that Indigo will use the default federated token provider.
                    //
                    infocardSupportRequired = false;
                }
            }
            else
            {
                if (chain[n].IsManagedIssuer)
                {
                    //
                    // Case 4: MCIP followed by a token supported by infocard was found. Return frame n-1.
                    //
                    relyingPartyIndex = n - 1;
                    infocardSupportRequired = true;

                }
                //
                // Case 5: Federated chain ending in self issued/null/anon. Return frame n.
                //
                else if (null == chain[n].Issuer ||
                    chain[n].Issuer.IsAnonymous ||
                    SelfIssuerUri.Equals(chain[n].Issuer.Uri) ||
                    (null != chain[n].Issuer && null == chain[n].Binding))
                {
                    relyingPartyIndex = n;
                    infocardSupportRequired = true;
                }
                else
                {
                    //
                    // Case 6: Do nothing so that Indigo will use the default federated token provider.
                    //
                    infocardSupportRequired = false;
                }

                //
                // Check to make sure that no other leg in the chain specifically requires infocard
                //
                for (int k = 0; k < n; k++)
                {
                    if (chain[k].IsManagedIssuer
                        || SelfIssuerUri.Equals(chain[k].Issuer.Uri)
                        || null == chain[k].Issuer
                        || chain[k].Issuer.IsAnonymous)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InfoCardInvalidChain)));
                    }
                }
            }

            //
            // Get the issuer Uri from the policy chain. Also, handle the case where
            // the issuer may not have been specified.
            //
            if (infocardSupportRequired)
            {
                relyingPartyIssuer = (null == chain[relyingPartyIndex].Issuer) ? null : chain[relyingPartyIndex].Issuer.Uri;
            }

            return infocardSupportRequired;
        }

        static SecurityTokenProvider CreateTokenProviderForNextLeg(SecurityTokenRequirement tokenRequirement, EndpointAddress target, EndpointAddress issuerAddress, Uri relyingPartyIssuer, ClientCredentialsSecurityTokenManager clientCredentialsTokenManager, InfoCardChannelParameter infocardChannelParameter)
        {
            if (((null == relyingPartyIssuer && null == issuerAddress) || issuerAddress.Uri == relyingPartyIssuer))
            {
                return new InternalInfoCardTokenProvider(infocardChannelParameter);
            }
            else
            {
                // create a federation token provider and add an internal client credentials shim that contains the chain
                IssuedSecurityTokenProvider federationTokenProvider = (IssuedSecurityTokenProvider)clientCredentialsTokenManager.CreateSecurityTokenProvider(tokenRequirement, true);
                federationTokenProvider.IssuerChannelBehaviors.Remove<SecurityCredentialsManager>();
                federationTokenProvider.IssuerChannelBehaviors.Add(new InternalClientCredentials(clientCredentialsTokenManager.ClientCredentials, target, relyingPartyIssuer, infocardChannelParameter));
                return federationTokenProvider;

            }
        }

        //
        // Summary:
        //  Looks for a standards manager on the security binding element of the specified binding.
        //  If one is not found it returns the default.
        //
        // Parameters
        //  binding     - Collection
        //  parameters  - Security parameters for current invocation.
        //    
        // Returns:
        //  Always returns an instance of the SecurityVersion class
        //
        public static MessageSecurityVersion GetBindingSecurityVersionOrDefault(Binding binding)
        {
            if (null != binding)
            {
                SecurityBindingElement sbe = binding.CreateBindingElements().Find<SecurityBindingElement>();
                if (null != sbe)
                {
                    return sbe.MessageSecurityVersion;
                }
            }

            return MessageSecurityVersion.Default;
        }

        //
        // Summary:
        // Given an Issuer binding determines whether that issuer is a managed card provider.
        //
        //
        static bool IsReferralToManagedIssuer(Binding issuerBinding)
        {
            bool bRetVal = false;

            if (null != issuerBinding)
            {
                //
                // If the UseManagedPresentationBindingElement is present then this is a ManagedCardProvider.
                //
                UseManagedPresentationBindingElement useManagedPresentationBE =
                    issuerBinding.CreateBindingElements().Find<UseManagedPresentationBindingElement>();

                if (null != useManagedPresentationBE)
                {
                    bRetVal = true;
                }
            }

            return bRetVal;
        }

        //
        // Summary:
        // Given an issuer Binding retrieves any privacy notice links that might be present.
        //
        static void GetPrivacyNoticeLinkFromIssuerBinding(Binding issuerBinding, out Uri privacyNotice, out int privacyNoticeVersion)
        {
            privacyNotice = null;
            privacyNoticeVersion = 0;

            if (null != issuerBinding)
            {
                PrivacyNoticeBindingElement privacyNoticeBE =
                    issuerBinding.CreateBindingElements().Find<PrivacyNoticeBindingElement>();

                if (null != privacyNoticeBE)
                {
                    privacyNotice = privacyNoticeBE.Url;
                    privacyNoticeVersion = privacyNoticeBE.Version;
                }
            }
        }

        // Summary:
        //  Searches a binding for a single IssuedSecurityTokenParameters.  This method will throw an 
        //  argument exception if more than one is found.
        //
        // Parameters:
        //  currentStsBinding   -  The Binding to search.
        //
        // Return Value:
        //  Returns an IssuedSecurityTokenParameters if one is found, otherwise null.
        //
        static IssuedSecurityTokenParameters TryGetNextStsIssuedTokenParameters(Binding currentStsBinding)
        {
            if (null == currentStsBinding)
            {
                return null;
            }

            BindingElementCollection bindingElements = currentStsBinding.CreateBindingElements();
            SecurityBindingElement secBindingElement = bindingElements.Find<SecurityBindingElement>();

            return TryGetNextStsIssuedTokenParameters(secBindingElement);
        }

        // Summary:
        //  Searches a security binding element for a single IssuedSecurityTokenParameters.  This method will throw an 
        //  argument exception if more than one is found.
        //
        // Parameters:
        //  securityBindingEle   -  The SecurityBindingElement to search.
        //
        // Return Value:
        //  Returns an IssuedSecurityTokenParameters if one is found, otherwise null.
        //
        static IssuedSecurityTokenParameters TryGetNextStsIssuedTokenParameters(SecurityBindingElement securityBindingEle)
        {
            if (securityBindingEle == null)
            {
                return null;
            }

            //
            // This object can have a value assigned to it exactly once.  After one assignment of a non-null value
            // any other non-null assignment will cause the object to throw an argument excaption.
            //
            ThrowOnMultipleAssignment<IssuedSecurityTokenParameters> issuedTokenParameters =
                new ThrowOnMultipleAssignment<IssuedSecurityTokenParameters>(SR.GetString(SR.TooManyIssuedSecurityTokenParameters));

            FindInfoCardIssuerBinding(securityBindingEle, issuedTokenParameters);

            return issuedTokenParameters.Value;
        }

        static void FindInfoCardIssuerBinding(SecurityBindingElement secBindingElement, ThrowOnMultipleAssignment<IssuedSecurityTokenParameters> issuedSecurityTokenParameters)
        {
            if (secBindingElement == null)
                return;

            //
            // Go down the list of possible places for an IssuedSecurityTokenParameters, and hope we don't 
            // miss anything.
            //
            SecurityTokenParametersEnumerable tokenParamEnumerator = new SecurityTokenParametersEnumerable(secBindingElement);
            foreach (SecurityTokenParameters param in tokenParamEnumerator)
            {
                IssuedSecurityTokenParameters issuedTokenParam = param as IssuedSecurityTokenParameters;
                if (issuedTokenParam != null &&
                    ((issuedTokenParam.IssuerBinding == null ||
                    issuedTokenParam.IssuerAddress == null ||
                    issuedTokenParam.IssuerAddress.IsAnonymous ||
                    SelfIssuerUri.Equals(issuedTokenParam.IssuerAddress)) ||
                    (IsReferralToManagedIssuer(issuedTokenParam.IssuerBinding))))
                {
                    if (issuedSecurityTokenParameters != null)
                        issuedSecurityTokenParameters.Value = issuedTokenParam;
                }
                else if (param is SecureConversationSecurityTokenParameters)
                {
                    IssuedSecurityTokenParameters istp = TryGetNextStsIssuedTokenParameters(((SecureConversationSecurityTokenParameters)param).BootstrapSecurityBindingElement);
                    if ((istp != null) && (issuedSecurityTokenParameters != null))
                    {
                        issuedSecurityTokenParameters.Value = istp;
                    }
                }
                else if ((issuedTokenParam != null) && (issuedTokenParam.IssuerBinding != null))
                {
                    BindingElementCollection bindingElements = issuedTokenParam.IssuerBinding.CreateBindingElements();
                    SecurityBindingElement innerSecurityBindingElement = bindingElements.Find<SecurityBindingElement>();
                    IssuedSecurityTokenParameters istp = TryGetNextStsIssuedTokenParameters(innerSecurityBindingElement);
                    if ((istp != null) && (issuedSecurityTokenParameters != null))
                    {
                        issuedSecurityTokenParameters.Value = issuedTokenParam;
                    }
                }
            }

        }

        //
        // Summary:
        //  This class throws an Argument exception if an attempt is made to assign a non-null
        //  value to the Value property more than once.
        //
        class ThrowOnMultipleAssignment<T>
        {

            string m_errorString;
            T m_value;

            public T Value
            {
                get { return m_value; }
                set
                {
                    if (null != m_value && null != value)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(m_errorString);
                    }
                    else if (null == m_value)
                    {
                        m_value = value;
                    }
                }
            }

            //
            // Parameters:
            //  errorString  - If Value gets assigned to more than once an argument exception will be thrown with this
            //                 string as the Exception string.
            //
            public ThrowOnMultipleAssignment(string errorString)
            {
                Fx.Assert(!String.IsNullOrEmpty(errorString), "Must have an error string");

                m_errorString = errorString;
            }
        }

        class InternalInfoCardTokenProvider : SecurityTokenProvider, IDisposable
        {
            InfoCardChannelParameter m_infocardChannelParameter;

            public InternalInfoCardTokenProvider(InfoCardChannelParameter infocardChannelParameter)
            {
                this.m_infocardChannelParameter = infocardChannelParameter;
            }


            protected override SecurityToken GetTokenCore(TimeSpan timeout)
            {
                if (null != m_infocardChannelParameter && null != m_infocardChannelParameter.Token)
                {
                    if (m_infocardChannelParameter.Token.ValidTo < DateTime.UtcNow)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ExpiredSecurityTokenException((SR.GetString(SR.ExpiredTokenInChannelParameters))));
                    }
                    else
                    {
                        return m_infocardChannelParameter.Token;
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException((SR.GetString(SR.NoTokenInChannelParameters))));
                }
            }

            public void Dispose()
            {
                //
                // Don't want to pull the rug under the user when channel is aborted
                // as user may be managing cards - nothing to do.
                //
            }

        }

        //
        // Summary:
        //  This class knows how to walk a chain of IssuedSecurityTokenParameters and figure out at what point
        //  in the chain the InfoCard system should be invoked.  The idea is to identify the single most 
        //  appropriate place in the referral chain to involve user selection(if at all) . This decision is affected
        //  by the previously established relationships that the user has with some issuers. Some issuers may have 
        //  issued "InfoCards" to the user allowing them to choose which collection of claims should be used to generate
        //  a token for the relying party.
        //
        //  When asked for a token provider this class will return a FederatedServiceTokenProvider for
        //  an IssuedSecurityTokenParameters until it reaches the previously identified relyingparty/issuer, 
        //  at which point it will return an InternalTokenProvider instance, which will invoke the infocard 
        //  system allowing the user to get involved in the token generatio process.
        // 
        class InternalClientCredentials : ClientCredentials
        {
            Uri m_relyingPartyIssuer;
            ClientCredentials m_clientCredentials;
            InfoCardChannelParameter m_infocardChannelParameter;

            //
            // Summary:
            //  Constructs the policy chain and determines the depth at which to invoke the InfoCard system.
            //
            // Parameters
            //  target      - Target specified in CreateChannel call. This will fully specify a referral chain.
            //  parameters  - Security parameters for current invocation.
            //    
            public InternalClientCredentials(
                            ClientCredentials infocardCredentials,
                            EndpointAddress target,
                            Uri relyingPartyIssuer,
                            InfoCardChannelParameter infocardChannelParameter)
                : base(infocardCredentials)
            {
                m_relyingPartyIssuer = relyingPartyIssuer;
                m_clientCredentials = infocardCredentials;
                m_infocardChannelParameter = infocardChannelParameter;

            }

            InternalClientCredentials(InternalClientCredentials other)
                : base(other)
            {
                m_relyingPartyIssuer = other.m_relyingPartyIssuer;
                m_clientCredentials = other.m_clientCredentials;
                m_infocardChannelParameter = other.InfoCardChannelParameter;
            }

            public InfoCardChannelParameter InfoCardChannelParameter
            {
                get
                {
                    return m_infocardChannelParameter;
                }
            }
            public override SecurityTokenManager CreateSecurityTokenManager()
            {
                return new InternalClientCredentialsSecurityTokenManager(this, m_infocardChannelParameter);
            }

            public override void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
            {
            }

            protected override ClientCredentials CloneCore()
            {
                return new InternalClientCredentials(this);
            }


            class InternalClientCredentialsSecurityTokenManager : ClientCredentialsSecurityTokenManager
            {
                Uri m_relyingPartyIssuer;
                InfoCardChannelParameter m_infocardChannelParameter;

                public InternalClientCredentialsSecurityTokenManager(InternalClientCredentials internalClientCredentials, InfoCardChannelParameter infocardChannelParameter)
                    : base(internalClientCredentials)
                {
                    m_relyingPartyIssuer = internalClientCredentials.m_relyingPartyIssuer;
                    m_infocardChannelParameter = infocardChannelParameter;

                }

                public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement)
                {
                    if (IsIssuedSecurityTokenRequirement(tokenRequirement))
                    {
                        EndpointAddress target = tokenRequirement.GetProperty<EndpointAddress>(ServiceModelSecurityTokenRequirement.TargetAddressProperty);
                        IssuedSecurityTokenParameters issuedTokenParameters = tokenRequirement.GetProperty<IssuedSecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty);
                        return InfoCardHelper.CreateTokenProviderForNextLeg(tokenRequirement, target, issuedTokenParameters.IssuerAddress, m_relyingPartyIssuer, this, m_infocardChannelParameter);
                    }
                    else
                    {
                        return base.CreateSecurityTokenProvider(tokenRequirement);
                    }
                }
            }
        }

        class PolicyElement
        {
            EndpointAddress m_target;
            EndpointAddress m_issuer;
            Collection<XmlElement> m_parameters;
            Uri m_policyNoticeLink;
            int m_policyNoticeVersion;
            bool m_isManagedIssuer;
            Binding m_binding;

            public bool IsManagedIssuer
            {
                get { return m_isManagedIssuer; }
            }

            public EndpointAddress Issuer
            {
                get { return m_issuer; }
            }

            public Binding Binding
            {
                get { return m_binding; }
            }

            //
            // Parameters:
            //  target     - The target of the token being described.
            //  parameters - describes the type of token required by the target.
            //
            public PolicyElement(EndpointAddress target, EndpointAddress issuer, Collection<XmlElement> parameters, Uri privacyNoticeLink, int privacyNoticeVersion, bool isManagedIssuer, Binding binding)
            {
                m_target = target;
                m_issuer = issuer;
                m_parameters = parameters;
                m_policyNoticeLink = privacyNoticeLink;
                m_policyNoticeVersion = privacyNoticeVersion;
                m_isManagedIssuer = isManagedIssuer;
                m_binding = binding;
            }

            //
            // Summary:
            //   Convert the PolicyElement to an CardSpacePolicyElement
            //
            // Returns:
            //  The CardSpacePolicyElement object
            //
            [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "The call to CardSpacePolicyElement is safe.")]
            public CardSpacePolicyElement ToCardSpacePolicyElement()
            {
                return new CardSpacePolicyElement(EndPointAddressToXmlElement(m_target),
                                                  EndPointAddressToXmlElement(m_issuer),
                                                  m_parameters,
                                                  m_policyNoticeLink,
                                                  m_policyNoticeVersion,
                                                  m_isManagedIssuer);

            }

            //
            // Summary:
            //   Convert an endpoint address to an XmlElement
            //
            // Parameters:
            //   epr - EndPointAddress that need to be converted
            //
            // Returns:
            //  The XmlElement version of the epr
            //
            private XmlElement EndPointAddressToXmlElement(EndpointAddress epr)
            {
                if (null == epr)
                    return null;

                using (MemoryStream buffer = new MemoryStream())
                {
                    using (XmlWriter writer = new XmlTextWriter(buffer, Encoding.UTF8))
                    {
                        epr.WriteTo(AddressingVersion.WSAddressing10, writer);
                        //
                        // Skip the BOM and trailing padding.
                        //
                        writer.Flush();
                        buffer.Flush();
                        buffer.Seek(0, 0);
                        using (XmlReader reader = XmlReader.Create(buffer))
                        {
                            XmlDocument doc = new XmlDocument();
                            return (XmlElement)doc.ReadNode(reader);
                        }

                    }

                }
            }
        }

        class SecurityTokenParametersEnumerable : IEnumerable<SecurityTokenParameters>
        {
            SecurityBindingElement sbe;

            public SecurityTokenParametersEnumerable(SecurityBindingElement sbe)
            {
                if (sbe == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sbe");
                this.sbe = sbe;
            }

            public IEnumerator<SecurityTokenParameters> GetEnumerator()
            {
                foreach (SecurityTokenParameters stp in this.sbe.EndpointSupportingTokenParameters.Endorsing)
                    if (stp != null)
                        yield return stp;
                foreach (SecurityTokenParameters stp in this.sbe.EndpointSupportingTokenParameters.SignedEndorsing)
                    if (stp != null)
                        yield return stp;
                foreach (SupportingTokenParameters str in this.sbe.OperationSupportingTokenParameters.Values)
                    if (str != null)
                    {
                        foreach (SecurityTokenParameters stp in str.Endorsing)
                            if (stp != null)
                                yield return stp;
                        foreach (SecurityTokenParameters stp in str.SignedEndorsing)
                            if (stp != null)
                                yield return stp;
                    }
                if (this.sbe is SymmetricSecurityBindingElement)
                {
                    SymmetricSecurityBindingElement ssbe = (SymmetricSecurityBindingElement)sbe;
                    if (ssbe.ProtectionTokenParameters != null)
                        yield return ssbe.ProtectionTokenParameters;
                }
                else if (this.sbe is AsymmetricSecurityBindingElement)
                {
                    AsymmetricSecurityBindingElement asbe = (AsymmetricSecurityBindingElement)sbe;
                    if (asbe.RecipientTokenParameters != null)
                        yield return asbe.RecipientTokenParameters;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }
        }

    }
}
