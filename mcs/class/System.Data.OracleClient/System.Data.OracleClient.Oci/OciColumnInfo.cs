// 
// OciColumnInfo.cs 
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
	internal struct OciColumnInfo 
	{
		public string ColumnName;
		public int ColumnOrdinal;
		public ushort ColumnSize;
		public byte Precision;
		public sbyte Scale;
		public OciDataType DataType;
		public bool AllowDBNull;
		public string BaseColumnName;
	}
}
