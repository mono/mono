//
// System.Web.UI.WebControls.DataControlField.cs
//
// Authors:
//	Sanjay Gupta (gsanjay@novell.com)
//
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

#if NET_2_0
using System.Collections;
using System.Collections.Specialized;
using System.Web.UI;

namespace System.Web.UI.WebControls {
	public abstract class DataControlField : IStateManager, IDataSourceViewSchemaAccessor
	{
		bool tracking = false;
		StateBag viewState;
		object dataSourceViewSchema;
		string accessibleHeaderText;
		Control control;
		Style controlStyle;
		bool designMode;
		TableItemStyle footerStyle;
		string footerText;
		string headerImageUrl;
		TableItemStyle headerStyle;
		string headerText;
		TableItemStyle itemStyle;
		bool showHeader;
		string sortExpression;
		bool visible;

		protected DataControlField()
		{ 
			viewState = new StateBag ();
		}

		[MonoTODO]
		public virtual void ExtractValuesFromCell (IOrderedDictionary dictionary,
			DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool Initialize (bool sortingEnabled, Control control)
		{ 
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void InitializeCell (DataControlFieldCell cell,
			DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{ 
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnFieldChanged ()
		{
			throw new NotImplementedException ();
		}	
	
		protected virtual void LoadViewState(object savedState)
		{
			ArrayList items = savedState as ArrayList;
			if (items == null)
				return;
			foreach (Pair p in items) {
				if (((string)p.First).Equals("accessibleHeaderText"))				{
					accessibleHeaderText = (string)p.Second;
					continue;
				}
				if (((string)p.First).Equals("footerText")) {
					footerText = (string)p.Second;
					continue;
				}
				if (((string)p.First).Equals("headerImageUrl")) {
					headerImageUrl = (string)p.Second;
					continue;
				}
				if (((string)p.First).Equals("headerText")) {
					headerText = (string)p.Second;
					continue;
				}
				if (((string)p.First).Equals("showHeader")) {
					showHeader = (bool)p.Second;
					continue;
				}
				if (((string)p.First).Equals("sortExpression")) {
					sortExpression = (string)p.Second;
					continue;
				}
				if (((string)p.First).Equals("visible")) {
					visible = (bool)p.Second;
					continue;
				}
				if (((string)p.First).Equals("dataSourceViewSchema"))
				{
					dataSourceViewSchema = p.Second;
					continue;
				}
			}

		}

		protected virtual object SaveViewState()
		{
			ArrayList items = new ArrayList();
			Pair pair = new Pair ();
			if (viewState.IsItemDirty("accessibleHeaderText")) {
				pair.First = "accessibleHeaderText";
				pair.Second = accessibleHeaderText;
				items.Add(pair);
			}
			if (viewState.IsItemDirty("footerText")) {
				pair.First = "footerText";
				pair.Second = footerText;
				items.Add(pair);
			}
			if (viewState.IsItemDirty("headerImageUrl")) {
				pair.First = "headerImageUrl";
				pair.Second = headerImageUrl;
				items.Add(pair);
			}
			if (viewState.IsItemDirty("headerText")) {
				pair.First = "headerText";
				pair.Second = headerText;
				items.Add(pair);
			}
			if (viewState.IsItemDirty("showHeader")) {
				pair.First = "showHeader";
				pair.Second = showHeader;
				items.Add(pair);
			}
			if (viewState.IsItemDirty("sortExpression")) {
				pair.First = "sortExpression";
				pair.Second = sortExpression;
				items.Add(pair);
			}
			if (viewState.IsItemDirty("visible")) {
				pair.First = "visible";
				pair.Second = visible;
				items.Add(pair);
			}
			if (viewState.IsItemDirty("dataSourceViewSchema")) {
				pair.First = "dataSourceViewSchema";
				pair.Second = dataSourceViewSchema;
				items.Add(pair);
			}
			if (items.Count == 0)
				return null;
			else
				return items;
		}

		protected virtual void TrackViewState()
		{
			tracking = true;			
		}
		
		[MonoTODO]
		public virtual void ValidateSupportsCallback ()
		{
			throw new NotImplementedException ();
		}

		void IStateManager.LoadViewState(object savedState)
		{
			LoadViewState(savedState);
		}

		object IStateManager.SaveViewState()
		{
			return SaveViewState();
		}

		void IStateManager.TrackViewState()
		{
			TrackViewState();
		}

		public virtual string AccessibleHeaderText {
			get { return (string) viewState ["accessibleHeaderText"]; }
			set { 
				accessibleHeaderText = value;
				viewState.SetItemDirty ("accessibleHeaderText", true);
			}
		}

		protected Control Control {
			get { return control; }
		}

		public virtual Style ControlStyle {
			get { return controlStyle; }
		}
	
		protected bool DesignMode {
			get { return designMode; }
		}

		public virtual TableItemStyle FooterStyle {
			get { return footerStyle; }
		}

		public virtual string FooterText {
			get { return (string) viewState ["footerText"]; }
			set { 
				footerText = value;
				viewState.SetItemDirty ("footerText", true);
			}
		}

		public virtual string HeaderImageUrl {
			get { return (string) viewState ["headerImageUrl"]; }
			set { 
				headerImageUrl = value;
				viewState.SetItemDirty ("headerImageUrl", true);
			}
		}

		public virtual TableItemStyle HeaderStyle {
			get { return headerStyle; }
		}

		public virtual string HeaderText {
			get { return (string) viewState ["headerText"]; }
			set { 
				headerText = value;
				viewState.SetItemDirty ("headerText", true);
			}
		}

		public virtual TableItemStyle ItemStyle {
			get { return itemStyle; }
		}

		public virtual bool ShowHeader {
			get { return (bool) viewState ["showHeader"]; }
			set { 
				showHeader = value;
				viewState.SetItemDirty ("showHeader", true);
			}
		}

		public virtual string SortExpression {
			get { return (string) viewState ["sortExpression"]; }
			set { 
				sortExpression = value;
				viewState.SetItemDirty ("sortExpression", true);
			}
		}

		public bool Visible {
			get { return (bool) viewState ["visible"]; }
			set { 
				visible = value;
				viewState.SetItemDirty ("visible", true);
			}
		}

		protected bool IsTrackingViewState
		{
			get { return tracking; }
		}

		bool IStateManager.IsTrackingViewState
		{
			get { return IsTrackingViewState; }
		}

		object IDataSourceViewSchemaAccessor.DataSourceViewSchema {
			get { return viewState ["dataSourceViewSchema"]; }
			set { 
				dataSourceViewSchema = value;
				viewState.SetItemDirty ("dataSourceViewSchema", true);
			}
		}		
	}
}
#endif
