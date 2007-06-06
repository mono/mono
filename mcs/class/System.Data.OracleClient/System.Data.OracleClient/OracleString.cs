//
// OracleString.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Authors: Tim Coleman <tim@timcoleman.com>
//          Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) Tim Coleman, 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace System.Data.OracleClient
{
	public struct OracleString : IComparable, INullable
	{
		#region Fields

		string value;
		bool notNull;

		public static readonly OracleString Empty = new OracleString (String.Empty);
		public static readonly OracleString Null = new OracleString ();

		#endregion // Fields

		#region Constructors

		public OracleString (string s)
		{
			value = s;
			notNull = true;
		}

		#endregion // Constructors

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public int Length {
			get { return value.Length; }
		}

		public char this [int index] {
			get { return value [index]; }
		}

		public string Value {
			get { return value; }
		}

		#endregion // Properties

		#region Methods

		public int CompareTo (object obj)
		{
			if (obj == null)
				return 1;
			else if (!(obj is OracleString))
				throw new ArgumentException ("Value is not a System.Data.OracleClient.OracleString");
			else if (((OracleString) obj).IsNull)
				return 1;
			else
				return value.CompareTo (((OracleString) obj).Value);
		}

		public static OracleBoolean GreaterThan (OracleString x, OracleString y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x > y);
		}

		public static OracleBoolean GreaterThanOrEqual (OracleString x, OracleString y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return (x  >= y);
		}

		public static OracleBoolean LessThan (OracleString x, OracleString y)
		{
			return (x < y);
		}

		public static OracleBoolean LessThanOrEqual (OracleString x, OracleString y)
		{
			return (x <= y);
		}

		public static OracleString Concat (OracleString x, OracleString y)
		{
			return x + y;
		}

		public override int GetHashCode ()
		{
			// It returns value string's HashCode.
			return notNull ? value.GetHashCode () : 0;
		}

		public override bool Equals (object value)
		{
			if (value is OracleString) {
				OracleString s = (OracleString) value;
				if (notNull && s.notNull)
					return this.value == s.value;
				else
					throw new InvalidOperationException ("the value is Null.");
			}
			return false;
		}

		public static OracleBoolean Equals (OracleString x, OracleString y)
		{
			return (x == y);
		}

		public static OracleBoolean NotEquals (OracleString x, OracleString y)
		{
			return (x != y);
		}

		public override string ToString ()
		{
			return notNull ? value : "Null";
		}

		#endregion // Methods

		#region Operators

		public static OracleString operator + (OracleString x, OracleString y)
		{
			return (x.notNull && y.notNull) ?
				new OracleString (x.value + y.value) :
				Null;
		}

		public static OracleBoolean operator == (OracleString x, OracleString y)
		{
			return (!x.notNull || !y.notNull) ?
				OracleBoolean.Null : new OracleBoolean (x.value == y.value);
		}

		public static explicit operator string (OracleString x)
		{
			return x.Value;
		}

		[MonoTODO]
		public static OracleBoolean operator > (OracleString x, OracleString y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static OracleBoolean operator >= (OracleString x, OracleString y)
		{
			throw new NotImplementedException ();
		}

		public static implicit operator OracleString (string s)
		{
			return new OracleString (s);
		}

		public static OracleBoolean operator != (OracleString x, OracleString y)
		{
			return (!x.notNull || !y.notNull) ?
				OracleBoolean.Null : x.value != y.value;
		}

		public static OracleBoolean operator < (OracleString x, OracleString y)
		{
			return (!x.notNull || !y.notNull) ?
				OracleBoolean.Null :
				new OracleBoolean (String.Compare (x.value, y.value, false, CultureInfo.InvariantCulture) < 0);
		}

		public static OracleBoolean operator <= (OracleString x, OracleString y)
		{
			return (!x.notNull || !y.notNull) ?
				OracleBoolean.Null : new OracleBoolean (String.Compare (x.value, y.value, false, CultureInfo.InvariantCulture) <= 0);
		}

		#endregion // Operators
	}
}
