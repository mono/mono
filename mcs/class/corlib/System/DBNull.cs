//
// DBNull.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
// (C) 2003 Ben Maurer
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

		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}			

		byte IConvertible.ToByte (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}

		char IConvertible.ToChar (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}

		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}

		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}
		
		double IConvertible.ToDouble (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}

		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}

		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}

		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}

		[CLSCompliant (false)]
		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}

		float IConvertible.ToSingle (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}

		object IConvertible.ToType (Type type, IFormatProvider provider)
		{
			if (type == typeof (string)) return String.Empty;
			throw new InvalidCastException ();
		}

		[CLSCompliant (false)]
		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}

		[CLSCompliant (false)]
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}

		[CLSCompliant (false)]
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			throw new InvalidCastException ();
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
