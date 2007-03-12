//
// Tests for System.Web.UI.AttributeCollection.cs and CssStyleCollection
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
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
using System.Collections;
using AC = System.Web.UI.AttributeCollection;

namespace MonoTests.System.Web.UI {
	[TestFixture]
	public class AttributeCollectionTest {
		[Test]
		public void InitialNoBag1 ()
		{
			AC ac = new AC (null);
			Assert.IsNotNull (ac.CssStyle, "style");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void InitialNoBag2 ()
		{
			AC ac = new AC (null);
			int i = ac.Count;
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void InitialNoBag3 ()
		{
			AC ac = new AC (null);
			ICollection coll = ac.Keys;
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void InitialNoBag4 ()
		{
			AC ac = new AC (null);
			string k = ac ["hola"];
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void InitialNoBag5 ()
		{
			AC ac = new AC (null);
			ac.Add ("att", "value");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void InitialNoBag6 ()
		{
			AC ac = new AC (null);
			ac.Clear ();
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void InitialNoBag7 ()
		{
			AC ac = new AC (null);
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			ac.AddAttributes (writer);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void InitialNoBag8 ()
		{
			AC ac = new AC (null);
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			ac.Render (writer);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void InitialNoBag9 ()
		{
			AC ac = new AC (null);
			ac.Remove ("hola");
		}

		[Test]
		public void InitialNoBag10()
		{
		    AC ac = new AC(null);
		    CssStyleCollection css = ac.CssStyle;
		    int i = css.Count;
		    Assert.AreEqual(0, i, "InitialNoBag10");
		}

		[Test]
		public void InitialNoBag11()
		{
		    AC ac = new AC(null);
		    CssStyleCollection css = ac.CssStyle;
		    ICollection coll = css.Keys;
		    Assert.AreEqual(0, coll.Count, "InitialNoBag11");
		}

		[Test]
		public void InitialNoBag12()
		{
		    AC ac = new AC(null);
		    CssStyleCollection css = ac.CssStyle;
		    string v = css["hola"];
		    Assert.AreEqual(null, v, "InitialNoBag12");
		}
			
		[Test]
		public void InitialBag1 ()
		{
			StateBag bag = new StateBag (true);
			AC ac = new AC (bag);
			Assert.AreEqual (0, ac.Count, "count");
			Assert.AreEqual (null, ac ["hola"], "item");
			Assert.AreEqual (0, ac.Keys.Count, "keys");
			ac.Add ("notexists", "invalid");
			ac.Remove ("notexists");
			ac.Remove ("notexists");

			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			ac.AddAttributes (writer);
			ac.Render (writer);
			Assert.AreEqual (0, writer.InnerWriter.ToString().Length, "length");
			CssStyleCollection css = ac.CssStyle;
			Assert.AreEqual (0, css.Count, "csscount");
			Assert.AreEqual (null, css ["hola"], "cssitem");
			Assert.AreEqual (0, css.Keys.Count, "csskeys");
			css.Add ("notexists", "invalid");
			css.Remove ("notexists");
			css.Remove ("notexists");
			css.Add ("notexists", "invalid");
			css.Clear ();
			Assert.AreEqual (0, css.Keys.Count, "csskeys2");
		}

		[Test]
		public void NonStyleAttributes1 ()
		{
			StateBag bag = new StateBag (true);
			AC ac = new AC (bag);
			StringWriter sr = new StringWriter ();
			HtmlTextWriter writer = new HtmlTextWriter (sr);
			ac.Add ("notexists", "somevalue");
			ac.AddAttributes (writer);
			string str = sr.ToString ();
			Assert.AreEqual ("", str, "value1");
			Assert.AreEqual (1, bag.Count, "count1");
			writer = new HtmlTextWriter (sr);
			writer.RenderBeginTag (HtmlTextWriterTag.A);
			ac.AddAttributes (writer);
			writer.RenderEndTag ();
			Assert.AreEqual ("", str, "value2");
			Assert.AreEqual (1, bag.Count, "count2");
		}

		[Test]
		public void NonStyleAttributes2 ()
		{
			StateBag bag = new StateBag (true);
			AC ac = new AC (bag);
			StringWriter sr = new StringWriter ();
			HtmlTextWriter writer = new HtmlTextWriter (sr);
			ac.Add ("class", "classname");
			ac.AddAttributes (writer);
			string str = sr.ToString ();
			Assert.AreEqual ("", str, "value1");
			Assert.AreEqual (1, bag.Count, "count1");
			writer = new HtmlTextWriter (sr);
			writer.RenderBeginTag (HtmlTextWriterTag.A);
			ac.AddAttributes (writer);
			writer.RenderEndTag ();
			Assert.AreEqual ("", str, "value2");
			Assert.AreEqual (1, bag.Count, "count2");
		}
		
		[Test]
		public void Count1 ()
		{
			StateBag bag = new StateBag (true);
			AC ac = new AC (bag);
			ac.Add ("style", "padding: 0px; margin: 0px");
			Assert.AreEqual (1, ac.Count, "AttributeCollection.Count");
			Assert.AreEqual (2, ac.CssStyle.Count, "AttributeCollection.Count");
			
			ac.Remove ("style");
			Assert.AreEqual (0, ac.Count, "AttributeCollection.Count");
			Assert.AreEqual (0, ac.CssStyle.Count, "AttributeCollection.Count");
		}

		[Test]
		public void Count2 ()
		{
			StateBag bag = new StateBag (true);
			AC ac = new AC (bag);
			ac ["style"] = "padding: 0px; margin: 0px";
			Assert.AreEqual (1, ac.Count, "AttributeCollection.Count");
			Assert.AreEqual (2, ac.CssStyle.Count, "AttributeCollection.Count");

			ac ["style"] = null;
			Assert.AreEqual (0, ac.Count, "AttributeCollection.Count");
			Assert.AreEqual (0, ac.CssStyle.Count, "AttributeCollection.Count");
		}

		[Test]
		public void Count3 ()
		{
			StateBag bag = new StateBag (true);
			AC ac = new AC (bag);
			ac.CssStyle.Add("padding", "0px");
			ac.CssStyle.Add("margin", "0px");
			Assert.AreEqual (1, ac.Count, "AttributeCollection.Count");
			Assert.AreEqual (2, ac.CssStyle.Count, "AttributeCollection.Count");

			ac.CssStyle.Remove ("padding");
			ac.CssStyle.Remove ("margin");
			Assert.AreEqual (0, ac.Count, "AttributeCollection.Count");
			Assert.AreEqual (0, ac.CssStyle.Count, "AttributeCollection.Count");
		}

#if NET_2_0
		[Test]
		public void Count4 ()
		{
			StateBag bag = new StateBag (true);
			AC ac = new AC (bag);
			ac.CssStyle ["padding"] = "0px";
			ac.CssStyle ["margin"] = "0px";
			Assert.AreEqual (1, ac.Count, "AttributeCollection.Count");
			Assert.AreEqual (2, ac.CssStyle.Count, "AttributeCollection.Count");

			ac.CssStyle.Value = null;
			Assert.AreEqual (0, ac.Count, "AttributeCollection.Count");
			Assert.AreEqual (0, ac.CssStyle.Count, "AttributeCollection.Count");
		}
#endif
	}
}

