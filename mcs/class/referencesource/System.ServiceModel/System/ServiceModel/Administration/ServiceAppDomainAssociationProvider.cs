//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System;
    using System.Runtime;

    class ServiceAppDomainAssociationProvider : ProviderBase, IWmiProvider
    {
        void IWmiProvider.EnumInstances(IWmiInstances instances)
        {
            Fx.Assert(null != instances, "");
            foreach (ServiceInfo info in new ServiceInfoCollection(ManagementExtension.Services))
            {
                IWmiInstance instance = instances.NewInstance(null);

                instance.SetProperty(AdministrationStrings.AppDomainInfo, AppDomainInstanceProvider.GetReference());
                instance.SetProperty(AdministrationStrings.Service, ServiceInstanceProvider.GetReference(info));

                instances.AddInstance(instance);
            }
        }

        bool IWmiProvider.GetInstance(IWmiInstance instance)
        {
            Fx.Assert(null != instance, "");
            string serviceRef = instance.GetProperty(AdministrationStrings.Service) as string;
            string appDomainInfoRef = instance.GetProperty(AdministrationStrings.AppDomainInfo) as string;

            return !String.IsNullOrEmpty(serviceRef) && !String.IsNullOrEmpty(appDomainInfoRef);
        }
    }
}
