//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation.Factories
{
    using System.Activities;
    using System.Activities.Presentation;
    using System.Activities.Statements;
    using System.ServiceModel.Activities;
    using System.Windows;
    using System.Xml.Linq;
    using Microsoft.VisualBasic.Activities;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Services;
    using System.Activities.Expressions;

    public sealed class SendAndReceiveReplyFactory : IActivityTemplateFactory
    {
        const string correlationHandleNamePrefix = "__handle";
        static string requiredAssemblyName = typeof(CorrelationHandle).Assembly.GetName().Name;
        static string requiredNamespace = typeof(CorrelationHandle).Namespace;

        public Activity Create(DependencyObject target)
        {
            string correlationHandleName = ActivityDesignerHelper.GenerateUniqueVariableNameForContext(target, correlationHandleNamePrefix);

            Variable<CorrelationHandle> requestReplyCorrelation = new Variable<CorrelationHandle> { Name = correlationHandleName };
           
            Send send = new Send
            {
                OperationName = "Operation1",
                ServiceContractName = XName.Get("IService", "http://tempuri.org/"),
                CorrelationInitializers =
                {
                    new RequestReplyCorrelationInitializer
                    {
                        CorrelationHandle = new VariableValue<CorrelationHandle> { Variable = requestReplyCorrelation }
                    }
                }
            };

            Sequence sequence = new Sequence()
            {
                Variables = { requestReplyCorrelation },
                Activities =
                {
                    send,
                    new ReceiveReply
                    {      
                        DisplayName = "ReceiveReplyForSend",
                        Request = send,
                    },
                }
            };
            return sequence;
        }
    }
}
