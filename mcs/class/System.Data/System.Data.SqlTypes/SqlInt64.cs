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
		
		[MonoTODO]
		public static readonly SqlInt64 MaxValue; // 2^63 - 1

		[MonoTODO]
		public static readonly SqlInt64 MinValue; // -2^63

		[MonoTODO]
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
			get { return (bool) (this == SqlInt64.Null); }
		}

		public long Value { 
			get { 
				if (this.IsNull) 
					throw new SqlNullValueException ("The property contains Null.");
				else 
					return value; 
			}
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

		public SqlBoolean ToSqlBoolean ()
		{
			return ((SqlBoolean)this);
		}
		
		public SqlByte ToSqlByte ()
		{
			return ((SqlByte)this);
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
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlInt64 operator ^ (SqlInt64 x, SqlInt64 y)
		{
			return new SqlInt64 (x.Value ^ y.Value);
		}

		public static SqlBoolean operator > (SqlInt64 x, SqlInt64 y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlInt64 x, SqlInt64 y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlInt64 x, SqlInt64 y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlInt64 x, SqlInt64 y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlInt64 x, SqlInt64 y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
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

		public static explicit operator SqlInt64 (SqlBoolean x)
		{
			if (x.IsNull) 
				return SqlInt64.Null;
			else
				return new SqlInt64 ((long)x.ByteValue);
		}

		public static explicit operator SqlInt64 (SqlDecimal x)
		{
			if (x.IsNull) 
				return SqlInt64.Null;
			else
				return new SqlInt64 ((long)x.Value);
		}

		public static explicit operator SqlInt64 (SqlDouble x)
		{
			if (x.IsNull) 
				return SqlInt64.Null;
			else
				return new SqlInt64 ((long)x.Value);
		}

		public static explicit operator long (SqlInt64 x)
		{
			return x.Value;
		}

		public static explicit operator SqlInt64 (SqlMoney x)
		{
			if (x.IsNull) 
				return SqlInt64.Null;
			else
				return new SqlInt64 ((long)x.Value);
		}

		public static explicit operator SqlInt64 (SqlSingle x)
		{
			if (x.IsNull) 
				return SqlInt64.Null;
			else
				return new SqlInt64 ((long)x.Value);
		}

		[MonoTODO]
		public static explicit operator SqlInt64 (SqlString x)
		{
			throw new NotImplementedException ();
		}

		public static explicit operator SqlInt64 (long x)
		{
			return new SqlInt64 (x);
		}

		public static explicit operator SqlInt64 (SqlByte x)
		{
			if (x.IsNull) 
				return SqlInt64.Null;
			else
				return new SqlInt64 ((long)x.Value);
		}

		public static explicit operator SqlInt64 (SqlInt16 x)
		{
			if (x.IsNull) 
				return SqlInt64.Null;
			else
				return new SqlInt64 ((long)x.Value);
		}

		public static explicit operator SqlInt64 (SqlInt32 x)
		{
			if (x.IsNull) 
				return SqlInt64.Null;
			else
				return new SqlInt64 ((long)x.Value);
		}

		#endregion
	}
}
			
