//
// System.Runtime.Remoting.Identity.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

using System;
using System.Threading;
using System.Collections;
using System.Runtime.Remoting;

namespace System.Runtime.Remoting.Lifetime
{
	internal class LeaseManager
	{
		ArrayList _objects = new ArrayList();
		Timer _timer = null;

		public void SetPollTime (TimeSpan timeSpan)
		{
			lock (_objects.SyncRoot) 
			{
				if (_timer != null)
					_timer.Change (timeSpan,timeSpan);
			}
		}

		public void TrackLifetime (ServerIdentity identity)
		{
			lock (_objects.SyncRoot)
			{
				identity.Lease.Activate();
				_objects.Add (identity);

				if (_timer == null) StartManager();
			}
		}

		public void StopTrackingLifetime (ServerIdentity identity)
		{
			lock (_objects.SyncRoot)
			{
				_objects.Remove (identity);
			}
		}

		public void StartManager()
		{
			_timer = new Timer (new TimerCallback (ManageLeases), null, LifetimeServices.LeaseManagerPollTime,LifetimeServices.LeaseManagerPollTime);
		}

		public void StopManager()
		{
			_timer.Dispose();
			_timer = null;
		}

		public void ManageLeases(object state)
		{
			lock (_objects.SyncRoot)
			{
				int n=0;
				while (n < _objects.Count)
				{
					ServerIdentity ident = (ServerIdentity)_objects[n];
					ident.Lease.UpdateState();
					if (ident.Lease.CurrentState == LeaseState.Expired)
					{
						_objects.RemoveAt (n);
						ident.OnLifetimeExpired ();
					}
					else
						n++;
				}

				if (_objects.Count == 0) 
					StopManager();
			}
		}
	}
}
