//
// System.Data.SqlTypes.SqlMoney
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright 2002 Tim Coleman
//

using System;

namespace System.Data.SqlTypes
{
	public struct SqlMoney : INullable, IComparable
	{
		#region Fields

		decimal value;

		public static readonly SqlMoney MaxValue = new SqlMoney (922337203685475.5807);
		public static readonly SqlMoney MinValue = new SqlMoney (-922337203685477.5808);
		public static readonly SqlMoney Null;
		public static readonly SqlMoney Zero = new SqlMoney (0);

		#endregion

		#region Constructors

		public SqlMoney (decimal value) 
		{
			this.value = value;
		}

		public SqlMoney (double value) 
		{
			this.value = (decimal)value;
		}

		public SqlMoney (int value) 
		{
			this.value = (decimal)value;
		}

		public SqlMoney (long value) 
		{
			this.value = (decimal)value;
		}

		#endregion

		#region Properties

		[MonoTODO]
		public bool IsNull { 
			get { return (bool) (this == Null); }
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

		public static SqlMoney Add (SqlMoney x, SqlMoney y)
		{
			return (x + y);
		}

		[MonoTODO]
		public int CompareTo (object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlMoney Divide (SqlMoney x, SqlMoney y)
		{
			return (x / y);
		}

		[MonoTODO]
		public override bool Equals (object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean Equals (SqlMoney x, SqlMoney y)
		{
			return (x == y);
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return (int)value;
		}

		public static SqlBoolean GreaterThan (SqlMoney x, SqlMoney y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlMoney x, SqlMoney y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlMoney x, SqlMoney y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlMoney x, SqlMoney y)
		{
			return (x <= y);
		}

		public static SqlMoney Multiply (SqlMoney x, SqlMoney y)
		{
			return (x * y);
		}

		public static SqlBoolean NotEquals (SqlMoney x, SqlMoney y)
		{
			return (x != y);
		}

		[MonoTODO]
		public static SqlMoney Parse (string s)
		{
			throw new NotImplementedException ();
		}

		public static SqlMoney Subtract (SqlMoney x, SqlMoney y)
		{
			return (x - y);
		}

		public decimal ToDecimal ()
		{
			return value;
		}

		public double ToDouble ()
		{
			return (double)value;
		}

		public int ToInt32 ()
		{
			return (int)value;
		}

		public long ToInt64 ()
		{
			return (long)value;
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

		public static SqlMoney operator + (SqlMoney x, SqlMoney y)
		{
			return new SqlMoney (x.Value + y.Value);
		}

		public static SqlMoney operator / (SqlMoney x, SqlMoney y)
		{
			return new SqlMoney (x.Value / y.Value);
		}

		public static SqlBoolean operator == (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value <= y.Value);
		}

		public static SqlMoney operator * (SqlMoney x, SqlMoney y)
		{
			return new SqlMoney (x.Value * y.Value);
		}

		public static SqlMoney operator - (SqlMoney x, SqlMoney y)
		{
			return new SqlMoney (x.Value - y.Value);
		}

		public static SqlMoney operator - (SqlMoney n)
		{
			return new SqlMoney (-(n.Value));
		}

		public static explicit operator SqlMoney (SqlBoolean x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlMoney ((decimal)x.ByteValue);
		}

		public static explicit operator SqlMoney (SqlDecimal x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlMoney (x.Value);
		}

		public static explicit operator SqlMoney (SqlDouble x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlMoney ((decimal)x.Value);
		}

		public static explicit operator decimal (SqlMoney x)
		{
			return x.Value;
		}

		public static explicit operator SqlMoney (SqlSingle x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlMoney ((decimal)x.Value);
		}

		[MonoTODO]
		public static explicit operator SqlMoney (SqlString x)
		{
			throw new NotImplementedException ();
		}

		public static implicit operator SqlMoney (decimal x)
		{
			return new SqlMoney (x);
		}

		public static implicit operator SqlMoney (SqlByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlMoney ((decimal)x.Value);
		}

		public static implicit operator SqlMoney (SqlInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlMoney ((decimal)x.Value);
		}

		public static implicit operator SqlMoney (SqlInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlMoney ((decimal)x.Value);
		}

		public static implicit operator SqlMoney (SqlInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlMoney ((decimal)x.Value);
		}

		#endregion
	}
}
			
