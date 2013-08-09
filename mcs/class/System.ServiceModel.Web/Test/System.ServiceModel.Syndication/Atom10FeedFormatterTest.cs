//
// Atom10FeedFormatterTest.cs
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
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.ServiceModel.Syndication;
using NUnit.Framework;

using QName = System.Xml.XmlQualifiedName;

namespace MonoTests.System.ServiceModel.Syndication
{
	[TestFixture]
	public class Atom10FeedFormatterTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullFeed ()
		{
			new Atom10FeedFormatter ((SyndicationFeed) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullType ()
		{
			new Atom10FeedFormatter ((Type) null);
		}

		/*
		[Test]
		public void FeedType ()
		{
			Atom10FeedFormatter f = new Atom10FeedFormatter ();
			Assert.IsNull (f.FeedType, "#1");
			f = new Atom10FeedFormatter (new SyndicationFeed ());
			Assert.IsNull (f.FeedType, "#2");
		}
		*/

		[Test]
		public void Version ()
		{
			Atom10FeedFormatter f = new Atom10FeedFormatter ();
			Assert.AreEqual ("Atom10", f.Version, "#1");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DefaultConstructorThenWriteXml ()
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Atom10FeedFormatter ().WriteTo (w);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WriteToNull ()
		{
			SyndicationFeed feed = new SyndicationFeed ();
			new Atom10FeedFormatter (feed).WriteTo (null);
		}

		string DummyId (string s)
		{
			return Regex.Replace (s, "<id>.+</id>", "<id>XXX</id>");
		}

		string DummyId2 (string s)
		{
			return Regex.Replace (s, "<id xmlns=\"http://www.w3.org/2005/Atom\">.+</id>", "<id>XXX</id>");
		}

		string DummyUpdated (string s)
		{
			return Regex.Replace (s, "<updated>.+</updated>", "<updated>XXX</updated>");
		}

		string DummyUpdated2 (string s)
		{
			return Regex.Replace (s, "<updated xmlns=\"http://www.w3.org/2005/Atom\">.+</updated>", "<updated>XXX</updated>");
		}

		[Test]
		public void WriteTo_EmptyFeed ()
		{
			// It however automatically fills id (very likely bug in .NET) and DateTimeOffset though.
			SyndicationFeed feed = new SyndicationFeed ();
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Atom10FeedFormatter (feed).WriteTo (w);
			Assert.IsNull (feed.Id, "#1"); // automatically generated, but not automatically set.
			using (XmlWriter w = CreateWriter (sw))
				new Atom10FeedFormatter (feed).WriteTo (w);
			Assert.AreEqual ("<feed xmlns=\"http://www.w3.org/2005/Atom\"><title type=\"text\"></title><id>XXX</id><updated>XXX</updated></feed>", DummyUpdated (DummyId (sw.ToString ())));
		}

		[Test]
		public void WriteTo_TitleOnlyFeed ()
		{
			// It however automatically fills id (very likely bug in .NET) and DateTimeOffset though.
			SyndicationFeed feed = new SyndicationFeed ();
			feed.Title = new TextSyndicationContent ("title text");
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Atom10FeedFormatter (feed).WriteTo (w);
			Assert.AreEqual ("<feed xmlns=\"http://www.w3.org/2005/Atom\"><title type=\"text\">title text</title><id>XXX</id><updated>XXX</updated></feed>", DummyUpdated (DummyId (sw.ToString ())));
		}

		[Test]
		public void WriteTo_CategoryAuthorsContributors ()
		{
			// It however automatically fills ...
			SyndicationFeed feed = new SyndicationFeed ();
			feed.Categories.Add (new SyndicationCategory ("myname", "myscheme", "mylabel"));
			feed.Authors.Add (new SyndicationPerson ("john@doe.com", "John Doe", "http://john.doe.name"));
			feed.Contributors.Add (new SyndicationPerson ("jane@doe.com", "Jane Doe", "http://jane.doe.name"));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Atom10FeedFormatter (feed).WriteTo (w);
			// contributors are serialized as Atom extension
			Assert.AreEqual ("<feed xmlns=\"http://www.w3.org/2005/Atom\"><title type=\"text\"></title><id>XXX</id><updated>XXX</updated><category term=\"myname\" label=\"mylabel\" scheme=\"myscheme\" /><author><name>John Doe</name><uri>http://john.doe.name</uri><email>john@doe.com</email></author><contributor><name>Jane Doe</name><uri>http://jane.doe.name</uri><email>jane@doe.com</email></contributor></feed>", DummyUpdated (DummyId (sw.ToString ())));
		}

		[Test]
		public void WriteTo ()
		{
			SyndicationFeed feed = new SyndicationFeed ();
			feed.BaseUri = new Uri ("http://mono-project.com");
			feed.Copyright = new TextSyndicationContent ("No rights reserved");
			feed.Description = new TextSyndicationContent ("A sample feed for unit testing");
			feed.Generator = "mono test generator";
			// .NET bug: it ignores this value.
			feed.Id = "urn:myid";
			feed.ImageUrl = new Uri ("http://mono-project.com/images/mono.png");
			feed.LastUpdatedTime = new DateTimeOffset (DateTime.SpecifyKind (new DateTime (2008, 1, 1), DateTimeKind.Utc));

			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Atom10FeedFormatter (feed).WriteTo (w);
			Assert.AreEqual ("<feed xml:base=\"http://mono-project.com/\" xmlns=\"http://www.w3.org/2005/Atom\"><title type=\"text\"></title><id>XXX</id><rights type=\"text\">No rights reserved</rights><updated>2008-01-01T00:00:00Z</updated><subtitle type=\"text\">A sample feed for unit testing</subtitle><logo>http://mono-project.com/images/mono.png</logo><generator>mono test generator</generator></feed>", DummyId (sw.ToString ()));
		}

		[Test]
		public void ISerializableWriteXml ()
		{
			SyndicationFeed feed = new SyndicationFeed ();
			feed.Title = new TextSyndicationContent ("title text");
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw)) {
				w.WriteStartElement ("dummy");
				((IXmlSerializable) new Atom10FeedFormatter (feed)).WriteXml (w);
				w.WriteEndElement ();
			}
			Assert.AreEqual ("<dummy><title type=\"text\" xmlns=\"http://www.w3.org/2005/Atom\">title text</title><id>XXX</id><updated>XXX</updated></dummy>", DummyUpdated2 (DummyId2 (sw.ToString ())));
		}

