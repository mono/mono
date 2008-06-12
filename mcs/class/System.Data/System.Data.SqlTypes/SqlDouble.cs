//
// System.Data.SqlTypes.SqlDouble
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
	public struct SqlDouble : INullable, IComparable
#if NET_2_0
				, IXmlSerializable
#endif
	{
		#region Fields

		private double value;
		private bool notNull;

		public static readonly SqlDouble MaxValue = new SqlDouble (1.7976931348623157E+308);
		public static readonly SqlDouble MinValue = new SqlDouble (-1.7976931348623157E+308);
		public static readonly SqlDouble Null;
		public static readonly SqlDouble Zero = new SqlDouble (0);

		#endregion

		#region Constructors

		public SqlDouble (double value)
		{
			this.value = value;
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public double Value {
			get {
				if (this.IsNull)
					throw new SqlNullValueException ();
				else 
					return value;
			}
		}

		#endregion

		#region Methods

		public static SqlDouble Add (SqlDouble x, SqlDouble y)
		{
			return (x + y);
		}
		
		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			if (!(value is SqlDouble))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlDouble"));

			return CompareTo ((SqlDouble) value);
		}

#if NET_2_0
		public
#endif
		int CompareTo (SqlDouble value)
		{
			if (value.IsNull)
				return 1;
			else
				return this.value.CompareTo (value.Value);
		}

		public static SqlDouble Divide (SqlDouble x, SqlDouble y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SqlDouble))
				return false;
			if (this.IsNull)
				return ((SqlDouble)value).IsNull;
			else if (((SqlDouble)value).IsNull)
				return false;
			else
				return (bool) (this == (SqlDouble)value);
		}

		public static SqlBoolean Equals (SqlDouble x, SqlDouble y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			long LongValue = (long)value;
			return (int)(LongValue ^ (LongValue >> 32));
		}

		public static SqlBoolean GreaterThan (SqlDouble x, SqlDouble y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlDouble x, SqlDouble y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlDouble x, SqlDouble y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlDouble x, SqlDouble y)
		{
			return (x <= y);
		}

		public static SqlDouble Multiply (SqlDouble x, SqlDouble y)
		{
			return (x * y);
		}

		public static SqlBoolean NotEquals (SqlDouble x, SqlDouble y)
		{
			return (x != y);
		}

		public static SqlDouble Parse (string s)
		{
			return new SqlDouble (Double.Parse (s));
		}

		public static SqlDouble Subtract (SqlDouble x, SqlDouble y)
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

		public SqlInt16 ToSqlInt16 ()
		{
			return ((SqlInt16) this);
		}

		public SqlInt32 ToSqlInt32 ()
		{
			return ((SqlInt32) this);
		}

		public SqlInt64 ToSqlInt64 ()
		{
			return ((SqlInt64) this);
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
			if (!notNull)
				return "Null";
			else
				return value.ToString ();
		}

		public static SqlDouble operator + (SqlDouble x, SqlDouble y)
		{
			double d = 0;
			d = x.Value + y.Value;
			
			if (Double.IsInfinity (d))
				throw new OverflowException ();

			return new SqlDouble (d);
		}

		public static SqlDouble operator / (SqlDouble x, SqlDouble y)
		{
			double d = x.Value / y.Value;

			if (Double.IsInfinity (d)) {
				if (y.Value == 0)
					throw new DivideByZeroException ();
			}
				
			return new SqlDouble (d);
		}

		public static SqlBoolean operator == (SqlDouble x, SqlDouble y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlDouble x, SqlDouble y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlDouble x, SqlDouble y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlDouble x, SqlDouble y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlDouble x, SqlDouble y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlDouble x, SqlDouble y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value <= y.Value);
		}

		public static SqlDouble operator * (SqlDouble x, SqlDouble y)
		{
			double d = x.Value * y.Value;
			
			if (Double.IsInfinity (d))
				throw new OverflowException ();

			return new SqlDouble (d);

		}

		public static SqlDouble operator - (SqlDouble x, SqlDouble y)
		{
			double d = x.Value - y.Value;
			
			if (Double.IsInfinity (d))
				throw new OverflowException ();

			return new SqlDouble (d);
		}

		public static SqlDouble operator - (SqlDouble x)
		{
			return new SqlDouble (-(x.Value));
		}

		public static explicit operator SqlDouble (SqlBoolean x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlDouble ((double)x.ByteValue);
		}

		public static explicit operator double (SqlDouble x)
		{
			return x.Value;
		}

		public static explicit operator SqlDouble (SqlString x)
		{
			checked {
				return SqlDouble.Parse (x.Value);
			}
		}

		public static implicit operator SqlDouble (double x)
		{
			return new SqlDouble (x);
		}

		public static implicit operator SqlDouble (SqlByte x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlDouble ((double) x.Value);
		}

		public static implicit operator SqlDouble (SqlDecimal x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlDouble (x.ToDouble ());
		}

		public static implicit operator SqlDouble (SqlInt16 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlDouble ((double) x.Value);
		}

		public static implicit operator SqlDouble (SqlInt32 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlDouble ((double) x.Value);
		}

		public static implicit operator SqlDouble (SqlInt64 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlDouble ((double) x.Value);
		}

		public static implicit operator SqlDouble (SqlMoney x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlDouble ((double) x.Value);
		}

		public static implicit operator SqlDouble (SqlSingle x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlDouble ((double) x.Value);
		}

#if NET_2_0
		public static XmlQualifiedName GetXsdType (XmlSchemaSet schemaSet)
		{
			if (schemaSet != null && schemaSet.Count == 0) {
				XmlSchema xs = new XmlSchema ();
				XmlSchemaComplexType ct = new XmlSchemaComplexType ();
				ct.Name = "double";
				xs.Items.Add (ct);
				schemaSet.Add (xs);
			}
			return new XmlQualifiedName ("double", "http://www.w3.org/2001/XMLSchema");
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
				// FIXME: do we need to handle the FormatException?
				this.value = Double.Parse (reader.Value);
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
