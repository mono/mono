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
	public enum MessageQueueTransactionStatus
	{
		Aborted = 0,
		Committed = 1,
		Initialized = 2,
		Pending = 3
	}
}
