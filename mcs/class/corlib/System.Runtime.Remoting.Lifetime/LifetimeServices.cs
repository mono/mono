//
// System.Runtime.Remoting.Lifetime.LifetimeServices.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;

namespace System.Runtime.Remoting.Lifetime {

	//LAMESPEC: MS docs don't say that this class is sealed.
	public sealed class LifetimeServices
	{
		private static TimeSpan _leaseManagerPollTime;
		private static TimeSpan _leaseTime;
		private static TimeSpan _renewOnCallTime;
		private static TimeSpan _sponsorshipTimeout;

		public LifetimeServices ()
		{
		}

		public static TimeSpan LeaseManagerPollTime {
			get {
				return _leaseManagerPollTime;
			}

			set {
				_leaseManagerPollTime = value;
			}
		}

		public static TimeSpan LeaseTime {
			get {
				return _leaseTime;
			}

			set {
				_leaseTime = value;
			}
		}

		public static TimeSpan RenewOnCallTime {
			get {
				return _renewOnCallTime;
			}
			
			set {
				_renewOnCallTime = value;
			}
		}

		public static TimeSpan SponsorshipTimeout {
			get {
				return _sponsorshipTimeout;
			}

			set {
				_sponsorshipTimeout = value;
			}
		}

		internal static void TrackLifetime (Identity identity)
		{
		}

		internal static void ManageLeases ()
		{
		}
	}
}
