//
// System.Data.SqlTypes.SqlByte
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright 2002 Tim Coleman
//

using System;

namespace System.Data.SqlTypes
{
	public struct SqlByte : INullable, IComparable
	{
		#region Fields
		private byte value;

		public static readonly SqlByte MaxValue = new SqlByte (0xff);
		public static readonly SqlByte MinValue = new SqlByte (0);

		[MonoTODO]
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
			get { return (bool) (this == SqlByte.Null); }
		}

		public byte Value { 
			get { return value; }
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

		[MonoTODO]
		public int CompareTo (object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlByte Divide (SqlByte x, SqlByte y)
		{
			return (x / y);
		}

		[MonoTODO]
		public override bool Equals (object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean Equals (SqlByte x, SqlByte y)
		{
			return (x == y);
		}

		[MonoTODO]
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

		[MonoTODO]
		public static SqlByte Parse (string s)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public SqlString ToSqlString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
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
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlByte operator ^ (SqlByte x, SqlByte y)
		{
			return new SqlByte ((byte) (x.Value ^ y.Value));
		}

		public static SqlBoolean operator > (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
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
			return new SqlByte (x.ByteValue);
		}

		public static explicit operator byte (SqlByte x)
		{
			return x.Value;
		}

		public static explicit operator SqlByte (SqlDecimal x)
		{
			return new SqlByte ((byte)x.Value);
		}

		public static explicit operator SqlByte (SqlDouble x)
		{
			return new SqlByte ((byte)x.Value);
		}

		public static explicit operator SqlByte (SqlInt16 x)
		{
			return new SqlByte ((byte)x.Value);
		}

		public static explicit operator SqlByte (SqlInt32 x)
		{
			return new SqlByte ((byte)x.Value);
		}

		public static explicit operator SqlByte (SqlInt64 x)
		{
			return new SqlByte ((byte)x.Value);
		}

		public static explicit operator SqlByte (SqlMoney x)
		{
			return new SqlByte ((byte)x.Value);
		}

		public static explicit operator SqlByte (SqlSingle x)
		{
			return new SqlByte ((byte)x.Value);
		}

		[MonoTODO]
		public static explicit operator SqlByte (SqlString x)
		{
			throw new NotImplementedException ();
		}

		public static explicit operator SqlByte (byte x)
		{
			return new SqlByte (x);
		}
		
		#endregion
	}
}
			
