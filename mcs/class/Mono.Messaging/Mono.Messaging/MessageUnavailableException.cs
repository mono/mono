// MessageUnavailableException.cs created with MonoDevelop
// User: mike at 11:46 a 7/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Mono.Messaging
{
	
	[Serializable]
	public class MessageUnavailableException : MonoMessagingException
	{
		public MessageUnavailableException () : base ()
		{
		}
		
		public MessageUnavailableException (string msg) : base (msg)
		{
		}
		
		public MessageUnavailableException (string msg, Exception e) 
			: base (msg, e)
		{
		}
	}
}
