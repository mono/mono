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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)


using System;
using System.ComponentModel;

namespace System.Windows.Forms {

	public class Timer : Component {

		private System.Timers.Timer timer;
		private IContainer container;

		public Timer ()
		{
			timer = new System.Timers.Timer ();
			timer.Elapsed += new System.Timers.ElapsedEventHandler (ElapsedEventHandler);
		}

		public Timer (IContainer container) : this ()
		{
			container.Add (this);
		}

		public bool Enabled {
			get { return timer.Enabled; }
			set { timer.Enabled = value; }
		}

		public int Interval {
			get { return (int) timer.Interval; }
			set { timer.Interval = (int) value; }
		}

		public void Start ()
		{
			timer.Start ();
		}

		public void Stop ()
		{
			timer.Stop ();
		}

		public event EventHandler Tick;

		public override string ToString ()
		{
			return base.ToString () + ", Interval: " + Interval;
		}

		protected virtual void OnTick (EventArgs e)
		{
			if (Tick != null)
				Tick (this, e);
		}

		protected override void Dispose (bool disposing)
		{
			Enabled = false;
			timer.Dispose ();
		}

		private void TickHandler (object sender, EventArgs e)
		{
			OnTick (e);
		}

		private void ElapsedEventHandler (object sender, System.Timers.ElapsedEventArgs e)
		{
			Control.BeginInvokeInternal (new EventHandler (TickHandler), new object [] { this, e });
		}
		
	}
}

