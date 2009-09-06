//
// WeakEventManager.cs
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
// (C) 2007 Novell, Inc.
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

using System.Collections;
using System.Windows.Threading;

namespace System.Windows {

	public abstract class WeakEventManager : DispatcherObject
	{
		Hashtable sourceData;

		protected WeakEventManager ()
		{
			sourceData = new Hashtable ();
		}

		protected IDisposable ReadLock {
			get { throw new NotImplementedException (); }
		}

		protected IDisposable WriteLock {
			get { throw new NotImplementedException (); }
		}

		protected object this [object source] {
			get { return sourceData[source]; }
			set { sourceData[source] = value; }
		}

		protected void DeliverEvent (object sender, EventArgs args)
		{
			DeliverEventToList (sender, args, (ListenerList)this[sender]);
		}

		protected void DeliverEventToList (object sender, EventArgs args, ListenerList list)
		{
			for (int i = 0; i < list.Count; i ++) {
				IWeakEventListener listener = list[i];
				listener.ReceiveWeakEvent (GetType(), sender, args);
			}
		}

		protected void ProtectedAddListener (object source, IWeakEventListener listener)
		{
			ListenerList list = sourceData[source] as ListenerList;
			if (list != null)
				list.Add (listener);
		}

		protected void ProtectedRemoveListener (object source, IWeakEventListener listener)
		{
			ListenerList list = sourceData[source] as ListenerList;
			if (list != null)
				list.Remove (listener);
		}

		protected virtual bool Purge (object source, object data, bool purgeAll)
		{
			throw new NotImplementedException ();
		}

		protected void Remove (object source)
		{
			throw new NotImplementedException ();
		}

		protected void ScheduleCleanup ()
		{
			throw new NotImplementedException ();
		}

		protected abstract void StartListening (object source);
		protected abstract void StopListening (object source);


		protected static WeakEventManager GetCurrentManager (Type managerType)
		{
			throw new NotImplementedException ();
		}

		protected static void SetCurrentManager (Type managerType, WeakEventManager manager)
		{
			throw new NotImplementedException ();
		}


		protected class ListenerList
		{
			public ListenerList ()
			{
				throw new NotImplementedException ();
			}

			public ListenerList (int capacity)
			{
				throw new NotImplementedException ();
			}

			public int Count {
				get { throw new NotImplementedException (); }
			}

			public static ListenerList Empty {
				get { throw new NotImplementedException (); }
			}

			public bool IsEmpty {
				get { throw new NotImplementedException (); }
			}

			public IWeakEventListener this[int index] {
				get { throw new NotImplementedException (); }
			}

			public void Add (IWeakEventListener listener)
			{
				throw new NotImplementedException ();
			}

			public bool BeginUse ()
			{
				throw new NotImplementedException ();
			}

			public WeakEventManager.ListenerList Clone ()
			{
				throw new NotImplementedException ();
			}

			public void EndUse ()
			{
				throw new NotImplementedException ();
			}

			public static bool PrepareForWriting (ref WeakEventManager.ListenerList list)
			{
				throw new NotImplementedException ();
			}

			public bool Purge ()
			{
				throw new NotImplementedException ();
			}

			public void Remove (IWeakEventListener listener)
			{
				throw new NotImplementedException ();
			}
		}
	}

}
