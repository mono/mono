//
// System.Data.SqlTypes.SqlString
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc. 2002
//

using System;
using System.Globalization;

namespace System.Data.SqlTypes
{
	/// <summary>
	/// A variable-length stream of characters 
	/// to be stored in or retrieved from the database
	/// </summary>
	public struct SqlString : INullable, IComparable 
	{

		#region Fields

		private string value;

		public static readonly int BinarySort;

		public static readonly int IgnoreCase;

		public static readonly int IgnoreKanaType;

		public static readonly int IgnoreNonSpace;

		public static readonly int IgnoreWidth;

		public static readonly SqlString Null = new SqlString (null);

		#endregion // Fields

		#region Constructors

		// init with a string data
		public SqlString (string data) 
		{
			this.value = data;
		}

		// init with a string data and locale id values.
		[MonoTODO]
		public SqlString (string data, int lcid) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, 
		// and an array of bytes data
		[MonoTODO]
		public SqlString (int lcid, SqlCompareOptions compareOptions, byte[] data) 
		{
			throw new NotImplementedException ();
		}

		// init with string data, locale id, and compare options
		[MonoTODO]
		public SqlString (string data, int lcid, SqlCompareOptions compareOptions) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, array of bytes data,
		// and whether unicode is encoded or not
		[MonoTODO]
		public SqlString (int lcid, SqlCompareOptions compareOptions, byte[] data, bool fUnicode) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, array of bytes data,
		// starting index in the byte array, 
		// and number of bytes to copy
		[MonoTODO]
		public SqlString (int lcid, SqlCompareOptions compareOptions, byte[] data, int index, int count) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, array of bytes data,
		// starting index in the byte array, number of byte to copy,
		// and whether unicode is encoded or not
		[MonoTODO]
		public SqlString (int lcid, SqlCompareOptions compareOptions, byte[] data, int index, int count, bool fUnicode) 
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
			get { return (value == null); }
		}

		// geographics location and language (locale id)
		public int LCID {
			[MonoTODO]
			get { throw new NotImplementedException ();
			}
		}
	
		public SqlCompareOptions SqlCompareOptions {
			[MonoTODO]
			get { throw new NotImplementedException ();
			}
		}

		public string Value {
			get { return value; }
		}

		#endregion // Public Properties

		#region Public Methods

		[MonoTODO]
		public SqlString Clone() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static CompareOptions CompareOptionsFromSqlCompareOptions ( SqlCompareOptions compareOptions) 
		{
			throw new NotImplementedException ();
		}

		// **********************************
		// Comparison Methods
		// **********************************

		[MonoTODO]
		public int CompareTo(object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlString Concat(SqlString x, SqlString y) 
		{
			return (x + y);
		}

		[MonoTODO]
		public override bool Equals(object value) 
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean Equals(SqlString x, SqlString y) 
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

		[MonoTODO]
		public SqlBoolean ToSqlBoolean() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlByte ToSqlByte() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlDateTime ToSqlDateTime() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlDecimal ToSqlDecimal() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlDouble ToSqlDouble() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlGuid ToSqlGuid() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlInt16 ToSqlInt16() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlInt32 ToSqlInt32() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlInt64 ToSqlInt64() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlMoney ToSqlMoney() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlSingle ToSqlSingle() 
		{
			throw new NotImplementedException ();
		}

		public override string ToString() 
		{
			return value;
		}

		// ***********************************
		// Operators
		// ***********************************

		// Concatenates
		public static SqlString operator + (SqlString x, SqlString y) 
		{
			return new SqlString (x.Value + y.Value);
		}

		// Equality
		public static SqlBoolean operator == (SqlString x, SqlString y) 
		{
			return new SqlBoolean (x.Value == y.Value);
		}

		[MonoTODO]
		public static SqlBoolean operator > (SqlString x, SqlString y) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator >= (SqlString x, SqlString y) 
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean operator != (SqlString x, SqlString y) 
		{ 
			return new SqlBoolean (x.Value != y.Value);
		}

		[MonoTODO]
		public static SqlBoolean operator < (SqlString x, SqlString y) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator <= (SqlString x, SqlString y) 
		{
			throw new NotImplementedException ();
		}

		// **************************************
		// Type Conversions
		// **************************************

		public static explicit operator SqlString (SqlBoolean x) 
		{
			throw new NotImplementedException ();
		}

		public static explicit operator SqlString (SqlByte x) 
		{
			throw new NotImplementedException ();
		}

		public static explicit operator SqlString (SqlDateTime x) 
		{
			throw new NotImplementedException ();
		}

		public static explicit operator SqlString (SqlDecimal x) 
		{
			throw new NotImplementedException ();
		}

		public static explicit operator SqlString (SqlDouble x) 
		{
			throw new NotImplementedException ();
		}

		public static explicit operator SqlString (SqlGuid x) 
		{
			throw new NotImplementedException ();
		}

		public static explicit operator SqlString (SqlInt16 x) 
		{
			throw new NotImplementedException ();
		}

		public static explicit operator SqlString (SqlInt32 x) 
		{
			throw new NotImplementedException ();
		}

		public static explicit operator SqlString (SqlInt64 x) 
		{
			throw new NotImplementedException ();
		}

		public static explicit operator SqlString (SqlMoney x) 
		{
			throw new NotImplementedException ();
		}

		public static explicit operator SqlString (SqlSingle x) 
		{
			throw new NotImplementedException ();
		}

		public static explicit operator string (SqlString x) 
		{
			throw new NotImplementedException ();
		}

		public static implicit operator SqlString (string x) 
		{
			throw new NotImplementedException ();
		}

		#endregion // Public Methods
	}
}
