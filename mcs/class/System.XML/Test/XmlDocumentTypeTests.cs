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

			Assertion.AssertNull (cloned.ParentNode);
			Assertion.AssertEquals ("Value incorrectly cloned",
				      original.Value, cloned.Value);

                        Assertion.Assert ("Copies, not pointers", !Object.ReferenceEquals (original, cloned));
		}

		[Test]
		public void Name ()
		{
			Assertion.AssertEquals ("Getting Name property", "book", docType.Name);
		}

		[Test]
		public void LocalName ()
		{
			Assertion.AssertEquals ("Getting LocalName property", "book", docType.LocalName);
		}

		[Test]
		public void InternalSubset ()
		{
			Assertion.AssertEquals ("Getting Internal Subset property",
				      "<!ELEMENT book ANY>", docType.InternalSubset);
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
				Assertion.Fail ("Incorrect Exception thrown.");
			}
		}

		[Test]
		public void NodeType ()
		{
			Assertion.AssertEquals ("NodeType property broken",
				      docType.NodeType.ToString (), "DocumentType");
		}
		
		[Test]
		public void IsReadOnly ()
		{
			Assertion.AssertEquals ("IsReadOnly property", "True", docType.IsReadOnly.ToString ());
		}

		[Test]
		public void CloneNode ()
		{
			XmlNode original = docType;

			XmlNode cloned1 = docType.CloneNode (true);
			XmlNodeBaseProperties (original, cloned1);

			XmlNode cloned2 = docType.CloneNode (false);
			XmlNodeBaseProperties (original, cloned2);

			Assertion.AssertEquals ("Deep and shallow cloning", cloned1.Value, cloned2.Value);
		}
	       
	}
}
