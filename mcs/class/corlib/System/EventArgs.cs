//
// System.EventArgs.cs 
//
// Author:
//   Michael Lambert (michaellambert@email.com)
//
// (C) 2001 Michael Lambert, All Rights Reserved
//

namespace System
{
	[Serializable]
	public class EventArgs
	{
		public static readonly EventArgs Empty = new EventArgs ();

		public EventArgs ()
		{
		}
	}
}
