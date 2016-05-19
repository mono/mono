//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Description
{
    using System.Activities.Statements;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Runtime.DurableInstancing;
    using System.Collections.Generic;
    using System.Threading;
    using System.ServiceModel.Diagnostics;
    using System.Activities;

    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public class WorkflowRuntimeEndpoint : WorkflowHostingEndpoint
    {
        static readonly Uri baseUri = new Uri(string.Format(CultureInfo.InvariantCulture, "net.pipe://localhost/ExternalDataExchangeEndpoint/{0}/{1}",
            Process.GetCurrentProcess().Id,
            AppDomain.CurrentDomain.Id));

        static int uriCounter = 0;
        internal static readonly Binding netNamedPipeContextBinding = new CustomBinding(new ContextBindingElement(),
                new BinaryMessageEncodingBindingElement(),
                new NamedPipeTransportBindingElement()) { Name = "ExternalDataExchangeBinding" };

        WorkflowRuntimeServicesBehavior behavior;

        internal const string ExternalDataExchangeNamespace = "http://wf.microsoft.com/externaldataexchange/";
        internal const string RaiseEventAction = "http://wf.microsoft.com/externaldataexchange/IExternalDataExchange/RaiseEvent";

        public WorkflowRuntimeEndpoint()
            : base(typeof(IExternalDataExchange))
        {          
            base.Binding = netNamedPipeContextBinding;
            string endpointUri = String.Format(CultureInfo.InvariantCulture, "{0}/{1}", baseUri, Interlocked.Increment(ref uriCounter));
            base.Address = new EndpointAddress(endpointUri);
            this.behavior = new WorkflowRuntimeServicesBehavior();
            this.Behaviors.Add(behavior);
        }

        protected override Guid OnGetInstanceId(object[] inputs, OperationContext operationContext)
        {
            Fx.Assert(operationContext.IncomingMessageHeaders.Action == RaiseEventAction, "Message action is not RaiseEvent");
            Guid instanceId = Guid.Empty;
            ContextMessageProperty contextMessageProperty;
            if (ContextMessageProperty.TryGet(operationContext.IncomingMessageProperties, out contextMessageProperty))
            {
                string stringInstanceId = null;
                if (contextMessageProperty.Context.TryGetValue("instanceId", out stringInstanceId))
                {
                    Fx.TryCreateGuid(stringInstanceId, out instanceId);                    
                }
            }

            return instanceId;
        }

        protected override Bookmark OnResolveBookmark(object[] inputs, OperationContext operationContext, WorkflowHostingResponseContext responseContext, out object value)
        {
            Fx.Assert(operationContext.IncomingMessageHeaders.Action == RaiseEventAction, "Message action is not RaiseEvent");

            Fx.Assert(inputs.Length >= 3, "Insufficient number of inputs");

            Fx.Assert(inputs[1] is IComparable, "The queue name from ExternalDataExchangeService is not an IComparable object");
            IComparable queueName = (IComparable)inputs[1];
           
            value = inputs[2];
            responseContext.SendResponse(null, null);
            return new Bookmark(queueName.ToString());
        }

        public void AddService(object service)
        {
            behavior.AddService(service);
        }

        public void RemoveService(object service)
        {
            behavior.RemoveService(service);
        }

        public object GetService(Type serviceType)
        {
            return behavior.GetService(serviceType);
        }

        public T GetService<T>()
        {
            return behavior.GetService<T>();
        }
    }
}
