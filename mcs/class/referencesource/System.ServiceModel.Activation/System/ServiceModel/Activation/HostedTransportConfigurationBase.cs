//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

using System.Collections.Generic;

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Globalization;
using System.Web.Hosting;
using System.Web;

namespace System.ServiceModel.Activation
{
    abstract class HostedTransportConfigurationBase : HostedTransportConfiguration
    {
        List<BaseUriWithWildcard> listenAddresses;
        string scheme;

        internal protected HostedTransportConfigurationBase(string scheme)
        {
            this.scheme = scheme;
            this.listenAddresses = new List<BaseUriWithWildcard>();
        }

        internal string Scheme
        {
            get
            {
                return scheme;
            }
        }

        internal protected IList<BaseUriWithWildcard> ListenAddresses
        {
            get
            {
                return listenAddresses;
            }
        }

        public override Uri[] GetBaseAddresses(string virtualPath)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            Uri[] addresses = new Uri[listenAddresses.Count];
            for (int i = 0; i < listenAddresses.Count; i++)
            {
                string absoluteVirtualPath = VirtualPathUtility.ToAbsolute(virtualPath, HostingEnvironmentWrapper.ApplicationVirtualPath);
                addresses[i] = new Uri(listenAddresses[i].BaseAddress, absoluteVirtualPath);
            }

            return addresses;
        }

        internal BaseUriWithWildcard FindBaseAddress(Uri uri)
        {
            BaseUriWithWildcard foundBaseAddress = null;
            BaseUriWithWildcard weakBaseAddress = null;

            for (int i = 0; i < listenAddresses.Count; i++)
            {
                if ((string.Compare(listenAddresses[i].BaseAddress.Scheme, uri.Scheme, StringComparison.OrdinalIgnoreCase) == 0)
                    && (listenAddresses[i].BaseAddress.Port == uri.Port))
                {
                    if (listenAddresses[i].HostNameComparisonMode == HostNameComparisonMode.StrongWildcard)
                    {
                        return listenAddresses[i];
                    }

                    if (listenAddresses[i].HostNameComparisonMode == HostNameComparisonMode.WeakWildcard)
                    {
                        weakBaseAddress = listenAddresses[i];
                    }

                    if ((listenAddresses[i].HostNameComparisonMode == HostNameComparisonMode.Exact)
                        && (string.Compare(listenAddresses[i].BaseAddress.Host, uri.Host, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        foundBaseAddress = listenAddresses[i];
                    }
                }
            }

            if (foundBaseAddress == null)
                foundBaseAddress = weakBaseAddress;

            return foundBaseAddress;
        }
    }
}
