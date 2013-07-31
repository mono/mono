//
// Rss20ItemFormatterTest.cs
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
	public class Rss20ItemFormatterTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullItem ()
		{
			new Rss20ItemFormatter ((SyndicationItem) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullType ()
		{
			new Rss20ItemFormatter ((Type) null);
		}

		/*
		[Test]
		public void ItemType ()
		{
			Rss20ItemFormatter f = new Rss20ItemFormatter ();
			Assert.IsNull (f.ItemType, "#1");
			f = new Rss20ItemFormatter (new SyndicationItem ());
			Assert.IsNull (f.ItemType, "#2");
		}
		*/

		[Test]
		public void Version ()
		{
			Rss20ItemFormatter f = new Rss20ItemFormatter ();
			Assert.AreEqual ("Rss20", f.Version, "#1");
		}

		[Test]
		public void SerializeExtensionsAsAtom ()
		{
			Rss20ItemFormatter f = new Rss20ItemFormatter ();
			Assert.IsTrue (f.SerializeExtensionsAsAtom, "#1");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DefaultConstructorThenWriteXml ()
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Rss20ItemFormatter ().WriteTo (w);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WriteToNull ()
		{
			SyndicationItem item = new SyndicationItem ();
			new Rss20ItemFormatter (item).WriteTo (null);
		}

		[Test]
		public void WriteTo_EmptyItem ()
		{
			SyndicationItem item = new SyndicationItem ();
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Rss20ItemFormatter (item).WriteTo (w);
			// either title or description must exist (RSS 2.0 spec)
			Assert.AreEqual ("<item><description /></item>", sw.ToString ());
		}

		[Test]
		public void WriteTo_TitleOnlyItem ()
		{
			SyndicationItem item = new SyndicationItem ();
			item.Title = new TextSyndicationContent ("title text");
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Rss20ItemFormatter (item).WriteTo (w);
			Assert.AreEqual ("<item><title>title text</title></item>", sw.ToString ());
		}

		[Test]
		public void WriteTo_CategoryAuthorsContributors ()
		{
			SyndicationItem item = new SyndicationItem ();
			item.Categories.Add (new SyndicationCategory ("myname", "myscheme", "mylabel"));
			item.Authors.Add (new SyndicationPerson ("john@doe.com", "John Doe", "http://john.doe.name"));
			item.Contributors.Add (new SyndicationPerson ("jane@doe.com", "Jane Doe", "http://jane.doe.name"));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Rss20ItemFormatter (item).WriteTo (w);
			// contributors are serialized as Atom extension
			Assert.AreEqual ("<item><author>john@doe.com</author><category domain=\"myscheme\">myname</category><description /><contributor xmlns=\"http://www.w3.org/2005/Atom\"><name>Jane Doe</name><uri>http://jane.doe.name</uri><email>jane@doe.com</email></contributor></item>", sw.ToString ());
		}

		[Test]
		[Category("NotWorking")]
		public void WriteTo ()
		{
			SyndicationItem item = new SyndicationItem ();
			item.BaseUri = new Uri ("http://mono-project.com");
			item.Copyright = new TextSyndicationContent ("No rights reserved");
			item.Content = new XmlSyndicationContent (null, 5, (XmlObjectSerializer) null);
			item.Id = "urn:myid";
			item.PublishDate = new DateTimeOffset (DateTime.SpecifyKind (new DateTime (2000, 1, 1), DateTimeKind.Utc));
			item.LastUpdatedTime = new DateTimeOffset (DateTime.SpecifyKind (new DateTime (2008, 1, 1), DateTimeKind.Utc));
			item.SourceFeed = new SyndicationFeed ();
			item.SourceFeed.Title = new TextSyndicationContent ("source title");

			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Rss20ItemFormatter (item).WriteTo (w);
			Assert.AreEqual ("<item xml:base=\"http://mono-project.com/\"><guid isPermaLink=\"false\">urn:myid</guid><description /><source>source title</source><pubDate>Sat, 01 Jan 2000 00:00:00 Z</pubDate><updated xmlns=\"http://www.w3.org/2005/Atom\">2008-01-01T00:00:00Z</updated><rights type=\"text\" xmlns=\"http://www.w3.org/2005/Atom\">No rights reserved</rights><content type=\"text/xml\" xmlns=\"http://www.w3.org/2005/Atom\"><int xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">5</int></content></item>", sw.ToString ());
		}

		[Test]
		public void SerializeExtensionsAsAtomFalse ()
		{
			SyndicationItem item = new SyndicationItem ();
			item.Contributors.Add (new SyndicationPerson ("jane@doe.com", "Jane Doe", "http://jane.doe.name"));
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				new Rss20ItemFormatter (item, false).WriteTo (w);
			// skip contributors
			Assert.AreEqual ("<item><description /></item>", sw.ToString ());
		}

		[Test]
		public void ISerializableWriteXml ()
		{
			SyndicationItem item = new SyndicationItem ();
			item.Title = new TextSyndicationContent ("title text");
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				((IXmlSerializable) new Rss20ItemFormatter (item)).WriteXml (w);
			Assert.AreEqual ("<title>title text</title>", sw.ToString ());
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
			Rss20ItemFormatter f = new Rss20ItemFormatter ();
			Assert.IsFalse (f.CanRead (CreateReader ("<rss>")), "#1");
			Assert.IsTrue (f.CanRead (CreateReader ("<item>")), "#2");
			Assert.IsFalse (f.CanRead (CreateReader ("<item xmlns='urn:foo'>")), "#3");
			Assert.IsFalse (f.CanRead (CreateReader ("<entry xmlns='http://www.w3.org/2005/Atom'>")), "#4");
			Assert.IsFalse (f.CanRead (CreateReader ("<hoge>")), "#5");
			XmlReader r = CreateReader ("<item></item>");
			r.Read (); // element
			r.Read (); // endelement
			Assert.IsFalse (f.CanRead (r), "#6");

			r = CreateReader ("<item><title>test</title></item>");
			r.Read (); // item
			r.Read (); // title
			Assert.IsFalse (f.CanRead (r), "#7");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadFromInvalid ()
		{
			new Rss20ItemFormatter ().ReadFrom (CreateReader ("<rss>"));
		}

		[Test]
		public void ReadFrom1 ()
		{
			Rss20ItemFormatter f = new Rss20ItemFormatter ();
			Assert.IsNull (f.Item, "#1");
			f.ReadFrom (CreateReader ("<item><title>test</title></item>"));
			SyndicationItem item1 = f.Item;
			Assert.IsNotNull (f.Item.Title, "#2");
			Assert.AreEqual ("test", f.Item.Title.Text, "#3");
			f.ReadFrom (CreateReader ("<item><title>test</title></item>"));
			Assert.IsFalse (object.ReferenceEquals (item1, f.Item), "#4");
		}

		[Test]
		public void ReadFrom2 () {
			SyndicationItem item = new SyndicationItem ();
			Rss20ItemFormatter f = new Rss20ItemFormatter (item);
			Assert.IsTrue (object.ReferenceEquals (item, f.Item), "Item #1");
			f.ReadFrom (CreateReader ("<item><title>test</title></item>"));
			Assert.IsFalse (object.ReferenceEquals(item, f.Item), "Item #2");
		}


		[Test]
		public void ReadXml_TitleOnly ()
		{
			Rss20ItemFormatter f = new Rss20ItemFormatter ();
			((IXmlSerializable) f).ReadXml (CreateReader ("<item><title>test</title></item>"));
			Assert.IsNotNull (f.Item.Title, "#1");
			Assert.AreEqual ("test", f.Item.Title.Text, "#2");

			((IXmlSerializable) f).ReadXml (CreateReader ("<dummy><title>test</title></dummy>")); // it is ok
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadXmlFromContent ()
		{
			((IXmlSerializable) new Rss20ItemFormatter ()).ReadXml (CreateReader ("<title>test</title>"));
		}

		[Test]
		public void ReadXml_Extension ()
		{
			new Rss20ItemFormatter<MySyndicationItem1> ().ReadFrom (CreateReader ("<item><foo>test</foo></item>"));
			new Rss20ItemFormatter<MySyndicationItem2> ().ReadFrom (CreateReader ("<item><foo>test</foo></item>"));
			try {
				new Rss20ItemFormatter<MySyndicationItem3> ().ReadFrom (CreateReader ("<item><foo>test</foo></item>"));
				Assert.Fail ("should trigger TryParseElement");
			} catch (ApplicationException) {
			}
		}

		class MySyndicationItem1 : SyndicationItem
		{
			protected override bool TryParseElement (XmlReader reader, string version)
			{
				Assert.AreEqual ("Rss20", version, "#1");
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
		[ExpectedException (typeof (XmlException))]
		[Category("NotWorking")]
		public void ReadFrom_WrongDate1 ()
		{
			Rss20ItemFormatter f = new Rss20ItemFormatter ();
			f.ReadFrom (CreateReader ("<item><pubDate /></item>"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		[Category("NotWorking")]
		public void ReadFrom_WrongDate2 ()
		{
			Rss20ItemFormatter f = new Rss20ItemFormatter ();
			f.ReadFrom (CreateReader ("<item><pubDate>2000-01-01T00:00:00</pubDate></item>"));
		}

		[Test]
		public void ReadFrom_Comments ()
		{
			Rss20ItemFormatter f = new Rss20ItemFormatter ();
			f.ReadFrom (CreateReader ("<item><comments>comment</comments></item>"));
			Assert.IsNotNull (f.Item, "#1");
			// 'comments' is treated as extensions ...
			Assert.AreEqual (1, f.Item.ElementExtensions.Count, "#2");
		}

		[Test]
		public void ReadFrom_Enclosure ()
		{
			Rss20ItemFormatter f = new Rss20ItemFormatter ();

			// .NET bug: it allows extension attributes, but rejects extension elements.
//			f.ReadFrom (CreateReader ("<item><enclosure url='urn:foo' length='50' type='text/html' wcf='wtf'><extended /></enclosure></item>"));
			f.ReadFrom (CreateReader ("<item><enclosure url='urn:foo' length='50' type='text/html' wcf='wtf'></enclosure></item>"));

			// 'enclosure' is treated as SyndicationLink
			Assert.AreEqual (1, f.Item.Links.Count, "#1");
			SyndicationLink link = f.Item.Links [0];
			Assert.AreEqual (50, link.Length, "#2");
			Assert.AreEqual ("urn:foo", link.Uri.ToString (), "#3");
			Assert.AreEqual ("text/html", link.MediaType, "#4");
			Assert.AreEqual ("enclosure", link.RelationshipType, "#5");
			Assert.AreEqual (1, link.AttributeExtensions.Count, "#6");
			//Assert.AreEqual (1, link.ElementExtensions.Count, "#7");
		}

		[Test]
		public void GetSchema ()
		{
			Assert.IsNull (((IXmlSerializable) new Rss20ItemFormatter ()).GetSchema ());
		}
	}
}
#endif