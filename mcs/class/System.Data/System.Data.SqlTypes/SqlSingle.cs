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
		public static readonly SqlSingle Null;
		public static readonly SqlSingle Zero = new SqlSingle (0);

		#endregion

		#region Constructors

		[MonoTODO]
		public SqlSingle (double value) 
		{
			throw new NotImplementedException ();
		}

		public SqlSingle (float value) 
		{
			this.value = value;
		}

		#endregion

		#region Properties

		[MonoTODO]
		public bool IsNull { 
			get { throw new NotImplementedException (); }
		}

		public float Value { 
			get { return value; }
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
		public static SqlString ToSqlString ()
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
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlSingle x, SqlSingle y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlSingle x, SqlSingle y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlSingle x, SqlSingle y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlSingle x, SqlSingle y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlSingle x, SqlSingle y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
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

		[MonoTODO]
		public static explicit operator SqlSingle (SqlBoolean x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlSingle (SqlDouble x)
		{
			return new NotImplementedException ();
		}

		public static explicit operator float (SqlSingle x)
		{
			return x.Value;
		}

		[MonoTODO]
		public static explicit operator SqlSingle (SqlString x)
		{
			return new NotImplementedException ();
		}

		public static explicit operator SqlSingle (float x)
		{
			return new SqlSingle (x);
		}

		[MonoTODO]
		public static explicit operator SqlSingle (SqlByte x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlSingle (SqlDecimal x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlSingle (SqlInt16 x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlSingle (SqlInt32 x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlSingle (SqlInt64 x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlSingle (SqlMoney x)
		{
			return new NotImplementedException ();
		}

		#endregion
	}
}
			
