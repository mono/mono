//
// HtmlTextWriterTest.cs
//	- Unit tests for System.Web.UI.HtmlTextWriter
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Ben Maurer <bmaurer@novell.com>
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

using System;
using System.IO;
using System.Web.UI;

using HtwTag = System.Web.UI.HtmlTextWriterTag;
using HtwAttribute = System.Web.UI.HtmlTextWriterAttribute;
using HtwStyle = System.Web.UI.HtmlTextWriterStyle;

using NUnit.Framework;

namespace MonoTests.System.Web.UI {

	[TestFixture]
	public class HtmlTextWriterTest {

		StringWriter sw;
		HtmlTextWriter w;

		[SetUp]
		public void SetupTests ()
		{
			sw = new StringWriter ();
			sw.NewLine = "\n"; // Keep sanity.
			w = new HtmlTextWriter (sw);
		}
		
		[Test]
		public void DefaultConstFields ()
		{
			Assert.AreEqual ("=\"", HtmlTextWriter.EqualsDoubleQuoteString, "EqualsDoubleQuoteString");
			// add more
		}

		[Test]
		public void NullWriter () 
		{
			HtmlTextWriter htw = new HtmlTextWriter (null);
			Assert.IsNotNull (htw, "NullWriter");
			Assert.IsNull (htw.InnerWriter, "InnerWriter");
		}

		[Test]
		public void WriteAttributes_NullValue ()
		{
			w.WriteAttribute ("name", null);
			Assert.AreEqual (" name", w.InnerWriter.ToString ());
		}

		[Test]
		public void Write1 ()
		{
			w.RenderBeginTag ("a");
			w.RenderEndTag ();

			Assert.AreEqual ("<a></a>", sw.ToString ());
		}

		[Test]
		public void WriteATagByKey ()
		{
			w.RenderBeginTag (HtwTag.A);
			w.RenderEndTag ();

			Assert.AreEqual ("<a></a>", sw.ToString ());
		}

		[Test]
		public void WriteDivTagByKey ()
		{
			w.RenderBeginTag (HtwTag.Div);
			w.RenderEndTag ();

			Assert.AreEqual ("<div>\n\n</div>", sw.ToString ());
		}

		[Test]
		public void WriteOptionTagByKey ()
		{
			w.RenderBeginTag (HtwTag.Option);
			w.RenderEndTag ();

			Assert.AreEqual ("<option>\n\n</option>", sw.ToString ());
		}

		[Test]
		public void WriteDivAndATagByKey ()
		{
			w.RenderBeginTag (HtwTag.Div);
			w.RenderBeginTag (HtwTag.A);
			w.RenderEndTag ();
			w.RenderEndTag ();

			Assert.AreEqual ("<div>\n\t<a></a>\n</div>", sw.ToString ());
		}

		[Test]
		public void NestedIndent ()
		{
			w.RenderBeginTag (HtwTag.Div);
			w.RenderBeginTag (HtwTag.Div);
			w.RenderEndTag ();
			w.RenderEndTag ();

			Assert.AreEqual ("<div>\n\t<div>\n\n\t</div>\n</div>", sw.ToString ());
		}
		
		[Test]
		public void WriteDivAndATagByKeyWithAttr ()
		{
			w.RenderBeginTag (HtwTag.Div);

			w.AddAttribute (HtwAttribute.Href, "http://slashdot.org/");
			w.RenderBeginTag (HtwTag.A);
			w.RenderEndTag ();
			w.RenderEndTag ();
			Assert.AreEqual ("<div>\n\t<a href=\"http://slashdot.org/\"></a>\n</div>", sw.ToString ());
		}
		
		[Test]
		public void WriteDivTagWithStyle ()
		{
			w.AddAttribute ("id", "foo");
			w.AddAttribute ("style", "align:left");
			w.AddStyleAttribute (HtwStyle.Color, "red");
			w.AddStyleAttribute (HtwStyle.FontWeight, "bold");
			w.RenderBeginTag (HtwTag.Div);
			w.RenderEndTag ();
			Assert.AreEqual ("<div id=\"foo\" style=\"color:red;font-weight:bold;align:left\">\n\n</div>", sw.ToString ());
		}

		[Test]
		public void EscapeJScript ()
		{
			w.AddAttribute (HtwAttribute.Onclick, "this.style.color = \"red\"");
			w.RenderBeginTag (HtwTag.Div);
			w.RenderEndTag ();
			Assert.AreEqual ("<div onclick=\"this.style.color = &quot;red&quot;\">\n\n</div>", sw.ToString ());
		}

