// 
// OciDescriptorType.cs 
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
	internal enum OciDescriptorType {
		LobLocator = 0x32,
		Snapshot = 0x33,
		ResultSet = 0x34,
		Parameter = 0x35,
		RowId = 0x36,
		ComplexObject = 0x37,
		FileLobLocator = 0x38,
		EnqueueOptions = 0x39,
		DequeueOptions = 0x3a,
		MessageProperties = 0x3b,
		Agent = 0x3c,
		Locator = 0x3d,
		IntervalYearToMonth = 0x3e,
		IntervalDayToSecond = 0x3f,
		Notify = 0x40,
		Date = 0x41,
		Time = 0x42,
		TimeWithTZ = 0x43,
		TimeStamp = 0x44,
		TimeStampWithTZ = 0x45,
		TimeStampLocal = 0x46,
		UserCallback = 0x47
	}
}
