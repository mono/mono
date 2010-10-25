//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace System.Xaml
{
	public static class XamlServices
	{
		public static Object Load (string fileName)
		{
			using (var xr = XmlReader.Create (fileName))
				return Load (xr);
		}

		public static Object Load (Stream stream)
		{
			return Load (new XamlXmlReader (stream));
		}

		public static Object Load (TextReader textReader)
		{
			return Load (new XamlXmlReader (textReader));
		}

		public static Object Load (XmlReader xmlReader)
		{
			return Load (new XamlXmlReader (xmlReader));
		}

		public static Object Load (XamlReader xamlReader)
		{
			if (xamlReader == null)
				throw new ArgumentNullException ("xamlReader");
			var w = new XamlObjectWriter (xamlReader.SchemaContext);
			Transform (xamlReader, w);
			return w.Result;
		}

		public static Object Parse (string xaml)
		{
			return Load (new StringReader (xaml));
		}

		public static string Save (object instance)
		{
			var sw = new StringWriter ();
			Save (sw, instance);
			return sw.ToString ();
		}

		public static void Save (string fileName, object instance)
		{
			using (var xw = XmlWriter.Create (fileName, new XmlWriterSettings () { OmitXmlDeclaration = true }))
				Save (xw, instance);
		}

		public static void Save (Stream stream, object instance)
		{
			using (var xw = XmlWriter.Create (stream, new XmlWriterSettings () { OmitXmlDeclaration = true }))
				Save (xw, instance);
		}

		public static void Save (TextWriter textWriter, object instance)
		{
			using (var xw = XmlWriter.Create (textWriter, new XmlWriterSettings () { OmitXmlDeclaration = true }))
				Save (xw, instance);
		}

		public static void Save (XmlWriter xmlWriter, object instance)
		{
			Save (new XamlXmlWriter (xmlWriter, new XamlSchemaContext ()), instance);
		}

		public static void Save (XamlWriter xamlWriter, object instance)
		{
			if (xamlWriter == null)
				throw new ArgumentNullException ("xamlWriter");
			var r = new XamlObjectReader (instance, xamlWriter.SchemaContext);
			Transform (r, xamlWriter);
		}

		public static void Transform (XamlReader xamlReader, XamlWriter xamlWriter)
		{
			Transform (xamlReader, xamlWriter, true);
		}

		public static void Transform (XamlReader xamlReader, XamlWriter xamlWriter, bool closeWriter)
		{
			if (xamlReader == null)
				throw new ArgumentNullException ("xamlReader");
			if (xamlWriter == null)
				throw new ArgumentNullException ("xamlWriter");

			if (xamlReader.NodeType == XamlNodeType.None)
				xamlReader.Read ();

			while (!xamlReader.IsEof) {
				xamlWriter.WriteNode (xamlReader);
				xamlReader.Read ();
			}
			if (closeWriter)
				xamlWriter.Close ();
		}
	}
}
