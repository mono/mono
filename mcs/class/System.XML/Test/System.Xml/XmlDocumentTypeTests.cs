//
// System.Xml.XmlDocumentTypeTests.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
// Author: Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Ximian, Inc.
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlDocumentTypeTests
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

		internal void XmlNodeBaseProperties (XmlNode original, XmlNode cloned)
		{
//			assertequals (original.nodetype + " was incorrectly cloned.",
//				      original.baseuri, cloned.baseuri);			

			Assert.IsNull (cloned.ParentNode);
			Assert.AreEqual (original.Value, cloned.Value, "Value incorrectly cloned");

                        Assert.IsTrue (!Object.ReferenceEquals (original, cloned), "Copies, not pointers");
		}

		[Test]
		public void Name ()
		{
			Assert.AreEqual ("book", docType.Name, "Getting Name property");
		}

		[Test]
		public void LocalName ()
		{
			Assert.AreEqual ("book", docType.LocalName, "Getting LocalName property");
		}

		[Test]
		public void InternalSubset ()
		{
			Assert.AreEqual ("<!ELEMENT book ANY>", docType.InternalSubset, "Getting Internal Subset property");
		}

		[Test]
		public void AppendChild ()
		{
			try {
				XmlDocumentType type1 = document.CreateDocumentType ("book", null, null, null);
				document.AppendChild (type1);

			} catch (InvalidOperationException) {
				return;

			} catch (Exception) {				
				Assert.Fail ("Incorrect Exception thrown.");
			}
		}

		[Test]
		public void NodeType ()
		{
			Assert.AreEqual (docType.NodeType.ToString (), "DocumentType", "NodeType property broken");
		}
		
		[Test]
		public void IsReadOnly ()
		{
			Assert.AreEqual ("True", docType.IsReadOnly.ToString (), "IsReadOnly property");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void IncorrectInternalSubset ()
		{
			XmlDocument doc = new XmlDocument ();
			XmlDocumentType doctype = doc.CreateDocumentType (
				"root", "public-hogehoge", null,
				"invalid_intsubset");
			doctype = doc.CreateDocumentType ("root",
				"public-hogehoge", null, 
				"<!ENTITY % pe1 '>'> <!ELEMENT e EMPTY%pe1;");
		}

		[Test]
		public void CloneNode ()
		{
			XmlNode original = docType;

			XmlNode cloned1 = docType.CloneNode (true);
			XmlNodeBaseProperties (original, cloned1);

			XmlNode cloned2 = docType.CloneNode (false);
			XmlNodeBaseProperties (original, cloned2);

			Assert.AreEqual (cloned1.Value, cloned2.Value, "Deep and shallow cloning");
		}
	       
	}
}
