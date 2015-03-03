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
// Copyright (c) 2004-2006 Novell, Inc.
//
// System.Windows.Forms.XEventQueue
//
// Authors:
//  Jackson Harper (jackson@ximian.com)
//  Peter Dennis Bartok (pbartok@novell.com)
//

using System;
using System.Threading;
using System.Collections;

namespace System.Windows.Forms {

	internal class XEventQueue {

		private XQueue		xqueue;
		private XQueue		lqueue;	// Events inserted from threads other then the main X thread
		private PaintQueue	paint;	// Paint-only queue
		internal ArrayList	timer_list;
		private Thread		thread;
		private bool            dispatch_idle;

		private static readonly int InitialXEventSize = 100;
		private static readonly int InitialLXEventSize = 10;
		private static readonly int InitialPaintSize = 50;

		public XEventQueue (Thread thread) {
			xqueue = new XQueue (InitialXEventSize);
			lqueue = new XQueue (InitialLXEventSize);
			paint = new PaintQueue(InitialPaintSize);
			timer_list = new ArrayList ();
			this.thread = thread;
			this.dispatch_idle = true;
		}

		public int Count {
			get {
				lock (lqueue) {
					return xqueue.Count + lqueue.Count;
				}
			}
		}

		public PaintQueue Paint {
			get {
				return paint;
			}
		}

		public Thread Thread {
			get {
				return thread;
			}
		}

		public void Enqueue (XEvent xevent)
		{
			if (Thread.CurrentThread != thread) {
				Console.WriteLine ("Hwnd.Queue.Enqueue called from a different thread without locking.");
				Console.WriteLine (Environment.StackTrace);
			}

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
			if (Thread.CurrentThread != thread) {
				Console.WriteLine ("Hwnd.Queue.Dequeue called from a different thread without locking.");
				Console.WriteLine (Environment.StackTrace);
			}

			if (xqueue.Count == 0) {
				lock (lqueue) {
					return lqueue.Dequeue ();
				}
			}
			return xqueue.Dequeue ();
		}

		public XEvent Peek()
		{
			if (Thread.CurrentThread != thread) {
				Console.WriteLine ("Hwnd.Queue.Peek called from a different thread without locking.");
				Console.WriteLine (Environment.StackTrace);
			}

			if (xqueue.Count == 0) {
				lock (lqueue) {
					return lqueue.Peek ();
				}
			}				
			return xqueue.Peek();
		}

		public bool DispatchIdle {
			get {
				return dispatch_idle;
			}
			set {
				dispatch_idle = value;
			}
		}

		public class PaintQueue {

			private ArrayList	hwnds;
			private XEvent		xevent;
			
			public PaintQueue (int size) {
				hwnds = new ArrayList(size);
				xevent = new XEvent();
				xevent.AnyEvent.type = XEventName.Expose;
			}

			public int Count {
				get { return hwnds.Count; }
			}

			public void Enqueue (Hwnd hwnd) {
				hwnds.Add(hwnd);
			}

			public void Remove(Hwnd hwnd) {
				if (!hwnd.expose_pending && !hwnd.nc_expose_pending) {
					hwnds.Remove(hwnd);
				}
			}

			public XEvent Dequeue () {
				Hwnd		hwnd;
				IEnumerator	next;

				if (hwnds.Count == 0) {
					xevent.ExposeEvent.window = IntPtr.Zero;
					return xevent;
				}

				next = hwnds.GetEnumerator();
				next.MoveNext();
				hwnd = (Hwnd)next.Current;

				// We only remove the event from the queue if we have one expose left since
				// a single 'entry in our queue may be for both NC and Client exposed
				if ( !(hwnd.nc_expose_pending && hwnd.expose_pending)) {
					hwnds.Remove(hwnd);
				}
				if (hwnd.expose_pending) {
					xevent.ExposeEvent.window = hwnd.client_window;
#if not
					xevent.ExposeEvent.x = hwnd.invalid.X;
					xevent.ExposeEvent.y = hwnd.invalid.Y;
					xevent.ExposeEvent.width = hwnd.invalid.Width;
					xevent.ExposeEvent.height = hwnd.invalid.Height;
#endif
					return xevent;
				} else {
					xevent.ExposeEvent.window = hwnd.whole_window;
					xevent.ExposeEvent.x = hwnd.nc_invalid.X;
					xevent.ExposeEvent.y = hwnd.nc_invalid.Y;
					xevent.ExposeEvent.width = hwnd.nc_invalid.Width;
					xevent.ExposeEvent.height = hwnd.nc_invalid.Height;
					return xevent;
				}
			}
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

			public XEvent Peek() {
				if (size < 1) {
					throw new Exception ("Attempt to peek at empty queue");
				}
				return xevents[head];
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

