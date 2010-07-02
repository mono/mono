//
// RepeatInfoTest.cs - Unit tests for System.Web.UI.WebControls.RepeatInfo
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {

	public class RepeatInfoUser : IRepeatInfoUser {

		private bool footer;
		private bool header;
		private bool separators;
		private int count;
		private int counter;


		public RepeatInfoUser (bool footer, bool header, bool separators, int count)
		{
			counter = 0;
			this.footer = footer;
			this.header = header;
			this.separators = separators;
			this.count = count;
		}


		public bool HasFooter {
			get { return footer; }
		}

		public bool HasHeader {
			get { return header; }
		}
		
		public bool HasSeparators {
			get { return separators; }
		}

		public int RepeatedItemCount {
			get { return count; }
		}

		public Style GetItemStyle (ListItemType itemType, int repeatIndex)
		{
			return null;
		}

		public void RenderItem (ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
		{
			writer.Write ((counter++).ToString ());
		}
	}

	[TestFixture]
	public class RepeatInfoTest {

		private HtmlTextWriter GetWriter ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			return new HtmlTextWriter (sw);
		}

		string DoTest (int cols, int cnt, RepeatDirection d, RepeatLayout l, bool OuterTableImplied, bool ftr, bool hdr, bool sep)
		{
			HtmlTextWriter htw = GetWriter ();
			RepeatInfo ri = new RepeatInfo ();
			ri.RepeatColumns = cols;
			ri.RepeatDirection = d;
			ri.RepeatLayout = l;
			ri.OuterTableImplied = OuterTableImplied;

			ri.RenderRepeater (htw, new RepeatInfoUser (ftr, hdr, sep, cnt), new TableStyle (), new DataList ());
			return htw.InnerWriter.ToString ();
		}

		[Test]
		public void DefaultValues ()
		{
			RepeatInfo ri = new RepeatInfo ();
			Assert.AreEqual (0, ri.RepeatColumns, "RepeatColumns");
			Assert.AreEqual (RepeatDirection.Vertical, ri.RepeatDirection, "RepeatDirection");
			Assert.AreEqual (RepeatLayout.Table, ri.RepeatLayout, "RepeatLayout");
			Assert.IsFalse (ri.OuterTableImplied, "OuterTableImplied");
		}

		[Test]
		public void RepeatColumns_Negative ()
		{
			RepeatInfo ri = new RepeatInfo ();
			ri.RepeatColumns = -1;
			Assert.AreEqual (-1, ri.RepeatColumns, "-1");
			ri.RepeatColumns = Int32.MinValue;
			Assert.AreEqual (Int32.MinValue, ri.RepeatColumns, "Int32.MinValue");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RepeatDirection_Invalid ()
		{
			RepeatInfo ri = new RepeatInfo ();
			ri.RepeatDirection = (RepeatDirection) Int32.MinValue;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RepeatLayout_Invalid ()
		{
			RepeatInfo ri = new RepeatInfo ();
			ri.RepeatLayout = (RepeatLayout) Int32.MinValue;
		}

		private void RenderRepeater_BaseControl (string s, string msg, WebControl wc)
		{
			RepeatInfo ri = new RepeatInfo ();
			ri.RepeatColumns = 3;
			ri.RepeatDirection = RepeatDirection.Vertical;
			ri.RepeatLayout = RepeatLayout.Table;

			HtmlTextWriter writer = GetWriter ();
			ri.RenderRepeater (writer, new RepeatInfoUser (false, false, false, 1), new TableStyle (), wc);
			string rendered = writer.InnerWriter.ToString ();
			Assert.AreEqual (s, rendered, msg);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void RenderRepeater_BaseControl_Null ()
		{
			RenderRepeater_BaseControl ("shouldn't get here", "null", null);
		}
		
		[Test]
		public void RenderRepeater_BaseControl ()
		{
#if NET_4_0
			string noid = "<table>\n\t<tr>\n\t\t<td>0</td><td></td><td></td>\n\t</tr>\n</table>";
			string id_enabled = "<table id=\"foo\" class=\"aspNetDisabled\">\n\t<tr>\n\t\t<td>0</td><td></td><td></td>\n\t</tr>\n</table>";
#else
			string noid = "<table border=\"0\">\n\t<tr>\n\t\t<td>0</td><td></td><td></td>\n\t</tr>\n</table>";
			string id_enabled = "<table id=\"foo\" disabled=\"disabled\" border=\"0\">\n\t<tr>\n\t\t<td>0</td><td></td><td></td>\n\t</tr>\n</table>";
#endif
			RenderRepeater_BaseControl (noid, "Table", new Table ());
			RenderRepeater_BaseControl (noid, "DataList", new DataList ());
			RenderRepeater_BaseControl (noid, "DataListItem", new DataListItem (0, ListItemType.Item));

			Label l = new Label ();
			l.Enabled = false;
			l.ID = "foo";
			
			RenderRepeater_BaseControl (id_enabled, "id and disabled", l);			
		}
	}
}

