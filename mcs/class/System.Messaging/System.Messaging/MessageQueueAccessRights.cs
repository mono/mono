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
	[MonoTODO("Have to force the right specific values for each element")]
	[Flags]
	[Serializable]
	public enum MessageQueueAccessRights 
	{
		ChangeQueuePermissions,
		DeleteJournalMessage, 
		DeleteMessage,
		DeleteQueue, 
		FullControl, 
		GenericRead,
		GenericWrite, 
		GetQueuePermissions, 
		GetQueueProperties,
		PeekMessage, 
		ReceiveJournalMessage, 
		ReceiveMessage,
		SetQueueProperties, 
		TakeQueueProperties, 
		TakeQueueOwnership,
		WriteMessage
	}
}
