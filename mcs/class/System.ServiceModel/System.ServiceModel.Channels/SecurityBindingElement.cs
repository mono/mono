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
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
#if !NET_2_1 && !XAMMAC_4_5
using System.ServiceModel.Channels.Security;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
#endif
using System.ServiceModel.Security.Tokens;
using System.Text;

namespace System.ServiceModel.Channels
{
	public abstract class SecurityBindingElement : BindingElement
	{
		internal SecurityBindingElement ()
		{
			MessageSecurityVersion = MessageSecurityVersion.Default;
			endpoint = new SupportingTokenParameters ();
#if !NET_2_1 && !XAMMAC_4_5
			DefaultAlgorithmSuite = SecurityAlgorithmSuite.Default;
			KeyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
			operation = new Dictionary<string,SupportingTokenParameters> ();
			opt_endpoint = new SupportingTokenParameters ();
			opt_operation = new Dictionary<string,SupportingTokenParameters> ();
			service_settings = new LocalServiceSecuritySettings ();
#endif
			IncludeTimestamp = true;
			LocalClientSettings = new LocalClientSecuritySettings ();
		}

		internal SecurityBindingElement (SecurityBindingElement other)
		{
			security_header_layout = other.security_header_layout;
			msg_security_version = other.msg_security_version;
			endpoint = other.endpoint.Clone ();
#if !NET_2_1 && !XAMMAC_4_5
			alg_suite = other.alg_suite;
			key_entropy_mode = other.key_entropy_mode;
			opt_endpoint = other.opt_endpoint.Clone ();
			operation = new Dictionary<string,SupportingTokenParameters> ();
			foreach (KeyValuePair<string,SupportingTokenParameters> p in other.operation)
				operation.Add (p.Key, p.Value.Clone ());
			opt_operation = new Dictionary<string,SupportingTokenParameters> ();
			foreach (KeyValuePair<string,SupportingTokenParameters> p in other.opt_operation)
				opt_operation.Add (p.Key, p.Value.Clone ());
			service_settings = other.service_settings.Clone ();
#endif
			IncludeTimestamp = other.IncludeTimestamp;
			LocalClientSettings = other.LocalClientSettings.Clone ();
		}

		SecurityHeaderLayout security_header_layout;
		MessageSecurityVersion msg_security_version;
		SupportingTokenParameters endpoint;

#if !NET_2_1 && !XAMMAC_4_5
		SecurityAlgorithmSuite alg_suite;
		SecurityKeyEntropyMode key_entropy_mode;
		SupportingTokenParameters opt_endpoint;
		IDictionary<string,SupportingTokenParameters> operation, opt_operation;
		LocalServiceSecuritySettings service_settings;
#endif

		public bool IncludeTimestamp { get; set; }

		public LocalClientSecuritySettings LocalClientSettings { get; private set; }

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

#if !NET_2_1 && !XAMMAC_4_5
		public SecurityAlgorithmSuite DefaultAlgorithmSuite {
			get { return alg_suite; }
			set { alg_suite = value; }
		}

		public SecurityKeyEntropyMode KeyEntropyMode {
			get { return key_entropy_mode; }
			set { key_entropy_mode = value; }
		}

