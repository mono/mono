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

		[MonoTODO]
		public static readonly byte MaxScale;  // ????

		public static readonly SqlDecimal MaxValue = new SqlDecimal (79228162514264337593543950335.0);
		public static readonly SqlDecimal MinValue = new SqlDecimal (-79228162514264337593543950335.0);
		public static readonly SqlDecimal Null;

		#endregion

		#region Constructors

		public SqlDecimal (decimal value) 
		{
			this.value = value;
		}

		public SqlDecimal (double value) 
		{
			this.value = ((decimal)value);
		}

		public SqlDecimal (int value) 
		{
			this.value = ((decimal)value);
		}

		public SqlDecimal (long value) 
		{
			this.value = ((decimal)value);
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
			get { return (bool) (this == Null); }
		}

		public bool IsPositive { 
			get { return (this.Value > 0); }
		}

		[MonoTODO]
		public byte Precision { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public byte Scale { 
			get { throw new NotImplementedException (); }
		}

		public decimal Value { 
			get { 
				if (this.IsNull) 
					throw new SqlNullValueException ("The property contains Null.");
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		[MonoTODO]
		public static SqlDecimal Abs (SqlDecimal n)
		{
			throw new NotImplementedException();
		}

		public static SqlDecimal Add (SqlDecimal x, SqlDecimal y)
		{
			return (x + y);
		}

		[MonoTODO]
		public static SqlDecimal AdjustScale (SqlDecimal n, int digits, bool fRound)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlDecimal Ceiling (SqlDecimal n)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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

		public double ToDouble ()
		{
			return ((double)value);
		}

		public SqlBoolean ToSqlBoolean ()
		{
			return ((SqlBoolean)this);
		}
		
		public SqlByte ToSqlByte ()
		{
			return ((SqlByte)this);
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
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
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

		public static explicit operator SqlDecimal (SqlBoolean x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.ByteValue);
		}

		public static explicit operator Decimal (SqlDecimal n)
		{
			return n.Value;
		}

		public static explicit operator SqlDecimal (SqlDouble x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.Value);
		}

		public static explicit operator SqlDecimal (SqlSingle x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.Value);
		}

		[MonoTODO]
		public static explicit operator SqlDecimal (SqlString x)
		{
			throw new NotImplementedException ();
		}

		public static implicit operator SqlDecimal (decimal x)
		{
			return new SqlDecimal (x);
		}

		public static implicit operator SqlDecimal (SqlByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.Value);
		}

		public static implicit operator SqlDecimal (SqlInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.Value);
		}

		public static implicit operator SqlDecimal (SqlInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.Value);
		}

		public static implicit operator SqlDecimal (SqlInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.Value);
		}

		public static implicit operator SqlDecimal (SqlMoney x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.Value);
		}

		#endregion
	}
}
			
