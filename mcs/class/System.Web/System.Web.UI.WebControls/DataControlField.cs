//
// System.Web.UI.WebControls.DataControlField.cs
//
// Authors:
//	Sanjay Gupta (gsanjay@novell.com)
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004-2010 Novell, Inc. (http://www.novell.com)
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
using System.Collections;
using System.Collections.Specialized;
using System.Web.UI;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	[DefaultPropertyAttribute ("HeaderText")]
	[TypeConverterAttribute (typeof(ExpandableObjectConverter))]
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class DataControlField : IStateManager, IDataSourceViewSchemaAccessor
	{
		static readonly object fieldChangedEvent = new object ();
		
		bool tracking = false;
		StateBag viewState;
		Control control;
		Style controlStyle;
		TableItemStyle footerStyle;
		TableItemStyle headerStyle;
		TableItemStyle itemStyle;
		bool sortingEnabled;
		EventHandlerList events = new EventHandlerList ();
		
		internal event EventHandler FieldChanged {
			add { events.AddHandler (fieldChangedEvent, value); }
			remove { events.RemoveHandler (fieldChangedEvent, value); }
		}
		
		protected DataControlField()
		{ 
			viewState = new StateBag ();
		}
		
		internal void SetDirty ()
		{
			viewState.SetDirty (true);
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
			this.sortingEnabled = sortingEnabled;
			this.control = control;
			return false;
		}

		public virtual void InitializeCell (DataControlFieldCell cell,
			DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			if (cellType == DataControlCellType.Header) {
				if (HeaderText.Length > 0 && sortingEnabled && SortExpression.Length > 0)
					cell.Controls.Add ((Control) DataControlButton.CreateButton (String.IsNullOrEmpty (HeaderImageUrl) ? ButtonType.Link : ButtonType.Image, control, HeaderText, HeaderImageUrl, DataControlCommands.SortCommandName, SortExpression, true));
				else if (HeaderImageUrl.Length > 0) {
					Image image = new Image ();
					image.ImageUrl = HeaderImageUrl;
					cell.Controls.Add (image);
				} else
					cell.Text = HeaderText.Length > 0 ? HeaderText : "&nbsp;";
			} else if (cellType == DataControlCellType.Footer) {
				string footerText = FooterText;
				cell.Text = (footerText.Length > 0) ? footerText : "&nbsp;";
			}
		}
		
		protected internal DataControlField CloneField ()
		{
			DataControlField field = CreateField ();
			CopyProperties (field);
			return field;
		}
		
		protected abstract DataControlField CreateField ();
		
		protected virtual void CopyProperties (DataControlField newField)
		{
			newField.AccessibleHeaderText = AccessibleHeaderText;
			newField.ControlStyle.CopyFrom (ControlStyle);
			newField.FooterStyle.CopyFrom (FooterStyle);
			newField.FooterText = FooterText;
			newField.HeaderImageUrl = HeaderImageUrl;
			newField.HeaderStyle.CopyFrom (HeaderStyle);
			newField.HeaderText = HeaderText;
			newField.InsertVisible = InsertVisible;
			newField.ItemStyle.CopyFrom (ItemStyle);
			newField.ShowHeader = ShowHeader;
			newField.SortExpression = SortExpression;
			newField.Visible = Visible;
		}
		
		protected virtual void OnFieldChanged ()
		{
			EventHandler eh = events [fieldChangedEvent] as EventHandler;
			
			if (eh != null)
				eh (this, EventArgs.Empty);
		}	
	
		protected virtual void LoadViewState (object savedState)
		{
			if (savedState == null)
				return;
				
			object [] states = (object []) savedState;
			viewState.LoadViewState (states[0]);
			
			if (states[1] != null)
				((IStateManager)ControlStyle).LoadViewState (states[1]);
			if (states[2] != null)
				((IStateManager)FooterStyle).LoadViewState (states[2]);
			if (states[3] != null)
				((IStateManager)HeaderStyle).LoadViewState (states[3]);
			if (states[4] != null)
				((IStateManager)ItemStyle).LoadViewState (states[4]);
		}

		protected virtual object SaveViewState ()
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

		protected virtual void TrackViewState ()
		{
			if (controlStyle != null) ((IStateManager) controlStyle).TrackViewState ();
			if (footerStyle != null) ((IStateManager) footerStyle).TrackViewState ();
			if (headerStyle != null) ((IStateManager) headerStyle).TrackViewState ();
			if (itemStyle != null) ((IStateManager) itemStyle).TrackViewState ();
			viewState.TrackViewState ();
			tracking = true;			
		}
		
		public virtual void ValidateSupportsCallback ()
		{
			throw new NotSupportedException ("Callback not supported");
		}

		void IStateManager.LoadViewState (object savedState)
		{
			LoadViewState (savedState);
		}

		object IStateManager.SaveViewState ()
		{
			return SaveViewState ();
		}

		void IStateManager.TrackViewState ()
		{
			TrackViewState ();
		}
		
		internal Exception GetNotSupportedPropException (string propName)
		{
			return new System.NotSupportedException ("The property '" + propName + "' is not supported in " + GetType().Name); 
		}

		internal bool ControlStyleCreated { get { return controlStyle != null; } }
		
		internal bool HeaderStyleCreated { get { return headerStyle != null; } }
		
		internal bool FooterStyleCreated { get { return footerStyle != null; } }
		
		internal bool ItemStyleCreated { get { return itemStyle != null; } }

		[MonoTODO ("Render this")]
		[DefaultValueAttribute ("")]
		[LocalizableAttribute (true)]
		[WebCategoryAttribute ("Accessibility")]
		public virtual string AccessibleHeaderText {
			get {
				object val = viewState ["accessibleHeaderText"];
				return val != null ? (string) val : String.Empty;
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
		public Style ControlStyle {
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
		public TableItemStyle FooterStyle {
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
				return val != null ? (string) val : String.Empty;
			}
			set { 
				viewState ["footerText"] = value;
				OnFieldChanged ();
			}
		}

		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[WebCategoryAttribute ("Appearance")]
		public virtual string HeaderImageUrl {
			get {
				object val = viewState ["headerImageUrl"];
				return val != null ? (string) val : String.Empty;
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
		public TableItemStyle HeaderStyle {
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
				return val != null ? (string) val : String.Empty;
			}
			set { 
				viewState ["headerText"] = value;
				OnFieldChanged ();
			}
		}

		[WebCategoryAttribute ("Behavior")]
		[DefaultValueAttribute (true)]
		public virtual bool InsertVisible {
			get {
				object val = viewState ["InsertVisible"];
				return val != null ? (bool) val : true;
			}
			set { 
				viewState ["InsertVisible"] = value;
				OnFieldChanged ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[WebCategoryAttribute ("Styles")]
		[DefaultValueAttribute (null)]
		public TableItemStyle ItemStyle {
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
//		[TypeConverterAttribute ("System.Web.UI.Design.DataSourceViewSchemaConverter, " + Consts.AssemblySystem_Design)]
		[WebCategoryAttribute ("Behavior")]
		public virtual string SortExpression {
			get {
				object val = viewState ["sortExpression"];
				return val != null ? (string) val : String.Empty;
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
				if (value == Visible)
					return;
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

		public override string ToString ()
		{
			if (string.IsNullOrEmpty (HeaderText))
				return base.ToString ();
			return HeaderText;
		}
	}
}
