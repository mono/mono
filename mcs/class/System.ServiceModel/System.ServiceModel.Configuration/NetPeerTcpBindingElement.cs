//
// NetPeerTcpBindingElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006-2009 Novell, Inc.  http://www.novell.com
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
using System.Linq;
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
	public partial class NetPeerTcpBindingElement
		 : StandardBindingElement,  IBindingConfigurationElement
	{
		ConfigurationPropertyCollection _properties;

		public NetPeerTcpBindingElement ()
		{
		}

		public NetPeerTcpBindingElement (string name) : base (name) { }

		// Properties

		protected override Type BindingElementType {
			get { return typeof (NetPeerTcpBinding); }
		}

		[MonoTODO ("get converter for IPAddress")]
		//[TypeConverter (typeof(IPAddressConverter))]
		[ConfigurationProperty ("listenIPAddress",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = null)]
		public IPAddress ListenIPAddress {
			get { return (IPAddress) this ["listenIPAddress"]; }
			set { this ["listenIPAddress"] = value; }
		}

		[ConfigurationProperty ("maxBufferPoolSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "524288")]
		[LongValidator ( MinValue = 0,
			 MaxValue = 9223372036854775807,
			ExcludeRange = false)]
		public long MaxBufferPoolSize {
			get { return (long) this ["maxBufferPoolSize"]; }
			set { this ["maxBufferPoolSize"] = value; }
		}

		[ConfigurationProperty ("maxReceivedMessageSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "65536")]
		[LongValidator ( MinValue = 16384,
			 MaxValue = 9223372036854775807,
			ExcludeRange = false)]
		public long MaxReceivedMessageSize {
			get { return (long) this ["maxReceivedMessageSize"]; }
			set { this ["maxReceivedMessageSize"] = value; }
		}

		[IntegerValidator ( MinValue = 0,
			 MaxValue = 65535,
			ExcludeRange = false)]
		[ConfigurationProperty ("port",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "0")]
		public int Port {
			get { return (int) this ["port"]; }
			set { this ["port"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = base.Properties;
					_properties.Add (new ConfigurationProperty ("listenIPAddress", typeof (IPAddress), null, null/* FIXME: get converter for IPAddress*/, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxBufferPoolSize", typeof (long), "524288", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxReceivedMessageSize", typeof (long), "65536", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("port", typeof (int), "0", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("readerQuotas", typeof (XmlDictionaryReaderQuotasElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("resolver", typeof (PeerResolverElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("security", typeof (PeerSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
				}
				return _properties;
			}
		}

		[ConfigurationProperty ("readerQuotas",
			 Options = ConfigurationPropertyOptions.None)]
		public XmlDictionaryReaderQuotasElement ReaderQuotas {
			get { return (XmlDictionaryReaderQuotasElement) this ["readerQuotas"]; }
		}

		[ConfigurationProperty ("resolver",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = null)]
		public PeerResolverElement Resolver {
			get { return (PeerResolverElement) this ["resolver"]; }
		}

		[ConfigurationProperty ("security",
			 Options = ConfigurationPropertyOptions.None)]
		public PeerSecurityElement Security {
			get { return (PeerSecurityElement) this ["security"]; }
		}

		protected override void OnApplyConfiguration (Binding binding)
		{
			var n = (NetPeerTcpBinding) binding;
			n.ListenIPAddress = ListenIPAddress;
			n.MaxBufferPoolSize = MaxBufferPoolSize;
			n.MaxReceivedMessageSize = MaxReceivedMessageSize;
			n.Port = Port;
			n.ReaderQuotas = ReaderQuotas.Create ();
			if (Resolver != null) {
				if (Resolver.Custom != null) {
					n.Resolver.Custom.Address = new EndpointAddress (Resolver.Custom.Address, Resolver.Custom.Identity.Create (), Resolver.Custom.Headers.Headers);
					if (Resolver.Custom.Binding != null) {
						var bcol = ConfigUtil.BindingsSection [Resolver.Custom.Binding];
						var bc = bcol.ConfiguredBindings.First (b => b.Name == Resolver.Custom.BindingConfiguration);
						n.Resolver.Custom.Binding = (Binding) Activator.CreateInstance (bcol.BindingType, new object[0]);
						bc.ApplyConfiguration (n.Resolver.Custom.Binding);
					}
					// FIXME: correct type instantiation.
					if (!String.IsNullOrEmpty (Resolver.Custom.ResolverType))
						n.Resolver.Custom.Resolver = (PeerResolver) Activator.CreateInstance (Type.GetType (Resolver.Custom.ResolverType), new object [0]);
				}
				n.Resolver.Mode = Resolver.Mode;
				n.Resolver.ReferralPolicy = Resolver.ReferralPolicy;
			}
			if (Security != null) {
				n.Security.Mode = Security.Mode;
				n.Security.Transport.CredentialType = Security.Transport.CredentialType;
			}
		}
	}

}
