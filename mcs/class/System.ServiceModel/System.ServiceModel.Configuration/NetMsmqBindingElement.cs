//
// NetMsmqBindingElement.cs
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
	public partial class NetMsmqBindingElement
		 : MsmqBindingElementBase,  IBindingConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty binding_element_type;
		static ConfigurationProperty max_buffer_pool_size;
		static ConfigurationProperty queue_transfer_protocol;
		static ConfigurationProperty reader_quotas;
		static ConfigurationProperty security;
		static ConfigurationProperty use_active_directory;

		static NetMsmqBindingElement ()
		{
			properties = new ConfigurationPropertyCollection ();

			max_buffer_pool_size = new ConfigurationProperty ("maxBufferPoolSize",
				typeof (long), "524288", null/* FIXME: get converter for long*/, null,
				ConfigurationPropertyOptions.None);

			queue_transfer_protocol = new ConfigurationProperty ("queueTransferProtocol",
				typeof (QueueTransferProtocol), "Native", null/* FIXME: get converter for QueueTransferProtocol*/, null,
				ConfigurationPropertyOptions.None);

			reader_quotas = new ConfigurationProperty ("readerQuotas",
				typeof (XmlDictionaryReaderQuotasElement), null, null/* FIXME: get converter for XmlDictionaryReaderQuotasElement*/, null,
				ConfigurationPropertyOptions.None);

			security = new ConfigurationProperty ("security",
				typeof (NetMsmqSecurityElement), null, null/* FIXME: get converter for NetMsmqSecurityElement*/, null,
				ConfigurationPropertyOptions.None);

			use_active_directory = new ConfigurationProperty ("useActiveDirectory",
				typeof (bool), "false", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			properties.Add (binding_element_type);
			properties.Add (max_buffer_pool_size);
			properties.Add (queue_transfer_protocol);
			properties.Add (reader_quotas);
			properties.Add (security);
			properties.Add (use_active_directory);
		}

		public NetMsmqBindingElement ()
		{
		}


		// Properties

		protected override Type BindingElementType {
			get { return (Type) base [binding_element_type]; }
		}

		[ConfigurationProperty ("maxBufferPoolSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "524288")]
		[LongValidator ( MinValue = 0,
			 MaxValue = 9223372036854775807,
			ExcludeRange = false)]
		public long MaxBufferPoolSize {
			get { return (long) base [max_buffer_pool_size]; }
			set { base [max_buffer_pool_size] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[ConfigurationProperty ("queueTransferProtocol",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Native")]
		public QueueTransferProtocol QueueTransferProtocol {
			get { return (QueueTransferProtocol) base [queue_transfer_protocol]; }
			set { base [queue_transfer_protocol] = value; }
		}

		[ConfigurationProperty ("readerQuotas",
			 Options = ConfigurationPropertyOptions.None)]
		public XmlDictionaryReaderQuotasElement ReaderQuotas {
			get { return (XmlDictionaryReaderQuotasElement) base [reader_quotas]; }
		}

		[ConfigurationProperty ("security",
			 Options = ConfigurationPropertyOptions.None)]
		public NetMsmqSecurityElement Security {
			get { return (NetMsmqSecurityElement) base [security]; }
		}

		[ConfigurationProperty ("useActiveDirectory",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool UseActiveDirectory {
			get { return (bool) base [use_active_directory]; }
			set { base [use_active_directory] = value; }
		}



		protected override void OnApplyConfiguration (Binding binding) {
			throw new NotImplementedException ();
		}
	}

}
