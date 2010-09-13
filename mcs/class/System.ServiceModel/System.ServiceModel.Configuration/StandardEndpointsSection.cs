//
// BindingsSection.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
	public sealed class StandardEndpointsSection : ConfigurationSection
	{
		ConfigurationPropertyCollection _properties;

		// Properties

		List<EndpointCollectionElement> endpoint_collections;

		public List<EndpointCollectionElement> EndpointCollections {
			get {
				if (endpoint_collections != null)
					return endpoint_collections;
				var list = new List<EndpointCollectionElement> ();
				foreach (ConfigurationProperty cp in Properties)
					list.Add ((EndpointCollectionElement) this [cp]);
				endpoint_collections = list;
				return list;
			}
		}


		[ConfigurationProperty ("mexEndpoint", Options = ConfigurationPropertyOptions.None)]
		public ServiceMetadataEndpointCollectionElement MexEndpoint {
			get { return (ServiceMetadataEndpointCollectionElement) this ["mexEndpoint"]; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = new ConfigurationPropertyCollection ();
					ExtensionElementCollection extensions = ((ExtensionsSection) EvaluationContext.GetSection ("system.serviceModel/extensions")).EndpointExtensions;
					for (int i = 0; i < extensions.Count; i++) {
						ExtensionElement extension = extensions [i];
						_properties.Add (new ConfigurationProperty (extension.Name, Type.GetType (extension.Type), null, null, null, ConfigurationPropertyOptions.None));
					}
				}
				return _properties;
			}
		}

		public static StandardEndpointsSection GetSection (System.Configuration.Configuration config) {
			ServiceModelSectionGroup sm = ServiceModelSectionGroup.GetSectionGroup (config);
			if (sm == null)
				throw new SystemException ("Could not retrieve configuration section group 'system.serviceModel'");
			if (sm.StandardEndpoints == null)
				throw new SystemException ("Could not retrieve configuration sub section group 'standardEndpoints' in 'system.serviceModel'");
			return sm.StandardEndpoints;
		}

		public new EndpointCollectionElement this [string name] {
			get {
				object element = base [name];
				if (element is EndpointCollectionElement)
					return (EndpointCollectionElement) element;
				throw new ArgumentException (String.Format ("Could not find {0}", name));
			}
		}

	}

}

#endif
