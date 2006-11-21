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
// Authors:
//  Jackson Harper (jackson@ximian.com)
//  Peter Dennis Bartok (pbartok@novell.com)
//  Chris Toshok (toshok@ximian.com)
//

using System;
using System.Threading;
using System.Collections;

namespace System.Windows.Forms.X11Internal {

	internal class X11ThreadQueue {

		XQueue xqueue;
		PaintQueue paint_queue;
		ArrayList timer_list;
		Thread thread;
		bool quit_posted;
		bool dispatch_idle;
		bool need_dispatch_idle = true;
		object lockobj = new object ();

		static readonly int InitialXEventSize = 100;
		static readonly int InitialPaintSize = 50;

		public X11ThreadQueue (Thread thread)
		{
			xqueue = new XQueue (InitialXEventSize);
			paint_queue = new PaintQueue(InitialPaintSize);
			timer_list = new ArrayList ();
			this.thread = thread;
			this.quit_posted = false;
			this.dispatch_idle = true;
		}

		public int CountUnlocked {
			get { return xqueue.Count + paint_queue.Count; }
		}

		public Thread Thread {
			get { return thread; }
		}

		public void EnqueueUnlocked (XEvent xevent)
		{
			xqueue.Enqueue (xevent);
			// wake up any thread blocking in DequeueUnlocked
			Monitor.PulseAll (lockobj);
		}

		public void Enqueue (XEvent xevent)
		{
			lock (lockobj) {
				EnqueueUnlocked (xevent);
			}
		}

		public bool DequeueUnlocked (out XEvent xevent)
		{
		try_again:
			if (xqueue.Count > 0) {
				xevent = xqueue.Dequeue ();
				return true;
			}

			if (paint_queue.Count > 0) {
				xevent = paint_queue.Dequeue ();
				return true;
			}

			// both queues are empty.  go to sleep until NextTimeout
			// (or until there's an event to handle).

			if (dispatch_idle && need_dispatch_idle) {
				OnIdle (EventArgs.Empty);
				need_dispatch_idle = false;
			}

			if (Monitor.Wait (lockobj, NextTimeout (), true)) {
				/* the lock was reaquired before timeout.
				   i.e. we have an event now */
				goto try_again;
			}
			else {
				xevent = new XEvent ();
				return false;
			}
		}

		public bool Dequeue (out XEvent xevent)
		{
			lock (lockobj) {
				return DequeueUnlocked (out xevent);
			}
		}

		public void RemovePaint (Hwnd hwnd)
		{
			lock (lockobj) {
				paint_queue.Remove (hwnd);
			}
		}

		public void AddPaint (Hwnd hwnd)
		{
			lock (lockobj) {
				Console.WriteLine ("adding paint event");
				paint_queue.Enqueue (hwnd);
				// wake up any thread blocking in DequeueUnlocked
				Monitor.PulseAll (lockobj);
			}
		}

		public void Lock ()
		{
			Monitor.Enter (lockobj);
		}

		public void Unlock ()
		{
			Monitor.Exit (lockobj);
		}

		private int NextTimeout ()
		{
			int timeout = Int32.MaxValue; 
			DateTime now = DateTime.Now;

			foreach (Timer timer in timer_list) {
				int next = (int) (timer.Expires - now).TotalMilliseconds;
				if (next < 0) {
					return 0; // Have a timer that has already expired
				}

				if (next < timeout) {
					timeout = next;
				}
			}
			if (timeout < Timer.Minimum) {
				timeout = Timer.Minimum;
			}

#if false
			if (timeout > 1000)
				timeout = 1000;
#endif
			return timeout;
		}

		public void CheckTimers ()
		{
			int count;
			DateTime now = DateTime.UtcNow;

			count = timer_list.Count;

			if (count == 0)
				return;

			for (int i = 0; i < timer_list.Count; i++) {
				Timer timer;

				timer = (Timer) timer_list [i];

				if (timer.Enabled && timer.Expires <= now) {
					timer.Update (now);
					timer.FireTick ();
				}
			}
		}

		public void SetTimer (Timer timer)
		{
			lock (lockobj) {
				timer_list.Add (timer);

				// we need to wake up any thread waiting in DequeueUnlocked,
				// since it might need to wait for a different amount of time.
				Monitor.PulseAll (lockobj);
			}

		}

		public void KillTimer (Timer timer)
		{
			lock (lockobj) {
				timer_list.Remove (timer);

				// we need to wake up any thread waiting in DequeueUnlocked,
				// since it might need to wait for a different amount of time.
				Monitor.PulseAll (lockobj);
			}
		}

		public event EventHandler Idle;
		public void OnIdle (EventArgs e)
		{
			if (Idle != null)
				Idle (thread, e);
		}

		public bool NeedDispatchIdle {
			get { return need_dispatch_idle; }
			set { need_dispatch_idle = value; }
		}

		public bool DispatchIdle {
			get { return dispatch_idle; }
			set { dispatch_idle = value; }
		}

		public bool PostQuitState {
			get { return quit_posted; }
			set { quit_posted = value; }
		}

		public class PaintQueue {

			private ArrayList	hwnds;
			
			public PaintQueue (int size) {
				hwnds = new ArrayList(size);
			}

			public int Count {
				get { return hwnds.Count; }
			}

			public void Enqueue (Hwnd hwnd)
			{
				hwnds.Add(hwnd);
			}

			public void Remove(Hwnd hwnd)
			{
				if (!hwnd.expose_pending && !hwnd.nc_expose_pending) {
					hwnds.Remove(hwnd);
				}
			}

			public XEvent Peek ()
			{
				if (hwnds.Count == 0)
					throw new Exception ("Attempt to dequeue empty queue.");

				Hwnd hwnd = (Hwnd)hwnds[0];

				XEvent xevent = new XEvent ();
				xevent.AnyEvent.type = XEventName.Expose;

				if (hwnd.expose_pending) {
					xevent.ExposeEvent.window = hwnd.client_window;
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

			public XEvent Dequeue ()
			{
				if (hwnds.Count == 0)
					throw new Exception ("Attempt to dequeue empty queue.");

				// populate the xevent
				XEvent xevent = Peek ();

				Hwnd hwnd = (Hwnd)hwnds[0];

				// We only remove the event from the queue if we have one expose left since
				// a single entry in our queue may be for both NC and Client exposed
				if ( !(hwnd.nc_expose_pending && hwnd.expose_pending))
					hwnds.RemoveAt(0);

				return xevent;
			}
		}

		private class XQueue {

			private XEvent [] xevents;
			private int head;
			private int tail;
			private int size;
			
			public XQueue (int initial_size)
			{
				xevents = new XEvent [initial_size];
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

			public XEvent Peek()
			{
				if (size < 1)
					throw new Exception ("Attempt to peek at empty queue.");

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

