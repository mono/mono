//
// System.Web.TimeoutManager
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Threading;
using System.Web.Configuration;

namespace System.Web
{
	class StepTimeout
	{
	}
	
	class TimeoutManager
	{
		Timer timer;
		Hashtable contexts;

		public TimeoutManager ()
		{
			timer = new Timer (new TimerCallback (CheckTimeouts), null, 0, 15000);
			contexts = Hashtable.Synchronized (new Hashtable ());
		}

		public void Add (HttpContext context)
		{
			object value = contexts [context];
			if (value == null) {
				value = Thread.CurrentThread;
			} else if (value is Thread) {
				ArrayList list = new ArrayList ();
				list.Add (value);
				list.Add (Thread.CurrentThread);
				value = list;
			} else {
				ArrayList list = (ArrayList) value;
				list.Add (Thread.CurrentThread);
				value = list;
			}

			lock (this) {
				contexts [context] = value;
			}
		}

		public Thread Remove (HttpContext context)
		{
			object value = contexts [context];
			if (value == null)
				return null;

			if (value is Thread) {
				lock (this) {
					contexts.Remove (context);
				}
				return (Thread) value;
			}
			
			ArrayList list = (ArrayList) value;
			Thread result = null;
			if (list.Count > 0) {
				result = (Thread) list [list.Count - 1];
				list.RemoveAt (list.Count - 1);
			}

			if (list.Count == 0) {
				lock (this) {
					contexts.Remove (context);
				}
			}

			return result;
		}

		void CheckTimeouts (object state)
		{
			if (contexts.Count == 0) {
				return;
			}

			DateTime now = DateTime.Now;
			ArrayList clist = new ArrayList ();

			lock (this) { // The lock prevents Keys enumerator from being out of synch
				clist.AddRange (contexts.Keys);
			}

			foreach (HttpContext context in clist) {
				if (!context.CheckIfTimeout (now))
					continue;

				Thread thread = Remove (context);
				if (thread != null) // Only if context is removed right after the lock
					thread.Abort (new StepTimeout ());
			}
		}
	}
}

