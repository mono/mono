//------------------------------------------------------------------------------
// <copyright file="WebPartZoneBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web.Security;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    /// <devdoc>
    /// Base class for all zone classes that host WebPart controls.  Inherits from Zone, adding client-side
    /// dragging, verbs, and additional styles.  Zones that are database driven should inherit from
    /// this class.
    /// </devdoc>
    [
    Designer("System.Web.UI.Design.WebControls.WebParts.WebPartZoneBaseDesigner, " + AssemblyRef.SystemDesign),
    ]
    public abstract class WebPartZoneBase : WebZone, IPostBackEventHandler, IWebPartMenuUser {

        private static readonly object CreateVerbsEvent = new object();

        internal const string EventArgumentSeparator = ":";
        private const char eventArgumentSeparatorChar = ':';

        private const string dragEventArgument = "Drag";
        private const string partVerbEventArgument = "partverb";
        private const string zoneVerbEventArgument = "zoneverb";
        private const string closeEventArgument = "close";
        private const string connectEventArgument = "connect";
        private const string deleteEventArgument = "delete";
        private const string editEventArgument = "edit";
        private const string minimizeEventArgument = "minimize";
        private const string restoreEventArgument = "restore";

        // PERF: concat the event arg with the separator at compile-time
        private const string partVerbEventArgumentWithSeparator = partVerbEventArgument + EventArgumentSeparator;
        private const string zoneVerbEventArgumentWithSeparator = zoneVerbEventArgument + EventArgumentSeparator;
        private const string connectEventArgumentWithSeparator = connectEventArgument + EventArgumentSeparator;
        private const string editEventArgumentWithSeparator = editEventArgument + EventArgumentSeparator;
        private const string minimizeEventArgumentWithSeparator = minimizeEventArgument + EventArgumentSeparator;
        private const string restoreEventArgumentWithSeparator = restoreEventArgument + EventArgumentSeparator;
        private const string closeEventArgumentWithSeparator = closeEventArgument + EventArgumentSeparator;
        private const string deleteEventArgumentWithSeparator = deleteEventArgument + EventArgumentSeparator;

        // Indexes into the ViewState array
        private const int baseIndex = 0;
        private const int selectedPartChromeStyleIndex = 1;
        private const int closeVerbIndex = 2;
        private const int connectVerbIndex = 3;
        private const int deleteVerbIndex = 4;
        private const int editVerbIndex = 5;
        private const int helpVerbIndex = 6;
        private const int minimizeVerbIndex = 7;
        private const int restoreVerbIndex = 8;
        private const int exportVerbIndex = 9;
        private const int menuPopupStyleIndex = 10;
        private const int menuLabelStyleIndex = 11;
        private const int menuLabelHoverStyleIndex = 12;
        private const int menuCheckImageStyleIndex = 13;
        private const int menuVerbStyleIndex = 14;
        private const int menuVerbHoverStyleIndex = 15;
        private const int controlStyleIndex = 16;
        private const int titleBarVerbStyleIndex = 17;
        private const int viewStateArrayLength = 18;

        private Style _selectedPartChromeStyle;
        private WebPartVerb _closeVerb;
        private WebPartVerb _connectVerb;
        private WebPartVerb _deleteVerb;
        private WebPartVerb _editVerb;
        private WebPartVerb _exportVerb;
        private WebPartVerb _helpVerb;
        private WebPartVerb _minimizeVerb;
        private WebPartVerb _restoreVerb;

        private WebPartVerbCollection _verbs;
        private WebPartMenuStyle _menuPopupStyle;
        private Style _menuLabelStyle;
        private Style _menuLabelHoverStyle;
        private Style _menuCheckImageStyle;
        private Style _menuVerbHoverStyle;
        private Style _menuVerbStyle;
        private Style _titleBarVerbStyle;

        private Color _borderColor;
        private BorderStyle _borderStyle;
        private Unit _borderWidth;

        private WebPartChrome _webPartChrome;
        private WebPartMenu _menu;

        [
        DefaultValue(true),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.WebPartZoneBase_AllowLayoutChange),
        ]
        public virtual bool AllowLayoutChange {
            get {
                object b = ViewState["AllowLayoutChange"];
                return (b != null) ? (bool)b : true;
            }
            set {
                ViewState["AllowLayoutChange"] = value;
            }
        }

        /// <devdoc>
        /// Overridden to change default value.
        /// </devdoc>
        [
        DefaultValue(typeof(Color), "Gray"),
        ]
        public override Color BorderColor {
            get {
                if (ControlStyleCreated == false) {
                    return Color.Gray;
                }
                return base.BorderColor;
            }
            set {
                base.BorderColor = value;
            }
        }

        /// <devdoc>
        /// Overridden to change default value.
        /// </devdoc>
        [
        DefaultValue(BorderStyle.Solid)
        ]
        public override BorderStyle BorderStyle {
            get {
                if (ControlStyleCreated == false) {
                    return BorderStyle.Solid;
                }
                return base.BorderStyle;
            }
            set {
                base.BorderStyle = value;
            }
        }

        /// <devdoc>
        /// Overridden to change default value.
        /// </devdoc>
        [
        DefaultValue(typeof(Unit), "1")
        ]
        public override Unit BorderWidth {
            get {
                if (ControlStyleCreated == false) {
                    return 1;
                }
                return base.BorderWidth;
            }
            set {
                base.BorderWidth = value;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.WebPartZoneBase_CloseVerb),
        ]
        public virtual WebPartVerb CloseVerb {
            get {
                if (_closeVerb == null) {
                    _closeVerb = new WebPartCloseVerb();
                    if (IsTrackingViewState) {
                        ((IStateManager)_closeVerb).TrackViewState();
                    }
                }
                return _closeVerb;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.WebPartZoneBase_ConnectVerb),
        ]
        public virtual WebPartVerb ConnectVerb {
            get {
                if (_connectVerb == null) {
                    _connectVerb = new WebPartConnectVerb();
                    if (IsTrackingViewState) {
                        ((IStateManager)_connectVerb).TrackViewState();
                    }
                }
                return _connectVerb;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.WebPartZoneBase_DeleteVerb),
        ]
        public virtual WebPartVerb DeleteVerb {
            get {
                if (_deleteVerb == null) {
                    _deleteVerb = new WebPartDeleteVerb();
                    if (IsTrackingViewState) {
                        ((IStateManager)_deleteVerb).TrackViewState();
                    }
                }
                return _deleteVerb;
            }
        }

        /// <devdoc>
        /// The string displayed to identify the zone.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual string DisplayTitle {
            get {
                string title = HeaderText;
                if (!String.IsNullOrEmpty(title)) {
                    return title;
                }

                string id = ID;
                if (!String.IsNullOrEmpty(id)) {
                    return id;
                }

                // ID is required to be set by WebPartManager.RegisterZone but this is still a good fallback.
                int index = 1;
                if (WebPartManager != null) {
                    index = WebPartManager.Zones.IndexOf(this) + 1;
                }
                return SR.GetString(SR.WebPartZoneBase_DisplayTitleFallback,
                                                        index.ToString(CultureInfo.CurrentCulture));
            }
        }

        protected internal bool DragDropEnabled {
            get {
                return (!DesignMode &&
                        RenderClientScript &&
                        AllowLayoutChange &&
                        WebPartManager != null &&
                        WebPartManager.DisplayMode.AllowPageDesign);
            }
        }

        [
        DefaultValue(typeof(Color), "Blue"),
        TypeConverterAttribute(typeof(WebColorConverter)),
        WebCategory("Appearance"),
        WebSysDescription(SR.WebPartZoneBase_DragHighlightColor),
        ]
        public virtual Color DragHighlightColor {
            get {
                object o = ViewState["DragHighlightColor"];
                if (o != null) {
                    Color c = (Color)o;
                    if (c.IsEmpty == false) {
                        return c;
                    }
                }
                return Color.Blue;
            }
            set {
                ViewState["DragHighlightColor"] = value;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.WebPartZoneBase_EditVerb),
        ]
        public virtual WebPartVerb EditVerb {
            get {
                if (_editVerb == null) {
                    _editVerb = new WebPartEditVerb();
                    if (IsTrackingViewState) {
                        ((IStateManager)_editVerb).TrackViewState();
                    }
                }
                return _editVerb;
            }
        }

        [
        WebSysDefaultValue(SR.WebPartZoneBase_DefaultEmptyZoneText)
        ]
        public override string EmptyZoneText {
            // Must look at viewstate directly instead of the property in the base class,
            // so we can distinguish between an unset property and a property set to String.Empty.
            get {
                string s = (string)ViewState["EmptyZoneText"];
                return((s == null) ? SR.GetString(SR.WebPartZoneBase_DefaultEmptyZoneText) : s);
            }
            set {
                ViewState["EmptyZoneText"] = value;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.WebPartZoneBase_ExportVerb),
        ]
        public virtual WebPartVerb ExportVerb {
            get {
                if (_exportVerb == null) {
                    _exportVerb = new WebPartExportVerb();
                    if (IsTrackingViewState) {
                        ((IStateManager)_exportVerb).TrackViewState();
                    }
                }

                return _exportVerb;
            }
        }

        protected override bool HasFooter {
            get {
                return false;
            }
        }

        protected override bool HasHeader {
            get {
                bool hasHeader = false;
                if (DesignMode) {
                    hasHeader = true;
                }
                else if (WebPartManager != null) {
                    hasHeader = WebPartManager.DisplayMode.AllowPageDesign;
                }
                return hasHeader;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.WebPartZoneBase_HelpVerb),
        ]
        public virtual WebPartVerb HelpVerb {
            get {
                if (_helpVerb == null) {
                    _helpVerb = new WebPartHelpVerb();
                    if (IsTrackingViewState) {
                        ((IStateManager)_helpVerb).TrackViewState();
                    }
                }

                return _helpVerb;
            }
        }

        internal WebPartMenu Menu {
            get {
                if (_menu == null) {
                    _menu = new WebPartMenu(this);
                }
                return _menu;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.WebPartZoneBase_MenuCheckImageStyle)
        ]
        public Style MenuCheckImageStyle {
            get {
                if (_menuCheckImageStyle == null) {
                    _menuCheckImageStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_menuCheckImageStyle).TrackViewState();
                    }
                }

                return _menuCheckImageStyle;
            }
        }

        /// <devdoc>
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebCategory("Appearance"),
        WebSysDescription(SR.WebPartZoneBase_MenuCheckImageUrl)
        ]
        public virtual string MenuCheckImageUrl {
            get {
                string s = (string)ViewState["MenuCheckImageUrl"];
                return ((s == null) ? String.Empty : s);
            }
            set {
                ViewState["MenuCheckImageUrl"] = value;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.WebPartZoneBase_MenuLabelHoverStyle)
        ]
        public Style MenuLabelHoverStyle {
            get {
                if (_menuLabelHoverStyle == null) {
                    _menuLabelHoverStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_menuLabelHoverStyle).TrackViewState();
                    }
                }

                return _menuLabelHoverStyle;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.WebPartZoneBase_MenuLabelStyle)
        ]
        public Style MenuLabelStyle {
            get {
                if (_menuLabelStyle == null) {
                    _menuLabelStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_menuLabelStyle).TrackViewState();
                    }
                }

                return _menuLabelStyle;
            }
        }

        [
        DefaultValue(""),
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDescription(SR.WebPartZoneBase_MenuLabelText)
        ]
        public virtual string MenuLabelText {
            get {
                string s = (string)ViewState["MenuLabelText"];
                return ((s == null) ? String.Empty : s);
            }
            set {
                ViewState["MenuLabelText"] = value;
            }
        }

        /// <devdoc>
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebCategory("Appearance"),
        WebSysDescription(SR.WebPartZoneBase_MenuPopupImageUrl)
        ]
        public virtual string MenuPopupImageUrl {
            get {
                string s = (string)ViewState["MenuPopupImageUrl"];
                return ((s == null) ? String.Empty : s);
            }
            set {
                ViewState["MenuPopupImageUrl"] = value;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.WebPartZoneBase_MenuPopupStyle)
        ]
        public WebPartMenuStyle MenuPopupStyle {
            get {
                if (_menuPopupStyle == null) {
                    _menuPopupStyle = new WebPartMenuStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_menuPopupStyle).TrackViewState();
                    }
                }

                return _menuPopupStyle;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.WebPartZoneBase_MenuVerbHoverStyle)
        ]
        public Style MenuVerbHoverStyle {
            get {
                if (_menuVerbHoverStyle == null) {
                    _menuVerbHoverStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_menuVerbHoverStyle).TrackViewState();
                    }
                }

                return _menuVerbHoverStyle;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.WebPartZoneBase_MenuVerbStyle)
        ]
        public Style MenuVerbStyle {
            get {
                if (_menuVerbStyle == null) {
                    _menuVerbStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_menuVerbStyle).TrackViewState();
                    }
                }

                return _menuVerbStyle;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.WebPartZoneBase_MinimizeVerb),
        ]
        public virtual WebPartVerb MinimizeVerb {
            get {
                if (_minimizeVerb == null) {
                    _minimizeVerb = new WebPartMinimizeVerb();
                    if (IsTrackingViewState) {
                        ((IStateManager)_minimizeVerb).TrackViewState();
                    }
                }

                return _minimizeVerb;
            }
        }

        /// <devdoc>
        /// The direction in which contained web parts are rendered.
        /// </devdoc>
        [
        DefaultValue(Orientation.Vertical),
        WebCategory("Layout"),
        WebSysDescription(SR.WebPartZoneBase_LayoutOrientation),
        ]
        public virtual Orientation LayoutOrientation {
            get {
                object o = ViewState["LayoutOrientation"];
                return (o != null) ? (Orientation)(int)o : Orientation.Vertical;
            }
            set {
                if ((value < Orientation.Horizontal) || (value > Orientation.Vertical)) {
                    throw new ArgumentOutOfRangeException("value");
                }

                ViewState["LayoutOrientation"] = (int)value;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Verbs"),
        WebSysDescription(SR.WebPartZoneBase_RestoreVerb),
        ]
        public virtual WebPartVerb RestoreVerb {
            get {
                if (_restoreVerb == null) {
                    _restoreVerb = new WebPartRestoreVerb();
                    if (IsTrackingViewState) {
                        ((IStateManager)_restoreVerb).TrackViewState();
                    }
                }

                return _restoreVerb;
            }
        }

        [
        DefaultValue(null),
        NotifyParentProperty(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("WebPart"),
        WebSysDescription(SR.WebPartZoneBase_SelectedPartChromeStyle),
        ]
        public Style SelectedPartChromeStyle {
            get {
                if (_selectedPartChromeStyle == null) {
                    _selectedPartChromeStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_selectedPartChromeStyle).TrackViewState();
                    }
                }

                return _selectedPartChromeStyle;
            }
        }

        /// <devdoc>
        /// Shows the icon image in the title bar of a part, if the TitleIconImageUrl is specified for a part.
        /// </devdoc>
        [
        DefaultValue(true),
        WebCategory("WebPart"),
        WebSysDescription(SR.WebPartZoneBase_ShowTitleIcons),
        ]
        public virtual bool ShowTitleIcons {
            get {
                object b = ViewState["ShowTitleIcons"];
                return (b != null) ? (bool)b : true;
            }
            set {
                ViewState["ShowTitleIcons"] = value;
            }
        }

        [
        DefaultValue(ButtonType.Image),
        WebCategory("Appearance"),
        WebSysDescription(SR.WebPartZoneBase_TitleBarVerbButtonType),
        ]
        public virtual ButtonType TitleBarVerbButtonType {
            get {
                object obj = ViewState["TitleBarVerbButtonType"];
                return (obj == null) ? ButtonType.Image : (ButtonType)obj;
            }
            set {
                if (value < ButtonType.Button || value > ButtonType.Link) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["TitleBarVerbButtonType"] = value;
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.WebPartZoneBase_TitleBarVerbStyle)
        ]
        public Style TitleBarVerbStyle {
            get {
                if (_titleBarVerbStyle == null) {
                    _titleBarVerbStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_titleBarVerbStyle).TrackViewState();
                    }
                }

                return _titleBarVerbStyle;
            }
        }

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        Themeable(false)
        ]
        public override ButtonType VerbButtonType {
            get {
                return base.VerbButtonType;
            }
            set {
                base.VerbButtonType = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public WebPartChrome WebPartChrome {
            get {
                if (_webPartChrome == null) {
                    _webPartChrome = CreateWebPartChrome();
                }
                return _webPartChrome;
            }
        }

        // 
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public WebPartCollection WebParts {
            get {
                WebPartCollection webParts;
                if (DesignMode) {
                    WebPart[] parts = new WebPart[Controls.Count];
                    Controls.CopyTo(parts, 0);
                    return new WebPartCollection(parts);
                }
                else {
                    if (WebPartManager != null) {
                        webParts = WebPartManager.GetWebPartsForZone(this);
                    }
                    else {
                        webParts = new WebPartCollection();
                    }
                }

                return webParts;
            }
        }

        [
        DefaultValue(WebPartVerbRenderMode.Menu),
        WebCategory("WebPart"),
        WebSysDescription(SR.WebPartZoneBase_WebPartVerbRenderMode)
        ]
        public virtual WebPartVerbRenderMode WebPartVerbRenderMode {
            get {
                object o = ViewState["WebPartVerbRenderMode"];
                return (o != null) ? (WebPartVerbRenderMode)(int)o : WebPartVerbRenderMode.Menu;
            }
            set {
                if ((value < WebPartVerbRenderMode.Menu) || (value > WebPartVerbRenderMode.TitleBar)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["WebPartVerbRenderMode"] = (int)value;
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartZoneBase_CreateVerbs)
        ]
        public event WebPartVerbsEventHandler CreateVerbs {
            add {
                Events.AddHandler(CreateVerbsEvent, value);
            }
            remove {
                Events.RemoveHandler(CreateVerbsEvent, value);
            }
        }

        protected virtual void CloseWebPart(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            if (WebPartManager != null && webPart.AllowClose && AllowLayoutChange) {
                WebPartManager.CloseWebPart(webPart);
            }
        }

        protected virtual void ConnectWebPart(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            if (WebPartManager != null &&
                WebPartManager.DisplayMode == WebPartManager.ConnectDisplayMode &&
                webPart != WebPartManager.SelectedWebPart &&
                webPart.AllowConnect) {
                WebPartManager.BeginWebPartConnecting(webPart);
            }
        }

        // 
        protected internal override void CreateChildControls() {
            if (DesignMode) {
                Controls.Clear();

                WebPartCollection webParts = GetInitialWebParts();
                foreach (WebPart part in webParts) {
                    Controls.Add(part);
                }
            }
        }

        /// <internalonly/>
        protected override ControlCollection CreateControlCollection() {
            if (DesignMode) {
                return new ControlCollection(this);
            }
            else {
                return new EmptyControlCollection(this);
            }
        }

        protected override Style CreateControlStyle() {
            // We need the ControlStyle to use its own StateBag, since we do not want the
            // default values we set here to be saved in ViewState.
            Style style = new Style();

            style.BorderColor = Color.Gray;
            style.BorderStyle = BorderStyle.Solid;
            style.BorderWidth = 1;

            return style;
        }

        /// <devdoc>
        /// Overridden by subclasses to use a different chrome when rendering the WebParts.
        /// </devdoc>
        protected virtual WebPartChrome CreateWebPartChrome() {
            return new WebPartChrome(this, WebPartManager);
        }

        protected virtual void DeleteWebPart(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            if (WebPartManager != null && AllowLayoutChange) {
                WebPartManager.DeleteWebPart(webPart);
            }
        }

        protected virtual void EditWebPart(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            if (WebPartManager != null &&
                WebPartManager.DisplayMode == WebPartManager.EditDisplayMode &&
                webPart != WebPartManager.SelectedWebPart) {
                WebPartManager.BeginWebPartEditing(webPart);
            }
        }

        /// <devdoc>
        /// The effective frame type of a part, taking into consideration the PartChromeType
        /// of the zone and the DisplayMode of the page.
        /// </devdoc>
        public override PartChromeType GetEffectiveChromeType(Part part) {
            PartChromeType chromeType = base.GetEffectiveChromeType(part);

            // Add title to chromeType if we are in page design mode.  We always want
            // to render a title bar when in page design mode. (VSWhidbey 77730)
            if ((WebPartManager != null) && (WebPartManager.DisplayMode.AllowPageDesign)) {
                if (chromeType == PartChromeType.None) {
                    chromeType = PartChromeType.TitleOnly;
                }
                else if (chromeType == PartChromeType.BorderOnly) {
                    chromeType = PartChromeType.TitleAndBorder;
                }
            }

            return chromeType;
        }

        /// <devdoc>
        /// Loads the initial web parts from a template, persistence medium, or some other way.
        /// These parts may be in a different zone when rendered, since control personalization
        /// may change their zone.
        /// </devdoc>
        protected internal abstract WebPartCollection GetInitialWebParts();

        protected override void LoadViewState(object savedState) {
            if (savedState == null) {
                base.LoadViewState(null);
            }
            else {
                object[] myState = (object[]) savedState;
                if (myState.Length != viewStateArrayLength) {
                    throw new ArgumentException(SR.GetString(SR.ViewState_InvalidViewState));
                }

                base.LoadViewState(myState[baseIndex]);
                if (myState[selectedPartChromeStyleIndex] != null) {
                    ((IStateManager) SelectedPartChromeStyle).LoadViewState(myState[selectedPartChromeStyleIndex]);
                }
                if (myState[closeVerbIndex] != null) {
                    ((IStateManager) CloseVerb).LoadViewState(myState[closeVerbIndex]);
                }
                if (myState[connectVerbIndex] != null) {
                    ((IStateManager) ConnectVerb).LoadViewState(myState[connectVerbIndex]);
                }
                if (myState[deleteVerbIndex] != null) {
                    ((IStateManager) DeleteVerb).LoadViewState(myState[deleteVerbIndex]);
                }
                if (myState[editVerbIndex] != null) {
                    ((IStateManager) EditVerb).LoadViewState(myState[editVerbIndex]);
                }
                if (myState[helpVerbIndex] != null) {
                    ((IStateManager) HelpVerb).LoadViewState(myState[helpVerbIndex]);
                }
                if (myState[minimizeVerbIndex] != null) {
                    ((IStateManager) MinimizeVerb).LoadViewState(myState[minimizeVerbIndex]);
                }
                if (myState[restoreVerbIndex] != null) {
                    ((IStateManager) RestoreVerb).LoadViewState(myState[restoreVerbIndex]);
                }
                if (myState[exportVerbIndex] != null) {
                    ((IStateManager) ExportVerb).LoadViewState(myState[exportVerbIndex]);
                }
                if (myState[menuPopupStyleIndex] != null) {
                    ((IStateManager) MenuPopupStyle).LoadViewState(myState[menuPopupStyleIndex]);
                }
                if (myState[menuLabelStyleIndex] != null) {
                    ((IStateManager) MenuLabelStyle).LoadViewState(myState[menuLabelStyleIndex]);
                }
                if (myState[menuLabelHoverStyleIndex] != null) {
                    ((IStateManager) MenuLabelHoverStyle).LoadViewState(myState[menuLabelHoverStyleIndex]);
                }
                if (myState[menuCheckImageStyleIndex] != null) {
                    ((IStateManager) MenuCheckImageStyle).LoadViewState(myState[menuCheckImageStyleIndex]);
                }
                if (myState[menuVerbStyleIndex] != null) {
                    ((IStateManager) MenuVerbStyle).LoadViewState(myState[menuVerbStyleIndex]);
                }
                if (myState[menuVerbHoverStyleIndex] != null) {
                    ((IStateManager) MenuVerbHoverStyle).LoadViewState(myState[menuVerbHoverStyleIndex]);
                }
                if (myState[controlStyleIndex] != null) {
                    ((IStateManager) ControlStyle).LoadViewState(myState[controlStyleIndex]);
                }
                if (myState[titleBarVerbStyleIndex] != null) {
                    ((IStateManager) TitleBarVerbStyle).LoadViewState(myState[titleBarVerbStyleIndex]);
                }
            }
        }

        /// <devdoc>
        /// Load the verbs defined by the page developer or zone subclass.
        /// </devdoc>
        private void CreateZoneVerbs() {
            WebPartVerbsEventArgs args = new WebPartVerbsEventArgs();
            OnCreateVerbs(args);
            _verbs = args.Verbs;
        }

        private bool IsDefaultVerbEvent(string[] eventArguments) {
            return (eventArguments.Length == 2);
        }

        private bool IsDragEvent(string[] eventArguments) {
            return (eventArguments.Length == 3 &&
                    String.Equals(eventArguments[0], dragEventArgument, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsPartVerbEvent(string[] eventArguments) {
            return (eventArguments.Length == 3 &&
                    String.Equals(eventArguments[0], partVerbEventArgument, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsZoneVerbEvent(string[] eventArguments) {
            return (eventArguments.Length == 3 &&
                    String.Equals(eventArguments[0], zoneVerbEventArgument, StringComparison.OrdinalIgnoreCase));
        }

        protected virtual void MinimizeWebPart(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            if (webPart.ChromeState == PartChromeState.Normal &&
                webPart.AllowMinimize &&
                AllowLayoutChange) {
                webPart.ChromeState = PartChromeState.Minimized;
            }
        }

        protected virtual void OnCreateVerbs(WebPartVerbsEventArgs e) {
            WebPartVerbsEventHandler handler = (WebPartVerbsEventHandler)Events[CreateVerbsEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // The zone verbs may have been loaded in RaisePostBackEvent, but we must load
            // them again in case the page developer wants to change the verbs at this time.
            CreateZoneVerbs();

            WebPartChrome.PerformPreRender();

            // 

        }

        protected virtual void RaisePostBackEvent(string eventArgument) {
            if (String.IsNullOrEmpty(eventArgument)) {
                return;
            }

            string[] eventArguments = eventArgument.Split(eventArgumentSeparatorChar);

            // We do not register all the possible combinations of drag/drop events because there are
            // too many combinations.  So we must not validate them either.  VSWhidbey 492706
            if (!IsDragEvent(eventArguments)) {
                ValidateEvent(UniqueID, eventArgument);
            }

            if (WebPartManager == null) {
                return;
            }

            // Look in collection of all WebParts instead of WebParts for this zone, since
            // an admin may have moved the part to a different Zone between postbacks.
            WebPartCollection allWebParts = WebPartManager.WebParts;
            if (IsDefaultVerbEvent(eventArguments)) {
                // Postback from a default verb
                string verbEventArgument = eventArguments[0];
                string partID = eventArguments[1];
                WebPart part = allWebParts[partID];

                // Part will be null or closed if the part was present on the previous request,
                // but is missing or closed now.  It may have been deleted or closed by the admin
                // or filtered by roles.
                if (part != null && !part.IsClosed) {
                    if (String.Equals(verbEventArgument, closeEventArgument, StringComparison.OrdinalIgnoreCase)) {
                        if (CloseVerb.Visible && CloseVerb.Enabled) {
                            CloseWebPart(part);
                        }
                    }
                    else if (String.Equals(verbEventArgument, connectEventArgument, StringComparison.OrdinalIgnoreCase)) {
                        if (ConnectVerb.Visible && ConnectVerb.Enabled) {
                            ConnectWebPart(part);
                        }
                    }
                    else if (String.Equals(verbEventArgument, deleteEventArgument, StringComparison.OrdinalIgnoreCase)) {
                        if (DeleteVerb.Visible && DeleteVerb.Enabled) {
                            DeleteWebPart(part);
                        }
                    }
                    else if (String.Equals(verbEventArgument, editEventArgument, StringComparison.OrdinalIgnoreCase)) {
                        if (EditVerb.Visible && EditVerb.Enabled) {
                            EditWebPart(part);
                        }
                    }
                    else if (String.Equals(verbEventArgument, minimizeEventArgument, StringComparison.OrdinalIgnoreCase)) {
                        if (MinimizeVerb.Visible && MinimizeVerb.Enabled) {
                            MinimizeWebPart(part);
                        }
                    }
                    else if (String.Equals(verbEventArgument, restoreEventArgument, StringComparison.OrdinalIgnoreCase)) {
                        if (RestoreVerb.Visible && RestoreVerb.Enabled) {
                            RestoreWebPart(part);
                        }
                    }
                }
            }
            else if (IsDragEvent(eventArguments)) {
                // Postback from a drop event

                // The eventArgument contains the WholePartID instead of just ID, since we
                // render WholePartID on the table containing the whole part.
                string wholePartID = eventArguments[1];

                string partID = null;
                if (wholePartID.StartsWith(WebPart.WholePartIDPrefix, StringComparison.Ordinal)) {
                    partID = wholePartID.Substring(WebPart.WholePartIDPrefix.Length);
                }

                int dropPartIndex = Int32.Parse(eventArguments[2], CultureInfo.InvariantCulture);
                WebPart actionPart = allWebParts[partID];

                // Part will be null or closed if the part was present on the previous request,
                // but is missing or closed now.  It may have been deleted or closed by the admin
                // or filtered by roles.
                if (actionPart != null && !actionPart.IsClosed) {
                    // If dragged part to larger index in its current zone, correct drop index
                    // by subtracting 1.  Otherwise the part will move 1 position farther than desired.
                    if (WebParts.Contains(actionPart) && (actionPart.ZoneIndex < dropPartIndex)) {
                        dropPartIndex--;
                    }

                    WebPartZoneBase fromZone = actionPart.Zone;
                    if (AllowLayoutChange &&
                        WebPartManager.DisplayMode.AllowPageDesign &&
                        fromZone != null &&
                        fromZone.AllowLayoutChange &&
                        (actionPart.AllowZoneChange || (fromZone == this))) {
                        WebPartManager.MoveWebPart(actionPart, this, dropPartIndex);
                    }
                }
            }
            else if (IsPartVerbEvent(eventArguments)) {
                // Postback from a part verb
                string verbID = eventArguments[1];
                string partID = eventArguments[2];
                WebPart part = allWebParts[partID];

                // Part will be null or closed if the part was present on the previous request,
                // but is missing or closed now.  It may have been deleted or closed by the admin
                // or filtered by roles.
                if (part != null && !part.IsClosed) {
                    WebPartVerb verb = part.Verbs[verbID];
                    if (verb != null && verb.Visible && verb.Enabled) {
                        verb.ServerClickHandler(verb, new WebPartEventArgs(part));
                    }
                }
            }
            else if (IsZoneVerbEvent(eventArguments)) {
                // Postback from a zone verb
                CreateZoneVerbs();
                string verbID = eventArguments[1];
                string partID = eventArguments[2];
                WebPart part = allWebParts[partID];

                // Part will be null or closed if the part was present on the previous request,
                // but is missing or closed now.  It may have been deleted or closed by the admin
                // or filtered by roles.
                if (part != null && !part.IsClosed) {
                    WebPartVerb verb = _verbs[verbID];
                    if (verb != null && verb.Visible && verb.Enabled) {
                        verb.ServerClickHandler(verb, new WebPartEventArgs(part));
                    }
                }
            }
        }

        protected internal override void Render(HtmlTextWriter writer) {
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            _borderColor = BorderColor;
            _borderStyle = BorderStyle;
            _borderWidth = BorderWidth;

            // PERF: If the control style has not been created, we don't need to set these values,
            // since no style properties will be rendered
            if (ControlStyleCreated) {
                BorderColor = Color.Empty;
                BorderStyle = BorderStyle.NotSet;
                BorderWidth = Unit.Empty;
            }

            base.Render(writer);

            if (ControlStyleCreated) {
                BorderColor = _borderColor;
                BorderStyle = _borderStyle;
                BorderWidth = _borderWidth;
            }
        }

        protected override void RenderBody(HtmlTextWriter writer) {
            Orientation orientation = LayoutOrientation;

            if (DesignMode || (WebPartManager != null && (WebPartManager.DisplayMode.AllowPageDesign))) {
                if (_borderColor != Color.Empty || _borderStyle != BorderStyle.NotSet || _borderWidth != Unit.Empty) {
                    Style s = new Style();
                    s.BorderColor = _borderColor;
                    s.BorderStyle = _borderStyle;
                    s.BorderWidth = _borderWidth;
                    s.AddAttributesToRender(writer, this);
                }
            }

            RenderBodyTableBeginTag(writer);
            if (DesignMode) {
                RenderDesignerRegionBeginTag(writer, orientation);
            }

            if (orientation == Orientation.Horizontal) {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            }

            bool dragDropEnabled = DragDropEnabled;
            if (dragDropEnabled) {
                RenderDropCue(writer);
            }

            WebPartCollection webParts = WebParts;
            if (webParts == null || webParts.Count == 0) {
                RenderEmptyZoneBody(writer);
            }
            else {
                WebPartChrome chrome = WebPartChrome;
                foreach (WebPart webPart in webParts) {
                    // Don't render anything visible for a  minimized part if its effective frame
                    // type dictates that a title bar will not be rendered. (VSWhidbey 77730)
                    if (webPart.ChromeState == PartChromeState.Minimized) {
                        PartChromeType chromeType = GetEffectiveChromeType(webPart);
                        if (chromeType == PartChromeType.None || chromeType == PartChromeType.BorderOnly) {
                            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
                        }
                    }


                    if (orientation == Orientation.Vertical) {
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    }
                    else {
                        // Mac IE needs height=100% set on <td> instead of <tr>
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
                        writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);

                    chrome.RenderWebPart(writer, webPart);

                    writer.RenderEndTag();      // Td
                    if (orientation == Orientation.Vertical) {
                        writer.RenderEndTag();  // Tr
                    }

                    if (dragDropEnabled) {
                        RenderDropCue(writer);
                    }
                }

                if (orientation == Orientation.Vertical) {
                    // Add an extra row with height of 100%, to Microsoft up any extra space
                    // if the height of the zone is larger than its contents
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    // Mozilla renders padding on an empty TD without this attribute
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "0");

                    // Mac IE needs height=100% set on <td> instead of <tr>
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.RenderEndTag(); // Td
                    writer.RenderEndTag(); // Tr
                }
                else {
                    // Add an extra cell with width of 100%, to Microsoft up any extra space
                    // if the width of the zone is larger than its contents.
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");

                    // Mozilla renders padding on an empty TD without this attribute
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "0");

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.RenderEndTag(); // Td
                }
            }

            if (orientation == Orientation.Horizontal) {
                writer.RenderEndTag();  // Tr
            }

            if (DesignMode) {
                RenderDesignerRegionEndTag(writer);
            }
            RenderBodyTableEndTag(writer);
        }

        protected virtual void RenderDropCue(HtmlTextWriter writer) {
            if (LayoutOrientation == Orientation.Vertical) {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingTop, "1");
                writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingBottom, "1");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                RenderDropCueIBar(writer, Orientation.Horizontal);
                writer.RenderEndTag();  // Td
                writer.RenderEndTag();  // Tr
            }
            else {
                writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingLeft, "1");
                writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingRight, "1");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                RenderDropCueIBar(writer, Orientation.Vertical);
                writer.RenderEndTag();  // Td
            }
        }

        private void RenderDropCueIBar(HtmlTextWriter writer, Orientation orientation) {
            // 10px is the total margin and border width that we have to substract
            // (2*2px for the margin, 2*3px for the border)
            // Places to touch if we want to change the rendering of the cues:
            // WebParts.js (Zone_ToggleDropCues)
            // WebPartZoneBase.RenderDropCueIBar
            string color = ColorTranslator.ToHtml(DragHighlightColor);
            string border = "solid 3px " + color;

            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            if (orientation == Orientation.Horizontal) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
                writer.AddStyleAttribute("border-left", border);
                writer.AddStyleAttribute("border-right", border);
            }
            else {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
                writer.AddStyleAttribute("border-top", border);
                writer.AddStyleAttribute("border-bottom", border);
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.Visibility, "hidden");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            if (orientation == Orientation.Vertical) {
                writer.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "0px");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            if (orientation == Orientation.Horizontal) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "2px 0px 2px 0px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "2px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            }
            else {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "0px 2px 0px 2px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "2px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, color);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.RenderEndTag();      // Div
            writer.RenderEndTag();      // Td
            writer.RenderEndTag();      // Tr
            writer.RenderEndTag();      // Table
        }

        private void RenderEmptyZoneBody(HtmlTextWriter writer) {
            bool vertical = (LayoutOrientation == Orientation.Vertical);
            bool horizontal = !vertical;
            string emptyZoneText = EmptyZoneText;

            bool renderText = (!DesignMode && AllowLayoutChange && WebPartManager != null &&
                               WebPartManager.DisplayMode.AllowPageDesign && !String.IsNullOrEmpty(emptyZoneText));

            if (vertical) {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            }

            if (renderText) {
                writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
            }

            if (horizontal) {
                // Want drop zone to shrink to size, so take up all width in zone
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            }
            else {
                // Want drop zone to shrink to size, so take up all height in zone
                // Mac IE needs height=100% set on <td> instead of <tr>
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            if (renderText) {
                Style emptyZoneTextStyle = EmptyZoneTextStyle;
                if (!emptyZoneTextStyle.IsEmpty) {
                    emptyZoneTextStyle.AddAttributesToRender(writer, this);
                }

                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(emptyZoneText);
                writer.RenderEndTag();  // Div
            }

            writer.RenderEndTag();  // Td
            if (vertical) {
                writer.RenderEndTag();  // Tr
            }

            if (renderText) {
                if (DragDropEnabled) {
                    // This drop cue will never be activated since there are no web parts, but it
                    // reserves space below the text equal to the real drop cue above the text
                    RenderDropCue(writer);
                }
            }
        }

        protected override void RenderHeader(HtmlTextWriter writer) {

            // 




            // Render title bar
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");

            TitleStyle headerStyle = HeaderStyle;
            if (!headerStyle.IsEmpty) {
                // Apply font and forecolor from HeaderStyle to inner table
                Style style = new Style();
                if (!headerStyle.ForeColor.IsEmpty) {
                    style.ForeColor = headerStyle.ForeColor;
                }
                style.Font.CopyFrom(headerStyle.Font);
                if (!headerStyle.Font.Size.IsEmpty) {
                    // If the font size is specified on the HeaderStyle, force the font size to 100%,
                    // so it inherits the font size from its parent in IE compatibility mode. I would
                    // think that "1em" would work here as well, but "1em" doesn't work when you change
                    // the font size in the browser.
                    style.Font.Size = new FontUnit(new Unit(100, UnitType.Percentage));
                }
                if (!style.IsEmpty) {
                    style.AddAttributesToRender(writer, this);
                }
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // Copied from Panel.cs
            // 
            HorizontalAlign hAlign = headerStyle.HorizontalAlign;
            if (hAlign != HorizontalAlign.NotSet) {
                TypeConverter hac = TypeDescriptor.GetConverter(typeof(HorizontalAlign));
                writer.AddAttribute(HtmlTextWriterAttribute.Align, hac.ConvertToString(hAlign));
            }

            writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(DisplayTitle);
            writer.RenderEndTag();  // Td

            writer.RenderEndTag();  // Tr
            writer.RenderEndTag();  // Table
        }

        protected virtual void RestoreWebPart(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            if ((webPart.ChromeState == PartChromeState.Minimized) && AllowLayoutChange) {
                webPart.ChromeState = PartChromeState.Normal;
            }
        }

        protected override object SaveViewState() {
            object[] myState = new object[viewStateArrayLength];

            myState[baseIndex] = base.SaveViewState();
            myState[selectedPartChromeStyleIndex] = (_selectedPartChromeStyle != null) ? ((IStateManager)_selectedPartChromeStyle).SaveViewState() : null;
            myState[closeVerbIndex] = (_closeVerb != null) ? ((IStateManager)_closeVerb).SaveViewState() : null;
            myState[connectVerbIndex] = (_connectVerb != null) ? ((IStateManager)_connectVerb).SaveViewState() : null;
            myState[deleteVerbIndex] = (_deleteVerb != null) ? ((IStateManager)_deleteVerb).SaveViewState() : null;
            myState[editVerbIndex] = (_editVerb != null) ? ((IStateManager)_editVerb).SaveViewState() : null;
            myState[helpVerbIndex] = (_helpVerb != null) ? ((IStateManager)_helpVerb).SaveViewState() : null;
            myState[minimizeVerbIndex] = (_minimizeVerb != null) ? ((IStateManager)_minimizeVerb).SaveViewState() : null;
            myState[restoreVerbIndex] = (_restoreVerb != null) ? ((IStateManager)_restoreVerb).SaveViewState() : null;
            myState[exportVerbIndex] = (_exportVerb != null) ? ((IStateManager)_exportVerb).SaveViewState() : null;
            myState[menuPopupStyleIndex] = (_menuPopupStyle != null) ? ((IStateManager)_menuPopupStyle).SaveViewState() : null;
            myState[menuLabelStyleIndex] = (_menuLabelStyle != null) ? ((IStateManager)_menuLabelStyle).SaveViewState() : null;
            myState[menuLabelHoverStyleIndex] = (_menuLabelHoverStyle != null) ? ((IStateManager)_menuLabelHoverStyle).SaveViewState() : null;
            myState[menuCheckImageStyleIndex] = (_menuCheckImageStyle != null) ? ((IStateManager)_menuCheckImageStyle).SaveViewState() : null;
            myState[menuVerbStyleIndex] = (_menuVerbStyle != null) ? ((IStateManager)_menuVerbStyle).SaveViewState() : null;
            myState[menuVerbHoverStyleIndex] = (_menuVerbHoverStyle != null) ? ((IStateManager)_menuVerbHoverStyle).SaveViewState() : null;
            myState[controlStyleIndex] = ControlStyleCreated ? ((IStateManager)ControlStyle).SaveViewState() : null;
            myState[titleBarVerbStyleIndex] = (_titleBarVerbStyle != null) ? ((IStateManager)_titleBarVerbStyle).SaveViewState() : null;

            for (int i=0; i < viewStateArrayLength; i++) {
                if (myState[i] != null) {
                    return myState;
                }
            }

            // More performant to return null than an array of null values
            return null;
        }

        protected override void TrackViewState() {
            base.TrackViewState();

            if (_selectedPartChromeStyle != null) {
                ((IStateManager) _selectedPartChromeStyle).TrackViewState();
            }
            if (_closeVerb != null) {
                ((IStateManager) _closeVerb).TrackViewState();
            }
            if (_connectVerb != null) {
                ((IStateManager) _connectVerb).TrackViewState();
            }
            if (_deleteVerb != null) {
                ((IStateManager) _deleteVerb).TrackViewState();
            }
            if (_editVerb != null) {
                ((IStateManager) _editVerb).TrackViewState();
            }
            if (_helpVerb != null) {
                ((IStateManager) _helpVerb).TrackViewState();
            }
            if (_minimizeVerb != null) {
                ((IStateManager) _minimizeVerb).TrackViewState();
            }
            if (_restoreVerb != null) {
                ((IStateManager) _restoreVerb).TrackViewState();
            }
            if (_exportVerb != null) {
                ((IStateManager) _exportVerb).TrackViewState();
            }
            if (_menuPopupStyle != null) {
                ((IStateManager) _menuPopupStyle).TrackViewState();
            }
            if (_menuLabelStyle != null) {
                ((IStateManager) _menuLabelStyle).TrackViewState();
            }
            if (_menuLabelHoverStyle != null) {
                ((IStateManager) _menuLabelHoverStyle).TrackViewState();
            }
            if (_menuCheckImageStyle != null) {
                ((IStateManager) _menuCheckImageStyle).TrackViewState();
            }
            if (_menuVerbStyle != null) {
                ((IStateManager) _menuVerbStyle).TrackViewState();
            }
            if (_menuVerbHoverStyle != null) {
                ((IStateManager)_menuVerbHoverStyle).TrackViewState();
            }
            if (ControlStyleCreated) {
                ((IStateManager) ControlStyle).TrackViewState();
            }
            if (_titleBarVerbStyle != null) {
                ((IStateManager)_titleBarVerbStyle).TrackViewState();
            }
        }

        // Called from WebPartChrome and DesignerWebPartChrome.  Can't be passed as argument to
        // RenderWebPart, since the WebPartZoneDesigner calls RenderWebPart as well.
        internal WebPartVerbCollection VerbsForWebPart(WebPart webPart) {
            WebPartVerbCollection verbs = new WebPartVerbCollection();

            WebPartVerbCollection partVerbs = webPart.Verbs;
            if (partVerbs != null) {
                foreach (WebPartVerb verb in partVerbs) {
                    if (verb.ServerClickHandler != null) {
                        verb.SetEventArgumentPrefix(partVerbEventArgumentWithSeparator);
                    }
                    verbs.Add(verb);
                }
            }

            if (_verbs != null) {
                foreach (WebPartVerb verb in _verbs) {
                    if (verb.ServerClickHandler != null) {
                        verb.SetEventArgumentPrefix(zoneVerbEventArgumentWithSeparator);
                    }
                    verbs.Add(verb);
                }
            }

            WebPartVerb minimizeVerb = MinimizeVerb;
            minimizeVerb.SetEventArgumentPrefix(minimizeEventArgumentWithSeparator);
            verbs.Add(minimizeVerb);

            WebPartVerb restoreVerb = RestoreVerb;
            restoreVerb.SetEventArgumentPrefix(restoreEventArgumentWithSeparator);
            verbs.Add(restoreVerb);

            WebPartVerb closeVerb = CloseVerb;
            closeVerb.SetEventArgumentPrefix(closeEventArgumentWithSeparator);
            verbs.Add(closeVerb);

            WebPartVerb deleteVerb = DeleteVerb;
            deleteVerb.SetEventArgumentPrefix(deleteEventArgumentWithSeparator);
            verbs.Add(deleteVerb);

            WebPartVerb editVerb = EditVerb;
            editVerb.SetEventArgumentPrefix(editEventArgumentWithSeparator);
            verbs.Add(editVerb);

            WebPartVerb connectVerb = ConnectVerb;
            connectVerb.SetEventArgumentPrefix(connectEventArgumentWithSeparator);
            verbs.Add(connectVerb);

            // Export does not post back
            verbs.Add(ExportVerb);

            // Help verb does not post back
            verbs.Add(HelpVerb);

            return verbs;
        }

        #region Implementation of IPostBackEventHandler
        /// <internalonly/>
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }
        #endregion

        #region Implementation of IWebPartMenuUser
        Style IWebPartMenuUser.CheckImageStyle {
            get {
                return _menuCheckImageStyle;
            }
        }

        string IWebPartMenuUser.CheckImageUrl {
            get {
                string s = MenuCheckImageUrl;
                if (!String.IsNullOrEmpty(s)) {
                    s = ResolveClientUrl(s);
                }
                return s;
            }
        }

        string IWebPartMenuUser.ClientID {
            get {
                return ClientID;
            }
        }

        string IWebPartMenuUser.PopupImageUrl {
            get {
                string s = MenuPopupImageUrl;
                if (!String.IsNullOrEmpty(s)) {
                    s = ResolveClientUrl(s);
                }
                return s;
            }
        }

        Style IWebPartMenuUser.ItemHoverStyle {
            get {
                return _menuVerbHoverStyle;
            }
        }

        Style IWebPartMenuUser.ItemStyle {
            get {
                return _menuVerbStyle;
            }
        }

        Style IWebPartMenuUser.LabelHoverStyle {
            get {
                return _menuLabelHoverStyle;
            }
        }

        string IWebPartMenuUser.LabelImageUrl {
            get {
                return null;
            }
        }

        Style IWebPartMenuUser.LabelStyle {
            get {
                return MenuLabelStyle;
            }
        }

        string IWebPartMenuUser.LabelText {
            get {
                return MenuLabelText;
            }
        }

        WebPartMenuStyle IWebPartMenuUser.MenuPopupStyle {
            get {
                return _menuPopupStyle;
            }
        }

        Page IWebPartMenuUser.Page {
            get {
                return Page;
            }
        }

        string IWebPartMenuUser.PostBackTarget {
            get {
                return UniqueID;
            }
        }

        IUrlResolutionService IWebPartMenuUser.UrlResolver {
            get {
                return this;
            }
        }

        void IWebPartMenuUser.OnBeginRender(HtmlTextWriter writer) {
        }

        void IWebPartMenuUser.OnEndRender(HtmlTextWriter writer) {
        }
        #endregion
    }
}
