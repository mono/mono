//
// System.Data.SqlTypes.SqlDecimal
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright 2002 Tim Coleman
//

using System;

namespace System.Data.SqlTypes
{
	public struct SqlDecimal : INullable, IComparable
	{
		#region Fields
		private decimal value;

		public static readonly byte MaxPrecision = 38; 
		public static readonly byte MaxScale; 
		public static readonly SqlDecimal MaxValue;
		public static readonly SqlDecimal MinValue;
		public static readonly SqlDecimal Null;

		#endregion

		#region Constructors

		public SqlDecimal (decimal value) 
		{
			this.value = value;
		}

		[MonoTODO]
		public SqlDecimal (double value) 
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public SqlDecimal (int value) 
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public SqlDecimal (long value) 
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public SqlDecimal (byte bPrecision, byte bScale, bool fPositive, int[] bits)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public SqlDecimal (byte bPrecision, byte bScale, bool fPositive, int data1, int data2, int data3, int data4) 
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Properties

		[MonoTODO]
		public byte[] BinData {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public byte[] Data { 
			get { throw new NotImplementedException (); }
		}

		public bool IsNull { 
			get { throw new NotImplementedException (); }
		}

		public bool IsPositive { 
			get { throw new NotImplementedException (); }
		}

		public byte Precision { 
			get { throw new NotImplementedException (); }
		}

		public byte Scale { 
			get { throw new NotImplementedException (); }
		}

		public byte Value { 
			get { return value; }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public static SqlDecimal Abs (SqlDecimal n)
		{
			return new NotImplementedException();
		}

		public static SqlDecimal Add (SqlDecimal x, SqlDecimal y)
		{
			return (x + y);
		}

		[MonoTODO]
		public static SqlDecimal Ceiling (SqlDecimal n)
		{
			return new NotImplementedException();
		}

		[MonoTODO]
		public int CompareTo (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlDecimal ConvertToPrecScale (SqlDecimal n, int precision, int scale)
		{
			throw new NotImplementedException ();
		}

		public static SqlDecimal Divide (SqlDecimal x, SqlDecimal y)
		{
			return (x / y);
		}

		[MonoTODO]
		public override bool Equals (object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean Equals (SqlDecimal x, SqlDecimal y)
		{
			return (x == y);
		}

		[MonoTODO]
		public static SqlDecimal Floor (SqlDecimal n)
		{
			return new NotImplementedException();
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return (int)value;
		}

		public static SqlBoolean GreaterThan (SqlDecimal x, SqlDecimal y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlDecimal x, SqlDecimal y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlDecimal x, SqlDecimal y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlDecimal x, SqlDecimal y)
		{
			return (x <= y);
		}

		public static SqlDecimal Multiply (SqlDecimal x, SqlDecimal y)
		{
			return (x * y);
		}

		public static SqlBoolean NotEquals (SqlDecimal x, SqlDecimal y)
		{
			return (x != y);
		}

		[MonoTODO]
		public static SqlDecimal Parse (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlDecimal Power (SqlDecimal n, double exp)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlDecimal Round (SqlDecimal n, int position)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlInt32 Sign (SqlDecimal n)
		{
			throw new NotImplementedException ();
		}

		public static SqlDecimal Subtract (SqlDecimal x, SqlDecimal y)
		{
			return (x - y);
		}

		[MonoTODO]
		public static double ToDouble ()
		{
			return new NotImplementedException ();
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

		[MonoTODO]
		public static SqlDecimal Truncate (SqlDecimal n, int position)
		{
			throw new NotImplementedException ();
		}

		public static SqlDecimal operator + (SqlDecimal x, SqlDecimal y)
		{
			return new SqlDecimal (x.Value + y.Value);
		}

		public static SqlDecimal operator / (SqlDecimal x, SqlDecimal y)
		{
			return new SqlDecimal (x.Value / y.Value);
		}

		public static SqlBoolean operator == (SqlDecimal x, SqlDecimal y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlDecimal x, SqlDecimal y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlDecimal x, SqlDecimal y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlDecimal x, SqlDecimal y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlDecimal x, SqlDecimal y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlDecimal x, SqlDecimal y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value <= y.Value);
		}

		public static SqlDecimal operator * (SqlDecimal x, SqlDecimal y)
		{
			return new SqlDecimal (x.Value * y.Value);
		}

		public static SqlDecimal operator - (SqlDecimal x, SqlDecimal y)
		{
			return new SqlDecimal (x.Value - y.Value);
		}

		public static SqlDecimal operator - (SqlDecimal n)
		{
			return new SqlDecimal (-(n.Value));
		}

		[MonoTODO]
		public static explicit operator SqlDecimal (SqlBoolean x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator Decimal (SqlDecimal n)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlDecimal (SqlDouble x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlDecimal (SqlSingle x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlDecimal (SqlString x)
		{
			return new NotImplementedException ();
		}

		public static explicit operator SqlDecimal (decimal x)
		{
			return new SqlDecimal (x);
		}

		[MonoTODO]
		public static explicit operator SqlDecimal (SqlByte x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlDecimal (SqlInt16 x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlDecimal (SqlInt32 x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlDecimal (SqlInt64 x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlDecimal (SqlMoney x)
		{
			return new NotImplementedException ();
		}

		#endregion
	}
}
			
