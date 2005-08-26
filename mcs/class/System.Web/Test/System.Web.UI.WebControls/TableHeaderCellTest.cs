//
// TableHeaderCellTest.cs
//	- Unit tests for System.Web.UI.WebControls.TableHeaderCell
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {

	public class TestTableHeaderCell : TableHeaderCell {

		public string Tag {
			get { return base.TagName; }
		}

		public StateBag StateBag {
			get { return base.ViewState; }
		}

		public string Render ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}
	}

	[TestFixture]
	public class TableHeaderCellTest {

		[Test]
		public void DefaultProperties ()
		{
			TestTableHeaderCell th = new TestTableHeaderCell ();
			Assert.AreEqual (0, th.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, th.StateBag.Count, "ViewState.Count");
#if NET_2_0
			Assert.AreEqual (String.Empty, th.AbbreviatedText, "AbbreviatedText");
			Assert.AreEqual (0, th.CategoryText.Length, "CategoryText");
			Assert.AreEqual (TableHeaderScope.NotSet, th.Scope, "Scope");
#endif
			Assert.AreEqual ("th", th.Tag, "TagName");
			Assert.AreEqual (0, th.Attributes.Count, "Attributes.Count-2");
			Assert.AreEqual (0, th.StateBag.Count, "ViewState.Count-2");
		}

		[Test]
		public void NullProperties ()
		{
			TestTableHeaderCell th = new TestTableHeaderCell ();
#if NET_2_0
			th.AbbreviatedText = null;
			Assert.AreEqual (String.Empty, th.AbbreviatedText, "AbbreviatedText");

			th.CategoryText = new string[0];
			Assert.AreEqual (0, th.CategoryText.Length, "CategoryText");
			Assert.AreEqual (1, th.StateBag.Count, "ViewState.Count-1");

			th.Scope = TableHeaderScope.NotSet;
			Assert.AreEqual (TableHeaderScope.NotSet, th.Scope, "Scope");
			Assert.AreEqual (2, th.StateBag.Count, "ViewState.Count-2");
#else
			Assert.AreEqual (0, th.StateBag.Count, "ViewState.Count-1");
#endif
			Assert.AreEqual (0, th.Attributes.Count, "Attributes.Count");
		}

		[Test]
		public void CleanProperties ()
		{
			TestTableHeaderCell th = new TestTableHeaderCell ();
#if NET_2_0
			th.AbbreviatedText = "header";
			Assert.AreEqual ("header", th.AbbreviatedText, "AbbreviatedText");
			th.AbbreviatedText = null;
			Assert.AreEqual (String.Empty, th.AbbreviatedText, "-AbbreviatedText");
			Assert.AreEqual (0, th.StateBag.Count, "ViewState.Count-1");

			th.CategoryText = new string[1] { "mono" };
			Assert.AreEqual (1, th.CategoryText.Length, "CategoryText");
			th.CategoryText = new string[0];
			Assert.AreEqual (0, th.CategoryText.Length, "-CategoryText");
			Assert.AreEqual (1, th.StateBag.Count, "ViewState.Count-1");

			th.Scope = TableHeaderScope.Row;
			Assert.AreEqual (TableHeaderScope.Row, th.Scope, "Scope");
			th.Scope = TableHeaderScope.NotSet;
			Assert.AreEqual (TableHeaderScope.NotSet, th.Scope, "-Scope");
			Assert.AreEqual (2, th.StateBag.Count, "ViewState.Count-2");
#else
			Assert.AreEqual (0, th.StateBag.Count, "ViewState.Count-1");
#endif
			Assert.AreEqual (0, th.Attributes.Count, "Attributes.Count");
		}

		private string AdjustLineEndings (string s)
		{
			return s.Replace ("\r\n", Environment.NewLine);
		}

		[Test]
		public void Render ()
		{
			TestTableHeaderCell th = new TestTableHeaderCell ();
			string s = th.Render ();
			Assert.AreEqual (AdjustLineEndings ("<th></th>"), s, "empty/default");
#if NET_2_0
			th.AbbreviatedText = "header";
			s = th.Render ();
			Assert.AreEqual (AdjustLineEndings ("<th abbr=\"header\"></th>"), s, "AbbreviatedText");
			th.AbbreviatedText = null;

			th.CategoryText = new string[1] { "mono" };
			s = th.Render ();
			Assert.AreEqual (AdjustLineEndings ("<th axis=\"mono\"></th>"), s, "CategoryText-1");
			th.CategoryText = new string[2] { "mono", "http://www.mono-project.com" };
			s = th.Render ();
			Assert.AreEqual (AdjustLineEndings ("<th axis=\"mono,http://www.mono-project.com\"></th>"), s, "CategoryText-2");
			th.CategoryText = new string[3] { "mono", "http://www.mono-project.com", "," };
			s = th.Render ();
			Assert.AreEqual (AdjustLineEndings ("<th axis=\"mono,http://www.mono-project.com,,\"></th>"), s, "CategoryText-2");
			th.CategoryText = new string[0];
			s = th.Render ();
			Assert.AreEqual (AdjustLineEndings ("<th></th>"), s, "CategoryText-2");

			th.Scope = TableHeaderScope.Row;
			s = th.Render ();
			Assert.AreEqual (AdjustLineEndings ("<th scope=\"row\"></th>"), s, "Row");
			th.Scope = TableHeaderScope.Column;
			s = th.Render ();
			Assert.AreEqual (AdjustLineEndings ("<th scope=\"column\"></th>"), s, "Column");
			th.Scope = TableHeaderScope.NotSet;
			s = th.Render ();
			Assert.AreEqual (AdjustLineEndings ("<th></th>"), s, "NotSet");
#endif
			th.Text = "test";
			s = th.Render ();
			Assert.AreEqual (AdjustLineEndings ("<th>test</th>"), s, "Text");
		}
	}
}
