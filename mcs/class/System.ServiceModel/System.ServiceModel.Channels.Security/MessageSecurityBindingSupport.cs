//
// MessageSecurityBindingSupport.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2007 Novell, Inc.  http://www.novell.com
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
using System.Net.Security;
using System.Security.Cryptography.Xml;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace System.ServiceModel.Channels.Security
{
	internal abstract class MessageSecurityBindingSupport
	{
		SecurityTokenManager manager;
		ChannelProtectionRequirements requirements;
		SecurityTokenSerializer serializer;
		SecurityCapabilities element_support;

		// only filled at prepared state.
		SecurityTokenAuthenticator authenticator;
		SecurityTokenResolver auth_token_resolver;

		protected MessageSecurityBindingSupport (
			SecurityCapabilities elementSupport,
			SecurityTokenManager manager,
			ChannelProtectionRequirements requirements)
		{
			element_support = elementSupport;
			Initialize (manager, requirements);
		}

		public void Initialize (SecurityTokenManager manager,
			ChannelProtectionRequirements requirements)
		{
			this.manager = manager;
			if (requirements == null)
				requirements = new ChannelProtectionRequirements ();
			this.requirements = requirements;
		}

		public abstract IDefaultCommunicationTimeouts Timeouts { get; }

		public ChannelProtectionRequirements ChannelRequirements {
			get { return requirements; }
		}

		public SecurityTokenManager SecurityTokenManager {
			get { return manager; }
		}

		public SecurityTokenSerializer TokenSerializer {
			get {
				if (serializer == null)
					serializer = manager.CreateSecurityTokenSerializer (Element.MessageSecurityVersion.SecurityTokenVersion);
				return serializer;
			}
		}

		public SecurityTokenAuthenticator TokenAuthenticator {
			get { return authenticator; }
		}

		public SecurityTokenResolver OutOfBandTokenResolver {
			get { return auth_token_resolver; }
		}

		public abstract SecurityToken EncryptionToken { get; }

		public abstract SecurityToken SigningToken { get; }

		#region element_support

		public SecurityBindingElement Element {
			get { return element_support.Element; }
		}

		public bool AllowSerializedSigningTokenOnReply {
			get { return element_support.AllowSerializedSigningTokenOnReply; }
		}

		public MessageProtectionOrder MessageProtectionOrder { 
			get { return element_support.MessageProtectionOrder; }
		}

		public SecurityTokenParameters InitiatorParameters { 
			get { return element_support.InitiatorParameters; }
		}

		public SecurityTokenParameters RecipientParameters { 
			get { return element_support.RecipientParameters; }
		}

		public bool RequireSignatureConfirmation {
			get { return element_support.RequireSignatureConfirmation; }
		}

		public string DefaultSignatureAlgorithm {
			get { return element_support.DefaultSignatureAlgorithm; }
		}

		public string DefaultKeyWrapAlgorithm {
			get { return element_support.DefaultKeyWrapAlgorithm; }
		}

		#endregion

		public SecurityTokenProvider CreateTokenProvider (SecurityTokenRequirement requirement)
		{
			return manager.CreateSecurityTokenProvider (requirement);
		}

		public abstract SecurityTokenAuthenticator CreateTokenAuthenticator (SecurityTokenParameters p, out SecurityTokenResolver resolver);

		protected void PrepareAuthenticator ()
		{
			authenticator = CreateTokenAuthenticator (RecipientParameters, out auth_token_resolver);
		}

		protected void InitializeRequirement (SecurityTokenParameters p, SecurityTokenRequirement r)
		{
			p.CallInitializeSecurityTokenRequirement (r);

			// r.Properties [ChannelParametersCollectionProperty] =
			// r.Properties [ReqType.EndpointFilterTableProperty] =
			// r.Properties [ReqType.HttpAuthenticationSchemeProperty] =
			// r.Properties [ReqType.IsOutOfBandTokenProperty] =
			// r.Properties [ReqType.IssuerAddressProperty] =
			// r.Properties [ReqType.MessageDirectionProperty] = 
			r.Properties [ReqType.MessageSecurityVersionProperty] = Element.MessageSecurityVersion.SecurityTokenVersion;
			r.Properties [ReqType.SecurityAlgorithmSuiteProperty] = Element.DefaultAlgorithmSuite;
			r.Properties [ReqType.SecurityBindingElementProperty] = Element;
			// r.Properties [ReqType.SupportingTokenAttachmentModeProperty] =
			// r.TransportScheme =
		}

		public void Release ()
		{
			ReleaseCore ();

			authenticator = null;
		}

		protected abstract void ReleaseCore ();

		public SupportingTokenInfoCollection CollectSupportingTokens (string action)
		{
			SupportingTokenInfoCollection tokens =
				new SupportingTokenInfoCollection ();

			SupportingTokenParameters supp;

			CollectSupportingTokensCore (tokens, Element.EndpointSupportingTokenParameters, true);
			if (Element.OperationSupportingTokenParameters.TryGetValue (action, out supp))
				CollectSupportingTokensCore (tokens, supp, true);
			CollectSupportingTokensCore (tokens, Element.OptionalEndpointSupportingTokenParameters, false);
			if (Element.OptionalOperationSupportingTokenParameters.TryGetValue (action, out supp))
				CollectSupportingTokensCore (tokens, supp, false);

			return tokens;
		}

		void CollectSupportingTokensCore (
			SupportingTokenInfoCollection l,
			SupportingTokenParameters s,
			bool required)
		{
			foreach (SecurityTokenParameters p in s.Signed)
				l.Add (new SupportingTokenInfo (GetSigningToken (p), SecurityTokenAttachmentMode.Signed, required));
			foreach (SecurityTokenParameters p in s.Endorsing)
				l.Add (new SupportingTokenInfo (GetSigningToken (p), SecurityTokenAttachmentMode.Endorsing, required));
			foreach (SecurityTokenParameters p in s.SignedEndorsing)
				l.Add (new SupportingTokenInfo (GetSigningToken (p), SecurityTokenAttachmentMode.SignedEndorsing, required));
			foreach (SecurityTokenParameters p in s.SignedEncrypted)
				l.Add (new SupportingTokenInfo (GetSigningToken (p), SecurityTokenAttachmentMode.SignedEncrypted, required));
		}

		SecurityToken GetSigningToken (SecurityTokenParameters p)
		{
			return GetToken (CreateRequirement (), p, SecurityKeyUsage.Signature);
		}

		SecurityToken GetExchangeToken (SecurityTokenParameters p)
		{
			return GetToken (CreateRequirement (), p, SecurityKeyUsage.Exchange);
		}

		public SecurityToken GetToken (SecurityTokenRequirement requirement, SecurityTokenParameters targetParams, SecurityKeyUsage usage)
		{
			requirement.KeyUsage = usage;
			requirement.Properties [ReqType.SecurityBindingElementProperty] = Element;
			requirement.Properties [ReqType.MessageSecurityVersionProperty] =
				Element.MessageSecurityVersion.SecurityTokenVersion;

			InitializeRequirement (targetParams, requirement);

			SecurityTokenProvider provider =
				CreateTokenProvider (requirement);
			ICommunicationObject obj = provider as ICommunicationObject;
			try {
				if (obj != null)
					obj.Open (Timeouts.OpenTimeout);
				return provider.GetToken (Timeouts.SendTimeout);
			} finally {
				if (obj != null && obj.State == CommunicationState.Opened)
					obj.Close ();
			}
		}
		
		public abstract SecurityTokenRequirement CreateRequirement ();
	}

	internal class InitiatorMessageSecurityBindingSupport : MessageSecurityBindingSupport
	{
		ChannelFactoryBase factory;
		EndpointAddress message_to;
		SecurityToken encryption_token;
		SecurityToken signing_token;

		public InitiatorMessageSecurityBindingSupport (
			SecurityCapabilities elementSupport,
			SecurityTokenManager manager,
			ChannelProtectionRequirements requirements)
			: base (elementSupport, manager, requirements)
		{
		}

		public override IDefaultCommunicationTimeouts Timeouts {
			get { return factory; }
		}

		public void Prepare (ChannelFactoryBase factory, EndpointAddress address)
		{
			this.factory = factory;
			this.message_to = address;

			PrepareAuthenticator ();

			// This check is almost extra, though it is needed
			// to check correct signing token existence.
			if (EncryptionToken == null)
				throw new Exception ("INTERNAL ERROR");
		}

		public override SecurityToken EncryptionToken {
			get {
				if (encryption_token == null) {
					SecurityTokenRequirement r = CreateRequirement ();
					r.Properties [ReqType.MessageDirectionProperty] = MessageDirection.Input;
					InitializeRequirement (RecipientParameters, r);
					encryption_token = GetToken (r, RecipientParameters, SecurityKeyUsage.Exchange);
				}
				return encryption_token;
			}
		}

		public override SecurityToken SigningToken {
			get {
				if (signing_token == null) {
					SecurityTokenRequirement r = CreateRequirement ();
					r.Properties [ReqType.MessageDirectionProperty] = MessageDirection.Input;
					InitializeRequirement (InitiatorParameters, r);
					signing_token = GetToken (r, InitiatorParameters, SecurityKeyUsage.Signature);
				}
				return signing_token;
			}
		}

		protected override void ReleaseCore ()
		{
			this.factory = null;
			this.message_to = null;

			IDisposable disposable = signing_token as IDisposable;
			if (disposable != null)
				disposable.Dispose ();
			signing_token = null;

			disposable = encryption_token as IDisposable;
			if (disposable != null)
				disposable.Dispose ();
			encryption_token = null;
		}

		public override SecurityTokenRequirement CreateRequirement ()
		{
			SecurityTokenRequirement r = new InitiatorServiceModelSecurityTokenRequirement ();
//			r.Properties [ReqType.IssuerAddressProperty] = message_to;
			r.Properties [ReqType.TargetAddressProperty] = message_to;
			// FIXME: set Via
			return r;
		}

		public override SecurityTokenAuthenticator CreateTokenAuthenticator (SecurityTokenParameters p, out SecurityTokenResolver resolver)
		{
			resolver = null;
			// This check might be almost extra, though it is
			// needed to check correct signing token existence.
			//
			// Not sure if it is limited to this condition, but
			// Ssl parameters do not support token provider and
			// still do not fail. X509 parameters do fail.
			if (!InitiatorParameters.InternalSupportsClientAuthentication)
				return null;

			SecurityTokenRequirement r = CreateRequirement ();
			r.Properties [ReqType.MessageDirectionProperty] = MessageDirection.Output;
			InitializeRequirement (p, r);
			return SecurityTokenManager.CreateSecurityTokenAuthenticator (r, out resolver);
		}
	}

	class RecipientMessageSecurityBindingSupport : MessageSecurityBindingSupport
	{
		ChannelListenerBase listener;
		SecurityToken encryption_token;
		SecurityToken signing_token;

		public RecipientMessageSecurityBindingSupport (
			SecurityCapabilities elementSupport,
			SecurityTokenManager manager,
			ChannelProtectionRequirements requirements)
			: base (elementSupport, manager, requirements)
		{
		}

		public override IDefaultCommunicationTimeouts Timeouts {
			get { return listener; }
		}

		public void Prepare (ChannelListenerBase listener)
		{
			this.listener = listener;

			PrepareAuthenticator ();

			// This check is almost extra, though it is needed
			// to check correct signing token existence.
			//
			// Not sure if it is limited to this condition, but
			// Ssl parameters do not support token provider and
			// still do not fail. X509 parameters do fail.
			//
			// FIXME: as AsymmetricSecurityBindingElementTest
			// .ServiceRecipientHasNoKeys() implies, it should be
			// the recipient's parameters that is used. However
			// such changes will break some of existing tests...
			if (InitiatorParameters.InternalHasAsymmetricKey &&
			    EncryptionToken == null)
				throw new Exception ("INTERNAL ERROR");
		}

		public override SecurityToken EncryptionToken {
			get {
				if (encryption_token == null) {
					SecurityTokenRequirement r = CreateRequirement ();
					r.Properties [ReqType.MessageDirectionProperty] = MessageDirection.Output;
					encryption_token = GetToken (r, InitiatorParameters, SecurityKeyUsage.Exchange);
				}
				return encryption_token;
			}
		}

		public override SecurityToken SigningToken {
			get {
				if (signing_token == null) {
					SecurityTokenRequirement r = CreateRequirement ();
					r.Properties [ReqType.MessageDirectionProperty] = MessageDirection.Input;
					InitializeRequirement (RecipientParameters, r);
					signing_token = GetToken (r, RecipientParameters, SecurityKeyUsage.Signature);
				}
				return signing_token;
			}
		}

		protected override void ReleaseCore ()
		{
			this.listener = null;

			IDisposable disposable = signing_token as IDisposable;
			if (disposable != null)
				disposable.Dispose ();
			signing_token = null;

			disposable = encryption_token as IDisposable;
			if (disposable != null)
				disposable.Dispose ();
			encryption_token = null;
		}

		public override SecurityTokenRequirement CreateRequirement ()
		{
			SecurityTokenRequirement requirement =
				new RecipientServiceModelSecurityTokenRequirement ();
			requirement.Properties [ReqType.ListenUriProperty] = listener.Uri;
			return requirement;
		}

		public override SecurityTokenAuthenticator CreateTokenAuthenticator (SecurityTokenParameters p, out SecurityTokenResolver resolver)
		{
			resolver = null;
			// This check might be almost extra, though it is
			// needed to check correct signing token existence.
			//
			// Not sure if it is limited to this condition, but
			// Ssl parameters do not support token provider and
			// still do not fail. X509 parameters do fail.
			if (!RecipientParameters.InternalSupportsServerAuthentication)
				return null;

			SecurityTokenRequirement r = CreateRequirement ();
			r.Properties [ReqType.MessageDirectionProperty] = MessageDirection.Input;
			InitializeRequirement (p, r);
			return SecurityTokenManager.CreateSecurityTokenAuthenticator (r, out resolver);
		}
	}
}
