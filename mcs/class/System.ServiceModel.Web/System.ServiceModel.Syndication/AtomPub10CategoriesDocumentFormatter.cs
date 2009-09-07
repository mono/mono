//
// AtomPub10CategoriesDocumentFormatter.cs
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
	[XmlRoot ("categories", Namespace = Namespaces.AtomPP)]
	public class AtomPub10CategoriesDocumentFormatter : CategoriesDocumentFormatter, IXmlSerializable
	{
		public AtomPub10CategoriesDocumentFormatter ()
			: this (typeof (InlineCategoriesDocument), typeof (ReferencedCategoriesDocument))
		{
		}

		public AtomPub10CategoriesDocumentFormatter (CategoriesDocument documentToWrite)
			: base (documentToWrite)
		{
		}

		public AtomPub10CategoriesDocumentFormatter (Type inlineDocumentType, Type referencedDocumentType)
		{
			if (inlineDocumentType == null)
				throw new ArgumentNullException ("inlineDocumentType");
			if (referencedDocumentType == null)
				throw new ArgumentNullException ("referencedDocumentType");

			inline_type = inlineDocumentType;
			ref_type = referencedDocumentType;
		}

		Type inline_type, ref_type;

		public override string Version {
			get { return "http://www.w3.org/2007/app"; }
		}

		public override bool CanRead (XmlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			reader.MoveToContent ();
			return reader.LocalName != "categories" || reader.NamespaceURI != Version;
		}

		protected override InlineCategoriesDocument CreateInlineCategoriesDocument ()
		{
			return (InlineCategoriesDocument) Activator.CreateInstance (inline_type, new object [0]);
		}

		protected override ReferencedCategoriesDocument CreateReferencedCategoriesDocument ()
		{
			return (ReferencedCategoriesDocument) Activator.CreateInstance (ref_type, new object [0]);
		}

		public override void ReadFrom (XmlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			reader.MoveToContent ();

			bool isEmpty = reader.IsEmptyElement;

			if (Document == null) {
				var href = reader.GetAttribute ("href");
				if (href != null) {
					var doc = CreateReferencedCategoriesDocument ();
					doc.Link = new Uri (href, UriKind.RelativeOrAbsolute);
					SetDocument (doc);
				} else {
					var doc = CreateInlineCategoriesDocument ();
					doc.Scheme = reader.GetAttribute ("scheme");
					if (reader.GetAttribute ("fixed") == "yes")
						doc.IsFixed = true;
					SetDocument (doc);
				}
			}
			var inline = Document as InlineCategoriesDocument;
			// var referenced = Document as ReferencedCategoriesDocument;

			reader.ReadStartElement ("categories", Version);
			if (isEmpty)
				return;

			for (reader.MoveToContent ();
			     reader.NodeType != XmlNodeType.EndElement;
			     reader.MoveToContent ()) {
				if (inline != null && reader.LocalName == "category" && reader.NamespaceURI == Namespaces.Atom10)
					ReadInlineCategoriesContent (inline, reader);
				// FIXME: else read element as an extension
				else
					reader.Skip ();
			}

			reader.ReadEndElement (); // </app:categories>
		}

		void ReadInlineCategoriesContent (InlineCategoriesDocument doc, XmlReader reader)
		{
			var cat = new SyndicationCategory ();
			atom10_formatter.ReadCategory (reader, cat);
			doc.Categories.Add (cat);
		}

		public override void WriteTo (XmlWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");

			writer.WriteStartElement ("app", "categories", Version);

			if (writer.LookupPrefix (Namespaces.Atom10) != "a10")
				writer.WriteAttributeString ("xmlns", "a10", Namespaces.Xmlns, Namespaces.Atom10);

			// xml:lang, xml:base, term, scheme, label
			if (Document.Language != null)
				writer.WriteAttributeString ("xml", "lang", Namespaces.Xml, Document.Language);
			if (Document.BaseUri != null)
				writer.WriteAttributeString ("xml", "base", Namespaces.Xml, Document.BaseUri.ToString ());

			InlineCategoriesDocument inline = Document as InlineCategoriesDocument;
			ReferencedCategoriesDocument referenced = Document as ReferencedCategoriesDocument;

			// ... no term ?

			if (inline != null) {
				if (inline.IsFixed)
					writer.WriteAttributeString ("fixed", "yes");
				if (inline.Scheme != null)
					writer.WriteAttributeString ("scheme", inline.Scheme);
			} else if (referenced != null) {
				if (referenced.Link != null)
					writer.WriteAttributeString ("href", referenced.Link.ToString ());
			}

			Document.WriteAttributeExtensions (writer, Version);

			Document.WriteElementExtensions (writer, Version);

			if (inline != null)
				WriteInlineCategoriesContent (inline, writer);
			// no (non-extension) contents for out-of-line category

			writer.WriteEndElement ();
		}

		Atom10FeedFormatter atom10_formatter = new Atom10FeedFormatter ();

		void WriteInlineCategoriesContent (InlineCategoriesDocument doc, XmlWriter writer)
		{
			foreach (var cat in doc.Categories)
				atom10_formatter.WriteCategory (cat, writer);
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
			WriteTo (writer);
		}
	}
}
