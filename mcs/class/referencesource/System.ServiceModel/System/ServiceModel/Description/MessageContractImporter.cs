//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using WsdlNS = System.Web.Services.Description;

    class MessageContractImporter
    {
        static readonly XmlQualifiedName AnyType = new XmlQualifiedName("anyType", XmlSchema.Namespace);

        readonly XmlSchemaSet allSchemas;
        readonly WsdlContractConversionContext contractContext;
        readonly WsdlImporter importer;
        SchemaImporter schemaImporter;
        readonly FaultImportOptions faultImportOptions;

        static object schemaImporterLock = new object();

        Dictionary<WsdlNS.Message, IList<string>> bodyPartsTable;

        Dictionary<WsdlNS.Message, IList<string>> BodyPartsTable
        {
            get
            {
                if (bodyPartsTable == null)
                    bodyPartsTable = new Dictionary<WsdlNS.Message, IList<string>>();
                return bodyPartsTable;
            }
        }

        static internal void ImportMessageBinding(WsdlImporter importer, WsdlEndpointConversionContext endpointContext, Type schemaImporterType)
        {
            // All the work is done in ImportMessageContract call
            bool isReferencedContract = IsReferencedContract(importer, endpointContext);

            MarkSoapExtensionsAsHandled(endpointContext.WsdlBinding);

            foreach (WsdlNS.OperationBinding wsdlOperationBinding in endpointContext.WsdlBinding.Operations)
            {
                OperationDescription operation = endpointContext.GetOperationDescription(wsdlOperationBinding);
                if (isReferencedContract || OperationHasBeenHandled(operation))
                {
                    MarkSoapExtensionsAsHandled(wsdlOperationBinding);

                    if (wsdlOperationBinding.Input != null)
                    {
                        MarkSoapExtensionsAsHandled(wsdlOperationBinding.Input);
                    }

                    if (wsdlOperationBinding.Output != null)
                    {
                        MarkSoapExtensionsAsHandled(wsdlOperationBinding.Output);
                    }

                    foreach (WsdlNS.MessageBinding wsdlMessageBinding in wsdlOperationBinding.Faults)
                    {
                        MarkSoapExtensionsAsHandled(wsdlMessageBinding);
                    }
                }
            }

        }

        static bool OperationHasBeenHandled(OperationDescription operation)
        {
            return (operation.Behaviors.Find<IOperationContractGenerationExtension>() != null);
        }

        static bool IsReferencedContract(WsdlImporter importer, WsdlEndpointConversionContext endpointContext)
        {
            return importer.KnownContracts.ContainsValue(endpointContext.Endpoint.Contract);
        }

        static void MarkSoapExtensionsAsHandled(WsdlNS.NamedItem item)
        {
            foreach (object o in item.Extensions)
            {
                WsdlNS.ServiceDescriptionFormatExtension ext = o as WsdlNS.ServiceDescriptionFormatExtension;
                if (ext != null && IsSoapBindingExtension(ext))
                    ext.Handled = true;
                else if (SoapHelper.IsSoapFaultBinding(o as XmlElement))
                    ext.Handled = true;
            }
        }

        static bool IsSoapBindingExtension(WsdlNS.ServiceDescriptionFormatExtension ext)
        {
            if (ext is WsdlNS.SoapBinding
                || ext is WsdlNS.SoapBodyBinding
                || ext is WsdlNS.SoapHeaderBinding
                || ext is WsdlNS.SoapOperationBinding
                || ext is WsdlNS.SoapFaultBinding
                || ext is WsdlNS.SoapHeaderFaultBinding
                )
            {
                return true;
            }

            return false;

        }

        static internal void ImportMessageContract(WsdlImporter importer, WsdlContractConversionContext contractContext, SchemaImporter schemaImporter)
        {
            new MessageContractImporter(importer, contractContext, schemaImporter).ImportMessageContract();
        }

        MessageContractImporter(WsdlImporter importer, WsdlContractConversionContext contractContext, SchemaImporter schemaImporter)
        {
            this.contractContext = contractContext;
            this.importer = importer;
            this.allSchemas = GatherSchemas(importer);
            this.schemaImporter = schemaImporter;

            object faultImportOptions;
            if (this.importer.State.TryGetValue(typeof(FaultImportOptions), out faultImportOptions))
                this.faultImportOptions = (FaultImportOptions)faultImportOptions;
            else
                this.faultImportOptions = new FaultImportOptions();
        }

        XmlSchemaSet AllSchemas
        {
            get { return this.allSchemas; }
        }

        SchemaImporter CurrentSchemaImporter
        {
            get { return schemaImporter; }
        }

        internal void AddWarning(string message)
        {
            AddError(message, true);
        }

        void AddError(string message)
        {
            AddError(message, false);
        }

        void AddError(string message, bool isWarning)
        {
            MetadataConversionError warning = new MetadataConversionError(message, isWarning);
            if (!importer.Errors.Contains(warning))
                importer.Errors.Add(warning);
        }

        void TraceImportInformation(OperationDescription operation)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<String, String> ht = new Dictionary<string, string>(2)
                {
                    { "Operation", operation.Name },
                    { "Format", CurrentSchemaImporter.GetFormatName() }
                };
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.CannotBeImportedInCurrentFormat,
                    SR.GetString(SR.TraceCodeCannotBeImportedInCurrentFormat), new DictionaryTraceRecord(ht), null, null);
            }
        }

        void ImportMessageContract()
        {
            if (contractContext.Contract.Operations.Count <= 0)
                return;
            CurrentSchemaImporter.PreprocessSchema();

            bool importerUsed = true;

            OperationInfo[] infos = new OperationInfo[contractContext.Contract.Operations.Count];
            int i = 0;
            foreach (OperationDescription operation in contractContext.Contract.Operations)
            {
                OperationInfo operationInfo;
                if (!CanImportOperation(operation, out operationInfo))
                {
                    TraceImportInformation(operation);
                    importerUsed = false;
                    break;
                }
                infos[i++] = operationInfo;
            }

            if (importerUsed)
            {
                i = 0;
                foreach (OperationDescription operation in contractContext.Contract.Operations)
                {
                    ImportOperationContract(operation, infos[i++]);
                }
            }
            CurrentSchemaImporter.PostprocessSchema(importerUsed);
        }

        bool CanImportOperation(OperationDescription operation, out OperationInfo operationInfo)
        {
            operationInfo = null;
            if (OperationHasBeenHandled(operation))
                return false;

            WsdlNS.Operation wsdlOperation = contractContext.GetOperation(operation);
            Collection<WsdlNS.OperationBinding> wsdlOperationBindings = contractContext.GetOperationBindings(wsdlOperation);

            return CanImportOperation(operation, wsdlOperation, wsdlOperationBindings, out operationInfo) 
                && CanImportFaults(wsdlOperation, operation);
        }

        bool CanImportFaults(WsdlNS.Operation operation, OperationDescription description)
        {
            // When this.faultImportOptions.UseMessageFormat is false, we fall back to the V1 behavior of using DataContractSerializer to import all faults.
            // We can, therefore, return true in those cases without actually doing the checks involved in CanImportFaults.
            if (!this.faultImportOptions.UseMessageFormat)
                return true;

            foreach (WsdlNS.OperationFault fault in operation.Faults)
            {
                if (!CanImportFault(fault, description))
                    return false;
            }
            return true;
        }

        bool CanImportFault(WsdlNS.OperationFault fault, OperationDescription description)
        {
            XmlSchemaElement detailElement;
            XmlQualifiedName detailElementTypeName;
            XmlQualifiedName detailElementQname;

            if (!ValidateFault(fault, description, out detailElement, out detailElementTypeName, out detailElementQname))
                return false;

            return this.CurrentSchemaImporter.CanImportFault(detailElement, detailElementTypeName);
        }

        void ImportOperationContract(OperationDescription operation, OperationInfo operationInfo)
        {
            Fx.Assert(!OperationHasBeenHandled(operation), "");
            Fx.Assert(operationInfo != null, "");

            WsdlNS.Operation wsdlOperation = contractContext.GetOperation(operation);
            Collection<WsdlNS.OperationBinding> wsdlOperationBindings = contractContext.GetOperationBindings(wsdlOperation);

            bool isReply = false;
            foreach (WsdlNS.OperationMessage operationMessage in wsdlOperation.Messages)
            {
                ImportMessage(operationMessage, isReply, operationInfo.IsEncoded, operationInfo.AreAllMessagesWrapped);
                isReply = true;
            }

            if (operationInfo.Style == OperationFormatStyle.Rpc)
                SetWrapperName(operation);
            this.CurrentSchemaImporter.SetOperationStyle(operation, operationInfo.Style);
            this.CurrentSchemaImporter.SetOperationIsEncoded(operation, operationInfo.IsEncoded);
            this.CurrentSchemaImporter.SetOperationSupportFaults(operation,
                this.faultImportOptions.UseMessageFormat);

            ImportFaults(wsdlOperation, operation, operationInfo.IsEncoded);

            foreach (WsdlNS.OperationBinding wsdlOperationBinding in wsdlOperationBindings)
            {
                foreach (MessageDescription message in operation.Messages)
                {
                    WsdlNS.OperationMessage wsdlOperationMessage = contractContext.GetOperationMessage(message);
                    WsdlNS.ServiceDescriptionCollection wsdlDocuments = wsdlOperationMessage.Operation.PortType.ServiceDescription.ServiceDescriptions;
                    WsdlNS.Message wsdlMessage = wsdlDocuments.GetMessage(wsdlOperationMessage.Message);

                    WsdlNS.MessageBinding messageBinding = (message.Direction == MessageDirection.Input)
                        ? (WsdlNS.MessageBinding)wsdlOperationBinding.Input
                        : (WsdlNS.MessageBinding)wsdlOperationBinding.Output;

                    if (messageBinding != null)
                        ImportMessageBinding(messageBinding, wsdlMessage, message, operationInfo.Style, operationInfo.IsEncoded);
                }
            }

            operation.Behaviors.Add(CurrentSchemaImporter.GetOperationGenerator());
        }

        bool CanImportOperation(OperationDescription operation, WsdlNS.Operation wsdlOperation, Collection<WsdlNS.OperationBinding> operationBindings,
            out OperationInfo operationInfo)
        {
            operationInfo = null;
            OperationFormatStyle style = OperationFormatStyle.Document;
            bool isEncoded = false;
            bool areAllMessagesWrapped = true;

            // Check if operation bindings can be imported 

            StyleAndUse? styleAndUse = null;
            WsdlNS.ServiceDescriptionCollection documents = wsdlOperation.PortType.ServiceDescription.ServiceDescriptions;
            WsdlNS.OperationBinding prevOperationBinding = null;
            foreach (WsdlNS.OperationBinding operationBinding in operationBindings)
            {
                OperationFormatStyle operationStyle = GetStyle(operationBinding);
                bool? isOperationEncoded = null;
                foreach (MessageDescription message in operation.Messages)
                {
                    WsdlNS.OperationMessage operationMessage = contractContext.GetOperationMessage(message);

                    if (operationMessage.Message.IsEmpty)
                    {
                        if (operationMessage is WsdlNS.OperationInput)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxWsdlOperationInputNeedsMessageAttribute2, wsdlOperation.Name, wsdlOperation.PortType.Name)));
                        if (operationMessage is WsdlNS.OperationOutput)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxWsdlOperationOutputNeedsMessageAttribute2, wsdlOperation.Name, wsdlOperation.PortType.Name)));
                    }
                    WsdlNS.Message wsdlMessage = documents.GetMessage(operationMessage.Message);

                    if (wsdlMessage != null)
                    {
                        WsdlNS.MessageBinding messageBinding = (message.Direction == MessageDirection.Input)
                            ? (WsdlNS.MessageBinding)operationBinding.Input
                            : (WsdlNS.MessageBinding)operationBinding.Output;

                        if (messageBinding != null)
                        {
                            bool isMessageEncoded;
                            if (!CanImportMessageBinding(messageBinding, wsdlMessage, operationStyle, out isMessageEncoded))
                                return false;

                            if (isOperationEncoded == null)
                                isOperationEncoded = isMessageEncoded;
                            else if (isOperationEncoded != isMessageEncoded)
                                AddError(SR.GetString(SR.SFxInconsistentWsdlOperationUseInBindingMessages, messageBinding.OperationBinding.Name, messageBinding.OperationBinding.Binding.Name));
                        }
                    }
                }
                foreach (WsdlNS.FaultBinding faultBinding in operationBinding.Faults)
                {
                    bool isFaultEncoded;
                    if (!CanImportFaultBinding(faultBinding, operationStyle, out isFaultEncoded))
                        return false;
                    if (isOperationEncoded == null)
                        isOperationEncoded = isFaultEncoded;
                    else if (isOperationEncoded != isFaultEncoded)
                        AddError(SR.GetString(SR.SFxInconsistentWsdlOperationUseInBindingFaults, faultBinding.OperationBinding.Name, faultBinding.OperationBinding.Binding.Name));
                }
                isOperationEncoded = isOperationEncoded ?? false;

                if (styleAndUse == null)
                {
                    styleAndUse = GetStyleAndUse(operationStyle, isOperationEncoded.Value);
                    style = operationStyle;
                    isEncoded = isOperationEncoded.Value;
                    prevOperationBinding = operationBinding;
                }
                else
                {
                    StyleAndUse operationStyleAndUse = GetStyleAndUse(operationStyle, isOperationEncoded.Value);
                    if (operationStyleAndUse != styleAndUse)
                    {
                        AddError(SR.GetString(SR.SFxInconsistentWsdlOperationUseAndStyleInBinding,
                            operation.Name, operationBinding.Binding.Name, GetUse(operationStyleAndUse), GetStyle(operationStyleAndUse),
                            prevOperationBinding.Binding.Name, GetUse(styleAndUse.Value), GetStyle(styleAndUse.Value)));
                    }
                    if (operationStyleAndUse < styleAndUse)
                    {
                        styleAndUse = operationStyleAndUse;
                        style = operationStyle;
                        isEncoded = isOperationEncoded.Value;
                        prevOperationBinding = operationBinding;
                    }
                }
            }

            // Check if operation can be imported 
            OperationFormatStyle? inferredOperationStyle = null;
            foreach (WsdlNS.OperationMessage wsdlOperationMessage in wsdlOperation.Messages)
            {
                if (wsdlOperationMessage.Message.IsEmpty)
                {
                    if (wsdlOperationMessage is WsdlNS.OperationInput)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxWsdlOperationInputNeedsMessageAttribute2, wsdlOperation.Name, wsdlOperation.PortType.Name)));
                    if (wsdlOperationMessage is WsdlNS.OperationOutput)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxWsdlOperationOutputNeedsMessageAttribute2, wsdlOperation.Name, wsdlOperation.PortType.Name)));
                }
                WsdlNS.Message wsdlMessage = documents.GetMessage(wsdlOperationMessage.Message);

                OperationFormatStyle? inferredMessageStyle;
                if (!CanImportMessage(wsdlMessage, wsdlOperationMessage.Name, out inferredMessageStyle, ref areAllMessagesWrapped))
                    return false;

                if (wsdlMessage.Parts.Count > 0)
                {
                    if (inferredOperationStyle == null
                        || (inferredMessageStyle != null
                            && inferredMessageStyle != inferredOperationStyle
                            && inferredMessageStyle.Value == OperationFormatStyle.Document))
                    {
                        inferredOperationStyle = inferredMessageStyle;
                    }
                }
            }

            // Verify that information in operation bindings and operation match
            if (styleAndUse == null)
                style = inferredOperationStyle ?? OperationFormatStyle.Document;
            else if (inferredOperationStyle != null)
            {
                if (inferredOperationStyle.Value != style && inferredOperationStyle.Value == OperationFormatStyle.Document)
                    AddError(SR.GetString(SR.SFxInconsistentWsdlOperationStyleInOperationMessages, operation.Name, inferredOperationStyle, style));
            }
            operationInfo = new OperationInfo(style, isEncoded, areAllMessagesWrapped);
            return true;
        }

        bool CanImportMessage(WsdlNS.Message wsdlMessage, string operationName, out OperationFormatStyle? inferredStyle, ref bool areAllMessagesWrapped)
        {
            WsdlNS.MessagePartCollection messageParts = wsdlMessage.Parts;
            // try the special cases: wrapped and Message
            if (messageParts.Count == 1)
            {
                if (CanImportAnyMessage(messageParts[0]))
                {
                    areAllMessagesWrapped = false;
                    inferredStyle = OperationFormatStyle.Document;
                    return true;
                }

                if (CanImportStream(messageParts[0], out inferredStyle, ref areAllMessagesWrapped))
                    return true;

                if (areAllMessagesWrapped && CanImportWrappedMessage(messageParts[0]))
                {
                    inferredStyle = OperationFormatStyle.Document;
                    return true;
                }
                areAllMessagesWrapped = false;
            }

            inferredStyle = null;
            IList<string> bodyPartsFromBindings;
            BodyPartsTable.TryGetValue(wsdlMessage, out bodyPartsFromBindings);
            foreach (WsdlNS.MessagePart part in messageParts)
            {
                if (bodyPartsFromBindings != null && !bodyPartsFromBindings.Contains(part.Name))
                    continue;
                OperationFormatStyle style;
                if (!CurrentSchemaImporter.CanImportMessagePart(part, out style))
                    return false;

                if (inferredStyle == null)
                    inferredStyle = style;
                else if (style != inferredStyle.Value)
                    AddError(SR.GetString(SR.SFxInconsistentWsdlOperationStyleInMessageParts, operationName));
            }
            return true;
        }

        void ImportMessage(WsdlNS.OperationMessage wsdlOperationMessage, bool isReply, bool isEncoded, bool areAllMessagesWrapped)
        {
            MessageDescription messageDescription = contractContext.GetMessageDescription(wsdlOperationMessage);
            OperationDescription operation = contractContext.GetOperationDescription(wsdlOperationMessage.Operation);
            WsdlNS.ServiceDescriptionCollection wsdlDocuments = wsdlOperationMessage.Operation.PortType.ServiceDescription.ServiceDescriptions;
            WsdlNS.Message wsdlMessage = wsdlDocuments.GetMessage(wsdlOperationMessage.Message);

            if (wsdlMessage.Parts.Count == 1)
            {
                if (TryImportAnyMessage(wsdlMessage.Parts[0], messageDescription, isReply))
                    return;
                if (TryImportStream(wsdlMessage.Parts[0], messageDescription, isReply, areAllMessagesWrapped))
                    return;
                if (areAllMessagesWrapped && TryImportWrappedMessage(messageDescription, operation.Messages[0], wsdlMessage, isReply))
                    return;
            }

            WsdlNS.MessagePartCollection messageParts = wsdlMessage.Parts;
            IList<string> bodyPartsFromBindings;
            BodyPartsTable.TryGetValue(wsdlMessage, out bodyPartsFromBindings);
            string[] parameterOrder = wsdlOperationMessage.Operation.ParameterOrder;

            foreach (WsdlNS.MessagePart part in messageParts)
            {
                if (!ValidWsdl.Check(part, wsdlMessage, AddWarning))
                    continue;

                if (bodyPartsFromBindings != null && !bodyPartsFromBindings.Contains(part.Name))
                    continue;
                bool isReturn = false;
                if (parameterOrder != null && isReply)
                    isReturn = Array.IndexOf<string>(parameterOrder, part.Name) == -1;
                MessagePartDescription partDesc = CurrentSchemaImporter.ImportMessagePart(part, false/*isHeader*/, isEncoded);
                if (isReturn && messageDescription.Body.ReturnValue == null)
                    messageDescription.Body.ReturnValue = partDesc;
                else
                    messageDescription.Body.Parts.Add(partDesc);
            }

            if (isReply && messageDescription.Body.ReturnValue == null)
            {
                if (messageDescription.Body.Parts.Count > 0)
                {
                    if (!CheckIsRef(operation.Messages[0], messageDescription.Body.Parts[0]))
                    {
                        messageDescription.Body.ReturnValue = messageDescription.Body.Parts[0];
                        messageDescription.Body.Parts.RemoveAt(0);
                    }
                }
            }
        }

        enum StyleAndUse
        {
            DocumentLiteral,
            RpcLiteral,
            RpcEncoded,
            DocumentEncoded,
        }

        static StyleAndUse GetStyleAndUse(OperationFormatStyle style, bool isEncoded)
        {
            if (style == OperationFormatStyle.Document)
            {
                return isEncoded ? StyleAndUse.DocumentEncoded : StyleAndUse.DocumentLiteral;
            }
            else
            {
                return isEncoded ? StyleAndUse.RpcEncoded : StyleAndUse.RpcLiteral;
            }
        }

        static string GetStyle(StyleAndUse styleAndUse)
        {
            return (styleAndUse == StyleAndUse.RpcLiteral || styleAndUse == StyleAndUse.RpcEncoded) ? "rpc" : "document";
        }

        static string GetUse(StyleAndUse styleAndUse)
        {
            return (styleAndUse == StyleAndUse.RpcEncoded || styleAndUse == StyleAndUse.DocumentEncoded) ? "encoded" : "literal";
        }

        static void SetWrapperName(OperationDescription operation)
        {
            MessageDescriptionCollection messages = operation.Messages;
            if (messages != null && messages.Count > 0)
            {
                MessageDescription request = messages[0];
                if (request != null)
                {
                    request.Body.WrapperName = operation.Name;
                    request.Body.WrapperNamespace = operation.DeclaringContract.Namespace;
                }
                if (messages.Count > 1)
                {
                    MessageDescription response = messages[1];
                    if (response != null)
                    {
                        response.Body.WrapperName = TypeLoader.GetBodyWrapperResponseName(operation.Name).EncodedName;
                        response.Body.WrapperNamespace = operation.DeclaringContract.Namespace;
                    }
                }
            }
        }

        void ImportFaults(WsdlNS.Operation operation, OperationDescription description, bool isEncoded)
        {
            foreach (WsdlNS.OperationFault fault in operation.Faults)
            {
                ImportFault(fault, description, isEncoded);
            }
        }

        void ImportFault(WsdlNS.OperationFault fault, OperationDescription description, bool isEncoded)
        {
            XmlSchemaElement detailElement;
            XmlQualifiedName detailElementTypeName;
            XmlQualifiedName detailElementQname;

            if (!ValidateFault(fault, description, out detailElement, out detailElementTypeName, out detailElementQname))
                return;

            SchemaImporter faultImporter;
            if (this.faultImportOptions.UseMessageFormat)
                faultImporter = this.CurrentSchemaImporter;
            else
                faultImporter = DataContractSerializerSchemaImporter.Get(this.importer);
            CodeTypeReference detailElementTypeRef;
            if (IsNullOrEmpty(detailElementTypeName))
                detailElementTypeRef = faultImporter.ImportFaultElement(detailElementQname, detailElement, isEncoded);
            else
                detailElementTypeRef = faultImporter.ImportFaultType(detailElementQname, detailElementTypeName, isEncoded);
            FaultDescription faultDescription = contractContext.GetFaultDescription(fault);
            faultDescription.DetailTypeReference = detailElementTypeRef;
            faultDescription.ElementName = new XmlName(detailElementQname.Name, true /*isEncoded*/);
            faultDescription.Namespace = detailElementQname.Namespace;
        }

        bool ValidateFault(WsdlNS.OperationFault fault, OperationDescription description, out XmlSchemaElement detailElement,
            out XmlQualifiedName detailElementTypeName, out XmlQualifiedName detailElementQname)
        {
            detailElement = null;
            detailElementTypeName = null;
            detailElementQname = null;

            // this will throw if the message is not found (consider wrapping exception)
            WsdlNS.ServiceDescriptionCollection wsdlDocuments = fault.Operation.PortType.ServiceDescription.ServiceDescriptions;

            if (fault.Message.IsEmpty)
            {
                TraceFaultCannotBeImported(fault.Name, description.Name, SR.GetString(SR.SFxWsdlOperationFaultNeedsMessageAttribute2, fault.Name, fault.Operation.PortType.Name));
                description.Faults.Remove(contractContext.GetFaultDescription(fault));
                return false;
            }
            WsdlNS.Message faultMessage = wsdlDocuments.GetMessage(fault.Message);

            // we only recognize faults with a single part (single element inside detail):
            if (faultMessage.Parts.Count != 1)
            {
                TraceFaultCannotBeImported(fault.Name, description.Name, SR.GetString(SR.UnsupportedWSDLOnlyOneMessage));
                description.Faults.Remove(contractContext.GetFaultDescription(fault));
                return false;
            }

            WsdlNS.MessagePart faultMessageDetail = faultMessage.Parts[0];
            detailElementQname = faultMessageDetail.Element;

            if (IsNullOrEmpty(detailElementQname) || !IsNullOrEmpty(faultMessageDetail.Type))
            {
                TraceFaultCannotBeImported(fault.Name, description.Name, SR.GetString(SR.UnsupportedWSDLTheFault));
                description.Faults.Remove(contractContext.GetFaultDescription(fault));
                return false;
            }

            detailElement = FindSchemaElement(this.AllSchemas, detailElementQname);
            detailElementTypeName = GetTypeName(detailElement);
            return true;
        }

        bool CanImportAnyMessage(WsdlNS.MessagePart part)
        {
            return CheckPart(part.Type, DataContractSerializerMessageContractImporter.GenericMessageTypeName);
        }

        bool TryImportAnyMessage(WsdlNS.MessagePart part, MessageDescription description, bool isReply)
        {
            return CheckAndAddPart(part.Type, DataContractSerializerMessageContractImporter.GenericMessageTypeName, part.Name, string.Empty, typeof(Message), description, isReply);
        }

        bool CanImportStream(WsdlNS.MessagePart part, out OperationFormatStyle? style, ref bool areAllMessagesWrapped)
        {
            style = OperationFormatStyle.Document;
            string ns;
            XmlSchemaForm elementFormDefault;
            if (areAllMessagesWrapped && IsWrapperPart(part))
            {
                XmlSchemaComplexType complexType = GetElementComplexType(part.Element, allSchemas, out ns, out elementFormDefault);
                if (complexType != null)
                {
                    XmlSchemaSequence rootSequence = GetRootSequence(complexType);
                    if (rootSequence != null && rootSequence.Items.Count == 1 && rootSequence.Items[0] is XmlSchemaElement)
                        return CheckPart(((XmlSchemaElement)rootSequence.Items[0]).SchemaTypeName, DataContractSerializerMessageContractImporter.StreamBodyTypeName);
                }
                return false;
            }
            areAllMessagesWrapped = false;
            XmlQualifiedName typeName = part.Type;
            style = OperationFormatStyle.Rpc;
            if (IsNullOrEmpty(typeName))
            {
                if (IsNullOrEmpty(part.Element))
                    return false;
                style = OperationFormatStyle.Document;
                typeName = GetTypeName(FindSchemaElement(allSchemas, part.Element));
            }
            return CheckPart(typeName, DataContractSerializerMessageContractImporter.StreamBodyTypeName);
        }

        bool TryImportStream(WsdlNS.MessagePart part, MessageDescription description, bool isReply, bool areAllMessagesWrapped)
        {
            string ns = string.Empty;
            XmlSchemaForm elementFormDefault;
            if (areAllMessagesWrapped && IsWrapperPart(part))
            {
                XmlSchemaSequence rootSequence = GetRootSequence(GetElementComplexType(part.Element, allSchemas, out ns, out elementFormDefault));
                if (rootSequence != null && rootSequence.Items.Count == 1 && rootSequence.Items[0] is XmlSchemaElement)
                {
                    XmlSchemaElement element = (XmlSchemaElement)rootSequence.Items[0];
                    description.Body.WrapperName = new XmlName(part.Element.Name, true /*isEncoded*/).EncodedName;
                    description.Body.WrapperNamespace = part.Element.Namespace;

                    if (element.SchemaTypeName.IsEmpty && element.RefName != null)
                    {
                        return CheckAndAddPart(element.ElementSchemaType.QualifiedName, DataContractSerializerMessageContractImporter.StreamBodyTypeName, element.RefName.Name, GetLocalElementNamespace(element.RefName.Namespace, element, elementFormDefault), typeof(Stream), description, isReply);
                    }
                    else
                    {
                        return CheckAndAddPart(element.SchemaTypeName, DataContractSerializerMessageContractImporter.StreamBodyTypeName, element.Name, GetLocalElementNamespace(ns, element, elementFormDefault), typeof(Stream), description, isReply);
                    }
                }
                return false;
            }
            XmlQualifiedName typeName = part.Type;
            if (IsNullOrEmpty(typeName))
            {
                if (IsNullOrEmpty(part.Element))
                    return false;
                ns = part.Element.Namespace;
                typeName = GetTypeName(FindSchemaElement(allSchemas, part.Element));
            }
            return CheckAndAddPart(typeName, DataContractSerializerMessageContractImporter.StreamBodyTypeName, part.Name, ns, typeof(Stream), description, isReply);
        }

        bool CanImportWrappedMessage(WsdlNS.MessagePart wsdlPart)
        {
            return (IsWrapperPart(wsdlPart)) ? CurrentSchemaImporter.CanImportWrapperElement(wsdlPart.Element) : false;
        }

        bool TryImportWrappedMessage(MessageDescription messageDescription, MessageDescription requestMessage, WsdlNS.Message wsdlMessage, bool isReply)
        {

            WsdlNS.MessagePart wsdlPart = wsdlMessage.Parts[0];
            if (CanImportWrappedMessage(wsdlPart))
            {
                XmlQualifiedName elementName = wsdlPart.Element;
                MessagePartDescription[] parts = CurrentSchemaImporter.ImportWrapperElement(elementName);
                if (parts == null)
                    return false;
                messageDescription.Body.WrapperName = new XmlName(elementName.Name, true /*isEncoded*/).EncodedName;
                messageDescription.Body.WrapperNamespace = elementName.Namespace;

                if (parts.Length > 0)
                {
                    int partIndex = 0;
                    if (isReply && messageDescription.Body.ReturnValue == null && !CheckIsRef(requestMessage, parts[0]))
                    {
                        messageDescription.Body.ReturnValue = parts[0];
                        partIndex = 1;
                    }
                    for (; partIndex < parts.Length; partIndex++)
                    {
                        MessagePartDescription part = parts[partIndex];
                        messageDescription.Body.Parts.Add(part);
                    }
                }
                return true;
            }
            return false;
        }

        private bool IsWrapperPart(WsdlNS.MessagePart wsdlPart)
        {
            bool wrapFlag = false; // turn off special-casing for partname="parameters" if "wrapped" flag was set by user
            object wrappedOptions = null;
            if (this.importer.State.TryGetValue(typeof(WrappedOptions), out wrappedOptions))
            {
                wrapFlag = ((WrappedOptions)wrappedOptions).WrappedFlag;
            }
            return wsdlPart.Name == "parameters" && !IsNullOrEmpty(wsdlPart.Element) && !wrapFlag;
        }

        bool CheckIsRef(MessageDescription requestMessage, MessagePartDescription part)
        {
            foreach (MessagePartDescription requestPart in requestMessage.Body.Parts)
            {
                if (CompareMessageParts(requestPart, part))
                    return true;
            }
            return false;
        }

        bool CompareMessageParts(MessagePartDescription x, MessagePartDescription y)
        {
            return (x.Name == y.Name && x.Namespace == y.Namespace);
        }

        static WsdlNS.MessagePart FindPartByName(WsdlNS.Message message, string name)
        {
            Fx.Assert(message != null, "Should not attempt to look for a part in an null message.");
            foreach (WsdlNS.MessagePart part in message.Parts)
                if (part.Name == name)
                    return part;

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxWsdlMessageDoesNotContainPart3, name, message.Name, message.ServiceDescription.TargetNamespace)));
        }

        static XmlSchemaElement FindSchemaElement(XmlSchemaSet schemaSet, XmlQualifiedName elementName)
        {
            XmlSchema schema;
            return FindSchemaElement(schemaSet, elementName, out schema);
        }

        static XmlSchemaElement FindSchemaElement(XmlSchemaSet schemaSet, XmlQualifiedName elementName, out XmlSchema containingSchema)
        {
            XmlSchemaElement element = null;
            containingSchema = null;
            foreach (XmlSchema schema in GetSchema(schemaSet, elementName.Namespace))
            {
                element = (XmlSchemaElement)schema.Elements[elementName];
                if (element != null)
                {
                    containingSchema = schema;
                    break;
                }
            }
            if (element == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxSchemaDoesNotContainElement, elementName.Name, elementName.Namespace)));
            return element;
        }

        static XmlSchemaType FindSchemaType(XmlSchemaSet schemaSet, XmlQualifiedName typeName)
        {
            if (typeName.Namespace == XmlSchema.Namespace)
                return null;
            XmlSchema schema;
            return FindSchemaType(schemaSet, typeName, out schema);
        }

        static XmlSchemaType FindSchemaType(XmlSchemaSet schemaSet, XmlQualifiedName typeName, out XmlSchema containingSchema)
        {
            containingSchema = null;
            if (StockSchemas.IsKnownSchema(typeName.Namespace))
                return null;

            XmlSchemaType type = null;
            foreach (XmlSchema schema in GetSchema(schemaSet, typeName.Namespace))
            {
                type = (XmlSchemaType)schema.SchemaTypes[typeName];
                if (type != null)
                {
                    containingSchema = schema;
                    break;
                }
            }
            if (type == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxSchemaDoesNotContainType, typeName.Name, typeName.Namespace)));
            return type;
        }

        static XmlSchemaSet GatherSchemas(WsdlImporter importer)
        {
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.XmlResolver = null;

            foreach (WsdlNS.ServiceDescription wsdl in importer.WsdlDocuments)
            {
                XmlQualifiedName[] wsdlPrefixNsPairs = wsdl.Namespaces.ToArray();
                if (wsdl.Types != null && wsdl.Types.Schemas != null)
                {
                    foreach (XmlSchema xsd in wsdl.Types.Schemas)
                    {
                        XmlSerializerNamespaces xsdNamespaces = xsd.Namespaces;
                        XmlQualifiedName[] xsdPrefixNsPairs = xsdNamespaces.ToArray();
                        Dictionary<string, object> prefixesUsed = new Dictionary<string, object>();
                        foreach (XmlQualifiedName pair in xsdPrefixNsPairs)
                            prefixesUsed.Add(pair.Name, null);
                        foreach (XmlQualifiedName pair in wsdlPrefixNsPairs)
                            if (!prefixesUsed.ContainsKey(pair.Name))
                                xsdNamespaces.Add(pair.Name, pair.Namespace);
                        if (xsd.Items.Count > 0)
                        {
                            schemaSet.Add(xsd);
                        }
                        else
                        {
                            // only add include schemas
                            foreach (XmlSchemaExternal include in xsd.Includes)
                            {
                                if (include.Schema != null)
                                {
                                    schemaSet.Add(include.Schema);
                                }
                            }
                        }
                    }
                }
            }

            schemaSet.Add(importer.XmlSchemas);

            return schemaSet;
        }

        // Segregate the schemas containing abstract types from those 
        // containing regular XML definitions.  This is important because
        // when you import something returning the ur-type (object), then
        // you need to import ALL types/elements within ALL schemas.  We
        // don't want the RPC-based types leaking over into the XML-based
        // element definitions or literal types in the encoded schemas,
        // beacase it can cause schema coimpilation falure. 
        static void CollectEncodedAndLiteralSchemas(WsdlNS.ServiceDescriptionCollection serviceDescriptions, XmlSchemas encodedSchemas, XmlSchemas literalSchemas, XmlSchemaSet allSchemas)
        {
            XmlSchema wsdl = StockSchemas.CreateWsdl();
            XmlSchema soap = StockSchemas.CreateSoap();
            XmlSchema soapEncoding = StockSchemas.CreateSoapEncoding();

            Hashtable references = new Hashtable();
            if (!allSchemas.Contains(wsdl.TargetNamespace))
            {
                references[soap] = wsdl;
            }

            if (!allSchemas.Contains(soap.TargetNamespace))
            {
                references[soap] = soap;
            }
            if (!allSchemas.Contains(soapEncoding.TargetNamespace))
            {
                references[soapEncoding] = soapEncoding;
            }
            foreach (WsdlNS.ServiceDescription description in serviceDescriptions)
            {
                foreach (WsdlNS.Message message in description.Messages)
                {
                    foreach (WsdlNS.MessagePart part in message.Parts)
                    {
                        bool isEncoded;
                        bool isLiteral;
                        FindUse(part, out isEncoded, out isLiteral);
                        if (part.Element != null && !part.Element.IsEmpty)
                        {
                            XmlSchemaElement element = FindSchemaElement(allSchemas, part.Element);
                            if (element != null)
                            {
                                AddSchema(element.Parent as XmlSchema, isEncoded, isLiteral, encodedSchemas, literalSchemas, references);
                                if (element.SchemaTypeName != null && !element.SchemaTypeName.IsEmpty)
                                {
                                    XmlSchemaType type = FindSchemaType(allSchemas, element.SchemaTypeName);
                                    if (type != null)
                                    {
                                        AddSchema(type.Parent as XmlSchema, isEncoded, isLiteral, encodedSchemas, literalSchemas, references);
                                    }
                                }
                            }
                        }
                        if (part.Type != null && !part.Type.IsEmpty)
                        {
                            XmlSchemaType type = FindSchemaType(allSchemas, part.Type);
                            if (type != null)
                            {
                                AddSchema(type.Parent as XmlSchema, isEncoded, isLiteral, encodedSchemas, literalSchemas, references);
                            }
                        }
                    }
                }
            }

            Hashtable imports;
            foreach (XmlSchemas schemas in new XmlSchemas[] { encodedSchemas, literalSchemas })
            {
                // collect all imports
                imports = new Hashtable();
                foreach (XmlSchema schema in schemas)
                {
                    AddImport(schema, imports, allSchemas);
                }
                // make sure we add them to the corresponding schema collections
                foreach (XmlSchema schema in imports.Keys)
                {
                    if (references[schema] == null && !schemas.Contains(schema))
                    {
                        schemas.Add(schema);
                    }
                }
            }

            // If a schema was not referenced by either a literal or an encoded message part,
            // add it to both collections. There's no way to tell which it should be.
            imports = new Hashtable();
            foreach (XmlSchema schema in allSchemas.Schemas())
            {
                if (!encodedSchemas.Contains(schema) && !literalSchemas.Contains(schema))
                {
                    AddImport(schema, imports, allSchemas);
                }
            }

            // make sure we add them to the corresponding schema collections
            foreach (XmlSchema schema in imports.Keys)
            {
                if (references[schema] != null)
                    continue;
                if (!encodedSchemas.Contains(schema))
                {
                    encodedSchemas.Add(schema);
                }
                if (!literalSchemas.Contains(schema))
                {
                    literalSchemas.Add(schema);
                }
            }
            if (encodedSchemas.Count > 0)
            {
                foreach (XmlSchema schema in references.Values)
                {
                    encodedSchemas.AddReference(schema);
                }
            }
            if (literalSchemas.Count > 0)
            {
                foreach (XmlSchema schema in references.Values)
                {
                    literalSchemas.AddReference(schema);
                }
            }
            AddSoapEncodingSchemaIfNeeded(literalSchemas);
        }

        static void AddSoapEncodingSchemaIfNeeded(XmlSchemas schemas)
        {
            XmlSchema fakeXsdSchema = StockSchemas.CreateFakeXsdSchema();

            foreach (XmlSchema schema in schemas)
            {
                foreach (object include in schema.Includes)
                {
                    XmlSchemaImport import = include as XmlSchemaImport;
                    if (import != null && import.Namespace == fakeXsdSchema.TargetNamespace)
                    {
                        schemas.Add(fakeXsdSchema);
                        return;
                    }
                }
            }
        }

        static void AddImport(XmlSchema schema, Hashtable imports, XmlSchemaSet allSchemas)
        {
            if (schema == null || imports[schema] != null)
                return;
            imports.Add(schema, schema);
            foreach (XmlSchemaExternal external in schema.Includes)
            {
                if (external is XmlSchemaImport)
                {
                    XmlSchemaImport import = (XmlSchemaImport)external;
                    foreach (XmlSchema s in allSchemas.Schemas(import.Namespace))
                    {
                        AddImport(s, imports, allSchemas);
                    }
                }
            }
        }

        static void AddSchema(XmlSchema schema, bool isEncoded, bool isLiteral, XmlSchemas encodedSchemas, XmlSchemas literalSchemas, Hashtable references)
        {
            if (schema != null)
            {
                if (isEncoded && !encodedSchemas.Contains(schema))
                {
                    if (references.Contains(schema))
                    {
                        encodedSchemas.AddReference(schema);
                    }
                    else
                    {
                        encodedSchemas.Add(schema);
                    }
                }
                if (isLiteral && !literalSchemas.Contains(schema))
                {
                    if (references.Contains(schema))
                    {
                        literalSchemas.AddReference(schema);
                    }
                    else
                    {
                        literalSchemas.Add(schema);
                    }
                }
            }
        }

        static void FindUse(WsdlNS.MessagePart part, out bool isEncoded, out bool isLiteral)
        {
            isEncoded = false;
            isLiteral = false;
            string messageName = part.Message.Name;
            WsdlNS.Operation associatedOperation = null;
            WsdlNS.ServiceDescription description = part.Message.ServiceDescription;
            foreach (WsdlNS.PortType portType in description.PortTypes)
            {
                foreach (WsdlNS.Operation operation in portType.Operations)
                {
                    foreach (WsdlNS.OperationMessage message in operation.Messages)
                    {
                        if (message.Message.Equals(new XmlQualifiedName(part.Message.Name, description.TargetNamespace)))
                        {
                            associatedOperation = operation;
                            FindUse(associatedOperation, description, messageName, ref isEncoded, ref isLiteral);
                        }
                    }
                }
            }
            if (associatedOperation == null)
                FindUse(null, description, messageName, ref isEncoded, ref isLiteral);
        }

        static void FindUse(WsdlNS.Operation operation, WsdlNS.ServiceDescription description, string messageName, ref bool isEncoded, ref bool isLiteral)
        {
            string targetNamespace = description.TargetNamespace;
            foreach (WsdlNS.Binding binding in description.Bindings)
            {
                if (operation != null && !new XmlQualifiedName(operation.PortType.Name, targetNamespace).Equals(binding.Type))
                    continue;
                foreach (WsdlNS.OperationBinding bindingOperation in binding.Operations)
                {
                    if (bindingOperation.Input != null) foreach (object extension in bindingOperation.Input.Extensions)
                        {
                            if (operation != null)
                            {
                                WsdlNS.SoapBodyBinding body = extension as WsdlNS.SoapBodyBinding;
                                if (body != null && operation.IsBoundBy(bindingOperation))
                                {
                                    if (body.Use == WsdlNS.SoapBindingUse.Encoded)
                                        isEncoded = true;
                                    else if (body.Use == WsdlNS.SoapBindingUse.Literal)
                                        isLiteral = true;
                                }
                            }
                            else
                            {
                                WsdlNS.SoapHeaderBinding header = extension as WsdlNS.SoapHeaderBinding;
                                if (header != null && header.Message.Name == messageName)
                                {
                                    if (header.Use == WsdlNS.SoapBindingUse.Encoded)
                                        isEncoded = true;
                                    else if (header.Use == WsdlNS.SoapBindingUse.Literal)
                                        isLiteral = true;
                                }
                            }
                        }
                    if (bindingOperation.Output != null) foreach (object extension in bindingOperation.Output.Extensions)
                        {
                            if (operation != null)
                            {
                                if (operation.IsBoundBy(bindingOperation))
                                {
                                    WsdlNS.SoapBodyBinding body = extension as WsdlNS.SoapBodyBinding;
                                    if (body != null)
                                    {
                                        if (body.Use == WsdlNS.SoapBindingUse.Encoded)
                                            isEncoded = true;
                                        else if (body.Use == WsdlNS.SoapBindingUse.Literal)
                                            isLiteral = true;
                                    }
                                    else if (extension is WsdlNS.MimeXmlBinding)
                                        isLiteral = true;
                                }
                            }
                            else
                            {
                                WsdlNS.SoapHeaderBinding header = extension as WsdlNS.SoapHeaderBinding;
                                if (header != null && header.Message.Name == messageName)
                                {
                                    if (header.Use == WsdlNS.SoapBindingUse.Encoded)
                                        isEncoded = true;
                                    else if (header.Use == WsdlNS.SoapBindingUse.Literal)
                                        isLiteral = true;
                                }
                            }
                        }
                }
            }
        }

        static string GetLocalElementNamespace(string ns, XmlSchemaElement element, XmlSchemaForm elementFormDefault)
        {
            XmlSchemaForm elementForm = (element.Form != XmlSchemaForm.None) ? element.Form : elementFormDefault;
            if (elementForm != XmlSchemaForm.Qualified)
                return string.Empty;
            return ns;
        }

        static IEnumerable GetSchema(XmlSchemaSet schemaSet, string ns)
        {
            ICollection schemas = schemaSet.Schemas(ns);
            if (schemas == null || schemas.Count == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxSchemaNotFound, ns)));
            return schemas;
        }

        static WsdlNS.SoapBindingStyle GetStyle(WsdlNS.Binding binding)
        {
            WsdlNS.SoapBindingStyle style = WsdlNS.SoapBindingStyle.Default;
            if (binding != null)
            {
                WsdlNS.SoapBinding soapBinding = binding.Extensions.Find(typeof(WsdlNS.SoapBinding)) as WsdlNS.SoapBinding;
                if (soapBinding != null)
                    style = soapBinding.Style;
            }
            return style;
        }

        static OperationFormatStyle GetStyle(WsdlNS.OperationBinding operationBinding)
        {
            WsdlNS.SoapBindingStyle style = GetStyle(operationBinding.Binding);
            if (operationBinding != null)
            {
                WsdlNS.SoapOperationBinding soapOperationBinding = operationBinding.Extensions.Find(typeof(WsdlNS.SoapOperationBinding)) as WsdlNS.SoapOperationBinding;
                if (soapOperationBinding != null)
                {
                    if (soapOperationBinding.Style != WsdlNS.SoapBindingStyle.Default)
                        style = soapOperationBinding.Style;
                }
            }
            return (style == WsdlNS.SoapBindingStyle.Rpc) ? OperationFormatStyle.Rpc : OperationFormatStyle.Document;
        }

        static XmlQualifiedName GetTypeName(XmlSchemaElement element)
        {
            if (element.SchemaType != null)
                return XmlQualifiedName.Empty;
            else if (IsNullOrEmpty(element.SchemaTypeName))
                return AnyType;
            else
                return element.SchemaTypeName;
        }

        static bool IsNullOrEmpty(XmlQualifiedName qname)
        {
            return qname == null || qname.IsEmpty;
        }

        void TraceFaultCannotBeImported(string faultName, string operationName, string message)
        {
            AddWarning(SR.GetString(SR.SFxFaultCannotBeImported, faultName, operationName, message));
        }

        static bool CheckAndAddPart(XmlQualifiedName typeNameFound, XmlQualifiedName typeNameRequired, string name, string ns, Type type, MessageDescription description, bool isReply)
        {
            if (IsNullOrEmpty(typeNameFound) || typeNameFound != typeNameRequired)
                return false;
            MessagePartDescription bodyPart = new MessagePartDescription(name, ns);
            bodyPart.Type = type;
            if (isReply && description.Body.ReturnValue == null)
                description.Body.ReturnValue = bodyPart;
            else
                description.Body.Parts.Add(bodyPart);
            return true;
        }

        static bool CheckPart(XmlQualifiedName typeNameFound, XmlQualifiedName typeNameRequired)
        {
            return !IsNullOrEmpty(typeNameFound) && typeNameFound == typeNameRequired;
        }

        static XmlSchemaComplexType GetElementComplexType(XmlQualifiedName elementName, XmlSchemaSet schemaSet, out string ns, out XmlSchemaForm elementFormDefault)
        {
            XmlSchema schema;
            XmlSchemaElement schemaElement = FindSchemaElement(schemaSet, elementName, out schema);
            ns = elementName.Namespace;
            elementFormDefault = schema.ElementFormDefault;

            XmlSchemaType schemaType = null;
            if (schemaElement.SchemaType != null)
            {
                schemaType = schemaElement.SchemaType;
            }
            else
            {
                XmlQualifiedName schemaTypeName = GetTypeName(schemaElement);
                if (schemaTypeName.Namespace == XmlSchema.Namespace)
                    return null;

                schemaType = FindSchemaType(schemaSet, schemaTypeName, out schema);
                ns = schemaTypeName.Namespace;
                elementFormDefault = schema.ElementFormDefault;
            }
            if (schemaType == null)
                return null;

            return schemaType as XmlSchemaComplexType;
        }

        static XmlSchemaSequence GetRootSequence(XmlSchemaComplexType complexType)
        {
            if (complexType == null)
                return null;
            return complexType.Particle != null ? complexType.Particle as XmlSchemaSequence : null;
        }

        bool CanImportMessageBinding(WsdlNS.MessageBinding messageBinding, WsdlNS.Message wsdlMessage, OperationFormatStyle style, out bool isEncoded)
        {
            isEncoded = false;

            bool? isMessageEncoded = null;
            foreach (object extension in messageBinding.Extensions)
            {
                bool currentIsEncoded;
                WsdlNS.SoapHeaderBinding soapHeaderBinding = extension as WsdlNS.SoapHeaderBinding;

                if (soapHeaderBinding != null)
                {
                    if (!ValidWsdl.Check(soapHeaderBinding, messageBinding, AddWarning))
                        return false;

                    if (!CanImportMessageHeaderBinding(soapHeaderBinding, wsdlMessage, style, out currentIsEncoded))
                        return false;
                    if (isMessageEncoded == null)
                        isMessageEncoded = currentIsEncoded;
                    else if (isMessageEncoded.Value != currentIsEncoded)
                        AddError(SR.GetString(SR.SFxInconsistentWsdlOperationUseInBindingExtensions, messageBinding.OperationBinding.Name, messageBinding.OperationBinding.Binding.Name));
                }
                else
                {
                    WsdlNS.SoapBodyBinding soapBodyBinding = extension as WsdlNS.SoapBodyBinding;
                    if (soapBodyBinding != null)
                    {
                        if (!CanImportMessageBodyBinding(soapBodyBinding, style, out currentIsEncoded))
                            return false;
                        if (isMessageEncoded == null)
                            isMessageEncoded = currentIsEncoded;
                        else if (isMessageEncoded.Value != currentIsEncoded)
                            AddError(SR.GetString(SR.SFxInconsistentWsdlOperationUseInBindingExtensions, messageBinding.OperationBinding.Name, messageBinding.OperationBinding.Binding.Name));
                        string[] messageParts = soapBodyBinding.Parts;
                        if (messageParts == null)
                        {
                            messageParts = new string[wsdlMessage.Parts.Count];
                            for (int i = 0; i < messageParts.Length; i++)
                                messageParts[i] = wsdlMessage.Parts[i].Name;
                        }
                        IList<string> bodyPartsFromBindings;
                        bool isFirstBinding = false;
                        if (!BodyPartsTable.TryGetValue(wsdlMessage, out bodyPartsFromBindings))
                        {
                            bodyPartsFromBindings = new List<string>();
                            BodyPartsTable.Add(wsdlMessage, bodyPartsFromBindings);
                            isFirstBinding = true;
                        }
                        foreach (string partName in messageParts)
                        {
                            if (string.IsNullOrEmpty(partName))
                                continue;
                            if (isFirstBinding)
                                bodyPartsFromBindings.Add(partName);
                            else if (!bodyPartsFromBindings.Contains(partName))
                            {
                                AddError(SR.GetString(SR.SFxInconsistentBindingBodyParts, messageBinding.OperationBinding.Name, messageBinding.OperationBinding.Binding.Name, partName));
                                bodyPartsFromBindings.Add(partName);
                            }
                        }
                    }
                }
            }
            if (isMessageEncoded != null)
                isEncoded = isMessageEncoded.Value;
            return true;
        }

        private bool CanImportFaultBinding(WsdlNS.FaultBinding faultBinding, OperationFormatStyle style, out bool isFaultEncoded)
        {
            bool? isEncoded = null;
            bool currentIsEncoded;
            foreach (object extension in faultBinding.Extensions)
            {
                XmlElement soapFaultBindingRaw = extension as XmlElement;
                if (SoapHelper.IsSoapFaultBinding(soapFaultBindingRaw))
                {
                    currentIsEncoded = SoapHelper.IsEncoded(soapFaultBindingRaw);
                }
                else
                {
                    WsdlNS.SoapFaultBinding soapFaultBinding = extension as WsdlNS.SoapFaultBinding;
                    if (soapFaultBinding == null)
                        continue;

                    if (!ValidWsdl.Check(soapFaultBinding, faultBinding, AddWarning))
                        continue;
                    currentIsEncoded = (soapFaultBinding.Use == System.Web.Services.Description.SoapBindingUse.Encoded);
                }

                if (isEncoded == null)
                    isEncoded = currentIsEncoded;
                else if (isEncoded.Value != currentIsEncoded)
                    AddError(SR.GetString(SR.SFxInconsistentWsdlOperationUseInBindingExtensions, faultBinding.OperationBinding.Name, faultBinding.OperationBinding.Binding.Name));
            }
            isFaultEncoded = isEncoded ?? false;
            return this.CurrentSchemaImporter.CanImportStyleAndUse(style, isFaultEncoded);
        }


        bool CanImportMessageBodyBinding(WsdlNS.SoapBodyBinding bodyBinding, OperationFormatStyle style, out bool isEncoded)
        {
            isEncoded = (bodyBinding.Use == WsdlNS.SoapBindingUse.Encoded);
            return CurrentSchemaImporter.CanImportStyleAndUse(style, isEncoded);
        }

        bool CanImportMessageHeaderBinding(WsdlNS.SoapHeaderBinding headerBinding, WsdlNS.Message wsdlMessage, OperationFormatStyle style, out bool isEncoded)
        {
            isEncoded = (headerBinding.Use == WsdlNS.SoapBindingUse.Encoded);
            WsdlNS.Message wsdlHeaderMessage = wsdlMessage.ServiceDescription.ServiceDescriptions.GetMessage(headerBinding.Message);
            WsdlNS.MessagePart part = FindPartByName(wsdlHeaderMessage, headerBinding.Part);

            OperationFormatStyle headerStyle;
            if (!CurrentSchemaImporter.CanImportMessagePart(part, out headerStyle))
                return false;
            if (headerStyle != style)
                AddError(SR.GetString(SR.SFxInconsistentWsdlOperationStyleInHeader, part.Name, headerStyle, style));
            return CurrentSchemaImporter.CanImportStyleAndUse(style, isEncoded);
        }

        void ImportMessageBinding(WsdlNS.MessageBinding messageBinding, WsdlNS.Message wsdlMessage, MessageDescription description, OperationFormatStyle style, bool isEncoded)
        {
            WsdlNS.OperationMessage wsdlOperationMessage = contractContext.GetOperationMessage(description);
            foreach (object extension in messageBinding.Extensions)
            {
                WsdlNS.SoapHeaderBinding soapHeaderBinding = extension as WsdlNS.SoapHeaderBinding;
                if (soapHeaderBinding != null)
                {
                    ImportMessageHeaderBinding(soapHeaderBinding, wsdlMessage, description, style, isEncoded, messageBinding.OperationBinding.Name);
                }
                else
                {
                    WsdlNS.SoapBodyBinding soapBodyBinding = extension as WsdlNS.SoapBodyBinding;
                    if (soapBodyBinding != null)
                    {
                        ImportMessageBodyBinding(soapBodyBinding, wsdlMessage, description, style, isEncoded, messageBinding.OperationBinding.Name);
                    }
                }
            }
        }

        void ImportMessageBodyBinding(WsdlNS.SoapBodyBinding bodyBinding, WsdlNS.Message wsdlMessage, MessageDescription description, OperationFormatStyle style, bool isEncoded, string operationName)
        {
            if (style == OperationFormatStyle.Rpc && bodyBinding.Namespace != null)
                description.Body.WrapperNamespace = bodyBinding.Namespace;
            this.CurrentSchemaImporter.ValidateStyleAndUse(style, isEncoded, operationName);
        }

        void ImportMessageHeaderBinding(WsdlNS.SoapHeaderBinding headerBinding, WsdlNS.Message wsdlMessage, MessageDescription description, OperationFormatStyle style, bool isEncoded, string operationName)
        {
            WsdlNS.Message wsdlHeaderMessage = wsdlMessage.ServiceDescription.ServiceDescriptions.GetMessage(headerBinding.Message);
            WsdlNS.MessagePart part = FindPartByName(wsdlHeaderMessage, headerBinding.Part);
            if (!description.Headers.Contains(this.CurrentSchemaImporter.GetPartName(part)))
            {
                description.Headers.Add((MessageHeaderDescription)schemaImporter.ImportMessagePart(part, true/*isHeader*/, isEncoded));
                this.CurrentSchemaImporter.ValidateStyleAndUse(style, isEncoded, operationName);
            }
        }


        internal abstract class SchemaImporter
        {
            readonly protected XmlSchemaSet schemaSet;
            readonly protected WsdlImporter importer;

            internal SchemaImporter(WsdlImporter importer)
            {
                this.importer = importer;
                this.schemaSet = GatherSchemas(importer);
            }

            internal XmlQualifiedName GetPartName(WsdlNS.MessagePart part)
            {
                if (!IsNullOrEmpty(part.Element))
                    return part.Element;
                if (!IsNullOrEmpty(part.Type))
                    return new XmlQualifiedName(part.Name, String.Empty);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxWsdlPartMustHaveElementOrType, part.Name, part.Message.Name, part.Message.Namespaces)));
            }

            internal bool CanImportMessagePart(WsdlNS.MessagePart part, out OperationFormatStyle style)
            {
                style = OperationFormatStyle.Document;
                if (!IsNullOrEmpty(part.Element))
                    return CanImportElement(FindSchemaElement(this.schemaSet, part.Element));
                if (!IsNullOrEmpty(part.Type))
                {
                    style = OperationFormatStyle.Rpc;
                    return CanImportType(part.Type);
                }
                return false;
            }

            internal MessagePartDescription ImportMessagePart(WsdlNS.MessagePart part, bool isHeader, bool isEncoded)
            {
                MessagePartDescription bodyPart = null;
                if (!IsNullOrEmpty(part.Element))
                    return ImportParameterElement(part.Element, isHeader, false/*isMultiple*/);
                if (!IsNullOrEmpty(part.Type))
                {
                    bodyPart = isHeader ? (MessagePartDescription)new MessageHeaderDescription(part.Name, String.Empty) : new MessagePartDescription(part.Name, String.Empty);
                    bodyPart.BaseType = ImportType(bodyPart, part.Type, isEncoded);
                    return bodyPart;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxWsdlPartMustHaveElementOrType, part.Name, part.Message.Name, part.Message.Namespaces)));
            }


            internal MessagePartDescription ImportParameterElement(XmlQualifiedName elementName, bool isHeader, bool isMultiple)
            {
                return ImportParameterElement(FindSchemaElement(this.schemaSet, elementName), elementName.Namespace, isHeader, isMultiple);
            }

            internal MessagePartDescription ImportParameterElement(XmlSchemaElement element, string ns, bool isHeader, bool isMultiple)
            {
                if (element.MaxOccurs > 1)
                    isMultiple = true;
                if (!IsNullOrEmpty(element.RefName))
                    return ImportParameterElement(element.RefName, isHeader, isMultiple);

                MessagePartDescription part = isHeader ? (MessagePartDescription)new MessageHeaderDescription(element.Name, ns) : new MessagePartDescription(element.Name, ns);
                part.Multiple = isMultiple;
                part.BaseType = ImportElement(part, element, false/*isEncoded*/);
                return part;
            }

            internal virtual bool CanImportFault(XmlSchemaElement detailElement, XmlQualifiedName detailElementTypeName)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            internal virtual CodeTypeReference ImportFaultElement(XmlQualifiedName elementName, XmlSchemaElement element, bool isEncoded)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            internal virtual CodeTypeReference ImportFaultType(XmlQualifiedName elementName, XmlQualifiedName typeName, bool isEncoded)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            internal virtual void SetOperationSupportFaults(OperationDescription operation, bool supportFaults)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            internal abstract void PreprocessSchema();
            internal abstract void PostprocessSchema(bool used);
            internal abstract bool CanImportStyleAndUse(OperationFormatStyle style, bool isEncoded);
            internal abstract void ValidateStyleAndUse(OperationFormatStyle style, bool isEncoded, string operationName);
            internal abstract IOperationBehavior GetOperationGenerator();
            internal abstract bool CanImportType(XmlQualifiedName typeName);
            internal abstract string ImportType(MessagePartDescription part, XmlQualifiedName typeName, bool isEncoded);
            internal abstract bool CanImportElement(XmlSchemaElement element);
            internal abstract string ImportElement(MessagePartDescription part, XmlSchemaElement element, bool isEncoded);
            internal abstract bool CanImportWrapperElement(XmlQualifiedName elementName);
            internal abstract MessagePartDescription[] ImportWrapperElement(XmlQualifiedName elementName);
            internal abstract void SetOperationStyle(OperationDescription operation, OperationFormatStyle style);
            internal abstract bool GetOperationIsEncoded(OperationDescription operation);
            internal abstract void SetOperationIsEncoded(OperationDescription operation, bool isEncoded);
            internal abstract string GetFormatName();
        }

        internal class DataContractSerializerSchemaImporter : SchemaImporter
        {
            // Same string used in DataContractSet of System.Runtime.Serialization.dll
            internal const String FailedReferenceTypeExceptionKey = "System.Runtime.Serialization.FailedReferenceType";

            DataContractSerializerOperationGenerator DataContractSerializerOperationGenerator;
            ValidationEventHandler compileValidationEventHandler;
            Collection<MetadataConversionError> errors;
            public DataContractSerializerSchemaImporter(WsdlImporter importer)
                : base(importer)
            {
                DataContractSerializerOperationGenerator = new DataContractSerializerOperationGenerator(DataContractImporter.CodeCompileUnit);
            }

            XsdDataContractImporter DataContractImporter
            {
                get
                {
                    object dataContractImporter;
                    if (!importer.State.TryGetValue(typeof(XsdDataContractImporter), out dataContractImporter))
                    {
                        object compileUnit;
                        if (!importer.State.TryGetValue(typeof(CodeCompileUnit), out compileUnit))
                        {
                            compileUnit = new CodeCompileUnit();
                            importer.State.Add(typeof(CodeCompileUnit), compileUnit);
                        }
                        dataContractImporter = new XsdDataContractImporter((CodeCompileUnit)compileUnit);
                        importer.State.Add(typeof(XsdDataContractImporter), dataContractImporter);
                    }
                    return (XsdDataContractImporter)dataContractImporter;
                }
            }

            internal override bool CanImportElement(XmlSchemaElement element)
            {
                if (!element.IsNillable && !SchemaHelper.IsElementValueType(element))
                    return false;
                return DataContractImporter.CanImport(schemaSet, element);
            }


            internal override bool CanImportType(XmlQualifiedName typeName)
            {
                return DataContractImporter.CanImport(schemaSet, typeName);
            }
            internal override bool CanImportWrapperElement(XmlQualifiedName elementName)
            {
                string ns;
                XmlSchemaForm elementFormDefault;
                XmlSchemaComplexType complexType = GetElementComplexType(elementName, schemaSet, out ns, out elementFormDefault);
                if (complexType == null)
                    return false;
                if (complexType.Particle == null)
                    return true;
                XmlSchemaSequence rootSequence = complexType.Particle as XmlSchemaSequence;
                if (rootSequence == null)
                    return false;

                for (int i = 0; i < rootSequence.Items.Count; i++)
                {
                    XmlSchemaElement element = rootSequence.Items[i] as XmlSchemaElement;
                    if (element == null)
                        return false;
                    if (!IsNullOrEmpty(element.RefName))
                        element = FindSchemaElement(this.schemaSet, element.RefName);
                    if (element.MaxOccurs > 1)
                        return false;
                    if (!DataContractImporter.CanImport(schemaSet, element))
                        return false;
                }
                return true;
            }

            internal override bool CanImportFault(XmlSchemaElement detailElement, XmlQualifiedName detailElementTypeName)
            {
                DataContractSerializerSchemaImporter faultImporter = DataContractSerializerSchemaImporter.Get(importer);
                if (IsNullOrEmpty(detailElementTypeName))
                    return faultImporter.CanImportFaultElement(detailElement);
                else
                    return faultImporter.CanImportFaultType(detailElementTypeName);
            }

            internal static DataContractSerializerSchemaImporter Get(WsdlImporter importer)
            {
                Type type = typeof(DataContractSerializerSchemaImporter);
                object schemaImporter;
                if (importer.State.ContainsKey(type))
                    schemaImporter = importer.State[type];
                else
                {
                    schemaImporter = new DataContractSerializerSchemaImporter(importer);
                    importer.State.Add(type, schemaImporter);
                }
                return (DataContractSerializerSchemaImporter)schemaImporter;
            }

            internal override MessagePartDescription[] ImportWrapperElement(XmlQualifiedName elementName)
            {
                string ns;
                XmlSchemaForm elementFormDefault;
                XmlSchemaComplexType complexType = GetElementComplexType(elementName, schemaSet, out ns, out elementFormDefault);
                if (complexType == null)
                    return null;
                if (complexType.Particle == null)
                    return new MessagePartDescription[0];
                XmlSchemaSequence rootSequence = complexType.Particle as XmlSchemaSequence;
                if (rootSequence == null)
                    return null;

                MessagePartDescription[] parts = new MessagePartDescription[rootSequence.Items.Count];

                for (int i = 0; i < rootSequence.Items.Count; i++)
                {
                    XmlSchemaElement localElement = rootSequence.Items[i] as XmlSchemaElement;
                    if (localElement == null)
                        return null;
                    parts[i] = ImportParameterElement(localElement, GetLocalElementNamespace(ns, localElement, elementFormDefault), false/*isHeader*/, false/*isMultiple*/);
                    if (parts[i] == null)
                        return null;
                }
                return parts;
            }

            internal override string ImportType(MessagePartDescription part, XmlQualifiedName typeName, bool isEncoded)
            {
                if (isEncoded)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxDataContractSerializerDoesNotSupportEncoded, part.Name)));
                DataContractImporter.Import(schemaSet, typeName);
                CodeTypeReference typeRef = DataContractImporter.GetCodeTypeReference(typeName);
                ICollection<CodeTypeReference> knownTypeRefs = DataContractImporter.GetKnownTypeReferences(typeName);
                DataContractSerializerOperationGenerator.Add(part, typeRef, knownTypeRefs, false/*IsNonNillableReferenceType*/);
                if (typeRef.ArrayRank == 0)
                    return typeRef.BaseType;
                else
                    return typeRef.BaseType + "[]";
            }

            internal static bool TryGetFailedReferenceType(Exception ex, out Type failedReferenceType)
            {
                if (null == ex)
                    throw new ArgumentNullException("ex");

                if (ex.Data.Contains(FailedReferenceTypeExceptionKey))
                {
                    failedReferenceType = ex.Data[FailedReferenceTypeExceptionKey] as Type;
                    if (null != failedReferenceType)
                    {
                        return true;
                    }
                }
                failedReferenceType = null;
                return false;
            }

            internal override string ImportElement(MessagePartDescription part, XmlSchemaElement element, bool isEncoded)
            {
                if (part.Multiple)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxDataContractSerializerDoesNotSupportBareArray, part.Name)));
                XmlQualifiedName typeName = null;
                Type failedReferenceType;

                while (null == typeName)
                {
                    try
                    {
                        typeName = DataContractImporter.Import(schemaSet, element);
                        break;
                    }
                    catch (InvalidDataContractException ex)
                    {
                        if (TryGetFailedReferenceType(ex, out failedReferenceType))
                        {
                            DataContractImporter.Options.ReferencedTypes.Remove(failedReferenceType);
                            continue;
                        }
                        throw;
                    }
                    catch (InvalidOperationException ex)
                    {
                        if (TryGetFailedReferenceType(ex, out failedReferenceType))
                        {
                            DataContractImporter.Options.ReferencedTypes.Remove(failedReferenceType);
                            continue;
                        }
                        throw;
                    }
                }

                CodeTypeReference typeRef = DataContractImporter.GetCodeTypeReference(typeName, element);
                ICollection<CodeTypeReference> knownTypeRefs = DataContractImporter.GetKnownTypeReferences(typeName);
                DataContractSerializerOperationGenerator.Add(part, typeRef, knownTypeRefs, !element.IsNillable && !IsValueType(typeName));
                if (typeRef.ArrayRank == 0)
                    return typeRef.BaseType;
                else
                    return typeRef.BaseType + "[]";
            }

            private bool IsValueType(XmlQualifiedName typeName)
            {
                XmlSchemaElement element = new XmlSchemaElement();
                element.IsNillable = true;
                CodeTypeReference typeRef = DataContractImporter.GetCodeTypeReference(typeName, element);
                return typeRef.BaseType == typeof(Nullable<>).FullName;
            }

            int SetImportXmlType(bool value)
            {
                if (DataContractImporter.Options == null)
                {
                    DataContractImporter.Options = new ImportOptions();
                    DataContractImporter.Options.ImportXmlType = value;
                    return -1;
                }
                if (DataContractImporter.Options.ImportXmlType != value)
                {
                    DataContractImporter.Options.ImportXmlType = value;
                    return 0;
                }
                return 1;
            }

            void RestoreImportXmlType(int oldValue)
            {
                if (oldValue == 1)
                    return;
                if (oldValue == 0)
                {
                    DataContractImporter.Options.ImportXmlType = !DataContractImporter.Options.ImportXmlType;
                    return;
                }
                DataContractImporter.Options = null;
            }

            internal override CodeTypeReference ImportFaultElement(XmlQualifiedName elementName, XmlSchemaElement element, bool isEncoded)
            {
                int oldValue = SetImportXmlType(true);
                try
                {
                    XmlQualifiedName typeName = DataContractImporter.Import(schemaSet, element);
                    return DataContractImporter.GetCodeTypeReference(typeName, element);
                }
                finally
                {
                    RestoreImportXmlType(oldValue);
                }
            }

            internal bool CanImportFaultElement(XmlSchemaElement element)
            {
                int oldValue = SetImportXmlType(false);
                try
                {
                    return DataContractImporter.CanImport(schemaSet, element);
                }
                finally
                {
                    RestoreImportXmlType(oldValue);
                }
            }

            internal override CodeTypeReference ImportFaultType(XmlQualifiedName elementName, XmlQualifiedName typeName, bool isEncoded)
            {
                int oldValue = SetImportXmlType(true);
                try
                {
                    DataContractImporter.Import(schemaSet, typeName);
                    return DataContractImporter.GetCodeTypeReference(typeName);
                }
                finally
                {
                    RestoreImportXmlType(oldValue);
                }
            }

            internal bool CanImportFaultType(XmlQualifiedName typeName)
            {
                int oldValue = SetImportXmlType(false);
                try
                {
                    return DataContractImporter.CanImport(schemaSet, typeName);
                }
                finally
                {
                    RestoreImportXmlType(oldValue);
                }
            }

            internal override void PreprocessSchema()
            {
                errors = new Collection<MetadataConversionError>();
                compileValidationEventHandler = new ValidationEventHandler(delegate(object sender, ValidationEventArgs args)
                {
                    SchemaHelper.HandleSchemaValidationError(sender, args, errors);
                }
                );
                schemaSet.ValidationEventHandler += compileValidationEventHandler;
            }

            internal override void PostprocessSchema(bool used)
            {
                if (used && errors != null)
                {
                    foreach (MetadataConversionError error in errors)
                        importer.Errors.Add(error);
                    errors.Clear();
                }
                schemaSet.ValidationEventHandler -= compileValidationEventHandler;
            }

            internal override IOperationBehavior GetOperationGenerator()
            {
                return DataContractSerializerOperationGenerator;
            }

            internal override bool CanImportStyleAndUse(OperationFormatStyle style, bool isEncoded)
            {
                return !isEncoded;
            }

            internal override void ValidateStyleAndUse(OperationFormatStyle style, bool isEncoded, string operationName)
            {
                if (isEncoded)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxDataContractSerializerDoesNotSupportEncoded, operationName)));
            }

            internal override void SetOperationStyle(OperationDescription operation, OperationFormatStyle style)
            {
                DataContractSerializerOperationBehavior operationBehavior = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (operationBehavior == null)
                {
                    operationBehavior = new DataContractSerializerOperationBehavior(operation, new DataContractFormatAttribute());
                    operation.Behaviors.Add(operationBehavior);
                }
                operationBehavior.DataContractFormatAttribute.Style = style;
            }

            internal override bool GetOperationIsEncoded(OperationDescription operation)
            {
                return false;
            }

            internal override void SetOperationIsEncoded(OperationDescription operation, bool isEncoded)
            {
                if (isEncoded)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxDataContractSerializerDoesNotSupportEncoded, operation.Name)));
            }

            internal override void SetOperationSupportFaults(OperationDescription operation, bool supportFaults)
            {
                return;
            }

            internal override string GetFormatName()
            {
                return "DataContract";
            }
        }

        internal class XmlSerializerSchemaImporter : SchemaImporter
        {
            XmlSerializerOperationGenerator xmlSerializerOperationGenerator;
            XmlSchemaImporter xmlImporter;
            SoapSchemaImporter soapImporter;
            CodeDomProvider codeProvider;
            XmlSchemas literalSchemas, encodedSchemas;
            public XmlSerializerSchemaImporter(WsdlImporter importer)
                : base(importer)
            {

                XmlSerializerImportOptions options;
                if (importer.State.ContainsKey(typeof(XmlSerializerImportOptions)))
                {
                    options = (XmlSerializerImportOptions)importer.State[typeof(XmlSerializerImportOptions)];
                }
                else
                {
                    object compileUnit;
                    if (!importer.State.TryGetValue(typeof(CodeCompileUnit), out compileUnit))
                    {
                        compileUnit = new CodeCompileUnit();
                        importer.State.Add(typeof(CodeCompileUnit), compileUnit);
                    }
                    options = new XmlSerializerImportOptions((CodeCompileUnit)compileUnit);
                    importer.State.Add(typeof(XmlSerializerImportOptions), options);
                }
                WsdlNS.WebReferenceOptions webReferenceOptions = options.WebReferenceOptions;
                codeProvider = options.CodeProvider;

                encodedSchemas = new XmlSchemas();
                literalSchemas = new XmlSchemas();
                CollectEncodedAndLiteralSchemas(importer.WsdlDocuments, encodedSchemas, literalSchemas, schemaSet);

                CodeIdentifiers codeIdentifiers = new CodeIdentifiers();

                //SchemaImporter.ctor is not thread safe: MB49115, VSWhidbey580396
                lock (schemaImporterLock)
                {
                    xmlImporter = new XmlSchemaImporter(literalSchemas, webReferenceOptions.CodeGenerationOptions, options.CodeProvider, new ImportContext(codeIdentifiers, false));
                }

                if (webReferenceOptions != null)
                {
                    foreach (string extTypeName in webReferenceOptions.SchemaImporterExtensions)
                    {
                        xmlImporter.Extensions.Add(extTypeName, Type.GetType(extTypeName, true /*throwOnError*/));
                    }
                }
                //SchemaImporter.ctor is not thread safe: MB49115, VSWhidbey580396
                lock (schemaImporterLock)
                {
                    soapImporter = new SoapSchemaImporter(encodedSchemas, webReferenceOptions.CodeGenerationOptions, options.CodeProvider, new ImportContext(codeIdentifiers, false));
                }
                xmlSerializerOperationGenerator = new XmlSerializerOperationGenerator(options);
            }

            internal override bool CanImportElement(XmlSchemaElement element)
            {
                return true;
            }

            internal override bool CanImportType(XmlQualifiedName typeName)
            {
                return true;
            }

            internal override bool CanImportWrapperElement(XmlQualifiedName elementName)
            {
                string ns;
                XmlSchemaForm elementFormDefault;
                XmlSchemaComplexType complexType = GetElementComplexType(elementName, schemaSet, out ns, out elementFormDefault);
                if (complexType == null)
                    return false;
                return true;
            }

            internal override bool CanImportFault(XmlSchemaElement detailElement, XmlQualifiedName detailElementTypeName)
            {
                return true;
            }

            internal static XmlSerializerSchemaImporter Get(WsdlImporter importer)
            {
                Type type = typeof(XmlSerializerSchemaImporter);
                object schemaImporter;
                if (importer.State.ContainsKey(type))
                    schemaImporter = importer.State[type];
                else
                {
                    schemaImporter = new XmlSerializerSchemaImporter(importer);
                    importer.State.Add(type, schemaImporter);
                }
                return (XmlSerializerSchemaImporter)schemaImporter;
            }

            internal override MessagePartDescription[] ImportWrapperElement(XmlQualifiedName elementName)
            {
                XmlMembersMapping membersMapping = xmlImporter.ImportMembersMapping(elementName);
                ArrayList parts = new ArrayList();
                for (int i = 0; i < membersMapping.Count; i++)
                {
                    XmlMemberMapping member = membersMapping[i];
                    string xmlName = NamingHelper.XmlName(member.MemberName);
                    MessagePartDescription part = new MessagePartDescription(xmlName, member.Namespace == null ? string.Empty : member.Namespace);
                    xmlSerializerOperationGenerator.Add(part, member, membersMapping, false/*isEncoded*/);
                    part.BaseType = member.GenerateTypeName(codeProvider);
                    parts.Add(part);
                }
                return (MessagePartDescription[])parts.ToArray(typeof(MessagePartDescription));
            }

            internal override CodeTypeReference ImportFaultElement(XmlQualifiedName elementName, XmlSchemaElement element, bool isEncoded)
            {
                if (isEncoded)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxDocEncodedFaultNotSupported)));
                XmlMembersMapping membersMapping = xmlImporter.ImportMembersMapping(new XmlQualifiedName[] { elementName });
                this.xmlSerializerOperationGenerator.XmlExporter.ExportMembersMapping(membersMapping);
                return new CodeTypeReference(this.xmlSerializerOperationGenerator.GetTypeName(membersMapping[0]));
            }

            internal override CodeTypeReference ImportFaultType(XmlQualifiedName elementName, XmlQualifiedName typeName, bool isEncoded)
            {
                XmlName memberName = new XmlName(elementName.Name, true /*isEncoded*/);
                string memberNs = elementName.Namespace;
                XmlMembersMapping membersMapping;
                SoapSchemaMember schemaMember = new SoapSchemaMember();
                schemaMember.MemberName = memberName.EncodedName;
                schemaMember.MemberType = typeName;
                if (isEncoded)
                {
                    membersMapping = soapImporter.ImportMembersMapping(memberName.DecodedName, memberNs, new SoapSchemaMember[] { schemaMember });
                    this.xmlSerializerOperationGenerator.SoapExporter.ExportMembersMapping(membersMapping);
                }
                else
                {
                    membersMapping = xmlImporter.ImportMembersMapping(memberName.DecodedName, memberNs, new SoapSchemaMember[] { schemaMember });
                    this.xmlSerializerOperationGenerator.XmlExporter.ExportMembersMapping(membersMapping);
                }
                return new CodeTypeReference(this.xmlSerializerOperationGenerator.GetTypeName(membersMapping[0]));
            }

            internal override string ImportType(MessagePartDescription part, XmlQualifiedName typeName, bool isEncoded)
            {
                XmlName memberName = new XmlName(part.Name, true /*isEncoded*/);
                string memberNs = part.Namespace;
                XmlMembersMapping membersMapping;
                SoapSchemaMember schemaMember = new SoapSchemaMember();
                schemaMember.MemberName = memberName.EncodedName;
                schemaMember.MemberType = typeName;
                if (isEncoded)
                    membersMapping = soapImporter.ImportMembersMapping(memberName.DecodedName, memberNs, new SoapSchemaMember[] { schemaMember });
                else
                    membersMapping = xmlImporter.ImportMembersMapping(memberName.DecodedName, memberNs, new SoapSchemaMember[] { schemaMember });
                return AddPartType(part, membersMapping, isEncoded);
            }

            internal override string ImportElement(MessagePartDescription part, XmlSchemaElement element, bool isEncoded)
            {
                if (isEncoded)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxDocEncodedNotSupported, part.Name)));
                XmlMembersMapping membersMapping = xmlImporter.ImportMembersMapping(new XmlQualifiedName[] { element.QualifiedName });
                return AddPartType(part, membersMapping, isEncoded);
            }

            private string AddPartType(MessagePartDescription part, XmlMembersMapping membersMapping, bool isEncoded)
            {
                xmlSerializerOperationGenerator.Add(part, membersMapping[0], membersMapping, isEncoded);
                return membersMapping[0].GenerateTypeName(codeProvider);
            }

            internal override void PreprocessSchema()
            {
                XmlSchema wsdl = StockSchemas.CreateWsdl();
                XmlSchema soap = StockSchemas.CreateSoap();
                XmlSchema soapEncoding = StockSchemas.CreateSoapEncoding();
                XmlSchema fakeXsdSchema = StockSchemas.CreateFakeXsdSchema();
                XmlSchema fakeXmlSchema = StockSchemas.CreateFakeXmlSchema();

                schemaSet.Add(wsdl);
                schemaSet.Add(soap);
                schemaSet.Add(soapEncoding);
                schemaSet.Add(fakeXsdSchema);
                schemaSet.Add(fakeXmlSchema);
                SchemaHelper.Compile(schemaSet, importer.Errors);
                schemaSet.Remove(wsdl);
                schemaSet.Remove(soap);
                schemaSet.Remove(soapEncoding);
                schemaSet.Remove(fakeXsdSchema);
                schemaSet.Remove(fakeXmlSchema);
            }

            internal override void PostprocessSchema(bool used)
            {
            }

            internal override IOperationBehavior GetOperationGenerator()
            {
                return xmlSerializerOperationGenerator;
            }

            internal override bool CanImportStyleAndUse(OperationFormatStyle style, bool isEncoded)
            {
                // Intentionally return true in all cases. Warning will be generated later if there are multiple bindings and one is doc-encoded.
                return true;
            }

            internal override void ValidateStyleAndUse(OperationFormatStyle style, bool isEncoded, string operationName)
            {
                if (isEncoded && style != OperationFormatStyle.Rpc)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxDocEncodedNotSupported, operationName)));
            }

            internal static XmlSerializerFormatAttribute GetFormatAttribute(OperationDescription operation, bool createNew)
            {
                XmlSerializerOperationBehavior operationBehavior = operation.Behaviors.Find<XmlSerializerOperationBehavior>();

                if (operationBehavior != null)
                    return operationBehavior.XmlSerializerFormatAttribute;
                if (!createNew)
                    return null;
                operationBehavior = new XmlSerializerOperationBehavior(operation);
                operation.Behaviors.Add(operationBehavior);
                return operationBehavior.XmlSerializerFormatAttribute;
            }

            internal override void SetOperationStyle(OperationDescription operation, OperationFormatStyle style)
            {
                XmlSerializerFormatAttribute operationAttribute = GetFormatAttribute(operation, true/*createNew*/);
                operationAttribute.Style = style;
            }

            internal override bool GetOperationIsEncoded(OperationDescription operation)
            {
                XmlSerializerFormatAttribute operationAttribute = GetFormatAttribute(operation, false /*createNew*/);
                if (operationAttribute == null)
                    return TypeLoader.DefaultXmlSerializerFormatAttribute.IsEncoded;
                return operationAttribute.IsEncoded;
            }

            internal override void SetOperationIsEncoded(OperationDescription operation, bool isEncoded)
            {
                XmlSerializerFormatAttribute operationAttribute = GetFormatAttribute(operation, true/*createNew*/);
                operationAttribute.IsEncoded = isEncoded;
            }

            internal override void SetOperationSupportFaults(OperationDescription operation, bool supportFaults)
            {
                XmlSerializerFormatAttribute operationAttribute = GetFormatAttribute(operation, true/*createNew*/);
                operationAttribute.SupportFaults = supportFaults;
            }

            internal override string GetFormatName()
            {
                return "XmlSerializer";
            }
        }

        class OperationInfo
        {
            OperationFormatStyle style;
            bool isEncoded;
            bool areAllMessagesWrapped;

            internal OperationInfo(OperationFormatStyle style, bool isEncoded, bool areAllMessagesWrapped)
            {
                this.style = style;
                this.isEncoded = isEncoded;
                this.areAllMessagesWrapped = areAllMessagesWrapped;
            }
            internal OperationFormatStyle Style { get { return style; } }
            internal bool IsEncoded { get { return isEncoded; } }
            internal bool AreAllMessagesWrapped { get { return areAllMessagesWrapped; } }
        }
    }

    internal delegate void WsdlWarningHandler(string warning);

    static class ValidWsdl
    {
        internal static bool Check(WsdlNS.SoapHeaderBinding soapHeaderBinding, WsdlNS.MessageBinding messageBinding, WsdlWarningHandler warningHandler)
        {
            if (soapHeaderBinding.Message == null || soapHeaderBinding.Message.IsEmpty)
            {
                string reason = SR.GetString(SR.XsdMissingRequiredAttribute1, "message");
                string warning = SR.GetString(SR.IgnoreSoapHeaderBinding3, messageBinding.OperationBinding.Name, messageBinding.OperationBinding.Binding.ServiceDescription.TargetNamespace, reason);
                warningHandler(warning);
                return false;
            }

            if (string.IsNullOrEmpty(soapHeaderBinding.Part))
            {
                string reason = SR.GetString(SR.XsdMissingRequiredAttribute1, "part");
                string warning = SR.GetString(SR.IgnoreSoapHeaderBinding3, messageBinding.OperationBinding.Name, messageBinding.OperationBinding.Binding.ServiceDescription.TargetNamespace, reason);
                warningHandler(warning);
                return false;
            }
            return true;
        }

        internal static bool Check(WsdlNS.SoapFaultBinding soapFaultBinding, WsdlNS.FaultBinding faultBinding, WsdlWarningHandler warningHandler)
        {
            if (string.IsNullOrEmpty(soapFaultBinding.Name))
            {
                string reason = SR.GetString(SR.XsdMissingRequiredAttribute1, "name");
                string warning = SR.GetString(SR.IgnoreSoapFaultBinding3, faultBinding.OperationBinding.Name, faultBinding.OperationBinding.Binding.ServiceDescription.TargetNamespace, reason);
                warningHandler(warning);
                return false;
            }
            return true;
        }

        internal static bool Check(WsdlNS.MessagePart part, WsdlNS.Message message, WsdlWarningHandler warningHandler)
        {
            // check required name attribute, do not check NCName validity
            if (string.IsNullOrEmpty(part.Name))
            {
                string reason = SR.GetString(SR.XsdMissingRequiredAttribute1, "name");
                string warning = SR.GetString(SR.IgnoreMessagePart3, message.Name, message.ServiceDescription.TargetNamespace, reason);
                warningHandler(warning);
                return false;
            }
            return true;
        }
    }
}
