// BinaryCommon.cs
//
// Author:
//   Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2003 Lluis Sanchez Gual

using System;

namespace System.Runtime.Serialization.Formatters.Binary
{
	internal class BinaryCommon
	{
		// Header present in all binary serializations
		public static byte[] BinaryHeader = new Byte[] {0,1,0,0,0,255,255,255,255,1,0,0,0,0,0,0,0};

		public static bool IsPrimitive (Type type)
		{
			return type.IsPrimitive || type == typeof (DateTime) || type == typeof (Decimal);
		}

	}

	internal enum BinaryElement : byte
	{
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
}
