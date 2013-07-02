// BinaryCommon.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003 Lluis Sanchez Gual

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace System.Runtime.Serialization.Formatters.Binary
{
	internal class BinaryCommon
	{
		// Header present in all binary serializations
		public static byte[] BinaryHeader = new Byte[] {0,1,0,0,0,255,255,255,255,1,0,0,0,0,0,0,0};

		static Type[] _typeCodesToType;
		static byte[] _typeCodeMap;
		public static bool UseReflectionSerialization = false;

		static BinaryCommon()
		{
			_typeCodesToType = new Type [19];
			_typeCodesToType[(int)BinaryTypeCode.Boolean] = typeof (Boolean);
			_typeCodesToType[(int)BinaryTypeCode.Byte] = typeof (Byte);
			_typeCodesToType[(int)BinaryTypeCode.Char] = typeof (Char);
			_typeCodesToType[(int)BinaryTypeCode.TimeSpan] = typeof (TimeSpan);
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

			// TimeStamp does not have a TypeCode, so it is managed as a special
			// case in GetTypeCode()
			// This environment variable is only for test and benchmarking purposes.
			// By default, mono will always use IL generated class serializers.
			string s = Environment.GetEnvironmentVariable("MONO_REFLECTION_SERIALIZER");
			if (s == null) s = "no";
			UseReflectionSerialization = (s != "no");
		}

		public static bool IsPrimitive (Type type)
		{
			return (type.IsPrimitive && type != typeof (IntPtr)) || 
				type == typeof (DateTime) || 
				type == typeof (TimeSpan) || 
				type == typeof (Decimal);
		}

		public static byte GetTypeCode (Type type)
		{
			if (type == typeof(TimeSpan)) return (byte) BinaryTypeCode.TimeSpan;
			else return _typeCodeMap [(int)Type.GetTypeCode(type)];
		}

		public static Type GetTypeFromCode (int code)
		{
			return _typeCodesToType [code];
		}
		
		public static void CheckSerializable (Type type, ISurrogateSelector selector, StreamingContext context)
		{
			if (!type.IsSerializable && !type.IsInterface) 
			{
				if (selector != null && selector.GetSurrogate (type, context, out selector) != null)
					return;

				throw new SerializationException ("Type " + type + " is not marked as Serializable.");
			}
		}
		
		public static void SwapBytes (byte[] byteArray, int size, int dataSize)
		{
			byte b;
			if (dataSize == 8) {
				for (int n=0; n<size; n+=8) {
					b = byteArray [n]; byteArray [n] = byteArray [n + 7]; byteArray [n + 7] = b;
					b = byteArray [n+1]; byteArray [n+1] = byteArray [n + 6]; byteArray [n + 6] = b;
					b = byteArray [n+2]; byteArray [n+2] = byteArray [n + 5]; byteArray [n + 5] = b;
					b = byteArray [n+3]; byteArray [n+3] = byteArray [n + 4]; byteArray [n + 4] = b;
				}
			} else if (dataSize == 4) {
				for (int n=0; n<size; n+=4) {
					b = byteArray [n]; byteArray [n] = byteArray [n + 3]; byteArray [n + 3] = b;
					b = byteArray [n+1]; byteArray [n+1] = byteArray [n + 2]; byteArray [n + 2] = b;
				}
			} else if (dataSize == 2) {
				for (int n=0; n<size; n+=2) {
					b = byteArray [n]; byteArray [n] = byteArray [n + 1]; byteArray [n + 1] = b;
				}
			}
		}
	}

	internal enum BinaryElement : byte
	{
		Header = 0,
		RefTypeObject = 1,
		UntypedRuntimeObject = 2,
		UntypedExternalObject = 3,
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

	internal enum MethodFlags
	{
		NoArguments = 1,
		PrimitiveArguments = 2,
		ArgumentsInSimpleArray = 4,
		ArgumentsInMultiArray = 8,
		ExcludeLogicalCallContext = 16,
		IncludesLogicalCallContext = 64,
		IncludesSignature = 128,

		FormatMask = 15,

		GenericArguments = 0x8000,
		NeedsInfoArrayMask = 4 + 8 + 64 + 128 + 0x8000,
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
		TimeSpan = 12,
		DateTime = 13,
		UInt16 = 14,
		UInt32 = 15,
		UInt64 = 16,
		Null = 17,
		String = 18
	}

}
