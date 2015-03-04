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
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using NUnit.Framework;

// resolve the ambiguity between System.ComponentModel and NUnit.Framework
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class DataGridTextBoxColumnTest : TestHelper
	{
		private bool eventhandled;
		//private object Element;
		//private CollectionChangeAction Action;

		[Test]
		public void TestDefaultValues ()
		{
			DataGridTextBoxColumn col = new DataGridTextBoxColumn ();

			Assert.AreEqual (HorizontalAlignment.Left, col.Alignment, "HorizontalAlignment property");
			Assert.AreEqual ("", col.HeaderText, "HeaderText property");
			Assert.AreEqual ("", col.MappingName, "MappingName property");
			Assert.AreEqual ("(null)", col.NullText, "NullText property");
			Assert.AreEqual (false, col.ReadOnly, "ReadOnly property");
			Assert.AreEqual (-1, col.Width, "Width property");
			Assert.AreEqual ("", col.Format, "Format property");
			Assert.AreEqual (null, col.FormatInfo, "FormatInfo property");
		}

		[Test]
		public void TestMappingNameChangedEvent ()
		{
			DataGridTextBoxColumn col = new DataGridTextBoxColumn ();
			eventhandled = false;
			col.MappingNameChanged += new EventHandler (OnEventHandler);
			col.MappingName = "name1";
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestAlignmentChangedEvent ()
		{
			DataGridTextBoxColumn col = new DataGridTextBoxColumn ();
			eventhandled = false;
			col.AlignmentChanged += new EventHandler (OnEventHandler);
			col.Alignment = HorizontalAlignment.Center;
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestHeaderTextChangedEvent ()
		{
			DataGridTextBoxColumn col = new DataGridTextBoxColumn ();
			eventhandled = false;
			col.HeaderTextChanged += new EventHandler (OnEventHandler);
			col.HeaderText = "Header";
			Assert.AreEqual (true, eventhandled, "A1");
		}

		[Test]
		public void TestNullTextChangedEvent ()
		{
			DataGridTextBoxColumn col = new DataGridTextBoxColumn ();
			eventhandled = false;
			col.NullTextChanged += new EventHandler (OnEventHandler);
			col.NullText = "Null";
			Assert.AreEqual (true, eventhandled, "A1");
		}

		private void OnEventHandler (object sender, EventArgs e)
		{
			eventhandled = true;
		}

		DataTable table;
		DataView view;
		DataGridTableStyle tableStyle;
		ColumnPoker nameColumnStyle;
		ColumnPoker companyColumnStyle;

		public void MakeTable (bool readonly_name)
		{
			table = new DataTable ();
			view = table.DefaultView;
			table.Columns.Add (new DataColumn ("who"));
			table.Columns.Add (new DataColumn ("where"));
			DataRow row = table.NewRow ();
			row ["who"] = "Miguel";
			row ["where"] = null;
			table.Rows.Add (row);

			row = table.NewRow ();
			row ["who"] = "Toshok";
			row ["where"] = "Novell";
			table.Rows.Add (row);

			tableStyle = new DataGridTableStyle ();
			nameColumnStyle = new ColumnPoker ();
			nameColumnStyle.MappingName = "who";
			nameColumnStyle.ReadOnly = readonly_name;
			tableStyle.GridColumnStyles.Add (nameColumnStyle);
			companyColumnStyle = new ColumnPoker ();
			companyColumnStyle.HeaderText = "Company";
			companyColumnStyle.MappingName = "where";
			companyColumnStyle.NullText = "(not set)";
			tableStyle.GridColumnStyles.Add (companyColumnStyle);
		}

		class ColumnPoker : DataGridTextBoxColumn
		{
			public ColumnPoker ()
			{
			}

			public ColumnPoker (PropertyDescriptor prop) : base (prop)
			{
			}

			public void DoAbort (int rowNum)
			{
				base.Abort (rowNum);
			}

			public bool DoCommit (CurrencyManager dataSource, int rowNum)
			{
				return base.Commit (dataSource, rowNum);
			}

			public void DoConcedeFocus ()
			{
				base.ConcedeFocus ();
			}

			public void DoEdit (CurrencyManager source, int rowNum,  Rectangle bounds,  bool _ro, string instantText, bool cellIsVisible)
			{
				base.Edit (source, rowNum, bounds, _ro, instantText, cellIsVisible);
			}

			public void DoEndEdit ()
			{
				base.EndEdit ();
			}

			public void DoEnterNullValue ()
			{
				base.EnterNullValue ();
			}

			public void DoHideEditBox ()
			{
				base.HideEditBox ();
			}

			public void DoReleaseHostedControl ()
			{
				base.ReleaseHostedControl ();
			}

			public void DoSetDataGridInColumn (DataGrid value)
			{
				base.SetDataGridInColumn (value);
			}

			public void DoUpdateUI (CurrencyManager source, int rowNum, string instantText)
			{
				base.UpdateUI (source, rowNum, instantText);
			}
		}

		[Test]
		public void TestDoEdit ()
		{
			MakeTable (true);

			BindingContext bc = new BindingContext ();
			DataGrid dg = new DataGrid ();
			dg.BindingContext = bc;
			dg.TableStyles.Add (tableStyle);
			dg.DataSource = table;

			CurrencyManager cm = (CurrencyManager)bc[view];
			ColumnPoker column = nameColumnStyle;
			TextBox tb = nameColumnStyle.TextBox;

			Assert.IsNotNull (tb, "1");
			Assert.AreEqual (typeof (DataGridTextBox), tb.GetType(), "2");
			Assert.IsTrue (tb.Enabled, "3");
			Assert.IsFalse (tb.Visible, "4");
			Assert.AreEqual ("", tb.Text, "5");
			Assert.IsFalse (tb.ReadOnly, "6");

			column.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "hi there", true);
			Assert.IsTrue (tb.ReadOnly, "7");

			// since it's readonly
			Assert.AreEqual ("Miguel", tb.Text, "8");
		}

		[Test]
		public void TestDoEdit_NullInstantTest ()
		{
			MakeTable (false);

			BindingContext bc = new BindingContext ();
			DataGrid dg = new DataGrid ();
			dg.BindingContext = bc;
			dg.TableStyles.Add (tableStyle);
			dg.DataSource = table;

			CurrencyManager cm = (CurrencyManager)bc[view];
			ColumnPoker column = nameColumnStyle;
			TextBox tb = nameColumnStyle.TextBox;

			Assert.IsNotNull (tb, "1");
			Assert.AreEqual (typeof (DataGridTextBox), tb.GetType(), "2");
			Assert.IsTrue (tb.Enabled, "3");
			Assert.IsFalse (tb.Visible, "4");
			Assert.AreEqual ("", tb.Text, "5");
			Assert.IsFalse (tb.ReadOnly, "6");

			column.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, null, true);
			Assert.IsFalse (tb.ReadOnly, "7");

			// since it's readonly
			Assert.AreEqual ("Miguel", tb.Text, "8");
		}

		[Test]
		public void TestEndEdit ()
		{
			MakeTable (false);

			BindingContext bc = new BindingContext ();
			DataGrid dg = new DataGrid ();
			dg.BindingContext = bc;
			dg.TableStyles.Add (tableStyle);
			dg.DataSource = table;

			CurrencyManager cm = (CurrencyManager)bc[view];
			ColumnPoker column = nameColumnStyle;
			TextBox tb = column.TextBox;

			Assert.AreEqual ("", tb.Text, "1");
			column.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "hi there", true);
			Assert.AreEqual ("hi there", tb.Text, "2");

			tb.Text = "yo";

			column.DoEndEdit ();

			DataRowView v = (DataRowView)cm.Current;

			Assert.AreEqual ("Miguel", v[0], "3");
		}

		[Test]
		public void TestCommit ()
		{
			MakeTable (false);

			BindingContext bc = new BindingContext ();
			DataGrid dg = new DataGrid ();
			dg.BindingContext = bc;
			dg.TableStyles.Add (tableStyle);
			dg.DataSource = table;

			CurrencyManager cm = (CurrencyManager)bc[view];
			ColumnPoker column = nameColumnStyle;
			DataGridTextBox tb = (DataGridTextBox)column.TextBox;

			Assert.AreEqual ("", tb.Text, "1");
			Assert.IsTrue (tb.IsInEditOrNavigateMode, "1.5");
			column.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "hi there", true);
			Assert.AreEqual ("hi there", tb.Text, "2");
			Assert.AreEqual (new Rectangle (new Point (2,2), new Size (97,97)), tb.Bounds, "3");
			Assert.IsFalse (tb.ReadOnly, "4");
			Assert.IsFalse (tb.IsInEditOrNavigateMode, "5");

			bool rv;
			rv = column.DoCommit (cm, cm.Position);
			column.DoEndEdit ();

			Assert.IsTrue (tb.IsInEditOrNavigateMode, "6");
			Assert.IsTrue (rv, "7");
			DataRowView v = (DataRowView)cm.Current;
			Assert.AreEqual ("hi there", v[0], "8");
		}

		[Test]
		public void TestCommit2 ()
		{
			MakeTable (false);

			BindingContext bc = new BindingContext ();
			DataGrid dg = new DataGrid ();
			dg.BindingContext = bc;
			dg.TableStyles.Add (tableStyle);
			dg.DataSource = table;

			CurrencyManager cm = (CurrencyManager)bc[view];
			ColumnPoker column = nameColumnStyle;
			DataGridTextBox tb = (DataGridTextBox)column.TextBox;

			Assert.AreEqual ("", tb.Text, "1");
			Assert.IsTrue (tb.IsInEditOrNavigateMode, "1.5");
			column.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "hi there", true);
			Assert.AreEqual ("hi there", tb.Text, "2");
			Assert.AreEqual (new Rectangle (new Point (2,2), new Size (97,97)), tb.Bounds, "3");
			Assert.IsFalse (tb.ReadOnly, "4");
			Assert.IsFalse (tb.IsInEditOrNavigateMode, "5");

			tb.Text = "yo";

			column.DoEndEdit ();

			Assert.IsTrue (tb.IsInEditOrNavigateMode, "5.5");

			bool rv = column.DoCommit (cm, cm.Position);
			Assert.IsTrue (rv, "6");
			DataRowView v = (DataRowView)cm.Current;
			Assert.AreEqual ("Miguel", v[0], "7");

			/* try it again with the DoCommit before the DoEndEdit */
			cm.Position = 0;
			column.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100,100)), false, "hi there", true);
			Assert.AreEqual ("hi there", tb.Text, "8");
			Assert.AreEqual (new Rectangle (new Point (2,2), new Size (97,97)), tb.Bounds, "9");
			Assert.IsFalse (tb.ReadOnly, "10");
			Assert.IsFalse (tb.IsInEditOrNavigateMode, "11");
			tb.Text = "yo";

			rv = column.DoCommit (cm, cm.Position);
			column.DoEndEdit ();
			Assert.IsTrue (rv, "12");
			v = (DataRowView)cm.Current;
			Assert.AreEqual ("yo", v[0], "13");
		}

		[Test]
		public void TestAbort ()
		{
			MakeTable (false);

			BindingContext bc = new BindingContext ();
			DataGrid dg = new DataGrid ();
			dg.BindingContext = bc;
			dg.TableStyles.Add (tableStyle);
			dg.DataSource = table;

			CurrencyManager cm = (CurrencyManager)bc[view];
			ColumnPoker column = nameColumnStyle;
			DataGridTextBox tb = (DataGridTextBox)column.TextBox;

			Assert.AreEqual ("", tb.Text, "1");
			Assert.IsTrue (tb.IsInEditOrNavigateMode, "1.5");
			column.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "hi there", true);
			Assert.AreEqual ("hi there", tb.Text, "2");
			Assert.AreEqual (new Rectangle (new Point (2,2), new Size (97,97)), tb.Bounds, "3");
			Assert.IsFalse (tb.ReadOnly, "4");
			Assert.IsFalse (tb.IsInEditOrNavigateMode, "5");

			tb.Text = "yo";

			column.DoAbort (0);

			Assert.IsTrue (tb.IsInEditOrNavigateMode, "6");
			DataRowView v = (DataRowView)cm.Current;
			Assert.AreEqual ("Miguel", v[0], "7");
		}

		[Test]
		public void TestAbort_DifferentRow ()
		{
			MakeTable (false);

			BindingContext bc = new BindingContext ();
			DataGrid dg = new DataGrid ();
			dg.BindingContext = bc;
			dg.TableStyles.Add (tableStyle);
			dg.DataSource = table;

			CurrencyManager cm = (CurrencyManager)bc[view];
			ColumnPoker column = nameColumnStyle;
			DataGridTextBox tb = (DataGridTextBox)column.TextBox;

			Assert.AreEqual ("", tb.Text, "1");
			Assert.IsTrue (tb.IsInEditOrNavigateMode, "1.5");
			column.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "hi there", true);
			Assert.AreEqual ("hi there", tb.Text, "2");
			Assert.AreEqual (new Rectangle (new Point (2,2), new Size (97,97)), tb.Bounds, "3");
			Assert.IsFalse (tb.ReadOnly, "4");
			Assert.IsFalse (tb.IsInEditOrNavigateMode, "5");

			tb.Text = "yo";

			column.DoAbort (1);

			Assert.IsTrue (tb.IsInEditOrNavigateMode, "6");
			DataRowView v = (DataRowView)cm.Current;
			Assert.AreEqual ("Miguel", v[0], "7");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestUpdateUI ()
		{
			MakeTable (false);

			BindingContext bc = new BindingContext ();
			DataGrid dg = new DataGrid ();
			dg.BindingContext = bc;
			dg.TableStyles.Add (tableStyle);
			dg.DataSource = table;

			CurrencyManager cm = (CurrencyManager)bc[view];
			ColumnPoker column = nameColumnStyle;
			DataGridTextBox tb = (DataGridTextBox)column.TextBox;

			Assert.AreEqual ("", tb.Text, "1");
			Assert.IsTrue (tb.IsInEditOrNavigateMode, "2");

			Assert.AreEqual (Point.Empty, tb.Location, "3");

			column.DoUpdateUI (cm, 0, "hi there");

			Assert.AreEqual (Point.Empty, tb.Location, "4");

			Assert.AreEqual ("hi there", tb.Text, "5");
			Assert.IsFalse (tb.ReadOnly, "6");
			Assert.IsTrue (tb.IsInEditOrNavigateMode, "7");

			DataRowView v = (DataRowView)cm.Current;
			Assert.AreEqual ("Miguel", v[0], "8");
		}

		[Test]
		public void TestReadOnly_InEditCall ()
		{
			MakeTable (false);

			BindingContext bc = new BindingContext ();
			DataGrid dg = new DataGrid ();
			dg.BindingContext = bc;
			dg.TableStyles.Add (tableStyle);
			dg.DataSource = table;

			CurrencyManager cm = (CurrencyManager)bc[view];
			ColumnPoker column = nameColumnStyle;
			TextBox tb = nameColumnStyle.TextBox;

			Assert.IsNotNull (tb, "1");
			Assert.AreEqual (typeof (DataGridTextBox), tb.GetType(), "2");
			Assert.IsTrue (tb.Enabled, "3");
			Assert.IsFalse (tb.Visible, "4");
			Assert.AreEqual ("", tb.Text, "5");
			Assert.IsFalse (tb.ReadOnly, "6");

			column.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), true, "hi there", true);

			Assert.IsTrue (tb.ReadOnly, "7");

			// since it's readonly
			Assert.AreEqual ("Miguel", tb.Text, "8");
		}

		[Test]
		public void TestReadOnly_AfterEditCall ()
		{
			MakeTable (false);

			BindingContext bc = new BindingContext ();
			DataGrid dg = new DataGrid ();
			dg.BindingContext = bc;
			dg.TableStyles.Add (tableStyle);
			dg.DataSource = table;

			CurrencyManager cm = (CurrencyManager)bc[view];
			ColumnPoker column = nameColumnStyle;
			TextBox tb = nameColumnStyle.TextBox;

			Assert.IsNotNull (tb, "1");
			Assert.AreEqual (typeof (DataGridTextBox), tb.GetType(), "2");
			Assert.IsTrue (tb.Enabled, "3");
			Assert.IsFalse (tb.Visible, "4");
			Assert.AreEqual ("", tb.Text, "5");
			Assert.IsFalse (tb.ReadOnly, "6");

			column.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "hi there", true);
			column.ReadOnly = true;

			Assert.IsFalse (tb.ReadOnly, "7");

			Assert.AreEqual ("hi there", tb.Text, "8");

			bool rv;

			rv = column.DoCommit (cm, cm.Position);
			column.DoEndEdit ();
			Assert.IsTrue (rv, "9");
			DataRowView v = (DataRowView)cm.Current;
			Assert.AreEqual ("hi there", v[0], "10");
		}

		[Test]
		public void TestReadOnly_DataGrid ()
		{
			MakeTable (false);

			BindingContext bc = new BindingContext ();
			DataGrid dg = new DataGrid ();
			dg.BindingContext = bc;
			dg.TableStyles.Add (tableStyle);
			dg.DataSource = table;
			dg.ReadOnly = true;

			CurrencyManager cm = (CurrencyManager)bc[view];
			ColumnPoker column = nameColumnStyle;
			TextBox tb = nameColumnStyle.TextBox;

			Assert.IsNotNull (tb, "1");
			Assert.AreEqual (typeof (DataGridTextBox), tb.GetType(), "2");
			Assert.IsTrue (tb.Enabled, "3");
			Assert.IsFalse (tb.Visible, "4");
			Assert.AreEqual ("", tb.Text, "5");
			Assert.IsFalse (tb.ReadOnly, "6");

			column.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "hi there", true);

			Assert.IsFalse (tb.ReadOnly, "7");

			Assert.AreEqual ("hi there", tb.Text, "8");

			bool rv;

			rv = column.DoCommit (cm, cm.Position);
			column.DoEndEdit ();
			Assert.IsTrue (rv, "9");
			DataRowView v = (DataRowView)cm.Current;
			Assert.AreEqual ("hi there", v[0], "10");
		}

		[Test]
		public void TestReadOnly_TableStyle ()
		{
			MakeTable (false);

			BindingContext bc = new BindingContext ();
			DataGrid dg = new DataGrid ();
			dg.BindingContext = bc;
			dg.TableStyles.Add (tableStyle);
			dg.DataSource = table;

			tableStyle.ReadOnly = true;

			CurrencyManager cm = (CurrencyManager)bc[view];
			ColumnPoker column = nameColumnStyle;
			TextBox tb = nameColumnStyle.TextBox;

			Assert.IsNotNull (tb, "1");
			Assert.AreEqual (typeof (DataGridTextBox), tb.GetType(), "2");
			Assert.IsTrue (tb.Enabled, "3");
			Assert.IsFalse (tb.Visible, "4");
			Assert.AreEqual ("", tb.Text, "5");
			Assert.IsFalse (tb.ReadOnly, "6");

			column.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "hi there", true);

			Assert.IsTrue (tb.ReadOnly, "7");

			Assert.AreEqual ("Miguel", tb.Text, "8");
		}

		[Test]
		public void IFormattable ()
		{
			CultureInfo originalCulture = CultureInfo.CurrentCulture;

			try {
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-GB");

				table = new DataTable ();
				view = table.DefaultView;
				table.Columns.Add (new DataColumn ("Amount", typeof (MockNumericFormattable)));

				DataRow row = table.NewRow ();
				row ["Amount"] = new MockNumericFormattable (1);
				table.Rows.Add (row);

				row = table.NewRow ();
				row ["Amount"] = new MockNumericFormattable (2);
				table.Rows.Add (row);

				row = table.NewRow ();
				row ["Amount"] = new MockNumeric (3);
				table.Rows.Add (row);

				tableStyle = new DataGridTableStyle ();
				ColumnPoker amountColumnStyle = new ColumnPoker ();
				amountColumnStyle.MappingName = "Amount";
				tableStyle.GridColumnStyles.Add (amountColumnStyle);

				BindingContext bc = new BindingContext ();
				DataGrid dg = new DataGrid ();
				dg.BindingContext = bc;
				dg.TableStyles.Add (tableStyle);
				dg.DataSource = table;

				CurrencyManager cm = (CurrencyManager) bc [view];
				TextBox tb = amountColumnStyle.TextBox;

				Assert.IsNotNull (tb, "#A1");
				Assert.AreEqual (string.Empty, tb.Text, "#A2");

				amountColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, null, true);
				Assert.AreEqual ("uno", tb.Text, "#B1");
				Assert.AreEqual (new MockNumericFormattable (1), table.Rows [0] ["Amount"], "#B2");

				amountColumnStyle.DoEdit (cm, 1, new Rectangle (new Point (0,0), new Size (100, 100)), false, null, true);
				Assert.AreEqual ("dos", tb.Text, "#C1");
				Assert.AreEqual (new MockNumericFormattable (2), table.Rows [1] ["Amount"], "#C2");

				amountColumnStyle.DoEdit (cm, 2, new Rectangle (new Point (0,0), new Size (100, 100)), false, null, true);
				Assert.AreEqual ("tres", tb.Text, "#D1");
				Assert.AreEqual (new MockNumeric (3), table.Rows [2] ["Amount"], "#D2");

				amountColumnStyle.Format = string.Empty;

				amountColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, null, true);
				Assert.AreEqual ("uno", tb.Text, "#E1");
				Assert.AreEqual (new MockNumericFormattable (1), table.Rows [0] ["Amount"], "#E2");

				amountColumnStyle.Format = "currency";

				amountColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, null, true);
				Assert.AreEqual ("#£1.00", tb.Text, "#F1");
				Assert.AreEqual (new MockNumericFormattable (1), table.Rows [0] ["Amount"], "#F2");

				amountColumnStyle.DoEdit (cm, 2, new Rectangle (new Point (0,0), new Size (100, 100)), false, null, true);
				Assert.AreEqual ("tres", tb.Text, "#G1");
				Assert.AreEqual (new MockNumeric (3), table.Rows [2] ["Amount"], "#G2");

				amountColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "#£5.00", true);
				Assert.AreEqual ("#£5.00", tb.Text, "#H1");
				Assert.AreEqual (new MockNumericFormattable (1), table.Rows [0] ["Amount"], "#H2");

				amountColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "INVALID", true);
				Assert.IsTrue (amountColumnStyle.DoCommit (cm, 0), "#I1");
				Assert.AreEqual ("INVALID", tb.Text, "#I2");
				Assert.AreEqual ("INVALID", table.Rows [0] ["Amount"], "#I3");

				amountColumnStyle.FormatInfo = new CultureInfo ("en-US");

				amountColumnStyle.DoEdit (cm, 1, new Rectangle (new Point (0,0), new Size (100, 100)), false, null, true);
				Assert.AreEqual ("#$2.00", tb.Text, "#J1");
				Assert.AreEqual (new MockNumericFormattable (2), table.Rows [1] ["Amount"], "#J2");
			} finally {
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void IFormattable_DateTime ()
		{
			CultureInfo originalCulture = CultureInfo.CurrentCulture;

			try {
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("nl-BE");
				DateTime today = DateTime.Today;
				DateTime now = DateTime.Now;

				table = new DataTable ();
				view = table.DefaultView;
				table.Columns.Add (new DataColumn ("Date", typeof (DateTime)));

				DataRow row = table.NewRow ();
				row ["Date"] = today;
				table.Rows.Add (row);

				row = table.NewRow ();
				row ["Date"] = now;
				table.Rows.Add (row);

				tableStyle = new DataGridTableStyle ();
				ColumnPoker dateColumnStyle = new ColumnPoker ();
				dateColumnStyle.MappingName = "Date";
				tableStyle.GridColumnStyles.Add (dateColumnStyle);

				BindingContext bc = new BindingContext ();
				DataGrid dg = new DataGrid ();
				dg.BindingContext = bc;
				dg.TableStyles.Add (tableStyle);
				dg.DataSource = table;

				CurrencyManager cm = (CurrencyManager) bc [view];
				TextBox tb = dateColumnStyle.TextBox;
				DateTimeConverter converter = new DateTimeConverter ();

				Assert.IsNotNull (tb, "#A1");
				Assert.AreEqual (string.Empty, tb.Text, "#A2");

				dateColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, null, true);
				Assert.AreEqual (converter.ConvertTo (null, CultureInfo.CurrentCulture,
					today, typeof (string)), tb.Text, "#B1");
				Assert.AreEqual (today, table.Rows [0] ["Date"], "#B2");

				dateColumnStyle.DoEdit (cm, 1, new Rectangle (new Point (0,0), new Size (100, 100)), false, null, true);
				Assert.AreEqual (converter.ConvertTo (null, CultureInfo.CurrentCulture,
					now, typeof (string)), tb.Text, "#C1");
				Assert.AreEqual (now, table.Rows [1] ["Date"], "#C2");

				dateColumnStyle.Format = "MM";

				dateColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, null, true);
				Assert.AreEqual (today.ToString ("MM", CultureInfo.CurrentCulture), tb.Text, "#D1");
				Assert.AreEqual (today, table.Rows [0] ["Date"], "#D2");

				dateColumnStyle.DoEdit (cm, 1, new Rectangle (new Point (0,0), new Size (100, 100)), false, null, true);
				Assert.AreEqual (now.ToString ("MM", CultureInfo.CurrentCulture), tb.Text, "#E1");
				Assert.AreEqual (now, table.Rows [1] ["Date"], "#E2");

				dateColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "INVALID", true);
				Assert.IsFalse (dateColumnStyle.DoCommit (cm, 0), "#F1");
				Assert.AreEqual ("INVALID", tb.Text, "#F2");
				Assert.AreEqual (today, table.Rows [0] ["Date"], "#F3");

				dateColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "12", true);
				Assert.IsFalse (dateColumnStyle.DoCommit (cm, 0), "#G1");
				Assert.AreEqual ("12", tb.Text, "#G2");
				Assert.AreEqual (today, table.Rows [0] ["Date"], "#G3");

				dateColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "07/09/2007", true);
				Assert.IsTrue (dateColumnStyle.DoCommit (cm, 0), "#H1");
				Assert.AreEqual (converter.ConvertTo (null, CultureInfo.CurrentCulture,
					new DateTime (2007, 9, 7), typeof (string)), tb.Text, "#H2");
				Assert.AreEqual (new DateTime (2007, 9, 7), table.Rows [0] ["Date"], "#H3");

				dateColumnStyle.FormatInfo = CultureInfo.CurrentCulture;

				dateColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "08/06/2005", true);
				Assert.IsTrue (dateColumnStyle.DoCommit (cm, 0), "#I1");
				Assert.AreEqual ("06", tb.Text, "#I2");
				Assert.AreEqual (new DateTime (2005, 6, 8), table.Rows [0] ["Date"], "#I3");
			} finally {
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}

		[Test]
		public void StringConverterTest ()
		{
			CultureInfo originalCulture = CultureInfo.CurrentCulture;

			try {
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-GB");

				table = new DataTable ();
				view = table.DefaultView;
				table.Columns.Add (new DataColumn ("Amount", typeof (MockNumericStringConvertable)));

				DataRow row = table.NewRow ();
				row ["Amount"] = new MockNumericStringConvertable (1);
				table.Rows.Add (row);

				row = table.NewRow ();
				row ["Amount"] = new MockNumericStringConvertable (2);
				table.Rows.Add (row);

				tableStyle = new DataGridTableStyle ();
				ColumnPoker amountColumnStyle = new ColumnPoker ();
				amountColumnStyle.MappingName = "Amount";
				tableStyle.GridColumnStyles.Add (amountColumnStyle);

				BindingContext bc = new BindingContext ();
				DataGrid dg = new DataGrid ();
				dg.BindingContext = bc;
				dg.TableStyles.Add (tableStyle);
				dg.DataSource = table;

				CurrencyManager cm = (CurrencyManager) bc [view];
				DataGridTextBox tb = (DataGridTextBox) amountColumnStyle.TextBox;

				Assert.IsNotNull (tb, "#A1");
				Assert.AreEqual (string.Empty, tb.Text, "#A2");
				Assert.IsTrue (tb.IsInEditOrNavigateMode, "#A3");

				amountColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, null, true);
				Assert.AreEqual ("£1.00", tb.Text, "#B1");
				Assert.AreEqual (new MockNumericStringConvertable (1), table.Rows [0] ["Amount"], "#B2");
				Assert.IsTrue (tb.IsInEditOrNavigateMode, "#B3");

				amountColumnStyle.DoEdit (cm, 1, new Rectangle (new Point (0,0), new Size (100, 100)), false, null, true);
				Assert.AreEqual ("£2.00", tb.Text, "#C1");
				Assert.AreEqual (new MockNumericStringConvertable (2), table.Rows [1] ["Amount"], "#C2");
				Assert.IsTrue (tb.IsInEditOrNavigateMode, "#C3");

				amountColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "£3.00", true);
				Assert.AreEqual ("£3.00", tb.Text, "#D1");
				Assert.AreEqual (new MockNumericStringConvertable (1), table.Rows [0] ["Amount"], "#D2");
				Assert.IsFalse (tb.IsInEditOrNavigateMode, "#D3");

				Assert.IsTrue (amountColumnStyle.DoCommit (cm, cm.Position), "#E1");
				Assert.AreEqual ("£3.00", tb.Text, "#E2");
				Assert.AreEqual (new MockNumericStringConvertable (3), table.Rows [0] ["Amount"], "#E3");
				Assert.IsTrue (tb.IsInEditOrNavigateMode, "#E4");

				amountColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, "INVALID", true);
				Assert.IsFalse (amountColumnStyle.DoCommit (cm, cm.Position), "#F1");
				Assert.AreEqual ("INVALID", tb.Text, "#F2");
				Assert.AreEqual (new MockNumericStringConvertable (3), table.Rows [0] ["Amount"], "#F3");
				Assert.IsFalse (tb.IsInEditOrNavigateMode, "#F4");

				amountColumnStyle.Format = "whatever";
				amountColumnStyle.FormatInfo = new CultureInfo ("en-US");

				amountColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0,0), new Size (100, 100)), false, null, true);
				Assert.AreEqual ("£3.00", tb.Text, "#G1");
				Assert.AreEqual (new MockNumericStringConvertable (3), table.Rows [0] ["Amount"], "#G2");
				Assert.IsFalse (tb.IsInEditOrNavigateMode, "#G3");

				tb.Text = "5";
				Assert.IsTrue (amountColumnStyle.DoCommit (cm, cm.Position), "#H1");
				Assert.AreEqual ("£5.00", tb.Text, "#H2");
				Assert.AreEqual (new MockNumericStringConvertable (5), table.Rows [0] ["Amount"], "#H3");
			} finally {
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}

		[Test]
		public void NonStringConverterTest ()
		{
			CultureInfo originalCulture = CultureInfo.CurrentCulture;

			try {
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-GB");

				table = new DataTable ();
				view = table.DefaultView;
				table.Columns.Add (new DataColumn ("Amount", typeof (MockNumericNonStringConvertable)));

				DataRow row = table.NewRow ();
				row ["Amount"] = new MockNumericNonStringConvertable (1);
				table.Rows.Add (row);

				row = table.NewRow ();
				row ["Amount"] = new MockNumericNonStringConvertable (2);
				table.Rows.Add (row);

				tableStyle = new DataGridTableStyle ();
				ColumnPoker amountColumnStyle = new ColumnPoker ();
				amountColumnStyle.MappingName = "Amount";
				tableStyle.GridColumnStyles.Add (amountColumnStyle);

				BindingContext bc = new BindingContext ();
				DataGrid dg = new DataGrid ();
				dg.BindingContext = bc;
				dg.TableStyles.Add (tableStyle);
				dg.DataSource = table;

				CurrencyManager cm = (CurrencyManager) bc [view];
				TextBox tb = amountColumnStyle.TextBox;

				Assert.IsNotNull (tb, "#A1");
				Assert.AreEqual (string.Empty, tb.Text, "#A2");

				amountColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0, 0), new Size (100, 100)), false, null, true);
				Assert.AreEqual ("uno", tb.Text, "#B1");
				Assert.AreEqual (new MockNumericStringConvertable (1), table.Rows [0] ["Amount"], "#B2");

				amountColumnStyle.DoEdit (cm, 1, new Rectangle (new Point (0, 0), new Size (100, 100)), false, null, true);
				Assert.AreEqual ("dos", tb.Text, "#C1");
				Assert.AreEqual (new MockNumericStringConvertable (2), table.Rows [1] ["Amount"], "#C2");

				amountColumnStyle.DoEdit (cm, 0, new Rectangle (new Point (0, 0), new Size (100, 100)), false, "£3.00", true);
				Assert.AreEqual ("£3.00", tb.Text, "#D1");
				Assert.AreEqual (new MockNumericStringConvertable (1), table.Rows [0] ["Amount"], "#D2");

				Assert.IsTrue (amountColumnStyle.DoCommit (cm, cm.Position), "#E1");
				Assert.AreEqual ("£3.00", tb.Text, "#E2");
				Assert.AreEqual ("£3.00", table.Rows [0] ["Amount"], "#E3");
			} finally {
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}

		class MockNumeric
		{
			public MockNumeric (int number)
			{
				this.number = number;
			}

			public int Number {
				get { return number; }
				set { number = value; }
			}

			public override bool Equals (object obj)
			{
				if (obj == null)
					return false;

				MockNumeric numeric = obj as MockNumeric;
				return (numeric != null && numeric.Number == Number);
			}

			public override int GetHashCode ()
			{
				return number.GetHashCode ();
			}

			public override string ToString ()
			{
				switch (Number) {
				case 1:
					return "uno";
				case 2:
					return "dos";
				case 3:
					return "tres";
				default:
					return "bad";
				}
			}

			private int number;
		}

		class MockNumericFormattable : MockNumeric, IFormattable
		{
			public MockNumericFormattable (int number) : base (number)
			{
			}

			string IFormattable.ToString (string format, IFormatProvider formatProvider)
			{
				if (format == "currency") {
					return "#" + Number.ToString ("c", formatProvider);
				} else {
					return "#" + Number.ToString (formatProvider);
				}
			}
		}

		[TypeConverter (typeof (MockNumericStringConverter))]
		class MockNumericStringConvertable : MockNumeric
		{
			public MockNumericStringConvertable (int number) : base (number)
			{
			}
		}

		[TypeConverter (typeof (MockNumericNonStringConverter))]
		class MockNumericNonStringConvertable : MockNumeric
		{
			public MockNumericNonStringConvertable (int number) : base (number)
			{
			}
		}

		class MockNumericStringConverter : TypeConverter
		{
			public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
			{
				if (sourceType == typeof (string))
					return true;
				return false;
			}

			public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
			{
				if (destinationType == typeof (string))
					return true;
				return false;
			}

			public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				if (value == null)
					return value;

				string val = value as string;
				if (val != null) {
					int number = int.Parse (val, NumberStyles.Currency, culture);
					return new MockNumericStringConvertable (number);
				}

				return base.ConvertFrom (context, culture, value);
			}

			public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				if (value == null)
					return value;

				if (destinationType == typeof (string)) {
					MockNumeric val = value as MockNumeric;
					return val.Number.ToString ("C", culture);
				}

				return base.ConvertTo (context, culture, value, destinationType);
			}
		}

		class MockNumericNonStringConverter : TypeConverter
		{
			public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
			{
				return false;
			}

			public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
			{
				return false;
			}

			public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				throw new NotSupportedException ();
			}

			public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				throw new NotSupportedException ();
			}
		}
	}
}
