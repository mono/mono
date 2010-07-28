//
// LocalClientSecuritySettingsElement.cs
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
	public sealed class LocalClientSecuritySettingsElement
		 : ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty cache_cookies;
		static ConfigurationProperty cookie_renewal_threshold_percentage;
		static ConfigurationProperty detect_replays;
		static ConfigurationProperty max_clock_skew;
		static ConfigurationProperty max_cookie_caching_time;
		static ConfigurationProperty reconnect_transport_on_failure;
		static ConfigurationProperty replay_cache_size;
		static ConfigurationProperty replay_window;
		static ConfigurationProperty session_key_renewal_interval;
		static ConfigurationProperty session_key_rollover_interval;
		static ConfigurationProperty timestamp_validity_duration;

		static LocalClientSecuritySettingsElement ()
		{
			cache_cookies = new ConfigurationProperty ("cacheCookies", typeof (bool), true, null, null, ConfigurationPropertyOptions.None);

			cookie_renewal_threshold_percentage = new ConfigurationProperty ("cookieRenewalThresholdPercentage", typeof (int), 60, null, new IntegerValidator (0, 100, false), ConfigurationPropertyOptions.None);

			detect_replays = new ConfigurationProperty ("detectReplays", typeof (bool), true, null, null, ConfigurationPropertyOptions.None);

			max_clock_skew = new ConfigurationProperty ("maxClockSkew", typeof (TimeSpan), "00:05:00", new TimeSpanConverter (), null, ConfigurationPropertyOptions.None);

			max_cookie_caching_time = new ConfigurationProperty ("maxCookieCachingTime", typeof (TimeSpan), "10675199.02:48:05.4775807", new TimeSpanConverter (), null, ConfigurationPropertyOptions.None);

			reconnect_transport_on_failure = new ConfigurationProperty ("reconnectTransportOnFailure", typeof (bool), true, null, null, ConfigurationPropertyOptions.None);

			replay_cache_size = new ConfigurationProperty ("replayCacheSize", typeof (int), 900000, null, new IntegerValidator (1, int.MaxValue, false), ConfigurationPropertyOptions.None);

			replay_window = new ConfigurationProperty ("replayWindow", typeof (TimeSpan), "00:05:00", new TimeSpanConverter (), null, ConfigurationPropertyOptions.None);

			session_key_renewal_interval = new ConfigurationProperty ("sessionKeyRenewalInterval", typeof (TimeSpan), "10:00:00", new TimeSpanConverter (), null, ConfigurationPropertyOptions.None);

			session_key_rollover_interval = new ConfigurationProperty ("sessionKeyRolloverInterval", typeof (TimeSpan), "00:05:00", new TimeSpanConverter (), null, ConfigurationPropertyOptions.None);

			timestamp_validity_duration = new ConfigurationProperty ("timestampValidityDuration", typeof (TimeSpan), "00:05:00", new TimeSpanConverter (), null, ConfigurationPropertyOptions.None);

			properties = new ConfigurationPropertyCollection ();

			properties.Add (cache_cookies);
			properties.Add (cookie_renewal_threshold_percentage);
			properties.Add (detect_replays);
			properties.Add (max_clock_skew);
			properties.Add (max_cookie_caching_time);
			properties.Add (reconnect_transport_on_failure);
			properties.Add (replay_cache_size);
			properties.Add (replay_window);
			properties.Add (session_key_renewal_interval);
			properties.Add (session_key_rollover_interval);
			properties.Add (timestamp_validity_duration);
		}

		public LocalClientSecuritySettingsElement ()
		{
		}


		// Properties

		[ConfigurationProperty ("cacheCookies",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool CacheCookies {
			get { return (bool) base ["cacheCookies"]; }
			set { base ["cacheCookies"] = value; }
		}

		[IntegerValidator ( MinValue = 0,
			 MaxValue = 100,
			ExcludeRange = false)]
		[ConfigurationProperty ("cookieRenewalThresholdPercentage",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "60")]
		public int CookieRenewalThresholdPercentage {
			get { return (int) base ["cookieRenewalThresholdPercentage"]; }
			set { base ["cookieRenewalThresholdPercentage"] = value; }
		}

		[ConfigurationProperty ("detectReplays",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool DetectReplays {
			get { return (bool) base ["detectReplays"]; }
			set { base ["detectReplays"] = value; }
		}

		[ConfigurationProperty ("maxClockSkew",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:05:00")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan MaxClockSkew {
			get { return (TimeSpan) base ["maxClockSkew"]; }
			set { base ["maxClockSkew"] = value; }
		}

		[ConfigurationProperty ("maxCookieCachingTime",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "10675199.02:48:05.4775807")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan MaxCookieCachingTime {
			get { return (TimeSpan) base ["maxCookieCachingTime"]; }
			set { base ["maxCookieCachingTime"] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[ConfigurationProperty ("reconnectTransportOnFailure",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool ReconnectTransportOnFailure {
			get { return (bool) base ["reconnectTransportOnFailure"]; }
			set { base ["reconnectTransportOnFailure"] = value; }
		}

		[ConfigurationProperty ("replayCacheSize",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "900000")]
		[IntegerValidator ( MinValue = 1,
			MaxValue = int.MaxValue,
			ExcludeRange = false)]
		public int ReplayCacheSize {
			get { return (int) base ["replayCacheSize"]; }
			set { base ["replayCacheSize"] = value; }
		}

		[ConfigurationProperty ("replayWindow",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:05:00")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan ReplayWindow {
			get { return (TimeSpan) base ["replayWindow"]; }
			set { base ["replayWindow"] = value; }
		}

		[ConfigurationProperty ("sessionKeyRenewalInterval",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "10:00:00")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan SessionKeyRenewalInterval {
			get { return (TimeSpan) base ["sessionKeyRenewalInterval"]; }
			set { base ["sessionKeyRenewalInterval"] = value; }
		}

		[ConfigurationProperty ("sessionKeyRolloverInterval",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:05:00")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan SessionKeyRolloverInterval {
			get { return (TimeSpan) base ["sessionKeyRolloverInterval"]; }
			set { base ["sessionKeyRolloverInterval"] = value; }
		}

		[ConfigurationProperty ("timestampValidityDuration",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "00:05:00")]
		[TypeConverter (typeof (TimeSpanConverter))]
		public TimeSpan TimestampValidityDuration {
			get { return (TimeSpan) base ["timestampValidityDuration"]; }
			set { base ["timestampValidityDuration"] = value; }
		}


	}

}
