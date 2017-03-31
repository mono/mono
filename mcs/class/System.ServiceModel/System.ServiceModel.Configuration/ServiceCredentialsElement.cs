//
// ServiceCredentialsElement.cs
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
using System.IdentityModel.Selectors;
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
using System.Web.Security;
using System.Xml;

namespace System.ServiceModel.Configuration
{
	public class ServiceCredentialsElement : BehaviorExtensionElement
	{
		public ServiceCredentialsElement () {
		}


		// Properties

		public override Type BehaviorType {
			get { return typeof (ServiceCredentials); }
		}

		[ConfigurationProperty ("clientCertificate",
			 Options = ConfigurationPropertyOptions.None)]
		public X509InitiatorCertificateServiceElement ClientCertificate {
			get { return (X509InitiatorCertificateServiceElement) base ["clientCertificate"]; }
		}

		[ConfigurationProperty ("issuedTokenAuthentication",
			 Options = ConfigurationPropertyOptions.None)]
		public IssuedTokenServiceElement IssuedTokenAuthentication {
			get { return (IssuedTokenServiceElement) base ["issuedTokenAuthentication"]; }
		}

		[ConfigurationProperty ("peer",
			 Options = ConfigurationPropertyOptions.None)]
		public PeerCredentialElement Peer {
			get { return (PeerCredentialElement) base ["peer"]; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return base.Properties; }
		}

		[ConfigurationProperty ("secureConversationAuthentication",
			 Options = ConfigurationPropertyOptions.None)]
		public SecureConversationServiceElement SecureConversationAuthentication {
			get { return (SecureConversationServiceElement) base ["secureConversationAuthentication"]; }
		}

		[ConfigurationProperty ("serviceCertificate",
			 Options = ConfigurationPropertyOptions.None)]
		public X509RecipientCertificateServiceElement ServiceCertificate {
			get { return (X509RecipientCertificateServiceElement) base ["serviceCertificate"]; }
		}

