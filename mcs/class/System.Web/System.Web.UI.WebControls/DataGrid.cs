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
using System.Web;
using System.Web.UI;
using System.ComponentModel;

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
				return false;
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
				return false;
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

		public event DataGridCommandEventHandler ItemCreated
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

		public event DataGridCommandEventHandler ItemDataBound
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

		public event DataGridCommandEventHandler PageIndexChanged
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

		public event DataGridCommandEventHandler SortCommand
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
					LoadViewState(states[0]);
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
			states[0] = SaveViewState();
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
			TrackViewState();
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

		[MonoTODO]
		protected override bool OnBubbleEvent(object source, EventArgs e)
		{
			/*
			bool retVal = false;
			if(e is DataGridCommandEventArgs)
			{
				DataGridCommandEventArgs ea = (DataGridCommandEventArgs)e;
				retVal = true;
				OnItemCommand(ea);
				string cmd = ea.CommandName;
				if(cmd == "Select")
				{
					SelectedIndex = ea.Item.SelectedIndex;
					OnSelectedIndexChanged(EventArgs.Empty);
				} else if(cmd == "Page")
				{
					throw new NotImplementedException();
					// Next; Prev; Sort etc
				}
			}
			*/
			throw new NotImplementedException();
			//return retVal;
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

		[MonoTODO]
		protected override void PrepareControlHierarchy()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void CreateControlHierarchy(bool useDataSource)
		{
			throw new NotImplementedException();
		}

		internal void OnColumnsChanged()
		{
		}

		internal void OnPagerChanged()
		{
		}
	}
}
