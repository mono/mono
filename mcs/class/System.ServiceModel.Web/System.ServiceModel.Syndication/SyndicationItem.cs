//
// SyndicationItem.cs
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
	public class SyndicationItem : ISyndicationElement
	{
		public static SyndicationItem Load (XmlReader reader)
		{
			return Load<SyndicationItem> (reader);
		}

		public static TSyndicationItem Load<TSyndicationItem> (XmlReader reader) where TSyndicationItem : SyndicationItem, new()
		{
			return SyndicationVersions.LoadItem<TSyndicationItem> (reader);
		}

		SyndicationExtensions extensions = new SyndicationExtensions ();
		Collection<SyndicationCategory> categories;
		Collection<SyndicationPerson> authors, contributors;
		Collection<SyndicationLink> links;
		Uri base_uri;
		TextSyndicationContent copyright, summary, title;
		SyndicationContent content;
		string id;
		DateTimeOffset last_updated_time, published_date;
		SyndicationFeed source_feed;

		public SyndicationItem ()
		{
		}

		public SyndicationItem (string title, string content, Uri feedAlternateLink)
			: this (title, content, feedAlternateLink, null, default (DateTimeOffset))
		{
		}

		public SyndicationItem (string title, string content, Uri feedAlternateLink, string id,
					DateTimeOffset lastUpdatedTime)
			: this (title, content != null ? SyndicationContent.CreatePlaintextContent (content) : null, feedAlternateLink, id, lastUpdatedTime)
		{
		}

		public SyndicationItem (string title, SyndicationContent content, Uri feedAlternateLink, string id,
					DateTimeOffset lastUpdatedTime)
		{
			Title = title != null ? new TextSyndicationContent (title) : null;
			Content = content;
			if (feedAlternateLink != null)
				AddPermalink (feedAlternateLink);
			Id = id;
			LastUpdatedTime = lastUpdatedTime;
		}

		protected SyndicationItem (SyndicationItem source)
		{
			extensions = source.extensions.Clone ();
			categories = Copy<SyndicationCategory> (source.categories);
			authors = Copy<SyndicationPerson> (source.authors);
			contributors = Copy<SyndicationPerson> (source.contributors);
			links = Copy<SyndicationLink> (source.links);
			base_uri = source.base_uri; // copy by reference !!
			copyright = source.copyright == null ? null : source.copyright.Clone () as TextSyndicationContent;
			summary = source.summary == null ? null : source.summary.Clone () as TextSyndicationContent;
			title = source.title == null ? null : source.title.Clone () as TextSyndicationContent;
			content = source.content == null ? null : source.content.Clone ();
			id = source.id;
			last_updated_time = source.last_updated_time;
			published_date = source.published_date;
			source_feed = source.source_feed == null ? null : source.source_feed.Clone (false);
		}

		Collection<T> Copy<T> (Collection<T> source)
		{
			if (source == null)
				return source;
			Collection<T> ret = new Collection<T> ();
			foreach (object item in source)
				ret.Add ((T) CreateCopy (item));
			return ret;
		}

		object CreateCopy (object source)
		{
			if (source == null)
				return null;
			if (source is SyndicationPerson)
				return ((SyndicationPerson) source).Clone ();
			if (source is SyndicationLink)
				return ((SyndicationLink) source).Clone ();
			if (source is SyndicationCategory)
				return ((SyndicationCategory) source).Clone ();
			throw new InvalidOperationException ();
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

		public SyndicationContent Content {
			get { return content; }
			set { content = value; }
		}

		public string Id {
			get { return id; }
			set { id = value; }
		}

		public DateTimeOffset LastUpdatedTime {
			get { return last_updated_time; }
			set { last_updated_time = value; }
		}

		public DateTimeOffset PublishDate {
			get { return published_date; }
			set { published_date = value; }
		}

		public SyndicationFeed SourceFeed {
			get { return source_feed; }
			set { source_feed = value; }
		}

		public TextSyndicationContent Summary {
			get { return summary; }
			set { summary = value; }
		}

		public TextSyndicationContent Title {
			get { return title; }
			set { title = value; }
		}

		public void AddPermalink (Uri link)
		{
			if (link == null)
				throw new ArgumentNullException ("link");
			Links.Add (SyndicationLink.CreateAlternateLink (link));
		}

		public virtual SyndicationItem Clone ()
		{
			return new SyndicationItem (this);
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

		public Atom10ItemFormatter GetAtom10Formatter ()
		{
			return new Atom10ItemFormatter (this);
		}

		public Rss20ItemFormatter GetRss20Formatter ()
		{
			return GetRss20Formatter (true);
		}

		public Rss20ItemFormatter GetRss20Formatter (bool serializeExtensionsAsAtom)
		{
			return new Rss20ItemFormatter (this, serializeExtensionsAsAtom);
		}

		public void SaveAsAtom10 (XmlWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			GetAtom10Formatter ().WriteTo (writer);
		}

		public void SaveAsRss20 (XmlWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			GetRss20Formatter ().WriteTo (writer);
		}

		protected internal virtual bool TryParseContent (XmlReader reader, string contentType, string version, out SyndicationContent content)
		{
			content = null;
			return false;
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
