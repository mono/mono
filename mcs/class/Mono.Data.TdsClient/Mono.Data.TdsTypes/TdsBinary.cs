//
// Mono.Data.TdsTypes.TdsBinary
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Copyright Tim Coleman, 2002
//

using Mono.Data.TdsClient;
using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Mono.Data.TdsTypes {
	public struct TdsBinary : INullable, IComparable
	{
		#region Fields

		byte[] value;
		private bool notNull;

		public static readonly TdsBinary Null;

		#endregion

		#region Constructors
		
		public TdsBinary (byte[] value) 
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
					throw new TdsNullValueException ("The property contains Null.");
				else if (index >= this.Length)
					throw new TdsNullValueException ("The index parameter indicates a position beyond the length of the byte array.");
				else
					return value [index]; 
			}
		}

		public int Length {
			get { 
				if (this.IsNull)
					throw new TdsNullValueException ("The property contains Null.");
				else
					return value.Length;
			}
		}

		public byte[] Value 
		{
			get { 
				if (this.IsNull) 
					throw new TdsNullValueException ("The property contains Null.");
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

		public static TdsBinary Concat (TdsBinary x, TdsBinary y) 
		{
			return (x + y);
		}

		public override bool Equals (object value) 
		{
			if (!(value is TdsBinary))
				return false;
			else
				return (bool) (this == (TdsBinary)value);
		}

		public static TdsBoolean Equals (TdsBinary x, TdsBinary y) 
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

		public static TdsBoolean GreaterThan (TdsBinary x, TdsBinary y) 
		{
			return (x > y);
		}

		public static TdsBoolean GreaterThanOrEqual (TdsBinary x, TdsBinary y) 
		{
			return (x >= y);
		}

		public static TdsBoolean LessThan (TdsBinary x, TdsBinary y) 
		{
			return (x < y);
		}

		public static TdsBoolean LessThanOrEqual (TdsBinary x, TdsBinary y) 
		{
			return (x <= y);
		}

		public static TdsBoolean NotEquals (TdsBinary x, TdsBinary y) 
		{
			return (x != y);
		}

		public TdsGuid ToTdsGuid () 
		{
			return new TdsGuid (value);
		}

		public override string ToString () 
		{
			if (IsNull)
				return "null";
			return String.Format ("TdsBinary ({0})", Length);
		}

		#endregion

		#region Operators

		[MonoTODO]
		public static TdsBinary operator + (TdsBinary x, TdsBinary y) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static TdsBoolean operator == (TdsBinary x, TdsBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		[MonoTODO]
		public static TdsBoolean operator > (TdsBinary x, TdsBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		[MonoTODO]
		public static TdsBoolean operator >= (TdsBinary x, TdsBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		[MonoTODO]
		public static TdsBoolean operator != (TdsBinary x, TdsBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		[MonoTODO]
		public static TdsBoolean operator < (TdsBinary x, TdsBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		[MonoTODO]
		public static TdsBoolean operator <= (TdsBinary x, TdsBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		public static explicit operator byte[] (TdsBinary x) 
		{
			return x.Value;
		}

		[MonoTODO]
		public static explicit operator TdsBinary (TdsGuid x) 
		{
			throw new NotImplementedException ();
		}

		public static implicit operator TdsBinary (byte[] x) 
		{
			return new TdsBinary (x);
		}

		#endregion
	}
}
