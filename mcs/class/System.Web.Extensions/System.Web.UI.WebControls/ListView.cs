//
// System.Web.UI.WebControls.ListView
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007-2010 Novell, Inc
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
	[DefaultEvent ("SelectedIndexChanged")]
	[DefaultProperty ("SelectedValue")]
	[SupportsEventValidation]
	[ControlValueProperty ("SelectedValue")]
	[ToolboxBitmap (typeof (System.Web.UI.WebControls.ListView), "ListView.ico")]
	[ToolboxItemFilter ("System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", ToolboxItemFilterType.Require)]
	[Designer ("System.Web.UI.Design.WebControls.ListViewDesigner, System.Web.Extensions.Design, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
	public class ListView : DataBoundControl, INamingContainer, IPageableItemContainer
	{
		const int CSTATE_BASE_STATE = 0;
		const int CSTATE_DATAKEYNAMES = 1;
		const int CSTATE_DATAKEYSSTATE = 2;
		const int CSTATE_GROUPITEMCOUNT = 3;
		const int CSTATE_TOTALROWCOUNT = 4;
		const int CSTATE_EDITINDEX = 5;
		const int CSTATE_SELECTEDINDEX = 6;
		const int CSTATE_SORTDIRECTION = 7;
		const int CSTATE_SORTEXPRESSION = 8;
		const int CSTATE_COUNT = 9;

		delegate void GroupStart ();
		
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

		int _totalRowCount;
		int _startRowIndex = -1;
		int _maximumRows = -1;
		int _selectedIndex;
		int _editIndex;
		int _groupItemCount;

		List <ListViewDataItem> _items;
		string [] _dataKeyNames;
		DataKeyArray _dataKeys;
		ArrayList _dataKeyArray;
		SortDirection _sortDirection = SortDirection.Ascending;
		string _sortExpression = String.Empty;

		Control _layoutTemplatePlaceholder;
		Control _nonGroupedItemsContainer;
		Control _groupedItemsContainer;
		int _nonGroupedItemsContainerFirstItemIndex = -1;
		int _groupedItemsContainerPlaceholderIndex = -1;
		int _nonGroupedItemsContainerItemCount;
		int _groupedItemsContainerItemCount;
		IOrderedDictionary _lastInsertValues;
		IOrderedDictionary _currentEditOldValues;
		IOrderedDictionary _currentEditNewValues;
		IOrderedDictionary _currentDeletingItemKeys;
		IOrderedDictionary _currentDeletingItemValues;
		
		int _firstIdAfterLayoutTemplate = 0;

		bool usingFakeData;
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
		
		[Category ("Action")]
		public event EventHandler <ListViewCancelEventArgs> ItemCanceling {
			add { Events.AddHandler (ItemCancellingEvent, value); }
			remove { Events.RemoveHandler (ItemCancellingEvent, value); }
		}
	
		[Category ("Action")]
		public event EventHandler <ListViewCommandEventArgs> ItemCommand {
			add { Events.AddHandler (ItemCommandEvent, value); }
			remove { Events.RemoveHandler (ItemCommandEvent, value); }
		}
	
		[Category ("Behavior")]
		public event EventHandler <ListViewItemEventArgs> ItemCreated {
			add { Events.AddHandler (ItemCreatedEvent, value); }
			remove { Events.RemoveHandler (ItemCreatedEvent, value); }
		}
	
		[Category ("Data")]
		public event EventHandler <ListViewItemEventArgs> ItemDataBound {
			add { Events.AddHandler (ItemDataBoundEvent, value); }
			remove { Events.RemoveHandler (ItemDataBoundEvent, value); }
		}
	
		[Category ("Action")]
		public event EventHandler <ListViewDeletedEventArgs> ItemDeleted {
			add { Events.AddHandler (ItemDeletedEvent, value); }
			remove { Events.RemoveHandler (ItemDeletedEvent, value); }
		}
	
		[Category ("Action")]
		public event EventHandler <ListViewDeleteEventArgs> ItemDeleting {
			add { Events.AddHandler (ItemDeletingEvent, value); }
			remove { Events.RemoveHandler (ItemDeletingEvent, value); }
		}
	
		[Category ("Action")]
		public event EventHandler <ListViewEditEventArgs> ItemEditing {
			add { Events.AddHandler (ItemEditingEvent, value); }
			remove { Events.RemoveHandler (ItemEditingEvent, value); }
		}
	
		[Category ("Action")]
		public event EventHandler <ListViewInsertedEventArgs> ItemInserted {
			add { Events.AddHandler (ItemInsertedEvent, value); }
			remove { Events.RemoveHandler (ItemInsertedEvent, value); }
		}
	
		[Category ("Action")]
		public event EventHandler <ListViewInsertEventArgs> ItemInserting {
			add { Events.AddHandler (ItemInsertingEvent, value); }
			remove { Events.RemoveHandler (ItemInsertingEvent, value); }
		}
	
		[Category ("Action")]
		public event EventHandler <ListViewUpdatedEventArgs> ItemUpdated {
			add { Events.AddHandler (ItemUpdatedEvent, value); }
			remove { Events.RemoveHandler (ItemUpdatedEvent, value); }
		}
	
		[Category ("Action")]
		public event EventHandler <ListViewUpdateEventArgs> ItemUpdating {
			add { Events.AddHandler (ItemUpdatingEvent, value); }
			remove { Events.RemoveHandler (ItemUpdatingEvent, value); }
		}
	
		[Category ("Behavior")]
		public event EventHandler LayoutCreated {
			add { Events.AddHandler (LayoutCreatedEvent, value); }
			remove { Events.RemoveHandler (LayoutCreatedEvent, value); }
		}
	
		[Category ("Behavior")]
		public event EventHandler PagePropertiesChanged {
			add { Events.AddHandler (PagePropertiesChangedEvent, value); }
			remove { Events.RemoveHandler (PagePropertiesChangedEvent, value); }
		}
	
		[Category ("Behavior")]
		public event EventHandler <PagePropertiesChangingEventArgs> PagePropertiesChanging {
			add { Events.AddHandler (PagePropertiesChangingEvent, value); }
			remove { Events.RemoveHandler (PagePropertiesChangingEvent, value); }
		}
	
		[Category ("Action")]
		public event EventHandler SelectedIndexChanged {
			add { Events.AddHandler (SelectedIndexChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedIndexChangedEvent, value); }
		}
	
		[Category ("Action")]
		public event EventHandler <ListViewSelectEventArgs> SelectedIndexChanging {
			add { Events.AddHandler (SelectedIndexChangingEvent, value); }
			remove { Events.RemoveHandler (SelectedIndexChangingEvent, value); }
		}
	
		[Category ("Action")]
		public event EventHandler Sorted {
			add { Events.AddHandler (SortedEvent, value); }
			remove { Events.RemoveHandler (SortedEvent, value); }
		}
	
		[Category ("Action")]
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
		IOrderedDictionary CurrentEditOldValues {
			get {
				if (_currentEditOldValues == null)
					_currentEditOldValues = new OrderedDictionary ();

				return _currentEditOldValues;
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override string AccessKey {
			get { return base.AccessKey; }
			set { throw StylingNotSupported (); }
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[Browsable (false)]
		[TemplateContainer (typeof (System.Web.UI.WebControls.ListViewDataItem), BindingDirection.TwoWay)]
		public virtual ITemplate AlternatingItemTemplate {
			get { return _alternatingItemTemplate; }
			set { _alternatingItemTemplate = value; }
		}
	
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override Color BackColor {
			get { return base.BackColor; }
			set { throw StylingNotSupported (); }
		}
	
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Color BorderColor {
			get { return base.BorderColor; }
			set { throw StylingNotSupported (); }
		}
	
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override BorderStyle BorderStyle {
			get { return base.BorderStyle; }
			set { throw StylingNotSupported (); }
		}
	
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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
	
		[Category ("Behavior")]
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

		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[CssClassProperty]
		public override string CssClass {
			get { return base.CssClass; }
			set { throw StylingNotSupported (); }
		}
		
		[Editor ("System.Web.UI.Design.WebControls.DataFieldEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof (System.Drawing.Design.UITypeEditor))]
		[DefaultValue ("")]
		[TypeConverter (typeof (System.Web.UI.WebControls.StringArrayConverter))]
		[Category ("Data")]	
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
	
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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
		[Category ("Default")]	
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
	
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual ListViewItem EditItem {
			get {
				IList <ListViewDataItem> items = Items;
				if (_editIndex >= 0 && _editIndex < items.Count)
					return items [_editIndex];
				return null;
			}
		}
	
		[TemplateContainer (typeof (System.Web.UI.WebControls.ListViewDataItem), BindingDirection.TwoWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		[DefaultValue ("")]
		public virtual ITemplate EditItemTemplate {
			get { return _editItemTemplate; }
			set { _editItemTemplate = value; }
		}	

		[TemplateContainer (typeof (System.Web.UI.WebControls.ListView))]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[Browsable (false)]
		public virtual ITemplate EmptyDataTemplate {
			get { return _emptyDataTemplate; }
			set { _emptyDataTemplate = value; }
		}
	

		[Browsable (false)]
		[TemplateContainer (typeof (System.Web.UI.WebControls.ListViewItem))]
		[DefaultValue ("")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public virtual ITemplate EmptyItemTemplate {
			get { return _emptyItemTemplate; }
			set { _emptyItemTemplate = value; }
		}

		[WebCategory ("Behavior")]
		[DefaultValue (false)]
		[MonoTODO ("Figure out where it is used and what's the effect of setting it to true.")]
		public virtual bool EnableModelValidation {
			get {
				object o = ViewState ["EnableModelValidation"];
				if (o == null)
					return false;

				return (bool)o;
			}
			
			set {
				if (value)
					ViewState ["EnableModelValidation"] = value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override FontInfo Font {
			get { throw StylingNotSupported (); }
		}
	
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override Color ForeColor {
			get { return base.ForeColor; }
			set { throw StylingNotSupported (); }
		}
	
		[Category ("Default")]
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
	
		[Category ("Behavior")]
		[DefaultValue ("groupPlaceholder")]
		public virtual string GroupPlaceholderID {
			get {
				string s = ViewState ["GroupPlaceholderID"] as string;
				if (s != null)
					return s;

				return "groupPlaceholder";
			}
			
			set {
				if (String.IsNullOrEmpty (value))
					throw new ArgumentOutOfRangeException ("value");

				ViewState ["GroupPlaceholderID"] = value;
			}
		}	

		[TemplateContainer (typeof (System.Web.UI.WebControls.ListViewItem))]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		[DefaultValue ("")]
		public virtual ITemplate GroupSeparatorTemplate {
			get { return _groupSeparatorTemplate; }
			set { _groupSeparatorTemplate = value; }
		}

		[TemplateContainer (typeof (System.Web.UI.WebControls.ListViewItem))]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[Browsable (false)]
		public virtual ITemplate GroupTemplate {
			get { return _groupTemplate; }
			set { _groupTemplate = value; }
		}
	
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override Unit Height {
			get { return base.Height; }
			set { throw StylingNotSupported (); }
		}
	
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public virtual ListViewItem InsertItem {
			get;
			private set;
		}
	
		[Category ("Default")]
		[DefaultValue (InsertItemPosition.None)]
		public virtual InsertItemPosition InsertItemPosition {
			get;
			set;
		}
	
		[TemplateContainer (typeof (System.Web.UI.WebControls.ListViewItem), BindingDirection.TwoWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[Browsable (false)]
		public virtual ITemplate InsertItemTemplate {
			get { return _insertItemTemplate; }
			set { _insertItemTemplate = value; }
		}
	
		[DefaultValue ("itemPlaceholder")]
		[Category ("Behavior")]
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
	
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public virtual IList <ListViewDataItem> Items {
			get {
				if (_items == null)
					_items = new List <ListViewDataItem> ();

				return _items;
			}
		}
	
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[Browsable (false)]
		[TemplateContainer (typeof (System.Web.UI.WebControls.ListViewItem))]
		public virtual ITemplate ItemSeparatorTemplate {
			get { return _itemSeparatorTemplate; }
			set { _itemSeparatorTemplate = value; }
		}

		[Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (System.Web.UI.WebControls.ListViewDataItem), BindingDirection.TwoWay)]
		[DefaultValue ("")]
		public virtual ITemplate ItemTemplate {
			get { return _itemTemplate; }
			set { _itemTemplate = value; }
		}
	
		[TemplateContainer (typeof (System.Web.UI.WebControls.ListView))]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[Browsable (false)]
		public virtual ITemplate LayoutTemplate {
			get { return _layoutTemplate; }
			set { _layoutTemplate = value; }
		}
	
		protected virtual int MaximumRows {
			get { return _maximumRows; }
		}
	
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual DataKey SelectedDataKey {
			get {
				if (_dataKeyNames == null || _dataKeyNames.Length == 0)
					throw new InvalidOperationException ("Data keys must be specified on ListView '" + ID + "' before the selected data keys can be retrieved. Use the DataKeyNames property to specify data keys.");

				DataKeyArray dataKeys = DataKeys;
				int selIndex = SelectedIndex;
				if (selIndex > -1 && selIndex < dataKeys.Count)
					return dataKeys [selIndex];

				return null;
			}
		}

		[Browsable (false)]
		public virtual DataKey SelectedPersistedDataKey {
			get;
			set;
		}

		[Category ("Default")]
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
	

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[Browsable (false)]
		[TemplateContainer (typeof (System.Web.UI.WebControls.ListViewDataItem), BindingDirection.TwoWay)]
		public virtual ITemplate SelectedItemTemplate {
			get { return _selectedItemTemplate; }
			set { _selectedItemTemplate = value; }
		}
	
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public object SelectedValue {
			get {
				DataKey dk = SelectedDataKey;
				if (dk != null)
					return dk.Value;

				return null;
			}
		}
	
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[DefaultValue (SortDirection.Ascending)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public virtual SortDirection SortDirection {
			get { return _sortDirection; }
		}
	
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual string SortExpression {
			get { return _sortExpression; }
		}
	
		protected virtual int StartRowIndex {
			get {
				if (_startRowIndex < 0)
					return 0;
				
				return _startRowIndex;
			}
		}
	
		int IPageableItemContainer.MaximumRows {
			get { return MaximumRows; }
		}
	
		int IPageableItemContainer.StartRowIndex {
			get { return StartRowIndex; }
		}
	
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override short TabIndex {
			get { return 0; }
			set { throw new NotSupportedException ("ListView does not allow setting this property."); }
		}
	
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override string ToolTip {
			get { return base.ToolTip; }
			set { throw StylingNotSupported (); }
		}
	
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public override Unit Width {
			get { return base.Width; }
			set { throw StylingNotSupported (); }
		}
#endregion
		
		public ListView ()
		{
			InsertItemPosition = InsertItemPosition.None;
			ResetDefaults ();
		}

		void ResetDefaults ()
		{
			_totalRowCount = -1;
			_selectedIndex = -1;
			_editIndex = -1;
			_groupItemCount = 1;
		}
		
		protected virtual void AddControlToContainer (Control control, Control container, int addLocation)
		{
			if (control == null)
				throw new ArgumentNullException ("control");

			// .NET doesn't check container for null (!)
// 			if (container == null)
// 				throw new ArgumentNullException ("container");

			Control ctl;

			if (container is HtmlTable) {
				ctl = new ListViewTableRow ();
				ctl.Controls.Add (control);
			} else if (container is HtmlTableRow) {
				ctl = new ListViewTableCell ();
				ctl.Controls.Add (control);
			} else
				ctl = control;

			container.Controls.AddAt (addLocation, ctl);
		}
	
		protected internal override void CreateChildControls ()
		{
			object itemCount = ViewState ["_!ItemCount"];
			if (itemCount != null) {
				int c = (int)itemCount;
				if (c >= 0) {
					// Fake data - we only need to make sure
					// OnTotalRowCountAvailable is called now - so that any
					// pagers can create child controls.
					object[] data = new object [c];
					usingFakeData = true;
					try {
						CreateChildControls (data, false);
					} finally {
						usingFakeData = false;
					}
				}
			} else if (RequiresDataBinding)
				EnsureDataBound ();
			
			base.CreateChildControls ();
		}

		protected virtual int CreateChildControls (IEnumerable dataSource, bool dataBinding)
		{
			IList <ListViewDataItem> retList = null;
			EnsureLayoutTemplate ();
			RemoveItems ();
			
			// If any of the _maximumRows or _startRowIndex is different to their
			// defaults, it means we are paging - i.e. SetPageProperties has been
			// called.
			bool haveDataToPage = _maximumRows > 0 || _startRowIndex > 0;
			var pagedDataSource = new ListViewPagedDataSource ();
			
			if (dataBinding) {
				DataSourceView view = GetData ();
				if (view == null)
					throw new InvalidOperationException ("dataSource returned a null reference for DataSourceView.");

				int totalRowCount = 0;
				if (haveDataToPage && view.CanPage) {
					pagedDataSource.AllowServerPaging = true;
					if (view.CanRetrieveTotalRowCount)
						totalRowCount = SelectArguments.TotalRowCount;
					else {
						ICollection ds = dataSource as ICollection;
						if (ds == null)
							throw new InvalidOperationException ("dataSource does not implement the ICollection interface.");
						totalRowCount = ds.Count + StartRowIndex;
					}
				}

				pagedDataSource.TotalRowCount = totalRowCount;
				_totalRowCount = totalRowCount;
				DataKeyArray.Clear ();
			} else {
				if (!(dataSource is ICollection))
					throw new InvalidOperationException ("dataSource does not implement the ICollection interface and dataBinding is false.");
				pagedDataSource.TotalRowCount = _totalRowCount;
				_totalRowCount = -1;
			}

			pagedDataSource.StartRowIndex = StartRowIndex;
			pagedDataSource.MaximumRows = MaximumRows;
			pagedDataSource.DataSource = dataSource;

			bool emptySet = false;
			if (dataSource != null) {
				if (GroupItemCount <= 1 && GroupTemplate == null)
					retList = CreateItemsWithoutGroups (pagedDataSource, dataBinding, InsertItemPosition, DataKeyArray);
				else
					retList = CreateItemsInGroups (pagedDataSource, dataBinding, InsertItemPosition, DataKeyArray);

				if (retList == null || retList.Count == 0)
					emptySet = true;

				if (haveDataToPage) {
					// Data source has paged data for us, so we must use its total row
					// count
					_totalRowCount = pagedDataSource.DataSourceCount;
				} else if (!emptySet && _totalRowCount > -1)
					_totalRowCount = retList.Count;
				else if (_totalRowCount > -1)
					_totalRowCount = 0;
				
				OnTotalRowCountAvailable (new PageEventArgs (_startRowIndex, _maximumRows, _totalRowCount));
			} else
				emptySet = true;

			if (!usingFakeData && emptySet) {
				Controls.Clear ();
				CreateEmptyDataItem ();
			}
			
			if (retList == null)
				return 0;

			return _totalRowCount;
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
			if (_emptyItemTemplate != null) {
				ListViewItem item = CreateItem (ListViewItemType.EmptyItem);
				InstantiateEmptyItemTemplate (item);
				OnItemCreated (new ListViewItemEventArgs (item));
				return item;
			}

			return null;
		}
	
		protected virtual ListViewItem CreateInsertItem ()
		{
			if (_insertItemTemplate == null)
				// .NET throws a different message, but it's incorrect so we'll use
				// this one
				throw new InvalidOperationException ("The ListView control '" + ID + "' does not have an InsertItemTemplate template specified.");
			
			ListViewItem ret = CreateItem (ListViewItemType.InsertItem);
			InstantiateInsertItemTemplate (ret);
			OnItemCreated (new ListViewItemEventArgs (ret));
			InsertItem = ret;

			return ret;
		}
	
		protected virtual ListViewItem CreateItem (ListViewItemType itemType)
		{
			return new ListViewItem (itemType);
		}

		void InsertSeparatorItem (Control container, int position)
		{
			Control control = CreateItem (ListViewItemType.DataItem);
			InstantiateItemSeparatorTemplate (control);
			AddControlToContainer (control, container, position);
		}

		ListViewDataItem InsertDataItem (object dataItem, Control container, bool dataBinding, ArrayList keyArray, int startIndex, int position, ref int displayIndex)
		{
			ListViewDataItem lvdi = CreateDataItem (startIndex + displayIndex, displayIndex);
			InstantiateItemTemplate (lvdi, displayIndex);

			if (dataBinding) {
				lvdi.DataItem = dataItem;

				OrderedDictionary dict = new OrderedDictionary ();
				string[] dataKeyNames = DataKeyNames;
					
				foreach (string s in dataKeyNames)
					dict.Add (s, DataBinder.GetPropertyValue (dataItem, s));
					
				DataKey dk = new DataKey (dict, dataKeyNames);
				if (keyArray.Count == displayIndex)
					keyArray.Add (dk);
				else
					keyArray [displayIndex] = dk;
			}
				
			OnItemCreated (new ListViewItemEventArgs (lvdi));
			AddControlToContainer (lvdi, container, position);

			if (dataBinding) {
				lvdi.DataBind ();
				OnItemDataBound (new ListViewItemEventArgs (lvdi));
			}
			displayIndex++;

			return lvdi;
		}
		
		protected virtual IList <ListViewDataItem> CreateItemsInGroups (ListViewPagedDataSource dataSource, bool dataBinding, InsertItemPosition insertPosition,
										ArrayList keyArray)
		{
			if (_groupTemplate == null)
				return null;
			
			if (_groupedItemsContainer == null)
				_groupedItemsContainer = FindPlaceholder (GroupPlaceholderID, this);

			if (_groupedItemsContainer == null)
				throw NoPlaceholder (true);

			Control parent = _groupedItemsContainer.Parent;
			int gpos;
			if (_groupedItemsContainerPlaceholderIndex == -1) {
				gpos = 0;
				if (parent != null) {
					gpos = parent.Controls.IndexOf (_groupedItemsContainer);
					parent.Controls.Remove (_groupedItemsContainer);
					_groupedItemsContainer = parent;
					if (_groupedItemsContainer != _layoutTemplatePlaceholder)
						AddControlToContainer (_groupedItemsContainer, _layoutTemplatePlaceholder, 0);
				}
				_groupedItemsContainerPlaceholderIndex = gpos;
			} else {
				gpos = _groupedItemsContainerPlaceholderIndex;
				ResetChildNames (_firstIdAfterLayoutTemplate);
			}

			IList <ListViewDataItem> ret = Items;
			ret.Clear ();

			int firstItemIndexInGroup = -1;
			Control currentGroup = StartNewGroup (false, ref gpos, ref firstItemIndexInGroup);
			int groupItemCount = GroupItemCount;
			int itemPosInGroup = firstItemIndexInGroup;
			int groupItemCounter = groupItemCount;
			ListViewItem lvi;
			bool needSeparator = false;
			bool haveSeparatorTemplate = _itemSeparatorTemplate != null;
			
			if (insertPosition == InsertItemPosition.FirstItem) {
				lvi = CreateInsertItem ();
				AddControlToContainer (lvi, currentGroup, itemPosInGroup++);
				groupItemCounter--;
				needSeparator = true;
			}

			int displayIndex = 0;
			int startIndex = dataSource.StartRowIndex;
			int dataCount = dataSource.Count;
			int numberOfGroups = (dataCount / groupItemCount) + (dataCount % groupItemCount) - 1;
			GroupStart groupStart = () => {
				if (groupItemCounter <= 0) {
					groupItemCounter = groupItemCount;
					currentGroup = StartNewGroup (numberOfGroups >= 1, ref gpos, ref firstItemIndexInGroup);
					numberOfGroups--;
					itemPosInGroup = firstItemIndexInGroup;
					_groupedItemsContainerItemCount++;
					needSeparator = false;
				}
			};
			
			foreach (object item in dataSource) {
				groupStart ();
				if (needSeparator && haveSeparatorTemplate)
					InsertSeparatorItem (currentGroup, itemPosInGroup++);

				ret.Add (InsertDataItem (item, currentGroup, dataBinding, keyArray, startIndex, itemPosInGroup++, ref displayIndex));
				groupItemCounter--;

				if (!needSeparator)
					needSeparator = true;
			}

			groupStart ();			
			if (insertPosition == InsertItemPosition.LastItem) {
				if (needSeparator && haveSeparatorTemplate)
					InsertSeparatorItem (currentGroup, itemPosInGroup++);
				
				groupStart ();
				lvi = CreateInsertItem ();
				AddControlToContainer (lvi, currentGroup, itemPosInGroup++);
				groupItemCounter--;
			}

			if (groupItemCounter > 0 && _emptyItemTemplate != null) {
				while (groupItemCounter > 0) {
					if (haveSeparatorTemplate)
						InsertSeparatorItem (currentGroup, itemPosInGroup++);
					
					lvi = CreateEmptyItem ();
					AddControlToContainer (lvi, currentGroup, itemPosInGroup++);
					groupItemCounter--;
				}
			}
			
			return ret;
		}

		Control StartNewGroup (bool needSeparator, ref int position, ref int firstItemIndexInGroup)
		{
			Control control = new ListViewContainer ();
			InstantiateGroupTemplate (control);
			Control placeholder = FindPlaceholder (ItemPlaceholderID, control);
			if (placeholder == null)
				throw NoPlaceholder (false);
			
			Control parent = placeholder.Parent;
			
			firstItemIndexInGroup = parent.Controls.IndexOf (placeholder);
			if (needSeparator) {
				Control separator = new Control ();
				InstantiateGroupSeparatorTemplate (separator);
				if (separator.Controls.Count > 0) {
					AddControlToContainer (separator, _groupedItemsContainer, position++);
					_groupedItemsContainerItemCount++;
				} else
					separator = null;
			}

			parent.Controls.RemoveAt (firstItemIndexInGroup);
			AddControlToContainer (control, _groupedItemsContainer, position++);
			
			return parent;
		}
		
		protected virtual IList <ListViewDataItem> CreateItemsWithoutGroups (ListViewPagedDataSource dataSource, bool dataBinding, InsertItemPosition insertPosition,
										     ArrayList keyArray)
		{
			if (_nonGroupedItemsContainer == null)
				_nonGroupedItemsContainer = FindPlaceholder (ItemPlaceholderID, this);
			_nonGroupedItemsContainerItemCount = 0;

			if (_nonGroupedItemsContainer == null)
				throw NoPlaceholder (false);

			Control parent = _nonGroupedItemsContainer.Parent;
			
			int ipos;
			if (_nonGroupedItemsContainerFirstItemIndex == -1) {
				ipos = 0;
				if (parent != null) {
					ipos = parent.Controls.IndexOf (_nonGroupedItemsContainer);
					parent.Controls.Remove (_nonGroupedItemsContainer);
					_nonGroupedItemsContainer = parent;
					if (_nonGroupedItemsContainer != _layoutTemplatePlaceholder)
						AddControlToContainer (_nonGroupedItemsContainer, _layoutTemplatePlaceholder, 0);
				}
				_nonGroupedItemsContainerFirstItemIndex = ipos;
			} else {
				ipos = _nonGroupedItemsContainerFirstItemIndex;
				ResetChildNames (_firstIdAfterLayoutTemplate);
			}
			
			IList <ListViewDataItem> ret = Items;
			ret.Clear ();
			
			ListViewItem lvi;
			ListViewItem container;
			bool needSeparator = false;

			if (insertPosition == InsertItemPosition.FirstItem) {
				lvi = CreateInsertItem ();
				AddControlToContainer (lvi, _nonGroupedItemsContainer, ipos++);
				_nonGroupedItemsContainerItemCount++;
				needSeparator = true;
			}

			bool haveSeparatorTemplate = _itemSeparatorTemplate != null;
			int displayIndex = 0;
			int startIndex = dataSource.StartRowIndex;

			foreach (object item in dataSource) {
				if (needSeparator && haveSeparatorTemplate) {
					InsertSeparatorItem (_nonGroupedItemsContainer, ipos++);
					_nonGroupedItemsContainerItemCount++;
				}

				ret.Add (InsertDataItem (item, _nonGroupedItemsContainer, dataBinding, keyArray, startIndex, ipos++, ref displayIndex));
				_nonGroupedItemsContainerItemCount++;
				
				if (!needSeparator)
					needSeparator = true;
			}

			if (insertPosition == InsertItemPosition.LastItem) {
				if (needSeparator && haveSeparatorTemplate) {
					container = new ListViewItem ();
					InstantiateItemSeparatorTemplate (container);
					AddControlToContainer (container, _nonGroupedItemsContainer, ipos++);
					_nonGroupedItemsContainerItemCount++;
				}
				
				lvi = CreateInsertItem ();
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
				Controls.Add (_layoutTemplatePlaceholder);
			}
			
			OnLayoutCreated (EventArgs.Empty);
		}
	
		public virtual void DeleteItem (int itemIndex)
		{
			if (itemIndex < 0)
				throw new InvalidOperationException ("itemIndex is less than 0.");

			IList <ListViewDataItem> items = Items;
			if (itemIndex < items.Count)
				DoDelete (items [itemIndex], itemIndex);
		}
	
		protected virtual void EnsureLayoutTemplate ()
		{
			if (Controls.Count != 0)
				return;
			
			CreateLayoutTemplate ();
			_firstIdAfterLayoutTemplate = GetDefaultNumberID ();
		}
	
		public virtual void ExtractItemValues (IOrderedDictionary itemValues, ListViewItem item, bool includePrimaryKey)
		{
			if (itemValues == null)
				throw new ArgumentNullException ("itemValues");

			IBindableTemplate bt = null;
			if (item.ItemType == ListViewItemType.DataItem) {
				ListViewDataItem dataItem = item as ListViewDataItem;
				if (dataItem == null)
					throw new InvalidOperationException ("item is not a ListViewDataItem object.");

				int displayIndex = dataItem.DisplayIndex;
				if (_editItemTemplate != null && displayIndex == EditIndex)
					bt = (IBindableTemplate) _editItemTemplate;
				else if (_selectedItemTemplate != null && (displayIndex == SelectedIndex))
					bt = (IBindableTemplate) _selectedItemTemplate;
				else if (_alternatingItemTemplate != null && (displayIndex % 2 != 0))
					bt = (IBindableTemplate) _alternatingItemTemplate;
				else
					bt = (IBindableTemplate) _itemTemplate;
			} else if (_insertItemTemplate != null && item.ItemType == ListViewItemType.InsertItem)
				bt = (IBindableTemplate) _insertItemTemplate;

			if (bt == null)
				return;

			IOrderedDictionary values = bt.ExtractValues (item);
			if (values == null || values.Count == 0)
				return;

			string[] keyNames = includePrimaryKey ? null : DataKeyNames;
			bool haveKeyNames = keyNames != null && keyNames.Length > 0;
			object key, value;
			string s;
			bool convertEmptyStringToNull = ConvertEmptyStringToNull;
			
			foreach (DictionaryEntry de in values) {
				key = de.Key;
				if (includePrimaryKey || (haveKeyNames && Array.IndexOf (keyNames, key) != -1)) {
					value = de.Value;
					if (convertEmptyStringToNull) {
						s = value as string;
						if (s != null && s.Length == 0)
							value = null;
					}
					
					itemValues [key] = value;
				}
			}
		}

		protected virtual Control FindPlaceholder (string containerID, Control container)
		{
			// .NET doesn't check whether container is null (!)
			if (String.IsNullOrEmpty (containerID))
				return null;
			
			if (container.ID == containerID)
				return container;
			
			Control ret = container.FindControl (containerID);
			if (ret != null)
				return ret;

			foreach (Control c in container.Controls) {
				ret = FindPlaceholder (containerID, c);
				if (ret != null)
					return ret;
			}

			return null;
		}
	
		public virtual void InsertNewItem (bool causesValidation)
		{
			ListViewItem insertItem = InsertItem;

			if (insertItem == null)
				throw new InvalidOperationException ("The ListView control does not have an insert item.");

			if (causesValidation) {
				Page page = Page;
				if (page != null)
					page.Validate ();
			}
			
			DoInsert (insertItem, causesValidation);
		}

		public virtual void UpdateItem (int itemIndex, bool causesValidation)
		{
			if (itemIndex < 0)
				throw new InvalidOperationException ("itemIndex is less than 0.");

			IList <ListViewDataItem> items = Items;
			if (itemIndex > items.Count)
				return;

			if (causesValidation) {
				Page page = Page;
				if (page != null)
					page.Validate ();
			}
			
			DoUpdate (items [itemIndex], itemIndex, causesValidation);
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

			if (_alternatingItemTemplate != null && (displayIndex % 2 != 0))
				template = _alternatingItemTemplate;
			
			if (_selectedItemTemplate != null && (displayIndex == _selectedIndex))
				template = _selectedItemTemplate;

			if (_editItemTemplate != null && (displayIndex == _editIndex))
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
		
		protected internal override void LoadControlState (object savedState)
		{
			ResetDefaults ();
			object[] state = savedState as object[];
			if (state == null || state.Length != CSTATE_COUNT)
				return;
			
			object o;
			base.LoadControlState (state [CSTATE_BASE_STATE]);
			if ((o = state [CSTATE_DATAKEYNAMES]) != null)
				DataKeyNames = (string[])o;
			LoadDataKeysState (state [CSTATE_DATAKEYSSTATE]);
			if ((o = state [CSTATE_GROUPITEMCOUNT]) != null)
				GroupItemCount = (int)o;
			if ((o = state [CSTATE_TOTALROWCOUNT]) != null)
				_totalRowCount = (int)o;
			if ((o = state [CSTATE_EDITINDEX]) != null)
				EditIndex = (int)o;
			if ((o = state [CSTATE_SELECTEDINDEX]) != null)
				SelectedIndex = (int)o;
			if ((o = state [CSTATE_SORTDIRECTION]) != null)
				_sortDirection = (SortDirection)o;
			if ((o = state [CSTATE_SORTEXPRESSION]) != null)
				_sortExpression = (string)o;
			
			OnTotalRowCountAvailable (new PageEventArgs (_startRowIndex, _maximumRows, _totalRowCount));
		}
	
		protected override void LoadViewState (object savedState)
		{
			object[] state = savedState as object[];
			int len = state != null ? state.Length : 0;

			if (len == 0)
				return;

			base.LoadViewState (state [0]);
			object[] values = state [0] as object[];
			if (values == null || values.Length == 0)
				return;

			Pair pair;
			IOrderedDictionary currentEditOldValues = CurrentEditOldValues;
			currentEditOldValues.Clear ();
			foreach (object value in values) {
				pair = value as Pair;
				if (pair == null)
					continue;
				currentEditOldValues.Add (pair.First, pair.Second);
			}
		}
	
		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			ListViewCommandEventArgs args = e as ListViewCommandEventArgs;
			if (args == null)
				args = new ListViewCommandEventArgs (CreateItem (ListViewItemType.EmptyItem), source, e as CommandEventArgs);
			
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

			string commandName = args.CommandName;
			string commandArgument = args.CommandArgument as string;

			if (String.Compare (commandName, DataControlCommands.SortCommandName, StringComparison.OrdinalIgnoreCase) == 0)
				Sort (commandArgument, DetermineSortDirection (commandArgument));
			else if (String.Compare (commandName, DataControlCommands.EditCommandName, StringComparison.OrdinalIgnoreCase) == 0)
				DoEdit (args);
			else if (String.Compare (commandName, DataControlCommands.CancelCommandName, StringComparison.OrdinalIgnoreCase) == 0)
				DoCancel (args);
			else if (String.Compare (commandName, DataControlCommands.DeleteCommandName, StringComparison.OrdinalIgnoreCase) == 0)
				DoDelete (args);
			else if (String.Compare (commandName, DataControlCommands.InsertCommandName, StringComparison.OrdinalIgnoreCase) == 0)
				DoInsert (args, causesValidation);
			else if (String.Compare (commandName, DataControlCommands.SelectCommandName, StringComparison.OrdinalIgnoreCase) == 0)
				DoSelect (args);
			else if (String.Compare (commandName, DataControlCommands.UpdateCommandName, StringComparison.OrdinalIgnoreCase) == 0) {
				if (causesValidation) {
					Page page = Page;
					if (page != null && !page.IsValid)
						return;
				}
				DoUpdate (args, causesValidation);
			}
		}

		int GetItemIndex (ListViewDataItem item)
		{
			if (item == null)
				return -1;

			int index = item.DisplayIndex;
			if (index < 0)
				return -1;

			return index;
		}

		void DoSelect (ListViewCommandEventArgs args)
		{
			ListViewDataItem item = args.Item as ListViewDataItem;
			int index = GetItemIndex (item);
			if (index < 0)
				return;

			var selectingArgs = new ListViewSelectEventArgs (index);
			OnSelectedIndexChanging (selectingArgs);
			if (selectingArgs.Cancel)
				return;

			SelectedIndex = selectingArgs.NewSelectedIndex;
			OnSelectedIndexChanged (EventArgs.Empty);
		}
		
		void DoInsert (ListViewCommandEventArgs args, bool causesValidation)
		{
			ListViewItem item = args.Item as ListViewItem;
			if (item == null)
				return;

			DoInsert (item, causesValidation);
		}

		void DoInsert (ListViewItem item, bool causesValidation)
		{
			if (causesValidation) {
				Page page = Page;
				if (page != null && !page.IsValid)
					return;
			}

			DataSourceView view;
			ListViewInsertEventArgs insertingArgs;
			bool usingDataSourceID = IsBoundUsingDataSourceID;
			
			if (usingDataSourceID) {
				view = GetData ();
				if (view == null)
					throw NoDataSourceView ();

				insertingArgs = new ListViewInsertEventArgs (item);
				ExtractItemValues (insertingArgs.Values, item, true);
			} else {
				view = null;
				insertingArgs = new ListViewInsertEventArgs (item);
			}
			
			OnItemInserting (insertingArgs);
			if (!usingDataSourceID || insertingArgs.Cancel)
				return;
			
			_lastInsertValues = insertingArgs.Values;
			view.Insert (_lastInsertValues, DoInsertCallback);
		}

		bool DoInsertCallback (int recordsAffected, Exception ex)
		{
			var insertedArgs = new ListViewInsertedEventArgs (recordsAffected, ex, _lastInsertValues);
			OnItemInserted (insertedArgs);
			_lastInsertValues = null;

			// This will effectively reset the insert values
			if (!insertedArgs.KeepInInsertMode)
				RequiresDataBinding = true;

			return insertedArgs.ExceptionHandled;
		}

		static InvalidOperationException NoDataSourceView ()
		{
			return new InvalidOperationException ("DataSourceView associated with the ListView is null.");
		}

		InvalidOperationException NoPlaceholder (bool group)
		{
			return new InvalidOperationException (
				String.Format ("A{0} {1} placeholder must be specified on ListView '{2}'. Specify a{0} {1} placeholder by setting a control's ID property to \"{3}\". The {1} placeholder control must also specify runat=\"server\".",
					       group ? "" : "n",
					       group ? "group" : "item",
					       ID,
					       group ? GroupPlaceholderID : ItemPlaceholderID));
		}
		
		void DoDelete (ListViewCommandEventArgs args)
		{
			ListViewDataItem item = args.Item as ListViewDataItem;
			int index = GetItemIndex (item);
			if (index < 0)
				return;

			DoDelete (item, index);
		}
		
		void DoDelete (ListViewDataItem item, int index)
		{
			bool usingDataSourceID = IsBoundUsingDataSourceID;
			var deletingArgs = new ListViewDeleteEventArgs (index);

			if (usingDataSourceID) {
				DataKeyArray dka = DataKeys;
				if (index < dka.Count)
					dka [index].Values.CopyTo (deletingArgs.Keys);
				
				ExtractItemValues (deletingArgs.Values, item, true);
			}
			OnItemDeleting (deletingArgs);
			if (!usingDataSourceID || deletingArgs.Cancel)
				return;

			DataSourceView view = GetData ();
			if (view == null)
				throw NoDataSourceView ();
			_currentDeletingItemKeys = deletingArgs.Keys;
			_currentDeletingItemValues = deletingArgs.Values;
			
			view.Delete (_currentDeletingItemKeys, _currentDeletingItemValues, DoDeleteCallback);
		}

		bool DoDeleteCallback (int affectedRows, Exception exception)
		{
			var args = new ListViewDeletedEventArgs (affectedRows, exception, _currentDeletingItemKeys, _currentDeletingItemValues);
			OnItemDeleted (args);
			
			EditIndex = -1;
			RequiresDataBinding = true;

			return args.ExceptionHandled;
		}
		
		void DoUpdate (ListViewCommandEventArgs args, bool causesValidation)
		{
			ListViewDataItem item = args.Item as ListViewDataItem;
			int index = GetItemIndex (item);
			if (index < 0)
				return;

			DoUpdate (item, index, causesValidation);
		}

		void DoUpdate (ListViewDataItem item, int index, bool causesValidation)
		{
			if (causesValidation) {
				Page page = Page;
				if (page != null && !page.IsValid)
					return;
			}
			
			bool usingDataSourceID = IsBoundUsingDataSourceID;
			var updatingArgs = new ListViewUpdateEventArgs (index);
			if (usingDataSourceID) {
				DataKeyArray dka = DataKeys;
				if (index < dka.Count)
					dka [index].Values.CopyTo (updatingArgs.Keys);

				CurrentEditOldValues.CopyTo (updatingArgs.OldValues);
				ExtractItemValues (updatingArgs.NewValues, item, true);
			}

			OnItemUpdating (updatingArgs);
			if (!usingDataSourceID || updatingArgs.Cancel)
				return;

			DataSourceView view = GetData ();
			if (view == null)
				throw NoDataSourceView ();

			_currentEditOldValues = updatingArgs.OldValues;
			_currentEditNewValues = updatingArgs.NewValues;
			view.Update (updatingArgs.Keys, _currentEditNewValues, _currentEditOldValues, DoUpdateCallback);
		}
		
		bool DoUpdateCallback (int affectedRows, Exception exception)
		{
			var args = new ListViewUpdatedEventArgs (affectedRows, exception, _currentEditNewValues, _currentEditOldValues);
			OnItemUpdated (args);

			if (!args.KeepInEditMode) {
				EditIndex = -1;
				RequiresDataBinding = true;
				_currentEditOldValues = null;
				_currentEditNewValues = null;
			}
			
			return args.ExceptionHandled;
		}
		
		void DoCancel (ListViewCommandEventArgs args)
		{
			ListViewDataItem item = args.Item as ListViewDataItem;
			int index = GetItemIndex (item);
			if (index < 0)
				return;

			ListViewCancelMode cancelMode;
			if (index == EditIndex)
				cancelMode = ListViewCancelMode.CancelingEdit;
			else if (item.ItemType == ListViewItemType.InsertItem)
				cancelMode = ListViewCancelMode.CancelingInsert;
			else
				throw new InvalidOperationException ("Item being cancelled is neither an edit item or insert item.");

			var cancelArgs = new ListViewCancelEventArgs (index, cancelMode);
			OnItemCanceling (cancelArgs);
			if (cancelArgs.Cancel)
				return;

			if (cancelMode == ListViewCancelMode.CancelingEdit)
				EditIndex = -1;

			RequiresDataBinding = true;
		}
		
		void DoEdit (ListViewCommandEventArgs args)
		{
			int index = GetItemIndex (args.Item as ListViewDataItem);
			if (index < 0)
				return;

			var editArgs = new ListViewEditEventArgs (index);
			OnItemEditing (editArgs);
			if (editArgs.Cancel)
				return;

			if (IsBoundUsingDataSourceID)
				EditIndex = index;

			RequiresDataBinding = true;
		}
		
		SortDirection DetermineSortDirection (string sortExpression)
		{
			SortDirection ret;

			if (sortExpression != SortExpression)
				return SortDirection.Ascending;

			if (SortDirection == SortDirection.Ascending)
				ret = SortDirection.Descending;
			else
				ret = SortDirection.Ascending;

			return ret;
		}
		
		protected internal override void OnInit (EventArgs e)
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
	
		protected internal override void PerformDataBinding (IEnumerable data)
		{
			base.PerformDataBinding (data);
			TrackViewState ();
			
			if (IsBoundUsingDataSourceID) {
				int editIndex = EditIndex;
				IList <ListViewDataItem> items = Items;
				
				if (editIndex > 0 && editIndex < items.Count) {
					CurrentEditOldValues.Clear ();
					ExtractItemValues (CurrentEditOldValues, items [editIndex], true);
				}
			}
					
			int childCount = CreateChildControls (data, true);
			ChildControlsCreated = true;
			ViewState ["_!ItemCount"] = childCount;
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
			if (_groupedItemsContainer != null)
				RemoveItems (_groupedItemsContainer, _groupedItemsContainerPlaceholderIndex, _groupedItemsContainerItemCount);
		}

		void RemoveItems (Control container, int start, int count)
		{
			int i = count;
			while (i-- > 0)
				container.Controls.RemoveAt (start);
		}
		
		protected internal override void Render (HtmlTextWriter writer)
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
		
		protected internal override object SaveControlState ()
		{
			object[] ret = new object [CSTATE_COUNT];
			string[] dataKeyNames = DataKeyNames;
			object dataKeysState = SaveDataKeysState ();
			
			ret [CSTATE_BASE_STATE] = base.SaveControlState ();
			ret [CSTATE_DATAKEYNAMES] = dataKeyNames.Length > 0 ? dataKeyNames : null;
			ret [CSTATE_DATAKEYSSTATE] = dataKeysState != null ? dataKeysState : null;
			ret [CSTATE_GROUPITEMCOUNT] = _groupItemCount > 1 ? (object)_groupItemCount : null;
			ret [CSTATE_TOTALROWCOUNT] = _totalRowCount >= 1 ? (object)_totalRowCount : null;
			ret [CSTATE_EDITINDEX] = _editIndex != -1 ? (object)_editIndex : null;
			ret [CSTATE_SELECTEDINDEX] = _selectedIndex != -1 ? (object)_selectedIndex : null;
			ret [CSTATE_SORTDIRECTION] = _sortDirection != SortDirection.Ascending ? (object)_sortDirection : null;
			ret [CSTATE_SORTEXPRESSION] = String.IsNullOrEmpty (_sortExpression) ? null : _sortExpression;
			
			return ret;
		}

		object SaveCurrentEditOldValues ()
		{
			IOrderedDictionary values = CurrentEditOldValues;
			int count = values.Count;
			if (count == 0)
				return null;

			object[] ret = new object [count];
			DictionaryEntry entry;
			int i = -1;
			foreach (object o in values) {
				i++;
				entry = (DictionaryEntry)o;
				ret [i] = new Pair (entry.Key, entry.Value);
			}

			return ret;
		}
		
		protected override object SaveViewState ()
		{
			object[] states = new object [2];

			states [0] = base.SaveViewState ();
			states [1] = SaveCurrentEditOldValues ();
			
			return states;
		}
	
		protected virtual void SetPageProperties (int startRowIndex, int maximumRows, bool databind)
		{
			if (maximumRows < 1)
				throw new ArgumentOutOfRangeException ("maximumRows");
			if (startRowIndex < 0)
				throw new ArgumentOutOfRangeException ("startRowIndex");

			if (maximumRows != _maximumRows || startRowIndex != _startRowIndex) {
				if (databind) {
					var args = new PagePropertiesChangingEventArgs (startRowIndex, maximumRows);
					OnPagePropertiesChanging (args);
					_startRowIndex = args.StartRowIndex;
					_maximumRows = args.MaximumRows;
					
				} else {
					_startRowIndex = startRowIndex;
					_maximumRows = maximumRows;
				}

				if (databind)
					OnPagePropertiesChanged (EventArgs.Empty);
			}

			if (databind)
				RequiresDataBinding = true;
		}
	
		public virtual void Sort (string sortExpression, SortDirection sortDirection)
		{
			ListViewSortEventArgs args = new ListViewSortEventArgs (sortExpression, sortDirection);
			OnSorting (args);

			if (args.Cancel)
				return;
			
			if (IsBoundUsingDataSourceID) {
				DataSourceView dsv = GetData ();
				if (dsv == null)
					throw new InvalidOperationException ("Missing data.");
				
				_sortDirection = args.SortDirection;
				_sortExpression = args.SortExpression;
				_startRowIndex = 0;
				EditIndex = -1;
			}
			
			OnSorted (EventArgs.Empty);
			RequiresDataBinding = true;
		}
	
		void IPageableItemContainer.SetPageProperties (int startRowIndex, int maximumRows, bool databind)
		{
			SetPageProperties (startRowIndex, maximumRows, databind);
		}

		NotSupportedException StylingNotSupported ()
		{
			return new NotSupportedException ("Style properties are not supported on ListView. Apply styling or CSS classes to the elements in the ListView's templates.");
		}
	}
}
#endif

