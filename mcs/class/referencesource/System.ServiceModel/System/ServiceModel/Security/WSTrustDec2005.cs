//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Security.Tokens;
    using HexBinary = System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Runtime.Serialization;
    using System.ServiceModel.Dispatcher;

    using KeyIdentifierEntry = WSSecurityTokenSerializer.KeyIdentifierEntry;
    using KeyIdentifierClauseEntry = WSSecurityTokenSerializer.KeyIdentifierClauseEntry;
    using TokenEntry = WSSecurityTokenSerializer.TokenEntry;
    using StrEntry = WSSecurityTokenSerializer.StrEntry;

    class WSTrustDec2005 : WSTrustFeb2005
    {
        public WSTrustDec2005(WSSecurityTokenSerializer tokenSerializer)
            : base(tokenSerializer)
        {
        }

        public override TrustDictionary SerializerDictionary
        {
            get { return DXD.TrustDec2005Dictionary; }
        }

        public class DriverDec2005 : WSTrustFeb2005.DriverFeb2005
        {
            public DriverDec2005(SecurityStandardsManager standardsManager)
                : base(standardsManager)
            {
            }

            public override TrustDictionary DriverDictionary
            {
                get
                {
                    return DXD.TrustDec2005Dictionary;
                }
            }

            public override XmlDictionaryString RequestSecurityTokenResponseFinalAction
            {
                get
                {
                    return DXD.TrustDec2005Dictionary.RequestSecurityTokenCollectionIssuanceFinalResponse;
                }
            }

            public override XmlElement CreateKeyTypeElement(SecurityKeyType keyType)
            {
                if (keyType == SecurityKeyType.BearerKey)
                {
                    XmlDocument doc = new XmlDocument();
                    XmlElement result = doc.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.KeyType.Value,
                        this.DriverDictionary.Namespace.Value);
                    result.AppendChild(doc.CreateTextNode(DXD.TrustDec2005Dictionary.BearerKeyType.Value));
                    return result;
                }

                return base.CreateKeyTypeElement(keyType);
            }

            public override bool TryParseKeyTypeElement(XmlElement element, out SecurityKeyType keyType)
            {
                if (element == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");

                if (element.LocalName == this.DriverDictionary.KeyType.Value
                    && element.NamespaceURI == this.DriverDictionary.Namespace.Value
                    && element.InnerText == DXD.TrustDec2005Dictionary.BearerKeyType.Value)
                {
                    keyType = SecurityKeyType.BearerKey;
                    return true;
                }

                return base.TryParseKeyTypeElement(element, out keyType);
            }

            public override XmlElement CreateRequiredClaimsElement(IEnumerable<XmlElement> claimsList)
            {
                XmlElement result = base.CreateRequiredClaimsElement(claimsList);
                XmlAttribute dialectAttribute = result.OwnerDocument.CreateAttribute(DXD.TrustDec2005Dictionary.Dialect.Value);
                dialectAttribute.Value = DXD.TrustDec2005Dictionary.DialectType.Value;
                result.Attributes.Append(dialectAttribute);

                return result;
            }

            public override IChannelFactory<IRequestChannel> CreateFederationProxy(EndpointAddress address, Binding binding, KeyedByTypeCollection<IEndpointBehavior> channelBehaviors)
            {
                if (channelBehaviors == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelBehaviors");

                ChannelFactory<IWsTrustDec2005SecurityTokenService> result = new ChannelFactory<IWsTrustDec2005SecurityTokenService>(binding, address);
                SetProtectionLevelForFederation(result.Endpoint.Contract.Operations);
                // remove the default client credentials that gets added to channel factories
                result.Endpoint.Behaviors.Remove<ClientCredentials>();
                for (int i = 0; i < channelBehaviors.Count; ++i)
                {
                    result.Endpoint.Behaviors.Add(channelBehaviors[i]);
                }
                // add a behavior that removes the UI channel initializer added by the client credentials since there should be no UI
                // initializer popped up as part of obtaining the federation token (the UI should already have been popped up for the main channel)
                result.Endpoint.Behaviors.Add(new WSTrustFeb2005.DriverFeb2005.InteractiveInitializersRemovingBehavior());

                return new WSTrustFeb2005.DriverFeb2005.RequestChannelFactory<IWsTrustDec2005SecurityTokenService>(result);
            }

            public override Collection<XmlElement> ProcessUnknownRequestParameters(Collection<XmlElement> unknownRequestParameters, Collection<XmlElement> originalRequestParameters)
            {
                // For WS-Trust 1.3 we want everything in the requestSecurityTokenTemplate parameters to endup as Addtional parameters.
                // The parameters will appear as a child element under a XmlElement named secondaryParameters.
                if (originalRequestParameters == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("originalRequestParameters");

                if (originalRequestParameters.Count > 0 && originalRequestParameters[0] != null && originalRequestParameters[0].OwnerDocument != null)
                {
                    XmlElement secondaryParamElement = originalRequestParameters[0].OwnerDocument.CreateElement(DXD.TrustDec2005Dictionary.Prefix.Value, DXD.TrustDec2005Dictionary.SecondaryParameters.Value, DXD.TrustDec2005Dictionary.Namespace.Value);
                    for (int i = 0; i < originalRequestParameters.Count; ++i)
                    {
                        secondaryParamElement.AppendChild(originalRequestParameters[i]);
                    }

                    Collection<XmlElement> tempCollection = new Collection<XmlElement>();
                    tempCollection.Add(secondaryParamElement);
                    return tempCollection;
                }

                return originalRequestParameters;
            }

            internal virtual bool IsSecondaryParametersElement(XmlElement element)
            {
                return ((element.LocalName == DXD.TrustDec2005Dictionary.SecondaryParameters.Value) &&
                        (element.NamespaceURI == DXD.TrustDec2005Dictionary.Namespace.Value));
            }

            public virtual XmlElement CreateKeyWrapAlgorithmElement(string keyWrapAlgorithm)
            {
                if (keyWrapAlgorithm == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyWrapAlgorithm");
                }
                XmlDocument doc = new XmlDocument();
                XmlElement result = doc.CreateElement(DXD.TrustDec2005Dictionary.Prefix.Value, DXD.TrustDec2005Dictionary.KeyWrapAlgorithm.Value,
                    DXD.TrustDec2005Dictionary.Namespace.Value);
                result.AppendChild(doc.CreateTextNode(keyWrapAlgorithm));
                return result;
            }

            internal override bool IsKeyWrapAlgorithmElement(XmlElement element, out string keyWrapAlgorithm)
            {
                return CheckElement(element, DXD.TrustDec2005Dictionary.KeyWrapAlgorithm.Value, DXD.TrustDec2005Dictionary.Namespace.Value, out keyWrapAlgorithm);
            }

            [ServiceContract]
            internal interface IWsTrustDec2005SecurityTokenService
            {
                [OperationContract(IsOneWay = false,
                                   Action = TrustDec2005Strings.RequestSecurityTokenIssuance,
                                   ReplyAction = TrustDec2005Strings.RequestSecurityTokenCollectionIssuanceFinalResponse)]
                [FaultContract(typeof(string), Action = "*", ProtectionLevel = System.Net.Security.ProtectionLevel.Sign)]
                Message RequestToken(Message message);
            }
        }
    }
}
