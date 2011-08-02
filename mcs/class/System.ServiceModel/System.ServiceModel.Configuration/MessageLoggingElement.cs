//
// MessageLoggingElement.cs
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
	public sealed partial class MessageLoggingElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty filters;
		static ConfigurationProperty log_entire_message;
		static ConfigurationProperty log_known_pii;
		static ConfigurationProperty log_malformed_messages;
		static ConfigurationProperty log_messages_at_service_level;
		static ConfigurationProperty log_messages_at_transport_level;
		static ConfigurationProperty max_messages_to_log;
		static ConfigurationProperty max_size_of_message_to_log;

		static MessageLoggingElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			filters = new ConfigurationProperty ("filters",
				typeof (XPathMessageFilterElementCollection), null, null/* FIXME: get converter for XPathMessageFilterElementCollection*/, null,
				ConfigurationPropertyOptions.None);

			log_entire_message = new ConfigurationProperty ("logEntireMessage", typeof (bool), false, new BooleanConverter (), null, ConfigurationPropertyOptions.None);

			log_known_pii = new ConfigurationProperty ("logKnownPii", typeof (bool), false, new BooleanConverter (), null, ConfigurationPropertyOptions.None);

			log_malformed_messages = new ConfigurationProperty ("logMalformedMessages", typeof (bool), false, new BooleanConverter (), null, ConfigurationPropertyOptions.None);

			log_messages_at_service_level = new ConfigurationProperty ("logMessagesAtServiceLevel", typeof (bool), false, new BooleanConverter (), null, ConfigurationPropertyOptions.None);

			log_messages_at_transport_level = new ConfigurationProperty ("logMessagesAtTransportLevel", typeof (bool), false, new BooleanConverter (), null, ConfigurationPropertyOptions.None);

			max_messages_to_log = new ConfigurationProperty ("maxMessagesToLog", typeof (int), "10000", null, null, ConfigurationPropertyOptions.None);

			max_size_of_message_to_log = new ConfigurationProperty ("maxSizeOfMessageToLog", typeof (int), 262144, null, null, ConfigurationPropertyOptions.None);

			properties.Add (filters);
			properties.Add (log_entire_message);
			properties.Add (log_known_pii);
			properties.Add (log_malformed_messages);
			properties.Add (log_messages_at_service_level);
			properties.Add (log_messages_at_transport_level);
			properties.Add (max_messages_to_log);
			properties.Add (max_size_of_message_to_log);
		}

		public MessageLoggingElement ()
		{
		}


		// Properties

		[ConfigurationProperty ("filters",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = null)]
		public XPathMessageFilterElementCollection Filters {
			get { return (XPathMessageFilterElementCollection) base [filters]; }
		}

		[ConfigurationProperty ("logEntireMessage",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool LogEntireMessage {
			get { return (bool) base [log_entire_message]; }
			set { base [log_entire_message] = value; }
		}

#if NET_4_0
		[ConfigurationProperty ("logKnownPii",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool LogKnownPii {
			get { return (bool) base [log_known_pii]; }
			set { base [log_known_pii] = value; }
		}
#endif

		[ConfigurationProperty ("logMalformedMessages",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool LogMalformedMessages {
			get { return (bool) base [log_malformed_messages]; }
			set { base [log_malformed_messages] = value; }
		}

		[ConfigurationProperty ("logMessagesAtServiceLevel",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool LogMessagesAtServiceLevel {
			get { return (bool) base [log_messages_at_service_level]; }
			set { base [log_messages_at_service_level] = value; }
		}

		[ConfigurationProperty ("logMessagesAtTransportLevel",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = false)]
		public bool LogMessagesAtTransportLevel {
			get { return (bool) base [log_messages_at_transport_level]; }
			set { base [log_messages_at_transport_level] = value; }
		}

		[IntegerValidator ( MinValue = -1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxMessagesToLog",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "10000")]
		public int MaxMessagesToLog {
			get { return (int) base [max_messages_to_log]; }
			set { base [max_messages_to_log] = value; }
		}

		[IntegerValidator ( MinValue = -1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxSizeOfMessageToLog",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "262144")]
		public int MaxSizeOfMessageToLog {
			get { return (int) base [max_size_of_message_to_log]; }
			set { base [max_size_of_message_to_log] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}


	}

}
