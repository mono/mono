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
using System.Threading;
using System.ComponentModel;

namespace System.Windows.Forms {

	public class Timer : Component {

		private bool enabled;
		private IContainer container;
		private int interval = 100;
		private DateTime expires;

		public Timer ()
		{
			enabled = false;
		}

		public Timer (IContainer container) : this ()
		{
			container.Add (this);
		}

		public bool Enabled {
			get {
				return enabled;
			}
			set {
				if (value != enabled) {
					enabled = value;
					if (value) {
						XplatUI.SetTimer (this);
					} else {
						XplatUI.KillTimer (this);
					}
				}
			}
		}

		public int Interval {
			get {
				return interval;
			}
			set {
				interval = value;
				// Use AddTicks so we get some rounding
				expires = DateTime.Now.AddMilliseconds (interval);
			}
		}

		public void Start ()
		{
			Enabled = true;
		}

		public void Stop ()
		{
			Enabled = false;
		}

		internal DateTime Expires {
			get {
				return expires;
			}
		}

		public event EventHandler Tick;

		public override string ToString ()
		{
			return base.ToString () + ", Interval: " + Interval;
		}

		internal void Update ()
		{
			expires = DateTime.Now.AddMilliseconds (interval);
		}

		internal void FireTick ()
		{
			OnTick (EventArgs.Empty);
		}


		protected virtual void OnTick (EventArgs e)
		{
			lock (this) {
				if (Tick != null)
					Tick (this, e);
			}
		}

		protected override void Dispose (bool disposing)
		{
			Enabled = false;
		}

		private void TickHandler (object sender, EventArgs e)
		{
			OnTick (e);
		}
	}
}

