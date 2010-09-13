//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
#if NET_4_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.MsmqIntegration;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Security;
using System.ServiceModel.Web;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Configuration
{
	public class WebScriptEndpointElement : StandardEndpointElement
	{
		static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection ();

		static ConfigurationProperty content_type_mapper, cross_domain_script_access_enabled, host_name_comparison_mode, max_buffer_pool_size, max_buffer_size, max_received_message_size, reader_quotas, security, transfer_mode, write_encoding;

		static WebScriptEndpointElement ()
		{
			content_type_mapper = new ConfigurationProperty ("contentTypeMapper", typeof (string), "", null, null, ConfigurationPropertyOptions.None);
			cross_domain_script_access_enabled = new ConfigurationProperty ("crossDomainScriptAccessEnabled", typeof (bool), false, null, null, ConfigurationPropertyOptions.None);
			host_name_comparison_mode = new ConfigurationProperty ("hostNameComparisonMode", typeof (HostNameComparisonMode), HostNameComparisonMode.StrongWildcard, null, null, ConfigurationPropertyOptions.None);
			max_buffer_pool_size = new ConfigurationProperty ("maxBufferPoolSize", typeof (long), 0x80000, null, null, ConfigurationPropertyOptions.None);
			max_buffer_size = new ConfigurationProperty ("maxBufferSize", typeof (int), 0x10000, null, null, ConfigurationPropertyOptions.None);
			max_received_message_size = new ConfigurationProperty ("maxReceivedMessageSize", typeof (long), 0x10000, null, null, ConfigurationPropertyOptions.None);
			reader_quotas = new ConfigurationProperty ("readerQuotas", typeof (XmlDictionaryReaderQuotas), null, null, null, ConfigurationPropertyOptions.None);
			security = new ConfigurationProperty ("security", typeof (WebHttpSecurity), null, null, null, ConfigurationPropertyOptions.None);
			transfer_mode = new ConfigurationProperty ("transferMode", typeof (TransferMode), TransferMode.Buffered, null, null, ConfigurationPropertyOptions.None);
			write_encoding = new ConfigurationProperty ("writeEncoding", typeof (Encoding), "utf-8", new EncodingConverter (), null, ConfigurationPropertyOptions.None);

			foreach (var item in new ConfigurationProperty [] {content_type_mapper, cross_domain_script_access_enabled, host_name_comparison_mode, max_buffer_pool_size, max_buffer_size, max_received_message_size, reader_quotas, security, transfer_mode, write_encoding})
				properties.Add (item);
		}

		protected internal override Type EndpointType {
			get { return typeof (WebScriptEndpoint); }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[ConfigurationProperty ("contentTypeMapper", DefaultValue = "")]
		[StringValidator (MinLength = 0)]
		public string ContentTypeMapper {
			get { return (string) this ["contentTypeMapper"]; }
			set { this ["contentTypeMapper"] = value; }
		}

		[ConfigurationProperty ("crossDomainScriptAccessEnabled", DefaultValue = false)]
		public bool CrossDomainScriptAccessEnabled {
			get { return (bool) this ["crossDomainScriptAccessEnabled"]; }
			set { this ["crossDomainScriptAccessEnabled"] = value; }
		}

		[ConfigurationProperty ("hostNameComparisonMode", DefaultValue = HostNameComparisonMode.StrongWildcard)]
		public HostNameComparisonMode HostNameComparisonMode {
			get { return (HostNameComparisonMode) this ["hostNameComparisonMode"]; }
			set { this ["hostNameComparisonMode"] = value; }
		}

		[LongValidator (MinValue = 0, MaxValue = long.MaxValue, ExcludeRange = false)]
		[ConfigurationProperty ("maxBufferPoolSize", DefaultValue = 0x80000,
			 Options = ConfigurationPropertyOptions.None)]
		public long MaxBufferPoolSize {
			get { return (long) this ["maxBufferPoolSize"]; }
			set { this ["maxBufferPoolSize"] = value; }
		}

		[IntegerValidator ( MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxBufferSize", DefaultValue = 0x10000,
			 Options = ConfigurationPropertyOptions.None)]
		public int MaxBufferSize {
			get { return (int) this ["maxBufferSize"]; }
			set { this ["maxBufferSize"] = value; }
		}

		[LongValidator ( MinValue = 1, MaxValue = long.MaxValue, ExcludeRange = false)]
		[ConfigurationProperty ("maxReceivedMessageSize", DefaultValue = 0x10000,
			 Options = ConfigurationPropertyOptions.None)]
		public long MaxReceivedMessageSize {
			get { return (long) this ["maxReceivedMessageSize"]; }
			set { this ["maxReceivedMessageSize"] = value; }
		}

		[ConfigurationProperty ("readerQuotas")]
		public XmlDictionaryReaderQuotasElement ReaderQuotas {
			get { return (XmlDictionaryReaderQuotasElement) this ["readerQuotas"]; }
		}

		[ConfigurationProperty ("security")]
		public WebHttpSecurityElement Security {
			get { return (WebHttpSecurityElement) this ["security"]; }
		}

		[ConfigurationProperty ("transferMode", DefaultValue = TransferMode.Buffered)]
		public TransferMode TransferMode {
			get { return (TransferMode) this ["transferMode"]; }
			set { this ["transferMode"] = value; }
		}

		[TypeConverter (typeof (EncodingConverter))]
		[ConfigurationProperty ("writeEncoding", DefaultValue = "utf-8")]
		public Encoding WriteEncoding {
			get { return (Encoding) this ["writeEncoding"]; }
			set { this ["writeEncoding"] = value; }
		}

		protected internal override ServiceEndpoint CreateServiceEndpoint (ContractDescription contractDescription)
		{
			throw new NotImplementedException ();
		}

		protected override void OnApplyConfiguration (ServiceEndpoint endpoint, ChannelEndpointElement serviceEndpointElement)
		{
			throw new NotImplementedException ();
		}

		protected override void OnApplyConfiguration (ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement)
		{
			throw new NotImplementedException ();
		}

		protected override void OnInitializeAndValidate (ChannelEndpointElement channelEndpointElement)
		{
			throw new NotImplementedException ();
		}

		protected override void OnInitializeAndValidate (ServiceEndpointElement serviceEndpointElement)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
