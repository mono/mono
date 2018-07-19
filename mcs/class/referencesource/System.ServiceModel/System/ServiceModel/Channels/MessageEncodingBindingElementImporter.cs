//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel.Description;
    using System.Xml;
    using System.Xml.Schema;
    using System.Web.Services.Description;

    public class MessageEncodingBindingElementImporter : IWsdlImportExtension, IPolicyImportExtension
    {

        void IWsdlImportExtension.BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
        }

        void IWsdlImportExtension.ImportContract(WsdlImporter importer, WsdlContractConversionContext context) { }
        void IWsdlImportExtension.ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

#pragma warning suppress 56506 // Microsoft, these properties cannot be null in this context
            if (context.Endpoint.Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context.Endpoint.Binding");
            }

            BindingElementCollection bindingElements = GetBindingElements(context);
            MessageEncodingBindingElement messageEncodingBindingElement = bindingElements.Find<MessageEncodingBindingElement>();
            TextMessageEncodingBindingElement textEncodingBindingElement = messageEncodingBindingElement as TextMessageEncodingBindingElement;

            if (messageEncodingBindingElement != null)
            {
                Type elementType = messageEncodingBindingElement.GetType();
                if (elementType != typeof(TextMessageEncodingBindingElement)
                    && elementType != typeof(BinaryMessageEncodingBindingElement)
                    && elementType != typeof(MtomMessageEncodingBindingElement))
                    return;
            }

            EnsureMessageEncoding(context, messageEncodingBindingElement);

            foreach (OperationBinding wsdlOperationBinding in context.WsdlBinding.Operations)
            {
                OperationDescription operation = context.GetOperationDescription(wsdlOperationBinding);

                for (int i = 0; i < operation.Messages.Count; i++)
                {
                    MessageDescription message = operation.Messages[i];
                    MessageBinding wsdlMessageBinding = context.GetMessageBinding(message);
                    ImportMessageSoapAction(context.ContractConversionContext, message, wsdlMessageBinding, i != 0 /*isResponse*/);
                }

                foreach (FaultDescription fault in operation.Faults)
                {
                    FaultBinding wsdlFaultBinding = context.GetFaultBinding(fault);
                    if (wsdlFaultBinding != null)
                    {
                        ImportFaultSoapAction(context.ContractConversionContext, fault, wsdlFaultBinding);
                    }
                }
            }

        }

        static void ImportFaultSoapAction(WsdlContractConversionContext contractContext, FaultDescription fault, FaultBinding wsdlFaultBinding)
        {
            string soapAction = SoapHelper.ReadSoapAction(wsdlFaultBinding.OperationBinding);

            if (contractContext != null)
            {
                OperationFault wsdlOperationFault = contractContext.GetOperationFault(fault);
                string wsaAction = WsdlImporter.WSAddressingHelper.FindWsaActionAttribute(wsdlOperationFault);
                if (wsaAction == null && soapAction != null)
                    fault.Action = soapAction;
                //

            }
            else
            {
                //
            }
        }

        static void ImportMessageSoapAction(WsdlContractConversionContext contractContext, MessageDescription message, MessageBinding wsdlMessageBinding, bool isResponse)
        {
            string soapAction = SoapHelper.ReadSoapAction(wsdlMessageBinding.OperationBinding);

            if (contractContext != null)
            {
                OperationMessage wsdlOperationMessage = contractContext.GetOperationMessage(message);
                string wsaAction = WsdlImporter.WSAddressingHelper.FindWsaActionAttribute(wsdlOperationMessage);
                if (wsaAction == null && soapAction != null)
                {
                    if (isResponse)
                    {
                        message.Action = "*";
                    }
                    else
                    {
                        message.Action = soapAction;
                    }
                }
                //

            }
            else
            {
                //
            }
        }

        static void EnsureMessageEncoding(WsdlEndpointConversionContext context, MessageEncodingBindingElement encodingBindingElement)
        {
            EnvelopeVersion soapVersion = SoapHelper.GetSoapVersion(context.WsdlBinding);
            AddressingVersion addressingVersion;

            if (encodingBindingElement == null)
            {
                encodingBindingElement = new TextMessageEncodingBindingElement();
                ConvertToCustomBinding(context).Elements.Add(encodingBindingElement);

                addressingVersion = AddressingVersion.None;
            }
            else
            {
                if (soapVersion == EnvelopeVersion.None)
                    addressingVersion = AddressingVersion.None;
                else
                    addressingVersion = encodingBindingElement.MessageVersion.Addressing;
            }

            MessageVersion newMessageVersion = MessageVersion.CreateVersion(soapVersion, addressingVersion);
            if (!encodingBindingElement.MessageVersion.IsMatch(newMessageVersion))
            {
                ConvertToCustomBinding(context).Elements.Find<MessageEncodingBindingElement>().MessageVersion
                    = MessageVersion.CreateVersion(soapVersion, addressingVersion);
            }
        }

        static BindingElementCollection GetBindingElements(WsdlEndpointConversionContext context)
        {
            Binding binding = context.Endpoint.Binding;
            BindingElementCollection elements = binding is CustomBinding ? ((CustomBinding)binding).Elements : binding.CreateBindingElements();
            return elements;
        }

        static CustomBinding ConvertToCustomBinding(WsdlEndpointConversionContext context)
        {
            CustomBinding customBinding = context.Endpoint.Binding as CustomBinding;
            if (customBinding == null)
            {
                customBinding = new CustomBinding(context.Endpoint.Binding);
                context.Endpoint.Binding = customBinding;
            }
            return customBinding;
        }


        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            if (importer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("importer");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            ImportPolicyInternal(context);
        }

        void ImportPolicyInternal(PolicyConversionContext context)
        {
            ICollection<XmlElement> assertions = context.GetBindingAssertions();

            XmlElement encodingAssertion;
            MessageEncodingBindingElement encodingBindingElement;
            encodingBindingElement = CreateEncodingBindingElement(context.GetBindingAssertions(), out encodingAssertion);

            AddressingVersion addressingVersion = WsdlImporter.WSAddressingHelper.FindAddressingVersion(context);
            ApplyAddressingVersion(encodingBindingElement, addressingVersion);


#pragma warning suppress 56506
            context.BindingElements.Add(encodingBindingElement);
        }

        static void ApplyAddressingVersion(MessageEncodingBindingElement encodingBindingElement, AddressingVersion addressingVersion)
        {
            EnvelopeVersion defaultEnvelopeVersion = encodingBindingElement.MessageVersion.Envelope;

            if (defaultEnvelopeVersion == EnvelopeVersion.None
                && addressingVersion != AddressingVersion.None)
            {
                // The default envelope version is None which incompatible with the 
                // addressing version.
                // We replace it with soap12. This will be updated at wsdl import time if necessary.
                encodingBindingElement.MessageVersion = MessageVersion.CreateVersion(EnvelopeVersion.Soap12, addressingVersion);
            }
            else
            {
                encodingBindingElement.MessageVersion = MessageVersion.CreateVersion(defaultEnvelopeVersion, addressingVersion);
            }
        }

        MessageEncodingBindingElement CreateEncodingBindingElement(ICollection<XmlElement> assertions, out XmlElement encodingAssertion)
        {
            encodingAssertion = null;
            foreach (XmlElement assertion in assertions)
            {
                switch (assertion.NamespaceURI)
                {
                    case MessageEncodingPolicyConstants.BinaryEncodingNamespace:
                        if (assertion.LocalName == MessageEncodingPolicyConstants.BinaryEncodingName)
                        {
                            encodingAssertion = assertion;
                            assertions.Remove(encodingAssertion);
                            return new BinaryMessageEncodingBindingElement();
                        }
                        break;
                    case MessageEncodingPolicyConstants.OptimizedMimeSerializationNamespace:
                        if (assertion.LocalName == MessageEncodingPolicyConstants.MtomEncodingName)
                        {
                            encodingAssertion = assertion;
                            assertions.Remove(encodingAssertion);
                            return new MtomMessageEncodingBindingElement();
                        }
                        break;
                }
            }

            return new TextMessageEncodingBindingElement();
        }
    }

    static class MessageEncodingPolicyConstants
    {
        public const string BinaryEncodingName = "BinaryEncoding";
        public const string BinaryEncodingNamespace = "http://schemas.microsoft.com/ws/06/2004/mspolicy/netbinary1";
        public const string BinaryEncodingPrefix = "msb";
        public const string OptimizedMimeSerializationNamespace = "http://schemas.xmlsoap.org/ws/2004/09/policy/optimizedmimeserialization";
        public const string OptimizedMimeSerializationPrefix = "wsoma";
        public const string MtomEncodingName = "OptimizedMimeSerialization";
    }
}

