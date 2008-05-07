//
// OracleMonthSpan.cs 
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

namespace System.Data.OracleClient
{
	public struct OracleMonthSpan : IComparable, INullable
	{
		#region Fields

		public static readonly OracleMonthSpan MaxValue = new OracleMonthSpan (176556);
		public static readonly OracleMonthSpan MinValue = new OracleMonthSpan (-176556);
		public static readonly OracleMonthSpan Null = new OracleMonthSpan ();

		bool notNull; 
		int value;

		#endregion // Fields

		#region Constructors

		public OracleMonthSpan (int months)
		{
			value = months;
			notNull = true;
		}

		public OracleMonthSpan (OracleMonthSpan from)
		{
			this.notNull = from.notNull;
			this.value = from.value;
		}

		public OracleMonthSpan (int years, int months)
			: this (years * 12 + months)
		{
		}

		#endregion // Constructors

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public int Value {
			get {
				if (IsNull)
					throw new Exception ("Data is null.");

				return value;
			}
		}

		#endregion // Properties

		#region Methods

		public int CompareTo (object obj)
		{
			OracleMonthSpan o = (OracleMonthSpan) obj;
			if (obj == null)
				throw new NullReferenceException ("Object reference not set to an instance of an object");
			else if (!(obj is OracleMonthSpan))
				throw new ArgumentException ("Value is not a System.Data.OracleClient.OracleMonthSpan", obj.ToString ());
			else if (o.IsNull && this.IsNull)
				return 0;
			else if (o.IsNull && !(this.IsNull))
				return 1;
			else
				return value.CompareTo (o.Value);
		}

		public override bool Equals (object value)
		{
			if (value is OracleMonthSpan) {
				OracleMonthSpan m = (OracleMonthSpan) value;
				return this.value == m.value;
			}
			return false;
		}

		public static OracleBoolean Equals (OracleMonthSpan x, OracleMonthSpan y)
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

		public static OracleBoolean GreaterThan (OracleMonthSpan x, OracleMonthSpan y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x.value > y.value);
		}

		public static OracleBoolean GreaterThanOrEqual (OracleMonthSpan x, OracleMonthSpan y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x.value >= y.value);
		}

		public static OracleBoolean LessThan (OracleMonthSpan x, OracleMonthSpan y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x.value < y.value);
		}

		public static OracleBoolean LessThanOrEqual (OracleMonthSpan x, OracleMonthSpan y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x.value <= y.value);
		}

		public static OracleBoolean NotEquals (OracleMonthSpan x, OracleMonthSpan y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x.value != y.value);
		}
		
		public static OracleMonthSpan Parse (string s)
		{
			return new OracleMonthSpan (Int32.Parse (s));
		}

		public override string ToString ()
		{
			if (IsNull)
				return "Null";
			return value.ToString ();
		}

		#endregion // Methods

		#region Operators and Type Conversions

		public static OracleBoolean operator == (OracleMonthSpan x, OracleMonthSpan y)
		{
			return Equals (x, y);
		}

		public static OracleBoolean operator > (OracleMonthSpan x, OracleMonthSpan y)
		{
			return GreaterThan (x, y);
		}

		public static OracleBoolean operator >= (OracleMonthSpan x, OracleMonthSpan y)
		{
			return GreaterThanOrEqual (x, y);
		}

		public static OracleBoolean operator != (OracleMonthSpan x, OracleMonthSpan y)
		{
			return NotEquals (x, y);
		}

		public static OracleBoolean operator < (OracleMonthSpan x, OracleMonthSpan y)
		{
			return LessThan (x, y);
		}

		public static OracleBoolean operator <= (OracleMonthSpan x, OracleMonthSpan y)
		{
			return LessThan (x, y);
		}

		public static explicit operator int (OracleMonthSpan x)
		{
			return x.value;
		}

		public static explicit operator OracleMonthSpan (string x)
		{
			return Parse (x);
		}

		#endregion // Operators and Type Conversions
	}
}
