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
			Assertion.AssertEquals (String.Empty, whitespace.InnerXml);
			Assertion.AssertEquals ("\r\n\t ", whitespace.OuterXml);
		}

		[Test]
		public void DataAndValue ()
		{
			string val = "\t\t\r\n ";
			whitespace = doc2.CreateSignificantWhitespace (val);
			Assertion.AssertEquals ("#DataValue.1", val, whitespace.Data);
			Assertion.AssertEquals ("#DataValue.2", val, whitespace.Value);
			whitespace.Value = val + "\t";
			Assertion.AssertEquals ("#DataValue.3", val + "\t", whitespace.Data);
		}
			
		internal void XmlNodeBaseProperties (XmlNode original, XmlNode cloned)
		{
//			assertequals (original.nodetype + " was incorrectly cloned.",
//				      original.baseuri, cloned.baseuri);			
			Assertion.AssertNull (cloned.ParentNode);
			Assertion.AssertEquals ("Value incorrectly cloned",
				       cloned.Value, original.Value);
			
                        Assertion.Assert ("Copies, not pointers", !Object.ReferenceEquals (original,cloned));
		}

		[Test]
		public void XmlSignificantWhitespaceBadConstructor ()
		{
			try {
				broken = document.CreateSignificantWhitespace ("black");				

			} catch (ArgumentException) {
				return;

			} catch (Exception) {
				Assertion.Fail ("Incorrect Exception thrown.");
			}
		}

		[Test]
		public void XmlSignificantWhitespaceConstructor ()
		{
			Assertion.AssertEquals ("whitespace char didn't get copied right",
				      "\r\n", whitespace.Data);
		}
		
	       	[Test]
		public void XmlSignificantWhitespaceName ()
		{
			Assertion.AssertEquals (whitespace.NodeType + " Name property broken",
				      whitespace.Name, "#significant-whitespace");
		}

		[Test]
		public void XmlSignificantWhitespaceLocalName ()
		{
			Assertion.AssertEquals (whitespace.NodeType + " LocalName property broken",
				      whitespace.LocalName, "#significant-whitespace");
		}

		[Test]
		public void XmlSignificantWhitespaceNodeType ()
		{
			Assertion.AssertEquals ("XmlSignificantWhitespace NodeType property broken",
				      whitespace.NodeType.ToString (), "SignificantWhitespace");
		}

		[Test]
		public void XmlSignificantWhitespaceIsReadOnly ()
		{
			Assertion.AssertEquals ("XmlSignificantWhitespace IsReadOnly property broken",
				      whitespace.IsReadOnly, false);
		}

		[Test]
		public void XmlSignificantWhitespaceCloneNode ()
		{
			original = whitespace;

			shallow = whitespace.CloneNode (false); // shallow
			XmlNodeBaseProperties (original, shallow);
						
			deep = whitespace.CloneNode (true); // deep
			XmlNodeBaseProperties (original, deep); 

                        Assertion.AssertEquals ("deep cloning differs from shallow cloning",
				      deep.OuterXml, shallow.OuterXml);
		}
	}
}
