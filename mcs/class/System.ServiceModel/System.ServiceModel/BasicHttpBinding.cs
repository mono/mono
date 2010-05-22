//
// BasicHttpBinding.cs
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
using System.Net;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using System.ServiceModel.Configuration;

namespace System.ServiceModel
{
	public class BasicHttpBinding : Binding,
		IBindingRuntimePreferences
	{
		bool allow_cookies, bypass_proxy_on_local;
		HostNameComparisonMode host_name_comparison_mode
			= HostNameComparisonMode.StrongWildcard;
		long max_buffer_pool_size = 0x80000;
		int max_buffer_size = 0x10000;
		long max_recv_message_size = 0x10000;
		WSMessageEncoding message_encoding
			= WSMessageEncoding.Text;
		Uri proxy_address;
		XmlDictionaryReaderQuotas reader_quotas
			= new XmlDictionaryReaderQuotas ();
		EnvelopeVersion env_version = EnvelopeVersion.Soap11;
		Encoding text_encoding = new UTF8Encoding ();
		TransferMode transfer_mode
			 = TransferMode.Buffered;
		bool use_default_web_proxy = true;
		BasicHttpSecurity security;

		public BasicHttpBinding ()
			: this (BasicHttpSecurityMode.None)
		{
		}

#if !NET_2_1
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

		public bool AllowCookies {
			get { return allow_cookies; }
			set { allow_cookies = value; }
		}

		public bool BypassProxyOnLocal {
			get { return bypass_proxy_on_local; }
			set { bypass_proxy_on_local = value; }
		}

#if NET_2_1
		public bool EnableHttpCookieContainer { get; set; }
#endif

		public HostNameComparisonMode HostNameComparisonMode {
			get { return host_name_comparison_mode; }
			set { host_name_comparison_mode = value; }
		}

		public long MaxBufferPoolSize {
			get { return max_buffer_pool_size; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ();
				max_buffer_pool_size = value;
			}
		}

		public int MaxBufferSize {
			get { return max_buffer_size; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ();
				max_buffer_size = value;
			}
		}

		public long MaxReceivedMessageSize {
			get { return max_recv_message_size; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ();
				max_recv_message_size = value;
			}
		}

		public WSMessageEncoding MessageEncoding {
			get { return message_encoding; }
			set { message_encoding = value; }
		}

		public Uri ProxyAddress {
			get { return proxy_address; }
			set { proxy_address = value; }
		}

		public XmlDictionaryReaderQuotas ReaderQuotas {
			get { return reader_quotas; }
			set { reader_quotas = value; }
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
		}

		public EnvelopeVersion EnvelopeVersion {
			get { return env_version; }
		}

		public Encoding TextEncoding {
			get { return text_encoding; }
			set { text_encoding = value; }
		}

		public TransferMode TransferMode {
			get { return transfer_mode; }
			set { transfer_mode = value; }
		}

		public bool UseDefaultWebProxy {
			get { return use_default_web_proxy; }
			set { use_default_web_proxy = value; }
		}

		public override BindingElementCollection
			CreateBindingElements ()
		{
			var list = new List<BindingElement> ();
			switch (Security.Mode) {
#if !NET_2_1
			case BasicHttpSecurityMode.Message:
				if (Security.Message.ClientCredentialType != BasicHttpMessageCredentialType.Certificate)
					throw new InvalidOperationException ("When Message security is enabled in a BasicHttpBinding, the message security credential type must be BasicHttpMessageCredentialType.Certificate.");
				goto case BasicHttpSecurityMode.TransportWithMessageCredential;
			case BasicHttpSecurityMode.TransportWithMessageCredential:
				SecurityBindingElement sec;
				if (Security.Message.ClientCredentialType != BasicHttpMessageCredentialType.Certificate)
					// FIXME: pass proper security token parameters.
					sec = SecurityBindingElement.CreateCertificateOverTransportBindingElement ();
				else
					sec = new AsymmetricSecurityBindingElement ();
				list.Add (sec);
				break;
#endif
			}

#if NET_2_1
			if (EnableHttpCookieContainer)
				list.Add (new HttpCookieContainerBindingElement ());
#endif

			list.Add (BuildMessageEncodingBindingElement ());
			list.Add (GetTransport ());

			return new BindingElementCollection (list.ToArray ());
		}

		MessageEncodingBindingElement BuildMessageEncodingBindingElement ()
		{
			if (MessageEncoding == WSMessageEncoding.Text) {
				TextMessageEncodingBindingElement tm = new TextMessageEncodingBindingElement (
					MessageVersion.CreateVersion (EnvelopeVersion, AddressingVersion.None), TextEncoding);
#if !NET_2_1
				ReaderQuotas.CopyTo (tm.ReaderQuotas);
#endif
				return tm;
			}
			else
#if NET_2_1
				throw new SystemException ("INTERNAL ERROR: should not happen");
#else
				return new MtomMessageEncodingBindingElement (
					MessageVersion.CreateVersion (EnvelopeVersion, AddressingVersion.None), TextEncoding);
#endif
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

#if !NET_2_1
			switch (Security.Mode) {
			case BasicHttpSecurityMode.Transport:
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
					var https = (HttpsTransportBindingElement) h;
					https.RequireClientCertificate = true;
					break;
				}
				break;
			case BasicHttpSecurityMode.TransportCredentialOnly:
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
					throw new InvalidOperationException ("Certificate-based client authentication is not supported by 'TransportCredentialOnly' mode.");
				}
				break;
			}
#endif

			return h;
		}

		// explicit interface implementations

		bool IBindingRuntimePreferences.ReceiveSynchronously {
			get { return false; }
		}
	}
}
