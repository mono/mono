//
// ClientCredentialsSecurityTokenManagerTest.cs
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
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;
using NUnit.Framework;

using MonoTests.System.ServiceModel.Channels;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class ClientCredentialsSecurityTokenManagerTest
	{
		MyManager def_c;
		X509Certificate2 cert = new X509Certificate2 ("Test/Resources/test.pfx", "mono");
		X509Certificate2 certpub = new X509Certificate2 ("Test/Resources/test.cer");

		[SetUp]
		public void Initialize ()
		{
			def_c = new MyManager (new MyClientCredentials ());
		}

		[Test]
		public void IsIssuedSecurityTokenRequirement ()
		{
			ServiceModelSecurityTokenRequirement r;
			MyManager mgr = new MyManager (new MyClientCredentials ());

			r = new InitiatorServiceModelSecurityTokenRequirement ();
			MySslSecurityTokenParameters ssl =
				new MySslSecurityTokenParameters ();
			ssl.InitRequirement (r);
			Assert.IsFalse (mgr.IsIssued (r), "ssl");

			r = new InitiatorServiceModelSecurityTokenRequirement ();
			MySspiSecurityTokenParameters sspi =
				new MySspiSecurityTokenParameters ();
			sspi.InitRequirement (r);
			Assert.IsFalse (mgr.IsIssued (r), "sspi");

			r = new InitiatorServiceModelSecurityTokenRequirement ();
			MyIssuedSecurityTokenParameters issued =
				new MyIssuedSecurityTokenParameters ();
			issued.InitRequirement (r);
			Assert.IsTrue (mgr.IsIssued (r), "issued");

//			r = new InitiatorServiceModelSecurityTokenRequirement ();
//			MySecureConversationSecurityTokenParameters sc =
//				new MySecureConversationSecurityTokenParameters ();
//			sc.InitRequirement (r);
//			Assert.IsFalse (mgr.IsIssued (r), "sc");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateProviderDefault ()
		{
			SecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateProviderUserNameWithoutName ()
		{
			SecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.UserName;
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		public void CreateProviderUserName ()
		{
			SecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.UserName;
			def_c.ClientCredentials.UserName.UserName = "mono";
			UserNameSecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r)
				as UserNameSecurityTokenProvider;
			Assert.IsNotNull (p, "#1");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateProviderRsaDefault ()
		{
			// actually is Rsa usable here??

			SecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.Rsa;
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateProviderX509WithoutCert ()
		{
			SecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.X509Certificate;
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateProviderX509WithX509EndpointIdentity ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.X509Certificate;
			// X509CertificateEndpointIdentity does not work like
			// a client certificate; it still requires client cert
			r.TargetAddress = new EndpointAddress (
				new Uri ("http://localhost:8080"),
				new X509CertificateEndpointIdentity (cert));
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		public void CreateProviderX509WithX509IdentityKeyExchange ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.X509Certificate;
			// ... however when it is KeyExchange mode, this
			// endpoint identity is used.
			r.KeyUsage = SecurityKeyUsage.Exchange;
			r.TargetAddress = new EndpointAddress (
				new Uri ("http://localhost:8080"),
				new X509CertificateEndpointIdentity (cert));
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateProviderX509WithClientCertKeyExchange ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.X509Certificate;
			// ... and in such case ClientCertificate makes no sense.
			r.KeyUsage = SecurityKeyUsage.Exchange;
			def_c.ClientCredentials.ClientCertificate.Certificate = cert;
			r.TargetAddress = new EndpointAddress ("http://localhost:8080");
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		public void CreateProviderX509 ()
		{
			SecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.X509Certificate;
			def_c.ClientCredentials.ClientCertificate.Certificate = cert;
			X509SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r)
				as X509SecurityTokenProvider;
			Assert.IsNotNull (p, "#1");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateProviderX509RecipientNoKeyUsage ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.X509Certificate;
			def_c.ClientCredentials.ClientCertificate.Certificate = cert;

			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		public void CreateProviderX509Recipient ()
		{
			RecipientServiceModelSecurityTokenRequirement r =
				new RecipientServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.X509Certificate;
			r.KeyUsage = SecurityKeyUsage.Exchange;
			def_c.ClientCredentials.ClientCertificate.Certificate = cert;

			X509SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r)
				as X509SecurityTokenProvider;
			Assert.IsNotNull (p, "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateProviderAnonSslNoTargetAddress ()
		{
			SecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
//			r.TokenType = ServiceModelSecurityTokenTypes.AnonymousSslnego;
			new MySslSecurityTokenParameters ().InitRequirement (r);
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateProviderAnonSslNoBindingElement ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = ServiceModelSecurityTokenTypes.AnonymousSslnego;
			r.TargetAddress = new EndpointAddress ("http://localhost:8080");
			SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r);
			Assert.IsNotNull (p, "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateProviderAnonSslNoIssuerBindingContext ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
//			r.TokenType = ServiceModelSecurityTokenTypes.AnonymousSslnego;
			new MySslSecurityTokenParameters ().InitRequirement (r);
			r.TargetAddress = new EndpointAddress ("http://localhost:8080");
			r.SecurityBindingElement = new SymmetricSecurityBindingElement ();
			SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r);
			Assert.IsNotNull (p, "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateProviderAnonSslNoMessageSecurityVersion ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
//			r.TokenType = ServiceModelSecurityTokenTypes.AnonymousSslnego;
			new MySslSecurityTokenParameters ().InitRequirement (r);
			r.TargetAddress = new EndpointAddress ("http://localhost:8080");
			r.SecurityBindingElement = new SymmetricSecurityBindingElement ();
			r.Properties [ReqType.IssuerBindingContextProperty] =
				new BindingContext (new CustomBinding (), new BindingParameterCollection ());
			SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r);
			Assert.IsNotNull (p, "#1");
		}

		EndpointAddress CreateEndpointAddress (string s, bool publicOnly)
		{
			return new EndpointAddress (new Uri (s),
				new X509CertificateEndpointIdentity (publicOnly ? certpub : cert));
		}

		InitiatorServiceModelSecurityTokenRequirement  GetAnonSslProviderRequirement (bool useTransport)
		{
			return GetSslProviderRequirement (useTransport, false);
		}

		InitiatorServiceModelSecurityTokenRequirement  GetMutualSslProviderRequirement (bool useTransport)
		{
			return GetSslProviderRequirement (useTransport, true);
		}
		
		InitiatorServiceModelSecurityTokenRequirement  GetSslProviderRequirement (bool useTransport, bool mutual)
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			new MySslSecurityTokenParameters (mutual).InitRequirement (r);
//			r.TokenType = ServiceModelSecurityTokenTypes.AnonymousSslnego;
			r.TargetAddress = new EndpointAddress ("http://localhost:8080");
//			r.TargetAddress = CreateEndpointAddress ("http://localhost:8080", true);
			r.SecurityBindingElement = SecurityBindingElement.CreateUserNameForSslBindingElement ();
			CustomBinding binding =
				useTransport ?
				new CustomBinding (new HandlerTransportBindingElement (null)) :
				new CustomBinding ();
			r.Properties [ReqType.IssuerBindingContextProperty] =
				new BindingContext (binding, new BindingParameterCollection ());
			r.MessageSecurityVersion =
				MessageSecurityVersion.Default.SecurityTokenVersion;
			r.SecurityAlgorithmSuite =
				SecurityAlgorithmSuite.Default;

Assert.IsFalse (new MyManager (new MyClientCredentials ()).IsIssued (r), "premise");
			return r;
		}

		[Test]
		public void CreateProviderAnonSsl ()
		{
			CreateProviderAnonSsl (false);
			CreateProviderAnonSsl (true);
		}

		public void CreateProviderAnonSsl (bool useTransport)
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				GetAnonSslProviderRequirement (useTransport);
			SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r);
			Assert.IsNotNull (p, "#1");

			ICommunicationObject comm = p as ICommunicationObject;
			Assert.IsNotNull (comm, "#2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void GetAnonSslProviderSecurityTokenNoTransport ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				GetAnonSslProviderRequirement (false);
			SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r);
			Assert.IsNotNull (p, "#1");

			ICommunicationObject comm = p as ICommunicationObject;
			Assert.IsNotNull (comm, "#2");
			comm.Open ();
			try {
				p.GetToken (TimeSpan.FromSeconds (5));
			} finally {
				comm.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetAnonSslProviderSecurityTokenNoAlgorithmSuite ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				GetAnonSslProviderRequirement (true);
			r.Properties.Remove (ReqType.SecurityAlgorithmSuiteProperty);
			SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r);
			Assert.IsNotNull (p, "#1");

			ICommunicationObject comm = p as ICommunicationObject;
			Assert.IsNotNull (comm, "#2");
			comm.Open ();
			try {
				p.GetToken (TimeSpan.FromSeconds (5));
			} finally {
				comm.Close ();
			}
		}

		[Test]
		[Ignore ("it somehow causes NRE - smells .NET bug.")]
		public void GetAnonSslProviderSecurityToken ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				GetAnonSslProviderRequirement (true);

			// What causes NRE!?
			def_c.ClientCredentials.ClientCertificate.Certificate = certpub;
			r.Properties [ReqType.IssuedSecurityTokenParametersProperty] =
				new X509SecurityTokenParameters ();
			r.TargetAddress = CreateEndpointAddress ("http://localhost:8080", true);
			r.IssuerAddress = CreateEndpointAddress ("http://localhost:8080", true);
			r.IssuerBinding = new CustomBinding (new HandlerTransportBindingElement (null));
			def_c.ClientCredentials.ServiceCertificate.DefaultCertificate = certpub;

			SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r);
			Assert.IsNotNull (p, "#1");

			ICommunicationObject comm = p as ICommunicationObject;
			Assert.IsNotNull (comm, "#2");
			comm.Open ();
			try {
				p.GetToken (TimeSpan.FromSeconds (5));
			} finally {
				comm.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateProviderMutualSslWithoutClientCert ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				GetMutualSslProviderRequirement (true);
			SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r);
			Assert.IsNotNull (p, "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateProviderSecureConvNoTargetAddress ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				CreateRequirement ();
			r.Properties.Remove (ReqType.TargetAddressProperty);
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateProviderSecureConvNoSecurityBindingElement ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				CreateRequirement ();
			r.Properties.Remove (ReqType.SecurityBindingElementProperty);
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateProviderSecureConvNoIssuerBindingContext ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				CreateRequirement ();
			r.Properties.Remove (ReqType.IssuerBindingContextProperty);
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateProviderSecureConvNoKeySize ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = ServiceModelSecurityTokenTypes.SecureConversation;
			r.TargetAddress = new EndpointAddress ("http://localhost:8080");
			r.SecurityBindingElement =
				new SymmetricSecurityBindingElement ();
			r.Properties [ReqType.IssuerBindingContextProperty] =
				new BindingContext (new CustomBinding (), new BindingParameterCollection ());
/* it somehow does not cause an error ...
			InitiatorServiceModelSecurityTokenRequirement r =
				CreateRequirement ();
			r.Properties.Remove (SecurityTokenRequirement.KeySizeProperty);
*/
			def_c.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateProviderSecureConvNoMessageSecurityVersion ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				CreateRequirement ();
			r.Properties.Remove (ReqType.MessageSecurityVersionProperty);
			def_c.CreateSecurityTokenProvider (r);
		}

		InitiatorServiceModelSecurityTokenRequirement CreateRequirement ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = ServiceModelSecurityTokenTypes.SecureConversation;
			r.TargetAddress = new EndpointAddress ("http://localhost:8080");
			r.SecurityBindingElement =
				new SymmetricSecurityBindingElement ();

			// Without it, mysterious "The key length (blabla) 
			// is not a multiple of 8 for symmetric keys." occurs.
			r.SecureConversationSecurityBindingElement =
				new SymmetricSecurityBindingElement ();

			r.MessageSecurityVersion = MessageSecurityVersion.Default.SecurityTokenVersion;
			r.Properties [ReqType.IssuerBindingContextProperty] =
				new BindingContext (new CustomBinding (new HttpTransportBindingElement ()), new BindingParameterCollection ());
			r.KeySize = 256;
			return r;
		}

		[Test]
		public void CreateProviderSecureConv ()
		{
			SecurityTokenRequirement r = CreateRequirement ();
			SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r);
			Assert.IsNotNull (p, "#1");
			// non-standard provider, it looks similar to IssuedSecurityTokenProvider.
		}

		[Test]
		[Ignore ("it ends up to require running service on .NET, and it's anyways too implementation dependent.")]
		public void SecureConvProviderGetToken ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				CreateRequirement ();
			// it still requires SecurityAlgorithmSuite on GetToken().
			r.SecurityAlgorithmSuite = SecurityAlgorithmSuite.Default;
			// the actual security binding element requires
			// ProtectionTokenParameters.
			r.SecureConversationSecurityBindingElement =
				SecurityBindingElement.CreateAnonymousForCertificateBindingElement ();
			// the above requires service certificate
			BindingContext ctx = r.GetProperty<BindingContext> (ReqType.IssuerBindingContextProperty);
			ClientCredentials cred = new ClientCredentials ();
			cred.ServiceCertificate.DefaultCertificate = cert;
			ctx.BindingParameters.Add (cred);

			// without it, identity check fails on IssuerAddress
			// (TargetAddress is used when IssuerAddress is not set)
			r.TargetAddress = new EndpointAddress (new Uri ("http://localhost:8080"), new X509CertificateEndpointIdentity (cert));

			SecurityTokenProvider p =
				def_c.CreateSecurityTokenProvider (r);
			Assert.IsNotNull (p, "#1");
			// non-standard provider, it looks similar to IssuedSecurityTokenProvider.
			((ICommunicationObject) p).Open ();
			p.GetToken (TimeSpan.FromSeconds (5));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotWorking")]
		public void SecureConvProviderOnlyWithIssuedParameters ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = ServiceModelSecurityTokenTypes.SecureConversation;
			IssuedSecurityTokenParameters ip =
				new IssuedSecurityTokenParameters ();
			ip.IssuerAddress = new EndpointAddress ("http://localhost:8080");
			ip.IssuerBinding = new WSHttpBinding ();

			r.Properties [ReqType.IssuedSecurityTokenParametersProperty] = ip;

			def_c.CreateSecurityTokenProvider (r);
		}

		// CreateSecurityTokenAuthenticator

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateAuthenticatorUserName ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.UserName;
			def_c.ClientCredentials.UserName.UserName = "mono";
			SecurityTokenResolver resolver;
			def_c.CreateSecurityTokenAuthenticator (r, out resolver);
		}

		[Test]
		public void CreateAuthenticatorRsa ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.Rsa;
			SecurityTokenResolver resolver;
			RsaSecurityTokenAuthenticator rsa =
				def_c.CreateSecurityTokenAuthenticator (r, out resolver)
				as RsaSecurityTokenAuthenticator;
			Assert.IsNotNull (rsa, "#1");
			Assert.IsNull (resolver, "#2"); // but probably non-null if there is RsaSecurityToken that could be somehow created from the credential.
		}

		[Test]
		public void CreateAuthenticatorX509 ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			r.TokenType = SecurityTokenTypes.X509Certificate;
			def_c.ClientCredentials.ClientCertificate.Certificate = cert;
			SecurityTokenResolver resolver;
			SecurityTokenAuthenticator x509 =
				def_c.CreateSecurityTokenAuthenticator (r, out resolver);
			Assert.IsNotNull (x509, "#1");
			Assert.IsNull (resolver, "#2"); // hmm...

			def_c.ClientCredentials.ServiceCertificate.DefaultCertificate = cert;
			x509 = def_c.CreateSecurityTokenAuthenticator (r, out resolver);
			Assert.IsNotNull (x509, "#3");
			Assert.IsNull (resolver, "#4"); // hmmm...
		}

		[Test]
		[Ignore ("this test is not fully written yet.")]
		public void OtherParameterInEndorsingSupport ()
		{
			SymmetricSecurityBindingElement be =
				new SymmetricSecurityBindingElement ();
			be.ProtectionTokenParameters =
				new X509SecurityTokenParameters ();
			be.EndpointSupportingTokenParameters.Endorsing.Add (
				new MyEndorsingTokenParameters ());
			Binding b = new CustomBinding (be, new HttpTransportBindingElement ());
			EndpointAddress ea = new EndpointAddress (new Uri ("http://localhost:37564"), new X509CertificateEndpointIdentity (cert));
			CalcProxy client = new CalcProxy (b, ea);
			client.Endpoint.Behaviors.RemoveAll<ClientCredentials> ();
			client.Endpoint.Behaviors.Add (new MyClientCredentials ());
			client.Sum (1, 2);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void MissingCloneCore ()
		{
			new MyClientCredentials2 ().Clone ();
		}

		[Test]
		public void AnonSslIsIssued ()
		{
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			new MySslSecurityTokenParameters ().InitRequirement (r);
			Assert.IsFalse (new MyManager (new MyClientCredentials ()).IsIssued (r), "#1");
		}
	}

	class MyClientCredentials : ClientCredentials
	{
		public override SecurityTokenManager CreateSecurityTokenManager ()
		{
			return new MyManager (this);
		}

		protected override ClientCredentials CloneCore ()
		{
			return new MyClientCredentials ();
		}
	}

	class MyClientCredentials2 : ClientCredentials
	{
		public override SecurityTokenManager CreateSecurityTokenManager ()
		{
			return null;
		}
	}

	class MyManager : ClientCredentialsSecurityTokenManager
	{
		public MyManager (MyClientCredentials cred)
			: base (cred)
		{
		}

		public bool IsIssued (SecurityTokenRequirement r)
		{
			return IsIssuedSecurityTokenRequirement (r);
		}

		public override SecurityTokenProvider CreateSecurityTokenProvider (SecurityTokenRequirement tokenRequirement
)
		{
			if (tokenRequirement.TokenType == "urn:my")
				return new MySecurityTokenProvider ();
			return base.CreateSecurityTokenProvider (tokenRequirement);
		}

		public override SecurityTokenSerializer CreateSecurityTokenSerializer (SecurityTokenVersion ver)
		{
			return new MySecurityTokenSerializer ();
		}
	}

	class MySslSecurityTokenParameters : SslSecurityTokenParameters
	{
		public MySslSecurityTokenParameters ()
		{
		}

		public MySslSecurityTokenParameters (bool mutual)
			: base (mutual)
		{
		}

		public void InitRequirement (SecurityTokenRequirement r)
		{
			InitializeSecurityTokenRequirement (r);
		}
	}

	class MySspiSecurityTokenParameters : SspiSecurityTokenParameters
	{
		public void InitRequirement (SecurityTokenRequirement r)
		{
			InitializeSecurityTokenRequirement (r);
		}
	}

	class MyIssuedSecurityTokenParameters : IssuedSecurityTokenParameters
	{
		public void InitRequirement (SecurityTokenRequirement r)
		{
			InitializeSecurityTokenRequirement (r);
		}
	}

	class MySecureConversationSecurityTokenParameters : SecureConversationSecurityTokenParameters
	{
		public MySecureConversationSecurityTokenParameters ()
		{
		}

		public MySecureConversationSecurityTokenParameters (SecureConversationSecurityTokenParameters clone)
			: base (clone)
		{
		}

		public MySecureConversationSecurityTokenParameters (SecurityBindingElement element)
			: base (element)
		{
		}

		public MySecureConversationSecurityTokenParameters (SecurityBindingElement element, bool requireCancel, ChannelProtectionRequirements cpr)
			: base (element, requireCancel, cpr)
		{
		}

		public void InitRequirement (SecurityTokenRequirement r)
		{
			InitializeSecurityTokenRequirement (r);
		}
	}

	class MySecurityTokenProvider : SecurityTokenProvider
	{
		public MySecurityTokenProvider ()
		{
		}

		protected override SecurityToken GetTokenCore (TimeSpan timeout)
		{
			return new RsaSecurityToken (RSA.Create ());
		}
	}

	class MySecurityTokenSerializer : WSSecurityTokenSerializer
	{
		protected override void WriteTokenCore (XmlWriter w, SecurityToken token)
		{
			RsaSecurityToken r = token as RsaSecurityToken;
			w.Flush ();
			if (r != null)
				w.WriteRaw (r.Rsa.ToXmlString (false));
			else
				base.WriteTokenCore (w, token);
		}
	}

	class MyEndorsingTokenParameters : SecurityTokenParameters
	{
		public MyEndorsingTokenParameters ()
		{
		}

		protected MyEndorsingTokenParameters (MyEndorsingTokenParameters source)
		{
		}

		protected override bool HasAsymmetricKey {
			get { return true; }
		}

		protected override bool SupportsClientAuthentication {
			get { return true; }
		}

		protected override bool SupportsClientWindowsIdentity {
			get { return false; }
		}

		protected override bool SupportsServerAuthentication {
			get { return true; }
		}

		protected override SecurityTokenParameters CloneCore ()
		{
			return new MyEndorsingTokenParameters (this);
		}

		protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause (
			SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
		{
			RsaSecurityToken r = token as RsaSecurityToken;
			return r.CreateKeyIdentifierClause <RsaKeyIdentifierClause> ();
		}

		protected override void InitializeSecurityTokenRequirement (SecurityTokenRequirement requirement)
		{
			// If there were another token type that supports protection
			// and does not require X509, it should be used instead ...
			requirement.TokenType = "urn:my";
		}
	}
}
