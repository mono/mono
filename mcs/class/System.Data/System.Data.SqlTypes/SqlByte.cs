//
// System.Data.SqlTypes.SqlByte
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright 2002 Tim Coleman
//

using System;
using System.Globalization;

namespace System.Data.SqlTypes
{
	public struct SqlByte : INullable, IComparable
	{
		#region Fields

		byte value;
		public static readonly SqlByte MaxValue = new SqlByte (0xff);
		public static readonly SqlByte MinValue = new SqlByte (0);
		public static readonly SqlByte Null;
		public static readonly SqlByte Zero = new SqlByte (0);

		#endregion

		#region Constructors

		public SqlByte (byte value) 
		{
			this.value = value;
		}

		#endregion

		#region Properties

		public bool IsNull {
			get { return (bool) (this == Null); }
		}

		public byte Value { 
			get { 
				if (this.IsNull) 
					throw new SqlNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static SqlByte Add (SqlByte x, SqlByte y)
		{
			return (x + y);
		}

		public static SqlByte BitwiseAnd (SqlByte x, SqlByte y)
		{
			return (x & y);
		}

		public static SqlByte BitwiseOr (SqlByte x, SqlByte y)
		{
			return (x | y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SqlByte))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlByte"));
			else if (((SqlByte)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SqlByte)value).Value);
		}

		public static SqlByte Divide (SqlByte x, SqlByte y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SqlByte))
				return false;
			else
				return (bool) (this == (SqlByte)value);
		}

		public static SqlBoolean Equals (SqlByte x, SqlByte y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			return (int)value;
		}

		public static SqlBoolean GreaterThan (SqlByte x, SqlByte y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlByte x, SqlByte y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlByte x, SqlByte y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlByte x, SqlByte y)
		{
			return (x <= y);
		}

		public static SqlByte Mod (SqlByte x, SqlByte y)
		{
			return (x % y);
		}

		public static SqlByte Multiply (SqlByte x, SqlByte y)
		{
			return (x * y);
		}

		public static SqlBoolean NotEquals (SqlByte x, SqlByte y)
		{
			return (x != y);
		}

		public static SqlByte OnesComplement (SqlByte x)
		{
			return ~x;
		}

		public static SqlByte Parse (string s)
		{
			return new SqlByte (Byte.Parse (s));
		}

		public static SqlByte Subtract (SqlByte x, SqlByte y)
		{
			return (x - y);
		}

		public SqlBoolean ToSqlBoolean ()
		{
			return ((SqlBoolean)this);
		}
		
		public SqlDecimal ToSqlDecimal ()
		{
			return ((SqlDecimal)this);
		}

		public SqlDouble ToSqlDouble ()
		{
			return ((SqlDouble)this);
		}

		public SqlInt16 ToSqlInt16 ()
		{
			return ((SqlInt16)this);
		}

		public SqlInt32 ToSqlInt32 ()
		{
			return ((SqlInt32)this);
		}

		public SqlInt64 ToSqlInt64 ()
		{
			return ((SqlInt64)this);
		}

		public SqlMoney ToSqlMoney ()
		{
			return ((SqlMoney)this);
		}

		public SqlSingle ToSqlSingle ()
		{
			return ((SqlSingle)this);
		}

		public SqlString ToSqlString ()
		{
			return ((SqlString)this);
		}

		public override string ToString ()
		{
			if (this.IsNull)
				return String.Empty;
			else
				return value.ToString ();
		}

		public static SqlByte Xor (SqlByte x, SqlByte y)
		{
			return (x ^ y);
		}

		public static SqlByte operator + (SqlByte x, SqlByte y)
		{
			return new SqlByte ((byte) (x.Value + y.Value));
		}

		public static SqlByte operator & (SqlByte x, SqlByte y)
		{
			return new SqlByte ((byte) (x.Value & y.Value));
		}

		public static SqlByte operator | (SqlByte x, SqlByte y)
		{
			return new SqlByte ((byte) (x.Value | y.Value));
		}

		public static SqlByte operator / (SqlByte x, SqlByte y)
		{
			return new SqlByte ((byte) (x.Value / y.Value));
		}

		public static SqlBoolean operator == (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlByte operator ^ (SqlByte x, SqlByte y)
		{
			return new SqlByte ((byte) (x.Value ^ y.Value));
		}

		public static SqlBoolean operator > (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value <= y.Value);
		}

		public static SqlByte operator % (SqlByte x, SqlByte y)
		{
			return new SqlByte ((byte) (x.Value % y.Value));
		}

		public static SqlByte operator * (SqlByte x, SqlByte y)
		{
			return new SqlByte ((byte) (x.Value * y.Value));
		}

		public static SqlByte operator ~ (SqlByte x)
		{
			return new SqlByte ((byte) ~x.Value);
		}

		public static SqlByte operator - (SqlByte x, SqlByte y)
		{
			return new SqlByte ((byte) (x.Value - y.Value));
		}

		public static explicit operator SqlByte (SqlBoolean x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlByte (x.ByteValue);
		}

		public static explicit operator byte (SqlByte x)
		{
			return x.Value;
		}

		public static explicit operator SqlByte (SqlDecimal x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlByte ((byte)x.Value);
		}

		public static explicit operator SqlByte (SqlDouble x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlByte ((byte)x.Value);
		}

		public static explicit operator SqlByte (SqlInt16 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlByte ((byte)x.Value);
		}

		public static explicit operator SqlByte (SqlInt32 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlByte ((byte)x.Value);
		}

		public static explicit operator SqlByte (SqlInt64 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlByte ((byte)x.Value);
		}

		public static explicit operator SqlByte (SqlMoney x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlByte ((byte)x.Value);
		}

		public static explicit operator SqlByte (SqlSingle x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlByte ((byte)x.Value);
		}


		public static explicit operator SqlByte (SqlString x)
		{
			return SqlByte.Parse (x.Value);
		}

		public static implicit operator SqlByte (byte x)
		{
			return new SqlByte (x);
		}
		
		#endregion
	}
}
			
