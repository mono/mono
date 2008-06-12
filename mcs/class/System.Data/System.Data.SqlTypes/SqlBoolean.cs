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
using System.Globalization;
#if NET_2_0
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Runtime.Serialization;
#endif

namespace System.Data.SqlTypes
{
	/// <summary>
	/// Represents an integer value that is either 1 or 0 
	/// to be stored in or retrieved from a database.
	/// </summary>
#if NET_2_0
	[SerializableAttribute]
	[XmlSchemaProvider ("GetXsdType")]
#endif
	public struct SqlBoolean : INullable, IComparable
#if NET_2_0
				   , IXmlSerializable
#endif
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
			if (!(value is SqlBoolean))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlBoolean"));

			return CompareTo ((SqlBoolean) value);
		}
#if NET_2_0
		public
#endif
		int CompareTo (SqlBoolean value) 
		{
			if (value.IsNull)
				return 1;
			else
				return this.value.CompareTo (value.ByteValue);
		}

		public override bool Equals(object value) 
		{
			if (!(value is SqlBoolean))
				return false;
			if (this.IsNull)
				return ((SqlBoolean)value).IsNull;
			else if (((SqlBoolean)value).IsNull)
				return false;
			else
				return (bool) (this == (SqlBoolean)value);
		}

		public static SqlBoolean Equals(SqlBoolean x, SqlBoolean y) 
		{
			return (x == y);
		}

#if NET_2_0
		public static SqlBoolean GreaterThan (SqlBoolean x, SqlBoolean y) 
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEquals (SqlBoolean x, SqlBoolean y) 
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlBoolean x, SqlBoolean y) 
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEquals (SqlBoolean x, SqlBoolean y) 
		{
			return (x <= y);
		}
#endif
		public override int GetHashCode() 
		{
			int hash;
			if (this.IsTrue)
				hash = 1;
			else 
				hash = 0;

			return hash;
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
			switch (s) {
			case "0":
				return new SqlBoolean (false);
			case "1":
				return new SqlBoolean (true);
			}
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
			SqlBoolean b;
			if (x.IsTrue)
				b = new SqlBoolean(false);
			else
				b = new SqlBoolean(true);

			return b;
		}

#if NET_2_0
		public static SqlBoolean operator > (SqlBoolean x, SqlBoolean y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;

			return new SqlBoolean (Compare (x, y) > 0);
		}

		public static SqlBoolean operator >= (SqlBoolean x, SqlBoolean y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;

			return new SqlBoolean (Compare (x, y) >= 0);
		}

		public static SqlBoolean operator < (SqlBoolean x, SqlBoolean y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;

			return new SqlBoolean (Compare (x, y) < 0);
		}

		public static SqlBoolean operator <= (SqlBoolean x, SqlBoolean y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;

			return new SqlBoolean (Compare (x, y) <= 0);
		}
#endif
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
			checked {
				if (x.IsNull)
					return Null;
				else
					return new SqlBoolean ((int)x.Value);
			}
		}

		// SqlDecimal to SqlBoolean
		public static explicit operator SqlBoolean (SqlDecimal x) 
		{
			checked {
				if (x.IsNull)
					return Null;
				else
					return new SqlBoolean ((int)x.Value);
			}
		}
		
		// SqlDouble to SqlBoolean
		public static explicit operator SqlBoolean (SqlDouble x) 
		{
			// FIXME
			//checked {
				if (x.IsNull)
					return Null;
				else
					return new SqlBoolean ((int)x.Value);
				//}
		}

		// SqlInt16 to SqlBoolean
		public static explicit operator SqlBoolean (SqlInt16 x) 
		{
			checked {
				if (x.IsNull)
					return Null;
				else
					return new SqlBoolean ((int)x.Value);
			}
		}

		// SqlInt32 to SqlBoolean
		public static explicit operator SqlBoolean (SqlInt32 x) 
		{
			checked {
				if (x.IsNull)
					return Null;
				else
					return new SqlBoolean (x.Value);
			}
		}

		// SqlInt64 to SqlBoolean
		public static explicit operator SqlBoolean (SqlInt64 x) 
		{
			checked {
				if (x.IsNull)
					return Null;
				else
					return new SqlBoolean ((int)x.Value);
			}
		}

		// SqlMoney to SqlBoolean
		public static explicit operator SqlBoolean (SqlMoney x) 
		{
			checked {
				if (x.IsNull)
					return Null;
				else
					return new SqlBoolean ((int)x.Value);
			}
		}

		// SqlSingle to SqlBoolean
		public static explicit operator SqlBoolean (SqlSingle x) 
		{
			// FIXME
			//checked {
				if (x.IsNull)
					return Null;
				else
					return new SqlBoolean ((int)x.Value);
				//}
		}

		// SqlString to SqlBoolean
		public static explicit operator SqlBoolean (SqlString x) 
		{
			checked {
				if (x.IsNull)
					return Null;
				return SqlBoolean.Parse (x.Value);
			}
		}

		// Boolean to SqlBoolean
		public static implicit operator SqlBoolean (bool x) 
		{
			return new SqlBoolean (x);
		}
#if NET_2_0
		// Helper method to Compare methods and operators.
		// Returns 0 if x == y
		//         1 if x > y
		//        -1 if x < y
		private static int Compare (SqlBoolean x, SqlBoolean y)
		{
			if (x == y)
				return 0;
			if (x.IsTrue && y.IsFalse)
				return 1;
			if (x.IsFalse && y.IsTrue)
				return -1;
			return 0;
		}

		public static XmlQualifiedName GetXsdType (XmlSchemaSet schemaSet)
		{
			XmlQualifiedName qualifiedName = new XmlQualifiedName ("boolean", "http://www.w3.org/2001/XMLSchema");
			return qualifiedName;
		}
		
		[MonoTODO]
		XmlSchema IXmlSerializable.GetSchema ()
		{
			throw new NotImplementedException ();
		}
                                                                                
		[MonoTODO]
		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			throw new NotImplementedException ();
		}
                                                                                
		[MonoTODO]
		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
