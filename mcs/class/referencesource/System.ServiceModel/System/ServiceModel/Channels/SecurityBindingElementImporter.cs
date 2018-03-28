//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.ServiceModel.Diagnostics;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Configuration;
using System.Collections.ObjectModel;
using System.Net.Security;

namespace System.ServiceModel.Channels
{
    using System.Xml;
    using System.ServiceModel.Security.Tokens;
    using System.ServiceModel.Security;
    using System.ServiceModel;
    using System.IO;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class SecurityBindingElementImporter : IPolicyImportExtension
    {
        internal const string MaxPolicyRedirectionsKey = "MaxPolicyRedirections";
        internal const string SecureConversationBootstrapEncryptionRequirements = "SecureConversationBootstrapEncryptionRequirements";
        internal const string SecureConversationBootstrapSignatureRequirements = "SecureConversationBootstrapSignatureRequirements";
        internal const string InSecureConversationBootstrapBindingImportMode = "InSecureConversationBootstrapBindingImportMode";
        internal const string ContractProtectionLevelKey = "ContractProtectionLevelKey";

        int maxPolicyRedirections;

        public SecurityBindingElementImporter()
        {
            this.maxPolicyRedirections = 10;
        }

        public int MaxPolicyRedirections
        {
            get
            {
                return this.maxPolicyRedirections;
            }
        }

        void ImportOperationScopeSupportingTokensPolicy(MetadataImporter importer, PolicyConversionContext policyContext, SecurityBindingElement binding)
        {
            foreach (OperationDescription operation in policyContext.Contract.Operations)
            {
                string requestAction = null;
                foreach (MessageDescription message in operation.Messages)
                {
                    if (message.Direction == MessageDirection.Input)
                    {
                        requestAction = message.Action;
                        break;
                    }
                }

                SupportingTokenParameters requirements = new SupportingTokenParameters();
                SupportingTokenParameters optionalRequirements = new SupportingTokenParameters();
                ICollection<XmlElement> operationBindingAssertions = policyContext.GetOperationBindingAssertions(operation);
                this.ImportSupportingTokenAssertions(importer, policyContext, operationBindingAssertions, requirements, optionalRequirements);
                if (requirements.Endorsing.Count > 0
                    || requirements.Signed.Count > 0
                    || requirements.SignedEncrypted.Count > 0
                    || requirements.SignedEndorsing.Count > 0)
                {
                    if (requestAction != null)
                    {
                        binding.OperationSupportingTokenParameters[requestAction] = requirements;
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotImportSupportingTokensForOperationWithoutRequestAction)));
                    }
                }
                if (optionalRequirements.Endorsing.Count > 0
                    || optionalRequirements.Signed.Count > 0
                    || optionalRequirements.SignedEncrypted.Count > 0
                    || optionalRequirements.SignedEndorsing.Count > 0)
                {
                    if (requestAction != null)
                    {
                        binding.OptionalOperationSupportingTokenParameters[requestAction] = optionalRequirements;
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotImportSupportingTokensForOperationWithoutRequestAction)));
                    }
                }
            }
        }

        void ImportProtectionAssertions(ICollection<XmlElement> assertions, out MessagePartSpecification signedParts, out MessagePartSpecification encryptedParts)
        {
            XmlElement assertion;

            signedParts = null;
            encryptedParts = null;

            WSSecurityPolicy securityPolicy;
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(assertions, out securityPolicy))
            {
                if (!securityPolicy.TryImportWsspEncryptedPartsAssertion(assertions, out encryptedParts, out assertion)
                    && assertion != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedSecurityPolicyAssertion, assertion.OuterXml)));
                }

                if (!securityPolicy.TryImportWsspSignedPartsAssertion(assertions, out signedParts, out assertion)
                    && assertion != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedSecurityPolicyAssertion, assertion.OuterXml)));
                }
            }

            if (encryptedParts == null)
            {
                encryptedParts = MessagePartSpecification.NoParts;
            }
            if (signedParts == null)
            {
                signedParts = MessagePartSpecification.NoParts;
            }
        }

        void ValidateExistingOrSetNewProtectionLevel(MessagePartDescription part, MessageDescription message, OperationDescription operation, ContractDescription contract, ProtectionLevel newProtectionLevel)
        {
            ProtectionLevel existingProtectionLevel;

            if (part != null && part.HasProtectionLevel)
            {
                existingProtectionLevel = part.ProtectionLevel;
            }
            else if (message.HasProtectionLevel)
            {
                existingProtectionLevel = message.ProtectionLevel;
            }
            else if (operation.HasProtectionLevel)
            {
                existingProtectionLevel = operation.ProtectionLevel;
            }
            else
            {
                if (part != null)
                {
                    part.ProtectionLevel = newProtectionLevel;
                }
                else
                {
                    message.ProtectionLevel = newProtectionLevel;
                }
                existingProtectionLevel = newProtectionLevel;
            }

            if (existingProtectionLevel != newProtectionLevel)
            {
                if (part != null && !part.HasProtectionLevel)
                {
                    part.ProtectionLevel = newProtectionLevel;
                }
                else if (part == null && !message.HasProtectionLevel)
                {
                    message.ProtectionLevel = newProtectionLevel;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.CannotImportProtectionLevelForContract, contract.Name, contract.Namespace)));
                }
            }
        }

        void AddParts(ref MessagePartSpecification parts1, MessagePartSpecification parts2)
        {
            if (parts1 == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parts1"));
            if (parts2 == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parts2"));

            if (!parts2.IsEmpty())
            {
                if (parts1.IsReadOnly)
                {
                    MessagePartSpecification p = new MessagePartSpecification();
                    p.Union(parts1);
                    p.Union(parts2);
                    parts1 = p;
                }
                else
                {
                    parts1.Union(parts2);
                }
            }
        }

        class ContractProtectionLevel
        {
            bool hasProtectionRequirements;
            bool hasUniformProtectionLevel;
            ProtectionLevel uniformProtectionLevel;

            public ContractProtectionLevel(bool hasProtectionRequirements, bool hasUniformProtectionLevel, ProtectionLevel uniformProtectionLevel)
            {
                this.hasProtectionRequirements = hasProtectionRequirements;
                this.hasUniformProtectionLevel = hasUniformProtectionLevel;
                this.uniformProtectionLevel = uniformProtectionLevel;
            }

            public bool HasProtectionRequirements { get { return this.hasProtectionRequirements; } }
            public bool HasUniformProtectionLevel { get { return this.hasUniformProtectionLevel; } }
            public ProtectionLevel UniformProtectionLevel { get { return this.uniformProtectionLevel; } }
        }

        void ImportMessageScopeProtectionPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            MessagePartSpecification endpointSignedParts;
            MessagePartSpecification endpointEncryptedParts;
            bool isContractAssociatedWithAtLeastOneOtherBinding;
            ContractProtectionLevel otherBindingProtectionLevel = null;
            bool hasContractProtectionLevel = false;
            bool isContractProtectionLevelUniform = true;
            ProtectionLevel contractProtectionLevel = ProtectionLevel.None;

            string contractAssociationName = String.Format("{0}:{1}:{2}", ContractProtectionLevelKey, policyContext.Contract.Name, policyContext.Contract.Namespace);
            if (importer.State.ContainsKey(contractAssociationName))
            {
                isContractAssociatedWithAtLeastOneOtherBinding = true;
                otherBindingProtectionLevel = (ContractProtectionLevel)importer.State[contractAssociationName];
            }
            else
            {
                isContractAssociatedWithAtLeastOneOtherBinding = false;
            }

            ICollection<XmlElement> endpointBindingAssertions = policyContext.GetBindingAssertions();
            this.ImportProtectionAssertions(endpointBindingAssertions, out endpointSignedParts, out endpointEncryptedParts);

            if (importer.State.ContainsKey(InSecureConversationBootstrapBindingImportMode))
            {
                // when importing secure conversation boostrap binding, add the endpoint scope protection requirements
                // to the importer state to be consumed in SecurityPolicy11.TryImportWsspBootrstapPolicyAssertion
                if (endpointEncryptedParts != null)
                    importer.State[SecureConversationBootstrapEncryptionRequirements] = endpointEncryptedParts;
                if (endpointSignedParts != null)
                    importer.State[SecureConversationBootstrapSignatureRequirements] = endpointSignedParts;
            }

            foreach (OperationDescription operation in policyContext.Contract.Operations)
            {
                MessagePartSpecification operationSignedParts;
                MessagePartSpecification operationEncryptedParts;

                ICollection<XmlElement> operationBindingAssertions = policyContext.GetOperationBindingAssertions(operation);
                this.ImportProtectionAssertions(operationBindingAssertions, out operationSignedParts, out operationEncryptedParts);
                this.AddParts(ref operationSignedParts, endpointSignedParts);
                this.AddParts(ref operationEncryptedParts, endpointEncryptedParts);

                MessagePartSpecification messageSignedParts;
                MessagePartSpecification messageEncryptedParts;
                bool hasProtectionLevel = false;
                bool isProtectionLevelUniform = true;
                ProtectionLevel protectionLevel = ProtectionLevel.None;

                // import application message protection requirements
                foreach (MessageDescription message in operation.Messages)
                {
                    ICollection<XmlElement> messageBindingAssertions = policyContext.GetMessageBindingAssertions(message);
                    this.ImportProtectionAssertions(messageBindingAssertions, out messageSignedParts, out messageEncryptedParts);
                    this.AddParts(ref messageSignedParts, operationSignedParts);
                    this.AddParts(ref messageEncryptedParts, operationEncryptedParts);

                    // validate or set body protection level
                    ProtectionLevel newProtectionLevel = GetProtectionLevel(messageSignedParts.IsBodyIncluded, messageEncryptedParts.IsBodyIncluded, message.Action);
                    if (OperationFormatter.IsValidReturnValue(message.Body.ReturnValue))
                    {
                        ValidateExistingOrSetNewProtectionLevel(message.Body.ReturnValue, message, operation, policyContext.Contract, newProtectionLevel);
                    }
                    foreach (MessagePartDescription body in message.Body.Parts)
                    {
                        ValidateExistingOrSetNewProtectionLevel(body, message, operation, policyContext.Contract, newProtectionLevel);
                    }
                    if (!OperationFormatter.IsValidReturnValue(message.Body.ReturnValue) || message.Body.Parts.Count == 0)
                    {
                        ValidateExistingOrSetNewProtectionLevel(null, message, operation, policyContext.Contract, newProtectionLevel);
                    }

                    if (hasProtectionLevel)
                    {
                        if (protectionLevel != newProtectionLevel)
                        {
                            isProtectionLevelUniform = false;
                        }
                    }
                    else
                    {
                        protectionLevel = newProtectionLevel;
                        hasProtectionLevel = true;
                    }
                    if (hasContractProtectionLevel)
                    {
                        if (contractProtectionLevel != newProtectionLevel)
                        {
                            isContractProtectionLevelUniform = false;
                        }
                    }
                    else
                    {
                        contractProtectionLevel = newProtectionLevel;
                        hasContractProtectionLevel = true;
                    }

                    // validate o set header protection level
                    foreach (MessageHeaderDescription header in message.Headers)
                    {
                        bool signed = messageSignedParts.IsHeaderIncluded(header.Name, header.Namespace);
                        bool encrypted = messageEncryptedParts.IsHeaderIncluded(header.Name, header.Namespace);
                        newProtectionLevel = GetProtectionLevel(signed, encrypted, message.Action);
                        ValidateExistingOrSetNewProtectionLevel(header, message, operation, policyContext.Contract, newProtectionLevel);

                        if (hasProtectionLevel)
                        {
                            if (protectionLevel != newProtectionLevel)
                            {
                                isProtectionLevelUniform = false;
                            }
                        }
                        else
                        {
                            protectionLevel = newProtectionLevel;
                            hasProtectionLevel = true;
                        }
                        if (hasContractProtectionLevel)
                        {
                            if (contractProtectionLevel != newProtectionLevel)
                            {
                                isContractProtectionLevelUniform = false;
                            }
                        }
                        else
                        {
                            contractProtectionLevel = newProtectionLevel;
                            hasContractProtectionLevel = true;
                        }
                    }
                }

                // normalize protection level settings at the operation scope if possible to help avoid typed message generation
                if (hasProtectionLevel && isProtectionLevelUniform)
                {
                    // (Microsoft) remove the foreach message here
                    //  foreach (MessageDescription message in operation.Messages)

                    this.ResetProtectionLevelForMessages(operation);

                    operation.ProtectionLevel = protectionLevel;
                }

                // import fault protection requirements
                foreach (FaultDescription fault in operation.Faults)
                {
                    ICollection<XmlElement> faultBindingAssertions = policyContext.GetFaultBindingAssertions(fault);
                    this.ImportProtectionAssertions(faultBindingAssertions, out messageSignedParts, out messageEncryptedParts);
                    this.AddParts(ref messageSignedParts, operationSignedParts);
                    this.AddParts(ref messageEncryptedParts, operationEncryptedParts);

                    // validate or set fault protection level
                    ProtectionLevel newProtectionLevel = GetProtectionLevel(messageSignedParts.IsBodyIncluded, messageEncryptedParts.IsBodyIncluded, fault.Action);
                    if (fault.HasProtectionLevel)
                    {
                        if (fault.ProtectionLevel != newProtectionLevel)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.CannotImportProtectionLevelForContract, policyContext.Contract.Name, policyContext.Contract.Namespace)));
                        }
                    }
                    else
                    {
                        fault.ProtectionLevel = newProtectionLevel;
                    }
                    if (hasContractProtectionLevel)
                    {
                        if (contractProtectionLevel != newProtectionLevel)
                        {
                            isContractProtectionLevelUniform = false;
                        }
                    }
                    else
                    {
                        contractProtectionLevel = newProtectionLevel;
                        hasContractProtectionLevel = true;
                    }
                }
            }

            if (isContractAssociatedWithAtLeastOneOtherBinding)
            {
                if (hasContractProtectionLevel != otherBindingProtectionLevel.HasProtectionRequirements
                    || isContractProtectionLevelUniform != otherBindingProtectionLevel.HasUniformProtectionLevel
                    || contractProtectionLevel != otherBindingProtectionLevel.UniformProtectionLevel)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.CannotImportProtectionLevelForContract, policyContext.Contract.Name, policyContext.Contract.Namespace)));
                }
            }
            else
            {
                if (hasContractProtectionLevel && isContractProtectionLevelUniform && contractProtectionLevel == ProtectionLevel.EncryptAndSign)
                {
                    // remove all explicitly set protection levels on the contract description, since they are uniform across the contract
                    // and match our binding's default of EncryptAndSign
                    foreach (OperationDescription operation in policyContext.Contract.Operations)
                    {
                        this.ResetProtectionLevelForMessages(operation);
                        foreach (FaultDescription fault in operation.Faults)
                        {
                            fault.ResetProtectionLevel();
                        }
                        operation.ResetProtectionLevel();
                    }
                }
                importer.State[contractAssociationName] = new ContractProtectionLevel(hasContractProtectionLevel, isContractProtectionLevelUniform, contractProtectionLevel);
            }
        }

        void ResetProtectionLevelForMessages(OperationDescription operation)
        {
            foreach (MessageDescription message in operation.Messages)
            {
                if (OperationFormatter.IsValidReturnValue(message.Body.ReturnValue))
                {
                    message.Body.ReturnValue.ResetProtectionLevel();
                }
                foreach (MessagePartDescription body in message.Body.Parts)
                {
                    body.ResetProtectionLevel();
                }
                foreach (MessageHeaderDescription header in message.Headers)
                {
                    header.ResetProtectionLevel();
                }
                message.ResetProtectionLevel();
            }
        }

        static ProtectionLevel GetProtectionLevel(bool signed, bool encrypted, string action)
        {
            ProtectionLevel result;

            if (encrypted)
            {
                if (signed)
                {
                    result = ProtectionLevel.EncryptAndSign;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(SR.GetString(SR.PolicyRequiresConfidentialityWithoutIntegrity, action)));
                }
            }
            else if (signed)
            {
                result = ProtectionLevel.Sign;
            }
            else
            {
                result = ProtectionLevel.None;
            }

            return result;
        }

        void ImportSupportingTokenAssertions(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, SupportingTokenParameters requirements, SupportingTokenParameters optionalRequirements)
        {
            WSSecurityPolicy securityPolicy;
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(assertions, out securityPolicy))
            {
                securityPolicy.TryImportWsspSupportingTokensAssertion(
                importer,
                policyContext,
                assertions,
                requirements.Signed,
                requirements.SignedEncrypted,
                requirements.Endorsing,
                requirements.SignedEndorsing,
                optionalRequirements.Signed,
                optionalRequirements.SignedEncrypted,
                optionalRequirements.Endorsing,
                optionalRequirements.SignedEndorsing);
            }
        }

        void ImportEndpointScopeMessageBindingAssertions(MetadataImporter importer, PolicyConversionContext policyContext, SecurityBindingElement binding)
        {
            XmlElement assertion = null;

            this.ImportSupportingTokenAssertions(importer, policyContext, policyContext.GetBindingAssertions(), binding.EndpointSupportingTokenParameters, binding.OptionalEndpointSupportingTokenParameters);

            WSSecurityPolicy securityPolicy;
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(policyContext.GetBindingAssertions(), out securityPolicy))
            {
                if (!securityPolicy.TryImportWsspWssAssertion(importer, policyContext.GetBindingAssertions(), binding, out assertion)
                    && assertion != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedSecurityPolicyAssertion, assertion.OuterXml)));
                }

                if (!securityPolicy.TryImportWsspTrustAssertion(importer, policyContext.GetBindingAssertions(), binding, out assertion)
                    && assertion != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedSecurityPolicyAssertion, assertion.OuterXml)));
                }
            }

            //
            // We don't have WSTrust assertion => it is possible we are a BasicHttpBinding
            // Set the flag here so that later when we tried to compare binding element with basic http binding
            // we can have an exact match.
            //
            if (assertion == null)
                binding.DoNotEmitTrust = true;
        }


        bool TryImportSymmetricSecurityBindingElement(MetadataImporter importer, PolicyConversionContext policyContext, out SecurityBindingElement sbe)
        {
            SymmetricSecurityBindingElement binding = null;
            XmlElement assertion;
            WSSecurityPolicy securityPolicy;
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(policyContext.GetBindingAssertions(), out securityPolicy))
            {
                if (securityPolicy.TryImportWsspSymmetricBindingAssertion(importer, policyContext, policyContext.GetBindingAssertions(), out binding, out assertion))
                {
                    this.ImportEndpointScopeMessageBindingAssertions(importer, policyContext, binding);
                    this.ImportOperationScopeSupportingTokensPolicy(importer, policyContext, binding);
                    this.ImportMessageScopeProtectionPolicy(importer, policyContext);
                    policyContext.BindingElements.Add(binding);
                }
                else if (assertion != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedSecurityPolicyAssertion, assertion.OuterXml)));
                }
            }

            sbe = binding;
            return binding != null;
        }

        bool TryImportAsymmetricSecurityBindingElement(MetadataImporter importer, PolicyConversionContext policyContext, out SecurityBindingElement sbe)
        {
            AsymmetricSecurityBindingElement binding = null;
            XmlElement assertion;
            WSSecurityPolicy securityPolicy;
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(policyContext.GetBindingAssertions(), out securityPolicy))
            {
                if (securityPolicy.TryImportWsspAsymmetricBindingAssertion(importer, policyContext, policyContext.GetBindingAssertions(), out binding, out assertion))
                {
                    this.ImportEndpointScopeMessageBindingAssertions(importer, policyContext, binding);
                    this.ImportOperationScopeSupportingTokensPolicy(importer, policyContext, binding);
                    this.ImportMessageScopeProtectionPolicy(importer, policyContext);
                    policyContext.BindingElements.Add(binding);
                }
                else if (assertion != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedSecurityPolicyAssertion, assertion.OuterXml)));
                }
            }

            sbe = binding;
            return binding != null;
        }

        // isDualSecurityModeOnly is true if the binding has both message security and https security enabled.
        bool TryImportTransportSecurityBindingElement(MetadataImporter importer, PolicyConversionContext policyContext, out SecurityBindingElement sbe, bool isDualSecurityModeOnly)
        {
            TransportSecurityBindingElement binding = null;
            XmlElement assertion;
            sbe = null;

            WSSecurityPolicy securityPolicy;
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(policyContext.GetBindingAssertions(), out securityPolicy))
            {
                if (securityPolicy.TryImportWsspTransportBindingAssertion(importer, policyContext.GetBindingAssertions(), out binding, out assertion))
                {
                    this.ImportEndpointScopeMessageBindingAssertions(importer, policyContext, binding);

                    // If it is not DualSecurityMode then it is Mixed mode. So we need to look for supporting tokens in the binding.
                    if (!isDualSecurityModeOnly)
                    {
                        this.ImportOperationScopeSupportingTokensPolicy(importer, policyContext, binding);
                        if (importer.State.ContainsKey(InSecureConversationBootstrapBindingImportMode))
                        {
                            this.ImportMessageScopeProtectionPolicy(importer, policyContext);
                        }

                        if (HasSupportingTokens(binding) || binding.IncludeTimestamp)
                        {
                            sbe = binding;
                            policyContext.BindingElements.Add(binding);
                        }
                    }
                }
                else if (assertion != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedSecurityPolicyAssertion, assertion.OuterXml)));
                }
            }

            return binding != null;
        }


        static bool HasSupportingTokens(SecurityBindingElement binding)
        {
            if (binding.EndpointSupportingTokenParameters.Endorsing.Count > 0
                    || binding.EndpointSupportingTokenParameters.SignedEndorsing.Count > 0
                    || binding.EndpointSupportingTokenParameters.SignedEncrypted.Count > 0
                    || binding.EndpointSupportingTokenParameters.Signed.Count > 0)
            {
                return true;
            }

            foreach (SupportingTokenParameters r in binding.OperationSupportingTokenParameters.Values)
            {
                if (r.Endorsing.Count > 0
                        || r.SignedEndorsing.Count > 0
                        || r.SignedEncrypted.Count > 0
                        || r.Signed.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            if (importer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("importer");
            }
            if (policyContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policyContext");
            }
            WSSecurityPolicy securityPolicy;
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(policyContext.GetBindingAssertions(), out securityPolicy))
            {
                if ((importer.State != null) && (!importer.State.ContainsKey(MaxPolicyRedirectionsKey)))
                {
                    importer.State.Add(MaxPolicyRedirectionsKey, this.MaxPolicyRedirections);
                }

                SecurityBindingElement sbe = null;
                bool success = this.TryImportSymmetricSecurityBindingElement(importer, policyContext, out sbe);
                if (!success)
                {
                    success = this.TryImportAsymmetricSecurityBindingElement(importer, policyContext, out sbe);
                }
                if (!success)
                {
                    success = this.TryImportTransportSecurityBindingElement(importer, policyContext, out sbe, false);
                }
                else
                {
                    // We already have found and imported the message security binding element above. Hence this could be the dual mode security.
                    // Now let us see if there is HttpsTransportBinding assertion also below it .This is to avoid the 
                    // warning messages while importing wsdl representing the message security over Https transport security scenario. See Bug:136416.

                    SecurityBindingElement tbe = null;
                    this.TryImportTransportSecurityBindingElement(importer, policyContext, out tbe, true);
                }

                if (sbe != null)
                {
                    SecurityElement config = new SecurityElement();
                    config.InitializeFrom(sbe, false);
                    if (config.HasImportFailed)
                    {
#pragma warning suppress 56506
                        importer.Errors.Add(new MetadataConversionError(SR.GetString(SR.SecurityBindingElementCannotBeExpressedInConfig), true));
                    }
                }
            }
        }
    }
}
