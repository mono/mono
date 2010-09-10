//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if NET_4_0
using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace System.ServiceModel.Discovery.Configuration
{
	public class DiscoveryEndpointElement : StandardEndpointElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty discovery_mode, discovery_version, max_response_delay;
		
		static DiscoveryEndpointElement ()
		{
			discovery_mode = new ConfigurationProperty ("discoveryMode", typeof (ServiceDiscoveryMode), ServiceDiscoveryMode.Managed, null, null, ConfigurationPropertyOptions.None);
			discovery_version = new ConfigurationProperty ("discoveryVersion", typeof (DiscoveryVersion), "WSDiscovery11", new DiscoveryVersionConverter (), null, ConfigurationPropertyOptions.None);
			max_response_delay = new ConfigurationProperty ("maxResponseDelay", typeof (TimeSpan), "00:00:00", new TimeSpanConverter (), null, ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (discovery_mode);
			properties.Add (discovery_version);
			properties.Add (max_response_delay);
		}

		public DiscoveryEndpointElement ()
		{
		}

		[ConfigurationProperty ("discoveryMode", DefaultValue = ServiceDiscoveryMode.Managed)]
		public ServiceDiscoveryMode DiscoveryMode {
			get { return (ServiceDiscoveryMode) base [discovery_mode]; }
			set { base [discovery_mode] = value; }
		}

		[TypeConverter (typeof (DiscoveryVersionConverter))]
		[ConfigurationProperty ("discoveryVersion", DefaultValue = "WSDiscovery11")]
		public DiscoveryVersion DiscoveryVersion {
			get { return (DiscoveryVersion) base [discovery_version]; }
			set { base [discovery_version] = value; }
		}

		[ConfigurationProperty ("maxResponseDelay", DefaultValue = "00:00:00")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan MaxResponseDelay {
			get { return (TimeSpan) base [max_response_delay]; }
			set { base [max_response_delay] = value; }
		}
		
		protected override Type EndpointType {
			get { return typeof (DiscoveryEndpoint); }
		}
		
		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		protected override ServiceEndpoint CreateServiceEndpoint (ContractDescription contractDescription)
		{
			throw new NotImplementedException ();
		}

		protected override void InitializeFrom (ServiceEndpoint endpoint)
		{
			throw new NotImplementedException ();
		}
		
		protected override void OnApplyConfiguration (ServiceEndpoint endpoint, ChannelEndpointElement serviceEndpointElement)
		{
			throw new NotImplementedException ();
		}
		
		protected override void OnApplyConfiguration (ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement)
		{
			throw new NotImplementedException ();
		}
		
		protected override void OnInitializeAndValidate (ChannelEndpointElement channelEndpointElement)
		{
			// It seems to do nothing.
		}
		
		protected override void OnInitializeAndValidate (ServiceEndpointElement channelEndpointElement)
		{
			// It seems to do nothing.
		}
	}
}

#endif
