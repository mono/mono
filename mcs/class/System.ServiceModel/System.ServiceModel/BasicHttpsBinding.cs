//
// BasicHttpsBinding.cs
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
	public class BasicHttpsBinding : HttpBindingBase,
		IBindingRuntimePreferences
	{
		WSMessageEncoding message_encoding = WSMessageEncoding.Text;
		BasicHttpsSecurity security;

		public BasicHttpsBinding ()
			: this (BasicHttpsSecurityMode.Transport)
		{
		}
		
#if !NET_2_1
		public BasicHttpsBinding (string configurationName)
			: this ()
		{
			BindingsSection bindingsSection = ConfigUtil.BindingsSection;
			BasicHttpsBindingElement el = 
				bindingsSection.BasicHttpsBinding.Bindings [configurationName];

			el.ApplyConfiguration (this);
		}
#endif

		public BasicHttpsBinding (
			BasicHttpsSecurityMode securityMode)
		{
			security = new BasicHttpsSecurity (securityMode);
		}

		public WSMessageEncoding MessageEncoding {
			get { return message_encoding; }
			set { message_encoding = value; }
		}

		public override string Scheme {
			get { return Uri.UriSchemeHttps; }
		}

		public BasicHttpsSecurity Security {
			get { return security; }
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
			case BasicHttpsSecurityMode.TransportWithMessageCredential:
#if NET_2_1
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
			
#if !NET_2_1
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
#if NET_2_1
				throw new NotImplementedException ();
#else
				return new MtomMessageEncodingBindingElement (
					MessageVersion.CreateVersion (EnvelopeVersion, AddressingVersion.None), TextEncoding);
#endif
			}
		}

		TransportBindingElement GetTransport ()
		{
			HttpsTransportBindingElement h = new HttpsTransportBindingElement ();

			h.AllowCookies = AllowCookies;
			h.BypassProxyOnLocal = BypassProxyOnLocal;
			h.HostNameComparisonMode = HostNameComparisonMode;
			h.MaxBufferPoolSize = MaxBufferPoolSize;
			h.MaxBufferSize = MaxBufferSize;
			h.MaxReceivedMessageSize = MaxReceivedMessageSize;
			h.ProxyAddress = ProxyAddress;
			h.UseDefaultWebProxy = UseDefaultWebProxy;
			h.TransferMode = TransferMode;
			
#if NET_4_0
			h.ExtendedProtectionPolicy = Security.Transport.ExtendedProtectionPolicy;
#endif

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
				h.RequireClientCertificate = true;
				break;
			}

			return h;
		}
	}
}
