//
// SecurityBindingElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel.Channels
{
	public abstract class SecurityBindingElement : BindingElement
	{
		internal SecurityBindingElement ()
		{
			DefaultAlgorithmSuite = SecurityAlgorithmSuite.Default;
			MessageSecurityVersion = MessageSecurityVersion.Default;
			IncludeTimestamp = true;
			KeyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
			endpoint = new SupportingTokenParameters ();
			operation = new Dictionary<string,SupportingTokenParameters> ();
			opt_endpoint = new SupportingTokenParameters ();
			opt_operation = new Dictionary<string,SupportingTokenParameters> ();
			client_settings = new LocalClientSecuritySettings ();
			service_settings = new LocalServiceSecuritySettings ();
		}

		internal SecurityBindingElement (SecurityBindingElement other)
		{
			alg_suite = other.alg_suite;
			include_timestamp = other.include_timestamp;
			key_entropy_mode = other.key_entropy_mode;
			security_header_layout = other.security_header_layout;
			msg_security_version = other.msg_security_version;
			endpoint = other.endpoint.Clone ();
			opt_endpoint = other.opt_endpoint.Clone ();
			operation = new Dictionary<string,SupportingTokenParameters> ();
			foreach (KeyValuePair<string,SupportingTokenParameters> p in other.operation)
				operation.Add (p.Key, p.Value.Clone ());
			opt_operation = new Dictionary<string,SupportingTokenParameters> ();
			foreach (KeyValuePair<string,SupportingTokenParameters> p in other.opt_operation)
				opt_operation.Add (p.Key, p.Value.Clone ());
			client_settings = other.client_settings.Clone ();
			service_settings = other.service_settings.Clone ();
		}

		SecurityAlgorithmSuite alg_suite;
		bool include_timestamp;
		SecurityKeyEntropyMode key_entropy_mode;
		SecurityHeaderLayout security_header_layout;
		MessageSecurityVersion msg_security_version;
		SupportingTokenParameters endpoint, opt_endpoint;
		IDictionary<string,SupportingTokenParameters> operation, opt_operation;
		LocalClientSecuritySettings client_settings;
		LocalServiceSecuritySettings service_settings;

		public SecurityAlgorithmSuite DefaultAlgorithmSuite {
			get { return alg_suite; }
			set { alg_suite = value; }
		}

		public bool IncludeTimestamp {
			get { return include_timestamp; }
			set { include_timestamp = value; }
		}

		public SecurityKeyEntropyMode KeyEntropyMode {
			get { return key_entropy_mode; }
			set { key_entropy_mode = value; }
		}

		public LocalClientSecuritySettings LocalClientSettings {
			get { return client_settings; }
		}

		public LocalServiceSecuritySettings LocalServiceSettings {
			get { return service_settings; }
		}

		public SecurityHeaderLayout SecurityHeaderLayout {
			get { return security_header_layout; }
			set { security_header_layout = value; }
		}

		public MessageSecurityVersion MessageSecurityVersion {
			get { return msg_security_version; }
			set { msg_security_version = value; }
		}

		public SupportingTokenParameters EndpointSupportingTokenParameters {
			get { return endpoint; }
		}

		public IDictionary<string,SupportingTokenParameters> OperationSupportingTokenParameters {
			get { return operation; }
		}

		public SupportingTokenParameters OptionalEndpointSupportingTokenParameters {
			get { return opt_endpoint; }
		}

		public IDictionary<string,SupportingTokenParameters> OptionalOperationSupportingTokenParameters {
			get { return opt_operation; }
		}

		[MonoTODO ("It supports only IRequestSessionChannel")]
		public override bool CanBuildChannelFactory<TChannel> (BindingContext context)
		{
			return context.CanBuildInnerChannelFactory<TChannel> ();
		}

		[MonoTODO ("It probably supports only IReplySessionChannel")]
		public override bool CanBuildChannelListener<TChannel> (BindingContext context)
		{
			return context.CanBuildInnerChannelListener<TChannel> ();
		}

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (
			BindingContext context)
		{
			return BuildChannelFactoryCore<TChannel> (context);
		}

		public override IChannelListener<TChannel> BuildChannelListener<TChannel> (
			BindingContext context)
		{
			return BuildChannelListenerCore<TChannel> (context);
		}

		public virtual void SetKeyDerivation (bool requireDerivedKeys)
		{
			endpoint.SetKeyDerivation (requireDerivedKeys);
			opt_endpoint.SetKeyDerivation (requireDerivedKeys);
			foreach (SupportingTokenParameters p in operation.Values)
				p.SetKeyDerivation (requireDerivedKeys);
			foreach (SupportingTokenParameters p in opt_operation.Values)
				p.SetKeyDerivation (requireDerivedKeys);
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}

		protected abstract IChannelFactory<TChannel>
			BuildChannelFactoryCore<TChannel> (BindingContext context);

		protected abstract IChannelListener<TChannel> 
			BuildChannelListenerCore<TChannel> (BindingContext context)
			where TChannel : class, IChannel;

		#region Factory methods
		public static SymmetricSecurityBindingElement 
			CreateAnonymousForCertificateBindingElement ()
		{
			SymmetricSecurityBindingElement be = new SymmetricSecurityBindingElement ();
			be.RequireSignatureConfirmation = true;
			be.ProtectionTokenParameters = CreateProtectionTokenParameters (true);
			return be;
		}

		public static TransportSecurityBindingElement 
			CreateCertificateOverTransportBindingElement ()
		{
			return CreateCertificateOverTransportBindingElement (MessageSecurityVersion.Default);
		}

		[MonoTODO]
		public static TransportSecurityBindingElement 
			CreateCertificateOverTransportBindingElement (MessageSecurityVersion version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static AsymmetricSecurityBindingElement 
			CreateCertificateSignatureBindingElement  ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SymmetricSecurityBindingElement 
			CreateIssuedTokenBindingElement  (
			IssuedSecurityTokenParameters issuedTokenParameters)
		{
			SymmetricSecurityBindingElement be = new SymmetricSecurityBindingElement ();
			be.ProtectionTokenParameters = issuedTokenParameters;
			return be;
		}

		public static SymmetricSecurityBindingElement
			CreateIssuedTokenForCertificateBindingElement (
			IssuedSecurityTokenParameters issuedTokenParameters)
		{
			SymmetricSecurityBindingElement be = new SymmetricSecurityBindingElement ();
			be.RequireSignatureConfirmation = true;
			be.ProtectionTokenParameters = CreateProtectionTokenParameters (true);
			be.EndpointSupportingTokenParameters.Endorsing.Add (
				issuedTokenParameters);
			return be;
		}

		[MonoTODO]
		public static SymmetricSecurityBindingElement 
			CreateIssuedTokenForSslBindingElement (
			IssuedSecurityTokenParameters issuedTokenParameters)
		{
			return CreateIssuedTokenForSslBindingElement (
				issuedTokenParameters, false);
		}

		[MonoTODO]
		public static SymmetricSecurityBindingElement 
			CreateIssuedTokenForSslBindingElement (
			IssuedSecurityTokenParameters issuedTokenParameters,
			bool requireCancellation)
		{
			SymmetricSecurityBindingElement be = new SymmetricSecurityBindingElement ();
			be.RequireSignatureConfirmation = true;
			be.ProtectionTokenParameters = CreateProtectionTokenParameters (false);
			be.EndpointSupportingTokenParameters.Endorsing.Add (
				issuedTokenParameters);
			return be;
		}

		[MonoTODO]
		public static TransportSecurityBindingElement 
			CreateIssuedTokenOverTransportBindingElement (
			IssuedSecurityTokenParameters issuedTokenParameters)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SymmetricSecurityBindingElement CreateKerberosBindingElement ()
		{
			SymmetricSecurityBindingElement be = new SymmetricSecurityBindingElement ();
			be.DefaultAlgorithmSuite = SecurityAlgorithmSuite.Basic128;
			be.ProtectionTokenParameters = CreateProtectionTokenParameters (false);
			be.ProtectionTokenParameters.InclusionMode =
				SecurityTokenInclusionMode.Once;
			return be;
		}

		[MonoTODO]
		public static TransportSecurityBindingElement 
			CreateKerberosOverTransportBindingElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SecurityBindingElement 
			CreateMutualCertificateBindingElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SecurityBindingElement 
			CreateMutualCertificateBindingElement (MessageSecurityVersion version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SecurityBindingElement 
			CreateMutualCertificateBindingElement (
			MessageSecurityVersion version,
			bool allowSerializedSigningTokenOnReply)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static AsymmetricSecurityBindingElement 
			CreateMutualCertificateDuplexBindingElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static AsymmetricSecurityBindingElement 
			CreateMutualCertificateDuplexBindingElement (
			MessageSecurityVersion version)
		{
			throw new NotImplementedException ();
		}

		public static SecurityBindingElement 
			CreateSecureConversationBindingElement (SecurityBindingElement binding)
		{
			return CreateSecureConversationBindingElement (binding, false);
		}

		public static SecurityBindingElement 
			CreateSecureConversationBindingElement (
			SecurityBindingElement binding, bool requireCancellation)
		{
			return CreateSecureConversationBindingElement (binding, requireCancellation, null);
		}

		[MonoTODO]
		public static SecurityBindingElement 
			CreateSecureConversationBindingElement (
			SecurityBindingElement binding, bool requireCancellation,
			ChannelProtectionRequirements protectionRequirements)
		{
			SymmetricSecurityBindingElement be =
				new SymmetricSecurityBindingElement ();
			be.ProtectionTokenParameters =
				new SecureConversationSecurityTokenParameters (
					binding, requireCancellation, protectionRequirements);
			return be;
		}

		[MonoTODO]
		public static SymmetricSecurityBindingElement 
			CreateSslNegotiationBindingElement (bool requireClientCertificate)
		{
			return CreateSslNegotiationBindingElement (
				requireClientCertificate, false);
		}

		[MonoTODO]
		public static SymmetricSecurityBindingElement 
			CreateSslNegotiationBindingElement (
			bool requireClientCertificate,
			bool requireCancellation)
		{
			SymmetricSecurityBindingElement be = new SymmetricSecurityBindingElement ();
			be.ProtectionTokenParameters = new SslSecurityTokenParameters (requireClientCertificate, requireCancellation);
			return be;
		}

		[MonoTODO]
		public static SymmetricSecurityBindingElement 
			CreateSspiNegotiationBindingElement ()
		{
			return CreateSspiNegotiationBindingElement (true);
		}

		[MonoTODO]
		public static SymmetricSecurityBindingElement 
			CreateSspiNegotiationBindingElement (bool requireCancellation)
		{
			SymmetricSecurityBindingElement be = new SymmetricSecurityBindingElement ();
			be.ProtectionTokenParameters = CreateProtectionTokenParameters (false);
			return be;
		}

		public static TransportSecurityBindingElement 
			CreateSspiNegotiationOverTransportBindingElement ()
		{
			return CreateSspiNegotiationOverTransportBindingElement (false);
		}

		[MonoTODO]
		public static TransportSecurityBindingElement 
			CreateSspiNegotiationOverTransportBindingElement (bool requireCancellation)
		{
			throw new NotImplementedException ();
		}

		static X509SecurityTokenParameters CreateProtectionTokenParameters (bool cert)
		{
			X509SecurityTokenParameters p =
				new X509SecurityTokenParameters ();
			p.X509ReferenceStyle = X509KeyIdentifierClauseType.Thumbprint;
			if (cert)
				p.InclusionMode = SecurityTokenInclusionMode.Never;
			return p;
		}

		[MonoTODO]
		public static SymmetricSecurityBindingElement 
			CreateUserNameForCertificateBindingElement ()
		{
			SymmetricSecurityBindingElement be = new SymmetricSecurityBindingElement ();
			be.ProtectionTokenParameters = CreateProtectionTokenParameters (true);
			UserNameSecurityTokenParameters utp =
				new UserNameSecurityTokenParameters ();
			be.EndpointSupportingTokenParameters.SignedEncrypted.Add (utp);
			return be;
		}

		[MonoTODO]
		public static SymmetricSecurityBindingElement 
			CreateUserNameForSslBindingElement ()
		{
			return CreateUserNameForSslBindingElement (false);
		}

		[MonoTODO]
		public static SymmetricSecurityBindingElement 
			CreateUserNameForSslBindingElement (bool requireCancellation)
		{
			SymmetricSecurityBindingElement be = new SymmetricSecurityBindingElement ();
			be.ProtectionTokenParameters = CreateProtectionTokenParameters (false);
			UserNameSecurityTokenParameters utp =
				new UserNameSecurityTokenParameters ();
			be.EndpointSupportingTokenParameters.SignedEncrypted.Add (utp);
			return be;
		}

		[MonoTODO]
		public static TransportSecurityBindingElement 
			CreateUserNameOverTransportBindingElement ()
		{
			throw new NotImplementedException ();
		}
		#endregion

		// It seems almost internal, hardcoded like this (I tried
		// custom parameters that sets IssuedTokenSecurityTokenParameters
		// like below ones, but that didn't trigger this method).
		protected static void SetIssuerBindingContextIfRequired (
			SecurityTokenParameters parameters,
			BindingContext issuerBindingContext)
		{
			if (parameters is IssuedSecurityTokenParameters ||
			    parameters is SecureConversationSecurityTokenParameters ||
			    parameters is SslSecurityTokenParameters ||
			    parameters is SspiSecurityTokenParameters) {
				parameters.IssuerBindingContext = issuerBindingContext;
			}
		}
	}
}
