/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataGrid
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  95%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Collections;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.Util;
using System.ComponentModel;
using System.Reflection;

namespace System.Web.UI.WebControls
{
	//TODO: [Designer("??")]
	//TODO: [Editor("??")]
	[DefaultEvent("SelectedIndexChanged")]
	[DefaultProperty("DataSource")]
	[ParseChildren(true)]
	[PersistChildren(false)]
	public class DataGrid : BaseDataList, INamingContainer
	{
		public const string CancelCommandName       = "Cancel";
		public const string DeleteCommandName       = "Delete";
		public const string EditCommandName         = "Edit";
		public const string NextPageCommandArgument = "Next";
		public const string PageCommandName         = "Page";
		public const string PrevPageCommandArgument = "Prev";
		public const string SelectCommandName       = "Select";
		public const string SortCommandName         = "Sort";
		public const string UpdateCommandName       = "Update";

		private TableItemStyle alternatingItemStyle;
		private TableItemStyle editItemStyle;
		private TableItemStyle headerStyle;
		private TableItemStyle footerStyle;
		private TableItemStyle itemStyle;
		private TableItemStyle selectedItemStyle;
		private DataGridPagerStyle pagerStyle;

		private DataGridColumnCollection columns;
		private ArrayList                columnsArrayList;
		private DataGridItemCollection   items;
		private ArrayList                itemsArrayList;
		private PagedDataSource          pagedDataSource;

		private ArrayList   autoGenColsArrayList;
		private IEnumerator storedData;
		private object      storedDataFirst;
		private bool        storedDataValid;

		private static readonly object CancelCommandEvent    = new object();
		private static readonly object DeleteCommandEvent    = new object();
		private static readonly object EditCommandEvent      = new object();
		private static readonly object ItemCommandEvent      = new object();
		private static readonly object ItemCreatedEvent      = new object();
		private static readonly object ItemDataBoundEvent    = new object();
		private static readonly object PageIndexChangedEvent = new object();
		private static readonly object SortCommandEvent      = new object();
		private static readonly object UpdateCommandEvent    = new object();

		public DataGrid(): base()
		{
		}

