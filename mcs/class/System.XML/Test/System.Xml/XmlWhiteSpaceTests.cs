//
// System.Xml.XmlWhitespaceTests.cs
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
	public class XmlWhiteSpaceTests
	{
		XmlDocument document;
		XmlDocument doc2;
		XmlWhitespace whitespace;
		XmlWhitespace broken;
		XmlNode original;
		XmlNode deep;
		XmlNode shallow;
		
		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
			document.LoadXml ("<root><foo></foo></root>");
			XmlElement element = document.CreateElement ("foo");
			whitespace = document.CreateWhitespace ("\r\n");
			element.AppendChild (whitespace);

			doc2 = new XmlDocument ();
			doc2.PreserveWhitespace = true;
		}

		[Test]
		public void InnerAndOuterXml ()
		{
			whitespace = doc2.CreateWhitespace ("\r\n\t ");
			Assert.AreEqual (String.Empty, whitespace.InnerXml);
			Assert.AreEqual ("\r\n\t ", whitespace.OuterXml);
		}
			
		internal void XmlNodeBaseProperties (XmlNode original, XmlNode cloned)
		{
//			assertequals (original.nodetype + " was incorrectly cloned.",
//				      original.baseuri, cloned.baseuri);			
			Assert.IsNull (cloned.ParentNode);
			Assert.AreEqual (cloned.Value, original.Value, "Value incorrectly cloned");
			
                        Assert.IsTrue (!Object.ReferenceEquals (original, cloned), "Copies, not pointers");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void XmlWhitespaceBadConstructor ()
		{
			broken = document.CreateWhitespace ("black");				
		}

		[Test]
		public void XmlWhitespaceConstructor ()
		{
			Assert.AreEqual ("\r\n", whitespace.Data, "whitespace char didn't get copied right");
		}
			       
		[Test]
		public void XmlWhitespaceName ()
		{
			Assert.AreEqual (whitespace.Name, "#whitespace", whitespace.NodeType + " Name property broken");
		}

		[Test]
		public void XmlWhitespaceLocalName ()
		{
			Assert.AreEqual (whitespace.LocalName, "#whitespace", whitespace.NodeType + " LocalName property broken");
		}

		[Test]
		public void XmlWhitespaceNodeType ()
		{
			Assert.AreEqual (whitespace.NodeType.ToString (), "Whitespace", "XmlWhitespace NodeType property broken");
		}

		[Test]
		public void XmlWhitespaceIsReadOnly ()
		{
			Assert.AreEqual (whitespace.IsReadOnly, false, "XmlWhitespace IsReadOnly property broken");
		}

		[Test]
		public void XmlWhitespaceCloneNode ()
		{
			original = whitespace;

			shallow = whitespace.CloneNode (false); // shallow
			XmlNodeBaseProperties (original, shallow);
						
			deep = whitespace.CloneNode (true); // deep
			XmlNodeBaseProperties (original, deep);			

                        Assert.AreEqual (deep.OuterXml, shallow.OuterXml, "deep cloning differs from shallow cloning");
		}
	}
}