		[StringValidator (MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("type",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "")]
		public string Type {
			get { return (string) base ["type"]; }
			set { base ["type"] = value; }
		}

		[ConfigurationProperty ("userNameAuthentication",
			 Options = ConfigurationPropertyOptions.None)]
		public UserNameServiceElement UserNameAuthentication {
			get { return (UserNameServiceElement) base ["userNameAuthentication"]; }
		}

		[ConfigurationProperty ("windowsAuthentication",
			 Options = ConfigurationPropertyOptions.None)]
		public WindowsServiceElement WindowsAuthentication {
			get { return (WindowsServiceElement) base ["windowsAuthentication"]; }
		}

		protected internal override object CreateBehavior ()
		{
			var sb = new ServiceCredentials ();
			ApplyConfiguration (sb);
			return sb;
		}

		protected internal void ApplyConfiguration (ServiceCredentials behavior)
		{
			// IssuedToken
			foreach (AllowedAudienceUriElement ae in IssuedTokenAuthentication.AllowedAudienceUris)
				behavior.IssuedTokenAuthentication.AllowedAudienceUris.Add (ae.AllowedAudienceUri);
			behavior.IssuedTokenAuthentication.AllowUntrustedRsaIssuers = IssuedTokenAuthentication.AllowUntrustedRsaIssuers;
			behavior.IssuedTokenAuthentication.AudienceUriMode = IssuedTokenAuthentication.AudienceUriMode;

			if (!String.IsNullOrEmpty (IssuedTokenAuthentication.CustomCertificateValidatorType))
			behavior.IssuedTokenAuthentication.CustomCertificateValidator = (X509CertificateValidator) CreateInstance (IssuedTokenAuthentication.CustomCertificateValidatorType);
			behavior.IssuedTokenAuthentication.CertificateValidationMode = IssuedTokenAuthentication.CertificateValidationMode;
			behavior.IssuedTokenAuthentication.RevocationMode = IssuedTokenAuthentication.RevocationMode;
			behavior.IssuedTokenAuthentication.TrustedStoreLocation = IssuedTokenAuthentication.TrustedStoreLocation;
			foreach (X509CertificateTrustedIssuerElement ce in IssuedTokenAuthentication.KnownCertificates)
				behavior.IssuedTokenAuthentication.KnownCertificates.Add (GetCertificate (ce.StoreLocation, ce.StoreName, ce.X509FindType, ce.FindValue));

			behavior.IssuedTokenAuthentication.SamlSerializer = (SamlSerializer) CreateInstance (IssuedTokenAuthentication.SamlSerializerType);


			// Peer
			if (!String.IsNullOrEmpty (Peer.Certificate.FindValue))
				behavior.Peer.SetCertificate (Peer.Certificate.StoreLocation, Peer.Certificate.StoreName, Peer.Certificate.X509FindType, Peer.Certificate.FindValue);
			// sb.Peer.MeshPassword = /* cannot fill it here */
			behavior.Peer.MessageSenderAuthentication.CustomCertificateValidator = (X509CertificateValidator) CreateInstance (Peer.MessageSenderAuthentication.CustomCertificateValidatorType);
			behavior.Peer.MessageSenderAuthentication.CertificateValidationMode = Peer.MessageSenderAuthentication.CertificateValidationMode;
			behavior.Peer.MessageSenderAuthentication.RevocationMode = Peer.MessageSenderAuthentication.RevocationMode;
			behavior.Peer.MessageSenderAuthentication.TrustedStoreLocation = Peer.MessageSenderAuthentication.TrustedStoreLocation;
			behavior.Peer.PeerAuthentication.CustomCertificateValidator = (X509CertificateValidator) CreateInstance (Peer.PeerAuthentication.CustomCertificateValidatorType);
			behavior.Peer.PeerAuthentication.CertificateValidationMode = Peer.PeerAuthentication.CertificateValidationMode;
			behavior.Peer.PeerAuthentication.RevocationMode = Peer.PeerAuthentication.RevocationMode;
			behavior.Peer.PeerAuthentication.TrustedStoreLocation = Peer.PeerAuthentication.TrustedStoreLocation;

			// WSSC
			behavior.SecureConversationAuthentication.SecurityStateEncoder = (SecurityStateEncoder) CreateInstance (SecureConversationAuthentication.SecurityStateEncoderType);

			// X509
			if (!String.IsNullOrEmpty (ServiceCertificate.FindValue))
				behavior.ServiceCertificate.SetCertificate (ServiceCertificate.StoreLocation, ServiceCertificate.StoreName, ServiceCertificate.X509FindType, ServiceCertificate.FindValue);

			// UserNamePassword
			behavior.UserNameAuthentication.CachedLogonTokenLifetime = UserNameAuthentication.CachedLogonTokenLifetime;
			behavior.UserNameAuthentication.CacheLogonTokens = UserNameAuthentication.CacheLogonTokens;
			behavior.UserNameAuthentication.CustomUserNamePasswordValidator = (UserNamePasswordValidator) CreateInstance (UserNameAuthentication.CustomUserNamePasswordValidatorType);
			behavior.UserNameAuthentication.IncludeWindowsGroups = UserNameAuthentication.IncludeWindowsGroups;
			behavior.UserNameAuthentication.MaxCachedLogonTokens = UserNameAuthentication.MaxCachedLogonTokens;
			behavior.UserNameAuthentication.MembershipProvider = (MembershipProvider) CreateInstance (UserNameAuthentication.MembershipProviderName);
			behavior.UserNameAuthentication.UserNamePasswordValidationMode = UserNameAuthentication.UserNamePasswordValidationMode;

			// Windows
			behavior.WindowsAuthentication.AllowAnonymousLogons = WindowsAuthentication.AllowAnonymousLogons;
			behavior.WindowsAuthentication.IncludeWindowsGroups = WindowsAuthentication.IncludeWindowsGroups;
		}

		X509Certificate2 GetCertificate (StoreLocation storeLocation, StoreName storeName, X509FindType findType, object findValue)
		{
			return ConfigUtil.CreateCertificateFrom (storeLocation, storeName, findType, findValue);
		}

		object CreateInstance (string typeName)
		{
			return String.IsNullOrEmpty (typeName) ? null : Activator.CreateInstance (System.Type.GetType (typeName, true));
		}
	}

}
