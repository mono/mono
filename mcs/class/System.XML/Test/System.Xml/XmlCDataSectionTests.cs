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
	public class XmlCDataSectionTests
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
			// Assert.AreEqual (// 		 original.baseuri, cloned.baseuri, original.nodetype + " was incorrectly cloned.");			
			Assert.IsNull (cloned.ParentNode);
                        Assert.IsTrue (!Object.ReferenceEquals (original, cloned), "Copies, not pointers");
		}
	       
		[Test]
		public void XmlCDataSectionInnerAndOuterXml ()
		{
			section = document.CreateCDataSection ("foo");
			Assert.AreEqual (String.Empty, section.InnerXml);
			Assert.AreEqual ("<![CDATA[foo]]>", section.OuterXml);
		}

		[Test]
		public void XmlCDataSectionName ()
		{
			Assert.AreEqual (section.Name, "#cdata-section", section.NodeType + " Name property broken");
		}

		[Test]
		public void XmlCDataSectionLocalName ()
		{
			Assert.AreEqual (section.LocalName, "#cdata-section", section.NodeType + " LocalName property broken");
		}

		[Test]
		public void XmlCDataSectionNodeType ()
		{
			Assert.AreEqual (section.NodeType.ToString (), "CDATA", "XmlCDataSection NodeType property broken");
		}

		[Test]
		public void XmlCDataSectionIsReadOnly ()
		{
			Assert.AreEqual (section.IsReadOnly, false, "XmlCDataSection IsReadOnly property broken");
		}

		[Test]
		public void XmlCDataSectionCloneNode ()
		{
			original = section;

			shallow = section.CloneNode (false); // shallow
			XmlNodeBaseProperties (original, shallow);
			Assert.AreEqual (original.Value, shallow.Value, "Value incorrectly cloned");
			
			deep = section.CloneNode (true); // deep
			XmlNodeBaseProperties (original, deep);
			Assert.AreEqual (original.Value, deep.Value, "Value incorrectly cloned");

                        Assert.AreEqual (deep.OuterXml, shallow.OuterXml, "deep cloning differs from shallow cloning");
		}
	}
}
