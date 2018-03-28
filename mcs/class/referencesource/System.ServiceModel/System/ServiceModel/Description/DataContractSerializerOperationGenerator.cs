//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    class DataContractSerializerOperationGenerator : IOperationBehavior, IOperationContractGenerationExtension
    {
        Dictionary<OperationDescription, DataContractFormatAttribute> operationAttributes = new Dictionary<OperationDescription, DataContractFormatAttribute>();
        OperationGenerator operationGenerator;
        Dictionary<MessagePartDescription, ICollection<CodeTypeReference>> knownTypes;
        Dictionary<MessagePartDescription, bool> isNonNillableReferenceTypes;
        CodeCompileUnit codeCompileUnit;

        public DataContractSerializerOperationGenerator() : this(new CodeCompileUnit()) { }
        public DataContractSerializerOperationGenerator(CodeCompileUnit codeCompileUnit)
        {
            this.codeCompileUnit = codeCompileUnit;
            this.operationGenerator = new OperationGenerator();
        }

        internal void Add(MessagePartDescription part, CodeTypeReference typeReference, ICollection<CodeTypeReference> knownTypeReferences, bool isNonNillableReferenceType)
        {
            OperationGenerator.ParameterTypes.Add(part, typeReference);
            if (knownTypeReferences != null)
                KnownTypes.Add(part, knownTypeReferences);
            if (isNonNillableReferenceType)
            {
                if (isNonNillableReferenceTypes == null)
                    isNonNillableReferenceTypes = new Dictionary<MessagePartDescription, bool>();
                isNonNillableReferenceTypes.Add(part, isNonNillableReferenceType);
            }
        }

        internal OperationGenerator OperationGenerator
        {
            get { return this.operationGenerator; }
        }

        internal Dictionary<OperationDescription, DataContractFormatAttribute> OperationAttributes
        {
            get { return operationAttributes; }
        }

        internal Dictionary<MessagePartDescription, ICollection<CodeTypeReference>> KnownTypes
        {
            get
            {
                if (this.knownTypes == null)
                    this.knownTypes = new Dictionary<MessagePartDescription, ICollection<CodeTypeReference>>();
                return this.knownTypes;
            }
        }

        void IOperationBehavior.Validate(OperationDescription description)
        {
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch) { }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription description, ClientOperation proxy) { }

        void IOperationBehavior.AddBindingParameters(OperationDescription description, BindingParameterCollection parameters) { }

        // Assumption: gets called exactly once per operation
        void IOperationContractGenerationExtension.GenerateOperation(OperationContractGenerationContext context)
        {
            DataContractSerializerOperationBehavior DataContractSerializerOperationBehavior = context.Operation.Behaviors.Find<DataContractSerializerOperationBehavior>() as DataContractSerializerOperationBehavior;
            DataContractFormatAttribute dataContractFormatAttribute = (DataContractSerializerOperationBehavior == null) ? new DataContractFormatAttribute() : DataContractSerializerOperationBehavior.DataContractFormatAttribute;
            OperationFormatStyle style = dataContractFormatAttribute.Style;
            operationGenerator.GenerateOperation(context, ref style, false/*isEncoded*/, new WrappedBodyTypeGenerator(this, context), knownTypes);
            dataContractFormatAttribute.Style = style;
            if (dataContractFormatAttribute.Style != TypeLoader.DefaultDataContractFormatAttribute.Style)
                context.SyncMethod.CustomAttributes.Add(OperationGenerator.GenerateAttributeDeclaration(context.Contract.ServiceContractGenerator, dataContractFormatAttribute));
            if (knownTypes != null)
            {
                Dictionary<CodeTypeReference, object> operationKnownTypes = new Dictionary<CodeTypeReference, object>(new CodeTypeReferenceComparer());
                foreach (MessageDescription message in context.Operation.Messages)
                {
                    foreach (MessagePartDescription part in message.Body.Parts)
                        AddKnownTypesForPart(context, part, operationKnownTypes);
                    foreach (MessageHeaderDescription header in message.Headers)
                        AddKnownTypesForPart(context, header, operationKnownTypes);
                    if (OperationFormatter.IsValidReturnValue(message.Body.ReturnValue))
                        AddKnownTypesForPart(context, message.Body.ReturnValue, operationKnownTypes);
                }
            }
            UpdateTargetCompileUnit(context, this.codeCompileUnit);
        }

        void AddKnownTypesForPart(OperationContractGenerationContext context, MessagePartDescription part, Dictionary<CodeTypeReference, object> operationKnownTypes)
        {
            ICollection<CodeTypeReference> knownTypesForPart;
            if (knownTypes.TryGetValue(part, out knownTypesForPart))
            {
                foreach (CodeTypeReference knownTypeReference in knownTypesForPart)
                {
                    object value;
                    if (!operationKnownTypes.TryGetValue(knownTypeReference, out value))
                    {
                        operationKnownTypes.Add(knownTypeReference, null);
                        CodeAttributeDeclaration knownTypeAttribute = new CodeAttributeDeclaration(typeof(ServiceKnownTypeAttribute).FullName);
                        knownTypeAttribute.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(knownTypeReference)));
                        context.SyncMethod.CustomAttributes.Add(knownTypeAttribute);
                    }
                }
            }
        }

        internal static void UpdateTargetCompileUnit(OperationContractGenerationContext context, CodeCompileUnit codeCompileUnit)
        {
            CodeCompileUnit targetCompileUnit = context.ServiceContractGenerator.TargetCompileUnit;
            if (!Object.ReferenceEquals(targetCompileUnit, codeCompileUnit))
            {
                foreach (CodeNamespace codeNamespace in codeCompileUnit.Namespaces)
                    if (!targetCompileUnit.Namespaces.Contains(codeNamespace))
                        targetCompileUnit.Namespaces.Add(codeNamespace);
                foreach (string referencedAssembly in codeCompileUnit.ReferencedAssemblies)
                    if (!targetCompileUnit.ReferencedAssemblies.Contains(referencedAssembly))
                        targetCompileUnit.ReferencedAssemblies.Add(referencedAssembly);
                foreach (CodeAttributeDeclaration assemblyCustomAttribute in codeCompileUnit.AssemblyCustomAttributes)
                    if (!targetCompileUnit.AssemblyCustomAttributes.Contains(assemblyCustomAttribute))
                        targetCompileUnit.AssemblyCustomAttributes.Add(assemblyCustomAttribute);
                foreach (CodeDirective startDirective in codeCompileUnit.StartDirectives)
                    if (!targetCompileUnit.StartDirectives.Contains(startDirective))
                        targetCompileUnit.StartDirectives.Add(startDirective);
                foreach (CodeDirective endDirective in codeCompileUnit.EndDirectives)
                    if (!targetCompileUnit.EndDirectives.Contains(endDirective))
                        targetCompileUnit.EndDirectives.Add(endDirective);
                foreach (DictionaryEntry userData in codeCompileUnit.UserData)
                    targetCompileUnit.UserData[userData.Key] = userData.Value;
            }
        }

        internal class WrappedBodyTypeGenerator : IWrappedBodyTypeGenerator
        {
            static CodeTypeReference dataContractAttributeTypeRef = new CodeTypeReference(typeof(DataContractAttribute));
            int memberCount;
            OperationContractGenerationContext context;
            DataContractSerializerOperationGenerator dataContractSerializerOperationGenerator;
            public void ValidateForParameterMode(OperationDescription operation)
            {
                if (dataContractSerializerOperationGenerator.isNonNillableReferenceTypes == null)
                    return;
                foreach (MessageDescription messageDescription in operation.Messages)
                {
                    if (messageDescription.Body != null)
                    {
                        if (messageDescription.Body.ReturnValue != null)
                            ValidateForParameterMode(messageDescription.Body.ReturnValue);
                        foreach (MessagePartDescription bodyPart in messageDescription.Body.Parts)
                        {
                            ValidateForParameterMode(bodyPart);
                        }
                    }
                }
            }

            void ValidateForParameterMode(MessagePartDescription part)
            {
                if (dataContractSerializerOperationGenerator.isNonNillableReferenceTypes.ContainsKey(part))
                {
                    ParameterModeException parameterModeException = new ParameterModeException(SR.GetString(SR.SFxCannotImportAsParameters_ElementIsNotNillable, part.Name, part.Namespace));
                    parameterModeException.MessageContractType = MessageContractType.BareMessageContract;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(parameterModeException);
                }
            }

            public WrappedBodyTypeGenerator(DataContractSerializerOperationGenerator dataContractSerializerOperationGenerator, OperationContractGenerationContext context)
            {
                this.context = context;
                this.dataContractSerializerOperationGenerator = dataContractSerializerOperationGenerator;
            }

            public void AddMemberAttributes(XmlName messageName, MessagePartDescription part, CodeAttributeDeclarationCollection attributesImported, CodeAttributeDeclarationCollection typeAttributes, CodeAttributeDeclarationCollection fieldAttributes)
            {
                CodeAttributeDeclaration dataContractAttributeDecl = null;
                foreach (CodeAttributeDeclaration attr in typeAttributes)
                {
                    if (attr.AttributeType.BaseType == dataContractAttributeTypeRef.BaseType)
                    {
                        dataContractAttributeDecl = attr;
                        break;
                    }

                }

                if (dataContractAttributeDecl == null)
                {
                    Fx.Assert(String.Format(CultureInfo.InvariantCulture, "Cannot find DataContract attribute for  {0}", messageName));

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(String.Format(CultureInfo.InvariantCulture, "Cannot find DataContract attribute for  {0}", messageName)));
                }
                bool nsAttrFound = false;
                foreach (CodeAttributeArgument attrArg in dataContractAttributeDecl.Arguments)
                {
                    if (attrArg.Name == "Namespace")
                    {
                        nsAttrFound = true;
                        string nsValue = ((CodePrimitiveExpression)attrArg.Value).Value.ToString();
                        if (nsValue != part.Namespace)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxWrapperTypeHasMultipleNamespaces, messageName)));
                    }
                }
                if (!nsAttrFound)
                    dataContractAttributeDecl.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(part.Namespace)));

                DataMemberAttribute dataMemberAttribute = new DataMemberAttribute();
                dataMemberAttribute.Order = memberCount++;
                dataMemberAttribute.EmitDefaultValue = !IsNonNillableReferenceType(part);
                fieldAttributes.Add(OperationGenerator.GenerateAttributeDeclaration(context.Contract.ServiceContractGenerator, dataMemberAttribute));

            }

            private bool IsNonNillableReferenceType(MessagePartDescription part)
            {
                if (dataContractSerializerOperationGenerator.isNonNillableReferenceTypes == null)
                    return false;
                return dataContractSerializerOperationGenerator.isNonNillableReferenceTypes.ContainsKey(part);
            }

            public void AddTypeAttributes(string messageName, string typeNS, CodeAttributeDeclarationCollection typeAttributes, bool isEncoded)
            {
                typeAttributes.Add(OperationGenerator.GenerateAttributeDeclaration(context.Contract.ServiceContractGenerator, new DataContractAttribute()));
                memberCount = 0;
            }
        }

        class CodeTypeReferenceComparer : IEqualityComparer<CodeTypeReference>
        {
            public bool Equals(CodeTypeReference x, CodeTypeReference y)
            {
                if (Object.ReferenceEquals(x, y))
                    return true;

                if (x == null || y == null || x.ArrayRank != y.ArrayRank || x.BaseType != y.BaseType)
                    return false;

                CodeTypeReferenceCollection xTypeArgs = x.TypeArguments;
                CodeTypeReferenceCollection yTypeArgs = y.TypeArguments;
                if (yTypeArgs.Count == xTypeArgs.Count)
                {
                    foreach (CodeTypeReference xTypeArg in xTypeArgs)
                    {
                        foreach (CodeTypeReference yTypeArg in yTypeArgs)
                        {
                            if (!this.Equals(xTypeArg, xTypeArg))
                                return false;
                        }
                    }
                }
                return true;
            }

            public int GetHashCode(CodeTypeReference obj)
            {
                return obj.GetHashCode();
            }
        }

    }
}
