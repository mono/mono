//
// System.Data.SqlTypes.SqlBoolean
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
	/// Represents an integer value that is either 1 or 0 
	/// to be stored in or retrieved from a database.
	/// </summary>
	public struct SqlBoolean : INullable, IComparable {

		#region Fields

		// FIXME: populate the static Fields?

		// Value
		public static readonly SqlBoolean False;

		// Value
		public static readonly SqlBoolean Null;

		// ByteValue
		public static readonly SqlBoolean One;
		
		// Value
		public static readonly SqlBoolean True;

		// ByteValue
		public static readonly SqlBoolean Zero;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public SqlBoolean(bool value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlBoolean(int value) {
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		public byte ByteValue {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public bool IsFalse {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public bool IsNull {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public bool IsTrue {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public bool Value {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion // Properties

		[MonoTODO]
		public static SqlBoolean And(SqlBoolean x, SqlBoolean y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int CompareTo(object value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Equals(object value) {
		}

		[MonoTODO]
		public static SqlBoolean Equals(SqlBoolean x, SqlBoolean y) {
		}

		[MonoTODO]
		public override int GetHashCode() {
		}

		[MonoTODO]
		public static SqlBoolean NotEquals(SqlBoolean x, SqlBoolean y) {
		}

		[MonoTODO]
		public static SqlBoolean OnesComplement(SqlBoolean x) {
		}

		[MonoTODO]
		public static SqlBoolean Or(SqlBoolean x, SqlBoolean y) {
		}

		[MonoTODO]
		public static SqlBoolean Parse(string s) {
		}

		[MonoTODO]
		public SqlByte ToSqlByte() {
		}

		// **************************************************
		// Conversion from SqlBoolean to other SqlTypes
		// **************************************************

		[MonoTODO]
		public SqlDecimal ToSqlDecimal() {
		}

		[MonoTODO]
		public SqlDouble ToSqlDouble() {
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
		public SqlString ToSqlString() {
		}

		[MonoTODO]
		public override string ToString() {
		}

		// Bitwise exclusive-OR (XOR)
		[MonoTODO]
		public static SqlBoolean Xor(SqlBoolean x, SqlBoolean y) {
		}

		// **************************************************
		// Public Operators
		// **************************************************

		// Bitwise AND
		[MonoTODO]
		public static SqlBoolean operator &(SqlBoolean x, SqlBoolean y) {
		}

		// Bitwise OR
		[MonoTODO]
		public static SqlBoolean operator |(SqlBoolean x, SqlBoolean y) {
		}

		// Compares two instances for equality
		[MonoTODO]
		public static SqlBoolean operator ==(SqlBoolean x, SqlBoolean y) {
		}
		
		// Bitwize exclusive-OR (XOR)
		[MonoTODO]
		public static SqlBoolean operator ^(SqlBoolean x, SqlBoolean y) {
		}

		// test Value of SqlBoolean to determine it is false.
		[MonoTODO]
		public static bool operator false(SqlBoolean x) {
		}

		// in-equality
		[MonoTODO]
		public static SqlBoolean operator !=(SqlBoolean x, SqlBoolean y) {
		}

		// Logical NOT
		[MonoTODO]
		public static SqlBoolean operator !(SqlBoolean x) {
		}

		// One's Complement
		[MonoTODO]
		public static SqlBoolean operator ~(SqlBoolean x) {
		}

		// test to see if value is true
		[MonoTODO]
		public static bool operator true(SqlBoolean x) {
		}

		// ****************************************
		// Type Conversion 
		// ****************************************

		
		// SqlBoolean to Boolean
		[MonoTODO]
		public static explicit operator bool(SqlBoolean x) {
		}

		
		// SqlByte to SqlBoolean
		[MonoTODO]
		public static explicit operator SqlBoolean(SqlByte x) {
		}

		// SqlDecimal to SqlBoolean
		[MonoTODO]
		public static explicit operator SqlBoolean(SqlDecimal x) {
		}
		
		// SqlDouble to SqlBoolean
		[MonoTODO]
		public static explicit operator SqlBoolean(SqlDouble x) {
		}

		// SqlInt16 to SqlBoolean
		[MonoTODO]
		public static explicit operator SqlBoolean(SqlInt16 x) {
		}

		// SqlInt32 to SqlBoolean
		[MonoTODO]
		public static explicit operator SqlBoolean(SqlInt32 x) {
		}

		// SqlInt64 to SqlBoolean
		[MonoTODO]
		public static explicit operator SqlBoolean(SqlInt64 x) {
		}

		// SqlMoney to SqlBoolean
		[MonoTODO]
		public static explicit operator SqlBoolean(SqlMoney x) {
		}

		// SqlSingle to SqlBoolean
		[MonoTODO]
		public static explicit operator SqlBoolean(SqlSingle x) {
		}

		// SqlString to SqlBoolean
		[MonoTODO]
		public static explicit operator SqlBoolean(SqlString x) {
		}

		// Boolean to SqlBoolean
		[MonoTODO]
		public static implicit operator SqlBoolean(bool x) {
		}
	}
}
