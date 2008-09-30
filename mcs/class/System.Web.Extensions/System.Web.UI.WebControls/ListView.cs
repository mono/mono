//
// System.Web.UI.WebControls.ListView
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007-2008 Novell, Inc
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace System.Web.UI.WebControls
{
	[DefaultEventAttribute ("SelectedIndexChanged")]
	[ControlValuePropertyAttribute ("SelectedValue")]
	[DesignerAttribute ("System.Web.UI.Design.WebControls.ListViewDesigner, System.Web.Extensions.Design, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
	[SupportsEventValidationAttribute ()]
	[ToolboxBitmapAttribute (typeof (ListView), "ListView.ico")]
	[DefaultPropertyAttribute ("SelectedValue")]
	public class ListView : DataBoundControl, INamingContainer, IPageableItemContainer
	{
		ITemplate _emptyDataTemplate;
		ITemplate _emptyItemTemplate;
		ITemplate _insertItemTemplate;
		ITemplate _groupSeparatorTemplate;
		ITemplate _groupTemplate;
		ITemplate _itemSeparatorTemplate;
		ITemplate _itemTemplate;
		ITemplate _selectedItemTemplate;
		ITemplate _alternatingItemTemplate;
		ITemplate _editItemTemplate;
		ITemplate _layoutTemplate;

		int _startRowIndex;
		int _maximumRows;
		int _selectedIndex;
		int _editIndex;
		int _groupItemCount;

		string [] _dataKeyNames;
		DataKeyArray _dataKeys;
		ArrayList _dataKeyArray;
		SortDirection _sortDirection = SortDirection.Ascending;
		string _sortExpression = String.Empty;

		Control _layoutTemplatePlaceholder;
		Control _nonGroupedItemsContainer;
		int _nonGroupedItemsContainerFirstItemIndex;
		int _nonGroupedItemsContainerItemCount;
		IOrderedDictionary _lastInsertValues;

#region Events
		// Event keys
		static readonly object ItemCancellingEvent = new object ();
		static readonly object ItemCommandEvent = new object ();
		static readonly object ItemCreatedEvent = new object ();
		static readonly object ItemDataBoundEvent = new object ();
		static readonly object ItemDeletedEvent = new object ();
		static readonly object ItemDeletingEvent = new object ();
		static readonly object ItemEditingEvent = new object ();
		static readonly object ItemInsertedEvent = new object ();
		static readonly object ItemInsertingEvent = new object ();
		static readonly object ItemUpdatedEvent = new object ();
		static readonly object ItemUpdatingEvent = new object ();
		static readonly object LayoutCreatedEvent = new object ();
		static readonly object PagePropertiesChangedEvent = new object ();
		static readonly object PagePropertiesChangingEvent = new object ();
		static readonly object SelectedIndexChangedEvent = new object ();
		static readonly object SelectedIndexChangingEvent = new object ();
		static readonly object SortedEvent = new object ();
		static readonly object SortingEvent = new object ();
		static readonly object TotalRowCountAvailableEvent = new object ();
		
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewCancelEventArgs> ItemCanceling {
			add { Events.AddHandler (ItemCancellingEvent, value); }
			remove { Events.RemoveHandler (ItemCancellingEvent, value); }
		}
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewCommandEventArgs> ItemCommand {
			add { Events.AddHandler (ItemCommandEvent, value); }
			remove { Events.RemoveHandler (ItemCommandEvent, value); }
		}
	
		[CategoryAttribute ("Behavior")]
		public event EventHandler <ListViewItemEventArgs> ItemCreated {
			add { Events.AddHandler (ItemCreatedEvent, value); }
			remove { Events.RemoveHandler (ItemCreatedEvent, value); }
		}
	
		[CategoryAttribute ("Data")]
		public event EventHandler <ListViewItemEventArgs> ItemDataBound {
			add { Events.AddHandler (ItemDataBoundEvent, value); }
			remove { Events.RemoveHandler (ItemDataBoundEvent, value); }
		}
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewDeletedEventArgs> ItemDeleted {
			add { Events.AddHandler (ItemDeletedEvent, value); }
			remove { Events.RemoveHandler (ItemDeletedEvent, value); }
		}
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewDeleteEventArgs> ItemDeleting {
			add { Events.AddHandler (ItemDeletingEvent, value); }
			remove { Events.RemoveHandler (ItemDeletingEvent, value); }
		}
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewEditEventArgs> ItemEditing {
			add { Events.AddHandler (ItemEditingEvent, value); }
			remove { Events.RemoveHandler (ItemEditingEvent, value); }
		}
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewInsertedEventArgs> ItemInserted {
			add { Events.AddHandler (ItemInsertedEvent, value); }
			remove { Events.RemoveHandler (ItemInsertedEvent, value); }
		}
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewInsertEventArgs> ItemInserting {
			add { Events.AddHandler (ItemInsertingEvent, value); }
			remove { Events.RemoveHandler (ItemInsertingEvent, value); }
		}
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewUpdatedEventArgs> ItemUpdated {
			add { Events.AddHandler (ItemUpdatedEvent, value); }
			remove { Events.RemoveHandler (ItemUpdatedEvent, value); }
		}
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewUpdateEventArgs> ItemUpdating {
			add { Events.AddHandler (ItemUpdatingEvent, value); }
			remove { Events.RemoveHandler (ItemUpdatingEvent, value); }
		}
	
		[CategoryAttribute ("Behavior")]
		public event EventHandler LayoutCreated {
			add { Events.AddHandler (LayoutCreatedEvent, value); }
			remove { Events.RemoveHandler (LayoutCreatedEvent, value); }
		}
	
		[CategoryAttribute ("Behavior")]
		public event EventHandler PagePropertiesChanged {
			add { Events.AddHandler (PagePropertiesChangedEvent, value); }
			remove { Events.RemoveHandler (PagePropertiesChangedEvent, value); }
		}
	
		[CategoryAttribute ("Behavior")]
		public event EventHandler <PagePropertiesChangingEventArgs> PagePropertiesChanging {
			add { Events.AddHandler (PagePropertiesChangingEvent, value); }
			remove { Events.RemoveHandler (PagePropertiesChangingEvent, value); }
		}
	
		[CategoryAttribute ("Action")]
		public event EventHandler SelectedIndexChanged {
			add { Events.AddHandler (SelectedIndexChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedIndexChangedEvent, value); }
		}
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewSelectEventArgs> SelectedIndexChanging {
			add { Events.AddHandler (SelectedIndexChangingEvent, value); }
			remove { Events.RemoveHandler (SelectedIndexChangingEvent, value); }
		}
	
		[CategoryAttribute ("Action")]
		public event EventHandler Sorted {
			add { Events.AddHandler (SortedEvent, value); }
			remove { Events.RemoveHandler (SortedEvent, value); }
		}
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewSortEventArgs> Sorting {
			add { Events.AddHandler (SortingEvent, value); }
			remove { Events.RemoveHandler (SortingEvent, value); }
		}
	
		event EventHandler <PageEventArgs> IPageableItemContainer.TotalRowCountAvailable {
			add { Events.AddHandler (TotalRowCountAvailableEvent, value); }
			remove { Events.RemoveHandler (TotalRowCountAvailableEvent, value); }
		}
#endregion

#region Properties
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[BrowsableAttribute (false)]
		public override string AccessKey {
			get { return base.AccessKey; }
			set { throw StylingNotSupported (); }
		}
	
		[TemplateContainerAttribute (typeof (ListViewDataItem), BindingDirection.TwoWay)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValue (null)]
		[BrowsableAttribute (false)]
		public virtual ITemplate AlternatingItemTemplate {
			get { return _alternatingItemTemplate; }
			set { _alternatingItemTemplate = value; }
		}
	
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[BrowsableAttribute (false)]
		public override Color BackColor {
			get { return base.BackColor; }
			set { throw StylingNotSupported (); }
		}
	
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		public override Color BorderColor {
			get { return base.BorderColor; }
			set { throw StylingNotSupported (); }
		}
	
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[BrowsableAttribute (false)]
		public override BorderStyle BorderStyle {
			get { return base.BorderStyle; }
			set { throw StylingNotSupported (); }
		}
	
		[BrowsableAttribute (false)]
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public override Unit BorderWidth {
			get { return base.BorderWidth; }
			set { throw StylingNotSupported (); }
		}
	
		public override ControlCollection Controls {
			get {
				EnsureChildControls ();
				return base.Controls;
			}
		}
	
		[CategoryAttribute ("Behavior")]
		[DefaultValue (true)]
		public virtual bool ConvertEmptyStringToNull {
			get {
				object o = ViewState ["ConvertEmptyStringToNull"];
				if (o != null)
					return (bool) o;

				return true;
			}
			
			set { ViewState ["ConvertEmptyStringToNull"] = value; }
		}
	
		public override string CssClass {
			get { return base.CssClass; }
			set { throw StylingNotSupported (); }
		}
	
		[DefaultValue (null)]
		[TypeConverterAttribute (typeof (StringArrayConverter))]
		[CategoryAttribute ("Data")]
		public virtual string [] DataKeyNames {
			get {
				if (_dataKeyNames != null)
					return _dataKeyNames;

				return new string [0];
			}
			set {
				if (value == null)
					_dataKeyNames = null;
				else
					_dataKeyNames = (string []) value.Clone ();

				// They will eventually be recreated while creating the child controls
				_dataKeyArray = null;
				_dataKeys = null;
				
				if (Initialized)
					RequiresDataBinding = true;
			}
		}
	
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public virtual DataKeyArray DataKeys {
			get {
				if (_dataKeys == null) {
					_dataKeys = new DataKeyArray (DataKeyArray);
					if (IsTrackingViewState)
						((IStateManager) _dataKeys).TrackViewState ();
				}

				return _dataKeys;
			}
		}

		ArrayList DataKeyArray {
			get {
				if (_dataKeyArray == null)
					_dataKeyArray = new ArrayList ();

				return _dataKeyArray;
			}
		}
		
		[DefaultValue (-1)]
		[CategoryAttribute ("Misc")]
		public virtual int EditIndex {
			get { return _editIndex; }
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value");

				if (value != _editIndex) {
					_editIndex = value;
					if (Initialized)
						RequiresDataBinding = true;
				}
			}
		}
	
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public virtual ListViewItem EditItem {
			get { throw new NotImplementedException (); }
		}
	
		[DefaultValue (null)]
		[BrowsableAttribute (false)]
		[TemplateContainerAttribute (typeof (ListViewDataItem), BindingDirection.TwoWay)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public virtual ITemplate EditItemTemplate {
			get { return _editItemTemplate; }
			set { _editItemTemplate = value; }
		}
	
		[DefaultValue (null)]
		[TemplateContainerAttribute (typeof (ListView))]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[BrowsableAttribute (false)]
		public virtual ITemplate EmptyDataTemplate {
			get { return _emptyDataTemplate; }
			set { _emptyDataTemplate = value; }
		}
	
		[TemplateContainerAttribute (typeof (ListViewItem))]
		[DefaultValue (null)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[BrowsableAttribute (false)]
		public virtual ITemplate EmptyItemTemplate {
			get { return _emptyItemTemplate; }
			set { _emptyItemTemplate = value; }
		}
	
		[BrowsableAttribute (false)]
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public override FontInfo Font {
			get { throw StylingNotSupported (); }
		}
	
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[BrowsableAttribute (false)]
		public override Color ForeColor {
			get { return base.ForeColor; }
			set { throw StylingNotSupported (); }
		}
	
		[CategoryAttribute ("Misc")]
		[DefaultValue (1)]
		public virtual int GroupItemCount {
			get { return _groupItemCount; }

			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException ("value");

				if (value != _groupItemCount) {
					_groupItemCount = value;
					if (Initialized)
						RequiresDataBinding = true;
				}
			}
		}
	
		[CategoryAttribute ("Behavior")]
		[DefaultValue ("groupPlaceholder")]
		public virtual string GroupPlaceholderID {
			get {
				string s = ViewState ["GroupPlaceholderID"] as string;
				if (s != null)
					return s;

				return "groupPlaceHolder";
			}
			
			set {
				if (String.IsNullOrEmpty (value))
					throw new ArgumentOutOfRangeException ("value");

				ViewState ["GroupPlaceholderID"] = value;
			}
		}
	
		[BrowsableAttribute (false)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValue (null)]
		[TemplateContainerAttribute (typeof (ListViewItem))]
		public virtual ITemplate GroupSeparatorTemplate {
			get { return _groupSeparatorTemplate; }
			set { _groupSeparatorTemplate = value; }
		}
	
		[TemplateContainerAttribute (typeof (ListViewItem))]
		[DefaultValue (null)]
		[BrowsableAttribute (false)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public virtual ITemplate GroupTemplate {
			get { return _groupTemplate; }
			set { _groupTemplate = value; }
		}
	
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public override Unit Height {
			get { return base.Height; }
			set { throw StylingNotSupported (); }
		}
	
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual ListViewItem InsertItem {
			get;
			private set;
		}
	
		[CategoryAttribute ("Misc")]
		[DefaultValue (InsertItemPosition.None)]
		public virtual InsertItemPosition InsertItemPosition {
			get;
			set;
		}
	
		[TemplateContainerAttribute (typeof (ListViewItem), BindingDirection.TwoWay)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValue (null)]
		[BrowsableAttribute (false)]
		public virtual ITemplate InsertItemTemplate {
			get { return _insertItemTemplate; }
			set { _insertItemTemplate = value; }
		}
	
		[DefaultValue ("itemPlaceholder")]
		[CategoryAttribute ("Behavior")]
		public virtual string ItemPlaceholderID {
			get {
				string s = ViewState ["ItemPlaceHolderID"] as string;
				if (s != null)
					return s;

				return "itemPlaceholder";
			}
			
			set {
				if (String.IsNullOrEmpty (value))
					throw new ArgumentOutOfRangeException ("value");
				
				ViewState ["ItemPlaceHolderID"] = value;
			}
		}
	
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual IList <ListViewDataItem> Items {
			get { throw new NotImplementedException (); }
		}
	
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[BrowsableAttribute (false)]
		[TemplateContainerAttribute (typeof (ListViewItem))]
		[DefaultValue (null)]
		public virtual ITemplate ItemSeparatorTemplate {
			get { return _itemSeparatorTemplate; }
			set { _itemSeparatorTemplate = value; }
		}
	
		[TemplateContainerAttribute (typeof (ListViewDataItem), BindingDirection.TwoWay)]
		[DefaultValue (null)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[BrowsableAttribute (false)]
		public virtual ITemplate ItemTemplate {
			get { return _itemTemplate; }
			set { _itemTemplate = value; }
		}
	
		[TemplateContainerAttribute (typeof (ListView))]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValue (null)]
		[BrowsableAttribute (false)]
		public virtual ITemplate LayoutTemplate {
			get { return _layoutTemplate; }
			set { _layoutTemplate = value; }
		}
	
		protected virtual int MaximumRows {
			get { return _maximumRows; }
		}
	
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public virtual DataKey SelectedDataKey {
			get {
				if (_dataKeyNames == null || _dataKeyNames.Length == 0)
					throw new InvalidOperationException ("No data keys are specified in the DataKeyNames property.");

				DataKeyArray dataKeys = DataKeys;
				int selIndex = SelectedIndex;
				if (selIndex > -1 || selIndex < dataKeys.Count)
					return dataKeys [selIndex];

				return null;
			}
		}
	
		[CategoryAttribute ("Misc")]
		[DefaultValue (-1)]
		public virtual int SelectedIndex {
			get { return _selectedIndex; }
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value");

				if (value != _selectedIndex) {
					_selectedIndex = value;
					if (Initialized)
						RequiresDataBinding = true;
				}
			}
		}
	
		[BrowsableAttribute (false)]
		[DefaultValue (null)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute (typeof (ListViewDataItem), BindingDirection.TwoWay)]
		public virtual ITemplate SelectedItemTemplate {
			get { return _selectedItemTemplate; }
			set { _selectedItemTemplate = value; }
		}
	
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public object SelectedValue {
			get {
				DataKey dk = SelectedDataKey;
				if (dk != null)
					return dk.Value;

				return null;
			}
		}
	
		[DefaultValue (SortDirection.Ascending)]
		[BrowsableAttribute (false)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public virtual SortDirection SortDirection {
			get { return _sortDirection; }
		}
	
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public virtual string SortExpression {
			get { return _sortExpression; }
		}
	
		protected virtual int StartRowIndex {
			get { return _startRowIndex; }
		}
	
		int IPageableItemContainer.MaximumRows {
			get { return _maximumRows; }
		}
	
		int IPageableItemContainer.StartRowIndex {
			get { return _startRowIndex; }
		}
	
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public override short TabIndex {
			get { return 0; }
			set { throw new NotSupportedException ("ListView does not allow setting this property."); }
		}
	
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public override string ToolTip {
			get { return base.ToolTip; }
			set { throw StylingNotSupported (); }
		}
	
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public override Unit Width {
			get { return base.Width; }
			set { throw StylingNotSupported (); }
		}
#endregion
		
		public ListView ()
		{
			InsertItemPosition = InsertItemPosition.None;
		}
		
		protected virtual void AddControlToContainer (Control control, Control container, int addLocation)
		{
			if (control == null || container == null)
				return;

			Control ctl;

			if (container is HtmlTable) {
				ctl = new ListViewTableRow ();
				ctl.Controls.Add (control);
			} else
				ctl = control;
			
			container.Controls.AddAt (addLocation, ctl);
		}
	
		protected internal override void CreateChildControls ()
		{
			if (RequiresDataBinding)
				EnsureDataBound ();
			
			base.CreateChildControls ();
		}
	
		protected virtual int CreateChildControls (IEnumerable dataSource, bool dataBinding)
		{
			IList <ListViewDataItem> retList = null;

			EnsureLayoutTemplate ();
			RemoveItems ();

			bool haveDataToDisplay = _maximumRows > 0 && _startRowIndex > 0;
			var pagedDataSource = new ListViewPagedDataSource ();
			
			if (dataBinding) {
				DataSourceView view = GetData ();
				if (view == null)
					throw new InvalidOperationException ("dataSource returned a null reference for DataSourceView.");

				int totalRowCount = 0;
				if (haveDataToDisplay) {
					if (view.CanRetrieveTotalRowCount)
						totalRowCount = SelectArguments.TotalRowCount;
					else {
						ICollection ds = dataSource as ICollection;
						if (ds == null)
							throw new InvalidOperationException ("dataSource does not implement the ICollection interface.");
						totalRowCount = ds.Count + _startRowIndex;
					}
				}
				
				pagedDataSource.StartRowIndex = _startRowIndex;
				pagedDataSource.DataSource = dataSource;
				pagedDataSource.TotalRowCount = totalRowCount;
			} else {
				if (!(dataSource is ICollection))
					throw new InvalidOperationException ("dataSource does not implement the ICollection interface and dataBinding is false.");
			}

			if (GroupItemCount <= 0)
				retList = CreateItemsWithoutGroups (pagedDataSource, dataBinding, InsertItemPosition, DataKeyArray);
			
			if (retList == null)
				return 0;

			return retList.Count;
		}
	
		protected override Style CreateControlStyle ()
		{
			throw StylingNotSupported ();
		}
	
		protected virtual ListViewDataItem CreateDataItem (int dataItemIndex, int displayIndex)
		{
			return new ListViewDataItem (dataItemIndex, displayIndex);
		}
	
		protected override DataSourceSelectArguments CreateDataSourceSelectArguments ()
		{
			DataSourceSelectArguments arg = DataSourceSelectArguments.Empty;
			DataSourceView view = GetData();
			
			if (view.CanPage) {
				arg.StartRowIndex = _startRowIndex;
				if (view.CanRetrieveTotalRowCount) {
					arg.RetrieveTotalRowCount = true;
					arg.MaximumRows = _maximumRows;
				} else
					arg.MaximumRows = -1;
			}

			if (IsBoundUsingDataSourceID && !String.IsNullOrEmpty (_sortExpression)) {
				if (_sortDirection == SortDirection.Ascending)
					arg.SortExpression = _sortExpression;
				else
					arg.SortExpression = _sortExpression + " DESC";
			}
			
			return arg;
		}
	
		protected virtual void CreateEmptyDataItem ()
		{
			if (_emptyDataTemplate != null) {
				ListViewItem item = CreateItem (ListViewItemType.EmptyItem);
				InstantiateEmptyDataTemplate (item);
				OnItemCreated (new ListViewItemEventArgs (item));
				AddControlToContainer (item, this, 0);
			}
		}
	
		protected virtual ListViewItem CreateEmptyItem ()
		{
			if (_emptyDataTemplate != null) {
				ListViewItem item = CreateItem (ListViewItemType.EmptyItem);
				InstantiateEmptyItemTemplate (item);
				OnItemCreated (new ListViewItemEventArgs (item));
				return item;
			}

			return null;
		}
	
		protected virtual ListViewItem CreateInsertItem ()
		{
			ListViewItem ret = CreateItem (ListViewItemType.InsertItem);
			InsertItem = ret;

			return ret;
		}
	
		protected virtual ListViewItem CreateItem (ListViewItemType itemType)
		{
			return new ListViewItem (itemType);
		}
	
		protected virtual IList <ListViewDataItem> CreateItemsInGroups (ListViewPagedDataSource dataSource, bool dataBinding, InsertItemPosition insertPosition,
										ArrayList keyArray)
		{
			if (_groupTemplate == null)
				return null;
			
			throw new NotImplementedException ();
		}
	
		protected virtual IList <ListViewDataItem> CreateItemsWithoutGroups (ListViewPagedDataSource dataSource, bool dataBinding,
										     InsertItemPosition insertPosition, ArrayList keyArray)
		{
			_nonGroupedItemsContainer = FindPlaceholder (ItemPlaceholderID, _layoutTemplatePlaceholder);
			_nonGroupedItemsContainerItemCount = 0;
			
			if (_nonGroupedItemsContainer == null)
				throw new InvalidOperationException (
					String.Format ("An item placeholder must be specified on ListView '{0}'. Specify an item placeholder by setting a control's ID property to \"itemPlaceholder\". The item placeholder control must also specify runat=\"server\".", ID));

			Control parent = _nonGroupedItemsContainer.Parent;
			int ipos = 0;
			
			if (parent != null) {
				ipos = parent.Controls.IndexOf (_nonGroupedItemsContainer);
				parent.Controls.Remove (_nonGroupedItemsContainer);
				_nonGroupedItemsContainer = parent;
				if (_nonGroupedItemsContainer != _layoutTemplatePlaceholder)
					AddControlToContainer (_nonGroupedItemsContainer, _layoutTemplatePlaceholder, 0);
			}
			_nonGroupedItemsContainerFirstItemIndex = ipos;
			
			List <ListViewDataItem> ret = new List <ListViewDataItem> ();
			ListViewItem lvi;
			ListViewItem container;
			bool needSeparator = false;

			if (insertPosition == InsertItemPosition.FirstItem) {
				lvi = CreateInsertItem ();
				InstantiateInsertItemTemplate (lvi);
				AddControlToContainer (lvi, _nonGroupedItemsContainer, ipos++);
				_nonGroupedItemsContainerItemCount++;
				needSeparator = true;
			}

			bool haveSeparatorTemplate = _itemSeparatorTemplate != null;
			int displayIndex = 0;
			ListViewDataItem lvdi;
			int startIndex = dataSource.StartRowIndex;

			foreach (object item in dataSource) {
				if (needSeparator && haveSeparatorTemplate) {
					container = new ListViewItem ();
					InstantiateItemSeparatorTemplate (container);
					AddControlToContainer (container, _nonGroupedItemsContainer, ipos++);
					_nonGroupedItemsContainerItemCount++;
				}

				lvdi = CreateDataItem (startIndex + displayIndex, displayIndex);
				InstantiateItemTemplate (lvdi, displayIndex);

				if (dataBinding) {
					lvdi.DataItem = item;

					OrderedDictionary dict = new OrderedDictionary ();
					string[] dataKeyNames = DataKeyNames;
					
					foreach (string s in dataKeyNames)
						dict.Add (s, DataBinder.GetPropertyValue (item, s));
					
					DataKey dk = new DataKey (dict, dataKeyNames);
					if (keyArray.Count == displayIndex)
						keyArray.Add (dk);
					else
						keyArray [displayIndex] = dk;
				}
				
				OnItemCreated (new ListViewItemEventArgs (lvdi));
				AddControlToContainer (lvdi, _nonGroupedItemsContainer, ipos++);
				_nonGroupedItemsContainerItemCount++;
				
				if (!needSeparator)
					needSeparator = true;

				if (dataBinding) {
					lvdi.DataBind ();
					OnItemDataBound (new ListViewItemEventArgs (lvdi));
				}
				displayIndex++;

				ret.Add (lvdi);
			}

			if (insertPosition == InsertItemPosition.LastItem) {
				if (needSeparator && haveSeparatorTemplate) {
					container = new ListViewItem ();
					InstantiateItemSeparatorTemplate (container);
					AddControlToContainer (container, _nonGroupedItemsContainer, ipos++);
					_nonGroupedItemsContainerItemCount++;
				}
				
				lvi = CreateInsertItem ();
				InstantiateInsertItemTemplate (lvi);
				AddControlToContainer (lvi, _nonGroupedItemsContainer, ipos++);
				_nonGroupedItemsContainerItemCount++;
			}
			
			return ret;
		}
	
		protected virtual void CreateLayoutTemplate ()
		{
			if (_layoutTemplate != null) {
				_layoutTemplatePlaceholder = new Control ();
				_layoutTemplate.InstantiateIn (_layoutTemplatePlaceholder);
				AddControlToContainer (_layoutTemplatePlaceholder, this, 0);
			}
			
			OnLayoutCreated (EventArgs.Empty);
		}
	
		public virtual void DeleteItem (int itemIndex)
		{
		}
	
		protected virtual void EnsureLayoutTemplate ()
		{
			Controls.Clear ();
			CreateLayoutTemplate ();
		}
	
		public virtual void ExtractItemValues (IOrderedDictionary itemValues, ListViewItem item, bool includePrimaryKey)
		{
			if (itemValues == null)
				throw new ArgumentNullException ("itemValues");
			
			if (!(item is ListViewDataItem))
				throw new InvalidOperationException ("item is not a ListViewDataItem object.");
		}
	
		protected virtual Control FindPlaceholder (string containerID, Control container)
		{
			if (container == null || String.IsNullOrEmpty (containerID))
				return null;

			return container.FindControl (containerID);
		}
	
		public virtual void InsertNewItem (bool causesValidation)
		{
			ListViewItem insertItem = InsertItem;

			if (insertItem == null)
				throw new InvalidOperationException ("The ListView control does not have an insert item.");

			DataSourceView dsv = null;
			ListViewInsertEventArgs eventArgs = null;
			
			if (IsBoundUsingDataSourceID) {
				dsv = GetData ();
				if (dsv == null)
					throw new InvalidOperationException ("Missing data.");

				eventArgs = new ListViewInsertEventArgs (insertItem);
				ExtractItemValues (eventArgs.Values, insertItem, true);
			} else
				eventArgs = new ListViewInsertEventArgs (insertItem);

			OnItemInserting (eventArgs);
			if (!eventArgs.Cancel && IsBoundUsingDataSourceID) {
				_lastInsertValues = eventArgs.Values;
				dsv.Insert (_lastInsertValues, new DataSourceViewOperationCallback (InsertNewItemCallback));
			}
		}

		bool InsertNewItemCallback (int recordsAffected, Exception ex)
		{
			var eventArgs = new ListViewInsertedEventArgs (_lastInsertValues, recordsAffected, ex);
			OnItemInserted (eventArgs);
			_lastInsertValues = null;

			if (ex != null && !eventArgs.ExceptionHandled)
				return false;

			// This will effectively reset the insert values
			if (!eventArgs.KeepInInsertMode)
				RequiresDataBinding = true;

			return true;
		}
		
		protected virtual void InstantiateEmptyDataTemplate (Control container)
		{
			if (_emptyDataTemplate != null)
				_emptyDataTemplate.InstantiateIn (container);
		}
	
		protected virtual void InstantiateEmptyItemTemplate (Control container)
		{
			if (_emptyItemTemplate != null)
				_emptyItemTemplate.InstantiateIn (container);
		}
	
		protected virtual void InstantiateGroupSeparatorTemplate (Control container)
		{
			if (_groupSeparatorTemplate != null)
				_groupSeparatorTemplate.InstantiateIn (container);
		}
		
		protected virtual void InstantiateGroupTemplate (Control container)
		{
			if (_groupTemplate != null)
				_groupTemplate.InstantiateIn (container);
		}
	
		protected virtual void InstantiateInsertItemTemplate (Control container)
		{
			if (_insertItemTemplate != null)
				_insertItemTemplate.InstantiateIn (container);
		}
	
		protected virtual void InstantiateItemSeparatorTemplate (Control container)
		{
			if (_itemSeparatorTemplate != null)
				_itemSeparatorTemplate.InstantiateIn (container);
		}
	
		protected virtual void InstantiateItemTemplate (Control container, int displayIndex)
		{
			if (_itemTemplate == null)
				throw new InvalidOperationException ("ItemTemplate is missing");

			ITemplate template = _itemTemplate;

			if ((displayIndex % 2 != 0) && _alternatingItemTemplate != null)
				template = _alternatingItemTemplate;
			
			if ((displayIndex == _selectedIndex) && _selectedItemTemplate != null)
				template = _selectedItemTemplate;

			if ((displayIndex == _editIndex) && _editItemTemplate != null)
				template = _editItemTemplate;

			template.InstantiateIn (container);
		}

		void LoadDataKeysState (object savedState)
		{
			object[] state = savedState as object[];
			int len = state != null ? state.Length : 0;

			if (len == 0)
				return;

			ArrayList dataKeyArray = DataKeyArray;
			DataKey dk;
			string[] keyNames = DataKeyNames;
			
			for (int i = 0; i < len; i++) {
				dk = new DataKey (new OrderedDictionary (), keyNames);
				((IStateManager)dk).LoadViewState (state [i]);
				dataKeyArray.Add (dk);
			}

			_dataKeys = null;
		}
		
		protected override void LoadControlState (object savedState)
		{
			object[] state = savedState as object[];
			if (state == null || state.Length != 8)
				return;

			object o;
			base.LoadViewState (state [0]);
			if ((o = state [1]) != null)
				DataKeyNames = (string[])o;
			LoadDataKeysState (state [2]);
			if ((o = state [3]) != null)
				GroupItemCount = (int)o;
			if ((o = state [4]) != null)
				EditIndex = (int)o;
			if ((o = state [5]) != null)
				SelectedIndex = (int)o;
			if ((o = state [6]) != null)
				_sortDirection = (SortDirection)o;
			if ((o = state [7]) != null)
				_sortExpression = (string)o;
		}
	
		protected override void LoadViewState (object savedState)
		{
			object[] state = savedState as object[];
			int len = state != null ? state.Length : 0;

			if (len == 0)
				return;

			base.LoadViewState (state [0]);
		}
	
		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			ListViewCommandEventArgs args = e as ListViewCommandEventArgs;
			if (args != null) {
				bool causesValidation = false;
				IButtonControl button = args.CommandSource as IButtonControl;
				if (button != null && button.CausesValidation) {
					Page.Validate (button.ValidationGroup);
					causesValidation = true;
				}

				ProcessCommand (args, causesValidation);
				return true;
			}

			return base.OnBubbleEvent (source, e);
		}

		void ProcessCommand (ListViewCommandEventArgs args, bool causesValidation)
		{
			OnItemCommand (args);
		}
		
		protected override void OnInit (EventArgs e)
		{
			Page.RegisterRequiresControlState (this);
			base.OnInit (e);
		}

		void InvokeEvent <T> (object key, T args) where T : EventArgs
		{
			EventHandlerList events = Events;

			if (events != null) {
				EventHandler <T> eh = events [key] as EventHandler <T>;
				if (eh != null)
					eh (this, args);
			}
		}

		void InvokeEvent (object key, EventArgs args)
		{
			EventHandlerList events = Events;

			if (events != null) {
				EventHandler eh = events [key] as EventHandler;
				if (eh != null)
					eh (this, args);
			}
		}
		
		protected virtual void OnItemCanceling (ListViewCancelEventArgs e)
		{
			InvokeEvent <ListViewCancelEventArgs> (ItemCancellingEvent, e);
		}
	
		protected virtual void OnItemCommand (ListViewCommandEventArgs e)
		{
			InvokeEvent <ListViewCommandEventArgs> (ItemCommandEvent, e);
		}
	
		protected virtual void OnItemCreated (ListViewItemEventArgs e)
		{
			InvokeEvent <ListViewItemEventArgs> (ItemCreatedEvent, e);
		}
	
		protected virtual void OnItemDataBound (ListViewItemEventArgs e)
		{
			InvokeEvent <ListViewItemEventArgs> (ItemDataBoundEvent, e);
		}
	
		protected virtual void OnItemDeleted (ListViewDeletedEventArgs e)
		{
			InvokeEvent <ListViewDeletedEventArgs> (ItemDeletedEvent, e);
		}
	
		protected virtual void OnItemDeleting (ListViewDeleteEventArgs e)
		{
			InvokeEvent <ListViewDeleteEventArgs> (ItemDeletingEvent, e);
		}
	
		protected virtual void OnItemEditing (ListViewEditEventArgs e)
		{
			InvokeEvent <ListViewEditEventArgs> (ItemEditingEvent, e);
		}
	
		protected virtual void OnItemInserted (ListViewInsertedEventArgs e)
		{
			InvokeEvent <ListViewInsertedEventArgs> (ItemInsertedEvent, e);
		}
	
		protected virtual void OnItemInserting (ListViewInsertEventArgs e)
		{
			InvokeEvent <ListViewInsertEventArgs> (ItemInsertingEvent, e);
		}
	
		protected virtual void OnItemUpdated (ListViewUpdatedEventArgs e)
		{
			InvokeEvent <ListViewUpdatedEventArgs> (ItemUpdatedEvent, e);
		}
	
		protected virtual void OnItemUpdating (ListViewUpdateEventArgs e)
		{
			InvokeEvent <ListViewUpdateEventArgs> (ItemUpdatingEvent, e);
		}
	
		protected virtual void OnLayoutCreated (EventArgs e)
		{
			InvokeEvent (LayoutCreatedEvent, e);
		}
	
		protected virtual void OnPagePropertiesChanged (EventArgs e)
		{
			InvokeEvent (PagePropertiesChangedEvent, e);
		}
	
		protected virtual void OnPagePropertiesChanging (PagePropertiesChangingEventArgs e)
		{
			InvokeEvent (PagePropertiesChangingEvent, e);
		}
	
		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
			InvokeEvent (SelectedIndexChangedEvent, e);
		}
	
		protected virtual void OnSelectedIndexChanging (ListViewSelectEventArgs e)
		{
			InvokeEvent (SelectedIndexChangingEvent, e);
		}
	
		protected virtual void OnSorted (EventArgs e)
		{
			InvokeEvent (SortedEvent, e);
		}
	
		protected virtual void OnSorting (ListViewSortEventArgs e)
		{
			InvokeEvent <ListViewSortEventArgs> (SortingEvent, e);
		}
	
		protected virtual void OnTotalRowCountAvailable (PageEventArgs e)
		{
			InvokeEvent <PageEventArgs> (TotalRowCountAvailableEvent, e);
		}
	
		protected override void PerformDataBinding (IEnumerable data)
		{
			base.PerformDataBinding (data);
			TrackViewState ();

			int childCount = CreateChildControls (data, true);
			ChildControlsCreated = true;
		}
	
		protected override void PerformSelect ()
		{
			EnsureLayoutTemplate ();
			base.PerformSelect ();
		}
	
		protected virtual void RemoveItems ()
		{
			if (_nonGroupedItemsContainer != null)
				RemoveItems (_nonGroupedItemsContainer, _nonGroupedItemsContainerFirstItemIndex, _nonGroupedItemsContainerItemCount);
		}

		void RemoveItems (Control container, int start, int count)
		{
			int i = count;
			while (i-- > 0)
				container.Controls.RemoveAt (start);
		}
		
		protected override void Render (HtmlTextWriter writer)
		{
			base.Render (writer);
			// Why override?
		}

		object SaveDataKeysState ()
		{
			DataKeyArray dka = DataKeys;

			int len = dka != null ? dka.Count : 0;
			if (len == 0)
				return null;

			object[] state = new object [len];
			DataKey dk;
			for (int i = 0; i < len; i++) {
				dk = dka [i];
				if (dk == null) {
					state [i] = null;
					continue;
				}

				state [i] = ((IStateManager)dk).SaveViewState ();
			}
			
			return state;
		}
		
		protected override object SaveControlState ()
		{
			object[] ret = new object [8];
			string[] dataKeyNames = DataKeyNames;
			object dataKeysState = SaveDataKeysState ();
			
			ret [0] = base.SaveViewState ();
			ret [1] = dataKeyNames.Length > 0 ? dataKeyNames : null;
			ret [2] = dataKeysState != null ? dataKeysState : null;
			ret [3] = _groupItemCount > 1 ? (object)_groupItemCount : null;
			ret [4] = _editIndex != -1 ? (object)_editIndex : null;
			ret [5] = _selectedIndex != -1 ? (object)_selectedIndex : null;
			ret [6] = _sortDirection != SortDirection.Ascending ? (object)_sortDirection : null;
			ret [7] = String.IsNullOrEmpty (_sortExpression) ? null : _sortExpression;
			
			return ret;
		}
	
		protected override object SaveViewState ()
		{
			object[] states = new object [2];

			states [0] = base.SaveViewState ();
			states [1] = null; // What goes here?
			
			return states;
		}
	
		protected virtual void SetPageProperties (int startRowIndex, int maximumRows, bool databind)
		{
		}
	
		public virtual void Sort (string sortExpression, SortDirection sortDirection)
		{
		}
	
		void IPageableItemContainer.SetPageProperties (int startRowIndex, int maximumRows, bool databind)
		{
			throw new NotImplementedException ();
		}
	
		public virtual void UpdateItem (int itemIndex, bool causesValidation)
		{
		}
		

		NotSupportedException StylingNotSupported ()
		{
			return new NotSupportedException ("Style properties are not supported on ListView. Apply styling or CSS classes to the elements in the ListView's templates.");
		}
	}
}
#endif

