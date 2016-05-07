//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using WsdlNS = System.Web.Services.Description;

    public class WsdlExporter : MetadataExporter
    {
        static XmlDocument xmlDocument;
        bool isFaulted = false;


        WsdlNS.ServiceDescriptionCollection wsdlDocuments = new WsdlNS.ServiceDescriptionCollection();
        XmlSchemaSet xmlSchemas = WsdlExporter.GetEmptySchemaSet();

        Dictionary<ContractDescription, WsdlContractConversionContext> exportedContracts
            = new Dictionary<ContractDescription, WsdlContractConversionContext>();
        Dictionary<BindingDictionaryKey, WsdlEndpointConversionContext> exportedBindings = new Dictionary<BindingDictionaryKey, WsdlEndpointConversionContext>();
        Dictionary<EndpointDictionaryKey, ServiceEndpoint> exportedEndpoints = new Dictionary<EndpointDictionaryKey, ServiceEndpoint>();

        public override void ExportContract(ContractDescription contract)
        {
            if (this.isFaulted)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.WsdlExporterIsFaulted)));

            if (contract == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contract");

            if (!this.exportedContracts.ContainsKey(contract))
            {
                try
                {
                    WsdlNS.PortType wsdlPortType = CreateWsdlPortType(contract);
                    WsdlContractConversionContext contractContext;


                    contractContext = new WsdlContractConversionContext(contract, wsdlPortType);

                    foreach (OperationDescription operation in contract.Operations)
                    {
                        bool isWildcardAction;
                        if (!OperationIsExportable(operation, out isWildcardAction))
                        {
                            string warningMsg = isWildcardAction ? SR.GetString(SR.WarnSkippingOpertationWithWildcardAction, contract.Name, contract.Namespace, operation.Name)
                                : SR.GetString(SR.WarnSkippingOpertationWithSessionOpenNotificationEnabled, "Action", OperationDescription.SessionOpenedAction, contract.Name, contract.Namespace, operation.Name);

                            LogExportWarning(warningMsg);
                            continue;
                        }

                        WsdlNS.Operation wsdlOperation = CreateWsdlOperation(operation, contract);
                        wsdlPortType.Operations.Add(wsdlOperation);

                        contractContext.AddOperation(operation, wsdlOperation);

                        foreach (MessageDescription message in operation.Messages)
                        {
                            //Create Operation Message
                            WsdlNS.OperationMessage wsdlOperationMessage = CreateWsdlOperationMessage(message);
                            wsdlOperation.Messages.Add(wsdlOperationMessage);
                            contractContext.AddMessage(message, wsdlOperationMessage);
                        }

                        foreach (FaultDescription fault in operation.Faults)
                        {
                            //Create Operation Fault
                            WsdlNS.OperationFault wsdlOperationFault = CreateWsdlOperationFault(fault);
                            wsdlOperation.Faults.Add(wsdlOperationFault);
                            contractContext.AddFault(fault, wsdlOperationFault);
                        }
                    }

                    CallExportContract(contractContext);

                    exportedContracts.Add(contract, contractContext);
                }
                catch
                {
                    isFaulted = true;
                    throw;
                }
            }
        }

        public override void ExportEndpoint(ServiceEndpoint endpoint)
        {
            if (this.isFaulted)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.WsdlExporterIsFaulted)));

            if (endpoint == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");

            ExportEndpoint(endpoint, new XmlQualifiedName(NamingHelper.DefaultServiceName, NamingHelper.DefaultNamespace), null);
        }

        public void ExportEndpoints(IEnumerable<ServiceEndpoint> endpoints, XmlQualifiedName wsdlServiceQName)
        {
            this.ExportEndpoints(endpoints, wsdlServiceQName, null);
        }

        internal void ExportEndpoints(IEnumerable<ServiceEndpoint> endpoints, XmlQualifiedName wsdlServiceQName, BindingParameterCollection bindingParameters)
        {
            if (this.isFaulted)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.WsdlExporterIsFaulted)));

            if (endpoints == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoints");
            if (wsdlServiceQName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wsdlServiceQName");

            foreach (ServiceEndpoint endpoint in endpoints)
            {
                ExportEndpoint(endpoint, wsdlServiceQName, bindingParameters);
            }
        }

        public override MetadataSet GetGeneratedMetadata()
        {
            MetadataSet set = new MetadataSet();

            foreach (WsdlNS.ServiceDescription wsdl in wsdlDocuments)
                set.MetadataSections.Add(MetadataSection.CreateFromServiceDescription(wsdl));

            foreach (XmlSchema schema in xmlSchemas.Schemas())
                set.MetadataSections.Add(MetadataSection.CreateFromSchema(schema));

            return set;
        }

        public WsdlNS.ServiceDescriptionCollection GeneratedWsdlDocuments { get { return wsdlDocuments; } }
        public XmlSchemaSet GeneratedXmlSchemas { get { return xmlSchemas; } }

        void ExportEndpoint(ServiceEndpoint endpoint, XmlQualifiedName wsdlServiceQName, BindingParameterCollection bindingParameters)
        {
            if (endpoint.Binding == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.EndpointsMustHaveAValidBinding1, endpoint.Name)));

            EndpointDictionaryKey endpointKey = new EndpointDictionaryKey(endpoint, wsdlServiceQName);

            try
            {
                if (exportedEndpoints.ContainsKey(endpointKey))
                    return;

                this.ExportContract(endpoint.Contract);

                // Retreive Conversion Context for Contract;
                // Note: Contract must have already been exported at this point.
                WsdlContractConversionContext contractContext = this.exportedContracts[endpoint.Contract];


                bool newWsdlBinding, bindingNameWasUniquified;
                WsdlNS.Port wsdlPort;
                WsdlNS.Binding wsdlBinding;
                wsdlBinding = CreateWsdlBindingAndPort(endpoint, wsdlServiceQName, out wsdlPort, out newWsdlBinding, out bindingNameWasUniquified);


                if (!newWsdlBinding && wsdlPort == null)
                    return;

                // Create an Endpoint conversion context based on 
                // the contract's conversion context (reuse contract correlation information)
                WsdlEndpointConversionContext endpointContext;
                if (newWsdlBinding)
                {
                    endpointContext = new WsdlEndpointConversionContext(contractContext, endpoint, wsdlBinding, wsdlPort);

                    foreach (OperationDescription operation in endpoint.Contract.Operations)
                    {
                        if (!WsdlExporter.OperationIsExportable(operation))
                        {
                            continue;
                        }

                        WsdlNS.OperationBinding wsdlOperationBinding = CreateWsdlOperationBinding(endpoint.Contract, operation);
                        wsdlBinding.Operations.Add(wsdlOperationBinding);

                        endpointContext.AddOperationBinding(operation, wsdlOperationBinding);

                        foreach (MessageDescription message in operation.Messages)
                        {
                            WsdlNS.MessageBinding wsdlMessageBinding = CreateWsdlMessageBinding(message, endpoint.Binding, wsdlOperationBinding);
                            endpointContext.AddMessageBinding(message, wsdlMessageBinding);
                        }

                        foreach (FaultDescription fault in operation.Faults)
                        {
                            WsdlNS.FaultBinding wsdlFaultBinding = CreateWsdlFaultBinding(fault, endpoint.Binding, wsdlOperationBinding);
                            endpointContext.AddFaultBinding(fault, wsdlFaultBinding);
                        }
                    }

                    // CSDMain 180381:  Added internal functionality for passing BindingParameters into the ExportPolicy process via PolicyConversionContext.
                    // However, in order to not change existing behavior, we only call the internal ExportPolicy method which accepts BindingParameters if they are not null
                    // (non-null binding parameters can only be passed in via internal code paths).  Otherwise, we call the existing ExportPolicy method, just like before.
                    PolicyConversionContext policyContext;
                    if (bindingParameters == null)
                    {
                        policyContext = this.ExportPolicy(endpoint);
                    }
                    else
                    {
                        policyContext = this.ExportPolicy(endpoint, bindingParameters);
                    }
                    // consider factoring this out of wsdl exporter
                    new WSPolicyAttachmentHelper(this.PolicyVersion).AttachPolicy(endpoint, endpointContext, policyContext);
                    exportedBindings.Add(new BindingDictionaryKey(endpoint.Contract, endpoint.Binding), endpointContext);
                }
                else
                {
                    endpointContext = new WsdlEndpointConversionContext(exportedBindings[new BindingDictionaryKey(endpoint.Contract, endpoint.Binding)], endpoint, wsdlPort);
                }

                CallExportEndpoint(endpointContext);
                exportedEndpoints.Add(endpointKey, endpoint);
                if (bindingNameWasUniquified)
                    Errors.Add(new MetadataConversionError(SR.GetString(SR.WarnDuplicateBindingQNameNameOnExport, endpoint.Binding.Name, endpoint.Binding.Namespace, endpoint.Contract.Name), true /*isWarning*/));
            }
            catch
            {
                isFaulted = true;
                throw;
            }
        }

        void CallExportEndpoint(WsdlEndpointConversionContext endpointContext)
        {
            foreach (IWsdlExportExtension extension in endpointContext.ExportExtensions)
            {
                CallExtension(endpointContext, extension);
            }
        }

        void CallExportContract(WsdlContractConversionContext contractContext)
        {
            foreach (IWsdlExportExtension extension in contractContext.ExportExtensions)
            {
                CallExtension(contractContext, extension);
            }
        }

        WsdlNS.PortType CreateWsdlPortType(ContractDescription contract)
        {
            XmlQualifiedName wsdlPortTypeQName = WsdlNamingHelper.GetPortTypeQName(contract);

            WsdlNS.ServiceDescription wsdl = GetOrCreateWsdl(wsdlPortTypeQName.Namespace);
            WsdlNS.PortType wsdlPortType = new WsdlNS.PortType();
            wsdlPortType.Name = wsdlPortTypeQName.Name;
            if (wsdl.PortTypes[wsdlPortType.Name] != null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.DuplicateContractQNameNameOnExport, contract.Name, contract.Namespace)));
            NetSessionHelper.AddUsingSessionAttributeIfNeeded(wsdlPortType, contract);
            wsdl.PortTypes.Add(wsdlPortType);

            return wsdlPortType;
        }

        WsdlNS.Operation CreateWsdlOperation(OperationDescription operation, ContractDescription contract)
        {
            WsdlNS.Operation wsdlOperation = new WsdlNS.Operation();
            wsdlOperation.Name = WsdlNamingHelper.GetWsdlOperationName(operation, contract);
            NetSessionHelper.AddInitiatingTerminatingAttributesIfNeeded(wsdlOperation, operation, contract);
            return wsdlOperation;
        }

        WsdlNS.OperationMessage CreateWsdlOperationMessage(MessageDescription message)
        {
            WsdlNS.OperationMessage wsdlOperationMessage;

            if (message.Direction == MessageDirection.Input)
                wsdlOperationMessage = new WsdlNS.OperationInput();
            else
                wsdlOperationMessage = new WsdlNS.OperationOutput();

            if (!XmlName.IsNullOrEmpty(message.MessageName))
                wsdlOperationMessage.Name = message.MessageName.EncodedName;

            // consider factoring this out of wslExporter
            WSAddressingHelper.AddActionAttribute(message.Action, wsdlOperationMessage, this.PolicyVersion);
            return wsdlOperationMessage;
        }

        WsdlNS.OperationFault CreateWsdlOperationFault(FaultDescription fault)
        {
            WsdlNS.OperationFault wsdlOperationFault;
            wsdlOperationFault = new WsdlNS.OperationFault();

            // operation fault name must not be empty (FaultDescription checks this)
            wsdlOperationFault.Name = fault.Name;

            // consider factoring this out of wslExporter
            WSAddressingHelper.AddActionAttribute(fault.Action, wsdlOperationFault, this.PolicyVersion);
            return wsdlOperationFault;
        }

        WsdlNS.Binding CreateWsdlBindingAndPort(ServiceEndpoint endpoint, XmlQualifiedName wsdlServiceQName, out WsdlNS.Port wsdlPort, out bool newBinding, out bool bindingNameWasUniquified)
        {
            WsdlNS.ServiceDescription bindingWsdl;
            WsdlNS.Binding wsdlBinding;
            WsdlEndpointConversionContext bindingConversionContext;
            XmlQualifiedName wsdlBindingQName;
            XmlQualifiedName wsdlPortTypeQName;
            bool printWsdlDeclaration = IsWsdlExportable(endpoint.Binding);

            if (!exportedBindings.TryGetValue(new BindingDictionaryKey(endpoint.Contract, endpoint.Binding), out bindingConversionContext))
            {
                wsdlBindingQName = WsdlNamingHelper.GetBindingQName(endpoint, this, out bindingNameWasUniquified);
                bindingWsdl = GetOrCreateWsdl(wsdlBindingQName.Namespace);
                wsdlBinding = new WsdlNS.Binding();
                wsdlBinding.Name = wsdlBindingQName.Name;
                newBinding = true;

                WsdlNS.PortType wsdlPortType = exportedContracts[endpoint.Contract].WsdlPortType;
                wsdlPortTypeQName = new XmlQualifiedName(wsdlPortType.Name, wsdlPortType.ServiceDescription.TargetNamespace);
                wsdlBinding.Type = wsdlPortTypeQName;
                if (printWsdlDeclaration)
                {
                    bindingWsdl.Bindings.Add(wsdlBinding);
                }
                WsdlExporter.EnsureWsdlContainsImport(bindingWsdl, wsdlPortTypeQName.Namespace);
            }
            else
            {
                wsdlBindingQName = new XmlQualifiedName(bindingConversionContext.WsdlBinding.Name, bindingConversionContext.WsdlBinding.ServiceDescription.TargetNamespace);
                bindingNameWasUniquified = false;
                bindingWsdl = wsdlDocuments[wsdlBindingQName.Namespace];
                wsdlBinding = bindingWsdl.Bindings[wsdlBindingQName.Name];
                wsdlPortTypeQName = wsdlBinding.Type;
                newBinding = false;
            }


            //We can only create a Port if there is an address
            if (endpoint.Address != null)
            {
                WsdlNS.Service wsdlService = GetOrCreateWsdlService(wsdlServiceQName);

                wsdlPort = new WsdlNS.Port();
                string wsdlPortName = WsdlNamingHelper.GetPortName(endpoint, wsdlService);
                wsdlPort.Name = wsdlPortName;
                wsdlPort.Binding = wsdlBindingQName;

                WsdlNS.SoapAddressBinding addressBinding = SoapHelper.GetOrCreateSoapAddressBinding(wsdlBinding, wsdlPort, this);

                if (addressBinding != null)
                {
                    addressBinding.Location = endpoint.Address.Uri.AbsoluteUri;
                }

                WsdlExporter.EnsureWsdlContainsImport(wsdlService.ServiceDescription, wsdlBindingQName.Namespace);
                if (printWsdlDeclaration)
                {
                    wsdlService.Ports.Add(wsdlPort);
                }
            }
            else
            {
                wsdlPort = null;
            }

            return wsdlBinding;
        }

        WsdlNS.OperationBinding CreateWsdlOperationBinding(ContractDescription contract, OperationDescription operation)
        {
            WsdlNS.OperationBinding wsdlOperationBinding = new WsdlNS.OperationBinding();
            wsdlOperationBinding.Name = WsdlNamingHelper.GetWsdlOperationName(operation, contract);
            return wsdlOperationBinding;
        }

        WsdlNS.MessageBinding CreateWsdlMessageBinding(MessageDescription messageDescription, Binding binding, WsdlNS.OperationBinding wsdlOperationBinding)
        {
            WsdlNS.MessageBinding wsdlMessageBinding;
            if (messageDescription.Direction == MessageDirection.Input)
            {
                wsdlOperationBinding.Input = new WsdlNS.InputBinding();
                wsdlMessageBinding = wsdlOperationBinding.Input;
            }
            else
            {
                wsdlOperationBinding.Output = new WsdlNS.OutputBinding();
                wsdlMessageBinding = wsdlOperationBinding.Output;
            }

            if (!XmlName.IsNullOrEmpty(messageDescription.MessageName))
                wsdlMessageBinding.Name = messageDescription.MessageName.EncodedName;

            return wsdlMessageBinding;
        }

        WsdlNS.FaultBinding CreateWsdlFaultBinding(FaultDescription faultDescription, Binding binding, WsdlNS.OperationBinding wsdlOperationBinding)
        {
            WsdlNS.FaultBinding wsdlFaultBinding = new WsdlNS.FaultBinding();
            wsdlOperationBinding.Faults.Add(wsdlFaultBinding);
            if (faultDescription.Name != null)
                wsdlFaultBinding.Name = faultDescription.Name;

            return wsdlFaultBinding;
        }

        internal static bool OperationIsExportable(OperationDescription operation)
        {
            bool isWildcardAction;
            return OperationIsExportable(operation, out isWildcardAction);
        }

        internal static bool OperationIsExportable(OperationDescription operation, out bool isWildcardAction)
        {
            isWildcardAction = false;

            if (operation.IsSessionOpenNotificationEnabled)
            {
                return false;
            }

            for (int i = 0; i < operation.Messages.Count; i++)
            {
                if (operation.Messages[i].Action == MessageHeaders.WildcardAction)
                {
                    isWildcardAction = true;
                    return false;
                }
            }
            return true;
        }

        internal static bool IsBuiltInOperationBehavior(IWsdlExportExtension extension)
        {
            DataContractSerializerOperationBehavior dcsob = extension as DataContractSerializerOperationBehavior;
            if (dcsob != null)
            {
                return dcsob.IsBuiltInOperationBehavior;
            }

            XmlSerializerOperationBehavior xsob = extension as XmlSerializerOperationBehavior;
            if (xsob != null)
            {
                return xsob.IsBuiltInOperationBehavior;
            }

            return false;
        }

        static XmlDocument XmlDoc
        {
            get
            {
                if (xmlDocument == null)
                {
                    NameTable nameTable = new NameTable();
                    nameTable.Add(MetadataStrings.WSPolicy.Elements.Policy);
                    nameTable.Add(MetadataStrings.WSPolicy.Elements.All);
                    nameTable.Add(MetadataStrings.WSPolicy.Elements.ExactlyOne);
                    nameTable.Add(MetadataStrings.WSPolicy.Attributes.PolicyURIs);
                    nameTable.Add(MetadataStrings.Wsu.Attributes.Id);
                    nameTable.Add(MetadataStrings.Addressing200408.Policy.UsingAddressing);
                    nameTable.Add(MetadataStrings.Addressing10.WsdlBindingPolicy.UsingAddressing);
                    nameTable.Add(MetadataStrings.Addressing10.MetadataPolicy.Addressing);
                    nameTable.Add(MetadataStrings.Addressing10.MetadataPolicy.AnonymousResponses);
                    nameTable.Add(MetadataStrings.Addressing10.MetadataPolicy.NonAnonymousResponses);
                    xmlDocument = new XmlDocument(nameTable);
                }
                return xmlDocument;
            }
        }

        // Generate WSDL Document if it doesn't already exist otherwise, return the appropriate WSDL document
        internal WsdlNS.ServiceDescription GetOrCreateWsdl(string ns)
        {
            // NOTE: this method is not thread safe
            WsdlNS.ServiceDescriptionCollection wsdlCollection = this.wsdlDocuments;
            WsdlNS.ServiceDescription wsdl = wsdlCollection[ns];

            // Look for wsdl in service descriptions that have been created. If we cannot find it then we create it
            if (wsdl == null)
            {
                wsdl = new WsdlNS.ServiceDescription();
                wsdl.TargetNamespace = ns;

                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces(new WsdlNamespaceHelper(this.PolicyVersion).SerializerNamespaces);
                if (!string.IsNullOrEmpty(wsdl.TargetNamespace))
                    namespaces.Add("tns", wsdl.TargetNamespace);
                wsdl.Namespaces = namespaces;

                wsdlCollection.Add(wsdl);
            }

            return wsdl;
        }

        WsdlNS.Service GetOrCreateWsdlService(XmlQualifiedName wsdlServiceQName)
        {
            // NOTE: this method is not thread safe

            WsdlNS.ServiceDescription wsdl = GetOrCreateWsdl(wsdlServiceQName.Namespace);

            WsdlNS.Service wsdlService = wsdl.Services[wsdlServiceQName.Name];
            if (wsdlService == null)
            {
                //Service not found. Create service.
                wsdlService = new WsdlNS.Service();
                wsdlService.Name = wsdlServiceQName.Name;

                if (string.IsNullOrEmpty(wsdl.Name))
                    wsdl.Name = wsdlService.Name;

                wsdl.Services.Add(wsdlService);
            }
            return wsdlService;
        }

        static void EnsureWsdlContainsImport(WsdlNS.ServiceDescription srcWsdl, string target)
        {
            if (srcWsdl.TargetNamespace == target)
                return;
            // FindImport
            foreach (WsdlNS.Import import in srcWsdl.Imports)
            {
                if (import.Namespace == target)
                    return;
            }
            {
                WsdlNS.Import import = new WsdlNS.Import();
                import.Location = null;
                import.Namespace = target;
                srcWsdl.Imports.Add(import);
                WsdlNamespaceHelper.FindOrCreatePrefix("i", target, srcWsdl);
                return;
            }
        }

        void LogExportWarning(string warningMessage)
        {
            this.Errors.Add(new MetadataConversionError(warningMessage, true));
        }

        static internal XmlSchemaSet GetEmptySchemaSet()
        {
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.XmlResolver = null;
            return schemaSet;
        }

        static bool IsWsdlExportable(Binding binding)
        {
            BindingElementCollection bindingElements = binding.CreateBindingElements();
            if (bindingElements == null)
            {
                return true;
            }
            foreach (BindingElement bindingElement in bindingElements)
            {
                MessageEncodingBindingElement messageEncodingBindingElement = bindingElement as MessageEncodingBindingElement;
                if (messageEncodingBindingElement != null && !messageEncodingBindingElement.IsWsdlExportable)
                {
                    return false;
                }
            }
            return true;
        }

        internal static class WSAddressingHelper
        {
            internal static void AddActionAttribute(string actionUri, WsdlNS.OperationMessage wsdlOperationMessage, PolicyVersion policyVersion)
            {
                XmlAttribute attribute;
                if (policyVersion == PolicyVersion.Policy12)
                {
                    attribute = WsdlExporter.XmlDoc.CreateAttribute(MetadataStrings.AddressingWsdl.Prefix,
                        MetadataStrings.AddressingWsdl.Action,
                        MetadataStrings.AddressingWsdl.NamespaceUri);
                }
                else
                {
                    attribute = WsdlExporter.XmlDoc.CreateAttribute(MetadataStrings.AddressingMetadata.Prefix,
                        MetadataStrings.AddressingMetadata.Action,
                        MetadataStrings.AddressingMetadata.NamespaceUri);
                }

                attribute.Value = actionUri;
                wsdlOperationMessage.ExtensibleAttributes = new XmlAttribute[] { attribute };
            }

            internal static void AddAddressToWsdlPort(WsdlNS.Port wsdlPort, EndpointAddress addr, AddressingVersion addressing)
            {
                if (addressing == AddressingVersion.None)
                {
                    return;
                }

                MemoryStream stream = new MemoryStream();
                XmlWriter xw = XmlWriter.Create(stream);
                xw.WriteStartElement("temp");

                if (addressing == AddressingVersion.WSAddressing10)
                {
                    xw.WriteAttributeString("xmlns", MetadataStrings.Addressing10.Prefix, null, MetadataStrings.Addressing10.NamespaceUri);
                }
                else if (addressing == AddressingVersion.WSAddressingAugust2004)
                {
                    xw.WriteAttributeString("xmlns", MetadataStrings.Addressing200408.Prefix, null, MetadataStrings.Addressing200408.NamespaceUri);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.AddressingVersionNotSupported, addressing)));
                }

                addr.WriteTo(addressing, xw);
                xw.WriteEndElement();

                xw.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                XmlReader xr = XmlReader.Create(stream);
                xr.MoveToContent();
                XmlElement endpointRef = (XmlElement)XmlDoc.ReadNode(xr).ChildNodes[0];

                wsdlPort.Extensions.Add(endpointRef);
            }

            internal static void AddWSAddressingAssertion(MetadataExporter exporter, PolicyConversionContext context, AddressingVersion addressVersion)
            {
                XmlElement addressingAssertion;
                if (addressVersion == AddressingVersion.WSAddressingAugust2004)
                {
                    addressingAssertion = XmlDoc.CreateElement(MetadataStrings.Addressing200408.Policy.Prefix,
                        MetadataStrings.Addressing200408.Policy.UsingAddressing,
                        MetadataStrings.Addressing200408.Policy.NamespaceUri);
                }
                else if (addressVersion == AddressingVersion.WSAddressing10)
                {
                    if (exporter.PolicyVersion == PolicyVersion.Policy12)
                    {
                        addressingAssertion = XmlDoc.CreateElement(MetadataStrings.Addressing10.WsdlBindingPolicy.Prefix,
                            MetadataStrings.Addressing10.WsdlBindingPolicy.UsingAddressing,
                            MetadataStrings.Addressing10.WsdlBindingPolicy.NamespaceUri);
                    }
                    else
                    {
                        addressingAssertion = XmlDoc.CreateElement(MetadataStrings.Addressing10.MetadataPolicy.Prefix,
                            MetadataStrings.Addressing10.MetadataPolicy.Addressing,
                            MetadataStrings.Addressing10.MetadataPolicy.NamespaceUri);

                        // All of our existing transports are anonymous, so default to it.
                        SupportedAddressingMode mode = SupportedAddressingMode.Anonymous;
                        string key = typeof(SupportedAddressingMode).Name;

                        if (exporter.State.ContainsKey(key) && exporter.State[key] is SupportedAddressingMode)
                        {
                            mode = (SupportedAddressingMode)exporter.State[key];
                            if (!SupportedAddressingModeHelper.IsDefined(mode))
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SupportedAddressingModeNotSupported, mode)));
                        }

                        if (mode != SupportedAddressingMode.Mixed)
                        {
                            string responsesAssertionLocalName;
                            if (mode == SupportedAddressingMode.Anonymous)
                            {
                                responsesAssertionLocalName = MetadataStrings.Addressing10.MetadataPolicy.AnonymousResponses;
                            }
                            else
                            {
                                responsesAssertionLocalName = MetadataStrings.Addressing10.MetadataPolicy.NonAnonymousResponses;
                            }

                            XmlElement innerPolicyElement = XmlDoc.CreateElement(MetadataStrings.WSPolicy.Prefix,
                                    MetadataStrings.WSPolicy.Elements.Policy,
                                    MetadataStrings.WSPolicy.NamespaceUri15);

                            XmlElement responsesAssertion = XmlDoc.CreateElement(MetadataStrings.Addressing10.MetadataPolicy.Prefix,
                                    responsesAssertionLocalName,
                                    MetadataStrings.Addressing10.MetadataPolicy.NamespaceUri);

                            innerPolicyElement.AppendChild(responsesAssertion);
                            addressingAssertion.AppendChild(innerPolicyElement);
                        }
                    }
                }
                else if (addressVersion == AddressingVersion.None)
                {
                    // do nothing
                    addressingAssertion = null;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.AddressingVersionNotSupported, addressVersion)));
                }

                if (addressingAssertion != null)
                {
                    context.GetBindingAssertions().Add(addressingAssertion);
                }
            }
        }

        class WSPolicyAttachmentHelper
        {
            PolicyVersion policyVersion;
            internal WSPolicyAttachmentHelper(PolicyVersion policyVersion)
            {
                this.policyVersion = policyVersion;
            }

            internal void AttachPolicy(ServiceEndpoint endpoint, WsdlEndpointConversionContext endpointContext, PolicyConversionContext policyContext)
            {
                SortedList<string, string> policyKeys = new SortedList<string, string>();
                NamingHelper.DoesNameExist policyKeyIsUnique
                    = delegate(string name, object nameCollection)
                    {
                        return policyKeys.ContainsKey(name);
                    };

                string key, keyBase;
                ICollection<XmlElement> assertions;

                WsdlNS.ServiceDescription policyWsdl = endpointContext.WsdlBinding.ServiceDescription;

                assertions = policyContext.GetBindingAssertions();

                // Add [wsdl:Binding] level Policy
                WsdlNS.Binding wsdlBinding = endpointContext.WsdlBinding;
                if (assertions.Count > 0)
                {
                    keyBase = CreateBindingPolicyKey(wsdlBinding);
                    key = NamingHelper.GetUniqueName(keyBase, policyKeyIsUnique, null);
                    policyKeys.Add(key, key);
                    AttachItemPolicy(assertions, key, policyWsdl, wsdlBinding);
                }

                foreach (OperationDescription operation in endpoint.Contract.Operations)
                {
                    if (!WsdlExporter.OperationIsExportable(operation))
                    {
                        continue;
                    }

                    assertions = policyContext.GetOperationBindingAssertions(operation);

                    // Add [wsdl:Binding/wsdl:operation] policy
                    if (assertions.Count > 0)
                    {
                        WsdlNS.OperationBinding wsdlOperationBinding = endpointContext.GetOperationBinding(operation);
                        keyBase = CreateOperationBindingPolicyKey(wsdlOperationBinding);
                        key = NamingHelper.GetUniqueName(keyBase, policyKeyIsUnique, null);
                        policyKeys.Add(key, key);
                        AttachItemPolicy(assertions, key, policyWsdl, wsdlOperationBinding);
                    }

                    //
                    // Add [wsdl:Binding/wsdl:operation] child policy
                    //

                    foreach (MessageDescription message in operation.Messages)
                    {
                        assertions = policyContext.GetMessageBindingAssertions(message);

                        // Add [wsdl:Binding/wsdl:operation/wsdl:(input, output, message)] policy
                        if (assertions.Count > 0)
                        {
                            WsdlNS.MessageBinding wsdlMessageBinding = endpointContext.GetMessageBinding(message);
                            keyBase = CreateMessageBindingPolicyKey(wsdlMessageBinding, message.Direction);
                            key = NamingHelper.GetUniqueName(keyBase, policyKeyIsUnique, null);
                            policyKeys.Add(key, key);
                            AttachItemPolicy(assertions, key, policyWsdl, wsdlMessageBinding);
                        }
                    }

                    foreach (FaultDescription fault in operation.Faults)
                    {
                        assertions = policyContext.GetFaultBindingAssertions(fault);

                        // Add [wsdl:Binding/wsdl:operation/wsdl:fault] policy
                        if (assertions.Count > 0)
                        {
                            WsdlNS.FaultBinding wsdlFaultBinding = endpointContext.GetFaultBinding(fault);
                            keyBase = CreateFaultBindingPolicyKey(wsdlFaultBinding);
                            key = NamingHelper.GetUniqueName(keyBase, policyKeyIsUnique, null);
                            policyKeys.Add(key, key);
                            AttachItemPolicy(assertions, key, policyWsdl, wsdlFaultBinding);
                        }
                    }
                }
            }

            void AttachItemPolicy(ICollection<XmlElement> assertions, string key, WsdlNS.ServiceDescription policyWsdl, WsdlNS.DocumentableItem item)
            {
                string policyKey = InsertPolicy(key, policyWsdl, assertions);
                InsertPolicyReference(policyKey, item);
            }

            void InsertPolicyReference(string policyKey, WsdlNS.DocumentableItem item)
            {
                //Create wsp:PolicyReference Element On DocumentableItem
                //---------------------------------------------------------------------------------------------------------
                XmlElement policyReferenceElement = XmlDoc.CreateElement(MetadataStrings.WSPolicy.Prefix,
                                                            MetadataStrings.WSPolicy.Elements.PolicyReference,
                                                            policyVersion.Namespace);

                //Create wsp:PolicyURIs Attribute On DocumentableItem
                //---------------------------------------------------------------------------------------------------------
                XmlAttribute uriAttribute = XmlDoc.CreateAttribute(MetadataStrings.WSPolicy.Attributes.URI);

                uriAttribute.Value = policyKey;
                policyReferenceElement.Attributes.Append(uriAttribute);
                item.Extensions.Add(policyReferenceElement);
            }

            string InsertPolicy(string key, WsdlNS.ServiceDescription policyWsdl, ICollection<XmlElement> assertions)
            {
                // Create [wsp:Policy]
                XmlElement policyElement = CreatePolicyElement(assertions);

                //Create [wsp:Policy/@wsu:Id]
                XmlAttribute idAttribute = XmlDoc.CreateAttribute(MetadataStrings.Wsu.Prefix,
                                                            MetadataStrings.Wsu.Attributes.Id,
                                                            MetadataStrings.Wsu.NamespaceUri);
                idAttribute.Value = key;
                policyElement.SetAttributeNode(idAttribute);

                // Add wsp:Policy To WSDL
                if (policyWsdl != null)
                {
                    policyWsdl.Extensions.Add(policyElement);
                }

                return string.Format(CultureInfo.InvariantCulture, "#{0}", key);
            }

            XmlElement CreatePolicyElement(ICollection<XmlElement> assertions)
            {
                // Create [wsp:Policy]
                XmlElement policyElement = XmlDoc.CreateElement(MetadataStrings.WSPolicy.Prefix,
                                                            MetadataStrings.WSPolicy.Elements.Policy,
                                                            policyVersion.Namespace);

                // Create [wsp:Policy/wsp:ExactlyOne]
                XmlElement exactlyOneElement = XmlDoc.CreateElement(MetadataStrings.WSPolicy.Prefix,
                                                            MetadataStrings.WSPolicy.Elements.ExactlyOne,
                                                            policyVersion.Namespace);
                policyElement.AppendChild(exactlyOneElement);

                // Create [wsp:Policy/wsp:ExactlyOne/wsp:All]
                XmlElement allElement = XmlDoc.CreateElement(MetadataStrings.WSPolicy.Prefix,
                                                            MetadataStrings.WSPolicy.Elements.All,
                                                            policyVersion.Namespace);
                exactlyOneElement.AppendChild(allElement);

                // Add [wsp:Policy/wsp:ExactlyOne/wsp:All/*]
                foreach (XmlElement assertion in assertions)
                {
                    XmlNode iNode = XmlDoc.ImportNode(assertion, true);
                    allElement.AppendChild(iNode);
                }

                return policyElement;
            }

            static string CreateBindingPolicyKey(WsdlNS.Binding wsdlBinding)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}_policy", wsdlBinding.Name);
            }

            static string CreateOperationBindingPolicyKey(WsdlNS.OperationBinding wsdlOperationBinding)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}_{1}_policy",
                    wsdlOperationBinding.Binding.Name,
                    wsdlOperationBinding.Name);
            }

            static string CreateMessageBindingPolicyKey(WsdlNS.MessageBinding wsdlMessageBinding, MessageDirection direction)
            {
                WsdlNS.OperationBinding wsdlOperationBinding = wsdlMessageBinding.OperationBinding;
                WsdlNS.Binding wsdlBinding = wsdlOperationBinding.Binding;

                if (direction == MessageDirection.Input)
                    return string.Format(CultureInfo.InvariantCulture, "{0}_{1}_Input_policy", wsdlBinding.Name, wsdlOperationBinding.Name);
                else
                    return string.Format(CultureInfo.InvariantCulture, "{0}_{1}_output_policy", wsdlBinding.Name, wsdlOperationBinding.Name);
            }

            static string CreateFaultBindingPolicyKey(WsdlNS.FaultBinding wsdlFaultBinding)
            {
                WsdlNS.OperationBinding wsdlOperationBinding = wsdlFaultBinding.OperationBinding;
                WsdlNS.Binding wsdlBinding = wsdlOperationBinding.Binding;
                if (string.IsNullOrEmpty(wsdlFaultBinding.Name))
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0}_{1}_Fault", wsdlBinding.Name, wsdlOperationBinding.Name);
                }
                else
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_Fault", wsdlBinding.Name, wsdlOperationBinding.Name, wsdlFaultBinding.Name);
                }
            }

        }

        class WsdlNamespaceHelper
        {
            XmlSerializerNamespaces xmlSerializerNamespaces;
            PolicyVersion policyVersion;
            internal XmlSerializerNamespaces SerializerNamespaces
            {
                get
                {
                    if (xmlSerializerNamespaces == null)
                    {
                        XmlSerializerNamespaceWrapper namespaces = new XmlSerializerNamespaceWrapper();
                        namespaces.Add("wsdl", WsdlNS.ServiceDescription.Namespace);
                        namespaces.Add("xsd", XmlSchema.Namespace);
                        namespaces.Add(MetadataStrings.WSPolicy.Prefix, policyVersion.Namespace);
                        namespaces.Add(MetadataStrings.Wsu.Prefix, MetadataStrings.Wsu.NamespaceUri);
                        namespaces.Add(MetadataStrings.Addressing200408.Prefix, MetadataStrings.Addressing200408.NamespaceUri);
                        namespaces.Add(MetadataStrings.Addressing200408.Policy.Prefix, MetadataStrings.Addressing200408.Policy.NamespaceUri);
                        namespaces.Add(MetadataStrings.Addressing10.Prefix, MetadataStrings.Addressing10.NamespaceUri);
                        namespaces.Add(MetadataStrings.Addressing10.WsdlBindingPolicy.Prefix, MetadataStrings.Addressing10.WsdlBindingPolicy.NamespaceUri);
                        namespaces.Add(MetadataStrings.Addressing10.MetadataPolicy.Prefix, MetadataStrings.Addressing10.MetadataPolicy.NamespaceUri);
                        namespaces.Add(MetadataStrings.MetadataExchangeStrings.Prefix, MetadataStrings.MetadataExchangeStrings.Namespace);
                        namespaces.Add(NetSessionHelper.Prefix, NetSessionHelper.NamespaceUri);

                        namespaces.Add("soapenc", "http://schemas.xmlsoap.org/soap/encoding/");
                        namespaces.Add("soap12", "http://schemas.xmlsoap.org/wsdl/soap12/");
                        namespaces.Add("soap", "http://schemas.xmlsoap.org/wsdl/soap/");

                        xmlSerializerNamespaces = namespaces.GetNamespaces();
                    }
                    return xmlSerializerNamespaces;
                }
            }

            internal WsdlNamespaceHelper(PolicyVersion policyVersion)
            {
                this.policyVersion = policyVersion;
            }

            // doesn't care if you add a duplicate prefix
            class XmlSerializerNamespaceWrapper
            {
                readonly XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                readonly Dictionary<string, string> lookup = new Dictionary<string, string>();

                internal void Add(string prefix, string namespaceUri)
                {
                    if (!lookup.ContainsKey(prefix))
                    {
                        namespaces.Add(prefix, namespaceUri);
                        lookup.Add(prefix, namespaceUri);
                    }
                }

                internal XmlSerializerNamespaces GetNamespaces()
                {
                    return namespaces;
                }
            }

            internal static string FindOrCreatePrefix(string prefixBase, string ns, params WsdlNS.DocumentableItem[] scopes)
            {
                if (!(scopes.Length > 0))
                {
                    Fx.Assert("You must pass at least one namespaceScope");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "You must pass at least one namespaceScope")));
                }
                string prefix = null;

                if (string.IsNullOrEmpty(ns))
                {
                    prefix = string.Empty;
                }
                else
                {
                    //See if a prefix for the namespace has already been defined at one of the scopes
                    for (int j = 0; j < scopes.Length; j++)
                        if (TryMatchNamespace(scopes[j].Namespaces.ToArray(), ns, out prefix))
                            return prefix;

                    // Create prefix definition at the nearest scope.
                    int i = 0;
                    prefix = prefixBase + i.ToString(CultureInfo.InvariantCulture);

                    //[....], consider do we need to check at higher scopes as well?
                    while (PrefixExists(scopes[0].Namespaces.ToArray(), prefix))
                        prefix = prefixBase + (++i).ToString(CultureInfo.InvariantCulture);
                }
                scopes[0].Namespaces.Add(prefix, ns);

                return prefix;
            }

            static bool PrefixExists(XmlQualifiedName[] prefixDefinitions, string prefix)
            {
                return Array.Exists<XmlQualifiedName>(prefixDefinitions,
                    delegate(XmlQualifiedName prefixDef)
                    {
                        if (prefixDef.Name == prefix)
                        {
                            return true;
                        }
                        return false;
                    });

            }

            static bool TryMatchNamespace(XmlQualifiedName[] prefixDefinitions, string ns, out string prefix)
            {
                string foundPrefix = null;
                Array.Find<XmlQualifiedName>(prefixDefinitions,
                    delegate(XmlQualifiedName prefixDef)
                    {
                        if (prefixDef.Namespace == ns)
                        {
                            foundPrefix = prefixDef.Name;
                            return true;
                        }
                        return false;
                    });

                prefix = foundPrefix;
                return foundPrefix != null;
            }
        }

        internal static class WsdlNamingHelper
        {
            internal static XmlQualifiedName GetPortTypeQName(ContractDescription contract)
            {
                return new XmlQualifiedName(contract.Name, contract.Namespace);
            }

            internal static XmlQualifiedName GetBindingQName(ServiceEndpoint endpoint, WsdlExporter exporter, out bool wasUniquified)
            {
                // due to problems in Sysytem.Web.Services.Descriprion.ServiceDescription.Write() (double encoding) method we cannot use encoded names for
                // wsdl:binding item: we need to make sure that XmlConvert.EncodeLocalName will not find any problems with the name, and leave it unchanged.
                // consider changing the name here to something that will not be encoded by XmlSerializer (GenerateSimpleXmlName()?)
                string localName = endpoint.Name;

                string bindingWsdlNamespace = endpoint.Binding.Namespace;
                string uniquifiedLocalName = NamingHelper.GetUniqueName(localName, WsdlBindingQNameExists(exporter, bindingWsdlNamespace), null);
                wasUniquified = localName != uniquifiedLocalName;

                return new XmlQualifiedName(uniquifiedLocalName, bindingWsdlNamespace);
            }

            static NamingHelper.DoesNameExist WsdlBindingQNameExists(WsdlExporter exporter, string bindingWsdlNamespace)
            {
                return delegate(string localName, object nameCollection)
                {
                    XmlQualifiedName wsdlBindingQName = new XmlQualifiedName(localName, bindingWsdlNamespace);
                    WsdlNS.ServiceDescription wsdl = exporter.wsdlDocuments[bindingWsdlNamespace];
                    if (wsdl != null && wsdl.Bindings[localName] != null)
                        return true;

                    return false;
                };
            }



            internal static string GetPortName(ServiceEndpoint endpoint, WsdlNS.Service wsdlService)
            {
                return NamingHelper.GetUniqueName(endpoint.Name, ServiceContainsPort(wsdlService), null);
            }

            static NamingHelper.DoesNameExist ServiceContainsPort(WsdlNS.Service service)
            {
                return delegate(string portName, object nameCollection)
                {
                    foreach (WsdlNS.Port port in service.Ports)
                        if (port.Name == portName)
                            return true;
                    return false;
                };
            }

            internal static string GetWsdlOperationName(OperationDescription operationDescription, ContractDescription parentContractDescription)
            {
                return operationDescription.Name;
            }
        }

        internal static class NetSessionHelper
        {
            internal const string NamespaceUri = "http://schemas.microsoft.com/ws/2005/12/wsdl/contract";
            internal const string Prefix = "msc";
            internal const string UsingSession = "usingSession";
            internal const string IsInitiating = "isInitiating";
            internal const string IsTerminating = "isTerminating";
            internal const string True = "true";
            internal const string False = "false";

            internal static void AddUsingSessionAttributeIfNeeded(WsdlNS.PortType wsdlPortType, ContractDescription contract)
            {
                bool sessionValue;

                if (contract.SessionMode == SessionMode.Required)
                {
                    sessionValue = true;
                }
                else if (contract.SessionMode == SessionMode.NotAllowed)
                {
                    sessionValue = false;
                }
                else
                {
                    return;
                }

                wsdlPortType.ExtensibleAttributes = CloneAndAddToAttributes(wsdlPortType.ExtensibleAttributes, NetSessionHelper.Prefix,
                    NetSessionHelper.UsingSession, NetSessionHelper.NamespaceUri, ToValue(sessionValue));
            }

            internal static void AddInitiatingTerminatingAttributesIfNeeded(WsdlNS.Operation wsdlOperation,
                OperationDescription operation, ContractDescription contract)
            {
                if (contract.SessionMode == SessionMode.Required)
                {
                    AddInitiatingAttribute(wsdlOperation, operation.IsInitiating);
                    AddTerminatingAttribute(wsdlOperation, operation.IsTerminating);
                }
            }

            static void AddInitiatingAttribute(System.Web.Services.Description.Operation wsdlOperation, bool isInitiating)
            {
                wsdlOperation.ExtensibleAttributes = CloneAndAddToAttributes(wsdlOperation.ExtensibleAttributes, NetSessionHelper.Prefix,
                    NetSessionHelper.IsInitiating, NetSessionHelper.NamespaceUri, ToValue(isInitiating));
            }

            static void AddTerminatingAttribute(System.Web.Services.Description.Operation wsdlOperation, bool isTerminating)
            {
                wsdlOperation.ExtensibleAttributes = CloneAndAddToAttributes(wsdlOperation.ExtensibleAttributes, NetSessionHelper.Prefix,
                    NetSessionHelper.IsTerminating, NetSessionHelper.NamespaceUri, ToValue(isTerminating));
            }

            static XmlAttribute[] CloneAndAddToAttributes(XmlAttribute[] originalAttributes, string prefix, string localName, string ns, string value)
            {
                XmlAttribute newAttribute = XmlDoc.CreateAttribute(prefix, localName, ns);
                newAttribute.Value = value;

                int originalAttributeCount = 0;
                if (originalAttributes != null)
                    originalAttributeCount = originalAttributes.Length;

                XmlAttribute[] attributes = new XmlAttribute[originalAttributeCount + 1];

                if (originalAttributes != null)
                    originalAttributes.CopyTo(attributes, 0);

                attributes[attributes.Length - 1] = newAttribute;

                return attributes;
            }

            static string ToValue(bool b)
            {
                return b ? NetSessionHelper.True : NetSessionHelper.False;
            }
        }

        void CallExtension(WsdlContractConversionContext contractContext, IWsdlExportExtension extension)
        {
            try
            {
                extension.ExportContract(this, contractContext);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ThrowExtensionException(contractContext.Contract, extension, e));
            }
        }

        void CallExtension(WsdlEndpointConversionContext endpointContext, IWsdlExportExtension extension)
        {
            try
            {
                extension.ExportEndpoint(this, endpointContext);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ThrowExtensionException(endpointContext.Endpoint, extension, e));
            }
        }

        Exception ThrowExtensionException(ContractDescription contract, IWsdlExportExtension exporter, Exception e)
        {
            string contractIdentifier = new XmlQualifiedName(contract.Name, contract.Namespace).ToString();
            string errorMessage = SR.GetString(SR.WsdlExtensionContractExportError, exporter.GetType(), contractIdentifier);

            return new InvalidOperationException(errorMessage, e);
        }

        Exception ThrowExtensionException(ServiceEndpoint endpoint, IWsdlExportExtension exporter, Exception e)
        {
            string endpointIdentifier;
            if (endpoint.Address != null && endpoint.Address.Uri != null)
                endpointIdentifier = endpoint.Address.Uri.ToString();
            else
                endpointIdentifier = String.Format(CultureInfo.InvariantCulture,
                    "Contract={1}:{0} ,Binding={3}:{2}",
                    endpoint.Contract.Name,
                    endpoint.Contract.Namespace,
                    endpoint.Binding.Name,
                    endpoint.Binding.Namespace);

            string errorMessage = SR.GetString(SR.WsdlExtensionEndpointExportError, exporter.GetType(), endpointIdentifier);

            return new InvalidOperationException(errorMessage, e);
        }

        sealed class BindingDictionaryKey
        {
            public readonly ContractDescription Contract;
            public readonly Binding Binding;

            public BindingDictionaryKey(ContractDescription contract, Binding binding)
            {
                this.Contract = contract;
                this.Binding = binding;
            }

            public override bool Equals(object obj)
            {
                BindingDictionaryKey key = obj as BindingDictionaryKey;
                if (key != null && key.Binding == this.Binding && key.Contract == this.Contract)
                    return true;
                return false;
            }

            public override int GetHashCode()
            {
                return this.Contract.GetHashCode() ^ this.Binding.GetHashCode();
            }
        }

        sealed class EndpointDictionaryKey
        {
            public readonly ServiceEndpoint Endpoint;
            public readonly XmlQualifiedName ServiceQName;

            public EndpointDictionaryKey(ServiceEndpoint endpoint, XmlQualifiedName serviceQName)
            {
                this.Endpoint = endpoint;
                this.ServiceQName = serviceQName;
            }

            public override bool Equals(object obj)
            {
                EndpointDictionaryKey key = obj as EndpointDictionaryKey;
                if (key != null && key.Endpoint == this.Endpoint && key.ServiceQName == this.ServiceQName)
                    return true;
                return false;
            }

            public override int GetHashCode()
            {
                return this.Endpoint.GetHashCode() ^ this.ServiceQName.GetHashCode();
            }
        }
    }
}


