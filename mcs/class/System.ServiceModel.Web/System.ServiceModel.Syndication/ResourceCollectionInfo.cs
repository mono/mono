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
			AttributeExtensions = new Dictionary<XmlQualifiedName, string> ();
			Categories = new Collection<CategoriesDocument> ();
			ElementExtensions = new SyndicationElementExtensionCollection ();
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

		public Collection<string> Accepts { get; private set; }

		public Dictionary<XmlQualifiedName, string> AttributeExtensions { get; private set; }

		public Uri BaseUri { get; set; }

		public Collection<CategoriesDocument> Categories { get; private set; }

		public SyndicationElementExtensionCollection ElementExtensions { get; private set; }

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

		[MonoTODO]
		protected internal virtual bool TryParseAttribute (string name, string ns, string value, string version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual bool TryParseElement (XmlReader reader, string version)
		{
			throw new NotImplementedException ();
		}

		protected internal virtual void WriteAttributeExtensions (XmlWriter writer, string version)
		{
			Utility.WriteElementExtensions (ElementExtensions, writer, version);
		}
	}
}
