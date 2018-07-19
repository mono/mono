//
// SecureConversationSecurityTokenParameters.cs
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
#if !MOBILE && !XAMMAC_4_5
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
#endif
using System.ServiceModel.Channels;
using System.ServiceModel.Security;

#if !MOBILE && !XAMMAC_4_5
using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;
#endif

namespace System.ServiceModel.Security.Tokens
{
	public class SecureConversationSecurityTokenParameters : SecurityTokenParameters
	{
#if !MOBILE && !XAMMAC_4_5
		static readonly ChannelProtectionRequirements default_channel_protection_requirements;
#endif
		static readonly BindingContext dummy_context;

		static SecureConversationSecurityTokenParameters ()
		{
#if !MOBILE && !XAMMAC_4_5
			ChannelProtectionRequirements r =
				new ChannelProtectionRequirements ();
			r.IncomingSignatureParts.ChannelParts.IsBodyIncluded = true;
			r.OutgoingSignatureParts.ChannelParts.IsBodyIncluded = true;
			r.IncomingEncryptionParts.ChannelParts.IsBodyIncluded = true;
			r.OutgoingEncryptionParts.ChannelParts.IsBodyIncluded = true;
			r.MakeReadOnly ();
			default_channel_protection_requirements = r;
#endif

			dummy_context = new BindingContext (
				new CustomBinding (),
				new BindingParameterCollection ());
		}

		SecurityBindingElement element;
#if !MOBILE && !XAMMAC_4_5
		ChannelProtectionRequirements requirements;
#endif
		bool cancellable;

		public SecureConversationSecurityTokenParameters ()
			: this ((SecurityBindingElement) null)
		{
		}

		public SecureConversationSecurityTokenParameters (
			SecurityBindingElement bootstrapSecurityBindingElement)
			: this (bootstrapSecurityBindingElement, true)
		{
		}

		public SecureConversationSecurityTokenParameters (
			SecurityBindingElement bootstrapSecurityBindingElement,
			bool requireCancellation)
			: this (bootstrapSecurityBindingElement, requireCancellation, null)
		{
		}

#if !MOBILE && !XAMMAC_4_5
		public SecureConversationSecurityTokenParameters (
			SecurityBindingElement bootstrapSecurityBindingElement,
			bool requireCancellation,
			ChannelProtectionRequirements bootstrapProtectionRequirements)
		{
			this.element = bootstrapSecurityBindingElement;
			this.cancellable = requireCancellation;
			if (bootstrapProtectionRequirements == null)
				this.requirements = new ChannelProtectionRequirements (default_channel_protection_requirements);
			else
				this.requirements = new ChannelProtectionRequirements (bootstrapProtectionRequirements);
		}
#else
		internal SecureConversationSecurityTokenParameters (
			SecurityBindingElement element,
			bool requireCancellation,
			object dummy)
		{
			this.element = element;
			this.cancellable = requireCancellation;
		}
#endif

		protected SecureConversationSecurityTokenParameters (SecureConversationSecurityTokenParameters other)
			: base (other)
		{
			this.element = (SecurityBindingElement) other.element.Clone ();
			this.cancellable = other.cancellable;
#if !MOBILE && !XAMMAC_4_5
			this.requirements = new ChannelProtectionRequirements (default_channel_protection_requirements);
#endif
		}

		public bool RequireCancellation {
			get { return cancellable; }
			set { cancellable = value; }
		}

		public SecurityBindingElement BootstrapSecurityBindingElement {
			get { return element; }
			set { element = value; }
		}

#if !MOBILE && !XAMMAC_4_5
		public ChannelProtectionRequirements BootstrapProtectionRequirements {
			get { return requirements; }
		}
#endif

		// SecurityTokenParameters

		protected override bool HasAsymmetricKey {
			get { return false; }
		}

		protected override bool SupportsClientAuthentication {
			get { return element.GetProperty<ISecurityCapabilities> (dummy_context).SupportsClientAuthentication; }
		}

		protected override bool SupportsClientWindowsIdentity {
			get { return element.GetProperty<ISecurityCapabilities> (dummy_context).SupportsClientWindowsIdentity; }
		}

		protected override bool SupportsServerAuthentication {
			get { return element.GetProperty<ISecurityCapabilities> (dummy_context).SupportsServerAuthentication; }
		}

		protected override SecurityTokenParameters CloneCore ()
		{
			return new SecureConversationSecurityTokenParameters (this);
		}

#if !MOBILE && !XAMMAC_4_5
		[MonoTODO]
		protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause (
			SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected internal override void InitializeSecurityTokenRequirement (SecurityTokenRequirement requirement)
		{
			// .NET somehow causes NRE. dunno why.
			requirement.TokenType = ServiceModelSecurityTokenTypes.SecureConversation;
			requirement.RequireCryptographicToken = true;
			requirement.Properties [ReqType.SupportSecurityContextCancellationProperty] = RequireCancellation;
			requirement.Properties [ReqType.SecureConversationSecurityBindingElementProperty] =
				BootstrapSecurityBindingElement;
			requirement.Properties [ReqType.IssuedSecurityTokenParametersProperty] = this.Clone ();
			requirement.KeyType = SecurityKeyType.SymmetricKey;
		}
#endif

		public override string ToString ()
		{
			return base.ToString ();
		}
	}
}
