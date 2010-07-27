//
// WSDualHttpBindingElement.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.MsmqIntegration;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Security;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Configuration
{
	[MonoTODO]
	public partial class WSDualHttpBindingElement
		 : StandardBindingElement,  IBindingConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty bypass_proxy_on_local;
		static ConfigurationProperty client_base_address;
		static ConfigurationProperty host_name_comparison_mode;
		static ConfigurationProperty max_buffer_pool_size;
		static ConfigurationProperty max_received_message_size;
		static ConfigurationProperty message_encoding;
		static ConfigurationProperty proxy_address;
		static ConfigurationProperty reader_quotas;
		static ConfigurationProperty reliable_session;
		static ConfigurationProperty security;
		static ConfigurationProperty text_encoding;
		static ConfigurationProperty transaction_flow;
		static ConfigurationProperty use_default_web_proxy;

		static WSDualHttpBindingElement ()
		{
			properties = new ConfigurationPropertyCollection ();

			bypass_proxy_on_local = new ConfigurationProperty ("bypassProxyOnLocal",
				typeof (bool), "false", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			client_base_address = new ConfigurationProperty ("clientBaseAddress",
				typeof (Uri), null, new UriTypeConverter (), null,
				ConfigurationPropertyOptions.None);

			host_name_comparison_mode = new ConfigurationProperty ("hostNameComparisonMode",
				typeof (HostNameComparisonMode), "StrongWildcard", null/* FIXME: get converter for HostNameComparisonMode*/, null,
				ConfigurationPropertyOptions.None);

			max_buffer_pool_size = new ConfigurationProperty ("maxBufferPoolSize",
				typeof (long), "524288", null/* FIXME: get converter for long*/, null,
				ConfigurationPropertyOptions.None);

			max_received_message_size = new ConfigurationProperty ("maxReceivedMessageSize",
				typeof (long), "65536", null/* FIXME: get converter for long*/, null,
				ConfigurationPropertyOptions.None);

			message_encoding = new ConfigurationProperty ("messageEncoding",
				typeof (WSMessageEncoding), "Text", null/* FIXME: get converter for WSMessageEncoding*/, null,
				ConfigurationPropertyOptions.None);

			proxy_address = new ConfigurationProperty ("proxyAddress",
				typeof (Uri), null, new UriTypeConverter (), null,
				ConfigurationPropertyOptions.None);

			reader_quotas = new ConfigurationProperty ("readerQuotas",
				typeof (XmlDictionaryReaderQuotasElement), null, null/* FIXME: get converter for XmlDictionaryReaderQuotasElement*/, null,
				ConfigurationPropertyOptions.None);

			reliable_session = new ConfigurationProperty ("reliableSession",
				typeof (StandardBindingReliableSessionElement), null, null/* FIXME: get converter for StandardBindingReliableSessionElement*/, null,
				ConfigurationPropertyOptions.None);

			security = new ConfigurationProperty ("security",
				typeof (WSDualHttpSecurityElement), null, null/* FIXME: get converter for WSDualHttpSecurityElement*/, null,
				ConfigurationPropertyOptions.None);

			text_encoding = new ConfigurationProperty ("textEncoding",
				typeof (Encoding), "utf-8", null/* FIXME: get converter for Encoding*/, null,
				ConfigurationPropertyOptions.None);

			transaction_flow = new ConfigurationProperty ("transactionFlow",
				typeof (bool), "false", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			use_default_web_proxy = new ConfigurationProperty ("useDefaultWebProxy",
				typeof (bool), "true", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			properties.Add (bypass_proxy_on_local);
			properties.Add (client_base_address);
			properties.Add (host_name_comparison_mode);
			properties.Add (max_buffer_pool_size);
			properties.Add (max_received_message_size);
			properties.Add (message_encoding);
			properties.Add (proxy_address);
			properties.Add (reader_quotas);
			properties.Add (reliable_session);
			properties.Add (security);
			properties.Add (text_encoding);
			properties.Add (transaction_flow);
			properties.Add (use_default_web_proxy);
		}

		public WSDualHttpBindingElement ()
		{
		}


		// Properties

		protected override Type BindingElementType {
			get { return typeof (WSDualHttpBindingElement); }
		}

		[ConfigurationProperty ("bypassProxyOnLocal",
			DefaultValue = false,
			 Options = ConfigurationPropertyOptions.None)]
		public bool BypassProxyOnLocal {
			get { return (bool) base [bypass_proxy_on_local]; }
			set { base [bypass_proxy_on_local] = value; }
		}

		[ConfigurationProperty ("clientBaseAddress",
			 DefaultValue = null,
			 Options = ConfigurationPropertyOptions.None)]
		public Uri ClientBaseAddress {
			get { return (Uri) base [client_base_address]; }
			set { base [client_base_address] = value; }
		}

		[ConfigurationProperty ("hostNameComparisonMode",
			 DefaultValue = "StrongWildcard",
			 Options = ConfigurationPropertyOptions.None)]
		public HostNameComparisonMode HostNameComparisonMode {
			get { return (HostNameComparisonMode) base [host_name_comparison_mode]; }
			set { base [host_name_comparison_mode] = value; }
		}

		[LongValidator ( MinValue = 0,
			 MaxValue = 9223372036854775807,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxBufferPoolSize",
			 DefaultValue = "524288",
			 Options = ConfigurationPropertyOptions.None)]
		public long MaxBufferPoolSize {
			get { return (long) base [max_buffer_pool_size]; }
			set { base [max_buffer_pool_size] = value; }
		}

		[LongValidator ( MinValue = 1,
			 MaxValue = 9223372036854775807,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxReceivedMessageSize",
			 DefaultValue = "65536",
			 Options = ConfigurationPropertyOptions.None)]
		public long MaxReceivedMessageSize {
			get { return (long) base [max_received_message_size]; }
			set { base [max_received_message_size] = value; }
		}

		[ConfigurationProperty ("messageEncoding",
			 DefaultValue = "Text",
			 Options = ConfigurationPropertyOptions.None)]
		public WSMessageEncoding MessageEncoding {
			get { return (WSMessageEncoding) base [message_encoding]; }
			set { base [message_encoding] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[ConfigurationProperty ("proxyAddress",
			 DefaultValue = null,
			 Options = ConfigurationPropertyOptions.None)]
		public Uri ProxyAddress {
			get { return (Uri) base [proxy_address]; }
			set { base [proxy_address] = value; }
		}

		[ConfigurationProperty ("readerQuotas",
			 Options = ConfigurationPropertyOptions.None)]
		public XmlDictionaryReaderQuotasElement ReaderQuotas {
			get { return (XmlDictionaryReaderQuotasElement) base [reader_quotas]; }
		}

		[ConfigurationProperty ("reliableSession",
			 Options = ConfigurationPropertyOptions.None)]
		public StandardBindingReliableSessionElement ReliableSession {
			get { return (StandardBindingReliableSessionElement) base [reliable_session]; }
		}

		[ConfigurationProperty ("security",
			 Options = ConfigurationPropertyOptions.None)]
		public WSDualHttpSecurityElement Security {
			get { return (WSDualHttpSecurityElement) base [security]; }
		}

		[ConfigurationProperty ("textEncoding",
			 DefaultValue = "utf-8",
			 Options = ConfigurationPropertyOptions.None)]
		[TypeConverter (typeof(EncodingConverter))]
		public Encoding TextEncoding {
			get { return (Encoding) base [text_encoding]; }
			set { base [text_encoding] = value; }
		}

		[ConfigurationProperty ("transactionFlow",
			DefaultValue = false,
			 Options = ConfigurationPropertyOptions.None)]
		public bool TransactionFlow {
			get { return (bool) base [transaction_flow]; }
			set { base [transaction_flow] = value; }
		}

		[ConfigurationProperty ("useDefaultWebProxy",
			DefaultValue = true,
			 Options = ConfigurationPropertyOptions.None)]
		public bool UseDefaultWebProxy {
			get { return (bool) base [use_default_web_proxy]; }
			set { base [use_default_web_proxy] = value; }
		}



		protected override void OnApplyConfiguration (Binding binding) {
			throw new NotImplementedException ();
		}
	}

}
