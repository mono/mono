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
	[DesignerAttribute ("System.Web.UI.Design.WebControls.WizardDesigner, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.IDesigner")]
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
		
		int activeStepIndex;
		ArrayList history;

		Table wizardTable;
		MultiView multiView;
		DataList stepDatalist;
		ArrayList styles = new ArrayList ();
		SideBarButtonTemplate sideBarItemTemplate;
		
		private static readonly object ActiveStepChangedEvent = new object();
		private static readonly object CancelButtonClickEvent = new object();
		private static readonly object FinishButtonClickEvent = new object();
		private static readonly object NextButtonClickEvent = new object();
		private static readonly object PreviousButtonClickEvent = new object();
		private static readonly object SideBarButtonClickEvent = new object();
		
		public Wizard ()
		{
			sideBarItemTemplate = new SideBarButtonTemplate (this);
		}
		
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
				if (activeStepIndex < -1 || activeStepIndex >= steps.Count)
					throw new InvalidOperationException ("ActiveStepIndex has an invalid value.");
				if (activeStepIndex == -1) return null;
				return steps [activeStepIndex];
			}
		}
		
	    [DefaultValueAttribute (-1)]
	    [ThemeableAttribute (false)]
		public int ActiveStepIndex {
			get {
				return activeStepIndex;
			}
			set {
				if (!AllowNavigationToStep (value))
					return;
				if (activeStepIndex != value) {
					if (history == null) history = new ArrayList ();
					history.Insert (0, activeStepIndex);
				}
				activeStepIndex = value;
				UpdateControls ();
				OnActiveStepChanged (this, EventArgs.Empty);
			}
		}
		
	    [UrlPropertyAttribute]
	    [DefaultValueAttribute ("")]
	    [EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public string CancelButtonImageUrl {
			get {
				object v = ViewState ["CancelButtonImageUrl"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["CancelButtonImageUrl"] = value;
				UpdateControls ();
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
		public string CancelButtonText {
			get {
				object v = ViewState ["CancelButtonText"];
				return v != null ? (string)v : "Cancel";
			}
			set {
				ViewState ["CancelButtonText"] = value;
				UpdateControls ();
			}
		}
		
	    [DefaultValueAttribute (ButtonType.Button)]
		public ButtonType CancelButtonType {
			get {
				object v = ViewState ["CancelButtonType"];
				return v != null ? (ButtonType)v : ButtonType.Button;
			}
			set {
				ViewState ["CancelButtonType"] = value;
				UpdateControls ();
			}
		}
		
	    [UrlPropertyAttribute]
	    [EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	    [DefaultValueAttribute ("")]
		public string CancelDestinationPageUrl {
			get {
				object v = ViewState ["CancelDestinationPageUrl"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["CancelDestinationPageUrl"] = value;
			}
		}
	    
	    [DefaultValueAttribute (0)]
		public int CellPadding {
			get {
				object v = ViewState ["CellPadding"];
				return v != null ? (int)v : 0;
			}
			set {
				ViewState ["CellPadding"] = value;
				UpdateControls ();
			}
		}
		
	    [DefaultValueAttribute (0)]
		public int CellSpacing {
			get {
				object v = ViewState ["CellSpacing"];
				return v != null ? (int)v : 0;
			}
			set {
				ViewState ["CellSpacing"] = value;
				UpdateControls ();
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
				UpdateControls ();
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
				UpdateControls ();
			}
		}
		
	    [UrlPropertyAttribute]
	    [DefaultValueAttribute ("")]
	    [EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public string FinishCompleteButtonImageUrl {
			get {
				object v = ViewState ["FinishCompleteButtonImageUrl"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["FinishCompleteButtonImageUrl"] = value;
				UpdateControls ();
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
		public string FinishCompleteButtonText {
			get {
				object v = ViewState ["FinishCompleteButtonText"];
				return v != null ? (string)v : "Finish";
			}
			set {
				ViewState ["FinishCompleteButtonText"] = value;
				UpdateControls ();
			}
		}
		
	    [DefaultValueAttribute (ButtonType.Button)]
		public ButtonType FinishCompleteButtonType {
			get {
				object v = ViewState ["FinishCompleteButtonType"];
				return v != null ? (ButtonType)v : ButtonType.Button;
			}
			set {
				ViewState ["FinishCompleteButtonType"] = value;
				UpdateControls ();
			}
		}
		
	    [UrlPropertyAttribute]
	    [EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	    [DefaultValueAttribute ("")]
		public string FinishDestinationPageUrl {
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
		public ITemplate FinishNavigationTemplate {
			get { return finishNavigationTemplate; }
			set { finishNavigationTemplate = value; UpdateControls (); }
		}
		
	    [UrlPropertyAttribute]
	    [DefaultValueAttribute ("")]
	    [EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public string FinishPreviousButtonImageUrl {
			get {
				object v = ViewState ["FinishPreviousButtonImageUrl"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["FinishPreviousButtonImageUrl"] = value;
				UpdateControls ();
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
		public string FinishPreviousButtonText {
			get {
				object v = ViewState ["FinishPreviousButtonText"];
				return v != null ? (string)v : "Previous";
			}
			set {
				ViewState ["FinishPreviousButtonText"] = value;
				UpdateControls ();
			}
		}
		
	    [DefaultValueAttribute (ButtonType.Button)]
		public ButtonType FinishPreviousButtonType {
			get {
				object v = ViewState ["FinishPreviousButtonType"];
				return v != null ? (ButtonType)v : ButtonType.Button;
			}
			set {
				ViewState ["FinishPreviousButtonType"] = value;
				UpdateControls ();
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
		public ITemplate HeaderTemplate {
			get { return headerTemplate; }
			set { headerTemplate = value; UpdateControls (); }
		}
		
	    [DefaultValueAttribute ("")]
	    [LocalizableAttribute (true)]
		public string HeaderText {
			get {
				object v = ViewState ["HeaderText"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["HeaderText"] = value;
				UpdateControls ();
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
		public ITemplate SideBarTemplate {
			get { return sideBarTemplate; }
			set { sideBarTemplate = value; UpdateControls (); }
		}
		
		[DefaultValue (null)]
		[TemplateContainer (typeof(Wizard), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [Browsable (false)]
		public ITemplate StartNavigationTemplate {
			get { return startNavigationTemplate; }
			set { startNavigationTemplate = value; UpdateControls (); }
		}
		
	    [UrlPropertyAttribute]
	    [DefaultValueAttribute ("")]
	    [EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public string StartNextButtonImageUrl {
			get {
				object v = ViewState ["StartNextButtonImageUrl"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["StartNextButtonImageUrl"] = value;
				UpdateControls ();
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
		public string StartNextButtonText {
			get {
				object v = ViewState ["StartNextButtonText"];
				return v != null ? (string)v : "Next";
			}
			set {
				ViewState ["StartNextButtonText"] = value;
				UpdateControls ();
			}
		}
		
	    [DefaultValueAttribute (ButtonType.Button)]
		public ButtonType StartNextButtonType {
			get {
				object v = ViewState ["StartNextButtonType"];
				return v != null ? (ButtonType)v : ButtonType.Button;
			}
			set {
				ViewState ["StartNextButtonType"] = value;
				UpdateControls ();
			}
		}
		
		[DefaultValue (null)]
		[TemplateContainer (typeof(Wizard), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [Browsable (false)]
		public ITemplate StepNavigationTemplate {
			get { return stepNavigationTemplate; }
			set { stepNavigationTemplate = value; UpdateControls (); }
		}
		
	    [UrlPropertyAttribute]
	    [DefaultValueAttribute ("")]
	    [EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public string StepNextButtonImageUrl {
			get {
				object v = ViewState ["StepNextButtonImageUrl"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["StepNextButtonImageUrl"] = value;
				UpdateControls ();
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
		public string StepNextButtonText {
			get {
				object v = ViewState ["StepNextButtonText"];
				return v != null ? (string)v : "Next";
			}
			set {
				ViewState ["StepNextButtonText"] = value;
				UpdateControls ();
			}
		}
		
	    [DefaultValueAttribute (ButtonType.Button)]
		public ButtonType StepNextButtonType {
			get {
				object v = ViewState ["StepNextButtonType"];
				return v != null ? (ButtonType)v : ButtonType.Button;
			}
			set {
				ViewState ["StepNextButtonType"] = value;
				UpdateControls ();
			}
		}
		
	    [UrlPropertyAttribute]
	    [DefaultValueAttribute ("")]
	    [EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public string StepPreviousButtonImageUrl {
			get {
				object v = ViewState ["StepPreviousButtonImageUrl"];
				return v != null ? (string)v : string.Empty;
			}
			set {
				ViewState ["StepPreviousButtonImageUrl"] = value;
				UpdateControls ();
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
		public string StepPreviousButtonText {
			get {
				object v = ViewState ["StepPreviousButtonText"];
				return v != null ? (string)v : "Previous";
			}
			set {
				ViewState ["StepPreviousButtonText"] = value;
				UpdateControls ();
			}
		}
		
	    [DefaultValueAttribute (ButtonType.Button)]
		public ButtonType StepPreviousButtonType {
			get {
				object v = ViewState ["StepPreviousButtonType"];
				return v != null ? (ButtonType)v : ButtonType.Button;
			}
			set {
				ViewState ["StepPreviousButtonType"] = value;
				UpdateControls ();
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
	    [EditorAttribute ("System.Web.UI.Design.WebControls.WizardStepCollectionEditor,System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	    [PersistenceModeAttribute (PersistenceMode.InnerProperty)]
	    [ThemeableAttribute (false)]
		public WizardStepCollection WizardSteps {
			get {
				if (steps == null)
					steps = new WizardStepCollection (this);
				return steps;
			}
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
				if (index == 0)
					return WizardStepType.Start;
				else if (index == WizardSteps.Count - 1)
					return WizardStepType.Finish;
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
		
		protected override void OnInit (EventArgs e)
		{
			Page.RegisterRequiresControlState (this);
			base.OnInit (e);
		}
		
		protected override ControlCollection CreateControlCollection ()
		{
			ControlCollection col = new ControlCollection (this);
			col.SetReadonly (true);
			return col;
		}
		
		protected override void CreateChildControls ()
		{
			CreateControlHierarchy ();
		}
		
		protected virtual void CreateControlHierarchy ()
		{
			styles.Clear ();

			wizardTable = new Table ();
			wizardTable.CellPadding = CellPadding; 
			wizardTable.CellSpacing = CellSpacing; 
			
			AddHeaderRow (wizardTable);
			
			TableRow viewRow = new TableRow ();
			TableCell viewCell = new TableCell ();
			
			if (multiView == null) {
				multiView = new MultiView ();
				foreach (View v in WizardSteps)
					multiView.Views.Add (v);
			}
			
			multiView.ActiveViewIndex = activeStepIndex;
			
			RegisterApplyStyle (viewCell, StepStyle);
			viewCell.Controls.Add (multiView);
			
			viewCell.Height = new Unit ("100%");
			viewRow.Cells.Add (viewCell);
			wizardTable.Rows.Add (viewRow);
			
			TableRow buttonRow = new TableRow ();
			TableCell buttonCell = new TableCell ();
			CreateButtonBar (buttonCell);
			buttonRow.Cells.Add (buttonCell);
			wizardTable.Rows.Add (buttonRow);
			
			if (DisplaySideBar && ActiveStep.StepType != WizardStepType.Complete) {
				Table contentTable = wizardTable;
				contentTable.Height = new Unit ("100%");
				
				wizardTable = new Table ();
				wizardTable.CellPadding = CellPadding; 
				wizardTable.CellSpacing = CellSpacing;
				TableRow row = new TableRow ();
				
				TableCell sideBarCell = new TableCell ();
				CreateSideBar (sideBarCell);
				row.Cells.Add (sideBarCell);
				
				TableCell contentCell = new TableCell ();
				contentCell.Controls.Add (contentTable);
				row.Cells.Add (contentCell);
				
				wizardTable.Rows.Add (row);
			}
			
			Controls.SetReadonly (false);
			Controls.Add (wizardTable);
			Controls.SetReadonly (true);
		}
		
		void CreateButtonBar (TableCell buttonBarCell)
		{
			Table t = new Table ();
			TableRow row = new TableRow ();
			RegisterApplyStyle (buttonBarCell, NavigationStyle);
			
			WizardStepType stepType = GetStepType (ActiveStep, ActiveStepIndex);
			switch (stepType) {
				case WizardStepType.Start:
					if (startNavigationTemplate != null) {
						AddTemplateCell (row, startNavigationTemplate, StartNextButtonID, CancelButtonID);
					} else {
						if (DisplayCancelButton)
							AddButtonCell (row, CreateButton (CancelButtonID, CancelCommandName, CancelButtonType, CancelButtonText, CancelButtonImageUrl, CancelButtonStyle));
						if (AllowNavigationToStep (ActiveStepIndex + 1))
							AddButtonCell (row, CreateButton (StartNextButtonID, MoveNextCommandName, StartNextButtonType, StartNextButtonText, StartNextButtonImageUrl, StartNextButtonStyle));
					}
					break;
				case WizardStepType.Step:
					if (stepNavigationTemplate != null) {
						AddTemplateCell (row, stepNavigationTemplate, StepPreviousButtonID, StepNextButtonID, CancelButtonID);
					} else {
						if (DisplayCancelButton)
							AddButtonCell (row, CreateButton (CancelButtonID, CancelCommandName, CancelButtonType, CancelButtonText, CancelButtonImageUrl, CancelButtonStyle));
						if (AllowNavigationToStep (ActiveStepIndex - 1))
							AddButtonCell (row, CreateButton (StepPreviousButtonID, MovePreviousCommandName, StepPreviousButtonType, StepPreviousButtonText, StepPreviousButtonImageUrl, StepPreviousButtonStyle));
						if (AllowNavigationToStep (ActiveStepIndex + 1))
							AddButtonCell (row, CreateButton (StepNextButtonID, MoveNextCommandName, StepNextButtonType, StepNextButtonText, StepNextButtonImageUrl, StepNextButtonStyle));
					}
					break;
				case WizardStepType.Finish:
					if (finishNavigationTemplate != null) {
						AddTemplateCell (row, finishNavigationTemplate, FinishPreviousButtonID, FinishButtonID, CancelButtonID);
					} else {
						if (DisplayCancelButton)
							AddButtonCell (row, CreateButton (CancelButtonID, CancelCommandName, CancelButtonType, CancelButtonText, CancelButtonImageUrl, CancelButtonStyle));
						if (AllowNavigationToStep (ActiveStepIndex - 1))
							AddButtonCell (row, CreateButton (FinishPreviousButtonID, MovePreviousCommandName, FinishPreviousButtonType, FinishPreviousButtonText, FinishPreviousButtonImageUrl, FinishPreviousButtonStyle));
						AddButtonCell (row, CreateButton (FinishButtonID, MoveCompleteCommandName, FinishCompleteButtonType, FinishCompleteButtonText, FinishCompleteButtonImageUrl, FinishCompleteButtonStyle));
					}
					break;
			}
			t.Rows.Add (row);
			buttonBarCell.Controls.Add (t);
		}
		
		Control CreateButton (string id, string command, ButtonType type, string text, string image, Style style)
		{
			DataControlButton b = new DataControlButton (this, text, image, command, "", false);
			b.ID = id;
			b.ButtonType = type;
			RegisterApplyStyle (b, NavigationButtonStyle);
			RegisterApplyStyle (b, style);
			return b;
		}
		
		void AddTemplateCell (TableRow row, ITemplate template, params string[] buttonIds)
		{
			TableCell cell = new TableCell ();
			template.InstantiateIn (cell);
			
			foreach (string id in buttonIds) {
				IButtonControl b = cell.FindControl (id) as IButtonControl;
				if (b != null) RegisterCommandEvents (b);
			}
			
			row.Cells.Add (cell);
		}
		
		void AddButtonCell (TableRow row, Control control)
		{
			TableCell cell = new TableCell ();
			cell.Controls.Add (control);
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
			} else {
				stepDatalist = new DataList ();
				stepDatalist.ID = DataListID;
				sideBarCell.Controls.Add (stepDatalist);
			}

			stepDatalist.DataSource = WizardSteps;
			stepDatalist.ItemTemplate = sideBarItemTemplate;
			stepDatalist.DataBind ();
		}
		
		void AddHeaderRow (Table table)
		{
			if (HeaderText.Length != 0 || headerTemplate != null) {
				TableRow row = new TableRow ();
				TableCell cell = new TableCell ();
				RegisterApplyStyle (cell, HeaderStyle);
				if (headerTemplate != null)
					headerTemplate.InstantiateIn (cell);
				else
					cell.Text = HeaderText;
				row.Cells.Add (cell);
				table.Rows.Add (row);
			}
		}
		
		internal void RegisterApplyStyle (WebControl control, Style style)
		{
			styles.Add (new object[] { control, style });
		}
		
		protected override Style CreateControlStyle ()
		{
			return new TableStyle ();
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
		}
		
		protected override object SaveViewState ()
		{
			object[] state = new object [13];
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
					if (FinishDestinationPageUrl.Length > 0) {
						Context.Response.Redirect (FinishDestinationPageUrl);
						return;
					}
				
					int next = -1;
					for (int n=0; n<WizardSteps.Count; n++) {
						if (WizardSteps [n].StepType == WizardStepType.Complete) {
							next = n;
							break;
						}
					}
					if (next != -1) {
						WizardNavigationEventArgs args = new WizardNavigationEventArgs (ActiveStepIndex, next);
						OnFinishButtonClick (args);
						if (!args.Cancel)
							ActiveStepIndex = next;
					}
					break;
					
				case "MoveNext":
					if (ActiveStepIndex < WizardSteps.Count - 1) {
						WizardNavigationEventArgs args = new WizardNavigationEventArgs (ActiveStepIndex, ActiveStepIndex + 1);
						OnNextButtonClick (args);
						if (!args.Cancel)
							ActiveStepIndex++;
					}
					break;
							
				case "MovePrevious":
					if (ActiveStepIndex > 0) {
						WizardNavigationEventArgs args = new WizardNavigationEventArgs (ActiveStepIndex, ActiveStepIndex - 1);
						OnPreviousButtonClick (args);
						if (!args.Cancel)
							ActiveStepIndex--;
					}
					break;
					
				case "Move":
					int newb = int.Parse (commandArg);
					ActiveStepIndex = newb;
					break;
			}
		}
		
		void UpdateControls ()
		{
			ChildControlsCreated = false;
		}
		
		internal void UpdateViews ()
		{
			multiView = null;
			UpdateControls ();
		}
		
		protected override void Render (HtmlTextWriter writer)
		{
			wizardTable.ApplyStyle (ControlStyle);

			foreach (object[] styleDef in styles)
				((WebControl)styleDef[0]).ApplyStyle ((Style)styleDef[1]);
			
			wizardTable.Render (writer);
		}
		
		class SideBarButtonTemplate: ITemplate
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
					Control c = (Control)s;
					LinkButton b = (LinkButton) c.Controls[0];
					b.ID = SideBarButtonID;
					b.CommandName = Wizard.MoveToCommandName;
					b.CommandArgument = wizard.WizardSteps.IndexOf (step).ToString ();
					b.Text = step.Title;
					if (step.StepType == WizardStepType.Complete)
						b.Enabled = false;
					if (step == wizard.ActiveStep)
						b.Font.Bold = true;
					wizard.RegisterCommandEvents (b);
				}
			}
		}
	}
}

#endif
