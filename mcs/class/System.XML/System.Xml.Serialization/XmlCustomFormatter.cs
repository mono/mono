//
// System.Xml.Serialization.XmlCustomFormatter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

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
using System.Collections;
using System.Text;
using System.Xml;
using System.Globalization;

namespace System.Xml.Serialization {
	internal class XmlCustomFormatter {

		#region Methods

		internal static string FromByteArrayBase64 (byte[] value)
		{
			return Convert.ToBase64String(value);
		}

		internal static string FromByteArrayHex (byte[] value)
		{
			if (value == null) return null;
			StringBuilder output = new StringBuilder ();
			foreach (byte val in value)
				output.Append (val.ToString ("X2", CultureInfo.InvariantCulture));
			return output.ToString ();
		}

		internal static string FromChar (char value)
		{
			return ((int) value).ToString (CultureInfo.InvariantCulture);
		}

		internal static string FromDate (DateTime value)
		{
			return XmlConvert.ToString (value, "yyyy-MM-dd");
		}

		internal static string FromDateTime (DateTime value)
		{
			return XmlConvert.ToString (value, "yyyy-MM-ddTHH:mm:ss.fffffffzzz");
		}

		internal static string FromTime (DateTime value)
		{
			return XmlConvert.ToString (value, "HH:mm:ss.fffffffzzz");
		}

		internal static string FromEnum (long value, string[] values, long[] ids)
		{
			int length = ids.Length;

			for (int i = 0; i < length; i ++) {
				if (ids[i] == value) 
					if (i >= values.Length)
						return String.Empty;
					else
						return values[i].ToString ();
			}
			return String.Empty;
		}

		internal static string FromXmlName (string name)
		{
			return XmlConvert.EncodeName (name);
		}

		internal static string FromXmlNCName (string ncName)
		{
			return XmlConvert.EncodeLocalName (ncName);
		}

		internal static string FromXmlNmToken (string nmToken)
		{
			return XmlConvert.EncodeNmToken (nmToken);
		}

		internal static string FromXmlNmTokens (string nmTokens)
		{
			string [] tokens = nmTokens.Split (' ');
			for (int i=0; i<tokens.Length; i++)
				tokens [i] = FromXmlNmToken (tokens [i]);
			return String.Join (" ", tokens);
		}

		internal static byte[] ToByteArrayBase64 (string value)
		{
			return Convert.FromBase64String(value);
		}

		internal static char ToChar (string value)
		{
			return (char) XmlConvert.ToUInt16 (value);
		}

		internal static DateTime ToDate (string value)
		{
			return ToDateTime (value);
		}

		internal static DateTime ToDateTime (string value)
		{
			return XmlConvert.ToDateTime (value);
		}

		internal static DateTime ToTime (string value)
		{
			return ToDateTime (value);
		}

		internal static long ToEnum (string value, Hashtable values, string typeName, bool validate)
		{
			// Assuming that h contains map from value to Enumerated Name
/*
			You can try : 
				return ToEnum ("Element", h, "XmlNodeType");
			where:
				(1) no keys and values for h.
				(2) string keys and Enum, Type, long, string value.
*/
			string memberName = (string) values [value];
			if (memberName == null)
				throw new InvalidOperationException (String.Format ("{0} is not a valid member of type {1}", value, typeName));

			return (long) Enum.Parse (Type.GetType (typeName), memberName);
		}

		internal static string ToXmlName (string value)
		{
			return XmlConvert.DecodeName (value);
		}

		internal static string ToXmlNCName (string value)
		{
			return ToXmlName (value);
		}

		internal static string ToXmlNmToken (string value)
		{
			return ToXmlName (value);
		}

		internal static string ToXmlNmTokens (string value)
		{
			return ToXmlName (value);
		}

