//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
// (C) 2003 Peter Van Isacker
//
using System;

namespace System.Messaging 
{
	[MonoTODO("Have to force the right specific values for each element")]
	[Flags]
	[Serializable]
	public enum StandardAccessRights 
	{
		All,
		Delete,
		Execute,
		ModifyOwner,
		None,
		Read,
		ReadSecurity,
		Required,
		Synchronize,
		Write,
		WriteSecurity
	}
}
