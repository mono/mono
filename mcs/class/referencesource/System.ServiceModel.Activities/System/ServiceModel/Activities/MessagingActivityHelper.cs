//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.Xaml;
    using Microsoft.VisualBasic.Activities;
    using SR2 = System.ServiceModel.Activities.SR;

    static class MessagingActivityHelper
    {
        static Type faultExceptionType = typeof(FaultException);
        static Type faultExceptionGenericType = typeof(FaultException<>);

        public const string ActivityInstanceId = "ActivityInstanceId";
        public const string ActivityName = "ActivityName";
        public const string ActivityType = "ActivityType";
        public const string ActivityTypeExecuteUserCode = "ExecuteUserCode";
        public const string MessagingActivityTypeActivityExecution = "MessagingActivityExecution";
        public const string E2EActivityId = "E2EActivityId";
        public const string MessageId = "MessageId";
        public const string ActivityNameWorkflowOperationInvoke = "WorkflowOperationInvoke";
        public const string MessageCorrelationReceiveRecord = "MessageCorrelationReceiveRecord";
        public const string MessageCorrelationSendRecord = "MessageCorrelationSendRecord";
        
        public static void FixMessageArgument(Argument messageArgument, ArgumentDirection direction, ActivityMetadata metadata)
        {
            Type messageType = (messageArgument == null) ? TypeHelper.ObjectType : messageArgument.ArgumentType;
            AddRuntimeArgument(messageArgument, "Message", messageType, direction, metadata);
        }

        public static void AddRuntimeArgument(Argument messageArgument, string runtimeArgumentName, Type runtimeArgumentType,
                ArgumentDirection runtimeArgumentDirection, ActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument(runtimeArgumentName, runtimeArgumentType, runtimeArgumentDirection);
            metadata.Bind(messageArgument, argument);
            metadata.AddArgument(argument);
        }

        // 
        public static IList<T> GetCallbacks<T>(ExecutionProperties executionProperties)
            where T : class
        {
            List<T> list = null;
            
            if (!executionProperties.IsEmpty)
            {
                T temp;
                foreach (KeyValuePair<string, object> item in executionProperties)
                {
                    temp = item.Value as T;

                    if (temp != null)
                    {
                        if (list == null)
                        {
                            list = new List<T>();
                        }
                        list.Add(temp);
                    }
                }
            }

            return list;
        }

        public static Message InitializeCorrelationHandles(NativeActivityContext context,
            CorrelationHandle selectHandle, CorrelationHandle ambientHandle, Collection<CorrelationInitializer> additionalCorrelations,
            CorrelationKeyCalculator keyCalculator, Message message)
        {
            InstanceKey instanceKey;
            ICollection<InstanceKey> additionalKeys;

            // 
            MessageBuffer buffer = message.CreateBufferedCopy(int.MaxValue);
            if (keyCalculator.CalculateKeys(buffer, message, out instanceKey, out additionalKeys))
            {
                InitializeCorrelationHandles(context, selectHandle, ambientHandle, additionalCorrelations, instanceKey, additionalKeys);
            }
            return buffer.CreateMessage();
        }

        public static void InitializeCorrelationHandles(NativeActivityContext context,
            CorrelationHandle selectHandle, CorrelationHandle ambientHandle, Collection<CorrelationInitializer> additionalCorrelations,
            MessageProperties messageProperties)
        {
            CorrelationMessageProperty correlationMessageProperty;
            if (CorrelationMessageProperty.TryGet(messageProperties, out correlationMessageProperty))
            {
                InitializeCorrelationHandles(context, selectHandle, ambientHandle, additionalCorrelations,
                    correlationMessageProperty.CorrelationKey, correlationMessageProperty.AdditionalKeys);
            }
        }

        
        // both receive and send initialize correlations using this method
        // if selectHandle is not null, we first try to initalize instanceKey with it , else we try to initalize the ambient handle
        // if ambient handle is not used for initializing instance key , we might use it for initalizing  queryCorrelationsInitalizer. 

        // SelectHandle usage:
        // Receive: selectHandle is the correlatesWith handle
        // SendReply: in case of context based correlation, this is the context handle
        // Send: in case of context based correlation, this will be the callback handle 
        // ReceiveReply: selectHandle will be always null
        // Note that only Receive can initialize a content based correlation with a selectHandle (parallel convoy)
        internal static void InitializeCorrelationHandles(NativeActivityContext context,
             CorrelationHandle selectHandle, CorrelationHandle ambientHandle, Collection<CorrelationInitializer> additionalCorrelations,
             InstanceKey instanceKey, ICollection<InstanceKey> additionalKeys)
        {
            bool isAmbientHandleUsed = false;
            if (instanceKey != null && instanceKey.IsValid)
            {
                if (selectHandle != null)
                {
                    selectHandle.InitializeBookmarkScope(context, instanceKey);
                }
                else if (ambientHandle != null)
                {
                    ambientHandle.InitializeBookmarkScope(context, instanceKey);
                    isAmbientHandleUsed = true;
                }
                else if (context.DefaultBookmarkScope.IsInitialized)
                {
                    if (context.DefaultBookmarkScope.Id != instanceKey.Value)
                    {
                        throw FxTrace.Exception.AsError(
                            new InvalidOperationException(SR2.CorrelationHandleInUse(context.DefaultBookmarkScope.Id, instanceKey.Value)));
                    }
                }
                else
                {
                    context.DefaultBookmarkScope.Initialize(context, instanceKey.Value);
                }
            }

            if (additionalKeys != null && additionalCorrelations != null)
            {
                // The ordering of items in SelectAdditional and additional correlations are the same
                // Therefore, we assign keys iteratively

                IEnumerator<CorrelationInitializer> enumerator = additionalCorrelations.GetEnumerator();

                foreach (InstanceKey key in additionalKeys)
                {
                    Fx.Assert(key != null && key.IsValid, "only valid keys should be passed into InitializeCorrelationHandles");

                    while (enumerator.MoveNext())
                    {
                        QueryCorrelationInitializer queryCorrelation = enumerator.Current as QueryCorrelationInitializer;
                        if (queryCorrelation != null)
                        {
                            CorrelationHandle handle = (queryCorrelation.CorrelationHandle != null ? queryCorrelation.CorrelationHandle.Get(context) : null);
                            if (handle == null)
                            {
                                if (ambientHandle != null && !isAmbientHandleUsed)
                                {
                                    handle = ambientHandle;
                                    isAmbientHandleUsed = true;
                                }
                                else
                                {
                                    throw FxTrace.Exception.AsError(
                                        new InvalidOperationException(SR2.QueryCorrelationInitializerCannotBeInitialized));
                                }
                            }
                            handle.InitializeBookmarkScope(context, key);
                            break;
                        }
                    }
                }
            }
        }

        public static CorrelationCallbackContext CreateCorrelationCallbackContext(MessageProperties messageProperties)
        {
            CallbackContextMessageProperty callbackMessageContextProperty;
            if (CallbackContextMessageProperty.TryGet(messageProperties, out callbackMessageContextProperty))
            {
                EndpointAddress listenAddress;
                IDictionary<string, string> context;
                callbackMessageContextProperty.GetListenAddressAndContext(out listenAddress, out context);

                return new CorrelationCallbackContext
                {
                    ListenAddress = EndpointAddress10.FromEndpointAddress(listenAddress),
                    Context = context
                };
            }
            return null;
        }

        public static CorrelationContext CreateCorrelationContext(MessageProperties messageProperties)
        {
            ContextMessageProperty contextMessageProperty;
            if (ContextMessageProperty.TryGet(messageProperties, out contextMessageProperty))
            {
                IDictionary<string, string> context;
                context = contextMessageProperty.Context; 
                return new CorrelationContext
                {
                    Context = context
                };
            }
            return null;
        }

        public static bool CompareContextEquality(IDictionary<string, string> context1, IDictionary<string, string> context2)
        {
            if (context1 != context2)
            {
                if (context1 == null ||
                    context2 == null ||
                    context1.Count != context2.Count)
                {
                    return false;
                }
                foreach (KeyValuePair<string, string> pair in context1)
                {
                    if (!context2.Contains(pair))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static InArgument<CorrelationHandle> CreateReplyCorrelatesWith(InArgument<CorrelationHandle> requestCorrelatesWith)
        {
            Fx.Assert(requestCorrelatesWith != null, "Argument cannot be null!");

            VariableValue<CorrelationHandle> variableValue = requestCorrelatesWith.Expression as VariableValue<CorrelationHandle>;
            if (variableValue != null)
            {
                return new InArgument<CorrelationHandle>(variableValue.Variable);
            }

            VisualBasicValue<CorrelationHandle> vbvalue = requestCorrelatesWith.Expression as VisualBasicValue<CorrelationHandle>;
            if (vbvalue != null)
            {
                return new InArgument<CorrelationHandle>(new VisualBasicValue<CorrelationHandle>(vbvalue.ExpressionText));
            }

            // We use XAML roundtrip to clone expression
            string xamlStr = XamlServices.Save(requestCorrelatesWith.Expression);
            object obj = XamlServices.Parse(xamlStr);

            Activity<CorrelationHandle> expression = obj as Activity<CorrelationHandle>;
            Fx.Assert(expression != null, "Failed to clone CorrelationHandle using XAML roundtrip!");

            return new InArgument<CorrelationHandle>(expression);
               
        }

        public static void ValidateCorrelationInitializer(ActivityMetadata metadata, Collection<CorrelationInitializer> correlationInitializers, bool isReply, string displayName, string operationName)
        {
            Fx.Assert(metadata != null, "cannot be null");
            
            if (correlationInitializers != null && correlationInitializers.Count > 0)
            {
                bool queryInitializerWithEmptyHandle = false;
                foreach (CorrelationInitializer correlation in correlationInitializers)
                {
                    if (correlation is RequestReplyCorrelationInitializer && isReply)
                    {
                        // This is a reply, so additional correlations should not have a request reply handle
                        metadata.AddValidationError(SR.ReplyShouldNotIncludeRequestReplyHandle(displayName, operationName));
                    }

                    QueryCorrelationInitializer queryCorrelation = correlation as QueryCorrelationInitializer;
                    if (queryCorrelation != null)
                    {
                        if (queryCorrelation.MessageQuerySet.Count == 0)
                        {
                            metadata.AddValidationError(SR.QueryCorrelationInitializerWithEmptyMessageQuerySet(displayName, operationName));
                        }
                    }

                    if (correlation.CorrelationHandle == null)
                    {
                        if (correlation is QueryCorrelationInitializer)
                        {
                            if (!queryInitializerWithEmptyHandle)
                            {
                                queryInitializerWithEmptyHandle = true;
                            }
                            else
                            {
                                // more than one queryInitializer present, in this case we don't permit null handle
                                metadata.AddValidationError(SR.NullCorrelationHandleInMultipleQueryCorrelation);
                            }
                        }
                        else
                        {
                            metadata.AddValidationError(SR.NullCorrelationHandleInInitializeCorrelation(correlation.GetType().Name));
                        }
                    }
                }
            }
        }
    }
}
