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
	public class XmlSignificantWhitespaceTests
	{
		XmlDocument document;
		XmlDocument doc2;
		XmlSignificantWhitespace whitespace;
		XmlSignificantWhitespace broken;
		XmlNode original;
		XmlNode deep;
		XmlNode shallow;
		
		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
			document.LoadXml ("<root><foo></foo></root>");
			XmlElement element = document.CreateElement ("foo");
			whitespace = document.CreateSignificantWhitespace ("\r\n");
			element.AppendChild (whitespace);

			doc2 = new XmlDocument ();
		}

		[Test]
		public void InnerAndOuterXml ()
		{
			whitespace = doc2.CreateSignificantWhitespace ("\r\n\t ");
			Assert.AreEqual (String.Empty, whitespace.InnerXml);
			Assert.AreEqual ("\r\n\t ", whitespace.OuterXml);
		}

		[Test]
		public void DataAndValue ()
		{
			string val = "\t\t\r\n ";
			whitespace = doc2.CreateSignificantWhitespace (val);
			Assert.AreEqual (val, whitespace.Data, "#DataValue.1");
			Assert.AreEqual (val, whitespace.Value, "#DataValue.2");
			whitespace.Value = val + "\t";
			Assert.AreEqual (val + "\t", whitespace.Data, "#DataValue.3");
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
		public void XmlSignificantWhitespaceBadConstructor ()
		{
			try {
				broken = document.CreateSignificantWhitespace ("black");				

			} catch (ArgumentException) {
				return;

			} catch (Exception) {
				Assert.Fail ("Incorrect Exception thrown.");
			}
		}

		[Test]
		public void XmlSignificantWhitespaceConstructor ()
		{
			Assert.AreEqual ("\r\n", whitespace.Data, "whitespace char didn't get copied right");
		}
		
	       	[Test]
		public void XmlSignificantWhitespaceName ()
		{
			Assert.AreEqual (whitespace.Name, "#significant-whitespace", whitespace.NodeType + " Name property broken");
		}

		[Test]
		public void XmlSignificantWhitespaceLocalName ()
		{
			Assert.AreEqual (whitespace.LocalName, "#significant-whitespace", whitespace.NodeType + " LocalName property broken");
		}

		[Test]
		public void XmlSignificantWhitespaceNodeType ()
		{
			Assert.AreEqual (whitespace.NodeType.ToString (), "SignificantWhitespace", "XmlSignificantWhitespace NodeType property broken");
		}

		[Test]
		public void XmlSignificantWhitespaceIsReadOnly ()
		{
			Assert.AreEqual (whitespace.IsReadOnly, false, "XmlSignificantWhitespace IsReadOnly property broken");
		}

		[Test]
		public void XmlSignificantWhitespaceCloneNode ()
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
