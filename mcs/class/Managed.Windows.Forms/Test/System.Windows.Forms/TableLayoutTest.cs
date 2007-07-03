//
// TableLayoutTests.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

#if NET_2_0
using System;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class TableLayoutTests
	{
		[Test]
		public void TestConstruction ()
		{
			TableLayoutPanel p = new TableLayoutPanel ();

			Assert.AreEqual (BorderStyle.None, p.BorderStyle, "A1");
			Assert.AreEqual (TableLayoutPanelCellBorderStyle.None, p.CellBorderStyle, "A2");
			Assert.AreEqual (0, p.ColumnCount, "A3");
			Assert.AreEqual (TableLayoutPanelGrowStyle.AddRows, p.GrowStyle, "A4");
			Assert.AreEqual ("System.Windows.Forms.Layout.TableLayout", p.LayoutEngine.ToString (), "A5");
			Assert.AreEqual ("System.Windows.Forms.TableLayoutSettings", p.LayoutSettings.ToString (), "A6");
			Assert.AreEqual (0, p.RowCount, "A7");
			Assert.AreEqual (0, p.ColumnStyles.Count, "A8");
			Assert.AreEqual (0, p.RowStyles.Count, "A9");
			Assert.AreEqual (new Size (200, 100), p.Size, "A10");
		}

		[Test]
		public void TestPropertySetters ()
		{
			TableLayoutPanel p = new TableLayoutPanel ();

			p.BorderStyle = BorderStyle.Fixed3D;
			p.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetDouble;
			p.ColumnCount = 1;
			p.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
			p.RowCount = 1;

			Assert.AreEqual (BorderStyle.Fixed3D, p.BorderStyle, "A1");
			Assert.AreEqual (TableLayoutPanelCellBorderStyle.OutsetDouble, p.CellBorderStyle, "A2");
			Assert.AreEqual (1, p.ColumnCount, "A3");
			Assert.AreEqual (TableLayoutPanelGrowStyle.FixedSize, p.GrowStyle, "A4");
			Assert.AreEqual (1, p.RowCount, "A7");
		}

		[Test]
		public void TestExtenderMethods ()
		{
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c = new Button ();

			Assert.AreEqual (new TableLayoutPanelCellPosition (-1, -1), p.GetCellPosition (c), "A1");
			Assert.AreEqual (-1, p.GetColumn (c), "A2");
			Assert.AreEqual (1, p.GetColumnSpan (c), "A3");
			Assert.AreEqual (-1, p.GetRow (c), "A4");
			Assert.AreEqual (1, p.GetRowSpan (c), "A5");

			p.SetCellPosition (c, new TableLayoutPanelCellPosition (1, 1));
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 1), p.GetCellPosition (c), "A6");

			p.SetColumn (c, 2);
			Assert.AreEqual (2, p.GetColumn (c), "A7");
			p.SetRow (c, 2);
			Assert.AreEqual (2, p.GetRow (c), "A9");

			p.SetColumnSpan (c, 2);
			Assert.AreEqual (2, p.GetColumnSpan (c), "A8");


			p.SetRowSpan (c, 2);
			Assert.AreEqual (2, p.GetRowSpan (c), "A10");

			Assert.AreEqual (new TableLayoutPanelCellPosition (2, 2), p.GetCellPosition (c), "A11");

			// ???????
			//Assert.AreEqual (new TableLayoutPanelCellPosition (-1, -1), p.GetPositionFromControl (c), "A12");
			//Assert.AreEqual (c, p.GetControlFromPosition(0, 0), "A13");
		}

		[Test]
		public void TestColumnStyles ()
		{
			TableLayoutPanel p = new TableLayoutPanel ();

			p.ColumnStyles.Add (new ColumnStyle ());
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Absolute));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Percent, 20F));

			Assert.AreEqual (3, p.ColumnStyles.Count, "A1");
			Assert.AreEqual (SizeType.AutoSize, p.ColumnStyles[0].SizeType, "A2");
			Assert.AreEqual (0, p.ColumnStyles[0].Width, "A3");
			Assert.AreEqual (SizeType.Absolute, p.ColumnStyles[1].SizeType, "A4");
			Assert.AreEqual (0, p.ColumnStyles[1].Width, "A5");
			Assert.AreEqual (SizeType.Percent, p.ColumnStyles[2].SizeType, "A6");
			Assert.AreEqual (20F, p.ColumnStyles[2].Width, "A7");

			p.ColumnStyles.Remove (p.ColumnStyles[0]);

			Assert.AreEqual (2, p.ColumnStyles.Count, "A8");
			Assert.AreEqual (SizeType.Absolute, p.ColumnStyles[0].SizeType, "A9");
			Assert.AreEqual (0, p.ColumnStyles[0].Width, "A10");
			Assert.AreEqual (SizeType.Percent, p.ColumnStyles[1].SizeType, "A11");
			Assert.AreEqual (20F, p.ColumnStyles[1].Width, "A12");
		}

		[Test]
		public void TestRowStyles ()
		{
			TableLayoutPanel p = new TableLayoutPanel ();

			p.RowStyles.Add (new RowStyle ());
			p.RowStyles.Add (new RowStyle (SizeType.Absolute));
			p.RowStyles.Add (new RowStyle (SizeType.Percent, 20F));

			Assert.AreEqual (3, p.RowStyles.Count, "A1");
			Assert.AreEqual (SizeType.AutoSize, p.RowStyles[0].SizeType, "A2");
			Assert.AreEqual (0, p.RowStyles[0].Height, "A3");
			Assert.AreEqual (SizeType.Absolute, p.RowStyles[1].SizeType, "A4");
			Assert.AreEqual (0, p.RowStyles[1].Height, "A5");
			Assert.AreEqual (SizeType.Percent, p.RowStyles[2].SizeType, "A6");
			Assert.AreEqual (20F, p.RowStyles[2].Height, "A7");

			p.RowStyles.Remove (p.RowStyles[0]);

			Assert.AreEqual (2, p.RowStyles.Count, "A8");
			Assert.AreEqual (SizeType.Absolute, p.RowStyles[0].SizeType, "A9");
			Assert.AreEqual (0, p.RowStyles[0].Height, "A10");
			Assert.AreEqual (SizeType.Percent, p.RowStyles[1].SizeType, "A11");
			Assert.AreEqual (20F, p.RowStyles[1].Height, "A12");
		}

		[Test]
		public void TestColumnStyles3 ()
		{
			// Don't lose the 2nd style
			TableLayoutPanel p = new TableLayoutPanel ();

			p.ColumnCount = 2;
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Absolute, 20F));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Absolute, 20F));

			p.ColumnCount = 1;

			Assert.AreEqual (2, p.ColumnStyles.Count, "A1");
		}

		[Test]
		public void TestColumnStyles2 ()
		{
			// Don't lose the 2nd style
			TableLayoutPanel p = new TableLayoutPanel ();

			p.ColumnCount = 1;
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Absolute, 20F));

			p.ColumnCount = 2;

			Assert.AreEqual (1, p.ColumnStyles.Count, "A2");
		}

		[Test]
		public void TestCellPositioning ()
		{
			// Standard Add
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();
			Control c4 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);
			p.Controls.Add (c4);

			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 0), p.GetPositionFromControl (c1), "C1");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 0), p.GetPositionFromControl (c2), "C2");
			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 1), p.GetPositionFromControl (c3), "C3");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 1), p.GetPositionFromControl (c4), "C4");
		}

		[Test]
		public void TestCellPositioning2 ()
		{
			// Growstyle = Add Rows
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();
			Control c4 = new Button ();
			Control c5 = new Button ();
			Control c6 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);
			p.Controls.Add (c4);
			p.Controls.Add (c5);
			p.Controls.Add (c6);

			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 0), p.GetPositionFromControl (c1), "C1");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 0), p.GetPositionFromControl (c2), "C2");
			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 1), p.GetPositionFromControl (c3), "C3");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 1), p.GetPositionFromControl (c4), "C4");
			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 2), p.GetPositionFromControl (c5), "C5");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 2), p.GetPositionFromControl (c6), "C6");
		}

		[Test]
		public void TestCellPositioning3 ()
		{
			// Growstyle = Add Columns
			TableLayoutPanel p = new TableLayoutPanel ();
			p.GrowStyle = TableLayoutPanelGrowStyle.AddColumns;

			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();
			Control c4 = new Button ();
			Control c5 = new Button ();
			Control c6 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);
			p.Controls.Add (c4);
			p.Controls.Add (c5);
			p.Controls.Add (c6);

			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 0), p.GetPositionFromControl (c1), "C1");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 0), p.GetPositionFromControl (c2), "C2");
			Assert.AreEqual (new TableLayoutPanelCellPosition (2, 0), p.GetPositionFromControl (c3), "C3");
			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 1), p.GetPositionFromControl (c4), "C4");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 1), p.GetPositionFromControl (c5), "C5");
			Assert.AreEqual (new TableLayoutPanelCellPosition (2, 1), p.GetPositionFromControl (c6), "C6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestCellPositioning4 ()
		{
			// Growstyle = Fixed Size
			TableLayoutPanel p = new TableLayoutPanel ();
			p.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;

			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();
			Control c4 = new Button ();
			Control c5 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);
			p.Controls.Add (c4);
			p.Controls.Add (c5);
		}

		[Test]
		public void TestCellPositioning5 ()
		{
			// One control have fixed position
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();
			Control c4 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.SetCellPosition (c4, new TableLayoutPanelCellPosition (0, 0));

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);
			p.Controls.Add (c4);

			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 0), p.GetPositionFromControl (c4), "C1");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 0), p.GetPositionFromControl (c1), "C2");
			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 1), p.GetPositionFromControl (c2), "C3");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 1), p.GetPositionFromControl (c3), "C4");
		}

		[Test]
		public void TestCellPositioning6 ()
		{
			// One control has fixed column, it should be ignored
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();
			Control c4 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.SetColumn (c3, 1);

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);
			p.Controls.Add (c4);

			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 0), p.GetPositionFromControl (c1), "C1");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 0), p.GetPositionFromControl (c2), "C2");
			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 1), p.GetPositionFromControl (c3), "C3");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 1), p.GetPositionFromControl (c4), "C4");
		}

		[Test]
		public void TestCellPositioning7 ()
		{
			// One control has fixed column and row
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();
			Control c4 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.SetColumn (c3, 1);
			p.SetRow (c3, 1);

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);
			p.Controls.Add (c4);

			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 0), p.GetPositionFromControl (c1), "C1");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 0), p.GetPositionFromControl (c2), "C2");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 1), p.GetPositionFromControl (c3), "C3");
			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 1), p.GetPositionFromControl (c4), "C4");
		}

		[Test]
		public void TestCellPositioning8 ()
		{
			// Column span
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.SetColumnSpan (c1, 2);

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 0), p.GetPositionFromControl (c1), "C1");
			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 1), p.GetPositionFromControl (c2), "C2");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 1), p.GetPositionFromControl (c3), "C3");
		}

		[Test]
		public void TestCellPositioning9 ()
		{
			// Row span
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.SetRowSpan (c1, 2);

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 0), p.GetPositionFromControl (c1), "C1");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 0), p.GetPositionFromControl (c2), "C2");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 1), p.GetPositionFromControl (c3), "C3");
		}

		[Test]
		public void TestCellPositioning10 ()
		{
			// Column span = 2, but control is in the last column, forces control back into 1st column, next row
			// I have no clue why c3 shouldn't be in (1,0), but MS says it's not
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.SetColumnSpan (c2, 2);

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 0), p.GetPositionFromControl (c1), "C1");
			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 1), p.GetPositionFromControl (c2), "C2");
			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 2), p.GetPositionFromControl (c3), "C3");
		}

		[Test]
		public void TestCellPositioning11 ()
		{
			// Row span = 2, but control is in the last row, creates new row
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.SetRowSpan (c3, 2);

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 0), p.GetPositionFromControl (c1), "C1");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 0), p.GetPositionFromControl (c2), "C2");
			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 1), p.GetPositionFromControl (c3), "C3");
		}

		[Test]
		public void TestCellPositioning12 ()
		{
			// Requesting a column greater than ColumnCount, request is ignored
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.SetColumn (c1, 4);

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 0), p.GetPositionFromControl (c1), "C1");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 0), p.GetPositionFromControl (c2), "C2");
			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 1), p.GetPositionFromControl (c3), "C3");
		}

		[Test]
		public void TestCellPositioning13 ()
		{
			// Row span = 2, but control is in the last row, creates new row
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 3;
			p.RowCount = 2;

			p.SetRowSpan (c3, 2);

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 0), p.GetPositionFromControl (c1), "C1");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 0), p.GetPositionFromControl (c2), "C2");
			Assert.AreEqual (new TableLayoutPanelCellPosition (2, 0), p.GetPositionFromControl (c3), "C3");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestCellPositioning14 ()
		{
			// Col span = 3, fixed grow style
			TableLayoutPanel p = new TableLayoutPanel ();
			p.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
			Control c1 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.SetColumnSpan (c1, 3);

			p.Controls.Add (c1);

			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 0), p.GetPositionFromControl (c1), "C1");
		}

		[Test]
		public void TestCellPositioning15 ()
		{
			// Column span = 2, but control is in the last column, forces control back into 1st column, next row
			// I have no clue why c3 shouldn't be in (1,0), but MS says it's not
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.SetColumnSpan (c2, 2);
			p.SetCellPosition (c2, new TableLayoutPanelCellPosition (1, 0));

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 0), p.GetPositionFromControl (c1), "C1");
			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 1), p.GetPositionFromControl (c2), "C2");
			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 2), p.GetPositionFromControl (c3), "C3");
		}

		[Test]
		public void TestCellPositioning16 ()
		{
			// Row span = 2, but control is in the last row, creates new row
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.SetRowSpan (c3, 2);
			p.SetCellPosition (c3, new TableLayoutPanelCellPosition (0, 1));

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 0), p.GetPositionFromControl (c1), "C1");
			Assert.AreEqual (new TableLayoutPanelCellPosition (1, 0), p.GetPositionFromControl (c2), "C2");
			Assert.AreEqual (new TableLayoutPanelCellPosition (0, 1), p.GetPositionFromControl (c3), "C3");
		}

		[Test]
		public void TestRowColumnSizes1 ()
		{
			// Row span = 2, but control is in the last row, creates new row
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 1;

			p.RowStyles.Add (new RowStyle (SizeType.Percent, 100F));

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (71, p.GetRowHeights ()[0], "D1");
			Assert.AreEqual (29, p.GetRowHeights ()[1], "D2");
		}

		[Test]
		public void TestRowColumnSizes2 ()
		{
			// Row span = 2, but control is in the last row, creates new row
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 1;

			p.RowStyles.Add (new RowStyle (SizeType.Absolute, 100F));

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (100, p.GetRowHeights ()[0], "D1");
			Assert.AreEqual (29, p.GetRowHeights ()[1], "D2");
		}

		[Test]
		public void TestRowColumnSizes3 ()
		{
			// Row span = 2, but control is in the last row, creates new row
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();
			Control c4 = new Button ();
			Control c5 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 1;

			p.RowStyles.Add (new RowStyle (SizeType.Percent, 100F));

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);
			p.Controls.Add (c4);
			p.Controls.Add (c5);

			Assert.AreEqual (42, p.GetRowHeights ()[0], "D1");
			Assert.AreEqual (29, p.GetRowHeights ()[1], "D2");
			Assert.AreEqual (29, p.GetRowHeights ()[2], "D3");
		}

		[Test]
		public void TestRowColumnSizes4 ()
		{
			// Row span = 2, but control is in the last row, creates new row
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();
			Control c4 = new Button ();
			Control c5 = new Button ();
			Control c6 = new Button ();
			Control c7 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 1;

			p.RowStyles.Add (new RowStyle (SizeType.Percent, 100F));

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);
			p.Controls.Add (c4);
			p.Controls.Add (c5);
			p.Controls.Add (c6);
			p.Controls.Add (c7);

			//Assert.AreEqual (100, p.GetRowHeights ()[0], "D1");
			Assert.AreEqual (29, p.GetRowHeights ()[1], "D2");
			Assert.AreEqual (29, p.GetRowHeights ()[2], "D3");
			Assert.AreEqual (29, p.GetRowHeights ()[3], "D4");
		}

		[Test]
		public void TestRowColumnSizes5 ()
		{
			// 2 Absolute Columns/Rows
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.RowStyles.Add (new RowStyle (SizeType.Absolute, 20));
			p.RowStyles.Add (new RowStyle (SizeType.Absolute, 30));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Absolute, 20));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Absolute, 30));

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (20, p.GetRowHeights ()[0], "D1");
			Assert.AreEqual (80, p.GetRowHeights ()[1], "D2");
			Assert.AreEqual (20, p.GetColumnWidths ()[0], "D3");
			Assert.AreEqual (180, p.GetColumnWidths ()[1], "D4");
		}

		[Test]
		public void TestRowColumnSizes6 ()
		{
			// 2 50% Columns/Rows
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.RowStyles.Add (new RowStyle (SizeType.Percent, 50));
			p.RowStyles.Add (new RowStyle (SizeType.Percent, 50));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Percent, 50));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Percent, 50));

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (50, p.GetRowHeights ()[0], "D1");
			Assert.AreEqual (50, p.GetRowHeights ()[1], "D2");
			Assert.AreEqual (100, p.GetColumnWidths ()[0], "D3");
			Assert.AreEqual (100, p.GetColumnWidths ()[1], "D4");
		}

		[Test]
		public void TestRowColumnSizes7 ()
		{
			// 1 Absolute and 2 Percent Columns/Rows
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 3;
			p.RowCount = 3;

			p.RowStyles.Add (new RowStyle (SizeType.Absolute, 50));
			p.RowStyles.Add (new RowStyle (SizeType.Percent, 50));
			p.RowStyles.Add (new RowStyle (SizeType.Percent, 50));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Absolute, 50));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Percent, 50));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Percent, 50));

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (50, p.GetRowHeights ()[0], "D1");
			Assert.AreEqual (25, p.GetRowHeights ()[1], "D2");
			Assert.AreEqual (25, p.GetRowHeights ()[2], "D3");
			Assert.AreEqual (50, p.GetColumnWidths ()[0], "D4");
			Assert.AreEqual (75, p.GetColumnWidths ()[1], "D5");
			Assert.AreEqual (75, p.GetColumnWidths ()[2], "D6");
		}

		[Test]
		public void TestRowColumnSizes8 ()
		{
			// 1 Absolute and 2 Percent Columns/Rows (with total percents > 100)
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 3;
			p.RowCount = 3;

			p.RowStyles.Add (new RowStyle (SizeType.Absolute, 50));
			p.RowStyles.Add (new RowStyle (SizeType.Percent, 100));
			p.RowStyles.Add (new RowStyle (SizeType.Percent, 100));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Absolute, 50));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Percent, 100));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Percent, 100));

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (50, p.GetRowHeights ()[0], "D1");
			Assert.AreEqual (25, p.GetRowHeights ()[1], "D2");
			Assert.AreEqual (25, p.GetRowHeights ()[2], "D3");
			Assert.AreEqual (50, p.GetColumnWidths ()[0], "D4");
			Assert.AreEqual (75, p.GetColumnWidths ()[1], "D5");
			Assert.AreEqual (75, p.GetColumnWidths ()[2], "D6");
		}

		[Test]
		public void TestRowColumnSizes9 ()
		{
			// 1 Absolute and 2 Percent Columns/Rows (with total percents > 100)
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 3;
			p.RowCount = 3;

			p.RowStyles.Add (new RowStyle (SizeType.Absolute, 50));
			p.RowStyles.Add (new RowStyle (SizeType.Percent, 80));
			p.RowStyles.Add (new RowStyle (SizeType.Percent, 40));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Absolute, 50));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Percent, 80));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.Percent, 40));

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (50, p.GetRowHeights ()[0], "D1");
			Assert.AreEqual (33, p.GetRowHeights ()[1], "D2");
			Assert.AreEqual (17, p.GetRowHeights ()[2], "D3");
			Assert.AreEqual (50, p.GetColumnWidths ()[0], "D4");
			Assert.AreEqual (100, p.GetColumnWidths ()[1], "D5");
			Assert.AreEqual (50, p.GetColumnWidths ()[2], "D6");
		}

		[Test]
		public void TestRowColumnSizes10 ()
		{
			// 2 AutoSize Columns/Rows
			TableLayoutPanel p = new TableLayoutPanel ();
			Control c1 = new Button ();
			Control c2 = new Button ();
			Control c3 = new Button ();

			p.ColumnCount = 2;
			p.RowCount = 2;

			p.RowStyles.Add (new RowStyle (SizeType.AutoSize));
			p.RowStyles.Add (new RowStyle (SizeType.AutoSize));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.AutoSize));
			p.ColumnStyles.Add (new ColumnStyle (SizeType.AutoSize));

			p.Controls.Add (c1);
			p.Controls.Add (c2);
			p.Controls.Add (c3);

			Assert.AreEqual (29, p.GetRowHeights ()[0], "D1");
			Assert.AreEqual (71, p.GetRowHeights ()[1], "D2");
			Assert.AreEqual (81, p.GetColumnWidths ()[0], "D3");
			Assert.AreEqual (119, p.GetColumnWidths ()[1], "D4");
		}
		
		[Test]
		public void Bug81843 ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			
		        TableLayoutPanel tableLayoutPanel1;
			Button button2;
			TextBox textBox1;
			Button button4;

			tableLayoutPanel1 = new TableLayoutPanel ();
			button2 = new Button ();
			button4 = new Button ();
			textBox1 = new TextBox ();
			tableLayoutPanel1.SuspendLayout ();
			f.SuspendLayout ();

			tableLayoutPanel1.AutoSize = true;
			tableLayoutPanel1.ColumnCount = 3;
			tableLayoutPanel1.ColumnStyles.Add (new ColumnStyle ());
			tableLayoutPanel1.ColumnStyles.Add (new ColumnStyle ());
			tableLayoutPanel1.ColumnStyles.Add (new ColumnStyle ());
			tableLayoutPanel1.Controls.Add (button2, 0, 1);
			tableLayoutPanel1.Controls.Add (button4, 2, 1);
			tableLayoutPanel1.Controls.Add (textBox1, 1, 0);
			tableLayoutPanel1.Location = new Point (0, 0);
			tableLayoutPanel1.RowCount = 2;
			tableLayoutPanel1.RowStyles.Add (new RowStyle (SizeType.Percent, 50F));
			tableLayoutPanel1.RowStyles.Add (new RowStyle (SizeType.Percent, 50F));
			tableLayoutPanel1.Size = new Size (292, 287);

			button2.Size = new Size (75, 23);
			
			button4.Size = new Size (75, 23);

			textBox1.Dock = DockStyle.Fill;
			textBox1.Location = new Point (84, 3);
			textBox1.Multiline = true;
			textBox1.Size = new Size (94, 137);

			f.ClientSize = new Size (292, 312);
			f.Controls.Add (tableLayoutPanel1);
			f.Name = "Form1";
			f.Text = "Form1";
			tableLayoutPanel1.ResumeLayout (false);
			tableLayoutPanel1.PerformLayout ();
			f.ResumeLayout (false);
			f.PerformLayout ();

			f.Show ();

			Assert.AreEqual (new Rectangle (3, 146, 75, 23), button2.Bounds, "A1");
			Assert.AreEqual (new Rectangle (184, 146, 75, 23), button4.Bounds, "A2");
			Assert.AreEqual (new Rectangle (84, 3, 94, 137), textBox1.Bounds, "A3");
			
			f.Dispose ();
		}
		
		[Test]  // From bug #81884
		public void CellBorderStyle ()
		{
			if (TestHelper.RunningOnUnix)
				Assert.Ignore ("Depends on WM, values correspond to windows");
			
			Form f = new Form ();
			f.ShowInTaskbar = false;

			TableLayoutPanel p = new TableLayoutPanel ();
			p = new TableLayoutPanel ();
			p.ColumnCount = 3;
			p.ColumnStyles.Add (new ColumnStyle ());
			p.ColumnStyles.Add (new ColumnStyle ());
			p.ColumnStyles.Add (new ColumnStyle ());
			p.Dock = DockStyle.Top;
			p.Height = 200;
			p.RowCount = 2;
			p.RowStyles.Add (new RowStyle (SizeType.Percent, 50F));
			p.RowStyles.Add (new RowStyle (SizeType.Percent, 50F));
			f.Controls.Add (p);

			Label _labelA = new Label ();
			_labelA.Dock = DockStyle.Fill;
			_labelA.Size = new Size (95, 20);
			_labelA.Text = "A";
			p.Controls.Add (_labelA, 0, 0);

			Label _labelB = new Label ();
			_labelB.Dock = DockStyle.Fill;
			_labelB.Size = new Size (95, 20);
			_labelB.Text = "B";
			p.Controls.Add (_labelB, 1, 0);

			Label _labelC = new Label ();
			_labelC.Dock = DockStyle.Fill;
			_labelC.Size = new Size (95, 20);
			_labelC.Text = "C";
			p.Controls.Add (_labelC, 2, 0);

			Label _labelD = new Label ();
			_labelD.Dock = DockStyle.Fill;
			_labelD.Size = new Size (95, 20);
			_labelD.Text = "D";
			p.Controls.Add (_labelD, 0, 1);

			Label _labelE = new Label ();
			_labelE.Dock = DockStyle.Fill;
			_labelE.Size = new Size (95, 20);
			_labelE.Text = "E";
			p.Controls.Add (_labelE, 1, 1);

			Label _labelF = new Label ();
			_labelF.Dock = DockStyle.Fill;
			_labelF.Size = new Size (95, 20);
			_labelF.Text = "F";
			p.Controls.Add (_labelF, 2, 1);

			_labelA.BackColor = Color.Red;
			_labelB.BackColor = Color.Orange;
			_labelC.BackColor = Color.Yellow;
			_labelD.BackColor = Color.Green;
			_labelE.BackColor = Color.Blue;
			_labelF.BackColor = Color.Purple;

			f.Show ();
			// None
			Assert.AreEqual (new Rectangle (3, 0, 95, 100), _labelA.Bounds, "A1");
			Assert.AreEqual (new Rectangle (104, 0, 95, 100), _labelB.Bounds, "A2");
			Assert.AreEqual (new Rectangle (205, 0, 95, 100), _labelC.Bounds, "A3");
			Assert.AreEqual (new Rectangle (3, 100, 95, 100), _labelD.Bounds, "A4");
			Assert.AreEqual (new Rectangle (104, 100, 95, 100), _labelE.Bounds, "A5");
			Assert.AreEqual (new Rectangle (205, 100, 95, 100), _labelF.Bounds, "A6");
			
			p.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
			Assert.AreEqual (new Rectangle (4, 1, 95, 98), _labelA.Bounds, "A7");
			Assert.AreEqual (new Rectangle (106, 1, 95, 98), _labelB.Bounds, "A8");
			Assert.AreEqual (new Rectangle (208, 1, 95, 98), _labelC.Bounds, "A9");
			Assert.AreEqual (new Rectangle (4, 100, 95, 99), _labelD.Bounds, "A10");
			Assert.AreEqual (new Rectangle (106, 100, 95, 99), _labelE.Bounds, "A11");
			Assert.AreEqual (new Rectangle (208, 100, 95, 99), _labelF.Bounds, "A12");

			p.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
			Assert.AreEqual (new Rectangle (5, 2, 95, 97), _labelA.Bounds, "A13");
			Assert.AreEqual (new Rectangle (108, 2, 95, 97), _labelB.Bounds, "A14");
			Assert.AreEqual (new Rectangle (211, 2, 95, 97), _labelC.Bounds, "A15");
			Assert.AreEqual (new Rectangle (5, 101, 95, 97), _labelD.Bounds, "A16");
			Assert.AreEqual (new Rectangle (108, 101, 95, 97), _labelE.Bounds, "A17");
			Assert.AreEqual (new Rectangle (211, 101, 95, 97), _labelF.Bounds, "A18");

			p.CellBorderStyle = TableLayoutPanelCellBorderStyle.InsetDouble;
			Assert.AreEqual (new Rectangle (6, 3, 95, 95), _labelA.Bounds, "A19");
			Assert.AreEqual (new Rectangle (110, 3, 95, 95), _labelB.Bounds, "A20");
			Assert.AreEqual (new Rectangle (214, 3, 95, 95), _labelC.Bounds, "A21");
			Assert.AreEqual (new Rectangle (6, 101, 95, 96), _labelD.Bounds, "A22");
			Assert.AreEqual (new Rectangle (110, 101, 95, 96), _labelE.Bounds, "A23");
			Assert.AreEqual (new Rectangle (214, 101, 95, 96), _labelF.Bounds, "A24");

			p.CellBorderStyle = TableLayoutPanelCellBorderStyle.Outset;
			Assert.AreEqual (new Rectangle (5, 2, 95, 97), _labelA.Bounds, "A25");
			Assert.AreEqual (new Rectangle (108, 2, 95, 97), _labelB.Bounds, "A26");
			Assert.AreEqual (new Rectangle (211, 2, 95, 97), _labelC.Bounds, "A27");
			Assert.AreEqual (new Rectangle (5, 101, 95, 97), _labelD.Bounds, "A28");
			Assert.AreEqual (new Rectangle (108, 101, 95, 97), _labelE.Bounds, "A29");
			Assert.AreEqual (new Rectangle (211, 101, 95, 97), _labelF.Bounds, "A30");

			p.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetDouble;
			Assert.AreEqual (new Rectangle (6, 3, 95, 95), _labelA.Bounds, "A31");
			Assert.AreEqual (new Rectangle (110, 3, 95, 95), _labelB.Bounds, "A32");
			Assert.AreEqual (new Rectangle (214, 3, 95, 95), _labelC.Bounds, "A33");
			Assert.AreEqual (new Rectangle (6, 101, 95, 96), _labelD.Bounds, "A34");
			Assert.AreEqual (new Rectangle (110, 101, 95, 96), _labelE.Bounds, "A35");
			Assert.AreEqual (new Rectangle (214, 101, 95, 96), _labelF.Bounds, "A36");

			p.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetPartial;
			Assert.AreEqual (new Rectangle (6, 3, 95, 95), _labelA.Bounds, "A37");
			Assert.AreEqual (new Rectangle (110, 3, 95, 95), _labelB.Bounds, "A38");
			Assert.AreEqual (new Rectangle (214, 3, 95, 95), _labelC.Bounds, "A39");
			Assert.AreEqual (new Rectangle (6, 101, 95, 96), _labelD.Bounds, "A40");
			Assert.AreEqual (new Rectangle (110, 101, 95, 96), _labelE.Bounds, "A41");
			Assert.AreEqual (new Rectangle (214, 101, 95, 96), _labelF.Bounds, "A42");
		}
	}
}
#endif