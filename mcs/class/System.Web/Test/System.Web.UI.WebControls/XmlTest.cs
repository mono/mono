//
// Tests for System.Web.UI.WebControls.Xml.cs 
//
// Author:
//	Miguel de Icaza (miguel@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;

namespace MonoTests.System.Web.UI.WebControls
{
	class XmlPoker : Xml {
		public XmlPoker ()
		{
		}
		
		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void LoadState (object o)
		{
			LoadViewState (o);
		}

		public void DoRender (HtmlTextWriter output)
		{
			Render (output);
		}

		public void DoAdd (object o)
		{
			AddParsedSubObject (o);
		}
	}
	
	
	[TestFixture]	
	public class XmlTest {

#if false
		public void Label_ViewState ()
		{
			XmlPoker p = new XmlPoker ();

			Assert.AreEqual (p.Text, "", "A1");
			p.Text = "Hello";
			Assert.AreEqual (p.Text, "Hello", "A2");

			object state = p.SaveState ();

			Poker copy = new Poker ();
			copy.LoadState (state);
			Assert.AreEqual (copy.Text, "Hello", "A3");
		}

		[Test]
		public void Label_Render ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			Label l = new Label ();
			l.Text = "Hello";
			l.RenderControl (tw);
			Assert.AreEqual ("<span>Hello</span>", sw.ToString (), "R1");
		}
#endif

		[SetUp] public void Xml_Setup ()
		{
			if (File.Exists ("test.xml")) File.Delete ("test.xml");
			
			using (FileStream f = File.OpenWrite ("test.xml")){
				StreamWriter sw = new StreamWriter (f);
				sw.WriteLine ("<?xml version=\"1.0\"?><testfile></testfile>");
			}

			if (File.Exists ("test.xsl")) File.Delete ("test.xsl");
			
			using (FileStream f = File.OpenWrite ("test.xsl")){
				StreamWriter sw = new StreamWriter (f);
				sw.WriteLine ("<xsl:stylesheet version='1.0' " + 
					      "xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
					      "<xsl:template match=\"*\">" +
					      "<xsl:copy-of select=\".\"/>" +
					      "</xsl:template>" +
					      "</xsl:stylesheet>");

			}
		}
		
		[TearDown] public void Xml_TearDown ()
		{
			if (File.Exists ("test.xml")) File.Delete ("test.xml");
			if (File.Exists ("test.xsl")) File.Delete ("test.xsl");
		}
		
		[Test] public void Xml_Values ()
		{
			Xml xml = new Xml ();

			Assert.AreEqual ("", xml.DocumentContent, "V1");
			Assert.AreEqual (null, xml.Document, "V2");
			Assert.AreEqual ("", xml.DocumentSource, "V3");
			
			Assert.AreEqual (null, xml.Transform, "V4");
			Assert.AreEqual (null, xml.TransformArgumentList, "V5");
			Assert.AreEqual ("", xml.TransformSource, "V6");

			// Check that assignments to null, are mapped back into ""
			xml.DocumentContent = null;
			Assert.AreEqual ("", xml.DocumentContent, "V7");
			
			xml.TransformSource = null;
			Assert.AreEqual ("", xml.TransformSource, "V8");

			Assert.AreEqual (null, xml.Transform, "V9");
#if NET_2_0
			Assert.AreEqual (false, xml.EnableTheming, "EnableTheming");
			Assert.AreEqual (String.Empty, xml.SkinID, "SkinID");
			Assert.AreEqual (null, xml.XPathNavigator, "XPathNavigator");
#endif
		}

		// Tests that invalid documents can be set before rendering.
		[Test] 
		public void Xml_InvalidDocument ()
		{
			Xml xml = new Xml ();
			xml.DocumentContent = "Hey";
#if NET_2_0
			Assert.AreEqual ("Hey", xml.DocumentContent);
#else
			Assert.AreEqual ("", xml.DocumentContent);
#endif
			xml.DocumentContent = "<hey></hey>";
#if NET_2_0
			Assert.AreEqual ("<hey></hey>", xml.DocumentContent);
#else
			Assert.AreEqual ("", xml.DocumentContent);
#endif

			xml.TransformSource = "test.xsl";
			Assert.AreEqual (null, xml.Transform, "ID");
		}

		[Test] public void Xml_RenderEmpty ()
		{
			XmlPoker xml = new XmlPoker ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter hw = new HtmlTextWriter (sw);
			
			xml.DoRender (hw);
			Assert.AreEqual ("", sw.ToString (), "RE1");
		}
		
		[Test] public void Xml_SourcePrecedence ()
		{
			XmlPoker xml = new XmlPoker ();
			xml.DocumentContent = "<content></content>";

			XmlDocument xml_doc = new XmlDocument ();
			xml_doc.LoadXml ("<document></document>");
			
			xml.Document = xml_doc;

			//
			// Can not check the precendece for the file backend
			// because it requires a complete setup to check for the
			// file access permission to it (Control.MapPathSecure)
			//
			// xml.DocumentSource = "test.xml";
			//

			StringWriter sw = new StringWriter ();
			HtmlTextWriter hw = new HtmlTextWriter (sw);
			xml.DoRender (hw);

			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?><document></document>",
					 sw.ToString (), "SP1");

