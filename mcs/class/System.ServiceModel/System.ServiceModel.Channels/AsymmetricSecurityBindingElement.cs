//
// AsymmetricSecurityBindingElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Net.Security;
using System.IdentityModel.Selectors;
using System.ServiceModel.Channels;
using System.ServiceModel.Channels.Security;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel.Channels
{
	public sealed class AsymmetricSecurityBindingElement
		: SecurityBindingElement, IPolicyExportExtension
	{
		public AsymmetricSecurityBindingElement ()
			: this (null, null)
		{
		}

		public AsymmetricSecurityBindingElement (
			SecurityTokenParameters recipientTokenParameters)
			: this (recipientTokenParameters, null)
		{
		}

		public AsymmetricSecurityBindingElement (
			SecurityTokenParameters recipientTokenParameters,
			SecurityTokenParameters initiatorTokenParameters)
		{
			this.initiator_token_params = initiatorTokenParameters;
			this.recipient_token_params = recipientTokenParameters;
			msg_protection_order = MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature;
		}

		private AsymmetricSecurityBindingElement (
			AsymmetricSecurityBindingElement other)
			: base (other)
		{
			msg_protection_order = other.msg_protection_order;
			require_sig_confirm = other.require_sig_confirm;
			if (other.initiator_token_params != null)
				initiator_token_params = other.initiator_token_params.Clone ();
			if (other.recipient_token_params != null)
				recipient_token_params = other.recipient_token_params.Clone ();
			allow_serialized_sign = other.allow_serialized_sign;
		}

		MessageProtectionOrder msg_protection_order;
		SecurityTokenParameters initiator_token_params,
			recipient_token_params;
		bool allow_serialized_sign, require_sig_confirm;

		public bool AllowSerializedSigningTokenOnReply {
			get { return allow_serialized_sign; }
			set { allow_serialized_sign = value; }
		}

		public MessageProtectionOrder MessageProtectionOrder {
			get { return msg_protection_order; }
			set { msg_protection_order = value; }
		}

		public SecurityTokenParameters InitiatorTokenParameters {
			get { return initiator_token_params; }
			set { initiator_token_params = value; }
		}

		public SecurityTokenParameters RecipientTokenParameters {
			get { return recipient_token_params; }
			set { recipient_token_params = value; }
		}

		public bool RequireSignatureConfirmation {
			get { return require_sig_confirm; }
			set { require_sig_confirm = value; }
		}

		public override void SetKeyDerivation (bool requireDerivedKeys)
		{
			base.SetKeyDerivation (requireDerivedKeys);
			if (InitiatorTokenParameters != null)
				InitiatorTokenParameters.RequireDerivedKeys = requireDerivedKeys;
			if (RecipientTokenParameters != null)
				RecipientTokenParameters.RequireDerivedKeys = requireDerivedKeys;
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}

		[MonoTODO]
		protected override IChannelFactory<TChannel>
			BuildChannelFactoryCore<TChannel> (
			BindingContext context)
		{
			if (InitiatorTokenParameters == null)
				throw new InvalidOperationException ("InitiatorTokenParameters must be set before building channel factory.");
			if (RecipientTokenParameters == null)
				throw new InvalidOperationException ("RecipientTokenParameters must be set before building channel factory.");

			SetIssuerBindingContextIfRequired (InitiatorTokenParameters, context);
			SetIssuerBindingContextIfRequired (RecipientTokenParameters, context);

			ClientCredentials cred = context.BindingParameters.Find<ClientCredentials> ();
			if (cred == null)
				// it happens when there is no ChannelFactory<T>.
				cred = new ClientCredentials ();
			SecurityTokenManager manager = cred.CreateSecurityTokenManager ();
			ChannelProtectionRequirements requirements =
				context.BindingParameters.Find<ChannelProtectionRequirements> ();

			return new SecurityChannelFactory<TChannel> (
				context.BuildInnerChannelFactory<TChannel> (), new InitiatorMessageSecurityBindingSupport (GetCapabilities (), manager, requirements));
		}

		[MonoTODO]
		protected override IChannelListener<TChannel>
			BuildChannelListenerCore<TChannel> (
			BindingContext context)
		{
			if (InitiatorTokenParameters == null)
				throw new InvalidOperationException ("InitiatorTokenParameters must be set before building channel factory.");
			if (RecipientTokenParameters == null)
				throw new InvalidOperationException ("RecipientTokenParameters must be set before building channel factory.");

			SetIssuerBindingContextIfRequired (InitiatorTokenParameters, context);
			SetIssuerBindingContextIfRequired (RecipientTokenParameters, context);

			ServiceCredentials cred = context.BindingParameters.Find<ServiceCredentials> ();
			if (cred == null)
				// it happens when there is no ChannelFactory<T>.
				cred = new ServiceCredentials ();
			ServiceCredentialsSecurityTokenManager manager = (ServiceCredentialsSecurityTokenManager) cred.CreateSecurityTokenManager ();
			ChannelProtectionRequirements requirements =
				context.BindingParameters.Find<ChannelProtectionRequirements> ();

			return new SecurityChannelListener<TChannel> (
				context.BuildInnerChannelListener<TChannel> (), new RecipientMessageSecurityBindingSupport (GetCapabilities (), manager, requirements));
		}

		public override BindingElement Clone ()
		{
			return new AsymmetricSecurityBindingElement (this);
		}

		[MonoTODO]
		public override T GetProperty<T> (BindingContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			if (typeof (T) == typeof (ISecurityCapabilities))
				return (T) (object) GetCapabilities ();
			if (typeof (T) == typeof (IdentityVerifier))
				throw new NotImplementedException ();
			return base.GetProperty<T> (context);
		}

		AsymmetricSecurityCapabilities GetCapabilities ()
		{
			return new AsymmetricSecurityCapabilities (this);
		}

		#region explicit interface implementations

		[MonoTODO]
		void IPolicyExportExtension.ExportPolicy (
			MetadataExporter exporter,
			PolicyConversionContext policyContext)
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}
