//
// System.Data.SqlTypes.SqlSingle
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright 2002 Tim Coleman
//

using System;

namespace System.Data.SqlTypes
{
	public struct SqlSingle : INullable, IComparable
	{
		#region Fields

		private float value;

		public static readonly SqlSingle MaxValue = new SqlSingle (3.40E+38);
		public static readonly SqlSingle MinValue = new SqlSingle (-3.40E+38);
		[MonoTODO]
		public static readonly SqlSingle Null;
		public static readonly SqlSingle Zero = new SqlSingle (0);

		#endregion

		#region Constructors

		public SqlSingle (double value) 
		{
			this.value = (float)value;
		}

		public SqlSingle (float value) 
		{
			this.value = value;
		}

		#endregion

		#region Properties

		public bool IsNull { 
			get { return (bool) (this == SqlSingle.Null); }
		}

		public float Value { 
			get { 
				if (this.IsNull) 
					throw new SqlNullValueException ("The property contains Null.");
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

		[MonoTODO]
		public int CompareTo (object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlSingle Divide (SqlSingle x, SqlSingle y)
		{
			return (x / y);
		}

		[MonoTODO]
		public override bool Equals (object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean Equals (SqlSingle x, SqlSingle y)
		{
			return (x == y);
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return (int)value;
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

		[MonoTODO]
		public static SqlSingle Parse (string s)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public static explicit operator SqlSingle (SqlString x)
		{
			throw new NotImplementedException ();
		}

		public static explicit operator SqlSingle (float x)
		{
			return new SqlSingle (x);
		}

		public static explicit operator SqlSingle (SqlByte x)
		{
			if (x.IsNull) 
				return SqlSingle.Null;
			else
				return new SqlSingle((float)x.Value);
		}

		public static explicit operator SqlSingle (SqlDecimal x)
		{
			if (x.IsNull) 
				return SqlSingle.Null;
			else
				return new SqlSingle((float)x.Value);
		}

		public static explicit operator SqlSingle (SqlInt16 x)
		{
			if (x.IsNull) 
				return SqlSingle.Null;
			else
				return new SqlSingle((float)x.Value);
		}

		public static explicit operator SqlSingle (SqlInt32 x)
		{
			if (x.IsNull) 
				return SqlSingle.Null;
			else
				return new SqlSingle((float)x.Value);
		}

		public static explicit operator SqlSingle (SqlInt64 x)
		{
			if (x.IsNull) 
				return SqlSingle.Null;
			else
				return new SqlSingle((float)x.Value);
		}

		public static explicit operator SqlSingle (SqlMoney x)
		{
			if (x.IsNull) 
				return SqlSingle.Null;
			else
				return new SqlSingle((float)x.Value);
		}

		#endregion
	}
}
			
