//
// Tests for System.Web.UI.PostBackOptions.cs 
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

#if NET_2_0
using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

namespace MonoTests.System.Web.UI
{
	[TestFixture]
	public class PostBackOptionsTest
	{
		[Test]
		public void Constructors () {
			Control c = new WebControl(HtmlTextWriterTag.A);
			c.Page = new Page ();
			PostBackOptions options=new PostBackOptions(c);
			Assert.IsFalse (options.PerformValidation);
			Assert.IsFalse (options.AutoPostBack);
			Assert.IsFalse (options.TrackFocus);
			Assert.IsTrue (options.ClientSubmit);
			// MSDN: The default value is true, BUT FALSE
			Assert.IsFalse (options.RequiresJavaScriptProtocol);
			// MSDN: The default value is an empty string (""), BUT NULL
			Assert.AreEqual (null, options.Argument);
			// MSDN: The default value is an empty string (""), BUT NULL
			Assert.AreEqual (null, options.ActionUrl);
			// MSDN: The default value is an empty string (""), BUT NULL
			Assert.AreEqual (null, options.ValidationGroup);

			options = new PostBackOptions (c, null);
			Assert.IsFalse (options.PerformValidation);
			Assert.IsFalse (options.AutoPostBack);
			Assert.IsFalse (options.TrackFocus);
			Assert.IsTrue (options.ClientSubmit);
			Assert.IsFalse (options.RequiresJavaScriptProtocol);
			Assert.AreEqual (null, options.Argument);
			Assert.AreEqual (null, options.ActionUrl);
			Assert.AreEqual (null, options.ValidationGroup);

			options = new PostBackOptions (c, null, null, false, false, false, false, false, null);
			Assert.IsFalse (options.PerformValidation);
			Assert.IsFalse (options.AutoPostBack);
			Assert.IsFalse (options.TrackFocus);
			Assert.IsFalse (options.ClientSubmit);
			Assert.IsFalse (options.RequiresJavaScriptProtocol);
			Assert.AreEqual (null, options.Argument);
			Assert.AreEqual (null, options.ActionUrl);
			Assert.AreEqual (null, options.ValidationGroup);

			options = new PostBackOptions (c, "ARG");
			Assert.IsFalse (options.PerformValidation);
			Assert.IsFalse (options.AutoPostBack);
			Assert.IsFalse (options.TrackFocus);
			Assert.IsTrue (options.ClientSubmit);
			Assert.IsFalse (options.RequiresJavaScriptProtocol);
			Assert.AreEqual ("ARG", options.Argument);
			Assert.AreEqual (null, options.ActionUrl);
			Assert.AreEqual (null, options.ValidationGroup);

			options = new PostBackOptions (c, "ARG", "Page.aspx", true, true, false, false, false, "VG");
			Assert.IsFalse (options.PerformValidation);
			Assert.IsTrue (options.AutoPostBack);
			Assert.IsFalse (options.TrackFocus);
			Assert.IsFalse (options.ClientSubmit);
			Assert.IsTrue (options.RequiresJavaScriptProtocol);
			Assert.AreEqual ("ARG", options.Argument);
			Assert.AreEqual ("Page.aspx", options.ActionUrl);
			Assert.AreEqual ("VG", options.ValidationGroup);
		}
	}
}
#endif