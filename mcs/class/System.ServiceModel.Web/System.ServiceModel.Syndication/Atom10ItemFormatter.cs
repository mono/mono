//
// Atom10ItemFormatter.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

//
// WARNING:
// This class is not for outputting ATOM 1.0 conformant XML. For example
// it does not report errors with related to the following constraints:

// - atom:entry elements MUST NOT contain more than one atom:link
//   element with a rel attribute value of "alternate" that has the
//   same combination of type and hreflang attribute values.
// - atom:entry elements that contain no child atom:content element
//   MUST contain at least one atom:link element with a rel attribute
//   value of "alternate".
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.ServiceModel.Syndication
{
	[XmlRoot ("entry", Namespace = "http://www.w3.org/2005/Atom")]
	public class Atom10ItemFormatter : SyndicationItemFormatter, IXmlSerializable
	{
		const string AtomNamespace ="http://www.w3.org/2005/Atom";

		bool preserve_att_ext = true, preserve_elem_ext = true;
		Type item_type;

		public Atom10ItemFormatter ()
		{
		}

		public Atom10ItemFormatter (SyndicationItem feedToWrite)
			: base (feedToWrite)
		{
		}

		public Atom10ItemFormatter (Type itemTypeToCreate)
		{
			if (itemTypeToCreate == null)
				throw new ArgumentNullException ("itemTypeToCreate");
			item_type = itemTypeToCreate;
		}

		protected Type ItemType {
			get { return item_type; }
		}

		public bool PreserveAttributeExtensions {
			get { return preserve_att_ext; }
			set { preserve_att_ext = value; }
		}

		public bool PreserveElementExtensions {
			get { return preserve_elem_ext; }
			set { preserve_elem_ext = value; }
		}

		public override string Version {
			get { return "Atom10"; }
		}

		protected override SyndicationItem CreateItemInstance ()
		{
			return new SyndicationItem ();
		}

		public override bool CanRead (XmlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			reader.MoveToContent ();
			return reader.IsStartElement ("entry", AtomNamespace);
		}

		public override void ReadFrom (XmlReader reader)
		{
			if (!CanRead (reader))
				throw new XmlException (String.Format ("Element '{0}' in namespace '{1}' is not accepted by this syndication formatter", reader.LocalName, reader.NamespaceURI));
			ReadXml (reader, true);
		}

		public override void WriteTo (XmlWriter writer)
		{
			WriteXml (writer, true);
		}

		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			ReadXml (reader, false);
		}

		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			WriteXml (writer, false);
		}

		XmlSchema IXmlSerializable.GetSchema ()
		{
			return null;
		}

		// read

		void ReadXml (XmlReader reader, bool fromSerializable)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			SetItem (CreateItemInstance ());

			reader.MoveToContent ();

			if (reader.MoveToFirstAttribute ()) {
				do {
					if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
						continue;
					if (!TryParseAttribute (reader.LocalName, reader.NamespaceURI, reader.Value, Item, Version) && PreserveAttributeExtensions)
						Item.AttributeExtensions.Add (new XmlQualifiedName (reader.LocalName, reader.NamespaceURI), reader.Value);
				} while (reader.MoveToNextAttribute ());
			}

			reader.ReadStartElement ();

			for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
				if (reader.NodeType != XmlNodeType.Element)
					throw new XmlException ("Only element node is expected under 'entry' element");
				if (reader.NamespaceURI == AtomNamespace)
					switch (reader.LocalName) {
					case "author":
						SyndicationPerson p = Item.CreatePerson ();
						ReadPerson (reader, p);
						Item.Authors.Add (p);
						continue;
					case "category":
						SyndicationCategory c = Item.CreateCategory ();
						ReadCategory (reader, c);
						Item.Categories.Add (c);
						continue;
					case "contributor":
						p = Item.CreatePerson ();
						ReadPerson (reader, p);
						Item.Contributors.Add (p);
						continue;
					case "id":
						Item.Id = reader.ReadElementContentAsString ();
						continue;
					case "link":
						SyndicationLink l = Item.CreateLink ();
						ReadLink (reader, l);
						Item.Links.Add (l);
						continue;
					case "published":
						Item.PublishDate = XmlConvert.ToDateTimeOffset (reader.ReadElementContentAsString ());
						continue;
					case "rights":
						Item.Copyright = ReadTextSyndicationContent (reader);
						continue;
					case "source":
						Item.SourceFeed = ReadSourceFeed (reader);
						continue;
					case "summary":
						Item.Summary = ReadTextSyndicationContent (reader);
						continue;
					case "title":
						Item.Title = ReadTextSyndicationContent (reader);
						continue;
					case "updated":
						Item.LastUpdatedTime = XmlConvert.ToDateTimeOffset (reader.ReadElementContentAsString ());
						continue;

					// Atom 1.0 does not specify "content" element, but it is required to distinguish Content property from extension elements.
					case "content":
						if (reader.GetAttribute ("src") != null) {
							Item.Content = new UrlSyndicationContent (CreateUri (reader.GetAttribute ("src")), reader.GetAttribute ("type"));
							reader.Skip ();
							continue;
						}
						switch (reader.GetAttribute ("type")) {
						case "text":
						case "html":
						case "xhtml":
							Item.Content = ReadTextSyndicationContent (reader);
							continue;
						default:
							SyndicationContent content;
							if (!TryParseContent (reader, Item, reader.GetAttribute ("type"), Version, out content))
								Item.Content = new XmlSyndicationContent (reader);
							continue;
						}
					}
				if (!TryParseElement (reader, Item, Version)) {
					if (PreserveElementExtensions)
						// FIXME: what to specify for maxExtensionSize
						LoadElementExtensions (reader, Item, int.MaxValue);
					else
						reader.Skip ();
				}
			}

			reader.ReadEndElement (); // </item>
		}

		TextSyndicationContent ReadTextSyndicationContent (XmlReader reader)
		{
			TextSyndicationContentKind kind = TextSyndicationContentKind.Plaintext;
			switch (reader.GetAttribute ("type")) {
			case "html":
				kind = TextSyndicationContentKind.Html;
				break;
			case "xhtml":
				kind = TextSyndicationContentKind.XHtml;
				break;
			}
			string text = reader.ReadElementContentAsString ();
			TextSyndicationContent t = new TextSyndicationContent (text, kind);
			return t;
		}

		void ReadCategory (XmlReader reader, SyndicationCategory category)
		{
			if (reader.MoveToFirstAttribute ()) {
				do {
					if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
						continue;
					if (reader.NamespaceURI == String.Empty) {
						switch (reader.LocalName) {
						case "term":
							category.Name = reader.Value;
							continue;
						case "scheme":
							category.Scheme = reader.Value;
							continue;
						case "label":
							category.Label = reader.Value;
							continue;
						}
					}
					if (!TryParseAttribute (reader.LocalName, reader.NamespaceURI, reader.Value, category, Version) && PreserveAttributeExtensions)
						category.AttributeExtensions.Add (new XmlQualifiedName (reader.LocalName, reader.NamespaceURI), reader.Value);
				} while (reader.MoveToNextAttribute ());
				reader.MoveToElement ();
			}

			if (!reader.IsEmptyElement) {
				reader.Read ();
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (!TryParseElement (reader, category, Version)) {
						if (PreserveElementExtensions)
							// FIXME: what should be used for maxExtenswionSize
							LoadElementExtensions (reader, category, int.MaxValue);
						else
							reader.Skip ();
					}
				}
			}
			reader.Read (); // </category> or <category ... />
		}

		void ReadLink (XmlReader reader, SyndicationLink link)
		{
			if (reader.MoveToFirstAttribute ()) {
				do {
					if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
						continue;
					if (reader.NamespaceURI == String.Empty) {
						switch (reader.LocalName) {
						case "href":
							link.Uri = CreateUri (reader.Value);
							continue;
						case "rel":
							link.RelationshipType = reader.Value;
							continue;
						case "type":
							link.MediaType = reader.Value;
							continue;
						case "length":
							link.Length = XmlConvert.ToInt64 (reader.Value);
							continue;
						case "title":
							link.Title = reader.Value;
							continue;
						}
					}
					if (!TryParseAttribute (reader.LocalName, reader.NamespaceURI, reader.Value, link, Version) && PreserveAttributeExtensions)
						link.AttributeExtensions.Add (new XmlQualifiedName (reader.LocalName, reader.NamespaceURI), reader.Value);
				} while (reader.MoveToNextAttribute ());
				reader.MoveToElement ();
			}

			if (!reader.IsEmptyElement) {
				reader.Read ();
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (!TryParseElement (reader, link, Version)) {
						if (PreserveElementExtensions)
							// FIXME: what should be used for maxExtenswionSize
							LoadElementExtensions (reader, link, int.MaxValue);
						else
							reader.Skip ();
					}
				}
			}
			reader.Read (); // </link> or <link ... />
		}

		void ReadPerson (XmlReader reader, SyndicationPerson person)
		{
			if (reader.MoveToFirstAttribute ()) {
				do {
					if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
						continue;
					if (!TryParseAttribute (reader.LocalName, reader.NamespaceURI, reader.Value, person, Version) && PreserveAttributeExtensions)
						person.AttributeExtensions.Add (new XmlQualifiedName (reader.LocalName, reader.NamespaceURI), reader.Value);
				} while (reader.MoveToNextAttribute ());
				reader.MoveToElement ();
			}

			if (!reader.IsEmptyElement) {
				reader.Read ();
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (reader.NodeType == XmlNodeType.Element && reader.NamespaceURI == AtomNamespace) {
						switch (reader.LocalName) {
						case "name":
							person.Name = reader.ReadElementContentAsString ();
							continue;
						case "uri":
							person.Uri = reader.ReadElementContentAsString ();
							continue;
						case "email":
							person.Email = reader.ReadElementContentAsString ();
							continue;
						}
					}
					if (!TryParseElement (reader, person, Version)) {
						if (PreserveElementExtensions)
							// FIXME: what should be used for maxExtenswionSize
							LoadElementExtensions (reader, person, int.MaxValue);
						else
							reader.Skip ();
					}
				}
			}
			reader.Read (); // end element or empty element
		}

		SyndicationFeed ReadSourceFeed (XmlReader reader)
		{
			SyndicationFeed feed = null;
			if (!reader.IsEmptyElement) {
				Atom10FeedFormatter ff = new Atom10FeedFormatter ();
				((IXmlSerializable) ff).ReadXml (reader); // this does not check the QName of the wrapping element.
				feed = ff.Feed;
			}
			else
				feed = new SyndicationFeed ();
			reader.Read (); // </source> or <source ... />
			return feed;
		}

		Uri CreateUri (string uri)
		{
			return new Uri (uri, UriKind.RelativeOrAbsolute);
		}

		// write

		void WriteXml (XmlWriter writer, bool writeRoot)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			if (Item == null)
				throw new InvalidOperationException ("Syndication item must be set before writing");

			if (writeRoot)
				writer.WriteStartElement ("entry", AtomNamespace);

			if (Item.BaseUri != null)
				writer.WriteAttributeString ("xml:base", Item.BaseUri.ToString ());

			// atom:entry elements MUST contain exactly one atom:id element.
			writer.WriteElementString ("id", AtomNamespace, Item.Id ?? new UniqueId ().ToString ());

			// atom:entry elements MUST contain exactly one atom:title element.
			(Item.Title ?? new TextSyndicationContent (String.Empty)).WriteTo (writer, "title", AtomNamespace);

			if (Item.Summary != null)
				Item.Summary.WriteTo (writer, "summary", AtomNamespace);

			if (!Item.PublishDate.Equals (default (DateTimeOffset))) {
				writer.WriteStartElement ("published");
				// FIXME: use DateTimeOffset itself once it is implemented.
				writer.WriteString (XmlConvert.ToString (Item.PublishDate.UtcDateTime, XmlDateTimeSerializationMode.RoundtripKind));
				writer.WriteEndElement ();
			}

			// atom:entry elements MUST contain exactly one atom:updated element.
			writer.WriteStartElement ("updated", AtomNamespace);
			// FIXME: use DateTimeOffset itself once it is implemented.
			writer.WriteString (XmlConvert.ToString (Item.LastUpdatedTime.UtcDateTime, XmlDateTimeSerializationMode.RoundtripKind));
			writer.WriteEndElement ();

			foreach (SyndicationPerson author in Item.Authors)
				if (author != null) {
					writer.WriteStartElement ("author", AtomNamespace);
					WriteAttributeExtensions (writer, author, Version);
					writer.WriteElementString ("name", AtomNamespace, author.Name);
					writer.WriteElementString ("uri", AtomNamespace, author.Uri);
					writer.WriteElementString ("email", AtomNamespace, author.Email);
					WriteElementExtensions (writer, author, Version);
					writer.WriteEndElement ();
				}

			foreach (SyndicationPerson contributor in Item.Contributors) {
				if (contributor != null) {
					writer.WriteStartElement ("contributor", AtomNamespace);
					WriteAttributeExtensions (writer, contributor, Version);
					writer.WriteElementString ("name", AtomNamespace, contributor.Name);
					writer.WriteElementString ("uri", AtomNamespace, contributor.Uri);
					writer.WriteElementString ("email", AtomNamespace, contributor.Email);
					WriteElementExtensions (writer, contributor, Version);
					writer.WriteEndElement ();
				}
			}

			foreach (SyndicationCategory category in Item.Categories)
				if (category != null) {
					writer.WriteStartElement ("category", AtomNamespace);
					if (category.Name != null)
						writer.WriteAttributeString ("term", category.Name);
					if (category.Label != null)
						writer.WriteAttributeString ("label", category.Label);
					if (category.Scheme != null)
						writer.WriteAttributeString ("scheme", category.Scheme);
					WriteAttributeExtensions (writer, category, Version);
					WriteElementExtensions (writer, category, Version);
					writer.WriteEndElement ();
				}

			foreach (SyndicationLink link in Item.Links)
				if (link != null) {
					writer.WriteStartElement ("link");
					if (link.RelationshipType != null)
						writer.WriteAttributeString ("rel", link.RelationshipType);
					if (link.MediaType != null)
						writer.WriteAttributeString ("type", link.MediaType);
					if (link.Title != null)
						writer.WriteAttributeString ("title", link.Title);
					if (link.Length != 0)
						writer.WriteAttributeString ("length", link.Length.ToString (CultureInfo.InvariantCulture));
					writer.WriteAttributeString ("href", link.Uri != null ? link.Uri.ToString () : String.Empty);
					WriteAttributeExtensions (writer, link, Version);
					WriteElementExtensions (writer, link, Version);
					writer.WriteEndElement ();
				}

			if (Item.Content != null)
				Item.Content.WriteTo (writer, "content", AtomNamespace);

			if (Item.Copyright != null)
				Item.Copyright.WriteTo (writer, "rights", AtomNamespace);

			if (Item.SourceFeed != null) {
				writer.WriteStartElement ("source", AtomNamespace);
				Item.SourceFeed.SaveAsAtom10 (writer);
				writer.WriteEndElement ();
			}

			WriteElementExtensions (writer, Item, Version);
			if (writeRoot)
				writer.WriteEndElement ();
		}
	}
}
