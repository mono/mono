//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net.Security;
    using System.Runtime;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Xml;
    using System.Xml.Linq;

    static class ContractInferenceHelper
    {
        static DataContractFormatAttribute dataContractFormatAttribute;
        static XmlSerializerFormatAttribute xmlSerializerFormatAttribute;
        static Type exceptionType;
        static Type faultExceptionType;

        public static DataContractFormatAttribute DataContractFormatAttribute
        {
            get
            {
                if (dataContractFormatAttribute == null)
                {
                    dataContractFormatAttribute = new DataContractFormatAttribute();
                }
                return dataContractFormatAttribute;
            }
        }

        public static XmlSerializerFormatAttribute XmlSerializerFormatAttribute
        {
            get
            {
                if (xmlSerializerFormatAttribute == null)
                {
                    xmlSerializerFormatAttribute = new XmlSerializerFormatAttribute
                    {
                        SupportFaults = true
                    };
                }
                return xmlSerializerFormatAttribute;
            }
        }

        public static Type ExceptionType
        {
            get
            {
                if (exceptionType == null)
                {
                    exceptionType = typeof(Exception);
                }
                return exceptionType;
            }
        }

        public static Type FaultExceptionType
        {
            get
            {
                if (faultExceptionType == null)
                {
                    faultExceptionType = typeof(FaultException<>);
                }
                return faultExceptionType;
            }
        }

        public static void ProvideDefaultNamespace(ref XName serviceContractName)
        {
            Fx.Assert(serviceContractName != null, "Argument cannot be null!");

            if (string.IsNullOrEmpty(serviceContractName.NamespaceName))
            {
                // If no namespace is given by the user, we provide default namespace. This is consistent with WCF.
                serviceContractName = XName.Get(serviceContractName.LocalName, NamingHelper.DefaultNamespace);
            }
        }

        public static ContractDescription CreateContractFromOperation(XName serviceContractName, OperationDescription operation)
        {
            Fx.Assert(serviceContractName != null, "serviceContractName cannot be null");
            ProvideDefaultNamespace(ref serviceContractName);

            ContractDescription contract = new ContractDescription(serviceContractName.LocalName, serviceContractName.NamespaceName)
            {
                // For inferred client side contracts, we do not set ContractType

                ConfigurationName = serviceContractName.LocalName,
                SessionMode = SessionMode.Allowed
            };
            contract.Operations.Add(operation);
            return contract;
        }

        public static ContractDescription CreateOutputChannelContractDescription(XName serviceContractName, ProtectionLevel? protectionLevel)
        {
            Fx.Assert(serviceContractName != null, "cannot be null");
            Type channelType = typeof(IOutputChannel);

            ProvideDefaultNamespace(ref serviceContractName);
            
            ContractDescription contract = new ContractDescription(serviceContractName.LocalName, serviceContractName.NamespaceName)
            {
                ContractType = channelType,
                ConfigurationName = serviceContractName.LocalName,
                SessionMode = SessionMode.Allowed
            };
            OperationDescription operation = new OperationDescription("Send", contract);
            MessageDescription message = new MessageDescription(MessageHeaders.WildcardAction, MessageDirection.Input);
            operation.Messages.Add(message);

            if (protectionLevel.HasValue)
            {
                operation.ProtectionLevel = protectionLevel.Value;
            }

            contract.Operations.Add(operation);
            return contract;
        }

        public static ContractDescription CreateRequestChannelContractDescription(XName serviceContractName, ProtectionLevel? protectionLevel)
        {
            Fx.Assert(serviceContractName != null, "cannot be null");
            Type channelType = typeof(IRequestChannel);

            ProvideDefaultNamespace(ref serviceContractName);

            ContractDescription contract = new ContractDescription(serviceContractName.LocalName, serviceContractName.NamespaceName)
            {
                ContractType = channelType,
                ConfigurationName = serviceContractName.LocalName,
                SessionMode = SessionMode.Allowed
            };
            OperationDescription operation = new OperationDescription("Request", contract);
            MessageDescription request = new MessageDescription(MessageHeaders.WildcardAction, MessageDirection.Input);
            MessageDescription reply = new MessageDescription(MessageHeaders.WildcardAction, MessageDirection.Output);
            operation.Messages.Add(request);
            operation.Messages.Add(reply);

            if (protectionLevel.HasValue)
            {
                operation.ProtectionLevel = protectionLevel.Value;
            }

            contract.Operations.Add(operation);
            return contract;
        }

        public static void EnsureTransactionFlowOnContract(
            ref ServiceEndpoint serviceEndpoint,
            XName serviceContractName,
            string operationName,
            string action,
            ProtectionLevel? protectionLevel)
        {
            Fx.Assert(serviceEndpoint != null, "ServiceEndpoint cannot be null!");

            // Client side fully inferred contract always has null ContractType
            if (serviceEndpoint.Contract.ContractType == null)
            {
                // If we are using the real contract, we only need to add TrancactionFlowAttribute to the operation
                Fx.Assert(serviceEndpoint.Contract.Operations.Count == 1, "Client side contract should have exactly one operation!");

                serviceEndpoint.Contract.Operations[0].Behaviors.Add(new TransactionFlowAttribute(TransactionFlowOption.Allowed));
            }
            else
            {
                // Replace the original fake contract with a fake contract tailored for transaction

                ContractDescription contract = null;
                OperationDescription operation = null;
                MessageDescription request = null;
                MessageDescription reply = null;

                Type channelType = typeof(IRequestChannel);

                // We need to create a contract description with the real service contract name
                // and operation name and actions and with the TransactionFlow operation behavior
                // because the TransactionChannelFactory has a dictionary of "Directional Action" to
                // transaction flow value that it uses to decide whether or not to include the
                // transaction header in the message.
                Fx.Assert(serviceContractName != null, "Argument serviceContractName cannot be null!");
                Fx.Assert(operationName != null, "Argument operationName cannot be null!");

                ProvideDefaultNamespace(ref serviceContractName);

                contract = new ContractDescription(serviceContractName.LocalName, serviceContractName.NamespaceName)
                {
                    ContractType = channelType,
                    SessionMode = SessionMode.Allowed
                };
                operation = new OperationDescription(operationName, contract);
                operation.Behaviors.Add(new TransactionFlowAttribute(TransactionFlowOption.Allowed));

                string requestAction = null;
                string replyAction = null;
                if (String.IsNullOrEmpty(action))
                {
                    // Construct the action.
                    requestAction = NamingHelper.GetMessageAction(operation, false);
                    replyAction = NamingHelper.GetMessageAction(operation, true);
                }
                else
                {
                    requestAction = action;
                    replyAction = action + TypeLoader.ResponseSuffix;
                }

                request = new MessageDescription(requestAction, MessageDirection.Input);
                reply = new MessageDescription(replyAction, MessageDirection.Output);

                operation.Messages.Add(request);
                operation.Messages.Add(reply);

                if (protectionLevel.HasValue)
                {
                    operation.ProtectionLevel = protectionLevel.Value;
                }

                contract.Operations.Add(operation);

                // We need to replace the ServiceEndpoint because ServiceEndpoint.Contract does not have a public setter
                Uri listenUri = serviceEndpoint.ListenUri;
                serviceEndpoint = new ServiceEndpoint(contract)
                {
                    Binding = serviceEndpoint.Binding,
                    Address = serviceEndpoint.Address,
                    Name = serviceEndpoint.Name,
                };
                if (listenUri != null)
                {
                    serviceEndpoint.ListenUri = listenUri;
                }
            }
        }

        public static OperationDescription CreateOneWayOperationDescription(Send send)
        {
            Fx.Assert(send != null, "Argument cannot be null!");
            return CreateOperationDescriptionCore(send, null);
        }

        public static OperationDescription CreateTwoWayOperationDescription(Send send, ReceiveReply receiveReply)
        {
            Fx.Assert(send != null && receiveReply != null, "Arguments cannot be null!");
            return CreateOperationDescriptionCore(send, receiveReply);
        }

        static OperationDescription CreateOperationDescriptionCore(Send send, ReceiveReply receiveReply)
        {
            XName contractXName = send.ServiceContractName;
            ProvideDefaultNamespace(ref contractXName);

            // Infer Name, Namespace, ConfigurationName
            ContractDescription contract = new ContractDescription(contractXName.LocalName, contractXName.NamespaceName);
            contract.ConfigurationName = send.EndpointConfigurationName;

            OperationDescription operation = new OperationDescription(NamingHelper.XmlName(send.OperationName), contract);
            if (send.ProtectionLevel.HasValue)
            {
                operation.ProtectionLevel = send.ProtectionLevel.Value;
            }

            AddKnownTypesToOperation(operation, send.KnownTypes);

            // Infer In-Message
            send.InternalContent.InferMessageDescription(operation, send, MessageDirection.Input);

            // Infer Out-Message
            if (receiveReply != null)
            {
                receiveReply.InternalContent.InferMessageDescription(operation, receiveReply, MessageDirection.Output);
            }

            PostProcessOperation(operation);
            AddSerializerProvider(operation, send.SerializerOption);

            contract.Operations.Add(operation);

            return operation;
        }

        // Create server side OperationDescription. 
        // Note this method assumes that CacheMetadata has been called on the Receive activity (as part of
        // the activity tree walk that is done in WorkflowService.GetContractDescriptions) because it relies on 
        // InternalReceiveMessage property of the Receive actitivy to be non-null.
        public static OperationDescription CreateOperationDescription(Receive receive, ContractDescription contract)
        {
            Fx.Assert(receive.InternalReceive != null, "This method can only be called if CacheMetadata has been called on the receive activity");

            OperationDescription operation = new OperationDescription(NamingHelper.XmlName(receive.OperationName), contract);

            if (receive.ProtectionLevel.HasValue)
            {
                operation.ProtectionLevel = receive.ProtectionLevel.Value;
            }

            // Infer In-Message
            receive.InternalContent.InferMessageDescription(operation, receive, MessageDirection.Input);

            // Infer Out-Message
            if (receive.HasReply)
            {
                // At this point, we already know all the following SendReplies are equivalent
                SendReply sendReply = receive.FollowingReplies[0];
                sendReply.InternalContent.InferMessageDescription(operation, sendReply, MessageDirection.Output);
            }
            else if (receive.HasFault)
            {
                // We infer Receive-SendFault pair as a two-way operation with void return value
                CheckForDisposableParameters(operation, Constants.EmptyTypeArray);
                AddOutputMessage(operation, null, Constants.EmptyStringArray, Constants.EmptyTypeArray);
            }

            PostProcessOperation(operation);

            // Behaviors
            AddSerializerProvider(operation, receive.SerializerOption);
            AddWorkflowOperationBehaviors(operation, receive.InternalReceive.OperationBookmarkName, receive.CanCreateInstance);

            if (receive.InternalReceive.AdditionalData.IsInsideTransactedReceiveScope)
            {
                operation.IsInsideTransactedReceiveScope = true;
                EnableTransactionBehavior(operation);
                if (receive.InternalReceive.AdditionalData.IsFirstReceiveOfTransactedReceiveScopeTree)
                {
                    operation.IsFirstReceiveOfTransactedReceiveScopeTree = true;
                }
            }

            return operation;
        }

        public static void AddInputMessage(OperationDescription operation, string overridingAction, Type type, SerializerOption serializerOption)
        {
            Fx.Assert(operation.Messages.Count == 0, "Operation already has input message");

            bool isResponse = false;
            MessageDescription message = MessageBuilder.CreateMessageDescription(
                operation, isResponse, MessageDirection.Input, overridingAction, type, serializerOption);

            operation.Messages.Add(message);
        }

        public static void AddInputMessage(OperationDescription operation, string overridingAction,
            string[] argumentNames, Type[] argumentTypes)
        {
            Fx.Assert(operation.Messages.Count == 0, "Operation already has input message");

            bool isResponse = false;
            MessageDescription message = MessageBuilder.CreateMessageDescription(
                operation, isResponse, MessageDirection.Input, overridingAction, argumentNames, argumentTypes);

            operation.Messages.Add(message);
        }

        public static void AddOutputMessage(OperationDescription operation, string overridingAction, Type type, SerializerOption serializerOption)
        {
            Fx.Assert(operation.Messages.Count > 0, "Operation does not have input message");
            Fx.Assert(operation.Messages.Count < 2, "Operation already has output message");

            bool isResponse = true;
            MessageDescription message = MessageBuilder.CreateMessageDescription(
                operation, isResponse, MessageDirection.Output, overridingAction, type, serializerOption);

            operation.Messages.Add(message);
        }

        public static void AddOutputMessage(OperationDescription operation, string overridingAction,
            string[] argumentNames, Type[] argumentTypes)
        {
            Fx.Assert(operation.Messages.Count > 0, "Operation does not have input message");
            Fx.Assert(operation.Messages.Count < 2, "Operation already has output message");

            bool isResponse = true;
            MessageDescription message = MessageBuilder.CreateMessageDescription(
                operation, isResponse, MessageDirection.Output, overridingAction, argumentNames, argumentTypes);

            operation.Messages.Add(message);
        }

        static void AddKnownTypesToOperation(OperationDescription operation, Collection<Type> knownTypes)
        {
            if (knownTypes != null)
            {
                foreach (Type knownType in knownTypes)
                {
                    operation.KnownTypes.Add(knownType);
                }
            }
        }

        public static void CheckForDisposableParameters(OperationDescription operation, Type type)
        {
            if (type == null)
            {
                 operation.HasNoDisposableParameters = true;
            }
            else
            {
                operation.HasNoDisposableParameters = !ServiceReflector.IsParameterDisposable(type);
            }
        }

        public static void CheckForDisposableParameters(OperationDescription operation, Type[] types)
        {
            Fx.Assert(types != null, "Argument cannot be null!");

            operation.HasNoDisposableParameters = true;
            foreach (Type type in types)
            {
                if (ServiceReflector.IsParameterDisposable(type))
                {
                    operation.HasNoDisposableParameters = false;
                    break;
                }
            }
        }

        static void EnableTransactionBehavior(OperationDescription operationDescription)
        {
            Fx.Assert(operationDescription != null, "OperationDescription is null");

            OperationBehaviorAttribute attribute = operationDescription.Behaviors.Find<OperationBehaviorAttribute>();
            if (attribute != null)
            {
                attribute.TransactionScopeRequired = true;
                attribute.TransactionAutoComplete = false;
            }
            else
            {
                OperationBehaviorAttribute attr = new OperationBehaviorAttribute
                {
                    TransactionAutoComplete = false,
                    TransactionScopeRequired = true
                };
                operationDescription.Behaviors.Add(attr);
            }
            TransactionFlowAttribute transactionFlowAttribute = operationDescription.Behaviors.Find<TransactionFlowAttribute>();
            if (transactionFlowAttribute != null)
            {
                if (transactionFlowAttribute.Transactions != TransactionFlowOption.Allowed)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ContractInferenceValidationForTransactionFlowBehavior));
                }
            }
            else
            {
                if (!operationDescription.IsOneWay)
                {
                    operationDescription.Behaviors.Add(new TransactionFlowAttribute(TransactionFlowOption.Allowed));
                }
            }
        }
        
        static void PostProcessOperation(OperationDescription operation)
        {
            MessageBuilder.ClearWrapperNames(operation);
        }

        static void AddSerializerProvider(OperationDescription operation, SerializerOption serializerOption)
        {
            switch (serializerOption)
            {
                case SerializerOption.DataContractSerializer:
                    AddDataContractSerializerFormat(operation);
                    break;
                case SerializerOption.XmlSerializer:
                    AddXmlSerializerFormat(operation);
                    break;
            }
        }

        static void AddDataContractSerializerFormat(OperationDescription operation)
        {
            if (operation.Behaviors.Find<DataContractSerializerOperationBehavior>() != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.OperationHasSerializerBehavior(
                    operation.Name, operation.DeclaringContract.Name, typeof(DataContractSerializerOperationBehavior))));
            }
            operation.Behaviors.Add(new DataContractSerializerOperationBehavior(operation, DataContractFormatAttribute));
            if (!operation.Behaviors.Contains(typeof(DataContractSerializerOperationGenerator)))
            {
                operation.Behaviors.Add(new DataContractSerializerOperationGenerator());
            }
        }

        static void AddXmlSerializerFormat(OperationDescription operation)
        {
            if (operation.Behaviors.Find<XmlSerializerOperationBehavior>() != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(
                    SR.OperationHasSerializerBehavior(operation.Name, operation.DeclaringContract.Name, typeof(XmlSerializerOperationBehavior))));
            }
            operation.Behaviors.Add(new XmlSerializerOperationBehavior(operation, XmlSerializerFormatAttribute));
            if (!operation.Behaviors.Contains(typeof(XmlSerializerOperationGenerator)))
            {
                operation.Behaviors.Add(new XmlSerializerOperationGenerator(new XmlSerializerImportOptions()));
            }
        }

        static void AddWorkflowOperationBehaviors(OperationDescription operation, string bookmarkName, bool canCreateInstance)
        {
            KeyedByTypeCollection<IOperationBehavior> behaviors = operation.Behaviors;
            WorkflowOperationBehavior workflowOperationBehavior = behaviors.Find<WorkflowOperationBehavior>();
            if (workflowOperationBehavior == null)
            {
                behaviors.Add(new WorkflowOperationBehavior(new Bookmark(bookmarkName), canCreateInstance));
            }
            else
            {
                workflowOperationBehavior.CanCreateInstance = workflowOperationBehavior.CanCreateInstance || canCreateInstance;
            }
        }

        public static void CorrectOutMessageForOperation(Receive receive, OperationDescription operation)
        {
            // Remove the original outMessage
            Fx.Assert(operation.Messages.Count == 2, "OperationDescription must be two-way for CorrectOutMessageForOperation to be invoked!");
            operation.Messages.RemoveAt(1);

            SendReply sendReply = receive.FollowingReplies[0];
            sendReply.InternalContent.InferMessageDescription(operation, sendReply, MessageDirection.Output);

            ContractInferenceHelper.PostProcessOperation(operation);
        }

        public static void UpdateIsOneWayFlag(Receive receive, OperationDescription operation)
        {
            // Set InternalReceiveMessage.IsOneWay to false for two-way operations
            if (!operation.IsOneWay)
            {
                receive.SetIsOneWay(false);
            }
        }

        public static void AddFaultDescription(Receive activity, OperationDescription operation)
        {
            if (activity.HasFault)
            {
                foreach (SendReply sendFault in activity.FollowingFaults)
                {
                    string action = null;
                    Type type = null;

                    action = sendFault.Action;

                    SendMessageContent sendReply = sendFault.InternalContent as SendMessageContent;
                    if (sendReply != null)
                    {
                        type = sendReply.InternalDeclaredMessageType;
                    }
                    else
                    {
                        SendParametersContent sendReplyParameters = sendFault.InternalContent as SendParametersContent;
                        if (sendReplyParameters != null)
                        {
                            type = sendReplyParameters.ArgumentTypes[0];  // Exception should be the only parameter in SendFault
                        }
                    }

                    Fx.Assert(type != null, "Exception type cannot be null!");
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == FaultExceptionType)
                    {
                        Type faultType = type.GetGenericArguments()[0];
                        bool exists = false;

                        // We expect the number of fault types to be small, so we use iterative comparison 
                        foreach (FaultDescription faultDescription in operation.Faults)
                        {
                            if (faultDescription.DetailType == faultType)
                            {
                                if (faultDescription.Action != action)
                                {
                                    throw FxTrace.Exception.AsError(new ValidationException(SR.SendRepliesHaveSameFaultTypeDifferentAction));
                                }
                                else
                                {
                                    exists = true;
                                    break;
                                }
                            }
                        }

                        if (!exists)
                        {
                            FaultDescription faultDescription = MessageBuilder.CreateFaultDescription(operation, faultType, action);
                            operation.Faults.Add(faultDescription);
                        }
                    }
                }
            }
        }

        public static void AddKnownTypesToOperation(Receive receive, OperationDescription operation)
        {
            Collection<Type> knownTypes = receive.InternalKnownTypes;

            if (knownTypes != null)
            {
                foreach (Type knownType in knownTypes)
                {
                    // We expect the number of known types to be small, so we use iterative comparison 
                    if (!operation.KnownTypes.Contains(knownType))
                    {
                        operation.KnownTypes.Add(knownType);
                    }
                }
            }
        }

        public static void AddReceiveToFormatterBehavior(Receive receive, OperationDescription operation)
        {
            Fx.Assert(receive != null && operation != null, "Argument cannot be null!");

            KeyedByTypeCollection<IOperationBehavior> behaviors = operation.Behaviors;
            WorkflowFormatterBehavior formatterBehavior = behaviors.Find<WorkflowFormatterBehavior>();
            if (formatterBehavior == null)
            {
                formatterBehavior = new WorkflowFormatterBehavior();
                behaviors.Add(formatterBehavior);
            }

            formatterBehavior.Receives.Add(receive);
        }

        public static void RemoveReceiveFromFormatterBehavior(Receive receive, OperationDescription operation)
        {
            Fx.Assert(receive != null && operation != null, "Arguments cannot be null!");

            KeyedByTypeCollection<IOperationBehavior> behaviors = operation.Behaviors;
            WorkflowFormatterBehavior formatterBehavior = behaviors.Find<WorkflowFormatterBehavior>();
            if (formatterBehavior != null)
            {
                formatterBehavior.Receives.Remove(receive);
            }
        }

        public static CorrelationQuery CreateServerCorrelationQuery(MessageQuerySet select, Collection<CorrelationInitializer> correlationInitializers,
            OperationDescription operation, bool isResponse)
        {
            Fx.Assert(operation != null, "Argument cannot be null!");

            CorrelationQuery correlationQuery = CreateCorrelationQueryCore(select, correlationInitializers);

            if (correlationQuery != null)
            {
                string action = !isResponse ? operation.Messages[0].Action : operation.Messages[1].Action;
                correlationQuery.Where = new CorrelationActionMessageFilter { Action = action };
            }

            return correlationQuery;
        }

        // this method generates the correlationQuery for client side send and receiveReply
        public static Collection<CorrelationQuery> CreateClientCorrelationQueries(MessageQuerySet select, Collection<CorrelationInitializer> correlationInitializers,
            string overridingAction, XName serviceContractName, string operationName, bool isResponse)
        {
            Fx.Assert(serviceContractName != null && operationName != null, "Argument cannot be null!");

            Collection<CorrelationQuery> queryCollection = new Collection<CorrelationQuery>();
            CorrelationQuery correlationQuery = CreateCorrelationQueryCore(select, correlationInitializers);

            if (correlationQuery != null)
            {
                if (overridingAction != null)
                {
                    correlationQuery.Where = new CorrelationActionMessageFilter { Action = overridingAction };
                }
                else
                {
                    ProvideDefaultNamespace(ref serviceContractName);
                    string defaultAction = NamingHelper.GetMessageAction(new XmlQualifiedName(serviceContractName.LocalName, serviceContractName.NamespaceName),
                        operationName, null, isResponse);

                    correlationQuery.Where = new CorrelationActionMessageFilter { Action = defaultAction };
                }

                queryCollection.Add(correlationQuery);

                if (isResponse)
                {
                    // we need an additional query with empty action to support soap1.1 reply cases
                    CorrelationQuery noActionQuery = correlationQuery.Clone();
                    noActionQuery.Where = new CorrelationActionMessageFilter { Action = String.Empty };
                    queryCollection.Add(noActionQuery);
                }
            }

            return queryCollection;
        }

        static CorrelationQuery CreateCorrelationQueryCore(MessageQuerySet select, Collection<CorrelationInitializer> correlationInitializers)
        {
            CorrelationQuery correlationQuery = null;

            if (select != null)
            {
                Fx.Assert(select.Count != 0, "Empty MessageQuerySet is not allowed!");

                correlationQuery = new CorrelationQuery
                {
                    Select = select
                };
            }

            if (correlationInitializers != null && correlationInitializers.Count > 0)
            {
                foreach (CorrelationInitializer correlation in correlationInitializers)
                {
                    QueryCorrelationInitializer queryCorrelation = correlation as QueryCorrelationInitializer;
                    if (queryCorrelation != null)
                    {
                        Fx.Assert(queryCorrelation.MessageQuerySet.Count != 0, "Empty MessageQuerySet is not allowed!");

                        correlationQuery = correlationQuery ?? new CorrelationQuery();
                        correlationQuery.SelectAdditional.Add(queryCorrelation.MessageQuerySet);
                    }
                }
            }

            return correlationQuery;
        }
    }
}
