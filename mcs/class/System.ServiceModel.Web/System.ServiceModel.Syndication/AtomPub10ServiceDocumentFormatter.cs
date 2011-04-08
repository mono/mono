//
// AtomPub10ServiceDocumentFormatter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.ServiceModel.Syndication
{
	class Namespaces
	{
		public const string Xml = "http://www.w3.org/XML/1998/namespace";
		public const string Xmlns = "http://www.w3.org/2000/xmlns/";
		public const string AtomPP = "http://www.w3.org/2007/app";
		public const string Atom10 = "http://www.w3.org/2005/Atom";
	}

	[XmlRoot ("service", Namespace = Namespaces.AtomPP)]
	public class AtomPub10ServiceDocumentFormatter : ServiceDocumentFormatter, IXmlSerializable
	{
		public AtomPub10ServiceDocumentFormatter ()
		{
		}

		public AtomPub10ServiceDocumentFormatter (ServiceDocument documentToWrite)
			: base (documentToWrite)
		{
		}

		public AtomPub10ServiceDocumentFormatter (Type documentTypeToCreate)
		{
			doc_type = documentTypeToCreate;
		}

		Type doc_type;

		public override string Version {
			get { return Namespaces.AtomPP; }
		}

		public override bool CanRead (XmlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			reader.MoveToContent ();
			return reader.LocalName == "service" && reader.NamespaceURI == Version;
		}

		protected override ServiceDocument CreateDocumentInstance ()
		{
			var doc = doc_type != null ? (ServiceDocument) Activator.CreateInstance (doc_type, new object [0]) : base.CreateDocumentInstance ();
			doc.InternalFormatter = this;
			return doc;
		}

		public override void ReadFrom (XmlReader reader)
		{
			if (!Document.TryParseElement (reader, Version))
				throw new XmlException (String.Format ("Unexpected element '{0}' in namespace '{1}'", reader.LocalName, reader.NamespaceURI));
		}

		public override void WriteTo (XmlWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");

			writer.WriteStartElement ("app", "service", Version);

			WriteContentTo (writer);

			writer.WriteEndElement ();
		}
		
		void WriteContentTo (XmlWriter writer)
		{
			if (writer.LookupPrefix (Namespaces.Atom10) == null)
				writer.WriteAttributeString ("xmlns", "a10", Namespaces.Xmlns, Namespaces.Atom10);
			if (writer.LookupPrefix (Version) == null)
				writer.WriteAttributeString ("xmlns", "app", Namespaces.Xmlns, Version);

			// xml:lang, xml:base, workspace*
			if (Document.Language != null)
				writer.WriteAttributeString ("xml", "lang", Namespaces.Xml, Document.Language);
			if (Document.BaseUri != null)
				writer.WriteAttributeString ("xml", "base", Namespaces.Xml, Document.BaseUri.ToString ());

			Document.WriteAttributeExtensions (writer, Version);
			Document.WriteElementExtensions (writer, Version);

			foreach (var ws in Document.Workspaces) {
				writer.WriteStartElement ("app", "workspace", Version);

				// xml:base, title, collection*
				if (ws.BaseUri != null)
					writer.WriteAttributeString ("xml", "base", Namespaces.Xml, ws.BaseUri.ToString ());

				ws.WriteAttributeExtensions (writer, Version);
				ws.WriteElementExtensions (writer, Version);

				if (ws.Title != null)
					ws.Title.WriteTo (writer, "title", Namespaces.Atom10);
				foreach (var rc in ws.Collections) {
					writer.WriteStartElement ("app", "collection", Version);

					// accept*, xml:base, category, @href, title
					if (rc.BaseUri != null)
						writer.WriteAttributeString ("xml", "base", Namespaces.Xml, rc.BaseUri.ToString ());
					if (rc.Link != null)
						writer.WriteAttributeString ("href", rc.Link.ToString ());

					rc.WriteAttributeExtensions (writer, Version);

					if (rc.Title != null)
						rc.Title.WriteTo (writer, "title", Namespaces.Atom10);
					foreach (var s in rc.Accepts) {
						writer.WriteStartElement ("app", "accept", Version);
						writer.WriteString (s);
						writer.WriteEndElement ();
					}
					foreach (var cat in rc.Categories)
						cat.Save (writer);

					writer.WriteEndElement ();
				}
				writer.WriteEndElement ();
			}
		}

		XmlSchema IXmlSerializable.GetSchema ()
		{
			return null;
		}

		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			ReadFrom (reader);
		}

		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			WriteContentTo (writer);
		}
	}
}
