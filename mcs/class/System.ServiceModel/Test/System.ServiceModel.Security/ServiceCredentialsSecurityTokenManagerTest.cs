//
// ServiceCredentialsSecurityTokenManagerTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc.  http://www.novell.com
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
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;
using NUnit.Framework;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace MonoTests.System.ServiceModel.Security
{
	[TestFixture]
	public class ServiceCredentialsSecurityTokenManagerTest
	{
		class MyManager : ServiceCredentialsSecurityTokenManager
		{
			public MyManager ()
				: this (new ServiceCredentials ())
			{
			}

			public MyManager (ServiceCredentials cred)
				: base (cred)
			{
			}

			public bool IsIssued (SecurityTokenRequirement r)
			{
				return IsIssuedSecurityTokenRequirement (r);
			}
		}

		class MySslSecurityTokenParameters : SslSecurityTokenParameters
		{
			public void InitRequirement (SecurityTokenRequirement r)
			{
				InitializeSecurityTokenRequirement (r);
			}
		}

		MyManager def_c;

		[SetUp]
		public void Initialize ()
		{
			def_c = new MyManager ();
		}

		[Test]
		public void DefaultValues ()
		{
			// FIXME: check more
			MyManager mgr = new MyManager ();
			Assert.IsTrue (mgr.ServiceCredentials.SecureConversationAuthentication.SecurityStateEncoder is DataProtectionSecurityStateEncoder, "#n-1");
		}

		[Test]
		public void IsIssuedSecurityTokenRequirement ()
		{
			RecipientServiceModelSecurityTokenRequirement r;
			MyManager mgr = new MyManager ();

			r = new RecipientServiceModelSecurityTokenRequirement ();
			MySslSecurityTokenParameters ssl =
				new MySslSecurityTokenParameters ();
			ssl.InitRequirement (r);
			Assert.IsFalse (mgr.IsIssued (r), "ssl");

			r = new RecipientServiceModelSecurityTokenRequirement ();
			MySspiSecurityTokenParameters sspi =
				new MySspiSecurityTokenParameters ();
			sspi.InitRequirement (r);
			Assert.IsFalse (mgr.IsIssued (r), "sspi");

			r = new RecipientServiceModelSecurityTokenRequirement ();
			MyIssuedSecurityTokenParameters issued =
				new MyIssuedSecurityTokenParameters ();
			issued.InitRequirement (r);
			Assert.IsTrue (mgr.IsIssued (r), "issued");

/*
			r = new RecipientServiceModelSecurityTokenRequirement ();
			MySecureConversationSecurityTokenParameters sc =
				new MySecureConversationSecurityTokenParameters (
					new SymmetricSecurityBindingElement (new X509SecurityTokenParameters ()),
					false,
					new ChannelProtectionRequirements ());
			r.Properties [ReqType.IssuerBindingContextProperty] =
				new BindingContext (new CustomBinding (), new BindingParameterCollection ());
			r.Properties [ReqType.MessageSecurityVersionProperty] =
				MessageSecurityVersion.Default;
			r.Properties [ReqType.ChannelParametersCollectionProperty] =
				new ChannelParameterCollection ();
			r.Properties [ReqType.IssuedSecurityTokenParametersProperty] = sc.Clone ();
			r.Properties [ReqType.IssuerBindingProperty] =
				new CustomBinding (new HttpTransportBindingElement ());
			r.Properties [ReqType.MessageDirectionProperty] =
				MessageDirection.Input;
			r.SecureConversationSecurityBindingElement =
				new SymmetricSecurityBindingElement (
					new X509SecurityTokenParameters ());
			r.SecurityAlgorithmSuite = SecurityAlgorithmSuite.Default;
			r.Properties [ReqType.SupportSecurityContextCancellationProperty] = true;
			r.ListenUri = new Uri ("http://localhost:8080");
			r.KeySize = 256;
			sc.InitRequirement (r);
			Assert.IsFalse (mgr.IsIssued (r), "sc");
*/
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateProviderDefault ()
		{
			SecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Ignore ("")]
		public void CreateProviderUserNameWithoutName ()
		{
			SecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.UserName;
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateProviderUserName ()
		{
			SecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.UserName;
			def_c.CreateSecurityTokenProvider (r);
		}

		class MyUserNameValidator : UserNamePasswordValidator
		{
			public override void Validate (string userName, string password)
			{
				throw new Exception ();
			}
		}

		[Test]
		public void CreateAuthenticatorUserName ()
		{
			SecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.UserName;
			SecurityTokenResolver resolver;

			SecurityTokenAuthenticator a =
				def_c.CreateSecurityTokenAuthenticator (r, out resolver);
			Assert.AreEqual (typeof (WindowsUserNameSecurityTokenAuthenticator), a.GetType (), "#1");
			Assert.IsNull (resolver, "#2");

			def_c.ServiceCredentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
			def_c.ServiceCredentials.UserNameAuthentication.CustomUserNamePasswordValidator = new MyUserNameValidator ();
			a = def_c.CreateSecurityTokenAuthenticator (r, out resolver);
			Assert.AreEqual (typeof (CustomUserNameSecurityTokenAuthenticator), a.GetType (), "#3");
			Assert.IsNull (resolver, "#4");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateAuthenticatorUserNameCustomWithoutValidator ()
		{
			SecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.UserName;
			SecurityTokenResolver resolver;
			def_c.ServiceCredentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
			def_c.CreateSecurityTokenAuthenticator (r, out resolver);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateProviderRsaDefault ()
		{
			// actually is Rsa usable here??

			SecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.Rsa;
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		public void CreateAuthenticatorRsaDefault ()
		{
			SecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			SecurityTokenResolver resolver;
			r.TokenType = SecurityTokenTypes.Rsa;
			SecurityTokenAuthenticator a = def_c.CreateSecurityTokenAuthenticator (r, out resolver);
			Assert.AreEqual (typeof (RsaSecurityTokenAuthenticator), a.GetType (), "#1");
			Assert.IsNull (resolver, "#2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateProviderX509WithoutCert ()
		{
			SecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.X509Certificate;
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateProviderX509PublicOnlyKey ()
		{
			SecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.X509Certificate;
			X509Certificate2 cert = new X509Certificate2 ("Test/Resources/test.cer");
			def_c.ServiceCredentials.ServiceCertificate.Certificate = cert;
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		public void CreateProviderX509 ()
		{
			SecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.X509Certificate;
			def_c.ServiceCredentials.ServiceCertificate.Certificate =
				new X509Certificate2 ("Test/Resources/test.pfx", "mono");
			X509SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r)
				as X509SecurityTokenProvider;
			Assert.IsNotNull (p, "#1");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateProviderX509Initiator ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.X509Certificate;
			r.KeyUsage = SecurityKeyUsage.Exchange;
			// ClientCredential is somehow required ...
			def_c.ServiceCredentials.ServiceCertificate.Certificate =
				new X509Certificate2 ("Test/Resources/test.pfx", "mono");

			X509SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r)
				as X509SecurityTokenProvider;
			Assert.IsNotNull (p, "#1");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateProviderAnonSslError ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			r.TokenType = ServiceModelSecurityTokenTypes.AnonymousSslnego;
			r.ListenUri = new Uri ("http://localhost:8080");
			r.SecurityBindingElement = new SymmetricSecurityBindingElement ();
			r.Properties [ReqType.IssuerBindingContextProperty] =
				new BindingContext (new CustomBinding (), new BindingParameterCollection ());
			r.MessageSecurityVersion =
				MessageSecurityVersion.Default.SecurityTokenVersion;
			SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r);
			Assert.IsNotNull (p, "#1");
		}

		[Test]
		[Ignore ("incomplete")]
		[Category ("NotWorking")]
		public void CreateProviderAnonSsl ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			new MySslSecurityTokenParameters ().InitRequirement (r);

			Assert.IsFalse (r.Properties.ContainsKey (ReqType.ChannelParametersCollectionProperty), "#1");
			Assert.IsFalse (r.Properties.ContainsKey (ReqType.EndpointFilterTableProperty), "#2");
			Assert.IsFalse (r.Properties.ContainsKey (ReqType.HttpAuthenticationSchemeProperty), "#3");
			Assert.IsFalse (r.Properties.ContainsKey (ReqType.IsOutOfBandTokenProperty), "#4");
			Assert.IsFalse (r.Properties.ContainsKey (ReqType.IssuerAddressProperty), "#5");
			Assert.IsFalse (r.Properties.ContainsKey (ReqType.MessageDirectionProperty), "#6");
			Assert.IsFalse (r.Properties.ContainsKey (ReqType.MessageSecurityVersionProperty), "#7");
			//Assert.IsTrue (r.Properties.ContainsKey (SecurityTokenRequirement.PeerAuthenticationMode), "#8");
			Assert.IsFalse (r.Properties.ContainsKey (ReqType.SecurityAlgorithmSuiteProperty), "#9");
			Assert.IsFalse (r.Properties.ContainsKey (ReqType.SecurityBindingElementProperty), "#10");
			Assert.IsFalse (r.Properties.ContainsKey (ReqType.SupportingTokenAttachmentModeProperty), "#11");
			Assert.AreEqual (null, r.TransportScheme, "#12");

			r.TokenType = ServiceModelSecurityTokenTypes.AnonymousSslnego;
			r.ListenUri = new Uri ("http://localhost:8080");
			r.SecurityBindingElement = new SymmetricSecurityBindingElement ();
			r.Properties [ReqType.IssuerBindingContextProperty] =
				new BindingContext (new CustomBinding (), new BindingParameterCollection ());
			r.MessageSecurityVersion =
				MessageSecurityVersion.Default.SecurityTokenVersion;

			r.Properties [ReqType.SecurityAlgorithmSuiteProperty] =
				SecurityAlgorithmSuite.Default;
			r.TransportScheme = "https";

			r.Properties [ReqType.ChannelParametersCollectionProperty] = new ChannelParameterCollection ();
			r.Properties [ReqType.EndpointFilterTableProperty] = null;
			r.Properties [ReqType.HttpAuthenticationSchemeProperty] = AuthenticationSchemes.Anonymous;
			r.Properties [ReqType.IsOutOfBandTokenProperty] = true;
			r.Properties [ReqType.IssuerAddressProperty] = new EndpointAddress ("http://localhost:9090");
//			r.Properties [ReqType.MessageDirectionProperty] = MessageDirection.Input;
			r.Properties [ReqType.SecurityBindingElementProperty] = new SymmetricSecurityBindingElement ();
			r.Properties [ReqType.SupportingTokenAttachmentModeProperty] = SecurityTokenAttachmentMode.Signed;

			SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r);
			Assert.IsNotNull (p, "#1");
		}

		RecipientServiceModelSecurityTokenRequirement CreateAnonSslRequirement ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			MySslSecurityTokenParameters p = new MySslSecurityTokenParameters ();
			p.InitRequirement (r);
			r.SecurityBindingElement = new SymmetricSecurityBindingElement (new X509SecurityTokenParameters ());
			r.Properties [ReqType.IssuedSecurityTokenParametersProperty] = p.Clone ();
			r.Properties [ReqType.IssuerBindingContextProperty] =
				new BindingContext (new CustomBinding (new HttpTransportBindingElement ()), new BindingParameterCollection ());
			r.Properties [ReqType.MessageSecurityVersionProperty] =
				MessageSecurityVersion.Default.SecurityTokenVersion;
			return r;
		}

		RecipientServiceModelSecurityTokenRequirement CreateSecureConvRequirement ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				CreateRecipientRequirement (ServiceModelSecurityTokenTypes.SecureConversation);
			r.Properties [ReqType.IssuedSecurityTokenParametersProperty] = new SecureConversationSecurityTokenParameters (new SymmetricSecurityBindingElement (new X509SecurityTokenParameters ()));
			// without it, "The key length (...) is not a multiple of 8 for symmetric keys" occurs.
			r.SecureConversationSecurityBindingElement =
				new SymmetricSecurityBindingElement ();
			return r;
		}

		RecipientServiceModelSecurityTokenRequirement CreateRecipientRequirement (string tokenType)
		{
			RecipientServiceModelSecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			r.TokenType = tokenType;
			r.SecurityBindingElement = new SymmetricSecurityBindingElement ();
			r.Properties [ReqType.IssuerBindingContextProperty] =
				new BindingContext (new CustomBinding (), new BindingParameterCollection ());
			r.Properties [ReqType.IssuedSecurityTokenParametersProperty] = new IssuedSecurityTokenParameters ();
			r.MessageSecurityVersion =
				MessageSecurityVersion.Default.SecurityTokenVersion;
			return r;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateAuthenticatorAnonSslNoSecurityBindingElement ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				CreateAnonSslRequirement ();
			r.SecurityBindingElement = null;
			SecurityTokenResolver resolver;
			def_c.CreateSecurityTokenAuthenticator (r, out resolver);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateAuthenticatorAnonSslNoIssuedSecurityTokenParameters ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				CreateAnonSslRequirement ();
			r.Properties.Remove (ReqType.IssuedSecurityTokenParametersProperty);
			SecurityTokenResolver resolver;
			def_c.CreateSecurityTokenAuthenticator (r, out resolver);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateAuthenticatorAnonSslNoIssuerBindingContext ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				CreateAnonSslRequirement ();
			r.Properties.Remove (ReqType.IssuerBindingContextProperty);
			SecurityTokenResolver resolver;
			def_c.CreateSecurityTokenAuthenticator (r, out resolver);
		}

		[Test]
		// The type of exception should not matter though.
		[ExpectedException (typeof (NotSupportedException))]
		[Category ("NotWorking")]
		public void CreateAuthenticatorAnonSslNullMessageSecurityVersion ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				CreateAnonSslRequirement ();
			r.MessageSecurityVersion = null;
			SecurityTokenResolver resolver;
			def_c.CreateSecurityTokenAuthenticator (r, out resolver);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateAuthenticatorAnonSslNoMessageSecurityVersion ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				CreateAnonSslRequirement ();
			r.Properties.Remove (ReqType.MessageSecurityVersionProperty);
			SecurityTokenResolver resolver;
			def_c.CreateSecurityTokenAuthenticator (r, out resolver);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void CreateAuthenticatorAnonSslNoServiceCertificate ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				CreateAnonSslRequirement ();
			SecurityTokenResolver resolver;
			def_c.CreateSecurityTokenAuthenticator (r, out resolver);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateAuthenticatorAnonSslCertPublicOnly ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				CreateAnonSslRequirement ();
			SecurityTokenResolver resolver;
			def_c.ServiceCredentials.ServiceCertificate.Certificate =
				new X509Certificate2 ("Test/Resources/test.cer");
			def_c.CreateSecurityTokenAuthenticator (r, out resolver);
		}

		[Test]
		[Category ("NotWorking")]
		public void CreateAuthenticatorAnonSsl ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				CreateAnonSslRequirement ();
			SecurityTokenResolver resolver;
			X509Certificate2 cert = new X509Certificate2 ("Test/Resources/test.pfx", "mono");
			def_c.ServiceCredentials.ServiceCertificate.Certificate = cert;
			SecurityTokenAuthenticator a = def_c.CreateSecurityTokenAuthenticator (r, out resolver);
			// non-standard authenticator type.
			Assert.IsNotNull (resolver, "#1");
			Assert.IsTrue (a is IIssuanceSecurityTokenAuthenticator, "#2");

			try {
				a.ValidateToken (new X509SecurityToken (cert));
				Assert.Fail ("It cannot validate raw X509SecurityToken");
			} catch (SecurityTokenValidationException) {
			}
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateProviderSecureConv ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			r.TokenType = ServiceModelSecurityTokenTypes.SecureConversation;
			r.ListenUri = new Uri ("http://localhost:8080");
			r.MessageSecurityVersion = MessageSecurityVersion.Default.SecurityTokenVersion;
			r.KeySize = 256;
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateAuthenticatorSecureConvNoSecurityBindingElement ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				CreateSecureConvRequirement ();
			r.SecurityBindingElement = null;
			SecurityTokenResolver resolver;
			def_c.CreateSecurityTokenAuthenticator (r, out resolver);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateAuthenticatorSecureConvNoIssuedSecurityTokenParameters ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				CreateSecureConvRequirement ();
			r.Properties.Remove (ReqType.IssuedSecurityTokenParametersProperty);
			SecurityTokenResolver resolver;
			def_c.CreateSecurityTokenAuthenticator (r, out resolver);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateAuthenticatorSecureConvNoIssuerBindingContext ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				CreateSecureConvRequirement ();
			r.Properties.Remove (ReqType.IssuerBindingContextProperty);
			SecurityTokenResolver resolver;
			def_c.CreateSecurityTokenAuthenticator (r, out resolver);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateAuthenticatorSecureConvNoMessageSecurityVersion ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				CreateSecureConvRequirement ();
			r.Properties.Remove (ReqType.MessageSecurityVersionProperty);
			SecurityTokenResolver resolver;
			def_c.CreateSecurityTokenAuthenticator (r, out resolver);
		}

		[Test]
		public void CreateAuthenticatorSecureConv ()
		{
			// service certificate is not required
			RecipientServiceModelSecurityTokenRequirement r =
				CreateSecureConvRequirement ();
			SecurityTokenResolver resolver;
			//SecurityTokenAuthenticator a =
				def_c.CreateSecurityTokenAuthenticator (r, out resolver);
			Assert.IsNotNull (resolver, "#1");
		}
	}
}
