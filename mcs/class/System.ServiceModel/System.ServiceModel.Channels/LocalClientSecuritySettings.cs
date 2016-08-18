//
// LocalClientSecuritySettings.cs
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
using System.ServiceModel.Security;

namespace System.ServiceModel.Channels
{
	[MonoTODO]
	public sealed class LocalClientSecuritySettings
	{
#if !MOBILE
		bool cache_cookies = true;
		int cookie_renewal = 60;
		bool detect_replays = true;
		IdentityVerifier verifier = IdentityVerifier.CreateDefault ();
		TimeSpan max_cookie_cache_time = TimeSpan.MaxValue;
		bool reconnect = true;
		int replay_cache_size = 900000;
		TimeSpan renewal_interval = TimeSpan.FromHours (10);
		TimeSpan rollover_interval = TimeSpan.FromMinutes (5);
#endif

		public LocalClientSecuritySettings ()
		{
			MaxClockSkew = TimeSpan.FromMinutes (5);
			ReplayWindow = TimeSpan.FromMinutes (5);
			TimestampValidityDuration = TimeSpan.FromMinutes (5);
		}

		public TimeSpan MaxClockSkew { get; set; }
		public TimeSpan ReplayWindow { get; set; }
		public TimeSpan TimestampValidityDuration { get; set; }

#if !MOBILE
		public bool CacheCookies {
			get { return cache_cookies; }
			set { cache_cookies = value; }
		}
		public int CookieRenewalThresholdPercentage {
			get { return cookie_renewal; }
			set { cookie_renewal = value; }
		}
		public bool DetectReplays {
			get { return detect_replays; }
			set { detect_replays = value; }
		}
		public IdentityVerifier IdentityVerifier {
			get { return verifier; }
			set { verifier = value; }
		}
		public TimeSpan MaxCookieCachingTime {
			get { return max_cookie_cache_time; }
			set { max_cookie_cache_time = value; }
		}
		public bool ReconnectTransportOnFailure {
			get { return reconnect; }
			set { reconnect = value; }
		}
		public int ReplayCacheSize {
			get { return replay_cache_size; }
			set { replay_cache_size = value; }
		}
		public TimeSpan SessionKeyRenewalInterval {
			get { return renewal_interval; }
			set { renewal_interval = value; }
		}
		public TimeSpan SessionKeyRolloverInterval {
			get { return rollover_interval; }
			set { rollover_interval = value; }
		}
#endif

		[MonoTODO ("What happens to IdentityVerifier?")]
		public LocalClientSecuritySettings Clone ()
		{
			return (LocalClientSecuritySettings) MemberwiseClone ();
		}
	}
}
