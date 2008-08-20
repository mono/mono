//
// PeerCustomResolverElement.cs
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
	public sealed partial class PeerCustomResolverElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty address;
		static ConfigurationProperty binding;
		static ConfigurationProperty binding_configuration;
		static ConfigurationProperty headers;
		static ConfigurationProperty identity;
		static ConfigurationProperty resolver_type;

		static PeerCustomResolverElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			address = new ConfigurationProperty ("address",
				typeof (Uri), null, new UriTypeConverter (), null,
				ConfigurationPropertyOptions.None);

			binding = new ConfigurationProperty ("binding",
				typeof (string), "", new StringConverter (), null,
				ConfigurationPropertyOptions.None);

			binding_configuration = new ConfigurationProperty ("bindingConfiguration",
				typeof (string), "", new StringConverter (), null,
				ConfigurationPropertyOptions.None);

			headers = new ConfigurationProperty ("headers",
				typeof (AddressHeaderCollectionElement), null, null/* FIXME: get converter for AddressHeaderCollectionElement*/, null,
				ConfigurationPropertyOptions.None);

			identity = new ConfigurationProperty ("identity",
				typeof (IdentityElement), null, null/* FIXME: get converter for IdentityElement*/, null,
				ConfigurationPropertyOptions.None);

			resolver_type = new ConfigurationProperty ("resolverType",
				typeof (string), "", new StringConverter (), null,
				ConfigurationPropertyOptions.None);

			properties.Add (address);
			properties.Add (binding);
			properties.Add (binding_configuration);
			properties.Add (headers);
			properties.Add (identity);
			properties.Add (resolver_type);
		}

		public PeerCustomResolverElement ()
		{
		}


		// Properties

		[ConfigurationProperty ("address",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = null)]
		public Uri Address {
			get { return (Uri) base [address]; }
			set { base [address] = value; }
		}

		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("binding",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "")]
		public string Binding {
			get { return (string) base [binding]; }
			set { base [binding] = value; }
		}

		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("bindingConfiguration",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "")]
		public string BindingConfiguration {
			get { return (string) base [binding_configuration]; }
			set { base [binding_configuration] = value; }
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

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[ConfigurationProperty ("resolverType",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "")]
		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		public string ResolverType {
			get { return (string) base [resolver_type]; }
			set { base [resolver_type] = value; }
		}


	}

}
