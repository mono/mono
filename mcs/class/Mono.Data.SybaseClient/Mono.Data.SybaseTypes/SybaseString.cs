//
// Mono.Data.SybaseTypes.SybaseString
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
	public struct SybaseString : INullable, IComparable 
	{
		#region Fields

		string value;

		private bool notNull;

		public static readonly int BinarySort;
		public static readonly int IgnoreCase;
		public static readonly int IgnoreKanaType;
		public static readonly int IgnoreNonSpace;
		public static readonly int IgnoreWidth;
		public static readonly SybaseString Null;

		#endregion // Fields

		#region Constructors

		// init with a string data
		public SybaseString (string data) 
		{
			this.value = data;
			notNull = true;
		}

		// init with a string data and locale id values.
		[MonoTODO]
		public SybaseString (string data, int lcid) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, 
		// and an array of bytes data
		[MonoTODO]
		public SybaseString (int lcid, SybaseCompareOptions compareOptions, byte[] data) 
		{
			throw new NotImplementedException ();
		}

		// init with string data, locale id, and compare options
		[MonoTODO]
		public SybaseString (string data, int lcid, SybaseCompareOptions compareOptions) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, array of bytes data,
		// and whether unicode is encoded or not
		[MonoTODO]
		public SybaseString (int lcid, SybaseCompareOptions compareOptions, byte[] data, bool fUnicode) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, array of bytes data,
		// starting index in the byte array, 
		// and number of bytes to copy
		[MonoTODO]
		public SybaseString (int lcid, SybaseCompareOptions compareOptions, byte[] data, int index, int count) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, array of bytes data,
		// starting index in the byte array, number of byte to copy,
		// and whether unicode is encoded or not
		[MonoTODO]
		public SybaseString (int lcid, SybaseCompareOptions compareOptions, byte[] data, int index, int count, bool fUnicode) 
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors


		#region Public Properties

		public CompareInfo CompareInfo {
			[MonoTODO]
			get { throw new NotImplementedException ();
			}
		}

		public CultureInfo CultureInfo {
			[MonoTODO]
			get { throw new NotImplementedException ();
			}
		}

		public bool IsNull {
			get { return !notNull; }
		}

		// geographics location and language (locale id)
		public int LCID {
			[MonoTODO]
			get { throw new NotImplementedException ();
			}
		}
	
		public SybaseCompareOptions SybaseCompareOptions {
			[MonoTODO]
			get { throw new NotImplementedException ();
			}
		}

                public string Value {
                        get {
                                if (this.IsNull)
                                        throw new SybaseNullValueException ("The property contains Null.");
                                else
                                        return value;
                        }
                }

		#endregion // Public Properties

		#region Public Methods

		[MonoTODO]
		public SybaseString Clone() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static CompareOptions CompareOptionsFromSybaseCompareOptions (SybaseCompareOptions compareOptions) 
		{
			throw new NotImplementedException ();
		}

		// **********************************
		// Comparison Methods
		// **********************************

		public int CompareTo(object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SybaseString))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SybaseTypes.SybaseString"));
			else if (((SybaseString)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SybaseString)value).Value);
		}

		public static SybaseString Concat(SybaseString x, SybaseString y) 
		{
			return (x + y);
		}

		public override bool Equals(object value) 
		{
			if (!(value is SybaseString))
				return false;
			else
				return (bool) (this == (SybaseString)value);
		}

		public static SybaseBoolean Equals(SybaseString x, SybaseString y) 
		{
			return (x == y);
		}

		[MonoTODO]
		public override int GetHashCode() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public byte[] GetNonUnicodeBytes() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public byte[] GetUnicodeBytes() 
		{
			throw new NotImplementedException ();
		}

		public static SybaseBoolean GreaterThan(SybaseString x, SybaseString y) 
		{
			return (x > y);
		}

		public static SybaseBoolean GreaterThanOrEqual(SybaseString x, SybaseString y) 
		{
			return (x >= y);
		}

		public static SybaseBoolean LessThan(SybaseString x, SybaseString y) 
		{
			return (x < y);
		}

		public static SybaseBoolean LessThanOrEqual(SybaseString x, SybaseString y) 
		{
			return (x <= y);
		}

		public static SybaseBoolean NotEquals(SybaseString x,	SybaseString y) 
		{
			return (x != y);
		}

		// ****************************************
		// Type Conversions From SybaseString To ...
		// ****************************************

		public SybaseBoolean ToSybaseBoolean() 
		{
			return ((SybaseBoolean)this);
		}

		public SybaseByte ToSybaseByte() 
		{
			return ((SybaseByte)this);
		}

		public SybaseDateTime ToSybaseDateTime() 
		{
			return ((SybaseDateTime)this);
		}

		public SybaseDecimal ToSybaseDecimal() 
		{
			return ((SybaseDecimal)this);
		}

		public SybaseDouble ToSybaseDouble() 
		{
			return ((SybaseDouble)this);
		}

		public SybaseGuid ToSybaseGuid() 
		{
			return ((SybaseGuid)this);
		}

		public SybaseInt16 ToSybaseInt16() 
		{
			return ((SybaseInt16)this);
		}

		public SybaseInt32 ToSybaseInt32() 
		{
			return ((SybaseInt32)this);
		}

		public SybaseInt64 ToSybaseInt64() 
		{
			return ((SybaseInt64)this);
		}

		public SybaseMoney ToSybaseMoney() 
		{
			return ((SybaseMoney)this);
		}

		public SybaseSingle ToSybaseSingle() 
		{
			return ((SybaseSingle)this);
		}

		public override string ToString() 
		{
			return ((string)this);
		}

		// ***********************************
		// Operators
		// ***********************************

		// Concatenates
		public static SybaseString operator + (SybaseString x, SybaseString y) 
		{
			return new SybaseString (x.Value + y.Value);
		}

		// Equality
		public static SybaseBoolean operator == (SybaseString x, SybaseString y) 
		{
			if (x.IsNull || y.IsNull)
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value == y.Value);
		}

		// Greater Than
		public static SybaseBoolean operator > (SybaseString x, SybaseString y) 
		{
			if (x.IsNull || y.IsNull)
				return SybaseBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		// Greater Than Or Equal
		public static SybaseBoolean operator >= (SybaseString x, SybaseString y) 
		{
			if (x.IsNull || y.IsNull)
				return SybaseBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		public static SybaseBoolean operator != (SybaseString x, SybaseString y) 
		{ 
			if (x.IsNull || y.IsNull)
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value != y.Value);
		}

		// Less Than
		public static SybaseBoolean operator < (SybaseString x, SybaseString y) 
		{
			if (x.IsNull || y.IsNull)
				return SybaseBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		// Less Than Or Equal
		public static SybaseBoolean operator <= (SybaseString x, SybaseString y) 
		{
			if (x.IsNull || y.IsNull)
				return SybaseBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		// **************************************
		// Type Conversions
		// **************************************

		public static explicit operator SybaseString (SybaseBoolean x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseString (x.ByteValue.ToString ());
		}

		public static explicit operator SybaseString (SybaseByte x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseString (x.Value.ToString ());
		}

		public static explicit operator SybaseString (SybaseDateTime x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseString (x.Value.ToString ());
		}

		public static explicit operator SybaseString (SybaseDecimal x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseString (x.Value.ToString ());
		}

		public static explicit operator SybaseString (SybaseDouble x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseString (x.Value.ToString ());
		}

		public static explicit operator SybaseString (SybaseGuid x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseString (x.Value.ToString ());
		}

		public static explicit operator SybaseString (SybaseInt16 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseString (x.Value.ToString ());
		}

		public static explicit operator SybaseString (SybaseInt32 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseString (x.Value.ToString ());
		}

		public static explicit operator SybaseString (SybaseInt64 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseString (x.Value.ToString ());
		}

		public static explicit operator SybaseString (SybaseMoney x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseString (x.Value.ToString ());
		}

		public static explicit operator SybaseString (SybaseSingle x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseString (x.Value.ToString ());
		}

		public static explicit operator string (SybaseString x) 
		{
			return x.Value;
		}

		public static implicit operator SybaseString (string x) 
		{
			return new SybaseString (x);
		}

		#endregion // Public Methods
	}
}
