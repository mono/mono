//
// System.Data.SqlTypes.SqlString
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//   Ville Palo (vi64pa@koti.soon.fi)
//
// (C) Ximian, Inc. 2002
// (C) Copyright 2002 Tim Coleman
//

using System;
using System.Globalization;
using System.Threading;

namespace System.Data.SqlTypes
{
	/// <summary>
	/// A variable-length stream of characters 
	/// to be stored in or retrieved from the database
	/// </summary>
	public struct SqlString : INullable, IComparable 
	{

		#region Fields

		string value;

		private bool notNull;

		// FIXME: locale id is not working yet
		private int lcid;
		private SqlCompareOptions compareOptions;

		public static readonly int BinarySort = 0x8000;
		public static readonly int IgnoreCase = 0x1;
		public static readonly int IgnoreKanaType = 0x8;
		public static readonly int IgnoreNonSpace = 0x2;
		public static readonly int IgnoreWidth = 0x10;
		public static readonly SqlString Null;

		internal static readonly NumberFormatInfo MoneyFormat;
		internal static NumberFormatInfo DecimalFormat;
		#endregion // Fields

		#region Constructors

		static SqlString ()
		{
			MoneyFormat = (NumberFormatInfo) NumberFormatInfo.InvariantInfo.Clone ();
			MoneyFormat.NumberDecimalDigits = 4;
			MoneyFormat.NumberGroupSeparator = String.Empty;

			DecimalFormat = (NumberFormatInfo) NumberFormatInfo.InvariantInfo.Clone ();
			DecimalFormat.NumberDecimalDigits = 13;
			DecimalFormat.NumberGroupSeparator = String.Empty;
		}

		// init with a string data
		public SqlString (string data) 
		{
			this.value = data;
			lcid = CultureInfo.CurrentCulture.LCID;
			notNull = true;
			this.compareOptions = SqlCompareOptions.IgnoreCase |
				SqlCompareOptions.IgnoreKanaType |
				SqlCompareOptions.IgnoreWidth;
		}

		// init with a string data and locale id values.
		public SqlString (string data, int lcid) 
		{
			this.value = data;
			this.lcid = lcid;
			notNull = true;
			this.compareOptions = SqlCompareOptions.IgnoreCase |
				SqlCompareOptions.IgnoreKanaType |
				SqlCompareOptions.IgnoreWidth;
		}

		// init with locale id, compare options, 
		// and an array of bytes data
		public SqlString (int lcid, SqlCompareOptions compareOptions, byte[] data) 
			: this (lcid, compareOptions, data, true) { }

		// init with string data, locale id, and compare options
		public SqlString (string data, int lcid, SqlCompareOptions compareOptions) 
		{
			this.value = data;
			this.lcid = lcid;
			this.compareOptions = compareOptions;
			notNull = true;
		}

		// init with locale id, compare options, array of bytes data,
		// and whether unicode is encoded or not
		public SqlString (int lcid, SqlCompareOptions compareOptions, byte[] data, bool fUnicode) 
		{
			char [] chars;

			if (fUnicode)
				chars = new char [data.Length/2];
			else
				chars = new char [data.Length];
			
			int j = 0;
			for (int i = 0; i < chars.Length; i++) {

				if (fUnicode) {
					chars [i] = (char)(data [j] << 16);
					chars [i] += (char)data [j + 1];
					j += 2;
				} else {
					chars [i] = (char)data[i];
				}
			}
				
			this.value = new String (chars);
			this.lcid = lcid;
			this.compareOptions = compareOptions;
			notNull = true;
		}

		// init with locale id, compare options, array of bytes data,
		// starting index in the byte array, 
		// and number of bytes to copy
		public SqlString (int lcid, SqlCompareOptions compareOptions, byte[] data, 
				  int index, int count) 
			: this (lcid, compareOptions, data, index, count, true) { }

