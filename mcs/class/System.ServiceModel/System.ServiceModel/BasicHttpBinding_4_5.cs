//
// BasicHttpBinding_4_5.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
// Copyright 2011-2012 Xamarin Inc (http://www.xamarin.com).
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
using System.Net;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using System.ServiceModel.Configuration;

namespace System.ServiceModel
{
	public class BasicHttpBinding : HttpBindingBase,
		IBindingRuntimePreferences
	{
		WSMessageEncoding message_encoding = WSMessageEncoding.Text;
		BasicHttpSecurity security;

		public BasicHttpBinding ()
			: this (BasicHttpSecurityMode.None)
		{
		}
		
#if !MOBILE && !XAMMAC_4_5
		public BasicHttpBinding (string configurationName)
			: this ()
		{
			BindingsSection bindingsSection = ConfigUtil.BindingsSection;
			BasicHttpBindingElement el = 
				bindingsSection.BasicHttpBinding.Bindings [configurationName];

			el.ApplyConfiguration (this);
		}
#endif

		public BasicHttpBinding (
			BasicHttpSecurityMode securityMode)
		{
			security = new BasicHttpSecurity (securityMode);
		}

		public WSMessageEncoding MessageEncoding {
			get { return message_encoding; }
			set { message_encoding = value; }
		}

		public override string Scheme {
			get {
				switch (Security.Mode) {
				case BasicHttpSecurityMode.Transport:
				case BasicHttpSecurityMode.TransportWithMessageCredential:
					return Uri.UriSchemeHttps;
				default:
					return Uri.UriSchemeHttp;
				}
			}
		}

		public BasicHttpSecurity Security {
			get { return security; }
			set { security = value; }
		}

		public override BindingElementCollection
			CreateBindingElements ()
		{
			var list = new List<BindingElement> ();
			
			var security = CreateSecurityBindingElement ();
			if (security != null)
				list.Add (security);

			list.Add (BuildMessageEncodingBindingElement ());
			list.Add (GetTransport ());

			return new BindingElementCollection (list.ToArray ());
		}
		
		SecurityBindingElement CreateSecurityBindingElement () 
		{
			SecurityBindingElement element;
			switch (Security.Mode) {
			case BasicHttpSecurityMode.Message:
#if MOBILE || XAMMAC_4_5
				throw new NotImplementedException ();
#else
				if (Security.Message.ClientCredentialType != BasicHttpMessageCredentialType.Certificate)
					throw new InvalidOperationException ("When Message security is enabled in a BasicHttpBinding, the message security credential type must be BasicHttpMessageCredentialType.Certificate.");
				element = SecurityBindingElement.CreateMutualCertificateBindingElement (
				    MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10);
				break;
#endif

			case BasicHttpSecurityMode.TransportWithMessageCredential:
#if MOBILE || XAMMAC_4_5
				throw new NotImplementedException ();
#else
				if (Security.Message.ClientCredentialType != BasicHttpMessageCredentialType.Certificate)
					// FIXME: pass proper security token parameters.
					element = SecurityBindingElement.CreateCertificateOverTransportBindingElement ();
				else
					element = new AsymmetricSecurityBindingElement ();
				break;
#endif
			default: 
				return null;
			}
			
#if !MOBILE && !XAMMAC_4_5
			element.SetKeyDerivation (false);
			element.SecurityHeaderLayout = SecurityHeaderLayout.Lax;
#endif
			return element;
		}

		MessageEncodingBindingElement BuildMessageEncodingBindingElement ()
		{
			if (MessageEncoding == WSMessageEncoding.Text) {
				TextMessageEncodingBindingElement tm = new TextMessageEncodingBindingElement (
					MessageVersion.CreateVersion (EnvelopeVersion, AddressingVersion.None), TextEncoding);
				ReaderQuotas.CopyTo (tm.ReaderQuotas);
				return tm;
			} else {
#if MOBILE || XAMMAC_4_5
				throw new NotImplementedException ();
#else
				return new MtomMessageEncodingBindingElement (
					MessageVersion.CreateVersion (EnvelopeVersion, AddressingVersion.None), TextEncoding);
#endif
			}
		}

		TransportBindingElement GetTransport ()
		{
			HttpTransportBindingElement h;
			switch (Security.Mode) {
			case BasicHttpSecurityMode.Transport:
			case BasicHttpSecurityMode.TransportWithMessageCredential:
				h = new HttpsTransportBindingElement ();
				break;
			default:
				h = new HttpTransportBindingElement ();
				break;
			}

			h.AllowCookies = AllowCookies;
			h.BypassProxyOnLocal = BypassProxyOnLocal;
			h.HostNameComparisonMode = HostNameComparisonMode;
			h.MaxBufferPoolSize = MaxBufferPoolSize;
			h.MaxBufferSize = MaxBufferSize;
			h.MaxReceivedMessageSize = MaxReceivedMessageSize;
			h.ProxyAddress = ProxyAddress;
			h.UseDefaultWebProxy = UseDefaultWebProxy;
			h.TransferMode = TransferMode;
			h.ExtendedProtectionPolicy = Security.Transport.ExtendedProtectionPolicy;

			switch (Security.Transport.ClientCredentialType) {
			case HttpClientCredentialType.Basic:
				h.AuthenticationScheme = AuthenticationSchemes.Basic;
				break;
			case HttpClientCredentialType.Ntlm:
				h.AuthenticationScheme = AuthenticationSchemes.Ntlm;
				break;
			case HttpClientCredentialType.Windows:
				h.AuthenticationScheme = AuthenticationSchemes.Negotiate;
				break;
			case HttpClientCredentialType.Digest:
				h.AuthenticationScheme = AuthenticationSchemes.Digest;
				break;
			case HttpClientCredentialType.Certificate:
				switch (Security.Mode) {
				case BasicHttpSecurityMode.Transport:
					(h as HttpsTransportBindingElement).RequireClientCertificate = true;
					break;
				case BasicHttpSecurityMode.TransportCredentialOnly:
					throw new InvalidOperationException ("Certificate-based client authentication is not supported by 'TransportCredentialOnly' mode.");
				}
				break;
			}

			return h;
		}
	}
}
