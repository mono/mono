//
// SessionEndedEventArgs.cs
//
// Author:
//  Johannes Roith (johannes@jroith.de)
//
// (C) 2002 Johannes Roith
//
namespace Microsoft.Win32 {

	/// <summary>
	/// </summary>
public class SessionEndedEventArgs : System.EventArgs{

	SessionEndReasons myreason;
	
	public SessionEndedEventArgs(SessionEndReasons reason)
	{
		this.myreason = reason;
	}
	
	public SessionEndReasons Reason {

		get{
			return myreason;
		}
		
	}
	
}

}
