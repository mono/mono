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
	[Flags]
	[Serializable]
	public enum GenericAccessRights 
	{
		All, 
		Execute, 
		None,
		Read, 
		Write
	}
}
