//
// System.Data.SqlTypes.SqlInt32
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
	/// a 32-bit signed integer to be used in reading or writing
	/// of data from a database
	/// </summary>
	public struct SqlInt32 : INullable, IComparable {
		// FIXME: Fields need to be initialized

		// Constructor
		[MonoTODO]
		public SqlInt32(int value) {
		}

		// Fields (Constants)

		public static readonly SqlInt32 MaxValue;

		public static readonly SqlInt32 MinValue;

		public static readonly SqlInt32 Null;

		public static readonly SqlInt32 Zero;

		// Public Properties

		public bool IsNull {
			[MonoTODO]
			get {
			} 
		}

		public int Value {
			[MonoTODO]
			get {
			}
		}

		// Public Methods

		[MonoTODO]
		public static SqlInt32 Add(SqlInt32 x, SqlInt32 y) {
		}

		[MonoTODO]
		public static SqlInt32 BitwiseAnd(SqlInt32 x, SqlInt32 y) {
		}
		
		[MonoTODO]
		public static SqlInt32 BitwiseOr(SqlInt32 x, SqlInt32 y) {
		}

		[MonoTODO]
		public int CompareTo(object value) {
		}

		[MonoTODO]
		public static SqlInt32 Divide(SqlInt32 x, SqlInt32 y) {
		}

		[MonoTODO]
		public override bool Equals(object value) {
		}

		[MonoTODO]
		public static SqlBoolean Equals(SqlInt32 x, SqlInt32 y) {
		}

		[MonoTODO]
		public override int GetHashCode() {
		}

		[MonoTODO]
		public static SqlBoolean GreaterThan(SqlInt32 x,
			SqlInt32 y) {
		}

		[MonoTODO]
		public static SqlBoolean GreaterThanOrEqual(SqlInt32 x,
			SqlInt32 y) {
		}
                
		[MonoTODO]
		public static SqlBoolean LessThan(SqlInt32 x, SqlInt32 y) {
		}

		[MonoTODO]
		public static SqlBoolean LessThanOrEqual(SqlInt32 x,
			SqlInt32 y) {
		}

		[MonoTODO]
		public static SqlInt32 Mod(SqlInt32 x, SqlInt32 y) {
		}

		[MonoTODO]
		public static SqlInt32 Multiply(SqlInt32 x,
			SqlInt32 y) {
		}

		[MonoTODO]
		public static SqlBoolean NotEquals(SqlInt32 x,
			SqlInt32 y) {
		}

		[MonoTODO]
		public static SqlInt32 OnesComplement(SqlInt32 x) {
		}

		[MonoTODO]
		public static SqlInt32 Parse(string s) {
		}

		[MonoTODO]
		public static SqlInt32 Subtract(SqlInt32 x, SqlInt32 y) {
		}

		// Type Conversions

		[MonoTODO]
		public SqlBoolean ToSqlBoolean() {
		}

		[MonoTODO]
		public SqlByte ToSqlByte() {
		}

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

		[MonoTODO]
		public static SqlInt32 Xor(SqlInt32 x, SqlInt32 y) {
		}

		// Public Operators

		// Compute Addition
		[MonoTODO]
		public static SqlInt32 operator +(SqlInt32 x, SqlInt32 y) {
		}

		// Bitwise AND
		[MonoTODO]
		public static SqlInt32 operator &(SqlInt32 x, SqlInt32 y) {
		}

		// Bitwise OR
		[MonoTODO]
		public static SqlInt32 operator |(SqlInt32 x, SqlInt32 y) {
		}

		// Compute Division
		[MonoTODO]
		public static SqlInt32 operator /(SqlInt32 x, SqlInt32 y) {
		}

		// Compare Equality
		[MonoTODO]
		public static SqlBoolean operator ==(SqlInt32 x, SqlInt32 y) {
		}

		// Bitwise Exclusive-OR (XOR)
		[MonoTODO]
		public static SqlInt32 operator ^(SqlInt32 x, SqlInt32 y) {
		}

		// > Compare
		[MonoTODO]
		public static SqlBoolean operator >(SqlInt32 x, SqlInt32 y) {
		}

		// >= Compare
		[MonoTODO]
		public static SqlBoolean operator >=(SqlInt32 x, SqlInt32 y) {
		}

		// != Inequality Compare
		[MonoTODO]
		public static SqlBoolean operator !=(SqlInt32 x, SqlInt32 y) {
		}
		
		// < Compare
		[MonoTODO]
		public static SqlBoolean operator <(SqlInt32 x, SqlInt32 y) {
		}

		// <= Compare
		[MonoTODO]
		public static SqlBoolean operator <=(SqlInt32 x, SqlInt32 y) {
		}

		// Compute Modulus
		[MonoTODO]
		public static SqlInt32 operator %(SqlInt32 x, SqlInt32 y) {
		}

		// Compute Multiplication
		[MonoTODO]
		public static SqlInt32 operator *(SqlInt32 x, SqlInt32 y) {
		}

		// Ones Complement
		[MonoTODO]
		public static SqlInt32 operator ~(SqlInt32 x) {
		}

		// Subtraction
		[MonoTODO]
		public static SqlInt32 operator -(SqlInt32 x, SqlInt32 y) {
		}

		// Negates the Value
		[MonoTODO]
		public static SqlInt32 operator -(SqlInt32 x) {
		}

		// Type Conversions

		[MonoTODO]
		public static explicit operator SqlInt32(SqlBoolean x) {
		}

		[MonoTODO]
		public static explicit operator SqlInt32(SqlDecimal x) {
		}

		[MonoTODO]
		public static explicit operator SqlInt32(SqlDouble x) {
		}

		[MonoTODO]
		public static explicit operator int(SqlInt32 x){
		}

		[MonoTODO]
		public static explicit operator SqlInt32(SqlInt64 x) {
		}

		[MonoTODO]
		public static explicit operator SqlInt32(SqlMoney x) {
		}

		[MonoTODO]
		public static explicit operator SqlInt32(SqlSingle x) {
		}

		[MonoTODO]
		public static explicit operator SqlInt32(SqlString x) {
		}

		[MonoTODO]
		public static implicit operator SqlInt32(int x) {
		}

		[MonoTODO]
		public static implicit operator SqlInt32(SqlByte x) {
		}

		[MonoTODO]
		public static implicit operator SqlInt32(SqlInt16 x) {
		}

		[MonoTODO]
		~SqlInt32() {
			// FIXME: does this class need a Finalize?
		}
	}
}
