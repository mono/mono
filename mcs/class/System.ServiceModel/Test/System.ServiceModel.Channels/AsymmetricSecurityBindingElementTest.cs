//
// AsymmetricSecurityBindingElementTest.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Threading;
using System.Xml;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class AsymmetricSecurityBindingElementTest
	{
		static X509Certificate2 cert = new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");
		static X509Certificate2 cert2 = new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.cer"));

		// InitiatorTokenParameters should have asymmetric key.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")] // this test unnecessarily requires some internal processing order
		public void ClientInitiatorHasNoKeys1 ()
		{
			ClientInitiatorHasNoKeysCore (false, MessageProtectionOrder.SignBeforeEncrypt);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")] // this test unnecessarily requires some internal processing order
		public void ClientInitiatorHasNoKeys2 ()
		{
			ClientInitiatorHasNoKeysCore (true, MessageProtectionOrder.SignBeforeEncrypt);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")] // this test unnecessarily requires some internal processing order
		public void ClientInitiatorHasNoKeys3 ()
		{
			ClientInitiatorHasNoKeysCore (false, MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")] // this test unnecessarily requires some internal processing order
		public void ClientInitiatorHasNoKeys4 ()
		{
			ClientInitiatorHasNoKeysCore (true, MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature);
		}

		public void ClientInitiatorHasNoKeysCore (bool deriveKeys, MessageProtectionOrder order)
		{
			AsymmetricSecurityBindingElement sbe =
				new AsymmetricSecurityBindingElement ();
			sbe.InitiatorTokenParameters =
				new UserNameSecurityTokenParameters ();
			sbe.RecipientTokenParameters =
				new X509SecurityTokenParameters ();
			sbe.SetKeyDerivation (deriveKeys);
			sbe.MessageProtectionOrder = order;
			TransportBindingElement tbe = new HandlerTransportBindingElement (delegate (Message input) {
				// funky, but .NET does not raise an error
				// until it writes the message to somewhere.
				// That is, it won't raise an error if this
				// HandlerTransportBindingElement does not
				// write the input message to somewhere.
				// It is an obvious bug.
				input.WriteMessage (XmlWriter.Create (TextWriter.Null));
				throw new Exception ();
			});
			CustomBinding binding = new CustomBinding (sbe, tbe);
			EndpointAddress address = new EndpointAddress (
				new Uri ("stream:dummy"),
				new X509CertificateEndpointIdentity (cert2));
			CalcProxy proxy = new CalcProxy (binding, address);
			proxy.ClientCredentials.UserName.UserName = "mono";
			proxy.Open ();
			// Until here the wrong parameters are not checked.
			proxy.Sum (1, 2);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		[Category ("NotWorking")]
		public void ServiceRecipientHasNoKeys ()
		{
			AsymmetricSecurityBindingElement sbe =
				new AsymmetricSecurityBindingElement ();
			sbe.InitiatorTokenParameters =
				new X509SecurityTokenParameters ();
			sbe.RecipientTokenParameters =
				new UserNameSecurityTokenParameters ();
			//sbe.SetKeyDerivation (false);
			//sbe.MessageProtectionOrder = MessageProtectionOrder.SignBeforeEncrypt;
			CustomBinding binding = new CustomBinding (sbe,
				new HttpTransportBindingElement ());
			IChannelListener<IReplyChannel> l =
				binding.BuildChannelListener<IReplyChannel> (new Uri ("http://localhost:" + NetworkHelpers.FindFreePort ()), new BindingParameterCollection ());
			try {
				l.Open ();
			} finally {
				if (l.State == CommunicationState.Opened)
					l.Close ();
			}
		}

		Message tmp_request, tmp_reply;

		[Test]
		[ExpectedException (typeof (MessageSecurityException))]
		// after having to fix several issues, I forgot what I originally wanted to test here ...
		[Ignore ("It causes some weird failure and port blocking ...")]
		public void VerifyX509MessageSecurityAtService ()
		{
			AsymmetricSecurityBindingElement clisbe =
				new AsymmetricSecurityBindingElement ();
			clisbe.InitiatorTokenParameters =
				new X509SecurityTokenParameters ();
			clisbe.RecipientTokenParameters =
				new X509SecurityTokenParameters ();

			AsymmetricSecurityBindingElement svcsbe =
				new AsymmetricSecurityBindingElement ();
			svcsbe.InitiatorTokenParameters =
				new X509SecurityTokenParameters ();
			svcsbe.RecipientTokenParameters =
				new X509SecurityTokenParameters ();

			CustomBinding b_req = new CustomBinding (clisbe,
				new HttpTransportBindingElement ());

			b_req.ReceiveTimeout = b_req.SendTimeout = TimeSpan.FromSeconds (10);

			CustomBinding b_res = new CustomBinding (svcsbe, new HttpTransportBindingElement ());
			b_res.ReceiveTimeout = b_res.SendTimeout = TimeSpan.FromSeconds (10);

			EndpointAddress remaddr = new EndpointAddress (
				new Uri ("http://localhost:" + NetworkHelpers.FindFreePort ()),
				new X509CertificateEndpointIdentity (cert2));
			CalcProxy proxy = null;
			ServiceHost host = new ServiceHost (typeof (CalcService));
			host.AddServiceEndpoint (typeof (ICalc), b_res, "http://localhost:" + NetworkHelpers.FindFreePort ());

			ServiceCredentials cred = new ServiceCredentials ();
			cred.ServiceCertificate.Certificate = cert;
			host.Description.Behaviors.Add (cred);
			try {
				host.Open ();

				proxy = new CalcProxy (b_req, remaddr);
				proxy.ClientCredentials.ClientCertificate.Certificate = cert;

				// FIXME: on WinFX, when this Begin method
				// is invoked before the listener setup, it
				// somehow works, while ours doesn't.
				//IAsyncResult result = proxy.BeginSum (1, 2, null, null);
				//Assert.AreEqual (3, proxy.EndSum (result));
				Assert.AreEqual (3, proxy.Sum (1, 2));
			} finally {
				if (host.State == CommunicationState.Opened)
					host.Close ();
			}
		}

		[Test]
		public void SetKeyDerivation ()
		{
			AsymmetricSecurityBindingElement be;
			X509SecurityTokenParameters p, p2;

			be = new AsymmetricSecurityBindingElement ();
			p = new X509SecurityTokenParameters ();
			p2 = new X509SecurityTokenParameters ();
			be.InitiatorTokenParameters = p;
			be.RecipientTokenParameters = p2;
			be.SetKeyDerivation (false);
			Assert.AreEqual (false, p.RequireDerivedKeys, "#1");
			Assert.AreEqual (false, p2.RequireDerivedKeys, "#2");

			be = new AsymmetricSecurityBindingElement ();
			p = new X509SecurityTokenParameters ();
			p2 = new X509SecurityTokenParameters ();
			be.SetKeyDerivation (false); // set in prior - makes no sense
			be.InitiatorTokenParameters = p;
			be.RecipientTokenParameters = p2;
			Assert.AreEqual (true, p.RequireDerivedKeys, "#3");
			Assert.AreEqual (true, p2.RequireDerivedKeys, "#4");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void RejectInclusionModeNever ()
		{
			AsymmetricSecurityBindingElement sbe =
				new AsymmetricSecurityBindingElement ();
			sbe.InitiatorTokenParameters = sbe.RecipientTokenParameters =
				new X509SecurityTokenParameters (
					X509KeyIdentifierClauseType.Thumbprint,
					// this leads to the failure.
					SecurityTokenInclusionMode.Never);
			ServiceHost host = new ServiceHost (typeof (Foo));
			HttpTransportBindingElement hbe =
				new HttpTransportBindingElement ();
			CustomBinding binding = new CustomBinding (sbe, hbe);
			host.AddServiceEndpoint (typeof (IFoo),
				binding, new Uri ("http://localhost:" + NetworkHelpers.FindFreePort ()));
			ServiceCredentials cred = new ServiceCredentials ();
			cred.ServiceCertificate.Certificate =
				new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");
			cred.ClientCertificate.Authentication.CertificateValidationMode =
				X509CertificateValidationMode.None;
			host.Description.Behaviors.Add (cred);
			try {
				host.Open ();
			} finally {
				if (host.State == CommunicationState.Opened)
					host.Close ();
			}
		}
	}
}
#endif
