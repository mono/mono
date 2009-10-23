//
// Rss20FeedFormatter.cs
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
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.ServiceModel.Syndication
{
	[XmlRoot ("rss", Namespace = "")]
	public class Rss20FeedFormatter : SyndicationFeedFormatter, IXmlSerializable
	{
		const string AtomNamespace ="http://www.w3.org/2005/Atom";

		bool ext_atom_serialization, preserve_att_ext = true, preserve_elem_ext = true;
		Type feed_type;

		public Rss20FeedFormatter ()
		{
			ext_atom_serialization = true;
		}

		public Rss20FeedFormatter (SyndicationFeed feedToWrite)
			: this (feedToWrite, true)
		{
		}

		public Rss20FeedFormatter (SyndicationFeed feedToWrite, bool serializeExtensionsAsAtom)
			: base (feedToWrite)
		{
			ext_atom_serialization = serializeExtensionsAsAtom;
		}

		public Rss20FeedFormatter (Type feedTypeToCreate)
		{
			if (feedTypeToCreate == null)
				throw new ArgumentNullException ("feedTypeToCreate");
			feed_type = feedTypeToCreate;
		}

		public bool SerializeExtensionsAsAtom {
			get { return ext_atom_serialization; }
			set { ext_atom_serialization = value; }
		}

		protected Type FeedType {
			get { return feed_type; }
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

		protected override SyndicationFeed CreateFeedInstance ()
		{
			return new SyndicationFeed ();
		}

		// hmm, why is it overriden? probably failed API cleanup.
		protected internal override void SetFeed (SyndicationFeed feed)
		{
			base.SetFeed (feed);
		}

		public override bool CanRead (XmlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			reader.MoveToContent ();
			return reader.IsStartElement ("rss", String.Empty);
		}

		public override void ReadFrom (XmlReader reader)
		{
			if (!CanRead (reader))
				throw new XmlException (String.Format ("Element '{0}' in namespace '{1}' is not accepted by this syndication formatter", reader.LocalName, reader.NamespaceURI));
			ReadXml (reader, true);
		}

		protected virtual SyndicationItem ReadItem (XmlReader reader, SyndicationFeed feed)
		{
			Rss20ItemFormatter formatter = new Rss20ItemFormatter();
			formatter.ReadFrom (reader);
			return formatter.Item;
		}

		protected virtual IEnumerable<SyndicationItem> ReadItems (XmlReader reader, SyndicationFeed feed, out bool areAllItemsRead)
		{
			Collection<SyndicationItem> c = new Collection<SyndicationItem> ();
			for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ())
				if (reader.LocalName == "item" && reader.NamespaceURI == String.Empty)
					c.Add (ReadItem (reader, feed));
			areAllItemsRead = (reader.NodeType == XmlNodeType.EndElement);
			return c;
		}

		protected virtual void WriteItem (XmlWriter writer, SyndicationItem item, Uri feedBaseUri)
		{
			item.SaveAsRss20 (writer);
		}

		protected virtual void WriteItems (XmlWriter writer, IEnumerable<SyndicationItem> items, Uri feedBaseUri)
		{
			if (items == null)
				throw new ArgumentNullException ("items");
			foreach (SyndicationItem item in items)
				WriteItem (writer, item, feedBaseUri);
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
			SetFeed (CreateFeedInstance ());

			reader.MoveToContent ();

			string ver = reader.GetAttribute ("version");
			if (ver != "2.0")
				throw new NotSupportedException (String.Format ("RSS Version '{0}' is not supported", ver));

			if (PreserveAttributeExtensions && reader.MoveToFirstAttribute ()) {
				do {
					if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
						continue;
					if (reader.NamespaceURI == String.Empty && reader.LocalName == "version")
						continue;
					if (!TryParseAttribute (reader.LocalName, reader.NamespaceURI, reader.Value, Feed, Version))
						Feed.AttributeExtensions.Add (new XmlQualifiedName (reader.LocalName, reader.NamespaceURI), reader.Value);
				} while (reader.MoveToNextAttribute ());
			}

			reader.ReadStartElement (); // <rss> => <channel>
			reader.MoveToContent ();
			reader.ReadStartElement ("channel", String.Empty); // <channel> => *

			Collection<SyndicationItem> items = null;

			for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
				if (reader.NodeType != XmlNodeType.Element)
					throw new XmlException ("Only element node is expected under 'channel' element");
				if (reader.NamespaceURI == String.Empty)
					switch (reader.LocalName) {
					case "title":
						Feed.Title = ReadTextSyndicationContent (reader);
						continue;
					case "link":
						SyndicationLink l = Feed.CreateLink ();
						ReadLink (reader, l);
						Feed.Links.Add (l);
						continue;
					case "description":
						Feed.Description = ReadTextSyndicationContent (reader);
						continue;
					case "language":
						Feed.Language = reader.ReadElementContentAsString ();
						continue;
					case "copyright":
						Feed.Copyright = ReadTextSyndicationContent (reader);
						continue;
					case "managingEditor":
						SyndicationPerson p = Feed.CreatePerson ();
						ReadPerson (reader, p);
						Feed.Authors.Add (p);
						continue;
					case "pubDate":
						// FIXME: somehow DateTimeOffset causes the runtime crash.
						reader.ReadElementContentAsString ();
						// Feed.PublishDate = FromRFC822DateString (reader.ReadElementContentAsString ());
						continue;
					case "lastBuildDate":
						// FIXME: somehow DateTimeOffset causes the runtime crash.
						reader.ReadElementContentAsString ();
						// Feed.LastUpdatedTime = FromRFC822DateString (reader.ReadElementContentAsString ());
						continue;
					case "category":
						SyndicationCategory c = Feed.CreateCategory ();
						ReadCategory (reader, c);
						Feed.Categories.Add (c);
						continue;
					case "generator":
						Feed.Generator = reader.ReadElementContentAsString ();
						continue;
					//  "webMaster" "docs" "cloud" "ttl" "image" "rating" "textInput" "skipHours" "skipDays" are not handled.
					case "item":
						if (items == null) {
							items = new Collection<SyndicationItem> ();
							Feed.Items = items;
						}
						items.Add (ReadItem (reader, Feed));
						continue;
					}
				if (!TryParseElement (reader, Feed, Version)) {
					if (PreserveElementExtensions)
						// FIXME: what to specify for maxExtensionSize
						LoadElementExtensions (reader, Feed, int.MaxValue);
					else
						reader.Skip ();
				}
			}

			reader.ReadEndElement (); // </channel>
			reader.ReadEndElement (); // </rss>
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

		Uri CreateUri (string uri)
		{
			return new Uri (uri, UriKind.RelativeOrAbsolute);
		}

		// write

		void WriteXml (XmlWriter writer, bool writeRoot)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			if (Feed == null)
				throw new InvalidOperationException ("Syndication feed must be set before writing");

			if (writeRoot)
				writer.WriteStartElement ("rss");

			if (SerializeExtensionsAsAtom)
				writer.WriteAttributeString ("xmlns", "a10", "http://www.w3.org/2000/xmlns/", AtomNamespace);
			writer.WriteAttributeString ("version", "2.0");

			writer.WriteStartElement ("channel");

			if (Feed.BaseUri != null)
				writer.WriteAttributeString ("xml:base", Feed.BaseUri.ToString ());

			writer.WriteElementString ("title", String.Empty, Feed.Title != null ? Feed.Title.Text : String.Empty);

			writer.WriteElementString ("description", String.Empty, Feed.Description != null ? Feed.Description.Text : String.Empty);

			if (Feed.Copyright != null)
				writer.WriteElementString ("copyright", String.Empty, Feed.Copyright.Text);

			if (!Feed.LastUpdatedTime.Equals (default (DateTimeOffset))) {
				writer.WriteStartElement ("lastBuildDate");
				writer.WriteString (ToRFC822DateString (Feed.LastUpdatedTime));
				writer.WriteEndElement ();
			}

			if (Feed.Generator != null)
				writer.WriteElementString ("generator", String.Empty, Feed.Generator);
			if (Feed.ImageUrl != null) {
				writer.WriteStartElement ("image");
				writer.WriteElementString ("url", String.Empty, Feed.ImageUrl.ToString ());
				// FIXME: are they really empty?
				writer.WriteElementString ("title", String.Empty, String.Empty);
				writer.WriteElementString ("link", String.Empty, String.Empty);
				writer.WriteEndElement ();
			}
			if (Feed.Language != null)
				writer.WriteElementString ("language", String.Empty, Feed.Language);

			foreach (SyndicationPerson author in Feed.Authors)
				if (author != null) {
					writer.WriteStartElement ("managingEditor");
					WriteAttributeExtensions (writer, author, Version);
					writer.WriteString (author.Email);
					WriteElementExtensions (writer, author, Version);
					writer.WriteEndElement ();
				}
			foreach (SyndicationCategory category in Feed.Categories)
				if (category != null) {
					writer.WriteStartElement ("category");
					if (category.Scheme != null)
						writer.WriteAttributeString ("domain", category.Scheme);
					WriteAttributeExtensions (writer, category, Version);
					writer.WriteString (category.Name);
					WriteElementExtensions (writer, category, Version);
					writer.WriteEndElement ();
				}

			foreach (SyndicationLink link in Feed.Links)
				if (link != null) {
					writer.WriteStartElement ("link");
					WriteAttributeExtensions (writer, link, Version);
					writer.WriteString (link.Uri != null ? link.Uri.ToString () : String.Empty);
					WriteElementExtensions (writer, link, Version);
					writer.WriteEndElement ();
				}

			WriteItems (writer, Feed.Items, Feed.BaseUri);

			if (SerializeExtensionsAsAtom) {

				if (Feed.Id != null) {
					writer.WriteStartElement ("a10", "id", AtomNamespace);
					writer.WriteString (Feed.Id);
					writer.WriteEndElement ();
				}

				foreach (SyndicationPerson contributor in Feed.Contributors) {
					if (contributor != null) {
						writer.WriteStartElement ("a10", "contributor", AtomNamespace);
						WriteAttributeExtensions (writer, contributor, Version);
						writer.WriteElementString ("a10", "name", AtomNamespace, contributor.Name);
						writer.WriteElementString ("a10", "uri", AtomNamespace, contributor.Uri);
						writer.WriteElementString ("a10", "email", AtomNamespace, contributor.Email);
						WriteElementExtensions (writer, contributor, Version);
						writer.WriteEndElement ();
					}
				}
			}

			writer.WriteEndElement (); // </channel>

			if (writeRoot)
				writer.WriteEndElement (); // </rss>
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
	}
}
