//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Activities.Activation
{
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUninstantiatedInternalClasses,
        Justification = "This is instantiated by AspNet.")]
    class ServiceModelActivitiesActivationHandler : HttpHandler, IServiceModelActivationHandler
    {
        public ServiceHostFactoryBase GetFactory()
        {
            return new WorkflowServiceHostFactory();
        }
    }
}
