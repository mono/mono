//
// Mono.Data.TdsTypes.TdsDateTime
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright Tim Coleman, 2002
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

using Mono.Data.TdsClient;
using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Mono.Data.TdsTypes {
	public struct TdsDateTime : INullable, IComparable
	{
		#region Fields
		private DateTime value;
		private bool notNull;

		public static readonly TdsDateTime MaxValue = new TdsDateTime (9999,12,31);
		public static readonly TdsDateTime MinValue = new TdsDateTime (1753,1,1);
		public static readonly TdsDateTime Null;
		public static readonly int SQLTicksPerHour;
		public static readonly int SQLTicksPerMinute;
		public static readonly int SQLTicksPerSecond;

		#endregion

		#region Constructors

		public TdsDateTime (DateTime value) 
		{
			this.value = value;
			notNull = true;
		}

		[MonoTODO]
		public TdsDateTime (int dayTicks, int timeTicks) 
		{
			throw new NotImplementedException ();
		}

		public TdsDateTime (int year, int month, int day) 
		{
			this.value = new DateTime (year, month, day);
			notNull = true;
		}

		public TdsDateTime (int year, int month, int day, int hour, int minute, int second) 
		{
			this.value = new DateTime (year, month, day, hour, minute, second);
			notNull = true;
		}

		[MonoTODO]
		public TdsDateTime (int year, int month, int day, int hour, int minute, int second, double millisecond) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public TdsDateTime (int year, int month, int day, int hour, int minute, int second, int bilisecond) 
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
					throw new TdsNullValueException ("The property contains Null.");
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
			else if (!(value is TdsDateTime))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.TdsTypes.TdsDateTime"));
			else if (((TdsDateTime)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((TdsDateTime)value).Value);
		}

		public override bool Equals (object value)
		{
			if (!(value is TdsDateTime))
				return false;
			else
				return (bool) (this == (TdsDateTime)value);
		}

		public static TdsBoolean Equals (TdsDateTime x, TdsDateTime y)
		{
			return (x == y);
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return 42;
		}

		public static TdsBoolean GreaterThan (TdsDateTime x, TdsDateTime y)
		{
			return (x > y);
		}

		public static TdsBoolean GreaterThanOrEqual (TdsDateTime x, TdsDateTime y)
		{
			return (x >= y);
		}

		public static TdsBoolean LessThan (TdsDateTime x, TdsDateTime y)
		{
			return (x < y);
		}

		public static TdsBoolean LessThanOrEqual (TdsDateTime x, TdsDateTime y)
		{
			return (x <= y);
		}

		public static TdsBoolean NotEquals (TdsDateTime x, TdsDateTime y)
		{
			return (x != y);
		}

		[MonoTODO]
		public static TdsDateTime Parse (string s)
		{
			throw new NotImplementedException ();
		}

		public TdsString ToTdsString ()
		{
			return ((TdsString)this);
		}

		public override string ToString ()
		{	
			if (this.IsNull)
				return String.Empty;
			else
				return value.ToString ();
		}
	
		[MonoTODO]	
		public static TdsDateTime operator + (TdsDateTime x, TimeSpan t)
		{
			throw new NotImplementedException ();
		}

		public static TdsBoolean operator == (TdsDateTime x, TdsDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value == y.Value);
		}

		public static TdsBoolean operator > (TdsDateTime x, TdsDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value > y.Value);
		}

		public static TdsBoolean operator >= (TdsDateTime x, TdsDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value >= y.Value);
		}

		public static TdsBoolean operator != (TdsDateTime x, TdsDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (!(x.Value == y.Value));
		}

		public static TdsBoolean operator < (TdsDateTime x, TdsDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value < y.Value);
		}

		public static TdsBoolean operator <= (TdsDateTime x, TdsDateTime y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value <= y.Value);
		}

		[MonoTODO]
		public static TdsDateTime operator - (TdsDateTime x, TimeSpan t)
		{
			throw new NotImplementedException ();
		}

		public static explicit operator DateTime (TdsDateTime x)
		{
			return x.Value;
		}

		[MonoTODO]
		public static explicit operator TdsDateTime (TdsString x)
		{
			throw new NotImplementedException();
		}

		public static implicit operator TdsDateTime (DateTime x)
		{
			return new TdsDateTime (x);
		}

		#endregion
	}
}
			
