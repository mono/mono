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
using System.Xml;
using System.Xml.Schema;
using System.Globalization;
using System.Xml.Serialization;

namespace System.Data.SqlTypes
{
#if NET_2_0
	[SerializableAttribute]
	[XmlSchemaProvider ("GetXsdType")]
#endif
	public struct SqlDateTime : INullable, IComparable
#if NET_2_0
				, IXmlSerializable
#endif
	{
		#region Fields

		private DateTime value;
		private bool notNull;

		public static readonly SqlDateTime MaxValue;
		public static readonly SqlDateTime MinValue;
		public static readonly SqlDateTime Null;
		public static readonly int SQLTicksPerHour = 1080000;
		public static readonly int SQLTicksPerMinute = 18000;
		public static readonly int SQLTicksPerSecond = 300;

		static readonly DateTime zero_day = new DateTime (1900, 1, 1);

		#endregion

		#region Constructors

		static SqlDateTime ()
		{
			DateTime t = new DateTime (9999, 12, 31, 23, 59, 59);
			long ticks = (long) (t.Ticks + (997 * 10000));
			MaxValue.value = new DateTime (ticks);
			MaxValue.notNull = true;

			MinValue.value = new DateTime (1753, 1, 1);
			MinValue.notNull = true;
		}

		public SqlDateTime (DateTime value)
		{
			this.value = value;
			notNull = true;
			CheckRange (this);
		}

		public SqlDateTime (int dayTicks, int timeTicks)
		{
			try {
				long ms = SQLTicksToMilliseconds (timeTicks);
				this.value = zero_day.AddDays (dayTicks).AddMilliseconds (ms);
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

		static int TimeSpanTicksToSQLTicks (long ticks)
		{
			return (int) ((ticks * SQLTicksPerSecond) / TimeSpan.TicksPerSecond);
		}

		static long SQLTicksToMilliseconds (int timeTicks)
		{
			return (long) (((timeTicks * 1000.0) / SQLTicksPerSecond) + 0.5);
		}

		public SqlDateTime (int year, int month, int day, int hour, int minute, int second, double millisecond)
		{
			try {
				long ticks = (long) (millisecond * TimeSpan.TicksPerMillisecond);
				long ms = SQLTicksToMilliseconds (TimeSpanTicksToSQLTicks (ticks));
				this.value = new DateTime (year, month, day, hour, minute, second).AddMilliseconds (ms);
			} catch (ArgumentOutOfRangeException ex) {
				throw new SqlTypeException (ex.Message);
			}
			notNull = true;
			CheckRange (this);
		}

		// Some genius in MS came up with 'bilisecond', and gave it the ambiguous definition of one-"billionth"
		// of a second.  I'm almost tempted to use a nanosecond or a picosecond depending on the current culture :-)
		// But, wait!! it turns out it's a microsecond, in reality.  AAAAAAAAAAAARGH.  Did this misguided
		// individual think that a millisecond is a millionth of a second and thus come up with the dastardly name
		// and the very wrong definition?
		public SqlDateTime (int year, int month, int day, int hour, int minute, int second, int bilisecond)
		{
			try {
				long ticks = bilisecond * 10;
				long ms = SQLTicksToMilliseconds (TimeSpanTicksToSQLTicks (ticks));
				this.value = new DateTime (year, month, day, hour, minute, second).AddMilliseconds (ms);
			} catch (ArgumentOutOfRangeException ex) {
				throw new SqlTypeException (ex.Message);
			}
			notNull = true;
			CheckRange (this);
		}

		#endregion

		#region Properties

		public int DayTicks {
			get { return (Value - zero_day).Days; }
		}

		public bool IsNull {
			get { return !notNull; }
		}

		public int TimeTicks {
			get { return TimeSpanTicksToSQLTicks (Value.TimeOfDay.Ticks); }
		}

		public DateTime Value {
			get {
				if (this.IsNull) 
					throw new SqlNullValueException ("The property contains Null.");
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
			if (!(value is SqlDateTime))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlDateTime"));

			return CompareTo ((SqlDateTime) value);
		}

#if NET_2_0
		public
#endif
		int CompareTo (SqlDateTime value)
		{
			if (value.IsNull)
				return 1;
			else
				return this.value.CompareTo (value.Value);
		}

		public override bool Equals (object value)
		{
			if (!(value is SqlDateTime))
				return false;
			else if (this.IsNull)
				return ((SqlDateTime)value).IsNull;
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

#if NET_2_0
		public static SqlDateTime Add (SqlDateTime x, TimeSpan t)
		{
			return (x + t);
		}

		public static SqlDateTime Subtract (SqlDateTime x, TimeSpan t)
		{
			return (x - t);
		}
#endif

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
			} catch (Exception) {
			}

			// try parsing in invariant culture
			try {
				return new SqlDateTime (DateTime.Parse (s, CultureInfo.InvariantCulture));
			} catch (Exception) {
			}

			throw new FormatException (String.Format ("String {0}" +
				" is not recognized as valid DateTime.", s));
		}

		public SqlString ToSqlString ()
		{
			return ((SqlString) this);
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
			return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlDateTime x, SqlDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
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

		public static implicit operator SqlDateTime (DateTime value)
		{
			return new SqlDateTime (value);
		}

#if NET_2_0
		public static XmlQualifiedName GetXsdType (XmlSchemaSet schemaSet)
		{
			XmlQualifiedName qualifiedName = new XmlQualifiedName ("dateTime", "http://www.w3.org/2001/XMLSchema");
			return qualifiedName;
		}
		
		[MonoTODO]
		XmlSchema IXmlSerializable.GetSchema ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		void IXmlSerializable.WriteXml (XmlWriter writer) 
		{
			throw new NotImplementedException ();
		}
#endif

		#endregion
	}
}
