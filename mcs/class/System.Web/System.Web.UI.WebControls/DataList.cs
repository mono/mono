/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataList
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  70%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.UI.WebControls
{
	//TODO: [Designer("??")]
	//TODO: [Editor("??")]
	public class DataList: BaseDataList, INamingContainer, IRepeatInfoUser
	{
		public const string CancelCommandName = "Cancel";
		public const string DeleteCommandName = "Delete";
		public const string EditCommandName   = "Edit";
		public const string SelectCommandName = "Select";
		public const string UpdateCommandName = "Update";

		private static readonly object CancelCommandEvent = new object();
		private static readonly object DeleteCommandEvent = new object();
		private static readonly object EditCommandEvent   = new object();
		private static readonly object ItemCommandEvent   = new object();
		private static readonly object ItemCreatedEvent   = new object();
		private static readonly object ItemDataBoundEvent = new object();
		private static readonly object UpdateCommandEvent = new object();

		private TableItemStyle alternatingItemStyle;
		private TableItemStyle editItemStyle;
		private TableItemStyle footerStyle;
		private TableItemStyle headerStyle;
		private TableItemStyle itemStyle;
		private TableItemStyle selectedItemStyle;
		private TableItemStyle separatorStyle;

		private ITemplate alternatingItemTemplate;
		private ITemplate editItemTemplate;
		private ITemplate footerTemplate;
		private ITemplate headerTemplate;
		private ITemplate itemTemplate;
		private ITemplate selectedItemTemplate;
		private ITemplate separatorTemplate;
		private ITemplate separatorItemTemplate;

		private ArrayList itemsArray;
		private DataListItemCollection items;

		private bool extractTemplateRows;

		public DataList(): base()
		{
		}

		public virtual TableItemStyle AlternatingItemStyle
		{
			get
			{
				if(alternatingItemStyle == null)
				{
					alternatingItemStyle = new TableItemStyle();
					if(IsTrackingViewState)
						alternatingItemStyle.TrackViewState();
				}
				return alternatingItemStyle;
			}
		}

		public virtual ITemplate AlternatingItemTemplate
		{
			get
			{
				return alternatingItemTemplate;
			}
			set
			{
				alternatingItemTemplate = value;
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
						editItemStyle.TrackViewState();
				}
				return editItemStyle;
			}
		}

		public virtual ITemplate EditItemTemplate
		{
			get
			{
				return editItemTemplate;
			}
			set
			{
				editItemTemplate = value;
			}
		}

		public virtual bool ExtractTemplateRows
		{
			get
			{
				object o = ViewState["ExtractTemplateRows"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["ExtractTemplateRows"] = value;
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
						footerStyle.TrackViewState();
				}
				return footerStyle;
			}
		}

		public virtual ITemplate FooterTemplate
		{
			get
			{
				return footerTemplate;
			}
			set
			{
				footerTemplate = value;
			}
		}

		public override GridLines GridLines
		{
			get
			{
				return GridLines;
			}
			set
			{
				GridLines = value;
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
						headerStyle.TrackViewState();
				}
				return headerStyle;
			}
		}

		public virtual ITemplate HeaderTemplate
		{
			get
			{
				return headerTemplate;
			}
			set
			{
				headerTemplate = value;
			}
		}

		public virtual DataListItemCollection Items
		{
			get
			{
				if(items == null)
				{
					if(itemsArray == null)
					{
						EnsureChildControls();
						itemsArray = new ArrayList();
					}
					items = new DataListItemCollection(itemsArray);
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
						itemStyle.TrackViewState();
				}
				return itemStyle;
			}
		}

		public virtual ITemplate ItemTemplate
		{
			get
			{
				return itemTemplate;
			}
			set
			{
				itemTemplate = value;
			}
		}

		public virtual int RepeatColumns
		{
			get
			{
				object o = ViewState["RepeatColumns"];
				if(o != null)
					return (int)o;
				return 0;
			}
			set
			{
				if(value < 0)
					throw new ArgumentOutOfRangeException("value");
				ViewState["RepeatColumns"] = value;
			}
		}

		public virtual RepeatDirection RepeatDirection
		{
			get
			{
				object o = ViewState["RepeatDirection"];
				if(o != null)
					return (RepeatDirection)o;
				return RepeatDirection.Vertical;
			}
			set
			{
				if(!Enum.IsDefined(typeof(RepeatDirection), value))
					throw new ArgumentException();
				ViewState["RepeatDirection"] = value;
			}
		}

		public virtual RepeatLayout RepeatLayout
		{
			get
			{
				object o = ViewState["RepeatLayout"];
				if(o != null)
					return (RepeatLayout)o;
				return RepeatLayout.Table;
			}
			set
			{
				if(!Enum.IsDefined(typeof(RepeatLayout), value))
					throw new ArgumentException();
				ViewState["RepeatLayout"] = value;
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
				//FIXME: Looks like a bug in Microsoft's specs.
				// Exception is missing in document. I haven't tested the case
				// But I think exception should follow
				if(value < -1)
					throw new ArgumentOutOfRangeException("value");
				int prevSel = SelectedIndex;
				ViewState["SelectedIndex"] = value;
				DataListItem prevSelItem;
				ListItemType liType;
				if(itemsArray != null)
				{
					if(prevSel >= 0 && prevSel < itemsArray.Count)
					{
						prevSelItem = (DataListItem)itemsArray[prevSel];
						if(prevSelItem.ItemType != ListItemType.EditItem)
						{
							liType = ( (prevSel % 2) == 0 ? ListItemType.AlternatingItem : ListItemType.Item );
							prevSelItem.SetItemType(liType);
						}
					}
					if(value >= 0 && value < itemsArray.Count)
					{
						prevSelItem = (DataListItem) itemsArray[value];
						if(prevSelItem.ItemType != ListItemType.EditItem)
						{
							prevSelItem.SetItemType(ListItemType.SelectedItem);
						}
					}
				}
			}
		}

		public virtual DataListItem SelectedItem
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
						selectedItemStyle.TrackViewState();
				}
				return selectedItemStyle;
			}
		}

		public virtual ITemplate SelectedItemTemplate
		{
			get
			{
				return selectedItemTemplate;
			}
			set
			{
				selectedItemTemplate = value;
			}
		}

		public virtual TableItemStyle SeparatorStyle
		{
			get
			{
				if(separatorStyle == null)
				{
					separatorStyle = new TableItemStyle();
					if(IsTrackingViewState)
						separatorStyle.TrackViewState();
				}
				return separatorStyle;
			}
		}

		public virtual ITemplate SeparatorTemplate
		{
			get
			{
				return separatorTemplate;
			}
			set
			{
				separatorTemplate = value;
			}
		}

		public virtual ITemplate SeparatorItemTemplate
		{
			get
			{
				return separatorItemTemplate;
			}
			set
			{
				separatorItemTemplate = value;
			}
		}

		public virtual bool ShowHeader
		{
			get
			{
				object o = ViewState["ShowHeader"];
				if(o!=null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["ShowHeader"] = value;
			}
		}

		public virtual bool ShowFooter
		{
			get
			{
				object o = ViewState["ShowFooter"];
				if(o!=null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["ShowFooter"] = value;
			}
		}

		public event DataListCommandEventHandler CancelCommand
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

		public event DataListCommandEventHandler DeleteCommand
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

		public event DataListCommandEventHandler EditCommand
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

		public event DataListCommandEventHandler ItemCommand
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

		public event DataListCommandEventHandler ItemCreated
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

		public event DataListCommandEventHandler ItemDataBound
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

		public event DataListCommandEventHandler UpdateCommand
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
			TableStyle retVal = new TableStyle(ViewState);
			retVal.CellSpacing = 0;
			return retVal;
		}

		protected override void LoadViewState(object savedState)
		{
			object[] states;
			if(savedState != null && (states = (object[])savedState) != null)
			{
				if(states[0] != null)
					LoadViewState(states[0]);
				if(states[1] != null)
					alternatingItemStyle.LoadViewState(states[1]);
				if(states[2] != null)
					editItemStyle.LoadViewState(states[2]);
				if(states[3] != null)
					footerStyle.LoadViewState(states[3]);
				if(states[4] != null)
					headerStyle.LoadViewState(states[4]);
				if(states[5] != null)
					itemStyle.LoadViewState(states[5]);
				if(states[6] != null)
					selectedItemStyle.LoadViewState(states[6]);
				if(states[7] != null)
					separatorStyle.LoadViewState(states[7]);
			}
		}
		protected override object SaveViewState()
		{
			object[] states = new object[8];
			states[0] = SaveViewState();
			states[1] = (alternatingItemStyle == null ? null : alternatingItemStyle.SaveViewState());
			states[2] = (editItemStyle == null        ? null : editItemStyle.SaveViewState());
			states[3] = (footerStyle == null          ? null : footerStyle.SaveViewState());
			states[4] = (headerStyle == null          ? null : headerStyle.SaveViewState());
			states[5] = (itemStyle == null            ? null : itemStyle.SaveViewState());
			states[6] = (selectedItemStyle == null    ? null : selectedItemStyle.SaveViewState());
			states[7] = (separatorStyle == null       ? null : separatorStyle.SaveViewState());
			return states;
		}

		protected override void TrackViewState()
		{
			TrackViewState();
			if(alternatingItemStyle != null)
				alternatingItemStyle.TrackViewState();
			if(editItemStyle != null)
				editItemStyle.TrackViewState();
			if(footerStyle != null)
				footerStyle.TrackViewState();
			if(headerStyle != null)
				headerStyle.TrackViewState();
			if(itemStyle != null)
				itemStyle.TrackViewState();
			if(selectedItemStyle != null)
				selectedItemStyle.TrackViewState();
			if(separatorStyle != null)
				separatorStyle.TrackViewState();
		}

		protected override bool OnBubbleEvent(object source, EventArgs e)
		{
			bool retVal = false;
			if(e is DataListCommandEventArgs)
			{
				DataListCommandEventArgs dlcea = (DataListCommandEventArgs)e;
				OnItemCommand(dlcea);
				retVal = true;
				if(String.Compare(dlcea.CommandName, "Cancel") == 0)
				{
					OnCancelCommand(dlcea);
				}
				if(String.Compare(dlcea.CommandName, "Delete") == 0)
				{
					OnDeleteCommand(dlcea);
				}
				if(String.Compare(dlcea.CommandName, "Edit") == 0)
				{
					OnEditCommand(dlcea);
				}
				if(String.Compare(dlcea.CommandName, "Select") == 0)
				{
					SelectedIndex = dlcea.Item.ItemIndex;
					OnSelectedIndexChanged(EventArgs.Empty);
				}
				if(String.Compare(dlcea.CommandName, "Update") == 0)
				{
					OnUpdateCommand(dlcea);
				}
			}
			return retVal;
		}

		protected virtual void OnCancelCommand(DataListCommandEventArgs e)
		{
			if(Events != null)
			{
				DataListCommandEventHandler dlceh = (DataListCommandEventHandler)(Events[CancelCommandEvent]);
				if(dlceh != null)
					dlceh(this, e);
			}
		}

		protected virtual void OnDeleteCommand(DataListCommandEventArgs e)
		{
			if(Events != null)
			{
				DataListCommandEventHandler dlceh = (DataListCommandEventHandler)(Events[DeleteCommandEvent]);
				if(dlceh != null)
					dlceh(this, e);
			}
		}

		protected virtual void OnEditCommand(DataListCommandEventArgs e)
		{
			if(Events != null)
			{
				DataListCommandEventHandler dlceh = (DataListCommandEventHandler)(Events[EditCommandEvent]);
				if(dlceh != null)
					dlceh(this, e);
			}
		}

		protected virtual void OnItemCommand(DataListCommandEventArgs e)
		{
			if(Events != null)
			{
				DataListCommandEventHandler dlceh = (DataListCommandEventHandler)(Events[ItemCommandEvent]);
				if(dlceh != null)
					dlceh(this, e);
			}
		}

		protected virtual void OnItemCreated(DataListItemEventArgs e)
		{
			if(Events != null)
			{
				DataListItemEventHandler dlceh = (DataListItemEventHandler)(Events[ItemCreatedEvent]);
				if(dlceh != null)
					dlceh(this, e);
			}
		}

		protected virtual void OnItemDataBound(DataListItemEventArgs e)
		{
			if(Events != null)
			{
				DataListItemEventHandler dlceh = (DataListItemEventHandler)(Events[ItemDataBoundEvent]);
				if(dlceh != null)
					dlceh(this, e);
			}
		}

		protected virtual void OnUpdateCommand(DataListCommandEventArgs e)
		{
			if(Events != null)
			{
				DataListCommandEventHandler dlceh = (DataListCommandEventHandler)(Events[UpdateCommandEvent]);
				if(dlceh != null)
					dlceh(this, e);
			}
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			if(Controls.Count > 0)
			{
				RepeatInfo repeater       = new RepeatInfo();
				Table      templateTable  = null;
				if(extractTemplateRows)
				{
					repeater.RepeatDirection = RepeatDirection.Vertical;
					repeater.RepeatLayout    = RepeatLayout.Flow;
					repeater.RepeatColumns   = 1;
					repeater.OuterTableImplied = true;
					
					templateTable = new Table();
					templateTable.ID = ClientID;
					templateTable.CopyBaseAttributes(this);
					templateTable.ApplyStyle(ControlStyle);
					templateTable.RenderBeginTag(writer);
				} else
				{
					repeater.RepeatDirection = RepeatDirection;
					repeater.RepeatLayout    = RepeatLayout;
					repeater.RepeatColumns   = RepeatColumns;
				}
				repeater.RenderRepeater(writer, this, ControlStyle, this);
				if(templateTable != null)
				{
					templateTable.RenderEndTag(writer);
				}
			}
		}

		private DataListItem GetItem(ListItemType itemType, int repeatIndex)
		{
			DataListItem retVal = null;
			switch(itemType)
			{
				case ListItemType.Header: retVal = (DataListItem)Controls[0];
				                          break;
				case ListItemType.Footer: retVal = (DataListItem)Controls[Controls.Count - 1];
				                          break;
				case ListItemType.Item:   goto case ListItemType.EditItem;
				case ListItemType.AlternatingItem: goto case ListItemType.EditItem;
				case ListItemType.SelectedItem: goto case ListItemType.EditItem;
				case ListItemType.EditItem: retVal = (DataListItem)Controls[repeatIndex];
				                           break;
				case ListItemType.Separator: int index = 2 * repeatIndex + 1;
				                             if(headerTemplate != null)
				                             	index ++;
				                             retVal = (DataListItem)Controls[index];
				                             break;
			}
			return retVal;
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		protected override void CreateControlHierarchy(bool useDataSource)
		{
			IEnumerable source = null;
			ArrayList          dkeys   = DataKeysArray;
			if(itemsArray != null)
				itemsArray.Clear();
			else
				itemsArray = new ArrayList();
			extractTemplateRows = ExtractTemplateRows;
			if(!useDataSource)
			{
				int count = (int)ViewState["_!ItemCount"];
				if(count != -1)
				{
					source = new DataSourceInternal(count);
					itemsArray.Capacity = count;
				}
			} else
			{
				dkeys.Clear();
				source = DataSourceHelper.GetResolvedDataSource(DataSource, DataMember);
				if(source != null && source is ICollection)
				{
					dkeys.Capacity = ((ICollection)source).Count;
					itemsArray.Capacity = ((ICollection)source).Count;
				}
			}
			IEnumerator listEnumerator = null;
			int         itemCount      = 0;
			if(source != null)
			{
				int index          = 0;
				int editIndex      = EditItemIndex;
				int selIndex       = SelectedIndex;
				string dataKey     = DataKeyField;
				
				bool useDB = (useDataSource ? (dataKey.Length != 0) : false);
				
				if(headerTemplate != null)
				{
					CreateItem(-1, 0, useDataSource, null);
				}
				listEnumerator = source.GetEnumerator();
				try
				{
					while(listEnumerator.MoveNext())
					{
						object current = listEnumerator.Current;
						if(useDB)
						{
							dkeys.Add(DataBinder.GetPropertyValue(current, dataKey));
						}
						ListItemType type = ListItemType.Item;
						if(index == editIndex)
						{
							type = ListItemType.EditItem;
						} else if(index == selIndex)
						{
							type = ListItemType.SelectedItem;
						} else if((index % 2) != 0)
						{
							type = ListItemType.AlternatingItem;
						}
						itemsArray.Add(CreateItem(index, type, useDataSource, current));
						if(separatorTemplate != null)
							CreateItem(index, ListItemType.Separator, useDataSource, null);
						itemCount++;
						index++;
					}
				} finally
				{
					if(listEnumerator is IDisposable)
					{
						((IDisposable)listEnumerator).Dispose();
					}
				}
				if(footerTemplate != null)
					CreateItem(-1, ListItemType.Footer, useDataSource, null);
			}
			if(useDataSource)
			{
				ViewState["_!ItemCount"] = (listEnumerator != null ? itemCount : -1);
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		protected virtual DataListItem CreateItem(int itemIndex, ListItemType itemType)
		{
			return new DataListItem(itemIndex, itemType);
		}

		private DataListItem CreateItem(int itemIndex, ListItemType itemType, bool dataBind, object dataItem)
		{
			DataListItem retVal = CreateItem(itemIndex, itemType);
			DataListItemEventArgs e = new DataListItemEventArgs(retVal);
			InitializeItem(retVal);
			if(dataBind)
			{
				retVal.DataItem = dataItem;
			}
			OnItemCreated(e);
			Controls.Add(retVal);
			if(dataBind)
			{
				retVal.DataBind();
				OnItemDataBound(e);
				retVal.DataItem = null;
			}
			return retVal;
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		[MonoTODO]
		protected override void PrepareControlHierarchy()
		{
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		protected virtual void InitializeItem(DataListItem item)
		{
			ListItemType type = item.ItemType;
			ITemplate    template = itemTemplate;
			switch(type)
			{
				case ListItemType.Header : template = headerTemplate;
				                           break;
				case ListItemType.Footer : template = footerTemplate;
				                           break;
				case ListItemType.AlternatingItem
				                         : if(alternatingItemTemplate != null)
				                           	template = alternatingItemTemplate;
				                           break;
				case ListItemType.SelectedItem
				                         : if(selectedItemTemplate != null)
				                           {
				                           	template = selectedItemTemplate;
				                           	break;
				                           }
				                           if((item.ItemIndex % 2) != 0)
				                           	goto case ListItemType.AlternatingItem;
				                           break;
				case ListItemType.EditItem
				                         : if(editItemTemplate != null)
				                           {
				                           	template = editItemTemplate;
				                           	break;
				                           }
				                           if(item.ItemIndex == SelectedIndex)
				                           	goto case ListItemType.SelectedItem;
				                           if((item.ItemIndex % 2) != 0)
				                           	goto case ListItemType.AlternatingItem;
				                           break;
				case ListItemType.Separator
				                         : template = separatorTemplate;
				                           break;
			}
			if(itemTemplate != null)
				itemTemplate.InstantiateIn(this);
		}

		bool IRepeatInfoUser.HasFooter
		{
			get
			{
				return !(ShowFooter && footerTemplate!=null);
			}
		}

		bool IRepeatInfoUser.HasHeader
		{
			get
			{
				return !(ShowHeader && headerTemplate!=null);
			}
		}

		bool IRepeatInfoUser.HasSeparators
		{
			get
			{
				return (separatorItemTemplate!=null);
			}
		}

		int IRepeatInfoUser.RepeatedItemCount
		{
			get
			{
				if(itemsArray!=null)
					return itemsArray.Count;
				return 0;
			}
		}

		void IRepeatInfoUser.RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
		{
			DataListItem item = GetItem(itemType, repeatIndex);
			if(item!=null)
			{
				item.RenderItem(writer, extractTemplateRows, repeatInfo.RepeatLayout == RepeatLayout.Table);
			}
		}

		Style IRepeatInfoUser.GetItemStyle(ListItemType itemType, int repeatIndex)
		{
			if(GetItem(itemType, repeatIndex)!=null && ControlStyleCreated)
				return ControlStyle;
			return null;
		}
	}
}
