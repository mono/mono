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
	public enum MessageQueuePermissionAccess 
	{
		Administer = 62,
		Browse = 2,
		None = 0,
		Peek = 10,
		Receive = 26,
		Send = 6
	}
}
