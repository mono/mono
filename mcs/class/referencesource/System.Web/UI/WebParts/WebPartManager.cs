//------------------------------------------------------------------------------
// <copyright file="WebPartManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;
    using System.Xml;

    [
    Bindable(false),
    Designer("System.Web.UI.Design.WebControls.WebParts.WebPartManagerDesigner, " + AssemblyRef.SystemDesign),
    NonVisualControl(),
    ParseChildren(true),
    PersistChildren(false),
    ViewStateModeById(),
    ]
    public class WebPartManager : Control, INamingContainer, IPersonalizable {

        public static readonly WebPartDisplayMode CatalogDisplayMode = new CatalogWebPartDisplayMode();
        public static readonly WebPartDisplayMode ConnectDisplayMode = new ConnectWebPartDisplayMode();
        public static readonly WebPartDisplayMode DesignDisplayMode = new DesignWebPartDisplayMode();
        public static readonly WebPartDisplayMode EditDisplayMode = new EditWebPartDisplayMode();
        public static readonly WebPartDisplayMode BrowseDisplayMode = new BrowseWebPartDisplayMode();

        // Cache collections of ConnectionPoints for each object Type.  We store an array of
        // 2 ConnectionPointCollections (consumer, provider) for each Type.  The Hashtable
        // is synchronized so it is threadsafe with multiple writers.
        private static Hashtable ConnectionPointsCache;

        private static readonly object AuthorizeWebPartEvent = new object();
        private static readonly object ConnectionsActivatedEvent = new object();
        private static readonly object ConnectionsActivatingEvent = new object();
        private static readonly object DisplayModeChangedEvent = new object();
        private static readonly object DisplayModeChangingEvent = new object();
        private static readonly object SelectedWebPartChangingEvent = new object();
        private static readonly object SelectedWebPartChangedEvent = new object();
        private static readonly object WebPartAddedEvent = new object();
        private static readonly object WebPartAddingEvent = new object();
        private static readonly object WebPartClosedEvent = new object();
        private static readonly object WebPartClosingEvent = new object();
        private static readonly object WebPartDeletedEvent = new object();
        private static readonly object WebPartDeletingEvent = new object();
        private static readonly object WebPartMovedEvent = new object();
        private static readonly object WebPartMovingEvent = new object();
        private static readonly object WebPartsConnectedEvent = new object();
        private static readonly object WebPartsConnectingEvent = new object();
        private static readonly object WebPartsDisconnectedEvent = new object();
        private static readonly object WebPartsDisconnectingEvent = new object();

        private PermissionSet _minimalPermissionSet;
        private PermissionSet _mediumPermissionSet;
        private bool? _usePermitOnly;

        private const string DynamicConnectionIDPrefix = "c";
        private const string DynamicWebPartIDPrefix = "wp";

        private const int baseIndex = 0;
        private const int selectedWebPartIndex = 1;
        private const int displayModeIndex = 2;
        private const int controlStateArrayLength = 3;

        private WebPartPersonalization _personalization;
        private WebPartDisplayMode _displayMode;
        private WebPartDisplayModeCollection _displayModes;
        private WebPartDisplayModeCollection _supportedDisplayModes;
        private WebPartManagerInternals _internals;

        private bool _allowCreateDisplayTitles;
        private bool _pageInitComplete;

        // When this flag is set to false, then cancelled events are ignored.  We will not actually
        // cancel the action even though e.Cancel is true.  (VSWhidbey 516012)
        private bool _allowEventCancellation;

        private PersonalizationDictionary _personalizationState;
        private bool _hasDataChanged;

        private WebPartConnectionCollection _staticConnections;
        private WebPartConnectionCollection _dynamicConnections;

        private WebPartZoneCollection _webPartZones;
        private TransformerTypeCollection _availableTransformers;

        // Dictionary mapping a WebPart to its DisplayTitle.  Created and filled on demand when
        // GetDisplayTitle() is called after PreRender.
        private IDictionary _displayTitles;

        // NOTE: We are no longer rendering the LRO or PDF characters (VSWhidbey 364897)
        // LRO is the Unicode left-to-right override marker.  Effectively creates a "run break"
        // so that contents in parentheses et. al. maintain correct reading order regardless
        // of text direction (LTR or RTL).  PDF "pops" the formatting and allows ensuing text
        // to lay out as it would w/o the markers.  The PDF is needed when constructing dialogs
        // that use the web part titles.  We must use the Unicode characters instead of
        // <span dir="ltr">, since the DisplayTitle is HTML Encoded before being rendered.
        // (VSWhidbey 190501)
        // private static string LRO = new String((char)0x202d, 1);  // left-to-right override
        // private static string PDF = new String((char)0x202c, 1);  // pop directional formatting

        // PERF: At compile-time, compute strings to append to DisplayTitle
        // We chose to compute suffixes up to 20, since it is unlikely there will be more than
        // 20 WebParts with the same title.
        // The 0 element is currently not used, but is a placeholder so the index into the array
        // matches the string.
        private static string[] displayTitleSuffix = new string[] {
            " [0]", " [1]", " [2]", " [3]", " [4]", " [5]", " [6]", " [7]", " [8]", " [9]", " [10]",
            " [11]", " [12]", " [13]", " [14]", " [15]", " [16]", " [17]", " [18]", " [19]", " [20]" };

        // Dictionary mapping a zone to the parts in the zone.  Used by GetAllWebPartsForZone
        // to improve performance.
        private IDictionary _partsForZone;

        // Contains the IDs of WebParts and Child Controls already added.  WebParts and the child
        // controls of GenericWebParts share the same namespace, meaning you cannot have a WebPart
        // and a Child Control with the same ID. An exception is thrown if a WebPart or Child Control
        // is added with a duplicate ID.
        private IDictionary _partAndChildControlIDs;

        // Contains the IDs of Zones already added.  An exception is thrown if a Zone is added with
        // a duplicate ID.
        private IDictionary _zoneIDs;

        private WebPart _selectedWebPart;

        private bool _renderClientScript;

        private const string DragOverlayElementHtmlTemplate = @"
<div id=""{0}___Drag"" style=""display:none; position:absolute; z-index: 32000; filter:alpha(opacity=75)""></div>";
        private const string ExportSensitiveDataWarningDeclaration = "ExportSensitiveDataWarningDeclaration";
        private const string CloseProviderWarningDeclaration = "CloseProviderWarningDeclaration";
        private const string DeleteWarningDeclaration = "DeleteWarningDeclaration";
        private const string StartupScript = @"
<script type=""text/javascript"">

__wpm = new WebPartManager();
__wpm.overlayContainerElement = {0};
__wpm.personalizationScopeShared = {1};

var zoneElement;
var zoneObject;
{2}
</script>
";
        private const string ZoneScript = @"
zoneElement = document.getElementById('{0}');
if (zoneElement != null) {{
    zoneObject = __wpm.AddZone(zoneElement, '{1}', {2}, {3}, '{4}');";

        private const string ZonePartScript = @"
    zoneObject.AddWebPart(document.getElementById('{0}'), {1}, {2});";

        private const string ZoneEndScript = @"
}";

        private const string AuthorizationFilterName = "AuthorizationFilter";
        private const string ImportErrorMessageName = "ImportErrorMessage";
        private const string ZoneIDName = "ZoneID";
        private const string ZoneIndexName = "ZoneIndex";

        internal const string ExportRootElement = "webParts";
        internal const string ExportPartElement = "webPart";
        internal const string ExportPartNamespaceAttribute = "xmlns";
        internal const string ExportPartNamespaceValue = "http://schemas.microsoft.com/WebPart/v3";
        internal const string ExportMetaDataElement = "metaData";
        internal const string ExportTypeElement = "type";
        internal const string ExportErrorMessageElement = "importErrorMessage";
        internal const string ExportDataElement = "data";
        internal const string ExportPropertiesElement = "properties";
        internal const string ExportPropertyElement = "property";
        internal const string ExportTypeNameAttribute = "name";
        internal const string ExportUserControlSrcAttribute = "src";
        internal const string ExportPropertyNameAttribute = "name";
        internal const string ExportGenericPartPropertiesElement = "genericWebPartProperties";
        internal const string ExportIPersonalizableElement = "ipersonalizable";
        internal const string ExportPropertyTypeAttribute = "type";
        internal const string ExportPropertyScopeAttribute = "scope";
        internal const string ExportPropertyNullAttribute = "null";

        private const string ExportTypeBool = "bool";
        private const string ExportTypeInt = "int";
        private const string ExportTypeChromeState = "chromestate";
        private const string ExportTypeChromeType = "chrometype";
        private const string ExportTypeColor = "color";
        private const string ExportTypeDateTime = "datetime";
        private const string ExportTypeDirection = "direction";
        private const string ExportTypeDouble = "double";
        private const string ExportTypeExportMode = "exportmode";
        private const string ExportTypeFontSize = "fontsize";
        private const string ExportTypeHelpMode = "helpmode";
        private const string ExportTypeObject = "object";
        private const string ExportTypeSingle = "single";
        private const string ExportTypeString = "string";
        private const string ExportTypeUnit = "unit";

        /// <devdoc>
        /// </devdoc>
        public WebPartManager() {
            _allowEventCancellation = true;
            _displayMode = BrowseDisplayMode;
            _webPartZones = new WebPartZoneCollection();
            _partAndChildControlIDs = new HybridDictionary(true /* caseInsensitive */);
            _zoneIDs = new HybridDictionary(true /* caseInsensitive */);
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public TransformerTypeCollection AvailableTransformers {
            get {
                if (_availableTransformers == null) {
                    _availableTransformers = CreateAvailableTransformers();
                }
                return _availableTransformers;
            }
        }

        [
        WebCategory("Behavior"),
        WebSysDefaultValue(SR.WebPartManager_DefaultCloseProviderWarning),
        WebSysDescription(SR.WebPartManager_CloseProviderWarning)
        ]
        public virtual string CloseProviderWarning {
            get {
                object o = ViewState["CloseProviderWarning"];
                return (o != null) ? (string)o : SR.GetString(SR.WebPartManager_DefaultCloseProviderWarning);
            }
            set {
                ViewState["CloseProviderWarning"] = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public WebPartConnectionCollection Connections {
            get {
                WebPartConnectionCollection connections = new WebPartConnectionCollection(this);
                if (_staticConnections != null) {
                    foreach (WebPartConnection connection in _staticConnections) {
                        if (!Internals.ConnectionDeleted(connection)) {
                            connections.Add(connection);
                        }
                    }
                }
                if (_dynamicConnections != null) {
                    foreach (WebPartConnection connection in _dynamicConnections) {
                        if (!Internals.ConnectionDeleted(connection)) {
                            connections.Add(connection);
                        }
                    }
                }
                connections.SetReadOnly(SR.WebPartManager_ConnectionsReadOnly);
                return connections;
            }
        }

        // Hide the Controls property from IntelliSense.  The developer should use the
        // WebParts property instead.
        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override ControlCollection Controls {
            get {
                return base.Controls;
            }
        }

        [
        WebCategory("Behavior"),
        WebSysDefaultValue(SR.WebPartManager_DefaultDeleteWarning),
        WebSysDescription(SR.WebPartManager_DeleteWarning)
        ]
        public virtual string DeleteWarning {
            get {
                object o = ViewState["DeleteWarning"];
                return (o != null) ? (string)o : SR.GetString(SR.WebPartManager_DefaultDeleteWarning);
            }
            set {
                ViewState["DeleteWarning"] = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual WebPartDisplayMode DisplayMode {
            get {
                return _displayMode;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }

                if (DisplayMode == value) {
                    return;
                }

                if (SupportedDisplayModes.Contains(value) == false) {
                    throw new ArgumentException(SR.GetString(SR.WebPartManager_InvalidDisplayMode), "value");
                }

                if (!value.IsEnabled(this)) {
                    throw new ArgumentException(SR.GetString(SR.WebPartManager_DisabledDisplayMode), "value");
                }

                WebPartDisplayModeCancelEventArgs dmce = new WebPartDisplayModeCancelEventArgs(value);
                OnDisplayModeChanging(dmce);

                if (_allowEventCancellation && dmce.Cancel) {
                    return;
                }

                // Custom display modes can take actions like this in the OnDisplayModeChanging method.
                // For example:
                // public override void OnDisplayModeChanging(WebPartDisplayModeCancelEventArgs e) {
                //     base.OnDisplayModeChanging(e);
                //     if (e.Cancel) return;
                //     if (DisplayMode == CustomDisplayMode) {
                //         // Take some actions and set e.Cancel=true if appropriate
                //     }
                // }

                // End web part connecting if necessary
                if ((DisplayMode == ConnectDisplayMode) && (SelectedWebPart != null)) {
                    EndWebPartConnecting();
                    if (SelectedWebPart != null) {
                        // WebPartConnectModeChanging event was cancelled
                        return;
                    }
                }

                // End web part editing if necessary
                if ((DisplayMode == EditDisplayMode) && (SelectedWebPart != null)) {
                    EndWebPartEditing();
                    if (SelectedWebPart != null) {
                        // WebPartEditModeChanging event was cancelled
                        return;
                    }
                }

                WebPartDisplayModeEventArgs dme = new WebPartDisplayModeEventArgs(DisplayMode);
                _displayMode = value;
                OnDisplayModeChanged(dme);
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public WebPartDisplayModeCollection DisplayModes {
            get {
                if (_displayModes == null) {
                    _displayModes = CreateDisplayModes();
                    _displayModes.SetReadOnly(SR.WebPartManager_DisplayModesReadOnly);
                }

                return _displayModes;
            }
        }

        protected internal WebPartConnectionCollection DynamicConnections {
            get {
                if (_dynamicConnections == null) {
                    _dynamicConnections = new WebPartConnectionCollection(this);
                }
                return _dynamicConnections;
            }
        }

        [
        DefaultValue(true),
        WebCategory("Behavior"),
        WebSysDescription(SR.WebPartManager_EnableClientScript)
        ]
        public virtual bool EnableClientScript {
            get {
                object o = ViewState["EnableClientScript"];
                return (o != null) ? (bool)o : true;
            }
            set {
                ViewState["EnableClientScript"] = value;
            }
        }

        // Theming must be enabled, so the WebPart child controls have theming enabled
        [
        Browsable(false),
        DefaultValue(true),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override bool EnableTheming {
            get {
                return true;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.WebPartManager_CantSetEnableTheming));
            }
        }

        [
        WebCategory("Behavior"),
        WebSysDefaultValue(SR.WebPartChrome_ConfirmExportSensitive),
        WebSysDescription(SR.WebPartManager_ExportSensitiveDataWarning)
        ]
        public virtual string ExportSensitiveDataWarning {
            get {
                object o = ViewState["ExportSensitiveDataWarning"];
                return (o != null) ? (string)o : SR.GetString(SR.WebPartChrome_ConfirmExportSensitive);
            }
            set {
                ViewState["ExportSensitiveDataWarning"] = value;
            }
        }

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        protected WebPartManagerInternals Internals {
            get {
                if (_internals == null) {
                    _internals = new WebPartManagerInternals(this);
                }
                return _internals;
            }
        }

        /// <devdoc>
        /// </devdoc>
        protected virtual bool IsCustomPersonalizationStateDirty {
            get {
                return _hasDataChanged;
            }
        }

        // PermissionSet that allows only Execution and AspNetHostingPermissionLevel.Medium.
        // AspNetHostingPermissionLevel.Medium is needed to call BuildManager.GetType().
        // Used for during Import for type deserialization.
        protected virtual PermissionSet MediumPermissionSet {
            get {
                if (_mediumPermissionSet == null) {
                    _mediumPermissionSet = new PermissionSet(PermissionState.None);
                    _mediumPermissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                    _mediumPermissionSet.AddPermission(new AspNetHostingPermission(AspNetHostingPermissionLevel.Medium));
                }
                return _mediumPermissionSet;
            }
        }

        // PermissionSet that allows only Execution and AspNetHostingPermissionLevel.Minimal.
        // Used for during Import for everything except type deserialization.
        protected virtual PermissionSet MinimalPermissionSet {
            get {
                if (_minimalPermissionSet == null) {
                    _minimalPermissionSet = new PermissionSet(PermissionState.None);
                    _minimalPermissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                    _minimalPermissionSet.AddPermission(new AspNetHostingPermission(AspNetHostingPermissionLevel.Minimal));
                }
                return _minimalPermissionSet;
            }
        }

        /// <devdoc>
        /// </devdoc>
        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Behavior"),
        WebSysDescription(SR.WebPartManager_Personalization)
        ]
        public WebPartPersonalization Personalization {
            get {
                if (_personalization == null) {
                    _personalization = CreatePersonalization();
                }

                return _personalization;
            }
        }

        internal bool RenderClientScript {
            get {
                return _renderClientScript;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public WebPart SelectedWebPart {
            get {
                return _selectedWebPart;
            }
        }

        [
        Browsable(false),
        DefaultValue(""),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override string SkinID {
            get {
                return String.Empty;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.NoThemingSupport, this.GetType().Name));
            }
        }

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Behavior"),
        WebSysDescription(SR.WebPartManager_StaticConnections),
        ]
        public WebPartConnectionCollection StaticConnections {
            get {
                if (_staticConnections == null) {
                    _staticConnections = new WebPartConnectionCollection(this);
                }
                return _staticConnections;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public WebPartDisplayModeCollection SupportedDisplayModes {
            get {
                if (_supportedDisplayModes == null) {
                    _supportedDisplayModes = new WebPartDisplayModeCollection();

                    foreach (WebPartDisplayMode mode in DisplayModes) {
                        if (mode.AssociatedWithToolZone == false) {
                            _supportedDisplayModes.Add(mode);
                        }
                    }

                    _supportedDisplayModes.SetReadOnly(SR.WebPartManager_DisplayModesReadOnly);
                }
                return _supportedDisplayModes;
            }
        }

        // Only call PermitOnly() in legacy CAS mode.  In the v4 CAS model, calling PermitOnly() would prevent us from calling
        // Activator.CreateInstance() on types in App_Code (assuming it is non-APTCA). (Dev10 
        private bool UsePermitOnly {
            get {
                if (!_usePermitOnly.HasValue) {
                    _usePermitOnly =  RuntimeConfig.GetAppConfig().Trust.LegacyCasModel;
                }
                return _usePermitOnly.Value;
            }
        }

        [
        Bindable(false),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override bool Visible {
            get {
                // Even though we are a non-visual control, this returns true, because we want our
                // child controls (the WebParts) to be Visible.
                return true;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.ControlNonVisual, this.GetType().Name));
            }
        }

        /// <devdoc>
        /// All the WebParts on the page.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public WebPartCollection WebParts {
            get {
                // PERF: Consider changing WebPartCollection so it just wraps the ControlCollection,
                // instead of copying the controls to a new collection.
                if (HasControls()) {
                    return new WebPartCollection(Controls);
                }
                else {
                    return new WebPartCollection();
                }
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public WebPartZoneCollection Zones {
            get {
                return _webPartZones;
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_AuthorizeWebPart)
        ]
        public event WebPartAuthorizationEventHandler AuthorizeWebPart {
            add {
                Events.AddHandler(AuthorizeWebPartEvent, value);
            }
            remove {
                Events.RemoveHandler(AuthorizeWebPartEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_ConnectionsActivated)
        ]
        public event EventHandler ConnectionsActivated {
            add {
                Events.AddHandler(ConnectionsActivatedEvent, value);
            }
            remove {
                Events.RemoveHandler(ConnectionsActivatedEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_ConnectionsActivating)
        ]
        public event EventHandler ConnectionsActivating {
            add {
                Events.AddHandler(ConnectionsActivatingEvent, value);
            }
            remove {
                Events.RemoveHandler(ConnectionsActivatingEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_DisplayModeChanged)
        ]
        public event WebPartDisplayModeEventHandler DisplayModeChanged {
            add {
                Events.AddHandler(DisplayModeChangedEvent, value);
            }
            remove {
                Events.RemoveHandler(DisplayModeChangedEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_DisplayModeChanging)
        ]
        public event WebPartDisplayModeCancelEventHandler DisplayModeChanging {
            add {
                Events.AddHandler(DisplayModeChangingEvent, value);
            }
            remove {
                Events.RemoveHandler(DisplayModeChangingEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_SelectedWebPartChanged)
        ]
        public event WebPartEventHandler SelectedWebPartChanged {
            add {
                Events.AddHandler(SelectedWebPartChangedEvent, value);
            }
            remove {
                Events.RemoveHandler(SelectedWebPartChangedEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_SelectedWebPartChanging)
        ]
        public event WebPartCancelEventHandler SelectedWebPartChanging {
            add {
                Events.AddHandler(SelectedWebPartChangingEvent, value);
            }
            remove {
                Events.RemoveHandler(SelectedWebPartChangingEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_WebPartAdded)
        ]
        public event WebPartEventHandler WebPartAdded {
            add {
                Events.AddHandler(WebPartAddedEvent, value);
            }
            remove {
                Events.RemoveHandler(WebPartAddedEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_WebPartAdding)
        ]
        public event WebPartAddingEventHandler WebPartAdding {
            add {
                Events.AddHandler(WebPartAddingEvent, value);
            }
            remove {
                Events.RemoveHandler(WebPartAddingEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_WebPartClosed)
        ]
        public event WebPartEventHandler WebPartClosed {
            add {
                Events.AddHandler(WebPartClosedEvent, value);
            }
            remove {
                Events.RemoveHandler(WebPartClosedEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_WebPartClosing)
        ]
        public event WebPartCancelEventHandler WebPartClosing {
            add {
                Events.AddHandler(WebPartClosingEvent, value);
            }
            remove {
                Events.RemoveHandler(WebPartClosingEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_WebPartDeleted)
        ]
        public event WebPartEventHandler WebPartDeleted {
            add {
                Events.AddHandler(WebPartDeletedEvent, value);
            }
            remove {
                Events.RemoveHandler(WebPartDeletedEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_WebPartDeleting)
        ]
        public event WebPartCancelEventHandler WebPartDeleting {
            add {
                Events.AddHandler(WebPartDeletingEvent, value);
            }
            remove {
                Events.RemoveHandler(WebPartDeletingEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_WebPartMoved)
        ]
        public event WebPartEventHandler WebPartMoved {
            add {
                Events.AddHandler(WebPartMovedEvent, value);
            }
            remove {
                Events.RemoveHandler(WebPartMovedEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_WebPartMoving)
        ]
        public event WebPartMovingEventHandler WebPartMoving {
            add {
                Events.AddHandler(WebPartMovingEvent, value);
            }
            remove {
                Events.RemoveHandler(WebPartMovingEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_WebPartsConnected)
        ]
        public event WebPartConnectionsEventHandler WebPartsConnected {
            add {
                Events.AddHandler(WebPartsConnectedEvent, value);
            }
            remove {
                Events.RemoveHandler(WebPartsConnectedEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_WebPartsConnecting)
        ]
        public event WebPartConnectionsCancelEventHandler WebPartsConnecting {
            add {
                Events.AddHandler(WebPartsConnectingEvent, value);
            }
            remove {
                Events.RemoveHandler(WebPartsConnectingEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_WebPartsDisconnected)
        ]
        public event WebPartConnectionsEventHandler WebPartsDisconnected {
            add {
                Events.AddHandler(WebPartsDisconnectedEvent, value);
            }
            remove {
                Events.RemoveHandler(WebPartsDisconnectedEvent, value);
            }
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.WebPartManager_WebPartsDisconnecting)
        ]
        public event WebPartConnectionsCancelEventHandler WebPartsDisconnecting {
            add {
                Events.AddHandler(WebPartsDisconnectingEvent, value);
            }
            remove {
                Events.RemoveHandler(WebPartsDisconnectingEvent, value);
            }
        }

        protected virtual void ActivateConnections() {
            try {
                // ActivateConnections() is called as a result of no user action, so the events
                // should not be cancellable. (VSWhidbey 516012)
                _allowEventCancellation = false;
                foreach (WebPartConnection connection in ConnectionsToActivate()) {
                    connection.Activate();
                }
            }
            finally {
                _allowEventCancellation = true;
            }
        }

        // Called by WebPartManagerInternals
        internal void AddWebPart(WebPart webPart) {
            ((WebPartManagerControlCollection)Controls).AddWebPart(webPart);
        }

        private WebPart AddDynamicWebPartToZone(WebPart webPart, WebPartZoneBase zone, int zoneIndex) {
            Debug.Assert(Personalization.IsModifiable);

            // Zone should not be set on a dynamic web part being added to the page for the first time
            Debug.Assert(webPart.Zone == null);

            // Only add WebPart if IsAuthorized(webPart) == true
            if (!IsAuthorized(webPart)) {
                return null;
            }

            WebPart newWebPart = CopyWebPart(webPart);
            Internals.SetIsStatic(newWebPart, false);
            Internals.SetIsShared(newWebPart, Personalization.Scope == PersonalizationScope.Shared);

            AddWebPartToZone(newWebPart, zone, zoneIndex);
            Internals.AddWebPart(newWebPart);

            // We set the personalized properties on the added WebPart AFTER it has been added to the
            // control tree, since we want to exactly recreate the process the WebPart will go through
            // when it is added from Personalization.
            Personalization.CopyPersonalizationState(webPart, newWebPart);

            // Raise event at very end of Add method
            OnWebPartAdded(new WebPartEventArgs(newWebPart));

            return newWebPart;
        }

        // Returns the WebPart that was actually added.  For an existing Closed WebPart, this is a reference
        // to the webPart parameter.  For a new DynamicWebPart, this will be a copy of the webPart parameter.
        public WebPart AddWebPart(WebPart webPart, WebPartZoneBase zone, int zoneIndex) {
            Personalization.EnsureEnabled(/* ensureModifiable */ true);

            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            // Do not check that Controls.Contains(webPart), since this will be called on a WebPart
            // before it is added to the Controls collection.
            if (zone == null) {
                throw new ArgumentNullException("zone");
            }
            if (_webPartZones.Contains(zone) == false) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_MustRegister), "zone");
            }
            if (zoneIndex < 0) {
                throw new ArgumentOutOfRangeException("zoneIndex");
            }
            if (webPart.Zone != null && !webPart.IsClosed) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_AlreadyInZone), "webPart");
            }

            WebPartAddingEventArgs e = new WebPartAddingEventArgs(webPart, zone, zoneIndex);
            OnWebPartAdding(e);
            if (_allowEventCancellation && e.Cancel) {
                return null;
            }

            WebPart addedWebPart;

            // If a part is already in the controls collection, dynamic or static, just make it
            // not closed and add it to the specified zone
            if (Controls.Contains(webPart)) {
                addedWebPart = webPart;
                AddWebPartToZone(webPart, zone, zoneIndex);
                OnWebPartAdded(new WebPartEventArgs(addedWebPart));
            } else {
                addedWebPart = AddDynamicWebPartToZone(webPart, zone, zoneIndex);
                // OnWebPartAdded() is called by AddDynamicWebPartToZone
            }

#if DEBUG
            CheckPartZoneIndexes(zone);
#endif

            return addedWebPart;
        }

        /// <devdoc>
        /// Adds the part to the dictionary mapping zones to parts.
        /// </devdoc>
        private void AddWebPartToDictionary(WebPart webPart) {
            if (_partsForZone != null) {
                string zoneID = Internals.GetZoneID(webPart);
                if (!String.IsNullOrEmpty(zoneID)) {
                    SortedList partsForZone = (SortedList)(_partsForZone[zoneID]);
                    if (partsForZone == null) {
                        partsForZone = new SortedList(new WebPart.ZoneIndexComparer());
                        _partsForZone[zoneID] = partsForZone;
                    }
                    partsForZone.Add(webPart, null);
                }
            }
        }

        /// <devdoc>
        /// Adds a web part to a zone at the specified zoneIndex, and renumbers all the parts in the zone
        /// sequentially.
        /// </devdoc>
        private void AddWebPartToZone(WebPart webPart, WebPartZoneBase zone, int zoneIndex) {
            Debug.Assert(webPart.Zone == null || webPart.IsClosed);

            // All the parts for the zone
            IList allParts = GetAllWebPartsForZone(zone);

            // The parts for the zone that were actually rendered
            WebPartCollection renderedParts = GetWebPartsForZone(zone);

            // The zoneIndex parameter is the desired index in the renderedParts collection.
            // Calculate the destination index into the allParts collection. (VSWhidbey 77719)
            int allPartsDestinationIndex;
            if (zoneIndex < renderedParts.Count) {
                WebPart successor = renderedParts[zoneIndex];
                Debug.Assert(allParts.Contains(successor));
                allPartsDestinationIndex = allParts.IndexOf(successor);
            }
            else {
                allPartsDestinationIndex = allParts.Count;
            }

            // Renumber all parts in the zone, leaving room for the added part
            for (int i = 0; i < allPartsDestinationIndex; i++) {
                WebPart part = ((WebPart)allParts[i]);
                Internals.SetZoneIndex(part, i);
            }
            for (int i = allPartsDestinationIndex; i < allParts.Count; i++) {
                WebPart part = ((WebPart)allParts[i]);
                Internals.SetZoneIndex(part, i + 1);
            }

            // Set the part index and add to destination zone
            Internals.SetZoneIndex(webPart, allPartsDestinationIndex);
            Internals.SetZoneID(webPart, zone.ID);
            Internals.SetIsClosed(webPart, false);

            _hasDataChanged = true;

            AddWebPartToDictionary(webPart);
        }

        public virtual void BeginWebPartConnecting(WebPart webPart) {
            Personalization.EnsureEnabled(/* ensureModifiable */ true);

            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }

            if (webPart.IsClosed) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_CantBeginConnectingClosed), "webPart");
            }

            if (!Controls.Contains(webPart)) {
                throw new ArgumentException(SR.GetString(SR.UnknownWebPart), "webPart");
            }

            if (DisplayMode != ConnectDisplayMode) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartManager_MustBeInConnect));
            }

            if (webPart == SelectedWebPart) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_AlreadyInConnect), "webPart");
            }

            WebPartCancelEventArgs ce = new WebPartCancelEventArgs(webPart);
            OnSelectedWebPartChanging(ce);
            if (_allowEventCancellation && ce.Cancel) {
                return;
            }

            if (SelectedWebPart != null) {
                EndWebPartConnecting();
                if (SelectedWebPart != null) {
                    // The ConnectModeChange was cancelled
                    return;
                }
            }

            SetSelectedWebPart(webPart);

            Internals.CallOnConnectModeChanged(webPart);

            OnSelectedWebPartChanged(new WebPartEventArgs(webPart));
        }

        public virtual void BeginWebPartEditing(WebPart webPart) {
            Personalization.EnsureEnabled(/* ensureModifiable */ true);

            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }

            if (webPart.IsClosed) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_CantBeginEditingClosed), "webPart");
            }

            if (!Controls.Contains(webPart)) {
                throw new ArgumentException(SR.GetString(SR.UnknownWebPart), "webPart");
            }

            if (DisplayMode != EditDisplayMode) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartManager_MustBeInEdit));
            }

            if (webPart == SelectedWebPart) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_AlreadyInEdit), "webPart");
            }

            WebPartCancelEventArgs ce = new WebPartCancelEventArgs(webPart);
            OnSelectedWebPartChanging(ce);
            if (_allowEventCancellation && ce.Cancel) {
                return;
            }

            if (SelectedWebPart != null) {
                EndWebPartEditing();
                if (SelectedWebPart != null) {
                    // The EditModeChange was cancelled
                    return;
                }
            }

            SetSelectedWebPart(webPart);

            Internals.CallOnEditModeChanged(webPart);

            OnSelectedWebPartChanged(new WebPartEventArgs(webPart));
        }

