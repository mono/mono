//
// System.Xml.XmlCDataSectionTests.cs
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Xml;

using NUnit.Framework;

namespace Ximian.Mono.Tests
{
	public class XmlCDataSectionTests : TestCase
	{
		XmlDocument document;
		XmlCDataSection section;
		XmlNode original;
		XmlNode deep;
		XmlNode shallow;

		public XmlCDataSectionTests ()
			: base ("Ximian.Mono.Tests.XmlCDataSectionTests testsuite")
		{
		}

		public XmlCDataSectionTests (string name)
			: base (name)
		{
		}

		protected override void SetUp ()
		{
			document = new XmlDocument ();
			document.LoadXml ("<root><foo></foo></root>");
			section = document.CreateCDataSection ("CDataSection");
		}

		internal void TestXmlNodeBaseProperties (XmlNode original, XmlNode cloned)
		{
			// AssertEquals (original.nodetype + " was incorrectly cloned.",
			// 		 original.baseuri, cloned.baseuri);			
			AssertNull (cloned.ParentNode);
                        Assert ("Copies, not pointers", !Object.ReferenceEquals (original,cloned));
		}
	       
		public void TestXmlCDataSectionInnerAndOuterXml ()
		{
			section = document.CreateCDataSection ("foo");
			AssertEquals (String.Empty, section.InnerXml);
			AssertEquals ("<![CDATA[foo]]>", section.OuterXml);
		}

		public void TestXmlCDataSectionName ()
		{
			AssertEquals (section.NodeType + " Name property broken",
				      section.Name, "#cdata-section");
		}

		public void TestXmlCDataSectionLocalName ()
		{
			AssertEquals (section.NodeType + " LocalName property broken",
				      section.LocalName, "#cdata-section");
		}

		public void TestXmlCDataSectionNodeType ()
		{
			AssertEquals ("XmlCDataSection NodeType property broken",
				      section.NodeType.ToString (), "CDATA");
		}

		public void TestXmlCDataSectionIsReadOnly ()
		{
			AssertEquals ("XmlCDataSection IsReadOnly property broken",
				      section.IsReadOnly, false);
		}

		public void TestXmlCDataSectionCloneNode ()
		{
			original = section;

			shallow = section.CloneNode (false); // shallow
			TestXmlNodeBaseProperties (original, shallow);
			AssertEquals ("Value incorrectly cloned",
				      original.Value, shallow.Value);
			
			deep = section.CloneNode (true); // deep
			TestXmlNodeBaseProperties (original, deep);
			AssertEquals ("Value incorrectly cloned",
				       original.Value, deep.Value);

                        AssertEquals ("deep cloning differs from shallow cloning",
				      deep.OuterXml, shallow.OuterXml);
		}
	}
}
