//
// System.Xml.XmlDocumentTypeTests.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Xml;

using NUnit.Framework;

namespace Ximian.Mono.Tests
{
	public class XmlDocumentTypeTests : TestCase
	{
		XmlDocument document;
		XmlDocumentType docType;
		public XmlDocumentTypeTests ()
			: base ("XmlDocumentTypeTests testsuite")
		{
		}

		public XmlDocumentTypeTests (string name)
			: base (name)
		{
		}

		protected override void SetUp ()
		{
			document = new XmlDocument ();
			docType = document.CreateDocumentType ("book", null, null, "<!ELEMENT book ANY>");
			document.AppendChild (docType);
		}

		internal void TestXmlNodeBaseProperties (XmlNode original, XmlNode cloned)
		{
//			assertequals (original.nodetype + " was incorrectly cloned.",
//				      original.baseuri, cloned.baseuri);			

			AssertNull (cloned.ParentNode);
			AssertEquals ("Value incorrectly cloned",
				      original.Value, cloned.Value);

                        Assert ("Copies, not pointers", !Object.ReferenceEquals (original, cloned));
		}

		public void TestName ()
		{
			AssertEquals ("Getting Name property", "book", docType.Name);
		}

		public void TestLocalName ()
		{
			AssertEquals ("Getting LocalName property", "book", docType.LocalName);
		}

		public void TestInternalSubset ()
		{
			AssertEquals ("Getting Internal Subset property",
				      "<!ELEMENT book ANY>", docType.InternalSubset);
		}

		public void TestAppendChild ()
		{
			try {
				XmlDocumentType type1 = document.CreateDocumentType ("book", null, null, null);
										     
				document.AppendChild (type1);
			} catch (Exception e) {
				AssertEquals ("Incorrect Exception thrown",
					      e.GetType (), Type.GetType ("System.InvalidOperationException"));
			}
		}

		public void TestNodeType ()
		{
			AssertEquals ("NodeType property broken",
				      docType.NodeType.ToString (), "DocumentType");
		}
		
		public void TestIsReadOnly ()
		{
			AssertEquals ("IsReadOnly property", "True", docType.IsReadOnly.ToString ());
		}

		public void TestCloneNode ()
		{
			XmlNode original = docType;

			XmlNode cloned1 = docType.CloneNode (true);
			TestXmlNodeBaseProperties (original, cloned1);

			XmlNode cloned2 = docType.CloneNode (false);
			TestXmlNodeBaseProperties (original, cloned2);

			AssertEquals ("Deep and shallow cloning", cloned1.Value, cloned2.Value);
		}
	       
	}
}
