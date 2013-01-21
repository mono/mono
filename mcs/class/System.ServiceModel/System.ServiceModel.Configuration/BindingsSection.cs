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
	public sealed class BindingsSection
		 : ConfigurationSection
	{
		ConfigurationPropertyCollection _properties;
		List<BindingCollectionElement> _collections;

		// Properties

		[ConfigurationProperty ("basicHttpBinding",
			 Options = ConfigurationPropertyOptions.None)]
		public BasicHttpBindingCollectionElement BasicHttpBinding {
			get { return (BasicHttpBindingCollectionElement) this ["basicHttpBinding"]; }
		}

#if NET_4_5
		[ConfigurationProperty ("basicHttpsBinding",
		                        Options = ConfigurationPropertyOptions.None)]
		public BasicHttpsBindingCollectionElement BasicHttpsBinding {
			get { return (BasicHttpsBindingCollectionElement) this ["basicHttpsBinding"]; }
		}
#endif

		public List<BindingCollectionElement> BindingCollections {
			get {
				if (_collections != null)
					return _collections;
				_collections = new List<BindingCollectionElement> ();
				foreach (PropertyInformation prop in ElementInformation.Properties) {
					var element = prop.Value as BindingCollectionElement;
					if (element != null)
						_collections.Add (element);
				}
				return _collections;
			}
		}

		[ConfigurationProperty ("customBinding",
			 Options = ConfigurationPropertyOptions.None)]
		public CustomBindingCollectionElement CustomBinding {
			get { return (CustomBindingCollectionElement) this ["customBinding"]; }
		}

		[ConfigurationProperty ("msmqIntegrationBinding",
			 Options = ConfigurationPropertyOptions.None)]
		public MsmqIntegrationBindingCollectionElement MsmqIntegrationBinding {
			get { return (MsmqIntegrationBindingCollectionElement) this ["msmqIntegrationBinding"]; }
		}

		[ConfigurationProperty ("netMsmqBinding",
			 Options = ConfigurationPropertyOptions.None)]
		public NetMsmqBindingCollectionElement NetMsmqBinding {
			get { return (NetMsmqBindingCollectionElement) this ["netMsmqBinding"]; }
		}

		[ConfigurationProperty ("netNamedPipeBinding",
			 Options = ConfigurationPropertyOptions.None)]
		public NetNamedPipeBindingCollectionElement NetNamedPipeBinding {
			get { return (NetNamedPipeBindingCollectionElement) this ["netNamedPipeBinding"]; }
		}

		[ConfigurationProperty ("netPeerTcpBinding",
			 Options = ConfigurationPropertyOptions.None)]
		public NetPeerTcpBindingCollectionElement NetPeerTcpBinding {
			get { return (NetPeerTcpBindingCollectionElement) this ["netPeerTcpBinding"]; }
		}

		[ConfigurationProperty ("netTcpBinding",
			 Options = ConfigurationPropertyOptions.None)]
		public NetTcpBindingCollectionElement NetTcpBinding {
			get { return (NetTcpBindingCollectionElement) this ["netTcpBinding"]; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = new ConfigurationPropertyCollection ();
					ExtensionElementCollection extensions = ((ExtensionsSection) EvaluationContext.GetSection ("system.serviceModel/extensions")).BindingExtensions;
					for (int i = 0; i < extensions.Count; i++) {
						ExtensionElement extension = extensions [i];
						_properties.Add (new ConfigurationProperty (extension.Name, Type.GetType (extension.Type), null, null, null, ConfigurationPropertyOptions.None));
					}
				}
				return _properties;
			}
		}

		[ConfigurationProperty ("wsDualHttpBinding",
			 Options = ConfigurationPropertyOptions.None)]
		public WSDualHttpBindingCollectionElement WSDualHttpBinding {
			get { return (WSDualHttpBindingCollectionElement) this ["wsDualHttpBinding"]; }
		}

		[ConfigurationProperty ("wsFederationHttpBinding",
			 Options = ConfigurationPropertyOptions.None)]
		public WSFederationHttpBindingCollectionElement WSFederationHttpBinding {
			get { return (WSFederationHttpBindingCollectionElement) this ["wsFederationHttpBinding"]; }
		}

		[ConfigurationProperty ("wsHttpBinding",
			 Options = ConfigurationPropertyOptions.None)]
		public WSHttpBindingCollectionElement WSHttpBinding {
			get { return (WSHttpBindingCollectionElement) this ["wsHttpBinding"]; }
		}

		public static BindingsSection GetSection (System.Configuration.Configuration config) {
			ServiceModelSectionGroup sm = ServiceModelSectionGroup.GetSectionGroup (config);
			if (sm == null)
				throw new SystemException ("Could not retrieve configuration section group 'system.serviceModel'");
			if (sm.Bindings == null)
				throw new SystemException ("Could not retrieve configuration sub section group 'bindings' in 'system.serviceModel'");
			return sm.Bindings;
		}

		public new BindingCollectionElement this [string name] {
			get {
				object element = base [name];
				if (element is BindingCollectionElement)
					return (BindingCollectionElement) element;
				throw new NotImplementedException (String.Format ("Could not find {0}", name));
			}
		}

	}

}
