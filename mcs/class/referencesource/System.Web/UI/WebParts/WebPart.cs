//------------------------------------------------------------------------------
// <copyright file="WebPart.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    // 





    /// <devdoc>
    /// Adds several features to the Part class, including connections, personalization behavior,
    /// and additional UI properties.
    /// </devdoc>
    [
    Designer("System.Web.UI.Design.WebControls.WebParts.WebPartDesigner, " + AssemblyRef.SystemDesign)
    ]
    public abstract class WebPart : Part, IWebPart, IWebActionable, IWebEditable {

        private WebPartManager _webPartManager;

        private string _zoneID;
        private int _zoneIndex;
        private WebPartZoneBase _zone;

        private bool _allowClose;
        private bool _allowConnect;
        private bool _allowEdit;
        private bool _allowHide;
        private bool _allowMinimize;
        private bool _allowZoneChange;
        private string _authorizationFilter;
        private string _catalogIconImageUrl;
        private PartChromeState _chromeState;
        private string _connectErrorMessage;
        private WebPartExportMode _exportMode;
        private WebPartHelpMode _helpMode;
        private string _helpUrl;
        private bool _hidden;
        private string _importErrorMessage;
        private string _titleIconImageUrl;
        private string _titleUrl;

        private bool _hasUserData;
        private bool _hasSharedData;
        private bool _isClosed;
        private bool _isShared;
        private bool _isStandalone;
        private bool _isStatic;

        // Counter to detect circular connections
        private Dictionary<ProviderConnectionPoint, int> _trackerCounter;

        internal const string WholePartIDPrefix = "WebPart_";
        private const string titleBarIDPrefix = "WebPartTitle_";

        protected WebPart() {
            _allowClose = true;
            _allowConnect = true;
            _allowEdit = true;
            _allowHide = true;
            _allowMinimize = true;
            _allowZoneChange = true;
            _chromeState = PartChromeState.Normal;
            _exportMode = WebPartExportMode.None;
            _helpMode = WebPartHelpMode.Navigate;
            _isStatic = true;
            _isStandalone = true;
        }

        /// <devdoc>
        /// Whether the user is allowed to close the web part
        /// </devdoc>
        [
        DefaultValue(true),
        Personalizable(PersonalizationScope.Shared),
        Themeable(false),
        WebCategory("WebPartBehavior"),
        WebSysDescription(SR.WebPart_AllowClose),
        ]
        public virtual bool AllowClose {
            get {
                return _allowClose;
            }
            set {
                _allowClose = value;
            }
        }

        [
        DefaultValue(true),
        Personalizable(PersonalizationScope.Shared),
        Themeable(false),
        WebCategory("WebPartBehavior"),
        WebSysDescription(SR.WebPart_AllowConnect),
        ]
        public virtual bool AllowConnect {
            get {
                return _allowConnect;
            }
            set {
                _allowConnect = value;
            }
        }

        /// <devdoc>
        /// If false, then LayoutEditorPart is the only visible EditorPart.  Custom EditorParts
        /// may choose to be visible as well.
        /// </devdoc>
        [
        DefaultValue(true),
        Personalizable(PersonalizationScope.Shared),
        Themeable(false),
        WebCategory("WebPartBehavior"),
        WebSysDescription(SR.WebPart_AllowEdit),
        ]
        public virtual bool AllowEdit {
            get {
                return _allowEdit;
            }
            set {
                _allowEdit = value;
            }
        }

        /// <devdoc>
        /// </devdoc>
        [
        DefaultValue(true),
        Personalizable(PersonalizationScope.Shared),
        Themeable(false),
        WebCategory("WebPartBehavior"),
        WebSysDescription(SR.WebPart_AllowHide),
        ]
        public virtual bool AllowHide {
            get {
                return _allowHide;
            }
            set {
                _allowHide = value;
            }
        }

        /// <devdoc>
        /// Whether the user is allowed to minimize the web part
        /// </devdoc>
        [
        DefaultValue(true),
        Personalizable(PersonalizationScope.Shared),
        Themeable(false),
        WebCategory("WebPartBehavior"),
        WebSysDescription(SR.WebPart_AllowMinimize),
        ]
        public virtual bool AllowMinimize {
            get {
                return _allowMinimize;
            }
            set {
                _allowMinimize = value;
            }
        }

        /// <devdoc>
        /// Whether the user is allowed move the web part around the page
        /// </devdoc>
        [
        DefaultValue(true),
        Personalizable(PersonalizationScope.Shared),
        Themeable(false),
        WebCategory("WebPartBehavior"),
        WebSysDescription(SR.WebPart_AllowZoneChange),
        ]
        public virtual bool AllowZoneChange {
            get {
                return _allowZoneChange;
            }
            set {
                _allowZoneChange = value;
            }
        }

        [
        DefaultValue(""),
        Personalizable(PersonalizationScope.Shared),
        Themeable(false),
        WebCategory("WebPartBehavior"),
        WebSysDescription(SR.WebPart_AuthorizationFilter),
        ]
        public virtual string AuthorizationFilter {
            get {
                return (_authorizationFilter != null) ? _authorizationFilter : String.Empty;
            }
            set {
                _authorizationFilter = value;
            }
        }

        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebCategory("WebPartAppearance"),
        Personalizable(PersonalizationScope.Shared),
        WebSysDescription(SR.WebPart_CatalogIconImageUrl),
        ]
        public virtual string CatalogIconImageUrl {
            get {
                return (_catalogIconImageUrl != null) ? _catalogIconImageUrl : String.Empty;
            }
            set {
                if (CrossSiteScriptingValidation.IsDangerousUrl(value)) {
                    throw new ArgumentException(SR.GetString(SR.WebPart_BadUrl, value), "value");
                }
                _catalogIconImageUrl = value;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// Overriden to mark as personalizable
        /// </devdoc>
        // PERF: Use a field instead of calling base.ChromeState, since the base implementation uses
        // viewstate.
        [
        Personalizable
        ]
        public override PartChromeState ChromeState {
            get {
                return _chromeState;
            }
            set {
                if ((value < PartChromeState.Normal) || (value > PartChromeState.Minimized)) {
                    throw new ArgumentOutOfRangeException("value");
                }

                _chromeState = value;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// Overriden to mark as personalizable
        /// </devdoc>
        [
        Personalizable
        ]
        public override PartChromeType ChromeType {
            get {
                return base.ChromeType;
            }
            set {
                base.ChromeType = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string ConnectErrorMessage {
            get {
                return (_connectErrorMessage != null) ? _connectErrorMessage : String.Empty;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// Overriden to mark as personalizable
        /// </devdoc>
        [
        Personalizable(PersonalizationScope.Shared),
        ]
        public override string Description {
            get {
                return base.Description;
            }
            set {
                base.Description = value;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// Overriden to mark as personalizable
        /// </devdoc>
        [
        Personalizable
        ]
        public override ContentDirection Direction {
            get {
                return base.Direction;
            }
            set {
                base.Direction = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string DisplayTitle {
            get {
                if (_webPartManager != null) {
                    return _webPartManager.GetDisplayTitle(this);
                }
                else {
                    // Needed for a WebPart in a DeclarativeCatalogPart, or any case where WebPartManager
                    // has not been set.
                    string displayTitle = Title;
                    if (String.IsNullOrEmpty(displayTitle)) {
                        displayTitle = SR.GetString(SR.Part_Untitled);
                    }
                    return displayTitle;
                }
            }
        }

        [
        DefaultValue(WebPartExportMode.None),
        Personalizable(PersonalizationScope.Shared),
        Themeable(false),
        WebCategory("WebPartBehavior"),
        WebSysDescription(SR.WebPart_ExportMode),
        ]
        public virtual WebPartExportMode ExportMode {
            get {
                return _exportMode;
            }
            set {
                if (ControlState >= ControlState.Loaded &&
                    (WebPartManager == null ||
                     (WebPartManager.Personalization.Scope == PersonalizationScope.User && IsShared))) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPart_CantSetExportMode));
                }
                if (value < WebPartExportMode.None || value > WebPartExportMode.NonSensitiveData) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _exportMode = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool HasUserData {
            get {
                return _hasUserData;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool HasSharedData {
            get {
                return _hasSharedData;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// Overriden to mark as personalizable
        /// </devdoc>
        [
        Personalizable
        ]
        public override Unit Height {
            get {
                return base.Height;
            }
            set {
                base.Height = value;
            }
        }

        /// <devdoc>
        /// The type of help UI used to display the help topic
        /// </devdoc>
        [
        DefaultValue(WebPartHelpMode.Navigate),
        Personalizable(PersonalizationScope.Shared),
        Themeable(false),
        WebCategory("WebPartBehavior"),
        WebSysDescription(SR.WebPart_HelpMode),
        ]
        public virtual WebPartHelpMode HelpMode {
            get {
                return _helpMode;
            }
            set {
                if ((value < WebPartHelpMode.Modal) || (value > WebPartHelpMode.Navigate)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _helpMode = value;
            }
        }

        /// <devdoc>
        /// The URL of the web part's associated help topic
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        Personalizable(PersonalizationScope.Shared),
        Themeable(false),
        WebCategory("WebPartBehavior"),
        WebSysDescription(SR.WebPart_HelpUrl),
        ]
        public virtual string HelpUrl {
            get {
                return (_helpUrl != null) ? _helpUrl : String.Empty;
            }
            set {
                if (CrossSiteScriptingValidation.IsDangerousUrl(value)) {
                    throw new ArgumentException(SR.GetString(SR.WebPart_BadUrl, value), "value");
                }
                _helpUrl = value;
            }
        }

        /// <devdoc>
        /// Whether the web part is to be visually displayed or not.
        /// A web part with Hidden set to true still participates in various
        /// page lifecycle phases such as PreRender.
        /// </devdoc>
        [
        DefaultValue(false),
        Personalizable,
        Themeable(false),
        WebCategory("WebPartAppearance"),
        WebSysDescription(SR.WebPart_Hidden),
        ]
        public virtual bool Hidden {
            get {
                return _hidden;
            }
            set {
                _hidden = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool IsClosed {
            get {
                return _isClosed;
            }
        }

        /// <devdoc>
        /// An "orphaned" part has no Zone, but has not been moved to the page catalog yet.
        /// </devdoc>
        internal bool IsOrphaned {
            get {
                return (Zone == null && !IsClosed);
            }
        }

        [
        Localizable(true),
        WebCategory("WebPartAppearance"),
        WebSysDefaultValue(SR.WebPart_DefaultImportErrorMessage),
        Personalizable(PersonalizationScope.Shared),
        WebSysDescription(SR.WebPart_ImportErrorMessage),
        ]
        public virtual string ImportErrorMessage {
            get {
                return (_importErrorMessage != null) ?
                    _importErrorMessage : SR.GetString(SR.WebPart_DefaultImportErrorMessage);
            }
            set {
                _importErrorMessage = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool IsShared {
            get {
                return _isShared;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool IsStandalone {
            get {
                return _isStandalone;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool IsStatic {
            get {
                return _isStatic;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        Localizable(true),
        ]
        public virtual string Subtitle {
            get {
                return String.Empty;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// Overriden to mark as personalizable
        /// </devdoc>
        [
        Personalizable,
        ]
        public override string Title {
            get {
                return base.Title;
            }
            set {
                base.Title = value;
            }
        }

        // ID rendered on the title bar of the WebPart, so a mouse listener can be attached
        // for drag and drop.
        internal string TitleBarID {
            get {
                return titleBarIDPrefix + ID;
            }
        }

        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebCategory("WebPartAppearance"),
        Personalizable(PersonalizationScope.Shared),
        WebSysDescription(SR.WebPart_TitleIconImageUrl),
        ]
        public virtual string TitleIconImageUrl {
            get {
                return (_titleIconImageUrl != null) ? _titleIconImageUrl : String.Empty;
            }
            set {
                if (CrossSiteScriptingValidation.IsDangerousUrl(value)) {
                    throw new ArgumentException(SR.GetString(SR.WebPart_BadUrl, value), "value");
                }
                _titleIconImageUrl = value;
            }
        }

        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        Personalizable(PersonalizationScope.Shared),
        Themeable(false),
        WebCategory("WebPartBehavior"),
        WebSysDescription(SR.WebPart_TitleUrl),
        ]
        public virtual string TitleUrl {
            get {
                return (_titleUrl != null) ? _titleUrl : String.Empty;
            }
            set {
                if (CrossSiteScriptingValidation.IsDangerousUrl(value)) {
                    throw new ArgumentException(SR.GetString(SR.WebPart_BadUrl, value), "value");
                }
                _titleUrl = value;
            }
        }

        internal Dictionary<ProviderConnectionPoint, int> TrackerCounter {
            get {
                if (_trackerCounter == null) {
                    _trackerCounter = new Dictionary<ProviderConnectionPoint, int>();
                }
                return _trackerCounter;
            }
        }

        /// <devdoc>
        /// Overriden by subclasses to add Verbs for this WebPart.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual WebPartVerbCollection Verbs {
            get {
                return WebPartVerbCollection.Empty;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual object WebBrowsableObject {
            get {
                return this;
            }
        }

        protected WebPartManager WebPartManager {
            get {
                return _webPartManager;
            }
        }

        // ID rendered on the table containing the whole web part.  We shouldn't render ClientID
        // on the table, since it will be rendered by the WebPart on the container for the part
        // contents.  We shouldn't render ID either, since it may be the same as another control
        // on the page, and it should be different than ID since it is being rendered by the Zone,
        // not the WebPart.
        internal string WholePartID {
            get {
                return WholePartIDPrefix + ID;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// Overriden to mark as personalizable
        /// </devdoc>
        [
        Personalizable
        ]
        public override Unit Width {
            get {
                return base.Width;
            }
            set {
                base.Width = value;
            }
        }

        /// <devdoc>
        /// The WebPartZone that this WebPart is currently rendered within.
        /// If the WebPart is closed, returns the WebPartZone that the WebPart
        /// was last rendered within.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public WebPartZoneBase Zone {
            get {
                if (_zone == null) {
                    string zoneID = ZoneID;
                    if (!String.IsNullOrEmpty(zoneID) && WebPartManager != null) {
                        WebPartZoneCollection zones = WebPartManager.Zones;
                        if (zones != null) {
                            _zone = zones[zoneID];
                        }
                    }
                }

                return _zone;
            }
        }

        /// <devdoc>
        /// The ID of the web part zone that this web part logically belongs to
        /// </devdoc>
        internal string ZoneID {
            get {
                return _zoneID;
            }
            set {
                if (ZoneID != value) {
                    _zoneID = value;
                    // Invalidate cache
                    _zone = null;
                }
            }
        }

        /// <devdoc>
        /// The index of this web part within the web part zone it logically belongs to.
        /// An index of -1 means the part is not currently in a zone.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int ZoneIndex {
            get {
                return _zoneIndex;
            }
        }

        public virtual EditorPartCollection CreateEditorParts() {
            return EditorPartCollection.Empty;
        }

        protected internal virtual void OnClosing(EventArgs e) {
        }

        protected internal virtual void OnConnectModeChanged(EventArgs e) {
        }

        protected internal virtual void OnDeleting(EventArgs e) {
        }

        protected internal virtual void OnEditModeChanged(EventArgs e) {
        }

        internal override void PreRenderRecursiveInternal() {
            if (IsStandalone) {
                if (Hidden) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPart_NotStandalone, "Hidden", ID));
                }
            }
            else {
                if (!Visible) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPart_OnlyStandalone, "Visible", ID));
                }
            }
            base.PreRenderRecursiveInternal();
        }

        internal void SetConnectErrorMessage(string connectErrorMessage) {
            // Only set the error message if it has not been set already.  The first error message
            // set should be displayed.
            if (String.IsNullOrEmpty(_connectErrorMessage)) {
                _connectErrorMessage = connectErrorMessage;
            }
        }

        internal void SetHasUserData(bool hasUserData) {
            _hasUserData = hasUserData;
        }

        internal void SetHasSharedData(bool hasSharedData) {
            _hasSharedData = hasSharedData;
        }

        internal void SetIsClosed(bool isClosed) {
            _isClosed = isClosed;
        }

        internal void SetIsShared(bool isShared) {
            _isShared = isShared;
        }

        internal void SetIsStandalone(bool isStandalone) {
            _isStandalone = isStandalone;
        }

        internal void SetIsStatic(bool isStatic) {
            _isStatic = isStatic;
        }

        protected void SetPersonalizationDirty() {
            if (WebPartManager == null) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartManagerRequired));
            }

            WebPartManager.Personalization.SetDirty(this);
        }

        /// <devdoc>
        /// This method allows a non-WebPart control to mark its personalization as dirty.
        /// </devdoc>
        public static void SetPersonalizationDirty(Control control) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }
            if (control.Page == null) {
                throw new ArgumentException(SR.GetString(SR.PropertyCannotBeNull, "Page"), "control");
            }

            WebPartManager wpm = WebPartManager.GetCurrentWebPartManager(control.Page);
            if (wpm == null) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartManagerRequired));
            }

            WebPart webPart = wpm.GetGenericWebPart(control);
            if (webPart == null) {
                throw new ArgumentException(SR.GetString(SR.WebPart_NonWebPart), "control");
            }

            webPart.SetPersonalizationDirty();
        }

        internal void SetWebPartManager(WebPartManager webPartManager) {
            _webPartManager = webPartManager;
        }

        /// <devdoc>
        /// The index of this web part within the web part zone it logically belongs to.
        /// </devdoc>
        internal void SetZoneIndex(int zoneIndex) {
            if (zoneIndex < 0) {
                throw new ArgumentOutOfRangeException("zoneIndex");
            }
            _zoneIndex = zoneIndex;
        }

        // If this is a GenericWebPart, returns the ChildControl.  Else, just returns a pointer to itself.
        // Used when you need the Control to pass to methods on ConnectionPoint.
        internal Control ToControl() {
            GenericWebPart genericWebPart = this as GenericWebPart;
            if (genericWebPart != null) {
                Control control = genericWebPart.ChildControl;
                if (control != null) {
                    return control;
                } else {
                    throw new InvalidOperationException(SR.GetString(SR.GenericWebPart_ChildControlIsNull));
                }
            }
            else {
                return this;
            }
        }

        protected override void TrackViewState() {
            if (WebPartManager != null) {
                WebPartManager.Personalization.ApplyPersonalizationState(this);
            }

            base.TrackViewState();
        }

         /// <devdoc>
        /// We need this to return nonzero for any two distinct WebParts on the page,
        /// since we use SortedList in WebPartManager, and SortedList cannot contain
        /// two keys where the IComparer returns zero.  Two WebParts can have the same
        /// ZoneIndex when we merge the Shared and User parts.  We use the ID
        /// to order the parts if ZoneIndex is the same.
        /// </devdoc>
        internal sealed class ZoneIndexComparer : IComparer {
            public int Compare(object x, object y) {
                WebPart p1 = (WebPart)x;
                WebPart p2 = (WebPart)y;

                int c = p1.ZoneIndex - p2.ZoneIndex;
                if (c == 0) {
                    c = String.Compare(p1.ID, p2.ID, StringComparison.CurrentCulture);
                }
                return c;
            }
        }
    }
}
