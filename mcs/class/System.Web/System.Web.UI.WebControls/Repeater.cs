//
// System.Web.UI.WebControls.Repeater.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.Util;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Web.UI.WebControls
{
	[DefaultEvent("ItemCommand")]
	[DefaultProperty("DataSource")]
	[Designer ("System.Web.UI.Design.WebControls.RepeaterDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[ParseChildren(true)]
	[PersistChildren(false)]
	public class Repeater : Control, INamingContainer
	{
		private static readonly object ItemCommandEvent   = new object();
		private static readonly object ItemCreatedEvent   = new object();
		private static readonly object ItemDataBoundEvent = new object();

		private static readonly string ITEMCOUNT = "Repeater_Item_Count";

		private ITemplate alternatingItemTemplate;
		private ITemplate footerTemplate;
		private ITemplate headerTemplate;
		private ITemplate itemTemplate;
		private ITemplate separatorTemplate;
		private object    dataSource;

		private RepeaterItemCollection items;
		private ArrayList              itemsArrayList;

		public Repeater(): base()
		{
		}

		[WebCategory ("Action")]
		[WebSysDescription ("Raised when a command is executed in the DataList.")]
		public event RepeaterCommandEventHandler ItemCommand
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

		[WebCategory ("Behavior")]
		[WebSysDescription ("Raised when an item gets created.")]
		public event RepeaterItemEventHandler ItemCreated
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

		[WebCategory ("Behavior")]
		[WebSysDescription ("Raised when an item gets data-bound.")]
		public event RepeaterItemEventHandler ItemDataBound
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

		[DefaultValue (null), Browsable (false), PersistenceMode (PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (RepeaterItem))]
		[WebSysDescription ("The template that is used to create an alternating item.")]
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

		public override ControlCollection Controls
		{
			get
			{
				EnsureChildControls();
				return base.Controls;
			}
		}

		[DefaultValue (""), WebCategory ("Data")]
		[WebSysDescription ("The name of the table that is used for binding when a DataSource is specified.")]
		public virtual string DataMember
		{
			get
			{
				object o = ViewState["DataMember"];
				if(o != null)
				{
					return (string)o;
				}
				return String.Empty;
			}
			set
			{
				ViewState["DataMember"] = value;
			}
		}

		[DefaultValue (null), Bindable (true), WebCategory ("Data")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("The DataSource that is used for data-binding.")]
		public virtual object DataSource
		{
			get
			{
				return dataSource;
			}
			set
			{
				dataSource = value;
			}
		}

		[DefaultValue (null), Browsable (false), PersistenceMode (PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (RepeaterItem))]
		[WebSysDescription ("The template that is used to create a footer.")]
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

		[DefaultValue (null), Browsable (false), PersistenceMode (PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (RepeaterItem))]
		[WebSysDescription ("The template that is used to create a header.")]
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

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("A collection containing all items.")]
		public virtual RepeaterItemCollection Items
		{
			get
			{
				if(items != null)
				{
					if(itemsArrayList != null)
					{
						EnsureChildControls();
					}
					items = new RepeaterItemCollection(itemsArrayList);
				}
				return items;
			}
		}

		[DefaultValue (null), Browsable (false), PersistenceMode (PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (RepeaterItem))]
		[WebSysDescription ("The template that is used to create an item.")]
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

		[DefaultValue (null), Browsable (false), PersistenceMode (PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (RepeaterItem))]
		[WebSysDescription ("The template that is used to create a seperator.")]
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

		public override void DataBind()
		{
			OnDataBinding(EventArgs.Empty);
		}

		protected override void CreateChildControls()
		{
			Controls.Clear();
			if(ViewState[ITEMCOUNT] != null)
			{
				CreateControlHierarchy(false);
			} else
			{
				itemsArrayList = new ArrayList();
			}
			ClearChildViewState();
		}

		private RepeaterItem CreateItem (int itemIndex,
				                 ListItemType itemType,
						 bool dataBind,
						 object dataItem)
		{
			RepeaterItem repeaterItem = new RepeaterItem (itemIndex, itemType);
			RepeaterItemEventArgs repeaterEventArgs = new RepeaterItemEventArgs (repeaterItem);
			InstantiateItem (repeaterItem);
			if (dataBind)
				repeaterItem.DataItem = dataItem;
			OnItemCreated (repeaterEventArgs);
			Controls.Add (repeaterItem);
			if (dataBind) {
				repeaterItem.DataBind ();
				OnItemDataBound (repeaterEventArgs);
				repeaterItem.DataItem = null;
			}
			return repeaterItem;
		}

		private void InstantiateItem (RepeaterItem item)
		{
			ITemplate template;
			switch (item.ItemType) {
				case ListItemType.Header:
					template = this.headerTemplate;
					break;
				case ListItemType.Footer:
					template = this.footerTemplate;
					break;
				case ListItemType.Item:
					template = this.itemTemplate;
					break;
				case ListItemType.AlternatingItem:
					template = this.alternatingItemTemplate;
					break;
				case ListItemType.Separator:
					template = this.separatorTemplate;
					break;
				default:
					throw new HttpException ("Unknown ListItemType: " + item.ItemType);
			}

			template.InstantiateIn (item);
		}

		protected virtual void CreateControlHierarchy (bool useDataSource)
		{
			ArrayList itemList = new ArrayList ();
			items = new RepeaterItemCollection (itemList);
			IEnumerable ds = null;
			if (useDataSource)
				ds = DataSourceHelper.GetResolvedDataSource (DataSource, DataMember);

			if (!useDataSource) {
				int itemCount  = (int) ViewState [ITEMCOUNT];
				if (itemCount != -1) 
					ds = new ArrayList (itemCount);
			}

			if (ds == null)
				return;

			if (headerTemplate != null)
				CreateItem (-1, ListItemType.Header, useDataSource, null);

			bool even = true;
			int index = 0;
			foreach (object item in ds){
				if (separatorTemplate != null && index > 0)
					CreateItem (index - 1, ListItemType.Separator, useDataSource, null);

				RepeaterItem repeaterItem;
				ListItemType lType;
				if (!even && alternatingItemTemplate != null)
					lType = ListItemType.AlternatingItem;
				else
					lType = ListItemType.Item;
					
				repeaterItem = CreateItem (index, lType, useDataSource, item);
				itemList.Add (repeaterItem);
				index++;
				even = !even;
			}

			if (footerTemplate != null)
				CreateItem (-1, ListItemType.Footer, useDataSource, null);
		}

		protected override bool OnBubbleEvent(object sender, EventArgs e)
		{
			bool retVal = false;
			if(e is RepeaterCommandEventArgs)
			{
				OnItemCommand((RepeaterCommandEventArgs)e);
				retVal = true;
			}
			return retVal;
		}

		protected override void OnDataBinding(EventArgs e)
		{
			base.OnDataBinding(e);
			Controls.Clear();
			ClearChildViewState();
			CreateControlHierarchy(true);
			ChildControlsCreated = true;
		}

		protected virtual void OnItemCommand(RepeaterCommandEventArgs e)
		{
			if(Events != null)
			{
				RepeaterCommandEventHandler rceh = (RepeaterCommandEventHandler) 
									Events [ItemCommandEvent];
				if(rceh != null)
				{
					rceh(this, e);
				}
			}
		}

		protected virtual void OnItemCreated(RepeaterItemEventArgs e)
		{
			if(Events != null)
			{
				RepeaterItemEventHandler rceh = (RepeaterItemEventHandler) 
									Events [ItemCreatedEvent];
				if(rceh != null)
				{
					rceh(this, e);
				}
			}
		}

		protected virtual void OnItemDataBound(RepeaterItemEventArgs e)
		{
			if(Events != null)
			{
				RepeaterItemEventHandler rceh = (RepeaterItemEventHandler)
									Events [ItemDataBoundEvent];
				if(rceh != null)
				{
					rceh(this, e);
				}
			}
		}

		protected virtual RepeaterItem CreateItem (int itemIndex, ListItemType itemType)
		{
			return new RepeaterItem (itemIndex, itemType);
		}

		protected virtual void InitializeItem (RepeaterItem item)
		{
			InstantiateItem (item);
		}
	}
}