		public virtual bool AllowCustomPaging
		{
			get
			{
				object o = ViewState["AllowCustomPaging"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["AllowCustomPaging"] = value;
			}
		}

		public virtual bool AllowPaging
		{
			get
			{
				object o = ViewState["AllowPaging"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["AllowPaging"] = value;
			}
		}

		public virtual bool AllowSorting
		{
			get
			{
				object o = ViewState["AllowSorting"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["AllowSorting"] = value;
			}
		}

		public virtual TableItemStyle AlternatingItemStyle
		{
			get
			{
				if(alternatingItemStyle == null)
				{
					alternatingItemStyle = new TableItemStyle();
				}
				if(IsTrackingViewState)
				{
					alternatingItemStyle.TrackViewState();
				}
				return alternatingItemStyle;
			}
		}

		public virtual bool AutoGenerateColumns
		{
			get
			{
				object o = ViewState["AutoGenerateColumns"];
				if(o != null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["AutoGenerateColumns"] = value;
			}
		}

		public virtual string BackImageUrl
		{
			get
			{
				object o = ViewState["BackImageUrl"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["BackImageUrl"] = value;
			}
		}

		public virtual DataGridColumnCollection Columns
		{
			get
			{
				if(columns == null)
				{
					columnsArrayList = new ArrayList();
					columns = new DataGridColumnCollection(this, columnsArrayList);
					if(IsTrackingViewState)
					{
						((IStateManager)columns).TrackViewState();
					}
				}
				return columns;
			}
		}

		public int CurrentPageIndex
		{
			get
			{
				object o = ViewState["CurrentPageIndex"];
				if(o != null)
					return (int)o;
				return 0;
			}
			set
			{
				if(value < 0)
					throw new ArgumentOutOfRangeException();
				ViewState["CurrentPageIndex"] = value;
			}
		}

		public virtual int EditItemIndex
		{
			get
			{
				object o = ViewState["EditItemIndex"];
				if(o != null)
					return (int)o;
				return -1;
			}
			set
			{
				if(value < -1)
					throw new ArgumentOutOfRangeException();
				ViewState["EditItemIndex"] = value;
			}
		}

		public virtual TableItemStyle EditItemStyle
		{
			get
			{
				if(editItemStyle == null)
				{
					editItemStyle = new TableItemStyle();
					if(IsTrackingViewState)
					{
						editItemStyle.TrackViewState();
					}
				}
				return editItemStyle;
			}
		}

		public virtual TableItemStyle FooterStyle
		{
			get
			{
				if(footerStyle == null)
				{
					footerStyle = new TableItemStyle();
					if(IsTrackingViewState)
					{
						footerStyle.TrackViewState();
					}
				}
				return footerStyle;
			}
		}

		public virtual TableItemStyle HeaderStyle
		{
			get
			{
				if(headerStyle == null)
				{
					headerStyle = new TableItemStyle();
					if(IsTrackingViewState)
					{
						headerStyle.TrackViewState();
					}
				}
				return headerStyle;
			}
		}

		public virtual DataGridItemCollection Items
		{
			get
			{
				if(items == null)
				{
					if(itemsArrayList == null)
						EnsureChildControls();
					if(itemsArrayList == null)
					{
						itemsArrayList = new ArrayList();
					}
					items = new DataGridItemCollection(itemsArrayList);
				}
				return items;
			}
		}

		public virtual TableItemStyle ItemStyle
		{
			get
			{
				if(itemStyle == null)
				{
					itemStyle = new TableItemStyle();
					if(IsTrackingViewState)
					{
						itemStyle.TrackViewState();
					}
				}
				return itemStyle;
			}
		}

		public int PageCount
		{
			get
			{
				if(pagedDataSource != null)
				{
					return pagedDataSource.PageCount;
				}
				object o = ViewState["PageCount"];
				if(o != null)
					return (int)o;
				return 0;
			}
		}

		public virtual DataGridPagerStyle PagerStyle
		{
			get
			{
				if(pagerStyle == null)
				{
					pagerStyle = new DataGridPagerStyle(this);
					if(IsTrackingViewState)
					{
						pagerStyle.TrackViewState();
					}
				}
				return pagerStyle;
			}
		}

		public virtual int PageSize
		{
			get
			{
				object o = ViewState["PageSize"];
				if(o != null)
					return (int)o;
				return 10;
			}
			set
			{
				if(value < 1)
					throw new ArgumentOutOfRangeException();
				ViewState["PageSize"] = value;
			}
		}

		public virtual int SelectedIndex
		{
			get
			{
				object o = ViewState["SelectedIndex"];
				if(o != null)
					return (int)o;
				return -1;
			}
			set
			{
				if(value < -1)
					throw new ArgumentOutOfRangeException();
				int prevVal = SelectedIndex;
				ViewState["SelectedIndex"] = value;
				if(items != null)
				{
					if(prevVal !=-1 && prevVal < items.Count)
					{
						DataGridItem prev = (DataGridItem)items[prevVal];
						if(prev.ItemType != ListItemType.EditItem)
						{
							ListItemType newType = ListItemType.Item;
							if( (prevVal % 2) != 0)
							{
								newType = ListItemType.AlternatingItem;
							}
							prev.SetItemType(newType);
						}
					}
				}
			}
		}

		public virtual DataGridItem SelectedItem
		{
			get
			{
				if(SelectedIndex == -1)
					return null;
				return Items[SelectedIndex];
			}
		}

		public virtual TableItemStyle SelectedItemStyle
		{
			get
			{
				if(selectedItemStyle == null)
				{
					selectedItemStyle = new TableItemStyle();
					if(IsTrackingViewState)
					{
						selectedItemStyle.TrackViewState();
					}
				}
				return selectedItemStyle;
			}
		}

		public virtual bool ShowFooter
		{
			get
			{
				object o = ViewState["ShowFooter"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["ShowFooter"] = value;
			}
		}

		public virtual bool ShowHeader
		{
			get
			{
				object o = ViewState["ShowHeader"];
				if(o != null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["ShowHeader"] = value;
			}
		}

		public virtual int VirtualItemCount
		{
			get
			{
				object o = ViewState["VirtualItemCount"];
				if(o != null)
					return (int)o;
				return 0;
			}
			set
			{
				if(value < 0)
					throw new ArgumentOutOfRangeException();
				ViewState["VirtualItemCount"] = value;
			}
		}

		public event DataGridCommandEventHandler CancelCommand
		{
			add
			{
				Events.AddHandler(CancelCommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(CancelCommandEvent, value);
			}
		}

		public event DataGridCommandEventHandler DeleteCommand
		{
			add
			{
				Events.AddHandler(DeleteCommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(DeleteCommandEvent, value);
			}
		}

		public event DataGridCommandEventHandler EditCommand
		{
			add
			{
				Events.AddHandler(EditCommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(EditCommandEvent, value);
			}
		}

		public event DataGridCommandEventHandler ItemCommand
		{
			add
			{
				Events.AddHandler(ItemCommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ItemCommandEvent, value);
			}
		}

		public event DataGridItemEventHandler ItemCreated
		{
			add
			{
				Events.AddHandler(ItemCreatedEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ItemCreatedEvent, value);
			}
		}

		public event DataGridItemEventHandler ItemDataBound
		{
			add
			{
				Events.AddHandler(ItemDataBoundEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ItemDataBoundEvent, value);
			}
		}

		public event DataGridPageChangedEventHandler PageIndexChanged
		{
			add
			{
				Events.AddHandler(PageIndexChangedEvent, value);
			}
			remove
			{
				Events.RemoveHandler(PageIndexChangedEvent, value);
			}
		}

		public event DataGridSortCommandEventHandler SortCommand
		{
			add
			{
				Events.AddHandler(SortCommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(SortCommandEvent, value);
			}
		}

		public event DataGridCommandEventHandler UpdateCommand
		{
			add
			{
				Events.AddHandler(UpdateCommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(UpdateCommandEvent, value);
			}
		}

		protected override Style CreateControlStyle()
		{
			TableStyle style = new TableStyle(ViewState);
			style.GridLines = GridLines.Both;
			style.CellSpacing = 0;
			return style;
		}

		protected override void LoadViewState(object savedState)
		{
			if(savedState != null)
			{
				object[] states = (object[])savedState;
				if(states != null)
				{
					base.LoadViewState(states[0]);
					if(columns != null)
						((IStateManager)columns).LoadViewState(states[1]);
					if(pagerStyle != null)
						pagerStyle.LoadViewState(states[2]);
					if(headerStyle != null)
						headerStyle.LoadViewState(states[3]);
					if(footerStyle != null)
						footerStyle.LoadViewState(states[4]);
					if(itemStyle != null)
						itemStyle.LoadViewState(states[5]);
					if(alternatingItemStyle != null)
						alternatingItemStyle.LoadViewState(states[6]);
					if(selectedItemStyle != null)
						selectedItemStyle.LoadViewState(states[7]);
					if(editItemStyle != null)
						editItemStyle.LoadViewState(states[8]);
				}
			}
		}

		protected override object SaveViewState()
		{
			object[] states = new object[9];
			states[0] = base.SaveViewState();
			states[1] = (columns == null ? null : ((IStateManager)columns).SaveViewState());
			states[2] = (pagerStyle == null ? null : pagerStyle.SaveViewState());
			states[3] = (headerStyle == null ? null : headerStyle.SaveViewState());
			states[4] = (footerStyle == null ? null : footerStyle.SaveViewState());
			states[5] = (itemStyle == null ? null : itemStyle.SaveViewState());
			states[6] = (alternatingItemStyle == null ? null : alternatingItemStyle.SaveViewState());
			states[7] = (selectedItemStyle == null ? null : selectedItemStyle.SaveViewState());
			states[8] = (editItemStyle == null ? null : editItemStyle.SaveViewState());
			return states;
		}

		protected override void TrackViewState()
		{
			base.TrackViewState();
			if(alternatingItemStyle != null)
			{
				alternatingItemStyle.TrackViewState();
			}
			if(editItemStyle != null)
			{
				editItemStyle.TrackViewState();
			}
			if(headerStyle != null)
			{
				headerStyle.TrackViewState();
			}
			if(footerStyle != null)
			{
				footerStyle.TrackViewState();
			}
			if(itemStyle != null)
			{
				itemStyle.TrackViewState();
			}
			if(selectedItemStyle != null)
			{
				selectedItemStyle.TrackViewState();
			}
			if(pagerStyle != null)
			{
				pagerStyle.TrackViewState();
			}

			if(columns != null)
			{
				((IStateManager)columns).TrackViewState();
			}
		}

		protected override bool OnBubbleEvent(object source, EventArgs e)
		{
			bool retVal = false;
			if(e is DataGridCommandEventArgs)
			{
				DataGridCommandEventArgs ea = (DataGridCommandEventArgs)e;
				retVal = true;
				OnItemCommand(ea);
				string cmd = ea.CommandName;
				if(String.Compare(cmd, "select", true) == 0)
				{
					SelectedIndex = ea.Item.ItemIndex;
					OnSelectedIndexChanged(EventArgs.Empty);
				} else if(String.Compare(cmd,"page", true) == 0)
				{
					int    cIndex = CurrentPageIndex;
					string cea = (string) ea.CommandArgument;
					if(String.Compare(cea, "prev", true) == 0)
					{
						cIndex--;
					} else if(String.Compare(cea, "next", true) == 0)
					{
						cIndex++;
					}
					OnPageIndexChanged(new DataGridPageChangedEventArgs(source, cIndex));
				} else if(String.Compare(cmd, "sort", true) == 0)
				{
					OnSortCommand(new DataGridSortCommandEventArgs(source, ea));
				} else if(String.Compare(cmd, "edit", true) == 0)
				{
					OnEditCommand(ea);
				} else if(String.Compare(cmd, "update", true) == 0)
				{
					OnUpdateCommand(ea);
				} else if(String.Compare(cmd, "cancel", true) == 0)
				{
					OnCancelCommand(ea);
				} else if(String.Compare(cmd, "delete", true) == 0)
				{
					OnDeleteCommand(ea);
				}
			}
			return retVal;
		}

		protected virtual void OnCancelCommand(DataGridCommandEventArgs e)
		{
			if(Events != null)
			{
				DataGridCommandEventHandler dceh = (DataGridCommandEventHandler)(Events[CancelCommandEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnDeleteCommand(DataGridCommandEventArgs e)
		{
			if(Events != null)
			{
				DataGridCommandEventHandler dceh = (DataGridCommandEventHandler)(Events[DeleteCommandEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnEditCommand(DataGridCommandEventArgs e)
		{
			if(Events != null)
			{
				DataGridCommandEventHandler dceh = (DataGridCommandEventHandler)(Events[EditCommandEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnItemCommand(DataGridCommandEventArgs e)
		{
			if(Events != null)
			{
				DataGridCommandEventHandler dceh = (DataGridCommandEventHandler)(Events[ItemCommandEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnItemCreated(DataGridItemEventArgs e)
		{
			if(Events != null)
			{
				DataGridItemEventHandler dceh = (DataGridItemEventHandler)(Events[ItemCreatedEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnItemDataBound(DataGridItemEventArgs e)
		{
			if(Events != null)
			{
				DataGridItemEventHandler dceh = (DataGridItemEventHandler)(Events[ItemDataBoundEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnPageIndexChanged(DataGridPageChangedEventArgs e)
		{
			if(Events != null)
			{
				DataGridPageChangedEventHandler dceh = (DataGridPageChangedEventHandler)(Events[PageIndexChangedEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnSortCommand(DataGridSortCommandEventArgs e)
		{
			if(Events != null)
			{
				DataGridSortCommandEventHandler dceh = (DataGridSortCommandEventHandler)(Events[SortCommandEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnUpdateCommand(DataGridCommandEventArgs e)
		{
			if(Events != null)
			{
				DataGridCommandEventHandler dceh = (DataGridCommandEventHandler)(Events[UpdateCommandEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected override void PrepareControlHierarchy()
		{
			if (Controls.Count == 0)
				return;

			Table display = (Table) Controls [0];
			display.CopyBaseAttributes (this);
			if (ControlStyleCreated) {
				display.ApplyStyle (ControlStyle);
			} else {
				display.GridLines   = GridLines.Both;
				display.CellSpacing = 0;
			}

			TableRowCollection rows = display.Rows;
			if (rows.Count == 0)
				return;

			int nCols = Columns.Count;
			DataGridColumn [] cols = new DataGridColumn [nCols];
			Style deployStyle = null;

			if (nCols > 0)
				Columns.CopyTo (cols, 0);

			if (alternatingItemStyle != null) {
				deployStyle = new TableItemStyle ();
				deployStyle.CopyFrom (itemStyle);
				deployStyle.CopyFrom (alternatingItemStyle);
			} else {
				deployStyle = itemStyle;
			}

			int nrows = rows.Count;
			for (int counter = 0; counter < nrows; counter++)
				PrepareControlHierarchyForItem (cols,
								(DataGridItem) rows [counter],
								counter,
								deployStyle);
		}

		private void PrepareControlHierarchyForItem (DataGridColumn [] cols,
							     DataGridItem item,
							     int index,
							     Style deployStyle)
		{
			switch (item.ItemType) {
			case ListItemType.Header:
				if (!ShowHeader) {
					item.Visible = false;
					break;
				}

				if (headerStyle != null)
					item.MergeStyle (headerStyle);

				goto case ListItemType.Separator;
			case ListItemType.Footer:
				if (!ShowFooter) {
					item.Visible = false;
					break;
				}

				if (footerStyle != null)
					item.MergeStyle (footerStyle);

				goto case ListItemType.Separator;
			case ListItemType.Item  :
				item.MergeStyle (itemStyle);
				goto case ListItemType.Separator;
			case ListItemType.AlternatingItem:
				item.MergeStyle (deployStyle);
				goto case ListItemType.Separator;
			case ListItemType.SelectedItem:
				Style selStyle = new TableItemStyle ();
				if ((item.ItemIndex % 2) == 0) {
					selStyle.CopyFrom (itemStyle);
				} else {
					selStyle.CopyFrom (deployStyle);
				}

				selStyle.CopyFrom (selectedItemStyle);
				item.MergeStyle (selStyle);
				goto case ListItemType.Separator;
			case ListItemType.EditItem:
				Style edStyle = new TableItemStyle ();
				if ((item.ItemIndex % 2) == 0) {
					edStyle.CopyFrom (itemStyle);
				} else {
					edStyle.CopyFrom (deployStyle);
				}

				edStyle.CopyFrom (editItemStyle);
				item.MergeStyle (edStyle);
				goto case ListItemType.Separator;
			case ListItemType.Pager:
				if (pagerStyle == null)
					break;

				if (!pagerStyle.Visible)
					item.Visible = false;

				if (index == 0) {
					if (!pagerStyle.IsPagerOnTop) {
						item.Visible = false;
						break;
					}
				} else if (!pagerStyle.IsPagerOnBottom) {
					item.Visible = false;
					break;
				}

				item.MergeStyle (pagerStyle);
				goto case ListItemType.Separator;
			case ListItemType.Separator:
				TableCellCollection cells = item.Cells;
				int cellCount = cells.Count;
				if (cellCount > cols.Length)
					cellCount = cols.Length;

				if (cellCount > 0 && item.ItemType != ListItemType.Pager) {
					for (int i = 0; i < cellCount; i++) {
						Style colStyle = null;
						if (cols [i].Visible) {
							switch (item.ItemType) {
							case ListItemType.Header:
								colStyle = cols [i].HeaderStyleInternal;
								break;
							case ListItemType.Footer:
								colStyle = cols [i].FooterStyleInternal;
								break;
							}
							item.MergeStyle (colStyle);
						} else {
							cells [i].Visible = false;
						}
					}
				}
				break;
			default:
				goto case ListItemType.Separator;
			}
		}

		protected override void CreateControlHierarchy(bool useDataSource)
		{
			IEnumerator pageSourceEnumerator;
			int         itemCount;
			ArrayList   dataKeys;
			ArrayList   columns;
			IEnumerable resolvedDS;
			ICollection collResolvedDS;
			int         pageDSCount;
			int         colCount;
			DataGridColumn[] cols;
			Table       deployTable;
			TableRowCollection deployRows;
			ListItemType deployType;
			int         indexCounter;
			string      dkField;
			bool        dsUse;
			bool        pgEnabled;
			int         editIndex;
			int         selIndex;

			pagedDataSource = CreatePagedDataSource();
			pageSourceEnumerator  = null;
			itemCount       = -1;
			dataKeys        = DataKeysArray;
			columns         = null;
			if(itemsArrayList != null)
			{
				itemsArrayList.Clear();
			} else
			{
				itemsArrayList = new ArrayList();
			}
			if(!useDataSource)
			{
				itemCount    = (int) ViewState["_!ItemCount"];
				pageDSCount  = (int) ViewState["_!DataSource_ItemCount"];
				if(itemCount != -1)
				{
					if(pagedDataSource.IsCustomPagingEnabled)
					{
						pagedDataSource.DataSource = new DataSourceInternal(itemCount);
					} else
					{
						pagedDataSource.DataSource = new DataSourceInternal(pageDSCount);
					}
					pageSourceEnumerator = pagedDataSource.GetEnumerator();
					columns              = CreateColumnSet(null, false);
					itemsArrayList.Capacity = itemCount;
				}
			} else
			{
				dataKeys.Clear();
				resolvedDS = DataSourceHelper.GetResolvedDataSource(DataSource, DataMember);
				if(resolvedDS != null)
				{
					collResolvedDS = resolvedDS as ICollection;
					if(pagedDataSource.IsPagingEnabled && !pagedDataSource.IsCustomPagingEnabled
					   && collResolvedDS == null)
					{
						throw new HttpException(HttpRuntime.FormatResourceString("DataGrid_Missing_VirtualItemCount", ID));
					}
					pagedDataSource.DataSource = resolvedDS;
					if(pagedDataSource.IsPagingEnabled && (pagedDataSource.CurrentPageIndex < 0 ||
					                       pagedDataSource.CurrentPageIndex >= pagedDataSource.PageCount))
					{
						throw new HttpException(HttpRuntime.FormatResourceString("DataGrid_Invalid_Current_PageIndex", ID));
					}
					columns = CreateColumnSet(pagedDataSource, useDataSource);

					if(storedDataValid)
					{
						pageSourceEnumerator = storedData;
					} else
					{
						pageSourceEnumerator = pagedDataSource.GetEnumerator();
					}
					if(collResolvedDS != null)
					{
						pageDSCount         = pagedDataSource.Count;
						dataKeys.Capacity   = pageDSCount;
						itemsArrayList.Capacity = pageDSCount;
					}
				}
			}

			colCount = 0;
			if(columns != null)
				colCount = columns.Count;
			int currentSourceIndex;
			if(colCount > 0)
			{
				cols = (DataGridColumn []) columns.ToArray (typeof (DataGridColumn));
				foreach(DataGridColumn current in cols)
				{
					current.Initialize();
				}
				deployTable = new DataGridTableInternal();
				Controls.Add(deployTable);
				deployRows = deployTable.Rows;

				indexCounter = 0;
				currentSourceIndex  = 0;
				dkField = DataKeyField;

				dsUse = (useDataSource) ? (dkField.Length > 0) : false;
				pgEnabled = pagedDataSource.IsPagingEnabled;
				editIndex = EditItemIndex;
				selIndex  = SelectedIndex;
				if(pgEnabled)
				{
					currentSourceIndex = pagedDataSource.FirstIndexInPage;
					CreateItem(-1, -1, ListItemType.Pager, false, null,
						   cols, deployRows, pagedDataSource);
				}
				itemCount = 0;
				CreateItem(-1, -1, ListItemType.Header, useDataSource, null,
					   cols, deployRows, null);
				
				if(storedDataValid && storedDataFirst != null)
				{
					if(dsUse)
					{
						dataKeys.Add(DataBinder.GetPropertyValue(storedDataFirst, dkField));
					}
					if (indexCounter == editIndex) {
						deployType = ListItemType.EditItem;
					} else if (indexCounter == selIndex) {
						deployType = ListItemType.SelectedItem;
					} else {
						deployType = ListItemType.Item;
					}

					itemsArrayList.Add(CreateItem(0, currentSourceIndex, deployType,
								      useDataSource, storedDataFirst,
								      cols, deployRows, null));
					itemCount++;
					indexCounter++;
					currentSourceIndex++;
					storedDataValid = false;
					storedDataFirst = null;
				}

				while(pageSourceEnumerator.MoveNext())
				{
					object current = pageSourceEnumerator.Current;
					if(dsUse)
					{
						dataKeys.Add(DataBinder.GetPropertyValue(current, dkField));
					}

					if (indexCounter == editIndex) {
						deployType = ListItemType.EditItem;
					} else if (indexCounter == selIndex) {
						deployType = ListItemType.SelectedItem;
					} else if ((indexCounter % 2) == 1) {
						deployType = ListItemType.AlternatingItem;
					} else {
						deployType = ListItemType.Item;
					}

					itemsArrayList.Add(CreateItem(indexCounter, currentSourceIndex,
								      deployType, useDataSource, current,
								      cols, deployRows, null));
					itemCount++;
					indexCounter++;
					currentSourceIndex++;
				}

				CreateItem(-1, -1, ListItemType.Footer, useDataSource, null,
					   cols, deployRows, null);

				if(pgEnabled)
				{
					CreateItem(-1, -1, ListItemType.Pager, false, null, cols, deployRows,
						   pagedDataSource);
				}
			}

			if(useDataSource)
			{
				if(pageSourceEnumerator != null)
				{
					ViewState["_!ItemCount"] = itemCount;
					if(pagedDataSource.IsPagingEnabled)
					{
						ViewState["PageCount"] = pagedDataSource.PageCount;
						ViewState["_!DataSource_ItemCount"] = pagedDataSource.DataSourceCount;
					} else
					{
						ViewState["PageCount"] = 1;
						ViewState["_!DataSource_ItemCount"] = itemCount;
					}
				} else
				{
					ViewState["_!ItemCount"] = -1;
					ViewState["_!DataSource_ItemCount"] = -1;
					ViewState["PageCount"] = 0;
				}
			}
			pagedDataSource = null;
		}

		private DataGridItem CreateItem(int itemIndex, int dsIndex, ListItemType type,
		                                bool bind, object item, DataGridColumn[] columns,
		                                TableRowCollection rows, PagedDataSource dataSrc)

		{
			DataGridItem retVal;
			DataGridItemEventArgs args;

			retVal = CreateItem(itemIndex, dsIndex, type);
			args = new DataGridItemEventArgs(retVal);

			if(type != ListItemType.Pager)
			{
				InitializeItem(retVal, columns);
				if(bind)
				{
					retVal.DataItem = item;
				}
				OnItemCreated(args);
				rows.Add(retVal);
				if(bind)
				{
					retVal.DataBind();
					OnItemDataBound(args);
					retVal.DataItem = null;
				}
			} else
			{
				InitializePager(retVal, columns.Length, dataSrc);
				OnItemCreated(args);
				rows.Add(retVal);
			}
			return retVal;
		}

		protected virtual DataGridItem CreateItem(int itemIndex, int dataSourceIndex, ListItemType itemType)
		{
			return new DataGridItem(itemIndex, dataSourceIndex, itemType);
		}

		protected virtual void InitializeItem(DataGridItem item, DataGridColumn[] columns)
		{
			TableCellCollection cells = item.Cells;
			TableCell cCell;
			
			for(int i = 0; i < columns.Length; i++)
			{
				cCell = new TableCell();
				columns[i].InitializeCell(cCell, i, item.ItemType);
				cells.Add(cCell);
			}
		}

		protected virtual void InitializePager(DataGridItem item,
		                       int columnSpan, PagedDataSource pagedDataSource)
		{
			TableCell toAdd = new TableCell();
			toAdd.ColumnSpan = columnSpan;
			
			if(PagerStyle.Mode == PagerMode.NextPrev)
			{
				if(!pagedDataSource.IsFirstPage)
				{
					LinkButton link = new DataGridLinkButton();
					link.Text = PagerStyle.PrevPageText;
					link.CommandName = "Page";
					link.CommandArgument = "Prev";
					link.CausesValidation = false;
					toAdd.Controls.Add(link);
				} else
				{
					Label label = new Label();
					label.Text = PagerStyle.PrevPageText;
					toAdd.Controls.Add(label);
				}
				toAdd.Controls.Add(new LiteralControl("&nbsp;"));
				if(!pagedDataSource.IsLastPage)
				{
					LinkButton link = new DataGridLinkButton();
					link.Text = PagerStyle.NextPageText;
					link.CommandName = "Page";
					link.CommandArgument = "Next";
					link.CausesValidation = false;
					toAdd.Controls.Add(link);
				} else
				{
					Label label = new Label();
					label.Text = PagerStyle.NextPageText;
					toAdd.Controls.Add(label);
				}
			} else
			{
				int pageCount = pagedDataSource.PageCount;
				int currPage  = pagedDataSource.CurrentPageIndex + 1;
				int btnCount  = PagerStyle.PageButtonCount;
				int numberOfPages = btnCount;
				if(numberOfPages > pageCount)
					numberOfPages = pageCount;
				int firstPageNumber = 1; // 10
				int lastPageNumber  = numberOfPages; // 11
				if(currPage > lastPageNumber)
				{
					firstPageNumber = (pagedDataSource.CurrentPageIndex / btnCount) * btnCount + 1;
					lastPageNumber  = firstPageNumber + btnCount - 1;
					if(lastPageNumber > pageCount)
						lastPageNumber = pageCount;
					if((lastPageNumber - firstPageNumber + 1) < btnCount)
						firstPageNumber = Math.Max(1, lastPageNumber - btnCount + 1);
				}
				if(firstPageNumber != 1)
				{
					LinkButton toAddBtn = new DataGridLinkButton();
					toAddBtn.Text = "...";
					toAddBtn.CommandName = "Page";
					toAddBtn.CommandArgument = (lastPageNumber - 1).ToString(NumberFormatInfo.InvariantInfo);
					toAddBtn.CausesValidation = false;
					toAdd.Controls.Add(toAddBtn);
					toAdd.Controls.Add(new LiteralControl("&nbsp;"));
				}
				for(int i = firstPageNumber; i <= lastPageNumber; i++)
				{
					string argText = i.ToString(NumberFormatInfo.InvariantInfo);
					if(i == currPage)
					{
						Label cPageLabel = new Label();
						cPageLabel.Text = argText;
						toAdd.Controls.Add(cPageLabel);
					} else
					{
						LinkButton indexButton = new DataGridLinkButton();
						indexButton.Text = argText;
						indexButton.CommandName = "Page";
						indexButton.CommandArgument = argText;
						indexButton.CausesValidation = false;
						toAdd.Controls.Add(indexButton);
					}
					if(i < lastPageNumber)
						toAdd.Controls.Add(new LiteralControl("&nbsp;"));
				}
				if(pageCount > lastPageNumber)
				{
					toAdd.Controls.Add(new LiteralControl("&nbsp;"));
					LinkButton contLink = new DataGridLinkButton();
					contLink.Text = "...";
					contLink.CommandName = "Page";
					contLink.CommandArgument = (lastPageNumber + 1).ToString(NumberFormatInfo.InvariantInfo);
					contLink.CausesValidation = false;
					toAdd.Controls.Add(contLink);
				}
			}
			item.Cells.Add(toAdd);
		}

		private PagedDataSource CreatePagedDataSource()
		{
			PagedDataSource retVal;

			retVal = new PagedDataSource();
			retVal.CurrentPageIndex = CurrentPageIndex;
			retVal.PageSize         = PageSize;
			retVal.AllowPaging      = AllowPaging;
			retVal.AllowCustomPaging = AllowCustomPaging;
			retVal.VirtualCount      = VirtualItemCount;

			return retVal;
		}

		///<summary>
		/// UnDocumented method
		/// </summary>
		protected ArrayList CreateColumnSet(PagedDataSource source, bool useDataSource)
		{
			DataGridColumn[] cols = new DataGridColumn [Columns.Count];
			Columns.CopyTo (cols, 0);
			ArrayList l_columns = new ArrayList ();

			foreach (DataGridColumn current in cols)
				l_columns.Add (current);

			if (AutoGenerateColumns) {
				ArrayList auto_columns = null;
				if (useDataSource) {
					auto_columns = AutoCreateColumns (source);
					autoGenColsArrayList = auto_columns;
				} else {
					auto_columns = autoGenColsArrayList;
				}

				if (auto_columns != null && auto_columns.Count > 0)
					l_columns.AddRange (auto_columns);
			}

			return l_columns;
		}

		/// <summary>
		/// Generates the columns when AutoGenerateColumns is true.
		/// This method is called by CreateColumnSet when dataSource
		/// is to be used and columns need to be generated automatically.
		/// </summary>
		private ArrayList AutoCreateColumns(PagedDataSource source)
		{
			if(source != null)
			{
				ArrayList retVal = new ArrayList();
				PropertyDescriptorCollection props = source.GetItemProperties(new PropertyDescriptor[0]);
				Type      prop_type;
				BoundColumn b_col;
				if(props == null)
				{
					prop_type   = null;
					PropertyInfo prop_item =  source.DataSource.GetType().GetProperty("Item",
					          BindingFlags.Instance | BindingFlags.Static |
					          BindingFlags.Public, null, null,
					          new Type[] { typeof(int) }, null);
					
					if(prop_item != null)
					{
						prop_type = prop_item.PropertyType;
					}
					if(prop_type != null && prop_type == typeof(object))
					{
						object fitem = null;
						IEnumerator en = source.GetEnumerator();
						if(en.MoveNext())
							fitem = en.Current;
						if(fitem != null)
						{
							prop_type = fitem.GetType();
						}
						StoreEnumerator(en, fitem);
						if(fitem != null && fitem is ICustomTypeDescriptor)
						{
							props = TypeDescriptor.GetProperties(fitem);
						} else if(prop_type != null)
						{
							if(IsBindableType(prop_type))
							{
								b_col = new BoundColumn();
								// b_col.TrackViewState();
								b_col.HeaderText = "Item";
								b_col.SortExpression = "Item";
								b_col.DataField  = BoundColumn.thisExpr;
								b_col.SetOwner(this);
								retVal.Add(b_col);
							} else
							{
								props = TypeDescriptor.GetProperties(prop_type);
							}
						}
					}
				}
				if(props != null && props.Count > 0)
				{
					//IEnumerable p_en = props.GetEnumerator();
					try
					{
						foreach(PropertyDescriptor current in props)
						{
							if(IsBindableType(current.PropertyType))
							{
								b_col = new BoundColumn();
								// b_col.TrackViewState();
								b_col.HeaderText     = current.Name;
								b_col.SortExpression = current.Name;
								b_col.DataField      = current.Name;
								// b_col.IsReadOnly     = current.IsReadOnly;
								b_col.SetOwner(this);
								retVal.Add(b_col);
							}
						}
					} finally
					{
						if(props is IDisposable)
							((IDisposable)props).Dispose();
					}
				}
				if(retVal.Count > 0)
				{
					return retVal;
				}
				throw new HttpException(HttpRuntime.FormatResourceString("DataGrid_NoAutoGenColumns", ID));
			}
			return null;
		}

		internal void StoreEnumerator(IEnumerator source, object firstItem)
		{
			storedData      = source;
			storedDataFirst = firstItem;
			storedDataValid = true;
		}

		internal void OnColumnsChanged()
		{
		}

		internal void OnPagerChanged()
		{
		}
	}
}
