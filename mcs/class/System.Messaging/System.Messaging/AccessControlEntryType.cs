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
	[Serializable]
	public enum AccessControlEntryType 
	{
		Allow = 1,
		Deny = 3,
		Revoke = 4,
		Set = 2
	}
}
