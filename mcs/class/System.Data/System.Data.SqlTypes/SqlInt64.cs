//
// System.Data.SqlTypes.SqlInt64
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//   Ville Palo <vi64pa@koti.soon.fi>
//
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
#if NET_2_0
	[SerializableAttribute]
	[XmlSchemaProvider ("GetXsdType")]
#endif
	public struct SqlInt64 : INullable, IComparable
#if NET_2_0
				, IXmlSerializable
#endif
	{
		#region Fields

		private long value;
		private bool notNull;
		
		public static readonly SqlInt64 MaxValue = new SqlInt64 (9223372036854775807);
		public static readonly SqlInt64 MinValue = new SqlInt64 (-9223372036854775808);

		public static readonly SqlInt64 Null;
		public static readonly SqlInt64 Zero = new SqlInt64 (0);

		#endregion

		#region Constructors

		public SqlInt64 (long value)
		{
			this.value = value;
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public long Value {
			get {
				if (this.IsNull) 
					throw new SqlNullValueException ();
				else 
					return value;
			}
		}

		#endregion

		#region Methods

		public static SqlInt64 Add (SqlInt64 x, SqlInt64 y)
		{
			return (x + y);
		}

		public static SqlInt64 BitwiseAnd (SqlInt64 x, SqlInt64 y)
		{
			return (x & y);
		}

		public static SqlInt64 BitwiseOr (SqlInt64 x, SqlInt64 y)
		{
			return (x | y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SqlInt64))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlInt64"));
			return CompareSqlInt64 ((SqlInt64) value);
		}

#if NET_2_0
		public int CompareTo (SqlInt64 value)
		{
			return CompareSqlInt64 ((SqlInt64) value);
		}
#endif
	
		private int CompareSqlInt64 (SqlInt64 value)
		{
			if (value.IsNull)
				return 1;
			else
				return this.value.CompareTo (value.Value);
		}

		public static SqlInt64 Divide (SqlInt64 x, SqlInt64 y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SqlInt64))
				return false;
			else if (this.IsNull)
				return ((SqlInt64)value).IsNull;
			else if (((SqlInt64)value).IsNull)
				return false;
			else
				return (bool) (this == (SqlInt64)value);
		}

		public static SqlBoolean Equals (SqlInt64 x, SqlInt64 y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			return (int)(value & 0xffffffff) ^ (int)(value >> 32);
		}

		public static SqlBoolean GreaterThan (SqlInt64 x, SqlInt64 y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlInt64 x, SqlInt64 y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlInt64 x, SqlInt64 y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlInt64 x, SqlInt64 y)
		{
			return (x <= y);
		}

		public static SqlInt64 Mod (SqlInt64 x, SqlInt64 y)
		{
			return (x % y);
		}

#if NET_2_0
		public static SqlInt64 Modulus (SqlInt64 x, SqlInt64 y)
		{
			return (x % y);
		}
