//
// SymmetricSecurityBindingElement.cs
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
using System.ServiceModel.Channels;
using System.ServiceModel.Channels.Security;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace System.ServiceModel.Channels
{
	public sealed class SymmetricSecurityBindingElement
		: SecurityBindingElement, IPolicyExportExtension
	{
		public SymmetricSecurityBindingElement ()
			: this ((SecurityTokenParameters) null)
		{
		}

		public SymmetricSecurityBindingElement (
			SecurityTokenParameters protectionTokenParameters)
		{
			ProtectionTokenParameters = protectionTokenParameters;
		}

		private SymmetricSecurityBindingElement (
			SymmetricSecurityBindingElement other)
			: base (other)
		{
			msg_protection_order = other.msg_protection_order;
			require_sig_confirm = other.require_sig_confirm;
			if (other.protection_token_params != null)
				protection_token_params = other.protection_token_params.Clone ();
		}

		MessageProtectionOrder msg_protection_order =
			MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature;
		SecurityTokenParameters protection_token_params;
		bool require_sig_confirm;
		// make sure that they are also cloned.

		[MonoTODO]
		public MessageProtectionOrder MessageProtectionOrder {
			get { return msg_protection_order; }
			set { msg_protection_order = value; }
		}

		public SecurityTokenParameters ProtectionTokenParameters {
			get { return protection_token_params; }
			set { protection_token_params = value; }
		}

		[MonoTODO]
		public bool RequireSignatureConfirmation {
			get { return require_sig_confirm; }
			set { require_sig_confirm = value; }
		}

		public override void SetKeyDerivation (bool requireDerivedKeys)
		{
			base.SetKeyDerivation (requireDerivedKeys);
			if (ProtectionTokenParameters != null)
				ProtectionTokenParameters.RequireDerivedKeys = requireDerivedKeys;
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
			if (ProtectionTokenParameters == null)
				throw new InvalidOperationException ("Protection token parameters must be set before building channel factory.");

			SetIssuerBindingContextIfRequired (ProtectionTokenParameters, context);

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
			if (ProtectionTokenParameters == null)
				throw new InvalidOperationException ("Protection token parameters must be set before building channel factory.");

			SetIssuerBindingContextIfRequired (ProtectionTokenParameters, context);

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
			return new SymmetricSecurityBindingElement (this);
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
			return context.GetInnerProperty<T> ();
		}

		SymmetricSecurityCapabilities GetCapabilities ()
		{
			return new SymmetricSecurityCapabilities (this);
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
