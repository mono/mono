//
// ClientCredentialsSecurityTokenManager.cs
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
using System.Net.Security;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace System.ServiceModel
{
	[MonoTODO]
	public class ClientCredentialsSecurityTokenManager : SecurityTokenManager
	{
		ClientCredentials credentials;

		public ClientCredentialsSecurityTokenManager (ClientCredentials credentials)
		{
			if (credentials == null)
				throw new ArgumentNullException ("credentials");
			this.credentials = credentials;
		}

		public ClientCredentials ClientCredentials {
			get { return credentials; }
		}

		[MonoTODO]
		public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator (
			SecurityTokenRequirement requirement,
			out SecurityTokenResolver outOfBandTokenResolver)
		{
			outOfBandTokenResolver = null;
			if (requirement == null)
				throw new ArgumentNullException ("requirement");
			if (requirement.TokenType == SecurityTokenTypes.UserName) {
				// unsupported
			}
			else if (requirement.TokenType == SecurityTokenTypes.Rsa)
				return new RsaSecurityTokenAuthenticator ();
			else if (requirement.TokenType == SecurityTokenTypes.X509Certificate)
				return CreateX509Authenticator (requirement);
			else if (requirement.TokenType == ServiceModelSecurityTokenTypes.Spnego)
				return new SspiClientSecurityTokenAuthenticator (this, requirement);
			else
				throw new NotImplementedException ("Security token type " + requirement.TokenType);

			throw new NotSupportedException (String.Format ("Security token requirement '{0}' is not supported to create SecurityTokenAuthenticator.", requirement));
		}


		X509SecurityTokenAuthenticator CreateX509Authenticator (SecurityTokenRequirement requirement)
		{
			X509CertificateRecipientClientCredential c = ClientCredentials.ServiceCertificate;
			switch (c.Authentication.CertificateValidationMode) {
			case X509CertificateValidationMode.Custom:
				if (c.Authentication.CustomCertificateValidator == null)
					throw new InvalidOperationException ("For Custom certificate validation mode, CustomCertificateValidator is required to create a token authenticator for X509 certificate.");
				return new X509SecurityTokenAuthenticator (c.Authentication.CustomCertificateValidator);
			case X509CertificateValidationMode.None:
				return new X509SecurityTokenAuthenticator (X509CertificateValidator.None);
			case X509CertificateValidationMode.PeerOrChainTrust:
				return new X509SecurityTokenAuthenticator (X509CertificateValidator.PeerOrChainTrust);
			case X509CertificateValidationMode.ChainTrust:
				return new X509SecurityTokenAuthenticator (X509CertificateValidator.ChainTrust);
			default:
				return new X509SecurityTokenAuthenticator (X509CertificateValidator.PeerTrust);
			}
		}

		#region CreateSecurityTokenProvider()

		[MonoTODO]
		public override SecurityTokenProvider CreateSecurityTokenProvider (SecurityTokenRequirement requirement)
		{
			if (IsIssuedSecurityTokenRequirement (requirement))
				return CreateIssuedTokenProvider (requirement);

			bool isInitiator;

			// huh, they are not constants but properties.
			if (requirement.TokenType == SecurityTokenTypes.X509Certificate)
				return CreateX509SecurityTokenProvider (requirement);
			else if (requirement.TokenType == ServiceModelSecurityTokenTypes.SecureConversation)
				return CreateSecureConversationProvider (requirement);
			else if (requirement.TokenType == ServiceModelSecurityTokenTypes.AnonymousSslnego) {
				if (requirement.TryGetProperty<bool> (ReqType.IsInitiatorProperty, out isInitiator) && isInitiator)
					return CreateSslnegoProvider (requirement);
			} else if (requirement.TokenType == ServiceModelSecurityTokenTypes.MutualSslnego) {
				if (requirement.TryGetProperty<bool> (ReqType.IsInitiatorProperty, out isInitiator) && isInitiator)
					return CreateSslnegoProvider (requirement);
			} else if (requirement.TokenType == ServiceModelSecurityTokenTypes.SecurityContext) {
				// FIXME: implement
			} else if (requirement.TokenType == ServiceModelSecurityTokenTypes.Spnego) {
				return CreateSpnegoProvider (requirement);
			} else if (requirement.TokenType == ServiceModelSecurityTokenTypes.SspiCredential) {
				// FIXME: implement
			} else if (requirement.TokenType == SecurityTokenTypes.Rsa) {
				// FIXME: implement
			} else if (requirement.TokenType == SecurityTokenTypes.Saml) {
				// FIXME: implement
			} else if (requirement.TokenType == SecurityTokenTypes.UserName)
				return CreateUserNameProvider (requirement);
			else if (requirement.TokenType == SecurityTokenTypes.Kerberos) {
				return CreateKerberosProvider (requirement);
			}
			throw new NotSupportedException (String.Format ("Token type '{0}' is not supported", requirement.TokenType));
		}

		UserNameSecurityTokenProvider CreateUserNameProvider (
SecurityTokenRequirement requirement)
		{
			UserNamePasswordClientCredential c =
				credentials.UserName;
			if (c.UserName == null)
				throw new InvalidOperationException ("User name is not specified in ClientCredentials.");
			UserNameSecurityTokenProvider p =
				new UserNameSecurityTokenProvider (c.UserName, c.Password);
			return p;
		}

		KerberosSecurityTokenProvider CreateKerberosProvider (SecurityTokenRequirement requirement)
		{
			// FIXME: how to get SPN?
			return new KerberosSecurityTokenProvider (
				"", credentials.Windows.AllowedImpersonationLevel, credentials.Windows.ClientCredential);
		}

		X509SecurityTokenProvider CreateX509SecurityTokenProvider (SecurityTokenRequirement requirement)
		{
			// - When the request is as an initiator, then
			//   - if the purpose is key exchange, then
			//     the initiator wants the service certificate
			//     to encrypt the message with its public key.
			//   - otherwise, the initiator wants the client
			//     certificate to sign the message with the
			//     private key.
			// - otherwise
			//   - if the purpose is key exchange, then
			//     the recipient wants the client certificate
			//     to encrypt the message with its public key.
			//   - otherwise, the recipient wants the service
			//     certificate to sign the message with the
			//     private key.
			bool isInitiator;
			if (!requirement.TryGetProperty<bool> (ReqType.IsInitiatorProperty, out isInitiator))
				isInitiator = false;
			X509Certificate2 cert;
			bool isClient;
			if (isInitiator)
				isClient = requirement.KeyUsage == SecurityKeyUsage.Signature;
			else {
				if (!requirement.Properties.ContainsKey (SecurityTokenRequirement.KeyUsageProperty))
					throw new NotSupportedException (String.Format ("Cannot create a security token provider from this requirement '{0}'", requirement));
				isClient = requirement.KeyUsage == SecurityKeyUsage.Exchange;
			}
			if (isClient)
				cert = credentials.ClientCertificate.Certificate;
			else
				cert = GetServiceCertificate (requirement);

			if (cert == null) {
				if (isClient)
					throw new InvalidOperationException ("Client certificate is not provided in ClientCredentials.");
				else
					throw new InvalidOperationException ("Service certificate is not provided.");
			}
			X509SecurityTokenProvider p =
				new X509SecurityTokenProvider (cert);
			return p;
		}

		X509Certificate2 GetServiceCertificate (SecurityTokenRequirement requirement)
		{
			// try X509CertificateEndpointIdentity,
			// ServiceCertificate.ScopedCertificate and
			// ServiceCertificate.DefaultCertificate.

			X509Certificate2 cert = null;
			EndpointAddress address = null;
			requirement.TryGetProperty (ReqType.TargetAddressProperty, out address);

			if (address != null) {
				X509CertificateEndpointIdentity ident = address.Identity as X509CertificateEndpointIdentity;
				if (ident != null && ident.Certificates.Count > 0)
					cert = ident.Certificates [0];
				if (cert == null)
					credentials.ServiceCertificate.ScopedCertificates.TryGetValue (address.Uri, out cert);
			}
			if (cert == null)
				cert = credentials.ServiceCertificate.DefaultCertificate;
			return cert;
		}

		void InitializeProviderCommunicationObject (ProviderCommunicationObject p, SecurityTokenRequirement r)
		{
			p.TargetAddress = r.GetProperty<EndpointAddress> (ReqType.TargetAddressProperty);

			// FIXME: use it somewhere, probably to build 
			// IssuerBinding. However, there is also IssuerBinding 
			// property. SecureConversationSecurityBindingElement
			// as well.
			SecurityBindingElement sbe =
				r.GetProperty<SecurityBindingElement> (ReqType.SecurityBindingElementProperty);

			// I doubt the binding is acquired this way ...
			Binding binding;
			if (!r.TryGetProperty<Binding> (ReqType.IssuerBindingProperty, out binding))
				binding = new CustomBinding (
					new TextMessageEncodingBindingElement (),
					new HttpTransportBindingElement ());
			p.IssuerBinding = binding;

			// not sure if it is used only for this purpose though ...
			BindingContext ctx = r.GetProperty<BindingContext> (ReqType.IssuerBindingContextProperty);
			foreach (IEndpointBehavior b in ctx.BindingParameters.FindAll<IEndpointBehavior> ())
				p.IssuerChannelBehaviors.Add (b);

			SecurityTokenVersion ver =
				r.GetProperty<SecurityTokenVersion> (ReqType.MessageSecurityVersionProperty);
			p.SecurityTokenSerializer =
				CreateSecurityTokenSerializer (ver);

			// seems like they are optional here ... (but possibly
			// used later)
			EndpointAddress address;
			if (!r.TryGetProperty<EndpointAddress> (ReqType.IssuerAddressProperty, out address))
				address = p.TargetAddress;
			p.IssuerAddress = address;

			// It is somehow not checked as mandatory ...
			SecurityAlgorithmSuite suite = null;
			r.TryGetProperty<SecurityAlgorithmSuite> (ReqType.SecurityAlgorithmSuiteProperty, out suite);
			p.SecurityAlgorithmSuite = suite;
		}

		// FIXME: it is far from done.
		SecurityTokenProvider CreateSecureConversationProvider (SecurityTokenRequirement r)
		{
			IssuedSecurityTokenProvider p =
				new IssuedSecurityTokenProvider ();
			InitializeProviderCommunicationObject (p.Communication, r);

			// FIXME: use it somewhere.
			int keySize = r.KeySize;

			return p;
		}

		SecurityTokenProvider CreateSslnegoProvider (SecurityTokenRequirement r)
		{
			SslSecurityTokenProvider p = new SslSecurityTokenProvider (this, r.TokenType == ServiceModelSecurityTokenTypes.MutualSslnego);
			InitializeProviderCommunicationObject (p.Communication, r);

			return p;
		}

		SecurityTokenProvider CreateSpnegoProvider (SecurityTokenRequirement r)
		{
			SpnegoSecurityTokenProvider p = new SpnegoSecurityTokenProvider (this, r);
			InitializeProviderCommunicationObject (p.Communication, r);

			return p;
		}

		IssuedSecurityTokenProvider CreateIssuedTokenProvider (SecurityTokenRequirement requirement)
		{
			IssuedSecurityTokenProvider p =
				new IssuedSecurityTokenProvider ();
			// FIXME: fill properties
			EndpointAddress address;
			if (requirement.TryGetProperty<EndpointAddress> (ReqType.IssuerAddressProperty, out address))
				p.IssuerAddress = address;
			if (requirement.TryGetProperty<EndpointAddress> (ReqType.TargetAddressProperty, out address))
				p.TargetAddress = address;
			Binding binding;
			if (requirement.TryGetProperty<Binding> (ReqType.IssuerBindingProperty, out binding))
				p.IssuerBinding = binding;
			MessageSecurityVersion ver;
			if (requirement.TryGetProperty<MessageSecurityVersion> (ReqType.MessageSecurityVersionProperty, out ver))
				p.SecurityTokenSerializer = CreateSecurityTokenSerializer (ver.SecurityVersion);
			SecurityAlgorithmSuite suite;
			if (requirement.TryGetProperty<SecurityAlgorithmSuite> (ReqType.SecurityAlgorithmSuiteProperty, out suite))
				p.SecurityAlgorithmSuite = suite;
			return p;
		}

		#endregion

		public override SecurityTokenSerializer CreateSecurityTokenSerializer (SecurityTokenVersion version)
		{
			bool bsp = version.GetSecuritySpecifications ().Contains (Constants.WSBasicSecurityProfileCore1);
			SecurityVersion ver =
				version.GetSecuritySpecifications ().Contains (Constants.Wss11Namespace) ?
				SecurityVersion.WSSecurity11 :
				SecurityVersion.WSSecurity10;
			return new WSSecurityTokenSerializer (ver, bsp);
		}

		protected SecurityTokenSerializer CreateSecurityTokenSerializer (SecurityVersion version)
		{
			return new WSSecurityTokenSerializer (version);
		}

		protected internal bool IsIssuedSecurityTokenRequirement (
			SecurityTokenRequirement requirement)
		{
			SecurityTokenParameters ret;
			if (!requirement.TryGetProperty<SecurityTokenParameters> (ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty, out ret))
				return false;
			return ret is IssuedSecurityTokenParameters;
		}
	}
}
