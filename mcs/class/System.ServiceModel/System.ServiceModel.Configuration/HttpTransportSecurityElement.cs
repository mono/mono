//
// HttpTransportSecurityElement.cs
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
#if NET_4_0
using System.Security.Authentication.ExtendedProtection;
using System.Security.Authentication.ExtendedProtection.Configuration;
#endif
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
	public sealed partial class HttpTransportSecurityElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty client_credential_type;
		static ConfigurationProperty proxy_credential_type;
		static ConfigurationProperty realm;
		static ConfigurationProperty extended_protection_policy;

		static HttpTransportSecurityElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			client_credential_type = new ConfigurationProperty ("clientCredentialType",
				typeof (HttpClientCredentialType), "None", null/* FIXME: get converter for HttpClientCredentialType*/, null,
				ConfigurationPropertyOptions.None);

			proxy_credential_type = new ConfigurationProperty ("proxyCredentialType",
				typeof (HttpProxyCredentialType), "None", null/* FIXME: get converter for HttpProxyCredentialType*/, null,
				ConfigurationPropertyOptions.None);

			realm = new ConfigurationProperty ("realm",
				typeof (string), "", new StringConverter (), null,
				ConfigurationPropertyOptions.None);

#if NET_4_0
			extended_protection_policy = new ConfigurationProperty ("extendedProtectionPolicy",
				typeof (ExtendedProtectionPolicyElement), null, new ExtendedProtectionPolicyTypeConverter (), null,
				ConfigurationPropertyOptions.None);
#endif

			properties.Add (client_credential_type);
			properties.Add (proxy_credential_type);
			properties.Add (realm);
#if NET_4_0
			properties.Add (extended_protection_policy);
#endif
		}

		public HttpTransportSecurityElement ()
		{
		}


		// Properties

		[ConfigurationProperty ("clientCredentialType",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = HttpClientCredentialType.None)]
		public HttpClientCredentialType ClientCredentialType {
			get { return (HttpClientCredentialType) base [client_credential_type]; }
			set { base [client_credential_type] = value; }
		}

#if NET_4_0
		[ConfigurationProperty ("extendedProtectionPolicy",
			 Options = ConfigurationPropertyOptions.None)]
		public ExtendedProtectionPolicyElement extendedProtectionPolicy {
			get { return (ExtendedProtectionPolicyElement) base [extended_protection_policy]; }
			set { base [extended_protection_policy] = value; }
		}
#endif

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[ConfigurationProperty ("proxyCredentialType",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = HttpProxyCredentialType.None)]
		public HttpProxyCredentialType ProxyCredentialType {
			get { return (HttpProxyCredentialType) base [proxy_credential_type]; }
			set { base [proxy_credential_type] = value; }
		}

		[ConfigurationProperty ("realm",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "")]
		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		public string Realm {
			get { return (string) base [realm]; }
			set { base [realm] = value; }
		}

		internal void ApplyConfiguration (HttpTransportSecurity security)
		{
			security.ClientCredentialType = ClientCredentialType;
			security.ProxyCredentialType = ProxyCredentialType;
			security.Realm = Realm;
#if NET_4_0
			// FIXME: enable this
			// security.ExtendedProtectionPolicy = ExtendedProtectionPolicy.BuildPolicy ();
#endif
		}
	}

}
