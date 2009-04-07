//
// NetTcpBindingElement.cs
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
	public partial class NetTcpBindingElement
		 : StandardBindingElement,  IBindingConfigurationElement
	{
		ConfigurationPropertyCollection _properties;

		public NetTcpBindingElement ()
		{
		}
		public NetTcpBindingElement (string name) : base (name) { }


		// Properties

		protected override Type BindingElementType {
			get { return typeof (NetTcpBinding); }
		}

		[ConfigurationProperty ("hostNameComparisonMode",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "StrongWildcard")]
		public HostNameComparisonMode HostNameComparisonMode {
			get { return (HostNameComparisonMode) this ["hostNameComparisonMode"]; }
			set { this ["hostNameComparisonMode"] = value; }
		}

		[ConfigurationProperty ("listenBacklog",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "10")]
		[IntegerValidator ( MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int ListenBacklog {
			get { return (int) this ["listenBacklog"]; }
			set { this ["listenBacklog"] = value; }
		}

		[LongValidator ( MinValue = 0,
			 MaxValue = 9223372036854775807,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxBufferPoolSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "524288")]
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

		[LongValidator ( MinValue = 1,
			 MaxValue = 9223372036854775807,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxReceivedMessageSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "65536")]
		public long MaxReceivedMessageSize {
			get { return (long) this ["maxReceivedMessageSize"]; }
			set { this ["maxReceivedMessageSize"] = value; }
		}

		[ConfigurationProperty ("portSharingEnabled",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool PortSharingEnabled {
			get { return (bool) this ["portSharingEnabled"]; }
			set { this ["portSharingEnabled"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = base.Properties;
					_properties.Add (new ConfigurationProperty ("hostNameComparisonMode", typeof (HostNameComparisonMode), "StrongWildcard", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("listenBacklog", typeof (int), "10", null, new IntegerValidator (1, int.MaxValue, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxBufferPoolSize", typeof (long), "524288", null, new LongValidator (0, 9223372036854775807, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxBufferSize", typeof (int), "65536", null, new IntegerValidator (1, int.MaxValue, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxConnections", typeof (int), "10", null, new IntegerValidator (1, int.MaxValue, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxReceivedMessageSize", typeof (long), "65536", null, new LongValidator (1, 9223372036854775807, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("portSharingEnabled", typeof (bool), "false", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("readerQuotas", typeof (XmlDictionaryReaderQuotasElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("reliableSession", typeof (StandardBindingOptionalReliableSessionElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("security", typeof (NetTcpSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("transactionFlow", typeof (bool), "false", null, null, ConfigurationPropertyOptions.None));
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

		[MonoTODO ("This configuration prpperty is not applied yet")]
		[ConfigurationProperty ("reliableSession",
			 Options = ConfigurationPropertyOptions.None)]
		public StandardBindingOptionalReliableSessionElement ReliableSession {
			get { return (StandardBindingOptionalReliableSessionElement) this ["reliableSession"]; }
		}

		[ConfigurationProperty ("security",
			 Options = ConfigurationPropertyOptions.None)]
		public NetTcpSecurityElement Security {
			get { return (NetTcpSecurityElement) this ["security"]; }
		}

		[ConfigurationProperty ("transactionFlow",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool TransactionFlow {
			get { return (bool) this ["transactionFlow"]; }
			set { this ["transactionFlow"] = value; }
		}

		[TypeConverter (typeof (TransactionProtocolConverter))]
		[ConfigurationProperty ("transactionProtocol",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "OleTransactions")]
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

		protected override void OnApplyConfiguration (Binding binding)
		{
			NetTcpBinding n = (NetTcpBinding) binding;
			n.CloseTimeout = CloseTimeout;
			n.HostNameComparisonMode = HostNameComparisonMode;
			n.ListenBacklog = ListenBacklog;
			n.MaxBufferPoolSize = MaxBufferPoolSize;
			n.MaxBufferSize = MaxBufferSize;
			n.MaxConnections = MaxConnections;
			n.MaxReceivedMessageSize = MaxReceivedMessageSize;
			n.OpenTimeout = OpenTimeout;
			n.PortSharingEnabled = PortSharingEnabled;
			if (ReaderQuotas != null)
				n.ReaderQuotas = ReaderQuotas.Create ();
			n.ReceiveTimeout = ReceiveTimeout;

			// FIXME: apply this too.
			//ReliableSession.ApplyTo (n.ReliableSession);

			if (Security != null) {
				n.Security.Mode = Security.Mode;
				if (Security.Message != null) {
					n.Security.Message.AlgorithmSuite = Security.Message.AlgorithmSuite;
					n.Security.Message.ClientCredentialType = Security.Message.ClientCredentialType;
				}
				if (Security.Transport != null) {
					n.Security.Transport.ClientCredentialType = Security.Transport.ClientCredentialType;
					n.Security.Transport.ProtectionLevel = Security.Transport.ProtectionLevel;
				}
			}

			n.SendTimeout = SendTimeout;
			n.TransactionFlow = TransactionFlow;
			n.TransactionProtocol = TransactionProtocol;
			n.TransferMode = TransferMode;
		}
	}

}
