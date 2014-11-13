//------------------------------------------------------------------------------
// <copyright file="Page.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// Uncomment out this line to display rare field statistics at the end of the page
//#define DISPLAYRAREFIELDSTATISTICS

/*
 * Page class definition
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web.UI {

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.EnterpriseServices;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Handlers;
using System.Web.Hosting;
using System.Web.Management;
using System.Web.RegularExpressions;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI.Adapters;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.Util;
using System.Xml;
using System.Web.Routing;
using System.Web.ModelBinding;
using System.Web.Security.Cryptography;



/// <devdoc>
///    Default ControlBuilder used to parse page files.
/// </devdoc>
public class FileLevelPageControlBuilder: RootBuilder {

    private ArrayList _contentBuilderEntries;
    private ControlBuilder _firstControlBuilder;
    private int _firstLiteralLineNumber;
    private bool _containsContentPage;
    private string _firstLiteralText;

    internal ICollection ContentBuilderEntries {
        get {
            return _contentBuilderEntries;
        }
    }

    public override void AppendLiteralString(string text) {
        if (_firstLiteralText == null) {
            if (!Util.IsWhiteSpaceString(text)) {
                int iFirstNonWhiteSpace = Util.FirstNonWhiteSpaceIndex(text);
                if (iFirstNonWhiteSpace < 0) iFirstNonWhiteSpace = 0;
                _firstLiteralLineNumber = Parser._lineNumber - Util.LineCount(text, iFirstNonWhiteSpace, text.Length);
                _firstLiteralText = text;

                if (_containsContentPage) {
                    throw new HttpException(SR.GetString(SR.Only_Content_supported_on_content_page));
                }
            }
        }

        base.AppendLiteralString(text);
    }

    public override void AppendSubBuilder(ControlBuilder subBuilder) {
        // Tell the sub builder that it's about to be appended to its parent

        if (subBuilder is ContentBuilderInternal) {
            ContentBuilderInternal contentBuilder = (ContentBuilderInternal)subBuilder;

            _containsContentPage = true;

            if (_contentBuilderEntries == null) {
                _contentBuilderEntries = new ArrayList();
            }

            if (_firstLiteralText != null) {
                throw new HttpParseException(SR.GetString(SR.Only_Content_supported_on_content_page),
                    null, Parser.CurrentVirtualPath, _firstLiteralText, _firstLiteralLineNumber);
            }

            if (_firstControlBuilder != null) {
                Parser._lineNumber = _firstControlBuilder.Line;
                throw new HttpException(SR.GetString(SR.Only_Content_supported_on_content_page));
            }

            TemplatePropertyEntry entry = new TemplatePropertyEntry();
            entry.Filter = contentBuilder.ContentPlaceHolderFilter;
            entry.Name = contentBuilder.ContentPlaceHolder;
            entry.Builder = contentBuilder;

            _contentBuilderEntries.Add(entry);
        }
        else {
            if (_firstControlBuilder == null) {
                if (_containsContentPage) {
                    throw new HttpException(SR.GetString(SR.Only_Content_supported_on_content_page));
                }

                _firstControlBuilder = subBuilder;
            }
        }

        base.AppendSubBuilder(subBuilder);
    }

    internal override void InitObject(object obj) {
        base.InitObject(obj);

        if (_contentBuilderEntries == null)
            return;

        ICollection entries = GetFilteredPropertyEntrySet(_contentBuilderEntries);

        foreach(TemplatePropertyEntry entry in entries) {
            ContentBuilderInternal contentBuilder = (ContentBuilderInternal)entry.Builder;
            try {
                contentBuilder.SetServiceProvider(ServiceProvider);

                // Note that 'obj' can be either a Page or a MasterPage,
                // hence the need for this virtual method.
                AddContentTemplate(obj, contentBuilder.ContentPlaceHolder, contentBuilder.BuildObject() as ITemplate);
            }
            finally {
                contentBuilder.SetServiceProvider(null);
            }
        }
    }

    internal virtual void AddContentTemplate(object obj, string templateName, ITemplate template) {
        Page page = (Page)obj;
        page.AddContentTemplate(templateName, template);
    }

    internal override void SortEntries() {
        base.SortEntries();

        FilteredPropertyEntryComparer comparer = null;
        ProcessAndSortPropertyEntries(_contentBuilderEntries, ref comparer);
    }
}

/// <devdoc>
///    <para>
///       Defines the properties, methods, and events common to
///       all pages that are processed on the server by the Web Forms page framework.
///    <see langword='Page '/>
///    objects are compiled and cached in
///    memory when any ASP.NET page is
///    requested.</para>
///    <para>This class is not marked as abstract, because the VS designer
///          needs to instantiate it when opening .ascx files</para>
/// </devdoc>
[
DefaultEvent("Load"),
Designer("Microsoft.VisualStudio.Web.WebForms.WebFormDesigner, " + AssemblyRef.MicrosoftVisualStudioWeb, typeof(IRootDesigner)),
DesignerCategory("ASPXCodeBehind"),
DesignerSerializer("Microsoft.VisualStudio.Web.WebForms.WebFormCodeDomSerializer, " + AssemblyRef.MicrosoftVisualStudioWeb,  "System.ComponentModel.Design.Serialization.TypeCodeDomSerializer, " + AssemblyRef.SystemDesign),
ToolboxItem(false)
]
public class Page: TemplateControl, IHttpHandler {
    private const string HiddenClassName = "aspNetHidden";
    private const string PageID = "__Page";
    private const string PageScrollPositionScriptKey = "PageScrollPositionScript";
    private const string PageSubmitScriptKey = "PageSubmitScript";
    private const string PageReEnableControlsScriptKey = "PageReEnableControlsScript";

    // NOTE: Make sure this stays in [....] with MobilePage.PageRegisteredControlsThatRequirePostBackKey
    // 
    private const string PageRegisteredControlsThatRequirePostBackKey = "__ControlsRequirePostBackKey__";

    private const string EnabledControlArray = "__enabledControlArray";

    //used by TemplateControl to hookup auto-events
    internal static readonly object EventPreRenderComplete = new Object();
    internal static readonly object EventPreLoad = new object();
    internal static readonly object EventLoadComplete = new object();
    internal static readonly object EventPreInit = new object();
    internal static readonly object EventInitComplete = new object();
    internal static readonly object EventSaveStateComplete = new object();

    private static readonly Version FocusMinimumEcmaVersion = new Version("1.4");
    private static readonly Version FocusMinimumJScriptVersion = new Version("3.0");
    private static readonly Version JavascriptMinimumVersion = new Version("1.0");
    private static readonly Version MSDomScrollMinimumVersion = new Version("4.0");

    // Review: this is consistent with MMIT legacy -do we prefer two underscores?
    private static readonly string     UniqueFilePathSuffixID = "__ufps";
    private string _uniqueFilePathSuffix;

    internal static readonly int DefaultMaxPageStateFieldLength = -1;
    internal static readonly int DefaultAsyncTimeoutSeconds = 45;

    private int _maxPageStateFieldLength = DefaultMaxPageStateFieldLength;
    private string _requestViewState;
    private bool _cachedRequestViewState;

    private PageAdapter _pageAdapter;

    // Has the page layout changed since last request
    private bool _fPageLayoutChanged;

    private bool _haveIdSeparator;
    private char _idSeparator;

    // Session state
    private bool                _sessionRetrieved;
    private HttpSessionState    _session;

    private int _transactionMode; /* 0 = TransactionOption.Disabled*/
    private bool _aspCompatMode;
    private bool _asyncMode;

    // Async related
    private static readonly TimeSpan _maxAsyncTimeout = TimeSpan.FromMilliseconds(Int32.MaxValue);
    private TimeSpan _asyncTimeout;
    private bool _asyncTimeoutSet;

    private PageAsyncTaskManager _asyncTaskManager;
    private LegacyPageAsyncTaskManager _legacyAsyncTaskManager;
    private LegacyPageAsyncInfo _legacyAsyncInfo;

    // Page culture and uiculture set dynamically
    private CultureInfo _dynamicCulture;
    private CultureInfo _dynamicUICulture;

    // ViewState
    private string _clientState;
    private PageStatePersister _persister;
    internal ControlSet _registeredControlsRequiringControlState;
    private StringSet _controlStateLoadedControlIds;
    internal HybridDictionary _registeredControlsRequiringClearChildControlState;
    internal const ViewStateEncryptionMode EncryptionModeDefault = ViewStateEncryptionMode.Auto;
    private ViewStateEncryptionMode _encryptionMode = EncryptionModeDefault;
    private bool _viewStateEncryptionRequested;

    private ArrayList _enabledControls;

    // Http Intrinsics
    internal HttpRequest _request;
    internal HttpResponse _response;
    internal HttpApplicationState _application;
    internal Cache _cache;

    internal string _errorPage;
    private string _clientTarget;

    // Form related fields
    private HtmlForm _form;
    private bool _inOnFormRender;
    private bool _fOnFormRenderCalled;
    private bool _fRequireWebFormsScript;
    private bool _fWebFormsScriptRendered;
    private bool _fRequirePostBackScript;
    private bool _fPostBackScriptRendered;
    private bool _containsCrossPagePost;
    private RenderMethod _postFormRenderDelegate;
    internal Dictionary<String, String> _hiddenFieldsToRender;

    private bool _requireFocusScript;

    private bool _profileTreeBuilt;

    internal const bool MaintainScrollPositionOnPostBackDefault = false;
    private bool _maintainScrollPosition = MaintainScrollPositionOnPostBackDefault;

    private ClientScriptManager _clientScriptManager;

    // Needed to support Validators in AJAX 1.0 (Windows OS Bugs 2015831)
    private static Type _scriptManagerType;

    internal const bool EnableViewStateMacDefault = true;
    internal const bool EnableEventValidationDefault = true;

    internal const string systemPostFieldPrefix = "__";

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public const string postEventSourceID = systemPostFieldPrefix + "EVENTTARGET";

    private const string lastFocusID = systemPostFieldPrefix + "LASTFOCUS";
    private const string _scrollPositionXID = systemPostFieldPrefix + "SCROLLPOSITIONX";
    private const string _scrollPositionYID = systemPostFieldPrefix + "SCROLLPOSITIONY";

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public const string postEventArgumentID = systemPostFieldPrefix + "EVENTARGUMENT";

    internal const string ViewStateFieldPrefixID = systemPostFieldPrefix + "VIEWSTATE";
    internal const string ViewStateFieldCountID = ViewStateFieldPrefixID + "FIELDCOUNT";
    internal const string ViewStateGeneratorFieldID = ViewStateFieldPrefixID + "GENERATOR";
    internal const string ViewStateEncryptionID = systemPostFieldPrefix + "VIEWSTATEENCRYPTED";
    internal const string EventValidationPrefixID = systemPostFieldPrefix + "EVENTVALIDATION";
    // Any change in this constant must be duplicated in DetermineIsExportingWebPart
    internal const string WebPartExportID = systemPostFieldPrefix + "WEBPARTEXPORT";

    private bool _requireScrollScript;
    private bool _isCallback;
    private bool _isCrossPagePostBack;
    private bool _containsEncryptedViewState;
    private bool _enableEventValidation = EnableEventValidationDefault;
    internal const string callbackID = systemPostFieldPrefix + "CALLBACKID";
    internal const string callbackParameterID = systemPostFieldPrefix + "CALLBACKPARAM";
    internal const string callbackLoadScriptID = systemPostFieldPrefix + "CALLBACKLOADSCRIPT";
    internal const string callbackIndexID = systemPostFieldPrefix + "CALLBACKINDEX";

    internal const string previousPageID = systemPostFieldPrefix + "PREVIOUSPAGE";

    // BasePartialCachingControl's currently on the stack
    private Stack _partialCachingControlStack;

    private ArrayList   _controlsRequiringPostBack;
    private ArrayList   _registeredControlsThatRequirePostBack;
    private NameValueCollection _leftoverPostData;
    private IPostBackEventHandler _registeredControlThatRequireRaiseEvent;
    private ArrayList _changedPostDataConsumers;

    private bool _needToPersistViewState;
    private bool _enableViewStateMac;
    private string _viewStateUserKey;

    private string _themeName;
    private PageTheme _theme;
    private string _styleSheetName;
    private PageTheme _styleSheet;

    private VirtualPath _masterPageFile;
    private MasterPage _master;
    private IDictionary _contentTemplateCollection;

    private SmartNavigationSupport _smartNavSupport;
    internal HttpContext _context;

    private ValidatorCollection _validators;
    private bool _validated;

    private HtmlHead _header;
    private int _supportsStyleSheets;

    private Control _autoPostBackControl;

    private string _focusedControlID;
    private Control _focusedControl;
    private string _validatorInvalidControl;

    private int _scrollPositionX;
    private int _scrollPositionY;

    private Page _previousPage;
    private VirtualPath _previousPagePath;

    private bool _preInitWorkComplete;

    private bool _clientSupportsJavaScriptChecked;
    private bool _clientSupportsJavaScript;

    private string _titleToBeSet;
    private string _descriptionToBeSet;
    private string _keywordsToBeSet;

    private ICallbackEventHandler _callbackControl;

    // DevDiv 33149, 43258: A backward compat. switch for Everett rendering,
    private bool _xhtmlConformanceModeSet;
    private XhtmlConformanceMode _xhtmlConformanceMode;

    // const masks into the BitVector32
    private const int styleSheetInitialized          = 0x00000001;
    private const int isExportingWebPart             = 0x00000002;
    private const int isExportingWebPartShared       = 0x00000004;
    private const int isCrossPagePostRequest         = 0x00000008;
    // Needed to support Validators in AJAX 1.0 (Windows OS Bugs 2015831)
    private const int isPartialRenderingSupported    = 0x00000010;
    private const int isPartialRenderingSupportedSet = 0x00000020;
    private const int skipFormActionValidation       = 0x00000040;
    private const int wasViewStateMacErrorSuppressed = 0x00000080;

    // Todo: Move boolean fields into _pageFlags.
    #pragma warning disable 0649
    private SimpleBitVector32 _pageFlags;
    #pragma warning restore 0649

    // Can be either Context.Request.Form or Context.Request.QueryString
    // depending on the method used.
    private NameValueCollection _requestValueCollection;
    // The unvalidated version of _requestValueCollection
    private NameValueCollection _unvalidatedRequestValueCollection;

    private ModelStateDictionary _modelState;
    private ModelBindingExecutionContext _modelBindingExecutionContext;

    private UnobtrusiveValidationMode? _unobtrusiveValidationMode;

    private static StringSet s_systemPostFields;
    static Page() {
        // Create a static hashtable with all the names that should be
        // ignored in ProcessPostData().
        s_systemPostFields = new StringSet();
        s_systemPostFields.Add(postEventSourceID);
        s_systemPostFields.Add(postEventArgumentID);
        s_systemPostFields.Add(ViewStateFieldCountID);
        s_systemPostFields.Add(ViewStateGeneratorFieldID);
        s_systemPostFields.Add(ViewStateFieldPrefixID);
        s_systemPostFields.Add(ViewStateEncryptionID);
        s_systemPostFields.Add(previousPageID);
        s_systemPostFields.Add(callbackID);
        s_systemPostFields.Add(callbackParameterID);
        s_systemPostFields.Add(lastFocusID);
        s_systemPostFields.Add(UniqueFilePathSuffixID);
        s_systemPostFields.Add(HttpResponse.RedirectQueryStringVariable);
        s_systemPostFields.Add(EventValidationPrefixID);
    }

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.Page'/> class.</para>
    /// </devdoc>
    public Page() {
        _page = this;   // Set the page to ourselves

        _enableViewStateMac = EnableViewStateMacDefault;

        // Ensure that the page has an ID, for things like trace
        ID = PageID;

        _supportsStyleSheets = -1;

        // Set the default ValidateRequestMode of a page to Enabled since the value Inherit
        // does not make sense as the page has nobody to inherit from.
        // Also, since this is the default value for Page, we do not want this value to be
        // stored in ViewState , so we set the value here but do not set the property changed
        // flag so that it's not stored in ViewState by default.
        SetValidateRequestModeInternal(ValidateRequestMode.Enabled, setDirty: false);
    }

    public ModelStateDictionary ModelState {
        get {
            if (_modelState == null) {
                _modelState = new ModelStateDictionary();
            }
            return _modelState;
        }
    }

    private IValueProvider ActiveValueProvider {
        get;
        set;
    }

    public ModelBindingExecutionContext ModelBindingExecutionContext {
        get {
            if (_modelBindingExecutionContext == null) {
                _modelBindingExecutionContext = new ModelBindingExecutionContext(new HttpContextWrapper(Context), this.ModelState);

                //This is used to query the ViewState in ViewStateValueProvider later.
                _modelBindingExecutionContext.PublishService<StateBag>(ViewState);

                //This is used to query RouteData in RouteDataValueProvider later.
                _modelBindingExecutionContext.PublishService<RouteData>(RouteData);
            }
            return _modelBindingExecutionContext;
        }
    }
    
    /// <summary>
    /// We support calling TryUpdateModel only within a Data Method of DataBoundControl.
    /// So this method provides a way to enfore that.
    /// This sets the active value provider which is used to provide the values for
    /// TryUpdateModel. This method should be called before calling TryUpdateModel otherwise the latter
    /// would throw. Also it's callers responsibility to reset the active value Provider by calling this 
    /// method again with null values. (Currently this is all done by ModelDataSourceView).
    /// </summary>
    internal void SetActiveValueProvider(IValueProvider valueProvider) {
        ActiveValueProvider = valueProvider;
    }

    /// <summary>
    /// Attempts to update the model object from the values within a databound control. This
    /// must be invoked within the Select/Update/Delete/InsertMethods used for data binding. 
    /// </summary>
    /// <returns>True if the model object is updated succesfully with valid values. False otherwise.</returns>
    public virtual bool TryUpdateModel<TModel>(TModel model) where TModel : class {

        if (ActiveValueProvider == null) {
            throw new InvalidOperationException(SR.GetString(SR.Page_InvalidUpdateModelAttempt, "TryUpdateModel"));
        }

        return TryUpdateModel<TModel>(model, ActiveValueProvider);
    }

    /// <summary>
    /// Attempts to update the model object from the values provided by given valueProvider.
    /// </summary>
    /// <returns>True if the model object is updated succesfully with valid values. False otherwise.</returns>
    public virtual bool TryUpdateModel<TModel>(TModel model, IValueProvider valueProvider) where TModel : class {

        if (model == null) {
            throw new ArgumentNullException("model");
        }

        if (valueProvider == null) {
            throw new ArgumentNullException("valueProvider");
        }

        IModelBinder binder = ModelBinders.Binders.DefaultBinder;

        ModelBindingContext bindingContext = new ModelBindingContext() {
            ModelBinderProviders = ModelBinderProviders.Providers,
            ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, typeof(TModel)),
            ModelState = ModelState,
            ValueProvider = valueProvider
        };

        if (binder.BindModel(ModelBindingExecutionContext, bindingContext)) {
            return ModelState.IsValid;
        }

        //ModelBinding failed!!!
        return false;
    }

    /// <summary>
    /// Updates the model object from the values within a databound control. This must be invoked 
    /// within the Select/Update/Delete/InsertMethods used for data binding.
    /// Throws an exception if the update fails.
    /// </summary>
    public virtual void UpdateModel<TModel>(TModel model) where TModel : class {

        if (ActiveValueProvider == null) {
            throw new InvalidOperationException(SR.GetString(SR.Page_InvalidUpdateModelAttempt, "UpdateModel"));
        }

        UpdateModel<TModel>(model, ActiveValueProvider);
    }

    /// <summary>
    /// Updates the model object from the values provided by given valueProvider.
    /// Throws an exception if the update fails.
    /// </summary>
    public virtual void UpdateModel<TModel>(TModel model, IValueProvider valueProvider) where TModel : class {
        if (!TryUpdateModel(model, valueProvider)) {
            throw new InvalidOperationException(SR.GetString(SR.Page_UpdateModel_UpdateUnsuccessful, typeof(TModel).FullName));
        }
    }

    [
    DefaultValue(UnobtrusiveValidationMode.None),
    WebCategory("Behavior"),
    WebSysDescription(SR.Page_UnobtrusiveValidationMode)
    ]
    public UnobtrusiveValidationMode UnobtrusiveValidationMode {
        get {
            return _unobtrusiveValidationMode ?? ValidationSettings.UnobtrusiveValidationMode;
        }
        set {
            if (value < UnobtrusiveValidationMode.None || value > UnobtrusiveValidationMode.WebForms) {
                throw new ArgumentOutOfRangeException("value");
            }

            _unobtrusiveValidationMode = value;
        }
    }

    /// <devdoc>
    /// <para>Gets the <see langword='Application'/> object provided by the HTTP Runtime.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public HttpApplicationState Application {
        get {
            return _application;
        }
    }

    /// <devdoc>
    /// <para>Gets the HttpContext for the Page.</para>
    /// </devdoc>
    protected internal override HttpContext Context {
        [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        get {
            if (_context == null) {
                _context = HttpContext.Current;
            }
            return _context;
        }
    }

    // Set of unique control ids which have already loaded control state
    private StringSet ControlStateLoadedControlIds {
        get {
            if (_controlStateLoadedControlIds == null) {
                _controlStateLoadedControlIds = new StringSet();
            }
            return _controlStateLoadedControlIds;
        }
    }

    /// <devdoc>
    ///     The value to be written to the __VIEWSTATE hidden fields.  Getter is exposed through a protected property in
    ///     PageAdapter.
    /// </devdoc>
    internal string ClientState {
        get {
            return _clientState;
        }
        set {
            _clientState = value;
        }
    }

    /*
     * Any onsubmit statment to hook up by the form. The HtmlForm object calls this
     * during RenderAttributes.
     */
    internal string ClientOnSubmitEvent {
        get {
            if (ClientScript.HasSubmitStatements ||
                (Form != null && Form.SubmitDisabledControls && (EnabledControls.Count > 0))) {
                // to avoid being affected by earlier instructions we must
                // write out the language as well
                return "javascript:return WebForm_OnSubmit();";
            }
            return string.Empty;
        }
    }


    public ClientScriptManager ClientScript {
        get {
            if (_clientScriptManager == null) {
                _clientScriptManager = new ClientScriptManager(this);
            }

            return _clientScriptManager;
        }
    }


    /// <devdoc>
    ///    <para>Indicates whether the requesting browser is uplevel or downlevel so that the appropriate behavior can be
    ///       generated for the request.</para>
    /// </devdoc>
    [
    DefaultValue(""),
    WebSysDescription(SR.Page_ClientTarget),
    Browsable(false),
    EditorBrowsable(EditorBrowsableState.Advanced),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public string ClientTarget {
        get {
            return (_clientTarget == null) ? String.Empty : _clientTarget;
        }
        set {
            _clientTarget = value;
            if (_request != null) {
                _request.ClientTarget = value;
            }
        }
    }

    private string _clientQueryString = null;
    public String ClientQueryString {
        get {
            if (_clientQueryString == null) {
                if (RequestInternal != null && Request.HasQueryString) {
                    // Eliminate system post fields (generated by the framework) from the
                    // querystring used for adaptive rendering.
                    Hashtable ht = new Hashtable();
                    foreach (string systemPostField in s_systemPostFields) {
                        ht.Add(systemPostField, true);
                    }

                    // 
                    HttpValueCollection httpValueCollection = (HttpValueCollection)((SkipFormActionValidation) ? Request.Unvalidated.QueryString : Request.QueryString);
                    _clientQueryString = httpValueCollection.ToString(urlencoded: true, excludeKeys: ht);
                }
                else {
                    _clientQueryString = String.Empty;
                }
            }

            return _clientQueryString;
        }
    }

    internal bool ContainsEncryptedViewState {
        get {
            return _containsEncryptedViewState;
        }
        set {
            _containsEncryptedViewState = value;
        }
    }

    /// <devdoc>
    ///    <para>
    ///       Gets or sets the error page to which the requesting browser should be
    ///       redirected in the event of an unhandled page exception.
    ///    </para>
    /// </devdoc>
    [
    DefaultValue(""),
    WebSysDescription(SR.Page_ErrorPage),
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public string ErrorPage {
        get {
            return _errorPage;
        }
        set {
            _errorPage = value;
        }
    }

    /// <devdoc>
    ///   Gets a value indicating whether the page is being loaded in response to a client callback.
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public bool IsCallback {
        get {
            return _isCallback;
        }
    }

    /// <internalonly/>
    /// <devdoc>Page class can be cached/reused</devdoc>
    [
    Browsable(false),
    EditorBrowsable(EditorBrowsableState.Never)
    ]
    public bool IsReusable {
        get { return false; }
    }

    /// <devdoc>
    ///     Required for small browsers that cache too aggressively.
    /// </devdoc>
    protected internal virtual String UniqueFilePathSuffix {
        get {
            if (_uniqueFilePathSuffix != null) {
                return _uniqueFilePathSuffix;
            }
            // Only need a few digits, so save space by modulo'ing by a prime.
            // The chosen prime is the highest of six digits.
            long ticks = DateTime.Now.Ticks % 999983;
            _uniqueFilePathSuffix = String.Concat(UniqueFilePathSuffixID + "=", ticks.ToString("D6", CultureInfo.InvariantCulture));
            _uniqueFilePathSuffix = _uniqueFilePathSuffix.PadLeft(6, '0');
            return _uniqueFilePathSuffix;
        }
    }

    // This property should be public. (DevDiv Bugs 161340)
    public Control AutoPostBackControl {
        get {
            return _autoPostBackControl;
        }
        set {
            _autoPostBackControl = value;
        }
    }

    internal bool ClientSupportsFocus {
        get {
            return (_request != null) &&
                ((_request.Browser.EcmaScriptVersion >= FocusMinimumEcmaVersion) || (_request.Browser.JScriptVersion >= FocusMinimumJScriptVersion));
        }
    }

    internal bool ClientSupportsJavaScript {
        get {
            if (!_clientSupportsJavaScriptChecked) {
                _clientSupportsJavaScript = (_request != null) &&
                    (_request.Browser.EcmaScriptVersion >= JavascriptMinimumVersion);
                _clientSupportsJavaScriptChecked = true;
            }

            return _clientSupportsJavaScript;
        }
    }

    private ArrayList EnabledControls {
        get {
            if (_enabledControls == null) {
                _enabledControls = new ArrayList();
            }
            return _enabledControls;
        }
    }

    internal string FocusedControlID {
        get {
            if (_focusedControlID == null) {
                return String.Empty;
            }
            return _focusedControlID;
        }
    }

    /// <devdoc>
    ///    The control that has been set to be focused (empty if there was no such control)
    /// </devdoc>
    internal Control FocusedControl {
        get {
            return _focusedControl;
        }
    }


    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public HtmlHead Header {
        get {
            return _header;
        }
    }

    /// <internalonly/>
    /// <devdoc>
    ///     VSWhidbey 80467: Need to adapt Id separator.
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    EditorBrowsable(EditorBrowsableState.Never)
    ]
    public new virtual char IdSeparator {
        get {
            if (!_haveIdSeparator) {
                if (AdapterInternal != null) {
                    _idSeparator = PageAdapter.IdSeparator;
                }
                else {
                    _idSeparator = IdSeparatorFromConfig;
                }
                _haveIdSeparator = true;
            }

            return _idSeparator;
        }
    }


    /// <devdoc>
    ///    The control that has was last focused (empty if there was no such control)
    /// </devdoc>
    // We
    internal string LastFocusedControl {
        [AspNetHostingPermission(SecurityAction.Assert, Level = AspNetHostingPermissionLevel.Low)]
        get {
            if (RequestInternal != null) {
                // SECURITY: Change this to just check form + query string
                string lastFocus = Request[lastFocusID];
                if (lastFocus != null) {
                    return lastFocus;
                }
            }
            return String.Empty;
        }
    }


    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public bool MaintainScrollPositionOnPostBack {
        get {
            if (RequestInternal != null && RequestInternal.Browser != null && !RequestInternal.Browser.SupportsMaintainScrollPositionOnPostback)
                return false;
            return _maintainScrollPosition;
        }
        set {
            if (_maintainScrollPosition != value) {
                _maintainScrollPosition = value;
                if (_maintainScrollPosition) LoadScrollPosition();
            }
        }
    }


    /// <devdoc>
    ///    <para>The MasterPage used by the Page.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    WebSysDescription(SR.MasterPage_MasterPage)
    ]
    public MasterPage Master {
        get {
            if (_master == null && !_preInitWorkComplete) {
                _master = MasterPage.CreateMaster(this, Context, _masterPageFile, _contentTemplateCollection);
            }

            return _master;
        }
    }


    /// <devdoc>
    ///    <para>Gets and sets the masterPageFile of this Page.</para>
    /// </devdoc>
    [
    DefaultValue(""),
    WebCategory("Behavior"),
    WebSysDescription(SR.MasterPage_MasterPageFile)
    ]
    public virtual string MasterPageFile {
        get {
            return VirtualPath.GetVirtualPathString(_masterPageFile);
        }
        set {
            if (_preInitWorkComplete) {
                throw new InvalidOperationException(SR.GetString(SR.PropertySetBeforePageEvent, "MasterPageFile", "Page_PreInit"));
            }

            if (value != VirtualPath.GetVirtualPathString(_masterPageFile)) {
                _masterPageFile = VirtualPath.CreateAllowNull(value);

                if (_master != null && Controls.Contains(_master)) {
                    Controls.Remove(_master);
                }
                _master = null;
            }
        }
    }

    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    EditorBrowsable(EditorBrowsableState.Never)
    ]
    public int MaxPageStateFieldLength {
        get {
            return _maxPageStateFieldLength;
        }
        set {
            if (this.ControlState > ControlState.FrameworkInitialized) {
                throw new InvalidOperationException(SR.GetString(SR.PropertySetAfterFrameworkInitialize, "MaxPageStateFieldLength"));
            }

            if (value == 0 || value < -1) {
                throw new ArgumentException(SR.GetString(SR.Page_Illegal_MaxPageStateFieldLength), "MaxPageStateFieldLength");
            }
            _maxPageStateFieldLength = value;
        }
    }

    /// <devdoc>
    ///    Indicates whether page requires cross post script
    /// </devdoc>
    internal bool ContainsCrossPagePost {
        get {
            return _containsCrossPagePost;
        }
        set {
            _containsCrossPagePost = value;
        }
    }

    /// <devdoc>
    /// True if the form should render a reference to the focus script.
    /// </devdoc>
    internal bool RenderFocusScript {
        get {
            return _requireFocusScript;
        }
    }

    internal Stack PartialCachingControlStack {
        get {
            return _partialCachingControlStack;
        }
    }

    /// <devdoc>
    ///    Returns the page state persister associated with the page.
    /// </devdoc>
    protected virtual PageStatePersister PageStatePersister {
        get {
            if (_persister == null) {
                PageAdapter adapter = PageAdapter;
                if (adapter != null) {
                    _persister = adapter.GetStatePersister();
                }
                if (_persister == null) {
                    _persister = new HiddenFieldPageStatePersister(this);
                }
            }
            return _persister;
        }
    }

    // Reconstructs the view state string from the view state fields in the request
    internal string RequestViewStateString {
        get {
            if (!_cachedRequestViewState) {
                StringBuilder state = new StringBuilder();
                try {
                    NameValueCollection requestValueCollection = RequestValueCollection;
                    if (requestValueCollection != null) {
                        // If ViewStateChunking is disabled(-1) or there is no ViewStateFieldCount, return the __VIEWSTATE field
                        string fieldCountStr = RequestValueCollection[ViewStateFieldCountID];
                        if (MaxPageStateFieldLength == -1 || fieldCountStr == null) {
                            _cachedRequestViewState = true;
                            _requestViewState = RequestValueCollection[ViewStateFieldPrefixID];
                            return _requestViewState;
                        }

                        // Build up the entire persisted state from all the viewstate fields
                        int numViewStateFields = Convert.ToInt32(fieldCountStr, CultureInfo.InvariantCulture);
                        if (numViewStateFields < 0) {
                            throw new HttpException(SR.GetString(SR.ViewState_InvalidViewState));
                        }

                        // The view state is split into __VIEWSTATE, __VIEWSTATE1, __VIEWSTATE2, ... fields
                        for (int i=0; i<numViewStateFields; ++i) {
                            string key = ViewStateFieldPrefixID;

                            // For backwards compat we always need the first chunk to be __VIEWSTATE
                            if (i > 0) key += i.ToString(CultureInfo.InvariantCulture);
                            string viewStateChunk = RequestValueCollection[key];
                            if (viewStateChunk == null) {
                                throw new HttpException(SR.GetString(SR.ViewState_MissingViewStateField, key));
                            }

                            state.Append(viewStateChunk);
                        }
                    }

                    _cachedRequestViewState = true;
                    _requestViewState = state.ToString();
                } catch (Exception e) {
                    ViewStateException.ThrowViewStateError(e, state.ToString());
                }
            }
            return _requestViewState;
        }
    }

    internal string ValidatorInvalidControl {
        get {
            if (_validatorInvalidControl == null) {
                return String.Empty;
            }
            return _validatorInvalidControl;
        }
    }


    /// <devdoc>
    /// <para>Gets the <see cref='System.Web.TraceContext'/> object for the current Web
    ///    request. Tracing tracks and presents the execution details about a Web request. </para>
    /// For trace data to be visible in a rendered page, you must
    /// turn tracing on for that page.
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public TraceContext Trace {
        get {
            return Context.Trace;
        }
    }


    /// <devdoc>
    /// <para>Gets the <see langword='Request'/> object provided by the HTTP Runtime, which
    ///    allows you to access data from incoming HTTP requests.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public HttpRequest Request {
        [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        get {
            if (_request == null)
                throw new HttpException(SR.GetString(SR.Request_not_available));

            return _request;
        }
    }

    internal HttpRequest RequestInternal {
        get {
            return _request;
        }
    }

    /// <devdoc>
    /// <para>Gets the <see langword='Response '/>object provided by the HTTP Runtime, which
    ///    allows you to send HTTP response data to a client browser.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public HttpResponse Response {
        get {
            if (_response == null)
                throw new HttpException(SR.GetString(SR.Response_not_available));

            return _response;
        }
    }

    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public RouteData RouteData {
        get {
            if (Context != null && Context.Request != null) {
                // RequestContext is created on demand if not set so it should never be null
                return Context.Request.RequestContext.RouteData;
            }
            return null;
        }
    }

    /// <devdoc>
    /// <para>Gets the <see langword='Server'/> object supplied by the HTTP runtime.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public HttpServerUtility Server {
        get { return Context.Server;}
    }


    /// <devdoc>
    /// <para>Retrieves a <see langword='Cache'/> object in which to store the page for
    ///    subsequent requests. This property is read-only.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public Cache Cache {
        get {
            if (_cache == null)
                throw new HttpException(SR.GetString(SR.Cache_not_available));

            return _cache;
        }
    }

    /// <devdoc>
    /// <para>Gets the <see langword='Session'/>
    /// object provided by the HTTP Runtime. This object provides information about the current request's session.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public virtual HttpSessionState Session {
        get {
            if (!_sessionRetrieved) {
                /* try just once to retrieve it */
                _sessionRetrieved = true;

                try {
                    _session = Context.Session;
                }
                catch {
                    //  Just ignore exceptions, return null.
                }
            }

            if (_session == null) {
                throw new HttpException(SR.GetString(SR.Session_not_enabled));
            }

            return _session;
        }
    }

    [
    Bindable(true),
    Localizable(true),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public string Title {
        get {
            if ((Page.Header == null) && (this.ControlState >= ControlState.ChildrenInitialized)) {
                throw new InvalidOperationException(SR.GetString(SR.Page_Title_Requires_Head));
            }

            if (_titleToBeSet != null) {
                return _titleToBeSet;
            }

            return Page.Header.Title;
        }
        set {
            if (Page.Header == null) {
                if (this.ControlState >= ControlState.ChildrenInitialized) {
                    throw new InvalidOperationException(SR.GetString(SR.Page_Title_Requires_Head));
                }
                else {
                    _titleToBeSet = value;
                }
            }
            else {
                Page.Header.Title = value;
            }
        }
    }

    [
    Bindable(true),
    Localizable(true),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public string MetaDescription {
        get {
            if ((Page.Header == null) && (this.ControlState >= ControlState.ChildrenInitialized)) {
                throw new InvalidOperationException(SR.GetString(SR.Page_Description_Requires_Head));
            }

            if (_descriptionToBeSet != null) {
                return _descriptionToBeSet;
            }

            return Page.Header.Description;
        }
        set {
            if (Page.Header == null) {
                if (this.ControlState >= ControlState.ChildrenInitialized) {
                    throw new InvalidOperationException(SR.GetString(SR.Page_Description_Requires_Head));
                }
                else {
                    _descriptionToBeSet = value;
                }
            }
            else {
                Page.Header.Description = value;
            }
        }
    }

    [
    Bindable(true),
    Localizable(true),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public string MetaKeywords {
        get {
            if ((Page.Header == null) && (this.ControlState >= ControlState.ChildrenInitialized)) {
                throw new InvalidOperationException(SR.GetString(SR.Page_Keywords_Requires_Head));
            }

            if (_keywordsToBeSet != null) {
                return _keywordsToBeSet;
            }

            return Page.Header.Keywords;
        }
        set {
            if (Page.Header == null) {
                if (this.ControlState >= ControlState.ChildrenInitialized) {
                    throw new InvalidOperationException(SR.GetString(SR.Page_Keywords_Requires_Head));
                }
                else {
                    _keywordsToBeSet = value;
                }
            }
            else {
                Page.Header.Keywords = value;
            }
        }
    }

    /// <devdoc>
    /// indicates whether the Page has PageTheme defined.
    /// </devdoc>
    internal bool ContainsTheme {
        get {
            Debug.Assert(_preInitWorkComplete || DesignMode, "ContainsTheme should not be accessed before Page's PreInit.");
            return _theme != null;
        }
    }


    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public virtual String Theme {
        get {
            return _themeName;
        }
        set {
            if (_preInitWorkComplete) {
                throw new InvalidOperationException(SR.GetString(SR.PropertySetBeforePageEvent, "Theme", "Page_PreInit"));
            }

            if (!String.IsNullOrEmpty(value) && !FileUtil.IsValidDirectoryName(value)) {
                throw new ArgumentException(SR.GetString(SR.Page_theme_invalid_name, value), "Theme");
            }

            _themeName = value;
        }
    }

    internal bool SupportsStyleSheets {
        get {
            if (_supportsStyleSheets == -1) {
                if (Header != null &&
                    Header.StyleSheet != null &&
                    RequestInternal != null &&
                    Request.Browser != null &&
                    (string)Request.Browser["preferredRenderingType"] != "xhtml-mp" &&
                    Request.Browser.SupportsCss &&
                    !Page.IsCallback &&
                    (ScriptManager == null || !ScriptManager.IsInAsyncPostBack)) {

                    // We don't want to render the style sheet for XHTML mobile profile devices even though
                    // SupportsCss may be true because they need the CSS to be in a separate file.

                    // We don't want embedded styles sheet to render during a callback (VSWhidbey 420743)
                    _supportsStyleSheets = 1;
                    return true;
                }

                _supportsStyleSheets = 0;
                return false;
            }
            return (_supportsStyleSheets == 1);
        }
    }

    [
    Browsable(false),
    Filterable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public virtual String StyleSheetTheme {
        get {
            return _styleSheetName;
        }
        set {
            if (_pageFlags[styleSheetInitialized]) {
                throw new InvalidOperationException(SR.GetString(SR.SetStyleSheetThemeCannotBeSet));
            }
            _styleSheetName = value;
        }
    }

    /// <devdoc>
    ///    <para>Indicates the user making the page request. This property uses the
    ///       Context.User property to determine where the request originates. This property
    ///       is read-only.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public IPrincipal User {
        get { return Context.User;}
    }

    internal XhtmlConformanceMode XhtmlConformanceMode {
        get {
            // We only want the evaluation of conformance mode to be done at most once per page request.
            if (!_xhtmlConformanceModeSet) {
                // The conformance mode is used to determine if backward compatible markup should
                // be generated as in pre-Whidbey versions.  So if an adapter is assigned, we can
                // assume this is Whidbey rendering and we return the default mode that doesn't do
                // backward compatible rendering.
                if (DesignMode) {
                    _xhtmlConformanceMode = XhtmlConformanceSection.DefaultMode;
                }
                else {
                    _xhtmlConformanceMode = GetXhtmlConformanceSection().Mode;
                }
                _xhtmlConformanceModeSet = true;
            }

            return _xhtmlConformanceMode;
        }
    }

    /*
     * This protected virtual method is called by the Page to create the HtmlTextWriter
     * to use for rendering. The class created is based on the TagWriter property on
     * the browser capabilities.
     */

    /// <devdoc>
    /// <para>Creates an <see cref='System.Web.UI.HtmlTextWriter'/> object to render the page's
    ///    content. If the <see langword='IsUplevel'/> property is set to
    /// <see langword='false'/>, an <see langword='Html32TextWriter'/> object is created
    ///    to render requests originating from downlevel browsers. For derived pages, you
    ///    can override this method to create a custom text writer.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected internal virtual HtmlTextWriter CreateHtmlTextWriter(TextWriter tw) {
        // Use Context.Request (rather than Request) to avoid exception in get_Request when
        // Request is not available.
        if (Context != null && Context.Request != null && Context.Request.Browser != null) {
            return Context.Request.Browser.CreateHtmlTextWriter(tw);
        }

        HtmlTextWriter writer = CreateHtmlTextWriterInternal(tw, _request );
        if (writer == null) {
            writer = new HtmlTextWriter(tw);
        }
        return writer;
    }

    internal static HtmlTextWriter CreateHtmlTextWriterInternal(TextWriter tw, HttpRequest request) {

        if (request != null && request.Browser != null) {
            return request.Browser.CreateHtmlTextWriterInternal(tw);
        }

        // Fall back to Html 3.2
        return new Html32TextWriter(tw);
    }

    public static HtmlTextWriter CreateHtmlTextWriterFromType(TextWriter tw, Type writerType) {
        if (writerType == typeof(HtmlTextWriter)) {
            return new HtmlTextWriter(tw);
        }
        else if (writerType == typeof(Html32TextWriter)) {
            return new Html32TextWriter(tw);
        }
        else {
            try {
                // Make sure the type has the correct base class (ASURT 123677)
                Util.CheckAssignableType(typeof(HtmlTextWriter), writerType);

                return (HtmlTextWriter)HttpRuntime.CreateNonPublicInstance(writerType, new object[] { tw });
            }
            catch {
                throw new HttpException(SR.GetString(SR.Invalid_HtmlTextWriter, writerType.FullName));
            }
        }
    }

    /// <devdoc>
    /// Overridden to check the Page's own ID against the one being searched.
    /// </devdoc>
    public override Control FindControl(String id) {
        if (StringUtil.EqualsIgnoreCase(id, PageID)) {
            return this;
        }
        return base.FindControl(id, 0);
    }
    /*
     * This method is implemented by the Page classes that we generate on
     * the fly.  It returns a has code unique to the control layout.
     */

    /// <devdoc>
    /// <para>Retrieves a hash code that is generated by <see langword='Page'/> objects that
    ///    are generated at runtime. This hash code is unique to the page's control
    ///    layout.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public virtual int GetTypeHashCode() {
        return 0;
    }

    /*
     * Override for small efficiency win: page doesn't prepend its name
     */
    internal override string GetUniqueIDPrefix() {
        // Only overridde if we're at the top level
        if (Parent == null)
            return String.Empty;

        // Use base implementation for interior nodes
        return base.GetUniqueIDPrefix();
    }

    // This is a non-cryptographic hash code that can be used to identify which Page generated
    // a __VIEWSTATE field. It shouldn't be considered sensitive information since its inputs
    // are assumed to be known by all parties.
    internal uint GetClientStateIdentifier() {
        // 

        // Use the page's directory and class name as part of the key (ASURT 64044)
        // We need to make sure that the hash is case insensitive, since the file system
        // is, and strange view state errors could otherwise happen (ASURT 128657)
        int pageHashCode = StringComparer.InvariantCultureIgnoreCase.GetHashCode(
            TemplateSourceDirectory);
        pageHashCode += StringComparer.InvariantCultureIgnoreCase.GetHashCode(GetType().Name);
        return (uint)pageHashCode;
    }

    /*
     * Called when an exception occurs in ProcessRequest
     */

    /// <devdoc>
    /// <para>Throws an <see cref='System.Web.HttpException'/> object when an error occurs during a call to the
    /// <see cref='System.Web.UI.Page.ProcessRequest'/> method. If there is a custom error page, and
    ///    custom error page handling is enabled, the method redirects to the specified
    ///    custom error page.</para>
    /// </devdoc>
    private bool HandleError(Exception e) {

        try {
            // Remember the exception to be accessed via Server.GetLastError/ClearError
            Context.TempError = e;
            // Raise the error event
            OnError(EventArgs.Empty);
            // If the error has been cleared by the event handler, nothing else to do
            if (Context.TempError == null)
                return true;
        } finally {
            Context.TempError = null;
        }

        // If an error page was specified, redirect to it
        if (!String.IsNullOrEmpty(_errorPage)) {
            // only redirect if custom errors are enabled:
            if (Context.IsCustomErrorEnabled) {
                _response.RedirectToErrorPage(_errorPage, CustomErrorsSection.GetSettings(Context).RedirectMode);
                return true;
            }
        }

        // Increment all of the appropriate error counters
        PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_UNHANDLED);

        string traceString = null;
        if (Context.TraceIsEnabled) {
            Trace.Warn(SR.GetString(SR.Unhandled_Err_Error), null, e);
            if (Trace.PageOutput) {
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                // Try to build the profile tree so the control hierarchy will show up
                BuildPageProfileTree(false);

                // these three calls will happen again at the end of the request, but
                // in order to have the full trace log on the rendered page, we need
                // to call them now.
                Trace.EndRequest();
                Trace.StopTracing();
                Trace.StatusCode = 500;
                Trace.Render(htw);
                traceString = sw.ToString();
            }
        }

        // If the exception is an HttpException with a formatter, just
        // rethrow it instead of a new one (ASURT 45479)
        if (HttpException.GetErrorFormatter(e) != null) {
            return false;
        }

        // Don't touch security exceptions (ASURT 78366)
        if (e is System.Security.SecurityException)
            return false;

        throw new HttpUnhandledException(null, traceString, e);
    }


    /// <devdoc>
    ///    <para>Gets a value indicating whether the page is being created in response to a
    ///       cross page postback.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public bool IsCrossPagePostBack {
        get {
            return _isCrossPagePostBack;
        }
    }

    internal bool IsExportingWebPart {
        get {
            return _pageFlags[isExportingWebPart];
        }
    }

    internal bool IsExportingWebPartShared {
        get {
            return _pageFlags[isExportingWebPartShared];
        }
    }

    /*
     * Returns true if this is a postback, which means it has some
     * previous viewstate to reload. Use this in the Load method to differentiate
     * an initial load from a postback reload.
     */

    /// <devdoc>
    ///    <para>Gets a value indicating whether the page is being loaded in response to a
    ///       client postback, or if it is being loaded and accessed for the first time.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public bool IsPostBack {
        get {
            if (_requestValueCollection == null)
                return false;

            // Treat it as postback if the page is created thru cross page postback.
            if (_isCrossPagePostBack)
                return true;

            // Don't treat it as a postback if the page is posted from cross page
            if (_pageFlags[isCrossPagePostRequest])
                return false;

            // Don't treat it as a postback if a view state MAC check failed and we
            // simply ate the exception.
            if (ViewStateMacValidationErrorWasSuppressed)
                return false;

            // If we're in a Transfer/Execute, never treat as postback (ASURT 121000)
            // Unless we are being transfered back to the original page, in which case
            // it is ok to treat it as a postback (VSWhidbey 117747)
            // Note that Context.Handler could be null (VSWhidbey 159775)
            if (Context.ServerExecuteDepth > 0 &&
                (Context.Handler == null || GetType() != Context.Handler.GetType())) {
                return false;
            }

            // If the page control layout has changed, pretend that we are in
            // a non-postback situation.
            return !_fPageLayoutChanged;
        }
    }

    internal NameValueCollection RequestValueCollection {
        get { return _requestValueCollection; }
    }

    [
    Browsable(false),
    DefaultValue(true),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    EditorBrowsable(EditorBrowsableState.Never),
    ]
    public virtual bool EnableEventValidation {
        get {
            return _enableEventValidation;
        }
        set {
            if (this.ControlState > ControlState.FrameworkInitialized) {
                throw new InvalidOperationException(SR.GetString(SR.PropertySetAfterFrameworkInitialize, "EnableEventValidation"));
            }

            _enableEventValidation = value;
        }
    }

    [
    Browsable(false)
    ]
    public override bool EnableViewState {
        get {
            return base.EnableViewState;
        }
        set {
            base.EnableViewState = value;
        }
    }

    [
    Browsable(false),
    DefaultValue(ViewStateEncryptionMode.Auto),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    EditorBrowsable(EditorBrowsableState.Never),
    ]
    public ViewStateEncryptionMode ViewStateEncryptionMode {
        get {
            return _encryptionMode;
        }
        set {
            if (this.ControlState > ControlState.FrameworkInitialized) {
                throw new InvalidOperationException(SR.GetString(SR.PropertySetAfterFrameworkInitialize, "ViewStateEncryptionMode"));
            }

            if (value < ViewStateEncryptionMode.Auto || value > ViewStateEncryptionMode.Never) {
                throw new ArgumentOutOfRangeException("value");
            }

            _encryptionMode = value;
        }
    }

    /// <devdoc>
    ///    <para>Setting this property helps prevent one-click attacks (ASURT 126375)</para>
    /// </devdoc>
    [
    Browsable(false)
    ]
    public string ViewStateUserKey {
        get {
            return _viewStateUserKey;
        }
        set {
            // Make sure it's not called too late
            if (ControlState >= ControlState.Initialized) {
                throw new HttpException(SR.GetString(SR.Too_late_for_ViewStateUserKey));
            }

            _viewStateUserKey = value;
        }
    }


    [
    Browsable(false),
    EditorBrowsable(EditorBrowsableState.Never)
    ]
    public override string ID {
        get {
            return base.ID;
        }
        set {
            base.ID = value;
        }
    }


    [
    Browsable(false),
    EditorBrowsable(EditorBrowsableState.Never),
    DefaultValue(ValidateRequestMode.Enabled)
    ]
    public override ValidateRequestMode ValidateRequestMode {
        get {
            return base.ValidateRequestMode;
        }
        set {
            base.ValidateRequestMode = value;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [DefaultValue(false)]
    // If this property is false, then
    // - we eagerly validate RawUrl at the appropriate point in ProcessRequestMain / ProcessRequestTransacted, and
    // - the ClientQueryString property is populated from Request.QueryString (and might be validated) instead of Request.Unvalidated.QueryString.
    public bool SkipFormActionValidation {
        get {
            return _pageFlags[skipFormActionValidation];
        }
        set {
            // Clear the cached ClientQueryString value if the value of this property changes
            if (value != SkipFormActionValidation) {
                _clientQueryString = null;
            }

            _pageFlags[skipFormActionValidation] = value;
        }
    }

    [
    Browsable(false)
    ]
    public override bool Visible {
        [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        get {
            return base.Visible;
        }
        set {
            base.Visible = value;
        }
    }

    /// <devdoc>
    ///    <para>Decrypt the string using symmetric algorithm defined in config.</para>
    /// </devdoc>
    internal static string DecryptString(string s, Purpose purpose) {
        if (s == null)
            return null;

        byte[] protectedData = HttpServerUtility.UrlTokenDecode(s);

        // DevDiv Bugs 137864: IVType.Hash is necessary for WebResource / ScriptResource URLs
        // so that client and server caching will continue to work. MS AJAX also caches these
        // client-side, and switching to a different IV type could potentially break MS AJAX
        // due to loading the same Javascript resource multiple times.
        // MSRC 10405: Crypto board approves of this usage of IVType.Hash.

        byte[] clearData = null;
        if (protectedData != null) {
            if (AspNetCryptoServiceProvider.Instance.IsDefaultProvider) {
                // ASP.NET 4.5 Crypto DCR: Go through the new AspNetCryptoServiceProvider
                // if we're configured to do so.
                ICryptoService cryptoService = AspNetCryptoServiceProvider.Instance.GetCryptoService(purpose, CryptoServiceOptions.CacheableOutput);
                clearData = cryptoService.Unprotect(protectedData);
            }
            else {
                // If we're not configured to go through the new crypto routines,
                // fall back to the standard MachineKey crypto routines.
#pragma warning disable 618 // calling obsolete methods
                clearData = MachineKeySection.EncryptOrDecryptData(fEncrypt: false, buf: protectedData, modifier: null, start: 0, length: protectedData.Length, useValidationSymAlgo: false, useLegacyMode: false, ivType: IVType.Hash);
#pragma warning restore 618 // calling obsolete methods
            }
        }

        if (clearData == null)
            throw new HttpException(SR.GetString(SR.ViewState_InvalidViewState));
        return Encoding.UTF8.GetString(clearData);
    }

    /*
     * Performs intialization of the page required by the designer.
     */

    /// <devdoc>
    ///    <para>Performs any initialization of the page that is required by RAD designers.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void DesignerInitialize() {
        InitRecursive(null);
    }

    internal NameValueCollection GetCollectionBasedOnMethod(bool dontReturnNull) {
        // Get the right NameValueCollection base on the method
        if (_request.HttpVerb == HttpVerb.POST) {
            return (dontReturnNull || _request.HasForm) ? _request.Form : null;
        }
        else {
            return (dontReturnNull || _request.HasQueryString) ? _request.QueryString : null;
        }
    }

    private bool DetermineIsExportingWebPart() {
        byte[] queryString = Request.QueryStringBytes;
        if ((queryString == null) || (queryString.Length < 28)) {
            return false;
        }
        // query string is never unicode - it can be UTF-8, in which case it's fine to compare character by character
        // because what we're looking for is only in the low-ASCII range.
        if ((queryString[0] != '_') ||
            (queryString[1] != '_') ||
            (queryString[2] != 'W') ||
            (queryString[3] != 'E') ||
            (queryString[4] != 'B') ||
            (queryString[5] != 'P') ||
            (queryString[6] != 'A') ||
            (queryString[7] != 'R') ||
            (queryString[8] != 'T') ||
            (queryString[9] != 'E') ||
            (queryString[10] != 'X') ||
            (queryString[11] != 'P') ||
            (queryString[12] != 'O') ||
            (queryString[13] != 'R') ||
            (queryString[14] != 'T') ||
            (queryString[15] != '=') ||
            (queryString[16] != 't') ||
            (queryString[17] != 'r') ||
            (queryString[18] != 'u') ||
            (queryString[19] != 'e') ||
            (queryString[20] != '&')) {

            return false;
        }
        // Setting the export flag so that personalization can know not to toggle modes,
        // which would create a new subrequest and kill the export.
        _pageFlags.Set(isExportingWebPart);
        return true;
    }

    /*
     * Determine which of the following three cases we're in:
     * - Initial request.  No postback, return null
     * - GET postback request.  Return Context.Request.QueryString
     * - POST postback request.  Return Context.Request.Form
     */

    /// <devdoc>
    ///    <para>Determines the type of request made for the page based on if the page was a
    ///       postback, and whether a GET or POST method was used for the request.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected internal virtual NameValueCollection DeterminePostBackMode() {
        if (Context.Request == null)
            return null;

        // If PreventPostback is set, don't treat as postback (VSWhidbey 181013).
        if (Context.PreventPostback)
            return null;

        NameValueCollection ret = GetCollectionBasedOnMethod(dontReturnNull: false);

        if (ret == null)
            return null;

        // Some devices may send incorrect POST strings without trailing equal signs
        // if the last field is empty. Detecting this:
        bool isPostback = false;
        String [] nullValues = ret.GetValues(null);
        if (nullValues != null) {
            int numNull = nullValues.Length;
            for (int i = 0; i < numNull; i++) {
                if (nullValues[i].StartsWith(ViewStateFieldPrefixID, StringComparison.Ordinal) || nullValues[i] == postEventSourceID) {
                    isPostback = true;
                    break;
                }
            }
        }

        // If there is no state or postEventSourceID in the request,
        // it's an initial request
        // 



        if (ret[ViewStateFieldPrefixID] == null &&
            ret[ViewStateFieldCountID] == null &&
            ret[postEventSourceID] == null &&
            !isPostback)
            ret = null;

        // If page was posted due to a HttpResponse.Redirect, ignore the postback.
        else if (Request.QueryStringText.IndexOf(HttpResponse.RedirectQueryStringAssignment, StringComparison.Ordinal) != -1)
            ret = null;

        return ret;
    }

    /// <summary>
    /// Returns an unvalidated name/value collection of the postback variables. This method will
    /// only be called if DeterminePostBackMode() returns a non-null value.
    /// This method exists to support the granular request validation feature added in .NET 4.5
    /// </summary>
    /// <returns>An unvalidated name/value collection of the postback variables.</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected internal virtual NameValueCollection DeterminePostBackModeUnvalidated() {
        // Get the right NameValueCollection base on the method. This is modeled on GetCollectionBasedOnMethod()
        return _request.HttpVerb == HttpVerb.POST ? _request.Unvalidated.Form : _request.Unvalidated.QueryString;
    }

    /// <devdoc>
    /// <para>This method is used to encrypt previous page hidden form variable that is sent to the client
    /// during cross page post. This is to prevent spoofed previous pages from being instantiated and executed.</para>
    /// This is also used by the AssemblyResourceLoader to prevent tampering of URLs.
    /// </devdoc>
    internal static string EncryptString(string s, Purpose purpose) {
        Debug.Assert(s != null);

        // DevDiv Bugs 137864: IVType.Hash is necessary for WebResource / ScriptResource URLs
        // so that client and server caching will continue to work. MS AJAX also caches these
        // client-side, and switching to a different IV type could potentially break MS AJAX
        // due to loading the same Javascript resource multiple times.
        // MSRC 10405: Crypto board approves of this usage of IVType.Hash.

        byte[] clearData = Encoding.UTF8.GetBytes(s);
        byte[] protectedData;
        if (AspNetCryptoServiceProvider.Instance.IsDefaultProvider) {
            // ASP.NET 4.5 Crypto DCR: Go through the new AspNetCryptoServiceProvider
            // if we're configured to do so.
            ICryptoService cryptoService = AspNetCryptoServiceProvider.Instance.GetCryptoService(purpose, CryptoServiceOptions.CacheableOutput);
            protectedData = cryptoService.Protect(clearData);
        }
        else {
            // If we're not configured to go through the new crypto routines,
            // fall back to the standard MachineKey crypto routines.
#pragma warning disable 618 // calling obsolete methods
            protectedData = MachineKeySection.EncryptOrDecryptData(fEncrypt: true, buf: clearData, modifier: null, start: 0, length: clearData.Length, useValidationSymAlgo: false, useLegacyMode: false, ivType: IVType.Hash);
#pragma warning restore 618 // calling obsolete methods
        }

        return HttpServerUtility.UrlTokenEncode(protectedData);
    }

    private void LoadAllState() {

        object state = LoadPageStateFromPersistenceMedium();
        IDictionary controlStates = null;
        Pair allSavedViewState = null;
        Pair statePair = state as Pair;
        if (state != null) {
            controlStates = statePair.First as IDictionary;
            allSavedViewState = statePair.Second as Pair;
        }

        // The control state (controlStatePair) was saved as an dictionary of objects:
        // 1. A list of controls that require postback[under the page id]
        // 2. A dictionary of control states

        if (controlStates != null) {
            _controlsRequiringPostBack = (ArrayList)controlStates[PageRegisteredControlsThatRequirePostBackKey];

            if (_registeredControlsRequiringControlState != null) {
                foreach (Control ctl in _registeredControlsRequiringControlState) {
                    ctl.LoadControlStateInternal(controlStates[ctl.UniqueID]);
                }
            }
        }

        // The view state (allSavedViewState) was saved as an array of objects:
        // 1. The hash code string
        // 2. The state of the entire control hierarchy

        // Is there any state?
        if (allSavedViewState != null) {
            // Get the hash code from the state
            string hashCode = (string) allSavedViewState.First;

            // If it's different from the current one, the layout has changed
            int viewhash = Int32.Parse(hashCode, NumberFormatInfo.InvariantInfo);
            _fPageLayoutChanged = viewhash != GetTypeHashCode();

            // If the page control layout has changed, don't attempt to
            // load any more state.
            if (!_fPageLayoutChanged) {
                // UNCOMMENT FOR DEBUG OUTPUT
                // WalkViewState(allSavedViewState.Second, null, 0);
                LoadViewStateRecursive(allSavedViewState.Second);

            }
        }
    }

    /*
     * Override this method to persist view state to something other
     * than hidden fields (

*/

    /// <devdoc>
    ///    <para>Loads any saved view state information to the page. Override this method if
    ///       you want to load the page view state in anything other than a hidden field.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected internal virtual object LoadPageStateFromPersistenceMedium() {
        PageStatePersister persister = PageStatePersister;
        try {
            persister.Load();
        }
        catch (HttpException e) {
            //VSWhidbey 201601. Ignore the exception in cross-page post
            //since this might be a cross application postback.

            if (_pageFlags[isCrossPagePostRequest]) {
                return null;
            }

            // DevDiv #461378: Ignore validation errors for cross-page postbacks.
            if (ShouldSuppressMacValidationException(e)) {
                if (Context != null && Context.TraceIsEnabled) {
                    Trace.Write("aspx.page", "Ignoring page state", e);
                }

                ViewStateMacValidationErrorWasSuppressed = true;
                return null;
            }

            e.WebEventCode = WebEventCodes.RuntimeErrorViewStateFailure;
            throw;
        }
        return new Pair(persister.ControlState, persister.ViewState);
    }

    private bool ViewStateMacValidationErrorWasSuppressed {
        get { return _pageFlags[wasViewStateMacErrorSuppressed]; }
        set { _pageFlags[wasViewStateMacErrorSuppressed] = value; }
    }

    internal bool ShouldSuppressMacValidationException(Exception e) {
        // If the patch isn't active, don't suppress anything, as it would be a change in behavior.
        if (!EnableViewStateMacRegistryHelper.SuppressMacValidationErrorsFromCrossPagePostbacks) {
            return false;
        }

        // We check the __VIEWSTATEGENERATOR field for an identifier that matches the current Page.
        // If the generator field exists and says that the current Page generated the incoming
        // __VIEWSTATE field, then a validation failure represents a real error and we need to
        // surface this information to the developer for resolution. Otherwise we assume this
        // view state was not meant for us, so if validation fails we'll just ignore __VIEWSTATE.
        if (ViewStateException.IsMacValidationException(e)) {
            if (EnableViewStateMacRegistryHelper.SuppressMacValidationErrorsAlways) {
                return true;
            }

            // DevDiv #841854: VSUK is often used for CSRF checks, so we can't ---- MAC exceptions by default in this case.
            if (!String.IsNullOrEmpty(ViewStateUserKey)) {
                return false;
            }

            if (_requestValueCollection == null) {
                return true;
            }

            if (!VerifyClientStateIdentifier(_requestValueCollection[ViewStateGeneratorFieldID])) {
                return true;
            }
        }

        return false;
    }

    private bool VerifyClientStateIdentifier(string identifier) {
        // Returns true iff we can parse the incoming identifier and it matches our own.
        // If we can't parse the identifier, then by definition we didn't generate it.
        uint parsedIdentifier;
        return identifier != null
            && UInt32.TryParse(identifier, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parsedIdentifier)
            && parsedIdentifier == GetClientStateIdentifier();
    }

    internal void LoadScrollPosition() {
        // Don't load scroll position if the previous page was a crosspage postback
        if (_previousPagePath != null) {
            return;
        }
        // Load the scroll positions from the request if they exist
        if (_requestValueCollection != null) {
            string xpos = _requestValueCollection[_scrollPositionXID];
            if (xpos != null) {
                if (!Int32.TryParse(xpos, out _scrollPositionX)) {
                    _scrollPositionX = 0;
                }
            }
            string ypos = _requestValueCollection[_scrollPositionYID];
            if (ypos != null) {
                if (!Int32.TryParse(ypos, out _scrollPositionY)) {
                    _scrollPositionY = 0;
                }
            }
        }
    }

    internal IStateFormatter2 CreateStateFormatter() {
        return new ObjectStateFormatter(this, true);
    }

    // Decomposes the large view state string into pieces of size <= MaxPageStateFieldLength
    internal ICollection DecomposeViewStateIntoChunks() {
        string state = ClientState;
        if (state == null) return null;

        // Any value less than or equal to 0 turns off chunking
        if (MaxPageStateFieldLength <= 0) {
            ArrayList chunks = new ArrayList(1);
            chunks.Add(state);
            return chunks;
        }

        // Break up the view state into the correctly sized chunks
        int numFullChunks = ClientState.Length / MaxPageStateFieldLength;
        ArrayList viewStateChunks = new ArrayList(numFullChunks+1);
        int curPos = 0;
        for (int i=0; i<numFullChunks; i++) {
            viewStateChunks.Add(state.Substring(curPos, MaxPageStateFieldLength));
            curPos += MaxPageStateFieldLength;
        }
        // Add the leftover characters
        if (curPos < state.Length) {
            viewStateChunks.Add(state.Substring(curPos));
        }

        // Always want to return at least one empty chunk
        if (viewStateChunks.Count == 0) {
            viewStateChunks.Add(String.Empty);
        }
        return viewStateChunks;
    }

    internal void RenderViewStateFields(HtmlTextWriter writer) {
        if (_hiddenFieldsToRender == null) {
            _hiddenFieldsToRender = new Dictionary<string, string>();
        }
        if (ClientState != null) {
            ICollection viewStateChunks = DecomposeViewStateIntoChunks();

            writer.WriteLine();

            // Don't write out a view state field count if there is only 1 viewstate field
            if (viewStateChunks.Count > 1) {
                string value = viewStateChunks.Count.ToString(CultureInfo.InvariantCulture);
                writer.Write("<input type=\"hidden\" name=\"");
                writer.Write(ViewStateFieldCountID);
                writer.Write("\" id=\"");
                writer.Write(ViewStateFieldCountID);
                writer.Write("\" value=\"");
                writer.Write(value);
                writer.WriteLine("\" />");
                _hiddenFieldsToRender[ViewStateFieldCountID] = value;
            }

            int count = 0;
            foreach (string stateChunk in viewStateChunks) {
                writer.Write("<input type=\"hidden\" name=\"");
                string name = ViewStateFieldPrefixID;
                writer.Write(ViewStateFieldPrefixID);
                string countString = null;
                if (count > 0) {
                    countString = count.ToString(CultureInfo.InvariantCulture);
                    name += countString;
                    writer.Write(countString);
                }
                writer.Write("\" id=\"");
                writer.Write(name);
                writer.Write("\" value=\"");
                writer.Write(stateChunk);
                writer.WriteLine("\" />");
                ++count;
                _hiddenFieldsToRender[name] = stateChunk;
            }

            // DevDiv #461378: Write out an identifier so we know who generated this __VIEWSTATE field.
            // It doesn't need to be MACed since the only thing we use it for is error suppression,
            // similar to how __PREVIOUSPAGE works.
            if (EnableViewStateMacRegistryHelper.WriteViewStateGeneratorField) {
                // hex is easier than base64 to work with and consumes only one extra byte on the wire
                ClientScript.RegisterHiddenField(ViewStateGeneratorFieldID, GetClientStateIdentifier().ToString("X8", CultureInfo.InvariantCulture));
            }
        }
        else {
            // ASURT 106992
            // Need to always render out the viewstate field so alternate viewstate persistence will get called
            writer.Write("\r\n<input type=\"hidden\" name=\"");
            writer.Write(ViewStateFieldPrefixID);
            // Dev10 Bug 486494
            // Remove previously rendered NewLine
            writer.Write("\" id=\"");
            writer.Write(ViewStateFieldPrefixID);
            writer.WriteLine("\" value=\"\" />");
            _hiddenFieldsToRender[ViewStateFieldPrefixID] = String.Empty;
        }
    }

    /// <devdoc>
    ///     Default markup for begin form.
    /// </devdoc>
    internal void BeginFormRender(HtmlTextWriter writer, string formUniqueID) {

        // DevDiv 27324: Form should render div tag around hidden inputs
        // DevDiv 33149: backward compat. switch for obsolete rendering
        // Dev10 705089: in 4.0 mode or later, we render the div block with class="aspNetHidden" for xHTML conformance.
        bool renderDivAroundHiddenInputs = RenderDivAroundHiddenInputs(writer);
        if (renderDivAroundHiddenInputs) {
            writer.WriteLine();
            if (RenderingCompatibility >= VersionUtil.Framework40) {
                writer.Write("<div class=\"" + HiddenClassName + "\">");
            }
            else {
                writer.Write("<div>");
            }
        }

        ClientScript.RenderHiddenFields(writer);
        RenderViewStateFields(writer);

        if (renderDivAroundHiddenInputs) {
            writer.WriteLine("</div>");
        }

        if (ClientSupportsJavaScript) {
            if (MaintainScrollPositionOnPostBack && _requireScrollScript == false) {
                ClientScript.RegisterHiddenField(_scrollPositionXID, _scrollPositionX.ToString(CultureInfo.InvariantCulture));
                ClientScript.RegisterHiddenField(_scrollPositionYID, _scrollPositionY.ToString(CultureInfo.InvariantCulture));
                ClientScript.RegisterStartupScript(typeof(Page), PageScrollPositionScriptKey, @"
theForm.oldSubmit = theForm.submit;
theForm.submit = WebForm_SaveScrollPositionSubmit;

theForm.oldOnSubmit = theForm.onsubmit;
theForm.onsubmit = WebForm_SaveScrollPositionOnSubmit;
" + (IsPostBack ? @"
theForm.oldOnLoad = window.onload;
window.onload = WebForm_RestoreScrollPosition;
" : String.Empty), true);
                RegisterWebFormsScript();
                _requireScrollScript = true;
            }

            // VSWhidbey 375885, Render the focus script later (specifically for interaction with scrollposition)
            if (ClientSupportsFocus && Form != null && (RenderFocusScript || (Form.DefaultFocus.Length > 0) || (Form.DefaultButton.Length > 0))) {
                string focusedControlId = String.Empty;

                // Someone calling SetFocus(controlId) is the most precendent
                if (FocusedControlID.Length > 0) {
                    focusedControlId = FocusedControlID;
                }
                else if (FocusedControl != null) {
                    if (FocusedControl.Visible) {
                        focusedControlId = FocusedControl.ClientID;
                    }
                }
                else if (ValidatorInvalidControl.Length > 0) {
                    focusedControlId = ValidatorInvalidControl;
                }
                // AutoPostBack focus is the second least precendent
                else if (LastFocusedControl.Length > 0) {
                    // This doesn't have to be an ASP.NET control
                    focusedControlId = LastFocusedControl;
                }
                // DefaultFocus is the next
                else if (Form.DefaultFocus.Length > 0) {
                    // VSWhidbey 379627: Always render the default focus, regardless if we can find it, or if its visible
                    focusedControlId = Form.DefaultFocus;
                }
                // DefaultButton is the least precendent
                else if (Form.DefaultButton.Length > 0) {
                    focusedControlId = Form.DefaultButton;
                }

                // If something got focused, render some script to focus it only if its safe
                int match;
                if (focusedControlId.Length > 0 && !CrossSiteScriptingValidation.IsDangerousString(focusedControlId, out match) &&
                    CrossSiteScriptingValidation.IsValidJavascriptId(focusedControlId)) {

                    ClientScript.RegisterClientScriptResource(typeof(HtmlForm), "Focus.js");

                    if (!ClientScript.IsClientScriptBlockRegistered(typeof(HtmlForm), "Focus")) {
                        RegisterWebFormsScript();
                        ClientScript.RegisterStartupScript(
                            typeof(HtmlForm),
                            "Focus",
                            "WebForm_AutoFocus('" + Util.QuoteJScriptString(focusedControlId) + "');",
                            true);
                    }

                    IScriptManager scriptManager = ScriptManager;
                    if (scriptManager != null) {
                        scriptManager.SetFocusInternal(focusedControlId);
                    }
                }
            }


            // Set the necessary stuff to re-enable disabled controls on the client
            if (RenderDisabledControlsScript) {
                ClientScript.RegisterOnSubmitStatement(typeof(Page), PageReEnableControlsScriptKey, "WebForm_ReEnableControls();");
                RegisterWebFormsScript();
            }

            if (_fRequirePostBackScript) {
                RenderPostBackScript(writer, formUniqueID);
            }

            if (_fRequireWebFormsScript) {
                RenderWebFormsScript(writer);
            }
        }

        ClientScript.RenderClientScriptBlocks(writer);
    }

    internal void EndFormRenderArrayAndExpandoAttribute(HtmlTextWriter writer, string formUniqueID) {
        if (ClientSupportsJavaScript) {
            // Devdiv 9409 - Register the array for reenabling only after the controls have been processed,
            // so that list controls can have their children registered.
            if (RenderDisabledControlsScript) {
                foreach (Control control in EnabledControls) {
                    ClientScript.RegisterArrayDeclaration(EnabledControlArray, "'" + control.ClientID + "'");
                }
            }
            ClientScript.RenderArrayDeclares(writer);
            ClientScript.RenderExpandoAttribute(writer);
        }
    }

    private bool RenderDisabledControlsScript {
        get {
            return Form.SubmitDisabledControls && (EnabledControls.Count > 0) &&
                (_request.Browser.W3CDomVersion.Major > 0);
        }
    }

    internal void EndFormRenderHiddenFields(HtmlTextWriter writer, string formUniqueID) {
        if (RequiresViewStateEncryptionInternal) {
            ClientScript.RegisterHiddenField(ViewStateEncryptionID, String.Empty);
        }

        if (_containsCrossPagePost) {
            string path = EncryptString(Request.CurrentExecutionFilePath, Purpose.WebForms_Page_PreviousPageID);
            ClientScript.RegisterHiddenField(previousPageID, path);
        }

        if (EnableEventValidation) {
            ClientScript.SaveEventValidationField();
        }

        if (ClientScript.HasRegisteredHiddenFields) {
            bool renderDivAroundHiddenInputs = RenderDivAroundHiddenInputs(writer);
            if (renderDivAroundHiddenInputs) {
                writer.WriteLine();
                if (RenderingCompatibility >= VersionUtil.Framework40) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, HiddenClassName);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
            }

            ClientScript.RenderHiddenFields(writer);

            if (renderDivAroundHiddenInputs) {
                writer.RenderEndTag(); // DIV
            }
        }
    }

    internal void EndFormRenderPostBackAndWebFormsScript(HtmlTextWriter writer, string formUniqueID) {
        if (ClientSupportsJavaScript) {
            if (_fRequirePostBackScript && !_fPostBackScriptRendered) {
                RenderPostBackScript(writer, formUniqueID);
            }

            if (_fRequireWebFormsScript && !_fWebFormsScriptRendered)
                RenderWebFormsScript(writer);
        }

        ClientScript.RenderClientStartupScripts(writer);
    }

    /// <devdoc>
    ///     Default markup for end form.
    /// </devdoc>
    internal void EndFormRender(HtmlTextWriter writer, string formUniqueID) {
        EndFormRenderArrayAndExpandoAttribute(writer, formUniqueID);
        EndFormRenderHiddenFields(writer, formUniqueID);
        EndFormRenderPostBackAndWebFormsScript(writer, formUniqueID);
    }

    // VSWhidbey 475945: For ClientScriptManager.GetPostBackEventReference() to check if '$' should be used for id separator
    internal bool IsInOnFormRender {
        get {
            return _inOnFormRender;
        }
    }

    /// <devdoc>
    ///     Called by both adapters and default rendering prior to form rendering.
    /// </devdoc>
    internal void OnFormRender() {
        // Make sure there is only one form tag (ASURT 18891, 18894)
        if (_fOnFormRenderCalled) {
            throw new HttpException(SR.GetString(SR.Multiple_forms_not_allowed));
        }

        _fOnFormRenderCalled = true;
        _inOnFormRender = true;
    }

    /// <devdoc>
    ///     Called by both adapters and default rendering after form rendering.
    /// </devdoc>
    internal void OnFormPostRender(HtmlTextWriter writer) {
        _inOnFormRender = false;
        if (_postFormRenderDelegate != null) {
            _postFormRenderDelegate(writer, null);
        }
    }

    /// <devdoc>
    ///     Needed by adapters which do more than one pass, so that OnFormRender can be called more than once.
    /// </devdoc>
    // 
    internal void ResetOnFormRenderCalled() {
        _fOnFormRenderCalled = false;
    }


    /// <devdoc>
    /// Sets focus to the specified control
    /// </devdoc>
    public void SetFocus(Control control) {
        if (control == null) {
            throw new ArgumentNullException("control");
        }

        if (Form == null) {
            throw new InvalidOperationException(SR.GetString(SR.Form_Required_For_Focus));
        }

        if (Form.ControlState == ControlState.PreRendered) {
            throw new InvalidOperationException(SR.GetString(SR.Page_MustCallBeforeAndDuringPreRender, "SetFocus"));
        }

        _focusedControl = control;
        _focusedControlID = null;

        RegisterFocusScript();
    }


    /// <devdoc>
    /// Sets focus to the specified client id
    /// </devdoc>
    public void SetFocus(string clientID) {
        if ((clientID == null) || (clientID.Trim().Length == 0)) {
            throw new ArgumentNullException("clientID");
        }

        if (Form == null) {
            throw new InvalidOperationException(SR.GetString(SR.Form_Required_For_Focus));
        }

        if (Form.ControlState == ControlState.PreRendered) {
            throw new InvalidOperationException(SR.GetString(SR.Page_MustCallBeforeAndDuringPreRender, "SetFocus"));
        }

        _focusedControlID = clientID.Trim();
        _focusedControl = null;

        RegisterFocusScript();
    }

    internal void SetValidatorInvalidControlFocus(string clientID) {
        if (String.IsNullOrEmpty(_validatorInvalidControl)) {
            _validatorInvalidControl = clientID;

            RegisterFocusScript();
        }
    }

    //Note: BCL should provide a way to abort threads without asserting ControlThread for platform internal code.
    [SecurityPermission(SecurityAction.Assert, ControlThread = true)]
    internal static void ThreadResetAbortWithAssert() {
        Thread.ResetAbort();
    }

    /*
     * Enables controls to obtain client-side script function that will cause
     * (when invoked) a server post-back to the form.
     */

    /// <devdoc>
    ///    <para>
    ///       Associates the reference to the control that will
    ///       process the postback on the server.
    ///    </para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Obsolete("The recommended alternative is ClientScript.GetPostBackEventReference. http://go.microsoft.com/fwlink/?linkid=14202")]
    public string GetPostBackEventReference(Control control) {
        return ClientScript.GetPostBackEventReference(control, String.Empty);
    }

    /*
     * Enables controls to obtain client-side script function that will cause
     * (when invoked) a server post-back to the form.
     * argument: Parameter that will be passed to control on server
     */

    /// <devdoc>
    ///    <para>Passes a parameter to the control that will do the postback processing on the
    ///       server.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Obsolete("The recommended alternative is ClientScript.GetPostBackEventReference. http://go.microsoft.com/fwlink/?linkid=14202")]
    public string GetPostBackEventReference(Control control,
                                            string argument) {
        return ClientScript.GetPostBackEventReference(control, argument);
    }


    /// <devdoc>
    ///    <para>This returs a string that can be put in client event to post back to the named control</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Obsolete("The recommended alternative is ClientScript.GetPostBackEventReference. http://go.microsoft.com/fwlink/?linkid=14202")]
    public string GetPostBackClientEvent(Control control, string argument) {
        return ClientScript.GetPostBackEventReference(control, argument);
    }


    /// <devdoc>
    ///    <para>This returs a string that can be put in client event to post back to the named control</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Obsolete("The recommended alternative is ClientScript.GetPostBackClientHyperlink. http://go.microsoft.com/fwlink/?linkid=14202")]
    public string GetPostBackClientHyperlink(Control control, string argument) {
        return ClientScript.GetPostBackClientHyperlink(control, argument, false);
    }

    internal void InitializeStyleSheet() {
        if (_pageFlags[styleSheetInitialized]) {
            return;
        }

        String styleSheetName = StyleSheetTheme;
        if (!String.IsNullOrEmpty(styleSheetName)) {

            BuildResultCompiledType resultType = ThemeDirectoryCompiler.GetThemeBuildResultType(
                Context, styleSheetName);

            if (resultType != null) {
                _styleSheet = (PageTheme)resultType.CreateInstance();
                _styleSheet.Initialize(this, true);
            }
            else {
                throw new HttpException(SR.GetString(SR.Page_theme_not_found, styleSheetName));
            }
        }

        _pageFlags.Set(styleSheetInitialized);
    }

    private void InitializeThemes() {
        String themeName = Theme;
        if (!String.IsNullOrEmpty(themeName)) {
            BuildResultCompiledType resultType = ThemeDirectoryCompiler.GetThemeBuildResultType(
                Context, themeName);

            if (resultType != null) {
                _theme = (PageTheme)resultType.CreateInstance();
                _theme.Initialize(this, false);
            }
            else {
                throw new HttpException(SR.GetString(SR.Page_theme_not_found, themeName));
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected internal void AddContentTemplate(string templateName, ITemplate template) {
        if (_contentTemplateCollection == null) {
            _contentTemplateCollection = new Hashtable(11, StringComparer.OrdinalIgnoreCase);
        }

        try {
            _contentTemplateCollection.Add(templateName, template);
        }
        catch (ArgumentException) {
            throw new HttpException(SR.GetString(SR.MasterPage_Multiple_content, templateName));
        }
    }

    private void ApplyMasterPage() {
        if (Master != null) {
            ArrayList appliedMasterPages = new ArrayList();
            appliedMasterPages.Add(_masterPageFile.VirtualPathString.ToLower(CultureInfo.InvariantCulture));
            MasterPage.ApplyMasterRecursive(Master, appliedMasterPages);
        }
    }

    internal void ApplyControlSkin(Control ctrl) {
        if (_theme != null) {
            _theme.ApplyControlSkin(ctrl);
        }
    }

    [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    internal bool ApplyControlStyleSheet(Control ctrl) {
        if (_styleSheet != null) {
            _styleSheet.ApplyControlSkin(ctrl);
            return true;
        }

        return false;
    }

    internal void RegisterFocusScript() {
        if (ClientSupportsFocus && (_requireFocusScript == false)) {
            ClientScript.RegisterHiddenField(lastFocusID, String.Empty);
            _requireFocusScript = true;

            // If there are any partial caching controls on the stack, forward the call to them
            if (_partialCachingControlStack != null) {
                foreach(BasePartialCachingControl c in _partialCachingControlStack) {
                    c.RegisterFocusScript();
                }
            }
        }
    }

    internal void RegisterPostBackScript() {
        if (!ClientSupportsJavaScript) {
            return;
        }

        if (_fPostBackScriptRendered) {
            return;
        }

        if (!_fRequirePostBackScript) {
            ClientScript.RegisterHiddenField(postEventSourceID, String.Empty);
            ClientScript.RegisterHiddenField(postEventArgumentID, String.Empty);

            _fRequirePostBackScript = true;
        }

        // If there are any partial caching controls on the stack, forward the call to them
        if (_partialCachingControlStack != null) {
            foreach(BasePartialCachingControl c in _partialCachingControlStack) {
                c.RegisterPostBackScript();
            }
        }
    }

    private void RenderPostBackScript(HtmlTextWriter writer, string formUniqueID) {
        writer.Write(EnableLegacyRendering ?
            ClientScriptManager.ClientScriptStartLegacy :
            ClientScriptManager.ClientScriptStart);
        if (PageAdapter != null) {
            writer.Write("var theForm = ");
            writer.Write(PageAdapter.GetPostBackFormReference(formUniqueID));
            writer.WriteLine(";");
        }
        else {
            writer.Write("var theForm = document.forms['");
            writer.Write(formUniqueID);
            writer.WriteLine("'];");

            // VSWhidbey 392597: Try to use the document._ctl00 syntax since PocketPC doesn't support document.forms[id]
            writer.Write("if (!theForm) {\r\n    theForm = document.");
            writer.Write(formUniqueID);
            writer.WriteLine(";\r\n}");
        }
        writer.WriteLine(@"function __doPostBack(eventTarget, eventArgument) {
    if (!theForm.onsubmit || (theForm.onsubmit() != false)) {
        theForm.__EVENTTARGET.value = eventTarget;
        theForm.__EVENTARGUMENT.value = eventArgument;
        theForm.submit();
    }
}");
        writer.WriteLine(EnableLegacyRendering ?
             ClientScriptManager.ClientScriptEndLegacy :
             ClientScriptManager.ClientScriptEnd);
        _fPostBackScriptRendered = true;
    }


    /// <devdoc>
    ///   Allows controls on a page to access to the _doPostBack and _doCallback JavaScript handlers on the
    ///   client. This method can be called multiple times by multiple controls. It should
    ///   render only one instance of the WebForms script.
    /// </devdoc>
    internal void RegisterWebFormsScript() {
        if (ClientSupportsJavaScript) {
            if (_fWebFormsScriptRendered) {
                return;
            }

            RegisterPostBackScript();

            _fRequireWebFormsScript = true;

            // If there are any partial caching controls on the stack, forward the call to them
            if (_partialCachingControlStack != null) {
                foreach(BasePartialCachingControl c in _partialCachingControlStack) {
                    c.RegisterWebFormsScript();
                }
            }
        }
    }

    private void RenderWebFormsScript(HtmlTextWriter writer) {
        ClientScript.RenderWebFormsScript(writer);
        _fWebFormsScriptRendered = true;
    }


    /// <devdoc>
    ///    <para>Determines if the client script block is registered with the page.</para>
    /// </devdoc>
    [Obsolete("The recommended alternative is ClientScript.IsClientScriptBlockRegistered(string key). http://go.microsoft.com/fwlink/?linkid=14202")]
    public bool IsClientScriptBlockRegistered(string key) {
        return ClientScript.IsClientScriptBlockRegistered(typeof(Page), key);
    }


    /// <devdoc>
    ///    <para>Determines if the client startup script is registered with the
    ///       page.</para>
    /// </devdoc>
    [Obsolete("The recommended alternative is ClientScript.IsStartupScriptRegistered(string key). http://go.microsoft.com/fwlink/?linkid=14202")]
    public bool IsStartupScriptRegistered(string key) {
        return ClientScript.IsStartupScriptRegistered(typeof(Page), key);
    }


    /// <devdoc>
    ///    <para>Declares a value that will be declared as a JavaScript array declaration
    ///       when the page renders. This can be used by script-based controls to declare
    ///       themselves within an array so that a client script library can work with
    ///       all the controls of the same type.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Obsolete("The recommended alternative is ClientScript.RegisterArrayDeclaration(string arrayName, string arrayValue). http://go.microsoft.com/fwlink/?linkid=14202")]
    public void RegisterArrayDeclaration(string arrayName, string arrayValue) {
        ClientScript.RegisterArrayDeclaration(arrayName, arrayValue);
    }


    /// <devdoc>
    ///    <para>
    ///       Allows controls to automatically register a hidden field on the form. The
    ///       field will be emitted when the form control renders itself.
    ///    </para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Obsolete("The recommended alternative is ClientScript.RegisterHiddenField(string hiddenFieldName, string hiddenFieldInitialValue). http://go.microsoft.com/fwlink/?linkid=14202")]
    public virtual void RegisterHiddenField(string hiddenFieldName, string hiddenFieldInitialValue) {
        ClientScript.RegisterHiddenField(hiddenFieldName, hiddenFieldInitialValue);
    }


    /// <devdoc>
    ///    <para> Prevents controls from sending duplicate blocks of
    ///       client-side script to the client. Any script blocks with the same <paramref name="key"/> parameter
    ///       values are considered duplicates.</para>
    /// </devdoc>
    [Obsolete("The recommended alternative is ClientScript.RegisterClientScriptBlock(Type type, string key, string script). http://go.microsoft.com/fwlink/?linkid=14202")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public virtual void RegisterClientScriptBlock(string key, string script) {
        ClientScript.RegisterClientScriptBlock(typeof(Page), key, script);
    }


    /// <devdoc>
    ///    <para>
    ///       Allows controls to keep duplicate blocks of client-side script code from
    ///       being sent to the client. Any script blocks with the same <paramref name="key"/> parameter
    ///       value are considered duplicates.
    ///    </para>
    /// </devdoc>
    [Obsolete("The recommended alternative is ClientScript.RegisterStartupScript(Type type, string key, string script). http://go.microsoft.com/fwlink/?linkid=14202")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public virtual void RegisterStartupScript(string key, string script) {
        ClientScript.RegisterStartupScript(typeof(Page), key, script, false);
    }


    /// <devdoc>
    ///    <para>Allows a control to access a the client
    ///    <see langword='onsubmit'/> event.
    ///       The script should be a function call to client code registered elsewhere.</para>
    /// </devdoc>
    [Obsolete("The recommended alternative is ClientScript.RegisterOnSubmitStatement(Type type, string key, string script). http://go.microsoft.com/fwlink/?linkid=14202")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public void RegisterOnSubmitStatement(string key, string script) {
        ClientScript.RegisterOnSubmitStatement(typeof(Page), key, script);
    }

    internal void RegisterEnabledControl(Control control) {
        EnabledControls.Add(control);
    }

    /// <devdoc>
    ///    <para>If called, Control State for this control will be persisted.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public void RegisterRequiresControlState(Control control) {
        if (control == null) {
            throw new ArgumentException(SR.GetString(SR.Page_ControlState_ControlCannotBeNull));
        }

        if (control.ControlState == ControlState.PreRendered) {
            throw new InvalidOperationException(SR.GetString(SR.Page_MustCallBeforeAndDuringPreRender, "RegisterRequiresControlState"));
        }

        if (_registeredControlsRequiringControlState == null) {
            _registeredControlsRequiringControlState = new ControlSet();
        }

        // Don't do anything if RegisterRequiresControlState is called multiple times on the same control.
        if (!_registeredControlsRequiringControlState.Contains(control)) {
            _registeredControlsRequiringControlState.Add(control);

            IDictionary controlState = (IDictionary)PageStatePersister.ControlState;
            if (controlState != null) {
                string uniqueID = control.UniqueID;

                // VSWhidbey 422416: We allow control state loaded only once, to
                // match the same behavior of ViewState loading in Control.AddedControl
                // method which ViewState is removed after applied once.  The
                // scenario is having a control to be re-parented multiple times.
                // Note: We can't call remove here, because we may be in the middle of iterating thru
                // the keys of controlState(within in a call to RegisterRequiresClearChildControlState),
                // so we just remember that we loaded this control's control state
                if (!ControlStateLoadedControlIds.Contains(uniqueID)) {
                    control.LoadControlStateInternal(controlState[uniqueID]);
                    ControlStateLoadedControlIds.Add(uniqueID);
                }
            }
        }
    }

    public bool RequiresControlState(Control control) {
        return (_registeredControlsRequiringControlState != null && _registeredControlsRequiringControlState.Contains(control));
    }

    /// <devdoc>
    ///    <para>If called, Control State for this control will no longer persisted.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public void UnregisterRequiresControlState(Control control) {
        if (control == null) {
            throw new ArgumentException(SR.GetString(SR.Page_ControlState_ControlCannotBeNull));
        }

        if (_registeredControlsRequiringControlState == null) {
            return;
        }

        _registeredControlsRequiringControlState.Remove(control);
    }

    internal bool ShouldLoadControlState(Control control) {
        if (_registeredControlsRequiringClearChildControlState == null) return true;
        foreach (Control cleared in _registeredControlsRequiringClearChildControlState.Keys) {
            if (control != cleared && control.IsDescendentOf(cleared)) return false;
        }
        return true;
    }

    internal void RegisterRequiresClearChildControlState(Control control) {
        if (_registeredControlsRequiringClearChildControlState == null) {
            _registeredControlsRequiringClearChildControlState = new HybridDictionary();
            _registeredControlsRequiringClearChildControlState.Add(control, true);
        }
        else if (_registeredControlsRequiringClearChildControlState[control] == null) {
            _registeredControlsRequiringClearChildControlState.Add(control, true);
        }

        IDictionary controlState = (IDictionary)PageStatePersister.ControlState;
        if (controlState != null) {
            // Clear out the control state for children of this control
            List<string> controlsToClear = new List<string>(controlState.Count);
            foreach (string id in controlState.Keys) {
                Control controlWithState = FindControl(id);
                if (controlWithState != null && controlWithState.IsDescendentOf(control)) {
                    controlsToClear.Add(id);
                }
            }
            foreach (string id in controlsToClear) {
                controlState[id] = null;
            }
        }
    }


    /// <devdoc>
    ///    <para>Registers a control as one that requires postback handling.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public void RegisterRequiresPostBack(Control control) {

        // Fail if the control is not an IPostBackDataHandler (VSWhidbey 184483)
        if (!(control is IPostBackDataHandler)) {
            IPostBackDataHandler dataHandler = control.AdapterInternal as IPostBackDataHandler;
            if (dataHandler == null)
                throw new HttpException(SR.GetString(SR.Ctrl_not_data_handler));
        }

        if (_registeredControlsThatRequirePostBack == null)
            _registeredControlsThatRequirePostBack = new ArrayList();

        _registeredControlsThatRequirePostBack.Add(control.UniqueID);
    }

    // Push a BasePartialCachingControl on the stack of registered caching controls
    internal void PushCachingControl(BasePartialCachingControl c) {

        // Create the stack on demand
        if (_partialCachingControlStack == null) {
            _partialCachingControlStack = new Stack();
        }

        _partialCachingControlStack.Push(c);
    }

    // Pop a BasePartialCachingControl from the stack of registered caching controls
    internal void PopCachingControl() {
        Debug.Assert(_partialCachingControlStack != null);
        _partialCachingControlStack.Pop();
    }


    /*
     * This method will process the data posted back in the request header.
     * The collection of posted data keys consists of three types :
     * 1.  Fully qualified ids of controls.  The associated value is the data
     *     posted back by the browser for an intrinsic html element.
     * 2.  Fully qualified ids of controls that have explicitly registered that
     *     they want to be notified on postback.  This is required for intrinsic
     *     html elements that for some states do not postback data ( e.g. a select
     *     when there is no selection, a checkbox or radiobutton that is not checked )
     *     The associated value for these keys is not relevant.
     * 3.  Framework generated hidden fields for event processing, whose values are
     *     set by client-side script prior to postback.
     *
     * This method handles the process of notifying the relevant controls that a postback
     * has occurred, via the IPostBackDataHandler interface.
     *
     * It can potentially be called twice: before and after LoadControl.  This is to
     * handle the case where users programmatically add controls in Page_Load (ASURT 29045).
     */
    private void ProcessPostData(NameValueCollection postData, bool fBeforeLoad) {
        if (_changedPostDataConsumers == null)
            _changedPostDataConsumers = new ArrayList();

        // identify controls that have postback data
        if (postData != null) {
            foreach (string postKey in postData) {
                if (postKey != null) {
                    // Ignore system post fields
                    if (IsSystemPostField(postKey))
                        continue;

                    Control ctrl = FindControl(postKey);
                    if (ctrl == null) {
                        if (fBeforeLoad) {
                            // It was not found, so keep track of it for the post load attempt
                            if (_leftoverPostData == null)
                                _leftoverPostData = new NameValueCollection();
                            _leftoverPostData.Add(postKey, null);
                        }
                        continue;
                    }

                    IPostBackDataHandler consumer = ctrl.PostBackDataHandler;

                    // Ignore controls that are not IPostBackDataHandler (see ASURT 13581)
                    if (consumer == null) {

                        // If it's a IPostBackEventHandler (which doesn't implement IPostBackDataHandler),
                        // register it (ASURT 39040)
                        if(ctrl.PostBackEventHandler != null)
                            RegisterRequiresRaiseEvent(ctrl.PostBackEventHandler);

                        continue;
                    }

                    bool changed;
                    if(consumer != null) {
                        NameValueCollection postCollection = ctrl.CalculateEffectiveValidateRequest() ? _requestValueCollection : _unvalidatedRequestValueCollection;
                        changed = consumer.LoadPostData(postKey, postCollection);
                        if(changed)
                           _changedPostDataConsumers.Add(ctrl);
                    }

                    // ensure controls are only notified of postback once
                    if (_controlsRequiringPostBack != null)
                        _controlsRequiringPostBack.Remove(postKey);
                }
            }
        }

        // Keep track of the leftover for the post-load attempt
        ArrayList leftOverControlsRequiringPostBack = null;

        // process controls that explicitly registered to be notified of postback
        if (_controlsRequiringPostBack != null) {
            foreach (string controlID in _controlsRequiringPostBack) {
                Control c = FindControl(controlID);

                if (c != null) {
                    IPostBackDataHandler consumer = c.AdapterInternal as IPostBackDataHandler;
                    if(consumer == null) {
                        consumer = c as IPostBackDataHandler;
                    }

                    // Give a helpful error if the control is not a IPostBackDataHandler (ASURT 128532)
                    if (consumer == null) {
                        throw new HttpException(SR.GetString(SR.Postback_ctrl_not_found, controlID));
                    }

                    NameValueCollection postCollection = c.CalculateEffectiveValidateRequest() ? _requestValueCollection : _unvalidatedRequestValueCollection;
                    bool changed = consumer.LoadPostData(controlID, postCollection);
                    if (changed)
                        _changedPostDataConsumers.Add(c);
                }
                else {
                    if (fBeforeLoad) {
                        if (leftOverControlsRequiringPostBack == null)
                            leftOverControlsRequiringPostBack = new ArrayList();
                        leftOverControlsRequiringPostBack.Add(controlID);
                    }
                }
            }

            _controlsRequiringPostBack = leftOverControlsRequiringPostBack;
        }

    }

    /*
     * This method will raise change events for those controls that indicated
     * during PostProcessData that their data has changed.
     */
    // !! IMPORTANT !!
    // If you change this method, also change RaiseChangedEventsAsync.
    internal void RaiseChangedEvents() {
        if (_changedPostDataConsumers != null) {
            // fire change notifications for those controls that changed as a result of postback
            for (int i=0; i < _changedPostDataConsumers.Count; i++) {
                Control c = (Control)_changedPostDataConsumers[i];
                IPostBackDataHandler changedPostDataConsumer;

                if(c != null) {
                    changedPostDataConsumer = c.PostBackDataHandler;
                }
                else {
                    continue;
                }

                // Make sure the IPostBackDataHandler is still in the tree (ASURT 82495)
                if (c != null && !c.IsDescendentOf(this))
                    continue;

                if(c != null && c.PostBackDataHandler != null) {
                    changedPostDataConsumer.RaisePostDataChangedEvent();
                }
            }
        }
    }

    // TAP version of RaiseChangedEvents.
    // !! IMPORTANT !!
    // If you change this method, also change RaiseChangedEvents.
    internal async Task RaiseChangedEventsAsync() {
        if (_changedPostDataConsumers != null) {
            // fire change notifications for those controls that changed as a result of postback
            for (int i = 0; i < _changedPostDataConsumers.Count; i++) {
                Control c = (Control)_changedPostDataConsumers[i];
                IPostBackDataHandler changedPostDataConsumer;

                if (c != null) {
                    changedPostDataConsumer = c.PostBackDataHandler;
                }
                else {
                    continue;
                }

                // Make sure the IPostBackDataHandler is still in the tree (ASURT 82495)
                if (c != null && !c.IsDescendentOf(this))
                    continue;

                if (c != null && c.PostBackDataHandler != null) {
                    using (Context.SyncContext.AllowVoidAsyncOperationsBlock()) {
                        changedPostDataConsumer.RaisePostDataChangedEvent();
                        await GetWaitForPreviousStepCompletionAwaitable();
                    }
                }
            }
        }
    }

    private void RaisePostBackEvent(NameValueCollection postData) {

        // first check if there is a register control needing the postback event
        // if we don't have one of those, fall back to the hidden field
        // Note: this must happen before we look at postData[postEventArgumentID] (ASURT 50106)
        if (_registeredControlThatRequireRaiseEvent != null) {
            RaisePostBackEvent(_registeredControlThatRequireRaiseEvent, null);
        }
        else {
            string eventSource = postData[postEventSourceID];
            bool hasEventSource = (!String.IsNullOrEmpty(eventSource));

            // VSWhidbey 204824: We also need to check if the postback is submitted
            // by an autopostback control in mobile browsers which cannot set
            // event target in markup
            if (hasEventSource || AutoPostBackControl != null) {
                Control sourceControl = null;
                if (hasEventSource) {
                    sourceControl = FindControl(eventSource);
                }

                if (sourceControl != null && sourceControl.PostBackEventHandler != null) {
                    string eventArgument = postData[postEventArgumentID];
                    RaisePostBackEvent((sourceControl.PostBackEventHandler), eventArgument);
                }
            }
            else {
                Validate();
            }
        }
    }

    // Overridable method that just calls RaisePostBackEvent on controls (ASURT 48154)

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void RaisePostBackEvent(IPostBackEventHandler sourceControl, string eventArgument) {
        sourceControl.RaisePostBackEvent(eventArgument);
    }

    // 

    /// <devdoc>
    ///    <para>Registers a control as requiring an event to be raised when it is processed
    ///       on the page.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public virtual void RegisterRequiresRaiseEvent(IPostBackEventHandler control) {
        _registeredControlThatRequireRaiseEvent = control;
    }

    // VSWhidbey 402530
    // This property should be public. (DevDiv Bugs 161340)
    public bool IsPostBackEventControlRegistered {
        get {
            return (_registeredControlThatRequireRaiseEvent != null);
        }
    }


    /// <devdoc>
    ///    <para> Indicates whether page validation succeeded.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public bool IsValid {
        get {
            if (!_validated)
                throw new HttpException(SR.GetString(SR.IsValid_Cant_Be_Called));

            if (_validators != null) {
                ValidatorCollection vc = Validators;
                int count = vc.Count;
                for (int i = 0; i < count; i++) {
                    if (!vc[i].IsValid) {
                        return false;
                    }
                }
            }
            return true;
        }
    }


    /// <devdoc>
    ///    <para>Gets a collection of all validation controls contained on the requested page.</para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public ValidatorCollection Validators {
        get {
            if (_validators == null) {
                _validators = new ValidatorCollection();
            }
            return _validators;
        }
    }


    /// <devdoc>
    ///    <para>Gets the PreviousPage of current Page, it could be either the original Page from
    ///    Server.Transfer or cross page posting.
    ///    </para>
    /// </devdoc>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
    ]
    public Page PreviousPage {
        get {
            // check _previousPage first since _previousPagePath could be null in case of Server.Transfer
            if (_previousPage == null) {
                if (_previousPagePath != null) {

                    if (!Util.IsUserAllowedToPath(Context, _previousPagePath)) {
                        throw new InvalidOperationException(SR.GetString(SR.Previous_Page_Not_Authorized));
                    }

                    ITypedWebObjectFactory result =
                        (ITypedWebObjectFactory)BuildManager.GetVPathBuildResult(Context, _previousPagePath);

                    // Make sure it has the correct base type
                    if (typeof(Page).IsAssignableFrom(result.InstantiatedType)) {
                        _previousPage = (Page)result.CreateInstance();
                        _previousPage._isCrossPagePostBack = true;

                        Server.Execute(_previousPage, TextWriter.Null,
                            true /*preserveForm*/, false /*setPreviousPage*/);
                    }
                }
            }

            return _previousPage;
        }
    }

    /*
     * Map virtual path (absolute or relative) to physical path
     */

    /// <devdoc>
    ///    <para>Assigns a virtual path, either absolute or relative, to a physical path.</para>
    /// </devdoc>
    public string MapPath(string virtualPath) {
        return _request.MapPath(VirtualPath.CreateAllowNull(virtualPath), TemplateControlVirtualDirectory,
            true/*allowCrossAppMapping*/);
    }

    /*
     * The following members should only be set by derived class through codegen.
     * 
*/

    static char[] s_varySeparator = new char[] {';'};


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    ///    Note: this methods needs to be virtual because the Mobile control team
    ///    overrides it (ASURT 66157)
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected virtual void InitOutputCache(int duration, string varyByHeader,
        string varyByCustom, OutputCacheLocation location, string varyByParam) {
        InitOutputCache(duration, null, varyByHeader, varyByCustom, location, varyByParam);
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    ///    Note: this methods needs to be virtual because the Mobile control team
    ///    overrides it (ASURT 66157)
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected virtual void InitOutputCache(int duration, string varyByContentEncoding, string varyByHeader,
        string varyByCustom, OutputCacheLocation location, string varyByParam) {

        // DevDivBugs 18348: for a cross-page postback, use cache policy for
        // original page and ignore cache policy for this page.
        if (_isCrossPagePostBack) {
            return;
        }

        OutputCacheParameters cacheSettings = new OutputCacheParameters();

        cacheSettings.Duration = duration;
        cacheSettings.VaryByContentEncoding = varyByContentEncoding;
        cacheSettings.VaryByHeader = varyByHeader;
        cacheSettings.VaryByCustom = varyByCustom;
        cacheSettings.Location = location;
        cacheSettings.VaryByParam = varyByParam;

        InitOutputCache(cacheSettings);
    }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    ///    Note: this methods needs to be virtual because the Mobile control team
    ///    overrides it (ASURT 66157)
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected internal virtual void InitOutputCache(OutputCacheParameters cacheSettings)
    {
        // DevDivBugs 18348: for a cross-page postback, use cache policy for
        // original page and ignore cache policy for this page.
        if (_isCrossPagePostBack) {
            return;
        }
        OutputCacheSettingsSection outputCacheSettings;
        OutputCacheProfile profile = null;
        HttpCachePolicy     cache = Response.Cache;
        HttpCacheability    cacheability;
        OutputCacheLocation location = (OutputCacheLocation) (-1);
        int duration = 0;
        string varyByContentEncoding = null;
        string varyByHeader = null;
        string varyByCustom = null;
        string varyByParam = null;
        string sqlDependency = null;
        string varyByControl = null;
        bool noStore = false;
        RuntimeConfig config;

        config = RuntimeConfig.GetAppConfig();
        OutputCacheSection outputCacheConfig = config.OutputCache;

        // If output cache is not enabled, then don't do anything and return.
        if (! outputCacheConfig.EnableOutputCache)
        {
            return;
        }

        if (cacheSettings.CacheProfile != null && cacheSettings.CacheProfile.Length != 0)
        {
            outputCacheSettings = config.OutputCacheSettings;
            profile = (OutputCacheProfile) outputCacheSettings.OutputCacheProfiles[cacheSettings.CacheProfile];

            if (profile == null) {
                throw new HttpException(SR.GetString(SR.CacheProfile_Not_Found, cacheSettings.CacheProfile));
            }

            // If the profile disables it, then bail out
            if (!profile.Enabled) {
                return;
            }
        }

        // If a cache profile was set above, the settings below will override the set defaults from config

        // Pick up the settings from the configuration settings profile first
        if (profile != null) {
            duration = profile.Duration;
            varyByContentEncoding = profile.VaryByContentEncoding;
            varyByHeader = profile.VaryByHeader;
            varyByCustom = profile.VaryByCustom;
            varyByParam = profile.VaryByParam;
            sqlDependency = profile.SqlDependency;
            noStore = profile.NoStore;
            varyByControl = profile.VaryByControl;
            location = profile.Location;

            if (String.IsNullOrEmpty(varyByContentEncoding)) {
                varyByContentEncoding = null;
            }
            if (String.IsNullOrEmpty(varyByHeader)) {
                varyByHeader = null;
            }
            if (String.IsNullOrEmpty(varyByCustom)) {
                varyByCustom = null;
            }
            if (String.IsNullOrEmpty(varyByParam)) {
                varyByParam = null;
            }
            if (String.IsNullOrEmpty(varyByControl)) {
                varyByControl = null;
            }

            if (StringUtil.EqualsIgnoreCase(varyByParam, "none")) {
                varyByParam = null;
            }

            if (StringUtil.EqualsIgnoreCase(varyByControl, "none")) {
                varyByControl = null;
            }
        }

        // Start overriding options from the directive
        if (cacheSettings.IsParameterSet(OutputCacheParameter.Duration)) {
            duration = cacheSettings.Duration;
        }
        if (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByContentEncoding)) {
            varyByContentEncoding = cacheSettings.VaryByContentEncoding;
        }
        if (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByHeader)) {
            varyByHeader = cacheSettings.VaryByHeader;
        }
        if (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByCustom)) {
            varyByCustom = cacheSettings.VaryByCustom;
        }
        if (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByControl)) {
            varyByControl = cacheSettings.VaryByControl;
        }
        if (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByParam)) {
            varyByParam = cacheSettings.VaryByParam;
        }
        if (cacheSettings.IsParameterSet(OutputCacheParameter.SqlDependency)) {
            sqlDependency = cacheSettings.SqlDependency;
        }
        if (cacheSettings.IsParameterSet(OutputCacheParameter.NoStore)) {
            noStore = cacheSettings.NoStore;
        }
        if (cacheSettings.IsParameterSet(OutputCacheParameter.Location)) {
            location = cacheSettings.Location;
        }

        // 

        // Make some checks here and see if a configuration exception needs to be thrown:

        // If location wasn't specified in the profile or in the directive, set a default one
        if (location == (OutputCacheLocation) (-1)) {
            location = OutputCacheLocation.Any;
        }

        // Skip all checks if Location is "None" or we are disabled
        if ((location != OutputCacheLocation.None) &&
            (profile == null || profile.Enabled)) {

            // Check and see if duration is specified in the profile or in the directives
            if ((profile == null || profile.Duration == -1) &&
                (cacheSettings.IsParameterSet(OutputCacheParameter.Duration) == false)) {
                throw new HttpException(SR.GetString(SR.Missing_output_cache_attr, "duration"));
            }

            // Check and see if varyByParam is specified in the profile or in the directives
            if ((profile == null || ((profile.VaryByParam == null) && (profile.VaryByControl == null))) &&
                (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByParam) == false &&
                cacheSettings.IsParameterSet(OutputCacheParameter.VaryByControl) == false)) {
                throw new HttpException(SR.GetString(SR.Missing_output_cache_attr, "varyByParam"));
            }
        }

        // Set the cache policy based upon these settings

        if (noStore) {
            Response.Cache.SetNoStore();
        }

        switch (location) {
            case OutputCacheLocation.Any:
                cacheability = HttpCacheability.Public;
                break;

            case OutputCacheLocation.Server:
                cacheability = HttpCacheability.ServerAndNoCache;
                break;

            case OutputCacheLocation.ServerAndClient:
                cacheability = HttpCacheability.ServerAndPrivate;
                break;

            case OutputCacheLocation.Client:
                cacheability = HttpCacheability.Private;
                break;

            case OutputCacheLocation.Downstream:
                cacheability = HttpCacheability.Public;
                cache.SetNoServerCaching();
                break;

            case OutputCacheLocation.None:
                cacheability = HttpCacheability.NoCache;
                break;

            default:
                throw new ArgumentOutOfRangeException("cacheSettings", SR.GetString(SR.Invalid_cache_settings_location));
        }

        cache.SetCacheability(cacheability);

        if (location != OutputCacheLocation.None) {
            cache.SetExpires(Context.Timestamp.AddSeconds(duration));
            cache.SetMaxAge(new TimeSpan(0, 0, duration));
            cache.SetValidUntilExpires(true);
            cache.SetLastModified(Context.Timestamp);

            //
            // A client cache'd item won't be cached on
            // the server or a proxy, so it doesn't need
            // a Varies header.
            //
            if (location != OutputCacheLocation.Client) {
                if (varyByContentEncoding != null) {
                    string[] a = varyByContentEncoding.Split(s_varySeparator);
                    foreach (string s in a) {
                        cache.VaryByContentEncodings[s.Trim()] = true;
                    }
                }
                if (varyByHeader != null) {
                    string[] a = varyByHeader.Split(s_varySeparator);
                    foreach (string s in a) {
                        cache.VaryByHeaders[s.Trim()] = true;
                    }
                }
                if(PageAdapter != null) {
                    StringCollection adapterVaryByHeaders = PageAdapter.CacheVaryByHeaders;
                    if(adapterVaryByHeaders != null) {
                        foreach(string header in adapterVaryByHeaders) {
                            cache.VaryByHeaders[header] = true;
                        }
                    }
                }

                //
                // Only items cached on the server need VaryByCustom and
                // VaryByParam
                //
                if (location != OutputCacheLocation.Downstream) {
                    if (varyByCustom != null) {
                        cache.SetVaryByCustom(varyByCustom);
                    }

                    if (String.IsNullOrEmpty(varyByParam) &&
                        String.IsNullOrEmpty(varyByControl) &&
                        (PageAdapter == null || PageAdapter.CacheVaryByParams == null)) {
                        cache.VaryByParams.IgnoreParams = true;
                    }
                    else {
                        if (!String.IsNullOrEmpty(varyByParam)) {
                            string[] a = varyByParam.Split(s_varySeparator);
                            foreach (string s in a) {
                                cache.VaryByParams[s.Trim()] = true;
                            }
                        }
                        if (!String.IsNullOrEmpty(varyByControl)) {
                            string[] a = varyByControl.Split(s_varySeparator);
                            foreach (string s in a) {
                                cache.VaryByParams[s.Trim()] = true;
                            }
                        }
                        if(PageAdapter != null) {
                            IList adapterVaryByParams = PageAdapter.CacheVaryByParams;
                            if(adapterVaryByParams != null) {
                                foreach(string p in adapterVaryByParams) {
                                    cache.VaryByParams[p] = true;
                                }
                            }
                        }
                    }

#if !FEATURE_PAL // FEATURE_PAL does not fully SQL dependencies
                    if (!String.IsNullOrEmpty(sqlDependency)) {
                        Response.AddCacheDependency(SqlCacheDependency.CreateOutputCacheDependency(sqlDependency));
                    }
#endif // !FEATURE_PAL
                }
            }
        }
    }


    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("The recommended alternative is HttpResponse.AddFileDependencies. http://go.microsoft.com/fwlink/?linkid=14202")]
    protected ArrayList FileDependencies {
        set { Response.AddFileDependencies(value); }
    }

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected object GetWrappedFileDependencies(string[] virtualFileDependencies) {
        Debug.Assert(virtualFileDependencies != null);
        return virtualFileDependencies;
    }


    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected internal void AddWrappedFileDependencies(object virtualFileDependencies) {
        Response.AddVirtualPathDependencies((string[])virtualFileDependencies);
    }

    internal const bool BufferDefault = true;

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Buffer {
        set { Response.BufferOutput = value; }
        get { return Response.BufferOutput; }
    }

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ContentType {
        set { Response.ContentType = value; }
        get { return Response.ContentType; }
    }

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CodePage {
        set { Response.ContentEncoding = Encoding.GetEncoding(value); }
        get { return Response.ContentEncoding.CodePage; }
    }

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ResponseEncoding {
        set { Response.ContentEncoding = Encoding.GetEncoding(value); }
        get { return Response.ContentEncoding.EncodingName; }
    }

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Culture {
        set {
            CultureInfo newCulture = null;

            if(StringUtil.EqualsIgnoreCase(value, HttpApplication.AutoCulture)) {
                CultureInfo browserCulture = CultureFromUserLanguages(true);
                if(browserCulture != null) {
                    newCulture = browserCulture;
                }
            }
            else if(StringUtil.StringStartsWithIgnoreCase(value, HttpApplication.AutoCulture)) {
                CultureInfo browserCulture = CultureFromUserLanguages(true);
                if(browserCulture != null) {
                    newCulture = browserCulture;
                }
                else {
                    try {
                        newCulture = HttpServerUtility.CreateReadOnlyCultureInfo(value.Substring(5));
                    }
                    catch {}
                }
            }
            else {
                newCulture = HttpServerUtility.CreateReadOnlyCultureInfo(value);
            }

            if (newCulture != null) {
                Thread.CurrentThread.CurrentCulture = newCulture;
                _dynamicCulture = newCulture;
            }
        }
        get { return Thread.CurrentThread.CurrentCulture.DisplayName; }
    }

    internal CultureInfo DynamicCulture {
        get { return _dynamicCulture; }
    }

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int LCID {
        set {
            CultureInfo newCulture = HttpServerUtility.CreateReadOnlyCultureInfo(value);
            Thread.CurrentThread.CurrentCulture = newCulture;
            _dynamicCulture = newCulture;
        }

        get { return Thread.CurrentThread.CurrentCulture.LCID; }
    }

    private CultureInfo CultureFromUserLanguages(bool specific) {
        if(_context != null &&
                        _context.Request != null &&
                        _context.Request.UserLanguages != null) {
            try {
                return CultureUtil.CreateReadOnlyCulture(_context.Request.UserLanguages, specific);
            }
            catch {
            }
        }
        return null;
    }


    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string UICulture {
        set {
            CultureInfo newUICulture = null;

            if(StringUtil.EqualsIgnoreCase(value, HttpApplication.AutoCulture)) {
                CultureInfo browserCulture = CultureFromUserLanguages(false);
                if(browserCulture != null) {
                    newUICulture = browserCulture;
                }
            }
            else if(StringUtil.StringStartsWithIgnoreCase(value, HttpApplication.AutoCulture)) {
                CultureInfo browserCulture = CultureFromUserLanguages(false);
                if(browserCulture != null) {
                    newUICulture = browserCulture;
                }
                else {
                    try {
                        newUICulture = HttpServerUtility.CreateReadOnlyCultureInfo(value.Substring(5));
                    }
                    catch {}
                }
            }
            else {
                newUICulture = HttpServerUtility.CreateReadOnlyCultureInfo(value);
            }

            if (newUICulture != null) {
                Thread.CurrentThread.CurrentUICulture = newUICulture;
                _dynamicUICulture = newUICulture;
            }
        }
        get { return Thread.CurrentThread.CurrentUICulture.DisplayName; }
    }

    internal CultureInfo DynamicUICulture {
        get { return _dynamicUICulture; }
    }

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TimeSpan AsyncTimeout {
        set {
            if (value < TimeSpan.Zero) {
                throw new ArgumentException(SR.GetString(SR.Page_Illegal_AsyncTimeout), "AsyncTimeout");
            }

            _asyncTimeout = value;
            _asyncTimeoutSet = true;
        }
        get {
            if (!_asyncTimeoutSet) {
                if (Context != null) {
                    PagesSection pagesSection = RuntimeConfig.GetConfig(Context).Pages;

                    if (pagesSection != null) {
                        AsyncTimeout = pagesSection.AsyncTimeout;
                    }
                }

                if (!_asyncTimeoutSet) {
                    AsyncTimeout = TimeSpan.FromSeconds((double)Page.DefaultAsyncTimeoutSeconds);
                }
            }

            return _asyncTimeout;
        }
    }


    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected int TransactionMode {
        set { _transactionMode = value; }
        get { return _transactionMode; }
    }

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected bool AspCompatMode {
        set { _aspCompatMode = value; }
        get { return _aspCompatMode; }
    }

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected bool AsyncMode {
        set { _asyncMode = value; }
        get { return _asyncMode; }
    }

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool TraceEnabled {
        set { Trace.IsEnabled = value; }
        get { return Trace.IsEnabled; }
    }

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public System.Web.TraceMode TraceModeValue {
        set { Trace.TraceMode = value; }
        get { return Trace.TraceMode; }
    }


    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool EnableViewStateMac {
        get { return _enableViewStateMac; }
        set {
            // DevDiv #461378: EnableViewStateMac=false can lead to remote code execution, so we
            // have an mechanism that forces this to keep its default value of 'true'. We only
            // allow actually setting the value if this enforcement mechanism is inactive.
            if (!EnableViewStateMacRegistryHelper.EnforceViewStateMac) {
                _enableViewStateMac = value;
            }
        }
    }

    internal const bool SmartNavigationDefault = false;

    /// <devdoc>
    ///    <para>Is the SmartNavigation feature in use</para>
    /// </devdoc>
    [
    Browsable(false),
    Filterable(false)
    ]
    [Obsolete("The recommended alternative is Page.SetFocus and Page.MaintainScrollPositionOnPostBack. http://go.microsoft.com/fwlink/?linkid=14202")]
    public bool SmartNavigation {
        get {
            // If it's not supported or asked for, return false
            if (_smartNavSupport == SmartNavigationSupport.NotDesiredOrSupported)
                return false;

            // Otherwise, determine what the browser supports
            if (_smartNavSupport == SmartNavigationSupport.Desired) {
                // *** We need to check the current context here since
                //     we check SmartNavigation when the context == null.
                HttpContext currentContext = HttpContext.Current;

                // Make sure that there is a current context
                if (currentContext == null) {
                    // If there isn't one, assume SmartNavigation is off
                    return false;
                }

                // *** We CANNOT just check Request.Browser since Request will be null and throw an exception
                HttpBrowserCapabilities browser = currentContext.Request.Browser;

                // If it's not IE6+ on Windows, we don't support Smart Navigation
                if (!String.Equals(browser.Browser, "ie", StringComparison.OrdinalIgnoreCase) || browser.MajorVersion < 6 ||
                    !browser.Win32) {
                    _smartNavSupport = SmartNavigationSupport.NotDesiredOrSupported;
                }
                else
                    _smartNavSupport = SmartNavigationSupport.IE6OrNewer;
            }

            return (_smartNavSupport != SmartNavigationSupport.NotDesiredOrSupported);
        }
        set {
            if (value)
                _smartNavSupport = SmartNavigationSupport.Desired;
            else
                _smartNavSupport = SmartNavigationSupport.NotDesiredOrSupported;
        }
    }
    internal bool IsTransacted { get { return (_transactionMode != 0 /*TransactionOption.Disabled*/); } }
    internal bool IsInAspCompatMode { get { return _aspCompatMode; } }


    public bool IsAsync {
        get { return _asyncMode; }
    }


    /// <devdoc>
    /// Occurs when the control is done handling postback data, and before PreRender.
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public event EventHandler LoadComplete {
        add {
            Events.AddHandler(EventLoadComplete, value);
        }
        remove {
            Events.RemoveHandler(EventLoadComplete, value);
        }
    }


    /// <devdoc>
    /// Raised after postback data is handled, and before PreRender.
    /// </devdoc>
    protected virtual void OnLoadComplete(EventArgs e) {
        EventHandler handler = (EventHandler)Events[EventLoadComplete];
        if (handler != null) {
            handler(this, e);
        }
    }


    /// <devdoc>
    /// Raised after PreRender is complete
    /// </devdoc>
    protected virtual void OnPreRenderComplete(EventArgs e) {
        EventHandler handler = (EventHandler)Events[EventPreRenderComplete];
        if (handler != null) {
            handler(this, e);
        }
    }

    /// <devdoc>
    /// Heres where all the 'final' page-level framework stuff happens.
    /// Raise the PreRenderComplete event giving the user the first (and last)
    /// chance to do certain things that affect page behavior, and
    /// then perform the processing steps.
    /// </devdoc>
    private void PerformPreRenderComplete() {
        OnPreRenderComplete(EventArgs.Empty);
    }


    /// <devdoc>
    /// Occurs before controls' OnInit
    /// </devdoc>
    public event EventHandler PreInit {
        add {
            Events.AddHandler(EventPreInit, value);
        }
        remove {
            Events.RemoveHandler(EventPreInit, value);
        }
    }


    /// <devdoc>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public event EventHandler PreLoad {
        add {
            Events.AddHandler(EventPreLoad, value);
        }
        remove {
            Events.RemoveHandler(EventPreLoad, value);
        }
    }


    /// <devdoc>
    /// Occurs after all controls have completed PreRender
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public event EventHandler PreRenderComplete {
        add {
            Events.AddHandler(EventPreRenderComplete, value);
        }
        remove {
            Events.RemoveHandler(EventPreRenderComplete, value);
        }
    }

    /// <devdoc>
    /// Override this method to apply stylesheet before building controls
    /// </devdoc>
    protected override void FrameworkInitialize() {
        base.FrameworkInitialize();

        InitializeStyleSheet();
    }

    /// <devdoc>
    /// Override this method to initialize culture properties.
    /// </devdoc>
    protected virtual void InitializeCulture() {
    }


    /// <devdoc>
    /// </devdoc>
    protected internal override void OnInit(EventArgs e) {
        base.OnInit(e);

        if (_theme != null) {
            _theme.SetStyleSheet();
        }

        if (_styleSheet != null) {
            _styleSheet.SetStyleSheet();
        }
    }


    /// <devdoc>
    /// Raised before OnInit.
    /// </devdoc>
    protected virtual void OnPreInit(EventArgs e) {
        EventHandler handler = (EventHandler)Events[EventPreInit];
        if (handler != null) {
            handler(this, e);
        }
    }

    /// <devdoc>
    /// Heres where all the 'early' page initialization happens.
    /// Raise the PreInit event giving the user the first (and last)
    /// chance to do certain things that affect page behavior, and
    /// then perform the initialization steps.
    ///
    /// For now this early initialization includes
    /// theme loading.
    /// 



    private void PerformPreInit() {
        OnPreInit(EventArgs.Empty);

        InitializeThemes();

        ApplyMasterPage();

        _preInitWorkComplete = true;
    }

    // TAP version of the PerformPreInit routine.
    // !! IMPORTANT !!
    // If you change this method, also change PerformPreInit.
    private async Task PerformPreInitAsync() {
        using (Context.SyncContext.AllowVoidAsyncOperationsBlock()) {
            OnPreInit(EventArgs.Empty);
            await GetWaitForPreviousStepCompletionAwaitable();
        }

        InitializeThemes();

        ApplyMasterPage();

        _preInitWorkComplete = true;
    }


    /// <devdoc>
    /// Occurs when the page is done initializing, and before loading viewstate data.
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public event EventHandler InitComplete {
        add {
            Events.AddHandler(EventInitComplete, value);
        }
        remove {
            Events.RemoveHandler(EventInitComplete, value);
        }
    }


    /// <devdoc>
    /// Raised after page is initialized, and before loading viewstate.
    /// </devdoc>
    protected virtual void OnInitComplete(EventArgs e) {
        EventHandler handler = (EventHandler)Events[EventInitComplete];
        if (handler != null) {
            handler(this, e);
        }
    }


    /// <devdoc>
    /// Raises the PreLoad event
    /// </devdoc>
    protected virtual void OnPreLoad(EventArgs e) {
        EventHandler handler = (EventHandler)Events[EventPreLoad];
        if (handler != null) {
            handler(this, e);
        }
    }

    public void RegisterRequiresViewStateEncryption() {
        if (ControlState >= ControlState.PreRendered) {
            throw new InvalidOperationException(SR.GetString(SR.Too_late_for_RegisterRequiresViewStateEncryption));
        }

        _viewStateEncryptionRequested = true;
    }

    internal bool RequiresViewStateEncryptionInternal {
        get {
            return ViewStateEncryptionMode == ViewStateEncryptionMode.Always ||
                   _viewStateEncryptionRequested && ViewStateEncryptionMode == ViewStateEncryptionMode.Auto;
        }
    }


    /// <devdoc>
    /// Occurs when the page has completed saving view state and control state.
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public event EventHandler SaveStateComplete {
        add {
            Events.AddHandler(EventSaveStateComplete, value);
        }
        remove {
            Events.RemoveHandler(EventSaveStateComplete, value);
        }
    }


    /// <devdoc>
    /// Raises the SaveStateComplete event
    /// </devdoc>
    protected virtual void OnSaveStateComplete(EventArgs e) {
        EventHandler handler = (EventHandler)Events[EventSaveStateComplete];
        if (handler != null) {
            handler(this, e);
        }
    }


    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public virtual void ProcessRequest(HttpContext context) {
        // If running in non-full trust, call PermitOnly to cause the following code
        // to run as if there were user code on the stack.

        // Check if we're running in non-full trust
        if (HttpRuntime.NamedPermissionSet != null && !HttpRuntime.DisableProcessRequestInApplicationTrust) {

            // Are we supposed to execute process request without full trust?
            if (HttpRuntime.ProcessRequestInApplicationTrust) {

                // If so, we don't normally need to do anything, because this ProcessRequest method
                // is being called from an override in the generated code (so there is user code
                // on the stack).

                // However, if the page is no-compile, there won't be any user code on the stack,
                // so we need to explicitely call PermitOnly to make it happen
                if (NoCompile) {
                    HttpRuntime.NamedPermissionSet.PermitOnly();
                }
            }
            else {
                // Here, we want to run the request in full trust, so the situation is reversed.
                // i.e. in the no-compile case, there is no user code on the stack, so we don't need to
                // do anything.  But in the compiled case, the ProcessRequest override is on the stack,
                // so we need to nullify it using an Assert.
                ProcessRequestWithAssert(context);
                return;
            }
        }

        ProcessRequestWithNoAssert(context);
    }

    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    private void ProcessRequestWithAssert(HttpContext context) {
        ProcessRequestWithNoAssert(context);
    }

    private void ProcessRequestWithNoAssert(HttpContext context) {
        SetIntrinsics(context);
        ProcessRequest();
    }

    // assert SecurityPermission, for ASURT #112116
    [SecurityPermission(SecurityAction.Assert, ControlThread=true)]
    void SetCultureWithAssert(Thread currentThread, CultureInfo currentCulture, CultureInfo currentUICulture) {
        SetCulture(currentThread, currentCulture, currentUICulture);
    }

    void SetCulture(Thread currentThread, CultureInfo currentCulture, CultureInfo currentUICulture) {
        currentThread.CurrentCulture = currentCulture;
        currentThread.CurrentUICulture = currentUICulture;
    }

    //
    // ProcessRequestXXX methods are there because
    // transacted pages require some code (ProcessRequestMain)
    // to run inside the transaction and some outside
    //
    // Another reason - support for async pages
    //

    private void ProcessRequest() {
        // culture needs to be saved/restored only on synchronous pages (if at all)
        // save culture
        Thread currentThread = Thread.CurrentThread;
        CultureInfo prevCulture = currentThread.CurrentCulture;
        CultureInfo prevUICulture = currentThread.CurrentUICulture;

        try {
            ProcessRequest(true /*includeStagesBeforeAsyncPoint*/, true /*includeStagesAfterAsyncPoint*/);
        }
        finally {
            // restore culture
            RestoreCultures(currentThread, prevCulture, prevUICulture);
        }
    }

    // !! IMPORTANT !!
    // If you change this method, also change ProcessRequestAsync(bool, bool).
    private void ProcessRequest(bool includeStagesBeforeAsyncPoint, bool includeStagesAfterAsyncPoint) {
        // Initialize the object and build the tree of controls.
        // This must happen *after* the intrinsics have been set.
        // On async pages only call Initialize once (ProcessRequest is called twice)
        if (includeStagesBeforeAsyncPoint) {
            FrameworkInitialize();

            this.ControlState = ControlState.FrameworkInitialized;
        }

        bool needToCallEndTrace = Context.WorkerRequest is IIS7WorkerRequest;
        try {
            try {
                if (IsTransacted) {
                    ProcessRequestTransacted();
                }
                else {
                    // No transactions
                    ProcessRequestMain(includeStagesBeforeAsyncPoint, includeStagesAfterAsyncPoint);
                }

                if (includeStagesAfterAsyncPoint) {
                    needToCallEndTrace = false;
                    ProcessRequestEndTrace();
                }
            }
            catch (ThreadAbortException) {
                try {
                    if (needToCallEndTrace)
                        ProcessRequestEndTrace();
                } catch {}
            }
            finally {
                if (includeStagesAfterAsyncPoint) {
                    ProcessRequestCleanup();
                }
            }
        }
        catch { throw; }    // Prevent Exception Filter Security Issue (ASURT 122835)
    }

    // TAP version of ProcessRequest(bool, bool)
    // !! IMPORTANT !!
    // If you change this method, also change ProcessRequest(bool, bool).
    private async Task ProcessRequestAsync(bool includeStagesBeforeAsyncPoint, bool includeStagesAfterAsyncPoint) {
        // Initialize the object and build the tree of controls.
        // This must happen *after* the intrinsics have been set.
        // On async pages only call Initialize once (ProcessRequest is called twice)
        if (includeStagesBeforeAsyncPoint) {
            FrameworkInitialize();

            this.ControlState = ControlState.FrameworkInitialized;
        }

        bool needToCallEndTrace = Context.WorkerRequest is IIS7WorkerRequest;
        try {
            try {
                if (IsTransacted) {
                    ProcessRequestTransacted();
                }
                else {
                    // No transactions
                    await ProcessRequestMainAsync(includeStagesBeforeAsyncPoint, includeStagesAfterAsyncPoint).WithinCancellableCallback(Context);
                }

                if (includeStagesAfterAsyncPoint) {
                    needToCallEndTrace = false;
                    ProcessRequestEndTrace();
                }
            }
            catch (ThreadAbortException) {
                try {
                    if (needToCallEndTrace)
                        ProcessRequestEndTrace();
                } catch {}
            }
            finally {
                if (includeStagesAfterAsyncPoint) {
                    ProcessRequestCleanup();
                }
            }
        }
        catch { throw; }    // Prevent Exception Filter Security Issue (ASURT 122835)
    }

    private void RestoreCultures(Thread currentThread, CultureInfo prevCulture, CultureInfo prevUICulture) {
        if (prevCulture != currentThread.CurrentCulture || prevUICulture != currentThread.CurrentUICulture) {
            if (HttpRuntime.IsFullTrust) {
                SetCulture(currentThread, prevCulture, prevUICulture);
            }
            else {
                SetCultureWithAssert(currentThread, prevCulture, prevUICulture);
            }
        }
    }

    // This must be in its own method to avoid jitting System.EnterpriseServices.dll
    // when it is not needed (ASURT 71868)
    private void ProcessRequestTransacted() {

        bool transactionAborted = false;
        TransactedCallback processRequestCallback = new TransactedCallback(ProcessRequestMain);

        // Part of the request needs to be done under transacted context
        Transactions.InvokeTransacted(processRequestCallback,
            (TransactionOption) _transactionMode, ref transactionAborted);

        // The remainder has to be done outside
        try {
            if (transactionAborted) {
                OnAbortTransaction(EventArgs.Empty);
                WebBaseEvent.RaiseSystemEvent(this, WebEventCodes.RequestTransactionAbort);
            }
            else {
                OnCommitTransaction(EventArgs.Empty);
                WebBaseEvent.RaiseSystemEvent(this, WebEventCodes.RequestTransactionComplete);
            }

            // Make sure Request.RawUrl gets validated.
            ValidateRawUrlIfRequired();
        }
        catch (ThreadAbortException) {
            // Don't go into HandleError logic for ThreadAbortException's, since they
            // are expected (e.g. when Response.Redirect() is called).
            throw;
        }
        catch (Exception e) {
            // Increment all of the appropriate error counters
            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_DURING_REQUEST);
            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_TOTAL);

            // If it hasn't been handled, rethrow it
            if (!HandleError(e))
                throw;
        }
    }

    private void ProcessRequestCleanup() {
        if (_request == null) {
            // ProcessRequestCleanup() has already been called
            return;
        }

#if DISPLAYRAREFIELDSTATISTICS
        // Display rare field statistics at the end of the page (for debugging purpose)
        DisplayRareFieldStatistics();
#endif

        _request = null;
        _response = null;

        if (!IsCrossPagePostBack) {
            UnloadRecursive(true);
        }

        if (Context.TraceIsEnabled) {
            Trace.StopTracing();
        }
    }

    private void ProcessRequestEndTrace() {

        if (Context.TraceIsEnabled) {
            Trace.EndRequest();

            // DevDiv Bugs 154103: Do not write trace output while in an async postback
            if (Trace.PageOutput && !IsCallback &&
                 (ScriptManager == null || !ScriptManager.IsInAsyncPostBack)) {
                Trace.Render(CreateHtmlTextWriter(Response.Output));

                // responses with trace should not be cached
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
            }
        }
    }