		[Test]
		public void EscapeUrl ()
		{
			w.AddAttribute (HtwAttribute.Href, "http://www.example.com/search?hl=en&q=i+love+nunit&btnG=Google+Search");
			w.RenderBeginTag (HtwTag.A);
			w.RenderEndTag ();
			Assert.AreEqual ("<a href=\"http://www.example.com/search?hl=en&amp;q=i+love+nunit&amp;btnG=Google+Search\"></a>", sw.ToString ());
		}

		// Which attrs fall here
		[Test]
		public void NoEscapeAttrName ()
		{
			w.AddAttribute (HtwAttribute.Name, "cookies&cream");
			w.RenderBeginTag (HtwTag.Div);
			w.RenderEndTag ();
			Assert.AreEqual ("<div name=\"cookies&cream\">\n\n</div>", sw.ToString ());
		}
		
		[Test]
		public void NoEscapeAttrName2 () {
			w.AddAttribute ("name", "cookies&cream");
			w.RenderBeginTag (HtwTag.Div);
			w.RenderEndTag ();
			Assert.AreEqual ("<div name=\"cookies&cream\">\n\n</div>", sw.ToString ());
		}
		
		[Test]
		public void NoEscapeAttrName3 () {
			w.AddAttribute (HtwAttribute.Name, "cookies&cream", true);
			w.RenderBeginTag (HtwTag.Div);
			w.RenderEndTag ();
			Assert.AreEqual ("<div name=\"cookies&amp;cream\">\n\n</div>", sw.ToString ());
		}
		
		[Test]
		public void NoEscapeAttrName4 () {
			w.AddAttribute ("NaMe", "cookies&cream");
			w.RenderBeginTag (HtwTag.Div);
			w.RenderEndTag ();
			Assert.AreEqual ("<div NaMe=\"cookies&cream\">\n\n</div>", sw.ToString ());
		}
		
		[Test]
		public void EscapeAttribute1 () {
			sw = new StringWriter ();
			sw.NewLine = "\n"; // Keep sanity.
			Poker w1 = new Poker (sw);

			w1.AddAttribute ("attr", "cookies&cream");
			w1.RenderBeginTag (HtwTag.Div);
			w1.RenderEndTag ();

			Assert.AreEqual ("cookies&amp;cream", w1.AttrValue_At_AddAttribute, "AttrValue_At_AddAttribute");
			Assert.AreEqual ("cookies&amp;cream", w1.AttrValue_At_OnAttributeRender, "AttrValue_At_OnAttributeRender");
			Assert.AreEqual ("<div attr=\"cookies&amp;cream\">\n\n</div>", sw.ToString ());
		}
		
		[Test]
		public void EscapeAttribute2 () {
			sw = new StringWriter ();
			sw.NewLine = "\n"; // Keep sanity.
			Poker w1 = new Poker (sw);

			w1.AddAttribute ("attr", "cookies&cream", false);
			w1.RenderBeginTag (HtwTag.Div);
			w1.RenderEndTag ();
			
			Assert.AreEqual ("cookies&cream", w1.AttrValue_At_AddAttribute, "AttrValue_At_AddAttribute");
			Assert.AreEqual ("cookies&cream", w1.AttrValue_At_OnAttributeRender, "AttrValue_At_OnAttributeRender");
			Assert.AreEqual ("<div attr=\"cookies&cream\">\n\n</div>", sw.ToString ());
		}
		
		[Test]
		public void EscapeAttribute3 () {
			sw = new StringWriter ();
			sw.NewLine = "\n"; // Keep sanity.
			Poker w1 = new Poker (sw);

			w1.AddAttribute ("attr", "cookies&cream", true);
			w1.RenderBeginTag (HtwTag.Div);
			w1.RenderEndTag ();
			
			Assert.AreEqual ("cookies&amp;cream", w1.AttrValue_At_AddAttribute, "AttrValue_At_AddAttribute");
			Assert.AreEqual ("cookies&amp;cream", w1.AttrValue_At_OnAttributeRender, "AttrValue_At_OnAttributeRender");
			Assert.AreEqual ("<div attr=\"cookies&amp;cream\">\n\n</div>", sw.ToString ());
		}	

