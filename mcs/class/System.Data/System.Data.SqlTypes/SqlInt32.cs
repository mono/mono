//
// System.Data.SqlTypes.SqlInt32
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
using System.Xml.Schema;
using System.Globalization;
using System.Xml.Serialization;

namespace System.Data.SqlTypes
{

	/// <summary>
	/// a 32-bit signed integer to be used in reading or writing
	/// of data from a database
	/// </summary>
#if NET_2_0
	[SerializableAttribute]
	[XmlSchemaProvider ("GetXsdType")]
#endif
	public struct SqlInt32 : INullable, IComparable 
#if NET_2_0
				, IXmlSerializable
#endif
	{
		#region Fields

		int value;
		private bool notNull;

		public static readonly SqlInt32 MaxValue = new SqlInt32 (2147483647);
		public static readonly SqlInt32 MinValue = new SqlInt32 (-2147483648);
		public static readonly SqlInt32 Null;
		public static readonly SqlInt32 Zero = new SqlInt32 (0);

		#endregion

		#region Constructors

		public SqlInt32(int value) 
		{
			this.value = value;
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public int Value {
			get { 
				if (this.IsNull) 
					throw new SqlNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static SqlInt32 Add (SqlInt32 x, SqlInt32 y) 
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

		public int CompareTo(object value) 
		{
			if (value == null)
				return 1;
			else if (!(value is SqlInt32))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlInt32"));
			return CompareSqlInt32 ((SqlInt32) value);
		}

		#if NET_2_0
		public int CompareTo (SqlInt32 value)
		{
			return CompareSqlInt32 (value);
		}
		#endif
	
		private int CompareSqlInt32 (SqlInt32 value)
		{
			if (value.IsNull)
				return 1;
			else
				return this.value.CompareTo (value.Value);
		}

		public static SqlInt32 Divide(SqlInt32 x, SqlInt32 y) 
		{
			return (x / y);
		}

		public override bool Equals(object value) 
		{
			if (!(value is SqlInt32))
				return false;
			else if (this.IsNull)
				return ((SqlInt32)value).IsNull;
			else if (((SqlInt32)value).IsNull)
				return false;
			else
				return (bool) (this == (SqlInt32)value);
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

		#if NET_2_0
		public static SqlInt32 Modulus (SqlInt32 x, SqlInt32 y)
		{
			return (x % y);
		}
		#endif

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
			return new SqlInt32 (Int32.Parse (s));
		}

		public static SqlInt32 Subtract(SqlInt32 x, SqlInt32 y) 
		{
			return (x - y);
		}

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

		public SqlString ToSqlString ()
		{
			return ((SqlString)this);
		}

		public override string ToString() 
		{
			if (this.IsNull)
				return "Null";
			else
				return value.ToString ();
		}

		public static SqlInt32 Xor(SqlInt32 x, SqlInt32 y) 
		{
			return (x ^ y);
		}

		#endregion

		#region Operators

		// Compute Addition
		public static SqlInt32 operator + (SqlInt32 x, SqlInt32 y) 
		{
			checked {
				return new SqlInt32 (x.Value + y.Value);
			}
		}

		// Bitwise AND
		public static SqlInt32 operator & (SqlInt32 x, SqlInt32 y) 
		{
			return new SqlInt32 (x.Value & y.Value);
		}

		// Bitwise OR
		public static SqlInt32 operator | (SqlInt32 x, SqlInt32 y) 
		{
			checked {
				return new SqlInt32 (x.Value | y.Value);
			}
		}

		// Compute Division
		public static SqlInt32 operator / (SqlInt32 x, SqlInt32 y) 
		{
			checked {
				return new SqlInt32 (x.Value / y.Value);
			}
		}

		// Compare Equality
		public static SqlBoolean operator == (SqlInt32 x, SqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value == y.Value);
		}

		// Bitwise Exclusive-OR (XOR)
		public static SqlInt32 operator ^ (SqlInt32 x, SqlInt32 y) 
		{
			return new SqlInt32 (x.Value ^ y.Value);
		}

		// > Compare
		public static SqlBoolean operator >(SqlInt32 x, SqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value > y.Value);
		}

		// >= Compare
		public static SqlBoolean operator >= (SqlInt32 x, SqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value >= y.Value);
		}

		// != Inequality Compare
		public static SqlBoolean operator != (SqlInt32 x, SqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value != y.Value);
		}
		
