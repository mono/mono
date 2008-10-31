//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Author:
//	DataGridViewTest.GenerateClipboardTest (false);
//
#if NET_2_0
using NUnit.Framework;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
namespace MonoTests.System.Windows.Forms {
	[TestFixture]
	public class DataGridViewClipboardTest : TestHelper {
		[Test]
		public void Test () {
			DataObject data;
			DataGridViewRowHeaderTest.DataGridViewRowHeaderClipboardCell row_header_cell;
			DataGridViewColumnHeaderTest.DataGridViewColumnHeaderClipboardCell col_header_cell;
			string code = null;
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#EnableWithAutoHeaderText#0-0");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#1-0");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#1-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000211\r\nStartFragment:00000133\r\nEndFragment:00000175\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#1-2");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#1-3");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#1-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#2-0");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#2-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000211\r\nStartFragment:00000133\r\nEndFragment:00000175\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell C3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#2-2");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#2-3");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#2-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#EnableWithAutoHeaderText#3-0");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#4-0");
				Assert.AreEqual ("Row#1,Cell A1,Cell B1,Cell C1,Cell D1", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#4-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000295\r\nStartFragment:00000133\r\nEndFragment:00000259\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>Cell B1</TD><TD>Cell C1</TD><TD>Cell D1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#4-2");
				Assert.AreEqual ("Row#1\tCell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#4-3");
				Assert.AreEqual ("Row#1\tCell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#4-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#5-0");
				Assert.AreEqual ("Row#3,Cell A3,Cell B3,Cell C3,Cell D3", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#5-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000295\r\nStartFragment:00000133\r\nEndFragment:00000259\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#5-2");
				Assert.AreEqual ("Row#3\tCell A3\tCell B3\tCell C3\tCell D3", data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#5-3");
				Assert.AreEqual ("Row#3\tCell A3\tCell B3\tCell C3\tCell D3", data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#5-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#6-0");
				Assert.AreEqual (string.Format ("Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#6-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000404\r\nStartFragment:00000133\r\nEndFragment:00000368\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#6-2");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#6-3");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#6-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#7-0");
				Assert.AreEqual (string.Format ("Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#4,Cell A4,Cell B4,Cell C4,Cell D4", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#7-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000404\r\nStartFragment:00000133\r\nEndFragment:00000368\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell A4</TD><TD>Cell B4</TD><TD>Cell C4</TD><TD>Cell D4</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#7-2");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#7-3");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#7-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#EnableWithAutoHeaderText#8-0");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#9-0");
				Assert.AreEqual (string.Format ("A{0}Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#9-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000329\r\nStartFragment:00000133\r\nEndFragment:00000293\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>A</TH></THEAD><TR><TD>Cell A1</TD></TR><TR><TD>Cell A2</TD></TR><TR><TD>Cell A3</TD></TR><TR><TD>Cell A4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#9-2");
				Assert.AreEqual (string.Format ("A{0}Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#9-3");
				Assert.AreEqual (string.Format ("A{0}Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#9-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#10-0");
				Assert.AreEqual (string.Format ("C{0}Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#10-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000329\r\nStartFragment:00000133\r\nEndFragment:00000293\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>C</TH></THEAD><TR><TD>Cell C1</TD></TR><TR><TD>Cell C2</TD></TR><TR><TD>Cell C3</TD></TR><TR><TD>Cell C4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#10-2");
				Assert.AreEqual (string.Format ("C{0}Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#10-3");
				Assert.AreEqual (string.Format ("C{0}Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#10-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#11-0");
				Assert.AreEqual (string.Format ("B,C{0}Cell B1,Cell C1{0}Cell B2,Cell C2{0}Cell B3,Cell C3{0}Cell B4,Cell C4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#11-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000412\r\nStartFragment:00000133\r\nEndFragment:00000376\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>B</TH><TH>C</TH></THEAD><TR><TD>Cell B1</TD><TD>Cell C1</TD></TR><TR><TD>Cell B2</TD><TD>Cell C2</TD></TR><TR><TD>Cell B3</TD><TD>Cell C3</TD></TR><TR><TD>Cell B4</TD><TD>Cell C4</TD></TR><TR><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#11-2");
				Assert.AreEqual (string.Format ("B\tC{0}Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\tCell C3{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#11-3");
				Assert.AreEqual (string.Format ("B\tC{0}Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\tCell C3{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#11-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#12-0");
				Assert.AreEqual (string.Format ("B,D{0}Cell B1,Cell D1{0}Cell B2,Cell D2{0}Cell B3,Cell D3{0}Cell B4,Cell D4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#12-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000412\r\nStartFragment:00000133\r\nEndFragment:00000376\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>B</TH><TH>D</TH></THEAD><TR><TD>Cell B1</TD><TD>Cell D1</TD></TR><TR><TD>Cell B2</TD><TD>Cell D2</TD></TR><TR><TD>Cell B3</TD><TD>Cell D3</TD></TR><TR><TD>Cell B4</TD><TD>Cell D4</TD></TR><TR><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#12-2");
				Assert.AreEqual (string.Format ("B\tD{0}Cell B1\tCell D1{0}Cell B2\tCell D2{0}Cell B3\tCell D3{0}Cell B4\tCell D4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#12-3");
				Assert.AreEqual (string.Format ("B\tD{0}Cell B1\tCell D1{0}Cell B2\tCell D2{0}Cell B3\tCell D3{0}Cell B4\tCell D4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#12-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#EnableWithAutoHeaderText#13-0");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#14-0");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#14-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000211\r\nStartFragment:00000133\r\nEndFragment:00000175\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#14-2");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#14-3");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#14-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#15-0");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#15-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000211\r\nStartFragment:00000133\r\nEndFragment:00000175\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell C3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#15-2");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#15-3");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#15-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#16-0");
				Assert.AreEqual ("Row#1,Cell A1,Cell B1,Cell C1,Cell D1", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#16-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000295\r\nStartFragment:00000133\r\nEndFragment:00000259\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>Cell B1</TD><TD>Cell C1</TD><TD>Cell D1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#16-2");
				Assert.AreEqual ("Row#1\tCell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#16-3");
				Assert.AreEqual ("Row#1\tCell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#16-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#17-0");
				Assert.AreEqual ("Row#1,Cell A1,Cell B1,Cell C1,Cell D1", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#17-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000295\r\nStartFragment:00000133\r\nEndFragment:00000259\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>Cell B1</TD><TD>Cell C1</TD><TD>Cell D1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#17-2");
				Assert.AreEqual ("Row#1\tCell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#17-3");
				Assert.AreEqual ("Row#1\tCell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#17-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#18-0");
				Assert.AreEqual ("Row#1,Cell A1,Cell B1,Cell C1,Cell D1", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#18-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000295\r\nStartFragment:00000133\r\nEndFragment:00000259\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>Cell B1</TD><TD>Cell C1</TD><TD>Cell D1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#18-2");
				Assert.AreEqual ("Row#1\tCell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#18-3");
				Assert.AreEqual ("Row#1\tCell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#18-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [0].Cells [1].Selected = true;
				dgv.Rows [0].Cells [2].Selected = true;
				dgv.Rows [0].Cells [3].Selected = true;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#19-0");
				Assert.AreEqual (string.Format ("Cell B1,Cell C1,Cell D1{0},,{0},Cell C3,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#19-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000352\r\nStartFragment:00000133\r\nEndFragment:00000316\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell B1</TD><TD>Cell C1</TD><TD>Cell D1</TD></TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD>&nbsp;</TD><TD>Cell C3</TD><TD>&nbsp;</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#19-2");
				Assert.AreEqual (string.Format ("Cell B1\tCell C1\tCell D1{0}\t\t{0}\tCell C3\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#19-3");
				Assert.AreEqual (string.Format ("Cell B1\tCell C1\tCell D1{0}\t\t{0}\tCell C3\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#19-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#20-0");
				Assert.AreEqual ("Row#3,Cell A3,Cell B3,Cell C3,Cell D3", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#20-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000295\r\nStartFragment:00000133\r\nEndFragment:00000259\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#20-2");
				Assert.AreEqual ("Row#3\tCell A3\tCell B3\tCell C3\tCell D3", data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#20-3");
				Assert.AreEqual ("Row#3\tCell A3\tCell B3\tCell C3\tCell D3", data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#20-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [2].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#21-0");
				Assert.AreEqual (string.Format ("Row#1,Cell A1,,,{0}Row#2,,,,{0}Row#3,Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#21-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000506\r\nStartFragment:00000133\r\nEndFragment:00000470\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#21-2");
				Assert.AreEqual (string.Format ("Row#1\tCell A1\t\t\t{0}Row#2\t\t\t\t{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#21-3");
				Assert.AreEqual (string.Format ("Row#1\tCell A1\t\t\t{0}Row#2\t\t\t\t{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#21-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [2].Cells [0].Selected = true;
				dgv.Rows [2].Cells [1].Selected = true;
				dgv.Rows [2].Cells [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#22-0");
				Assert.AreEqual ("Cell A3,Cell B3,,Cell D3", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#22-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000258\r\nStartFragment:00000133\r\nEndFragment:00000222\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A3</TD><TD>Cell B3</TD><TD>&nbsp;</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#22-2");
				Assert.AreEqual ("Cell A3\tCell B3\t\tCell D3", data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#22-3");
				Assert.AreEqual ("Cell A3\tCell B3\t\tCell D3", data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#22-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#23-0");
				Assert.AreEqual ("Row#3,Cell A3,Cell B3,Cell C3,Cell D3", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#23-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000295\r\nStartFragment:00000133\r\nEndFragment:00000259\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#23-2");
				Assert.AreEqual ("Row#3\tCell A3\tCell B3\tCell C3\tCell D3", data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#23-3");
				Assert.AreEqual ("Row#3\tCell A3\tCell B3\tCell C3\tCell D3", data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#23-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#24-0");
				Assert.AreEqual (string.Format ("Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#24-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000404\r\nStartFragment:00000133\r\nEndFragment:00000368\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#24-2");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#24-3");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#24-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [2].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#25-0");
				Assert.AreEqual (string.Format ("Row#1,Cell A1,,,{0}Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#25-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000510\r\nStartFragment:00000133\r\nEndFragment:00000474\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#25-2");
				Assert.AreEqual (string.Format ("Row#1\tCell A1\t\t\t{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#25-3");
				Assert.AreEqual (string.Format ("Row#1\tCell A1\t\t\t{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#25-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [2].Cells [0].Selected = true;
				dgv.Rows [2].Cells [1].Selected = true;
				dgv.Rows [2].Cells [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#26-0");
				Assert.AreEqual (string.Format ("Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,Cell A3,Cell B3,,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#26-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000403\r\nStartFragment:00000133\r\nEndFragment:00000367\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>&nbsp;</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#26-2");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\t\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#26-3");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\t\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#26-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#27-0");
				Assert.AreEqual (string.Format ("Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#27-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000404\r\nStartFragment:00000133\r\nEndFragment:00000368\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#27-2");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#27-3");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#27-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#28-0");
				Assert.AreEqual (string.Format ("Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,,,,{0}Row#4,Cell A4,Cell B4,Cell C4,Cell D4", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#28-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000509\r\nStartFragment:00000133\r\nEndFragment:00000473\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell A4</TD><TD>Cell B4</TD><TD>Cell C4</TD><TD>Cell D4</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#28-2");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\t\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#28-3");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\t\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#28-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [3].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#29-0");
				Assert.AreEqual (string.Format ("Row#1,Cell A1,,,{0}Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,,,,{0}Row#4,Cell A4,Cell B4,Cell C4,Cell D4", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#29-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000615\r\nStartFragment:00000133\r\nEndFragment:00000579\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell A4</TD><TD>Cell B4</TD><TD>Cell C4</TD><TD>Cell D4</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#29-2");
				Assert.AreEqual (string.Format ("Row#1\tCell A1\t\t\t{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\t\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#29-3");
				Assert.AreEqual (string.Format ("Row#1\tCell A1\t\t\t{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\t\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#29-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#30-0");
				Assert.AreEqual (string.Format ("Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,,,,{0}Row#4,Cell A4,Cell B4,Cell C4,Cell D4", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#30-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000509\r\nStartFragment:00000133\r\nEndFragment:00000473\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell A4</TD><TD>Cell B4</TD><TD>Cell C4</TD><TD>Cell D4</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#30-2");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\t\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#30-3");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\t\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#30-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [3].Selected = true;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#31-0");
				Assert.AreEqual (string.Format ("Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,,,Cell C3,{0}Row#4,Cell A4,Cell B4,Cell C4,Cell D4", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#31-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000510\r\nStartFragment:00000133\r\nEndFragment:00000474\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>Cell C3</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell A4</TD><TD>Cell B4</TD><TD>Cell C4</TD><TD>Cell D4</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#31-2");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\tCell C3\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#31-3");
				Assert.AreEqual (string.Format ("Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\tCell C3\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#31-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#EnableWithAutoHeaderText#32-0");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#33-0");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#33-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000211\r\nStartFragment:00000133\r\nEndFragment:00000175\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#33-2");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#33-3");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#33-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#34-0");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#34-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000211\r\nStartFragment:00000133\r\nEndFragment:00000175\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell C3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#34-2");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#34-3");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#34-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#35-0");
				Assert.AreEqual (string.Format ("A{0}Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#35-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000329\r\nStartFragment:00000133\r\nEndFragment:00000293\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>A</TH></THEAD><TR><TD>Cell A1</TD></TR><TR><TD>Cell A2</TD></TR><TR><TD>Cell A3</TD></TR><TR><TD>Cell A4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#35-2");
				Assert.AreEqual (string.Format ("A{0}Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#35-3");
				Assert.AreEqual (string.Format ("A{0}Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#35-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#36-0");
				Assert.AreEqual (string.Format ("A{0}Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#36-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000329\r\nStartFragment:00000133\r\nEndFragment:00000293\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>A</TH></THEAD><TR><TD>Cell A1</TD></TR><TR><TD>Cell A2</TD></TR><TR><TD>Cell A3</TD></TR><TR><TD>Cell A4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#36-2");
				Assert.AreEqual (string.Format ("A{0}Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#36-3");
				Assert.AreEqual (string.Format ("A{0}Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#36-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#37-0");
				Assert.AreEqual (string.Format ("A{0}Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#37-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000329\r\nStartFragment:00000133\r\nEndFragment:00000293\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>A</TH></THEAD><TR><TD>Cell A1</TD></TR><TR><TD>Cell A2</TD></TR><TR><TD>Cell A3</TD></TR><TR><TD>Cell A4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#37-2");
				Assert.AreEqual (string.Format ("A{0}Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#37-3");
				Assert.AreEqual (string.Format ("A{0}Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#37-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [1].Cells [0].Selected = true;
				dgv.Rows [2].Cells [0].Selected = true;
				dgv.Rows [2].Cells [2].Selected = true;
				dgv.Rows [3].Cells [0].Selected = true;
				dgv.Rows [4].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#38-0");
				Assert.AreEqual (string.Format ("Cell A2,,{0}Cell A3,,Cell C3{0}Cell A4,,{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#38-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000400\r\nStartFragment:00000133\r\nEndFragment:00000364\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A2</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD>Cell A3</TD><TD>&nbsp;</TD><TD>Cell C3</TD></TR><TR><TD>Cell A4</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD></TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#38-2");
				Assert.AreEqual (string.Format ("Cell A2\t\t{0}Cell A3\t\tCell C3{0}Cell A4\t\t{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#38-3");
				Assert.AreEqual (string.Format ("Cell A2\t\t{0}Cell A3\t\tCell C3{0}Cell A4\t\t{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#38-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#39-0");
				Assert.AreEqual (string.Format ("C{0}Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#39-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000329\r\nStartFragment:00000133\r\nEndFragment:00000293\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>C</TH></THEAD><TR><TD>Cell C1</TD></TR><TR><TD>Cell C2</TD></TR><TR><TD>Cell C3</TD></TR><TR><TD>Cell C4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#39-2");
				Assert.AreEqual (string.Format ("C{0}Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#39-3");
				Assert.AreEqual (string.Format ("C{0}Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#39-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [2].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#40-0");
				Assert.AreEqual (string.Format ("A,B,C{0}Cell A1,,Cell C1{0},,Cell C2{0},,Cell C3{0},,Cell C4{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#40-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000500\r\nStartFragment:00000133\r\nEndFragment:00000464\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>A</TH><TH>B</TH><TH>C</TH></THEAD><TR><TD>Cell A1</TD><TD>&nbsp;</TD><TD>Cell C1</TD></TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>Cell C2</TD></TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>Cell C3</TD></TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>Cell C4</TD></TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#40-2");
				Assert.AreEqual (string.Format ("A\tB\tC{0}Cell A1\t\tCell C1{0}\t\tCell C2{0}\t\tCell C3{0}\t\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#40-3");
				Assert.AreEqual (string.Format ("A\tB\tC{0}Cell A1\t\tCell C1{0}\t\tCell C2{0}\t\tCell C3{0}\t\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#40-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Rows [0].Cells [2].Selected = true;
				dgv.Rows [1].Cells [2].Selected = true;
				dgv.Rows [3].Cells [2].Selected = true;
				dgv.Rows [4].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#41-0");
				Assert.AreEqual (string.Format ("Cell C1{0}Cell C2{0}{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#41-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000303\r\nStartFragment:00000133\r\nEndFragment:00000267\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell C1</TD></TR><TR><TD>Cell C2</TD></TR><TR><TD>&nbsp;</TD></TR><TR><TD>Cell C4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#41-2");
				Assert.AreEqual (string.Format ("Cell C1{0}Cell C2{0}{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#41-3");
				Assert.AreEqual (string.Format ("Cell C1{0}Cell C2{0}{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#41-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#42-0");
				Assert.AreEqual (string.Format ("C{0}Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#42-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000329\r\nStartFragment:00000133\r\nEndFragment:00000293\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>C</TH></THEAD><TR><TD>Cell C1</TD></TR><TR><TD>Cell C2</TD></TR><TR><TD>Cell C3</TD></TR><TR><TD>Cell C4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#42-2");
				Assert.AreEqual (string.Format ("C{0}Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#42-3");
				Assert.AreEqual (string.Format ("C{0}Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#42-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#43-0");
				Assert.AreEqual (string.Format ("B,C{0}Cell B1,Cell C1{0}Cell B2,Cell C2{0}Cell B3,Cell C3{0}Cell B4,Cell C4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#43-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000412\r\nStartFragment:00000133\r\nEndFragment:00000376\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>B</TH><TH>C</TH></THEAD><TR><TD>Cell B1</TD><TD>Cell C1</TD></TR><TR><TD>Cell B2</TD><TD>Cell C2</TD></TR><TR><TD>Cell B3</TD><TD>Cell C3</TD></TR><TR><TD>Cell B4</TD><TD>Cell C4</TD></TR><TR><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#43-2");
				Assert.AreEqual (string.Format ("B\tC{0}Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\tCell C3{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#43-3");
				Assert.AreEqual (string.Format ("B\tC{0}Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\tCell C3{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#43-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [2].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#44-0");
				Assert.AreEqual (string.Format ("A,B,C{0}Cell A1,Cell B1,Cell C1{0},Cell B2,Cell C2{0},Cell B3,Cell C3{0},Cell B4,Cell C4{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#44-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000498\r\nStartFragment:00000133\r\nEndFragment:00000462\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>A</TH><TH>B</TH><TH>C</TH></THEAD><TR><TD>Cell A1</TD><TD>Cell B1</TD><TD>Cell C1</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B2</TD><TD>Cell C2</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B3</TD><TD>Cell C3</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B4</TD><TD>Cell C4</TD></TR><TR><TD>&nbsp;</TD><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#44-2");
				Assert.AreEqual (string.Format ("A\tB\tC{0}Cell A1\tCell B1\tCell C1{0}\tCell B2\tCell C2{0}\tCell B3\tCell C3{0}\tCell B4\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#44-3");
				Assert.AreEqual (string.Format ("A\tB\tC{0}Cell A1\tCell B1\tCell C1{0}\tCell B2\tCell C2{0}\tCell B3\tCell C3{0}\tCell B4\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#44-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Rows [0].Cells [2].Selected = true;
				dgv.Rows [1].Cells [2].Selected = true;
				dgv.Rows [3].Cells [2].Selected = true;
				dgv.Rows [4].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#45-0");
				Assert.AreEqual (string.Format ("B,C{0}Cell B1,Cell C1{0}Cell B2,Cell C2{0}Cell B3,{0}Cell B4,Cell C4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#45-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000411\r\nStartFragment:00000133\r\nEndFragment:00000375\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>B</TH><TH>C</TH></THEAD><TR><TD>Cell B1</TD><TD>Cell C1</TD></TR><TR><TD>Cell B2</TD><TD>Cell C2</TD></TR><TR><TD>Cell B3</TD><TD>&nbsp;</TD></TR><TR><TD>Cell B4</TD><TD>Cell C4</TD></TR><TR><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#45-2");
				Assert.AreEqual (string.Format ("B\tC{0}Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\t{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#45-3");
				Assert.AreEqual (string.Format ("B\tC{0}Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\t{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#45-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#46-0");
				Assert.AreEqual (string.Format ("B,C{0}Cell B1,Cell C1{0}Cell B2,Cell C2{0}Cell B3,Cell C3{0}Cell B4,Cell C4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#46-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000412\r\nStartFragment:00000133\r\nEndFragment:00000376\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>B</TH><TH>C</TH></THEAD><TR><TD>Cell B1</TD><TD>Cell C1</TD></TR><TR><TD>Cell B2</TD><TD>Cell C2</TD></TR><TR><TD>Cell B3</TD><TD>Cell C3</TD></TR><TR><TD>Cell B4</TD><TD>Cell C4</TD></TR><TR><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#46-2");
				Assert.AreEqual (string.Format ("B\tC{0}Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\tCell C3{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#46-3");
				Assert.AreEqual (string.Format ("B\tC{0}Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\tCell C3{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#46-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#47-0");
				Assert.AreEqual (string.Format ("B,C,D{0}Cell B1,,Cell D1{0}Cell B2,,Cell D2{0}Cell B3,,Cell D3{0}Cell B4,,Cell D4{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#47-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000497\r\nStartFragment:00000133\r\nEndFragment:00000461\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD>Cell B1</TD><TD>&nbsp;</TD><TD>Cell D1</TD></TR><TR><TD>Cell B2</TD><TD>&nbsp;</TD><TD>Cell D2</TD></TR><TR><TD>Cell B3</TD><TD>&nbsp;</TD><TD>Cell D3</TD></TR><TR><TD>Cell B4</TD><TD>&nbsp;</TD><TD>Cell D4</TD></TR><TR><TD></TD><TD>&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#47-2");
				Assert.AreEqual (string.Format ("B\tC\tD{0}Cell B1\t\tCell D1{0}Cell B2\t\tCell D2{0}Cell B3\t\tCell D3{0}Cell B4\t\tCell D4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#47-3");
				Assert.AreEqual (string.Format ("B\tC\tD{0}Cell B1\t\tCell D1{0}Cell B2\t\tCell D2{0}Cell B3\t\tCell D3{0}Cell B4\t\tCell D4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#47-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [3].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#48-0");
				Assert.AreEqual (string.Format ("A,B,C,D{0}Cell A1,Cell B1,,Cell D1{0},Cell B2,,Cell D2{0},Cell B3,,Cell D3{0},Cell B4,,Cell D4{0},,,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#48-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000583\r\nStartFragment:00000133\r\nEndFragment:00000547\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD>Cell A1</TD><TD>Cell B1</TD><TD>&nbsp;</TD><TD>Cell D1</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B2</TD><TD>&nbsp;</TD><TD>Cell D2</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B3</TD><TD>&nbsp;</TD><TD>Cell D3</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B4</TD><TD>&nbsp;</TD><TD>Cell D4</TD></TR><TR><TD>&nbsp;</TD><TD></TD><TD>&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#48-2");
				Assert.AreEqual (string.Format ("A\tB\tC\tD{0}Cell A1\tCell B1\t\tCell D1{0}\tCell B2\t\tCell D2{0}\tCell B3\t\tCell D3{0}\tCell B4\t\tCell D4{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#48-3");
				Assert.AreEqual (string.Format ("A\tB\tC\tD{0}Cell A1\tCell B1\t\tCell D1{0}\tCell B2\t\tCell D2{0}\tCell B3\t\tCell D3{0}\tCell B4\t\tCell D4{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#48-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#49-0");
				Assert.AreEqual (string.Format ("B,C,D{0}Cell B1,,Cell D1{0}Cell B2,,Cell D2{0}Cell B3,,Cell D3{0}Cell B4,,Cell D4{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#49-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000497\r\nStartFragment:00000133\r\nEndFragment:00000461\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD>Cell B1</TD><TD>&nbsp;</TD><TD>Cell D1</TD></TR><TR><TD>Cell B2</TD><TD>&nbsp;</TD><TD>Cell D2</TD></TR><TR><TD>Cell B3</TD><TD>&nbsp;</TD><TD>Cell D3</TD></TR><TR><TD>Cell B4</TD><TD>&nbsp;</TD><TD>Cell D4</TD></TR><TR><TD></TD><TD>&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#49-2");
				Assert.AreEqual (string.Format ("B\tC\tD{0}Cell B1\t\tCell D1{0}Cell B2\t\tCell D2{0}Cell B3\t\tCell D3{0}Cell B4\t\tCell D4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#49-3");
				Assert.AreEqual (string.Format ("B\tC\tD{0}Cell B1\t\tCell D1{0}Cell B2\t\tCell D2{0}Cell B3\t\tCell D3{0}Cell B4\t\tCell D4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#49-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [3].Selected = true;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithAutoHeaderText#50-0");
				Assert.AreEqual (string.Format ("B,C,D{0}Cell B1,,Cell D1{0}Cell B2,,Cell D2{0}Cell B3,Cell C3,Cell D3{0}Cell B4,,Cell D4{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithAutoHeaderText#50-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000498\r\nStartFragment:00000133\r\nEndFragment:00000462\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD>Cell B1</TD><TD>&nbsp;</TD><TD>Cell D1</TD></TR><TR><TD>Cell B2</TD><TD>&nbsp;</TD><TD>Cell D2</TD></TR><TR><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR><TR><TD>Cell B4</TD><TD>&nbsp;</TD><TD>Cell D4</TD></TR><TR><TD></TD><TD>&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithAutoHeaderText#50-2");
				Assert.AreEqual (string.Format ("B\tC\tD{0}Cell B1\t\tCell D1{0}Cell B2\t\tCell D2{0}Cell B3\tCell C3\tCell D3{0}Cell B4\t\tCell D4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithAutoHeaderText#50-3");
				Assert.AreEqual (string.Format ("B\tC\tD{0}Cell B1\t\tCell D1{0}Cell B2\t\tCell D2{0}Cell B3\tCell C3\tCell D3{0}Cell B4\t\tCell D4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithAutoHeaderText#50-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#EnableWithoutHeaderText#0-0");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#1-0");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#1-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000211\r\nStartFragment:00000133\r\nEndFragment:00000175\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#1-2");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#1-3");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#1-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#2-0");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#2-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000211\r\nStartFragment:00000133\r\nEndFragment:00000175\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell C3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#2-2");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#2-3");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#2-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#EnableWithoutHeaderText#3-0");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#4-0");
				Assert.AreEqual ("Cell A1,Cell B1,Cell C1,Cell D1", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#4-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000259\r\nStartFragment:00000133\r\nEndFragment:00000223\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD><TD>Cell B1</TD><TD>Cell C1</TD><TD>Cell D1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#4-2");
				Assert.AreEqual ("Cell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#4-3");
				Assert.AreEqual ("Cell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#4-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#5-0");
				Assert.AreEqual ("Cell A3,Cell B3,Cell C3,Cell D3", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#5-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000259\r\nStartFragment:00000133\r\nEndFragment:00000223\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#5-2");
				Assert.AreEqual ("Cell A3\tCell B3\tCell C3\tCell D3", data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#5-3");
				Assert.AreEqual ("Cell A3\tCell B3\tCell C3\tCell D3", data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#5-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#6-0");
				Assert.AreEqual (string.Format ("Cell A2,Cell B2,Cell C2,Cell D2{0}Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#6-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000332\r\nStartFragment:00000133\r\nEndFragment:00000296\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#6-2");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}Cell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#6-3");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}Cell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#6-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#7-0");
				Assert.AreEqual (string.Format ("Cell A2,Cell B2,Cell C2,Cell D2{0}Cell A4,Cell B4,Cell C4,Cell D4", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#7-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000332\r\nStartFragment:00000133\r\nEndFragment:00000296\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD>Cell A4</TD><TD>Cell B4</TD><TD>Cell C4</TD><TD>Cell D4</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#7-2");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}Cell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#7-3");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}Cell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#7-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#EnableWithoutHeaderText#8-0");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#9-0");
				Assert.AreEqual (string.Format ("Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#9-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000304\r\nStartFragment:00000133\r\nEndFragment:00000268\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD></TR><TR><TD>Cell A2</TD></TR><TR><TD>Cell A3</TD></TR><TR><TD>Cell A4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#9-2");
				Assert.AreEqual (string.Format ("Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#9-3");
				Assert.AreEqual (string.Format ("Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#9-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#10-0");
				Assert.AreEqual (string.Format ("Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#10-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000304\r\nStartFragment:00000133\r\nEndFragment:00000268\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell C1</TD></TR><TR><TD>Cell C2</TD></TR><TR><TD>Cell C3</TD></TR><TR><TD>Cell C4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#10-2");
				Assert.AreEqual (string.Format ("Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#10-3");
				Assert.AreEqual (string.Format ("Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#10-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#11-0");
				Assert.AreEqual (string.Format ("Cell B1,Cell C1{0}Cell B2,Cell C2{0}Cell B3,Cell C3{0}Cell B4,Cell C4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#11-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000377\r\nStartFragment:00000133\r\nEndFragment:00000341\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell B1</TD><TD>Cell C1</TD></TR><TR><TD>Cell B2</TD><TD>Cell C2</TD></TR><TR><TD>Cell B3</TD><TD>Cell C3</TD></TR><TR><TD>Cell B4</TD><TD>Cell C4</TD></TR><TR><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#11-2");
				Assert.AreEqual (string.Format ("Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\tCell C3{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#11-3");
				Assert.AreEqual (string.Format ("Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\tCell C3{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#11-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#12-0");
				Assert.AreEqual (string.Format ("Cell B1,Cell D1{0}Cell B2,Cell D2{0}Cell B3,Cell D3{0}Cell B4,Cell D4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#12-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000377\r\nStartFragment:00000133\r\nEndFragment:00000341\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell B1</TD><TD>Cell D1</TD></TR><TR><TD>Cell B2</TD><TD>Cell D2</TD></TR><TR><TD>Cell B3</TD><TD>Cell D3</TD></TR><TR><TD>Cell B4</TD><TD>Cell D4</TD></TR><TR><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#12-2");
				Assert.AreEqual (string.Format ("Cell B1\tCell D1{0}Cell B2\tCell D2{0}Cell B3\tCell D3{0}Cell B4\tCell D4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#12-3");
				Assert.AreEqual (string.Format ("Cell B1\tCell D1{0}Cell B2\tCell D2{0}Cell B3\tCell D3{0}Cell B4\tCell D4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#12-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#EnableWithoutHeaderText#13-0");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#14-0");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#14-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000211\r\nStartFragment:00000133\r\nEndFragment:00000175\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#14-2");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#14-3");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#14-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#15-0");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#15-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000211\r\nStartFragment:00000133\r\nEndFragment:00000175\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell C3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#15-2");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#15-3");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#15-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#16-0");
				Assert.AreEqual ("Cell A1,Cell B1,Cell C1,Cell D1", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#16-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000259\r\nStartFragment:00000133\r\nEndFragment:00000223\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD><TD>Cell B1</TD><TD>Cell C1</TD><TD>Cell D1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#16-2");
				Assert.AreEqual ("Cell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#16-3");
				Assert.AreEqual ("Cell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#16-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#17-0");
				Assert.AreEqual ("Cell A1,Cell B1,Cell C1,Cell D1", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#17-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000259\r\nStartFragment:00000133\r\nEndFragment:00000223\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD><TD>Cell B1</TD><TD>Cell C1</TD><TD>Cell D1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#17-2");
				Assert.AreEqual ("Cell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#17-3");
				Assert.AreEqual ("Cell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#17-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#18-0");
				Assert.AreEqual ("Cell A1,Cell B1,Cell C1,Cell D1", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#18-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000259\r\nStartFragment:00000133\r\nEndFragment:00000223\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD><TD>Cell B1</TD><TD>Cell C1</TD><TD>Cell D1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#18-2");
				Assert.AreEqual ("Cell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#18-3");
				Assert.AreEqual ("Cell A1\tCell B1\tCell C1\tCell D1", data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#18-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [0].Cells [1].Selected = true;
				dgv.Rows [0].Cells [2].Selected = true;
				dgv.Rows [0].Cells [3].Selected = true;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#19-0");
				Assert.AreEqual (string.Format ("Cell B1,Cell C1,Cell D1{0},,{0},Cell C3,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#19-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000352\r\nStartFragment:00000133\r\nEndFragment:00000316\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell B1</TD><TD>Cell C1</TD><TD>Cell D1</TD></TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD>&nbsp;</TD><TD>Cell C3</TD><TD>&nbsp;</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#19-2");
				Assert.AreEqual (string.Format ("Cell B1\tCell C1\tCell D1{0}\t\t{0}\tCell C3\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#19-3");
				Assert.AreEqual (string.Format ("Cell B1\tCell C1\tCell D1{0}\t\t{0}\tCell C3\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#19-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#20-0");
				Assert.AreEqual ("Cell A3,Cell B3,Cell C3,Cell D3", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#20-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000259\r\nStartFragment:00000133\r\nEndFragment:00000223\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#20-2");
				Assert.AreEqual ("Cell A3\tCell B3\tCell C3\tCell D3", data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#20-3");
				Assert.AreEqual ("Cell A3\tCell B3\tCell C3\tCell D3", data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#20-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [2].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#21-0");
				Assert.AreEqual (string.Format ("Cell A1,,,{0},,,{0}Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#21-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000398\r\nStartFragment:00000133\r\nEndFragment:00000362\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#21-2");
				Assert.AreEqual (string.Format ("Cell A1\t\t\t{0}\t\t\t{0}Cell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#21-3");
				Assert.AreEqual (string.Format ("Cell A1\t\t\t{0}\t\t\t{0}Cell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#21-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [2].Cells [0].Selected = true;
				dgv.Rows [2].Cells [1].Selected = true;
				dgv.Rows [2].Cells [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#22-0");
				Assert.AreEqual ("Cell A3,Cell B3,,Cell D3", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#22-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000258\r\nStartFragment:00000133\r\nEndFragment:00000222\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A3</TD><TD>Cell B3</TD><TD>&nbsp;</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#22-2");
				Assert.AreEqual ("Cell A3\tCell B3\t\tCell D3", data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#22-3");
				Assert.AreEqual ("Cell A3\tCell B3\t\tCell D3", data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#22-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#23-0");
				Assert.AreEqual ("Cell A3,Cell B3,Cell C3,Cell D3", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#23-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000259\r\nStartFragment:00000133\r\nEndFragment:00000223\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#23-2");
				Assert.AreEqual ("Cell A3\tCell B3\tCell C3\tCell D3", data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#23-3");
				Assert.AreEqual ("Cell A3\tCell B3\tCell C3\tCell D3", data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#23-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#24-0");
				Assert.AreEqual (string.Format ("Cell A2,Cell B2,Cell C2,Cell D2{0}Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#24-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000332\r\nStartFragment:00000133\r\nEndFragment:00000296\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#24-2");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}Cell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#24-3");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}Cell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#24-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [2].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#25-0");
				Assert.AreEqual (string.Format ("Cell A1,,,{0}Cell A2,Cell B2,Cell C2,Cell D2{0}Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#25-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000402\r\nStartFragment:00000133\r\nEndFragment:00000366\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#25-2");
				Assert.AreEqual (string.Format ("Cell A1\t\t\t{0}Cell A2\tCell B2\tCell C2\tCell D2{0}Cell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#25-3");
				Assert.AreEqual (string.Format ("Cell A1\t\t\t{0}Cell A2\tCell B2\tCell C2\tCell D2{0}Cell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#25-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [2].Cells [0].Selected = true;
				dgv.Rows [2].Cells [1].Selected = true;
				dgv.Rows [2].Cells [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#26-0");
				Assert.AreEqual (string.Format ("Cell A2,Cell B2,Cell C2,Cell D2{0}Cell A3,Cell B3,,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#26-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000331\r\nStartFragment:00000133\r\nEndFragment:00000295\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD>Cell A3</TD><TD>Cell B3</TD><TD>&nbsp;</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#26-2");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}Cell A3\tCell B3\t\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#26-3");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}Cell A3\tCell B3\t\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#26-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#27-0");
				Assert.AreEqual (string.Format ("Cell A2,Cell B2,Cell C2,Cell D2{0}Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#27-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000332\r\nStartFragment:00000133\r\nEndFragment:00000296\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#27-2");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}Cell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#27-3");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}Cell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#27-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#28-0");
				Assert.AreEqual (string.Format ("Cell A2,Cell B2,Cell C2,Cell D2{0},,,{0}Cell A4,Cell B4,Cell C4,Cell D4", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#28-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000401\r\nStartFragment:00000133\r\nEndFragment:00000365\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD>Cell A4</TD><TD>Cell B4</TD><TD>Cell C4</TD><TD>Cell D4</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#28-2");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}\t\t\t{0}Cell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#28-3");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}\t\t\t{0}Cell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#28-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [3].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#29-0");
				Assert.AreEqual (string.Format ("Cell A1,,,{0}Cell A2,Cell B2,Cell C2,Cell D2{0},,,{0}Cell A4,Cell B4,Cell C4,Cell D4", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#29-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000471\r\nStartFragment:00000133\r\nEndFragment:00000435\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD>Cell A4</TD><TD>Cell B4</TD><TD>Cell C4</TD><TD>Cell D4</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#29-2");
				Assert.AreEqual (string.Format ("Cell A1\t\t\t{0}Cell A2\tCell B2\tCell C2\tCell D2{0}\t\t\t{0}Cell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#29-3");
				Assert.AreEqual (string.Format ("Cell A1\t\t\t{0}Cell A2\tCell B2\tCell C2\tCell D2{0}\t\t\t{0}Cell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#29-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#30-0");
				Assert.AreEqual (string.Format ("Cell A2,Cell B2,Cell C2,Cell D2{0},,,{0}Cell A4,Cell B4,Cell C4,Cell D4", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#30-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000401\r\nStartFragment:00000133\r\nEndFragment:00000365\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD>Cell A4</TD><TD>Cell B4</TD><TD>Cell C4</TD><TD>Cell D4</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#30-2");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}\t\t\t{0}Cell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#30-3");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}\t\t\t{0}Cell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#30-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [3].Selected = true;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#31-0");
				Assert.AreEqual (string.Format ("Cell A2,Cell B2,Cell C2,Cell D2{0},,Cell C3,{0}Cell A4,Cell B4,Cell C4,Cell D4", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#31-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000402\r\nStartFragment:00000133\r\nEndFragment:00000366\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>Cell C3</TD><TD>&nbsp;</TD></TR><TR><TD>Cell A4</TD><TD>Cell B4</TD><TD>Cell C4</TD><TD>Cell D4</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#31-2");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}\t\tCell C3\t{0}Cell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#31-3");
				Assert.AreEqual (string.Format ("Cell A2\tCell B2\tCell C2\tCell D2{0}\t\tCell C3\t{0}Cell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#31-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#EnableWithoutHeaderText#32-0");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#33-0");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#33-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000211\r\nStartFragment:00000133\r\nEndFragment:00000175\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#33-2");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#33-3");
				Assert.AreEqual ("Cell A1", data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#33-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#34-0");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#34-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000211\r\nStartFragment:00000133\r\nEndFragment:00000175\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell C3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#34-2");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#34-3");
				Assert.AreEqual ("Cell C3", data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#34-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#35-0");
				Assert.AreEqual (string.Format ("Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#35-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000304\r\nStartFragment:00000133\r\nEndFragment:00000268\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD></TR><TR><TD>Cell A2</TD></TR><TR><TD>Cell A3</TD></TR><TR><TD>Cell A4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#35-2");
				Assert.AreEqual (string.Format ("Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#35-3");
				Assert.AreEqual (string.Format ("Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#35-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#36-0");
				Assert.AreEqual (string.Format ("Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#36-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000304\r\nStartFragment:00000133\r\nEndFragment:00000268\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD></TR><TR><TD>Cell A2</TD></TR><TR><TD>Cell A3</TD></TR><TR><TD>Cell A4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#36-2");
				Assert.AreEqual (string.Format ("Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#36-3");
				Assert.AreEqual (string.Format ("Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#36-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#37-0");
				Assert.AreEqual (string.Format ("Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#37-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000304\r\nStartFragment:00000133\r\nEndFragment:00000268\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD></TR><TR><TD>Cell A2</TD></TR><TR><TD>Cell A3</TD></TR><TR><TD>Cell A4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#37-2");
				Assert.AreEqual (string.Format ("Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#37-3");
				Assert.AreEqual (string.Format ("Cell A1{0}Cell A2{0}Cell A3{0}Cell A4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#37-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [1].Cells [0].Selected = true;
				dgv.Rows [2].Cells [0].Selected = true;
				dgv.Rows [2].Cells [2].Selected = true;
				dgv.Rows [3].Cells [0].Selected = true;
				dgv.Rows [4].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#38-0");
				Assert.AreEqual (string.Format ("Cell A2,,{0}Cell A3,,Cell C3{0}Cell A4,,{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#38-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000400\r\nStartFragment:00000133\r\nEndFragment:00000364\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A2</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD>Cell A3</TD><TD>&nbsp;</TD><TD>Cell C3</TD></TR><TR><TD>Cell A4</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD></TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#38-2");
				Assert.AreEqual (string.Format ("Cell A2\t\t{0}Cell A3\t\tCell C3{0}Cell A4\t\t{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#38-3");
				Assert.AreEqual (string.Format ("Cell A2\t\t{0}Cell A3\t\tCell C3{0}Cell A4\t\t{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#38-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#39-0");
				Assert.AreEqual (string.Format ("Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#39-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000304\r\nStartFragment:00000133\r\nEndFragment:00000268\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell C1</TD></TR><TR><TD>Cell C2</TD></TR><TR><TD>Cell C3</TD></TR><TR><TD>Cell C4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#39-2");
				Assert.AreEqual (string.Format ("Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#39-3");
				Assert.AreEqual (string.Format ("Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#39-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [2].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#40-0");
				Assert.AreEqual (string.Format ("Cell A1,,Cell C1{0},,Cell C2{0},,Cell C3{0},,Cell C4{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#40-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000455\r\nStartFragment:00000133\r\nEndFragment:00000419\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD><TD>&nbsp;</TD><TD>Cell C1</TD></TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>Cell C2</TD></TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>Cell C3</TD></TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>Cell C4</TD></TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#40-2");
				Assert.AreEqual (string.Format ("Cell A1\t\tCell C1{0}\t\tCell C2{0}\t\tCell C3{0}\t\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#40-3");
				Assert.AreEqual (string.Format ("Cell A1\t\tCell C1{0}\t\tCell C2{0}\t\tCell C3{0}\t\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#40-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Rows [0].Cells [2].Selected = true;
				dgv.Rows [1].Cells [2].Selected = true;
				dgv.Rows [3].Cells [2].Selected = true;
				dgv.Rows [4].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#41-0");
				Assert.AreEqual (string.Format ("Cell C1{0}Cell C2{0}{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#41-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000303\r\nStartFragment:00000133\r\nEndFragment:00000267\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell C1</TD></TR><TR><TD>Cell C2</TD></TR><TR><TD>&nbsp;</TD></TR><TR><TD>Cell C4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#41-2");
				Assert.AreEqual (string.Format ("Cell C1{0}Cell C2{0}{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#41-3");
				Assert.AreEqual (string.Format ("Cell C1{0}Cell C2{0}{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#41-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#42-0");
				Assert.AreEqual (string.Format ("Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#42-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000304\r\nStartFragment:00000133\r\nEndFragment:00000268\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell C1</TD></TR><TR><TD>Cell C2</TD></TR><TR><TD>Cell C3</TD></TR><TR><TD>Cell C4</TD></TR><TR><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#42-2");
				Assert.AreEqual (string.Format ("Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#42-3");
				Assert.AreEqual (string.Format ("Cell C1{0}Cell C2{0}Cell C3{0}Cell C4{0}", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#42-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#43-0");
				Assert.AreEqual (string.Format ("Cell B1,Cell C1{0}Cell B2,Cell C2{0}Cell B3,Cell C3{0}Cell B4,Cell C4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#43-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000377\r\nStartFragment:00000133\r\nEndFragment:00000341\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell B1</TD><TD>Cell C1</TD></TR><TR><TD>Cell B2</TD><TD>Cell C2</TD></TR><TR><TD>Cell B3</TD><TD>Cell C3</TD></TR><TR><TD>Cell B4</TD><TD>Cell C4</TD></TR><TR><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#43-2");
				Assert.AreEqual (string.Format ("Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\tCell C3{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#43-3");
				Assert.AreEqual (string.Format ("Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\tCell C3{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#43-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [2].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#44-0");
				Assert.AreEqual (string.Format ("Cell A1,Cell B1,Cell C1{0},Cell B2,Cell C2{0},Cell B3,Cell C3{0},Cell B4,Cell C4{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#44-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000453\r\nStartFragment:00000133\r\nEndFragment:00000417\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD><TD>Cell B1</TD><TD>Cell C1</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B2</TD><TD>Cell C2</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B3</TD><TD>Cell C3</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B4</TD><TD>Cell C4</TD></TR><TR><TD>&nbsp;</TD><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#44-2");
				Assert.AreEqual (string.Format ("Cell A1\tCell B1\tCell C1{0}\tCell B2\tCell C2{0}\tCell B3\tCell C3{0}\tCell B4\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#44-3");
				Assert.AreEqual (string.Format ("Cell A1\tCell B1\tCell C1{0}\tCell B2\tCell C2{0}\tCell B3\tCell C3{0}\tCell B4\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#44-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Rows [0].Cells [2].Selected = true;
				dgv.Rows [1].Cells [2].Selected = true;
				dgv.Rows [3].Cells [2].Selected = true;
				dgv.Rows [4].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#45-0");
				Assert.AreEqual (string.Format ("Cell B1,Cell C1{0}Cell B2,Cell C2{0}Cell B3,{0}Cell B4,Cell C4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#45-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000376\r\nStartFragment:00000133\r\nEndFragment:00000340\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell B1</TD><TD>Cell C1</TD></TR><TR><TD>Cell B2</TD><TD>Cell C2</TD></TR><TR><TD>Cell B3</TD><TD>&nbsp;</TD></TR><TR><TD>Cell B4</TD><TD>Cell C4</TD></TR><TR><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#45-2");
				Assert.AreEqual (string.Format ("Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\t{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#45-3");
				Assert.AreEqual (string.Format ("Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\t{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#45-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#46-0");
				Assert.AreEqual (string.Format ("Cell B1,Cell C1{0}Cell B2,Cell C2{0}Cell B3,Cell C3{0}Cell B4,Cell C4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#46-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000377\r\nStartFragment:00000133\r\nEndFragment:00000341\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell B1</TD><TD>Cell C1</TD></TR><TR><TD>Cell B2</TD><TD>Cell C2</TD></TR><TR><TD>Cell B3</TD><TD>Cell C3</TD></TR><TR><TD>Cell B4</TD><TD>Cell C4</TD></TR><TR><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#46-2");
				Assert.AreEqual (string.Format ("Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\tCell C3{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#46-3");
				Assert.AreEqual (string.Format ("Cell B1\tCell C1{0}Cell B2\tCell C2{0}Cell B3\tCell C3{0}Cell B4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#46-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#47-0");
				Assert.AreEqual (string.Format ("Cell B1,,Cell D1{0}Cell B2,,Cell D2{0}Cell B3,,Cell D3{0}Cell B4,,Cell D4{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#47-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000452\r\nStartFragment:00000133\r\nEndFragment:00000416\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell B1</TD><TD>&nbsp;</TD><TD>Cell D1</TD></TR><TR><TD>Cell B2</TD><TD>&nbsp;</TD><TD>Cell D2</TD></TR><TR><TD>Cell B3</TD><TD>&nbsp;</TD><TD>Cell D3</TD></TR><TR><TD>Cell B4</TD><TD>&nbsp;</TD><TD>Cell D4</TD></TR><TR><TD></TD><TD>&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#47-2");
				Assert.AreEqual (string.Format ("Cell B1\t\tCell D1{0}Cell B2\t\tCell D2{0}Cell B3\t\tCell D3{0}Cell B4\t\tCell D4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#47-3");
				Assert.AreEqual (string.Format ("Cell B1\t\tCell D1{0}Cell B2\t\tCell D2{0}Cell B3\t\tCell D3{0}Cell B4\t\tCell D4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#47-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [3].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#48-0");
				Assert.AreEqual (string.Format ("Cell A1,Cell B1,,Cell D1{0},Cell B2,,Cell D2{0},Cell B3,,Cell D3{0},Cell B4,,Cell D4{0},,,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#48-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000528\r\nStartFragment:00000133\r\nEndFragment:00000492\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell A1</TD><TD>Cell B1</TD><TD>&nbsp;</TD><TD>Cell D1</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B2</TD><TD>&nbsp;</TD><TD>Cell D2</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B3</TD><TD>&nbsp;</TD><TD>Cell D3</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B4</TD><TD>&nbsp;</TD><TD>Cell D4</TD></TR><TR><TD>&nbsp;</TD><TD></TD><TD>&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#48-2");
				Assert.AreEqual (string.Format ("Cell A1\tCell B1\t\tCell D1{0}\tCell B2\t\tCell D2{0}\tCell B3\t\tCell D3{0}\tCell B4\t\tCell D4{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#48-3");
				Assert.AreEqual (string.Format ("Cell A1\tCell B1\t\tCell D1{0}\tCell B2\t\tCell D2{0}\tCell B3\t\tCell D3{0}\tCell B4\t\tCell D4{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#48-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#49-0");
				Assert.AreEqual (string.Format ("Cell B1,,Cell D1{0}Cell B2,,Cell D2{0}Cell B3,,Cell D3{0}Cell B4,,Cell D4{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#49-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000452\r\nStartFragment:00000133\r\nEndFragment:00000416\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell B1</TD><TD>&nbsp;</TD><TD>Cell D1</TD></TR><TR><TD>Cell B2</TD><TD>&nbsp;</TD><TD>Cell D2</TD></TR><TR><TD>Cell B3</TD><TD>&nbsp;</TD><TD>Cell D3</TD></TR><TR><TD>Cell B4</TD><TD>&nbsp;</TD><TD>Cell D4</TD></TR><TR><TD></TD><TD>&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#49-2");
				Assert.AreEqual (string.Format ("Cell B1\t\tCell D1{0}Cell B2\t\tCell D2{0}Cell B3\t\tCell D3{0}Cell B4\t\tCell D4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#49-3");
				Assert.AreEqual (string.Format ("Cell B1\t\tCell D1{0}Cell B2\t\tCell D2{0}Cell B3\t\tCell D3{0}Cell B4\t\tCell D4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#49-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [3].Selected = true;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableWithoutHeaderText#50-0");
				Assert.AreEqual (string.Format ("Cell B1,,Cell D1{0}Cell B2,,Cell D2{0}Cell B3,Cell C3,Cell D3{0}Cell B4,,Cell D4{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableWithoutHeaderText#50-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000453\r\nStartFragment:00000133\r\nEndFragment:00000417\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><TR><TD>Cell B1</TD><TD>&nbsp;</TD><TD>Cell D1</TD></TR><TR><TD>Cell B2</TD><TD>&nbsp;</TD><TD>Cell D2</TD></TR><TR><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR><TR><TD>Cell B4</TD><TD>&nbsp;</TD><TD>Cell D4</TD></TR><TR><TD></TD><TD>&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableWithoutHeaderText#50-2");
				Assert.AreEqual (string.Format ("Cell B1\t\tCell D1{0}Cell B2\t\tCell D2{0}Cell B3\tCell C3\tCell D3{0}Cell B4\t\tCell D4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableWithoutHeaderText#50-3");
				Assert.AreEqual (string.Format ("Cell B1\t\tCell D1{0}Cell B2\t\tCell D2{0}Cell B3\tCell C3\tCell D3{0}Cell B4\t\tCell D4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableWithoutHeaderText#50-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#EnableAlwaysIncludeHeaderText#0-0");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#1-0");
				Assert.AreEqual (string.Format (",A{0}Row#1,Cell A1", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#1-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000287\r\nStartFragment:00000133\r\nEndFragment:00000251\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#1-2");
				Assert.AreEqual (string.Format ("\tA{0}Row#1\tCell A1", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#1-3");
				Assert.AreEqual (string.Format ("\tA{0}Row#1\tCell A1", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#1-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#2-0");
				Assert.AreEqual (string.Format (",C{0}Row#3,Cell C3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#2-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000287\r\nStartFragment:00000133\r\nEndFragment:00000251\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>C</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell C3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#2-2");
				Assert.AreEqual (string.Format ("\tC{0}Row#3\tCell C3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#2-3");
				Assert.AreEqual (string.Format ("\tC{0}Row#3\tCell C3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#2-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#EnableAlwaysIncludeHeaderText#3-0");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#4-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#1,Cell A1,Cell B1,Cell C1,Cell D1", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#4-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000365\r\nStartFragment:00000133\r\nEndFragment:00000329\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>Cell B1</TD><TD>Cell C1</TD><TD>Cell D1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#4-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\tCell B1\tCell C1\tCell D1", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#4-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\tCell B1\tCell C1\tCell D1", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#4-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#5-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#3,Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#5-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000365\r\nStartFragment:00000133\r\nEndFragment:00000329\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#5-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#5-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#5-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#6-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#6-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000474\r\nStartFragment:00000133\r\nEndFragment:00000438\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#6-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#6-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#6-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#7-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#4,Cell A4,Cell B4,Cell C4,Cell D4", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#7-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000474\r\nStartFragment:00000133\r\nEndFragment:00000438\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell A4</TD><TD>Cell B4</TD><TD>Cell C4</TD><TD>Cell D4</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#7-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#7-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#7-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#EnableAlwaysIncludeHeaderText#8-0");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#9-0");
				Assert.AreEqual (string.Format (",A{0}Row#1,Cell A1{0}Row#2,Cell A2{0}Row#3,Cell A3{0}Row#4,Cell A4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#9-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000518\r\nStartFragment:00000133\r\nEndFragment:00000482\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell A4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#9-2");
				Assert.AreEqual (string.Format ("\tA{0}Row#1\tCell A1{0}Row#2\tCell A2{0}Row#3\tCell A3{0}Row#4\tCell A4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#9-3");
				Assert.AreEqual (string.Format ("\tA{0}Row#1\tCell A1{0}Row#2\tCell A2{0}Row#3\tCell A3{0}Row#4\tCell A4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#9-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#10-0");
				Assert.AreEqual (string.Format (",C{0}Row#1,Cell C1{0}Row#2,Cell C2{0}Row#3,Cell C3{0}Row#4,Cell C4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#10-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000518\r\nStartFragment:00000133\r\nEndFragment:00000482\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>C</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell C1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell C2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell C3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell C4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#10-2");
				Assert.AreEqual (string.Format ("\tC{0}Row#1\tCell C1{0}Row#2\tCell C2{0}Row#3\tCell C3{0}Row#4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#10-3");
				Assert.AreEqual (string.Format ("\tC{0}Row#1\tCell C1{0}Row#2\tCell C2{0}Row#3\tCell C3{0}Row#4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#10-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#11-0");
				Assert.AreEqual (string.Format (",B,C{0}Row#1,Cell B1,Cell C1{0}Row#2,Cell B2,Cell C2{0}Row#3,Cell B3,Cell C3{0}Row#4,Cell B4,Cell C4{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#11-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000601\r\nStartFragment:00000133\r\nEndFragment:00000565\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>B</TH><TH>C</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell B1</TD><TD>Cell C1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell B2</TD><TD>Cell C2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell B3</TD><TD>Cell C3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell B4</TD><TD>Cell C4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#11-2");
				Assert.AreEqual (string.Format ("\tB\tC{0}Row#1\tCell B1\tCell C1{0}Row#2\tCell B2\tCell C2{0}Row#3\tCell B3\tCell C3{0}Row#4\tCell B4\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#11-3");
				Assert.AreEqual (string.Format ("\tB\tC{0}Row#1\tCell B1\tCell C1{0}Row#2\tCell B2\tCell C2{0}Row#3\tCell B3\tCell C3{0}Row#4\tCell B4\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#11-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#12-0");
				Assert.AreEqual (string.Format (",B,D{0}Row#1,Cell B1,Cell D1{0}Row#2,Cell B2,Cell D2{0}Row#3,Cell B3,Cell D3{0}Row#4,Cell B4,Cell D4{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#12-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000601\r\nStartFragment:00000133\r\nEndFragment:00000565\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>B</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell B1</TD><TD>Cell D1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell B2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell B3</TD><TD>Cell D3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell B4</TD><TD>Cell D4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#12-2");
				Assert.AreEqual (string.Format ("\tB\tD{0}Row#1\tCell B1\tCell D1{0}Row#2\tCell B2\tCell D2{0}Row#3\tCell B3\tCell D3{0}Row#4\tCell B4\tCell D4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#12-3");
				Assert.AreEqual (string.Format ("\tB\tD{0}Row#1\tCell B1\tCell D1{0}Row#2\tCell B2\tCell D2{0}Row#3\tCell B3\tCell D3{0}Row#4\tCell B4\tCell D4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#12-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#EnableAlwaysIncludeHeaderText#13-0");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#14-0");
				Assert.AreEqual (string.Format (",A{0}Row#1,Cell A1", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#14-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000287\r\nStartFragment:00000133\r\nEndFragment:00000251\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#14-2");
				Assert.AreEqual (string.Format ("\tA{0}Row#1\tCell A1", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#14-3");
				Assert.AreEqual (string.Format ("\tA{0}Row#1\tCell A1", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#14-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#15-0");
				Assert.AreEqual (string.Format (",C{0}Row#3,Cell C3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#15-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000287\r\nStartFragment:00000133\r\nEndFragment:00000251\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>C</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell C3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#15-2");
				Assert.AreEqual (string.Format ("\tC{0}Row#3\tCell C3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#15-3");
				Assert.AreEqual (string.Format ("\tC{0}Row#3\tCell C3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#15-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#16-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#1,Cell A1,Cell B1,Cell C1,Cell D1", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#16-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000365\r\nStartFragment:00000133\r\nEndFragment:00000329\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>Cell B1</TD><TD>Cell C1</TD><TD>Cell D1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#16-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\tCell B1\tCell C1\tCell D1", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#16-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\tCell B1\tCell C1\tCell D1", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#16-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#17-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#1,Cell A1,Cell B1,Cell C1,Cell D1", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#17-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000365\r\nStartFragment:00000133\r\nEndFragment:00000329\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>Cell B1</TD><TD>Cell C1</TD><TD>Cell D1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#17-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\tCell B1\tCell C1\tCell D1", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#17-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\tCell B1\tCell C1\tCell D1", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#17-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#18-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#1,Cell A1,Cell B1,Cell C1,Cell D1", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#18-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000365\r\nStartFragment:00000133\r\nEndFragment:00000329\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>Cell B1</TD><TD>Cell C1</TD><TD>Cell D1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#18-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\tCell B1\tCell C1\tCell D1", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#18-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\tCell B1\tCell C1\tCell D1", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#18-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [0].Cells [1].Selected = true;
				dgv.Rows [0].Cells [2].Selected = true;
				dgv.Rows [0].Cells [3].Selected = true;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#19-0");
				Assert.AreEqual (string.Format (",B,C,D{0}Row#1,Cell B1,Cell C1,Cell D1{0}Row#2,,,{0}Row#3,,Cell C3,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#19-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000520\r\nStartFragment:00000133\r\nEndFragment:00000484\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell B1</TD><TD>Cell C1</TD><TD>Cell D1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>&nbsp;</TD><TD>Cell C3</TD><TD>&nbsp;</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#19-2");
				Assert.AreEqual (string.Format ("\tB\tC\tD{0}Row#1\tCell B1\tCell C1\tCell D1{0}Row#2\t\t\t{0}Row#3\t\tCell C3\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#19-3");
				Assert.AreEqual (string.Format ("\tB\tC\tD{0}Row#1\tCell B1\tCell C1\tCell D1{0}Row#2\t\t\t{0}Row#3\t\tCell C3\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#19-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#20-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#3,Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#20-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000365\r\nStartFragment:00000133\r\nEndFragment:00000329\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#20-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#20-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#20-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [2].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#21-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#1,Cell A1,,,{0}Row#2,,,,{0}Row#3,Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#21-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000576\r\nStartFragment:00000133\r\nEndFragment:00000540\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#21-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\t\t\t{0}Row#2\t\t\t\t{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#21-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\t\t\t{0}Row#2\t\t\t\t{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#21-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [2].Cells [0].Selected = true;
				dgv.Rows [2].Cells [1].Selected = true;
				dgv.Rows [2].Cells [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#22-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#3,Cell A3,Cell B3,,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#22-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000364\r\nStartFragment:00000133\r\nEndFragment:00000328\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>&nbsp;</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#22-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#3\tCell A3\tCell B3\t\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#22-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#3\tCell A3\tCell B3\t\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#22-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#23-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#3,Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#23-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000365\r\nStartFragment:00000133\r\nEndFragment:00000329\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#23-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#23-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#23-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#24-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#24-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000474\r\nStartFragment:00000133\r\nEndFragment:00000438\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#24-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#24-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#24-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [2].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#25-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#1,Cell A1,,,{0}Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#25-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000580\r\nStartFragment:00000133\r\nEndFragment:00000544\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#25-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\t\t\t{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#25-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\t\t\t{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#25-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [2].Cells [0].Selected = true;
				dgv.Rows [2].Cells [1].Selected = true;
				dgv.Rows [2].Cells [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#26-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,Cell A3,Cell B3,,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#26-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000473\r\nStartFragment:00000133\r\nEndFragment:00000437\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>&nbsp;</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#26-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\t\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#26-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\t\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#26-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#27-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,Cell A3,Cell B3,Cell C3,Cell D3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#27-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000474\r\nStartFragment:00000133\r\nEndFragment:00000438\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#27-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#27-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\tCell A3\tCell B3\tCell C3\tCell D3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#27-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#28-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,,,,{0}Row#4,Cell A4,Cell B4,Cell C4,Cell D4", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#28-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000579\r\nStartFragment:00000133\r\nEndFragment:00000543\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell A4</TD><TD>Cell B4</TD><TD>Cell C4</TD><TD>Cell D4</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#28-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\t\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#28-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\t\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#28-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [3].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#29-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#1,Cell A1,,,{0}Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,,,,{0}Row#4,Cell A4,Cell B4,Cell C4,Cell D4", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#29-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000685\r\nStartFragment:00000133\r\nEndFragment:00000649\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell A4</TD><TD>Cell B4</TD><TD>Cell C4</TD><TD>Cell D4</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#29-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\t\t\t{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\t\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#29-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\t\t\t{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\t\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#29-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#30-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,,,,{0}Row#4,Cell A4,Cell B4,Cell C4,Cell D4", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#30-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000579\r\nStartFragment:00000133\r\nEndFragment:00000543\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell A4</TD><TD>Cell B4</TD><TD>Cell C4</TD><TD>Cell D4</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#30-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\t\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#30-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\t\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#30-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [1].Selected = true;
				dgv.Rows [3].Selected = true;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#31-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#2,Cell A2,Cell B2,Cell C2,Cell D2{0}Row#3,,,Cell C3,{0}Row#4,Cell A4,Cell B4,Cell C4,Cell D4", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#31-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000580\r\nStartFragment:00000133\r\nEndFragment:00000544\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>Cell B2</TD><TD>Cell C2</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>Cell C3</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell A4</TD><TD>Cell B4</TD><TD>Cell C4</TD><TD>Cell D4</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#31-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\tCell C3\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#31-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#2\tCell A2\tCell B2\tCell C2\tCell D2{0}Row#3\t\t\tCell C3\t{0}Row#4\tCell A4\tCell B4\tCell C4\tCell D4", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#31-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#EnableAlwaysIncludeHeaderText#32-0");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#33-0");
				Assert.AreEqual (string.Format (",A{0}Row#1,Cell A1", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#33-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000287\r\nStartFragment:00000133\r\nEndFragment:00000251\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#33-2");
				Assert.AreEqual (string.Format ("\tA{0}Row#1\tCell A1", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#33-3");
				Assert.AreEqual (string.Format ("\tA{0}Row#1\tCell A1", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#33-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#34-0");
				Assert.AreEqual (string.Format (",C{0}Row#3,Cell C3", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#34-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000287\r\nStartFragment:00000133\r\nEndFragment:00000251\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>C</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell C3</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#34-2");
				Assert.AreEqual (string.Format ("\tC{0}Row#3\tCell C3", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#34-3");
				Assert.AreEqual (string.Format ("\tC{0}Row#3\tCell C3", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#34-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#35-0");
				Assert.AreEqual (string.Format (",A{0}Row#1,Cell A1{0}Row#2,Cell A2{0}Row#3,Cell A3{0}Row#4,Cell A4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#35-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000518\r\nStartFragment:00000133\r\nEndFragment:00000482\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell A4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#35-2");
				Assert.AreEqual (string.Format ("\tA{0}Row#1\tCell A1{0}Row#2\tCell A2{0}Row#3\tCell A3{0}Row#4\tCell A4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#35-3");
				Assert.AreEqual (string.Format ("\tA{0}Row#1\tCell A1{0}Row#2\tCell A2{0}Row#3\tCell A3{0}Row#4\tCell A4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#35-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#36-0");
				Assert.AreEqual (string.Format (",A{0}Row#1,Cell A1{0}Row#2,Cell A2{0}Row#3,Cell A3{0}Row#4,Cell A4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#36-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000518\r\nStartFragment:00000133\r\nEndFragment:00000482\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell A4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#36-2");
				Assert.AreEqual (string.Format ("\tA{0}Row#1\tCell A1{0}Row#2\tCell A2{0}Row#3\tCell A3{0}Row#4\tCell A4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#36-3");
				Assert.AreEqual (string.Format ("\tA{0}Row#1\tCell A1{0}Row#2\tCell A2{0}Row#3\tCell A3{0}Row#4\tCell A4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#36-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#37-0");
				Assert.AreEqual (string.Format (",A{0}Row#1,Cell A1{0}Row#2,Cell A2{0}Row#3,Cell A3{0}Row#4,Cell A4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#37-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000518\r\nStartFragment:00000133\r\nEndFragment:00000482\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell A4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#37-2");
				Assert.AreEqual (string.Format ("\tA{0}Row#1\tCell A1{0}Row#2\tCell A2{0}Row#3\tCell A3{0}Row#4\tCell A4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#37-3");
				Assert.AreEqual (string.Format ("\tA{0}Row#1\tCell A1{0}Row#2\tCell A2{0}Row#3\tCell A3{0}Row#4\tCell A4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#37-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [1].Cells [0].Selected = true;
				dgv.Rows [2].Cells [0].Selected = true;
				dgv.Rows [2].Cells [2].Selected = true;
				dgv.Rows [3].Cells [0].Selected = true;
				dgv.Rows [4].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#38-0");
				Assert.AreEqual (string.Format (",A,B,C{0}Row#2,Cell A2,,{0}Row#3,Cell A3,,Cell C3{0}Row#4,Cell A4,,{0},,,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#38-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000598\r\nStartFragment:00000133\r\nEndFragment:00000562\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell A2</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell A3</TD><TD>&nbsp;</TD><TD>Cell C3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell A4</TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD><TD>&nbsp;</TD><TD>&nbsp;</TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#38-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC{0}Row#2\tCell A2\t\t{0}Row#3\tCell A3\t\tCell C3{0}Row#4\tCell A4\t\t{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#38-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC{0}Row#2\tCell A2\t\t{0}Row#3\tCell A3\t\tCell C3{0}Row#4\tCell A4\t\t{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#38-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#39-0");
				Assert.AreEqual (string.Format (",C{0}Row#1,Cell C1{0}Row#2,Cell C2{0}Row#3,Cell C3{0}Row#4,Cell C4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#39-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000518\r\nStartFragment:00000133\r\nEndFragment:00000482\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>C</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell C1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell C2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell C3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell C4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#39-2");
				Assert.AreEqual (string.Format ("\tC{0}Row#1\tCell C1{0}Row#2\tCell C2{0}Row#3\tCell C3{0}Row#4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#39-3");
				Assert.AreEqual (string.Format ("\tC{0}Row#1\tCell C1{0}Row#2\tCell C2{0}Row#3\tCell C3{0}Row#4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#39-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [2].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#40-0");
				Assert.AreEqual (string.Format (",A,B,C{0}Row#1,Cell A1,,Cell C1{0}Row#2,,,Cell C2{0}Row#3,,,Cell C3{0}Row#4,,,Cell C4{0},,,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#40-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000689\r\nStartFragment:00000133\r\nEndFragment:00000653\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>&nbsp;</TD><TD>Cell C1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>Cell C2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>Cell C3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>Cell C4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#40-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC{0}Row#1\tCell A1\t\tCell C1{0}Row#2\t\t\tCell C2{0}Row#3\t\t\tCell C3{0}Row#4\t\t\tCell C4{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#40-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC{0}Row#1\tCell A1\t\tCell C1{0}Row#2\t\t\tCell C2{0}Row#3\t\t\tCell C3{0}Row#4\t\t\tCell C4{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#40-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Rows [0].Cells [2].Selected = true;
				dgv.Rows [1].Cells [2].Selected = true;
				dgv.Rows [3].Cells [2].Selected = true;
				dgv.Rows [4].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#41-0");
				Assert.AreEqual (string.Format (",C{0}Row#1,Cell C1{0}Row#2,Cell C2{0}Row#3,{0}Row#4,Cell C4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#41-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000517\r\nStartFragment:00000133\r\nEndFragment:00000481\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>C</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell C1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell C2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell C4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#41-2");
				Assert.AreEqual (string.Format ("\tC{0}Row#1\tCell C1{0}Row#2\tCell C2{0}Row#3\t{0}Row#4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#41-3");
				Assert.AreEqual (string.Format ("\tC{0}Row#1\tCell C1{0}Row#2\tCell C2{0}Row#3\t{0}Row#4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#41-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#42-0");
				Assert.AreEqual (string.Format (",C{0}Row#1,Cell C1{0}Row#2,Cell C2{0}Row#3,Cell C3{0}Row#4,Cell C4{0},", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#42-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000518\r\nStartFragment:00000133\r\nEndFragment:00000482\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>C</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell C1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell C2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell C3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell C4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#42-2");
				Assert.AreEqual (string.Format ("\tC{0}Row#1\tCell C1{0}Row#2\tCell C2{0}Row#3\tCell C3{0}Row#4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#42-3");
				Assert.AreEqual (string.Format ("\tC{0}Row#1\tCell C1{0}Row#2\tCell C2{0}Row#3\tCell C3{0}Row#4\tCell C4{0}\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#42-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#43-0");
				Assert.AreEqual (string.Format (",B,C{0}Row#1,Cell B1,Cell C1{0}Row#2,Cell B2,Cell C2{0}Row#3,Cell B3,Cell C3{0}Row#4,Cell B4,Cell C4{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#43-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000601\r\nStartFragment:00000133\r\nEndFragment:00000565\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>B</TH><TH>C</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell B1</TD><TD>Cell C1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell B2</TD><TD>Cell C2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell B3</TD><TD>Cell C3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell B4</TD><TD>Cell C4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#43-2");
				Assert.AreEqual (string.Format ("\tB\tC{0}Row#1\tCell B1\tCell C1{0}Row#2\tCell B2\tCell C2{0}Row#3\tCell B3\tCell C3{0}Row#4\tCell B4\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#43-3");
				Assert.AreEqual (string.Format ("\tB\tC{0}Row#1\tCell B1\tCell C1{0}Row#2\tCell B2\tCell C2{0}Row#3\tCell B3\tCell C3{0}Row#4\tCell B4\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#43-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [2].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#44-0");
				Assert.AreEqual (string.Format (",A,B,C{0}Row#1,Cell A1,Cell B1,Cell C1{0}Row#2,,Cell B2,Cell C2{0}Row#3,,Cell B3,Cell C3{0}Row#4,,Cell B4,Cell C4{0},,,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#44-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000687\r\nStartFragment:00000133\r\nEndFragment:00000651\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>Cell B1</TD><TD>Cell C1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>&nbsp;</TD><TD>Cell B2</TD><TD>Cell C2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>&nbsp;</TD><TD>Cell B3</TD><TD>Cell C3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>&nbsp;</TD><TD>Cell B4</TD><TD>Cell C4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD>&nbsp;</TD><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#44-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC{0}Row#1\tCell A1\tCell B1\tCell C1{0}Row#2\t\tCell B2\tCell C2{0}Row#3\t\tCell B3\tCell C3{0}Row#4\t\tCell B4\tCell C4{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#44-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC{0}Row#1\tCell A1\tCell B1\tCell C1{0}Row#2\t\tCell B2\tCell C2{0}Row#3\t\tCell B3\tCell C3{0}Row#4\t\tCell B4\tCell C4{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#44-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Rows [0].Cells [2].Selected = true;
				dgv.Rows [1].Cells [2].Selected = true;
				dgv.Rows [3].Cells [2].Selected = true;
				dgv.Rows [4].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#45-0");
				Assert.AreEqual (string.Format (",B,C{0}Row#1,Cell B1,Cell C1{0}Row#2,Cell B2,Cell C2{0}Row#3,Cell B3,{0}Row#4,Cell B4,Cell C4{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#45-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000600\r\nStartFragment:00000133\r\nEndFragment:00000564\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>B</TH><TH>C</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell B1</TD><TD>Cell C1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell B2</TD><TD>Cell C2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell B3</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell B4</TD><TD>Cell C4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#45-2");
				Assert.AreEqual (string.Format ("\tB\tC{0}Row#1\tCell B1\tCell C1{0}Row#2\tCell B2\tCell C2{0}Row#3\tCell B3\t{0}Row#4\tCell B4\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#45-3");
				Assert.AreEqual (string.Format ("\tB\tC{0}Row#1\tCell B1\tCell C1{0}Row#2\tCell B2\tCell C2{0}Row#3\tCell B3\t{0}Row#4\tCell B4\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#45-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#46-0");
				Assert.AreEqual (string.Format (",B,C{0}Row#1,Cell B1,Cell C1{0}Row#2,Cell B2,Cell C2{0}Row#3,Cell B3,Cell C3{0}Row#4,Cell B4,Cell C4{0},,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#46-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000601\r\nStartFragment:00000133\r\nEndFragment:00000565\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>B</TH><TH>C</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell B1</TD><TD>Cell C1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell B2</TD><TD>Cell C2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell B3</TD><TD>Cell C3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell B4</TD><TD>Cell C4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#46-2");
				Assert.AreEqual (string.Format ("\tB\tC{0}Row#1\tCell B1\tCell C1{0}Row#2\tCell B2\tCell C2{0}Row#3\tCell B3\tCell C3{0}Row#4\tCell B4\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#46-3");
				Assert.AreEqual (string.Format ("\tB\tC{0}Row#1\tCell B1\tCell C1{0}Row#2\tCell B2\tCell C2{0}Row#3\tCell B3\tCell C3{0}Row#4\tCell B4\tCell C4{0}\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#46-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#47-0");
				Assert.AreEqual (string.Format (",B,C,D{0}Row#1,Cell B1,,Cell D1{0}Row#2,Cell B2,,Cell D2{0}Row#3,Cell B3,,Cell D3{0}Row#4,Cell B4,,Cell D4{0},,,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#47-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000686\r\nStartFragment:00000133\r\nEndFragment:00000650\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell B1</TD><TD>&nbsp;</TD><TD>Cell D1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell B2</TD><TD>&nbsp;</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell B3</TD><TD>&nbsp;</TD><TD>Cell D3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell B4</TD><TD>&nbsp;</TD><TD>Cell D4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD><TD>&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#47-2");
				Assert.AreEqual (string.Format ("\tB\tC\tD{0}Row#1\tCell B1\t\tCell D1{0}Row#2\tCell B2\t\tCell D2{0}Row#3\tCell B3\t\tCell D3{0}Row#4\tCell B4\t\tCell D4{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#47-3");
				Assert.AreEqual (string.Format ("\tB\tC\tD{0}Row#1\tCell B1\t\tCell D1{0}Row#2\tCell B2\t\tCell D2{0}Row#3\tCell B3\t\tCell D3{0}Row#4\tCell B4\t\tCell D4{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#47-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [3].Selected = true;
				dgv.Rows [0].Cells [0].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#48-0");
				Assert.AreEqual (string.Format (",A,B,C,D{0}Row#1,Cell A1,Cell B1,,Cell D1{0}Row#2,,Cell B2,,Cell D2{0}Row#3,,Cell B3,,Cell D3{0}Row#4,,Cell B4,,Cell D4{0},,,,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#48-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000772\r\nStartFragment:00000133\r\nEndFragment:00000736\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell A1</TD><TD>Cell B1</TD><TD>&nbsp;</TD><TD>Cell D1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>&nbsp;</TD><TD>Cell B2</TD><TD>&nbsp;</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>&nbsp;</TD><TD>Cell B3</TD><TD>&nbsp;</TD><TD>Cell D3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>&nbsp;</TD><TD>Cell B4</TD><TD>&nbsp;</TD><TD>Cell D4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD>&nbsp;</TD><TD></TD><TD>&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#48-2");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\tCell B1\t\tCell D1{0}Row#2\t\tCell B2\t\tCell D2{0}Row#3\t\tCell B3\t\tCell D3{0}Row#4\t\tCell B4\t\tCell D4{0}\t\t\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#48-3");
				Assert.AreEqual (string.Format ("\tA\tB\tC\tD{0}Row#1\tCell A1\tCell B1\t\tCell D1{0}Row#2\t\tCell B2\t\tCell D2{0}Row#3\t\tCell B3\t\tCell D3{0}Row#4\t\tCell B4\t\tCell D4{0}\t\t\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#48-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [3].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#49-0");
				Assert.AreEqual (string.Format (",B,C,D{0}Row#1,Cell B1,,Cell D1{0}Row#2,Cell B2,,Cell D2{0}Row#3,Cell B3,,Cell D3{0}Row#4,Cell B4,,Cell D4{0},,,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#49-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000686\r\nStartFragment:00000133\r\nEndFragment:00000650\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell B1</TD><TD>&nbsp;</TD><TD>Cell D1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell B2</TD><TD>&nbsp;</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell B3</TD><TD>&nbsp;</TD><TD>Cell D3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell B4</TD><TD>&nbsp;</TD><TD>Cell D4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD><TD>&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#49-2");
				Assert.AreEqual (string.Format ("\tB\tC\tD{0}Row#1\tCell B1\t\tCell D1{0}Row#2\tCell B2\t\tCell D2{0}Row#3\tCell B3\t\tCell D3{0}Row#4\tCell B4\t\tCell D4{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#49-3");
				Assert.AreEqual (string.Format ("\tB\tC\tD{0}Row#1\tCell B1\t\tCell D1{0}Row#2\tCell B2\t\tCell D2{0}Row#3\tCell B3\t\tCell D3{0}Row#4\tCell B4\t\tCell D4{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#49-4");
			}
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				dgv.Columns [1].Selected = true;
				dgv.Columns [3].Selected = true;
				dgv.Rows [2].Cells [2].Selected = true;
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#EnableAlwaysIncludeHeaderText#50-0");
				Assert.AreEqual (string.Format (",B,C,D{0}Row#1,Cell B1,,Cell D1{0}Row#2,Cell B2,,Cell D2{0}Row#3,Cell B3,Cell C3,Cell D3{0}Row#4,Cell B4,,Cell D4{0},,,", Environment.NewLine), data.GetData (DataFormats.CommaSeparatedValue), "#EnableAlwaysIncludeHeaderText#50-1");
				Assert.AreEqual ("Version:1.0\r\nStartHTML:00000097\r\nEndHTML:00000687\r\nStartFragment:00000133\r\nEndFragment:00000651\r\n<HTML>\r\n<BODY>\r\n<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>B</TH><TH>C</TH><TH>D</TH></THEAD><TR><TD ALIGN=\"center\"><B>Row#1</B></TD><TD>Cell B1</TD><TD>&nbsp;</TD><TD>Cell D1</TD></TR><TR><TD ALIGN=\"center\"><B>Row#2</B></TD><TD>Cell B2</TD><TD>&nbsp;</TD><TD>Cell D2</TD></TR><TR><TD ALIGN=\"center\"><B>Row#3</B></TD><TD>Cell B3</TD><TD>Cell C3</TD><TD>Cell D3</TD></TR><TR><TD ALIGN=\"center\"><B>Row#4</B></TD><TD>Cell B4</TD><TD>&nbsp;</TD><TD>Cell D4</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD></TD><TD>&nbsp;</TD><TD></TD></TR></TABLE>\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>", data.GetData (DataFormats.Html), "#EnableAlwaysIncludeHeaderText#50-2");
				Assert.AreEqual (string.Format ("\tB\tC\tD{0}Row#1\tCell B1\t\tCell D1{0}Row#2\tCell B2\t\tCell D2{0}Row#3\tCell B3\tCell C3\tCell D3{0}Row#4\tCell B4\t\tCell D4{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.UnicodeText), "#EnableAlwaysIncludeHeaderText#50-3");
				Assert.AreEqual (string.Format ("\tB\tC\tD{0}Row#1\tCell B1\t\tCell D1{0}Row#2\tCell B2\t\tCell D2{0}Row#3\tCell B3\tCell C3\tCell D3{0}Row#4\tCell B4\t\tCell D4{0}\t\t\t", Environment.NewLine), data.GetData (DataFormats.Text), "#EnableAlwaysIncludeHeaderText#50-4");
			}
		}
	}
}
#endif
