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
	public enum MessageType 
	{
		Acknowledgment = 1,
		Normal = 2,
		Report = 3
	}
}
