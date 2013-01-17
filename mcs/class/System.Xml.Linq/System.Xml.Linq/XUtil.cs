//
// Authors:
//   Atsushi Enomoto
//
// Copyright 2007 Novell (http://www.novell.com)
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using XPI = System.Xml.Linq.XProcessingInstruction;

namespace System.Xml.Linq
{
	internal static class XUtil
	{
		public const string XmlnsNamespace =
			"http://www.w3.org/2000/xmlns/";

		public static bool ConvertToBoolean (string s)
		{
			return XmlConvert.ToBoolean (s.ToLower (CultureInfo.InvariantCulture));
		}

		public static DateTime ToDateTime (string s)
		{
			try {
				return XmlConvert.ToDateTime (s, XmlDateTimeSerializationMode.RoundtripKind);
			} catch {
				return DateTime.Parse (s);
			}
		}

		public static string ToString (object o)
		{
			if (o == null)
				throw new InvalidOperationException ("Attempt to get string from null");

			switch (Type.GetTypeCode (o.GetType ())) {
			case TypeCode.String:
				return (string) o;
			case TypeCode.DateTime:
				return XmlConvert.ToString ((DateTime) o, XmlDateTimeSerializationMode.RoundtripKind);
			case TypeCode.Decimal:
				return ((decimal) o).ToString (CultureInfo.InvariantCulture);
			case TypeCode.Double:
				return ((double) o).ToString ("r", CultureInfo.InvariantCulture);
			case TypeCode.Single:
				return ((float) o).ToString ("r", CultureInfo.InvariantCulture);
			case TypeCode.Boolean:
				// Valid XML values are `true' and `false', not `True' and `False' that boolean returns
				return o.ToString().ToLower();
			default:
				if (o is TimeSpan)
					return XmlConvert.ToString ((TimeSpan) o);
				if (o is DateTimeOffset)
					return XmlConvert.ToString ((DateTimeOffset) o);
				return o.ToString ();
			}
		}

		public static bool ToBoolean (object o)
		{
			throw new NotImplementedException ();
		}

		public static Nullable <bool> ToNullableBoolean (object o)
		{
			throw new NotImplementedException ();
		}

		public static IEnumerable ExpandArray (object o)
		{
			XNode n = o as XNode;
			if (n != null)
				yield return n;
			else if (o is string)
				yield return o;
			else if (o is IEnumerable)
				foreach (object obj in (IEnumerable) o)
					foreach (object oo in ExpandArray (obj))
						yield return oo;
			else
				yield return o;
		}

		public static XNode ToNode (object o)
		{
			if (o is XAttribute)
				throw new ArgumentException ("Attribute node is not allowed as argument");
			XNode n = o as XNode;
			if (n != null)
				return n;
			else if (o is string)
				return new XText ((string) o);
			else
				return new XText (ToString (o));
		}

		public static object GetDetachedObject (XObject child)
		{
			return child.Owner != null ? Clone (child) : child;
		}

		public static object Clone (object o)
		{
			if (o is string)
				return (string) o;
			if (o is XAttribute)
				return new XAttribute ((XAttribute) o);
			if (o is XElement)
				return new XElement ((XElement) o);
			if (o is XCData)
				return new XCData ((XCData) o);
			if (o is XComment)
				return new XComment ((XComment) o);
			if (o is XPI)
				return new XPI ((XPI) o);
			if (o is XDeclaration)
				return new XDeclaration ((XDeclaration) o);
			if (o is XDocumentType)
				return new XDocumentType ((XDocumentType) o);
			if (o is XText)
				return new XText ((XText) o);
			throw new ArgumentException ();
		}
	}
}
