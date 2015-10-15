//------------------------------------------------------------------------------
// <copyright file="ScriptManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Globalization;
    using System.Web.Handlers;
    using System.Web.Hosting;
    using System.Web.Resources;
    using System.Web.Script;
    using System.Web.Script.Serialization;
    using System.Web.Script.Services;
    using System.Web.Security.Cryptography;
    using System.Web.UI.Design;
    using System.Web.Util;

    [
    DefaultProperty("Scripts"),
    Designer("System.Web.UI.Design.ScriptManagerDesigner, " + AssemblyRef.SystemWebExtensionsDesign),
    NonVisualControl(),
    ParseChildren(true),
    PersistChildren(false),
    ToolboxBitmap(typeof(EmbeddedResourceFinder), "System.Web.Resources.ScriptManager.bmp")
    ]
    public class ScriptManager : Control, IPostBackDataHandler, IPostBackEventHandler, IControl, IScriptManager, IScriptManagerInternal {
        private readonly new IPage _page;
        private readonly IControl _control;
        private readonly ICompilationSection _appLevelCompilationSection;
        private readonly IDeploymentSection _deploymentSection;
        private readonly ICustomErrorsSection _customErrorsSection;
        private static bool _ajaxFrameworkAssemblyConfigChecked;
        private static Assembly _defaultAjaxFrameworkAssembly = null;
        private Assembly _ajaxFrameworkAssembly = DefaultAjaxFrameworkAssembly;

        private const int AsyncPostBackTimeoutDefault = 90;

        private ScriptMode _scriptMode;
        private string _scriptPath;
        private CompositeScriptReference _compositeScript;
        private ScriptReferenceCollection _scripts;
        private ServiceReferenceCollection _services;
        private bool? _isRestMethodCall;
        private bool? _isSecureConnection;
        private List<ScriptManagerProxy> _proxies;
        private AjaxFrameworkMode _ajaxFrameworkMode = AjaxFrameworkMode.Enabled;
        private bool _enablePartialRendering = true;
        private bool _supportsPartialRendering = true;
        internal bool _supportsPartialRenderingSetByUser;
        internal ScriptReferenceBase _applicationServicesReference;
        private string _appServicesInitializationScript;
        private bool _enableScriptGlobalization;
        private bool _enableScriptLocalization = true;
        private bool _enablePageMethods;
        private bool _loadScriptsBeforeUI = true;
        private bool _initCompleted;
        private bool _preRenderCompleted;
        private bool _isInAsyncPostBack;
        private int _asyncPostBackTimeout = AsyncPostBackTimeoutDefault;
        private bool _allowCustomErrorsRedirect = true;
        private string _asyncPostBackErrorMessage;
        private bool _zip;
        private bool _zipSet;
        private int _uniqueScriptCounter;
        private bool _enableCdn;
        private bool _enableCdnFallback = true;
        private HashSet<String> _scriptPathsDefiningSys = new HashSet<String>(StringComparer.OrdinalIgnoreCase);

        private static readonly object AsyncPostBackErrorEvent = new object();
        private static readonly object ResolveCompositeScriptReferenceEvent = new object();
        private static readonly object ResolveScriptReferenceEvent = new object();
        private static HashSet<String> _splitFrameworkScript;
        private ScriptRegistrationManager _scriptRegistration;
        private PageRequestManager _pageRequestManager;

        private ScriptControlManager _scriptControlManager;

        private ProfileServiceManager _profileServiceManager;
        private AuthenticationServiceManager _authenticationServiceManager;
        private RoleServiceManager _roleServiceManager;

        private BundleReflectionHelper _bundleReflectionHelper;

        // History fields
        private bool _enableSecureHistoryState = true;
        private bool _enableHistory;
        private bool _isNavigating;
        private string _clientNavigateHandler;
        // Using a hashtable here, which will be more efficiently serialized 
        // by the page state formatter than a Dictionary<string, object>
        // or een NameValueCollection.
        private Hashtable _initialState;
        private static readonly object NavigateEvent = new object();
        private bool _newPointCreated;

        static ScriptManager() {
            ClientScriptManager._scriptResourceMapping = new ScriptResourceMapping();
        }

        public ScriptManager() {
        }

        internal ScriptManager(IControl control,
                               IPage page,
                               ICompilationSection appLevelCompilationSection,
                               IDeploymentSection deploymentSection,
                               ICustomErrorsSection customErrorsSection,
                               Assembly ajaxFrameworkAssembly,
                               bool isSecureConnection) {
            _control = control;
            _page = page;
            _appLevelCompilationSection = appLevelCompilationSection;
            _deploymentSection = deploymentSection;
            _customErrorsSection = customErrorsSection;
            _ajaxFrameworkAssembly = ajaxFrameworkAssembly ?? DefaultAjaxFrameworkAssembly;
            _isSecureConnection = isSecureConnection;
        }

        [
        ResourceDescription("ScriptManager_AjaxFrameworkAssembly"),
        Browsable(false)
        ]
        public virtual Assembly AjaxFrameworkAssembly {
            get {
                // value is set to the static DefaultAjaxFrameworkAssembly one at constructor time,
                // so this property value can't change in the middle of a request.
                return _ajaxFrameworkAssembly;
            }
        }

        [
        DefaultValue(true),
        ResourceDescription("ScriptManager_AllowCustomErrorsRedirect"),
        Category("Behavior"),
        ]
        public bool AllowCustomErrorsRedirect {
            get {
                return _allowCustomErrorsRedirect;
            }
            set {
                _allowCustomErrorsRedirect = value;
            }
        }

        private ICompilationSection AppLevelCompilationSection {
            get {
                if (_appLevelCompilationSection != null) {
                    return _appLevelCompilationSection;
                }
                else {
                    return AppLevelCompilationSectionCache.Instance;
                }
            }
        }

        [
        DefaultValue(""),
        ResourceDescription("ScriptManager_AsyncPostBackErrorMessage"),
        Category("Behavior")
        ]
        public string AsyncPostBackErrorMessage {
            get {
                if (_asyncPostBackErrorMessage == null) {
                    return String.Empty;
                }
                return _asyncPostBackErrorMessage;
            }
            set {
                _asyncPostBackErrorMessage = value;
            }
        }

        // FxCop does not flag this as a violation, because it is an implicit implementation of
        // IScriptManagerInternal.AsyncPostBackSourceElementID.  
        // 
        [
        Browsable(false),
        SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")
        ]
        public string AsyncPostBackSourceElementID {
            get {
                return PageRequestManager.AsyncPostBackSourceElementID;
            }
        }

        [
        ResourceDescription("ScriptManager_AsyncPostBackTimeout"),
        Category("Behavior"),
        DefaultValue(AsyncPostBackTimeoutDefault)
        ]
        public int AsyncPostBackTimeout {
            get {
                return _asyncPostBackTimeout;
            }
            set {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _asyncPostBackTimeout = value;
            }
        }

        [
        ResourceDescription("ScriptManager_AuthenticationService"),
        Category("Behavior"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        MergableProperty(false),
        ]
        public AuthenticationServiceManager AuthenticationService {
            get {
                if (_authenticationServiceManager == null) {
                    _authenticationServiceManager = new AuthenticationServiceManager();
                }
                return _authenticationServiceManager;
            }
        }

        internal BundleReflectionHelper BundleReflectionHelper {
            get {
                if (_bundleReflectionHelper == null) {
                    _bundleReflectionHelper = new BundleReflectionHelper();
                }
                return _bundleReflectionHelper;
            }
            set {
                _bundleReflectionHelper = value;
            }
        }

        public static ScriptResourceMapping ScriptResourceMapping {
            get {
                return (ScriptResourceMapping)ClientScriptManager._scriptResourceMapping;
            }
        }

        [
        ResourceDescription("ScriptManager_ClientNavigateHandler"),
        Category("Behavior"),
        DefaultValue("")
        ]
        public string ClientNavigateHandler {
            get {
                return _clientNavigateHandler ?? String.Empty;
            }
            set {
                _clientNavigateHandler = value;
            }
        }

        [
        ResourceDescription("ScriptManager_CompositeScript"),
        Category("Behavior"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        MergableProperty(false),
        ]
        public CompositeScriptReference CompositeScript {
            get {
                if (_compositeScript == null) {
                    _compositeScript = new CompositeScriptReference();
                }
                return _compositeScript;
            }
        }

        internal IControl Control {
            get {
                if (_control != null) {
                    return _control;
                }
                else {
                    return this;
                }
            }
        }

        internal ICustomErrorsSection CustomErrorsSection {
            [SecurityCritical()]
            get {
                if (_customErrorsSection != null) {
                    return _customErrorsSection;
                }
                else {
                    return GetCustomErrorsSectionWithAssert();
                }
            }
        }

        internal static Assembly DefaultAjaxFrameworkAssembly {
            get {
                if ((_defaultAjaxFrameworkAssembly == null) && !_ajaxFrameworkAssemblyConfigChecked && AssemblyCache._useCompilationSection) {
                    IEnumerable<Assembly> referencedAssemblies;
                    // In a hosted environment we want to get the assemblies from the BuildManager. This will include
                    // dynamically added assemblies through the PreAppStart phase.
                    // In non-hosted scenarios (VS designer) we want to look the assemblies directly from the config system
                    // since the PreAppStart phase will not execute.
                    if (HostingEnvironment.IsHosted) {
                        referencedAssemblies = BuildManager.GetReferencedAssemblies().OfType<Assembly>();
                    }
                    else {
                        CompilationSection compilationSection = RuntimeConfig.GetAppConfig().Compilation;
                        referencedAssemblies = compilationSection.Assemblies.OfType<AssemblyInfo>().SelectMany(assemblyInfo => assemblyInfo.AssemblyInternal);
                    }

                    foreach (Assembly assembly in referencedAssemblies) {
                        if (assembly != AssemblyCache.SystemWebExtensions) {
                            AjaxFrameworkAssemblyAttribute attribute =
                                AssemblyCache.GetAjaxFrameworkAssemblyAttribute(assembly);
                            if (attribute != null) {
                                _defaultAjaxFrameworkAssembly = attribute.GetDefaultAjaxFrameworkAssembly(assembly);
                                break;
                            }
                        }
                        _ajaxFrameworkAssemblyConfigChecked = true;
                    }
                    _ajaxFrameworkAssemblyConfigChecked = true;
                }
                return _defaultAjaxFrameworkAssembly ?? AssemblyCache.SystemWebExtensions;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                _defaultAjaxFrameworkAssembly = value;
            }
        }

        private IDeploymentSection DeploymentSection {
            get {
                if (_deploymentSection != null) {
                    return _deploymentSection;
                }
                else {
                    return DeploymentSectionCache.Instance;
                }
            }
        }

        internal bool DeploymentSectionRetail {
            get {
                return DeploymentSection.Retail;
            }
        }

        [
        ResourceDescription("ScriptManager_EmptyPageUrl"),
        Category("Appearance"),
        Editor(typeof(UrlEditor), typeof(UITypeEditor)),
        DefaultValue(""),
        UrlProperty,
        SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Consistent with other asp.net url properties.")
        ]
        public virtual string EmptyPageUrl {
            get {
                return ViewState["EmptyPageUrl"] as string ?? string.Empty;
            }
            set {
                ViewState["EmptyPageUrl"] = value;
            }
        }

        [
        ResourceDescription("ScriptManager_EnableCdn"),
        Category("Behavior"),
        DefaultValue(false),
        ]
        public bool EnableCdn {
            get {
                return _enableCdn;
            }
            set {
                if (_preRenderCompleted) {
                    throw new InvalidOperationException(AtlasWeb.ScriptManager_CannotChangeEnableCdn);
                }
                _enableCdn = value;
            }
        }

        [
        ResourceDescription("ScriptManager_EnableCdnFallback"),
        Category("Behavior"),
        DefaultValue(true)
        ]
        public bool EnableCdnFallback {
            get {
                return _enableCdnFallback;
            }
            set {
                if (_preRenderCompleted) {
                    throw new InvalidOperationException(AtlasWeb.ScriptManager_CannotChangeEnableCdnFallback);
                }
                _enableCdnFallback = value;
            }
        }

        [
        ResourceDescription("ScriptManager_EnableHistory"),
        Category("Behavior"),
        DefaultValue(false),
        ]
        public bool EnableHistory {
            get {
                return _enableHistory;
            }
            set {
                if (_initCompleted) {
                    throw new InvalidOperationException(AtlasWeb.ScriptManager_CannotChangeEnableHistory);
                }
                _enableHistory = value;
            }
        }

        [
        ResourceDescription("ScriptManager_AjaxFrameworkMode"),
        Category("Behavior"),
        DefaultValue(AjaxFrameworkMode.Enabled),
        ]
        public AjaxFrameworkMode AjaxFrameworkMode {
            get {
                return _ajaxFrameworkMode;
            }
            set {
                if (value < AjaxFrameworkMode.Enabled || value > AjaxFrameworkMode.Explicit) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (_initCompleted) {
                    throw new InvalidOperationException(AtlasWeb.ScriptManager_CannotChangeAjaxFrameworkMode);
                }
                _ajaxFrameworkMode = value;
            }
        }

        [
        ResourceDescription("ScriptManager_EnablePageMethods"),
        Category("Behavior"),
        DefaultValue(false),
        ]
        public bool EnablePageMethods {
            get {
                return _enablePageMethods;
            }
            set {
                _enablePageMethods = value;
            }
        }

        [
        ResourceDescription("ScriptManager_EnablePartialRendering"),
        Category("Behavior"),
        DefaultValue(true),
        ]
        public bool EnablePartialRendering {
            get {
                return _enablePartialRendering;
            }
            set {
                if (_initCompleted) {
                    throw new InvalidOperationException(AtlasWeb.ScriptManager_CannotChangeEnablePartialRendering);
                }
                _enablePartialRendering = value;
            }
        }

        [
        ResourceDescription("ScriptManager_EnableScriptGlobalization"),
        Category("Behavior"),
        DefaultValue(false),
        ]
        public bool EnableScriptGlobalization {
            get {
                return _enableScriptGlobalization;
            }
            set {
                if (_initCompleted) {
                    throw new InvalidOperationException(AtlasWeb.ScriptManager_CannotChangeEnableScriptGlobalization);
                }
                _enableScriptGlobalization = value;
            }
        }

        [
        ResourceDescription("ScriptManager_EnableScriptLocalization"),
        Category("Behavior"),
        DefaultValue(true),
        ]
        public bool EnableScriptLocalization {
            get {
                return _enableScriptLocalization;
            }
            set {
                _enableScriptLocalization = value;
            }
        }

        [
        ResourceDescription("ScriptManager_EnableSecureHistoryState"),
        Category("Behavior"),
        DefaultValue(true),
        ]
        public bool EnableSecureHistoryState {
            get {
                return _enableSecureHistoryState;
            }
            set {
                _enableSecureHistoryState = value;
            }
        }

        internal bool HasAuthenticationServiceManager {
            get {
                return this._authenticationServiceManager != null;
            }
        }

        internal bool HasProfileServiceManager {
            get {
                return this._profileServiceManager != null;
            }
        }

        internal bool HasRoleServiceManager {
            get {
                return this._roleServiceManager != null;
            }
        }

        [Browsable(false)]
        public bool IsDebuggingEnabled {
            get {
                // Returns false when:
                // - Deployment mode is set to retail (override all other settings)
                // - ScriptMode is set to Auto or Inherit, and debugging it not enabled in web.config
                // - ScriptMode is set to Release

                if (DeploymentSectionRetail) {
                    return false;
                }
                if (ScriptMode == ScriptMode.Auto || ScriptMode == ScriptMode.Inherit) {
                    return AppLevelCompilationSection.Debug;
                }
                return (ScriptMode == ScriptMode.Debug);
            }
        }

        [
        Browsable(false),
        SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")
        ]
        public bool IsInAsyncPostBack {
            get {
                return _isInAsyncPostBack;
            }
        }

        [
        Browsable(false),
        SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")
        ]
        public bool IsNavigating {
            get {
                return _isNavigating;
            }
        }

        internal bool IsRestMethodCall {
            get {
                if (!_isRestMethodCall.HasValue) {
                    _isRestMethodCall = (Context != null) && RestHandlerFactory.IsRestMethodCall(Context.Request);
                }
                return _isRestMethodCall.Value;
            }
        }

        internal bool IsSecureConnection {
            get {
                if (!_isSecureConnection.HasValue) {
                    _isSecureConnection = (Context != null) && (Context.Request != null) && Context.Request.IsSecureConnection;
                }
                return _isSecureConnection.Value;
            }
        }

        internal IPage IPage {
            get {
                if (_page != null) {
                    return _page;
                }
                else {
                    Page page = Page;
                    if (page == null) {
                        throw new InvalidOperationException(AtlasWeb.Common_PageCannotBeNull);
                    }
                    return new PageWrapper(page);
                }
            }
        }

        // DevDiv bugs #46710: Ability to specify whether scripts are loaded inline at the top of the form (before UI), or via ScriptLoader (after UI).
        [
        ResourceDescription("ScriptManager_LoadScriptsBeforeUI"),
        Category("Behavior"),
        DefaultValue(true),
        ]
        public bool LoadScriptsBeforeUI {
            get {
                return _loadScriptsBeforeUI;
            }
            set {
                _loadScriptsBeforeUI = value;
            }
        }

        private PageRequestManager PageRequestManager {
            get {
                if (_pageRequestManager == null) {
                    _pageRequestManager = new PageRequestManager(this);
                }
                return _pageRequestManager;
            }
        }

        [
        ResourceDescription("ScriptManager_ProfileService"),
        Category("Behavior"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        MergableProperty(false),
        ]
        public ProfileServiceManager ProfileService {
            get {
                if (_profileServiceManager == null) {
                    _profileServiceManager = new ProfileServiceManager();
                }
                return _profileServiceManager;
            }
        }

        internal List<ScriptManagerProxy> Proxies {
            get {
                if (_proxies == null) {
                    _proxies = new List<ScriptManagerProxy>();
                }
                return _proxies;
            }
        }

        [
        ResourceDescription("ScriptManager_RoleService"),
        Category("Behavior"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        MergableProperty(false),
        ]
        public RoleServiceManager RoleService {
            get {
                if (_roleServiceManager == null) {
                    _roleServiceManager = new RoleServiceManager();
                }
                return _roleServiceManager;
            }
        }

        internal ScriptControlManager ScriptControlManager {
            get {
                if (_scriptControlManager == null) {
                    _scriptControlManager = new ScriptControlManager(this);
                }
                return _scriptControlManager;
            }
        }

        [
        ResourceDescription("ScriptManager_ScriptMode"),
        Category("Behavior"),
        DefaultValue(ScriptMode.Auto),
        ]
        public ScriptMode ScriptMode {
            get {
                return _scriptMode;
            }
            set {
                if (value < ScriptMode.Auto || value > ScriptMode.Release) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _scriptMode = value;
            }
        }

        internal ScriptRegistrationManager ScriptRegistration {
            get {
                if (_scriptRegistration == null) {
                    _scriptRegistration = new ScriptRegistrationManager(this);
                }
                return _scriptRegistration;
            }
        }

        [
        ResourceDescription("ScriptManager_Scripts"),
        Category("Behavior"),
        Editor("System.Web.UI.Design.CollectionEditorBase, " +
            AssemblyRef.SystemWebExtensionsDesign, typeof(UITypeEditor)),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        MergableProperty(false),
        ]
        public ScriptReferenceCollection Scripts {
            get {
                if (_scripts == null) {
                    _scripts = new ScriptReferenceCollection();
                }
                return _scripts;
            }
        }

        [
        ResourceDescription("ScriptManager_ScriptPath"),
        Category("Behavior"),
        DefaultValue(""),
        Obsolete("This property is obsolete. Set the Path property on each individual ScriptReference instead."),
        ]
        public string ScriptPath {
            get {
                return (_scriptPath == null) ? String.Empty : _scriptPath;
            }
            set {
                _scriptPath = value;
            }
        }

        [
        ResourceDescription("ScriptManager_Services"),
        Category("Behavior"),
        Editor("System.Web.UI.Design.ServiceReferenceCollectionEditor, " +
            AssemblyRef.SystemWebExtensionsDesign, typeof(UITypeEditor)),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        MergableProperty(false),
        ]
        public ServiceReferenceCollection Services {
            get {
                if (_services == null) {
                    _services = new ServiceReferenceCollection();
                }
                return _services;
            }
        }

        private static HashSet<String> SplitFrameworkScripts {
            get {
                if (_splitFrameworkScript == null) {
                    HashSet<String> scripts = new HashSet<String>();
                    scripts.Add("MicrosoftAjaxComponentModel.js");
                    scripts.Add("MicrosoftAjaxComponentModel.debug.js");
                    scripts.Add("MicrosoftAjaxCore.js");
                    scripts.Add("MicrosoftAjaxCore.debug.js");
                    scripts.Add("MicrosoftAjaxGlobalization.js");
                    scripts.Add("MicrosoftAjaxGlobalization.debug.js");
                    scripts.Add("MicrosoftAjaxHistory.js");
                    scripts.Add("MicrosoftAjaxHistory.debug.js");
                    scripts.Add("MicrosoftAjaxNetwork.js");
                    scripts.Add("MicrosoftAjaxNetwork.debug.js");
                    scripts.Add("MicrosoftAjaxSerialization.js");
                    scripts.Add("MicrosoftAjaxSerialization.debug.js");
                    scripts.Add("MicrosoftAjaxWebServices.js");
                    scripts.Add("MicrosoftAjaxWebServices.debug.js");
                    _splitFrameworkScript = scripts;
                }
                return _splitFrameworkScript;
            }
        }

        [
        Browsable(false),
        DefaultValue(true),
        SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")
        ]
        public bool SupportsPartialRendering {
            get {
                if (!EnablePartialRendering) {
                    // If the user doesn't even want partial rendering then
                    // we definitely don't support it.
                    return false;
                }
                return _supportsPartialRendering;
            }
            set {
                if (!EnablePartialRendering) {
                    throw new InvalidOperationException(AtlasWeb.ScriptManager_CannotSetSupportsPartialRenderingWhenDisabled);
                }
                if (_initCompleted) {
                    throw new InvalidOperationException(AtlasWeb.ScriptManager_CannotChangeSupportsPartialRendering);
                }
                _supportsPartialRendering = value;

                // Mark that this was explicitly set. We'll set this back to false if we
                // explicitly set the value of this property.
                _supportsPartialRenderingSetByUser = true;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override bool Visible {
            get {
                return base.Visible;
            }
            set {
                throw new NotImplementedException();
            }
        }

        internal bool Zip {
            get {
                if (!_zipSet) {
                    _zip = HeaderUtility.IsEncodingInAcceptList(IPage.Request.Headers["Accept-encoding"], "gzip");
                    _zipSet = true;
                }
                return _zip;
            }
            set {
                _zip = value;
                _zipSet = true;
            }
        }

        [
        Category("Action"),
        ResourceDescription("ScriptManager_AsyncPostBackError")
        ]
        public event EventHandler<AsyncPostBackErrorEventArgs> AsyncPostBackError {
            add {
                Events.AddHandler(AsyncPostBackErrorEvent, value);
            }
            remove {
                Events.RemoveHandler(AsyncPostBackErrorEvent, value);
            }
        }

        [
        Category("Action"),
        ResourceDescription("ScriptManager_Navigate"),
        ]
        public event EventHandler<HistoryEventArgs> Navigate {
            add {
                Events.AddHandler(NavigateEvent, value);
            }
            remove {
                Events.RemoveHandler(NavigateEvent, value);
            }
        }

        [
        Category("Action"),
        ResourceDescription("ScriptManager_ResolveCompositeScriptReference"),
        ]
        public event EventHandler<CompositeScriptReferenceEventArgs> ResolveCompositeScriptReference {
            add {
                Events.AddHandler(ResolveCompositeScriptReferenceEvent, value);
            }
            remove {
                Events.RemoveHandler(ResolveCompositeScriptReferenceEvent, value);
            }
        }

        [
        Category("Action"),
        ResourceDescription("ScriptManager_ResolveScriptReference"),
        ]
        public event EventHandler<ScriptReferenceEventArgs> ResolveScriptReference {
            add {
                Events.AddHandler(ResolveScriptReferenceEvent, value);
            }
            remove {
                Events.RemoveHandler(ResolveScriptReferenceEvent, value);
            }
        }

        public void AddHistoryPoint(string key, string value) {
            AddHistoryPoint(key, value, null);
        }

        public void AddHistoryPoint(string key, string value, string title) {
            PrepareNewHistoryPoint();
            SetStateValue(key, value);
            SetPageTitle(title);
        }

        public void AddHistoryPoint(NameValueCollection state, string title) {
            PrepareNewHistoryPoint();
            foreach (string key in state) {
                SetStateValue(key, state[key]);
            }
            SetPageTitle(title);
        }

        private void AddFrameworkLoadedCheck() {
            // Add check for Sys to give better error message when the framework failed to load.
            IPage.ClientScript.RegisterClientScriptBlock(typeof(ScriptManager), "FrameworkLoadedCheck",
                ClientScriptManager.ClientScriptStart + "if (typeof(Sys) === 'undefined') throw new Error('" +
                HttpUtility.JavaScriptStringEncode(AtlasWeb.ScriptManager_FrameworkFailedToLoad) +
                "');\r\n" + ClientScriptManager.ClientScriptEnd,
                addScriptTags: false);
        }

        private ScriptReferenceBase AddFrameworkScript(ScriptReference frameworkScript, List<ScriptReferenceBase> scripts, bool webFormsWithoutAjax) {
            int scriptIndex = 0;
            ScriptReferenceBase frameworkScriptBase = frameworkScript;
            // PERF: If scripts.Count <= scriptIndex, then there are no user-specified scripts that might match
            // the current framework script, so we don't even need to look, except for composite references.
            if (scripts.Count != 0) {
                // For each framework script we want to register, try to find it in the list of user-specified scripts.
                // If it's there, move it to the top of the list. If it's not there, add it with our default settings.
                // If multiple user-specified scripts match a framework script, the first one is moved to the top
                // of the list, and later ones will be removed via RemoveDuplicates().

                // In the scenarios when MicrosoftWebForms.js is used (partial rendering is enabled), and MicrosoftAjax.js
                // is disabled, MicrosoftWebForms.js cannot be used unless the users define and registere beforehand 
                // some equivalant script to MicrosoftAjax.js. So if there is MicrosoftWebForms.js in the list of 
                // user-specified scripts, we take no action and respect the order of the scripts that the users 
                // specified. Otherwise, we insert MicrosoftWebForms.js at the end of the user-specified script list.
                string frameworkScriptName = frameworkScript.EffectiveResourceName;
                string frameworkScriptPath = null;
                if (String.IsNullOrEmpty(frameworkScriptName)) {
                    frameworkScriptPath = frameworkScript.EffectivePath;
                }
                Assembly frameworkAssembly = frameworkScript.GetAssembly(this);
                for (int i = 0; i < scripts.Count; i++) {
                    ScriptReferenceBase script = scripts[i];
                    ScriptReference sr = script as ScriptReference;
                    if ((sr != null) &&
                        ((!String.IsNullOrEmpty(frameworkScriptName) &&
                        (sr.EffectiveResourceName == frameworkScriptName)) &&
                        (sr.GetAssembly(this) == frameworkAssembly) ||
                        (!String.IsNullOrEmpty(frameworkScriptPath) &&
                        sr.ScriptInfo.Path == frameworkScriptPath))) {

                        // If the found script is already on the top of the list, we dont need to remove then insert it back.
                        if (webFormsWithoutAjax || (i == 0)) {
                            script.AlwaysLoadBeforeUI = true;
                            return script;
                        }

                        frameworkScriptBase = script;
                        scripts.Remove(script);
                        break;
                    }
                    else {
                        CompositeScriptReference csr = script as CompositeScriptReference;
                        if (csr != null) {
                            bool found = false;
                            foreach (ScriptReference scriptReference in csr.Scripts) {
                                if (((!String.IsNullOrEmpty(frameworkScriptName) &&
                                    (scriptReference.EffectiveResourceName == frameworkScriptName)) &&
                                    (scriptReference.GetAssembly(this) == frameworkAssembly) ||
                                    (!String.IsNullOrEmpty(frameworkScriptPath) &&
                                    scriptReference.ScriptInfo.Path == frameworkScriptPath))) {

                                    // If the found script is already on the top of the list, we dont need to remove then insert it back.
                                    if (webFormsWithoutAjax || (i == 0)) {
                                        script.AlwaysLoadBeforeUI = true;
                                        return script;
                                    }
                                    // Even composite references are moved to the top if they contain an fx script.
                                    frameworkScriptBase = script;
                                    scripts.Remove(script);
                                    found = true;
                                    break;
                                }
                            }
                            if (found) {
                                break;
                            }
                        }
                    }
                }
                if (webFormsWithoutAjax) {
                    scriptIndex = scripts.Count;
                }
            }

            frameworkScriptBase.AlwaysLoadBeforeUI = true;
            scripts.Insert(scriptIndex, frameworkScriptBase);
            return frameworkScriptBase;
        }

        // Called by ScriptManagerDesigner.GetScriptReferences()
        internal void AddFrameworkScripts(List<ScriptReferenceBase> scripts) {
            // The 0 and 1 scriptIndex parameter to AddFrameworkScript() is how
            // we guarantee that the Atlas framework scripts get inserted
            // consecutively as the first script to be registered. If we add
            // more optional framework scripts we will have to increment the index
            // dynamically for each script so that there are no gaps between the
            // Atlas scripts.
            // Add MicrosoftAjaxApplicationServices.js first,
            // then MicrosoftAjaxWebForms.js, then MicrosoftAjax.js. So that the insert index is always at 0.
            // This is to fix the issue in DevDiv 664653, where there is only one composite script with both
            // MicrosoftAjaxWebForms.js and MicrosoftAjax.js, and inserting at index 1 is out of bound.
            AjaxFrameworkMode mode = AjaxFrameworkMode;
            if (mode != AjaxFrameworkMode.Disabled) {
                _appServicesInitializationScript = GetApplicationServicesInitializationScript();
                // only add the script explicitly in enabled mode -- in explicit mode, we will
                // register the initialization script only after we find the reference that was included
                // explicitly.
                if ((mode == AjaxFrameworkMode.Enabled) && !String.IsNullOrEmpty(_appServicesInitializationScript)) {
                    ScriptReference appServices = new ScriptReference("MicrosoftAjaxApplicationServices.js", this, this);
                    _applicationServicesReference = AddFrameworkScript(appServices, scripts, false);
                }
            }
            if (SupportsPartialRendering && (mode != AjaxFrameworkMode.Disabled)) {
                ScriptReference atlasWebForms = new ScriptReference("MicrosoftAjaxWebForms.js", this, this);
                AddFrameworkScript(atlasWebForms, scripts, (AjaxFrameworkMode == AjaxFrameworkMode.Explicit));
            }
            if (mode == AjaxFrameworkMode.Enabled) {
                ScriptReference atlasCore = new ScriptReference("MicrosoftAjax.js", this, this);
                atlasCore.IsDefiningSys = true;
                _scriptPathsDefiningSys.Add(atlasCore.EffectivePath);
                AddFrameworkScript(atlasCore, scripts, false);
            }
        }

        // Add ScriptReferences from Scripts collections of ScriptManager and ScriptManagerProxies
        // Called by ScriptManagerDesigner.GetScriptReferences().
        internal void AddScriptCollections(List<ScriptReferenceBase> scripts, IEnumerable<ScriptManagerProxy> proxies) {
            if ((_compositeScript != null) && (_compositeScript.Scripts.Count != 0)) {
                _compositeScript.ClientUrlResolver = Control;
                _compositeScript.ContainingControl = this;
                _compositeScript.IsStaticReference = true;
                scripts.Add(_compositeScript);
            }
            // Register user-specified scripts from the ScriptManager
            // PERF: Use field directly to avoid creating List if not already created
            if (_scripts != null) {
                foreach (ScriptReference scriptReference in _scripts) {
                    // Fix for Dev11 

                    if (scriptReference.IsAjaxFrameworkScript(this) && (scriptReference.Name.StartsWith("MicrosoftAjax.", StringComparison.OrdinalIgnoreCase) || scriptReference.Name.StartsWith("MicrosoftAjaxCore.", StringComparison.OrdinalIgnoreCase))) {
                        scriptReference.IsDefiningSys = true;
                        _scriptPathsDefiningSys.Add(scriptReference.EffectivePath);
                    }
                    scriptReference.ClientUrlResolver = Control;
                    scriptReference.ContainingControl = this;
                    scriptReference.IsStaticReference = true;
                    scripts.Add(scriptReference);
                }
            }

            // Register user-specified scripts from ScriptManagerProxy controls, if any
            if (proxies != null) {
                foreach (ScriptManagerProxy proxy in proxies) {
                    proxy.CollectScripts(scripts);
                }
            }
        }

        internal string CreateUniqueScriptKey() {
            _uniqueScriptCounter++;
            return "UniqueScript_" + _uniqueScriptCounter.ToString(CultureInfo.InvariantCulture);
        }

        private string GetApplicationServicesInitializationScript() {
            StringBuilder sb = null;

            // Script that configures the application service proxies. For example setting the path properties.
            // If the services are disabled and none of the service manager properties are set, the sb will be null.
            ProfileServiceManager.ConfigureProfileService(ref sb, Context, this, _proxies);
            AuthenticationServiceManager.ConfigureAuthenticationService(ref sb, Context, this, _proxies);
            RoleServiceManager.ConfigureRoleService(ref sb, Context, this, _proxies);

            if (sb != null && sb.Length > 0) {
                return sb.ToString();
            }
            return null;
        }

        public static ScriptManager GetCurrent(Page page) {
            if (page == null) {
                throw new ArgumentNullException("page");
            }

            return page.Items[typeof(ScriptManager)] as ScriptManager;
        }

        // AspNetHostingPermission attributes must be copied to this method, to satisfy FxCop rule
        // CA2114:MethodSecurityShouldBeASupersetOfType.
        [
        ConfigurationPermission(SecurityAction.Assert, Unrestricted = true),
        SecurityCritical()
        ]
        private static ICustomErrorsSection GetCustomErrorsSectionWithAssert() {
            return new CustomErrorsSectionWrapper(
                (CustomErrorsSection)WebConfigurationManager.GetSection("system.web/customErrors"));
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Depends on registered resources so order of execution is important.")]
        public ReadOnlyCollection<RegisteredArrayDeclaration> GetRegisteredArrayDeclarations() {
            return new ReadOnlyCollection<RegisteredArrayDeclaration>(ScriptRegistration.ScriptArrays);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Depends on registered resources so order of execution is important.")]
        public ReadOnlyCollection<RegisteredScript> GetRegisteredClientScriptBlocks() {
            // includes RegisterClientScriptBlock, RegisterClientScriptInclude, RegisterClientScriptResource
            return new ReadOnlyCollection<RegisteredScript>(ScriptRegistration.ScriptBlocks);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Depends on registered resources so order of execution is important.")]
        public ReadOnlyCollection<RegisteredDisposeScript> GetRegisteredDisposeScripts() {
            return new ReadOnlyCollection<RegisteredDisposeScript>(ScriptRegistration.ScriptDisposes);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Depends on registered resources so order of execution is important.")]
        public ReadOnlyCollection<RegisteredExpandoAttribute> GetRegisteredExpandoAttributes() {
            return new ReadOnlyCollection<RegisteredExpandoAttribute>(ScriptRegistration.ScriptExpandos);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Depends on registered resources so order of execution is important.")]
        public ReadOnlyCollection<RegisteredHiddenField> GetRegisteredHiddenFields() {
            return new ReadOnlyCollection<RegisteredHiddenField>(ScriptRegistration.ScriptHiddenFields);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Depends on registered resources so order of execution is important.")]
        public ReadOnlyCollection<RegisteredScript> GetRegisteredOnSubmitStatements() {
            return new ReadOnlyCollection<RegisteredScript>(ScriptRegistration.ScriptSubmitStatements);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Depends on registered resources so order of execution is important.")]
        public ReadOnlyCollection<RegisteredScript> GetRegisteredStartupScripts() {
            return new ReadOnlyCollection<RegisteredScript>(ScriptRegistration.ScriptStartupBlocks);
        }

        internal string GetScriptResourceUrl(string resourceName, Assembly assembly) {
            return ScriptResourceHandler.GetScriptResourceUrl(
                assembly,
                resourceName,
                (EnableScriptLocalization ? CultureInfo.CurrentUICulture : CultureInfo.InvariantCulture),
                Zip);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Getting the state string is a heavy operation.")]
        public string GetStateString() {
            if (EnableSecureHistoryState) {
                StatePersister persister = new StatePersister(Page);
                return persister.Serialize(_initialState);
            }
            else if (_initialState == null) {
                return String.Empty;
            }
            else {
                StringBuilder sb = new StringBuilder();
                bool first = true;
                foreach (DictionaryEntry kvp in _initialState) {
                    if (!first) {
                        sb.Append('&');
                    }
                    else {
                        first = false;
                    }
                    sb.Append(HttpUtility.UrlEncode((string)kvp.Key));
                    sb.Append('=');
                    sb.Append(HttpUtility.UrlEncode((string)kvp.Value));
                }
                return sb.ToString();
            }
        }

        private void LoadHistoryState(string serverState) {
            NameValueCollection state;
            if (String.IsNullOrEmpty(serverState)) {
                _initialState = new Hashtable(StringComparer.Ordinal);
                state = new NameValueCollection();
            }
            else if (EnableSecureHistoryState) {
                StatePersister persister = new StatePersister(Page);
                _initialState = (Hashtable)persister.Deserialize(serverState);

                state = new NameValueCollection();
                foreach (DictionaryEntry entry in _initialState) {
                    state.Add((string)entry.Key, (string)entry.Value);
                }
            }
            else {
                state = HttpUtility.ParseQueryString(serverState);
                _initialState = new Hashtable(state.Count, StringComparer.Ordinal);

                foreach (string key in state) {
                    _initialState.Add(key, state[key]);
                }
            }
            HistoryEventArgs args = new HistoryEventArgs(state);
            RaiseNavigate(args);
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection) {
            if (IsInAsyncPostBack) {
                PageRequestManager.LoadPostData(postDataKey, postCollection);
            }
            else if (EnableHistory && (AjaxFrameworkMode != AjaxFrameworkMode.Disabled)) {
                // get current state string if it exists
                string serverState = postCollection[postDataKey];
                LoadHistoryState(serverState);
            }

            return false;
        }

        private bool NeedToLoadBeforeUI(ScriptReference script, AjaxFrameworkMode ajaxMode) {
            return script.IsFromSystemWeb() ||
                (ajaxMode == AjaxFrameworkMode.Explicit &&
                (script.IsAjaxFrameworkScript(this) && SplitFrameworkScripts.Contains(script.EffectiveResourceName)));
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected internal virtual void OnAsyncPostBackError(AsyncPostBackErrorEventArgs e) {
            EventHandler<AsyncPostBackErrorEventArgs> handler =
                (EventHandler<AsyncPostBackErrorEventArgs>)Events[AsyncPostBackErrorEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            if (!DesignMode) {
                // Ensure the current ajax framework assembly has a higher version number than System.Web.Extensions.
                Assembly ajaxFrameworkAssembly = AjaxFrameworkAssembly;
                if ((ajaxFrameworkAssembly != null) && (ajaxFrameworkAssembly != AssemblyCache.SystemWebExtensions)) {
                    if (AssemblyCache.GetVersion(ajaxFrameworkAssembly) <=
                        AssemblyCache.GetVersion(AssemblyCache.SystemWebExtensions)) {
                        // Must have a higher version number
                        throw new InvalidOperationException(
                            String.Format(CultureInfo.CurrentUICulture, AtlasWeb.ScriptManager_MustHaveGreaterVersion,
                                ajaxFrameworkAssembly, AssemblyCache.GetVersion(AssemblyCache.SystemWebExtensions)));
                    }
                }

                IPage page = IPage;
                ScriptManager existingInstance = ScriptManager.GetCurrent(Page);

                if (existingInstance != null) {
                    throw new InvalidOperationException(AtlasWeb.ScriptManager_OnlyOneScriptManager);
                }
                page.Items[typeof(IScriptManager)] = this;
                page.Items[typeof(ScriptManager)] = this;

                page.InitComplete += OnPageInitComplete;
                page.PreRenderComplete += OnPagePreRenderComplete;

                if (page.IsPostBack) {
                    _isInAsyncPostBack = PageRequestManager.IsAsyncPostBackRequest(page.Request);
                }

                // Delegate to PageRequestManager to hook up error handling for async posts
                PageRequestManager.OnInit();

                page.PreRender += ScriptControlManager.OnPagePreRender;
            }
        }

        private void RaiseNavigate(HistoryEventArgs e) {
            EventHandler<HistoryEventArgs> handler = (EventHandler<HistoryEventArgs>)Events[NavigateEvent];
            if (handler != null) {
                handler(this, e);
            }
            foreach (ScriptManagerProxy proxy in Proxies) {
                handler = proxy.NavigateEvent;
                if (handler != null) {
                    handler(this, e);
                }
            }
        }

        private void OnPagePreRenderComplete(object sender, EventArgs e) {
            _preRenderCompleted = true;
            if (!IsInAsyncPostBack) {
                if (SupportsPartialRendering && (AjaxFrameworkMode != AjaxFrameworkMode.Disabled)) {
                    // Force ASP.NET to include the __doPostBack function. If we don't do
                    // this then on the client we might not be able to override the function
                    // (since it won't be defined).
                    // We also need to force it to include the ASP.NET WebForms.js since it
                    // has other required functionality.
                    IPage.ClientScript.GetPostBackEventReference(new PostBackOptions(this, null, null, false, false, false, false, true, null));
                }
                // on GET request we register the glob block...
                RegisterGlobalizationScriptBlock();
                // all script references, declared and from script controls...
                RegisterScripts();
                // and all service references.
                RegisterServices();
            }
            else {
                // on async postbacks we only need to register script control references and inline references.
                RegisterScripts();
                if (EnableHistory && (AjaxFrameworkMode != AjaxFrameworkMode.Disabled)) {
                    // Send empty object as null to the client
                    if ((_initialState != null) && (_initialState.Count == 0)) {
                        _initialState = null;
                    }
                    if (_newPointCreated) {
                        RegisterDataItem(this, GetStateString(), false);
                    }
                }
            }
        }

        private void OnPageInitComplete(object sender, EventArgs e) {
            if (IPage.IsPostBack) {
                if (IsInAsyncPostBack && !SupportsPartialRendering) {
                    throw new InvalidOperationException(AtlasWeb.ScriptManager_AsyncPostBackNotInPartialRenderingMode);
                }
            }

            _initCompleted = true;

            if (EnableHistory && (AjaxFrameworkMode != AjaxFrameworkMode.Disabled)) {
                RegisterAsyncPostBackControl(this);
                if (IPage.IsPostBack) {
                    // Currently navigation is the only postback that ScriptManager handles
                    // so we can assume that if the event target is the script manager, then
                    // we're navigating.
                    _isNavigating = IPage.Request[System.Web.UI.Page.postEventSourceID] == this.UniqueID;
                }
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            if (IsInAsyncPostBack) {
                PageRequestManager.OnPreRender();
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected virtual void OnResolveCompositeScriptReference(CompositeScriptReferenceEventArgs e) {
            EventHandler<CompositeScriptReferenceEventArgs> handler =
                (EventHandler<CompositeScriptReferenceEventArgs>)Events[ResolveCompositeScriptReferenceEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected virtual void OnResolveScriptReference(ScriptReferenceEventArgs e) {
            EventHandler<ScriptReferenceEventArgs> handler =
                (EventHandler<ScriptReferenceEventArgs>)Events[ResolveScriptReferenceEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        private void PrepareNewHistoryPoint() {
            if (!EnableHistory) {
                throw new InvalidOperationException(AtlasWeb.ScriptManager_CannotAddHistoryPointWithHistoryDisabled);
            }
            if (!IsInAsyncPostBack) {
                throw new InvalidOperationException(AtlasWeb.ScriptManager_CannotAddHistoryPointOutsideOfAsyncPostBack);
            }
            _newPointCreated = true;
            if (_initialState == null) {
                _initialState = new Hashtable(StringComparer.Ordinal);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate",
            Justification = "Matches IPostBackEventHandler interface.")]
        protected virtual void RaisePostBackEvent(string eventArgument) {
            LoadHistoryState(eventArgument);
        }

        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate",
            Justification = "Matches IPostBackDataHandler interface.")]
        protected virtual void RaisePostDataChangedEvent() {
        }

        // 




        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "The overload specifically exists to strengthen the support for passing in a Page parameter.")]
        public static void RegisterArrayDeclaration(Page page, string arrayName, string arrayValue) {
            ScriptRegistrationManager.RegisterArrayDeclaration(page, arrayName, arrayValue);
        }

        public static void RegisterArrayDeclaration(Control control, string arrayName, string arrayValue) {
            ScriptRegistrationManager.RegisterArrayDeclaration(control, arrayName, arrayValue);
        }

        // Registers a control as causing an async postback instead of a regular postback
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void RegisterAsyncPostBackControl(Control control) {
            PageRequestManager.RegisterAsyncPostBackControl(control);
        }

        // Internal virtual for testing.  Cannot mock static RegisterClientScriptBlock().
        internal virtual void RegisterClientScriptBlockInternal(Control control, Type type, string key, string script, bool addScriptTags) {
            RegisterClientScriptBlock(control, type, key, script, addScriptTags);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "The overload specifically exists to strengthen the support for passing in a Page parameter.")]
        public static void RegisterClientScriptBlock(Page page, Type type, string key, string script, bool addScriptTags) {
            ScriptRegistrationManager.RegisterClientScriptBlock(page, type, key, script, addScriptTags);
        }

        public static void RegisterClientScriptBlock(Control control, Type type, string key, string script, bool addScriptTags) {
            ScriptRegistrationManager.RegisterClientScriptBlock(control, type, key, script, addScriptTags);
        }

        // Internal virtual for testing.  Cannot mock static RegisterClientScriptInclude().
        internal virtual void RegisterClientScriptIncludeInternal(Control control, Type type, string key, string url) {
            RegisterClientScriptInclude(control, type, key, url);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "Needs to take same parameters as ClientScriptManager.RegisterClientScriptInclude()." +
            "We could provide an overload that takes a System.Uri parameter, but then FxCop rule " +
            "StringUriOverloadsCallSystemUriOverloads would require that the string overload call the Uri overload. " +
            "But we cannot do this, because the ClientScriptManager API allows any string, even invalid Uris. " +
            "We cannot start throwing exceptions on input we previously passed to the browser. So it does not make " +
            "sense to add an overload that takes System.Uri.")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "The overload specifically exists to strengthen the support for passing in a Page parameter.")]
        public static void RegisterClientScriptInclude(Page page, Type type, string key, string url) {
            ScriptRegistrationManager.RegisterClientScriptInclude(page, type, key, url);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "Needs to take same parameters as ClientScriptManager.RegisterClientScriptInclude()." +
            "We could provide an overload that takes a System.Uri parameter, but then FxCop rule " +
            "StringUriOverloadsCallSystemUriOverloads would require that the string overload call the Uri overload. " +
            "But we cannot do this, because the ClientScriptManager API allows any string, even invalid Uris. " +
            "We cannot start throwing exceptions on input we previously passed to the browser. So it does not make " +
            "sense to add an overload that takes System.Uri.")]
        public static void RegisterClientScriptInclude(Control control, Type type, string key, string url) {
            ScriptRegistrationManager.RegisterClientScriptInclude(control, type, key, url);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "The overload specifically exists to strengthen the support for passing in a Page parameter.")]
        public static void RegisterClientScriptResource(Page page, Type type, string resourceName) {
            ScriptRegistrationManager.RegisterClientScriptResource(page, type, resourceName);
        }

        public static void RegisterClientScriptResource(Control control, Type type, string resourceName) {
            ScriptRegistrationManager.RegisterClientScriptResource(control, type, resourceName);
        }

        // Only if we have a script manager on the page and we can resolve a path for the resource definition 
        // then can we add it to the script definitions so script references will be de-duped
        private static bool TryRegisterNamedClientScriptResourceUsingScriptReference(Page page, string resourceName) {
            if (page != null) {
                ScriptManager sm = GetCurrent(page);
                ScriptResourceDefinition def = ScriptManager.ScriptResourceMapping.GetDefinition(resourceName);
                if (sm != null && def != null) {
                        sm.Scripts.Add(new ScriptReference() { Name = resourceName });
                        return true;
                }
            }

            return false;
        }

        public static void RegisterNamedClientScriptResource(Control control, string resourceName) {
            // Try to use ScriptManager Scripts collection if we can(for de-dupe logic), otherwise fall back to RegisterClientScriptResource
            if (control != null && TryRegisterNamedClientScriptResourceUsingScriptReference(control.Page, resourceName)) {
                return;
            }
            RegisterClientScriptResource(control, typeof(ScriptManager), resourceName);
        }

        public static void RegisterNamedClientScriptResource(Page page, string resourceName) {
            // Try to use ScriptManager Scripts collection if we can(for de-dupe logic), otherwise fall back to RegisterClientScriptResource
            if (TryRegisterNamedClientScriptResourceUsingScriptReference(page, resourceName)) {
                return;
            }
            RegisterClientScriptResource(page, typeof(ScriptManager), resourceName);
        }

        public void RegisterDataItem(Control control, string dataItem) {
            RegisterDataItem(control, dataItem, false);
        }

        public void RegisterDataItem(Control control, string dataItem, bool isJsonSerialized) {
            PageRequestManager.RegisterDataItem(control, dataItem, isJsonSerialized);
        }

        public void RegisterDispose(Control control, string disposeScript) {
            if (SupportsPartialRendering && (AjaxFrameworkMode != AjaxFrameworkMode.Disabled)) {
                // DevDiv Bugs 124041: Do not register if SupportsPartialRendering=false
                // It would cause a script error since PageRequestManager will not exist on the client.
                ScriptRegistration.RegisterDispose(control, disposeScript);
            }
        }

        public static void RegisterExpandoAttribute(Control control, string controlId, string attributeName, string attributeValue, bool encode) {
            ScriptRegistrationManager.RegisterExpandoAttribute(control, controlId, attributeName, attributeValue, encode);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void RegisterExtenderControl<TExtenderControl>(TExtenderControl extenderControl, Control targetControl)
            where TExtenderControl : Control, IExtenderControl {
            ScriptControlManager.RegisterExtenderControl(extenderControl, targetControl);
        }

        private void RegisterGlobalizationScriptBlock() {
            if (EnableScriptGlobalization && (AjaxFrameworkMode != AjaxFrameworkMode.Disabled)) {
                Tuple<String, String> entry = ClientCultureInfo.GetClientCultureScriptBlock(CultureInfo.CurrentCulture);
                if ((entry != null) && !String.IsNullOrEmpty(entry.Item1)) {
                    if (IsDebuggingEnabled && (AjaxFrameworkMode == AjaxFrameworkMode.Explicit)) {
                        string script = "Type._checkDependency('MicrosoftAjaxGlobalization.js', 'ScriptManager.EnableScriptGlobalization');\r\n";
                        ScriptRegistrationManager.RegisterStartupScript(this, typeof(ScriptManager), "CultureInfoScriptCheck", script, true);
                    }
                    ScriptRegistrationManager.RegisterClientScriptBlock(this, typeof(ScriptManager), "CultureInfo", entry.Item1, true);
                    if (!String.IsNullOrEmpty(entry.Item2)) {
                        ScriptReference reference = new ScriptReference(entry.Item2, null);
#pragma warning disable 618
                        // ScriptPath is obsolete but still functional
                        reference.IgnoreScriptPath = true;
#pragma warning restore 618
                        reference.AlwaysLoadBeforeUI = true;
                        // added to Script collection instead of directly registered so that it goes through
                        // script resolution, and is resolved for debug/release version, etc.
                        // It can also be explicitly declared to customize it.
                        // e.g. <asp:ScriptReference Name="Date.HijriCalendar.js" Path="foo.js"/>
                        Scripts.Add(reference);
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "The overload specifically exists to strengthen the support for passing in a Page parameter.")]
        public static void RegisterHiddenField(Page page, string hiddenFieldName, string hiddenFieldInitialValue) {
            ScriptRegistrationManager.RegisterHiddenField(page, hiddenFieldName, hiddenFieldInitialValue);
        }

        public static void RegisterHiddenField(Control control, string hiddenFieldName, string hiddenFieldInitialValue) {
            ScriptRegistrationManager.RegisterHiddenField(control, hiddenFieldName, hiddenFieldInitialValue);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "The overload specifically exists to strengthen the support for passing in a Page parameter.")]
        public static void RegisterOnSubmitStatement(Page page, Type type, string key, string script) {
            ScriptRegistrationManager.RegisterOnSubmitStatement(page, type, key, script);
        }

        public static void RegisterOnSubmitStatement(Control control, Type type, string key, string script) {
            ScriptRegistrationManager.RegisterOnSubmitStatement(control, type, key, script);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void RegisterScriptControl<TScriptControl>(TScriptControl scriptControl)
            where TScriptControl : Control, IScriptControl {
            ScriptControlManager.RegisterScriptControl(scriptControl);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void RegisterScriptDescriptors(IExtenderControl extenderControl) {
            ScriptControlManager.RegisterScriptDescriptors(extenderControl);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void RegisterScriptDescriptors(IScriptControl scriptControl) {
            ScriptControlManager.RegisterScriptDescriptors(scriptControl);
        }

        // Registers a control as causing a regular postback instead of an async postback
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void RegisterPostBackControl(Control control) {
            PageRequestManager.RegisterPostBackControl(control);
        }

        private static string GetEffectivePath(ScriptReferenceBase scriptRef) {
            string effectivePath = scriptRef.Path;
            if (String.IsNullOrEmpty(effectivePath)) {
                // Also try using Effective path for ScriptReferences, which is the case for framework scripts
                ScriptReference sref = scriptRef as ScriptReference;
                if (sref != null) {
                    effectivePath = sref.EffectivePath;
                }
            }
            return effectivePath;
        }

        // If bundling is supported, look for references to bundles, eliminate duplicates, and get the true bundle url for the reference
        internal List<ScriptReferenceBase> ProcessBundleReferences(List<ScriptReferenceBase> scripts) {
            // If we have a bundle resolver, look through all the scripts and see which are bundles
            object resolver = BundleReflectionHelper.BundleResolver;
            if (resolver != null) {
                HashSet<string> virtualPathsInBundles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // For each bundle, expand and remember every path inside
                foreach (ScriptReferenceBase scriptRef in scripts) {
                    string effectivePath = GetEffectivePath(scriptRef);
                    if (BundleReflectionHelper.IsBundleVirtualPath(effectivePath)) {
                        scriptRef.IsBundleReference = true;

                        IEnumerable<string> bundleContents = BundleReflectionHelper.GetBundleContents(effectivePath);
                        if (bundleContents != null) {
                            foreach (string path in bundleContents) {
                                virtualPathsInBundles.Add(path);
                                if (_scriptPathsDefiningSys.Contains(path)) {
                                    scriptRef.IsDefiningSys = true;
                                }
                            }
                        }
                    }
                }

                // No bundles so we are done
                if (virtualPathsInBundles.Count == 0) {
                    return scripts;
                }

                // Go through the scripts and remove any references to scripts inside of bundles
                List<ScriptReferenceBase> collapsedReferences = new List<ScriptReferenceBase>();
                foreach (ScriptReferenceBase scriptRef in scripts) {
                    string effectivePath = GetEffectivePath(scriptRef);
                    if (scriptRef.IsBundleReference) {
                        collapsedReferences.Add(scriptRef);
                    }
                    else {
                        if (!virtualPathsInBundles.Contains(effectivePath)) {
                            collapsedReferences.Add(scriptRef);
                        }
                    }
                }

                return collapsedReferences;
            }

            return scripts;
        }

        private void RegisterScripts() {
            List<ScriptReferenceBase> scripts = new List<ScriptReferenceBase>();

            // Add ScriptReferences from Scripts collections of ScriptManager and ScriptManagerProxies
            // PERF: Use _proxies field directly to avoid creating List if not already created
            AddScriptCollections(scripts, _proxies);

            // Add ScriptReferences registered by ScriptControls and ExtenderControls
            ScriptControlManager.AddScriptReferences(scripts);

            // Inject Atlas Framework scripts
            AddFrameworkScripts(scripts);

            // Allow custom resolve work to happen
            foreach (ScriptReferenceBase script in scripts) {
                ScriptReference sr = script as ScriptReference;
                if (sr != null) {
                    OnResolveScriptReference(new ScriptReferenceEventArgs(sr));
                }
                else {
                    CompositeScriptReference csr = script as CompositeScriptReference;
                    if (csr != null) {
                        OnResolveCompositeScriptReference(new CompositeScriptReferenceEventArgs(csr));
                    }
                }
            }
            // Remove duplicate Name+Assembly references
            List<ScriptReferenceBase> uniqueScripts = RemoveDuplicates(scripts, AjaxFrameworkMode,
                                LoadScriptsBeforeUI, (IsInAsyncPostBack ? null : IPage.ClientScript),
                                ref _applicationServicesReference);

            // Remove any scripts that are contained inside of any bundles
            uniqueScripts = ProcessBundleReferences(uniqueScripts);

            // Register the final list of unique scripts
            RegisterUniqueScripts(uniqueScripts);
        }

        private void RegisterUniqueScripts(List<ScriptReferenceBase> uniqueScripts) {
            bool loadCheckRegistered = !(IsDebuggingEnabled && !IsInAsyncPostBack);
            bool hasAppServicesScript = !String.IsNullOrEmpty(_appServicesInitializationScript);
            bool loadScriptsBeforeUI = this.LoadScriptsBeforeUI;
            AjaxFrameworkMode mode = AjaxFrameworkMode;
            foreach (ScriptReferenceBase script in uniqueScripts) {
                string url = script.GetUrl(this, Zip);
                string key = url;
                if (loadScriptsBeforeUI || script.AlwaysLoadBeforeUI) {
                    RegisterClientScriptIncludeInternal(script.ContainingControl, typeof(ScriptManager), key, url);
                }
                else {
                    // this method of building the script tag matches exactly ClientScriptManager.RegisterClientScriptInclude
                    string scriptTag = "\r\n<script src=\"" + HttpUtility.HtmlAttributeEncode(url) + "\" type=\"text/javascript\"></script>";
                    RegisterStartupScriptInternal(script.ContainingControl, typeof(ScriptManager), url, scriptTag, false);
                }

                RegisterFallbackScript(script, key);

                // configure app services and check framework right after framework script is included & before other scripts
                if ((!loadCheckRegistered || hasAppServicesScript) && script.IsAjaxFrameworkScript(this) && (mode != AjaxFrameworkMode.Disabled)) {
                    // In debug mode, detect if the framework loaded properly before we reference Sys in any way.
                    if (!loadCheckRegistered && script.IsDefiningSys) {
                        AddFrameworkLoadedCheck();
                        loadCheckRegistered = true;
                    }

                    if (hasAppServicesScript && (script == _applicationServicesReference)) {
                        this.IPage.ClientScript.RegisterClientScriptBlock(typeof(ScriptManager),
                            "AppServicesConfig",
                            _appServicesInitializationScript,
                            true);
                        hasAppServicesScript = false;
                    }
                }
            }
            // note: in explicit mode it is possible at this point that hasAppServicesScript is still true
            // because the dev did not reference appservices.js. By design, we will treat this scenario
            // as if app services are not intended to be used.
            // In Enabled mode it isn't possible that the script is not included and there is init script,
            // because we add it if it doesn't already exist.
        }

        /// <summary>
        /// Registers a script that causes the local copy of a script to load in the event the Cdn is unavailable.
        /// </summary>
        /// <param name="script"></param>
        private void RegisterFallbackScript(ScriptReferenceBase script, string key) {
            if (!EnableCdn || !EnableCdnFallback) {
                return;
            }

            // If we are using Cdn, register a fallback script for the ScriptReference it is available.
            var scriptReference = script as ScriptReference;
            if (scriptReference != null) {
                var scriptInfo = scriptReference.ScriptInfo;
                if (!String.IsNullOrEmpty(scriptInfo.LoadSuccessExpression)) {
                    string fallbackPath = scriptReference.GetUrlInternal(this, Zip, useCdnPath: false);
                    if (String.IsNullOrEmpty(fallbackPath)) {
                        return;
                    }

                    if (_isInAsyncPostBack) {
                        // For async postbacks, we need to register the script with the ScriptRegistrationManager. document.write won't work.
                        ScriptRegistrationManager.RegisterFallbackScriptForAjaxPostbacks(script.ContainingControl, typeof(ScriptManager),
                            key, scriptInfo.LoadSuccessExpression, fallbackPath);
                    }
                    else {
                        RegisterClientScriptBlockInternal(script.ContainingControl, typeof(ScriptManager), scriptInfo.LoadSuccessExpression,
                            String.Format(CultureInfo.InvariantCulture, "({0})||document.write('<script type=\"text/javascript\" src=\"{1}\"><\\/script>');", scriptInfo.LoadSuccessExpression, fallbackPath),
                            addScriptTags: true);
                    }
                }
            }
        }

        private void RegisterServices() {
            // Do not attempt to resolve inline service references on PageMethod requests.
            if (_services != null) {
                foreach (ServiceReference serviceReference in _services) {
                    serviceReference.Register(this, this);
                }
            }

            if (_proxies != null) {
                foreach (ScriptManagerProxy proxy in _proxies) {
                    proxy.RegisterServices(this);
                }
            }

            if (EnablePageMethods) {
                string pageMethods = PageClientProxyGenerator.GetClientProxyScript(Context, IPage, IsDebuggingEnabled);
                if (!String.IsNullOrEmpty(pageMethods)) {
                    RegisterClientScriptBlockInternal(this, typeof(ScriptManager), pageMethods, pageMethods, true);
                }
            }
        }

        private static void RegisterResourceWithClientScriptManager(IClientScriptManager clientScriptManager, Assembly assembly, String key) {
            // Tells the ClientScriptManager that ScriptManager has registered an assembly resource, 'key' from 'assembly',
            // so that it does not register it again if it were registered with RegisterClientScriptResource. This allows
            // script references to override them, allowing devs to take advantage of setting a 'path' to override them or
            // to use ScriptCombining to combine them.
            // RegisteredResourcesToSuppress is a Dictionary of Dictionaries. The outer dictionary key is assembly,
            // the inner key is resource.
            Dictionary<Assembly, Dictionary<String, Object>> suppressedResources = clientScriptManager.RegisteredResourcesToSuppress;
            Debug.Assert(suppressedResources != null, "ClientScriptManager.RegisteredResourcesToSuppress is not expected to return null.");
            Dictionary<String, Object> resourcesForAssembly;
            if (!suppressedResources.TryGetValue(assembly, out resourcesForAssembly)) {
                resourcesForAssembly = new Dictionary<String, Object>();
                suppressedResources[assembly] = resourcesForAssembly;
            }
            resourcesForAssembly[key] = true;
        }

        // Called by ScriptManagerDesigner.GetScriptReferences().
        internal List<ScriptReferenceBase> RemoveDuplicates(List<ScriptReferenceBase> scripts,
            AjaxFrameworkMode ajaxFrameworkMode, bool loadScriptsBeforeUI, IClientScriptManager clientScriptManager,
            ref ScriptReferenceBase applicationServicesReference) {
            // ClientScriptManager.RegisterClientScriptInclude() does not register multiple scripts
            // with the same type and key.  We use the url as the key, so multiple scripts with
            // the same final url are handled by ClientScriptManager.  In ScriptManager, we only
            // need to remove ScriptReferences that we consider "the same" but that will generate
            // different urls.  For example, Name+Assembly+Path and Name+Assembly will generate
            // different urls, but we consider them to represent the same script.  For this purpose,
            // two scripts are considered "the same" iff Name is non-empty, and they have the same
            // Name and Assembly.

            // This method also removes all the script reference with name equals MicrosoftAjax.js and 
            // assembly equals System.Web.Extensions when MicrosoftAjax is disabled (MicrosoftAjaxMode == Disabled)

            // Scenario:
            // Two references from two new instances of the same component that are each in different
            // UpdatePanels, during an async post, where one update panel is updating and the other is not.
            // If we remove one of the references because we consider one a duplicate of the other, the
            // reference remaining may be for the control in the update panel that is not updating, and
            // it wouldn't be included (incorrectly).
            // So, references from components can only be considered duplicate against static
            // script references. The returned list may contain duplicates, but they will be duplicates
            // across script controls, which may potentially live in different update panels.
            // When rendering the partial update content, duplicate paths are handled and only one is output.

            // PERF: Optimize for the following common cases.
            // - One ScriptReference (i.e. MicrosoftAjax.js).
            // - Two unique ScriptReferences (i.e. MicrosoftAjax.js and MicrosoftAjaxWebForms.js).  It is
            //   unlikely there will be two non-unique ScriptReferences, since the first ScriptReference is always
            //   MicrosoftAjax.js.
            // - Reduced cost from 0.43% to 0.04% in ScriptManager\HelloWorld\Scenario.aspx.
            int numScripts = scripts.Count;
            if (ajaxFrameworkMode == AjaxFrameworkMode.Enabled) {
                if (numScripts == 1) {
                    ScriptReference script1 = scripts[0] as ScriptReference;
                    if (script1 != null) {
                        if (clientScriptManager != null) {
                            if (!String.IsNullOrEmpty(script1.EffectiveResourceName)) {
                                RegisterResourceWithClientScriptManager(clientScriptManager, script1.GetAssembly(this), script1.EffectiveResourceName);
                            }
                        }
                        return scripts;
                    }
                }
                else if (numScripts == 2) {
                    ScriptReference script1 = scripts[0] as ScriptReference;
                    ScriptReference script2 = scripts[1] as ScriptReference;
                    if ((script1 != null) && (script2 != null) &&
                        ((script1.EffectiveResourceName != script2.EffectiveResourceName) || (script1.Assembly != script2.Assembly))) {
                        if (clientScriptManager != null) {
                            if (!String.IsNullOrEmpty(script1.EffectiveResourceName)) {
                                RegisterResourceWithClientScriptManager(clientScriptManager, script1.GetAssembly(this), script1.EffectiveResourceName);
                            }
                            if (!String.IsNullOrEmpty(script2.EffectiveResourceName)) {
                                RegisterResourceWithClientScriptManager(clientScriptManager, script2.GetAssembly(this), script2.EffectiveResourceName);
                            }
                        }
                        return scripts;
                    }
                }
            }

            // PERF: HybridDictionary is significantly more performant than Dictionary<K,V>, since the number
            // of scripts will frequently be small.  Reduced cost from 1.49% to 0.43% in
            // ScriptManager\HelloWorld\Scenario.aspx.
            HybridDictionary uniqueScriptDict = new HybridDictionary(numScripts);
            List<ScriptReferenceBase> filteredScriptList = new List<ScriptReferenceBase>(numScripts);

            // CompositeScriptReferences are always included, so scan them first
            foreach (ScriptReferenceBase script in scripts) {
                var csr = script as CompositeScriptReference;
                if (csr != null) {
                    bool loadBeforeUI = false;
                    foreach (ScriptReference sr in csr.Scripts) {
                        // Previously, we weren't removing duplicate path-based scripts
                        // because the script registration was using the url as the key,
                        // which resulted in duplicates effectively being removed later.
                        // With composite script references, this ceases to be true.
                        Tuple<string, Assembly> key = (String.IsNullOrEmpty(sr.EffectiveResourceName)) ?
                            new Tuple<string, Assembly>(sr.EffectivePath, null) :
                            new Tuple<string, Assembly>(sr.EffectiveResourceName, sr.GetAssembly(this));
                        if (uniqueScriptDict.Contains(key)) {
                            // A script reference declared multiple times in one or 
                            // multiple composite script references throws.
                            throw new InvalidOperationException(
                                AtlasWeb.ScriptManager_CannotRegisterScriptInMultipleCompositeReferences);
                        }
                        else {
                            if ((clientScriptManager != null) && (key.Item2 != null)) {
                                // its a resource script, tell ClientScriptManager about it so it suppresses any
                                // calls to RegisterClientScriptResource
                                RegisterResourceWithClientScriptManager(clientScriptManager, key.Item2, key.Item1);
                            }
                            if ((ajaxFrameworkMode == AjaxFrameworkMode.Explicit) && sr.IsAjaxFrameworkScript(this) &&
                                (applicationServicesReference == null) &&
                                sr.EffectiveResourceName.StartsWith("MicrosoftAjaxApplicationServices.", StringComparison.Ordinal)) {
                                applicationServicesReference = csr;
                            }
                            if (!loadScriptsBeforeUI && !loadBeforeUI && NeedToLoadBeforeUI(sr, ajaxFrameworkMode)) {
                                csr.AlwaysLoadBeforeUI = true;
                                loadBeforeUI = true;
                            }
                            uniqueScriptDict.Add(key, sr);
                        }
                    }
                }
            }

            foreach (ScriptReferenceBase script in scripts) {
                var csr = script as CompositeScriptReference;
                if (csr != null) {
                    filteredScriptList.Add(csr);
                }
                else {
                    var sr = script as ScriptReference;
                    if (sr != null) {
                        Tuple<string, Assembly> key = (String.IsNullOrEmpty(sr.EffectiveResourceName)) ?
                            new Tuple<string, Assembly>(sr.EffectivePath, null) :
                            new Tuple<string, Assembly>(sr.EffectiveResourceName, sr.GetAssembly(this));
                        // skip over duplicates, and skip over MicrosoftAjax.[debug.].js if the mode is Explicit
                        if (!((ajaxFrameworkMode == AjaxFrameworkMode.Explicit) && sr.IsAjaxFrameworkScript(this) &&
                            (sr.EffectiveResourceName.StartsWith("MicrosoftAjax.", StringComparison.Ordinal))) && !uniqueScriptDict.Contains(key)) {
                            if (sr.IsStaticReference) {
                                // only static script references are compared against for duplicates.
                                uniqueScriptDict.Add(key, sr);
                            }
                            if ((ajaxFrameworkMode == AjaxFrameworkMode.Explicit) && sr.IsAjaxFrameworkScript(this) &&
                                (applicationServicesReference == null) &&
                                sr.EffectiveResourceName.StartsWith("MicrosoftAjaxApplicationServices.", StringComparison.Ordinal)) {
                                applicationServicesReference = sr;
                            }
                            if (!loadScriptsBeforeUI && NeedToLoadBeforeUI(sr, ajaxFrameworkMode)) {
                                sr.AlwaysLoadBeforeUI = true;
                            }
                            if ((clientScriptManager != null) && (key.Item2 != null)) {
                                // its a resource script, tell ClientScriptManager about it so it suppresses any
                                // calls to RegisterClientScriptResource
                                RegisterResourceWithClientScriptManager(clientScriptManager, key.Item2, key.Item1);
                            }
                            filteredScriptList.Add(sr);
                        }
                    }
                }
            }
            return filteredScriptList;
        }

        // Internal virtual for testing.  Cannot mock static RegisterStartupScript().
        internal virtual void RegisterStartupScriptInternal(Control control, Type type, string key, string script, bool addScriptTags) {
            RegisterStartupScript(control, type, key, script, addScriptTags);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "The overload specifically exists to strengthen the support for passing in a Page parameter.")]
        public static void RegisterStartupScript(Page page, Type type, string key, string script, bool addScriptTags) {
            ScriptRegistrationManager.RegisterStartupScript(page, type, key, script, addScriptTags);
        }

        public static void RegisterStartupScript(Control control, Type type, string key, string script, bool addScriptTags) {
            ScriptRegistrationManager.RegisterStartupScript(control, type, key, script, addScriptTags);
        }

        protected internal override void Render(HtmlTextWriter writer) {
            if (!IsInAsyncPostBack && (AjaxFrameworkMode != AjaxFrameworkMode.Disabled)) {
                if (!((IControl)this).DesignMode && SupportsPartialRendering) {
                    PageRequestManager.Render(writer);
                }

                if (EnableHistory && !DesignMode && (IPage != null)) {
                    // Render hidden field for the script manager
                    writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
                    writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
                    writer.RenderBeginTag(HtmlTextWriterTag.Input);
                    writer.RenderEndTag();

                    // Render initialization script
                    // Dev10 540269: History startup script moved to directly rendered script so that
                    // (1) is occurs before the iframe is rendered
                    // (2) after the state hidden field
                    // A ClientScript block would violate #2, and StartupScript violates #1.
                    // Navigation handler remains as a StartupScript since the handler may be defined inline within the page.
                    JavaScriptSerializer serializer = new JavaScriptSerializer(new SimpleTypeResolver());
                    writer.Write(ClientScriptManager.ClientScriptStart);
                    if (IsDebuggingEnabled && (AjaxFrameworkMode == AjaxFrameworkMode.Explicit)) {
                        writer.WriteLine("Type._checkDependency('MicrosoftAjaxHistory.js', 'ScriptManager.EnableHistory');");
                    }
                    writer.Write("Sys.Application.setServerId(");
                    writer.Write(serializer.Serialize(ClientID));
                    writer.Write(", ");
                    writer.Write(serializer.Serialize(UniqueID));
                    writer.WriteLine(");");
                    if ((_initialState != null) && (_initialState.Count != 0)) {
                        writer.Write("Sys.Application.setServerState('");
                        writer.Write(HttpUtility.JavaScriptStringEncode(GetStateString()));
                        writer.WriteLine("');");
                    }
                    writer.WriteLine("Sys.Application._enableHistoryInScriptManager();");
                    writer.Write(ClientScriptManager.ClientScriptEnd);

                    if (!String.IsNullOrEmpty(ClientNavigateHandler)) {
                        string script = "Sys.Application.add_navigate(" + ClientNavigateHandler + ");";
                        ScriptManager.RegisterStartupScript(this, typeof(ScriptManager), "HistoryNavigate", script, true);
                    }

                    HttpBrowserCapabilitiesBase browserCaps = IPage.Request.Browser;
                    // Generating the iframe on the server-side allows access to its document
                    // without access denied errors. This enables the title to be updated in history.
                    // DevDiv 13920: Output the iframe even if it looks like IE8+ since it could be in a lower document mode
                    if (browserCaps.Browser.Equals("IE", StringComparison.OrdinalIgnoreCase)) {
                        // DevDivBugs 196735
                        // Empty page title causes us to name the first server history point after
                        // the last thing navigated to, which is the url of the frame. That shows some
                        // garbled characters in the history list in case it contains multibyte chars in IE. 
                        // We never want the page title to be empty.
                        if (String.IsNullOrEmpty(IPage.Title)) {
                            IPage.Title = AtlasWeb.ScriptManager_PageUntitled;
                        }
                        string iFrameUrl = (EmptyPageUrl.Length == 0) ?
                            ScriptResourceHandler.GetEmptyPageUrl(IPage.Title) :
                            EmptyPageUrl +
                                ((EmptyPageUrl.IndexOf('?') != -1) ? "&title=" : "?title=") +
                                IPage.Title;
                        writer.AddAttribute(HtmlTextWriterAttribute.Id, "__historyFrame");
                        writer.AddAttribute(HtmlTextWriterAttribute.Src, iFrameUrl);
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
                        writer.RenderBeginTag(HtmlTextWriterTag.Iframe);
                        writer.RenderEndTag();
                    }
                }
            }
            base.Render(writer);
        }

        public void SetFocus(Control control) {
            PageRequestManager.SetFocus(control);
        }

        private void SetPageTitle(string title) {
            if ((Page != null) && (Page.Header != null)) {
                Page.Title = title;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")]
        public void SetFocus(string clientID) {
            PageRequestManager.SetFocus(clientID);
        }

        private void SetStateValue(string key, string value) {
            if (value == null) {
                if (_initialState.ContainsKey(key)) {
                    _initialState.Remove(key);
                }
            }
            else {
                if (_initialState.ContainsKey(key)) {
                    _initialState[key] = value;
                }
                else {
                    _initialState.Add(key, value);
                }
            }
        }

        #region IControl Members
        HttpContextBase IControl.Context {
            get {
                return new System.Web.HttpContextWrapper(Context);
            }
        }

        bool IControl.DesignMode {
            get {
                return DesignMode;
            }
        }
        #endregion

        #region IScriptManagerInternal Members
        void IScriptManagerInternal.RegisterProxy(ScriptManagerProxy proxy) {
            // Under normal circumstances a ScriptManagerProxy will only register once per page lifecycle.
            // However, a malicious page developer could trick a proxy into registering more than once,
            // and this is guarding against that.
            if (!Proxies.Contains(proxy)) {
                Proxies.Add(proxy);
            }
        }

        void IScriptManagerInternal.RegisterUpdatePanel(UpdatePanel updatePanel) {
            PageRequestManager.RegisterUpdatePanel(updatePanel);
        }

        void IScriptManagerInternal.UnregisterUpdatePanel(UpdatePanel updatePanel) {
            PageRequestManager.UnregisterUpdatePanel(updatePanel);
        }
        #endregion

        #region IPostBackDataHandler Members
        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }
        #endregion

        #region IPostBackEventHandler Members
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }
        #endregion

        #region IScriptManager Members
        void IScriptManager.RegisterArrayDeclaration(Control control, string arrayName, string arrayValue) {
            RegisterArrayDeclaration(control, arrayName, arrayValue);
        }

        void IScriptManager.RegisterClientScriptBlock(Control control, Type type, string key, string script, bool addScriptTags) {
            RegisterClientScriptBlock(control, type, key, script, addScriptTags);
        }

        void IScriptManager.RegisterClientScriptInclude(Control control, Type type, string key, string url) {
            RegisterClientScriptInclude(control, type, key, url);
        }

        void IScriptManager.RegisterClientScriptResource(Control control, Type type, string resourceName) {
            RegisterClientScriptResource(control, type, resourceName);
        }

        void IScriptManager.RegisterDispose(Control control, string disposeScript) {
            RegisterDispose(control, disposeScript);
        }

        void IScriptManager.RegisterExpandoAttribute(Control control, string controlId, string attributeName, string attributeValue, bool encode) {
            RegisterExpandoAttribute(control, controlId, attributeName, attributeValue, encode);
        }

        void IScriptManager.RegisterHiddenField(Control control, string hiddenFieldName, string hiddenFieldValue) {
            RegisterHiddenField(control, hiddenFieldName, hiddenFieldValue);
        }

        void IScriptManager.RegisterOnSubmitStatement(Control control, Type type, string key, string script) {
            RegisterOnSubmitStatement(control, type, key, script);
        }

        void IScriptManager.RegisterPostBackControl(Control control) {
            RegisterPostBackControl(control);
        }

        void IScriptManager.RegisterStartupScript(Control control, Type type, string key, string script, bool addScriptTags) {
            RegisterStartupScript(control, type, key, script, addScriptTags);
        }

        void IScriptManager.SetFocusInternal(string clientID) {
            PageRequestManager.SetFocusInternal(clientID);
        }

        bool IScriptManager.IsSecureConnection {
            get {
                return IsSecureConnection;
            }
        }
        #endregion

        // The following class will hijack the page's state persister with its current
        // settings so the history server state will be just as safe as the viewstate.
        private class StatePersister : PageStatePersister {
            public StatePersister(Page page) : base(page) { }

            public override void Load() {
                throw new NotImplementedException();
            }
            public override void Save() {
                throw new NotImplementedException();
            }

            public string Serialize(object state) {
                return StateFormatter2.Serialize(state, Purpose.WebForms_ScriptManager_HistoryState);
            }

            public object Deserialize(string serialized) {
                return StateFormatter2.Deserialize(serialized, Purpose.WebForms_ScriptManager_HistoryState);
            }
        }
    }
}