#if DEBUG
        /// <devdoc>
        /// Checks that the web parts in a zone are numbered sequentially.  This invariant
        /// should hold at the exit of AddWebPart, CloseWebPart, DeleteWebPart, MoveWebPart, and RegisterZone.
        /// </devdoc>
        private void CheckPartZoneIndexes(WebPartZoneBase zone) {
            ICollection parts = GetAllWebPartsForZone(zone);
            int index = 0;
            foreach (WebPart part in parts) {
                if (part.ZoneIndex != index) {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    builder.Append("Title\tZone\tZoneIndex");
                    foreach (WebPart part2 in Controls) {
                        string zoneTitle = (part2.Zone == null) ? "null" : part2.Zone.DisplayTitle;
                        builder.Append(part2.DisplayTitle + "\t" + zoneTitle + "\t" + part2.ZoneIndex);
                    }
                    Debug.Assert(false, builder.ToString());
                    return;
                }
                index++;
            }
        }
#endif // DEBUG

        protected virtual bool CheckRenderClientScript() {
            bool renderClientScript = false;

            if (EnableClientScript && Page != null) {
                HttpBrowserCapabilities browserCaps = Page.Request.Browser;

                // Win32 IE5.5+ and JScript 5.5+
                if (browserCaps.Win32 && (browserCaps.MSDomVersion.CompareTo(new Version(5, 5)) >= 0)) {
                    renderClientScript = true;
                }
            }

            return renderClientScript;
        }

        // When a Zone is deleted, any web parts in that zone should move to the page catalog.
        // VSWhidbey 77708
        private void CloseOrphanedParts() {
            // PERF: Use Controls instead of WebParts property, to avoid creating another collection
            if (HasControls()) {
                try {
                    // CloseOrphanedParts() is called as a result of no user action, so the events
                    // should not be cancellable. (VSWhidbey 516012)
                    _allowEventCancellation = false;
                    foreach (WebPart part in Controls) {
                        if (part.IsOrphaned) {
                            CloseWebPart(part);
                        }
                    }
                }
                finally {
                    _allowEventCancellation = true;
                }
            }
        }

        public bool CanConnectWebParts(WebPart provider, ProviderConnectionPoint providerConnectionPoint,
                                       WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint) {
            return CanConnectWebParts(provider, providerConnectionPoint, consumer, consumerConnectionPoint, null);
        }

        public virtual bool CanConnectWebParts(WebPart provider, ProviderConnectionPoint providerConnectionPoint,
                                               WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint,
                                               WebPartTransformer transformer) {
            return CanConnectWebPartsCore(provider, providerConnectionPoint, consumer, consumerConnectionPoint,
                                          transformer, false);
        }

        private bool CanConnectWebPartsCore(WebPart provider, ProviderConnectionPoint providerConnectionPoint,
                                            WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint,
                                            WebPartTransformer transformer, bool throwOnError) {
            if (!Personalization.IsModifiable) {
                if (throwOnError) {
                    // Will throw appropriate exception
                    Personalization.EnsureEnabled(/* ensureModifiable */ true);
                }
                else {
                    return false;
                }
            }

            if (provider == null) {
                throw new ArgumentNullException("provider");
            }
            if (!Controls.Contains(provider)) {
                throw new ArgumentException(SR.GetString(SR.UnknownWebPart), "provider");
            }

            if (consumer == null) {
                throw new ArgumentNullException("consumer");
            }
            if (!Controls.Contains(consumer)) {
                throw new ArgumentException(SR.GetString(SR.UnknownWebPart), "consumer");
            }

            if (providerConnectionPoint == null) {
                throw new ArgumentNullException("providerConnectionPoint");
            }
            if (consumerConnectionPoint == null) {
                throw new ArgumentNullException("consumerConnectionPoint");
            }

            Control providerControl = provider.ToControl();
            Control consumerControl = consumer.ToControl();

            if (providerConnectionPoint.ControlType != providerControl.GetType()) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_InvalidConnectionPoint), "providerConnectionPoint");
            }
            if (consumerConnectionPoint.ControlType != consumerControl.GetType()) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_InvalidConnectionPoint), "consumerConnectionPoint");
            }

            if (provider == consumer) {
                if (throwOnError) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPartManager_CantConnectToSelf));
                }
                else {
                    return false;
                }
            }

            if (provider.IsClosed) {
                if (throwOnError) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPartManager_CantConnectClosed, provider.ID));
                }
                else {
                    return false;
                }
            }

            if (consumer.IsClosed) {
                if (throwOnError) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPartManager_CantConnectClosed, consumer.ID));
                }
                else {
                    return false;
                }
            }

            if (!providerConnectionPoint.GetEnabled(providerControl)) {
                if (throwOnError) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPartConnection_DisabledConnectionPoint, providerConnectionPoint.ID, provider.ID));
                }
                else {
                    return false;
                }
            }

            if (!consumerConnectionPoint.GetEnabled(consumerControl)) {
                if (throwOnError) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPartConnection_DisabledConnectionPoint, consumerConnectionPoint.ID, consumer.ID));
                }
                else {
                    return false;
                }
            }

            // Check AllowsMultipleConnections on each ConnectionPoint
            if (!providerConnectionPoint.AllowsMultipleConnections) {
                foreach (WebPartConnection c in Connections) {
                    if (c.Provider == provider && c.ProviderConnectionPoint == providerConnectionPoint) {
                        if (throwOnError) {
                            throw new InvalidOperationException(SR.GetString(SR.WebPartConnection_Duplicate, providerConnectionPoint.ID, provider.ID));
                        }
                        else {
                            return false;
                        }
                    }
                }
            }

            if (!consumerConnectionPoint.AllowsMultipleConnections) {
                foreach (WebPartConnection c in Connections) {
                    if (c.Consumer == consumer && c.ConsumerConnectionPoint == consumerConnectionPoint) {
                        if (throwOnError) {
                            throw new InvalidOperationException(SR.GetString(SR.WebPartConnection_Duplicate, consumerConnectionPoint.ID, consumer.ID));
                        }
                        else {
                            return false;
                        }
                    }
                }
            }

            if (transformer == null) {
                if (providerConnectionPoint.InterfaceType != consumerConnectionPoint.InterfaceType) {
                    if (throwOnError) {
                        throw new InvalidOperationException(SR.GetString(SR.WebPartConnection_NoCommonInterface,
                            new string[] {providerConnectionPoint.DisplayName, provider.ID,
                                consumerConnectionPoint.DisplayName, consumer.ID}));
                    }
                    else {
                        return false;
                    }
                }

                ConnectionInterfaceCollection secondaryInterfaces = providerConnectionPoint.GetSecondaryInterfaces(providerControl);
                if (!consumerConnectionPoint.SupportsConnection(consumerControl, secondaryInterfaces)) {
                    if (throwOnError) {
                        throw new InvalidOperationException(SR.GetString(SR.WebPartConnection_IncompatibleSecondaryInterfaces, new string[] {
                                consumerConnectionPoint.DisplayName, consumer.ID,
                                providerConnectionPoint.DisplayName, provider.ID}));
                    }
                    else {
                        return false;
                    }
                }
            }
            else {
                Type transformerType = transformer.GetType();

                if (!AvailableTransformers.Contains(transformerType)) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPartConnection_TransformerNotAvailable, transformerType.FullName));
                }

                // Check matching interfaces on connection points and transformer attribute.
                // Note that we require the connection interfaces to match exactly.  We do not match
                // a derived interface type.  This is because we want to simplify the interface matching
                // algorithm when transformers are involved.  If we allowed derived interfaces to match,
                // then we would to take into account the "closest" match if multiple transformers
                // have compatible interfaces.
                Type transformerConsumerType = WebPartTransformerAttribute.GetConsumerType(transformerType);
                Type transformerProviderType = WebPartTransformerAttribute.GetProviderType(transformerType);
                if (providerConnectionPoint.InterfaceType != transformerConsumerType) {
                    if (throwOnError) {
                        throw new InvalidOperationException(SR.GetString(SR.WebPartConnection_IncompatibleProviderTransformer,
                            providerConnectionPoint.DisplayName, provider.ID, transformerType.FullName));
                    }
                    else {
                        return false;
                    }
                }
                if (transformerProviderType != consumerConnectionPoint.InterfaceType) {
                    if (throwOnError) {
                        throw new InvalidOperationException(SR.GetString(SR.WebPartConnection_IncompatibleConsumerTransformer,
                            transformerType.FullName, consumerConnectionPoint.DisplayName, consumer.ID));
                    }
                    else {
                        return false;
                    }
                }

                // A transformer never provides any secondary interfaces
                if (!consumerConnectionPoint.SupportsConnection(consumerControl, ConnectionInterfaceCollection.Empty)) {
                    if (throwOnError) {
                        throw new InvalidOperationException(SR.GetString(SR.WebPartConnection_ConsumerRequiresSecondaryInterfaces,
                            consumerConnectionPoint.DisplayName, consumer.ID));
                    }
                    else {
                        return false;
                    }
                }

            }

            return true;
        }

        public void CloseWebPart(WebPart webPart) {
            CloseOrDeleteWebPart(webPart, /* delete */ false);
        }

        private void CloseOrDeleteWebPart(WebPart webPart, bool delete) {
            Personalization.EnsureEnabled(/* ensureModifiable */ true);

            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            if (!Controls.Contains(webPart)) {
                throw new ArgumentException(SR.GetString(SR.UnknownWebPart), "webPart");
            }

            if (!delete && webPart.IsClosed) {
                // Throw an exception instead of just returning.  If the shared user and per user close
                // a WebPart at the same time, then the WebPartZoneBase should not call CloseWebPart
                // if the WebPart is now closed.
                throw new ArgumentException(SR.GetString(SR.WebPartManager_AlreadyClosed), "webPart");
            }

            if (delete) {
                if (webPart.IsStatic) {
                    // Can't delete static parts
                    throw new ArgumentException(SR.GetString(SR.WebPartManager_CantDeleteStatic), "webPart");
                }
                else if (webPart.IsShared && (Personalization.Scope == PersonalizationScope.User)) {
                    // Can't delete shared part in user scope
                    throw new ArgumentException(SR.GetString(SR.WebPartManager_CantDeleteSharedInUserScope), "webPart");
                }
            }

            WebPartCancelEventArgs ce = new WebPartCancelEventArgs(webPart);
            if (delete) {
                OnWebPartDeleting(ce);
            }
            else {
                OnWebPartClosing(ce);
            }
            if (_allowEventCancellation && ce.Cancel) {
                return;
            }

            if ((DisplayMode == ConnectDisplayMode) && (webPart == SelectedWebPart)) {
                EndWebPartConnecting();
                if (SelectedWebPart != null) {
                    // The ConnectModeChange was cancelled
                    return;
                }
            }

            // VSWhidbey 77768
            if ((DisplayMode == EditDisplayMode) && (webPart == SelectedWebPart)) {
                EndWebPartEditing();
                if (SelectedWebPart != null) {
                    // The EditModeChange was cancelled
                    return;
                }
            }

            if (delete) {
                Internals.CallOnDeleting(webPart);
            }
            else {
                Internals.CallOnClosing(webPart);
            }

#if DEBUG
            WebPartZoneBase zone = webPart.Zone;
#endif

            // If we are deleting a closed WebPart, it has already been removed from
            // its Zone, so there is no need to do it again.
            if (!webPart.IsClosed) {
                RemoveWebPartFromZone(webPart);
            }

            DisconnectWebPart(webPart);

            if (delete) {
                Internals.RemoveWebPart(webPart);

                // Raise the WebPartDeleted event after changing the WebPart properties
                // The WebPartDeleting event is raised before changing the WebPart properties
                OnWebPartDeleted(new WebPartEventArgs(webPart));
            }
            else {
                // Raise the WebPartClosed event after changing the WebPart properties
                // The WebPartClosing event is raised before changing the WebPart properties
                OnWebPartClosed(new WebPartEventArgs(webPart));
            }

#if DEBUG
            if (zone != null) {
                CheckPartZoneIndexes(zone);
            }
#endif
        }

        private WebPartConnection[] ConnectionsToActivate() {
            // PERF: We could implement this with a sorted list to simplify the code

            ArrayList connectionsToActivate = new ArrayList();

            // Contains the connection IDs we have already seen
            HybridDictionary connectionIDs = new HybridDictionary(true /* caseInsensitive */);

            WebPartConnection[] connections = new WebPartConnection[StaticConnections.Count + DynamicConnections.Count];
            StaticConnections.CopyTo(connections, 0);
            DynamicConnections.CopyTo(connections, StaticConnections.Count);
            foreach (WebPartConnection connection in connections) {
                ConnectionsToActivateHelper(connection, connectionIDs, connectionsToActivate);
            }

            // Check unshared connections for conflicts with shared connections
            // Maybe this should only be done in user scope
            WebPartConnection[] connectionsToActivateArray = (WebPartConnection[])connectionsToActivate.ToArray(typeof(WebPartConnection));
            foreach (WebPartConnection connection in connectionsToActivateArray) {
                if (connection.IsShared) {
                    continue;
                }

                ArrayList connectionsToDelete = new ArrayList();
                foreach (WebPartConnection otherConnection in connectionsToActivate) {
                    if (connection == otherConnection) {
                        continue;
                    }

                    if (otherConnection.IsShared && connection.ConflictsWith(otherConnection)) {
                        // Delete shared connection.
                        connectionsToDelete.Add(otherConnection);
                    }
                }

                foreach (WebPartConnection connectionToDelete in connectionsToDelete) {
                    DisconnectWebParts(connectionToDelete);
                    connectionsToActivate.Remove(connectionToDelete);
                }
            }

            // Check shared, nonstatic connections for conflicts with static connections
            connectionsToActivateArray = (WebPartConnection[])connectionsToActivate.ToArray(typeof(WebPartConnection));
            foreach (WebPartConnection connection in connectionsToActivateArray) {
                if (!connection.IsShared || connection.IsStatic) {
                    continue;
                }

                ArrayList connectionsToDelete = new ArrayList();
                foreach (WebPartConnection otherConnection in connectionsToActivate) {
                    if (connection == otherConnection) {
                        continue;
                    }

                    if (otherConnection.IsStatic && connection.ConflictsWith(otherConnection)) {
                        // Delete static connection.
                        connectionsToDelete.Add(otherConnection);
                    }
                }

                foreach (WebPartConnection connectionToDelete in connectionsToDelete) {
                    DisconnectWebParts(connectionToDelete);
                    connectionsToActivate.Remove(connectionToDelete);
                }
            }

            // Check all remaining connections for conflicts.  Any conflicts at this stage will
            // cause an error to be rendered in the consumer WebPart, and the conflicting connections
            // will not be activated.
            ArrayList finalConnectionsToActivate = new ArrayList();
            foreach (WebPartConnection connection in connectionsToActivate) {
                bool hasConflict = false;

                foreach (WebPartConnection otherConnection in connectionsToActivate) {
                    if (connection == otherConnection) {
                        continue;
                    }

                    if (connection.ConflictsWithConsumer(otherConnection)) {
                        connection.Consumer.SetConnectErrorMessage(SR.GetString(SR.WebPartConnection_Duplicate, connection.ConsumerConnectionPoint.DisplayName,
                                connection.Consumer.DisplayTitle));
                        hasConflict = true;
                    }

                    if (connection.ConflictsWithProvider(otherConnection)) {
                        connection.Consumer.SetConnectErrorMessage(SR.GetString(SR.WebPartConnection_Duplicate, connection.ProviderConnectionPoint.DisplayName,
                                connection.Provider.DisplayTitle));
                        hasConflict = true;
                    }
                }

                if (!hasConflict) {
                    finalConnectionsToActivate.Add(connection);
                }
            }

            // Don't allow the user to modify the StaticConnections collection after its connections have
            // been activated.  Use property instead of field to force creation of collection.
            StaticConnections.SetReadOnly(SR.WebPartManager_StaticConnectionsReadOnly);

            // The user can't directly change the DynamicConnections property since it is internal.
            // Make it read-only in case we have a 

            DynamicConnections.SetReadOnly(SR.WebPartManager_DynamicConnectionsReadOnly);

            return (WebPartConnection[])finalConnectionsToActivate.ToArray(typeof(WebPartConnection));
        }

        // If we think we should activate the connection, adds it to the dictionary under the key
        // for its provider and consumer connection points.
        private void ConnectionsToActivateHelper(WebPartConnection connection, IDictionary connectionIDs,
                                                 ArrayList connectionsToActivate) {
            string connectionID = connection.ID;

            if (String.IsNullOrEmpty(connectionID)) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartConnection_NoID));
            }

            if (connectionIDs.Contains(connectionID)) {
                throw new InvalidOperationException(
                    SR.GetString(SR.WebPartManager_DuplicateConnectionID, connectionID));
            }
            connectionIDs.Add(connectionID, null);

            if (connection.Deleted) {
                return;
            }

            WebPart providerWebPart = connection.Provider;
            if (providerWebPart == null) {
                if (connection.IsStatic) {
                    // throw an exception, to alert the developer that his static connection is invalid
                    throw new InvalidOperationException(SR.GetString(SR.WebPartConnection_NoProvider, connection.ProviderID));
                }
                else {
                    // Silently delete the connection, since this is a valid runtime scenario.
                    // A connected web part may have been deleted.
                    DisconnectWebParts(connection);
                    return;
                }
            }

            WebPart consumerWebPart = connection.Consumer;
            if (consumerWebPart == null) {
                if (connection.IsStatic) {
                    // throw an exception, to alert the developer that his static connection is invalid
                    throw new InvalidOperationException(SR.GetString(SR.WebPartConnection_NoConsumer, connection.ConsumerID));
                }
                else {
                    // Silently delete the connection, since this is a valid runtime scenario.
                    // A connected web part may have been deleted.
                    DisconnectWebParts(connection);
                    return;
                }
            }

            // Do not activate connections involving ProxyWebParts
            if (providerWebPart is ProxyWebPart || consumerWebPart is ProxyWebPart) {
                return;
            }

            Control providerControl = providerWebPart.ToControl();
            Control consumerControl = consumerWebPart.ToControl();

            // Cannot connect a WebPart to itself
            if (providerControl == consumerControl) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartManager_CantConnectToSelf));
            }

            ProviderConnectionPoint providerConnectionPoint = connection.ProviderConnectionPoint;
            if (providerConnectionPoint == null) {
                consumerWebPart.SetConnectErrorMessage(SR.GetString(SR.WebPartConnection_NoProviderConnectionPoint, connection.ProviderConnectionPointID,
                    providerWebPart.DisplayTitle));
                return;
            }
            // Don't need to check that providerConnectionPoint is enabled, since this will be checked
            // in WebPartConnection.Activate().

            ConsumerConnectionPoint consumerConnectionPoint = connection.ConsumerConnectionPoint;
            if (consumerConnectionPoint == null) {
                consumerWebPart.SetConnectErrorMessage(SR.GetString(SR.WebPartConnection_NoConsumerConnectionPoint, connection.ConsumerConnectionPointID,
                    consumerWebPart.DisplayTitle));
                return;
            }
            // Don't need to check that consumer ConnectionPoint is enabled, since this will be checked
            // in WebPartConnection.Activate().

            connectionsToActivate.Add(connection);
        }

        public WebPartConnection ConnectWebParts(WebPart provider, ProviderConnectionPoint providerConnectionPoint,
                                                 WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint) {
            return ConnectWebParts(provider, providerConnectionPoint, consumer, consumerConnectionPoint, null);
        }

        public virtual WebPartConnection ConnectWebParts(WebPart provider, ProviderConnectionPoint providerConnectionPoint,
                                                         WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint,
                                                         WebPartTransformer transformer) {
            CanConnectWebPartsCore(provider, providerConnectionPoint, consumer, consumerConnectionPoint,
                                   transformer, /*throwOnError*/ true);

            if (DynamicConnections.IsReadOnly) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartManager_ConnectTooLate));
            }

            WebPartConnectionsCancelEventArgs ce = new WebPartConnectionsCancelEventArgs(
                provider, providerConnectionPoint, consumer, consumerConnectionPoint);
            OnWebPartsConnecting(ce);
            if (_allowEventCancellation && ce.Cancel) {
                return null;
            }

            Control providerControl = provider.ToControl();
            Control consumerControl = consumer.ToControl();

            WebPartConnection connection = new WebPartConnection();
            connection.ID = CreateDynamicConnectionID();
            connection.ProviderID = providerControl.ID;
            connection.ConsumerID = consumerControl.ID;
            connection.ProviderConnectionPointID = providerConnectionPoint.ID;
            connection.ConsumerConnectionPointID = consumerConnectionPoint.ID;

            if (transformer != null) {
                Internals.SetTransformer(connection, transformer);
            }

            Internals.SetIsShared(connection, Personalization.Scope == PersonalizationScope.Shared);
            Internals.SetIsStatic(connection, false);

            DynamicConnections.Add(connection);
            _hasDataChanged = true;

            OnWebPartsConnected(new WebPartConnectionsEventArgs(provider, providerConnectionPoint,
                                                                consumer, consumerConnectionPoint, connection));

            return connection;
        }

        // Returns a copy of the WebPart, with all the properties reset to their default value.
        // If the WebPart is a GenericWebPart, returns a copy of the GenericWebPart and a copy of the
        // ChildControl inside the GenericWebPart.  The ID of the new WebPart and ChildControl should
        // be set to a value obtained from CreateDynamicWebPartID.
        // Virtual because a derived WebPartManager will deserialize a WebPart from XML in this method.
        protected virtual WebPart CopyWebPart(WebPart webPart) {
            WebPart newWebPart;

            GenericWebPart genericWebPart = webPart as GenericWebPart;
            if (genericWebPart != null) {
                Control childControl = genericWebPart.ChildControl;
                VerifyType(childControl);
                Type childControlType = childControl.GetType();
                Control newChildControl = (Control)Internals.CreateObjectFromType(childControlType);
                newChildControl.ID = CreateDynamicWebPartID(childControlType);

                newWebPart = CreateWebPart(newChildControl);
            }
            else {
                VerifyType(webPart);
                newWebPart = (WebPart)Internals.CreateObjectFromType(webPart.GetType());
            }

            newWebPart.ID = CreateDynamicWebPartID(webPart.GetType());

            return newWebPart;
        }

        protected virtual TransformerTypeCollection CreateAvailableTransformers() {
            TransformerTypeCollection availableTransformers = new TransformerTypeCollection();

            WebPartsSection configSection = RuntimeConfig.GetConfig().WebParts;

            IDictionary transformers = configSection.Transformers.GetTransformerEntries();

            foreach (Type type in transformers.Values) {
                availableTransformers.Add(type);
            }

            return availableTransformers;
        }

        // Returns an array of ICollection objects.  The first is the ConsumerConnectionPoints, the
        // second is the ProviderConnectionPoints.
        private static ICollection[] CreateConnectionPoints(Type type) {
            ArrayList consumerConnectionPoints = new ArrayList();
            ArrayList providerConnectionPoints = new ArrayList();

            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (MethodInfo method in methods) {

                // Create consumer connection points
                object[] consumerAttributes = method.GetCustomAttributes(typeof(ConnectionConsumerAttribute), true);
                // ConnectionConsumerAttribute.AllowMultiple is false
                Debug.Assert(consumerAttributes.Length == 0 || consumerAttributes.Length == 1);
                if (consumerAttributes.Length == 1) {
                    // Consumer signature: method is public, return type is void, takes one parameter
                    ParameterInfo[] parameters = method.GetParameters();
                    Type parameterType = null;
                    if (parameters.Length == 1) {
                        parameterType = parameters[0].ParameterType;
                    }

                    if (method.IsPublic && method.ReturnType == typeof(void) && parameterType != null) {
                        ConnectionConsumerAttribute attribute = consumerAttributes[0] as ConnectionConsumerAttribute;
                        String displayName = attribute.DisplayName;
                        String id = attribute.ID;
                        Type connectionPointType = attribute.ConnectionPointType;
                        bool allowsMultipleConnections = attribute.AllowsMultipleConnections;
                        ConsumerConnectionPoint connectionPoint;
                        if (connectionPointType == null) {
                            connectionPoint = new ConsumerConnectionPoint(method, parameterType, type,
                                                                          displayName, id, allowsMultipleConnections);
                        }
                        else {
                            // The ConnectionPointType is validated in the attribute property getter
                            Object[] args = new Object[] { method, parameterType, type, displayName, id, allowsMultipleConnections };
                            connectionPoint = (ConsumerConnectionPoint)Activator.CreateInstance(connectionPointType, args);
                        }
                        consumerConnectionPoints.Add(connectionPoint);
                    }
                    else {
                        throw new InvalidOperationException(SR.GetString(SR.WebPartManager_InvalidConsumerSignature, method.Name, type.FullName));
                    }
                }

                // Create provider connection points
                object[] providerAttributes = method.GetCustomAttributes(typeof(ConnectionProviderAttribute), true);
                // ConnectionProviderAttribute.AllowMultiple is false
                Debug.Assert(providerAttributes.Length == 0 || providerAttributes.Length == 1);
                if (providerAttributes.Length == 1) {
                    // Provider signature: method is public, return type is an object, and takes no parameters
                    Type returnType = method.ReturnType;
                    if (method.IsPublic && returnType != typeof(void) && method.GetParameters().Length == 0) {
                        ConnectionProviderAttribute attribute = providerAttributes[0] as ConnectionProviderAttribute;
                        String displayName = attribute.DisplayName;
                        String id = attribute.ID;
                        Type connectionPointType = attribute.ConnectionPointType;
                        bool allowsMultipleConnections = attribute.AllowsMultipleConnections;
                        ProviderConnectionPoint connectionPoint;
                        if (connectionPointType == null) {
                            connectionPoint = new ProviderConnectionPoint(method, returnType, type,
                                                                          displayName, id, allowsMultipleConnections);
                        }
                        else {
                            // The ConnectionPointType is validated in the attribute property getter
                            Object[] args = new Object[] { method, returnType, type, displayName, id, allowsMultipleConnections };
                            connectionPoint = (ProviderConnectionPoint)Activator.CreateInstance(connectionPointType, args);
                        }
                        providerConnectionPoints.Add(connectionPoint);
                    }
                    else {
                        throw new InvalidOperationException(SR.GetString(SR.WebPartManager_InvalidProviderSignature, method.Name, type.FullName));
                    }
                }
            }

            return new ICollection[] { new ConsumerConnectionPointCollection(consumerConnectionPoints),
                new ProviderConnectionPointCollection(providerConnectionPoints) };
        }

        protected sealed override ControlCollection CreateControlCollection() {
            return new WebPartManagerControlCollection(this);
        }

        /// <devdoc>
        /// Can be overridden by derived types, to add additional display modes.  Display modes
        /// should be added in the order they are to appear in the page menu.
        /// </devdoc>
        protected virtual WebPartDisplayModeCollection CreateDisplayModes() {
            WebPartDisplayModeCollection displayModes = new WebPartDisplayModeCollection();

            displayModes.Add(BrowseDisplayMode);
            displayModes.Add(CatalogDisplayMode);
            displayModes.Add(DesignDisplayMode);
            displayModes.Add(EditDisplayMode);
            displayModes.Add(ConnectDisplayMode);

            return displayModes;
        }

        private string CreateDisplayTitle(string title, WebPart webPart, int count) {
            string displayTitle = title;

            if (webPart.Hidden) {
                displayTitle = SR.GetString(SR.WebPart_HiddenFormatString, displayTitle);
            }

            if (webPart is ErrorWebPart) {
                displayTitle = SR.GetString(SR.WebPart_ErrorFormatString, displayTitle);
            }

            if (count != 0) {
                if (count < displayTitleSuffix.Length) {
                    displayTitle += displayTitleSuffix[count];
                }
                else {
                    displayTitle += " [" + count.ToString(CultureInfo.CurrentCulture) + "]";
                }
            }

            return displayTitle;
        }

        private IDictionary CreateDisplayTitles() {
            Hashtable displayTitles = new Hashtable();

            Hashtable titles = new Hashtable();
            foreach (WebPart part in Controls) {
                string title = part.Title;
                if (String.IsNullOrEmpty(title)) {
                    title = SR.GetString(SR.Part_Untitled);
                }

                if (part is UnauthorizedWebPart) {
                    displayTitles[part] = title;
                    continue;
                }

                ArrayList parts = (ArrayList)titles[title];
                if (parts == null) {
                    parts = new ArrayList();
                    titles[title] = parts;
                    displayTitles[part] = CreateDisplayTitle(title, part, 0);
                }
                else {
                    int count = parts.Count;
                    if (count == 1) {
                        WebPart firstPart = (WebPart)parts[0];
                        displayTitles[firstPart] = CreateDisplayTitle(title, firstPart, 1);
                    }
                    displayTitles[part] = CreateDisplayTitle(title, part, count + 1);
                }

                parts.Add(part);
            }

            return displayTitles;
        }

        protected virtual string CreateDynamicConnectionID() {
            Debug.Assert(Personalization.IsModifiable);
            // 
            int guidHash = Math.Abs(Guid.NewGuid().GetHashCode());
            return DynamicConnectionIDPrefix + guidHash.ToString(CultureInfo.InvariantCulture);
        }

        protected virtual string CreateDynamicWebPartID(Type webPartType) {
            if (webPartType == null) {
                throw new ArgumentNullException("webPartType");
            }

            Debug.Assert(Personalization.IsModifiable);

            // 

            int guidHash = Math.Abs(Guid.NewGuid().GetHashCode());
            string id = DynamicWebPartIDPrefix + guidHash.ToString(CultureInfo.InvariantCulture);

            if (Page != null && Page.Trace.IsEnabled) {
                id += webPartType.Name;
            }

            return id;
        }

        protected virtual ErrorWebPart CreateErrorWebPart(string originalID, string originalTypeName,
                                                          string originalPath, string genericWebPartID,
                                                          string errorMessage) {
            ErrorWebPart errorWebPart = new ErrorWebPart(originalID, originalTypeName, originalPath, genericWebPartID);
            errorWebPart.ErrorMessage = errorMessage;
            return errorWebPart;
        }

        /// <devdoc>
        /// </devdoc>
        protected virtual WebPartPersonalization CreatePersonalization() {
            return new WebPartPersonalization(this);
        }

        /// <devdoc>
        /// Wraps the control in a GenericWebPart, and returns the GenericWebPart.  Virtual so it can be
        /// overridden to use a derived type of GenericWebPart instead.  Needs to be public so it can
        /// be called by the page developer.
        /// </devdoc>
        public virtual GenericWebPart CreateWebPart(Control control) {
            return CreateWebPartStatic(control);
        }

        // Called by other WebParts classes to create a GenericWebPart, if they do not have
        // a reference to a WebPartManager (i.e. at design time).  This method centralizes
        // the creation of GenericWebParts.
        internal static GenericWebPart CreateWebPartStatic(Control control) {
            GenericWebPart genericWebPart = new GenericWebPart(control);
            
            // The ChildControl should be added to the GenericWebPart.Controls collection when CreateWebPart()
            // is called, instead of waiting until the GenericWebPart.Controls collection is accessed.
            // This is necessary since the caller has a direct reference to the ChildControl, and may
            // perform operations on the ChildControl that assume the ChildControl is parented.
            // (VSWhidbey 498039)
            genericWebPart.CreateChildControls();

            return genericWebPart;
        }

        public void DeleteWebPart(WebPart webPart) {
            CloseOrDeleteWebPart(webPart, /* delete */ true);
        }

        // Disconnects all connections involving the Web Part
        protected virtual void DisconnectWebPart(WebPart webPart) {
            try {
                // We cannot allow any of the WebPartsDisconnecting events to be cancelled, since we may have already
                // disconnected some connections before we hit the one that needs to be cancelled. (VSWhidbey 516012)
                _allowEventCancellation = false;
                foreach (WebPartConnection connection in Connections) {
                    if (connection.Provider == webPart || connection.Consumer == webPart) {
                        DisconnectWebParts(connection);
                    }
                }
            }
            finally {
                _allowEventCancellation = true;
            }
        }

        public virtual void DisconnectWebParts(WebPartConnection connection) {
            Personalization.EnsureEnabled(/* ensureModifiable */ true);

            if (connection == null) {
                throw new ArgumentNullException("connection");
            }

            Debug.Assert(!(StaticConnections.Contains(connection) && DynamicConnections.Contains(connection)));

            WebPart provider = connection.Provider;
            ProviderConnectionPoint providerConnectionPoint = connection.ProviderConnectionPoint;
            WebPart consumer = connection.Consumer;
            ConsumerConnectionPoint consumerConnectionPoint = connection.ConsumerConnectionPoint;

            WebPartConnectionsCancelEventArgs ce = new WebPartConnectionsCancelEventArgs(
                provider, providerConnectionPoint, consumer, consumerConnectionPoint, connection);
            OnWebPartsDisconnecting(ce);
            if (_allowEventCancellation && ce.Cancel) {
                return;
            }

            WebPartConnectionsEventArgs eventArgs = new WebPartConnectionsEventArgs(
                provider, providerConnectionPoint, consumer, consumerConnectionPoint);

            if (StaticConnections.Contains(connection)) {
                if (StaticConnections.IsReadOnly) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPartManager_DisconnectTooLate));
                }
                if (Internals.ConnectionDeleted(connection)) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPartManager_AlreadyDisconnected));
                }
                Internals.DeleteConnection(connection);
                _hasDataChanged = true;
                OnWebPartsDisconnected(eventArgs);
            }
            else if (DynamicConnections.Contains(connection)) {
                if (DynamicConnections.IsReadOnly) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPartManager_DisconnectTooLate));
                }

                if (ShouldRemoveConnection(connection)) {
                    // Unshared dynamic connection should never be disabled
                    Debug.Assert(!Internals.ConnectionDeleted(connection));
                    DynamicConnections.Remove(connection);
                }
                else {
                    if (Internals.ConnectionDeleted(connection)) {
                        throw new InvalidOperationException(SR.GetString(SR.WebPartManager_AlreadyDisconnected));
                    }
                    Internals.DeleteConnection(connection);
                }
                _hasDataChanged = true;
                OnWebPartsDisconnected(eventArgs);
            }
            else {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_UnknownConnection), "connection");
            }
        }

        public virtual void EndWebPartConnecting() {
            Personalization.EnsureEnabled(/* ensureModifiable */ true);

            WebPart selectedWebPart = SelectedWebPart;

            if (selectedWebPart == null) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartManager_NoSelectedWebPartConnect));
            }

            WebPartCancelEventArgs ce = new WebPartCancelEventArgs(selectedWebPart);
            OnSelectedWebPartChanging(ce);

            if (_allowEventCancellation && ce.Cancel) {
                return;
            }

            SetSelectedWebPart(null);

            Internals.CallOnConnectModeChanged(selectedWebPart);

            // The EventArg should always contain the new SelectedWebPart, so it should contain null
            // when we are ending connecting.
            OnSelectedWebPartChanged(new WebPartEventArgs(null));
        }

        public virtual void EndWebPartEditing() {
            Personalization.EnsureEnabled(/* ensureModifiable */ true);

            WebPart selectedWebPart = SelectedWebPart;

            if (selectedWebPart == null) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartManager_NoSelectedWebPartEdit));
            }

            WebPartCancelEventArgs ce = new WebPartCancelEventArgs(selectedWebPart);
            OnSelectedWebPartChanging(ce);

            if (_allowEventCancellation && ce.Cancel) {
                return;
            }

            SetSelectedWebPart(null);

            Internals.CallOnEditModeChanged(selectedWebPart);

            // The EventArg should always contain the new SelectedWebPart, so it should contain null
            // when we are ending editing.
            OnSelectedWebPartChanged(new WebPartEventArgs(null));
        }

        public virtual void ExportWebPart(WebPart webPart, XmlWriter writer) {
//            Personalization.EnsureEnabled(/* ensureModifiable */ false);

            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            if (!Controls.Contains(webPart)) {
                throw new ArgumentException(SR.GetString(SR.UnknownWebPart), "webPart");
            }

            if (writer == null) {
                throw new ArgumentNullException("writer");
            }
            if (webPart.ExportMode == WebPartExportMode.None) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_PartNotExportable), "webPart");
            }
            bool excludeSensitive = (webPart.ExportMode == WebPartExportMode.NonSensitiveData &&
                !(Personalization.Scope == PersonalizationScope.Shared));

            // Write the root elements
            writer.WriteStartElement(ExportRootElement);
            writer.WriteStartElement(ExportPartElement);
            writer.WriteAttributeString(ExportPartNamespaceAttribute, ExportPartNamespaceValue);
            // Write metadata
            writer.WriteStartElement(ExportMetaDataElement);
            writer.WriteStartElement(ExportTypeElement);
            Control control = webPart.ToControl();
            UserControl userControl = control as UserControl;
            if (userControl != null) {
                writer.WriteAttributeString(ExportUserControlSrcAttribute, userControl.AppRelativeVirtualPath);
            }
            else {
                writer.WriteAttributeString(ExportTypeNameAttribute, WebPartUtil.SerializeType(control.GetType()));
            }
            writer.WriteEndElement(); //type
            writer.WriteElementString(ExportErrorMessageElement, webPart.ImportErrorMessage);
            writer.WriteEndElement(); //metadata
            // Write the data
            writer.WriteStartElement(ExportDataElement);
            // We get the personalization data for the current page personalization mode
            IDictionary propBag = PersonalizableAttribute.GetPersonalizablePropertyValues(webPart, PersonalizationScope.Shared, excludeSensitive);

            writer.WriteStartElement(ExportPropertiesElement);

            // Special case GenericWebPart
            GenericWebPart genericWebPart = webPart as GenericWebPart;

            if (genericWebPart != null) {

                // Export IPersonalizable user control data first
                ExportIPersonalizable(writer, control, excludeSensitive);

                IDictionary controlData = PersonalizableAttribute.GetPersonalizablePropertyValues(control,
                                                                                                  PersonalizationScope.Shared,
                                                                                                  excludeSensitive);
                ExportToWriter(controlData, writer);
                writer.WriteEndElement(); //properties
                writer.WriteStartElement(ExportGenericPartPropertiesElement);
                // Export IPersonalizable part data first
                ExportIPersonalizable(writer, webPart, excludeSensitive);
                ExportToWriter(propBag, writer);
            }
            else {
                // Export IPersonalizable part data first
                ExportIPersonalizable(writer, webPart, excludeSensitive);
                ExportToWriter(propBag, writer);
            }
            writer.WriteEndElement(); //properties or genericWebPartProperties
            writer.WriteEndElement(); //data
            writer.WriteEndElement(); //webpart
            writer.WriteEndElement(); //webparts
        }

        private void ExportIPersonalizable(XmlWriter writer, Control control, bool excludeSensitive) {
            IPersonalizable personalizableControl = control as IPersonalizable;
            if (personalizableControl != null) {
                PersonalizationDictionary personalizableData = new PersonalizationDictionary();
                personalizableControl.Save(personalizableData);
                if (personalizableData.Count > 0) {
                    writer.WriteStartElement(ExportIPersonalizableElement);
                    ExportToWriter(personalizableData, writer, /* isIPersonalizable */ true, excludeSensitive);
                    writer.WriteEndElement(); // ipersonalizable
                }
            }
        }

        private static void ExportProperty(XmlWriter writer, string name, string value, Type type,
            PersonalizationScope scope, bool isIPersonalizable) {

            writer.WriteStartElement(ExportPropertyElement);
            writer.WriteAttributeString(ExportPropertyNameAttribute, name);
            writer.WriteAttributeString(ExportPropertyTypeAttribute, GetExportName(type));
            if (isIPersonalizable) {
                writer.WriteAttributeString(ExportPropertyScopeAttribute, scope.ToString());
            }
            if (value == null) {
                writer.WriteAttributeString(ExportPropertyNullAttribute, "true");
            }
            else {
                writer.WriteString(value);
            }
            writer.WriteEndElement(); //property
        }

        private void ExportToWriter(IDictionary propBag, XmlWriter writer) {
            ExportToWriter(propBag, writer, false, false);
        }

        private void ExportToWriter(IDictionary propBag,
                                    XmlWriter writer,
                                    bool isIPersonalizable,
                                    bool excludeSensitive) {
            // We only honor excludeSensitive if isIpersonalizable is true.
            Debug.Assert((!excludeSensitive) || isIPersonalizable);

            // Work on each property in the persomalization data
            foreach(DictionaryEntry entry in propBag) {
                string name = (string)entry.Key;
                if (name == AuthorizationFilterName || name == ImportErrorMessageName) {
                    continue;
                }

                PropertyInfo pi = null;
                object val = null;
                Pair data = entry.Value as Pair;
                PersonalizationScope scope = PersonalizationScope.User;
                // We expect a pair if not exporting types
                // (which happens only for non-IPersonalizable data)
                if (isIPersonalizable == false && data != null) {
                    pi = (PropertyInfo)data.First;
                    val = data.Second;
                }
                else if (isIPersonalizable) {
                    PersonalizationEntry personalizationEntry = entry.Value as PersonalizationEntry;
                    if (personalizationEntry != null &&
                        (Personalization.Scope == PersonalizationScope.Shared ||
                        personalizationEntry.Scope == PersonalizationScope.User)) {

                        val = personalizationEntry.Value;
                        scope = personalizationEntry.Scope;
                    }
                    if (excludeSensitive && personalizationEntry.IsSensitive) {
                        continue;
                    }
                }
                // we get the type from the PropertyInfo if we have it, or from the value if it's not null, or we use object.
                Type valType = ((pi != null) ? pi.PropertyType : ((val != null) ? val.GetType() : typeof(object)));

                string exportString;
                if (ShouldExportProperty(pi, valType, val, out exportString)) {
                    ExportProperty(writer, name, exportString, valType, scope, isIPersonalizable);
                }
            }
        }

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override void Focus() {
            throw new NotSupportedException(SR.GetString(SR.NoFocusSupport, this.GetType().Name));
        }

        /// <devdoc>
        /// Returns all the web parts in a zone, excluding closed web parts.
        /// Since this is only a private method, return an IList instead of a WebPartCollection
        /// for better performance.
        /// </devdoc>
        private IList GetAllWebPartsForZone(WebPartZoneBase zone) {
            if (_partsForZone == null) {
                _partsForZone = new HybridDictionary(true /* caseInsensitive */);
                foreach (WebPart part in Controls) {
                    if (!part.IsClosed) {
                        string zoneID = Internals.GetZoneID(part);
                        Debug.Assert(!String.IsNullOrEmpty(zoneID));
                        if (!String.IsNullOrEmpty(zoneID)) {
                            SortedList partsForZone = (SortedList)_partsForZone[zoneID];
                            if (partsForZone == null) {
                                partsForZone = new SortedList(new WebPart.ZoneIndexComparer());
                                _partsForZone[zoneID] = partsForZone;
                            }
                            partsForZone.Add(part, null);
                        }
                    }
                }
            }

            SortedList parts = (SortedList)_partsForZone[zone.ID];
            if (parts == null) {
                parts = new SortedList();
            }

            return parts.GetKeyList();
        }

        private static ICollection[] GetConnectionPoints(Type type) {
            if (ConnectionPointsCache == null) {
                // I don't think there is a race condition here.  Even if multiple threads enter this block
                // at the same time, the worst thing that can happen is that the ConnectionPointsCache gets
                // replaced by a new Hashtable(), and existing entries will need to be recomputed.
                // There is no way for the ConnectionPointsCache to become null.
                ConnectionPointsCache = Hashtable.Synchronized(new Hashtable());
            }

            // DevDiv Bugs 38677: Cache by culture and type as it may vary by culture within this app
            ConnectionPointKey connectionPointKey = new ConnectionPointKey(type, CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);

            ICollection[] connectionPoints = (ICollection[])ConnectionPointsCache[connectionPointKey];
            if (connectionPoints == null) {
                connectionPoints = CreateConnectionPoints(type);
                ConnectionPointsCache[connectionPointKey] = connectionPoints;
            }

            return connectionPoints;
        }

        internal ConsumerConnectionPoint GetConsumerConnectionPoint(WebPart webPart, string connectionPointID) {
            ConsumerConnectionPointCollection points = GetConsumerConnectionPoints(webPart);
            if (points != null && points.Count > 0) {
                return points[connectionPointID];
            }
            else {
                return null;
            }
        }

        public virtual ConsumerConnectionPointCollection GetConsumerConnectionPoints(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            // Do not check that Controls.Contains(webPart), since this may be called on a WebPart
            // outside of a Zone.  Also, this method shouldn't really care whether the WebPart is
            // inside the WebPartManager.

            return GetConsumerConnectionPoints(webPart.ToControl().GetType());
        }

        private static ConsumerConnectionPointCollection GetConsumerConnectionPoints(Type type) {
            ICollection[] connectionPoints = GetConnectionPoints(type);
            return (ConsumerConnectionPointCollection)connectionPoints[0];
        }

        public static WebPartManager GetCurrentWebPartManager(Page page) {
            if (page == null) {
                throw new ArgumentNullException("page");
            }

            return page.Items[typeof(WebPartManager)] as WebPartManager;
        }

        // Before PreRender, return String.Empty.
        // On first call to this function after PreRender, compute DisplayTitle for all WebParts
        // and save it in a dictionary.  WebPart.DisplayTitle is nonvirtual and calls this method every time.
        // A derived WebPartManager can override this method to compute and store DisplayTitle any way it
        // sees fit.  It could compute the values sooner than PreRender.
        protected internal virtual string GetDisplayTitle(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            if (!Controls.Contains(webPart)) {
                throw new ArgumentException(SR.GetString(SR.UnknownWebPart), "webPart");
            }

            if (!_allowCreateDisplayTitles) {
                return String.Empty;
            }

            if (_displayTitles == null) {
                _displayTitles = CreateDisplayTitles();
            }

            string displayTitle = (string)_displayTitles[webPart];
            Debug.Assert(!String.IsNullOrEmpty(displayTitle));
            return displayTitle;
        }

        private static ICollection GetEnabledConnectionPoints(ICollection connectionPoints, WebPart webPart) {
            Control control = webPart.ToControl();
            ArrayList enabledPoints = new ArrayList();
            foreach (ConnectionPoint point in connectionPoints) {
                if (point.GetEnabled(control)) {
                    enabledPoints.Add(point);
                }
            }
            return enabledPoints;
        }

        internal ConsumerConnectionPointCollection GetEnabledConsumerConnectionPoints(WebPart webPart) {
            ICollection enabledPoints = GetEnabledConnectionPoints(GetConsumerConnectionPoints(webPart), webPart);
            return new ConsumerConnectionPointCollection(enabledPoints);
        }

        internal ProviderConnectionPointCollection GetEnabledProviderConnectionPoints(WebPart webPart) {
            ICollection enabledPoints = GetEnabledConnectionPoints(GetProviderConnectionPoints(webPart), webPart);
            return new ProviderConnectionPointCollection(enabledPoints);
        }

        public string GetExportUrl(WebPart webPart) {
            string personalizationScope =
                (Personalization.Scope == PersonalizationScope.Shared) ? "&scope=shared" : String.Empty;
            string queryString = Page.Request.QueryStringText;

            return Page.Request.FilePath + "?" + Page.WebPartExportID + "=true&webPart=" +
                HttpUtility.UrlEncode(webPart.ID) +
                (!String.IsNullOrEmpty(queryString) ?
                    "&query=" + HttpUtility.UrlEncode(queryString) :
                    String.Empty) +
                personalizationScope;
        }

        private static Type GetExportType(string name) {
            switch (name) {
                case ExportTypeString:
                    return typeof(string);
                case ExportTypeInt:
                    return typeof(int);
                case ExportTypeBool:
                    return typeof(bool);
                case ExportTypeDouble:
                    return typeof(double);
                case ExportTypeSingle:
                    return typeof(Single);
                case ExportTypeDateTime:
                    return typeof(DateTime);
                case ExportTypeColor:
                    return typeof(Color);
                case ExportTypeUnit:
                    return typeof(Unit);
                case ExportTypeFontSize:
                    return typeof(FontSize);
                case ExportTypeDirection:
                    return typeof(ContentDirection);
                case ExportTypeHelpMode:
                    return typeof(WebPartHelpMode);
                case ExportTypeChromeState:
                    return typeof(PartChromeState);
                case ExportTypeChromeType:
                    return typeof(PartChromeType);
                case ExportTypeExportMode:
                    return typeof(WebPartExportMode);
                case ExportTypeObject:
                    return typeof(object);
                default:
                    return WebPartUtil.DeserializeType(name, false);
            }
        }

        private static string GetExportName(Type type) {
            if (type == typeof(string)) {
                return ExportTypeString;
            }
            else if (type == typeof(int)) {
                return ExportTypeInt;
            }
            else if (type == typeof(bool)) {
                return ExportTypeBool;
            }
            else if (type == typeof(double)) {
                return ExportTypeDouble;
            }
            else if (type == typeof(Single)) {
                return ExportTypeSingle;
            }
            else if (type == typeof(DateTime)) {
                return ExportTypeDateTime;
            }
            else if (type == typeof(Color)) {
                return ExportTypeColor;
            }
            else if (type == typeof(Unit)) {
                return ExportTypeUnit;
            }
            else if (type == typeof(FontSize)) {
                return ExportTypeFontSize;
            }
            else if (type == typeof(ContentDirection)) {
                return ExportTypeDirection;
            }
            else if (type == typeof(WebPartHelpMode)) {
                return ExportTypeHelpMode;
            }
            else if (type == typeof(PartChromeState)) {
                return ExportTypeChromeState;
            }
            else if (type == typeof(PartChromeType)) {
                return ExportTypeChromeType;
            }
            else if (type == typeof(WebPartExportMode)) {
                return ExportTypeExportMode;
            }
            else if (type == typeof(object)) {
                return ExportTypeObject;
            }
            else {
                return type.AssemblyQualifiedName;
            }
        }

        /// <devdoc>
        /// Used by the page developer to get a reference to the WebPart that contains a control
        /// placed in a WebPartZone.  Returns null if the control is not inside a WebPart.
        /// </devdoc>
        public GenericWebPart GetGenericWebPart(Control control) {
             if (control == null) {
                 throw new ArgumentNullException("control");
             }

             // PERF: First check the parent of the control, before looping through all GenericWebParts
             Control parent = control.Parent;
             GenericWebPart genericParent = parent as GenericWebPart;
             if (genericParent != null && genericParent.ChildControl == control) {
                 return genericParent;
             }
             else {
                 foreach (WebPart part in Controls) {
                     GenericWebPart genericPart = part as GenericWebPart;
                     if (genericPart != null && genericPart.ChildControl == control) {
                         return genericPart;
                     }
                 }
             }

             return null;
        }

        internal ProviderConnectionPoint GetProviderConnectionPoint(WebPart webPart, string connectionPointID) {
            ProviderConnectionPointCollection points = GetProviderConnectionPoints(webPart);
            if (points != null && points.Count > 0) {
                return points[connectionPointID];
            }
            else {
                return null;
            }
        }

        public virtual ProviderConnectionPointCollection GetProviderConnectionPoints(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            // Do not check that Controls.Contains(webPart), since this may be called on a WebPart
            // outside of a Zone.  Also, this method shouldn't really care whether the WebPart is
            // inside the WebPartManager.

            return GetProviderConnectionPoints(webPart.ToControl().GetType());
        }

        private static ProviderConnectionPointCollection GetProviderConnectionPoints(Type type) {
            ICollection[] connectionPoints = GetConnectionPoints(type);
            return (ProviderConnectionPointCollection)connectionPoints[1];
        }

        /// <devdoc>
        /// Returns the web parts that should currently be rendered by the zone.  It is important that
        /// this method filter out any web parts that will not be rendered by the zone, otherwise
        /// the AddWebPart method will not work correctly (VSWhidbey 77719)
        /// </devdoc>
        internal WebPartCollection GetWebPartsForZone(WebPartZoneBase zone) {
            if (zone == null) {
                throw new ArgumentNullException("zone");
            }
            if (_webPartZones.Contains(zone) == false) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_MustRegister), "zone");
            }

            IList allWebPartsForZone = GetAllWebPartsForZone(zone);
            WebPartCollection webParts = new WebPartCollection();

            if (allWebPartsForZone.Count > 0) {
                foreach (WebPart part in allWebPartsForZone) {
                    if (ShouldRenderWebPartInZone(part, zone)) {
                        webParts.Add(part);
                    }
                }
            }

            return webParts;
        }

        /// <devdoc>
        /// If the WebPart is a consumer on the given connection point, returns the corresponding connection.
        /// Else, returns null.
        /// </devdoc>
        internal WebPartConnection GetConnectionForConsumer(WebPart consumer, ConsumerConnectionPoint connectionPoint) {
            ConsumerConnectionPoint actualConnectionPoint = connectionPoint;
            // 
            if (connectionPoint == null) {
                actualConnectionPoint = GetConsumerConnectionPoint(consumer, null);
            }

            // PERF: Use the StaticConnections and DynamicConnections collections separately, instead
            // of using the Connections property which is created on every call.
            foreach (WebPartConnection connection in StaticConnections) {
                if (!Internals.ConnectionDeleted(connection) && connection.Consumer == consumer) {
                    ConsumerConnectionPoint c =
                        GetConsumerConnectionPoint(consumer, connection.ConsumerConnectionPointID);
                    if (c == actualConnectionPoint) {
                        return connection;
                    }
                }
            }

            foreach (WebPartConnection connection in DynamicConnections) {
                if (!Internals.ConnectionDeleted(connection) && connection.Consumer == consumer) {
                    ConsumerConnectionPoint c =
                        GetConsumerConnectionPoint(consumer, connection.ConsumerConnectionPointID);
                    if (c == actualConnectionPoint) {
                        return connection;
                    }
                }
            }

            return null;
        }

        /// <devdoc>
        /// If the WebPart is a provider on the given connection point, returns the corresponding connection.
        /// Else, returns null.
        /// </devdoc>
        internal WebPartConnection GetConnectionForProvider(WebPart provider, ProviderConnectionPoint connectionPoint) {
            ProviderConnectionPoint actualConnectionPoint = connectionPoint;
            if (connectionPoint == null) {
                actualConnectionPoint = GetProviderConnectionPoint(provider, null);
            }

            // PERF: Use the StaticConnections and DynamicConnections collections separately, instead
            // of using the Connections property which is created on every call.
            foreach (WebPartConnection connection in StaticConnections) {
                if (!Internals.ConnectionDeleted(connection) && connection.Provider == provider) {
                    ProviderConnectionPoint c =
                        GetProviderConnectionPoint(provider, connection.ProviderConnectionPointID);
                    if (c == actualConnectionPoint) {
                        return connection;
                    }
                }
            }

            foreach (WebPartConnection connection in DynamicConnections) {
                if (!Internals.ConnectionDeleted(connection) && connection.Provider == provider) {
                    ProviderConnectionPoint c =
                        GetProviderConnectionPoint(provider, connection.ProviderConnectionPointID);
                    if (c == actualConnectionPoint) {
                        return connection;
                    }
                }
            }

            return null;
        }

        private static void ImportReadTo(XmlReader reader, string elementToFind) {
            while (reader.Name != elementToFind) {
                if (!reader.Read()) {
                    throw new XmlException();
                }
            }
        }

        private static void ImportReadTo(XmlReader reader, string elementToFindA, string elementToFindB) {
            while (reader.Name != elementToFindA && reader.Name != elementToFindB) {
                if (!reader.Read()) {
                    throw new XmlException();
                }
            }
        }

        private static void ImportSkipTo(XmlReader reader, string elementToFind) {
            while (reader.Name != elementToFind) {
                reader.Skip();
                if (reader.EOF) {
                    throw new XmlException();
                }
            }
        }

        /// <devdoc>
        /// Never throws except for null arguments. Returns an error message in the out parameter instead.
        /// [Microsoft] I investigated whether this could be refactored to share common code with
        ///           LoadDynamicWebPart(), but it seems the methods are too different.
        /// </devdoc>
        public virtual WebPart ImportWebPart(XmlReader reader, out string errorMessage) {
            Personalization.EnsureEnabled(/* ensureModifiable */ true);

            if (reader == null) {
                throw new ArgumentNullException("reader");
            }

            bool permitOnly = false;

            if (UsePermitOnly) {
                MinimalPermissionSet.PermitOnly();
                permitOnly = true;
            }
            string importErrorMessage = string.Empty;
            // Extra try-catch block to prevent elevation of privilege attack via exception filter
            try {
                try {
                    // Get to the metadata
                    reader.MoveToContent();
                    reader.ReadStartElement(ExportRootElement);
                    ImportSkipTo(reader, ExportPartElement);
                    // Check the version on the webPart element
                    string version = reader.GetAttribute(ExportPartNamespaceAttribute);
                    if (String.IsNullOrEmpty(version)) {
                        errorMessage = SR.GetString(SR.WebPart_ImportErrorNoVersion);
                        return null;
                    }
                    if (!String.Equals(version, ExportPartNamespaceValue, StringComparison.OrdinalIgnoreCase)) {
                        errorMessage = SR.GetString(SR.WebPart_ImportErrorInvalidVersion);
                        return null;
                    }
                    ImportReadTo(reader, ExportMetaDataElement);
                    reader.ReadStartElement(ExportMetaDataElement);
                    // Get the type name
                    string partTypeName = null;
                    string userControlTypeName = null;
                    ImportSkipTo(reader, ExportTypeElement);
                    partTypeName = reader.GetAttribute(ExportTypeNameAttribute);
                    userControlTypeName = reader.GetAttribute(ExportUserControlSrcAttribute);
                    // Get the error message to display if unsuccessful to load the type
                    ImportSkipTo(reader, ExportErrorMessageElement);
                    importErrorMessage = reader.ReadElementString();
                    // Get a type object from the type name
                    Type partType;
                    WebPart part = null;
                    Control childControl = null;

                    try {
                        // If we are in shared scope, we are importing a shared WebPart
                        bool isShared = (Personalization.Scope == PersonalizationScope.Shared);

                        if (!String.IsNullOrEmpty(partTypeName)) {

                            if (UsePermitOnly) {
                                CodeAccessPermission.RevertPermitOnly();
                                permitOnly = false;
                                MediumPermissionSet.PermitOnly();
                                permitOnly = true;
                            }

                            partType = WebPartUtil.DeserializeType(partTypeName, true);

                            if (UsePermitOnly) {
                                CodeAccessPermission.RevertPermitOnly();
                                permitOnly = false;
                                MinimalPermissionSet.PermitOnly();
                                permitOnly = true;
                            }

                            // First check if the type is authorized
                            if (!IsAuthorized(partType, null, null, isShared)) {
                                errorMessage = SR.GetString(SR.WebPartManager_ForbiddenType);
                                return null;
                            }
                            // If the type is not a webpart, create a generic Web Part
                            if (!partType.IsSubclassOf(typeof(WebPart))) {
                                if (!partType.IsSubclassOf(typeof(Control))) {
                                    // We only allow for Controls (VSWhidbey 428511)
                                    errorMessage = SR.GetString(SR.WebPartManager_TypeMustDeriveFromControl);
                                    return null;
                                }
                                // Create an instance of the object
                                childControl = (Control)(Internals.CreateObjectFromType(partType));
                                childControl.ID = CreateDynamicWebPartID(partType);
                                part = CreateWebPart(childControl);
                            }
                            else {
                                // Create an instance of the object
                                part = (WebPart)(Internals.CreateObjectFromType(partType));
                            }
                        }
                        else {
                            // Instantiate a user control in a generic web part

                            // Check if the path is authorized
                            if (!IsAuthorized(typeof(UserControl), userControlTypeName, null, isShared)) {
                                errorMessage = SR.GetString(SR.WebPartManager_ForbiddenType);
                                return null;
                            }

                            if (UsePermitOnly) {
                                CodeAccessPermission.RevertPermitOnly();
                                permitOnly = false;
                            }
                            childControl = Page.LoadControl(userControlTypeName);
                            partType = childControl.GetType();
                            if (UsePermitOnly) {
                                MinimalPermissionSet.PermitOnly();
                                permitOnly = true;
                            }
                            childControl.ID = CreateDynamicWebPartID(partType);
                            part = CreateWebPart(childControl);
                        }
                    }
                    catch {
                        if (!String.IsNullOrEmpty(importErrorMessage)) {
                            errorMessage = importErrorMessage;
                        }
                        else {
                            errorMessage = SR.GetString(SR.WebPartManager_ErrorLoadingWebPartType);
                        }
                        return null;
                    }

                    // Set default error message for all subsequent errors
                    if (String.IsNullOrEmpty(importErrorMessage)) {
                        importErrorMessage = SR.GetString(SR.WebPart_DefaultImportErrorMessage);
                    }
                    // Get to the data
                    ImportSkipTo(reader, ExportDataElement);
                    reader.ReadStartElement(ExportDataElement);
                    ImportSkipTo(reader, ExportPropertiesElement);
                    if (!reader.IsEmptyElement) {
                        reader.ReadStartElement(ExportPropertiesElement);
                        // Special-case IPersonalizable controls
                        // ImportFromReader will set the right permission set when appropriate, reverting before we call
                        if (UsePermitOnly) {
                            CodeAccessPermission.RevertPermitOnly();
                            permitOnly = false;
                        }
                        ImportIPersonalizable(reader, (childControl != null ? childControl : part));
                        if (UsePermitOnly) {
                            MinimalPermissionSet.PermitOnly();
                            permitOnly = true;
                        }
                    }
                    // Set property values from XML description
                    IDictionary personalizableProperties;
                    if (childControl != null) {
                        if (!reader.IsEmptyElement) {
                            // Get the collection of personalizable properties for the child control
                            personalizableProperties = PersonalizableAttribute.GetPersonalizablePropertyEntries(partType);

                            // Copied from below.  We must also execute this code when parsing the ChildControl
                            // IPersonalizable and Personalizable properties.
                            while (reader.Name != ExportPropertyElement) {
                                reader.Skip();
                                if (reader.EOF) {
                                    errorMessage = null;
                                    return part;
                                }
                            }                            
                            
                            // ImportFromReader will set the right permission set when appropriate, reverting before we call
                            if (UsePermitOnly) {
                                CodeAccessPermission.RevertPermitOnly();
                                permitOnly = false;
                            }
                            ImportFromReader(personalizableProperties, childControl, reader);
                            if (UsePermitOnly) {
                                MinimalPermissionSet.PermitOnly();
                                permitOnly = true;
                            }
                        }
                        // And then for the generic WebPart
                        ImportSkipTo(reader, ExportGenericPartPropertiesElement);
                        reader.ReadStartElement(ExportGenericPartPropertiesElement);
                        // ImportFromReader will set the right permission set when appropriate, reverting before we call
                        if (UsePermitOnly) {
                            CodeAccessPermission.RevertPermitOnly();
                            permitOnly = false;
                        }
                        ImportIPersonalizable(reader, part);
                        if (UsePermitOnly) {
                            MinimalPermissionSet.PermitOnly();
                            permitOnly = true;
                        }
                        personalizableProperties = PersonalizableAttribute.GetPersonalizablePropertyEntries(part.GetType());
                    }
                    else {
                        // Get the collection of personalizable properties
                        personalizableProperties = PersonalizableAttribute.GetPersonalizablePropertyEntries(partType);
                    }

                    while (reader.Name != ExportPropertyElement) {
                        reader.Skip();
                        if (reader.EOF) {
                            errorMessage = null;
                            return part;
                        }
                    }

                    // ImportFromReader will set the right permission set when appropriate, reverting before we call
                    if (UsePermitOnly) {
                        CodeAccessPermission.RevertPermitOnly();
                        permitOnly = false;
                    }
                    ImportFromReader(personalizableProperties, part, reader);
                    if (UsePermitOnly) {
                        MinimalPermissionSet.PermitOnly();
                        permitOnly = true;
                    }
                    errorMessage = null;
                    // Return imported part
                    return part;
                }
                catch (XmlException) {
                    errorMessage = SR.GetString(SR.WebPartManager_ImportInvalidFormat);
                    return null;
                }
                catch (Exception e) {
                    if ((Context != null) && (Context.IsCustomErrorEnabled)) {
                        errorMessage = (importErrorMessage.Length != 0) ?
                            importErrorMessage :
                            SR.GetString(SR.WebPart_DefaultImportErrorMessage);
                    }
                    else {
                        errorMessage = e.Message;
                    }
                    return null;
                }
                finally {
                    if (permitOnly) {
                        // revert if you're not just exiting the stack frame anyway
                        CodeAccessPermission.RevertPermitOnly();
                    }
                }
            }
            catch {
                throw;
            }
        }

        private void ImportIPersonalizable(XmlReader reader, Control control) {
            if (control is IPersonalizable) {
                // The control may implement IPersonalizable, but the .WebPart file may not contain
                // an "ipersonalizable" element.  The WebPart may have returned no data from its
                // IPersonalizable.Save() method, or the WebPart may have been recently changed to
                // implement IPersonalizable.  This are valid scenarios, so we should not require
                // the XML to contain the "ipersonalizable" element. (VSWhidbey 499016)

                // Read to the next element that is either "property" or "ipersonalizable".
                ImportReadTo(reader, ExportIPersonalizableElement, ExportPropertyElement);

                // If the next element is "ipersonalizable", then we import the IPersonalizable data.
                // Else, we do nothing, and the current "property" element will be imported as a standard
                // personalizable property.
                if (reader.Name == ExportIPersonalizableElement) {
                    // Create a dictionary from the XML description
                    reader.ReadStartElement(ExportIPersonalizableElement);
                    ImportFromReader(null, control, reader);
                }
            }
        }

        private void ImportFromReader(IDictionary personalizableProperties,
                                      Control target,
                                      XmlReader reader) {

            Debug.Assert(target != null);

            ImportReadTo(reader, ExportPropertyElement);

            bool permitOnly = false;

            if (UsePermitOnly) {
                MinimalPermissionSet.PermitOnly();
                permitOnly = true;
            }

            try {
                try {
                    IDictionary properties;
                    if (personalizableProperties != null) {
                        properties = new HybridDictionary();
                    }
                    else {
                        properties = new PersonalizationDictionary();
                    }
                    // Set properties from the xml document
                    while (reader.Name == ExportPropertyElement) {
                        // Get the name of the property
                        string propertyName = reader.GetAttribute(ExportPropertyNameAttribute);
                        string typeName = reader.GetAttribute(ExportPropertyTypeAttribute);
                        string scope = reader.GetAttribute(ExportPropertyScopeAttribute);
                        bool isNull = String.Equals(
                            reader.GetAttribute(ExportPropertyNullAttribute),
                            "true",
                            StringComparison.OrdinalIgnoreCase);

                        // Do not import Zone information or AuthorizationFilter or custom data
                        if (propertyName == AuthorizationFilterName ||
                             propertyName == ZoneIDName ||
                             propertyName == ZoneIndexName) {

                            reader.ReadElementString();
                            if (!reader.Read()) {
                                throw new XmlException();
                            }
                        }
                        else {
                            string valString = reader.ReadElementString();
                            object val = null;
                            bool valueComputed = false;
                            PropertyInfo pi = null;
                            if (personalizableProperties != null) {
                                // Get the relevant personalizable property on the target (no need to check the property is personalizable)
                                PersonalizablePropertyEntry entry = (PersonalizablePropertyEntry)(personalizableProperties[propertyName]);
                                if (entry != null) {
                                    pi = entry.PropertyInfo;
                                    Debug.Assert(pi != null);
                                    // If the property is a url, validate protocol (VSWhidbey 290418)
                                    UrlPropertyAttribute urlAttr = Attribute.GetCustomAttribute(pi, typeof(UrlPropertyAttribute), true) as UrlPropertyAttribute;
                                    if (urlAttr != null && CrossSiteScriptingValidation.IsDangerousUrl(valString)) {
                                        throw new InvalidDataException(SR.GetString(SR.WebPart_BadUrl, valString));
                                    }
                                }
                            }

                            Type type = null;
                            if (!String.IsNullOrEmpty(typeName)) {
                                if (UsePermitOnly) {
                                    // Need medium trust to call BuildManager.GetType()
                                    CodeAccessPermission.RevertPermitOnly();
                                    permitOnly = false;
                                    MediumPermissionSet.PermitOnly();
                                    permitOnly = true;
                                }
                                type = GetExportType(typeName);

                                if (UsePermitOnly) {
                                    CodeAccessPermission.RevertPermitOnly();
                                    permitOnly = false;
                                    MinimalPermissionSet.PermitOnly();
                                    permitOnly = true;
                                }
                            }

                            if ((pi != null) && ((pi.PropertyType == type) || (type == null))) {
                                // Look at the target property
                                // See if the property itself has a type converter associated with it
                                TypeConverterAttribute attr = Attribute.GetCustomAttribute(pi, typeof(TypeConverterAttribute), true) as TypeConverterAttribute;
                                if (attr != null) {
                                    if (UsePermitOnly) {
                                        // Need medium trust to call BuildManager.GetType()
                                        CodeAccessPermission.RevertPermitOnly();
                                        permitOnly = false;
                                        MediumPermissionSet.PermitOnly();
                                        permitOnly = true;
                                    }

                                    Type converterType = WebPartUtil.DeserializeType(attr.ConverterTypeName, false);

                                    if (UsePermitOnly) {
                                        CodeAccessPermission.RevertPermitOnly();
                                        permitOnly = false;
                                        MinimalPermissionSet.PermitOnly();
                                        permitOnly = true;
                                    }

                                    // SECURITY: Check that the type is a subclass of TypeConverter before instantiating.
                                    if (converterType != null && converterType.IsSubclassOf(typeof(TypeConverter))) {
                                        TypeConverter converter = (TypeConverter)(Internals.CreateObjectFromType(converterType));
                                        if (Util.CanConvertToFrom(converter, typeof(string))) {
                                            if (!isNull) {
                                                val = converter.ConvertFromInvariantString(valString);
                                            }
                                            valueComputed = true;
                                        }
                                    }
                                }
                                // Then, look at the converters on the property type
                                if (!valueComputed) {
                                    // Use the type converter associated with the type itself
                                    TypeConverter converter = TypeDescriptor.GetConverter(pi.PropertyType);
                                    if (Util.CanConvertToFrom(converter, typeof(string))) {
                                        if (!isNull) {
                                            val = converter.ConvertFromInvariantString(valString);
                                        }
                                        valueComputed = true;
                                        // Not importing anything else for security reasons
                                    }
                                }
                            }
                            // finally, use the XML-specified type
                            if (!valueComputed && (type != null)) {
                                // Look at the XML-declared type
                                if (type == typeof(string)) {
                                    if (!isNull) {
                                        val = valString;
                                    }
                                    valueComputed = true;
                                }
                                else {
                                    TypeConverter typeConverter = TypeDescriptor.GetConverter(type);
                                    if (Util.CanConvertToFrom(typeConverter, typeof(string))) {
                                        if (!isNull) {
                                            val = typeConverter.ConvertFromInvariantString(valString);
                                        }
                                        valueComputed = true;
                                    }
                                }
                            }

                            // Always want to import a null IPersonalizable value, since we will never have a type
                            // converter for the value.  However, we should not import a null Personalizable value
                            // unless the PropertyInfo had a type converter, since the property may be a value type
                            // that cannot accept null as a value. (VSWhidbey 537895)
                            if (isNull && personalizableProperties == null) {
                                valueComputed = true;
                            }

                            // Now we should have a value (val)
                            if (valueComputed) {
                                if (personalizableProperties != null) {
                                    properties.Add(propertyName, val);
                                }
                                else {
                                    // Determine scope:
                                    PersonalizationScope personalizationScope =
                                        String.Equals(scope, PersonalizationScope.Shared.ToString(), StringComparison.OrdinalIgnoreCase) ?
                                        PersonalizationScope.Shared : PersonalizationScope.User;
                                    properties.Add(propertyName, new PersonalizationEntry(val, personalizationScope));
                                }
                            }
                            else {
                                throw new HttpException(SR.GetString(SR.WebPartManager_ImportInvalidData, propertyName));
                            }
                        }
                        while (reader.Name != ExportPropertyElement) {
                            if (reader.EOF ||
                                (reader.Name == ExportGenericPartPropertiesElement) ||
                                (reader.Name == ExportPropertiesElement) ||
                                ((reader.Name == ExportIPersonalizableElement) && (reader.NodeType == XmlNodeType.EndElement))) {
                                goto EndOfData;
                            }
                            reader.Skip();
                        }
                    }
                    EndOfData:
                    if (personalizableProperties != null) {
                        IDictionary unused = BlobPersonalizationState.SetPersonalizedProperties(target, properties);
                        if ((unused != null) && (unused.Count > 0)) {
                            IVersioningPersonalizable versioningTarget = target as IVersioningPersonalizable;
                            if (versioningTarget != null) {
                                versioningTarget.Load(unused);
                            }
                        }
                    }
                    else {
                        Debug.Assert(target is IPersonalizable);
                        ((IPersonalizable)target).Load((PersonalizationDictionary)properties);
                    }
                }
                finally {
                    if (permitOnly) {
                        // revert if you're not just exiting the stack frame anyway
                        CodeAccessPermission.RevertPermitOnly();
                    }
                }
            }
            catch {
                throw;
            }
        }

        public virtual bool IsAuthorized(Type type, string path, string authorizationFilter, bool isShared) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            if (type == typeof(UserControl)) {
                if (String.IsNullOrEmpty(path)) {
                    throw new ArgumentException(SR.GetString(SR.WebPartManager_PathCannotBeEmpty));
                }
            }
            else {
                if (!String.IsNullOrEmpty(path)) {
                    throw new ArgumentException(SR.GetString(SR.WebPartManager_PathMustBeEmpty, path));
                }
            }

            WebPartAuthorizationEventArgs auth = new WebPartAuthorizationEventArgs(type, path, authorizationFilter, isShared);
            OnAuthorizeWebPart(auth);
            return auth.IsAuthorized;
        }

        public bool IsAuthorized(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }

            // Do not check that Controls.Contains(webPart), since this will be called on a WebPart
            // before it is added to the Controls collection.

            // Calculate authorizationFilter from property value and personalization data
            string authorizationFilter = webPart.AuthorizationFilter;

            // webPart.ID will be null for imported WebParts.  Also, a user may want to call
            // this method on a WebPart before it has an ID.
            string webPartID = webPart.ID;
            if (!String.IsNullOrEmpty(webPartID) && Personalization.IsEnabled) {
                string personalizedAuthorizationFilter = Personalization.GetAuthorizationFilter(webPart.ID);
                if (personalizedAuthorizationFilter != null) {
                    authorizationFilter = personalizedAuthorizationFilter;
                }
            }

            GenericWebPart genericWebPart = webPart as GenericWebPart;
            if (genericWebPart != null) {
                Type childType = null;
                string childPath = null;

                Control childControl = genericWebPart.ChildControl;
                UserControl childUserControl = childControl as UserControl;
                if (childUserControl != null) {
                    childType = typeof(UserControl);
                    childPath = childUserControl.AppRelativeVirtualPath;
                }
                else {
                    childType = childControl.GetType();
                }

                // Only authorize the type/path of the child control
                // Don't need to authorize the GenericWebPart as well
                return IsAuthorized(childType, childPath, authorizationFilter, webPart.IsShared);
            }
            else {
                return IsAuthorized(webPart.GetType(), null, authorizationFilter, webPart.IsShared);
            }
        }

        internal bool IsConsumerConnected(WebPart consumer, ConsumerConnectionPoint connectionPoint) {
            return (GetConnectionForConsumer(consumer, connectionPoint) != null);
        }

        internal bool IsProviderConnected(WebPart provider, ProviderConnectionPoint connectionPoint) {
            return (GetConnectionForProvider(provider, connectionPoint) != null);
        }

        /// <devdoc>
        /// Loads the control state for those properties that should persist across postbacks
        /// even when EnableViewState=false.
        /// </devdoc>
        protected internal override void LoadControlState(object savedState) {
            if (savedState == null) {
                base.LoadControlState(null);
            }
            else {
                object[] myState = (object[])savedState;
                if (myState.Length != controlStateArrayLength) {
                    throw new ArgumentException(SR.GetString(SR.Invalid_ControlState));
                }

                base.LoadControlState(myState[baseIndex]);

                // 

                if (myState[selectedWebPartIndex] != null) {
                    // All dynamic parts must be loaded before this point, in case a dynamic part is the
                    // SelectedWebPart.
                    WebPart selectedWebPart = WebParts[(string)myState[selectedWebPartIndex]];
                    if (selectedWebPart == null || selectedWebPart.IsClosed) {
                        // The SelectedWebPart was either closed or deleted between requests.
                        // Raise the changed event, since the SelectedWebPart was not null on the previous request.
                        SetSelectedWebPart(null);
                        OnSelectedWebPartChanged(new WebPartEventArgs(null));
                    }
                    else {
                        SetSelectedWebPart(selectedWebPart);
                    }
                }
                if (myState[displayModeIndex] != null) {
                    string modeName = (string)myState[displayModeIndex];

                    WebPartDisplayMode restoredDisplayMode = SupportedDisplayModes[modeName];
                    if (!restoredDisplayMode.IsEnabled(this)) {
                        // Throw
                    }

                    if (restoredDisplayMode == null) {
                        _displayMode = BrowseDisplayMode;
                        OnDisplayModeChanged(new WebPartDisplayModeEventArgs(null));
                    }
                    else {
                        _displayMode = restoredDisplayMode;
                    }
                }
            }
        }

        protected virtual void LoadCustomPersonalizationState(PersonalizationDictionary state) {
            // The state must be loaded after the Static Connections and WebParts have been added
            // to the WebPartManager (after the WebPartZone's and ProxyWebPartManager's Init methods)
            _personalizationState = state;
        }

        private void LoadDynamicConnections(PersonalizationEntry entry) {
            if (entry != null) {
                object[] dynamicConnectionState = (object[])entry.Value;
                if (dynamicConnectionState != null) {
                    Debug.Assert(dynamicConnectionState.Length % 7 == 0);
                    for (int i = 0; i < dynamicConnectionState.Length; i += 7) {
                        string ID = (string)dynamicConnectionState[i];
                        string consumerID = (string)dynamicConnectionState[i + 1];
                        string consumerConnectionPointID = (string)dynamicConnectionState[i + 2];
                        string providerID = (string)dynamicConnectionState[i + 3];
                        string providerConnectionPointID = (string)dynamicConnectionState[i + 4];

                        // Add a new connection to the collection
                        WebPartConnection connection = new WebPartConnection();
                        connection.ID = ID;
                        connection.ConsumerID = consumerID;
                        connection.ConsumerConnectionPointID = consumerConnectionPointID;
                        connection.ProviderID = providerID;
                        connection.ProviderConnectionPointID = providerConnectionPointID;
                        Internals.SetIsShared(connection, (entry.Scope == PersonalizationScope.Shared));
                        Internals.SetIsStatic(connection, false);

                        Type type = dynamicConnectionState[i + 5] as Type;
                        if (type != null) {
                            // SECURITY: Only instantiate type if it is a subclass of WebPartTransformer
                            if (type.IsSubclassOf(typeof(WebPartTransformer))) {
                                object configuration = dynamicConnectionState[i + 6];
                                WebPartTransformer transformer = (WebPartTransformer)Internals.CreateObjectFromType(type);
                                Internals.LoadConfigurationState(transformer, configuration);
                                Internals.SetTransformer(connection, transformer);
                            }
                            else {
                                throw new InvalidOperationException(SR.GetString(SR.WebPartTransformerAttribute_NotTransformer, type.Name));
                            }
                        }

                        DynamicConnections.Add(connection);
                    }
                }
            }
        }

        private void LoadDynamicWebPart(string id, string typeName, string path, string genericWebPartID, bool isShared) {
            WebPart dynamicWebPart = null;
            Type type = WebPartUtil.DeserializeType(typeName, false);
            if (type == null) {
                string errorMessage;
                if (Context != null && Context.IsCustomErrorEnabled) {
                    errorMessage = SR.GetString(SR.WebPartManager_ErrorLoadingWebPartType);
                }
                else {
                    errorMessage = SR.GetString(SR.Invalid_type, typeName);
                }
                dynamicWebPart = CreateErrorWebPart(id, typeName, path, genericWebPartID, errorMessage);
            }
            else if (type.IsSubclassOf(typeof(WebPart))) {
                string authorizationFilter = Personalization.GetAuthorizationFilter(id);
                if (IsAuthorized(type, null, authorizationFilter, isShared)) {
                    try {
                        dynamicWebPart = (WebPart)Internals.CreateObjectFromType(type);
                        dynamicWebPart.ID = id;
                    }
                    catch {
                        // If custom errors are enabled, we do not want to render the type name to the browser.
                        // (VSWhidbey 381646)
                        string errorMessage;
                        if (Context != null && Context.IsCustomErrorEnabled) {
                            errorMessage = SR.GetString(SR.WebPartManager_CantCreateInstance);
                        }
                        else {
                            errorMessage = SR.GetString(SR.WebPartManager_CantCreateInstanceWithType, typeName);
                        }
                        dynamicWebPart = CreateErrorWebPart(id, typeName, path, genericWebPartID, errorMessage);
                    }
                }
                else {
                    dynamicWebPart = new UnauthorizedWebPart(id, typeName, path, genericWebPartID);
                }
            }
            else if (type.IsSubclassOf(typeof(Control))) {
                string authorizationFilter = Personalization.GetAuthorizationFilter(genericWebPartID);
                if (IsAuthorized(type, path, authorizationFilter, isShared)) {
                    Control childControl = null;
                    try {
                        if (!String.IsNullOrEmpty(path)) {
                            Debug.Assert(type == typeof(UserControl));
                            childControl = Page.LoadControl(path);
                        }
                        else {
                            childControl = (Control)Internals.CreateObjectFromType(type);
                        }
                        childControl.ID = id;

                        dynamicWebPart = CreateWebPart(childControl);
                        dynamicWebPart.ID = genericWebPartID;
                    }
                    catch {
                        string errorMessage;
                        if (childControl == null && String.IsNullOrEmpty(path)) {
                            if (Context != null && Context.IsCustomErrorEnabled) {
                                errorMessage = SR.GetString(SR.WebPartManager_CantCreateInstance);
                            }
                            else {
                                errorMessage = SR.GetString(SR.WebPartManager_CantCreateInstanceWithType, typeName);
                            }
                        }
                        else if (childControl == null) {
                            if (Context != null && Context.IsCustomErrorEnabled) {
                                errorMessage = SR.GetString(SR.WebPartManager_InvalidPath);
                            }
                            else {
                                errorMessage = SR.GetString(SR.WebPartManager_InvalidPathWithPath, path);
                            }
                        }
                        else {
                            errorMessage = SR.GetString(SR.WebPartManager_CantCreateGeneric);
                        }
                        dynamicWebPart = CreateErrorWebPart(id, typeName, path, genericWebPartID, errorMessage);
                    }
                }
                else {
                    dynamicWebPart = new UnauthorizedWebPart(id, typeName, path, genericWebPartID);
                }
            }
            else {
                // Type is not a subclass of Control.  For security, do not even instantiate
                // the type (VSWhidbey 428511).
                string errorMessage;
                if (Context != null && Context.IsCustomErrorEnabled) {
                    errorMessage = SR.GetString(SR.WebPartManager_TypeMustDeriveFromControl);
                }
                else {
                    errorMessage = SR.GetString(SR.WebPartManager_TypeMustDeriveFromControlWithType, typeName);
                }
                dynamicWebPart = CreateErrorWebPart(id, typeName, path, genericWebPartID, errorMessage);
            }

            Debug.Assert(dynamicWebPart != null);
            Internals.SetIsStatic(dynamicWebPart, false);
            Internals.SetIsShared(dynamicWebPart, isShared);
            Internals.AddWebPart(dynamicWebPart);
        }

        private void LoadDynamicWebParts(PersonalizationEntry entry) {
            if (entry != null) {
                object[] dynamicWebPartState = (object[])entry.Value;
                if (dynamicWebPartState != null) {
                    Debug.Assert(dynamicWebPartState.Length % 4 == 0);
                    bool isShared = (entry.Scope == PersonalizationScope.Shared);

                    // 
                    for (int i = 0; i < dynamicWebPartState.Length; i += 4) {
                        string id = (string)dynamicWebPartState[i];
                        string typeName = (string)dynamicWebPartState[i + 1];
                        string path = (string)dynamicWebPartState[i + 2];
                        string genericWebPartID = (string)dynamicWebPartState[i + 3];

                        LoadDynamicWebPart(id, typeName, path, genericWebPartID, isShared);
                    }
                }
            }
        }

        private void LoadDeletedConnectionState(PersonalizationEntry entry) {
            if (entry != null) {
                string[] deletedConnections = (string[])entry.Value;
                if (deletedConnections != null) {
                    for (int i=0; i < deletedConnections.Length; i++) {
                        string idToDelete = deletedConnections[i];
                        WebPartConnection connectionToDelete = null;

                        foreach (WebPartConnection connection in StaticConnections) {
                            if (String.Equals(connection.ID, idToDelete, StringComparison.OrdinalIgnoreCase)) {
                                connectionToDelete = connection;
                                break;
                            }
                        }
                        if (connectionToDelete == null) {
                            foreach (WebPartConnection connection in DynamicConnections) {
                                if (String.Equals(connection.ID, idToDelete, StringComparison.OrdinalIgnoreCase)) {
                                    connectionToDelete = connection;
                                    break;
                                }
                            }
                        }

                        if (connectionToDelete != null) {
                            // Only shared connections can be deleted
                            Debug.Assert(connectionToDelete.IsShared);

                            // In shared scope, only static connections should be deleted
                            // In user scope, static and dynamic connections can be deleted
                            Debug.Assert(connectionToDelete.IsStatic || entry.Scope == PersonalizationScope.User);

                            Internals.DeleteConnection(connectionToDelete);
                        }
                        else {
                            // Some of the personalization data is invalid, so we should mark ourselves
                            // as dirty so the data will be re-saved, and the invalid data will be removed.
                            _hasDataChanged = true;
                        }
                    }
                }
            }
        }

        /// <devdoc>
        /// Sets the ZoneID, ZoneIndex, and IsClosed properties on the WebParts.  The state
        /// was loaded from personalization.
        /// </devdoc>
        private void LoadWebPartState(PersonalizationEntry entry) {
            if (entry != null) {
                object[] webPartState = (object[])entry.Value;
                if (webPartState != null) {
                    Debug.Assert(webPartState.Length % 4 == 0);
                    for (int i=0; i < webPartState.Length; i += 4) {
                        string id = (string)webPartState[i];
                        string zoneID = (string)webPartState[i + 1];
                        int zoneIndex = (int)webPartState[i + 2];
                        bool isClosed = (bool)webPartState[i + 3];

                        WebPart part = (WebPart)FindControl(id);
                        if (part != null) {
                            Internals.SetZoneID(part, zoneID);
                            Internals.SetZoneIndex(part, zoneIndex);
                            // 

                            Internals.SetIsClosed(part, isClosed);
                        }
                        else {
                            // Some of the personalization data is invalid, so we should mark ourselves
                            // as dirty so the data will be re-saved, and the invalid data will be removed.
                            _hasDataChanged = true;
                        }
                    }
                }
            }
        }

        /// <devdoc>
        /// </devdoc>
        public virtual void MoveWebPart(WebPart webPart, WebPartZoneBase zone, int zoneIndex) {
            Personalization.EnsureEnabled(/* ensureModifiable */ true);

            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            if (!Controls.Contains(webPart)) {
                throw new ArgumentException(SR.GetString(SR.UnknownWebPart), "webPart");
            }
            if (zone == null) {
                throw new ArgumentNullException("zone");
            }
            if (_webPartZones.Contains(zone) == false) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_MustRegister), "zone");
            }
            if (zoneIndex < 0) {
                throw new ArgumentOutOfRangeException("zoneIndex");
            }
            if (webPart.Zone == null || webPart.IsClosed) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_MustBeInZone), "webPart");
            }

            // Return immediately if moving part to its current location
            if ((webPart.Zone == zone) && (webPart.ZoneIndex == zoneIndex)) {
                return;
            }

            WebPartMovingEventArgs e = new WebPartMovingEventArgs(webPart, zone, zoneIndex);
            OnWebPartMoving(e);
            if (_allowEventCancellation && e.Cancel) {
                return;
            }

            RemoveWebPartFromZone(webPart);
            AddWebPartToZone(webPart, zone, zoneIndex);

            // Raise event at very end of Move method
            OnWebPartMoved(new WebPartEventArgs(webPart));

