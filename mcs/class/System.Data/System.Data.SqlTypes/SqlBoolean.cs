//
// System.Data.SqlTypes.SqlBoolean
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc. 2002
//

namespace System.Data.SqlTypes
{
	/// <summary>
	/// Represents an integer value that is either 1 or 0 
	/// to be stored in or retrieved from a database.
	/// </summary>
	public struct SqlBoolean : INullable, IComparable 
	{

		#region Fields

		private byte value;

		public static readonly SqlBoolean False = new SqlBoolean (false);
		[MonoTODO]
		public static readonly SqlBoolean Null;

		public static readonly SqlBoolean One = new SqlBoolean (1);
		
		public static readonly SqlBoolean True = new SqlBoolean (true);

		public static readonly SqlBoolean Zero = new SqlBoolean (0);


		#endregion // Fields

		#region Constructors

		public SqlBoolean (bool value) 
		{
			this.value = (byte) (value ? 1 : 0);
		}

		public SqlBoolean (int value) 
		{
			this.value = (byte) (value != 0 ? 1 : 0);
		}

		#endregion // Constructors

		#region Properties

		public byte ByteValue {
			get { return value; }
		}

		public bool IsFalse {
			get { 
				if (this.IsNull) return false;
				else return (value == 0);
			}
		}

		public bool IsNull {
			get { return (bool) (this == SqlBoolean.Null); }
		}

		public bool IsTrue {
			get { 
				if (this.IsNull) return false;
				else return (value != 0);
			}
		}

		public bool Value {
			get { 
				if (this.IsNull)
					throw new SqlNullValueException( "The property is set to null.");
				else
					return this.IsTrue;
			}
		}

		#endregion // Properties

		public static SqlBoolean And (SqlBoolean x, SqlBoolean y) 
		{
			return (x & y);
		}

		[MonoTODO]
		public int CompareTo (object value) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Equals(object value) 
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean Equals(SqlBoolean x, SqlBoolean y) 
		{
			return (x == y);
		}

		public override int GetHashCode() 
		{
			return (int)value;
		}

		public static SqlBoolean NotEquals(SqlBoolean x, SqlBoolean y) 
		{
			return (x != y);
		}

		public static SqlBoolean OnesComplement(SqlBoolean x) 
		{
			return ~x;
		}

		public static SqlBoolean Or(SqlBoolean x, SqlBoolean y) 
		{
			return (x | y);
		}

		public static SqlBoolean Parse(string s) 
		{
			return new SqlBoolean (Boolean.Parse (s));
		}

		public SqlByte ToSqlByte() 
		{
			return new SqlByte (value);
		}

		// **************************************************
		// Conversion from SqlBoolean to other SqlTypes
		// **************************************************

		public SqlDecimal ToSqlDecimal() 
		{
			return ((SqlDecimal)value);
		}

		public SqlDouble ToSqlDouble() 
		{
			return ((SqlDouble)value);
		}

		public SqlInt16 ToSqlInt16() 
		{
			return ((SqlInt16)value);
		}

		public SqlInt32 ToSqlInt32() 
		{
			return ((SqlInt32)value);
		}

		public SqlInt64 ToSqlInt64() 
		{
			return ((SqlInt64)value);
		}

		public SqlMoney ToSqlMoney() 
		{
			return ((SqlMoney)value);
		}

		public SqlSingle ToSqlSingle() 
		{
			return ((SqlSingle)value);
		}

		public SqlString ToSqlString() 
		{
			return new SqlString (this.ToString());
		}

		[MonoTODO]
		public override string ToString() 
		{
			throw new NotImplementedException ();
		}

		// Bitwise exclusive-OR (XOR)
		public static SqlBoolean Xor(SqlBoolean x, SqlBoolean y) 
		{
			return (x ^ y);
		}

		// **************************************************
		// Public Operators
		// **************************************************

		// Bitwise AND
		public static SqlBoolean operator & (SqlBoolean x, SqlBoolean y)
		{
			return new SqlBoolean (x.Value & y.Value);
		}

		// Bitwise OR
		public static SqlBoolean operator | (SqlBoolean x, SqlBoolean y)
		{
			return new SqlBoolean (x.Value | y.Value);

		}

		// Compares two instances for equality
		public static SqlBoolean operator == (SqlBoolean x, SqlBoolean y)
		{
			return new SqlBoolean (x.Value == y.Value);
		}
		
		// Bitwize exclusive-OR (XOR)
		public static SqlBoolean operator ^ (SqlBoolean x, SqlBoolean y) 
		{
			return new SqlBoolean (x.Value ^ y.Value);
		}

		// test Value of SqlBoolean to determine it is false.
		public static bool operator false (SqlBoolean x) 
		{
			return x.IsFalse;
		}

		// in-equality
		public static SqlBoolean operator != (SqlBoolean x, SqlBoolean y)
		{
			return new SqlBoolean (x.Value != y.Value);
		}

		// Logical NOT
		public static SqlBoolean operator ! (SqlBoolean x) 
		{
			return new SqlBoolean (!x.Value);
		}

		// One's Complement
		public static SqlBoolean operator ~ (SqlBoolean x) 
		{
			return new SqlBoolean (~x.ByteValue);
		}

		// test to see if value is true
		public static bool operator true (SqlBoolean x) 
		{
			return x.IsTrue;
		}

		// ****************************************
		// Type Conversion 
		// ****************************************

		
		// SqlBoolean to Boolean
		public static explicit operator bool (SqlBoolean x) 
		{
			return x.Value;
		}

		
		// SqlByte to SqlBoolean
		public static explicit operator SqlBoolean (SqlByte x) 
		{
			return x.ToSqlBoolean ();
		}

		// SqlDecimal to SqlBoolean
		public static explicit operator SqlBoolean (SqlDecimal x) 
		{
			return x.ToSqlBoolean ();
		}
		
		// SqlDouble to SqlBoolean
		public static explicit operator SqlBoolean (SqlDouble x) 
		{
			return x.ToSqlBoolean ();
		}

		// SqlInt16 to SqlBoolean
		public static explicit operator SqlBoolean (SqlInt16 x) 
		{
			return x.ToSqlBoolean ();
		}

		// SqlInt32 to SqlBoolean
		public static explicit operator SqlBoolean (SqlInt32 x) 
		{
			return x.ToSqlBoolean ();
		}

		// SqlInt64 to SqlBoolean
		public static explicit operator SqlBoolean (SqlInt64 x) 
		{
			return x.ToSqlBoolean ();
		}

		// SqlMoney to SqlBoolean
		public static explicit operator SqlBoolean (SqlMoney x) 
		{
			return x.ToSqlBoolean ();
		}

		// SqlSingle to SqlBoolean
		public static explicit operator SqlBoolean (SqlSingle x) 
		{
			return x.ToSqlBoolean ();
		}

		// SqlString to SqlBoolean
		public static explicit operator SqlBoolean (SqlString x) 
		{
			return x.ToSqlBoolean ();
		}

		// Boolean to SqlBoolean
		public static implicit operator SqlBoolean (bool x) 
		{
			return new SqlBoolean (x);
		}
	}
}
