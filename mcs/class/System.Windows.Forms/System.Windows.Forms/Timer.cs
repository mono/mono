//
// System.Windows.Forms.Timer
//
// Author:
//	stubbed out by Jackson Harper (jackson@latitudegeo.com)
//	Dennis Hayes (dennish@raytek.com)
//	Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) 2002/3 Ximian, Inc
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

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections;

namespace System.Windows.Forms {

	// <summary>
	//	Represents a timer that raises an event at user-defined intervals.
	// </summary>
	public class Timer : Component {

		private bool enabled = false;
		private int  interval = 100;
		private uint timerid = 0;
		private GCHandle timerHandle;
		private Win32.TimerProc proc;

		public Timer(){
		}

		public Timer( IContainer container ) {
			container.Add ( this );
		}

		public virtual bool Enabled {
			get { 
				return enabled;
			}
			set { 
				enabled = value;
				if ( enabled ) {
					if ( timerid != 0 )
						Win32.KillTimer ( IntPtr.Zero , timerid );

					if ( !timerHandle.IsAllocated )
						timerHandle = GCHandle.Alloc( this );

					if ( proc == null )
						proc = new Win32.TimerProc( this.TimeProc );
					
					timerid = Win32.SetTimer( IntPtr.Zero,	0, (uint)Interval, proc );
				}
				else {
					if ( timerid != 0 )
						Win32.KillTimer ( IntPtr.Zero , timerid );

					timerid = 0;

					if ( timerHandle.IsAllocated )
						timerHandle.Free();
				}
			}
		}

		public int Interval {
			get {
				return interval;
			}
			set {
				if ( value <= 0 )
					throw new ArgumentException (
					string.Format (" '{0}' is not a valid value for Interval. Interval must be greater than 0.",
							value ) );
				interval = value;
				if ( Enabled )
					Enabled = true; // restart
			}
		}

		public void Start() {
			Enabled = true;
		}

		public void Stop() {
			Enabled = false;
		}

		public override string ToString() 
		{
			return "[" + GetType().FullName.ToString() + "], Interval: " + Interval;
		}

		public event EventHandler Tick;

		protected virtual void OnTick(EventArgs e) 
		{
			if ( Tick != null )
				Tick ( this, e );
		}

		private void TimeProc( IntPtr hwnd, uint uMsg, uint idEvent, int dwTime )
		{
			OnTick ( EventArgs.Empty );
		}  
		
		protected override void Dispose( bool disposing	) {
			Enabled = false;
			base.Dispose ( disposing );
		}
	}
}
