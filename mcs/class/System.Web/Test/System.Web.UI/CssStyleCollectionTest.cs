//
// Tests for System.Web.UI.CssStyleCollection.cs 
//
// Author:
//	Igor Zelmanovich (igorz@mainsoft.com)
//
//
// Copyright (C) 2006 Mainsoft, Inc (http://www.mainsoft.com)
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
using System.Collections;
using System.Drawing;
using System.IO;
using System.Globalization;
using refl = System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using System.Text;

namespace MonoTests.System.Web.UI
{
	[TestFixture]
	public class CssStyleCollectionTest
	{

		[Test]
		public void CssStyleCollection_BackgroundImage () {
			WebControl c = new WebControl (HtmlTextWriterTag.A);
			CssStyleCollection css = c.Style;
			string url = "http://www.example.com/space here?key1=val1&key2=val2";
			string url_encoded = "http://www.example.com/space%20here?key1=val1&key2=val2";
			
			css.Add (HtmlTextWriterStyle.BackgroundImage, url);

			Assert.AreEqual (url, css ["background-image"], "CssStyleCollection_BackgroundImage#1");
			Assert.AreEqual ("background-image:url(" + url_encoded + ");", css.Value, "CssStyleCollection_BackgroundImage#3");
			Assert.AreEqual ("background-image:url(" + url_encoded + ");", c.Attributes["style"], "CssStyleCollection_BackgroundImage#4");
		}

		[Test]
		public void CssStyleCollection_BackgroundImage2 () {
			WebControl c = new WebControl (HtmlTextWriterTag.A);
			CssStyleCollection css = c.Style;
			string url = "http://www.example.com/space here?key1=val1&key2=val2";
			string url_encoded = "http://www.example.com/space%20here?key1=val1&key2=val2";

			css.Add ("background-image", url);

			Assert.AreEqual (url, css ["background-image"], "CssStyleCollection_BackgroundImage#1");
			Assert.AreEqual ("background-image:url(" + url_encoded + ");", css.Value, "CssStyleCollection_BackgroundImage#3");
			Assert.AreEqual ("background-image:url(" + url_encoded + ");", c.Attributes ["style"], "CssStyleCollection_BackgroundImage#4");
		}

		[Test]
		public void CssStyleCollection_BackgroundImage3 () {
			WebControl c = new WebControl (HtmlTextWriterTag.A);
			CssStyleCollection css = c.Style;
			string url = "http://www.example.com/space here?key1=val1&key2=val2";
			string url_encoded = "http://www.example.com/space%20here?key1=val1&key2=val2";

			css.Add ("background-image", "url(" + url_encoded + ")");

			Assert.AreEqual ("url(" + url_encoded + ")", css ["background-image"], "CssStyleCollection_BackgroundImage#1");
			Assert.AreEqual ("background-image:url(" + url_encoded + ");", css.Value, "CssStyleCollection_BackgroundImage#3");
			Assert.AreEqual ("background-image:url(" + url_encoded + ");", c.Attributes ["style"], "CssStyleCollection_BackgroundImage#4");
		}

		[Test]
		public void CssStyleCollection_BackgroundImage4 () {
			WebControl c = new WebControl (HtmlTextWriterTag.A);
			CssStyleCollection css = c.Style;
			string url = "http://www.example.com/space here?key1=val1&key2=val2";
			string url_encoded = "http://www.example.com/space%20here?key1=val1&key2=val2";

			c.Attributes ["style"] = "background-image:url(" + url_encoded + ");";

			Assert.AreEqual ("url(" + url_encoded + ")", css ["background-image"], "CssStyleCollection_BackgroundImage#1");
			Assert.AreEqual ("background-image:url(" + url_encoded + ");", css.Value, "CssStyleCollection_BackgroundImage#3");
			Assert.AreEqual ("background-image:url(" + url_encoded + ");", c.Attributes ["style"], "CssStyleCollection_BackgroundImage#4");
		}

		[Test]
		public void CssStyleCollection_Enumerator () {
			WebControl c = new WebControl (HtmlTextWriterTag.A);
			c.BackColor = Color.Beige;
			c.ForeColor = Color.Brown;
			c.Font.Bold = true;
			c.Attributes ["style"] = "padding: 0px; margin: 0px";

			Assert.AreEqual (2, c.Style.Count, "Style Count");
			Assert.AreEqual (3, c.ControlStyle.GetStyleAttributes (c).Count, "ControlStyle Count");

			CssStyleCollection col = c.Style;
			NameValueCollection styles = new NameValueCollection ();
			foreach (string key in col.Keys) {
				styles [key] = col [key];
			}
			Assert.AreEqual ("0px", styles ["padding"], "Style padding");
			Assert.AreEqual ("0px",styles ["margin"],  "Style margin");
		}

		[Test]
		public void CssStyleCollection_Style_Attribute () {
			WebControl c = new WebControl (HtmlTextWriterTag.A);
			Assert.IsTrue (Object.ReferenceEquals (c.Style, c.Attributes.CssStyle));

			// style attribute is parsed to CssStyleCollection
			c.Attributes.Add ("style", "padding: 1px; margin: 2px");
			Assert.AreEqual (2, c.Style.Count, "Style Count");
			Assert.AreEqual ("1px", c.Style ["padding"], "");
			Assert.AreEqual ("2px", c.Style ["margin"], "");

			// CssStyleCollection is merged to style attribute
			c.Style.Add ("color", "red");
			Assert.AreEqual (3, c.Style.Count, "Style Count");
			Assert.AreEqual ("red", c.Style ["color"], "");
			Assert.IsTrue (c.Attributes ["style"].IndexOf("color:")>=0);

			// replacing style attribute replaces CssStyleCollection's items
			c.Attributes ["style"] = "align: center";
			Assert.AreEqual (1, c.Style.Count, "Style Count");
			Assert.AreEqual ("center", c.Style ["align"], "");

			// removing style attribute clears CssStyleCollection
			c.Attributes.Remove("style");
			Assert.AreEqual (0, c.Style.Count, "Style Count");

			// adding to CssStyleCollection create style attribute
			c.Style.Add ("color", "red");
			Assert.AreEqual (1, c.Attributes.Count, "Attributes Count");

			c.Attributes ["style"] = "align: center; color: red;";
			Assert.AreEqual (2, c.Style.Count, "Style Count");
			Assert.AreEqual ("center", c.Style ["align"], "");
			Assert.AreEqual ("red", c.Style ["color"], "");

			// clearing CssStyleCollection removes style attribute
			c.Style.Clear ();
			Assert.AreEqual (0, c.Attributes.Count, "Attributes Count");
		}

		[Test]
		public void CssStyleCollection_case_sensitive  () {
			WebControl c = new WebControl (HtmlTextWriterTag.A);
			c.Style.Add ("color", "red");
			Assert.AreEqual ("red", c.Style ["Color"], "");
			c.Style.Add ("Color", "Blue");
			Assert.AreEqual ("Blue", c.Style ["color"], "");
			Assert.AreEqual (1, c.Style.Count, "Style Count");
		}
	}
}
