//
// System.Xml.Serialization.XmlCustomFormatter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;
using System.Text;
using System.Xml;

namespace System.Xml.Serialization {
	internal class XmlCustomFormatter {

		#region Methods

		internal static string FromByteArrayBase64 (byte[] value)
		{
			return Convert.ToBase64String(value);
		}

		internal static string FromByteArrayHex (byte[] value)
		{
			StringBuilder output = new StringBuilder ();
			foreach (byte val in value)
				output.Append (val.ToString ("X2"));
			return output.ToString ();
		}

		internal static string FromChar (char value)
		{
			return ((int) value).ToString ();
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
			return value.ToString ();
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

		internal static string ToXmlString (object value)
		{
			if (value == null) return null;
			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.Boolean: return XmlConvert.ToString ((bool)value);
				case TypeCode.Byte: return XmlConvert.ToString ((byte)value);
				case TypeCode.Char: return XmlConvert.ToString ((int)(char)value);
				case TypeCode.DateTime: return XmlConvert.ToString ((DateTime)value);
				case TypeCode.Decimal: return XmlConvert.ToString ((decimal)value);
				case TypeCode.Double: return XmlConvert.ToString ((double)value);
				case TypeCode.Int16: return XmlConvert.ToString ((Int16)value);
				case TypeCode.Int32: return XmlConvert.ToString ((Int32)value);
				case TypeCode.Int64: return XmlConvert.ToString ((Int64)value);
				case TypeCode.SByte: return XmlConvert.ToString ((sbyte)value);
				case TypeCode.Single: return XmlConvert.ToString ((Single)value);
				case TypeCode.UInt16: return XmlConvert.ToString ((UInt16)value);
				case TypeCode.UInt32: return XmlConvert.ToString ((UInt32)value);
				case TypeCode.UInt64: return XmlConvert.ToString ((UInt64)value);
			}

			if (value.GetType() == typeof(TimeSpan)) return XmlConvert.ToString ((TimeSpan)value);
			else if (value.GetType() == typeof (Guid)) return XmlConvert.ToString ((Guid)value);
			else if (value.GetType() == typeof(byte[])) return Convert.ToBase64String ((byte[])value);

			return value.ToString ();
		}

		internal static object FromXmlString (Type type, string value)
		{
			if (value == null) return null;

			switch (Type.GetTypeCode (type))
			{
				case TypeCode.Boolean: return XmlConvert.ToBoolean (value);
				case TypeCode.Byte: return XmlConvert.ToByte (value);
				case TypeCode.Char: return (char)XmlConvert.ToInt32 (value);
				case TypeCode.DateTime: return XmlConvert.ToDateTime (value);
				case TypeCode.Decimal: return XmlConvert.ToDecimal (value);
				case TypeCode.Double: return XmlConvert.ToDouble (value);
				case TypeCode.Int16: return XmlConvert.ToInt16 (value);
				case TypeCode.Int32: return XmlConvert.ToInt32 (value);
				case TypeCode.Int64: return XmlConvert.ToInt64 (value);
				case TypeCode.SByte: return XmlConvert.ToSByte (value);
				case TypeCode.Single: return XmlConvert.ToSingle (value);
				case TypeCode.UInt16: return XmlConvert.ToUInt16 (value);
				case TypeCode.UInt32: return XmlConvert.ToUInt32 (value);
				case TypeCode.UInt64: return XmlConvert.ToUInt64 (value);
			}

			if (type == typeof (TimeSpan)) return XmlConvert.ToTimeSpan (value);
			else if (type == typeof(Guid)) return XmlConvert.ToGuid (value);
			else if (type == typeof(byte[])) return Convert.FromBase64String (value);
			else return Convert.ChangeType (value, type);
		}

		#endregion // Methods
	}
}