		// < Compare
		public static SqlBoolean operator < (SqlInt32 x, SqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value < y.Value);
		}

		// <= Compare
		public static SqlBoolean operator <= (SqlInt32 x, SqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value <= y.Value);
		}

		// Compute Modulus
		public static SqlInt32 operator % (SqlInt32 x, SqlInt32 y) 
		{
			return new SqlInt32 (x.Value % y.Value);
		}

		// Compute Multiplication
		public static SqlInt32 operator * (SqlInt32 x, SqlInt32 y) 
		{
			checked {
				return new SqlInt32 (x.Value * y.Value);
			}
		}

		// Ones Complement
		public static SqlInt32 operator ~ (SqlInt32 x) 
		{
			return new SqlInt32 (~x.Value);
		}

		// Subtraction
		public static SqlInt32 operator - (SqlInt32 x, SqlInt32 y) 
		{
			checked {
				return new SqlInt32 (x.Value - y.Value);
			}
		}

		// Negates the Value
		public static SqlInt32 operator - (SqlInt32 x) 
		{
			return new SqlInt32 (-x.Value);
		}

		// Type Conversions
		public static explicit operator SqlInt32 (SqlBoolean x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new SqlInt32 ((int)x.ByteValue);
		}

		public static explicit operator SqlInt32 (SqlDecimal x) 
		{
			checked {
				if (x.IsNull) 
					return Null;
				else 
					return new SqlInt32 ((int)x.Value);
			}
		}

		public static explicit operator SqlInt32 (SqlDouble x) 
		{
			checked {
				if (x.IsNull) 
					return Null;
				else 
					return new SqlInt32 ((int)x.Value);
			}
		}

		public static explicit operator int (SqlInt32 x)
		{
			return x.Value;
		}

		public static explicit operator SqlInt32 (SqlInt64 x) 
		{
			checked {
				if (x.IsNull) 
					return Null;
				else 
					return new SqlInt32 ((int)x.Value);
			}
		}

		public static explicit operator SqlInt32(SqlMoney x) 
		{
			checked {
				if (x.IsNull) 
					return Null;
				else 
					return new SqlInt32 ((int) Math.Round (x.Value));
			}
		}

		public static explicit operator SqlInt32(SqlSingle x) 
		{
			checked {
				if (x.IsNull) 
					return Null;
				else 
					return new SqlInt32 ((int)x.Value);
			}
		}

		public static explicit operator SqlInt32(SqlString x) 
		{
			checked {
				return SqlInt32.Parse (x.Value);
			}
		}

		public static implicit operator SqlInt32(int x) 
		{
			return new SqlInt32 (x);
		}

		public static implicit operator SqlInt32(SqlByte x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new SqlInt32 ((int)x.Value);
		}

		public static implicit operator SqlInt32(SqlInt16 x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new SqlInt32 ((int)x.Value);
		}

#if NET_2_0
		public static XmlQualifiedName GetXsdType (XmlSchemaSet schemaSet)
		{
			if (schemaSet != null && schemaSet.Count == 0) {
				XmlSchema xs = new XmlSchema ();
				XmlSchemaComplexType ct = new XmlSchemaComplexType ();
				ct.Name = "int";
				xs.Items.Add (ct);
				schemaSet.Add (xs);
			}
			return new XmlQualifiedName ("int", "http://www.w3.org/2001/XMLSchema");
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
			// or do I need to validate for the <SqlInt32> tag?
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
				this.value = Int32.Parse (reader.Value);
				this.notNull = true;
			}
		}
		
		void IXmlSerializable.WriteXml (XmlWriter writer) 
		{
			writer.WriteString (this.ToString ());
		}
#endif
		#endregion
	}
}
