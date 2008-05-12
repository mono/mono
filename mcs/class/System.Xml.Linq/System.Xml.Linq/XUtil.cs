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

		// FIXME: implement
		public static string ToString (object o)
		{
			if (o == null)
				throw new InvalidOperationException ("Attempt to get string from null");
			if (o is string)
				return (string) o;
			return o.ToString ();
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

		// FIXME: it will be removed. It makes attribute processing incorrect.
		public static IEnumerable<XNode> ToNodes (object o)
		{
			XNode n = o as XNode;
			if (n != null)
				yield return n;
			else if (o is string)
				yield return new XText ((string) o);
			else if (o is IEnumerable)
				foreach (object obj in (IEnumerable) o)
					foreach (XNode nn in ToNodes (obj))
						yield return nn;
			else
				yield return new XText (o.ToString ());
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

		// FIXME: it will be removed. Shrinking just arguments is insufficient.
		public static IEnumerable<object> ShrinkArray (params object [] content)
		{
			if (content == null || content.Length == 0)
				yield break;
			string prev = null;
			foreach (object o in content) {
				if (o is XNode) {
					if (prev != null) {
						yield return prev;
						prev = null;
					}
					yield return o;
				} else {
					prev += o;
				}
			}
			if (prev != null)
				yield return prev;
		}
	}
}
