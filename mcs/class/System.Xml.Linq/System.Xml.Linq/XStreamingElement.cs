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
using System.IO;
using System.Collections.Generic;

namespace System.Xml.Linq
{
	public class XStreamingElement
	{
		public XStreamingElement (XName name)
		{
			Name = name;
		}

		public XStreamingElement (XName name, object content)
			: this (name)
		{
			Add (content);
		}

		public XStreamingElement (XName name, params object [] content)
			: this (name)
		{
			Add (content);
		}

		XName name;
		List<object> contents;

		public XName Name {
			get { return name; }
			set { name = value; }
		}

		internal IEnumerable<object> Contents {
			get { return contents; }
		}

		public void Add (object content)
		{
			if (contents == null)
				contents = new List<object> ();
			contents.Add (content);
		}

		public void Add (params object [] content)
		{
			if (contents == null)
				contents = new List<object> ();
			contents.Add (content);
		}

		public void Save (string fileName)
		{
			using (TextWriter w = File.CreateText (fileName))
				Save (w, SaveOptions.None);
		}

		public void Save (TextWriter textWriter)
		{
			Save (textWriter, SaveOptions.None);
		}

		public void Save (XmlWriter writer)
		{
			WriteTo (writer);
		}

		public void Save (string fileName, SaveOptions options)
		{
			using (TextWriter w = File.CreateText (fileName))
				Save (w, options);
		}

		public void Save (TextWriter textWriter, SaveOptions options)
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			
			if ((options & SaveOptions.DisableFormatting) == SaveOptions.None)
				s.Indent = true;
#if NET_4_0
			if ((options & SaveOptions.OmitDuplicateNamespaces) == SaveOptions.OmitDuplicateNamespaces)
				s.NamespaceHandling |= NamespaceHandling.OmitDuplicates;
#endif
			using (XmlWriter w = XmlWriter.Create (textWriter, s))
				WriteTo (w);
		}

#if NET_4_0
		public void Save (Stream stream)
		{
			Save (stream, SaveOptions.None);
		}

		public void Save (Stream stream, SaveOptions options)
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			if ((options & SaveOptions.DisableFormatting) == SaveOptions.None)
				s.Indent = true;
			if ((options & SaveOptions.OmitDuplicateNamespaces) == SaveOptions.OmitDuplicateNamespaces)
				s.NamespaceHandling |= NamespaceHandling.OmitDuplicates;
			
			using (var writer = XmlWriter.Create (stream, s)){
				WriteTo (writer);
			}
		}
#endif

		public override string ToString ()
		{
			return ToString (SaveOptions.None);
		}

		public string ToString (SaveOptions options)
		{
			StringWriter sw = new StringWriter ();
			Save (sw, options);
			return sw.ToString ();
		}

		public void WriteTo (XmlWriter writer)
		{
			writer.WriteStartElement (name.LocalName, name.Namespace.NamespaceName);
			WriteContents (contents, writer);
			writer.WriteEndElement ();
		}

		void WriteContents (IEnumerable<object> items, XmlWriter w)
		{
			foreach (object o in XUtil.ExpandArray (items)) {
				if (o == null)
					continue;
				else if (o is XStreamingElement)
					((XStreamingElement) o).WriteTo (w);
				else if (o is XNode)
					((XNode) o).WriteTo (w);
				else if (o is object [])
					WriteContents ((object []) o, w);
				else if (o is XAttribute)
					WriteAttribute ((XAttribute) o, w);
				else
					new XText (o.ToString ()).WriteTo (w);
			}
		}

		void WriteAttribute (XAttribute a, XmlWriter w)
		{
			if (a.IsNamespaceDeclaration) {
				if (a.Name.Namespace == XNamespace.Xmlns)
					w.WriteAttributeString ("xmlns", a.Name.LocalName, XNamespace.Xmlns.NamespaceName, a.Value);
				else
					w.WriteAttributeString ("xmlns", a.Value);
			}
			else
				w.WriteAttributeString (a.Name.LocalName, a.Name.Namespace.NamespaceName, a.Value);
		}
	}
}
