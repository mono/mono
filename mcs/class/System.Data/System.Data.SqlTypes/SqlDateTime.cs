//
// System.Data.SqlTypes.SqlDateTime
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//   Ville Palo <vi64pa@koti.soon.fi>
//
// (C) Copyright 2002 Tim Coleman
//

using System;
using System.Globalization;

namespace System.Data.SqlTypes
{
	public struct SqlDateTime : INullable, IComparable
	{
		#region Fields
		private DateTime value;
		private bool notNull;

		public static readonly SqlDateTime MaxValue = new SqlDateTime (9999,12,31);
		public static readonly SqlDateTime MinValue = new SqlDateTime (1753,1,1);
		public static readonly SqlDateTime Null;
		public static readonly int SQLTicksPerHour;
		public static readonly int SQLTicksPerMinute;
		public static readonly int SQLTicksPerSecond;

		#endregion

		#region Constructors

		public SqlDateTime (DateTime value) 
		{
			this.value = value;
			notNull = true;
		}

		[MonoTODO]
		public SqlDateTime (int dayTicks, int timeTicks) 
		{
			throw new NotImplementedException ();
		}

		public SqlDateTime (int year, int month, int day) 
		{
			this.value = new DateTime (year, month, day);
			notNull = true;
		}

		public SqlDateTime (int year, int month, int day, int hour, int minute, int second) 
		{
			this.value = new DateTime (year, month, day, hour, minute, second);
			notNull = true;
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
			get { return !notNull; }
		}

		[MonoTODO]
		public int TimeTicks {
			get { throw new NotImplementedException (); }
		}

		public DateTime Value {
			get { 
				if (this.IsNull) 
					throw new SqlNullValueException ("The property contains Null.");
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SqlDateTime))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlDateTime"));
			else if (((SqlDateTime)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SqlDateTime)value).Value);
		}

		public override bool Equals (object value)
		{
			if (!(value is SqlDateTime))
				return false;
			else if (this.IsNull && ((SqlDateTime)value).IsNull)
				return true;
			else if (((SqlDateTime)value).IsNull)
				return false;
			else
				return (bool) (this == (SqlDateTime)value);
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
			return ((SqlString)this);
		}

		public override string ToString ()
		{	
			if (this.IsNull)
				return String.Empty;
			else
				return value.ToString ();
		}
	
		[MonoTODO]	
		public static SqlDateTime operator + (SqlDateTime x, TimeSpan t)
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean operator == (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value <= y.Value);
		}

		[MonoTODO]
		public static SqlDateTime operator - (SqlDateTime x, TimeSpan t)
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

		public static implicit operator SqlDateTime (DateTime x)
		{
			return new SqlDateTime (x);
		}

		#endregion
	}
}
			
