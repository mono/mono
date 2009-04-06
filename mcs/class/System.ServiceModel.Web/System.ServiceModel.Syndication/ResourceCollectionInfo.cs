//
// ResourceCollectionInfo.cs
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
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	public class ResourceCollectionInfo
	{
		public ResourceCollectionInfo ()
		{
			Accepts = new Collection<string> ();
			Categories = new Collection<CategoriesDocument> ();
		}

		public ResourceCollectionInfo (TextSyndicationContent title, Uri link)
			: this ()
		{
			Title = title;
			Link = link;
		}

		public ResourceCollectionInfo (string title, Uri link)
			: this (new TextSyndicationContent (title), link)
		{
		}

		public ResourceCollectionInfo (TextSyndicationContent title, Uri link, IEnumerable<CategoriesDocument> categories, bool allowsNewEntries)
			: this (title, link)
		{
			if (categories == null)
				throw new ArgumentNullException ("categories");

			foreach (var c in categories)
				Categories.Add (c);
			allow_new_entries = allowsNewEntries;
		}

		public ResourceCollectionInfo (TextSyndicationContent title, Uri link, IEnumerable<CategoriesDocument> categories, IEnumerable<string> accepts)
			: this (title, link, categories, true)
		{
			if (accepts == null)
				throw new ArgumentNullException ("accepts");
			foreach (var a in accepts)
				Accepts.Add (a);
		}

		bool allow_new_entries;
		SyndicationExtensions extensions = new SyndicationExtensions ();

		public Collection<string> Accepts { get; private set; }

		public Dictionary<XmlQualifiedName, string> AttributeExtensions {
			get { return extensions.Attributes; }
		}

		public Uri BaseUri { get; set; }

		public Collection<CategoriesDocument> Categories { get; private set; }

		public SyndicationElementExtensionCollection ElementExtensions {
			get { return extensions.Elements; }
		}

		public Uri Link { get; set; }

		public TextSyndicationContent Title { get; set; }


		protected internal virtual InlineCategoriesDocument CreateInlineCategoriesDocument ()
		{
			return new InlineCategoriesDocument ();
		}

		protected internal virtual ReferencedCategoriesDocument CreateReferencedCategoriesDocument ()
		{
			return new ReferencedCategoriesDocument ();
		}

		protected internal virtual bool TryParseAttribute (string name, string ns, string value, string version)
		{
			if (name == "base" && ns == Namespaces.Xml)
				BaseUri = new Uri (value, UriKind.RelativeOrAbsolute);
			else if (name == "href" && ns == String.Empty)
				Link = new Uri (value, UriKind.RelativeOrAbsolute);
			else
				return false;
			return true;
		}

		protected internal virtual bool TryParseElement (XmlReader reader, string version)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			reader.MoveToContent ();

			if (reader.LocalName != "collection" || reader.NamespaceURI != version)
				return false;

			for (int i = 0; i < reader.AttributeCount; i++) {
				reader.MoveToAttribute (i);
				if (!TryParseAttribute (reader.LocalName, reader.NamespaceURI, reader.Value, version))
					AttributeExtensions.Add (new XmlQualifiedName (reader.LocalName, reader.NamespaceURI), reader.Value);
			}
			reader.MoveToElement ();

			if (!reader.IsEmptyElement) {
				reader.Read ();
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (reader.LocalName == "title" && reader.NamespaceURI == Namespaces.Atom10)
						Title = Atom10FeedFormatter.ReadTextSyndicationContent (reader);
					else
						ElementExtensions.Add (new SyndicationElementExtension (reader));
				}
			}
			reader.Read ();
			return true;
		}

		protected internal virtual void WriteAttributeExtensions (XmlWriter writer, string version)
		{
			extensions.WriteAttributeExtensions (writer, version);
		}

		protected internal virtual void WriteElementExtensions (XmlWriter writer, string version)
		{
			extensions.WriteElementExtensions (writer, version);
		}
	}
}
