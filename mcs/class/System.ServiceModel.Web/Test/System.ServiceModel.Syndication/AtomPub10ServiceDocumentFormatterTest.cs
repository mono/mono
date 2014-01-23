//
// AtomPub10ServiceDocumentFormatterTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
	class MyServiceFormatter : AtomPub10ServiceDocumentFormatter
	{
		public void Read (XmlReader reader)
		{
			ReadFrom (reader);
		}

		public void CallWriteDocument ()
		{
			var w = XmlWriter.Create (TextWriter.Null);
			ServiceDocumentFormatter.WriteElementExtensions (w, new MyDocument (), "http://www.w3.org/2007/app");
		}
	}

	class MyDocument : ServiceDocument
	{
		protected override void WriteElementExtensions (XmlWriter writer, string version)
		{
			throw new ApplicationException ();
		}
	}

	[TestFixture]
	public class AtomPub10ServiceDocumentFormatterTest
	{
		static XmlWriterSettings settings = new XmlWriterSettings () { OmitXmlDeclaration = true};

		string app1 = "<app:service xmlns:a10=\"http://www.w3.org/2005/Atom\" xmlns:app=\"http://www.w3.org/2007/app\" />";

		[Test]
		public void WriteTo ()
		{
			var s = new ServiceDocument ();
			var a = new AtomPub10ServiceDocumentFormatter (s);
			Assert.AreEqual ("http://www.w3.org/2007/app", a.Version, "#1");
			Assert.IsTrue (a.CanRead (XmlReader.Create (new StringReader (app1))), "#2");
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw, settings))
				a.WriteTo (xw);
			Assert.AreEqual (app1, sw.ToString (), "#3");
		}

		string app2 = "<app:service xmlns:a10=\"http://www.w3.org/2005/Atom\" xmlns:app=\"http://www.w3.org/2007/app\"><app:workspace><a10:title type=\"text\">test title</a10:title><app:collection href=\"urn:foo\"><a10:title type=\"text\">test resource</a10:title><app:accept>application/atom+xml;type=entry</app:accept></app:collection></app:workspace></app:service>";

		[Test]
		public void WriteTo2 ()
		{
			var s = new ServiceDocument ();
			var ws = new Workspace ("test title", null);
			var rc = new ResourceCollectionInfo ("test resource", new Uri ("urn:foo"));
			rc.Accepts.Add ("application/atom+xml;type=entry");
			ws.Collections.Add (rc);
			s.Workspaces.Add (ws);
			var a = new AtomPub10ServiceDocumentFormatter (s);
			Assert.AreEqual ("http://www.w3.org/2007/app", a.Version, "#1");
			Assert.IsTrue (a.CanRead (XmlReader.Create (new StringReader (app2))), "#2");
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw, settings))
				a.WriteTo (xw);
			Assert.AreEqual (app2, sw.ToString (), "#3");
		}

		[Test]
		[ExpectedException (typeof (XmlException))] // insufficient content
		public void Load ()
		{
			ServiceDocument.Load (XmlReader.Create (new StringReader (app1)));
		}

		[Test]
		public void Load2 ()
		{
			ServiceDocument.Load (XmlReader.Create (new StringReader (app2)));
		}

		[Test]
		public void ReadFrom ()
		{
			new MyServiceFormatter ().Read (XmlReader.Create (new StringReader (app2)));
		}

		[Test]
		public void GetSchema ()
		{
			IXmlSerializable i = new AtomPub10ServiceDocumentFormatter ();
			Assert.IsNull (i.GetSchema ());
		}

		[Test]
		[ExpectedException (typeof (ApplicationException))]
		public void WriteElementExtensions ()
		{
			// this test is to verify that the overriden WriteElementExtensions() is called in the staic ServiceDocumentFormatter method.
			new MyServiceFormatter ().CallWriteDocument ();
		}
	}
}
#endif