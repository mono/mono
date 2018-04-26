//
// ClientCredentialsTest.cs
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
#if !MOBILE
using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class ClientCredentialsTest
	{
		[Test]
		public void ClientCertificate ()
		{
			ClientCredentials c = new ClientCredentials ();
			Assert.AreEqual (true, c.SupportInteractive, "#1");
			X509CertificateInitiatorClientCredential ccert =
				c.ClientCertificate;
			Assert.IsNull (ccert.Certificate, "#2");
		}

		[Test]
		public void HttpDigest ()
		{
			ClientCredentials c = new ClientCredentials ();
			// FIXME: implement
			HttpDigestClientCredential http = c.HttpDigest;
		}

		[Test]
		public void IssuedToken ()
		{
			ClientCredentials c = new ClientCredentials ();
			IssuedTokenClientCredential iss = c.IssuedToken;
			Assert.IsNotNull (iss, "#1");
			Assert.AreEqual (true, iss.CacheIssuedTokens, "#2");
			Assert.AreEqual (SecurityKeyEntropyMode.CombinedEntropy, iss.DefaultKeyEntropyMode, "#3");
			Assert.AreEqual (60, iss.IssuedTokenRenewalThresholdPercentage, "#4");
			Assert.AreEqual (0, iss.IssuerChannelBehaviors.Count, "#5");
			Assert.IsNull (iss.LocalIssuerAddress, "#6");
			Assert.IsNull (iss.LocalIssuerBinding, "#7");
			Assert.AreEqual (0, iss.LocalIssuerChannelBehaviors.Count, "#8");
			Assert.AreEqual (TimeSpan.MaxValue, iss.MaxIssuedTokenCachingTime, "#9");
		}

		[Test]
		public void Peer ()
		{
			ClientCredentials c = new ClientCredentials ();
			// FIXME: implement
			PeerCredential peer = c.Peer;
		}

		[Test]
		public void ServiceCertificate ()
		{
			ClientCredentials c = new ClientCredentials ();
			// FIXME: implement
			X509CertificateRecipientClientCredential scert =
				c.ServiceCertificate;
		}

		[Test]
		public void UserName ()
		{
			ClientCredentials c = new ClientCredentials ();
			// FIXME: implement
			UserNamePasswordClientCredential userpass = c.UserName;
		}

		[Test]
		public void Windows ()
		{
			ClientCredentials c = new ClientCredentials ();
			WindowsClientCredential win = c.Windows;
			Assert.IsNotNull (win.ClientCredential, "#1");
			Assert.IsTrue (win.AllowNtlm, "#2");
			Assert.AreEqual (TokenImpersonationLevel.Identification, win.AllowedImpersonationLevel, "#3");
		}
	}
}
#endif