#if DEBUG
    private void DisplayRareFieldStatistics() {
        int totalControls = 0;
        int withOccasionalFields = 0;
        int withRareFields = 0;
        GetRareFieldStatistics(ref totalControls, ref withOccasionalFields, ref withRareFields);
        _response.Write("<hr><b><p>Total controls: " + totalControls + "<br>");
        _response.Write("With Occasional Fields: " + withOccasionalFields + "<br>");
        _response.Write("With Rare Fields: " + withRareFields + "</p></b>");
    }
#endif

    internal void SetPreviousPage(Page previousPage) {
        _previousPage = previousPage;
    }


    private void ProcessRequestMain() {
        ProcessRequestMain(true /*includeStagesBeforeAsyncPoint*/, true /*includeStagesAfterAsyncPoint*/);
    }

    // !! IMPORTANT !!
    // If you make changes to this method, also make changes to ProcessRequestMainAsync.
    private void ProcessRequestMain(bool includeStagesBeforeAsyncPoint, bool includeStagesAfterAsyncPoint) {
        try {
            HttpContext con = Context;

            string exportedWebPartID = null;
            if (includeStagesBeforeAsyncPoint) {
                // For ASPCOMPAT need to call OnPageStart for each Session object

#if !FEATURE_PAL // FEATURE_PAL does not enable COM
                if (IsInAspCompatMode)
                    AspCompatApplicationStep.OnPageStartSessionObjects();
#else // !FEATURE_PAL
				throw new NotImplementedException ("ROTORTODO");
#endif // !FEATURE_PAL

                // Is it a GET, POST or initial request?
                if(PageAdapter != null) {
                    _requestValueCollection = PageAdapter.DeterminePostBackMode();
                    if (_requestValueCollection != null) {
                        _unvalidatedRequestValueCollection = PageAdapter.DeterminePostBackModeUnvalidated();
                    }
                }
                else {
                    _requestValueCollection = DeterminePostBackMode();
                    // The contract for DeterminePostBackModeUnvalidated() is that it will only be called when
                    // DeterminePostBackMode() returns a non-null result. This was done so that the implementation
                    // of DeterminePostBackModeUnvalidated() can be kep simple, without having to duplicate the
                    // same logic as DeterminePostBackMode().
                    if (_requestValueCollection != null) {
                        _unvalidatedRequestValueCollection = DeterminePostBackModeUnvalidated();
                    }
                }
                // It's possible that someone incorrectly implements DeterminePostBackModeUnvalidated() such that it
                // returns null when DeterminePostBackMode() a non-null value. This could cause NullRefExceptions later on.
                // However since few customers would override these methods we assume that this won't happen very often.
                // A customer overriding DeterminePostBackModeUnvalidated() should understand what they are doing.

                string callbackControlId = String.Empty;

                // Special-case Web Part Export so it executes in the same security context as the page itself (VSWhidbey 426574)
                if (DetermineIsExportingWebPart()) {
                    if (!RuntimeConfig.GetAppConfig().WebParts.EnableExport) {
                        throw new InvalidOperationException(SR.GetString(SR.WebPartExportHandler_DisabledExportHandler));
                    }

                    exportedWebPartID = Request.QueryString["webPart"];
                    if (String.IsNullOrEmpty(exportedWebPartID)) {
                        throw new InvalidOperationException(SR.GetString(SR.WebPartExportHandler_InvalidArgument));
                    }

                    if (String.Equals(Request.QueryString["scope"], "shared", StringComparison.OrdinalIgnoreCase)) {
                        _pageFlags.Set(isExportingWebPartShared);
                    }

                    string queryString = Request.QueryString["query"];
                    if (queryString == null) {
                        queryString = String.Empty;
                    }
                    Request.QueryStringText = queryString;
                    con.Trace.IsEnabled = false;
                }

                if (_requestValueCollection != null) {

                    // Determine if viewstate was encrypted.
                    if (_requestValueCollection[ViewStateEncryptionID] != null) {
                        ContainsEncryptedViewState = true;
                    }

                    // Determine if this is a callback.
                    callbackControlId = _requestValueCollection[callbackID];
                    // Only accepting POST callbacks to reduce mail attack possibilities (VSWhidbey 417355)
                    if ((callbackControlId != null) && (_request.HttpVerb == HttpVerb.POST)) {
                        _isCallback = true;
                    }
                    else { // Otherwise, determine if this is cross-page posting(callsbacks can never be cross page posts)
                        if (!IsCrossPagePostBack) {
                            VirtualPath previousPagePath = null;

                            if (_requestValueCollection[previousPageID] != null) {
                                try {
                                    previousPagePath = VirtualPath.CreateNonRelativeAllowNull(
                                        DecryptString(_requestValueCollection[previousPageID], Purpose.WebForms_Page_PreviousPageID));
                                }
                                catch {
                                    // VSWhidbey 493209 If we fails to decrypt the previouspageid, still
                                    // treat this as a cross page post, not a regular postback. Otherwise
                                    // the viewstate cannot be decrypted properly. This will happen during
                                    // cross page post between different applications.
                                    _pageFlags[isCrossPagePostRequest] = true;

                                    // do nothing, ignore CryptographicException.
                                }

                                // Process if the page is posted from cross-page that still exists and the target page is not same as source page.
                                if (previousPagePath != null &&
                                    previousPagePath != Request.CurrentExecutionFilePathObject) {
                                    _pageFlags[isCrossPagePostRequest] = true;
                                    _previousPagePath = previousPagePath;
                                    Debug.Assert(_previousPagePath != null);
                                }
                            }
                        }
                    }
                }

                // Load the scroll position data now that we have the request value collection
                if (MaintainScrollPositionOnPostBack) {
                    LoadScrollPosition();
                }

                // we can't cache the value of IsEnabled because it could change during any phase.
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin PreInit");
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_PRE_INIT_ENTER, _context.WorkerRequest);
                PerformPreInit();
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_PRE_INIT_LEAVE, _context.WorkerRequest);
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End PreInit");

                if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin Init");
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_INIT_ENTER, _context.WorkerRequest);
                InitRecursive(null);
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_INIT_LEAVE, _context.WorkerRequest);
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End Init");

                if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin InitComplete");
                OnInitComplete(EventArgs.Empty);
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End InitComplete");

                if (IsPostBack) {
                    if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin LoadState");
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_VIEWSTATE_ENTER, _context.WorkerRequest);
                    LoadAllState();
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_VIEWSTATE_LEAVE, _context.WorkerRequest);
                    if (con.TraceIsEnabled) {
                        Trace.Write("aspx.page", "End LoadState");
                        Trace.Write("aspx.page", "Begin ProcessPostData");
                    }

                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_POSTDATA_ENTER, _context.WorkerRequest);
                    ProcessPostData(_requestValueCollection, true /* fBeforeLoad */);
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_POSTDATA_LEAVE, _context.WorkerRequest);
                    if (con.TraceIsEnabled) Trace.Write("aspx.page", "End ProcessPostData");
                }

                if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin PreLoad");
                OnPreLoad(EventArgs.Empty);
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End PreLoad");

                if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin Load");
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_ENTER, _context.WorkerRequest);
                LoadRecursive();
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_LEAVE, _context.WorkerRequest);
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End Load");

                if (IsPostBack) {
                    // Try process the post data again (ASURT 29045)
                    if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin ProcessPostData Second Try");
                    ProcessPostData(_leftoverPostData, false /* !fBeforeLoad */);
                    if (con.TraceIsEnabled) {
                        Trace.Write("aspx.page", "End ProcessPostData Second Try");
                        Trace.Write("aspx.page", "Begin Raise ChangedEvents");
                    }

                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_POST_DATA_CHANGED_ENTER, _context.WorkerRequest);
                    RaiseChangedEvents();
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_POST_DATA_CHANGED_LEAVE, _context.WorkerRequest);
                    if (con.TraceIsEnabled) {
                        Trace.Write("aspx.page", "End Raise ChangedEvents");
                        Trace.Write("aspx.page", "Begin Raise PostBackEvent");
                    }
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_RAISE_POSTBACK_ENTER, _context.WorkerRequest);
                    RaisePostBackEvent(_requestValueCollection);
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_RAISE_POSTBACK_LEAVE, _context.WorkerRequest);
                    if (con.TraceIsEnabled) Trace.Write("aspx.page", "End Raise PostBackEvent");
                }

                if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin LoadComplete");
                OnLoadComplete(EventArgs.Empty);
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End LoadComplete");

                if (IsPostBack && IsCallback) {
                    PrepareCallback(callbackControlId);
                }
                else if (!IsCrossPagePostBack) {
                    if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin PreRender");
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_PRE_RENDER_ENTER, _context.WorkerRequest);
                    PreRenderRecursiveInternal();
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_PRE_RENDER_LEAVE, _context.WorkerRequest);
                    if (con.TraceIsEnabled) Trace.Write("aspx.page", "End PreRender");
                }
            }

            /// Async Point here

            if (_legacyAsyncInfo == null || _legacyAsyncInfo.CallerIsBlocking) {
                // for non-async pages with registered async tasks - run the tasks here
                // also when running async page via server.execute - run the tasks here
                ExecuteRegisteredAsyncTasks();
            }

            // Make sure RawUrl gets validated.
            ValidateRawUrlIfRequired();

            if (includeStagesAfterAsyncPoint) {
                if (IsCallback) {
                    RenderCallback();
                    return;
                }

                if (IsCrossPagePostBack) {
                    return;
                }

                if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin PreRenderComplete");
                PerformPreRenderComplete();
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End PreRenderComplete");

                if (con.TraceIsEnabled) {
                    BuildPageProfileTree(EnableViewState);
                    Trace.Write("aspx.page", "Begin SaveState");
                }

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_SAVE_VIEWSTATE_ENTER, _context.WorkerRequest);

                SaveAllState();

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_SAVE_VIEWSTATE_LEAVE, _context.WorkerRequest);
                if (con.TraceIsEnabled) {
                    Trace.Write("aspx.page", "End SaveState");
                    Trace.Write("aspx.page", "Begin SaveStateComplete");
                }
                OnSaveStateComplete(EventArgs.Empty);
                if (con.TraceIsEnabled) {
                    Trace.Write("aspx.page", "End SaveStateComplete");
                    Trace.Write("aspx.page", "Begin Render");
                }

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_RENDER_ENTER, _context.WorkerRequest);
                // Special-case Web Part Export so it executes in the same security context as the page itself (VSWhidbey 426574)
                if (exportedWebPartID != null) {
                    ExportWebPart(exportedWebPartID);
                }
                else {
                    RenderControl(CreateHtmlTextWriter(Response.Output));
                }

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_RENDER_LEAVE, _context.WorkerRequest);

                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End Render");

                CheckRemainingAsyncTasks(false);
            }
        }
        catch (ThreadAbortException e) {
            // Don't go into HandleError logic for ThreadAbortExceptions, since they
            // are expected (e.g. when Response.Redirect() is called).

            // VSWhidbey 500309: perf improvement. We can safely cancel the thread abort here
            // to avoid re-throwing the exception if this is a redirect and we're not being executed
            // under the context of a Server.Execute call (i.e. _context.Handler == this).  Otherwise,
            // re-throw so this can be handled lower in the stack (see HttpApplication.ExecuteStep).

            // This perf optimization can only be applied if we are executing the entire page
            // lifecycle within this method call (otherwise, in async pages) calling ResetAbort
            // would only skip part of the lifecycle, not the entire page (as Response.End is supposed to)

            HttpApplication.CancelModuleException cancelException = e.ExceptionState as HttpApplication.CancelModuleException;
            if (includeStagesBeforeAsyncPoint && includeStagesAfterAsyncPoint &&    // executing entire page
                _context.Handler == this &&                                         // not in server execute
                _context.ApplicationInstance != null &&                             // application must be non-null so we can complete the request
                cancelException != null && !cancelException.Timeout) {              // this is Response.End
                _context.ApplicationInstance.CompleteRequest();
                ThreadResetAbortWithAssert();
            }
            else {
                CheckRemainingAsyncTasks(true);
                throw;
            }
        }
        catch (System.Configuration.ConfigurationException) {
            throw;
        }
        catch (Exception e) {
            // Increment all of the appropriate error counters
            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_DURING_REQUEST);
            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_TOTAL);

            // If it hasn't been handled, rethrow it
            if (!HandleError(e))
                throw;
        }
    }

    // TAP version of ProcessRequestMain routine.
    // !! IMPORTANT !!
    // If you make changes to this method, also make changes to ProcessRequestMain.
    private async Task ProcessRequestMainAsync(bool includeStagesBeforeAsyncPoint, bool includeStagesAfterAsyncPoint) {
        try {
            HttpContext con = Context;

            string exportedWebPartID = null;
            if (includeStagesBeforeAsyncPoint) {
                // For ASPCOMPAT need to call OnPageStart for each Session object

#if !FEATURE_PAL // FEATURE_PAL does not enable COM
                if (IsInAspCompatMode)
                    AspCompatApplicationStep.OnPageStartSessionObjects();
#else // !FEATURE_PAL
				throw new NotImplementedException ("ROTORTODO");
#endif // !FEATURE_PAL

                // Is it a GET, POST or initial request?
                if(PageAdapter != null) {
                    _requestValueCollection = PageAdapter.DeterminePostBackMode();
                    if (_requestValueCollection != null) {
                        _unvalidatedRequestValueCollection = PageAdapter.DeterminePostBackModeUnvalidated();
                    }
                }
                else {
                    _requestValueCollection = DeterminePostBackMode();
                    // The contract for DeterminePostBackModeUnvalidated() is that it will only be called when
                    // DeterminePostBackMode() returns a non-null result. This was done so that the implementation
                    // of DeterminePostBackModeUnvalidated() can be kep simple, without having to duplicate the
                    // same logic as DeterminePostBackMode().
                    if (_requestValueCollection != null) {
                        _unvalidatedRequestValueCollection = DeterminePostBackModeUnvalidated();
                    }
                }
                // It's possible that someone incorrectly implements DeterminePostBackModeUnvalidated() such that it
                // returns null when DeterminePostBackMode() a non-null value. This could cause NullRefExceptions later on.
                // However since few customers would override these methods we assume that this won't happen very often.
                // A customer overriding DeterminePostBackModeUnvalidated() should understand what they are doing.

                string callbackControlId = String.Empty;

                // Special-case Web Part Export so it executes in the same security context as the page itself (VSWhidbey 426574)
                if (DetermineIsExportingWebPart()) {
                    if (!RuntimeConfig.GetAppConfig().WebParts.EnableExport) {
                        throw new InvalidOperationException(SR.GetString(SR.WebPartExportHandler_DisabledExportHandler));
                    }

                    exportedWebPartID = Request.QueryString["webPart"];
                    if (String.IsNullOrEmpty(exportedWebPartID)) {
                        throw new InvalidOperationException(SR.GetString(SR.WebPartExportHandler_InvalidArgument));
                    }

                    if (String.Equals(Request.QueryString["scope"], "shared", StringComparison.OrdinalIgnoreCase)) {
                        _pageFlags.Set(isExportingWebPartShared);
                    }

                    string queryString = Request.QueryString["query"];
                    if (queryString == null) {
                        queryString = String.Empty;
                    }
                    Request.QueryStringText = queryString;
                    con.Trace.IsEnabled = false;
                }

                if (_requestValueCollection != null) {

                    // Determine if viewstate was encrypted.
                    if (_requestValueCollection[ViewStateEncryptionID] != null) {
                        ContainsEncryptedViewState = true;
                    }

                    // Determine if this is a callback.
                    callbackControlId = _requestValueCollection[callbackID];
                    // Only accepting POST callbacks to reduce mail attack possibilities (VSWhidbey 417355)
                    if ((callbackControlId != null) && (_request.HttpVerb == HttpVerb.POST)) {
                        _isCallback = true;
                    }
                    else { // Otherwise, determine if this is cross-page posting(callsbacks can never be cross page posts)
                        if (!IsCrossPagePostBack) {
                            VirtualPath previousPagePath = null;

                            if (_requestValueCollection[previousPageID] != null) {
                                try {
                                    previousPagePath = VirtualPath.CreateNonRelativeAllowNull(
                                        DecryptString(_requestValueCollection[previousPageID], Purpose.WebForms_Page_PreviousPageID));
                                }
                                catch {
                                    // VSWhidbey 493209 If we fails to decrypt the previouspageid, still
                                    // treat this as a cross page post, not a regular postback. Otherwise
                                    // the viewstate cannot be decrypted properly. This will happen during
                                    // cross page post between different applications.
                                    _pageFlags[isCrossPagePostRequest] = true;

                                    // do nothing, ignore CryptographicException.
                                }

                                // Process if the page is posted from cross-page that still exists and the target page is not same as source page.
                                if (previousPagePath != null &&
                                    previousPagePath != Request.CurrentExecutionFilePathObject) {
                                    _pageFlags[isCrossPagePostRequest] = true;
                                    _previousPagePath = previousPagePath;
                                    Debug.Assert(_previousPagePath != null);
                                }
                            }
                        }
                    }
                }

                // Load the scroll position data now that we have the request value collection
                if (MaintainScrollPositionOnPostBack) {
                    LoadScrollPosition();
                }

                // we can't cache the value of IsEnabled because it could change during any phase.
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin PreInit");
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_PRE_INIT_ENTER, _context.WorkerRequest);
                await PerformPreInitAsync().WithinCancellableCallback(con);
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_PRE_INIT_LEAVE, _context.WorkerRequest);
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End PreInit");

                if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin Init");
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_INIT_ENTER, _context.WorkerRequest);
                Task initRecursiveTask = InitRecursiveAsync(null, this);
                await initRecursiveTask.WithinCancellableCallback(con);
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_INIT_LEAVE, _context.WorkerRequest);
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End Init");

                if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin InitComplete");
                using (con.SyncContext.AllowVoidAsyncOperationsBlock()) {
                    OnInitComplete(EventArgs.Empty);
                    await GetWaitForPreviousStepCompletionAwaitable();
                }
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End InitComplete");

                if (IsPostBack) {
                    if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin LoadState");
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_VIEWSTATE_ENTER, _context.WorkerRequest);
                    LoadAllState();
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_VIEWSTATE_LEAVE, _context.WorkerRequest);
                    if (con.TraceIsEnabled) {
                        Trace.Write("aspx.page", "End LoadState");
                        Trace.Write("aspx.page", "Begin ProcessPostData");
                    }

                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_POSTDATA_ENTER, _context.WorkerRequest);
                    ProcessPostData(_requestValueCollection, true /* fBeforeLoad */);
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_POSTDATA_LEAVE, _context.WorkerRequest);
                    if (con.TraceIsEnabled) Trace.Write("aspx.page", "End ProcessPostData");
                }

                if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin PreLoad");
                using (con.SyncContext.AllowVoidAsyncOperationsBlock()) {
                    OnPreLoad(EventArgs.Empty);
                    await GetWaitForPreviousStepCompletionAwaitable();
                }
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End PreLoad");

                if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin Load");
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_ENTER, _context.WorkerRequest);
                await LoadRecursiveAsync(this).WithinCancellableCallback(con);
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_LOAD_LEAVE, _context.WorkerRequest);
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End Load");

                if (IsPostBack) {
                    // Try process the post data again (ASURT 29045)
                    if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin ProcessPostData Second Try");
                    ProcessPostData(_leftoverPostData, false /* !fBeforeLoad */);
                    if (con.TraceIsEnabled) {
                        Trace.Write("aspx.page", "End ProcessPostData Second Try");
                        Trace.Write("aspx.page", "Begin Raise ChangedEvents");
                    }

                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_POST_DATA_CHANGED_ENTER, _context.WorkerRequest);
                    await RaiseChangedEventsAsync().WithinCancellableCallback(con);
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_POST_DATA_CHANGED_LEAVE, _context.WorkerRequest);
                    if (con.TraceIsEnabled) {
                        Trace.Write("aspx.page", "End Raise ChangedEvents");
                        Trace.Write("aspx.page", "Begin Raise PostBackEvent");
                    }
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_RAISE_POSTBACK_ENTER, _context.WorkerRequest);
                    using (con.SyncContext.AllowVoidAsyncOperationsBlock()) {
                        RaisePostBackEvent(_requestValueCollection);
                        await GetWaitForPreviousStepCompletionAwaitable();
                    }
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_RAISE_POSTBACK_LEAVE, _context.WorkerRequest);
                    if (con.TraceIsEnabled) Trace.Write("aspx.page", "End Raise PostBackEvent");
                }

                if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin LoadComplete");
                using (con.SyncContext.AllowVoidAsyncOperationsBlock()) {
                    OnLoadComplete(EventArgs.Empty);
                    await GetWaitForPreviousStepCompletionAwaitable();
                }
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End LoadComplete");

                if (IsPostBack && IsCallback) {
                    await PrepareCallbackAsync(callbackControlId).WithinCancellableCallback(con);
                }
                else if (!IsCrossPagePostBack) {
                    if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin PreRender");
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_PRE_RENDER_ENTER, _context.WorkerRequest);
                    await PreRenderRecursiveInternalAsync(this).WithinCancellableCallback(con);
                    if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_PRE_RENDER_LEAVE, _context.WorkerRequest);
                    if (con.TraceIsEnabled) Trace.Write("aspx.page", "End PreRender");
                }
            }

            /// Async Point here

            if (_legacyAsyncInfo == null || _legacyAsyncInfo.CallerIsBlocking) {
                // for non-async pages with registered async tasks - run the tasks here
                // also when running async page via server.execute - run the tasks here
                ExecuteRegisteredAsyncTasks();
            }

            // Make sure RawUrl gets validated.
            ValidateRawUrlIfRequired();

            if (includeStagesAfterAsyncPoint) {
                if (IsCallback) {
                    RenderCallback();
                    return;
                }

                if (IsCrossPagePostBack) {
                    return;
                }

                if (con.TraceIsEnabled) Trace.Write("aspx.page", "Begin PreRenderComplete");
                PerformPreRenderComplete();
                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End PreRenderComplete");

                if (con.TraceIsEnabled) {
                    BuildPageProfileTree(EnableViewState);
                    Trace.Write("aspx.page", "Begin SaveState");
                }

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_SAVE_VIEWSTATE_ENTER, _context.WorkerRequest);

                SaveAllState();

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_SAVE_VIEWSTATE_LEAVE, _context.WorkerRequest);
                if (con.TraceIsEnabled) {
                    Trace.Write("aspx.page", "End SaveState");
                    Trace.Write("aspx.page", "Begin SaveStateComplete");
                }
                OnSaveStateComplete(EventArgs.Empty);
                if (con.TraceIsEnabled) {
                    Trace.Write("aspx.page", "End SaveStateComplete");
                    Trace.Write("aspx.page", "Begin Render");
                }

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_RENDER_ENTER, _context.WorkerRequest);
                // Special-case Web Part Export so it executes in the same security context as the page itself (VSWhidbey 426574)
                if (exportedWebPartID != null) {
                    ExportWebPart(exportedWebPartID);
                }
                else {
                    RenderControl(CreateHtmlTextWriter(Response.Output));
                }

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Page)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PAGE_RENDER_LEAVE, _context.WorkerRequest);

                if (con.TraceIsEnabled) Trace.Write("aspx.page", "End Render");

                CheckRemainingAsyncTasks(false);
            }
        }
        catch (ThreadAbortException e) {
            // Don't go into HandleError logic for ThreadAbortExceptions, since they
            // are expected (e.g. when Response.Redirect() is called).

            // VSWhidbey 500309: perf improvement. We can safely cancel the thread abort here
            // to avoid re-throwing the exception if this is a redirect and we're not being executed
            // under the context of a Server.Execute call (i.e. _context.Handler == this).  Otherwise,
            // re-throw so this can be handled lower in the stack (see HttpApplication.ExecuteStep).

            // This perf optimization can only be applied if we are executing the entire page
            // lifecycle within this method call (otherwise, in async pages) calling ResetAbort
            // would only skip part of the lifecycle, not the entire page (as Response.End is supposed to)

            HttpApplication.CancelModuleException cancelException = e.ExceptionState as HttpApplication.CancelModuleException;
            if (includeStagesBeforeAsyncPoint && includeStagesAfterAsyncPoint &&    // executing entire page
                _context.Handler == this &&                                         // not in server execute
                _context.ApplicationInstance != null &&                             // application must be non-null so we can complete the request
                cancelException != null && !cancelException.Timeout) {              // this is Response.End
                _context.ApplicationInstance.CompleteRequest();
                ThreadResetAbortWithAssert();
            }
            else {
                CheckRemainingAsyncTasks(true);
                throw;
            }
        }
        catch (System.Configuration.ConfigurationException) {
            throw;
        }
        catch (Exception e) {
            // Increment all of the appropriate error counters
            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_DURING_REQUEST);
            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_TOTAL);

            // If it hasn't been handled, rethrow it
            if (!HandleError(e))
                throw;
        }
    }

    internal WithinCancellableCallbackTaskAwaitable GetWaitForPreviousStepCompletionAwaitable() {
        AspNetSynchronizationContext syncContext = SynchronizationContext.Current as AspNetSynchronizationContext;
        if (syncContext != null) {
            return syncContext.WaitForPendingOperationsAsync().WithinCancellableCallback(Context);
        }
        else {
            // If the SynchronizationContext has been replaced, we can't query for previous step completion, so just assume it completed.
            return WithinCancellableCallbackTaskAwaitable.Completed;
        }
    }

    private void BuildPageProfileTree(bool enableViewState) {
        if (!_profileTreeBuilt) {
            _profileTreeBuilt = true;
            BuildProfileTree("ROOT", enableViewState);
        }
    }

    private void ExportWebPart(string exportedWebPartID) {
        WebPart webPartToExport = null;

        WebPartManager webPartManager = WebPartManager.GetCurrentWebPartManager(this);
        if (webPartManager != null) {
            webPartToExport = webPartManager.WebParts[exportedWebPartID];
        }

        if (webPartToExport == null || webPartToExport.IsClosed || webPartToExport is ProxyWebPart) {
            // If there is no WebPartManager, or the web part is not on the page, or has been replaced
            // by a ProxyWebPart, we should perform a Response.Redirect() back to the page with the
            // Export query string removed. The Export query string has already been removed in
            // ProcessRequestMain().
            // (VSWhidbey 358464, 496050, 504819, 515472)
            Response.Redirect(Request.RawUrl, false);
        }
        else {
            // We'll be writing Xml to the response -> Prepare the response
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Expires = 0;
            Response.ContentType = "application/mswebpart";
            string title = webPartToExport.DisplayTitle;
            if (String.IsNullOrEmpty(title)) {
                title = SR.GetString(SR.Part_Untitled);
            }
            NonWordRegex nonWordRegex = new NonWordRegex();
            Response.AddHeader("content-disposition", "attachment; filename=" +
                                nonWordRegex.Replace(title, "") +
                                ".WebPart");
            using (XmlTextWriter writer = new XmlTextWriter(Response.Output)) {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                // Export to the response stream
                webPartManager.ExportWebPart(webPartToExport, writer);
                writer.WriteEndDocument();
            }
        }
    }

    private void InitializeWriter(HtmlTextWriter writer) {
        Html32TextWriter h32tw = writer as Html32TextWriter;
        if (h32tw != null && Request.Browser != null) {
            h32tw.ShouldPerformDivTableSubstitution = Request.Browser.Tables;
        }
    }


    protected internal override void Render(HtmlTextWriter writer) {
        InitializeWriter(writer);

        base.Render(writer);
    }

    // !! IMPORTANT !!
    // If you change this method, also change PrepareCallbackAsync.
    private void PrepareCallback(string callbackControlID) {
        Response.Cache.SetNoStore();
        try {
            string param = _requestValueCollection[callbackParameterID];
            _callbackControl = FindControl(callbackControlID) as ICallbackEventHandler;

            if (_callbackControl != null) {
                _callbackControl.RaiseCallbackEvent(param);
            }
            else {
                throw new InvalidOperationException(SR.GetString(SR.Page_CallBackTargetInvalid, callbackControlID));
            }
        }
        catch (Exception e) {
            Response.Clear();
            Response.Write('e');
            if (Context.IsCustomErrorEnabled) {
                Response.Write(SR.GetString(SR.Page_CallBackError));
            }
            else {
                bool needsCallbackLoadScript = !String.IsNullOrEmpty(_requestValueCollection[callbackLoadScriptID]);
                Response.Write(needsCallbackLoadScript ?
                    Util.QuoteJScriptString(HttpUtility.HtmlEncode(e.Message)) :
                    HttpUtility.HtmlEncode(e.Message));
            }
        }
        return;
    }

    // TAP version of PrepareCallback.
    // !! IMPORTANT !!
    // If you change this method, also change PrepareCallback.
    private async Task PrepareCallbackAsync(string callbackControlID) {
        Response.Cache.SetNoStore();
        try {
            string param = _requestValueCollection[callbackParameterID];
            _callbackControl = FindControl(callbackControlID) as ICallbackEventHandler;

            if (_callbackControl != null) {
                using (Context.SyncContext.AllowVoidAsyncOperationsBlock()) {
                    _callbackControl.RaiseCallbackEvent(param);
                    await GetWaitForPreviousStepCompletionAwaitable();
                }
            }
            else {
                throw new InvalidOperationException(SR.GetString(SR.Page_CallBackTargetInvalid, callbackControlID));
            }
        }
        catch (Exception e) {
            Response.Clear();
            Response.Write('e');
            if (Context.IsCustomErrorEnabled) {
                Response.Write(SR.GetString(SR.Page_CallBackError));
            }
            else {
                bool needsCallbackLoadScript = !String.IsNullOrEmpty(_requestValueCollection[callbackLoadScriptID]);
                Response.Write(needsCallbackLoadScript ?
                    Util.QuoteJScriptString(HttpUtility.HtmlEncode(e.Message)) :
                    HttpUtility.HtmlEncode(e.Message));
            }
        }
        return;
    }

    private void RenderCallback() {
        bool needsCallbackLoadScript = !String.IsNullOrEmpty(_requestValueCollection[callbackLoadScriptID]);
        try {
            string index = null;
            if (needsCallbackLoadScript) {
                index = _requestValueCollection[callbackIndexID];
                if (String.IsNullOrEmpty(index)) {
                    throw new HttpException(SR.GetString(SR.Page_CallBackInvalid));
                }
                // We validate the user string because we're injecting it into the response script.
                // We don't need the integer, so we don't call Parse, we just need to check only expected
                // characters are in the string (0 to 9)
                for (int i = 0; i < index.Length; i++) {
                    char c = index[i];
                    if (c < '0' || c > '9') {
                        throw new HttpException(SR.GetString(SR.Page_CallBackInvalid));
                    }
                }
                Response.Write("<script>parent.__pendingCallbacks[");
                Response.Write(index);
                Response.Write("].xmlRequest.responseText=\"");
            }
            if (_callbackControl != null) {
                string result = _callbackControl.GetCallbackResult();
                if (EnableEventValidation) {
                    // Outputting the new value for the validation field under the length|value format.
                    string validation = ClientScript.GetEventValidationFieldValue();
                    Response.Write(validation.Length.ToString(CultureInfo.InvariantCulture));
                    Response.Write('|');
                    Response.Write(validation);
                }
                else {
                    Response.Write('s');
                }
                Response.Write(needsCallbackLoadScript ? Util.QuoteJScriptString(result) : result);
            }

            if (needsCallbackLoadScript) {
                Response.Write("\";parent.__pendingCallbacks[");
                Response.Write(index);
                Response.Write("].xmlRequest.readyState=4;parent.WebForm_CallbackComplete();</script>");
            }
        }
        catch (Exception e) {
            Response.Clear();
            Response.Write('e');
            if (Context.IsCustomErrorEnabled) {
                Response.Write(SR.GetString(SR.Page_CallBackError));
            }
            else {
                Response.Write(needsCallbackLoadScript ?
                    Util.QuoteJScriptString(HttpUtility.HtmlEncode(e.Message)) :
                    HttpUtility.HtmlEncode(e.Message));
            }
        }
        return;
    }

    private bool RenderDivAroundHiddenInputs(HtmlTextWriter writer) {
        return writer.RenderDivAroundHiddenInputs && (!EnableLegacyRendering || (RenderingCompatibility >= VersionUtil.Framework40));
    }

    internal void SetForm(HtmlForm form) {
        _form = form;
    }

    internal void SetPostFormRenderDelegate(RenderMethod renderMethod) {
        _postFormRenderDelegate = renderMethod;
    }

    public HtmlForm Form { get { return _form; } }

    /// <devdoc>
    ///    <para>If called, ViewState will be persisted (see ASURT 73020).</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public void RegisterViewStateHandler() {
        _needToPersistViewState = true;
    }

    private void SaveAllState() {
        // Don't do anything if no one cares about the view state (see ASURT 73020)
        // Note: If _needToPersistViewState is false, control state should also be ignored.
        if (!_needToPersistViewState)
            return;

        Pair statePair = new Pair();

        // Control state is saved as a dictionary of
        // 1. A list of controls that require postback (stored under the page id)
        // 2. A dictionary of controls and their control state

        IDictionary controlStates = null;

        if (_registeredControlsRequiringControlState != null &&
            _registeredControlsRequiringControlState.Count > 0) {

#if OBJECTSTATEFORMATTER
            controlStates = new HybridDictionary(_registeredControlsRequiringControlState.Count + 1);
#else
            controlStates = new Hashtable(_registeredControlsRequiringControlState.Count + 1);
#endif

            foreach (Control ctl in _registeredControlsRequiringControlState) {
                object controlState = ctl.SaveControlStateInternal();
                // Do not allow null control states to be added, and do not allow a control's state to be
                // added more than once.
                if (controlStates[ctl.UniqueID] == null && controlState != null) {
                    controlStates.Add(ctl.UniqueID, controlState);
                }
            }
        }
        if (_registeredControlsThatRequirePostBack != null && _registeredControlsThatRequirePostBack.Count > 0) {
            if (controlStates == null) {
#if OBJECTSTATEFORMATTER
                controlStates = new HybridDictionary();
#else
                controlStates = new Hashtable();
#endif
            }
            controlStates.Add(PageRegisteredControlsThatRequirePostBackKey, _registeredControlsThatRequirePostBack);
        }

        // Only persist control state if it is nonempty.
        if (controlStates != null && controlStates.Count > 0) {
            statePair.First = controlStates;
        }

        // The state is saved as an array of objects:
        // 1. The hash code string
        // 2. The state of the entire control hierarchy
        ViewStateMode inheritedMode = ViewStateMode;
        if (inheritedMode == ViewStateMode.Inherit) {
            inheritedMode = ViewStateMode.Enabled;
        }
        Pair allSavedViewState = new Pair(GetTypeHashCode().ToString(NumberFormatInfo.InvariantInfo), SaveViewStateRecursive(inheritedMode));

        if (Context.TraceIsEnabled) {
            int viewStateSize = 0;
            if (allSavedViewState.Second is Pair) {
                viewStateSize = EstimateStateSize(((Pair)allSavedViewState.Second).First);
            } else if (allSavedViewState.Second is Triplet) {
                viewStateSize = EstimateStateSize(((Triplet)allSavedViewState.Second).First);
            }

            Trace.AddControlStateSize(UniqueID, viewStateSize, controlStates == null? 0 : EstimateStateSize(controlStates[UniqueID]));
        }

        statePair.Second = allSavedViewState;

        SavePageStateToPersistenceMedium(statePair);
    }

    /*
     * Override this method to persist view state to something other
     * than hidden fields (

*/

    /// <devdoc>
    ///    <para>Saves any view state information for the page. Override
    ///       this method if you want to save the page view state in anything other than a hidden field.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected internal virtual void SavePageStateToPersistenceMedium(object state) {
        PageStatePersister persister = PageStatePersister;
        if (state is Pair) {
            Pair pair = (Pair)state;

            persister.ControlState = pair.First;
            persister.ViewState = pair.Second;
        }
        else /* triplet, legacy case, see VSWhidbey 155185 */ {
            persister.ViewState = state;
        }
        persister.Save();
    }

    /*
     * Set the intrinsics in this page object
     */

    private void SetIntrinsics(HttpContext context) {
        SetIntrinsics(context, false /* allowAsync */);
    }

    private void SetIntrinsics(HttpContext context, bool allowAsync) {
        _context = context;
        _request = context.Request;
        _response = context.Response;
        _application = context.Application;
        _cache = context.Cache;

        if (!allowAsync && _context != null && _context.ApplicationInstance != null) {
            // disable attempts to launch async operations from pages not marked as async=true
            // the first non-async page [on the server.execute stack] disables async operations
            // it is re-enabled back in HttpApplication after done executing handler
            _context.SyncContext.Disable();
        }

        // Synchronize the ClientTarget
        if (!String.IsNullOrEmpty(_clientTarget)) {
            _request.ClientTarget = _clientTarget;
        }

        // DCR 85444: Support per device type encoding
        HttpCapabilitiesBase caps = _request.Browser;

        if(caps != null) {
            // Dev10 440476: Page.SetIntrinsics method has a bug causing throwing NullReferenceException
            // in certain circumstances. This edge case was regressed by the VSWhidbey fix below.

            // VSWhidbey 109162: Set content type at the very beginning so it can be
            // overwritten within the user code of the page if needed.
            _response.ContentType = caps.PreferredRenderingMime;

            string preferredResponseEncoding = caps.PreferredResponseEncoding;
            string preferredRequestEncoding =  caps.PreferredRequestEncoding;

            if (!String.IsNullOrEmpty(preferredResponseEncoding)) {
                _response.ContentEncoding = Encoding.GetEncoding(preferredResponseEncoding);
            }

            if(!String.IsNullOrEmpty(preferredRequestEncoding)) {
                _request.ContentEncoding = Encoding.GetEncoding(preferredRequestEncoding);
            }
        }

        // Hook up any automatic handler we may find (e.g. Page_Load)
        HookUpAutomaticHandlers();
    }

    /// <devdoc>
    /// Initializes the page's reference to the header control
    /// </devdoc>
    internal void SetHeader(HtmlHead header) {
        _header = header;

        if (!String.IsNullOrEmpty(_titleToBeSet)) {
            if (_header == null) {
                throw new InvalidOperationException(SR.GetString(SR.Page_Title_Requires_Head));
            }
            else {
                Title = _titleToBeSet;
                _titleToBeSet = null;
            }
        }

        if (!String.IsNullOrEmpty(_descriptionToBeSet)) {
            if (_header == null) {
                throw new InvalidOperationException(SR.GetString(SR.Page_Description_Requires_Head));
            }
            else {
                MetaDescription = _descriptionToBeSet;
                _descriptionToBeSet = null;
            }
        }

        if (!String.IsNullOrEmpty(_keywordsToBeSet)) {
            if (_header == null) {
                throw new InvalidOperationException(SR.GetString(SR.Page_Description_Requires_Head));
            }
            else {
                MetaKeywords = _keywordsToBeSet;
                _keywordsToBeSet = null;
            }
        }

    }

    /// Override to unload the previousPage. The previousPage is unloaded after
    /// the current page since current page could depend on previousPage.
    internal override void UnloadRecursive(bool dispose) {
        base.UnloadRecursive(dispose);

        if (_previousPage != null && _previousPage.IsCrossPagePostBack) {
            _previousPage.UnloadRecursive(dispose);
        }
    }

    // ASP Compat helpers
