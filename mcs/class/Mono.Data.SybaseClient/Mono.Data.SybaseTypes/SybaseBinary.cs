//
// Mono.Data.SybaseTypes.SybaseBinary
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Copyright Tim Coleman, 2002
//

using Mono.Data.SybaseClient;
using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Mono.Data.SybaseTypes {
	public struct SybaseBinary : INullable, IComparable
	{
		#region Fields

		byte[] value;
		private bool notNull;

		public static readonly SybaseBinary Null;

		#endregion

		#region Constructors
		
		public SybaseBinary (byte[] value) 
		{
			this.value = value;
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public byte this[int index] {
			get { 
				if (this.IsNull)
					throw new SybaseNullValueException ("The property contains Null.");
				else if (index >= this.Length)
					throw new SybaseNullValueException ("The index parameter indicates a position beyond the length of the byte array.");
				else
					return value [index]; 
			}
		}

		public int Length {
			get { 
				if (this.IsNull)
					throw new SybaseNullValueException ("The property contains Null.");
				else
					return value.Length;
			}
		}

		public byte[] Value 
		{
			get { 
				if (this.IsNull) 
					throw new SybaseNullValueException ("The property contains Null.");
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		[MonoTODO]
		public int CompareTo (object value) 
		{
			throw new NotImplementedException ();
		}

		public static SybaseBinary Concat (SybaseBinary x, SybaseBinary y) 
		{
			return (x + y);
		}

		public override bool Equals (object value) 
		{
			if (!(value is SybaseBinary))
				return false;
			else
				return (bool) (this == (SybaseBinary)value);
		}

		public static SybaseBoolean Equals (SybaseBinary x, SybaseBinary y) 
		{
			return (x == y);
		}

		[MonoTODO]
		public override int GetHashCode () 
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Operators

		public static SybaseBoolean GreaterThan (SybaseBinary x, SybaseBinary y) 
		{
			return (x > y);
		}

		public static SybaseBoolean GreaterThanOrEqual (SybaseBinary x, SybaseBinary y) 
		{
			return (x >= y);
		}

		public static SybaseBoolean LessThan (SybaseBinary x, SybaseBinary y) 
		{
			return (x < y);
		}

		public static SybaseBoolean LessThanOrEqual (SybaseBinary x, SybaseBinary y) 
		{
			return (x <= y);
		}

		public static SybaseBoolean NotEquals (SybaseBinary x, SybaseBinary y) 
		{
			return (x != y);
		}

		public SybaseGuid ToSybaseGuid () 
		{
			return new SybaseGuid (value);
		}

		public override string ToString () 
		{
			if (IsNull)
				return "null";
			return String.Format ("SybaseBinary ({0})", Length);
		}

		#endregion

		#region Operators

		[MonoTODO]
		public static SybaseBinary operator + (SybaseBinary x, SybaseBinary y) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SybaseBoolean operator == (SybaseBinary x, SybaseBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SybaseBoolean operator > (SybaseBinary x, SybaseBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SybaseBoolean operator >= (SybaseBinary x, SybaseBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SybaseBoolean operator != (SybaseBinary x, SybaseBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SybaseBoolean operator < (SybaseBinary x, SybaseBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SybaseBoolean operator <= (SybaseBinary x, SybaseBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		public static explicit operator byte[] (SybaseBinary x) 
		{
			return x.Value;
		}

		[MonoTODO]
		public static explicit operator SybaseBinary (SybaseGuid x) 
		{
			throw new NotImplementedException ();
		}

		public static implicit operator SybaseBinary (byte[] x) 
		{
			return new SybaseBinary (x);
		}

		#endregion
	}
}
