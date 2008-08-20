//
// LocalServiceSecuritySettings.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Transactions;
using System.ServiceModel.Security;

namespace System.ServiceModel.Channels
{
	[MonoTODO]
	public sealed class LocalServiceSecuritySettings
	{
		bool detect_replays;
		TimeSpan max_clock_skew;
		bool reconnect;
		int replay_cache_size;
		TimeSpan inactivity_timeout, cookie_lifetime,
			negotiation_timeout, replay_window, renewal_interval,
			rollover_interval, validity_duration;
		Collection<Type> claim_types =
			new Collection<Type> ();
		int max_sessions, max_negotiations, max_cached_cookies;
		SecurityStateEncoder encoder;
		bool send_fault;

		public LocalServiceSecuritySettings ()
		{
		}

		public bool DetectReplays {
			get { return detect_replays; }
			set { detect_replays = value; }
		}

		public TimeSpan InactivityTimeout {
			get { return inactivity_timeout; }
			set { inactivity_timeout = value; }
		}

		public TimeSpan IssuedCookieLifetime {
			get { return cookie_lifetime; }
			set { cookie_lifetime = value; }
		}

		public int MaxCachedCookies {
			get { return max_cached_cookies; }
			set { max_cached_cookies = value; }
		}

		public TimeSpan MaxClockSkew {
			get { return max_clock_skew; }
			set { max_clock_skew = value; }
		}

		public int MaxPendingSessions {
			get { return max_sessions; }
			set { max_sessions = value; }
		}

		public int MaxStatefulNegotiations {
			get { return max_negotiations; }
			set { max_negotiations = value; }
		}

		public TimeSpan NegotiationTimeout {
			get { return negotiation_timeout; }
			set { negotiation_timeout = value; }
		}

		public bool ReconnectTransportOnFailure {
			get { return reconnect; }
			set { reconnect = value; }
		}

		public int ReplayCacheSize {
			get { return replay_cache_size; }
			set { replay_cache_size = value; }
		}

		public TimeSpan ReplayWindow {
			get { return replay_window; }
			set { replay_window = value; }
		}

		public TimeSpan SessionKeyRenewalInterval {
			get { return renewal_interval; }
			set { renewal_interval = value; }
		}

		public TimeSpan SessionKeyRolloverInterval {
			get { return rollover_interval; }
			set { rollover_interval = value; }
		}

		public TimeSpan TimestampValidityDuration {
			get { return validity_duration; }
			set { validity_duration = value; }
		}

		public LocalServiceSecuritySettings Clone ()
		{
			LocalServiceSecuritySettings other = (LocalServiceSecuritySettings) MemberwiseClone ();
			other.claim_types = new Collection<Type> (claim_types);
			return other;
		}
	}
}
