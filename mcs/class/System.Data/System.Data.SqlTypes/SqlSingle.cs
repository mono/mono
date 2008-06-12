//
// System.Data.SqlTypes.SqlSingle
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
	public struct SqlSingle : INullable, IComparable
#if NET_2_0
				, IXmlSerializable
#endif
	{
		#region Fields

		private float value;
		private bool notNull;

		public static readonly SqlSingle MaxValue = new SqlSingle (3.40282346638528859E+38f);
		public static readonly SqlSingle MinValue = new SqlSingle (-3.40282346638528859E+38f);
		public static readonly SqlSingle Null;
		public static readonly SqlSingle Zero = new SqlSingle (0);

		#endregion

		#region Constructors

		public SqlSingle (double value)
		{
			this.value = (float)value;
			notNull = true;
		}

		public SqlSingle (float value)
		{
			this.value = value;
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public float Value {
			get {
				if (this.IsNull)
					throw new SqlNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static SqlSingle Add (SqlSingle x, SqlSingle y)
		{
			return (x + y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SqlSingle))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlSingle"));

			return CompareSqlSingle ((SqlSingle) value);
		}

#if NET_2_0
		public int CompareTo (SqlSingle value)
		{
			return CompareSqlSingle (value);
		}
#endif
		
		private int CompareSqlSingle (SqlSingle value)
		{
			if (value.IsNull)
				return 1;
			else
				return this.value.CompareTo (value.Value);
		}

		public static SqlSingle Divide (SqlSingle x, SqlSingle y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SqlSingle))
				return false;
			else if (this.IsNull)
				return ((SqlSingle)value).IsNull;
			else if (((SqlSingle)value).IsNull)
				return false;
			else
				return (bool) (this == (SqlSingle)value);
		}

		public static SqlBoolean Equals (SqlSingle x, SqlSingle y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			long LongValue = (long) value;
			return (int) (LongValue ^ (LongValue >> 32));
		}

		public static SqlBoolean GreaterThan (SqlSingle x, SqlSingle y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlSingle x, SqlSingle y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlSingle x, SqlSingle y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlSingle x, SqlSingle y)
		{
			return (x <= y);
		}

		public static SqlSingle Multiply (SqlSingle x, SqlSingle y)
		{
			return (x * y);
		}

		public static SqlBoolean NotEquals (SqlSingle x, SqlSingle y)
		{
			return (x != y);
		}

		public static SqlSingle Parse (string s)
		{
			return new SqlSingle (Single.Parse (s));
		}

		public static SqlSingle Subtract (SqlSingle x, SqlSingle y)
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

		public SqlInt64 ToSqlInt64 ()
		{
			return ((SqlInt64) this);
		}

		public SqlMoney ToSqlMoney ()
		{
			return ((SqlMoney) this);
		}

		public SqlString ToSqlString ()
		{
			return ((SqlString) this);
		}

		public override string ToString ()
		{
			if (!notNull)
				return "Null";
			return value.ToString ();
		}

		public static SqlSingle operator + (SqlSingle x, SqlSingle y)
		{
			float f = (float) (x.Value + y.Value);

			if (Single.IsInfinity (f))
				throw new OverflowException ();

			return new SqlSingle (f);
		}

		public static SqlSingle operator / (SqlSingle x, SqlSingle y)
		{
			float f = (float) (x.Value / y.Value);

			if (Single.IsInfinity (f)) {
				if (y.Value == 0d) 
					throw new DivideByZeroException ();
			}

			return new SqlSingle (x.Value / y.Value);
		}

		public static SqlBoolean operator == (SqlSingle x, SqlSingle y)
		{
			if (x.IsNull || y .IsNull)
				return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlSingle x, SqlSingle y)
		{
			if (x.IsNull || y .IsNull)
				return SqlBoolean.Null;
			return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlSingle x, SqlSingle y)
		{
			if (x.IsNull || y .IsNull)
				return SqlBoolean.Null;
			return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlSingle x, SqlSingle y)
		{
			if (x.IsNull || y .IsNull)
				return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlSingle x, SqlSingle y)
		{
			if (x.IsNull || y .IsNull)
				return SqlBoolean.Null;
			return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlSingle x, SqlSingle y)
		{
			if (x.IsNull || y .IsNull)
				return SqlBoolean.Null;
			return new SqlBoolean (x.Value <= y.Value);
		}

		public static SqlSingle operator * (SqlSingle x, SqlSingle y)
		{
			float f = (float) (x.Value * y.Value);
			
			if (Single.IsInfinity (f))
				throw new OverflowException ();

			return new SqlSingle (f);
		}

		public static SqlSingle operator - (SqlSingle x, SqlSingle y)
		{
			float f = (float) (x.Value - y.Value);

			if (Single.IsInfinity (f))
				throw new OverflowException ();

			return new SqlSingle (f);
		}

		public static SqlSingle operator - (SqlSingle x)
		{
			return new SqlSingle (-(x.Value));
		}

		public static explicit operator SqlSingle (SqlBoolean x)
		{
			checked {
				if (x.IsNull)
					return Null;
				
				return new SqlSingle((float)x.ByteValue);
			}
		}

		public static explicit operator SqlSingle (SqlDouble x)
		{
			if (x.IsNull)
				return Null;

			float f = (float)x.Value;

			if (Single.IsInfinity (f))
				throw new OverflowException ();

			return new SqlSingle(f);
		}

		public static explicit operator float (SqlSingle x)
		{
			return x.Value;
		}

		public static explicit operator SqlSingle (SqlString x)
		{
			checked {
				if (x.IsNull)
					return Null;
				
				return SqlSingle.Parse (x.Value);
			}
		}

		public static implicit operator SqlSingle (float x)
		{
			return new SqlSingle (x);
		}

		public static implicit operator SqlSingle (SqlByte x)
		{
			if (x.IsNull)
				return Null;
			else 
				return new SqlSingle((float) x.Value);
		}

		public static implicit operator SqlSingle (SqlDecimal x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlSingle((float) x.Value);
		}

		public static implicit operator SqlSingle (SqlInt16 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlSingle((float) x.Value);
		}

		public static implicit operator SqlSingle (SqlInt32 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlSingle((float) x.Value);
		}

		public static implicit operator SqlSingle (SqlInt64 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlSingle((float) x.Value);
		}

		public static implicit operator SqlSingle (SqlMoney x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlSingle((float) x.Value);
		}

#if NET_2_0
		public static XmlQualifiedName GetXsdType (XmlSchemaSet schemaSet)
		{
			XmlQualifiedName qualifiedName = new XmlQualifiedName ("float", "http://www.w3.org/2001/XMLSchema");
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
