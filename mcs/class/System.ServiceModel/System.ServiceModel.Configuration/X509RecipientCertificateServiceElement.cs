//
// X509RecipientCertificateServiceElement.cs
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
	public sealed partial class X509RecipientCertificateServiceElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty find_value;
		static ConfigurationProperty store_location;
		static ConfigurationProperty store_name;
		static ConfigurationProperty x509_find_type;

		static X509RecipientCertificateServiceElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			find_value = new ConfigurationProperty ("findValue",
				typeof (string), "", new StringConverter (), null,
				ConfigurationPropertyOptions.None);

			store_location = new ConfigurationProperty ("storeLocation",
				typeof (StoreLocation), "LocalMachine", null/* FIXME: get converter for StoreLocation*/, null,
				ConfigurationPropertyOptions.None);

			store_name = new ConfigurationProperty ("storeName",
				typeof (StoreName), "My", null/* FIXME: get converter for StoreName*/, null,
				ConfigurationPropertyOptions.None);

			x509_find_type = new ConfigurationProperty ("x509FindType",
				typeof (X509FindType), "FindBySubjectDistinguishedName", null/* FIXME: get converter for X509FindType*/, null,
				ConfigurationPropertyOptions.None);

			properties.Add (find_value);
			properties.Add (store_location);
			properties.Add (store_name);
			properties.Add (x509_find_type);
		}

		public X509RecipientCertificateServiceElement ()
		{
		}


		// Properties

		[ConfigurationProperty ("findValue",
			 DefaultValue = "",
			 Options = ConfigurationPropertyOptions.None)]
		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		public string FindValue {
			get { return (string) base [find_value]; }
			set { base [find_value] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[ConfigurationProperty ("storeLocation",
			 DefaultValue = "LocalMachine",
			 Options = ConfigurationPropertyOptions.None)]
		public StoreLocation StoreLocation {
			get { return (StoreLocation) base [store_location]; }
			set { base [store_location] = value; }
		}

		[ConfigurationProperty ("storeName",
			 DefaultValue = "My",
			 Options = ConfigurationPropertyOptions.None)]
		public StoreName StoreName {
			get { return (StoreName) base [store_name]; }
			set { base [store_name] = value; }
		}

		[ConfigurationProperty ("x509FindType",
			 DefaultValue = "FindBySubjectDistinguishedName",
			 Options = ConfigurationPropertyOptions.None)]
		public X509FindType X509FindType {
			get { return (X509FindType) base [x509_find_type]; }
			set { base [x509_find_type] = value; }
		}


	}

}
