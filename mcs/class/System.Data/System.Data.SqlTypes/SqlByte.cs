//
// System.Data.SqlTypes.SqlByte
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//   Ville Palo <vi64pa@kolumbus.fi>
//
// (C) Copyright 2002 Tim Coleman
// (C) Copyright 2003 Ville Palo
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
#if NET_2_0
	[SerializableAttribute]
	[XmlSchemaProvider ("GetXsdType")]
#endif
	public struct SqlByte : INullable, IComparable
#if NET_2_0
				   , IXmlSerializable
#endif
	{
		#region Fields

		byte value;
		private bool notNull;

		public static readonly SqlByte MaxValue = new SqlByte (0xff);
		public static readonly SqlByte MinValue = new SqlByte (0);
		public static readonly SqlByte Null;
		public static readonly SqlByte Zero = new SqlByte (0);

		#endregion

		#region Constructors

		public SqlByte (byte value) 
		{
			this.value = value;
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public byte Value { 
			get { 
				if (this.IsNull) 
					throw new SqlNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static SqlByte Add (SqlByte x, SqlByte y)
		{
			return (x + y);
		}

		public static SqlByte BitwiseAnd (SqlByte x, SqlByte y)
		{
			return (x & y);
		}

		public static SqlByte BitwiseOr (SqlByte x, SqlByte y)
		{
			return (x | y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			if (!(value is SqlByte))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlByte"));

			return CompareTo ((SqlByte) value);
		}
#if NET_2_0
		public
#endif
		int CompareTo (SqlByte value)
		{
			if (value.IsNull)
				return 1;
			else
				return this.value.CompareTo (value.Value);
		}

		public static SqlByte Divide (SqlByte x, SqlByte y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SqlByte))
				return false;
			else if (this.IsNull)
				return ((SqlByte)value).IsNull;
			else if (((SqlByte)value).IsNull)
				return false;
			else
				return (bool) (this == (SqlByte)value);
		}

		public static SqlBoolean Equals (SqlByte x, SqlByte y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			return (int)value;
		}

		public static SqlBoolean GreaterThan (SqlByte x, SqlByte y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlByte x, SqlByte y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlByte x, SqlByte y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlByte x, SqlByte y)
		{
			return (x <= y);
		}

		public static SqlByte Mod (SqlByte x, SqlByte y)
		{
			return (x % y);
		}

#if NET_2_0
		// Why did Microsoft add this method in 2.0???  What's 
		// the difference????
		public static SqlByte Modulus (SqlByte x, SqlByte y) 
		{
			return (x % y);
		}
#endif

		public static SqlByte Multiply (SqlByte x, SqlByte y)
		{
			return (x * y);
		}

		public static SqlBoolean NotEquals (SqlByte x, SqlByte y)
		{
			return (x != y);
		}

		public static SqlByte OnesComplement (SqlByte x)
		{
			return ~x;
		}

		public static SqlByte Parse (string s)
		{
			checked {
				return new SqlByte (Byte.Parse (s));
			}
		}

		public static SqlByte Subtract (SqlByte x, SqlByte y)
		{
			return (x - y);
		}

		public SqlBoolean ToSqlBoolean ()
		{
			return ((SqlBoolean)this);
		}
		
		public SqlDecimal ToSqlDecimal ()
		{
			return ((SqlDecimal)this);
		}

		public SqlDouble ToSqlDouble ()
		{
			return ((SqlDouble)this);
		}

		public SqlInt16 ToSqlInt16 ()
		{
			return ((SqlInt16)this);
		}

		public SqlInt32 ToSqlInt32 ()
		{
			return ((SqlInt32)this);
		}

		public SqlInt64 ToSqlInt64 ()
		{
			return ((SqlInt64)this);
		}

		public SqlMoney ToSqlMoney ()
		{
			return ((SqlMoney)this);
		}

		public SqlSingle ToSqlSingle ()
		{
			return ((SqlSingle)this);
		}

		public SqlString ToSqlString ()
		{
			return ((SqlString)this);
		}

		public override string ToString ()
		{
			if (this.IsNull)
				return "Null";
			else
				return value.ToString ();
		}

		public static SqlByte Xor (SqlByte x, SqlByte y)
		{
			return (x ^ y);
		}

		public static SqlByte operator + (SqlByte x, SqlByte y)
		{
			checked {
				return new SqlByte ((byte) (x.Value + y.Value));
			}
		}

		public static SqlByte operator & (SqlByte x, SqlByte y)
		{
			return new SqlByte ((byte) (x.Value & y.Value));
		}

		public static SqlByte operator | (SqlByte x, SqlByte y)
		{
			return new SqlByte ((byte) (x.Value | y.Value));
		}

		public static SqlByte operator / (SqlByte x, SqlByte y)
		{
			checked {
				return new SqlByte ((byte) (x.Value / y.Value));
			}
		}

		public static SqlBoolean operator == (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlByte operator ^ (SqlByte x, SqlByte y)
		{
			return new SqlByte ((byte) (x.Value ^ y.Value));
		}

		public static SqlBoolean operator > (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlByte x, SqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value <= y.Value);
		}

		public static SqlByte operator % (SqlByte x, SqlByte y)
		{
			return new SqlByte ((byte) (x.Value % y.Value));
		}

		public static SqlByte operator * (SqlByte x, SqlByte y)
		{
			checked {
				return new SqlByte ((byte) (x.Value * y.Value));
			}
		}

		public static SqlByte operator ~ (SqlByte x)
		{
			return new SqlByte ((byte) ~x.Value);
		}

		public static SqlByte operator - (SqlByte x, SqlByte y)
		{
			checked {
				return new SqlByte ((byte) (x.Value - y.Value));
			}
		}

		public static explicit operator SqlByte (SqlBoolean x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlByte (x.ByteValue);
		}

		public static explicit operator byte (SqlByte x)
		{
			return x.Value;
		}

		public static explicit operator SqlByte (SqlDecimal x)
		{
			checked {
				if (x.IsNull)
					return Null;
				else 
					return new SqlByte ((byte)x.Value);
			}
		}

		public static explicit operator SqlByte (SqlDouble x)
		{
			if (x.IsNull)
				return Null;
			else {
				checked {
					return new SqlByte ((byte)x.Value);
				}
			}
		}

		public static explicit operator SqlByte (SqlInt16 x)
		{
			checked {
				if (x.IsNull)
					return Null;
				else 			       
					return new SqlByte ((byte)x.Value);
			}
		}

		public static explicit operator SqlByte (SqlInt32 x)
		{
			checked {
				if (x.IsNull)
					return Null;
				else 			       
					return new SqlByte ((byte)x.Value);
			}
		}

		public static explicit operator SqlByte (SqlInt64 x)
		{
			if (x.IsNull)
				return Null;
			else {
				checked {
					return new SqlByte ((byte)x.Value);
				}
			}
		}

		public static explicit operator SqlByte (SqlMoney x)
		{
			checked {
				if (x.IsNull)
					return Null;
				else 					
					return new SqlByte ((byte)x.Value);
			}
		}

		public static explicit operator SqlByte (SqlSingle x)
		{
			if (x.IsNull)
				return Null;
			else {
				checked {
					return new SqlByte ((byte)x.Value);
				}
			}
		}


		public static explicit operator SqlByte (SqlString x)
		{			
			checked {
				return SqlByte.Parse (x.Value);
			}
		}

		public static implicit operator SqlByte (byte x)
		{
			return new SqlByte (x);
		}
		
#if NET_2_0
		public static XmlQualifiedName GetXsdType (XmlSchemaSet schemaSet)
		{
			XmlQualifiedName qualifiedName = new XmlQualifiedName ("unsignedByte", "http://www.w3.org/2001/XMLSchema");
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
		#endregion
	}
}
			
