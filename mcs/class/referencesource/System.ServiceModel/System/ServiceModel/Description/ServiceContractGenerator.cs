//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Net.Security;
    using System.Reflection;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    public class ServiceContractGenerator
    {
        CodeCompileUnit compileUnit;
        ConfigWriter configWriter;
        Configuration configuration;
        NamespaceHelper namespaceManager;

        // options
        OptionsHelper options = new OptionsHelper(ServiceContractGenerationOptions.ChannelInterface |
                                                    ServiceContractGenerationOptions.ClientClass);

        Dictionary<ContractDescription, Type> referencedTypes;
        Dictionary<ContractDescription, ServiceContractGenerationContext> generatedTypes;
        Dictionary<OperationDescription, OperationContractGenerationContext> generatedOperations;
        Dictionary<MessageDescription, CodeTypeReference> generatedTypedMessages;

        Collection<MetadataConversionError> errors = new Collection<MetadataConversionError>();

        public ServiceContractGenerator()
            : this(null, null)
        {
        }

        public ServiceContractGenerator(Configuration targetConfig)
            : this(null, targetConfig)
        {
        }

        public ServiceContractGenerator(CodeCompileUnit targetCompileUnit)
            : this(targetCompileUnit, null)
        {
        }

        public ServiceContractGenerator(CodeCompileUnit targetCompileUnit, Configuration targetConfig)
        {
            this.compileUnit = targetCompileUnit ?? new CodeCompileUnit();
            this.namespaceManager = new NamespaceHelper(this.compileUnit.Namespaces);

            AddReferencedAssembly(typeof(ServiceContractGenerator).Assembly);
            this.configuration = targetConfig;
            if (targetConfig != null)
                this.configWriter = new ConfigWriter(targetConfig);
            this.generatedTypes = new Dictionary<ContractDescription, ServiceContractGenerationContext>();
            this.generatedOperations = new Dictionary<OperationDescription, OperationContractGenerationContext>();
            this.referencedTypes = new Dictionary<ContractDescription, Type>();
        }

        internal CodeTypeReference GetCodeTypeReference(Type type)
        {
            AddReferencedAssembly(type.Assembly);
            return new CodeTypeReference(type);
        }

        internal void AddReferencedAssembly(Assembly assembly)
        {
            string assemblyName = System.IO.Path.GetFileName(assembly.Location);
            bool alreadyExisting = false;
            foreach (string existingName in this.compileUnit.ReferencedAssemblies)
            {
                if (String.Compare(existingName, assemblyName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    alreadyExisting = true;
                    break;
                }
            }
            if (!alreadyExisting)
                this.compileUnit.ReferencedAssemblies.Add(assemblyName);

        }

        // options
        public ServiceContractGenerationOptions Options
        {
            get { return this.options.Options; }
            set { this.options = new OptionsHelper(value); }
        }

        internal OptionsHelper OptionsInternal
        {
            get { return this.options; }
        }

        public Dictionary<ContractDescription, Type> ReferencedTypes
        {
            get { return this.referencedTypes; }
        }

        public CodeCompileUnit TargetCompileUnit
        {
            get { return this.compileUnit; }
        }

        public Configuration Configuration
        {
            get { return this.configuration; }
        }

        public Dictionary<string, string> NamespaceMappings
        {
            get { return this.NamespaceManager.NamespaceMappings; }
        }

        public Collection<MetadataConversionError> Errors
        {
            get { return errors; }
        }

        internal NamespaceHelper NamespaceManager
        {
            get { return this.namespaceManager; }
        }

        public void GenerateBinding(Binding binding, out string bindingSectionName, out string configurationName)
        {
            configWriter.WriteBinding(binding, out bindingSectionName, out configurationName);
        }

        public CodeTypeReference GenerateServiceEndpoint(ServiceEndpoint endpoint, out ChannelEndpointElement channelElement)
        {
            if (endpoint == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");

            if (configuration == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxServiceContractGeneratorConfigRequired)));

            CodeTypeReference retVal;
            string typeName;

            Type existingType;
            if (referencedTypes.TryGetValue(endpoint.Contract, out existingType))
            {
                retVal = GetCodeTypeReference(existingType);
                typeName = existingType.FullName;
            }
            else
            {
                retVal = GenerateServiceContractType(endpoint.Contract);
                typeName = retVal.BaseType;
            }
            channelElement = configWriter.WriteChannelDescription(endpoint, typeName);
            return retVal;
        }

        public CodeTypeReference GenerateServiceContractType(ContractDescription contractDescription)
        {
            CodeTypeReference retVal = GenerateServiceContractTypeInternal(contractDescription);
            System.CodeDom.Compiler.CodeGenerator.ValidateIdentifiers(TargetCompileUnit);
            return retVal;
        }

        CodeTypeReference GenerateServiceContractTypeInternal(ContractDescription contractDescription)
        {
            if (contractDescription == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractDescription");

            Type existingType;
            if (referencedTypes.TryGetValue(contractDescription, out existingType))
            {
                return GetCodeTypeReference(existingType);
            }

            ServiceContractGenerationContext context;
            CodeNamespace ns = this.NamespaceManager.EnsureNamespace(contractDescription.Namespace);
            if (!generatedTypes.TryGetValue(contractDescription, out context))
            {
                context = new ContextInitializer(this, new CodeTypeFactory(this, options.IsSet(ServiceContractGenerationOptions.InternalTypes))).CreateContext(contractDescription);

                ExtensionsHelper.CallContractExtensions(GetBeforeExtensionsBuiltInContractGenerators(), context);
                ExtensionsHelper.CallOperationExtensions(GetBeforeExtensionsBuiltInOperationGenerators(), context);

                ExtensionsHelper.CallBehaviorExtensions(context);

                ExtensionsHelper.CallContractExtensions(GetAfterExtensionsBuiltInContractGenerators(), context);
                ExtensionsHelper.CallOperationExtensions(GetAfterExtensionsBuiltInOperationGenerators(), context);

                generatedTypes.Add(contractDescription, context);
            }
            return context.ContractTypeReference;
        }

        IEnumerable<IServiceContractGenerationExtension> GetBeforeExtensionsBuiltInContractGenerators()
        {
            return EmptyArray<IServiceContractGenerationExtension>.Instance;
        }

        IEnumerable<IOperationContractGenerationExtension> GetBeforeExtensionsBuiltInOperationGenerators()
        {
            yield return new FaultContractAttributeGenerator();
            yield return new TransactionFlowAttributeGenerator();
        }

        IEnumerable<IServiceContractGenerationExtension> GetAfterExtensionsBuiltInContractGenerators()
        {
            if (this.options.IsSet(ServiceContractGenerationOptions.ChannelInterface))
            {
                yield return new ChannelInterfaceGenerator();
            }

            if (this.options.IsSet(ServiceContractGenerationOptions.ClientClass))
            {
                // unless the caller explicitly asks for TM we try to generate a helpful overload if we end up with TM
                bool tryAddHelperMethod = !this.options.IsSet(ServiceContractGenerationOptions.TypedMessages);
                bool generateEventAsyncMethods = this.options.IsSet(ServiceContractGenerationOptions.EventBasedAsynchronousMethods);
                yield return new ClientClassGenerator(tryAddHelperMethod, generateEventAsyncMethods);
            }
        }

        IEnumerable<IOperationContractGenerationExtension> GetAfterExtensionsBuiltInOperationGenerators()
        {
            return EmptyArray<IOperationContractGenerationExtension>.Instance;
        }

        internal static CodeExpression GetEnumReference<EnumType>(EnumType value)
        {
            return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(EnumType)), Enum.Format(typeof(EnumType), value, "G"));
        }

        internal Dictionary<MessageDescription, CodeTypeReference> GeneratedTypedMessages
        {
            get
            {
                if (generatedTypedMessages == null)
                    generatedTypedMessages = new Dictionary<MessageDescription, CodeTypeReference>(MessageDescriptionComparer.Singleton);
                return generatedTypedMessages;
            }
        }

        internal class ContextInitializer
        {
            readonly ServiceContractGenerator parent;
            readonly CodeTypeFactory typeFactory;
            readonly bool asyncMethods;
            readonly bool taskMethod;

            ServiceContractGenerationContext context;
            UniqueCodeIdentifierScope contractMemberScope;
            UniqueCodeIdentifierScope callbackMemberScope;

            internal ContextInitializer(ServiceContractGenerator parent, CodeTypeFactory typeFactory)
            {
                this.parent = parent;
                this.typeFactory = typeFactory;

                this.asyncMethods = parent.OptionsInternal.IsSet(ServiceContractGenerationOptions.AsynchronousMethods);
                this.taskMethod = parent.OptionsInternal.IsSet(ServiceContractGenerationOptions.TaskBasedAsynchronousMethod);
            }

            public ServiceContractGenerationContext CreateContext(ContractDescription contractDescription)
            {
                VisitContract(contractDescription);

                Fx.Assert(this.context != null, "context was not initialized");
                return this.context;
            }

            // this could usefully be factored into a base class for use by WSDL export and others
            void VisitContract(ContractDescription contract)
            {
                this.Visit(contract);

                foreach (OperationDescription operation in contract.Operations)
                {
                    this.Visit(operation);

                    // not used in this case
                    //foreach (MessageDescription message in operation.Messages)
                    //{
                    //    this.Visit(message);
                    //}
                }
            }

            void Visit(ContractDescription contractDescription)
            {
                bool isDuplex = IsDuplex(contractDescription);

                this.contractMemberScope = new UniqueCodeIdentifierScope();
                this.callbackMemberScope = isDuplex ? new UniqueCodeIdentifierScope() : null;

                UniqueCodeNamespaceScope codeNamespaceScope = new UniqueCodeNamespaceScope(parent.NamespaceManager.EnsureNamespace(contractDescription.Namespace));

                CodeTypeDeclaration contract = typeFactory.CreateInterfaceType();
                CodeTypeReference contractReference = codeNamespaceScope.AddUnique(contract, contractDescription.CodeName, Strings.DefaultContractName);

                CodeTypeDeclaration callbackContract = null;
                CodeTypeReference callbackContractReference = null;
                if (isDuplex)
                {
                    callbackContract = typeFactory.CreateInterfaceType();
                    callbackContractReference = codeNamespaceScope.AddUnique(callbackContract, contractDescription.CodeName + Strings.CallbackTypeSuffix, Strings.DefaultContractName);
                }

                this.context = new ServiceContractGenerationContext(parent, contractDescription, contract, callbackContract);
                this.context.Namespace = codeNamespaceScope.CodeNamespace;
                this.context.TypeFactory = this.typeFactory;
                this.context.ContractTypeReference = contractReference;
                this.context.DuplexCallbackTypeReference = callbackContractReference;

                AddServiceContractAttribute(this.context);
            }

            void Visit(OperationDescription operationDescription)
            {
                bool isCallback = operationDescription.IsServerInitiated();
                CodeTypeDeclaration declaringType = isCallback ? context.DuplexCallbackType : context.ContractType;
                UniqueCodeIdentifierScope memberScope = isCallback ? this.callbackMemberScope : this.contractMemberScope;

                Fx.Assert(declaringType != null, "missing callback type");

                string syncMethodName = memberScope.AddUnique(operationDescription.CodeName, Strings.DefaultOperationName);

                CodeMemberMethod syncMethod = new CodeMemberMethod();
                syncMethod.Name = syncMethodName;
                declaringType.Members.Add(syncMethod);

                OperationContractGenerationContext operationContext;
                CodeMemberMethod beginMethod = null;
                CodeMemberMethod endMethod = null;
                if (asyncMethods)
                {
                    beginMethod = new CodeMemberMethod();
                    beginMethod.Name = ServiceReflector.BeginMethodNamePrefix + syncMethodName;
                    beginMethod.Parameters.Add(new CodeParameterDeclarationExpression(context.ServiceContractGenerator.GetCodeTypeReference(typeof(AsyncCallback)), Strings.AsyncCallbackArgName));
                    beginMethod.Parameters.Add(new CodeParameterDeclarationExpression(context.ServiceContractGenerator.GetCodeTypeReference(typeof(object)), Strings.AsyncStateArgName));
                    beginMethod.ReturnType = context.ServiceContractGenerator.GetCodeTypeReference(typeof(IAsyncResult));
                    declaringType.Members.Add(beginMethod);

                    endMethod = new CodeMemberMethod();
                    endMethod.Name = ServiceReflector.EndMethodNamePrefix + syncMethodName;
                    endMethod.Parameters.Add(new CodeParameterDeclarationExpression(context.ServiceContractGenerator.GetCodeTypeReference(typeof(IAsyncResult)), Strings.AsyncResultArgName));
                    declaringType.Members.Add(endMethod);

                    operationContext = new OperationContractGenerationContext(parent, context, operationDescription, declaringType, syncMethod, beginMethod, endMethod);
                }
                else
                {
                    operationContext = new OperationContractGenerationContext(parent, context, operationDescription, declaringType, syncMethod);
                }

                if (taskMethod)
                {
                    if (isCallback)
                    {
                        if (beginMethod == null)
                        {
                            operationContext = new OperationContractGenerationContext(parent, context, operationDescription, declaringType, syncMethod);
                        }
                        else
                        {
                            operationContext = new OperationContractGenerationContext(parent, context, operationDescription, declaringType, syncMethod, beginMethod, endMethod);
                        }
                    }
                    else
                    {
                        CodeMemberMethod taskBasedAsyncMethod = new CodeMemberMethod { Name = syncMethodName + ServiceReflector.AsyncMethodNameSuffix };
                        declaringType.Members.Add(taskBasedAsyncMethod);
                        if (beginMethod == null)
                        {
                            operationContext = new OperationContractGenerationContext(parent, context, operationDescription, declaringType, syncMethod, taskBasedAsyncMethod);
                        }
                        else
                        {
                            operationContext = new OperationContractGenerationContext(parent, context, operationDescription, declaringType, syncMethod, beginMethod, endMethod, taskBasedAsyncMethod);
                        }
                    }
                }

                operationContext.DeclaringTypeReference = operationDescription.IsServerInitiated() ? context.DuplexCallbackTypeReference : context.ContractTypeReference;

                context.Operations.Add(operationContext);

                AddOperationContractAttributes(operationContext);
            }

            void AddServiceContractAttribute(ServiceContractGenerationContext context)
            {
                CodeAttributeDeclaration serviceContractAttr = new CodeAttributeDeclaration(context.ServiceContractGenerator.GetCodeTypeReference(typeof(ServiceContractAttribute)));

                if (context.ContractType.Name != context.Contract.CodeName)
                {
                    // make sure that decoded Contract name can be used, if not, then override name with encoded value
                    // specified in wsdl; this only works beacuse our Encoding algorithm will leave alredy encoded names untouched
                    string friendlyName = NamingHelper.XmlName(context.Contract.CodeName) == context.Contract.Name ? context.Contract.CodeName : context.Contract.Name;
                    serviceContractAttr.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(friendlyName)));
                }

                if (NamingHelper.DefaultNamespace != context.Contract.Namespace)
                    serviceContractAttr.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(context.Contract.Namespace)));

                serviceContractAttr.Arguments.Add(new CodeAttributeArgument("ConfigurationName", new CodePrimitiveExpression(NamespaceHelper.GetCodeTypeReference(context.Namespace, context.ContractType).BaseType)));

                if (context.Contract.HasProtectionLevel)
                {
                    serviceContractAttr.Arguments.Add(new CodeAttributeArgument("ProtectionLevel",
                        new CodeFieldReferenceExpression(
                            new CodeTypeReferenceExpression(typeof(ProtectionLevel)), context.Contract.ProtectionLevel.ToString())));
                }

                if (context.DuplexCallbackType != null)
                {
                    serviceContractAttr.Arguments.Add(new CodeAttributeArgument("CallbackContract", new CodeTypeOfExpression(context.DuplexCallbackTypeReference)));
                }

                if (context.Contract.SessionMode != SessionMode.Allowed)
                {
                    serviceContractAttr.Arguments.Add(new CodeAttributeArgument("SessionMode",
                        new CodeFieldReferenceExpression(
                            new CodeTypeReferenceExpression(typeof(SessionMode)), context.Contract.SessionMode.ToString())));
                }

                context.ContractType.CustomAttributes.Add(serviceContractAttr);
            }

            void AddOperationContractAttributes(OperationContractGenerationContext context)
            {
                if (context.SyncMethod != null)
                {
                    context.SyncMethod.CustomAttributes.Add(CreateOperationContractAttributeDeclaration(context.Operation, false));
                }
                if (context.BeginMethod != null)
                {
                    context.BeginMethod.CustomAttributes.Add(CreateOperationContractAttributeDeclaration(context.Operation, true));
                }
                if (context.TaskMethod != null)
                {
                    context.TaskMethod.CustomAttributes.Add(CreateOperationContractAttributeDeclaration(context.Operation, false));
                }
            }

            CodeAttributeDeclaration CreateOperationContractAttributeDeclaration(OperationDescription operationDescription, bool asyncPattern)
            {
                CodeAttributeDeclaration serviceOperationAttr = new CodeAttributeDeclaration(context.ServiceContractGenerator.GetCodeTypeReference(typeof(OperationContractAttribute)));
                if (operationDescription.IsOneWay)
                {
                    serviceOperationAttr.Arguments.Add(new CodeAttributeArgument("IsOneWay", new CodePrimitiveExpression(true)));
                }
                if ((operationDescription.DeclaringContract.SessionMode == SessionMode.Required) && operationDescription.IsTerminating)
                {
                    serviceOperationAttr.Arguments.Add(new CodeAttributeArgument("IsTerminating", new CodePrimitiveExpression(true)));
                }
                if ((operationDescription.DeclaringContract.SessionMode == SessionMode.Required) && !operationDescription.IsInitiating)
                {
                    serviceOperationAttr.Arguments.Add(new CodeAttributeArgument("IsInitiating", new CodePrimitiveExpression(false)));
                }
                if (asyncPattern)
                {
                    serviceOperationAttr.Arguments.Add(new CodeAttributeArgument("AsyncPattern", new CodePrimitiveExpression(true)));
                }
                if (operationDescription.HasProtectionLevel)
                {
                    serviceOperationAttr.Arguments.Add(new CodeAttributeArgument("ProtectionLevel",
                        new CodeFieldReferenceExpression(
                            new CodeTypeReferenceExpression(typeof(ProtectionLevel)), operationDescription.ProtectionLevel.ToString())));
                }
                return serviceOperationAttr;
            }

            static bool IsDuplex(ContractDescription contract)
            {
                foreach (OperationDescription operation in contract.Operations)
                    if (operation.IsServerInitiated())
                        return true;

                return false;
            }
        }

        class ChannelInterfaceGenerator : IServiceContractGenerationExtension
        {
            void IServiceContractGenerationExtension.GenerateContract(ServiceContractGenerationContext context)
            {
                CodeTypeDeclaration channelType = context.TypeFactory.CreateInterfaceType();
                channelType.BaseTypes.Add(context.ContractTypeReference);
                channelType.BaseTypes.Add(context.ServiceContractGenerator.GetCodeTypeReference(typeof(IClientChannel)));

                new UniqueCodeNamespaceScope(context.Namespace).AddUnique(channelType, context.ContractType.Name + Strings.ChannelTypeSuffix, Strings.ChannelTypeSuffix);
            }
        }

        internal class CodeTypeFactory
        {
            ServiceContractGenerator parent;
            bool internalTypes;
            public CodeTypeFactory(ServiceContractGenerator parent, bool internalTypes)
            {
                this.parent = parent;
                this.internalTypes = internalTypes;
            }

            public CodeTypeDeclaration CreateClassType()
            {
                return CreateCodeType(false);
            }

            CodeTypeDeclaration CreateCodeType(bool isInterface)
            {
                CodeTypeDeclaration codeType = new CodeTypeDeclaration();
                codeType.IsClass = !isInterface;
                codeType.IsInterface = isInterface;

                RunDecorators(codeType);

                return codeType;
            }

            public CodeTypeDeclaration CreateInterfaceType()
            {
                return CreateCodeType(true);
            }

            void RunDecorators(CodeTypeDeclaration codeType)
            {
                AddPartial(codeType);
                AddInternal(codeType);
                AddDebuggerStepThroughAttribute(codeType);
                AddGeneratedCodeAttribute(codeType);
            }

            #region CodeTypeDeclaration decorators

            void AddDebuggerStepThroughAttribute(CodeTypeDeclaration codeType)
            {
                if (codeType.IsClass)
                {
                    codeType.CustomAttributes.Add(new CodeAttributeDeclaration(parent.GetCodeTypeReference(typeof(DebuggerStepThroughAttribute))));
                }
            }

            void AddGeneratedCodeAttribute(CodeTypeDeclaration codeType)
            {
                CodeAttributeDeclaration generatedCodeAttribute = new CodeAttributeDeclaration(parent.GetCodeTypeReference(typeof(GeneratedCodeAttribute)));

                AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
                generatedCodeAttribute.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(assemblyName.Name)));
                generatedCodeAttribute.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(assemblyName.Version.ToString())));

                codeType.CustomAttributes.Add(generatedCodeAttribute);
            }

            void AddInternal(CodeTypeDeclaration codeType)
            {
                if (internalTypes)
                {
                    codeType.TypeAttributes &= ~TypeAttributes.Public;
                }
            }

            void AddPartial(CodeTypeDeclaration codeType)
            {
                if (codeType.IsClass)
                {
                    codeType.IsPartial = true;
                }
            }
            #endregion
        }

        internal static class ExtensionsHelper
        {
            // calls the behavior extensions
            static internal void CallBehaviorExtensions(ServiceContractGenerationContext context)
            {
                CallContractExtensions(EnumerateBehaviorExtensions(context.Contract), context);

                foreach (OperationContractGenerationContext operationContext in context.Operations)
                {
                    CallOperationExtensions(EnumerateBehaviorExtensions(operationContext.Operation), operationContext);
                }
            }

            // calls a specific set of contract-level extensions
            static internal void CallContractExtensions(IEnumerable<IServiceContractGenerationExtension> extensions, ServiceContractGenerationContext context)
            {
                foreach (IServiceContractGenerationExtension extension in extensions)
                {
                    extension.GenerateContract(context);
                }
            }

            // calls a specific set of operation-level extensions on each operation in the contract
            static internal void CallOperationExtensions(IEnumerable<IOperationContractGenerationExtension> extensions, ServiceContractGenerationContext context)
            {
                foreach (OperationContractGenerationContext operationContext in context.Operations)
                {
                    CallOperationExtensions(extensions, operationContext);
                }
            }

            // calls a specific set of operation-level extensions
            static void CallOperationExtensions(IEnumerable<IOperationContractGenerationExtension> extensions, OperationContractGenerationContext context)
            {
                foreach (IOperationContractGenerationExtension extension in extensions)
                {
                    extension.GenerateOperation(context);
                }
            }

            static IEnumerable<IServiceContractGenerationExtension> EnumerateBehaviorExtensions(ContractDescription contract)
            {
                foreach (IContractBehavior behavior in contract.Behaviors)
                {
                    if (behavior is IServiceContractGenerationExtension)
                    {
                        yield return (IServiceContractGenerationExtension)behavior;
                    }
                }
            }

            static IEnumerable<IOperationContractGenerationExtension> EnumerateBehaviorExtensions(OperationDescription operation)
            {
                foreach (IOperationBehavior behavior in operation.Behaviors)
                {
                    if (behavior is IOperationContractGenerationExtension)
                    {
                        yield return (IOperationContractGenerationExtension)behavior;
                    }
                }
            }
        }

        class FaultContractAttributeGenerator : IOperationContractGenerationExtension
        {
            static CodeTypeReference voidTypeReference = new CodeTypeReference(typeof(void));

            void IOperationContractGenerationExtension.GenerateOperation(OperationContractGenerationContext context)
            {
                CodeMemberMethod methodDecl = context.SyncMethod ?? context.BeginMethod;
                foreach (FaultDescription fault in context.Operation.Faults)
                {
                    CodeAttributeDeclaration faultAttr = CreateAttrDecl(context, fault);
                    if (faultAttr != null)
                        methodDecl.CustomAttributes.Add(faultAttr);
                }
            }

            static CodeAttributeDeclaration CreateAttrDecl(OperationContractGenerationContext context, FaultDescription fault)
            {
                CodeTypeReference exceptionTypeReference = fault.DetailType != null ? context.Contract.ServiceContractGenerator.GetCodeTypeReference(fault.DetailType) : fault.DetailTypeReference;
                if (exceptionTypeReference == null || exceptionTypeReference == voidTypeReference)
                    return null;
                CodeAttributeDeclaration faultContractAttr = new CodeAttributeDeclaration(context.ServiceContractGenerator.GetCodeTypeReference(typeof(FaultContractAttribute)));
                faultContractAttr.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(exceptionTypeReference)));
                if (fault.Action != null)
                    faultContractAttr.Arguments.Add(new CodeAttributeArgument("Action", new CodePrimitiveExpression(fault.Action)));
                if (fault.HasProtectionLevel)
                {
                    faultContractAttr.Arguments.Add(new CodeAttributeArgument("ProtectionLevel",
                        new CodeFieldReferenceExpression(
                            new CodeTypeReferenceExpression(typeof(ProtectionLevel)), fault.ProtectionLevel.ToString())));
                }
                // override name with encoded value specified in wsdl; this only works beacuse
                // our Encoding algorithm will leave alredy encoded names untouched
                if (!XmlName.IsNullOrEmpty(fault.ElementName))
                    faultContractAttr.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(fault.ElementName.EncodedName)));
                if (fault.Namespace != context.Contract.Contract.Namespace)
                    faultContractAttr.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(fault.Namespace)));
                return faultContractAttr;
            }
        }

        class MessageDescriptionComparer : IEqualityComparer<MessageDescription>
        {
            static internal MessageDescriptionComparer Singleton = new MessageDescriptionComparer();
            MessageDescriptionComparer() { }

            bool IEqualityComparer<MessageDescription>.Equals(MessageDescription x, MessageDescription y)
            {
                if (x.XsdTypeName != y.XsdTypeName)
                    return false;
                // compare headers
                if (x.Headers.Count != y.Headers.Count)
                    return false;

                MessageHeaderDescription[] xHeaders = new MessageHeaderDescription[x.Headers.Count];
                x.Headers.CopyTo(xHeaders, 0);
                MessageHeaderDescription[] yHeaders = new MessageHeaderDescription[y.Headers.Count];
                y.Headers.CopyTo(yHeaders, 0);
                if (x.Headers.Count > 1)
                {
                    Array.Sort((MessagePartDescription[])xHeaders, MessagePartDescriptionComparer.Singleton);
                    Array.Sort((MessagePartDescription[])yHeaders, MessagePartDescriptionComparer.Singleton);
                }

                for (int i = 0; i < xHeaders.Length; i++)
                {
                    if (MessagePartDescriptionComparer.Singleton.Compare(xHeaders[i], yHeaders[i]) != 0)
                        return false;
                }
                return true;
            }

            int IEqualityComparer<MessageDescription>.GetHashCode(MessageDescription obj)
            {
                return obj.XsdTypeName.GetHashCode();
            }

            class MessagePartDescriptionComparer : IComparer<MessagePartDescription>
            {
                static internal MessagePartDescriptionComparer Singleton = new MessagePartDescriptionComparer();
                MessagePartDescriptionComparer() { }

                public int Compare(MessagePartDescription p1, MessagePartDescription p2)
                {
                    if (null == p1)
                    {
                        return (null == p2) ? 0 : -1;
                    }
                    if (null == p2)
                    {
                        return 1;
                    }
                    int i = String.CompareOrdinal(p1.Namespace, p2.Namespace);
                    if (i == 0)
                    {
                        i = String.CompareOrdinal(p1.Name, p2.Name);
                    }
                    return i;
                }
            }
        }

        internal class NamespaceHelper
        {
            static readonly object referenceKey = new object();

            const string WildcardNamespaceMapping = "*";

            readonly CodeNamespaceCollection codeNamespaces;
            Dictionary<string, string> namespaceMappings;

            public NamespaceHelper(CodeNamespaceCollection namespaces)
            {
                this.codeNamespaces = namespaces;
            }

            public Dictionary<string, string> NamespaceMappings
            {
                get
                {
                    if (namespaceMappings == null)
                        namespaceMappings = new Dictionary<string, string>();

                    return namespaceMappings;
                }
            }

            string DescriptionToCode(string descriptionNamespace)
            {
                string target = String.Empty;

                // use field to avoid init'ing dictionary if possible
                if (namespaceMappings != null)
                {
                    if (!namespaceMappings.TryGetValue(descriptionNamespace, out target))
                    {
                        // try to fall back to wildcard
                        if (!namespaceMappings.TryGetValue(WildcardNamespaceMapping, out target))
                        {
                            return String.Empty;
                        }
                    }
                }

                return target;
            }

            public CodeNamespace EnsureNamespace(string descriptionNamespace)
            {
                string ns = DescriptionToCode(descriptionNamespace);

                CodeNamespace codeNamespace = FindNamespace(ns);
                if (codeNamespace == null)
                {
                    codeNamespace = new CodeNamespace(ns);
                    this.codeNamespaces.Add(codeNamespace);
                }

                return codeNamespace;
            }

            CodeNamespace FindNamespace(string ns)
            {
                foreach (CodeNamespace codeNamespace in this.codeNamespaces)
                {
                    if (codeNamespace.Name == ns)
                        return codeNamespace;
                }

                return null;
            }

            public static CodeTypeDeclaration GetCodeType(CodeTypeReference codeTypeReference)
            {
                return codeTypeReference.UserData[referenceKey] as CodeTypeDeclaration;
            }

            static internal CodeTypeReference GetCodeTypeReference(CodeNamespace codeNamespace, CodeTypeDeclaration codeType)
            {
                CodeTypeReference codeTypeReference = new CodeTypeReference(String.IsNullOrEmpty(codeNamespace.Name) ? codeType.Name : codeNamespace.Name + '.' + codeType.Name);
                codeTypeReference.UserData[referenceKey] = codeType;
                return codeTypeReference;
            }
        }

        internal struct OptionsHelper
        {
            public readonly ServiceContractGenerationOptions Options;

            public OptionsHelper(ServiceContractGenerationOptions options)
            {
                this.Options = options;
            }

            public bool IsSet(ServiceContractGenerationOptions option)
            {
                Fx.Assert(IsSingleBit((int)option), "");

                return ((this.Options & option) != ServiceContractGenerationOptions.None);
            }

            static bool IsSingleBit(int x)
            {
                //figures out if the mode has a single bit set ( is a power of 2)
                return (x != 0) && ((x & (x + ~0)) == 0);
            }
        }

        static class Strings
        {
            public const string AsyncCallbackArgName = "callback";
            public const string AsyncStateArgName = "asyncState";
            public const string AsyncResultArgName = "result";

            public const string CallbackTypeSuffix = "Callback";
            public const string ChannelTypeSuffix = "Channel";

            public const string DefaultContractName = "IContract";
            public const string DefaultOperationName = "Method";

            public const string InterfaceTypePrefix = "I";
        }

        // ideally this one would appear on TransactionFlowAttribute
        class TransactionFlowAttributeGenerator : IOperationContractGenerationExtension
        {
            void IOperationContractGenerationExtension.GenerateOperation(OperationContractGenerationContext context)
            {
                System.ServiceModel.TransactionFlowAttribute attr = context.Operation.Behaviors.Find<System.ServiceModel.TransactionFlowAttribute>();
                if (attr != null && attr.Transactions != TransactionFlowOption.NotAllowed)
                {
                    CodeMemberMethod methodDecl = context.SyncMethod ?? context.BeginMethod;
                    methodDecl.CustomAttributes.Add(CreateAttrDecl(context, attr));

                }
            }

            static CodeAttributeDeclaration CreateAttrDecl(OperationContractGenerationContext context, TransactionFlowAttribute attr)
            {
                CodeAttributeDeclaration attrDecl = new CodeAttributeDeclaration(context.Contract.ServiceContractGenerator.GetCodeTypeReference(typeof(TransactionFlowAttribute)));
                attrDecl.Arguments.Add(new CodeAttributeArgument(ServiceContractGenerator.GetEnumReference<TransactionFlowOption>(attr.Transactions)));
                return attrDecl;
            }
        }
    }
}
