//
// System.Web.UI.WebControls.ListView
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
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
		IOrderedDictionary _lastInsertValues;
		
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

				if (!(dataSource is ICollection))
					throw new InvalidOperationException ("dataSource does not implement the ICollection interface.");

				int totalRowCount = 0;
				if (haveDataToDisplay) {
					if (view.CanRetrieveTotalRowCount)
						totalRowCount = SelectArguments.TotalRowCount;
					else 
						totalRowCount = ((ICollection) dataSource).Count + _startRowIndex;
				}
				
				pagedDataSource.StartRowIndex = _startRowIndex;
				pagedDataSource.DataSource = dataSource;
				pagedDataSource.TotalRowCount = totalRowCount;
			} else {
				if (!(dataSource is ICollection))
					throw new InvalidOperationException ("dataSource does not implement the ICollection interface and dataBinding is false.");
			}

			if (GroupItemCount <= 0) {
				retList = CreateItemsWithoutGroups (pagedDataSource, dataBinding, InsertItemPosition, DataKeyArray);
				Console.WriteLine ("Data key names:");
				foreach (string s in DataKeyNames)
					Console.WriteLine ("\t{0}", s);
				
				Console.WriteLine ("Keys:");
				foreach (object o in DataKeyArray)
					Console.WriteLine ("\t{0}", o);
			}
			
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
			Control contentPlaceholder = FindPlaceholder (ItemPlaceholderID, _layoutTemplatePlaceholder);
			
			if (contentPlaceholder == null)
				return new List <ListViewDataItem> ();

			Control parent = contentPlaceholder.Parent;
			int ipos = 0;
			
			if (parent != null) {
				ipos = parent.Controls.IndexOf (contentPlaceholder);
				parent.Controls.Remove (contentPlaceholder);
				contentPlaceholder = parent;
				AddControlToContainer (contentPlaceholder, _layoutTemplatePlaceholder, 0);
			}

			List <ListViewDataItem> ret = new List <ListViewDataItem> ();
			ListViewItem lvi;
			ListViewItem container;
			bool needSeparator = false;

			if (insertPosition == InsertItemPosition.FirstItem) {
				lvi = CreateInsertItem ();
				InstantiateInsertItemTemplate (lvi);
				AddControlToContainer (lvi, contentPlaceholder, ipos++);
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
					AddControlToContainer (container, contentPlaceholder, ipos++);
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
				AddControlToContainer (lvdi, contentPlaceholder, ipos++);

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
					AddControlToContainer (container, contentPlaceholder, ipos++);
				}
				
				lvi = CreateInsertItem ();
				InstantiateInsertItemTemplate (lvi);
				AddControlToContainer (lvi, contentPlaceholder, ipos++);
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
	
		protected override void LoadControlState (object savedState)
		{
		}
	
		protected override void LoadViewState (object savedState)
		{
		}
	
		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			throw new NotImplementedException ();
		}
	
		protected override void OnInit (EventArgs e)
		{
		}
	
		protected virtual void OnItemCanceling (ListViewCancelEventArgs e)
		{
		}
	
		protected virtual void OnItemCommand (ListViewCommandEventArgs e)
		{
		}
	
		protected virtual void OnItemCreated (ListViewItemEventArgs e)
		{
		}
	
		protected virtual void OnItemDataBound (ListViewItemEventArgs e)
		{
		}
	
		protected virtual void OnItemDeleted (ListViewDeletedEventArgs e)
		{
		}
	
		protected virtual void OnItemDeleting (ListViewDeleteEventArgs e)
		{
		}
	
		protected virtual void OnItemEditing (ListViewEditEventArgs e)
		{
		}
	
		protected virtual void OnItemInserted (ListViewInsertedEventArgs e)
		{
		}
	
		protected virtual void OnItemInserting (ListViewInsertEventArgs e)
		{
		}
	
		protected virtual void OnItemUpdated (ListViewUpdatedEventArgs e)
		{
		}
	
		protected virtual void OnItemUpdating (ListViewUpdateEventArgs e)
		{
		}
	
		protected virtual void OnLayoutCreated (EventArgs e)
		{
		}
	
		protected virtual void OnPagePropertiesChanged (EventArgs e)
		{
		}
	
		protected virtual void OnPagePropertiesChanging (PagePropertiesChangingEventArgs e)
		{
		}
	
		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
		}
	
		protected virtual void OnSelectedIndexChanging (ListViewSelectEventArgs e)
		{
		}
	
		protected virtual void OnSorted (EventArgs e)
		{
		}
	
		protected virtual void OnSorting (ListViewSortEventArgs e)
		{
		}
	
		protected virtual void OnTotalRowCountAvailable (PageEventArgs e)
		{
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
		}
	
		protected override void Render (HtmlTextWriter writer)
		{
			base.Render (writer);
		}
	
		protected override object SaveControlState ()
		{
			throw new NotImplementedException ();
		}
	
		protected override object SaveViewState ()
		{
			object[] states = new object [2];

			states [0] = base.SaveViewState ();
			states [1] = null;

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
	
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[BrowsableAttribute (false)]
		public override string AccessKey {
			get { return base.AccessKey; }
			set { throw StylingNotSupported (); }
		}
	
		[TemplateContainerAttribute (typeof (ListViewDataItem), BindingDirection.TwoWay)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValueAttribute (null)]
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
		[DefaultValueAttribute (true)]
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
	
		[DefaultValueAttribute (null)]
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

				// They will eventually be recreated when data binding
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
		
		[DefaultValueAttribute (-1)]
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
	
		[DefaultValueAttribute (null)]
		[BrowsableAttribute (false)]
		[TemplateContainerAttribute (typeof (ListViewDataItem), BindingDirection.TwoWay)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public virtual ITemplate EditItemTemplate {
			get { return _editItemTemplate; }
			set { _editItemTemplate = value; }
		}
	
		[DefaultValueAttribute (null)]
		[TemplateContainerAttribute (typeof (ListView))]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[BrowsableAttribute (false)]
		public virtual ITemplate EmptyDataTemplate {
			get { return _emptyDataTemplate; }
			set { _emptyDataTemplate = value; }
		}
	
		[TemplateContainerAttribute (typeof (ListViewItem))]
		[DefaultValueAttribute (null)]
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
		[DefaultValueAttribute (1)]
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
		[DefaultValueAttribute ("groupPlaceholder")]
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
		[DefaultValueAttribute (null)]
		[TemplateContainerAttribute (typeof (ListViewItem))]
		public virtual ITemplate GroupSeparatorTemplate {
			get { return _groupSeparatorTemplate; }
			set { _groupSeparatorTemplate = value; }
		}
	
		[TemplateContainerAttribute (typeof (ListViewItem))]
		[DefaultValueAttribute (null)]
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
		[DefaultValueAttribute (InsertItemPosition.None)]
		public virtual InsertItemPosition InsertItemPosition {
			get;
			set;
		}
	
		[TemplateContainerAttribute (typeof (ListViewItem), BindingDirection.TwoWay)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValueAttribute (null)]
		[BrowsableAttribute (false)]
		public virtual ITemplate InsertItemTemplate {
			get { return _insertItemTemplate; }
			set { _insertItemTemplate = value; }
		}
	
		[DefaultValueAttribute ("itemPlaceholder")]
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
		[DefaultValueAttribute (null)]
		public virtual ITemplate ItemSeparatorTemplate {
			get { return _itemSeparatorTemplate; }
			set { _itemSeparatorTemplate = value; }
		}
	
		[TemplateContainerAttribute (typeof (ListViewDataItem), BindingDirection.TwoWay)]
		[DefaultValueAttribute (null)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[BrowsableAttribute (false)]
		public virtual ITemplate ItemTemplate {
			get { return _itemTemplate; }
			set { _itemTemplate = value; }
		}
	
		[TemplateContainerAttribute (typeof (ListView))]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValueAttribute (null)]
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
		[DefaultValueAttribute (-1)]
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
		[DefaultValueAttribute (null)]
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
	
		[DefaultValueAttribute (SortDirection.Ascending)]
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
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewCancelEventArgs> ItemCanceling;
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewCommandEventArgs> ItemCommand;
	
		[CategoryAttribute ("Behavior")]
		public event EventHandler <ListViewItemEventArgs> ItemCreated;
	
		[CategoryAttribute ("Data")]
		public event EventHandler <ListViewItemEventArgs> ItemDataBound;
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewDeletedEventArgs> ItemDeleted;
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewDeleteEventArgs> ItemDeleting;
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewEditEventArgs> ItemEditing;
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewInsertedEventArgs> ItemInserted;
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewInsertEventArgs> ItemInserting;
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewUpdatedEventArgs> ItemUpdated;
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewUpdateEventArgs> ItemUpdating;
	
		[CategoryAttribute ("Behavior")]
		public event EventHandler LayoutCreated;
	
		[CategoryAttribute ("Behavior")]
		public event EventHandler PagePropertiesChanged;
	
		[CategoryAttribute ("Behavior")]
		public event EventHandler <PagePropertiesChangingEventArgs> PagePropertiesChanging;
	
		[CategoryAttribute ("Action")]
		public event EventHandler SelectedIndexChanged;
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewSelectEventArgs> SelectedIndexChanging;
	
		[CategoryAttribute ("Action")]
		public event EventHandler Sorted;
	
		[CategoryAttribute ("Action")]
		public event EventHandler <ListViewSortEventArgs> Sorting;
	
		event EventHandler <PageEventArgs> IPageableItemContainer.TotalRowCountAvailable {
			add {
			}

			remove {
			}
		}

		NotSupportedException StylingNotSupported ()
		{
			return new NotSupportedException ("Style properties are not supported on ListView. Apply styling or CSS classes to the elements in the ListView's templates.");
		}
	}
}
#endif

