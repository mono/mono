#if NET_4_0
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2011 Novell, Inc.  http://www.novell.com
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
	public sealed class EndToEndTracingElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty activity_tracing;
		static ConfigurationProperty message_flow_tracing;
		static ConfigurationProperty propagate_activity;


		static EndToEndTracingElement ()
		{
			properties = new ConfigurationPropertyCollection ();

			activity_tracing = new ConfigurationProperty ("acticityTracing", typeof (bool), null, null, null, ConfigurationPropertyOptions.None);

			message_flow_tracing = new ConfigurationProperty ("messageFlowTracing", typeof (bool), null, null, null, ConfigurationPropertyOptions.None);

			propagate_activity = new ConfigurationProperty ("propagateActivity", typeof (bool), null, null, null, ConfigurationPropertyOptions.None);

			properties.Add (activity_tracing);
			properties.Add (message_flow_tracing);
			properties.Add (propagate_activity);
		}


		// Properties

		[ConfigurationProperty ("activityTracing",
			 DefaultValue = false,
			 Options = ConfigurationPropertyOptions.None)]
		public bool ActivityTracing {
			get { return (bool) base [activity_tracing]; }
			set { base [activity_tracing] = value; }
		}

		[ConfigurationProperty ("messageFlowTracing",
			 DefaultValue = false,
			 Options = ConfigurationPropertyOptions.None)]
		public bool MessageFlowTracing {
			get { return (bool) base [message_flow_tracing]; }
			set { base [message_flow_tracing] = value; }
		}

		[ConfigurationProperty ("propagateActivity",
			 DefaultValue = false,
			 Options = ConfigurationPropertyOptions.None)]
		public bool PropagateActivity {
			get { return (bool) base [propagate_activity]; }
			set { base [propagate_activity] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return base.Properties; }
		}
	}

}
#endif
