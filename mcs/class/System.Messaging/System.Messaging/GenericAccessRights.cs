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
	[Flags]
	[Serializable]
	public enum GenericAccessRights 
	{
		All = 268435456,
		Execute = 536870912,
		None = 0,
		Read = -2147483648,
		Write = 1073741824
	}
}
