//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.CodeDom;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace System.ServiceModel.Description
{
    class XmlSerializerOperationGenerator : IOperationBehavior, IOperationContractGenerationExtension
    {
        OperationGenerator operationGenerator;
        Dictionary<MessagePartDescription, PartInfo> partInfoTable;
        Dictionary<OperationDescription, XmlSerializerFormatAttribute> operationAttributes = new Dictionary<OperationDescription, XmlSerializerFormatAttribute>();
        XmlCodeExporter xmlExporter;
        SoapCodeExporter soapExporter;

        XmlSerializerImportOptions options;
        CodeNamespace codeNamespace;

        internal XmlSerializerOperationGenerator(XmlSerializerImportOptions options)
        {
            operationGenerator = new OperationGenerator();
            this.options = options;
            this.codeNamespace = GetTargetCodeNamespace(options);
            partInfoTable = new Dictionary<MessagePartDescription, PartInfo>();
        }

        static CodeNamespace GetTargetCodeNamespace(XmlSerializerImportOptions options)
        {
            CodeNamespace targetCodeNamespace = null;
            string clrNamespace = options.ClrNamespace ?? string.Empty;
            foreach (CodeNamespace ns in options.CodeCompileUnit.Namespaces)
            {
                if (ns.Name == clrNamespace)
                {
                    targetCodeNamespace = ns;
                }
            }
            if (targetCodeNamespace == null)
            {
                targetCodeNamespace = new CodeNamespace(clrNamespace);
                options.CodeCompileUnit.Namespaces.Add(targetCodeNamespace);
            }
            return targetCodeNamespace;
        }

        internal void Add(MessagePartDescription part, XmlMemberMapping memberMapping, XmlMembersMapping membersMapping, bool isEncoded)
        {
            PartInfo partInfo = new PartInfo();
            partInfo.MemberMapping = memberMapping;
            partInfo.MembersMapping = membersMapping;
            partInfo.IsEncoded = isEncoded;
            partInfoTable[part] = partInfo;
        }

        public XmlCodeExporter XmlExporter
        {
            get
            {
                if (this.xmlExporter == null)
                {
                    this.xmlExporter = new XmlCodeExporter(this.codeNamespace, this.options.CodeCompileUnit, this.options.CodeProvider,
                        this.options.WebReferenceOptions.CodeGenerationOptions, null);
                }
                return xmlExporter;
            }
        }

        public SoapCodeExporter SoapExporter
        {
            get
            {
                if (this.soapExporter == null)
                {
                    this.soapExporter = new SoapCodeExporter(this.codeNamespace, this.options.CodeCompileUnit, this.options.CodeProvider,
                        this.options.WebReferenceOptions.CodeGenerationOptions, null);
                }
                return soapExporter;
            }
        }

        OperationGenerator OperationGenerator
        {
            get { return this.operationGenerator; }
        }

        internal Dictionary<OperationDescription, XmlSerializerFormatAttribute> OperationAttributes
        {
            get { return operationAttributes; }
        }


        void IOperationBehavior.Validate(OperationDescription description)
        {
        }

        void IOperationBehavior.AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
        {
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch) { }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription description, ClientOperation proxy) { }

        static object contractMarker = new object();
        // Assumption: gets called exactly once per operation
        void IOperationContractGenerationExtension.GenerateOperation(OperationContractGenerationContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            if (partInfoTable != null && partInfoTable.Count > 0)
            {
                Dictionary<XmlMembersMapping, XmlMembersMapping> alreadyExported = new Dictionary<XmlMembersMapping, XmlMembersMapping>();
                foreach (MessageDescription message in context.Operation.Messages)
                {
                    foreach (MessageHeaderDescription header in message.Headers)
                        GeneratePartType(alreadyExported, header, header.Namespace);


                    MessageBodyDescription body = message.Body;
                    bool isWrapped = (body.WrapperName != null);
                    if (OperationFormatter.IsValidReturnValue(body.ReturnValue))
                        GeneratePartType(alreadyExported, body.ReturnValue, isWrapped ? body.WrapperNamespace : body.ReturnValue.Namespace);

                    foreach (MessagePartDescription part in body.Parts)
                        GeneratePartType(alreadyExported, part, isWrapped ? body.WrapperNamespace : part.Namespace);
                }
            }
            XmlSerializerOperationBehavior xmlSerializerOperationBehavior = context.Operation.Behaviors.Find<XmlSerializerOperationBehavior>() as XmlSerializerOperationBehavior;
            if (xmlSerializerOperationBehavior == null)
                return;

            XmlSerializerFormatAttribute xmlSerializerFormatAttribute = (xmlSerializerOperationBehavior == null) ? new XmlSerializerFormatAttribute() : xmlSerializerOperationBehavior.XmlSerializerFormatAttribute;
            OperationFormatStyle style = xmlSerializerFormatAttribute.Style;
            operationGenerator.GenerateOperation(context, ref style, xmlSerializerFormatAttribute.IsEncoded, new WrappedBodyTypeGenerator(context), new Dictionary<MessagePartDescription, ICollection<CodeTypeReference>>());
            context.ServiceContractGenerator.AddReferencedAssembly(typeof(System.Xml.Serialization.XmlTypeAttribute).Assembly);
            xmlSerializerFormatAttribute.Style = style;
            context.SyncMethod.CustomAttributes.Add(OperationGenerator.GenerateAttributeDeclaration(context.Contract.ServiceContractGenerator, xmlSerializerFormatAttribute));
            AddKnownTypes(context.SyncMethod.CustomAttributes, xmlSerializerFormatAttribute.IsEncoded ? SoapExporter.IncludeMetadata : XmlExporter.IncludeMetadata);
            DataContractSerializerOperationGenerator.UpdateTargetCompileUnit(context, this.options.CodeCompileUnit);
        }

        private void AddKnownTypes(CodeAttributeDeclarationCollection destination, CodeAttributeDeclarationCollection source)
        {
            foreach (CodeAttributeDeclaration attribute in source)
            {
                CodeAttributeDeclaration knownType = ToKnownType(attribute);
                if (knownType != null)
                {
                    destination.Add(knownType);
                }
            }
        }

        // Convert [XmlInclude] or [SoapInclude] attribute to [KnownType] attribute
        private CodeAttributeDeclaration ToKnownType(CodeAttributeDeclaration include)
        {
            if (include.Name == typeof(SoapIncludeAttribute).FullName || include.Name == typeof(XmlIncludeAttribute).FullName)
            {
                CodeAttributeDeclaration knownType = new CodeAttributeDeclaration(new CodeTypeReference(typeof(ServiceKnownTypeAttribute)));
                foreach (CodeAttributeArgument argument in include.Arguments)
                {
                    knownType.Arguments.Add(argument);
                }
                return knownType;
            }
            return null;
        }

        private void GeneratePartType(Dictionary<XmlMembersMapping, XmlMembersMapping> alreadyExported, MessagePartDescription part, string partNamespace)
        {
            if (!partInfoTable.ContainsKey(part))
                return;
            PartInfo partInfo = partInfoTable[part];
            XmlMembersMapping membersMapping = partInfo.MembersMapping;
            XmlMemberMapping memberMapping = partInfo.MemberMapping;
            if (!alreadyExported.ContainsKey(membersMapping))
            {
                if (partInfo.IsEncoded)
                    SoapExporter.ExportMembersMapping(membersMapping);
                else
                    XmlExporter.ExportMembersMapping(membersMapping);
                alreadyExported.Add(membersMapping, membersMapping);
            }
            CodeAttributeDeclarationCollection additionalAttributes = new CodeAttributeDeclarationCollection();
            if (partInfo.IsEncoded)
                SoapExporter.AddMappingMetadata(additionalAttributes, memberMapping, false/*forceUseMemberName*/);
            else
                XmlExporter.AddMappingMetadata(additionalAttributes, memberMapping, partNamespace, false/*forceUseMemberName*/);
            part.BaseType = GetTypeName(memberMapping);
            operationGenerator.ParameterTypes.Add(part, new CodeTypeReference(part.BaseType));
            operationGenerator.ParameterAttributes.Add(part, additionalAttributes);
        }

        internal string GetTypeName(XmlMemberMapping member)
        {
            string typeName = member.GenerateTypeName(options.CodeProvider);
            // If it is an array type, get the array element type name instead
            string comparableTypeName = typeName.Replace("[]", null);
            if (codeNamespace != null && !string.IsNullOrEmpty(codeNamespace.Name))
            {
                foreach (CodeTypeDeclaration typeDecl in codeNamespace.Types)
                {
                    if (typeDecl.Name == comparableTypeName)
                    {
                        typeName = codeNamespace.Name + "." + typeName;
                    }
                }
            }
            return typeName;
        }

        class PartInfo
        {
            internal XmlMemberMapping MemberMapping;
            internal XmlMembersMapping MembersMapping;
            internal bool IsEncoded;
        }

        internal class WrappedBodyTypeGenerator : IWrappedBodyTypeGenerator
        {
            OperationContractGenerationContext context;
            public WrappedBodyTypeGenerator(OperationContractGenerationContext context)
            {
                this.context = context;
            }
            public void ValidateForParameterMode(OperationDescription operation)
            {
            }

            public void AddMemberAttributes(XmlName messageName, MessagePartDescription part, CodeAttributeDeclarationCollection importedAttributes, CodeAttributeDeclarationCollection typeAttributes, CodeAttributeDeclarationCollection fieldAttributes)
            {
                if (importedAttributes != null)
                    fieldAttributes.AddRange(importedAttributes);
            }
            public void AddTypeAttributes(string messageName, string typeNS, CodeAttributeDeclarationCollection typeAttributes, bool isEncoded)
            {
                // we do not need top-level attibutes for the encoded SOAP
                if (isEncoded)
                    return;
                XmlTypeAttribute xmlType = new XmlTypeAttribute();
                xmlType.Namespace = typeNS;
                typeAttributes.Add(OperationGenerator.GenerateAttributeDeclaration(context.Contract.ServiceContractGenerator, xmlType));
            }
        }
    }
}
