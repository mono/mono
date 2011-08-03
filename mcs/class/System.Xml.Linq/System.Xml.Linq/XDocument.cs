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
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Xml.Linq
{
	public class XDocument : XContainer
	{
		XDeclaration xmldecl;

		public XDocument ()
		{
		}

		public XDocument (params object [] content)
		{
			Add (content);
		}

		public XDocument (XDeclaration xmldecl, params object [] content)
		{
			Declaration = xmldecl;
			Add (content);
		}

		public XDocument (XDocument other)
		{
			foreach (object o in other.Nodes ())
				Add (XUtil.Clone (o));
		}

		public XDeclaration Declaration {
			get { return xmldecl; }
			set { xmldecl = value; }
		}

		public XDocumentType DocumentType {
			get {
				foreach (object o in Nodes ())
					if (o is XDocumentType)
						return (XDocumentType) o;
				return null;
			}
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Document; }
		}

		public XElement Root {
			get {
				foreach (object o in Nodes ())
					if (o is XElement)
						return (XElement) o;
				return null;
			}
		}

		public static XDocument Load (string uri)
		{
			return Load (uri, LoadOptions.None);
		}

		public static XDocument Load (string uri, LoadOptions options)
		{
			XmlReaderSettings s = new XmlReaderSettings ();
#if !MOONLIGHT
			s.ProhibitDtd = false; // see XNodeNavigatorTest.MoveToId().
#endif
			s.IgnoreWhitespace = (options & LoadOptions.PreserveWhitespace) == 0;
			using (XmlReader r = XmlReader.Create (uri, s)) {
				return LoadCore (r, options);
			}
		}

		public static XDocument Load (Stream stream)
		{
			return Load (new StreamReader (stream), LoadOptions.None);
		}

		public static XDocument Load (Stream stream, LoadOptions options)
		{
			return Load (new StreamReader (stream), options);
		}

		public static XDocument Load (TextReader reader)
		{
			return Load (reader, LoadOptions.None);
		}

		public static XDocument Load (TextReader reader, LoadOptions options)
		{
			XmlReaderSettings s = new XmlReaderSettings ();
#if !MOONLIGHT
			s.ProhibitDtd = false; // see XNodeNavigatorTest.MoveToId().
#endif
			s.IgnoreWhitespace = (options & LoadOptions.PreserveWhitespace) == 0;
			using (XmlReader r = XmlReader.Create (reader, s)) {
				return LoadCore (r, options);
			}
		}

		public static XDocument Load (XmlReader reader)
		{
			return Load (reader, LoadOptions.None);
		}

		public static XDocument Load (XmlReader reader, LoadOptions options)
		{
			XmlReaderSettings s = reader.Settings != null ? reader.Settings.Clone () : new XmlReaderSettings ();
			s.IgnoreWhitespace = (options & LoadOptions.PreserveWhitespace) == 0;
			using (XmlReader r = XmlReader.Create (reader, s)) {
				return LoadCore (r, options);
			}
		}

		static XDocument LoadCore (XmlReader reader, LoadOptions options)
		{
			XDocument doc = new XDocument ();
			doc.ReadContent (reader, options);
			return doc;
		}

		void ReadContent (XmlReader reader, LoadOptions options)
		{
			if (reader.ReadState == ReadState.Initial)
				reader.Read ();
			if (reader.NodeType == XmlNodeType.XmlDeclaration) {
				Declaration = new XDeclaration (
					reader.GetAttribute ("version"),
					reader.GetAttribute ("encoding"),
					reader.GetAttribute ("standalone"));
				reader.Read ();
			}
			ReadContentFrom (reader, options);
			if (Root == null)
				throw new InvalidOperationException ("The document element is missing.");
		}

		static void ValidateWhitespace (string s)
		{
			for (int i = 0; i < s.Length; i++)
				switch (s [i]) {
				case ' ': case '\t': case '\n': case '\r':
					continue;
				default:
					throw new ArgumentException ("Non-whitespace text appears directly in the document.");
				}
		}

		public static XDocument Parse (string s)
		{
			return Parse (s, LoadOptions.None);
		}

		public static XDocument Parse (string s, LoadOptions options)
		{
			return Load (new StringReader (s), options);
		}

		public void Save (string filename)
		{
			Save (filename, SaveOptions.None);
		}

		public void Save (string filename, SaveOptions options)
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			if ((options & SaveOptions.DisableFormatting) == SaveOptions.None)
				s.Indent = true;
#if NET_4_0
			if ((options & SaveOptions.OmitDuplicateNamespaces) == SaveOptions.OmitDuplicateNamespaces)
				s.NamespaceHandling |= NamespaceHandling.OmitDuplicates;
#endif
			
			using (XmlWriter w = XmlWriter.Create (filename, s)) {
				Save (w);
			}
		}

		public void Save (TextWriter tw)
		{
			Save (tw, SaveOptions.None);
		}

		public void Save (TextWriter tw, SaveOptions options)
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			if ((options & SaveOptions.DisableFormatting) == SaveOptions.None)
				s.Indent = true;
#if NET_4_0
			if ((options & SaveOptions.OmitDuplicateNamespaces) == SaveOptions.OmitDuplicateNamespaces)
				s.NamespaceHandling |= NamespaceHandling.OmitDuplicates;
#endif
			using (XmlWriter w = XmlWriter.Create (tw, s)) {
				Save (w);
			}
		}

		public void Save (XmlWriter w)
		{
			WriteTo (w);
		}

		public override void WriteTo (XmlWriter w)
		{
			if (xmldecl != null) {
				if (xmldecl.Standalone != null)
					w.WriteStartDocument (xmldecl.Standalone == "yes");
				else
					w.WriteStartDocument ();
			}
			foreach (XNode node in Nodes ())
				node.WriteTo (w);
		}

		internal override bool OnAddingObject (object obj, bool rejectAttribute, XNode refNode, bool addFirst)
		{
			VerifyAddedNode (obj, addFirst);
			return false;
		}

		void VerifyAddedNode (object node, bool addFirst)
		{
			if (node == null)
				throw new InvalidOperationException ("Only a node is allowed here");

			if (node is string)
				ValidateWhitespace ((string) node);
			if (node is XText)
				ValidateWhitespace (((XText) node).Value);
			else if (node is XDocumentType) {
				if (DocumentType != null)
					throw new InvalidOperationException ("There already is another document type declaration");
				if (Root != null && !addFirst)
					throw new InvalidOperationException ("A document type cannot be added after the document element");
			}
			else if (node is XElement) {
				if (Root != null)
					throw new InvalidOperationException ("There already is another document element");
				if (DocumentType != null && addFirst)
					throw new InvalidOperationException ("An element cannot be added before the document type declaration");
			}
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
				Save (writer);
			}
		}

#endif
	}
}
