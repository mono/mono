//
// System.Xml.XmlCommentTests.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
// Author: Martin Willemoes Hansen (mwh@sysrq.dk)
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
	public class XmlCommentTests : Assertion
	{
		XmlDocument document;
		XmlComment comment;
		XmlNode original;
		XmlNode deep;
		XmlNode shallow;

		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
		}

		[Test]
		public void XmlCommentCloneNode ()
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");
			original = comment;

			shallow = comment.CloneNode (false); // shallow
			XmlNodeBaseProperties (original, shallow);
			
			deep = comment.CloneNode (true); // deep
			XmlNodeBaseProperties (original, deep);
			AssertEquals ("Value incorrectly cloned",
				original.Value, deep.Value);

			AssertEquals ("deep cloning differs from shallow cloning",
				deep.OuterXml, shallow.OuterXml);
		}

		[Test]
		public void XmlCommentInnerAndOuterXml ()
		{
			comment = document.CreateComment ("foo");
			AssertEquals (String.Empty, comment.InnerXml);
			AssertEquals ("<!--foo-->", comment.OuterXml);
		}

		[Test]
		public void XmlCommentIsReadOnly ()
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");
			AssertEquals ("XmlComment IsReadOnly property broken",
				comment.IsReadOnly, false);
		}

		[Test]
		public void XmlCommentLocalName ()
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");
			AssertEquals (comment.NodeType + " LocalName property broken",
				      comment.LocalName, "#comment");
		}

		[Test]
		public void XmlCommentName ()
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");
			AssertEquals (comment.NodeType + " Name property broken",
				comment.Name, "#comment");
		}

		[Test]
		public void XmlCommentNodeType ()
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");
			AssertEquals ("XmlComment NodeType property broken",
				      comment.NodeType.ToString (), "Comment");
		}

		internal void XmlNodeBaseProperties (XmlNode original, XmlNode cloned)
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
