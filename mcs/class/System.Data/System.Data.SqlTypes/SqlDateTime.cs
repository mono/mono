//
// System.Data.SqlTypes.SqlDateTime
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright 2002 Tim Coleman
//

using System;

namespace System.Data.SqlTypes
{
	public struct SqlDateTime : INullable, IComparable
	{
		#region Fields
		private DateTime value;

		public static readonly SqlDateTime MaxValue = new SqlDateTime (9999,12,31);
		public static readonly SqlDateTime MinValue = new SqlDateTime (1753,1,1);
		[MonoTODO]
		public static readonly SqlDateTime Null;
		public static readonly int SQLTicksPerHour;
		public static readonly int SQLTicksPerMinute;
		public static readonly int SQLTicksPerSecond;

		#endregion

		#region Constructors

		public SqlDateTime (DateTime value) 
		{
			this.value = value;
		}

		[MonoTODO]
		public SqlDateTime (int dayTicks, int timeTicks) 
		{
			throw new NotImplementedException ();
		}

		public SqlDateTime (int year, int month, int day) 
		{
			this.value = new DateTime (year, month, day);
		}

		public SqlDateTime (int year, int month, int day, int hour, int minute, int second) 
		{
			this.value = new DateTime (year, month, day, hour, minute, second);
		}

		[MonoTODO]
		public SqlDateTime (int year, int month, int day, int hour, int minute, int second, double millisecond) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlDateTime (int year, int month, int day, int hour, int minute, int second, int bilisecond) 
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Properties

		[MonoTODO]
		public int DayTicks {
			get { throw new NotImplementedException (); }
		}

		public bool IsNull { 
			get { return (bool) (this == SqlDateTime.Null); }
		}

		[MonoTODO]
		public int TimeTicks {
			get { throw new NotImplementedException (); }
		}

		public DateTime Value {
			get { return value; }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public int CompareTo (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Equals (object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean Equals (SqlDateTime x, SqlDateTime y)
		{
			return (x == y);
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return 42;
		}

		public static SqlBoolean GreaterThan (SqlDateTime x, SqlDateTime y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlDateTime x, SqlDateTime y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlDateTime x, SqlDateTime y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlDateTime x, SqlDateTime y)
		{
			return (x <= y);
		}

		public static SqlBoolean NotEquals (SqlDateTime x, SqlDateTime y)
		{
			return (x != y);
		}

		[MonoTODO]
		public static SqlDateTime Parse (string s)
		{
			throw new NotImplementedException ();
		}

		public SqlString ToSqlString ()
		{
			return new SqlString (value.ToString ());
		}

		public override string ToString ()
		{	
			return value.ToString ();
		}
	
		[MonoTODO]	
		public static SqlDateTime operator + (SqlDateTime x, TimeSpan t)
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean operator == (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value <= y.Value);
		}

		[MonoTODO]
		public static SqlByte operator - (SqlDateTime x, TimeSpan t)
		{
			throw new NotImplementedException ();
		}

		public static explicit operator DateTime (SqlDateTime x)
		{
			return x.Value;
		}

		[MonoTODO]
		public static explicit operator SqlDateTime (SqlString x)
		{
			throw new NotImplementedException();
		}

		public static explicit operator SqlDateTime (DateTime x)
		{
			return new SqlDateTime (x);
		}

		#endregion
	}
}
			
