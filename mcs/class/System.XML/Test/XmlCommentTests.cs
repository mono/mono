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

		public XmlCommentTests ()
			: base ("Ximian.Mono.Tests.XmlCommentTests testsuite")
		{
		}

		public XmlCommentTests (string name)
			: base (name)
		{
		}

		protected override void SetUp ()
		{
			document = new XmlDocument ();
			document.LoadXml ("<root><foo></foo></root>");
			comment = document.CreateComment ("Comment");
		}

		internal void TestXmlNodeBaseProperties (XmlNode original, XmlNode cloned)
		{
//			assertequals (original.nodetype + " was incorrectly cloned.",
//				      original.baseuri, cloned.baseuri);			
			AssertNull (cloned.ParentNode);
                        Assert ("Copies, not pointers", !Object.ReferenceEquals (original,cloned));
		}
	       
		public void TestXmlCommentName ()
		{
			AssertEquals (comment.NodeType + " Name property broken",
				      comment.Name, "#comment");
		}

		public void TestXmlCommentLocalName ()
		{
			AssertEquals (comment.NodeType + " LocalName property broken",
				      comment.LocalName, "#comment");
		}

		public void TestXmlCommentNodeType ()
		{
			AssertEquals ("XmlComment NodeType property broken",
				      comment.NodeType.ToString (), "Comment");
		}

		public void TestXmlCommentIsReadOnly ()
		{
			AssertEquals ("XmlComment IsReadOnly property broken",
				      comment.IsReadOnly, false);
		}

		public void TestXmlCommentCloneNode ()
		{
			original = comment;

			shallow = comment.CloneNode (false); // shallow
			TestXmlNodeBaseProperties (original, shallow);
			AssertEquals ("Value incorrectly cloned",
				      original.Value, shallow.Value);
			
			deep = comment.CloneNode (true); // deep
			TestXmlNodeBaseProperties (original, deep);
			AssertEquals ("Value incorrectly cloned",
				       original.Value, deep.Value);

                        AssertEquals ("deep cloning differs from shallow cloning",
				      deep.OuterXml, shallow.OuterXml);
		}
	}
}
