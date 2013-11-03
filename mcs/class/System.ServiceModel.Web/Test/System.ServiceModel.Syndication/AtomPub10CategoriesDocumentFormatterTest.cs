//
// AtomPub10CategoriesDocumentFormatterTest.cs
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
	[TestFixture]
	public class AtomPub10CategoriesDocumentFormatterTest
	{
		static XmlWriterSettings settings = new XmlWriterSettings () { OmitXmlDeclaration = true};

		class MyFormatter : AtomPub10CategoriesDocumentFormatter
		{
			public InlineCategoriesDocument CreateInline ()
			{
				return CreateInlineCategoriesDocument ();
			}

			public ReferencedCategoriesDocument CreateReferenced ()
			{
				return CreateReferencedCategoriesDocument ();
			}
		}

		[Test]
		public void CreateInstances ()
		{
			var f = new MyFormatter ();
			Assert.IsTrue (f.CreateInline () is InlineCategoriesDocument, "#1");
			Assert.IsTrue (f.CreateReferenced () is ReferencedCategoriesDocument, "#2");
		}


		[Test]
		public void WriteTo ()
		{
			var doc = new InlineCategoriesDocument ();
			var f = new AtomPub10CategoriesDocumentFormatter (doc);
			doc.Scheme = "http";
			doc.IsFixed = false;
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw, settings))
				f.WriteTo (xw);
			Assert.AreEqual (app1, sw.ToString ());
		}

		string app1 = "<app:categories xmlns:a10=\"http://www.w3.org/2005/Atom\" scheme=\"http\" xmlns:app=\"http://www.w3.org/2007/app\" />";

		[Test]
		public void WriteTo2 ()
		{
			var doc = new InlineCategoriesDocument ();
			var f = new AtomPub10CategoriesDocumentFormatter (doc);
			doc.Categories.Add (new SyndicationCategory ("TEST CATEGORY"));
			var sw = new StringWriter ();
			using (var xw = XmlWriter.Create (sw, settings))
				f.WriteTo (xw);
			Assert.AreEqual (app2, sw.ToString ());
		}

		string app2 = "<app:categories xmlns:a10=\"http://www.w3.org/2005/Atom\" xmlns:app=\"http://www.w3.org/2007/app\"><a10:category term=\"TEST CATEGORY\" /></app:categories>";

		[Test]
		public void ReadFrom ()
		{
			var f = new AtomPub10CategoriesDocumentFormatter ();
			f.ReadFrom (XmlReader.Create (new StringReader (app1)));
			var inline = f.Document as InlineCategoriesDocument;
			Assert.IsNotNull (inline, "#1");
			Assert.AreEqual ("http", inline.Scheme, "#2");
		}

		[Test]
		public void ReadFrom2 ()
		{
			var f = new AtomPub10CategoriesDocumentFormatter ();
			f.ReadFrom (XmlReader.Create (new StringReader (app2)));
			var inline = f.Document as InlineCategoriesDocument;
			Assert.IsNotNull (inline, "#1");
			Assert.IsNull (inline.Scheme, "#2");
			Assert.AreEqual (1, inline.Categories.Count, "#3");
		}

		[Test]
		public void GetSchema ()
		{
			IXmlSerializable i = new AtomPub10CategoriesDocumentFormatter ();
			Assert.IsNull (i.GetSchema ());
		}
	}
}
#endif