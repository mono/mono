//
// System.Xml.XmlCommentTests.cs
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
	public class XmlCommentTests : TestCase
	{
		XmlDocument document;
		XmlComment comment;
		XmlNode original;
		XmlNode deep;
		XmlNode shallow;

		public XmlCommentTests () : base ("Ximian.Mono.Tests.XmlCommentTests testsuite") {}

		public XmlCommentTests (string name) : base (name) {}

		protected override void SetUp ()
		{
			document = new XmlDocument ();
		}

		public void TestXmlCommentCloneNode ()
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");
			original = comment;

			shallow = comment.CloneNode (false); // shallow
			TestXmlNodeBaseProperties (original, shallow);
			
			deep = comment.CloneNode (true); // deep
			TestXmlNodeBaseProperties (original, deep);
			AssertEquals ("Value incorrectly cloned",
				original.Value, deep.Value);

			AssertEquals ("deep cloning differs from shallow cloning",
				deep.OuterXml, shallow.OuterXml);
		}

		public void TestXmlCommentInnerAndOuterXml ()
		{
			comment = document.CreateComment ("foo");
			AssertEquals (String.Empty, comment.InnerXml);
			AssertEquals ("<!--foo-->", comment.OuterXml);
		}

		public void TestXmlCommentIsReadOnly ()
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");
			AssertEquals ("XmlComment IsReadOnly property broken",
				comment.IsReadOnly, false);
		}

		public void TestXmlCommentLocalName ()
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");
			AssertEquals (comment.NodeType + " LocalName property broken",
				      comment.LocalName, "#comment");
		}

		public void TestXmlCommentName ()
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");
			AssertEquals (comment.NodeType + " Name property broken",
				comment.Name, "#comment");
		}

		public void TestXmlCommentNodeType ()
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");
			AssertEquals ("XmlComment NodeType property broken",
				      comment.NodeType.ToString (), "Comment");
		}

		internal void TestXmlNodeBaseProperties (XmlNode original, XmlNode cloned)
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");

			//			assertequals (original.nodetype + " was incorrectly cloned.",
			//				      original.baseuri, cloned.baseuri);			

			AssertNull (cloned.ParentNode);
			AssertEquals ("Value incorrectly cloned",
				original.Value, cloned.Value);

			Assert ("Copies, not pointers", !Object.ReferenceEquals (original,cloned));
		}
       
	}
}
