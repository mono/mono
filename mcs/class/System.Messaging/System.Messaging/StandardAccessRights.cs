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
	[Flags]
	[Serializable]
	public enum StandardAccessRights 
	{
		All = 2031616,
		Delete = 65536,
		Execute = 131072,
		ModifyOwner = 524288,
		None = 0,
		Read = 131072,
		ReadSecurity = 131072,
		Required = 851968,
		Synchronize = 1048576,
		Write = 131072,
		WriteSecurity = 262144
	}
}
