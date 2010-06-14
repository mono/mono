//
// Rss20ItemFormatter.cs
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.ServiceModel.Syndication
{
	static class XmlReaderExtensions
	{
		public static bool IsTextNode (this XmlReader r)
		{
			switch (r.NodeType) {
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				return true;
			}
			return false;
		}
	}

	[XmlRoot ("item", Namespace = "")]
	public class Rss20ItemFormatter : SyndicationItemFormatter, IXmlSerializable
	{
		const string AtomNamespace ="http://www.w3.org/2005/Atom";

		bool ext_atom_serialization, preserve_att_ext = true, preserve_elem_ext = true;
		Type item_type;

		public Rss20ItemFormatter ()
		{
			ext_atom_serialization = true;
		}

		public Rss20ItemFormatter (SyndicationItem itemToWrite)
			: this (itemToWrite, true)
		{
		}

		public Rss20ItemFormatter (SyndicationItem itemToWrite, bool serializeExtensionsAsAtom)
			: base (itemToWrite)
		{
			ext_atom_serialization = serializeExtensionsAsAtom;
		}

		public Rss20ItemFormatter (Type itemTypeToCreate)
		{
			if (itemTypeToCreate == null)
				throw new ArgumentNullException ("itemTypeToCreate");
			item_type = itemTypeToCreate;
		}

		public bool SerializeExtensionsAsAtom {
			get { return ext_atom_serialization; }
			set { ext_atom_serialization = value; }
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
			get { return "Rss20"; }
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
			return reader.IsStartElement ("item", String.Empty);
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

			if (PreserveAttributeExtensions && reader.MoveToFirstAttribute ()) {
				do {
					if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
						continue;
					if (!TryParseAttribute (reader.LocalName, reader.NamespaceURI, reader.Value, Item, Version))
						Item.AttributeExtensions.Add (new XmlQualifiedName (reader.LocalName, reader.NamespaceURI), reader.Value);
				} while (reader.MoveToNextAttribute ());
			}

			reader.ReadStartElement ();

			for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
				if (reader.NodeType != XmlNodeType.Element)
					throw new XmlException ("Only element node is expected under 'item' element");
				if (reader.NamespaceURI == String.Empty)
					switch (reader.LocalName) {
					case "title":
						Item.Title = ReadTextSyndicationContent (reader);
						continue;
					case "link":
						SyndicationLink l = Item.CreateLink ();
						ReadLink (reader, l);
						Item.Links.Add (l);
						continue;
					case "description":
						Item.Summary = ReadTextSyndicationContent (reader);
						continue;
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
					// case "comments": // treated as extension ...
					case "enclosure":
						l = Item.CreateLink ();
						ReadEnclosure (reader, l);
						Item.Links.Add (l);
						continue;
					case "guid":
						if (reader.GetAttribute ("isPermaLink") == "true")
							Item.AddPermalink (CreateUri (reader.ReadElementContentAsString ()));
						else
							Item.Id = reader.ReadElementContentAsString ();
						continue;
					case "pubDate":
						// FIXME: somehow DateTimeOffset causes the runtime crash.
						reader.ReadElementContentAsString ();
						// Item.PublishDate = FromRFC822DateString (reader.ReadElementContentAsString ());
						continue;
					case "source":
						Item.SourceFeed = new SyndicationFeed ();
						ReadSourceFeed (reader, Item.SourceFeed);
						continue;
					}
				else if (SerializeExtensionsAsAtom && reader.NamespaceURI == AtomNamespace) {
					switch (reader.LocalName) {
					case "contributor":
						SyndicationPerson p = Item.CreatePerson ();
						ReadPersonAtom10 (reader, p);
						Item.Contributors.Add (p);
						continue;
					case "updated":
						// FIXME: somehow DateTimeOffset causes the runtime crash.
						reader.ReadElementContentAsString ();
						// Item.LastUpdatedTime = XmlConvert.ToDateTimeOffset (reader.ReadElementContentAsString ());
						continue;
					case "rights":
						Item.Copyright = ReadTextSyndicationContent (reader);
						continue;
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
						case "domain":
							category.Scheme = reader.Value;
							continue;
						}
					}
					if (PreserveAttributeExtensions)
						if (!TryParseAttribute (reader.LocalName, reader.NamespaceURI, reader.Value, category, Version))
							category.AttributeExtensions.Add (new XmlQualifiedName (reader.LocalName, reader.NamespaceURI), reader.Value);
				} while (reader.MoveToNextAttribute ());
				reader.MoveToElement ();
			}

			if (!reader.IsEmptyElement) {
				reader.Read ();
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (reader.IsTextNode ())
						category.Name += reader.Value;
					else if (!TryParseElement (reader, category, Version)) {
						if (PreserveElementExtensions)
							// FIXME: what should be used for maxExtenswionSize
							LoadElementExtensions (reader, category, int.MaxValue);
						else
							reader.Skip ();
					}
					reader.Read ();
				}
			}
			reader.Read (); // </category> or <category ... />
		}

		// SyndicationLink.CreateMediaEnclosureLink() is almost
		// useless here since it cannot handle extension attributes
		// in straightforward way (it I use it, I have to iterate
		// attributes twice just to read extensions).
		void ReadEnclosure (XmlReader reader, SyndicationLink link)
		{
			link.RelationshipType = "enclosure";

			if (PreserveAttributeExtensions && reader.MoveToFirstAttribute ()) {
				do {
					if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
						continue;
					if (reader.NamespaceURI == String.Empty) {
						switch (reader.LocalName) {
						case "url":
							link.Uri = CreateUri (reader.Value);
							continue;
						case "type":
							link.MediaType = reader.Value;
							continue;
						case "length":
							link.Length = XmlConvert.ToInt64 (reader.Value);
							continue;
						}
					}
					if (!TryParseAttribute (reader.LocalName, reader.NamespaceURI, reader.Value, link, Version))
						link.AttributeExtensions.Add (new XmlQualifiedName (reader.LocalName, reader.NamespaceURI), reader.Value);
				} while (reader.MoveToNextAttribute ());
				reader.MoveToElement ();
			}

			// Actually .NET fails to read extension here.
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
			reader.Read (); // </enclosure> or <enclosure ... />
		}

		void ReadLink (XmlReader reader, SyndicationLink link)
		{
			if (PreserveAttributeExtensions && reader.MoveToFirstAttribute ()) {
				do {
					if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
						continue;
					if (!TryParseAttribute (reader.LocalName, reader.NamespaceURI, reader.Value, link, Version))
						link.AttributeExtensions.Add (new XmlQualifiedName (reader.LocalName, reader.NamespaceURI), reader.Value);
				} while (reader.MoveToNextAttribute ());
				reader.MoveToElement ();
			}

			if (!reader.IsEmptyElement) {
				string url = null;
				reader.Read ();
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (reader.IsTextNode ())
						url += reader.Value;
					else if (!TryParseElement (reader, link, Version)) {
						if (PreserveElementExtensions)
							// FIXME: what should be used for maxExtenswionSize
							LoadElementExtensions (reader, link, int.MaxValue);
						else
							reader.Skip ();
					}
					reader.Read ();
				}
				link.Uri = CreateUri (url);
			}
			reader.Read (); // </link> or <link ... />
		}

		void ReadPerson (XmlReader reader, SyndicationPerson person)
		{
			if (PreserveAttributeExtensions && reader.MoveToFirstAttribute ()) {
				do {
					if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
						continue;
					if (!TryParseAttribute (reader.LocalName, reader.NamespaceURI, reader.Value, person, Version))
						person.AttributeExtensions.Add (new XmlQualifiedName (reader.LocalName, reader.NamespaceURI), reader.Value);
				} while (reader.MoveToNextAttribute ());
				reader.MoveToElement ();
			}

			if (!reader.IsEmptyElement) {
				reader.Read ();
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (reader.IsTextNode ())
						person.Email += reader.Value;
					else if (!TryParseElement (reader, person, Version)) {
						if (PreserveElementExtensions)
							// FIXME: what should be used for maxExtenswionSize
							LoadElementExtensions (reader, person, int.MaxValue);
						else
							reader.Skip ();
					}
					reader.Read ();
				}
			}
			reader.Read (); // end element or empty element
		}

		// copied from Atom10ItemFormatter
		void ReadPersonAtom10 (XmlReader reader, SyndicationPerson person)
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

		void ReadSourceFeed (XmlReader reader, SyndicationFeed feed)
		{
			if (reader.MoveToFirstAttribute ()) {
				do {
					if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
						continue;
					if (reader.NamespaceURI == String.Empty) {
						switch (reader.LocalName) {
						case "url":
							feed.Links.Add (new SyndicationLink (CreateUri (reader.Value)));
							continue;
						}
					}
				} while (reader.MoveToNextAttribute ());
				reader.MoveToElement ();
			}

			if (!reader.IsEmptyElement) {
				reader.Read ();
				string title = null;
				while (reader.NodeType != XmlNodeType.EndElement) {
					if (reader.IsTextNode ())
						title += reader.Value;
					reader.Skip ();
					reader.MoveToContent ();
				}
				feed.Title = new TextSyndicationContent (title);
			}
			reader.Read (); // </source> or <source ... />
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
				writer.WriteStartElement ("item");

			if (Item.BaseUri != null)
				writer.WriteAttributeString ("xml:base", Item.BaseUri.ToString ());

			WriteAttributeExtensions (writer, Item, Version);

			if (Item.Id != null) {
				writer.WriteStartElement ("guid");
				writer.WriteAttributeString ("isPermaLink", "false");
				writer.WriteString (Item.Id);
				writer.WriteEndElement ();
			}

			if (Item.Title != null) {
				writer.WriteStartElement ("title");
				writer.WriteString (Item.Title.Text);
				writer.WriteEndElement ();
			}

			foreach (SyndicationPerson author in Item.Authors)
				if (author != null) {
					writer.WriteStartElement ("author");
					WriteAttributeExtensions (writer, author, Version);
					writer.WriteString (author.Email);
					WriteElementExtensions (writer, author, Version);
					writer.WriteEndElement ();
				}
			foreach (SyndicationCategory category in Item.Categories)
				if (category != null) {
					writer.WriteStartElement ("category");
					if (category.Scheme != null)
						writer.WriteAttributeString ("domain", category.Scheme);
					WriteAttributeExtensions (writer, category, Version);
					writer.WriteString (category.Name);
					WriteElementExtensions (writer, category, Version);
					writer.WriteEndElement ();
				}

			if (Item.Content != null) {
				Item.Content.WriteTo (writer, "description", String.Empty);
			} else if (Item.Summary != null)
				Item.Summary.WriteTo (writer, "description", String.Empty);
			else if (Item.Title == null) { // according to the RSS 2.0 spec, either of title or description must exist.
				writer.WriteStartElement ("description");
				writer.WriteEndElement ();
			}

			foreach (SyndicationLink link in Item.Links)
				switch (link.RelationshipType) {
				case "enclosure":
					writer.WriteStartElement ("enclosure");
					if (link.Uri != null)
						writer.WriteAttributeString ("uri", link.Uri.ToString ());
					if (link.Length != 0)
						writer.WriteAttributeString ("length", XmlConvert.ToString (link.Length));
					if (link.MediaType != null)
						writer.WriteAttributeString ("type", link.MediaType);
					WriteAttributeExtensions (writer, link, Version);
					WriteElementExtensions (writer, link, Version);
					writer.WriteEndElement ();
					break;
				default:
					writer.WriteStartElement ("link");
					WriteAttributeExtensions (writer, link, Version);
					writer.WriteString (link.Uri != null ? link.Uri.ToString () : String.Empty);
					WriteElementExtensions (writer, link, Version);
					writer.WriteEndElement ();
					break;
				}

			if (Item.SourceFeed != null) {
				writer.WriteStartElement ("source");
				if (Item.SourceFeed.Links.Count > 0) {
					Uri u = Item.SourceFeed.Links [0].Uri;
					writer.WriteAttributeString ("url", u != null ? u.ToString () : String.Empty);
				}
				writer.WriteString (Item.SourceFeed.Title != null ? Item.SourceFeed.Title.Text : String.Empty);
				writer.WriteEndElement ();
			}

			if (!Item.PublishDate.Equals (default (DateTimeOffset))) {
				writer.WriteStartElement ("pubDate");
				writer.WriteString (ToRFC822DateString (Item.PublishDate));
				writer.WriteEndElement ();
			}

			if (SerializeExtensionsAsAtom) {
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

				if (!Item.LastUpdatedTime.Equals (default (DateTimeOffset))) {
					writer.WriteStartElement ("updated", AtomNamespace);
					// FIXME: how to handle offset part?
					writer.WriteString (XmlConvert.ToString (Item.LastUpdatedTime.DateTime, XmlDateTimeSerializationMode.Local));
					writer.WriteEndElement ();
				}

				if (Item.Copyright != null)
					Item.Copyright.WriteTo (writer, "rights", AtomNamespace);
#if false
				if (Item.Content != null)
					Item.Content.WriteTo (writer, "content", AtomNamespace);
#endif
			}

			WriteElementExtensions (writer, Item, Version);

			if (writeRoot)
				writer.WriteEndElement ();
		}

		// FIXME: DateTimeOffset.ToString() needs another overload.
		// When it is implemented, just remove ".DateTime" parts below.
		string ToRFC822DateString (DateTimeOffset date)
		{
			switch (date.DateTime.Kind) {
			case DateTimeKind.Utc:
				return date.DateTime.ToString ("ddd, dd MMM yyyy HH:mm:ss 'Z'", DateTimeFormatInfo.InvariantInfo);
			case DateTimeKind.Local:
				StringBuilder sb = new StringBuilder (date.DateTime.ToString ("ddd, dd MMM yyyy HH:mm:ss zzz", DateTimeFormatInfo.InvariantInfo));
				sb.Remove (sb.Length - 3, 1);
				return sb.ToString (); // remove ':' from +hh:mm
			default:
				return date.DateTime.ToString ("ddd, dd MMM yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
			}
		}

		string [] rfc822formats = new string [] {
			"ddd, dd MMM yyyy HH:mm:ss 'Z'",
			"ddd, dd MMM yyyy HH:mm:ss zzz",
			"ddd, dd MMM yyyy HH:mm:ss"};

		// FIXME: DateTimeOffset is still incomplete. When it is done,
		// simplify the code.
		DateTimeOffset FromRFC822DateString (string s)
		{
			return XmlConvert.ToDateTimeOffset (s, rfc822formats);
		}
	}
}
