//
// System.Data.SqlTypes.SqlDouble
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright 2002 Tim Coleman
//

using System;

namespace System.Data.SqlTypes
{
	public struct SqlDouble : INullable, IComparable
	{
		#region Fields
		private double value;

		public static readonly SqlDouble MaxValue = new SqlDouble (1.79E+308);
		public static readonly SqlDouble MinValue = new SqlDouble (-1.79E+308);
		public static readonly SqlDouble Null;
		public static readonly SqlDouble Zero = new SqlDouble (0);

		#endregion

		#region Constructors

		public SqlDouble (double value) 
		{
			this.value = value;
		}

		#endregion

		#region Properties

		[MonoTODO]
		public bool IsNull { 
			get { throw new NotImplementedException (); }
		}

		public double Value { 
			get { return value; }
		}

		#endregion

		#region Methods

		public static SqlDouble Add (SqlDouble x, SqlDouble y)
		{
			return (x + y);
		}

		[MonoTODO]
		public int CompareTo (object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlDouble Divide (SqlDouble x, SqlDouble y)
		{
			return (x / y);
		}

		[MonoTODO]
		public override bool Equals (object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean Equals (SqlDouble x, SqlDouble y)
		{
			return (x == y);
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return (int)value;
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

		[MonoTODO]
		public static SqlDouble Parse (string s)
		{
			throw new NotImplementedException ();
		}

		public static SqlDouble Subtract (SqlDouble x, SqlDouble y)
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
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlDouble x, SqlDouble y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlDouble x, SqlDouble y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlDouble x, SqlDouble y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlDouble x, SqlDouble y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlDouble x, SqlDouble y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
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

		[MonoTODO]
		public static explicit operator SqlDouble (SqlBoolean x)
		{
			return new NotImplementedException ();
		}

		public static explicit operator double (SqlDouble x)
		{
			return x.Value;
		}

		[MonoTODO]
		public static explicit operator SqlDouble (SqlString x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlDouble (double x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlDouble (SqlByte x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlDouble (SqlDecimal x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlDouble (SqlInt16 x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlDouble (SqlInt32 x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlDouble (SqlInt64 x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlDouble (SqlMoney x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlDouble (SqlSingle x)
		{
			return new NotImplementedException ();
		}

		#endregion
	}
}
			
