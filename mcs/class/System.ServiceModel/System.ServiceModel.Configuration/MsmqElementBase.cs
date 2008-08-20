//
// MsmqElementBase.cs
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
	public abstract class MsmqElementBase
		 : TransportElement
	{
		ConfigurationPropertyCollection _properties;

		protected MsmqElementBase () {
		}


		// Properties

		[ConfigurationProperty ("customDeadLetterQueue",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = null)]
		public Uri CustomDeadLetterQueue {
			get { return (Uri) base ["customDeadLetterQueue"]; }
			set { base ["customDeadLetterQueue"] = value; }
		}

		[ConfigurationProperty ("deadLetterQueue",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "System")]
		public DeadLetterQueue DeadLetterQueue {
			get { return (DeadLetterQueue) base ["deadLetterQueue"]; }
			set { base ["deadLetterQueue"] = value; }
		}

		[ConfigurationProperty ("durable",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool Durable {
			get { return (bool) base ["durable"]; }
			set { base ["durable"] = value; }
		}

		[ConfigurationProperty ("exactlyOnce",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool ExactlyOnce {
			get { return (bool) base ["exactlyOnce"]; }
			set { base ["exactlyOnce"] = value; }
		}

		[ConfigurationProperty ("maxRetryCycles",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "2")]
		[IntegerValidator (MinValue = 0,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int MaxRetryCycles {
			get { return (int) base ["maxRetryCycles"]; }
			set { base ["maxRetryCycles"] = value; }
		}

		[ConfigurationProperty ("msmqTransportSecurity",
			 Options = ConfigurationPropertyOptions.None)]
		public MsmqTransportSecurityElement MsmqTransportSecurity {
			get { return (MsmqTransportSecurityElement) base ["msmqTransportSecurity"]; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = base.Properties;
					_properties.Add (new ConfigurationProperty ("customDeadLetterQueue", typeof (Uri), null, new UriTypeConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("deadLetterQueue", typeof (DeadLetterQueue), "System", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("durable", typeof (bool), "true", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("exactlyOnce", typeof (bool), "true", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("maxRetryCycles", typeof (int), "2", null, new IntegerValidator (0, int.MaxValue, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("msmqTransportSecurity", typeof (MsmqTransportSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("receiveErrorHandling", typeof (ReceiveErrorHandling), "Fault", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("receiveRetryCount", typeof (int), "5", null, new IntegerValidator (0, int.MaxValue, false), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("retryCycleDelay", typeof (TimeSpan), "00:30:00", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("timeToLive", typeof (TimeSpan), "1.00:00:00", null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("useMsmqTracing", typeof (bool), "false", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("useSourceJournal", typeof (bool), "false", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
				}
				return _properties;
			}
		}

		[ConfigurationProperty ("receiveErrorHandling",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Fault")]
		public ReceiveErrorHandling ReceiveErrorHandling {
			get { return (ReceiveErrorHandling) base ["receiveErrorHandling"]; }
			set { base ["receiveErrorHandling"] = value; }
		}

		[ConfigurationProperty ("receiveRetryCount",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "5")]
		[IntegerValidator (MinValue = 0,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int ReceiveRetryCount {
			get { return (int) base ["receiveRetryCount"]; }
			set { base ["receiveRetryCount"] = value; }
		}

		[ConfigurationProperty ("retryCycleDelay",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:30:00")]
		public TimeSpan RetryCycleDelay {
			get { return (TimeSpan) base ["retryCycleDelay"]; }
			set { base ["retryCycleDelay"] = value; }
		}

		[ConfigurationProperty ("timeToLive",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "1.00:00:00")]
		public TimeSpan TimeToLive {
			get { return (TimeSpan) base ["timeToLive"]; }
			set { base ["timeToLive"] = value; }
		}

		[ConfigurationProperty ("useMsmqTracing",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool UseMsmqTracing {
			get { return (bool) base ["useMsmqTracing"]; }
			set { base ["useMsmqTracing"] = value; }
		}

		[ConfigurationProperty ("useSourceJournal",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool UseSourceJournal {
			get { return (bool) base ["useSourceJournal"]; }
			set { base ["useSourceJournal"] = value; }
		}


	}

}
