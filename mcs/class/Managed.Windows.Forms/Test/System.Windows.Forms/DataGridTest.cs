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
using System.Data;
using System.Xml;

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
	public class DataGridTest : TestHelper
	{
		private bool eventhandled;


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

			// Font
			Assert.IsFalse (dg.Font.Bold, "Font Bold");
#if NET_2_0
			Assert.IsTrue (dg.Font.IsSystemFont, "Font IsSystemFont");
#endif
			Assert.IsFalse (dg.Font.Italic, "Font Italic");
			Assert.IsFalse (dg.Font.Strikeout, "Font Strikeout");
			Assert.IsFalse (dg.Font.Underline, "Font Underline");
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
		public void CaptionFont ()
		{
			DataGrid dg = new DataGrid ();

			// default values
			Assert.IsTrue (dg.CaptionFont.Bold, "#A1");
			Assert.AreEqual (dg.CaptionFont.FontFamily, dg.Font.FontFamily, "#A2");
			Assert.AreEqual (dg.CaptionFont.Height, dg.Font.Height, "#A3");
#if NET_2_0
			Assert.IsFalse(dg.CaptionFont.IsSystemFont, "#A4");
#endif
			Assert.AreEqual (dg.CaptionFont.Italic, dg.Font.Italic, "#A5");
			Assert.AreEqual (dg.CaptionFont.Name, dg.Font.Name, "#A6");
			Assert.AreEqual (dg.CaptionFont.Size, dg.Font.Size, "#A7");
			Assert.AreEqual (dg.CaptionFont.SizeInPoints, dg.Font.SizeInPoints, "#A8");
			Assert.AreEqual (dg.CaptionFont.Strikeout, dg.Font.Strikeout, "#A9");
			Assert.AreEqual (dg.CaptionFont.Underline, dg.Font.Underline, "#A10");
			Assert.AreEqual (dg.CaptionFont.Unit, dg.Font.Unit, "#A11");

			// modifying Font affects CaptionFont, except for FontStyle
			dg.Font = new Font (dg.Font.FontFamily, 3, FontStyle.Italic);
			Assert.IsTrue (dg.CaptionFont.Bold, "#B1");
			Assert.IsFalse (dg.Font.Bold, "#B2");
			Assert.IsFalse (dg.CaptionFont.Italic, "#B3");
			Assert.IsTrue (dg.Font.Italic, "#B4");
			Assert.AreEqual (3, dg.Font.SizeInPoints, "#B5");
			Assert.AreEqual (dg.CaptionFont.SizeInPoints, dg.Font.SizeInPoints, "#B6");

			// explicitly setting CaptionFont removes link between CaptionFont
			// and Font
			dg.CaptionFont = dg.Font;
			Assert.AreSame (dg.CaptionFont, dg.Font, "#C1");
			dg.Font = new Font (dg.Font.FontFamily, 7, FontStyle.Bold);
			Assert.IsFalse (dg.CaptionFont.Bold, "#C2");
			Assert.IsTrue (dg.Font.Bold, "#C3");
			Assert.AreEqual (7, dg.Font.SizeInPoints, "#C4");
			Assert.AreEqual (3, dg.CaptionFont.SizeInPoints, "#C5");
		}

		[Test]
		public void HeaderFont ()
		{
			DataGrid dg = new DataGrid ();
			dg.Font = new Font (dg.Font, FontStyle.Italic);
			Assert.AreSame (dg.HeaderFont, dg.Font, "#1");

			dg.HeaderFont = dg.Font;
			Assert.AreSame (dg.HeaderFont, dg.Font, "#2");

			dg.Font = new Font (dg.Font, FontStyle.Regular);
			Assert.IsTrue (dg.HeaderFont.Italic, "#3");
			Assert.IsFalse (dg.Font.Italic, "#4");

			dg.ResetHeaderFont ();
			Assert.AreSame (dg.HeaderFont, dg.Font, "#5");
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

		[Test]
		public void TestSetDataBinding ()
		{
			DataGrid dg = new DataGrid ();
			DataSet ds = new DataSet ("DataSet");
			DataTable dt = new DataTable ("DataTable");
			ds.Tables.Add (dt);

			dg.SetDataBinding (ds, "DataTable");
		}

		int data_source_changed_count = 0;
		void OnDataSourceChanged (object sender, EventArgs e)
		{
			data_source_changed_count ++;
		}

		[Test]
		public void TestManager1 ()
		{
			TestDataGrid dg = new TestDataGrid ();

			data_source_changed_count = 0;
			dg.DataSourceChanged += new EventHandler (OnDataSourceChanged);

			/* make sure everything is fine to start with */
			Assert.IsNull (dg.Manager, "A1");
			Assert.IsNull (dg.DataSource, "A2");
			Assert.AreEqual (dg.DataMember, "", "A3");
			// NotWorking Assert.AreEqual (0, data_source_changed_count, "A4");
		}

		[Test]
		public void TestManagerSetDataMember ()
		{
			TestDataGrid dg = new TestDataGrid ();

			data_source_changed_count = 0;
			dg.DataSourceChanged += new EventHandler (OnDataSourceChanged);

			/* set the datamember to something */
			dg.DataMember = "hi there";
			Assert.IsNull (dg.Manager, "A1");
			// NotWorking Assert.AreEqual (0, data_source_changed_count, "A2");
		}

		[Test]
		public void TestManagerSetDataSource ()
		{
			TestDataGrid dg = new TestDataGrid ();

			data_source_changed_count = 0;
			dg.DataSourceChanged += new EventHandler (OnDataSourceChanged);

			/* set our datasource to something */
			dg = new TestDataGrid ();
			DataSet ds = new DataSet ("DataSet");
			DataTable dt = new DataTable ("DataTable");
			ds.Tables.Add (dt);

			dg.DataSource = ds;
			Assert.IsNull (dg.Manager, "A1");

			/* set the datamember to something as well.. anything yet? */
			dg.DataMember = "DataTable";
			Assert.IsNull (dg.Manager, "A2");
			Assert.AreEqual (0, data_source_changed_count, "A3");
		}

		[Test]
		public void TestManagerCreateHandle ()
		{
			TestDataGrid dg = new TestDataGrid ();

			data_source_changed_count = 0;
			dg.DataSourceChanged += new EventHandler (OnDataSourceChanged);

			/* set our datasource to something */
			dg = new TestDataGrid ();
			DataSet ds = new DataSet ("DataSet");
			DataTable dt = new DataTable ("DataTable");
			ds.Tables.Add (dt);

			dg.DataSource = ds;

			/* cause the control to create its handle and
			 * see if that does anything */
			Assert.IsNotNull (dg.Handle, "A1");
			Assert.IsNull (dg.Manager, "A2");
			Assert.AreEqual (0, data_source_changed_count, "A3");
		}

		[Test]
		public void TestManagerSetBindingContext ()
		{
			TestDataGrid dg = new TestDataGrid ();

			data_source_changed_count = 0;
			dg.DataSourceChanged += new EventHandler (OnDataSourceChanged);

			/* set our datasource to something */
			dg = new TestDataGrid ();
			DataSet ds = new DataSet ("DataSet");
			DataTable dt = new DataTable ("DataTable");
			ds.Tables.Add (dt);

			dg.DataSource = ds;
			dg.DataMember = "DataTable";

			/* now set the BindingContext and see if something changes */
			dg.BindingContext = new BindingContext ();
			Assert.IsNotNull (dg.Manager, "A1");
			Assert.AreEqual (0, data_source_changed_count, "A2");
		}

		[Test]
		public void TestManagerAfterSetBindingContext ()
		{
			TestDataGrid dg = new TestDataGrid ();

			data_source_changed_count = 0;
			dg.DataSourceChanged += new EventHandler (OnDataSourceChanged);

			dg.BindingContext = new BindingContext ();

			/* set our datasource to something */
			dg = new TestDataGrid ();
			DataSet ds = new DataSet ("DataSet");
			DataTable dt = new DataTable ("DataTable");
			ds.Tables.Add (dt);

			dg.DataSource = ds;
			Assert.IsNull (dg.Manager, "A1");

			dg.DataMember = "DataTable";
			Assert.IsNull (dg.Manager, "A2");

			dg.BindingContext = new BindingContext ();
			Assert.IsNotNull (dg.Manager, "A3");
			// NotWorking Assert.AreEqual (0, data_source_changed_count, "A4");
		}

		[Test]
		public void TestManagerSetDataMemberAfterSetBindingContext ()
		{
			TestDataGrid dg = new TestDataGrid ();

			data_source_changed_count = 0;
			dg.DataSourceChanged += new EventHandler (OnDataSourceChanged);

			/* set our datasource to something */
			dg = new TestDataGrid ();
			DataSet ds = new DataSet ("DataSet");
			DataTable dt = new DataTable ("DataTable");
			ds.Tables.Add (dt);

			dg.DataSource = ds;

			dg.BindingContext = new BindingContext ();
			Assert.AreEqual (0, data_source_changed_count, "A1");

			CurrencyManager mgr = dg.Manager;

			dg.DataMember = "DataTable";
			Assert.IsNotNull (dg.Manager, "A2");
			Assert.IsTrue (mgr != dg.Manager, "A3");
			Assert.AreEqual (0, data_source_changed_count, "A4");
		}

		[Test]
		public void TestManagerSetDataSourceAfterSetBindingContext ()
		{
			TestDataGrid dg = new TestDataGrid ();

			data_source_changed_count = 0;
			dg.DataSourceChanged += new EventHandler (OnDataSourceChanged);

			/* set our datasource to something */
			dg = new TestDataGrid ();
			DataSet ds = new DataSet ("DataSet");
			DataTable dt = new DataTable ("DataTable");
			ds.Tables.Add (dt);

			dg.DataMember = "DataTable";

			dg.BindingContext = new BindingContext ();
			Assert.AreEqual (0, data_source_changed_count, "A1");

			CurrencyManager mgr = dg.Manager;

			dg.DataSource = ds;
			Assert.IsNotNull (dg.Manager, "A2");
			Assert.IsTrue (mgr != dg.Manager, "A3");
			Assert.AreEqual (0, data_source_changed_count, "A4");
		}

		[Test]
		public void TestManagerSetDataSourceAfterSetBindingContextWithHandle ()
		{
			TestDataGrid dg = new TestDataGrid ();

			data_source_changed_count = 0;
			dg.DataSourceChanged += new EventHandler (OnDataSourceChanged);

			/* set our datasource to something */
			dg = new TestDataGrid ();
			DataSet ds = new DataSet ("DataSet");
			DataTable dt = new DataTable ("DataTable");
			ds.Tables.Add (dt);

			/* cause the control to create its handle and
			 * see if that does anything */
			Assert.IsNotNull (dg.Handle, "A1");

			dg.DataSource = new ArrayList ();

			dg.BindingContext = new BindingContext ();
			Assert.AreEqual (0, data_source_changed_count, "A2");

			dg.DataSource = ds;
			Assert.AreEqual (0, data_source_changed_count, "A3");
		}

		[Test]
		public void TestManagerSetDataSourceWithEmptyStyle ()
		{
			TestDataGrid dg = new TestDataGrid ();
			dg.BindingContext = new BindingContext ();

			DataSet ds = new DataSet ("DataSet");
			DataTable dt = new DataTable ("MyTable");
			dt.Columns.Add ("A", typeof (string));
			dt.NewRow ();
			ds.Tables.Add (dt);

			// Add the style for the table we have, but leave it empty
			// - this is, no column styles
			DataGridTableStyle table_style = new DataGridTableStyle ();
			table_style.MappingName = "MyTable";
			dg.TableStyles.Add (table_style);

			Assert.AreEqual (0, table_style.GridColumnStyles.Count, "#A1");

			dg.DataSource = dt;

			Assert.AreEqual (1, table_style.GridColumnStyles.Count, "#B1");
		}

		public class ClickableDataGrid : DataGrid
		{
			public void ClickGrid (int X, int Y)
			{
				MouseEventArgs me = new MouseEventArgs (
					MouseButtons.Left,
					1, /*# of clicks*/
					X, Y, 0);
				OnMouseDown (me);
				OnClick (me);
				OnMouseUp (me);
			}
		}

		public class Form5487 : Form
		{
			private ClickableDataGrid dataGrid1;
			private Container components = null;

			public Form5487 ()
			{
				InitializeComponent ();
			}

			protected override void Dispose (bool disposing)
			{
				if (disposing) {
					if (components != null) {
						components.Dispose ();
					}
				}
				base.Dispose (disposing);
			}

			private void InitializeComponent ()
			{
				this.dataGrid1 = new ClickableDataGrid ();
				((ISupportInitialize)(this.dataGrid1)).BeginInit ();
				this.SuspendLayout ();
				this.dataGrid1.DataMember = "";
				this.dataGrid1.HeaderForeColor = SystemColors.ControlText;
				this.dataGrid1.Location = new Point (16, 16);
				this.dataGrid1.Name = "dataGrid1";
				this.dataGrid1.Size = new Size (624, 440);
				this.dataGrid1.TabIndex = 0;
				this.AutoScaleBaseSize = new Size (5, 13);
				this.ClientSize = new Size (656, 470);
				this.Controls.Add (this.dataGrid1);
				this.Name = "Form1";
				this.Text = "Form1";
				this.Shown += new EventHandler (this.Form1_Load);
				((ISupportInitialize)(this.dataGrid1)).EndInit ();
				this.ResumeLayout (false);

			}

			private void Form1_Load (object sender, EventArgs e)
			{
				DataSet ds = new DataSet ();
				String XMLString = "";
				XmlTextReader XMLTR;

				XMLString += "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>";
				XMLString += "<pa><pb><pc><pd><id>1</id>";
				XMLString += "</pd></pc><pc><pd><id>1</id>";
				XMLString += "</pd><pd><id>1</id></pd><pd>";
				XMLString += "<id>1</id></pd><pd><id>1</id>";
				XMLString += "</pd><pd><id>1</id></pd><pd>";
				XMLString += "<id>1</id></pd><pd><id>1</id>";
				XMLString += "</pd><pd><id>1</id></pd></pc>";
				XMLString += "</pb></pa>";
				XMLTR = new XmlTextReader (XMLString,
					XmlNodeType.Document, null);
				XMLTR.ReadOuterXml ();
				ds.ReadXml (XMLTR);
				this.dataGrid1.DataSource = ds;
				this.dataGrid1.ClickGrid (25, 45);
				Application.DoEvents ();
				this.dataGrid1.ClickGrid (46, 73);
				Application.DoEvents ();
				this.dataGrid1.NavigateBack ();
				Close ();
			}
		}
		[Test]
		public void Bug5487AndRelated ()
		{
			//this should crash on fail
			Application.Run (new Form5487 ());
		}
	}
}
