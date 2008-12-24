// CompletedEventHandler.cs created with MonoDevelop
// User: mike at 8:16 p 21/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Mono.Messaging
{
	[Serializable]
	public delegate void CompletedEventHandler(object sender, CompletedEventArgs e);
}
