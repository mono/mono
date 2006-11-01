//
// System.Xml.XmlEntityTests.cs
//
// Author: Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2003 Atsushi Enomoto
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlEntityTests : Assertion
	{
		XmlDocument document;
		XmlDocumentType docType;

		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
			docType = document.CreateDocumentType ("book", null, null, "<!ELEMENT book ANY>");
			document.AppendChild (docType);
		}

		[Test]
		public void TestValue ()
		{
			XmlTextReader xtr = new XmlTextReader ("<!DOCTYPE x:foo [<!ENTITY foo 'fooent'><!ENTITY bar 'test &foo;'>]><x:foo xmlns:x='hoge' />", XmlNodeType.Document, null);
			document.Load (xtr);
			xtr.Close ();
			docType = document.DocumentType;
			AssertEquals (2, docType.Entities.Count);
			XmlEntity foo = docType.Entities.Item (0) as XmlEntity;
			XmlEntity bar = docType.Entities.Item (1) as XmlEntity;
			AssertEquals ("foo", foo.Name);
			AssertNull (bar.Value);
			AssertEquals (1, foo.ChildNodes.Count);
			AssertEquals ("bar", bar.Name);
			AssertNull (bar.Value);
			AssertEquals (1, foo.ChildNodes.Count);
		}
	       
	}
}
