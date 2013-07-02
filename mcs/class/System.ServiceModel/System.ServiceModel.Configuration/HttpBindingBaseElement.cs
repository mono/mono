//
// HttpBindingBaseElement.cs
//
// This is a .NET 4.5 addition and contains most of the code from
// BasicHttpBindingElement.
//
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
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
	public abstract class HttpBindingBaseElement
		 : StandardBindingElement,  IBindingConfigurationElement
	{
		ConfigurationPropertyCollection _properties;

		public HttpBindingBaseElement ()
		{
		}

		public HttpBindingBaseElement (string name) : base (name) { }

		// Properties

		[ConfigurationProperty ("allowCookies",
			DefaultValue = false,
			 Options = ConfigurationPropertyOptions.None)]
		public bool AllowCookies {
			get { return (bool) this ["allowCookies"]; }
			set { this ["allowCookies"] = value; }
		}

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

		[IntegerValidator ( MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxBufferSize",
			 DefaultValue = "65536",
			 Options = ConfigurationPropertyOptions.None)]
		public int MaxBufferSize {
			get { return (int) this ["maxBufferSize"]; }
			set { this ["maxBufferSize"] = value; }
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

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = base.Properties;
					_properties.Add (new ConfigurationProperty ("allowCookies", typeof (bool), "false", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("bypassProxyOnLocal", typeof (bool), "false", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("hostNameComparisonMode", typeof (HostNameComparisonMode), "StrongWildcard", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxBufferPoolSize", typeof (long), "524288", null, new LongValidator (0, 9223372036854775807, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxBufferSize", typeof (int), "65536", null, new IntegerValidator (1, int.MaxValue, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxReceivedMessageSize", typeof (long), "65536", null, new LongValidator (1, 9223372036854775807, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("proxyAddress", typeof (Uri), null, new UriTypeConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("readerQuotas", typeof (XmlDictionaryReaderQuotasElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("textEncoding", typeof (Encoding), HttpBindingBase.DefaultTextEncoding, EncodingConverter.Instance, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("transferMode", typeof (TransferMode), "Buffered", null, null, ConfigurationPropertyOptions.None));
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

		[TypeConverter (typeof (EncodingConverter))]
		[ConfigurationProperty ("textEncoding",
			 DefaultValue = "utf-8",
			 Options = ConfigurationPropertyOptions.None)]
		public Encoding TextEncoding {
			get { return (Encoding) this ["textEncoding"]; }
			set { this ["textEncoding"] = value; }
		}

		[ConfigurationProperty ("transferMode",
			 DefaultValue = "Buffered",
			 Options = ConfigurationPropertyOptions.None)]
		public TransferMode TransferMode {
			get { return (TransferMode) this ["transferMode"]; }
			set { this ["transferMode"] = value; }
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
			HttpBindingBase basicHttpBinding = (HttpBindingBase) binding;
			
			basicHttpBinding.AllowCookies = AllowCookies;
			basicHttpBinding.BypassProxyOnLocal = BypassProxyOnLocal;
			basicHttpBinding.HostNameComparisonMode = HostNameComparisonMode;
			basicHttpBinding.MaxBufferPoolSize = MaxBufferPoolSize;
			basicHttpBinding.MaxBufferSize = MaxBufferSize;
			basicHttpBinding.MaxReceivedMessageSize = MaxReceivedMessageSize;
			basicHttpBinding.ProxyAddress = ProxyAddress;

			ReaderQuotas.ApplyConfiguration (basicHttpBinding.ReaderQuotas);

			basicHttpBinding.TextEncoding = TextEncoding;
			basicHttpBinding.TransferMode = TransferMode;
			basicHttpBinding.UseDefaultWebProxy = UseDefaultWebProxy;
		}

		protected internal override void InitializeFrom (Binding binding)
		{
			HttpBindingBase b = (HttpBindingBase) binding;
			
			base.InitializeFrom (binding);
			AllowCookies = b.AllowCookies;
			BypassProxyOnLocal = b.BypassProxyOnLocal;
			HostNameComparisonMode = b.HostNameComparisonMode;
			MaxBufferPoolSize = b.MaxBufferPoolSize;
			MaxBufferSize = b.MaxBufferSize;
			MaxReceivedMessageSize = b.MaxReceivedMessageSize;
			ProxyAddress = b.ProxyAddress;

			ReaderQuotas.ApplyConfiguration (b.ReaderQuotas);

			TextEncoding = b.TextEncoding;
			TransferMode = b.TransferMode;
			UseDefaultWebProxy = b.UseDefaultWebProxy;
		}
	}

}
