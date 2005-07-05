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
	class DataGridTableStyleTest : Assertion
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
			DataGridTableStyle dg = new DataGridTableStyle ();

			AssertEquals ("AllowSorting property", true, dg.AllowSorting);
			AssertEquals ("ColumnHeadersVisible property", true, dg.ColumnHeadersVisible);
			AssertEquals ("GridLineStyle property", DataGridLineStyle.Solid, dg.GridLineStyle);
			AssertEquals ("PreferredColumnWidth property", 75, dg.PreferredColumnWidth);
			AssertEquals ("PreferredRowHeight property", 16, dg.PreferredRowHeight);
			AssertEquals ("ReadOnly property", false, dg.ReadOnly);
			AssertEquals ("RowHeadersVisible property", true, dg.RowHeadersVisible);
			AssertEquals ("RowHeaderWidth property", 35, dg.RowHeaderWidth);
		}

		[Test]
		public void TestAllowSortingChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.AllowSortingChanged   += new EventHandler (OnEventHandler);
			dg.AllowSorting = !dg.AllowSorting;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestAlternatingBackColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.AlternatingBackColorChanged  += new EventHandler (OnEventHandler);
			dg.AlternatingBackColor = Color.Red;
			AssertEquals (true, eventhandled);
		}

		[Ignore ("Microsoft lunches ForeColor event instead of BackColor")]
		public void TestBackColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.BackColorChanged += new EventHandler (OnEventHandler);
			dg.BackColor = Color.Yellow;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestColumnHeadersVisibleChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.ColumnHeadersVisibleChanged   += new EventHandler (OnEventHandler);
			dg.ColumnHeadersVisible = !dg.ColumnHeadersVisible;
			AssertEquals (true, eventhandled);
		}

		[Ignore ("Microsoft lunches  BackColor event instead of ForeColor")]
		public void TestForeColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.ForeColorChanged   += new EventHandler (OnEventHandler);
			dg.ForeColor = Color.Red;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestGridLineColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.GridLineColorChanged += new EventHandler (OnEventHandler);
			dg.GridLineColor = Color.Red;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestGridLineStyleChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.GridLineStyleChanged += new EventHandler (OnEventHandler);
			dg.GridLineStyle = DataGridLineStyle.None;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestHeaderBackColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.HeaderBackColorChanged  += new EventHandler (OnEventHandler);
			dg.HeaderBackColor = Color.Red;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestHeaderFontChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.HeaderFontChanged += new EventHandler (OnEventHandler);
			dg.HeaderFont = new Font ("Arial", 20);
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestHeaderForeColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.HeaderForeColorChanged += new EventHandler (OnEventHandler);
			dg.HeaderForeColor = Color.Red;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestLinkColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.LinkColorChanged += new EventHandler (OnEventHandler);
			dg.LinkColor = Color.Red;
			AssertEquals (true, eventhandled);
		}


		[Ignore ("Microsoft is not firing any event")]
		public void TestLinkHoverColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.LinkHoverColorChanged += new EventHandler (OnEventHandler);
			dg.LinkHoverColor = Color.Red;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestMappingNameChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.MappingNameChanged += new EventHandler (OnEventHandler);
			dg.MappingName = "name1";
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestPreferredColumnWidthChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.PreferredColumnWidthChanged += new EventHandler (OnEventHandler);
			dg.PreferredColumnWidth = dg.PreferredColumnWidth++;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestPreferredRowHeightChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.PreferredRowHeightChanged += new EventHandler (OnEventHandler);
			dg.PreferredRowHeight = dg.PreferredRowHeight++;
			AssertEquals (true, eventhandled);
		}
		[Test]
		public void TestReadOnlyChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.ReadOnlyChanged += new EventHandler (OnEventHandler);
			dg.ReadOnly = !dg.ReadOnly;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestRowHeadersVisibleChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.RowHeadersVisibleChanged += new EventHandler (OnEventHandler);
			dg.RowHeadersVisible = !dg.RowHeadersVisible;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestRowHeaderWidthChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.RowHeaderWidthChanged += new EventHandler (OnEventHandler);
			dg.RowHeaderWidth = dg.RowHeaderWidth++;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestSelectionBackColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.SelectionBackColorChanged   += new EventHandler (OnEventHandler);
			dg.SelectionBackColor = Color.Red;
			AssertEquals (true, eventhandled);
		}

		[Test]
		public void TestSelectionForeColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.SelectionForeColorChanged  += new EventHandler (OnEventHandler);
			dg.SelectionForeColor = Color.Red;
			AssertEquals (true, eventhandled);
		}

		public void OnEventHandler (object sender, EventArgs e)
	        {
	            	eventhandled = true;
	        }
	}
}