#if !FEATURE_PAL // FEATURE_PAL does not enable COM
    private AspCompatApplicationStep _aspCompatStep;

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected IAsyncResult AspCompatBeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData) {
        SetIntrinsics(context);

        _aspCompatStep = new AspCompatApplicationStep(context, new AspCompatCallback(ProcessRequest));
        return _aspCompatStep.BeginAspCompatExecution(cb, extraData);
    }


    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected void AspCompatEndProcessRequest(IAsyncResult result) {
        _aspCompatStep.EndAspCompatExecution(result);
    }
#endif // !FEATURE_PAL

    // Async page helpers

    public void ExecuteRegisteredAsyncTasks() {
        if (_legacyAsyncTaskManager == null) {
            // no tasks registered
            return;
        }

        if (_legacyAsyncTaskManager.TaskExecutionInProgress) {
            // already executing - don't re-enter
            return;
        }

        HttpAsyncResult ar = _legacyAsyncTaskManager.ExecuteTasks(null /*callback*/, null /*extraData*/);

        if (ar.Error != null) {
            // rethrow any errors running tasks synchronously
            throw new HttpException(null, ar.Error);
        }
    }

    private void CheckRemainingAsyncTasks(bool isThreadAbort) {
        // this method is called at the end of page execution
        // it throws if there are registered async tasks not executed yet
        if (_legacyAsyncTaskManager != null) {
            _legacyAsyncTaskManager.DisposeTimer();

            if (isThreadAbort) {
                _legacyAsyncTaskManager.CompleteAllTasksNow(true);
                return;
            }

            if (!_legacyAsyncTaskManager.FailedToStartTasks && _legacyAsyncTaskManager.AnyTasksRemain) {
                throw new HttpException(SR.GetString(SR.Registered_async_tasks_remain));
            }
        }
    }

    // Registers an asynchronous task with a page. Like other APIs on Page, this method is itself not thread-safe.
    public void RegisterAsyncTask(PageAsyncTask task) {
        if (task == null) {
            throw new ArgumentNullException("task");
        }

        if (SynchronizationContextUtil.CurrentMode == SynchronizationContextMode.Legacy) {
            if (_legacyAsyncTaskManager == null) {
                _legacyAsyncTaskManager = new LegacyPageAsyncTaskManager(this);
            }

            // Need to convert the user-provided PageAsyncTask to a legacy-style task for consumption by the legacy task manager
            LegacyPageAsyncTask legacyTask = new LegacyPageAsyncTask(task.BeginHandler, task.EndHandler, task.TimeoutHandler, task.State, task.ExecuteInParallel);
            _legacyAsyncTaskManager.AddTask(legacyTask);
        }
        else {
            // synchronous pages don't support async tasks
            if (!(this is IHttpAsyncHandler)) {
                throw new InvalidOperationException(SR.GetString(SR.Async_required));
            }

            if (_asyncTaskManager == null) {
                _asyncTaskManager = new PageAsyncTaskManager();
            }

            // We need to detect whether this is TAP or APM and enqueue the appropriate concrete type
            IPageAsyncTask asyncTask = (task.TaskHandler != null)
                ? (IPageAsyncTask)new PageAsyncTaskTap(task.TaskHandler)
                : (IPageAsyncTask)new PageAsyncTaskApm(task.BeginHandler, task.EndHandler, task.State);
            _asyncTaskManager.EnqueueTask(asyncTask);
        }
    }

    class LegacyPageAsyncInfo {
        private Page _page;
        private bool _callerIsBlocking; // in case of blocking caller can't lock app instance on another thread (deadlock)
        private HttpApplication _app;
        private AspNetSynchronizationContextBase _syncContext;
        private HttpAsyncResult _asyncResult;

        private bool _asyncPointReached;
        private int  _handlerCount;
        private ArrayList _beginHandlers;
        private ArrayList _endHandlers;
        private ArrayList _stateObjects;

        private AsyncCallback _completionCallback;
        private WaitCallback _callHandlersThreadpoolCallback;

        private int _currentHandler;
        private Exception _error;
        private bool _completed;

        internal LegacyPageAsyncInfo(Page page) {
            _page = page;
            _app = page.Context.ApplicationInstance;
            _syncContext = page.Context.SyncContext;
            _completionCallback = new AsyncCallback(this.OnAsyncHandlerCompletion);
            _callHandlersThreadpoolCallback = new WaitCallback(this.CallHandlersFromThreadpoolThread);
        }

        internal HttpAsyncResult AsyncResult {
            get { return _asyncResult; }
            set { _asyncResult = value; }
        }

        internal bool AsyncPointReached {
            get { return _asyncPointReached; }
            set { _asyncPointReached = value; }
        }

        internal bool CallerIsBlocking {
            get { return _callerIsBlocking; }
            set { _callerIsBlocking = value; }
        }

        internal void AddHandler(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
            if (_handlerCount == 0) {
                _beginHandlers = new ArrayList();
                _endHandlers   = new ArrayList();
                _stateObjects  = new ArrayList();
            }

            _beginHandlers.Add(beginHandler);
            _endHandlers.Add(endHandler);
            _stateObjects.Add(state);
            _handlerCount++;
        }



        internal void CallHandlers(bool onPageThread) {
            try {
                if (CallerIsBlocking || onPageThread) {
                    // locking app on another thread when the caller is blocking will lead to deadlocks, so don't lock here
                    // VSWhidbey 189344
                    CallHandlersPossiblyUnderLock(onPageThread);
                } else {
                    lock (_app) {
                        CallHandlersPossiblyUnderLock(onPageThread);
                    }
                }
            }
            catch (Exception e) {
                _error = e;
                _completed = true;
                _asyncResult.Complete(onPageThread, null /*result*/, _error);

                if (!onPageThread &&
                    e is ThreadAbortException &&
                    ((ThreadAbortException)e).ExceptionState is HttpApplication.CancelModuleException) {
                    // don't leave this threadpool thread with CancelModuleException
                    // as thread state - it might lead to AppDomainUnloadedException
                    // later, when the current app domain is unloaded and there is
                    // an attempt to get thread state form another app domain on
                    // the same thread
                    ThreadResetAbortWithAssert();
                }
            }
        }

        private void CallHandlersPossiblyUnderLock(bool onPageThread) {
            ThreadContext threadContext = null;

            if (!onPageThread) {
                threadContext = _app.OnThreadEnter();
            }

            try {
                while (_currentHandler < _handlerCount && _error == null) {
                    try {
                        IAsyncResult ar = ((BeginEventHandler)_beginHandlers[_currentHandler])(_page, EventArgs.Empty, _completionCallback, _stateObjects[_currentHandler]);

                        if (ar == null) {
                            throw new InvalidOperationException(SR.GetString(SR.Async_null_asyncresult));
                        }

                        if (ar.CompletedSynchronously) {
                            try {
                                ((EndEventHandler)_endHandlers[_currentHandler])(ar);
                            }
                            finally {
                                _currentHandler++;
                            }

                            continue;
                        }

                        // async completion
                        return;
                    }
                    catch (Exception e) {
                        if (onPageThread && _syncContext.PendingOperationsCount == 0) {
                            throw;
                        }

                        // Increment all of the appropriate error counters
                        PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_DURING_REQUEST);
                        PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_TOTAL);

                        // If it hasn't been handled, rememeber it
                        try {
                            if (!_page.HandleError(e))
                                _error = e;
                        }
                        catch (Exception e2) {
                            _error = e2;
                        }
                    }
                }

                // check if any async operations started
#if DBG
                Debug.Trace("Async", "Page has PendingOperationsCount of " + _syncContext.PendingOperationsCount);
#endif

                if (_syncContext.PendingCompletion(_callHandlersThreadpoolCallback)) {
                    return;
                }

                // get the error that happened in async completion delegates

                if (_error == null && _syncContext.Error != null) {
                    try {
                        if (!_page.HandleError(_syncContext.Error)) {
                            _error = _syncContext.Error;
                            _syncContext.ClearError();
                        }
                    }
                    catch (Exception e) {
                        _error = e;
                    }
                }

                // finish up the page processing

                try {
                    _page.Context.InvokeCancellableCallback(new WaitCallback(o => { _page.ProcessRequest(false /*includeStagesBeforeAsyncPoint*/, true /*includeStagesAfterAsyncPoint*/); }), null);
                }
                catch (Exception e) {
                    if (onPageThread)
                        throw;

                    _error = e;
                }

                // complete the async request (notify HttpAppplication)

                if (threadContext != null) {
                    // call DisassociateFromCurrentThread before Complete, because complete
                    // might resume and finish up the pipeline inside the call
                    try {
                        threadContext.DisassociateFromCurrentThread();
                    }
                    finally {
                        threadContext = null;
                    }
                }

                _completed = true;
                _asyncResult.Complete(onPageThread, null /*result*/, _error);
            }
            finally {
                if (threadContext != null) {
                    threadContext.DisassociateFromCurrentThread();
                }
            }
        }

        private void OnAsyncHandlerCompletion(IAsyncResult ar) {
            if (ar.CompletedSynchronously)  // handled in CallHandlers()
                return;

            try {
                ((EndEventHandler)_endHandlers[_currentHandler])(ar);
            }
            catch (Exception e) {
                _error = e;
            }

            if (_completed) {
                // already completed (possibly due to timeout, don't continue)
                return;
            }

            _currentHandler++;

            if (Thread.CurrentThread.IsThreadPoolThread) {
                // if on thread pool thread, use the current thread
                CallHandlers(false);
            }
            else {
                // if on a non-threadpool thread, requeue
                ThreadPool.QueueUserWorkItem(_callHandlersThreadpoolCallback);
            }
        }

        private void CallHandlersFromThreadpoolThread(Object data) {
            Debug.Trace("Async", "Page -- CallHandlersFromThreadpoolThread");
            CallHandlers(false);
        }

        internal void SetError(Exception error) {
            _error = error;
        }
    }

    private void AsyncPageProcessRequestBeforeAsyncPointCancellableCallback(Object state) {
        ProcessRequest(true /*includeStagesBeforeAsyncPoint*/, false /*includeStagesAfterAsyncPoint*/);
    }

    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected IAsyncResult AsyncPageBeginProcessRequest(HttpContext context, AsyncCallback callback, Object extraData) {
        // This method just dispatches to either the TAP or APM implementation, depending on the synchronization mode
        if (SynchronizationContextUtil.CurrentMode == SynchronizationContextMode.Legacy) {
            return LegacyAsyncPageBeginProcessRequest(context, callback, extraData);
        }
        else {
            return TaskAsyncHelper.BeginTask(() => ProcessRequestAsync(context), callback, extraData);
        }
    }

    private CancellationTokenSource CreateCancellationTokenFromAsyncTimeout() {
        TimeSpan timeout = AsyncTimeout;

        // CancellationTokenSource can only create timers within a specific range (<= _maxAsyncTimeout)
        return (timeout <= _maxAsyncTimeout)
            ? new CancellationTokenSource(timeout)
            : new CancellationTokenSource();
    }

    // TAP
    private async Task ProcessRequestAsync(HttpContext context) {
        // we disallow async operations except during specific portions of the lifecycle
        context.SyncContext.ProhibitVoidAsyncOperations();

        SetIntrinsics(context, true /* allowAsync */);

        if (_asyncTaskManager == null) {
            // could be already created if AddOnPreRenderCompleteAsync called before ProcessRequest
            _asyncTaskManager = new PageAsyncTaskManager();
        }

        try {
            // process everything before the async point
            // the AsyncPageProcessRequestBeforeAsyncPointCancellableCallback method has its own HandleError semantics and will not throw if the exception is handled
            Task preWorkTask = null;
            _context.InvokeCancellableCallback(_ => {
                preWorkTask = ProcessRequestAsync(includeStagesBeforeAsyncPoint: true, includeStagesAfterAsyncPoint: false);
            }, null);
            await preWorkTask;

            // perform the asynchronous work
            try {
                using (CancellationTokenSource cancellationTokenSource = CreateCancellationTokenFromAsyncTimeout()) {
                    CancellationToken cancellationToken = cancellationTokenSource.Token;
                    try {
                        await _asyncTaskManager.ExecuteTasksAsync(this, EventArgs.Empty, cancellationToken, _context.SyncContext, _context.ApplicationInstance);
                    }
                    finally {
                        // Homogenize any exceptions due to request timeout into a TimeoutException.
                        if (cancellationToken.IsCancellationRequested) {
                            throw new TimeoutException(SR.GetString(SR.Async_task_timed_out));
                        }
                    }
                }
            }
            catch (Exception ex) {
                // This catch block is copied from ProcessRequestMain
                // Increment all of the appropriate error counters
                PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_DURING_REQUEST);
                PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_TOTAL);

                // If it hasn't been handled, rethrow it
                if (!HandleError(ex))
                    throw;
            }

            // process everything after the async point
            Task postWorkTask = null;
            _context.InvokeCancellableCallback(_ => {
                postWorkTask = ProcessRequestAsync(includeStagesBeforeAsyncPoint: false, includeStagesAfterAsyncPoint: true);
            }, null);
            await postWorkTask;
        }
        finally {
            // call Unload events
            ProcessRequestCleanup();
        }
    }

    private IAsyncResult LegacyAsyncPageBeginProcessRequest(HttpContext context, AsyncCallback callback, Object extraData) {
        SetIntrinsics(context, true /* allowAsync */);

        if (_legacyAsyncInfo == null) {
            // could be already created if AddOnPreRenderCompleteAsync called before ProcessRequest
            _legacyAsyncInfo = new LegacyPageAsyncInfo(this);
        }

        _legacyAsyncInfo.AsyncResult = new HttpAsyncResult(callback, extraData);
        _legacyAsyncInfo.CallerIsBlocking = (callback == null);

        // process request stages before async point
        try {
            _context.InvokeCancellableCallback(new WaitCallback(this.AsyncPageProcessRequestBeforeAsyncPointCancellableCallback), null);
        }
        catch (Exception e) {
            if (_context.SyncContext.PendingOperationsCount == 0) {
                // if there are no pending async operations it is ok to throw
                throw;
            }

            // can't throw yet, have to wait for pending async operations to finish
            Debug.Trace("Async", "Exception with async pending - saving the error");
            _legacyAsyncInfo.SetError(e);
        }

        // register handler from async manager to run async tasks (if any tasks are registered)
        // for blocking callers async tasks were already run
        if (_legacyAsyncTaskManager != null && !_legacyAsyncInfo.CallerIsBlocking) {
            _legacyAsyncTaskManager.RegisterHandlersForPagePreRenderCompleteAsync();
        }

        // mark async point
        _legacyAsyncInfo.AsyncPointReached = true;

        // disable async operations after this point
        _context.SyncContext.Disable();

        // call into async subscribers
        _legacyAsyncInfo.CallHandlers(true /*onPageThread*/);

        return _legacyAsyncInfo.AsyncResult;
    }


    /// <internalonly/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected void AsyncPageEndProcessRequest(IAsyncResult result) {
        // This method just dispatches to either the TAP or APM implementation, depending on the synchronization mode
        if (SynchronizationContextUtil.CurrentMode == SynchronizationContextMode.Legacy) {
            LegacyAsyncPageEndProcessRequest(result);
        }
        else {
            // EndTask() observes and throws any captured exceptions; also waits for asynchronous completion
            TaskAsyncHelper.EndTask(result);
        }
    }

    private void LegacyAsyncPageEndProcessRequest(IAsyncResult result) {
        if (_legacyAsyncInfo == null)
            return;

        Debug.Assert(_legacyAsyncInfo.AsyncResult == result);

        // End() observes and throws any captured exceptions
        _legacyAsyncInfo.AsyncResult.End();
    }

    public void AddOnPreRenderCompleteAsync(BeginEventHandler beginHandler, EndEventHandler endHandler) {
        AddOnPreRenderCompleteAsync(beginHandler, endHandler, null);
    }

    public void AddOnPreRenderCompleteAsync(BeginEventHandler beginHandler, EndEventHandler endHandler, Object state) {
        if (beginHandler == null) {
            throw new ArgumentNullException("beginHandler");
        }

        if (endHandler == null) {
            throw new ArgumentNullException("endHandler");
        }

        if (SynchronizationContextUtil.CurrentMode == SynchronizationContextMode.Normal) {
            // If using the new synchronization patterns, go against the task manager directly
            RegisterAsyncTask(new PageAsyncTask(beginHandler, endHandler, null, state));
            return;
        }

        if (_legacyAsyncInfo == null) {
            if (this is IHttpAsyncHandler) {
                // could be called from ctor before process request
                _legacyAsyncInfo = new LegacyPageAsyncInfo(this);
            }
            else {
                // synchronous pages don't support add async handler
                throw new InvalidOperationException(SR.GetString(SR.Async_required));
            }
        }

        if (_legacyAsyncInfo.AsyncPointReached) {
            throw new InvalidOperationException(SR.GetString(SR.Async_addhandler_too_late));
        }

        _legacyAsyncInfo.AddHandler(beginHandler, endHandler, state);
    }


    /// <devdoc>
    ///    <para>Instructs any validation controls included on the page to validate their
    ///       assigned information for the incoming page request.</para>
    /// </devdoc>
    public virtual void Validate() {
        _validated = true;
        if (_validators != null) {
            for (int i = 0; i < Validators.Count; i++) {
                Validators[i].Validate();
            }
        }
    }


    public virtual void Validate(string validationGroup) {
        _validated = true;
        if (_validators != null) {
            ValidatorCollection validators = GetValidators(validationGroup);

            // VSWhidbey 207823: When ValidationGroup is the default empty string,
            // we should call the V1 method which could have been overridden so
            // the overridden method on user page wouldn't be missed.
            if (String.IsNullOrEmpty(validationGroup) &&
                _validators.Count == validators.Count) {
                Validate();
            }
            else {
                for (int i = 0; i < validators.Count; i++) {
                    validators[i].Validate();
                }
            }
        }
    }

    public ValidatorCollection GetValidators(string validationGroup) {
        if (validationGroup == null) {
            validationGroup = String.Empty;
        }

        ValidatorCollection validators = new ValidatorCollection();
        if (_validators != null) {
            for (int i = 0; i < Validators.Count; i++) {
                BaseValidator baseValidator = Validators[i] as BaseValidator;
                if (baseValidator != null) {
                    if (0 == String.Compare(baseValidator.ValidationGroup, validationGroup,
                                            StringComparison.Ordinal)) {
                        validators.Add(baseValidator);
                    }
                }
                else if (validationGroup.Length == 0) {
                    validators.Add(Validators[i]);
                }
            }
        }
        return validators;
    }


    /// <devdoc>
    ///    <para>Throws an exception if it is runtime and we are not currently rendering the form runat=server tag.
    ///          Most controls that post back or that use client script require to be in this tag to function, so
    ///          they can call this during rendering. At design time this will do nothing.</para>
    ///    <para>Custom Control creators should call this during render if they render any sort of input tag, if they call
    ///          GetPostBackEventReference, or if they emit client script. A composite control does not need to make this
    ///          call.</para>
    ///    <para>This method should not be overridden unless creating an alternative page framework.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public virtual void VerifyRenderingInServerForm(Control control) {
        // We only want to make this check if we are definitely at runtime
        if (Context == null || DesignMode) {
            return;
        }

        if (control == null) {
            throw new ArgumentNullException("control");
        }

        if (!_inOnFormRender && !IsCallback) {
            throw new HttpException(SR.GetString(SR.ControlRenderedOutsideServerForm, control.ClientID, control.GetType().Name));
        }
    }

    public PageAdapter PageAdapter {
        get {
            if(_pageAdapter == null) {
                ResolveAdapter();
                _pageAdapter = (PageAdapter)AdapterInternal;
            }
            return _pageAdapter;
        }
    }

    // 
    private String _relativeFilePath;

    internal String RelativeFilePath {
        get {
            if (_relativeFilePath == null) {
                String s = Context.Request.CurrentExecutionFilePath;
                String filePath = Context.Request.FilePath;

                if(filePath.Equals(s)) {
                    int slash = s.LastIndexOf('/');

                    if (slash >= 0) {
                        s = s.Substring(slash+1);
                    }
                    _relativeFilePath = s;
                }
                else {
                    _relativeFilePath = Server.UrlDecode(UrlPath.MakeRelative(filePath, s));
                }
            }
            return _relativeFilePath;
        }
    }

    private bool _designModeChecked = false;
    private bool _designMode = false;

    internal bool GetDesignModeInternal() {
        if(!_designModeChecked) {
            _designMode = (Site != null) ? Site.DesignMode : false;
            _designModeChecked = true;
        }
        return _designMode;
    }

    // For use by controls to store information with same lifetime as the request, for example, all radio buttons can
    // use this to store the dictionary of radio button groups.  The key should be a type.  For the example, the value associated with
    // the key System.Web.UI.WebControls.WmlRadioButtonAdapter is a NameValueCollection of RadioButtonGroups.
    private IDictionary _items;

    [
    Browsable(false)
    ]
    public IDictionary Items {
        get {
            if (_items == null) {
                _items = new HybridDictionary();
            }
            return _items;
        }
    }

    // Simplified data binding context stack
    private Stack _dataBindingContext;

    /// <devdoc>
    /// Creates a new context for databinding by pushing a new data item onto the databinding context stack.
    /// </devdoc>
    internal void PushDataBindingContext(object dataItem) {
        if (_dataBindingContext == null) {
            _dataBindingContext = new Stack();
        }
        _dataBindingContext.Push(dataItem);
    }

    /// <devdoc>
    /// Exits a databinding context by removing the current data item from the databinding context stack.
    /// </devdoc>
    internal void PopDataBindingContext() {
        Debug.Assert(_dataBindingContext != null);
        Debug.Assert(_dataBindingContext.Count > 0);
        _dataBindingContext.Pop();
    }


    /// <devdoc>
    /// Gets the current data item from the top of the databinding context stack.
    /// </devdoc>
    public object GetDataItem() {
        if ((_dataBindingContext == null) || (_dataBindingContext.Count == 0)) {
            throw new InvalidOperationException(SR.GetString(SR.Page_MissingDataBindingContext));
        }
        return _dataBindingContext.Peek();
    }

    internal static bool IsSystemPostField(string field) {
        return s_systemPostFields.Contains(field);
    }

    internal IScriptManager ScriptManager {
        get {
            return (IScriptManager)Items[typeof(IScriptManager)];
        }
    }

    private void ValidateRawUrlIfRequired() {
        // Only validate the RawUrl if we weren't asked to skip validation and the current validation mode says we should validate.
        bool validationRequired = !SkipFormActionValidation && CalculateEffectiveValidateRequest();

        if (validationRequired) {
            // Simply touching the RawUrl property getter is sufficient to perform validation.
            string unused = _request.RawUrl;
        }
    }

    // Needed to support Validators in AJAX 1.0 (Windows OS Bugs 2015831)
    #region Atlas ScriptManager Partial Rendering support
    internal bool IsPartialRenderingSupported {
        get {
            if (!_pageFlags[isPartialRenderingSupportedSet]) {
                Type scriptManagerType = ScriptManagerType;
                if (scriptManagerType != null) {
                    object scriptManager = Page.Items[scriptManagerType];
                    if (scriptManager != null) {
                        PropertyInfo supportsPartialRenderingProperty = scriptManagerType.GetProperty("SupportsPartialRendering");
                        if (supportsPartialRenderingProperty != null) {
                            object supportsPartialRenderingValue = supportsPartialRenderingProperty.GetValue(scriptManager, null);
                            _pageFlags[isPartialRenderingSupported] = (bool)supportsPartialRenderingValue;
                        }
                    }
                }
                _pageFlags[isPartialRenderingSupportedSet] = true;
            }
            return _pageFlags[isPartialRenderingSupported];
        }
    }

    internal Type ScriptManagerType {
        get {
            if (_scriptManagerType == null) {
                _scriptManagerType = BuildManager.GetType("System.Web.UI.ScriptManager", false);
            }
            return _scriptManagerType;
        }
        set {
            // Meant for unit testing
            _scriptManagerType = value;
        }
    }
    #endregion

