//
// Mono.Data.TdsTypes.TdsString
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Copyright Tim Coleman, 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Mono.Data.TdsClient;
using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Mono.Data.TdsTypes {
	public struct TdsString : INullable, IComparable 
	{
		#region Fields

		string value;

		private bool notNull;

		public static readonly int BinarySort;
		public static readonly int IgnoreCase;
		public static readonly int IgnoreKanaType;
		public static readonly int IgnoreNonSpace;
		public static readonly int IgnoreWidth;
		public static readonly TdsString Null;

		#endregion // Fields

		#region Constructors

		// init with a string data
		public TdsString (string data) 
		{
			this.value = data;
			notNull = true;
		}

		// init with a string data and locale id values.
		[MonoTODO]
		public TdsString (string data, int lcid) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, 
		// and an array of bytes data
		[MonoTODO]
		public TdsString (int lcid, TdsCompareOptions compareOptions, byte[] data) 
		{
			throw new NotImplementedException ();
		}

		// init with string data, locale id, and compare options
		[MonoTODO]
		public TdsString (string data, int lcid, TdsCompareOptions compareOptions) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, array of bytes data,
		// and whether unicode is encoded or not
		[MonoTODO]
		public TdsString (int lcid, TdsCompareOptions compareOptions, byte[] data, bool fUnicode) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, array of bytes data,
		// starting index in the byte array, 
		// and number of bytes to copy
		[MonoTODO]
		public TdsString (int lcid, TdsCompareOptions compareOptions, byte[] data, int index, int count) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, array of bytes data,
		// starting index in the byte array, number of byte to copy,
		// and whether unicode is encoded or not
		[MonoTODO]
		public TdsString (int lcid, TdsCompareOptions compareOptions, byte[] data, int index, int count, bool fUnicode) 
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
	
		public TdsCompareOptions TdsCompareOptions {
			[MonoTODO]
			get { throw new NotImplementedException ();
			}
		}

                public string Value {
                        get {
                                if (this.IsNull)
                                        throw new TdsNullValueException ("The property contains Null.");
                                else
                                        return value;
                        }
                }

		#endregion // Public Properties

		#region Public Methods

		[MonoTODO]
		public TdsString Clone() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static CompareOptions CompareOptionsFromTdsCompareOptions (TdsCompareOptions compareOptions) 
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
			else if (!(value is TdsString))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.TdsTypes.TdsString"));
			else if (((TdsString)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((TdsString)value).Value);
		}

		public static TdsString Concat(TdsString x, TdsString y) 
		{
			return (x + y);
		}

		public override bool Equals(object value) 
		{
			if (!(value is TdsString))
				return false;
			else
				return (bool) (this == (TdsString)value);
		}

		public static TdsBoolean Equals(TdsString x, TdsString y) 
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

		public static TdsBoolean GreaterThan(TdsString x, TdsString y) 
		{
			return (x > y);
		}

		public static TdsBoolean GreaterThanOrEqual(TdsString x, TdsString y) 
		{
			return (x >= y);
		}

		public static TdsBoolean LessThan(TdsString x, TdsString y) 
		{
			return (x < y);
		}

		public static TdsBoolean LessThanOrEqual(TdsString x, TdsString y) 
		{
			return (x <= y);
		}

		public static TdsBoolean NotEquals(TdsString x,	TdsString y) 
		{
			return (x != y);
		}

		// ****************************************
		// Type Conversions From TdsString To ...
		// ****************************************

		public TdsBoolean ToTdsBoolean() 
		{
			return ((TdsBoolean)this);
		}

		public TdsByte ToTdsByte() 
		{
			return ((TdsByte)this);
		}

		public TdsDateTime ToTdsDateTime() 
		{
			return ((TdsDateTime)this);
		}

		public TdsDecimal ToTdsDecimal() 
		{
			return ((TdsDecimal)this);
		}

		public TdsDouble ToTdsDouble() 
		{
			return ((TdsDouble)this);
		}

		public TdsGuid ToTdsGuid() 
		{
			return ((TdsGuid)this);
		}

		public TdsInt16 ToTdsInt16() 
		{
			return ((TdsInt16)this);
		}

		public TdsInt32 ToTdsInt32() 
		{
			return ((TdsInt32)this);
		}

		public TdsInt64 ToTdsInt64() 
		{
			return ((TdsInt64)this);
		}

		public TdsMoney ToTdsMoney() 
		{
			return ((TdsMoney)this);
		}

		public TdsSingle ToTdsSingle() 
		{
			return ((TdsSingle)this);
		}

		public override string ToString() 
		{
			return ((string)this);
		}

		// ***********************************
		// Operators
		// ***********************************

		// Concatenates
		public static TdsString operator + (TdsString x, TdsString y) 
		{
			return new TdsString (x.Value + y.Value);
		}

		// Equality
		public static TdsBoolean operator == (TdsString x, TdsString y) 
		{
			if (x.IsNull || y.IsNull)
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value == y.Value);
		}

		// Greater Than
		public static TdsBoolean operator > (TdsString x, TdsString y) 
		{
			if (x.IsNull || y.IsNull)
				return TdsBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		// Greater Than Or Equal
		public static TdsBoolean operator >= (TdsString x, TdsString y) 
		{
			if (x.IsNull || y.IsNull)
				return TdsBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		public static TdsBoolean operator != (TdsString x, TdsString y) 
		{ 
			if (x.IsNull || y.IsNull)
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value != y.Value);
		}

		// Less Than
		public static TdsBoolean operator < (TdsString x, TdsString y) 
		{
			if (x.IsNull || y.IsNull)
				return TdsBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		// Less Than Or Equal
		public static TdsBoolean operator <= (TdsString x, TdsString y) 
		{
			if (x.IsNull || y.IsNull)
				return TdsBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		// **************************************
		// Type Conversions
		// **************************************

		public static explicit operator TdsString (TdsBoolean x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsString (x.ByteValue.ToString ());
		}

		public static explicit operator TdsString (TdsByte x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsString (x.Value.ToString ());
		}

		public static explicit operator TdsString (TdsDateTime x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsString (x.Value.ToString ());
		}

		public static explicit operator TdsString (TdsDecimal x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsString (x.Value.ToString ());
		}

		public static explicit operator TdsString (TdsDouble x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsString (x.Value.ToString ());
		}

		public static explicit operator TdsString (TdsGuid x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsString (x.Value.ToString ());
		}

		public static explicit operator TdsString (TdsInt16 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsString (x.Value.ToString ());
		}

		public static explicit operator TdsString (TdsInt32 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsString (x.Value.ToString ());
		}

		public static explicit operator TdsString (TdsInt64 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsString (x.Value.ToString ());
		}

		public static explicit operator TdsString (TdsMoney x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsString (x.Value.ToString ());
		}

		public static explicit operator TdsString (TdsSingle x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsString (x.Value.ToString ());
		}

		public static explicit operator string (TdsString x) 
		{
			return x.Value;
		}

		public static implicit operator TdsString (string x) 
		{
			return new TdsString (x);
		}

		#endregion // Public Methods
	}
}
