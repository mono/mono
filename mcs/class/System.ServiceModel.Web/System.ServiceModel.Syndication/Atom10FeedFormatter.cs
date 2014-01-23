//
// Atom10FeedFormatter.cs
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

// WARNING: this class is not for ensuring valid ATOM 1.0 document output
// (as well as Atom10ItemFormatter).

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
	[XmlRoot ("feed", Namespace = "http://www.w3.org/2005/Atom")]
	public class Atom10FeedFormatter : SyndicationFeedFormatter, IXmlSerializable
	{
		const string AtomNamespace ="http://www.w3.org/2005/Atom";

		bool preserve_att_ext = true, preserve_elem_ext = true;
		Type feed_type;

		public Atom10FeedFormatter ()
		{
		}

		public Atom10FeedFormatter (SyndicationFeed feedToWrite)
			: base (feedToWrite)
		{
		}

		public Atom10FeedFormatter (Type feedTypeToCreate)
		{
			if (feedTypeToCreate == null)
				throw new ArgumentNullException ("feedTypeToCreate");
			feed_type = feedTypeToCreate;
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
			get { return "Atom10"; }
		}

		protected override SyndicationFeed CreateFeedInstance ()
		{
			return new SyndicationFeed ();
		}

		public override bool CanRead (XmlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			reader.MoveToContent ();
			return reader.IsStartElement ("feed", AtomNamespace);
		}

		public override void ReadFrom (XmlReader reader)
		{
			if (!CanRead (reader))
				throw new XmlException (String.Format ("Element '{0}' in namespace '{1}' is not accepted by this syndication formatter", reader.LocalName, reader.NamespaceURI));
			ReadXml (reader, true);
		}

		protected virtual SyndicationItem ReadItem (XmlReader reader, SyndicationFeed feed)
		{
			Atom10ItemFormatter formatter = new Atom10ItemFormatter ();
			formatter.ReadFrom (reader);
			return formatter.Item;
		}

		protected virtual IEnumerable<SyndicationItem> ReadItems (XmlReader reader, SyndicationFeed feed, out bool areAllItemsRead)
		{
			Collection<SyndicationItem> c = new Collection<SyndicationItem> ();
			for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ())
				if (reader.LocalName == "entry" && reader.NamespaceURI == AtomNamespace)
					c.Add (ReadItem (reader, feed));
			areAllItemsRead = (reader.NodeType == XmlNodeType.EndElement);
			return c;
		}

		[MonoTODO ("Find out how feedBaseUri is used")]
		protected virtual void WriteItem (XmlWriter writer, SyndicationItem item, Uri feedBaseUri)
		{
			item.SaveAsAtom10 (writer);
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

			if (reader.MoveToFirstAttribute ()) {
				do {
					if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
						continue;
					if (reader.LocalName == "lang" && reader.NamespaceURI == "http://www.w3.org/XML/1998/namespace") {
						Feed.Language = reader.Value;
						continue;
					}
					if (!TryParseAttribute (reader.LocalName, reader.NamespaceURI, reader.Value, Feed, Version) && PreserveAttributeExtensions)
						Feed.AttributeExtensions.Add (new XmlQualifiedName (reader.LocalName, reader.NamespaceURI), reader.Value);
				} while (reader.MoveToNextAttribute ());
			}

			reader.ReadStartElement ();

			Collection<SyndicationItem> items = null;

			for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
				if (reader.NodeType != XmlNodeType.Element)
					throw new XmlException ("Only element node is expected under 'feed' element");
				if (reader.NamespaceURI == AtomNamespace)
					switch (reader.LocalName) {
					case "author":
						SyndicationPerson p = Feed.CreatePerson ();
						ReadPerson (reader, p);
						Feed.Authors.Add (p);
						continue;
					case "category":
						SyndicationCategory c = Feed.CreateCategory ();
						ReadCategory (reader, c);
						Feed.Categories.Add (c);
						continue;
					case "contributor":
						p = Feed.CreatePerson ();
						ReadPerson (reader, p);
						Feed.Contributors.Add (p);
						continue;
					case "generator":
						Feed.Generator = reader.ReadElementContentAsString ();
						continue;
					// "icon" is an extension
					case "id":
						Feed.Generator = reader.ReadElementContentAsString ();
						continue;
					case "link":
						SyndicationLink l = Feed.CreateLink ();
						ReadLink (reader, l);
						Feed.Links.Add (l);
						continue;
					case "logo":
						Feed.ImageUrl = CreateUri (reader.ReadElementContentAsString ());
						continue;
					case "rights":
						Feed.Copyright = ReadTextSyndicationContent (reader);
						continue;
					case "subtitle":
						Feed.Description = ReadTextSyndicationContent (reader);
						continue;
					case "title":
						Feed.Title = ReadTextSyndicationContent (reader);
						continue;
					case "updated":
						// FIXME: somehow DateTimeOffset causes the runtime crash.
						reader.ReadElementContentAsString ();
						// Feed.LastUpdatedTime = XmlConvert.ToDateTimeOffset (reader.ReadElementContentAsString ());
						continue;
					case "entry":
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

			reader.ReadEndElement ();
		}

		internal static TextSyndicationContent ReadTextSyndicationContent (XmlReader reader)
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

		internal void ReadCategory (XmlReader reader, SyndicationCategory category)
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
				throw new InvalidOperationException ("Syndication item must be set before writing");

			if (writeRoot)
				writer.WriteStartElement ("feed", AtomNamespace);

			if (Feed.BaseUri != null)
				writer.WriteAttributeString ("xml:base", Feed.BaseUri.ToString ());

			if (Feed.Language != null)
				writer.WriteAttributeString ("xml:lang", Feed.Language);

			// atom:feed elements MUST contain exactly one atom:title element.
			(Feed.Title ?? new TextSyndicationContent (String.Empty)).WriteTo (writer, "title", AtomNamespace);

			// atom:feed elements MUST contain exactly one atom:id element.
			writer.WriteElementString ("id", AtomNamespace, Feed.Id ?? new UniqueId ().ToString ());

			if (Feed.Copyright != null)
				Feed.Copyright.WriteTo (writer, "rights", AtomNamespace);

			// atom:feed elements MUST contain exactly one atom:updated element.
			writer.WriteStartElement ("updated", AtomNamespace);
			// FIXME: use DateTimeOffset itself once it is implemented.
			writer.WriteString (XmlConvert.ToString (Feed.LastUpdatedTime.UtcDateTime, XmlDateTimeSerializationMode.RoundtripKind));
			writer.WriteEndElement ();

			foreach (SyndicationCategory category in Feed.Categories)
				if (category != null)
					WriteCategory (category, writer);

			foreach (SyndicationPerson author in Feed.Authors)
				if (author != null) {
					writer.WriteStartElement ("author", AtomNamespace);
					WriteAttributeExtensions (writer, author, Version);
					writer.WriteElementString ("name", AtomNamespace, author.Name);
					writer.WriteElementString ("uri", AtomNamespace, author.Uri);
					writer.WriteElementString ("email", AtomNamespace, author.Email);
					WriteElementExtensions (writer, author, Version);
					writer.WriteEndElement ();
				}

			foreach (SyndicationPerson contributor in Feed.Contributors) {
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

			foreach (SyndicationLink link in Feed.Links)
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

			if (Feed.Description != null)
				Feed.Description.WriteTo (writer, "subtitle", AtomNamespace);

			if (Feed.ImageUrl != null)
				writer.WriteElementString ("logo", AtomNamespace, Feed.ImageUrl.ToString ());

			if (Feed.Generator != null)
				writer.WriteElementString ("generator", AtomNamespace, Feed.Generator);

			WriteItems (writer, Feed.Items, Feed.BaseUri);

			WriteElementExtensions (writer, Feed, Version);
			if (writeRoot)
				writer.WriteEndElement ();
		}

		internal void WriteCategory (SyndicationCategory category, XmlWriter writer)
		{
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
	}
}