#if DBG
    // Temporary debugging method

    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    public virtual void WalkViewState(object viewState, Control control, int indentLevel) {
        if (viewState == null) {
            return;
        }
        object [] viewStateArray = (object [])viewState;
        object controlViewState = viewStateArray[0];
        IDictionary childViewState = (IDictionary)viewStateArray[1];

        string prefix = "";
        for (int i=0; i < indentLevel; i++) {
            prefix = prefix + "  ";
        }

        if (controlViewState == null) {
            System.Web.Util.Debug.Trace("tpeters", prefix + "ObjViewState: null");
        }
        else {
            System.Web.Util.Debug.Trace("tpeters", prefix + "ObjViewState: " + controlViewState.ToString());
        }

        if (childViewState != null) {
            for (IDictionaryEnumerator e = childViewState.GetEnumerator(); e.MoveNext();) {
                int index = (int) e.Key;
                object value = e.Value;

                if (control == null) {
                    System.Web.Util.Debug.Trace("tpeters", prefix + "Control index: " + index.ToString());
                    WalkViewState(value, null, indentLevel + 1);
                }
                else {

                    string s = "None";
                    bool recurse = false;
                    if (control.HasControls()) {
                        if (index < control.Controls.Count) {
                            s = control.Controls[index].ToString();
                            recurse = true;
                        }
                        else {
                            s = "out of range";
                        }
                    }
                    System.Web.Util.Debug.Trace("tpeters", prefix + "Control index: " + index.ToString() + " control: " + s);
                    if (recurse) {
                        WalkViewState(value, control.Controls[index], indentLevel + 1);
                    }
                }
            }
        }
    }

#endif // DBG
}

// Used to define the list of valid values of the location attribute of the
// OutputCache directive.

/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
public enum OutputCacheLocation {

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    Any,

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    Client,

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    Downstream,

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    Server,

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    None,

    ServerAndClient
}

internal enum SmartNavigationSupport {
    NotDesiredOrSupported=0,   // The Page does not ask for SmartNav, or the browser doesn't support it
    Desired,        // The Page asks for SmartNavigation, but we have not checked browser support
    IE6OrNewer     // SmartNavigation supported by IE6 or newer browsers
}

}

