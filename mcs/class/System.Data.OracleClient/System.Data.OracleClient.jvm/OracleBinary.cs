//
// OracleBinary.cs 
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
using System.IO;
using System.Data.SqlTypes;

namespace System.Data.OracleClient {
	public struct OracleBinary : IComparable, INullable {
		#region Fields

		public static readonly OracleBinary Null = new OracleBinary ();

		bool notNull; 
		byte[] value; 

		#endregion // Fields

		#region Constructors

		public OracleBinary (byte[] b) {
			value = b;
			notNull = true;
		}

		#endregion // Constructors

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public byte this [int index] {
			get { 
				if (IsNull)
					throw new Exception ("Data is null.");
				return value [index];
			}
		}

		public int Length {
			get {
				if (IsNull)
					throw new Exception ("Data is null.");
				return value.Length;
			}
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
		public int CompareTo (object obj) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static OracleBinary Concat (OracleBinary x, OracleBinary y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Equals (object value) {
			throw new NotImplementedException ();
		}

		public static OracleBoolean Equals (OracleBinary x, OracleBinary y) {
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			return new OracleBoolean (x.Value == y.Value);
		}

		[MonoTODO]
		public override int GetHashCode () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static OracleBoolean GreaterThan (OracleBinary x, OracleBinary y) {
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			//return (x.Value > y.Value);
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static OracleBoolean GreaterThanOrEqual (OracleBinary x, OracleBinary y) {
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			//return (x.Value >= y.Value);
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static OracleBoolean LessThan (OracleBinary x, OracleBinary y) {
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			//return (x.Value < y.Value);
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static OracleBoolean LessThanOrEqual (OracleBinary x, OracleBinary y) {
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			//return (x.Value <= y.Value);
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static OracleBoolean NotEquals (OracleBinary x, OracleBinary y) {
			if (x.IsNull || y.IsNull)
				return OracleBoolean.Null;
			//return (x.Value != y.Value);
			throw new NotImplementedException ();
		}

		#endregion // Methods

		#region Operators and Type Conversions

		public static OracleBinary operator + (OracleBinary x, OracleBinary y) {
			return Concat (x, y);
		}

		public static OracleBoolean operator == (OracleBinary x, OracleBinary y) {
			return Equals (x, y);
		}

		public static OracleBoolean operator > (OracleBinary x, OracleBinary y) {
			return GreaterThan (x, y);
		}

		public static OracleBoolean operator >= (OracleBinary x, OracleBinary y) {
			return GreaterThanOrEqual (x, y);
		}

		public static OracleBoolean operator != (OracleBinary x, OracleBinary y) {
			return NotEquals (x, y);
		}

		public static OracleBoolean operator < (OracleBinary x, OracleBinary y) {
			return LessThan (x, y);
		}

		public static OracleBoolean operator <= (OracleBinary x, OracleBinary y) {
			return LessThan (x, y);
		}

		public static explicit operator byte[] (OracleBinary x) {
			return x.Value;
		}

		public static implicit operator OracleBinary (byte[] b) {
			return new OracleBinary (b);
		}

		#endregion // Operators and Type Conversions
	}
}