		public LocalServiceSecuritySettings LocalServiceSettings {
			get { return service_settings; }
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
#endif

		[MonoTODO ("Implement for TransportSecurityBindingElement")]
		public override bool CanBuildChannelFactory<TChannel> (BindingContext context)
		{
#if NET_2_1 || XAMMAC_4_5
			// not sure this should be like this, but there isn't Symmetric/Asymmetric elements in 2.1 anyways.
			return context.CanBuildInnerChannelFactory<TChannel> ();
#else
			if (this is TransportSecurityBindingElement)
				throw new NotImplementedException ();

			var symm = this as SymmetricSecurityBindingElement;
			var asymm = this as AsymmetricSecurityBindingElement;
			var pt = symm != null ? symm.ProtectionTokenParameters : asymm != null ? asymm.InitiatorTokenParameters : null;
			if (pt == null)
				return false;

			var t = typeof (TChannel);
			var req = new InitiatorServiceModelSecurityTokenRequirement ();
			pt.InitializeSecurityTokenRequirement (req);
			object dummy;
			if (req.Properties.TryGetValue (ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty, out dummy) && dummy != null) {
				if (t == typeof (IRequestSessionChannel))
					return context.CanBuildInnerChannelFactory<IRequestChannel> () ||
						context.CanBuildInnerChannelFactory<IRequestSessionChannel> ();
				else if (t == typeof (IDuplexSessionChannel))
					return context.CanBuildInnerChannelFactory<IDuplexChannel> () ||
						context.CanBuildInnerChannelFactory<IDuplexSessionChannel> ();
			} else {
				if (t == typeof (IRequestChannel))
					return context.CanBuildInnerChannelFactory<IRequestChannel> () ||
						context.CanBuildInnerChannelFactory<IRequestSessionChannel> ();
				else if (t == typeof (IDuplexChannel))
					return context.CanBuildInnerChannelFactory<IDuplexChannel> () ||
						context.CanBuildInnerChannelFactory<IDuplexSessionChannel> ();
			}
			return false;
#endif
		}

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (
			BindingContext context)
		{
			return BuildChannelFactoryCore<TChannel> (context);
		}

		protected abstract IChannelFactory<TChannel>
			BuildChannelFactoryCore<TChannel> (BindingContext context);

#if !NET_2_1 && !XAMMAC_4_5
		[MonoTODO ("Implement for TransportSecurityBindingElement")]
		public override bool CanBuildChannelListener<TChannel> (BindingContext context)
		{
			if (this is TransportSecurityBindingElement)
				throw new NotImplementedException ();

			var symm = this as SymmetricSecurityBindingElement;
			var asymm = this as AsymmetricSecurityBindingElement;
			var pt = symm != null ? symm.ProtectionTokenParameters : asymm != null ? asymm.RecipientTokenParameters : null;
			if (pt == null)
				return false;

			var t = typeof (TChannel);
			var req = new InitiatorServiceModelSecurityTokenRequirement ();
			pt.InitializeSecurityTokenRequirement (req);
			object dummy;
			if (req.Properties.TryGetValue (ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty, out dummy) && dummy != null) {
				if (t == typeof (IReplySessionChannel))
					return context.CanBuildInnerChannelListener<IReplyChannel> () ||
						context.CanBuildInnerChannelListener<IReplySessionChannel> ();
				else if (t == typeof (IDuplexSessionChannel))
					return context.CanBuildInnerChannelListener<IDuplexChannel> () ||
						context.CanBuildInnerChannelListener<IDuplexSessionChannel> ();
			} else {
				if (t == typeof (IReplyChannel))
					return context.CanBuildInnerChannelListener<IReplyChannel> () ||
						context.CanBuildInnerChannelListener<IReplySessionChannel> ();
				else if (t == typeof (IDuplexChannel))
					return context.CanBuildInnerChannelListener<IDuplexChannel> () ||
						context.CanBuildInnerChannelListener<IDuplexSessionChannel> ();
			}
			return false;
		}

		public override IChannelListener<TChannel> BuildChannelListener<TChannel> (
			BindingContext context)
		{
			return BuildChannelListenerCore<TChannel> (context);
		}

		protected abstract IChannelListener<TChannel> 
			BuildChannelListenerCore<TChannel> (BindingContext context)
			where TChannel : class, IChannel;

		public override T GetProperty<T> (BindingContext context)
		{
			// It is documented that ISecurityCapabilities and IdentityVerifier can be returned.
			// Though, this class is not inheritable, and they are returned by the derived types.
			// So I don't care about them here.
			return context.GetInnerProperty<T> ();
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

		public override string ToString ()
		{
			var sb = new StringBuilder ();
			sb.Append (GetType ().FullName).Append (":\n");
			foreach (var pi in GetType ().GetProperties ()) {
				var simple = Type.GetTypeCode (pi.PropertyType) != TypeCode.Object;
				var val = pi.GetValue (this, null);
				sb.Append (pi.Name).Append (':');
				if (val != null)
					sb.AppendFormat ("{0}{1}{2}", simple ? " " : "\n", simple ? "" : "  ", String.Join ("\n  ", val.ToString ().Split ('\n')));
				sb.Append ('\n');
			}
			sb.Length--; // chop trailing EOL.
			return sb.ToString ();
		}
#else
		[MonoTODO]
		public override T GetProperty<T> (BindingContext context)
		{
			return null;
		}
#endif

		#region Factory methods
#if !NET_2_1 && !XAMMAC_4_5
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

		public static TransportSecurityBindingElement 
			CreateCertificateOverTransportBindingElement (MessageSecurityVersion version)
		{
			var be = new TransportSecurityBindingElement () { MessageSecurityVersion = version };
			be.EndpointSupportingTokenParameters.SignedEncrypted.Add (new X509SecurityTokenParameters ());
			return be;
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

		public static SecurityBindingElement 
			CreateMutualCertificateBindingElement ()
		{
			return CreateMutualCertificateBindingElement (MessageSecurityVersion.Default, false);
		}

		public static SecurityBindingElement 
			CreateMutualCertificateBindingElement (MessageSecurityVersion version)
		{
			return CreateMutualCertificateBindingElement (version, false);
		}

		[MonoTODO("Does not support allowSerializedSigningTokenOnReply.")]
		public static SecurityBindingElement 
			CreateMutualCertificateBindingElement (
			MessageSecurityVersion version,
			bool allowSerializedSigningTokenOnReply)
		{
			if (version == null)
				throw new ArgumentNullException ("version");
			
			if (allowSerializedSigningTokenOnReply)
				throw new NotSupportedException ("allowSerializedSigningTokenOnReply is not supported");
			
			if (version.SecurityVersion == SecurityVersion.WSSecurity10) {
			
				var recipient = new X509SecurityTokenParameters (
					X509KeyIdentifierClauseType.Any,	
				    SecurityTokenInclusionMode.Never);
				recipient.RequireDerivedKeys = false;
				
				var initiator = new X509SecurityTokenParameters (
				    X509KeyIdentifierClauseType.Any, 
				    SecurityTokenInclusionMode.AlwaysToRecipient);
				initiator.RequireDerivedKeys = false;                                          
				                                                 
				return new AsymmetricSecurityBindingElement (recipient, initiator) {
					MessageSecurityVersion = version
				};
			} else {
				X509SecurityTokenParameters p =
					new X509SecurityTokenParameters (X509KeyIdentifierClauseType.Thumbprint);
				p.RequireDerivedKeys = false;
					
				var sym = new SymmetricSecurityBindingElement () {
					MessageSecurityVersion = version,
					RequireSignatureConfirmation = true
			};
				
				X509SecurityTokenParameters p2 = new X509SecurityTokenParameters (X509KeyIdentifierClauseType.Thumbprint);
				p2.ReferenceStyle = SecurityTokenReferenceStyle.External;
				sym.ProtectionTokenParameters = p2;
				sym.EndpointSupportingTokenParameters.Endorsing.Add (p);
				return sym;
			}
			
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

		public static SymmetricSecurityBindingElement 
			CreateSslNegotiationBindingElement (bool requireClientCertificate)
		{
			return CreateSslNegotiationBindingElement (
				requireClientCertificate, false);
		}

		public static SymmetricSecurityBindingElement 
			CreateSslNegotiationBindingElement (
			bool requireClientCertificate,
			bool requireCancellation)
		{
			SymmetricSecurityBindingElement be = new SymmetricSecurityBindingElement ();
			be.ProtectionTokenParameters = new SslSecurityTokenParameters (requireClientCertificate, requireCancellation);
			return be;
		}

		public static SymmetricSecurityBindingElement 
			CreateSspiNegotiationBindingElement ()
		{
			return CreateSspiNegotiationBindingElement (true);
		}

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

		public static SymmetricSecurityBindingElement 
			CreateUserNameForSslBindingElement ()
		{
			return CreateUserNameForSslBindingElement (false);
		}

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
#endif

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

		public static SecurityBindingElement 
			CreateSecureConversationBindingElement (
			SecurityBindingElement binding, bool requireCancellation,
			ChannelProtectionRequirements protectionRequirements)
		{
#if !NET_2_1 && !XAMMAC_4_5
			SymmetricSecurityBindingElement be =
				new SymmetricSecurityBindingElement ();
			be.ProtectionTokenParameters =
				new SecureConversationSecurityTokenParameters (
					binding, requireCancellation, protectionRequirements);
			return be;
#else
			throw new NotImplementedException ();
#endif
		}

		[MonoTODO]
		public static TransportSecurityBindingElement 
			CreateUserNameOverTransportBindingElement ()
		{
			var be = new TransportSecurityBindingElement ();
#if !NET_2_1 && !XAMMAC_4_5 // FIXME: there should be whatever else to do for 2.1 instead.
			be.EndpointSupportingTokenParameters.SignedEncrypted.Add (new UserNameSecurityTokenParameters ());
#endif
			return be;
		}
		#endregion

#if !NET_2_1 && !XAMMAC_4_5
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
#endif
	}
}
