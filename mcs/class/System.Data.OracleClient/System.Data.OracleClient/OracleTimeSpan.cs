//
// OracleTimeSpan.cs 
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

namespace System.Data.OracleClient {
	public struct OracleTimeSpan : IComparable, INullable
	{
		#region Fields

		public static readonly OracleTimeSpan MaxValue = new OracleTimeSpan (TimeSpan.MaxValue);
		public static readonly OracleTimeSpan MinValue = new OracleTimeSpan (TimeSpan.MinValue);
		public static readonly OracleTimeSpan Null = new OracleTimeSpan ();

		bool notNull; 
		TimeSpan value;

		#endregion // Fields

		#region Constructors

		public OracleTimeSpan (long ticks)
			: this (new TimeSpan (ticks))
		{
		}

		public OracleTimeSpan (OracleTimeSpan from)
			: this (from.Value)
		{
		}

		public OracleTimeSpan (TimeSpan ts)
		{
			value = ts;
			notNull = true;
		}

		public OracleTimeSpan (int hours, int minutes, int seconds)
			: this (new TimeSpan (hours, minutes, seconds))
		{
		}

		public OracleTimeSpan (int days, int hours, int minutes, int seconds)
			: this (new TimeSpan (days, hours, minutes, seconds))
		{
		}

		public OracleTimeSpan (int days, int hours, int minutes, int seconds, int milliseconds)
			: this (new TimeSpan (days, hours, minutes, seconds, milliseconds))
		{
		}

		#endregion // Constructors

		#region Properties

		public int Days {
			get { return Value.Days; }
		}

		public int Hours {
			get { return Value.Days; }
		}

		public bool IsNull {
			get { return !notNull; }
		}

		public int Milliseconds {
			get { return Value.Milliseconds; }
		}

		public int Minutes {
			get { return Value.Minutes; }
		}

		public int Seconds {
			get { return Value.Seconds; }
		}

		public TimeSpan Value {
			get { return value; }
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

		public static OracleBoolean Equals (OracleTimeSpan x, OracleTimeSpan y)
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

		public static OracleBoolean GreaterThan (OracleTimeSpan x, OracleTimeSpan y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x.Value > y.Value);
		}

		public static OracleBoolean GreaterThanOrEqual (OracleTimeSpan x, OracleTimeSpan y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x.Value >= y.Value);
		}

		public static OracleBoolean LessThan (OracleTimeSpan x, OracleTimeSpan y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x.Value < y.Value);
		}

		public static OracleBoolean LessThanOrEqual (OracleTimeSpan x, OracleTimeSpan y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x.Value <= y.Value);
		}

		public static OracleBoolean NotEquals (OracleTimeSpan x, OracleTimeSpan y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x.Value != y.Value);
		}
		
		public static OracleTimeSpan Parse (string s)
		{
			return new OracleTimeSpan (TimeSpan.Parse (s));
		}

		public override string ToString ()
		{
			if (IsNull)
				return "Null";
			return value.ToString ();
		}

		#endregion // Methods

		#region Operators and Type Conversions

		public static OracleBoolean operator == (OracleTimeSpan x, OracleTimeSpan y)
		{
			return Equals (x, y);
		}

		public static OracleBoolean operator > (OracleTimeSpan x, OracleTimeSpan y)
		{
			return GreaterThan (x, y);
		}

		public static OracleBoolean operator >= (OracleTimeSpan x, OracleTimeSpan y)
		{
			return GreaterThanOrEqual (x, y);
		}

		public static OracleBoolean operator != (OracleTimeSpan x, OracleTimeSpan y)
		{
			return NotEquals (x, y);
		}

		public static OracleBoolean operator < (OracleTimeSpan x, OracleTimeSpan y)
		{
			return LessThan (x, y);
		}

		public static OracleBoolean operator <= (OracleTimeSpan x, OracleTimeSpan y)
		{
			return LessThan (x, y);
		}

		public static explicit operator TimeSpan (OracleTimeSpan x)
		{
			return x.Value;
		}

		public static explicit operator OracleTimeSpan (string s)
		{
			return Parse (s);
		}

		#endregion // Operators and Type Conversions
	}
}
