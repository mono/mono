//
// System.Data.SqlTypes.SqlMoney
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
	public struct SqlMoney : INullable, IComparable
#if NET_2_0
				, IXmlSerializable
#endif
	{
		#region Fields

		private decimal value;
		private bool notNull;
		
		public static readonly SqlMoney MaxValue = new SqlMoney (922337203685477.5807m);
		public static readonly SqlMoney MinValue = new SqlMoney (-922337203685477.5808m);
		public static readonly SqlMoney Null;
		public static readonly SqlMoney Zero = new SqlMoney (0);

		private static readonly NumberFormatInfo MoneyFormat;

		#endregion

		#region Constructors

		static SqlMoney ()
		{
			MoneyFormat = (NumberFormatInfo) NumberFormatInfo.InvariantInfo.Clone ();
			MoneyFormat.NumberDecimalDigits = 4;
			MoneyFormat.NumberGroupSeparator = String.Empty;
		}

		public SqlMoney (decimal value)
		{
			if (value > 922337203685477.5807m || value < -922337203685477.5808m)
				throw new OverflowException ();
			this.value = Decimal.Round (value, 4);
			notNull = true;
		}

		public SqlMoney (double value) : this ((decimal) value)
		{			
		}

		public SqlMoney (int value) : this ((decimal) value)
		{
		}

		public SqlMoney (long value) : this ((decimal) value)
		{
		}

		#endregion

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public decimal Value {
			get {
				if (this.IsNull)
					throw new SqlNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static SqlMoney Add (SqlMoney x, SqlMoney y)
		{
			return (x + y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SqlMoney))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlMoney"));
			return CompareSqlMoney ((SqlMoney)value);
		}
		
		private int CompareSqlMoney (SqlMoney value)
		{
			if (value.IsNull)
				return 1;
			else
				return this.value.CompareTo (value.Value);
		}

#if NET_2_0
		public int CompareTo (SqlMoney value)
		{
			return CompareSqlMoney (value);
		}
#endif

		public static SqlMoney Divide (SqlMoney x, SqlMoney y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SqlMoney))
				return false;
			if (this.IsNull)
				return ((SqlMoney)value).IsNull;
			else if (((SqlMoney)value).IsNull)
				return false;
			else
				return (bool) (this == (SqlMoney)value);
		}

		public static SqlBoolean Equals (SqlMoney x, SqlMoney y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			return (int) value;
		}

		public static SqlBoolean GreaterThan (SqlMoney x, SqlMoney y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlMoney x, SqlMoney y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlMoney x, SqlMoney y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlMoney x, SqlMoney y)
		{
			return (x <= y);
		}

		public static SqlMoney Multiply (SqlMoney x, SqlMoney y)
		{
			return (x * y);
		}

		public static SqlBoolean NotEquals (SqlMoney x, SqlMoney y)
		{
			return (x != y);
		}

		public static SqlMoney Parse (string s)
		{
			decimal d = Decimal.Parse (s);

			if (d > SqlMoney.MaxValue.Value || d < SqlMoney.MinValue.Value) 
				throw new OverflowException ();
			
			return new SqlMoney (d);
		}

		public static SqlMoney Subtract (SqlMoney x, SqlMoney y)
		{
			return (x - y);
		}

		public decimal ToDecimal ()
		{
			return value;
		}

		public double ToDouble ()
		{
			return (double) value;
		}

		public int ToInt32 ()
		{
			return (int) Math.Round (value);
		}

		public long ToInt64 ()
		{
			return (long) Math.Round (value);
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

		public SqlInt64 ToSqlInt64 ()
		{
			return ((SqlInt64) this);
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
				return value.ToString ("N", MoneyFormat);
		}

		public static SqlMoney operator + (SqlMoney x, SqlMoney y)
		{
			checked {
				return new SqlMoney (x.Value + y.Value);
			}
		}

		public static SqlMoney operator / (SqlMoney x, SqlMoney y)
		{
			checked {
				return new SqlMoney (x.Value / y.Value);
			}
		}

		public static SqlBoolean operator == (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			return new SqlBoolean (x.Value <= y.Value);
		}

		public static SqlMoney operator * (SqlMoney x, SqlMoney y)
		{
			checked {
				return new SqlMoney (x.Value * y.Value);
			}
		}

		public static SqlMoney operator - (SqlMoney x, SqlMoney y)
		{
			checked {
				return new SqlMoney (x.Value - y.Value);
			}
		}

		public static SqlMoney operator - (SqlMoney x)
		{
			return new SqlMoney (-(x.Value));
		}

		public static explicit operator SqlMoney (SqlBoolean x)
		{
			if (x.IsNull)
				return Null;
			else {
				checked {
					return new SqlMoney ((decimal) x.ByteValue);
				}
			}
		}

		public static explicit operator SqlMoney (SqlDecimal x)
		{
			if (x.IsNull)
				return Null;
			else {
				checked {
					return new SqlMoney (x.Value);
				}
			}
		}

		public static explicit operator SqlMoney (SqlDouble x)
		{
			if (x.IsNull)
				return Null;
			else {
				checked {
					return new SqlMoney ((decimal) x.Value);
				}
			}
		}

		public static explicit operator decimal (SqlMoney x)
		{
			return x.Value;
		}

		public static explicit operator SqlMoney (SqlSingle x)
		{
			if (x.IsNull)
				return Null;
			else {
				checked {
					return new SqlMoney ((decimal) x.Value);
				}
			}
		}

		public static explicit operator SqlMoney (SqlString x)
		{
			checked {
				return SqlMoney.Parse (x.Value);
			}
		}

#if NET_2_0
		public static explicit operator SqlMoney (double x)
		{
			return new SqlMoney (x);
		}

		public static implicit operator SqlMoney (long x)
		{
			return new SqlMoney (x);
		}
#endif

		public static implicit operator SqlMoney (decimal x)
		{
			return new SqlMoney (x);
		}

		public static implicit operator SqlMoney (SqlByte x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlMoney ((decimal) x.Value);
		}

		public static implicit operator SqlMoney (SqlInt16 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlMoney ((decimal) x.Value);
		}

		public static implicit operator SqlMoney (SqlInt32 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlMoney (x.Value);
		}

		public static implicit operator SqlMoney (SqlInt64 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlMoney (x.Value);
		}

#if NET_2_0
		public static XmlQualifiedName GetXsdType (XmlSchemaSet schemaSet)
		{
			XmlQualifiedName qualifiedName = new XmlQualifiedName ("decimal", "http://www.w3.org/2001/XMLSchema");
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
