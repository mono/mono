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
	public enum Acknowledgment 
	{
		AccessDenied = 32772,
		BadDestinationQueue = 32768,
		BadEncryption = 32775,
		BadSignature = 32774,
		CouldNotEncrypt = 32776,
		HopCountExceeded = 32773,
		None = 0,
		NotTransactionalMessage = 32778,
		NotTransactionalQueue = 32777,
		Purged = 32769,
		QueueDeleted = 49152,
		QueueExceedMaximumSize = 32771,
		QueuePurged = 49153,
		ReachQueue = 2,
		ReachQueueTimeout = 32770,
		Receive = 16384,
		ReceiveTimeout = 49154
	}
}
