//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using WsdlNS = System.Web.Services.Description;

    abstract class MessageContractExporter
    {
        readonly protected WsdlContractConversionContext contractContext;
        readonly protected WsdlExporter exporter;
        readonly protected OperationDescription operation;
        readonly protected IOperationBehavior extension;

        static internal void ExportMessageBinding(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext, Type messageContractExporterType, OperationDescription operation)
        {
            new MessageBindingExporter(exporter, endpointContext).ExportMessageBinding(operation, messageContractExporterType);

        }

        protected abstract object OnExportMessageContract();
        protected abstract void ExportHeaders(int messageIndex, object state);
        protected abstract void ExportBody(int messageIndex, object state);

        protected abstract void ExportKnownTypes();
        protected abstract bool IsRpcStyle();
        protected abstract bool IsEncoded();
        protected abstract object GetExtensionData();

        protected MessageExportContext ExportedMessages
        {
            get { return GetMessageExportContext(exporter); }
        }

        void AddElementToSchema(XmlSchemaElement element, string elementNs, XmlSchemaSet schemaSet)
        {
            OperationDescription parentOperation = this.operation;
            if (parentOperation.OperationMethod != null)
            {
                XmlQualifiedName qname = new XmlQualifiedName(element.Name, elementNs);

                OperationElement existingElement;
                if (ExportedMessages.ElementTypes.TryGetValue(qname, out existingElement))
                {
                    if (existingElement.Operation.OperationMethod == parentOperation.OperationMethod)
                        return;
                    if (!SchemaHelper.IsMatch(element, existingElement.Element))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotHaveTwoOperationsWithTheSameElement5, parentOperation.OperationMethod.DeclaringType, parentOperation.OperationMethod.Name, qname, existingElement.Operation.OperationMethod.DeclaringType, existingElement.Operation.Name)));
                    }
                    return;
                }
                else
                {
                    ExportedMessages.ElementTypes.Add(qname, new OperationElement(element, parentOperation));
                }
            }
            SchemaHelper.AddElementToSchema(element, SchemaHelper.GetSchema(elementNs, schemaSet), schemaSet);
        }

        static MessageExportContext GetMessageExportContext(WsdlExporter exporter)
        {
            object messageExportContext;
            if (!exporter.State.TryGetValue(typeof(MessageExportContext), out messageExportContext))
            {
                messageExportContext = new MessageExportContext();
                exporter.State[typeof(MessageExportContext)] = messageExportContext;
            }
            return (MessageExportContext)messageExportContext;
        }

        protected MessageContractExporter(WsdlExporter exporter, WsdlContractConversionContext context, OperationDescription operation, IOperationBehavior extension)
        {
            this.exporter = exporter;
            this.contractContext = context;
            this.operation = operation;
            this.extension = extension;
        }

        internal void ExportMessageContract()
        {
            if (extension == null)
                return;

            object state = OnExportMessageContract();

            OperationFormatter.Validate(operation, IsRpcStyle(), IsEncoded());
            ExportKnownTypes();

            for (int messageIndex = 0; messageIndex < operation.Messages.Count; messageIndex++)
                ExportMessage(messageIndex, state);

            if (!operation.IsOneWay)
            {
                ExportFaults(state);
            }

            foreach (XmlSchema schema in exporter.GeneratedXmlSchemas.Schemas())
                EnsureXsdImport(schema.TargetNamespace, contractContext.WsdlPortType.ServiceDescription);
        }

        void ExportMessage(int messageIndex, object state)
        {
            try
            {
                MessageDescription description = operation.Messages[messageIndex];
                WsdlNS.Message wsdlMessage;

                if (CreateMessage(description, messageIndex, out wsdlMessage))
                {
                    if (description.IsUntypedMessage)
                    {
                        ExportAnyMessage(wsdlMessage, description.Body.ReturnValue ?? description.Body.Parts[0]);
                        return;
                    }
                    bool isRequest = (messageIndex == 0);
                    StreamFormatter streamFormatter = StreamFormatter.Create(description, operation.Name, isRequest);
                    if (streamFormatter != null)
                    {
                        ExportStreamBody(wsdlMessage, streamFormatter.WrapperName, streamFormatter.WrapperNamespace, streamFormatter.PartName, streamFormatter.PartNamespace, IsRpcStyle(), false /*IsOperationInherited(operation)*/);
                    }
                    else
                    {
                        ExportBody(messageIndex, state);
                    }
                }
                if (!description.IsUntypedMessage)
                {
                    ExportHeaders(messageIndex, state);
                }
            }
            finally
            {
                Compile();
            }
        }

        protected virtual void ExportFaults(object state)
        {
            foreach (FaultDescription fault in operation.Faults)
            {
                ExportFault(fault);
            }
        }

        protected bool IsOperationInherited()
        {
            return operation.DeclaringContract != contractContext.Contract;
        }

        void ExportAnyMessage(WsdlNS.Message message, MessagePartDescription part)
        {
            XmlSchemaSet schemas = this.exporter.GeneratedXmlSchemas;
            XmlSchema schema = SchemaHelper.GetSchema(DataContractSerializerMessageContractImporter.GenericMessageTypeName.Namespace, schemas);

            if (!schema.SchemaTypes.Contains(DataContractSerializerMessageContractImporter.GenericMessageTypeName))
            {
                XmlSchemaComplexType genericMessageType = new XmlSchemaComplexType();
                genericMessageType.Name = DataContractSerializerMessageContractImporter.GenericMessageTypeName.Name;
                XmlSchemaSequence bodySequence = new XmlSchemaSequence();
                genericMessageType.Particle = bodySequence;

                XmlSchemaAny anyElement = new XmlSchemaAny();
                anyElement.MinOccurs = 0;
                anyElement.MaxOccurs = decimal.MaxValue;
                anyElement.Namespace = "##any";

                bodySequence.Items.Add(anyElement);

                SchemaHelper.AddTypeToSchema(genericMessageType, schema, schemas);
            }
            string partName = string.IsNullOrEmpty(part.UniquePartName) ? part.Name : part.UniquePartName;
            WsdlNS.MessagePart wsdlPart = AddMessagePart(message, partName, XmlQualifiedName.Empty, DataContractSerializerMessageContractImporter.GenericMessageTypeName);
            part.UniquePartName = wsdlPart.Name;
        }

        protected void ExportStreamBody(WsdlNS.Message message, string wrapperName, string wrapperNs, string partName, string partNs, bool isRpc, bool skipSchemaExport)
        {
            XmlSchemaSet schemas = this.exporter.GeneratedXmlSchemas;
            XmlSchema schema = SchemaHelper.GetSchema(DataContractSerializerMessageContractImporter.StreamBodyTypeName.Namespace, schemas);
            if (!schema.SchemaTypes.Contains(DataContractSerializerMessageContractImporter.StreamBodyTypeName))
            {
                XmlSchemaSimpleType streamBodyType = new XmlSchemaSimpleType();
                streamBodyType.Name = DataContractSerializerMessageContractImporter.StreamBodyTypeName.Name;
                XmlSchemaSimpleTypeRestriction contentRestriction = new XmlSchemaSimpleTypeRestriction();
                contentRestriction.BaseTypeName = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Base64Binary).QualifiedName;
                streamBodyType.Content = contentRestriction;
                SchemaHelper.AddTypeToSchema(streamBodyType, schema, schemas);
            }
            XmlSchemaSequence wrapperSequence = null;
            if (!isRpc && wrapperName != null)
                wrapperSequence = ExportWrappedPart(message, wrapperName, wrapperNs, schemas, skipSchemaExport);
            MessagePartDescription streamPart = new MessagePartDescription(partName, partNs);
            ExportMessagePart(message, streamPart, DataContractSerializerMessageContractImporter.StreamBodyTypeName, null/*xsdType*/, false/*isOptional*/, false/*isNillable*/, skipSchemaExport, !isRpc, wrapperNs, wrapperSequence, schemas);
        }

        void ExportFault(FaultDescription fault)
        {
            WsdlNS.Message faultMessage = new WsdlNS.Message();
            faultMessage.Name = GetFaultMessageName(fault.Name);

            XmlQualifiedName elementName = ExportFaultElement(fault);
            this.contractContext.WsdlPortType.ServiceDescription.Messages.Add(faultMessage);
            AddMessagePart(faultMessage, "detail", elementName, null);

            // create a wsdl:fault to put inside the wsdl:portType/wsdl:operation
            WsdlNS.OperationFault operationFault = contractContext.GetOperationFault(fault);
            WsdlExporter.WSAddressingHelper.AddActionAttribute(fault.Action, operationFault, this.exporter.PolicyVersion);
            operationFault.Message = new XmlQualifiedName(faultMessage.Name, faultMessage.ServiceDescription.TargetNamespace);
        }

        XmlQualifiedName ExportFaultElement(FaultDescription fault)
        {
            XmlSchemaType xsdType;
            XmlQualifiedName typeName = ExportType(fault.DetailType, fault.Name, operation.Name, out xsdType);
            XmlQualifiedName elementName;
            if (XmlName.IsNullOrEmpty(fault.ElementName))
            {
                elementName = DataContractExporter.GetRootElementName(fault.DetailType);
                if (elementName == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxFaultTypeAnonymous, operation.Name, fault.DetailType.FullName)));
            }
            else
                elementName = new XmlQualifiedName(fault.ElementName.EncodedName, fault.Namespace);
            ExportGlobalElement(elementName.Name, elementName.Namespace, true/*isNillable*/, typeName, xsdType, this.exporter.GeneratedXmlSchemas);
            return elementName;
        }

        protected XsdDataContractExporter DataContractExporter
        {
            get
            {
                object dataContractExporter;
                if (!exporter.State.TryGetValue(typeof(XsdDataContractExporter), out dataContractExporter))
                {
                    dataContractExporter = new XsdDataContractExporter(this.exporter.GeneratedXmlSchemas);
                    exporter.State.Add(typeof(XsdDataContractExporter), dataContractExporter);
                }
                return (XsdDataContractExporter)dataContractExporter;
            }
        }

        protected XmlQualifiedName ExportType(Type type, string partName, string operationName, out XmlSchemaType xsdType)
        {
            xsdType = null;
            if (type == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxExportMustHaveType, operationName, partName)));
            if (type == typeof(void))
                return null;

            DataContractExporter.Export(type);
            XmlQualifiedName typeName = DataContractExporter.GetSchemaTypeName(type);
            if (IsNullOrEmpty(typeName))
                xsdType = DataContractExporter.GetSchemaType(type);
            return typeName;
        }

        protected XmlSchemaSet SchemaSet
        {
            get
            {
                return exporter.GeneratedXmlSchemas;
            }
        }

        static protected WsdlNS.MessagePart AddMessagePart(WsdlNS.Message message, string partName, XmlQualifiedName elementName, XmlQualifiedName typeName)
        {
            if (message.Parts[partName] != null)
            {
                if (IsNullOrEmpty(elementName))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SFxPartNameMustBeUniqueInRpc, partName)));
                int i = 1;
                while (message.Parts[partName + i] != null)
                {
                    if (i == Int32.MaxValue)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SFxTooManyPartsWithSameName, partName)));
                    i++;
                }
                partName = partName + i.ToString(CultureInfo.InvariantCulture);
            }
            WsdlNS.MessagePart part = new WsdlNS.MessagePart();
            part.Name = partName;
            part.Element = elementName;
            part.Type = typeName;
            message.Parts.Add(part);
            EnsureXsdImport(IsNullOrEmpty(elementName) ? typeName.Namespace : elementName.Namespace, message.ServiceDescription);
            return part;
        }

        static void EnsureXsdImport(string ns, WsdlNS.ServiceDescription wsdl)
        {
            string refNs = wsdl.TargetNamespace;
            if (!refNs.EndsWith("/", StringComparison.Ordinal))
                refNs = refNs + "/Imports";
            else
                refNs += "Imports";
            if (refNs == ns)
                refNs = wsdl.TargetNamespace;

            XmlSchema xsd = GetContainedSchema(wsdl, refNs);
            if (xsd != null)
            {
                foreach (object include in xsd.Includes)
                {
                    XmlSchemaImport import = include as XmlSchemaImport;
                    if (import != null && SchemaHelper.NamespacesEqual(import.Namespace, ns))
                        return;
                }
            }
            else
            {
                xsd = new XmlSchema();
                xsd.TargetNamespace = refNs;
                wsdl.Types.Schemas.Add(xsd);
            }

            XmlSchemaImport imp = new XmlSchemaImport();
            if (ns != null && ns.Length > 0)
                imp.Namespace = ns;
            xsd.Includes.Add(imp);
        }

        static XmlSchema GetContainedSchema(WsdlNS.ServiceDescription wsdl, string ns)
        {
            foreach (XmlSchema xsd in wsdl.Types.Schemas)
                if (SchemaHelper.NamespacesEqual(xsd.TargetNamespace, ns))
                    return xsd;

            return null;
        }


        static protected bool IsNullOrEmpty(XmlQualifiedName qname)
        {
            return qname == null || qname.IsEmpty;
        }

        protected void ExportGlobalElement(string elementName, string elementNs, bool isNillable, XmlQualifiedName typeName, XmlSchemaType xsdType, XmlSchemaSet schemaSet)
        {
#if DEBUG
            Fx.Assert(NamingHelper.IsValidNCName(elementName), "Name value has to be a valid NCName.");
            if (xsdType == null)
                Fx.Assert(NamingHelper.IsValidNCName(typeName.Name), "Name value has to be a valid NCName.");
#endif
            XmlSchemaElement element = new XmlSchemaElement();
            element.Name = elementName;
            if (xsdType != null)
                element.SchemaType = xsdType;
            else
                element.SchemaTypeName = typeName;
            element.IsNillable = isNillable;
            AddElementToSchema(element, elementNs, schemaSet);
        }

        void ExportLocalElement(string wrapperNs, string elementName, string elementNs, XmlQualifiedName typeName, XmlSchemaType xsdType, bool multiple, bool isOptional, bool isNillable, XmlSchemaSequence sequence, XmlSchemaSet schemaSet)
        {
#if DEBUG
            Fx.Assert(NamingHelper.IsValidNCName(elementName), "Name value has to be a valid NCName.");
            if (xsdType == null)
                Fx.Assert(NamingHelper.IsValidNCName(typeName.Name), "Name value has to be a valid NCName.");
#endif
            XmlSchema schema = SchemaHelper.GetSchema(wrapperNs, schemaSet);
            XmlSchemaElement element = new XmlSchemaElement();
            if (elementNs == wrapperNs)
            {
                element.Name = elementName;
                if (xsdType != null)
                    element.SchemaType = xsdType;
                else
                {
                    element.SchemaTypeName = typeName;
                    SchemaHelper.AddImportToSchema(element.SchemaTypeName.Namespace, schema);
                }
                SchemaHelper.AddElementForm(element, schema);
                element.IsNillable = isNillable;
            }
            else
            {
                element.RefName = new XmlQualifiedName(elementName, elementNs);

                SchemaHelper.AddImportToSchema(elementNs, schema);
                ExportGlobalElement(elementName, elementNs, isNillable, typeName, xsdType, schemaSet);
            }
            if (multiple)
                element.MaxOccurs = Decimal.MaxValue;
            if (isOptional)
                element.MinOccurs = 0;
            sequence.Items.Add(element);
        }

        static readonly XmlSchemaSequence emptySequence = new XmlSchemaSequence();
        protected XmlSchemaSequence ExportWrappedPart(WsdlNS.Message message, string elementName, string elementNs, XmlSchemaSet schemaSet, bool skipSchemaExport)
        {
#if DEBUG
            Fx.Assert(NamingHelper.IsValidNCName(elementName), "Name value has to be a valid NCName.");
#endif
            AddMessagePart(message, "parameters", new XmlQualifiedName(elementName, elementNs), XmlQualifiedName.Empty);
            if (skipSchemaExport)
                return emptySequence; //return empty to denote it is wrapped part


            XmlSchemaElement wrapperGlobalElement = new XmlSchemaElement();
            wrapperGlobalElement.Name = elementName;

            XmlSchemaComplexType wrapperType = new XmlSchemaComplexType();
            wrapperGlobalElement.SchemaType = wrapperType; // generating an anonymous type for wrapper

            XmlSchemaSequence rootSequence = new XmlSchemaSequence();
            wrapperType.Particle = rootSequence;

            AddElementToSchema(wrapperGlobalElement, elementNs, schemaSet);

            return rootSequence;
        }

        protected bool CreateMessage(MessageDescription message, int messageIndex, out WsdlNS.Message wsdlMessage)
        {
            wsdlMessage = null;
            bool isNewMessage = true;

            if (ExportedMessages.WsdlMessages.ContainsKey(new MessageDescriptionDictionaryKey(contractContext.Contract, message)))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MultipleCallsToExportContractWithSameContract)));

            TypedMessageKey typedMessageKey = null;
            OperationMessageKey messageKey = null;
            if (message.IsTypedMessage)
            {
                typedMessageKey = new TypedMessageKey(message.MessageType, operation.DeclaringContract.Namespace, this.GetExtensionData());
                if (ExportedMessages.TypedMessages.TryGetValue(typedMessageKey, out wsdlMessage))
                    isNewMessage = false;
            }
            else if (operation.OperationMethod != null)
            {
                messageKey = new OperationMessageKey(operation, messageIndex);
                if (ExportedMessages.ParameterMessages.TryGetValue(messageKey, out wsdlMessage))
                    isNewMessage = false;
            }

            WsdlNS.ServiceDescription wsdl = contractContext.WsdlPortType.ServiceDescription;
            if (isNewMessage)
            {
                wsdlMessage = new WsdlNS.Message();
                wsdlMessage.Name = GetMessageName(message);
                wsdl.Messages.Add(wsdlMessage);
                if (message.IsTypedMessage)
                    ExportedMessages.TypedMessages.Add(typedMessageKey, wsdlMessage);
                else if (messageKey != null)
                    ExportedMessages.ParameterMessages.Add(messageKey, wsdlMessage);
            }

            //Add Name to OperationMessage
            WsdlNS.OperationMessage wsdlOperationMessage = contractContext.GetOperationMessage(message);
            wsdlOperationMessage.Message = new XmlQualifiedName(wsdlMessage.Name, wsdlMessage.ServiceDescription.TargetNamespace);
            this.ExportedMessages.WsdlMessages.Add(new MessageDescriptionDictionaryKey(contractContext.Contract, message), wsdlMessage);

            return isNewMessage;
        }


        protected bool CreateHeaderMessage(MessageDescription message, out WsdlNS.Message wsdlMessage)
        {
            wsdlMessage = null;

            if (ExportedMessages.WsdlHeaderMessages.ContainsKey(new MessageDescriptionDictionaryKey(contractContext.Contract, message)))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MultipleCallsToExportContractWithSameContract)));

            TypedMessageKey typedMessageKey = null;
            if (message.IsTypedMessage)
            {
                typedMessageKey = new TypedMessageKey(message.MessageType, operation.DeclaringContract.Namespace, GetExtensionData());
                if (ExportedMessages.TypedHeaderMessages.TryGetValue(typedMessageKey, out wsdlMessage))
                {
                    this.ExportedMessages.WsdlHeaderMessages.Add(new MessageDescriptionDictionaryKey(contractContext.Contract, message), wsdlMessage);
                    return false;
                }
            }

            string messageName = GetHeaderMessageName(message);
            wsdlMessage = new WsdlNS.Message();
            wsdlMessage.Name = messageName;
            contractContext.WsdlPortType.ServiceDescription.Messages.Add(wsdlMessage);
            if (message.IsTypedMessage)
                ExportedMessages.TypedHeaderMessages.Add(typedMessageKey, wsdlMessage);

            this.ExportedMessages.WsdlHeaderMessages.Add(new MessageDescriptionDictionaryKey(contractContext.Contract, message), wsdlMessage);

            return true;
        }

        string GetMessageName(MessageDescription messageDescription)
        {
            string messageNameBase = XmlName.IsNullOrEmpty(messageDescription.MessageName) ? null : messageDescription.MessageName.EncodedName;

            //If there wasn't one in the Message Description we create one.
            if (string.IsNullOrEmpty(messageNameBase))
            {
                string portTypeName = contractContext.WsdlPortType.Name;
                string operationName = contractContext.GetOperation(operation).Name;

                string callbackString = operation.IsServerInitiated() ? "Callback" : string.Empty;
                // [....]: composing names have potential problem of generating name that looks like an encoded name, consider avoiding '_'
                if (messageDescription.Direction == MessageDirection.Input)
                    messageNameBase = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                        "{0}_{1}_Input{2}Message", portTypeName, operationName, callbackString);
                else
                    messageNameBase = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                        "{0}_{1}_Output{2}Message", portTypeName, operationName, callbackString);
            }

            WsdlNS.ServiceDescription wsdl = contractContext.WsdlPortType.ServiceDescription;
            return GetUniqueMessageName(wsdl, messageNameBase);
        }

        string GetHeaderMessageName(MessageDescription messageDescription)
        {
            WsdlNS.Message wsdlBodyMessage = this.ExportedMessages.WsdlMessages[new MessageDescriptionDictionaryKey(this.contractContext.Contract, messageDescription)];

            string messageNameBase = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                        "{0}_Headers", wsdlBodyMessage.Name);

            WsdlNS.ServiceDescription wsdl = contractContext.WsdlPortType.ServiceDescription;
            return GetUniqueMessageName(wsdl, messageNameBase);
        }

        protected string GetFaultMessageName(string faultName)
        {
            string portTypeName = contractContext.WsdlPortType.Name;
            string operationName = contractContext.GetOperation(operation).Name;
            // [....]: composing names have potential problem of generating name that looks like an encoded name, consider avoiding '_'
            string faultNameBase = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_{1}_{2}_FaultMessage", portTypeName, operationName, faultName);

            WsdlNS.ServiceDescription wsdl = contractContext.WsdlPortType.ServiceDescription;
            return GetUniqueMessageName(wsdl, faultNameBase);
        }

        static bool DoesMessageNameExist(string messageName, object wsdlObject)
        {
            return ((WsdlNS.ServiceDescription)wsdlObject).Messages[messageName] != null;
        }
        string GetUniqueMessageName(WsdlNS.ServiceDescription wsdl, string messageNameBase)
        {
            return NamingHelper.GetUniqueName(messageNameBase, DoesMessageNameExist, wsdl);
        }

        protected void ExportMessagePart(WsdlNS.Message message, MessagePartDescription part, XmlQualifiedName typeName, XmlSchemaType xsdType, bool isOptional, bool isNillable, bool skipSchemaExport, bool generateElement, string wrapperNs, XmlSchemaSequence wrapperSequence, XmlSchemaSet schemaSet)
        {
            if (IsNullOrEmpty(typeName) && xsdType == null)
                return;
#if DEBUG
            if (xsdType == null)
                Fx.Assert(NamingHelper.IsValidNCName(typeName.Name), "Name value has to be a valid NCName.");
#endif
            string elementName = part.Name;
            string partName = string.IsNullOrEmpty(part.UniquePartName) ? elementName : part.UniquePartName;

            WsdlNS.MessagePart wsdlPart = null;
            if (generateElement)
            {
                if (wrapperSequence != null)
                {
                    if (!skipSchemaExport)
                        ExportLocalElement(wrapperNs, partName, part.Namespace, typeName, xsdType, part.Multiple, isOptional, isNillable, wrapperSequence, schemaSet);
                }
                else
                {
                    if (!skipSchemaExport)
                        ExportGlobalElement(elementName, part.Namespace, isNillable, typeName, xsdType, schemaSet);
                    wsdlPart = AddMessagePart(message, partName, new XmlQualifiedName(elementName, part.Namespace), XmlQualifiedName.Empty);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(typeName.Name))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxAnonymousTypeNotSupported, message.Name, partName)));

                wsdlPart = AddMessagePart(message, partName, XmlQualifiedName.Empty, typeName);
            }
            if (wsdlPart != null)
            {
                part.UniquePartName = wsdlPart.Name;
            }
        }

        protected void AddParameterOrder(MessageDescription message)
        {
            if (operation == null)
                return;

            WsdlNS.Operation wsdlOperation = contractContext.GetOperation(operation);
            if (wsdlOperation != null)
            {
                if (wsdlOperation.ParameterOrder == null)
                {
                    wsdlOperation.ParameterOrder = new string[GetParameterCount()];
                }

                if (wsdlOperation.ParameterOrder.Length == 0)
                    return;

                foreach (MessagePartDescription part in message.Body.Parts)
                {
                    ParameterInfo paramInfo = part.AdditionalAttributesProvider as ParameterInfo;
                    if (paramInfo != null && paramInfo.Position >= 0)
                        wsdlOperation.ParameterOrder[paramInfo.Position] = part.Name;
                }
            }
        }

        int GetParameterCount()
        {
            int count = -1;
            foreach (MessageDescription message in operation.Messages)
            {
                foreach (MessagePartDescription part in message.Body.Parts)
                {
                    ParameterInfo paramInfo = part.AdditionalAttributesProvider as ParameterInfo;
                    if (paramInfo == null)
                        return 0;
                    if (count < paramInfo.Position)
                        count = paramInfo.Position;
                }
            }
            return count + 1;
        }

        protected virtual void Compile()
        {
            foreach (XmlSchema schema in SchemaSet.Schemas())
                SchemaSet.Reprocess(schema);
            SchemaHelper.Compile(SchemaSet, exporter.Errors);
        }

        class MessageBindingExporter
        {
            WsdlEndpointConversionContext endpointContext;
            MessageExportContext exportedMessages;
            EnvelopeVersion soapVersion;
            WsdlExporter exporter;


            internal MessageBindingExporter(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext)
            {
                this.endpointContext = endpointContext;
                this.exportedMessages = (MessageExportContext)exporter.State[typeof(MessageExportContext)];
                this.soapVersion = SoapHelper.GetSoapVersion(endpointContext.WsdlBinding);
                this.exporter = exporter;
            }

            internal void ExportMessageBinding(OperationDescription operation, Type messageContractExporterType)
            {

                WsdlNS.OperationBinding wsdlOperationBinding = endpointContext.GetOperationBinding(operation);


                bool isRpc, isEncoded;
                if (!GetStyleAndUse(operation, messageContractExporterType, out isRpc, out isEncoded))
                    return;

                WsdlNS.SoapOperationBinding soapOperationBinding = SoapHelper.GetOrCreateSoapOperationBinding(endpointContext, operation, exporter);

                if (soapOperationBinding == null)
                    return;

                soapOperationBinding.Style = isRpc ? WsdlNS.SoapBindingStyle.Rpc : WsdlNS.SoapBindingStyle.Document;
                if (isRpc)
                {
                    WsdlNS.SoapBinding soapBinding = (WsdlNS.SoapBinding)endpointContext.WsdlBinding.Extensions.Find(typeof(WsdlNS.SoapBinding));
                    soapBinding.Style = soapOperationBinding.Style;
                }
                soapOperationBinding.SoapAction = operation.Messages[0].Action;

                foreach (MessageDescription message in operation.Messages)
                {
                    WsdlNS.MessageBinding wsdlMessageBinding = endpointContext.GetMessageBinding(message);

                    WsdlNS.Message headerMessage;
                    if (this.exportedMessages.WsdlHeaderMessages.TryGetValue(new MessageDescriptionDictionaryKey(this.endpointContext.Endpoint.Contract, message), out headerMessage))
                    {
                        XmlQualifiedName wsdlHeaderMessageName = new XmlQualifiedName(headerMessage.Name, headerMessage.ServiceDescription.TargetNamespace);

                        foreach (MessageHeaderDescription header in message.Headers)
                        {
                            if (header.IsUnknownHeaderCollection)
                                continue;
                            ExportMessageHeaderBinding(header, wsdlHeaderMessageName, isEncoded, wsdlMessageBinding);
                        }
                    }

                    ExportMessageBodyBinding(message, isRpc, isEncoded, wsdlMessageBinding);
                }

                foreach (FaultDescription fault in operation.Faults)
                {
                    ExportFaultBinding(fault, isEncoded, wsdlOperationBinding);
                }
            }

            void ExportFaultBinding(FaultDescription fault, bool isEncoded, WsdlNS.OperationBinding operationBinding)
            {
                SoapHelper.CreateSoapFaultBinding(fault.Name, endpointContext, endpointContext.GetFaultBinding(fault), isEncoded);
            }

            void ExportMessageBodyBinding(MessageDescription messageDescription, bool isRpc, bool isEncoded, WsdlNS.MessageBinding messageBinding)
            {
                WsdlNS.SoapBodyBinding bodyBinding = SoapHelper.GetOrCreateSoapBodyBinding(endpointContext, messageBinding, exporter);

                if (bodyBinding == null)
                    return;

                bodyBinding.Use = isEncoded ? WsdlNS.SoapBindingUse.Encoded : WsdlNS.SoapBindingUse.Literal;
                if (isRpc)
                {
                    string ns;
                    if (!ExportedMessages.WrapperNamespaces.TryGetValue(new MessageDescriptionDictionaryKey(endpointContext.ContractConversionContext.Contract, messageDescription), out ns))
                        ns = messageDescription.Body.WrapperNamespace;
                    bodyBinding.Namespace = ns;
                }
                if (isEncoded)
                    bodyBinding.Encoding = XmlSerializerOperationFormatter.GetEncoding(soapVersion);
            }

            void ExportMessageHeaderBinding(MessageHeaderDescription header, XmlQualifiedName messageName, bool isEncoded, WsdlNS.MessageBinding messageBinding)
            {
#if DEBUG
                Fx.Assert(NamingHelper.IsValidNCName(messageName.Name), "Name value has to be a valid NCName.");
#endif
                WsdlNS.SoapHeaderBinding headerBinding = SoapHelper.CreateSoapHeaderBinding(endpointContext, messageBinding);
                headerBinding.Part = string.IsNullOrEmpty(header.UniquePartName) ? header.Name : header.UniquePartName;
                headerBinding.Message = messageName;
                headerBinding.Use = isEncoded ? WsdlNS.SoapBindingUse.Encoded : WsdlNS.SoapBindingUse.Literal;
                if (isEncoded)
                    headerBinding.Encoding = XmlSerializerOperationFormatter.GetEncoding(soapVersion);
            }

            static bool GetStyleAndUse(OperationDescription operation, Type messageContractExporterType, out bool isRpc, out bool isEncoded)
            {
                isRpc = isEncoded = false;
                if (messageContractExporterType == typeof(DataContractSerializerMessageContractExporter) || messageContractExporterType == null)
                {
                    DataContractSerializerOperationBehavior dataContractSerializerBehavior = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                    if (dataContractSerializerBehavior != null)
                    {
                        isRpc = dataContractSerializerBehavior.DataContractFormatAttribute.Style == OperationFormatStyle.Rpc;
                        isEncoded = false;
                        return true;
                    }
                    if (messageContractExporterType == typeof(DataContractSerializerMessageContractExporter))
                        return false;
                }
                if (messageContractExporterType == typeof(XmlSerializerMessageContractExporter) || messageContractExporterType == null)
                {
                    XmlSerializerOperationBehavior xmlSerializerBehavior = operation.Behaviors.Find<XmlSerializerOperationBehavior>();
                    if (xmlSerializerBehavior != null)
                    {
                        isRpc = xmlSerializerBehavior.XmlSerializerFormatAttribute.Style == OperationFormatStyle.Rpc;
                        isEncoded = xmlSerializerBehavior.XmlSerializerFormatAttribute.IsEncoded;
                        return true;
                    }
                    return false;
                }
                return false;
            }

            MessageExportContext ExportedMessages
            {
                get { return GetMessageExportContext(exporter); }
            }
        }

        protected class MessageExportContext
        {
            readonly internal Dictionary<MessageDescriptionDictionaryKey, WsdlNS.Message> WsdlMessages = new Dictionary<MessageDescriptionDictionaryKey, System.Web.Services.Description.Message>();
            readonly internal Dictionary<MessageDescriptionDictionaryKey, WsdlNS.Message> WsdlHeaderMessages = new Dictionary<MessageDescriptionDictionaryKey, System.Web.Services.Description.Message>();
            readonly internal Dictionary<MessageDescriptionDictionaryKey, string> WrapperNamespaces = new Dictionary<MessageDescriptionDictionaryKey, string>();
            readonly internal Dictionary<TypedMessageKey, WsdlNS.Message> TypedMessages = new Dictionary<TypedMessageKey, WsdlNS.Message>();
            readonly internal Dictionary<TypedMessageKey, WsdlNS.Message> TypedHeaderMessages = new Dictionary<TypedMessageKey, WsdlNS.Message>();
            readonly internal Dictionary<OperationMessageKey, WsdlNS.Message> ParameterMessages = new Dictionary<OperationMessageKey, WsdlNS.Message>();
            readonly internal Dictionary<XmlQualifiedName, OperationElement> ElementTypes = new Dictionary<XmlQualifiedName, OperationElement>();
        }

        protected sealed class MessageDescriptionDictionaryKey
        {
            public readonly ContractDescription Contract;
            public readonly MessageDescription MessageDescription;

            public MessageDescriptionDictionaryKey(ContractDescription contract, MessageDescription MessageDescription)
            {
                this.Contract = contract;
                this.MessageDescription = MessageDescription;
            }

            public override bool Equals(object obj)
            {
                MessageDescriptionDictionaryKey key = obj as MessageDescriptionDictionaryKey;
                if (key != null && key.MessageDescription == this.MessageDescription && key.Contract == this.Contract)
                    return true;
                return false;
            }

            public override int GetHashCode()
            {
                return this.Contract.GetHashCode() ^ this.MessageDescription.GetHashCode();
            }
        }

        internal sealed class TypedMessageKey
        {
            Type type;
            string contractNS;
            object extensionData;

            public TypedMessageKey(Type type, string contractNS, object extensionData)
            {
                this.type = type;
                this.contractNS = contractNS;
                this.extensionData = extensionData;
            }

            public override bool Equals(object obj)
            {
                TypedMessageKey key = obj as TypedMessageKey;
                if (key != null && key.type == this.type &&
                    key.contractNS == this.contractNS &&
                    key.extensionData.Equals(this.extensionData))
                    return true;
                return false;
            }

            [SuppressMessage(FxCop.Category.Usage, "CA2303:FlagTypeGetHashCode", Justification = "The hashcode is not used for identity purposes for embedded types.")]
            public override int GetHashCode()
            {
                return type.GetHashCode();
            }
        }

        internal sealed class OperationMessageKey
        {
            MethodInfo methodInfo;
            int messageIndex;
            ContractDescription declaringContract;

            public OperationMessageKey(OperationDescription operation, int messageIndex)
            {
                this.methodInfo = operation.OperationMethod;
                this.messageIndex = messageIndex;
                this.declaringContract = operation.DeclaringContract;
            }

            public override bool Equals(object obj)
            {
                OperationMessageKey key = obj as OperationMessageKey;
                if (key != null && key.methodInfo == this.methodInfo &&
                    key.messageIndex == this.messageIndex &&
                    key.declaringContract.Name == this.declaringContract.Name &&
                    key.declaringContract.Namespace == this.declaringContract.Namespace)
                    return true;
                return false;
            }

            public override int GetHashCode()
            {
                return methodInfo.GetHashCode() ^ messageIndex;
            }
        }

        internal sealed class OperationElement
        {
            XmlSchemaElement element;
            OperationDescription operation;
            internal OperationElement(XmlSchemaElement element, OperationDescription operation)
            {
                this.element = element;
                this.operation = operation;
            }
            internal XmlSchemaElement Element { get { return element; } }
            internal OperationDescription Operation { get { return operation; } }
        }
    }

    class DataContractSerializerMessageContractExporter : MessageContractExporter
    {
        internal DataContractSerializerMessageContractExporter(WsdlExporter exporter, WsdlContractConversionContext context, OperationDescription operation, IOperationBehavior extension)
            : base(exporter, context, operation, extension)
        {
        }

        protected override void Compile()
        {
            XmlSchema wsdl = StockSchemas.CreateWsdl();
            XmlSchema soap = StockSchemas.CreateSoap();
            XmlSchema soapEncoding = StockSchemas.CreateSoapEncoding();
            XmlSchema fakeXsdSchema = StockSchemas.CreateFakeXsdSchema();

            SchemaSet.Add(wsdl);
            SchemaSet.Add(soap);
            SchemaSet.Add(soapEncoding);
            SchemaSet.Add(fakeXsdSchema);
            base.Compile();
            SchemaSet.Remove(wsdl);
            SchemaSet.Remove(soap);
            SchemaSet.Remove(soapEncoding);
            SchemaSet.Remove(fakeXsdSchema);
        }

        protected override bool IsRpcStyle()
        {
            return ((DataContractSerializerOperationBehavior)extension).DataContractFormatAttribute.Style == OperationFormatStyle.Rpc;
        }

        protected override bool IsEncoded()
        {
            return false;
        }

        protected override object OnExportMessageContract()
        {
            return null;
        }

        protected override void ExportHeaders(int messageIndex, object state)
        {
            MessageDescription description = operation.Messages[messageIndex];

            if (description.Headers.Count > 0)
            {
                WsdlNS.Message wsdlMessage;
                if (CreateHeaderMessage(description, out wsdlMessage))
                {
                    foreach (MessageHeaderDescription header in description.Headers)
                    {
                        if (header.IsUnknownHeaderCollection)
                            continue;
                        XmlSchemaType xsdType;
                        bool isQueryable;
                        Type dataContractType = DataContractSerializerOperationFormatter.GetSubstituteDataContractType(header.Type, out isQueryable);
                        XmlQualifiedName typeName = ExportType(dataContractType, header.Name, operation.Name, out xsdType);
                        ExportMessagePart(wsdlMessage, header, typeName, xsdType, true/*isOptional*/, IsTypeNullable(header.Type), false/*IsOperationInherited(operation)*/, true /*generateElement*/, null/*wrapperNamespace*/, null/*wrapperSequence*/, SchemaSet);
                    }
                }
            }
        }

        static internal bool IsTypeNullable(Type type)
        {
            return !type.IsValueType ||
                    (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        protected override void ExportBody(int messageIndex, object state)
        {
            MessageDescription description = operation.Messages[messageIndex];
            WsdlNS.Message wsdlMessage = this.ExportedMessages.WsdlMessages[new MessageDescriptionDictionaryKey(this.contractContext.Contract, description)];

            DataContractFormatAttribute dataContractFormatAttribute = ((DataContractSerializerOperationBehavior)extension).DataContractFormatAttribute;
            XmlSchemaSequence wrapperSequence = null;
            bool isWrapped = description.Body.WrapperName != null;
            //bool isOperationInherited = IsOperationInherited(operation);
            if (dataContractFormatAttribute.Style == OperationFormatStyle.Document && isWrapped)
                wrapperSequence = ExportWrappedPart(wsdlMessage, description.Body.WrapperName, description.Body.WrapperNamespace, SchemaSet, false /*isOperationInherited*/);
            XmlSchemaType xsdType;
            if (OperationFormatter.IsValidReturnValue(description.Body.ReturnValue))
            {
                bool isQueryable;
                Type dataContractType = DataContractSerializerOperationFormatter.GetSubstituteDataContractType(description.Body.ReturnValue.Type, out isQueryable);
                XmlQualifiedName typeName = ExportType(dataContractType, description.Body.ReturnValue.Name, operation.Name, out xsdType);
                ExportMessagePart(wsdlMessage, description.Body.ReturnValue, typeName, xsdType, true/*isOptional*/, IsTypeNullable(description.Body.ReturnValue.Type), false/*isOperationInherited*/, dataContractFormatAttribute.Style != OperationFormatStyle.Rpc, description.Body.WrapperNamespace, wrapperSequence, SchemaSet);
            }

            foreach (MessagePartDescription bodyPart in description.Body.Parts)
            {
                bool isQueryable;
                Type dataContractType = DataContractSerializerOperationFormatter.GetSubstituteDataContractType(bodyPart.Type, out isQueryable);
                XmlQualifiedName typeName = ExportType(dataContractType, bodyPart.Name, operation.Name, out xsdType);
                ExportMessagePart(wsdlMessage, bodyPart, typeName, xsdType, true/*isOptional*/, IsTypeNullable(bodyPart.Type), false/*isOperationInherited*/, dataContractFormatAttribute.Style != OperationFormatStyle.Rpc, description.Body.WrapperNamespace, wrapperSequence, SchemaSet);
            }
            if (dataContractFormatAttribute.Style == OperationFormatStyle.Rpc)
            {
                AddParameterOrder(description);
            }
        }

        protected override void ExportKnownTypes()
        {
            foreach (Type knownType in operation.KnownTypes)
            {
                DataContractExporter.Export(knownType);
            }
        }

        protected override object GetExtensionData()
        {
            return new ExtensionData(((DataContractSerializerOperationBehavior)extension).DataContractFormatAttribute);
        }
        class ExtensionData
        {
            DataContractFormatAttribute dcFormatAttr;
            internal ExtensionData(DataContractFormatAttribute dcFormatAttr)
            {
                this.dcFormatAttr = dcFormatAttr;
            }

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(dcFormatAttr, obj))
                    return true;
                ExtensionData otherExtensionData = obj as ExtensionData;
                if (otherExtensionData == null)
                    return false;
                return dcFormatAttr.Style == otherExtensionData.dcFormatAttr.Style;
            }

            public override int GetHashCode()
            {
                return 1; //This is never called
            }
        }
    }

    class XmlSerializerMessageContractExporter : MessageContractExporter
    {
        internal XmlSerializerMessageContractExporter(WsdlExporter exporter, WsdlContractConversionContext context, OperationDescription operation, IOperationBehavior extension)
            : base(exporter, context, operation, extension)
        {
        }

        protected override bool IsRpcStyle()
        {
            return ((XmlSerializerOperationBehavior)extension).XmlSerializerFormatAttribute.Style == OperationFormatStyle.Rpc;
        }

        protected override bool IsEncoded()
        {
            return ((XmlSerializerOperationBehavior)extension).XmlSerializerFormatAttribute.IsEncoded;
        }

        protected override object OnExportMessageContract()
        {
            object result = Reflector.ReflectOperation(operation);
            if (result == null)
            {
                // If result is null, that means that XmlSerializerFormatAttribute wasn't available in reflection, 
                // so we need to get it from the XmlSerializerOperationBehavior instead
                XmlSerializerOperationBehavior serializerBehavior = this.extension as XmlSerializerOperationBehavior;
                if (serializerBehavior != null)
                {
                    result = Reflector.ReflectOperation(operation, serializerBehavior.XmlSerializerFormatAttribute);
                }
            }
            return result;
        }

        protected override void ExportHeaders(int messageIndex, object state)
        {
            string portTypeName = contractContext.WsdlPortType.Name;
            string portTypeNs = contractContext.WsdlPortType.ServiceDescription.TargetNamespace;

            MessageDescription description = operation.Messages[messageIndex];
            if (description.Headers.Count > 0)
            {

                XmlSerializerOperationBehavior.Reflector.OperationReflector operationReflector = (XmlSerializerOperationBehavior.Reflector.OperationReflector)state;
                XmlMembersMapping membersMapping = null;
                if (messageIndex == 0)
                {
                    membersMapping = operationReflector.Request.HeadersMapping;
                }
                else
                {
                    membersMapping = operationReflector.Reply.HeadersMapping;
                }

                if (membersMapping != null)
                {
                    WsdlNS.Message wsdlMessage;
                    if (CreateHeaderMessage(description, out wsdlMessage))
                    {
                        ExportMembersMapping(membersMapping, wsdlMessage, false /*IsOperationInherited(operation)*/, operationReflector.IsEncoded, false/*isRpc*/, false/*isWrapped*/, true/*isHeader*/);
                    }
                }
            }
        }

        protected override void ExportBody(int messageIndex, object state)
        {
            MessageDescription description = operation.Messages[messageIndex];
            string portTypeName = contractContext.WsdlPortType.Name;
            string portTypeNs = contractContext.WsdlPortType.ServiceDescription.TargetNamespace;
            MessageDescriptionDictionaryKey key = new MessageDescriptionDictionaryKey(this.contractContext.Contract, description);
            WsdlNS.Message wsdlMessage = this.ExportedMessages.WsdlMessages[key];

            XmlSerializerOperationBehavior.Reflector.OperationReflector operationReflector = (XmlSerializerOperationBehavior.Reflector.OperationReflector)state;
            XmlMembersMapping membersMapping = null;
            if (messageIndex == 0)
            {
                membersMapping = operationReflector.Request.BodyMapping;
            }
            else
            {
                membersMapping = operationReflector.Reply.BodyMapping;
            }

            if (membersMapping != null)
            {
                bool isDocWrapped = !operationReflector.IsRpc && description.Body.WrapperName != null;
                ExportMembersMapping(membersMapping, wsdlMessage, false /*IsOperationInherited(operation)*/, operationReflector.IsEncoded, operationReflector.IsRpc, isDocWrapped, false/*isHeader*/);
                if (operationReflector.IsRpc)
                {
                    AddParameterOrder(operation.Messages[messageIndex]);
                    this.ExportedMessages.WrapperNamespaces.Add(key, membersMapping.Namespace);
                }
            }
        }

        protected override void ExportFaults(object state)
        {
            XmlSerializerOperationBehavior.Reflector.OperationReflector operationReflector = (XmlSerializerOperationBehavior.Reflector.OperationReflector)state;
            if (operationReflector.Attribute.SupportFaults)
            {
                foreach (FaultDescription fault in operation.Faults)
                {
                    ExportFault(fault, operationReflector);
                }
                Compile();
            }
            else
            {
                base.ExportFaults(state);
            }
        }

        void ExportFault(FaultDescription fault, XmlSerializerOperationBehavior.Reflector.OperationReflector operationReflector)
        {
            WsdlNS.Message faultMessage = new WsdlNS.Message();
            faultMessage.Name = GetFaultMessageName(fault.Name);

            XmlQualifiedName elementName = ExportFaultElement(fault, operationReflector);
            this.contractContext.WsdlPortType.ServiceDescription.Messages.Add(faultMessage);
            AddMessagePart(faultMessage, "detail", elementName, null);

            // create a wsdl:fault to put inside the wsdl:portType/wsdl:operation
            WsdlNS.OperationFault operationFault = contractContext.GetOperationFault(fault);
            WsdlExporter.WSAddressingHelper.AddActionAttribute(fault.Action, operationFault, this.exporter.PolicyVersion);
            operationFault.Message = new XmlQualifiedName(faultMessage.Name, faultMessage.ServiceDescription.TargetNamespace);
        }

        XmlQualifiedName ExportFaultElement(FaultDescription fault, XmlSerializerOperationBehavior.Reflector.OperationReflector operationReflector)
        {
            XmlQualifiedName elementName;
            XmlMembersMapping mapping = operationReflector.ImportFaultElement(fault, out elementName);
            if (operationReflector.IsEncoded)
                SoapExporter.ExportMembersMapping(mapping);
            else
                XmlExporter.ExportMembersMapping(mapping);
            return elementName;
        }

        protected override void ExportKnownTypes()
        {
        }

        protected override object GetExtensionData()
        {
            return new ExtensionData(((XmlSerializerOperationBehavior)this.extension).XmlSerializerFormatAttribute);
        }

        class ExtensionData
        {
            XmlSerializerFormatAttribute xsFormatAttr;
            internal ExtensionData(XmlSerializerFormatAttribute xsFormatAttr)
            {
                this.xsFormatAttr = xsFormatAttr;
            }

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(xsFormatAttr, obj))
                    return true;
                ExtensionData otherExtensionData = obj as ExtensionData;
                if (otherExtensionData == null)
                    return false;
                return xsFormatAttr.Style == otherExtensionData.xsFormatAttr.Style &&
                    xsFormatAttr.Use == otherExtensionData.xsFormatAttr.Use;
            }
            public override int GetHashCode()
            {
                return 1; //This is never called
            }
        }
        void ExportMembersMapping(XmlMembersMapping membersMapping, WsdlNS.Message message, bool skipSchemaExport, bool isEncoded, bool isRpc, bool isDocWrapped, bool isHeader)
        {
            if (!skipSchemaExport)
            {
                if (isEncoded)
                    SoapExporter.ExportMembersMapping(membersMapping);
                else
                    XmlExporter.ExportMembersMapping(membersMapping, !isRpc);
            }


            if (isDocWrapped)
            {
                if (isHeader)
                {
                    Fx.Assert("Header cannot be Document Wrapped");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Header cannot be Document Wrapped")));
                }
                AddMessagePart(message, "parameters", new XmlQualifiedName(membersMapping.XsdElementName, membersMapping.Namespace), XmlQualifiedName.Empty);
                return;
            }
            bool generateElement = !isRpc && !isEncoded;
            for (int i = 0; i < membersMapping.Count; i++)
            {
                XmlMemberMapping member = membersMapping[i];

                string partName = (isHeader || generateElement) ? NamingHelper.XmlName(member.MemberName) : member.XsdElementName;
                if (generateElement)
                    AddMessagePart(message, partName, new XmlQualifiedName(member.XsdElementName, member.Namespace), XmlQualifiedName.Empty);
                else
                {
                    if (string.IsNullOrEmpty(member.TypeName))
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxAnonymousTypeNotSupported, message.Name, partName)));

                    AddMessagePart(message, partName, XmlQualifiedName.Empty, new XmlQualifiedName(member.TypeName, member.TypeNamespace));
                }
            }
        }

        XmlSerializerOperationBehavior.Reflector Reflector
        {
            get
            {
                object reflector;
                if (!exporter.State.TryGetValue(typeof(XmlSerializerOperationBehavior.Reflector), out reflector))
                {
                    reflector = new XmlSerializerOperationBehavior.Reflector(contractContext.Contract.Namespace, contractContext.Contract.ContractType);
                    exporter.State.Add(typeof(XmlSerializerOperationBehavior.Reflector), reflector);
                }
                return (XmlSerializerOperationBehavior.Reflector)reflector;
            }
        }

        SoapSchemaExporter SoapExporter
        {
            get
            {
                object soapExporter;
                if (!exporter.State.TryGetValue(typeof(SoapSchemaExporter), out soapExporter))
                {
                    soapExporter = new SoapSchemaExporter(Schemas);
                    exporter.State.Add(typeof(SoapSchemaExporter), soapExporter);
                }
                return (SoapSchemaExporter)soapExporter;
            }
        }

        XmlSchemaExporter XmlExporter
        {
            get
            {
                object xmlExporter;
                if (!exporter.State.TryGetValue(typeof(XmlSchemaExporter), out xmlExporter))
                {
                    xmlExporter = new XmlSchemaExporter(Schemas);
                    exporter.State.Add(typeof(XmlSchemaExporter), xmlExporter);
                }
                return (XmlSchemaExporter)xmlExporter;
            }
        }

        XmlSchemas Schemas
        {
            get
            {
                object schemas;
                if (!exporter.State.TryGetValue(typeof(XmlSchemas), out schemas))
                {
                    schemas = new XmlSchemas();
                    foreach (XmlSchema schema in this.SchemaSet.Schemas())
                        if (!((XmlSchemas)schemas).Contains(schema.TargetNamespace))
                            ((XmlSchemas)schemas).Add(schema);
                    exporter.State.Add(typeof(XmlSchemas), schemas);
                }
                return (XmlSchemas)schemas;
            }
        }

        protected override void Compile()
        {
            XmlSchema wsdl = StockSchemas.CreateWsdl();
            XmlSchema soap = StockSchemas.CreateSoap();
            XmlSchema soapEncoding = StockSchemas.CreateSoapEncoding();
            XmlSchema fakeXsdSchema = StockSchemas.CreateFakeXsdSchema();

            MoveSchemas();
            SchemaSet.Add(wsdl);
            SchemaSet.Add(soap);
            SchemaSet.Add(soapEncoding);
            SchemaSet.Add(fakeXsdSchema);
            base.Compile();
            SchemaSet.Remove(wsdl);
            SchemaSet.Remove(soap);
            SchemaSet.Remove(soapEncoding);
            SchemaSet.Remove(fakeXsdSchema);
        }

        void MoveSchemas()
        {
            XmlSchemas schemas = this.Schemas;
            XmlSchemaSet schemaSet = this.SchemaSet;
            if (schemas != null)
            {
                schemas.Compile(
                     delegate(object sender, ValidationEventArgs args)
                     {
                         SchemaHelper.HandleSchemaValidationError(sender, args, exporter.Errors);
                     },
                     false/*fullCompile*/
                );
                foreach (XmlSchema srcSchema in schemas)
                {
                    if (!schemaSet.Contains(srcSchema))
                    {
                        schemaSet.Add(srcSchema);
                        schemaSet.Reprocess(srcSchema);
                    }
                }
            }
        }
    }

    static class StockSchemas
    {

        internal static XmlSchema CreateWsdl()
        {
            return XmlSchema.Read(new StringReader(wsdl), null);
        }
        internal static XmlSchema CreateSoap()
        {
            return XmlSchema.Read(new StringReader(soap), null);
        }

        internal static XmlSchema CreateSoapEncoding()
        {
            return XmlSchema.Read(new StringReader(soapEncoding), null);
        }

        internal static XmlSchema CreateFakeSoapEncoding()
        {
            return XmlSchema.Read(new StringReader(fakeSoapEncoding), null);
        }

        internal static XmlSchema CreateFakeXsdSchema()
        {
            return XmlSchema.Read(new StringReader(fakeXsd), null);
        }

        internal static XmlSchema CreateFakeXmlSchema()
        {
            return XmlSchema.Read(new StringReader(fakeXmlSchema), null);
        }

        internal static bool IsKnownSchema(string ns)
        {
            return ns == XmlSchema.Namespace || ns == "http://schemas.xmlsoap.org/wsdl/soap/" || ns == "http://schemas.xmlsoap.org/soap/encoding/";
        }

        internal const string WsdlNamespace = "http://schemas.xmlsoap.org/wsdl/";
        internal const string SoapNamespace = "http://schemas.xmlsoap.org/wsdl/soap/";
        internal const string SoapEncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";

        const string wsdl = @"<?xml version='1.0' encoding='UTF-8' ?> 
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
           xmlns:wsdl='http://schemas.xmlsoap.org/wsdl/'
           targetNamespace='http://schemas.xmlsoap.org/wsdl/'
           elementFormDefault='qualified' >
   
  <xs:complexType mixed='true' name='tDocumentation' >
    <xs:sequence>
      <xs:any minOccurs='0' maxOccurs='unbounded' processContents='lax' />
    </xs:sequence>
  </xs:complexType>

  <xs:complexType name='tDocumented' >
    <xs:annotation>
      <xs:documentation>
      This type is extended by  component types to allow them to be documented
      </xs:documentation>
    </xs:annotation>
    <xs:sequence>
      <xs:element name='documentation' type='wsdl:tDocumentation' minOccurs='0' />
    </xs:sequence>
  </xs:complexType>
 <!-- allow extensibility via elements and attributes on all elements swa124 -->
 <xs:complexType name='tExtensibleAttributesDocumented' abstract='true' >
    <xs:complexContent>
      <xs:extension base='wsdl:tDocumented' >
        <xs:annotation>
          <xs:documentation>
          This type is extended by component types to allow attributes from other namespaces to be added.
          </xs:documentation>
        </xs:annotation>
        <xs:sequence>
          <xs:any namespace='##other' minOccurs='0' maxOccurs='unbounded' processContents='lax' />
        </xs:sequence>
        <xs:anyAttribute namespace='##other' processContents='lax' />   
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  <xs:complexType name='tExtensibleDocumented' abstract='true' >
    <xs:complexContent>
      <xs:extension base='wsdl:tDocumented' >
        <xs:annotation>
          <xs:documentation>
          This type is extended by component types to allow elements from other namespaces to be added.
          </xs:documentation>
        </xs:annotation>
        <xs:sequence>
          <xs:any namespace='##other' minOccurs='0' maxOccurs='unbounded' processContents='lax' />
        </xs:sequence>
        <xs:anyAttribute namespace='##other' processContents='lax' />   
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  <!-- original wsdl removed as part of swa124 resolution
  <xs:complexType name='tExtensibleAttributesDocumented' abstract='true' >
    <xs:complexContent>
      <xs:extension base='wsdl:tDocumented' >
        <xs:annotation>
          <xs:documentation>
          This type is extended by component types to allow attributes from other namespaces to be added.
          </xs:documentation>
        </xs:annotation>
        <xs:anyAttribute namespace='##other' processContents='lax' />    
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>

  <xs:complexType name='tExtensibleDocumented' abstract='true' >
    <xs:complexContent>
      <xs:extension base='wsdl:tDocumented' >
        <xs:annotation>
          <xs:documentation>
          This type is extended by component types to allow elements from other namespaces to be added.
          </xs:documentation>
        </xs:annotation>
        <xs:sequence>
          <xs:any namespace='##other' minOccurs='0' maxOccurs='unbounded' processContents='lax' />
        </xs:sequence>
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
 -->
  <xs:element name='definitions' type='wsdl:tDefinitions' >
    <xs:key name='message' >
      <xs:selector xpath='wsdl:message' />
      <xs:field xpath='@name' />
    </xs:key>
    <xs:key name='portType' >
      <xs:selector xpath='wsdl:portType' />
      <xs:field xpath='@name' />
    </xs:key>
    <xs:key name='binding' >
      <xs:selector xpath='wsdl:binding' />
      <xs:field xpath='@name' />
    </xs:key>
    <xs:key name='service' >
      <xs:selector xpath='wsdl:service' />
      <xs:field xpath='@name' />
    </xs:key>
    <xs:key name='import' >
      <xs:selector xpath='wsdl:import' />
      <xs:field xpath='@namespace' />
    </xs:key>
  </xs:element>

  <xs:group name='anyTopLevelOptionalElement' >
    <xs:annotation>
      <xs:documentation>
      Any top level optional element allowed to appear more then once - any child of definitions element except wsdl:types. Any extensibility element is allowed in any place.
      </xs:documentation>
    </xs:annotation>
    <xs:choice>
      <xs:element name='import' type='wsdl:tImport' />
      <xs:element name='types' type='wsdl:tTypes' />                     
      <xs:element name='message'  type='wsdl:tMessage' >
        <xs:unique name='part' >
          <xs:selector xpath='wsdl:part' />
          <xs:field xpath='@name' />
        </xs:unique>
      </xs:element>
      <xs:element name='portType' type='wsdl:tPortType' />
      <xs:element name='binding'  type='wsdl:tBinding' />
      <xs:element name='service'  type='wsdl:tService' >
        <xs:unique name='port' >
          <xs:selector xpath='wsdl:port' />
          <xs:field xpath='@name' />
        </xs:unique>
      </xs:element>
    </xs:choice>
  </xs:group>

  <xs:complexType name='tDefinitions' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:sequence>
          <xs:group ref='wsdl:anyTopLevelOptionalElement'  minOccurs='0'   maxOccurs='unbounded' />
        </xs:sequence>
        <xs:attribute name='targetNamespace' type='xs:anyURI' use='optional' />
        <xs:attribute name='name' type='xs:NCName' use='optional' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
   
  <xs:complexType name='tImport' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleAttributesDocumented' >
        <xs:attribute name='namespace' type='xs:anyURI' use='required' />
        <xs:attribute name='location' type='xs:anyURI' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
   
  <xs:complexType name='tTypes' >
    <xs:complexContent>   
      <xs:extension base='wsdl:tExtensibleDocumented' />
    </xs:complexContent>   
  </xs:complexType>
     
  <xs:complexType name='tMessage' >
    <xs:complexContent>   
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:sequence>
          <xs:element name='part' type='wsdl:tPart' minOccurs='0' maxOccurs='unbounded' />
        </xs:sequence>
        <xs:attribute name='name' type='xs:NCName' use='required' />
      </xs:extension>
    </xs:complexContent>   
  </xs:complexType>

  <xs:complexType name='tPart' >
    <xs:complexContent>   
      <xs:extension base='wsdl:tExtensibleAttributesDocumented' >
        <xs:attribute name='name' type='xs:NCName' use='required' />
        <xs:attribute name='element' type='xs:QName' use='optional' />
        <xs:attribute name='type' type='xs:QName' use='optional' />    
      </xs:extension>
    </xs:complexContent>   
  </xs:complexType>

  <xs:complexType name='tPortType' >
    <xs:complexContent>   
      <xs:extension base='wsdl:tExtensibleAttributesDocumented' >
        <xs:sequence>
          <xs:element name='operation' type='wsdl:tOperation' minOccurs='0' maxOccurs='unbounded' />
        </xs:sequence>
        <xs:attribute name='name' type='xs:NCName' use='required' />
      </xs:extension>
    </xs:complexContent>   
  </xs:complexType>
   
  <xs:complexType name='tOperation' >
    <xs:complexContent>   
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:sequence>
          <xs:choice>
            <xs:group ref='wsdl:request-response-or-one-way-operation' />
            <xs:group ref='wsdl:solicit-response-or-notification-operation' />
          </xs:choice>
        </xs:sequence>
        <xs:attribute name='name' type='xs:NCName' use='required' />
        <xs:attribute name='parameterOrder' type='xs:NMTOKENS' use='optional' />
      </xs:extension>
    </xs:complexContent>   
  </xs:complexType>
    
  <xs:group name='request-response-or-one-way-operation' >
    <xs:sequence>
      <xs:element name='input' type='wsdl:tParam' />
      <xs:sequence minOccurs='0' >
        <xs:element name='output' type='wsdl:tParam' />
        <xs:element name='fault' type='wsdl:tFault' minOccurs='0' maxOccurs='unbounded' />
      </xs:sequence>
    </xs:sequence>
  </xs:group>

  <xs:group name='solicit-response-or-notification-operation' >
    <xs:sequence>
      <xs:element name='output' type='wsdl:tParam' />
      <xs:sequence minOccurs='0' >
        <xs:element name='input' type='wsdl:tParam' />
        <xs:element name='fault' type='wsdl:tFault' minOccurs='0' maxOccurs='unbounded' />
      </xs:sequence>
    </xs:sequence>
  </xs:group>
        
  <xs:complexType name='tParam' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleAttributesDocumented' >
        <xs:attribute name='name' type='xs:NCName' use='optional' />
        <xs:attribute name='message' type='xs:QName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>

  <xs:complexType name='tFault' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleAttributesDocumented' >
        <xs:attribute name='name' type='xs:NCName'  use='required' />
        <xs:attribute name='message' type='xs:QName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
     
  <xs:complexType name='tBinding' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:sequence>
          <xs:element name='operation' type='wsdl:tBindingOperation' minOccurs='0' maxOccurs='unbounded' />
        </xs:sequence>
        <xs:attribute name='name' type='xs:NCName' use='required' />
        <xs:attribute name='type' type='xs:QName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
    
  <xs:complexType name='tBindingOperationMessage' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:attribute name='name' type='xs:NCName' use='optional' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  
  <xs:complexType name='tBindingOperationFault' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:attribute name='name' type='xs:NCName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>

  <xs:complexType name='tBindingOperation' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:sequence>
          <xs:element name='input' type='wsdl:tBindingOperationMessage' minOccurs='0' />
          <xs:element name='output' type='wsdl:tBindingOperationMessage' minOccurs='0' />
          <xs:element name='fault' type='wsdl:tBindingOperationFault' minOccurs='0' maxOccurs='unbounded' />
        </xs:sequence>
        <xs:attribute name='name' type='xs:NCName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
     
  <xs:complexType name='tService' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:sequence>
          <xs:element name='port' type='wsdl:tPort' minOccurs='0' maxOccurs='unbounded' />
        </xs:sequence>
        <xs:attribute name='name' type='xs:NCName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
     
  <xs:complexType name='tPort' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:attribute name='name' type='xs:NCName' use='required' />
        <xs:attribute name='binding' type='xs:QName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>

  <xs:attribute name='arrayType' type='xs:string' />
  <xs:attribute name='required' type='xs:boolean' />
  <xs:complexType name='tExtensibilityElement' abstract='true' >
    <xs:attribute ref='wsdl:required' use='optional' />
  </xs:complexType>

</xs:schema>";

        const string soap = @"<?xml version='1.0' encoding='UTF-8' ?> 
<xs:schema xmlns:soap='http://schemas.xmlsoap.org/wsdl/soap/' xmlns:wsdl='http://schemas.xmlsoap.org/wsdl/' targetNamespace='http://schemas.xmlsoap.org/wsdl/soap/' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:import namespace='http://schemas.xmlsoap.org/wsdl/' />
  <xs:simpleType name='encodingStyle'>
    <xs:annotation>
      <xs:documentation>
      'encodingStyle' indicates any canonicalization conventions followed in the contents of the containing element.  For example, the value 'http://schemas.xmlsoap.org/soap/encoding/' indicates the pattern described in SOAP specification
      </xs:documentation>
    </xs:annotation>
    <xs:list itemType='xs:anyURI' />
  </xs:simpleType>
  <xs:element name='binding' type='soap:tBinding' />
  <xs:complexType name='tBinding'>
    <xs:complexContent mixed='false'>
      <xs:extension base='wsdl:tExtensibilityElement'>
        <xs:attribute name='transport' type='xs:anyURI' use='required' />
        <xs:attribute name='style' type='soap:tStyleChoice' use='optional' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  <xs:simpleType name='tStyleChoice'>
    <xs:restriction base='xs:string'>
      <xs:enumeration value='rpc' />
      <xs:enumeration value='document' />
    </xs:restriction>
  </xs:simpleType>
  <xs:element name='operation' type='soap:tOperation' />
  <xs:complexType name='tOperation'>
    <xs:complexContent mixed='false'>
      <xs:extension base='wsdl:tExtensibilityElement'>
        <xs:attribute name='soapAction' type='xs:anyURI' use='optional' />
        <xs:attribute name='style' type='soap:tStyleChoice' use='optional' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  <xs:element name='body' type='soap:tBody' />
  <xs:attributeGroup name='tBodyAttributes'>
    <xs:attribute name='encodingStyle' type='soap:encodingStyle' use='optional' />
    <xs:attribute name='use' type='soap:useChoice' use='optional' />
    <xs:attribute name='namespace' type='xs:anyURI' use='optional' />
  </xs:attributeGroup>
  <xs:complexType name='tBody'>
    <xs:complexContent mixed='false'>
      <xs:extension base='wsdl:tExtensibilityElement'>
        <xs:attribute name='parts' type='xs:NMTOKENS' use='optional' />
        <xs:attributeGroup ref='soap:tBodyAttributes' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  <xs:simpleType name='useChoice'>
    <xs:restriction base='xs:string'>
      <xs:enumeration value='literal' />
      <xs:enumeration value='encoded' />
    </xs:restriction>
  </xs:simpleType>
  <xs:element name='fault' type='soap:tFault' />
  <xs:complexType name='tFaultRes' abstract='true'>
    <xs:complexContent mixed='false'>
      <xs:restriction base='soap:tBody'>
        <xs:attribute ref='wsdl:required' use='optional' />
        <xs:attribute name='parts' type='xs:NMTOKENS' use='prohibited' />
        <xs:attributeGroup ref='soap:tBodyAttributes' />
      </xs:restriction>
    </xs:complexContent>
  </xs:complexType>
  <xs:complexType name='tFault'>
    <xs:complexContent mixed='false'>
      <xs:extension base='soap:tFaultRes'>
        <xs:attribute name='name' type='xs:NCName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  <xs:element name='header' type='soap:tHeader' />
  <xs:attributeGroup name='tHeaderAttributes'>
    <xs:attribute name='message' type='xs:QName' use='required' />
    <xs:attribute name='part' type='xs:NMTOKEN' use='required' />
    <xs:attribute name='use' type='soap:useChoice' use='required' />
    <xs:attribute name='encodingStyle' type='soap:encodingStyle' use='optional' />
    <xs:attribute name='namespace' type='xs:anyURI' use='optional' />
  </xs:attributeGroup>
  <xs:complexType name='tHeader'>
    <xs:complexContent mixed='false'>
      <xs:extension base='wsdl:tExtensibilityElement'>
        <xs:sequence>
          <xs:element minOccurs='0' maxOccurs='unbounded' ref='soap:headerfault' />
        </xs:sequence>
        <xs:attributeGroup ref='soap:tHeaderAttributes' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  <xs:element name='headerfault' type='soap:tHeaderFault' />
  <xs:complexType name='tHeaderFault'>
    <xs:attributeGroup ref='soap:tHeaderAttributes' />
  </xs:complexType>
  <xs:element name='address' type='soap:tAddress' />
  <xs:complexType name='tAddress'>
    <xs:complexContent mixed='false'>
      <xs:extension base='wsdl:tExtensibilityElement'>
        <xs:attribute name='location' type='xs:anyURI' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
</xs:schema>";
        const string soapEncoding = @"<?xml version='1.0' encoding='UTF-8' ?>
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
           xmlns:tns='http://schemas.xmlsoap.org/soap/encoding/'
           targetNamespace='http://schemas.xmlsoap.org/soap/encoding/' >
        
 <xs:attribute name='root' >
   <xs:simpleType>
     <xs:restriction base='xs:boolean'>
       <xs:pattern value='0|1' />
     </xs:restriction>
   </xs:simpleType>
 </xs:attribute>

  <xs:attributeGroup name='commonAttributes' >
    <xs:attribute name='id' type='xs:ID' />
    <xs:attribute name='href' type='xs:anyURI' />
    <xs:anyAttribute namespace='##other' processContents='lax' />
  </xs:attributeGroup>
   
  <xs:simpleType name='arrayCoordinate' >
    <xs:restriction base='xs:string' />
  </xs:simpleType>
          
  <xs:attribute name='arrayType' type='xs:string' />
  <xs:attribute name='offset' type='tns:arrayCoordinate' />
  
  <xs:attributeGroup name='arrayAttributes' >
    <xs:attribute ref='tns:arrayType' />
    <xs:attribute ref='tns:offset' />
  </xs:attributeGroup>    
  
  <xs:attribute name='position' type='tns:arrayCoordinate' /> 
  
  <xs:attributeGroup name='arrayMemberAttributes' >
    <xs:attribute ref='tns:position' />
  </xs:attributeGroup>    

  <xs:group name='Array' >
    <xs:sequence>
      <xs:any namespace='##any' minOccurs='0' maxOccurs='unbounded' processContents='lax' />
    </xs:sequence>
  </xs:group>

  <xs:element name='Array' type='tns:Array' />
  <xs:complexType name='Array' >
    <xs:group ref='tns:Array' minOccurs='0' />
    <xs:attributeGroup ref='tns:arrayAttributes' />
    <xs:attributeGroup ref='tns:commonAttributes' />
  </xs:complexType> 
  <xs:element name='Struct' type='tns:Struct' />
  <xs:group name='Struct' >
    <xs:sequence>
      <xs:any namespace='##any' minOccurs='0' maxOccurs='unbounded' processContents='lax' />
    </xs:sequence>
  </xs:group>

  <xs:complexType name='Struct' >
    <xs:group ref='tns:Struct' minOccurs='0' />
    <xs:attributeGroup ref='tns:commonAttributes'/>
  </xs:complexType> 
  
  <xs:simpleType name='base64' >
    <xs:restriction base='xs:base64Binary' />
  </xs:simpleType>

  <xs:element name='duration' type='tns:duration' />
  <xs:complexType name='duration' >
    <xs:simpleContent>
      <xs:extension base='xs:duration' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='dateTime' type='tns:dateTime' />
  <xs:complexType name='dateTime' >
    <xs:simpleContent>
      <xs:extension base='xs:dateTime' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>



  <xs:element name='NOTATION' type='tns:NOTATION' />
  <xs:complexType name='NOTATION' >
    <xs:simpleContent>
      <xs:extension base='xs:QName' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>
  

  <xs:element name='time' type='tns:time' />
  <xs:complexType name='time' >
    <xs:simpleContent>
      <xs:extension base='xs:time' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='date' type='tns:date' />
  <xs:complexType name='date' >
    <xs:simpleContent>
      <xs:extension base='xs:date' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='gYearMonth' type='tns:gYearMonth' />
  <xs:complexType name='gYearMonth' >
    <xs:simpleContent>
      <xs:extension base='xs:gYearMonth' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='gYear' type='tns:gYear' />
  <xs:complexType name='gYear' >
    <xs:simpleContent>
      <xs:extension base='xs:gYear' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='gMonthDay' type='tns:gMonthDay' />
  <xs:complexType name='gMonthDay' >
    <xs:simpleContent>
      <xs:extension base='xs:gMonthDay' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='gDay' type='tns:gDay' />
  <xs:complexType name='gDay' >
    <xs:simpleContent>
      <xs:extension base='xs:gDay' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='gMonth' type='tns:gMonth' />
  <xs:complexType name='gMonth' >
    <xs:simpleContent>
      <xs:extension base='xs:gMonth' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>
  
  <xs:element name='boolean' type='tns:boolean' />
  <xs:complexType name='boolean' >
    <xs:simpleContent>
      <xs:extension base='xs:boolean' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='base64Binary' type='tns:base64Binary' />
  <xs:complexType name='base64Binary' >
    <xs:simpleContent>
      <xs:extension base='xs:base64Binary' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='hexBinary' type='tns:hexBinary' />
  <xs:complexType name='hexBinary' >
    <xs:simpleContent>
     <xs:extension base='xs:hexBinary' >
       <xs:attributeGroup ref='tns:commonAttributes' />
     </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='float' type='tns:float' />
  <xs:complexType name='float' >
    <xs:simpleContent>
      <xs:extension base='xs:float' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='double' type='tns:double' />
  <xs:complexType name='double' >
    <xs:simpleContent>
      <xs:extension base='xs:double' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='anyURI' type='tns:anyURI' />
  <xs:complexType name='anyURI' >
    <xs:simpleContent>
      <xs:extension base='xs:anyURI' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='QName' type='tns:QName' />
  <xs:complexType name='QName' >
    <xs:simpleContent>
      <xs:extension base='xs:QName' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  
  <xs:element name='string' type='tns:string' />
  <xs:complexType name='string' >
    <xs:simpleContent>
      <xs:extension base='xs:string' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='normalizedString' type='tns:normalizedString' />
  <xs:complexType name='normalizedString' >
    <xs:simpleContent>
      <xs:extension base='xs:normalizedString' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='token' type='tns:token' />
  <xs:complexType name='token' >
    <xs:simpleContent>
      <xs:extension base='xs:token' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='language' type='tns:language' />
  <xs:complexType name='language' >
    <xs:simpleContent>
      <xs:extension base='xs:language' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='Name' type='tns:Name' />
  <xs:complexType name='Name' >
    <xs:simpleContent>
      <xs:extension base='xs:Name' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='NMTOKEN' type='tns:NMTOKEN' />
  <xs:complexType name='NMTOKEN' >
    <xs:simpleContent>
      <xs:extension base='xs:NMTOKEN' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='NCName' type='tns:NCName' />
  <xs:complexType name='NCName' >
    <xs:simpleContent>
      <xs:extension base='xs:NCName' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='NMTOKENS' type='tns:NMTOKENS' />
  <xs:complexType name='NMTOKENS' >
    <xs:simpleContent>
      <xs:extension base='xs:NMTOKENS' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='ID' type='tns:ID' />
  <xs:complexType name='ID' >
    <xs:simpleContent>
      <xs:extension base='xs:ID' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='IDREF' type='tns:IDREF' />
  <xs:complexType name='IDREF' >
    <xs:simpleContent>
      <xs:extension base='xs:IDREF' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='ENTITY' type='tns:ENTITY' />
  <xs:complexType name='ENTITY' >
    <xs:simpleContent>
      <xs:extension base='xs:ENTITY' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='IDREFS' type='tns:IDREFS' />
  <xs:complexType name='IDREFS' >
    <xs:simpleContent>
      <xs:extension base='xs:IDREFS' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='ENTITIES' type='tns:ENTITIES' />
  <xs:complexType name='ENTITIES' >
    <xs:simpleContent>
      <xs:extension base='xs:ENTITIES' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='decimal' type='tns:decimal' />
  <xs:complexType name='decimal' >
    <xs:simpleContent>
      <xs:extension base='xs:decimal' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='integer' type='tns:integer' />
  <xs:complexType name='integer' >
    <xs:simpleContent>
      <xs:extension base='xs:integer' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='nonPositiveInteger' type='tns:nonPositiveInteger' />
  <xs:complexType name='nonPositiveInteger' >
    <xs:simpleContent>
      <xs:extension base='xs:nonPositiveInteger' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='negativeInteger' type='tns:negativeInteger' />
  <xs:complexType name='negativeInteger' >
    <xs:simpleContent>
      <xs:extension base='xs:negativeInteger' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='long' type='tns:long' />
  <xs:complexType name='long' >
    <xs:simpleContent>
      <xs:extension base='xs:long' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='int' type='tns:int' />
  <xs:complexType name='int' >
    <xs:simpleContent>
      <xs:extension base='xs:int' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='short' type='tns:short' />
  <xs:complexType name='short' >
    <xs:simpleContent>
      <xs:extension base='xs:short' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='byte' type='tns:byte' />
  <xs:complexType name='byte' >
    <xs:simpleContent>
      <xs:extension base='xs:byte' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='nonNegativeInteger' type='tns:nonNegativeInteger' />
  <xs:complexType name='nonNegativeInteger' >
    <xs:simpleContent>
      <xs:extension base='xs:nonNegativeInteger' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='unsignedLong' type='tns:unsignedLong' />
  <xs:complexType name='unsignedLong' >
    <xs:simpleContent>
      <xs:extension base='xs:unsignedLong' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='unsignedInt' type='tns:unsignedInt' />
  <xs:complexType name='unsignedInt' >
    <xs:simpleContent>
      <xs:extension base='xs:unsignedInt' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='unsignedShort' type='tns:unsignedShort' />
  <xs:complexType name='unsignedShort' >
    <xs:simpleContent>
      <xs:extension base='xs:unsignedShort' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='unsignedByte' type='tns:unsignedByte' />
  <xs:complexType name='unsignedByte' >
    <xs:simpleContent>
      <xs:extension base='xs:unsignedByte' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='positiveInteger' type='tns:positiveInteger' />
  <xs:complexType name='positiveInteger' >
    <xs:simpleContent>
      <xs:extension base='xs:positiveInteger' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='anyType' />
</xs:schema>";

        const string fakeXsd = @"<?xml version='1.0' encoding='UTF-8' ?> 
<xsd:schema targetNamespace=""http://www.w3.org/2001/XMLSchema"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
    <xsd:element name=""schema"">
    <xsd:complexType />
    </xsd:element>
</xsd:schema>";

        const string fakeXmlSchema = @"<xs:schema targetNamespace='http://www.w3.org/XML/1998/namespace' xmlns:xs='http://www.w3.org/2001/XMLSchema' xml:lang='en'>
 <xs:attribute name='lang' type='xs:language'/>
 <xs:attribute name='space'>
  <xs:simpleType>
   <xs:restriction base='xs:NCName'>
    <xs:enumeration value='default'/>
    <xs:enumeration value='preserve'/>
   </xs:restriction>
  </xs:simpleType>
 </xs:attribute>
 <xs:attribute name='base' type='xs:anyURI'/>
 <xs:attribute name='id' type='xs:ID' />
 <xs:attributeGroup name='specialAttrs'>
  <xs:attribute ref='xml:base'/>
  <xs:attribute ref='xml:lang'/>
  <xs:attribute ref='xml:space'/>
 </xs:attributeGroup>
</xs:schema>";


        const string fakeSoapEncoding = @"<?xml version='1.0' encoding='UTF-8' ?>
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
           xmlns:tns='http://schemas.xmlsoap.org/soap/encoding/'
           targetNamespace='http://schemas.xmlsoap.org/soap/encoding/' >
        
  <xs:attributeGroup name='commonAttributes' >
    <xs:attribute name='id' type='xs:ID' />
    <xs:attribute name='href' type='xs:anyURI' />
    <xs:anyAttribute namespace='##other' processContents='lax' />
  </xs:attributeGroup>
   
  <xs:simpleType name='arrayCoordinate' >
    <xs:restriction base='xs:string' />
  </xs:simpleType>
          
  <xs:attribute name='arrayType' type='xs:string' />
  <xs:attribute name='offset' type='tns:arrayCoordinate' />
  
  <xs:attributeGroup name='arrayAttributes' >
    <xs:attribute ref='tns:arrayType' />
    <xs:attribute ref='tns:offset' />
  </xs:attributeGroup>    

  <xs:group name='Array' >
    <xs:sequence>
      <xs:any namespace='##any' minOccurs='0' maxOccurs='unbounded' processContents='lax' />
    </xs:sequence>
  </xs:group>

  <xs:element name='Array' type='tns:Array' />
  <xs:complexType name='Array' >
    <xs:group ref='tns:Array' minOccurs='0' />
    <xs:attributeGroup ref='tns:arrayAttributes' />
    <xs:attributeGroup ref='tns:commonAttributes' />
  </xs:complexType> 
</xs:schema>";


    }
}
