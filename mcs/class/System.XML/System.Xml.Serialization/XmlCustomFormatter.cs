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

		public static string FromXmlName (string name)
		{
			return XmlConvert.EncodeName (name);
		}

		public static string FromXmlNCName (string ncName)
		{
			return XmlConvert.EncodeLocalName (ncName);
		}

		public static string FromXmlNmToken (string nmToken)
		{
			return XmlConvert.EncodeNmToken (nmToken);
		}

		public static string FromXmlNmTokens (string nmTokens)
		{
			StringBuilder output = new StringBuilder ();
			string [] tokens = nmTokens.Split (' ');
			foreach (string token in tokens)
				output.Append (FromXmlNmToken (token));
			return output.ToString ();
		}

		#endregion // Methods
	}
}
