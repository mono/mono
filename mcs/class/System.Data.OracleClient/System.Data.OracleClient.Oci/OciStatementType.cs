// 
// OciStatementType.cs 
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
	internal enum OciStatementType {
		Default = 0x00,
		Select = 0x01,
		Update = 0x02,
		Delete = 0x03,
		Insert = 0x04,
		Create = 0x05,
		Drop = 0x06,
		Alter = 0x07,
		Begin = 0x08,
		Declare = 0x09
	}
}
