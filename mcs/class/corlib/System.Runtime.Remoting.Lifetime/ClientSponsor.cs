//
// System.Runtime.Remoting.Lifetime.ClientSponsor.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;
using System.Runtime.Remoting.Lifetime;

namespace System.Runtime.Remoting.Lifetime {

	public class ClientSponsor : MarshalByRefObject, ISponsor
	{
		TimeSpan renewal_time;

		public ClientSponsor ()
		{
			renewal_time = new TimeSpan (0, 2, 0); // default is 2 mins
		}

		public ClientSponsor (TimeSpan time)
		{
			renewal_time = time;
		}

		public TimeSpan RenewalTime {
			get {
				return renewal_time;
			}

			set {
				renewal_time = value;
			}
		}

		[MonoTODO]
		public void Close ()
		{
		}

		[MonoTODO]
		~ClientSponsor ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object InitializeLifetimeService ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Register (MarshalByRefObject obj)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public TimeSpan Renewal (ILease lease)
		{
			throw new NotImplementedException ();
		}
		       
		[MonoTODO]
		public void Unregister (MarshalByRefObject obj)
		{
			throw new NotImplementedException ();
		}
	}
}
