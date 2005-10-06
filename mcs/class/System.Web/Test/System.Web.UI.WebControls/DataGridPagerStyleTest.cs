//
// Tests for System.Web.UI.WebControls.DataGridPagerStyle 
//
// Author:
//	Peter Dennis Bartok (pbartok@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls {
	public class DGTestClass : DataGrid {

		public DGTestClass ()
			: base () {
		}

		public StateBag StateBag {
			get { return base.ViewState; }
		}

		public string[] KeyValuePairs() {
			IEnumerator	e;
			string[]	result;
			int		item;

			e = ViewState.GetEnumerator();
			result = new string[ViewState.Keys.Count];
			item = 0;

			while (e.MoveNext()) {
				DictionaryEntry	d;
				StateItem	si;

				d = (DictionaryEntry)e.Current;
				si = (StateItem)d.Value;

				if (si.Value is String[]) {
					string[] values;

					values = (string[]) si.Value;
					result[item] = d.Key.ToString() + "=";
					if (values.Length > 0) {
						result[item] += values[0];

						for (int i = 1; i < values.Length; i++) {
							result[item] += ", " + values[i];
						}
					}
				} else {
					result[item] =  d.Key.ToString() + "=" + si.Value;
				}
				item++;
			}

			return result;
		}
	}

	[TestFixture]	
	public class DataGridPagerStyleTest {
		private static HtmlTextWriter GetWriter () {
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			return new HtmlTextWriter (sw);
		}

		private bool IsEqual(object[] a1, object[] a2, string assertion) {
			int	matches;
			bool[]	notfound;	

			if (a1.Length != a2.Length) {
				if (assertion != null) {
					Assert.Fail(assertion + "( different length )");
				}
				return false;
			}

			matches = 0;
			notfound = new bool[a1.Length];

			for (int i = 0; i < a1.Length; i++) {
				for (int j = 0; j < a2.Length; j++) {
					if (a1[i].Equals(a2[j])) {
						matches++;
						break;
					}
				}
				if ((assertion != null) && (matches != i+1)) {
					Assert.Fail(assertion + "( missing " + a1[i].ToString() + " )");
				}
			}

			return matches == a1.Length;
		}

		[Test]
		public void DataGridPagerStyle_Defaults () {
			DataGrid		g;
			DataGridPagerStyle	s;

			g = new DataGrid();
			s = g.PagerStyle;

			Assert.AreEqual (Color.Empty, s.BackColor, "D1");
			Assert.AreEqual (Color.Empty, s.BorderColor, "D2");
			Assert.AreEqual (BorderStyle.NotSet, s.BorderStyle, "D3");
			Assert.AreEqual (Unit.Empty, s.BorderWidth, "D4");
			Assert.AreEqual (string.Empty, s.CssClass, "D5");
			Assert.AreEqual (Color.Empty, s.ForeColor, "D6");
			Assert.AreEqual (Unit.Empty, s.Height, "D7");
			Assert.AreEqual (Unit.Empty, s.Width, "D8");

			Assert.AreEqual (PagerMode.NextPrev, s.Mode, "D9");
			Assert.AreEqual ("&gt;", s.NextPageText, "D10");
			Assert.AreEqual (10, s.PageButtonCount, "D11");
			Assert.AreEqual (PagerPosition.Bottom, s.Position, "D12");
			Assert.AreEqual ("&lt;", s.PrevPageText, "D13");
			Assert.AreEqual (true, s.Visible, "D14");
		}

		[Test]
		public void DataGridPagerStyle_Assignment () {
			DataGrid		g;
			DataGridPagerStyle	s;

			g = new DataGrid();
			s = g.PagerStyle;


			s.Mode = PagerMode.NumericPages;
			Assert.AreEqual (PagerMode.NumericPages, s.Mode, "A1");

			s.NextPageText = "Next";
			Assert.AreEqual ("Next", s.NextPageText, "A2");

			s.PageButtonCount = 20;
			Assert.AreEqual (20, s.PageButtonCount, "A3");

			s.Position = PagerPosition.TopAndBottom;
			Assert.AreEqual (PagerPosition.TopAndBottom, s.Position, "A4");

			s.PrevPageText = "Prev";
			Assert.AreEqual ("Prev", s.PrevPageText, "A5");

			s.Visible = false;
			Assert.AreEqual (false, s.Visible, "A6");
		}

		[Test]
		public void DataGridPagerStyle_Copy () {
			string[]		keyvalues;
			DataGrid		g;
			DataGrid		g2;
			DataGridPagerStyle	s;
			DataGridPagerStyle	copy;

			g = new DataGrid();
			g2 = new DataGrid();

			s = g.PagerStyle;
			copy = g2.PagerStyle;

			s.Mode = PagerMode.NumericPages;
			Assert.AreEqual (PagerMode.NumericPages, s.Mode, "C1");

			s.NextPageText = "Next";
			Assert.AreEqual ("Next", s.NextPageText, "C2");

			s.PageButtonCount = 20;
			Assert.AreEqual (20, s.PageButtonCount, "C3");

			s.Position = PagerPosition.TopAndBottom;
			Assert.AreEqual (PagerPosition.TopAndBottom, s.Position, "C4");

			s.PrevPageText = "Prev";
			Assert.AreEqual ("Prev", s.PrevPageText, "C5");

			s.Visible = false;
			Assert.AreEqual (false, s.Visible, "C6");

			// Now copy them over;
			copy.CopyFrom(s);
			Assert.AreEqual (PagerMode.NumericPages, copy.Mode, "C7");
			Assert.AreEqual ("Next", copy.NextPageText, "C8");
			Assert.AreEqual (20, copy.PageButtonCount, "C9");
			Assert.AreEqual (PagerPosition.TopAndBottom, copy.Position, "C10");
			Assert.AreEqual ("Prev", copy.PrevPageText, "C11");
			Assert.AreEqual (false, copy.Visible, "C12");
		}

		[Test]
		public void DataGridPagerStyle_Merge () {
			string[]		keyvalues;
			DataGrid		g;
			DataGrid		g2;
			DataGridPagerStyle	s;
			DataGridPagerStyle	copy;

			g = new DataGrid();
			g2 = new DataGrid();

			s = g.PagerStyle;
			copy = g2.PagerStyle;

			s.Mode = PagerMode.NumericPages;
			Assert.AreEqual (PagerMode.NumericPages, s.Mode, "M1");

			s.NextPageText = "Next";
			Assert.AreEqual ("Next", s.NextPageText, "M2");

			s.PageButtonCount = 20;
			Assert.AreEqual (20, s.PageButtonCount, "M3");

			copy.Position = PagerPosition.Top;
			s.Position = PagerPosition.TopAndBottom;
			Assert.AreEqual (PagerPosition.TopAndBottom, s.Position, "M4");

			copy.PrevPageText = "Blah";
			s.PrevPageText = "Prev";
			Assert.AreEqual ("Prev", s.PrevPageText, "M5");

			copy.Visible = true;
			s.Visible = false;
			Assert.AreEqual (false, s.Visible, "M6");

			// Now merge
			copy.MergeWith(s);

			Assert.AreEqual (PagerMode.NumericPages, copy.Mode, "M7");
			Assert.AreEqual ("Next", copy.NextPageText, "M8");
			Assert.AreEqual (20, copy.PageButtonCount, "M9");
			Assert.AreEqual (PagerPosition.Top, copy.Position, "M10");
			Assert.AreEqual ("Blah", copy.PrevPageText, "M11");
			Assert.AreEqual (true, copy.Visible, "M12");
		}

		[Test]
		public void DataGridPagerStyle_Reset () {
			string[]		keyvalues;
			DataGrid		g;
			DataGridPagerStyle	s;

			g = new DataGrid();

			s = g.PagerStyle;

			s.Mode = PagerMode.NumericPages;
			Assert.AreEqual (PagerMode.NumericPages, s.Mode, "R1");

			s.NextPageText = "Next";
			Assert.AreEqual ("Next", s.NextPageText, "R2");

			s.PageButtonCount = 20;
			Assert.AreEqual (20, s.PageButtonCount, "R3");

			s.Position = PagerPosition.TopAndBottom;
			Assert.AreEqual (PagerPosition.TopAndBottom, s.Position, "R4");

			s.PrevPageText = "Prev";
			Assert.AreEqual ("Prev", s.PrevPageText, "R5");

			s.Visible = false;
			Assert.AreEqual (false, s.Visible, "R6");

			s.Reset();

			Assert.AreEqual (PagerMode.NextPrev, s.Mode, "D9");
			Assert.AreEqual ("&gt;", s.NextPageText, "D10");
			Assert.AreEqual (10, s.PageButtonCount, "D11");
			Assert.AreEqual (PagerPosition.Bottom, s.Position, "D12");
			Assert.AreEqual ("&lt;", s.PrevPageText, "D13");
			Assert.AreEqual (true, s.Visible, "D14");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void PageButtonCount_Neg ()
		{
                        DataGrid        d;

                        d = new DataGrid();
			// bug 50236
                        d.PagerStyle.PageButtonCount = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void PageButtonCount_Zero ()
		{
			DataGrid d;

			d = new DataGrid ();
			d.PagerStyle.PageButtonCount = 0;
		}
	}
}