			// Now compare the inline XML
			xml.DocumentContent = "<content></content>";
			sw = new StringWriter ();
			hw = new HtmlTextWriter (sw);
			xml.DoRender (hw);
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?><content></content>",
					 sw.ToString (), "SP1");
		}

#if NET_2_0
		[Test]
		public void Xml_SourcePrecedence_2 () 
		{
			XmlPoker xml = new XmlPoker ();

			XmlDocument xml_doc = new XmlDocument ();
			xml_doc.LoadXml ("<document></document>");

			xml.Document = xml_doc;

			XmlDocument xml_navigator_doc = new XmlDocument ();
			xml_navigator_doc.LoadXml ("<navigator></navigator>");

			xml.XPathNavigator = xml_navigator_doc.CreateNavigator ();

			StringWriter sw = new StringWriter ();
			HtmlTextWriter hw = new HtmlTextWriter (sw);
			xml.DoRender (hw);

			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?><navigator></navigator>",
					 sw.ToString (), "Xml_SourcePrecedence_2");

		}
#endif

		[Test] public void Xml_DefaultTrasnform ()
		{
			XmlPoker xml = new XmlPoker ();

			// For the actual transform, I was lazy, but
			// xsp's web_xml.aspx works.
		}

		[ExpectedException(typeof (HttpException))]
		[Test] public void Xml_InsertInvalid ()
		{
			XmlPoker xml = new XmlPoker ();

			xml.DoAdd ("hello");
		}

		[ExpectedException(typeof (NullReferenceException))]
		[Test] public void Xml_InsertInvalid2 ()
		{
			XmlPoker xml = new XmlPoker ();

			xml.DoAdd (null);
		}

		[Test] public void Xml_InsertValid ()
		{
			XmlPoker xml = new XmlPoker ();

			xml.DoAdd (new LiteralControl ("<test></test>"));
		}

#if NET_2_0
		class CustomXPathNavigator : XPathNavigator
		{
			public override string BaseURI
			{
				get { return "Test"; }
			}

			#region fake
			public override XPathNavigator Clone ()
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			public override bool IsEmptyElement
			{
				get { throw new Exception ("The method or operation is not implemented."); }
			}

			public override bool IsSamePosition (XPathNavigator other)
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			public override string LocalName
			{
				get { throw new Exception ("The method or operation is not implemented."); }
			}

			public override bool MoveTo (XPathNavigator other)
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			public override bool MoveToFirstAttribute ()
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			public override bool MoveToFirstChild ()
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			public override bool MoveToFirstNamespace (XPathNamespaceScope namespaceScope)
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			public override bool MoveToId (string id)
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			public override bool MoveToNext ()
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			public override bool MoveToNextAttribute ()
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			public override bool MoveToNextNamespace (XPathNamespaceScope namespaceScope)
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			public override bool MoveToParent ()
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			public override bool MoveToPrevious ()
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			public override string Name
			{
				get { throw new Exception ("The method or operation is not implemented."); }
			}

			public override XmlNameTable NameTable
			{
				get { throw new Exception ("The method or operation is not implemented."); }
			}

			public override string NamespaceURI
			{
				get { throw new Exception ("The method or operation is not implemented."); }
			}

			public override XPathNodeType NodeType
			{
				get { throw new Exception ("The method or operation is not implemented."); }
			}

			public override string Prefix
			{
				get { throw new Exception ("The method or operation is not implemented."); }
			}

			public override string Value
			{
				get { throw new Exception ("The method or operation is not implemented."); }
			}
			#endregion
		}

		[Test]
		public void XPathNavigable_1 ()
		{
			Xml xml = new Xml ();
			XmlDocument doc = new XmlDocument ();
			xml.XPathNavigator = doc.CreateNavigator ();
			Assert.IsNotNull (xml.XPathNavigator);
		}

		
		[Test]
		public void XPathNavigable_2 ()
		{
			Xml xml = new Xml ();
			xml.XPathNavigator = new CustomXPathNavigator();
			Assert.AreEqual ("Test", xml.XPathNavigator.BaseURI, "XPathNavigable_2");
		}

		[Test]
		public void XPathNavigatorInstance_1 () 
		{
			Xml xml = new Xml ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<document></document>");

			xml.Document = doc;

			XPathNavigator nav1 = xml.XPathNavigator;

			Assert.IsNull (nav1, "XPathNavigatorInstance_1");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void Focus ()
		{
			Xml xml = new Xml ();
			xml.Focus ();
		}
		

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void SkinID ()
		{
			Xml xml = new Xml ();
			xml.SkinID = "Fake";
		}
#endif
	}
}

		