#endif

		public static SqlInt64 Multiply (SqlInt64 x, SqlInt64 y)
		{
			return (x * y);
		}

		public static SqlBoolean NotEquals (SqlInt64 x, SqlInt64 y)
		{
			return (x != y);
		}

		public static SqlInt64 OnesComplement (SqlInt64 x)
		{
			if (x.IsNull)
				return Null;

			return ~x;
		}


		public static SqlInt64 Parse (string s)
		{
			checked {
				return new SqlInt64 (Int64.Parse (s));
			}
		}

		public static SqlInt64 Subtract (SqlInt64 x, SqlInt64 y)
		{
			return (x - y);
		}

		public SqlBoolean ToSqlBoolean ()
		{
			return ((SqlBoolean) this);
		}
		
		public SqlByte ToSqlByte ()
		{
			return ((SqlByte) this);
		}

		public SqlDecimal ToSqlDecimal ()
		{
			return ((SqlDecimal) this);
		}

		public SqlDouble ToSqlDouble ()
		{
			return ((SqlDouble) this);
		}

		public SqlInt16 ToSqlInt16 ()
		{
			return ((SqlInt16) this);
		}

		public SqlInt32 ToSqlInt32 ()
		{
			return ((SqlInt32) this);
		}

		public SqlMoney ToSqlMoney ()
		{
			return ((SqlMoney) this);
		}

		public SqlSingle ToSqlSingle ()
		{
			return ((SqlSingle) this);
		}

		public SqlString ToSqlString ()
		{
			return ((SqlString) this);
		}

		public override string ToString ()
		{
			if (this.IsNull)
				return "Null";

			return value.ToString ();
		}

		public static SqlInt64 Xor (SqlInt64 x, SqlInt64 y)
		{
			return (x ^ y);
		}

		public static SqlInt64 operator + (SqlInt64 x, SqlInt64 y)
		{
			checked {
				return new SqlInt64 (x.Value + y.Value);
			}
		}

		public static SqlInt64 operator & (SqlInt64 x, SqlInt64 y)
		{
			return new SqlInt64 (x.value & y.Value);
		}

		public static SqlInt64 operator | (SqlInt64 x, SqlInt64 y)
		{
			return new SqlInt64 (x.value | y.Value);
		}

		public static SqlInt64 operator / (SqlInt64 x, SqlInt64 y)
		{
			checked {
				return new SqlInt64 (x.Value / y.Value);
			}
		}

		public static SqlBoolean operator == (SqlInt64 x, SqlInt64 y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlInt64 operator ^ (SqlInt64 x, SqlInt64 y)
		{
			return new SqlInt64 (x.Value ^ y.Value);
		}

		public static SqlBoolean operator > (SqlInt64 x, SqlInt64 y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlInt64 x, SqlInt64 y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlInt64 x, SqlInt64 y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlInt64 x, SqlInt64 y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlInt64 x, SqlInt64 y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value <= y.Value);
		}

		public static SqlInt64 operator % (SqlInt64 x, SqlInt64 y)
		{
			return new SqlInt64 (x.Value % y.Value);
		}

		public static SqlInt64 operator * (SqlInt64 x, SqlInt64 y)
		{
			checked {
				return new SqlInt64 (x.Value * y.Value);
			}
		}

		public static SqlInt64 operator ~ (SqlInt64 x)
		{
			if (x.IsNull)
				return SqlInt64.Null;

			return new SqlInt64 (~(x.Value));
		}

		public static SqlInt64 operator - (SqlInt64 x, SqlInt64 y)
		{
			checked {
				return new SqlInt64 (x.Value - y.Value);
			}
		}

		public static SqlInt64 operator - (SqlInt64 x)
		{
			return new SqlInt64 (-(x.Value));
		}

		public static explicit operator SqlInt64 (SqlBoolean x)
		{
			if (x.IsNull)
				return SqlInt64.Null;
			else
				return new SqlInt64 ((long) x.ByteValue);
		}

		public static explicit operator SqlInt64 (SqlDecimal x)
		{
			checked {
				if (x.IsNull)
					return SqlInt64.Null;
				else
					return new SqlInt64 ((long) x.Value);
			}
		}

		public static explicit operator SqlInt64 (SqlDouble x)
		{
			if (x.IsNull)
				return SqlInt64.Null;
			else {
				checked {
					return new SqlInt64 ((long) x.Value);
				}
			}
		}

		public static explicit operator long (SqlInt64 x)
		{
			return x.Value;
		}

		public static explicit operator SqlInt64 (SqlMoney x)
		{
			checked {
				if (x.IsNull)
					return SqlInt64.Null;
				else
					return new SqlInt64 ((long) Math.Round (x.Value));
			}
		}

		public static explicit operator SqlInt64 (SqlSingle x)
		{
			if (x.IsNull)
				return SqlInt64.Null;
			else {
				checked {
					return new SqlInt64 ((long) x.Value);
				}
			}
		}

		public static explicit operator SqlInt64 (SqlString x)
		{
			checked {
				return SqlInt64.Parse (x.Value);
			}
		}

		public static implicit operator SqlInt64 (long x)
		{
			return new SqlInt64 (x);
		}

		public static implicit operator SqlInt64 (SqlByte x)
		{
			if (x.IsNull)
				return SqlInt64.Null;
			else
				return new SqlInt64 ((long) x.Value);
		}

		public static implicit operator SqlInt64 (SqlInt16 x)
		{
			if (x.IsNull)
				return SqlInt64.Null;
			else
				return new SqlInt64 ((long) x.Value);
		}

		public static implicit operator SqlInt64 (SqlInt32 x)
		{
			if (x.IsNull)
				return SqlInt64.Null;
			else
				return new SqlInt64 ((long) x.Value);
		}

#if NET_2_0
		public static XmlQualifiedName GetXsdType (XmlSchemaSet schemaSet)
		{
			if (schemaSet != null && schemaSet.Count == 0) {
				XmlSchema xs = new XmlSchema ();
				XmlSchemaComplexType ct = new XmlSchemaComplexType ();
				ct.Name = "long";
				xs.Items.Add (ct);
				schemaSet.Add (xs);
			}
			return new XmlQualifiedName ("long", "http://www.w3.org/2001/XMLSchema");
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
			// or do I need to validate for the <SqlInt64> tag?
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
				this.value = Int64.Parse (reader.Value);
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
