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
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Miguel de Icaza (miguel@novell.com)
//
using System;
using System.Collections;
using System.Threading;

namespace System.Windows.Threading {

	public class DispatcherTimer {
		DispatcherPriority priority;
		Dispatcher target_dispatcher;
		long interval;
		EventHandler callback;
		Timer timer;
		object tag;
		
		public DispatcherTimer ()
			: this (DispatcherPriority.Background, Dispatcher.CurrentDispatcher)
		{
		}

		public DispatcherTimer (DispatcherPriority priority)
			: this (priority, Dispatcher.CurrentDispatcher)
		{
		}
	       
		public DispatcherTimer (DispatcherPriority priority, Dispatcher dispatcher)
			: this (TimeSpan.Zero, priority, null, dispatcher)
		{
		}
	       
		public DispatcherTimer (TimeSpan interval, DispatcherPriority priority,
					EventHandler callback, Dispatcher dispatcher)
		{
			this.priority = priority;
			this.target_dispatcher = dispatcher;
			this.interval = interval.Ticks;
			this.callback = callback;
		}

		public void Start ()
		{
			if (timer == null){
				long repeat_interval = interval;
				if (repeat_interval == 0)
					repeat_interval = 1;
				timer = new Timer (new TimerCallback (timer_tick),
							null, new TimeSpan (interval), 
							new TimeSpan (repeat_interval));
			}
		}

		void timer_tick (object state)
		{
			target_dispatcher.BeginInvoke (priority, (ThreadStart) delegate {
				EventHandler h = Tick;
				if (h != null)
					h (this, EventArgs.Empty);
				if (callback != null)
					callback (this, EventArgs.Empty);
			});
		}

		public void Stop ()
		{
			if (timer == null)
				return;
			
			timer.Dispose ();
			timer = null;
		}
		
		public Dispatcher Dispatcher {
			get {
				return target_dispatcher;
			}
		}

		public TimeSpan Interval {
			get {
				return new TimeSpan (interval);
			}

			set {
				if (interval == value.Ticks)
					return;

				interval = value.Ticks;
				
				if (timer != null)
					timer.Change (new TimeSpan (interval),
							new TimeSpan (interval));
			}
		}

		public bool IsEnabled {
			get {
				return timer != null;
			}

			set {
				if (value && timer == null)
					Start ();
				if (value == false && timer != null)
					Stop ();
			}
		}

		public object Tag {
			get {
				return tag;
			}

			set {
				tag = value;
			}
		}
		public event EventHandler Tick; 
	}
}
