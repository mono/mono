//
// System.Runtime.Remoting.Lifetime.ClientSponsor.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;
using System.Collections;
using System.Runtime.Remoting.Lifetime;

namespace System.Runtime.Remoting.Lifetime {

	public class ClientSponsor : MarshalByRefObject, ISponsor
	{
		TimeSpan renewal_time;
		ArrayList registered_objects = new ArrayList ();

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

		public void Close ()
		{
			foreach (MarshalByRefObject obj in registered_objects)
			{
				ILease lease = obj.GetLifetimeService () as ILease;
				lease.Unregister (this);
			}
			registered_objects.Clear ();
		}

		~ClientSponsor ()
		{
			Close ();
		}

		public override object InitializeLifetimeService ()
		{
			return base.InitializeLifetimeService ();
		}

		public bool Register (MarshalByRefObject obj)
		{
			if (registered_objects.Contains (obj)) return false;
			ILease lease = obj.GetLifetimeService () as ILease;
			if (lease == null) return false;
			lease.Register (this);
			registered_objects.Add (obj);
			return true;
		}

		public TimeSpan Renewal (ILease lease)
		{
			return renewal_time;
		}
		       
		public void Unregister (MarshalByRefObject obj)
		{
			if (!registered_objects.Contains (obj)) return;
			ILease lease = obj.GetLifetimeService () as ILease;
			lease.Unregister (this);
			registered_objects.Remove (obj);
		}
	}
}
