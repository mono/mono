//
// System.Data.SqlTypes.SqlBoolean
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc. 2002
// (C) Copyright 2002 Tim Coleman
//

using System;
using System.Globalization;

namespace System.Data.SqlTypes
{
	/// <summary>
	/// Represents an integer value that is either 1 or 0 
	/// to be stored in or retrieved from a database.
	/// </summary>
	public struct SqlBoolean : INullable, IComparable 
	{

		#region Fields

		byte value;
		
		// default is false
		private bool notNull;

		public static readonly SqlBoolean False = new SqlBoolean (false);
		public static readonly SqlBoolean Null;
		public static readonly SqlBoolean One = new SqlBoolean (1);
		public static readonly SqlBoolean True = new SqlBoolean (true);
		public static readonly SqlBoolean Zero = new SqlBoolean (0);

		#endregion // Fields

		#region Constructors

		public SqlBoolean (bool value) 
		{
			this.value = (byte) (value ? 1 : 0);
			notNull = true;
		}

		public SqlBoolean (int value) 
		{
			this.value = (byte) (value != 0 ? 1 : 0);
			notNull = true;
		}

		#endregion // Constructors

		#region Properties

		public byte ByteValue {
			get {
				if (this.IsNull)
					throw new SqlNullValueException(Locale.GetText("The property is set to null."));
				else
					return value;
			}
		}

		public bool IsFalse {
			get { 
				if (this.IsNull) 
					return false;
				else 
					return (value == 0);
			}
		}

		public bool IsNull {
			get { 
				return !notNull;
			}
		}

		public bool IsTrue {
			get { 
				if (this.IsNull) 
					return false;
				else 	
					return (value != 0);
			}
		}

		public bool Value {
			get { 
				if (this.IsNull)
					throw new SqlNullValueException(Locale.GetText("The property is set to null."));
				else
					return this.IsTrue;
			}
		}

		#endregion // Properties

		public static SqlBoolean And (SqlBoolean x, SqlBoolean y) 
		{
			return (x & y);
		}

		public int CompareTo (object value) 
		{
			if (value == null)
				return 1;
			else if (!(value is SqlBoolean))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlBoolean"));
			else if (((SqlBoolean)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SqlBoolean)value).ByteValue);
		}

		public override bool Equals(object value) 
		{
			if (!(value is SqlByte))
				return false;
			else
				return (bool) (this == (SqlBoolean)value);
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
			return ((SqlDecimal)this);
		}

		public SqlDouble ToSqlDouble() 
		{
			return ((SqlDouble)this);
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

		public SqlString ToSqlString() 
		{
			if (this.IsNull)
			        return new SqlString ("Null");
			if (this.IsTrue)
				return new SqlString ("True");
			else
				return new SqlString ("False");
		}

		public override string ToString() 
		{
			if (this.IsNull)
			        return "Null";
			if (this.IsTrue)
				return "True";
			else
				return "False";
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
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
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
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value != y.Value);
		}

		// Logical NOT
		public static SqlBoolean operator ! (SqlBoolean x) 
		{
			if (x.IsNull)
				return SqlBoolean.Null;
			else
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
			if (x.IsNull)
				return Null;
			else
				return new SqlBoolean ((int)x.Value);
		}

		// SqlDecimal to SqlBoolean
		public static explicit operator SqlBoolean (SqlDecimal x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlBoolean ((int)x.Value);
		}
		
		// SqlDouble to SqlBoolean
		public static explicit operator SqlBoolean (SqlDouble x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlBoolean ((int)x.Value);
		}

		// SqlInt16 to SqlBoolean
		public static explicit operator SqlBoolean (SqlInt16 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlBoolean ((int)x.Value);
		}

		// SqlInt32 to SqlBoolean
		public static explicit operator SqlBoolean (SqlInt32 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlBoolean (x.Value);
		}

		// SqlInt64 to SqlBoolean
		public static explicit operator SqlBoolean (SqlInt64 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlBoolean ((int)x.Value);
		}

		// SqlMoney to SqlBoolean
		public static explicit operator SqlBoolean (SqlMoney x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlBoolean ((int)x.Value);
		}

		// SqlSingle to SqlBoolean
		public static explicit operator SqlBoolean (SqlSingle x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlBoolean ((int)x.Value);
		}

		// SqlString to SqlBoolean
		public static explicit operator SqlBoolean (SqlString x) 
		{
			return SqlBoolean.Parse (x.Value);
		}

		// Boolean to SqlBoolean
		public static implicit operator SqlBoolean (bool x) 
		{
			return new SqlBoolean (x);
		}
	}
}
