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
	public enum MessagePriority 
	{
		AboveNormal = 4,
		High = 5,
		Highest = 7,
		Low = 2,
		Lowest = 0,
		Normal = 3,
		VeryHigh = 6,
		VeryLow = 1
	}
}
