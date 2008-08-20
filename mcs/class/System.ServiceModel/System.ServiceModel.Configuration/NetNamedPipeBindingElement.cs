//
// NetNamedPipeBindingElement.cs
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
	public partial class NetNamedPipeBindingElement
		 : StandardBindingElement,  IBindingConfigurationElement
	{
		ConfigurationPropertyCollection _properties;

		public NetNamedPipeBindingElement ()
		{
		}

		public NetNamedPipeBindingElement (string name) : base (name) { }

		// Properties

		protected override Type BindingElementType {
			get { return typeof (NetNamedPipeBinding); }
		}

		[ConfigurationProperty ("hostNameComparisonMode",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "StrongWildcard")]
		public HostNameComparisonMode HostNameComparisonMode {
			get { return (HostNameComparisonMode) this ["hostNameComparisonMode"]; }
			set { this ["hostNameComparisonMode"] = value; }
		}

		[ConfigurationProperty ("maxBufferPoolSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "524288")]
		[LongValidator ( MinValue = 0,
			 MaxValue = 9223372036854775807,
			ExcludeRange = false)]
		public long MaxBufferPoolSize {
			get { return (long) this ["maxBufferPoolSize"]; }
			set { this ["maxBufferPoolSize"] = value; }
		}

		[ConfigurationProperty ("maxBufferSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "65536")]
		[IntegerValidator ( MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int MaxBufferSize {
			get { return (int) this ["maxBufferSize"]; }
			set { this ["maxBufferSize"] = value; }
		}

		[IntegerValidator ( MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxConnections",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "10")]
		public int MaxConnections {
			get { return (int) this ["maxConnections"]; }
			set { this ["maxConnections"] = value; }
		}

		[ConfigurationProperty ("maxReceivedMessageSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "65536")]
		[LongValidator ( MinValue = 1,
			 MaxValue = 9223372036854775807,
			ExcludeRange = false)]
		public long MaxReceivedMessageSize {
			get { return (long) this ["maxReceivedMessageSize"]; }
			set { this ["maxReceivedMessageSize"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = base.Properties;
					_properties.Add (new ConfigurationProperty ("hostNameComparisonMode", typeof (HostNameComparisonMode), "StrongWildcard", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxBufferPoolSize", typeof (long), "524288", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxBufferSize", typeof (int), "65536", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxConnections", typeof (int), "10", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxReceivedMessageSize", typeof (long), "65536", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("readerQuotas", typeof (XmlDictionaryReaderQuotasElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("security", typeof (NetNamedPipeSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("transactionFlow", typeof (bool), "false", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("transactionProtocol", typeof (TransactionProtocol), "OleTransactions", TransactionProtocolConverter.Instance, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("transferMode", typeof (TransferMode), "Buffered", null, null, ConfigurationPropertyOptions.None));
				}
				return _properties;
			}
		}

		[ConfigurationProperty ("readerQuotas",
			 Options = ConfigurationPropertyOptions.None)]
		public XmlDictionaryReaderQuotasElement ReaderQuotas {
			get { return (XmlDictionaryReaderQuotasElement) this ["readerQuotas"]; }
		}

		[ConfigurationProperty ("security",
			 Options = ConfigurationPropertyOptions.None)]
		public NetNamedPipeSecurityElement Security {
			get { return (NetNamedPipeSecurityElement) this ["security"]; }
		}

		[ConfigurationProperty ("transactionFlow",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool TransactionFlow {
			get { return (bool) this ["transactionFlow"]; }
			set { this ["transactionFlow"] = value; }
		}

		[ConfigurationProperty ("transactionProtocol",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "OleTransactions")]
		[TypeConverter (typeof (TransactionProtocolConverter))]
		public TransactionProtocol TransactionProtocol {
			get { return (TransactionProtocol) this ["transactionProtocol"]; }
			set { this ["transactionProtocol"] = value; }
		}

		[ConfigurationProperty ("transferMode",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Buffered")]
		public TransferMode TransferMode {
			get { return (TransferMode) this ["transferMode"]; }
			set { this ["transferMode"] = value; }
		}



		protected override void OnApplyConfiguration (Binding binding) {
			throw new NotImplementedException ();
		}
	}

}
