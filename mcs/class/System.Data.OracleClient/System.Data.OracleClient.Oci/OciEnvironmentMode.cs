// 
// OciEnvironmentMode.cs 
//  
// Part of managed C#/.NET library System.Data.OracleClient.dll
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient.Oci
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient.Oci
// 
// Author: 
//     Tim Coleman <tim@timcoleman.com>
//         
// Copyright (C) Tim Coleman, 2003
// 

using System;

namespace System.Data.OracleClient.Oci {
	[Flags]
	internal enum OciEnvironmentMode {
		Default = 0x00,
		Threaded = 0x01,
		Object = 0x02,
		Events = 0x04,
		Shared = 0x10,
		NoUserCallback = 0x40,
		NoMutex = 0x80,
		SharedExt = 0x100,
		Cache = 0x200,
		NoCache = 0x400
	}
}
