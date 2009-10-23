//
// SyndicationVersions.cs
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
	public static class SyndicationVersions
	{
		private enum ReaderKind
		{
			Item,
			Feed,
		}

		public const string Atom10 = "Atom10";
		public const string Rss20 = "Rss20";

		const string AtomNamespace ="http://www.w3.org/2005/Atom";

		static string DetectVersion (XmlReader reader, ReaderKind kind)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			reader.MoveToContent ();
			if (reader.NodeType != XmlNodeType.Element)
				throw new XmlException ("An element is expected for syndication item");
			if (reader.IsStartElement ("rss", String.Empty) && reader.GetAttribute ("version") == "2.0")
				return SyndicationVersions.Rss20;
			if ((kind == ReaderKind.Item && reader.IsStartElement ("entry", AtomNamespace)) ||
			    (kind == ReaderKind.Feed && reader.IsStartElement("feed", AtomNamespace)))
				return SyndicationVersions.Atom10;
			else if (reader.IsStartElement ("item", String.Empty))
				return SyndicationVersions.Rss20;
			else
				throw new XmlException (String.Format ("Unexpected syndication item element: name is '{0}' and namespace is '{1}'", reader.LocalName, reader.NamespaceURI));
		}

		internal static TSyndicationFeed LoadFeed<TSyndicationFeed> (XmlReader reader) where TSyndicationFeed : SyndicationFeed, new()
		{
			switch (DetectVersion (reader, ReaderKind.Feed)) {
			case SyndicationVersions.Atom10:
				Atom10FeedFormatter af = new Atom10FeedFormatter<TSyndicationFeed> ();
				af.ReadFrom (reader);
				return (TSyndicationFeed) af.Feed;
			case SyndicationVersions.Rss20:
			default: // anything else are rejected by DetectVersion
				Rss20FeedFormatter rf = new Rss20FeedFormatter<TSyndicationFeed> ();
				rf.ReadFrom (reader);
				return (TSyndicationFeed) rf.Feed;
			}
		}

		internal static TSyndicationItem LoadItem<TSyndicationItem> (XmlReader reader) where TSyndicationItem : SyndicationItem, new()
		{
			switch (DetectVersion (reader, ReaderKind.Item)) {
			case SyndicationVersions.Atom10:
				Atom10ItemFormatter af = new Atom10ItemFormatter<TSyndicationItem> ();
				af.ReadFrom (reader);
				return (TSyndicationItem) af.Item;
			case SyndicationVersions.Rss20:
			default: // anything else are rejected by DetectVersion
				Rss20ItemFormatter rf = new Rss20ItemFormatter<TSyndicationItem> ();
				rf.ReadFrom (reader);
				return (TSyndicationItem) rf.Item;
			}
		}
	}
}
