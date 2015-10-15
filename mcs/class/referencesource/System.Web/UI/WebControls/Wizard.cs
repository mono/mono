//------------------------------------------------------------------------------
// <copyright file="Wizard.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Linq;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [
    Bindable(false),
    DefaultEvent("FinishButtonClick"),
    Designer("System.Web.UI.Design.WebControls.WizardDesigner, " + AssemblyRef.SystemDesign),
    ToolboxData("<{0}:Wizard runat=\"server\"> <WizardSteps> <asp:WizardStep title=\"Step 1\" runat=\"server\"></asp:WizardStep> <asp:WizardStep title=\"Step 2\" runat=\"server\"></asp:WizardStep> </WizardSteps> </{0}:Wizard>")
    ]

    public class Wizard : CompositeControl {

        private ITemplate _finishNavigationTemplate;
        private ITemplate _headerTemplate;
        private ITemplate _layoutTemplate;
        private ITemplate _startNavigationTemplate;
        private ITemplate _stepNavigationTemplate;
        private ITemplate _sideBarTemplate;

        private MultiView _multiView;

        private static readonly object _eventActiveStepChanged = new object();
        private static readonly object _eventFinishButtonClick = new object();
        private static readonly object _eventNextButtonClick = new object();
        private static readonly object _eventPreviousButtonClick = new object();
        private static readonly object _eventSideBarButtonClick = new object();
        private static readonly object _eventCancelButtonClick = new object();

        // 
        public static readonly string HeaderPlaceholderId = "headerPlaceholder";
        public static readonly string NavigationPlaceholderId = "navigationPlaceholder";
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "SideBar", Justification = "The casing has already been established in the context of Wizard")]
        public static readonly string SideBarPlaceholderId = "sideBarPlaceholder";
        public static readonly string WizardStepPlaceholderId = "wizardStepPlaceholder";

        public static readonly string CancelCommandName = "Cancel";
        public static readonly string MoveNextCommandName = "MoveNext";
        public static readonly string MovePreviousCommandName = "MovePrevious";
        public static readonly string MoveToCommandName = "Move";
        public static readonly string MoveCompleteCommandName = "MoveComplete";

        protected static readonly string CancelButtonID = "CancelButton";
        protected static readonly string StartNextButtonID = "StartNextButton";
        protected static readonly string StepPreviousButtonID = "StepPreviousButton";
        protected static readonly string StepNextButtonID = "StepNextButton";
        protected static readonly string FinishButtonID = "FinishButton";
        protected static readonly string FinishPreviousButtonID = "FinishPreviousButton";
        protected static readonly string CustomPreviousButtonID = "CustomPreviousButton";
        protected static readonly string CustomNextButtonID = "CustomNextButton";
        protected static readonly string CustomFinishButtonID = "CustomFinishButton";
        protected static readonly string DataListID = "SideBarList";
        protected static readonly string SideBarButtonID = "SideBarButton";

        internal const string _customNavigationControls = "CustomNavigationControls";

        private const string _templatedStepsID = "TemplatedWizardSteps";
        private const string _multiViewID = "WizardMultiView";

        private const string _customNavigationContainerIdPrefix = "__CustomNav";

        private TableCell _sideBarTableCell;

        private IWizardSideBarListControl _sideBarList;

        private IButtonControl _commandSender;
        private Dictionary<WizardStepBase, BaseNavigationTemplateContainer> _customNavigationContainers;
        private IDictionary _designModeState;
        private Stack<int> _historyStack;
        private List<TemplatedWizardStep> _templatedSteps;
        private WizardStepCollection _wizardStepCollection;

        private WizardRenderingBase _rendering;

        private bool _activeStepIndexSet;
        private bool _displaySideBarDefault;
        private bool _displaySideBar;
        private bool? _isMacIE;
        private bool _renderSideBarDataList;

        private Style _cancelButtonStyle;
        private Style _finishCompleteButtonStyle;
        private Style _finishPreviousButtonStyle;
        private Style _navigationButtonStyle;
        private Style _sideBarButtonStyle;
        private Style _startNextButtonStyle;
        private Style _stepNextButtonStyle;
        private Style _stepPreviousButtonStyle;

        private TableItemStyle _headerStyle;
        private TableItemStyle _navigationStyle;
        private TableItemStyle _sideBarStyle;
        private TableItemStyle _stepStyle;

        private const bool _displaySideBarDefaultValue = true; /* default to true */
        private const int _viewStateArrayLength = 15;

        public Wizard()
            : this(_displaySideBarDefaultValue) {
        }


        internal Wizard(bool displaySideBarDefault) {
            _displaySideBarDefault = displaySideBarDefault;
            _displaySideBar = displaySideBarDefault;
        }

        #region Public Properties

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.Wizard_ActiveStep)
        ]
        public WizardStepBase ActiveStep {
            get {
                if (ActiveStepIndex < -1 || ActiveStepIndex >= WizardSteps.Count) {
                    throw new InvalidOperationException(SR.GetString(SR.Wizard_ActiveStepIndex_out_of_range));
                }

                return MultiView.GetActiveView() as WizardStepBase;
            }
        }


        [
        DefaultValue(-1),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.Wizard_ActiveStepIndex),
        ]
        public virtual int ActiveStepIndex {
            get {
                return MultiView.ActiveViewIndex;
            }
            set {
                if (value < -1 ||
                    (value >= WizardSteps.Count && ControlState >= ControlState.FrameworkInitialized)) {
                    throw new ArgumentOutOfRangeException("value",
                        SR.GetString(SR.Wizard_ActiveStepIndex_out_of_range));
                }

                if (MultiView.ActiveViewIndex != value) {
                    MultiView.ActiveViewIndex = value;
                    _activeStepIndexSet = true;

                    // Need to rebind the DataList control if the active step is changed.
                    // This is necessary since custom sidebar template might have different
                    // itemtemplates defined.
                    if (_sideBarList != null && SideBarTemplate != null) {
                        _sideBarList.SelectedIndex = ActiveStepIndex;
                        _sideBarList.DataBind();
                    }
                }
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an image to be displayed for the cancel button.
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebCategory("Appearance"),
        WebSysDescription(SR.Wizard_CancelButtonImageUrl),
        UrlProperty(),
        ]
        public virtual string CancelButtonImageUrl {
            get {
                object obj = ViewState["CancelButtonImageUrl"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["CancelButtonImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of the cancel buttons.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Wizard_CancelButtonStyle)
        ]
        public Style CancelButtonStyle {
            get {
                if (_cancelButtonStyle == null) {
                    _cancelButtonStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_cancelButtonStyle).TrackViewState();
                    }
                }
                return _cancelButtonStyle;
            }
        }


        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.Wizard_Default_CancelButtonText),
        WebSysDescription(SR.Wizard_CancelButtonText)
        ]
        public virtual String CancelButtonText {
            get {
                string s = ViewState["CancelButtonText"] as String;
                return s == null ? SR.GetString(SR.Wizard_Default_CancelButtonText) : s;
            }
            set {
                if (value != CancelButtonText) {
                    ViewState["CancelButtonText"] = value;
                }
            }
        }


        [
        DefaultValue(ButtonType.Button),
        WebCategory("Appearance"),
        WebSysDescription(SR.Wizard_CancelButtonType)
        ]
        public virtual ButtonType CancelButtonType {
            get {
                object obj = ViewState["CancelButtonType"];
                return (obj == null) ? ButtonType.Button : (ButtonType)obj;
            }
            set {
                ValidateButtonType(value);
                ViewState["CancelButtonType"] = value;
            }
        }


        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.Wizard_CancelDestinationPageUrl),
        UrlProperty(),
        ]
        public virtual String CancelDestinationPageUrl {
            get {
                string s = ViewState["CancelDestinationPageUrl"] as String;
                return s == null ? String.Empty : s;
            }
            set {
                ViewState["CancelDestinationPageUrl"] = value;
            }
        }


        [
        WebCategory("Layout"),
        DefaultValue(0),
        WebSysDescription(SR.Wizard_CellPadding)
        ]
        public virtual int CellPadding {
            get {
                if (ControlStyleCreated == false) {
                    return 0;
                }
                return ((TableStyle)ControlStyle).CellPadding;
            }
            set {
                ((TableStyle)ControlStyle).CellPadding = value;
            }
        }


        [
        WebCategory("Layout"),
        DefaultValue(0),
        WebSysDescription(SR.Wizard_CellSpacing)
        ]
        public virtual int CellSpacing {
            get {
                if (ControlStyleCreated == false) {
                    return 0;
                }
                return ((TableStyle)ControlStyle).CellSpacing;
            }
            set {
                ((TableStyle)ControlStyle).CellSpacing = value;
            }
        }


        [
        DefaultValue(false),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.Wizard_DisplayCancelButton),
        ]
        public virtual bool DisplayCancelButton {
            get {
                object o = ViewState["DisplayCancelButton"];
                return o == null ? false : (bool)o;
            }
            set {
                ViewState["DisplayCancelButton"] = value;
            }
        }


        [
        DefaultValue(true),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.Wizard_DisplaySideBar),
        ]
        public virtual bool DisplaySideBar {
            get {
                return _displaySideBar;
            }
            set {
                if (value != _displaySideBar) {
                    _displaySideBar = value;
                    _sideBarTableCell = null;
                    RequiresControlsRecreation();
                }
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an image to be displayed for the finish button.
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebCategory("Appearance"),
        WebSysDescription(SR.Wizard_FinishCompleteButtonImageUrl),
        UrlProperty(),
        ]
        public virtual string FinishCompleteButtonImageUrl {
            get {
                object obj = ViewState["FinishCompleteButtonImageUrl"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["FinishCompleteButtonImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of the finishStep buttons.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Wizard_FinishCompleteButtonStyle)
        ]
        public Style FinishCompleteButtonStyle {
            get {
                if (_finishCompleteButtonStyle == null) {
                    _finishCompleteButtonStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_finishCompleteButtonStyle).TrackViewState();
                    }
                }
                return _finishCompleteButtonStyle;
            }
        }


        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.Wizard_Default_FinishButtonText),
        WebSysDescription(SR.Wizard_FinishCompleteButtonText)
        ]
        public virtual String FinishCompleteButtonText {
            get {
                string s = ViewState["FinishCompleteButtonText"] as String;
                return s == null ? SR.GetString(SR.Wizard_Default_FinishButtonText) : s;
            }
            set {
                ViewState["FinishCompleteButtonText"] = value;
            }
        }


        [
        WebCategory("Appearance"),
        DefaultValue(ButtonType.Button),
        WebSysDescription(SR.Wizard_FinishCompleteButtonType)
        ]
        public virtual ButtonType FinishCompleteButtonType {
            get {
                object obj = ViewState["FinishCompleteButtonType"];
                return (obj == null) ? ButtonType.Button : (ButtonType)obj;
            }
            set {
                ValidateButtonType(value);
                ViewState["FinishCompleteButtonType"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL for the continue button.
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.Wizard_FinishDestinationPageUrl),
        UrlProperty(),
        ]
        public virtual string FinishDestinationPageUrl {
            get {
                object obj = ViewState["FinishDestinationPageUrl"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["FinishDestinationPageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an image to be displayed for the finish step's previous button.
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebCategory("Appearance"),
        WebSysDescription(SR.Wizard_FinishPreviousButtonImageUrl),
        UrlProperty(),
        ]
        public virtual string FinishPreviousButtonImageUrl {
            get {
                object obj = ViewState["FinishPreviousButtonImageUrl"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["FinishPreviousButtonImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of the navigation buttons.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Wizard_FinishPreviousButtonStyle)
        ]
        public Style FinishPreviousButtonStyle {
            get {
                if (_finishPreviousButtonStyle == null) {
                    _finishPreviousButtonStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_finishPreviousButtonStyle).TrackViewState();
                    }
                }
                return _finishPreviousButtonStyle;
            }
        }


        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.Wizard_Default_StepPreviousButtonText),
        WebSysDescription(SR.Wizard_FinishPreviousButtonText)
        ]
        public virtual String FinishPreviousButtonText {
            get {
                string s = ViewState["FinishPreviousButtonText"] as String;
                return s == null ? SR.GetString(SR.Wizard_Default_StepPreviousButtonText) : s;
            }
            set {
                ViewState["FinishPreviousButtonText"] = value;
            }
        }


        [
        WebCategory("Appearance"),
        DefaultValue(ButtonType.Button),
        WebSysDescription(SR.Wizard_FinishPreviousButtonType)
        ]
        public virtual ButtonType FinishPreviousButtonType {
            get {
                object obj = ViewState["FinishPreviousButtonType"];
                return (obj == null) ? ButtonType.Button : (ButtonType)obj;
            }
            set {
                ValidateButtonType(value);
                ViewState["FinishPreviousButtonType"] = value;
            }
        }


        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(Wizard)),
        WebSysDescription(SR.Wizard_FinishNavigationTemplate)
        ]
        public virtual ITemplate FinishNavigationTemplate {
            get {
                return _finishNavigationTemplate;
            }
            set {
                _finishNavigationTemplate = value;
                RequiresControlsRecreation();
            }
        }


        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.WebControl_HeaderStyle)
        ]
        public TableItemStyle HeaderStyle {
            get {
                if (_headerStyle == null) {
                    _headerStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_headerStyle).TrackViewState();
                }
                return _headerStyle;
            }
        }


        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(Wizard)),
        WebSysDescription(SR.WebControl_HeaderTemplate)
        ]
        public virtual ITemplate HeaderTemplate {
            get {
                return _headerTemplate;
            }
            set {
                _headerTemplate = value;
                RequiresControlsRecreation();
            }
        }


        [
        DefaultValue(""),
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDescription(SR.Wizard_HeaderText)
        ]
        public virtual string HeaderText {
            get {
                string s = ViewState["HeaderText"] as String;
                return s == null ? String.Empty : s;
            }
            set {
                ViewState["HeaderText"] = value;
            }
        }


        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(Wizard)),
        WebSysDescription(SR.Wizard_LayoutTemplate)
        ]
        public virtual ITemplate LayoutTemplate {
            get {
                return _layoutTemplate;
            }
            set {
                _layoutTemplate = value;
                RequiresControlsRecreation();
            }
        }


        /// <devdoc>
        ///     Gets the style of the navigation buttons.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Wizard_NavigationButtonStyle)
        ]
        public Style NavigationButtonStyle {
            get {
                if (_navigationButtonStyle == null) {
                    _navigationButtonStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_navigationButtonStyle).TrackViewState();
                    }
                }
                return _navigationButtonStyle;
            }
        }


        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Wizard_NavigationStyle)
        ]
        public TableItemStyle NavigationStyle {
            get {
                if (_navigationStyle == null) {
                    _navigationStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_navigationStyle).TrackViewState();
                }
                return _navigationStyle;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an image to be displayed for the finish step's previous button.
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebCategory("Appearance"),
        WebSysDescription(SR.Wizard_StartNextButtonImageUrl),
        UrlProperty(),
        ]
        public virtual string StartNextButtonImageUrl {
            get {
                object obj = ViewState["StartNextButtonImageUrl"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["StartNextButtonImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of the navigation buttons.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Wizard_StartNextButtonStyle)
        ]
        public Style StartNextButtonStyle {
            get {
                if (_startNextButtonStyle == null) {
                    _startNextButtonStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_startNextButtonStyle).TrackViewState();
                    }
                }
                return _startNextButtonStyle;
            }
        }


        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.Wizard_Default_StepNextButtonText),
        WebSysDescription(SR.Wizard_StartNextButtonText)
        ]
        public virtual String StartNextButtonText {
            get {
                string s = ViewState["StartNextButtonText"] as String;
                return s == null ? SR.GetString(SR.Wizard_Default_StepNextButtonText) : s;
            }
            set {
                ViewState["StartNextButtonText"] = value;
            }
        }


        [
        WebCategory("Appearance"),
        DefaultValue(ButtonType.Button),
        WebSysDescription(SR.Wizard_StartNextButtonType)
        ]
        public virtual ButtonType StartNextButtonType {
            get {
                object obj = ViewState["StartNextButtonType"];
                return (obj == null) ? ButtonType.Button : (ButtonType)obj;
            }
            set {
                ValidateButtonType(value);
                ViewState["StartNextButtonType"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an image to be displayed for the next button.
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebCategory("Appearance"),
        WebSysDescription(SR.Wizard_StepNextButtonImageUrl),
        UrlProperty(),
        ]
        public virtual string StepNextButtonImageUrl {
            get {
                object obj = ViewState["StepNextButtonImageUrl"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["StepNextButtonImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of the navigation buttons.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Wizard_StepNextButtonStyle)
        ]
        public Style StepNextButtonStyle {
            get {
                if (_stepNextButtonStyle == null) {
                    _stepNextButtonStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_stepNextButtonStyle).TrackViewState();
                    }
                }
                return _stepNextButtonStyle;
            }
        }


        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.Wizard_Default_StepNextButtonText),
        WebSysDescription(SR.Wizard_StepNextButtonText)
        ]
        public virtual String StepNextButtonText {
            get {
                string s = ViewState["StepNextButtonText"] as String;
                return s == null ? SR.GetString(SR.Wizard_Default_StepNextButtonText) : s;
            }
            set {
                ViewState["StepNextButtonText"] = value;
            }
        }


        [
        WebCategory("Appearance"),
        DefaultValue(ButtonType.Button),
        WebSysDescription(SR.Wizard_StepNextButtonType)
        ]
        public virtual ButtonType StepNextButtonType {
            get {
                object obj = ViewState["StepNextButtonType"];
                return (obj == null) ? ButtonType.Button : (ButtonType)obj;
            }
            set {
                ValidateButtonType(value);
                ViewState["StepNextButtonType"] = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets the URL of an image to be displayed for the previous button.
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebCategory("Appearance"),
        WebSysDescription(SR.Wizard_StepPreviousButtonImageUrl),
        UrlProperty(),
        ]
        public virtual string StepPreviousButtonImageUrl {
            get {
                object obj = ViewState["StepPreviousButtonImageUrl"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["StepPreviousButtonImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of the navigation buttons.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Wizard_StepPreviousButtonStyle)
        ]
        public Style StepPreviousButtonStyle {
            get {
                if (_stepPreviousButtonStyle == null) {
                    _stepPreviousButtonStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_stepPreviousButtonStyle).TrackViewState();
                    }
                }
                return _stepPreviousButtonStyle;
            }
        }


        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.Wizard_Default_StepPreviousButtonText),
        WebSysDescription(SR.Wizard_StepPreviousButtonText)
        ]
        public virtual String StepPreviousButtonText {
            get {
                string s = ViewState["StepPreviousButtonText"] as String;
                return s == null ? SR.GetString(SR.Wizard_Default_StepPreviousButtonText) : s;
            }
            set {
                ViewState["StepPreviousButtonText"] = value;
            }
        }


        [
        WebCategory("Appearance"),
        DefaultValue(ButtonType.Button),
        WebSysDescription(SR.Wizard_StepPreviousButtonType)
        ]
        public virtual ButtonType StepPreviousButtonType {
            get {
                object obj = ViewState["StepPreviousButtonType"];
                return (obj == null) ? ButtonType.Button : (ButtonType)obj;
            }
            set {
                ValidateButtonType(value);
                ViewState["StepPreviousButtonType"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the style of the side bar buttons.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Wizard_SideBarButtonStyle)
        ]
        public Style SideBarButtonStyle {
            get {
                if (_sideBarButtonStyle == null) {
                    _sideBarButtonStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_sideBarButtonStyle).TrackViewState();
                    }
                }
                return _sideBarButtonStyle;
            }
        }


        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Wizard_SideBarStyle)
        ]
        public TableItemStyle SideBarStyle {
            get {
                if (_sideBarStyle == null) {
                    _sideBarStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_sideBarStyle).TrackViewState();
                }
                return _sideBarStyle;
            }
        }


        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(Wizard)),
        WebSysDescription(SR.Wizard_SideBarTemplate)
        ]
        public virtual ITemplate SideBarTemplate {
            get {
                return _sideBarTemplate;
            }
            set {
                _sideBarTemplate = value;
                _sideBarTableCell = null;
                RequiresControlsRecreation();
            }
        }


        [
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDefaultValue(SR.Wizard_Default_SkipToContentText),
        WebSysDescription(SR.WebControl_SkipLinkText)
        ]
        public virtual String SkipLinkText {
            get {
                string s = SkipLinkTextInternal;
                return s == null ? SR.GetString(SR.Wizard_Default_SkipToContentText) : s;
            }
            set {
                ViewState["SkipLinkText"] = value;
            }
        }


        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(Wizard)),
        WebSysDescription(SR.Wizard_StartNavigationTemplate)
        ]
        public virtual ITemplate StartNavigationTemplate {
            get {
                return _startNavigationTemplate;
            }
            set {
                _startNavigationTemplate = value;
                RequiresControlsRecreation();
            }
        }


        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(Wizard)),
        WebSysDescription(SR.Wizard_StepNavigationTemplate)
        ]
        public virtual ITemplate StepNavigationTemplate {
            get {
                return _stepNavigationTemplate;
            }
            set {
                _stepNavigationTemplate = value;
                RequiresControlsRecreation();
            }
        }


        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.Wizard_StepStyle)
        ]
        public TableItemStyle StepStyle {
            get {
                if (_stepStyle == null) {
                    _stepStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_stepStyle).TrackViewState();
                }
                return _stepStyle;
            }
        }


        // 
        protected override HtmlTextWriterTag TagKey {
            get {
                return HtmlTextWriterTag.Table;
            }
        }


        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor("System.Web.UI.Design.WebControls.WizardStepCollectionEditor," + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        Themeable(false),
        WebSysDescription(SR.Wizard_WizardSteps),
        ]
        public virtual WizardStepCollection WizardSteps {
            get {
                if (_wizardStepCollection == null) {
                    _wizardStepCollection = new WizardStepCollection(this);
                }

                return _wizardStepCollection;
            }
        }

        #endregion

        #region Public Events

        [
        WebCategory("Action"),
        WebSysDescription(SR.Wizard_ActiveStepChanged)
        ]
        public event EventHandler ActiveStepChanged {
            add {
                Events.AddHandler(_eventActiveStepChanged, value);
            }
            remove {
                Events.RemoveHandler(_eventActiveStepChanged, value);
            }
        }


        [
        WebCategory("Action"),
        WebSysDescription(SR.Wizard_CancelButtonClick)
        ]
        public event EventHandler CancelButtonClick {
            add {
                Events.AddHandler(_eventCancelButtonClick, value);
            }
            remove {

                Events.RemoveHandler(_eventCancelButtonClick, value);
            }
        }


        [
        WebCategory("Action"),
        WebSysDescription(SR.Wizard_FinishButtonClick)
        ]
        public event WizardNavigationEventHandler FinishButtonClick {
            add {
                Events.AddHandler(_eventFinishButtonClick, value);
            }
            remove {
                Events.RemoveHandler(_eventFinishButtonClick, value);
            }
        }


        [
        WebCategory("Action"),
        WebSysDescription(SR.Wizard_NextButtonClick)
        ]
        public event WizardNavigationEventHandler NextButtonClick {
            add {
                Events.AddHandler(_eventNextButtonClick, value);
            }
            remove {

                Events.RemoveHandler(_eventNextButtonClick, value);
            }
        }


        [
        WebCategory("Action"),
        WebSysDescription(SR.Wizard_PreviousButtonClick)
        ]
        public event WizardNavigationEventHandler PreviousButtonClick {
            add {
                Events.AddHandler(_eventPreviousButtonClick, value);
            }
            remove {
                Events.RemoveHandler(_eventPreviousButtonClick, value);
            }
        }


        [
        WebCategory("Action"),
        WebSysDescription(SR.Wizard_SideBarButtonClick)
        ]
        public virtual event WizardNavigationEventHandler SideBarButtonClick {
            add {
                Events.AddHandler(_eventSideBarButtonClick, value);
            }
            remove {
                Events.RemoveHandler(_eventSideBarButtonClick, value);
            }
        }

        #endregion

        #region Internal Properties

        internal Dictionary<WizardStepBase, BaseNavigationTemplateContainer> CustomNavigationContainers {
            get {
                if (_customNavigationContainers == null) {
                    _customNavigationContainers = new Dictionary<WizardStepBase, BaseNavigationTemplateContainer>();
                }
                return _customNavigationContainers;
            }
        }


        private ITemplate CustomNavigationTemplate {
            get {
                var templatedActiveStep = ActiveStep as TemplatedWizardStep;
                return templatedActiveStep == null ? null : templatedActiveStep.CustomNavigationTemplate;
            }
        }


        private Stack<int> History {
            get {
                if (_historyStack == null)
                    _historyStack = new Stack<int>();

                return _historyStack;
            }
        }

        private bool IsMacIE5 {
            get {
                if (!_isMacIE.HasValue && !DesignMode) {
                    HttpBrowserCapabilities browser = null;
                    if (Page != null) {
                        browser = Page.Request.Browser;
                    } else {
                        HttpContext context = HttpContext.Current;
                        // Context could be null if this control is created by the parser at designtime
                        if (context != null) {
                            browser = context.Request.Browser;
                        }
                    }

                    _isMacIE = (browser != null && browser.Type == "IE5" && browser.Platform == "MacPPC");
                }

                return _isMacIE.Value;
            }
        }


        internal MultiView MultiView {
            get {
                if (_multiView == null) {
                    _multiView = new MultiView();
                    _multiView.EnableTheming = true;
                    _multiView.ID = _multiViewID;
                    _multiView.ActiveViewChanged += new EventHandler(this.MultiViewActiveViewChanged);
                    _multiView.IgnoreBubbleEvents();
                }

                return _multiView;
            }
        }


        internal virtual bool ShowCustomNavigationTemplate {
            get {
                return CustomNavigationTemplate != null;
            }
        }


        internal bool ShouldRenderChildControl {
            get {
                if (!DesignMode) {
                    return true;
                }

                if (_designModeState == null) {
                    return true;
                }

                object o = _designModeState["ShouldRenderWizardSteps"];
                return o == null ? true : (bool)o;
            }
        }


        private IWizardSideBarListControl SideBarList {
            get { return _sideBarList; }
        }


        private bool SideBarEnabled {
            get {
                return _sideBarList != null && DisplaySideBar;
            }
        }


        internal string SkipLinkTextInternal {
            get {
                return ViewState["SkipLinkText"] as String;
            }
        }


        internal List<TemplatedWizardStep> TemplatedSteps {
            get {
                if (_templatedSteps == null) {
                    _templatedSteps = new List<TemplatedWizardStep>();
                }
                return _templatedSteps;
            }
        }

        #endregion

        private void MultiViewActiveViewChanged(object source, EventArgs e) {
            OnActiveStepChanged(this, EventArgs.Empty);
        }


        private void ApplyControlProperties() {
            _rendering.ApplyControlProperties();
        }


        internal BaseNavigationTemplateContainer CreateBaseNavigationTemplateContainer(string id) {
            return new BaseNavigationTemplateContainer(this) {
                ID = id
            };
        }


        /// <internalonly />
        protected internal override void CreateChildControls() {
            using (new WizardControlCollectionModifier(this)) {
                Controls.Clear();
                _customNavigationContainers = null;
            }

            if (LayoutTemplate == null) {
                _rendering = CreateTableRendering();
            }
            else {
                _rendering = CreateLayoutTemplateRendering();
            }

            CreateControlHierarchy();
            ClearChildViewState();
        }

        internal virtual TableWizardRendering CreateTableRendering() {
            return new TableWizardRendering(this);
        }

        internal virtual LayoutTemplateWizardRendering CreateLayoutTemplateRendering() {
            return new LayoutTemplateWizardRendering(this);
        }


        protected override ControlCollection CreateControlCollection() {
            return new WizardControlCollection(this);
        }


        protected virtual void CreateControlHierarchy() {
            _rendering.CreateControlHierarchy();
        }


        private void SetStepsAndDataBindSideBarList(IWizardSideBarListControl sideBarList) {
            if (sideBarList != null) {
                sideBarList.DataSource = WizardSteps;
                sideBarList.SelectedIndex = ActiveStepIndex;
                sideBarList.DataBind();
            }
        }


        internal virtual ITemplate CreateDefaultSideBarTemplate() {
            return new DefaultSideBarTemplate(this);
        }

        internal virtual ITemplate CreateDefaultDataListItemTemplate() {
            return new DataListItemTemplate(this);
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>A protected method. Creates a table control style.</para>
        /// </devdoc>
        protected override Style CreateControlStyle() {
            TableStyle controlStyle = new TableStyle();

            // initialize defaults that are different from TableStyle
            controlStyle.CellSpacing = 0;
            controlStyle.CellPadding = 0;

            return controlStyle;
        }

        internal virtual void CreateCustomNavigationTemplates() {
            for (int i = 0; i < WizardSteps.Count; ++i) {
                TemplatedWizardStep step = WizardSteps[i] as TemplatedWizardStep;
                if (step != null) {
                    RegisterCustomNavigationContainers(step);
                }
            }
        }

        internal void RegisterCustomNavigationContainers(TemplatedWizardStep step) {
            // Instantiate the step's ContentTemplate
            InstantiateStepContentTemplate(step);

            if (!CustomNavigationContainers.ContainsKey(step)) {
                BaseNavigationTemplateContainer container = null;
                string id = GetCustomContainerID(WizardSteps.IndexOf(step));
                if (step.CustomNavigationTemplate != null) {
                    container = CreateBaseNavigationTemplateContainer(id);
                    step.CustomNavigationTemplate.InstantiateIn(container);
                    step.CustomNavigationTemplateContainer = container;
                    container.RegisterButtonCommandEvents();
                } else {
                    container = CreateBaseNavigationTemplateContainer(id);
                    container.RegisterButtonCommandEvents();
                }
                CustomNavigationContainers[step] = container;
            }
        }


        internal virtual void DataListItemDataBound(object sender, WizardSideBarListControlItemEventArgs e) {
            var dataListItem = e.Item;

            // Ignore the item that is not created from DataSource
            if (dataListItem.ItemType != ListItemType.Item &&
                dataListItem.ItemType != ListItemType.AlternatingItem &&
                dataListItem.ItemType != ListItemType.SelectedItem &&
                dataListItem.ItemType != ListItemType.EditItem) {
                return;
            }

            IButtonControl button = dataListItem.FindControl(SideBarButtonID) as IButtonControl;
            if (button == null) {
                if (!DesignMode) {
                    throw new InvalidOperationException(
                        SR.GetString(SR.Wizard_SideBar_Button_Not_Found, DataListID, SideBarButtonID));
                }

                return;
            }

            var ctrlButton = button as Button;
            if (ctrlButton != null) {
                // Use javascript submit to use the postdata instead, this is necessarily since the buttons could be recreated during DataBind(). Previously
                // registered buttons will lose their parents and events won't bubble up. VSWhidbey 120640.
                // For devices that do not support Javascript, fall back to sumit behavior VSWhidbey 154576
                ctrlButton.UseSubmitBehavior = false;
            }

            WebControl webCtrlButton = button as WebControl;
            if (webCtrlButton != null) {
                webCtrlButton.TabIndex = this.TabIndex;
            }

            int index = 0;

            // Render wizardstep title on the button control.
            WizardStepBase step = dataListItem.DataItem as WizardStepBase;
            if (step != null) {
                // Disable the button if it's a Complete step.
                if (GetStepType(step) == WizardStepType.Complete &&
                    webCtrlButton != null) {
                    webCtrlButton.Enabled = false;
                }

                // Need to render the sidebar tablecell.
                RegisterSideBarDataListForRender();

                // Use the step title if defined, otherwise use ID
                if (step.Title.Length > 0) {
                    button.Text = step.Title;
                } else {
                    button.Text = step.ID;
                }

                index = WizardSteps.IndexOf(step);

                button.CommandName = MoveToCommandName;
                button.CommandArgument = index.ToString(NumberFormatInfo.InvariantInfo);

                RegisterCommandEvents(button);
            }
        }

        internal void RegisterSideBarDataListForRender() {
            _renderSideBarDataList = true;
        }

        private void DataListItemCommand(object sender, CommandEventArgs e) {
            if (!MoveToCommandName.Equals(e.CommandName, StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            int oldIndex = ActiveStepIndex;
            int newIndex = Int32.Parse((String)e.CommandArgument, CultureInfo.InvariantCulture);

            WizardNavigationEventArgs args = new WizardNavigationEventArgs(oldIndex, newIndex);

            // Never cancel the item command at design time.
            if (_commandSender != null && !DesignMode && Page != null && !Page.IsValid) {
                args.Cancel = true;
            }

            _activeStepIndexSet = false;
            OnSideBarButtonClick(args);

            if (!args.Cancel) {
                // Honor user's change if activeStepIndex is set explicitely;
                if (!_activeStepIndexSet) {
                    if (AllowNavigationToStep(newIndex)) {
                        ActiveStepIndex = newIndex;
                    }
                }
            } else {
                // revert active step if it's cancelled.
                ActiveStepIndex = oldIndex;
            }
        }

        internal static string GetCustomContainerID(int index) {
            return _customNavigationContainerIdPrefix + index;
        }


        /// <internalonly/>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override IDictionary GetDesignModeState() {
            IDictionary dictionary = base.GetDesignModeState();
            Debug.Assert(dictionary != null && DesignMode);

            _designModeState = dictionary;
            int oldIndex = ActiveStepIndex;

            // Set the activestepindex to 0 in designmode if it's -1.
            try {
                if (oldIndex == -1 && WizardSteps.Count > 0) {
                    ActiveStepIndex = 0;
                }

                RequiresControlsRecreation();
                EnsureChildControls();
                ApplyControlProperties();

                _rendering.SetDesignModeState(dictionary);

                if (ShowCustomNavigationTemplate) {
                    BaseNavigationTemplateContainer customContainer = CustomNavigationContainers[ActiveStep];
                    dictionary[CustomNextButtonID] = customContainer.NextButton;
                    dictionary[CustomPreviousButtonID] = customContainer.PreviousButton;
                    dictionary[CustomFinishButtonID] = customContainer.FinishButton;
                    dictionary[CancelButtonID] = customContainer.CancelButton;
                    dictionary[_customNavigationControls] = customContainer.Controls;
                }

                // VSWhidbey 456506. Reset the ItemTemplate so it can be persisted correctly 
                // based on current SideBarButtonStyle
                if (SideBarTemplate == null && _sideBarList != null) {
                    _sideBarList.ItemTemplate = CreateDefaultDataListItemTemplate();
                }

                dictionary[DataListID] = _sideBarList;
                dictionary[_templatedStepsID] = TemplatedSteps;
            } finally {
                ActiveStepIndex = oldIndex;
            }

            return dictionary;
        }


        public ICollection GetHistory() {
            ArrayList list = new ArrayList();
            foreach (int index in History) {
                list.Add(WizardSteps[index]);
            }
            return list;
        }

        internal int GetPreviousStepIndex(bool popStack) {
            int previousIndex = -1;
            int index = ActiveStepIndex;

            if (_historyStack == null || _historyStack.Count == 0) {
                return previousIndex;
            }

            if (popStack) {
                previousIndex = _historyStack.Pop();

                // Ignore the current step, the current step index is already in the historyStack.
                if (previousIndex == index && _historyStack.Count > 0) {
                    previousIndex = _historyStack.Pop();
                }
            } else {
                previousIndex = _historyStack.Peek();

                // Ignore the current step, the current step index is already in the historyStack.
                if (previousIndex == index && _historyStack.Count > 1) {
                    int originalIndex = _historyStack.Pop();
                    previousIndex = _historyStack.Peek();
                    _historyStack.Push(originalIndex);
                }
            }

            // Return -1 if the current step is same as previous step
            if (previousIndex == index) {
                return -1;
            }

            return previousIndex;
        }

        private WizardStepType GetStepType(int index) {
            Debug.Assert(index > -1 && index < WizardSteps.Count);
            WizardStepBase step = WizardSteps[index] as WizardStepBase;
            return GetStepType(step, index);
        }

        private WizardStepType GetStepType(WizardStepBase step) {
            int index = WizardSteps.IndexOf(step);
            return GetStepType(step, index);
        }


        public WizardStepType GetStepType(WizardStepBase wizardStep, int index) {
            if (wizardStep.StepType == WizardStepType.Auto) {

                // If it's the only step or a Complete step is after current step, then make it Finish step
                if (WizardSteps.Count == 1 ||
                    (index < WizardSteps.Count - 1 &&
                    WizardSteps[index + 1].StepType == WizardStepType.Complete)) {
                    return WizardStepType.Finish;
                }

                // First one is the start step
                if (index == 0) {
                    return WizardStepType.Start;
                }

                // Last one is the finish step
                if (index == WizardSteps.Count - 1) {
                    return WizardStepType.Finish;
                }

                return WizardStepType.Step;
            }

            return wizardStep.StepType;
        }

        /// <devdoc>
        ///     Instantiates all the content templates for each TemplatedWizardStep
        /// </devdoc>
        internal virtual void InstantiateStepContentTemplates() {
            TemplatedSteps.ForEach(step => InstantiateStepContentTemplate(step));
        }

        internal void InstantiateStepContentTemplate(TemplatedWizardStep step) {
            step.Controls.Clear();

            BaseContentTemplateContainer container = new BaseContentTemplateContainer(this, true);
            ITemplate contentTemplate = step.ContentTemplate;

            if (contentTemplate != null) {
                container.SetEnableTheming();
                contentTemplate.InstantiateIn(container.InnerCell);
            }

            step.ContentTemplateContainer = container;
            step.Controls.Add(container);
        }

        /// <devdoc>
        /// <para>Loads the control state.</para>
        /// </devdoc>
        protected internal override void LoadControlState(object state) {
            Triplet t = state as Triplet;
            if (t != null) {
                base.LoadControlState(t.First);

                Array collection = t.Second as Array;
                if (collection != null) {
                    Array.Reverse(collection);
                    _historyStack = new Stack<int>(collection.Cast<int>());
                }

                ActiveStepIndex = (int)t.Third;
            }
        }


        protected override void LoadViewState(object savedState) {
            if (savedState == null) {
                base.LoadViewState(null);
            } else {
                object[] myState = (object[])savedState;
                if (myState.Length != _viewStateArrayLength) {
                    throw new ArgumentException(SR.GetString(SR.ViewState_InvalidViewState));
                }

                base.LoadViewState(myState[0]);

                if (myState[1] != null)
                    ((IStateManager)NavigationButtonStyle).LoadViewState(myState[1]);

                if (myState[2] != null)
                    ((IStateManager)SideBarButtonStyle).LoadViewState(myState[2]);

                if (myState[3] != null)
                    ((IStateManager)HeaderStyle).LoadViewState(myState[3]);

                if (myState[4] != null)
                    ((IStateManager)NavigationStyle).LoadViewState(myState[4]);

                if (myState[5] != null)
                    ((IStateManager)SideBarStyle).LoadViewState(myState[5]);

                if (myState[6] != null)
                    ((IStateManager)StepStyle).LoadViewState(myState[6]);

                if (myState[7] != null)
                    ((IStateManager)StartNextButtonStyle).LoadViewState(myState[7]);

                if (myState[8] != null)
                    ((IStateManager)StepNextButtonStyle).LoadViewState(myState[8]);

                if (myState[9] != null)
                    ((IStateManager)StepPreviousButtonStyle).LoadViewState(myState[9]);

                if (myState[10] != null)
                    ((IStateManager)FinishPreviousButtonStyle).LoadViewState(myState[10]);

                if (myState[11] != null)
                    ((IStateManager)FinishCompleteButtonStyle).LoadViewState(myState[11]);

                if (myState[12] != null)
                    ((IStateManager)CancelButtonStyle).LoadViewState(myState[12]);

                if (myState[13] != null)
                    ((IStateManager)ControlStyle).LoadViewState(myState[13]);

                if (myState[14] != null)
                    DisplaySideBar = (bool)myState[14];
            }
        }


        public void MoveTo(WizardStepBase wizardStep) {
            if (wizardStep == null)
                throw new ArgumentNullException("wizardStep");

            int index = WizardSteps.IndexOf(wizardStep);
            if (index == -1) {
                throw new ArgumentException(SR.GetString(SR.Wizard_Step_Not_In_Wizard));
            }

            ActiveStepIndex = index;
        }


        protected virtual void OnActiveStepChanged(object source, EventArgs e) {
            EventHandler handler = (EventHandler)Events[_eventActiveStepChanged];
            if (handler != null) handler(this, e);
        }


        protected override bool OnBubbleEvent(object source, EventArgs e) {
            bool handled = false;

            CommandEventArgs ce = e as CommandEventArgs;
            if (ce != null) {
                if (String.Equals(CancelCommandName, ce.CommandName, StringComparison.OrdinalIgnoreCase)) {
                    OnCancelButtonClick(EventArgs.Empty);
                    return true;
                }

                int oldIndex = ActiveStepIndex;
                int newIndex = oldIndex;

                // Check if we need to validate the bubble commands VSWhidbey 312445
                bool verifyEvent = true;

                WizardStepType stepType = WizardStepType.Auto;
                WizardStepBase step = WizardSteps[oldIndex];

                // Don't validate commands if it's a templated wizard step
                if (step is TemplatedWizardStep) {
                    verifyEvent = false;
                } else {
                    stepType = GetStepType(step);
                }

                WizardNavigationEventArgs args = new WizardNavigationEventArgs(oldIndex, newIndex);

                // Do not navigate away from current view if view is not valid.
                if (_commandSender != null && Page != null && !Page.IsValid) {
                    args.Cancel = true;
                }

                bool previousButtonCommand = false;
                _activeStepIndexSet = false;

                if (String.Equals(MoveNextCommandName, ce.CommandName, StringComparison.OrdinalIgnoreCase)) {
                    if (verifyEvent) {
                        if (stepType != WizardStepType.Start && stepType != WizardStepType.Step) {
                            throw new InvalidOperationException(SR.GetString(SR.Wizard_InvalidBubbleEvent, MoveNextCommandName));
                        }
                    }

                    if (oldIndex < WizardSteps.Count - 1) {
                        args.SetNextStepIndex(oldIndex + 1);
                    }

                    OnNextButtonClick(args);
                    handled = true;
                } else if (String.Equals(MovePreviousCommandName, ce.CommandName, StringComparison.OrdinalIgnoreCase)) {
                    if (verifyEvent) {
                        if (stepType != WizardStepType.Step && stepType != WizardStepType.Finish) {
                            throw new InvalidOperationException(SR.GetString(SR.Wizard_InvalidBubbleEvent, MovePreviousCommandName));
                        }
                    }

                    previousButtonCommand = true;

                    int previousIndex = GetPreviousStepIndex(false);
                    if (previousIndex != -1) {
                        args.SetNextStepIndex(previousIndex);
                    }

                    OnPreviousButtonClick(args);
                    handled = true;
                } else if (String.Equals(MoveCompleteCommandName, ce.CommandName, StringComparison.OrdinalIgnoreCase)) {
                    if (verifyEvent) {
                        if (stepType != WizardStepType.Finish) {
                            throw new InvalidOperationException(SR.GetString(SR.Wizard_InvalidBubbleEvent, MoveCompleteCommandName));
                        }
                    }

                    if (oldIndex < WizardSteps.Count - 1) {
                        args.SetNextStepIndex(oldIndex + 1);
                    }

                    OnFinishButtonClick(args);
                    handled = true;
                } else if (String.Equals(MoveToCommandName, ce.CommandName, StringComparison.OrdinalIgnoreCase)) {
                    newIndex = Int32.Parse((String)ce.CommandArgument, CultureInfo.InvariantCulture);
                    args.SetNextStepIndex(newIndex);

                    handled = true;
                }

                if (handled) {
                    if (!args.Cancel) {
                        // Honor user's change if activeStepIndex is set explicitely;
                        if (!_activeStepIndexSet) {
                            // Make sure the next step is valid to navigate to
                            if (AllowNavigationToStep(args.NextStepIndex)) {
                                if (previousButtonCommand) {
                                    GetPreviousStepIndex(true);
                                }

                                ActiveStepIndex = args.NextStepIndex;
                            }
                        }
                    } else {
                        // revert active step if it's cancelled.
                        ActiveStepIndex = oldIndex;
                    }
                }
            }

            return handled;
        }

        internal void OnWizardStepsChanged() {
            SetStepsAndDataBindSideBarList(_sideBarList);
        }


        protected virtual bool AllowNavigationToStep(int index) {
            if (_historyStack != null && _historyStack.Contains(index)) {
                return WizardSteps[index].AllowReturn;
            }
            return true;
        }


        protected virtual void OnCancelButtonClick(EventArgs e) {
            EventHandler handler = (EventHandler)Events[_eventCancelButtonClick];
            if (handler != null) {
                handler(this, e);
            }

            string cancelDestinationUrl = CancelDestinationPageUrl;
            if (!String.IsNullOrEmpty(cancelDestinationUrl)) {
                Page.Response.Redirect(ResolveClientUrl(cancelDestinationUrl), false);
            }
        }


        private void OnCommand(object sender, CommandEventArgs e) {
            Debug.Assert(_commandSender == null);
            _commandSender = sender as IButtonControl;
        }


        protected virtual void OnFinishButtonClick(WizardNavigationEventArgs e) {
            WizardNavigationEventHandler handler = (WizardNavigationEventHandler)Events[_eventFinishButtonClick];
            if (handler != null) handler(this, e);

            string finishPageUrl = FinishDestinationPageUrl;
            if (!String.IsNullOrEmpty(finishPageUrl)) {
                // Microsoft suggested that we should not terminate execution of current page, to give
                // page a chance to cleanup its resources.  This may be less performant though.
                // Microsoft suggested that we need to call ResolveClientUrl before redirecting.
                // Example is this control inside user control, want redirect relative to user control dir.
                Page.Response.Redirect(ResolveClientUrl(finishPageUrl), false);
            }
        }


        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            // Always set the current step to the first step if not specified.
            if (ActiveStepIndex == -1 && WizardSteps.Count > 0 && !DesignMode) {
                ActiveStepIndex = 0;
            }

            // Add default control layout during OnInit to track WizardStep viewstate properly.
            EnsureChildControls();

            if (Page != null) {
                Page.RegisterRequiresControlState(this);
            }
        }


        protected virtual void OnNextButtonClick(WizardNavigationEventArgs e) {
            WizardNavigationEventHandler handler = (WizardNavigationEventHandler)Events[_eventNextButtonClick];
            if (handler != null) handler(this, e);
        }


        protected virtual void OnPreviousButtonClick(WizardNavigationEventArgs e) {
            WizardNavigationEventHandler handler = (WizardNavigationEventHandler)Events[_eventPreviousButtonClick];
            if (handler != null) handler(this, e);
        }


        protected virtual void OnSideBarButtonClick(WizardNavigationEventArgs e) {
            WizardNavigationEventHandler handler = (WizardNavigationEventHandler)Events[_eventSideBarButtonClick];
            if (handler != null) handler(this, e);
        }

        internal void RequiresControlsRecreation() {
            if (ChildControlsCreated) {
                using (new WizardControlCollectionModifier(this)) {
                    base.ChildControlsCreated = false;
                }
                _rendering = null;
            }
        }


        protected internal void RegisterCommandEvents(IButtonControl button) {
            if (button != null && button.CausesValidation) {
                button.Command += new CommandEventHandler(this.OnCommand);
            }
        }


        protected internal override void Render(HtmlTextWriter writer) {

            // Make sure we're in runat=server form.
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            EnsureChildControls();
            ApplyControlProperties();

            // Nothing to do if the Wizard is empty;
            if (ActiveStepIndex == -1 || WizardSteps.Count == 0) {
                return;
            }

            RenderContents(writer);
        }

        protected internal override object SaveControlState() {
            int activeStepIndex = ActiveStepIndex;

            // Save the ActiveStepIndex here so steps dynamically added
            // before or during OnPagePreRenderComplete will be tracked
            // properly. See VSWhidbey 395312
            if (_historyStack == null || _historyStack.Count == 0 ||
                _historyStack.Peek() != activeStepIndex) {
                // Remember the active step.
                History.Push(ActiveStepIndex);
            }

            object obj = base.SaveControlState();

            bool containsHistory = _historyStack != null && _historyStack.Count > 0;

            if (obj != null || containsHistory || activeStepIndex != -1) {
                object array = containsHistory ? _historyStack.ToArray() : null;
                return new Triplet(obj, array, activeStepIndex);
            }

            return null;
        }


        protected override object SaveViewState() {
            object[] myState = new object[_viewStateArrayLength];

            Debug.Assert(_viewStateArrayLength == 15, "Forgot to change array length when adding new item to view state?");

            myState[0] = base.SaveViewState();
            myState[1] = (_navigationButtonStyle != null) ? ((IStateManager)_navigationButtonStyle).SaveViewState() : null;
            myState[2] = (_sideBarButtonStyle != null) ? ((IStateManager)_sideBarButtonStyle).SaveViewState() : null;
            myState[3] = (_headerStyle != null) ? ((IStateManager)_headerStyle).SaveViewState() : null;
            myState[4] = (_navigationStyle != null) ? ((IStateManager)_navigationStyle).SaveViewState() : null;
            myState[5] = (_sideBarStyle != null) ? ((IStateManager)_sideBarStyle).SaveViewState() : null;
            myState[6] = (_stepStyle != null) ? ((IStateManager)_stepStyle).SaveViewState() : null;
            myState[7] = (_startNextButtonStyle != null) ? ((IStateManager)_startNextButtonStyle).SaveViewState() : null;
            myState[8] = (_stepNextButtonStyle != null) ? ((IStateManager)_stepNextButtonStyle).SaveViewState() : null;
            myState[9] = (_stepPreviousButtonStyle != null) ? ((IStateManager)_stepPreviousButtonStyle).SaveViewState() : null;
            myState[10] = (_finishPreviousButtonStyle != null) ? ((IStateManager)_finishPreviousButtonStyle).SaveViewState() : null;
            myState[11] = (_finishCompleteButtonStyle != null) ? ((IStateManager)_finishCompleteButtonStyle).SaveViewState() : null;
            myState[12] = (_cancelButtonStyle != null) ? ((IStateManager)_cancelButtonStyle).SaveViewState() : null;
            myState[13] = ControlStyleCreated ? ((IStateManager)ControlStyle).SaveViewState() : null;
            if (DisplaySideBar != _displaySideBarDefault) {
                myState[14] = DisplaySideBar;
            }

            for (int i = 0; i < _viewStateArrayLength; i++) {
                if (myState[i] != null) {
                    return myState;
                }
            }

            // More performant to return null than an array of null values
            return null;
        }


        private void SetCancelButtonVisibility(BaseNavigationTemplateContainer container) {
            // Make the parent TD invisible if possible.
            Control c = container.CancelButton as Control;

            if (c != null) {
                Control parent = c.Parent;
                if (parent != null) {
                    Debug.Assert(parent is TableCell);
                    parent.Visible = DisplayCancelButton;
                }

                c.Visible = DisplayCancelButton;
            }
        }


        protected override void TrackViewState() {
            base.TrackViewState();

            if (_navigationButtonStyle != null)
                ((IStateManager)_navigationButtonStyle).TrackViewState();

            if (_sideBarButtonStyle != null)
                ((IStateManager)_sideBarButtonStyle).TrackViewState();

            if (_headerStyle != null)
                ((IStateManager)_headerStyle).TrackViewState();

            if (_navigationStyle != null)
                ((IStateManager)_navigationStyle).TrackViewState();

            if (_sideBarStyle != null)
                ((IStateManager)_sideBarStyle).TrackViewState();

            if (_stepStyle != null)
                ((IStateManager)_stepStyle).TrackViewState();

            if (_startNextButtonStyle != null)
                ((IStateManager)_startNextButtonStyle).TrackViewState();

            if (_stepPreviousButtonStyle != null)
                ((IStateManager)_stepPreviousButtonStyle).TrackViewState();

            if (_stepNextButtonStyle != null)
                ((IStateManager)_stepNextButtonStyle).TrackViewState();

            if (_finishPreviousButtonStyle != null)
                ((IStateManager)_finishPreviousButtonStyle).TrackViewState();

            if (_finishCompleteButtonStyle != null)
                ((IStateManager)_finishCompleteButtonStyle).TrackViewState();

            if (_cancelButtonStyle != null)
                ((IStateManager)_cancelButtonStyle).TrackViewState();

            if (ControlStyleCreated) {
                ((IStateManager)ControlStyle).TrackViewState();
            }
        }

        private static void ValidateButtonType(ButtonType value) {
            if (value < ButtonType.Button || value > ButtonType.Link) {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        #region Rendering Abstractions

        internal abstract class WizardRenderingBase {
            private const string _startNavigationTemplateContainerID = "StartNavigationTemplateContainerID";
            private const string _stepNavigationTemplateContainerID = "StepNavigationTemplateContainerID";
            private const string _finishNavigationTemplateContainerID = "FinishNavigationTemplateContainerID";

            private NavigationTemplate _defaultStartNavigationTemplate;
            private NavigationTemplate _defaultStepNavigationTemplate;
            private NavigationTemplate _defaultFinishNavigationTemplate;

            protected BaseNavigationTemplateContainer _finishNavigationTemplateContainer;
            protected BaseNavigationTemplateContainer _startNavigationTemplateContainer;
            protected BaseNavigationTemplateContainer _stepNavigationTemplateContainer;

            protected Wizard Owner { get; private set; }

            protected WizardRenderingBase(Wizard wizard) {
                Owner = wizard;
            }

            public abstract void ApplyControlProperties();

            public abstract void CreateControlHierarchy();

            public virtual void SetDesignModeState(IDictionary dictionary) {
                if (_startNavigationTemplateContainer != null) {
                    dictionary[StartNextButtonID] = _startNavigationTemplateContainer.NextButton;
                    dictionary[CancelButtonID] = _startNavigationTemplateContainer.CancelButton;
                }

                if (_stepNavigationTemplateContainer != null) {
                    dictionary[StepNextButtonID] = _stepNavigationTemplateContainer.NextButton;
                    dictionary[StepPreviousButtonID] = _stepNavigationTemplateContainer.PreviousButton;
                    dictionary[CancelButtonID] = _stepNavigationTemplateContainer.CancelButton;
                }

                if (_finishNavigationTemplateContainer != null) {
                    dictionary[FinishPreviousButtonID] = _finishNavigationTemplateContainer.PreviousButton;
                    dictionary[FinishButtonID] = _finishNavigationTemplateContainer.FinishButton;
                    dictionary[CancelButtonID] = _finishNavigationTemplateContainer.CancelButton;
                }
            }


            protected void ApplyControlProperties_Sidebar() {
                if (Owner.SideBarEnabled) {
                    Owner.SetStepsAndDataBindSideBarList(Owner._sideBarList);

                    // Only apply the styles to the sidebar or sidebar buttons if custom template is not used.
                    if (Owner.SideBarTemplate == null) {
                        foreach (Control item in Owner._sideBarList.Items) {
                            WebControl button = item.FindControl(SideBarButtonID) as WebControl;
                            if (button != null) {
                                button.MergeStyle(Owner._sideBarButtonStyle);
                            }
                        }
                    }
                }
            }


            protected void ApplyNavigationTemplateProperties() {
                // Do not apply any template properties if the containers are null
                // This happens when GetDesignModeState is called before child controls are created.
                if (_finishNavigationTemplateContainer == null ||
                    _startNavigationTemplateContainer == null ||
                    _stepNavigationTemplateContainer == null) {
                    return;
                }

                WizardStepType renderType = WizardStepType.Start;
                // Set the active templates based on ActiveStep
                // Make sure the activestepindex is valid. // VSWhidbey 376438
                if (Owner.ActiveStepIndex >= Owner.WizardSteps.Count || Owner.ActiveStepIndex < 0) {
                    return;
                }

                renderType = SetActiveTemplates();

                bool requiresFinishPreviousButton =
                    renderType != WizardStepType.Finish || Owner.ActiveStepIndex != 0 || Owner.ActiveStep.StepType != WizardStepType.Auto;

                ApplyDefaultStartNavigationTemplateProperties();

                bool showPrevious = true;
                int prevStepIndex = Owner.GetPreviousStepIndex(false);
                if (prevStepIndex >= 0) {
                    showPrevious = Owner.WizardSteps[prevStepIndex].AllowReturn;
                }

                ApplyDefaultFinishNavigationTemplateProperties(showPrevious);
                ApplyDefaultStepNavigationTemplateProperties(showPrevious);

                // if the first step using auto type is assigned to a finish step, do not render the previous button's container.
                if (!requiresFinishPreviousButton) {
                    Control ctrl = _finishNavigationTemplateContainer.PreviousButton as Control;
                    if (ctrl != null) {
                        if (Owner.FinishNavigationTemplate == null) {
                            ctrl.Parent.Visible = false;
                        } else {
                            ctrl.Visible = false;
                        }
                    }
                }
            }

            private void ApplyDefaultStepNavigationTemplateProperties(bool previousImageButtonVisible) {
                // Only apply properties to the default template (if a custom template has not been specified)
                if (Owner.StepNavigationTemplate != null) {
                    return;
                }

                var navContainer = _stepNavigationTemplateContainer;
                NavigationTemplate defaultTemplate = _defaultStepNavigationTemplate;

                if (Owner.DesignMode) {
                    defaultTemplate.ResetButtonsVisibility();
                }

                navContainer.PreviousButton = defaultTemplate.FirstButton;
                ((Control)navContainer.PreviousButton).Visible = true;

                navContainer.NextButton = defaultTemplate.SecondButton;
                ((Control)navContainer.NextButton).Visible = true;

                navContainer.CancelButton = defaultTemplate.CancelButton;

                ApplyButtonProperties(navContainer.NextButton, Owner.StepNextButtonText, Owner.StepNextButtonImageUrl);

                ApplyButtonProperties(navContainer.PreviousButton, Owner.StepPreviousButtonText, Owner.StepPreviousButtonImageUrl, previousImageButtonVisible);

                ApplyButtonProperties(navContainer.CancelButton, Owner.CancelButtonText, Owner.CancelButtonImageUrl);

                int previousStepIndex = Owner.GetPreviousStepIndex(false);
                if (previousStepIndex != -1 && !Owner.WizardSteps[previousStepIndex].AllowReturn) {
                    ((Control)navContainer.PreviousButton).Visible = false;
                }

                Owner.SetCancelButtonVisibility(navContainer);
                navContainer.ApplyButtonStyle(Owner.FinishCompleteButtonStyle, Owner.StepPreviousButtonStyle, Owner.StepNextButtonStyle, Owner.CancelButtonStyle);
            }

            private void ApplyDefaultFinishNavigationTemplateProperties(bool previousImageButtonVisible) {
                // Only apply properties to the default template (if a custom template has not been specified)
                if (Owner.FinishNavigationTemplate != null) {
                    return;
                }

                var finishContainer = _finishNavigationTemplateContainer;
                NavigationTemplate defaultTemplate = _defaultFinishNavigationTemplate;

                if (Owner.DesignMode) {
                    defaultTemplate.ResetButtonsVisibility();
                }

                finishContainer.PreviousButton = defaultTemplate.FirstButton;
                ((Control)finishContainer.PreviousButton).Visible = true;

                finishContainer.FinishButton = defaultTemplate.SecondButton;
                ((Control)finishContainer.FinishButton).Visible = true;

                finishContainer.CancelButton = defaultTemplate.CancelButton;

                finishContainer.FinishButton.CommandName = MoveCompleteCommandName;

                ApplyButtonProperties(finishContainer.FinishButton, Owner.FinishCompleteButtonText, Owner.FinishCompleteButtonImageUrl);

                ApplyButtonProperties(finishContainer.PreviousButton, Owner.FinishPreviousButtonText, Owner.FinishPreviousButtonImageUrl, previousImageButtonVisible);

                ApplyButtonProperties(finishContainer.CancelButton, Owner.CancelButtonText, Owner.CancelButtonImageUrl);

                int previousStepIndex = Owner.GetPreviousStepIndex(false);
                if (previousStepIndex != -1 && !Owner.WizardSteps[previousStepIndex].AllowReturn) {
                    ((Control)finishContainer.PreviousButton).Visible = false;
                }

                Owner.SetCancelButtonVisibility(finishContainer);
                finishContainer.ApplyButtonStyle(Owner.FinishCompleteButtonStyle, Owner.FinishPreviousButtonStyle, Owner.StepNextButtonStyle, Owner.CancelButtonStyle);
            }

            private void ApplyDefaultStartNavigationTemplateProperties() {
                // Only apply properties to the default template (if a custom template has not been specified)
                if (Owner.StartNavigationTemplate != null) {
                    return;
                }

                var startContainer = _startNavigationTemplateContainer;
                NavigationTemplate defaultStartTemplate = _defaultStartNavigationTemplate;

                if (Owner.DesignMode) {
                    defaultStartTemplate.ResetButtonsVisibility();
                }

                startContainer.NextButton = defaultStartTemplate.SecondButton;
                ((Control)startContainer.NextButton).Visible = true;

                startContainer.CancelButton = defaultStartTemplate.CancelButton;

                ApplyButtonProperties(startContainer.NextButton, Owner.StartNextButtonText, Owner.StartNextButtonImageUrl);

                ApplyButtonProperties(startContainer.CancelButton, Owner.CancelButtonText, Owner.CancelButtonImageUrl);

                Owner.SetCancelButtonVisibility(startContainer);
                startContainer.ApplyButtonStyle(Owner.FinishCompleteButtonStyle, Owner.StepPreviousButtonStyle, Owner.StartNextButtonStyle, Owner.CancelButtonStyle);
            }


            protected virtual WizardStepType SetActiveTemplates() {
                WizardStepType type = Owner.GetStepType(Owner.ActiveStepIndex);

                _startNavigationTemplateContainer.Visible = (type == WizardStepType.Start);
                _stepNavigationTemplateContainer.Visible = (type == WizardStepType.Step);
                _finishNavigationTemplateContainer.Visible = (type == WizardStepType.Finish);

                // Do not render header or sidebarlist in complete steps;
                if (type == WizardStepType.Complete) {
                    OnlyShowCompleteStep();
                }

                return type;
            }

            private static void ApplyButtonProperties(IButtonControl button, string text, string imageUrl) {
                ApplyButtonProperties(button, text, imageUrl, true);
            }

            private static void ApplyButtonProperties(IButtonControl button, string text, string imageUrl, bool imageButtonVisible) {
                if (button == null) {
                    return;
                }

                ImageButton imageButton = button as ImageButton;
                if (imageButton != null) {
                    imageButton.ImageUrl = imageUrl;
                    imageButton.AlternateText = text;
                    imageButton.Visible = imageButtonVisible;
                } else {
                    button.Text = text;
                }
            }

            public abstract void OnlyShowCompleteStep();

            protected void ApplyCustomNavigationTemplateProperties() {
                // Make all custom navigation containers invisible
                foreach (BaseNavigationTemplateContainer c in Owner.CustomNavigationContainers.Values) {
                    c.Visible = false;
                }

                // If we are going to show a custom navigation template, make that one visible
                if (Owner.ShowCustomNavigationTemplate) {
                    BaseNavigationTemplateContainer container = Owner._customNavigationContainers[Owner.ActiveStep];
                    container.Visible = true;
                    _startNavigationTemplateContainer.Visible = false;
                    _stepNavigationTemplateContainer.Visible = false;
                    _finishNavigationTemplateContainer.Visible = false;
                }
            }


            protected void CreateControlHierarchy_CleanUpOldSideBarList(IWizardSideBarListControl sideBarList) {
                // Remove obsolete events from sideBarDataList
                if (sideBarList != null) {
                    sideBarList.ItemCommand -= new CommandEventHandler(Owner.DataListItemCommand);
                    sideBarList.ItemDataBound -= new EventHandler<WizardSideBarListControlItemEventArgs>(Owner.DataListItemDataBound);
                }
            }

            protected IWizardSideBarListControl CreateControlHierarchy_SetUpSideBarList(Control sideBarContainer) {
                var sideBarList = sideBarContainer.FindControl(DataListID) as IWizardSideBarListControl;
                if (sideBarList != null) {
                    sideBarList.ItemCommand += new CommandEventHandler(Owner.DataListItemCommand);
                    sideBarList.ItemDataBound += new EventHandler<WizardSideBarListControlItemEventArgs>(Owner.DataListItemDataBound);
                    if (Owner.DesignMode) {
                        // This line is necessary for ListView to databind correctly to the list of steps when
                        // viewing the control on the design surface. ListView will not databind automatically when
                        // it is not a top-level control (as is the case here) unless the flag has a value.
                        ((IControlDesignerAccessor)sideBarList).GetDesignModeState()["EnableDesignTimeDataBinding"] = true;
                    }
                    Owner.SetStepsAndDataBindSideBarList(sideBarList);
                } else {
                    // Do not throw at designmode otherwise template will not be persisted correctly.
                    if (!Owner.DesignMode) {
                        throw new InvalidOperationException(
                            SR.GetString(SR.Wizard_DataList_Not_Found, DataListID));
                    }
                }

                return sideBarList;
            }


            // Helper method to create navigation templates.
            protected void CreateNavigationControlHierarchy(Control container) {
                container.Controls.Clear();
                Owner.CustomNavigationContainers.Clear();
                Owner.CreateCustomNavigationTemplates();

                foreach (BaseNavigationTemplateContainer c in Owner.CustomNavigationContainers.Values) {
                    container.Controls.Add(c);
                }

                CreateStartNavigationTemplate(container);
                CreateFinishNavigationTemplate(container);
                CreateStepNavigationTemplate(container);
            }


            private void CreateStartNavigationTemplate(Control container) {
                // Start navigation template
                ITemplate startNavigationTemplate = Owner.StartNavigationTemplate;
                _startNavigationTemplateContainer = new StartNavigationTemplateContainer(Owner);
                _startNavigationTemplateContainer.ID = _startNavigationTemplateContainerID;

                // Use the default template
                if (startNavigationTemplate == null) {
                    _startNavigationTemplateContainer.EnableViewState = false;
                    _defaultStartNavigationTemplate = NavigationTemplate.GetDefaultStartNavigationTemplate(Owner);
                    startNavigationTemplate = _defaultStartNavigationTemplate;
                } else {
                    // Custom template is used here.
                    _startNavigationTemplateContainer.SetEnableTheming();
                }

                startNavigationTemplate.InstantiateIn(_startNavigationTemplateContainer);
                container.Controls.Add(_startNavigationTemplateContainer);
            }

            private void CreateStepNavigationTemplate(Control container) {
                // step navigation template
                ITemplate stepNavigationTemplate = Owner.StepNavigationTemplate;
                _stepNavigationTemplateContainer = new StepNavigationTemplateContainer(Owner);
                _stepNavigationTemplateContainer.ID = _stepNavigationTemplateContainerID;

                if (stepNavigationTemplate == null) {
                    _stepNavigationTemplateContainer.EnableViewState = false;
                    _defaultStepNavigationTemplate = NavigationTemplate.GetDefaultStepNavigationTemplate(Owner);
                    stepNavigationTemplate = _defaultStepNavigationTemplate;
                } else {
                    _stepNavigationTemplateContainer.SetEnableTheming();
                }

                stepNavigationTemplate.InstantiateIn(_stepNavigationTemplateContainer);
                container.Controls.Add(_stepNavigationTemplateContainer);
            }

            private void CreateFinishNavigationTemplate(Control container) {
                // finish navigation template
                ITemplate finishNavigationTemplate = Owner.FinishNavigationTemplate;
                _finishNavigationTemplateContainer = new FinishNavigationTemplateContainer(Owner);
                _finishNavigationTemplateContainer.ID = _finishNavigationTemplateContainerID;

                if (finishNavigationTemplate == null) {
                    _finishNavigationTemplateContainer.EnableViewState = false;
                    _defaultFinishNavigationTemplate = NavigationTemplate.GetDefaultFinishNavigationTemplate(Owner);
                    finishNavigationTemplate = _defaultFinishNavigationTemplate;
                } else {
                    _finishNavigationTemplateContainer.SetEnableTheming();
                }

                finishNavigationTemplate.InstantiateIn(_finishNavigationTemplateContainer);
                container.Controls.Add(_finishNavigationTemplateContainer);
            }
        }

        internal class LayoutTemplateWizardRendering : WizardRenderingBase {

            private Literal _headerLiteral;
            private WizardContainer _layoutContainer;

            public LayoutTemplateWizardRendering(Wizard wizard)
                : base(wizard) {
            }

            public override void ApplyControlProperties() {
                ApplyControlProperties_Header();
                ApplyControlProperties_Sidebar();
                // nothing interesting to apply to the wizard steps
                ApplyControlProperties_Navigation();
            }

            private void ApplyControlProperties_Navigation() {
                ApplyNavigationTemplateProperties();
                ApplyCustomNavigationTemplateProperties();
            }

            private void ApplyControlProperties_Header() {
                if (Owner.HeaderTemplate != null) {
                    // nothing to apply if there is a header template
                    return;
                }

                if (_headerLiteral != null) {
                    // not necessary to set the literal to not visible if the text is empty because a
                    // literal with empty text does not render anything
                    _headerLiteral.Text = Owner.HeaderText;
                } else if (!String.IsNullOrEmpty(Owner.HeaderText)) {
                    // only throw if the headertext is set and there is no headerLiteral. This means
                    // that the layout template did not contain a header placeholder.

                    throw new InvalidOperationException(SR.GetString(SR.Wizard_Header_Placeholder_Must_Be_Specified_For_HeaderText, Owner.ID, HeaderPlaceholderId));
                }
            }


            public override void OnlyShowCompleteStep() {
                _layoutContainer.ControlToRender = Owner.MultiView;
            }


            public override void CreateControlHierarchy() {
                _layoutContainer = new WizardContainer();
                Owner.LayoutTemplate.InstantiateIn(_layoutContainer);

                using (new WizardControlCollectionModifier(Owner)) {
                    Owner.Controls.Add(_layoutContainer);
                }

                CreateControlHierarchy_Header(_layoutContainer);
                CreateControlHierarchy_SideBar(_layoutContainer);
                CreateControlHierarchy_WizardStep(_layoutContainer);
                CreateControlHierarchy_Navigation(_layoutContainer);
            }

            private void CreateControlHierarchy_Navigation(Control layoutContainer) {
                var placeholder = layoutContainer.FindControl(NavigationPlaceholderId);
                if (placeholder == null) {
                    throw new InvalidOperationException(SR.GetString(SR.Wizard_Navigation_Placeholder_Must_Be_Specified, Owner.ID, NavigationPlaceholderId));
                }

                Control navigationContainer = new Control();

                ReplacePlaceholderWithControl(layoutContainer, placeholder, navigationContainer);
                CreateNavigationControlHierarchy(navigationContainer);
            }

            private void CreateControlHierarchy_Header(Control layoutContainer) {
                var placeholder = layoutContainer.FindControl(HeaderPlaceholderId);

                if (Owner.HeaderTemplate != null) {
                    if (placeholder == null) {
                        throw new InvalidOperationException(SR.GetString(SR.Wizard_Header_Placeholder_Must_Be_Specified_For_HeaderTemplate, Owner.ID, HeaderPlaceholderId));
                    } else {
                        // just replace the placeholder with the template
                        ReplacePlaceholderWithTemplateInstance(layoutContainer, placeholder, Owner.HeaderTemplate);
                    }
                } else if (placeholder != null) {
                    // only create a header literal if the template contains a placeholder
                    // if the template does not contain a placeholder an exception will be thrown from
                    // ApplyControlProperties
                    _headerLiteral = new Literal();
                    ReplacePlaceholderWithControl(layoutContainer, placeholder, _headerLiteral);
                }
            }

            private void CreateControlHierarchy_SideBar(Control layoutContainer) {
                if (!Owner.DisplaySideBar) {
                    // it's ok to exit here since changing the DisplaySideBar value would
                    // force a recreation of the control tree.
                    return;
                }

                var placeholder = layoutContainer.FindControl(SideBarPlaceholderId);

                if (placeholder == null) {
                    // 
                    throw new InvalidOperationException(SR.GetString(SR.Wizard_Sidebar_Placeholder_Must_Be_Specified, Owner.ID, SideBarPlaceholderId));
                }

                ITemplate sideBarTemplate = Owner.SideBarTemplate ?? Owner.CreateDefaultSideBarTemplate();

                ReplacePlaceholderWithTemplateInstance(layoutContainer, placeholder, sideBarTemplate);

                CreateControlHierarchy_CleanUpOldSideBarList(Owner.SideBarList);
                Owner._sideBarList = CreateControlHierarchy_SetUpSideBarList(layoutContainer);
            }

            private void CreateControlHierarchy_WizardStep(Control layoutContainer) {
                var placeholder = layoutContainer.FindControl(WizardStepPlaceholderId);

                if (placeholder == null) {
                    throw new InvalidOperationException(SR.GetString(SR.Wizard_Step_Placeholder_Must_Be_Specified, Owner.ID, WizardStepPlaceholderId));
                }

                ReplacePlaceholderWithControl(layoutContainer, placeholder, Owner.MultiView);
            }

            private static void ReplacePlaceholderWithTemplateInstance(Control targetContainer, Control placeholder, ITemplate template) {
                var templateContainer = new Control();
                template.InstantiateIn(templateContainer);

                ReplacePlaceholderWithControl(targetContainer, placeholder, templateContainer);
            }

            private static void ReplacePlaceholderWithControl(Control targetContainer, Control placeholder, Control replacement) {
                var placeholderIndex = targetContainer.Controls.IndexOf(placeholder);
                targetContainer.Controls.RemoveAt(placeholderIndex);

                targetContainer.Controls.AddAt(placeholderIndex, replacement);
            }

            internal class WizardContainer : WebControl {
                internal Control ControlToRender { get; set; }

                protected internal override void Render(HtmlTextWriter writer) {
                    if (ControlToRender == null) {
                        RenderChildren(writer);
                    } else {
                        ControlToRender.Render(writer);
                    }
                }
            }
        }

        internal class TableWizardRendering : WizardRenderingBase {
            private const string _headerCellID = "HeaderContainer";
            private const string _sideBarCellID = "SideBarContainer";
            private const string _stepTableCellID = "StepTableCell";

            private TableCell _headerTableCell;
            private TableRow _headerTableRow;
            private TableRow _navigationRow;
            private TableCell _navigationTableCell;
            private Table _renderTable;
            private TableCell _stepTableCell;

            private LiteralControl _titleLiteral;

            public TableWizardRendering(Wizard wizard)
                : base(wizard) {
            }

            // Returns the cell which the navigation template container should be added to
            private TableCell NavigationTableCell {
                get {
                    if (_navigationTableCell == null) {
                        _navigationTableCell = new TableCell();
                    }
                    return _navigationTableCell;
                }
            }

            public override void ApplyControlProperties() {
                // Nothing to do if the Wizard is empty;
                // Apply the control properties at designtime so that controls inside templates are styled properly.
                if (!Owner.DesignMode &&
                    (Owner.ActiveStepIndex < 0 || Owner.ActiveStepIndex >= Owner.WizardSteps.Count || Owner.WizardSteps.Count == 0)) {
                    return;
                }

                if (Owner.SideBarEnabled && Owner._sideBarStyle != null) {
                    // Apply Sidebar style on the table cell VSWhidbey 377064
                    Owner._sideBarTableCell.ApplyStyle(Owner._sideBarStyle);
                }

                ApplyControlProperties_Header();

                ApplyControlProperties_WizardSteps();

                ApplyControlProperties_Navigation();

                ApplyControlProperties_Sidebar();

                if (_renderTable != null) {
                    // Clear out the accesskey/tab index so it doesn't get applied to the tables in the container
                    Util.CopyBaseAttributesToInnerControl(Owner, _renderTable);

                    if (Owner.ControlStyleCreated) {
                        _renderTable.ApplyStyle(Owner.ControlStyle);
                    } else {
                        // initialize defaults that are different from TableStyle
                        _renderTable.CellSpacing = 0;
                        _renderTable.CellPadding = 0;
                    }

                    // On Mac IE, if height is not set, render height:1px, so the table sizes to contents.
                    // Otherwise, Mac IE may give the table an arbitrary height (equal to the width of its contents).
                    if (!Owner.DesignMode && Owner.IsMacIE5 &&
                        (!Owner.ControlStyleCreated || Owner.ControlStyle.Height == Unit.Empty)) {
                        _renderTable.ControlStyle.Height = Unit.Pixel(1);
                    }
                }

                // On Mac IE, render height:1px of the inner table cell, so the table sizes to contents.
                // Otherwise, Mac IE may give the table an arbitrary height (equal to the width of its contents).
                if (!Owner.DesignMode && _navigationTableCell != null && Owner.IsMacIE5) {
                    _navigationTableCell.ControlStyle.Height = Unit.Pixel(1);
                }
            }


            private void ApplyControlProperties_Navigation() {
                ApplyNavigationTemplateProperties();
                ApplyCustomNavigationTemplateProperties();

                if (_navigationTableCell != null) {
                    NavigationTableCell.HorizontalAlign = HorizontalAlign.Right;
                    // Copy the styles from the StepStyle property if defined.
                    if (Owner._navigationStyle != null) {
                        if (!Owner.DesignMode && Owner.IsMacIE5 && Owner._navigationStyle.Height == Unit.Empty) {
                            Owner._navigationStyle.Height = Unit.Pixel(1);
                        }

                        _navigationTableCell.ApplyStyle(Owner._navigationStyle);
                    }
                }

                if (Owner.ShowCustomNavigationTemplate) {
                    // Make sure the navigation row is visible
                    _navigationRow.Visible = true;
                }
            }

            private void ApplyControlProperties_WizardSteps() {
                // Apply the WizardSteps style
                if (_stepTableCell != null) {
                    // Copy the styles from the StepStyle property if defined.
                    if (Owner._stepStyle != null) {
                        if (!Owner.DesignMode && Owner.IsMacIE5 && Owner._stepStyle.Height == Unit.Empty) {
                            Owner._stepStyle.Height = Unit.Pixel(1);
                        }

                        _stepTableCell.ApplyStyle(Owner._stepStyle);
                    }
                }
            }

            private void ApplyControlProperties_Header() {
                if (_headerTableRow != null) {
                    // If headerTemplate is not defined and headertext is empty, do not render the
                    // empty table row.
                    if ((Owner.HeaderTemplate == null) && String.IsNullOrEmpty(Owner.HeaderText)) {
                        _headerTableRow.Visible = false;
                    } else {
                        _headerTableCell.ApplyStyle(Owner._headerStyle);

                        // if HeaderTemplate is defined.
                        if (Owner.HeaderTemplate != null) {
                            if (_titleLiteral != null) {
                                _titleLiteral.Visible = false;
                            }
                        }
                            // Otherwise HeaderText is defined.
                        else {
                            Debug.Assert(Owner.HeaderText != null && Owner.HeaderText.Length > 0);
                            if (_titleLiteral != null) {
                                _titleLiteral.Text = Owner.HeaderText;
                            }
                        }
                    }
                }
            }


            protected override WizardStepType SetActiveTemplates() {
                WizardStepType type = base.SetActiveTemplates();

                if (type != WizardStepType.Complete) {
                    // Only render sidebartablecell if necessary
                    if (Owner._sideBarTableCell != null) {
                        Owner._sideBarTableCell.Visible = Owner.SideBarEnabled && Owner._renderSideBarDataList;
                    }
                }

                return type;
            }

            public override void OnlyShowCompleteStep() {
                // Do not render header or sidebarlist in complete steps;
                if (_headerTableRow != null) {
                    _headerTableRow.Visible = false;
                }

                if (Owner._sideBarTableCell != null) {
                    Owner._sideBarTableCell.Visible = false;
                }

                _navigationRow.Visible = false;
            }


            public override void CreateControlHierarchy() {
                // Use the inner table to render header template, step and navigation template
                Table mainContentTable = null;

                if (Owner.DisplaySideBar) {
                    mainContentTable = CreateControlHierarchy_CreateLayoutWithSideBar();
                } else {
                    mainContentTable = CreateControlHierarchy_CreateLayoutWithoutSideBar();
                }

                CreateControlHierarchy_CreateHeaderArea(mainContentTable);

                CreateControlHierarchy_CreateStepArea(mainContentTable);

                CreateControlHierarchy_CreateNavigationArea(mainContentTable);
            }


            private void CreateControlHierarchy_CreateNavigationArea(Table mainContentTable) {
                _navigationRow = new TableRow();
                mainContentTable.Controls.Add(_navigationRow);
                _navigationRow.Controls.Add(NavigationTableCell);

                CreateNavigationControlHierarchy(NavigationTableCell);
            }

            private void CreateControlHierarchy_CreateStepArea(Table mainContentTable) {
                TableRow stepRow = new TableRow() {
                    // The step row needs to use the most of the table size.
                    Height = Unit.Percentage(100)
                };
                mainContentTable.Controls.Add(stepRow);
                _stepTableCell = new TableCell();

                stepRow.Controls.Add(_stepTableCell);

                _stepTableCell.Controls.Add(Owner.MultiView);

                Owner.InstantiateStepContentTemplates();
            }

            private void CreateControlHierarchy_CreateHeaderArea(Table mainContentTable) {
                _headerTableRow = new TableRow();
                mainContentTable.Controls.Add(_headerTableRow);
                _headerTableCell = new InternalTableCell(Owner) {
                    ID = _headerCellID
                };

                if (Owner.HeaderTemplate != null) {
                    _headerTableCell.EnableTheming = Owner.EnableTheming;
                    Owner.HeaderTemplate.InstantiateIn(_headerTableCell);
                } else {
                    // Render the title property if HeaderTemplate is not defined.
                    _titleLiteral = new LiteralControl();
                    _headerTableCell.Controls.Add(_titleLiteral);
                }

                _headerTableRow.Controls.Add(_headerTableCell);
            }

            private Table CreateControlHierarchy_CreateLayoutWithoutSideBar() {
                // if sidebar is disabled, add mainContentTable directly into the Wizard control.
                var mainContentTable = new WizardChildTable(Owner) {
                    EnableTheming = false
                };
                using (new WizardControlCollectionModifier(Owner)) {
                    Owner.Controls.Add(mainContentTable);
                }
                _renderTable = mainContentTable;
                return mainContentTable;
            }

            private Table CreateControlHierarchy_CreateLayoutWithSideBar() {
                // Create an outer table and make all child controls into the right cell.
                // Use the left cell to render the side bar.

                Table outerTable = new WizardChildTable(Owner) {
                    EnableTheming = false
                };

                TableRow outerRow = new TableRow();
                outerTable.Controls.Add(outerRow);

                // Use the existing sideBarTableCell if possible.
                TableCell outerLeftCell = Owner._sideBarTableCell ?? CreateControlHierarchy_CreateSideBarTableCell();
                outerRow.Controls.Add(outerLeftCell);

                Owner._sideBarTableCell = outerLeftCell;
                Owner._renderSideBarDataList = false;

                // Right cell is used for header, step and navigation areas.
                TableCell outerRightCell = new TableCell() {
                    // Maximize the inner default table Whidbey 143409.
                    Height = Unit.Percentage(100)
                };

                outerRow.Controls.Add(outerRightCell);

                var mainContentTable = new WizardDefaultInnerTable() {
                    CellSpacing = 0,
                    Height = Unit.Percentage(100),
                    Width = Unit.Percentage(100)
                };

                outerRightCell.Controls.Add(mainContentTable);

                // On Mac IE, render height:1px of the inner table cell, so the table sizes to contents.
                // Otherwise, Mac IE may give the table an arbitrary height (equal to the width of its contents).
                if (!Owner.DesignMode && Owner.IsMacIE5) {
                    outerRightCell.Height = Unit.Pixel(1);
                }

                // Add the table into the Wizard control
                using (new WizardControlCollectionModifier(Owner)) {
                    Owner.Controls.Add(outerTable);
                }

                CreateControlHierarchy_CleanUpOldSideBarList(Owner.SideBarList);
                Owner._sideBarList = CreateControlHierarchy_SetUpSideBarList(Owner._sideBarTableCell);

                _renderTable = outerTable;
                return mainContentTable;
            }

            private TableCell CreateControlHierarchy_CreateSideBarTableCell() {
                // Left cell is used for SideBar
                TableCell outerLeftCell = new AccessibleTableCell(Owner) {
                    ID = _sideBarCellID,
                    // Left cell should expand to all height if sidebar is displayed.
                    Height = Unit.Percentage(100)
                };

                ITemplate sideBarTemplate = Owner.SideBarTemplate;

                if (sideBarTemplate == null) {
                    outerLeftCell.EnableViewState = false;
                    sideBarTemplate = Owner.CreateDefaultSideBarTemplate();
                } else {
                    outerLeftCell.EnableTheming = Owner.EnableTheming;
                }
                sideBarTemplate.InstantiateIn(outerLeftCell);

                return outerLeftCell;
            }


            public override void SetDesignModeState(IDictionary dictionary) {
                base.SetDesignModeState(dictionary);
                dictionary[_stepTableCellID] = _stepTableCell;

            }
        }

        #endregion

        private class WizardControlCollection : ControlCollection {
            public WizardControlCollection(Wizard wizard)
                : base(wizard) {
                if (!wizard.DesignMode)
                    SetCollectionReadOnly(SR.Wizard_Cannot_Modify_ControlCollection);
            }
        }

        private class WizardControlCollectionModifier : IDisposable {
            Wizard _wizard;
            ControlCollection _controls;
            String _originalError;

            public WizardControlCollectionModifier(Wizard wizard) {
                _wizard = wizard;
                if (!_wizard.DesignMode) {
                    // Remember the ControlCollection so we don't need to access the
                    // Controls property by GC during Dispose. Accessing this property has the
                    // side-effect of creating the entire child controls on CompositeControl.
                    _controls = _wizard.Controls;
                    _originalError = _controls.SetCollectionReadOnly(null);
                }
            }

            void IDisposable.Dispose() {
                if (!_wizard.DesignMode) {
                    _controls.SetCollectionReadOnly(_originalError);
                }
            }
        }

        [SupportsEventValidation]
        private class WizardChildTable : ChildTable {
            private Wizard _owner;

            internal WizardChildTable(Wizard owner) {
                _owner = owner;
            }

            protected override bool OnBubbleEvent(object source, EventArgs args) {
                return _owner.OnBubbleEvent(source, args);
            }
        }

        private enum WizardTemplateType {
            StartNavigationTemplate = 0,
            StepNavigationTemplate = 1,
            FinishNavigationTemplate = 2,
        }

        private sealed class NavigationTemplate : ITemplate {

            private Wizard _wizard;
            private WizardTemplateType _templateType;
            private String _button1ID;
            private String _button2ID;
            private String _button3ID;

            private const string _startNextButtonID = "StartNext";
            private const string _stepNextButtonID = "StepNext";
            private const string _stepPreviousButtonID = "StepPrevious";
            private const string _finishPreviousButtonID = "FinishPrevious";
            private const string _finishButtonID = "Finish";
            private const string _cancelButtonID = "Cancel";

            private TableRow _row;

            private IButtonControl[][] _buttons;

            private bool _button1CausesValidation;

            internal static NavigationTemplate GetDefaultStartNavigationTemplate(Wizard wizard) {
                return new NavigationTemplate(wizard, WizardTemplateType.StartNavigationTemplate,
                    true, null, _startNextButtonID, _cancelButtonID);
            }

            internal static NavigationTemplate GetDefaultStepNavigationTemplate(Wizard wizard) {
                return new NavigationTemplate(wizard, WizardTemplateType.StepNavigationTemplate,
                    false, _stepPreviousButtonID, _stepNextButtonID, _cancelButtonID);
            }

            internal static NavigationTemplate GetDefaultFinishNavigationTemplate(Wizard wizard) {
                return new NavigationTemplate(wizard, WizardTemplateType.FinishNavigationTemplate,
                    false, _finishPreviousButtonID, _finishButtonID, _cancelButtonID);
            }

            internal void ResetButtonsVisibility() {
                Debug.Assert(_wizard.DesignMode);

                for (int i = 0; i < 3; i++) {
                    for (int j = 0; j < 3; j++) {
                        Control c = _buttons[i][j] as Control;
                        if (c != null) c.Visible = false;
                    }
                }
            }

            private NavigationTemplate(Wizard wizard, WizardTemplateType templateType, bool button1CausesValidation,
                String label1ID, String label2ID, String label3ID) {

                _wizard = wizard;
                _button1ID = label1ID;
                _button2ID = label2ID;
                _button3ID = label3ID;

                _templateType = templateType;

                _buttons = new IButtonControl[3][];
                _buttons[0] = new IButtonControl[3];
                _buttons[1] = new IButtonControl[3];
                _buttons[2] = new IButtonControl[3];

                _button1CausesValidation = button1CausesValidation;
            }

            void ITemplate.InstantiateIn(Control container) {
                Table table = new WizardDefaultInnerTable();

                // Increase the default space and padding so the layout
                // of the buttons look good. Also, to make custom border
                // visible. VSWhidbey 377069
                table.CellSpacing = 5;
                table.CellPadding = 5;
                container.Controls.Add(table);

                _row = new TableRow();
                table.Rows.Add(_row);

                if (_button1ID != null) {
                    CreateButtonControl(_buttons[0], _button1ID, _button1CausesValidation,
                        MovePreviousCommandName);
                }

                if (_button2ID != null) {
                    CreateButtonControl(_buttons[1], _button2ID, true /* causesValidation */,
                        _templateType == WizardTemplateType.FinishNavigationTemplate ? MoveCompleteCommandName : MoveNextCommandName);
                }

                CreateButtonControl(_buttons[2], _button3ID, false /* causesValidation */, CancelCommandName);
            }

            private void OnPreRender(object source, EventArgs e) {
                ((ImageButton)source).Visible = false;
            }

            private void CreateButtonControl(IButtonControl[] buttons, String id, bool causesValidation, string commandName) {
                LinkButton linkButton = new LinkButton();
                linkButton.CausesValidation = causesValidation;
                linkButton.ID = id + "LinkButton";
                linkButton.Visible = false;
                linkButton.CommandName = commandName;
                linkButton.TabIndex = _wizard.TabIndex;
                _wizard.RegisterCommandEvents(linkButton);
                buttons[0] = linkButton;

                ImageButton imageButton = new ImageButton();
                imageButton.CausesValidation = causesValidation;
                imageButton.ID = id + "ImageButton";
                imageButton.Visible = true;
                imageButton.CommandName = commandName;
                imageButton.TabIndex = _wizard.TabIndex;
                _wizard.RegisterCommandEvents(imageButton);
                imageButton.PreRender += new EventHandler(OnPreRender);
                buttons[1] = imageButton;

                Button button = new Button();
                button.CausesValidation = causesValidation;
                button.ID = id + "Button";
                button.Visible = false;
                button.CommandName = commandName;
                button.TabIndex = _wizard.TabIndex;
                _wizard.RegisterCommandEvents(button);
                buttons[2] = button;

                TableCell tableCell = new TableCell();
                tableCell.HorizontalAlign = HorizontalAlign.Right;
                _row.Cells.Add(tableCell);

                tableCell.Controls.Add(linkButton);
                tableCell.Controls.Add(imageButton);
                tableCell.Controls.Add(button);
            }

            internal IButtonControl FirstButton {
                get {
                    ButtonType buttonType = ButtonType.Button;
                    switch (_templateType) {
                        case WizardTemplateType.StartNavigationTemplate:
                            Debug.Fail("Invalid template/button type");
                            break;

                        case WizardTemplateType.StepNavigationTemplate:
                            buttonType = _wizard.StepPreviousButtonType;
                            break;

                        case WizardTemplateType.FinishNavigationTemplate:
                        default:
                            buttonType = _wizard.FinishPreviousButtonType;
                            break;
                    }

                    return GetButtonBasedOnType(0, buttonType);
                }
            }

            internal IButtonControl SecondButton {
                get {
                    ButtonType buttonType = ButtonType.Button;
                    switch (_templateType) {
                        case WizardTemplateType.StartNavigationTemplate:
                            buttonType = _wizard.StartNextButtonType;
                            break;

                        case WizardTemplateType.StepNavigationTemplate:
                            buttonType = _wizard.StepNextButtonType;
                            break;

                        case WizardTemplateType.FinishNavigationTemplate:
                        default:
                            buttonType = _wizard.FinishCompleteButtonType;
                            break;
                    }

                    return GetButtonBasedOnType(1, buttonType);
                }
            }

            internal IButtonControl CancelButton {
                get {
                    ButtonType buttonType = _wizard.CancelButtonType;
                    return GetButtonBasedOnType(2, buttonType);
                }
            }

            private IButtonControl GetButtonBasedOnType(int pos, ButtonType type) {
                switch (type) {
                    case ButtonType.Button:
                        return _buttons[pos][2];

                    case ButtonType.Image:
                        return _buttons[pos][1];

                    case ButtonType.Link:
                        return _buttons[pos][0];
                }

                return null;
            }
        }

        private class DataListItemTemplate : ITemplate {
            Wizard _owner;

            internal DataListItemTemplate(Wizard owner) {
                _owner = owner;
            }

            public void InstantiateIn(Control container) {
                LinkButton linkButton = new LinkButton();
                container.Controls.Add(linkButton);
                linkButton.ID = SideBarButtonID;

                if (_owner.DesignMode) {
                    linkButton.MergeStyle(_owner.SideBarButtonStyle);
                }
            }
        }

        private class DefaultSideBarTemplate : ITemplate {
            private Wizard _owner;

            internal DefaultSideBarTemplate(Wizard owner) {
                _owner = owner;
            }

            public void InstantiateIn(Control container) {
                Control sideBarListControl = null;

                if (_owner.SideBarList == null) {
                    var dataList = new DataList();
                    dataList.ID = Wizard.DataListID;

                    dataList.SelectedItemStyle.Font.Bold = true;
                    dataList.ItemTemplate = _owner.CreateDefaultDataListItemTemplate();

                    sideBarListControl = dataList;
                } else {
                    sideBarListControl = (Control)_owner.SideBarList;
                }

                container.Controls.Add(sideBarListControl);
            }
        }

        // Internally use an empty table (with a row and a cell) to render the content.
        internal abstract class BlockControl : WebControl, INamingContainer, INonBindingContainer {
            private Table _table;
            internal TableCell _cell;
            internal Wizard _owner;

            internal BlockControl(Wizard owner) {
                Debug.Assert(owner != null);
                _owner = owner;

                _table = new WizardDefaultInnerTable();
                _table.EnableTheming = false;

                Controls.Add(_table);

                TableRow row = new TableRow();
                _table.Controls.Add(row);

                _cell = new TableCell();
                _cell.Height = Unit.Percentage(100);
                _cell.Width = Unit.Percentage(100);
                row.Controls.Add(_cell);

                HandleMacIECellHeight();
                PreventAutoID();
            }

            protected Table Table {
                get { return _table; }
            }

            internal TableCell InnerCell {
                get { return _cell; }
            }

            protected override Style CreateControlStyle() {
                return new TableItemStyle(ViewState);
            }

            public override void Focus() {
                throw new NotSupportedException(SR.GetString(SR.NoFocusSupport, this.GetType().Name));
            }

            internal void HandleMacIECellHeight() {
                // On Mac IE, render height:1px of the inner table cell, so the table sizes to contents.
                // Otherwise, Mac IE may give the table an arbitrary height (equal to the width of its contents).
                if (!_owner.DesignMode && _owner.IsMacIE5) {
                    _cell.Height = Unit.Pixel(1);
                }
            }

            // Renders the inner control only
            protected internal override void Render(HtmlTextWriter writer) {
                RenderContents(writer);
            }

            internal void SetEnableTheming() {
                _cell.EnableTheming = _owner.EnableTheming;
            }
        }

        private class InternalTableCell : TableCell, INamingContainer, INonBindingContainer {
            protected Wizard _owner;

            internal InternalTableCell(Wizard owner) {
                _owner = owner;
            }

            // Do not render any attributes other than Style on this cell.
            protected override void AddAttributesToRender(HtmlTextWriter writer) {
                if (ControlStyleCreated && !ControlStyle.IsEmpty) {
                    // let the style add attributes
                    ControlStyle.AddAttributesToRender(writer, this);
                }
            }
        }

        private class AccessibleTableCell : InternalTableCell {
            internal AccessibleTableCell(Wizard owner)
                : base(owner) {
            }

            protected internal override void RenderChildren(HtmlTextWriter writer) {
                ControlRenderingHelper.WriteSkipLinkStart(writer, RenderingCompatibility, _owner.DesignMode, _owner.SkipLinkText, SpacerImageUrl, _owner.ClientID);

                base.RenderChildren(writer);

                ControlRenderingHelper.WriteSkipLinkEnd(writer, _owner.DesignMode, _owner.SkipLinkText, _owner.ClientID);
            }
        }

        internal class BaseContentTemplateContainer : BlockControl {
            private bool _useInnerTable;
            internal BaseContentTemplateContainer(Wizard owner,  bool useInnerTable)
                : base(owner) {
                _useInnerTable = useInnerTable;
                if (useInnerTable) {
                    // Set the table width to 100% so the table within each
                    // row will have the same width. VSWhidbey 377182
                    Table.Width = Unit.Percentage(100);
                    Table.Height = Unit.Percentage(100);
                }
                else {
                    // remove nested table from Control tree
                    Controls.Clear();
                }
            }

            internal void AddChildControl(Control c) {
                Container.Controls.Add(c);
            }

            internal Control Container {
                get {
                    return _useInnerTable ? InnerCell : (Control)this;
                }
            }
        }

        #region Navigation Template Containers

        internal class BaseNavigationTemplateContainer : WebControl, INamingContainer, INonBindingContainer {
            private IButtonControl _finishButton;
            private IButtonControl _previousButton;
            private IButtonControl _nextButton;
            private IButtonControl _cancelButton;
            private Wizard _owner;

            internal BaseNavigationTemplateContainer(Wizard owner) {
                _owner = owner;
            }

            internal Wizard Owner {
                get {
                    return _owner;
                }
            }

            internal void ApplyButtonStyle(Style finishStyle, Style prevStyle, Style nextStyle, Style cancelStyle) {
                if (FinishButton != null) ApplyButtonStyleInternal(FinishButton, finishStyle);
                if (PreviousButton != null) ApplyButtonStyleInternal(PreviousButton, prevStyle);
                if (NextButton != null) ApplyButtonStyleInternal(NextButton, nextStyle);
                if (CancelButton != null) ApplyButtonStyleInternal(CancelButton, cancelStyle);
            }

            protected void ApplyButtonStyleInternal(IButtonControl control, Style buttonStyle) {
                WebControl webCtrl = control as WebControl;
                if (webCtrl != null) {
                    webCtrl.ApplyStyle(buttonStyle);
                    webCtrl.ControlStyle.MergeWith(Owner.NavigationButtonStyle);
                }
            }

            public override void Focus() {
                throw new NotSupportedException(SR.GetString(SR.NoFocusSupport, this.GetType().Name));
            }

            internal void RegisterButtonCommandEvents() {
                Owner.RegisterCommandEvents(NextButton);
                Owner.RegisterCommandEvents(FinishButton);
                Owner.RegisterCommandEvents(PreviousButton);
                Owner.RegisterCommandEvents(CancelButton);
            }

            internal IButtonControl CancelButton {
                get {
                    if (_cancelButton != null) {
                        return _cancelButton;
                    }

                    _cancelButton = FindControl(Wizard.CancelButtonID) as IButtonControl;
                    return _cancelButton;
                }

                set {
                    _cancelButton = value;
                }
            }

            internal virtual IButtonControl NextButton {
                get {
                    if (_nextButton != null) {
                        return _nextButton;
                    }

                    _nextButton = FindControl(Wizard.StepNextButtonID) as IButtonControl;
                    return _nextButton;
                }

                set {
                    _nextButton = value;
                }
            }

            internal virtual IButtonControl PreviousButton {
                get {
                    if (_previousButton != null) {
                        return _previousButton;
                    }

                    _previousButton = FindControl(Wizard.StepPreviousButtonID) as IButtonControl;
                    return _previousButton;
                }

                set {
                    _previousButton = value;
                }
            }

            internal IButtonControl FinishButton {
                get {
                    if (_finishButton != null) {
                        return _finishButton;
                    }

                    _finishButton = FindControl(Wizard.FinishButtonID) as IButtonControl;
                    return _finishButton;
                }

                set {
                    _finishButton = value;
                }
            }

            internal void SetEnableTheming() {
                this.EnableTheming = _owner.EnableTheming;
            }

            // Renders the inner control only
            protected internal override void Render(HtmlTextWriter writer) {
                RenderContents(writer);
            }
        }

        private class FinishNavigationTemplateContainer : BaseNavigationTemplateContainer {
            private IButtonControl _previousButton;

            internal FinishNavigationTemplateContainer(Wizard owner)
                : base(owner) {
            }

            internal override IButtonControl PreviousButton {
                get {
                    if (_previousButton != null) {
                        return _previousButton;
                    }

                    _previousButton = FindControl(Wizard.FinishPreviousButtonID) as IButtonControl;
                    return _previousButton;
                }

                set {
                    _previousButton = value;
                }
            }
        }

        private class StartNavigationTemplateContainer : BaseNavigationTemplateContainer {
            private IButtonControl _nextButton;

            internal StartNavigationTemplateContainer(Wizard owner)
                : base(owner) {
            }

            internal override IButtonControl NextButton {
                get {
                    if (_nextButton != null) {
                        return _nextButton;
                    }

                    _nextButton = FindControl(Wizard.StartNextButtonID) as IButtonControl;

                    return _nextButton;
                }

                set {
                    _nextButton = value;
                }
            }
        }

        private class StepNavigationTemplateContainer : BaseNavigationTemplateContainer {

            internal StepNavigationTemplateContainer(Wizard owner)
                : base(owner) {
            }
        }

        #endregion
    }

    public sealed class WizardStepCollection : IList {
        private Wizard _wizard;


        internal WizardStepCollection(Wizard wizard) {
            this._wizard = wizard;
            wizard.TemplatedSteps.Clear();
        }


        public int Count {
            get {
                return Views.Count;
            }
        }


        public bool IsReadOnly {
            get {
                return Views.IsReadOnly;
            }
        }


        public bool IsSynchronized {
            get {
                return false;
            }
        }


        public object SyncRoot {
            get {
                return this;
            }
        }

        private ViewCollection Views {
            get {
                return _wizard.MultiView.Views;
            }
        }


        public WizardStepBase this[int index] {
            get {
                return (WizardStepBase)Views[index];
            }
        }


        public void Add(WizardStepBase wizardStep) {
            // 
            if (wizardStep == null) {
                throw new ArgumentNullException("wizardStep");
            }

            wizardStep.PreventAutoID();
            RemoveIfAlreadyExistsInWizard(wizardStep);

            wizardStep.Owner = _wizard;
            Views.Add(wizardStep);
            AddTemplatedWizardStep(wizardStep);

            NotifyWizardStepsChanged();
        }


        public void AddAt(int index, WizardStepBase wizardStep) {
            if (wizardStep == null) {
                throw new ArgumentNullException("wizardStep");
            }

            RemoveIfAlreadyExistsInWizard(wizardStep);

            wizardStep.PreventAutoID();
            wizardStep.Owner = _wizard;
            Views.AddAt(index, wizardStep);
            AddTemplatedWizardStep(wizardStep);

            NotifyWizardStepsChanged();
        }


        private void AddTemplatedWizardStep(WizardStepBase wizardStep) {
            var templatedWizardStep = wizardStep as TemplatedWizardStep;
            if (templatedWizardStep != null) {
                _wizard.TemplatedSteps.Add(templatedWizardStep);
                _wizard.RegisterCustomNavigationContainers(templatedWizardStep);
            }
        }


        public void Clear() {
            Views.Clear();
            _wizard.TemplatedSteps.Clear();

            NotifyWizardStepsChanged();
        }


        public bool Contains(WizardStepBase wizardStep) {
            if (wizardStep == null) {
                throw new ArgumentNullException("wizardStep");
            }

            return Views.Contains(wizardStep);
        }


        public void CopyTo(WizardStepBase[] array, int index) {
            Views.CopyTo(array, index);
        }


        public IEnumerator GetEnumerator() {
            return Views.GetEnumerator();
        }


        public int IndexOf(WizardStepBase wizardStep) {
            if (wizardStep == null) {
                throw new ArgumentNullException("wizardStep");
            }

            return Views.IndexOf(wizardStep);
        }


        public void Insert(int index, WizardStepBase wizardStep) {
            AddAt(index, wizardStep);
        }

        internal void NotifyWizardStepsChanged() {
            _wizard.OnWizardStepsChanged();
        }


        public void Remove(WizardStepBase wizardStep) {
            if (wizardStep == null) {
                throw new ArgumentNullException("wizardStep");
            }

            Views.Remove(wizardStep);
            wizardStep.Owner = null;
            var templatedWizardStep = wizardStep as TemplatedWizardStep;
            if (templatedWizardStep != null) {
                _wizard.TemplatedSteps.Remove(templatedWizardStep);
            }

            NotifyWizardStepsChanged();
        }


        public void RemoveAt(int index) {
            WizardStepBase wizardStep = Views[index] as WizardStepBase;
            if (wizardStep != null) {
                wizardStep.Owner = null;
                var templatedWizardStep = wizardStep as TemplatedWizardStep;
                if (templatedWizardStep != null) {
                    _wizard.TemplatedSteps.Remove(templatedWizardStep);
                }
            }
            Views.RemoveAt(index);

            NotifyWizardStepsChanged();
        }

        private static void RemoveIfAlreadyExistsInWizard(WizardStepBase wizardStep) {
            if (wizardStep.Owner != null) {
                wizardStep.Owner.WizardSteps.Remove(wizardStep);
            }
        }

        private static WizardStepBase GetStepAndVerify(object value) {
            WizardStepBase step = value as WizardStepBase;
            if (step == null)
                throw new ArgumentException(SR.GetString(SR.Wizard_WizardStepOnly));

            return step;
        }

        #region ICollection implementation


        /// <internalonly/>
        void ICollection.CopyTo(Array array, int index) {
            Views.CopyTo(array, index);
        }
        #endregion //ICollection implementation

        #region IList implementation


        /// <internalonly/>
        bool IList.IsFixedSize {
            get {
                return false;
            }
        }


        /// <internalonly/>
        object IList.this[int index] {
            get {
                return Views[index];
            }
            set {
                RemoveAt(index);
                AddAt(index, GetStepAndVerify(value));
            }
        }


        /// <internalonly/>
        int IList.Add(object value) {
            WizardStepBase step = GetStepAndVerify(value);
            step.PreventAutoID();
            Add(step);
            return IndexOf(step);
        }


        /// <internalonly/>
        bool IList.Contains(object value) {
            return Contains(GetStepAndVerify(value));
        }


        /// <internalonly/>
        int IList.IndexOf(object value) {
            return IndexOf(GetStepAndVerify(value));
        }


        /// <internalonly/>
        void IList.Insert(int index, object value) {
            AddAt(index, GetStepAndVerify(value));
        }


        /// <internalonly/>
        void IList.Remove(object value) {
            Remove(GetStepAndVerify(value));
        }
        #endregion // IList implementation
    }

    [SupportsEventValidation]
    internal class WizardDefaultInnerTable : Table {
        internal WizardDefaultInnerTable() {
            PreventAutoID();

            // cell padding and spacing should be 0 since these tables are for internal layout only.
            CellPadding = 0;
            CellSpacing = 0;
        }
    }


    public class WizardNavigationEventArgs : EventArgs {

        private int _currentStepIndex;
        private int _nextStepIndex;
        private bool _cancel;


        public WizardNavigationEventArgs(int currentStepIndex, int nextStepIndex) {
            _currentStepIndex = currentStepIndex;
            _nextStepIndex = nextStepIndex;
        }


        public bool Cancel {
            get {
                return _cancel;
            }
            set {
                _cancel = value;
            }
        }


        public int CurrentStepIndex {
            get {
                return _currentStepIndex;
            }
        }


        public int NextStepIndex {
            get {
                return _nextStepIndex;
            }
        }

        internal void SetNextStepIndex(int nextStepIndex) {
            _nextStepIndex = nextStepIndex;
        }
    }


    public delegate void WizardNavigationEventHandler(object sender, WizardNavigationEventArgs e);
}

