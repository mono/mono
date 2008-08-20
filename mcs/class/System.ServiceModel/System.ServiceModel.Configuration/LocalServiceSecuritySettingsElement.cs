//
// LocalServiceSecuritySettingsElement.cs
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
	public sealed partial class LocalServiceSecuritySettingsElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty detect_replays;
		static ConfigurationProperty inactivity_timeout;
		static ConfigurationProperty issued_cookie_lifetime;
		static ConfigurationProperty max_cached_cookies;
		static ConfigurationProperty max_clock_skew;
		static ConfigurationProperty max_pending_sessions;
		static ConfigurationProperty max_stateful_negotiations;
		static ConfigurationProperty negotiation_timeout;
		static ConfigurationProperty reconnect_transport_on_failure;
		static ConfigurationProperty replay_cache_size;
		static ConfigurationProperty replay_window;
		static ConfigurationProperty session_key_renewal_interval;
		static ConfigurationProperty session_key_rollover_interval;
		static ConfigurationProperty timestamp_validity_duration;

		static LocalServiceSecuritySettingsElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			detect_replays = new ConfigurationProperty ("detectReplays",
				typeof (bool), "true", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			inactivity_timeout = new ConfigurationProperty ("inactivityTimeout",
				typeof (TimeSpan), "00:02:00", null/* FIXME: get converter for TimeSpan*/, null,
				ConfigurationPropertyOptions.None);

			issued_cookie_lifetime = new ConfigurationProperty ("issuedCookieLifetime",
				typeof (TimeSpan), "10:00:00", null/* FIXME: get converter for TimeSpan*/, null,
				ConfigurationPropertyOptions.None);

			max_cached_cookies = new ConfigurationProperty ("maxCachedCookies",
				typeof (int), "1000", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			max_clock_skew = new ConfigurationProperty ("maxClockSkew",
				typeof (TimeSpan), "00:05:00", null/* FIXME: get converter for TimeSpan*/, null,
				ConfigurationPropertyOptions.None);

			max_pending_sessions = new ConfigurationProperty ("maxPendingSessions",
				typeof (int), "128", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			max_stateful_negotiations = new ConfigurationProperty ("maxStatefulNegotiations",
				typeof (int), "128", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			negotiation_timeout = new ConfigurationProperty ("negotiationTimeout",
				typeof (TimeSpan), "00:01:00", null/* FIXME: get converter for TimeSpan*/, null,
				ConfigurationPropertyOptions.None);

			reconnect_transport_on_failure = new ConfigurationProperty ("reconnectTransportOnFailure",
				typeof (bool), "true", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			replay_cache_size = new ConfigurationProperty ("replayCacheSize",
				typeof (int), "900000", null/* FIXME: get converter for int*/, null,
				ConfigurationPropertyOptions.None);

			replay_window = new ConfigurationProperty ("replayWindow",
				typeof (TimeSpan), "00:05:00", null/* FIXME: get converter for TimeSpan*/, null,
				ConfigurationPropertyOptions.None);

			session_key_renewal_interval = new ConfigurationProperty ("sessionKeyRenewalInterval",
				typeof (TimeSpan), "15:00:00", null/* FIXME: get converter for TimeSpan*/, null,
				ConfigurationPropertyOptions.None);

			session_key_rollover_interval = new ConfigurationProperty ("sessionKeyRolloverInterval",
				typeof (TimeSpan), "00:05:00", null/* FIXME: get converter for TimeSpan*/, null,
				ConfigurationPropertyOptions.None);

			timestamp_validity_duration = new ConfigurationProperty ("timestampValidityDuration",
				typeof (TimeSpan), "00:05:00", null/* FIXME: get converter for TimeSpan*/, null,
				ConfigurationPropertyOptions.None);

			properties.Add (detect_replays);
			properties.Add (inactivity_timeout);
			properties.Add (issued_cookie_lifetime);
			properties.Add (max_cached_cookies);
			properties.Add (max_clock_skew);
			properties.Add (max_pending_sessions);
			properties.Add (max_stateful_negotiations);
			properties.Add (negotiation_timeout);
			properties.Add (reconnect_transport_on_failure);
			properties.Add (replay_cache_size);
			properties.Add (replay_window);
			properties.Add (session_key_renewal_interval);
			properties.Add (session_key_rollover_interval);
			properties.Add (timestamp_validity_duration);
		}

		public LocalServiceSecuritySettingsElement ()
		{
		}


		// Properties

		[ConfigurationProperty ("detectReplays",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool DetectReplays {
			get { return (bool) base [detect_replays]; }
			set { base [detect_replays] = value; }
		}

		[ConfigurationProperty ("inactivityTimeout",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:02:00")]
		public TimeSpan InactivityTimeout {
			get { return (TimeSpan) base [inactivity_timeout]; }
			set { base [inactivity_timeout] = value; }
		}

		[ConfigurationProperty ("issuedCookieLifetime",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "10:00:00")]
		public TimeSpan IssuedCookieLifetime {
			get { return (TimeSpan) base [issued_cookie_lifetime]; }
			set { base [issued_cookie_lifetime] = value; }
		}

		[IntegerValidator ( MinValue = 0,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxCachedCookies",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "1000")]
		public int MaxCachedCookies {
			get { return (int) base [max_cached_cookies]; }
			set { base [max_cached_cookies] = value; }
		}

		[ConfigurationProperty ("maxClockSkew",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:05:00")]
		public TimeSpan MaxClockSkew {
			get { return (TimeSpan) base [max_clock_skew]; }
			set { base [max_clock_skew] = value; }
		}

		[IntegerValidator ( MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxPendingSessions",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "128")]
		public int MaxPendingSessions {
			get { return (int) base [max_pending_sessions]; }
			set { base [max_pending_sessions] = value; }
		}

		[IntegerValidator ( MinValue = 0,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("maxStatefulNegotiations",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "128")]
		public int MaxStatefulNegotiations {
			get { return (int) base [max_stateful_negotiations]; }
			set { base [max_stateful_negotiations] = value; }
		}

		[ConfigurationProperty ("negotiationTimeout",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:01:00")]
		public TimeSpan NegotiationTimeout {
			get { return (TimeSpan) base [negotiation_timeout]; }
			set { base [negotiation_timeout] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[ConfigurationProperty ("reconnectTransportOnFailure",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool ReconnectTransportOnFailure {
			get { return (bool) base [reconnect_transport_on_failure]; }
			set { base [reconnect_transport_on_failure] = value; }
		}

		[IntegerValidator ( MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		[ConfigurationProperty ("replayCacheSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "900000")]
		public int ReplayCacheSize {
			get { return (int) base [replay_cache_size]; }
			set { base [replay_cache_size] = value; }
		}

		[ConfigurationProperty ("replayWindow",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:05:00")]
		public TimeSpan ReplayWindow {
			get { return (TimeSpan) base [replay_window]; }
			set { base [replay_window] = value; }
		}

		[ConfigurationProperty ("sessionKeyRenewalInterval",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "15:00:00")]
		public TimeSpan SessionKeyRenewalInterval {
			get { return (TimeSpan) base [session_key_renewal_interval]; }
			set { base [session_key_renewal_interval] = value; }
		}

		[ConfigurationProperty ("sessionKeyRolloverInterval",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:05:00")]
		public TimeSpan SessionKeyRolloverInterval {
			get { return (TimeSpan) base [session_key_rollover_interval]; }
			set { base [session_key_rollover_interval] = value; }
		}

		[ConfigurationProperty ("timestampValidityDuration",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:05:00")]
		public TimeSpan TimestampValidityDuration {
			get { return (TimeSpan) base [timestamp_validity_duration]; }
			set { base [timestamp_validity_duration] = value; }
		}


	}

}
