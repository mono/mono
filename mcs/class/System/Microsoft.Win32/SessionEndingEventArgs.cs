//
// SessionEndingEventArgs.cs
//
// Author:
//  Johannes Roith (johannes@jroith.de)
//
// (C) 2002 Johannes Roith
//
namespace Microsoft.Win32 {

	/// <summary>
	/// </summary>
public class SessionEndingEventArgs : System.EventArgs{

	SessionEndReasons myreason;
	bool mycancel;

	public SessionEndingEventArgs(SessionEndReasons reason)
	{
		this.myreason = reason;
	}
	
	public SessionEndReasons Reason {

		get{
			return myreason;
		}
		
	}
	

	public bool Cancel {

		get{
			return mycancel;
		}
		set{
		
		}
		
	}

}

}
