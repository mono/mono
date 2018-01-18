//
// ServiceCredentialsTest.cs
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class ServiceCredentialsTest
	{
		[Test]
		public void ClientCertificate ()
		{
			X509CertificateInitiatorServiceCredential cc =
				new ServiceCredentials ().ClientCertificate;
			Assert.IsNull (cc.Certificate, "#1");
			Assert.AreEqual (X509CertificateValidationMode.ChainTrust, cc.Authentication.CertificateValidationMode, "#2-1");
			Assert.IsNull (cc.Authentication.CustomCertificateValidator, "#2-2");
			Assert.IsTrue (cc.Authentication.IncludeWindowsGroups, "#2-3");
			Assert.IsFalse (cc.Authentication.MapClientCertificateToWindowsAccount, "#2-4");
			Assert.AreEqual (X509RevocationMode.Online, cc.Authentication.RevocationMode, "#2-5");
			Assert.AreEqual (StoreLocation.LocalMachine, cc.Authentication.TrustedStoreLocation, "#2-6");
		}

		[Test]
		public void IssuedTokenAuthentication ()
		{
			IssuedTokenServiceCredential ic =
				new ServiceCredentials ().IssuedTokenAuthentication;
			Assert.IsFalse (ic.AllowUntrustedRsaIssuers, "#1");
			Assert.AreEqual (X509CertificateValidationMode.ChainTrust, ic.CertificateValidationMode, "#2");
			Assert.IsNull (ic.CustomCertificateValidator, "#3");
			Assert.AreEqual (0, ic.KnownCertificates.Count, "#4");
			Assert.AreEqual (X509RevocationMode.Online, ic.RevocationMode, "#5");
			Assert.IsNull (ic.SamlSerializer, "#6");
			Assert.AreEqual (StoreLocation.LocalMachine, ic.TrustedStoreLocation, "#7");
		}

		[Test]
		public void Peer ()
		{
			PeerCredential p = new ServiceCredentials ().Peer;
			Assert.IsNull (p.Certificate, "#1");
			Assert.IsNull (p.MeshPassword, "#2");
			X509PeerCertificateAuthentication pa = p.MessageSenderAuthentication;
			Assert.AreEqual (X509CertificateValidationMode.PeerOrChainTrust, pa.CertificateValidationMode, "#3-1");
			Assert.IsNull (pa.CustomCertificateValidator, "#3-2");
			Assert.AreEqual (X509RevocationMode.Online, pa.RevocationMode, "#3-3");
			Assert.AreEqual (StoreLocation.CurrentUser, pa.TrustedStoreLocation, "#3-4");
			pa = p.PeerAuthentication;
			Assert.AreEqual (X509CertificateValidationMode.PeerOrChainTrust, pa.CertificateValidationMode, "#4-1");
			Assert.IsNull (pa.CustomCertificateValidator, "#4-2");
			Assert.AreEqual (X509RevocationMode.Online, pa.RevocationMode, "#4-3");
			Assert.AreEqual (StoreLocation.CurrentUser, pa.TrustedStoreLocation, "#4-4");
		}

		[Test]
		public void SecureConversationAuthentication ()
		{
			SecureConversationServiceCredential sc =
				new ServiceCredentials ().SecureConversationAuthentication;
			Assert.AreEqual (5, sc.SecurityContextClaimTypes.Count, "#1");
			Collection<Type> types = new Collection<Type> (new Type [] {
				typeof (SamlAuthorizationDecisionClaimResource),
				typeof (SamlAuthenticationClaimResource),
				typeof (SamlAccessDecision),
				typeof (SamlAuthorityBinding),
				typeof (SamlNameIdentifierClaimResource)});
			foreach (Type type in sc.SecurityContextClaimTypes)
				if (!types.Contains (type))
					Assert.Fail (type.ToString ());
			DataProtectionSecurityStateEncoder sse = sc.SecurityStateEncoder
				as DataProtectionSecurityStateEncoder;
			Assert.IsNotNull (sse, "#2-1");
			Assert.IsTrue (sse.UseCurrentUserProtectionScope, "#2-2");
		}

		[Test]
		public void ServiceCertificate ()
		{
			Assert.IsNull (new ServiceCredentials ().ServiceCertificate.Certificate, "#1");
		}

		[Test]
		public void UserNameAuthentication ()
		{
			UserNamePasswordServiceCredential un =
				new ServiceCredentials ().UserNameAuthentication;
			Assert.AreEqual (TimeSpan.FromMinutes (15), un.CachedLogonTokenLifetime, "#1");
			Assert.IsFalse (un.CacheLogonTokens, "#2");
			Assert.IsNull (un.CustomUserNamePasswordValidator, "#3");
			Assert.IsTrue (un.IncludeWindowsGroups, "#4");
			Assert.AreEqual (0x80, un.MaxCachedLogonTokens, "#5");
			Assert.IsNull (un.MembershipProvider, "#6");
			Assert.AreEqual (UserNamePasswordValidationMode.Windows, un.UserNamePasswordValidationMode, "#7");

			// TODO: WindowsAuthentication if we have infinite amount of free time.
		}
	}
}
#endif

