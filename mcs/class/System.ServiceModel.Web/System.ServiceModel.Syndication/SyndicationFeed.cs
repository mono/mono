//
// SyndicationFeed.cs
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
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	public class SyndicationFeed : ISyndicationElement
	{
		public static SyndicationFeed Load (XmlReader reader)
		{
			return Load<SyndicationFeed> (reader);
		}

		public static TSyndicationFeed Load<TSyndicationFeed> (XmlReader reader) where TSyndicationFeed : SyndicationFeed, new()
		{
			return SyndicationVersions.LoadFeed<TSyndicationFeed> (reader);
		}

		SyndicationExtensions extensions = new SyndicationExtensions ();
		Collection<SyndicationCategory> categories;
		Collection<SyndicationPerson> authors, contributors;
		Collection<SyndicationLink> links;
		Uri base_uri;
		TextSyndicationContent copyright, title;
		string id;
		DateTimeOffset last_updated_time;

		IEnumerable<SyndicationItem> items;
		TextSyndicationContent description;
		string generator, language;
		Uri image_url;

		public SyndicationFeed ()
		{
		}

		public SyndicationFeed (IEnumerable<SyndicationItem> items)
		{
			if (items != null)
				Items = items;
		}

		public SyndicationFeed (string title, string description, Uri feedAlternateLink)
			: this (title, description, feedAlternateLink, null)
		{
		}

		public SyndicationFeed (string title, string description, Uri feedAlternateLink,
					IEnumerable<SyndicationItem> items)
			: this (title, description, feedAlternateLink, null, default (DateTimeOffset), items)
		{
		}

		public SyndicationFeed (string title, string description, Uri feedAlternateLink, string id,
					DateTimeOffset lastUpdatedTime)
			: this (title, description, feedAlternateLink, id, lastUpdatedTime, null)
		{
		}

		public SyndicationFeed (string title, string description, Uri feedAlternateLink, string id,
					DateTimeOffset lastUpdatedTime, IEnumerable<SyndicationItem> items)
		{
			Title = title != null ? new TextSyndicationContent (title) : null;
			Description = description != null ? new TextSyndicationContent (description) : null;
			if (feedAlternateLink != null)
				Links.Add (SyndicationLink.CreateAlternateLink (feedAlternateLink));
			Id = id;
			LastUpdatedTime = lastUpdatedTime;
			if (items != null)
				Items = items;
		}

		protected SyndicationFeed (SyndicationFeed source, bool cloneItems)
		{
			extensions = source.extensions.Clone ();
			categories = source.categories == null ? null : new Collection<SyndicationCategory> (source.categories);
			authors = source.authors == null ? null : new Collection<SyndicationPerson> (source.authors);
			contributors = source.contributors == null ? null : new Collection<SyndicationPerson> (source.contributors);
			links = source.links == null ? null : new Collection<SyndicationLink> (source.links);
			base_uri = source.base_uri; // copy by reference !!
			copyright = source.copyright == null ? null : source.copyright.Clone () as TextSyndicationContent;
			title = source.title == null ? null : source.title.Clone () as TextSyndicationContent;
			id = source.id;
			last_updated_time = source.last_updated_time;

			description = source.description == null ? null : source.description.Clone () as TextSyndicationContent;
			generator = source.generator;
			image_url = source.image_url; // copy by reference !!
			language = source.language;

			if (cloneItems && source.items != null)
				items = new Collection<SyndicationItem> (new List<SyndicationItem> (source.items));
		}

		public Dictionary<XmlQualifiedName, string> AttributeExtensions {
			get { return extensions.Attributes; }
		}

		public SyndicationElementExtensionCollection ElementExtensions {
			get { return extensions.Elements; }
		}

		public Collection<SyndicationPerson> Authors {
			get {
				if (authors == null)
					authors = new Collection<SyndicationPerson> ();
				return authors;
			}
		}

		public Collection<SyndicationCategory> Categories {
			get {
				if (categories == null)
					categories = new Collection<SyndicationCategory> ();
				return categories;
			}
		}

		public Collection<SyndicationPerson> Contributors {
			get {
				if (contributors == null)
					contributors = new Collection<SyndicationPerson> ();
				return contributors;
			}
		}

		public Collection<SyndicationLink> Links {
			get {
				if (links == null)
					links = new Collection<SyndicationLink> ();
				return links;
			}
		}

		public Uri BaseUri {
			get { return base_uri; }
			set { base_uri = value; }
		}

		public TextSyndicationContent Copyright {
			get { return copyright; }
			set { copyright = value; }
		}

		public string Id {
			get { return id; }
			set { id = value; }
		}

		public DateTimeOffset LastUpdatedTime {
			get { return last_updated_time; }
			set { last_updated_time = value; }
		}

		public TextSyndicationContent Title {
			get { return title; }
			set { title = value; }
		}

		public IEnumerable<SyndicationItem> Items {
			get {
				if (items == null)
					items = new Collection<SyndicationItem> ();
				return items;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				items = value;
			}
		}

		public TextSyndicationContent Description {
			get { return description; }
			set { description = value; }
		}

		public string Generator {
			get { return generator; }
			set { generator = value; }
		}

		public Uri ImageUrl {
			get { return image_url; }
			set { image_url = value; }
		}

		public string Language {
			get { return language; }
			set { language = value; }
		}

		public virtual SyndicationFeed Clone (bool cloneItems)
		{
			return new SyndicationFeed (this, cloneItems);
		}

		protected internal virtual SyndicationCategory CreateCategory ()
		{
			return new SyndicationCategory ();
		}

		protected internal virtual SyndicationLink CreateLink ()
		{
			return new SyndicationLink ();
		}

		protected internal virtual SyndicationPerson CreatePerson ()
		{
			return new SyndicationPerson ();
		}

		protected internal virtual SyndicationItem CreateItem ()
		{
			return new SyndicationItem ();
		}

		public Atom10FeedFormatter GetAtom10Formatter ()
		{
			return new Atom10FeedFormatter (this);
		}

		public Rss20FeedFormatter GetRss20Formatter ()
		{
			return GetRss20Formatter (true);
		}

		public Rss20FeedFormatter GetRss20Formatter (bool serializeExtensionsAsAtom)
		{
			return new Rss20FeedFormatter (this, serializeExtensionsAsAtom);
		}

		public void SaveAsAtom10 (XmlWriter writer)
		{
			GetAtom10Formatter ().WriteTo (writer);
		}

		public void SaveAsRss20 (XmlWriter writer)
		{
			GetRss20Formatter ().WriteTo (writer);
		}

		protected internal virtual bool TryParseAttribute (string name, string ns, string value, string version)
		{
			return false;
		}

		protected internal virtual bool TryParseElement (XmlReader reader, string version)
		{
			return false;
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
