//
// OracleDateTime.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Author: Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman, 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace System.Data.OracleClient {
	public struct OracleDateTime : IComparable, INullable
	{
		#region Fields

		public static readonly OracleDateTime MaxValue = new OracleDateTime (4712, 12, 31);
		public static readonly OracleDateTime MinValue = new OracleDateTime (1, 1, 1);
		public static readonly OracleDateTime Null = new OracleDateTime ();

		DateTime value;
		bool notNull;

		#endregion // Fields

		#region Constructors

		public OracleDateTime (DateTime dt)
		{
			value = dt; 
			notNull = true;
		}

		public OracleDateTime (long ticks)
			: this (new DateTime (ticks))
		{
		}

		public OracleDateTime (OracleDateTime from)
			: this (from.Value)
		{
		}

		public OracleDateTime (int year, int month, int day)
			: this (new DateTime (year, month, day))
		{
		}

		public OracleDateTime (int year, int month, int day, Calendar calendar)
			: this (new DateTime (year, month, day, calendar))
		{
		}

		public OracleDateTime (int year, int month, int day, int hour, int minute, int second)
			: this (new DateTime (year, month, day, hour, minute, second))
		{
		}

		public OracleDateTime (int year, int month, int day, int hour, int minute, int second, Calendar calendar)
			: this (new DateTime (year, month, day, hour, minute, second, calendar))
		{
		}

		public OracleDateTime (int year, int month, int day, int hour, int minute, int second, int millisecond)
			: this (new DateTime (year, month, day, hour, minute, second, millisecond))
		{
		}

		public OracleDateTime (int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar)
			: this (new DateTime (year, month, day, hour, minute, second, millisecond, calendar))
		{
		}

		#endregion // Constructors

		#region Properties

		public int Day {
			get { return value.Day; }
		}

		public int Hour {
			get { return value.Hour; }
		}

		public bool IsNull {
			get { return !notNull; }
		}

		public int Millisecond {
			get { return value.Millisecond; }
		}

		public int Minute {
			get { return value.Minute; }
		}

		public int Month {
			get { return value.Month; }
		}

		public int Second {
			get { return value.Second; }
		}

		public DateTime Value {
			get { return value; }
		}

		public int Year {
			get { return value.Year; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public int CompareTo (object obj)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Equals (object value)
		{
			throw new NotImplementedException ();
		}

		public static OracleBoolean Equals (OracleDateTime x, OracleDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value == y.Value);
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		public static OracleBoolean GreaterThan (OracleDateTime x, OracleDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value > y.Value);
		}

		public static OracleBoolean GreaterThanOrEqual (OracleDateTime x, OracleDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value >= y.Value);
		}

		public static OracleBoolean LessThan (OracleDateTime x, OracleDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value < y.Value);
		}

		public static OracleBoolean LessThanOrEqual (OracleDateTime x, OracleDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value <= y.Value);
		}

		public static OracleBoolean NotEquals (OracleDateTime x, OracleDateTime y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value != y.Value);
		}

		public static OracleDateTime Parse (string s)
		{
			return new OracleDateTime (DateTime.Parse (s));
		}

		public override string ToString ()
		{
			if (IsNull)
				return "Null";
			return Value.ToString ();
		}

		#endregion // Methods

		#region Operators and Type Conversions

		public static OracleBoolean operator == (OracleDateTime x, OracleDateTime y)
		{
			return Equals (x, y);
		}

		public static OracleBoolean operator > (OracleDateTime x, OracleDateTime y)
		{
			return GreaterThan (x, y);
		}

		public static OracleBoolean operator >= (OracleDateTime x, OracleDateTime y)
		{
			return GreaterThanOrEqual (x, y);
		}

		public static OracleBoolean operator != (OracleDateTime x, OracleDateTime y)
		{
			return NotEquals (x, y);
		}

		public static OracleBoolean operator < (OracleDateTime x, OracleDateTime y)
		{
			return LessThan (x, y);
		}

		public static OracleBoolean operator <= (OracleDateTime x, OracleDateTime y)
		{
			return LessThanOrEqual (x, y);
		}

		public static explicit operator DateTime (OracleDateTime x)
		{
			return x.Value;
		}

		public static explicit operator OracleDateTime (DateTime x)
		{
			return new OracleDateTime (x);
		}

		#endregion // Operators and Type Conversions
	}
}
