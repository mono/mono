//
// System.Xml.XmlDeclarationTests.cs
//
// Author:
// 	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlDeclarationTests : TestCase
	{

		XmlDocument document;
		XmlDeclaration declaration;
		
		public XmlDeclarationTests () : base ("MonoTests.System.Xml.XmlDeclarationTests testsuite") {}
		
		public XmlDeclarationTests (string name) : base (name) {}

		protected override void SetUp ()
		{
			document = new XmlDocument ();
			document.LoadXml ("<foo><bar></bar></foo>");
			declaration = document.CreateXmlDeclaration ("1.0", null, null);
		}

		public void TestInnerAndOuterXml ()
		{
			declaration = document.CreateXmlDeclaration ("1.0", null, null);
			AssertEquals (String.Empty, declaration.InnerXml);
			AssertEquals ("<?xml version=\"1.0\"?>", declaration.OuterXml);

			declaration = document.CreateXmlDeclaration ("1.0", "doesn't check", null);
			AssertEquals (String.Empty, declaration.InnerXml);
			AssertEquals ("<?xml version=\"1.0\" encoding=\"doesn't check\"?>", declaration.OuterXml);

			declaration = document.CreateXmlDeclaration ("1.0", null, "yes");
			AssertEquals (String.Empty, declaration.InnerXml);
			AssertEquals ("<?xml version=\"1.0\" standalone=\"yes\"?>", declaration.OuterXml);

			declaration = document.CreateXmlDeclaration ("1.0", "foo", "no");
			AssertEquals (String.Empty, declaration.InnerXml);
			AssertEquals ("<?xml version=\"1.0\" encoding=\"foo\" standalone=\"no\"?>", declaration.OuterXml);
		}

		internal void TestXmlNodeBaseProperties (XmlNode original, XmlNode cloned)
		{
//			assertequals (original.nodetype + " was incorrectly cloned.",
//				      original.baseuri, cloned.baseuri);			
			AssertNull (cloned.ParentNode);

			AssertEquals ("Value incorrectly cloned",
				      original.Value, cloned.Value);
			
                        Assert ("Copies, not pointers", !Object.ReferenceEquals (original,cloned));
		}

		public void TestConstructor ()
		{
			try {
				XmlDeclaration broken = document.CreateXmlDeclaration ("2.0", null, null);
			} catch (ArgumentException) {
				return;

			} catch (Exception e) {
				Fail("first arg null, wrong exception: " + e.ToString());
			}
		}

		public void TestNodeType ()
		{
			AssertEquals ("incorrect NodeType returned", XmlNodeType.XmlDeclaration, declaration.NodeType);
		}

		public void TestNames ()
		{
			AssertEquals ("Name is incorrect", "xml", declaration.Name);
			AssertEquals ("LocalName is incorrect", "xml", declaration.LocalName);
		}

		public void TestEncodingProperty ()
		{
			XmlDeclaration d1 = document.CreateXmlDeclaration ("1.0", "foo", null);
			AssertEquals ("Encoding property", "foo", d1.Encoding);

			XmlDeclaration d2 = document.CreateXmlDeclaration ("1.0", null, null);
			AssertEquals ("null Encoding property", String.Empty, d2.Encoding);
		}

		public void TestStandaloneProperty ()
		{
			XmlDeclaration d1 = document.CreateXmlDeclaration ("1.0", null, "yes");
			AssertEquals ("Yes standalone property", "yes", d1.Standalone);

			XmlDeclaration d2 = document.CreateXmlDeclaration ("1.0", null, "no");
			AssertEquals ("No standalone property", "no", d2.Standalone);

			XmlDeclaration d3 = document.CreateXmlDeclaration ("1.0", null, null);
			AssertEquals ("null Standalone property", String.Empty, d3.Standalone);
		}

		public void TestValueProperty ()
		{
			XmlDeclaration d = document.CreateXmlDeclaration ("1.0", "UTF-8", "yes");
			AssertEquals ("Value property", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"",
				      d.Value);
		}

		public void TestXmlCommentCloneNode ()
		{
			XmlNode original = declaration;

			XmlNode shallow = declaration.CloneNode (false); // shallow
			TestXmlNodeBaseProperties (original, shallow);
			
			XmlNode deep = declaration.CloneNode (true); // deep
			TestXmlNodeBaseProperties (original, deep);

                        AssertEquals ("deep cloning differs from shallow cloning",
				      deep.OuterXml, shallow.OuterXml);
		}
	}
}
