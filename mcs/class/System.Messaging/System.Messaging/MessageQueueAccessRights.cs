//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
//	(C) Ximian, Inc.  http://www.ximian.com
//
using System;

namespace System.Messaging 
{
	[Flags]
	[Serializable]
	public enum MessageQueueAccessRights
	{
		ChangeQueuePermissions = 262144,
		DeleteJournalMessage = 8, 
		DeleteMessage = 1,
		DeleteQueue = 65536,
		FullControl = 983103,
		GenericRead = 131115,
		GenericWrite = 131108, 
		GetQueuePermissions = 131072, 
		GetQueueProperties = 32,
		PeekMessage = 2, 
		ReceiveJournalMessage = 10, 
		ReceiveMessage = 3,
		SetQueueProperties = 16, 
		TakeQueueOwnership = 524288,
		WriteMessage = 4
	}
}
