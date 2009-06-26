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
	public class XmlCommentTests
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
			Assert.AreEqual (original.Value, deep.Value, "Value incorrectly cloned");

			Assert.AreEqual (deep.OuterXml, shallow.OuterXml, "deep cloning differs from shallow cloning");
		}

		[Test]
		public void XmlCommentInnerAndOuterXml ()
		{
			comment = document.CreateComment ("foo");
			Assert.AreEqual (String.Empty, comment.InnerXml);
			Assert.AreEqual ("<!--foo-->", comment.OuterXml);
		}

		[Test]
		public void XmlCommentIsReadOnly ()
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");
			Assert.AreEqual (comment.IsReadOnly, false, "XmlComment IsReadOnly property broken");
		}

		[Test]
		public void XmlCommentLocalName ()
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");
			Assert.AreEqual (comment.LocalName, "#comment", comment.NodeType + " LocalName property broken");
		}

		[Test]
		public void XmlCommentName ()
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");
			Assert.AreEqual (comment.Name, "#comment", comment.NodeType + " Name property broken");
		}

		[Test]
		public void XmlCommentNodeType ()
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");
			Assert.AreEqual (comment.NodeType.ToString (), "Comment", "XmlComment NodeType property broken");
		}

		internal void XmlNodeBaseProperties (XmlNode original, XmlNode cloned)
		{
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");

			//			assertequals (original.nodetype + " was incorrectly cloned.",
			//				      original.baseuri, cloned.baseuri);			

			Assert.IsNull (cloned.ParentNode);
			Assert.AreEqual (original.Value, cloned.Value, "Value incorrectly cloned");

			Assert.IsTrue (!Object.ReferenceEquals (original, cloned), "Copies, not pointers");
		}
       
	}
}
