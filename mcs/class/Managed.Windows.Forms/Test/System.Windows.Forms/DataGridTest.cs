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
	// Helper classes

	class TestDataGrid : DataGrid 
	{
		public TestDataGrid () 
		{

		}

		public CurrencyManager Manager {
			get {
				return ListManager;
			}
		}	
	}

	[TestFixture]
	class DataGridTest
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

			Assert.AreEqual (true, dg.AllowNavigation, "AllowNavigation property");
			Assert.AreEqual (true, dg.AllowSorting, "AllowSorting property");
			Assert.AreEqual (BorderStyle.Fixed3D, dg.BorderStyle, "BorderStyle property");
			Assert.AreEqual (string.Empty, dg.CaptionText, "CaptionText property");
			Assert.AreEqual (true, dg.CaptionVisible, "CaptionVisible property");
			Assert.AreEqual (true, dg.ColumnHeadersVisible, "ColumnHeadersVisible property");
			Assert.AreEqual (new DataGridCell (), dg.CurrentCell, "CurrentCell property");
			Assert.AreEqual (-1, dg.CurrentRowIndex, "CurrentRowIndex property");
			Assert.AreEqual (string.Empty, dg.DataMember, "DataMember property");
			Assert.AreEqual (null, dg.DataSource, "DataSource property");
			Assert.AreEqual (0, dg.FirstVisibleColumn, "FirstVisibleColumn property");
			Assert.AreEqual (false, dg.FlatMode, "FlatMode property");
			Assert.AreEqual (DataGridLineStyle.Solid, dg.GridLineStyle, "GridLineStyle property");
			Assert.AreEqual (DataGridParentRowsLabelStyle.Both, dg.ParentRowsLabelStyle, "ParentRowsLabelStyle property");
			Assert.AreEqual (true, dg.ParentRowsVisible, "ParentRowsVisible property");
			Assert.AreEqual (75, dg.PreferredColumnWidth, "PreferredColumnWidth property");
			//Assert.AreEqual (16, dg.PreferredRowHeight, "PreferredRowHeight property");
			Assert.AreEqual (false, dg.ReadOnly, "ReadOnly property");
			Assert.AreEqual (true, dg.RowHeadersVisible, "RowHeadersVisible property");
			Assert.AreEqual (35, dg.RowHeaderWidth, "RowHeaderWidth property");
			Assert.AreEqual (null, dg.Site, "Site property");
			Assert.AreEqual (string.Empty, dg.Text, "Text property");
			Assert.AreEqual (0, dg.VisibleColumnCount, "VisibleColumnCount property");
		}

		[Test]
		public void TestAllowNavigationChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.AllowNavigationChanged += new EventHandler (OnEventHandler);
			dg.AllowNavigation = !dg.AllowNavigation;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestBackgroundColorChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.BackgroundColorChanged  += new EventHandler (OnEventHandler);
			dg.BackgroundColor = Color.Red;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestBorderStyleChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.BorderStyleChanged  += new EventHandler (OnEventHandler);
			dg.BorderStyle = BorderStyle.None;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestCaptionVisibleChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.CaptionVisibleChanged += new EventHandler (OnEventHandler);
			dg.CaptionVisible = !dg.CaptionVisible;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestFlatModeChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.FlatModeChanged += new EventHandler (OnEventHandler);
			dg.FlatMode = !dg.FlatMode;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestParentRowsLabelStyleChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.ParentRowsLabelStyleChanged  += new EventHandler (OnEventHandler);
			dg.ParentRowsLabelStyle = DataGridParentRowsLabelStyle.None;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestParentRowsVisibleChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.ParentRowsVisibleChanged  += new EventHandler (OnEventHandler);
			dg.ParentRowsVisible = !dg.ParentRowsVisible;
			Assert.AreEqual (true, eventhandled, "A1");
		}
		
		[Test]
		public void TestReadOnlyChangedEvent ()
		{
			DataGrid dg = new DataGrid ();
			eventhandled = false;
			dg.ReadOnlyChanged  += new EventHandler (OnEventHandler);
			dg.ReadOnly = !dg.ReadOnly;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		public void OnEventHandler (object sender, EventArgs e)
	        {
	            	eventhandled = true;
	        }

		// Property exceptions

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GridLineColorException ()
		{
			DataGrid dg = new DataGrid ();
			dg.GridLineColor = Color.Empty;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HeaderBackColorException ()
		{
			DataGrid dg = new DataGrid ();
			dg.HeaderBackColor = Color.Empty;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PreferredColumnWidthException ()
		{
			DataGrid dg = new DataGrid ();
			dg.PreferredColumnWidth = -1;
		}
		
		[Test]
		public void ResetAlternatingBackColor ()
		{
			DataGrid dg = new DataGrid ();
			DataGrid dg2 = new DataGrid ();
			dg2.AlternatingBackColor = Color.Red;
			dg2.ResetAlternatingBackColor ();
			Assert.AreEqual (dg.AlternatingBackColor, dg2.AlternatingBackColor, "A1");
		}
		
		// Test reset colour methods
		[Test]
		public void ResetBackColorMethod ()
		{
			DataGrid dg = new DataGrid ();
			DataGrid dg2 = new DataGrid ();
			dg2.BackColor = Color.Red;
			dg2.ResetBackColor ();
			Assert.AreEqual (dg.BackColor, dg2.BackColor, "A1");
		}

		[Test]
		public void ResetForeColorMethod ()
		{
			DataGrid dg = new DataGrid ();
			DataGrid dg2 = new DataGrid ();
			dg2.ForeColor = Color.Red;
			dg2.ResetForeColor ();
			Assert.AreEqual (dg.ForeColor, dg2.ForeColor, "A1");
		}

		[Test]
		public void ResetGridLineColorMethod ()
		{			
			DataGrid dg = new DataGrid ();
			DataGrid dg2 = new DataGrid ();
			dg2.GridLineColor = Color.Red;
			dg2.ResetGridLineColor ();
			Assert.AreEqual (dg.GridLineColor, dg2.GridLineColor, "A1");
		}

		[Test]
		public void ResetHeaderBackColorMethod ()
		{
			DataGrid dg = new DataGrid ();
			DataGrid dg2 = new DataGrid ();
			dg2.HeaderBackColor = Color.Red;
			dg2.ResetHeaderBackColor ();
			Assert.AreEqual (dg.HeaderBackColor, dg2.HeaderBackColor, "A1");
		}

		[Test]
		public void ResetHeaderFontMethod ()
		{			
		}

		[Test]
		public void ResetHeaderForeColorMethod ()
		{
			DataGrid dg = new DataGrid ();
			DataGrid dg2 = new DataGrid ();
			dg2.HeaderForeColor = Color.Red;
			dg2.ResetHeaderForeColor ();
			Assert.AreEqual (dg.HeaderForeColor, dg2.HeaderForeColor, "A1");			
		}

		[Test]
		public void ResetLinkColorMethod ()
		{						
			DataGrid dg = new DataGrid ();
			DataGrid dg2 = new DataGrid ();
			dg2.LinkColor = Color.Red;
			dg2.ResetLinkColor ();
			Assert.AreEqual (dg.LinkColor, dg2.LinkColor, "A1");
		}

		[Test]
		public void ResetLinkHoverColor ()
		{
			DataGrid dg = new DataGrid ();
			DataGrid dg2 = new DataGrid ();
			dg2.LinkHoverColor = Color.Red;
			dg2.ResetLinkHoverColor ();
			Assert.AreEqual (dg.LinkHoverColor, dg2.LinkHoverColor, "A1");
		}

		[Test]		
		public void ResetSelectionBackColor ()
		{			
			DataGrid dg = new DataGrid ();
			DataGrid dg2 = new DataGrid ();
			dg2.SelectionBackColor = Color.Red;
			dg2.ResetSelectionBackColor ();
			Assert.AreEqual (dg.SelectionBackColor, dg2.SelectionBackColor, "A1");
		}

		[Test]
		public void ResetSelectionForeColor ()
		{
			DataGrid dg = new DataGrid ();
			DataGrid dg2 = new DataGrid ();
			dg2.SelectionForeColor = Color.Red;
			dg2.ResetSelectionForeColor ();
			Assert.AreEqual (dg.SelectionForeColor, dg2.SelectionForeColor, "A1");
		}

	}
}
