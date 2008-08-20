//
// WSDualHttpBinding.cs
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
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;

namespace System.ServiceModel
{
	[MonoTODO]
	public class WSDualHttpBinding : Binding, IBindingRuntimePreferences
	{
		bool bypass_proxy_on_local;
		HostNameComparisonMode host_name_comparison_mode;
		long max_buffer_pool_size;
		long max_recv_msg_size;
		WSDualHttpSecurity security;
		WSMessageEncoding message_encoding;
		Uri proxy_address;
		XmlDictionaryReaderQuotas reader_quotas;
		ReliableSession reliable_session;
		EnvelopeVersion env_version;
		Encoding text_encoding;
		bool transaction_flow;
		bool use_default_web_proxy;

		public WSDualHttpBinding ()
			: this (WSDualHttpSecurityMode.Message)
		{
		}

		protected WSDualHttpBinding (WSDualHttpSecurityMode securityMode)
		{
			security = new WSDualHttpSecurity (securityMode);
		}

		public Uri ClientBaseAddress {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public bool BypassProxyOnLocal {
			get { return bypass_proxy_on_local; }
			set { bypass_proxy_on_local = value; }
		}

		public HostNameComparisonMode HostNameComparisonMode {
			get { return host_name_comparison_mode; }
			set { host_name_comparison_mode = value; }
		}

		public long MaxBufferPoolSize {
			get { return max_buffer_pool_size; }
			set { max_buffer_pool_size = value; }
		}

		public long MaxReceivedMessageSize {
			get { return max_recv_msg_size; }
			set { max_recv_msg_size = value; }
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

		public ReliableSession ReliableSession {
			get { return reliable_session; }
		}

		public override string Scheme {
			get { return Uri.UriSchemeHttp; }
		}

		public WSDualHttpSecurity Security {
			get { return security; }
		}

		public EnvelopeVersion EnvelopeVersion {
			get { return env_version; }
		}

		public Encoding TextEncoding {
			get { return text_encoding; }
			set { text_encoding = value; }
		}

		public bool TransactionFlow {
			get { return transaction_flow; }
			set { transaction_flow = value; }
		}

		public bool UseDefaultWebProxy {
			get { return use_default_web_proxy; }
			set { use_default_web_proxy = value; }
		}

		public override BindingElementCollection
			CreateBindingElements ()
		{
			throw new NotImplementedException ();
		}

		// explicit interface implementations

		bool IBindingRuntimePreferences.ReceiveSynchronously {
			get { throw new NotImplementedException (); }
		}
	}
}
