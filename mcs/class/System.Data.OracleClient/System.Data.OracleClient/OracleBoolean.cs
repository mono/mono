//
// OracleBoolean.cs 
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
	public struct OracleBoolean : IComparable
	{
		#region Fields

		public static readonly OracleBoolean False = new OracleBoolean (false);
		public static readonly OracleBoolean Null = new OracleBoolean ();
		public static readonly OracleBoolean One = new OracleBoolean (1);
		public static readonly OracleBoolean True = new OracleBoolean (true);
		public static readonly OracleBoolean Zero = new OracleBoolean (0);

		bool value;
		bool notNull;

		#endregion // Fields

		#region Constructors

		public OracleBoolean (bool value)
		{
			this.value = value;
			notNull = true;
		}

		public OracleBoolean (int value)
			: this (value != 0)
		{
		}

		#endregion // Constructors

		#region Properties

		public bool IsFalse {
			get { return (!IsNull && !value); }
		}

		public bool IsNull {
			get { return !notNull; }
		}

		public bool IsTrue {
			get { return (!IsNull && value); }
		}

		public bool Value {
			get { return IsTrue; }
		}

		#endregion // Properties

		#region Methods

		public static OracleBoolean And (OracleBoolean x, OracleBoolean y)
		{
			if (x.IsNull || y.IsNull) 
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value && y.Value);
		}

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

		public static OracleBoolean Equals (OracleBoolean x, OracleBoolean y)
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

		public static OracleBoolean NotEquals (OracleBoolean x, OracleBoolean y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value != y.Value);
		}

		public static OracleBoolean OnesComplement (OracleBoolean x)
		{
			if (x.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (!x.Value);
		}

		public static OracleBoolean Or (OracleBoolean x, OracleBoolean y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value || y.Value);
		}

		[MonoTODO]
		public static OracleBoolean Parse (string s)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			if (IsNull)
				return "Null";

			if (IsTrue)
				return "True";

			return "False";
		}

		public static OracleBoolean Xor (OracleBoolean x, OracleBoolean y)
		{
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value ^ y.Value);
		}

		#endregion // Methods

		#region Operators and Type Conversions

		public static OracleBoolean operator & (OracleBoolean x, OracleBoolean y)
		{
			return And (x, y);
		}

		public static OracleBoolean operator | (OracleBoolean x, OracleBoolean y)
		{
			return Or (x, y);
		}

		public static OracleBoolean operator == (OracleBoolean x, OracleBoolean y)
		{
			return Equals (x, y);
		}

		public static OracleBoolean operator ^ (OracleBoolean x, OracleBoolean y)
		{
			return Xor (x, y);
		}

		public static bool operator false (OracleBoolean x)
		{
			return x.IsFalse; 
		}

		public static OracleBoolean operator != (OracleBoolean x, OracleBoolean y)
		{
			return NotEquals (x, y);
		}

		public static OracleBoolean operator ! (OracleBoolean x)
		{
			return OnesComplement (x);
		}

		public static OracleBoolean operator ~ (OracleBoolean x)
		{
			return OnesComplement (x);
		}

		public static bool operator true (OracleBoolean x)
		{
			return x.IsTrue;
		}

		public static explicit operator bool (OracleBoolean x)
		{
			if (x.IsNull)
				throw new NullReferenceException ();
			return x.Value;
		}
			
		public static explicit operator OracleBoolean (OracleNumber x)
		{
			return new OracleBoolean ((int) x);
		}

		public static explicit operator OracleBoolean (string x)
		{
			return OracleBoolean.Parse (x);
		}

		public static implicit operator OracleBoolean (bool x)
		{
			return new OracleBoolean (x);
		}

		#endregion // Operators and Type Conversions
	}
}
