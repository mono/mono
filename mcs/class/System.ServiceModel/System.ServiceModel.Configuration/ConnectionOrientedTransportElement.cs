//
// ConnectionOrientedTransportElement.cs
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
	public abstract class ConnectionOrientedTransportElement
		 : TransportElement
	{
		ConfigurationPropertyCollection _properties;

		protected ConnectionOrientedTransportElement () {
		}

		// Properties

		[ConfigurationProperty ("channelInitializationTimeout",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:00:05")]
		public TimeSpan ChannelInitializationTimeout {
			get { return (TimeSpan) base ["channelInitializationTimeout"]; }
			set { base ["channelInitializationTimeout"] = value; }
		}

		[IntegerValidator (MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("connectionBufferSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "8192")]
		public int ConnectionBufferSize {
			get { return (int) base ["connectionBufferSize"]; }
			set { base ["connectionBufferSize"] = value; }
		}

		[ConfigurationProperty ("hostNameComparisonMode",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "StrongWildcard")]
		public HostNameComparisonMode HostNameComparisonMode {
			get { return (HostNameComparisonMode) base ["hostNameComparisonMode"]; }
			set { base ["hostNameComparisonMode"] = value; }
		}

		[ConfigurationProperty ("maxBufferSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "65536")]
		[IntegerValidator (MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int MaxBufferSize {
			get { return (int) base ["maxBufferSize"]; }
			set { base ["maxBufferSize"] = value; }
		}

		[ConfigurationProperty ("maxOutputDelay",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:00:00.2")]
		public TimeSpan MaxOutputDelay {
			get { return (TimeSpan) base ["maxOutputDelay"]; }
			set { base ["maxOutputDelay"] = value; }
		}

		[IntegerValidator (MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxPendingAccepts",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "1")]
		public int MaxPendingAccepts {
			get { return (int) base ["maxPendingAccepts"]; }
			set { base ["maxPendingAccepts"] = value; }
		}

		[ConfigurationProperty ("maxPendingConnections",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "10")]
		[IntegerValidator (MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int MaxPendingConnections {
			get { return (int) base ["maxPendingConnections"]; }
			set { base ["maxPendingConnections"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = base.Properties;
					_properties.Add (new ConfigurationProperty ("channelInitializationTimeout", typeof (TimeSpan), "00:00:05", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("connectionBufferSize", typeof (int), "8192", null, new IntegerValidator (1, int.MaxValue, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("hostNameComparisonMode", typeof (HostNameComparisonMode), "StrongWildcard", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxBufferSize", typeof (int), "65536", null, new IntegerValidator (1, int.MaxValue, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxOutputDelay", typeof (TimeSpan), "00:00:00.2", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxPendingAccepts", typeof (int), "1", null, new IntegerValidator (1, int.MaxValue, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxPendingConnections", typeof (int), "10", null, new IntegerValidator (1, int.MaxValue, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("transferMode", typeof (TransferMode), "Buffered", null, null, ConfigurationPropertyOptions.None));
				}
				return _properties;
			}
		}

		[ConfigurationProperty ("transferMode",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Buffered")]
		public TransferMode TransferMode {
			get { return (TransferMode) base ["transferMode"]; }
			set { base ["transferMode"] = value; }
		}


	}

}
