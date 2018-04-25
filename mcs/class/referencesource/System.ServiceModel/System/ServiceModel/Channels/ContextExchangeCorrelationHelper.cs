//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel.Dispatcher;

    static class ContextExchangeCorrelationHelper
    {
        public static string CorrelationName = "wsc-instanceId";

        public static void AddIncomingContextCorrelationData(Message message)
        {
            CorrelationDataMessageProperty.AddData(message, CorrelationName, () => GetContextCorrelationData(message));
        }

        public static void AddOutgoingCorrelationCallbackData(CorrelationCallbackMessageProperty callback, Message message, bool client)
        {
            if (client)
            {
                callback.AddData(CorrelationName, () => GetCallbackContextCorrelationData(message));
            }
            else
            {
                callback.AddData(CorrelationName, () => GetContextCorrelationData(message));
            }
        }

        public static string GetContextCorrelationData(Message message)
        {
            ContextMessageProperty contextProperties = null;
            string instanceId = null;

            if (ContextMessageProperty.TryGet(message, out contextProperties))
            {
                contextProperties.Context.TryGetValue(ContextMessageProperty.InstanceIdKey, out instanceId);
            }

            return instanceId ?? string.Empty;
        }

        public static string GetContextCorrelationData(OperationContext operationContext)
        {
            ContextMessageProperty contextProperties = null;
            string instanceId = null;

            if (ContextMessageProperty.TryGet(operationContext.OutgoingMessageProperties, out contextProperties))
            {
                contextProperties.Context.TryGetValue(ContextMessageProperty.InstanceIdKey, out instanceId);
            }

            return instanceId ?? string.Empty;
        }

        public static string GetCallbackContextCorrelationData(Message message)
        {
            CallbackContextMessageProperty callbackContext;
            string instanceId = null;

            if (CallbackContextMessageProperty.TryGet(message, out callbackContext))
            {
                IDictionary<string, string> context;
                context = callbackContext.Context;
                if (context != null)
                {
                    context.TryGetValue(ContextMessageProperty.InstanceIdKey, out instanceId);
                }
            }

            return instanceId ?? string.Empty;
        }
    }
}
