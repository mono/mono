//
// TimerElapsedEventArgs.cs
//
// Authors:
//   Johannes Roith (johannes@jroith.de)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Johannes Roith
//

using System.Runtime.InteropServices;

namespace Microsoft.Win32 
{
	[ComVisible(false)]
	public class TimerElapsedEventArgs : System.EventArgs
	{
		System.IntPtr mytimerId;

		public TimerElapsedEventArgs (System.IntPtr timerId)
		{
			this.mytimerId = timerId;
		}

		public System.IntPtr TimerId {
			get {
				return mytimerId;
			}
		}
	}
}
