//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSerializerOperationBehavior : IOperationBehavior, IWsdlExportExtension
    {
        readonly Reflector.OperationReflector reflector;
        readonly bool builtInOperationBehavior;

        public XmlSerializerOperationBehavior(OperationDescription operation)
            : this(operation, null)
        {
        }

        public XmlSerializerOperationBehavior(OperationDescription operation, XmlSerializerFormatAttribute attribute)
        {
            if (operation == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operation");
#pragma warning suppress 56506 // Declaring contract cannot be null
            Reflector parentReflector = new Reflector(operation.DeclaringContract.Namespace, operation.DeclaringContract.ContractType);
#pragma warning suppress 56506 // parentReflector cannot be null
            this.reflector = parentReflector.ReflectOperation(operation, attribute ?? new XmlSerializerFormatAttribute());
        }

        internal XmlSerializerOperationBehavior(OperationDescription operation, XmlSerializerFormatAttribute attribute, Reflector parentReflector)
            : this(operation, attribute)
        {
            // used by System.ServiceModel.Web
            this.reflector = parentReflector.ReflectOperation(operation, attribute ?? new XmlSerializerFormatAttribute());
        }

        XmlSerializerOperationBehavior(Reflector.OperationReflector reflector, bool builtInOperationBehavior)
        {
            Fx.Assert(reflector != null, "");
            this.reflector = reflector;
            this.builtInOperationBehavior = builtInOperationBehavior;
        }

        internal Reflector.OperationReflector OperationReflector
        {
            get { return this.reflector; }
        }

        internal bool IsBuiltInOperationBehavior
        {
            get { return this.builtInOperationBehavior; }
        }

        public XmlSerializerFormatAttribute XmlSerializerFormatAttribute
        {
            get
            {
                return this.reflector.Attribute;
            }
        }

        internal static XmlSerializerOperationFormatter CreateOperationFormatter(OperationDescription operation)
        {
            return new XmlSerializerOperationBehavior(operation).CreateFormatter();
        }

        internal static XmlSerializerOperationFormatter CreateOperationFormatter(OperationDescription operation, XmlSerializerFormatAttribute attr)
        {
            return new XmlSerializerOperationBehavior(operation, attr).CreateFormatter();
        }

        internal static void AddBehaviors(ContractDescription contract)
        {
            AddBehaviors(contract, false);
        }

        internal static void AddBuiltInBehaviors(ContractDescription contract)
        {
            AddBehaviors(contract, true);
        }

        static void AddBehaviors(ContractDescription contract, bool builtInOperationBehavior)
        {
            Reflector reflector = new Reflector(contract.Namespace, contract.ContractType);

            foreach (OperationDescription operation in contract.Operations)
            {

                Reflector.OperationReflector operationReflector = reflector.ReflectOperation(operation);
                if (operationReflector != null)
                {
                    bool isInherited = operation.DeclaringContract != contract;
                    if (!isInherited)
                    {
                        operation.Behaviors.Add(new XmlSerializerOperationBehavior(operationReflector, builtInOperationBehavior));
                        operation.Behaviors.Add(new XmlSerializerOperationGenerator(new XmlSerializerImportOptions()));
                    }
                }
            }
        }

        internal XmlSerializerOperationFormatter CreateFormatter()
        {
            return new XmlSerializerOperationFormatter(reflector.Operation, reflector.Attribute, reflector.Request, reflector.Reply);
        }

        XmlSerializerFaultFormatter CreateFaultFormatter(SynchronizedCollection<FaultContractInfo> faultContractInfos)
        {
            return new XmlSerializerFaultFormatter(faultContractInfos, reflector.XmlSerializerFaultContractInfos);
        }

        void IOperationBehavior.Validate(OperationDescription description)
        {
        }

        void IOperationBehavior.AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
        {
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch)
        {
            if (description == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");

            if (dispatch == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatch");

            if (dispatch.Formatter == null)
            {
                dispatch.Formatter = (IDispatchMessageFormatter)CreateFormatter();
                dispatch.DeserializeRequest = reflector.RequestRequiresSerialization;
                dispatch.SerializeReply = reflector.ReplyRequiresSerialization;
            }

            if (reflector.Attribute.SupportFaults)
            {
                if (!dispatch.IsFaultFormatterSetExplicit)
                {
                    dispatch.FaultFormatter = (IDispatchFaultFormatter)CreateFaultFormatter(dispatch.FaultContractInfos);
                }
                else
                {
                    var wrapper = dispatch.FaultFormatter as IDispatchFaultFormatterWrapper;
                    if (wrapper != null)
                    {
                        wrapper.InnerFaultFormatter = (IDispatchFaultFormatter)CreateFaultFormatter(dispatch.FaultContractInfos);
                    }
                }
            }
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription description, ClientOperation proxy)
        {
            if (description == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");

            if (proxy == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("proxy");

            if (proxy.Formatter == null)
            {
                proxy.Formatter = (IClientMessageFormatter)CreateFormatter();
                proxy.SerializeRequest = reflector.RequestRequiresSerialization;
                proxy.DeserializeReply = reflector.ReplyRequiresSerialization;
            }

            if (reflector.Attribute.SupportFaults && !proxy.IsFaultFormatterSetExplicit)
                proxy.FaultFormatter = (IClientFaultFormatter)CreateFaultFormatter(proxy.FaultContractInfos);
        }

        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext)
        {
            if (exporter == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            if (endpointContext == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointContext");

            MessageContractExporter.ExportMessageBinding(exporter, endpointContext, typeof(XmlSerializerMessageContractExporter), this.reflector.Operation);
        }

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext contractContext)
        {
            if (exporter == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            if (contractContext == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractContext");
            new XmlSerializerMessageContractExporter(exporter, contractContext, this.reflector.Operation, this).ExportMessageContract();
        }

        public Collection<XmlMapping> GetXmlMappings()
        {
            Collection<XmlMapping> mappings = new Collection<XmlMapping>();
            if (OperationReflector.Request != null && OperationReflector.Request.HeadersMapping != null)
                mappings.Add(OperationReflector.Request.HeadersMapping);
            if (OperationReflector.Request != null && OperationReflector.Request.BodyMapping != null)
                mappings.Add(OperationReflector.Request.BodyMapping);
            if (OperationReflector.Reply != null && OperationReflector.Reply.HeadersMapping != null)
                mappings.Add(OperationReflector.Reply.HeadersMapping);
            if (OperationReflector.Reply != null && OperationReflector.Reply.BodyMapping != null)
                mappings.Add(OperationReflector.Reply.BodyMapping);
            return mappings;
        }

        // helper for reflecting operations
        internal class Reflector
        {
            readonly XmlSerializerImporter importer;
            readonly SerializerGenerationContext generation;
            Collection<OperationReflector> operationReflectors = new Collection<OperationReflector>();
            object thisLock = new object();

            internal Reflector(string defaultNs, Type type)
            {
                this.importer = new XmlSerializerImporter(defaultNs);
                this.generation = new SerializerGenerationContext(type);
            }

            internal void EnsureMessageInfos()
            {
                lock (this.thisLock)
                {
                    foreach (OperationReflector operationReflector in operationReflectors)
                    {
                        operationReflector.EnsureMessageInfos();
                    }
                }
            }


            static XmlSerializerFormatAttribute FindAttribute(OperationDescription operation)
            {
                Type contractType = operation.DeclaringContract != null ? operation.DeclaringContract.ContractType : null;
                XmlSerializerFormatAttribute contractFormatAttribute = contractType != null ? TypeLoader.GetFormattingAttribute(contractType, null) as XmlSerializerFormatAttribute : null;
                return TypeLoader.GetFormattingAttribute(operation.OperationMethod, contractFormatAttribute) as XmlSerializerFormatAttribute;
            }

            // auto-reflects the operation, returning null if no attribute was found or inherited
            internal OperationReflector ReflectOperation(OperationDescription operation)
            {
                XmlSerializerFormatAttribute attr = FindAttribute(operation);
                if (attr == null)
                    return null;

                return ReflectOperation(operation, attr);
            }

            // overrides the auto-reflection with an attribute
            internal OperationReflector ReflectOperation(OperationDescription operation, XmlSerializerFormatAttribute attrOverride)
            {
                OperationReflector operationReflector = new OperationReflector(this, operation, attrOverride, true/*reflectOnDemand*/);
                operationReflectors.Add(operationReflector);

                return operationReflector;
            }

            internal class OperationReflector
            {
                readonly Reflector parent;

                internal readonly OperationDescription Operation;
                internal readonly XmlSerializerFormatAttribute Attribute;

                internal readonly bool IsEncoded;
                internal readonly bool IsRpc;
                internal readonly bool IsOneWay;
                internal readonly bool RequestRequiresSerialization;
                internal readonly bool ReplyRequiresSerialization;

                readonly string keyBase;

                MessageInfo request;
                MessageInfo reply;
                SynchronizedCollection<XmlSerializerFaultContractInfo> xmlSerializerFaultContractInfos;

                internal OperationReflector(Reflector parent, OperationDescription operation, XmlSerializerFormatAttribute attr, bool reflectOnDemand)
                {
                    Fx.Assert(parent != null, "");
                    Fx.Assert(operation != null, "");
                    Fx.Assert(attr != null, "");

                    OperationFormatter.Validate(operation, attr.Style == OperationFormatStyle.Rpc, attr.IsEncoded);

                    this.parent = parent;

                    this.Operation = operation;
                    this.Attribute = attr;

                    this.IsEncoded = attr.IsEncoded;
                    this.IsRpc = (attr.Style == OperationFormatStyle.Rpc);
                    this.IsOneWay = operation.Messages.Count == 1;

                    this.RequestRequiresSerialization = !operation.Messages[0].IsUntypedMessage;
                    this.ReplyRequiresSerialization = !this.IsOneWay && !operation.Messages[1].IsUntypedMessage;

                    MethodInfo methodInfo = operation.OperationMethod;
                    if (methodInfo == null)
                    {
                        // keyBase needs to be unique within the scope of the parent reflector
                        keyBase = string.Empty;
                        if (operation.DeclaringContract != null)
                        {
                            keyBase = operation.DeclaringContract.Name + "," + operation.DeclaringContract.Namespace + ":";
                        }
                        keyBase = keyBase + operation.Name;
                    }
                    else
                        keyBase = methodInfo.DeclaringType.FullName + ":" + methodInfo.ToString();

                    foreach (MessageDescription message in operation.Messages)
                        foreach (MessageHeaderDescription header in message.Headers)
                            SetUnknownHeaderInDescription(header);
                    if (!reflectOnDemand)
                    {
                        this.EnsureMessageInfos();
                    }
                }

                private void SetUnknownHeaderInDescription(MessageHeaderDescription header)
                {
                    if (this.IsEncoded) //XmlAnyElementAttribute does not apply
                        return;
                    if (header.AdditionalAttributesProvider != null)
                    {
                        XmlAttributes xmlAttributes = new XmlAttributes(header.AdditionalAttributesProvider);
                        foreach (XmlAnyElementAttribute anyElement in xmlAttributes.XmlAnyElements)
                        {
                            if (String.IsNullOrEmpty(anyElement.Name))
                            {
                                header.IsUnknownHeaderCollection = true;
                            }
                        }
                    }
                }

                string ContractName
                {
                    get { return this.Operation.DeclaringContract.Name; }
                }

                string ContractNamespace
                {
                    get { return this.Operation.DeclaringContract.Namespace; }
                }

                internal MessageInfo Request
                {
                    get
                    {
                        parent.EnsureMessageInfos();
                        return this.request;
                    }
                }

                internal MessageInfo Reply
                {
                    get
                    {
                        parent.EnsureMessageInfos();
                        return this.reply;
                    }
                }

                internal SynchronizedCollection<XmlSerializerFaultContractInfo> XmlSerializerFaultContractInfos
                {
                    get
                    {
                        parent.EnsureMessageInfos();
                        return this.xmlSerializerFaultContractInfos;
                    }
                }

                internal void EnsureMessageInfos()
                {
                    if (this.request == null)
                    {
                        foreach (Type knownType in Operation.KnownTypes)
                        {
                            if (knownType == null)
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxKnownTypeNull, Operation.Name)));
                            parent.importer.IncludeType(knownType, IsEncoded);
                        }
                        this.request = CreateMessageInfo(this.Operation.Messages[0], ":Request");
                        if (this.request != null && this.IsRpc && this.Operation.IsValidateRpcWrapperName && this.request.BodyMapping.XsdElementName != this.Operation.Name)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxRpcMessageBodyPartNameInvalid, Operation.Name, this.Operation.Messages[0].MessageName, request.BodyMapping.XsdElementName, this.Operation.Name)));
                        if (!this.IsOneWay)
                        {
                            this.reply = CreateMessageInfo(this.Operation.Messages[1], ":Response");
                            XmlName responseName = TypeLoader.GetBodyWrapperResponseName(this.Operation.Name);
                            if (this.reply != null && this.IsRpc && this.Operation.IsValidateRpcWrapperName && this.reply.BodyMapping.XsdElementName != responseName.EncodedName)
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxRpcMessageBodyPartNameInvalid, Operation.Name, this.Operation.Messages[1].MessageName, reply.BodyMapping.XsdElementName, responseName.EncodedName)));
                        }
                        if (this.Attribute.SupportFaults)
                        {
                            GenerateXmlSerializerFaultContractInfos();
                        }
                    }
                }

                void GenerateXmlSerializerFaultContractInfos()
                {
                    SynchronizedCollection<XmlSerializerFaultContractInfo> faultInfos = new SynchronizedCollection<XmlSerializerFaultContractInfo>();
                    for (int i = 0; i < this.Operation.Faults.Count; i++)
                    {
                        FaultDescription fault = this.Operation.Faults[i];
                        FaultContractInfo faultContractInfo = new FaultContractInfo(fault.Action, fault.DetailType, fault.ElementName, fault.Namespace, this.Operation.KnownTypes);

                        XmlQualifiedName elementName;
                        XmlMembersMapping xmlMembersMapping = this.ImportFaultElement(fault, out elementName);

                        SerializerStub serializerStub = parent.generation.AddSerializer(xmlMembersMapping);
                        faultInfos.Add(new XmlSerializerFaultContractInfo(faultContractInfo, serializerStub, elementName));
                    }
                    this.xmlSerializerFaultContractInfos = faultInfos;
                }

                MessageInfo CreateMessageInfo(MessageDescription message, string key)
                {
                    if (message.IsUntypedMessage)
                        return null;
                    MessageInfo info = new MessageInfo();
                    if (message.IsTypedMessage)
                        key = message.MessageType.FullName + ":" + IsEncoded + ":" + IsRpc;
                    XmlMembersMapping headersMapping = LoadHeadersMapping(message, key + ":Headers");
                    info.SetHeaders(parent.generation.AddSerializer(headersMapping));
                    MessagePartDescriptionCollection rpcEncodedTypedMessgeBodyParts;
                    info.SetBody(parent.generation.AddSerializer(LoadBodyMapping(message, key, out rpcEncodedTypedMessgeBodyParts)), rpcEncodedTypedMessgeBodyParts);
                    CreateHeaderDescriptionTable(message, info, headersMapping);
                    return info;
                }

                private void CreateHeaderDescriptionTable(MessageDescription message, MessageInfo info, XmlMembersMapping headersMapping)
                {
                    int headerNameIndex = 0;
                    OperationFormatter.MessageHeaderDescriptionTable headerDescriptionTable = new OperationFormatter.MessageHeaderDescriptionTable();
                    info.SetHeaderDescriptionTable(headerDescriptionTable);
                    foreach (MessageHeaderDescription header in message.Headers)
                    {
                        if (header.IsUnknownHeaderCollection)
                            info.SetUnknownHeaderDescription(header);
                        else if (headersMapping != null)
                        {
                            XmlMemberMapping memberMapping = headersMapping[headerNameIndex++];
                            string headerName, headerNs;
                            if (IsEncoded)
                            {
                                headerName = memberMapping.TypeName;
                                headerNs = memberMapping.TypeNamespace;
                            }
                            else
                            {
                                headerName = memberMapping.XsdElementName;
                                headerNs = memberMapping.Namespace;
                            }
                            if (headerName != header.Name)
                            {
                                if (message.MessageType != null)
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxHeaderNameMismatchInMessageContract, message.MessageType, header.MemberInfo.Name, header.Name, headerName)));
                                else
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxHeaderNameMismatchInOperation, this.Operation.Name, this.Operation.DeclaringContract.Name, this.Operation.DeclaringContract.Namespace, header.Name, headerName)));
                            }
                            if (headerNs != header.Namespace)
                            {
                                if (message.MessageType != null)
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxHeaderNamespaceMismatchInMessageContract, message.MessageType, header.MemberInfo.Name, header.Namespace, headerNs)));
                                else
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxHeaderNamespaceMismatchInOperation, this.Operation.Name, this.Operation.DeclaringContract.Name, this.Operation.DeclaringContract.Namespace, header.Namespace, headerNs)));
                            }

                            headerDescriptionTable.Add(headerName, headerNs, header);
                        }
                    }
                }

                XmlMembersMapping LoadBodyMapping(MessageDescription message, string mappingKey, out MessagePartDescriptionCollection rpcEncodedTypedMessageBodyParts)
                {
                    MessagePartDescription returnPart;
                    string wrapperName, wrapperNs;
                    MessagePartDescriptionCollection bodyParts;
                    if (IsEncoded && message.IsTypedMessage && message.Body.WrapperName == null)
                    {
                        MessagePartDescription wrapperPart = GetWrapperPart(message);
                        returnPart = null;
                        rpcEncodedTypedMessageBodyParts = bodyParts = GetWrappedParts(wrapperPart);
                        wrapperName = wrapperPart.Name;
                        wrapperNs = wrapperPart.Namespace;
                    }
                    else
                    {
                        rpcEncodedTypedMessageBodyParts = null;
                        returnPart = OperationFormatter.IsValidReturnValue(message.Body.ReturnValue) ? message.Body.ReturnValue : null;
                        bodyParts = message.Body.Parts;
                        wrapperName = message.Body.WrapperName;
                        wrapperNs = message.Body.WrapperNamespace;
                    }
                    bool isWrapped = (wrapperName != null);
                    bool hasReturnValue = returnPart != null;
                    int paramCount = bodyParts.Count + (hasReturnValue ? 1 : 0);
                    if (paramCount == 0 && !isWrapped) // no need to create serializer
                    {
                        return null;
                    }

                    XmlReflectionMember[] members = new XmlReflectionMember[paramCount];
                    int paramIndex = 0;
                    if (hasReturnValue)
                        members[paramIndex++] = XmlSerializerHelper.GetXmlReflectionMember(returnPart, IsRpc, IsEncoded, isWrapped);

                    for (int i = 0; i < bodyParts.Count; i++)
                        members[paramIndex++] = XmlSerializerHelper.GetXmlReflectionMember(bodyParts[i], IsRpc, IsEncoded, isWrapped);

                    if (!isWrapped)
                        wrapperNs = ContractNamespace;
                    return ImportMembersMapping(wrapperName, wrapperNs, members, isWrapped, IsRpc, mappingKey);
                }

                private MessagePartDescription GetWrapperPart(MessageDescription message)
                {
                    if (message.Body.Parts.Count != 1)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxRpcMessageMustHaveASingleBody, Operation.Name, message.MessageName)));
                    MessagePartDescription bodyPart = message.Body.Parts[0];
                    Type bodyObjectType = bodyPart.Type;
                    if (bodyObjectType.BaseType != null && bodyObjectType.BaseType != typeof(object))
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxBodyObjectTypeCannotBeInherited, bodyObjectType.FullName)));
                    if (typeof(IEnumerable).IsAssignableFrom(bodyObjectType))
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxBodyObjectTypeCannotBeInterface, bodyObjectType.FullName, typeof(IEnumerable).FullName)));
                    if (typeof(IXmlSerializable).IsAssignableFrom(bodyObjectType))
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxBodyObjectTypeCannotBeInterface, bodyObjectType.FullName, typeof(IXmlSerializable).FullName)));
                    return bodyPart;
                }

                private MessagePartDescriptionCollection GetWrappedParts(MessagePartDescription bodyPart)
                {
                    Type bodyObjectType = bodyPart.Type;
                    MessagePartDescriptionCollection partList = new MessagePartDescriptionCollection();
                    foreach (MemberInfo member in bodyObjectType.GetMembers(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if ((member.MemberType & (MemberTypes.Field | MemberTypes.Property)) == 0)
                            continue;
                        if (member.IsDefined(typeof(SoapIgnoreAttribute), false/*inherit*/))
                            continue;
                        XmlName xmlName = new XmlName(member.Name);
                        MessagePartDescription part = new MessagePartDescription(xmlName.EncodedName, string.Empty);
                        part.AdditionalAttributesProvider = part.MemberInfo = member;
                        part.Index = part.SerializationPosition = partList.Count;
                        part.Type = (member.MemberType == MemberTypes.Property) ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;
                        if (bodyPart.HasProtectionLevel)
                            part.ProtectionLevel = bodyPart.ProtectionLevel;
                        partList.Add(part);
                    }
                    return partList;
                }

                XmlMembersMapping LoadHeadersMapping(MessageDescription message, string mappingKey)
                {
                    int headerCount = message.Headers.Count;

                    if (headerCount == 0)
                        return null;
                    if (IsEncoded)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxHeadersAreNotSupportedInEncoded, message.MessageName)));

                    int unknownHeaderCount = 0, headerIndex = 0;
                    XmlReflectionMember[] members = new XmlReflectionMember[headerCount];
                    for (int i = 0; i < headerCount; i++)
                    {
                        MessageHeaderDescription header = message.Headers[i];
                        if (!header.IsUnknownHeaderCollection)
                        {
                            members[headerIndex++] = XmlSerializerHelper.GetXmlReflectionMember(header, false/*isRpc*/, IsEncoded, false/*isWrapped*/);
                        }
                        else
                        {
                            unknownHeaderCount++;
                        }
                    }

                    if (unknownHeaderCount == headerCount)
                    {
                        return null;
                    }

                    if (unknownHeaderCount > 0)
                    {
                        XmlReflectionMember[] newMembers = new XmlReflectionMember[headerCount - unknownHeaderCount];
                        Array.Copy(members, newMembers, newMembers.Length);
                        members = newMembers;
                    }

                    return ImportMembersMapping(ContractName, ContractNamespace, members, false /*isWrapped*/, false /*isRpc*/, mappingKey);
                }

                internal XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool rpc, string mappingKey)
                {
                    string key = mappingKey.StartsWith(":", StringComparison.Ordinal) ? keyBase + mappingKey : mappingKey;
                    return this.parent.importer.ImportMembersMapping(new XmlName(elementName, true /*isEncoded*/), ns, members, hasWrapperElement, rpc, this.IsEncoded, key);
                }

                internal XmlMembersMapping ImportFaultElement(FaultDescription fault, out XmlQualifiedName elementName)
                {
                    // the number of reflection members is always 1 because there is only one fault detail type
                    XmlReflectionMember[] members = new XmlReflectionMember[1];

                    XmlName faultElementName = fault.ElementName;
                    string faultNamespace = fault.Namespace;
                    if (faultElementName == null)
                    {
                        XmlTypeMapping mapping = this.parent.importer.ImportTypeMapping(fault.DetailType, this.IsEncoded);
                        faultElementName = new XmlName(mapping.ElementName, this.IsEncoded);
                        faultNamespace = mapping.Namespace;
                        if (faultElementName == null)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxFaultTypeAnonymous, this.Operation.Name, fault.DetailType.FullName)));
                    }

                    elementName = new XmlQualifiedName(faultElementName.DecodedName, faultNamespace);

                    members[0] = XmlSerializerHelper.GetXmlReflectionMember(null /*memberName*/, faultElementName, faultNamespace, fault.DetailType,
                        null /*additionalAttributesProvider*/, false /*isMultiple*/, this.IsEncoded, false /*isWrapped*/);

                    string mappingKey = "fault:" + faultElementName.DecodedName + ":" + faultNamespace;
                    return ImportMembersMapping(faultElementName.EncodedName, faultNamespace, members, false /*hasWrapperElement*/, this.IsRpc, mappingKey);
                }
            }

            class XmlSerializerImporter
            {
                readonly string defaultNs;
                XmlReflectionImporter xmlImporter;
                SoapReflectionImporter soapImporter;
                Dictionary<string, XmlMembersMapping> xmlMappings;

                internal XmlSerializerImporter(string defaultNs)
                {
                    this.defaultNs = defaultNs;
                    this.xmlImporter = null;
                    this.soapImporter = null;
                }

                SoapReflectionImporter SoapImporter
                {
                    get
                    {
                        if (this.soapImporter == null)
                        {
                            this.soapImporter = new SoapReflectionImporter(NamingHelper.CombineUriStrings(defaultNs, "encoded"));
                        }
                        return this.soapImporter;
                    }
                }

                XmlReflectionImporter XmlImporter
                {
                    get
                    {
                        if (this.xmlImporter == null)
                        {
                            this.xmlImporter = new XmlReflectionImporter(defaultNs);
                        }
                        return this.xmlImporter;
                    }
                }

                Dictionary<string, XmlMembersMapping> XmlMappings
                {
                    get
                    {
                        if (this.xmlMappings == null)
                        {
                            this.xmlMappings = new Dictionary<string, XmlMembersMapping>();
                        }
                        return this.xmlMappings;
                    }
                }

                internal XmlMembersMapping ImportMembersMapping(XmlName elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool rpc, bool isEncoded, string mappingKey)
                {
                    XmlMembersMapping mapping;
                    string mappingName = elementName.DecodedName;
                    if (XmlMappings.TryGetValue(mappingKey, out mapping))
                    {
                        return mapping;
                    }

                    if (isEncoded)
                        mapping = this.SoapImporter.ImportMembersMapping(mappingName, ns, members, hasWrapperElement, rpc);
                    else
                        mapping = this.XmlImporter.ImportMembersMapping(mappingName, ns, members, hasWrapperElement, rpc);

                    mapping.SetKey(mappingKey);
                    XmlMappings.Add(mappingKey, mapping);
                    return mapping;
                }

                internal XmlTypeMapping ImportTypeMapping(Type type, bool isEncoded)
                {
                    if (isEncoded)
                        return this.SoapImporter.ImportTypeMapping(type);
                    else
                        return this.XmlImporter.ImportTypeMapping(type);
                }

                internal void IncludeType(Type knownType, bool isEncoded)
                {
                    if (isEncoded)
                        this.SoapImporter.IncludeType(knownType);
                    else
                        this.XmlImporter.IncludeType(knownType);
                }
            }

            internal class SerializerGenerationContext
            {
                List<XmlMembersMapping> Mappings = new List<XmlMembersMapping>();
                XmlSerializer[] serializers = null;
                Type type;
                object thisLock = new object();

                internal SerializerGenerationContext(Type type)
                {
                    this.type = type;
                }

                // returns a stub to a serializer
                internal SerializerStub AddSerializer(XmlMembersMapping mapping)
                {
                    int handle = -1;
                    if (mapping != null)
                    {
                        handle = ((IList)Mappings).Add(mapping);
                    }

                    return new SerializerStub(this, mapping, handle);
                }

                internal XmlSerializer GetSerializer(int handle)
                {
                    if (handle < 0)
                    {
                        return null;
                    }

                    if (this.serializers == null)
                    {
                        lock (this.thisLock)
                        {
                            if (this.serializers == null)
                            {
                                this.serializers = GenerateSerializers();
                            }
                        }
                    }
                    return this.serializers[handle];
                }

                XmlSerializer[] GenerateSerializers()
                {
                    //this.Mappings may have duplicate mappings (for e.g. samed message contract is used by more than one operation)
                    //XmlSerializer.FromMappings require unique mappings. The following code uniquifies, calls FromMappings and deuniquifies
                    List<XmlMembersMapping> uniqueMappings = new List<XmlMembersMapping>();
                    int[] uniqueIndexes = new int[Mappings.Count];
                    for (int srcIndex = 0; srcIndex < Mappings.Count; srcIndex++)
                    {
                        XmlMembersMapping mapping = Mappings[srcIndex];
                        int uniqueIndex = uniqueMappings.IndexOf(mapping);
                        if (uniqueIndex < 0)
                        {
                            uniqueMappings.Add(mapping);
                            uniqueIndex = uniqueMappings.Count - 1;
                        }
                        uniqueIndexes[srcIndex] = uniqueIndex;
                    }
                    XmlSerializer[] uniqueSerializers = CreateSerializersFromMappings(uniqueMappings.ToArray(), type);
                    if (uniqueMappings.Count == Mappings.Count)
                        return uniqueSerializers;
                    XmlSerializer[] serializers = new XmlSerializer[Mappings.Count];
                    for (int i = 0; i < Mappings.Count; i++)
                    {
                        serializers[i] = uniqueSerializers[uniqueIndexes[i]];
                    }
                    return serializers;
                }

                [Fx.Tag.SecurityNote(Critical = "XmlSerializer.FromMappings has a LinkDemand.",
                    Safe = "LinkDemand is spurious, not protecting anything in particular.")]
                [SecuritySafeCritical]
                XmlSerializer[] CreateSerializersFromMappings(XmlMapping[] mappings, Type type)
                {
                    return XmlSerializer.FromMappings(mappings, type);
                }
            }

            internal struct SerializerStub
            {
                readonly SerializerGenerationContext context;

                internal readonly XmlMembersMapping Mapping;
                internal readonly int Handle;

                internal SerializerStub(SerializerGenerationContext context, XmlMembersMapping mapping, int handle)
                {
                    this.context = context;
                    this.Mapping = mapping;
                    this.Handle = handle;
                }

                internal XmlSerializer GetSerializer()
                {
                    return context.GetSerializer(Handle);
                }
            }

            internal class XmlSerializerFaultContractInfo
            {
                FaultContractInfo faultContractInfo;
                SerializerStub serializerStub;
                XmlQualifiedName faultContractElementName;
                XmlSerializerObjectSerializer serializer;

                internal XmlSerializerFaultContractInfo(FaultContractInfo faultContractInfo, SerializerStub serializerStub,
                    XmlQualifiedName faultContractElementName)
                {
                    if (faultContractInfo == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("faultContractInfo");
                    }
                    if (faultContractElementName == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("faultContractElementName");
                    }
                    this.faultContractInfo = faultContractInfo;
                    this.serializerStub = serializerStub;
                    this.faultContractElementName = faultContractElementName;
                }

                internal FaultContractInfo FaultContractInfo
                {
                    get { return this.faultContractInfo; }
                }

                internal XmlQualifiedName FaultContractElementName
                {
                    get { return this.faultContractElementName; }
                }

                internal XmlSerializerObjectSerializer Serializer
                {
                    get
                    {
                        if (this.serializer == null)
                            this.serializer = new XmlSerializerObjectSerializer(faultContractInfo.Detail, this.faultContractElementName, this.serializerStub.GetSerializer());
                        return this.serializer;
                    }
                }
            }

            internal class MessageInfo : XmlSerializerOperationFormatter.MessageInfo
            {
                SerializerStub headers;
                SerializerStub body;
                OperationFormatter.MessageHeaderDescriptionTable headerDescriptionTable;
                MessageHeaderDescription unknownHeaderDescription;
                MessagePartDescriptionCollection rpcEncodedTypedMessageBodyParts;

                internal XmlMembersMapping BodyMapping
                {
                    get { return body.Mapping; }
                }

                internal override XmlSerializer BodySerializer
                {
                    get { return body.GetSerializer(); }
                }

                internal XmlMembersMapping HeadersMapping
                {
                    get { return headers.Mapping; }
                }

                internal override XmlSerializer HeaderSerializer
                {
                    get { return headers.GetSerializer(); }
                }

                internal override OperationFormatter.MessageHeaderDescriptionTable HeaderDescriptionTable
                {
                    get { return this.headerDescriptionTable; }
                }

                internal override MessageHeaderDescription UnknownHeaderDescription
                {
                    get { return this.unknownHeaderDescription; }
                }

                internal override MessagePartDescriptionCollection RpcEncodedTypedMessageBodyParts
                {
                    get { return rpcEncodedTypedMessageBodyParts; }
                }

                internal void SetBody(SerializerStub body, MessagePartDescriptionCollection rpcEncodedTypedMessageBodyParts)
                {
                    this.body = body;
                    this.rpcEncodedTypedMessageBodyParts = rpcEncodedTypedMessageBodyParts;
                }

                internal void SetHeaders(SerializerStub headers)
                {
                    this.headers = headers;
                }

                internal void SetHeaderDescriptionTable(OperationFormatter.MessageHeaderDescriptionTable headerDescriptionTable)
                {
                    this.headerDescriptionTable = headerDescriptionTable;
                }

                internal void SetUnknownHeaderDescription(MessageHeaderDescription unknownHeaderDescription)
                {
                    this.unknownHeaderDescription = unknownHeaderDescription;
                }

            }
        }
    }

    static class XmlSerializerHelper
    {
        static internal XmlReflectionMember GetXmlReflectionMember(MessagePartDescription part, bool isRpc, bool isEncoded, bool isWrapped)
        {
            string ns = isRpc ? null : part.Namespace;
            ICustomAttributeProvider additionalAttributesProvider = null;
            if (isEncoded || part.AdditionalAttributesProvider is MemberInfo)
                additionalAttributesProvider = part.AdditionalAttributesProvider;
            XmlName memberName = string.IsNullOrEmpty(part.UniquePartName) ? null : new XmlName(part.UniquePartName, true /*isEncoded*/);
            XmlName elementName = part.XmlName;
            return GetXmlReflectionMember(memberName, elementName, ns, part.Type, additionalAttributesProvider, part.Multiple, isEncoded, isWrapped);
        }

        static internal XmlReflectionMember GetXmlReflectionMember(XmlName memberName, XmlName elementName, string ns, Type type, ICustomAttributeProvider additionalAttributesProvider, bool isMultiple, bool isEncoded, bool isWrapped)
        {
            if (isEncoded && isMultiple)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxMultiplePartsNotAllowedInEncoded, elementName.DecodedName, ns)));

            XmlReflectionMember member = new XmlReflectionMember();
            member.MemberName = (memberName ?? elementName).DecodedName;
            member.MemberType = type;
            if (member.MemberType.IsByRef)
                member.MemberType = member.MemberType.GetElementType();
            if (isMultiple)
                member.MemberType = member.MemberType.MakeArrayType();
            if (additionalAttributesProvider != null)
            {
                if (isEncoded)
                    member.SoapAttributes = new SoapAttributes(additionalAttributesProvider);
                else
                    member.XmlAttributes = new XmlAttributes(additionalAttributesProvider);
            }
            if (isEncoded)
            {
                if (member.SoapAttributes == null)
                    member.SoapAttributes = new SoapAttributes();
                else
                {
                    Type invalidAttributeType = null;
                    if (member.SoapAttributes.SoapAttribute != null)
                        invalidAttributeType = typeof(SoapAttributeAttribute);
                    else if (member.SoapAttributes.SoapIgnore)
                        invalidAttributeType = typeof(SoapIgnoreAttribute);
                    else if (member.SoapAttributes.SoapType != null)
                        invalidAttributeType = typeof(SoapTypeAttribute);
                    if (invalidAttributeType != null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidSoapAttribute, invalidAttributeType, elementName.DecodedName)));
                }
                if (member.SoapAttributes.SoapElement == null)
                    member.SoapAttributes.SoapElement = new SoapElementAttribute(elementName.DecodedName);

            }
            else
            {
                if (member.XmlAttributes == null)
                    member.XmlAttributes = new XmlAttributes();
                else
                {
                    Type invalidAttributeType = null;
                    if (member.XmlAttributes.XmlAttribute != null)
                        invalidAttributeType = typeof(XmlAttributeAttribute);
                    else if (member.XmlAttributes.XmlAnyAttribute != null && !isWrapped)
                        invalidAttributeType = typeof(XmlAnyAttributeAttribute);
                    else if (member.XmlAttributes.XmlChoiceIdentifier != null)
                        invalidAttributeType = typeof(XmlChoiceIdentifierAttribute);
                    else if (member.XmlAttributes.XmlIgnore)
                        invalidAttributeType = typeof(XmlIgnoreAttribute);
                    else if (member.XmlAttributes.Xmlns)
                        invalidAttributeType = typeof(XmlNamespaceDeclarationsAttribute);
                    else if (member.XmlAttributes.XmlText != null)
                        invalidAttributeType = typeof(XmlTextAttribute);
                    else if (member.XmlAttributes.XmlEnum != null)
                        invalidAttributeType = typeof(XmlEnumAttribute);
                    if (invalidAttributeType != null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(isWrapped ? SR.SFxInvalidXmlAttributeInWrapped : SR.SFxInvalidXmlAttributeInBare, invalidAttributeType, elementName.DecodedName)));
                    if (member.XmlAttributes.XmlArray != null && isMultiple)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxXmlArrayNotAllowedForMultiple, elementName.DecodedName, ns)));

                }


                bool isArray = member.MemberType.IsArray;
                if ((isArray && !isMultiple && member.MemberType != typeof(byte[])) ||
                   (!isArray && typeof(IEnumerable).IsAssignableFrom(member.MemberType) && member.MemberType != typeof(string) && !typeof(XmlNode).IsAssignableFrom(member.MemberType) && !typeof(IXmlSerializable).IsAssignableFrom(member.MemberType)))
                {
                    if (member.XmlAttributes.XmlArray != null)
                    {
                        if (member.XmlAttributes.XmlArray.ElementName == String.Empty)
                            member.XmlAttributes.XmlArray.ElementName = elementName.DecodedName;
                        if (member.XmlAttributes.XmlArray.Namespace == null)
                            member.XmlAttributes.XmlArray.Namespace = ns;
                    }
                    else if (HasNoXmlParameterAttributes(member.XmlAttributes))
                    {
                        member.XmlAttributes.XmlArray = new XmlArrayAttribute();
                        member.XmlAttributes.XmlArray.ElementName = elementName.DecodedName;
                        member.XmlAttributes.XmlArray.Namespace = ns;
                    }
                }
                else
                {
                    if (member.XmlAttributes.XmlElements == null || member.XmlAttributes.XmlElements.Count == 0)
                    {
                        if (HasNoXmlParameterAttributes(member.XmlAttributes))
                        {
                            XmlElementAttribute elementAttribute = new XmlElementAttribute();
                            elementAttribute.ElementName = elementName.DecodedName;
                            elementAttribute.Namespace = ns;
                            member.XmlAttributes.XmlElements.Add(elementAttribute);
                        }
                    }
                    else
                    {
                        foreach (XmlElementAttribute elementAttribute in member.XmlAttributes.XmlElements)
                        {
                            if (elementAttribute.ElementName == String.Empty)
                                elementAttribute.ElementName = elementName.DecodedName;
                            if (elementAttribute.Namespace == null)
                                elementAttribute.Namespace = ns;
                        }
                    }
                }
            }
            return member;
        }

        static bool HasNoXmlParameterAttributes(XmlAttributes xmlAttributes)
        {
            return xmlAttributes.XmlAnyAttribute == null &&
                (xmlAttributes.XmlAnyElements == null || xmlAttributes.XmlAnyElements.Count == 0) &&
                xmlAttributes.XmlArray == null &&
                xmlAttributes.XmlAttribute == null &&
                !xmlAttributes.XmlIgnore &&
                xmlAttributes.XmlText == null &&
                xmlAttributes.XmlChoiceIdentifier == null &&
                (xmlAttributes.XmlElements == null || xmlAttributes.XmlElements.Count == 0) &&
                !xmlAttributes.Xmlns;
        }
    }
}
