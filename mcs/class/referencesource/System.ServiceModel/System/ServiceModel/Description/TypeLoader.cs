//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    class TypeLoader
    {
        static Type[] messageContractMemberAttributes = {
            typeof(MessageHeaderAttribute),
            typeof(MessageBodyMemberAttribute), 
            typeof(MessagePropertyAttribute),
        };

        static Type[] formatterAttributes = {
            typeof(XmlSerializerFormatAttribute),
            typeof(DataContractFormatAttribute)
        };

        static Type[] knownTypesMethodParamType = new Type[] { typeof(ICustomAttributeProvider) };

        internal static DataContractFormatAttribute DefaultDataContractFormatAttribute = new DataContractFormatAttribute();
        internal static XmlSerializerFormatAttribute DefaultXmlSerializerFormatAttribute = new XmlSerializerFormatAttribute();

        static readonly Type OperationContractAttributeType = typeof(OperationContractAttribute);

        internal const string ReturnSuffix = "Result";
        internal const string ResponseSuffix = "Response";
        internal const string FaultSuffix = "Fault";
        internal const BindingFlags DefaultBindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

        readonly object thisLock;
        readonly Dictionary<Type, ContractDescription> contracts;
        readonly Dictionary<Type, MessageDescriptionItems> messages;

        public TypeLoader()
        {
            this.thisLock = new object();
            this.contracts = new Dictionary<Type, ContractDescription>();
            this.messages = new Dictionary<Type, MessageDescriptionItems>();
        }

        [SuppressMessage(FxCop.Category.Usage, "CA2301:EmbeddableTypesInContainersRule", MessageId = "contracts", Justification = "No need to support type equivalence here.")]
        ContractDescription LoadContractDescriptionHelper(Type contractType, Type serviceType, object serviceImplementation)
        {
            ContractDescription contractDescription;
            if (contractType == typeof(IOutputChannel))
            {
                contractDescription = LoadOutputChannelContractDescription();
            }
            else if (contractType == typeof(IRequestChannel))
            {
                contractDescription = LoadRequestChannelContractDescription();
            }
            else
            {
                ServiceContractAttribute actualContractAttribute;
                Type actualContractType = ServiceReflector.GetContractTypeAndAttribute(contractType, out actualContractAttribute);
                lock (this.thisLock)
                {
                    if (!contracts.TryGetValue(actualContractType, out contractDescription))
                    {
                        EnsureNoInheritanceWithContractClasses(actualContractType);
                        EnsureNoOperationContractsOnNonServiceContractTypes(actualContractType);
                        ContractReflectionInfo reflectionInfo;
                        contractDescription = CreateContractDescription(actualContractAttribute, actualContractType, serviceType, out reflectionInfo, serviceImplementation);
                        // IContractBehaviors
                        if (serviceImplementation != null && serviceImplementation is IContractBehavior)
                        {
                            contractDescription.Behaviors.Add((IContractBehavior)serviceImplementation);
                        }
                        if (serviceType != null)
                        {
                            UpdateContractDescriptionWithAttributesFromServiceType(contractDescription, serviceType);
                            foreach (ContractDescription inheritedContract in contractDescription.GetInheritedContracts())
                            {
                                UpdateContractDescriptionWithAttributesFromServiceType(inheritedContract, serviceType);
                            }
                        }
                        UpdateOperationsWithInterfaceAttributes(contractDescription, reflectionInfo);
                        AddBehaviors(contractDescription, serviceType, false, reflectionInfo);

                        this.contracts.Add(actualContractType, contractDescription);
                    }
                }
            }
            return contractDescription;
        }

        void EnsureNoInheritanceWithContractClasses(Type actualContractType)
        {
            if (actualContractType.IsClass)
            {
                // we only need to check base _classes_ here, the check for interfaces happens elsewhere
                for (Type service = actualContractType.BaseType; service != null; service = service.BaseType)
                {
                    if (ServiceReflector.GetSingleAttribute<ServiceContractAttribute>(service) != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.SFxContractInheritanceRequiresInterfaces, actualContractType, service)));
                    }
                }
            }
        }

        void EnsureNoOperationContractsOnNonServiceContractTypes(Type actualContractType)
        {
            foreach (Type t in actualContractType.GetInterfaces())
            {
                EnsureNoOperationContractsOnNonServiceContractTypes_Helper(t);
            }
            for (Type u = actualContractType.BaseType; u != null; u = u.BaseType)
            {
                EnsureNoOperationContractsOnNonServiceContractTypes_Helper(u);
            }
        }

        void EnsureNoOperationContractsOnNonServiceContractTypes_Helper(Type aParentType)
        {
            // if not [ServiceContract]
            if (ServiceReflector.GetSingleAttribute<ServiceContractAttribute>(aParentType) == null)
            {
                foreach (MethodInfo methodInfo in aParentType.GetMethods(DefaultBindingFlags))
                {
                    // but does have an OperationContractAttribute
                    Type operationContractProviderType = ServiceReflector.GetOperationContractProviderType(methodInfo);
                    if (operationContractProviderType != null)
                    {
                        if (operationContractProviderType == OperationContractAttributeType)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                                SR.SFxOperationContractOnNonServiceContract, methodInfo.Name, aParentType.Name)));
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                                SR.SFxOperationContractProviderOnNonServiceContract, operationContractProviderType.Name, methodInfo.Name, aParentType.Name)));
                        }
                    }
                }
            }
        }

        public ContractDescription LoadContractDescription(Type contractType)
        {
            Fx.Assert(contractType != null, "");

            return LoadContractDescriptionHelper(contractType, null, null);
        }

        public ContractDescription LoadContractDescription(Type contractType, Type serviceType)
        {
            Fx.Assert(contractType != null, "");
            Fx.Assert(serviceType != null, "");

            return LoadContractDescriptionHelper(contractType, serviceType, null);
        }

        public ContractDescription LoadContractDescription(Type contractType, Type serviceType, object serviceImplementation)
        {
            Fx.Assert(contractType != null, "");
            Fx.Assert(serviceType != null, "");
            Fx.Assert(serviceImplementation != null, "");

            return LoadContractDescriptionHelper(contractType, serviceType, serviceImplementation);
        }

        ContractDescription LoadOutputChannelContractDescription()
        {
            Type channelType = typeof(IOutputChannel);
            XmlQualifiedName contractName = NamingHelper.GetContractName(channelType, null, NamingHelper.MSNamespace);
            ContractDescription contract = new ContractDescription(contractName.Name, contractName.Namespace);
            contract.ContractType = channelType;
            contract.ConfigurationName = channelType.FullName;
            contract.SessionMode = SessionMode.NotAllowed;
            OperationDescription operation = new OperationDescription("Send", contract);
            MessageDescription message = new MessageDescription(MessageHeaders.WildcardAction, MessageDirection.Input);
            operation.Messages.Add(message);
            contract.Operations.Add(operation);
            return contract;
        }

        ContractDescription LoadRequestChannelContractDescription()
        {
            Type channelType = typeof(IRequestChannel);
            XmlQualifiedName contractName = NamingHelper.GetContractName(channelType, null, NamingHelper.MSNamespace);
            ContractDescription contract = new ContractDescription(contractName.Name, contractName.Namespace);
            contract.ContractType = channelType;
            contract.ConfigurationName = channelType.FullName;
            contract.SessionMode = SessionMode.NotAllowed;
            OperationDescription operation = new OperationDescription("Request", contract);
            MessageDescription request = new MessageDescription(MessageHeaders.WildcardAction, MessageDirection.Input);
            MessageDescription reply = new MessageDescription(MessageHeaders.WildcardAction, MessageDirection.Output);
            operation.Messages.Add(request);
            operation.Messages.Add(reply);
            contract.Operations.Add(operation);
            return contract;
        }

        void AddBehaviors(ContractDescription contractDesc, Type implType, bool implIsCallback, ContractReflectionInfo reflectionInfo)
        {
            ServiceContractAttribute contractAttr = ServiceReflector.GetRequiredSingleAttribute<ServiceContractAttribute>(reflectionInfo.iface);
            for (int i = 0; i < contractDesc.Operations.Count; i++)
            {
                OperationDescription operationDescription = contractDesc.Operations[i];
                bool isInherited = operationDescription.DeclaringContract != contractDesc;
                if (!isInherited)
                {
                    operationDescription.Behaviors.Add(new OperationInvokerBehavior());
                }
            }
            contractDesc.Behaviors.Add(new OperationSelectorBehavior());

            for (int i = 0; i < contractDesc.Operations.Count; i++)
            {
                OperationDescription opDesc = contractDesc.Operations[i];
                bool isInherited = opDesc.DeclaringContract != contractDesc;
                Type targetIface = implIsCallback ? opDesc.DeclaringContract.CallbackContractType : opDesc.DeclaringContract.ContractType;

                if (implType == null && !isInherited)
                {
                    KeyedByTypeCollection<IOperationBehavior> toAdd =
                        GetIOperationBehaviorAttributesFromType(opDesc, targetIface, null);
                    for (int j = 0; j < toAdd.Count; j++)
                    {
                        opDesc.Behaviors.Add(toAdd[j]);
                    }
                }
                else
                {
                    // look for IOperationBehaviors on implementation methods in service class hierarchy
                    ApplyServiceInheritance<IOperationBehavior, KeyedByTypeCollection<IOperationBehavior>>(
                        implType, opDesc.Behaviors,
                        delegate(Type currentType, KeyedByTypeCollection<IOperationBehavior> behaviors)
                        {
                            KeyedByTypeCollection<IOperationBehavior> toAdd =
                                GetIOperationBehaviorAttributesFromType(opDesc, targetIface, currentType);
                            for (int j = 0; j < toAdd.Count; j++)
                            {
                                behaviors.Add(toAdd[j]);
                            }
                        });
                    // then look for IOperationBehaviors on interface type
                    if (!isInherited)
                    {
                        AddBehaviorsAtOneScope<IOperationBehavior, KeyedByTypeCollection<IOperationBehavior>>(
                            targetIface, opDesc.Behaviors,
                            delegate(Type currentType, KeyedByTypeCollection<IOperationBehavior> behaviors)
                            {
                                KeyedByTypeCollection<IOperationBehavior> toAdd =
                                    GetIOperationBehaviorAttributesFromType(opDesc, targetIface, null);
                                for (int j = 0; j < toAdd.Count; j++)
                                {
                                    behaviors.Add(toAdd[j]);
                                }
                            });
                    }
                }
            }

            for (int i = 0; i < contractDesc.Operations.Count; i++)
            {
                OperationDescription opDesc = contractDesc.Operations[i];
                OperationBehaviorAttribute operationBehavior = opDesc.Behaviors.Find<OperationBehaviorAttribute>();
                if (operationBehavior == null)
                {
                    operationBehavior = new OperationBehaviorAttribute();
                    opDesc.Behaviors.Add(operationBehavior);
                }
            }

            Type targetInterface = implIsCallback ? reflectionInfo.callbackiface : reflectionInfo.iface;
            AddBehaviorsAtOneScope<IContractBehavior, KeyedByTypeCollection<IContractBehavior>>(targetInterface, contractDesc.Behaviors,
                GetIContractBehaviorsFromInterfaceType);

            bool hasXmlSerializerMethod = false;
            for (int i = 0; i < contractDesc.Operations.Count; i++)
            {
                OperationDescription operationDescription = contractDesc.Operations[i];
                bool isInherited = operationDescription.DeclaringContract != contractDesc;
                MethodInfo opMethod = operationDescription.OperationMethod;
                Attribute formattingAttribute = GetFormattingAttribute(opMethod,
                                                    GetFormattingAttribute(operationDescription.DeclaringContract.ContractType,
                                                        DefaultDataContractFormatAttribute));
                DataContractFormatAttribute dataContractFormatAttribute = formattingAttribute as DataContractFormatAttribute;
                if (dataContractFormatAttribute != null)
                {
                    if (!isInherited)
                    {
                        operationDescription.Behaviors.Add(new DataContractSerializerOperationBehavior(operationDescription, dataContractFormatAttribute, true));
                        operationDescription.Behaviors.Add(new DataContractSerializerOperationGenerator());
                    }
                }
                else if (formattingAttribute != null && formattingAttribute is XmlSerializerFormatAttribute)
                {
                    hasXmlSerializerMethod = true;
                }
            }
            if (hasXmlSerializerMethod)
            {
                XmlSerializerOperationBehavior.AddBuiltInBehaviors(contractDesc);
            }
        }

        void GetIContractBehaviorsFromInterfaceType(Type interfaceType, KeyedByTypeCollection<IContractBehavior> behaviors)
        {
            object[] ifaceAttributes = ServiceReflector.GetCustomAttributes(interfaceType, typeof(IContractBehavior), false);
            for (int i = 0; i < ifaceAttributes.Length; i++)
            {
                IContractBehavior behavior = (IContractBehavior)ifaceAttributes[i];
                behaviors.Add(behavior);
            }
        }

        static void UpdateContractDescriptionWithAttributesFromServiceType(ContractDescription description, Type serviceType)
        {
            ApplyServiceInheritance<IContractBehavior, KeyedByTypeCollection<IContractBehavior>>(
                serviceType, description.Behaviors,
                delegate(Type currentType, KeyedByTypeCollection<IContractBehavior> behaviors)
                {

                    foreach (IContractBehavior iContractBehavior in ServiceReflector.GetCustomAttributes(currentType, typeof(IContractBehavior), false))
                    {
                        IContractBehaviorAttribute iContractBehaviorAttribute = iContractBehavior as IContractBehaviorAttribute;
                        if (iContractBehaviorAttribute == null
                            || (iContractBehaviorAttribute.TargetContract == null)
                            || (iContractBehaviorAttribute.TargetContract == description.ContractType))
                        {
                            behaviors.Add(iContractBehavior);
                        }
                    }
                });
        }

        void UpdateOperationsWithInterfaceAttributes(ContractDescription contractDesc, ContractReflectionInfo reflectionInfo)
        {
            object[] customAttributes = ServiceReflector.GetCustomAttributes(reflectionInfo.iface, typeof(ServiceKnownTypeAttribute), false);
            IEnumerable<Type> knownTypes = GetKnownTypes(customAttributes, reflectionInfo.iface);
            foreach (Type knownType in knownTypes)
            {
                foreach (OperationDescription operationDescription in contractDesc.Operations)
                {
                    if (!operationDescription.IsServerInitiated())
                        operationDescription.KnownTypes.Add(knownType);
                }
            }

            if (reflectionInfo.callbackiface != null)
            {
                customAttributes = ServiceReflector.GetCustomAttributes(reflectionInfo.callbackiface, typeof(ServiceKnownTypeAttribute), false);
                knownTypes = GetKnownTypes(customAttributes, reflectionInfo.callbackiface);
                foreach (Type knownType in knownTypes)
                {
                    foreach (OperationDescription operationDescription in contractDesc.Operations)
                    {
                        if (operationDescription.IsServerInitiated())
                            operationDescription.KnownTypes.Add(knownType);
                    }
                }
            }
        }

        private IEnumerable<Type> GetKnownTypes(object[] knownTypeAttributes, ICustomAttributeProvider provider)
        {
            if (knownTypeAttributes.Length == 1)
            {
                ServiceKnownTypeAttribute knownTypeAttribute = (ServiceKnownTypeAttribute)knownTypeAttributes[0];
                if (!string.IsNullOrEmpty(knownTypeAttribute.MethodName))
                {
                    Type type = knownTypeAttribute.DeclaringType;
                    if (type == null)
                    {
                        type = provider as Type;
                        if (type == null)
                            type = ((MethodInfo)provider).DeclaringType;
                    }
                    MethodInfo method = type.GetMethod(knownTypeAttribute.MethodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, knownTypesMethodParamType, null);
                    if (method == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxKnownTypeAttributeUnknownMethod3, provider, knownTypeAttribute.MethodName, type.FullName)));

                    if (!typeof(IEnumerable<Type>).IsAssignableFrom(method.ReturnType))
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxKnownTypeAttributeReturnType3, provider, knownTypeAttribute.MethodName, type.FullName)));

                    return (IEnumerable<Type>)method.Invoke(null, new object[] { provider });
                }
            }

            List<Type> knownTypes = new List<Type>();
            for (int i = 0; i < knownTypeAttributes.Length; ++i)
            {
                ServiceKnownTypeAttribute knownTypeAttribute = (ServiceKnownTypeAttribute)knownTypeAttributes[i];
                if (knownTypeAttribute.Type == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxKnownTypeAttributeInvalid1, provider.ToString())));
                knownTypes.Add(knownTypeAttribute.Type);
            }
            return knownTypes;
        }

        KeyedByTypeCollection<IOperationBehavior> GetIOperationBehaviorAttributesFromType(OperationDescription opDesc, Type targetIface, Type implType)
        {
            KeyedByTypeCollection<IOperationBehavior> result = new KeyedByTypeCollection<IOperationBehavior>();
            InterfaceMapping ifaceMap = default(InterfaceMapping);
            bool useImplAttrs = false;
            if (implType != null)
            {
                if (targetIface.IsAssignableFrom(implType) && targetIface.IsInterface)
                {
                    ifaceMap = implType.GetInterfaceMap(targetIface);
                    useImplAttrs = true;
                }
                else
                {
                    // implType does not implement any methods from the targetIface, so there is nothing to do
                    return result;
                }
            }
            MethodInfo opMethod = opDesc.OperationMethod;
            ProcessOpMethod(opMethod, true, opDesc, result, ifaceMap, useImplAttrs);
            if (opDesc.SyncMethod != null && opDesc.BeginMethod != null)
            {
                ProcessOpMethod(opDesc.BeginMethod, false, opDesc, result, ifaceMap, useImplAttrs);
            }
            else if (opDesc.SyncMethod != null && opDesc.TaskMethod != null)
            {
                ProcessOpMethod(opDesc.TaskMethod, false, opDesc, result, ifaceMap, useImplAttrs);
            }
            else if (opDesc.TaskMethod != null && opDesc.BeginMethod != null)
            {
                ProcessOpMethod(opDesc.BeginMethod, false, opDesc, result, ifaceMap, useImplAttrs);
            }
            return result;
        }

        void ProcessOpMethod(MethodInfo opMethod, bool canHaveBehaviors,
                             OperationDescription opDesc, KeyedByTypeCollection<IOperationBehavior> result,
                             InterfaceMapping ifaceMap, bool useImplAttrs)
        {
            MethodInfo method = null;
            if (useImplAttrs)
            {
                int methodIndex = Array.IndexOf(ifaceMap.InterfaceMethods, opMethod);
                // if opMethod doesn't exist in the interfacemap, it means opMethod was on
                // the "other" interface (not the one implemented by implType)
                if (methodIndex != -1)
                {
                    MethodInfo implMethod = ifaceMap.TargetMethods[methodIndex];
                    // C++ allows you to create abstract classes that have missing interface method
                    // implementations, which shows up as nulls in the interfacemapping
                    if (implMethod != null)
                    {
                        method = implMethod;
                    }
                }
                if (method == null)
                {
                    return;
                }
            }
            else
            {
                method = opMethod;
            }

            object[] methodAttributes = ServiceReflector.GetCustomAttributes(method, typeof(IOperationBehavior), false);
            for (int k = 0; k < methodAttributes.Length; k++)
            {
                IOperationBehavior opBehaviorAttr = (IOperationBehavior)methodAttributes[k];
                if (canHaveBehaviors)
                {
                    result.Add(opBehaviorAttr);
                }
                else
                {
                    if (opDesc.SyncMethod != null && opDesc.BeginMethod != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.SyncAsyncMatchConsistency_Attributes6,
                                                                       opDesc.SyncMethod.Name,
                                                                       opDesc.SyncMethod.DeclaringType,
                                                                       opDesc.BeginMethod.Name,
                                                                       opDesc.EndMethod.Name,
                                                                       opDesc.Name,
                                                                       opBehaviorAttr.GetType().FullName)));
                    }
                    else if (opDesc.SyncMethod != null && opDesc.TaskMethod != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.SyncTaskMatchConsistency_Attributes6,
                                                                       opDesc.SyncMethod.Name,
                                                                       opDesc.SyncMethod.DeclaringType,
                                                                       opDesc.TaskMethod.Name,
                                                                       opDesc.Name,
                                                                       opBehaviorAttr.GetType().FullName)));
                    }
                    else if (opDesc.TaskMethod != null && opDesc.BeginMethod != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.TaskAsyncMatchConsistency_Attributes6,
                                                                       opDesc.TaskMethod.Name,
                                                                       opDesc.TaskMethod.DeclaringType,
                                                                       opDesc.BeginMethod.Name,
                                                                       opDesc.EndMethod.Name,
                                                                       opDesc.Name,
                                                                       opBehaviorAttr.GetType().FullName)));
                    }
                    Fx.Assert("Invalid state. No exception for canHaveBehaviors = false");
                }
            }
        }

        internal void AddBehaviorsSFx(ServiceEndpoint serviceEndpoint, Type contractType)
        {
            if (serviceEndpoint.Contract.IsDuplex())
            {
                CallbackBehaviorAttribute attr = serviceEndpoint.Behaviors.Find<CallbackBehaviorAttribute>();
                if (attr == null)
                {
                    serviceEndpoint.Behaviors.Insert(0, new CallbackBehaviorAttribute());
                }
            }
        }

        internal void AddBehaviorsFromImplementationType(ServiceEndpoint serviceEndpoint, Type implementationType)
        {
            foreach (IEndpointBehavior behaviorAttribute in ServiceReflector.GetCustomAttributes(implementationType, typeof(IEndpointBehavior), false))
            {
                if (behaviorAttribute is CallbackBehaviorAttribute)
                {
                    serviceEndpoint.Behaviors.Insert(0, behaviorAttribute);
                }
                else
                {
                    serviceEndpoint.Behaviors.Add(behaviorAttribute);
                }
            }
            foreach (IContractBehavior behaviorAttribute in ServiceReflector.GetCustomAttributes(implementationType, typeof(IContractBehavior), false))
            {
                serviceEndpoint.Contract.Behaviors.Add(behaviorAttribute);
            }
            Type targetIface = serviceEndpoint.Contract.CallbackContractType;
            for (int i = 0; i < serviceEndpoint.Contract.Operations.Count; i++)
            {
                OperationDescription opDesc = serviceEndpoint.Contract.Operations[i];
                KeyedByTypeCollection<IOperationBehavior> opBehaviors = new KeyedByTypeCollection<IOperationBehavior>();
                // look for IOperationBehaviors on implementation methods in callback class hierarchy
                ApplyServiceInheritance<IOperationBehavior, KeyedByTypeCollection<IOperationBehavior>>(
                    implementationType, opBehaviors,
                    delegate(Type currentType, KeyedByTypeCollection<IOperationBehavior> behaviors)
                    {
                        KeyedByTypeCollection<IOperationBehavior> toAdd =
                            GetIOperationBehaviorAttributesFromType(opDesc, targetIface, currentType);
                        for (int j = 0; j < toAdd.Count; j++)
                        {
                            behaviors.Add(toAdd[j]);
                        }
                    });
                // a bunch of default IOperationBehaviors have already been added, which we may need to replace
                for (int k = 0; k < opBehaviors.Count; k++)
                {
                    IOperationBehavior behavior = opBehaviors[k];
                    Type t = behavior.GetType();
                    if (opDesc.Behaviors.Contains(t))
                    {
                        opDesc.Behaviors.Remove(t);
                    }
                    opDesc.Behaviors.Add(behavior);
                }
            }
        }

        internal static int CompareMessagePartDescriptions(MessagePartDescription a, MessagePartDescription b)
        {
            int posCmp = a.SerializationPosition - b.SerializationPosition;
            if (posCmp != 0)
            {
                return posCmp;
            }

            int nsCmp = string.Compare(a.Namespace, b.Namespace, StringComparison.Ordinal);
            if (nsCmp != 0)
            {
                return nsCmp;
            }

            return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        }

        internal static XmlName GetBodyWrapperResponseName(string operationName)
        {
#if DEBUG
            Fx.Assert(NamingHelper.IsValidNCName(operationName), "operationName value has to be a valid NCName.");
#endif
            return new XmlName(operationName + ResponseSuffix);
        }

        internal static XmlName GetBodyWrapperResponseName(XmlName operationName)
        {
            return new XmlName(operationName.EncodedName + ResponseSuffix, true /*isEncoded*/);
        }

        void CreateOperationDescriptions(ContractDescription contractDescription,
                                         ContractReflectionInfo reflectionInfo,
                                         Type contractToGetMethodsFrom,
                                         ContractDescription declaringContract,
                                         MessageDirection direction
                                         )
        {
            MessageDirection otherDirection = MessageDirectionHelper.Opposite(direction);
            if (!(declaringContract.ContractType.IsAssignableFrom(contractDescription.ContractType)))
            {
                Fx.Assert("bad contract inheritance");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    String.Format(CultureInfo.InvariantCulture, "Bad contract inheritence. Contract {0} does not implement {1}", declaringContract.ContractType.Name, contractDescription.ContractType.Name)
                    ));
            }

            foreach (MethodInfo methodInfo in contractToGetMethodsFrom.GetMethods(DefaultBindingFlags))
            {
                if (contractToGetMethodsFrom.IsInterface)
                {
                    object[] attrs = ServiceReflector.GetCustomAttributes(methodInfo, typeof(OperationBehaviorAttribute), false);
                    if (attrs.Length != 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.SFxOperationBehaviorAttributeOnlyOnServiceClass, methodInfo.Name, contractToGetMethodsFrom.Name)));
                    }
                }
                ServiceReflector.ValidateParameterMetadata(methodInfo);
                OperationDescription operation = CreateOperationDescription(contractDescription, methodInfo, direction, reflectionInfo, declaringContract);
                if (operation != null)
                {
                    contractDescription.Operations.Add(operation);
                }
            }
        }

        //Checks whether that the Callback contract provided on a ServiceContract follows rules
        //1. It has to be a interface
        //2. If its a class then it needs to implement MarshallByRefObject
        internal static void EnsureCallbackType(Type callbackType)
        {
            if (callbackType != null && !callbackType.IsInterface && !callbackType.IsMarshalByRef)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SFxInvalidCallbackContractType, callbackType.Name)));
            }
        }

        // checks a contract for substitutability (in the Liskov Substitution Principle sense), throws on error
        internal static void EnsureSubcontract(ServiceContractAttribute svcContractAttr, Type contractType)
        {
            Type callbackType = svcContractAttr.CallbackContract;

            List<Type> types = ServiceReflector.GetInheritedContractTypes(contractType);
            for (int i = 0; i < types.Count; i++)
            {
                Type inheritedContractType = types[i];
                ServiceContractAttribute inheritedContractAttr = ServiceReflector.GetRequiredSingleAttribute<ServiceContractAttribute>(inheritedContractType);
                // we must be covariant in our callbacks
                if (inheritedContractAttr.CallbackContract != null)
                {
                    if (callbackType == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.InAContractInheritanceHierarchyIfParentHasCallbackChildMustToo,
                            inheritedContractType.Name, inheritedContractAttr.CallbackContract.Name, contractType.Name)));
                    }
                    if (!inheritedContractAttr.CallbackContract.IsAssignableFrom(callbackType))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.InAContractInheritanceHierarchyTheServiceContract3_2,
                            inheritedContractType.Name, contractType.Name)));
                    }
                }
            }
        }

        ContractDescription CreateContractDescription(ServiceContractAttribute contractAttr, Type contractType, Type serviceType, out ContractReflectionInfo reflectionInfo, object serviceImplementation)
        {
            reflectionInfo = new ContractReflectionInfo();

            XmlQualifiedName contractName = NamingHelper.GetContractName(contractType, contractAttr.Name, contractAttr.Namespace);
            ContractDescription contractDescription = new ContractDescription(contractName.Name, contractName.Namespace);
            contractDescription.ContractType = contractType;
            if (contractAttr.HasProtectionLevel)
            {
                contractDescription.ProtectionLevel = contractAttr.ProtectionLevel;
            }

            Type callbackType = contractAttr.CallbackContract;

            EnsureCallbackType(callbackType);

            EnsureSubcontract(contractAttr, contractType);

            // reflect the methods in contractType and add OperationDescriptions to ContractDescription
            reflectionInfo.iface = contractType;
            reflectionInfo.callbackiface = callbackType;

            contractDescription.SessionMode = contractAttr.SessionMode;
            contractDescription.CallbackContractType = callbackType;
            contractDescription.ConfigurationName = contractAttr.ConfigurationName ?? contractType.FullName;

            // get inherited operations
            List<Type> types = ServiceReflector.GetInheritedContractTypes(contractType);
            List<Type> inheritedCallbackTypes = new List<Type>();
            for (int i = 0; i < types.Count; i++)
            {
                Type inheritedContractType = types[i];
                ServiceContractAttribute inheritedContractAttr = ServiceReflector.GetRequiredSingleAttribute<ServiceContractAttribute>(inheritedContractType);
                ContractDescription inheritedContractDescription = LoadContractDescriptionHelper(inheritedContractType, serviceType, serviceImplementation);
                foreach (OperationDescription op in inheritedContractDescription.Operations)
                {
                    if (!contractDescription.Operations.Contains(op)) // in a diamond hierarchy, ensure we don't add same op twice from two different parents
                    {
                        // ensure two different parents don't try to add conflicting operations
                        Collection<OperationDescription> existingOps = contractDescription.Operations.FindAll(op.Name);
                        foreach (OperationDescription existingOp in existingOps)
                        {
                            if (existingOp.Messages[0].Direction == op.Messages[0].Direction)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                    SR.GetString(SR.CannotInheritTwoOperationsWithTheSameName3,
                                        op.Name, inheritedContractDescription.Name, existingOp.DeclaringContract.Name)));
                            }
                        }
                        contractDescription.Operations.Add(op);
                    }
                }
                if (inheritedContractDescription.CallbackContractType != null)
                {
                    inheritedCallbackTypes.Add(inheritedContractDescription.CallbackContractType);
                }
            }

            // this contract 
            CreateOperationDescriptions(contractDescription, reflectionInfo, contractType, contractDescription, MessageDirection.Input);
            // CallbackContract 
            if (callbackType != null && !inheritedCallbackTypes.Contains(callbackType))
            {
                CreateOperationDescriptions(contractDescription, reflectionInfo, callbackType, contractDescription, MessageDirection.Output);
            }

            return contractDescription;
        }

        internal static Attribute GetFormattingAttribute(ICustomAttributeProvider attrProvider, Attribute defaultFormatAttribute)
        {
            if (attrProvider != null)
            {
                if (attrProvider.IsDefined(typeof(XmlSerializerFormatAttribute), false))
                {
                    return ServiceReflector.GetSingleAttribute<XmlSerializerFormatAttribute>(attrProvider, formatterAttributes);
                }
                if (attrProvider.IsDefined(typeof(DataContractFormatAttribute), false))
                {
                    return ServiceReflector.GetSingleAttribute<DataContractFormatAttribute>(attrProvider, formatterAttributes);
                }
            }
            return defaultFormatAttribute;
        }

        //[....] and Async should follow the rules:
        //    1. Parameter match
        //    2. Async cannot have behaviors (verification happens later in ProcessOpMethod - behaviors haven't yet been loaded here)
        //    3. Async cannot have known types
        //    4. Async cannot have known faults
        //    5. [....] and Async have to match on OneWay status
        //    6. [....] and Async have to match Action and ReplyAction
        void VerifyConsistency(OperationConsistencyVerifier verifier)
        {
            verifier.VerifyParameterLength();
            verifier.VerifyParameterType();
            verifier.VerifyOutParameterType();
            verifier.VerifyReturnType();
            verifier.VerifyFaultContractAttribute();
            verifier.VerifyKnownTypeAttribute();
            verifier.VerifyIsOneWayStatus();
            verifier.VerifyActionAndReplyAction();
        }

        // "direction" is the "direction of the interface" (from the perspective of the server, as usual):
        //    proxy interface on client: MessageDirection.Input
        //    callback interface on client: MessageDirection.Output
        //    service interface (or class) on server: MessageDirection.Input
        //    callback interface on server: MessageDirection.Output
        OperationDescription CreateOperationDescription(ContractDescription contractDescription, MethodInfo methodInfo, MessageDirection direction,
                                                        ContractReflectionInfo reflectionInfo, ContractDescription declaringContract)
        {
            OperationContractAttribute opAttr = ServiceReflector.GetOperationContractAttribute(methodInfo);
            if (opAttr == null)
            {
                return null;
            }

            if (ServiceReflector.HasEndMethodShape(methodInfo))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.EndMethodsCannotBeDecoratedWithOperationContractAttribute,
                                 methodInfo.Name, reflectionInfo.iface)));
            }

            Type taskTResult;
            bool isTask = ServiceReflector.IsTask(methodInfo, out taskTResult);
            bool isAsync = isTask ? false : ServiceReflector.IsBegin(opAttr, methodInfo);

            XmlName operationName = NamingHelper.GetOperationName(ServiceReflector.GetLogicalName(methodInfo, isAsync, isTask), opAttr.Name);

            opAttr.EnsureInvariants(methodInfo, operationName.EncodedName);

            Collection<OperationDescription> operations = contractDescription.Operations.FindAll(operationName.EncodedName);
            for (int i = 0; i < operations.Count; i++)
            {
                OperationDescription existingOp = operations[i];
                if (existingOp.Messages[0].Direction == direction)
                {
                    // if we have already seen a task-based method with the same name, we need to throw.
                    if (existingOp.TaskMethod != null && isTask)
                    {
                        string method1Name = existingOp.OperationMethod.Name;
                        string method2Name = methodInfo.Name;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotHaveTwoOperationsWithTheSameName3, method1Name, method2Name, reflectionInfo.iface)));
                    }
                    if (isAsync && (existingOp.BeginMethod != null))
                    {
                        string method1Name = existingOp.BeginMethod.Name;
                        string method2Name = methodInfo.Name;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotHaveTwoOperationsWithTheSameName3, method1Name, method2Name, reflectionInfo.iface)));
                    }
                    if (!isAsync && !isTask && (existingOp.SyncMethod != null))
                    {
                        string method1Name = existingOp.SyncMethod.Name;
                        string method2Name = methodInfo.Name;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotHaveTwoOperationsWithTheSameName3, method1Name, method2Name, reflectionInfo.iface)));
                    }

                    contractDescription.Operations.Remove(existingOp);
                    OperationDescription newOp = CreateOperationDescription(contractDescription,
                                                                            methodInfo,
                                                                            direction,
                                                                            reflectionInfo,
                                                                            declaringContract);

                    newOp.HasNoDisposableParameters = ServiceReflector.HasNoDisposableParameters(methodInfo);

                    if (isTask)
                    {
                        existingOp.TaskMethod = newOp.TaskMethod;
                        existingOp.TaskTResult = newOp.TaskTResult;
                        if (existingOp.SyncMethod != null)
                        {
                            // Task vs. [....] 
                            VerifyConsistency(new SyncTaskOperationConsistencyVerifier(existingOp, newOp));
                        }
                        else
                        {
                            // Task vs. Async
                            VerifyConsistency(new TaskAsyncOperationConsistencyVerifier(newOp, existingOp));
                        }
                        return existingOp;
                    }
                    else if (isAsync)
                    {
                        existingOp.BeginMethod = newOp.BeginMethod;
                        existingOp.EndMethod = newOp.EndMethod;
                        if (existingOp.SyncMethod != null)
                        {
                            // Async vs. [....]
                            VerifyConsistency(new SyncAsyncOperationConsistencyVerifier(existingOp, newOp));
                        }
                        else
                        {
                            // Async vs. Task
                            VerifyConsistency(new TaskAsyncOperationConsistencyVerifier(existingOp, newOp));
                        }
                        return existingOp;
                    }
                    else
                    {
                        newOp.BeginMethod = existingOp.BeginMethod;
                        newOp.EndMethod = existingOp.EndMethod;
                        newOp.TaskMethod = existingOp.TaskMethod;
                        newOp.TaskTResult = existingOp.TaskTResult;
                        if (existingOp.TaskMethod != null)
                        {
                            // [....] vs. Task
                            VerifyConsistency(new SyncTaskOperationConsistencyVerifier(newOp, existingOp));
                        }
                        else
                        {
                            // [....] vs. Async
                            VerifyConsistency(new SyncAsyncOperationConsistencyVerifier(newOp, existingOp));
                        }
                        return newOp;
                    }
                }
            }

            OperationDescription operationDescription = new OperationDescription(operationName.EncodedName, declaringContract);
            operationDescription.IsInitiating = opAttr.IsInitiating;
            operationDescription.IsTerminating = opAttr.IsTerminating;
            operationDescription.IsSessionOpenNotificationEnabled = opAttr.IsSessionOpenNotificationEnabled;

            operationDescription.HasNoDisposableParameters = ServiceReflector.HasNoDisposableParameters(methodInfo);

            if (opAttr.HasProtectionLevel)
            {
                operationDescription.ProtectionLevel = opAttr.ProtectionLevel;
            }

            XmlQualifiedName contractQname = new XmlQualifiedName(declaringContract.Name, declaringContract.Namespace);

            object[] methodAttributes = ServiceReflector.GetCustomAttributes(methodInfo, typeof(FaultContractAttribute), false);

            if (opAttr.IsOneWay && methodAttributes.Length > 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.OneWayAndFaultsIncompatible2, methodInfo.DeclaringType.FullName, operationName.EncodedName)));
            }

            for (int i = 0; i < methodAttributes.Length; i++)
            {
                FaultContractAttribute knownFault = (FaultContractAttribute)methodAttributes[i];
                FaultDescription faultDescription = CreateFaultDescription(knownFault, contractQname, declaringContract.Namespace, operationDescription.XmlName);
                CheckDuplicateFaultContract(operationDescription.Faults, faultDescription, operationName.EncodedName);
                operationDescription.Faults.Add(faultDescription);
            }

            methodAttributes = ServiceReflector.GetCustomAttributes(methodInfo, typeof(ServiceKnownTypeAttribute), false);
            IEnumerable<Type> knownTypes = GetKnownTypes(methodAttributes, methodInfo);
            foreach (Type knownType in knownTypes)
                operationDescription.KnownTypes.Add(knownType);

            MessageDirection requestDirection = direction;
            MessageDirection responseDirection = MessageDirectionHelper.Opposite(direction);

            string requestAction = NamingHelper.GetMessageAction(contractQname,
                                                                   operationDescription.CodeName,
                                                                   opAttr.Action,
                                                                   false);

            string responseAction = NamingHelper.GetMessageAction(contractQname,
                                                                  operationDescription.CodeName,
                                                                  opAttr.ReplyAction,
                                                                  true);
            XmlName wrapperName = operationName;
            XmlName wrapperResponseName = GetBodyWrapperResponseName(operationName);
            string wrapperNamespace = declaringContract.Namespace;

            MessageDescription requestDescription = CreateMessageDescription(methodInfo,
                                                           isAsync,
                                                           isTask,
                                                           null,
                                                           null,
                                                           contractDescription.Namespace,
                                                           requestAction,
                                                           wrapperName,
                                                           wrapperNamespace,
                                                           requestDirection);
            MessageDescription responseDescription = null;
            operationDescription.Messages.Add(requestDescription);
            MethodInfo outputMethod = methodInfo;
            if (isTask)
            {
                operationDescription.TaskMethod = methodInfo;
                operationDescription.TaskTResult = taskTResult;
            }
            else if (!isAsync)
            {
                operationDescription.SyncMethod = methodInfo;
            }
            else
            {
                outputMethod = ServiceReflector.GetEndMethod(methodInfo);
                operationDescription.EndMethod = outputMethod;
                operationDescription.BeginMethod = methodInfo;
            }

            if (!opAttr.IsOneWay)
            {
                XmlName returnValueName = GetReturnValueName(operationName);
                responseDescription = CreateMessageDescription(outputMethod,
                                                                isAsync,
                                                                isTask,
                                                                taskTResult,
                                                                returnValueName,
                                                                contractDescription.Namespace,
                                                                responseAction,
                                                                wrapperResponseName,
                                                                wrapperNamespace,
                                                                responseDirection);
                operationDescription.Messages.Add(responseDescription);
            }
            else
            {
                if ((!isTask && outputMethod.ReturnType != ServiceReflector.VoidType) || (isTask && taskTResult != ServiceReflector.VoidType) ||
                    ServiceReflector.HasOutputParameters(outputMethod, isAsync))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ServiceOperationsMarkedWithIsOneWayTrueMust0)));
                }

                if (opAttr.ReplyAction != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.OneWayOperationShouldNotSpecifyAReplyAction1, operationName)));
                }
            }

            if (!opAttr.IsOneWay)
            {
                if (responseDescription.IsVoid &&
                    (requestDescription.IsUntypedMessage || requestDescription.IsTypedMessage))
                {
                    responseDescription.Body.WrapperName = responseDescription.Body.WrapperNamespace = null;
                }
                else if (requestDescription.IsVoid &&
                    (responseDescription.IsUntypedMessage || responseDescription.IsTypedMessage))
                {
                    requestDescription.Body.WrapperName = requestDescription.Body.WrapperNamespace = null;
                }
            }
            return operationDescription;
        }

        private void CheckDuplicateFaultContract(FaultDescriptionCollection faultDescriptionCollection, FaultDescription fault, string operationName)
        {
            foreach (FaultDescription existingFault in faultDescriptionCollection)
            {
                if (XmlName.IsNullOrEmpty(existingFault.ElementName) && XmlName.IsNullOrEmpty(fault.ElementName) && existingFault.DetailType == fault.DetailType)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SFxFaultContractDuplicateDetailType, operationName, fault.DetailType)));
                if (!XmlName.IsNullOrEmpty(existingFault.ElementName) && !XmlName.IsNullOrEmpty(fault.ElementName) && existingFault.ElementName == fault.ElementName && existingFault.Namespace == fault.Namespace)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SFxFaultContractDuplicateElement, operationName, fault.ElementName, fault.Namespace)));
            }
        }

        FaultDescription CreateFaultDescription(FaultContractAttribute attr,
                                                XmlQualifiedName contractName,
                                                string contractNamespace,
                                                XmlName operationName)
        {
            XmlName faultName = new XmlName(attr.Name ?? NamingHelper.TypeName(attr.DetailType) + FaultSuffix);
            FaultDescription fault = new FaultDescription(NamingHelper.GetMessageAction(contractName, operationName.DecodedName + faultName.DecodedName, attr.Action, false/*isResponse*/));
            if (attr.Name != null)
                fault.SetNameAndElement(faultName);
            else
                fault.SetNameOnly(faultName);
            fault.Namespace = attr.Namespace ?? contractNamespace;
            fault.DetailType = attr.DetailType;
            if (attr.HasProtectionLevel)
            {
                fault.ProtectionLevel = attr.ProtectionLevel;
            }
            return fault;
        }

        MessageDescription CreateMessageDescription(MethodInfo methodInfo,
                                                           bool isAsync,
                                                           bool isTask,
                                                           Type taskTResult,
                                                           XmlName returnValueName,
                                                           string defaultNS,
                                                           string action,
                                                           XmlName wrapperName,
                                                           string wrapperNamespace,
                                                           MessageDirection direction)
        {
            string methodName = methodInfo.Name;
            MessageDescription messageDescription;
            if (returnValueName == null)
            {
                ParameterInfo[] parameters = ServiceReflector.GetInputParameters(methodInfo, isAsync);
                if (parameters.Length == 1 && parameters[0].ParameterType.IsDefined(typeof(MessageContractAttribute), false))
                {
                    messageDescription = CreateTypedMessageDescription(parameters[0].ParameterType,
                                                                null,
                                                                null,
                                                                defaultNS,
                                                                action,
                                                                direction);
                }
                else
                {
                    messageDescription = CreateParameterMessageDescription(parameters,
                                                             null,
                                                             null,
                                                             null,
                                                             methodName,
                                                             defaultNS,
                                                             action,
                                                             wrapperName,
                                                             wrapperNamespace,
                                                             direction);
                }
            }
            else
            {
                ParameterInfo[] parameters = ServiceReflector.GetOutputParameters(methodInfo, isAsync);
                Type responseType = isTask ? taskTResult : methodInfo.ReturnType;
                if (responseType.IsDefined(typeof(MessageContractAttribute), false) && parameters.Length == 0)
                {
                    messageDescription = CreateTypedMessageDescription(responseType,
                                                         methodInfo.ReturnTypeCustomAttributes,
                                                         returnValueName,
                                                         defaultNS,
                                                         action,
                                                         direction);
                }
                else
                {
                    messageDescription = CreateParameterMessageDescription(parameters,
                                                         responseType,
                                                         methodInfo.ReturnTypeCustomAttributes,
                                                         returnValueName,
                                                         methodName,
                                                         defaultNS,
                                                         action,
                                                         wrapperName,
                                                         wrapperNamespace,
                                                         direction);
                }
            }

            bool hasUnknownHeaders = false;
            for (int i = 0; i < messageDescription.Headers.Count; i++)
            {
                MessageHeaderDescription header = messageDescription.Headers[i];
                if (header.IsUnknownHeaderCollection)
                {
                    if (hasUnknownHeaders)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxMultipleUnknownHeaders, methodInfo, methodInfo.DeclaringType)));
                    }
                    else
                    {
                        hasUnknownHeaders = true;
                    }
                }
            }
            return messageDescription;
        }

        MessageDescription CreateParameterMessageDescription(ParameterInfo[] parameters,
                                                  Type returnType,
                                                  ICustomAttributeProvider returnAttrProvider,
                                                  XmlName returnValueName,
                                                  string methodName,
                                                  string defaultNS,
                                                  string action,
                                                  XmlName wrapperName,
                                                  string wrapperNamespace,
                                                  MessageDirection direction)
        {
            foreach (ParameterInfo param in parameters)
            {
                if (GetParameterType(param).IsDefined(typeof(MessageContractAttribute), false/*inherit*/))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidMessageContractSignature, methodName)));
                }
            }
            if (returnType != null && returnType.IsDefined(typeof(MessageContractAttribute), false/*inherit*/))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidMessageContractSignature, methodName)));
            }

            MessageDescription messageDescription = new MessageDescription(action, direction);
            MessagePartDescriptionCollection partDescriptionCollection = messageDescription.Body.Parts;
            for (int index = 0; index < parameters.Length; index++)
            {
                MessagePartDescription partDescription = CreateParameterPartDescription(new XmlName(parameters[index].Name), defaultNS, index, parameters[index], GetParameterType(parameters[index]));
                if (partDescriptionCollection.Contains(new XmlQualifiedName(partDescription.Name, partDescription.Namespace)))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageContractException(SR.GetString(SR.SFxDuplicateMessageParts, partDescription.Name, partDescription.Namespace)));
                messageDescription.Body.Parts.Add(partDescription);
            }

            if (returnType != null)
            {
                messageDescription.Body.ReturnValue = CreateParameterPartDescription(returnValueName, defaultNS, 0, returnAttrProvider, returnType);
            }
            if (messageDescription.IsUntypedMessage)
            {
                messageDescription.Body.WrapperName = null;
                messageDescription.Body.WrapperNamespace = null;
            }
            else
            {
                messageDescription.Body.WrapperName = wrapperName.EncodedName;
                messageDescription.Body.WrapperNamespace = wrapperNamespace;
            }

            return messageDescription;
        }

        private static MessagePartDescription CreateParameterPartDescription(XmlName defaultName, string defaultNS, int index, ICustomAttributeProvider attrProvider, Type type)
        {
            MessagePartDescription parameterPart;
            MessageParameterAttribute paramAttr = ServiceReflector.GetSingleAttribute<MessageParameterAttribute>(attrProvider);

            XmlName name = paramAttr == null || !paramAttr.IsNameSetExplicit ? defaultName : new XmlName(paramAttr.Name);
            parameterPart = new MessagePartDescription(name.EncodedName, defaultNS);
            parameterPart.Type = type;
            parameterPart.Index = index;
            parameterPart.AdditionalAttributesProvider = attrProvider;
            return parameterPart;
        }

        [SuppressMessage(FxCop.Category.Usage, "CA2301:EmbeddableTypesInContainersRule", MessageId = "messages", Justification = "No need to support type equivalence here.")]
        internal MessageDescription CreateTypedMessageDescription(Type typedMessageType,
                                                  ICustomAttributeProvider returnAttrProvider,
                                                  XmlName returnValueName,
                                                  string defaultNS,
                                                  string action,
                                                  MessageDirection direction)
        {


            MessageDescription messageDescription;
            bool messageItemsInitialized = false;
            MessageDescriptionItems messageItems;
            MessageContractAttribute messageContractAttribute = ServiceReflector.GetSingleAttribute<MessageContractAttribute>(typedMessageType);
            if (messages.TryGetValue(typedMessageType, out messageItems))
            {
                messageDescription = new MessageDescription(action, direction, messageItems);
                messageItemsInitialized = true;
            }
            else
                messageDescription = new MessageDescription(action, direction, null);
            messageDescription.MessageType = typedMessageType;
            messageDescription.MessageName = new XmlName(NamingHelper.TypeName(typedMessageType));
            if (messageContractAttribute.IsWrapped)
            {
                messageDescription.Body.WrapperName = GetWrapperName(messageContractAttribute.WrapperName, messageDescription.MessageName).EncodedName;
                messageDescription.Body.WrapperNamespace = messageContractAttribute.WrapperNamespace ?? defaultNS;
            }
            List<MemberInfo> contractMembers = new List<MemberInfo>();

            for (Type baseType = typedMessageType; baseType != null && baseType != typeof(object) && baseType != typeof(ValueType); baseType = baseType.BaseType)
            {
                if (!baseType.IsDefined(typeof(MessageContractAttribute), false/*inherit*/))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxMessageContractBaseTypeNotValid, baseType, typedMessageType)));
                }
                if (!messageDescription.HasProtectionLevel)
                {
                    MessageContractAttribute mca = ServiceReflector.GetRequiredSingleAttribute<MessageContractAttribute>(baseType);
                    if (mca.HasProtectionLevel)
                    {
                        messageDescription.ProtectionLevel = mca.ProtectionLevel;
                    }
                }

                if (messageItemsInitialized)
                    continue;
                foreach (MemberInfo memberInfo in baseType.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (memberInfo.MemberType != MemberTypes.Field &&
                        memberInfo.MemberType != MemberTypes.Property)
                    {
                        continue;
                    }
                    PropertyInfo property = memberInfo as PropertyInfo;
                    if (property != null)
                    {
                        MethodInfo getMethod = property.GetGetMethod(true);
                        if (getMethod != null && IsMethodOverriding(getMethod))
                        {
                            continue;
                        }
                        MethodInfo setMethod = property.GetSetMethod(true);
                        if (setMethod != null && IsMethodOverriding(setMethod))
                        {
                            continue;
                        }
                    }

                    if (memberInfo.IsDefined(typeof(MessageBodyMemberAttribute), false) ||
                        memberInfo.IsDefined(typeof(MessageHeaderAttribute), false) ||
                        memberInfo.IsDefined(typeof(MessageHeaderArrayAttribute), false) ||
                        memberInfo.IsDefined(typeof(MessagePropertyAttribute), false)
                        )
                    {
                        contractMembers.Add(memberInfo);
                    }
                }
            }

            if (messageItemsInitialized)
                return messageDescription;

            List<MessagePartDescription> bodyPartDescriptionList = new List<MessagePartDescription>();
            List<MessageHeaderDescription> headerPartDescriptionList = new List<MessageHeaderDescription>();
            for (int i = 0; i < contractMembers.Count; i++)
            {
                MemberInfo memberInfo = contractMembers[i];

                Type memberType;
                if (memberInfo.MemberType == MemberTypes.Property)
                {
                    memberType = ((PropertyInfo)memberInfo).PropertyType;
                }
                else
                {
                    memberType = ((FieldInfo)memberInfo).FieldType;
                }

                if (memberInfo.IsDefined(typeof(MessageHeaderArrayAttribute), false)
                    || memberInfo.IsDefined(typeof(MessageHeaderAttribute), false))
                {
                    headerPartDescriptionList.Add(CreateMessageHeaderDescription(memberType,
                                                                              memberInfo,
                                                                              new XmlName(memberInfo.Name),
                                                                              defaultNS,
                                                                              i,
                                                                              -1));
                }
                else if (memberInfo.IsDefined(typeof(MessagePropertyAttribute), false))
                {
                    messageDescription.Properties.Add(CreateMessagePropertyDescription(memberInfo,
                                                                              new XmlName(memberInfo.Name),
                                                                              i));
                }
                else
                {
                    bodyPartDescriptionList.Add(CreateMessagePartDescription(memberType,
                                                                         memberInfo,
                                                                         new XmlName(memberInfo.Name),
                                                                         defaultNS,
                                                                         i,
                                                                         -1));
                }
            }

            if (returnAttrProvider != null)
            {
                messageDescription.Body.ReturnValue = CreateMessagePartDescription(typeof(void),
                                                                  returnAttrProvider,
                                                                  returnValueName,
                                                                  defaultNS,
                                                                  0,
                                                                  0);
            }

            AddSortedParts<MessagePartDescription>(bodyPartDescriptionList, messageDescription.Body.Parts);
            AddSortedParts<MessageHeaderDescription>(headerPartDescriptionList, messageDescription.Headers);
            messages.Add(typedMessageType, messageDescription.Items);

            return messageDescription;
        }

        static bool IsMethodOverriding(MethodInfo method)
        {
            return method.IsVirtual && ((method.Attributes & MethodAttributes.NewSlot) == 0);
        }




        MessagePartDescription CreateMessagePartDescription(Type bodyType,
                                                         ICustomAttributeProvider attrProvider,
                                                         XmlName defaultName,
                                                         string defaultNS,
                                                         int parameterIndex,
                                                         int serializationIndex)
        {
            MessagePartDescription partDescription = null;
            MessageBodyMemberAttribute bodyAttr = ServiceReflector.GetSingleAttribute<MessageBodyMemberAttribute>(attrProvider, messageContractMemberAttributes);

            if (bodyAttr == null)
            {
                partDescription = new MessagePartDescription(defaultName.EncodedName, defaultNS);
                partDescription.SerializationPosition = serializationIndex;
            }
            else
            {
                XmlName partName = bodyAttr.IsNameSetExplicit ? new XmlName(bodyAttr.Name) : defaultName;
                string partNs = bodyAttr.IsNamespaceSetExplicit ? bodyAttr.Namespace : defaultNS;
                partDescription = new MessagePartDescription(partName.EncodedName, partNs);
                partDescription.SerializationPosition = bodyAttr.Order < 0 ? serializationIndex : bodyAttr.Order;
                if (bodyAttr.HasProtectionLevel)
                {
                    partDescription.ProtectionLevel = bodyAttr.ProtectionLevel;
                }
            }

            if (attrProvider is MemberInfo)
            {
                partDescription.MemberInfo = (MemberInfo)attrProvider;
            }
            partDescription.Type = bodyType;
            partDescription.Index = parameterIndex;
            return partDescription;
        }

        MessageHeaderDescription CreateMessageHeaderDescription(Type headerParameterType,
                                                                    ICustomAttributeProvider attrProvider,
                                                                    XmlName defaultName,
                                                                    string defaultNS,
                                                                    int parameterIndex,
                                                                    int serializationPosition)
        {
            MessageHeaderDescription headerDescription = null;
            MessageHeaderAttribute headerAttr = ServiceReflector.GetRequiredSingleAttribute<MessageHeaderAttribute>(attrProvider, messageContractMemberAttributes);
            XmlName headerName = headerAttr.IsNameSetExplicit ? new XmlName(headerAttr.Name) : defaultName;
            string headerNs = headerAttr.IsNamespaceSetExplicit ? headerAttr.Namespace : defaultNS;
            headerDescription = new MessageHeaderDescription(headerName.EncodedName, headerNs);
            headerDescription.UniquePartName = defaultName.EncodedName;

            if (headerAttr is MessageHeaderArrayAttribute)
            {
                if (!headerParameterType.IsArray || headerParameterType.GetArrayRank() != 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidMessageHeaderArrayType, defaultName)));
                }
                headerDescription.Multiple = true;
                headerParameterType = headerParameterType.GetElementType();
            }
            headerDescription.Type = TypedHeaderManager.GetHeaderType(headerParameterType);
            headerDescription.TypedHeader = (headerParameterType != headerDescription.Type);
            if (headerDescription.TypedHeader)
            {
                if (headerAttr.IsMustUnderstandSet || headerAttr.IsRelaySet || headerAttr.Actor != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxStaticMessageHeaderPropertiesNotAllowed, defaultName)));
                }
            }
            else
            {
                headerDescription.Actor = headerAttr.Actor;
                headerDescription.MustUnderstand = headerAttr.MustUnderstand;
                headerDescription.Relay = headerAttr.Relay;
            }
            headerDescription.SerializationPosition = serializationPosition;
            if (headerAttr.HasProtectionLevel)
            {
                headerDescription.ProtectionLevel = headerAttr.ProtectionLevel;
            }
            if (attrProvider is MemberInfo)
            {
                headerDescription.MemberInfo = (MemberInfo)attrProvider;
            }

            headerDescription.Index = parameterIndex;
            return headerDescription;
        }

        MessagePropertyDescription CreateMessagePropertyDescription(ICustomAttributeProvider attrProvider,
                                                            XmlName defaultName,
                                                            int parameterIndex)
        {
            MessagePropertyAttribute attr = ServiceReflector.GetSingleAttribute<MessagePropertyAttribute>(attrProvider, messageContractMemberAttributes);
            XmlName propertyName = attr.IsNameSetExplicit ? new XmlName(attr.Name) : defaultName;
            MessagePropertyDescription propertyDescription = new MessagePropertyDescription(propertyName.EncodedName);
            propertyDescription.Index = parameterIndex;

            if (attrProvider is MemberInfo)
            {
                propertyDescription.MemberInfo = (MemberInfo)attrProvider;
            }

            return propertyDescription;
        }

        internal static XmlName GetReturnValueName(XmlName methodName)
        {
            return new XmlName(methodName.EncodedName + ReturnSuffix, true);
        }

        internal static XmlName GetReturnValueName(string methodName)
        {
            return new XmlName(methodName + ReturnSuffix);
        }

        internal static Type GetParameterType(ParameterInfo parameterInfo)
        {
            Type parameterType = parameterInfo.ParameterType;
            if (parameterType.IsByRef)
            {
                return parameterType.GetElementType();
            }
            else
            {
                return parameterType;
            }
        }

        internal static XmlName GetWrapperName(string wrapperName, XmlName defaultName)
        {
            if (string.IsNullOrEmpty(wrapperName))
                return defaultName;
            return new XmlName(wrapperName);
        }

        void AddSortedParts<T>(List<T> partDescriptionList, KeyedCollection<XmlQualifiedName, T> partDescriptionCollection)
            where T : MessagePartDescription
        {
            MessagePartDescription[] partDescriptions = partDescriptionList.ToArray();
            if (partDescriptions.Length > 1)
            {
                Array.Sort<MessagePartDescription>(partDescriptions, CompareMessagePartDescriptions);
            }
            foreach (T partDescription in partDescriptions)
            {
                if (partDescriptionCollection.Contains(new XmlQualifiedName(partDescription.Name, partDescription.Namespace)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageContractException(SR.GetString(SR.SFxDuplicateMessageParts, partDescription.Name, partDescription.Namespace)));
                }
                partDescriptionCollection.Add(partDescription);
            }
        }

        private abstract class OperationConsistencyVerifier
        {
            public virtual void VerifyParameterLength() { }
            public virtual void VerifyParameterType() { }
            public virtual void VerifyOutParameterType() { }
            public virtual void VerifyReturnType() { }
            public virtual void VerifyFaultContractAttribute() { }
            public virtual void VerifyKnownTypeAttribute() { }
            public virtual void VerifyIsOneWayStatus() { }
            public virtual void VerifyActionAndReplyAction() { }
        }

        private class SyncAsyncOperationConsistencyVerifier : OperationConsistencyVerifier
        {
            OperationDescription syncOperation;
            OperationDescription asyncOperation;
            ParameterInfo[] syncInputs;
            ParameterInfo[] asyncInputs;
            ParameterInfo[] syncOutputs;
            ParameterInfo[] asyncOutputs;

            public SyncAsyncOperationConsistencyVerifier(OperationDescription syncOperation, OperationDescription asyncOperation)
            {
                this.syncOperation = syncOperation;
                this.asyncOperation = asyncOperation;
                this.syncInputs = ServiceReflector.GetInputParameters(this.syncOperation.SyncMethod, false);
                this.asyncInputs = ServiceReflector.GetInputParameters(this.asyncOperation.BeginMethod, true);
                this.syncOutputs = ServiceReflector.GetOutputParameters(this.syncOperation.SyncMethod, false);
                this.asyncOutputs = ServiceReflector.GetOutputParameters(this.asyncOperation.EndMethod, true);
            }

            public override void VerifyParameterLength()
            {
                if (this.syncInputs.Length != this.asyncInputs.Length || this.syncOutputs.Length != this.asyncOutputs.Length)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.SyncAsyncMatchConsistency_Parameters5,
                                                                   this.syncOperation.SyncMethod.Name,
                                                                   this.syncOperation.SyncMethod.DeclaringType,
                                                                   this.asyncOperation.BeginMethod.Name,
                                                                   this.asyncOperation.EndMethod.Name,
                                                                   this.syncOperation.Name)));
                }
            }

            public override void VerifyParameterType()
            {
                for (int i = 0; i < this.syncInputs.Length; i++)
                {
                    if (this.syncInputs[i].ParameterType != this.asyncInputs[i].ParameterType)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.SyncAsyncMatchConsistency_Parameters5,
                                                                       this.syncOperation.SyncMethod.Name,
                                                                       this.syncOperation.SyncMethod.DeclaringType,
                                                                       this.asyncOperation.BeginMethod.Name,
                                                                       this.asyncOperation.EndMethod.Name,
                                                                       this.syncOperation.Name)));
                    }
                }
            }

            public override void VerifyOutParameterType()
            {
                for (int i = 0; i < this.syncOutputs.Length; i++)
                {
                    if (this.syncOutputs[i].ParameterType != this.asyncOutputs[i].ParameterType)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.SyncAsyncMatchConsistency_Parameters5,
                                                                       this.syncOperation.SyncMethod.Name,
                                                                       this.syncOperation.SyncMethod.DeclaringType,
                                                                       this.asyncOperation.BeginMethod.Name,
                                                                       this.asyncOperation.EndMethod.Name,
                                                                       this.syncOperation.Name)));
                    }
                }
            }

            public override void VerifyReturnType()
            {
                if (this.syncOperation.SyncMethod.ReturnType != this.syncOperation.EndMethod.ReturnType)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.SyncAsyncMatchConsistency_ReturnType5,
                                                                   this.syncOperation.SyncMethod.Name,
                                                                   this.syncOperation.SyncMethod.DeclaringType,
                                                                   this.asyncOperation.BeginMethod.Name,
                                                                   this.asyncOperation.EndMethod.Name,
                                                                   this.syncOperation.Name)));
                }
            }

            public override void VerifyFaultContractAttribute()
            {
                if (this.asyncOperation.Faults.Count != 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.SyncAsyncMatchConsistency_Attributes6,
                                                                   this.syncOperation.SyncMethod.Name,
                                                                   this.syncOperation.SyncMethod.DeclaringType,
                                                                   this.asyncOperation.BeginMethod.Name,
                                                                   this.asyncOperation.EndMethod.Name,
                                                                   this.syncOperation.Name,
                                                                   typeof(FaultContractAttribute).Name)));

                }
            }

            public override void VerifyKnownTypeAttribute()
            {
                if (this.asyncOperation.KnownTypes.Count != 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.SyncAsyncMatchConsistency_Attributes6,
                                                                   this.syncOperation.SyncMethod.Name,
                                                                   this.syncOperation.SyncMethod.DeclaringType,
                                                                   this.asyncOperation.BeginMethod.Name,
                                                                   this.asyncOperation.EndMethod.Name,
                                                                   this.syncOperation.Name,
                                                                   typeof(ServiceKnownTypeAttribute).Name)));
                }
            }

            public override void VerifyIsOneWayStatus()
            {
                if (this.syncOperation.Messages.Count != this.asyncOperation.Messages.Count)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.SyncAsyncMatchConsistency_Property6,
                                                                       this.syncOperation.SyncMethod.Name,
                                                                       this.syncOperation.SyncMethod.DeclaringType,
                                                                       this.asyncOperation.BeginMethod.Name,
                                                                       this.asyncOperation.EndMethod.Name,
                                                                       this.syncOperation.Name,
                                                                       "IsOneWay")));
                }
            }

            public override void VerifyActionAndReplyAction()
            {
                for (int index = 0; index < this.syncOperation.Messages.Count; ++index)
                {
                    if (this.syncOperation.Messages[index].Action != this.asyncOperation.Messages[index].Action)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.SyncAsyncMatchConsistency_Property6,
                                                                       this.syncOperation.SyncMethod.Name,
                                                                       this.syncOperation.SyncMethod.DeclaringType,
                                                                       this.asyncOperation.BeginMethod.Name,
                                                                       this.asyncOperation.EndMethod.Name,
                                                                       this.syncOperation.Name,
                                                                       index == 0 ? "Action" : "ReplyAction")));
                    }
                }
            }
        }

        private class SyncTaskOperationConsistencyVerifier : OperationConsistencyVerifier
        {
            OperationDescription syncOperation;
            OperationDescription taskOperation;
            ParameterInfo[] syncInputs;
            ParameterInfo[] taskInputs;

            public SyncTaskOperationConsistencyVerifier(OperationDescription syncOperation, OperationDescription taskOperation)
            {
                this.syncOperation = syncOperation;
                this.taskOperation = taskOperation;
                this.syncInputs = ServiceReflector.GetInputParameters(this.syncOperation.SyncMethod, false);
                this.taskInputs = ServiceReflector.GetInputParameters(this.taskOperation.TaskMethod, false);
            }

            public override void VerifyParameterLength()
            {
                if (this.syncInputs.Length != this.taskInputs.Length)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.SyncTaskMatchConsistency_Parameters5,
                                                                   this.syncOperation.SyncMethod.Name,
                                                                   this.syncOperation.SyncMethod.DeclaringType,
                                                                   this.taskOperation.TaskMethod.Name,
                                                                   this.syncOperation.Name)));
                }
            }

            public override void VerifyParameterType()
            {
                for (int i = 0; i < this.syncInputs.Length; i++)
                {
                    if (this.syncInputs[i].ParameterType != this.taskInputs[i].ParameterType)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.SyncTaskMatchConsistency_Parameters5,
                                                                       this.syncOperation.SyncMethod.Name,
                                                                       this.syncOperation.SyncMethod.DeclaringType,
                                                                       this.taskOperation.TaskMethod.Name,
                                                                       this.syncOperation.Name)));
                    }
                }
            }

            public override void VerifyReturnType()
            {
                if (this.syncOperation.SyncMethod.ReturnType != this.syncOperation.TaskTResult)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.SyncTaskMatchConsistency_ReturnType5,
                                                                   this.syncOperation.SyncMethod.Name,
                                                                   this.syncOperation.SyncMethod.DeclaringType,
                                                                   this.taskOperation.TaskMethod.Name,
                                                                   this.syncOperation.Name)));
                }
            }

            public override void VerifyFaultContractAttribute()
            {
                if (this.taskOperation.Faults.Count != 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.SyncTaskMatchConsistency_Attributes6,
                                                                   this.syncOperation.SyncMethod.Name,
                                                                   this.syncOperation.SyncMethod.DeclaringType,
                                                                   this.taskOperation.TaskMethod.Name,
                                                                   this.syncOperation.Name,
                                                                   typeof(FaultContractAttribute).Name)));

                }
            }

            public override void VerifyKnownTypeAttribute()
            {
                if (this.taskOperation.KnownTypes.Count != 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.SyncTaskMatchConsistency_Attributes6,
                                                                   this.syncOperation.SyncMethod.Name,
                                                                   this.syncOperation.SyncMethod.DeclaringType,
                                                                   this.taskOperation.TaskMethod.Name,
                                                                   this.syncOperation.Name,
                                                                   typeof(ServiceKnownTypeAttribute).Name)));
                }
            }

            public override void VerifyIsOneWayStatus()
            {
                if (this.syncOperation.Messages.Count != this.taskOperation.Messages.Count)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.SyncTaskMatchConsistency_Property6,
                                                                       this.syncOperation.SyncMethod.Name,
                                                                       this.syncOperation.SyncMethod.DeclaringType,
                                                                       this.taskOperation.TaskMethod.Name,
                                                                       this.syncOperation.Name,
                                                                       "IsOneWay")));
                }
            }

            public override void VerifyActionAndReplyAction()
            {
                for (int index = 0; index < this.syncOperation.Messages.Count; ++index)
                {
                    if (this.syncOperation.Messages[index].Action != this.taskOperation.Messages[index].Action)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.SyncTaskMatchConsistency_Property6,
                                                                       this.syncOperation.SyncMethod.Name,
                                                                       this.syncOperation.SyncMethod.DeclaringType,
                                                                       this.taskOperation.TaskMethod.Name,
                                                                       this.syncOperation.Name,
                                                                       index == 0 ? "Action" : "ReplyAction")));
                    }
                }
            }
        }

        private class TaskAsyncOperationConsistencyVerifier : OperationConsistencyVerifier
        {
            OperationDescription taskOperation;
            OperationDescription asyncOperation;
            ParameterInfo[] taskInputs;
            ParameterInfo[] asyncInputs;

            public TaskAsyncOperationConsistencyVerifier(OperationDescription taskOperation, OperationDescription asyncOperation)
            {
                this.taskOperation = taskOperation;
                this.asyncOperation = asyncOperation;
                this.taskInputs = ServiceReflector.GetInputParameters(this.taskOperation.TaskMethod, false);
                this.asyncInputs = ServiceReflector.GetInputParameters(this.asyncOperation.BeginMethod, true);
            }

            public override void VerifyParameterLength()
            {
                if (this.taskInputs.Length != this.asyncInputs.Length)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.TaskAsyncMatchConsistency_Parameters5,
                                                                   this.taskOperation.TaskMethod.Name,
                                                                   this.taskOperation.TaskMethod.DeclaringType,
                                                                   this.asyncOperation.BeginMethod.Name,
                                                                   this.asyncOperation.EndMethod.Name,
                                                                   this.taskOperation.Name)));
                }
            }

            public override void VerifyParameterType()
            {
                for (int i = 0; i < this.taskInputs.Length; i++)
                {
                    if (this.taskInputs[i].ParameterType != this.asyncInputs[i].ParameterType)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.TaskAsyncMatchConsistency_Parameters5,
                                                                       this.taskOperation.TaskMethod.Name,
                                                                       this.taskOperation.TaskMethod.DeclaringType,
                                                                       this.asyncOperation.BeginMethod.Name,
                                                                       this.asyncOperation.EndMethod.Name,
                                                                       this.taskOperation.Name)));
                    }
                }
            }

            public override void VerifyReturnType()
            {
                if (this.taskOperation.TaskTResult != this.asyncOperation.EndMethod.ReturnType)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.TaskAsyncMatchConsistency_ReturnType5,
                                                                   this.taskOperation.TaskMethod.Name,
                                                                   this.taskOperation.TaskMethod.DeclaringType,
                                                                   this.asyncOperation.BeginMethod.Name,
                                                                   this.asyncOperation.EndMethod.Name,
                                                                   this.taskOperation.Name)));
                }
            }

            public override void VerifyFaultContractAttribute()
            {
                if (this.asyncOperation.Faults.Count != 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.TaskAsyncMatchConsistency_Attributes6,
                                                                   this.taskOperation.TaskMethod.Name,
                                                                   this.taskOperation.TaskMethod.DeclaringType,
                                                                   this.asyncOperation.BeginMethod.Name,
                                                                   this.asyncOperation.EndMethod.Name,
                                                                   this.taskOperation.Name,
                                                                   typeof(FaultContractAttribute).Name)));

                }
            }

            public override void VerifyKnownTypeAttribute()
            {
                if (this.asyncOperation.KnownTypes.Count != 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.TaskAsyncMatchConsistency_Attributes6,
                                                                   this.taskOperation.TaskMethod.Name,
                                                                   this.taskOperation.TaskMethod.DeclaringType,
                                                                   this.asyncOperation.BeginMethod.Name,
                                                                   this.asyncOperation.EndMethod.Name,
                                                                   this.taskOperation.Name,
                                                                   typeof(ServiceKnownTypeAttribute).Name)));
                }
            }

            public override void VerifyIsOneWayStatus()
            {
                if (this.taskOperation.Messages.Count != this.asyncOperation.Messages.Count)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.TaskAsyncMatchConsistency_Property6,
                                                                       this.taskOperation.TaskMethod.Name,
                                                                       this.taskOperation.TaskMethod.DeclaringType,
                                                                       this.asyncOperation.BeginMethod.Name,
                                                                       this.asyncOperation.EndMethod.Name,
                                                                       this.taskOperation.Name,
                                                                       "IsOneWay")));
                }
            }

            public override void VerifyActionAndReplyAction()
            {
                for (int index = 0; index < this.taskOperation.Messages.Count; ++index)
                {
                    if (this.taskOperation.Messages[index].Action != this.asyncOperation.Messages[index].Action)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.TaskAsyncMatchConsistency_Property6,
                                                                       this.taskOperation.TaskMethod.Name,
                                                                       this.taskOperation.TaskMethod.DeclaringType,
                                                                       this.asyncOperation.BeginMethod.Name,
                                                                       this.asyncOperation.EndMethod.Name,
                                                                       this.taskOperation.Name,
                                                                       index == 0 ? "Action" : "ReplyAction")));
                    }
                }
            }
        }

        class ContractReflectionInfo
        {
            internal Type iface;
            internal Type callbackiface;
        }

        // This function factors out the logic of how programming model attributes interact with service inheritance.
        // See MB 37427 for details.
        //
        // To use this, just call ApplyServiceInheritance() with
        //  - the service type you want to pull behavior attributes from
        //  - the "destination" behavior collection, where all the right behavior attributes should be added to
        //  - a delegate
        // The delegate is just a function you write that behaves like this: 
        //    imagine that "currentType" was the only type (imagine there was no inheritance hierarchy)
        //    find desired behavior attributes on this type, and add them to "behaviors"
        // ApplyServiceInheritance then uses the logic you provide for getting behavior attributes from a single type, 
        // and it walks the actual type hierarchy and does the inheritance/override logic for you.
        public static void ApplyServiceInheritance<IBehavior, TBehaviorCollection>(
                     Type serviceType,
                     TBehaviorCollection descriptionBehaviors,
                     ServiceInheritanceCallback<IBehavior, TBehaviorCollection> callback)
            where IBehavior : class
            where TBehaviorCollection : KeyedByTypeCollection<IBehavior>
        {
            // work our way up the class hierarchy, looking for attributes; adding "bottom up" so that for each
            // type of attribute, we only pick up the bottom-most one (the one attached to most-derived class)
            for (Type currentType = serviceType; currentType != null; currentType = currentType.BaseType)
            {
                AddBehaviorsAtOneScope(currentType, descriptionBehaviors, callback);
            }
        }

        public delegate void ServiceInheritanceCallback<IBehavior, TBehaviorCollection>(Type currentType, KeyedByTypeCollection<IBehavior> behaviors);

        // To use this, just call AddBehaviorsAtOneScope() with
        //  - the type you want to pull behavior attributes from
        //  - the "destination" behavior collection, where all the right behavior attributes should be added to
        //  - a delegate
        // The delegate is just a function you write that behaves like this: 
        //    imagine that "currentType" was the only type (imagine there was no inheritance hierarchy)
        //    find desired behavior attributes on this type, and add them to "behaviors"
        // AddBehaviorsAtOneScope then uses the logic you provide for getting behavior attributes from a single type, 
        // and it does the override logic for you (only add the behavior if it wasn't already in the descriptionBehaviors)
        static void AddBehaviorsAtOneScope<IBehavior, TBehaviorCollection>(
                     Type type,
                     TBehaviorCollection descriptionBehaviors,
                     ServiceInheritanceCallback<IBehavior, TBehaviorCollection> callback)
            where IBehavior : class
            where TBehaviorCollection : KeyedByTypeCollection<IBehavior>
        {
            KeyedByTypeCollection<IBehavior> toAdd = new KeyedByTypeCollection<IBehavior>();
            callback(type, toAdd);
            // toAdd now contains the set of behaviors we'd add if this type (scope) were the only source of behaviors

            for (int i = 0; i < toAdd.Count; i++)
            {
                IBehavior behavior = toAdd[i];
                if (!descriptionBehaviors.Contains(behavior.GetType()))
                {
                    // if we didn't already see this type of attribute at a previous scope
                    // then it belongs in the final result
                    if (behavior is ServiceBehaviorAttribute || behavior is CallbackBehaviorAttribute)
                    {
                        descriptionBehaviors.Insert(0, behavior);
                    }
                    else
                    {
                        descriptionBehaviors.Add(behavior);
                    }
                }
            }
        }
    }
}
