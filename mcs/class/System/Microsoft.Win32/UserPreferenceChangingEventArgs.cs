//
// UserPreferenceChangingEventArgs.cs
//
// Author:
//  Johannes Roith (johannes@jroith.de)
//
// (C) 2002 Johannes Roith
//
namespace Microsoft.Win32 {

	/// <summary>
	/// </summary>
public class UserPreferenceChangingEventArgs : System.EventArgs{

	UserPreferenceCategory mycategory;

	
	public UserPreferenceChangingEventArgs(UserPreferenceCategory category)
	{
		this.mycategory = category;
	}
	
	public UserPreferenceCategory Category {

		get{
			return mycategory;
		}
		
	}
	
}

}
