//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Workflow.Activities;

    class WorkflowRuntimeServicesExtensionProvider
    {
        Dictionary<Type, object> services;

        public WorkflowRuntimeServicesExtensionProvider()
        {
            this.services = new Dictionary<Type, object>();
        }

        public void AddService(object service)
        {
            Fx.Assert(service != null, "This should have been checked by our caller.");

            this.services.Add(service.GetType(), service);
        }

        public void RemoveService(object service)
        {
            Fx.Assert(service != null, "This should have been checked by our caller.");

            this.services.Remove(service.GetType());
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "APTCA related issues are not relevant because this code path is not supported in partial trust.")]
        public object GetService(Type serviceType)
        {
            object service;
            if (!this.services.TryGetValue(serviceType, out service))
            {
                object dataExchangeService;
                if (this.services.TryGetValue(typeof(ExternalDataExchangeService), out dataExchangeService))
                {
                    Fx.Assert(dataExchangeService is ExternalDataExchangeService, "Something went wrong with our housekeeping.");

                    service = ((ExternalDataExchangeService)dataExchangeService).GetService(serviceType);
                }
            }

            return service;
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        internal void PopulateExtensions(WorkflowServiceHost host, string baseUri)
        {
            Fx.Assert(host != null, "WorkflowServiceHost parameter was null");

            foreach (object service in this.services.Values)
            {
                host.WorkflowExtensions.Add(service);

                ExternalDataExchangeService dataExchangeService = service as ExternalDataExchangeService;
                if (dataExchangeService != null)
                {
                    dataExchangeService.SetEnqueueMessageWrapper(new WorkflowClientDeliverMessageWrapper(baseUri));

                    foreach (object innerService in dataExchangeService.GetAllServices())
                    {
                        host.WorkflowExtensions.Add(innerService);
                    }
                }
            }
        }
    }
}
