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
using System.Windows.Forms;
using System.Xml;
using NUnit.Framework;

// resolve the ambiguity between System.ComponentModel and NUnit.Framework
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class DataGridTextBoxColumnTest
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
			Assert.AreEqual (new Rectangle (new Point (2,2), new Size (98,98)), tb.Bounds, "3");
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
			Assert.AreEqual (new Rectangle (new Point (2,2), new Size (98,98)), tb.Bounds, "3");
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
			Assert.AreEqual (new Rectangle (new Point (2,2), new Size (98,98)), tb.Bounds, "9");
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
			Assert.AreEqual (new Rectangle (new Point (2,2), new Size (98,98)), tb.Bounds, "3");
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
			Assert.AreEqual (new Rectangle (new Point (2,2), new Size (98,98)), tb.Bounds, "3");
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
	}
}
