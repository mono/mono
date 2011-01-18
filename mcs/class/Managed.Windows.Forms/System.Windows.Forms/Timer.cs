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
	[DefaultProperty("Interval")]
	[DefaultEvent("Tick")]
	[ToolboxItemFilter("System.Windows.Forms", ToolboxItemFilterType.Allow)]
	public class Timer : Component {

		private bool enabled;
		private int interval = 100;
		private DateTime expires;
		internal Thread thread;
		internal bool Busy;
		internal IntPtr window;
		object control_tag;

		internal static readonly int Minimum = 15;

		public Timer ()
		{
			enabled = false;
		}

		public Timer (IContainer container) : this ()
		{
			container.Add (this);
		}

		[DefaultValue (false)]
		public virtual bool Enabled {
			get {
				return enabled;
			}
			set {
				if (value != enabled) {
					enabled = value;
					if (value) {
						// Use AddTicks so we get some rounding
						expires = DateTime.UtcNow.AddMilliseconds (interval > Minimum ? interval : Minimum);

						thread = Thread.CurrentThread;
						XplatUI.SetTimer (this);
					} else {
						XplatUI.KillTimer (this);
						thread = null;
					}
				}
			}
		}

		[DefaultValue (100)]
		public int Interval {
			get {
				return interval;
			}
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("Interval", string.Format ("'{0}' is not a valid value for Interval. Interval must be greater than 0.", value));

				if (interval == value) {
					return;
				}
				
				interval = value;
								
				// Use AddTicks so we get some rounding
				expires = DateTime.UtcNow.AddMilliseconds (interval > Minimum ? interval : Minimum);
									
				if (enabled == true) {				
					XplatUI.KillTimer (this);
					XplatUI.SetTimer (this);
				}
			}
		}
		
		[Localizable(false)]
		[Bindable(true)]
		[TypeConverter(typeof(StringConverter))]
		[DefaultValue(null)]
		[MWFCategory("Data")]
		public object Tag {
			get {
				return control_tag;
			}

			set {
				control_tag = value;
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

		internal void Update (DateTime update)
		{
			expires = update.AddMilliseconds (interval > Minimum ? interval : Minimum);
		}

		internal void FireTick ()
		{
			OnTick (EventArgs.Empty);
		}


		protected virtual void OnTick (EventArgs e)
		{
			if (Tick != null)
				Tick (this, e);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			Enabled = false;
		}

		internal void TickHandler (object sender, EventArgs e)
		{
			OnTick (e);
		}
	}
}

