//
// System.DBNull.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public sealed class DBNull : ISerializable, IConvertible
	{
		// Fields
		public static readonly DBNull Value = new DBNull ();

		// Private constructor
		private DBNull () {}

		// Methods
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			UnitySerializationHolder.GetDBNullData (this, info, context);
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.DBNull;
		}

		[MonoTODO]
		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return false;
		}			

		[MonoTODO]
		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return Byte.MinValue;
		}

		[MonoTODO]
		char IConvertible.ToChar (IFormatProvider provider)
		{
			return Char.MinValue;
		}

		[MonoTODO]
		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			return DateTime.MinValue;
		}

		[MonoTODO]
		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			return Decimal.MinValue;
		}
		
		[MonoTODO]
		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return Double.MinValue;
		}

		[MonoTODO]
		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return Int16.MinValue;
		}

		[MonoTODO]
		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return Int32.MinValue;
		}

		[MonoTODO]
		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return Int64.MinValue;
		}

		[MonoTODO]
		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return SByte.MinValue;
		}

		[MonoTODO]
		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return Single.MinValue;
		}

		[MonoTODO]
		object IConvertible.ToType (Type type, IFormatProvider provider)
		{
			return null;
		}

		[MonoTODO]
		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return UInt16.MinValue;
		}

		[MonoTODO]
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return UInt32.MinValue;
		}

		[MonoTODO]
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return UInt64.MinValue;
		}

		public override string ToString ()
		{
			return String.Empty;
		}

		public string ToString (IFormatProvider provider)
		{
			return String.Empty;
		}
	}
}
