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

		[MonoTODO]
		public bool IsNull {
			get { throw new NotImplementedException (); }
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

		public static SqlBoolean ToSqlBoolean ()
		{
			if (value != 0) return SqlBoolean.True;
			if (value == 0) return SqlBoolean.False;

			return SqlBoolean.Null;
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
		public static SqlInt64 ToSqlInt64 ()
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

		public static SqlByte Xor (SqlByte x, SqlByte y)
		{
			return (x ^ y);
		}

		public static SqlByte operator + (SqlByte x, SqlByte y)
		{
			return new SqlByte (x.Value + y.Value);
		}

		public static SqlByte operator & (SqlByte x, SqlByte y)
		{
			return new SqlByte (x.Value & y.Value);
		}

		public static SqlByte operator | (SqlByte x, SqlByte y)
		{
			return new SqlByte (x.Value | y.Value);
		}

		public static SqlByte operator / (SqlByte x, SqlByte y)
		{
			return new SqlByte (x.Value / y.Value);
		}

		public static SqlBoolean operator == (SqlByte x, SqlByte y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlByte operator ^ (SqlByte x, SqlByte y)
		{
			return new SqlByte (x.Value ^ y.Value);
		}

		public static SqlBoolean operator > (SqlByte x, SqlByte y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlByte x, SqlByte y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlByte x, SqlByte y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlByte x, SqlByte y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlByte x, SqlByte y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value <= y.Value);
		}

		public static SqlByte operator % (SqlByte x, SqlByte y)
		{
			return new SqlByte (x.Value % y.Value);
		}

		public static SqlByte operator * (SqlByte x, SqlByte y)
		{
			return new SqlByte (x.Value * y.Value);
		}

		//public static SqlByte operator ~ (SqlByte x)
		//{
			//return new SqlByte (~(x.Value));
		//}

		public static SqlByte operator - (SqlByte x, SqlByte y)
		{
			return new SqlByte (x.Value - y.Value);
		}

		public static explicit operator SqlByte (SqlBoolean x)
		{
			return new SqlByte (x.ByteValue);
		}

		public static explicit operator byte (SqlByte x)
		{
			return x.Value;
		}

		[MonoTODO]
		public static explicit operator SqlByte (SqlDecimal x)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlByte (SqlDouble x)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlByte (SqlInt16 x)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlByte (SqlInt16 x)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlByte (SqlInt32 x)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlByte (SqlInt64 x)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlByte (SqlMoney x)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlByte (SqlSingle x)
		{
			throw new NotImplementedException ();
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
			
