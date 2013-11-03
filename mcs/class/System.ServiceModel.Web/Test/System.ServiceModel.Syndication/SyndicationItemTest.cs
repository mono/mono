//
// SyndicationItemTest.cs
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
#if !MOBILE
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.ServiceModel.Syndication;
using NUnit.Framework;

using QName = System.Xml.XmlQualifiedName;

namespace MonoTests.System.ServiceModel.Syndication
{
	[TestFixture]
	public class SyndicationItemTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddPermalinkNull ()
		{
			SyndicationItem item = new SyndicationItem ();
			item.AddPermalink (null);
		}

		[Test]
		public void SetNullForProperties ()
		{
			SyndicationItem item = new SyndicationItem ();
			item.BaseUri = null;
			item.Copyright = null;
			item.Content = null;
			item.Id = null;
			item.SourceFeed = null;
			item.Summary = null;
			item.Title = null;
		}

		[Test]
		public void LastUpdatedTimeBeforePublishDate ()
		{
			SyndicationItem item = new SyndicationItem ();
			item.PublishDate = new DateTimeOffset (new DateTime (2007, 12, 10));
			item.LastUpdatedTime = new DateTimeOffset (new DateTime (2007, 12, 9));
		}

		[Test]
		public void AddPermalink ()
		{
			SyndicationItem item = new SyndicationItem ();
			Assert.AreEqual (0, item.Links.Count, "#1");
			item.AddPermalink (new Uri ("http://mono-project.com/index.rss.20071210"));
			Assert.AreEqual (1, item.Links.Count, "#2");
			SyndicationLink link = item.Links [0];
			Assert.AreEqual ("http://mono-project.com/index.rss.20071210", link.Uri.ToString (), "#3");
			Assert.AreEqual ("alternate", link.RelationshipType, "#4");
		}

		[Test]
		public void Clone ()
		{
			SyndicationItem item = new SyndicationItem ();
			item.AddPermalink (new Uri ("http://mono-project.com/index.rss.20071210"));
			item.Id = Guid.NewGuid ().ToString ();
			item.BaseUri = new Uri ("http://mono-project.com");
			item.Authors.Add (new SyndicationPerson ("atsushi@ximian.com"));
			item.SourceFeed = new SyndicationFeed ();
			item.SourceFeed.Items = new SyndicationItem [] {new SyndicationItem ()};

			SyndicationItem clone = item.Clone ();
			Assert.AreEqual (1, clone.Links.Count, "#1");
			Assert.AreEqual (item.Id, clone.Id, "#2"); // hmm ...
			Assert.AreEqual ("http://mono-project.com/", clone.BaseUri.ToString (), "#3");

			// LAMESPEC: .NET fails to clone it
			// Assert.IsFalse (Object.ReferenceEquals (item.BaseUri, clone.BaseUri), "#4"); // should not be just a shallow copy

			Assert.IsNull (clone.Title, "#5");
			Assert.IsNotNull (clone.SourceFeed, "#6");
			Assert.IsFalse (Object.ReferenceEquals (item.SourceFeed, clone.SourceFeed), "#7"); // ... not just a shallow copy??
			// items in the SourceFeed are not cloned, but Items property is not null
			Assert.IsNotNull (clone.SourceFeed.Items, "#8-1");
			Assert.IsFalse (clone.SourceFeed.Items.GetEnumerator ().MoveNext (), "#8-2");
			Assert.AreEqual (1, clone.Authors.Count, "#9");
			SyndicationPerson person = clone.Authors [0];
			Assert.IsFalse (Object.ReferenceEquals (item.Authors [0], person), "#10"); // should not be just a shallow copy
			Assert.AreEqual ("atsushi@ximian.com", person.Email, "#11");
		}

		[Test]
		public void GetRss20Formatter ()
		{
			SyndicationItem item = new SyndicationItem ();
			Rss20ItemFormatter f = item.GetRss20Formatter ();
			Assert.AreEqual (true, f.SerializeExtensionsAsAtom, "#1");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void LoadNonAtomRss ()
		{
			SyndicationItem.Load (XmlReader.Create (new StringReader ("<dummy />")));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void LoadFeed ()
		{
			// feed is not allowed.
			SyndicationItem.Load (XmlReader.Create (new StringReader ("<feed xmlns=\"http://www.w3.org/2005/Atom\"></feed>")));
		}

		[Test]
		public void LoadEntry ()
		{
			SyndicationItem.Load (XmlReader.Create (new StringReader ("<entry xmlns=\"http://www.w3.org/2005/Atom\"></entry>")));
		}
	}
}
#endif