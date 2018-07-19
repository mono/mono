//
// WSHttpBinding.cs
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
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel
{
	public class WSHttpBinding : WSHttpBindingBase
	{
		WSHttpSecurity security;
		bool allow_cookies;

		public WSHttpBinding ()
			: this (SecurityMode.Message)
		{
		}

		public WSHttpBinding (SecurityMode securityMode)
			: this (securityMode, false)
		{
		}

		public WSHttpBinding (SecurityMode securityMode,
			bool reliableSessionEnabled)
			: base (reliableSessionEnabled)
		{
			security = new WSHttpSecurity (securityMode);
		}

		[MonoTODO]
		public WSHttpBinding (string configName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool AllowCookies {
			get { return allow_cookies; }
			set { allow_cookies = value; }
		}

		[MonoTODO]
		public WSHttpSecurity Security {
			get { return security; }
		}

		[MonoTODO]
        	public override BindingElementCollection CreateBindingElements ()
		{
			BindingElementCollection bc = base.CreateBindingElements ();
			// message security element is returned only when
			// it is enabled (while CreateMessageSecurity() still
			// returns non-null instance).
			switch (Security.Mode) {
			case SecurityMode.None:
			case SecurityMode.Transport:
				bc.RemoveAll<SecurityBindingElement> ();
				break;
			}
			return bc;
		}

		[MonoTODO]
		protected override SecurityBindingElement CreateMessageSecurity ()
		{
			if (Security.Mode == SecurityMode.Transport ||
			    Security.Mode == SecurityMode.None)
				return null;

			SymmetricSecurityBindingElement element =
				new SymmetricSecurityBindingElement ();

			element.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;
			element.RequireSignatureConfirmation = true;

			switch (Security.Message.ClientCredentialType) {
			case MessageCredentialType.Certificate:
				X509SecurityTokenParameters p =
					new X509SecurityTokenParameters (X509KeyIdentifierClauseType.Thumbprint);
				p.RequireDerivedKeys = false;
				element.EndpointSupportingTokenParameters.Endorsing.Add (p);
				goto default;
			case MessageCredentialType.IssuedToken:
				IssuedSecurityTokenParameters istp =
					new IssuedSecurityTokenParameters ();
				// FIXME: issuer binding must be secure.
				istp.IssuerBinding = new CustomBinding (
					new TextMessageEncodingBindingElement (),
					GetTransport ());
				element.EndpointSupportingTokenParameters.Endorsing.Add (istp);
				goto default;
			case MessageCredentialType.UserName:
				element.EndpointSupportingTokenParameters.SignedEncrypted.Add (
					new UserNameSecurityTokenParameters ());
				element.RequireSignatureConfirmation = false;
				goto default;
			case MessageCredentialType.Windows:
				if (Security.Message.NegotiateServiceCredential) {
					// No SSPI on Linux though...
					element.ProtectionTokenParameters =
						// FIXME: fill proper parameters
						new SspiSecurityTokenParameters ();
				} else {
					// and no Kerberos ...
					element.ProtectionTokenParameters =
						new KerberosSecurityTokenParameters ();
				}
				break;
			default: // including .None
				if (Security.Message.NegotiateServiceCredential) {
					element.ProtectionTokenParameters =
						// FIXME: fill proper parameters
						new SslSecurityTokenParameters (false, true);
				} else {
					element.ProtectionTokenParameters =
						new X509SecurityTokenParameters (X509KeyIdentifierClauseType.Thumbprint, SecurityTokenInclusionMode.Never);
					element.ProtectionTokenParameters.RequireDerivedKeys = true;
				}
				break;
			}

			if (!Security.Message.EstablishSecurityContext)
				return element;

			// SecureConversation enabled

			ChannelProtectionRequirements reqs =
				new ChannelProtectionRequirements ();
			// FIXME: fill the reqs

			return SecurityBindingElement.CreateSecureConversationBindingElement (
				// FIXME: requireCancellation
				element, true, reqs);
		}

		[MonoTODO]
		protected override TransportBindingElement GetTransport ()
		{
			switch (Security.Mode) {
			case SecurityMode.Transport:
			case SecurityMode.TransportWithMessageCredential:
				return new HttpsTransportBindingElement ();
			default:
				return new HttpTransportBindingElement ();
			}
		}
	}
}
