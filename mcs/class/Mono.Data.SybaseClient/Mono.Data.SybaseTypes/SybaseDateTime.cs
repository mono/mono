//
// Mono.Data.SybaseTypes.SybaseDateTime
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// Based on System.Data.SqlTypes.SqlDateTime
//
// (C) Ximian, Inc. 2002-2003
// (C) Copyright Tim Coleman, 2002-2003
//

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

using Mono.Data.SybaseClient;
using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Mono.Data.SybaseTypes {
	public struct SybaseDateTime : INullable, IComparable
	{
		#region Fields
		private DateTime value;
		private bool notNull;
		private static readonly float DateTimeTicksPerHour = 3.6E+10f;
		private static readonly DateTime Epoch = new DateTime (1900, 1, 1);

		public static readonly SybaseDateTime MaxValue = new SybaseDateTime (9999, 12, 31, 23, 59, 59);
		public static readonly SybaseDateTime MinValue = new SybaseDateTime (1753,1,1);
		public static readonly SybaseDateTime Null;
		public static readonly int SQLTicksPerHour = 1080000;
		public static readonly int SQLTicksPerMinute = 18000;
		public static readonly int SQLTicksPerSecond = 300;

		#endregion

		#region Constructors

		public SybaseDateTime (DateTime value) 
		{
			this.value = value;
			notNull = true;
		}

		public SybaseDateTime (int dayTicks, int timeTicks) 
		{
			this.value = new DateTime (Epoch.Ticks + (long) (dayTicks + timeTicks));
			notNull = true;
		}

		public SybaseDateTime (int year, int month, int day) 
		{
			this.value = new DateTime (year, month, day);
			notNull = true;
		}

		public SybaseDateTime (int year, int month, int day, int hour, int minute, int second) 
		{
			this.value = new DateTime (year, month, day, hour, minute, second);
			notNull = true;
		}

		public SybaseDateTime (int year, int month, int day, int hour, int minute, int second, double millisecond) 
		{
			DateTime t = new DateTime (year, month, day);
			this.value = new DateTime ((long) (t.Day * 24 * SQLTicksPerHour + hour * SQLTicksPerHour + minute * SQLTicksPerMinute + second * SQLTicksPerSecond + millisecond * 1000));
			notNull = true;
		}

		public SybaseDateTime (int year, int month, int day, int hour, int minute, int second, int bilisecond) 
		{
			DateTime t = new DateTime (year, month, day);
			this.value = new DateTime ((long) (t.Day * 24 * SQLTicksPerHour + hour * SQLTicksPerHour + minute * SQLTicksPerMinute + second * SQLTicksPerSecond + bilisecond));
			notNull = true;
		}

		#endregion

		#region Properties

		public int DayTicks {
			get { 
				return (int) ((this.Value.Ticks - Epoch.Ticks) / (24 * DateTimeTicksPerHour));
			}
		}

		public bool IsNull { 
			get { return !notNull; }
		}

		public int TimeTicks {
			get { 
				if (this.IsNull)
					throw new SybaseNullValueException ();
				return (int) (value.Hour * SQLTicksPerHour + value.Minute * SQLTicksPerMinute + value.Second * SQLTicksPerSecond + value.Millisecond);
			}
		}

		public DateTime Value {
			get { 
				if (this.IsNull) 
					throw new SybaseNullValueException ("The property contains Null.");
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
			else if (!(value is SybaseDateTime))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SybaseTypes.SybaseDateTime"));
			else if (((SybaseDateTime)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SybaseDateTime)value).Value);
		}

		public override bool Equals (object value)
		{
			if (!(value is SybaseDateTime))
				return false;
			else
				return (bool) (this == (SybaseDateTime)value);
		}

		public static SybaseBoolean Equals (SybaseDateTime x, SybaseDateTime y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			return value.GetHashCode ();
		}

		public static SybaseBoolean GreaterThan (SybaseDateTime x, SybaseDateTime y)
		{
			return (x > y);
		}

		public static SybaseBoolean GreaterThanOrEqual (SybaseDateTime x, SybaseDateTime y)
		{
			return (x >= y);
		}

		public static SybaseBoolean LessThan (SybaseDateTime x, SybaseDateTime y)
		{
			return (x < y);
		}

		public static SybaseBoolean LessThanOrEqual (SybaseDateTime x, SybaseDateTime y)
		{
			return (x <= y);
		}

		public static SybaseBoolean NotEquals (SybaseDateTime x, SybaseDateTime y)
		{
			return (x != y);
		}

		public static SybaseDateTime Parse (string s)
		{
			return new SybaseDateTime (DateTime.Parse (s));
		}

		public SybaseString ToSybaseString ()
		{
			return ((SybaseString)this);
		}

		public override string ToString ()
		{	
			if (this.IsNull)
				return String.Empty;
			else
				return value.ToString ();
		}
	
		public static SybaseDateTime operator + (SybaseDateTime x, TimeSpan t)
		{
			if (x.IsNull)
				return SybaseDateTime.Null;
			return new SybaseDateTime (x.Value + t);
		}

		public static SybaseBoolean operator == (SybaseDateTime x, SybaseDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value == y.Value);
		}

		public static SybaseBoolean operator > (SybaseDateTime x, SybaseDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value > y.Value);
		}

		public static SybaseBoolean operator >= (SybaseDateTime x, SybaseDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value >= y.Value);
		}

		public static SybaseBoolean operator != (SybaseDateTime x, SybaseDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (!(x.Value == y.Value));
		}

		public static SybaseBoolean operator < (SybaseDateTime x, SybaseDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value < y.Value);
		}

		public static SybaseBoolean operator <= (SybaseDateTime x, SybaseDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value <= y.Value);
		}

		public static SybaseDateTime operator - (SybaseDateTime x, TimeSpan t)
		{
			if (x.IsNull)
				return SybaseDateTime.Null;
			return new SybaseDateTime (x.Value - t);
		}

		public static explicit operator DateTime (SybaseDateTime x)
		{
			return x.Value;
		}

		public static explicit operator SybaseDateTime (SybaseString x)
		{
			return SybaseDateTime.Parse (x.Value);
		}

		public static implicit operator SybaseDateTime (DateTime x)
		{
			return new SybaseDateTime (x);
		}

		#endregion
	}
}
			
