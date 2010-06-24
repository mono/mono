//
// Tests for System.Web.UI.WebControls.DetailsView.cs 
//
// Author:
//	Merav Sudri (meravs@mainsoft.com)
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
using System.Drawing;
using System.Collections;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;


namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class DetailsViewRowTest
	{

		public class PokerDetailsViewRow : DetailsViewRow
		{


			public PokerDetailsViewRow(int itemIndex, DataControlRowType rowType, DataControlRowState rowState)
				: base (itemIndex, rowType, rowState)
			{
				TrackViewState ();
			}

			public object SaveState ()
			{
				return SaveViewState ();
			}

			public void LoadState (object o)
			{
				LoadViewState (o);
			}

			public StateBag StateBag
			{
				get { return base.ViewState; }
			}

			public void DoOnBubbleEvent (Object sender, EventArgs e)
			{
				OnBubbleEvent (sender, e);
			}

			public string Render ()
			{
				StringWriter sw = new StringWriter ();
				HtmlTextWriter tw = new HtmlTextWriter (sw);
				Render (tw);
				return sw.ToString ();
			}

		}


		[Test]
		public void DetailsViewRow_Properties ()
		{
			PokerDetailsViewRow row = new PokerDetailsViewRow (2, DataControlRowType.DataRow, DataControlRowState.Edit);
			Assert.AreEqual (2, row.RowIndex, "ItemIndex");
			Assert.AreEqual (DataControlRowType.DataRow, row.RowType, "RowType");
			Assert.AreEqual (DataControlRowState.Edit, row.RowState, "RowState");
			row = new PokerDetailsViewRow (3, DataControlRowType.Pager , DataControlRowState.Insert);
			Assert.AreEqual (3, row.RowIndex, "ItemIndex");
			Assert.AreEqual (DataControlRowType.Pager, row.RowType, "RowType");
			Assert.AreEqual (DataControlRowState.Insert, row.RowState, "RowState");
		}


		private bool dataDeleting = false;
		private bool dataInserting = false;
		private bool dataUpdating=false;

		[Test]
		public void DetailsViewRow_BubbleEvent ()
		{
			DetailsView dv = new DetailsView ();
			dv.Page = new Page ();
			PokerDetailsViewRow row = new PokerDetailsViewRow (2, DataControlRowType.Footer, DataControlRowState.Insert);
			Button bt = new Button ();
			dv.Controls.Add (row);
			CommandEventArgs com = new CommandEventArgs (new CommandEventArgs ("Delete", null));
			dv.ItemDeleting += new DetailsViewDeleteEventHandler (R_DataDeleting);
			Assert.AreEqual (false, dataDeleting, "BeforeDeleteBubbleEvent");
			row.DoOnBubbleEvent (row, com);
			Assert.AreEqual (true, dataDeleting, "AfterDeleteBubbleEvent");
			dv.ChangeMode (DetailsViewMode.Insert); 
			com = new CommandEventArgs (new CommandEventArgs ("Insert", null));
			dv.ItemInserting += new DetailsViewInsertEventHandler (dv_ItemInserting);			
			Assert.AreEqual (false, dataInserting, "BeforeInsertBubbleEvent");
			row.DoOnBubbleEvent (row, com);
			Assert.AreEqual (true, dataInserting, "AfterInsertBubbleEvent");
			dv.ChangeMode (DetailsViewMode.Edit); 
			com = new CommandEventArgs (new CommandEventArgs ("Update", null));
			dv.ItemUpdating += new DetailsViewUpdateEventHandler (dv_ItemUpdating);
			Assert.AreEqual (false, dataUpdating, "BeforeUpdateBubbleEvent");
			row.DoOnBubbleEvent (row, com);
			Assert.AreEqual (true, dataUpdating, "AfterUpdateBubbleEvent");
			dv.ItemUpdating += new DetailsViewUpdateEventHandler (dv_ItemUpdating);

		}

		void dv_ItemUpdating (object sender, DetailsViewUpdateEventArgs e)
		{
			dataUpdating = true;
		}

		void dv_ItemInserting (object sender, DetailsViewInsertEventArgs e)
		{
			dataInserting = true;
		}

		public void R_DataDeleting(object sender, EventArgs e)
		{
			dataDeleting = true;
		}

		//ViewState
		[Test]
		public void DetailsViewRow_ViewState ()
		{
			PokerDetailsViewRow row = new PokerDetailsViewRow (2, DataControlRowType.Header, DataControlRowState.Selected);
			PokerDetailsViewRow copy = new PokerDetailsViewRow (3, DataControlRowType.Pager, DataControlRowState.Insert);
			row.CssClass = "style.css";
			row.BackColor = Color.Red;
			object state = row.SaveState ();
			copy.LoadState (state);
			Assert.AreEqual ("style.css", copy.CssClass, "ViewStateCssClass");
			Assert.AreEqual (Color.Red, copy.BackColor, "ViewStateHeaderText");
		}

		[Test]
		public void DetailsView_render ()
		{
			PokerDetailsViewRow row = new PokerDetailsViewRow (2, DataControlRowType.Header, DataControlRowState.Selected);
			row.ID = "TestingRow";
			row.BackColor = Color.Red; 
			string originalHtml = "<tr id=\"TestingRow\" style=\"background-color:Red;\">\r\n\r\n</tr>";
			string renderedHtml = row.Render ();
			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "DetailsViewRowRender");
		}
	}
}
#endif
