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
	[Serializable]
	public enum MessageQueueTransactionType 
	{
		Automatic = 1,
		None = 0,
		Single = 3
	}
}
