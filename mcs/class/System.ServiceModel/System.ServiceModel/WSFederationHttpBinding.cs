//
// WSFederationHttpBinding.cs
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
using System.Collections.Generic;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel
{
	public class WSFederationHttpBinding : WSHttpBindingBase
	{
		WSFederationHttpSecurity security;
		Uri privacy_notice_at;
		int privacy_notice_ver;
		bool allow_cookies;

		public WSFederationHttpBinding ()
			: this (WSFederationHttpSecurityMode.Message)
		{
		}

		public WSFederationHttpBinding (
			WSFederationHttpSecurityMode securityMode)
			: this (securityMode, true)
		{
		}

		public WSFederationHttpBinding (
			WSFederationHttpSecurityMode securityMode,
			bool reliableSessionEnabled)
		{
			security = new WSFederationHttpSecurity (securityMode);
		}

		[MonoTODO]
		public WSFederationHttpBinding (string configurationName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool AllowCookies {
			get { return allow_cookies; }
			set { allow_cookies = value; }
		}

		[MonoTODO]
		public Uri PrivacyNoticeAt {
			get { return privacy_notice_at; }
			set { privacy_notice_at = value; }
		}

		[MonoTODO]
		public int PrivacyNoticeVersion {
			get { return privacy_notice_ver; }
			set { privacy_notice_ver = value; }
		}

		[MonoTODO]
		public WSFederationHttpSecurity Security {
			get { return security; }
		}

		[MonoTODO]
        	public override BindingElementCollection CreateBindingElements ()
		{
			return base.CreateBindingElements ();
		}

		[MonoTODO]
		protected override SecurityBindingElement CreateMessageSecurity ()
		{
			SymmetricSecurityBindingElement element =
				new SymmetricSecurityBindingElement ();

			element.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;

//			if (!Security.Message.EstablishSecurityContext)
//				element.SetKeyDerivation (false);

			IssuedSecurityTokenParameters istp =
				new IssuedSecurityTokenParameters ();
			// FIXME: issuer binding must be secure.
			istp.IssuerBinding = new CustomBinding (
				new TextMessageEncodingBindingElement (),
				GetTransport ());
			element.EndpointSupportingTokenParameters.Endorsing.Add (istp);

			if (Security.Message.NegotiateServiceCredential) {
				element.ProtectionTokenParameters =
					// FIXME: fill proper parameters
					new SslSecurityTokenParameters (false, true);
			} else {
				element.ProtectionTokenParameters =
					new X509SecurityTokenParameters ();
			}

//			if (!Security.Message.EstablishSecurityContext)
//				return element;

			// SecureConversation enabled

			ChannelProtectionRequirements reqs =
				new ChannelProtectionRequirements ();
			// FIXME: fill the reqs

			// FIXME: for TransportWithMessageCredential mode,
			// return TransportSecurityBindingElement.

			return SecurityBindingElement.CreateSecureConversationBindingElement (
				// FIXME: requireCancellation
				element, true, reqs);
		}

		[MonoTODO]
		protected override TransportBindingElement GetTransport ()
		{
			switch (Security.Mode) {
			case WSFederationHttpSecurityMode.TransportWithMessageCredential:
				return new HttpsTransportBindingElement ();
			default:
				return new HttpTransportBindingElement ();
			}
		}
	}
}
