//
// System.Data.SqlTypes.SqlSingle
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
	public struct SqlSingle : INullable, IComparable
	{
		#region Fields

		float value;

		private bool notNull;

		public static readonly SqlSingle MaxValue = new SqlSingle (3.40282346638528859e38);
		public static readonly SqlSingle MinValue = new SqlSingle (-3.40282346638528859e38);
		public static readonly SqlSingle Null;
		public static readonly SqlSingle Zero = new SqlSingle (0);

		#endregion

		#region Constructors

		public SqlSingle (double value) 
		{
			this.value = (float)value;
			notNull = true;
		}

		public SqlSingle (float value) 
		{
			this.value = value;
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull { 
			get { return !notNull; }
		}

		public float Value { 
			get { 
				if (this.IsNull) 
					throw new SqlNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static SqlSingle Add (SqlSingle x, SqlSingle y)
		{
			return (x + y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SqlSingle))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlSingle"));
			else if (((SqlSingle)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SqlSingle)value).Value);
		}

		public static SqlSingle Divide (SqlSingle x, SqlSingle y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SqlSingle))
				return false;
			else
				return (bool) (this == (SqlSingle)value);
		}

		public static SqlBoolean Equals (SqlSingle x, SqlSingle y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			long LongValue = (long) value;
			return (int)(LongValue ^ (LongValue >> 32));
		}

		public static SqlBoolean GreaterThan (SqlSingle x, SqlSingle y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlSingle x, SqlSingle y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlSingle x, SqlSingle y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlSingle x, SqlSingle y)
		{
			return (x <= y);
		}

		public static SqlSingle Multiply (SqlSingle x, SqlSingle y)
		{
			return (x * y);
		}

		public static SqlBoolean NotEquals (SqlSingle x, SqlSingle y)
		{
			return (x != y);
		}

		public static SqlSingle Parse (string s)
		{
			return new SqlSingle (Single.Parse (s));
		}

		public static SqlSingle Subtract (SqlSingle x, SqlSingle y)
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

		public SqlInt64 ToSqlInt64 ()
		{
			return ((SqlInt64)this);
		}

		public SqlMoney ToSqlMoney ()
		{
			return ((SqlMoney)this);
		}


		public SqlString ToSqlString ()
		{
			return ((SqlString)this);
		}

		public override string ToString ()
		{
			return value.ToString ();
		}

		public static SqlSingle operator + (SqlSingle x, SqlSingle y)
		{
			return new SqlSingle (x.Value + y.Value);
		}

		public static SqlSingle operator / (SqlSingle x, SqlSingle y)
		{
			return new SqlSingle (x.Value / y.Value);
		}

		public static SqlBoolean operator == (SqlSingle x, SqlSingle y)
		{
			if (x.IsNull || y .IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlSingle x, SqlSingle y)
		{
			if (x.IsNull || y .IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlSingle x, SqlSingle y)
		{
			if (x.IsNull || y .IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlSingle x, SqlSingle y)
		{
			if (x.IsNull || y .IsNull) return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlSingle x, SqlSingle y)
		{
			if (x.IsNull || y .IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlSingle x, SqlSingle y)
		{
			if (x.IsNull || y .IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value <= y.Value);
		}

		public static SqlSingle operator * (SqlSingle x, SqlSingle y)
		{
			return new SqlSingle (x.Value * y.Value);
		}

		public static SqlSingle operator - (SqlSingle x, SqlSingle y)
		{
			return new SqlSingle (x.Value - y.Value);
		}

		public static SqlSingle operator - (SqlSingle n)
		{
			return new SqlSingle (-(n.Value));
		}

		public static explicit operator SqlSingle (SqlBoolean x)
		{
			return new SqlSingle((float)x.ByteValue);
		}

		public static explicit operator SqlSingle (SqlDouble x)
		{
			return new SqlSingle((float)x.Value);
		}

		public static explicit operator float (SqlSingle x)
		{
			return x.Value;
		}

		public static explicit operator SqlSingle (SqlString x)
		{
			return SqlSingle.Parse (x.Value);
		}

		public static implicit operator SqlSingle (float x)
		{
			return new SqlSingle (x);
		}

		public static implicit operator SqlSingle (SqlByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlSingle((float)x.Value);
		}

		public static implicit operator SqlSingle (SqlDecimal x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlSingle((float)x.Value);
		}

		public static implicit operator SqlSingle (SqlInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlSingle((float)x.Value);
		}

		public static implicit operator SqlSingle (SqlInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlSingle((float)x.Value);
		}

		public static implicit operator SqlSingle (SqlInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlSingle((float)x.Value);
		}

		public static implicit operator SqlSingle (SqlMoney x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlSingle((float)x.Value);
		}

		#endregion
	}
}
			
