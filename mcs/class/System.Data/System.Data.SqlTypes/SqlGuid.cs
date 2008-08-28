//
// System.Data.SqlTypes.SqlGuid
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
	public struct SqlGuid : INullable, IComparable
#if NET_2_0
				, IXmlSerializable
#endif
	{
		#region Fields

	        Guid value;

		private bool notNull;

		public static readonly SqlGuid Null;

		#endregion

		#region Constructors

		public SqlGuid (byte[] value) 
		{
			this.value = new Guid (value);
			notNull = true;
		}

		public SqlGuid (Guid g) 
		{
			this.value = g;
			notNull = true;
		}

		public SqlGuid (string s) 
		{
			this.value = new Guid (s);
			notNull = true;
		}

		public SqlGuid (int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k) 
		{
			this.value = new Guid (a, b, c, d, e, f, g, h, i, j, k);
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public Guid Value { 
			get { 
				if (this.IsNull) 
					throw new SqlNullValueException ("The property contains Null.");
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			if (!(value is SqlGuid))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlGuid"));

			return CompareTo ((SqlGuid) value);
		}
#if NET_2_0
		public
#endif
		int CompareTo (SqlGuid value)
		{
			if (value.IsNull)
				return 1;
			else
                                // LAMESPEC : ms.net implementation actually compares all the 16 bytes.
                                // This code is kept for future changes, if required.
                                /*
				{
					//MSDN documentation says that CompareTo is different from 
					//Guid's CompareTo. It uses the SQL Server behavior where
                                        //only the last 6 bytes of value are evaluated	
					byte[] compareValue = ((SqlGuid)value).GetLastSixBytes();
					byte[] currentValue = GetLastSixBytes();
					for (int i = 0; i < 6; i++)
					{
						if (currentValue[i] != compareValue[i]) {
			                              return Compare(currentValue[i], compareValue[i]);
                        			}
					}
			                return 0;
				}
                                */
                                return this.value.CompareTo (value.Value);
				
		}

		public override bool Equals (object value)
		{
			if (!(value is SqlGuid))
				return false;
			else if (this.IsNull)
				return ((SqlGuid)value).IsNull;
			else if (((SqlGuid)value).IsNull)
				return false;
			else
				return (bool) (this == (SqlGuid)value);
		}

		public static SqlBoolean Equals (SqlGuid x, SqlGuid y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			byte [] bytes  = this.ToByteArray ();
			
			int result = 10;
			foreach (byte b in  bytes) {
				result = 91 * result + b.GetHashCode ();
			}

			return result;
		}

		public static SqlBoolean GreaterThan (SqlGuid x, SqlGuid y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlGuid x, SqlGuid y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlGuid x, SqlGuid y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlGuid x, SqlGuid y)
		{
			return (x <= y);
		}

		public static SqlBoolean NotEquals (SqlGuid x, SqlGuid y)
		{
			return (x != y);
		}

		public static SqlGuid Parse (string s)
		{
			return new SqlGuid (s);
		}

		public byte[] ToByteArray()
		{
			return value.ToByteArray ();
		}

		public SqlBinary ToSqlBinary ()
		{
			return ((SqlBinary)this);
		}

		public SqlString ToSqlString ()
		{
			return ((SqlString)this);
		}

		public override string ToString ()
		{
			if (!notNull)
				return "Null";
			else
				return value.ToString ();
		}

		public static SqlBoolean operator == (SqlGuid x, SqlGuid y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlGuid x, SqlGuid y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;

			if (x.Value.CompareTo (y.Value) > 0)
				return new SqlBoolean (true);
			else
				return new SqlBoolean (false);
		}

		public static SqlBoolean operator >= (SqlGuid x, SqlGuid y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			
			if (x.Value.CompareTo (y.Value) >= 0)
				return new SqlBoolean (true);
			else
				return new SqlBoolean (false);

		}

		public static SqlBoolean operator != (SqlGuid x, SqlGuid y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlGuid x, SqlGuid y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;

			if (x.Value.CompareTo (y.Value) < 0)
				return new SqlBoolean (true);
			else
				return new SqlBoolean (false);

		}

		public static SqlBoolean operator <= (SqlGuid x, SqlGuid y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;

			if (x.Value.CompareTo (y.Value) <= 0)
				return new SqlBoolean (true);
			else
				return new SqlBoolean (false);
		}

		public static explicit operator SqlGuid (SqlBinary x)
		{
			return new SqlGuid (x.Value);
		}

		public static explicit operator Guid (SqlGuid x)
		{
			return x.Value;
		}

		public static explicit operator SqlGuid (SqlString x)
		{
			return new SqlGuid (x.Value);
		}

		public static implicit operator SqlGuid (Guid x)
		{
			return new SqlGuid (x);
		}

#if NET_2_0
		public static XmlQualifiedName GetXsdType (XmlSchemaSet schemaSet)
		{
			XmlQualifiedName qualifiedName = new XmlQualifiedName ("string", "http://www.w3.org/2001/XMLSchema");
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
			
