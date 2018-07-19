//------------------------------------------------------------------------------
// <copyright file="SoapProtocolReflector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Collections;
    using System;
    using System.Reflection;
    using System.Web.Services.Configuration;

    internal class SoapProtocolReflector : ProtocolReflector {
        // SoapProtocolInfo protocolInfo;
        ArrayList mappings = new ArrayList();
        SoapExtensionReflector[] extensions;
        SoapReflectedMethod soapMethod;

        internal override WsiProfiles ConformsTo {
            get { return WsiProfiles.BasicProfile1_1; }
        }

        public override string ProtocolName { 
            get { return "Soap"; } 
        }

        internal SoapReflectedMethod SoapMethod {
            get { return soapMethod; }
        }

        internal SoapReflectionImporter SoapImporter {
            get { 
                SoapReflectionImporter soapImporter = ReflectionContext[typeof(SoapReflectionImporter)] as SoapReflectionImporter;
                if (soapImporter == null) {
                    soapImporter = SoapReflector.CreateSoapImporter(DefaultNamespace, SoapReflector.ServiceDefaultIsEncoded(ServiceType));
                    ReflectionContext[typeof(SoapReflectionImporter)] = soapImporter;
                }
                return soapImporter;
            }
        }

        internal SoapSchemaExporter SoapExporter {
            get { 
                SoapSchemaExporter soapExporter = ReflectionContext[typeof(SoapSchemaExporter)] as SoapSchemaExporter;
                if (soapExporter == null) {
                    soapExporter = new SoapSchemaExporter(ServiceDescription.Types.Schemas);
                    ReflectionContext[typeof(SoapSchemaExporter)] = soapExporter;
                }
                return soapExporter;
            }
        }

        protected override bool ReflectMethod() {
            soapMethod = ReflectionContext[Method] as SoapReflectedMethod;
            if (soapMethod == null) {
                soapMethod = SoapReflector.ReflectMethod(Method, false, ReflectionImporter, SoapImporter, DefaultNamespace);
                ReflectionContext[Method] = soapMethod;
                soapMethod.portType = Binding != null ? Binding.Type : null;
            }
            WebMethodAttribute methodAttr = Method.MethodAttribute;

            OperationBinding.Extensions.Add(CreateSoapOperationBinding(soapMethod.rpc ? SoapBindingStyle.Rpc : SoapBindingStyle.Document, soapMethod.action));

            CreateMessage(soapMethod.rpc, soapMethod.use, soapMethod.paramStyle, InputMessage, OperationBinding.Input, soapMethod.requestMappings);
            if (!soapMethod.oneWay)
                CreateMessage(soapMethod.rpc, soapMethod.use, soapMethod.paramStyle, OutputMessage, OperationBinding.Output, soapMethod.responseMappings);
          
            CreateHeaderMessages(soapMethod.name, soapMethod.use, soapMethod.inHeaderMappings, soapMethod.outHeaderMappings, soapMethod.headers, soapMethod.rpc);

            if (soapMethod.rpc && soapMethod.use == SoapBindingUse.Encoded && soapMethod.methodInfo.OutParameters.Length > 0)
                Operation.ParameterOrder = GetParameterOrder(soapMethod.methodInfo);

            AllowExtensionsToReflectMethod();

            return true;
        }

        protected override void ReflectDescription() {
            AllowExtensionsToReflectDescription();
        }

        void CreateHeaderMessages(string methodName, SoapBindingUse use, XmlMembersMapping inHeaderMappings, XmlMembersMapping outHeaderMappings, SoapReflectedHeader[] headers, bool rpc) {
            // 
            if (use == SoapBindingUse.Encoded) {
                SoapExporter.ExportMembersMapping(inHeaderMappings, false);
                if (outHeaderMappings != null)
                    SoapExporter.ExportMembersMapping(outHeaderMappings, false);
            }
            else {
                SchemaExporter.ExportMembersMapping(inHeaderMappings);
                if (outHeaderMappings != null)
                    SchemaExporter.ExportMembersMapping(outHeaderMappings);
            }

            CodeIdentifiers identifiers = new CodeIdentifiers();
            int inCount = 0, outCount = 0;
            for (int i = 0; i < headers.Length; i++) {
                SoapReflectedHeader soapHeader = headers[i];
                if (!soapHeader.custom) continue;
                
                XmlMemberMapping member;
                if ((soapHeader.direction & SoapHeaderDirection.In) != 0) {
                    member = inHeaderMappings[inCount++];
                    if (soapHeader.direction != SoapHeaderDirection.In)
                        outCount++;
                }
                else {
                    member = outHeaderMappings[outCount++];
                }

                MessagePart part = new MessagePart();
                part.Name = member.XsdElementName;
                if (use == SoapBindingUse.Encoded)
                    part.Type = new XmlQualifiedName(member.TypeName, member.TypeNamespace);
                else
                    part.Element = new XmlQualifiedName(member.XsdElementName, member.Namespace);
                Message message = new Message();
                message.Name = identifiers.AddUnique(methodName + part.Name, message);
                message.Parts.Add(part);
                HeaderMessages.Add(message);

                ServiceDescriptionFormatExtension soapHeaderBinding = CreateSoapHeaderBinding(new XmlQualifiedName(message.Name, Binding.ServiceDescription.TargetNamespace), part.Name, rpc ? member.Namespace : null, use);
                
                if ((soapHeader.direction & SoapHeaderDirection.In) != 0)
                    OperationBinding.Input.Extensions.Add(soapHeaderBinding);
                if ((soapHeader.direction & SoapHeaderDirection.Out) != 0)
                    OperationBinding.Output.Extensions.Add(soapHeaderBinding);

                if ((soapHeader.direction & SoapHeaderDirection.Fault) != 0) {
                    if (soapMethod.IsClaimsConformance) {
                        throw new InvalidOperationException(Res.GetString(Res.BPConformanceHeaderFault, soapMethod.methodInfo.ToString(), soapMethod.methodInfo.DeclaringType.FullName, "Direction", typeof(SoapHeaderDirection).Name, SoapHeaderDirection.Fault.ToString())); 
                    }
                    OperationBinding.Output.Extensions.Add(soapHeaderBinding);
                }
            }
        }

        void CreateMessage(bool rpc, SoapBindingUse use, SoapParameterStyle paramStyle, Message message, MessageBinding messageBinding, XmlMembersMapping members) {
            bool wrapped = paramStyle != SoapParameterStyle.Bare;

            if (use == SoapBindingUse.Encoded)
                CreateEncodedMessage(message, messageBinding, members, wrapped && !rpc);
            else
                CreateLiteralMessage(message, messageBinding, members, wrapped && !rpc, rpc);
        }

        void CreateEncodedMessage(Message message, MessageBinding messageBinding, XmlMembersMapping members, bool wrapped) {
            SoapExporter.ExportMembersMapping(members, wrapped);

            if (wrapped) {
                MessagePart part = new MessagePart();
                part.Name = "parameters";
                part.Type = new XmlQualifiedName(members.TypeName, members.TypeNamespace);
                message.Parts.Add(part);
            }
            else {
                for (int i = 0; i < members.Count; i++) {
                    XmlMemberMapping member = members[i];
                    MessagePart part = new MessagePart();
                    part.Name = member.XsdElementName;
                    part.Type = new XmlQualifiedName(member.TypeName, member.TypeNamespace);
                    message.Parts.Add(part);
                }
            }

            messageBinding.Extensions.Add(CreateSoapBodyBinding(SoapBindingUse.Encoded, members.Namespace));
        }

        void CreateLiteralMessage(Message message, MessageBinding messageBinding, XmlMembersMapping members, bool wrapped, bool rpc) {
            if (members.Count == 1 && members[0].Any && members[0].ElementName.Length == 0 && !wrapped) {
                string typeName = SchemaExporter.ExportAnyType(members[0].Namespace);
                MessagePart part = new MessagePart();
                part.Name = members[0].MemberName;
                part.Type = new XmlQualifiedName(typeName, members[0].Namespace);
                message.Parts.Add(part);
            }
            else {
                SchemaExporter.ExportMembersMapping(members, !rpc);
                if (wrapped) {
                    MessagePart part = new MessagePart();
                    part.Name = "parameters";
                    part.Element = new XmlQualifiedName(members.XsdElementName, members.Namespace);
                    message.Parts.Add(part);
                }
                else {
                    for (int i = 0; i < members.Count; i++) {
                        XmlMemberMapping member = members[i];
                        MessagePart part = new MessagePart();
                        if (rpc) {
                            // Generate massage part with the type attribute
                            if (member.TypeName == null || member.TypeName.Length == 0) {
                                throw new InvalidOperationException(Res.GetString(Res.WsdlGenRpcLitAnonimousType, Method.DeclaringType.Name, Method.Name, member.MemberName));
                            }
                            part.Name = member.XsdElementName;
                            part.Type = new XmlQualifiedName(member.TypeName, member.TypeNamespace);
                        }
                        else {
                            part.Name = XmlConvert.EncodeLocalName(member.MemberName);
                            part.Element = new XmlQualifiedName(member.XsdElementName, member.Namespace);
                        }
                        message.Parts.Add(part);
                    }
                }
            }
            messageBinding.Extensions.Add(CreateSoapBodyBinding(SoapBindingUse.Literal, rpc ? members.Namespace : null));
        }

        static string[] GetParameterOrder(LogicalMethodInfo methodInfo) {
            ParameterInfo[] parameters = methodInfo.Parameters;
            string[] parameterOrder = new string[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) {
                parameterOrder[i] = parameters[i].Name;
            }
            return parameterOrder;
        }

        protected override string ReflectMethodBinding() { 
            return SoapReflector.GetSoapMethodBinding(Method);
        }

        protected override void BeginClass() {
            if (Binding != null) {
                SoapBindingStyle style;
                if (SoapReflector.GetSoapServiceAttribute(ServiceType) is SoapRpcServiceAttribute)
                    style = SoapBindingStyle.Rpc;
                else
                    style = SoapBindingStyle.Document;
                Binding.Extensions.Add(CreateSoapBinding(style));
                SoapReflector.IncludeTypes(Methods, SoapImporter);
            }

            Port.Extensions.Add(CreateSoapAddressBinding(ServiceUrl));
        }

        void AllowExtensionsToReflectMethod() {
            if (extensions == null) {
                TypeElementCollection extensionTypes = WebServicesSection.Current.SoapExtensionReflectorTypes;
                extensions = new SoapExtensionReflector[extensionTypes.Count];
                for (int i = 0; i < extensions.Length; i++) {
                    SoapExtensionReflector extension = (SoapExtensionReflector)Activator.CreateInstance(extensionTypes[i].Type);
                    extension.ReflectionContext = this;
                    extensions[i] = extension;
                }
            }
            foreach (SoapExtensionReflector extension in extensions) {
                extension.ReflectMethod();
            }
        }

        void AllowExtensionsToReflectDescription() {
            if (extensions == null) {
                TypeElementCollection extensionTypes = WebServicesSection.Current.SoapExtensionReflectorTypes;
                extensions = new SoapExtensionReflector[extensionTypes.Count];
                for (int i = 0; i < extensions.Length; i++) {
                    SoapExtensionReflector extension = (SoapExtensionReflector)Activator.CreateInstance(extensionTypes[i].Type);
                    extension.ReflectionContext = this;
                    extensions[i] = extension;
                }
            }
            foreach (SoapExtensionReflector extension in extensions) {
                extension.ReflectDescription();
            }
        }

        protected virtual SoapBinding CreateSoapBinding(SoapBindingStyle style) {
            SoapBinding soapBinding = new SoapBinding();
            soapBinding.Transport = SoapBinding.HttpTransport;
            soapBinding.Style = style;
            return soapBinding;
        }

        protected virtual SoapAddressBinding CreateSoapAddressBinding(string serviceUrl) {
            SoapAddressBinding soapAddress = new SoapAddressBinding();
            soapAddress.Location = serviceUrl;
            if (this.UriFixups != null)
            {
                this.UriFixups.Add(delegate(Uri current)
                {
                    soapAddress.Location = DiscoveryServerType.CombineUris(current, soapAddress.Location);
                });
            }
            return soapAddress;
        }

        protected virtual SoapOperationBinding CreateSoapOperationBinding(SoapBindingStyle style, string action) {
            SoapOperationBinding soapOperation = new SoapOperationBinding();
            soapOperation.SoapAction = action;
            soapOperation.Style = style;
            return soapOperation;
        }

        protected virtual SoapBodyBinding CreateSoapBodyBinding(SoapBindingUse use, string ns) {
            SoapBodyBinding soapBodyBinding = new SoapBodyBinding();
            soapBodyBinding.Use = use;
            if (use == SoapBindingUse.Encoded)
                soapBodyBinding.Encoding = Soap.Encoding;
            soapBodyBinding.Namespace = ns;
            return soapBodyBinding;
        }

        protected virtual SoapHeaderBinding CreateSoapHeaderBinding(XmlQualifiedName message, string partName, SoapBindingUse use) {
            return CreateSoapHeaderBinding(message, partName, null, use);
        }

        protected virtual SoapHeaderBinding CreateSoapHeaderBinding(XmlQualifiedName message, string partName, string ns, SoapBindingUse use) {
            SoapHeaderBinding soapHeaderBinding = new SoapHeaderBinding();
            soapHeaderBinding.Message = message;
            soapHeaderBinding.Part = partName;
            soapHeaderBinding.Use = use;
            if (use == SoapBindingUse.Encoded) {
                soapHeaderBinding.Encoding = Soap.Encoding;
                soapHeaderBinding.Namespace = ns;
            }
            return soapHeaderBinding;
        }
    }
}
