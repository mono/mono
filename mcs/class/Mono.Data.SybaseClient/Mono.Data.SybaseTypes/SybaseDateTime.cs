//
// Mono.Data.SybaseTypes.SybaseDateTime
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright Tim Coleman, 2002
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

		public static readonly SybaseDateTime MaxValue = new SybaseDateTime (9999,12,31);
		public static readonly SybaseDateTime MinValue = new SybaseDateTime (1753,1,1);
		public static readonly SybaseDateTime Null;
		public static readonly int SQLTicksPerHour;
		public static readonly int SQLTicksPerMinute;
		public static readonly int SQLTicksPerSecond;

		#endregion

		#region Constructors

		public SybaseDateTime (DateTime value) 
		{
			this.value = value;
			notNull = true;
		}

		[MonoTODO]
		public SybaseDateTime (int dayTicks, int timeTicks) 
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public SybaseDateTime (int year, int month, int day, int hour, int minute, int second, double millisecond) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SybaseDateTime (int year, int month, int day, int hour, int minute, int second, int bilisecond) 
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

		[MonoTODO]
		public override int GetHashCode ()
		{
			return 42;
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

		[MonoTODO]
		public static SybaseDateTime Parse (string s)
		{
			throw new NotImplementedException ();
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
	
		[MonoTODO]	
		public static SybaseDateTime operator + (SybaseDateTime x, TimeSpan t)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public static SybaseDateTime operator - (SybaseDateTime x, TimeSpan t)
		{
			throw new NotImplementedException ();
		}

		public static explicit operator DateTime (SybaseDateTime x)
		{
			return x.Value;
		}

		[MonoTODO]
		public static explicit operator SybaseDateTime (SybaseString x)
		{
			throw new NotImplementedException();
		}

		public static implicit operator SybaseDateTime (DateTime x)
		{
			return new SybaseDateTime (x);
		}

		#endregion
	}
}
			
