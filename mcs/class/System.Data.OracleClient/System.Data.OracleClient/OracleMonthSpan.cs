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

namespace System.Data.OracleClient {
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
			: this (from)
		{
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

		public byte[] Value {
			get {
				if (IsNull)
					throw new Exception ("Data is null.");
				return value;
			}
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
			return (x.Value > y.Value);
		}

		public static OracleBoolean GreaterThanOrEqual (OracleMonthSpan x, OracleMonthSpan y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x.Value >= y.Value);
		}

		public static OracleBoolean LessThan (OracleMonthSpan x, OracleMonthSpan y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x.Value < y.Value);
		}

		public static OracleBoolean LessThanOrEqual (OracleMonthSpan x, OracleMonthSpan y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x.Value <= y.Value);
		}

		public static OracleBoolean NotEquals (OracleMonthSpan x, OracleMonthSpan y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x.Value != y.Value);
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
			return x.Value;
		}

		public static explicit operator OracleMonthSpan (string s)
		{
			return Parse (s);
		}

		#endregion // Operators and Type Conversions
	}
}
