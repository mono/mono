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

namespace System.Runtime.Remoting.Lifetime
{
	internal class Lease : MarshalByRefObject, ILease
	{
		DateTime _leaseExpireTime;
		LeaseState _currentState;
		TimeSpan _initialLeaseTime;
		TimeSpan _renewOnCallTime;
		TimeSpan _sponsorshipTimeout;
		ArrayList _sponsors;
		Queue _renewingSponsors;
		RenewalDelegate _renewalDelegate;

		delegate TimeSpan RenewalDelegate(ILease lease);

		public Lease()
		{
			_currentState = LeaseState.Initial;
			_initialLeaseTime = LifetimeServices.LeaseTime;
			_renewOnCallTime = LifetimeServices.RenewOnCallTime;
			_sponsorshipTimeout = LifetimeServices.SponsorshipTimeout;
			_leaseExpireTime = DateTime.Now + _initialLeaseTime;
		}

		public TimeSpan CurrentLeaseTime 
		{ 
			get { return _leaseExpireTime - DateTime.Now; }
		}

		public LeaseState CurrentState 
		{ 
			get { return _currentState; }
		}

		public void Activate()
		{
			// Called when then Lease is registered in the LeaseManager
			_currentState = LeaseState.Active;
		}

		public TimeSpan InitialLeaseTime 
		{ 
			get { return _initialLeaseTime; }
			set 
			{ 
				if (_currentState != LeaseState.Initial)
					throw new RemotingException ("InitialLeaseTime property can only be set when the lease is in initial state; state is " + _currentState + ".");

				_initialLeaseTime = value; 
				_leaseExpireTime = DateTime.Now + _initialLeaseTime;
				if (value == TimeSpan.Zero) _currentState = LeaseState.Null;
			}
		}

		public TimeSpan RenewOnCallTime 
		{ 
			get { return _renewOnCallTime; }
			set 
			{ 
				if (_currentState != LeaseState.Initial)
					throw new RemotingException ("RenewOnCallTime property can only be set when the lease is in initial state; state is " + _currentState + ".");

				_renewOnCallTime = value; 
			}
		}

		public TimeSpan SponsorshipTimeout 
		{
			get { return _sponsorshipTimeout; }
			set 
			{ 
				if (_currentState != LeaseState.Initial)
					throw new RemotingException ("SponsorshipTimeout property can only be set when the lease is in initial state; state is " + _currentState + ".");

				_sponsorshipTimeout = value; 
			}
		}

		public void Register (ISponsor obj)
		{
			Register (obj, TimeSpan.Zero);
		}

		public void Register (ISponsor obj, TimeSpan renewalTime)
		{
			if (_sponsors == null) 
			{
				lock (this) {
					if (_sponsors == null)
						_sponsors = new ArrayList();
				}
			}

			lock (_sponsors.SyncRoot) {
				_sponsors.Add (obj);
			}

			if (renewalTime != TimeSpan.Zero)
				Renew (renewalTime);
		}

		public TimeSpan Renew (TimeSpan renewalTime)
		{
			DateTime newTime = DateTime.Now + renewalTime;
			if (newTime > _leaseExpireTime) _leaseExpireTime = newTime;
			return CurrentLeaseTime;
		}

		public void Unregister (ISponsor obj)
		{
			if (_sponsors == null) return;
			
			lock (_sponsors.SyncRoot) {
				_sponsors.Remove (obj);
			}
		}

		internal void UpdateState ()
		{
			// Called by the lease manager to update the state of this lease,
			// basically for knowing if it has expired

			if (_currentState != LeaseState.Active) return;
			if (CurrentLeaseTime > TimeSpan.Zero) return;

			// Expired. Try to renew using sponsors.

			if (_sponsors != null)
			{
				_currentState = LeaseState.Renewing;
				lock (_sponsors.SyncRoot) {
					_renewingSponsors = new Queue (_sponsors);
				}
				CheckNextSponsor ();
			}
			else
				_currentState = LeaseState.Expired;
		}

		void CheckNextSponsor ()
		{
			if (_renewingSponsors.Count == 0) {
				_currentState = LeaseState.Expired;
				_renewingSponsors = null;
				return;
			}

			ISponsor nextSponsor = (ISponsor) _renewingSponsors.Peek();
			_renewalDelegate = new RenewalDelegate (nextSponsor.Renewal);
			IAsyncResult ar = _renewalDelegate.BeginInvoke (this, null, null);
			ThreadPool.RegisterWaitForSingleObject (ar.AsyncWaitHandle, new WaitOrTimerCallback (ProcessSponsorResponse), ar, _sponsorshipTimeout, true);
		}

		void ProcessSponsorResponse (object state, bool timedOut)
		{
			if (!timedOut)
			{
				try
				{
					IAsyncResult ar = (IAsyncResult)state;
					TimeSpan newSpan = _renewalDelegate.EndInvoke (ar);
					if (newSpan != TimeSpan.Zero)
					{
						Renew (newSpan);
						_currentState = LeaseState.Active;
						_renewingSponsors = null;
						return;
					}
				}
				catch { }
			}

			// Sponsor failed, timed out, or returned TimeSpan.Zero

			Unregister ((ISponsor) _renewingSponsors.Dequeue());	// Drop the sponsor
			CheckNextSponsor ();
		}
	}
}
