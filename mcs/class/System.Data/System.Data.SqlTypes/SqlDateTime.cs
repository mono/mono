//
// System.Data.SqlTypes.SqlDateTime
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//   Ville Palo <vi64pa@koti.soon.fi>
//
// (C) Copyright 2002 Tim Coleman
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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

		public static readonly SqlDateTime MaxValue = new SqlDateTime (9999,12,31,23,59,59);
		public static readonly SqlDateTime MinValue = new SqlDateTime (1753,1,1);
		public static readonly SqlDateTime Null;
		public static readonly int SQLTicksPerHour = 1080000;
		public static readonly int SQLTicksPerMinute = 18000;
		public static readonly int SQLTicksPerSecond = 300;
	      
		#endregion

		#region Constructors

		public SqlDateTime (DateTime value) 
		{
			this.value = value;
			notNull = true;
			CheckRange (this);
		}

		public SqlDateTime (int dayTicks, int timeTicks) 
		{
			try {
				DateTime temp = new DateTime (1900, 1, 1);
				this.value = new DateTime (temp.Ticks + (long)(dayTicks + timeTicks));
			} catch (ArgumentOutOfRangeException ex) {
				throw new SqlTypeException (ex.Message);
			}
			notNull = true;
			CheckRange (this);
		}

		public SqlDateTime (int year, int month, int day) 
		{
			try {
				this.value = new DateTime (year, month, day);
			} catch (ArgumentOutOfRangeException ex) {
				throw new SqlTypeException (ex.Message);
			}
			notNull = true;
			CheckRange (this);
		}

		public SqlDateTime (int year, int month, int day, int hour, int minute, int second) 
		{
			try {
				this.value = new DateTime (year, month, day, hour, minute, second);
			} catch (ArgumentOutOfRangeException ex) {
				throw new SqlTypeException (ex.Message);
			}
			notNull = true;
			CheckRange (this);
		}

		[MonoTODO ("Round milisecond")]
		public SqlDateTime (int year, int month, int day, int hour, int minute, int second, double millisecond) 
		{
			try {
				DateTime t = new DateTime(year, month, day, hour, minute, second);
			
				long ticks = (long) (t.Ticks + millisecond * 10000);
				this.value = new DateTime (ticks);
			} catch (ArgumentOutOfRangeException ex) {
				throw new SqlTypeException (ex.Message);
			}
			notNull = true;
			CheckRange (this);
		}

		[MonoTODO ("Round bilisecond")]
		public SqlDateTime (int year, int month, int day, int hour, int minute, int second, int bilisecond) // bilisecond??
		{
			try {
				DateTime t = new DateTime(year, month, day, hour, minute, second);
			
				long dateTick = (long) (t.Ticks + bilisecond * 10);
				this.value = new DateTime (dateTick);
			} catch (ArgumentOutOfRangeException ex) {
				throw new SqlTypeException (ex.Message);
			}
			notNull = true;
			CheckRange (this);
		}

		#endregion

		#region Properties

		public int DayTicks {
			get { 
				float DateTimeTicksPerHour = 3.6E+10f;

				DateTime temp = new DateTime (1900, 1, 1);
				
				int result = (int)((this.Value.Ticks - temp.Ticks) / (24 * DateTimeTicksPerHour));
				return result;
			}
		}

		public bool IsNull { 
			get { return !notNull; }
		}

		public int TimeTicks {
			get {
				if (this.IsNull)
					throw new SqlNullValueException ();

				return (int)(value.Hour * SQLTicksPerHour + 
					     value.Minute * SQLTicksPerMinute +
					     value.Second * SQLTicksPerSecond +
					     value.Millisecond);
			}
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
		private static void CheckRange (SqlDateTime target)
		{
			if (target.IsNull)
				return;
			if (target.value > MaxValue.value || target.value < MinValue.value)
				throw new SqlTypeException (String.Format ("SqlDateTime overflow. Must be between {0} and {1}. Value was {2}", MinValue.Value, MaxValue.Value, target.value));
		}

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

		public override int GetHashCode ()
		{
			return value.GetHashCode ();
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

		public static SqlDateTime Parse (string s)
		{
                        if (s == null)
                                throw new ArgumentNullException ("Argument cannot be null");

                        // try parsing in local culture
                        DateTimeFormatInfo fmtInfo = DateTimeFormatInfo.CurrentInfo;
                        try {
                                return new SqlDateTime (DateTime.Parse (s, fmtInfo));
                        } catch (Exception e) {
                                // try parsing in invariant culture
                                return new SqlDateTime (DateTime.Parse (s, CultureInfo.InvariantCulture));
                        }
                        throw new FormatException (String.Format ("String {0} is not recognized as "+
                                                                  " valid DateTime.", s));
		}

		public SqlString ToSqlString ()
		{
			return ((SqlString)this);
		}

		public override string ToString ()
		{	
			if (this.IsNull)
				return "Null";
			else
				return value.ToString (CultureInfo.InvariantCulture);
		}
	
		public static SqlDateTime operator + (SqlDateTime x, TimeSpan t)
		{
			if (x.IsNull)
				return SqlDateTime.Null;
			
			return new SqlDateTime (x.Value + t);
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

		public static SqlDateTime operator - (SqlDateTime x, TimeSpan t)
		{
			if (x.IsNull)
				return x;
			return new SqlDateTime (x.Value - t);
		}

		public static explicit operator DateTime (SqlDateTime x)
		{
			return x.Value;
		}

		public static explicit operator SqlDateTime (SqlString x)
		{
			return SqlDateTime.Parse (x.Value);
		}

		public static implicit operator SqlDateTime (DateTime x)
		{
			return new SqlDateTime (x);
		}

		#endregion
	}
}
			
