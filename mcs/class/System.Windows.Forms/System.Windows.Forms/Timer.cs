//
// System.Windows.Forms.Timer
//
// Author:
//	stubbed out by Jackson Harper (jackson@latitudegeo.com)
//	Dennis Hayes (dennish@raytek.com)
//	Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) 2002 Ximian, Inc
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
