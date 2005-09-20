#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using XPI = System.Xml.XLinq.XProcessingInstruction;

namespace System.Xml.XLinq
{
	internal static class XUtil
	{
		public const string XmlnsNamespace =
			"http://www.w3.org/2000/xmlns/";

		// FIXME: implement
		public static string ToString (object o)
		{
			if (o is string)
				return (string) o;
			throw new NotImplementedException ();
		}

		public static bool ToBoolean (object o)
		{
			throw new NotImplementedException ();
		}

		public static Nullable <bool> ToNullableBoolean (object o)
		{
			throw new NotImplementedException ();
		}

		// FIXME: this method is not enough by design.
		public static XNode ToNode (object o)
		{
			XNode n = o as XNode;
			if (n != null)
				return n;
			if (o is string)
				return new XText ((string) o);
			if (o is IEnumerable)
				throw new NotImplementedException ();
			return new XText (o.ToString ());
		}

		public static object Clone (object o)

		{
			if (o is string)
				return (string) o;
			if (o is XElement)
				return new XElement ((XElement) o);
			if (o is XCData)
				return new XCData (((XCData) o).Value);
			if (o is XComment)
				return new XComment (((XComment) o).Value);
			XPI pi = o as XPI;
			if (pi != null)
				return new XPI (pi.Target, pi.Data);
			XDeclaration xd = o as XDeclaration;
			if (xd != null)
				return new XDeclaration (xd.Version, xd.Encoding, xd.Standalone);
			XDocumentType dtd = o as XDocumentType;
			if (dtd != null)
				throw new NotImplementedException ();
			throw new ArgumentException ();
		}
	}
}
#endif
