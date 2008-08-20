//
// TcpConnectionPoolSettingsElement.cs
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
	public sealed partial class TcpConnectionPoolSettingsElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty group_name;
		static ConfigurationProperty idle_timeout;
		static ConfigurationProperty lease_timeout;
		static ConfigurationProperty max_outbound_connections_per_endpoint;

		static TcpConnectionPoolSettingsElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			group_name = new ConfigurationProperty ("groupName",
				typeof (string), "default", new StringConverter (), null,
				ConfigurationPropertyOptions.None);

			idle_timeout = new ConfigurationProperty ("idleTimeout",
				typeof (TimeSpan), "00:02:00", null/* FIXME: get converter for TimeSpan*/, null,
				ConfigurationPropertyOptions.None);

			lease_timeout = new ConfigurationProperty ("leaseTimeout",
				typeof (TimeSpan), "00:05:00", null/* FIXME: get converter for TimeSpan*/, null,
				ConfigurationPropertyOptions.None);

			max_outbound_connections_per_endpoint = new ConfigurationProperty ("maxOutboundConnectionsPerEndpoint",
				typeof (int), "10", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			properties.Add (group_name);
			properties.Add (idle_timeout);
			properties.Add (lease_timeout);
			properties.Add (max_outbound_connections_per_endpoint);
		}

		public TcpConnectionPoolSettingsElement ()
		{
		}


		// Properties

		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("groupName",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "default")]
		public string GroupName {
			get { return (string) base [group_name]; }
			set { base [group_name] = value; }
		}

		[ConfigurationProperty ("idleTimeout",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:02:00")]
		public TimeSpan IdleTimeout {
			get { return (TimeSpan) base [idle_timeout]; }
			set { base [idle_timeout] = value; }
		}

		[ConfigurationProperty ("leaseTimeout",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:05:00")]
		public TimeSpan LeaseTimeout {
			get { return (TimeSpan) base [lease_timeout]; }
			set { base [lease_timeout] = value; }
		}

		[ConfigurationProperty ("maxOutboundConnectionsPerEndpoint",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "10")]
		[IntegerValidator ( MinValue = 0,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int MaxOutboundConnectionsPerEndpoint {
			get { return (int) base [max_outbound_connections_per_endpoint]; }
			set { base [max_outbound_connections_per_endpoint] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}


	}

}
