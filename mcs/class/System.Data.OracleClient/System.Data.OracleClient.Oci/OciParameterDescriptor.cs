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
//         
// Copyright (C) Tim Coleman, 2003
// 

using System;
using System.Data.OracleClient;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciParameterDescriptor : OciDescriptorHandle
	{
		#region Fields

		OciErrorHandle errorHandle;
		OciServiceHandle service;
		OciDataType type;

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
			return (OciDataType) GetAttributeInt32 (OciAttributeType.DataType, ErrorHandle);
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
				return Type.GetType ("System.Byte[]");
			case "Raw":
				return Type.GetType ("System.Byte[]");
			case "LongRaw":
				return Type.GetType ("System.Byte[]");
			case "UnsignedInt":
				return typeof (System.UInt32);
			case "LongVarChar":
				return typeof (System.String);
			case "LongVarRaw":
				return Type.GetType ("System.Byte[]");
			case "Char":
				return typeof (System.String);
			case "CharZ":
				return typeof (System.String);
			case "RowIdDescriptor":
				return typeof (System.String);
			case "NamedDataType":
				return typeof (System.String);
			case "Ref":
				return Type.GetType ("System.Data.OracleClient.OracleDataReader");
			case "Clob":
				return typeof (System.String);
			case "Blob":
				return Type.GetType ("System.Byte[]");
			case "BFile":
				return Type.GetType ("System.Byte[]");
			case "OciString":
				return typeof (System.String);
			case "OciDate":
				return typeof (System.DateTime);
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
