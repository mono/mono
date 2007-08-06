//
// Tests for System.Web.UI.WebControls.DataGrid.cs 
//
// Author:
//	Jackson Harper (jackson@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
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
using AttributeCollection = System.ComponentModel.AttributeCollection;
using System;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data;
using System.ComponentModel;
using System.Diagnostics;
#if NET_2_0
using System.Collections.Generic;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
#endif

namespace MonoTests.System.Web.UI.WebControls {

	public class DataGridPoker : DataGrid {

		public DataGridPoker ()
		{
			TrackViewState ();
		}

		public string GetTagName ()
		{
			return TagName;
		}

		public void PrepareCH ()
		{
			PrepareControlHierarchy ();
		}

		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			Render (tw);
			return sw.ToString ();
		}

		public StateBag GetViewState ()
		{
			return ViewState;
		}

		public new Style ControlStyle ()
		{
			return CreateControlStyle ();
		}

		public void DoCancelCommand (DataGridCommandEventArgs e)
		{
			OnCancelCommand (e);
		}

		public void DoDeleteCommand (DataGridCommandEventArgs e)
		{
			OnDeleteCommand (e);
		}

		public void DoEditCommand (DataGridCommandEventArgs e)
		{
			OnEditCommand (e);
		}

		public void DoItemCommand (DataGridCommandEventArgs e)
		{
			OnItemCommand (e);
		}

		public void DoUpdateCommand (DataGridCommandEventArgs e)
		{
			OnUpdateCommand (e);
		}

		public void DoItemCreated (DataGridItemEventArgs e)
		{
			OnItemCreated (e);
		}

		public void DoItemDataBound (DataGridItemEventArgs e)
		{
			OnItemDataBound (e);
		}

		public void DoPageIndexChanged (DataGridPageChangedEventArgs e)
		{
			OnPageIndexChanged (e);
		}

		public void DoSortCommand (DataGridSortCommandEventArgs e)
		{
			OnSortCommand (e);
		}

		public void DoBubbleEvent (object source, EventArgs e)
		{
			OnBubbleEvent (source, e);
		}

		public void TrackState ()
		{
			TrackViewState ();
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}

		public ArrayList CreateColumns (PagedDataSource data_source, bool use_data_source)
		{
			return CreateColumnSet (data_source, use_data_source);
		}

		public void CreateControls (bool use_data_source)
		{
			CreateControlHierarchy (use_data_source);
		}

