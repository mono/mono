//
// System.Xml.Serialization.XmlCustomFormatter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Text;
using System.Xml;

namespace System.Xml.Serialization {
	internal class XmlCustomFormatter {

		#region Methods

		[MonoTODO]
		internal static byte[] FromByteArrayBase64 (byte[] value)
		{
			throw new NotImplementedException ();
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
			return XmlConvert.ToString (value, "HH':'mm':'ss'.'fffffffzzz");
		}

		internal static string FromDateTime (DateTime value)
		{
			return XmlConvert.ToString (value, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzzz");
		}

		internal static string FromTime (DateTime value)
		{
			return XmlConvert.ToString (value, "yyyy'-'MM'-'dd");
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
			StringBuilder output = new StringBuilder ();
			string [] tokens = nmTokens.Split (' ');
			foreach (string token in tokens)
				output.Append (FromXmlNmToken (token));
			return output.ToString ();
		}

		[MonoTODO]
		internal static char ToChar (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal static DateTime ToDate (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal static DateTime ToDateTime (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal static DateTime ToTime (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal static string ToXmlName (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal static string ToXmlNCName (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal static string ToXmlNmToken (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal static string ToXmlNmTokens (string value)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
