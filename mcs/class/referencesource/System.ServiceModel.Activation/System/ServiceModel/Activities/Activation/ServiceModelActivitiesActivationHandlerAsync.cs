//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Activities.Activation
{
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Xaml.Hosting;

    [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUninstantiatedInternalClasses,
        Justification = "This is instantiated by AspNet.")]
    class ServiceModelActivitiesActivationHandlerAsync : ServiceHttpHandlerFactory, IServiceModelActivationHandler, IXamlBuildProviderExtensionFactory
    {
        public ServiceHostFactoryBase GetFactory()
        {
            return new WorkflowServiceHostFactory();
        }

        public IXamlBuildProviderExtension GetXamlBuildProviderExtension()
        {
            return new XamlBuildProviderExtension();
        }
    }
}