		[Test]
		public void WriteInput ()
		{
			w.RenderBeginTag (HtwTag.Input);
			w.RenderEndTag ();
			Assert.AreEqual ("<input />", sw.ToString ());
		}
		[Test]
		public void WriteInputStringTag ()
		{
			w.RenderBeginTag ("input");
			w.RenderEndTag ();
			Assert.AreEqual ("<input />", sw.ToString ());
		}

		[Test]
		public void WriteDivStringTag ()
		{
			w.RenderBeginTag ("div");
			w.RenderEndTag ();
			Assert.AreEqual ("<div>\n\n</div>", sw.ToString ());
		}

		[Test]
		public void WriteUnknownTag ()
		{
			w.RenderBeginTag ("somerandomtag");
			w.RenderEndTag ();
			Assert.AreEqual ("<somerandomtag>\n\n</somerandomtag>", sw.ToString ());	
		}
		
		[Test]
		public void WritePartialBlock ()
		{
			w.RenderBeginTag ("div");
			Assert.AreEqual ("<div>\n", sw.ToString ());	
		}
		
		[Test]
		public void WritePartialInline ()
		{
			w.RenderBeginTag ("a");
			Assert.AreEqual ("<a>", sw.ToString ());	
		}

		[Test]
		public void WritePartialSelfClosing ()
		{
			w.RenderBeginTag ("input");
			Assert.AreEqual ("<input />", sw.ToString ());	
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoMatchingClose ()
		{
			w.RenderEndTag ();
		}
		
		[Test]
		public void AttributeAfterBegin ()
		{
			w.RenderBeginTag ("div");
			w.AddAttribute ("class", "foo");
			w.RenderEndTag ();
			w.RenderBeginTag ("div");
			w.RenderEndTag ();
			
			Assert.AreEqual ("<div>\n\n</div><div class=\"foo\">\n\n</div>", sw.ToString ());	
		}
		
		[Test]
		public void StyleAttribute ()
		{
			w.WriteStyleAttribute ("a", "b");
			
			Assert.AreEqual ("a:b;", sw.ToString ());	
		}

		[Test]
		public void TagByNameGetsCaseChanged ()
		{
			w.RenderBeginTag ("InPuT");
			Assert.AreEqual ("<input />", sw.ToString ());	
		}

		[Test]
		public void EnsurePopWorks ()
		{
			w.RenderBeginTag (HtwTag.A);
			w.RenderBeginTag (HtwTag.B);
			w.RenderEndTag ();
			w.RenderEndTag ();

			Assert.AreEqual ("<a><b></b></a>", sw.ToString ());
		}

		[Test]
		public void AddAttributeTwice_SameValue ()
		{
			w.AddAttribute (HtmlTextWriterAttribute.Border, "0");
			w.AddAttribute (HtmlTextWriterAttribute.Border, "0");
			w.RenderBeginTag ("div");
			w.RenderEndTag ();

			Assert.AreEqual ("<div border=\"0\" border=\"0\">\n\n</div>", sw.ToString ());
		}

		[Test]
		public void AddAttributeTwice_DifferentValue ()
		{
			w.AddAttribute (HtmlTextWriterAttribute.Border, "0");
			w.AddAttribute (HtmlTextWriterAttribute.Border, "1");
			w.RenderBeginTag ("div");
			w.RenderEndTag ();

			Assert.AreEqual ("<div border=\"0\" border=\"1\">\n\n</div>", sw.ToString ());
		}

		[Test]
		public void AddStyleAttribute1 ()
		{
			w.AddStyleAttribute (HtmlTextWriterStyle.BackgroundImage, "http://www.example.com/");
			w.RenderBeginTag ("div");
			w.RenderEndTag ();

			Assert.AreEqual ("<div style=\"background-image:url(http://www.example.com/);\">\n\n</div>", sw.ToString ());
		}

		[Test]
		public void AddStyleAttribute3 ()
		{
			sw = new StringWriter ();
			sw.NewLine = "\n"; // Keep sanity.
			Poker w1 = new Poker (sw);

			w1.AddStyleAttribute ("mystyle", "my value&space");
			w1.RenderBeginTag ("div");
			w1.RenderEndTag ();
			Assert.AreEqual ("my value&space", w1.StyleValue_At_AddStyleAttribute, "StyleValue_At_AddStyleAttribute");
			Assert.AreEqual ("my value&amp;space", w1.StyleValue_At_OnStyleAttributeRender, "StyleValue_At_OnStyleAttributeRender");
			Assert.AreEqual ("<div style=\"mystyle:my value&amp;space;\">\n\n</div>", sw.ToString ());
		}

		[Test]
		public void WriteAttribute1 ()
		{
			w.WriteAttribute ("attr", "my value&space");
			Assert.AreEqual (" attr=\"my value&space\"", sw.ToString ());
		}

		[Test]
		public void WriteAttribute2 ()
		{
			w.WriteAttribute ("attr", "my value&space", false);
			Assert.AreEqual (" attr=\"my value&space\"", sw.ToString ());
		}

		[Test]
		public void WriteAttribute3 ()
		{
			w.WriteAttribute ("attr", "my value&space", true);
			Assert.AreEqual (" attr=\"my value&amp;space\"", sw.ToString ());
		}

		[Test]
		public void WriteStyleAttribute1 ()
		{
			w.WriteStyleAttribute ("mystyle", "my value&space");
			Assert.AreEqual ("mystyle:my value&space;", sw.ToString ());
		}

		[Test]
		public void WriteStyleAttribute2 ()
		{
			w.WriteStyleAttribute ("mystyle", "my value&space", false);
			Assert.AreEqual ("mystyle:my value&space;", sw.ToString ());
		}

		[Test]
		public void WriteStyleAttribute3 ()
		{
			w.WriteStyleAttribute ("mystyle", "my value&space", true);
			Assert.AreEqual ("mystyle:my value&amp;space;", sw.ToString ());
		}
		
		[Test]
		public void WriteStyleAttribute_BackgroundImage1 () 
		{
			w.WriteStyleAttribute ("background-image", "http://www.mainsoft.com/space here?a=b&c=d");
			Assert.AreEqual ("background-image:http://www.mainsoft.com/space here?a=b&c=d;", sw.ToString ());
		}
		
		[Test]
		public void WriteStyleAttribute_BackgroundImage2 ()
		{
			w.WriteStyleAttribute ("BackGround-Image", "http://www.mainsoft.com/space here?a=b&c=d");
			Assert.AreEqual ("BackGround-Image:http://www.mainsoft.com/space here?a=b&c=d;", sw.ToString ());
		}
		
		[Test]
		public void AddStyleAttribute_BackgroundImage1 () 
		{
			sw = new StringWriter ();
			sw.NewLine = "\n"; // Keep sanity.
			Poker w1 = new Poker (sw);

			w1.AddStyleAttribute (HtmlTextWriterStyle.BackgroundImage, "http://www.mainsoft.com/space here?a=b&c=d");
			w1.RenderBeginTag ("div");
			w1.RenderEndTag ();

			Assert.AreEqual ("http://www.mainsoft.com/space here?a=b&c=d", w1.StyleValue_At_AddStyleAttribute, "StyleValue_At_AddStyleAttribute");
			Assert.AreEqual ("http://www.mainsoft.com/space here?a=b&amp;c=d", w1.StyleValue_At_OnStyleAttributeRender, "StyleValue_At_OnStyleAttributeRender");
			Assert.AreEqual ("<div style=\"background-image:url(http://www.mainsoft.com/space%20here?a=b&amp;c=d);\">\n\n</div>", sw.ToString ());
		}

		[Test]
		public void AddStyleAttribute_BackgroundImage2 ()
		{
			sw = new StringWriter ();
			sw.NewLine = "\n"; // Keep sanity.
			Poker w1 = new Poker (sw);

			w1.AddStyleAttribute ("background-image", "http://www.mainsoft.com/space here?a=b&c=d");
			w1.RenderBeginTag ("div");
			w1.RenderEndTag ();

			Assert.AreEqual ("http://www.mainsoft.com/space here?a=b&c=d", w1.StyleValue_At_AddStyleAttribute, "StyleValue_At_AddStyleAttribute");
			Assert.AreEqual ("http://www.mainsoft.com/space here?a=b&amp;c=d", w1.StyleValue_At_OnStyleAttributeRender, "StyleValue_At_OnStyleAttributeRender");
			Assert.AreEqual ("<div style=\"background-image:url(http://www.mainsoft.com/space%20here?a=b&amp;c=d);\">\n\n</div>", sw.ToString ());
		}

		[Test]
		public void AddStyleAttribute_BackgroundImage3 ()
		{
			sw = new StringWriter ();
			sw.NewLine = "\n"; // Keep sanity.
			Poker w1 = new Poker (sw);
			
			w1.AddStyleAttribute ("BackGround-Image", "http://www.mainsoft.com/space here?a=b&c=d");
			w1.RenderBeginTag ("div");
			w1.RenderEndTag ();

			Assert.AreEqual ("http://www.mainsoft.com/space here?a=b&c=d", w1.StyleValue_At_AddStyleAttribute, "StyleValue_At_AddStyleAttribute");
			Assert.AreEqual ("http://www.mainsoft.com/space here?a=b&amp;c=d", w1.StyleValue_At_OnStyleAttributeRender, "StyleValue_At_OnStyleAttributeRender");
			Assert.AreEqual ("<div style=\"BackGround-Image:url(http://www.mainsoft.com/space%20here?a=b&amp;c=d);\">\n\n</div>", sw.ToString ());
		}


		[Test]
		public void AddStyleAttribute2 ()
		{
			w.AddStyleAttribute (HtmlTextWriterStyle.BackgroundColor, "Aqua");
			w.RenderBeginTag ("div");
			w.RenderEndTag ();

			Assert.AreEqual ("<div style=\"background-color:Aqua;\">\n\n</div>", sw.ToString ());
		}

		[Test]
		public void WriteIndentation ()
		{
			w.RenderBeginTag (HtwTag.Div);
			w.Write ("Hello");
			w.RenderEndTag ();

			Assert.AreEqual ("<div>\n\tHello\n</div>", sw.ToString ());
		}

		[Test]
		public void WriteIndentation2 ()
		{
			w.RenderBeginTag (HtwTag.Div);
			w.Write ("");
			w.RenderEndTag ();

			Assert.AreEqual ("<div>\n\t\n</div>", sw.ToString ());
		}

		[Test]
		public void WriteIndentation3 ()
		{
			w.RenderBeginTag (HtwTag.Div);
			w.RenderEndTag ();

			Assert.AreEqual ("<div>\n\n</div>", sw.ToString ());
		}

		[Test]
		public void WriteBreakTest ()
		{
			w.WriteBreak ();
			Assert.AreEqual ("<br />", sw.ToString (), "WriteBreakTest01");
		}
		
		class MyHttpTextWriter : HtmlTextWriter {
			int i = 0;
			public MyHttpTextWriter(TextWriter tw) : base(tw) {
	
			}
			protected override bool OnAttributeRender(string name, string value, HtmlTextWriterAttribute key) {
				switch (i++) {
				case 0:
				case 1: Assert.AreEqual(HtmlTextWriterAttribute.Border, key);
					break;
				case 2: Assert.AreEqual((HtmlTextWriterAttribute)(-1), key);
					break;
				}
				return base.OnAttributeRender (name, value, key);
			}
	
		}

		class Poker : HtmlTextWriter
		{
			public string StyleValue_At_OnStyleAttributeRender;
			public string StyleValue_At_AddStyleAttribute;
			public string AttrValue_At_OnAttributeRender;
			public string AttrValue_At_AddAttribute;

			public Poker (TextWriter tw)
				: base (tw)
			{
			}

			protected override bool OnAttributeRender (string name, string value, HtmlTextWriterAttribute key)
			{
				AttrValue_At_OnAttributeRender = value;
				return base.OnAttributeRender (name, value, key);
			}
			
			protected override bool OnStyleAttributeRender (string name, string value, HtmlTextWriterStyle key)
			{
				StyleValue_At_OnStyleAttributeRender = value;
				return base.OnStyleAttributeRender (name, value, key);
			}
			
			protected override void AddStyleAttribute (string name, string value, HtmlTextWriterStyle key)
			{
				StyleValue_At_AddStyleAttribute = value;
				base.AddStyleAttribute (name, value, key);
			}

			protected override void AddAttribute (string name, string value, HtmlTextWriterAttribute key)
			{
				AttrValue_At_AddAttribute = value;
				base.AddAttribute (name, value, key);
			}

		}
		
		[Test]
		public void TestOnAttributeRender() {
			MyHttpTextWriter myw = new MyHttpTextWriter (sw);
			
			myw.AddAttribute ("border", "0");
			myw.AddAttribute ("BoRDeR", "0");
			myw.AddAttribute ("error", "0");
			myw.RenderBeginTag ("div");
			myw.RenderEndTag ();
		}
	}
}
