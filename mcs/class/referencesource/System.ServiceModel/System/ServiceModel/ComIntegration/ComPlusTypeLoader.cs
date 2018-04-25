//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.EnterpriseServices;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Xml;
    using SR = System.ServiceModel.SR;

    class ComPlusTypeLoader : IContractResolver
    {
        ServiceInfo info;
        bool transactionFlow;
        ITypeCacheManager interfaceResolver;
        Dictionary<Guid, ContractDescription> contracts;

        public ComPlusTypeLoader(ServiceInfo info)
        {
            this.info = info;
            this.transactionFlow = info.TransactionOption == TransactionOption.Required ||
                                    info.TransactionOption == TransactionOption.Supported;
            this.interfaceResolver = new TypeCacheManager();
            this.contracts = new Dictionary<Guid, ContractDescription>();
        }

        void ValidateInterface(Guid iid)
        {
            // Filter known invalid IIDs
            if (!ComPlusTypeValidator.IsValidInterface(iid))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(SR.GetString(SR.InvalidWebServiceInterface, iid)));
            }

            // Filter out interfaces with no configured methods
            bool configuredInterface = false;
            foreach (ContractInfo contractInfo in this.info.Contracts)
            {
                if (contractInfo.IID == iid)
                {
                    if (contractInfo.Operations.Count == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(SR.GetString(SR.RequireConfiguredMethods, iid)));
                    }

                    configuredInterface = true;
                    break;
                }
            }

            // Filter out interfaces that aren't configured at all
            if (!configuredInterface)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(SR.GetString(SR.RequireConfiguredInterfaces, iid)));
            }
        }

        ContractDescription CreateContractDescriptionInternal(Guid iid, Type type)
        {
            ComContractElement contractConfigElement = ConfigLoader.LookupComContract(iid);

            if (contractConfigElement == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(SR.GetString(SR.InterfaceNotFoundInConfig, iid)));
            if (String.IsNullOrEmpty(contractConfigElement.Name) || String.IsNullOrEmpty(contractConfigElement.Namespace))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(SR.GetString(SR.CannotHaveNullOrEmptyNameOrNamespaceForIID, iid)));

            ContractDescription contract = new ContractDescription(contractConfigElement.Name, contractConfigElement.Namespace);
            contract.ContractType = type;
            contract.SessionMode = contractConfigElement.RequiresSession ? SessionMode.Required : SessionMode.Allowed;

            bool methodFound = false;

            List<Guid> guidList = new List<Guid>();

            foreach (ComPersistableTypeElement typeElement in contractConfigElement.PersistableTypes)
            {
                Guid typeGuid = Fx.CreateGuid(typeElement.ID);

                guidList.Add(typeGuid);
            }

            IDataContractSurrogate contractSurrogate = null;

            // We create a surrogate when the persistable types config section is there
            // even if we have no types that we allow.
            // That way we have control over the error when the client tries to make a call 
            // persistable type.
            if (guidList.Count > 0 || contractConfigElement.PersistableTypes.EmitClear)
            {
                contractSurrogate = new DataContractSurrogateForPersistWrapper(guidList.ToArray());
            }

            foreach (ComMethodElement configMethod in contractConfigElement.ExposedMethods)
            {
                methodFound = false;
                foreach (MethodInfo method in type.GetMethods())
                {
                    if (method.Name == configMethod.ExposedMethod)
                    {
                        OperationDescription operation = CreateOperationDescription(contract, method, contractConfigElement, (null != contractSurrogate));
                        ConfigureOperationDescriptionBehaviors(operation, contractSurrogate);
                        contract.Operations.Add(operation);
                        methodFound = true;
                        break;
                    }
                }
                if (!methodFound)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(SR.GetString(SR.MethodGivenInConfigNotFoundOnInterface, configMethod.ExposedMethod, iid)));
            }

            if (contract.Operations.Count == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(SR.GetString(SR.NoneOfTheMethodsForInterfaceFoundInConfig, iid)));

            ConfigureContractDescriptionBehaviors(contract);
            return contract;
        }

        void ConfigureContractDescriptionBehaviors(ContractDescription contract)
        {
            // OperationSelectorBehavior
            contract.Behaviors.Add(new OperationSelectorBehavior());

            // ComPlusContractBehavior
            ComPlusContractBehavior comPlusContractBehavior = new ComPlusContractBehavior(this.info);
            contract.Behaviors.Add(comPlusContractBehavior);
        }

        void ConfigureOperationDescriptionBehaviors(OperationDescription operation, IDataContractSurrogate contractSurrogate)
        {
            // DataContractSerializerOperationBehavior
            DataContractSerializerOperationBehavior contractSerializer = new DataContractSerializerOperationBehavior(operation, TypeLoader.DefaultDataContractFormatAttribute);

            if (null != contractSurrogate)
            {
                contractSerializer.DataContractSurrogate = contractSurrogate;
            }

            operation.Behaviors.Add(contractSerializer);

            // OperationInvokerBehavior
            operation.Behaviors.Add(new OperationInvokerBehavior());

            if (info.TransactionOption == TransactionOption.Supported || info.TransactionOption == TransactionOption.Required)
            {
                operation.Behaviors.Add(new TransactionFlowAttribute(TransactionFlowOption.Allowed));
            }

            // OperationBehaviorAttribute
            OperationBehaviorAttribute operationBehaviorAttribute = new OperationBehaviorAttribute();
            operationBehaviorAttribute.TransactionAutoComplete = true;
            operationBehaviorAttribute.TransactionScopeRequired = false;
            operation.Behaviors.Add(operationBehaviorAttribute);
        }

        //
        // Note - the code below this line a paraphrase of the SM reflection code in TypeLoader.cs
        // Ideally we would be re-using their code, but our assumptions are too disjoint
        // for that to be realistic at the time of writing (12/2004).
        //

        OperationDescription CreateOperationDescription(ContractDescription contract, MethodInfo methodInfo, ComContractElement config, bool allowReferences)
        {
            XmlName operationName = new XmlName(ServiceReflector.GetLogicalName(methodInfo));
            XmlName returnValueName = TypeLoader.GetReturnValueName(operationName);

            if (ServiceReflector.IsBegin(methodInfo) || ServiceReflector.IsTask(methodInfo))
            {
                Fx.Assert("No async operations allowed");

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.NoAsyncOperationsAllowed());
            }
            if (contract.Operations.FindAll(operationName.EncodedName).Count != 0)
            {
                Fx.Assert("Duplicate operation name");

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.DuplicateOperation());
            }

            OperationDescription operationDescription = new OperationDescription(operationName.EncodedName, contract);
            operationDescription.SyncMethod = methodInfo;
            operationDescription.IsInitiating = true;
            operationDescription.IsTerminating = false;

            operationDescription.KnownTypes.Add(typeof(Array));
            operationDescription.KnownTypes.Add(typeof(DBNull));
            operationDescription.KnownTypes.Add(typeof(CurrencyWrapper));
            operationDescription.KnownTypes.Add(typeof(ErrorWrapper));

            if (allowReferences)
                operationDescription.KnownTypes.Add(typeof(PersistStreamTypeWrapper));

            foreach (ComUdtElement udt in config.UserDefinedTypes)
            {
                Type knownType;

                Guid typeLibID = Fx.CreateGuid(udt.TypeLibID);

                TypeCacheManager.Provider.FindOrCreateType(typeLibID, udt.TypeLibVersion, Fx.CreateGuid(udt.TypeDefID), out knownType, false);

                this.info.AddUdt(knownType, typeLibID);
                operationDescription.KnownTypes.Add(knownType);
            }


            string ns = contract.Namespace;
            XmlQualifiedName contractQName = new XmlQualifiedName(contract.Name, ns);

            string requestAction = NamingHelper.GetMessageAction(contractQName,
                                                                 operationName.DecodedName,
                                                                 null,
                                                                 false);

            string responseAction = NamingHelper.GetMessageAction(contractQName,
                                                                  operationName.DecodedName,
                                                                  null,
                                                                  true);

            MessageDescription inMessage = CreateIncomingMessageDescription(contract,
                                                                            methodInfo,
                                                                            ns,
                                                                            requestAction,
                                                                            allowReferences);

            MessageDescription outMessage = CreateOutgoingMessageDescription(contract,
                                                                             methodInfo,
                                                                             returnValueName,
                                                                             ns,
                                                                             responseAction,
                                                                             allowReferences);

            operationDescription.Messages.Add(inMessage);
            operationDescription.Messages.Add(outMessage);

            return operationDescription;
        }

        MessageDescription CreateIncomingMessageDescription(ContractDescription contract,
                                                            MethodInfo methodInfo,
                                                            string ns,
                                                            string action,
                                                            bool allowReferences)
        {
            ParameterInfo[] parameters = ServiceReflector.GetInputParameters(methodInfo, false);
            return CreateParameterMessageDescription(contract,
                                                     parameters,
                                                     null,
                                                     null,
                                                     null,
                                                     methodInfo.Name,
                                                     ns,
                                                     action,
                                                     MessageDirection.Input,
                                                     allowReferences);
        }

        MessageDescription CreateOutgoingMessageDescription(ContractDescription contract,
                                                            MethodInfo methodInfo,
                                                            XmlName returnValueName,
                                                            string ns,
                                                            string action,
                                                            bool allowReferences)
        {
            ParameterInfo[] parameters = ServiceReflector.GetOutputParameters(methodInfo, false);
            return CreateParameterMessageDescription(contract,
                                                     parameters,
                                                     methodInfo.ReturnType,
                                                     methodInfo.ReturnTypeCustomAttributes,
                                                     returnValueName,
                                                     methodInfo.Name,
                                                     ns,
                                                     action,
                                                     MessageDirection.Output,
                                                     allowReferences);
        }

        MessageDescription CreateParameterMessageDescription(ContractDescription contract,
                                                             ParameterInfo[] parameters,
                                                             Type returnType,
                                                             ICustomAttributeProvider returnCustomAttributes,
                                                             XmlName returnValueName,
                                                             string methodName,
                                                             string ns,
                                                             string action,
                                                             MessageDirection direction,
                                                             bool allowReferences)
        {
            MessageDescription messageDescription = new MessageDescription(action, direction);
            messageDescription.Body.WrapperNamespace = ns;

            for (int index = 0; index < parameters.Length; index++)
            {
                ParameterInfo parameter = parameters[index];
                Type parameterType = TypeLoader.GetParameterType(parameter);

                if (!ComPlusTypeValidator.IsValidParameter(parameterType, parameter, allowReferences))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(SR.GetString(SR.InvalidWebServiceParameter,
                                                                parameter.Name,
                                                                parameterType.Name,
                                                                methodName,
                                                                contract.Name)));
                }

                MessagePartDescription messagePart = CreateMessagePartDescription(parameterType,
                                                                                  new XmlName(parameter.Name),
                                                                                  ns,
                                                                                  index);
                messageDescription.Body.Parts.Add(messagePart);
            }

            XmlName xmlName = new XmlName(methodName);
            if (returnType == null)
            {
                messageDescription.Body.WrapperName = xmlName.EncodedName;
            }
            else
            {
                messageDescription.Body.WrapperName = TypeLoader.GetBodyWrapperResponseName(xmlName).EncodedName;

                if (!ComPlusTypeValidator.IsValidParameter(returnType, returnCustomAttributes, allowReferences))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(SR.GetString(SR.InvalidWebServiceReturnValue,
                                                                returnType.Name,
                                                                methodName,
                                                                contract.Name)));
                }

                MessagePartDescription messagePart = CreateMessagePartDescription(returnType,
                                                                                  returnValueName,
                                                                                  ns,
                                                                                  0);
                messageDescription.Body.ReturnValue = messagePart;
            }

            return messageDescription;
        }

        MessagePartDescription CreateMessagePartDescription(Type bodyType,
                                                            XmlName name,
                                                            string ns,
                                                            int index)
        {
            MessagePartDescription partDescription = new MessagePartDescription(name.EncodedName, ns);
            partDescription.SerializationPosition = index;
            partDescription.MemberInfo = null;
            partDescription.Type = bodyType;
            partDescription.Index = index;
            return partDescription;
        }

        ContractDescription ResolveIMetadataExchangeToContract()
        {
            // Use ServiceModel's TypeLoader to load the IMetadataExchange contract
            TypeLoader typeLoader = new TypeLoader();
            return typeLoader.LoadContractDescription(typeof(IMetadataExchange));
        }

        public ContractDescription ResolveContract(string contractTypeString)
        {
            Guid iid;
            if (ServiceMetadataBehavior.MexContractName == contractTypeString)
                iid = typeof(IMetadataExchange).GUID;
            else
            {
                if (!DiagnosticUtility.Utility.TryCreateGuid(contractTypeString, out iid))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(SR.GetString(SR.ContractTypeNotAnIID, contractTypeString)));
                }

                ValidateInterface(iid);
            }

            // Check our cache to see if we already have one
            ContractDescription contract;
            if (this.contracts.TryGetValue(iid, out contract))
            {
                return contract;
            }

            // If this is not IMetadataExchange continue
            if (iid != typeof(IMetadataExchange).GUID)
            {
                Type type;
                // Generate a managed type corresponding to the interface in question
                try
                {
                    this.interfaceResolver.FindOrCreateType(this.info.ServiceType, iid, out type, false, true);
                }
                catch (InvalidOperationException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(e.Message));
                }

                contract = CreateContractDescriptionInternal(iid, type);
            }
            else
                contract = ResolveIMetadataExchangeToContract();
            contracts.Add(iid, contract);

            ComPlusServiceHostTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationServiceHostCreatedServiceContract,
                                SR.TraceCodeComIntegrationServiceHostCreatedServiceContract, this.info, contract);

            return contract;
        }
    }
}
