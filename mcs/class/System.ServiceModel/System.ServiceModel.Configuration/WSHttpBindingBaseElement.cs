//
// WSHttpBindingBaseElement.cs
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
	public abstract partial class WSHttpBindingBaseElement
		 : StandardBindingElement,  IBindingConfigurationElement
	{
		ConfigurationPropertyCollection _properties;

		protected WSHttpBindingBaseElement () {
		}

		protected WSHttpBindingBaseElement (string name)
			: base (name) {
		}

		// Properties

		[ConfigurationProperty ("bypassProxyOnLocal",
			DefaultValue = false,
			 Options = ConfigurationPropertyOptions.None)]
		public bool BypassProxyOnLocal {
			get { return (bool) this ["bypassProxyOnLocal"]; }
			set { this ["bypassProxyOnLocal"] = value; }
		}

		[ConfigurationProperty ("hostNameComparisonMode",
			 DefaultValue = "StrongWildcard",
			 Options = ConfigurationPropertyOptions.None)]
		public HostNameComparisonMode HostNameComparisonMode {
			get { return (HostNameComparisonMode) this ["hostNameComparisonMode"]; }
			set { this ["hostNameComparisonMode"] = value; }
		}

		[LongValidator ( MinValue = 0,
			 MaxValue = 9223372036854775807,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxBufferPoolSize",
			 DefaultValue = "524288",
			 Options = ConfigurationPropertyOptions.None)]
		public long MaxBufferPoolSize {
			get { return (long) this ["maxBufferPoolSize"]; }
			set { this ["maxBufferPoolSize"] = value; }
		}

		[LongValidator ( MinValue = 1,
			 MaxValue = 9223372036854775807,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxReceivedMessageSize",
			 DefaultValue = "65536",
			 Options = ConfigurationPropertyOptions.None)]
		public long MaxReceivedMessageSize {
			get { return (long) this ["maxReceivedMessageSize"]; }
			set { this ["maxReceivedMessageSize"] = value; }
		}

		[ConfigurationProperty ("messageEncoding",
			 DefaultValue = "Text",
			 Options = ConfigurationPropertyOptions.None)]
		public WSMessageEncoding MessageEncoding {
			get { return (WSMessageEncoding) this ["messageEncoding"]; }
			set { this ["messageEncoding"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = base.Properties;
					_properties.Add (new ConfigurationProperty ("bypassProxyOnLocal", typeof (bool), "false", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("hostNameComparisonMode", typeof (HostNameComparisonMode), "StrongWildcard", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxBufferPoolSize", typeof (long), "524288", null, new LongValidator (0, 9223372036854775807, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxReceivedMessageSize", typeof (long), "65536", null, new LongValidator (1, 9223372036854775807, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("messageEncoding", typeof (WSMessageEncoding), "Text", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("proxyAddress", typeof (Uri), null, new UriTypeConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("readerQuotas", typeof (XmlDictionaryReaderQuotasElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("reliableSession", typeof (StandardBindingOptionalReliableSessionElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("textEncoding", typeof (Encoding), "utf-8", EncodingConverter.Instance, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("transactionFlow", typeof (bool), "false", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("useDefaultWebProxy", typeof (bool), "true", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
				}
				return _properties;
			}
		}

		[ConfigurationProperty ("proxyAddress",
			 DefaultValue = null,
			 Options = ConfigurationPropertyOptions.None)]
		public Uri ProxyAddress {
			get { return (Uri) this ["proxyAddress"]; }
			set { this ["proxyAddress"] = value; }
		}

		[ConfigurationProperty ("readerQuotas",
			 Options = ConfigurationPropertyOptions.None)]
		public XmlDictionaryReaderQuotasElement ReaderQuotas {
			get { return (XmlDictionaryReaderQuotasElement) this ["readerQuotas"]; }
		}

		[ConfigurationProperty ("reliableSession",
			 Options = ConfigurationPropertyOptions.None)]
		public StandardBindingOptionalReliableSessionElement ReliableSession {
			get { return (StandardBindingOptionalReliableSessionElement) this ["reliableSession"]; }
		}

		[TypeConverter (typeof (EncodingConverter))]
		[ConfigurationProperty ("textEncoding",
			 DefaultValue = "utf-8",
			 Options = ConfigurationPropertyOptions.None)]
		public Encoding TextEncoding {
			get { return (Encoding) this ["textEncoding"]; }
			set { this ["textEncoding"] = value; }
		}

		[ConfigurationProperty ("transactionFlow",
			DefaultValue = false,
			 Options = ConfigurationPropertyOptions.None)]
		public bool TransactionFlow {
			get { return (bool) this ["transactionFlow"]; }
			set { this ["transactionFlow"] = value; }
		}

		[ConfigurationProperty ("useDefaultWebProxy",
			DefaultValue = true,
			 Options = ConfigurationPropertyOptions.None)]
		public bool UseDefaultWebProxy {
			get { return (bool) this ["useDefaultWebProxy"]; }
			set { this ["useDefaultWebProxy"] = value; }
		}

		protected override void OnApplyConfiguration (Binding binding)
		{
			var b = (WSHttpBindingBase) binding;
			b.BypassProxyOnLocal = BypassProxyOnLocal;
			b.HostNameComparisonMode = HostNameComparisonMode;
			b.MaxBufferPoolSize = MaxBufferPoolSize;
			b.MaxReceivedMessageSize = MaxReceivedMessageSize;
			b.MessageEncoding = MessageEncoding;
			b.ProxyAddress = ProxyAddress;
			b.ReaderQuotas = ReaderQuotas.Create ();
			ReliableSession.ApplyConfiguration (b.ReliableSession);
			b.TextEncoding = TextEncoding;
			b.TransactionFlow = TransactionFlow;
			b.UseDefaultWebProxy = UseDefaultWebProxy;
		}
	}

}
