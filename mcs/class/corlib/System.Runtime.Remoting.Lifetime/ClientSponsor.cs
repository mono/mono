//
// System.Runtime.Remoting.Lifetime.ClientSponsor.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
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
using System.Collections;
using System.Runtime.Remoting.Lifetime;

namespace System.Runtime.Remoting.Lifetime {

#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public class ClientSponsor : MarshalByRefObject, ISponsor
	{
		TimeSpan renewal_time;
		Hashtable registered_objects = new Hashtable ();

		public ClientSponsor ()
		{
			renewal_time = new TimeSpan (0, 2, 0); // default is 2 mins
		}

		public ClientSponsor (TimeSpan renewalTime)
		{
			renewal_time = renewalTime;
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
			foreach (MarshalByRefObject obj in registered_objects.Values)
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
			if (registered_objects.ContainsKey (obj)) return false;
			ILease lease = obj.GetLifetimeService () as ILease;
			if (lease == null) return false;
			lease.Register (this);
			registered_objects.Add (obj,obj);
			return true;
		}

		public TimeSpan Renewal (ILease lease)
		{
			return renewal_time;
		}
		       
		public void Unregister (MarshalByRefObject obj)
		{
			if (!registered_objects.ContainsKey (obj)) return;
			ILease lease = obj.GetLifetimeService () as ILease;
			lease.Unregister (this);
			registered_objects.Remove (obj);
		}
	}
}
