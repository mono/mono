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
		Style sideBarButtonStyle;
		
		Style cancelButtonStyle;
		Style finishCompleteButtonStyle;
		Style finishPreviousButtonStyle;
		Style startNextButtonStyle;
		Style stepNextButtonStyle;
		Style stepPreviousButtonStyle;
		Style navigationButtonStyle;
		
		ITemplate finishNavigationTemplate;
		
		// Control state
		
		int activeStepIndex;


		Table wizardTable;
		MultiView multiView;
		ArrayList styles = new ArrayList ();
		
		private static readonly object ActiveStepChangedEvent = new object();
		private static readonly object CancelButtonClickEvent = new object();
		private static readonly object FinishButtonClickEvent = new object();
		private static readonly object NextButtonClickEvent = new object();
		private static readonly object PreviousButtonClickEvent = new object();
		private static readonly object SideBarButtonClickEvent = new object();
		
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
		
		public WizardStepBase ActiveStep {
			get {
				if (activeStepIndex < -1 || activeStepIndex >= steps.Count)
					throw new InvalidOperationException ("ActiveStepIndex has an invalid value.");
				if (activeStepIndex == -1) return null;
				return steps [activeStepIndex];
			}
		}
		
		public int ActiveStepIndex {
			get { return activeStepIndex; }
			set {
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
		
		public WizardStepCollection WizardSteps {
			get {
				if (steps == null)
					steps = new WizardStepCollection (this);
				return steps;
			}
		}
		
		public void MoveTo (WizardStep wizardStep)
		{
			if (wizardStep == null) throw new ArgumentNullException ("wizardStep");
			
			int i = WizardSteps.IndexOf (wizardStep);
			if (i == -1) throw new ArgumentException ("The provided wizard step does not belong to this wizard.");
			
			ActiveStepIndex = i;
		}
		
		protected override void OnInit (EventArgs e)
		{
			Page.RegisterRequiresControlState (this);
			base.OnInit (e);
		}
		
		protected override void CreateChildControls ()
		{
			CreateControlHierarchy ();
		}
		
		protected virtual void CreateControlHierarchy ()
		{
			styles.Clear ();

			wizardTable = new Table ();
			
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
			
			viewRow.Cells.Add (viewCell);
			wizardTable.Rows.Add (viewRow);
			
			TableRow buttonRow = new TableRow ();
			TableCell buttonCell = new TableCell ();
			CreateButtonBar (buttonCell);
			buttonRow.Cells.Add (buttonCell);
			wizardTable.Rows.Add (buttonRow);
			
			if (DisplaySideBar && ActiveStep.StepType != WizardStepType.Complete) {
				Table contentTable = wizardTable;
				wizardTable = new Table ();
				TableRow row = new TableRow ();
			
				TableCell sideBarCell = new TableCell ();
				CreateSideBar (sideBarCell);
				row.Cells.Add (sideBarCell);
				
				TableCell contentCell = new TableCell ();
				contentCell.Controls.Add (contentTable);
				row.Cells.Add (contentCell);
				
				wizardTable.Rows.Add (row);
			}
			
			Controls.Add (wizardTable);
		}
		
		void CreateButtonBar (TableCell buttonBarCell)
		{
			Table t = new Table ();
			TableRow row = new TableRow ();
			
			if (DisplayCancelButton)
				AddButtonCell (row, CreateButton (CancelButtonID, CancelCommandName, CancelButtonType, CancelButtonText, CancelButtonImageUrl, CancelButtonStyle));

			WizardStepType stepType;
			
			if (ActiveStep.StepType == WizardStepType.Auto) {
				if (ActiveStepIndex == 0)
					stepType = WizardStepType.Start;
				else if (ActiveStepIndex == WizardSteps.Count - 1)
					stepType = WizardStepType.Finish;
				else
					stepType = WizardStepType.Step;
			} else
				stepType = ActiveStep.StepType;
			 
			switch (stepType) {
				case WizardStepType.Start:
					AddButtonCell (row, CreateButton (StartNextButtonID, MoveNextCommandName, StartNextButtonType, StartNextButtonText, StartNextButtonImageUrl, StartNextButtonStyle));
					break;
				case WizardStepType.Step:
					AddButtonCell (row, CreateButton (StepPreviousButtonID, MovePreviousCommandName, StepPreviousButtonType, StepPreviousButtonText, StepPreviousButtonImageUrl, StepPreviousButtonStyle));
					AddButtonCell (row, CreateButton (StepNextButtonID, MoveNextCommandName, StepNextButtonType, StepNextButtonText, StepNextButtonImageUrl, StepNextButtonStyle));
					break;
				case WizardStepType.Finish:
					AddButtonCell (row, CreateButton (FinishPreviousButtonID, MovePreviousCommandName, FinishPreviousButtonType, FinishPreviousButtonText, FinishPreviousButtonImageUrl, FinishPreviousButtonStyle));
					AddButtonCell (row, CreateButton (FinishButtonID, MoveCompleteCommandName, FinishCompleteButtonType, FinishCompleteButtonText, FinishCompleteButtonImageUrl, FinishCompleteButtonStyle));
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
		
		void AddButtonCell (TableRow row, Control control)
		{
			TableCell cell = new TableCell ();
			cell.Controls.Add (control);
			row.Cells.Add (cell);
		}
		
		void CreateSideBar (TableCell sideBarCell)
		{
			RegisterApplyStyle (sideBarCell, SideBarStyle);
			sideBarCell.Text = "Side bar";
		}
		
		void AddHeaderRow (Table table)
		{
			if (HeaderText.Length != 0) {
				TableRow row = new TableRow ();
				TableCell cell = new TableCell ();
				RegisterApplyStyle (cell, HeaderStyle);
				cell.Text = HeaderText;
				row.Cells.Add (cell);
				table.Rows.Add (row);
			}
		}
		
		void RegisterApplyStyle (WebControl control, Style style)
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
		}
		
		protected internal override object SaveControlState ()
		{
			object bstate = base.SaveControlState ();
			return new object[] {
				bstate, activeStepIndex
			};
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
	}
}

#endif
