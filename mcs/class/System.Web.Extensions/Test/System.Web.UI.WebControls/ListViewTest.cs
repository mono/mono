//
// System.Web.UI.WebControls.ListView
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2008-2010 Novell, Inc
//

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
#if NET_3_5
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.UI.WebControls
{
	public sealed class ListViewPoker : ListView
	{
		EventRecorder recorder;

		public StateBag StateBag {
			get { return base.ViewState; }
		}
		
		void RecordEvent (string suffix)
		{
			if (recorder == null)
				return;

			recorder.Record (suffix);
		}

		public ListViewPoker ()
			: base ()
		{
		}
		
		public ListViewPoker (EventRecorder recorder)
		{
			this.recorder = recorder;
		}

		internal void SetEventRecorder (EventRecorder recorder)
		{
			this.recorder = recorder;
		}

		public override void ExtractItemValues (IOrderedDictionary itemValues, ListViewItem item, bool includePrimaryKey)
		{
			RecordEvent ("Enter");
			base.ExtractItemValues (itemValues, item, includePrimaryKey);
			RecordEvent ("Leave");
		}
		
		protected override void OnItemCanceling (ListViewCancelEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemCanceling (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemCommand (ListViewCommandEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemCommand (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemCreated (ListViewItemEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemCreated (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemDataBound (ListViewItemEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemDataBound (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemDeleted (ListViewDeletedEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemDeleted (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemDeleting (ListViewDeleteEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemDeleting (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemEditing (ListViewEditEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemEditing (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemInserted (ListViewInsertedEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemInserted (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemInserting (ListViewInsertEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemInserting (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemUpdated (ListViewUpdatedEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemUpdated (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemUpdating (ListViewUpdateEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemUpdating (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnLayoutCreated (EventArgs e)
		{
			RecordEvent ("Enter");
			base.OnLayoutCreated (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnPagePropertiesChanged (EventArgs e)
		{
			RecordEvent ("Enter");
			base.OnPagePropertiesChanged (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnPagePropertiesChanging (PagePropertiesChangingEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnPagePropertiesChanging (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			RecordEvent ("Enter");
			base.OnSelectedIndexChanged (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnSelectedIndexChanging (ListViewSelectEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnSelectedIndexChanging (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnSorted (EventArgs e)
		{
			RecordEvent ("Enter");
			base.OnSorted (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnSorting (ListViewSortEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnSorting (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnTotalRowCountAvailable (PageEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnTotalRowCountAvailable (e);
			RecordEvent ("Leave");
		}

		public void DoSetPageProperties (int startRowIndex, int maximumRows, bool databind)
		{
			SetPageProperties (startRowIndex, maximumRows, databind);
		}

		public bool GetRequiresDataBinding ()
		{
			return RequiresDataBinding;
		}
		
		public int GetMaximumRowsProperty ()
		{
			return MaximumRows;
		}

		public int GetStartRowIndexProperty ()
		{
			return StartRowIndex;
		}
		
		public void DoAddControlToContainer (Control control, Control container, int addLocation)
		{
			AddControlToContainer (control, container, addLocation);
		}

		public void DoCreateControlStyle ()
		{
			CreateControlStyle ();
		}

		public ListViewDataItem DoCreateDataItem (int dataItemIndex, int displayIndex)
		{
			return CreateDataItem (dataItemIndex, displayIndex);
		}

		public DataSourceSelectArguments DoCreateDataSourceSelectArguments ()
		{
			return CreateDataSourceSelectArguments ();
		}

		public void DoCreateEmptyDataItem ()
		{
			CreateEmptyDataItem ();
		}

		public ListViewItem DoCreateEmptyItem ()
		{
			return CreateEmptyItem ();
		}

		public ListViewItem DoCreateInsertItem ()
		{
			return CreateInsertItem ();
		}

		public ListViewItem DoCreateItem (ListViewItemType type)
		{
			return CreateItem (type);
		}

		public void DoCreateLayoutTemplate ()
		{
			CreateLayoutTemplate ();
		}

		public void DoEnsureLayoutTemplate ()
		{
			EnsureLayoutTemplate ();
		}

		public Control DoFindPlaceholder (string containerID, Control container)
		{
			return FindPlaceholder (containerID, container);
		}

		public void DoInstantiateEmptyDataTemplate (Control container)
		{
			InstantiateEmptyDataTemplate (container);
		}

		public void DoInstantiateEmptyItemTemplate (Control container)
		{
			InstantiateEmptyItemTemplate (container);
		}

		public void DoInstantiateGroupSeparatorTemplate (Control container)
		{
			InstantiateGroupSeparatorTemplate (container);
		}

		public void DoInstantiateGroupTemplate (Control container)
		{
			InstantiateGroupTemplate (container);
		}

		public void DoInstantiateInsertItemTemplate (Control container)
		{
			InstantiateInsertItemTemplate (container);
		}

		public void DoInstantiateItemSeparatorTemplate (Control container)
		{
			InstantiateItemSeparatorTemplate (container);
		}

		public void DoInstantiateItemTemplate (Control container, int displayIndex)
		{
			InstantiateItemTemplate (container, displayIndex);
		}
	}

	class TestTemplate : ITemplate
	{
		public void InstantiateIn (Control container)
		{
			Control control = new Control ();
			control.ID = "TestTemplateControl";
			
			container.Controls.Add (control);
		}
	}

	class DeepTestTemplate : ITemplate
	{
		public void InstantiateIn (Control container)
		{
			Control top = new Control (), control;
			top.Controls.Add (new Control ());
			control = new Control ();
			control.ID = "DeepTestTemplateControl";
			top.Controls [0].Controls.Add (control);

			container.Controls.Add (top);
		}
	}
	
	[TestFixture]
	public class ListViewTest
	{
		enum ListViewItemTemplate
		{
			EmptyData,
			EmptyItem,
			GroupSeparator,
			Group,
			InsertItem,
			ItemSeparator,
			Item,
			EditItem,
			AlternatingItem,
			SelectedItem
		}
		
		[TestFixtureSetUp]
                public void ListView_Init ()
                {
#if VISUAL_STUDIO
                        WebTest.CopyResource (GetType (), "MonoTests.System.Web.Extensions.resources.ListViewTest.aspx", "ListViewTest.aspx");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.Extensions.resources.ListViewTotalRowCount_Bug535701_1.aspx", "ListViewTotalRowCount_Bug535701_1.aspx");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.Extensions.resources.ListViewTotalRowCount_Bug535701_2.aspx", "ListViewTotalRowCount_Bug535701_2.aspx");
#else
                        WebTest.CopyResource (GetType (), "ListViewTest.aspx", "ListViewTest.aspx");
			WebTest.CopyResource (GetType (), "ListViewTotalRowCount_Bug535701_1.aspx", "ListViewTotalRowCount_Bug535701_1.aspx");
			WebTest.CopyResource (GetType (), "ListViewTotalRowCount_Bug535701_2.aspx", "ListViewTotalRowCount_Bug535701_2.aspx");
#endif
                }
		
		[Test]
		public void ListView_InitialValues ()
		{
			ListViewPoker lvp = new ListViewPoker (null);

			Assert.AreEqual (0, lvp.StateBag.Count, "ViewState.Count");
			Assert.AreEqual (true, lvp.ConvertEmptyStringToNull, "ConvertEmptyStringToNull");
			Assert.AreEqual (0, lvp.DataKeyNames.Length, "DataKeyNames.Length");
			Assert.AreEqual (-1, lvp.EditIndex, "EditIndex");
			Assert.AreEqual (null, lvp.EditItem, "EditItem");
			Assert.AreEqual (null, lvp.EditItemTemplate, "EditItemTemplate");
			Assert.AreEqual (null, lvp.EmptyDataTemplate, "EmptyDataTemplate");
			Assert.AreEqual (null, lvp.EmptyItemTemplate, "EmptyItemTemplate");
			Assert.AreEqual (false, lvp.EnableModelValidation, "EnableModelValidation");
			Assert.AreEqual (1, lvp.GroupItemCount, "GroupItemCount");
			Assert.AreEqual ("groupPlaceholder", lvp.GroupPlaceholderID, "GroupPlaceholderID");
			Assert.AreEqual (null, lvp.GroupSeparatorTemplate, "GroupSeparatorTemplate");
			Assert.AreEqual (null, lvp.GroupTemplate, "GroupTemplate");
			Assert.AreEqual (null, lvp.InsertItem, "InsertItem");
			Assert.AreEqual (InsertItemPosition.None, lvp.InsertItemPosition, "InsertItemPosition");
			Assert.AreEqual (null, lvp.InsertItemTemplate, "InsertItemTemplate");
			Assert.AreEqual ("itemPlaceholder", lvp.ItemPlaceholderID, "ItemPlaceholderID");
			Assert.AreEqual (0, lvp.Items.Count, "Items.Length");
			Assert.AreEqual (null, lvp.ItemSeparatorTemplate, "ItemSeparatorTemplate");
			Assert.AreEqual (null, lvp.ItemTemplate, "ItemTemplate");
			Assert.AreEqual (null, lvp.LayoutTemplate, "LayoutTemplate");
			Assert.AreEqual (-1, lvp.GetMaximumRowsProperty (), "MaximumRows");
			Assert.AreEqual (null, lvp.SelectedPersistedDataKey, "SelectedPersistedDataKey");
			Assert.AreEqual (-1, lvp.SelectedIndex, "SelectedIndex");
			Assert.AreEqual (null, lvp.SelectedItemTemplate, "SelectedItemTemplate");
			Assert.AreEqual (SortDirection.Ascending, lvp.SortDirection, "SortDirection");
			Assert.AreEqual (String.Empty, lvp.SortExpression, "SortExpression");
			Assert.AreEqual (0, lvp.GetStartRowIndexProperty (), "StartRowIndex");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ListView_InitialValues_SelectedValue ()
		{
			var lvp = new ListViewPoker (null);
			Assert.AreEqual (null, lvp.SelectedValue, "SelectedValue");
		}
		
		[Test]
		public void ListView_SetPageProperties_Events ()
		{
			var events = new EventRecorder ();
			var lvp = new ListViewPoker (events);

			// No events expected: databind is false
			events.Clear ();
			lvp.DoSetPageProperties (0, 1, false);

			// No events expected: startRowIndex and maximumRows don't change values
			events.Clear ();
			lvp.DoSetPageProperties (0, 1, true);
			Assert.AreEqual (0, events.Count, "#A1");
			
			// No events expected: startRowIndex changes, but databind is false
			events.Clear();
			lvp.DoSetPageProperties(1, 1, false);
			Assert.AreEqual (0, events.Count, "#A2");
			
			// No events expected: maximumRows changes, but databind is false
			events.Clear();
			lvp.DoSetPageProperties(1, 2, false);
			Assert.AreEqual (0, events.Count, "#A3");
			
			// No events expected: maximumRows and startRowIndex change but databind is
			// false
			events.Clear();
			lvp.DoSetPageProperties(3, 4, false);
			Assert.AreEqual (0, events.Count, "#A4");
			
			// Events expected: maximumRows and startRowIndex change and databind is
			// true
			events.Clear();
			lvp.DoSetPageProperties(5, 6, true);
			Assert.AreEqual (4, events.Count, "#A5");
			Assert.AreEqual ("OnPagePropertiesChanging:Enter", events [0], "#A6");
			Assert.AreEqual ("OnPagePropertiesChanging:Leave", events [1], "#A7");
			Assert.AreEqual ("OnPagePropertiesChanged:Enter", events [2], "#A8");
			Assert.AreEqual ("OnPagePropertiesChanged:Leave", events [3], "#A9");

			// Events expected: maximumRows changes and databind is true
			events.Clear();
			lvp.DoSetPageProperties(5, 7, true);
			Assert.AreEqual (4, events.Count, "#A10");
			Assert.AreEqual ("OnPagePropertiesChanging:Enter", events [0], "#A11");
			Assert.AreEqual ("OnPagePropertiesChanging:Leave", events [1], "#A12");
			Assert.AreEqual ("OnPagePropertiesChanged:Enter", events [2], "#A13");
			Assert.AreEqual ("OnPagePropertiesChanged:Leave", events [3], "#A14");

			// Events expected: startRowIndex changes and databind is true
			events.Clear();
			lvp.DoSetPageProperties(6, 7, true);
			Assert.AreEqual (4, events.Count, "#A15");
			Assert.AreEqual ("OnPagePropertiesChanging:Enter", events [0], "#A16");
			Assert.AreEqual ("OnPagePropertiesChanging:Leave", events [1], "#A17");
			Assert.AreEqual ("OnPagePropertiesChanged:Enter", events [2], "#A18");
			Assert.AreEqual ("OnPagePropertiesChanged:Leave", events [3], "#A19");			
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ListView_AddControlToContainer_NullControl ()
		{
			var lvp = new ListViewPoker ();
			Control container = new Control ();
			Control control = new Control ();
			control.ID = "TestControl";
			
			lvp.DoAddControlToContainer (null, container, 0);
			Assert.AreEqual (0, container.Controls.Count, "#A1");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ListView_AddControlToContainer_NullContainer ()
		{
			var lvp = new ListViewPoker ();
			Control container = new Control ();
			Control control = new Control ();
			control.ID = "TestControl";
			
			lvp.DoAddControlToContainer (control, null, 0);
			Assert.AreEqual (0, container.Controls.Count, "#A2");
		}
		
		[Test]
		public void ListView_AddControlToContainer ()
		{
			var lvp = new ListViewPoker ();
			Control container = new Control ();
			Control control = new Control ();
			control.ID = "TestControl";

			lvp.DoAddControlToContainer (control, container, 0);
			Assert.AreEqual (typeof (Control), container.Controls [0].GetType (), "#A1");
			Assert.AreEqual ("TestControl", container.Controls [0].ID, "#A2");

			container = new HtmlTable ();
			lvp.DoAddControlToContainer (control, container, 0);
			Assert.AreEqual ("System.Web.UI.WebControls.ListViewTableRow", container.Controls [0].GetType ().ToString (), "#B1");
			Assert.AreEqual ("TestControl", container.Controls [0].Controls [0].ID, "#B2");

			container = new HtmlTableRow ();
			lvp.DoAddControlToContainer (control, container, 0);
			Assert.AreEqual ("System.Web.UI.WebControls.ListViewTableCell", container.Controls [0].GetType ().ToString (), "#C1");
			Assert.AreEqual ("TestControl", container.Controls [0].Controls [0].ID, "#C2");

			container = new Control ();
			lvp.DoAddControlToContainer (control, container, -1);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ListView_CreateControlStyle ()
		{
			var lvp = new ListViewPoker ();
			lvp.DoCreateControlStyle ();
		}

		[Test]
		public void ListView_CreateDataItem ()
		{
			var lvp = new ListViewPoker ();
			ListViewDataItem lvdi = lvp.DoCreateDataItem (0, 0);

			Assert.IsTrue (lvdi != null, "#A1");
			Assert.AreEqual (null, lvdi.DataItem, "#A2");
			Assert.AreEqual (0, lvdi.DataItemIndex, "#A3");
			Assert.AreEqual (0, lvdi.DisplayIndex, "#A4");
			Assert.AreEqual (ListViewItemType.DataItem, lvdi.ItemType, "#A5");

			lvdi = lvp.DoCreateDataItem (-1, -1);
			Assert.AreEqual (-1, lvdi.DataItemIndex, "#A6");
			Assert.AreEqual (-1, lvdi.DisplayIndex, "#A7");
		}

		[Test]
		public void ListView_CreateDataSourceSelectArguments ()
		{
			var lvp = new ListViewPoker ();
			DataSourceSelectArguments args = lvp.DoCreateDataSourceSelectArguments ();

			Assert.IsTrue (args != null, "#A1");
		}

		[Test]
		public void ListView_CreateEmptyDataItem ()
		{
			var events = new EventRecorder ();
			var lvp = new ListViewPoker (events);
			lvp.DoCreateEmptyDataItem ();

			Assert.AreEqual (0, events.Count, "#A1");
			
			lvp.EmptyDataTemplate = new TestTemplate ();
			lvp.DoCreateEmptyDataItem ();
			Assert.AreEqual (1, lvp.Controls.Count, "#B1");
			Assert.AreEqual (typeof (ListViewItem), lvp.Controls [0].GetType (), "#B2");
			Assert.AreEqual ("TestTemplateControl", lvp.Controls [0].Controls [0].ID, "#B3");
			Assert.AreEqual ("OnItemCreated:Enter", events [0], "#B4");
			Assert.AreEqual ("OnItemCreated:Leave", events [1], "#B5");
		}

		[Test]
		public void ListView_CreateEmptyItem ()
		{
			var events = new EventRecorder ();
			var lvp = new ListViewPoker (events);
			ListViewItem item = lvp.DoCreateEmptyItem ();

			Assert.AreEqual (0, events.Count, "#A1");
			Assert.AreEqual (null, item, "#A2");
			
			lvp.EmptyItemTemplate = new TestTemplate ();
			item = lvp.DoCreateEmptyItem ();
			Assert.AreEqual (0, lvp.Controls.Count, "#B1");
			Assert.AreEqual (typeof (Control), item.Controls [0].GetType (), "#B2");
			Assert.AreEqual ("TestTemplateControl", item.Controls [0].ID, "#B3");
			Assert.AreEqual ("OnItemCreated:Enter", events [0], "#B4");
			Assert.AreEqual ("OnItemCreated:Leave", events [1], "#B5");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ListView_CreateInsertItem_NoInsertItemTemplate ()
		{
			var lvp = new ListViewPoker ();
			ListViewItem item = lvp.DoCreateInsertItem ();
		}
		
		[Test]
		public void ListView_CreateInsertItem ()
		{
			var events = new EventRecorder ();
			var lvp = new ListViewPoker (events);
			
			lvp.InsertItemTemplate = new TestTemplate ();
			ListViewItem item = lvp.DoCreateInsertItem ();
			Assert.AreEqual (0, lvp.Controls.Count, "#A1");
			Assert.AreEqual (typeof (ListViewItem), item.GetType (), "#A2");
			Assert.AreEqual (typeof (Control), item.Controls [0].GetType (), "#A3");

			Assert.AreEqual (2, events.Count, "#A4");
			Assert.AreEqual ("TestTemplateControl", item.Controls [0].ID, "#A5");
			Assert.AreEqual ("OnItemCreated:Enter", events [0], "#A6");
			Assert.AreEqual ("OnItemCreated:Leave", events [1], "#A7");
			
			Assert.AreEqual (ListViewItemType.InsertItem, item.ItemType, "#A7");
			Assert.IsTrue (item.Equals (lvp.InsertItem), "#A8");
		}

		[Test]
		public void ListView_CreateItem ()
		{
			var events = new EventRecorder ();
			var lvp = new ListViewPoker (events);
			ListViewItem item;

			item = lvp.DoCreateItem (ListViewItemType.DataItem);
			Assert.IsFalse (item == null, "#A1");
			Assert.AreEqual (ListViewItemType.DataItem, item.ItemType, "#A2");
			Assert.AreEqual (typeof (ListViewItem), item.GetType (), "#A3");

			Assert.AreEqual (0, events.Count, "#B1");
			
			item = lvp.DoCreateItem (ListViewItemType.InsertItem);
			Assert.IsFalse (item == null, "#C1");
			Assert.AreEqual (ListViewItemType.InsertItem, item.ItemType, "#C2");
			Assert.AreEqual (typeof (ListViewItem), item.GetType (), "#C3");

			item = lvp.DoCreateItem (ListViewItemType.EmptyItem);
			Assert.IsFalse (item == null, "#D1");
			Assert.AreEqual (ListViewItemType.EmptyItem, item.ItemType, "#D2");
			Assert.AreEqual (typeof (ListViewItem), item.GetType (), "#D3");
		}

		[Test]
		public void ListView_CreateLayoutTemplate ()
		{
			var events = new EventRecorder ();
			var lvp = new ListViewPoker (events);

			lvp.DoCreateLayoutTemplate ();
			Assert.AreEqual (2, events.Count, "#A1");
			Assert.AreEqual ("OnLayoutCreated:Enter", events [0], "#A2");
			Assert.AreEqual ("OnLayoutCreated:Leave", events [1], "#A3");
			Assert.AreEqual (0, lvp.Controls.Count, "#A4");
			
			events.Clear ();
			lvp.LayoutTemplate = new TestTemplate ();
			lvp.DoCreateLayoutTemplate ();
			Assert.AreEqual (2, events.Count, "#B1");
			Assert.AreEqual ("OnLayoutCreated:Enter", events [0], "#B2");
			Assert.AreEqual ("OnLayoutCreated:Leave", events [1], "#B3");
			Assert.AreEqual (1, lvp.Controls.Count, "#B4");
			Assert.AreEqual (typeof (Control), lvp.Controls [0].GetType (), "#B5");
			Assert.AreEqual ("TestTemplateControl", lvp.Controls [0].Controls [0].ID, "#B6");
		}

		[Test]
		public void ListView_EnsureLayoutTemplate ()
		{
			var events = new EventRecorder ();
			var lvp = new ListViewPoker (events);

			lvp.DoEnsureLayoutTemplate ();
			Assert.AreEqual (2, events.Count, "#A1");
			Assert.AreEqual ("OnLayoutCreated:Enter", events [0], "#A2");
			Assert.AreEqual ("OnLayoutCreated:Leave", events [1], "#A3");
			Assert.AreEqual (0, lvp.Controls.Count, "#A4");
			
			events.Clear ();
			lvp.LayoutTemplate = new TestTemplate ();
			lvp.DoEnsureLayoutTemplate ();
			Assert.AreEqual (2, events.Count, "#B1");
			Assert.AreEqual ("OnLayoutCreated:Enter", events [0], "#B2");
			Assert.AreEqual ("OnLayoutCreated:Leave", events [1], "#B3");
			Assert.AreEqual (1, lvp.Controls.Count, "#B4");
			Assert.AreEqual (typeof (Control), lvp.Controls [0].GetType (), "#B5");
			Assert.AreEqual ("TestTemplateControl", lvp.Controls [0].Controls [0].ID, "#B6");

			events.Clear ();
			lvp.DoEnsureLayoutTemplate ();
			Assert.AreEqual (0, events.Count, "#C1");
			Assert.AreEqual (1, lvp.Controls.Count, "#C2");
			Assert.AreEqual (typeof (Control), lvp.Controls [0].GetType (), "#C3");
			Assert.AreEqual ("TestTemplateControl", lvp.Controls [0].Controls [0].ID, "#C4");
		}

		[Test]
		public void ListView_FindPlaceholder ()
		{
			var lvp = new ListViewPoker ();
			Control control;

			control = lvp.DoFindPlaceholder ("somePlaceholder", lvp);
			Assert.IsTrue (control == null, "#A1");

			control = lvp.DoFindPlaceholder (null, lvp);
			Assert.IsTrue (control == null, "#A2");
			
			control = lvp.DoFindPlaceholder (String.Empty, lvp);
			Assert.IsTrue (control == null, "#A3");

			lvp.LayoutTemplate = new TestTemplate ();
			lvp.DoEnsureLayoutTemplate ();
			control = lvp.DoFindPlaceholder ("TestTemplateControl", lvp);
			Assert.IsFalse (control == null, "#B1");
			Assert.AreEqual ("TestTemplateControl", control.ID, "#B2");
			control = lvp.DoFindPlaceholder ("DoesNotExist", lvp);
			Assert.IsTrue (control == null, "#B3");
			
			lvp = new ListViewPoker ();
			lvp.LayoutTemplate = new DeepTestTemplate ();
			lvp.DoEnsureLayoutTemplate ();
			control = lvp.DoFindPlaceholder ("DeepTestTemplateControl", lvp);
			Assert.IsFalse (control == null, "#C1");
			Assert.AreEqual ("DeepTestTemplateControl", control.ID, "#C2");
			control = lvp.DoFindPlaceholder ("DoesNotExist", lvp);
			Assert.IsTrue (control == null, "#C3");
		}

		void DoInstantiateCall (ListViewItemTemplate whichTemplate)
		{
			var events = new EventRecorder ();
			var lvp = new ListViewPoker (events);
			var container = new Control ();
			var template = new TestTemplate ();

			switch (whichTemplate) {
				case ListViewItemTemplate.EmptyData:
					lvp.DoInstantiateEmptyDataTemplate (null);
					lvp.EmptyDataTemplate = template;
					lvp.DoInstantiateEmptyDataTemplate (container);
					break;

				case ListViewItemTemplate.EmptyItem:
					lvp.DoInstantiateEmptyItemTemplate (null);
					lvp.EmptyItemTemplate = template;
					lvp.DoInstantiateEmptyItemTemplate (container);
					break;

				case ListViewItemTemplate.GroupSeparator:
					lvp.DoInstantiateGroupSeparatorTemplate (null);
					lvp.GroupSeparatorTemplate = template;
					lvp.DoInstantiateGroupSeparatorTemplate (container);
					break;

				case ListViewItemTemplate.Group:
					lvp.DoInstantiateGroupTemplate (null);
					lvp.GroupTemplate = template;
					lvp.DoInstantiateGroupTemplate (container);
					break;

				case ListViewItemTemplate.InsertItem:
					lvp.DoInstantiateInsertItemTemplate (null);
					lvp.InsertItemTemplate = template;
					lvp.DoInstantiateInsertItemTemplate (container);
					break;

				case ListViewItemTemplate.ItemSeparator:
					lvp.DoInstantiateItemSeparatorTemplate (null);
					lvp.ItemSeparatorTemplate = template;
					lvp.DoInstantiateItemSeparatorTemplate (container);
					break;

				case ListViewItemTemplate.Item:
					lvp.ItemTemplate = template;
					lvp.DoInstantiateItemTemplate (container, 0);
					break;
					
				case ListViewItemTemplate.EditItem:
					lvp.EditIndex = 0;
					lvp.ItemTemplate = template;
					lvp.EditItemTemplate = template;
					lvp.DoInstantiateItemTemplate (container, 0);
					break;
					
				case ListViewItemTemplate.AlternatingItem:
					lvp.ItemTemplate = template;
					lvp.AlternatingItemTemplate = template;
					lvp.DoInstantiateItemTemplate (container, 1);
					break;
					
				case ListViewItemTemplate.SelectedItem:
					lvp.ItemTemplate = template;
					lvp.SelectedIndex = 0;
					lvp.SelectedItemTemplate = template;
					lvp.DoInstantiateItemTemplate (container, 0);
					break;

				default:
					throw new NotSupportedException ("Unsupported ListView item type.");
			}
			
			Assert.AreEqual (0, events.Count, "#A1");
			Assert.AreEqual (typeof (Control), container.Controls [0].GetType (), "#A2");
			Assert.AreEqual ("TestTemplateControl", container.Controls [0].ID, "#A3");
		}

		void DoInstantiateContainerNullCall (ListViewItemTemplate whichTemplate)
		{
			var lvp = new ListViewPoker ();
			var template = new TestTemplate ();

			switch (whichTemplate) {
				case ListViewItemTemplate.EmptyData:
					lvp.EmptyDataTemplate = template;
					lvp.DoInstantiateEmptyDataTemplate (null);
					break;

				case ListViewItemTemplate.EmptyItem:
					lvp.EmptyItemTemplate = template;
					lvp.DoInstantiateEmptyItemTemplate (null);
					break;

				case ListViewItemTemplate.GroupSeparator:
					lvp.GroupSeparatorTemplate = template;
					lvp.DoInstantiateGroupSeparatorTemplate (null);
					break;

				case ListViewItemTemplate.Group:
					lvp.GroupTemplate = template;
					lvp.DoInstantiateGroupTemplate (null);
					break;

				case ListViewItemTemplate.InsertItem:
					lvp.InsertItemTemplate = template;
					lvp.DoInstantiateInsertItemTemplate (null);
					break;

				case ListViewItemTemplate.ItemSeparator:
					lvp.ItemSeparatorTemplate = template;
					lvp.DoInstantiateItemSeparatorTemplate (null);
					break;
					
				case ListViewItemTemplate.Item:
					lvp.ItemTemplate = template;
					lvp.DoInstantiateItemTemplate (null, 0);
					break;
					
				case ListViewItemTemplate.EditItem:
					lvp.EditItemTemplate = template;
					lvp.ItemTemplate = template;
					lvp.DoInstantiateItemTemplate (null, 0);
					break;
					
				case ListViewItemTemplate.AlternatingItem:
					lvp.AlternatingItemTemplate = template;
					lvp.ItemTemplate = template;
					lvp.DoInstantiateItemTemplate (null, 0);
					break;
					
				case ListViewItemTemplate.SelectedItem:
					lvp.SelectedItemTemplate = template;
					lvp.ItemTemplate = template;
					lvp.DoInstantiateItemTemplate (null, 0);
					break;

				default:
					throw new NotSupportedException ("Unsupported ListView item type.");
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ListView_InstantiateItemTemplate_NoItemTemplate ()
		{
			var lvp = new ListViewPoker ();
			lvp.DoInstantiateItemTemplate (new Control (), 0);
		}
		
		[Test]
		public void ListView_InstantiateEmptyDataTemplate ()
		{
			DoInstantiateCall (ListViewItemTemplate.EmptyData);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ListView_InstantiateEmptyDataTemplate_NullContainer ()
		{
			DoInstantiateContainerNullCall (ListViewItemTemplate.EmptyData);
		}
		
		[Test]
		public void ListView_InstantiateEmptyItemTemplate ()
		{
			DoInstantiateCall (ListViewItemTemplate.EmptyItem);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ListView_InstantiateEmptyItemTemplate_NullContainer ()
		{
			DoInstantiateContainerNullCall (ListViewItemTemplate.EmptyItem);
		}

		[Test]
		public void ListView_InstantiateGroupSeparatorTemplate ()
		{
			DoInstantiateCall (ListViewItemTemplate.GroupSeparator);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ListView_InstantiateGroupSeparatorTemplate_NullContainer ()
		{
			DoInstantiateContainerNullCall (ListViewItemTemplate.GroupSeparator);
		}

		[Test]
		public void ListView_InstantiateGroupTemplate ()
		{
			DoInstantiateCall (ListViewItemTemplate.Group);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ListView_InstantiateGroupTemplate_NullContainer ()
		{
			DoInstantiateContainerNullCall (ListViewItemTemplate.Group);
		}

		[Test]
		public void ListView_InstantiateInsertItemTemplate ()
		{
			DoInstantiateCall (ListViewItemTemplate.InsertItem);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ListView_InstantiateInsertItemTemplate_NullContainer ()
		{
			DoInstantiateContainerNullCall (ListViewItemTemplate.InsertItem);
		}

		[Test]
		public void ListView_InstantiateItemSeparatorTemplate ()
		{
			DoInstantiateCall (ListViewItemTemplate.ItemSeparator);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ListView_InstantiateItemSeparatorTemplate_NullContainer ()
		{
			DoInstantiateContainerNullCall (ListViewItemTemplate.ItemSeparator);
		}

		[Test]
		public void ListView_InstantiateItemTemplate ()
		{
			DoInstantiateCall (ListViewItemTemplate.Item);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ListView_InstantiateItemTemplate_NullContainer ()
		{
			DoInstantiateContainerNullCall (ListViewItemTemplate.Item);
		}

		[Test]
		public void ListView_InstantiateEditItemTemplate ()
		{
			DoInstantiateCall (ListViewItemTemplate.EditItem);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ListView_InstantiateEditItemTemplate_NullContainer ()
		{
			DoInstantiateContainerNullCall (ListViewItemTemplate.EditItem);
		}

		[Test]
		public void ListView_InstantiateAlternatingItemTemplate ()
		{
			DoInstantiateCall (ListViewItemTemplate.AlternatingItem);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ListView_InstantiateAlternatingItemTemplate_NullContainer ()
		{
			DoInstantiateContainerNullCall (ListViewItemTemplate.AlternatingItem);
		}

		[Test]
		public void ListView_InstantiateSelectedItemTemplate ()
		{
			DoInstantiateCall (ListViewItemTemplate.SelectedItem);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ListView_InstantiateSelectedItemTemplate_NullContainer ()
		{
			DoInstantiateContainerNullCall (ListViewItemTemplate.SelectedItem);
		}
		
		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ListView_FindPlaceholder_NullContainer ()
		{
			var lvp = new ListViewPoker ();
			Control control;

			control = lvp.DoFindPlaceholder ("somePlaceholder", null);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ListView_SetPageProperties_Parameters1 ()
		{
			var lvp = new ListViewPoker ();
			lvp.DoSetPageProperties (-1, 1, false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ListView_SetPageProperties_Parameters2 ()
		{
			var lvp = new ListViewPoker ();
			lvp.DoSetPageProperties (0, 0, false);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ListView_AccessKey ()
		{
			var lvp = new ListViewPoker ();
			lvp.AccessKey = String.Empty;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ListView_BackColor ()
		{
			var lvp = new ListViewPoker ();
			lvp.BackColor = Color.White;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ListView_BorderColor ()
		{
			var lvp = new ListViewPoker ();
			lvp.BorderColor = Color.White;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ListView_BorderStyle ()
		{
			var lvp = new ListViewPoker ();
			lvp.BorderStyle = BorderStyle.NotSet;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ListView_BorderWidth ()
		{
			var lvp = new ListViewPoker ();
			lvp.BorderWidth = Unit.Empty;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ListView_CssClass ()
		{
			var lvp = new ListViewPoker ();
			lvp.CssClass = String.Empty;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ListView_Font ()
		{
			var lvp = new ListViewPoker ();
			lvp.Font.Bold = true;
		}
		
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ListView_ForeColor ()
		{
			var lvp = new ListViewPoker ();
			lvp.ForeColor = Color.White;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ListView_Height ()
		{
			var lvp = new ListViewPoker ();
			lvp.Height = Unit.Empty;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ListView_ToolTip ()
		{
			var lvp = new ListViewPoker ();
			lvp.ToolTip = String.Empty;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ListView_Width ()
		{
			var lvp = new ListViewPoker ();
			lvp.Width = Unit.Empty;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ListView_EditIndex_SetInvalid ()
		{
			var lvp = new ListViewPoker ();
			lvp.EditIndex = -2;
		}

		[Test]
		public void ListView_EditIndex_Set ()
		{
			var lvp = new ListViewPoker ();
			lvp.EditIndex = 0;
			Assert.AreEqual (0, lvp.EditIndex, "#A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ListView_SelectedIndex_SetInvalid ()
		{
			var lvp = new ListViewPoker ();
			lvp.SelectedIndex = -2;
		}

		[Test]
		public void ListView_SelectedIndex_Set ()
		{
			var lvp = new ListViewPoker ();
			lvp.SelectedIndex = 0;
			Assert.AreEqual (0, lvp.SelectedIndex, "#A1");
		}
		
		[Test]
		public void ListView_Edit ()
		{
			WebTest t = new WebTest ("ListViewTest.aspx");
			t.Invoker = PageInvoker.CreateOnInit (ListView_Edit_OnInit);
			t.Run ();

			FormRequest fr = new FormRequest(t.Response, "form1");
#if DOT_NET
			fr.Controls.Add ("ListView1$ctrl0$ctl03$EditButton");
			fr.Controls.Add ("ListView1$ctrl6$ctrl7$CapitalTextBox");
			fr.Controls.Add ("ListView1$ctrl6$ctrl7$IDTextBox");
			fr.Controls.Add ("ListView1$ctrl6$ctrl7$NameTextBox");
			fr.Controls.Add ("ListView1$ctrl6$ctrl7$PopulationTextBox");
			fr.Controls ["ListView1$ctrl0$ctl03$EditButton"].Value = "Edit";
#else
			fr.Controls.Add ("ListView1$ctl13$EditButton");
			fr.Controls.Add ("ListView1$ctl51$CapitalTextBox");
			fr.Controls.Add ("ListView1$ctl51$IDTextBox");
			fr.Controls.Add ("ListView1$ctl51$NameTextBox");
			fr.Controls.Add ("ListView1$ctl51$PopulationTextBox");
			fr.Controls ["ListView1$ctl13$EditButton"].Value = "Edit";
#endif
			t.Request = fr;
			
			EventRecorder events = new EventRecorder();
			t.UserData = events;
			t.Run ();			
		}

		public static void ListView_Edit_OnInit (Page p)
		{
			ListViewPoker poker = p.FindControl ("ListView1") as ListViewPoker;
			poker.SetEventRecorder (WebTest.CurrentTest.UserData as EventRecorder);
		}

		[Test (Description="Bug #535701, test 1")]
		public void Bug_535701_1 ()
		{
			string originalHtml_1 = @"<span id=""ListViewTest"">
        0 1 2 3 4 5 6 7 8 9 
        </span>
        <span id=""DataPager1""><a disabled=""disabled"">First</a>&nbsp;<a disabled=""disabled"">Previous</a>&nbsp;<span>1</span>&nbsp;<a href=""javascript:__doPostBack('DataPager1$ctl01$ctl01','')"">2</a>&nbsp;<a href=""javascript:__doPostBack('DataPager1$ctl02$ctl00','')"">Next</a>&nbsp;<a href=""javascript:__doPostBack('DataPager1$ctl02$ctl01','')"">Last</a>&nbsp;</span>";
			string originalHtml_2 = @"<span id=""ListViewTest"">
        10 11 12 
        </span>
        <span id=""DataPager1""><a href=""javascript:__doPostBack('DataPager1$ctl00$ctl00','')"">First</a>&nbsp;<a href=""javascript:__doPostBack('DataPager1$ctl00$ctl01','')"">Previous</a>&nbsp;<a href=""javascript:__doPostBack('DataPager1$ctl01$ctl00','')"">1</a>&nbsp;<span>2</span>&nbsp;<a disabled=""disabled"">Next</a>&nbsp;<a disabled=""disabled"">Last</a>&nbsp;</span>";
			
			WebTest t = new WebTest ("ListViewTotalRowCount_Bug535701_1.aspx");
			string pageHtml = t.Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);
			
			Assert.AreEqual (originalHtml_1, renderedHtml, "#A1");

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls ["__EVENTTARGET"].Value = "DataPager1$ctl01$ctl01";
			t.Request = fr;

			pageHtml = t.Run ();
			renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);

			Assert.AreEqual (originalHtml_2, renderedHtml, "#A2");
		}

		[Test (Description="Bug #535701, test 2")]
		public void Bug_535701_2 ()
		{
			string originalHtml_1 = @"<span id=""ListViewTest2"">
        12345678910
        </span>
        <span id=""DataPager1""><a disabled=""disabled"">First</a>&nbsp;<a disabled=""disabled"">Previous</a>&nbsp;<span>1</span>&nbsp;<a href=""javascript:__doPostBack('DataPager1$ctl01$ctl01','')"">2</a>&nbsp;<a href=""javascript:__doPostBack('DataPager1$ctl02$ctl00','')"">Next</a>&nbsp;<a href=""javascript:__doPostBack('DataPager1$ctl02$ctl01','')"">Last</a>&nbsp;</span>
        	
        <br /><div>
        DataPager.TotalRowCount = 14<br />
        Actual TotalRowCount = 14</div>";
			string originalHtml_2 = @"<span id=""ListViewTest2"">
        11121314
        </span>
        <span id=""DataPager1""><a href=""javascript:__doPostBack('DataPager1$ctl00$ctl00','')"">First</a>&nbsp;<a href=""javascript:__doPostBack('DataPager1$ctl00$ctl01','')"">Previous</a>&nbsp;<a href=""javascript:__doPostBack('DataPager1$ctl01$ctl00','')"">1</a>&nbsp;<span>2</span>&nbsp;<a disabled=""disabled"">Next</a>&nbsp;<a disabled=""disabled"">Last</a>&nbsp;</span>
        	
        <br /><div>
        DataPager.TotalRowCount = 14<br />
        Actual TotalRowCount = 14</div>";
			
			WebTest t = new WebTest ("ListViewTotalRowCount_Bug535701_2.aspx");
			string pageHtml = t.Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);
			
			Assert.AreEqual (originalHtml_1, renderedHtml, "#A1");

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls ["__EVENTTARGET"].Value = "DataPager1$ctl01$ctl01";
			t.Request = fr;

			pageHtml = t.Run ();
			renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);

			Assert.AreEqual (originalHtml_2, renderedHtml, "#A2");
		}
	}
}
#endif
