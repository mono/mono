// 
// OciHandleType.cs 
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
	internal enum OciHandleType {
		Environment = 0x01,
		Error = 0x02,
		Service = 0x03,
		Statement = 0x04,
		Bind = 0x05,
		Define = 0x06,
		Describe = 0x07,
		Server = 0x08,
		Session = 0x09,
		Transaction = 0x0a,
		ComplexObject = 0x0b,
		Security = 0x0c,
		Subscription = 0x0d,
		DirectPathContext = 0x0e,
		DirectPathColumnArray = 0x0f,
		DirectPathStream = 0x10,
		Process = 0x11
	}
}
