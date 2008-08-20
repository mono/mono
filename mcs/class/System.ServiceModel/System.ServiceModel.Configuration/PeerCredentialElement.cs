//
// PeerCredentialElement.cs
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
	public sealed partial class PeerCredentialElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty certificate;
		static ConfigurationProperty message_sender_authentication;
		static ConfigurationProperty peer_authentication;

		static PeerCredentialElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			certificate = new ConfigurationProperty ("certificate",
				typeof (X509PeerCertificateElement), null, null/* FIXME: get converter for X509PeerCertificateElement*/, null,
				ConfigurationPropertyOptions.None);

			message_sender_authentication = new ConfigurationProperty ("messageSenderAuthentication",
				typeof (X509PeerCertificateAuthenticationElement), null, null/* FIXME: get converter for X509PeerCertificateAuthenticationElement*/, null,
				ConfigurationPropertyOptions.None);

			peer_authentication = new ConfigurationProperty ("peerAuthentication",
				typeof (X509PeerCertificateAuthenticationElement), null, null/* FIXME: get converter for X509PeerCertificateAuthenticationElement*/, null,
				ConfigurationPropertyOptions.None);

			properties.Add (certificate);
			properties.Add (message_sender_authentication);
			properties.Add (peer_authentication);
		}

		public PeerCredentialElement ()
		{
		}


		// Properties

		[ConfigurationProperty ("certificate",
			 Options = ConfigurationPropertyOptions.None)]
		public X509PeerCertificateElement Certificate {
			get { return (X509PeerCertificateElement) base [certificate]; }
		}

		[ConfigurationProperty ("messageSenderAuthentication",
			 Options = ConfigurationPropertyOptions.None)]
		public X509PeerCertificateAuthenticationElement MessageSenderAuthentication {
			get { return (X509PeerCertificateAuthenticationElement) base [message_sender_authentication]; }
		}

		[ConfigurationProperty ("peerAuthentication",
			 Options = ConfigurationPropertyOptions.None)]
		public X509PeerCertificateAuthenticationElement PeerAuthentication {
			get { return (X509PeerCertificateAuthenticationElement) base [peer_authentication]; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}


	}

}
