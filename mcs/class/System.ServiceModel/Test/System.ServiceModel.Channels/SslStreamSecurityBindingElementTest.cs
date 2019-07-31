//
// SslStreamSecurityBindingElementTest.cs
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
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class SslStreamSecurityBindingElementTest
	{
		[Test]
		[Category ("NotWorking")]
		public void DefaultValues ()
		{
			SslStreamSecurityBindingElement bel =
				new SslStreamSecurityBindingElement ();
			Assert.IsNotNull (bel.IdentityVerifier, "#1");
			Assert.AreEqual (false, bel.RequireClientCertificate, "#2");
			Assert.AreEqual ("<msf:SslTransportSecurity xmlns:msf=\"http://schemas.microsoft.com/ws/2006/05/framing/policy\" />", bel.GetTransportTokenAssertion ().OuterXml, "#3");
		}

		StreamSecurityUpgradeProvider CreateClientProvider (params object [] parameters)
		{
			SslStreamSecurityBindingElement bel =
				new SslStreamSecurityBindingElement ();
			BindingParameterCollection pl =
				new BindingParameterCollection ();
			foreach (object o in parameters)
				pl.Add (o);
			BindingContext ctx = new BindingContext (
				new CustomBinding (new HttpTransportBindingElement ()), pl);
			return bel.BuildClientStreamUpgradeProvider (ctx)
				as StreamSecurityUpgradeProvider;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void ClientProviderCreateAcceptorBeforeOpen ()
		{
			StreamSecurityUpgradeProvider p = CreateClientProvider ();
			p.CreateUpgradeAcceptor ();
		}

		[Test]
		[Category ("NotWorking")]
		public void ClientAcceptUpgradeWithoutServiceCertificate ()
		{
			StreamSecurityUpgradeProvider p = CreateClientProvider ();
			Assert.IsNotNull (p, "#1");
			Assert.IsNull (p.Identity, "#2"); // not yet, before Open().
			p.Open ();
			StreamUpgradeAcceptor a = p.CreateUpgradeAcceptor ();
			try {
				Stream s = a.AcceptUpgrade (new MemoryStream (new byte [] {1, 2, 3, 4, 5}));
				Assert.Fail ("It should somehow raise an error."); // on Winfx it is unwise ArgumentNullException
			} catch (Exception) {
			} finally {
				p.Close ();
			}
		}

		[Test]
		[Ignore ("find out how to fill serverCertificate")]
		public void ClientAcceptUpgrade ()
		{
			ServiceCredentials cred = new ServiceCredentials ();
			X509Certificate2 cert = 
				new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.cer"));
			cred.ServiceCertificate.Certificate = cert;
			X509CertificateEndpointIdentity ident =
				new X509CertificateEndpointIdentity (cert);
			StreamSecurityUpgradeProvider p = CreateClientProvider (cred, ident);
			p.Open ();
			try {
				StreamSecurityUpgradeAcceptor a =
					p.CreateUpgradeAcceptor ()
					as StreamSecurityUpgradeAcceptor;
				Assert.IsNotNull (a, "#1");
				SecurityMessageProperty prop =
					a.GetRemoteSecurity ();
				Assert.IsNull (prop, "#2"); // hmm
				Stream s = a.AcceptUpgrade (new MemoryStream (new byte [] {1, 2, 3, 4, 5}));
			} finally {
				p.Close ();
			}
		}
	}
}
#endif
