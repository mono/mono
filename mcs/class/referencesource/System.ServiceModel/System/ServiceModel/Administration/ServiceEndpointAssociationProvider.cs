//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System;
    using System.ServiceModel.Description;
    using System.Diagnostics;
    using System.ServiceModel.Channels;
    using System.Globalization;

    internal class ServiceEndpointAssociationProvider : ProviderBase, IWmiProvider
    {
        void IWmiProvider.EnumInstances(IWmiInstances instances)
        {
            foreach (ServiceInfo info in new ServiceInfoCollection(ManagementExtension.Services))
            {
                string serviceRef = ServiceInstanceProvider.GetReference(info);

                foreach (EndpointInfo endpointInfo in info.Endpoints)
                {
                    IWmiInstance instance = instances.NewInstance(null);

                    string endpointRef = EndpointInstanceProvider.EndpointReference(endpointInfo.ListenUri, endpointInfo.Contract.Name);
                    instance.SetProperty(AdministrationStrings.Endpoint, endpointRef);
                    instance.SetProperty(AdministrationStrings.Service, serviceRef);

                    instances.AddInstance(instance);
                }
            }
        }

        bool IWmiProvider.GetInstance(IWmiInstance instance)
        {
            string serviceRef = instance.GetProperty(AdministrationStrings.Service) as string;
            string endpointRef = instance.GetProperty(AdministrationStrings.Endpoint) as string;

            return !String.IsNullOrEmpty(serviceRef) && !String.IsNullOrEmpty(endpointRef);
        }
    }
}