		// init with locale id, compare options, array of bytes data,
		// starting index in the byte array, number of byte to copy,
		// and whether unicode is encoded or not
		public SqlString (int lcid, SqlCompareOptions compareOptions, byte[] data, int index, int count, bool fUnicode) 
		{		       
			char [] chars;

			if (fUnicode)
				chars = new char [(count - index) / 2];
			else
				chars = new char [count - index];

			if (index >= data.Length)
				throw new ArgumentOutOfRangeException ("index");

			if ((index + count) > data.Length)
				throw new ArgumentOutOfRangeException ("count");

			int j = 0;
			for (int i = index; i < chars.Length; i++) {
				
				if (fUnicode) {
					chars [i] = (char)(data[j] << 16);
					chars [i] += (char)data[j+1];
					j += 2;
				} else {
					chars [i] = (char)data [j];
					j++;
				}
			}

			this.value = new String (chars);
			this.lcid = lcid;
			this.compareOptions = compareOptions;
			notNull = true;
		}

		#endregion // Constructors


		#region Public Properties

		public CompareInfo CompareInfo {
			get { 
				return new CultureInfo (lcid).CompareInfo;
			}
		}

		public CultureInfo CultureInfo {
			get { 
				return new CultureInfo (lcid);
			}
		}

		public CompareOptions CompareOptions {
			get {
				return 
					(this.compareOptions & SqlCompareOptions.BinarySort) != 0 ? 
					CompareOptions.Ordinal :
					// 27 == all SqlCompareOptions - BinarySort 
					// (1,2,8,24 are common to CompareOptions)
					(CompareOptions) ((int) this.compareOptions & 27);
			}
		}

		public bool IsNull {
			get { return !notNull; }
		}

		// geographics location and language (locale id)
		public int LCID {
			get { 
				return lcid;
			}
		}
	
		public SqlCompareOptions SqlCompareOptions {
			get { 
				return compareOptions;
			}
		}

                public string Value {
                        get {
                                if (this.IsNull)
                                        throw new SqlNullValueException (Locale.GetText ("The property contains Null."));
                                else
                                        return value;
                        }
                }

		#endregion // Public Properties

		#region Public Methods

		public SqlString Clone() 
		{
			return new  SqlString (value, lcid, compareOptions);
		}

		public static CompareOptions CompareOptionsFromSqlCompareOptions (SqlCompareOptions compareOptions) 
		{
			CompareOptions options = CompareOptions.None;
			
			if ((compareOptions & SqlCompareOptions.IgnoreCase) != 0)
				options |= CompareOptions.IgnoreCase;
			if ((compareOptions & SqlCompareOptions.IgnoreKanaType) != 0)
				options |= CompareOptions.IgnoreKanaType;
			if ((compareOptions & SqlCompareOptions.IgnoreNonSpace) != 0)
				options |= CompareOptions.IgnoreNonSpace;
			if ((compareOptions & SqlCompareOptions.IgnoreWidth) != 0)
				options |= CompareOptions.IgnoreWidth;
			if ((compareOptions & SqlCompareOptions.BinarySort) != 0)
				// FIXME: Exception string
				throw new ArgumentOutOfRangeException (); 
			
			return options;		
		}

		// **********************************
		// Comparison Methods
		// **********************************

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SqlString))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlString"));
			else if (((SqlString)value).IsNull)
				return 1;
