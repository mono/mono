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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.Collections;
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
	public class GridView: CompositeDataBoundControl
	{
		Table table;
		GridViewRowCollection rows;
		GridViewRow headerRow;
		GridViewRow footerRow;
		GridViewRow bottomPagerRow;
		GridViewRow topPagerRow;
		
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
		
		// Control state
		int pageIndex;
		
		public GridView ()
		{
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
		
	    [WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
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
		
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual GridViewRow BottomPagerRow {
			get {
				if (bottomPagerRow == null)
					bottomPagerRow = CreateRow (0, 0, DataControlRowType.Pager, DataControlRowState.Normal);
				return bottomPagerRow;
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
	
	    [WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
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
		[DefaultValue (null)]
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
		
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public int PageCount {
			get { return (SelectArguments.TotalRowCount / PageSize) + 1; }
		}

		[WebCategoryAttribute ("Paging")]
		[BrowsableAttribute (true)]
		[DefaultValueAttribute (0)]
		public int PageIndex {
			get { return pageIndex; }
			set { pageIndex = value; }
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
		[DefaultValue (null)]
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
		[DefaultValue (null)]
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
		
	    [WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
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
		
		public virtual bool IsBindableType (Type type)
		{
			return type.IsPrimitive || type == typeof(DateTime) || type == typeof(Guid);
		}
		
		protected override DataSourceSelectArguments CreateDataSourceSelectArguments ()
		{
			DataSourceSelectArguments args = base.CreateDataSourceSelectArguments ();
			if (AllowPaging) {
				args.StartRowIndex = PageIndex * PageSize;
				args.MaximumRows = PageSize;
			}
			return args;
		}
	
		protected virtual GridViewRow CreateRow (int rowIndex, int dataSourceIndex, DataControlRowType rowType, DataControlRowState rowState)
		{
			return new GridViewRow (rowIndex, dataSourceIndex, rowType, rowState);
		}
	
		protected override int CreateChildControls (IEnumerable dataSource, bool dataBinding)
		{
			table = new Table ();
			Controls.Add (table);

			ArrayList list = new ArrayList ();
			DataControlField[] fields = new DataControlField [Columns.Count];
			Columns.CopyTo (fields, 0);
			
			if (ShowHeader) {
				table.Rows.Add (HeaderRow);
				InitializeRow (HeaderRow, fields);
			}
				
			foreach (object obj in dataSource) {
				DataControlRowState rstate = (list.Count % 2) == 0 ? DataControlRowState.Normal : DataControlRowState.Alternate;
				GridViewRow row = CreateRow (list.Count, list.Count, DataControlRowType.DataRow, rstate);
				row.DataItem = obj;
				list.Add (row);
				table.Rows.Add (row);
				InitializeRow (row, fields);
				if (dataBinding)
					row.DataBind ();
			}
			
			if (ShowFooter) {
				table.Rows.Add (FooterRow);
				InitializeRow (FooterRow, fields);
			}
				
			rows = new GridViewRowCollection (list);
			
			return list.Count;
		}
		
		protected virtual void InitializeRow (GridViewRow row, DataControlField[] fields)
		{
			for (int n=0; n<fields.Length; n++) {
				DataControlField field = fields [n];
				DataControlFieldCell cell = new DataControlFieldCell (field);
				row.Cells.Add (cell);
				field.InitializeCell (cell, DataControlCellType.DataCell, row.RowState, row.RowIndex);
			}
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
			RequiresDataBinding = true;
		}
		
		protected internal override void LoadControlState (object ob)
		{
			if (ob == null) return;
			object[] state = (object[]) ob;
			base.LoadControlState (state[0]);
			pageIndex = (int) state[1];
		}
		
		protected internal override object SaveControlState ()
		{
			object bstate = base.SaveControlState ();
			object mstate = pageIndex;
			
			if (bstate != null || mstate != null)
				return new object[] { bstate, mstate };
			else
				return null;
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
		}

		protected override object SaveViewState()
		{
			object[] states = new object [11];
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
		}
		
		protected override void Render (HtmlTextWriter writer)
		{
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
					} else {
						cell.Render (writer);
					}
				}
			}
			table.RenderEndTag (writer);
		}
	}
}

#endif
