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

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Xml;
using System.Text;
using System.Threading;
using System.Xml.Schema;
using System.Globalization;
using System.Xml.Serialization;

namespace System.Data.SqlTypes
{
	/// <summary>
	/// A variable-length stream of characters 
	/// to be stored in or retrieved from the database
	/// </summary>
#if NET_2_0
	[SerializableAttribute]
	[XmlSchemaProvider ("GetXsdType")]
#endif
	public struct SqlString : INullable, IComparable 
#if NET_2_0
				, IXmlSerializable
#endif
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

		internal static NumberFormatInfo DecimalFormat;
		#endregion // Fields

		#region Constructors

		static SqlString ()
		{
			DecimalFormat = (NumberFormatInfo) NumberFormatInfo.InvariantInfo.Clone ();
			DecimalFormat.NumberDecimalDigits = 13;
			DecimalFormat.NumberGroupSeparator = String.Empty;
		}

		// init with a string data
		public SqlString (string data) 
		{
			this.value = data;
			lcid = CultureInfo.CurrentCulture.LCID;
			if (value != null)
				notNull = true;
			else
				notNull = false;
			this.compareOptions = SqlCompareOptions.IgnoreCase |
				SqlCompareOptions.IgnoreKanaType |
				SqlCompareOptions.IgnoreWidth;
		}

		// init with a string data and locale id values.
		public SqlString (string data, int lcid) 
		{
			this.value = data;
			this.lcid = lcid;
			if (value != null)
				notNull = true;
			else
				notNull = false;
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
			if (value != null)
				notNull = true;
			else
				notNull = false;
		}

		// init with locale id, compare options, array of bytes data,
		// and whether unicode is encoded or not
		public SqlString (int lcid, SqlCompareOptions compareOptions, byte[] data, bool fUnicode) 
		{
			Encoding encoding = (fUnicode ? Encoding.Unicode : Encoding.ASCII);
			this.value = encoding.GetString (data);
			this.lcid = lcid;
			this.compareOptions = compareOptions;
			if (value != null)
				notNull = true;
			else
				notNull = false;
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
			Encoding encoding = (fUnicode ? Encoding.Unicode : Encoding.ASCII);
			this.value = encoding.GetString (data, index, count);
			this.lcid = lcid;
			this.compareOptions = compareOptions;
			if (value != null)
				notNull = true;
			else
				notNull = false;
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

		#region Private Properties

		private CompareOptions CompareOptions {
			get {
				return
					(this.compareOptions & SqlCompareOptions.BinarySort) != 0 ?
					CompareOptions.Ordinal :
					// 27 == all SqlCompareOptions - BinarySort 
					// (1,2,8,24 are common to CompareOptions)
					(CompareOptions)((int)this.compareOptions & 27);
			}
		}

		#endregion Private Properties

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
			
			return CompareSqlString ((SqlString)value);
		}

		
		private int CompareSqlString (SqlString value)
		{
			if (value.IsNull)
				return 1;
			else if (value.CompareOptions != this.CompareOptions)
				throw new SqlTypeException (Locale.GetText ("Two strings to be compared have different collation"));
//			else
//				return String.Compare (this.value, ((SqlString)value).Value, (this.SqlCompareOptions & SqlCompareOptions.IgnoreCase) != 0, this.CultureInfo);
			return CultureInfo.CompareInfo.Compare (this.value, value.Value, this.CompareOptions);
		}

		public static SqlString Concat(SqlString x, SqlString y) 
		{
			return (x + y);
		}

		public override bool Equals(object value) 
		{
			if (!(value is SqlString))
				return false;
			if (this.IsNull)
				return ((SqlString)value).IsNull;
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
			return Encoding.ASCII.GetBytes (value);
		}

		public byte[] GetUnicodeBytes() 
		{
			return Encoding.Unicode.GetBytes (value);
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
			if (!notNull)
				return "Null";
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
			else
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
				// return new SqlString (x.Value.ToString ("N", DecimalFormat));
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
				return new SqlString (x.ToString ());
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

#if NET_2_0
                public static SqlString Add (SqlString x, SqlString y)
                {
                  	return (x + y);
                }

		public int CompareTo (SqlString value)
                {
                	return CompareSqlString (value);
                }
#endif



		#endregion // Public Methods
#if NET_2_0
		public static XmlQualifiedName GetXsdType (XmlSchemaSet schemaSet)
		{
			if (schemaSet != null && schemaSet.Count == 0) {
				XmlSchema xs = new XmlSchema ();
				XmlSchemaComplexType ct = new XmlSchemaComplexType ();
				ct.Name = "string";
				xs.Items.Add (ct);
				schemaSet.Add (xs);
			}
			return new XmlQualifiedName ("string", "http://www.w3.org/2001/XMLSchema");
		}

		XmlSchema IXmlSerializable.GetSchema ()
		{
			return null;
		}
		
		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			if (reader == null)
				return;

			switch (reader.ReadState) {
			case ReadState.EndOfFile:
			case ReadState.Error:
			case ReadState.Closed:
				return;
			}
			// Skip XML declaration and prolog
			// or do I need to validate for the <string> tag?
			reader.MoveToContent ();
			if (reader.EOF)
				return;
			
			reader.Read ();
			if (reader.NodeType == XmlNodeType.EndElement)
				return;

			if (reader.Value.Length > 0) {
				if (String.Compare ("Null", reader.Value) == 0) {
					// means a null reference/invalid value
					notNull = false;
					return; 
				}
				// FIXME: Validate the value for expected format
				this.value = reader.Value;
				this.notNull = true;
				this.compareOptions = SqlCompareOptions.IgnoreCase |
					SqlCompareOptions.IgnoreKanaType |
					SqlCompareOptions.IgnoreWidth;
			}
		}
		
		void IXmlSerializable.WriteXml (XmlWriter writer) 
		{
			writer.WriteString (this.ToString ());
		}
#endif
	}
}
