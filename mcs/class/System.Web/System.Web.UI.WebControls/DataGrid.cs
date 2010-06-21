//
// System.Web.UI.WebControls.DataGrid.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Web.Util;
using System.Collections;
using System.Globalization;
using System.ComponentModel;
using System.Reflection;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[Editor("System.Web.UI.Design.WebControls.DataGridComponentEditor, " + Consts.AssemblySystem_Design, typeof(System.ComponentModel.ComponentEditor))]
	[Designer("System.Web.UI.Design.WebControls.DataGridDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class DataGrid : BaseDataList, INamingContainer {

		public const string CancelCommandName = "Cancel";
		public const string DeleteCommandName = "Delete";
		public const string EditCommandName = "Edit";
		public const string SelectCommandName = "Select";
		public const string SortCommandName = "Sort";
		public const string UpdateCommandName = "Update";

		public const string PageCommandName = "Page";
		public const string NextPageCommandArgument = "Next";
		public const string PrevPageCommandArgument = "Prev";

		static readonly object CancelCommandEvent = new object ();
		static readonly object DeleteCommandEvent = new object ();
		static readonly object EditCommandEvent = new object ();
		static readonly object ItemCommandEvent = new object ();
		static readonly object ItemCreatedEvent = new object ();
		static readonly object ItemDataBoundEvent = new object ();
		static readonly object PageIndexChangedEvent = new object ();
		static readonly object SortCommandEvent = new object ();
		static readonly object UpdateCommandEvent = new object ();

		TableItemStyle alt_item_style;
		TableItemStyle edit_item_style;
		TableItemStyle footer_style;
		TableItemStyle header_style;
		TableItemStyle item_style;
		TableItemStyle selected_style;
		DataGridPagerStyle pager_style;
		
		ArrayList items_list;
		DataGridItemCollection items;

		ArrayList columns_list;
		DataGridColumnCollection columns;

		ArrayList data_source_columns_list;
		DataGridColumnCollection data_source_columns;

		Table render_table;
		DataGridColumn [] render_columns;
		PagedDataSource paged_data_source;
		IEnumerator data_enumerator;
		
		[DefaultValue(false)]
		[WebSysDescription ("")]
		[WebCategory ("Paging")]
		public virtual bool AllowCustomPaging {
			get { return ViewState.GetBool ("AllowCustomPaging", false); }
			set { ViewState ["AllowCustomPaging"] = value; }
		}

		[DefaultValue(false)]
 		[WebSysDescription ("")]
		[WebCategory ("Paging")]
		public virtual bool AllowPaging {
			get { return ViewState.GetBool ("AllowPaging", false); }
			set { ViewState ["AllowPaging"] = value; }
		}

		[DefaultValue(false)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool AllowSorting {
			get { return ViewState.GetBool ("AllowSorting", false); }
			set { ViewState ["AllowSorting"] = value; }
		}

		[DefaultValue(true)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool AutoGenerateColumns {
			get { return ViewState.GetBool ("AutoGenerateColumns", true); }
			set { ViewState ["AutoGenerateColumns"] = value; }
		}

#if NET_2_0
		[UrlProperty]
#else
		[Bindable(true)]
#endif
		[DefaultValue("")]
		[Editor("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
 		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string BackImageUrl {
			get { return TableStyle.BackImageUrl; }
			set { TableStyle.BackImageUrl = value; }
		}

#if ONLY_1_1
		[Bindable(true)]
#endif
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public int CurrentPageIndex {
			get { return ViewState.GetInt ("CurrentPageIndex", 0); }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["CurrentPageIndex"] = value;
			}
		}

		[DefaultValue(-1)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual int EditItemIndex {
			get { return ViewState.GetInt ("EditItemIndex", -1); }
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["EditItemIndex"] = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public int PageCount {
			get {
				if (paged_data_source != null)
					return paged_data_source.PageCount;

				return ViewState.GetInt ("PageCount", 0);
			}
		}

		[DefaultValue(10)]
		[WebSysDescription ("")]
		[WebCategory ("Paging")]
		public virtual int PageSize {
			get { return ViewState.GetInt ("PageSize", 10); }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["PageSize"] = value;
			}
		}

		void AdjustItemTypes (int prev_select, int new_select)
		{
			if (items_list == null)
				return; // nothing to select

			int count = items_list.Count;
			if (count == 0)
				return; // nothing to select

			DataGridItem item;
			// Restore item type for the previously selected one.
			if (prev_select >= 0 && prev_select < count) {
				item = (DataGridItem) items_list [prev_select];
				
				if (item.ItemType == ListItemType.EditItem) {
					// nothing to do. This has priority.
				} else if ((item.ItemIndex % 2) != 0) {
					item.SetItemType (ListItemType.AlternatingItem);
				} else {
					item.SetItemType (ListItemType.Item);
				}
			}

			if (new_select == -1 || new_select >= count)
				return; // nothing to select

			item = (DataGridItem) items_list [new_select];
			if (item.ItemType != ListItemType.EditItem) // EditItem takes precedence
				item.SetItemType (ListItemType.SelectedItem);
		}

		[Bindable(true)]
		[DefaultValue(-1)]
		[WebSysDescription ("")]
		[WebCategory ("Paging")]
		public virtual int SelectedIndex {
			get { return ViewState.GetInt ("SelectedIndex", -1); }
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value");

				int selected_index = ViewState.GetInt ("SelectedIndex", -1);
				AdjustItemTypes (selected_index, value);
				ViewState ["SelectedIndex"] = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle AlternatingItemStyle {
			get {
				if (alt_item_style == null) {
					alt_item_style = new TableItemStyle ();
					if (IsTrackingViewState)
						alt_item_style.TrackViewState ();
				}
				return alt_item_style;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle EditItemStyle {
			get {
				if (edit_item_style == null) {
					edit_item_style = new TableItemStyle ();
					if (IsTrackingViewState)
						edit_item_style.TrackViewState ();
				}
				return edit_item_style;
			}
		}

		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle FooterStyle {
			get {
				if (footer_style == null) {
					footer_style = new TableItemStyle ();
					if (IsTrackingViewState)
						footer_style.TrackViewState ();
				}
				return footer_style;
			}
		}

		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle HeaderStyle {
			get {
				if (header_style == null) {
					header_style = new TableItemStyle ();
					if (IsTrackingViewState)
						header_style.TrackViewState ();
				}
				return header_style;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle ItemStyle {
			get {
				if (item_style == null) {
					item_style = new TableItemStyle ();
					if (IsTrackingViewState)
						item_style.TrackViewState ();
				}
				return item_style;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle SelectedItemStyle {
			get {
				if (selected_style == null) {
					selected_style = new TableItemStyle ();
					if (IsTrackingViewState)
						selected_style.TrackViewState ();
				}
				return selected_style;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual DataGridPagerStyle PagerStyle {
			get {
				if (pager_style == null) {
					pager_style = new DataGridPagerStyle ();
					if (IsTrackingViewState)
						pager_style.TrackViewState ();
				}
				return pager_style;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual DataGridItemCollection Items {
			get {
				EnsureChildControls ();
				if (items == null) {
					if (items_list == null)
						items_list = new ArrayList ();
					items = new DataGridItemCollection (items_list);
				}
				return items;
			}
		}

		[DefaultValue (null)]
		[Editor ("System.Web.UI.Design.WebControls.DataGridColumnCollectionEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[MergableProperty (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual DataGridColumnCollection Columns {
			get {
				if (columns == null) {
					columns_list = new ArrayList ();
					columns = new DataGridColumnCollection (this, columns_list);
					if (IsTrackingViewState) {
						IStateManager manager = (IStateManager) columns;
						manager.TrackViewState ();
					}
				}
				return columns;
			}
		}

		DataGridColumnCollection DataSourceColumns {
			get {
				if (data_source_columns == null) {
					data_source_columns_list = new ArrayList ();
					data_source_columns = new DataGridColumnCollection (this,
							data_source_columns_list);
					if (IsTrackingViewState) {
						IStateManager manager = (IStateManager) data_source_columns;
						manager.TrackViewState ();
					}
				}
				return data_source_columns;
			}
		}
		
		Table RenderTable {
			get {
				if (render_table == null) {
#if ONLY_1_1
					render_table = new TableID (this);
#else
					render_table = new ChildTable (this);
#endif
					render_table.AutoID = false;
				}
				return render_table;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Paging")]
		public virtual DataGridItem SelectedItem {
			get {
				if (SelectedIndex == -1)
					return null;
				return Items [SelectedIndex];
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif
		[DefaultValue(false)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual bool ShowFooter {
			get { return ViewState.GetBool ("ShowFooter", false); }
			set { ViewState ["ShowFooter"] = value; }
		}

#if ONLY_1_1
		[Bindable(true)]
#endif
		[DefaultValue(true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual bool ShowHeader {
			get { return ViewState.GetBool ("ShowHeader", true); }
			set { ViewState ["ShowHeader"] = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual int VirtualItemCount {
			get { return ViewState.GetInt ("VirtualItemCount", 0); }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["VirtualItemCount"] = value;
			}
		}

#if NET_2_0
		protected override HtmlTextWriterTag TagKey {
			get { return HtmlTextWriterTag.Table; }
		}
#endif

		TableStyle TableStyle {
			get { return (TableStyle) ControlStyle; }
		}

		static Type [] item_args = new Type [] {typeof (int) };
		void AddColumnsFromSource (PagedDataSource data_source)
		{
			PropertyDescriptorCollection props = null;
			Type ptype = null;
			bool no_items = false;

			// Use plain reflection for the Item property.
			// If we use TypeDescriptor, props will hold
			// all of the Type properties, which will be listed as columns
			Type ds_type = data_source.GetType ();
			PropertyInfo pinfo = ds_type.GetProperty ("Item", item_args);
			if (pinfo == null) {
				IEnumerator items = (data_source.DataSource != null) ? data_source.GetEnumerator () : null;
				if (items != null && items.MoveNext ()) {
					object data = items.Current;
					if ((data is ICustomTypeDescriptor) || (!IsBindableType(data.GetType())))
						props = TypeDescriptor.GetProperties (data);
					else if (data != null)
						ptype = data.GetType ();
					data_enumerator = items;
				} else {
					no_items = true;
				}
			} else {
				ptype = pinfo.PropertyType;
			}

			if (ptype != null) {
				// Found the "Item" property
				AddPropertyToColumns ();
			} else if (props != null) {
				foreach (PropertyDescriptor pd in props)
					AddPropertyToColumns (pd, false);
			} else if (!no_items) {
				// This is not thrown for an empty ArrayList.
				string msg = String.Format ("DataGrid '{0}' cannot autogenerate " +
							"columns from the given datasource. {1}", ID, ptype);
				throw new HttpException (msg);
			}
		}

		protected virtual ArrayList CreateColumnSet (PagedDataSource dataSource, bool useDataSource)
		{
			ArrayList res = new ArrayList ();
			if (columns_list != null)
				res.AddRange (columns_list);

			if (AutoGenerateColumns) {
				if (useDataSource) {
					data_enumerator = null;
					PropertyDescriptorCollection props = dataSource.GetItemProperties (null);
					DataSourceColumns.Clear ();
					if (props != null) {
						foreach (PropertyDescriptor d in props)
							AddPropertyToColumns (d, false);
					} else {
						AddColumnsFromSource (dataSource);
					}
				}

				if (data_source_columns != null && data_source_columns.Count > 0)
					res.AddRange (data_source_columns);
			}

			return res;
		}

		void AddPropertyToColumns ()
		{
			BoundColumn b = new BoundColumn ();
			if (IsTrackingViewState) {
				IStateManager m = (IStateManager) b;
				m.TrackViewState ();
			}
			b.Set_Owner (this);
			b.HeaderText = "Item";
			b.SortExpression = "Item";
			b.DataField  = BoundColumn.thisExpr;
			DataSourceColumns.Add (b);
		}

		void AddPropertyToColumns (PropertyDescriptor prop, bool tothis)
		{
			BoundColumn b = new BoundColumn ();
			b.Set_Owner (this);
			if (IsTrackingViewState) {
				IStateManager m = (IStateManager) b;
				m.TrackViewState ();
			}
			b.HeaderText = prop.Name;
			b.DataField = (tothis ? BoundColumn.thisExpr : prop.Name);
			b.SortExpression = prop.Name;
#if NET_2_0
			if (string.Compare (DataKeyField, b.DataField, StringComparison.InvariantCultureIgnoreCase) == 0) {
				b.ReadOnly = true;
			}
#endif
			DataSourceColumns.Add (b);
		}

		protected override void TrackViewState ()
		{
			base.TrackViewState ();

			if (pager_style != null)
				pager_style.TrackViewState ();
			if (header_style != null)
				header_style.TrackViewState ();
			if (footer_style != null)
				footer_style.TrackViewState ();
			if (item_style != null)
				item_style.TrackViewState ();
			if (alt_item_style != null)
				alt_item_style.TrackViewState ();
			if (selected_style != null)
				selected_style.TrackViewState ();
			if (edit_item_style != null)
				edit_item_style.TrackViewState ();
#if NET_2_0
			if (ControlStyleCreated)
				ControlStyle.TrackViewState ();
#endif
			IStateManager manager = (IStateManager) columns;
			if (manager != null)
				manager.TrackViewState ();
		}

		protected override object SaveViewState ()
		{
#if NET_2_0
			object [] res = new object [11];
#else
			object [] res = new object [10];
#endif

			res [0] = base.SaveViewState ();
			if (columns != null) {
				IStateManager cm = (IStateManager) columns;
				res [1] = cm.SaveViewState ();
			}
			if (pager_style != null)
				res [2] = pager_style.SaveViewState ();
			if (header_style != null)
				res [3] = header_style.SaveViewState ();
			if (footer_style != null)
				res [4] = footer_style.SaveViewState ();
			if (item_style != null)
				res [5] = item_style.SaveViewState ();
			if (alt_item_style != null)
				res [6] = alt_item_style.SaveViewState ();
			if (selected_style != null)
				res [7] = selected_style.SaveViewState ();
			if (edit_item_style != null)
				res [8] = edit_item_style.SaveViewState ();
#if NET_2_0
			if (ControlStyleCreated)
				res [9] = ControlStyle.SaveViewState ();
			
			if (data_source_columns != null) {
				IStateManager m = (IStateManager) data_source_columns;
				res [10] = m.SaveViewState ();
			}
#else
			if (data_source_columns != null) {
				IStateManager m = (IStateManager) data_source_columns;
				res [9] = m.SaveViewState ();
			}
#endif

			return res;
		}

		protected override void LoadViewState (object savedState)
		{
			object [] pieces = savedState as object [];

			if (pieces == null)
				return;

			base.LoadViewState (pieces [0]);
			if (columns != null) {
				IStateManager cm = (IStateManager) columns;
				cm.LoadViewState (pieces [1]);
			}
			if (pieces [2] != null)
				PagerStyle.LoadViewState (pieces [2]);
			if (pieces [3] != null)
				HeaderStyle.LoadViewState (pieces [3]);
			if (pieces [4] != null)
				FooterStyle.LoadViewState (pieces [4]);
			if (pieces [5] != null)
				ItemStyle.LoadViewState (pieces [5]);
			if (pieces [6] != null)
				AlternatingItemStyle.LoadViewState (pieces [6]);
			if (pieces [7] != null)
				SelectedItemStyle.LoadViewState (pieces [7]);
			if (pieces [8] != null)
				EditItemStyle.LoadViewState (pieces [8]);

#if NET_2_0
			if (pieces [9] != null)
				ControlStyle.LoadViewState (pieces [8]);

			if (pieces [10] != null) {
				// IStateManager manager = (IStateManager) DataSourceColumns;
				// manager.LoadViewState (pieces [10]);
				object [] cols = (object []) pieces [10];
				foreach (object o in cols) {
					BoundColumn c = new BoundColumn ();
					((IStateManager) c).TrackViewState ();
					c.Set_Owner (this);
					((IStateManager) c).LoadViewState (o);
					DataSourceColumns.Add (c);
				}
			}
#else
			if (pieces [9] != null) {
				// IStateManager manager = (IStateManager) DataSourceColumns;
				// manager.LoadViewState (pieces [9]);
				object [] cols = (object []) pieces [9];
				foreach (object o in cols) {
					BoundColumn c = new BoundColumn ();
					c.Set_Owner (this);
					((IStateManager) c).LoadViewState (o);
					DataSourceColumns.Add (c);
				}
			}
#endif
		}

		protected override Style CreateControlStyle ()
		{
#if NET_2_0
			TableStyle res = new TableStyle ();
#else
			TableStyle res = new TableStyle (ViewState);
#endif
			res.GridLines = GridLines.Both;
			res.CellSpacing = 0;
			return res;
		}

		protected virtual void InitializeItem (DataGridItem item, DataGridColumn [] columns)
		{
			bool th = UseAccessibleHeader && item.ItemType == ListItemType.Header;
			for (int i = 0; i < columns.Length; i++) {
				TableCell cell = null;
				if (th) {
					cell = new TableHeaderCell ();
					cell.Attributes["scope"] = "col";
				}
				else
					cell = new TableCell ();
				columns [i].InitializeCell (cell, i, item.ItemType);
				item.Cells.Add (cell);
			}
		}

		protected virtual void InitializePager (DataGridItem item, int columnSpan, PagedDataSource pagedDataSource)
		{
			TableCell pager_cell;
			if (PagerStyle.Mode == PagerMode.NextPrev)
				pager_cell = InitializeNextPrevPager (item, columnSpan, pagedDataSource);
			else
				pager_cell = InitializeNumericPager (item, columnSpan, pagedDataSource);

			item.Controls.Add (pager_cell);
		}

		TableCell InitializeNumericPager (DataGridItem item, int columnSpan,
				PagedDataSource paged)
		{
			TableCell res = new TableCell ();
			res.ColumnSpan = columnSpan;

			int button_count = PagerStyle.PageButtonCount;
			int current = paged.CurrentPageIndex;
			int start = current - (current % button_count);
			int end = start + button_count;

			if (end > paged.PageCount)
				end = paged.PageCount;

			if (start > 0) {
				LinkButton link = new LinkButton ();
				link.Text = "...";
				link.CommandName = PageCommandName;
				link.CommandArgument = start.ToString (Helpers.InvariantCulture);
				link.CausesValidation = false;
				res.Controls.Add (link);
				res.Controls.Add (new LiteralControl ("&nbsp;"));
			}

			for (int i = start; i < end; i++) {
				Control number = null;
				string page = (i + 1).ToString (Helpers.InvariantCulture);
				if (i != paged.CurrentPageIndex) {
					LinkButton link = new LinkButton ();
					link.Text = page;
					link.CommandName = PageCommandName;
					link.CommandArgument = page;
					link.CausesValidation = false;
					number = link;
				} else {
					Label pageLabel = new Label();
					pageLabel.Text = page;
					number = pageLabel;
				}

				res.Controls.Add (number);
				if (i < end - 1)
					res.Controls.Add (new LiteralControl ("&nbsp;"));
			}

			if (end < paged.PageCount) {
				res.Controls.Add (new LiteralControl ("&nbsp;"));
				LinkButton link = new LinkButton ();
				link.Text = "...";
				link.CommandName = PageCommandName;
				link.CommandArgument = (end + 1).ToString (Helpers.InvariantCulture);
				link.CausesValidation = false;
				res.Controls.Add (link);
			}

			return res;
		}

		TableCell InitializeNextPrevPager (DataGridItem item, int columnSpan, PagedDataSource paged)
		{
			TableCell res = new TableCell ();
			res.ColumnSpan = columnSpan;

			Control prev;
			Control next;

			if (paged.IsFirstPage) {
				Label l = new Label ();
				l.Text = PagerStyle.PrevPageText;
				prev = l;
			} else {
#if NET_2_0
				LinkButton l = new DataControlLinkButton ();
#else
				LinkButton l = new LinkButton ();
#endif
				l.Text = PagerStyle.PrevPageText;
				l.CommandName = PageCommandName;
				l.CommandArgument = PrevPageCommandArgument;
				l.CausesValidation = false;
				prev = l;
			}

			if (paged.Count > 0 && !paged.IsLastPage) {
#if NET_2_0
				LinkButton l = new DataControlLinkButton ();
#else
				LinkButton l = new LinkButton ();
#endif
				l.Text = PagerStyle.NextPageText;
				l.CommandName = PageCommandName;
				l.CommandArgument = NextPageCommandArgument;
				l.CausesValidation = false;
				next = l;
			} else {
				Label l = new Label ();
				l.Text = PagerStyle.NextPageText;
				next = l;
			}

			res.Controls.Add (prev);
			res.Controls.Add (new LiteralControl ("&nbsp;"));
			res.Controls.Add (next);

			return res;
		}
				
		protected virtual DataGridItem CreateItem (int itemIndex, int dataSourceIndex,
				ListItemType itemType)
		{
			DataGridItem res = new DataGridItem (itemIndex, dataSourceIndex, itemType);
			return res;
		}

		DataGridItem CreateItem (int item_index, int data_source_index,
				ListItemType type, bool data_bind, object data_item,
				PagedDataSource paged)
		{
			DataGridItem res = CreateItem (item_index, data_source_index, type);
			DataGridItemEventArgs args = new DataGridItemEventArgs (res);
			bool no_pager = (type != ListItemType.Pager);

			if (no_pager) {
				InitializeItem (res, render_columns);
				if (data_bind)
					res.DataItem = data_item;
				OnItemCreated (args);
			} else {
				InitializePager (res, render_columns.Length, paged);
				if (pager_style != null)
					res.ApplyStyle (pager_style);
				OnItemCreated (args);
			}

			// Add before the column is bound, so that the
			// value is saved in the viewstate
			RenderTable.Controls.Add (res);

			if (no_pager && data_bind) {
				res.DataBind ();
				OnItemDataBound (args);
				res.DataItem = null;
			}
			return res;
		}

		class NCollection : ICollection {
			int n;

			public NCollection (int n)
			{
				this.n = n;
			}

			public IEnumerator GetEnumerator ()
			{
				for (int i = 0; i < n; i++)
					yield return i;
			}

			public int Count {
				get { return n; }
			}

			public bool IsSynchronized {
				get { return false; }
			}

			public object SyncRoot {
				get { return this; }
			}

			public void CopyTo (Array array, int index)
			{
				throw new NotImplementedException ("This should never be called");
			}
		}

		protected override void CreateControlHierarchy (bool useDataSource)
		{
			Controls.Clear ();
			RenderTable.Controls.Clear ();
			Controls.Add (RenderTable);
			
			IEnumerable data_source;
			ArrayList keys = null;
			if (useDataSource) {
#if NET_2_0
				if (IsBoundUsingDataSourceID)
					data_source = GetData ();
				else
#endif
				data_source = DataSourceResolver.ResolveDataSource (DataSource, DataMember);
				if (data_source == null) {
					Controls.Clear ();
					return;
				}
				
				keys = DataKeysArray;
				keys.Clear ();
			} else {
				int nitems = ViewState.GetInt ("Items", 0);
				data_source = new NCollection (nitems);
			}

			paged_data_source = new PagedDataSource ();
			PagedDataSource pds = paged_data_source;
			pds.AllowPaging = AllowPaging;
			pds.AllowCustomPaging = AllowCustomPaging;
			pds.DataSource = data_source;
			pds.CurrentPageIndex = CurrentPageIndex;
			pds.PageSize = PageSize;
			pds.VirtualCount = VirtualItemCount;

			if ((pds.IsPagingEnabled) && (pds.PageCount < pds.CurrentPageIndex)) {
				Controls.Clear ();
				throw new HttpException ("Invalid DataGrid PageIndex");
			}
			
			ArrayList cList = CreateColumnSet (paged_data_source, useDataSource);
			if (cList.Count == 0) {
				Controls.Clear ();
				return;
			}
			
			Page page = this.Page;
			if (page != null)
				page.RequiresPostBackScript ();
			
			render_columns = new DataGridColumn [cList.Count];
			for (int c = 0; c < cList.Count; c++) {
				DataGridColumn col = (DataGridColumn) cList [c];
				col.Set_Owner (this);
				col.Initialize ();
				render_columns [c] = col;
			}

			if (pds.IsPagingEnabled)
				CreateItem (-1, -1, ListItemType.Pager, false, null, pds);

			CreateItem (-1, -1, ListItemType.Header, useDataSource, null, pds);

			// No indexer on PagedDataSource so we have to do
			// this silly foreach and index++
			if (items_list == null)
				items_list = new ArrayList ();
			else
				items_list.Clear();

			bool skip_first = false;
			IEnumerator enumerator = null;
			if (data_enumerator != null) {
				// replaced when creating bound columns
				enumerator = data_enumerator;
				skip_first = true;
			} else if (pds.DataSource != null) {
				enumerator = pds.GetEnumerator ();
			} else {
				enumerator = null;
			}

			int index = 0;
			bool first = true;
			string key = null;
			int dataset_index = pds.FirstIndexInPage;
			int selected_index = SelectedIndex;
			int edit_item_index = EditItemIndex;
			while (enumerator != null && (skip_first || enumerator.MoveNext ())) {
				// MS does not render <table blah></table> on empty datasource.
				if (first) {
					first = false;
					key = DataKeyField;
					skip_first = false;
				}
				object data = enumerator.Current;
				// This will throw if the DataKeyField is not there. As on MS, this
				// will not be hit on an empty datasource.
				// The values stored here can be used in events so that you can
				// get, for example, the row that was clicked
				// (data.Rows.Find (ItemsGrid.DataKeys [e.Item.ItemIndex]))
				// BaseDataList will keep the array across postbacks.
				if (useDataSource && key != "")
					keys.Add (DataBinder.GetPropertyValue (data, key));

				ListItemType type = ListItemType.Item;
				if (index == edit_item_index) 
					type = ListItemType.EditItem;
				else if (index == selected_index) 
					type = ListItemType.SelectedItem;
				else if (index % 2 != 0) 
					type = ListItemType.AlternatingItem;

				items_list.Add (CreateItem (index, dataset_index, type, useDataSource, data, pds));
				index++;
				dataset_index++;
			}

			CreateItem (-1, -1, ListItemType.Footer, useDataSource, null, paged_data_source);
			if (pds.IsPagingEnabled) {
				CreateItem (-1, -1, ListItemType.Pager, false, null, paged_data_source);
				if (useDataSource)
					ViewState ["Items"] = pds.IsCustomPagingEnabled ? index : pds.DataSourceCount;
			} else if (useDataSource) {
				ViewState ["Items"] = index;
			}
		}

		void ApplyColumnStyle (TableCellCollection cells, ListItemType type)
		{
			int ncells = Math.Min (cells.Count, render_columns.Length);
			if (ncells <= 0)
				return;

			for (int i = 0; i < ncells; i++) {
				Style style = null;
				TableCell cell = cells [i];
				DataGridColumn column = render_columns [i];
				if (!column.Visible) {
					cell.Visible = false;
					continue;
				}

				style = column.GetStyle (type);
				if (style != null)
					cell.MergeStyle (style);
			}
		}

		protected override void PrepareControlHierarchy ()
		{
			if (!HasControls () || Controls.Count == 0)
				return; // No one called CreateControlHierarchy() with DataSource != null

			Table rt = render_table;
			rt.CopyBaseAttributes (this);
			rt.ApplyStyle (ControlStyle);

			rt.Caption = Caption;
			rt.CaptionAlign = CaptionAlign;
			rt.Enabled = IsEnabled;

			bool top_pager = true;
			foreach (DataGridItem item in rt.Rows) {
				
				switch (item.ItemType) {
				case ListItemType.Item:
					ApplyItemStyle (item);
					break;
				case ListItemType.AlternatingItem:
					ApplyItemStyle (item);
					break;
				case ListItemType.EditItem:
					item.MergeStyle (edit_item_style);
					ApplyItemStyle (item);
					ApplyColumnStyle (item.Cells, ListItemType.EditItem);
					break;
				case ListItemType.Footer:
					if (!ShowFooter) {
						item.Visible = false;
						break;
					}
					if (footer_style != null)
						item.MergeStyle (footer_style);
					ApplyColumnStyle (item.Cells, ListItemType.Footer);
					break;
				case ListItemType.Header:
					if (!ShowHeader) {
						item.Visible = false;
						break;
					}
					if (header_style != null)
						item.MergeStyle (header_style);
					ApplyColumnStyle (item.Cells, ListItemType.Header);
					break;
				case ListItemType.SelectedItem:
					item.MergeStyle (selected_style);
					ApplyItemStyle (item);
					ApplyColumnStyle (item.Cells, ListItemType.SelectedItem);
					break;
				case ListItemType.Separator:
					ApplyColumnStyle (item.Cells, ListItemType.Separator);
					break;
				case ListItemType.Pager:
					DataGridPagerStyle ps = PagerStyle;
					if (ps.Visible == false || !paged_data_source.IsPagingEnabled) {
						item.Visible = false;
					} else {
						if (top_pager)
							item.Visible = (ps.Position != PagerPosition.Bottom);
						else
							item.Visible = (ps.Position != PagerPosition.Top);
						top_pager = false;
					}

					if (item.Visible)
						item.MergeStyle (pager_style);
					break;
				}
			}
		}

		void ApplyItemStyle (DataGridItem item)
		{
			if (item.ItemIndex % 2 != 0)
				item.MergeStyle (alt_item_style);

			item.MergeStyle (item_style);
			ApplyColumnStyle (item.Cells, ListItemType.Item);
		}

		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			DataGridCommandEventArgs de = e as DataGridCommandEventArgs;

			if (de == null)
				return false;

			string cn = de.CommandName;
			CultureInfo inv = Helpers.InvariantCulture;

			OnItemCommand (de);
			if (String.Compare (cn, CancelCommandName, true, inv) == 0) {
				OnCancelCommand (de);
			} else if (String.Compare (cn, DeleteCommandName, true, inv) == 0) {
				OnDeleteCommand (de);
			} else if (String.Compare (cn, EditCommandName, true, inv) == 0) {
				OnEditCommand (de);
			} else if (String.Compare (cn, SelectCommandName, true, inv) == 0) {
				SelectedIndex = de.Item.ItemIndex;
				OnSelectedIndexChanged (de);
			} else if (String.Compare (cn, SortCommandName, true, inv) == 0) {
				DataGridSortCommandEventArgs se = new DataGridSortCommandEventArgs (de.CommandSource, de);
				OnSortCommand (se);
			} else if (String.Compare (cn, UpdateCommandName, true, inv) == 0) {
				OnUpdateCommand (de);
			} else if (String.Compare (cn, PageCommandName, true, inv) == 0) {
				int new_index;
				if (String.Compare ((string) de.CommandArgument,
						    NextPageCommandArgument, true, inv) == 0) {
					new_index = CurrentPageIndex + 1;
				} else if (String.Compare ((string) de.CommandArgument,
							   PrevPageCommandArgument, true, inv) == 0) {
					new_index = CurrentPageIndex - 1;
				} else {
					// It seems to just assume it's an int and parses, no
					// checks to make sure its valid or anything.
					//  also it's always one less then specified, not sure
					// why that is.
					new_index = Int32.Parse ((string) de.CommandArgument, inv) - 1;
				}
				DataGridPageChangedEventArgs pc = new DataGridPageChangedEventArgs (
					de.CommandSource, new_index);
				OnPageIndexChanged (pc);
			}

			return true;
		}

		protected virtual void OnCancelCommand (DataGridCommandEventArgs e)
		{
			DataGridCommandEventHandler handler = (DataGridCommandEventHandler) Events [CancelCommandEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnDeleteCommand (DataGridCommandEventArgs e)
		{
			DataGridCommandEventHandler handler = (DataGridCommandEventHandler) Events [DeleteCommandEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnEditCommand (DataGridCommandEventArgs e)
		{
			DataGridCommandEventHandler handler = (DataGridCommandEventHandler) Events [EditCommandEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnItemCommand (DataGridCommandEventArgs e)
		{
			DataGridCommandEventHandler handler = (DataGridCommandEventHandler) Events [ItemCommandEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnItemCreated (DataGridItemEventArgs e)
		{
			DataGridItemEventHandler handler = (DataGridItemEventHandler) Events [ItemCreatedEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnItemDataBound (DataGridItemEventArgs e)
		{
			DataGridItemEventHandler handler = (DataGridItemEventHandler) Events [ItemDataBoundEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnPageIndexChanged (DataGridPageChangedEventArgs e)
		{
			DataGridPageChangedEventHandler handler = (DataGridPageChangedEventHandler) Events [PageIndexChangedEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnSortCommand (DataGridSortCommandEventArgs e)
		{
			DataGridSortCommandEventHandler handler = (DataGridSortCommandEventHandler) Events [SortCommandEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnUpdateCommand (DataGridCommandEventArgs e)
		{
			DataGridCommandEventHandler handler = (DataGridCommandEventHandler) Events [UpdateCommandEvent];
			if (handler != null)
				handler (this, e);
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridCommandEventHandler CancelCommand {
			add { Events.AddHandler (CancelCommandEvent, value); }
			remove { Events.RemoveHandler (CancelCommandEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridCommandEventHandler DeleteCommand {
			add { Events.AddHandler (DeleteCommandEvent, value); }
			remove { Events.RemoveHandler (DeleteCommandEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridCommandEventHandler EditCommand {
			add { Events.AddHandler (EditCommandEvent, value); }
			remove { Events.RemoveHandler (EditCommandEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridCommandEventHandler ItemCommand {
			add { Events.AddHandler (ItemCommandEvent, value); }
			remove { Events.RemoveHandler (ItemCommandEvent, value); }
			
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridItemEventHandler ItemCreated {
			add { Events.AddHandler (ItemCreatedEvent, value); }
			remove { Events.RemoveHandler (ItemCreatedEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridItemEventHandler ItemDataBound {
			add { Events.AddHandler (ItemDataBoundEvent, value); }
			remove { Events.RemoveHandler (ItemDataBoundEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridPageChangedEventHandler PageIndexChanged {
			add { Events.AddHandler (PageIndexChangedEvent, value); }
			remove { Events.RemoveHandler (PageIndexChangedEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridSortCommandEventHandler SortCommand {
			add { Events.AddHandler (SortCommandEvent, value); }
			remove { Events.RemoveHandler (SortCommandEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridCommandEventHandler UpdateCommand {
			add { Events.AddHandler (UpdateCommandEvent, value); }
			remove { Events.AddHandler (UpdateCommandEvent, value); }
		}
	}
}


