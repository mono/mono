//
// System.Data.SqlTypes.SqlDouble
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
	public struct SqlDouble : INullable, IComparable
	{
		#region Fields
		double value;

		private bool notNull;

		public static readonly SqlDouble MaxValue = new SqlDouble (1.7976931348623157e308);
		public static readonly SqlDouble MinValue = new SqlDouble (-1.7976931348623157e308);
		public static readonly SqlDouble Null;
		public static readonly SqlDouble Zero = new SqlDouble (0);

		#endregion

		#region Constructors

		public SqlDouble (double value) 
		{
			this.value = value;
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull { 
			get { return !notNull; }
		}

		public double Value { 
			get { 
				if (this.IsNull) 
					throw new SqlNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static SqlDouble Add (SqlDouble x, SqlDouble y)
		{
			return (x + y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SqlDouble))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlDouble"));
			else if (((SqlDouble)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SqlDouble)value).Value);
		}

		public static SqlDouble Divide (SqlDouble x, SqlDouble y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SqlDouble))
				return false;
			else
				return (bool) (this == (SqlDouble)value);
		}

		public static SqlBoolean Equals (SqlDouble x, SqlDouble y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			long LongValue = (long)value;
			return (int)(LongValue ^ (LongValue >> 32));
			
		}

		public static SqlBoolean GreaterThan (SqlDouble x, SqlDouble y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlDouble x, SqlDouble y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlDouble x, SqlDouble y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlDouble x, SqlDouble y)
		{
			return (x <= y);
		}

		public static SqlDouble Multiply (SqlDouble x, SqlDouble y)
		{
			return (x * y);
		}

		public static SqlBoolean NotEquals (SqlDouble x, SqlDouble y)
		{
			return (x != y);
		}

		public static SqlDouble Parse (string s)
		{
			return new SqlDouble (Double.Parse (s));
		}

		public static SqlDouble Subtract (SqlDouble x, SqlDouble y)
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

		public static SqlDouble operator + (SqlDouble x, SqlDouble y)
		{
			return new SqlDouble (x.Value + y.Value);
		}

		public static SqlDouble operator / (SqlDouble x, SqlDouble y)
		{
			return new SqlDouble (x.Value / y.Value);
		}

		public static SqlBoolean operator == (SqlDouble x, SqlDouble y)
		{
			if (x.IsNull || y.IsNull) 	
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlDouble x, SqlDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlDouble x, SqlDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlDouble x, SqlDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlDouble x, SqlDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlDouble x, SqlDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value <= y.Value);
		}

		public static SqlDouble operator * (SqlDouble x, SqlDouble y)
		{
			return new SqlDouble (x.Value * y.Value);
		}

		public static SqlDouble operator - (SqlDouble x, SqlDouble y)
		{
			return new SqlDouble (x.Value - y.Value);
		}

		public static SqlDouble operator - (SqlDouble n)
		{
			return new SqlDouble (-(n.Value));
		}

		public static explicit operator SqlDouble (SqlBoolean x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDouble ((double)x.ByteValue);
		}

		public static explicit operator double (SqlDouble x)
		{
			return x.Value;
		}

		public static explicit operator SqlDouble (SqlString x)
		{
			return SqlDouble.Parse (x.Value);
		}

		public static implicit operator SqlDouble (double x)
		{
			return new SqlDouble (x);
		}

		public static implicit operator SqlDouble (SqlByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDouble ((double)x.Value);
		}

		public static implicit operator SqlDouble (SqlDecimal x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDouble ((double)x.Value);
		}

		public static implicit operator SqlDouble (SqlInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDouble ((double)x.Value);
		}

		public static implicit operator SqlDouble (SqlInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDouble ((double)x.Value);
		}

		public static implicit operator SqlDouble (SqlInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDouble ((double)x.Value);
		}

		public static implicit operator SqlDouble (SqlMoney x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDouble ((double)x.Value);
		}

		public static implicit operator SqlDouble (SqlSingle x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDouble ((double)x.Value);
		}

		#endregion
	}
}
			
