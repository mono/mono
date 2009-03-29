//
// OciParameterDescriptor.cs
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
//     Daniel Morgan <danielmorgan@verizon.net>
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Daniel Morgan, 2005
//

using System;
using System.Data.OracleClient;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciParameterDescriptor : OciDescriptorHandle
	{
		#region Fields

		OciErrorHandle errorHandle;
		//OciServiceHandle service;
		//OciDataType type;

		#endregion // Fields

		#region Constructors

		public OciParameterDescriptor (OciHandle parent, IntPtr handle)
			: base (OciHandleType.Parameter, parent, handle)
		{
		}

		#endregion // Constructors

		#region Properties

		public OciErrorHandle ErrorHandle {
			get { return errorHandle; }
			set { errorHandle = value; }
		}

		#endregion // Properties

		#region Methods

		public string GetName ()
		{
			return GetAttributeString (OciAttributeType.Name, ErrorHandle);
		}

		public int GetDataSize ()
		{
			return (int) GetAttributeUInt16 (OciAttributeType.DataSize, ErrorHandle);
		}

		public OciDataType GetDataType ()
		{
			return (OciDataType) GetAttributeUInt16 (OciAttributeType.DataType, ErrorHandle);
		}

		public static OracleType OciDataTypeToOracleType (OciDataType ociType)
		{
			switch (ociType) {
			case OciDataType.VarChar2:
				return OracleType.VarChar;
			case OciDataType.Number:
				return OracleType.Number;
			case OciDataType.Integer:
				return OracleType.UInt32;
			case OciDataType.Float:
				return OracleType.Float;
			case OciDataType.String:
				return OracleType.VarChar;
			case OciDataType.VarNum:
				return OracleType.Number;
			case OciDataType.Long:
				return OracleType.LongVarChar;
			case OciDataType.VarChar:
				return OracleType.VarChar;
			case OciDataType.RowId:
				return OracleType.RowId;
			case OciDataType.Date:
				return OracleType.DateTime;
			case OciDataType.VarRaw:
				return OracleType.Raw;
			case OciDataType.Raw:
				return OracleType.Raw;
			case OciDataType.LongRaw:
				return OracleType.Raw;
			case OciDataType.UnsignedInt:
				return OracleType.UInt32;
			case OciDataType.LongVarChar:
				return OracleType.LongVarChar;
			case OciDataType.LongVarRaw:
				return OracleType.Raw;
			case OciDataType.Char:
				return OracleType.Char;
			case OciDataType.CharZ:
				return OracleType.Char;
			case OciDataType.RowIdDescriptor:
				return OracleType.RowId;
			//case OciDataType.NamedDataType:
			//	return ???
			//case OciDataType.Ref:
			//	return ???
			case OciDataType.Clob:
				return OracleType.Clob;
			case OciDataType.Blob:
				return OracleType.Blob;
			case OciDataType.BFile:
				return OracleType.BFile;
			case OciDataType.OciString:
				return OracleType.VarChar;
			case OciDataType.OciDate:
				return OracleType.DateTime;
			case OciDataType.TimeStamp:
				return OracleType.Timestamp;
			case OciDataType.IntervalDayToSecond:
				return OracleType.IntervalDayToSecond;
			case OciDataType.IntervalYearToMonth:
				return OracleType.IntervalYearToMonth;
			default:
				throw new NotImplementedException ();
			}
		}

		public Type GetFieldType (string sDataTypeName)
		{
			switch (sDataTypeName) {
			case "VarChar2":
				return typeof (System.String);
			case "Number":
				return typeof (System.Decimal);
			case "Integer":
				return typeof (System.Int32);
			case "Float":
				return typeof (System.Decimal);
			case "String":
				return typeof (System.String);
			case "VarNum":
				return typeof (System.Decimal);
			case "Long":
				return typeof (System.String);
			case "VarChar":
				return typeof (System.String);
			case "RowId":
				return typeof (System.String);
			case "Date":
				return typeof (System.DateTime);
			case "VarRaw":
				return typeof (byte[]);
			case "Raw":
				return typeof (byte[]);
			case "LongRaw":
				return typeof (byte[]);
			case "UnsignedInt":
				return typeof (System.UInt32);
			case "LongVarChar":
				return typeof (System.String);
			case "LongVarRaw":
				return typeof (byte[]);
			case "Char":
				return typeof (System.String);
			case "CharZ":
				return typeof (System.String);
			case "RowIdDescriptor":
				return typeof (System.String);
			case "NamedDataType":
				return typeof (System.String);
			case "Ref":
				return typeof (System.String);
			case "Clob":
				return typeof (System.String);
			case "Blob":
				return typeof (byte[]);
			case "BFile":
				return typeof (byte[]);
			case "OciString":
				return typeof (System.String);
			case "OciDate":
				return typeof (System.DateTime);
			case "TimeStamp":
				return typeof (System.DateTime);
			case "IntervalDayToSecond":
				return typeof (System.TimeSpan);
			case "IntervalYearToMonth":
				return typeof (System.Int32);
			default:
				// FIXME: are these types correct?
				return typeof(System.String);
			}
		}

		public string GetDataTypeName ()
		{
			switch(GetDataType())
			{
				case OciDataType.VarChar2:
					return "VarChar2";
				case OciDataType.Number:
					return "Number";
				case OciDataType.Integer:
					return "Integer";
				case OciDataType.Float:
					return "Float";
				case OciDataType.String:
					return "String";
				case OciDataType.VarNum:
					return "VarNum";
				case OciDataType.Long:
					return "Long";
				case OciDataType.VarChar:
					return "VarChar";
				case OciDataType.RowId:
					return "RowId";
				case OciDataType.Date:
					return "Date";
				case OciDataType.VarRaw:
					return "VarRaw";
				case OciDataType.Raw:
					return "Raw";
				case OciDataType.LongRaw:
					return "LongRaw";
				case OciDataType.UnsignedInt:
					return "UnsignedInt";
				case OciDataType.LongVarChar:
					return "LongVarChar";
				case OciDataType.LongVarRaw:
					return "LongVarRaw";
				case OciDataType.Char:
					return "Char";
				case OciDataType.CharZ:
					return "CharZ";
				case OciDataType.RowIdDescriptor:
					return "RowIdDescriptor";
				case OciDataType.NamedDataType:
					return "NamedDataType";
				case OciDataType.Ref:
					return "Ref";
				case OciDataType.Clob:
					return "Clob";
				case OciDataType.Blob:
					return "Blob";
				case OciDataType.BFile:
					return "BFile";
				case OciDataType.OciString:
					return "OciString";
				case OciDataType.OciDate:
					return "OciDate";
				case OciDataType.TimeStamp:
					return "TimeStamp";
				case OciDataType.IntervalDayToSecond:
					return "IntervalDayToSecond";
				case OciDataType.IntervalYearToMonth:
					return "IntervalYearToMonth";
				default:
					return "Unknown";
			}
		}

		public short GetPrecision ()
		{
			return (short) GetAttributeByte (OciAttributeType.Precision, ErrorHandle);
		}

		public short GetScale ()
		{
			return (short) GetAttributeSByte (OciAttributeType.Scale, ErrorHandle);
		}

		public bool GetIsNull ()
		{
			return GetAttributeBool (OciAttributeType.IsNull, ErrorHandle);
		}

		#endregion // Methods
	}
}
