//
// X509ServiceCertificateAuthenticationElement.cs
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
	public sealed partial class X509ServiceCertificateAuthenticationElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty certificate_validation_mode;
		static ConfigurationProperty custom_certificate_validator_type;
		static ConfigurationProperty revocation_mode;
		static ConfigurationProperty trusted_store_location;

		static X509ServiceCertificateAuthenticationElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			certificate_validation_mode = new ConfigurationProperty ("certificateValidationMode",
				typeof (X509CertificateValidationMode), "ChainTrust", null/* FIXME: get converter for X509CertificateValidationMode*/, null,
				ConfigurationPropertyOptions.None);

			custom_certificate_validator_type = new ConfigurationProperty ("customCertificateValidatorType",
				typeof (string), "", new StringConverter (), null,
				ConfigurationPropertyOptions.None);

			revocation_mode = new ConfigurationProperty ("revocationMode",
				typeof (X509RevocationMode), "Online", null/* FIXME: get converter for X509RevocationMode*/, null,
				ConfigurationPropertyOptions.None);

			trusted_store_location = new ConfigurationProperty ("trustedStoreLocation",
				typeof (StoreLocation), "CurrentUser", null/* FIXME: get converter for StoreLocation*/, null,
				ConfigurationPropertyOptions.None);

			properties.Add (certificate_validation_mode);
			properties.Add (custom_certificate_validator_type);
			properties.Add (revocation_mode);
			properties.Add (trusted_store_location);
		}

		public X509ServiceCertificateAuthenticationElement ()
		{
		}


		// Properties

		[ConfigurationProperty ("certificateValidationMode",
			 DefaultValue = "ChainTrust",
			 Options = ConfigurationPropertyOptions.None)]
		public X509CertificateValidationMode CertificateValidationMode {
			get { return (X509CertificateValidationMode) base [certificate_validation_mode]; }
			set { base [certificate_validation_mode] = value; }
		}

		[ConfigurationProperty ("customCertificateValidatorType",
			 DefaultValue = "",
			 Options = ConfigurationPropertyOptions.None)]
		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		public string CustomCertificateValidatorType {
			get { return (string) base [custom_certificate_validator_type]; }
			set { base [custom_certificate_validator_type] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[ConfigurationProperty ("revocationMode",
			 DefaultValue = "Online",
			 Options = ConfigurationPropertyOptions.None)]
		public X509RevocationMode RevocationMode {
			get { return (X509RevocationMode) base [revocation_mode]; }
			set { base [revocation_mode] = value; }
		}

		[ConfigurationProperty ("trustedStoreLocation",
			 DefaultValue = "CurrentUser",
			 Options = ConfigurationPropertyOptions.None)]
		public StoreLocation TrustedStoreLocation {
			get { return (StoreLocation) base [trusted_store_location]; }
			set { base [trusted_store_location] = value; }
		}


	}

}
