//
// Mono.Data.SybaseTypes.SybaseString
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Based on System.Data.SqlTypes.SqlString
//
// (C) Ximian, Inc. 2002-2003
// (C) Copyright Tim Coleman, 2002-2003
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

using Mono.Data.SybaseClient;
using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;

namespace Mono.Data.SybaseTypes {
	public struct SybaseString : INullable, IComparable 
	{
		#region Fields

		string value;

		private bool notNull;

		private CultureInfo cultureInfo;
		private SybaseCompareOptions compareOptions;

		public static readonly int BinarySort = 0x8000;
		public static readonly int IgnoreCase = 0x1;
		public static readonly int IgnoreKanaType = 0x8;
		public static readonly int IgnoreNonSpace = 0x2;
		public static readonly int IgnoreWidth = 0x10;
		public static readonly SybaseString Null;

		#endregion // Fields

		#region Constructors

		// init with a string data
		public SybaseString (string data) 
		{
			this.value = data;
			this.cultureInfo = CultureInfo.CurrentCulture;
			this.compareOptions = SybaseCompareOptions.None;
			this.notNull = true;
		}

		// init with a string data and locale id values.
		public SybaseString (string data, int lcid) 
			: this (data, lcid, SybaseCompareOptions.None)
		{
		}

		// init with locale id, compare options, 
		// and an array of bytes data
		public SybaseString (int lcid, SybaseCompareOptions compareOptions, byte[] data) 
			: this (lcid, compareOptions, data, true)
		{
		}

		// init with string data, locale id, and compare options
		public SybaseString (string data, int lcid, SybaseCompareOptions compareOptions) 
		{
			this.value = data;
			this.cultureInfo = new CultureInfo (lcid);
			this.compareOptions = compareOptions;
			this.notNull = true;
		}

		// init with locale id, compare options, array of bytes data,
		// and whether unicode is encoded or not
		public SybaseString (int lcid, SybaseCompareOptions compareOptions, byte[] data, bool fUnicode) 
		{
			Encoding encoding;
			if (fUnicode)
				encoding = new UnicodeEncoding ();
			else
				encoding = new ASCIIEncoding ();

			this.value = encoding.GetString (data);
			this.cultureInfo = new CultureInfo (lcid);
			this.compareOptions = compareOptions;
			this.notNull = true;
		}

		// init with locale id, compare options, array of bytes data,
		// starting index in the byte array, 
		// and number of bytes to copy
		public SybaseString (int lcid, SybaseCompareOptions compareOptions, byte[] data, int index, int count) 
			: this (lcid, compareOptions, data, index, count, true)
		{
		}

		// init with locale id, compare options, array of bytes data,
		// starting index in the byte array, number of byte to copy,
		// and whether unicode is encoded or not
		public SybaseString (int lcid, SybaseCompareOptions compareOptions, byte[] data, int index, int count, bool fUnicode) 
		{
			Encoding encoding;
			if (fUnicode)
				encoding = new UnicodeEncoding ();
			else
				encoding = new ASCIIEncoding ();

			this.value = encoding.GetString (data, index, count);
			this.cultureInfo = new CultureInfo (lcid);
			this.compareOptions = compareOptions;
			this.notNull = true;
		}

		#endregion // Constructors


		#region Public Properties

		public CompareInfo CompareInfo {
			get { return cultureInfo.CompareInfo; }
		}

		public CultureInfo CultureInfo {
			get { return cultureInfo; }
		}

		public bool IsNull {
			get { return !notNull; }
		}

		// geographics location and language (locale id)
		public int LCID {
			get { return cultureInfo.LCID; }
		}
	
		public SybaseCompareOptions SybaseCompareOptions {
			get { return compareOptions; }
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

		public SybaseString Clone() 
		{
			return new SybaseString (value, LCID, SybaseCompareOptions);
		}

		public static CompareOptions CompareOptionsFromSybaseCompareOptions (SybaseCompareOptions compareOptions) 
		{
			CompareOptions options = CompareOptions.None;
			if ((compareOptions & SybaseCompareOptions.IgnoreCase) != 0)
				options |= CompareOptions.IgnoreCase;
			if ((compareOptions & SybaseCompareOptions.IgnoreKanaType) != 0)
				options |= CompareOptions.IgnoreKanaType;
			if ((compareOptions & SybaseCompareOptions.IgnoreNonSpace) != 0)
				options |= CompareOptions.IgnoreNonSpace;
			if ((compareOptions & SybaseCompareOptions.IgnoreWidth) != 0)
				options |= CompareOptions.IgnoreWidth;
			if ((compareOptions & SybaseCompareOptions.BinarySort) != 0)
				throw new ArgumentOutOfRangeException ();
			return options;
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

		public override int GetHashCode() 
		{
			int result = 10;
			for (int i = 0; i < value.Length; i += 1)
				result = 91 * result + (int) (value [i] ^ (value [i] >> 32));
			result = 91 * result + LCID.GetHashCode ();
			result = 91 * result + (int) compareOptions;
			return result;
		}

		public byte[] GetNonUnicodeBytes() 
		{
			return GetBytes (new ASCIIEncoding ());
		}

		public byte[] GetUnicodeBytes() 
		{
			return GetBytes (new UnicodeEncoding ());
		}

		private byte[] GetBytes (Encoding encoding)
		{
			int blen = encoding.GetByteCount (value);
			int clen = value.Length;
			byte[] bytes = new byte [blen];
			encoding.GetBytes (value, 0, clen, bytes, 0);
			return bytes;
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
