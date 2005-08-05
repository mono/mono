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
	class DataGridTableStyleTest
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

			Assert.AreEqual (true, dg.AllowSorting, "AllowSorting property");
			Assert.AreEqual (true, dg.ColumnHeadersVisible, "ColumnHeadersVisible property");
			Assert.AreEqual (DataGridLineStyle.Solid, dg.GridLineStyle, "GridLineStyle property");
			Assert.AreEqual (75, dg.PreferredColumnWidth, "PreferredColumnWidth property");
			Assert.AreEqual (16, dg.PreferredRowHeight, "PreferredRowHeight property");
			Assert.AreEqual (false, dg.ReadOnly, "ReadOnly property");
			Assert.AreEqual (true, dg.RowHeadersVisible, "RowHeadersVisible property");
			Assert.AreEqual (35, dg.RowHeaderWidth, "RowHeaderWidth property");
		}

		[Test]
		public void TestAllowSortingChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.AllowSortingChanged   += new EventHandler (OnEventHandler);
			dg.AllowSorting = !dg.AllowSorting;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestAlternatingBackColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.AlternatingBackColorChanged  += new EventHandler (OnEventHandler);
			dg.AlternatingBackColor = Color.Red;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Ignore ("Microsoft lunches ForeColor event instead of BackColor")]
		public void TestBackColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.BackColorChanged += new EventHandler (OnEventHandler);
			dg.BackColor = Color.Yellow;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestColumnHeadersVisibleChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.ColumnHeadersVisibleChanged   += new EventHandler (OnEventHandler);
			dg.ColumnHeadersVisible = !dg.ColumnHeadersVisible;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Ignore ("Microsoft lunches  BackColor event instead of ForeColor")]
		public void TestForeColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.ForeColorChanged   += new EventHandler (OnEventHandler);
			dg.ForeColor = Color.Red;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestGridLineColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.GridLineColorChanged += new EventHandler (OnEventHandler);
			dg.GridLineColor = Color.Red;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestGridLineStyleChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.GridLineStyleChanged += new EventHandler (OnEventHandler);
			dg.GridLineStyle = DataGridLineStyle.None;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestHeaderBackColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.HeaderBackColorChanged  += new EventHandler (OnEventHandler);
			dg.HeaderBackColor = Color.Red;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestHeaderFontChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.HeaderFontChanged += new EventHandler (OnEventHandler);
			dg.HeaderFont = new Font ("Arial", 20);
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestHeaderForeColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.HeaderForeColorChanged += new EventHandler (OnEventHandler);
			dg.HeaderForeColor = Color.Red;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestLinkColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.LinkColorChanged += new EventHandler (OnEventHandler);
			dg.LinkColor = Color.Red;
			Assert.AreEqual (true, eventhandled, "A1");
		}


		[Ignore ("Microsoft is not firing any event")]
		public void TestLinkHoverColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.LinkHoverColorChanged += new EventHandler (OnEventHandler);
			dg.LinkHoverColor = Color.Red;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestMappingNameChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.MappingNameChanged += new EventHandler (OnEventHandler);
			dg.MappingName = "name1";
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestPreferredColumnWidthChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.PreferredColumnWidthChanged += new EventHandler (OnEventHandler);
			dg.PreferredColumnWidth = dg.PreferredColumnWidth++;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestPreferredRowHeightChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.PreferredRowHeightChanged += new EventHandler (OnEventHandler);
			dg.PreferredRowHeight = dg.PreferredRowHeight++;
			Assert.AreEqual (true, eventhandled, "A1");
		}
		[Test]
		public void TestReadOnlyChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.ReadOnlyChanged += new EventHandler (OnEventHandler);
			dg.ReadOnly = !dg.ReadOnly;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestRowHeadersVisibleChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.RowHeadersVisibleChanged += new EventHandler (OnEventHandler);
			dg.RowHeadersVisible = !dg.RowHeadersVisible;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestRowHeaderWidthChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.RowHeaderWidthChanged += new EventHandler (OnEventHandler);
			dg.RowHeaderWidth = dg.RowHeaderWidth++;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestSelectionBackColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.SelectionBackColorChanged   += new EventHandler (OnEventHandler);
			dg.SelectionBackColor = Color.Red;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestSelectionForeColorChangedEvent ()
		{
			DataGridTableStyle dg = new DataGridTableStyle ();
			eventhandled = false;
			dg.SelectionForeColorChanged  += new EventHandler (OnEventHandler);
			dg.SelectionForeColor = Color.Red;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		public void OnEventHandler (object sender, EventArgs e)
	        {
	            	eventhandled = true;
	        }
	}
}
