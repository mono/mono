//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Net.Security;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ComponentModel;
    using System.Threading.Tasks;

    enum MessageContractType { None, WrappedMessageContract, BareMessageContract }
    interface IWrappedBodyTypeGenerator
    {
        void ValidateForParameterMode(OperationDescription operationDescription);
        void AddMemberAttributes(XmlName messageName, MessagePartDescription part, CodeAttributeDeclarationCollection attributesImported, CodeAttributeDeclarationCollection typeAttributes, CodeAttributeDeclarationCollection fieldAttributes);
        void AddTypeAttributes(string messageName, string typeNS, CodeAttributeDeclarationCollection typeAttributes, bool isEncoded);
    }

    class OperationGenerator //: IOperationBehavior, IOperationContractGenerationExtension
    {
        Dictionary<MessagePartDescription, CodeTypeReference> parameterTypes;
        Dictionary<MessagePartDescription, CodeAttributeDeclarationCollection> parameterAttributes;
        Dictionary<MessagePartDescription, string> specialPartName;

        internal OperationGenerator()
        {
        }

        internal Dictionary<MessagePartDescription, CodeAttributeDeclarationCollection> ParameterAttributes
        {
            get
            {
                if (this.parameterAttributes == null)
                    this.parameterAttributes = new Dictionary<MessagePartDescription, CodeAttributeDeclarationCollection>();
                return this.parameterAttributes;
            }
        }

        internal Dictionary<MessagePartDescription, CodeTypeReference> ParameterTypes
        {
            get
            {
                if (this.parameterTypes == null)
                    this.parameterTypes = new Dictionary<MessagePartDescription, CodeTypeReference>();
                return this.parameterTypes;
            }
        }

        internal Dictionary<MessagePartDescription, string> SpecialPartName
        {
            get
            {
                if (specialPartName == null)
                    this.specialPartName = new Dictionary<MessagePartDescription, string>();
                return specialPartName;
            }
        }

        internal void GenerateOperation(OperationContractGenerationContext context, ref OperationFormatStyle style, bool isEncoded, IWrappedBodyTypeGenerator wrappedBodyTypeGenerator, Dictionary<MessagePartDescription, ICollection<CodeTypeReference>> knownTypes)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            if (context.Operation == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.OperationPropertyIsRequiredForAttributeGeneration)));

            MethodSignatureGenerator methodSignatureGenerator = new MethodSignatureGenerator(this, context, style, isEncoded, wrappedBodyTypeGenerator, knownTypes);
            methodSignatureGenerator.GenerateSyncSignature(ref style);

            if (context.IsTask)
            {
                methodSignatureGenerator.GenerateTaskSignature(ref style);
            }

            if (context.IsAsync)
            {
                methodSignatureGenerator.GenerateAsyncSignature(ref style);
            }
        }

        internal static CodeAttributeDeclaration GenerateAttributeDeclaration(ServiceContractGenerator generator, Attribute attribute)
        {
            return CustomAttributeHelper.GenerateAttributeDeclaration(generator, attribute);
        }

        class MethodSignatureGenerator
        {
            readonly OperationGenerator Parent;
            readonly OperationContractGenerationContext Context;
            readonly OperationFormatStyle Style;
            readonly bool IsEncoded;
            readonly IWrappedBodyTypeGenerator WrappedBodyTypeGenerator;
            readonly Dictionary<MessagePartDescription, ICollection<CodeTypeReference>> KnownTypes;

            CodeMemberMethod Method;
            CodeMemberMethod EndMethod;

            readonly string ContractName;
            string DefaultName;
            readonly string ContractNS;
            readonly string DefaultNS;

            readonly bool Oneway;

            readonly MessageDescription Request;
            readonly MessageDescription Response;

            bool IsNewRequest;
            bool IsNewResponse;
            bool IsTaskWithOutputParameters;
            MessageContractType MessageContractType;

            IPartCodeGenerator BeginPartCodeGenerator;
            IPartCodeGenerator EndPartCodeGenerator;

            internal MethodSignatureGenerator(OperationGenerator parent, OperationContractGenerationContext context, OperationFormatStyle style, bool isEncoded, IWrappedBodyTypeGenerator wrappedBodyTypeGenerator, Dictionary<MessagePartDescription, ICollection<CodeTypeReference>> knownTypes)
            {
                this.Parent = parent;
                this.Context = context;
                this.Style = style;
                this.IsEncoded = isEncoded;
                this.WrappedBodyTypeGenerator = wrappedBodyTypeGenerator;
                this.KnownTypes = knownTypes;
                this.MessageContractType = context.ServiceContractGenerator.OptionsInternal.IsSet(ServiceContractGenerationOptions.TypedMessages) ? MessageContractType.WrappedMessageContract : MessageContractType.None;

                this.ContractName = context.Contract.Contract.CodeName;
                this.ContractNS = context.Operation.DeclaringContract.Namespace;
                this.DefaultNS = (style == OperationFormatStyle.Rpc) ? string.Empty : this.ContractNS;
                this.Oneway = (context.Operation.IsOneWay);
                this.Request = context.Operation.Messages[0];
                this.Response = this.Oneway ? null : context.Operation.Messages[1];

                this.IsNewRequest = true;
                this.IsNewResponse = true;
                this.BeginPartCodeGenerator = null;
                this.EndPartCodeGenerator = null;
                this.IsTaskWithOutputParameters = context.IsTask && context.Operation.HasOutputParameters;

                Fx.Assert(this.Oneway == (this.Response == null), "OperationContractGenerationContext.Operation cannot contain a null response message when the operation is not one-way");
            }

            internal void GenerateSyncSignature(ref OperationFormatStyle style)
            {
                this.Method = this.Context.SyncMethod;
                this.EndMethod = this.Context.SyncMethod;
                this.DefaultName = this.Method.Name;
                GenerateOperationSignatures(ref style);
            }

            internal void GenerateAsyncSignature(ref OperationFormatStyle style)
            {
                this.Method = this.Context.BeginMethod;
                this.EndMethod = this.Context.EndMethod;
                this.DefaultName = this.Method.Name.Substring(5);
                GenerateOperationSignatures(ref style);
            }

            void GenerateOperationSignatures(ref OperationFormatStyle style)
            {
                if (this.MessageContractType != MessageContractType.None || this.GenerateTypedMessageForTaskWithOutputParameters())
                {
                    CheckAndSetMessageContractTypeToBare();
                    this.GenerateTypedMessageOperation(false /*hideFromEditor*/, ref style);
                }
                else if (!this.TryGenerateParameterizedOperation())
                {
                    this.GenerateTypedMessageOperation(true /*hideFromEditor*/, ref style);
                }
            }

            bool GenerateTypedMessageForTaskWithOutputParameters()
            {
                if (this.IsTaskWithOutputParameters)
                {
                    if (this.Method == this.Context.TaskMethod)
                    {
                        this.Method.Comments.Add(new CodeCommentStatement(SR.GetString(SR.SFxCodeGenWarning, SR.GetString(SR.SFxCannotImportAsParameters_OutputParameterAndTask))));
                    }
                    
                    return true;
                }

                return false;
            }

            void CheckAndSetMessageContractTypeToBare()
            {
                if (this.MessageContractType == MessageContractType.BareMessageContract)
                    return;
                try
                {
                    this.WrappedBodyTypeGenerator.ValidateForParameterMode(this.Context.Operation);
                }
                catch (ParameterModeException ex)
                {
                    this.MessageContractType = ex.MessageContractType;
                }
            }

            bool TryGenerateParameterizedOperation()
            {
                CodeParameterDeclarationExpressionCollection methodParameters, endMethodParameters = null;
                methodParameters = new CodeParameterDeclarationExpressionCollection(this.Method.Parameters);
                if (this.EndMethod != null)
                    endMethodParameters = new CodeParameterDeclarationExpressionCollection(this.EndMethod.Parameters);

                try
                {
                    GenerateParameterizedOperation();
                }
                catch (ParameterModeException ex)
                {
                    this.MessageContractType = ex.MessageContractType;
                    CodeMemberMethod method = this.Method;
                    method.Comments.Add(new CodeCommentStatement(SR.GetString(SR.SFxCodeGenWarning, ex.Message)));
                    method.Parameters.Clear();
                    method.Parameters.AddRange(methodParameters);
                    if (this.Context.IsAsync)
                    {
                        CodeMemberMethod endMethod = this.EndMethod;
                        endMethod.Parameters.Clear();
                        endMethod.Parameters.AddRange(endMethodParameters);
                    }
                    return false;
                }
                return true;
            }

            void GenerateParameterizedOperation()
            {

                ParameterizedMessageHelper.ValidateProtectionLevel(this);
                CreateOrOverrideActionProperties();

                if (this.HasUntypedMessages)
                {
                    if (!this.IsCompletelyUntyped)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(SR.GetString(SR.SFxCannotImportAsParameters_Message, this.Context.Operation.CodeName)));

                    CreateUntypedMessages();
                }
                else
                {

                    ParameterizedMessageHelper.ValidateWrapperSettings(this);
                    ParameterizedMessageHelper.ValidateNoHeaders(this);
                    this.WrappedBodyTypeGenerator.ValidateForParameterMode(this.Context.Operation);

                    ParameterizedMethodGenerator generator = new ParameterizedMethodGenerator(this.Method, this.EndMethod);
                    this.BeginPartCodeGenerator = generator.InputGenerator;
                    this.EndPartCodeGenerator = generator.OutputGenerator;

                    if (!this.Oneway && this.Response.Body.ReturnValue != null)
                    {
                        this.EndMethod.ReturnType = GetParameterType(this.Response.Body.ReturnValue);
                        ParameterizedMessageHelper.GenerateMessageParameterAttribute(this.Response.Body.ReturnValue, this.EndMethod.ReturnTypeCustomAttributes, TypeLoader.GetReturnValueName(this.DefaultName), this.DefaultNS);
                        AddAdditionalAttributes(this.Response.Body.ReturnValue, this.EndMethod.ReturnTypeCustomAttributes, this.IsEncoded);
                    }

                    GenerateMessageBodyParts(false /*generateTypedMessages*/);
                }
            }

            void GenerateTypedMessageOperation(bool hideFromEditor, ref OperationFormatStyle style)
            {
                CreateOrOverrideActionProperties();

                if (this.HasUntypedMessages)
                {
                    CreateUntypedMessages();
                    if (this.IsCompletelyUntyped)
                        return;
                }

                CodeNamespace ns = this.Context.ServiceContractGenerator.NamespaceManager.EnsureNamespace(this.ContractNS);

                if (!this.Request.IsUntypedMessage)
                {
                    CodeTypeReference typedReqMessageRef = GenerateTypedMessageHeaderAndReturnValueParts(ns, this.DefaultName + "Request", this.Request, false /*isReply*/, hideFromEditor, ref this.IsNewRequest, out this.BeginPartCodeGenerator);
                    this.Method.Parameters.Insert(0, new CodeParameterDeclarationExpression(typedReqMessageRef, "request"));
                }

                if (!this.Oneway && !this.Response.IsUntypedMessage)
                {
                    CodeTypeReference typedRespMessageRef = GenerateTypedMessageHeaderAndReturnValueParts(ns, this.DefaultName + "Response", this.Response, true /*isReply*/, hideFromEditor, ref this.IsNewResponse, out this.EndPartCodeGenerator);
                    this.EndMethod.ReturnType = typedRespMessageRef;
                }

                GenerateMessageBodyParts(true /*generateTypedMessages*/);

                if (!this.IsEncoded)
                    style = OperationFormatStyle.Document;
            }

            CodeTypeReference GenerateTypedMessageHeaderAndReturnValueParts(CodeNamespace ns, string defaultName, MessageDescription message, bool isReply, bool hideFromEditor, ref bool isNewMessage, out IPartCodeGenerator partCodeGenerator)
            {
                CodeTypeReference typedMessageRef;
                if (TypedMessageHelper.FindGeneratedTypedMessage(this.Context.Contract, message, out typedMessageRef))
                {
                    partCodeGenerator = null;
                    isNewMessage = false;
                }
                else
                {
                    UniqueCodeNamespaceScope namespaceScope = new UniqueCodeNamespaceScope(ns);

                    CodeTypeDeclaration typedMessageDecl = Context.Contract.TypeFactory.CreateClassType();
                    string messageName = XmlName.IsNullOrEmpty(message.MessageName) ? null : message.MessageName.DecodedName;
                    typedMessageRef = namespaceScope.AddUnique(typedMessageDecl, messageName, defaultName);

                    TypedMessageHelper.AddGeneratedTypedMessage(this.Context.Contract, message, typedMessageRef);

                    if (this.MessageContractType == MessageContractType.BareMessageContract && message.Body.WrapperName != null)
                        WrapTypedMessage(ns, typedMessageDecl.Name, message, isReply, this.Context.IsInherited, hideFromEditor);

                    partCodeGenerator = new TypedMessagePartCodeGenerator(typedMessageDecl);

                    if (hideFromEditor)
                    {
                        TypedMessageHelper.AddEditorBrowsableAttribute(typedMessageDecl.CustomAttributes);
                    }
                    TypedMessageHelper.GenerateWrapperAttribute(message, partCodeGenerator);
                    TypedMessageHelper.GenerateProtectionLevelAttribute(message, partCodeGenerator);

                    foreach (MessageHeaderDescription setting in message.Headers)
                        GenerateHeaderPart(setting, partCodeGenerator);

                    if (isReply && message.Body.ReturnValue != null)
                    {
                        GenerateBodyPart(0, message.Body.ReturnValue, partCodeGenerator, true, this.IsEncoded, this.DefaultNS);
                    }
                }
                return typedMessageRef;
            }


            bool IsCompletelyUntyped
            {
                get
                {
                    bool isRequestMessage = this.Request != null && this.Request.IsUntypedMessage;
                    bool isResponseMessage = this.Response != null && this.Response.IsUntypedMessage;

                    if (isRequestMessage && isResponseMessage)
                        return true;
                    else if (isResponseMessage && Request == null || IsEmpty(Request))
                        return true;
                    else if (isRequestMessage && Response == null || IsEmpty(Response))
                        return true;
                    else
                        return false;
                }
            }

            bool IsEmpty(MessageDescription message)
            {
                return (message.Body.Parts.Count == 0 && message.Headers.Count == 0);
            }

            bool HasUntypedMessages
            {
                get
                {
                    bool isRequestMessage = this.Request != null && this.Request.IsUntypedMessage;
                    bool isResponseMessage = this.Response != null && this.Response.IsUntypedMessage;
                    return (isRequestMessage || isResponseMessage);
                }
            }

            void CreateUntypedMessages()
            {
                bool isRequestMessage = this.Request != null && this.Request.IsUntypedMessage;
                bool isResponseMessage = this.Response != null && this.Response.IsUntypedMessage;

                if (isRequestMessage)
                    this.Method.Parameters.Insert(0, new CodeParameterDeclarationExpression(Context.ServiceContractGenerator.GetCodeTypeReference((typeof(Message))), "request"));
                if (isResponseMessage)
                    this.EndMethod.ReturnType = Context.ServiceContractGenerator.GetCodeTypeReference(typeof(Message));
            }

            void CreateOrOverrideActionProperties()
            {
                if (this.Request != null)
                {
                    CustomAttributeHelper.CreateOrOverridePropertyDeclaration(
                        CustomAttributeHelper.FindOrCreateAttributeDeclaration<OperationContractAttribute>(this.Method.CustomAttributes), OperationContractAttribute.ActionPropertyName, this.Request.Action);
                }
                if (this.Response != null)
                {
                    CustomAttributeHelper.CreateOrOverridePropertyDeclaration(
                        CustomAttributeHelper.FindOrCreateAttributeDeclaration<OperationContractAttribute>(this.Method.CustomAttributes), OperationContractAttribute.ReplyActionPropertyName, this.Response.Action);
                }
            }

            interface IPartCodeGenerator
            {
                CodeAttributeDeclarationCollection AddPart(CodeTypeReference type, ref string name);
                CodeAttributeDeclarationCollection MessageLevelAttributes { get; }
                void EndCodeGeneration();
            }

            class ParameterizedMethodGenerator
            {
                ParametersPartCodeGenerator ins;
                ParametersPartCodeGenerator outs;
                bool isSync;

                internal ParameterizedMethodGenerator(CodeMemberMethod beginMethod, CodeMemberMethod endMethod)
                {
                    this.ins = new ParametersPartCodeGenerator(this, beginMethod.Name, beginMethod.Parameters, beginMethod.CustomAttributes, FieldDirection.In);
                    this.outs = new ParametersPartCodeGenerator(this, beginMethod.Name, endMethod.Parameters, beginMethod.CustomAttributes, FieldDirection.Out);
                    this.isSync = (beginMethod == endMethod);
                }

                internal CodeParameterDeclarationExpression GetOrCreateParameter(CodeTypeReference type, string name, FieldDirection direction, ref int index, out bool createdNew)
                {
                    Fx.Assert(System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(name), String.Format(System.Globalization.CultureInfo.InvariantCulture, "Type name '{0}' is not ValidLanguageIndependentIdentifier!", name));
                    ParametersPartCodeGenerator existingParams = direction != FieldDirection.In ? ins : outs;
                    int i = index;
                    CodeParameterDeclarationExpression existing = existingParams.GetParameter(name, ref i);
                    bool isRef = existing != null && existing.Type.BaseType == type.BaseType;
                    if (isRef)
                    {
                        existing.Direction = FieldDirection.Ref;
                        if (isSync)
                        {
                            index = i + 1;
                            createdNew = false;
                            return existing;
                        }
                    }

                    CodeParameterDeclarationExpression paramDecl = new CodeParameterDeclarationExpression();
                    paramDecl.Name = name;
                    paramDecl.Type = type;
                    paramDecl.Direction = direction;
                    if (isRef)
                        paramDecl.Direction = FieldDirection.Ref;

                    createdNew = true;

                    return paramDecl;
                }

                internal IPartCodeGenerator InputGenerator
                {
                    get
                    {
                        return this.ins;
                    }
                }

                internal IPartCodeGenerator OutputGenerator
                {
                    get
                    {
                        return this.outs;
                    }
                }

                class ParametersPartCodeGenerator : IPartCodeGenerator
                {
                    ParameterizedMethodGenerator parent;
                    FieldDirection direction;
                    CodeParameterDeclarationExpressionCollection parameters;
                    CodeAttributeDeclarationCollection messageAttrs;
                    string methodName;
                    int index;

                    internal ParametersPartCodeGenerator(ParameterizedMethodGenerator parent, string methodName, CodeParameterDeclarationExpressionCollection parameters, CodeAttributeDeclarationCollection messageAttrs, FieldDirection direction)
                    {
                        this.parent = parent;
                        this.methodName = methodName;
                        this.parameters = parameters;
                        this.messageAttrs = messageAttrs;
                        this.direction = direction;
                        this.index = 0;
                    }

                    public bool NameExists(string name)
                    {
                        if (String.Compare(name, methodName, StringComparison.OrdinalIgnoreCase) == 0)
                            return true;
                        int index = 0;
                        return GetParameter(name, ref index) != null;
                    }

                    CodeAttributeDeclarationCollection IPartCodeGenerator.AddPart(CodeTypeReference type, ref string name)
                    {
                        bool createdNew;
                        name = UniqueCodeIdentifierScope.MakeValid(name, "param");
                        CodeParameterDeclarationExpression paramDecl = parent.GetOrCreateParameter(type, name, this.direction, ref index, out createdNew);
                        if (createdNew)
                        {
                            paramDecl.Name = GetUniqueParameterName(paramDecl.Name, this);
                            parameters.Insert(this.index++, paramDecl);
                        }

                        name = paramDecl.Name;
                        if (!createdNew)
                            return null;
                        return paramDecl.CustomAttributes;
                    }


                    internal CodeParameterDeclarationExpression GetParameter(string name, ref int index)
                    {
                        for (int i = index; i < parameters.Count; i++)
                        {
                            CodeParameterDeclarationExpression parameter = parameters[i];
                            if (String.Compare(parameter.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                index = i;
                                return parameter;
                            }
                        }

                        return null;
                    }

                    CodeAttributeDeclarationCollection IPartCodeGenerator.MessageLevelAttributes
                    {
                        get
                        {
                            return messageAttrs;
                        }
                    }

                    void IPartCodeGenerator.EndCodeGeneration()
                    {
                    }

                    static string GetUniqueParameterName(string name, ParametersPartCodeGenerator parameters)
                    {
                        return NamingHelper.GetUniqueName(name, DoesParameterNameExist, parameters);
                    }
                    static bool DoesParameterNameExist(string name, object parametersObject)
                    {
                        return ((ParametersPartCodeGenerator)parametersObject).NameExists(name);
                    }
                }

            }


            class TypedMessagePartCodeGenerator : IPartCodeGenerator
            {
                CodeTypeDeclaration typeDecl;
                UniqueCodeIdentifierScope memberScope;

                internal TypedMessagePartCodeGenerator(CodeTypeDeclaration typeDecl)
                {
                    this.typeDecl = typeDecl;
                    this.memberScope = new UniqueCodeIdentifierScope();
                    this.memberScope.AddReserved(typeDecl.Name);
                }

                CodeAttributeDeclarationCollection IPartCodeGenerator.AddPart(CodeTypeReference type, ref string name)
                {
                    CodeMemberField memberDecl = new CodeMemberField();
                    memberDecl.Name = name = this.memberScope.AddUnique(name, "member");
                    memberDecl.Type = type;
                    memberDecl.Attributes = MemberAttributes.Public;
                    typeDecl.Members.Add(memberDecl);
                    return memberDecl.CustomAttributes;
                }

                CodeAttributeDeclarationCollection IPartCodeGenerator.MessageLevelAttributes
                {
                    get
                    {
                        return typeDecl.CustomAttributes;
                    }
                }

                void IPartCodeGenerator.EndCodeGeneration()
                {
                    TypedMessageHelper.GenerateConstructors(typeDecl);
                }
            }

            void WrapTypedMessage(CodeNamespace ns, string typeName, MessageDescription messageDescription, bool isReply, bool isInherited, bool hideFromEditor)
            {
                Fx.Assert(String.IsNullOrEmpty(typeName) || System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(typeName), String.Format(System.Globalization.CultureInfo.InvariantCulture, "Type name '{0}' is not ValidLanguageIndependentIdentifier!", typeName));
                UniqueCodeNamespaceScope namespaceScope = new UniqueCodeNamespaceScope(ns);
                CodeTypeDeclaration wrapperTypeDecl = Context.Contract.TypeFactory.CreateClassType();
                CodeTypeReference wrapperTypeRef = namespaceScope.AddUnique(wrapperTypeDecl, typeName + "Body", "Body");

                if (hideFromEditor)
                {
                    TypedMessageHelper.AddEditorBrowsableAttribute(wrapperTypeDecl.CustomAttributes);
                }

                string defaultNS = GetWrapperNamespace(messageDescription);
                string messageName = XmlName.IsNullOrEmpty(messageDescription.MessageName) ? null : messageDescription.MessageName.DecodedName;
                this.WrappedBodyTypeGenerator.AddTypeAttributes(messageName, defaultNS, wrapperTypeDecl.CustomAttributes, this.IsEncoded);

                IPartCodeGenerator partGenerator = new TypedMessagePartCodeGenerator(wrapperTypeDecl);
                System.Net.Security.ProtectionLevel protectionLevel = System.Net.Security.ProtectionLevel.None;
                bool isProtectionLevelSetExplicitly = false;

                if (messageDescription.Body.ReturnValue != null)
                {
                    AddWrapperPart(messageDescription.MessageName, this.WrappedBodyTypeGenerator, partGenerator, messageDescription.Body.ReturnValue, wrapperTypeDecl.CustomAttributes);
                    protectionLevel = ProtectionLevelHelper.Max(protectionLevel, messageDescription.Body.ReturnValue.ProtectionLevel);
                    if (messageDescription.Body.ReturnValue.HasProtectionLevel)
                        isProtectionLevelSetExplicitly = true;
                }

                List<CodeTypeReference> wrapperKnownTypes = new List<CodeTypeReference>();
                foreach (MessagePartDescription part in messageDescription.Body.Parts)
                {
                    AddWrapperPart(messageDescription.MessageName, this.WrappedBodyTypeGenerator, partGenerator, part, wrapperTypeDecl.CustomAttributes);
                    protectionLevel = ProtectionLevelHelper.Max(protectionLevel, part.ProtectionLevel);
                    if (part.HasProtectionLevel)
                        isProtectionLevelSetExplicitly = true;

                    ICollection<CodeTypeReference> knownTypesForPart = null;
                    if (this.KnownTypes != null && this.KnownTypes.TryGetValue(part, out knownTypesForPart))
                    {
                        foreach (CodeTypeReference typeReference in knownTypesForPart)
                            wrapperKnownTypes.Add(typeReference);
                    }
                }
                messageDescription.Body.Parts.Clear();

                MessagePartDescription wrapperPart = new MessagePartDescription(messageDescription.Body.WrapperName, messageDescription.Body.WrapperNamespace);
                if (this.KnownTypes != null)
                    this.KnownTypes.Add(wrapperPart, wrapperKnownTypes);
                if (isProtectionLevelSetExplicitly)
                    wrapperPart.ProtectionLevel = protectionLevel;
                messageDescription.Body.WrapperName = null;
                messageDescription.Body.WrapperNamespace = null;
                if (isReply)
                    messageDescription.Body.ReturnValue = wrapperPart;
                else
                    messageDescription.Body.Parts.Add(wrapperPart);
                TypedMessageHelper.GenerateConstructors(wrapperTypeDecl);
                this.Parent.ParameterTypes.Add(wrapperPart, wrapperTypeRef);
                this.Parent.SpecialPartName.Add(wrapperPart, "Body");

            }

            string GetWrapperNamespace(MessageDescription messageDescription)
            {
                string defaultNS = this.DefaultNS;
                if (messageDescription.Body.ReturnValue != null)
                    defaultNS = messageDescription.Body.ReturnValue.Namespace;
                else if (messageDescription.Body.Parts.Count > 0)
                    defaultNS = messageDescription.Body.Parts[0].Namespace;
                return defaultNS;
            }

            void GenerateMessageBodyParts(bool generateTypedMessages)
            {
                int order = 0;
                if (this.IsNewRequest)
                {
                    foreach (MessagePartDescription setting in this.Request.Body.Parts)
                        GenerateBodyPart(order++, setting, this.BeginPartCodeGenerator, generateTypedMessages, this.IsEncoded, this.DefaultNS);
                }

                if (!this.Oneway && IsNewResponse)
                {
                    order = this.Response.Body.ReturnValue != null ? 1 : 0;
                    foreach (MessagePartDescription setting in this.Response.Body.Parts)
                        GenerateBodyPart(order++, setting, this.EndPartCodeGenerator, generateTypedMessages, this.IsEncoded, this.DefaultNS);
                }
                if (IsNewRequest)
                {
                    if (this.BeginPartCodeGenerator != null)
                        this.BeginPartCodeGenerator.EndCodeGeneration();
                }
                if (IsNewResponse)
                {
                    if (EndPartCodeGenerator != null)
                        this.EndPartCodeGenerator.EndCodeGeneration();
                }
            }

            void AddWrapperPart(XmlName messageName, IWrappedBodyTypeGenerator wrappedBodyTypeGenerator, IPartCodeGenerator partGenerator, MessagePartDescription part, CodeAttributeDeclarationCollection typeAttributes)
            {
                string fieldName = part.CodeName;
                CodeTypeReference type;
                if (part.Type == typeof(System.IO.Stream))
                    type = Context.ServiceContractGenerator.GetCodeTypeReference(typeof(byte[]));
                else
                    type = GetParameterType(part);
                CodeAttributeDeclarationCollection fieldAttributes = partGenerator.AddPart(type, ref fieldName);

                CodeAttributeDeclarationCollection importedAttributes = null;

                bool hasAttributes = this.Parent.ParameterAttributes.TryGetValue(part, out importedAttributes);

                wrappedBodyTypeGenerator.AddMemberAttributes(messageName, part, importedAttributes, typeAttributes, fieldAttributes);
                this.Parent.ParameterTypes.Remove(part);
                if (hasAttributes)
                    this.Parent.ParameterAttributes.Remove(part);
            }

            void GenerateBodyPart(int order, MessagePartDescription messagePart, IPartCodeGenerator partCodeGenerator, bool generateTypedMessage, bool isEncoded, string defaultNS)
            {
                if (!generateTypedMessage) order = -1;

                string partName;
                if (!this.Parent.SpecialPartName.TryGetValue(messagePart, out partName))
                    partName = messagePart.CodeName;

                CodeTypeReference partType = GetParameterType(messagePart);
                CodeAttributeDeclarationCollection partAttributes = partCodeGenerator.AddPart(partType, ref partName);

                if (partAttributes == null)
                    return;

                XmlName xmlPartName = new XmlName(partName);
                if (generateTypedMessage)
                    TypedMessageHelper.GenerateMessageBodyMemberAttribute(order, messagePart, partAttributes, xmlPartName);
                else
                    ParameterizedMessageHelper.GenerateMessageParameterAttribute(messagePart, partAttributes, xmlPartName, defaultNS);

                AddAdditionalAttributes(messagePart, partAttributes, generateTypedMessage || isEncoded);
            }

            void GenerateHeaderPart(MessageHeaderDescription setting, IPartCodeGenerator parts)
            {
                string partName;
                if (!this.Parent.SpecialPartName.TryGetValue(setting, out partName))
                    partName = setting.CodeName;
                CodeTypeReference partType = GetParameterType(setting);
                CodeAttributeDeclarationCollection partAttributes = parts.AddPart(partType, ref partName);
                TypedMessageHelper.GenerateMessageHeaderAttribute(setting, partAttributes, new XmlName(partName));
                AddAdditionalAttributes(setting, partAttributes, true /*isAdditionalAttributesAllowed*/);
            }

            CodeTypeReference GetParameterType(MessagePartDescription setting)
            {
                if (setting.Type != null)
                    return Context.ServiceContractGenerator.GetCodeTypeReference(setting.Type);
                else if (this.Parent.parameterTypes.ContainsKey(setting))
                    return this.Parent.parameterTypes[setting];
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SfxNoTypeSpecifiedForParameter, setting.Name)));
            }

            void AddAdditionalAttributes(MessagePartDescription setting, CodeAttributeDeclarationCollection attributes, bool isAdditionalAttributesAllowed)
            {
                if (this.Parent.parameterAttributes != null && this.Parent.parameterAttributes.ContainsKey(setting))
                {
                    CodeAttributeDeclarationCollection localAttributes = this.Parent.parameterAttributes[setting];
                    if (localAttributes != null && localAttributes.Count > 0)
                    {
                        if (isAdditionalAttributesAllowed)
                            attributes.AddRange(localAttributes);
                        else
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(SR.GetString(SR.SfxUseTypedMessageForCustomAttributes, setting.Name, localAttributes[0].AttributeType.BaseType)));

                    }
                }
            }


            static class TypedMessageHelper
            {
                internal static void GenerateProtectionLevelAttribute(MessageDescription message, IPartCodeGenerator partCodeGenerator)
                {
                    CodeAttributeDeclaration messageContractAttr = CustomAttributeHelper.FindOrCreateAttributeDeclaration<MessageContractAttribute>(partCodeGenerator.MessageLevelAttributes);
                    if (message.HasProtectionLevel)
                    {
                        messageContractAttr.Arguments.Add(new CodeAttributeArgument("ProtectionLevel",
                            new CodeFieldReferenceExpression(
                                new CodeTypeReferenceExpression(typeof(ProtectionLevel)), message.ProtectionLevel.ToString())));
                    }
                }

                internal static void GenerateWrapperAttribute(MessageDescription message, IPartCodeGenerator partCodeGenerator)
                {
                    CodeAttributeDeclaration messageContractAttr = CustomAttributeHelper.FindOrCreateAttributeDeclaration<MessageContractAttribute>(partCodeGenerator.MessageLevelAttributes);
                    if (message.Body.WrapperName != null)
                    {
                        // use encoded name to specify exactly what goes on the wire.
                        messageContractAttr.Arguments.Add(new CodeAttributeArgument("WrapperName",
                            new CodePrimitiveExpression(NamingHelper.CodeName(message.Body.WrapperName))));
                        messageContractAttr.Arguments.Add(new CodeAttributeArgument("WrapperNamespace",
                            new CodePrimitiveExpression(message.Body.WrapperNamespace)));
                        messageContractAttr.Arguments.Add(new CodeAttributeArgument("IsWrapped",
                            new CodePrimitiveExpression(true)));
                    }
                    else
                        messageContractAttr.Arguments.Add(new CodeAttributeArgument("IsWrapped",
                            new CodePrimitiveExpression(false)));
                }

                internal static void AddEditorBrowsableAttribute(CodeAttributeDeclarationCollection attributes)
                {
                    attributes.Add(ClientClassGenerator.CreateEditorBrowsableAttribute(EditorBrowsableState.Advanced));
                }

                internal static void AddGeneratedTypedMessage(ServiceContractGenerationContext contract, MessageDescription message, CodeTypeReference codeTypeReference)
                {
                    if (message.XsdTypeName != null && !message.XsdTypeName.IsEmpty)
                    {
                        contract.ServiceContractGenerator.GeneratedTypedMessages.Add(message, codeTypeReference);
                    }
                }

                internal static bool FindGeneratedTypedMessage(ServiceContractGenerationContext contract, MessageDescription message, out CodeTypeReference codeTypeReference)
                {
                    if (message.XsdTypeName == null || message.XsdTypeName.IsEmpty)
                    {
                        codeTypeReference = null;
                        return false;
                    }
                    return contract.ServiceContractGenerator.GeneratedTypedMessages.TryGetValue(message, out codeTypeReference);
                }

                internal static void GenerateConstructors(CodeTypeDeclaration typeDecl)
                {
                    CodeConstructor defaultCtor = new CodeConstructor();
                    defaultCtor.Attributes = MemberAttributes.Public;
                    typeDecl.Members.Add(defaultCtor);
                    CodeConstructor otherCtor = new CodeConstructor();
                    otherCtor.Attributes = MemberAttributes.Public;
                    foreach (CodeTypeMember member in typeDecl.Members)
                    {
                        CodeMemberField field = member as CodeMemberField;
                        if (field == null)
                            continue;
                        CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression(field.Type, field.Name);
                        otherCtor.Parameters.Add(param);
                        otherCtor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name), new CodeArgumentReferenceExpression(param.Name)));
                    }
                    if (otherCtor.Parameters.Count > 0)
                        typeDecl.Members.Add(otherCtor);
                }

                internal static void GenerateMessageBodyMemberAttribute(int order, MessagePartDescription setting, CodeAttributeDeclarationCollection attributes, XmlName defaultName)
                {
                    GenerateMessageContractMemberAttribute<MessageBodyMemberAttribute>(order, setting, attributes, defaultName);
                }

                internal static void GenerateMessageHeaderAttribute(MessageHeaderDescription setting, CodeAttributeDeclarationCollection attributes, XmlName defaultName)
                {
                    if (setting.Multiple)
                        GenerateMessageContractMemberAttribute<MessageHeaderArrayAttribute>(-1, setting, attributes, defaultName);
                    else
                        GenerateMessageContractMemberAttribute<MessageHeaderAttribute>(-1, setting, attributes, defaultName);
                }

                static void GenerateMessageContractMemberAttribute<T>(int order, MessagePartDescription setting, CodeAttributeDeclarationCollection attrs, XmlName defaultName)
                    where T : Attribute
                {
                    CodeAttributeDeclaration decl = CustomAttributeHelper.FindOrCreateAttributeDeclaration<T>(attrs);

                    if (setting.Name != defaultName.EncodedName)
                        // override name with encoded value specified in wsdl; this only works beacuse
                        // our Encoding algorithm will leave alredy encoded names untouched
                        CustomAttributeHelper.CreateOrOverridePropertyDeclaration(decl, MessageContractMemberAttribute.NamePropertyName, setting.Name);

                    CustomAttributeHelper.CreateOrOverridePropertyDeclaration(decl, MessageContractMemberAttribute.NamespacePropertyName, setting.Namespace);

                    if (setting.HasProtectionLevel)
                        CustomAttributeHelper.CreateOrOverridePropertyDeclaration(decl, MessageContractMemberAttribute.ProtectionLevelPropertyName, setting.ProtectionLevel);

                    if (order >= 0)
                        CustomAttributeHelper.CreateOrOverridePropertyDeclaration(decl, MessageBodyMemberAttribute.OrderPropertyName, order);
                }

            }

            static class ParameterizedMessageHelper
            {

                internal static void GenerateMessageParameterAttribute(MessagePartDescription setting, CodeAttributeDeclarationCollection attributes, XmlName defaultName, string defaultNS)
                {
                    if (setting.Name != defaultName.EncodedName)
                    {
                        // override name with encoded value specified in wsdl; this only works beacuse
                        // our Encoding algorithm will leave alredy encoded names untouched
                        CustomAttributeHelper.CreateOrOverridePropertyDeclaration(
                            CustomAttributeHelper.FindOrCreateAttributeDeclaration<MessageParameterAttribute>(attributes), MessageParameterAttribute.NamePropertyName, setting.Name);
                    }
                    if (setting.Namespace != defaultNS)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(SR.GetString(SR.SFxCannotImportAsParameters_NamespaceMismatch, setting.Namespace, defaultNS)));
                }

                internal static void ValidateProtectionLevel(MethodSignatureGenerator parent)
                {
                    if (parent.Request != null && parent.Request.HasProtectionLevel)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(SR.GetString(SR.SFxCannotImportAsParameters_MessageHasProtectionLevel, parent.Request.Action == null ? "" : parent.Request.Action)));
                    }
                    if (parent.Response != null && parent.Response.HasProtectionLevel)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(SR.GetString(SR.SFxCannotImportAsParameters_MessageHasProtectionLevel, parent.Response.Action == null ? "" : parent.Response.Action)));
                    }
                }

                internal static void ValidateWrapperSettings(MethodSignatureGenerator parent)
                {
                    if (parent.Request.Body.WrapperName == null || (parent.Response != null && parent.Response.Body.WrapperName == null))
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(SR.GetString(SR.SFxCannotImportAsParameters_Bare, parent.Context.Operation.CodeName)));

                    if (!StringEqualOrNull(parent.Request.Body.WrapperNamespace, parent.ContractNS))
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(SR.GetString(SR.SFxCannotImportAsParameters_DifferentWrapperNs, parent.Request.MessageName, parent.Request.Body.WrapperNamespace, parent.ContractNS)));

                    XmlName defaultName = new XmlName(parent.DefaultName);
                    if (!String.Equals(parent.Request.Body.WrapperName, defaultName.EncodedName, StringComparison.Ordinal))
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(SR.GetString(SR.SFxCannotImportAsParameters_DifferentWrapperName, parent.Request.MessageName, parent.Request.Body.WrapperName, defaultName.EncodedName)));

                    if (parent.Response != null)
                    {
                        if (!StringEqualOrNull(parent.Response.Body.WrapperNamespace, parent.ContractNS))
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(SR.GetString(SR.SFxCannotImportAsParameters_DifferentWrapperNs, parent.Response.MessageName, parent.Response.Body.WrapperNamespace, parent.ContractNS)));

                        if (!String.Equals(parent.Response.Body.WrapperName, TypeLoader.GetBodyWrapperResponseName(defaultName).EncodedName, StringComparison.Ordinal))
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(SR.GetString(SR.SFxCannotImportAsParameters_DifferentWrapperName, parent.Response.MessageName, parent.Response.Body.WrapperName, defaultName.EncodedName)));
                    }
                }

                internal static void ValidateNoHeaders(MethodSignatureGenerator parent)
                {
                    if (parent.Request.Headers.Count > 0)
                    {
                        if (parent.IsEncoded)
                        {
                            parent.Context.Contract.ServiceContractGenerator.Errors.Add(new MetadataConversionError(SR.GetString(SR.SFxCannotImportAsParameters_HeadersAreIgnoredInEncoded, parent.Request.MessageName), true/*isWarning*/));
                        }
                        else
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(SR.GetString(SR.SFxCannotImportAsParameters_HeadersAreUnsupported, parent.Request.MessageName)));
                    }

                    if (!parent.Oneway && parent.Response.Headers.Count > 0)
                    {
                        if (parent.IsEncoded)
                            parent.Context.Contract.ServiceContractGenerator.Errors.Add(new MetadataConversionError(SR.GetString(SR.SFxCannotImportAsParameters_HeadersAreIgnoredInEncoded, parent.Response.MessageName), true/*isWarning*/));
                        else
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ParameterModeException(SR.GetString(SR.SFxCannotImportAsParameters_HeadersAreUnsupported, parent.Response.MessageName)));
                    }

                }

                static bool StringEqualOrNull(string overrideValue, string defaultValue)
                {
                    return overrideValue == null || String.Equals(overrideValue, defaultValue, StringComparison.Ordinal);
                }
            }

            internal void GenerateTaskSignature(ref OperationFormatStyle style)
            {
                this.Method = this.Context.TaskMethod;
                this.EndMethod = this.Context.TaskMethod;
                this.DefaultName = this.Context.SyncMethod.Name;
                GenerateOperationSignatures(ref style);

                CodeTypeReference resultType = this.Method.ReturnType;
                CodeTypeReference taskReturnType;
                if (resultType.BaseType == ServiceReflector.VoidType.FullName)
                {
                    taskReturnType = new CodeTypeReference(ServiceReflector.taskType);
                }
                else
                {
                    taskReturnType = new CodeTypeReference(this.Context.ServiceContractGenerator.GetCodeTypeReference(ServiceReflector.taskTResultType).BaseType, resultType);
                }
                
                this.Method.ReturnType = taskReturnType;
            }
        }

        static class CustomAttributeHelper
        {
            internal static void CreateOrOverridePropertyDeclaration<V>(CodeAttributeDeclaration attribute, string propertyName, V value)
            {
                SecurityAttributeGenerationHelper.CreateOrOverridePropertyDeclaration<V>(attribute, propertyName, value);
            }

            internal static CodeAttributeDeclaration FindOrCreateAttributeDeclaration<T>(CodeAttributeDeclarationCollection attributes)
                where T : Attribute
            {
                return SecurityAttributeGenerationHelper.FindOrCreateAttributeDeclaration<T>(attributes);
            }

            internal static CodeAttributeDeclaration GenerateAttributeDeclaration(ServiceContractGenerator generator, Attribute attribute)
            {
                Type attributeType = attribute.GetType();
                Attribute defaultAttribute = (Attribute)Activator.CreateInstance(attributeType);
                MemberInfo[] publicMembers = attributeType.GetMembers(BindingFlags.Instance | BindingFlags.Public);
                Array.Sort<MemberInfo>(publicMembers,
                                       delegate(MemberInfo a, MemberInfo b)
                                       {
                                           return String.Compare(a.Name, b.Name, StringComparison.Ordinal);
                                       }
                );
                // we should create this reference through ServiceContractGenerator, which tracks referenced assemblies
                CodeAttributeDeclaration attr = new CodeAttributeDeclaration(generator.GetCodeTypeReference(attributeType));
                foreach (MemberInfo member in publicMembers)
                {
                    if (member.DeclaringType == typeof(Attribute))
                        continue;
                    FieldInfo field = member as FieldInfo;
                    if (field != null)
                    {
                        object fieldValue = field.GetValue(attribute);
                        object defaultValue = field.GetValue(defaultAttribute);

                        if (!object.Equals(fieldValue, defaultValue))
                            attr.Arguments.Add(new CodeAttributeArgument(field.Name, GetArgValue(fieldValue)));
                        continue;
                    }
                    PropertyInfo property = member as PropertyInfo;
                    if (property != null)
                    {
                        object propertyValue = property.GetValue(attribute, null);
                        object defaultValue = property.GetValue(defaultAttribute, null);
                        if (!object.Equals(propertyValue, defaultValue))
                            attr.Arguments.Add(new CodeAttributeArgument(property.Name, GetArgValue(propertyValue)));
                        continue;
                    }
                }
                return attr;
            }

            static CodeExpression GetArgValue(object val)
            {
                Type type = val.GetType();
                if (type.IsPrimitive || type == typeof(string))
                    return new CodePrimitiveExpression(val);
                if (type.IsEnum)
                    return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(type), Enum.Format(type, val, "G"));

                Fx.Assert(String.Format(System.Globalization.CultureInfo.InvariantCulture, "Attribute generation is not supported for argument type {0}", type));
                return null;
            }
        }
    }

    class ParameterModeException : Exception
    {
        MessageContractType messageContractType = MessageContractType.WrappedMessageContract;
        public ParameterModeException() { }
        public ParameterModeException(string message) : base(message) { }
        public ParameterModeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public MessageContractType MessageContractType
        {
            get { return messageContractType; }
            set { messageContractType = value; }
        }

    }

}
