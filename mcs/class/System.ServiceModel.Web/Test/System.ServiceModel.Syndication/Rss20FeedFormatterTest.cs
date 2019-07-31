//
// Rss20FeedFormatterTest.cs
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
using System.Xml.Serialization;
using System.ServiceModel.Syndication;
using NUnit.Framework;

using QName = System.Xml.XmlQualifiedName;

namespace MonoTests.System.ServiceModel.Syndication
{
	[TestFixture]
	public class Rss20FeedFormatterTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullFeed ()
		{
			new Rss20FeedFormatter ((SyndicationFeed) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullType ()
		{
			new Rss20FeedFormatter ((Type) null);
		}

		/*
		[Test]
		public void FeedType ()
		{
			Rss20FeedFormatter f = new Rss20FeedFormatter ();
			Assert.IsNull (f.FeedType, "#1");
			f = new Rss20FeedFormatter (new SyndicationFeed ());
			Assert.IsNull (f.FeedType, "#2");
		}
		*/

		[Test]
		public void Version ()
		{
			Rss20FeedFormatter f = new Rss20FeedFormatter ();
			Assert.AreEqual ("Rss20", f.Version, "#1");
		}

		[Test]
		public void SerializeExtensionsAsAtom ()
		{
			Rss20FeedFormatter f = new Rss20FeedFormatter ();
			Assert.IsTrue (f.SerializeExtensionsAsAtom, "#1");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DefaultConstructorThenWriteXml ()
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Rss20FeedFormatter ().WriteTo (w);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WriteToNull ()
		{
			SyndicationFeed feed = new SyndicationFeed ();
			new Rss20FeedFormatter (feed).WriteTo (null);
		}

		[Test]
		public void WriteTo_EmptyFeed ()
		{
			SyndicationFeed feed = new SyndicationFeed ();
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Rss20FeedFormatter (feed).WriteTo (w);
			// either title or description must exist (RSS 2.0 spec)
			Assert.AreEqual ("<rss xmlns:a10=\"http://www.w3.org/2005/Atom\" version=\"2.0\"><channel><title /><description /></channel></rss>", sw.ToString ());
		}

		[Test]
		public void WriteTo_TitleOnlyFeed ()
		{
			SyndicationFeed feed = new SyndicationFeed ();
			feed.Title = new TextSyndicationContent ("title text");
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Rss20FeedFormatter (feed).WriteTo (w);
			Assert.AreEqual ("<rss xmlns:a10=\"http://www.w3.org/2005/Atom\" version=\"2.0\"><channel><title>title text</title><description /></channel></rss>", sw.ToString ());
		}

		[Test]
		public void WriteTo_CategoryAuthorsContributors ()
		{
			SyndicationFeed feed = new SyndicationFeed ();
			feed.Categories.Add (new SyndicationCategory ("myname", "myscheme", "mylabel"));
			feed.Authors.Add (new SyndicationPerson ("john@doe.com", "John Doe", "http://john.doe.name"));
			feed.Contributors.Add (new SyndicationPerson ("jane@doe.com", "Jane Doe", "http://jane.doe.name"));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Rss20FeedFormatter (feed).WriteTo (w);
			// contributors are serialized as Atom extension
			Assert.AreEqual ("<rss xmlns:a10=\"http://www.w3.org/2005/Atom\" version=\"2.0\"><channel><title /><description /><managingEditor>john@doe.com</managingEditor><category domain=\"myscheme\">myname</category><a10:contributor><a10:name>Jane Doe</a10:name><a10:uri>http://jane.doe.name</a10:uri><a10:email>jane@doe.com</a10:email></a10:contributor></channel></rss>", sw.ToString ());
		}

		[Test]
		[Category("NotWorking")]
		public void WriteTo ()
		{
			SyndicationFeed feed = new SyndicationFeed ();
			feed.BaseUri = new Uri ("http://example.com");
			feed.Copyright = new TextSyndicationContent ("No rights reserved");
			feed.Generator = "mono test generator";
			feed.Id = "urn:myid";
			feed.ImageUrl = new Uri ("http://example.com/images/mono.png");
			feed.LastUpdatedTime = new DateTimeOffset (DateTime.SpecifyKind (new DateTime (2008, 1, 1), DateTimeKind.Utc));

			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Rss20FeedFormatter (feed).WriteTo (w);
			Assert.AreEqual ("<rss xmlns:a10=\"http://www.w3.org/2005/Atom\" version=\"2.0\"><channel xml:base=\"http://example.com/\"><title /><description /><copyright>No rights reserved</copyright><lastBuildDate>Tue, 01 Jan 2008 00:00:00 Z</lastBuildDate><generator>mono test generator</generator><image><url>http://example.com/images/mono.png</url><title /><link /></image><a10:id>urn:myid</a10:id></channel></rss>", sw.ToString ());
		}

		[Test]
		public void SerializeExtensionsAsAtomFalse ()
		{
			SyndicationFeed feed = new SyndicationFeed ();
			feed.Contributors.Add (new SyndicationPerson ("jane@doe.com", "Jane Doe", "http://jane.doe.name"));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Rss20FeedFormatter (feed, false).WriteTo (w);
			// skip contributors
			Assert.AreEqual ("<rss version=\"2.0\"><channel><title /><description /></channel></rss>", sw.ToString ());
		}

		[Test]
		public void ISerializableWriteXml ()
		{
			SyndicationFeed feed = new SyndicationFeed ();
			feed.Title = new TextSyndicationContent ("title text");
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw)) {
				w.WriteStartElement ("dummy");
				((IXmlSerializable) new Rss20FeedFormatter (feed)).WriteXml (w);
				w.WriteEndElement ();
			}
			Assert.AreEqual ("<dummy xmlns:a10=\"http://www.w3.org/2005/Atom\" version=\"2.0\"><channel><title>title text</title><description /></channel></dummy>", sw.ToString ());
		}

