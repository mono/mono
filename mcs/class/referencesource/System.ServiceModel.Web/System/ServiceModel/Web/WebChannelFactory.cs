//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Web
{
    using System.IO;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Diagnostics.CodeAnalysis;
    using System.Configuration;
    using System.Net;
    using System.Globalization;

    public class WebChannelFactory<TChannel> : ChannelFactory<TChannel>
        where TChannel : class
    {
        public WebChannelFactory()
            : base()
        {
        }

        public WebChannelFactory(Binding binding)
            : base(binding)
        {
        }

        public WebChannelFactory(ServiceEndpoint endpoint) :
            base(endpoint)
        {
        }


        [SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads", Justification = "This is a configuration string and not a network location")]
        public WebChannelFactory(string endpointConfigurationName)
            : base(endpointConfigurationName)
        {
        }

        public WebChannelFactory(Type channelType)
            : base(channelType)
        {
        }

        public WebChannelFactory(Uri remoteAddress)
            : this(GetDefaultBinding(remoteAddress), remoteAddress)
        {
        }

        public WebChannelFactory(Binding binding, Uri remoteAddress)
            : base(binding, (remoteAddress != null) ? new EndpointAddress(remoteAddress) : null)
        {
        }

        public WebChannelFactory(string endpointConfigurationName, Uri remoteAddress)
            : base(endpointConfigurationName, (remoteAddress != null) ? new EndpointAddress(remoteAddress) : null)
        {
        }

        protected override void OnOpening()
        {
            if (this.Endpoint == null)
            {
                return;
            }

            // if the binding is missing, set up a default binding
            if (this.Endpoint.Binding == null && this.Endpoint.Address != null)
            {
                this.Endpoint.Binding = GetDefaultBinding(this.Endpoint.Address.Uri);
            }
            WebServiceHost.SetRawContentTypeMapperIfNecessary(this.Endpoint, false);
            if (this.Endpoint.Behaviors.Find<WebHttpBehavior>() == null)
            {
                this.Endpoint.Behaviors.Add(new WebHttpBehavior());
            }
            base.OnOpening();
        }

        static Binding GetDefaultBinding(Uri remoteAddress)
        {
            if (remoteAddress == null || (remoteAddress.Scheme != Uri.UriSchemeHttp && remoteAddress.Scheme != Uri.UriSchemeHttps))
            {
                return null;
            }
            if (remoteAddress.Scheme == Uri.UriSchemeHttp)
            {
                return new WebHttpBinding();
            }
            else
            {
                WebHttpBinding result = new WebHttpBinding();
                result.Security.Mode = WebHttpSecurityMode.Transport;
                result.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                return result;
            }
        }
    }
}
