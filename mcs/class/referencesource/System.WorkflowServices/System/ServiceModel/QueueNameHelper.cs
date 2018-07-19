//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.Workflow.Runtime;
    using System.ServiceModel.Channels;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using System.ServiceModel.Dispatcher;
    using System.Workflow.Activities;

    static class QueueNameHelper
    {
        public static string Create(Type contractType, string operationName)
        {
            return Create(contractType.AssemblyQualifiedName, operationName);
        }

        public static string Create(string typeName, string operationName)
        {
            return typeName + "|" + operationName;
        }

        public static string Create(string partialQueueName, IDictionary<string, string> contextProperties)
        {
            if (partialQueueName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("partialQueueName");
            }

            if (contextProperties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextProperties");
            }

            string conversationId = null;

            //Chosen explicit look up against generic looping of Context Headers
            //to mitigate security threat bug PS#3470.
            if (contextProperties.TryGetValue(WellKnownContextProperties.ConversationId, out conversationId))
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(partialQueueName);
                stringBuilder.Append("|");
                stringBuilder.Append(WellKnownContextProperties.ConversationId + ":" + conversationId);
                return stringBuilder.ToString();
            }
            return partialQueueName;
        }
    }
}
