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
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	[DefaultPropertyAttribute ("HeaderText")]
	[TypeConverterAttribute (typeof(ExpandableObjectConverter))]
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class DataControlField : IStateManager, IDataSourceViewSchemaAccessor
	{
		bool tracking = false;
		StateBag viewState;
		Control control;
		Style controlStyle;
		TableItemStyle footerStyle;
		TableItemStyle headerStyle;
		TableItemStyle itemStyle;
		
		protected DataControlField()
		{ 
			viewState = new StateBag ();
		}
		
		internal void SetDirty ()
		{
			viewState.SetDirty ();
		}
		
		protected StateBag ViewState {
			get { return viewState; }
		}

		public virtual void ExtractValuesFromCell (IOrderedDictionary dictionary,
			DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
		{
		}

		public virtual bool Initialize (bool sortingEnabled, Control control)
		{
			this.control = control;
			return true;
		}

		public virtual void InitializeCell (DataControlFieldCell cell,
			DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			if (cellType == DataControlCellType.Header && ShowHeader) {
				if (HeaderImageUrl != "") {
					Image img = new Image ();
					img.ImageUrl = HeaderImageUrl;
					cell.Controls.Add (img);
					if (HeaderText != "") {
						Label label = new Label ();
						label.Text = HeaderText;
						cell.Controls.Add (label);
					}
				} else {
					cell.Text = HeaderText;
				}
			} else if (cellType == DataControlCellType.Footer) {
				cell.Text = FooterText;
			}
		}
		
		void SetCellText (DataControlFieldCell cell, string text, string img)
		{
			if (img != "") {
				Image image = new Image ();
				image.ImageUrl = HeaderImageUrl;
				cell.Controls.Add (image);
				if (text != "") {
					Label label = new Label ();
					label.Text = text;
					cell.Controls.Add (label);
				}
			} else {
				cell.Text = text;
			}
		}

		protected virtual void OnFieldChanged ()
		{
			if (FieldChanged != null)
				FieldChanged (this, EventArgs.Empty);
		}	
	
		protected virtual void LoadViewState (object savedState)
		{
			if (savedState == null)
				return;
				
			object [] states = (object []) savedState;
			viewState.LoadViewState (states[0]);
			
			if (states[1] != null)
				((IStateManager)controlStyle).LoadViewState (states[1]);
			if (states[2] != null)
				((IStateManager)footerStyle).LoadViewState (states[2]);
			if (states[3] != null)
				((IStateManager)headerStyle).LoadViewState (states[3]);
			if (states[4] != null)
				((IStateManager)itemStyle).LoadViewState (states[4]);
		}

		protected virtual object SaveViewState()
		{
			object[] state = new object [5];
			state [0] = viewState.SaveViewState ();
			if (controlStyle != null)
				state [1] = ((IStateManager) controlStyle).SaveViewState ();
			if (footerStyle != null)
				state [2] = ((IStateManager) footerStyle).SaveViewState ();
			if (headerStyle != null)
				state [3] = ((IStateManager) headerStyle).SaveViewState ();
			if (itemStyle != null)
				state [4] = ((IStateManager) itemStyle).SaveViewState ();
			
			if (state [0] == null && state [1] == null && state [2] == null && 
				state [3] == null && state [4] == null)
				return null;
				
			return state;
		}

		protected virtual void TrackViewState()
		{
			if (controlStyle != null) ((IStateManager) controlStyle).TrackViewState ();
			if (footerStyle != null) ((IStateManager) footerStyle).TrackViewState ();
			if (headerStyle != null) ((IStateManager) headerStyle).TrackViewState ();
			if (itemStyle != null) ((IStateManager) itemStyle).TrackViewState ();
			viewState.TrackViewState ();
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

		[MonoTODO ("Render this")]
		[DefaultValueAttribute ("")]
		[LocalizableAttribute (true)]
		[WebCategoryAttribute ("Accessibility")]
		public virtual string AccessibleHeaderText {
			get {
				object val = viewState ["accessibleHeaderText"];
				return val != null ? (string) val : "";
			}
			set { 
				viewState ["accessibleHeaderText"] = value;
				OnFieldChanged ();
			}
		}

		protected Control Control {
			get { return control; }
		}

		[WebCategoryAttribute ("Styles")]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValueAttribute (null)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
		public virtual Style ControlStyle {
			get {
				if (controlStyle == null) {
					controlStyle = new Style ();
					if (IsTrackingViewState)
						controlStyle.TrackViewState();
				}
				return controlStyle;
			}
		}
	
		protected bool DesignMode {
			get { return control != null && control.Site != null ? control.Site.DesignMode : false; }
		}

		[DefaultValueAttribute (null)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[WebCategoryAttribute ("Styles")]
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

		[LocalizableAttribute (true)]
		[WebCategoryAttribute ("Appearance")]
		[DefaultValue ("")]
		public virtual string FooterText {
			get {
				object val = viewState ["footerText"];
				return val != null ? (string) val : "";
			}
			set { 
				viewState ["footerText"] = value;
				OnFieldChanged ();
			}
		}

		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		[EditorAttribute ("System.Web.UI.Design.UrlEditor, System.Design, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string HeaderImageUrl {
			get {
				object val = viewState ["headerImageUrl"];
				return val != null ? (string) val : "";
			}
			set { 
				viewState ["headerImageUrl"] = value;
				OnFieldChanged ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[WebCategoryAttribute ("Styles")]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValueAttribute (null)]
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

		[DefaultValueAttribute ("")]
		[LocalizableAttribute (true)]
		[WebCategoryAttribute ("Appearance")]
		public virtual string HeaderText {
			get {
				object val = viewState ["headerText"];
				return val != null ? (string) val : "";
			}
			set { 
				viewState ["headerText"] = value;
				OnFieldChanged ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[WebCategoryAttribute ("Styles")]
		[DefaultValueAttribute (null)]
		public virtual TableItemStyle ItemStyle {
			get {
				if (itemStyle == null) {
					itemStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						itemStyle.TrackViewState();
				}
				return itemStyle;
			}
		}

		[WebCategoryAttribute ("Behavior")]
		[DefaultValueAttribute (true)]
		public virtual bool ShowHeader {
			get {
				object val = viewState ["showHeader"];
				return val != null ? (bool) val : true;
			}
			set { 
				viewState ["showHeader"] = value;
				OnFieldChanged ();
			}
		}

		[DefaultValueAttribute ("")]
//		[TypeConverterAttribute ("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[WebCategoryAttribute ("Behavior")]
		public virtual string SortExpression {
			get {
				object val = viewState ["sortExpression"];
				return val != null ? (string) val : "";
			}
			set { 
				viewState ["sortExpression"] = value;
				OnFieldChanged ();
			}
		}

		[WebCategoryAttribute ("Behavior")]
		[DefaultValueAttribute (true)]
		public bool Visible {
			get {
				object val = viewState ["visible"];
				return val != null ? (bool) val : true;
			}
			set { 
				viewState ["visible"] = value;
				OnFieldChanged ();
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
				viewState ["dataSourceViewSchema"] = value;
			}
		}		

		internal event EventHandler FieldChanged;
	}
}
#endif
