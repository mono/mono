//
// System.Data.SqlTypes.SqlInt32
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc. 2002
//

using System;

namespace System.Data.SqlTypes
{

	/// <summary>
	/// a 32-bit signed integer to be used in reading or writing
	/// of data from a database
	/// </summary>
	public struct SqlInt32 : INullable, IComparable 
	{
		#region Fields

		private int value;

		[MonoTODO]
		public static readonly SqlInt32 MaxValue;

		[MonoTODO]
		public static readonly SqlInt32 MinValue;

		[MonoTODO]
		public static readonly SqlInt32 Null;

		[MonoTODO]
		public static readonly SqlInt32 Zero;

		#endregion

		#region Constructors

		public SqlInt32(int value) 
		{
			this.value = value;
		}

		#endregion

		// Public Properties

		public bool IsNull {
			get { return (bool) (this == SqlInt32.Null); }
		}

		public int Value {
			get { return value; }
		}

		// Public Methods

		public static SqlInt32 Add(SqlInt32 x, SqlInt32 y) 
		{
			return (x + y);
		}

		public static SqlInt32 BitwiseAnd(SqlInt32 x, SqlInt32 y) 
		{
			return (x & y);
		}
		
		public static SqlInt32 BitwiseOr(SqlInt32 x, SqlInt32 y) 
		{
			return (x | y);
		}

		[MonoTODO]
		public int CompareTo(object value) 
		{
			throw new NotImplementedException ();	
		}

		public static SqlInt32 Divide(SqlInt32 x, SqlInt32 y) 
		{
			return (x / y);
		}

		[MonoTODO]
		public override bool Equals(object value) 
		{
			throw new NotImplementedException ();	
		}

		public static SqlBoolean Equals(SqlInt32 x, SqlInt32 y) 
		{
			return (x == y);
		}

		public override int GetHashCode() 
		{
			return value;
		}

		public static SqlBoolean GreaterThan (SqlInt32 x, SqlInt32 y) 
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlInt32 x, SqlInt32 y) 
		{
			return (x >= y);
		}
                
		public static SqlBoolean LessThan(SqlInt32 x, SqlInt32 y) 
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual(SqlInt32 x, SqlInt32 y) 
		{
			return (x <= y);
		}

		public static SqlInt32 Mod(SqlInt32 x, SqlInt32 y) 
		{
			return (x % y);
		}

		public static SqlInt32 Multiply(SqlInt32 x, SqlInt32 y) 
		{
			return (x * y);
		}

		public static SqlBoolean NotEquals(SqlInt32 x, SqlInt32 y) 
		{
			return (x != y);
		}

		public static SqlInt32 OnesComplement(SqlInt32 x) 
		{
			return ~x;
		}

		public static SqlInt32 Parse(string s) 
		{
			throw new NotImplementedException ();
		}

		public static SqlInt32 Subtract(SqlInt32 x, SqlInt32 y) 
		{
			return (x - y);
		}

		// Type Conversions

		public SqlBoolean ToSqlBoolean() 
		{
			return ((SqlBoolean)this);
		}

		public SqlByte ToSqlByte() 
		{
			return ((SqlByte)this);
		}

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

		[MonoTODO]
		public override string ToString() 
		{
			throw new NotImplementedException ();	
		}

		public static SqlInt32 Xor(SqlInt32 x, SqlInt32 y) 
		{
			return (x ^ y);
		}

		// Public Operators

		// Compute Addition
		public static SqlInt32 operator +(SqlInt32 x, SqlInt32 y) 
		{
			return new SqlInt32 (x.Value + y.Value);
		}

		// Bitwise AND
		public static SqlInt32 operator &(SqlInt32 x, SqlInt32 y) 
		{
			return new SqlInt32 (x.Value & y.Value);
		}

		// Bitwise OR
		public static SqlInt32 operator |(SqlInt32 x, SqlInt32 y) 
		{
			return new SqlInt32 (x.Value | y.Value);
		}

		// Compute Division
		public static SqlInt32 operator /(SqlInt32 x, SqlInt32 y) 
		{
			return new SqlInt32 (x.Value / y.Value);
		}

		// Compare Equality
		public static SqlBoolean operator ==(SqlInt32 x, SqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		// Bitwise Exclusive-OR (XOR)
		public static SqlInt32 operator ^(SqlInt32 x, SqlInt32 y) 
		{
			return new SqlInt32 (x.Value ^ y.Value);
		}

		// > Compare
		public static SqlBoolean operator >(SqlInt32 x, SqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value > y.Value);
		}

		// >= Compare
		public static SqlBoolean operator >=(SqlInt32 x, SqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value >= y.Value);
		}

		// != Inequality Compare
		public static SqlBoolean operator !=(SqlInt32 x, SqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value != y.Value);
		}
		
		// < Compare
		public static SqlBoolean operator <(SqlInt32 x, SqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value < y.Value);
		}

		// <= Compare
		public static SqlBoolean operator <=(SqlInt32 x, SqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value <= y.Value);
		}

		// Compute Modulus
		public static SqlInt32 operator %(SqlInt32 x, SqlInt32 y) 
		{
			return new SqlInt32 (x.Value % y.Value);
		}

		// Compute Multiplication
		public static SqlInt32 operator *(SqlInt32 x, SqlInt32 y) 
		{
			return new SqlInt32 (x.Value * y.Value);
		}

		// Ones Complement
		public static SqlInt32 operator ~(SqlInt32 x) 
		{
			return new SqlInt32 (~x.Value);
		}

		// Subtraction
		public static SqlInt32 operator -(SqlInt32 x, SqlInt32 y) 
		{
			return new SqlInt32 (x.Value - y.Value);
		}

		// Negates the Value
		public static SqlInt32 operator -(SqlInt32 x) 
		{
			return new SqlInt32 (-x.Value);
		}

		// Type Conversions

		public static explicit operator SqlInt32(SqlBoolean x) 
		{
			return new SqlInt32 ((int)x.ByteValue);
		}

		public static explicit operator SqlInt32(SqlDecimal x) 
		{
			return new SqlInt32 ((int)x.Value);
		}

		public static explicit operator SqlInt32(SqlDouble x) 
		{
			return new SqlInt32 ((int)x.Value);
		}

		public static explicit operator int(SqlInt32 x)
		{
			return x.Value;
		}

		public static explicit operator SqlInt32(SqlInt64 x) 
		{
			return new SqlInt32 ((int)x.Value);
		}

		public static explicit operator SqlInt32(SqlMoney x) 
		{
			return new SqlInt32 ((int)x.Value);
		}

		public static explicit operator SqlInt32(SqlSingle x) 
		{
			return new SqlInt32 ((int)x.Value);
		}

		[MonoTODO]
		public static explicit operator SqlInt32(SqlString x) 
		{
			throw new NotImplementedException ();
		}

		public static implicit operator SqlInt32(int x) 
		{
			return new SqlInt32 (x);
		}

		public static implicit operator SqlInt32(SqlByte x) 
		{
			return new SqlInt32 ((int)x.Value);
		}

		public static implicit operator SqlInt32(SqlInt16 x) 
		{
			return new SqlInt32 ((int)x.Value);
		}
	}
}
