//
// HyperLinkColumnTest.cs
//
// Author: Duncan Mak (duncan@novell.com)
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
using System.Collections;
using System.Diagnostics;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls {

	[TestFixture]
	public class HyperLinkColumnTest {

		[Test]
		public void SetUpTest ()
		{
			HyperLinkColumn column = new HyperLinkColumn ();
			Assert.AreEqual (String.Empty, column.DataNavigateUrlField, "#1");
			Assert.AreEqual (String.Empty, column.DataTextField, "2");
			Assert.AreEqual (String.Empty, column.DataTextFormatString, "#3");
			Assert.AreEqual (String.Empty, column.NavigateUrl, "#4");			
			Assert.AreEqual (String.Empty, column.Target, "#5");
			Assert.AreEqual (String.Empty, column.Text, "#6");
		}

		[Test]
		public void DataNavigateUrlFieldTest ()
		{
			HyperLinkColumn column = new HyperLinkColumn ();
			string foo = "foo";
			string bar = "bar";

			column.NavigateUrl = foo;
			Assert.AreEqual (foo, column.NavigateUrl, "#1");

			// Test the bit about DataNavigateUrlField having precedence over NavigateUrl
			column.DataNavigateUrlField = bar;
			Assert.AreEqual (bar, column.DataNavigateUrlField, "#2");
			// what does this mean? shouldn't NavigateUrl be "bar" now?
			Assert.AreEqual (foo, column.NavigateUrl, "#3"); 
		}

		public class MyColumn : HyperLinkColumn {
			public string FormatUrl (object input)
			{
				return FormatDataNavigateUrlValue (input);
			}

			public string FormatText (object input)
			{
				return FormatDataTextValue (input);
			}

			public void InitCell (TableCell cell, int column_index, ListItemType item_type)
			{
			  base.InitializeCell (cell, column_index, item_type);
			}
		}

		[Test]
		public void FormatTest ()
		{
			MyColumn column = new MyColumn ();
			column.DataNavigateUrlFormatString = "!{0}!";
			Assert.AreEqual (String.Empty, column.FormatUrl (null), "#1");
			Assert.AreEqual ("!foo!", column.FormatUrl ("foo"), "#2");

			column.DataTextFormatString = "!{0}!";
			Assert.AreEqual (String.Empty, column.FormatText (null), "#3");
			Assert.AreEqual ("!foo!", column.FormatText ("foo"), "#4");
		}

		[Test]
		public void InitCellTest ()
		{
			MyColumn column;
			TableCell cell;

			/* test that for Header it just sets the cell.Text to HeaderText */
			column = new MyColumn();
			cell = new TableCell();
			column.HeaderText = "This is a Header";
			column.InitCell (cell, 0, ListItemType.Header);

			Assert.AreEqual ("This is a Header", cell.Text, "#1");

			/* test that for Item it adds a HyperLinkControl */
			column = new MyColumn();
			cell = new TableCell();
			column.NavigateUrl = "http://www.novell.com/";
			column.Text = "Novell.com";
			column.InitCell (cell, 0, ListItemType.Item);

			Assert.AreEqual (1, cell.Controls.Count, "#2");
			Assert.IsTrue (cell.Controls[0] is HyperLink, "#3");

			/* test that for EditItem it adds a HyperLinkControl */
			column = new MyColumn();
			cell = new TableCell();
			column.NavigateUrl = "http://www.novell.com/";
			column.Text = "Novell.com";
			column.InitCell (cell, 0, ListItemType.EditItem);

			Assert.AreEqual (1, cell.Controls.Count, "#4");
			Assert.IsTrue (cell.Controls[0] is HyperLink, "#5");

			/* test that for AlternatingItem it adds a HyperLinkControl */
			column = new MyColumn();
			cell = new TableCell();
			column.NavigateUrl = "http://www.novell.com/";
			column.Text = "Novell.com";
			column.InitCell (cell, 0, ListItemType.AlternatingItem);

			Assert.AreEqual (1, cell.Controls.Count, "#6");
			Assert.IsTrue (cell.Controls[0] is HyperLink, "#7");

			/* test that for Footer it just sets the cell.Text to FooterText */
			column = new MyColumn();
			cell = new TableCell();
			column.FooterText = "This is a Footer";
			column.InitCell (cell, 0, ListItemType.Footer);

			Assert.AreEqual ("This is a Footer", cell.Text, "#8");
		}

	}
}
