// 
// OciTransactionFlags.cs 
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
	internal enum OciTransactionFlags {
		New = 0x01,
		Resume = 0x04,
		ReadOnly = 0x100,
		ReadWrite = 0x200,
		Serializable = 0x400,
		Tight = 0x10000,
		Loose = 0x20000
	}
}
