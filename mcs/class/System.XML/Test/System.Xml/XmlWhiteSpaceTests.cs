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
	public class XmlWhiteSpaceTests : Assertion
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
			AssertEquals (String.Empty, whitespace.InnerXml);
			AssertEquals ("\r\n\t ", whitespace.OuterXml);
		}
			
		internal void XmlNodeBaseProperties (XmlNode original, XmlNode cloned)
		{
//			assertequals (original.nodetype + " was incorrectly cloned.",
//				      original.baseuri, cloned.baseuri);			
			AssertNull (cloned.ParentNode);
			AssertEquals ("Value incorrectly cloned",
				       cloned.Value, original.Value);
			
                        Assert ("Copies, not pointers", !Object.ReferenceEquals (original,cloned));
		}

		[Test]
		public void XmlWhitespaceBadConstructor ()
		{
			try {
				broken = document.CreateWhitespace ("black");				

			} catch (ArgumentException) {
				return;

			} catch (Exception) {
				Fail ("Incorrect Exception thrown.");
			}
		}

		[Test]
		public void XmlWhitespaceConstructor ()
		{
			AssertEquals ("whitespace char didn't get copied right",
				      "\r\n", whitespace.Data);
		}
			       
		[Test]
		public void XmlWhitespaceName ()
		{
			AssertEquals (whitespace.NodeType + " Name property broken",
				      whitespace.Name, "#whitespace");
		}

		[Test]
		public void XmlWhitespaceLocalName ()
		{
			AssertEquals (whitespace.NodeType + " LocalName property broken",
				      whitespace.LocalName, "#whitespace");
		}

		[Test]
		public void XmlWhitespaceNodeType ()
		{
			AssertEquals ("XmlWhitespace NodeType property broken",
				      whitespace.NodeType.ToString (), "Whitespace");
		}

		[Test]
		public void XmlWhitespaceIsReadOnly ()
		{
			AssertEquals ("XmlWhitespace IsReadOnly property broken",
				      whitespace.IsReadOnly, false);
		}

		[Test]
		public void XmlWhitespaceCloneNode ()
		{
			original = whitespace;

			shallow = whitespace.CloneNode (false); // shallow
			XmlNodeBaseProperties (original, shallow);
						
			deep = whitespace.CloneNode (true); // deep
			XmlNodeBaseProperties (original, deep);			

                        AssertEquals ("deep cloning differs from shallow cloning",
				      deep.OuterXml, shallow.OuterXml);
		}
	}
}
