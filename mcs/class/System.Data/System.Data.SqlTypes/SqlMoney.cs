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
		private decimal value;

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

		[MonoTODO]
		public SqlMoney (double value) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlMoney (int value) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlMoney (long value) 
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Properties

		[MonoTODO]
		public bool IsNull { 
			get { throw new NotImplementedException (); }
		}

		public decimal Value { 
			get { return value; }
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

		public static decimal ToDecimal ()
		{
			return value;
		}

		[MonoTODO]
		public static double ToDouble ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int ToInt32 ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static long ToInt64 ()
		{
			throw new NotImplementedException ();
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

		public static SqlDecimal ToSqlDecimal ()
		{
			return new SqlDecimal (value);
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
		public static SqlMoney ToSqlInt64 ()
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
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlMoney x, SqlMoney y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlMoney x, SqlMoney y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlMoney x, SqlMoney y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlMoney x, SqlMoney y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
			return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlMoney x, SqlMoney y)
		{
			if (x == null || y == null) return SqlBoolean.Null;
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

		[MonoTODO]
		public static explicit operator SqlMoney (SqlBoolean x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlMoney (SqlDecimal x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlMoney (SqlDouble x)
		{
			return new NotImplementedException ();
		}

		public static explicit operator decimal (SqlMoney x)
		{
			return x.Value;
		}

		[MonoTODO]
		public static explicit operator SqlMoney (SqlSingle x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlMoney (SqlString x)
		{
			return new NotImplementedException ();
		}

		public static explicit operator SqlMoney (decimal x)
		{
			return new SqlMoney (x);
		}

		[MonoTODO]
		public static explicit operator SqlMoney (SqlByte x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlMoney (SqlInt16 x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlMoney (SqlInt32 x)
		{
			return new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlMoney (SqlInt64 x)
		{
			return new NotImplementedException ();
		}

		#endregion
	}
}
			
