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
	[Serializable]
	public enum TrusteeType 
	{
		Alias, 
		Computer, 
		Domain,
		Group, 
		Unknown, 
		User
	}
}
