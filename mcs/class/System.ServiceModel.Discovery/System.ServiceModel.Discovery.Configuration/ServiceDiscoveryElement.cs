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
	public sealed class ServiceDiscoveryElement : BehaviorExtensionElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty announcement_endpoints;
		
		static ServiceDiscoveryElement ()
		{
			announcement_endpoints = new ConfigurationProperty ("announcementEndpoints", typeof (AnnouncementChannelEndpointElementCollection), null, null, null, ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (announcement_endpoints);
		}

		public ServiceDiscoveryElement ()
		{
		}

		[ConfigurationProperty ("announcementEndpoints")]
		public AnnouncementChannelEndpointElementCollection AnnouncementEndpoints {
			get { return (AnnouncementChannelEndpointElementCollection) base [announcement_endpoints]; }
		}
		public override Type BehaviorType {
			get { return typeof (ServiceDiscoveryBehavior); }
		}
		
		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
		
		protected internal override object CreateBehavior ()
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
