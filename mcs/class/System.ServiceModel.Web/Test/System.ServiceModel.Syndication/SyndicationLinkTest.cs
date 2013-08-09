//
// SyndicationLinkTest.cs
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
	public class SyndicationLinkTest
	{
		[Test]
		public void Constructor ()
		{
			// null Uri is allowed
			SyndicationLink link = new SyndicationLink (null);

			link = new SyndicationLink (new Uri ("empty.xml", UriKind.Relative));
			Assert.AreEqual ("empty.xml", link.Uri.ToString (), "#1-1");
			Assert.AreEqual (null, link.BaseUri, "#1-2");
			Assert.AreEqual (0, link.Length, "#1-3");
			Assert.AreEqual (null, link.MediaType, "#1-4");

			link = new SyndicationLink (null, null, null, null, 0);
		}

		[Test]
		public void TestUri ()
		{
			SyndicationLink link = new SyndicationLink (new Uri ("empty.xml", UriKind.Relative));
			link.Uri = null;
			Assert.IsNull (link.Uri, "#1");
			Assert.IsNull (link.GetAbsoluteUri (), "#2");
		}

		[Test]
		public void TestBaseUri ()
		{
			// relative
			SyndicationLink link = new SyndicationLink (new Uri ("empty.xml", UriKind.Relative));
			Assert.IsNull (link.BaseUri, "#1");

			// absolute
			link = new SyndicationLink (new Uri ("http://mono-project.com/index.rss"));
			Assert.IsNull (link.BaseUri, "#2");

			// absolute #2
			link = new SyndicationLink ();
			link.Uri = new Uri ("http://mono-project.com/index.rss");
			Assert.IsNull (link.BaseUri, "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotDotNet")] // LAMESPEC. See below.
		public void SetRelativeUriAsBaseUri ()
		{
			SyndicationLink link = new SyndicationLink (new Uri ("empty.xml", UriKind.Relative));
			// LAMESPEC: setting relative Uri as BaseUri is allowed (likely broken)
			link.BaseUri = new Uri ("base.xml", UriKind.Relative);

			// and below causes ArgumentOutOfRangeException.
			// Assert.IsNull (link.GetAbsoluteUri (), "#1");
		}

		[Test]
		public void MediaType ()
		{
			SyndicationLink link = new SyndicationLink (new Uri ("http://mono-project.com/index.rss"));
			link.MediaType = "text/xml";
			Assert.AreEqual ("text/xml", link.MediaType, "#1");
			link.MediaType = null;
			Assert.IsNull (link.MediaType, "#2");
			link.MediaType = "WTF"; // no error
		}

		[Test]
		public void RelationshipType ()
		{
			SyndicationLink link = new SyndicationLink (new Uri ("http://mono-project.com/index.rss"));
			link.RelationshipType = "alternate";
			Assert.AreEqual ("alternate", link.RelationshipType, "#1");
			link.RelationshipType = null;
			Assert.IsNull (link.RelationshipType, "#2");
			link.RelationshipType = "WTF"; // no error
		}

		[Test]
		public void Length ()
		{
			SyndicationLink link = new SyndicationLink (new Uri ("http://mono-project.com/index.rss"));
			link.Length = 0;
			link.Length = long.MaxValue;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void NegativeLength ()
		{
			SyndicationLink link = new SyndicationLink (new Uri ("http://mono-project.com/index.rss"));
			link.Length = -1;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void NegativeLength2 ()
		{
			new SyndicationLink (null, null, null, null, -1);
		}

		[Test]
		public void AttributeElementExtensions ()
		{
			// The properties do not affect extension attributes.
			SyndicationLink link = new SyndicationLink (new Uri ("http://mono-project.com/index.rss"));

			Assert.AreEqual (0, link.ElementExtensions.Count, "#0");
			Assert.IsFalse (link.AttributeExtensions.ContainsKey (new QName ("mediaType")), "#3");
			link.MediaType = "text/xml";
		}

		[Test]
		public void GetAbsoluteUri ()
		{
			// no Uri
			SyndicationLink link = new SyndicationLink ();
			Assert.IsNull (link.GetAbsoluteUri (), "#1");

			// Uri is relative
			link = new SyndicationLink (new Uri ("empty.xml", UriKind.Relative));
			Assert.IsNull (link.GetAbsoluteUri (), "#2");

			// Uri is absolute
			link = new SyndicationLink (new Uri ("http://mono-project.com/index.rss"));
			Assert.AreEqual ("http://mono-project.com/index.rss", link.GetAbsoluteUri ().ToString (), "#3");

			// only BaseUri - null result
			link = new SyndicationLink ();
			link.BaseUri = new Uri ("http://mono-project.com/index.rss");
			Assert.IsNull (link.GetAbsoluteUri (), "#4");
		}

		[Test]
		public void Clone ()
		{
			SyndicationLink link = new SyndicationLink (null, null, "my RSS", "text/xml", 1);
			link.BaseUri = new Uri ("http://mono-project.com/index.rss");
			SyndicationLink clone = link.Clone ();
			Assert.AreEqual (link.BaseUri, clone.BaseUri, "#1");
			Assert.AreEqual ("my RSS", clone.Title, "#2");
			Assert.AreEqual ("text/xml", clone.MediaType, "#3");
			Assert.IsNull (clone.RelationshipType, "#4");
			Assert.IsNull (clone.Uri, "#5");
		}

		[Test]
		public void CreateAlternateLink ()
		{
			SyndicationLink link = SyndicationLink.CreateAlternateLink (null);
			Assert.IsNull (link.Uri, "#1");
			Assert.IsNull (link.MediaType, "#2");
			Assert.AreEqual ("alternate", link.RelationshipType, "#3");
		}

		[Test]
		public void CreateMediaEnclosureLink ()
		{
			SyndicationLink link = SyndicationLink.CreateMediaEnclosureLink (null, null, 1);
			Assert.IsNull (link.Uri, "#1");
			Assert.IsNull (link.MediaType, "#2");
			Assert.AreEqual (1, link.Length, "#3");
			Assert.AreEqual ("enclosure", link.RelationshipType, "#4");
		}

		[Test]
		public void CreateSelfLink ()
		{
			SyndicationLink link = SyndicationLink.CreateSelfLink (null);
			Assert.IsNull (link.Uri, "#1");
			Assert.IsNull (link.MediaType, "#2");
			Assert.AreEqual ("self", link.RelationshipType, "#3");
		}
	}
}
#endif