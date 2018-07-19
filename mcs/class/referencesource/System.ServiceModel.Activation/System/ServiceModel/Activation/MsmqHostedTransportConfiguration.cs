//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Diagnostics;
    using System.Web.Hosting;

    class MsmqHostedTransportConfiguration : HostedTransportConfigurationBase
    {
        MsmqHostedTransportManager uniqueManager;        

        public MsmqHostedTransportConfiguration()
          : this(MsmqUri.NetMsmqAddressTranslator)
        {
        }

        protected MsmqHostedTransportConfiguration(MsmqUri.IAddressTranslator addressing)
            : base(addressing.Scheme)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet(); 
            
            string[] bindings = HostedTransportConfigurationManager.MetabaseSettings.GetBindings(addressing.Scheme);
            
            this.uniqueManager = new MsmqHostedTransportManager(bindings, addressing);

            for (int i = 0; i < bindings.Length; i++)
            {
                Uri address = addressing.CreateUri(bindings[i], HostingEnvironment.ApplicationVirtualPath, false);
                this.ListenAddresses.Add(new BaseUriWithWildcard(address, TransportDefaults.HostNameComparisonMode));

                UniqueTransportManagerRegistration registration = new UniqueTransportManagerRegistration(uniqueManager, address, TransportDefaults.HostNameComparisonMode);
                Msmq.StaticTransportManagerTable.RegisterUri(address, TransportDefaults.HostNameComparisonMode, registration);
            }

            this.uniqueManager.Start(null);
        }

        public override Uri[] GetBaseAddresses(string virtualPath)
        {
            return this.uniqueManager.GetBaseAddresses(virtualPath);
        }

        internal MsmqHostedTransportManager TransportManager 
        { 
            get { return uniqueManager; } 
        }
    }

    sealed class MsmqIntegrationHostedTransportConfiguration : MsmqHostedTransportConfiguration
    {
        public MsmqIntegrationHostedTransportConfiguration()
            : base(MsmqUri.FormatNameAddressTranslator)
        {
        }
    }
}

