//
// TimerElapsedEventArgs.cs
//
// Author:
//  Johannes Roith (johannes@jroith.de)
//
// (C) 2002 Johannes Roith
//
namespace Microsoft.Win32 {

	/// <summary>
	/// </summary>
public class TimerElapsedEventArgs : System.EventArgs{

	System.IntPtr mytimerId;

	
	public TimerElapsedEventArgs(System.IntPtr timerId)
	{
		this.mytimerId = timerId;
	}
	
	public System.IntPtr TimerId {

		get{
			return mytimerId;
		}
		
	}
	
}

}
