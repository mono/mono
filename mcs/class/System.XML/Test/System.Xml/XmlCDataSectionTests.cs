//
// System.Xml.XmlCDataSectionTests.cs
//
// Authors:
//	Duncan Mak  (duncan@ximian.com)
//      Martin Willemoes Hansen (mwh@sysrq.dk)
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
	public class XmlCDataSectionTests : Assertion
	{
		XmlDocument document;
		XmlCDataSection section;
		XmlNode original;
		XmlNode deep;
		XmlNode shallow;

		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
			document.LoadXml ("<root><foo></foo></root>");
			section = document.CreateCDataSection ("CDataSection");
		}

		internal void XmlNodeBaseProperties (XmlNode original, XmlNode cloned)
		{
			// AssertEquals (original.nodetype + " was incorrectly cloned.",
			// 		 original.baseuri, cloned.baseuri);			
			AssertNull (cloned.ParentNode);
                        Assert ("Copies, not pointers", !Object.ReferenceEquals (original,cloned));
		}
	       
		[Test]
		public void XmlCDataSectionInnerAndOuterXml ()
		{
			section = document.CreateCDataSection ("foo");
			AssertEquals (String.Empty, section.InnerXml);
			AssertEquals ("<![CDATA[foo]]>", section.OuterXml);
		}

		[Test]
		public void XmlCDataSectionName ()
		{
			AssertEquals (section.NodeType + " Name property broken",
				      section.Name, "#cdata-section");
		}

		[Test]
		public void XmlCDataSectionLocalName ()
		{
			AssertEquals (section.NodeType + " LocalName property broken",
				      section.LocalName, "#cdata-section");
		}

		[Test]
		public void XmlCDataSectionNodeType ()
		{
			AssertEquals ("XmlCDataSection NodeType property broken",
				      section.NodeType.ToString (), "CDATA");
		}

		[Test]
		public void XmlCDataSectionIsReadOnly ()
		{
			AssertEquals ("XmlCDataSection IsReadOnly property broken",
				      section.IsReadOnly, false);
		}

		[Test]
		public void XmlCDataSectionCloneNode ()
		{
			original = section;

			shallow = section.CloneNode (false); // shallow
			XmlNodeBaseProperties (original, shallow);
			AssertEquals ("Value incorrectly cloned",
				      original.Value, shallow.Value);
			
			deep = section.CloneNode (true); // deep
			XmlNodeBaseProperties (original, deep);
			AssertEquals ("Value incorrectly cloned",
				       original.Value, deep.Value);

                        AssertEquals ("deep cloning differs from shallow cloning",
				      deep.OuterXml, shallow.OuterXml);
		}
	}
}
