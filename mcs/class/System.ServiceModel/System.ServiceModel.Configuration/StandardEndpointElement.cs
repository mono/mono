//
// StandardEndpointElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.MsmqIntegration;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Security;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Configuration
{
	public abstract class StandardEndpointElement : ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty name = new ConfigurationProperty ("name",
				typeof (string), null, null, new StringValidator (0),
				ConfigurationPropertyOptions.IsKey);

		static StandardEndpointElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			properties.Add (name);
		}
		
		[StringValidator (MinLength = 0)]
		[ConfigurationProperty ("name", Options = ConfigurationPropertyOptions.IsKey)]
		public string Name {
			get { return (string) base [name]; }
			set { base [name] = value; }
		}

		protected internal abstract Type EndpointType { get; }
		
		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public void ApplyConfiguration (ServiceEndpoint endpoint, ChannelEndpointElement channelEndpointElement)
		{
			OnApplyConfiguration (endpoint, channelEndpointElement);
		}

		public void ApplyConfiguration (ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement)
		{
			OnApplyConfiguration (endpoint, serviceEndpointElement);
		}

		protected internal abstract ServiceEndpoint CreateServiceEndpoint (ContractDescription contractDescription);

		public void InitializeAndValidate (ChannelEndpointElement channelEndpointElement)
		{
			OnInitializeAndValidate (channelEndpointElement);
		}

		public void InitializeAndValidate (ServiceEndpointElement serviceEndpointElement)
		{
			OnInitializeAndValidate (serviceEndpointElement);
		}

		protected internal virtual void InitializeFrom (ServiceEndpoint endpoint)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");
			if (!EndpointType.IsAssignableFrom (endpoint.GetType ()))
				throw new ArgumentNullException (String.Format ("Argument endpoint type is not of expected type '{0}'", EndpointType));

			// not sure if that's all, but that's what is documented.
		}

		protected abstract void OnApplyConfiguration (ServiceEndpoint endpoint, ChannelEndpointElement channelEndpointElement);

		protected abstract void OnApplyConfiguration (ServiceEndpoint endpoint, ServiceEndpointElement channelEndpointElement);

		protected abstract void OnInitializeAndValidate (ChannelEndpointElement channelEndpointElement);

		protected abstract void OnInitializeAndValidate (ServiceEndpointElement channelEndpointElement);

		protected override void Reset (ConfigurationElement parentElement)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