		XmlWriter CreateWriter (StringWriter sw)
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			return XmlWriter.Create (sw, s);
		}

		XmlReader CreateReader (string xml)
		{
			return XmlReader.Create (new StringReader (xml));
		}

		[Test]
		public void CanRead ()
		{
			Rss20FeedFormatter f = new Rss20FeedFormatter ();
			Assert.IsTrue (f.CanRead (CreateReader ("<rss>")), "#1");
			Assert.IsFalse (f.CanRead (CreateReader ("<item>")), "#2");
			Assert.IsFalse (f.CanRead (CreateReader ("<rss xmlns='urn:foo'>")), "#3");
			Assert.IsFalse (f.CanRead (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'>")), "#4");
			Assert.IsFalse (f.CanRead (CreateReader ("<hoge>")), "#5");
			XmlReader r = CreateReader ("<rss></rss>");
			r.Read (); // element
			r.Read (); // endelement
			Assert.IsFalse (f.CanRead (r), "#6");

			r = CreateReader ("<rss><channel><title>test</title></channel></rss>");
			r.Read (); // feed
			r.Read (); // channel
			Assert.IsFalse (f.CanRead (r), "#7");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadFromInvalid ()
		{
			new Rss20FeedFormatter ().ReadFrom (CreateReader ("<item>"));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ReadFrom_Versionless ()
		{
			Rss20FeedFormatter f = new Rss20FeedFormatter ();
			Assert.IsNull (f.Feed, "#1");
			f.ReadFrom (CreateReader ("<rss>"));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ReadXml_Versionless ()
		{
			Rss20FeedFormatter f = new Rss20FeedFormatter ();
			((IXmlSerializable) f).ReadXml (CreateReader ("<dummy></dummy>"));
		}

		[Test]
		public void ReadFrom1 ()
		{
			Rss20FeedFormatter f = new Rss20FeedFormatter ();
			Assert.IsNull (f.Feed, "#1");
			f.ReadFrom (CreateReader ("<rss version='2.0'><channel><title>test</title></channel></rss>"));
			SyndicationFeed feed1 = f.Feed;
			Assert.IsNotNull (f.Feed.Title, "#2");
			Assert.AreEqual ("test", f.Feed.Title.Text, "#3");
			f.ReadFrom (CreateReader ("<rss version='2.0'><channel><title>test</title></channel></rss>"));
			Assert.IsFalse (object.ReferenceEquals (feed1, f.Feed), "#4");
		}

		[Test]
		public void ReadFrom_SyndicationFeed () {
			SyndicationFeed f = SyndicationFeed.Load (CreateReader ("<rss version='2.0'><channel><title>test</title></channel></rss>"));
			Assert.IsNotNull (f.Title);
		}


		[Test]
		public void ReadXml_TitleOnly ()
		{
			Rss20FeedFormatter f = new Rss20FeedFormatter ();
			((IXmlSerializable) f).ReadXml (CreateReader ("<rss version='2.0'><channel><title>test</title></channel></rss>"));
			Assert.IsNotNull (f.Feed.Title, "#1");
			Assert.AreEqual ("test", f.Feed.Title.Text, "#2");

			((IXmlSerializable) f).ReadXml (CreateReader ("<dummy version='2.0'><channel><title>test</title></channel></dummy>")); // it is ok
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadXmlFromContent ()
		{
			((IXmlSerializable) new Rss20FeedFormatter ()).ReadXml (CreateReader ("<channel version='2.0'><title>test</title></channel>"));
		}

		[Test]
		public void ReadXml_Extension ()
		{
			new Rss20FeedFormatter<MySyndicationFeed1> ().ReadFrom (CreateReader ("<rss version='2.0'><channel><foo>test</foo></channel></rss>"));
			new Rss20FeedFormatter<MySyndicationFeed2> ().ReadFrom (CreateReader ("<rss version='2.0'><channel><foo>test</foo></channel></rss>"));
			try {
				new Rss20FeedFormatter<MySyndicationFeed3> ().ReadFrom (CreateReader ("<rss version='2.0'><channel><foo>test</foo></channel></rss>"));
				Assert.Fail ("should trigger TryParseElement");
			} catch (ApplicationException) {
			}
		}

		class MySyndicationFeed1 : SyndicationFeed
		{
			protected override bool TryParseElement (XmlReader reader, string version)
			{
				Assert.AreEqual ("Rss20", version, "#1");
				Assert.IsFalse (base.TryParseElement (reader, version), "#2");
				return false;
			}
		}

		class MySyndicationFeed2 : SyndicationFeed
		{
			protected override bool TryParseElement (XmlReader reader, string version)
			{
				reader.Skip (); // without it, the caller expects that the reader did not proceed.
				return true;
			}
		}

		class MySyndicationFeed3 : SyndicationFeed
		{
			protected override bool TryParseElement (XmlReader reader, string version)
			{
				throw new ApplicationException ();
			}
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		[Category("NotWorking")]
		public void ReadFrom_WrongDate1 ()
		{
			Rss20FeedFormatter f = new Rss20FeedFormatter ();
			f.ReadFrom (CreateReader ("<rss version='2.0'><channel><lastBuildDate /></channel></rss>"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadFrom_WrongDate2 ()
		{
			Rss20FeedFormatter f = new Rss20FeedFormatter ();
			f.ReadFrom (CreateReader ("<rss version='2.0'><channel><lastBuildDate>2000-01-01T00:00:00</lastBuildDate></rss></channel>"));
		}

		[Test]
		public void ReadFrom_Docs ()
		{
			Rss20FeedFormatter f = new Rss20FeedFormatter ();
			f.ReadFrom (CreateReader ("<rss version='2.0'><channel><docs>documents</docs></channel></rss>"));
			Assert.IsNotNull (f.Feed, "#1");
			// 'docs' is treated as extensions ...
			Assert.AreEqual (1, f.Feed.ElementExtensions.Count, "#2");
		}

		[Test]
		public void GetSchema ()
		{
			Assert.IsNull (((IXmlSerializable) new Rss20FeedFormatter ()).GetSchema ());
		}

		[Test]
		public void ReadFrom_Feed () {
			string feed =
			@"<rss version=""2.0"" xmlns:a10=""http://www.w3.org/2005/Atom""><channel><title>My Blog Feed</title><link>http://someuri/</link><description>This is a how to sample that demonstrates how to expose a feed using RSS with WCF</description><managingEditor>someone@microsoft.com</managingEditor><category>How To Sample Code</category><item><guid isPermaLink=""false"">ItemOneID</guid><link>http://localhost/Content/One</link><title>Item One</title><description>This is the content for item one</description><a10:updated>2008-06-02T10:13:13+03:00</a10:updated></item><item><guid isPermaLink=""false"">ItemTwoID</guid><link>http://localhost/Content/Two</link><title>Item Two</title><description>This is the content for item two</description><a10:updated>2008-06-02T10:13:13+03:00</a10:updated></item><item><guid isPermaLink=""false"">ItemThreeID</guid><link>http://localhost/Content/three</link><title>Item Three</title><description>This is the content for item three</description><a10:updated>2008-06-02T10:13:13+03:00</a10:updated></item></channel></rss>";
			Rss20FeedFormatter f = new Rss20FeedFormatter ();
			f.ReadFrom (CreateReader (feed));
			Assert.IsNotNull (f.Feed, "#1");
		}
	}
}
#endif