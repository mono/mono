//
// SyndicationFeedTest.cs
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
	public class SyndicationFeedTest
	{
		[Test]
		public void ConstructorItemsNull ()
		{
			Assert.IsNotNull (new SyndicationFeed (null).Items, "#1");
			Assert.IsNotNull (new SyndicationFeed (null, null, null, null).Items, "#2");
			Assert.IsNotNull (new SyndicationFeed (null, null, null, null, default (DateTimeOffset), null).Items, "#3");
		}

		[Test]
		public void SetNullForProperties ()
		{
			SyndicationFeed feed = new SyndicationFeed ();
			feed.BaseUri = null;
			feed.Copyright = null;
			feed.Id = null;
			feed.Title = null;

			feed.Description = null;
			feed.Generator = null;
			feed.ImageUrl = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetItemsNull ()
		{
			SyndicationFeed feed = new SyndicationFeed ();
			feed.Items = null;
		}

		[Test]
		public void Items ()
		{
			SyndicationFeed feed = new SyndicationFeed ();
			Assert.IsNotNull (feed.Items, "#1");
			feed.Items = new SyndicationItem  [] {new SyndicationItem ()};
			Assert.IsTrue (feed.Items.GetEnumerator ().MoveNext (), "#2");
			/*
			feed.Items = null;
			// even after setting null, it autofills a collection.
			Assert.IsNotNull (feed.Items, "#3");
			Assert.IsFalse (feed.Items.GetEnumerator ().MoveNext (), "#4"); // make sure we reset it
			*/
		}

		[Test]
		public void LoadFeed ()
		{
			SyndicationFeed.Load (XmlReader.Create (new StringReader ("<feed xmlns=\"http://www.w3.org/2005/Atom\"></feed>")));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void LoadEntry ()
		{
			// entry is not allowed.
			SyndicationFeed.Load (XmlReader.Create (new StringReader ("<entry xmlns=\"http://www.w3.org/2005/Atom\"></entry>")));
		}
	}
}
#endif