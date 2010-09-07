//
// System.Web.UI.WebControls.DataList.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;
using System.Web.Util;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[Designer ("System.Web.UI.Design.WebControls.DataListDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ControlValueProperty ("SelectedValue")]
	[Editor ("System.Web.UI.Design.WebControls.DataListComponentEditor, " + Consts.AssemblySystem_Design, "System.ComponentModel.ComponentEditor, " + Consts.AssemblySystem)]
	public class DataList : BaseDataList, INamingContainer, IRepeatInfoUser
	{
		public const string CancelCommandName = "Cancel";
		public const string DeleteCommandName = "Delete";
		public const string EditCommandName = "Edit";
		public const string SelectCommandName = "Select";
		public const string UpdateCommandName = "Update";

		static readonly object cancelCommandEvent = new object ();
		static readonly object deleteCommandEvent = new object ();
		static readonly object editCommandEvent = new object ();
		static readonly object itemCommandEvent = new object ();
		static readonly object itemCreatedEvent = new object ();
		static readonly object itemDataBoundEvent = new object ();
		static readonly object updateCommandEvent = new object ();

		TableItemStyle alternatingItemStyle;
		TableItemStyle editItemStyle;
		TableItemStyle footerStyle;
		TableItemStyle headerStyle;
		TableItemStyle itemStyle;
		TableItemStyle selectedItemStyle;
		TableItemStyle separatorStyle;

		ITemplate alternatingItemTemplate;
		ITemplate editItemTemplate;
		ITemplate footerTemplate;
		ITemplate headerTemplate;
		ITemplate itemTemplate;
		ITemplate selectedItemTemplate;
		ITemplate separatorTemplate;

		DataListItemCollection items;
		ArrayList list;
		int idx;

		public DataList ()
		{
			idx = -1;
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle AlternatingItemStyle {
			get {
				if (alternatingItemStyle == null) {
					alternatingItemStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						alternatingItemStyle.TrackViewState ();
				}
				return alternatingItemStyle;
			}
		}

		[Browsable (false)]
		[DefaultValue (null)]
		[TemplateContainer (typeof (DataListItem))]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual ITemplate AlternatingItemTemplate {
			get { return alternatingItemTemplate; }
			set { alternatingItemTemplate = value; }
		}

		[DefaultValue (-1)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual int EditItemIndex {
			get {
				object o = ViewState ["EditItemIndex"];
				return (o == null) ? -1 : (int) o;
			}
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("EditItemIndex", "< -1");
				ViewState ["EditItemIndex"] = value;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle EditItemStyle {
			get {
				if (editItemStyle == null) {
					editItemStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						editItemStyle.TrackViewState ();
				}
				return editItemStyle;
			}
		}

		[Browsable (false)]
		[DefaultValue (null)]
		[TemplateContainer (typeof (DataListItem))]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual ITemplate EditItemTemplate {
			get { return editItemTemplate; }
			set { editItemTemplate = value; }
		}

		[DefaultValue (false)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual bool ExtractTemplateRows {
			get {
				object o = ViewState ["ExtractTemplateRows"];
				return (o == null) ? false : (bool) o;
			}
			set { ViewState ["ExtractTemplateRows"] = value; }
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle FooterStyle {
			get {
				if (footerStyle == null) {
					footerStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						footerStyle.TrackViewState ();
				}
				return footerStyle;
			}
		}

		[Browsable (false)]
		[DefaultValue (null)]
		[TemplateContainer (typeof (DataListItem))]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual ITemplate FooterTemplate {
			get { return footerTemplate; }
			set { footerTemplate = value; }
		}

		// yes! they do NOT match in fx1.1
		[DefaultValue (GridLines.None)]
		public override GridLines GridLines {
			get {
				if (!ControlStyleCreated)
					return GridLines.None;
				return TableStyle.GridLines;
			}
			set { TableStyle.GridLines = value; }
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle HeaderStyle {
			get {
				if (headerStyle == null) {
					headerStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						headerStyle.TrackViewState ();
				}
				return headerStyle;
			}
		}

		[Browsable (false)]
		[DefaultValue (null)]
		[TemplateContainer (typeof (DataListItem))]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual ITemplate HeaderTemplate {
			get { return headerTemplate; }
			set { headerTemplate = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual DataListItemCollection Items {
			get {
				if (items == null)
					items = new DataListItemCollection (ItemList);
				return items;
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle ItemStyle {
			get {
				if (itemStyle == null) {
					itemStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						itemStyle.TrackViewState ();
				}
				return itemStyle;
			}
		}

		[Browsable (false)]
		[DefaultValue (null)]
		[TemplateContainer (typeof (DataListItem))]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual ITemplate ItemTemplate {
			get { return itemTemplate; }
			set { itemTemplate = value; }
		}

		[DefaultValue (0)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual int RepeatColumns {
			get {
				object o = ViewState ["RepeatColumns"];
				return (o == null) ? 0 : (int) o;
			}
			set { 
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "RepeatColumns value has to be 0 for 'not set' or > 0.");
				
				ViewState ["RepeatColumns"] = value; 
			}
		}

		[DefaultValue (RepeatDirection.Vertical)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual RepeatDirection RepeatDirection {
			get {
				object o = ViewState ["RepeatDirection"];
				return (o == null) ? RepeatDirection.Vertical : (RepeatDirection) o;
			}
			set { ViewState ["RepeatDirection"] = value; }
		}

		[DefaultValue (RepeatLayout.Table)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual RepeatLayout RepeatLayout {
			get {
				object o = ViewState ["RepeatLayout"];
				return (o == null) ? RepeatLayout.Table : (RepeatLayout) o;
			}
			set {
#if NET_4_0
				if (value == RepeatLayout.OrderedList || value == RepeatLayout.UnorderedList)
					throw new ArgumentOutOfRangeException (String.Format ("DataList does not support the '{0}' layout.", value));
#endif
				ViewState ["RepeatLayout"] = value;
			}
		}

		[Bindable (true)]
		[DefaultValue (-1)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual int SelectedIndex {
			get {
				object o = ViewState ["SelectedIndex"];
				return (o == null) ? -1 : (int) o;
			}
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("SelectedIndex", "< -1");
				ViewState ["SelectedIndex"] = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual DataListItem SelectedItem {
			get {
				if (SelectedIndex < 0)
					return null;
				if (SelectedIndex >= Items.Count)
					throw new ArgumentOutOfRangeException ("SelectedItem", ">= Items.Count");
				return items [SelectedIndex];
			}
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle SelectedItemStyle {
			get {
				if (selectedItemStyle == null) {
					selectedItemStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						selectedItemStyle.TrackViewState ();
				}
				return selectedItemStyle;
			}
		}

		[Browsable (false)]
		[DefaultValue (null)]
		[TemplateContainer (typeof (DataListItem))]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual ITemplate SelectedItemTemplate {
			get { return selectedItemTemplate; }
			set { selectedItemTemplate = value; }
		}

		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle SeparatorStyle {
			get {
				if (separatorStyle == null) {
					separatorStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						separatorStyle.TrackViewState ();
				}
				return separatorStyle;
			}
		}

		[Browsable (false)]
		[DefaultValue (null)]
		[TemplateContainer (typeof (DataListItem))]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual ITemplate SeparatorTemplate {
			get { return separatorTemplate; }
			set { separatorTemplate = value; }
		}

		[DefaultValue (true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual bool ShowFooter {
			get {
				object o = ViewState ["ShowFooter"];
				return (o == null) ? true : (bool) o;
			}
			set { ViewState ["ShowFooter"] = value; }
		}

		[DefaultValue (true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual bool ShowHeader {
			get {
				object o = ViewState ["ShowHeader"];
				return (o == null) ? true : (bool) o;
			}
			set { ViewState ["ShowHeader"] = value; }
		}

		[MonoTODO ("incomplete")]
		[Browsable (false)]
		public object SelectedValue {
			get {
				if (DataKeyField.Length == 0)
					throw new InvalidOperationException (Locale.GetText ("No DataKeyField present."));

				int idx = SelectedIndex;
				if ((idx >= 0) && (idx < DataKeys.Count))
					return DataKeys [idx];

				return null;
			}
		}

		protected override HtmlTextWriterTag TagKey {
			get { return HtmlTextWriterTag.Table; }
		}

		TableStyle TableStyle {
			// this will throw an InvalidCasException just like we need
			get { return (TableStyle) ControlStyle; }
		}
		
		ArrayList ItemList {
			get {
				if (list == null)
					list = new ArrayList ();
				return list;
			}
		}

		void DoItem (int i, ListItemType t, object d, bool databind)
		{
			DataListItem itm = CreateItem (i, t);
			if (databind)
				itm.DataItem = d;
			DataListItemEventArgs e = new DataListItemEventArgs (itm);
			InitializeItem (itm);
			
			//
			// It is very important that this be called *before* data
			// binding. Otherwise, we won't save our state in the viewstate.
			//
			Controls.Add (itm);
			if (i != -1)
				ItemList.Add (itm);

			OnItemCreated (e);

			if (databind) {
				itm.DataBind ();
				OnItemDataBound (e);
				itm.DataItem = null;
			}
		}

		void DoItemInLoop (int i, object d, bool databind, ListItemType type)
		{
			DoItem (i, type, d, databind);
			if (SeparatorTemplate != null)
				DoItem (i, ListItemType.Separator, null, databind);

		}

		protected override void CreateControlHierarchy (bool useDataSource)
		{
			Controls.Clear();
			ItemList.Clear ();

			IEnumerable ds = null;
			ArrayList keys = null;

			if (useDataSource) {
				idx = 0;
				if (IsBoundUsingDataSourceID)
					ds = GetData();
				else
					ds = DataSourceResolver.ResolveDataSource (DataSource, DataMember);
				keys = DataKeysArray;
				keys.Clear ();
			} else
				idx = (int) ViewState ["Items"];

			if ((ds == null) && (idx == 0))
				return;

			if (headerTemplate != null)
				DoItem (-1, ListItemType.Header, null, useDataSource);

			// items
			int selected_index = SelectedIndex;
			int edit_item_index = EditItemIndex;
			ListItemType type;
			if (ds != null) {
				string key = DataKeyField;
				foreach (object o in ds) {
					if (useDataSource && !String.IsNullOrEmpty (key))
						keys.Add (DataBinder.GetPropertyValue (o, key));
					type = ListItemType.Item;
					if (idx == edit_item_index) 
						type = ListItemType.EditItem;
					else if (idx == selected_index) 
						type = ListItemType.SelectedItem;
					else if ((idx & 1) != 0) 
						type = ListItemType.AlternatingItem;

					DoItemInLoop (idx, o, useDataSource, type);
					idx++;
				}
			} else {
				for (int i = 0; i < idx; i++) {
					type = ListItemType.Item;
					if (i == edit_item_index) 
						type = ListItemType.EditItem;
					else if (i == selected_index) 
						type = ListItemType.SelectedItem;
					else if ((i & 1) != 0) 
						type = ListItemType.AlternatingItem;

					DoItemInLoop (i, null, useDataSource, type);
				}
			}

			if (footerTemplate != null)
				DoItem (-1, ListItemType.Footer, null, useDataSource);

			ViewState ["Items"] = idx;
		}

		protected override Style CreateControlStyle ()
		{
			// not kept (directly) in the DataList ViewState
			TableStyle tableStyle = new TableStyle ();
			tableStyle.CellSpacing = 0;
			return tableStyle;
		}

		protected virtual DataListItem CreateItem (int itemIndex, ListItemType itemType)
		{
			return new DataListItem (itemIndex, itemType);
		}

		protected virtual void InitializeItem (DataListItem item)
		{
			ITemplate t = null;
			
			switch (item.ItemType) {
				case ListItemType.Header:
					t = HeaderTemplate;
					break;
				case ListItemType.Footer:
					t = FooterTemplate;
					break;	
				case ListItemType.Separator:
					t = SeparatorTemplate;
					break;
				case ListItemType.Item:
				case ListItemType.AlternatingItem:
				case ListItemType.SelectedItem:
				case ListItemType.EditItem:
					if ((item.ItemType == ListItemType.EditItem) && (EditItemTemplate != null))
						t = EditItemTemplate;
					else if ((item.ItemType == ListItemType.SelectedItem) && (SelectedItemTemplate != null))
						t = SelectedItemTemplate;
					else if ((item.ItemType == ListItemType.AlternatingItem) && (AlternatingItemTemplate != null))
						t = AlternatingItemTemplate;
					else
						t = ItemTemplate;
					break;
			}

			if (t != null)
				t.InstantiateIn (item);
		}

		protected override void LoadViewState (object savedState)
		{
			object[] state = (object[]) savedState;
			base.LoadViewState (state [0]);
			if (state [1] != null)
				ItemStyle.LoadViewState (state [1]);
			if (state [2] != null)
				SelectedItemStyle.LoadViewState (state [2]);
			if (state [3] != null)
				AlternatingItemStyle.LoadViewState (state [3]);
			if (state [4] != null)
				EditItemStyle.LoadViewState (state [4]);
			if (state [5] != null)
				SeparatorStyle.LoadViewState (state [5]);
			if (state [6] != null)
				HeaderStyle.LoadViewState (state [6]);
			if (state [7] != null)
				FooterStyle.LoadViewState (state [7]);
			if (state [8] != null)
				ControlStyle.LoadViewState (state [8]);
		}

		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			DataListCommandEventArgs dlca = (e as DataListCommandEventArgs);
			if (dlca == null)
				return false;

			string cn = dlca.CommandName;
			CultureInfo inv = Helpers.InvariantCulture;

			OnItemCommand (dlca);
			if (String.Compare (cn, CancelCommandName, true, inv) == 0)
				OnCancelCommand (dlca);
			else if (String.Compare (cn, DeleteCommandName, true, inv) == 0)
				OnDeleteCommand (dlca);
			else if (String.Compare (cn, EditCommandName, true, inv) == 0)
				OnEditCommand (dlca);
			else if (String.Compare (cn, SelectCommandName, true, inv) == 0) {
				SelectedIndex = dlca.Item.ItemIndex;
				OnSelectedIndexChanged (dlca);
			} else if (String.Compare (cn, UpdateCommandName, true, inv) == 0)
				OnUpdateCommand (dlca);
			
			return true;
		}

		protected virtual void OnCancelCommand (DataListCommandEventArgs e)
		{
			DataListCommandEventHandler cancelCommand = (DataListCommandEventHandler) Events [cancelCommandEvent];
			if (cancelCommand != null)
				cancelCommand (this, e);
		}

		protected virtual void OnDeleteCommand (DataListCommandEventArgs e)
		{
			DataListCommandEventHandler deleteCommand = (DataListCommandEventHandler) Events [deleteCommandEvent];
			if (deleteCommand != null)
				deleteCommand (this, e);
		}

		protected virtual void OnEditCommand (DataListCommandEventArgs e)
		{
			DataListCommandEventHandler editCommand = (DataListCommandEventHandler) Events [editCommandEvent];
			if (editCommand != null)
				editCommand (this, e);
		}

		protected internal override void OnInit (EventArgs e)
		{
			// EditItemIndex and SelectedIndex now use the Control State (i.e not the
			// View State)
			Page page = Page;
			if (page != null)
				page.RegisterRequiresControlState (this);
			base.OnInit (e);
		}

		protected virtual void OnItemCommand (DataListCommandEventArgs e)
		{
			DataListCommandEventHandler itemCommand = (DataListCommandEventHandler) Events [itemCommandEvent];
			if (itemCommand != null)
				itemCommand (this, e);
		}

		protected virtual void OnItemCreated (DataListItemEventArgs e)
		{
			DataListItemEventHandler itemCreated = (DataListItemEventHandler) Events [itemCreatedEvent];
			if (itemCreated != null)
				itemCreated (this, e);
		}

		protected virtual void OnItemDataBound (DataListItemEventArgs e)
		{
			DataListItemEventHandler itemDataBound = (DataListItemEventHandler) Events [itemDataBoundEvent];
			if (itemDataBound != null)
				itemDataBound (this, e);
		}

		protected virtual void OnUpdateCommand (DataListCommandEventArgs e)
		{
			DataListCommandEventHandler updateCommand = (DataListCommandEventHandler) Events [updateCommandEvent];
			if (updateCommand != null)
				updateCommand (this, e);
		}

		protected override void PrepareControlHierarchy ()
		{
			if (!HasControls () || Controls.Count == 0)
				return; // No one called CreateControlHierarchy() with DataSource != null

			Style alt = null;
			foreach (DataListItem item in Controls) {
				switch (item.ItemType) {
					case ListItemType.Item:
						item.MergeStyle (itemStyle);
						break;
					case ListItemType.AlternatingItem:
						if (alt == null) {
							if (alternatingItemStyle != null) {
								alt = new TableItemStyle ();
								alt.CopyFrom (itemStyle);
								alt.CopyFrom (alternatingItemStyle);
							} else
								alt = itemStyle;
						}

						item.MergeStyle (alt);
						break;
					case ListItemType.EditItem:
						if (editItemStyle != null)
							item.MergeStyle (editItemStyle);
						else
							item.MergeStyle (itemStyle);
						break;
					case ListItemType.Footer:
						if (!ShowFooter) {
							item.Visible = false;
							break;
						}
						if (footerStyle != null)
							item.MergeStyle (footerStyle);
						break;
					case ListItemType.Header:
						if (!ShowHeader) {
							item.Visible = false;
							break;
						}
						if (headerStyle != null)
							item.MergeStyle (headerStyle);
						break;
					case ListItemType.SelectedItem:
						if (selectedItemStyle != null)
							item.MergeStyle (selectedItemStyle);
						else
							item.MergeStyle (itemStyle);
						break;
					case ListItemType.Separator:
						if (separatorStyle != null)
							item.MergeStyle(separatorStyle);
						else
							item.MergeStyle (itemStyle);
						break;
				}
			}
		}

		protected internal override void RenderContents (HtmlTextWriter writer)
		{
			if (Items.Count == 0)
				return;			

			RepeatInfo ri = new RepeatInfo ();
			ri.RepeatColumns = RepeatColumns;
			ri.RepeatDirection = RepeatDirection;
			ri.RepeatLayout = RepeatLayout;
			ri.CaptionAlign = CaptionAlign;
			ri.Caption = Caption;
			ri.UseAccessibleHeader = UseAccessibleHeader;
/*
// debugging stuff that I prefer to keep for a while
Console.WriteLine ("RepeatColumns {0}", ri.RepeatColumns);
Console.WriteLine ("RepeatDirection {0}", ri.RepeatDirection);
Console.WriteLine ("RepeatLayout {0}", ri.RepeatLayout);
Console.WriteLine ("OuterTableImplied {0}", ExtractTemplateRows);
Console.WriteLine ("IRepeatInfoUser.HasFooter {0}", (ShowFooter && (footerTemplate != null)));
Console.WriteLine ("IRepeatInfoUser.HasHeader {0}", (ShowHeader && (headerTemplate != null)));
Console.WriteLine ("IRepeatInfoUser.HasSeparators {0}", (separatorTemplate != null));
Console.WriteLine ("IRepeatInfoUser.RepeatedItemCount {0}", Items.Count);
for (int i=0; i < Items.Count; i++) {
	DataListItem dli = Items [i];
	Console.WriteLine ("{0}: Index {1}, Type {2}", i, dli.ItemIndex, dli.ItemType);
}
*/
			bool extract = ExtractTemplateRows;
			if (extract) {
				ri.OuterTableImplied = true;
				writer.AddAttribute (HtmlTextWriterAttribute.Id, ClientID);
				if (ControlStyleCreated)
					ControlStyle.AddAttributesToRender (writer);
				writer.RenderBeginTag (HtmlTextWriterTag.Table);
				ri.RenderRepeater (writer, this, ControlStyle, this);
				writer.RenderEndTag ();
			} else
				ri.RenderRepeater (writer, this, ControlStyle, this);
		}

		protected override object SaveViewState ()
		{
			object[] state = new object [9];
			state[0] = base.SaveViewState ();
			if (itemStyle != null)
				state [1] = itemStyle.SaveViewState ();
			if (selectedItemStyle != null)
				state [2] = selectedItemStyle.SaveViewState ();
			if (alternatingItemStyle != null)
				state [3] = alternatingItemStyle.SaveViewState ();
			if (editItemStyle != null)
				state [4] = editItemStyle.SaveViewState ();
			if (separatorStyle != null)
				state [5] = separatorStyle.SaveViewState ();
			if (headerStyle != null)
				state [6] = headerStyle.SaveViewState ();
			if (footerStyle != null)
				state [7] = footerStyle.SaveViewState ();
			if (ControlStyleCreated)
				state [8] = ControlStyle.SaveViewState ();
			return state;
		}

		protected override void TrackViewState ()
		{
			base.TrackViewState ();
			if (alternatingItemStyle != null)
				alternatingItemStyle.TrackViewState ();
			if (editItemStyle != null)
				editItemStyle.TrackViewState ();
			if (footerStyle != null)
				footerStyle.TrackViewState ();
			if (headerStyle != null)
				headerStyle.TrackViewState ();
			if (itemStyle != null)
				itemStyle.TrackViewState ();
			if (selectedItemStyle != null)
				selectedItemStyle.TrackViewState ();
			if (separatorStyle != null)
				separatorStyle.TrackViewState ();
			if (ControlStyleCreated)
				ControlStyle.TrackViewState ();
		}


		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataListCommandEventHandler CancelCommand {
			add { Events.AddHandler (cancelCommandEvent, value); }
			remove { Events.RemoveHandler (cancelCommandEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataListCommandEventHandler DeleteCommand {
			add { Events.AddHandler (deleteCommandEvent, value); }
			remove { Events.RemoveHandler (deleteCommandEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataListCommandEventHandler EditCommand {
			add { Events.AddHandler (editCommandEvent, value); }
			remove { Events.RemoveHandler (editCommandEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataListCommandEventHandler ItemCommand {
			add { Events.AddHandler (itemCommandEvent, value); }
			remove { Events.RemoveHandler (itemCommandEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataListItemEventHandler ItemCreated {
			add { Events.AddHandler (itemCreatedEvent, value); }
			remove { Events.RemoveHandler (itemCreatedEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataListItemEventHandler ItemDataBound {
			add { Events.AddHandler (itemDataBoundEvent, value); }
			remove { Events.RemoveHandler (itemDataBoundEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataListCommandEventHandler UpdateCommand {
			add { Events.AddHandler (updateCommandEvent, value); }
			remove { Events.RemoveHandler (updateCommandEvent, value); }
		}


		// IRepeatInfoUser support

		bool IRepeatInfoUser.HasFooter {
			get { return (ShowFooter && (footerTemplate != null)); }
		}

		bool IRepeatInfoUser.HasHeader {
			get { return (ShowHeader && (headerTemplate != null)); }
		}
		
		bool IRepeatInfoUser.HasSeparators {
			get { return (separatorTemplate != null); }
		}

		// don't include header, footer and separators in the count
		int IRepeatInfoUser.RepeatedItemCount {
			get {
				if (idx == -1) {
					object o = ViewState ["Items"];
					idx = (o == null) ? 0 : (int) o;
				}
				return idx;
			}
		}

		Style IRepeatInfoUser.GetItemStyle (ListItemType itemType, int repeatIndex)
		{
			DataListItem item = null;
			switch (itemType) {
				case ListItemType.Header:
				case ListItemType.Footer:
					if (repeatIndex >= 0 && (!HasControls () || repeatIndex >= Controls.Count))
						throw new ArgumentOutOfRangeException ();

					item = FindFirstItem (itemType);
					break;
				case ListItemType.Item:
				case ListItemType.AlternatingItem:
				case ListItemType.SelectedItem:
				case ListItemType.EditItem:
					if (repeatIndex >= 0 && (!HasControls () || repeatIndex >= Controls.Count))
						throw new ArgumentOutOfRangeException ();

					item = FindBestItem (repeatIndex);
					break;
				case ListItemType.Separator:
					if (repeatIndex >= 0 && (!HasControls () || repeatIndex >= Controls.Count))
						throw new ArgumentOutOfRangeException ();

					item = FindSpecificItem (itemType, repeatIndex);
					break;
				default:
					item = null;
					break;
			}

			if (item == null || item.ControlStyleCreated == false)
				return null;

			return item.ControlStyle;
		}

		// Header and Footer don't have a "real" index (-1)
		DataListItem FindFirstItem (ListItemType itemType)
		{
			for (int i = 0; i < Controls.Count; i++) {
				DataListItem item = (Controls [i] as DataListItem);
				if ((item != null) && (item.ItemType == itemType))
					return item;
			}
			return null;
		}

		// Both Type and Index must match (e.g. Separator)
		DataListItem FindSpecificItem (ListItemType itemType, int repeatIndex)
		{
			for (int i = 0; i < Controls.Count; i++) {
				DataListItem item = (Controls [i] as DataListItem);
				if ((item != null) && (item.ItemType == itemType) && (item.ItemIndex == repeatIndex))
					return item;
			}
			return null;
		}

		// we get call for Item even for AlternatingItem :(
		DataListItem FindBestItem (int repeatIndex)
		{
			for (int i = 0; i < Controls.Count; i++) {
				DataListItem item = (Controls [i] as DataListItem);
				if ((item != null) && (item.ItemIndex == repeatIndex)) {
					switch (item.ItemType) {
						case ListItemType.Item:
						case ListItemType.AlternatingItem:
						case ListItemType.SelectedItem:
						case ListItemType.EditItem:
							return item;
						default:
							return null;
					}
				}
			}
			return null;
		}

		void IRepeatInfoUser.RenderItem (ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
		{
			// if possible take the easy way out...
			if (!HasControls ())
				return;

			DataListItem item = null;
			switch (itemType) {
				case ListItemType.Header:
				case ListItemType.Footer:
					item = FindFirstItem (itemType);
					break;
				case ListItemType.Item:
				case ListItemType.AlternatingItem:
				case ListItemType.SelectedItem:
				case ListItemType.EditItem:
					item = FindBestItem (repeatIndex);
					break;
				case ListItemType.Separator:
					item = FindSpecificItem (itemType, repeatIndex);
					break;
			}

			if (item != null) {
				bool extract = ExtractTemplateRows;
				bool table = (RepeatLayout == RepeatLayout.Table);
				if (!table || extract) {
					// sadly RepeatInfo doesn't support Style for RepeatLayout.Flow
					Style s = (this as IRepeatInfoUser).GetItemStyle (itemType, repeatIndex);
					if (s != null)
						item.ControlStyle.CopyFrom (s);
				}
//Console.WriteLine ("RenderItem #{0} type {1}", repeatIndex, itemType);
				item.RenderItem (writer, extract, table);
			} else {
//Console.WriteLine ("Couldn't find #{0} type {1} out of {2} items / {3} controls", repeatIndex, itemType, Items.Count, Controls.Count);
			}
		}
	}
}
