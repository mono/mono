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
	public enum TrusteeType 
	{
		Alias = 4,
		Computer = 5,
		Domain = 3,
		Group = 2,
		Unknown = 0,
		User = 1
	}
}
