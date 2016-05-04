//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;

    sealed class ManagementExtension
    {
        static Dictionary<ServiceHostBase, DateTime> services;
        static bool activated = false;
        static object syncRoot = new object();
        static bool isEnabled = GetIsWmiProviderEnabled();

        internal static bool IsActivated
        {
            get { return ManagementExtension.activated; }
        }

        internal static bool IsEnabled
        {
            get { return ManagementExtension.isEnabled; }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls critical method DiagnosticSection.UnsafeGetSection.",
            Safe = "Doesn't leak config section instance, just returns a bool.")]
        [SecuritySafeCritical]
        static bool GetIsWmiProviderEnabled()
        {
            return DiagnosticSection.UnsafeGetSection().WmiProviderEnabled;
        }

        static Dictionary<ServiceHostBase, DateTime> GetServices()
        {
            if (ManagementExtension.services == null)
            {
                lock (ManagementExtension.syncRoot)
                {
                    if (ManagementExtension.services == null)
                    {
                        ManagementExtension.services = new Dictionary<ServiceHostBase, DateTime>();
                    }
                }
            }
            return ManagementExtension.services;
        }

        internal static ICollection<ServiceHostBase> Services
        {
            get
            {
                return GetServices().Keys;
            }
        }

        internal static DateTime GetTimeOpened(ServiceHostBase service)
        {
            return GetServices()[service];
        }

        public static void OnServiceOpened(ServiceHostBase serviceHostBase)
        {
            EnsureManagementProvider();
            Add(serviceHostBase);
        }

        public static void OnServiceClosing(ServiceHostBase serviceHostBase)
        {
            Remove(serviceHostBase);
        }

        static void Add(ServiceHostBase service)
        {
            Dictionary<ServiceHostBase, DateTime> services = GetServices();
            lock (services)
            {
                if (!services.ContainsKey(service))
                {
                    services.Add(service, DateTime.Now);
                }
            }
        }

        static void Remove(ServiceHostBase service)
        {
            Dictionary<ServiceHostBase, DateTime> services = GetServices();
            lock (services)
            {
                if (services.ContainsKey(service))
                {
                    services.Remove(service);
                }
            }
        }

        static void EnsureManagementProvider()
        {
            if (!ManagementExtension.activated)
            {
                lock (ManagementExtension.syncRoot)
                {
                    if (!ManagementExtension.activated)
                    {
                        Activate();
                        ManagementExtension.activated = true;
                    }
                }
            }
        }

        static void Activate()
        {
            WbemProvider wmi = new WbemProvider(AdministrationStrings.IndigoNamespace, AdministrationStrings.IndigoAppName);
            wmi.Register(AdministrationStrings.AppDomainInfo, new AppDomainInstanceProvider());
            wmi.Register(AdministrationStrings.Service, new ServiceInstanceProvider());
            wmi.Register(AdministrationStrings.Contract, new ContractInstanceProvider());
            wmi.Register(AdministrationStrings.Endpoint, new EndpointInstanceProvider());
            wmi.Register(AdministrationStrings.ServiceAppDomain, new ServiceAppDomainAssociationProvider());
            wmi.Register(AdministrationStrings.ServiceToEndpointAssociation, new ServiceEndpointAssociationProvider());
        }
    }
}
