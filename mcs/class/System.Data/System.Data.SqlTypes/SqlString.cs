//
// System.Data.SqlTypes.SqlString
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc. 2002
//

namespace System.Data.SqlTypes
{
	/// <summary>
	/// A variable-length stream of characters 
	/// to be stored in or retrieved from the database
	/// </summary>
	public struct SqlString : INullable, IComparable {

		// FIXME: the static readonly fields need to be initlized

		#region Constructors

		// init with a string data
		[MonoTODO]
		public SqlString(string data) {
		}

		// init with a string data and locale id values.
		[MonoTODO]
		public SqlString(string data, int lcid) {
		}

		// init with locale id, compare options, 
		// and an array of bytes data
		[MonoTODO]
		public SqlString(int lcid, SqlCompareOptions compareOptions,
			byte[] data) {
		}

		// init with string data, locale id, and compare options
		[MonoTODO]
		public SqlString(string data, int lcid,	
			SqlCompareOptions compareOptions) {
		}

		// init with locale id, compare options, array of bytes data,
		// and whether unicode is encoded or not
		[MonoTODO]
		public SqlString(int lcid, SqlCompareOptions compareOptions,
			byte[] data, bool fUnicode) {
		}

		// init with locale id, compare options, array of bytes data,
		// starting index in the byte array, 
		// and number of bytes to copy
		[MonoTODO]
		public SqlString(int lcid, SqlCompareOptions compareOptions,
			byte[] data, int index, int count) {
		}

		// init with locale id, compare options, array of bytes data,
		// starting index in the byte array, number of byte to copy,
		// and whether unicode is encoded or not
		[MonoTODO]
		public SqlString(int lcid, SqlCompareOptions compareOptions,
			byte[] data, int index, int count, bool fUnicode) {
		}

		#endregion // Constructors

		#region Public Fields

		public static readonly int BinarySort;

		public static readonly int IgnoreCase;

		public static readonly int IgnoreKanaType;

		public static readonly int IgnoreNonSpace;

		public static readonly int IgnoreWidth;

		public static readonly SqlString Null;

		#endregion // Fields

		#region Public Properties

		public CompareInfo CompareInfo {
			[MonoTODO]
			get {
			}
		}

		public CultureInfo CultureInfo {
			[MonoTODO]
			get {
			}
		}

		public bool IsNull {
			[MonoTODO]
			get {
			}
		}

		// geographics location and language (locale id)
		public int LCID {
			[MonoTODO]
			get {
			}
		}
	
		public SqlCompareOptions SqlCompareOptions {
			[MonoTODO]
			get {
			}
		}

		public string Value {
			[MonoTODO]
			get {
			}
		}

		#endregion // Public Properties

		#region Public Methods

		[MonoTODO]
		public SqlString Clone() {
		}

		[MonoTODO]
		public static CompareOptions 
			CompareOptionsFromSqlCompareOptions (
			SqlCompareOptions compareOptions) {
		}

		// **********************************
		// Comparison Methods
		// **********************************

		[MonoTODO]
		public int CompareTo(object value){
		}

		[MonoTODO]
		public static SqlString Concat(SqlString x, SqlString y) {
		}

		[MonoTODO]
		public override bool Equals(object value) {
		}

		[MonoTODO]
		public static SqlBoolean Equals(SqlString x, SqlString y) {
		}

		[MonoTODO]
		public override int GetHashCode() {
		}

		[MonoTODO]
		public byte[] GetNonUnicodeBytes() {
		}

		[MonoTODO]
		public static SqlBoolean GreaterThan(SqlString x, 
			SqlString y) {
		}

		[MonoTODO]
		public static SqlBoolean GreaterThanOrEqual(SqlString x,
			SqlString y) {
		}

		[MonoTODO]
		public static SqlBoolean LessThan(SqlString x, SqlString y) {
		}

		[MonoTODO]
		public static SqlBoolean LessThanOrEqual(SqlString x,
			SqlString y) {
		}

		[MonoTODO]
		public static SqlBoolean NotEquals(SqlString x,	SqlString y) {
		}

		// ****************************************
		// Type Conversions From SqlString To ...
		// ****************************************

		[MonoTODO]
		public SqlBoolean ToSqlBoolean() {
		}

		[MonoTODO]
		public SqlByte ToSqlByte() {
		}

		[MonoTODO]
		public SqlDateTime ToSqlDateTime() {
		}

		[MonoTODO]
		public SqlDecimal ToSqlDecimal() {
		}

		[MonoTODO]
		public SqlDouble ToSqlDouble() {
		}

		[MonoTODO]
		public SqlGuid ToSqlGuid() {
		}

		[MonoTODO]
		public SqlInt16 ToSqlInt16() {
		}

		[MonoTODO]
		public SqlInt32 ToSqlInt32() {
		}

		[MonoTODO]
		public SqlInt64 ToSqlInt64() {
		}

		[MonoTODO]
		public SqlMoney ToSqlMoney() {
		}

		[MonoTODO]
		public SqlSingle ToSqlSingle() {
		}

		[MonoTODO]
		public override string ToString() {
		}

		// ***********************************
		// Operators
		// ***********************************

		// Concatenates
		[MonoTODO]
		public static SqlString operator +(SqlString x,	SqlString y) {
		}

		// Equality
		[MonoTODO]
		public static SqlBoolean operator ==(SqlString x, 
						SqlString y) {
		}

		[MonoTODO]
		public static SqlBoolean operator >(SqlString x,
			SqlString y) {
		}

		[MonoTODO]
		public static SqlBoolean operator >=(SqlString x,
			SqlString y) {
		}

		[MonoTODO]
		public static SqlBoolean operator !=(SqlString x,
			SqlString y) { 
		}

		[MonoTODO]
		public static SqlBoolean operator <(SqlString x,
			SqlString y) {
		}

		[MonoTODO]
		public static SqlBoolean operator <=(SqlString x,
			SqlString y) {
		}

		// **************************************
		// Type Conversions
		// **************************************

		[MonoTODO]
		public static explicit operator SqlString(SqlBoolean x) {
		}

		[MonoTODO]
		public static explicit operator SqlString(SqlByte x) {
		}

		[MonoTODO]
		public static explicit operator SqlString(SqlDateTime x) {
		}

		[MonoTODO]
		public static explicit operator SqlString(SqlDecimal x) {
		}

		[MonoTODO]
		public static explicit operator SqlString(SqlDouble x) {
		}

		[MonoTODO]
		public static explicit operator SqlString(SqlGuid x) {
		}

		[MonoTODO]
		public static explicit operator SqlString(SqlInt16 x) {
		}

		[MonoTODO]
		public static explicit operator SqlString(SqlInt32 x) {
		}

		[MonoTODO]
		public static explicit operator SqlString(SqlInt64 x) {
		}

		[MonoTODO]
		public static explicit operator SqlString(SqlMoney x) {
		}

		[MonoTODO]
		public static explicit operator SqlString(SqlSingle x) {
		}

		[MonoTODO]
		public static explicit operator string(SqlString x) {
		}

		[MonoTODO]
		public static implicit operator SqlString(string x) {
		}

		[MonoTODO]
		~SqlString() {
			// FIXME: does SqlString need a destructor?
		}
	}
}
