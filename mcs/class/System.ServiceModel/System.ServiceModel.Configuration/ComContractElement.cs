//
// ComContractElement.cs
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
	public sealed partial class ComContractElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty contract;
		static ConfigurationProperty exposed_methods;
		static ConfigurationProperty name;
		static ConfigurationProperty ns;
		static ConfigurationProperty persistable_types;
		static ConfigurationProperty requires_session;
		static ConfigurationProperty user_defined_types;

		static ComContractElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			contract = new ConfigurationProperty ("contract",
				typeof (string), null, new StringConverter (), null,
				ConfigurationPropertyOptions.IsRequired| ConfigurationPropertyOptions.IsKey);

			exposed_methods = new ConfigurationProperty ("exposedMethods",
				typeof (ComMethodElementCollection), null, null/* FIXME: get converter for ComMethodElementCollection*/, null,
				ConfigurationPropertyOptions.None);

			name = new ConfigurationProperty ("name",
				typeof (string), "", new StringConverter (), null,
				ConfigurationPropertyOptions.None);

			ns = new ConfigurationProperty ("namespace",
				typeof (string), "", new StringConverter (), null,
				ConfigurationPropertyOptions.None);

			persistable_types = new ConfigurationProperty ("persistableTypes",
				typeof (ComPersistableTypeElementCollection), null, null/* FIXME: get converter for ComPersistableTypeElementCollection*/, null,
				ConfigurationPropertyOptions.None);

			requires_session = new ConfigurationProperty ("requiresSession",
				typeof (bool), "true", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			user_defined_types = new ConfigurationProperty ("userDefinedTypes",
				typeof (ComUdtElementCollection), null, null/* FIXME: get converter for ComUdtElementCollection*/, null,
				ConfigurationPropertyOptions.None);

			properties.Add (contract);
			properties.Add (exposed_methods);
			properties.Add (name);
			properties.Add (ns);
			properties.Add (persistable_types);
			properties.Add (requires_session);
			properties.Add (user_defined_types);
		}

		public ComContractElement ()
		{
		}


		// Properties

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

		[ConfigurationProperty ("exposedMethods",
			 Options = ConfigurationPropertyOptions.None)]
		public ComMethodElementCollection ExposedMethods {
			get { return (ComMethodElementCollection) base [exposed_methods]; }
		}

		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("name",
			 DefaultValue = "",
			 Options = ConfigurationPropertyOptions.None)]
		public string Name {
			get { return (string) base [name]; }
			set { base [name] = value; }
		}

		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("namespace",
			 DefaultValue = "",
			 Options = ConfigurationPropertyOptions.None)]
		public string Namespace {
			get { return (string) base [ns]; }
			set { base [ns] = value; }
		}

		[ConfigurationProperty ("persistableTypes",
			 Options = ConfigurationPropertyOptions.None)]
		public ComPersistableTypeElementCollection PersistableTypes {
			get { return (ComPersistableTypeElementCollection) base [persistable_types]; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[ConfigurationProperty ("requiresSession",
			DefaultValue = true,
			 Options = ConfigurationPropertyOptions.None)]
		public bool RequiresSession {
			get { return (bool) base [requires_session]; }
			set { base [requires_session] = value; }
		}

		[ConfigurationProperty ("userDefinedTypes",
			 Options = ConfigurationPropertyOptions.None)]
		public ComUdtElementCollection UserDefinedTypes {
			get { return (ComUdtElementCollection) base [user_defined_types]; }
		}


	}

}
