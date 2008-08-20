//
// MsmqTransportSecurityElement.cs
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
	public sealed partial class MsmqTransportSecurityElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty msmq_authentication_mode;
		static ConfigurationProperty msmq_encryption_algorithm;
		static ConfigurationProperty msmq_protection_level;
		static ConfigurationProperty msmq_secure_hash_algorithm;

		static MsmqTransportSecurityElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			msmq_authentication_mode = new ConfigurationProperty ("msmqAuthenticationMode",
				typeof (MsmqAuthenticationMode), "WindowsDomain", null/* FIXME: get converter for MsmqAuthenticationMode*/, null,
				ConfigurationPropertyOptions.None);

			msmq_encryption_algorithm = new ConfigurationProperty ("msmqEncryptionAlgorithm",
				typeof (MsmqEncryptionAlgorithm), "RC4Stream", null/* FIXME: get converter for MsmqEncryptionAlgorithm*/, null,
				ConfigurationPropertyOptions.None);

			msmq_protection_level = new ConfigurationProperty ("msmqProtectionLevel",
				typeof (ProtectionLevel), "Sign", null/* FIXME: get converter for ProtectionLevel*/, null,
				ConfigurationPropertyOptions.None);

			msmq_secure_hash_algorithm = new ConfigurationProperty ("msmqSecureHashAlgorithm",
				typeof (MsmqSecureHashAlgorithm), "Sha1", null/* FIXME: get converter for MsmqSecureHashAlgorithm*/, null,
				ConfigurationPropertyOptions.None);

			properties.Add (msmq_authentication_mode);
			properties.Add (msmq_encryption_algorithm);
			properties.Add (msmq_protection_level);
			properties.Add (msmq_secure_hash_algorithm);
		}

		public MsmqTransportSecurityElement ()
		{
		}


		// Properties

		[ConfigurationProperty ("msmqAuthenticationMode",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "WindowsDomain")]
		public MsmqAuthenticationMode MsmqAuthenticationMode {
			get { return (MsmqAuthenticationMode) base [msmq_authentication_mode]; }
			set { base [msmq_authentication_mode] = value; }
		}

		[ConfigurationProperty ("msmqEncryptionAlgorithm",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "RC4Stream")]
		public MsmqEncryptionAlgorithm MsmqEncryptionAlgorithm {
			get { return (MsmqEncryptionAlgorithm) base [msmq_encryption_algorithm]; }
			set { base [msmq_encryption_algorithm] = value; }
		}

		[ConfigurationProperty ("msmqProtectionLevel",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Sign")]
		public ProtectionLevel MsmqProtectionLevel {
			get { return (ProtectionLevel) base [msmq_protection_level]; }
			set { base [msmq_protection_level] = value; }
		}

		[ConfigurationProperty ("msmqSecureHashAlgorithm",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Sha1")]
		public MsmqSecureHashAlgorithm MsmqSecureHashAlgorithm {
			get { return (MsmqSecureHashAlgorithm) base [msmq_secure_hash_algorithm]; }
			set { base [msmq_secure_hash_algorithm] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}


	}

}
