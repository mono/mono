// 
// OciExecuteMode.cs 
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
	internal enum OciExecuteMode {
		Default = 0x00,
		BatchMode = 0x01,
		ExactFetch = 0x02,
		KeepFetchState = 0x04,
		ScrollableCursor = 0x08,
		DescribeOnly = 0x10,
		CommitOnSuccess = 0x20,
		NonBlocking = 0x40,
		BatchErrors = 0x80,
		ParseOnly = 0x100,
		ShowDmlWarnings = 0x400
	}
}
