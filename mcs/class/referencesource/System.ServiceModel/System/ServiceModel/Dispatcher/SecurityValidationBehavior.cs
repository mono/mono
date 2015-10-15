//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.IdentityModel.Tokens;

    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Net.Security;
    using System.Security.Principal;

    class SecurityValidationBehavior : IEndpointBehavior, IServiceBehavior
    {
        static SecurityValidationBehavior instance;

        public static SecurityValidationBehavior Instance
        {
            get
            {
                if (instance == null)
                    instance = new SecurityValidationBehavior();
                return instance;
            }
        }

        class ValidationBinding : Binding
        {
            Binding binding;
            BindingElementCollection elements;

            public ValidationBinding(Binding binding)
                : base(binding.Name, binding.Namespace)
            {
                this.binding = binding;
            }

            public override string Scheme
            {
                get { return this.binding.Scheme; }
            }

            public override BindingElementCollection CreateBindingElements()
            {
                if (this.elements == null)
                {
                    this.elements = this.binding.CreateBindingElements();
                }
                return this.elements;
            }

            public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingParameterCollection parameters)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(params object[] parameters)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, params object[] parameters)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, params object[] parameters)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, ListenUriMode listenUriMode, params object[] parameters)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingParameterCollection parameters)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, BindingParameterCollection parameters)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, BindingParameterCollection parameters)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, ListenUriMode listenUriMode, BindingParameterCollection parameters)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override bool CanBuildChannelFactory<TChannel>(BindingParameterCollection parameters)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override bool CanBuildChannelListener<TChannel>(BindingParameterCollection parameters)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
            if (serviceEndpoint == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpoint");
            SecurityBindingElement sbe;
            Binding binding = new ValidationBinding(serviceEndpoint.Binding);
            ValidateBinding(binding, serviceEndpoint.Contract, out sbe);
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection parameters)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");

            for (int i = 0; i < description.Endpoints.Count; i++)
            {
                ServiceEndpoint endpoint = description.Endpoints[i];
                Binding binding = new ValidationBinding(endpoint.Binding);
                SecurityBindingElement sbe;
                ValidateBinding(binding, endpoint.Contract, out sbe);
                if (sbe != null)
                {
                    SecurityTokenParameterInclusionModeRule.Validate(sbe, binding, endpoint.Contract, description.Behaviors);
                }
            }

            WindowsIdentitySupportRule.Validate(description);
            UsernameImpersonationRule.Validate(description);
            MissingClientCertificateRule.Validate(description);
        }

        void ValidateBinding(Binding binding, ContractDescription contract, out SecurityBindingElement securityBindingElement)
        {
            securityBindingElement = SecurityValidationBehavior.GetSecurityBinding(binding, contract);
            if (securityBindingElement != null)
                ValidateSecurityBinding(securityBindingElement, binding, contract);
            else
                ValidateNoSecurityBinding(binding, contract);

        }

        void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
        {
            ContractProtectionRequirementsRule.ValidateSecurityBinding(sbe, binding, contract);
            CookieAndSessionProtectionRequirementsRule.ValidateSecurityBinding(sbe, binding, contract);
            SoapOverSecureTransportRequirementsRule.ValidateSecurityBinding(sbe, binding, contract);
            SecurityVersionSupportForEncryptedKeyBindingRule.ValidateSecurityBinding(sbe, binding, contract);
            SecurityVersionSupportForThumbprintKeyIdentifierClauseRule.ValidateSecurityBinding(sbe, binding, contract);
            SecurityBindingSupportForOneWayOnlyRule.ValidateSecurityBinding(sbe, binding, contract);
            IssuedKeySizeCompatibilityWithAlgorithmSuiteRule.ValidateSecurityBinding(sbe, binding, contract);
            MessageSecurityAndManualAddressingRule.ValidateSecurityBinding(sbe, binding, contract);
            NoStreamingWithSecurityRule.ValidateSecurityBinding(sbe, binding, contract);
            UnknownHeaderProtectionRequirementsRule.ValidateSecurityBinding(sbe, binding, contract);
            BearerKeyTypeIssuanceRequirementRule.ValidateSecurityBinding(sbe, binding, contract);
        }

        void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
        {
            ContractProtectionRequirementsRule.ValidateNoSecurityBinding(binding, contract);
            CookieAndSessionProtectionRequirementsRule.ValidateNoSecurityBinding(binding, contract);
            SoapOverSecureTransportRequirementsRule.ValidateNoSecurityBinding(binding, contract);
            SecurityVersionSupportForEncryptedKeyBindingRule.ValidateNoSecurityBinding(binding, contract);
            SecurityVersionSupportForThumbprintKeyIdentifierClauseRule.ValidateNoSecurityBinding(binding, contract);
            SecurityBindingSupportForOneWayOnlyRule.ValidateNoSecurityBinding(binding, contract);
            IssuedKeySizeCompatibilityWithAlgorithmSuiteRule.ValidateNoSecurityBinding(binding, contract);
            MessageSecurityAndManualAddressingRule.ValidateNoSecurityBinding(binding, contract);
            UnknownHeaderProtectionRequirementsRule.ValidateNoSecurityBinding(binding, contract);
            BearerKeyTypeIssuanceRequirementRule.ValidateNoSecurityBinding(binding, contract);
        }

        static SecurityBindingElement GetSecurityBinding(Binding binding, ContractDescription contract)
        {
            SecurityBindingElement sbe = null;
            BindingElementCollection elements = binding.CreateBindingElements();
            for (int i = 0; i < elements.Count; i++)
            {
                BindingElement element = elements[i];
                if (element is SecurityBindingElement)
                {
                    if (sbe != null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.MoreThanOneSecurityBindingElementInTheBinding, binding.Name, binding.Namespace, contract.Name, contract.Namespace)));
                    sbe = (SecurityBindingElement)element;
                }
            }
            return sbe;
        }


        internal void AfterBuildTimeValidation(ServiceDescription description)
        {
            S4UImpersonationRule.Validate(description);
        }

        // We do not allow streaming with message security which makes our service vulnerable
        // for example, GetWhitespace may be a problem if it’s called on unbounded data.
        static class NoStreamingWithSecurityRule
        {
            static public void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                // check to see if we are doing message security
                // if transport security, the sbe would be transportsecuritybindingelement
                if (sbe is SymmetricSecurityBindingElement || sbe is AsymmetricSecurityBindingElement)
                {
                    // check to see if we are streaming
                    // (Microsoft 53690): need to have a general way get the transfer Mode from the binding
                    // TransferMode transferMode = binding.GetProperty<TransferMode>(new BindingParameterCollection());
                    if (GetTransferMode(binding) != TransferMode.Buffered)
                    {
                        // throw 
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NoStreamingWithSecurity, binding.Name, binding.Namespace)));
                    }
                }
            }

            static TransferMode GetTransferMode(Binding binding)
            {
                TransferMode mode = TransferMode.Buffered;

                BindingElementCollection elements = binding.CreateBindingElements();
                TransportBindingElement element = elements.Find<TransportBindingElement>();

                if (element is ConnectionOrientedTransportBindingElement)
                {
                    mode = ((ConnectionOrientedTransportBindingElement)element).TransferMode;
                }
                else if (element is HttpTransportBindingElement)
                {
                    mode = ((HttpTransportBindingElement)element).TransferMode;
                }

                return mode;
            }
        }


        static class WindowsIdentitySupportRule
        {
            static public void Validate(ServiceDescription description)
            {
                bool impersonateCallerForAllServiceMethods = false;
                ServiceAuthorizationBehavior authorizationBehavior = description.Behaviors.Find<ServiceAuthorizationBehavior>();
                if (authorizationBehavior != null)
                {
                    impersonateCallerForAllServiceMethods = authorizationBehavior.ImpersonateCallerForAllOperations;
                }
                else
                {
                    impersonateCallerForAllServiceMethods = false;
                }
                for (int i = 0; i < description.Endpoints.Count; i++)
                {
                    ServiceEndpoint endpoint = description.Endpoints[i];
                    if (endpoint.InternalIsSystemEndpoint(description))
                    {
                        continue;
                    }
                    for (int j = 0; j < endpoint.Contract.Operations.Count; j++)
                    {
                        OperationDescription operation = endpoint.Contract.Operations[j];
                        OperationBehaviorAttribute operationBehavior = operation.Behaviors.Find<OperationBehaviorAttribute>();
                        if (impersonateCallerForAllServiceMethods &&
                            !operation.IsServerInitiated() &&
                            (operationBehavior == null || operationBehavior.Impersonation == ImpersonationOption.NotAllowed))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.OperationDoesNotAllowImpersonation, operation.Name, endpoint.Contract.Name, endpoint.Contract.Namespace)));
                        }
                        if (impersonateCallerForAllServiceMethods || (operationBehavior != null && operationBehavior.Impersonation == ImpersonationOption.Required))
                        {
                            ValidateWindowsIdentityCapability(endpoint.Binding, endpoint.Contract, operation);
                        }
                    }
                }
            }

            static void ValidateWindowsIdentityCapability(Binding binding, ContractDescription contract, OperationDescription operation)
            {
                bool windowsIdentityProvided = false;

                ISecurityCapabilities capabilities = binding.GetProperty<ISecurityCapabilities>(new BindingParameterCollection());
                if (capabilities != null && capabilities.SupportsClientWindowsIdentity)
                {
                    windowsIdentityProvided = true;
                }

                if (!windowsIdentityProvided)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.BindingDoesNotSupportWindowsIdenityForImpersonation, operation.Name, binding.Name, binding.Namespace, contract.Name, contract.Namespace)));
                }
            }
        }

        static class S4UImpersonationRule
        {
            const int WindowsServerMajorNumber = 5;
            const int WindowsServerMinorNumber = 2;

            static bool IsS4URequiredForImpersonation(SecurityBindingElement sbe)
            {
                foreach (SecurityTokenParameters stp in new SecurityTokenParametersEnumerable(sbe, true))
                {
                    if (stp is SecureConversationSecurityTokenParameters)
                    {
                        SecureConversationSecurityTokenParameters scstp = (SecureConversationSecurityTokenParameters)stp;

                        if (scstp.RequireCancellation == false)
                            return true;

                        if (scstp.BootstrapSecurityBindingElement != null)
                        {
                            return IsS4URequiredForImpersonation(scstp.BootstrapSecurityBindingElement);
                        }
                    }

                    if (stp is SspiSecurityTokenParameters
                        && ((SspiSecurityTokenParameters)stp).RequireCancellation == false)
                        return true;

                    if (stp is X509SecurityTokenParameters)
                        return true;
                }

                return false;
            }

            static public void Validate(ServiceDescription description)
            {
                ServiceAuthorizationBehavior behavior = description.Behaviors.Find<ServiceAuthorizationBehavior>();
                bool impersonateCallerForAllMethods = (behavior != null) ? behavior.ImpersonateCallerForAllOperations : false;
                for (int i = 0; i < description.Endpoints.Count; i++)
                {
                    ServiceEndpoint endpoint = description.Endpoints[i];
                    if (endpoint.InternalIsSystemEndpoint(description))
                    {
                        continue;
                    }

                    bool isImpersonationRequested = impersonateCallerForAllMethods;
                    if (!isImpersonationRequested)
                    {
                        isImpersonationRequested = ValidatorUtils.EndpointRequiresImpersonation(endpoint);
                    }
                    if (isImpersonationRequested)
                    {
                        ICollection<BindingElement> bindingElements = endpoint.Binding.CreateBindingElements();
                        foreach (BindingElement element in bindingElements)
                        {
                            SecurityBindingElement sbe = (element as SecurityBindingElement);
                            if (sbe != null)
                            {
                                if (IsS4URequiredForImpersonation(sbe))
                                {
                                    Version osVersion = Environment.OSVersion.Version;
                                    if ((osVersion.Major < WindowsServerMajorNumber)
                                        || ((osVersion.Major == WindowsServerMajorNumber) && (osVersion.Minor < WindowsServerMinorNumber)))
                                    {
                                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                            SR.GetString(SR.CannotPerformS4UImpersonationOnPlatform, endpoint.Binding.Name, endpoint.Binding.Namespace, endpoint.Contract.Name, endpoint.Contract.Namespace)));
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        static class UnknownHeaderProtectionRequirementsRule
        {
            static public void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                if (sbe is SymmetricSecurityBindingElement || sbe is AsymmetricSecurityBindingElement)
                    ValidateContract(binding, contract, sbe.GetIndividualProperty<ISecurityCapabilities>().SupportedRequestProtectionLevel, sbe.GetIndividualProperty<ISecurityCapabilities>().SupportedResponseProtectionLevel);
                else
                    ValidateContract(binding, contract, ProtectionLevel.None, ProtectionLevel.None);
            }

            static public void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
                ValidateContract(binding, contract, ProtectionLevel.None, ProtectionLevel.None);
            }

            static void ValidateContract(Binding binding, ContractDescription contract, ProtectionLevel defaultRequestProtectionLevel, ProtectionLevel defaultResponseProtectionLevel)
            {
                if (contract == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contract"));

                ProtectionLevel contractScopeDefaultRequestProtectionLevel;
                ProtectionLevel contractScopeDefaultResponseProtectionLevel;
                if (contract.HasProtectionLevel)
                {
                    contractScopeDefaultRequestProtectionLevel = contract.ProtectionLevel;
                    contractScopeDefaultResponseProtectionLevel = contract.ProtectionLevel;
                }
                else
                {
                    contractScopeDefaultRequestProtectionLevel = defaultRequestProtectionLevel;
                    contractScopeDefaultResponseProtectionLevel = defaultResponseProtectionLevel;
                }

                foreach (OperationDescription operation in contract.Operations)
                {
                    ProtectionLevel operationScopeDefaultRequestProtectionLevel;
                    ProtectionLevel operationScopeDefaultResponseProtectionLevel;
                    if (operation.HasProtectionLevel)
                    {
                        operationScopeDefaultRequestProtectionLevel = operation.ProtectionLevel;
                        operationScopeDefaultResponseProtectionLevel = operation.ProtectionLevel;
                    }
                    else
                    {
                        operationScopeDefaultRequestProtectionLevel = contractScopeDefaultRequestProtectionLevel;
                        operationScopeDefaultResponseProtectionLevel = contractScopeDefaultResponseProtectionLevel;
                    }
                    foreach (MessageDescription message in operation.Messages)
                    {
                        ProtectionLevel messageScopeDefaultProtectionLevel;
                        if (message.HasProtectionLevel)
                        {
                            messageScopeDefaultProtectionLevel = message.ProtectionLevel;
                        }
                        else if (message.Direction == MessageDirection.Input)
                        {
                            messageScopeDefaultProtectionLevel = operationScopeDefaultRequestProtectionLevel;
                        }
                        else
                        {
                            messageScopeDefaultProtectionLevel = operationScopeDefaultResponseProtectionLevel;
                        }

                        foreach (MessageHeaderDescription header in message.Headers)
                        {
                            ProtectionLevel headerScopeDefaultProtectionLevel;

                            if (header.HasProtectionLevel)
                                headerScopeDefaultProtectionLevel = header.ProtectionLevel;
                            else
                                headerScopeDefaultProtectionLevel = messageScopeDefaultProtectionLevel;

                            //
                            // Finally we figured out the protection level for the individual header.
                            // We need to throw if the header is some unknown header, i.e., user can stick any Xml frag
                            // at the runtime, AND, its protection level is not ProtectionLevel.None
                            //
                            if (header.IsUnknownHeaderCollection && headerScopeDefaultProtectionLevel != ProtectionLevel.None)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnknownHeaderCannotProtected, contract.Name, contract.Namespace, header.Name, header.Namespace)));
                            }
                        }
                    }
                }
            }
        }

        static class ContractProtectionRequirementsRule
        {
            static public void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                if (sbe is SymmetricSecurityBindingElement || sbe is AsymmetricSecurityBindingElement)
                    ValidateContract(binding, contract, sbe.GetIndividualProperty<ISecurityCapabilities>().SupportedRequestProtectionLevel, sbe.GetIndividualProperty<ISecurityCapabilities>().SupportedResponseProtectionLevel);
                else
                    ValidateContract(binding, contract, ProtectionLevel.None, ProtectionLevel.None);
            }

            static public void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
                ValidateContract(binding, contract, ProtectionLevel.None, ProtectionLevel.None);
            }

            static void ValidateContract(Binding binding, ContractDescription contract, ProtectionLevel defaultRequestProtectionLevel, ProtectionLevel defaultResponseProtectionLevel)
            {
                ProtectionLevel requestProtectionLevel;
                ProtectionLevel responseProtectionLevel;
                GetRequiredProtectionLevels(contract, defaultRequestProtectionLevel, defaultResponseProtectionLevel, out requestProtectionLevel, out responseProtectionLevel);
                ValidateBindingProtectionCapability(binding, contract, requestProtectionLevel, responseProtectionLevel);
            }

            static internal void GetRequiredProtectionLevels(ContractDescription contract, ProtectionLevel defaultRequestProtectionLevel, ProtectionLevel defaultResponseProtectionLevel, out ProtectionLevel request, out ProtectionLevel response)
            {
                ChannelProtectionRequirements requirements = ChannelProtectionRequirements.CreateFromContract(contract, defaultRequestProtectionLevel, defaultResponseProtectionLevel, false);

                if (requirements.IncomingSignatureParts.IsEmpty())
                {
                    request = ProtectionLevel.None;
                }
                else if (requirements.IncomingEncryptionParts.IsEmpty())
                {
                    request = ProtectionLevel.Sign;
                }
                else
                {
                    request = ProtectionLevel.EncryptAndSign;
                }

                if (requirements.OutgoingSignatureParts.IsEmpty())
                {
                    response = ProtectionLevel.None;
                }
                else if (requirements.OutgoingEncryptionParts.IsEmpty())
                {
                    response = ProtectionLevel.Sign;
                }
                else
                {
                    response = ProtectionLevel.EncryptAndSign;
                }
            }

            static void ValidateBindingProtectionCapability(Binding binding, ContractDescription contract, ProtectionLevel request, ProtectionLevel response)
            {
                bool requestValidated = request == ProtectionLevel.None;
                bool responseValidated = response == ProtectionLevel.None;

                if (!requestValidated || !responseValidated)
                {
                    ISecurityCapabilities capabilities = binding.GetProperty<ISecurityCapabilities>(new BindingParameterCollection());
                    if (capabilities != null)
                    {
                        if (!requestValidated)
                        {
                            requestValidated = ProtectionLevelHelper.IsStrongerOrEqual(capabilities.SupportedRequestProtectionLevel, request);
                        }
                        if (!responseValidated)
                        {
                            responseValidated = ProtectionLevelHelper.IsStrongerOrEqual(capabilities.SupportedResponseProtectionLevel, response);
                        }
                    }
                }

                if (!requestValidated)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.AtLeastOneContractOperationRequestRequiresProtectionLevelNotSupportedByBinding, contract.Name, contract.Namespace, binding.Name, binding.Namespace)));
                }
                if (!responseValidated)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.AtLeastOneContractOperationResponseRequiresProtectionLevelNotSupportedByBinding, contract.Name, contract.Namespace, binding.Name, binding.Namespace)));
                }
            }
        }

        static class BearerKeyTypeIssuanceRequirementRule
        {
            static public void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                foreach (SecurityTokenParameters stp in new SecurityTokenParametersEnumerable(sbe, true))
                {
                    if (stp is IssuedSecurityTokenParameters)
                    {
                        IssuedSecurityTokenParameters issuedParameters = stp as IssuedSecurityTokenParameters;
                        if (issuedParameters.KeyType == System.IdentityModel.Tokens.SecurityKeyType.BearerKey)
                        {
                            // The issued Bearer token cannot be used as the primary protection token and it cannot be
                            // used as a Endorsing or Signed Endorsing token.
                            if ((sbe is SymmetricSecurityBindingElement) && IsBearerKeyType(((SymmetricSecurityBindingElement)sbe).ProtectionTokenParameters))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidBearerKeyUsage, binding.Name, binding.Namespace)));
                            }

                            if ((sbe is AsymmetricSecurityBindingElement) && (IsBearerKeyType(((AsymmetricSecurityBindingElement)sbe).InitiatorTokenParameters) || IsBearerKeyType(((AsymmetricSecurityBindingElement)sbe).RecipientTokenParameters)))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidBearerKeyUsage, binding.Name, binding.Namespace)));
                            }

                            foreach (SecurityTokenParameters tokenParam in sbe.EndpointSupportingTokenParameters.Endorsing)
                            {
                                if (IsBearerKeyType(tokenParam))
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidBearerKeyUsage, binding.Name, binding.Namespace)));
                                }
                            }

                            foreach (SecurityTokenParameters tokenParam in sbe.EndpointSupportingTokenParameters.SignedEndorsing)
                            {
                                if (IsBearerKeyType(tokenParam))
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InvalidBearerKeyUsage, binding.Name, binding.Namespace)));
                                }
                            }
                        }

                        if (issuedParameters.IssuerBinding != null)
                        {
                            SecurityBindingElement secBindingEle = SecurityValidationBehavior.GetSecurityBinding(issuedParameters.IssuerBinding, contract);
                            if (secBindingEle != null)
                                ValidateSecurityBinding(secBindingEle, issuedParameters.IssuerBinding, contract);
                        }
                    }
                    else if (stp is SecureConversationSecurityTokenParameters)
                    {
                        SecureConversationSecurityTokenParameters scParameters = stp as SecureConversationSecurityTokenParameters;
                        ValidateSecurityBinding(scParameters.BootstrapSecurityBindingElement, binding, contract);
                    }
                }
            }

            static public void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }

            static bool IsBearerKeyType(SecurityTokenParameters tokenParameters)
            {
                if (!(tokenParameters is IssuedSecurityTokenParameters))
                    return false;

                return ((IssuedSecurityTokenParameters)tokenParameters).KeyType == SecurityKeyType.BearerKey;
            }

        }

        static class CookieAndSessionProtectionRequirementsRule
        {
            static public void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                if (!(sbe is TransportSecurityBindingElement))
                    foreach (SecurityTokenParameters stp in new SecurityTokenParametersEnumerable(sbe, true))
                    {
                        SecureConversationSecurityTokenParameters scstp = stp as SecureConversationSecurityTokenParameters;
                        if (scstp != null)
                        {
                            ISecurityCapabilities bootstrapSecurityCapabilities = scstp.BootstrapSecurityBindingElement.GetIndividualProperty<ISecurityCapabilities>();
                            if (bootstrapSecurityCapabilities != null
                                && bootstrapSecurityCapabilities.SupportedRequestProtectionLevel == ProtectionLevel.EncryptAndSign
                                && bootstrapSecurityCapabilities.SupportedResponseProtectionLevel == ProtectionLevel.EncryptAndSign)
                            {
                                continue;
                            }

                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                SR.GetString(SR.BindingDoesNotSupportProtectionForRst, binding.Name, binding.Namespace, contract.Name, contract.Namespace)));
                        }
                    }
            }

            static public void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }
        }

        static class SoapOverSecureTransportRequirementsRule
        {
            static public void ValidateSecurityBinding(SecurityBindingElement securityBindingElement, Binding binding, ContractDescription contract)
            {
                if (securityBindingElement is TransportSecurityBindingElement && !securityBindingElement.AllowInsecureTransport)
                {
                    // ensure that if soap security cookie/session is configured, then the authentication mode supports encryption
                    IEnumerable<BindingElement> elements = binding.CreateBindingElements();
                    Collection<BindingElement> bindingElementStack = new Collection<BindingElement>();
                    bool isBelowSecurity = false;
                    foreach (BindingElement element in elements)
                    {
                        SecurityBindingElement sbe = element as SecurityBindingElement;
                        if (sbe != null)
                        {
                            isBelowSecurity = true;
                        }
                        else if (isBelowSecurity)
                        {
                            bindingElementStack.Add(element);
                        }
                    }
                    bool isTransportProtected = false;
                    if (bindingElementStack.Count != 0)
                    {
                        BindingContext context = new BindingContext(new CustomBinding(bindingElementStack), new BindingParameterCollection());
                        ISecurityCapabilities transportCapabilities = context.GetInnerProperty<ISecurityCapabilities>();
                        if (transportCapabilities != null
                            && transportCapabilities.SupportsServerAuthentication
                            && transportCapabilities.SupportedRequestProtectionLevel == ProtectionLevel.EncryptAndSign
                            && transportCapabilities.SupportedResponseProtectionLevel == ProtectionLevel.EncryptAndSign)
                        {
                            isTransportProtected = true;
                        }
                    }

                    if (!isTransportProtected)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.TransportDoesNotProtectMessage, binding.Name, binding.Namespace, contract.Name, contract.Namespace)));
                    }
                }
            }

            static public void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }
        }

        static class IssuedKeySizeCompatibilityWithAlgorithmSuiteRule
        {
            static public void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                SecurityAlgorithmSuite algorithmSuite = sbe.DefaultAlgorithmSuite;
                foreach (SecurityTokenParameters stp in new SecurityTokenParametersEnumerable(sbe, true))
                {
                    if (stp is IssuedSecurityTokenParameters)
                    {
                        IssuedSecurityTokenParameters issuedParameters = stp as IssuedSecurityTokenParameters;
                        if (issuedParameters.KeySize != 0)
                        {
                            bool isCompatible = true;
                            if (issuedParameters.KeyType == System.IdentityModel.Tokens.SecurityKeyType.SymmetricKey &&
                                !sbe.DefaultAlgorithmSuite.IsSymmetricKeyLengthSupported(issuedParameters.KeySize))
                            {
                                isCompatible = false;

                            }
                            else if (issuedParameters.KeyType == System.IdentityModel.Tokens.SecurityKeyType.AsymmetricKey &&
                                !sbe.DefaultAlgorithmSuite.IsAsymmetricKeyLengthSupported(issuedParameters.KeySize))
                            {
                                isCompatible = false;
                            }
                            if (!isCompatible)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.IssuedKeySizeNotCompatibleWithAlgorithmSuite, binding.Name, binding.Namespace, sbe.DefaultAlgorithmSuite, issuedParameters.KeySize)));
                            }
                        }
                    }
                    else if (stp is SecureConversationSecurityTokenParameters)
                    {
                        SecureConversationSecurityTokenParameters scParameters = stp as SecureConversationSecurityTokenParameters;
                        ValidateSecurityBinding(scParameters.BootstrapSecurityBindingElement, binding, contract);
                    }
                }
            }

            static public void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }
        }

        static class SecurityTokenParameterInclusionModeRule
        {
            static void EnforceInclusionMode(Binding binding, SecurityTokenParameters stp, params SecurityTokenInclusionMode[] allowedInclusionModes)
            {
                bool isMatch = false;
                for (int i = 0; i < allowedInclusionModes.Length; ++i)
                {
                    if (stp.InclusionMode == allowedInclusionModes[i])
                    {
                        isMatch = true;
                        break;
                    }
                }
                if (!isMatch)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecurityTokenParametersHasIncompatibleInclusionMode, binding.Name, binding.Namespace, stp.GetType(), stp.InclusionMode, allowedInclusionModes[0])));
                }
            }

            static public void Validate(SecurityBindingElement sbe, Binding binding, ContractDescription contract, KeyedByTypeCollection<IServiceBehavior> behaviors)
            {
                if (behaviors != null)
                {
                    ServiceCredentials serviceCredentials = behaviors.Find<ServiceCredentials>();
                    if (serviceCredentials != null && serviceCredentials.GetType() != typeof(ServiceCredentials))
                    {
                        // A custom service credentials has been plugged in. Dont validate the binding
                        return;
                    }
                }
                SymmetricSecurityBindingElement ssbe = (sbe as SymmetricSecurityBindingElement);
                AsymmetricSecurityBindingElement asbe = (sbe as AsymmetricSecurityBindingElement);
                foreach (SecurityTokenParameters stp in new SecurityTokenParametersEnumerable(sbe, true))
                {
                    if (stp is RsaSecurityTokenParameters)
                    {
                        // rsa keys can only be referred to using keyinfo. There's no wire format for 
                        // serializing them
                        EnforceInclusionMode(binding, stp, SecurityTokenInclusionMode.Never);
                        continue;
                    }
                    if (stp is SecureConversationSecurityTokenParameters)
                    {
                        Validate(((SecureConversationSecurityTokenParameters)stp).BootstrapSecurityBindingElement, binding, contract, behaviors);
                    }
                    if (ssbe != null)
                    {
                        // for the protection token, if it is asymmetric inclusion mode should be Never
                        // all other cases inclusion mode should be AlwaysToRecipient/Once
                        if (ssbe.ProtectionTokenParameters == stp && stp.HasAsymmetricKey)
                        {
                            EnforceInclusionMode(binding, stp, SecurityTokenInclusionMode.Never);
                        }
                        else
                        {
                            EnforceInclusionMode(binding, stp, SecurityTokenInclusionMode.AlwaysToRecipient, SecurityTokenInclusionMode.Once);
                        }
                    }
                    else if (asbe != null)
                    {
                        if (asbe.InitiatorTokenParameters == stp && stp.HasAsymmetricKey)
                        {
                            // allow AlwaysToRecipient, Once and AlwaysToInitiator in this case since the duplex binding
                            // configures AlwaysToInitiator in this case
                            EnforceInclusionMode(binding, stp, SecurityTokenInclusionMode.AlwaysToRecipient, SecurityTokenInclusionMode.AlwaysToInitiator, SecurityTokenInclusionMode.Once);
                        }
                        else
                        {
                            EnforceInclusionMode(binding, stp, SecurityTokenInclusionMode.AlwaysToRecipient, SecurityTokenInclusionMode.Once);
                        }

                    }
                    else
                    {
                        EnforceInclusionMode(binding, stp, SecurityTokenInclusionMode.AlwaysToRecipient, SecurityTokenInclusionMode.Once);
                    }
                }
            }
        }

        static class SecurityVersionSupportForEncryptedKeyBindingRule
        {
            static public void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                SymmetricSecurityBindingElement ssbe = sbe as SymmetricSecurityBindingElement;
                if (sbe.MessageSecurityVersion.SecurityVersion == SecurityVersion.WSSecurity10
                    && ssbe != null
                    && ssbe.ProtectionTokenParameters != null
                    && ssbe.ProtectionTokenParameters.HasAsymmetricKey)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SecurityVersionDoesNotSupportEncryptedKeyBinding, binding.Name, binding.Namespace, contract.Name, contract.Namespace, SecurityVersion.WSSecurity11)));
                }
            }

            static public void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }
        }

        static class SecurityVersionSupportForThumbprintKeyIdentifierClauseRule
        {
            static public void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                if (sbe.MessageSecurityVersion.SecurityVersion == SecurityVersion.WSSecurity10)
                {
                    foreach (SecurityTokenParameters stp in new SecurityTokenParametersEnumerable(sbe))
                    {
                        X509SecurityTokenParameters x509 = stp as X509SecurityTokenParameters;
                        if (x509 != null && x509.X509ReferenceStyle == X509KeyIdentifierClauseType.Thumbprint)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                SR.GetString(SR.SecurityVersionDoesNotSupportThumbprintX509KeyIdentifierClause, binding.Name, binding.Namespace, contract.Name, contract.Namespace, SecurityVersion.WSSecurity11)));
                    }
                }
            }

            static public void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }
        }

        static class MessageSecurityAndManualAddressingRule
        {
            static public void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                TransportBindingElement transport = binding.CreateBindingElements().Find<TransportBindingElement>();
                if (transport != null && transport.ManualAddressing)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                SR.GetString(SR.MessageSecurityDoesNotWorkWithManualAddressing, binding.Name, binding.Namespace)));
                }
            }

            static public void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }
        }

        static class SecurityBindingSupportForOneWayOnlyRule
        {
            static public void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                if (sbe is AsymmetricSecurityBindingElement && ((AsymmetricSecurityBindingElement)sbe).IsCertificateSignatureBinding)
                {
                    for (int i = 0; i < contract.Operations.Count; i++)
                    {
                        OperationDescription operation = contract.Operations[i];
                        if (!operation.IsOneWay)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                SR.GetString(SR.SecurityBindingSupportsOneWayOnly, binding.Name, binding.Namespace, contract.Name, contract.Namespace)));
                    }
                }
            }

            static public void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }
        }

        static class MissingClientCertificateRule
        {
            static void ValidateCore(ServiceDescription description, ServiceCredentials credentials)
            {
                for (int i = 0; i < description.Endpoints.Count; i++)
                {
                    ServiceEndpoint endpoint = description.Endpoints[i];
                    BindingElementCollection elements = endpoint.Binding.CreateBindingElements();

                    SecurityBindingElement security = elements.Find<SecurityBindingElement>();
                    CompositeDuplexBindingElement duplex = elements.Find<CompositeDuplexBindingElement>();
                    if (security != null && duplex != null && SecurityBindingElement.IsMutualCertificateDuplexBinding(security))
                    {
                        //
                        // We only throw when we have 
                        // 1. a MutualCertificateDuplexBindingElement,
                        // 2. missing client certificate on the service side
                        // 3. The server will encrypt the response, or the message going from server to client
                        //
                        if (credentials.ClientCertificate.Certificate == null)
                        {
                            ProtectionLevel requestProtectionLevel;
                            ProtectionLevel responseProtectionLevel;
                            ContractProtectionRequirementsRule.GetRequiredProtectionLevels(endpoint.Contract, security.GetIndividualProperty<ISecurityCapabilities>().SupportedRequestProtectionLevel, security.GetIndividualProperty<ISecurityCapabilities>().SupportedResponseProtectionLevel,
                                out requestProtectionLevel, out responseProtectionLevel);

                            if (responseProtectionLevel == ProtectionLevel.EncryptAndSign)
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NoClientCertificate, endpoint.Binding.Name, endpoint.Binding.Namespace)));

                        }
                    }
                }
            }

            static public void Validate(ServiceDescription description)
            {
                //
                // Verify if the service credentials are not customized
                //
                if (!description.Behaviors.Contains(typeof(ServiceCredentials)))
                    return;

                ValidateCore(description, description.Behaviors.Find<ServiceCredentials>());
            }
        }

        static class UsernameImpersonationRule
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            static void ValidateCore(ServiceDescription description, ServiceCredentials credentials)
            {
                if (credentials.UserNameAuthentication.UserNamePasswordValidationMode == UserNamePasswordValidationMode.Windows)
                {
                    return;
                }

                ServiceAuthorizationBehavior behavior = description.Behaviors.Find<ServiceAuthorizationBehavior>();
                bool impersonateCallerForAllMethods = (behavior != null) ? behavior.ImpersonateCallerForAllOperations : false;
                for (int i = 0; i < description.Endpoints.Count; i++)
                {
                    ServiceEndpoint endpoint = description.Endpoints[i];
                    if (endpoint.InternalIsSystemEndpoint(description))
                    {
                        continue;
                    }

                    if (ValidatorUtils.IsStandardBinding(endpoint.Binding))
                    {
                        bool isImpersonationRequested = impersonateCallerForAllMethods;
                        if (!isImpersonationRequested)
                        {
                            isImpersonationRequested = ValidatorUtils.EndpointRequiresImpersonation(endpoint);
                        }
                        if (isImpersonationRequested)
                        {
                            ICollection<BindingElement> bindingElements = endpoint.Binding.CreateBindingElements();
                            foreach (BindingElement element in bindingElements)
                            {
                                SecurityBindingElement sbe = (element as SecurityBindingElement);
                                if (sbe != null)
                                {
                                    ValidateSecurityBindingElement(sbe, endpoint);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            static public void Validate(ServiceDescription description)
            {
                ServiceCredentials credentials = description.Behaviors.Find<ServiceCredentials>();
                if (credentials == null)
                    return;
                ValidateCore(description, credentials);
            }

            static private void ValidateSecurityBindingElement(SecurityBindingElement sbe, ServiceEndpoint endpoint)
            {
                if (sbe == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sbe");

                if (endpoint == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");

                foreach (SecurityTokenParameters stp in new SecurityTokenParametersEnumerable(sbe, true))
                {
                    if (stp is UserNameSecurityTokenParameters)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotPerformImpersonationOnUsernameToken, endpoint.Binding.Name, endpoint.Binding.Namespace, endpoint.Contract.Name, endpoint.Contract.Namespace)));
                    }
                    else if (stp is SecureConversationSecurityTokenParameters)
                    {
                        ValidateSecurityBindingElement(((SecureConversationSecurityTokenParameters)stp).BootstrapSecurityBindingElement, endpoint);
                    }
                }
            }

        }

        static class ValidatorUtils
        {
            static public bool EndpointRequiresImpersonation(ServiceEndpoint endpoint)
            {
                if (endpoint == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");

                for (int i = 0; i < endpoint.Contract.Operations.Count; ++i)
                {
                    OperationDescription operation = endpoint.Contract.Operations[i];
                    OperationBehaviorAttribute operationBehavior = operation.Behaviors.Find<OperationBehaviorAttribute>();
                    if (operationBehavior != null && (operationBehavior.Impersonation == ImpersonationOption.Required))
                    {
                        return true;
                    }
                }

                return false;
            }

            static public bool IsStandardBinding(Binding binding)
            {
                return (binding is BasicHttpBinding) ||
                    (binding is BasicHttpsBinding) ||
                    (binding is NetTcpBinding) ||
                    (binding is NetMsmqBinding) ||
                    (binding is NetNamedPipeBinding) ||
#pragma warning disable 0618
                    (binding is NetPeerTcpBinding) ||
#pragma warning restore 0618	                    
                    (binding is WSDualHttpBinding) ||
                    (binding is WSFederationHttpBinding) ||
                    (binding is WSHttpBinding) ||
                    (binding is NetHttpBinding) ||
                    (binding is NetHttpsBinding);
            }
        }

    }
}
