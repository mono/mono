//
// System.Xml.XmlWhitespaceTests.cs
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlSignificantWhitespaceTests : TestCase
	{
		XmlDocument document;
		XmlDocument doc2;
		XmlSignificantWhitespace whitespace;
		XmlSignificantWhitespace broken;
		XmlNode original;
		XmlNode deep;
		XmlNode shallow;
		
		public XmlSignificantWhitespaceTests ()	: base ("MonoTests.System.Xml.XmlWhitespaceTests testsuite") {}
		public XmlSignificantWhitespaceTests (string name) : base (name) {}

		protected override void SetUp ()
		{
			document = new XmlDocument ();
			document.LoadXml ("<root><foo></foo></root>");
			XmlElement element = document.CreateElement ("foo");
			whitespace = document.CreateSignificantWhitespace ("\r\n");
			element.AppendChild (whitespace);

			doc2 = new XmlDocument ();
		}

		public void TestInnerAndOuterXml ()
		{
			whitespace = doc2.CreateSignificantWhitespace ("\r\n\t ");
			AssertEquals (String.Empty, whitespace.InnerXml);
			AssertEquals ("\r\n\t ", whitespace.OuterXml);
		}

		public void TestDataAndValue ()
		{
			string val = "\t\t\r\n ";
			whitespace = doc2.CreateSignificantWhitespace (val);
			AssertEquals ("#DataValue.1", val, whitespace.Data);
			AssertEquals ("#DataValue.2", val, whitespace.Value);
			whitespace.Value = val + "\t";
			AssertEquals ("#DataValue.3", val + "\t", whitespace.Data);
		}
			
		internal void TestXmlNodeBaseProperties (XmlNode original, XmlNode cloned)
		{
//			assertequals (original.nodetype + " was incorrectly cloned.",
//				      original.baseuri, cloned.baseuri);			
			AssertNull (cloned.ParentNode);
			AssertEquals ("Value incorrectly cloned",
				       cloned.Value, original.Value);
			
                        Assert ("Copies, not pointers", !Object.ReferenceEquals (original,cloned));
		}

		public void TestXmlSignificantWhitespaceBadConstructor ()
		{
			try {
				broken = document.CreateSignificantWhitespace ("black");				

			} catch (ArgumentException) {
				return;

			} catch (Exception) {
				Fail ("Incorrect Exception thrown.");
			}
		}

		public void TestXmlSignificantWhitespaceConstructor ()
		{
			AssertEquals ("whitespace char didn't get copied right",
				      "\r\n", whitespace.Data);
		}
		
	       
		public void TestXmlSignificantWhitespaceName ()
		{
			AssertEquals (whitespace.NodeType + " Name property broken",
				      whitespace.Name, "#significant-whitespace");
		}

		public void TestXmlSignificantWhitespaceLocalName ()
		{
			AssertEquals (whitespace.NodeType + " LocalName property broken",
				      whitespace.LocalName, "#significant-whitespace");
		}

		public void TestXmlSignificantWhitespaceNodeType ()
		{
			AssertEquals ("XmlSignificantWhitespace NodeType property broken",
				      whitespace.NodeType.ToString (), "SignificantWhitespace");
		}

		public void TestXmlSignificantWhitespaceIsReadOnly ()
		{
			AssertEquals ("XmlSignificantWhitespace IsReadOnly property broken",
				      whitespace.IsReadOnly, false);
		}

		public void TestXmlSignificantWhitespaceCloneNode ()
		{
			original = whitespace;

			shallow = whitespace.CloneNode (false); // shallow
			TestXmlNodeBaseProperties (original, shallow);
						
			deep = whitespace.CloneNode (true); // deep
			TestXmlNodeBaseProperties (original, deep); 

                        AssertEquals ("deep cloning differs from shallow cloning",
				      deep.OuterXml, shallow.OuterXml);
		}
	}
}
