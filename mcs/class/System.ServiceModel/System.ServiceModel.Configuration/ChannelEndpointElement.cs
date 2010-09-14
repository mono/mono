//
// ChannelEndpointElement.cs
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
	[MonoTODO]
	public sealed partial class ChannelEndpointElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty address;
		static ConfigurationProperty behavior_configuration;
		static ConfigurationProperty binding;
		static ConfigurationProperty binding_configuration;
		static ConfigurationProperty contract;
		static ConfigurationProperty headers;
		static ConfigurationProperty identity;
		static ConfigurationProperty name;
#if NET_4_0
		static ConfigurationProperty endpoint_configuration;
		static ConfigurationProperty kind;
#endif

		static ChannelEndpointElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			address = new ConfigurationProperty ("address",
				typeof (Uri), null, new UriTypeConverter (), null,
				ConfigurationPropertyOptions.None);

			behavior_configuration = new ConfigurationProperty ("behaviorConfiguration",
				typeof (string), "", new StringConverter (), null,
				ConfigurationPropertyOptions.None);

			binding = new ConfigurationProperty ("binding",
				typeof (string), null, new StringConverter (), null,
				ConfigurationPropertyOptions.IsRequired);

			binding_configuration = new ConfigurationProperty ("bindingConfiguration",
				typeof (string), "", new StringConverter (), null,
				ConfigurationPropertyOptions.None);

			contract = new ConfigurationProperty ("contract",
				typeof (string), null, new StringConverter (), null,
				ConfigurationPropertyOptions.IsRequired| ConfigurationPropertyOptions.IsKey);

			headers = new ConfigurationProperty ("headers",
				typeof (AddressHeaderCollectionElement), null, null/* FIXME: get converter for AddressHeaderCollectionElement*/, null,
				ConfigurationPropertyOptions.None);

			identity = new ConfigurationProperty ("identity",
				typeof (IdentityElement), null, null/* FIXME: get converter for IdentityElement*/, null,
				ConfigurationPropertyOptions.None);

			name = new ConfigurationProperty ("name",
				typeof (string), "", new StringConverter (), null,
				ConfigurationPropertyOptions.IsKey);

#if NET_4_0
			endpoint_configuration = new ConfigurationProperty ("endpointConfiguration", typeof (string), "", null, new StringValidator (0), ConfigurationPropertyOptions.IsKey);
			kind = new ConfigurationProperty ("kind", typeof (string), "", null, new StringValidator (0), ConfigurationPropertyOptions.IsKey);
#endif

			properties.Add (address);
			properties.Add (behavior_configuration);
			properties.Add (binding);
			properties.Add (binding_configuration);
			properties.Add (contract);
			properties.Add (headers);
			properties.Add (identity);
			properties.Add (name);

#if NET_4_0
			properties.Add (endpoint_configuration);
			properties.Add (kind);
#endif
		}

		public ChannelEndpointElement ()
		{
		}


		// Properties

		[ConfigurationProperty ("address",
			 Options = ConfigurationPropertyOptions.None)]
		public Uri Address {
			get { return (Uri) base [address]; }
			set { base [address] = value; }
		}

		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("behaviorConfiguration",
			 DefaultValue = "",
			 Options = ConfigurationPropertyOptions.None)]
		public string BehaviorConfiguration {
			get { return (string) base [behavior_configuration]; }
			set { base [behavior_configuration] = value; }
		}

		[StringValidator ( MinLength = 1,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("binding",
			 Options = ConfigurationPropertyOptions.IsRequired,
			IsRequired = true)]
		public string Binding {
			get { return (string) base [binding]; }
			set { base [binding] = value; }
		}

		[ConfigurationProperty ("bindingConfiguration",
			 DefaultValue = "",
			 Options = ConfigurationPropertyOptions.None)]
		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		public string BindingConfiguration {
			get { return (string) base [binding_configuration]; }
			set { base [binding_configuration] = value; }
		}

		[ConfigurationProperty ("contract",
			 Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey,
			IsRequired = true,
			IsKey = true)]
		[StringValidator ( MinLength = 1,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		public string Contract {
			get { return (string) base [contract]; }
			set { base [contract] = value; }
		}

		[ConfigurationProperty ("headers",
			 Options = ConfigurationPropertyOptions.None)]
		public AddressHeaderCollectionElement Headers {
			get { return (AddressHeaderCollectionElement) base [headers]; }
		}

		[ConfigurationProperty ("identity",
			 Options = ConfigurationPropertyOptions.None)]
		public IdentityElement Identity {
			get { return (IdentityElement) base [identity]; }
		}

		[ConfigurationProperty ("name",
			 DefaultValue = "",
			 Options = ConfigurationPropertyOptions.IsKey,
			IsKey = true)]
		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		public string Name {
			get { return (string) base [name]; }
			set { base [name] = value; }
		}

#if NET_4_0
		[StringValidator (MinLength = 0)]
		[ConfigurationProperty ("endpointConfiguration", DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
		public string EndpointConfiguration {
			get { return (string) base [endpoint_configuration]; }
			set { base [endpoint_configuration] = value; }
		}

		[ConfigurationProperty ("kind", DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
		[StringValidator (MinLength = 0)]
		public string Kind {
			get { return (string) base [kind]; }
			set { base [kind] = value; }
		}
#endif

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}


	}

}