//			else
//				return String.Compare (this.value, ((SqlString)value).Value, (this.SqlCompareOptions & SqlCompareOptions.IgnoreCase) != 0, this.CultureInfo);
			return CultureInfo.CompareInfo.Compare (this.value, ((SqlString)value).Value, this.CompareOptions);
		}

		public static SqlString Concat(SqlString x, SqlString y) 
		{
			return (x + y);
		}

		public override bool Equals(object value) 
		{
			if (!(value is SqlString))
				return false;
			if (this.IsNull && ((SqlString)value).IsNull)
				return true;
			else if (((SqlString)value).IsNull)
				return false;
			else
				return (bool) (this == (SqlString)value);
		}

		public static SqlBoolean Equals(SqlString x, SqlString y) 
		{
			return (x == y);
		}

		public override int GetHashCode() 
		{
			int result = 10;
			for (int i = 0; i < value.Length; i++)
				result = 91 * result + (int)(value [i] ^ (value [i] >> 32));
						
			result = 91 * result + lcid.GetHashCode ();
			result = 91 * result + (int)compareOptions;

			return result;
		}

		public byte[] GetNonUnicodeBytes() 
		{
			byte [] bytes = new byte [value.Length];

			for (int i = 0; i < bytes.Length; i++) 
				bytes [i] = (byte)value [i];

			return bytes;
		}

		public byte[] GetUnicodeBytes() 
		{
			byte [] bytes = new byte [value.Length * 2];
			
			int j = 0;
			for (int i = 0; i < value.Length; i++) {				
				bytes [j] = (byte)(value [i] & 0x0000FFFF);
				bytes [j + 1] = (byte)((value [i] & 0xFFFF0000) >> 16);
				j += 2;
			}
			
			return bytes;
		}

		public static SqlBoolean GreaterThan(SqlString x, SqlString y) 
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual(SqlString x, SqlString y) 
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan(SqlString x, SqlString y) 
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual(SqlString x, SqlString y) 
		{
			return (x <= y);
		}

		public static SqlBoolean NotEquals(SqlString x,	SqlString y) 
		{
			return (x != y);
		}

		// ****************************************
		// Type Conversions From SqlString To ...
		// ****************************************

		public SqlBoolean ToSqlBoolean() 
		{
			return ((SqlBoolean)this);
		}

		public SqlByte ToSqlByte() 
		{
			return ((SqlByte)this);
		}

		public SqlDateTime ToSqlDateTime() 
		{
			return ((SqlDateTime)this);
		}

		public SqlDecimal ToSqlDecimal() 
		{
			return ((SqlDecimal)this);
		}

		public SqlDouble ToSqlDouble() 
		{
			return ((SqlDouble)this);
		}

		public SqlGuid ToSqlGuid() 
		{
			return ((SqlGuid)this);
		}

		public SqlInt16 ToSqlInt16() 
		{
			return ((SqlInt16)this);
		}

		public SqlInt32 ToSqlInt32() 
		{
			return ((SqlInt32)this);
		}

		public SqlInt64 ToSqlInt64() 
		{
			return ((SqlInt64)this);
		}

		public SqlMoney ToSqlMoney() 
		{
			return ((SqlMoney)this);
		}

		public SqlSingle ToSqlSingle() 
		{
			return ((SqlSingle)this);
		}

		public override string ToString() 
		{
			return ((string)this);
		}

		// ***********************************
		// Operators
		// ***********************************

		// Concatenates
		public static SqlString operator + (SqlString x, SqlString y) 
		{
			if (x.IsNull || y.IsNull)
				return SqlString.Null;

			return new SqlString (x.Value + y.Value);
		}

		// Equality
		public static SqlBoolean operator == (SqlString x, SqlString y) 
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value == y.Value);
		}

		// Greater Than
		public static SqlBoolean operator > (SqlString x, SqlString y) 
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.CompareTo (y) > 0);
		}

		// Greater Than Or Equal
		public static SqlBoolean operator >= (SqlString x, SqlString y) 
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.CompareTo (y) >= 0);
		}

		public static SqlBoolean operator != (SqlString x, SqlString y) 
		{ 
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value != y.Value);
		}

		// Less Than
		public static SqlBoolean operator < (SqlString x, SqlString y) 
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.CompareTo (y) < 0);
		}

		// Less Than Or Equal
		public static SqlBoolean operator <= (SqlString x, SqlString y) 
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.CompareTo (y) <= 0);
		}

		// **************************************
		// Type Conversions
		// **************************************

		public static explicit operator SqlString (SqlBoolean x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlByte x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlDateTime x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlDecimal x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
				return new SqlString (x.Value.ToString ("N", DecimalFormat));
		}

		public static explicit operator SqlString (SqlDouble x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlGuid x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlInt16 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlInt32 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlInt64 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlMoney x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
				return new SqlString (x.Value.ToString ("N", MoneyFormat));
		}

		public static explicit operator SqlString (SqlSingle x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator string (SqlString x) 
		{
			return x.Value;
		}

		public static implicit operator SqlString (string x) 
		{
			return new SqlString (x);
		}

		#endregion // Public Methods
	}
}
