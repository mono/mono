//
// System.Web.TimeoutManager
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

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
			contexts = Hashtable.Synchronized (new Hashtable ());
			timer = new Timer (new TimerCallback (CheckTimeouts), null, 0, 15000);
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

			DateTime now = DateTime.UtcNow;
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

