//
// SecurityBindingElementTest.cs
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
using System.Xml;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class SecurityBindingElementTest
	{
		#region Factory methods

		[Test]
		public void CreateAnonymousForCertificateBindingElement ()
		{
			SymmetricSecurityBindingElement be =
				SecurityBindingElement.CreateAnonymousForCertificateBindingElement ();

			SecurityAssert.AssertSymmetricSecurityBindingElement (
				SecurityAlgorithmSuite.Default,
				true, // IncludeTimestamp
				SecurityKeyEntropyMode.CombinedEntropy,
				MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature,
				MessageSecurityVersion.Default,
				true, // RequireSignatureConfirmation
				SecurityHeaderLayout.Strict,
				// EndpointSupportingTokenParameters: endorsing, signed, signedEncrypted, signedEndorsing (by count)
				0, 0, 0, 0,
				// ProtectionTokenParameters
				true, SecurityTokenInclusionMode.Never, SecurityTokenReferenceStyle.Internal, true,
				// LocalClientSettings
				true, 60, true,

				be, "");

			// test ProtectionTokenParameters
			X509SecurityTokenParameters tp =
				be.ProtectionTokenParameters
				as X509SecurityTokenParameters;
			Assert.IsNotNull (tp, "#2-1");
			SecurityAssert.AssertSecurityTokenParameters (
				SecurityTokenInclusionMode.Never,
				SecurityTokenReferenceStyle.Internal, 
				true, tp, "Protection");
			Assert.AreEqual (X509KeyIdentifierClauseType.Thumbprint, tp.X509ReferenceStyle, "#2-2");
		}

		[Test]
		public void CreateIssuedTokenBindingElement1 ()
		{
			IssuedSecurityTokenParameters tp =
				new IssuedSecurityTokenParameters ();
			SymmetricSecurityBindingElement be =
				SecurityBindingElement.CreateIssuedTokenBindingElement (tp);

			SecurityAssert.AssertSymmetricSecurityBindingElement (
				SecurityAlgorithmSuite.Default,
				true, // IncludeTimestamp
				SecurityKeyEntropyMode.CombinedEntropy,
				MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature,
				MessageSecurityVersion.Default,
				false, // RequireSignatureConfirmation
				SecurityHeaderLayout.Strict,
				// EndpointSupportingTokenParameters: endorsing, signed, signedEncrypted, signedEndorsing (by count)
				0, 0, 0, 0,
				// ProtectionTokenParameters
				true, SecurityTokenInclusionMode.AlwaysToRecipient, SecurityTokenReferenceStyle.Internal, true,
				// LocalClientSettings
				true, 60, true,

				be, "");

			// test ProtectionTokenParameters
			Assert.AreEqual (tp, be.ProtectionTokenParameters, "#2-1");
			SecurityAssert.AssertSecurityTokenParameters (
				SecurityTokenInclusionMode.AlwaysToRecipient,
				SecurityTokenReferenceStyle.Internal, 
				true, tp, "Protection");
		}

		[Test]
		public void CreateIssuedTokenForCertificateBindingElement1 ()
		{
			IssuedSecurityTokenParameters tp =
				new IssuedSecurityTokenParameters ();
			SymmetricSecurityBindingElement be =
				SecurityBindingElement.CreateIssuedTokenForCertificateBindingElement (tp);

			SecurityAssert.AssertSymmetricSecurityBindingElement (
				SecurityAlgorithmSuite.Default,
				true, // IncludeTimestamp
				SecurityKeyEntropyMode.CombinedEntropy,
				MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature,
				MessageSecurityVersion.Default,
				true, // RequireSignatureConfirmation
				SecurityHeaderLayout.Strict,
				// EndpointSupportingTokenParameters: endorsing, signed, signedEncrypted, signedEndorsing (by count)
				1, 0, 0, 0,
				// ProtectionTokenParameters
				true, SecurityTokenInclusionMode.Never, SecurityTokenReferenceStyle.Internal, true,
				// LocalClientSettings
				true, 60, true,

				be, "");

			// test ProtectionTokenParameters
			X509SecurityTokenParameters ptp =
				be.ProtectionTokenParameters
				as X509SecurityTokenParameters;
			Assert.IsNotNull (ptp, "#2-1");
			SecurityAssert.AssertSecurityTokenParameters (
				SecurityTokenInclusionMode.Never,
				SecurityTokenReferenceStyle.Internal, 
				true, ptp, "Protection");
			Assert.AreEqual (X509KeyIdentifierClauseType.Thumbprint, ptp.X509ReferenceStyle, "#2-2");

			Assert.AreEqual (tp, be.EndpointSupportingTokenParameters.Endorsing [0], "EndpointParams.Endorsing[0]");
		}

		[Test]
		public void CreateIssuedTokenForSslBindingElement1 ()
		{
			IssuedSecurityTokenParameters tp =
				new IssuedSecurityTokenParameters ();
			SymmetricSecurityBindingElement be =
				SecurityBindingElement.CreateIssuedTokenForSslBindingElement (tp);

			SecurityAssert.AssertSymmetricSecurityBindingElement (
				SecurityAlgorithmSuite.Default,
				true, // IncludeTimestamp
				SecurityKeyEntropyMode.CombinedEntropy,
				MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature,
				MessageSecurityVersion.Default,
				true, // RequireSignatureConfirmation
				SecurityHeaderLayout.Strict,
				// EndpointSupportingTokenParameters: endorsing, signed, signedEncrypted, signedEndorsing (by count)
				1, 0, 0, 0,
				// ProtectionTokenParameters
				true, SecurityTokenInclusionMode.AlwaysToRecipient, SecurityTokenReferenceStyle.Internal, true,
				// LocalClientSettings
				true, 60, true,

				be, "");

			Assert.AreEqual (tp, be.EndpointSupportingTokenParameters.Endorsing [0], "EndpointParams.Endorsing[0]");

			// FIXME: test ProtectionTokenParameters
		}

		[Test]
		public void CreateKerberosBindingElement ()
		{
			SymmetricSecurityBindingElement be =
				SecurityBindingElement.CreateKerberosBindingElement ();

			SecurityAssert.AssertSymmetricSecurityBindingElement (
				SecurityAlgorithmSuite.Basic128,
				true, // IncludeTimestamp
				SecurityKeyEntropyMode.CombinedEntropy,
				MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature,
				MessageSecurityVersion.Default,
				false, // RequireSignatureConfirmation
				SecurityHeaderLayout.Strict,
				// EndpointSupportingTokenParameters: endorsing, signed, signedEncrypted, signedEndorsing (by count)
				0, 0, 0, 0,
				// ProtectionTokenParameters
				true, SecurityTokenInclusionMode.Once, SecurityTokenReferenceStyle.Internal, true,
				// LocalClientSettings
				true, 60, true,

				be, "");

			// FIXME: test ProtectionTokenParameters
		}

		[Test]
		public void CreateSslNegotiationBindingElement ()
		{
			SymmetricSecurityBindingElement be =
				SecurityBindingElement.CreateSslNegotiationBindingElement (true, true);

			SecurityAssert.AssertSymmetricSecurityBindingElement (
				SecurityAlgorithmSuite.Default,
				true, // IncludeTimestamp
				SecurityKeyEntropyMode.CombinedEntropy,
				MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature,
				MessageSecurityVersion.Default,
				false, // RequireSignatureConfirmation
				SecurityHeaderLayout.Strict,
				// EndpointSupportingTokenParameters: endorsing, signed, signedEncrypted, signedEndorsing (by count)
				0, 0, 0, 0,
				// ProtectionTokenParameters
				true, SecurityTokenInclusionMode.AlwaysToRecipient, SecurityTokenReferenceStyle.Internal, true,
				// LocalClientSettings
				true, 60, true,

				be, "");

			// FIXME: also try different constructor arguments

			// test ProtectionTokenParameters
			Assert.AreEqual (typeof (SslSecurityTokenParameters), be.ProtectionTokenParameters.GetType (), "#1");
			SslSecurityTokenParameters sp = be.ProtectionTokenParameters as SslSecurityTokenParameters;
			Assert.AreEqual (true, sp.RequireCancellation, "#2");
			Assert.AreEqual (true, sp.RequireClientCertificate, "#3");
		}

		[Test]
		public void CreateSspiNegotiationBindingElement ()
		{
			SymmetricSecurityBindingElement be =
				SecurityBindingElement.CreateSspiNegotiationBindingElement ();

			SecurityAssert.AssertSymmetricSecurityBindingElement (
				SecurityAlgorithmSuite.Default,
				true, // IncludeTimestamp
				SecurityKeyEntropyMode.CombinedEntropy,
				MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature,
				MessageSecurityVersion.Default,
				false, // RequireSignatureConfirmation
				SecurityHeaderLayout.Strict,
				// EndpointSupportingTokenParameters: endorsing, signed, signedEncrypted, signedEndorsing (by count)
				0, 0, 0, 0,
				// ProtectionTokenParameters
				true, SecurityTokenInclusionMode.AlwaysToRecipient, SecurityTokenReferenceStyle.Internal, true,
				// LocalClientSettings
				true, 60, true,

				be, "");

			// FIXME: Try boolean argument as well.

			// FIXME: test ProtectionTokenParameters
		}

		[Test]
		public void CreateUserNameForCertificateBindingElement ()
		{
			SymmetricSecurityBindingElement be =
				SecurityBindingElement.CreateUserNameForCertificateBindingElement ();

			SecurityAssert.AssertSymmetricSecurityBindingElement (
				SecurityAlgorithmSuite.Default,
				true, // IncludeTimestamp
				SecurityKeyEntropyMode.CombinedEntropy,
				MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature,
				MessageSecurityVersion.Default,
				false, // RequireSignatureConfirmation
				SecurityHeaderLayout.Strict,
				// EndpointSupportingTokenParameters: endorsing, signed, signedEncrypted, signedEndorsing (by count)
				0, 0, 1, 0,
				// ProtectionTokenParameters
				true, SecurityTokenInclusionMode.Never, SecurityTokenReferenceStyle.Internal, true,
				// LocalClientSettings
				true, 60, true,

				be, "");

			UserNameSecurityTokenParameters up =
				be.EndpointSupportingTokenParameters.SignedEncrypted [0] as UserNameSecurityTokenParameters;
			// FIXME: test it

			// FIXME: test ProtectionTokenParameters
		}

		[Test]
		public void CreateUserNameForSslBindingElement ()
		{
			SymmetricSecurityBindingElement be =
				SecurityBindingElement.CreateUserNameForSslBindingElement ();

			SecurityAssert.AssertSymmetricSecurityBindingElement (
				SecurityAlgorithmSuite.Default,
				true, // IncludeTimestamp
				SecurityKeyEntropyMode.CombinedEntropy,
				MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature,
				MessageSecurityVersion.Default,
				false, // RequireSignatureConfirmation
				SecurityHeaderLayout.Strict,
				// EndpointSupportingTokenParameters: endorsing, signed, signedEncrypted, signedEndorsing (by count)
				0, 0, 1, 0,
				// ProtectionTokenParameters
				true, SecurityTokenInclusionMode.AlwaysToRecipient, SecurityTokenReferenceStyle.Internal, true,
				// LocalClientSettings
				true, 60, true,

				be, "");

			UserNameSecurityTokenParameters up =
				be.EndpointSupportingTokenParameters.SignedEncrypted [0] as UserNameSecurityTokenParameters;
			// FIXME: test it

			// FIXME: test ProtectionTokenParameters
		}

		// non-symmetric return value by definition, but still
		// returns symmetric binding elements.

		[Test]
		public void CreateSecureConversationBindingElement ()
		{
			SymmetricSecurityBindingElement be =
				SecurityBindingElement.CreateSecureConversationBindingElement (new SymmetricSecurityBindingElement ())
				as SymmetricSecurityBindingElement;

			SecurityAssert.AssertSymmetricSecurityBindingElement (
				SecurityAlgorithmSuite.Default,
				true, // IncludeTimestamp
				SecurityKeyEntropyMode.CombinedEntropy,
				MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature,
				MessageSecurityVersion.Default,
				false, // RequireSignatureConfirmation
				SecurityHeaderLayout.Strict,
				// EndpointSupportingTokenParameters: endorsing, signed, signedEncrypted, signedEndorsing (by count)
				0, 0, 0, 0,
				// ProtectionTokenParameters
				true, SecurityTokenInclusionMode.AlwaysToRecipient, SecurityTokenReferenceStyle.Internal, true,
				// LocalClientSettings
				true, 60, true,

				be, "");

			// test ProtectionTokenParameters
			SecureConversationSecurityTokenParameters tp =
				be.ProtectionTokenParameters as SecureConversationSecurityTokenParameters;
			Assert.IsNotNull (tp, "#2-1");

			SecurityAssert.AssertSecurityTokenParameters (
				SecurityTokenInclusionMode.AlwaysToRecipient,
				SecurityTokenReferenceStyle.Internal,
				true, tp, "Protection");
		}

		#endregion

		[Test]
		public void SetKeyDerivation ()
		{
			SetKeyDerivationCorrect (new TransportSecurityBindingElement (), "transport");
			SetKeyDerivationIncorrect (new TransportSecurityBindingElement (), "transport");
			SetKeyDerivationCorrect (new SymmetricSecurityBindingElement (), "symmetric");
			SetKeyDerivationIncorrect (new SymmetricSecurityBindingElement (), "symmetric");
			SetKeyDerivationCorrect (new AsymmetricSecurityBindingElement (), "asymmetric");
			SetKeyDerivationIncorrect (new AsymmetricSecurityBindingElement (), "asymmetric");
		}

		void SetKeyDerivationCorrect (SecurityBindingElement be, string label)
		{
			X509SecurityTokenParameters p, p2;
			p = new X509SecurityTokenParameters ();
			p2 = new X509SecurityTokenParameters ();
			Assert.AreEqual (true, p.RequireDerivedKeys, label + "#1");
			Assert.AreEqual (true, p2.RequireDerivedKeys, label + "#2");
			be.EndpointSupportingTokenParameters.Endorsing.Add (p);
			be.EndpointSupportingTokenParameters.Endorsing.Add (p2);
			be.SetKeyDerivation (false);
			Assert.AreEqual (false, p.RequireDerivedKeys, label + "#3");
			Assert.AreEqual (false, p2.RequireDerivedKeys, label + "#4");
		}

		void SetKeyDerivationIncorrect (SecurityBindingElement be, string label)
		{
			X509SecurityTokenParameters p, p2;
			p = new X509SecurityTokenParameters ();
			p2 = new X509SecurityTokenParameters ();
			// setting in prior - makes no sense
			be.SetKeyDerivation (false);
			be.EndpointSupportingTokenParameters.Endorsing.Add (p);
			be.EndpointSupportingTokenParameters.Endorsing.Add (p2);
			Assert.AreEqual (true, p.RequireDerivedKeys, label + "#5");
			Assert.AreEqual (true, p2.RequireDerivedKeys, label + "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotWorking")]
		public void CheckDuplicateAuthenticatorTypesClient ()
		{
			SymmetricSecurityBindingElement be =
				new SymmetricSecurityBindingElement ();
			be.ProtectionTokenParameters =
				new X509SecurityTokenParameters ();
			be.EndpointSupportingTokenParameters.Endorsing.Add (
				new X509SecurityTokenParameters ());
			// This causes multiple supporting token authenticator
			// of the same type.
			be.OptionalEndpointSupportingTokenParameters.Endorsing.Add (
				new X509SecurityTokenParameters ());
			Binding b = new CustomBinding (be, new HttpTransportBindingElement ());
			ClientCredentials cred = new ClientCredentials ();
			cred.ClientCertificate.Certificate =
				new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");
			IChannelFactory<IReplyChannel> ch = b.BuildChannelFactory<IReplyChannel> (new Uri ("http://localhost:" + NetworkHelpers.FindFreePort ()), cred);
			try {
				ch.Open ();
			} finally {
				if (ch.State == CommunicationState.Closed)
					ch.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotWorking")]
		public void CheckDuplicateAuthenticatorTypesService ()
		{
			SymmetricSecurityBindingElement be =
				new SymmetricSecurityBindingElement ();
			be.ProtectionTokenParameters =
				new X509SecurityTokenParameters ();
			be.EndpointSupportingTokenParameters.Endorsing.Add (
				new X509SecurityTokenParameters ());
			// This causes multiple supporting token authenticator
			// of the same type.
			be.OptionalEndpointSupportingTokenParameters.Endorsing.Add (
				new X509SecurityTokenParameters ());
			Binding b = new CustomBinding (be, new HttpTransportBindingElement ());
			ServiceCredentials cred = new ServiceCredentials ();
			cred.ServiceCertificate.Certificate =
				new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");
			IChannelListener<IReplyChannel> ch = b.BuildChannelListener<IReplyChannel> (new Uri ("http://localhost:" + NetworkHelpers.FindFreePort ()), cred);
			try {
				ch.Open ();
			} finally {
				if (ch.State == CommunicationState.Closed)
					ch.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void NonEndorsibleParameterInEndorsingSupport ()
		{
			SymmetricSecurityBindingElement be =
				new SymmetricSecurityBindingElement ();
			be.ProtectionTokenParameters =
				new X509SecurityTokenParameters ();
			be.EndpointSupportingTokenParameters.Endorsing.Add (
				new UserNameSecurityTokenParameters ());
			Binding b = new CustomBinding (be, new HttpTransportBindingElement ());
			X509Certificate2 cert = new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");
			EndpointAddress ea = new EndpointAddress (new Uri ("http://localhost:" + NetworkHelpers.FindFreePort ()), new X509CertificateEndpointIdentity (cert));
			CalcProxy client = new CalcProxy (b, ea);
			client.ClientCredentials.UserName.UserName = "rupert";
			client.Sum (1, 2);
		}

		void AssertSecurityCapabilities (
			ProtectionLevel request, ProtectionLevel response,
			bool supportsClientAuth, bool supportsClientWinId,
			bool supportsServerAuth, ISecurityCapabilities c,
			string label)
		{
			Assert.AreEqual (request, c.SupportedRequestProtectionLevel, label + ".request");
			Assert.AreEqual (response, c.SupportedResponseProtectionLevel, label + ".response");
			Assert.AreEqual (supportsClientAuth, c.SupportsClientAuthentication, label + ".client-auth");
			Assert.AreEqual (supportsClientWinId, c.SupportsClientWindowsIdentity, label + ".client-identity");
			Assert.AreEqual (supportsServerAuth, c.SupportsServerAuthentication, label + ".server-auth");
		}

		ISecurityCapabilities GetSecurityCapabilities (SecurityBindingElement be)
		{
			BindingContext bc = new BindingContext (
				new CustomBinding (),
				new BindingParameterCollection ());
			return be.GetProperty<ISecurityCapabilities> (bc);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetPropertyNullBindingContext1 ()
		{
			new SymmetricSecurityBindingElement ()
				.GetProperty<ISecurityCapabilities> (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetPropertyNullBindingContext2 ()
		{
			new AsymmetricSecurityBindingElement ()
				.GetProperty<ISecurityCapabilities> (null);
		}

		[Test]
		public void GetPropertySecurityCapabilities ()
		{
			ISecurityCapabilities c;
			RsaSecurityTokenParameters rsa =
				new RsaSecurityTokenParameters ();
			UserNameSecurityTokenParameters user =
				new UserNameSecurityTokenParameters ();
			X509SecurityTokenParameters x509 =
				new X509SecurityTokenParameters ();
			SecureConversationSecurityTokenParameters sc1 =
				new SecureConversationSecurityTokenParameters ();
			sc1.BootstrapSecurityBindingElement =
				new SymmetricSecurityBindingElement (); // empty
			SecureConversationSecurityTokenParameters sc2 =
				new SecureConversationSecurityTokenParameters ();
			sc2.BootstrapSecurityBindingElement =
				new SymmetricSecurityBindingElement (x509);
			SecureConversationSecurityTokenParameters sc3 =
				new SecureConversationSecurityTokenParameters ();
			sc3.BootstrapSecurityBindingElement =
				new AsymmetricSecurityBindingElement (null, x509);
			SecureConversationSecurityTokenParameters sc4 =
				new SecureConversationSecurityTokenParameters ();
			sc4.BootstrapSecurityBindingElement =
				new AsymmetricSecurityBindingElement (x509, null);

			// no parameters
			c = GetSecurityCapabilities (
				new SymmetricSecurityBindingElement ());
			AssertSecurityCapabilities (
				ProtectionLevel.EncryptAndSign,
				ProtectionLevel.EncryptAndSign,
				false, false, false, c, "#1");

			// x509 parameters for both
			c = GetSecurityCapabilities (
				new SymmetricSecurityBindingElement (x509));
			AssertSecurityCapabilities (
				ProtectionLevel.EncryptAndSign,
				ProtectionLevel.EncryptAndSign,
				true, true, true, c, "#2");

			// no initiator parameters
			c = GetSecurityCapabilities (
				new AsymmetricSecurityBindingElement (x509, null));
			AssertSecurityCapabilities (
				ProtectionLevel.EncryptAndSign,
				ProtectionLevel.EncryptAndSign,
				false, false, true, c, "#3");

			// no recipient parameters
			c = GetSecurityCapabilities (
				new AsymmetricSecurityBindingElement (null, x509));
			AssertSecurityCapabilities (
				ProtectionLevel.EncryptAndSign,
				ProtectionLevel.EncryptAndSign,
				true, true, false, c, "#4");

			// initiator does not support identity
			c = GetSecurityCapabilities (
				new AsymmetricSecurityBindingElement (x509, rsa));
			AssertSecurityCapabilities (
				ProtectionLevel.EncryptAndSign,
				ProtectionLevel.EncryptAndSign,
				true, false, true, c, "#5");

			// recipient does not support server auth
			c = GetSecurityCapabilities (
				new AsymmetricSecurityBindingElement (user, x509));
			AssertSecurityCapabilities (
				ProtectionLevel.EncryptAndSign,
				ProtectionLevel.EncryptAndSign,
				true, true, false, c, "#6");

			// secureconv with no symm. bootstrap params
			c = GetSecurityCapabilities (
				new SymmetricSecurityBindingElement (sc1));
			AssertSecurityCapabilities (
				ProtectionLevel.EncryptAndSign,
				ProtectionLevel.EncryptAndSign,
				false, false, false, c, "#7");

			// secureconv with x509 symm. bootstrap params
			c = GetSecurityCapabilities (
				new SymmetricSecurityBindingElement (sc2));
			AssertSecurityCapabilities (
				ProtectionLevel.EncryptAndSign,
				ProtectionLevel.EncryptAndSign,
				true, true, true, c, "#8");

			// secureconv with x509 initiator bootstrap params
			c = GetSecurityCapabilities (
				new SymmetricSecurityBindingElement (sc3));
			AssertSecurityCapabilities (
				ProtectionLevel.EncryptAndSign,
				ProtectionLevel.EncryptAndSign,
				true, true, false, c, "#9");

			// secureconv with x509 recipient bootstrap params
			c = GetSecurityCapabilities (
				new SymmetricSecurityBindingElement (sc4));
			AssertSecurityCapabilities (
				ProtectionLevel.EncryptAndSign,
				ProtectionLevel.EncryptAndSign,
				false, false, true, c, "#10");

			// FIXME: find out such cases that returns other ProtectionLevel values.
		}
	}
}
#endif
