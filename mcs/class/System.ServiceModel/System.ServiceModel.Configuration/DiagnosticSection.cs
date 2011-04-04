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
	public sealed partial class DiagnosticSection
		 : ConfigurationSection
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
#if NET_4_0
		static ConfigurationProperty end_to_end_tracing;
		static ConfigurationProperty etw_provider_id;
#endif
		static ConfigurationProperty message_logging;
		static ConfigurationProperty performance_counters;
		static ConfigurationProperty performance_counter_enabled;
		static ConfigurationProperty wmi_provider_enabled;

		static DiagnosticSection ()
		{
			properties = new ConfigurationPropertyCollection ();
#if NET_4_0
			end_to_end_tracing = new ConfigurationProperty ("endToEndTracing", typeof (EndToEndTracingElement), null, null, null, ConfigurationPropertyOptions.None);

			etw_provider_id = new ConfigurationProperty ("etwProviderId", typeof (string), null, null, null, ConfigurationPropertyOptions.None);
#endif
			message_logging = new ConfigurationProperty ("messageLogging", typeof (MessageLoggingElement), null, null, null, ConfigurationPropertyOptions.None);

			performance_counters = new ConfigurationProperty ("performanceCounters", typeof (PerformanceCounterScope), "Off", null, null, ConfigurationPropertyOptions.None);

			performance_counter_enabled = new ConfigurationProperty ("performanceCounterEnabled", typeof (bool), false, null, null, ConfigurationPropertyOptions.None);

			wmi_provider_enabled = new ConfigurationProperty ("wmiProviderEnabled",
				typeof (bool), "false", new BooleanConverter (), null, ConfigurationPropertyOptions.None);

#if NET_4_0
			properties.Add (end_to_end_tracing);
			properties.Add (etw_provider_id);
#endif
			properties.Add (message_logging);
			properties.Add (performance_counters);
			properties.Add (performance_counter_enabled);
			properties.Add (wmi_provider_enabled);
		}

		public DiagnosticSection ()
		{
		}


		// Properties

#if NET_4_0
		[ConfigurationProperty ("endToEndTracing", Options = ConfigurationPropertyOptions.None)]
		public EndToEndTracingElement EndToEndTracing {
			get { return (EndToEndTracingElement) base [end_to_end_tracing]; }
		}

		[ConfigurationProperty ("etwProviderId", DefaultValue = "{c651f5f6-1c0d-492e-8ae1-b4efd7c9d503}")]
		[StringValidator (MinLength = 0)]
		public string EtwProviderId {
			get { return (string) base [etw_provider_id]; }
			set { base [etw_provider_id] = value; }
		}
#endif

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

		[ConfigurationProperty ("performanceCounterEnabled",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = false)]
		public bool PerformanceCounterEnabled {
			get { return (bool) base [performance_counter_enabled]; }
			set { base [performance_counter_enabled] = value; }
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
