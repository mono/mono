//
// MsmqBindingElementBase.cs
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
	public abstract partial class MsmqBindingElementBase
		 : StandardBindingElement,  IBindingConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty custom_dead_letter_queue;
		static ConfigurationProperty dead_letter_queue;
		static ConfigurationProperty durable;
		static ConfigurationProperty exactly_once;
		static ConfigurationProperty max_received_message_size;
		static ConfigurationProperty max_retry_cycles;
		static ConfigurationProperty receive_error_handling;
		static ConfigurationProperty receive_retry_count;
		static ConfigurationProperty retry_cycle_delay;
		static ConfigurationProperty time_to_live;
		static ConfigurationProperty use_msmq_tracing;
		static ConfigurationProperty use_source_journal;

		static MsmqBindingElementBase ()
		{
			properties = new ConfigurationPropertyCollection ();
			custom_dead_letter_queue = new ConfigurationProperty ("customDeadLetterQueue",
				typeof (Uri), null, new UriTypeConverter (), null,
				ConfigurationPropertyOptions.None);

			dead_letter_queue = new ConfigurationProperty ("deadLetterQueue",
				typeof (DeadLetterQueue), "System", null/* FIXME: get converter for DeadLetterQueue*/, null,
				ConfigurationPropertyOptions.None);

			durable = new ConfigurationProperty ("durable",
				typeof (bool), "true", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			exactly_once = new ConfigurationProperty ("exactlyOnce",
				typeof (bool), "true", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			max_received_message_size = new ConfigurationProperty ("maxReceivedMessageSize",
				typeof (long), "65536", null/* FIXME: get converter for long*/, null,
				ConfigurationPropertyOptions.None);

			max_retry_cycles = new ConfigurationProperty ("maxRetryCycles",
				typeof (int), "2", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			receive_error_handling = new ConfigurationProperty ("receiveErrorHandling",
				typeof (ReceiveErrorHandling), "Fault", null/* FIXME: get converter for ReceiveErrorHandling*/, null,
				ConfigurationPropertyOptions.None);

			receive_retry_count = new ConfigurationProperty ("receiveRetryCount",
				typeof (int), "5", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			retry_cycle_delay = new ConfigurationProperty ("retryCycleDelay",
				typeof (TimeSpan), "00:30:00", null/* FIXME: get converter for TimeSpan*/, null,
				ConfigurationPropertyOptions.None);

			time_to_live = new ConfigurationProperty ("timeToLive",
				typeof (TimeSpan), "1.00:00:00", null/* FIXME: get converter for TimeSpan*/, null,
				ConfigurationPropertyOptions.None);

			use_msmq_tracing = new ConfigurationProperty ("useMsmqTracing",
				typeof (bool), "false", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			use_source_journal = new ConfigurationProperty ("useSourceJournal",
				typeof (bool), "false", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			properties.Add (custom_dead_letter_queue);
			properties.Add (dead_letter_queue);
			properties.Add (durable);
			properties.Add (exactly_once);
			properties.Add (max_received_message_size);
			properties.Add (max_retry_cycles);
			properties.Add (receive_error_handling);
			properties.Add (receive_retry_count);
			properties.Add (retry_cycle_delay);
			properties.Add (time_to_live);
			properties.Add (use_msmq_tracing);
			properties.Add (use_source_journal);
		}

		protected MsmqBindingElementBase ()
		{
		}


		// Properties

		[ConfigurationProperty ("customDeadLetterQueue",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = null)]
		public Uri CustomDeadLetterQueue {
			get { return (Uri) base [custom_dead_letter_queue]; }
			set { base [custom_dead_letter_queue] = value; }
		}

		[ConfigurationProperty ("deadLetterQueue",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "System")]
		public DeadLetterQueue DeadLetterQueue {
			get { return (DeadLetterQueue) base [dead_letter_queue]; }
			set { base [dead_letter_queue] = value; }
		}

		[ConfigurationProperty ("durable",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool Durable {
			get { return (bool) base [durable]; }
			set { base [durable] = value; }
		}

		[ConfigurationProperty ("exactlyOnce",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool ExactlyOnce {
			get { return (bool) base [exactly_once]; }
			set { base [exactly_once] = value; }
		}

		[LongValidator ( MinValue = 0,
			 MaxValue = 9223372036854775807,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxReceivedMessageSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "65536")]
		public long MaxReceivedMessageSize {
			get { return (long) base [max_received_message_size]; }
			set { base [max_received_message_size] = value; }
		}

		[ConfigurationProperty ("maxRetryCycles",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "2")]
		[IntegerValidator ( MinValue = 0,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int MaxRetryCycles {
			get { return (int) base [max_retry_cycles]; }
			set { base [max_retry_cycles] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[ConfigurationProperty ("receiveErrorHandling",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Fault")]
		public ReceiveErrorHandling ReceiveErrorHandling {
			get { return (ReceiveErrorHandling) base [receive_error_handling]; }
			set { base [receive_error_handling] = value; }
		}

		[ConfigurationProperty ("receiveRetryCount",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "5")]
		[IntegerValidator ( MinValue = 0,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int ReceiveRetryCount {
			get { return (int) base [receive_retry_count]; }
			set { base [receive_retry_count] = value; }
		}

		[ConfigurationProperty ("retryCycleDelay",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:30:00")]
		public TimeSpan RetryCycleDelay {
			get { return (TimeSpan) base [retry_cycle_delay]; }
			set { base [retry_cycle_delay] = value; }
		}

		[ConfigurationProperty ("timeToLive",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "1.00:00:00")]
		public TimeSpan TimeToLive {
			get { return (TimeSpan) base [time_to_live]; }
			set { base [time_to_live] = value; }
		}

		[ConfigurationProperty ("useMsmqTracing",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool UseMsmqTracing {
			get { return (bool) base [use_msmq_tracing]; }
			set { base [use_msmq_tracing] = value; }
		}

		[ConfigurationProperty ("useSourceJournal",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool UseSourceJournal {
			get { return (bool) base [use_source_journal]; }
			set { base [use_source_journal] = value; }
		}


	}

}
