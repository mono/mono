//
// System.Windows.Forms.XEventQueue
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// Copyright (C) Novell, Inc (http://www.novell.com)
//

using System;
using System.Threading;
using System.Collections;


namespace System.Windows.Forms {

	internal class XEventQueue {

		private XQueue xqueue;
		private XQueue lqueue;	// Events inserted from threads other then the main X thread

		private static readonly int InitialXEventSize = 50;
		private static readonly int InitialLXEventSize = 10;

		public XEventQueue ()
		{
			xqueue = new XQueue (InitialXEventSize);
			lqueue = new XQueue (InitialLXEventSize);
		}

		public int Count {
			get { return xqueue.Count + lqueue.Count; }
		}

		public void Enqueue (XEvent xevent)
		{
			xqueue.Enqueue (xevent);
		}

		public void EnqueueLocked (XEvent xevent)
		{
			lock (lqueue) {
				lqueue.Enqueue (xevent);
			}
		}

		public XEvent Dequeue ()
		{
			if (xqueue.Count == 0) {
				lock (lqueue) {
					return lqueue.Dequeue ();
				}
			}
			return xqueue.Dequeue ();
		}

		private class XQueue {

			private XEvent [] xevents;
			private int head;
			private int tail;
			private int size;
			
			public XQueue (int size)
			{
				xevents = new XEvent [size];
			}

			public int Count {
				get { return size; }
			}

			public void Enqueue (XEvent xevent)
			{
				if (size == xevents.Length)
					Grow ();
				
				xevents [tail] = xevent;
				tail = (tail + 1) % xevents.Length;
				size++;
			}

			public XEvent Dequeue ()
			{
				if (size < 1)
					throw new Exception ("Attempt to dequeue empty queue.");
				XEvent res = xevents [head];
				head = (head + 1) % xevents.Length;
				size--;
				return res;
			}

			private void Grow ()
			{
				int newcap = (xevents.Length * 2);
				XEvent [] na = new XEvent [newcap];
				xevents.CopyTo (na, 0);
				xevents = na;
				head = 0;
				tail = head + size;
			}
		}
	}
}

