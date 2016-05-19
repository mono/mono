//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System.Collections;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Security;
    using System.Runtime;

    class HttpHostedTransportConfiguration : HostedTransportConfigurationBase
    {
        Collection<HostedHttpTransportManager> transportManagerDirectory;

        internal protected HttpHostedTransportConfiguration(string scheme)
            : base(scheme)
        {
            CreateTransportManagers();
        }

        internal HttpHostedTransportConfiguration()
            : this(Uri.UriSchemeHttp)
        { }

        HostedHttpTransportManager CreateTransportManager(BaseUriWithWildcard listenAddress)
        {
            UriPrefixTable<ITransportManagerRegistration> table = null;
            if (object.ReferenceEquals(this.Scheme, Uri.UriSchemeHttp))
            {
                table = HttpChannelListener.StaticTransportManagerTable;
            }
            else
            {
                table = SharedHttpsTransportManager.StaticTransportManagerTable;
            }

            HostedHttpTransportManager httpManager = null;
            lock (table)
            {
                ITransportManagerRegistration registration;
                if (!table.TryLookupUri(listenAddress.BaseAddress, listenAddress.HostNameComparisonMode, out registration))
                {
                    httpManager = new HostedHttpTransportManager(listenAddress);
                    table.RegisterUri(listenAddress.BaseAddress, listenAddress.HostNameComparisonMode, httpManager);
                }
            }

            return httpManager;
        }

        void CreateTransportManagers()
        {
            Collection<HostedHttpTransportManager> tempDirectory = new Collection<HostedHttpTransportManager>();
            string[] bindings = HostedTransportConfigurationManager.MetabaseSettings.GetBindings(this.Scheme);

            foreach (string binding in bindings)
            {
                TryDebugPrint("HttpHostedTransportConfiguration.CreateTransportManagers() adding binding: " + binding);
                BaseUriWithWildcard listenAddress = BaseUriWithWildcard.CreateHostedUri(this.Scheme, binding, HostingEnvironmentWrapper.ApplicationVirtualPath);

                bool done = false;
                if (ServiceHostingEnvironment.MultipleSiteBindingsEnabled)
                {
                    //In this specific mode we only create one transport manager and all the 
                    //hosted channel listeners hang off of this transport manager
                    listenAddress = new BaseUriWithWildcard(listenAddress.BaseAddress, HostNameComparisonMode.WeakWildcard);
                    done = true;
                }

                HostedHttpTransportManager httpManager = CreateTransportManager(listenAddress);

                //httpManager will be null when 2 site bindings differ only in ip address
                if (httpManager != null)
                {
                    tempDirectory.Add(httpManager);
                    ListenAddresses.Add(listenAddress);
                }

                if (done)
                {
                    break;
                }
            }

            transportManagerDirectory = tempDirectory;
        }

        static bool canDebugPrint = true;

        [Conditional("DEBUG")]
        static void TryDebugPrint(string message)
        {
            if (canDebugPrint)
            {
                try
                {
                    Debug.Print(message);
                }
                catch (SecurityException e)
                {
                    canDebugPrint = false;

                    // not re-throwing on purpose
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                }
            }
        }

        internal HostedHttpTransportManager GetHttpTransportManager(Uri uri)
        {
            if (ServiceHostingEnvironment.MultipleSiteBindingsEnabled)
            {
                Fx.Assert(transportManagerDirectory.Count == 1, "There should be only one TM in this mode");
                return transportManagerDirectory[0];
            }

            // Optimized common cases without having to create an enumerator.
            switch (this.transportManagerDirectory.Count)
            {
                case 0:
                    return null;

                case 1:
                    {
                        HostedHttpTransportManager manager = this.transportManagerDirectory[0];

                        if (manager.Port == uri.Port &&
                            (string.Compare(manager.Scheme, uri.Scheme, StringComparison.OrdinalIgnoreCase) == 0) &&
                            (manager.HostNameComparisonMode != HostNameComparisonMode.Exact ||
                            string.Compare(manager.ListenUri.Host, uri.NormalizedHost(), StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            return manager;
                        }
                        return null;
                    }

                default:
                    {
                        HostedHttpTransportManager foundTransportManager = null;
                        HostedHttpTransportManager weakTransportManager = null;

                        string scheme = uri.Scheme;
                        int port = uri.Port;
                        string host = null;

                        foreach (HostedHttpTransportManager manager in this.transportManagerDirectory)
                        {
                            if (manager.Port == port &&
                                string.Compare(manager.Scheme, scheme, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                if (manager.HostNameComparisonMode == HostNameComparisonMode.StrongWildcard)
                                {
                                    return manager;
                                }

                                if (manager.HostNameComparisonMode == HostNameComparisonMode.WeakWildcard)
                                {
                                    weakTransportManager = manager;
                                }

                                if ((manager.HostNameComparisonMode == HostNameComparisonMode.Exact) &&
                                    (string.Compare(manager.Host, host ?? (host = uri.NormalizedHost()),
                                    StringComparison.OrdinalIgnoreCase) == 0))
                                {
                                    foundTransportManager = manager;
                                }
                            }
                        }

                        return foundTransportManager ?? weakTransportManager;
                    }
            }
        }
    }
}
