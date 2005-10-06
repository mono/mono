//
// System.Web.UI.WebControls.Repeater.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sanjay Gupta (gsanjay@novell.com)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
// (C) 2004 Novell, Inc. (http://www.novell.com)
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

#if NET_2_0
		bool inited;
		IDataSource currentSource;
		DataSourceSelectArguments selectArguments = null;
		bool requiresDataBinding;
#endif

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
				if ((value!=null) && !(value is IListSource) && !(value is IEnumerable))
					throw new ArgumentException ("An invalid data source is being used for " +
						ID + ". A valid data source must implement either " +
						"IListSource or IEnumerable.");
				
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
				if(items == null)
				{
					if(itemsArrayList == null)
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

		public override void DataBind ()
		{
			#if NET_2_0
			RequiresDataBinding = false;
			#endif
			OnDataBinding (EventArgs.Empty);
		}

		protected override void CreateChildControls()
		{
			Controls.Clear ();
			if (ViewState[ITEMCOUNT] != null) {
				CreateControlHierarchy (false);
			} else {
				itemsArrayList = new ArrayList ();
				ClearChildViewState ();
			}
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
					template = (alternatingItemTemplate != null ? this.alternatingItemTemplate : itemTemplate);
					break;
				case ListItemType.Separator:
					template = this.separatorTemplate;
					break;
				default:
					throw new HttpException ("Unknown ListItemType: " + item.ItemType);
			}

			if (template != null)
				template.InstantiateIn (item);
		}

		protected virtual void CreateControlHierarchy (bool useDataSource)
		{
			if (itemsArrayList != null) {
				itemsArrayList.Clear ();
			} else {
				itemsArrayList = new ArrayList ();
			}

			IEnumerable ds = null;
			if (useDataSource) {
				ds = GetResolvedDataSource ();
			} else {
				int itemCount  = (int) ViewState [ITEMCOUNT];
				if (itemCount != -1)
					ds = new DataSourceInternal (itemCount);
			}

			int index = 0;
			if (ds != null) {
				if (headerTemplate != null)
					CreateItem (-1, ListItemType.Header, useDataSource, null);
				
				bool even = true;
				foreach (object item in ds){
					if (separatorTemplate != null && index > 0)
						CreateItem (index - 1, ListItemType.Separator,
							    useDataSource, null);

					RepeaterItem repeaterItem;
					ListItemType lType;
					if (!even)
						lType = ListItemType.AlternatingItem;
					else
						lType = ListItemType.Item;

					repeaterItem = CreateItem (index, lType, useDataSource, item);
					itemsArrayList.Add (repeaterItem);
					index++;
					even = !even;
				}
				
				if (footerTemplate != null)
					CreateItem (-1, ListItemType.Footer, useDataSource, null);
			}

			if (useDataSource)
				ViewState [ITEMCOUNT] = (ds == null) ? -1 : index;
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
		
		#if NET_2_0

			protected override void OnInit (EventArgs e)
			{
				base.OnInit(e);
				Page.PreLoad += new EventHandler (OnPagePreLoad);
			}
			
			void OnPagePreLoad (object sender, EventArgs e)
			{
				SubscribeSourceChangeEvent ();
				inited = true;
			}
		
			void SubscribeSourceChangeEvent ()
			{
				IDataSource ds = GetDataSource ();
				
				if (currentSource != ds) {
					currentSource.DataSourceChanged -= new EventHandler (OnDataSourceViewChanged);
					currentSource = ds;
				}
					
				if (ds != null)
					ds.DataSourceChanged += new EventHandler (OnDataSourceViewChanged);
			}
			
			protected override void OnLoad (EventArgs e)
			{
				if (IsBoundUsingDataSourceID && (!Page.IsPostBack || !EnableViewState))
					RequiresDataBinding = true;
	
				base.OnLoad(e);
			}
			
			protected override void OnPreRender (EventArgs e)
			{
				EnsureDataBound ();
				base.OnPreRender (e);
			}
				
			protected bool IsBoundUsingDataSourceID {
				get { return DataSourceID.Length > 0; }
			}
			
			protected void EnsureDataBound ()
			{
				if (RequiresDataBinding && IsBoundUsingDataSourceID)
					DataBind ();
			}
			
			IDataSource GetDataSource ()
			{
				if (IsBoundUsingDataSourceID) {
					Control ctrl = NamingContainer.FindControl (DataSourceID);
					if (ctrl == null)
						throw new HttpException (string.Format ("A control with ID '{0}' could not be found.", DataSourceID));
					if (!(ctrl is IDataSource))
						throw new HttpException (string.Format ("The control with ID '{0}' is not a control of type IDataSource.", DataSourceID));
					return (IDataSource) ctrl;
				}
				return DataSource as IDataSource;
			}
			
			protected IEnumerable GetData ()
			{
				if (DataSource != null && IsBoundUsingDataSourceID)
					throw new HttpException ("Control bound using both DataSourceID and DataSource properties.");
				
				IDataSource ds = GetDataSource ();
				if (ds != null)
					return ds.GetView (DataMember).ExecuteSelect (SelectArguments);
				
				IEnumerable ie = DataSourceHelper.GetResolvedDataSource (DataSource, DataMember);
				if (ie != null) return ie;
				
				throw new HttpException (string.Format ("Unexpected data source type: {0}", DataSource.GetType()));
			}
			
			protected virtual void OnDataSourceViewChanged (object sender, EventArgs e)
			{
				RequiresDataBinding = true;
			}

			protected virtual void OnDataPropertyChanged ()
			{
				RequiresDataBinding = true;
				SubscribeSourceChangeEvent ();
			}

			[DefaultValueAttribute ("")]
			[IDReferencePropertyAttribute (typeof(System.Web.UI.DataSourceControl))]
			[ThemeableAttribute (false)]
			public virtual string DataSourceID {
				get {
					object o = ViewState ["DataSourceID"];
					if (o != null)
						return (string)o;
					
					return String.Empty;
				}
				set {
					ViewState ["DataSourceID"] = value;
					if (inited) OnDataPropertyChanged ();
				}
			}
			
			protected bool Initialized {
				get { return inited; }
			}
			
			protected bool RequiresDataBinding {
				get { return requiresDataBinding; }
				set { requiresDataBinding = value; }
			}
			
			protected virtual DataSourceSelectArguments CreateDataSourceSelectArguments ()
			{
				return DataSourceSelectArguments.Empty;
			}
			
			protected DataSourceSelectArguments SelectArguments {
				get {
					if (selectArguments == null)
						selectArguments = CreateDataSourceSelectArguments ();
					return selectArguments;
				}
			}
			
			internal IEnumerable GetResolvedDataSource ()
			{
				return GetData ();
			}
				
		#else
			IEnumerable GetResolvedDataSource ()
			{
				if (DataSource != null)
					return DataSourceHelper.GetResolvedDataSource (DataSource, DataMember);
				else
					return null; 
			}
		#endif
	}
}
