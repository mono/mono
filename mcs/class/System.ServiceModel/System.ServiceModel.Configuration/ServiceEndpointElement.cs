//
// ServiceEndpointElement.cs
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
	public sealed class ServiceEndpointElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty address;
		static ConfigurationProperty behavior_configuration;
		static ConfigurationProperty binding;
		static ConfigurationProperty binding_configuration;
		static ConfigurationProperty binding_name;
		static ConfigurationProperty binding_namespace;
		static ConfigurationProperty contract;
		static ConfigurationProperty headers;
		static ConfigurationProperty identity;
		static ConfigurationProperty listen_uri;
		static ConfigurationProperty listen_uri_mode;
		static ConfigurationProperty name;
#if NET_4_0
		static ConfigurationProperty endpoint_configuration;
		static ConfigurationProperty is_system_endpoint;
		static ConfigurationProperty kind;
#endif

		static ServiceEndpointElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			address = new ConfigurationProperty ("address",
				typeof (Uri), "", new UriTypeConverter (), null,
				ConfigurationPropertyOptions.IsKey);

			behavior_configuration = new ConfigurationProperty ("behaviorConfiguration",
				typeof (string), "", new StringConverter (), new StringValidator (0, int.MaxValue, null),
				ConfigurationPropertyOptions.None);

			binding = new ConfigurationProperty ("binding",
				typeof (string), null, new StringConverter (), new StringValidator (1, int.MaxValue, null),
				ConfigurationPropertyOptions.IsRequired| ConfigurationPropertyOptions.IsKey);

			binding_configuration = new ConfigurationProperty ("bindingConfiguration",
				typeof (string), "", new StringConverter (), new StringValidator (0, int.MaxValue, null),
				ConfigurationPropertyOptions.IsKey);

			binding_name = new ConfigurationProperty ("bindingName",
				typeof (string), "", new StringConverter (), new StringValidator (0, int.MaxValue, null),
				ConfigurationPropertyOptions.IsKey);

			binding_namespace = new ConfigurationProperty ("bindingNamespace",
				typeof (string), "", new StringConverter (), new StringValidator (0, int.MaxValue, null),
				ConfigurationPropertyOptions.IsKey);

			contract = new ConfigurationProperty ("contract",
				typeof (string), "", new StringConverter (), new StringValidator (0, int.MaxValue, null),
				ConfigurationPropertyOptions.IsKey);

			headers = new ConfigurationProperty ("headers",
				typeof (AddressHeaderCollectionElement), null, null, null,
				ConfigurationPropertyOptions.None);

			identity = new ConfigurationProperty ("identity",
				typeof (IdentityElement), null, null, null,
				ConfigurationPropertyOptions.None);

			listen_uri = new ConfigurationProperty ("listenUri",
				typeof (Uri), null, new UriTypeConverter (), null,
				ConfigurationPropertyOptions.None);

			listen_uri_mode = new ConfigurationProperty ("listenUriMode",
				typeof (ListenUriMode), "Explicit", null, null,
				ConfigurationPropertyOptions.None);

			name = new ConfigurationProperty ("name",
				typeof (string), "", new StringConverter (), new StringValidator (0, int.MaxValue, null),
				ConfigurationPropertyOptions.None);

#if NET_4_0
			endpoint_configuration = new ConfigurationProperty ("endpointConfiguration", typeof (string), "", null, new StringValidator (0), ConfigurationPropertyOptions.IsKey);
			is_system_endpoint = new ConfigurationProperty ("isSystemEndpoint", typeof (bool), false, null, null, ConfigurationPropertyOptions.None);
			kind = new ConfigurationProperty ("kind", typeof (string), "", null, new StringValidator (0), ConfigurationPropertyOptions.IsKey);
#endif

			properties.Add (address);
			properties.Add (behavior_configuration);
			properties.Add (binding);
			properties.Add (binding_configuration);
			properties.Add (binding_name);
			properties.Add (binding_namespace);
			properties.Add (contract);
			properties.Add (headers);
			properties.Add (identity);
			properties.Add (listen_uri);
			properties.Add (listen_uri_mode);
			properties.Add (name);

#if NET_4_0
			properties.Add (endpoint_configuration);
			properties.Add (is_system_endpoint);
			properties.Add (kind);
#endif
		}

		public ServiceEndpointElement ()
		{
		}


		// Properties

		[ConfigurationProperty ("address",
			 Options = ConfigurationPropertyOptions.IsKey,
			 DefaultValue = "",
			IsKey = true)]
		public Uri Address {
			get { return (Uri) base [address]; }
			set { base [address] = value; }
		}

		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("behaviorConfiguration",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "")]
		public string BehaviorConfiguration {
			get { return (string) base [behavior_configuration]; }
			set { base [behavior_configuration] = value; }
		}

		[StringValidator ( MinLength = 1,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("binding",
			 Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey,
			IsRequired = true,
			IsKey = true)]
		public string Binding {
			get { return (string) base [binding]; }
			set { base [binding] = value; }
		}

		[ConfigurationProperty ("bindingConfiguration",
			 Options = ConfigurationPropertyOptions.IsKey,
			 DefaultValue = "",
			IsKey = true)]
		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		public string BindingConfiguration {
			get { return (string) base [binding_configuration]; }
			set { base [binding_configuration] = value; }
		}

		[ConfigurationProperty ("bindingName",
			 Options = ConfigurationPropertyOptions.IsKey,
			 DefaultValue = "",
			IsKey = true)]
		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		public string BindingName {
			get { return (string) base [binding_name]; }
			set { base [binding_name] = value; }
		}

		[ConfigurationProperty ("bindingNamespace",
			 Options = ConfigurationPropertyOptions.IsKey,
			 DefaultValue = "",
			IsKey = true)]
		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		public string BindingNamespace {
			get { return (string) base [binding_namespace]; }
			set { base [binding_namespace] = value; }
		}

		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("contract",
			 Options = ConfigurationPropertyOptions.IsKey,
			 DefaultValue = "",
			IsKey = true)]
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

#if NET_4_0
		[StringValidator (MinLength = 0)]
		[ConfigurationProperty ("endpointConfiguration", DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
		public string EndpointConfiguration {
			get { return (string) base [endpoint_configuration]; }
			set { base [endpoint_configuration] = value; }
		}

		[ConfigurationProperty ("isSystemEndpoint", DefaultValue = false)]
		public bool IsSystemEndpoint {
			get { return (bool) base [is_system_endpoint]; }
			set { base [is_system_endpoint] = value; }
		}

		[ConfigurationProperty ("kind", DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
		[StringValidator (MinLength = 0)]
		public string Kind {
			get { return (string) base [kind]; }
			set { base [kind] = value; }
		}
#endif

		[ConfigurationProperty ("listenUri",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = null)]
		public Uri ListenUri {
			get { return (Uri) base [listen_uri]; }
			set { base [listen_uri] = value; }
		}

		[ConfigurationProperty ("listenUriMode",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Explicit")]
		public ListenUriMode ListenUriMode {
			get { return (ListenUriMode) base [listen_uri_mode]; }
			set { base [listen_uri_mode] = value; }
		}

		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("name",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "")]
		public string Name {
			get { return (string) base [name]; }
			set { base [name] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}


	}

}
