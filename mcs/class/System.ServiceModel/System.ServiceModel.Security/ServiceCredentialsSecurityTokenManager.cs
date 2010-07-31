//
// ServiceCredentialsSecurityTokenManager.cs
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security.Tokens;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace System.ServiceModel.Security
{
	public class ServiceCredentialsSecurityTokenManager : SecurityTokenManager, IEndpointIdentityProvider
	{
		ServiceCredentials credentials;

		public ServiceCredentialsSecurityTokenManager (
			ServiceCredentials credentials)
		{
			this.credentials = credentials;
		}

		public ServiceCredentials ServiceCredentials {
			get { return credentials; }
		}

		[MonoTODO]
		public virtual EndpointIdentity GetIdentityOfSelf (
			SecurityTokenRequirement requirement)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator (
			SecurityTokenRequirement requirement,
			out SecurityTokenResolver outOfBandTokenResolver)
		{
			outOfBandTokenResolver = null;
			if (requirement.TokenType == SecurityTokenTypes.UserName)
				return CreateUserNameAuthenticator (requirement);
			if (requirement.TokenType == SecurityTokenTypes.X509Certificate)
				return CreateX509Authenticator (requirement);
			if (requirement.TokenType == SecurityTokenTypes.Rsa)
				return new RsaSecurityTokenAuthenticator ();
			if (requirement.TokenType == ServiceModelSecurityTokenTypes.SecureConversation) {
				SecurityBindingElement binding;
				if (!requirement.TryGetProperty<SecurityBindingElement> (ReqType.SecurityBindingElementProperty, out binding))
					throw new ArgumentException ("SecurityBindingElement is required in the security token requirement");
				SecureConversationSecurityTokenParameters issuedParams;
				if (!requirement.TryGetProperty<SecureConversationSecurityTokenParameters> (ReqType.IssuedSecurityTokenParametersProperty, out issuedParams))
					throw new ArgumentException ("IssuedSecurityTokenParameters are required in the security token requirement");
				BindingContext issuerBC;
				if (!requirement.TryGetProperty<BindingContext> (ReqType.IssuerBindingContextProperty, out issuerBC))
					throw new ArgumentException ("IssuerBindingContext is required in the security token requirement");
				SecurityTokenVersion secVer;
				if (!requirement.TryGetProperty<SecurityTokenVersion> (ReqType.MessageSecurityVersionProperty, out secVer))
					throw new ArgumentException ("MessageSecurityVersion property (of type SecurityTokenVersion) is required in the security token requirement");

				// FIXME: get parameters from somewhere
				SecurityContextSecurityTokenResolver resolver =
					new SecurityContextSecurityTokenResolver (0x1000, true);
				outOfBandTokenResolver = resolver;
				SecurityContextSecurityTokenAuthenticator sc =
					new SecurityContextSecurityTokenAuthenticator ();
				return new SecureConversationSecurityTokenAuthenticator (requirement, sc, resolver);
			}
			if (requirement.TokenType == ServiceModelSecurityTokenTypes.AnonymousSslnego)
				return CreateSslTokenAuthenticator (requirement);
			if (requirement.TokenType == ServiceModelSecurityTokenTypes.MutualSslnego)
				return CreateSslTokenAuthenticator (requirement);
			if (requirement.TokenType == ServiceModelSecurityTokenTypes.Spnego)
				return CreateSpnegoTokenAuthenticator (requirement);
			else
				throw new NotImplementedException ("Not implemented token type: " + requirement.TokenType);
		}

		SpnegoSecurityTokenAuthenticator CreateSpnegoTokenAuthenticator (SecurityTokenRequirement requirement)
		{
			SpnegoSecurityTokenAuthenticator a =
				new SpnegoSecurityTokenAuthenticator (this, requirement);
			InitializeAuthenticatorCommunicationObject (a.Communication, requirement);
			return a;
		}

		SslSecurityTokenAuthenticator CreateSslTokenAuthenticator (SecurityTokenRequirement requirement)
		{
			SslSecurityTokenAuthenticator a =
				new SslSecurityTokenAuthenticator (this, requirement);
			InitializeAuthenticatorCommunicationObject (a.Communication, requirement);
			return a;
		}

		UserNameSecurityTokenAuthenticator CreateUserNameAuthenticator (SecurityTokenRequirement requirement)
		{
			UserNamePasswordServiceCredential c = ServiceCredentials.UserNameAuthentication;
			switch (c.UserNamePasswordValidationMode) {
			case UserNamePasswordValidationMode.MembershipProvider:
				if (c.MembershipProvider == null)
					throw new InvalidOperationException ("For MembershipProvider validation mode, MembershipProvider is required to create a user name token authenticator.");
				return new CustomUserNameSecurityTokenAuthenticator (UserNamePasswordValidator.CreateMembershipProviderValidator (c.MembershipProvider));
			case UserNamePasswordValidationMode.Windows:
				return new WindowsUserNameSecurityTokenAuthenticator (c.IncludeWindowsGroups);
			default:
				if (c.CustomUserNamePasswordValidator == null)
					throw new InvalidOperationException ("For Custom validation mode, CustomUserNamePasswordValidator is required to create a user name token authenticator.");
				return new CustomUserNameSecurityTokenAuthenticator (c.CustomUserNamePasswordValidator);
			}
		}

		X509SecurityTokenAuthenticator CreateX509Authenticator (SecurityTokenRequirement requirement)
		{
			X509CertificateInitiatorServiceCredential c = ServiceCredentials.ClientCertificate;
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

		void InitializeAuthenticatorCommunicationObject (AuthenticatorCommunicationObject p, SecurityTokenRequirement r)
		{
			p.ListenUri = r.GetProperty<Uri> (ReqType.ListenUriProperty);

			// FIXME: use it somewhere, probably to build 
			// IssuerBinding. However, there is also IssuerBinding 
			// property. SecureConversationSecurityBindingElement
			// as well.
			SecurityBindingElement sbe =
				r.GetProperty<SecurityBindingElement> (ReqType.SecurityBindingElementProperty);
			p.SecurityBindingElement = sbe;

/*
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
*/

			SecurityTokenVersion ver =
				r.GetProperty<SecurityTokenVersion> (ReqType.MessageSecurityVersionProperty);
			p.SecurityTokenSerializer =
				CreateSecurityTokenSerializer (ver);

/*
			// seems like they are optional here ... (but possibly
			// used later)
			EndpointAddress address;
			if (!r.TryGetProperty<EndpointAddress> (ReqType.IssuerAddressProperty, out address))
				address = p.TargetAddress;
			p.IssuerAddress = address;
*/

			// It is somehow not checked as mandatory ...
			SecurityAlgorithmSuite suite = null;
			r.TryGetProperty<SecurityAlgorithmSuite> (ReqType.SecurityAlgorithmSuiteProperty, out suite);
			p.SecurityAlgorithmSuite = suite;
		}

		#region CreateSecurityTokenProvider()

		[MonoTODO]
		public override SecurityTokenProvider CreateSecurityTokenProvider (SecurityTokenRequirement requirement)
		{
			if (IsIssuedSecurityTokenRequirement (requirement))
				return CreateIssuedTokenProvider (requirement);

			// not supported: UserName, Rsa, AnonymousSslnego, SecureConv

			// huh, they are not constants but properties.
			if (requirement.TokenType == SecurityTokenTypes.X509Certificate)
				return CreateX509SecurityTokenProvider (requirement);
			else if (requirement.TokenType == ServiceModelSecurityTokenTypes.MutualSslnego) {
				// FIXME: implement
				throw new NotImplementedException ();
			} else if (requirement.TokenType == ServiceModelSecurityTokenTypes.SecurityContext) {
				// FIXME: implement
				throw new NotImplementedException ();
			} else if (requirement.TokenType == ServiceModelSecurityTokenTypes.AnonymousSslnego) {
				throw new NotSupportedException (String.Format ("Token type '{0}' is not supported", requirement.TokenType));
			} else if (requirement.TokenType == ServiceModelSecurityTokenTypes.Spnego) {
				// FIXME: implement
				throw new NotImplementedException ();
			} else if (requirement.TokenType == ServiceModelSecurityTokenTypes.SspiCredential) {
				// FIXME: implement
				throw new NotImplementedException ();
			} else if (requirement.TokenType == SecurityTokenTypes.Saml) {
				// FIXME: implement
				throw new NotImplementedException ();
			} else if (requirement.TokenType == SecurityTokenTypes.Kerberos) {
				// FIXME: implement
				throw new NotImplementedException ();
			}
			throw new NotSupportedException (String.Format ("Securirty token requirement '{0}' is not supported", requirement));
		}

		X509SecurityTokenProvider CreateX509SecurityTokenProvider (SecurityTokenRequirement requirement)
		{
			bool isInitiator;
			requirement.TryGetProperty<bool> (ReqType.IsInitiatorProperty, out isInitiator);
			// when it is initiator, then it is for MutualCertificateDuplex.
			X509Certificate2 cert;
			if (isInitiator) {
				cert = credentials.ClientCertificate.Certificate;
				if (cert == null)
					throw new InvalidOperationException ("Client certificate is not provided in ServiceCredentials.");
				if (cert.PrivateKey == null)
					throw new ArgumentException ("Client certificate for MutualCertificateDuplex does not have a private key which is required for key exchange.");
			} else {
				cert = credentials.ServiceCertificate.Certificate;
				if (cert == null)
					throw new InvalidOperationException ("Service certificate is not provided in ServiceCredentials.");
				if (cert.PrivateKey == null)
					throw new ArgumentException ("Service certificate does not have a private key which is required for key exchange.");
			}
			X509SecurityTokenProvider p =
				new X509SecurityTokenProvider (cert);
			return p;
		}

		IssuedSecurityTokenProvider CreateIssuedProviderBase (SecurityTokenRequirement r)
		{
			IssuedSecurityTokenProvider p =
				new IssuedSecurityTokenProvider ();

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
				binding = new CustomBinding (sbe,
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

			return p;
		}

		// FIXME: it is far from done.
		SecurityTokenProvider CreateSecureConversationProvider (SecurityTokenRequirement r)
		{
			IssuedSecurityTokenProvider p =
				CreateIssuedProviderBase (r);

			// FIXME: use it somewhere.
			int keySize = r.KeySize;

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
				p.SecurityTokenSerializer = CreateSecurityTokenSerializer (ver.SecurityTokenVersion);
			SecurityAlgorithmSuite suite;
			if (requirement.TryGetProperty<SecurityAlgorithmSuite> (ReqType.SecurityAlgorithmSuiteProperty, out suite))
				p.SecurityAlgorithmSuite = suite;
			return p;
		}

		#endregion

		[MonoTODO ("pass correct arguments to WSSecurityTokenSerializer..ctor()")]
		public override SecurityTokenSerializer CreateSecurityTokenSerializer (SecurityTokenVersion version)
		{
			bool bsp = version.GetSecuritySpecifications ().Contains (Constants.WSBasicSecurityProfileCore1);
			SecurityVersion ver =
				version.GetSecuritySpecifications ().Contains (Constants.Wss11Namespace) ?
				SecurityVersion.WSSecurity11 :
				SecurityVersion.WSSecurity10;

			// FIXME: pass correct arguments.
			return new WSSecurityTokenSerializer (ver, bsp, null,
				ServiceCredentials.SecureConversationAuthentication.SecurityStateEncoder,
				Type.EmptyTypes,
				int.MaxValue, int.MaxValue, int.MaxValue);
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
