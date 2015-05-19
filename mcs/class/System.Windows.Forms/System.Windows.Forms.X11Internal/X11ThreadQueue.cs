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

		XEventQueue xqueue;
		PaintQueue paint_queue;
		ConfigureQueue configure_queue;
		ArrayList timer_list;
		Thread thread;
		bool quit_posted;
		bool dispatch_idle;
		bool need_dispatch_idle = true;
		object lockobj = new object ();

		static readonly int InitialXEventQueueSize = 128;
		static readonly int InitialHwndQueueSize = 50;

		public X11ThreadQueue (Thread thread)
		{
			xqueue = new XEventQueue (InitialXEventQueueSize);
			paint_queue = new PaintQueue (InitialHwndQueueSize);
			configure_queue = new ConfigureQueue (InitialHwndQueueSize);
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
			switch (xevent.type) {
			case XEventName.KeyPress:
			case XEventName.KeyRelease:
			case XEventName.ButtonPress:
			case XEventName.ButtonRelease:
				NeedDispatchIdle = true;
				break;
			case XEventName.MotionNotify:
				if (xqueue.Count > 0) {
					XEvent peek = xqueue.Peek ();
					if (peek.AnyEvent.type == XEventName.MotionNotify)
						return; // we've already got a pending motion notify.
				}

				// otherwise fall through and enqueue
				// the event.
				break;
			}

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

		public bool Dequeue (out XEvent xevent)
		{
		StartOver:
			bool got_xevent = false;

			lock (lockobj) {
				if (xqueue.Count > 0) {
					got_xevent = true;
					xevent = xqueue.Dequeue ();
				}
				else
					xevent = new XEvent (); /* not strictly needed, but mcs complains */
			}

			if (got_xevent) {
				if (xevent.AnyEvent.type == XEventName.Expose) {
#if spew
					Console.Write ("E");
					Console.Out.Flush ();
#endif
					X11Hwnd hwnd = (X11Hwnd)Hwnd.GetObjectFromWindow (xevent.AnyEvent.window);
					hwnd.AddExpose (xevent.AnyEvent.window == hwnd.ClientWindow,
							xevent.ExposeEvent.x, xevent.ExposeEvent.y,
							xevent.ExposeEvent.width, xevent.ExposeEvent.height);
					goto StartOver;
				}
				else if (xevent.AnyEvent.type == XEventName.ConfigureNotify) {
#if spew
					Console.Write ("C");
					Console.Out.Flush ();
#endif
					X11Hwnd hwnd = (X11Hwnd)Hwnd.GetObjectFromWindow (xevent.AnyEvent.window);
					hwnd.AddConfigureNotify (xevent);
					goto StartOver;
				}
				else {
#if spew
					Console.Write ("X");
					Console.Out.Flush ();
#endif
					/* it was an event we can deal with directly, return it */
					return true;
				}
			}
			else {
				if (paint_queue.Count > 0) {
					xevent = paint_queue.Dequeue ();
#if spew
					Console.Write ("e");
					Console.Out.Flush ();
#endif
					return true;
				}
				else if (configure_queue.Count > 0) {
					xevent = configure_queue.Dequeue ();
#if spew
					Console.Write ("c");
					Console.Out.Flush ();
#endif
					return true;
				}
			}

			if (dispatch_idle && need_dispatch_idle) {
				OnIdle (EventArgs.Empty);
				need_dispatch_idle = false;
			}

			lock (lockobj) {
				if (CountUnlocked > 0)
					goto StartOver;

				if (Monitor.Wait (lockobj, NextTimeout (), true)) {
					// the lock was reaquired before the
					// timeout.  meaning an event was
					// enqueued by X11Display.XEventThread.
					goto StartOver;
				}
				else {
					CheckTimers ();
					return false;
				}
			}
		}

		public void RemovePaint (Hwnd hwnd)
		{
			paint_queue.Remove (hwnd);
		}

		public void AddPaint (Hwnd hwnd)
		{
			paint_queue.Enqueue (hwnd);
		}

		public void AddConfigure (Hwnd hwnd)
		{
			configure_queue.Enqueue (hwnd);
		}

		public ConfigureQueue Configure {
			get { return configure_queue; }
		}

		public PaintQueue Paint {
			get { return paint_queue; }
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
			DateTime now = DateTime.UtcNow;

			foreach (Timer timer in timer_list) {
				int next = (int) (timer.Expires - now).TotalMilliseconds;
				if (next < 0)
					return 0; // Have a timer that has already expired

				if (next < timeout)
					timeout = next;
			}

			if (timeout < Timer.Minimum) {
				timeout = Timer.Minimum;
			}

			if (timeout == Int32.MaxValue)
				timeout = Timeout.Infinite;

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

		public abstract class HwndEventQueue {
			protected ArrayList hwnds;
#if DebugHwndEventQueue
			protected ArrayList stacks;
#endif
			public HwndEventQueue (int size)
			{
				hwnds = new ArrayList (size);
#if DebugHwndEventQueue
				stacks = new ArrayList (size);
#endif
			}

			public int Count {
				get { return hwnds.Count; }
			}

			public void Enqueue (Hwnd hwnd)
			{
				if (hwnds.Contains (hwnd)) {
#if DebugHwndEventQueue
					Console.WriteLine ("hwnds can only appear in the queue once.");
					Console.WriteLine (Environment.StackTrace);
					Console.WriteLine ("originally added here:");
					Console.WriteLine (stacks[hwnds.IndexOf (hwnd)]);
#endif

					return;
				}
				hwnds.Add(hwnd);
#if DebugHwndEventQueue
				stacks.Add(Environment.StackTrace);
#endif
			}

			public void Remove(Hwnd hwnd)
			{
#if DebugHwndEventQueue
				int index = hwnds.IndexOf(hwnd);
				if (index != -1)
					stacks.RemoveAt(index);
#endif
				hwnds.Remove(hwnd);
			}

			protected abstract XEvent Peek ();

			public virtual XEvent Dequeue ()
			{
				if (hwnds.Count == 0)
					throw new Exception ("Attempt to dequeue empty queue.");

				return Peek ();
			}
		}


		public class ConfigureQueue : HwndEventQueue
		{
			public ConfigureQueue (int size) : base (size)
			{
			}

			protected override XEvent Peek ()
			{
				X11Hwnd hwnd = (X11Hwnd)hwnds[0];

				XEvent xevent = new XEvent ();
				xevent.AnyEvent.type = XEventName.ConfigureNotify;

				xevent.ConfigureEvent.window = hwnd.ClientWindow;
				xevent.ConfigureEvent.x = hwnd.X;
				xevent.ConfigureEvent.y = hwnd.Y;
				xevent.ConfigureEvent.width = hwnd.Width;
				xevent.ConfigureEvent.height = hwnd.Height;
				
				return xevent;
			}

			public override XEvent Dequeue ()
			{
				XEvent xev = base.Dequeue ();


				hwnds.RemoveAt(0);
#if DebugHwndEventQueue
				stacks.RemoveAt(0);
#endif

				return xev;
			}
		}

		public class PaintQueue : HwndEventQueue
		{
			public PaintQueue (int size) : base (size)
			{
			}

			protected override XEvent Peek ()
			{
				X11Hwnd hwnd = (X11Hwnd)hwnds[0];

				XEvent xevent = new XEvent ();

				xevent.AnyEvent.type = XEventName.Expose;

				if (hwnd.PendingExpose) {
					xevent.ExposeEvent.window = hwnd.ClientWindow;
				} else {
					xevent.ExposeEvent.window = hwnd.WholeWindow;
					xevent.ExposeEvent.x = hwnd.nc_invalid.X;
					xevent.ExposeEvent.y = hwnd.nc_invalid.Y;
					xevent.ExposeEvent.width = hwnd.nc_invalid.Width;
					xevent.ExposeEvent.height = hwnd.nc_invalid.Height;
				}

				return xevent;
			}

			// don't override Dequeue like ConfigureQueue does.
		}

		/* a circular queue for holding X events for processing by GetMessage */
		private class XEventQueue {

			XEvent[] xevents;
			int head;
			int tail;
			int size;
			
			public XEventQueue (int initial_size)
			{
				if (initial_size % 2 != 0)
					throw new Exception ("XEventQueue must be a power of 2 size");

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
				tail = (tail + 1) & (xevents.Length - 1);
				size++;
			}

			public XEvent Dequeue ()
			{
				if (size < 1)
					throw new Exception ("Attempt to dequeue empty queue.");

				XEvent res = xevents [head];
				head = (head + 1) & (xevents.Length - 1);
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

				if (head + size > xevents.Length) {
					Array.Copy (xevents, head, na, 0, xevents.Length - head);
					Array.Copy (xevents, 0, na, xevents.Length - head, head + size - xevents.Length);
				}
				else {
					Array.Copy (xevents, head, na, 0, size);
				}

				xevents = na;
				head = 0;
				tail = head + size;
			}
		}
	}
}

