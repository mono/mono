//------------------------------------------------------------------------------
// <copyright file="Control.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Caching;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Routing;
    using System.Web.UI.Adapters;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using System.Web.Util;
    using HttpException = System.Web.HttpException;

    // Delegate used for the compiled template
    public delegate void RenderMethod(HtmlTextWriter output, Control container);

    public delegate Control BuildMethod();

    // Defines the properties, methods, and events that are shared by all server
    // controls in the Web Forms page framework.</para>
    [
    Bindable(true),
    DefaultProperty("ID"),
    DesignerCategory("Code"),
    Designer("System.Web.UI.Design.ControlDesigner, " + AssemblyRef.SystemDesign),
    DesignerSerializer("Microsoft.VisualStudio.Web.WebForms.ControlCodeDomSerializer, " + AssemblyRef.MicrosoftVisualStudioWeb,  "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + AssemblyRef.SystemDesign),
    Themeable(false),
    ToolboxItemFilter("System.Web.UI", ToolboxItemFilterType.Require),
    ToolboxItemAttribute("System.Web.UI.Design.WebControlToolboxItem, " + AssemblyRef.SystemDesign)
    ]
    public class Control : IComponent, IParserAccessor, IUrlResolutionService, IDataBindingsAccessor, IControlBuilderAccessor, IControlDesignerAccessor, IExpressionsAccessor {

        internal static readonly object EventDataBinding = new object();
        internal static readonly object EventInit = new object();
        internal static readonly object EventLoad = new object();
        internal static readonly object EventUnload = new object();
        internal static readonly object EventPreRender = new object();
        private static readonly object EventDisposed = new object();

        internal const bool EnableViewStateDefault = true;
        internal const char ID_SEPARATOR = '$';
        private const char ID_RENDER_SEPARATOR = '_';
        internal const char LEGACY_ID_SEPARATOR = ':';

        private string _id;
        // allows us to reuse the id variable to store a calculated id w/o polluting the public getter
        private string _cachedUniqueID;
        private string _cachedPredictableID;
        private Control _parent;

        // fields related to being a container
        private ControlState _controlState;
        private StateBag _viewState;

        private EventHandlerList _events;
        private ControlCollection _controls;

        // The naming container that this control leaves in.  Note that even if
        // this ctrl is a naming container, it will not point to itself, but to
        // the naming container that contains it.
        private Control _namingContainer;
        internal Page _page;
        private OccasionalFields _occasionalFields;
        // The virtual directory of the Page or UserControl that hosts this control.

        // const masks into the BitVector32
        private const int idNotCalculated           = 0x00000001;
        private const int marked                    = 0x00000002;
        private const int disableViewState          = 0x00000004;
        private const int controlsCreated           = 0x00000008;
        private const int invisible                 = 0x00000010;
        private const int visibleDirty              = 0x00000020;
        private const int idNotRequired             = 0x00000040;
        private const int isNamingContainer         = 0x00000080;
        private const int creatingControls          = 0x00000100;
        private const int notVisibleOnPage          = 0x00000200;
        private const int themeApplied              = 0x00000400;
        private const int mustRenderID              = 0x00000800;
        private const int disableTheming            = 0x00001000;
        private const int enableThemingSet          = 0x00002000;
        private const int styleSheetApplied         = 0x00004000;
        private const int controlAdapterResolved    = 0x00008000;
        private const int designMode                = 0x00010000;
        private const int designModeChecked         = 0x00020000;
        private const int disableChildControlState  = 0x00040000;
        internal const int isWebControlDisabled     = 0x00080000;
        private const int controlStateApplied       = 0x00100000;
        private const int useGeneratedID            = 0x00200000;
        private const int validateRequestModeDirty  = 0x00400000;
        private const int viewStateNotInherited     = 0x00800000;
        private const int viewStateMode             = 0x01000000;
        private const int clientIDMode              = 0x06000000;
        private const int clientIDModeOffset        = 25;
        private const int effectiveClientIDMode     = 0x18000000;
        private const int effectiveClientIDModeOffset = 27;
        private const int validateRequestMode       = 0x60000000;
        private const int validateRequestModeOffset = 29;
        #pragma warning disable 0649
        internal SimpleBitVector32 flags;
        #pragma warning restore 0649

        private const string automaticIDPrefix = "ctl";
        private const string automaticLegacyIDPrefix = "_ctl";
        private const int automaticIDCount = 128;
        private static readonly string[] automaticIDs = new string [automaticIDCount] {
            "ctl00", "ctl01", "ctl02", "ctl03", "ctl04", "ctl05", "ctl06",
            "ctl07", "ctl08", "ctl09", "ctl10", "ctl11", "ctl12", "ctl13",
            "ctl14", "ctl15", "ctl16", "ctl17", "ctl18", "ctl19", "ctl20",
            "ctl21", "ctl22", "ctl23", "ctl24", "ctl25", "ctl26", "ctl27",
            "ctl28", "ctl29", "ctl30", "ctl31", "ctl32", "ctl33", "ctl34",
            "ctl35", "ctl36", "ctl37", "ctl38", "ctl39", "ctl40", "ctl41",
            "ctl42", "ctl43", "ctl44", "ctl45", "ctl46", "ctl47", "ctl48",
            "ctl49", "ctl50", "ctl51", "ctl52", "ctl53", "ctl54", "ctl55",
            "ctl56", "ctl57", "ctl58", "ctl59", "ctl60", "ctl61", "ctl62",
            "ctl63", "ctl64", "ctl65", "ctl66", "ctl67", "ctl68", "ctl69",
            "ctl70", "ctl71", "ctl72", "ctl73", "ctl74", "ctl75", "ctl76",
            "ctl77", "ctl78", "ctl79", "ctl80", "ctl81", "ctl82", "ctl83",
            "ctl84", "ctl85", "ctl86", "ctl87", "ctl88", "ctl89", "ctl90",
            "ctl91", "ctl92", "ctl93", "ctl94", "ctl95", "ctl96", "ctl97",
            "ctl98", "ctl99",
            "ctl100", "ctl101", "ctl102", "ctl103", "ctl104", "ctl105", "ctl106",
            "ctl107", "ctl108", "ctl109", "ctl110", "ctl111", "ctl112", "ctl113",
            "ctl114", "ctl115", "ctl116", "ctl117", "ctl118", "ctl119", "ctl120",
            "ctl121", "ctl122", "ctl123", "ctl124", "ctl125", "ctl126", "ctl127"

        };

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.Control'/> class.</para>
        /// </devdoc>
        public Control() {
            if (this is INamingContainer)
                flags.Set(isNamingContainer);
        }

        private ClientIDMode ClientIDModeValue {
            get {
                return (ClientIDMode)flags[clientIDMode, clientIDModeOffset];
            }
            set {
                flags[clientIDMode, clientIDModeOffset] = (int)value;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
        [
        DefaultValue(ClientIDMode.Inherit),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.Control_ClientIDMode)
        ]
        public virtual ClientIDMode ClientIDMode {
            get {
                return ClientIDModeValue;
            }
            set {
                if (ClientIDModeValue != value) {
                    if (value != EffectiveClientIDModeValue) {
                        ClearEffectiveClientIDMode();
                        ClearCachedClientID();
                    }
                    ClientIDModeValue = value;
                }
            }
        }

        private ClientIDMode EffectiveClientIDModeValue {
            get {
                return (ClientIDMode)flags[effectiveClientIDMode, effectiveClientIDModeOffset];
            }
            set {
                flags[effectiveClientIDMode, effectiveClientIDModeOffset] = (int)value;
            }
        }

        internal virtual ClientIDMode EffectiveClientIDMode {
            get {
                if (EffectiveClientIDModeValue == ClientIDMode.Inherit) {
                    EffectiveClientIDModeValue = ClientIDMode;
                    if (EffectiveClientIDModeValue == ClientIDMode.Inherit) {
                        if (NamingContainer != null) {
                            EffectiveClientIDModeValue = NamingContainer.EffectiveClientIDMode;
                        }
                        else {
                            HttpContext context = Context;
                            if (context != null) {
                                EffectiveClientIDModeValue = RuntimeConfig.GetConfig(context).Pages.ClientIDMode;
                            }
                            else {
                                EffectiveClientIDModeValue = RuntimeConfig.GetConfig().Pages.ClientIDMode;
                            }
                        }
                    }
                }
                return EffectiveClientIDModeValue;
            }
        }

        internal string UniqueClientID {
            get {
                string uniqueID = UniqueID;
                if(uniqueID != null && uniqueID.IndexOf(IdSeparator) >= 0) {
                    return uniqueID.Replace(IdSeparator, ID_RENDER_SEPARATOR);
                }
                return uniqueID;
            }
        }

        internal string StaticClientID {
            get {
                return flags[useGeneratedID] ? String.Empty : ID ?? String.Empty;
            }
        }

        internal ControlAdapter AdapterInternal {
            get {
                if (_occasionalFields == null ||
                    _occasionalFields.RareFields == null ||
                    _occasionalFields.RareFields.Adapter == null) {
                        return null;
                }
                return _occasionalFields.RareFields.Adapter;
            }
            set {
                if (value != null) {
                    RareFieldsEnsured.Adapter = value;
                }
                else {
                    if (_occasionalFields != null &&
                        _occasionalFields.RareFields != null &&
                        _occasionalFields.RareFields.Adapter != null) {
                            _occasionalFields.RareFields.Adapter = null;
                    }
                }
            }
        }
        
        private string GetClientID() {
            switch (EffectiveClientIDMode) {
                case ClientIDMode.Predictable:
                    return PredictableClientID;
                case ClientIDMode.Static:
                    return StaticClientID;
                default:
                    return UniqueClientID;
            }
        }

        private string GetPredictableClientIDPrefix() {
            string predictableIDPrefix;

            Control namingContainer = NamingContainer;
            if (namingContainer != null) {
                if (_id == null) {
                    GenerateAutomaticID();
                }
                if (namingContainer is Page || namingContainer is MasterPage) {
                    predictableIDPrefix = _id;
                }
                else {
                    predictableIDPrefix = namingContainer.GetClientID();
                    if (String.IsNullOrEmpty(predictableIDPrefix)) {
                        predictableIDPrefix = _id;
                    }
                    else {
                        if (!String.IsNullOrEmpty(_id) && (!(this is IDataItemContainer) || (this is IDataBoundItemControl))) {
                            predictableIDPrefix = predictableIDPrefix + ID_RENDER_SEPARATOR + _id;
                        }
                    }
                }
            }
            else {
                predictableIDPrefix = _id;
            }
            return predictableIDPrefix;
        }

        private string GetPredictableClientIDSuffix() {
            string predictableIDSuffix = null;

            Control dataItemContainer = DataItemContainer;
            if (dataItemContainer != null && 
                !(dataItemContainer is IDataBoundItemControl) &&
                (!(this is IDataItemContainer) || (this is IDataBoundItemControl))) {
                Control dataKeysContainer = dataItemContainer.DataKeysContainer;
                if (dataKeysContainer != null && (((IDataKeysControl)dataKeysContainer).ClientIDRowSuffix != null) && (((IDataKeysControl)dataKeysContainer).ClientIDRowSuffix.Length > 0)) {
                    predictableIDSuffix = String.Empty;
                    IOrderedDictionary dataKey = ((IDataKeysControl)dataKeysContainer).ClientIDRowSuffixDataKeys[((IDataItemContainer)dataItemContainer).DisplayIndex].Values;
                    foreach (string suffixName in ((IDataKeysControl)dataKeysContainer).ClientIDRowSuffix) {
                        predictableIDSuffix = predictableIDSuffix + ID_RENDER_SEPARATOR + dataKey[suffixName].ToString();
                    }
                }
                else {
                    int index = ((IDataItemContainer)dataItemContainer).DisplayIndex;
                    if (index >= 0) {
                        predictableIDSuffix = ID_RENDER_SEPARATOR + index.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
            return predictableIDSuffix;
        }

        internal string PredictableClientID {
            get {
                if (_cachedPredictableID != null) {
                    return _cachedPredictableID;
                }

                _cachedPredictableID = GetPredictableClientIDPrefix();
                string suffixID = GetPredictableClientIDSuffix();

                // Concatenates Predictable clientID and ClientIDRowSuffix if available
                if (!String.IsNullOrEmpty(suffixID)) {
                    if (!String.IsNullOrEmpty(_cachedPredictableID)) {
                        _cachedPredictableID = _cachedPredictableID + suffixID;
                    }
                    else {
                        _cachedPredictableID = suffixID.Substring(1);
                    }
                }
                return String.IsNullOrEmpty(_cachedPredictableID) ? String.Empty : _cachedPredictableID;
            }
        }

        /// <devdoc>
        ///    <para>Indicates the control identifier generated by the ASP.NET framework. </para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.Control_ClientID)
        ]
        public virtual string ClientID {
            // This property is required to render a unique client-friendly id.
            get {
                if (EffectiveClientIDMode != ClientIDMode.Static) {
                    // Ensure that ID is set. The assumption being made is that the caller
                    // is likely to use the client ID in script, and to support that the
                    // control should render out an ID attribute
                    EnsureID();
                }
                return GetClientID();
            }
        }

        protected char ClientIDSeparator {
            get {
                return ID_RENDER_SEPARATOR;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        WebSysDescription(SR.Control_OnDisposed)
        ]
        public event EventHandler Disposed {
            add {
                Events.AddHandler(EventDisposed, value);
            }
            remove {
                Events.RemoveHandler(EventDisposed, value);
            }
        }

        /// <devdoc>
        /// <para>Gets the <see langword='HttpContext'/> object of the current Web request. If
        ///    the control's context is <see langword='null'/>, this will be the context of the
        ///    control's parent, unless the parent control's context is <see langword='null'/>.
        ///    If this is the case, this will be equal to the HttpContext property.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        protected internal virtual HttpContext Context {
            //  Request context containing the intrinsics
            get {
                Page page = Page;
                if(page != null) {
                    return page.Context;
                }
                return HttpContext.Current;
            }
        }

        protected virtual ControlAdapter ResolveAdapter() {
            if(flags[controlAdapterResolved]) {
                return AdapterInternal;
            }
            if (DesignMode) {
                flags.Set(controlAdapterResolved);
                return null;
            }

            HttpContext context = Context;
            if (context != null && context.Request.Browser != null) {
                AdapterInternal = context.Request.Browser.GetAdapter(this);
            }
            flags.Set(controlAdapterResolved);
            return AdapterInternal;
        }

        /// <devdoc>
        ///    <para>Indicates the list of event handler delegates for the control. This property
        ///       is read-only.</para>
        /// </devdoc>
        protected ControlAdapter Adapter {
            get {
                if(flags[controlAdapterResolved]) {
                    return AdapterInternal;
                }
                AdapterInternal = ResolveAdapter();
                flags.Set(controlAdapterResolved);
                return AdapterInternal;
            }
        }

        /// <devdoc>
        /// Indicates whether a control is being used in the context of a design surface.
        /// </devdoc>
        protected internal bool DesignMode {
            get {
                if(!flags[designModeChecked]) {
                    Page page = Page;
                    if(page != null )  {
                        if(page.GetDesignModeInternal()) {
                            flags.Set(designMode);
                        }
                        else {
                            flags.Clear(designMode);
                        }
                    }
                    else {
                        if(Site != null) {
                            if(Site.DesignMode) {
                                flags.Set(designMode);
                            }
                            else {
                                flags.Clear(designMode);
                            }
                        }
                        else if (Parent != null) {
                            if(Parent.DesignMode) {
                                flags.Set(designMode);
                            }

                            // VSWhidbey 535747: If Page, Site and Parent are all null, do not change the 
                            // designMode flag since it might had been previously set by the controlBuilder.
                            // This does not affect runtime since designMode is by-default false.
                            /*
                            else {
                                flags.Clear(designMode);
                            }
                            */
                        }
                    }
                    flags.Set(designModeChecked);
                }
                return flags[designMode];

            }
        }

        // Helper function to call validateEvent.
        internal void ValidateEvent(string uniqueID) {
            ValidateEvent(uniqueID, String.Empty);
        }

        // Helper function to call validateEvent.
        internal void ValidateEvent(string uniqueID, string eventArgument) {
            if (Page != null && SupportsEventValidation) {
                Page.ClientScript.ValidateEvent(uniqueID, eventArgument);
            }
        }

        // Indicates whether the control supports event validation
        // By default, all web controls in System.Web assembly supports it but not custom controls.
        private bool SupportsEventValidation {
            get {
                return SupportsEventValidationAttribute.SupportsEventValidation(this.GetType());
            }
        }

        /// <devdoc>
        ///    <para>Indicates the list of event handler delegates for the control. This property
        ///       is read-only.</para>
        /// </devdoc>
        protected EventHandlerList Events {
            get {
                if (_events == null) {
                    _events = new EventHandlerList();
                }
                return _events;
            }
        }

        protected bool HasEvents() {
            return (_events != null);
        }

        /// <devdoc>
        ///    <para> Gets or sets the identifier for the control. Setting the
        ///       property on a control allows programmatic access to the control's properties. If
        ///       this property is not specified on a control, either declaratively or
        ///       programmatically, then you cannot write event handlers and the like for the control.</para>
        /// </devdoc>
        [
        ParenthesizePropertyName(true),
        MergableProperty(false),
        Filterable(false),
        Themeable(false),
        WebSysDescription(SR.Control_ID)
        ]
        public virtual string ID {
            get {
                if (!flags[idNotCalculated] && !flags[mustRenderID]) {
                    return null;
                }
                return _id;
            }
            set {
                // allow the id to be unset
                if (value != null && value.Length == 0)
                    value = null;

                string oldID = _id;

                _id = value;
                ClearCachedUniqueIDRecursive();
                flags.Set(idNotCalculated);
                flags.Clear(useGeneratedID);

                // Update the ID in the naming container
                if ((_namingContainer != null) && (oldID != null)) {
                    _namingContainer.DirtyNameTable();
                }

                if (oldID != null && oldID != _id) {
                    ClearCachedClientID();
                }
            }
        }


        /// <devdoc>
        ///    <para>Gets and sets a value indicating whether theme is enabled.</para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(true),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.Control_EnableTheming)
        ]
        public virtual bool EnableTheming {
            get {
                if (flags[enableThemingSet]) {
                    return !flags[disableTheming];
                }

                if (Parent != null) {
                    return Parent.EnableTheming;
                }

                return !flags[disableTheming];
            }
            set {
                if ((_controlState >= ControlState.FrameworkInitialized) && !DesignMode) {
                    throw new InvalidOperationException(SR.GetString(SR.PropertySetBeforePreInitOrAddToControls, "EnableTheming"));
                }

                if(!value) {
                    flags.Set(disableTheming);
                }
                else {
                    flags.Clear(disableTheming);
                }

                flags.Set(enableThemingSet);
            }
        }

        // Serialzie the value if it's set explicitely.
        internal bool ShouldSerializeEnableTheming() {
            return flags[enableThemingSet];;
        }

        internal bool IsBindingContainer {
            get {
                return this is INamingContainer && !(this is INonBindingContainer);
            }
        }

        protected internal bool IsChildControlStateCleared {
            get {
                return flags[disableChildControlState];
            }
        }


        /// <devdoc>
        ///    <para>Gets and sets the skinID of the control.</para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(""),
        Filterable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.Control_SkinId),
        ]
        public virtual string SkinID {
            get {
                if(_occasionalFields != null) {
                    return _occasionalFields.SkinId == null ? String.Empty : _occasionalFields.SkinId;
                }
                return String.Empty;
            }
            set {
                if (!DesignMode) {
                    if (flags[styleSheetApplied]) {
                        throw new InvalidOperationException(SR.GetString(SR.PropertySetBeforeStyleSheetApplied, "SkinId"));
                    }

                    if (_controlState >= ControlState.FrameworkInitialized) {
                        throw new InvalidOperationException(SR.GetString(SR.PropertySetBeforePreInitOrAddToControls, "SkinId"));
                    }
                }

                EnsureOccasionalFields();
                _occasionalFields.SkinId = value;
            }
        }

        private ControlRareFields RareFieldsEnsured {
            get {
                EnsureOccasionalFields();
                ControlRareFields rareFields = _occasionalFields.RareFields;
                if(rareFields == null) {
                    rareFields = new ControlRareFields();
                    _occasionalFields.RareFields = rareFields;
                }

                return rareFields;
            }
        }

        private ControlRareFields RareFields {
            get {
                if(_occasionalFields != null) {
                    return _occasionalFields.RareFields;
                }
                return null;
            }
        }

        private void EnsureOccasionalFields() {
            if(_occasionalFields == null) {
                _occasionalFields = new OccasionalFields();
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether the control should maintain its view
        ///       state, and the view state of any child control in contains, when the current
        ///       page request ends.
        ///    </para>
        /// </devdoc>
        [
        DefaultValue(EnableViewStateDefault),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.Control_MaintainState)
        ]
        public virtual bool EnableViewState {
            get {
                return !flags[disableViewState];
            }
            set {
                SetEnableViewStateInternal(value);
            }
        }

        [
        DefaultValue(ViewStateMode.Inherit),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.Control_ViewStateMode)
        ]
        public virtual ViewStateMode ViewStateMode {
            get {
                return flags[viewStateNotInherited] ?
                    (flags[viewStateMode] ? ViewStateMode.Enabled : ViewStateMode.Disabled) :
                    ViewStateMode.Inherit;
            }
            set {
                if ((value < ViewStateMode.Inherit) || (value > ViewStateMode.Disabled)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value == ViewStateMode.Inherit) {
                    flags.Clear(viewStateNotInherited);
                }
                else {
                    flags.Set(viewStateNotInherited);
                    if (value == ViewStateMode.Enabled) { 
                        flags.Set(viewStateMode);
                    }
                    else {
                        flags.Clear(viewStateMode);
                    }
                }
            }
        }


        internal void SetEnableViewStateInternal(bool value) {
            if (!value)
                flags.Set(disableViewState);
            else
                flags.Clear(disableViewState);
        }


        /// <devdoc>
        /// Gets a value indicating whether the control is maintaining its view
        /// state, when the current page request ends by looking at its own EnableViewState
        /// value, and the value for all its parents.
        /// </devdoc>
        protected internal bool IsViewStateEnabled {
            get {
                Control current = this;
                while (current != null) {
                    if (current.EnableViewState == false) {
                        return false;
                    }
                    ViewStateMode mode = current.ViewStateMode;
                    if (mode != ViewStateMode.Inherit) {
                        return (mode == ViewStateMode.Enabled);
                    }
                    current = current.Parent;
                }
                return true;
            }
        }


        /// <devdoc>
        ///    <para>Gets the reference to the current control's naming container.</para>
        /// </devdoc>
        [
        Bindable(false),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.Control_NamingContainer)
        ]
        public virtual Control NamingContainer {
            get {
                if (_namingContainer == null) {
                    if (Parent != null) {
                        // Search for the closest naming container in the tree
                        if (Parent.flags[isNamingContainer])
                            _namingContainer = Parent;
                        else
                            _namingContainer = Parent.NamingContainer;
                    }
                }

                return _namingContainer;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Returns the databinding container of this control.  In most cases,
        ///     this is the same as the NamingContainer. But when using LoadTemplate(),
        ///     we get into a situation where that is not the case (ASURT 94138)</para>
        ///     The behavior is different than V1 that Usercontrol.BindingContainer is no
        ///     longer the UserControl but the control contains it. The behavior is consistent
        ///     with LoadTemplate() case.
        /// </devdoc>
        [
        Bindable(false),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public Control BindingContainer {
            get {
                Control bindingContainer = NamingContainer;
                while (bindingContainer is INonBindingContainer) {
                    bindingContainer = bindingContainer.BindingContainer;
                }

                return bindingContainer;
            }
        }

        [
        Bindable(false),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public Control DataItemContainer {
            get {
                Control dataItemContainer = NamingContainer;
                while (dataItemContainer != null && !(dataItemContainer is IDataItemContainer)) {
                    dataItemContainer = dataItemContainer.DataItemContainer;
                }

                return dataItemContainer;
            }
        }

        [
        Bindable(false),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public Control DataKeysContainer {
            get {
                Control dataKeysContainer = NamingContainer;
                while (dataKeysContainer != null && !(dataKeysContainer is IDataKeysControl)) {
                    dataKeysContainer = dataKeysContainer.DataKeysContainer;
                }

                return dataKeysContainer;
            }
        }



        /// <internalonly/>
        /// <devdoc>
        /// VSWhidbey 80467: Need to adapt id separator.
        /// </devdoc>
        protected char IdSeparator {
            get {
                if (Page != null) {
                    return Page.IdSeparator;
                }
                return IdSeparatorFromConfig;
            }
        }

        // VSWhidbey 475945: Use the old id separator if configured
        internal char IdSeparatorFromConfig {
            get {
                return ((EnableLegacyRendering) ? LEGACY_ID_SEPARATOR : ID_SEPARATOR);
            }
        }

        // VSWhidbey 244374: Allow controls to opt into loading view state by ID instead of index (perf hit)
        protected bool LoadViewStateByID {
            get {
                return ViewStateModeByIdAttribute.IsEnabled(GetType());
            }
        }

        /// <devdoc>
        /// <para> Gets the <see cref='System.Web.UI.Page'/> object that contains the
        ///    current control.</para>
        /// </devdoc>
        [
        Bindable(false),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.Control_Page)
        ]
        public virtual Page Page {
            get {
                if (_page == null) {
                    if (Parent != null) {
                        _page = Parent.Page;
                    }
                }
                return _page;
            }

            set {
                if (OwnerControl != null) {
                    throw new InvalidOperationException();
                }
                // This is necessary because we need to set the page in generated
                // code before controls are added to the tree (ASURT 75330)
                Debug.Assert(_page == null);
                Debug.Assert(Parent == null || Parent.Page == null);
                _page = value;
            }
        }

        internal RouteCollection RouteCollection {
            get {
                if (_occasionalFields == null ||
                    _occasionalFields.RareFields == null ||
                    _occasionalFields.RareFields.RouteCollection == null) {
                        return RouteTable.Routes;
                }
                return _occasionalFields.RareFields.RouteCollection;
            }
            set {
                if (value != null) {
                    RareFieldsEnsured.RouteCollection = value;
                }
                else {
                    if (_occasionalFields != null &&
                        _occasionalFields.RareFields != null &&
                        _occasionalFields.RareFields.RouteCollection != null) {
                            _occasionalFields.RareFields.RouteCollection = null;
                    }
                }
            }
        }

        // VSWhidbey 244999
        internal virtual bool IsReloadable {
            get {
                return false;
            }
        }

        // DevDiv 33149, 43258: A backward compat. switch for Everett rendering
        internal bool EnableLegacyRendering {
            get {
                Page page = Page;
                if (page != null) {
                    return (page.XhtmlConformanceMode == XhtmlConformanceMode.Legacy);
                }
                else if (DesignMode || Adapter != null) {
                    return false;
                }
                else {
                    return (GetXhtmlConformanceSection().Mode == XhtmlConformanceMode.Legacy);
                }
            }
        }

        internal XhtmlConformanceSection GetXhtmlConformanceSection() {
            HttpContext context = Context;
            XhtmlConformanceSection xhtmlConformanceSection;
            if (context != null) {
                // if context is available, use the most efficient way to get the section
                xhtmlConformanceSection = RuntimeConfig.GetConfig(context).XhtmlConformance;
            }
            else {
                xhtmlConformanceSection = RuntimeConfig.GetConfig().XhtmlConformance;
            }
            Debug.Assert(xhtmlConformanceSection != null);
            return xhtmlConformanceSection;
        }

        [
        Bindable(false),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual Version RenderingCompatibility {
            get {
                if (_occasionalFields == null ||
                    _occasionalFields.RareFields == null ||
                    _occasionalFields.RareFields.RenderingCompatibility == null) {
                        return RuntimeConfig.Pages.ControlRenderingCompatibilityVersion;
                }
                return _occasionalFields.RareFields.RenderingCompatibility;
            }
            set {
                if (value != null) {
                    RareFieldsEnsured.RenderingCompatibility = value;
                }
                else {
                    if (_occasionalFields != null &&
                        _occasionalFields.RareFields != null &&
                        _occasionalFields.RareFields.RenderingCompatibility != null) {
                            _occasionalFields.RareFields.RenderingCompatibility = null;
                    }
                }
            }
        }

        private RuntimeConfig RuntimeConfig {
            get {
                HttpContext context = Context;
                if (context != null) {
                    // if context is available, use the most efficient way to get the config
                    return RuntimeConfig.GetConfig(context);
                } else {
                    return RuntimeConfig.GetConfig();
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "Consistent with other URL properties in ASP.NET.")]
        public string GetRouteUrl(object routeParameters) {
            return GetRouteUrl(new RouteValueDictionary(routeParameters));
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "Consistent with other URL properties in ASP.NET.")]
        public string GetRouteUrl(string routeName, object routeParameters) {
            return GetRouteUrl(routeName, new RouteValueDictionary(routeParameters));
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "Consistent with other URL properties in ASP.NET.")]
        public string GetRouteUrl(RouteValueDictionary routeParameters) {
            return GetRouteUrl(null, routeParameters);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "Consistent with other URL properties in ASP.NET.")]
        public string GetRouteUrl(string routeName, RouteValueDictionary routeParameters) {
            VirtualPathData data = RouteCollection.GetVirtualPath(Context.Request.RequestContext, routeName, routeParameters);
            if (data != null) {
                return data.VirtualPath;
            }
            return null;
        }

        /// <devdoc>
        ///    <para>Gets the reference to the <see cref='System.Web.UI.TemplateControl'/>
        ///    that hosts the control.</para>
        /// </devdoc>
        internal virtual TemplateControl GetTemplateControl() {
            if (_occasionalFields == null || _occasionalFields.TemplateControl == null) {
                if (Parent != null) {
                    TemplateControl templateControl = Parent.GetTemplateControl();
                    if (templateControl != null) {
                        EnsureOccasionalFields();
                        _occasionalFields.TemplateControl = templateControl;
                    }
                }
            }
            return (_occasionalFields != null) ? _occasionalFields.TemplateControl : null;
        }


        /// <devdoc>
        ///    <para>Gets the reference to the <see cref='System.Web.UI.TemplateControl'/>
        ///    that hosts the control.</para>
        /// </devdoc>
        [
        Bindable(false),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.Control_TemplateControl)
        ]
        public TemplateControl TemplateControl {
            get {
                return GetTemplateControl();
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            set {
                // This setter is necessary so that controls inside templates are based on
                // hosting pages not where the templates are used.
                if (value != null) {
                    EnsureOccasionalFields();
                    _occasionalFields.TemplateControl = value;
                }
                else {
                    if (_occasionalFields != null &&
                        _occasionalFields.TemplateControl != null) {
                            _occasionalFields.TemplateControl = null;
                    }
                }
            }
        }

        /*
         * Determine whether this control is a descendent of the passed in control
         */
        internal bool IsDescendentOf(Control ancestor) {
            Control current = this;
            while (current != ancestor && current.Parent != null) {
                current = current.Parent;
            }
            return (current == ancestor);
        }


        /// <devdoc>
        ///    <para> Gets the current control's parent control in the UI hierarchy.</para>
        /// </devdoc>
        [
        Bindable(false),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.Control_Parent)
        ]
        public virtual Control Parent {
            get {
                return _parent;
            }
        }

        internal bool IsParentedToUpdatePanel {
            get {
                Control parent = Parent;
                while (parent != null) {
                    if (parent is IUpdatePanel) {
                        return true;
                    }
                    parent = parent.Parent;
                }
                return false;
            }
        }

        /// <devdoc>
        ///    <para> Gets the virtual directory of the Page or UserControl that contains this control.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.Control_TemplateSourceDirectory)
        ]
        public virtual string TemplateSourceDirectory {
            get {
                if (TemplateControlVirtualDirectory == null)
                    return String.Empty;

                return TemplateControlVirtualDirectory.VirtualPathStringNoTrailingSlash;
            }
        }


        /// <devdoc>
        ///    <para> Gets the virtual directory of the Page or UserControl that contains this control.
        ///         Unlike TemplateSourceDirectory, this returns an app relative path (e.g. "~/sub")</para>
        /// </devdoc>
        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.Control_TemplateSourceDirectory)
        ]
        public string AppRelativeTemplateSourceDirectory {
            get {
                return VirtualPath.GetAppRelativeVirtualPathStringOrEmpty(TemplateControlVirtualDirectory);
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            set {
                // This setter is necessary so that skins are based on hosting skin file.
                this.TemplateControlVirtualDirectory = VirtualPath.CreateNonRelativeAllowNull(value);
            }
        }

        internal VirtualPath TemplateControlVirtualDirectory {
            get {
                if (_occasionalFields != null && _occasionalFields.TemplateSourceVirtualDirectory != null)
                    return _occasionalFields.TemplateSourceVirtualDirectory;

                TemplateControl control = TemplateControl;
                if (control == null) {
                    HttpContext context = Context;
                    if (context != null) {
                        VirtualPath templateSourceVirtualDirectory = context.Request.CurrentExecutionFilePathObject.Parent;
                        if (templateSourceVirtualDirectory != null) {
                            EnsureOccasionalFields();
                            _occasionalFields.TemplateSourceVirtualDirectory = templateSourceVirtualDirectory;
                        }
                    }
                    return (_occasionalFields != null) ? _occasionalFields.TemplateSourceVirtualDirectory : null;
                }
                // Prevent recursion if this is the TemplateControl
                if (control != this) {
                    VirtualPath templateSourceVirtualDirectory = control.TemplateControlVirtualDirectory;
                    if (templateSourceVirtualDirectory != null) {
                        EnsureOccasionalFields();
                        _occasionalFields.TemplateSourceVirtualDirectory = templateSourceVirtualDirectory;
                    }
                }
                return (_occasionalFields != null) ? _occasionalFields.TemplateSourceVirtualDirectory : null;
            }

            set {
                // This setter is necessary so that skins are based on hosting skin file.
                if (value != null) {
                    EnsureOccasionalFields();
                    _occasionalFields.TemplateSourceVirtualDirectory = value;
                }
                else {
                    if (_occasionalFields != null &&
                        _occasionalFields.TemplateSourceVirtualDirectory != null) {
                            _occasionalFields.TemplateSourceVirtualDirectory = null;
                    }
                }
            }
        }

        internal ControlState ControlState {
            get { return _controlState; }
            set { _controlState = value; }
        }


        /// <devdoc>
        ///    <para>Indicates the site information for the control.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Advanced),
        WebSysDescription(SR.Control_Site)
        ]
        public ISite Site {
            get {
                if (OwnerControl != null) {
                    return OwnerControl.Site;
                }

                if (RareFields != null) {
                    return RareFields.Site;
                }
                return null;
            }
            set {
                if (OwnerControl != null) {
                    throw new InvalidOperationException(SR.GetString(SR.Substitution_SiteNotAllowed));
                }

                RareFieldsEnsured.Site = value;
                flags.Clear(designModeChecked);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value that indicates whether a control should be rendered on
        ///       the page.
        ///    </para>
        /// </devdoc>
        [
        Bindable(true),
        DefaultValue(true),
        WebCategory("Behavior"),
        WebSysDescription(SR.Control_Visible)
        ]
        public virtual bool Visible {
            get {
                if (flags[invisible])
                    return false;
                else if ((_parent != null) && !DesignMode)
                    return _parent.Visible;
                else
                    return true;
            }
            set {
                if (flags[marked]) {
                    bool visible = !flags[invisible];
                    if (visible != value) {
                        flags.Set(visibleDirty);
                    }
                }

                if(!value) {
                    flags.Set(invisible);
                }
                else {
                    flags.Clear(invisible);
                }
            }
        }


        /// <devdoc>
        /// Do not remove or change the signature. It is called via reflection.
        /// This allows for correct serialization, since Visible is implemented as a
        /// recursive property.
        /// </devdoc>
        private void ResetVisible() {
            Visible = true;
        }


        /// <devdoc>
        /// Do not remove or change the signature. It is called via reflection.
        /// This allows for correct serialization, since Visible is implemented as a
        /// recursive property.
        /// </devdoc>
        private bool ShouldSerializeVisible() {
            return flags[invisible];
        }


        /// <devdoc>
        ///    <para> Gets the unique, hierarchically-qualified identifier for
        ///       a control. This is different from the ID property, in that the fully-qualified
        ///       identifier includes the identifier for the control's naming container.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.Control_UniqueID)
        ]
        public virtual string UniqueID {
            get {
                if (_cachedUniqueID != null) {
                    return _cachedUniqueID;
                }

                Control namingContainer = NamingContainer;
                if (namingContainer != null) {
                    // if the ID is null at this point, we need to have one created and the control added to the
                    // naming container.
                    if (_id == null) {
                        GenerateAutomaticID();
                    }

                    if (Page == namingContainer) {
                        _cachedUniqueID = _id;
                    }
                    else {
                        string uniqueIDPrefix = namingContainer.GetUniqueIDPrefix();
                        if (uniqueIDPrefix.Length == 0) {
                            // In this case, it is probably a naming container that is not sited, so we don't want to cache it
                            return _id;
                        }
                        else {
                            _cachedUniqueID = uniqueIDPrefix + _id;
                        }
                    }

                    return _cachedUniqueID;
                }
                else {
                    // no naming container
                    return _id;
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID", Justification="This is consistent with UniqueID")]
        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member", Justification="This is consistent with UniqueID")]
        public string GetUniqueIDRelativeTo(Control control) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }

            if (!IsDescendentOf(control.NamingContainer)) {
                throw new InvalidOperationException(SR.GetString(SR.Control_NotADescendentOfNamingContainer, control.ID));
            }

            if (control.NamingContainer == Page) {
                return UniqueID;
            } else {
                return UniqueID.Substring(control.NamingContainer.UniqueID.Length + 1); // add 1 for the ID seperator (which is a char)
            }
        }

        /// <devdoc>
        ///    <para>Occurs when the control binds to a data source. Notifies the control to perform any data binding during this event.</para>
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.Control_OnDataBind)
        ]
        public event EventHandler DataBinding {
            add {
                Events.AddHandler(EventDataBinding, value);
            }
            remove {
                Events.RemoveHandler(EventDataBinding, value);
            }
        }


        /// <devdoc>
        ///    <para>Occurs when the control is initialized, the first step in the page lifecycle. Controls should
        ///       perform any initialization steps that are required to create and set up an
        ///       instantiation.</para>
        /// </devdoc>
        [
        WebSysDescription(SR.Control_OnInit)
        ]
        public event EventHandler Init {
            add {
                Events.AddHandler(EventInit, value);
            }
            remove {
                Events.RemoveHandler(EventInit, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the control is loaded to the <see cref='System.Web.UI.Page'/> object. Notifies the control to perform any steps that
        ///    need to occur on each page request.</para>
        /// </devdoc>
        [
        WebSysDescription(SR.Control_OnLoad)
        ]
        public event EventHandler Load {
            add {
                Events.AddHandler(EventLoad, value);
            }
            remove {
                Events.RemoveHandler(EventLoad, value);
            }
        }


        /// <devdoc>
        ///    <para>Occurs when the control is about to render. Controls
        ///       should perform any pre-rendering steps necessary before saving view state and
        ///       rendering content to the <see cref='System.Web.UI.Page'/> object.</para>
        /// </devdoc>
        [
        WebSysDescription(SR.Control_OnPreRender)
        ]
        public event EventHandler PreRender {
            add {
                Events.AddHandler(EventPreRender, value);
            }
            remove {
                Events.RemoveHandler(EventPreRender, value);
            }
        }


        /// <devdoc>
        ///    <para>Occurs when the control is unloaded from memory. Controls should perform any
        ///       final cleanup before this instance of it is </para>
        /// </devdoc>
        [
        WebSysDescription(SR.Control_OnUnload)
        ]
        public event EventHandler Unload {
            add {
                Events.AddHandler(EventUnload, value);
            }
            remove {
                Events.RemoveHandler(EventUnload, value);
            }
        }

        /// <devdoc>
        /// <para>Apply stylesheet skin on the control.</para>
        /// </devdoc>
        [
        EditorBrowsable(EditorBrowsableState.Advanced),
        ]
        public virtual void ApplyStyleSheetSkin(Page page) {
            // Nothing to do if the control is not in a Page.
            if (page == null) {
                return;
            }

            // Only apply stylesheet if not already applied.
            if (flags[styleSheetApplied]) {
                throw new InvalidOperationException(SR.GetString(SR.StyleSheetAreadyAppliedOnControl));
            }

            if (page.ApplyControlStyleSheet(this)) {
                flags.Set(styleSheetApplied);
            }
        }

        /// <devdoc>
        /// <para>Apply theme on the control.</para>
        /// </devdoc>
        private void ApplySkin(Page page) {
            if (page == null) {
                throw new ArgumentNullException("page");
            }

            if (flags[themeApplied]) {
                return;
            }

            if (ThemeableAttribute.IsTypeThemeable(this.GetType())) {
                page.ApplyControlSkin(this);
                flags.Set(themeApplied);
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='DataBinding'/> event. This
        ///    notifies a control to perform any data binding logic that is associated with it.</para>
        /// </devdoc>
        protected virtual void OnDataBinding(EventArgs e) {
            if(HasEvents()) {
                EventHandler handler = _events[EventDataBinding] as EventHandler;
                if(handler != null) {
                    handler(this, e);
                }
            }
        }


        /// <devdoc>
        ///    <para> Causes data binding to occur on the invoked control and all of its child
        ///       controls.</para>
        /// </devdoc>
        public virtual void DataBind() {
            DataBind(true);
        }

        /// <devdoc>
        ///    <para> Causes the invoked controls' context to be pushed on the stack,
        ///       then conditionally call OnDataBinging on the invoked control, and databind all of its child
        ///       controls.  A control would call this with false if it overrides DataBind without calling
        ///       Control.DataBind, but still wants to be an IDataItemContainer.  FormView and DetailsView
        ///       are good examples of this.</para>
        /// </devdoc>
        protected virtual void DataBind(bool raiseOnDataBinding) {
            bool inDataBind = false;

            if (IsBindingContainer) {
                bool foundDataItem;

                object dataItem = DataBinder.GetDataItem(this, out foundDataItem);

                if (foundDataItem && (Page != null)) {
                    Page.PushDataBindingContext(dataItem);
                    inDataBind = true;
                }
            }
            try {
                if (raiseOnDataBinding) {
                    // Do our own databinding
                    OnDataBinding(EventArgs.Empty);
                }

                // Do all of our children's databinding
                DataBindChildren();
            }
            finally {
                if (inDataBind) {
                    Page.PopDataBindingContext();
                }
            }
        }


        /// <devdoc>
        /// <para> Causes data binding to occur on all of the child controls.</para>
        /// </devdoc>
        protected virtual void DataBindChildren() {
            if (HasControls()) {
                string oldmsg = _controls.SetCollectionReadOnly(SR.Parent_collections_readonly);

                try {
                    try {
                        int controlCount = _controls.Count;
                        for (int i=0; i < controlCount; i++)
                            _controls[i].DataBind();
                    }
                    finally {
                        _controls.SetCollectionReadOnly(oldmsg);
                    }
                }
                catch {
                    throw;
                }
            }
        }

        internal void PreventAutoID() {
            // controls that are also naming containers must always get an id
            if (flags[isNamingContainer] == false) {
                flags.Set(idNotRequired);
            }
        }


        /// <devdoc>
        ///    <para>Notifies the control that an element, XML or HTML, was parsed, and adds it to
        ///       the control.</para>
        /// </devdoc>
        protected virtual void AddParsedSubObject(object obj) {
            Control control = obj as Control;
            if (control != null) {
                Controls.Add(control);
            }
        }

        private void UpdateNamingContainer(Control namingContainer) {
            // Remove the cached uniqueID if the control already had a namingcontainer
            // and the namingcontainer is changed.
            if (_namingContainer == null || (_namingContainer != null && _namingContainer != namingContainer)) {
                ClearCachedUniqueIDRecursive();
            }

            // No need to clear the cache if never been initialized
            if (EffectiveClientIDModeValue != ClientIDMode.Inherit) {
                ClearCachedClientID();
                ClearEffectiveClientIDMode();
            }

            _namingContainer = namingContainer;
        }

        private void ClearCachedUniqueIDRecursive() {
            _cachedUniqueID = null;

            if (_occasionalFields != null) {
                _occasionalFields.UniqueIDPrefix = null;
            }

            if (_controls != null) {
                int controlCount = _controls.Count;
                for (int i = 0; i < controlCount; i++) {
                    _controls[i].ClearCachedUniqueIDRecursive();
                }
            }
        }

        protected void EnsureID() {
            if (_namingContainer != null) {
                if (_id == null) {
                    GenerateAutomaticID();
                }
                flags.Set(mustRenderID);
            }
        }

        private void GenerateAutomaticID() {
            Debug.Assert(_namingContainer != null);
            Debug.Assert(_id == null);

            // Remember that a generated ID is used for this control.
            flags.Set(useGeneratedID);

            // Calculate the automatic ID. For performance and memory reasons
            // we look up a static table entry if possible
            _namingContainer.EnsureOccasionalFields();
            int idNo = _namingContainer._occasionalFields.NamedControlsID++;
            if (EnableLegacyRendering) {
                // VSWhidbey 517118
                _id = automaticLegacyIDPrefix + idNo.ToString(NumberFormatInfo.InvariantInfo);
            }
            else {
                if (idNo < automaticIDCount) {
                    _id = automaticIDs[idNo];
                }
                else {
                    _id = automaticIDPrefix + idNo.ToString(NumberFormatInfo.InvariantInfo);
                }
            }

            _namingContainer.DirtyNameTable();
        }

        internal virtual string GetUniqueIDPrefix() {
            EnsureOccasionalFields();

            if (_occasionalFields.UniqueIDPrefix == null) {
                string uniqueID = UniqueID;
                if (!String.IsNullOrEmpty(uniqueID)) {
                    _occasionalFields.UniqueIDPrefix = uniqueID + IdSeparator;
                }
                else {
                    _occasionalFields.UniqueIDPrefix = String.Empty;
                }
            }

            return _occasionalFields.UniqueIDPrefix;
        }

        /// <devdoc>
        /// <para>Raises the <see langword='Init'/> event. This notifies the control to perform
        ///    any steps necessary for its creation on a page request.</para>
        /// </devdoc>
        protected internal virtual void OnInit(EventArgs e) {
            if(HasEvents()) {
                EventHandler handler = _events[EventInit] as EventHandler;
                if(handler != null) {
                    handler(this, e);
                }
            }
        }

        // !! IMPORTANT !!
        // If you make changes to this method, also change InitRecursiveAsync.
        internal virtual void InitRecursive(Control namingContainer) {
            ResolveAdapter();
            if (_controls != null) {
                if (flags[isNamingContainer]) {
                    namingContainer = this;
                }
                string oldmsg = _controls.SetCollectionReadOnly(SR.Parent_collections_readonly);

                int controlCount = _controls.Count;
                for (int i = 0; i < controlCount; i++) {
                    Control control = _controls[i];

                    // Propagate the page and namingContainer
                    control.UpdateNamingContainer(namingContainer);

                    if ((control._id == null) && (namingContainer != null) && !control.flags[idNotRequired]) {
                        control.GenerateAutomaticID();
                    }
                    control._page = Page;

                    control.InitRecursive(namingContainer);
                }
                _controls.SetCollectionReadOnly(oldmsg);

            }

            // Only make the actual call if it hasn't already happened (ASURT 111303)
            if (_controlState < ControlState.Initialized) {
                _controlState = ControlState.ChildrenInitialized; // framework also initialized

                if ((Page != null) && !DesignMode) {
                    if (Page.ContainsTheme && EnableTheming) {
                        ApplySkin(Page);
                    }
                }

                if (AdapterInternal != null) {
                    AdapterInternal.OnInit(EventArgs.Empty);
                }
                else {
                    OnInit(EventArgs.Empty);
                }

                _controlState = ControlState.Initialized;
            }

            // track all subsequent state changes
            TrackViewState();

#if DEBUG
            ControlInvariant();
#endif
        }

        // TAP version of InitRecursive
        // !! IMPORTANT !!
        // If you make changes to this method, also change InitRecursive.
        internal async Task InitRecursiveAsync(Control namingContainer, Page page) {
            ResolveAdapter();
            if (_controls != null) {
                if (flags[isNamingContainer]) {
                    namingContainer = this;
                }
                string oldmsg = _controls.SetCollectionReadOnly(SR.Parent_collections_readonly);

                int controlCount = _controls.Count;
                for (int i = 0; i < controlCount; i++) {
                    Control control = _controls[i];

                    // Propagate the page and namingContainer
                    control.UpdateNamingContainer(namingContainer);

                    if ((control._id == null) && (namingContainer != null) && !control.flags[idNotRequired]) {
                        control.GenerateAutomaticID();
                    }
                    control._page = Page;

                    control.InitRecursive(namingContainer);
                }
                _controls.SetCollectionReadOnly(oldmsg);

            }

            // Only make the actual call if it hasn't already happened (ASURT 111303)
            if (_controlState < ControlState.Initialized) {
                _controlState = ControlState.ChildrenInitialized; // framework also initialized

                if ((Page != null) && !DesignMode) {
                    if (Page.ContainsTheme && EnableTheming) {
                        ApplySkin(Page);
                    }
                }

                using (page.Context.SyncContext.AllowVoidAsyncOperationsBlock()) {
                    if (AdapterInternal != null) {
                        AdapterInternal.OnInit(EventArgs.Empty);
                    }
                    else {
                        OnInit(EventArgs.Empty);
                    }
                    await page.GetWaitForPreviousStepCompletionAwaitable();
                }

                _controlState = ControlState.Initialized;
            }

            // track all subsequent state changes
            TrackViewState();

#if DEBUG
            ControlInvariant();
#endif
        }

#if DEBUG

        /// <devdoc>
        ///    <para>This should be used to assert internal state about the control</para>
        /// </devdoc>
        internal void ControlInvariant() {

            // If the control is initialized, the naming container and page should have been pushed in
            if (_controlState >= ControlState.Initialized) {
                if (DesignMode) {
                    // Top-level UserControls do not have a page or a naming container in the designer
                    // hence the special casing.

                    Debug.Assert((_namingContainer != null) || (this is Page) || (this is UserControl));

                    // 



                }
                else {
                    if (!(this is Page)) {
                        Debug.Assert(_namingContainer != null);
                    }
                    Debug.Assert(Page != null);
                }
            }
            // If naming container is set and the name table exists, the ID should exist in it.

            if(_namingContainer != null &&
               _namingContainer._occasionalFields != null &&
               _namingContainer._occasionalFields.NamedControls != null &&
               _id != null) {
                Debug.Assert(_namingContainer._occasionalFields.NamedControls.Contains(_id));
            }
        }

        // Collect some statistic about the number of controls with occasional and
        // rare fields.
        internal void GetRareFieldStatistics(ref int totalControls,
            ref int withOccasionalFields, ref int withRareFields) {
            totalControls++;
            if (_occasionalFields != null) {
                withOccasionalFields++;
                if (_occasionalFields.RareFields != null)
                    withRareFields++;

                // No children: we're done
                if (_controls == null)
                    return;

                int controlCount = _controls.Count;
                for (int i = 0; i < controlCount; i++) {
                    Control control = _controls[i];

                    control.GetRareFieldStatistics(ref totalControls, ref withOccasionalFields,
                        ref withRareFields);
                }
            }
        }
#endif

        protected void ClearChildState() {
            ClearChildControlState();
            ClearChildViewState();
        }

        protected void ClearChildControlState() {
            //VSWhidbey 242621 to be consistent with ClearChildViewState, ignore calls before and during Init
            if (ControlState < ControlState.Initialized) {
                return;
            }
            flags.Set(disableChildControlState);
            if (Page != null) {
                Page.RegisterRequiresClearChildControlState(this);
            }
        }


        /// <devdoc>
        ///    <para>Deletes the view state information for all of the current control's child
        ///       controls.</para>
        /// </devdoc>
        protected void ClearChildViewState() {
            if(_occasionalFields != null) {
                _occasionalFields.ControlsViewState = null;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
        protected void ClearEffectiveClientIDMode() {
            EffectiveClientIDModeValue = ClientIDMode.Inherit;
            if (HasControls()) {
                foreach (Control control in Controls) {
                    control.ClearEffectiveClientIDMode();
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
        protected void ClearCachedClientID() {
            _cachedPredictableID = null;
            if (HasControls()) {
                foreach (Control control in Controls) {
                    control.ClearCachedClientID();
                }
            }
        }

        /// <devdoc>
        ///    <para>Indicates whether the current control's children have any saved view state
        ///       information. This property is read-only.</para>
        /// </devdoc>
        protected bool HasChildViewState {
            get {
                return ((_occasionalFields != null) &&
                        (_occasionalFields.ControlsViewState != null) &&
                        (_occasionalFields.ControlsViewState.Count > 0));
            }
        }


        /// <devdoc>
        /// Sets initial focus on the control
        /// </devdoc>
        public virtual void Focus() {
            Page.SetFocus(this);
        }

        internal void LoadControlStateInternal(object savedStateObj) {
            // Do not load the control state if it has been applied.
            if (flags[controlStateApplied]) {
                return;
            }

            flags.Set(controlStateApplied);

            Pair savedState = (Pair)savedStateObj;
            if (savedState == null) {
                return;
            }
            Page page = Page;
            if (page != null && !page.ShouldLoadControlState(this)) {
                return;
            }
            // VSWhidbey160650: Only call LoadControlState with non null savedState
            if (savedState.First != null) {
                LoadControlState(savedState.First);
            }
            // VSWhidbey356804: Only call LoadAdapterControlState with non null savedState
            if (AdapterInternal == null || savedState.Second == null) {
                return;
            }
            AdapterInternal.LoadAdapterControlState(savedState.Second);
        }


        /// <devdoc>
        /// Load the control state, which is the essential state information needed even if view state is disabled.
        /// </devdoc>
        protected internal virtual void LoadControlState(object savedState) {
        }


        /// <devdoc>
        ///    <para>Restores the view state information from a previous page
        ///       request that was saved by the Control.SavedState method.</para>
        /// </devdoc>
        protected virtual void LoadViewState(object savedState) {
            if (savedState != null) {
                ViewState.LoadViewState(savedState);

                // Load values cached out of view state
                object visible = ViewState["Visible"];
                if (visible != null) {
                    if(!(bool)visible) {
                        flags.Set(invisible);
                    }
                    else {
                        flags.Clear(invisible);
                    }
                    flags.Set(visibleDirty);
                }

                object validateRequestModeValue = ViewState["ValidateRequestMode"];
                if (validateRequestModeValue != null) {
                    flags[validateRequestMode, validateRequestModeOffset] = (int)validateRequestModeValue;
                    flags.Set(validateRequestModeDirty);
                }
            }
        }

        internal void LoadViewStateRecursive(object savedState) {
            // nothing to do if we have no state
            if (savedState == null || flags[disableViewState])
                return;

            if (Page != null && Page.IsPostBack) {
                object controlState = null;
                object adapterState = null;
                ArrayList childState = null;

                Pair allSavedState = savedState as Pair;
                if (allSavedState != null) {
                    controlState = allSavedState.First;
                    childState = (ArrayList)allSavedState.Second;
                }
                else {
                    Debug.Assert(savedState is Triplet);
                    Triplet t = (Triplet)savedState;

                    controlState = t.First;
                    adapterState = t.Second;
                    childState = (ArrayList)t.Third;
                }

                try {
                    if ((adapterState != null) && (AdapterInternal != null)) {
                        AdapterInternal.LoadAdapterViewState(adapterState);
                    }

                    if (controlState != null) {
                        LoadViewState(controlState);
                    }

                    if (childState != null) {
                        if (LoadViewStateByID) {
                            LoadChildViewStateByID(childState);
                        }
                        else {
                            LoadChildViewStateByIndex(childState);
                        }
                    }
                }
                catch (InvalidCastException) {
                    // catch all viewstate loading problems with casts.  They are most likely changed control trees.
                    throw new HttpException(SR.GetString(SR.Controls_Cant_Change_Between_Posts));
                }
                catch (IndexOutOfRangeException) {
                    // catch all viewstate loading problems with indeces.  They are most likely changed control trees.
                    throw new HttpException(SR.GetString(SR.Controls_Cant_Change_Between_Posts));
                }
            }

            _controlState = ControlState.ViewStateLoaded;
        }

        internal void LoadChildViewStateByID(ArrayList childState) {
            int childStateCount = childState.Count;
            for (int i = 0; i < childStateCount; i += 2) {
                // first element is index or ID of control with state and the
                // next element is state of the control
                string controlId = (string)childState[i];
                object state = childState[i + 1];

                Control childControl = FindControl(controlId);
                if (childControl != null) {
                    childControl.LoadViewStateRecursive(state);
                }
                else {
                    // couldn't find a control for this state blob, save it for later
                    EnsureOccasionalFields();
                    if (_occasionalFields.ControlsViewState == null) {
                        _occasionalFields.ControlsViewState = new Hashtable();
                    }
                    _occasionalFields.ControlsViewState[controlId] = state;
                }
            }
        }

        internal void LoadChildViewStateByIndex(ArrayList childState) {
            ControlCollection ctrlColl = Controls;
            int ctrlCount = ctrlColl.Count;

            int childStateCount = childState.Count;
            for (int i = 0; i < childStateCount; i += 2) {
                // first element is index of control with state and the
                // next element is state of the control
                int controlIndex = (int)childState[i];
                object state = childState[i + 1];

                if (controlIndex < ctrlCount) {
                    // we have a control for this state blob
                    ctrlColl[controlIndex].LoadViewStateRecursive(state);
                }
                else {
                    // couldn't find a control for this state blob, save it for later
                    EnsureOccasionalFields();
                    if (_occasionalFields.ControlsViewState == null) {
                        _occasionalFields.ControlsViewState = new Hashtable();
                    }
                    _occasionalFields.ControlsViewState[controlIndex] = state;
                }
            }
        }

        ///
        /// Figure out if a path is physical or virtual.  This is useful because a number of our controls
        /// accept either type of path for the same attribute.
        ///
        internal void ResolvePhysicalOrVirtualPath(string path, out VirtualPath virtualPath, out string physicalPath) {
            if (System.Web.Util.UrlPath.IsAbsolutePhysicalPath(path)) {
                physicalPath = path;
                virtualPath = null;
            }
            else {
                physicalPath = null;

                // It could be relative, so resolve it
                virtualPath = TemplateControlVirtualDirectory.Combine(VirtualPath.Create(path));
            }
        }


        /// <devdoc>
        /// <para>
        ///   This function takes a virtual path, that is a relative or root relative URL without a protocol.
        ///   It returns the mapped physcial file name relative to the template source. It throws an exception if
        ///   there is insufficient security access to read or investigate the mapped result. This should be used
        ///   by controls that can read files and live in fully trusted DLLs such as System.Web.dll to prevent
        ///   security issues. The exception thrown does not give away information about the mapping.  For absolute
        ///   physical paths, this function checks permission
        /// </para>
        /// </devdoc>
        protected internal string MapPathSecure(string virtualPath) {
            if (String.IsNullOrEmpty(virtualPath)) {
                throw new ArgumentNullException("virtualPath", SR.GetString(SR.VirtualPath_Length_Zero));
            }

            string physicalPath;
            VirtualPath virtualPathObject;
            ResolvePhysicalOrVirtualPath(virtualPath, out virtualPathObject, out physicalPath);
            if (physicalPath == null) {
                physicalPath = virtualPathObject.MapPathInternal(TemplateControlVirtualDirectory,
                    true /*allowCrossAppMapping*/);
            }

            // Security check
            HttpRuntime.CheckFilePermission(physicalPath);

            return physicalPath;
        }


        /// <devdoc>
        /// <para>
        ///   This function takes a virtual path, that is a relative or root relative URL without a protocol.
        ///   It can also take a physical path, either local (c:\) or UNC.
        ///   It returns a stream used to read to contents of the file. It throws an exception if
        ///   there is insufficient security access to read or investigate the mapped result. This should be used
        ///   by controls that can read files and live in fully trusted DLLs such as System.Web.dll to prevent
        ///   security issues. The exception thrown does not give away information about the mapping.  For absolute
        ///   physical paths, this function checks permission
        /// </para>
        /// </devdoc>
        protected internal Stream OpenFile(string path) {

            string physicalPath = null;
            VirtualFile vfile = null;

            // Need to Trim it since MapPath no longer allows trailing space (VSWhidbey 441210)
            path = path.Trim();

            if (UrlPath.IsAbsolutePhysicalPath(path)) {
                // Absolute physical path
                physicalPath = path;
            }
            else {
                vfile = HostingEnvironment.VirtualPathProvider.GetFile(path);
                MapPathBasedVirtualFile mapPathVFile = vfile as MapPathBasedVirtualFile;
                if (mapPathVFile != null) {
                    physicalPath = mapPathVFile.PhysicalPath;
                }
            }

            // If we got a physical path, make sure the user has access to it
            if (physicalPath != null) {
                HttpRuntime.CheckFilePermission(physicalPath);
            }

            if (vfile != null) {
                return vfile.Open();
            }
            else {
                return new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
        }

        ///
        /// Open a stream from either a virtual or physical path, and if possible get a CacheDependency
        /// for the resulting Stream.
        ///
        internal Stream OpenFileAndGetDependency(VirtualPath virtualPath, string physicalPath, out CacheDependency dependency) {

            // Only one of the paths should be non-null
            Debug.Assert((virtualPath == null) != (physicalPath == null));

            // If we got a virtual path, and we're using the default VPP, call MapPath
            if (physicalPath == null && HostingEnvironment.UsingMapPathBasedVirtualPathProvider) {
                physicalPath = virtualPath.MapPathInternal(TemplateControlVirtualDirectory,
                    true /*allowCrossAppMapping*/);
            }

            Stream stream;
            if (physicalPath != null) {
                // Security check
                HttpRuntime.CheckFilePermission(physicalPath);

                // Work directly with the physical file, bypassing the VPP
                stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                dependency = new CacheDependency(0, physicalPath);
            }
            else {
                // It's non file system based, so go though the VirtualPathProvider
                stream = virtualPath.OpenFile();
                dependency = VirtualPathProvider.GetCacheDependency(virtualPath);
            }

            return stream;
        }


        /// <devdoc>
        /// <para>Raises the <see langword='Load'/>
        /// event. This notifies the control that it should perform any work that needs to
        /// occur for each page request.</para>
        /// </devdoc>
        protected internal virtual void OnLoad(EventArgs e) {
            if(HasEvents()) {
                EventHandler handler = _events[EventLoad] as EventHandler;
                if(handler != null) {
                    handler(this, e);
                }
            }
        }

        internal virtual void LoadRecursive() {
            // Only make the actual call if it hasn't already happened (ASURT 111303)
            if (_controlState < ControlState.Loaded) {
                if (AdapterInternal != null) {
                    AdapterInternal.OnLoad(EventArgs.Empty);
                }
                else {
                    OnLoad(EventArgs.Empty);
                }
            }

            // Call Load on all our children
            if (_controls != null) {
                string oldmsg = _controls.SetCollectionReadOnly(SR.Parent_collections_readonly);

                int controlCount = _controls.Count;
                for (int i = 0; i < controlCount; i++) {
                    _controls[i].LoadRecursive();
                }

                _controls.SetCollectionReadOnly(oldmsg);
            }

            if (_controlState < ControlState.Loaded)
                _controlState = ControlState.Loaded;
        }

        // Same as LoadRecursive, but has an async point immediately after the call to this.OnLoad.
        internal async Task LoadRecursiveAsync(Page page) {
            // Only make the actual call if it hasn't already happened (ASURT 111303)
            if (_controlState < ControlState.Loaded) {
                using (page.Context.SyncContext.AllowVoidAsyncOperationsBlock()) {
                    if (AdapterInternal != null) {
                        AdapterInternal.OnLoad(EventArgs.Empty);
                    }
                    else {
                        OnLoad(EventArgs.Empty);
                    }
                    await page.GetWaitForPreviousStepCompletionAwaitable();
                }
            }

            // Call Load on all our children
            if (_controls != null) {
                string oldmsg = _controls.SetCollectionReadOnly(SR.Parent_collections_readonly);

                int controlCount = _controls.Count;
                for (int i = 0; i < controlCount; i++) {
                    _controls[i].LoadRecursive();
                }

                _controls.SetCollectionReadOnly(oldmsg);
            }

            if (_controlState < ControlState.Loaded)
                _controlState = ControlState.Loaded;
        }

        /// <devdoc>
        /// <para>Raises the <see langword='PreRender'/> event. This method uses event arguments
        ///    to pass the event data to the control.</para>
        /// </devdoc>
        protected internal virtual void OnPreRender(EventArgs e) {
            if(HasEvents()) {
                EventHandler handler = _events[EventPreRender] as EventHandler;
                if (handler != null) {
                    handler(this, e);
                }
            }
        }

        internal virtual void PreRenderRecursiveInternal() {
            // Call Visible property and cache value in !flags[invisible] to allow Visible to be overridden.
            // This avoids unnecessary virtual property calls in SaveViewState and Render.
            bool visible = Visible;
            if(!visible) {
                flags.Set(invisible);
            }
            else {
                flags.Clear(invisible);
                EnsureChildControls();

                if (AdapterInternal != null) {
                    AdapterInternal.OnPreRender(EventArgs.Empty);
                }
                else {
                    OnPreRender(EventArgs.Empty);
                }

                if (_controls != null) {
                    string oldmsg = _controls.SetCollectionReadOnly(SR.Parent_collections_readonly);

                    int controlCount = _controls.Count;
                    for (int i=0; i < controlCount; i++) {
                        _controls[i].PreRenderRecursiveInternal();
                    }
                    _controls.SetCollectionReadOnly(oldmsg);
                }
            }
            _controlState = ControlState.PreRendered;
        }

        // Same as PreRenderRecursive, but has an async point after the call to this.OnPreRender.
        internal async Task PreRenderRecursiveInternalAsync(Page page) {
            // Call Visible property and cache value in !flags[invisible] to allow Visible to be overridden.
            // This avoids unnecessary virtual property calls in SaveViewState and Render.
            bool visible = Visible;
            if (!visible) {
                flags.Set(invisible);
            }
            else {
                flags.Clear(invisible);
                if (AppSettings.EnableAsyncModelBinding) {
                    using (page.Context.SyncContext.AllowVoidAsyncOperationsBlock()) {
                        EnsureChildControls();
                        await page.GetWaitForPreviousStepCompletionAwaitable();
                    }
                }
                else {
                    EnsureChildControls();
                }                

                using (page.Context.SyncContext.AllowVoidAsyncOperationsBlock()) {
                    if (AdapterInternal != null) {
                        AdapterInternal.OnPreRender(EventArgs.Empty);
                    }
                    else {
                        OnPreRender(EventArgs.Empty);
                    }
                    await page.GetWaitForPreviousStepCompletionAwaitable();
                }

                if (_controls != null) {
                    string oldmsg = _controls.SetCollectionReadOnly(SR.Parent_collections_readonly);

                    int controlCount = _controls.Count;
                    for (int i = 0; i < controlCount; i++) {
                        if (AppSettings.EnableAsyncModelBinding) {
                            // To make sure every OnPreRender is awaited so that _controlState
                            // would not be set to ControlState.PreRendered until the control is
                            // really PreRendered
                            await _controls[i].PreRenderRecursiveInternalAsync(page);
                        }
                        else {
                            _controls[i].PreRenderRecursiveInternal();
                        }
                    }

                    _controls.SetCollectionReadOnly(oldmsg);
                }
            }
            _controlState = ControlState.PreRendered;
        }

        internal int EstimateStateSize(object state) {
            if(state == null) {
                return 0;
            }
            return Util.SerializeWithAssert(new ObjectStateFormatter(), state).Length;
        }

        /*
         * Walk the tree and fill in profile information
         */

        /// <internalonly/>
        /// <devdoc>
        /// <para>Gathers information about the control and delivers it to the <see cref='System.Web.UI.Page.Trace'/>
        /// property to be displayed when tracing is enabled for the page.</para>
        /// </devdoc>
        protected void BuildProfileTree(string parentId, bool calcViewState) {
            // estimate the viewstate size.
            calcViewState = calcViewState && (!flags[disableViewState]);
            int viewstatesize;
            if (calcViewState)
                viewstatesize = EstimateStateSize(SaveViewState());
            else
                viewstatesize = 0;

            int controlstatesize = 0;
            if(Page != null && Page._registeredControlsRequiringControlState != null && Page._registeredControlsRequiringControlState.Contains(this)) {
                controlstatesize = EstimateStateSize(SaveControlStateInternal());
            }

            // give it all to the profiler
            Page.Trace.AddNewControl(UniqueID, parentId, this.GetType().FullName, viewstatesize, controlstatesize);

            if (_controls != null) {
                int controlCount = _controls.Count;
                for (int i = 0; i < controlCount; i++) {
                    _controls[i].BuildProfileTree(UniqueID, calcViewState);
                }
            }
        }


        internal object SaveControlStateInternal() {
            object controlState = SaveControlState();
            object adapterControlState = null;
            if (AdapterInternal != null) {
                adapterControlState = AdapterInternal.SaveAdapterControlState();
            }
            if (controlState != null || adapterControlState != null) {
                return new Pair(controlState, adapterControlState);
            }
            return null;
        }


        /// <devdoc>
        /// Save the control state, which is the essential state information needed even if view state is disabled.
        /// </devdoc>
        protected internal virtual object SaveControlState() {
            return null;
        }

        // Save modified state the control would like restored on the postback.
        // Return null if there is no state to save.

        /// <devdoc>
        ///    <para>
        ///       Saves view state for use with a later <see cref='System.Web.UI.Control.LoadViewState'/>
        ///       request.
        ///    </para>
        /// </devdoc>
        protected virtual object SaveViewState() {
            // Save values cached out of view state
            if (flags[visibleDirty]) {
                ViewState["Visible"] = !flags[invisible];
            }
            if (flags[validateRequestModeDirty]) {
                ViewState["ValidateRequestMode"] = (int)ValidateRequestMode;
            }
            if (_viewState != null)
                return _viewState.SaveViewState();

            return null;
        }

        // Answer any state this control or its descendants want to save on freeze.
        // The format for saving is Triplet(myState, ArrayList childIDs, ArrayList childStates),
        // where myState or childStates and childIDs may be null.
        internal object SaveViewStateRecursive(ViewStateMode inheritedMode) {
            if (flags[disableViewState])
                return null;

            bool saveThisState;
            if (flags[viewStateNotInherited]) {
                if (flags[viewStateMode]) {
                    saveThisState = true;
                    inheritedMode = ViewStateMode.Enabled;
                }
                else {
                    saveThisState = false;
                    inheritedMode = ViewStateMode.Disabled;
                }
            }
            else {
                saveThisState = (inheritedMode == ViewStateMode.Enabled);
            }

            object adapterState = null;
            object controlSavedState = null;

            if (saveThisState) {
                if (AdapterInternal != null) {
                    adapterState = AdapterInternal.SaveAdapterViewState();
                }
                controlSavedState = SaveViewState();    
            }

            ArrayList childStates = null;
            if (HasControls()) {
                ControlCollection occasionalFieldControls = _controls;
                int occasionalFieldControlCount = occasionalFieldControls.Count;

                bool useId = LoadViewStateByID;
                for (int i = 0; i < occasionalFieldControlCount; i++) {
                    Control child = occasionalFieldControls[i];
                    object childState = child.SaveViewStateRecursive(inheritedMode);
                    if (childState != null) {
                        if (childStates == null) {
                            childStates = new ArrayList(occasionalFieldControlCount);
                        }

                        if (useId) {
                            child.EnsureID();
                            childStates.Add(child.ID);
                        }
                        else {
                            childStates.Add(i);
                        }
                        childStates.Add(childState);
                    }
                }
            }

            if (AdapterInternal != null) {
                if ((controlSavedState != null) || (adapterState != null) || (childStates != null)) {
                    return new Triplet(controlSavedState, adapterState, childStates);
                }
            }
            else {
                if ((controlSavedState != null) || (childStates != null)) {
                    return new Pair(controlSavedState, childStates);
                }
            }

            return null;
        }


        /// <devdoc>
        /// <para>Outputs control content to a provided HTMLTextWriter
        /// output stream.</para>
        /// </devdoc>
        protected internal virtual void Render(HtmlTextWriter writer) {
            RenderChildren(writer);
        }

        internal void RenderChildrenInternal(HtmlTextWriter writer, ICollection children) {
            // If we have a delegate, use it for the rendering.
            // This happens when there is some ASP code.  See also Whidbey 33012.
            if(RareFields != null && RareFields.RenderMethod != null ) {
                writer.BeginRender();
                RareFields.RenderMethod(writer, this);
                writer.EndRender();
                return;
            }
            if (children != null) {
                foreach (Control child in children) {
                    child.RenderControl(writer);
                }
            }
        }

        protected internal virtual void RenderChildren(HtmlTextWriter writer) {
            ICollection children = _controls;
            RenderChildrenInternal(writer, children);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void RenderControl(HtmlTextWriter writer) {
            //use the Adapter property to ensure it is resolved
            RenderControl(writer, Adapter);
        }

        /// <devdoc>
        ///    <para>Used for MobilePage implementation.</para>
        /// </devdoc>
        protected void RenderControl(HtmlTextWriter writer, ControlAdapter adapter) {
            if (!flags[invisible] && !flags[notVisibleOnPage]) {
                HttpContext context = (Page == null) ? null : Page._context;
                if (context  != null && context.TraceIsEnabled) {
                    int presize = context.Response.GetBufferedLength();
                    RenderControlInternal(writer, adapter);
                    int postsize = context.Response.GetBufferedLength();
                    context.Trace.AddControlSize(UniqueID, postsize - presize);
                }
                else {
                    RenderControlInternal(writer, adapter);
                }
            }
            else {
                TraceNonRenderingControlInternal(writer);
            }
        }

        private void RenderControlInternal(HtmlTextWriter writer, ControlAdapter adapter) {
            try {
                BeginRenderTracing(writer, this);

                if (adapter != null) {
                    // 
                    adapter.BeginRender(writer);
                    adapter.Render(writer);
                    adapter.EndRender(writer);
                }
                else {
                    Render(writer);
                }
            }
            finally {
                EndRenderTracing(writer, this);
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected internal virtual void OnUnload(EventArgs e) {
            if(HasEvents()) {
                EventHandler handler = _events[EventUnload] as EventHandler;
                if (handler != null) {
                    handler(this, e);
                }
            }
        }


        /// <devdoc>
        ///    <para>Enables a control to perform final cleanup.</para>
        /// </devdoc>
        public virtual void Dispose() {
            IContainer container = null;

            if (Site != null) {
                container = (IContainer)Site.GetService(typeof(IContainer));
                if (container != null) {
                    container.Remove(this);
                    EventHandler disp = Events[EventDisposed] as EventHandler;
                    if (disp != null)
                        disp(this, EventArgs.Empty);
                }
            }

            if (_occasionalFields != null) {
                _occasionalFields.Dispose();
                //do not null out for backwards compat, VSWhidbey 475940
                //_occasionalFields = null;
            }

            if (_events != null) {
                _events.Dispose();
                _events = null;
            }
        }


        internal virtual void UnloadRecursive(bool dispose) {
            Page page = Page;
            if (page != null && page.RequiresControlState(this)) {
                page.UnregisterRequiresControlState(this);
                RareFieldsEnsured.RequiredControlState = true;
            }

            // Remove the generated ID so it will be assigned a different ID next time.
            if (flags[useGeneratedID]) {
                _id = null;
                flags.Clear(useGeneratedID);
            }

            if (_controls != null) {
                string oldmsg = _controls.SetCollectionReadOnly(SR.Parent_collections_readonly);

                int controlCount = _controls.Count;
                for (int i = 0; i < controlCount; i++)
                    _controls[i].UnloadRecursive(dispose);

                _controls.SetCollectionReadOnly(oldmsg);
            }

            if (AdapterInternal != null) {
                AdapterInternal.OnUnload(EventArgs.Empty);
            }
            else {
                OnUnload(EventArgs.Empty);
            }

            // 
            if (dispose)
                Dispose();

            // VSWhidbey 244999: Everett behavior doesn't reset the control state.
            // But for control which requires its OnInit method to be called again
            // to properly initialize when the control is removed and added back
            // to Page's control tree, the control can override IsReloadable
            // to true so the control state is reset.  e.g. Validator, see bug
            if (IsReloadable) {
                _controlState = ControlState.Constructed;
            }
        }


        /// <devdoc>
        ///    <para>Assigns an sources of the event and its information up the page control
        ///       hierarchy until they reach the top of the control tree. </para>
        /// </devdoc>
        protected void RaiseBubbleEvent(object source, EventArgs args) {
            Control currentTarget = Parent;
            while (currentTarget != null) {
                if (currentTarget.OnBubbleEvent(source, args)) {
                    return;
                }
                currentTarget = currentTarget.Parent;
            }
        }


        /// <devdoc>
        ///    <para>Determines whether the event for the control should be passed up the page's
        ///       control hierarchy.</para>
        /// </devdoc>
        protected virtual bool OnBubbleEvent(object source, EventArgs args) {
            return false;
        }


        // Members related to being a container


        /// <devdoc>
        ///    <para> Gets a ControlCollection object that represents the child controls for a specified control in the
        ///       UI hierarchy.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.Control_Controls)
        ]
        public virtual ControlCollection Controls {
            get {
                if (_controls == null) {
                    _controls = CreateControlCollection();
                }
                return _controls;
            }
        }

        [
        WebCategory("Behavior"),
        WebSysDescription(SR.Control_ValidateRequestMode),
        DefaultValue(ValidateRequestMode.Inherit)
        ]
        public virtual ValidateRequestMode ValidateRequestMode {
            get {
                return (ValidateRequestMode)flags[validateRequestMode, validateRequestModeOffset];
            }
            set {
                SetValidateRequestModeInternal(value, setDirty: true);
            }
        }

        internal void SetValidateRequestModeInternal(UI.ValidateRequestMode value, bool setDirty) {
            if (value < ValidateRequestMode.Inherit || value > ValidateRequestMode.Enabled) {
                throw new ArgumentOutOfRangeException("value");
            }

            int oldValue = flags[validateRequestMode, validateRequestModeOffset];
            if (setDirty && (oldValue != (int)value)) {
                flags.Set(validateRequestModeDirty);
            }

            flags[validateRequestMode, validateRequestModeOffset] = (int)value;
        }

        internal bool CalculateEffectiveValidateRequest() {
            RuntimeConfig config = RuntimeConfig.GetConfig();
            HttpRuntimeSection runtimeSection = config.HttpRuntime;
            if (runtimeSection.RequestValidationMode >= VersionUtil.Framework45) {
                Control c = this;
                while (c != null) {
                    ValidateRequestMode mode = c.ValidateRequestMode;
                    if (mode != ValidateRequestMode.Inherit) {
                        return mode == ValidateRequestMode.Enabled;
                    }
                    c = c.Parent;
                }
            }
            return true;
        }


        /// <devdoc>
        ///    <para>Indicates a dictionary of state information that allows you to save and restore
        ///       the state of a control across multiple requests for the same page.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.Control_State)
        ]
        protected virtual StateBag ViewState {
            get {
                if (_viewState != null) {   // create a StateBag on demand; WebControl makes its case sensitive
                    return _viewState;
                }

                _viewState = new StateBag(ViewStateIgnoresCase);
                if (IsTrackingViewState)
                    _viewState.TrackViewState();
                return _viewState;
            }
        }

        // fast enough that we cam always use it.

        /// <devdoc>
        /// <para>Indicates whether the <see cref='System.Web.UI.StateBag'/> object is case-insensitive.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)        
        ]
        protected virtual bool ViewStateIgnoresCase {
            get {
                return false;
            }
        }


        /// <devdoc>
        /// </devdoc>
        protected internal virtual void AddedControl(Control control, int index) {
            if (control.OwnerControl != null) {
                throw new InvalidOperationException(SR.GetString(SR.Substitution_NotAllowed));
            }

            if (control._parent != null) {
                control._parent.Controls.Remove(control);
            }

            control._parent = this;
            control._page = Page;
            control.flags.Clear(designModeChecked);

            // We only add to naming container if it is available. Otherwise, it will be pushed through
            // during InitRecursive
            Control namingContainer = flags[isNamingContainer] ? this : _namingContainer;
            if (namingContainer != null) {
                control.UpdateNamingContainer(namingContainer);
                if (control._id == null && !control.flags[idNotRequired]) {
                    // this will also dirty the name table in the naming container
                    control.GenerateAutomaticID();
                }
                else if (control._id != null || (control._controls != null)) {
                    // If the control has and ID, or has children (which *may* themselves
                    // have ID's), we need to dirty the name table (ASURT 100557)
                    namingContainer.DirtyNameTable();
                }
            }

            /*
             * The following is for times when AddChild is called after CreateChildControls. This
             * allows users to add children at any time in the creation process without having
             * to understand the underlying machinery.
             * Note that if page is null, it means we haven't been attached to a container ourselves.
             * If this is true, when we are, our children will be recursively set up.
             */
            if (_controlState >= ControlState.ChildrenInitialized) {

                Debug.Assert(namingContainer != null);
                control.InitRecursive(namingContainer);

                // VSWhidbey 396372: We need to reregister the control state if the control is reparented because the control
                // is unregistered during unload, but its already been inited once, so it will not get its Init called again
                // which is where most controls call RegisterRequiresControlState
                if (control._controlState >= ControlState.Initialized &&
                    control.RareFields != null &&
                    control.RareFields.RequiredControlState) {
                    Page.RegisterRequiresControlState(control);
                }

                if (_controlState >= ControlState.ViewStateLoaded) {
                    object viewState = null;
                    if(_occasionalFields != null && _occasionalFields.ControlsViewState != null) {
                        viewState = _occasionalFields.ControlsViewState[index];
                        // This solution takes the conservative approach that once viewstate has been
                        // applied to a child control, it is thrown away.  This eliminates inadvertently
                        // setting viewstate on the wrong control, which can occur in scenarios where
                        // the child control collection is being manipulated via code.  Probably need
                        // to provide a feature where programmer can control whether to reapply viewstate
                        // or not.
                        if (LoadViewStateByID) {
                            control.EnsureID();
                            viewState = _occasionalFields.ControlsViewState[control.ID];
                            _occasionalFields.ControlsViewState.Remove(control.ID);
                        }
                        else {
                            viewState = _occasionalFields.ControlsViewState[index];
                            _occasionalFields.ControlsViewState.Remove(index);
                        }
                    }

                    control.LoadViewStateRecursive(viewState);

                    if (_controlState >= ControlState.Loaded) {
                        control.LoadRecursive();

                        if (_controlState >= ControlState.PreRendered)
                            control.PreRenderRecursiveInternal();
                    }
                }
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual ControlCollection CreateControlCollection() {
            return new ControlCollection(this);
        }


        /// <devdoc>
        ///    <para>
        ///       Notifies any controls that use composition-based implementation to create any
        ///       child controls they contain in preperation for postback or rendering.
        ///    </para>
        /// </devdoc>
        protected internal virtual void CreateChildControls() {
        }



        /// <devdoc>
        ///    <para>Indicates whether the control's child controls have been created.</para>
        /// </devdoc>
        protected bool ChildControlsCreated {
            get {
                return flags[controlsCreated];
            }
            set {
                if (!value && flags[controlsCreated]) {
                    Controls.Clear();
                }
                if(value) {
                    flags.Set(controlsCreated);
                }
                else {
                    flags.Clear(controlsCreated);
                }
            }
        }


        /// <devdoc>
        ///    <para>Make a URL absolute using the AppRelativeTemplateSourceDirectory.  The returned URL is for
        ///        client use, and will contain the session cookie if appropriate.</para>
        /// </devdoc>
        public string ResolveUrl(string relativeUrl) {
            if (relativeUrl == null) {
                throw new ArgumentNullException("relativeUrl");
            }

            // check if its empty or already absolute
            if ((relativeUrl.Length == 0) || (UrlPath.IsRelativeUrl(relativeUrl) == false)) {
                return relativeUrl;
            }

            string baseUrl = AppRelativeTemplateSourceDirectory;
            if (String.IsNullOrEmpty(baseUrl)) {
                return relativeUrl;
            }

            // first make it absolute
            string url = UrlPath.Combine(baseUrl, relativeUrl);

            // include the session cookie if available (ASURT 47658)
            // As a side effect, this will change an app relative path (~/...) to app absolute
            return Context.Response.ApplyAppPathModifier(url);
        }


        /// <devdoc>
        ///    <para> Return a URL that is suitable for use on the client.
        ///     If the URL is absolute, return it unchanged.  If it is relative, turn it into a
        ///     relative URL that is correct from the point of view of the current request path
        ///     (which is what the browser uses for resolution).</para>
        /// </devdoc>
        public string ResolveClientUrl(string relativeUrl) {
            if (DesignMode && Page != null && Page.Site != null) {
                IUrlResolutionService resolutionService = (IUrlResolutionService)Page.Site.GetService(typeof(IUrlResolutionService));
                if (resolutionService != null) {
                    return resolutionService.ResolveClientUrl(relativeUrl);
                }
            }

            if (relativeUrl == null) {
                throw new ArgumentNullException("relativeUrl");
            }

            // Get the app absolute TemplateSourceDirectory (not app relative)
            string tplSourceDir = VirtualPath.GetVirtualPathString(TemplateControlVirtualDirectory);
            if (String.IsNullOrEmpty(tplSourceDir))
                return relativeUrl;

            string baseRequestDir = Context.Request.ClientBaseDir.VirtualPathString;

            // If the path is app relative (~/...), we cannot take shortcuts, since
            // the ~ is meaningless on the client, and must be resolved
            if (!UrlPath.IsAppRelativePath(relativeUrl)) {

                // If the template source directory is the same as the directory of the request,
                // we don't need to do any adjustments to the input path
                if (StringUtil.EqualsIgnoreCase(baseRequestDir, tplSourceDir))
                    return relativeUrl;

                // check if it's empty or absolute
                if ((relativeUrl.Length == 0) || (!UrlPath.IsRelativeUrl(relativeUrl))) {
                    return relativeUrl;
                }
            }

            // first make it absolute
            string url = UrlPath.Combine(tplSourceDir, relativeUrl);

            // Make sure the path ends with a slash before calling MakeRelative
            baseRequestDir = UrlPath.AppendSlashToPathIfNeeded(baseRequestDir);

            // Now, make it relative to the current request, so that the client will
            // compute the correct path
            url = HttpUtility.UrlPathEncode(UrlPath.MakeRelative(baseRequestDir, url));
            Debug.Trace("ClientUrl", "*** ResolveClientUrl (" + relativeUrl + ") --> " + url + " ***");
            return url;
        }

        internal void DirtyNameTable() {
            Debug.Assert(this is INamingContainer);
            if(_occasionalFields != null) {
                _occasionalFields.NamedControls = null;
            }
        }

        private void EnsureNamedControlsTable() {
            Debug.Assert(this is INamingContainer);
            Debug.Assert(HasControls());
            Debug.Assert(_occasionalFields != null);
            Debug.Assert(_occasionalFields.NamedControls == null);

            _occasionalFields.NamedControls = new HybridDictionary(/*initialSize*/ _occasionalFields.NamedControlsID, /*caseInsensitive*/ true);
            FillNamedControlsTable(this, _controls);
        }

        private void FillNamedControlsTable(Control namingContainer, ControlCollection controls) {
            Debug.Assert(namingContainer._occasionalFields != null);
            Debug.Assert(namingContainer._occasionalFields.NamedControls != null);
            Debug.Assert((controls != null) && (controls.Count != 0));

            int controlCount = controls.Count;
            for (int i=0; i < controlCount; i++) {
                Control control = controls[i];
                if (control._id != null) {
#if DEBUG
                    if (control._namingContainer != null) {
                        Debug.Assert(control._namingContainer == namingContainer);
                    }
#endif // DEBUG
                    try {
                        namingContainer.EnsureOccasionalFields();
                        namingContainer._occasionalFields.NamedControls.Add(control._id, control);
                    }
                    catch {
                        throw new HttpException(SR.GetString(SR.Duplicate_id_used, control._id, "FindControl"));
                    }
                }
                if (control.HasControls() && (control.flags[isNamingContainer] == false)) {
                    FillNamedControlsTable(namingContainer, control.Controls);
                }
            }
        }


        /// <devdoc>
        ///    <para>Searches the current naming container for a control with
        ///       the specified <paramref name="id"/> .</para>
        /// </devdoc>
        public virtual Control FindControl(String id) {
            return FindControl(id, 0);
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Searches the current naming container for a control with the specified
        ///    <paramref name="id"/> and an offset to aid in the
        ///       search.</para>
        /// </devdoc>
        protected virtual Control FindControl(String id, int pathOffset) {
            // DevDiv #338426 - Since this is a recursive function, malicious clients can send us an id
            // which causes a very deep stack dive, resulting in SO (which terminates the worker process).
            // We avoid this via the following call, which at the time of this writing ensures that at
            // least 50% of the available stack remains. The check is very quick: < 1 microsecond.
            RuntimeHelpers.EnsureSufficientExecutionStack();

            string childID;

            EnsureChildControls();

            // If we're not the naming container, let it do the job
            if (!(flags[isNamingContainer])) {
                Control namingContainer = NamingContainer;
                if (namingContainer != null) {
                    return namingContainer.FindControl(id, pathOffset);
                }
                return null;
            }

            // No registered control, demand create the named controls table
            //call HasControls doesn't ensures _occasionalFields != null
            if (HasControls()) {
                EnsureOccasionalFields();
                if (_occasionalFields.NamedControls == null) {
                    EnsureNamedControlsTable();
                }
            }
            if (_occasionalFields == null || _occasionalFields.NamedControls == null) {
                return null;
            }

            // Need to support ':' for V1 backward compatibility.
            char[] findControlSeparators = { ID_SEPARATOR, LEGACY_ID_SEPARATOR };

            // Is it a hierarchical name?
            int newPathOffset = id.IndexOfAny(findControlSeparators, pathOffset);

            // If not, handle it here
            if (newPathOffset == -1) {
                childID = id.Substring(pathOffset);
                return _occasionalFields.NamedControls[childID] as Control;
            }

            // Get the name of the child, and try to locate it
            childID = id.Substring(pathOffset, newPathOffset - pathOffset);
            Control child =  _occasionalFields.NamedControls[childID] as Control;

            // Child doesn't exist: fail
            if (child == null)
                return null;

            return child.FindControl(id, newPathOffset + 1);
        }

        internal Control FindControlFromPageIfNecessary(string id) {
            Control c = FindControl(id);
            // Find control from the page if it's a hierarchical ID.
            // Dev11 bug 19915
            if (c == null && Page != null) {
                char[] findControlSeparators = { ID_SEPARATOR, LEGACY_ID_SEPARATOR };
                if (id.IndexOfAny(findControlSeparators) != -1) {
                    c = Page.FindControl(id);
                }
            }
            return c;
        }

        /*
         * Called when the controls of a naming container are cleared.
         */
        internal void ClearNamingContainer() {
            Debug.Assert(this is INamingContainer);

            EnsureOccasionalFields();
            _occasionalFields.NamedControlsID = 0;
            DirtyNameTable();
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected virtual IDictionary GetDesignModeState() {
            ControlRareFields rareFields = RareFieldsEnsured;
            if (rareFields.DesignModeState == null) {
                rareFields.DesignModeState = new HybridDictionary();
            }
            return rareFields.DesignModeState;
        }


        /// <devdoc>
        ///    <para>Determines if the current control contains any child
        ///       controls. Since this method simply deteremines if any child controls exist at
        ///       all, it can enhance performance by avoiding a call to the Count property,
        ///       inherited from the <see cref='System.Web.UI.ControlCollection'/> class, on the <see cref='System.Web.UI.Control.Controls'/>
        ///       property.</para>
        /// </devdoc>
        public virtual bool HasControls() {
            return _controls != null && _controls.Count > 0;
        }

        /*
         * Check if a Control either has children or has a compiled render method.
         * This is to address issues like ASURT 94127
         */
        internal bool HasRenderingData() {
            return HasControls() || HasRenderDelegate();
        }

        /*
         * Check if a Control either has children or has a compiled render method.
         * This is to address issues like ASURT 94127
         */
        internal bool HasRenderDelegate() {
            if(RareFields != null) {
                return (RareFields.RenderMethod != null );
            }
            return false;
        }

        /*
         * Returns true if the container contains just a static string, i.e.,
         * when the Controls collection has a single LiteralControl.
         */

        /// <devdoc>
        ///    <para>Determines if the container holds literal content only.
        ///       When this method returns <see langword='true'/>
        ///       , the container collection only holds a single literal control. The
        ///       content is then passed to the requesting browser as HTML.</para>
        /// </devdoc>
        protected bool IsLiteralContent() {
            return (_controls != null) && (_controls.Count == 1) &&
            ((_controls[0] is LiteralControl));
        }


        /// <devdoc>
        ///    <para>Determines if view state changes to the
        ///    <see langword='Control'/>
        ///    are being saved. </para>
        /// </devdoc>
        protected bool IsTrackingViewState {
            get {
                return flags[marked];
            }
        }


        /// <devdoc>
        ///    <para>Turns on tracking of view state changes to the control
        ///       so that they can be stored in the <see langword='StateBag'/>
        ///       object.</para>
        /// </devdoc>
        protected virtual void TrackViewState() {
            if (_viewState != null)
                _viewState.TrackViewState();

            flags.Set(marked);
        }


        /// <devdoc>
        ///    <para>Checks that the control contains child controls; if it does not, it creates
        ///       them. This includes any literal content being parsed as a <see cref='System.Web.UI.LiteralControl'/>
        ///       object. </para>
        /// </devdoc>
        protected virtual void EnsureChildControls() {
            if (!ChildControlsCreated && !flags[creatingControls]) {
                flags.Set(creatingControls);
                try {
                    ResolveAdapter();
                    if (AdapterInternal != null) {
                        AdapterInternal.CreateChildControls();
                    }
                    else {
                        CreateChildControls();
                    }

                    // Only set ChildControlsCreated = true if CreateChildControls() did not throw
                    // an exception (VSWhidbey 465798).
                    ChildControlsCreated = true;
                }
                finally {
                    flags.Clear(creatingControls);
                }
            }
        }

        /// <devdoc>
        /// Used internally to store a ControlBuilder reference for the control.
        /// The builder will be used at design-time to help persist all the filtered properties
        /// of the control.
        /// </devdoc>
        internal void SetControlBuilder(ControlBuilder controlBuilder) {
            RareFieldsEnsured.ControlBuilder = controlBuilder;
        }


        /// <devdoc>
        /// </devdoc>
        protected internal virtual void RemovedControl(Control control) {
            if (control.OwnerControl != null) {
                throw new InvalidOperationException(SR.GetString(SR.Substitution_NotAllowed));
            }

            if ((_namingContainer != null) && (control._id != null)) {
                _namingContainer.DirtyNameTable();
            }

            // Controls may need to do their own cleanup.
            control.UnloadRecursive(false);

            control._parent = null;
            control._page = null;
            control._namingContainer = null;

            // Don't reset template source virtual directory on TemplateControl's, because
            // the path is their own, not their parent. i.e. it doesn't change no matter
            // where in the tree they end up.
            if (!(control is TemplateControl)) {
                if (control._occasionalFields != null) {
                    control._occasionalFields.TemplateSourceVirtualDirectory = null;
                }
            }

            if (control._occasionalFields != null ) {
                control._occasionalFields.TemplateControl = null;
            }

            control.flags.Clear(mustRenderID);
            control.ClearCachedUniqueIDRecursive();
        }

        internal void SetDesignMode() {
            flags.Set(designMode);
            flags.Set(designModeChecked);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual void SetDesignModeState(IDictionary data) {
        }

        // Set the delegate to the render method

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Assigns any event handler delegates for the control to match the parameters
        ///       defined in the <see cref='System.Web.UI.RenderMethod'/>. </para>
        /// </devdoc>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void SetRenderMethodDelegate(RenderMethod renderMethod) {
            RareFieldsEnsured.RenderMethod = renderMethod;

            // Make the collection readonly if there are code blocks (ASURT 78810)
            Controls.SetCollectionReadOnly(SR.Collection_readonly_Codeblocks);
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Returns whether the control contains any data binding logic. This method is
        ///       only accessed by RAD designers.</para>
        /// </devdoc>
        bool IDataBindingsAccessor.HasDataBindings {
            get {
                return ((RareFields != null) && (RareFields.DataBindings != null) && (RareFields.DataBindings.Count != 0));
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Indicates a collection of all data bindings on the control. This property is
        /// read-only.</para>
        /// </devdoc>
        DataBindingCollection IDataBindingsAccessor.DataBindings {
            get {
                ControlRareFields rareFields = RareFieldsEnsured;
                if (rareFields.DataBindings == null) {
                    rareFields.DataBindings = new DataBindingCollection();
                }
                return rareFields.DataBindings;
            }
        }


        // IParserAccessor interface
        // A sub-object tag was parsed by the parser; add it to this control.

        /// <internalonly/>
        /// <devdoc>
        /// <para>Notifies the control that an element, XML or HTML, was parsed, and adds it to
        /// the control.</para>
        /// </devdoc>
        void IParserAccessor.AddParsedSubObject(object obj) {
            AddParsedSubObject(obj);
        }

        internal string SpacerImageUrl {
            get {
                EnsureOccasionalFields();
                if (_occasionalFields.SpacerImageUrl == null) {
                    _occasionalFields.SpacerImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(WebControl), "Spacer.gif");
                }
                return _occasionalFields.SpacerImageUrl;
            }
        }

        private Control OwnerControl {
            get {
                if (RareFields == null) {
                    return null;
                }

                return RareFields.OwnerControl;
            }
            set {
                RareFieldsEnsured.OwnerControl = value;
            }
        }

        internal IPostBackDataHandler PostBackDataHandler {
            get {
                IPostBackDataHandler pbdh = AdapterInternal as IPostBackDataHandler;
                if(pbdh != null)
                    return pbdh;
                pbdh = this as IPostBackDataHandler;
                return pbdh;
            }
        }

        internal IPostBackEventHandler PostBackEventHandler {
            get {
                IPostBackEventHandler pbeh = AdapterInternal as IPostBackEventHandler;
                if(pbeh != null)
                    return pbeh;
                pbeh = this as IPostBackEventHandler;
                return pbeh;
            }
        }

        /// <summary>
        /// This method is used for design-time tracing of rendering data.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void BeginRenderTracing(TextWriter writer, object traceObject) {
            RenderTraceListener.CurrentListeners.BeginRendering(writer, traceObject);
        }

        /// <summary>
        /// This method is used for design-time tracing of rendering data.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void EndRenderTracing(TextWriter writer, object traceObject) {
            RenderTraceListener.CurrentListeners.EndRendering(writer, traceObject);
        }

        private void TraceNonRenderingControlInternal(TextWriter writer) {
            BeginRenderTracing(writer, this);
            EndRenderTracing(writer, this);
        }

        /// <summary>
        /// This method is used for design-time tracing of rendering data.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetTraceData(object traceDataKey, object traceDataValue) {
            SetTraceData(this, traceDataKey, traceDataValue);
        }

        /// <summary>
        /// This method is used for design-time tracing of rendering data.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetTraceData(object tracedObject, object traceDataKey, object traceDataValue) {
            RenderTraceListener.CurrentListeners.SetTraceData(tracedObject, traceDataKey, traceDataValue);
        }

        #region IControlDesignerAccessor implementation

        /// <internalonly/>
        IDictionary IControlDesignerAccessor.UserData {
            get {
                ControlRareFields rareFields = RareFieldsEnsured;
                if (rareFields.ControlDesignerAccessorUserData == null) {
                    rareFields.ControlDesignerAccessorUserData = new HybridDictionary();
                }
                return rareFields.ControlDesignerAccessorUserData;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        IDictionary IControlDesignerAccessor.GetDesignModeState() {
            return GetDesignModeState();
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        void IControlDesignerAccessor.SetDesignModeState(IDictionary data) {
            SetDesignModeState(data);
        }

        void IControlDesignerAccessor.SetOwnerControl(Control owner) {
            if (owner == this) {
                throw new ArgumentException(SR.GetString(SR.Control_CannotOwnSelf), "owner");
            }
            OwnerControl = owner;
            _parent = owner.Parent;
            _page = owner.Page;
        }
        #endregion

        #region IControlBuilderAccessor implementation

        /// <internalonly/>
        /// <devdoc>
        /// A reference to the ControlBuilder that was used to construct this control (if there was one)
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        ControlBuilder IControlBuilderAccessor.ControlBuilder {
            get {
                return RareFields != null ? RareFields.ControlBuilder : null;
            }
        }
        #endregion IControlBuilderAccessor implementation

        #region IExpressionsAccessor


        /// <internalonly/>
        bool IExpressionsAccessor.HasExpressions {
            get {
                if (RareFields == null) {
                    return false;
                }
                ExpressionBindingCollection expressions = RareFields.ExpressionBindings;
                return ((expressions != null) && (expressions.Count > 0));
            }
        }


        /// <internalonly/>
        ExpressionBindingCollection IExpressionsAccessor.Expressions {
            get {
                ExpressionBindingCollection expressions = RareFieldsEnsured.ExpressionBindings;
                if (expressions == null) {
                    expressions = new ExpressionBindingCollection();
                    RareFields.ExpressionBindings = expressions;
                }
                return expressions;
            }
        }
        #endregion

        private sealed class ControlRareFields : IDisposable {

            internal ControlRareFields() {
            }
            public ISite Site;
            public RenderMethod RenderMethod;
            // Reference to the ControlBuilder used to build this control
            public ControlBuilder ControlBuilder;
            public DataBindingCollection DataBindings;
            public Control OwnerControl;
            public ExpressionBindingCollection ExpressionBindings;
            public bool RequiredControlState = false;

            // These fields are only used in the designer so we
            // keep them here to prevent memory bloat at runtime
            public IDictionary ControlDesignerAccessorUserData;
            public IDictionary DesignModeState;

            public Version RenderingCompatibility;
            public RouteCollection RouteCollection;
            public ControlAdapter Adapter;

            public void Dispose() {
                //do not null out for backwards compat, VSWhidbey 475940
                //Site = null;
                //RenderMethod = null;
                //DataBindings = null;
                //OwnerControl = null;
                //ExpressionBindings = null;
                //Adapter = null;
                ControlBuilder = null;
                if (OwnerControl != null)
                {
                    OwnerControl.Dispose();
                }
                ControlDesignerAccessorUserData = null;
                DesignModeState = null;
                RenderingCompatibility = null;
                RouteCollection = null;
            }
        }

        private sealed class OccasionalFields : IDisposable {
            internal OccasionalFields() {
            }

            public string SkinId;
            public IDictionary ControlsViewState;
            public int NamedControlsID;
            // Only used if we are a naming container.  It contains all the controls
            // in the namespace.
            public IDictionary NamedControls;
            public ControlRareFields RareFields;
            public String UniqueIDPrefix;

            public string SpacerImageUrl;
            public TemplateControl TemplateControl;
            public VirtualPath TemplateSourceVirtualDirectory;

            public void Dispose() {
                if (RareFields != null) {
                    RareFields.Dispose();
                }

                ControlsViewState = null;
                //do not null out for backwards compat, VSWhidbey 475940
                //NamedControls = null;
                //UniqueIDPrefix = null;
                //TemplateControl = null;
                //TemplateSourceVirtualDirectory = null;
            }
        }
    }
}
