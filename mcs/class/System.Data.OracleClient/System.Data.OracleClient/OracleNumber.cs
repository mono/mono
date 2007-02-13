//
// OracleNumber.cs
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
	public struct OracleNumber : IComparable, INullable
	{
		#region Fields

		public static readonly OracleNumber E = new OracleNumber (Math.E);
		public static readonly int MaxPrecision = 38;
		public static readonly int MaxScale = 127;
		public static readonly OracleNumber MaxValue; // FIXME
		public static readonly int MinScale = -84;
		public static readonly OracleNumber MinusOne = new OracleNumber (-1);
		public static readonly OracleNumber MinValue; // FIXME
		public static readonly OracleNumber Null = new OracleNumber ();
		public static readonly OracleNumber One = new OracleNumber (1);
		public static readonly OracleNumber PI = new OracleNumber (Math.PI);
		public static readonly OracleNumber Zero = new OracleNumber (0);

		decimal value;
		bool notNull;

		#endregion // Fields

		#region Constructors

		public OracleNumber (decimal decValue)
		{
			this.value = decValue;
			notNull = true;
		}

		public OracleNumber (double dblValue)
			: this ((decimal) dblValue)
		{
		}

		public OracleNumber (int intValue)
			: this ((decimal) intValue)
		{
		}

		public OracleNumber (long longValue)
			: this ((decimal) longValue)
		{
		}

		public OracleNumber (OracleNumber from)
			: this (from.Value)
		{
		}

		#endregion // Constructors

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public decimal Value {
			get {
				if (IsNull)
					throw new InvalidOperationException ("The value is Null.");
				return value;
			}
		}

		#endregion // Properties

		#region Methods

		public static OracleNumber Abs (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Abs (n.Value));
		}

		public static OracleNumber Acos (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Acos ((double) n));
		}

		public static OracleNumber Add (OracleNumber x, OracleNumber y)
		{
			if (x.IsNull || y.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (x.Value + y.Value);
		}

		public static OracleNumber Asin (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Asin ((double) n));
		}

		public static OracleNumber Atan (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Atan ((double) n));
		}

		public static OracleNumber Atan2 (OracleNumber y, OracleNumber x)
		{
			if (x.IsNull || y.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Atan2 ((double) y, (double) x));
		}

		public static OracleNumber Ceiling (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Ceiling ((double) n));
		}

		[MonoTODO]
		public int CompareTo (object obj)
		{
			throw new NotImplementedException ();
		}

		public static OracleNumber Cos (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Cos ((double) n));
		}

		public static OracleNumber Cosh (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Cosh ((double) n));
		}

		public static OracleNumber Divide (OracleNumber x, OracleNumber y)
		{
			if (x.IsNull || y.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (x.Value / y.Value);
		}

		[MonoTODO]
		public override bool Equals (object value)
		{
			throw new NotImplementedException ();
		}

		public static OracleBoolean Equals (OracleNumber x, OracleNumber y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value == y.Value);
		}

		public static OracleNumber Exp (OracleNumber p)
		{
			if (p.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Exp ((double) p));
		}

		public static OracleNumber Floor (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Decimal.Floor (n.Value));
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		public static OracleBoolean GreaterThan (OracleNumber x, OracleNumber y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value > y.Value);
		}

		public static OracleBoolean GreaterThanOrEqual (OracleNumber x, OracleNumber y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value >= y.Value);
		}

		public static OracleBoolean LessThan (OracleNumber x, OracleNumber y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value < y.Value);
		}

		public static OracleBoolean LessThanOrEqual (OracleNumber x, OracleNumber y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value <= y.Value);
		}

		public static OracleNumber Log (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Log ((double) n));
		}

		public static OracleNumber Log (OracleNumber n, int newBase)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Log ((double) n, (double) newBase));
		}

		public static OracleNumber Log (OracleNumber n, OracleNumber newBase)
		{
			if (n.IsNull || newBase.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Log ((double) n, (double) newBase));
		}

		public static OracleNumber Log10 (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Log10 ((double) n));
		}

		public static OracleNumber Max (OracleNumber x, OracleNumber y)
		{
			if (x.IsNull || y.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Max (x.Value, y.Value));
		}

		public static OracleNumber Min (OracleNumber x, OracleNumber y)
		{
			if (x.IsNull || y.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Min (x.Value, y.Value));
		}

		public static OracleNumber Modulo (OracleNumber x, OracleNumber y)
		{
			if (x.IsNull || y.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (x.Value % y.Value);
		}

		public static OracleNumber Multiply (OracleNumber x, OracleNumber y)
		{
			if (x.IsNull || y.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (x.Value * y.Value);
		}

		public static OracleNumber Negate (OracleNumber x)
		{
			if (x.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (-x.Value);
		}

		public static OracleBoolean NotEquals (OracleNumber x, OracleNumber y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value != y.Value);
		}

		public static OracleNumber Parse (string s)
		{
			return new OracleNumber (Decimal.Parse (s));
		}

		public static OracleNumber Pow (OracleNumber x, int y)
		{
			if (x.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Pow ((double) x, (double) y));
		}

		public static OracleNumber Pow (OracleNumber x, OracleNumber y)
		{
			if (x.IsNull || y.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Pow ((double) x, (double) y));
		}

		public static OracleNumber Round (OracleNumber n, int position)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Round (n.Value, position));
		}

		public static OracleNumber Shift (OracleNumber n, int digits)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (n * (OracleNumber) (Math.Pow (10, digits)));
		}

		public static OracleNumber Sign (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Sign (n.Value));
		}

		public static OracleNumber Sin (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Sin ((double) n));
		}

		public static OracleNumber Sinh (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Sinh ((double) n));
		}

		public static OracleNumber Sqrt (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Sqrt ((double) n));
		}

		public static OracleNumber Subtract (OracleNumber x, OracleNumber y)
		{
			if (x.IsNull || y.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (x.Value - y.Value);
		}

		public static OracleNumber Tan (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Tan ((double) n));
		}

		public static OracleNumber Tanh (OracleNumber n)
		{
			if (n.IsNull)
				return OracleNumber.Null;
			return new OracleNumber (Math.Tanh ((double) n));
		}

		public override string ToString ()
		{
			return ToString(null);
		}

		[MonoTODO]
		public static OracleNumber Truncate (OracleNumber n, int position)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods

		#region Operators and Type Conversions

		public static OracleNumber operator + (OracleNumber x, OracleNumber y)
		{
			return Add (x, y);
		}

		public static OracleNumber operator / (OracleNumber x, OracleNumber y)
		{
			return Divide (x, y);
		}

		public static OracleBoolean operator == (OracleNumber x, OracleNumber y)
		{
			return Equals (x, y);
		}

		public static OracleBoolean operator > (OracleNumber x, OracleNumber y)
		{
			return GreaterThan (x, y);
		}

		public static OracleBoolean operator >= (OracleNumber x, OracleNumber y)
		{
			return GreaterThanOrEqual (x, y);
		}

		public static OracleBoolean operator != (OracleNumber x, OracleNumber y)
		{
			return NotEquals (x, y);
		}

		public static OracleBoolean operator < (OracleNumber x, OracleNumber y)
		{
			return LessThan (x, y);
		}

		public static OracleBoolean operator <= (OracleNumber x, OracleNumber y)
		{
			return LessThanOrEqual (x, y);
		}

		public static OracleNumber operator % (OracleNumber x, OracleNumber y)
		{
			return Modulo (x, y);
		}

		public static OracleNumber operator * (OracleNumber x, OracleNumber y)
		{
			return Multiply (x, y);
		}

		public static OracleNumber operator - (OracleNumber x, OracleNumber y)
		{
			return Subtract (x, y);
		}

		public static OracleNumber operator - (OracleNumber x)
		{
			return Negate (x);
		}

		public static explicit operator OracleNumber (decimal x)
		{
			return new OracleNumber (x);
		}

		public static explicit operator OracleNumber (double x)
		{
			return new OracleNumber (x);
		}

		public static explicit operator OracleNumber (int x)
		{
			return new OracleNumber (x);
		}

		public static explicit operator OracleNumber (long x)
		{
			return new OracleNumber (x);
		}

		public static explicit operator double (OracleNumber x)
		{
			if (x.IsNull)
				throw new NullReferenceException ();
			return (double) x.Value;
		}

		public static explicit operator decimal (OracleNumber x)
		{
			if (x.IsNull)
				throw new NullReferenceException ();
			return x.Value;
		}

		public static explicit operator int (OracleNumber x)
		{
			if (x.IsNull)
				throw new NullReferenceException ();
			return (int) x.Value;
		}

		public static explicit operator long (OracleNumber x)
		{
			if (x.IsNull)
				throw new NullReferenceException ();
			return (long) x.Value;
		}

		public static explicit operator OracleNumber (string x)
		{
			return OracleNumber.Parse (x);
		}

		#endregion // Operators and Type Conversions

		#region internal IFormatProvider handling
		internal String ToString(IFormatProvider format)
		{
			if (IsNull)
				return "Null";
			return Value.ToString (format);
		}
		#endregion
	}
}
