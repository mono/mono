//
// System.Web.UI.WebControls.GridView.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	[DesignerAttribute ("System.Web.UI.Design.WebControls.GridViewDesigner, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.IDesigner")]
	[ControlValuePropertyAttribute ("SelectedValue")]
	[DefaultEventAttribute ("SelectedIndexChanged")]
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class GridView: CompositeDataBoundControl, IPostBackEventHandler
	{
		Table table;
		GridViewRowCollection rows;
		GridViewRow headerRow;
		GridViewRow footerRow;
		GridViewRow bottomPagerRow;
		GridViewRow topPagerRow;
		
		IOrderedDictionary currentEditRowKeys;
		IOrderedDictionary currentEditNewValues;
		IOrderedDictionary currentEditOldValues;
			
		// View state
		DataControlFieldCollection columns;
		PagerSettings pagerSettings;
		
		TableItemStyle alternatingRowStyle;
		TableItemStyle editRowStyle;
		TableItemStyle emptyDataRowStyle;
		TableItemStyle footerStyle;
		TableItemStyle headerStyle;
		TableItemStyle pagerStyle;
		TableItemStyle rowStyle;
		TableItemStyle selectedRowStyle;
		DataKeyArray keys;
		DataKey oldEditValues;
		
		private static readonly object PageIndexChangedEvent = new object();
		private static readonly object PageIndexChangingEvent = new object();
		private static readonly object RowCancelingEditEvent = new object();
		private static readonly object RowCommandEvent = new object();
		private static readonly object RowCreatedEvent = new object();
		private static readonly object RowDataBoundEvent = new object();
		private static readonly object RowDeletedEvent = new object();
		private static readonly object RowDeletingEvent = new object();
		private static readonly object RowEditingEvent = new object();
		private static readonly object RowUpdatedEvent = new object();
		private static readonly object RowUpdatingEvent = new object();
		private static readonly object SelectedIndexChangedEvent = new object();
		private static readonly object SelectedIndexChangingEvent = new object();
		private static readonly object SortedEvent = new object();
		private static readonly object SortingEvent = new object();
		
		// Control state
		int pageIndex;
		int pageCount = -1;
		int selectedIndex = -1;
		int editIndex = -1;
		SortDirection sortDirection = SortDirection.Ascending;
		string sortExpression;
		
		public GridView ()
		{
		}
		
		public event EventHandler PageIndexChanged {
			add { Events.AddHandler (PageIndexChangedEvent, value); }
			remove { Events.RemoveHandler (PageIndexChangedEvent, value); }
		}
		
		public event GridViewPageEventHandler PageIndexChanging {
			add { Events.AddHandler (PageIndexChangingEvent, value); }
			remove { Events.RemoveHandler (PageIndexChangingEvent, value); }
		}
		
		public event GridViewCancelEditEventHandler RowCancelingEdit {
			add { Events.AddHandler (RowCancelingEditEvent, value); }
			remove { Events.RemoveHandler (RowCancelingEditEvent, value); }
		}
		
		public event GridViewCommandEventHandler RowCommand {
			add { Events.AddHandler (RowCommandEvent, value); }
			remove { Events.RemoveHandler (RowCommandEvent, value); }
		}
		
		public event GridViewRowEventHandler RowCreated {
			add { Events.AddHandler (RowCreatedEvent, value); }
			remove { Events.RemoveHandler (RowCreatedEvent, value); }
		}
		
		public event GridViewRowEventHandler RowDataBound {
			add { Events.AddHandler (RowDataBoundEvent, value); }
			remove { Events.RemoveHandler (RowDataBoundEvent, value); }
		}
		
		public event GridViewDeletedEventHandler RowDeleted {
			add { Events.AddHandler (RowDeletedEvent, value); }
			remove { Events.RemoveHandler (RowDeletedEvent, value); }
		}
		
		public event GridViewDeleteEventHandler RowDeleting {
			add { Events.AddHandler (RowDeletingEvent, value); }
			remove { Events.RemoveHandler (RowDeletingEvent, value); }
		}
		
		public event GridViewEditEventHandler RowEditing {
			add { Events.AddHandler (RowEditingEvent, value); }
			remove { Events.RemoveHandler (RowEditingEvent, value); }
		}
		
		public event GridViewUpdatedEventHandler RowUpdated {
			add { Events.AddHandler (RowUpdatedEvent, value); }
			remove { Events.RemoveHandler (RowUpdatedEvent, value); }
		}
		
		public event GridViewUpdateEventHandler RowUpdating {
			add { Events.AddHandler (RowUpdatingEvent, value); }
			remove { Events.RemoveHandler (RowUpdatingEvent, value); }
		}
		
		public event EventHandler SelectedIndexChanged {
			add { Events.AddHandler (SelectedIndexChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedIndexChangedEvent, value); }
		}
		
		public event GridViewSelectEventHandler SelectedIndexChanging {
			add { Events.AddHandler (SelectedIndexChangingEvent, value); }
			remove { Events.RemoveHandler (SelectedIndexChangingEvent, value); }
		}
		
		public event EventHandler Sorted {
			add { Events.AddHandler (SortedEvent, value); }
			remove { Events.RemoveHandler (SortedEvent, value); }
		}
		
		public event GridViewSortEventHandler Sorting {
			add { Events.AddHandler (SortingEvent, value); }
			remove { Events.RemoveHandler (SortingEvent, value); }
		}
		
		protected virtual void OnPageIndexChanged (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [PageIndexChangedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnPageIndexChanging (GridViewPageEventArgs e)
		{
			if (Events != null) {
				GridViewPageEventHandler eh = (GridViewPageEventHandler) Events [PageIndexChangingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnRowCancelingEdit (GridViewCancelEditEventArgs e)
		{
			if (Events != null) {
				GridViewCancelEditEventHandler eh = (GridViewCancelEditEventHandler) Events [RowCancelingEditEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnRowCommand (GridViewCommandEventArgs e)
		{
			if (Events != null) {
				GridViewCommandEventHandler eh = (GridViewCommandEventHandler) Events [RowCommandEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnRowCreated (GridViewRowEventArgs e)
		{
			if (Events != null) {
				GridViewRowEventHandler eh = (GridViewRowEventHandler) Events [RowCreatedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnRowDataBound (GridViewRowEventArgs e)
		{
			if (Events != null) {
				GridViewRowEventHandler eh = (GridViewRowEventHandler) Events [RowDataBoundEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnRowDeleted (GridViewDeletedEventArgs e)
		{
			if (Events != null) {
				GridViewDeletedEventHandler eh = (GridViewDeletedEventHandler) Events [RowDeletedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnRowDeleting (GridViewDeleteEventArgs e)
		{
			if (Events != null) {
				GridViewDeleteEventHandler eh = (GridViewDeleteEventHandler) Events [RowDeletingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnRowEditing (GridViewEditEventArgs e)
		{
			if (Events != null) {
				GridViewEditEventHandler eh = (GridViewEditEventHandler) Events [RowEditingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnRowUpdated (GridViewUpdatedEventArgs e)
		{
			if (Events != null) {
				GridViewUpdatedEventHandler eh = (GridViewUpdatedEventHandler) Events [RowUpdatedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnRowUpdating (GridViewUpdateEventArgs e)
		{
			if (Events != null) {
				GridViewUpdateEventHandler eh = (GridViewUpdateEventHandler) Events [RowUpdatingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [SelectedIndexChangedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnSelectedIndexChanging (GridViewSelectEventArgs e)
		{
			if (Events != null) {
				GridViewSelectEventHandler eh = (GridViewSelectEventHandler) Events [SelectedIndexChangingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnSorted (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [SortedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnSorting (GridViewSortEventArgs e)
		{
			if (Events != null) {
				GridViewSortEventHandler eh = (GridViewSortEventHandler) Events [SortingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		
		[WebCategoryAttribute ("Paging")]
		[DefaultValueAttribute (false)]
		public bool AllowPaging {
			get {
				object ob = ViewState ["AllowPaging"];
				if (ob != null) return (bool) ob;
				return false;
			}
			set {
				ViewState ["AllowPaging"] = value;
			}
		}
		
		[WebCategoryAttribute ("Behavior")]
		[DefaultValueAttribute (false)]
		public bool AllowSorting {
			get {
				object ob = ViewState ["AllowSorting"];
				if (ob != null) return (bool) ob;
				return false;
			}
			set {
				ViewState ["AllowSorting"] = value;
			}
		}
		
	    [WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual TableItemStyle AlternatingRowStyle {
			get {
				if (alternatingRowStyle == null) {
					alternatingRowStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						alternatingRowStyle.TrackViewState();
				}
				return alternatingRowStyle;
			}
		}

		[WebCategoryAttribute ("Behavior")]
		[DefaultValueAttribute (false)]
		public virtual bool AutoGenerateEditButton {
			get {
				object ob = ViewState ["AutoGenerateEditButton"];
				if (ob != null) return (bool) ob;
				return false;
			}
			set {
				ViewState ["AutoGenerateEditButton"] = value;
			}
		}

		[WebCategoryAttribute ("Behavior")]
		[DefaultValueAttribute (false)]
		public virtual bool AutoGenerateDeleteButton {
			get {
				object ob = ViewState ["AutoGenerateDeleteButton"];
				if (ob != null) return (bool) ob;
				return false;
			}
			set {
				ViewState ["AutoGenerateDeleteButton"] = value;
			}
		}

		[WebCategoryAttribute ("Behavior")]
		[DefaultValueAttribute (false)]
		public virtual bool AutoGenerateSelectButton {
			get {
				object ob = ViewState ["AutoGenerateSelectButton"];
				if (ob != null) return (bool) ob;
				return false;
			}
			set {
				ViewState ["AutoGenerateSelectButton"] = value;
			}
		}

		[UrlPropertyAttribute]
		[WebCategoryAttribute ("Appearance")]
		[DefaultValueAttribute ("")]
		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public virtual string BackImageUrl {
			get {
				object ob = ViewState ["BackImageUrl"];
				if (ob != null) return (string) ob;
				return string.Empty;
			}
			set {
				ViewState ["BackImageUrl"] = value;
			}
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual GridViewRow BottomPagerRow {
			get {
				EnsureDataBound ();
				return bottomPagerRow;
			}
		}
	
		[WebCategoryAttribute ("Accessibility")]
		[DefaultValueAttribute ("")]
		[LocalizableAttribute (true)]
		public string Caption {
			get {
				object ob = ViewState ["Caption"];
				if (ob != null) return (string) ob;
				return string.Empty;
			}
			set {
				ViewState ["Caption"] = value;
			}
		}
		
		[WebCategoryAttribute ("Accessibility")]
		[DefaultValueAttribute (TableCaptionAlign.NotSet)]
		public virtual TableCaptionAlign CaptionAlign
		{
			get {
				object o = ViewState ["CaptionAlign"];
				if(o != null) return (TableCaptionAlign) o;
				return TableCaptionAlign.NotSet;
			}
			set {
				ViewState ["CaptionAlign"] = value;
			}
		}

		[WebCategoryAttribute ("Layout")]
		[DefaultValueAttribute (-1)]
		public virtual int CellPadding
		{
			get {
				object o = ViewState ["CellPadding"];
				if (o != null) return (int) o;
				return -1;
			}
			set {
				ViewState ["CellPadding"] = value;
			}
		}

		[WebCategoryAttribute ("Layout")]
		[DefaultValueAttribute (0)]
		public virtual int CellSpacing
		{
			get {
				object o = ViewState ["CellSpacing"];
				if (o != null) return (int) o;
				return 0;
			}
			set {
				ViewState ["CellSpacing"] = value;
			}
		}
		
		[EditorAttribute ("System.Web.UI.Design.WebControls.DataControlFieldTypeEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[MergablePropertyAttribute (false)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValueAttribute (null)]
		[WebCategoryAttribute ("Misc")]
		public virtual DataControlFieldCollection Columns {
			get {
				if (columns == null) {
					columns = new DataControlFieldCollection ();
					columns.FieldsChanged += new EventHandler (OnFieldsChanged);
					if (IsTrackingViewState)
						((IStateManager)columns).TrackViewState ();
				}
				return columns;
			}
		}

		[DefaultValueAttribute (null)]
		[WebCategoryAttribute ("Data")]
		[TypeConverter (typeof(StringArrayConverter))]
		[EditorAttribute ("System.Web.UI.Design.WebControls.DataFieldEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public virtual string[] DataKeyNames
		{
			get {
				object o = ViewState ["DataKeyNames"];
				if (o != null) return (string[]) o;
				return null;
			}
			set {
				ViewState ["DataKeyNames"] = value;
			}
		}
		
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public virtual DataKeyArray DataKeys {
			get {
				EnsureDataBound ();
				return keys;
			}
		}

		[WebCategoryAttribute ("Misc")]
		[DefaultValueAttribute (-1)]
		public int EditIndex {
			get {
				return editIndex;
			}
			set {
				editIndex = value;
				RequireBinding ();
			}
		}
	
	    [WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual TableItemStyle EditRowStyle {
			get {
				if (editRowStyle == null) {
					editRowStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						editRowStyle.TrackViewState();
				}
				return editRowStyle;
			}
		}
		
	    [WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual TableItemStyle EmptyDataRowStyle {
			get {
				if (emptyDataRowStyle == null) {
					emptyDataRowStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						emptyDataRowStyle.TrackViewState();
				}
				return emptyDataRowStyle;
			}
		}
		
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual GridViewRow FooterRow {
			get {
				if (footerRow == null)
					footerRow = CreateRow (0, 0, DataControlRowType.Footer, DataControlRowState.Normal);
				return footerRow;
			}
		}
	
	    [WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual TableItemStyle FooterStyle {
			get {
				if (footerStyle == null) {
					footerStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						footerStyle.TrackViewState();
				}
				return footerStyle;
			}
		}
		
		[WebCategoryAttribute ("Appearance")]
		[DefaultValueAttribute (GridLines.Both)]
		public virtual GridLines GridLines {
			get {
				object ob = ViewState ["GridLines"];
				if (ob != null) return (GridLines) ob;
				return GridLines.Both;
			}
			set {
				ViewState ["GridLines"] = value;
			}
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual GridViewRow HeaderRow {
			get {
				if (headerRow == null)
					headerRow = CreateRow (0, 0, DataControlRowType.Header, DataControlRowState.Normal);
				return headerRow;
			}
		}
	
	    [WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual TableItemStyle HeaderStyle {
			get {
				if (headerStyle == null) {
					headerStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						headerStyle.TrackViewState();
				}
				return headerStyle;
			}
		}
		
		[DefaultValueAttribute (HorizontalAlign.NotSet)]
		public virtual HorizontalAlign HorizontalAlign {
			get {
				object ob = ViewState ["HorizontalAlign"];
				if (ob != null) return (HorizontalAlign) ob;
				return HorizontalAlign.NotSet;
			}
			set {
				ViewState ["HorizontalAlign"] = value;
			}
		}

		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public int PageCount {
			get {
				if (pageCount != -1) return pageCount;
				EnsureDataBound ();
				if (SelectArguments.TotalRowCount == 0) pageCount = 1;
				else pageCount = ((SelectArguments.TotalRowCount - 1) / PageSize) + 1;
				return pageCount;
			}
		}

		[WebCategoryAttribute ("Paging")]
		[BrowsableAttribute (true)]
		[DefaultValueAttribute (0)]
		public int PageIndex {
			get {
				return pageIndex;
			}
			set {
				pageIndex = value;
				RequireBinding ();
			}
		}
	
		[WebCategoryAttribute ("Paging")]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
		[NotifyParentPropertyAttribute (true)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public PagerSettings PagerSettings {
			get {
				if (pagerSettings == null) {
					pagerSettings = new PagerSettings (this);
					if (IsTrackingViewState)
						((IStateManager)pagerSettings).TrackViewState ();
				}
				return pagerSettings;
			}
		}
	
	    [WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual TableItemStyle PagerStyle {
			get {
				if (pagerStyle == null) {
					pagerStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						pagerStyle.TrackViewState();
				}
				return pagerStyle;
			}
		}
		[DefaultValueAttribute (10)]
		[WebCategoryAttribute ("Paging")]
		public int PageSize {
			get {
				object ob = ViewState ["PageSize"];
				if (ob != null) return (int) ob;
				return 10;
			}
			set {
				ViewState ["PageSize"] = value;
			}
		}
	
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual GridViewRowCollection Rows {
			get {
				EnsureDataBound ();
				return rows;
			}
		}
		
	    [WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual TableItemStyle RowStyle {
			get {
				if (rowStyle == null) {
					rowStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						rowStyle.TrackViewState();
				}
				return rowStyle;
			}
		}
		
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public virtual DataKey SelectedDataKey {
			get {
				if (selectedIndex >= 0 && selectedIndex < DataKeys.Count) {
					return DataKeys [selectedIndex];
				} else
					return null;
			}
		}
		
		[BindableAttribute (true)]
		[DefaultValueAttribute (-1)]
		public int SelectedIndex {
			get {
				return selectedIndex;
			}
			set {
				if (selectedIndex >= 0 && selectedIndex < Rows.Count) {
					int oldIndex = selectedIndex;
					selectedIndex = -1;
					Rows [oldIndex].RowState = GetRowState (oldIndex);
				}
				selectedIndex = value;
				if (selectedIndex >= 0 && selectedIndex < Rows.Count) {
					Rows [selectedIndex].RowState = GetRowState (selectedIndex);
				}
			}
		}
	
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public virtual GridViewRow SelectedRow {
			get {
				if (selectedIndex >= 0 && selectedIndex < Rows.Count) {
					return Rows [selectedIndex];
				} else
					return null;
			}
		}
		
	    [WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual TableItemStyle SelectedRowStyle {
			get {
				if (selectedRowStyle == null) {
					selectedRowStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						selectedRowStyle.TrackViewState();
				}
				return selectedRowStyle;
			}
		}
		
		[BrowsableAttribute (false)]
		public virtual object SelectedValue {
			get { return SelectedDataKey.Value; }
		}
		
		[WebCategoryAttribute ("Appearance")]
		[DefaultValueAttribute (false)]
		public virtual bool ShowFooter {
			get {
				object ob = ViewState ["ShowFooter"];
				if (ob != null) return (bool) ob;
				return false;
			}
			set {
				ViewState ["ShowFooter"] = value;
			}
		}
	
		[WebCategoryAttribute ("Appearance")]
		[DefaultValueAttribute (true)]
		public virtual bool ShowHeader {
			get {
				object ob = ViewState ["ShowHeader"];
				if (ob != null) return (bool) ob;
				return true;
			}
			set {
				ViewState ["ShowHeader"] = value;
			}
		}
		
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[BrowsableAttribute (false)]
		[DefaultValueAttribute (SortDirection.Ascending)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public virtual SortDirection SortDirection {
			get { return sortDirection; }
		}
		
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public virtual string SortExpression {
			get { return sortExpression; }
		}
		
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual GridViewRow TopPagerRow {
			get {
				EnsureDataBound ();
				return topPagerRow;
			}
		}
	
		public virtual bool IsBindableType (Type type)
		{
			return type.IsPrimitive || type == typeof(DateTime) || type == typeof(Guid);
		}
		
		protected override DataSourceSelectArguments CreateDataSourceSelectArguments ()
		{
			return base.CreateDataSourceSelectArguments ();
		}
		
		protected virtual GridViewRow CreateRow (int rowIndex, int dataSourceIndex, DataControlRowType rowType, DataControlRowState rowState)
		{
			GridViewRow row = new GridViewRow (rowIndex, dataSourceIndex, rowType, rowState);
			OnRowCreated (new GridViewRowEventArgs (row));
			return row;
		}
		
		void RequireBinding ()
		{
			RequiresDataBinding = true;
			pageCount = -1;
		}
		
		protected virtual ChildTable CreateChildTable ()
		{
			ChildTable table = new ChildTable ();
			table.Caption = Caption;
			table.CaptionAlign = CaptionAlign;
			table.CellPadding = CellPadding;
			table.CellSpacing = CellSpacing;
			table.HorizontalAlign = HorizontalAlign;
			table.BackImageUrl = BackImageUrl;
			return table;
		}
	
		protected override int CreateChildControls (IEnumerable dataSource, bool dataBinding)
		{
			bool showPager = AllowPaging && (PageCount > 1);
			
			Controls.Clear ();
			table = CreateChildTable ();
			Controls.Add (table);
				
			ArrayList list = new ArrayList ();
			ArrayList keyList = new ArrayList ();

			DataControlField[] fields = new DataControlField [Columns.Count];
			Columns.CopyTo (fields, 0);
			foreach (DataControlField field in fields)
				field.Initialize (AllowSorting, this);
			
			if (showPager && PagerSettings.Position == PagerPosition.Top || PagerSettings.Position == PagerPosition.TopAndBottom) {
				topPagerRow = CreatePagerRow ();
				table.Rows.Add (topPagerRow);
			}
				
			if (ShowHeader) {
				headerRow = CreateRow (0, 0, DataControlRowType.Header, DataControlRowState.Normal);
				table.Rows.Add (headerRow);
				InitializeRow (headerRow, fields);
			}
			
			int n = 0;
			foreach (object obj in dataSource) {
				DataControlRowState rstate = GetRowState (list.Count);
				GridViewRow row = CreateRow (list.Count, list.Count, DataControlRowType.DataRow, rstate);
				row.DataItem = obj;
				list.Add (row);
				table.Rows.Add (row);
				InitializeRow (row, fields);
				if (dataBinding) {
					row.DataBind ();
					OnRowDataBound (new GridViewRowEventArgs (row));
					if (EditIndex == row.RowIndex)
						oldEditValues = new DataKey (GetRowValues (row));
					keyList.Add (new DataKey (CreateRowDataKey (row), DataKeyNames));
				} else {
					if (EditIndex == row.RowIndex)
						oldEditValues = new DataKey (new OrderedDictionary ());
					keyList.Add (new DataKey (new OrderedDictionary (), DataKeyNames));
				}

				if (n >= PageSize)
					break;
			}

			if (ShowFooter) {
				footerRow = CreateRow (0, 0, DataControlRowType.Footer, DataControlRowState.Normal);
				table.Rows.Add (footerRow);
				InitializeRow (footerRow, fields);
			}

			if (showPager && PagerSettings.Position == PagerPosition.Bottom || PagerSettings.Position == PagerPosition.TopAndBottom) {
				bottomPagerRow = CreatePagerRow ();
				table.Rows.Add (bottomPagerRow);
			}

			rows = new GridViewRowCollection (list);
			keys = new DataKeyArray (keyList);
			
			return list.Count;
		}
		
		DataControlRowState GetRowState (int index)
		{
			DataControlRowState rstate = (index % 2) == 0 ? DataControlRowState.Normal : DataControlRowState.Alternate;
			if (index == SelectedIndex) rstate |= DataControlRowState.Selected;
			if (index == EditIndex) rstate |= DataControlRowState.Edit;
			return rstate;
		}
		
		GridViewRow CreatePagerRow ()
		{
			GridViewRow row = CreateRow (-1, -1, DataControlRowType.Pager, DataControlRowState.Normal);
			TableCell cell = new TableCell ();
			cell.ColumnSpan = row.Cells.Count;
			cell.Controls.Add (PagerSettings.CreatePagerControl (PageIndex, PageCount));
			row.Cells.Add (cell);
			return row;
		}
		
		protected virtual void InitializeRow (GridViewRow row, DataControlField[] fields)
		{
			DataControlCellType ctype;

			switch (row.RowType) {
				case DataControlRowType.Header: ctype = DataControlCellType.Header; break;
				case DataControlRowType.Footer: ctype = DataControlCellType.Footer; break;
				default: ctype = DataControlCellType.DataCell; break;
			}
			
			if (AutoGenerateEditButton || AutoGenerateDeleteButton || AutoGenerateSelectButton) {
				TableCell cell = new TableCell ();
				row.Cells.Add (cell);
				
				if (ctype == DataControlCellType.DataCell)
				{
					if ((row.RowState & DataControlRowState.Edit) != 0)
					{
						HyperLink link = new HyperLink ();
						link.Text = "Update";
						link.NavigateUrl = Page.GetPostBackClientHyperlink (this, "update");
						cell.Controls.Add (link);
						
						Literal lit = new Literal ();
						lit.Text = "&nbsp;";
						cell.Controls.Add (lit);
						
						link = new HyperLink ();
						link.Text = "Cancel";
						link.NavigateUrl = Page.GetPostBackClientHyperlink (this, "cancel");
						cell.Controls.Add (link);
					}
					else
					{
						if (AutoGenerateEditButton) {
							HyperLink link = new HyperLink ();
							link.Text = "Edit";
							link.NavigateUrl = Page.GetPostBackClientHyperlink (this, "edit$" + row.RowIndex);
							cell.Controls.Add (link);
						}
						if (AutoGenerateDeleteButton) {
							HyperLink link = new HyperLink ();
							link.Text = "Delete";
							link.NavigateUrl = Page.GetPostBackClientHyperlink (this, "delete$" + row.RowIndex);
							if (cell.Controls.Count > 0) {
								Literal lit = new Literal ();
								lit.Text = "&nbsp;";
								cell.Controls.Add (lit);
							}
							cell.Controls.Add (link);
						}
						if (AutoGenerateSelectButton) {
							HyperLink link = new HyperLink ();
							link.Text = "Select";
							link.NavigateUrl = Page.GetPostBackClientHyperlink (this, "select$" + row.RowIndex);
							if (cell.Controls.Count > 0) {
								Literal lit = new Literal ();
								lit.Text = "&nbsp;";
								cell.Controls.Add (lit);
							}
							cell.Controls.Add (link);
						}
					 }
				}
			}

			for (int n=0; n<fields.Length; n++) {
				DataControlField field = fields [n];
				DataControlFieldCell cell = new DataControlFieldCell (field);
				row.Cells.Add (cell);
				field.InitializeCell (cell, ctype, row.RowState, row.RowIndex);
			}
		}
		
		IOrderedDictionary CreateRowDataKey (GridViewRow row)
		{
			OrderedDictionary dic = new OrderedDictionary ();
			ICustomTypeDescriptor desc = row.DataItem as ICustomTypeDescriptor;
			if (desc != null && DataKeyNames != null) {
				PropertyDescriptorCollection props = desc.GetProperties ();
				foreach (string key in DataKeyNames) {
					PropertyDescriptor prop = props [key];
					dic [key] = prop.GetValue (row.DataItem);
				}
			}
			return dic;
		}
		
		IOrderedDictionary GetRowValues (GridViewRow row)
		{
			OrderedDictionary dic = new OrderedDictionary ();
			
			foreach (TableCell cell in row.Cells) {
				DataControlFieldCell c = cell as DataControlFieldCell;
				if (c != null)
					c.ContainingField.ExtractValuesFromCell (dic, c, row.RowState, true);
			}
			
			return dic;
		}
		
		public sealed override void DataBind ()
		{
			if (AllowPaging) {
				SelectArguments.StartRowIndex = PageIndex * PageSize;
				SelectArguments.MaximumRows = PageSize;
				SelectArguments.RetrieveTotalRowCount = true;
				SelectArguments.SortExpression = sortExpression;
			}
			base.DataBind ();
		}
		
		protected override void PerformDataBinding (IEnumerable data)
		{
			base.PerformDataBinding (data);
		}
		
		protected override void OnInit (EventArgs e)
		{
			Page.RegisterRequiresControlState (this);
			base.OnInit (e);
		}
		
		void OnFieldsChanged (object sender, EventArgs args)
		{
			if (Initialized)
				RequireBinding ();
		}
		
		protected override void OnDataPropertyChanged ()
		{
			base.OnDataPropertyChanged ();
			RequireBinding ();
		}
		
		protected override void OnDataSourceViewChanged (object sender, EventArgs e)
		{
			base.OnDataSourceViewChanged (sender, e);
			RequireBinding ();
		}
		
		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			GridViewCommandEventArgs args = e as GridViewCommandEventArgs;
			if (args != null)
				OnRowCommand (args);
			return base.OnBubbleEvent (source, e);
		}
		
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}
		
		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			string eventName;
			string param;
			
			int i = eventArgument.IndexOf ('$');
			if (i != -1) {
				eventName = eventArgument.Substring (0, i);
				param = eventArgument.Substring (i + 1);
			} else {
				eventName = eventArgument;
				param = null;
			}
			
			switch (eventName)
			{
				case "page":
					int newIndex = -1;
					switch (param) {
						case "first":
							newIndex = 0;
							break;
						case "last":
							newIndex = PageCount - 1;
							break;
						case "next":
							if (PageIndex < PageCount - 1) newIndex = PageIndex + 1;
							break;
						case "prev":
							if (PageIndex > 0) newIndex = PageIndex - 1;
							break;
						default:
							newIndex = int.Parse (param);
							break;
					}
					ShowPage (newIndex);
					break;
					
				case "select":
					SelectRow (int.Parse (param));
					break;
					
				case "edit":
					EditRow (int.Parse (param));
					break;
					
				case "update":
					UpdateRow ();
					break;
					
				case "cancel":
					CancelEdit ();
					break;
					
				case "delete":
					DeleteRow (int.Parse (param));
					break;
					
				case "sort":
					Sort (param);
					break;
			}
		}
		
		void Sort (string newSortExpression)
		{
			SortDirection newDirection;
			if (sortExpression == newSortExpression) {
				if (sortDirection == SortDirection.Ascending)
					newDirection = SortDirection.Descending;
				else
					newDirection = SortDirection.Ascending;
			} else
				newDirection = sortDirection;
			
			GridViewSortEventArgs args = new GridViewSortEventArgs (newSortExpression, newDirection);
			OnSorting (args);
			if (args.Cancel) return;
			
			sortExpression = args.SortExpression;
			sortDirection = args.SortDirection;
			RequireBinding ();
			
			OnSorted (EventArgs.Empty);
		}
		
		void SelectRow (int index)
		{
			GridViewSelectEventArgs args = new GridViewSelectEventArgs (index);
			OnSelectedIndexChanging (args);
			if (!args.Cancel) {
				SelectedIndex = args.NewSelectedIndex;
				OnSelectedIndexChanged (EventArgs.Empty);
			}
		}
		
		void ShowPage (int newIndex)
		{
			GridViewPageEventArgs args = new GridViewPageEventArgs (newIndex);
			OnPageIndexChanging (args);
			if (!args.Cancel) {
				EndRowEdit ();
				PageIndex = args.NewPageIndex;
				OnPageIndexChanged (EventArgs.Empty);
			}
		}
		
		void EditRow (int index)
		{
			GridViewEditEventArgs args = new GridViewEditEventArgs (index);
			OnRowEditing (args);
			if (!args.Cancel) {
				EditIndex = args.NewEditIndex;
			}
		}
		
		void CancelEdit ()
		{
			GridViewCancelEditEventArgs args = new GridViewCancelEditEventArgs (EditIndex);
			OnRowCancelingEdit (args);
			if (!args.Cancel) {
				EndRowEdit ();
			}
		}

		void UpdateRow ()
		{
			GridViewRow row = Rows [EditIndex];
			currentEditRowKeys = DataKeys [EditIndex].Values;
			currentEditNewValues = GetRowValues (row);
			currentEditOldValues = oldEditValues.Values;
			
			GridViewUpdateEventArgs args = new GridViewUpdateEventArgs (EditIndex, currentEditRowKeys, currentEditOldValues, currentEditNewValues);
			OnRowUpdating (args);
			if (!args.Cancel) {
				DataSourceView view = GetData ();
				if (view != null)
					view.Update (currentEditRowKeys, currentEditNewValues, currentEditOldValues, new DataSourceViewOperationCallback (UpdateCallback));
				else {
					GridViewUpdatedEventArgs dargs = new GridViewUpdatedEventArgs (0, null, currentEditRowKeys, currentEditOldValues, currentEditNewValues);
					OnRowUpdated (dargs);
					if (!dargs.KeepInEditMode)				
						EndRowEdit ();
				}
			} else
				EndRowEdit ();
		}

        bool UpdateCallback (int recordsAffected, Exception exception)
		{
			GridViewUpdatedEventArgs dargs = new GridViewUpdatedEventArgs (recordsAffected, exception, currentEditRowKeys, currentEditOldValues, currentEditNewValues);
			OnRowUpdated (dargs);

			if (!dargs.KeepInEditMode)				
				EndRowEdit ();

			return dargs.ExceptionHandled;
		}
		
		void DeleteRow (int rowIndex)
		{
			GridViewRow row = Rows [rowIndex];
			currentEditRowKeys = DataKeys [rowIndex].Values;
			currentEditNewValues = GetRowValues (row);
			
			GridViewDeleteEventArgs args = new GridViewDeleteEventArgs (rowIndex, currentEditRowKeys, currentEditNewValues);
			OnRowDeleting (args);

			if (!args.Cancel) {
				DataSourceView view = GetData ();
				if (view != null)
					view.Delete (currentEditRowKeys, currentEditNewValues, new DataSourceViewOperationCallback (DeleteCallback));
				else {
					GridViewDeletedEventArgs dargs = new GridViewDeletedEventArgs (0, null, currentEditRowKeys, currentEditNewValues);
					OnRowDeleted (dargs);
				}
			}
		}

        bool DeleteCallback (int recordsAffected, Exception exception)
		{
			GridViewDeletedEventArgs dargs = new GridViewDeletedEventArgs (recordsAffected, exception, currentEditRowKeys, currentEditNewValues);
			OnRowDeleted (dargs);
			return dargs.ExceptionHandled;
		}
		
		void EndRowEdit ()
		{
			EditIndex = -1;
			oldEditValues = new DataKey (new OrderedDictionary ());
			currentEditRowKeys = null;
			currentEditOldValues = null;
			currentEditNewValues = null;
		}

		protected internal override void LoadControlState (object ob)
		{
			if (ob == null) return;
			object[] state = (object[]) ob;
			base.LoadControlState (state[0]);
			pageIndex = (int) state[1];
			pageCount = (int) state[2];
			selectedIndex = (int) state[3];
			editIndex = (int) state[4];
			sortExpression = (string) state[5];
			sortDirection = (SortDirection) state[6];
		}
		
		protected internal override object SaveControlState ()
		{
			object bstate = base.SaveControlState ();
			return new object[] {
				bstate, pageIndex, pageCount, selectedIndex, editIndex, sortExpression, sortDirection
			};
		}
		
		protected override void TrackViewState()
		{
			base.TrackViewState();
			if (columns != null) ((IStateManager)columns).TrackViewState();
			if (pagerSettings != null) ((IStateManager)pagerSettings).TrackViewState();
			if (alternatingRowStyle != null) ((IStateManager)alternatingRowStyle).TrackViewState();
			if (footerStyle != null) ((IStateManager)footerStyle).TrackViewState();
			if (headerStyle != null) ((IStateManager)headerStyle).TrackViewState();
			if (pagerStyle != null) ((IStateManager)pagerStyle).TrackViewState();
			if (rowStyle != null) ((IStateManager)rowStyle).TrackViewState();
			if (selectedRowStyle != null) ((IStateManager)selectedRowStyle).TrackViewState();
			if (editRowStyle != null) ((IStateManager)editRowStyle).TrackViewState();
			if (emptyDataRowStyle != null) ((IStateManager)emptyDataRowStyle).TrackViewState();
			if (keys != null) ((IStateManager)keys).TrackViewState();
		}

		protected override object SaveViewState()
		{
			object[] states = new object [13];
			states[0] = base.SaveViewState();
			states[1] = (columns == null ? null : ((IStateManager)columns).SaveViewState());
			states[2] = (pagerSettings == null ? null : ((IStateManager)pagerSettings).SaveViewState());
			states[3] = (alternatingRowStyle == null ? null : ((IStateManager)alternatingRowStyle).SaveViewState());
			states[4] = (footerStyle == null ? null : ((IStateManager)footerStyle).SaveViewState());
			states[5] = (headerStyle == null ? null : ((IStateManager)headerStyle).SaveViewState());
			states[6] = (pagerStyle == null ? null : ((IStateManager)pagerStyle).SaveViewState());
			states[7] = (rowStyle == null ? null : ((IStateManager)rowStyle).SaveViewState());
			states[8] = (selectedRowStyle == null ? null : ((IStateManager)selectedRowStyle).SaveViewState());
			states[9] = (editRowStyle == null ? null : ((IStateManager)editRowStyle).SaveViewState());
			states[10] = (emptyDataRowStyle == null ? null : ((IStateManager)emptyDataRowStyle).SaveViewState());
			states[11] = (keys == null ? null : ((IStateManager)keys).SaveViewState());
			states[12] = (oldEditValues == null ? null : ((IStateManager)oldEditValues).SaveViewState());

			for (int i = states.Length - 1; i >= 0; i--) {
				if (states [i] != null)
					return states;
			}

			return null;
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState == null)
				return;

			object [] states = (object []) savedState;
			base.LoadViewState (states[0]);
			
			EnsureChildControls ();
			
			if (states[1] != null) ((IStateManager)Columns).LoadViewState (states[1]);
			if (states[2] != null) ((IStateManager)PagerSettings).LoadViewState (states[2]);
			if (states[3] != null) ((IStateManager)AlternatingRowStyle).LoadViewState (states[3]);
			if (states[4] != null) ((IStateManager)FooterStyle).LoadViewState (states[4]);
			if (states[5] != null) ((IStateManager)HeaderStyle).LoadViewState (states[5]);
			if (states[6] != null) ((IStateManager)PagerStyle).LoadViewState (states[6]);
			if (states[7] != null) ((IStateManager)RowStyle).LoadViewState (states[7]);
			if (states[8] != null) ((IStateManager)SelectedRowStyle).LoadViewState (states[8]);
			if (states[9] != null) ((IStateManager)EditRowStyle).LoadViewState (states[9]);
			if (states[10] != null) ((IStateManager)EmptyDataRowStyle).LoadViewState (states[10]);
			if (states[11] != null) ((IStateManager)DataKeys).LoadViewState (states[11]);
			if (states[12] != null && oldEditValues != null) ((IStateManager)oldEditValues).LoadViewState (states[12]);
		}
		
		protected override void Render (HtmlTextWriter writer)
		{
			switch (GridLines) {
				case GridLines.Horizontal:
					writer.AddAttribute (HtmlTextWriterAttribute.Rules, "rows");
					writer.AddAttribute (HtmlTextWriterAttribute.Border, "1");
					break;
				case GridLines.Vertical:
					writer.AddAttribute (HtmlTextWriterAttribute.Rules, "cols");
					writer.AddAttribute (HtmlTextWriterAttribute.Border, "1");
					break;
				case GridLines.Both:
					writer.AddAttribute (HtmlTextWriterAttribute.Rules, "all");
					writer.AddAttribute (HtmlTextWriterAttribute.Border, "1");
					break;
				default:
					writer.AddAttribute (HtmlTextWriterAttribute.Border, "0");
					break;
			}
			
			writer.AddAttribute (HtmlTextWriterAttribute.Cellspacing, "0");
			writer.AddStyleAttribute (HtmlTextWriterStyle.BorderCollapse, "collapse");
			table.RenderBeginTag (writer);
			
			foreach (GridViewRow row in table.Rows)
			{
				switch (row.RowType) {
					case DataControlRowType.Header:
						if (headerStyle != null)headerStyle.AddAttributesToRender (writer, row);
						break;
					case DataControlRowType.Footer:
						if (footerStyle != null) footerStyle.AddAttributesToRender (writer, row);
						break;
					case DataControlRowType.Pager:
						if (pagerStyle != null) pagerStyle.AddAttributesToRender (writer, row);
						break;
					case DataControlRowType.EmptyDataRow:
						if (emptyDataRowStyle != null) emptyDataRowStyle.AddAttributesToRender (writer, row);
						break;
					default:
						if (rowStyle != null) rowStyle.AddAttributesToRender (writer, row);
						break;
				}

				if ((row.RowState & DataControlRowState.Alternate) != 0 && alternatingRowStyle != null)
					alternatingRowStyle.AddAttributesToRender (writer, row);
				if ((row.RowState & DataControlRowState.Edit) != 0 && editRowStyle != null)
					editRowStyle.AddAttributesToRender (writer, row);
				if ((row.RowState & DataControlRowState.Selected) != 0 && selectedRowStyle != null)
					selectedRowStyle.AddAttributesToRender (writer, row);
				
				row.RenderBeginTag (writer);
				
				foreach (TableCell cell in row.Cells) {
					DataControlFieldCell fcell = cell as DataControlFieldCell;
					if (fcell != null) {
						Style cellStyle = null;
						switch (row.RowType) {
							case DataControlRowType.Header: cellStyle = fcell.ContainingField.HeaderStyle; break;
							case DataControlRowType.Footer: cellStyle = fcell.ContainingField.FooterStyle; break;
							default: cellStyle = fcell.ContainingField.ItemStyle; break;
						}
						if (cellStyle != null)
							cellStyle.AddAttributesToRender (writer, cell);
					}
					cell.Render (writer);
				}
				row.RenderEndTag (writer);
			}
			table.RenderEndTag (writer);
		}
	}
}

#endif
