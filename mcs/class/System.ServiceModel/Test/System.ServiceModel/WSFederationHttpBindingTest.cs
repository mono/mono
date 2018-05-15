//
// WSFederationHttpBindingTest.cs
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
using System.IdentityModel.Tokens;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class WSFederationHttpBindingTest
	{
		[Test]
		public void DefaultValues ()
		{
			WSFederationHttpBinding b= new WSFederationHttpBinding ();
			// common tests
			DefaultValues (b, "http");

			// WSFederationHttpSecurity
			WSFederationHttpSecurity sec = b.Security;
			Assert.IsNotNull (sec, "#2-1");
			Assert.AreEqual (WSFederationHttpSecurityMode.Message, sec.Mode, "#2-2");
			// Security.Message
			FederatedMessageSecurityOverHttp msg = sec.Message;
			Assert.IsNotNull (msg, "#2-3");
			Assert.AreEqual (SecurityAlgorithmSuite.Default,
					 msg.AlgorithmSuite, "#2-3-2");
			Assert.AreEqual (SecurityKeyType.SymmetricKey,
					 msg.IssuedKeyType, "#2-3-3");
			Assert.AreEqual (true, msg.NegotiateServiceCredential, "#2-3-4");

			// Binding elements

			BindingElementCollection bec = b.CreateBindingElements ();
			Assert.AreEqual (4, bec.Count, "#5-1");
			Assert.AreEqual (typeof (TransactionFlowBindingElement),
				bec [0].GetType (), "#5-2");
			Assert.AreEqual (typeof (SymmetricSecurityBindingElement),
				bec [1].GetType (), "#5-3");
			Assert.AreEqual (typeof (TextMessageEncodingBindingElement),
				bec [2].GetType (), "#5-4");
			Assert.AreEqual (typeof (HttpTransportBindingElement),
				bec [3].GetType (), "#5-5");
		}

		[Test]
		[Category ("NotWorking")] // transport security
		public void DefaultValuesSecurityModeTransport ()
		{
			WSFederationHttpBinding b = new WSFederationHttpBinding (WSFederationHttpSecurityMode.TransportWithMessageCredential);
			// common tests.
			DefaultValues (b, "https");

			// WSFederationHttpSecurity
			WSFederationHttpSecurity sec = b.Security;
			Assert.IsNotNull (sec, "#2-1");
			Assert.AreEqual (WSFederationHttpSecurityMode.TransportWithMessageCredential, sec.Mode, "#2-2");
			// Security.Message
			FederatedMessageSecurityOverHttp msg = sec.Message;
			Assert.IsNotNull (msg, "#2-3");
			Assert.AreEqual (SecurityAlgorithmSuite.Default,
					 msg.AlgorithmSuite, "#2-3-2");
			Assert.AreEqual (SecurityKeyType.SymmetricKey,
					 msg.IssuedKeyType, "#2-3-3");
			Assert.AreEqual (true, msg.NegotiateServiceCredential, "#2-3-4");

			// Binding elements
			BindingElementCollection bec = b.CreateBindingElements ();
			Assert.AreEqual (4, bec.Count, "#5-1");
			Assert.AreEqual (typeof (TransactionFlowBindingElement),
				bec [0].GetType (), "#5-2");
			Assert.AreEqual (typeof (TransportSecurityBindingElement),
				bec [1].GetType (), "#5-3");
			Assert.AreEqual (typeof (TextMessageEncodingBindingElement),
				bec [2].GetType (), "#5-4");
			Assert.AreEqual (typeof (HttpsTransportBindingElement),
				bec [3].GetType (), "#5-5");
		}

		void DefaultValues (WSFederationHttpBinding b, string scheme)
		{
			Assert.AreEqual (false, b.BypassProxyOnLocal, "#1");
			Assert.AreEqual (HostNameComparisonMode.StrongWildcard,
				b.HostNameComparisonMode, "#2");
			Assert.AreEqual (0x80000, b.MaxBufferPoolSize, "#3");
			Assert.AreEqual (0x10000, b.MaxReceivedMessageSize, "#5");
			Assert.AreEqual (WSMessageEncoding.Text, b.MessageEncoding, "#6");
			Assert.IsNull (b.ProxyAddress, "#7");
			// FIXME: test b.ReaderQuotas
			Assert.AreEqual (scheme, b.Scheme, "#8");
			Assert.AreEqual (EnvelopeVersion.Soap12, b.EnvelopeVersion, "#9");
			Assert.AreEqual (65001, b.TextEncoding.CodePage, "#10"); // utf-8
			Assert.AreEqual (false, b.TransactionFlow, "#11");
			Assert.AreEqual (true, b.UseDefaultWebProxy, "#12");
			Assert.AreEqual (MessageVersion.Default, b.MessageVersion, "#14");
			Assert.IsNotNull (b.ReliableSession, "#15");
		}

/*
		[Test]
		public void DefaultMessageEncoding ()
		{
			WSHttpBinding b = new WSHttpBinding ();
			foreach (BindingElement be in b.CreateBindingElements ()) {
				MessageEncodingBindingElement mbe =
					be as MessageEncodingBindingElement;
				if (mbe == null)
					continue;
				MessageEncoderFactory f = mbe.CreateMessageEncoderFactory ();
				MessageEncoder e = f.Encoder;

				Assert.AreEqual (typeof (TextMessageEncodingBindingElement), mbe.GetType (), "#1-1");
				Assert.AreEqual (MessageVersion.Default, f.MessageVersion, "#2-1");
				Assert.AreEqual ("application/soap+xml; charset=utf-8", e.ContentType, "#3-1");
				Assert.AreEqual ("application/soap+xml", e.MediaType, "#3-2");
				return;
			}
			Assert.Fail ("No message encodiing binding element.");
		}

		[Test]
		public void DefaultHttpTransport ()
		{
			WSHttpBinding b = new WSHttpBinding ();
			foreach (BindingElement be in b.CreateBindingElements ()) {
				HttpTransportBindingElement tbe =
					be as HttpTransportBindingElement;
				if (tbe == null)
					continue;

				Assert.AreEqual (false, tbe.AllowCookies, "#1");
				Assert.AreEqual (AuthenticationSchemes.Anonymous, tbe.AuthenticationScheme, "#2");
				Assert.AreEqual (false, tbe.BypassProxyOnLocal, "#3");
				Assert.AreEqual (HostNameComparisonMode.StrongWildcard, tbe.HostNameComparisonMode, "#4");
				Assert.AreEqual (true, tbe.KeepAliveEnabled, "#5");
				Assert.AreEqual (false, tbe.ManualAddressing, "#6");
				Assert.AreEqual (0x80000, tbe.MaxBufferPoolSize, "#7");
				Assert.AreEqual (0x10000, tbe.MaxBufferSize, "#8");
				Assert.AreEqual (0x10000, tbe.MaxReceivedMessageSize, "#9");
				Assert.IsNull (tbe.ProxyAddress, "#10");
				Assert.AreEqual (AuthenticationSchemes.Anonymous, tbe.ProxyAuthenticationScheme, "#11");
				Assert.AreEqual ("", tbe.Realm, "#12");
				Assert.AreEqual (TransferMode.Buffered, tbe.TransferMode, "#13");
				Assert.AreEqual (true, tbe.UseDefaultWebProxy, "#14");

				return;
			}
			Assert.Fail ("No transport binding element.");
		}

		[Test]
		public void DefaultTransactionFlow ()
		{
			WSHttpBinding b = new WSHttpBinding ();
			foreach (BindingElement be in b.CreateBindingElements ()) {
				TransactionFlowBindingElement tbe =
					be as TransactionFlowBindingElement;
				if (tbe == null)
					continue;

				Assert.AreEqual (TransactionProtocol.WSAtomicTransactionOctober2004,
					tbe.TransactionProtocol, "#1");

				return;
			}
			Assert.Fail ("No transaction flow binding element.");
		}

		[Test]
		public void CreateMessageSecurity ()
		{
			Assert.IsNull (new MyWSBinding (SecurityMode.None).CreateMessageSecurityEx (), "None");
			Assert.IsNotNull (new MyWSBinding (SecurityMode.Message).CreateMessageSecurityEx (), "Message");
			Assert.IsNull (new MyWSBinding (SecurityMode.Transport).CreateMessageSecurityEx (), "Transport");
		}

		[Test]
		public void DefaultMessageSecurity ()
		{
			WSHttpBinding b = new WSHttpBinding ();
			SymmetricSecurityBindingElement sbe = b.CreateBindingElements ().Find<SymmetricSecurityBindingElement> ();
			Assert.IsNotNull (sbe, "#0");

			SecureConversationSecurityTokenParameters p =
				sbe.ProtectionTokenParameters as SecureConversationSecurityTokenParameters;
			Assert.IsNotNull (p, "#1");

			Assert.AreEqual (SecurityAlgorithmSuite.Default,
				sbe.DefaultAlgorithmSuite, "#2");

			SupportingTokenParameters s =
				sbe.EndpointSupportingTokenParameters;
			Assert.IsNotNull (s, "#3");
			Assert.AreEqual (0, s.Endorsing.Count, "#3-1");
			Assert.AreEqual (0, s.Signed.Count, "#3-2");
			Assert.AreEqual (0, s.SignedEndorsing.Count, "#3-3");
			Assert.AreEqual (0, s.SignedEncrypted.Count, "#3-4");

			Assert.AreEqual (0, sbe.OperationSupportingTokenParameters.Count, "#4");

			s = sbe.OptionalEndpointSupportingTokenParameters;
			Assert.IsNotNull (s, "#5");
			Assert.AreEqual (0, s.Endorsing.Count, "#5-1");
			Assert.AreEqual (0, s.Signed.Count, "#5-2");
			Assert.AreEqual (0, s.SignedEndorsing.Count, "#5-3");
			Assert.AreEqual (0, s.SignedEncrypted.Count, "#5-4");
			Assert.AreEqual (0, sbe.OptionalOperationSupportingTokenParameters.Count, "#6");
		}

		[Test]
		public void MessageSecurityNoSecureConversation ()
		{
			WSHttpBinding b = new WSHttpBinding ();
			b.Security.Message.EstablishSecurityContext = false;
			SymmetricSecurityBindingElement sbe = b.CreateBindingElements ().Find<SymmetricSecurityBindingElement> ();
			Assert.IsNotNull (sbe, "#0");

			Assert.AreEqual (
				typeof (SspiSecurityTokenParameters),
				sbe.ProtectionTokenParameters.GetType (), "#1");
			// no worthy to check SSPI security as we never support it.

			b.Security.Message.ClientCredentialType = MessageCredentialType.None;
			sbe = b.CreateBindingElements ().Find<SymmetricSecurityBindingElement> ();
			SslSecurityTokenParameters ssltp =
				sbe.ProtectionTokenParameters
				as SslSecurityTokenParameters;
			Assert.IsNotNull(ssltp, "#2-1");
			Assert.AreEqual (true, ssltp.RequireCancellation, "#2-2");
			Assert.AreEqual (false, ssltp.RequireClientCertificate, "#2-3");

			b.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
			sbe = b.CreateBindingElements ().Find<SymmetricSecurityBindingElement> ();
			ssltp = sbe.ProtectionTokenParameters as SslSecurityTokenParameters;
			Assert.IsNotNull(ssltp, "#3-1");

			// No NegotiateServiceCredential modes ...

			b.Security.Message.NegotiateServiceCredential = false;
			b.Security.Message.ClientCredentialType = MessageCredentialType.Windows;
			sbe = b.CreateBindingElements ().Find<SymmetricSecurityBindingElement> ();
			KerberosSecurityTokenParameters ktp =
				sbe.ProtectionTokenParameters
				as KerberosSecurityTokenParameters;
			Assert.IsNotNull (ktp, "#4-1");
			// no worthy of testing windows-only Kerberos stuff

			b.Security.Message.ClientCredentialType = MessageCredentialType.None;
			sbe = b.CreateBindingElements ().Find<SymmetricSecurityBindingElement> ();
			X509SecurityTokenParameters x509tp =
				sbe.ProtectionTokenParameters
				as X509SecurityTokenParameters;
			Assert.IsNotNull (x509tp, "#5-1");
			Assert.AreEqual (X509KeyIdentifierClauseType.Thumbprint, x509tp.X509ReferenceStyle, "#5-2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void BuildListenerWithoutServiceCertificate ()
		{
			ServiceHost host = new ServiceHost (typeof (Foo));
			WSHttpBinding binding = new WSHttpBinding ();
			binding.Security.Message.ClientCredentialType =
				MessageCredentialType.IssuedToken;
			host.AddServiceEndpoint ("Foo", binding, "http://localhost:8080");
			host.Open ();
		}
*/

		[ServiceContract]
		class Foo
		{
			[OperationContract]
			public void SayWhat () { }
		}

		class MyWSBinding : WSHttpBinding
		{
			public MyWSBinding (SecurityMode mode)
				: base (mode)
			{
			}

			public SecurityBindingElement CreateMessageSecurityEx ()
			{
				return CreateMessageSecurity ();
			}
		}
	}
}
#endif