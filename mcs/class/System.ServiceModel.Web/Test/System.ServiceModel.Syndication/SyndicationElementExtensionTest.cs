//
// SyndicationElementExtensionTest.cs
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

namespace MonoTests.System.ServiceModel.Syndication
{
	[TestFixture]
	public class SyndicationElementExtensionTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorExtensionNull ()
		{
			new SyndicationElementExtension ((object) null);
		}

		[Test]
		public void ConstructorXmlObjectSerializerNull ()
		{
			// null XmlObjectSerializer is allowed.
			var x = new SyndicationElementExtension (5, (XmlObjectSerializer) null);
			Assert.AreEqual ("int", x.OuterName, "#1");
			Assert.AreEqual ("http://schemas.microsoft.com/2003/10/Serialization/", x.OuterNamespace, "#2");
		}

		[Test]
		public void ConstructorOuterNameNull ()
		{
			// null name strings are allowed.
			var x = new SyndicationElementExtension (null, null, 5, null);
			Assert.AreEqual ("int", x.OuterName, "#1");
			Assert.AreEqual ("http://schemas.microsoft.com/2003/10/Serialization/", x.OuterNamespace, "#2");
		}

		[Test]
		public void ConstructorXmlSerializerNull ()
		{
			// null XmlSerializer is allowed.
			new SyndicationElementExtension (5, (XmlSerializer) null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ConstructorXmlSerializerNonSerializable ()
		{
			new SyndicationElementExtension (new NonXmlSerializable (null), (XmlSerializer) null);
		}

		public class NonXmlSerializable
		{
			public NonXmlSerializable (string dummy)
			{
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorXmlReaderNull ()
		{
			new SyndicationElementExtension ((XmlReader) null);
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ConstructorXmlReaderUnexpectedEndElement ()
		{
			string xml = "<root></root>";
			XmlReader r = XmlReader.Create (new StringReader (xml));
			r.Read ();
			r.Read (); // at end element
			new SyndicationElementExtension (r);
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ConstructorXmlReaderUnexpectedText ()
		{
			string xml = "<root>2</root>";
			XmlReader r = XmlReader.Create (new StringReader (xml));
			r.Read ();
			r.Read (); // at text
			new SyndicationElementExtension (r);
		}

		[Test]
		public void ConstructorXmlReader ()
		{
			string xml = "<root xmlns='x'>2</root>";
			XmlReader r = XmlReader.Create (new StringReader (xml));
			var e = new SyndicationElementExtension (r);
			Assert.AreEqual ("root", e.OuterName, "#1");
			Assert.AreEqual ("x", e.OuterNamespace, "#2");
		}

		XmlWriter GetWriter (StringWriter sw)
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			return XmlWriter.Create (sw, s);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WriteTo_NullWriter ()
		{
			new SyndicationElementExtension (5).WriteTo (null);
		}

		[Test]
		[Category("NotWorking")]
		public void WriteTo_Reader ()
		{
			string xml = "<root><child /><child2 /></root>";
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = GetWriter (sw)) {
				XmlReader r = XmlReader.Create (new StringReader (xml));
				r.Read ();
				r.Read (); // at child
				new SyndicationElementExtension (r).WriteTo (w);
			}
			Assert.AreEqual ("<child></child>", sw.ToString ());
		}

		[Test]
		[Category("NotWorking")]
		public void WriteToTwice_Reader ()
		{
			string xml = "<root><child /><child2 /></root>";
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = GetWriter (sw)) {
				XmlReader r = XmlReader.Create (new StringReader (xml));
				r.Read ();
				r.Read (); // at child
				SyndicationElementExtension x = new SyndicationElementExtension (r);
				w.WriteStartElement ("root");
				x.WriteTo (w);
				x.WriteTo (w); // it is VALID.
				w.WriteEndElement ();
			}
			Assert.AreEqual ("<root><child></child><child></child></root>", sw.ToString ());
		}

		[Test]
		public void WriteTo_ExtensionObject ()
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = GetWriter (sw))
				new SyndicationElementExtension (5).WriteTo (w);
			Assert.AreEqual ("<int xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">5</int>", sw.ToString ());
		}

		[Test]
		public void GetObject_DataContract ()
		{
			new SyndicationElementExtension (5).GetObject<int> ();
			new SyndicationElementExtension (5).GetObject<double> ();
			new SyndicationElementExtension ("5").GetObject<int> ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetObject_DataContract_XmlObjectSerializerNull ()
		{
			new SyndicationElementExtension (5).GetObject<int> ((XmlObjectSerializer) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetObject_DataContract_XmlSerializerNull ()
		{
			new SyndicationElementExtension (5).GetObject<int> ((XmlSerializer) null);
		}

		[Test]
		public void GetObject_XmlReader ()
		{
			string xml = "<root>3</root>";
			XmlReader r = XmlReader.Create (new StringReader (xml));
			SyndicationElementExtension x = new SyndicationElementExtension (r);
			Assert.AreEqual (3, x.GetObject<int> (), "#1");
			Assert.AreEqual (3, x.GetObject<int> (), "#2"); // it is VALID
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		[Category("NotWorking")]
		public void GetObject_DataContractError ()
		{
			new SyndicationElementExtension (DBNull.Value).GetObject<int> (); // Nullable<int> as well
		}
	}
}
#endif