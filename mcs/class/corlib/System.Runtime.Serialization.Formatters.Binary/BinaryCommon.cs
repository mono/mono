// BinaryCommon.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003 Lluis Sanchez Gual

using System;

namespace System.Runtime.Serialization.Formatters.Binary
{
	internal class BinaryCommon
	{
		// Header present in all binary serializations
		public static byte[] BinaryHeader = new Byte[] {0,1,0,0,0,255,255,255,255,1,0,0,0,0,0,0,0};

		static Type[] _typeCodesToType;
		static byte[] _typeCodeMap;

		static BinaryCommon()
		{
			_typeCodesToType = new Type [19];
			_typeCodesToType[(int)BinaryTypeCode.Boolean] = typeof (Boolean);
			_typeCodesToType[(int)BinaryTypeCode.Byte] = typeof (Byte);
			_typeCodesToType[(int)BinaryTypeCode.Char] = typeof (Char);
			_typeCodesToType[(int)BinaryTypeCode.DateTime] = typeof (DateTime);
			_typeCodesToType[(int)BinaryTypeCode.Decimal] = typeof (Decimal);
			_typeCodesToType[(int)BinaryTypeCode.Double] = typeof (Double);
			_typeCodesToType[(int)BinaryTypeCode.Int16] = typeof (Int16);
			_typeCodesToType[(int)BinaryTypeCode.Int32] = typeof (Int32);
			_typeCodesToType[(int)BinaryTypeCode.Int64] = typeof (Int64);
			_typeCodesToType[(int)BinaryTypeCode.SByte] = typeof (SByte);
			_typeCodesToType[(int)BinaryTypeCode.Single] = typeof (Single);
			_typeCodesToType[(int)BinaryTypeCode.UInt16] = typeof (UInt16);
			_typeCodesToType[(int)BinaryTypeCode.UInt32] = typeof (UInt32);
			_typeCodesToType[(int)BinaryTypeCode.UInt64] = typeof (UInt64);
			_typeCodesToType[(int)BinaryTypeCode.Null] = null;
			_typeCodesToType[(int)BinaryTypeCode.String] = typeof (string);

			_typeCodeMap = new byte[30];
			_typeCodeMap[(int)TypeCode.Boolean] = (byte) BinaryTypeCode.Boolean;
			_typeCodeMap[(int)TypeCode.Byte] = (byte) BinaryTypeCode.Byte;
			_typeCodeMap[(int)TypeCode.Char] = (byte) BinaryTypeCode.Char;
			_typeCodeMap[(int)TypeCode.DateTime] = (byte) BinaryTypeCode.DateTime;
			_typeCodeMap[(int)TypeCode.Decimal] = (byte) BinaryTypeCode.Decimal;
			_typeCodeMap[(int)TypeCode.Double] = (byte) BinaryTypeCode.Double;
			_typeCodeMap[(int)TypeCode.Int16] = (byte) BinaryTypeCode.Int16;
			_typeCodeMap[(int)TypeCode.Int32] = (byte) BinaryTypeCode.Int32;
			_typeCodeMap[(int)TypeCode.Int64] = (byte) BinaryTypeCode.Int64;
			_typeCodeMap[(int)TypeCode.SByte] = (byte) BinaryTypeCode.SByte;
			_typeCodeMap[(int)TypeCode.Single] = (byte) BinaryTypeCode.Single;
			_typeCodeMap[(int)TypeCode.UInt16] = (byte) BinaryTypeCode.UInt16;
			_typeCodeMap[(int)TypeCode.UInt32] = (byte) BinaryTypeCode.UInt32;
			_typeCodeMap[(int)TypeCode.UInt64] = (byte) BinaryTypeCode.UInt64;
			_typeCodeMap[(int)TypeCode.String] = (byte) BinaryTypeCode.String;
		}

		public static bool IsPrimitive (Type type)
		{
			return type.IsPrimitive || type == typeof (DateTime) || type == typeof (Decimal);
		}

		public static byte GetTypeCode (Type type)
		{
			return _typeCodeMap [(int)Type.GetTypeCode(type)];
		}

		public static Type GetTypeFromCode (int code)
		{
			return _typeCodesToType [code];
		}
	}

	internal enum BinaryElement : byte
	{
		Header = 0,
		RefTypeObject = 1,
		_Unknown1 = 2,
		_Unknown2 = 3,
		RuntimeObject = 4,
		ExternalObject = 5,
		String = 6,
		GenericArray = 7,
		BoxedPrimitiveTypeValue = 8,
		ObjectReference = 9,
		NullValue = 10,
		End = 11,
		Assembly = 12,
		ArrayFiller8b = 13,
		ArrayFiller32b = 14,
		ArrayOfPrimitiveType = 15,
		ArrayOfObject = 16,
		ArrayOfString = 17,
		Method = 18,
		_Unknown4 = 19,
		_Unknown5 = 20,
		MethodCall = 21,
		MethodResponse = 22
	}

	internal enum TypeTag : byte
	{
		PrimitiveType = 0,
		String = 1,
		ObjectType = 2,
		RuntimeType = 3,
		GenericType = 4,
		ArrayOfObject = 5,
		ArrayOfString = 6,
		ArrayOfPrimitiveType = 7
	}

	internal enum ArrayStructure : byte
	{
		SingleDimensional = 0,
		Jagged = 1,
		MultiDimensional = 2
	}

	internal enum MethodFlags : byte
	{
		NoArguments = 1,
		PrimitiveArguments = 2,
		ArgumentsInSimpleArray = 4,
		ArgumentsInMultiArray = 8,
		ExcludeLogicalCallContext = 16,
		IncludesLogicalCallContext = 64,
		IncludesSignature = 128,

		FormatMask = 15,
		NeedsInfoArrayMask = 4 + 8 + 64 + 128
	}

	internal enum ReturnTypeTag : byte
	{
		Null = 2,
		PrimitiveType = 8,
		ObjectType = 16,
		Exception = 32
	}

	enum BinaryTypeCode : byte
	{
		Boolean = 1,
		Byte = 2,
		Char = 3,
		Decimal = 5,
		Double = 6,
		Int16 = 7,
		Int32 = 8,
		Int64 = 9,
		SByte = 10,
		Single = 11,
		DateTime = 13,
		UInt16 = 14,
		UInt32 = 15,
		UInt64 = 16,
		Null = 17,
		String = 18
	}

}
