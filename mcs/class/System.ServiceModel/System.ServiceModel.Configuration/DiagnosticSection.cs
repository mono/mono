//
// DiagnosticSection.cs
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
	public sealed partial class DiagnosticSection
		 : ConfigurationSection
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty message_logging;
		static ConfigurationProperty performance_counters;
		static ConfigurationProperty wmi_provider_enabled;

		static DiagnosticSection ()
		{
			properties = new ConfigurationPropertyCollection ();
			message_logging = new ConfigurationProperty ("messageLogging",
				typeof (MessageLoggingElement), null, null/* FIXME: get converter for MessageLoggingElement*/, null,
				ConfigurationPropertyOptions.None);

			performance_counters = new ConfigurationProperty ("performanceCounters",
				typeof (PerformanceCounterScope), "Off", null/* FIXME: get converter for PerformanceCounterScope*/, null,
				ConfigurationPropertyOptions.None);

			wmi_provider_enabled = new ConfigurationProperty ("wmiProviderEnabled",
				typeof (bool), "false", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			properties.Add (message_logging);
			properties.Add (performance_counters);
			properties.Add (wmi_provider_enabled);
		}

		public DiagnosticSection ()
		{
		}


		// Properties

		[ConfigurationProperty ("messageLogging",
			 Options = ConfigurationPropertyOptions.None)]
		public MessageLoggingElement MessageLogging {
			get { return (MessageLoggingElement) base [message_logging]; }
		}

		[ConfigurationProperty ("performanceCounters",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Off")]
		public PerformanceCounterScope PerformanceCounters {
			get { return (PerformanceCounterScope) base [performance_counters]; }
			set { base [performance_counters] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[ConfigurationProperty ("wmiProviderEnabled",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool WmiProviderEnabled {
			get { return (bool) base [wmi_provider_enabled]; }
			set { base [wmi_provider_enabled] = value; }
		}


	}

}
