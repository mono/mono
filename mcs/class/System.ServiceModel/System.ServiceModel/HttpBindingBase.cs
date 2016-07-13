//
// HttpBindingBase.cs
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
	public abstract class HttpBindingBase : Binding,
		IBindingRuntimePreferences
	{
		bool allow_cookies, bypass_proxy_on_local;
		HostNameComparisonMode host_name_comparison_mode
			= HostNameComparisonMode.StrongWildcard;
		long max_buffer_pool_size = 0x80000;
		int max_buffer_size = 0x10000;
		long max_recv_message_size = 0x10000;
		Uri proxy_address;
		XmlDictionaryReaderQuotas reader_quotas
			= new XmlDictionaryReaderQuotas ();
		EnvelopeVersion env_version = EnvelopeVersion.Soap11;
		Encoding text_encoding = default_text_encoding;
		static readonly Encoding default_text_encoding = new UTF8Encoding ();
		TransferMode transfer_mode
			 = TransferMode.Buffered;
		bool use_default_web_proxy = true;

		public bool AllowCookies {
			get { return allow_cookies; }
			set { allow_cookies = value; }
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

		public Uri ProxyAddress {
			get { return proxy_address; }
			set { proxy_address = value; }
		}

		public XmlDictionaryReaderQuotas ReaderQuotas {
			get { return reader_quotas; }
			set { reader_quotas = value; }
		}

		public override string Scheme {
			get;
		}

		public EnvelopeVersion EnvelopeVersion {
			get { return env_version; }
		}

		internal static Encoding DefaultTextEncoding {
			get { return default_text_encoding; }
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

		public override abstract BindingElementCollection CreateBindingElements ();

		// explicit interface implementations

		bool IBindingRuntimePreferences.ReceiveSynchronously {
			get { return false; }
		}
	}
}
