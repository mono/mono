//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.ServiceModel.XamlIntegration;
    using System.Xml.Linq;
    using SMASR = System.ServiceModel.Activities.SR;

    public class Endpoint
    {
        Collection<AddressHeader> headers;

        [DefaultValue(null)]
        public string BehaviorConfigurationName
        {
            get;
            set;
        }

        [Fx.Tag.KnownXamlExternal]
        [DefaultValue(null)]
        public Binding Binding
        {
            get;
            set;
        }

        [DefaultValue(null)]
        [TypeConverter(typeof(ServiceXNameTypeConverter))]
        public XName ServiceContractName
        {
            get;
            set;
        }

        // concrete AddressHeader descendants aren't currently XAMLable, they are not initialized until runtime
        // If user adds an address header, this object will fail to xamlize. 
        [Fx.Tag.KnownXamlExternal]
        public Collection<AddressHeader> Headers
        {
            get
            {
                if (this.headers == null)
                {
                    this.headers = new Collection<AddressHeader>();
                }
                return this.headers;
            }
        }

        [DefaultValue(null)]
        [TypeConverter(typeof(EndpointIdentityConverter))]
        public EndpointIdentity Identity
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public Uri ListenUri
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public string Name
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public Uri AddressUri
        {
            get;
            set;
        }

        public EndpointAddress GetAddress()
        {
            return GetAddress(null);
        }

        public EndpointAddress GetAddress(ServiceHostBase host)
        {
            if (this.AddressUri == null)
            {
                string endpointName = ContractValidationHelper.GetErrorMessageEndpointName(this.Name);
                string contractName = ContractValidationHelper.GetErrorMessageEndpointServiceContractName(this.ServiceContractName);
                throw FxTrace.Exception.AsError(new InvalidOperationException(
                    SMASR.MissingUriInEndpoint(endpointName, contractName)));
            }

            Uri address = null;
            if (this.AddressUri.IsAbsoluteUri)
            {
                address = this.AddressUri;
            }
            else
            {
                if (this.Binding == null)
                {
                    string endpointName = ContractValidationHelper.GetErrorMessageEndpointName(this.Name);
                    string contractName = ContractValidationHelper.GetErrorMessageEndpointServiceContractName(this.ServiceContractName);
                    throw FxTrace.Exception.AsError(new InvalidOperationException(
                        SMASR.RelativeUriRequiresBinding(endpointName, contractName, this.AddressUri)));
                }
                if (host == null)
                {
                    string endpointName = ContractValidationHelper.GetErrorMessageEndpointName(this.Name);
                    string contractName = ContractValidationHelper.GetErrorMessageEndpointServiceContractName(this.ServiceContractName);
                    throw FxTrace.Exception.AsError(new InvalidOperationException(
                        SMASR.RelativeUriRequiresHost(endpointName, contractName, this.AddressUri)));
                }
                address = host.MakeAbsoluteUri(this.AddressUri, this.Binding);
            }

            return new EndpointAddress(address, this.Identity, new AddressHeaderCollection(this.Headers));
        }
    }
}