		public void InitPager (DataGridItem item, int columnSpan,
				PagedDataSource pagedDataSource)
		{
			InitializePager (item, columnSpan, pagedDataSource);
		}
	}

	public class AmazingEnumerable : IEnumerable {

		private IList list;
		public int CallCount;

		public AmazingEnumerable (IList list)
		{
			this.list = list;
		}

	        public IEnumerator GetEnumerator ()
		{
			CallCount++;
			return list.GetEnumerator ();
		}
		
	}

	[TestFixture]
	public class DataGridTest {

#if NET_2_0
		[TestFixtureSetUp()]
		public void FixtureSetup () 
		{
#if VISUAL_STUDIO
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.DataGrid.aspx", "DataGrid.aspx");
#else
			WebTest.CopyResource (GetType (), "DataGrid.aspx", "DataGrid.aspx");
#endif
		}

		[TestFixtureTearDown()]
		public void FixtureTearDown () 
		{
			WebTest.Unload ();
		}
#endif

		[Test]
		public void Defaults ()
		{
			DataGridPoker p = new DataGridPoker ();

			Assert.AreEqual (DataGrid.CancelCommandName, "Cancel", "A1");
			Assert.AreEqual (DataGrid.DeleteCommandName, "Delete", "A2");
			Assert.AreEqual (DataGrid.EditCommandName, "Edit", "A3");
			Assert.AreEqual (DataGrid.NextPageCommandArgument, "Next", "A4");
			Assert.AreEqual (DataGrid.PageCommandName, "Page", "A5");
			Assert.AreEqual (DataGrid.PrevPageCommandArgument, "Prev", "A6");
			Assert.AreEqual (DataGrid.SelectCommandName, "Select", "A7");
			Assert.AreEqual (DataGrid.SortCommandName, "Sort", "A8");
			Assert.AreEqual (DataGrid.UpdateCommandName, "Update", "A9");

			Assert.AreEqual (p.AllowCustomPaging, false, "A10");
			Assert.AreEqual (p.AllowPaging, false, "A11");
			Assert.AreEqual (p.AllowSorting, false, "A12");
			Assert.AreEqual (p.AutoGenerateColumns, true, "A13");
			Assert.AreEqual (p.BackImageUrl, String.Empty, "A14");
			Assert.AreEqual (p.CurrentPageIndex, 0, "A15");
			Assert.AreEqual (p.EditItemIndex, -1, "A16");
			Assert.AreEqual (p.PageCount, 0, "A17");
			Assert.AreEqual (p.PageSize, 10, "A18");
			Assert.AreEqual (p.SelectedIndex, -1, "A19");
			Assert.AreEqual (p.SelectedItem, null, "A20");
			Assert.AreEqual (p.ShowFooter, false, "A21");
			Assert.AreEqual (p.ShowHeader, true, "A22");
			Assert.AreEqual (p.VirtualItemCount, 0, "A23");
		}

		[Test]
		public void TagName ()
		{
			DataGridPoker p = new DataGridPoker ();
#if NET_2_0
			Assert.AreEqual (p.GetTagName (), "table", "A1");
#else
			Assert.AreEqual (p.GetTagName (), "span", "A1");
#endif
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullBackImage ()
		{
			DataGridPoker p = new DataGridPoker ();

			p.BackImageUrl = null;
			Assert.AreEqual (p.BackImageUrl, String.Empty, "A1");
		}

		[Test]
		public void CleanProperties ()
		{
			DataGridPoker p = new DataGridPoker ();

			p.AllowCustomPaging = true;
			Assert.IsTrue (p.AllowCustomPaging, "A1");
			p.AllowCustomPaging = false;
			Assert.IsFalse (p.AllowCustomPaging, "A2");

			p.AllowPaging = true;
			Assert.IsTrue (p.AllowPaging, "A3");
			p.AllowPaging = false;
			Assert.IsFalse (p.AllowPaging, "A4");

			p.AllowSorting = true;
			Assert.IsTrue (p.AllowSorting, "A5");
			p.AllowSorting = false;
			Assert.IsFalse (p.AllowSorting, "A6");

			p.AutoGenerateColumns = true;
			Assert.IsTrue (p.AutoGenerateColumns, "A7");
			p.AutoGenerateColumns = false;
			Assert.IsFalse (p.AutoGenerateColumns, "A8");

			p.BackImageUrl = "foobar";
			Assert.AreEqual (p.BackImageUrl, "foobar", "A9");

			p.CurrentPageIndex = 0;
			Assert.AreEqual (p.CurrentPageIndex, 0, "A10");
			p.CurrentPageIndex = Int32.MaxValue;
			Assert.AreEqual (p.CurrentPageIndex, Int32.MaxValue, "A11");

			p.EditItemIndex = 0;
			Assert.AreEqual (p.EditItemIndex, 0, "A12");
			p.EditItemIndex = -1;
			Assert.AreEqual (p.EditItemIndex, -1, "A13");
			p.EditItemIndex = Int32.MaxValue;
			Assert.AreEqual (p.EditItemIndex, Int32.MaxValue, "A14");

			p.PageSize = 1;
			Assert.AreEqual (p.PageSize, 1, "A15");
			p.PageSize = Int32.MaxValue;

			p.SelectedIndex = 0;
			Assert.AreEqual (p.SelectedIndex, 0, "A16");
			p.SelectedIndex = -1;
			Assert.AreEqual (p.SelectedIndex, -1, "A17");
			p.SelectedIndex = Int32.MaxValue;
			Assert.AreEqual (p.SelectedIndex, Int32.MaxValue, "A18");

			p.ShowFooter = true;
			Assert.IsTrue (p.ShowFooter, "A19");
			p.ShowFooter = false;
			Assert.IsFalse (p.ShowFooter, "A20");

			p.ShowHeader = true;
			Assert.IsTrue (p.ShowHeader, "A21");
			p.ShowHeader = false;
			Assert.IsFalse (p.ShowHeader, "A22");

			p.VirtualItemCount = 0;
			Assert.AreEqual (p.VirtualItemCount, 0, "A23");
			p.VirtualItemCount = Int32.MaxValue;
			Assert.AreEqual (p.VirtualItemCount, Int32.MaxValue, "A24");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CurrentPageIndexTooLow ()
		{
			DataGridPoker p = new DataGridPoker ();
			p.CurrentPageIndex = -1;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void EditItemIndexTooLow ()
		{
			DataGridPoker p = new DataGridPoker ();
			p.EditItemIndex = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void PageSizeTooLow ()
		{
			DataGridPoker p = new DataGridPoker ();
			p.PageSize = 0;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SelectedIndexTooLow ()
		{
			DataGridPoker p = new DataGridPoker ();
			p.SelectedIndex = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void VirtualItemCountTooLow ()
		{
			DataGridPoker p = new DataGridPoker ();
			p.VirtualItemCount = -1;
		}
			
		[Test]
		public void ViewState ()
		{
			DataGridPoker p = new DataGridPoker ();
			StateBag vs = p.GetViewState ();

			Assert.AreEqual (vs.Count, 0, "A1");

			p.AllowCustomPaging = true;
			Assert.AreEqual (vs.Count, 1, "A2");
			Assert.AreEqual (vs ["AllowCustomPaging"], true, "A3");
			p.AllowCustomPaging = false;
			Assert.AreEqual (vs.Count, 1, "A4");
			Assert.AreEqual (vs ["AllowCustomPaging"], false, "A5");

			p.AllowPaging = true;
			Assert.AreEqual (vs.Count, 2, "A6");
			Assert.AreEqual (vs ["AllowPaging"], true, "A7");
			p.AllowPaging = false;
			Assert.AreEqual (vs.Count, 2, "A8");
			Assert.AreEqual (vs ["AllowPaging"], false, "A9");

			p.AllowSorting = true;
			Assert.AreEqual (vs.Count, 3, "A10");
			Assert.AreEqual (vs ["AllowSorting"], true, "A11");
			p.AllowSorting = false;
			Assert.AreEqual (vs.Count, 3, "A12");
			Assert.AreEqual (vs ["AllowSorting"], false, "A13");

			p.AutoGenerateColumns = true;
			Assert.AreEqual (vs.Count, 4, "A14");
			Assert.AreEqual (vs ["AutoGenerateColumns"], true, "A15");
			p.AutoGenerateColumns = false;
			Assert.AreEqual (vs.Count, 4, "A16");
			Assert.AreEqual (vs ["AutoGenerateColumns"], false, "A17");

			p.CurrentPageIndex = 1;
			Assert.AreEqual (vs.Count, 5, "A18");
			Assert.AreEqual (vs ["CurrentPageIndex"], 1, "A19");

			p.EditItemIndex = 1;
			Assert.AreEqual (vs.Count, 6, "A20");
			Assert.AreEqual (vs ["EditItemIndex"], 1, "A20");

			p.PageSize = 25;
			Assert.AreEqual (vs.Count, 7, "A21");
			Assert.AreEqual (vs ["PageSize"], 25, "A22");

			p.SelectedIndex = 25;
			Assert.AreEqual (vs.Count, 8, "A23");
			Assert.AreEqual (vs ["SelectedIndex"], 25, "A24");

			p.ShowFooter = false;
			Assert.AreEqual (vs.Count, 9, "A25");
			Assert.AreEqual (vs ["ShowFooter"], false, "A26");
			p.ShowFooter = true;
			Assert.AreEqual (vs ["ShowFooter"], true, "A27");

			p.ShowHeader = false;
			Assert.AreEqual (vs.Count, 10, "A28");
			Assert.AreEqual (vs ["ShowHeader"], false, "A29");
			p.ShowHeader = true;
			Assert.AreEqual (vs ["ShowHeader"], true, "A30");

			p.VirtualItemCount = 100;
			Assert.AreEqual (vs.Count, 11, "A31");
			Assert.AreEqual (vs ["VirtualItemCount"], 100, "A32");
		}

		[Test]
		public void SelectIndexOutOfRange ()
		{
			DataGridPoker p = new DataGridPoker ();

			// No exception is thrown
			p.SelectedIndex = 25;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SelectItemOutOfRange ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataGridItem d;

			p.SelectedIndex = 25;
			d = p.SelectedItem;
		}

		[Test]
		public void ControlStyle ()
		{
			DataGridPoker p = new DataGridPoker ();

			Assert.AreEqual (p.ControlStyle ().GetType (),
					typeof (TableStyle), "A1");

			TableStyle t = (TableStyle) p.ControlStyle ();
			Assert.AreEqual (t.GridLines, GridLines.Both, "A2");
			Assert.AreEqual (t.CellSpacing, 0, "A3");
		}

		[Test]
		public void Styles ()
		{
			DataGridPoker p = new DataGridPoker ();
			StateBag vs = p.GetViewState ();
			
			p.BackImageUrl = "foobar url";

			// The styles get stored in the view state
#if NET_2_0
			Assert.AreEqual (vs.Count, 0, "A1");
			Assert.IsNull (vs ["BackImageUrl"], "A2");
			Assert.IsNull (vs ["GridLines"], "A3");
			Assert.IsNull (vs ["CellSpacing"], "A4");
#else
			Assert.AreEqual (vs.Count, 3, "A1");
			Assert.AreEqual (vs ["BackImageUrl"], "foobar url", "A2");
			Assert.AreEqual (vs ["GridLines"], GridLines.Both, "A3");
			Assert.AreEqual (vs ["CellSpacing"], 0, "A4");
#endif
		}

		private bool cancel_command;
		private bool delete_command;
		private bool edit_command;
		private bool item_command;
		private bool update_command;
		private bool item_created;
		private bool item_data_bound;
		private bool page_index_changed;
		private bool sort_command;
		private bool selected_changed;

		private int new_page_index;

		private void ResetEvents ()
		{
			cancel_command =
			delete_command =
			edit_command =
			item_command =
			update_command =
			item_created = 
			item_data_bound = 
			page_index_changed =
			sort_command =
			selected_changed = false;

			new_page_index = Int32.MinValue;
		}
				
		private void CancelCommandHandler (object sender, DataGridCommandEventArgs e)
		{
			cancel_command = true;
		}

		private void DeleteCommandHandler (object sender, DataGridCommandEventArgs e)
		{
			delete_command = true;
		}
		
		private void EditCommandHandler (object sender, DataGridCommandEventArgs e)
		{
			edit_command = true;
		}

		private void ItemCommandHandler (object sender, DataGridCommandEventArgs e)
		{
			item_command = true;
		}

		private void UpdateCommandHandler (object sender, DataGridCommandEventArgs e)
		{
			update_command = true;
		}

		private void ItemCreatedHandler (object sender, DataGridItemEventArgs e)
		{
			item_created = true;
		}

		private void ItemDataBoundHandler (object sender, DataGridItemEventArgs e)
		{
			item_data_bound = true;
		}
		
		private void PageIndexChangedHandler (object sender, DataGridPageChangedEventArgs e)
		{
			page_index_changed = true;
			new_page_index = e.NewPageIndex;
		}
		
		private void SortCommandHandler (object sender, DataGridSortCommandEventArgs e)
		{
			sort_command = true;
		}

		private void SelectedIndexChangedHandler (object sender, EventArgs e)
		{
			selected_changed = true;
		}

		[Test]
		public void Events ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataGridCommandEventArgs command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs (String.Empty, String.Empty));
			DataGridItemEventArgs item_args = new DataGridItemEventArgs (null);
			DataGridPageChangedEventArgs page_args = new DataGridPageChangedEventArgs (null, 0);
			DataGridSortCommandEventArgs sort_args = new DataGridSortCommandEventArgs (null,
					command_args);

			ResetEvents ();
			p.CancelCommand += new DataGridCommandEventHandler (CancelCommandHandler);
			p.DoCancelCommand (command_args);
			Assert.IsTrue (cancel_command, "A1");

			ResetEvents ();
			p.DeleteCommand += new DataGridCommandEventHandler (DeleteCommandHandler);
			p.DoDeleteCommand (command_args);
			Assert.IsTrue (delete_command, "A2");

			ResetEvents ();
			p.EditCommand += new DataGridCommandEventHandler (EditCommandHandler);
			p.DoEditCommand (command_args);
			Assert.IsTrue (edit_command, "A3");

			ResetEvents ();
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.DoItemCommand (command_args);
			Assert.IsTrue (item_command, "A4");

			ResetEvents ();
			p.UpdateCommand += new DataGridCommandEventHandler (UpdateCommandHandler);
			p.DoUpdateCommand (command_args);
			Assert.IsTrue (update_command, "A5");

			ResetEvents ();
			p.ItemCreated += new DataGridItemEventHandler (ItemCreatedHandler);
			p.DoItemCreated (item_args);
			Assert.IsTrue (item_created, "A6");

			ResetEvents ();
			p.ItemDataBound += new DataGridItemEventHandler (ItemDataBoundHandler);
			p.DoItemDataBound (item_args);
			Assert.IsTrue (item_data_bound, "A7");

			ResetEvents ();
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoPageIndexChanged (page_args);
			Assert.IsTrue (page_index_changed, "A8");

			ResetEvents ();
			p.SortCommand += new DataGridSortCommandEventHandler (SortCommandHandler);
			p.DoSortCommand (sort_args);
			Assert.IsTrue (sort_command, "A9");
		}

		[Test]
		public void BubbleEvent ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataGridCommandEventArgs command_args;

			//
			// Cancel
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("Cancel", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.CancelCommand += new DataGridCommandEventHandler (CancelCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (cancel_command, "A1");
			Assert.IsTrue (item_command, "#01");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("cancel", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.CancelCommand += new DataGridCommandEventHandler (CancelCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (cancel_command, "A2");
			Assert.IsTrue (item_command, "#02");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("CANCEL", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.CancelCommand += new DataGridCommandEventHandler (CancelCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (cancel_command, "A3");
			Assert.IsTrue (item_command, "#03");

			//
			// Delete
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("Delete", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.DeleteCommand += new DataGridCommandEventHandler (DeleteCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (delete_command, "A4");
			Assert.IsTrue (item_command, "#04");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("delete", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.DeleteCommand += new DataGridCommandEventHandler (DeleteCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (delete_command, "A5");
			Assert.IsTrue (item_command, "#05");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("DELETE", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.DeleteCommand += new DataGridCommandEventHandler (DeleteCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (delete_command, "A6");
			Assert.IsTrue (item_command, "#06");

			//
			// Edit
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("Edit", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.EditCommand += new DataGridCommandEventHandler (EditCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (edit_command, "A7");
			Assert.IsTrue (item_command, "#07");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("edit", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.EditCommand += new DataGridCommandEventHandler (EditCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (edit_command, "A8");
			Assert.IsTrue (item_command, "#08");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("EDIT", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.EditCommand += new DataGridCommandEventHandler (EditCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (edit_command, "A9");
			Assert.IsTrue (item_command, "#09");

			//
			// Item
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("Item", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (item_command, "A10");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("item", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (item_command, "A11");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("ITEM", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (item_command, "A12");

			//
			// Sort
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("Sort", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.SortCommand += new DataGridSortCommandEventHandler (SortCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (sort_command, "A13");
			Assert.IsTrue (item_command, "#10");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("sort", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.SortCommand += new DataGridSortCommandEventHandler (SortCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (sort_command, "A14");
			Assert.IsTrue (item_command, "#11");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("SORT", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.SortCommand += new DataGridSortCommandEventHandler (SortCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (sort_command, "A15");
			Assert.IsTrue (item_command, "#12");

			//
			// Update
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("Update", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.UpdateCommand += new DataGridCommandEventHandler (UpdateCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (update_command, "A16");
			Assert.IsTrue (item_command, "#13");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("update", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.UpdateCommand += new DataGridCommandEventHandler (UpdateCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (update_command, "A17");
			Assert.IsTrue (item_command, "#14");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("UPDATE", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.UpdateCommand += new DataGridCommandEventHandler (UpdateCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (update_command, "A18");
			Assert.IsTrue (item_command, "#15");

			//
			// Select
			//
			DataGridItem item = new DataGridItem (0, 0, ListItemType.Item);
			
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("Select", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.SelectedIndexChanged += new EventHandler (SelectedIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (selected_changed, "A19");
			Assert.IsTrue (item_command, "#16");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("select", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.SelectedIndexChanged += new EventHandler (SelectedIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (selected_changed, "A20");
			Assert.IsTrue (item_command, "#17");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("SELECT", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.SelectedIndexChanged += new EventHandler (SelectedIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (selected_changed, "A21");
			Assert.IsTrue (item_command, "#18");
		}

		[Test]
		public void BubblePageCommand ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataGridItem item = new DataGridItem (0, 0, ListItemType.Item);
			DataGridCommandEventArgs command_args;


			//
			// Prev
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("Page", "Prev"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A1");
			Assert.AreEqual (new_page_index, 9, "A2");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("page", "prev"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A3");
			Assert.AreEqual (new_page_index, 9, "A4");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("PAGE", "PREV"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A5");
			Assert.AreEqual (new_page_index, 9, "A6");

			
			//
			// Next
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("Page", "Next"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A5");
			Assert.AreEqual (new_page_index, 11, "A6");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("page", "next"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A7");
			Assert.AreEqual (new_page_index, 11, "A8");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("PAGE", "NEXT"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A9");
			Assert.AreEqual (new_page_index, 11, "A10");


			//
			// Specific
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("Page", "25"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A11");
			Assert.AreEqual (new_page_index, 24, "A12");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("Page", "0"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A11");
			Assert.AreEqual (new_page_index, -1, "A12");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void BadBubblePageArg ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataGridItem item = new DataGridItem (0, 0, ListItemType.Item);
			DataGridCommandEventArgs command_args;

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("Page", "i am bad"));

			p.DoBubbleEvent (this, command_args);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void BadBubblePageArg2 ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataGridItem item = new DataGridItem (0, 0, ListItemType.Item);
			DataGridCommandEventArgs command_args;

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("Page", new object ()));

			p.DoBubbleEvent (this, command_args);
		}

		[Test]
		public void SaveViewState ()
		{
			DataGridPoker p = new DataGridPoker ();

			p.TrackState ();

			object [] vs = (object []) p.SaveState ();
#if NET_2_0
			Assert.AreEqual (vs.Length, 11, "A1");
#else
			Assert.AreEqual (vs.Length, 10, "A1");
#endif

			// By default the viewstate is all null
			for (int i = 0; i < vs.Length; i++)
				Assert.IsNull (vs [i], "A2-" + i);

			//
			// TODO: What goes in the [1] and [9] slots?
			//

			p.AllowPaging = true;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [0], "A3");

			/*
			  This test doesn't work right now. It must be an issue
			  in the DataGridPagerStyle
			  
			p.PagerStyle.Visible = true;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [2], "A5");
			*/
			
			p.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [3], "A6");

			p.FooterStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [4], "A7");

			p.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [5], "A8");

			p.AlternatingItemStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [6], "A9");

			p.SelectedItemStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [7], "A10");

			p.EditItemStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [8], "A11");

			PagedDataSource source = new PagedDataSource ();
			DataTable table = new DataTable ();
			ArrayList columns;

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));
			source.DataSource = new DataView (table);
			columns = p.CreateColumns (source, true);

			vs = (object []) p.SaveState ();
#if NET_2_0
			Assert.IsNull (vs [9], "A12");
			p.BackImageUrl = "foobar url";
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [9], "A12");

			Assert.IsNotNull (vs [10], "A12");
			Assert.AreEqual (vs [10].GetType (), typeof (object []), "A12");

			object [] cols = (object []) vs [10];
			Assert.AreEqual (cols.Length, 3, "A13");
#else
			Assert.IsNotNull (vs [9], "A12");
			Assert.AreEqual (vs [9].GetType (), typeof (object []), "A12");

			object [] cols = (object []) vs [9];
			Assert.AreEqual (cols.Length, 3, "A13");
#endif
		}

		[Test]
		public void CreateColumnSet ()
		{
			DataGridPoker p = new DataGridPoker ();
			PagedDataSource source = new PagedDataSource ();
			DataTable table = new DataTable ();
			ArrayList columns;

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));

			source.DataSource = new DataView (table);

			columns = p.CreateColumns (source, true);
			Assert.AreEqual (columns.Count, 3, "A1");
			Assert.AreEqual (((DataGridColumn) columns [0]).HeaderText, "one", "A2");
			Assert.AreEqual (((DataGridColumn) columns [1]).HeaderText, "two", "A3");
			Assert.AreEqual (((DataGridColumn) columns [2]).HeaderText, "three", "A4");

			// AutoGenerated columns are not added to the ColumnsCollection
			Assert.AreEqual (p.Columns.Count, 0, "A5");
			
			// Without allowing data dinding,
			columns = p.CreateColumns (source, false);
			Assert.AreEqual (columns.Count, 3, "A6");
			Assert.AreEqual (((DataGridColumn) columns [0]).HeaderText, "one", "A7");
			Assert.AreEqual (((DataGridColumn) columns [1]).HeaderText, "two", "A8");
			Assert.AreEqual (((DataGridColumn) columns [2]).HeaderText, "three", "A9");


			// Mixing with already added columns
			p = new DataGridPoker ();
			DataGridColumn a = new ButtonColumn ();
			DataGridColumn b = new ButtonColumn ();

			a.HeaderText = "A";
			b.HeaderText = "B";
			p.Columns.Add (a);
			p.Columns.Add (b);

			columns = p.CreateColumns (source, true);
			Assert.AreEqual (columns.Count, 5, "A6");
			Assert.AreEqual (((DataGridColumn) columns [0]).HeaderText, "A", "A10");
			Assert.AreEqual (((DataGridColumn) columns [1]).HeaderText, "B", "A11");
			Assert.AreEqual (((DataGridColumn) columns [2]).HeaderText, "one", "A12");
			Assert.AreEqual (((DataGridColumn) columns [3]).HeaderText, "two", "A13");
			Assert.AreEqual (((DataGridColumn) columns [4]).HeaderText, "three", "A14");

			// Assigned properties of the newly created columns
			BoundColumn one = (BoundColumn) columns [2];

			Assert.AreEqual (one.HeaderText, "one", "A15");
			Assert.AreEqual (one.DataField, "one", "A16");
			Assert.AreEqual (one.DataFormatString, String.Empty, "A17");
			Assert.AreEqual (one.SortExpression, "one", "A18");
			Assert.AreEqual (one.HeaderImageUrl, String.Empty, "A19");
			Assert.AreEqual (one.FooterText, String.Empty, "A20");
		}

		[Test]
		public void CreateColumnsBinding ()
		{
			DataGridPoker p = new DataGridPoker ();
			PagedDataSource source = new PagedDataSource ();
			DataTable table = new DataTable ();
			ArrayList columns;

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));

			source.DataSource = new DataView (table);

			columns = p.CreateColumns (source, true);
			Assert.AreEqual (columns.Count, 3, "A1");
			Assert.AreEqual (((DataGridColumn) columns [0]).HeaderText, "one", "A2");
			Assert.AreEqual (((DataGridColumn) columns [1]).HeaderText, "two", "A3");
			Assert.AreEqual (((DataGridColumn) columns [2]).HeaderText, "three", "A4");

			table.Columns.Add (new DataColumn ("four", typeof (string)));
			table.Columns.Add (new DataColumn ("five", typeof (string)));
			table.Columns.Add (new DataColumn ("six", typeof (string)));

			// Just gets the old columns
			columns = p.CreateColumns (source, false);
			Assert.AreEqual (columns.Count, 3, "A5");
			Assert.AreEqual (((DataGridColumn) columns [0]).HeaderText, "one", "A6");
			Assert.AreEqual (((DataGridColumn) columns [1]).HeaderText, "two", "A7");
			Assert.AreEqual (((DataGridColumn) columns [2]).HeaderText, "three", "A8");

			columns = p.CreateColumns (source, true);
			Assert.AreEqual (columns.Count, 6, "A9");
			Assert.AreEqual (((DataGridColumn) columns [0]).HeaderText, "one", "A10");
			Assert.AreEqual (((DataGridColumn) columns [1]).HeaderText, "two", "A11");
			Assert.AreEqual (((DataGridColumn) columns [2]).HeaderText, "three", "A12");
			Assert.AreEqual (((DataGridColumn) columns [3]).HeaderText, "four", "A13");
			Assert.AreEqual (((DataGridColumn) columns [4]).HeaderText, "five", "A14");
			Assert.AreEqual (((DataGridColumn) columns [5]).HeaderText, "six", "A15");

			// Assigned properties of the newly created columns
			BoundColumn one = (BoundColumn) columns [0];

			Assert.AreEqual (one.HeaderText, "one", "A16");
			Assert.AreEqual (one.DataField, "one", "A17");
			Assert.AreEqual (one.DataFormatString, String.Empty, "A18");
			Assert.AreEqual (one.SortExpression, "one", "A19");
			Assert.AreEqual (one.HeaderImageUrl, String.Empty, "A20");
			Assert.AreEqual (one.FooterText, String.Empty, "A21");
		}

		[Test]
		public void CreateSimpleColumns ()
		{
			DataGridPoker p = new DataGridPoker ();
			PagedDataSource source = new PagedDataSource ();
			ArrayList list = new ArrayList ();
			ArrayList columns;
			
			list.Add ("One");
			list.Add ("Two");
			list.Add ("Three");

			source.DataSource = list;
			columns = p.CreateColumns (source, true);
			Assert.AreEqual (1, columns.Count, "A1");
			Assert.AreEqual ("Item", ((DataGridColumn) columns [0]).HeaderText, "A2");

			AmazingEnumerable amazing = new AmazingEnumerable (list);

			source.DataSource = amazing;
			columns = p.CreateColumns (source, true);
			Assert.AreEqual (1, columns.Count, "A3");

			BoundColumn one = (BoundColumn) columns [0];

			Assert.AreEqual ("Item", one.HeaderText, "A4");

			// I guess this makes it bind to itself ?
			Assert.AreEqual (BoundColumn.thisExpr, one.DataField, "A5"); 

			Assert.AreEqual (String.Empty, one.DataFormatString, "A6");
			Assert.AreEqual ("Item", one.SortExpression, "A7");
			Assert.AreEqual (String.Empty, one.HeaderImageUrl, "A8");
			Assert.AreEqual (String.Empty, one.FooterText, "A9");
			Assert.AreEqual ("Item", one.HeaderText, "A10");

			source.DataSource = new ArrayList ();
			columns = p.CreateColumns (source, true);
			Assert.AreEqual (0, columns.Count, "A11");
		}

		[Test]
		public void DataBindingEnumerator ()
		{
			DataGridPoker p = new DataGridPoker ();
			PagedDataSource source = new PagedDataSource ();
			ArrayList list = new ArrayList ();
			ArrayList columns;
			
			list.Add ("One");
			list.Add ("Two");
			list.Add ("Three");

			AmazingEnumerable amazing = new AmazingEnumerable (list);
			source.DataSource = amazing;
			columns = p.CreateColumns (source, true);
			Assert.AreEqual (1, columns.Count, "A1");
			Assert.AreEqual ("Item", ((DataGridColumn) columns [0]).HeaderText, "A2");
			Assert.AreEqual (1, amazing.CallCount, "A3");
			Assert.AreEqual (0, p.DataKeys.Count, "A4");
		}

		class Custom : ICustomTypeDescriptor {
			public AttributeCollection GetAttributes ()
			{
				throw new Exception ();
			}

			public string GetClassName()
			{
				throw new Exception ();
			}

			public string GetComponentName()
			{
				throw new Exception ();
			}

			public TypeConverter GetConverter()
			{
				throw new Exception ();
			}

			public EventDescriptor GetDefaultEvent()
			{
				throw new Exception ();
			}

			public PropertyDescriptor GetDefaultProperty()
			{
				throw new Exception ();
			}

			public object GetEditor (Type editorBaseType)
			{
				throw new Exception ();
			}

			public EventDescriptorCollection GetEvents ()
			{
				throw new Exception ();
			}

			public EventDescriptorCollection GetEvents (Attribute[] arr)
			{
				throw new Exception ();
			}

			public int CallCount;
			public PropertyDescriptorCollection GetProperties()
			{
				// MS calls this one
				if (CallCount++ > 0)
					throw new Exception ("This should not happen");
				PropertyDescriptorCollection coll = new PropertyDescriptorCollection (null);
				coll.Add (new MyPropertyDescriptor ());
				return coll;
			}

			public PropertyDescriptorCollection GetProperties (Attribute[] arr)
			{
				// We call this one
				return GetProperties ();
			}

			public object GetPropertyOwner (PropertyDescriptor pd)
			{
				throw new Exception ();
			}
		}

		class MyPropertyDescriptor : PropertyDescriptor {
			int val;

			public MyPropertyDescriptor () : base ("CustomName", null)
			{
			}

			public override Type ComponentType {
				get { return typeof (MyPropertyDescriptor); }
			}

			public override bool IsReadOnly {
				get { return true; }
			}

			public override Type PropertyType {
				get { return typeof (int); }
			}

			public override object GetValue (object component)
			{
				return val++;
			}

			public override void SetValue (object component, object value)
			{
			}

			public override void ResetValue (object component)
			{
			}

			public override bool CanResetValue (object component)
			{
				return false;
			}

			public override bool ShouldSerializeValue (object component)
			{
				return false;
			}
		}

		class MyEnumerable : IEnumerable {
			public object Item;
			public IEnumerator GetEnumerator ()
			{
				ArrayList list = new ArrayList ();
				list.Add (Item);
				return list.GetEnumerator ();
			}
		}

		[Test]
		public void DataBindingCustomElement ()
		{
			DataGridPoker p = new DataGridPoker ();
			p.DataKeyField = "CustomName";
			PagedDataSource source = new PagedDataSource ();
			MyEnumerable myenum = new MyEnumerable ();
			myenum.Item = new Custom ();
			source.DataSource = myenum;
			ArrayList columns = p.CreateColumns (source, true);
			Assert.AreEqual (1, columns.Count, "A1");
			Assert.AreEqual ("CustomName", ((DataGridColumn) columns [0]).HeaderText, "A2");
			Assert.AreEqual (0, p.DataKeys.Count, "A3");
		}

#if NET_2_0
		public class data
		{
			private static ArrayList _data = new ArrayList ();

			static data () {
				_data.Add (new DataItem (1, "heh1"));
				_data.Add (new DataItem (2, "heh2"));
				_data.Add (new DataItem (3, "heh3"));
				_data.Add (new DataItem (4, "heh4"));
				_data.Add (new DataItem (5, "heh5"));
				_data.Add (new DataItem (6, "heh6"));
				_data.Add (new DataItem (7, "heh7"));
				_data.Add (new DataItem (8, "heh8"));
				_data.Add (new DataItem (9, "heh9"));
				_data.Add (new DataItem (10, "heh10"));
			}

			public data () {
			}

			public ArrayList GetAllItems () {
				return _data;
			}

			public ArrayList GetPagedItems (int startIndex, int maxRows) 
			{
				ArrayList list = new ArrayList ();
				if (startIndex < _data.Count - 1) {
					int countToReturn = Math.Min (maxRows, _data.Count - startIndex);
					for (int i = startIndex; i < startIndex + countToReturn; i++) {
						list.Add (_data [i]);
					}
				}

				return list;
			}

			public int GetCount () 
			{
				return _data.Count;
			}

			public void UpdateItem (int id, string name) {
				foreach (DataItem i in _data) {
					if (i.ID == id) {
						i.Name = name;
						return;
					}
				}
			}
		}

		public class DataItem
		{
			int _id = 0;
			string _name = "";

			public DataItem (int id, string name) {
				_id = id;
				_name = name;
			}

			public int ID {
				get { return _id; }
			}

			public string Name {
				get { return _name; }
				set { _name = value; }
			}
		}

		public class DataSourceObject
		{
			public static List<string> GetList (string sortExpression, int startRowIndex, int maximumRows) {
				return GetList ();
			}

			public static List<string> GetList (int startRowIndex, int maximumRows) {
				return GetList ();
			}

			public static List<string> GetList (string sortExpression) {
				return GetList ();
			}

			public static List<string> GetList () {
				List<string> list = new List<string> ();
				list.Add ("Norway");
				list.Add ("Sweden");
				list.Add ("France");
				list.Add ("Italy");
				list.Add ("Israel");
				list.Add ("Russia");
				return list;
			}

			public static int GetCount () {
				return GetList ().Count;
			}
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void DataSourceAndDataSourceID () 
		{
			Page page = new Page ();
			DataGridPoker dg = new DataGridPoker ();
			
			page.Controls.Add (dg);

			DataTable dt = new DataTable ();
			dt.Columns.Add (new DataColumn ("something", typeof (Int32)));
			DataRow dr = dt.NewRow ();
			dt.Rows.Add (new object [] { 1 });
			DataView dv = new DataView (dt);

			dg.DataSource = dv;

			ObjectDataSource ds = new ObjectDataSource ();
			ds.TypeName = typeof (DataSourceObject).AssemblyQualifiedName;
			ds.SelectMethod = "GetList";
			ds.SortParameterName = "sortExpression";
			ds.ID = "Data";
			page.Controls.Add (ds);

			dg.DataSourceID = "Data";

			dg.DataBind ();
		}

		[Test]
		public void DataBindingDataSourceID () 
		{
			Page page = new Page ();
			DataGridPoker dg = new DataGridPoker ();
			page.Controls.Add (dg);

			ObjectDataSource ds = new ObjectDataSource ();
			ds.TypeName = typeof (DataSourceObject).AssemblyQualifiedName;
			ds.SelectMethod = "GetList";
			ds.SortParameterName = "sortExpression";
			ds.ID = "Data";
			page.Controls.Add (ds);

			dg.DataSourceID = "Data";
			dg.DataBind ();

			Assert.AreEqual (6, dg.Items.Count, "DataBindingDataSourceID");
		}

		[Test]
		[NUnit.Framework.Category ("NunitWeb")]
		public void DataBindingDataSourceIDAutomatic () 
		{
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates();
			pd.Load = DataSourceIDAutomatic_Load;
			pd.PreRender = DataSourceIDAutomatic_PreRender;
			t.Invoker = new PageInvoker (pd);

			t.Run ();
		}

		public static void DataSourceIDAutomatic_Load (Page page) 
		{
			DataGridPoker dg = new DataGridPoker ();
			dg.ID = "DataGrid";
			page.Controls.Add (dg);

			ObjectDataSource ds = new ObjectDataSource ();
			ds.TypeName = typeof (DataSourceObject).AssemblyQualifiedName;
			ds.SelectMethod = "GetList";
			ds.SortParameterName = "sortExpression";
			ds.ID = "Data";
			page.Controls.Add (ds);

			dg.DataSourceID = "Data";
		}

		public static void DataSourceIDAutomatic_PreRender (Page page) 
		{
			DataGrid dg = (DataGrid)page.FindControl ("DataGrid");

			Assert.AreEqual (6, dg.Items.Count, "DataBindingDataSourceID");
		}

		[Test]
		public void DataSourceIDBindingNoColumns () 
		{
			Page page = new Page ();
			DataGridPoker dg = new DataGridPoker ();
			dg.ID = "DataGrid";
			dg.AutoGenerateColumns = false;

			page.Controls.Add (dg);

			ObjectDataSource ds = new ObjectDataSource ();
			ds.TypeName = typeof (DataSourceObject).AssemblyQualifiedName;
			ds.SelectMethod = "GetList";
			ds.SortParameterName = "sortExpression";
			ds.ID = "Data";
			page.Controls.Add (ds);

			dg.DataSourceID = "Data";
			dg.DataBind ();

			Assert.AreEqual (0, dg.Columns.Count, "Columns Count");
			Assert.AreEqual (0, dg.Items.Count, "Items Count");
		}

		[Test]
		public void DataSourceIDBindingManualColumns () 
		{
			Page page = new Page ();
			DataGridPoker dg = new DataGridPoker ();
			dg.ID = "DataGrid";
			dg.AutoGenerateColumns = false;
			BoundColumn col = new BoundColumn();
			col.DataField = "something";
			dg.Columns.Add (col);

			page.Controls.Add (dg);

			DataTable dt = new DataTable ();
			dt.Columns.Add (new DataColumn ("something", typeof (Int32)));
			DataRow dr = dt.NewRow ();
			dt.Rows.Add (new object [] { 1 });
			dt.Rows.Add (new object [] { 2 });
			dt.Rows.Add (new object [] { 3 });
			dt.Rows.Add (new object [] { 4 });
			dt.Rows.Add (new object [] { 5 });
			dt.Rows.Add (new object [] { 6 });

			DataView dv = new DataView (dt);

			dg.DataSource = dv;
			dg.DataBind ();

			Assert.AreEqual (1, dg.Columns.Count, "Columns Count");
			Assert.AreEqual (6, dg.Items.Count, "Items Count");
			Assert.AreEqual ("1", dg.Items[0].Cells[0].Text, "Cell content");
		}

		[Test]
		[NUnit.Framework.Category ("NunitWeb")]
		public void Paging ()
		{
			WebTest t = new WebTest ("DataGrid.aspx");
			t.Invoker = PageInvoker.CreateOnInit (DataGrid_OnInit);
			string html = t.Run ();
			string gridHtml = HtmlDiff.GetControlFromPageHtml (html);
			string expectedFirstPage = @"<table cellspacing=""0"" cellpadding=""4"" border=""0"" id=""DataGrid1"" style=""color:#333333;border-collapse:collapse;"">
	<tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>ID</td><td>Name</td><td>&nbsp;</td><td>&nbsp;</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl00','')"" style=""color:White;"">ID</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl01','')"" style=""color:White;"">Name</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl02','')"" style=""color:White;"">Comment</a></td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>1</td><td>heh1</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>1</td><td>heh1</td><td>Comment 1</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>2</td><td>heh2</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>2</td><td>heh2</td><td>Comment 2</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>3</td><td>heh3</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>3</td><td>heh3</td><td>Comment 3</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>4</td><td>heh4</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>4</td><td>heh4</td><td>Comment 4</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>5</td><td>heh5</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>5</td><td>heh5</td><td>Comment 5</td>
	</tr><tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td>
	</tr><tr align=""center"" style=""color:#333333;background-color:#FFCC66;"">
		<td colspan=""4""><span>Previous</span>&nbsp;<a href=""javascript:__doPostBack('DataGrid1$ctl09$ctl01','')"" style=""color:#333333;"">Next</a></td>
	</tr>
</table>";

			HtmlDiff.AssertAreEqual (expectedFirstPage, gridHtml, "DataGrid initial Render");

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
#if DOT_NET
			fr.Controls ["__EVENTTARGET"].Value = "DataGrid1$ctl09$ctl01"; 
#else
			fr.Controls ["__EVENTTARGET"].Value = "DataGrid1$ctl08$ctl01";
#endif
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;

			html = t.Run ();
			gridHtml = HtmlDiff.GetControlFromPageHtml (html);
			string expectedSecondPage = @"<table cellspacing=""0"" cellpadding=""4"" border=""0"" id=""DataGrid1"" style=""color:#333333;border-collapse:collapse;"">
	<tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>ID</td><td>Name</td><td>&nbsp;</td><td>&nbsp;</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl00','')"" style=""color:White;"">ID</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl01','')"" style=""color:White;"">Name</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl02','')"" style=""color:White;"">Comment</a></td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>6</td><td>heh6</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>6</td><td>heh6</td><td>Comment 6</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>7</td><td>heh7</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>7</td><td>heh7</td><td>Comment 7</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>8</td><td>heh8</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>8</td><td>heh8</td><td>Comment 8</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>9</td><td>heh9</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>9</td><td>heh9</td><td>Comment 9</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>10</td><td>heh10</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>10</td><td>heh10</td><td>Comment 10</td>
	</tr><tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td>
	</tr><tr align=""center"" style=""color:#333333;background-color:#FFCC66;"">
		<td colspan=""4""><a href=""javascript:__doPostBack('DataGrid1$ctl09$ctl00','')"" style=""color:#333333;"">Previous</a>&nbsp;<span>Next</span></td>
	</tr>
</table>";
			HtmlDiff.AssertAreEqual (expectedSecondPage, gridHtml, "DataGrid Paging Next");

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
#if DOT_NET
			fr.Controls ["__EVENTTARGET"].Value = "DataGrid1$ctl09$ctl00"; 
#else
			fr.Controls ["__EVENTTARGET"].Value = "DataGrid1$ctl08$ctl00";
#endif
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;

			html = t.Run ();
			gridHtml = HtmlDiff.GetControlFromPageHtml (html);

			HtmlDiff.AssertAreEqual (expectedFirstPage, gridHtml, "DataGrid Paging Previous");
		}

		[Test]
		[NUnit.Framework.Category ("NunitWeb")]
		public void EditUpdateDelete ()
		{
			WebTest t = new WebTest ("DataGrid.aspx");
			t.Invoker = PageInvoker.CreateOnInit (DataGrid_OnInit);
			string html = t.Run ();
			string gridHtml = HtmlDiff.GetControlFromPageHtml (html);
			string expectedFirstPage = @"<table cellspacing=""0"" cellpadding=""4"" border=""0"" id=""DataGrid1"" style=""color:#333333;border-collapse:collapse;"">
	<tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>ID</td><td>Name</td><td>&nbsp;</td><td>&nbsp;</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl00','')"" style=""color:White;"">ID</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl01','')"" style=""color:White;"">Name</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl02','')"" style=""color:White;"">Comment</a></td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>1</td><td>heh1</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>1</td><td>heh1</td><td>Comment 1</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>2</td><td>heh2</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>2</td><td>heh2</td><td>Comment 2</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>3</td><td>heh3</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>3</td><td>heh3</td><td>Comment 3</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>4</td><td>heh4</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>4</td><td>heh4</td><td>Comment 4</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>5</td><td>heh5</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>5</td><td>heh5</td><td>Comment 5</td>
	</tr><tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td>
	</tr><tr align=""center"" style=""color:#333333;background-color:#FFCC66;"">
		<td colspan=""4""><span>Previous</span>&nbsp;<a href=""javascript:__doPostBack('DataGrid1$ctl09$ctl01','')"" style=""color:#333333;"">Next</a></td>
	</tr>
</table>";

			HtmlDiff.AssertAreEqual (expectedFirstPage, gridHtml, "DataGrid initial Render");

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
#if DOT_NET
			fr.Controls ["__EVENTTARGET"].Value = "DataGrid1$ctl03$ctl00";
#else
			fr.Controls ["__EVENTTARGET"].Value = "DataGrid1$ctl02$ctl00";
#endif
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;

			html = t.Run ();
			gridHtml = HtmlDiff.GetControlFromPageHtml (html);
			string expectedSecondPage = @"<table cellspacing=""0"" cellpadding=""4"" border=""0"" id=""DataGrid1"" style=""color:#333333;border-collapse:collapse;"">
	<tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>ID</td><td>Name</td><td>&nbsp;</td><td>&nbsp;</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl00','')"" style=""color:White;"">ID</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl01','')"" style=""color:White;"">Name</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl02','')"" style=""color:White;"">Comment</a></td>
	</tr><tr style=""color:#333333;background-color:Green;font-weight:normal;font-style:normal;text-decoration:none;"">
		<td>1</td><td><input name=""DataGrid1$ctl03$ctl00"" type=""text"" value=""heh1"" /></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl01','')"" style=""color:#333333;"">Update</a>&nbsp;<a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl02','')"" style=""color:#333333;"">Cancel</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl03','')"" style=""color:#333333;"">Delete</a></td><td>1</td><td><input name=""DataGrid1$ctl03$ctl04"" type=""text"" value=""heh1"" /></td><td><input name=""DataGrid1$ctl03$ctl05"" type=""text"" value=""Comment 1"" /></td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>2</td><td>heh2</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>2</td><td>heh2</td><td>Comment 2</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>3</td><td>heh3</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>3</td><td>heh3</td><td>Comment 3</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>4</td><td>heh4</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>4</td><td>heh4</td><td>Comment 4</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>5</td><td>heh5</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>5</td><td>heh5</td><td>Comment 5</td>
	</tr><tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td>
	</tr><tr align=""center"" style=""color:#333333;background-color:#FFCC66;"">
		<td colspan=""4""><span>Previous</span>&nbsp;<a href=""javascript:__doPostBack('DataGrid1$ctl09$ctl01','')"" style=""color:#333333;"">Next</a></td>
	</tr>
</table>";
			HtmlDiff.AssertAreEqual (expectedSecondPage, gridHtml, "DataGrid Edit");

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
#if DOT_NET
			fr.Controls ["__EVENTTARGET"].Value = "DataGrid1$ctl03$ctl01";
#else
			fr.Controls ["__EVENTTARGET"].Value = "DataGrid1$ctl02$ctl01";
#endif
			fr.Controls ["__EVENTARGUMENT"].Value = "";
#if DOT_NET
			fr.Controls.Add ("DataGrid1$ctl03$ctl00");
			fr.Controls ["DataGrid1$ctl03$ctl00"].Value = "New Value";
#else
			fr.Controls.Add ("DataGrid1$ctl02$ctl00");
			fr.Controls ["DataGrid1$ctl02$ctl00"].Value = "New Value";
#endif
			t.Request = fr;

			html = t.Run ();
			gridHtml = HtmlDiff.GetControlFromPageHtml (html);
			string expectedThirdPage = @"
        <table cellspacing=""0"" cellpadding=""4"" border=""0"" id=""DataGrid1"" style=""color:#333333;border-collapse:collapse;"">
	<tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>ID</td><td>Name</td><td>&nbsp;</td><td>&nbsp;</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl00','')"" style=""color:White;"">ID</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl01','')"" style=""color:White;"">Name</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl02','')"" style=""color:White;"">Comment</a></td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>1</td><td>New Value</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>1</td><td>New Value</td><td>Comment 1</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>2</td><td>heh2</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>2</td><td>heh2</td><td>Comment 2</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>3</td><td>heh3</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>3</td><td>heh3</td><td>Comment 3</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>4</td><td>heh4</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>4</td><td>heh4</td><td>Comment 4</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>5</td><td>heh5</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>5</td><td>heh5</td><td>Comment 5</td>
	</tr><tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td>
	</tr><tr align=""center"" style=""color:#333333;background-color:#FFCC66;"">
		<td colspan=""4""><span>Previous</span>&nbsp;<a href=""javascript:__doPostBack('DataGrid1$ctl09$ctl01','')"" style=""color:#333333;"">Next</a></td>
	</tr>
</table>";

			HtmlDiff.AssertAreEqual (expectedThirdPage, gridHtml, "DataGrid Update");

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
#if DOT_NET
			fr.Controls ["__EVENTTARGET"].Value = "DataGrid1$ctl04$ctl01";
#else
			fr.Controls ["__EVENTTARGET"].Value = "DataGrid1$ctl03$ctl01";
#endif
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;

			html = t.Run ();
			gridHtml = HtmlDiff.GetControlFromPageHtml (html);
			string expectedFourthPage = @"
        <table cellspacing=""0"" cellpadding=""4"" border=""0"" id=""DataGrid1"" style=""color:#333333;border-collapse:collapse;"">
	<tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>ID</td><td>Name</td><td>&nbsp;</td><td>&nbsp;</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl00','')"" style=""color:White;"">ID</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl01','')"" style=""color:White;"">Name</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl02','')"" style=""color:White;"">Comment</a></td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>1</td><td>New Value</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>1</td><td>New Value</td><td>Comment 1</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>3</td><td>heh3</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>3</td><td>heh3</td><td>Comment 3</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>4</td><td>heh4</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>4</td><td>heh4</td><td>Comment 4</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>5</td><td>heh5</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>5</td><td>heh5</td><td>Comment 5</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>6</td><td>heh6</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>6</td><td>heh6</td><td>Comment 6</td>
	</tr><tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td>
	</tr><tr align=""center"" style=""color:#333333;background-color:#FFCC66;"">
		<td colspan=""4""><span>Previous</span>&nbsp;<a href=""javascript:__doPostBack('DataGrid1$ctl09$ctl01','')"" style=""color:#333333;"">Next</a></td>
	</tr>
</table>";

			HtmlDiff.AssertAreEqual (expectedFourthPage, gridHtml, "DataGrid Delete");
		}

		[Test]
		[NUnit.Framework.Category ("NunitWeb")]
		public void SelectedIndex ()
		{
			WebTest t = new WebTest ("DataGrid.aspx");
			t.Invoker = PageInvoker.CreateOnInit (DataGrid_OnInit);
			string html = t.Run ();
			string gridHtml = HtmlDiff.GetControlFromPageHtml (html);
			string expectedFirstPage = @"<table cellspacing=""0"" cellpadding=""4"" border=""0"" id=""DataGrid1"" style=""color:#333333;border-collapse:collapse;"">
	<tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>ID</td><td>Name</td><td>&nbsp;</td><td>&nbsp;</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl00','')"" style=""color:White;"">ID</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl01','')"" style=""color:White;"">Name</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl02','')"" style=""color:White;"">Comment</a></td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>1</td><td>heh1</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>1</td><td>heh1</td><td>Comment 1</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>2</td><td>heh2</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>2</td><td>heh2</td><td>Comment 2</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>3</td><td>heh3</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>3</td><td>heh3</td><td>Comment 3</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>4</td><td>heh4</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>4</td><td>heh4</td><td>Comment 4</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>5</td><td>heh5</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>5</td><td>heh5</td><td>Comment 5</td>
	</tr><tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td>
	</tr><tr align=""center"" style=""color:#333333;background-color:#FFCC66;"">
		<td colspan=""4""><span>Previous</span>&nbsp;<a href=""javascript:__doPostBack('DataGrid1$ctl09$ctl01','')"" style=""color:#333333;"">Next</a></td>
	</tr>
</table>";

			HtmlDiff.AssertAreEqual (expectedFirstPage, gridHtml, "DataGrid initial Render");

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls ["__EVENTTARGET"].Value = "Button1";
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;

			html = t.Run ();
			gridHtml = HtmlDiff.GetControlFromPageHtml (html);
			string expectedSecondPage = @"
        <table cellspacing=""0"" cellpadding=""4"" border=""0"" id=""DataGrid1"" style=""color:#333333;border-collapse:collapse;"">
	<tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>ID</td><td>Name</td><td>&nbsp;</td><td>&nbsp;</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl00','')"" style=""color:White;"">ID</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl01','')"" style=""color:White;"">Name</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl02','')"" style=""color:White;"">Comment</a></td>
	</tr><tr style=""color:Navy;background-color:#FFCC66;font-weight:bold;"">
		<td>1</td><td>heh1</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl00','')"" style=""color:Navy;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl01','')"" style=""color:Navy;"">Delete</a></td><td>1</td><td>heh1</td><td>Comment 1</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>2</td><td>heh2</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>2</td><td>heh2</td><td>Comment 2</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>3</td><td>heh3</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>3</td><td>heh3</td><td>Comment 3</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>4</td><td>heh4</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>4</td><td>heh4</td><td>Comment 4</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>5</td><td>heh5</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>5</td><td>heh5</td><td>Comment 5</td>
	</tr><tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td>
	</tr><tr align=""center"" style=""color:#333333;background-color:#FFCC66;"">
		<td colspan=""4""><span>Previous</span>&nbsp;<a href=""javascript:__doPostBack('DataGrid1$ctl09$ctl01','')"" style=""color:#333333;"">Next</a></td>
	</tr>
</table>";
			HtmlDiff.AssertAreEqual (expectedSecondPage, gridHtml, "DataGrid Selected 1");

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls ["__EVENTTARGET"].Value = "Button1";
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;

			html = t.Run ();
			gridHtml = HtmlDiff.GetControlFromPageHtml (html);
			string expectedThirdPage = @"
        <table cellspacing=""0"" cellpadding=""4"" border=""0"" id=""DataGrid1"" style=""color:#333333;border-collapse:collapse;"">
	<tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>ID</td><td>Name</td><td>&nbsp;</td><td>&nbsp;</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl00','')"" style=""color:White;"">ID</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl01','')"" style=""color:White;"">Name</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl02$ctl02','')"" style=""color:White;"">Comment</a></td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>1</td><td>heh1</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl03$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>1</td><td>heh1</td><td>Comment 1</td>
	</tr><tr style=""color:Navy;background-color:#FFCC66;font-weight:bold;"">
		<td>2</td><td>heh2</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl00','')"" style=""color:Navy;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl04$ctl01','')"" style=""color:Navy;"">Delete</a></td><td>2</td><td>heh2</td><td>Comment 2</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>3</td><td>heh3</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl05$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>3</td><td>heh3</td><td>Comment 3</td>
	</tr><tr style=""color:#333333;background-color:White;"">
		<td>4</td><td>heh4</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl06$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>4</td><td>heh4</td><td>Comment 4</td>
	</tr><tr style=""color:#333333;background-color:#FFFBD6;"">
		<td>5</td><td>heh5</td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl00','')"" style=""color:#333333;"">Edit</a></td><td><a href=""javascript:__doPostBack('DataGrid1$ctl07$ctl01','')"" style=""color:#333333;"">Delete</a></td><td>5</td><td>heh5</td><td>Comment 5</td>
	</tr><tr style=""color:White;background-color:#990000;font-weight:bold;"">
		<td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td>
	</tr><tr align=""center"" style=""color:#333333;background-color:#FFCC66;"">
		<td colspan=""4""><span>Previous</span>&nbsp;<a href=""javascript:__doPostBack('DataGrid1$ctl09$ctl01','')"" style=""color:#333333;"">Next</a></td>
	</tr>
</table>";

			HtmlDiff.AssertAreEqual (expectedThirdPage, gridHtml, "DataGrid Selected 2");

		}

		public static void DataGrid_OnInit (Page p) 
		{
			if (!p.IsPostBack)
				MyDataSource.Init ();
			DataGrid DataGrid1 = (DataGrid)p.FindControl ("DataGrid1");
			DataGrid1.PageIndexChanged += new DataGridPageChangedEventHandler (DataGrid1_PageIndexChanged);
			DataGrid1.CancelCommand += new DataGridCommandEventHandler (DataGrid1_CancelCommand);
			DataGrid1.DeleteCommand += new DataGridCommandEventHandler (DataGrid1_DeleteCommand);
			DataGrid1.EditCommand += new DataGridCommandEventHandler (DataGrid1_EditCommand);
			DataGrid1.UpdateCommand += new DataGridCommandEventHandler (DataGrid1_UpdateCommand);
			DataGrid1.ItemCreated += new DataGridItemEventHandler (DataGrid1_ItemCreated);
		}

		public static void DataGrid1_ItemCreated (object sender, DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Pager) {
				e.Item.Cells [0].ColumnSpan = 4;
			}
		}

		public static void DataGrid1_PageIndexChanged (object source, DataGridPageChangedEventArgs e) 
		{
			DataGrid DataGrid1 = (DataGrid) source;
			DataGrid1.CurrentPageIndex = e.NewPageIndex;
			DataGrid1.DataBind ();
		}

		public static void DataGrid1_EditCommand (object source, DataGridCommandEventArgs e) 
		{
			DataGrid DataGrid1 = (DataGrid) source;
			DataGrid1.EditItemIndex = e.Item.ItemIndex;
			DataGrid1.DataBind ();
		}

		public static void DataGrid1_DeleteCommand (object source, DataGridCommandEventArgs e) 
		{
			DataGrid DataGrid1 = (DataGrid) source;
			MyDataSource ds = new MyDataSource ();
			ds.DeleteItem (e.Item.DataSetIndex);
			DataGrid1.DataBind ();
		}

		public static void DataGrid1_UpdateCommand (object source, DataGridCommandEventArgs e) 
		{
			DataGrid DataGrid1 = (DataGrid) source;
			MyDataSource ds = new MyDataSource ();
			TextBox edittedName = (TextBox) e.Item.Cells [1].Controls [0];
			ds.UpdateItem (e.Item.DataSetIndex, Int32.Parse (e.Item.Cells [0].Text), edittedName.Text);
			DataGrid1.EditItemIndex = -1;
			DataGrid1.DataBind ();
		}

		public static void DataGrid1_CancelCommand (object source, DataGridCommandEventArgs e) 
		{
			DataGrid DataGrid1 = (DataGrid) source;
			DataGrid1.EditItemIndex = -1;
			DataGrid1.DataBind ();
		}

		public class MyDataSource
		{
			private static ArrayList _data;

			static MyDataSource () 
			{
				Init ();
			}

			public static void Init ()
			{
				_data = new ArrayList ();
				_data.Add (new MyDataItem (1, "heh1", "Comment 1"));
				_data.Add (new MyDataItem (2, "heh2", "Comment 2"));
				_data.Add (new MyDataItem (3, "heh3", "Comment 3"));
				_data.Add (new MyDataItem (4, "heh4", "Comment 4"));
				_data.Add (new MyDataItem (5, "heh5", "Comment 5"));
				_data.Add (new MyDataItem (6, "heh6", "Comment 6"));
				_data.Add (new MyDataItem (7, "heh7", "Comment 7"));
				_data.Add (new MyDataItem (8, "heh8", "Comment 8"));
				_data.Add (new MyDataItem (9, "heh9", "Comment 9"));
				_data.Add (new MyDataItem (10, "heh10", "Comment 10"));
			}

			public MyDataSource () 
			{
			}

			public ArrayList GetAllItems () 
			{
				return _data;
			}

			public int GetCount () 
			{
				return _data.Count;
			}

			public void UpdateItem (int itemIndex, int id, string name) 
			{
				if (itemIndex >= 0 && itemIndex < _data.Count) {
					MyDataItem item = (MyDataItem) _data [itemIndex];
					item.Name = name;
					return;
				}
			}

			public void DeleteItem (int p) 
			{
				_data.RemoveAt (p);
			}
		}

		public class MyDataItem
		{
			int _id = 0;
			string _name = "";
			string _comment = "";

			public MyDataItem (int id, string name, string comment) 
			{
				_id = id;
				_name = name;
				_comment = comment;
			}

			public int ID {
				get { return _id; }
			}

			public string Name {
				get { return _name; }
				set { _name = value; }
			}

			public string Comment {
				get { return _comment; }
				set { _comment = value; }
			}
		}

#endif

		class MyTemplate : ITemplate {
			string text;
			public MyTemplate (string text)
			{
				this.text = text;
			}

			public void InstantiateIn (Control control)
			{
				control.Controls.Add (new LiteralControl (text));	
			}
		}

		[Test]
		public void OneTemplateColumn1 ()
		{
			DataGridPoker p = new DataGridPoker ();
			TemplateColumn tc = new TemplateColumn ();
			tc.ItemTemplate = new MyTemplate ("hola");
			p.Columns.Add (tc);
			ControlCollection controls = p.Controls;
			p.CreateControls (true);
			Assert.AreEqual (1, p.Columns.Count, "columns");
			Assert.AreEqual (0, controls.Count, "controls");
			string render = p.Render ();
			// no items, even with a templated column.
			// The table is not added if DataSource == null
			Assert.IsTrue (-1 == render.IndexOf ("hola"), "template");
		}

		[Test]
		public void OneTemplateColumn2 ()
		{
			DataGridPoker p = new DataGridPoker ();
			p.ShowFooter = true;
			p.AutoGenerateColumns = false;
			p.DataSource = new ArrayList ();
			TemplateColumn tc = new TemplateColumn ();
			tc.HeaderText = " ";
			tc.FooterTemplate = new MyTemplate ("hola");
			p.Columns.Add (tc);
			Assert.AreEqual (1, p.Columns.Count, "columns-1");
			Assert.AreEqual (0, p.Controls.Count, "controls-1");
			p.CreateControls (true);
			// This time we have the table there. Thanks to the empty ArrayList
			Assert.AreEqual (1, p.Columns.Count, "columns-2");
			Assert.AreEqual (1, p.Controls.Count, "controls-2");
			p.PrepareCH ();
			Assert.AreEqual (1, p.Columns.Count, "columns-3");
			Assert.AreEqual (1, p.Controls.Count, "controls-3");
		}

		[Test]
		public void OneTemplateColumn3 ()
		{
			DataGridPoker p = new DataGridPoker ();
			p.ShowFooter = true;
			p.AutoGenerateColumns = false;
			p.DataSource = new ArrayList ();
			TemplateColumn tc = new TemplateColumn ();
			tc.FooterTemplate = new MyTemplate ("hola");
			p.Columns.Add (tc);
			p.DataBind ();

			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			Assert.AreEqual (1, p.Columns.Count, "columns");
			Assert.AreEqual (1, p.Controls.Count, "controls");

			string render = p.Render ();
			// no items, but we have a footer
			Assert.IsTrue (-1 != render.IndexOf ("hola"), "template");
		}

		// This one throw nullref on MS and works with mono
		/*
		[Test]
		[NUnit.Framework.CategoryAttribute ("NotDotNet")]
		public void OneTemplateColumn4 ()
		{
			DataGridPoker p = new DataGridPoker ();
			TemplateColumn tc = new TemplateColumn ();
			tc.ItemTemplate = new MyTemplate ("hola");
			p.Columns.Add (tc);
			p.DataSource = new ArrayList ();
			p.CreateControls (false);
			Assert.AreEqual (1, p.Columns.Count, "columns");
			// Table added because useDataSource == false...
			Assert.AreEqual (1, p.Controls.Count, "controls");
			string render = p.Render ();
			// ... but no template rendered.
			Assert.IsTrue (-1 == render.IndexOf ("hola"), "template");
		}
		*/

		[Test]
		public void CreateControls ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataTable table = new DataTable ();

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));
			table.Rows.Add (new object [] { "1", "2", "3" });
			
			p.DataSource = new DataView (table);

			p.CreateControls (true);
			Assert.AreEqual (p.Controls.Count, 1, "A1");

			ShowControlsRecursive (p.Controls [0], 1);
		}

		[Test]
		public void CreationEvents ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataTable table = new DataTable ();

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));
			
			p.DataSource = new DataView (table);

			p.ItemCreated += new DataGridItemEventHandler (ItemCreatedHandler);
			p.ItemDataBound += new DataGridItemEventHandler (ItemDataBoundHandler);

			// No items added yet
			ResetEvents ();
			p.CreateControls (true);
			Assert.IsTrue (item_created, "A1");
			Assert.IsTrue (item_data_bound, "A2");

			table.Rows.Add (new object [] { "1", "2", "3" });

			ResetEvents ();
			p.CreateControls (true);
			Assert.IsTrue (item_created, "A3");
			Assert.IsTrue (item_data_bound, "A4");

			// no databinding
			ResetEvents ();
			p.CreateControls (false);
			Assert.IsTrue (item_created, "A5");
			Assert.IsFalse (item_data_bound, "A6");
		}

		[Test]
		public void InitializePager ()
		{
			DataGridPoker p = new DataGridPoker ();
			PagedDataSource paged = new PagedDataSource ();
			DataTable table = new DataTable ();
			DataGridItem item = new DataGridItem (-1, -1, ListItemType.Pager);
			ArrayList columns;
			LinkButton next;
			LinkButton prev;

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));

			for (int i = 0; i < 25; i++)
				table.Rows.Add (new object [] { "1", "2", "3" });
			paged.DataSource = new DataView (table);

			columns = p.CreateColumns (paged, true);
			p.InitPager (item, columns.Count, paged);

			//
			// No where to go
			//

			Assert.AreEqual (item.Controls.Count, 1, "A1");
			Assert.AreEqual (item.Controls [0].GetType (), typeof (TableCell), "A2");
			Assert.AreEqual (item.Controls [0].Controls.Count, 3, "A3");
			Assert.AreEqual (item.Controls [0].Controls [0].GetType (), typeof (Label), "A4");
			Assert.AreEqual (item.Controls [0].Controls [1].GetType (),
					typeof (LiteralControl), "A5");
			Assert.AreEqual (item.Controls [0].Controls [2].GetType (), typeof (Label), "A6");
			Assert.AreEqual (((Label) item.Controls [0].Controls [0]).Text, "&lt;", "A7");
			Assert.AreEqual (((LiteralControl) item.Controls [0].Controls [1]).Text,
					"&nbsp;", "A7");
			Assert.AreEqual (((Label) item.Controls [0].Controls [2]).Text, "&gt;", "A8");

			//
			// Next
			//

			item = new DataGridItem (-1, -1, ListItemType.Pager);
			paged.PageSize = 5;
			paged.VirtualCount = 25;
			paged.AllowPaging = true;
			p.InitPager (item, columns.Count, paged);

			Assert.AreEqual (item.Controls.Count, 1, "A9");
			Assert.AreEqual (item.Controls [0].GetType (), typeof (TableCell), "A10");
			Assert.AreEqual (item.Controls [0].Controls.Count, 3, "A11");
			Assert.AreEqual (item.Controls [0].Controls [0].GetType (), typeof (Label), "A12");
			Assert.AreEqual (item.Controls [0].Controls [1].GetType (),
					typeof (LiteralControl), "A13");
			Assert.AreEqual (((Label) item.Controls [0].Controls [0]).Text, "&lt;", "A14");
			Assert.AreEqual (((LiteralControl) item.Controls [0].Controls [1]).Text,
					"&nbsp;", "A16");

			next = (LinkButton) item.Controls [0].Controls [2];
			Assert.AreEqual (next.Text, "&gt;", "A17");
			Assert.AreEqual (next.CommandName, "Page", "A18");
			Assert.AreEqual (next.CommandArgument, "Next", "A19");


			//
			// Both
			//

			item = new DataGridItem (-1, -1, ListItemType.Pager);
			paged.PageSize = 5;
			paged.VirtualCount = 25;
			paged.AllowPaging = true;
			paged.CurrentPageIndex = 2;
			p.InitPager (item, columns.Count, paged);

			Assert.AreEqual (item.Controls.Count, 1, "A20");
			Assert.AreEqual (item.Controls [0].GetType (), typeof (TableCell), "A21");
			Assert.AreEqual (item.Controls [0].Controls.Count, 3, "A22");
			Assert.AreEqual (item.Controls [0].Controls [1].GetType (),
					typeof (LiteralControl), "A23");
			Assert.AreEqual (((LiteralControl) item.Controls [0].Controls [1]).Text,
					"&nbsp;", "A24");

			// This is failing with an invalidcast right now. It's something related to
			// the pager thinking that it's on the last page and rendering a label instead
			next = (LinkButton) item.Controls [0].Controls [2];
			Assert.AreEqual (next.Text, "&gt;", "A25");
			Assert.AreEqual (next.CommandName, "Page", "A26");
			Assert.AreEqual (next.CommandArgument, "Next", "A27");

			prev = (LinkButton) item.Controls [0].Controls [0];
			Assert.AreEqual (prev.Text, "&lt;", "A28");
			Assert.AreEqual (prev.CommandName, "Page", "A29");
			Assert.AreEqual (prev.CommandArgument, "Prev", "A30");

			//
			// Back only
			//

			item = new DataGridItem (-1, -1, ListItemType.Pager);
			paged.PageSize = 5;
			paged.VirtualCount = 25;
			paged.AllowPaging = true;
			paged.CurrentPageIndex = 4;
			p.InitPager (item, columns.Count, paged);

			Assert.AreEqual (item.Controls.Count, 1, "A31");
			Assert.AreEqual (item.Controls [0].GetType (), typeof (TableCell), "A32");
			Assert.AreEqual (item.Controls [0].Controls.Count, 3, "A33");
			Assert.AreEqual (item.Controls [0].Controls [1].GetType (),
					typeof (LiteralControl), "A34");
			Assert.AreEqual (item.Controls [0].Controls [2].GetType (), typeof (Label), "A35");
			Assert.AreEqual (((LiteralControl) item.Controls [0].Controls [1]).Text,
					"&nbsp;", "A36");
			Assert.AreEqual (((Label) item.Controls [0].Controls [2]).Text, "&gt;", "A37");

			prev = (LinkButton) item.Controls [0].Controls [0];
			Assert.AreEqual (prev.Text, "&lt;", "A38");
			Assert.AreEqual (prev.CommandName, "Page", "A39");
			Assert.AreEqual (prev.CommandArgument, "Prev", "A40");

		}

		[Conditional ("VERBOSE_DATAGRID")]
		private void ShowControlsRecursive (Control c, int depth)
		{
			for (int i = 0; i < depth; i++)
				Console.Write ("-");

			// StringWriter sw = new StringWriter ();
			// HtmlTextWriter tw = new HtmlTextWriter (sw);

			// c.RenderControl (tw);
			// Console.WriteLine (sw.ToString ());

			Console.WriteLine (c);

			foreach (Control child in c.Controls)
				ShowControlsRecursive (child, depth + 5);
		}

		[Test]
		public void Render ()
		{
			DataGridPoker p = new DataGridPoker ();

			Assert.AreEqual (p.Render (), String.Empty, "A1");
		}

		Control FindByType (Control parent, Type type)
		{
			if (!parent.HasControls ())
				return null;

			foreach (Control c in parent.Controls) {
				if (type.IsAssignableFrom (c.GetType ()))
					return c;

				Control ret = FindByType (c, type);
				if (ret != null)
					return ret;
			}
			return null;
		}

		// Header link
		[Test]
		public void SpecialLinkButton1 ()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add (new DataColumn("something", typeof(Int32)));
			DataRow dr = dt.NewRow ();
			dt.Rows.Add (new object [] { 1 });
			DataView dv = new DataView (dt);
			DataGridPoker dg = new DataGridPoker ();
			dg.AllowSorting = true;
			dg.HeaderStyle.Font.Bold = true;
			dg.HeaderStyle.ForeColor = Color.FromArgb (255,255,255,255);
			dg.HeaderStyle.BackColor = Color.FromArgb (33,33,33,33);
			dg.DataSource = dv;
			dg.DataBind ();
			LinkButton lb = (LinkButton) FindByType (dg.Controls [0], typeof (LinkButton));
			Assert.IsNotNull (lb, "lb");
			StringWriter sr = new StringWriter ();
			HtmlTextWriter output = new HtmlTextWriter (sr);
			// Nothing here...
			Assert.AreEqual (Color.Empty, lb.ControlStyle.ForeColor, "fore");
			lb.RenderControl (output);
			// Nothing here...
			Assert.AreEqual (Color.Empty, lb.ControlStyle.ForeColor, "fore2");
			dg.Render ();
			// Surprise! after rendering the datagrid, the linkbutton has the ForeColor from the datagrid
			Assert.AreEqual (Color.FromArgb (255,255,255,255), lb.ControlStyle.ForeColor, "fore3");

			// Extra. Items != empty
			Assert.AreEqual (1, dg.Items.Count, "itemCount");
		}

		// value link in buttoncolumn
		[Test]
		public void SpecialLinkButton2 ()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add (new DataColumn("string_col", typeof(string)));
			DataRow dr = dt.NewRow ();
			dt.Rows.Add (new object [] { "Item 1" });
			DataView dv = new DataView (dt);

			DataGridPoker dg = new DataGridPoker ();
			dg.DataSource = dv;
			dg.AutoGenerateColumns = false;
			dg.HeaderStyle.ForeColor = Color.FromArgb (255,255,255,255);
			dg.HeaderStyle.BackColor = Color.FromArgb (33,33,33,33);

			ButtonColumn bc = new ButtonColumn ();
			bc.HeaderText = "Some header";
			bc.DataTextField = "string_col";
			bc.CommandName = "lalala";
			dg.Columns.Add (bc);

			BoundColumn bound = new BoundColumn ();
			bound.HeaderText = "The other column";
			bound.DataField = "string_col";
			dg.Columns.Add (bound);

			dg.DataBind ();

			LinkButton lb = (LinkButton) FindByType (dg.Controls [0], typeof (LinkButton));
			Assert.IsNotNull (lb, "lb");
			StringWriter sr = new StringWriter ();
			HtmlTextWriter output = new HtmlTextWriter (sr);
			Assert.AreEqual (Color.Empty, lb.ControlStyle.ForeColor, "fore");
			lb.RenderControl (output);
			Assert.AreEqual (Color.Empty, lb.ControlStyle.ForeColor, "fore2");
			string str = dg.Render ();
			Assert.IsTrue (-1 != str.IndexOf ("<a>Item 1</a>"), "item1");
			Assert.IsTrue (-1 != str.IndexOf ("<td>Item 1</td>"), "item1-2");
		}
	}
}