#if DEBUG
            CheckPartZoneIndexes(webPart.Zone);
            CheckPartZoneIndexes(zone);
#endif
        }

        protected virtual void OnAuthorizeWebPart(WebPartAuthorizationEventArgs e) {
            WebPartAuthorizationEventHandler handler = (WebPartAuthorizationEventHandler)Events[AuthorizeWebPartEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnConnectionsActivated(EventArgs e) {
            EventHandler handler = (EventHandler)Events[ConnectionsActivatedEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnConnectionsActivating(EventArgs e) {
            EventHandler handler = (EventHandler)Events[ConnectionsActivatingEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnDisplayModeChanged(WebPartDisplayModeEventArgs e) {
            WebPartDisplayModeEventHandler handler = (WebPartDisplayModeEventHandler)Events[DisplayModeChangedEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnDisplayModeChanging(WebPartDisplayModeCancelEventArgs e) {
            WebPartDisplayModeCancelEventHandler handler = (WebPartDisplayModeCancelEventHandler)Events[DisplayModeChangingEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// </devdoc>
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            if (!DesignMode) {
                Page page = Page;
                if (page != null) {
                    WebPartManager existingInstance = (WebPartManager)page.Items[typeof(WebPartManager)];

                    if (existingInstance != null) {
                        Debug.Assert(existingInstance != this);
                        throw new InvalidOperationException(SR.GetString(SR.WebPartManager_OnlyOneInstance));
                    }

                    page.Items[typeof(WebPartManager)] = this;

                    page.InitComplete += new EventHandler(this.OnPageInitComplete);
                    page.LoadComplete += new EventHandler(this.OnPageLoadComplete);
                    page.SaveStateComplete += new EventHandler(this.OnPageSaveStateComplete);
                    page.RegisterRequiresControlState(this);

                    Personalization.LoadInternal();
                }
            }
        }

        /// <devdoc>
        /// </devdoc>
        protected internal override void OnUnload(EventArgs e) {
            base.OnUnload(e);

            if (!DesignMode) {
                Page page = Page;

                Debug.Assert(page != null);
                if (page != null) {
                    page.Items.Remove(typeof(WebPartManager));
                }
            }
        }

        private void OnPageInitComplete(object sender, EventArgs e) {
            if (_personalizationState != null) {
                // These must be loaded after the Static Connections have been added to the WebPartManager
                // (after the ProxyWebPartManager's Init methods)
                LoadDynamicConnections(_personalizationState["DynamicConnectionsShared"]);
                LoadDynamicConnections(_personalizationState["DynamicConnectionsUser"]);
                LoadDeletedConnectionState(_personalizationState["DeletedConnectionsShared"]);
                LoadDeletedConnectionState(_personalizationState["DeletedConnectionsUser"]);

                // These must be loaded after the Static WebParts have been added to the WebPartManager
                // (after the WebPartZone's Init methods)
                LoadDynamicWebParts(_personalizationState["DynamicWebPartsShared"]);
                LoadDynamicWebParts(_personalizationState["DynamicWebPartsUser"]);
                LoadWebPartState(_personalizationState["WebPartStateShared"]);
                LoadWebPartState(_personalizationState["WebPartStateUser"]);
            }

            _pageInitComplete = true;
        }

        private void OnPageLoadComplete(object sender, EventArgs e) {
            // VSWhidbey 77708
            CloseOrphanedParts();

            _allowCreateDisplayTitles = true;

            // Raise events outside of ActivateConnections() method, since the method is virtual
            OnConnectionsActivating(EventArgs.Empty);
            // Activate connections in Page.LoadComplete instead of WebPartManager.PreRender.
            // Additional connection types can be activated here, so this improves our compatibility. (VSWhidbey 266995)
            ActivateConnections();
            OnConnectionsActivated(EventArgs.Empty);
        }

        private void OnPageSaveStateComplete(object sender, EventArgs e) {
            // NOTE: Ideally this would be done by overriding SaveViewState in
            //       WebPartManager and WebPart to be symmetric with Personalization
            //       loading which happens in TrackViewState.
            //       However SaveViewState is not called when view state is disabled. Also,
            //       we don't want to have everything register for the SaveStateComplete event,
            //       because that creates more management issues for the event handler list.
            //       We don't want to change how the Apply works either, because we'd have
            //       to set up listeners for the Init event on every webpart.
            Personalization.ExtractPersonalizationState();
            foreach (WebPart webPart in Controls) {
                Personalization.ExtractPersonalizationState(webPart);
            }

            Personalization.SaveInternal();
        }

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            if (Page != null) {
                Page.ClientScript.RegisterStartupScript(
                    this,
                    typeof(WebPartManager),
                    ExportSensitiveDataWarningDeclaration,
                    "var __wpmExportWarning='" + Util.QuoteJScriptString(ExportSensitiveDataWarning) + "';",
                    true);

                Page.ClientScript.RegisterStartupScript(
                    this,
                    typeof(WebPartManager),
                    CloseProviderWarningDeclaration,
                    "var __wpmCloseProviderWarning='" + Util.QuoteJScriptString(CloseProviderWarning) + "';",
                    true);

                Page.ClientScript.RegisterStartupScript(
                    this,
                    typeof(WebPartManager),
                    DeleteWarningDeclaration,
                    "var __wpmDeleteWarning='" + Util.QuoteJScriptString(DeleteWarning) + "';",
                    true);

                _renderClientScript = CheckRenderClientScript();
                if (_renderClientScript) {
                    // 

                    Page.RegisterPostBackScript();

                    RegisterClientScript();
                }
            }
        }

        protected virtual void OnSelectedWebPartChanged(WebPartEventArgs e) {
            WebPartEventHandler handler = (WebPartEventHandler)Events[SelectedWebPartChangedEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnSelectedWebPartChanging(WebPartCancelEventArgs e) {
            WebPartCancelEventHandler handler = (WebPartCancelEventHandler)Events[SelectedWebPartChangingEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartAdded(WebPartEventArgs e) {
            WebPartEventHandler handler = (WebPartEventHandler)Events[WebPartAddedEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartAdding(WebPartAddingEventArgs e) {
            WebPartAddingEventHandler handler = (WebPartAddingEventHandler)Events[WebPartAddingEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartClosed(WebPartEventArgs e) {
            WebPartEventHandler handler = (WebPartEventHandler)Events[WebPartClosedEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartClosing(WebPartCancelEventArgs e) {
            WebPartCancelEventHandler handler = (WebPartCancelEventHandler)Events[WebPartClosingEvent];
            if (handler != null) {
                handler(this, e);
            }

        }

        protected virtual void OnWebPartDeleted(WebPartEventArgs e) {
            WebPartEventHandler handler = (WebPartEventHandler)Events[WebPartDeletedEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartDeleting(WebPartCancelEventArgs e) {
            WebPartCancelEventHandler handler = (WebPartCancelEventHandler)Events[WebPartDeletingEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartMoved(WebPartEventArgs e) {
            WebPartEventHandler handler = (WebPartEventHandler)Events[WebPartMovedEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartMoving(WebPartMovingEventArgs e) {
            WebPartMovingEventHandler handler = (WebPartMovingEventHandler)Events[WebPartMovingEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartsConnected(WebPartConnectionsEventArgs e) {
            WebPartConnectionsEventHandler handler = (WebPartConnectionsEventHandler)Events[WebPartsConnectedEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartsConnecting(WebPartConnectionsCancelEventArgs e) {
            WebPartConnectionsCancelEventHandler handler = (WebPartConnectionsCancelEventHandler)Events[WebPartsConnectingEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartsDisconnected(WebPartConnectionsEventArgs e) {
            WebPartConnectionsEventHandler handler = (WebPartConnectionsEventHandler)Events[WebPartsDisconnectedEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnWebPartsDisconnecting(WebPartConnectionsCancelEventArgs e) {
            WebPartConnectionsCancelEventHandler handler = (WebPartConnectionsCancelEventHandler)Events[WebPartsDisconnectingEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void RegisterClientScript() {
            Page.ClientScript.RegisterClientScriptResource(this, typeof(WebPartManager), "WebParts.js");

            bool allowPageDesign = DisplayMode.AllowPageDesign;

            string dragOverlayElementReference = "null";
            if (allowPageDesign) {
                dragOverlayElementReference = "document.getElementById('" + ClientID + "___Drag')";
            }

            StringBuilder zoneCode = new StringBuilder(1024);
            foreach (WebPartZoneBase zone in _webPartZones) {
                string isVertical = (zone.LayoutOrientation == Orientation.Vertical) ? "true" : "false";

                string allowLayoutChange = "false";
                string dragHighlightColor = "black";
                if (allowPageDesign && zone.AllowLayoutChange) {
                    allowLayoutChange = "true";
                    dragHighlightColor = ColorTranslator.ToHtml(zone.DragHighlightColor);
                }

                zoneCode.AppendFormat(CultureInfo.InvariantCulture, ZoneScript, zone.ClientID, zone.UniqueID, isVertical,
                                      allowLayoutChange, dragHighlightColor);

                WebPartCollection webParts = GetWebPartsForZone(zone);
                foreach (WebPart webPart in webParts) {
                    string titleBarElementReference = "null";
                    string allowMove = "false";
                    if (allowPageDesign) {
                        titleBarElementReference = "document.getElementById('" + webPart.TitleBarID + "')";
                        if (webPart.AllowZoneChange) {
                            allowMove = "true";
                        }
                    }
                    zoneCode.AppendFormat(ZonePartScript, webPart.WholePartID, titleBarElementReference, allowMove);
                }

                zoneCode.Append(ZoneEndScript);
            }

            string startupScript = String.Format(CultureInfo.InvariantCulture,
                                                 StartupScript,
                                                 dragOverlayElementReference,
                                                 (Personalization.Scope == PersonalizationScope.Shared ? "true" : "false"),
                                                 zoneCode.ToString());
            Page.ClientScript.RegisterStartupScript(this, typeof(WebPartManager), String.Empty, startupScript, false);

            IScriptManager scriptManager = Page.ScriptManager;
            if ((scriptManager != null) && scriptManager.SupportsPartialRendering) {
                scriptManager.RegisterDispose(this, "WebPartManager_Dispose();");
            }
        }

        internal void RegisterZone(WebZone zone) {
            Debug.Assert(zone != null);

            if (_pageInitComplete) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartManager_RegisterTooLate));
            }

            string zoneID = zone.ID;
            if (String.IsNullOrEmpty(zoneID)) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_NoZoneID), "zone");
            }
            if (_zoneIDs.Contains(zoneID)) {
                throw new ArgumentException(SR.GetString(SR.WebPartManager_DuplicateZoneID, zoneID));
            }
            _zoneIDs.Add(zoneID, zone);

            WebPartZoneBase webPartZone = zone as WebPartZoneBase;
            if (webPartZone != null) {
                if (_webPartZones.Contains(webPartZone)) {
                    throw new ArgumentException(SR.GetString(SR.WebPartManager_AlreadyRegistered), "zone");
                }

                _webPartZones.Add(webPartZone);

                WebPartCollection initialWebParts = webPartZone.GetInitialWebParts();
                ((WebPartManagerControlCollection)Controls).AddWebPartsFromZone(webPartZone, initialWebParts);
            }
            else {
                Debug.Assert(zone is ToolZone);
                ToolZone toolZone = (ToolZone)zone;

                WebPartDisplayModeCollection allDisplayModes = DisplayModes;
                WebPartDisplayModeCollection supportedDisplayModes = SupportedDisplayModes;
                foreach (WebPartDisplayMode displayMode in toolZone.AssociatedDisplayModes) {
                    if (allDisplayModes.Contains(displayMode) && !supportedDisplayModes.Contains(displayMode)) {
                        supportedDisplayModes.AddInternal(displayMode);
                    }
                }
            }
        }

        /// <devdoc>
        /// Deletes the part from the dictionary mapping zones to parts.
        /// </devdoc>
        private void RemoveWebPartFromDictionary(WebPart webPart) {
            if (_partsForZone != null) {
                string zoneID = Internals.GetZoneID(webPart);
                if (!String.IsNullOrEmpty(zoneID)) {
                    SortedList partsForZone = (SortedList)(_partsForZone[zoneID]);
                    if (partsForZone != null) {
                        partsForZone.Remove(webPart);
                    }
                }
            }
        }

        // Called by WebPartManagerInternals
        internal void RemoveWebPart(WebPart webPart) {
            ((WebPartManagerControlCollection)Controls).RemoveWebPart(webPart);
        }

        /// <devdoc>
        /// Removes a web part from its zone, and renumbers all the remaining parts sequentially.
        /// </devdoc>
        private void RemoveWebPartFromZone(WebPart webPart) {
            Debug.Assert(!webPart.IsClosed);

            WebPartZoneBase zone = webPart.Zone;
            Internals.SetIsClosed(webPart, true);
            _hasDataChanged = true;

            RemoveWebPartFromDictionary(webPart);

            // 

            if (zone != null) {
                IList parts = GetAllWebPartsForZone(zone);
                for (int i = 0; i < parts.Count; i++) {
                    WebPart part = ((WebPart)parts[i]);
                    Internals.SetZoneIndex(part, i);
                }
            }
        }

        protected internal override void Render(HtmlTextWriter writer) {
            if (DisplayMode.AllowPageDesign) {
                string dragOverlayElementHtml = String.Format(CultureInfo.InvariantCulture, DragOverlayElementHtmlTemplate, ClientID);
                writer.WriteLine(dragOverlayElementHtml);
            }
        }

        /// <devdoc>
        /// Saves the control state for those properties that should persist across postbacks
        /// even when EnableViewState=false.
        /// </devdoc>
        protected internal override object SaveControlState() {
            object[] myState = new object[controlStateArrayLength];

            myState[baseIndex] = base.SaveControlState();
            if (SelectedWebPart != null) {
                myState[selectedWebPartIndex] = SelectedWebPart.ID;
            }
            if (_displayMode != BrowseDisplayMode) {
                myState[displayModeIndex] = _displayMode.Name;
            }

            for (int i=0; i < controlStateArrayLength; i++) {
                if (myState[i] != null) {
                    return myState;
                }
            }

            // More performant to return null than an array of null values
            return null;
        }

        protected virtual void SaveCustomPersonalizationState(PersonalizationDictionary state) {
            PersonalizationScope scope = Personalization.Scope;

            int webPartsCount = Controls.Count;
            if (webPartsCount > 0) {
                object[] webPartState = new object[webPartsCount * 4];
                for (int i=0; i < webPartsCount; i++) {
                    WebPart webPart = (WebPart)Controls[i];
                    webPartState[4*i] = webPart.ID;
                    webPartState[4*i + 1] = Internals.GetZoneID(webPart);
                    webPartState[4*i + 2] = webPart.ZoneIndex;
                    webPartState[4*i + 3] = webPart.IsClosed;
                }
                if (scope == PersonalizationScope.Shared) {
                    state["WebPartStateShared"] =
                        new PersonalizationEntry(webPartState, PersonalizationScope.Shared);
                }
                else {
                    state["WebPartStateUser"] =
                        new PersonalizationEntry(webPartState, PersonalizationScope.User);
                }
            }

            // Select only the dynamic WebParts that should be saved for this mode
            ArrayList dynamicWebParts = new ArrayList();
            foreach (WebPart webPart in Controls) {
                if (!webPart.IsStatic &&
                    ((scope == PersonalizationScope.User && !webPart.IsShared) ||
                     (scope == PersonalizationScope.Shared && webPart.IsShared))) {
                    dynamicWebParts.Add(webPart);
                }
            }

            int dynamicWebPartsCount = dynamicWebParts.Count;
            if (dynamicWebPartsCount > 0) {
                // Use a 1-dimensional array for smallest storage space
                object[] dynamicWebPartState = new object[dynamicWebPartsCount * 4];
                for (int i = 0; i < dynamicWebPartsCount; i++) {
                    WebPart webPart = (WebPart)dynamicWebParts[i];

                    string id;
                    string typeName;
                    string path = null;
                    string genericWebPartID = null;
                    ProxyWebPart proxyWebPart = webPart as ProxyWebPart;
                    if (proxyWebPart != null) {
                        id = proxyWebPart.OriginalID;
                        typeName = proxyWebPart.OriginalTypeName;
                        path = proxyWebPart.OriginalPath;
                        genericWebPartID = proxyWebPart.GenericWebPartID;
                    }
                    else {
                        GenericWebPart genericWebPart = webPart as GenericWebPart;
                        if (genericWebPart != null) {
                            Control childControl = genericWebPart.ChildControl;
                            UserControl userControl = childControl as UserControl;

                            id = childControl.ID;
                            if (userControl != null) {
                                typeName = WebPartUtil.SerializeType(typeof(UserControl));
                                path = userControl.AppRelativeVirtualPath;
                            }
                            else {
                                typeName = WebPartUtil.SerializeType(childControl.GetType());
                            }
                            genericWebPartID = genericWebPart.ID;
                        }
                        else {
                            id = webPart.ID;
                            typeName = WebPartUtil.SerializeType(webPart.GetType());
                        }
                    }

                    dynamicWebPartState[4*i] = id;
                    dynamicWebPartState[4*i + 1] = typeName;
                    if (!String.IsNullOrEmpty(path)) {
                        dynamicWebPartState[4*i + 2] = path;
                    }
                    if (!String.IsNullOrEmpty(genericWebPartID)) {
                        dynamicWebPartState[4*i + 3] = genericWebPartID;
                    }
                }
                if (scope == PersonalizationScope.Shared) {
                    state["DynamicWebPartsShared"] =
                        new PersonalizationEntry(dynamicWebPartState, PersonalizationScope.Shared);
                }
                else {
                    state["DynamicWebPartsUser"] =
                        new PersonalizationEntry(dynamicWebPartState, PersonalizationScope.User);
                }
            }

            // Save deleted connections
            // 

            ArrayList deletedConnections = new ArrayList();
            // PERF: Use the StaticConnections and DynamicConnections collections separately, instead
            // of using the Connections property which is created on every call.
            foreach (WebPartConnection connection in StaticConnections) {
                if (Internals.ConnectionDeleted(connection)) {
                    deletedConnections.Add(connection);
                }
            }
            foreach (WebPartConnection connection in DynamicConnections) {
                if (Internals.ConnectionDeleted(connection)) {
                    deletedConnections.Add(connection);
                }
            }

            int deletedConnectionsCount = deletedConnections.Count;
            if (deletedConnections.Count > 0) {
                string[] deletedConnectionsState = new string[deletedConnectionsCount];
                for (int i=0; i < deletedConnectionsCount; i++) {
                    WebPartConnection deletedConnection = (WebPartConnection)deletedConnections[i];
                    // Only shared connections can be deleted
                    Debug.Assert(deletedConnection.IsShared);
                    // In shared scope, only static connections should be deleted
                    // In user scope, static and dynamic connections can be deleted
                    Debug.Assert(deletedConnection.IsStatic || scope == PersonalizationScope.User);
                    deletedConnectionsState[i] = deletedConnection.ID;
                }
                if (scope == PersonalizationScope.Shared) {
                    state["DeletedConnectionsShared"] =
                        new PersonalizationEntry(deletedConnectionsState, PersonalizationScope.Shared);
                }
                else {
                    state["DeletedConnectionsUser"] =
                        new PersonalizationEntry(deletedConnectionsState, PersonalizationScope.User);
                }
            }

            // Select only the dynamic Connections that should be saved for this mode
            ArrayList dynamicConnections = new ArrayList();
            foreach (WebPartConnection connection in DynamicConnections) {
                if (((scope == PersonalizationScope.User) && (!connection.IsShared)) ||
                    ((scope == PersonalizationScope.Shared) && (connection.IsShared))) {
                    dynamicConnections.Add(connection);
                }
            }

            int dynamicConnectionsCount = dynamicConnections.Count;
            if (dynamicConnectionsCount > 0) {
                // Use a 1-dimensional array for smallest storage space
                object[] dynamicConnectionState = new object[dynamicConnectionsCount * 7];
                for (int i = 0; i < dynamicConnectionsCount; i++) {
                    WebPartConnection connection = (WebPartConnection)dynamicConnections[i];
                    WebPartTransformer transformer = connection.Transformer;

                    // We should never be saving a deleted dynamic connection.  If the User has deleted a
                    // a shared connection, the connection will be saved in the Shared data, not here.
                    Debug.Assert(!Internals.ConnectionDeleted(connection));

                    dynamicConnectionState[7*i] = connection.ID;
                    dynamicConnectionState[7*i + 1] = connection.ConsumerID;
                    dynamicConnectionState[7*i + 2] = connection.ConsumerConnectionPointID;
                    dynamicConnectionState[7*i + 3] = connection.ProviderID;
                    dynamicConnectionState[7*i + 4] = connection.ProviderConnectionPointID;
                    if (transformer != null) {
                        dynamicConnectionState[7*i + 5] = transformer.GetType();
                        dynamicConnectionState[7*i + 6] = Internals.SaveConfigurationState(transformer);
                    }
                }

                if (scope == PersonalizationScope.Shared) {
                    state["DynamicConnectionsShared"] =
                        new PersonalizationEntry(dynamicConnectionState, PersonalizationScope.Shared);
                }
                else {
                    state["DynamicConnectionsUser"] =
                        new PersonalizationEntry(dynamicConnectionState, PersonalizationScope.User);
                }
            }
        }

        // Can be called by a derived WebPartManager to mark itself as dirty
        protected void SetPersonalizationDirty() {
            Personalization.SetDirty();
        }

        // Returns true if the WebPart should currently be rendered in the Zone.  Determines
        // which WebParts are returned by GetWebPartsForZone.
        private bool ShouldRenderWebPartInZone(WebPart part, WebPartZoneBase zone) {
            Debug.Assert(part.Zone == zone);

            // Never render UnauthorizedWebParts
            if (part is UnauthorizedWebPart) {
                return false;
            }

            return true;
        }

        protected void SetSelectedWebPart(WebPart webPart) {
            _selectedWebPart = webPart;
        }

        // PropertyInfo will be null for an IPersonalizable property, since there is no associated PropertyInfo
        private bool ShouldExportProperty(PropertyInfo propertyInfo, Type propertyValueType,
                                          object propertyValue, out string exportString) {
            string propertyValueAsString = propertyValue as string;
            if (propertyValueAsString != null) {
                exportString = propertyValueAsString;
                return true;
            }
            else {
                TypeConverter converter = null;

                if (propertyInfo != null) {
                    // See if the property itself has a type converter associated with it
                    TypeConverterAttribute attr =
                        Attribute.GetCustomAttribute(propertyInfo, typeof(TypeConverterAttribute), true) as TypeConverterAttribute;
                    if (attr != null) {
                        // Get the type using DeserializeType(), which calls BuildManager.GetType(),
                        // since we want this to work with a non-assembly qualified typename
                        // in the Code directory.
                        Type converterType = WebPartUtil.DeserializeType(attr.ConverterTypeName, false);
                        // SECURITY: Check that the type is a subclass of TypeConverter before instantiating.
                        if (converterType != null && converterType.IsSubclassOf(typeof(TypeConverter))) {
                            TypeConverter tempConverter = (TypeConverter)(Internals.CreateObjectFromType(converterType));
                            if (Util.CanConvertToFrom(tempConverter, typeof(string))) {
                                converter = tempConverter;
                            }
                        }
                    }
                }

                if (converter == null) {
                    // If there was no valid type converter on the property info, look on the type of the value
                    TypeConverter tempConverter = TypeDescriptor.GetConverter(propertyValueType);
                    if (Util.CanConvertToFrom(tempConverter, typeof(string))) {
                        converter = tempConverter;
                    }
                }

                // Only export property if we found a valid type converter (VSWhidbey 496495)
                if (converter != null) {
                    if (propertyValue != null) {
                        exportString = converter.ConvertToInvariantString(propertyValue);
                        return true;
                    }
                    else {
                        // Special-case null value
                        exportString = null;
                        return true;
                    }
                }
                else {
                    exportString = null;
                    if (propertyInfo == null && propertyValue == null) {
                        // Always want to export a null IPersonalizable value, since we will never have a type
                        // converter for the value.  However, we should not export a null Personalizable value
                        // unless the propertyInfo had a type converter, since we may not be able to import a
                        // null value, since the property may be a value type that cannot accept null as a value.
                        // (VSWhidbey 537895)
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            }
        }

        /// <devdoc>
        /// Returns true if the connection should be removed from the dynamic connection
        /// collection when deleted.
        /// </devdoc>
        private bool ShouldRemoveConnection(WebPartConnection connection) {
            Debug.Assert(Personalization.IsModifiable);

            if (connection.IsShared && (Personalization.Scope == PersonalizationScope.User)) {
                // Can't remove shared connection in user mode
                return false;
            }
            else {
                return true;
            }
        }

        /// <internalonly />
        protected override void TrackViewState() {
            Personalization.ApplyPersonalizationState();
            base.TrackViewState();
        }

        // Throw if the type cannot be loaded by BuildManager
        // For instance, we cannot load a type defined in the Page class
        private void VerifyType(Control control) {
            // Don't need to verify type of UserControls, since we load them using
            // their path instead of their type
            if (control is UserControl) {
                return;
            }

            Type type = control.GetType();
            string typeName = WebPartUtil.SerializeType(type);
            Type loadedType = WebPartUtil.DeserializeType(typeName, /* throwOnError */ false);
            if (loadedType != type) {
                throw new InvalidOperationException(
                    SR.GetString(SR.WebPartManager_CantAddControlType, typeName));
            }
        }

        #region Implementation of IPersonalizable
        /// <internalonly/>
        bool IPersonalizable.IsDirty {
            get {
                return IsCustomPersonalizationStateDirty;
            }
        }

        /// <internalonly/>
        void IPersonalizable.Load(PersonalizationDictionary state) {
            LoadCustomPersonalizationState(state);
        }

        /// <internalonly/>
        void IPersonalizable.Save(PersonalizationDictionary state) {
            SaveCustomPersonalizationState(state);
        }
        #endregion

        private sealed class WebPartManagerControlCollection : ControlCollection {

            private WebPartManager _manager;

            public WebPartManagerControlCollection(WebPartManager owner) : base(owner) {
                _manager = owner;
                SetCollectionReadOnly(SR.WebPartManager_CannotModify);
            }

            internal void AddWebPart(WebPart webPart) {
                string originalError = SetCollectionReadOnly(null);
                // Extra try-catch block to prevent elevation of privilege attack via exception filter
                try {
                    try {
                        AddWebPartHelper(webPart);
                    }
                    finally {
                        SetCollectionReadOnly(originalError);
                    }
                }
                catch {
                    throw;
                }
            }

            private void AddWebPartHelper(WebPart webPart) {
                string partID = webPart.ID;
                if (String.IsNullOrEmpty(partID)) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPartManager_NoWebPartID));
                }
                if (_manager._partAndChildControlIDs.Contains(partID)) {
                   throw new InvalidOperationException(SR.GetString(SR.WebPartManager_DuplicateWebPartID, partID));
                }

                // Add to dictionary to prevent duplicate IDs, even if this part is not authorized.  Don't want page
                // developer to have 2 parts with the same ID, and not get the exception until they are both authorized.
                _manager._partAndChildControlIDs.Add(partID, null);

                // Check and add child control ID (VSWhidbey 339482)
                GenericWebPart genericWebPart = webPart as GenericWebPart;
                if (genericWebPart != null) {
                    string childControlID = genericWebPart.ChildControl.ID;

                    if (String.IsNullOrEmpty(childControlID)) {
                        throw new InvalidOperationException(SR.GetString(SR.WebPartManager_NoChildControlID));
                    }

                    if (_manager._partAndChildControlIDs.Contains(childControlID)) {
                        throw new InvalidOperationException(SR.GetString(SR.WebPartManager_DuplicateWebPartID, childControlID));
                    }

                    _manager._partAndChildControlIDs.Add(childControlID, null);
                }

                _manager.Internals.SetIsStandalone(webPart, false);
                webPart.SetWebPartManager(_manager);
                Add(webPart);

                // Invalidate the part dictionary if it has already been created
                _manager._partsForZone = null;
            }

            internal void AddWebPartsFromZone(WebPartZoneBase zone, WebPartCollection webParts) {
                if ((webParts != null) && (webParts.Count != 0)) {
                    string originalError = SetCollectionReadOnly(null);

                    // Extra try-catch block to prevent elevation of privilege attack via exception filter
                    try {
                        try {
                            string zoneID = zone.ID;
                            int index = 0;

                            foreach (WebPart webPart in webParts) {
                                // Need to set IsShared before calling IsAuthorized
                                _manager.Internals.SetIsShared(webPart, true);

                                WebPart webPartOrProxy = webPart;
                                if (!_manager.IsAuthorized(webPart)) {
                                    webPartOrProxy = new UnauthorizedWebPart(webPart);
                                }

                                _manager.Internals.SetIsStatic(webPartOrProxy, true);
                                _manager.Internals.SetIsShared(webPartOrProxy, true);
                                _manager.Internals.SetZoneID(webPartOrProxy, zoneID);
                                _manager.Internals.SetZoneIndex(webPartOrProxy, index);

                                AddWebPartHelper(webPartOrProxy);
                                index++;
                            }
                        }
                        finally {
                            SetCollectionReadOnly(originalError);
                        }
                    } catch {
                        throw;
                    }
                }
            }

            internal void RemoveWebPart(WebPart webPart) {
                string originalError = SetCollectionReadOnly(null);
                // Extra try-catch block to prevent elevation of privilege attack via exception filter
                try {
                    try {
                        _manager._partAndChildControlIDs.Remove(webPart.ID);

                        // Remove child control ID (VSWhidbey 339482)
                        GenericWebPart genericWebPart = webPart as GenericWebPart;
                        if (genericWebPart != null) {
                            _manager._partAndChildControlIDs.Remove(genericWebPart.ChildControl.ID);
                        }

                        Remove(webPart);
                        _manager._hasDataChanged = true;
                        webPart.SetWebPartManager(null);
                        _manager.Internals.SetIsStandalone(webPart, true);

                        // Invalidate the part dictionary if it has already been created
                        _manager._partsForZone = null;
                    }
                    finally {
                        SetCollectionReadOnly(originalError);
                    }
                }
                catch {
                    throw;
                }
            }
        }

        private sealed class BrowseWebPartDisplayMode : WebPartDisplayMode {

            public BrowseWebPartDisplayMode() : base("Browse") {
            }
        }

        private sealed class CatalogWebPartDisplayMode : WebPartDisplayMode {

            public CatalogWebPartDisplayMode() : base("Catalog") {
            }

            public override bool AllowPageDesign {
                get {
                    return true;
                }
            }

            public override bool AssociatedWithToolZone {
                get {
                    return true;
                }
            }

            public override bool RequiresPersonalization {
                get {
                    return true;
                }
            }

            public override bool ShowHiddenWebParts {
                get {
                    return true;
                }
            }
        }

        private sealed class ConnectionPointKey {
            // DevDiv Bugs 38677
            // used as the Cache key for Connection Points, using Type and Culture
            private Type _type;
            private CultureInfo _culture;
            private CultureInfo _uiCulture;

            public ConnectionPointKey(Type type, CultureInfo culture, CultureInfo uiCulture) {
                 Debug.Assert(type != null && culture != null && uiCulture != null);

                 _type = type;
                 _culture = culture;
                 _uiCulture = uiCulture;
            }

            public override bool Equals(object obj) {
                 if (obj == this) {
                      return true;
                 }

                 ConnectionPointKey other = obj as ConnectionPointKey;
                 return (other != null) &&
				    (other._type.Equals(_type)) &&
				    (other._culture.Equals(_culture)) &&
				    (other._uiCulture.Equals(_uiCulture));
            }

            [SuppressMessage("Microsoft.Usage", "CA2303:FlagTypeGetHashCode", Justification = "The types are Sytem.Web.UI.Control derived classes and not com interop types.")]
            public override int GetHashCode()
            {
                 int typeHashCode = _type.GetHashCode();
                 // This is the algorithm used in Whidbey to combine hashcodes.
                 // It adheres better than a simple XOR to the randomness requirement for hashcodes.
                 int hashCode = ((typeHashCode << 5) + typeHashCode) ^ _culture.GetHashCode();
                 return ((hashCode << 5) + hashCode) ^ _uiCulture.GetHashCode();
            }
        }

        private sealed class ConnectWebPartDisplayMode : WebPartDisplayMode {

            public ConnectWebPartDisplayMode() : base("Connect") {
            }

            public override bool AllowPageDesign {
                get {
                    return true;
                }
            }

            public override bool AssociatedWithToolZone {
                get {
                    return true;
                }
            }

            public override bool RequiresPersonalization {
                get {
                    return true;
                }
            }

            public override bool ShowHiddenWebParts {
                get {
                    return true;
                }
            }
        }

        private sealed class DesignWebPartDisplayMode : WebPartDisplayMode {

            public DesignWebPartDisplayMode() : base("Design") {
            }

            public override bool AllowPageDesign {
                get {
                    return true;
                }
            }

            public override bool RequiresPersonalization {
                get {
                    return true;
                }
            }

            public override bool ShowHiddenWebParts {
                get {
                    return true;
                }
            }
        }

        private sealed class EditWebPartDisplayMode : WebPartDisplayMode {

            public EditWebPartDisplayMode() : base("Edit") {
            }

            public override bool AllowPageDesign {
                get {
                    return true;
                }
            }

            public override bool AssociatedWithToolZone {
                get {
                    return true;
                }
            }

            public override bool RequiresPersonalization {
                get {
                    return true;
                }
            }

            public override bool ShowHiddenWebParts {
                get {
                    return true;
                }
            }
        }
    }
}
