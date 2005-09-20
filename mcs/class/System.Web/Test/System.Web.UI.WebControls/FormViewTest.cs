//
// Tests for System.Web.UI.WebControls.FormView.cs 
//
// Author:
//	Chris Toshok (toshok@ximian.com)
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

#if NET_2_0
using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class FormViewTest {	
		class Poker : FormView {
			public Poker () {
				TrackViewState ();
			}

			public object SaveState () {
				return SaveViewState ();
			}

			public void LoadState (object state) {
				LoadViewState (state);
			}
		}
		
		[Test]
		public void Defaults ()
		{
			Poker p = new Poker ();

			Assert.IsFalse (p.AllowPaging, "A1");
			Assert.AreEqual ("", p.BackImageUrl, "A2");
			Assert.IsNull (p.BottomPagerRow, "A3");
			Assert.AreEqual ("", p.Caption, "A4");
			Assert.AreEqual (TableCaptionAlign.NotSet, p.CaptionAlign ,"A5");
			Assert.AreEqual (-1, p.CellPadding, "A6");
			Assert.AreEqual (0, p.CellSpacing, "A7");
			Assert.AreEqual (FormViewMode.ReadOnly, p.CurrentMode, "A8");
			Assert.AreEqual (FormViewMode.ReadOnly, p.DefaultMode, "A9");
			Assert.IsNotNull (p.DataKeyNames, "A10");
			Assert.AreEqual (0, p.DataKeyNames.Length, "A10.1");
			Assert.IsNotNull (p.DataKey, "A11");
			Assert.AreEqual (0, p.DataKey.Values.Count, "A11.1");
			Assert.IsNull (p.EditItemTemplate, "A12");
			Assert.IsNotNull (p.EditRowStyle, "A13");

			Assert.IsNotNull (p.EmptyDataRowStyle, "A14");

			Assert.IsNull (p.EmptyDataTemplate, "A15");
			Assert.AreEqual ("", p.EmptyDataText, "A16");
			Assert.IsNull (p.FooterRow, "A17");
			Assert.IsNull (p.FooterTemplate, "A18");
			Assert.AreEqual ("", p.FooterText, "A19");
			Assert.IsNotNull (p.FooterStyle, "A20");

			Assert.AreEqual (GridLines.None, p.GridLines, "A21");
			Assert.IsNull (p.HeaderRow, "A22");
			Assert.IsNotNull (p.HeaderStyle, "A23");

			Assert.IsNull (p.HeaderTemplate, "A24");
			Assert.AreEqual ("", p.HeaderText, "A25");
			Assert.AreEqual (HorizontalAlign.NotSet, p.HorizontalAlign, "A26");
			Assert.IsNull (p.InsertItemTemplate, "A27");
			Assert.IsNotNull (p.InsertRowStyle, "A28");

			Assert.IsNull (p.ItemTemplate, "A29");
			Assert.AreEqual (0, p.PageCount, "A30");
			Assert.AreEqual (0, p.PageIndex, "A31");
			Assert.IsNull (p.PagerTemplate, "A32");
			Assert.IsNull (p.Row, "A33");
			Assert.IsNotNull (p.RowStyle, "A34");

			Assert.IsNull (p.SelectedValue, "A35");
			Assert.IsNull (p.TopPagerRow, "A36");
			Assert.IsNull (p.DataItem, "A37");
			Assert.AreEqual (0, p.DataItemCount, "A38");
			Assert.AreEqual (0, p.DataItemIndex, "A39");
		}
	}
}
#endif
