//
// WSHttpBindingTest.cs
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
using System.Net;
using System.Net.Security;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class WSHttpBindingTest
	{
		[Test]
		public void DefaultValues ()
		{
			WSHttpBinding b= new WSHttpBinding ();
			// common tests
			DefaultValues (b, "http");

			// WSHttpSecurity
			WSHttpSecurity sec = b.Security;
			Assert.IsNotNull (sec, "#2-1");
			Assert.AreEqual (SecurityMode.Message, sec.Mode, "#2-2");
			// Security.Message
			NonDualMessageSecurityOverHttp msg = sec.Message;
			Assert.IsNotNull (msg, "#2-3");
			Assert.AreEqual (true, msg.EstablishSecurityContext, "#2-3-1");
			Assert.AreEqual (SecurityAlgorithmSuite.Default,
					 msg.AlgorithmSuite, "#2-3-2");
			// it is not worthy of test, just for checking default value.
			Assert.AreEqual (MessageCredentialType.Windows,
					 msg.ClientCredentialType, "#2-3-3");
			Assert.AreEqual (true, msg.NegotiateServiceCredential, "#2-3-4");
			// FIXME: test Security.Transport
			Assert.IsNotNull (sec.Transport, "#2-4");

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
		public void DefaultValuesSecurityModeTransport ()
		{
			WSHttpBinding b = new WSHttpBinding (SecurityMode.Transport);
			// common tests.
			DefaultValues (b, "https");

			// WSHttpSecurity
			WSHttpSecurity sec = b.Security;
			Assert.IsNotNull (sec, "#2-1");
			Assert.AreEqual (SecurityMode.Transport, sec.Mode, "#2-2");
			// Security.Message
			NonDualMessageSecurityOverHttp msg = sec.Message;
			Assert.IsNotNull (msg, "#2-3");
			Assert.AreEqual (true, msg.EstablishSecurityContext, "#2-3-1");
			Assert.AreEqual (SecurityAlgorithmSuite.Default,
					 msg.AlgorithmSuite, "#2-3-2");
			// it is not worthy of test, just for checking default value.
			Assert.AreEqual (MessageCredentialType.Windows,
					 msg.ClientCredentialType, "#2-3-3");
			Assert.AreEqual (true, msg.NegotiateServiceCredential, "#2-3-4");
			// FIXME: test Security.Transport
			Assert.IsNotNull (sec.Transport, "#2-4");

			// Binding elements
			BindingElementCollection bec = b.CreateBindingElements ();
			Assert.AreEqual (3, bec.Count, "#5-1");
			Assert.AreEqual (typeof (TransactionFlowBindingElement),
				bec [0].GetType (), "#5-2");
			Assert.AreEqual (typeof (TextMessageEncodingBindingElement),
				bec [1].GetType (), "#5-3");
			Assert.AreEqual (typeof (HttpsTransportBindingElement),
				bec [2].GetType (), "#5-4");
		}

		void DefaultValues (WSHttpBinding b, string scheme)
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
			Assert.AreEqual (false, b.AllowCookies, "#13");
			Assert.AreEqual (MessageVersion.Default, b.MessageVersion, "#14");
			Assert.IsNotNull (b.ReliableSession, "#15");
		}

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
			SymmetricSecurityBindingElement scbe =
				p.BootstrapSecurityBindingElement as SymmetricSecurityBindingElement;
			Assert.IsNotNull (scbe, "#1.1");
			// since the default w/o SecureConv is SSPI ...
			Assert.IsTrue (scbe.ProtectionTokenParameters is SspiSecurityTokenParameters, "#1.2");

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
			Assert.AreEqual (SecurityTokenInclusionMode.Never, x509tp.InclusionMode, "#5-3");

			b.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
			sbe = b.CreateBindingElements ().Find<SymmetricSecurityBindingElement> ();
			Assert.AreEqual (1, sbe.EndpointSupportingTokenParameters.Endorsing.Count, "#6-0");
			x509tp = sbe.EndpointSupportingTokenParameters.Endorsing [0] as X509SecurityTokenParameters;
			Assert.IsNotNull (x509tp, "#6-1");
			Assert.AreEqual (X509KeyIdentifierClauseType.Thumbprint, x509tp.X509ReferenceStyle, "#6-2");
			Assert.AreEqual (SecurityTokenInclusionMode.AlwaysToRecipient, x509tp.InclusionMode, "#6-3");
			Assert.AreEqual (false, x509tp.RequireDerivedKeys, "#6-4");
			x509tp = sbe.ProtectionTokenParameters as X509SecurityTokenParameters;
			Assert.IsNotNull (x509tp, "#7-1");
			Assert.AreEqual (X509KeyIdentifierClauseType.Thumbprint, x509tp.X509ReferenceStyle, "#7-2");
			Assert.AreEqual (SecurityTokenInclusionMode.Never, x509tp.InclusionMode, "#7-3");
			Assert.AreEqual (true, x509tp.RequireDerivedKeys, "#7-4");
			Assert.AreEqual (true, sbe.RequireSignatureConfirmation, "#8");
		}

		[Test]
		public void MessageSecurityCertificateNego ()
		{
			WSHttpBinding binding = new WSHttpBinding ();
			binding.Security.Message.ClientCredentialType =
				MessageCredentialType.Certificate;
			SymmetricSecurityBindingElement sbe =
				binding.CreateBindingElements ().Find<SymmetricSecurityBindingElement> ();
			Assert.IsNotNull (sbe, "#1");
			Assert.AreEqual (false, sbe.RequireSignatureConfirmation, "#1-2");

			SecureConversationSecurityTokenParameters sp =
				sbe.ProtectionTokenParameters
				as SecureConversationSecurityTokenParameters;
			Assert.IsNotNull (sp, "#2");
			SymmetricSecurityBindingElement spbe =
				sp.BootstrapSecurityBindingElement
				as SymmetricSecurityBindingElement;
			Assert.IsNotNull (spbe, "#3");
			SslSecurityTokenParameters p =
				spbe.ProtectionTokenParameters
				as SslSecurityTokenParameters;
			Assert.IsNotNull (p, "#4");
			Assert.AreEqual (SecurityTokenReferenceStyle.Internal,
					 p.ReferenceStyle, "#5");
			Assert.AreEqual (SecurityTokenInclusionMode.AlwaysToRecipient,
					 p.InclusionMode, "#6");
		}

		[Test]
		public void MessageSecuritySPNego ()
		{
			WSHttpBinding binding = new WSHttpBinding ();
			SymmetricSecurityBindingElement sbe =
				binding.CreateBindingElements ().Find<SymmetricSecurityBindingElement> ();
			Assert.IsNotNull (sbe, "#1");
			Assert.AreEqual (false, sbe.RequireSignatureConfirmation, "#1-2");

			SecureConversationSecurityTokenParameters sp =
				sbe.ProtectionTokenParameters
				as SecureConversationSecurityTokenParameters;
			Assert.IsNotNull (sp, "#2");
			SymmetricSecurityBindingElement spbe =
				sp.BootstrapSecurityBindingElement
				as SymmetricSecurityBindingElement;
			Assert.IsNotNull (spbe, "#3");
			SspiSecurityTokenParameters p =
				spbe.ProtectionTokenParameters
				as SspiSecurityTokenParameters;
			Assert.IsNotNull (p, "#4");
			Assert.AreEqual (SecurityTokenReferenceStyle.Internal,
					 p.ReferenceStyle, "#5");
			Assert.AreEqual (SecurityTokenInclusionMode.AlwaysToRecipient,
					 p.InclusionMode, "#6");
			Assert.AreEqual (0, sbe.EndpointSupportingTokenParameters.Signed.Count, "#7");
			Assert.AreEqual (0, sbe.EndpointSupportingTokenParameters.SignedEncrypted.Count, "#8");
			Assert.AreEqual (0, sbe.EndpointSupportingTokenParameters.Endorsing.Count, "#9");
			Assert.AreEqual (0, sbe.EndpointSupportingTokenParameters.SignedEndorsing.Count, "#10");
			Assert.AreEqual (0, spbe.EndpointSupportingTokenParameters.Signed.Count, "#11");
			Assert.AreEqual (0, spbe.EndpointSupportingTokenParameters.SignedEncrypted.Count, "#12");
			Assert.AreEqual (0, spbe.EndpointSupportingTokenParameters.Endorsing.Count, "#13");
			Assert.AreEqual (0, spbe.EndpointSupportingTokenParameters.SignedEndorsing.Count, "#14");

			Assert.AreEqual (0, sbe.OptionalEndpointSupportingTokenParameters.Signed.Count, "#17");
			Assert.AreEqual (0, sbe.OptionalEndpointSupportingTokenParameters.SignedEncrypted.Count, "#18");
			Assert.AreEqual (0, sbe.OptionalEndpointSupportingTokenParameters.Endorsing.Count, "#19");
			Assert.AreEqual (0, sbe.OptionalEndpointSupportingTokenParameters.SignedEndorsing.Count, "#110");
			Assert.AreEqual (0, spbe.OptionalEndpointSupportingTokenParameters.Signed.Count, "#21");
			Assert.AreEqual (0, spbe.OptionalEndpointSupportingTokenParameters.SignedEncrypted.Count, "#22");
			Assert.AreEqual (0, spbe.OptionalEndpointSupportingTokenParameters.Endorsing.Count, "#23");
			Assert.AreEqual (0, spbe.OptionalEndpointSupportingTokenParameters.SignedEndorsing.Count, "#24");
		}

		[Test]
		public void MessageSecurityUserName ()
		{
			WSHttpBinding binding = new WSHttpBinding ();
			binding.Security.Message.NegotiateServiceCredential = false;
			binding.Security.Message.EstablishSecurityContext = false;
			binding.Security.Message.ClientCredentialType =
				MessageCredentialType.UserName;
			SymmetricSecurityBindingElement sbe =
				binding.CreateBindingElements ().Find<SymmetricSecurityBindingElement> ();
			Assert.IsNotNull (sbe, "#1");
			Assert.AreEqual (false, sbe.RequireSignatureConfirmation, "#1-2");

			X509SecurityTokenParameters sp =
				sbe.ProtectionTokenParameters
				as X509SecurityTokenParameters;
			Assert.IsNotNull (sp, "#2");
			Assert.AreEqual (SecurityTokenReferenceStyle.Internal,
					 sp.ReferenceStyle, "#3");
			Assert.AreEqual (SecurityTokenInclusionMode.Never,
					 sp.InclusionMode, "#4");

			UserNameSecurityTokenParameters up =
				sbe.EndpointSupportingTokenParameters.SignedEncrypted [0]
				as UserNameSecurityTokenParameters;
			Assert.AreEqual (SecurityTokenReferenceStyle.Internal,
					 up.ReferenceStyle, "#5");
			Assert.AreEqual (SecurityTokenInclusionMode.AlwaysToRecipient,
					 up.InclusionMode, "#6");
		}

		[Test]
		[Category ("NotWorking")]
		public void MessageSecurityIssuedToken ()
		{
			WSHttpBinding binding = new WSHttpBinding ();
			binding.Security.Message.EstablishSecurityContext = false;
			binding.Security.Message.ClientCredentialType =
				MessageCredentialType.IssuedToken;
			SymmetricSecurityBindingElement sbe =
				binding.CreateBindingElements ().Find<SymmetricSecurityBindingElement> ();
			Assert.IsNotNull (sbe, "#1");
			Assert.AreEqual (0, sbe.EndpointSupportingTokenParameters.Signed.Count, "#1-1");
			Assert.AreEqual (1, sbe.EndpointSupportingTokenParameters.Endorsing.Count, "#1-2");
			Assert.AreEqual (0, sbe.EndpointSupportingTokenParameters.SignedEndorsing.Count, "#1-3");
			Assert.AreEqual (0, sbe.EndpointSupportingTokenParameters.SignedEncrypted.Count, "#1-4");
			IssuedSecurityTokenParameters p =
				sbe.EndpointSupportingTokenParameters.Endorsing [0]
				as IssuedSecurityTokenParameters;
			Assert.IsNotNull (p, "#2");
			Assert.IsNotNull (p.ClaimTypeRequirements, "#2-1");
			Assert.AreEqual (1, p.ClaimTypeRequirements.Count, "#2-2");
			ClaimTypeRequirement r = p.ClaimTypeRequirements [0];
			Assert.AreEqual (ClaimTypes.PPID, r.ClaimType, "#3-1");
			Assert.IsFalse (r.IsOptional, "#3-2");
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
			host.AddServiceEndpoint (typeof (Foo).FullName, binding, "http://localhost:8080");
			host.Open ();
		}

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
