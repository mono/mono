//
// HtmlFormTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlForm
//
// Author:
//	Dick Porter  <dick@ximian.com>
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
using System.Web.UI.HtmlControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

	[TestFixture]
	public class HtmlFormTest {

		[Test]
		public void DefaultProperties ()
		{
			HtmlForm form = new HtmlForm ();
			Assert.AreEqual (0, form.Attributes.Count, "Attributes.Count");

			Assert.AreEqual (String.Empty, form.Enctype, "Enctype");
			Assert.AreEqual ("post", form.Method, "Method");
			Assert.AreEqual (form.UniqueID, form.Name, "Name");
			Assert.AreEqual (String.Empty, form.Target, "Target");

			Assert.AreEqual ("form", form.TagName, "TagName");
		}

		[Test]
		public void NullProperties ()
		{
			HtmlForm form = new HtmlForm ();

			form.Enctype = null;
			Assert.AreEqual (String.Empty, form.Enctype, "Enctype");
			form.Method = null;
			Assert.AreEqual ("post", form.Method, "Method");
			form.Name = null;
			Assert.AreEqual (form.UniqueID, form.Name, "Name");
			form.Target = null;
			Assert.AreEqual (String.Empty, form.Target, "Target");

			Assert.AreEqual (0, form.Attributes.Count, "Attributes.Count");
		}
	}
}
