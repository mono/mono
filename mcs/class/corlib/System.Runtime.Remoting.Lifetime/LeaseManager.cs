//
// System.Runtime.Remoting.Identity.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
			Timer t = _timer;
			_timer = null;
			t.Dispose();
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
