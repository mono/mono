//
// System.Data.SqlTypes.SqlInt64
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright 2002 Tim Coleman
//

using System;

namespace System.Data.SqlTypes
{
	public struct SqlInt64 : INullable, IComparable
	{
		#region Fields
		private long value;

		public static readonly SqlInt64 MaxValue; // 2^63 - 1
		public static readonly SqlInt64 MinValue; // -2^63
		public static readonly SqlInt64 Null;
		public static readonly SqlInt64 Zero = new SqlInt64 (0);

		#endregion

		#region Constructors

		public SqlInt64 (long value) 
		{
			this.value = value;
		}

		#endregion

		#region Properties

		[MonoTODO]
		public bool IsNull { 
			get { throw new NotImplementedException (); }
		}

		public long Value { 
			get { return value; }
		}

		#endregion

		#region Methods

		public static SqlInt64 Add (SqlInt64 x, SqlInt64 y)
		{
			return (x + y);
		}

		public static SqlInt64 BitwiseAnd (SqlInt64 x, SqlInt64 y)
		{
			return (x & y);
		}

		public static SqlInt64 BitwiseOr (SqlInt64 x, SqlInt64 y)
		{
			return (x | y);
		}

		[MonoTODO]
		public int CompareTo (object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlInt64 Divide (SqlInt64 x, SqlInt64 y)
		{
			return (x / y);
		}

		[MonoTODO]
		public override bool Equals (object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean Equals (SqlInt64 x, SqlInt64 y)
		{
			return (x == y);
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return (int)value;
		}

		public static SqlBoolean GreaterThan (SqlInt64 x, SqlInt64 y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlInt64 x, SqlInt64 y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlInt64 x, SqlInt64 y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlInt64 x, SqlInt64 y)
		{
			return (x <= y);
		}

		public static SqlInt64 Multiply (SqlInt64 x, SqlInt64 y)
		{
			return (x * y);
		}

		public static SqlBoolean NotEquals (SqlInt64 x, SqlInt64 y)
		{
			return (x != y);
		}

		public static SqlInt64 OnesComplement (SqlInt64 x)
		{
			return ~x;
		}

		[MonoTODO]
		public static SqlInt64 Parse (string s)
		{
			throw new NotImplementedException ();
		}

		public static SqlInt64 Subtract (SqlInt64 x, SqlInt64 y)
		{
			return (x - y);
		}

		public static SqlBoolean ToSqlBoolean ()
		{
			if (value != 0) return SqlBoolean.True;
			if (value == 0) return SqlBoolean.False;

			return SqlBoolean.Null;
		}
		
		[MonoTODO]
		public static SqlByte ToSqlByte ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlDecimal ToSqlDecimal ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlDouble ToSqlDouble ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlInt16 ToSqlInt16 ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlInt32 ToSqlInt32 ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlMoney ToSqlMoney ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlSingle ToSqlSingle ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlString ToSqlString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public static SqlInt64 Xor (SqlInt64 x, SqlInt64 y)
		{
			return (x ^ y);
		}

		public static SqlInt64 operator + (SqlInt64 x, SqlInt64 y)
		{
			return new SqlInt64 (x.Value + y.Value);
		}

		public static SqlInt64 operator & (SqlInt64 x, SqlInt64 y)
		{
			return new SqlInt64 (x.value & y.Value);
		}

		public static SqlInt64 operator | (SqlInt64 x, SqlInt64 y)
		{
			return new SqlInt64 (x.value | y.Value);
		}

		public static SqlInt64 operator / (SqlInt64 x, SqlInt64 y)
		{
			return new SqlInt64 (x.Value / y.Value);
		}

		public static SqlBoolean operator == (SqlInt64 x, SqlInt64 y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlInt64 operator ^ (SqlInt64 x, SqlInt64 y)
		{
			return new SqlInt64 (x.Value ^ y.Value);
		}

		public static SqlBoolean operator > (SqlInt64 x, SqlInt64 y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlInt64 x, SqlInt64 y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlInt64 x, SqlInt64 y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlInt64 x, SqlInt64 y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlInt64 x, SqlInt64 y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value <= y.Value);
		}

		public static SqlInt64 operator % (SqlInt64 x, SqlInt64 y)
		{
			return new SqlInt64(x.Value % y.Value);
		}

		public static SqlInt64 operator * (SqlInt64 x, SqlInt64 y)
		{
			return new SqlInt64 (x.Value * y.Value);
		}

		public static SqlInt64 operator ~ (SqlInt64 x)
		{
			return new SqlInt64 (~(x.Value));
		}

		public static SqlInt64 operator - (SqlInt64 x, SqlInt64 y)
		{
			return new SqlInt64 (x.Value - y.Value);
		}

		public static SqlInt64 operator - (SqlInt64 n)
		{
			return new SqlInt64 (-(n.Value));
		}

		[MonoTODO]
		public static explicit operator SqlInt64 (SqlBoolean x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlInt64 (SqlDecimal x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlInt64 (SqlDouble x)
		{
			return new NotImplementedException ();
		}

		public static explicit operator long (SqlInt64 x)
		{
			return x.Value;
		}

		[MonoTODO]
		public static explicit operator SqlInt64 (SqlMoney x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlInt64 (SqlSingle x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlInt64 (SqlString x)
		{
			return new NotImplementedException ();
		}

		public static explicit operator SqlInt64 (long x)
		{
			return new SqlInt64 (x);
		}

		[MonoTODO]
		public static explicit operator SqlInt64 (SqlByte x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlInt64 (SqlInt16 x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlInt64 (SqlInt32 x)
		{
			return new NotImplementedException ();
		}

		#endregion
	}
}
			