		internal static string ToXmlString (TypeData type, object value)
		{
			if (value == null) return null;
			switch (type.XmlType)
			{
				case "boolean": return XmlConvert.ToString ((bool)value);
				case "unsignedByte": return XmlConvert.ToString ((byte)value);
				case "char": return XmlConvert.ToString ((int)(char)value);
				case "dateTime": return XmlConvert.ToString ((DateTime)value);
				case "date": return ((DateTime)value).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
				case "time": return ((DateTime)value).ToString("HH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture);
				case "decimal": return XmlConvert.ToString ((decimal)value);
				case "double": return XmlConvert.ToString ((double)value);
				case "short": return XmlConvert.ToString ((Int16)value);
				case "int": return XmlConvert.ToString ((Int32)value);
				case "long": return XmlConvert.ToString ((Int64)value);
				case "byte": return XmlConvert.ToString ((sbyte)value);
				case "float": return XmlConvert.ToString ((Single)value);
				case "unsignedShort": return XmlConvert.ToString ((UInt16)value);
				case "unsignedInt": return XmlConvert.ToString ((UInt32)value);
				case "unsignedLong": return XmlConvert.ToString ((UInt64)value);
				case "guid": return XmlConvert.ToString ((Guid)value);
				case "base64Binary": return Convert.ToBase64String ((byte[])value);
			default: return value is IFormattable ? ((IFormattable) value).ToString (null, CultureInfo.InvariantCulture) : value.ToString ();
			}
		}

		internal static object FromXmlString (TypeData type, string value)
		{
			if (value == null) return null;

			switch (type.XmlType)
			{
				case "boolean": return XmlConvert.ToBoolean (value);
				case "unsignedByte": return XmlConvert.ToByte (value);
				case "char": return (char)XmlConvert.ToInt32 (value);
				case "dateTime": return XmlConvert.ToDateTime (value);
				case "date": return DateTime.ParseExact (value, "yyyy-MM-dd", null);
				case "time": return DateTime.ParseExact (value, "HH:mm:ss.fffffffzzz", null);
				case "decimal": return XmlConvert.ToDecimal (value);
				case "double": return XmlConvert.ToDouble (value);
				case "short": return XmlConvert.ToInt16 (value);
				case "int": return XmlConvert.ToInt32 (value);
				case "long": return XmlConvert.ToInt64 (value);
				case "byte": return XmlConvert.ToSByte (value);
				case "float": return XmlConvert.ToSingle (value);
				case "unsignedShort": return XmlConvert.ToUInt16 (value);
				case "unsignedInt": return XmlConvert.ToUInt32 (value);
				case "unsignedLong": return XmlConvert.ToUInt64 (value);
				case "guid": return XmlConvert.ToGuid (value);
				case "base64Binary": return Convert.FromBase64String (value);
				default: 
					if (type.Type != null)
						return Convert.ChangeType (value, type.Type);
					else
						return value;
			}
		}

		internal static string GenerateToXmlString (TypeData type, string value)
		{
			switch (type.XmlType)
			{
				case "boolean": return "(" + value + "?\"true\":\"false\")";
				case "unsignedByte": return value + ".ToString(CultureInfo.InvariantCulture)";
				case "char": return "((int)(" + value + ")).ToString(CultureInfo.InvariantCulture)";
				case "dateTime": return value + ".ToString(\"yyyy-MM-ddTHH:mm:ss.fffffffzzz\", CultureInfo.InvariantCulture)";
				case "date": return value + ".ToString(\"yyyy-MM-dd\", CultureInfo.InvariantCulture)";
				case "time": return value + ".ToString(\"HH:mm:ss.fffffffzzz\", CultureInfo.InvariantCulture)";
				case "decimal": return "XmlConvert.ToString (" + value + ")";
				case "double": return "XmlConvert.ToString (" + value + ")";
				case "short": return value + ".ToString(CultureInfo.InvariantCulture)";
				case "int": return value + ".ToString(CultureInfo.InvariantCulture)";
				case "long": return value + ".ToString(CultureInfo.InvariantCulture)";
				case "byte": return value + ".ToString(CultureInfo.InvariantCulture)";
				case "float": return "XmlConvert.ToString (" + value + ")";
				case "unsignedShort": return value + ".ToString(CultureInfo.InvariantCulture)";
				case "unsignedInt": return value + ".ToString(CultureInfo.InvariantCulture)";
				case "unsignedLong": return value + ".ToString(CultureInfo.InvariantCulture)";
				case "guid": return "XmlConvert.ToString (" + value + ")";
				case "base64Binary": return "Convert.ToBase64String (" + value + ")";
				case "NMTOKEN":
				case "Name":
				case "NCName":
				case "language":
				case "ENTITY":
				case "ID":
				case "IDREF":
				case "NOTATION":
				case "token":
				case "normalizedString":
				case "string": return value;
				default: return "((" + value + " != null) ? (" + value + ").ToString() : null)";
			}
		}

		internal static string GenerateFromXmlString (TypeData type, string value)
		{
			switch (type.XmlType)
			{
				case "boolean": return "XmlConvert.ToBoolean (" + value + ")";
				case "unsignedByte": return "byte.Parse (" + value + ", CultureInfo.InvariantCulture)";
				case "char": return "(char)Int32.Parse (" + value + ", CultureInfo.InvariantCulture)";
				case "dateTime": return "XmlConvert.ToDateTime (" + value + ")";
				case "date": return "DateTime.ParseExact (" + value + ", \"yyyy-MM-dd\", CultureInfo.InvariantCulture)";
				case "time": return "DateTime.ParseExact (" + value + ", \"HH:mm:ss.fffffffzzz\", CultureInfo.InvariantCulture)";
				case "decimal": return "Decimal.Parse (" + value + ", CultureInfo.InvariantCulture)";
				case "double": return "XmlConvert.ToDouble (" + value + ")";
				case "short": return "Int16.Parse (" + value + ", CultureInfo.InvariantCulture)";
				case "int": return "Int32.Parse (" + value + ", CultureInfo.InvariantCulture)";
				case "long": return "Int64.Parse (" + value + ", CultureInfo.InvariantCulture)";
				case "byte": return "SByte.Parse (" + value + ", CultureInfo.InvariantCulture)";
				case "float": return "XmlConvert.ToSingle (" + value + ", CultureInfo.InvariantCulture)";
				case "unsignedShort": return "UInt16.Parse (" + value + ", CultureInfo.InvariantCulture)";
				case "unsignedInt": return "UInt32.Parse (" + value + ", CultureInfo.InvariantCulture)";
				case "unsignedLong": return "UInt64.Parse (" + value + ", CultureInfo.InvariantCulture)";
				case "guid": return "XmlConvert.ToGuid (" + value + ")";
				case "base64Binary": return "Convert.FromBase64String (" + value + ")";
				default: return value;
			}
		}

		#endregion // Methods
	}
}
