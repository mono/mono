//
// Atom10ItemFormatterTest.cs
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
	public class Atom10ItemFormatterTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullItem ()
		{
			new Atom10ItemFormatter ((SyndicationItem) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullType ()
		{
			new Atom10ItemFormatter ((Type) null);
		}

		/*
		[Test]
		public void ItemType ()
		{
			Atom10ItemFormatter f = new Atom10ItemFormatter ();
			Assert.IsNull (f.ItemType, "#1");
			f = new Atom10ItemFormatter (new SyndicationItem ());
			Assert.IsNull (f.ItemType, "#2");
		}
		*/

		[Test]
		public void Version ()
		{
			Atom10ItemFormatter f = new Atom10ItemFormatter ();
			Assert.AreEqual ("Atom10", f.Version, "#1");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DefaultConstructorThenWriteXml ()
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Atom10ItemFormatter ().WriteTo (w);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WriteToNull ()
		{
			SyndicationItem item = new SyndicationItem ();
			new Atom10ItemFormatter (item).WriteTo (null);
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
		public void WriteTo_EmptyItem ()
		{
			// It however automatically fills id (very likely bug in .NET) and DateTimeOffset though.
			SyndicationItem item = new SyndicationItem ();
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Atom10ItemFormatter (item).WriteTo (w);
			Assert.IsNull (item.Id, "#1"); // automatically generated, but not automatically set.
			using (XmlWriter w = CreateWriter (sw))
				new Atom10ItemFormatter (item).WriteTo (w);
			Assert.AreEqual ("<entry xmlns=\"http://www.w3.org/2005/Atom\"><id>XXX</id><title type=\"text\"></title><updated>XXX</updated></entry>", DummyUpdated (DummyId (sw.ToString ())));
		}

		[Test]
		public void WriteTo_TitleOnlyItem ()
		{
			// It however automatically fills id (very likely bug in .NET) and DateTimeOffset though.
			SyndicationItem item = new SyndicationItem ();
			item.Title = new TextSyndicationContent ("title text");
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Atom10ItemFormatter (item).WriteTo (w);
			Assert.AreEqual ("<entry xmlns=\"http://www.w3.org/2005/Atom\"><id>XXX</id><title type=\"text\">title text</title><updated>XXX</updated></entry>", DummyUpdated (DummyId (sw.ToString ())));
		}

		[Test]
		public void WriteTo_CategoryAuthorsContributors ()
		{
			// It however automatically fills ...
			SyndicationItem item = new SyndicationItem ();
			item.Categories.Add (new SyndicationCategory ("myname", "myscheme", "mylabel"));
			item.Authors.Add (new SyndicationPerson ("john@doe.com", "John Doe", "http://john.doe.name"));
			item.Contributors.Add (new SyndicationPerson ("jane@doe.com", "Jane Doe", "http://jane.doe.name"));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Atom10ItemFormatter (item).WriteTo (w);
			// contributors are serialized as Atom extension
			Assert.AreEqual ("<entry xmlns=\"http://www.w3.org/2005/Atom\"><id>XXX</id><title type=\"text\"></title><updated>XXX</updated><author><name>John Doe</name><uri>http://john.doe.name</uri><email>john@doe.com</email></author><contributor><name>Jane Doe</name><uri>http://jane.doe.name</uri><email>jane@doe.com</email></contributor><category term=\"myname\" label=\"mylabel\" scheme=\"myscheme\" /></entry>", DummyUpdated (DummyId (sw.ToString ())));
		}

		[Test]
		public void WriteTo ()
		{
			SyndicationItem item = new SyndicationItem ();
			item.BaseUri = new Uri ("http://mono-project.com");
			item.Copyright = new TextSyndicationContent ("No rights reserved");
			item.Content = new XmlSyndicationContent (null, 5, (XmlObjectSerializer) null);
			// .NET bug: it ignores this value.
			item.Id = "urn:myid";
			item.PublishDate = new DateTimeOffset (DateTime.SpecifyKind (new DateTime (2000, 1, 1), DateTimeKind.Utc));
			item.LastUpdatedTime = new DateTimeOffset (DateTime.SpecifyKind (new DateTime (2008, 1, 1), DateTimeKind.Utc));
			//item.SourceFeed = new SyndicationFeed ();
			item.Summary = new TextSyndicationContent ("great text");

			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Atom10ItemFormatter (item).WriteTo (w);
			Assert.AreEqual ("<entry xml:base=\"http://mono-project.com/\" xmlns=\"http://www.w3.org/2005/Atom\"><id>XXX</id><title type=\"text\"></title><summary type=\"text\">great text</summary><published>2000-01-01T00:00:00Z</published><updated>2008-01-01T00:00:00Z</updated><content type=\"text/xml\"><int xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">5</int></content><rights type=\"text\">No rights reserved</rights></entry>", DummyId (sw.ToString ()));
		}

		[Test]
		public void ISerializableWriteXml ()
		{
			SyndicationItem item = new SyndicationItem ();
			item.Title = new TextSyndicationContent ("title text");
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw)) {
				w.WriteStartElement ("dummy");
				((IXmlSerializable) new Atom10ItemFormatter (item)).WriteXml (w);
				w.WriteEndElement ();
			}
			Assert.AreEqual ("<dummy><id>XXX</id><title type=\"text\" xmlns=\"http://www.w3.org/2005/Atom\">title text</title><updated>XXX</updated></dummy>", DummyUpdated2 (DummyId2 (sw.ToString ())));
		}

		[Test]
		public void WriteTo_IllegalDuplicateAltLinks ()
		{
			// ... and it passes.
			SyndicationItem item = new SyndicationItem ();
			item.Links.Add (new SyndicationLink (new Uri ("http://mono-project.com/Page1"), "alternate", "Page 1", "text/html", 0));
			item.Links.Add (new SyndicationLink (new Uri ("http://mono-project.com/Page2"), "alternate", "Page 2", "text/html", 0));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Atom10ItemFormatter (item).WriteTo (w);
			Assert.AreEqual ("<entry xmlns=\"http://www.w3.org/2005/Atom\"><id>XXX</id><title type=\"text\"></title><updated>XXX</updated><link rel=\"alternate\" type=\"text/html\" title=\"Page 1\" href=\"http://mono-project.com/Page1\" /><link rel=\"alternate\" type=\"text/html\" title=\"Page 2\" href=\"http://mono-project.com/Page2\" /></entry>", DummyUpdated (DummyId (sw.ToString ())));
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
			Atom10ItemFormatter f = new Atom10ItemFormatter ();
			Assert.IsFalse (f.CanRead (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom'>")), "#1");
			Assert.IsTrue (f.CanRead (CreateReader ("<entry xmlns='http://www.w3.org/2005/Atom'>")), "#2");
			Assert.IsFalse (f.CanRead (CreateReader ("<entry>")), "#3");
			Assert.IsFalse (f.CanRead (CreateReader ("<item>")), "#4");
			Assert.IsFalse (f.CanRead (CreateReader ("<hoge xmlns='http://www.w3.org/2005/Atom'>")), "#5");
			XmlReader r = CreateReader ("<entry xmlns='http://www.w3.org/2005/Atom'></entry>");
			r.Read (); // element
			r.Read (); // endelement
			Assert.IsFalse (f.CanRead (r), "#6");

			r = CreateReader ("<entry xmlns='http://www.w3.org/2005/Atom'><title>test</title></entry>");
			r.Read (); // item
			r.Read (); // title
			Assert.IsFalse (f.CanRead (r), "#7");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadFromInvalid ()
		{
			new Atom10ItemFormatter ().ReadFrom (CreateReader ("<feed xmlns='http://www.w3.org/2005/Atom' />"));
		}

		[Test]
		public void ReadFrom1 ()
		{
			Atom10ItemFormatter f = new Atom10ItemFormatter ();
			Assert.IsNull (f.Item, "#1");
			f.ReadFrom (CreateReader ("<entry xmlns='http://www.w3.org/2005/Atom'><title>test</title></entry>"));
			SyndicationItem item1 = f.Item;
			Assert.IsNotNull (f.Item.Title, "#2");
			Assert.AreEqual ("test", f.Item.Title.Text, "#3");
			f.ReadFrom (CreateReader ("<entry xmlns='http://www.w3.org/2005/Atom'><title>test</title></entry>"));
			Assert.IsFalse (object.ReferenceEquals (item1, f.Item), "#4");
		}

		[Test]
		public void ReadXml_TitleOnly ()
		{
			Atom10ItemFormatter f = new Atom10ItemFormatter ();
			((IXmlSerializable) f).ReadXml (CreateReader ("<entry xmlns='http://www.w3.org/2005/Atom'><title>test</title></entry>"));
			Assert.IsNotNull (f.Item.Title, "#1");
			Assert.AreEqual ("test", f.Item.Title.Text, "#2");

			((IXmlSerializable) f).ReadXml (CreateReader ("<dummy><title>test</title></dummy>")); // it is ok
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadXmlFromContent ()
		{
			((IXmlSerializable) new Atom10ItemFormatter ()).ReadXml (CreateReader ("<title xmlns='http://www.w3.org/2005/Atom'>test</title>"));
		}

		[Test]
		public void ReadXml_Extension ()
		{
			new Atom10ItemFormatter<MySyndicationItem1> ().ReadFrom (CreateReader ("<entry xmlns='http://www.w3.org/2005/Atom'><foo>test</foo></entry>"));
			new Atom10ItemFormatter<MySyndicationItem2> ().ReadFrom (CreateReader ("<entry xmlns='http://www.w3.org/2005/Atom'><foo>test</foo></entry>"));
			try {
				new Atom10ItemFormatter<MySyndicationItem3> ().ReadFrom (CreateReader ("<entry xmlns='http://www.w3.org/2005/Atom'><foo>test</foo></entry>"));
				Assert.Fail ("should trigger TryParseElement");
			} catch (ApplicationException) {
			}
		}

		class MySyndicationItem1 : SyndicationItem
		{
			protected override bool TryParseElement (XmlReader reader, string version)
			{
				Assert.AreEqual ("Atom10", version, "#1");
				Assert.IsFalse (base.TryParseElement (reader, version), "#2");
				return false;
			}
		}

		class MySyndicationItem2 : SyndicationItem
		{
			protected override bool TryParseElement (XmlReader reader, string version)
			{
				reader.Skip (); // without it, the caller expects that the reader did not proceed.
				return true;
			}
		}

		class MySyndicationItem3 : SyndicationItem
		{
			protected override bool TryParseElement (XmlReader reader, string version)
			{
				throw new ApplicationException ();
			}
		}

		[Test]
		// It is not rejected. Though I think it is .NET bug.
		public void ReadFrom_EmptyDate ()
		{
			Atom10ItemFormatter f = new Atom10ItemFormatter ();
			f.ReadFrom (CreateReader ("<entry xmlns='http://www.w3.org/2005/Atom'><pubDate /></entry>"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadFrom_WrongDate ()
		{
			Atom10ItemFormatter f = new Atom10ItemFormatter ();
			f.ReadFrom (CreateReader ("<entry xmlns='http://www.w3.org/2005/Atom'><published>Sat, 01 Jan 2000 00:00:00 Z</pubDate></entry>"));
		}

		[Test]
		public void ReadFrom_Extension ()
		{
			Atom10ItemFormatter f = new Atom10ItemFormatter ();
			f.ReadFrom (CreateReader ("<entry xmlns='http://www.w3.org/2005/Atom'><ext>external</ext></entry>"));
			Assert.IsNotNull (f.Item, "#1");
			Assert.AreEqual (1, f.Item.ElementExtensions.Count, "#2");
		}

		[Test]
		public void ReadFrom_Id ()
		{
			Atom10ItemFormatter f = new Atom10ItemFormatter ();
			f.ReadFrom (CreateReader ("<entry xmlns='http://www.w3.org/2005/Atom'><id>urn:myid</id></entry>"));
			Assert.IsNotNull (f.Item, "#1");
			Assert.AreEqual ("urn:myid", f.Item.Id, "#2");
		}

		[Test]
		public void ReadFrom_Link ()
		{
			Atom10ItemFormatter f = new Atom10ItemFormatter ();

			f.ReadFrom (CreateReader ("<entry xmlns='http://www.w3.org/2005/Atom'><link href='urn:foo' rel='enclosure' length='50' type='text/html' wcf='wtf'><extended /></link></entry>"));

			Assert.AreEqual (1, f.Item.Links.Count, "#1");
			SyndicationLink link = f.Item.Links [0];
			Assert.AreEqual (50, link.Length, "#2");
			Assert.AreEqual ("urn:foo", link.Uri.ToString (), "#3");
			Assert.AreEqual ("text/html", link.MediaType, "#4");
			Assert.AreEqual ("enclosure", link.RelationshipType, "#5");
			Assert.AreEqual (1, link.AttributeExtensions.Count, "#6");
			Assert.AreEqual (1, link.ElementExtensions.Count, "#7");
		}

		[Test]
		public void GetSchema ()
		{
			Assert.IsNull (((IXmlSerializable) new Atom10ItemFormatter ()).GetSchema ());
		}

		[Test]
		public void TestToString ()
		{
			Assert.AreEqual (typeof (Atom10ItemFormatter).FullName + ", SyndicationVersion=Atom10", new Atom10ItemFormatter (new SyndicationItem ()).ToString ());
		}
	}
}
#endif