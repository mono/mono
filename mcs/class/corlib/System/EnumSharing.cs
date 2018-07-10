using System;

#if UNITY_AOT

namespace System
{
	enum SByteEnum : sbyte {}
	enum Int16Enum : short {}
	enum Int32Enum : int {}
	enum Int64Enum : long {}

	enum ByteEnum : byte {}
	enum UInt16Enum : ushort {}
	enum UInt32Enum : uint {}
	enum UInt64Enum : ulong {}
}

#endif