		[Test]
		public void WriteTo_IllegalDuplicateAltLinks ()
		{
			// ... and it passes.
			SyndicationFeed feed = new SyndicationFeed ();
			feed.Links.Add (new SyndicationLink (new Uri ("http://mono-project.com/Page1"), "alternate", "Page 1", "text/html", 0));
			feed.Links.Add (new SyndicationLink (new Uri ("http://mono-project.com/Page2"), "alternate", "Page 2", "text/html", 0));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Atom10FeedFormatter (feed).WriteTo (w);
			Assert.AreEqual ("<feed xmlns=\"http://www.w3.org/2005/Atom\"><title type=\"text\"></title><id>XXX</id><updated>XXX</updated><link rel=\"alternate\" type=\"text/html\" title=\"Page 1\" href=\"http://mono-project.com/Page1\" /><link rel=\"alternate\" type=\"text/html\" title=\"Page 2\" href=\"http://mono-project.com/Page2\" /></feed>", DummyUpdated (DummyId (sw.ToString ())));
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
			Atom10FeedFormatter f = new Atom10FeedFormatter ();
			Assert.IsTrue (f.CanRead (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'>")), "#1");
			Assert.IsFalse (f.CanRead (CreateReader ("<item xmlns='http://www.w3.org/2005/Atom'>")), "#2");
			Assert.IsFalse (f.CanRead (CreateReader ("<feed xmlns='urn:foo'>")), "#3");
			Assert.IsFalse (f.CanRead (CreateReader ("<feed>")), "#4");
			Assert.IsFalse (f.CanRead (CreateReader ("<hoge>")), "#5");
			XmlReader r = CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'></feed>");
			r.Read (); // element
			r.Read (); // endelement
			Assert.IsFalse (f.CanRead (r), "#6");

			r = CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'><title>test</title></feed>");
			r.Read (); // feed
			r.Read (); // channel
			Assert.IsFalse (f.CanRead (r), "#7");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadFromInvalid ()
		{
			new Atom10FeedFormatter ().ReadFrom (CreateReader ("<feed>"));
		}

		[Test]
		public void ReadFrom1 ()
		{
			Atom10FeedFormatter f = new Atom10FeedFormatter ();
			Assert.IsNull (f.Feed, "#1");
			f.ReadFrom (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'><title>test</title></feed>"));
			SyndicationFeed feed1 = f.Feed;
			Assert.IsNotNull (f.Feed.Title, "#2");
			Assert.AreEqual ("test", f.Feed.Title.Text, "#3");
			f.ReadFrom (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'><title>test</title></feed>"));
			Assert.IsFalse (object.ReferenceEquals (feed1, f.Feed), "#4");
		}

		[Test]
		public void ReadXml_TitleOnly ()
		{
			Atom10FeedFormatter f = new Atom10FeedFormatter ();
			((IXmlSerializable) f).ReadXml (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'><title>test</title></feed>"));
			Assert.IsNotNull (f.Feed.Title, "#1");
			Assert.AreEqual ("test", f.Feed.Title.Text, "#2");

			((IXmlSerializable) f).ReadXml (CreateReader ("<dummy  xmlns='http://www.w3.org/2005/Atom'><title>test</title></dummy>")); // it is ok
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadXmlFromContent ()
		{
			((IXmlSerializable) new Atom10FeedFormatter ()).ReadXml (CreateReader ("<title xmlns='http://www.w3.org/2005/Atom'>test</title>"));
		}

		[Test]
		public void ReadXml_Extension ()
		{
			new Atom10FeedFormatter<MySyndicationFeed1> ().ReadFrom (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'><foo>test</foo></feed>"));
			new Atom10FeedFormatter<MySyndicationFeed2> ().ReadFrom (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'><foo>test</foo></feed>"));
			try {
				new Atom10FeedFormatter<MySyndicationFeed3> ().ReadFrom (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'><foo>test</foo></feed>"));
				Assert.Fail ("should trigger TryParseElement");
			} catch (ApplicationException) {
			}
		}

		class MySyndicationFeed1 : SyndicationFeed
		{
			protected override bool TryParseElement (XmlReader reader, string version)
			{
				Assert.AreEqual ("Atom10", version, "#1");
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
		public void ReadFrom_EmptyDate ()
		{
			// strangely, it is checked while 'entry' is not checked
			Atom10FeedFormatter f = new Atom10FeedFormatter ();
			f.ReadFrom (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'><updated /></feed>"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		[Category("NotWorking")]
		public void ReadFrom_WrongDate ()
		{
			Atom10FeedFormatter f = new Atom10FeedFormatter ();
			f.ReadFrom (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'><updated>2000-01-01T00:00:00</updated></feed>"));
		}

		[Test]
		public void ReadFrom_Extension ()
		{
			Atom10FeedFormatter f = new Atom10FeedFormatter ();
			f.ReadFrom (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'><icon>http://www.mono-project.com/icons/mono.png</icon></feed>"));
			Assert.IsNotNull (f.Feed, "#1");
			// 'icon' is treated as an extension ...
			Assert.AreEqual (1, f.Feed.ElementExtensions.Count, "#2");
		}

		[Test]
		public void ReadFrom_Link ()
		{
			Atom10FeedFormatter f = new Atom10FeedFormatter ();

			f.ReadFrom (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'><link href='urn:foo' rel='enclosure' length='50' type='text/html' wcf='wtf'><extended /></link></feed>"));

			Assert.AreEqual (1, f.Feed.Links.Count, "#1");
			SyndicationLink link = f.Feed.Links [0];
			Assert.AreEqual (50, link.Length, "#2");
			Assert.AreEqual ("urn:foo", link.Uri.ToString (), "#3");
			Assert.AreEqual ("text/html", link.MediaType, "#4");
			Assert.AreEqual ("enclosure", link.RelationshipType, "#5");
			Assert.AreEqual (1, link.AttributeExtensions.Count, "#6");
			Assert.AreEqual (1, link.ElementExtensions.Count, "#7");
		}

		[Test]
		public void ReadFrom_ImageUrl ()
		{
			Atom10FeedFormatter f = new Atom10FeedFormatter ();
			f.ReadFrom (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'><logo>http://mono-project.com/images/mono.png</logo></feed>"));
			Assert.IsNotNull (f.Feed.ImageUrl, "#1");
			Assert.AreEqual ("http://mono-project.com/images/mono.png", f.Feed.ImageUrl.ToString (), "#2");
		}

		[Test]
		public void ReadFrom_Language ()
		{
			Atom10FeedFormatter f = new Atom10FeedFormatter ();
			f.ReadFrom (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom' xml:lang='ar-AR'></feed>"));
			Assert.AreEqual ("ar-AR", f.Feed.Language, "#1");
		}

		[Test]
		public void GetSchema ()
		{
			Assert.IsNull (((IXmlSerializable) new Atom10FeedFormatter ()).GetSchema ());
		}

		[Test]
		public void TestToString ()
		{
			Assert.AreEqual (typeof (Atom10FeedFormatter).FullName + ", SyndicationVersion=Atom10", new Atom10FeedFormatter (new SyndicationFeed ()).ToString ());
		}
	}
}
#endif