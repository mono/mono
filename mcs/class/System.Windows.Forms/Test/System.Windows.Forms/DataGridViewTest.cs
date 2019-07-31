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
// Copyright (c) 2005, 2006, 2007 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Pedro Martínez Juliá <pedromj@gmail.com>
//	Daniel Nauck    (dna(at)mono-project(dot)de)
//	Ivan N. Zlatev  <contact@i-nz.net>

using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class DataGridViewTest : TestHelper
	{
		// Send a mouse event in Win32.
		[DllImport ("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern void mouse_event (int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);
		private const int MOUSEEVENTF_LEFTDOWN = 0x02;
		private const int MOUSEEVENTF_LEFTUP = 0x04;
		private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
		private const int MOUSEEVENTF_RIGHTUP = 0x10;
		private const int MOUSEEVENTF_ABSOLUTE = 0x8000;

		// Set the mouse-pointer position in Win32.
		[DllImport ("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern long SetCursorPos (int x, int y);

		// Convert from window coordinates to screen coordinates in Win32.
		[DllImport ("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern bool ClientToScreen (IntPtr hWnd, ref Win32Point point);
		[StructLayout (LayoutKind.Sequential)]
		private struct Win32Point
		{
			public int x;
			public int y;
		};

		private DataGridView grid = null;

		[SetUp]
		protected override void SetUp()
		{
			grid = new DataGridView();
			base.SetUp ();
		}

		[TearDown]
		protected override void TearDown ()
		{
			grid.Dispose ();
			base.TearDown ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Generating Clipboard content is not supported when the ClipboardCopyMode property is Disable.")]
		public void GetClipboardContentsDisabled ()
		{
			using (DataGridView dgv = new DataGridView ()) {
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
				object o = dgv.GetClipboardContent ();
			}
		}

		private class ExposeProtectedProperties : DataGridView
		{
			public new Padding DefaultPadding { get { return base.DefaultPadding; } }
			public new Size DefaultSize { get { return base.DefaultSize; } }
			public new bool IsDoubleBuffered { get { return base.DoubleBuffered; } }

			public ControlStyles GetControlStyles ()
			{
				ControlStyles retval = (ControlStyles)0;

				foreach (ControlStyles cs in Enum.GetValues (typeof (ControlStyles)))
					if (this.GetStyle (cs) == true)
						retval |= cs;

				return retval;
			}
			
			public bool PublicIsInputKey (Keys keyData)
			{
				return base.IsInputKey (keyData);
			}
			
			public bool PublicIsInputChar (char charCode)
			{
				return base.IsInputChar (charCode);
			}
		}

#region GenerateClipboardTest
		public static void GenerateClipboardTest ()
		{
			GenerateClipboardTest (false);
			GenerateClipboardTest (true);
		}

		public static string GenerateClipboardTest (bool headers)
		{
			StringBuilder result = new StringBuilder ();

			int tab = 0;
			string classname = headers ? "DataGridViewClipboardHeaderTest" : "DataGridViewClipboardTest";

			append (result, tab, "//");
			append (result, tab, "// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)");
			append (result, tab, "//");
			append (result, tab, "// Author:");
			append (result, tab, "//	DataGridViewTest.GenerateClipboardTest ({0});", headers.ToString ().ToLower ());
			append (result, tab, "//");
			append (result, tab, "#if NET_2_0");
			append (result, tab, "using NUnit.Framework;");
			append (result, tab, "using System;");
			append (result, tab, "using System.Drawing;");
			append (result, tab, "using System.Windows.Forms;");
			append (result, tab, "using System.ComponentModel;");
			append (result, tab, "using System.Collections;");
			append (result, tab, "using System.Text;");
			append (result, tab, "using System.Collections.Generic;");
			append (result, tab, "using System.Diagnostics;");
			append (result, tab, "using System.IO;");
			append (result, tab, "namespace MonoTests.System.Windows.Forms {"); tab++;
			append (result, tab, "[TestFixture]");
			append (result, tab, "public class {0} {{", classname); tab++;
			append (result, tab, "[Test]");
			append (result, tab, "public void Test () {"); tab++;


			append (result, tab, "DataObject data;");
			append (result, tab, "DataGridViewRowHeaderTest.DataGridViewRowHeaderClipboardCell row_header_cell;");
			append (result, tab, "DataGridViewColumnHeaderTest.DataGridViewColumnHeaderClipboardCell col_header_cell;");
			//append (result, tab, "string csv = null, html = null, utext = null, text = null;");
			append (result, tab, "string code = null;");

			int counter;

			List<List<int>> selected_bands = new List<List<int>> ();
			List<List<CellSelection>> selected_cells = new List<List<CellSelection>> ();

			selected_bands.Add (new List<int> ());
			selected_bands.Add (new List<int> (new int [] { 0 }));
			selected_bands.Add (new List<int> (new int [] { 2 }));
			selected_bands.Add (new List<int> (new int [] { 1, 2 }));
			selected_bands.Add (new List<int> (new int [] { 1, 3 }));

			selected_cells.Add (new List<CellSelection> ());
			selected_cells.Add (new List<CellSelection> (new CellSelection [] { new CellSelection (0, 0, true) }));
			selected_cells.Add (new List<CellSelection> (new CellSelection [] { new CellSelection (2, 2, false) }));
			selected_cells.Add (new List<CellSelection> (new CellSelection [] { new CellSelection (0, 0, false), new CellSelection (2, 2, true) }));

			foreach (DataGridViewClipboardCopyMode copymode in Enum.GetValues (typeof (DataGridViewClipboardCopyMode))) {
				if (copymode == DataGridViewClipboardCopyMode.Disable)
					continue;

				counter = 0;
				foreach (DataGridViewSelectionMode selectionmode in Enum.GetValues (typeof (DataGridViewSelectionMode))) {
					bool is_row_selectable, is_col_selectable, is_cell_selectable;

					is_row_selectable = selectionmode == DataGridViewSelectionMode.RowHeaderSelect || selectionmode == DataGridViewSelectionMode.FullRowSelect;
					is_col_selectable = selectionmode == DataGridViewSelectionMode.ColumnHeaderSelect || selectionmode == DataGridViewSelectionMode.FullColumnSelect;
					is_cell_selectable = selectionmode == DataGridViewSelectionMode.CellSelect || selectionmode == DataGridViewSelectionMode.ColumnHeaderSelect || selectionmode == DataGridViewSelectionMode.RowHeaderSelect;

					foreach (List<int> cols in selected_bands) {
						if (!is_col_selectable && cols.Count > 0)
							continue;

						foreach (List<int> rows in selected_bands) {
							if (!is_row_selectable && rows.Count > 0)
								continue;

							foreach (List<CellSelection> cells in selected_cells) {
								if (!is_cell_selectable && cells.Count > 0)
									continue;

								using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {

									dgv.SelectionMode = selectionmode;
									dgv.ClipboardCopyMode = copymode;
									bool any_selected = false;
									if (is_col_selectable && cols.Count > 0) {
										foreach (int c in cols) {
											dgv.Columns [c].Selected = true;
											any_selected = true;
										}
									}
									if (is_row_selectable && rows.Count > 0) {
										foreach (int r in rows) {
											dgv.Rows [r].Selected = true;
											any_selected = true;
										}
									}
									if (is_cell_selectable && cells.Count > 0) {
										foreach (CellSelection selection in cells) {
											DataGridViewCell cell = dgv.Rows [selection.Row].Cells [selection.Col];
											if (cell.Selected != selection.Selected) {
												cell.Selected = selection.Selected;
												any_selected = true;
											}
										}
									}

									if (any_selected == false && !(cols.Count == 0 && rows.Count == 0 && cells.Count == 0)) {
										continue;
									}

									generate_case (result, dgv, copymode.ToString () + "#" + (counter++).ToString (), headers);
								}
							}
						}
					}
				}
			}

			append (result, --tab, "}");
			append (result, --tab, "}");
			append (result, --tab, "}");
			append (result, tab, "#endif"); ;

			throw new NotImplementedException ("Where am I?");
			// Uncomment the following line, change the path, and comment out the exception.
			//File.WriteAllText (@"Z:\mono\head\mcs\class\SWF\Test\System.Windows.Forms\" + classname + ".cs", result.ToString ());

			return string.Empty;
		}
		
		private static string tabs (int t) { return new string ('\t', t); }
		private static void append (StringBuilder result, int tab, string text) { result.Append (tabs (tab) + text + "\n"); }
		private static void append (StringBuilder result, int tab, string text, params object [] args) { result.Append (tabs (tab) + string.Format (text, args) + "\n"); }
		private static string cs_encode (string literal, string newline) {
			bool has_newlines = literal.Contains ("\r\n");
			bool format_string = has_newlines;

			literal = literal.Replace ("\\", "\\\\");
			literal = literal.Replace ("\"", "\\\"");
			literal = literal.Replace ("\t", "\\t");
			
			if (has_newlines) {
				if (newline == @"""\r\n""") {
					literal = literal.Replace ("\r\n", @"\r\n");
					format_string = false;
				} else {
					literal = literal.Replace ("\r\n", "{0}");
				}
			}

			literal = "\"" + literal + "\"";

			if (format_string) {
				return "string.Format (" + literal/*.Replace ("{", "{{").Replace ("}", "}}")*/ + ", " + newline + ")";
			} else {
				return literal;
			}
		}
		
		private static string cs_encode (string literal) {
			return cs_encode (literal, "Environment.NewLine");
		}
		
		private class CellSelection {
			public bool Selected;
			public int Row;
			public int Col;
			public CellSelection (int Row, int Col, bool Selected) {
				this.Selected = Selected;
				this.Row = Row;
				this.Col = Col;
			}
		}
		
		static private void generate_case (StringBuilder result, DataGridView dgv, string message, bool headers)
		{
			Console.WriteLine (message + ", current length: " + result.Length.ToString ());
			Debug.WriteLine (message + ", current length: " + result.Length.ToString ());
			
			if (headers) {
				if (dgv.SelectionMode != DataGridViewSelectionMode.CellSelect)
					return;
				if (dgv.ClipboardCopyMode != DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText)
					return;
			}
			
			int tab = 3;
			DataObject data;
			string csv = null, html = null, utext = null, text = null;
			string code = null;
			DataGridViewRowHeaderTest.DataGridViewRowHeaderClipboardCell row_header_cell;
			DataGridViewColumnHeaderTest.DataGridViewColumnHeaderClipboardCell col_header_cell;
			int counter = 0;
			
			append (result, tab, "using (DataGridView dgv = DataGridViewCommon.CreateAndFillForClipboard ()) {");
			tab++;
			
			append (result, tab, "dgv.SelectionMode = DataGridViewSelectionMode.{0};", dgv.SelectionMode.ToString ());
			append (result, tab, "dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.{0};", dgv.ClipboardCopyMode.ToString ());
			
			switch (dgv.SelectionMode) {
			case DataGridViewSelectionMode.FullRowSelect:
				foreach (DataGridViewRow row in dgv.Rows) {
					if (row.Selected) {
						append (result, tab, "dgv.Rows [{0}].Selected = true;", row.Index);
					}
				}
				break;
			case DataGridViewSelectionMode.FullColumnSelect:
				foreach (DataGridViewColumn col in dgv.Columns) {
					if (col.Selected) {
						append (result, tab, "dgv.Columns [{0}].Selected = true;", col.Index);
					}
				}
				break;
			case DataGridViewSelectionMode.ColumnHeaderSelect:
			case DataGridViewSelectionMode.RowHeaderSelect:
			case DataGridViewSelectionMode.CellSelect:
				if (dgv.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect) {
					foreach (DataGridViewRow row in dgv.Rows) {
						if (row.Selected) {
							append (result, tab, "dgv.Rows [{0}].Selected = true;", row.Index);
						}
					}
				}
				if (dgv.SelectionMode == DataGridViewSelectionMode.ColumnHeaderSelect) {
					foreach (DataGridViewColumn col in dgv.Columns) {
						if (col.Selected) {
							append (result, tab, "dgv.Columns [{0}].Selected = true;", col.Index);
						}
					}
				}
				for (int r = 0; r < dgv.RowCount; r++) {
					for (int c = 0; c < dgv.ColumnCount; c++) {
						bool rowS = dgv.Rows [r].Selected;
						bool colS = dgv.Columns [c].Selected;
						bool cellS = dgv.Rows [r].Cells [c].Selected;
						
						if ((rowS || colS) && !cellS) {
							append (result, tab, "dgv.Rows [{0}].Cells [{1}].Selected = false;", r, c);
						} else if ((!rowS && !colS) && cellS) {
							append (result, tab, "dgv.Rows [{0}].Cells [{1}].Selected = true;", r, c);
						}
					}
				}
				break;
			}
			
			if (!headers) {
				data = dgv.GetClipboardContent ();
				append (result, tab, "data = dgv.GetClipboardContent ();");
				
				if (data == null) {
					append (result, tab, "Assert.IsNull (data, {0});", cs_encode ("#" + message + "-" + (counter++).ToString ()));
				} else {
					append (result, tab, "Assert.IsNotNull (data, {0});", cs_encode ("#" + message + "-" + (counter++).ToString ()));
					
					csv = data.GetData (DataFormats.CommaSeparatedValue) as string;
					html = data.GetData (DataFormats.Html) as string;
					utext = data.GetData (DataFormats.UnicodeText) as string;
					text = data.GetData (DataFormats.Text) as string;
					
					append (result, tab, "Assert.AreEqual ({0}, data.GetData (DataFormats.CommaSeparatedValue), {1});", cs_encode (csv), cs_encode ("#" + message + "-" + (counter++).ToString ()));
					append (result, tab, "Assert.AreEqual ({0}, data.GetData (DataFormats.Html), {1});", cs_encode (html, @"""\r\n"""), cs_encode ("#" + message + "-" + (counter++).ToString ()));
					append (result, tab, "Assert.AreEqual ({0}, data.GetData (DataFormats.UnicodeText), {1});", cs_encode (utext), cs_encode ("#" + message + "-" + (counter++).ToString ()));
					append (result, tab, "Assert.AreEqual ({0}, data.GetData (DataFormats.Text), {1});", cs_encode (text), cs_encode ("#" + message + "-" + (counter++).ToString ()));
				}
			} else {
				bool [] bools = new bool [] { true, false };
				string [] formats = new string [] { DataFormats.Text, DataFormats.UnicodeText, DataFormats.Html, DataFormats.CommaSeparatedValue };


				foreach (bool a in bools) {
					foreach (bool b in bools) {
						foreach (bool c in bools) {
							foreach (bool d in bools) {
								foreach (string format in formats) {
									bool did_selected = false;
									bool did_unselected = false;
									foreach (DataGridViewRow row in dgv.Rows) {
										int i = row.Index;
										if (row.Selected) {
											if (did_selected)
												continue;
											did_selected = true;
										} else {
											if (did_unselected)
												continue;
											did_unselected = true;
										}
										row_header_cell = row.HeaderCell as DataGridViewRowHeaderTest.DataGridViewRowHeaderClipboardCell;
										if (row_header_cell == null) {
											append (result, tab, "Assert.IsNull (dgv.Rows [{0}].Headercell, {1});", row.Index, cs_encode ("#" + message + "-" + (counter++).ToString ()));
										} else {
											append (result, tab, "row_header_cell = dgv.Rows [{0}].HeaderCell as DataGridViewRowHeaderTest.DataGridViewRowHeaderClipboardCell;", row.Index);
											code = cs_encode (row_header_cell.GetClipboardContentPublic (i, a, b, c, d, format) as string);
											append (result, tab, "code = row_header_cell.GetClipboardContentPublic ({0}, {1}, {2}, {3}, {4}, \"{5}\") as string;", i, a.ToString ().ToLower (), b.ToString ().ToLower (), c.ToString ().ToLower (), d.ToString ().ToLower (), format);
											append (result, tab, "Assert.AreEqual ({0}, code, {1});", code, cs_encode ("#" + message + "-" + (counter++).ToString ()));
										}
									}
								}
							}
						}
					}
				}

				foreach (bool a in bools) {
					foreach (bool b in bools) {
						foreach (bool c in bools) {
							foreach (bool d in bools) {
								foreach (string format in formats) {
									bool did_selected = false;
									bool did_unselected = false;
									foreach (DataGridViewColumn col in dgv.Columns) {
										int i = -1;
										if (col.Index > 1)
											continue;
										if (col.Selected) {
											if (did_selected)
												continue;
											did_selected = true;
										} else {
											if (did_unselected)
												continue;
											did_unselected = true;
										}
										col_header_cell = col.HeaderCell as DataGridViewColumnHeaderTest.DataGridViewColumnHeaderClipboardCell;
										append (result, tab, "col_header_cell = dgv.Columns [{0}].HeaderCell as DataGridViewColumnHeaderTest.DataGridViewColumnHeaderClipboardCell;", col.Index);
										code = cs_encode (col_header_cell.GetClipboardContentPublic (i, a, b, c, d, format) as string);
										append (result, tab, "code = col_header_cell.GetClipboardContentPublic ({0}, {1}, {2}, {3}, {4}, \"{5}\") as string;", i, a.ToString ().ToLower (), b.ToString ().ToLower (), c.ToString ().ToLower (), d.ToString ().ToLower (), format);
										append (result, tab, "Assert.AreEqual ({0}, code, {1});", code, cs_encode ("#" + message + "-" + (counter++).ToString ()));
									}
								}
							}
						}
					}
				}
			}
			tab--;
			append (result, tab, "}");
		}
#endregion GenerateClipboardTest

		[Test]
		public void GetClipboardContents ()
		{
			DataObject data;
			string csv, html, utext, text;
			
			using (DataGridView dgv = DataGridViewCommon.CreateAndFill ()) {
				data = dgv.GetClipboardContent ();	
				Assert.IsNull (data, "#01");
				
				dgv.Rows [0].Cells [0].Selected = true;
				
				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#B1");

				Assert.AreEqual (new string [] { DataFormats.CommaSeparatedValue, DataFormats.Html, DataFormats.UnicodeText, DataFormats.Text }, data.GetFormats (), "#B2");
				Assert.AreEqual (new string [] { DataFormats.CommaSeparatedValue, DataFormats.Html, DataFormats.UnicodeText, DataFormats.Text }, data.GetFormats (true), "#B3");
				
				csv = data.GetData (DataFormats.CommaSeparatedValue) as string;
				html = data.GetData (DataFormats.Html) as string;
				utext = data.GetData (DataFormats.UnicodeText) as string;
				text = data.GetData (DataFormats.Text) as string;

				Assert.AreEqual ("Cell A1", csv, "CSV B");
				Assert.AreEqual ("Cell A1", utext, "UTEXT B");
				Assert.AreEqual ("Cell A1", text, "TEXT B");
				Assert.AreEqual (string.Format(@"Version:1.0{0}" + 
"StartHTML:00000097{0}" + 
"EndHTML:00000211{0}" + 
"StartFragment:00000133{0}" + 
"EndFragment:00000175{0}" + 
"<HTML>{0}" + 
"<BODY>{0}" + 
"<!--StartFragment--><TABLE><TR><TD>Cell A1</TD></TR></TABLE>{0}" + 
"<!--EndFragment-->{0}" + 
"</BODY>{0}" + 
"</HTML>", "\r\n"), html, "HTML B");

				dgv.Rows [1].Cells [1].Selected = true;

				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#C1");

				Assert.AreEqual (new string [] { DataFormats.CommaSeparatedValue, DataFormats.Html, DataFormats.UnicodeText, DataFormats.Text }, data.GetFormats (), "#C2");
				Assert.AreEqual (new string [] { DataFormats.CommaSeparatedValue, DataFormats.Html, DataFormats.UnicodeText, DataFormats.Text }, data.GetFormats (true), "#C3");

				csv = data.GetData (DataFormats.CommaSeparatedValue) as string;
				html = data.GetData (DataFormats.Html) as string;
				utext = data.GetData (DataFormats.UnicodeText) as string;
				text = data.GetData (DataFormats.Text) as string;

				Assert.AreEqual (string.Format("Cell A1,{0},Cell B2", Environment.NewLine), csv, "CSV C");
				Assert.AreEqual (string.Format("Cell A1\t{0}\tCell B2", Environment.NewLine), utext, "UTEXT C");
				Assert.AreEqual (string.Format("Cell A1\t{0}\tCell B2", Environment.NewLine), text, "TEXT C");
				string tmp;
				tmp = string.Format(@"Version:1.0{0}" +
"StartHTML:00000097{0}" +
"EndHTML:00000266{0}" +
"StartFragment:00000133{0}" +
"EndFragment:00000230{0}" +
"<HTML>{0}" +
"<BODY>{0}" +
"<!--StartFragment--><TABLE><TR><TD>Cell A1</TD><TD>&nbsp;</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B2</TD></TR></TABLE>{0}" +
"<!--EndFragment-->{0}" +
"</BODY>{0}" +
"</HTML>", "\r\n");

				Assert.AreEqual (string.Format(@"Version:1.0{0}" +
"StartHTML:00000097{0}" +
"EndHTML:00000266{0}" +
"StartFragment:00000133{0}" +
"EndFragment:00000230{0}" +
"<HTML>{0}" +
"<BODY>{0}" +
"<!--StartFragment--><TABLE><TR><TD>Cell A1</TD><TD>&nbsp;</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B2</TD></TR></TABLE>{0}" +
"<!--EndFragment-->{0}" +
"</BODY>{0}" +
"</HTML>", "\r\n"), html, "HTML C");
			}
		}

		[Test]
		public void GetClipboardContents_HeadersAlways ()
		{
			DataObject data;
			string csv, html, utext, text;

			using (DataGridView dgv = DataGridViewCommon.CreateAndFill ()) {
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#01");

				dgv.Rows [0].Cells [0].Selected = true;

				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#B1");

				Assert.AreEqual (new string [] { DataFormats.CommaSeparatedValue, DataFormats.Html, DataFormats.UnicodeText, DataFormats.Text }, data.GetFormats (), "#B2");
				Assert.AreEqual (new string [] { DataFormats.CommaSeparatedValue, DataFormats.Html, DataFormats.UnicodeText, DataFormats.Text }, data.GetFormats (true), "#B3");

				csv = data.GetData (DataFormats.CommaSeparatedValue) as string;
				html = data.GetData (DataFormats.Html) as string;
				utext = data.GetData (DataFormats.UnicodeText) as string;
				text = data.GetData (DataFormats.Text) as string;

				Assert.AreEqual (string.Format (",A{0},Cell A1", Environment.NewLine), csv, "CSV B");
				Assert.AreEqual (string.Format ("\tA{0}\tCell A1", Environment.NewLine), utext, "UTEXT B");
				Assert.AreEqual (string.Format ("\tA{0}\tCell A1", Environment.NewLine), text, "TEXT B");
				Assert.AreEqual (string.Format (@"Version:1.0{0}" +
"StartHTML:00000097{0}" +
"EndHTML:00000281{0}" +
"StartFragment:00000133{0}" +
"EndFragment:00000245{0}" +
"<HTML>{0}" +
"<BODY>{0}" +
"<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH></THEAD><TR><TD ALIGN=\"center\">&nbsp;</TD><TD>Cell A1</TD></TR></TABLE>{0}" +
"<!--EndFragment-->{0}" +
"</BODY>{0}" +
"</HTML>", "\r\n"), html, "HTML B");

				dgv.Rows [1].Cells [1].Selected = true;

				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#C1");

				Assert.AreEqual (new string [] { DataFormats.CommaSeparatedValue, DataFormats.Html, DataFormats.UnicodeText, DataFormats.Text }, data.GetFormats (), "#C2");
				Assert.AreEqual (new string [] { DataFormats.CommaSeparatedValue, DataFormats.Html, DataFormats.UnicodeText, DataFormats.Text }, data.GetFormats (true), "#C3");

				csv = data.GetData (DataFormats.CommaSeparatedValue) as string;
				html = data.GetData (DataFormats.Html) as string;
				utext = data.GetData (DataFormats.UnicodeText) as string;
				text = data.GetData (DataFormats.Text) as string;

				Assert.AreEqual (string.Format (",A,B{0},Cell A1,{0},,Cell B2", Environment.NewLine), csv, "CSV C");
				Assert.AreEqual (string.Format ("\tA\tB{0}\tCell A1\t{0}\t\tCell B2", Environment.NewLine), utext, "UTEXT C");
				Assert.AreEqual (string.Format ("\tA\tB{0}\tCell A1\t{0}\t\tCell B2", Environment.NewLine), text, "TEXT C");
				string tmp;
				tmp = string.Format (@"Version:1.0{0}" +
"StartHTML:00000097{0}" +
"EndHTML:00000266{0}" +
"StartFragment:00000133{0}" +
"EndFragment:00000230{0}" +
"<HTML>{0}" +
"<BODY>{0}" +
"<!--StartFragment--><TABLE><TR><TD>Cell A1</TD><TD>&nbsp;</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B2</TD></TR></TABLE>{0}" +
"<!--EndFragment-->{0}" +
"</BODY>{0}" +
"</HTML>", "\r\n");

				Assert.AreEqual (string.Format (@"Version:1.0{0}" +
"StartHTML:00000097{0}" +
"EndHTML:00000376{0}" +
"StartFragment:00000133{0}" +
"EndFragment:00000340{0}" +
"<HTML>{0}" +
"<BODY>{0}" +
"<!--StartFragment--><TABLE><THEAD><TH>&nbsp;</TH><TH>A</TH><TH>B</TH></THEAD><TR><TD ALIGN=\"center\">&nbsp;</TD><TD>Cell A1</TD><TD>&nbsp;</TD></TR><TR><TD ALIGN=\"center\">&nbsp;</TD><TD>&nbsp;</TD><TD>Cell B2</TD></TR></TABLE>{0}" +
"<!--EndFragment-->{0}" +
"</BODY>{0}" +
"</HTML>", "\r\n"), html, "HTML C");
			}
		}

		[Test]
		public void GetClipboardContents_HeadersNever ()
		{
			DataObject data;
			string csv, html, utext, text;

			using (DataGridView dgv = DataGridViewCommon.CreateAndFill ()) {
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				data = dgv.GetClipboardContent ();
				Assert.IsNull (data, "#01");

				dgv.Rows [0].Cells [0].Selected = true;

				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#B1");

				Assert.AreEqual (new string [] { DataFormats.CommaSeparatedValue, DataFormats.Html, DataFormats.UnicodeText, DataFormats.Text }, data.GetFormats (), "#B2");
				Assert.AreEqual (new string [] { DataFormats.CommaSeparatedValue, DataFormats.Html, DataFormats.UnicodeText, DataFormats.Text }, data.GetFormats (true), "#B3");

				csv = data.GetData (DataFormats.CommaSeparatedValue) as string;
				html = data.GetData (DataFormats.Html) as string;
				utext = data.GetData (DataFormats.UnicodeText) as string;
				text = data.GetData (DataFormats.Text) as string;

				Assert.AreEqual ("Cell A1", csv, "CSV B");
				Assert.AreEqual ("Cell A1", utext, "UTEXT B");
				Assert.AreEqual ("Cell A1", text, "TEXT B");
				Assert.AreEqual (string.Format (@"Version:1.0{0}" +
"StartHTML:00000097{0}" +
"EndHTML:00000211{0}" +
"StartFragment:00000133{0}" +
"EndFragment:00000175{0}" +
"<HTML>{0}" +
"<BODY>{0}" +
"<!--StartFragment--><TABLE><TR><TD>Cell A1</TD></TR></TABLE>{0}" +
"<!--EndFragment-->{0}" +
"</BODY>{0}" +
"</HTML>", "\r\n"), html, "HTML B");

				dgv.Rows [1].Cells [1].Selected = true;

				data = dgv.GetClipboardContent ();
				Assert.IsNotNull (data, "#C1");

				Assert.AreEqual (new string [] { DataFormats.CommaSeparatedValue, DataFormats.Html, DataFormats.UnicodeText, DataFormats.Text }, data.GetFormats (), "#C2");
				Assert.AreEqual (new string [] { DataFormats.CommaSeparatedValue, DataFormats.Html, DataFormats.UnicodeText, DataFormats.Text }, data.GetFormats (true), "#C3");

				csv = data.GetData (DataFormats.CommaSeparatedValue) as string;
				html = data.GetData (DataFormats.Html) as string;
				utext = data.GetData (DataFormats.UnicodeText) as string;
				text = data.GetData (DataFormats.Text) as string;

				Assert.AreEqual (string.Format ("Cell A1,{0},Cell B2", Environment.NewLine), csv, "CSV C");
				Assert.AreEqual (string.Format ("Cell A1\t{0}\tCell B2", Environment.NewLine), utext, "UTEXT C");
				Assert.AreEqual (string.Format ("Cell A1\t{0}\tCell B2", Environment.NewLine), text, "TEXT C");
				string tmp;
				tmp = string.Format (@"Version:1.0{0}" +
"StartHTML:00000097{0}" +
"EndHTML:00000266{0}" +
"StartFragment:00000133{0}" +
"EndFragment:00000230{0}" +
"<HTML>{0}" +
"<BODY>{0}" +
"<!--StartFragment--><TABLE><TR><TD>Cell A1</TD><TD>&nbsp;</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B2</TD></TR></TABLE>{0}" +
"<!--EndFragment-->{0}" +
"</BODY>{0}" +
"</HTML>", "\r\n");

				Assert.AreEqual (string.Format (@"Version:1.0{0}" +
"StartHTML:00000097{0}" +
"EndHTML:00000266{0}" +
"StartFragment:00000133{0}" +
"EndFragment:00000230{0}" +
"<HTML>{0}" +
"<BODY>{0}" +
"<!--StartFragment--><TABLE><TR><TD>Cell A1</TD><TD>&nbsp;</TD></TR><TR><TD>&nbsp;</TD><TD>Cell B2</TD></TR></TABLE>{0}" +
"<!--EndFragment-->{0}" +
"</BODY>{0}" +
"</HTML>", "\r\n"), html, "HTML C");
			}
		}
		
		[Test]
		public void EditingRow ()
		{
			using (DataGridView dgv = new DataGridView ()) {
				Assert.AreEqual (true, dgv.AllowUserToAddRows, "1");
				Assert.AreEqual (0, dgv.RowCount, "2");
				Assert.AreEqual (-1, dgv.NewRowIndex, "3");
				dgv.Columns.Add ("A", "B");
				Assert.AreEqual (1, dgv.RowCount, "4");
				
				int added;
				added = dgv.Rows.Add ("a");
				Assert.AreEqual (0, added, "5");
			}
		}

		[Test] // bug 82226
		public void EditingRowAfterAddingColumns ()
		{
			using (DataGridView _dataGridView = new DataGridView ()) {
				DataGridViewTextBoxColumn _nameTextBoxColumn;
				DataGridViewTextBoxColumn _firstNameTextBoxColumn;
				// 
				// _nameTextBoxColumn
				// 
				_nameTextBoxColumn = new DataGridViewTextBoxColumn ();
				_nameTextBoxColumn.HeaderText = "Name";
				_dataGridView.Columns.Add (_nameTextBoxColumn);
				// 
				// _firstNameTextBoxColumn
				// 
				_firstNameTextBoxColumn = new DataGridViewTextBoxColumn ();
				_firstNameTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
				_firstNameTextBoxColumn.HeaderText = "First Name";
				_dataGridView.Columns.Add (_firstNameTextBoxColumn);

				_dataGridView.Rows.Add ("de Icaza", "Miguel");
				_dataGridView.Rows.Add ("Toshok", "Chris");
				_dataGridView.Rows.Add ("Harper", "Jackson");
				
				Assert.AreEqual (4, _dataGridView.RowCount, "#01");
				Assert.AreEqual (2, _dataGridView.Rows [3].Cells.Count, "#02");
			}
		}

		// For testing the editing-control-showing event.
		int editingControlShowingTest_FoundColumns;
		private void DataGridView_EditingControlShowingTest (object sender,
			DataGridViewEditingControlShowingEventArgs e)
		{
			DataGridView dgv = sender as DataGridView;
			if (dgv.CurrentCellAddress.X == 0)
			{
				// This is the name combo-box column.
				// Remember that the event-handler was called for
				// this column.
				editingControlShowingTest_FoundColumns |= 1;

				// Get the combo-box and the column.
				ComboBox cb = e.Control as ComboBox;
				DataGridViewComboBoxColumn col
					= dgv.Columns[0] as DataGridViewComboBoxColumn;

				// Since ObjectCollection doesn't support ToArray(), make
				// a list of the items in the combo-box and in the column.
				List<string> itemList = new List<string> ();
				foreach (string item in cb.Items)
					itemList.Add (item);
				List<string> expectedItemList = new List<string> ();
				foreach (string item in col.Items)
					expectedItemList.Add (item);

				// Make sure the combo-box has the list of allowed
				// items from the column.
				string items = string.Join (",", itemList.ToArray ());
				string expectedItems = string.Join (",", expectedItemList.ToArray ());
				Assert.AreEqual (expectedItems, items, "1-1");

				// Make sure the combo-box has the right selected item.
				Assert.AreEqual ("Boswell", cb.Text, "1-2");
			}
			else if (dgv.CurrentCellAddress.X == 1)
			{
				// This is the first-name text-box column.
				// Remember that the event-handler was called for
				// this column.
				editingControlShowingTest_FoundColumns |= 2;

				// Get the text-box.
				TextBox tb = e.Control as TextBox;

				// Make sure the text-box has the right contents.
				Assert.AreEqual ("Miguel", tb.Text, "1-3");
			}
			else if (dgv.CurrentCellAddress.X == 2)
			{
				// This is the chosen check-box column.
				// Remember that the event-handler was called for
				// this column.
				editingControlShowingTest_FoundColumns |= 4;

				// Get the check-box.
				CheckBox tb = e.Control as CheckBox;

				// Make sure the check-box has the right contents.
				Assert.AreEqual (CheckState.Checked, tb.CheckState, "1-4");
			}
			else
				Assert.AreEqual (0, 1, "1-5");
		}

		[Test] // Xamarin bug 5419
		public void EditingControlShowingTest_Unbound ()
		{
			using (DataGridView _dataGridView = new DataGridView ()) {
				DataGridViewComboBoxColumn _nameComboBoxColumn;
				DataGridViewTextBoxColumn _firstNameTextBoxColumn;
				DataGridViewCheckBoxColumn _chosenCheckBoxColumn;

				// Add the event-handler.
				_dataGridView.EditingControlShowing
					+= new DataGridViewEditingControlShowingEventHandler
						(DataGridView_EditingControlShowingTest);
				
				// No columns have been found in the event-handler yet.
				editingControlShowingTest_FoundColumns = 0;

				// _nameComboBoxColumn
				_nameComboBoxColumn = new DataGridViewComboBoxColumn ();
				_nameComboBoxColumn.HeaderText = "Name";
				_dataGridView.Columns.Add (_nameComboBoxColumn);

				// _firstNameTextBoxColumn
				_firstNameTextBoxColumn = new DataGridViewTextBoxColumn ();
				_firstNameTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
				_firstNameTextBoxColumn.HeaderText = "First Name";
				_dataGridView.Columns.Add (_firstNameTextBoxColumn);

				// _chosenCheckBoxColumn
				_chosenCheckBoxColumn = new DataGridViewCheckBoxColumn ();
				_chosenCheckBoxColumn.HeaderText = "Chosen";
				_dataGridView.Columns.Add (_chosenCheckBoxColumn);

				// .NET requires that all possible values for combo-boxes in a column
				// are added to the column.
				_nameComboBoxColumn.Items.Add ("de Icaza");
				_nameComboBoxColumn.Items.Add ("Toshok");
				_nameComboBoxColumn.Items.Add ("Harper");
				_nameComboBoxColumn.Items.Add ("Boswell");

				// Set up the contents of the data-grid.
				_dataGridView.Rows.Add ("de Icaza", "Miguel", true);
				_dataGridView.Rows.Add ("Toshok", "Chris", false);
				_dataGridView.Rows.Add ("Harper", "Jackson", false);
				_dataGridView.Rows.Add ("Boswell", "Steven", true);
				
				// Edit a combo-box cell.
				_dataGridView.CurrentCell = _dataGridView.Rows[3].Cells[0];
				Assert.AreEqual (true, _dataGridView.Rows[3].Cells[0].Selected, "1-6");
				Assert.AreEqual (true, _dataGridView.BeginEdit (false), "1-7");
				_dataGridView.CancelEdit();

				// Edit a text-box cell.
				_dataGridView.CurrentCell = _dataGridView.Rows[0].Cells[1];
				Assert.AreEqual (false, _dataGridView.Rows[3].Cells[0].Selected, "1-8");
				Assert.AreEqual (true, _dataGridView.Rows[0].Cells[1].Selected, "1-9");
				Assert.AreEqual (true, _dataGridView.BeginEdit (false), "1-10");
				_dataGridView.CancelEdit();

				// Edit a check-box cell.
				_dataGridView.CurrentCell = _dataGridView.Rows[3].Cells[2];
				Assert.AreEqual (false, _dataGridView.Rows[0].Cells[1].Selected, "1-11");
				Assert.AreEqual (true, _dataGridView.Rows[3].Cells[2].Selected, "1-12");
				Assert.AreEqual (true, _dataGridView.BeginEdit (false), "1-13");
				_dataGridView.CancelEdit();

				// Make sure the event-handler was called each time.
				// (DataGridViewCheckBoxCell isn't derived from Control, so the
				// EditingControlShowing event doesn't get called for it.)
				Assert.AreEqual (3, editingControlShowingTest_FoundColumns, "1-14");

				_dataGridView.Dispose();
			}
		}

		// A simple class, for testing the data-binding variant of the
		// editing-control-showing event.
		private class EcstRecord
		{
			string name;
			string firstName;
			bool chosen;

			public EcstRecord (string newName, string newFirstName, bool newChosen)
			{
				name = newName;
				firstName = newFirstName;
				chosen = newChosen;
			}
			public string Name
			{
				get { return name; }
				set { name = value; }
			}
			public string FirstName
			{
				get { return firstName; }
				set { firstName = value; }
			}
			public bool Chosen
			{
				get { return chosen; }
				set { chosen = value; }
			}
		};

		[Test] // Xamarin bug 5419
		public void EditingControlShowingTest_Bound ()
		{
			using (DataGridView _dataGridView = new DataGridView ()) {
				DataGridViewComboBoxColumn _nameComboBoxColumn;
				DataGridViewTextBoxColumn _firstNameTextBoxColumn;
				DataGridViewCheckBoxColumn _chosenCheckBoxColumn;

				_dataGridView.AutoGenerateColumns = false;

				// Add the event-handler.
				_dataGridView.EditingControlShowing
					+= new DataGridViewEditingControlShowingEventHandler
						(DataGridView_EditingControlShowingTest);

				// No columns have been found in the event-handler yet.
				editingControlShowingTest_FoundColumns = 0;

				// _nameComboBoxColumn
				_nameComboBoxColumn = new DataGridViewComboBoxColumn ();
				_nameComboBoxColumn.HeaderText = "Name";
				_nameComboBoxColumn.DataPropertyName = "Name";
				_dataGridView.Columns.Add (_nameComboBoxColumn);

				// _firstNameTextBoxColumn
				_firstNameTextBoxColumn = new DataGridViewTextBoxColumn ();
				_firstNameTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
				_firstNameTextBoxColumn.HeaderText = "First Name";
				_firstNameTextBoxColumn.DataPropertyName = "FirstName";
				_dataGridView.Columns.Add (_firstNameTextBoxColumn);

				// _chosenCheckBoxColumn
				_chosenCheckBoxColumn = new DataGridViewCheckBoxColumn ();
				_chosenCheckBoxColumn.HeaderText = "Chosen";
				_chosenCheckBoxColumn.DataPropertyName = "Chosen";
				_chosenCheckBoxColumn.FalseValue = "false";
				_chosenCheckBoxColumn.TrueValue = "true";
				_dataGridView.Columns.Add (_chosenCheckBoxColumn);

				// .NET requires that all possible values for combo-boxes in a column
				// are added to the column.
				_nameComboBoxColumn.Items.Add ("de Icaza");
				_nameComboBoxColumn.Items.Add ("Toshok");
				_nameComboBoxColumn.Items.Add ("Harper");
				_nameComboBoxColumn.Items.Add ("Boswell");

				// Set up the contents of the data-grid.
				BindingList<EcstRecord> boundData = new BindingList<EcstRecord> ();
				boundData.Add (new EcstRecord ("de Icaza", "Miguel", true));
				boundData.Add (new EcstRecord ("Toshok", "Chris", false));
				boundData.Add (new EcstRecord ("Harper", "Jackson", false));
				boundData.Add (new EcstRecord ("Boswell", "Steven", true));
				_dataGridView.DataSource = boundData;

				// For data binding to work, there needs to be a Form, apparently.
				Form form = new Form ();
				form.ShowInTaskbar = false;
				form.Controls.Add (_dataGridView);
				form.Show ();

				// Make sure the data-source took.
				// (Without the Form, instead of having four rows, the data grid
				// only has one row, and all its cell values are null.)
				Assert.AreEqual (boundData.Count, _dataGridView.Rows.Count, "1-6");
				
				// Edit a combo-box cell.
				_dataGridView.CurrentCell = _dataGridView.Rows[3].Cells[0];
				Assert.AreEqual (true, _dataGridView.Rows[3].Cells[0].Selected, "1-7");
				Assert.AreEqual (true, _dataGridView.BeginEdit (false), "1-8");
				_dataGridView.CancelEdit();

				// Edit a text-box cell.
				_dataGridView.CurrentCell = _dataGridView.Rows[0].Cells[1];
				Assert.AreEqual (false, _dataGridView.Rows[3].Cells[0].Selected, "1-9");
				Assert.AreEqual (true, _dataGridView.Rows[0].Cells[1].Selected, "1-10");
				Assert.AreEqual (true, _dataGridView.BeginEdit (false), "1-11");
				_dataGridView.CancelEdit();

				// Edit a check-box cell.
				_dataGridView.CurrentCell = _dataGridView.Rows[3].Cells[2];
				Assert.AreEqual (false, _dataGridView.Rows[0].Cells[1].Selected, "1-12");
				Assert.AreEqual (true, _dataGridView.Rows[3].Cells[2].Selected, "1-13");
				Assert.AreEqual (true, _dataGridView.BeginEdit (false), "1-14");
				_dataGridView.CancelEdit();

				// Make sure the event-handler was called each time.
				// (DataGridViewCheckBoxCell isn't derived from Control, so the
				// EditingControlShowing event doesn't get called for it.)
				Assert.AreEqual (3, editingControlShowingTest_FoundColumns, "1-14");

				// Get rid of the form.
				form.Close();
			}
		}

		[Test]
		public void bug_81918 ()
		{
			using (DataGridView dgv = new DataGridView ()) {
				DataGridViewColumn col = new DataGridViewComboBoxColumn ();
				
				dgv.Columns.Add (col);
				
				dgv.Rows.Add ("a");
				
				DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell) dgv [0, 0];
			}
		}

		// A custom data-grid-view, created solely so that
		// mouse clicks can be faked on it.
		private class ClickableDataGridView : DataGridView
		{
			public ClickableDataGridView ()
			: base ()
			{
			}

			internal void OnMouseDownInternal (MouseEventArgs e)
			{
				OnMouseDown (e);
			}

			internal void OnMouseUpInternal (MouseEventArgs e)
			{
				OnMouseUp (e);
			}
		};

		[Test]
		public void OneClickComboBoxCell ()
		{
			Form form = null;

			try
			{
				// Create a form, a text label, and a data-grid-view.
				form = new Form ();
				Label label = new Label ();
				label.Text = "Label";
				label.Parent = form;
				ClickableDataGridView dgv = new ClickableDataGridView ();
				dgv.Parent = form;

				// Create a combo-box column.
				DataGridViewComboBoxColumn cbCol = new DataGridViewComboBoxColumn ();
				cbCol.HeaderText = "Name";
				dgv.Columns.Add (cbCol);

				// .NET requires that all possible values for combo-boxes
				// in a column are added to the column.
				cbCol.Items.Add ("Item1");
				cbCol.Items.Add ("Item2");
				cbCol.Items.Add ("Item3");
				cbCol.Items.Add ("Item4");

				// Set up the contents of the data-grid.
				dgv.Rows.Add ("Item1");
				dgv.Rows.Add ("Item2");

				// Select the cell.
				dgv.CurrentCell = dgv.Rows[0].Cells[0];

				// Focus the data-grid-view.  (Without this, its Leave
				// event won't get called when something outside of the
				// data-grid-view gets focused.)
				dgv.Focus ();

				// Show the form, let it draw.
				form.Show ();
				Application.DoEvents ();

				// Locate the drop-down button.  (This code is taken from mono-winforms,
				// from the private method DataGridViewComboBoxCell.CalculateButtonArea(),
				// and was then hacked mercilessly.)
				Rectangle button_area = Rectangle.Empty;
				{
					int border = 3 /* ThemeEngine.Current.Border3DSize.Width */;
					const int button_width = 16;
					Rectangle text_area = dgv.GetCellDisplayRectangle (0, 0, false);
					button_area.X = text_area.Right - button_width - border;
					button_area.Y = text_area.Y + border;
					button_area.Width = button_width;
					button_area.Height = text_area.Height - 2 * border;
				}

				// Click on the drop-down button.
				int x = button_area.X + (button_area.Width / 2);
				int y = button_area.Y + (button_area.Height / 2);
				if (Environment.OSVersion.Platform == PlatformID.Win32NT
				&& Type.GetType ("Mono.Runtime") == null)
				{
					// Calling OnMouseDownInternal () in Win32 doesn't work.
					// My best guess as to why is that the WinForms ComboBox
					// is a wrapper around the ComCtl control, e.g. similar
					// to the reason that Paint event-handlers don't work on
					// TreeView.  So we go through all this rigamarole to
					// simulate a mouse click.

					// First, get the location of the desired mouse-click, in
					// data-grid-view coordinates.
					Win32Point ptGlobal = new Win32Point ();
					ptGlobal.x = x + dgv.Location.X;
					ptGlobal.y = y + dgv.Location.Y;

					// Convert that to screen coordinates.
					ClientToScreen (form.Handle, ref ptGlobal);

					// Move the mouse-pointer there.  (Yes, this really appears
					// to be necessary.)
					SetCursorPos (ptGlobal.x, ptGlobal.y);

					// Convert screen coordinates to mouse coordinates.
					ptGlobal.x *= (65535 / SystemInformation.VirtualScreen.Width);
					ptGlobal.y *= (65535 / SystemInformation.VirtualScreen.Height);

					// Finally, fire a mouse-down and mouse-up event.
					mouse_event (MOUSEEVENTF_LEFTDOWN|MOUSEEVENTF_ABSOLUTE,
						ptGlobal.x, ptGlobal.y, 0, IntPtr.Zero);
					mouse_event (MOUSEEVENTF_LEFTUP|MOUSEEVENTF_ABSOLUTE,
						ptGlobal.x, ptGlobal.y, 0, IntPtr.Zero);

					// Let the system process these events.
					Application.DoEvents ();
				}
				else
				{
					// And this is how the same code is done under Linux.
					// (No one should wonder why I prefer Mono to MS Windows .NET ;-)
					MouseEventArgs me = new MouseEventArgs (MouseButtons.Left, 1, x, y, 0);
					DataGridViewCellMouseEventArgs cme = new DataGridViewCellMouseEventArgs (0, 0, x, y, me);
					dgv.OnMouseDownInternal (cme);
					dgv.OnMouseUpInternal (cme);
				}

				// Make sure that created an editing control.
				ComboBox cb = dgv.EditingControl as ComboBox;
				Assert.AreNotEqual (null, cb, "1-1");

				// Make sure that dropped down the menu.
				Assert.AreEqual (true, cb.DroppedDown, "1-2");

				// Close the menu.
				cb.DroppedDown = false;

				// Change the selection on the menu.
				cb.SelectedIndex = 2 /* "Item3" */;

				// Leave the data-grid-view.
				label.Focus ();

				// That should have ended editing and saved the value.
				string cellValue = (string)(dgv.Rows[0].Cells[0].FormattedValue);
				Assert.AreEqual ("Item3", cellValue, "1-3");
			}
			finally
			{
				if (form != null)
					form.Close ();
			}
		}

		// For testing row/column selection.
		List<List<int>> selections;
		void DataGridView_RowSelectionChanged (object sender, EventArgs e)
		{
			// Make a list of selected rows.
			DataGridView dgv = sender as DataGridView;
			List<int> selection = new List<int> ();
			foreach (DataGridViewRow row in dgv.SelectedRows)
				selection.Add (row.Index);
			selections.Add (selection);
		}
		void DataGridView_ColumnSelectionChanged (object sender, EventArgs e)
		{
			// Make a list of selected columns.
			DataGridView dgv = sender as DataGridView;
			List<int> selection = new List<int> ();
			foreach (DataGridViewColumn column in dgv.SelectedColumns)
				selection.Add (column.Index);
			selections.Add (selection);
		}

		// Used to generate printable representation of selections.
		string ListListIntToString (List<List<int>> selections)
		{
			List<string> selectionsList = new List<string> ();
			foreach (List<int> selection in selections)
			{
				List<string> selectionList = new List<string> ();
				foreach (int selectionNo in selection)
					selectionList.Add (selectionNo.ToString ("D"));
				selectionsList.Add ("<" + string.Join (",", selectionList.ToArray()) + ">");
			}
			return string.Join (",", selectionsList.ToArray());

			// (Here is the disallowed Linq version.)
			/* return string.Join (",", (selections.Select ((List<int> x)
				=> "<" + string.Join (",", (x.Select ((int y)
					=> (y.ToString("D")))).ToArray()) + ">").ToArray())); */
		}

		[Test]
		public void SelectedRowsTest ()
		{
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillBig ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

				// Prepare to test the SelectionChanged event.
				selections = new List<List<int>> ();
				List<List<int>> expectedSelections = new List<List<int>> ();
				dgv.SelectionChanged += new EventHandler (DataGridView_RowSelectionChanged);

				// Make sure there's no selection to begin with.
				Assert.AreEqual (0, dgv.SelectedRows.Count, "1-10");

				// Select a row.
				dgv.Rows [1].Selected = true;
				Assert.AreEqual (1, dgv.SelectedRows.Count, "1-1");
				Assert.AreEqual (1, dgv.SelectedRows [0].Index, "1-2");
				expectedSelections.Add (new List<int> { 1 });

				// Select another row.
				dgv.Rows [3].Selected = true;
				Assert.AreEqual (2, dgv.SelectedRows.Count, "1-3");
				Assert.AreEqual (3, dgv.SelectedRows [0].Index, "1-4");
				Assert.AreEqual (1, dgv.SelectedRows [1].Index, "1-5");
				expectedSelections.Add (new List<int> { 3, 1 });

				// Select another row.
				dgv.Rows [2].Selected = true;
				Assert.AreEqual (3, dgv.SelectedRows.Count, "1-6");
				Assert.AreEqual (2, dgv.SelectedRows [0].Index, "1-7");
				Assert.AreEqual (3, dgv.SelectedRows [1].Index, "1-8");
				Assert.AreEqual (1, dgv.SelectedRows [2].Index, "1-9");
				expectedSelections.Add (new List<int> { 2, 3, 1 });

				// Unselect a row.
				dgv.Rows [2].Selected = false;
				Assert.AreEqual (2, dgv.SelectedRows.Count, "1-11");
				Assert.AreEqual (3, dgv.SelectedRows [0].Index, "1-12");
				Assert.AreEqual (1, dgv.SelectedRows [1].Index, "1-13");
				expectedSelections.Add (new List<int> { 3, 1 });

				// Delete a row.
				// Since the row wasn't selected, it doesn't fire a
				// SelectionChanged event.
				dgv.Rows.RemoveAt (2);
				Assert.AreEqual (2, dgv.SelectedRows.Count, "1-14");
				Assert.AreEqual (2, dgv.SelectedRows [0].Index, "1-16");
				Assert.AreEqual (1, dgv.SelectedRows [1].Index, "1-17");

				// Delete a selected row.
				dgv.Rows.RemoveAt (2);
				Assert.AreEqual (1, dgv.SelectedRows.Count, "1-18");
				Assert.AreEqual (1, dgv.SelectedRows [0].Index, "1-19");
				expectedSelections.Add (new List<int> { 1 });

				// Make sure the SelectionChanged event was called when expected.
				string selectionsText = ListListIntToString (selections);
				string expectedSelectionsText = ListListIntToString (expectedSelections);
				Assert.AreEqual (expectedSelectionsText, selectionsText, "1-15");
			}

			using (DataGridView dgv = DataGridViewCommon.CreateAndFillBig ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
				dgv.Rows [1].Selected = true;
				Assert.AreEqual (0, dgv.SelectedRows.Count, "3-1");
				dgv.Rows [3].Selected = true;
				Assert.AreEqual (0, dgv.SelectedRows.Count, "3-3");
				dgv.Rows [2].Selected = true;
				Assert.AreEqual (0, dgv.SelectedRows.Count, "3-6");
			}

			using (DataGridView dgv = DataGridViewCommon.CreateAndFillBig ()) {
				foreach (DataGridViewColumn col in dgv.Columns)
					col.SortMode = DataGridViewColumnSortMode.NotSortable;
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				dgv.Rows [1].Selected = true;
				Assert.AreEqual (0, dgv.SelectedRows.Count, "4-1");
				dgv.Rows [3].Selected = true;
				Assert.AreEqual (0, dgv.SelectedRows.Count, "4-3");
				dgv.Rows [2].Selected = true;
				Assert.AreEqual (0, dgv.SelectedRows.Count, "4-6");
			}

			using (DataGridView dgv = DataGridViewCommon.CreateAndFillBig ()) {
				foreach (DataGridViewColumn col in dgv.Columns)
					col.SortMode = DataGridViewColumnSortMode.NotSortable;
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
				dgv.Rows [1].Selected = true;
				Assert.AreEqual (0, dgv.SelectedRows.Count, "5-1");
				dgv.Rows [3].Selected = true;
				Assert.AreEqual (0, dgv.SelectedRows.Count, "5-3");
				dgv.Rows [2].Selected = true;
				Assert.AreEqual (0, dgv.SelectedRows.Count, "5-6");
			}
		}

		[Test] // bug #325979
		public void SelectedRows_FindColumnByName ()
		{
			DataTable dt = new DataTable ();
			dt.Columns.Add ("Date", typeof (DateTime));
			dt.Columns.Add ("Registered", typeof (bool));
			dt.Columns.Add ("Event", typeof (string));

			DataRow row = dt.NewRow ();
			row ["Date"] = new DateTime (2007, 2, 3);
			row ["Event"] = "one";
			row ["Registered"] = false;
			dt.Rows.Add (row);

			row = dt.NewRow ();
			row ["Date"] = new DateTime (2008, 3, 4);
			row ["Event"] = "two";
			row ["Registered"] = true;
			dt.Rows.Add (row);

			DataGridView dgv = new DataGridView ();
			dgv.DataSource = dt;

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (dgv);
			form.Show ();

			dgv.Rows [1].Selected = true;

			DataGridViewCell cell = dgv.SelectedRows [0].Cells ["DaTE"];
			Assert.IsNotNull (cell, "#A1");
			Assert.IsNotNull (cell.OwningColumn, "#A2");
			Assert.AreEqual ("Date", cell.OwningColumn.Name, "#A3");
			Assert.IsNotNull (cell.Value, "#A4");
			Assert.AreEqual (new DateTime (2008, 3, 4), cell.Value, "#A5");

			cell = dgv.SelectedRows [0].Cells ["Event"];
			Assert.IsNotNull (cell, "#B1");
			Assert.IsNotNull (cell.OwningColumn, "#B2");
			Assert.AreEqual ("Event", cell.OwningColumn.Name, "#B3");
			Assert.IsNotNull (cell.Value, "#B3");
			Assert.AreEqual ("two", cell.Value, "#B4");

			form.Dispose ();
		}

		[Test]
		public void SelectedColumnsTest ()
		{
			using (DataGridView dgv = DataGridViewCommon.CreateAndFillBig ()) {
				foreach (DataGridViewColumn col in dgv.Columns)
					col.SortMode = DataGridViewColumnSortMode.NotSortable;
				dgv.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;

				// Prepare to test the SelectionChanged event.
				selections = new List<List<int>> ();
				List<List<int>> expectedSelections = new List<List<int>> ();
				dgv.SelectionChanged += new EventHandler (DataGridView_ColumnSelectionChanged);

				// Make sure there's no selection to begin with.
				Assert.AreEqual (0, dgv.SelectedColumns.Count, "1-13");

				// Select a column.
				dgv.Columns [1].Selected = true;
				Assert.AreEqual (1, dgv.SelectedColumns.Count, "1-1");
				Assert.AreEqual (1, dgv.SelectedColumns [0].Index, "1-2");
				expectedSelections.Add (new List<int> { 1 });

				// Select another column.
				dgv.Columns [3].Selected = true;
				Assert.AreEqual (2, dgv.SelectedColumns.Count, "1-3");
				Assert.AreEqual (3, dgv.SelectedColumns [0].Index, "1-4");
				Assert.AreEqual (1, dgv.SelectedColumns [1].Index, "1-5");
				expectedSelections.Add (new List<int> { 3, 1 });

				// Select another column.
				dgv.Columns [2].Selected = true;
				Assert.AreEqual (3, dgv.SelectedColumns.Count, "1-6");
				Assert.AreEqual (2, dgv.SelectedColumns [0].Index, "1-7");
				Assert.AreEqual (3, dgv.SelectedColumns [1].Index, "1-8");
				Assert.AreEqual (1, dgv.SelectedColumns [2].Index, "1-9");
				expectedSelections.Add (new List<int> { 2, 3, 1 });

				// Unselect a column.
				dgv.Columns [2].Selected = false;
				Assert.AreEqual (2, dgv.SelectedColumns.Count, "1-10");
				Assert.AreEqual (3, dgv.SelectedColumns [0].Index, "1-11");
				Assert.AreEqual (1, dgv.SelectedColumns [1].Index, "1-12");
				expectedSelections.Add (new List<int> { 3, 1 });

				// Delete a column.
				// Since the column wasn't selected, it doesn't fire a
				// SelectionChanged event.
				dgv.Columns.RemoveAt (2);
				Assert.AreEqual (2, dgv.SelectedColumns.Count, "1-14");
				Assert.AreEqual (2, dgv.SelectedColumns [0].Index, "1-16");
				Assert.AreEqual (1, dgv.SelectedColumns [1].Index, "1-17");

				// Delete a selected column.
				dgv.Columns.RemoveAt (2);
				Assert.AreEqual (1, dgv.SelectedColumns.Count, "1-18");
				Assert.AreEqual (1, dgv.SelectedColumns [0].Index, "1-19");
				expectedSelections.Add (new List<int> { 1 });

				// Make sure the SelectionChanged event was called when expected.
				string selectionsText = ListListIntToString (selections);
				string expectedSelectionsText = ListListIntToString (expectedSelections);
				Assert.AreEqual (expectedSelectionsText, selectionsText, "1-15");
			}

			using (DataGridView dgv = DataGridViewCommon.CreateAndFillBig ()) {
				foreach (DataGridViewColumn col in dgv.Columns)
					col.SortMode = DataGridViewColumnSortMode.NotSortable;
				dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;

				// Prepare to test the SelectionChanged event.
				selections = new List<List<int>> ();
				List<List<int>> expectedSelections = new List<List<int>> ();
				dgv.SelectionChanged += new EventHandler (DataGridView_ColumnSelectionChanged);

				// Make sure there's no selection to begin with.
				Assert.AreEqual (0, dgv.SelectedColumns.Count, "2-10");

				// Select a column.
				dgv.Columns [1].Selected = true;
				Assert.AreEqual (1, dgv.SelectedColumns.Count, "2-1");
				Assert.AreEqual (1, dgv.SelectedColumns [0].Index, "2-2");
				expectedSelections.Add (new List<int> { 1 });

				// Select another column.
				dgv.Columns [3].Selected = true;
				Assert.AreEqual (2, dgv.SelectedColumns.Count, "2-3");
				Assert.AreEqual (3, dgv.SelectedColumns [0].Index, "2-4");
				Assert.AreEqual (1, dgv.SelectedColumns [1].Index, "2-5");
				expectedSelections.Add (new List<int> { 3, 1 });

				// Select another column.
				dgv.Columns [2].Selected = true;
				Assert.AreEqual (3, dgv.SelectedColumns.Count, "2-6");
				Assert.AreEqual (2, dgv.SelectedColumns [0].Index, "2-7");
				Assert.AreEqual (3, dgv.SelectedColumns [1].Index, "2-8");
				Assert.AreEqual (1, dgv.SelectedColumns [2].Index, "2-9");
				expectedSelections.Add (new List<int> { 2, 3, 1 });

				// Unselect another column.
				dgv.Columns [2].Selected = false;
				Assert.AreEqual (2, dgv.SelectedColumns.Count, "2-11");
				Assert.AreEqual (3, dgv.SelectedColumns [0].Index, "2-12");
				Assert.AreEqual (1, dgv.SelectedColumns [1].Index, "2-13");
				expectedSelections.Add (new List<int> { 3, 1 });

				// Make sure the SelectionChanged event was called when expected.
				string selectionsText = ListListIntToString (selections);
				string expectedSelectionsText = ListListIntToString (expectedSelections);
				Assert.AreEqual (expectedSelectionsText, selectionsText, "2-14");
			}

			using (DataGridView dgv = DataGridViewCommon.CreateAndFillBig ()) {
				dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
				dgv.Columns [1].Selected = true;
				Assert.AreEqual (0, dgv.SelectedColumns.Count, "3-1");
				dgv.Columns [3].Selected = true;
				Assert.AreEqual (0, dgv.SelectedColumns.Count, "3-3");
				dgv.Columns [2].Selected = true;
				Assert.AreEqual (0, dgv.SelectedColumns.Count, "3-6");
			}

			using (DataGridView dgv = DataGridViewCommon.CreateAndFillBig ()) {
				foreach (DataGridViewColumn col in dgv.Columns)
					col.SortMode = DataGridViewColumnSortMode.NotSortable;
				dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				dgv.Columns [1].Selected = true;
				Assert.AreEqual (0, dgv.SelectedColumns.Count, "4-1");
				dgv.Columns [3].Selected = true;
				Assert.AreEqual (0, dgv.SelectedColumns.Count, "4-3");
				dgv.Columns [2].Selected = true;
				Assert.AreEqual (0, dgv.SelectedColumns.Count, "4-6");
			}

			using (DataGridView dgv = DataGridViewCommon.CreateAndFillBig ()) {
				foreach (DataGridViewColumn col in dgv.Columns)
					col.SortMode = DataGridViewColumnSortMode.NotSortable;
				dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgv.Columns [1].Selected = true;
				Assert.AreEqual (0, dgv.SelectedColumns.Count, "5-1");
				dgv.Columns [3].Selected = true;
				Assert.AreEqual (0, dgv.SelectedColumns.Count, "5-3");
				dgv.Columns [2].Selected = true;
				Assert.AreEqual (0, dgv.SelectedColumns.Count, "5-6");
			}
		}

		[Test]
		public void TopLeftHeaderCellTest ()
		{
			Assert.Ignore("Missing quite a few bits still");
			
			using (DataGridView dgv = new DataGridView ()) {
				DataGridViewHeaderCell cell = dgv.TopLeftHeaderCell;
				
				cell = dgv.TopLeftHeaderCell;

				Assert.IsNotNull (cell, "#01");
				Assert.AreEqual (cell.DataGridView, dgv, "#02");
				Assert.AreEqual ("DataGridViewTopLeftHeaderCell", cell.GetType ().Name, "#03");
								
				Assert.IsNotNull (cell.AccessibilityObject, "#cell.AccessibilityObject");
				Assert.AreEqual (-1, cell.ColumnIndex, "#cell.ColumnIndex");
				// /* NIE for the moment... */ Assert.IsNotNull (cell.ContentBounds, "#cell.ContentBounds");
				Assert.IsNull (cell.ContextMenuStrip, "#cell.ContextMenuStrip");
				Assert.IsNotNull (cell.DataGridView, "#cell.DataGridView");
				Assert.IsNull (cell.DefaultNewRowValue, "#cell.DefaultNewRowValue");
				Assert.AreEqual (false, cell.Displayed, "#cell.Displayed");
				// /* NIE for the moment... */ Assert.AreEqual (@"", cell.EditedFormattedValue, "#cell.EditedFormattedValue");
				Assert.IsNotNull (cell.EditType, "#cell.EditType");
				Assert.IsNotNull (cell.ErrorIconBounds, "#cell.ErrorIconBounds");
				Assert.AreEqual (@"", cell.ErrorText, "#cell.ErrorText");
				// /* NIE for the moment... */ Assert.AreEqual (@"", cell.FormattedValue, "#cell.FormattedValue");
				Assert.IsNotNull (cell.FormattedValueType, "#cell.FormattedValueType");
				// /* NIE for the moment... */ Assert.AreEqual (true, cell.Frozen, "#cell.Frozen");
				Assert.AreEqual (false, cell.HasStyle, "#cell.HasStyle");
				Assert.AreEqual (DataGridViewElementStates.Frozen | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Resizable | DataGridViewElementStates.ResizableSet | DataGridViewElementStates.Visible, cell.InheritedState, "#cell.InheritedState");
				Assert.IsNotNull (cell.InheritedStyle, "#cell.InheritedStyle");
				try {
					object zxf = cell.IsInEditMode;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#cell.IsInEditMode");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Operation cannot be performed on a cell of a shared row.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#cell.IsInEditMode");
				}
				Assert.IsNull (cell.OwningColumn, "#cell.OwningColumn");
				Assert.IsNull (cell.OwningRow, "#cell.OwningRow");
				Assert.IsNotNull (cell.PreferredSize, "#cell.PreferredSize");
				Assert.AreEqual (true, cell.ReadOnly, "#cell.ReadOnly");
				Assert.AreEqual (true, cell.Resizable, "#cell.Resizable");
				Assert.AreEqual (-1, cell.RowIndex, "#cell.RowIndex");
				Assert.AreEqual (false, cell.Selected, "#cell.Selected");
				Assert.IsNotNull (cell.Size, "#cell.Size");
				Assert.AreEqual (DataGridViewElementStates.None, cell.State, "#cell.State");
				if (cell.HasStyle)
					Assert.IsNotNull (cell.Style, "#cell.Style");
				Assert.IsNull (cell.Tag, "#cell.Tag");
				Assert.AreEqual (@"", cell.ToolTipText, "#cell.ToolTipText");
				Assert.IsNull (cell.Value, "#cell.Value");
				Assert.IsNotNull (cell.ValueType, "#cell.ValueType");
				Assert.AreEqual (true, cell.Visible, "#cell.Visible");
			}
		}

		[Test]
		public void TestDefaultValues ()
		{
			DataGridView grid = new DataGridView ();
			Assert.AreEqual (true, grid.AllowUserToAddRows, "#A1");
			Assert.AreEqual (true, grid.AllowUserToDeleteRows, "#A2");
			Assert.AreEqual (false, grid.AllowUserToOrderColumns, "#A3");
			Assert.AreEqual (true, grid.AllowUserToResizeColumns, "#A4");
			Assert.AreEqual (true, grid.AllowUserToResizeRows, "#A5");
			Assert.AreEqual (new DataGridViewCellStyle(), grid.AlternatingRowsDefaultCellStyle, "#A6");
			Assert.AreEqual (true, grid.AutoGenerateColumns, "#A7");
			Assert.AreEqual (DataGridViewAutoSizeRowsMode.None, grid.AutoSizeRowsMode, "#A8");
			Assert.AreEqual (Control.DefaultBackColor, grid.BackColor, "#A9");
			Assert.AreEqual (SystemColors.AppWorkspace, grid.BackgroundColor, "#A10");
			Assert.AreEqual (BorderStyle.FixedSingle, grid.BorderStyle, "#A11");
			Assert.AreEqual (DataGridViewClipboardCopyMode.EnableWithAutoHeaderText, grid.ClipboardCopyMode, "#A12");
			Assert.AreEqual (DataGridViewColumnHeadersHeightSizeMode.EnableResizing, grid.ColumnHeadersHeightSizeMode, "#A21");
			Assert.AreEqual (true, grid.ColumnHeadersVisible, "#A22");
			Assert.AreEqual (String.Empty, grid.DataMember, "#A23");
			Assert.AreEqual (DataGridViewEditMode.EditOnKeystrokeOrF2, grid.EditMode, "#A31");
			Assert.AreEqual (Control.DefaultFont, grid.Font, "#A32");
			Assert.AreEqual (Control.DefaultForeColor, grid.ForeColor, "#A33");
			Assert.AreEqual (Color.FromKnownColor(KnownColor.ControlDark), grid.GridColor, "#A34");
			Assert.AreEqual (true, grid.MultiSelect, "#A35");
			Assert.AreEqual (grid.Rows.Count - 1, grid.NewRowIndex, "#A36");
			Assert.AreEqual (Padding.Empty, grid.Padding, "#A37");
			Assert.AreEqual (false, grid.ReadOnly, "#A38");
			Assert.AreEqual (true, grid.RowHeadersVisible, "#A39");
			Assert.AreEqual (41, grid.RowHeadersWidth, "#A40");
			Assert.AreEqual (DataGridViewSelectionMode.RowHeaderSelect, grid.SelectionMode, "#A41");
			Assert.AreEqual (true, grid.ShowCellErrors, "#A42");
			Assert.AreEqual (true, grid.ShowEditingIcon, "#A43");
			Assert.AreEqual (Cursors.Default, grid.UserSetCursor, "#A44");
			Assert.AreEqual (false, grid.VirtualMode, "#A45");
		}

#region AutoSizeColumnsModeExceptions

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void TestAutoSizeColumnsModeInvalidEnumArgumentException ()
		{
			DataGridView grid = new DataGridView();
			grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill | DataGridViewAutoSizeColumnsMode.None;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAutoSizeColumnsModeInvalidOperationException1 ()
		{
			DataGridView grid = new DataGridView ();
			grid.ColumnHeadersVisible = false;
			DataGridViewColumn col = new DataGridViewColumn ();
			col.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
			grid.Columns.Add (col);
			grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAutoSizeColumnsModeInvalidOperationException2 ()
		{
			DataGridView grid = new DataGridView ();
			DataGridViewColumn col = new DataGridViewColumn ();
			col.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
			col.Frozen = true;
			grid.Columns.Add (col);
			grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
		}

#endregion

#region AutoSizeRowsModeExceptions

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void TestAutoSizeRowsModeInvalidEnumArgumentException ()
		{
			DataGridView grid = new DataGridView ();
			grid.AutoSizeRowsMode = (DataGridViewAutoSizeRowsMode) 4;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAutoSizeRowsModeInvalidOperationException1 ()
		{
			DataGridView grid = new DataGridView ();
			grid.RowHeadersVisible = false;
			grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllHeaders;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAutoSizeRowsModeInvalidOperationException2 ()
		{
			DataGridView grid = new DataGridView ();
			grid.RowHeadersVisible = false;
			grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedHeaders;
		}

#endregion

		[Test]
		public void AutoResizeColumTest ()
		{
			using (Form f = new Form ()) {
				f.Show ();
				using (DataGridView dgv = new DataGridView ()) {
					f.Controls.Add (dgv);
					
					DataGridViewColumn col, col2, col3;
					
					Assert.AreEqual ("{Width=240, Height=150}", dgv.ClientSize.ToString (), "#01");
					
					col = new DataGridViewColumn ();
					col.MinimumWidth = 20;
					col.FillWeight = 20;
					col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
					col.CellTemplate = new DataGridViewTextBoxCell ();
					
					Assert.AreEqual (100, col.Width, "#02");
					dgv.Columns.Add (col);
					
					Assert.AreEqual (197, col.Width, "#03");

					col2 = new DataGridViewColumn ();
					col2.MinimumWidth = 20;
					col2.FillWeight = 40;
					col2.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
					col2.CellTemplate = new DataGridViewTextBoxCell (); ;
					dgv.Columns.Add (col2);

					Assert.AreEqual (66, col.Width, "#04");
					Assert.AreEqual (131, col2.Width, "#05");

					col3 = new DataGridViewColumn ();
					col3.MinimumWidth = 20;
					col3.FillWeight = 5;
					col3.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
					col3.CellTemplate = new DataGridViewTextBoxCell (); ;
					dgv.Columns.Add (col3);

					Assert.AreEqual (59, col.Width, "#04");
					Assert.AreEqual (118, col2.Width, "#05");
					Assert.AreEqual (20, col3.Width, "#05");
				}
			}
		}

		[Test]
		public void ControlsTest ()
		{
			using (DataGridView grid = new DataGridView ()) {
				Assert.AreEqual ("DataGridViewControlCollection", grid.Controls.GetType ().Name, "#01");
				Assert.AreEqual (2, grid.Controls.Count, "#02");
			}
		}
	
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestBackgroundColorArgumentException ()
		{
			DataGridView grid = new DataGridView ();
			grid.BackgroundColor = Color.Empty;
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestBorderStyleInvalidEnumArgumentException ()
		{
			DataGridView grid = new DataGridView ();
			grid.BorderStyle = BorderStyle.FixedSingle | BorderStyle.Fixed3D;
		}

		[Test]
		public void ColumnCount ()
		{
			DataGridView dgv = new DataGridView ();

			dgv.RowCount = 10;
			dgv.ColumnCount = 2;

			Assert.AreEqual (10, dgv.RowCount, "A1");
			Assert.AreEqual (2, dgv.ColumnCount, "A2");

			dgv.ColumnCount = 1;

			Assert.AreEqual (10, dgv.RowCount, "B1");
			Assert.AreEqual (1, dgv.ColumnCount, "B2");

			dgv.ColumnCount = 3;

			Assert.AreEqual (10, dgv.RowCount, "C1");
			Assert.AreEqual (3, dgv.ColumnCount, "C2");


			dgv.ColumnCount = 0;

			Assert.AreEqual (0, dgv.RowCount, "D1");
			Assert.AreEqual (0, dgv.ColumnCount, "D2");

			Assert.AreEqual (0, dgv.ColumnCount, "E1");

			try {
				dgv.ColumnCount = -1;
				Assert.Fail ("F1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "F2");
				Assert.IsNotNull (ex.Message, "F3");
				Assert.IsNotNull (ex.ParamName, "F4");
				Assert.AreEqual ("ColumnCount", ex.ParamName, "F5");
				Assert.IsNull (ex.InnerException, "F6");
			}
		}

		[Test]
		public void ColumnCountIncrease ()
		{
			DataGridView dgv = new DataGridView ();
			dgv.ColumnCount = 1;
			
			// Increasing the ColumnCount adds TextBoxColumns, not generic columns
			Assert.AreEqual ("System.Windows.Forms.DataGridViewTextBoxColumn", dgv.Columns[0].GetType ().ToString (), "A1");
		}


		[Test]
		public void ColumnCountDecrease ()
		{
			DataGridView dgv = new DataGridView ();
			dgv.ColumnCount = 6;
			Assert.AreEqual (6, dgv.ColumnCount, "A1");

			dgv.ColumnCount = 3;
			Assert.AreEqual (3, dgv.ColumnCount, "A2");
			
			// Increasing the ColumnCount adds TextBoxColumns, not generic columns
			Assert.AreEqual ("System.Windows.Forms.DataGridViewTextBoxColumn", dgv.Columns[0].GetType ().ToString (), "A3");
			Assert.AreEqual ("System.Windows.Forms.DataGridViewTextBoxColumn", dgv.Columns[1].GetType ().ToString (), "A4");
			Assert.AreEqual ("System.Windows.Forms.DataGridViewTextBoxColumn", dgv.Columns[2].GetType ().ToString (), "A5");
		}

		private class DataItem
		{
			public string Text {
				get { return String.Empty; }
			}

			[Browsable (false)]
			public string NotVisible {
				get { return String.Empty; }
			}
		}

		[Test]
		public void NoDuplicateAutoGeneratedColumn ()
		{
			List<DataItem> dataList = new List<DataItem> ();
			dataList.Add (new DataItem ());
			dataList.Add (new DataItem ());

			DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn ();
			column.DataPropertyName = "Text";
			column.HeaderText = "Custom Column";
			grid.Columns.Add (column);

			grid.DataSource = dataList;
			// Test that the column autogeneration hasn't generated duplicate column 
			// for the property Text
			Assert.AreEqual (1, grid.Columns.Count, "#1");
			Assert.AreEqual ("Custom Column", grid.Columns[0].HeaderText, "#2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestColumnCountInvalidOperationException ()
		{
			DataGridView grid = new DataGridView ();
			grid.DataSource = new ArrayList ();
			grid.ColumnCount = 0;
		}

		[Test]
		public void ColumnHeadersHeight ()
		{
			DataGridView grid = new DataGridView ();
			Assert.AreEqual (23, grid.ColumnHeadersHeight, "#A1");
			grid.ColumnHeadersHeight = 4;
			Assert.AreEqual (4, grid.ColumnHeadersHeight, "#A2");
			grid.ColumnHeadersHeight = 32768;
			Assert.AreEqual (32768, grid.ColumnHeadersHeight, "#A3");

			try {
				grid.ColumnHeadersHeight = 3;
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNotNull (ex.ParamName, "#B4");
				Assert.AreEqual ("ColumnHeadersHeight", ex.ParamName, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}

			try {
				grid.ColumnHeadersHeight = 32769;
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsNotNull (ex.ParamName, "#C4");
				Assert.AreEqual ("ColumnHeadersHeight", ex.ParamName, "#C5");
				Assert.IsNull (ex.InnerException, "#C6");
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void TestColumnHeadersHeightSizeModeInvalidEnumArgumentException ()
		{
			DataGridView grid = new DataGridView ();
			grid.ColumnHeadersHeightSizeMode = (DataGridViewColumnHeadersHeightSizeMode) 3;
		}

		[Test]
		public void RowHeadersWidth ()
		{
			DataGridView grid = new DataGridView();
			Assert.AreEqual (41, grid.RowHeadersWidth, "#A1");
			grid.RowHeadersWidth = 4;
			Assert.AreEqual (4, grid.RowHeadersWidth, "#A2");
			grid.RowHeadersWidth = 32768;
			Assert.AreEqual (32768, grid.RowHeadersWidth, "#A3");

			try {
				grid.RowHeadersWidth = 3;
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNotNull (ex.ParamName, "#B4");
				Assert.AreEqual ("RowHeadersWidth", ex.ParamName, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}

			try {
				grid.RowHeadersWidth = 32769;
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsNotNull (ex.ParamName, "#C4");
				Assert.AreEqual ("RowHeadersWidth", ex.ParamName, "#C5");
				Assert.IsNull (ex.InnerException, "#C6");
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void TestDataGridViewRowHeadersWidthSizeModeInvalidEnumArgumentException () {
			DataGridView grid = new DataGridView ();
			grid.RowHeadersWidthSizeMode = (DataGridViewRowHeadersWidthSizeMode) 5;
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void TestScrollBarsInvalidEnumArgumentException ()
		{
			DataGridView grid = new DataGridView ();
			grid.ScrollBars = (ScrollBars) 4;
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void TestSelectionModeInvalidEnumArgumentException ()
		{
			DataGridView grid = new DataGridView ();
			grid.SelectionMode = (DataGridViewSelectionMode) 5;
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void TestAutoResizeRowsInvalidEnumArgumentException ()
		{
			DataGridView grid = new DataGridView ();
			grid.AutoResizeRows ((DataGridViewAutoSizeRowsMode) 4);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAutoResizeRowsInvalidOperationException1 ()
		{
			DataGridView grid = new DataGridView ();
			grid.RowHeadersVisible = false;
			grid.AutoResizeRows (DataGridViewAutoSizeRowsMode.AllHeaders);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAutoResizeRowsInvalidOperationException2 ()
		{
			DataGridView grid = new DataGridView ();
			grid.RowHeadersVisible = false;
			grid.AutoResizeRows (DataGridViewAutoSizeRowsMode.DisplayedHeaders);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestAutoResizeRowsArgumentException ()
		{
			DataGridView grid = new DataGridView ();
			grid.AutoResizeRows (DataGridViewAutoSizeRowsMode.None);
		}

		[Test]
		public void DefaultSize ()
		{
			MockDataGridView grid = new MockDataGridView ();
			Assert.AreEqual (new Size (240, 150), grid.default_size, "#1");
			Assert.AreEqual (new Size (240, 150), grid.Size, "#2");
		}

		[Test]
		public void ColumnHeadersDefaultCellStyle ()
		{
			DataGridView grid = new DataGridView();
			Assert.AreEqual (SystemColors.Control, grid.ColumnHeadersDefaultCellStyle.BackColor, "#A1");
			Assert.AreEqual (SystemColors.ControlText,  grid.ColumnHeadersDefaultCellStyle.ForeColor, "#A2");
			Assert.AreEqual (SystemColors.Highlight, grid.ColumnHeadersDefaultCellStyle.SelectionBackColor, "#A3");
			Assert.AreEqual (SystemColors.HighlightText, grid.ColumnHeadersDefaultCellStyle.SelectionForeColor, "#A4");
			Assert.AreSame (grid.Font, grid.ColumnHeadersDefaultCellStyle.Font, "#A5");
			Assert.AreEqual (DataGridViewContentAlignment.MiddleLeft, grid.ColumnHeadersDefaultCellStyle.Alignment, "#A6");
			Assert.AreEqual (DataGridViewTriState.True, grid.ColumnHeadersDefaultCellStyle.WrapMode, "#A7");
		}

		[Test]
		public void DefaultCellStyle ()
		{
			DataGridView grid = new DataGridView();
			Assert.AreEqual (SystemColors.Window, grid.DefaultCellStyle.BackColor, "#A1");
			Assert.AreEqual (SystemColors.WindowText,  grid.DefaultCellStyle.ForeColor, "#A2");
			Assert.AreEqual (SystemColors.Highlight, grid.DefaultCellStyle.SelectionBackColor, "#A3");
			Assert.AreEqual (SystemColors.HighlightText, grid.DefaultCellStyle.SelectionForeColor, "#A4");
			Assert.AreSame (grid.Font, grid.DefaultCellStyle.Font, "#A5");
			Assert.AreEqual (DataGridViewContentAlignment.MiddleLeft, grid.DefaultCellStyle.Alignment, "#A6");
			Assert.AreEqual (DataGridViewTriState.False, grid.DefaultCellStyle.WrapMode, "#A7");
		}

		[Test]
		public void MethodIsInputKey ()
		{
			string result = string.Empty;
			string expected = "13;13;33;33;34;34;35;36;37;38;39;40;46;48;96;113;";

			ExposeProtectedProperties dgv = new ExposeProtectedProperties ();
		
			foreach (Keys k in Enum.GetValues (typeof (Keys)))
				if (dgv.PublicIsInputKey (k))
					result += ((int)k).ToString () + ";";

			Assert.AreEqual (expected, result, "A1");
		}

		[Test]
		public void MethodIsInputChar ()
		{
			bool result = false;

			ExposeProtectedProperties dgv = new ExposeProtectedProperties ();

			for (int i = 0; i < 255; i++)
				if (!dgv.PublicIsInputChar ((char)i))
					result = true;

			// Basically, it always returns true
			Assert.AreEqual (false, result, "A1");
		}

		[Test]
		public void RowsDefaultCellStyle ()
		{
			DataGridView grid = new DataGridView();
			Assert.AreEqual (Color.Empty, grid.RowsDefaultCellStyle.BackColor, "#A1");
			Assert.AreEqual (Color.Empty, grid.RowsDefaultCellStyle.ForeColor, "#A2");
			Assert.AreEqual (Color.Empty, grid.RowsDefaultCellStyle.SelectionBackColor, "#A3");
			Assert.AreEqual (Color.Empty, grid.RowsDefaultCellStyle.SelectionForeColor, "#A4");
			Assert.IsNull(grid.RowsDefaultCellStyle.Font, "#A5");
			Assert.AreEqual (DataGridViewContentAlignment.NotSet, grid.RowsDefaultCellStyle.Alignment, "#A6");
			Assert.AreEqual (DataGridViewTriState.NotSet, grid.RowsDefaultCellStyle.WrapMode, "#A7");
		}

		[Test]
		public void RowHeadersDefaultCellStyle ()
		{
			DataGridView grid = new DataGridView();
			Assert.AreEqual (SystemColors.Control, grid.RowHeadersDefaultCellStyle.BackColor, "#A1");
			Assert.AreEqual (SystemColors.ControlText, grid.RowHeadersDefaultCellStyle.ForeColor, "#A2");
			Assert.AreEqual (SystemColors.Highlight, grid.RowHeadersDefaultCellStyle.SelectionBackColor, "#A3");
			Assert.AreEqual (SystemColors.HighlightText, grid.RowHeadersDefaultCellStyle.SelectionForeColor, "#A4");
			Assert.AreSame (grid.Font, grid.RowHeadersDefaultCellStyle.Font, "#A5");
			Assert.AreEqual (DataGridViewContentAlignment.MiddleLeft, grid.RowHeadersDefaultCellStyle.Alignment, "#A6");
			Assert.AreEqual (DataGridViewTriState.True, grid.RowHeadersDefaultCellStyle.WrapMode, "#A7");
		}

		private class MockDataGridView : DataGridView
		{
			public Size default_size {
				get { return base.DefaultSize; }
			}
		}

		[Test]
		public void bug_82326 ()
		{
			using (Form f = new Form ()) {
				DataGridView _dataGrid;
				DataGridViewTextBoxColumn _column;

				_dataGrid = new DataGridView ();
				_column = new DataGridViewTextBoxColumn ();
				f.SuspendLayout ();
				((ISupportInitialize)(_dataGrid)).BeginInit ();
				// 
				// _dataGrid
				// 
				_dataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
				_dataGrid.Columns.Add (_column);
				_dataGrid.RowTemplate.Height = 21;
				_dataGrid.Location = new Point (12, 115);
				_dataGrid.Size = new Size (268, 146);
				_dataGrid.TabIndex = 0;
				// 
				// _column
				// 
				_column.HeaderText = "Column";
				// 
				// MainForm
				// 
				f.ClientSize = new Size (292, 273);
				f.Controls.Add (_dataGrid);
				((ISupportInitialize)(_dataGrid)).EndInit ();
				f.ResumeLayout (false);
				f.Load += delegate (object sender, EventArgs e) { ((Control)sender).FindForm ().Close (); };

				Application.Run (f);
			}
		}
		
		[Test]
		public void RowCountIncrease ()
		{
			DataGridView dgv = new DataGridView ();
			dgv.RowCount = 3;
			
			Assert.AreEqual (3, dgv.RowCount, "A1");
			Assert.AreEqual (1, dgv.ColumnCount, "A2");

			Assert.AreEqual (0, dgv.Rows[0].Index, "A3");
			Assert.AreEqual (1, dgv.Rows[1].Index, "A4");
			Assert.AreEqual (2, dgv.Rows[2].Index, "A5");


			dgv.RowCount = 2;
			
			Assert.AreEqual (2, dgv.RowCount, "B1");
			Assert.AreEqual (1, dgv.ColumnCount, "B2");

			Assert.AreEqual (0, dgv.Rows[0].Index, "B3");
			Assert.AreEqual (1, dgv.Rows[1].Index, "B4");

			dgv.RowCount = 6;
			
			Assert.AreEqual (6, dgv.RowCount, "C1");
			Assert.AreEqual (1, dgv.ColumnCount, "C2");

			Assert.AreEqual (0, dgv.Rows[0].Index, "C3");
			Assert.AreEqual (1, dgv.Rows[1].Index, "C4");
			Assert.AreEqual (2, dgv.Rows[2].Index, "C5");

			dgv.AllowUserToAddRows = false;

			Assert.AreEqual (5, dgv.RowCount, "D1");
			Assert.AreEqual (1, dgv.ColumnCount, "D2");

			dgv.RowCount = 1;
			
			Assert.AreEqual (1, dgv.RowCount, "E1");
			Assert.AreEqual (1, dgv.ColumnCount, "E2");

			Assert.AreEqual (0, dgv.Rows[0].Index, "E3");

			dgv.RowCount = 8;
			
			Assert.AreEqual (8, dgv.RowCount, "F1");
			Assert.AreEqual (1, dgv.ColumnCount, "F2");

			Assert.AreEqual (0, dgv.Rows[0].Index, "F3");
			Assert.AreEqual (1, dgv.Rows[1].Index, "F4");
		}

		[Test]
		public void RowCountDecrease ()
		{
			DataGridView dgv = new DataGridView ();
			dgv.RowCount = 6;
			
			Assert.AreEqual (6, dgv.RowCount, "A1");
			Assert.AreEqual (1, dgv.ColumnCount, "A2");

			dgv.RowCount = 3;
			Assert.AreEqual (3, dgv.RowCount, "A3");
			Assert.AreEqual (0, dgv.Rows[0].Index, "A4");
			Assert.AreEqual (1, dgv.Rows[1].Index, "A5");
			Assert.AreEqual (2, dgv.Rows[2].Index, "A6");

			try {
				dgv.RowCount = 0;
				Assert.Fail ("C1");
			} catch {}


			dgv.RowCount = 6;
			
			Assert.AreEqual (6, dgv.RowCount, "B1");
			Assert.AreEqual (1, dgv.ColumnCount, "B2");

			Assert.AreEqual (0, dgv.Rows[0].Index, "B3");
			Assert.AreEqual (1, dgv.Rows[1].Index, "B4");
			Assert.AreEqual (2, dgv.Rows[2].Index, "B5");


			dgv.RowCount = 2;
			
			Assert.AreEqual (2, dgv.RowCount, "C1");
			Assert.AreEqual (1, dgv.ColumnCount, "C2");

			Assert.AreEqual (0, dgv.Rows[0].Index, "C3");
			Assert.AreEqual (1, dgv.Rows[1].Index, "C4");

			dgv.AllowUserToAddRows = false;

			Assert.AreEqual (1, dgv.RowCount, "D1");
			Assert.AreEqual (1, dgv.ColumnCount, "D2");

			Assert.AreEqual (0, dgv.Rows[0].Index, "D3");

			dgv.RowCount = 6;
			
			Assert.AreEqual (6, dgv.RowCount, "E1");
			Assert.AreEqual (1, dgv.ColumnCount, "E2");

			Assert.AreEqual (0, dgv.Rows[0].Index, "E3");
			Assert.AreEqual (1, dgv.Rows[1].Index, "E4");
			Assert.AreEqual (2, dgv.Rows[2].Index, "E5");


			dgv.RowCount = 2;
			
			Assert.AreEqual (2, dgv.RowCount, "F1");
			Assert.AreEqual (1, dgv.ColumnCount, "F2");

			Assert.AreEqual (0, dgv.Rows[0].Index, "F3");
			Assert.AreEqual (1, dgv.Rows[1].Index, "F4");

		}

		[Test]
		public void BindToReadonlyProperty ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			DataGridView dgv = new DataGridView ();
			
			List<cust> l = new List<cust> ();
			l.Add (new cust ());

			dgv.DataSource = l;
			f.Controls.Add (dgv);
			
			f.Show ();
			
			Assert.AreEqual ("Name", dgv.Columns[0].Name, "A1");
			Assert.AreEqual (true, dgv.Columns[0].ReadOnly, "A2");
			
			f.Close ();
			f.Dispose ();
		}

		class cust { public string Name { get { return "test"; } } }
	
		[Test]
		public void EnableHeadersVisualStylesDefaultValue ()
		{
			Assert.AreEqual (true, new DataGridView ().EnableHeadersVisualStyles);
		}

		[Test]
		public void RowTemplate ()
		{
			DataGridView dgv = new DataGridView ();

			Form f = new Form ();
			f.Controls.Add (dgv);
			f.Show ();

			dgv.Columns.Add ("A1", "A1");
			Assert.AreEqual (0, dgv.RowTemplate.Cells.Count, "A1");
			Assert.IsNull (dgv.RowTemplate.DataGridView, "A2");

			dgv.Columns.Add ("A2", "A2");
			Assert.AreEqual (0, dgv.RowTemplate.Cells.Count, "A3");

			dgv.Rows.Add (3, 6);

			dgv.Columns.Remove ("A1");

			Assert.AreEqual (0, dgv.RowTemplate.Cells.Count, "A4");
			Assert.AreEqual (1, dgv.Rows[0].Cells.Count, "A5");

			dgv.Columns.Clear ();

			Assert.AreEqual (0, dgv.RowTemplate.Cells.Count, "A6");
			Assert.AreEqual (0, dgv.Rows.Count, "A7");

			f.Close ();
			f.Dispose ();
		}

		[Test]
		public void ScrollToSelectionSynchronous()
		{
			DataGridView dgv = new DataGridView ();
			dgv.RowCount = 1000;
			dgv.CurrentCell = dgv[0, dgv.RowCount -1];		
			Rectangle rowRect = dgv.GetRowDisplayRectangle (dgv.RowCount - 1, false);
			Assert.AreEqual (true, dgv.DisplayRectangle.Contains (rowRect), "#01");
		}

		[Test]
		public void CurrentCell()
		{
			DataGridView dgv = new DataGridView ();
			dgv.AllowUserToAddRows = false;

			Assert.IsNull (dgv.CurrentCell, "A1");

			dgv.RowCount = 10;
			dgv.ColumnCount = 2;
			Assert.AreEqual (10, dgv.RowCount, "B1");
			Assert.AreEqual (2, dgv.ColumnCount, "B2");
			Assert.IsNull (dgv.CurrentCell, "B3");

			dgv.CurrentCell = dgv[1, 9];
			Assert.IsNotNull (dgv.CurrentCell, "H1");
			Assert.AreEqual (9, dgv.CurrentCell.RowIndex, "H2");
			Assert.AreEqual (1, dgv.CurrentCell.ColumnIndex, "H3");
			
			dgv.CurrentCell = null;
			Assert.IsNull (dgv.CurrentCell, "C1");

			dgv.CurrentCell = dgv[1, 9];
			Assert.IsNotNull (dgv.CurrentCell, "D1");
			Assert.AreEqual (9, dgv.CurrentCell.RowIndex, "D2");
			Assert.AreEqual (1, dgv.CurrentCell.ColumnIndex, "D3");

			dgv.RowCount = 9;
			Assert.IsNotNull (dgv.CurrentCell, "E1");
			Assert.AreEqual (8, dgv.CurrentCell.RowIndex, "E2");
			Assert.AreEqual (1, dgv.CurrentCell.ColumnIndex, "E3");

			dgv.CurrentCell = dgv[0, 4];
			dgv.RowCount = 2;
			Assert.IsNotNull (dgv.CurrentCell, "F1");
			Assert.AreEqual (1, dgv.CurrentCell.RowIndex, "F2");
			Assert.AreEqual (0, dgv.CurrentCell.ColumnIndex, "F3");

			dgv.RowCount = 0;
			Assert.IsNull (dgv.CurrentCell, "P1");

			dgv.RowCount = 10;
			Assert.AreEqual (10, dgv.RowCount, "I1");
			dgv.CurrentCell = dgv[0, 4];
			dgv.ColumnCount = 0;
			Assert.AreEqual (0, dgv.RowCount, "I2");
			Assert.IsNull (dgv.CurrentCell, "I3");

			dgv.RowCount = 0;
			dgv.ColumnCount = 0;
			dgv.CreateControl ();
			dgv.ColumnCount = 2;
			dgv.RowCount = 3;

			Assert.IsNotNull (dgv.CurrentCell, "G1");
			Assert.AreEqual (0, dgv.CurrentCell.RowIndex, "G1");
			Assert.AreEqual (0, dgv.CurrentCell.ColumnIndex, "G1");
		}

		[Test]
		public void DataSourceBindingContextDependency ()
		{
			List<DataItem> dataList = new List<DataItem> ();
			dataList.Add (new DataItem ());
			dataList.Add (new DataItem ());

			DataGridView dgv = new DataGridView ();
			dgv.DataSource = dataList;
			Assert.IsNull (dgv.BindingContext, "#1");
			Assert.IsFalse (dgv.IsHandleCreated, "#2");
			Assert.AreEqual (0, dgv.RowCount, "#3");

			dgv.DataSource = null;

			Form form = new Form ();
			form.Controls.Add (dgv);
			dgv.DataSource = dataList;

			Assert.IsNotNull (dgv.BindingContext, "#4");
			Assert.IsFalse (dgv.IsHandleCreated, "#5");
			Assert.AreEqual (2, dgv.RowCount, "#6");

			dgv.Dispose ();
			dgv = new DataGridView ();
			dgv.DataSource = dataList;

			Assert.IsNull (dgv.BindingContext, "#7");
			Assert.IsFalse (dgv.IsHandleCreated, "#8");
			Assert.AreEqual (0, dgv.RowCount, "#9");

			dgv.CreateControl ();

			Assert.IsNull (dgv.BindingContext, "#10");
			Assert.IsTrue (dgv.IsHandleCreated, "#11");
			Assert.AreEqual (0, dgv.RowCount, "#12");
		}

		[Test]
		public void RowTemplateDataGridView ()
		{
			DataGridView gdv = new DataGridView ();
			Assert.IsNull (gdv.RowTemplate.DataGridView, "#1");
		}

		[Test] // Xamarin bug 2392
		public void RowHeightInVirtualMode ()
		{
			using (var dgv = new DataGridView ()) {
				dgv.RowHeightInfoNeeded += (sender, e) => {
					e.Height = 50;
					e.MinimumHeight = 30;
				};
				dgv.VirtualMode = true;
				dgv.RowCount = 2;
				Assert.AreEqual (50, dgv.Rows [0].Height);
				Assert.AreEqual (30, dgv.Rows [0].MinimumHeight);
				Assert.AreEqual (50, dgv.Rows [1].Height);
				Assert.AreEqual (30, dgv.Rows [1].MinimumHeight);
			}
		}

		[Test] // Novell bug 660986
		public void TestDispose ()
		{
			DataGridView dgv = new DataGridView ();
			dgv.Columns.Add ("TestColumn", "Test column");
			dgv.Rows.Add ();
			dgv.Dispose ();

			try {
				DataGridViewColumn col = dgv.Columns[0];
				Assert.Fail ("#1");
			}
			catch (ArgumentOutOfRangeException) {
			}

			try {
				DataGridViewRow row = dgv.Rows[0];
				Assert.Fail ("#2");
			}
			catch (ArgumentOutOfRangeException) {
			}
		}

		private class MyDataGridView: DataGridView
		{
			public void SetCurrentCell ()
			{
				CurrentCell = Rows [1].Cells [1];
			}
		}

		[Test]
		public void TestDisposeWhenInEditMode_Xamarin19567 ()
		{
			var dgv = new MyDataGridView ();
			dgv.EditMode = DataGridViewEditMode.EditOnEnter;
			dgv.Columns.Add ("TestColumn", "Test column");
			dgv.Columns.Add ("Column2", "Second column");
			dgv.Rows.Add ();
			dgv.Rows.Add ();
			dgv.SetCurrentCell ();

			// The Dispose() call will fail if #19567 is not fixed
			dgv.Dispose ();
		}

		[Test] // Xamarin bug 3125
		public void TestRemoveBug3125 ()
		{
			DataGridViewRow dgvr1 = new DataGridViewRow ();
			DataGridViewRow dgvr2 = new DataGridViewRow ();
			DataGridViewRow dgvr3 = new DataGridViewRow ();

			Assert.IsNull (dgvr1.DataGridView, "#1");
			Assert.IsNull (dgvr2.DataGridView, "#2");
			Assert.IsNull (dgvr3.DataGridView, "#3");

			DataGridView dgv = new DataGridView ();
			// dgv needs column and cell or throws error
			DataGridViewColumn  dgvc1 = new DataGridViewColumn ();
			DataGridViewCell cell = new DataGridViewTextBoxCell ();
			dgvc1.CellTemplate = cell;
			dgv.Columns.Add (dgvc1);
			
			dgv.Rows.Add (dgvr1);
			dgv.Rows.Add (dgvr2);
			dgv.Rows.Add (dgvr3);
			// was dgv.Clear () and that caused test build to fail
			dgv.Rows.Clear (); // presumbly this was the intention?

			Assert.IsNull (dgvr1.DataGridView, "#4");
			Assert.IsNull (dgvr2.DataGridView, "#5");
			Assert.IsNull (dgvr3.DataGridView, "#6");
		}

		[Test] // Xamarin bug #2394
		public void Bug2394_RowHeightLessThanOldMinHeightVirtMode ()
		{
			using (var dgv = new DataGridView ()) {
				dgv.VirtualMode = true;
				dgv.RowCount = 1;
				dgv.Rows [0].MinimumHeight = 5;
				dgv.Rows [0].Height = 10;
				dgv.RowHeightInfoNeeded += (sender, e) => {
					// NOTE: the order is important here.
					e.MinimumHeight = 2;
					e.Height = 2;
				};
				dgv.UpdateRowHeightInfo (0, false);
				Assert.AreEqual (2, dgv.Rows [0].Height);
				Assert.AreEqual (2, dgv.Rows [0].MinimumHeight);
			}
		}

		[Test] // Xamarin bug #2394
		public void Bug2394_RowHeightLessThanMinHeightVirtMode ()
		{
			using (var dgv = new DataGridView ()) {
				dgv.VirtualMode = true;
				dgv.RowCount = 1;
				dgv.Rows [0].Height = 10;
				dgv.Rows [0].MinimumHeight = 5;
				dgv.RowHeightInfoNeeded += (sender, e) => {
					// Setting the height to a value less than the minimum height
					// will be silently ignored and instead set to MinimumHeight.
					e.Height = 2;
				};
				dgv.UpdateRowHeightInfo (0, false);
				Assert.AreEqual(5, dgv.Rows[0].Height);
				Assert.AreEqual(5, dgv.Rows[0].MinimumHeight);
			}
		}

		[Test] // Xamarin bug #2394
		public void Bug2394_MinHeightGreaterThanOldRowHeightVirtMode ()
		{
			using (var dgv = new DataGridView ()) {
				dgv.VirtualMode = true;
				dgv.RowCount = 1;
				dgv.Rows [0].Height = 10;
				dgv.Rows [0].MinimumHeight = 5;
				dgv.RowHeightInfoNeeded += (sender, e) => {
					e.MinimumHeight = 30;
					e.Height = 40;
				};
				dgv.UpdateRowHeightInfo (0, false);
				Assert.AreEqual (40, dgv.Rows [0].Height);
				Assert.AreEqual (30, dgv.Rows [0].MinimumHeight);
			}
		}

		[Test] // Xamarin bug #24372
		public void Bug24372_first_row_index ()
		{
			Form form = new Form ();
			DataGridView24372 dgv = new DataGridView24372 ();
			dgv.Parent = form;
			dgv.ColumnCount = 1;
			dgv.RowCount = 100;
			dgv.CurrentCell = dgv[0,50];
			dgv.Focus ();
			form.Show ();

			dgv.Rows.Clear ();
			form.Refresh ();
			Application.DoEvents ();

			if (dgv.HasException)
				Assert.Fail("#A1");

			form.Dispose ();
		}

		class DataGridView24372 : DataGridView
		{
			public bool HasException { get; private set; }
			protected override void OnPaint (PaintEventArgs e)
			{
				HasException = false;
				try {
					base.OnPaint(e);
				} catch (ArgumentOutOfRangeException ex) {
					HasException = true;
				}
			}
		}
	}
	
	[TestFixture]
	public class DataGridViewControlCollectionTest
	{
		[Test]
		public void TestClear ()
		{
			using (DataGridView dgv = new DataGridView ()) {
				DataGridView.DataGridViewControlCollection controls = (DataGridView.DataGridViewControlCollection) dgv.Controls;
				Control c1 = new Control ();
				Control c2 = new Control ();
				Control c3 = new Control ();
				Assert.AreEqual (2, controls.Count, "#02");
				controls.Add (c1);
				controls.Add (c2);
				controls.Add (c3);
				Assert.AreEqual (5, controls.Count, "#02");
				controls.Clear ();
				Assert.AreEqual (3, controls.Count, "#03");
				Assert.AreSame (c2, controls [2], "#04");
			}

			// Maybe MS should start writing unit-tests?

			using (DataGridView dgv = new DataGridView ()) {
				DataGridView.DataGridViewControlCollection controls = (DataGridView.DataGridViewControlCollection)dgv.Controls;
				Control [] c = new Control [20];
				for (int i = 0; i < c.Length; i++) {
					c [i] = new Control ();
					c [i].Text = "#" + i.ToString ();
				}
				
				Assert.AreEqual (2, controls.Count, "#02");
				controls.AddRange (c);
				Assert.AreEqual (22, controls.Count, "#02");
				controls.Clear ();
				Assert.AreEqual (12, controls.Count, "#03");
				
				for (int i = 0; i < c.Length; i += 2) {
					Assert.AreSame (c [i+1], controls [ (i / 2) + 2], "#A" + i.ToString ());
				}
			}
		}

		[Test]
		public void TestCopyTo ()
		{
			using (DataGridView dgv = new DataGridView ()) {
				DataGridView.DataGridViewControlCollection controls = (DataGridView.DataGridViewControlCollection)dgv.Controls;
				Control c1 = new Control ();
				Control c2 = new Control ();
				Control c3 = new Control ();
				Control [] copy = new Control [10];
				Assert.AreEqual (2, controls.Count, "#01");
				controls.AddRange (new Control [] { c1, c2, c3 });
				Assert.AreEqual (5, controls.Count, "#01-b");
				controls.CopyTo (copy, 0);
				Assert.AreEqual (5, controls.Count, "#02");
				Assert.AreEqual (10, copy.Length, "#03");
				for (int i = 0; i < copy.Length; i++) {
					if (i >= 5)
						Assert.IsNull (copy [i], "#A" + i.ToString ());
					else
						Assert.IsNotNull (copy [i], "#B" + i.ToString ());
				}
			}
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestInsert ()
		{
			using (DataGridView dgv = new DataGridView ()) {
				DataGridView.DataGridViewControlCollection controls = (DataGridView.DataGridViewControlCollection)dgv.Controls;
				controls.Insert (1, new Control ());
			}
		}

		[Test]
		public void TestRemove ()
		{
			using (DataGridView dgv = new DataGridView ()) {
				DataGridView.DataGridViewControlCollection controls = (DataGridView.DataGridViewControlCollection)dgv.Controls;
				Control c1 = new Control ();
				Control c2 = new Control ();
				Control c3 = new Control ();
				Control [] copy = new Control [10];
				
				controls.AddRange (new Control [] {c1, c2, c3});
				
				controls.Remove (c2);
				Assert.AreEqual (4, controls.Count, "#01");
				controls.Remove (c2);
				Assert.AreEqual (4, controls.Count, "#02");
				controls.Remove (c1);
				Assert.AreEqual (3, controls.Count, "#03");
				controls.Remove (c3);
				Assert.AreEqual (2, controls.Count, "#04");
				
				controls.Remove (controls [0]);
				controls.Remove (controls [1]);
				Assert.AreEqual (2, controls.Count, "#05");
			}
		}
	}
		
}

