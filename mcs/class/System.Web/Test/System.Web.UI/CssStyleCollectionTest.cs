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
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.UI
{
	[TestFixture]
	public class CssStyleCollectionTest
	{

#if NET_2_0
		[Test]
		public void CssStyleCollection_BackgroundImage () {
			WebControl c = new WebControl (HtmlTextWriterTag.A);
			CssStyleCollection css = c.Style;
			string url = "http://www.go-mono.com/space here?key1=val1&key2=val2";
			string url_encoded = "http://www.go-mono.com/space%20here?key1=val1&key2=val2";
			
			css.Add (HtmlTextWriterStyle.BackgroundImage, url);

			Assert.AreEqual (url, css ["background-image"], "CssStyleCollection_BackgroundImage#1");
			Assert.AreEqual (url, css [HtmlTextWriterStyle.BackgroundImage], "CssStyleCollection_BackgroundImage#2");
			Assert.AreEqual ("background-image:url(" + url_encoded + ");", css.Value, "CssStyleCollection_BackgroundImage#3");
		}

#endif

	}
}
