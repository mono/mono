//
// System.Runtime.Remoting.Lifetime.LifetimeServices.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;

namespace System.Runtime.Remoting.Lifetime {

	//LAMESPEC: MS docs don't say that this class is sealed.
	public sealed class LifetimeServices
	{
		[MonoTODO]
		public LifetimeServices ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static TimeSpan LeaseManagerPollTime {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static TimeSpan LeaseTime {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static TimeSpan RenewOnCallTime {
			get {
				throw new NotImplementedException ();
			}
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static TimeSpan SponsorshipTimeout {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}
	}
}
