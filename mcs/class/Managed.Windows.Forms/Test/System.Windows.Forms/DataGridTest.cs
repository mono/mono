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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Jordi Mas i Hernandez <jordi@ximian.com>
//
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	class DataGridTest : Assertion
	{
		private bool eventhandled;

		[TearDown]
		public void Clean() {}

		[SetUp]
		public void GetReady ()
		{
		}

		[Test]
		public void TestDefaultValues ()
		{
			DataGrid dg = new DataGrid ();

			AssertEquals ("AllowNavigation property", true, dg.AllowNavigation);
			AssertEquals ("AllowSorting property", true, dg.AllowSorting);
			AssertEquals ("BorderStyle property", BorderStyle.Fixed3D, dg.BorderStyle);
			AssertEquals ("CaptionText property", string.Empty, dg.CaptionText);
			AssertEquals ("CaptionVisible property", true, dg.CaptionVisible);
			AssertEquals ("ColumnHeadersVisible property", true, dg.ColumnHeadersVisible);
			AssertEquals ("CurrentCell property", new DataGridCell (), dg.CurrentCell);
			AssertEquals ("CurrentRowIndex property", -1, dg.CurrentRowIndex);
			AssertEquals ("DataMember property", string.Empty, dg.DataMember);
			AssertEquals ("DataSource property", null, dg.DataSource);
			AssertEquals ("FirstVisibleColumn property", 0, dg.FirstVisibleColumn);
			AssertEquals ("FlatMode property", false, dg.FlatMode);
			AssertEquals ("GridLineStyle property", DataGridLineStyle.Solid, dg.GridLineStyle);
			AssertEquals ("ParentRowsLabelStyle property", DataGridParentRowsLabelStyle.Both, dg.ParentRowsLabelStyle);
			AssertEquals ("ParentRowsVisible property", true,dg.ParentRowsVisible);
			AssertEquals ("PreferredColumnWidth property", 75, dg.PreferredColumnWidth);
			AssertEquals ("PreferredRowHeight property", 16, dg.PreferredRowHeight);
			AssertEquals ("ReadOnly property", false, dg.ReadOnly);
			AssertEquals ("RowHeadersVisible property", true, dg.RowHeadersVisible);
			AssertEquals ("RowHeaderWidth property", 35, dg.RowHeaderWidth);
			AssertEquals ("Site property", null, dg.Site);
			AssertEquals ("Text property", string.Empty, dg.Text);
			AssertEquals ("VisibleColumnCount property", 0, dg.VisibleColumnCount);
		}

		[Test]
		public void TestAllowNavigationChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.AllowNavigationChanged += new EventHandler (OnEventHandler);
			dg.AllowNavigation = !dg.AllowNavigation;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestBackgroundColorChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.BackgroundColorChanged  += new EventHandler (OnEventHandler);
			dg.BackgroundColor = Color.Red;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestBorderStyleChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.BorderStyleChanged  += new EventHandler (OnEventHandler);
			dg.BorderStyle = BorderStyle.None;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestCaptionVisibleChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.CaptionVisibleChanged += new EventHandler (OnEventHandler);
			dg.CaptionVisible = !dg.CaptionVisible;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestFlatModeChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.FlatModeChanged += new EventHandler (OnEventHandler);
			dg.FlatMode = !dg.FlatMode;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestParentRowsLabelStyleChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.ParentRowsLabelStyleChanged  += new EventHandler (OnEventHandler);
			dg.ParentRowsLabelStyle = DataGridParentRowsLabelStyle.None;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestParentRowsVisibleChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.ParentRowsVisibleChanged  += new EventHandler (OnEventHandler);
			dg.ParentRowsVisible = !dg.ParentRowsVisible;
			AssertEquals (true, eventhandled);
		}
		
		[Test]
		public void TestReadOnlyChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.ReadOnlyChanged  += new EventHandler (OnEventHandler);
			dg.ReadOnly = !dg.ReadOnly;
			AssertEquals (true, eventhandled);
		}


		public void OnEventHandler (object sender, EventArgs e)
	        {
	            	eventhandled = true;
	        }
	}
}
