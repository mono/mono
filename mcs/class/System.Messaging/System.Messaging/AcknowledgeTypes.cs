//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
//	(C) 2003 Peter Van Isacker
//
using System;

namespace System.Messaging 
{
	[Flags]
	[Serializable]
	public enum AcknowledgeTypes 
	{
		FullReachQueue = 5,
		FullReceive = 14,
		NegativeReceive = 8,
		None = 0, 
		NotAcknowledgeReachQueue = 4,
		NotAcknowledgeReceive = 12,
		PositiveArrival = 1,
		PositiveReceive = 2
	}
}
