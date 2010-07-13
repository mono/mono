//
// System.Web.UI.WebControls.Wizard
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
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

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[DefaultEventAttribute ("FinishButtonClick")]
	[BindableAttribute (false)]
	[DesignerAttribute ("System.Web.UI.Design.WebControls.WizardDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ToolboxData ("<{0}:Wizard runat=\"server\"> <WizardSteps> <asp:WizardStep title=\"Step 1\" runat=\"server\"></asp:WizardStep> <asp:WizardStep title=\"Step 2\" runat=\"server\"></asp:WizardStep> </WizardSteps> </{0}:Wizard>")]
	public class Wizard: CompositeControl
	{
		public static readonly string CancelCommandName = "Cancel";
		public static readonly string MoveCompleteCommandName = "MoveComplete";
		public static readonly string MoveNextCommandName = "MoveNext";
		public static readonly string MovePreviousCommandName = "MovePrevious";
		public static readonly string MoveToCommandName = "Move";
		
		protected static readonly string CancelButtonID = "CancelButton";
		protected static readonly string CustomFinishButtonID = "CustomFinishButton";
		protected static readonly string CustomNextButtonID = "CustomNextButton";
		protected static readonly string CustomPreviousButtonID = "CustomPreviousButton";
		protected static readonly string DataListID = "SideBarList";
		protected static readonly string FinishButtonID = "FinishButton";
		protected static readonly string FinishPreviousButtonID = "FinishPreviousButton";
		protected static readonly string SideBarButtonID = "SideBarButton";
		protected static readonly string StartNextButtonID = "StartNextButton";
		protected static readonly string StepNextButtonID = "StepNextButton";
		protected static readonly string StepPreviousButtonID = "StepPreviousButton";
		
		WizardStepCollection steps;
		
		// View state
		
		TableItemStyle stepStyle;
		TableItemStyle sideBarStyle;
		TableItemStyle headerStyle;
		TableItemStyle navigationStyle;
		Style sideBarButtonStyle;
		
		Style cancelButtonStyle;
		Style finishCompleteButtonStyle;
		Style finishPreviousButtonStyle;
		Style startNextButtonStyle;
		Style stepNextButtonStyle;
		Style stepPreviousButtonStyle;
		Style navigationButtonStyle;
		
		ITemplate finishNavigationTemplate;
		ITemplate startNavigationTemplate;
		ITemplate stepNavigationTemplate;
		ITemplate headerTemplate;
		ITemplate sideBarTemplate;
		
		// Control state
		
		int activeStepIndex = -1;
		bool inited = false;
		ArrayList history;

		Table wizardTable;
		WizardHeaderCell _headerCell;
		TableCell _navigationCell;
		StartNavigationContainer _startNavContainer;
		StepNavigationContainer _stepNavContainer;
		FinishNavigationContainer _finishNavContainer;
		MultiView multiView;
		DataList stepDatalist;
		ArrayList styles = new ArrayList ();
		Hashtable customNavigation;
		
		static readonly object ActiveStepChangedEvent = new object();
		static readonly object CancelButtonClickEvent = new object();
		static readonly object FinishButtonClickEvent = new object();
		static readonly object NextButtonClickEvent = new object();
		static readonly object PreviousButtonClickEvent = new object();
		static readonly object SideBarButtonClickEvent = new object();
		
		public event EventHandler ActiveStepChanged {
			add { Events.AddHandler (ActiveStepChangedEvent, value); }
			remove { Events.RemoveHandler (ActiveStepChangedEvent, value); }
		}
		
		public event EventHandler CancelButtonClick {
			add { Events.AddHandler (CancelButtonClickEvent, value); }
			remove { Events.RemoveHandler (CancelButtonClickEvent, value); }
		}
		
		public event WizardNavigationEventHandler FinishButtonClick {
			add { Events.AddHandler (FinishButtonClickEvent, value); }
			remove { Events.RemoveHandler (FinishButtonClickEvent, value); }
		}
		
		public event WizardNavigationEventHandler NextButtonClick {
			add { Events.AddHandler (NextButtonClickEvent, value); }
			remove { Events.RemoveHandler (NextButtonClickEvent, value); }
		}
		
		public event WizardNavigationEventHandler PreviousButtonClick {
			add { Events.AddHandler (PreviousButtonClickEvent, value); }
			remove { Events.RemoveHandler (PreviousButtonClickEvent, value); }
		}
		
		public event WizardNavigationEventHandler SideBarButtonClick {
			add { Events.AddHandler (SideBarButtonClickEvent, value); }
			remove { Events.RemoveHandler (SideBarButtonClickEvent, value); }
		}
		
		protected virtual void OnActiveStepChanged (object source, EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [ActiveStepChangedEvent];
				if (eh != null) eh (source, e);
			}
		}
		
		protected virtual void OnCancelButtonClick (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [CancelButtonClickEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnFinishButtonClick (WizardNavigationEventArgs e)
		{
			if (Events != null) {
				WizardNavigationEventHandler eh = (WizardNavigationEventHandler) Events [FinishButtonClickEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnNextButtonClick (WizardNavigationEventArgs e)
		{
			if (Events != null) {
				WizardNavigationEventHandler eh = (WizardNavigationEventHandler) Events [NextButtonClickEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnPreviousButtonClick (WizardNavigationEventArgs e)
		{
			if (Events != null) {
				WizardNavigationEventHandler eh = (WizardNavigationEventHandler) Events [PreviousButtonClickEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnSideBarButtonClick (WizardNavigationEventArgs e)
		{
			if (Events != null) {
				WizardNavigationEventHandler eh = (WizardNavigationEventHandler) Events [SideBarButtonClickEvent];
				if (eh != null) eh (this, e);
			}
		}
		
	    [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
	    [BrowsableAttribute (false)]
		public WizardStepBase ActiveStep {
			get {
				if (ActiveStepIndex < -1 || ActiveStepIndex >= WizardSteps.Count)
					throw new InvalidOperationException ("ActiveStepIndex has an invalid value.");
				if (ActiveStepIndex == -1) return null;
				return WizardSteps [activeStepIndex];
			}
		}
		
	    [DefaultValueAttribute (-1)]
	    [ThemeableAttribute (false)]
	    public virtual int ActiveStepIndex {
		    get {
			    return activeStepIndex;
		    }
		    set {
			    if (value < -1 || (value > WizardSteps.Count && (inited || WizardSteps.Count > 0)))
				    throw new ArgumentOutOfRangeException ("The ActiveStepIndex must be less than WizardSteps.Count and at least -1");
			    if (inited && !AllowNavigationToStep (value))
				    return;

			    if(activeStepIndex != value) {
				    activeStepIndex = value;
				    
				    if (inited) {
					    multiView.ActiveViewIndex = value;
					    if (stepDatalist != null) {
						    stepDatalist.SelectedIndex = value;
						    stepDatalist.DataBind ();
					    }
					    OnActiveStepChanged (this, EventArgs.Empty);
				    }
			    }
		    }
	    }
		
	    [UrlPropertyAttribute]
	    [DefaultValueAttribute ("")]
	    [EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string CancelButtonImageUrl {
			get {
				object v = ViewState ["CancelButtonImageUrl"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["CancelButtonImageUrl"] = value;
			}
		}
		
	    [DefaultValueAttribute (null)]
	    [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
	    [NotifyParentPropertyAttribute (true)]
	    [PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public Style CancelButtonStyle {
			get {
				if (cancelButtonStyle == null) {
					cancelButtonStyle = new Style ();
					if (IsTrackingViewState)
						((IStateManager)cancelButtonStyle).TrackViewState ();
				}
				return cancelButtonStyle;
			}
		}
		
	    [LocalizableAttribute (true)]
		public virtual string CancelButtonText {
			get {
				object v = ViewState ["CancelButtonText"];
				return v != null ? (string)v : "Cancel";
			}
			set {
				ViewState ["CancelButtonText"] = value;
			}
		}
		
	    [DefaultValueAttribute (ButtonType.Button)]
		public virtual ButtonType CancelButtonType {
			get {
				object v = ViewState ["CancelButtonType"];
				return v != null ? (ButtonType)v : ButtonType.Button;
			}
			set {
				ViewState ["CancelButtonType"] = value;
			}
		}
		
	    [UrlPropertyAttribute]
	    [EditorAttribute ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	    [DefaultValueAttribute ("")]
		public virtual string CancelDestinationPageUrl {
			get {
				object v = ViewState ["CancelDestinationPageUrl"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["CancelDestinationPageUrl"] = value;
			}
		}
	    
	    [DefaultValueAttribute (0)]
		public virtual int CellPadding {
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).CellPadding;
				return 0;
			}
			set {
				((TableStyle) ControlStyle).CellPadding = value;
			}
		}
		
	    [DefaultValueAttribute (0)]
		public virtual int CellSpacing {
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).CellSpacing;
				return 0;
			}
			set {
				((TableStyle) ControlStyle).CellSpacing = value;
			}
		}
		
	    [DefaultValueAttribute (false)]
	    [ThemeableAttribute (false)]
		public virtual bool DisplayCancelButton {
			get {
				object v = ViewState ["DisplayCancelButton"];
				return v != null ? (bool) v : false;
			}
			set {
				ViewState ["DisplayCancelButton"] = value;
			}
		}
		
	    [DefaultValueAttribute (true)]
	    [ThemeableAttribute (false)]
		public virtual bool DisplaySideBar {
			get {
				object v = ViewState ["DisplaySideBar"];
				return v != null ? (bool) v : true;
			}
			set {
				ViewState ["DisplaySideBar"] = value;
				UpdateViews ();
			}
		}
		
	    [UrlPropertyAttribute]
	    [DefaultValueAttribute ("")]
	    [EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string FinishCompleteButtonImageUrl {
			get {
				object v = ViewState ["FinishCompleteButtonImageUrl"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["FinishCompleteButtonImageUrl"] = value;
			}
		}
		
	    [DefaultValueAttribute (null)]
	    [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
	    [NotifyParentPropertyAttribute (true)]
	    [PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public Style FinishCompleteButtonStyle {
			get {
				if (finishCompleteButtonStyle == null) {
					finishCompleteButtonStyle = new Style ();
					if (IsTrackingViewState)
						((IStateManager)finishCompleteButtonStyle).TrackViewState ();
				}
				return finishCompleteButtonStyle;
			}
		}
		
	    [LocalizableAttribute (true)]
		public virtual string FinishCompleteButtonText {
			get {
				object v = ViewState ["FinishCompleteButtonText"];
				return v != null ? (string)v : "Finish";
			}
			set {
				ViewState ["FinishCompleteButtonText"] = value;
			}
		}
		
	    [DefaultValueAttribute (ButtonType.Button)]
		public virtual ButtonType FinishCompleteButtonType {
			get {
				object v = ViewState ["FinishCompleteButtonType"];
				return v != null ? (ButtonType)v : ButtonType.Button;
			}
			set {
				ViewState ["FinishCompleteButtonType"] = value;
			}
		}
		
	    [UrlPropertyAttribute]
	    [EditorAttribute ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	    [DefaultValueAttribute ("")]
		public virtual string FinishDestinationPageUrl {
			get {
				object v = ViewState ["FinishDestinationPageUrl"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["FinishDestinationPageUrl"] = value;
			}
		}
	    
		[DefaultValue (null)]
		[TemplateContainer (typeof(Wizard), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [Browsable (false)]
		public virtual ITemplate FinishNavigationTemplate {
			get { return finishNavigationTemplate; }
			set { finishNavigationTemplate = value; UpdateViews (); }
		}
		
	    [UrlPropertyAttribute]
	    [DefaultValueAttribute ("")]
	    [EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string FinishPreviousButtonImageUrl {
			get {
				object v = ViewState ["FinishPreviousButtonImageUrl"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["FinishPreviousButtonImageUrl"] = value;
			}
		}
		
	    [DefaultValueAttribute (null)]
	    [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
	    [NotifyParentPropertyAttribute (true)]
	    [PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public Style FinishPreviousButtonStyle {
			get {
				if (finishPreviousButtonStyle == null) {
					finishPreviousButtonStyle = new Style ();
					if (IsTrackingViewState)
						((IStateManager)finishPreviousButtonStyle).TrackViewState ();
				}
				return finishPreviousButtonStyle;
			}
		}
		
	    [LocalizableAttribute (true)]
		public virtual string FinishPreviousButtonText {
			get {
				object v = ViewState ["FinishPreviousButtonText"];
				return v != null ? (string)v : "Previous";
			}
			set {
				ViewState ["FinishPreviousButtonText"] = value;
			}
		}
		
	    [DefaultValueAttribute (ButtonType.Button)]
		public virtual ButtonType FinishPreviousButtonType {
			get {
				object v = ViewState ["FinishPreviousButtonType"];
				return v != null ? (ButtonType)v : ButtonType.Button;
			}
			set {
				ViewState ["FinishPreviousButtonType"] = value;
			}
		}
		
	    [DefaultValueAttribute (null)]
	    [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
	    [NotifyParentPropertyAttribute (true)]
	    [PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public TableItemStyle HeaderStyle {
			get {
				if (headerStyle == null) {
					headerStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager)headerStyle).TrackViewState ();
				}
				return headerStyle;
			}
		}
		
		[DefaultValue (null)]
		[TemplateContainer (typeof(Wizard), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [Browsable (false)]
		public virtual ITemplate HeaderTemplate {
			get { return headerTemplate; }
			set { headerTemplate = value; UpdateViews (); }
		}
		
	    [DefaultValueAttribute ("")]
	    [LocalizableAttribute (true)]
		public virtual string HeaderText {
			get {
				object v = ViewState ["HeaderText"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["HeaderText"] = value;
			}
		}
		
	    [DefaultValueAttribute (null)]
	    [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
	    [NotifyParentPropertyAttribute (true)]
	    [PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public Style NavigationButtonStyle {
			get {
				if (navigationButtonStyle == null) {
					navigationButtonStyle = new Style ();
					if (IsTrackingViewState)
						((IStateManager)navigationButtonStyle).TrackViewState ();
				}
				return navigationButtonStyle;
			}
		}
		
	    [DefaultValueAttribute (null)]
	    [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
	    [NotifyParentPropertyAttribute (true)]
	    [PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public TableItemStyle NavigationStyle {
			get {
				if (navigationStyle == null) {
					navigationStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager)navigationStyle).TrackViewState ();
				}
				return navigationStyle;
			}
		}
		
	    [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
	    [PersistenceModeAttribute (PersistenceMode.InnerProperty)]
	    [DefaultValueAttribute (null)]
	    [NotifyParentPropertyAttribute (true)]
		public TableItemStyle SideBarStyle {
			get {
				if (sideBarStyle == null) {
					sideBarStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager)sideBarStyle).TrackViewState ();
				}
				return sideBarStyle;
			}
		}
		
	    [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
	    [PersistenceModeAttribute (PersistenceMode.InnerProperty)]
	    [DefaultValueAttribute (null)]
	    [NotifyParentPropertyAttribute (true)]
		public Style SideBarButtonStyle {
			get {
				if (sideBarButtonStyle == null) {
					sideBarButtonStyle = new Style ();
					if (IsTrackingViewState)
						((IStateManager)sideBarButtonStyle).TrackViewState ();
				}
				return sideBarButtonStyle;
			}
		}
		
		[DefaultValue (null)]
		[TemplateContainer (typeof(Wizard), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public virtual ITemplate SideBarTemplate {
			get { return sideBarTemplate; }
			set { sideBarTemplate = value; UpdateViews (); }
		}

		[Localizable (true)]
		public virtual string SkipLinkText 
		{
			get
			{
				object v = ViewState ["SkipLinkText"];
				return v != null ? (string) v : "Skip Navigation Links.";
			}
			set
			{
				ViewState ["SkipLinkText"] = value;
			}
		}
		
		[DefaultValue (null)]
		[TemplateContainer (typeof(Wizard), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [Browsable (false)]
		public virtual ITemplate StartNavigationTemplate {
			get { return startNavigationTemplate; }
			set { startNavigationTemplate = value; UpdateViews (); }
		}
		
	    [UrlPropertyAttribute]
	    [DefaultValueAttribute ("")]
	    [EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string StartNextButtonImageUrl {
			get {
				object v = ViewState ["StartNextButtonImageUrl"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["StartNextButtonImageUrl"] = value;
			}
		}
		
	    [DefaultValueAttribute (null)]
	    [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
	    [NotifyParentPropertyAttribute (true)]
	    [PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public Style StartNextButtonStyle {
			get {
				if (startNextButtonStyle == null) {
					startNextButtonStyle = new Style ();
					if (IsTrackingViewState)
						((IStateManager)startNextButtonStyle).TrackViewState ();
				}
				return startNextButtonStyle;
			}
		}
		
	    [LocalizableAttribute (true)]
		public virtual string StartNextButtonText {
			get {
				object v = ViewState ["StartNextButtonText"];
				return v != null ? (string)v : "Next";
			}
			set {
				ViewState ["StartNextButtonText"] = value;
			}
		}
		
	    [DefaultValueAttribute (ButtonType.Button)]
		public virtual ButtonType StartNextButtonType {
			get {
				object v = ViewState ["StartNextButtonType"];
				return v != null ? (ButtonType)v : ButtonType.Button;
			}
			set {
				ViewState ["StartNextButtonType"] = value;
			}
		}
		
		[DefaultValue (null)]
		[TemplateContainer (typeof(Wizard), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [Browsable (false)]
		public virtual ITemplate StepNavigationTemplate {
			get { return stepNavigationTemplate; }
			set { stepNavigationTemplate = value; UpdateViews (); }
		}
		
	    [UrlPropertyAttribute]
	    [DefaultValueAttribute ("")]
	    [EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string StepNextButtonImageUrl {
			get {
				object v = ViewState ["StepNextButtonImageUrl"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["StepNextButtonImageUrl"] = value;
			}
		}
		
	    [DefaultValueAttribute (null)]
	    [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
	    [NotifyParentPropertyAttribute (true)]
	    [PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public Style StepNextButtonStyle {
			get {
				if (stepNextButtonStyle == null) {
					stepNextButtonStyle = new Style ();
					if (IsTrackingViewState)
						((IStateManager)stepNextButtonStyle).TrackViewState ();
				}
				return stepNextButtonStyle;
			}
		}
		
	    [LocalizableAttribute (true)]
		public virtual string StepNextButtonText {
			get {
				object v = ViewState ["StepNextButtonText"];
				return v != null ? (string)v : "Next";
			}
			set {
				ViewState ["StepNextButtonText"] = value;
			}
		}
		
	    [DefaultValueAttribute (ButtonType.Button)]
		public virtual ButtonType StepNextButtonType {
			get {
				object v = ViewState ["StepNextButtonType"];
				return v != null ? (ButtonType)v : ButtonType.Button;
			}
			set {
				ViewState ["StepNextButtonType"] = value;
			}
		}
		
	    [UrlPropertyAttribute]
	    [DefaultValueAttribute ("")]
	    [EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string StepPreviousButtonImageUrl {
			get {
				object v = ViewState ["StepPreviousButtonImageUrl"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["StepPreviousButtonImageUrl"] = value;
			}
		}
		
	    [DefaultValueAttribute (null)]
	    [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
	    [NotifyParentPropertyAttribute (true)]
	    [PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public Style StepPreviousButtonStyle {
			get {
				if (stepPreviousButtonStyle == null) {
					stepPreviousButtonStyle = new Style ();
					if (IsTrackingViewState)
						((IStateManager)stepPreviousButtonStyle).TrackViewState ();
				}
				return stepPreviousButtonStyle;
			}
		}
		
	    [LocalizableAttribute (true)]
		public virtual string StepPreviousButtonText {
			get {
				object v = ViewState ["StepPreviousButtonText"];
				return v != null ? (string)v : "Previous";
			}
			set {
				ViewState ["StepPreviousButtonText"] = value;
			}
		}
		
	    [DefaultValueAttribute (ButtonType.Button)]
		public virtual ButtonType StepPreviousButtonType {
			get {
				object v = ViewState ["StepPreviousButtonType"];
				return v != null ? (ButtonType)v : ButtonType.Button;
			}
			set {
				ViewState ["StepPreviousButtonType"] = value;
			}
		}
		
	    [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
	    [PersistenceModeAttribute (PersistenceMode.InnerProperty)]
	    [DefaultValueAttribute (null)]
	    [NotifyParentPropertyAttribute (true)]
		public TableItemStyle StepStyle {
			get {
				if (stepStyle == null) {
					stepStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						((IStateManager)stepStyle).TrackViewState ();
				}
				return stepStyle;
			}
		}
		
	    [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
	    [EditorAttribute ("System.Web.UI.Design.WebControls.WizardStepCollectionEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	    [PersistenceModeAttribute (PersistenceMode.InnerProperty)]
	    [ThemeableAttribute (false)]
		public virtual WizardStepCollection WizardSteps {
			get {
				if (steps == null)
					steps = new WizardStepCollection (this);
				return steps;
			}
		}

		protected virtual new HtmlTextWriterTag TagKey
		{
			get {
				return HtmlTextWriterTag.Table;
			}
		}

		internal virtual ITemplate SideBarItemTemplate
		{
			get { return new SideBarButtonTemplate (this); }
		}
		
		public ICollection GetHistory ()
		{
			if (history == null) history = new ArrayList ();
			return history;
		}
		
		public void MoveTo (WizardStepBase wizardStep)
		{
			if (wizardStep == null) throw new ArgumentNullException ("wizardStep");
			
			int i = WizardSteps.IndexOf (wizardStep);
			if (i == -1) throw new ArgumentException ("The provided wizard step does not belong to this wizard.");
			
			ActiveStepIndex = i;
		}
		
		public WizardStepType GetStepType (WizardStepBase wizardStep, int index)
		{
			if (wizardStep.StepType == WizardStepType.Auto) {
				if ((index == WizardSteps.Count - 1) || 
						(WizardSteps.Count > 1 && 
						WizardSteps[WizardSteps.Count - 1].StepType == WizardStepType.Complete && 
						index == WizardSteps.Count - 2))
					return WizardStepType.Finish;
				else if (index == 0)
					return WizardStepType.Start;
				else
					return WizardStepType.Step;
			} else
				return wizardStep.StepType;
			 
		}
		
		protected virtual bool AllowNavigationToStep (int index)
		{
			if (index < 0 || index >= WizardSteps.Count) return false;
			if (history == null) return true;
			if (!history.Contains (index)) return true;
			return WizardSteps [index].AllowReturn;
		} 
		
		protected internal override void OnInit (EventArgs e)
		{
			Page.RegisterRequiresControlState (this);
			base.OnInit (e);

			if (ActiveStepIndex == -1)
				ActiveStepIndex = 0;

			EnsureChildControls ();
			
			inited = true;
		}
		
		protected override ControlCollection CreateControlCollection ()
		{
			ControlCollection col = new ControlCollection (this);
			col.SetReadonly (true);
			return col;
		}
		
		protected internal override void CreateChildControls ()
		{
			CreateControlHierarchy ();
		}

		protected virtual void CreateControlHierarchy ()
		{
			styles.Clear ();

			wizardTable = new ContainedTable (this);

			Table contentTable = wizardTable;

			if (DisplaySideBar) {
				contentTable = new Table ();
				contentTable.CellPadding = 0;
				contentTable.CellSpacing = 0;
				contentTable.Height = new Unit ("100%");
				contentTable.Width = new Unit ("100%");

				TableRow row = new TableRow ();

				TableCellNamingContainer sideBarCell = new TableCellNamingContainer (SkipLinkText, ClientID);
				sideBarCell.ID = "SideBarContainer";
				sideBarCell.ControlStyle.Height = Unit.Percentage (100);
				CreateSideBar (sideBarCell);
				row.Cells.Add (sideBarCell);

				TableCell contentCell = new TableCell ();
				contentCell.Controls.Add (contentTable);
				contentCell.Height = new Unit ("100%");
				row.Cells.Add (contentCell);

				wizardTable.Rows.Add (row);
			}

			AddHeaderRow (contentTable);

			TableRow viewRow = new TableRow ();
			TableCell viewCell = new TableCell ();

			customNavigation = null;
			multiView = new MultiView ();
			foreach (View v in WizardSteps) {
				if (v is TemplatedWizardStep)
					InstantiateTemplateStep ((TemplatedWizardStep) v);
				multiView.Views.Add (v);
			}
			multiView.ActiveViewIndex = ActiveStepIndex;

			RegisterApplyStyle (viewCell, StepStyle);
			viewCell.Controls.Add (multiView);
			viewRow.Cells.Add (viewCell);
			viewRow.Height = new Unit ("100%");
			contentTable.Rows.Add (viewRow);

			TableRow buttonRow = new TableRow ();
			_navigationCell = new TableCell ();
			_navigationCell.HorizontalAlign = HorizontalAlign.Right;
			RegisterApplyStyle (_navigationCell, NavigationStyle);
			CreateButtonBar (_navigationCell);
			buttonRow.Cells.Add (_navigationCell);
			contentTable.Rows.Add (buttonRow);

			Controls.SetReadonly (false);
			Controls.Add (wizardTable);
			Controls.SetReadonly (true);
		}

		internal virtual void InstantiateTemplateStep(TemplatedWizardStep step)
		{
			BaseWizardContainer contentTemplateContainer = new BaseWizardContainer ();

			if (step.ContentTemplate != null)
				step.ContentTemplate.InstantiateIn (contentTemplateContainer.InnerCell);

			step.ContentTemplateContainer = contentTemplateContainer;
			step.Controls.Clear ();
			step.Controls.Add (contentTemplateContainer);

			BaseWizardNavigationContainer customNavigationTemplateContainer = new BaseWizardNavigationContainer ();

			if (step.CustomNavigationTemplate != null) {
				step.CustomNavigationTemplate.InstantiateIn (customNavigationTemplateContainer);
				RegisterCustomNavigation (step, customNavigationTemplateContainer);
			}
			step.CustomNavigationTemplateContainer = customNavigationTemplateContainer;
		}

		internal void RegisterCustomNavigation (TemplatedWizardStep step, BaseWizardNavigationContainer customNavigationTemplateContainer) {
			if (customNavigation == null)
				customNavigation = new Hashtable ();
			customNavigation [step] = customNavigationTemplateContainer;
		}
		
		void CreateButtonBar (TableCell buttonBarCell)
		{
			if(customNavigation!=null && customNavigation.Values.Count>0)
			{
				int i = 0;
				foreach (Control customNavigationTemplateContainer in customNavigation.Values) {
					customNavigationTemplateContainer.ID = "CustomNavContainer" + i++;
					buttonBarCell.Controls.Add (customNavigationTemplateContainer);
				}
			}
			
			//
			// StartNavContainer
			//
			_startNavContainer = new StartNavigationContainer (this);
			_startNavContainer.ID = "StartNavContainer";
			if (startNavigationTemplate != null) {
				startNavigationTemplate.InstantiateIn (_startNavContainer);
			}
			else {
				TableRow row;
				AddNavButtonsTable (_startNavContainer, out row);
				AddButtonCell (row, CreateButtonSet (StartNextButtonID, MoveNextCommandName));
				AddButtonCell (row, CreateButtonSet (CancelButtonID, CancelCommandName, false));
				_startNavContainer.ConfirmDefaultTemplate ();
			}
			buttonBarCell.Controls.Add (_startNavContainer);

			//
			// StepNavContainer
			//
			_stepNavContainer = new StepNavigationContainer (this);
			_stepNavContainer.ID = "StepNavContainer";
			if (stepNavigationTemplate != null) {
				stepNavigationTemplate.InstantiateIn (_stepNavContainer);
			}
			else {
				TableRow row;
				AddNavButtonsTable (_stepNavContainer, out row);
				AddButtonCell (row, CreateButtonSet (StepPreviousButtonID, MovePreviousCommandName, false));
				AddButtonCell (row, CreateButtonSet (StepNextButtonID, MoveNextCommandName));
				AddButtonCell (row, CreateButtonSet (CancelButtonID, CancelCommandName, false));
				_stepNavContainer.ConfirmDefaultTemplate ();
			}
			buttonBarCell.Controls.Add (_stepNavContainer);

			//
			// StepNavContainer
			//
			_finishNavContainer = new FinishNavigationContainer (this);
			_finishNavContainer.ID = "FinishNavContainer";
			if (finishNavigationTemplate != null) {
				finishNavigationTemplate.InstantiateIn (_finishNavContainer);
			}
			else {
				TableRow row;
				AddNavButtonsTable (_finishNavContainer, out row);
				AddButtonCell (row, CreateButtonSet (FinishPreviousButtonID, MovePreviousCommandName, false));
				AddButtonCell (row, CreateButtonSet (FinishButtonID, MoveCompleteCommandName));
				AddButtonCell (row, CreateButtonSet (CancelButtonID, CancelCommandName, false));
				_finishNavContainer.ConfirmDefaultTemplate ();
			}
			buttonBarCell.Controls.Add (_finishNavContainer);
		}

		static void AddNavButtonsTable (BaseWizardNavigationContainer container, out TableRow row)
		{
			Table t = new Table ();
			t.CellPadding = 5;
			t.CellSpacing = 5;
			row = new TableRow ();
			t.Rows.Add (row);
			container.Controls.Add (t);
		}

		Control [] CreateButtonSet (string id, string command)
		{
			return CreateButtonSet (id, command, true, null);
		}

		Control [] CreateButtonSet (string id, string command, bool causesValidation)
		{
			return CreateButtonSet (id, command, causesValidation, null);
		}

		internal Control [] CreateButtonSet (string id, string command, bool causesValidation, string validationGroup)
		{
			return new Control [] { 
				CreateButton ( id + ButtonType.Button,  command, ButtonType.Button, causesValidation, validationGroup),
				CreateButton ( id + ButtonType.Image,  command, ButtonType.Image, causesValidation, validationGroup),
				CreateButton ( id + ButtonType.Link,  command, ButtonType.Link, causesValidation, validationGroup)
				};
		}

		Control CreateButton (string id, string command, ButtonType type, bool causesValidation, string validationGroup)
		{
			WebControl b;
			switch (type) {
			case ButtonType.Button:
				b = CreateStandartButton ();
				break;
			case ButtonType.Image:
				b = CreateImageButton (null);
				break;
			case ButtonType.Link:
				b = CreateLinkButton ();
				break;
			default:
				throw new ArgumentOutOfRangeException ("type");
			}

			b.ID = id;
			b.EnableTheming = false;
			((IButtonControl) b).CommandName = command;
			((IButtonControl) b).CausesValidation = causesValidation;
			if(!String.IsNullOrEmpty(validationGroup))
				((IButtonControl) b).ValidationGroup = validationGroup;

			RegisterApplyStyle (b, NavigationButtonStyle);

			return b;
		}

		WebControl CreateStandartButton () {
			Button btn = new Button ();
			return btn;
		}

		WebControl CreateImageButton (string imageUrl) {
			ImageButton img = new ImageButton ();
			img.ImageUrl = imageUrl;
			return img;
		}

		WebControl CreateLinkButton () {
			LinkButton link = new LinkButton ();
			return link;
		}

		void AddButtonCell (TableRow row, params Control[] controls)
		{
			TableCell cell = new TableCell ();
			cell.HorizontalAlign = HorizontalAlign.Right;
			for (int i = 0; i < controls.Length; i++)
				cell.Controls.Add (controls [i]);
			row.Cells.Add (cell);
		}
		
		void CreateSideBar (TableCell sideBarCell)
		{
			RegisterApplyStyle (sideBarCell, SideBarStyle);
			if (sideBarTemplate != null) {
				sideBarTemplate.InstantiateIn (sideBarCell);
				stepDatalist = sideBarCell.FindControl (DataListID) as DataList;
				if (stepDatalist == null)
					throw new InvalidOperationException ("The side bar template must contain a DataList control with id '" + DataListID + "'.");
				stepDatalist.ItemDataBound += new DataListItemEventHandler(StepDatalistItemDataBound);
			} else {
				stepDatalist = new DataList ();
				stepDatalist.ID = DataListID;
				stepDatalist.SelectedItemStyle.Font.Bold = true;
				stepDatalist.ItemTemplate = SideBarItemTemplate;
				sideBarCell.Controls.Add (stepDatalist);
			}

			stepDatalist.ItemCommand += new DataListCommandEventHandler (StepDatalistItemCommand);
			stepDatalist.CellSpacing = 0;
			stepDatalist.DataSource = WizardSteps;
			stepDatalist.SelectedIndex = ActiveStepIndex;
			stepDatalist.DataBind ();
		}

		void StepDatalistItemCommand (object sender, DataListCommandEventArgs e)
		{
			WizardNavigationEventArgs arg = new WizardNavigationEventArgs (ActiveStepIndex, Convert.ToInt32 (e.CommandArgument));
			OnSideBarButtonClick (arg);

			if (!arg.Cancel)
				ActiveStepIndex = arg.NextStepIndex;
		}

		void StepDatalistItemDataBound (object sender, DataListItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.SelectedItem) {
				IButtonControl button = (IButtonControl) e.Item.FindControl (SideBarButtonID);
				if (button == null)
					throw new InvalidOperationException ("SideBarList control must contain an IButtonControl with ID " + SideBarButtonID + " in every item template, this maybe include ItemTemplate, EditItemTemplate, SelectedItemTemplate or AlternatingItemTemplate if they exist.");

				WizardStepBase step = (WizardStepBase) e.Item.DataItem;

				if (button is Button)
					((Button) button).UseSubmitBehavior = false;

				button.CommandName = Wizard.MoveToCommandName;
				button.CommandArgument = WizardSteps.IndexOf (step).ToString ();
				button.Text = step.Name;
				if (step.StepType == WizardStepType.Complete && button is WebControl)
					((WebControl) button).Enabled = false;
			}
		}

		void AddHeaderRow (Table table)
		{
			TableRow row = new TableRow ();
			_headerCell = new WizardHeaderCell ();
			_headerCell.ID = "HeaderContainer";
			RegisterApplyStyle (_headerCell, HeaderStyle);
			if (headerTemplate != null) {
				headerTemplate.InstantiateIn (_headerCell);
				_headerCell.ConfirmInitState ();
			}
			row.Cells.Add (_headerCell);
			table.Rows.Add (row);
		}

		internal void RegisterApplyStyle (WebControl control, Style style)
		{
			styles.Add (new object [] { control, style });
		}
		
		protected override Style CreateControlStyle ()
		{
			TableStyle style = new TableStyle ();
			style.CellPadding = 0;
			style.CellSpacing = 0;
			return style;
		}

		protected override IDictionary GetDesignModeState ()
		{
			throw new NotImplementedException ();
		}
		
		protected internal override void LoadControlState (object ob)
		{
			if (ob == null) return;
			object[] state = (object[]) ob;
			base.LoadControlState (state[0]);
			activeStepIndex = (int) state[1];
			history = (ArrayList) state[2];
		}
		
		protected internal override object SaveControlState ()
		{
			if (GetHistory ().Count == 0 || (int) history [0] != ActiveStepIndex)
				history.Insert (0, ActiveStepIndex);

			object bstate = base.SaveControlState ();
			return new object[] {
				bstate, activeStepIndex, history
			};
		}
		
		protected override void LoadViewState (object savedState)
		{
			if (savedState == null) {
				base.LoadViewState (null);
				return;
			}
			
			object[] states = (object[]) savedState;
			base.LoadViewState (states [0]);
			
			if (states[1] != null) ((IStateManager)StepStyle).LoadViewState (states[1]);
			if (states[2] != null) ((IStateManager)SideBarStyle).LoadViewState (states[2]);
			if (states[3] != null) ((IStateManager)HeaderStyle).LoadViewState (states[3]);
			if (states[4] != null) ((IStateManager)NavigationStyle).LoadViewState (states[4]);
			if (states[5] != null) ((IStateManager)SideBarButtonStyle).LoadViewState (states[5]);
			if (states[6] != null) ((IStateManager)CancelButtonStyle).LoadViewState (states[6]);
			if (states[7] != null) ((IStateManager)FinishCompleteButtonStyle).LoadViewState (states[7]);
			if (states[8] != null) ((IStateManager)FinishPreviousButtonStyle).LoadViewState (states[8]);
			if (states[9] != null) ((IStateManager)StartNextButtonStyle).LoadViewState (states[9]);
			if (states[10] != null) ((IStateManager)StepNextButtonStyle).LoadViewState (states[10]);
			if (states[11] != null) ((IStateManager)StepPreviousButtonStyle).LoadViewState (states[11]);
			if (states[12] != null) ((IStateManager)NavigationButtonStyle).LoadViewState (states[12]);
			if (states [13] != null)
				ControlStyle.LoadViewState (states [13]);
		}
		
		protected override object SaveViewState ()
		{
			object [] state = new object [14];
			state [0] = base.SaveViewState ();
			
			if (stepStyle != null) state [1] = ((IStateManager)stepStyle).SaveViewState ();
			if (sideBarStyle != null) state [2] = ((IStateManager)sideBarStyle).SaveViewState ();
			if (headerStyle != null) state [3] = ((IStateManager)headerStyle).SaveViewState ();
			if (navigationStyle != null) state [4] = ((IStateManager)navigationStyle).SaveViewState ();
			if (sideBarButtonStyle != null) state [5] = ((IStateManager)sideBarButtonStyle).SaveViewState ();
			if (cancelButtonStyle != null) state [6] = ((IStateManager)cancelButtonStyle).SaveViewState ();
			if (finishCompleteButtonStyle != null) state [7] = ((IStateManager)finishCompleteButtonStyle).SaveViewState ();
			if (finishPreviousButtonStyle != null) state [8] = ((IStateManager)finishPreviousButtonStyle).SaveViewState ();
			if (startNextButtonStyle != null) state [9] = ((IStateManager)startNextButtonStyle).SaveViewState ();
			if (stepNextButtonStyle != null) state [10] = ((IStateManager)stepNextButtonStyle).SaveViewState ();
			if (stepPreviousButtonStyle != null) state [11] = ((IStateManager)stepPreviousButtonStyle).SaveViewState ();
			if (navigationButtonStyle != null) state [12] = ((IStateManager)navigationButtonStyle).SaveViewState ();
			if (ControlStyleCreated)
				state [13] = ControlStyle.SaveViewState ();
	
			for (int n=0; n<state.Length; n++)
				if (state [n] != null) return state;
			return null;
		}
		
		protected override void TrackViewState ()
		{
			base.TrackViewState();
			if (stepStyle != null) ((IStateManager)stepStyle).TrackViewState();
			if (sideBarStyle != null) ((IStateManager)sideBarStyle).TrackViewState();
			if (headerStyle != null) ((IStateManager)headerStyle).TrackViewState();
			if (navigationStyle != null) ((IStateManager)navigationStyle).TrackViewState();
			if (sideBarButtonStyle != null) ((IStateManager)sideBarButtonStyle).TrackViewState();
			if (cancelButtonStyle != null) ((IStateManager)cancelButtonStyle).TrackViewState();
			if (finishCompleteButtonStyle != null) ((IStateManager)finishCompleteButtonStyle).TrackViewState();
			if (finishPreviousButtonStyle != null) ((IStateManager)finishPreviousButtonStyle).TrackViewState();
			if (startNextButtonStyle != null) ((IStateManager)startNextButtonStyle).TrackViewState();
			if (stepNextButtonStyle != null) ((IStateManager)stepNextButtonStyle).TrackViewState();
			if (stepPreviousButtonStyle != null) ((IStateManager)stepPreviousButtonStyle).TrackViewState();
			if (navigationButtonStyle != null) ((IStateManager)navigationButtonStyle).TrackViewState();
			if (ControlStyleCreated)
				ControlStyle.TrackViewState ();
		}
		
		protected internal void RegisterCommandEvents (IButtonControl button)
		{
			button.Command += ProcessCommand;
		}
		
		void ProcessCommand (object sender, CommandEventArgs args)
		{
			Control c = sender as Control;
			if (c != null) {
				switch (c.ID) {
					case "CancelButton":
						ProcessEvent ("Cancel", null);
						return;
					case "FinishButton":
						ProcessEvent ("MoveComplete", null);
						return;
					case "StepPreviousButton":
					case "FinishPreviousButton":
						ProcessEvent ("MovePrevious", null);
						return;
					case "StartNextButton":
					case "StepNextButton":
						ProcessEvent ("MoveNext", null);
						return;
				}
			}
			ProcessEvent (args.CommandName, args.CommandArgument as string);
		}

		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			CommandEventArgs args = e as CommandEventArgs;
			if (args != null) {
				ProcessEvent (args.CommandName, args.CommandArgument as string);
				return true;
			}
			return base.OnBubbleEvent (source, e);
		}
		
		void ProcessEvent (string commandName, string commandArg)
		{
			switch (commandName) {
				case "Cancel":
					if (CancelDestinationPageUrl.Length > 0)
						Context.Response.Redirect (CancelDestinationPageUrl);
					else
						OnCancelButtonClick (EventArgs.Empty);
					break;

				case "MoveComplete":
					int next = -1;
					for (int n=0; n<WizardSteps.Count; n++) {
						if (WizardSteps [n].StepType == WizardStepType.Complete) {
							next = n;
							break;
						}
					}

					if (next == -1 && ActiveStepIndex == WizardSteps.Count - 1)
						next = ActiveStepIndex;

					WizardNavigationEventArgs navArgs = new WizardNavigationEventArgs (ActiveStepIndex, next);
					OnFinishButtonClick (navArgs);

					if (FinishDestinationPageUrl.Length > 0) {
						Context.Response.Redirect (FinishDestinationPageUrl);
						return;
					}

					if (next != -1 && !navArgs.Cancel)
						ActiveStepIndex = next;

					break;
					
				case "MoveNext":
					if (ActiveStepIndex < WizardSteps.Count - 1) {
						WizardNavigationEventArgs args = new WizardNavigationEventArgs (ActiveStepIndex, ActiveStepIndex + 1);
						int curStep = ActiveStepIndex;
						OnNextButtonClick (args);
						if (!args.Cancel && curStep == activeStepIndex)
							ActiveStepIndex++;
					}
					break;
							
				case "MovePrevious":
					if (ActiveStepIndex > 0) {
						WizardNavigationEventArgs args = new WizardNavigationEventArgs (ActiveStepIndex, ActiveStepIndex - 1);
						int curStep = ActiveStepIndex;
						OnPreviousButtonClick (args);
						if (!args.Cancel) {
							if (curStep == activeStepIndex)
								ActiveStepIndex--;
							if (history != null && activeStepIndex < curStep)
								history.Remove (curStep);
						}
					}
					break;
					
				case "Move":
					int newb = int.Parse (commandArg);
					ActiveStepIndex = newb;
					break;
			}
		}
		
		internal void UpdateViews ()
		{
			ChildControlsCreated = false;
		}
		
		protected internal override void Render (HtmlTextWriter writer)
		{
			PrepareControlHierarchy ();
			
			wizardTable.Render (writer);
		}

		void PrepareControlHierarchy ()
		{
			// header
			if (!_headerCell.Initialized) {
				if (String.IsNullOrEmpty (HeaderText))
					_headerCell.Parent.Visible = false;
				else
					_headerCell.Text = HeaderText;
			}

			if (ActiveStep.StepType == WizardStepType.Complete)
				_headerCell.Parent.Visible = false;

			// sidebar
			if (stepDatalist != null) {
				stepDatalist.SelectedIndex = ActiveStepIndex;
				stepDatalist.DataBind ();

				if (ActiveStep.StepType == WizardStepType.Complete)
					stepDatalist.NamingContainer.Visible = false;
			}

			// content
			TemplatedWizardStep templateStep = ActiveStep as TemplatedWizardStep;
			if (templateStep != null) {
				BaseWizardContainer contentContainer = templateStep.ContentTemplateContainer as BaseWizardContainer;
				if (contentContainer != null)
					contentContainer.PrepareControlHierarchy ();
			}

			// navigation
			if (customNavigation != null) {
				foreach (Control c in customNavigation.Values)
					c.Visible = false;
			}
			_startNavContainer.Visible = false;
			_stepNavContainer.Visible = false;
			_finishNavContainer.Visible = false;

			BaseWizardNavigationContainer currentNavContainer = GetCurrentNavContainer ();
			if (currentNavContainer == null) {
				_navigationCell.Parent.Visible = false;
			}
			else {
				currentNavContainer.Visible = true;
				currentNavContainer.PrepareControlHierarchy ();
				if (!currentNavContainer.Visible)
					_navigationCell.Parent.Visible = false;
			}

			foreach (object [] styleDef in styles)
				((WebControl) styleDef [0]).ApplyStyle ((Style) styleDef [1]);
		}

		BaseWizardNavigationContainer GetCurrentNavContainer ()
		{
			if (customNavigation != null && customNavigation [ActiveStep] != null) {
				return (BaseWizardNavigationContainer) customNavigation [ActiveStep];
			}
			else {
				WizardStepType stepType = GetStepType (ActiveStep, ActiveStepIndex);
				switch (stepType) {
				case WizardStepType.Start:
					return _startNavContainer;
				case WizardStepType.Step:
					return _stepNavContainer;
				case WizardStepType.Finish:
					return _finishNavContainer;
				default:
					return null;
				}
			}
		}

		sealed class TableCellNamingContainer : TableCell, INamingContainer, INonBindingContainer
		{
			string skipLinkText;
			string clientId;
			bool haveSkipLink;
			
			protected internal override void RenderChildren (HtmlTextWriter writer)
			{
				if (haveSkipLink) {
					// <a href="#ID_SkipLink">
					writer.AddAttribute (HtmlTextWriterAttribute.Href, "#" + clientId + "_SkipLink");
					writer.RenderBeginTag (HtmlTextWriterTag.A);

					// <img alt="" height="0" width="0" src="" style="border-width:0px;"/>
					writer.AddAttribute (HtmlTextWriterAttribute.Alt, skipLinkText);
					writer.AddAttribute (HtmlTextWriterAttribute.Height, "0");
					writer.AddAttribute (HtmlTextWriterAttribute.Width, "0");

					Page page = Page;
					ClientScriptManager csm;
					
					if (page != null)
						csm = page.ClientScript;
					else
						csm = new ClientScriptManager (null);
					writer.AddAttribute (HtmlTextWriterAttribute.Src, csm.GetWebResourceUrl (typeof (SiteMapPath), "transparent.gif"));
					writer.AddStyleAttribute (HtmlTextWriterStyle.BorderWidth, "0px");
					writer.RenderBeginTag (HtmlTextWriterTag.Img);
					writer.RenderEndTag ();
					
					writer.RenderEndTag (); // </a>
				}
				
				base.RenderChildren (writer);

				if (haveSkipLink) {
					writer.AddAttribute (HtmlTextWriterAttribute.Id, "SkipLink");
					writer.RenderBeginTag (HtmlTextWriterTag.A);
					writer.RenderEndTag ();
				}
			}
			
			public TableCellNamingContainer (string skipLinkText, string clientId)
			{
				this.skipLinkText = skipLinkText;
				this.clientId = clientId;
				this.haveSkipLink = !String.IsNullOrEmpty (skipLinkText);
			}
		}

		sealed class SideBarButtonTemplate: ITemplate
		{
			Wizard wizard;
			
			public SideBarButtonTemplate (Wizard wizard)
			{
				this.wizard = wizard;
			}
			
			public void InstantiateIn (Control control)
			{
				LinkButton b = new LinkButton ();
				wizard.RegisterApplyStyle (b, wizard.SideBarButtonStyle);
				control.Controls.Add (b);
				control.DataBinding += Bound;
			}
			
			void Bound (object s, EventArgs args)
			{
				WizardStepBase step = DataBinder.GetDataItem (s) as WizardStepBase;
				if (step != null) {
					DataListItem c = (DataListItem) s;
					LinkButton b = (LinkButton) c.Controls[0];
					b.ID = SideBarButtonID;
					b.CommandName = Wizard.MoveToCommandName;
					b.CommandArgument = wizard.WizardSteps.IndexOf (step).ToString ();
					b.Text = step.Name;
					if (step.StepType == WizardStepType.Complete)
						b.Enabled = false;
				}
			}
		}

		class WizardHeaderCell : TableCell, INamingContainer, INonBindingContainer
		{
			bool _initialized;

			public bool Initialized {
				get { return _initialized; }
			}
			
			public WizardHeaderCell ()
			{
			}
			
			public void ConfirmInitState ()
			{
				_initialized = true;
			}
		}

		internal abstract class DefaultNavigationContainer : BaseWizardNavigationContainer
		{
			bool _isDefault;
			Wizard _wizard;

			protected Wizard Wizard {
				get { return _wizard; }
			}

			protected DefaultNavigationContainer (Wizard wizard)
			{
				_wizard = wizard;
			}

			public override sealed void PrepareControlHierarchy ()
			{
				if (_isDefault)
					UpdateState ();
			}

			protected abstract void UpdateState ();

			public void ConfirmDefaultTemplate ()
			{
				_isDefault = true;
			}

			protected void UpdateNavButtonState (string id, string text, string image, Style style)
			{
				WebControl b = (WebControl) FindControl (id);
				foreach (Control c in b.Parent.Controls)
					c.Visible = b == c;

				((IButtonControl) b).Text = text;
				ImageButton imgbtn = b as ImageButton;
				if (imgbtn != null)
					imgbtn.ImageUrl = image;

				b.ApplyStyle (style);
			}
		}

		sealed class StartNavigationContainer : DefaultNavigationContainer
		{
			public StartNavigationContainer (Wizard wizard)
				: base (wizard)
			{
			}

			protected override void UpdateState ()
			{
				bool visible = false;
				
				// next
				if (Wizard.AllowNavigationToStep (Wizard.ActiveStepIndex + 1)) {
					visible = true;
					UpdateNavButtonState (Wizard.StartNextButtonID + Wizard.StartNextButtonType, Wizard.StartNextButtonText, Wizard.StartNextButtonImageUrl, Wizard.StartNextButtonStyle);
				}
				else {
					((Table) Controls [0]).Rows [0].Cells [0].Visible = false;
				}

				// cancel
				if (Wizard.DisplayCancelButton) {
					visible = true;
					UpdateNavButtonState (Wizard.CancelButtonID + Wizard.CancelButtonType, Wizard.CancelButtonText, Wizard.CancelButtonImageUrl, Wizard.CancelButtonStyle);
				}
				else {
					((Table) Controls [0]).Rows [0].Cells [1].Visible = false;
				}
				Visible = visible;
			}
		}

		sealed class StepNavigationContainer : DefaultNavigationContainer
		{
			public StepNavigationContainer (Wizard wizard)
				: base (wizard)
			{
			}

			protected override void UpdateState ()
			{
				bool visible = false;

				// previous
				if (Wizard.AllowNavigationToStep (Wizard.ActiveStepIndex - 1)) {
					visible = true;
					UpdateNavButtonState (Wizard.StepPreviousButtonID + Wizard.StepPreviousButtonType, Wizard.StepPreviousButtonText, Wizard.StepPreviousButtonImageUrl, Wizard.StepPreviousButtonStyle);
				}
				else {
					((Table) Controls [0]).Rows [0].Cells [0].Visible = false;
				}

				// next
				if (Wizard.AllowNavigationToStep (Wizard.ActiveStepIndex + 1)) {
					visible = true;
					UpdateNavButtonState (Wizard.StepNextButtonID + Wizard.StepNextButtonType, Wizard.StepNextButtonText, Wizard.StepNextButtonImageUrl, Wizard.StepNextButtonStyle);
				}
				else {
					((Table) Controls [0]).Rows [0].Cells [1].Visible = false;
				}

				// cancel
				if (Wizard.DisplayCancelButton) {
					visible = true;
					UpdateNavButtonState (Wizard.CancelButtonID + Wizard.CancelButtonType, Wizard.CancelButtonText, Wizard.CancelButtonImageUrl, Wizard.CancelButtonStyle);
				}
				else {
					((Table) Controls [0]).Rows [0].Cells [2].Visible = false;
				}
				Visible = visible;
			}
		}

		sealed class FinishNavigationContainer : DefaultNavigationContainer
		{
			public FinishNavigationContainer (Wizard wizard)
				: base (wizard)
			{
			}

			protected override void UpdateState ()
			{
				// previous
				if (Wizard.AllowNavigationToStep (Wizard.ActiveStepIndex - 1)) {
					UpdateNavButtonState (Wizard.FinishPreviousButtonID + Wizard.FinishPreviousButtonType, Wizard.FinishPreviousButtonText, Wizard.FinishPreviousButtonImageUrl, Wizard.FinishPreviousButtonStyle);
				}
				else {
					((Table) Controls [0]).Rows [0].Cells [0].Visible = false;
				}

				// finish
				UpdateNavButtonState (Wizard.FinishButtonID + Wizard.FinishCompleteButtonType, Wizard.FinishCompleteButtonText, Wizard.FinishCompleteButtonImageUrl, Wizard.FinishCompleteButtonStyle);

				// cancel
				if (Wizard.DisplayCancelButton) {
					UpdateNavButtonState (Wizard.CancelButtonID + Wizard.CancelButtonType, Wizard.CancelButtonText, Wizard.CancelButtonImageUrl, Wizard.CancelButtonStyle);
				}
				else {
					((Table) Controls [0]).Rows [0].Cells [2].Visible = false;
				}
			}
		}

		internal class BaseWizardContainer : Table, INamingContainer, INonBindingContainer
		{
			public TableCell InnerCell {
				get { return Rows [0].Cells [0]; }
			}

			internal BaseWizardContainer ()
			{
				InitTable ();
			}

			void InitTable () {
				TableRow row = new TableRow ();
				TableCell cell = new TableCell ();

				cell.ControlStyle.Width = Unit.Percentage (100);
				cell.ControlStyle.Height = Unit.Percentage (100);

				row.Cells.Add (cell);

				this.ControlStyle.Width = Unit.Percentage (100);
				this.ControlStyle.Height = Unit.Percentage (100);
				this.CellPadding = 0;
				this.CellSpacing = 0;

				this.Rows.Add (row);
			}

			public virtual void PrepareControlHierarchy ()
			{
			}
		}

		internal class BaseWizardNavigationContainer : Control, INamingContainer, INonBindingContainer
		{
			internal BaseWizardNavigationContainer ()
			{
			}

			public virtual void PrepareControlHierarchy ()
			{
			}
		}

		internal abstract class DefaultContentContainer : BaseWizardContainer
		{
			bool _isDefault;
			Wizard _wizard;

			protected bool IsDefaultTemplate {
				get { return _isDefault; }
			}

			protected Wizard Wizard {
				get { return _wizard; }
			}

			protected DefaultContentContainer (Wizard wizard)
			{
				_wizard = wizard;
			}

			public override sealed void PrepareControlHierarchy ()
			{
				if (_isDefault)
					UpdateState ();
			}

			protected abstract void UpdateState ();

			public void ConfirmDefaultTemplate ()
			{
				_isDefault = true;
			}
		}
	}
}

#endif
