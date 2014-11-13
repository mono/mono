//------------------------------------------------------------------------------
// <copyright file="TemplateControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

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
    using System.Configuration;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Compilation;
    using System.Web.Util;
    using System.Xml;
    using Debug = System.Web.Util.Debug;

/*
 * Base class for Pages and UserControls
 */

/// <devdoc>
/// <para>Provides the <see cref='System.Web.UI.Page'/> class and the <see cref='System.Web.UI.UserControl'/> class with a base set of functionality.</para>
/// </devdoc>
public abstract class TemplateControl : Control, INamingContainer, IFilterResolutionService {

    // Used for the literal string optimization (reading strings from resource)
    private IntPtr _stringResourcePointer;
    private int _maxResourceOffset;
    private static object _lockObject = new object();

    // Caches the list of auto-hookup methods for each compiled Type (Hashtable<Type,ListDictionary>).
    // We use a Hashtable instead of the central Cache for optimal performance (VSWhidbey 479476)
    private static Hashtable _eventListCache = new Hashtable();

    private static object _emptyEventSingleton = new EventList();

    private VirtualPath _virtualPath;

    private IResourceProvider _resourceProvider;

    private const string _pagePreInitEventName                = "Page_PreInit";
    private const string _pageInitEventName                   = "Page_Init";
    private const string _pageInitCompleteEventName           = "Page_InitComplete";
    private const string _pageLoadEventName                   = "Page_Load";
    private const string _pagePreLoadEventName                = "Page_PreLoad";
    private const string _pageLoadCompleteEventName           = "Page_LoadComplete";
    private const string _pagePreRenderCompleteEventName      = "Page_PreRenderComplete";
    private const string _pagePreRenderCompleteAsyncEventName = "Page_PreRenderCompleteAsync";
    private const string _pageDataBindEventName               = "Page_DataBind";
    private const string _pagePreRenderEventName              = "Page_PreRender";
    private const string _pageSaveStateCompleteEventName      = "Page_SaveStateComplete";
    private const string _pageUnloadEventName                 = "Page_Unload";
    private const string _pageErrorEventName                  = "Page_Error";
    private const string _pageAbortTransactionEventName       = "Page_AbortTransaction";
    private const string _onTransactionAbortEventName         = "OnTransactionAbort";
    private const string _pageCommitTransactionEventName      = "Page_CommitTransaction";
    private const string _onTransactionCommitEventName        = "OnTransactionCommit";

    private static IDictionary _eventObjects;

    // Used to implement no-compile pages/uc
    private BuildResultNoCompileTemplateControl _noCompileBuildResult;

    static TemplateControl() {
        _eventObjects = new Hashtable(16);
        _eventObjects.Add(_pagePreInitEventName, Page.EventPreInit);
        _eventObjects.Add(_pageInitEventName, EventInit);
        _eventObjects.Add(_pageInitCompleteEventName, Page.EventInitComplete);
        _eventObjects.Add(_pageLoadEventName, EventLoad);
        _eventObjects.Add(_pagePreLoadEventName, Page.EventPreLoad);
        _eventObjects.Add(_pageLoadCompleteEventName, Page.EventLoadComplete);
        _eventObjects.Add(_pagePreRenderCompleteEventName, Page.EventPreRenderComplete);
        _eventObjects.Add(_pageDataBindEventName, EventDataBinding);
        _eventObjects.Add(_pagePreRenderEventName, EventPreRender);
        _eventObjects.Add(_pageSaveStateCompleteEventName, Page.EventSaveStateComplete);
        _eventObjects.Add(_pageUnloadEventName, EventUnload);
        _eventObjects.Add(_pageErrorEventName, EventError);
        _eventObjects.Add(_pageAbortTransactionEventName, EventAbortTransaction);
        _eventObjects.Add(_onTransactionAbortEventName, EventAbortTransaction);
        _eventObjects.Add(_pageCommitTransactionEventName, EventCommitTransaction);
        _eventObjects.Add(_onTransactionCommitEventName, EventCommitTransaction);
    }


    protected TemplateControl() {
        Construct();
    }


    /// <devdoc>
    /// <para>Do construction time logic (ASURT 66166)</para>
    /// </devdoc>
    protected virtual void Construct() {}


    private static readonly object EventCommitTransaction = new object();


    /// <devdoc>
    ///    <para>Occurs when a user initiates a transaction.</para>
    /// </devdoc>
    [
    WebSysDescription(SR.Page_OnCommitTransaction)
    ]
    public event EventHandler CommitTransaction {
        add {
            Events.AddHandler(EventCommitTransaction, value);
        }
        remove {
            Events.RemoveHandler(EventCommitTransaction, value);
        }
    }

    /// <devdoc>
    ///    <para>Gets and sets a value indicating whether theme is enabled.</para>
    /// </devdoc>
    [
    Browsable(true)
    ]
    public override bool EnableTheming {
        get {
            return base.EnableTheming;
        }
        set {
            base.EnableTheming = value;
        }
    }


    /// <devdoc>
    /// <para>Raises the <see langword='CommitTransaction'/> event. You can use this method
    ///    for any transaction processing logic in which your page or user control
    ///    participates.</para>
    /// </devdoc>
    protected virtual void OnCommitTransaction(EventArgs e) {
        EventHandler handler = (EventHandler)Events[EventCommitTransaction];
        if (handler != null) handler(this, e);
    }

    private static readonly object EventAbortTransaction = new object();


    /// <devdoc>
    ///    <para>Occurs when a user aborts a transaction.</para>
    /// </devdoc>
    [
    WebSysDescription(SR.Page_OnAbortTransaction)
    ]
    public event EventHandler AbortTransaction {
        add {
            Events.AddHandler(EventAbortTransaction, value);
        }
        remove {
            Events.RemoveHandler(EventAbortTransaction, value);
        }
    }


    /// <devdoc>
    /// <para>Raises the <see langword='AbortTransaction'/> event.</para>
    /// </devdoc>
    protected virtual void OnAbortTransaction(EventArgs e) {
        EventHandler handler = (EventHandler)Events[EventAbortTransaction];
        if (handler != null) handler(this, e);
    }

    // Page_Error related events/methods

    private static readonly object EventError = new object();


    /// <devdoc>
    ///    <para>Occurs when an uncaught exception is thrown.</para>
    /// </devdoc>
    [
    WebSysDescription(SR.Page_Error)
    ]
    public event EventHandler Error {
        add {
            Events.AddHandler(EventError, value);
        }
        remove {
            Events.RemoveHandler(EventError, value);
        }
    }


    /// <devdoc>
    /// <para>Raises the <see langword='Error'/> event.
    ///    </para>
    /// </devdoc>
    protected virtual void OnError(EventArgs e) {
        EventHandler handler = (EventHandler)Events[EventError];
        if (handler != null) handler(this, e);
    }

    /*
     * Receive a no-compile build result that we call during FrameworkInitialize
     */
    internal void SetNoCompileBuildResult(BuildResultNoCompileTemplateControl noCompileBuildResult) {
        _noCompileBuildResult = noCompileBuildResult;
    }

    internal bool NoCompile {
        get { return _noCompileBuildResult != null; }
    }

    /*
     * Method sometime overidden by the generated sub classes.  Users
     * should not override.
     */

    /// <internalonly/>
    /// <devdoc>
    ///    <para>Initializes the requested page. While this is sometimes
    ///       overridden when the page is generated at runtime, you should not explicitly override this method.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected virtual void FrameworkInitialize() {

        // If it's non-compiled, perform the FrameworkInitialize logic.
        if (NoCompile) {
            if (!HttpRuntime.DisableProcessRequestInApplicationTrust) {
                // If PermitOnly was disabled in config, we call PermitOnly from here so that at least
                // the control tree creation is protected, as it always is for compiled pages (VSWhidbey 449666)
                if (HttpRuntime.NamedPermissionSet != null && !HttpRuntime.ProcessRequestInApplicationTrust) {
                    HttpRuntime.NamedPermissionSet.PermitOnly();
                }
            }

            _noCompileBuildResult.FrameworkInitialize(this);
        }
    }

    /*
     * This property is overriden by the generated classes (hence it cannot be internal)
     * If false, we don't do the HookUpAutomaticHandlers() magic.
     */

    /// <internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected virtual bool SupportAutoEvents  {
        get { return true; }
    }

    /*
     * Returns a pointer to the resource buffer, and the largest valid offset
     * in the buffer (for security reason)
     */
    internal IntPtr StringResourcePointer { get { return _stringResourcePointer; } }
    internal int MaxResourceOffset { get { return _maxResourceOffset; } }


    // This method is now obsolete.  Ideally, we should get rid of it altogether, but that
    // would be a breaking change (VSWhidbey 464430)
    // We now use the parameter-less override, which is simpler
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static object ReadStringResource(Type t) {
        return StringResourceManager.ReadSafeStringResource(t);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public object ReadStringResource() {
        return StringResourceManager.ReadSafeStringResource(GetType());
    }


    /// <internalonly/>
    /// <devdoc>
    ///    <para>This method is called by the generated classes (hence it cannot be internal)</para>
    /// </devdoc>
    protected LiteralControl CreateResourceBasedLiteralControl(int offset, int size, bool fAsciiOnly) {
        return new ResourceBasedLiteralControl(this, offset, size, fAsciiOnly);
    }


    /// <internalonly/>
    /// <devdoc>
    ///    <para>This method is called by the generated classes (hence it cannot be internal)</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected void SetStringResourcePointer(object stringResourcePointer, int maxResourceOffset) {

        // Ignore the passed in maxResourceOffset, which cannot be trusted.  Instead, use
        // the resource size that we obtained from the resource (ASURT 122759)
        SafeStringResource ssr = (SafeStringResource) stringResourcePointer;
        _stringResourcePointer = ssr.StringResourcePointer;
        _maxResourceOffset = ssr.ResourceSize;
    }

    internal VirtualPath VirtualPath {
        get {
            return _virtualPath;
        }
    }

    [
    EditorBrowsable(EditorBrowsableState.Advanced),
    Browsable(false)
    ]
    public string AppRelativeVirtualPath {
        get {
            return VirtualPath.GetAppRelativeVirtualPathString(TemplateControlVirtualPath);
        }
        set {
            // Set the TemplateSourceDirectory based on the VirtualPath
            this.TemplateControlVirtualPath = VirtualPath.CreateNonRelative(value);
        }
    }

    internal VirtualPath TemplateControlVirtualPath {
        get {
            return _virtualPath;
        }
        set {
            _virtualPath = value;

            // Set the TemplateSourceDirectory based on the VirtualPath
            this.TemplateControlVirtualDirectory = _virtualPath.Parent;
        }
    }


    /// <devdoc>
    ///    <para>Tests if a device filter applies to this request</para>
    /// </devdoc>
    public virtual bool TestDeviceFilter(string filterName) {
        return(Context.Request.Browser.IsBrowser(filterName));
    }


    /// <internalonly/>
    /// <devdoc>
    ///    <para>This method is called by the generated classes (hence it cannot be internal)</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected void WriteUTF8ResourceString(HtmlTextWriter output, int offset, int size, bool fAsciiOnly) {

        // Make sure we don't access invalid data
        checked {
            if (offset < 0 || size < 0 || offset + size > _maxResourceOffset)
                throw new ArgumentOutOfRangeException("offset");
        }

        output.WriteUTF8ResourceString(StringResourcePointer, offset, size, fAsciiOnly);
    }

    /*
     * This method is overriden by the generated classes (hence it cannot be internal)
     */

    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use of this property is not recommended because it is no longer useful. http://go.microsoft.com/fwlink/?linkid=14202")]
    protected virtual int AutoHandlers {
        get { return 0;}
        set {}
    }

    internal override TemplateControl GetTemplateControl() {
        return this;
    }

    [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "See comment on GetDelegateInformationWithAssert.")]
    internal void HookUpAutomaticHandlers() {
        // Do nothing if auto-events are not supported
        if (!SupportAutoEvents) {
            return;
        }

        // Get the event list for this Type from our cache, if possible
        object o = _eventListCache[GetType()];
        EventList eventList;

        // Try to find what handlers are implemented if not tried before
        if (o == null) {
            lock (_lockObject) {

                // Try the cache again, in case another thread took care of it
                o = (EventList)_eventListCache[GetType()];

                if (o == null) {
                    eventList = new EventList();

                    GetDelegateInformation(eventList);

                    // Cannot find any known handlers.
                    if (eventList.IsEmpty) {
                        o = _emptyEventSingleton;
                    }
                    else {
                        o = eventList;
                    }

                    // Cache it for next time
                    _eventListCache[GetType()] = o;
                }
            }
        }

        // Don't do any thing if no known handlers are found.
        if (o == _emptyEventSingleton) {
            return;
        }

        eventList = (EventList)o;
        IDictionary<string, SyncEventMethodInfo> syncEvents = eventList.SyncEvents;

        // Hook up synchronous events
        foreach (var entry in syncEvents) {
            string key = entry.Key;
            SyncEventMethodInfo info = entry.Value;

            Debug.Assert(_eventObjects[key] != null);

            bool eventExists = false;
            MethodInfo methodInfo = info.MethodInfo;

            Delegate eventDelegates = Events[_eventObjects[key]];
            if (eventDelegates != null) {
                foreach (Delegate eventDelegate in eventDelegates.GetInvocationList()) {
                    // Ignore if this method is already added to the events list.
                    if (eventDelegate.Method.Equals(methodInfo)) {
                        eventExists = true;
                        break;
                    }
                }
            }

            if (!eventExists) {
                // Create a new Calli delegate proxy
                IntPtr functionPtr = methodInfo.MethodHandle.GetFunctionPointer();
                EventHandler handler = (new CalliEventHandlerDelegateProxy(this, functionPtr, info.IsArgless)).Handler;

                // Adds the delegate to events list.
                Events.AddHandler(_eventObjects[key], handler);
            }
        }

        // Hook up asynchronous events
        IDictionary<string, AsyncEventMethodInfo> asyncEvents = eventList.AsyncEvents;

        AsyncEventMethodInfo preRenderCompleteAsyncEvent;
        if (asyncEvents.TryGetValue(_pagePreRenderCompleteAsyncEventName, out preRenderCompleteAsyncEvent)) {
            Page page = (Page)this; // this event handler only exists for the Page type
            if (preRenderCompleteAsyncEvent.RequiresCancellationToken) {
                var handler = FastDelegateCreator<Func<CancellationToken, Task>>.BindTo(this, preRenderCompleteAsyncEvent.MethodInfo);
                page.RegisterAsyncTask(new PageAsyncTask(handler));
            }
            else {
                var handler = FastDelegateCreator<Func<Task>>.BindTo(this, preRenderCompleteAsyncEvent.MethodInfo);
                page.RegisterAsyncTask(new PageAsyncTask(handler));
            }
        }
    }

    private void GetDelegateInformation(EventList eventList) {
        if (HttpRuntime.IsFullTrust) {
            GetDelegateInformationWithNoAssert(eventList);
        }
        else {
            GetDelegateInformationWithAssert(eventList);
        }
    }

    // Make sure we have reflection permission to discover the handlers (ASURT 105965)
    // Using this permission is bad practice; we should use RMA instead of full MemberAccess,
    // or we should force the instance methods to be public. But this is a legacy behavior
    // and we can't change it without breaking the world.
    [ReflectionPermission(SecurityAction.Assert, Flags = ReflectionPermissionFlag.MemberAccess)]
    [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "See comment above.")]
    private void GetDelegateInformationWithAssert(EventList eventList) {
        GetDelegateInformationWithNoAssert(eventList);
    }

    private void GetDelegateInformationWithNoAssert(EventList eventList) {
        IDictionary<string, SyncEventMethodInfo> syncEventDictionary = eventList.SyncEvents;
        IDictionary<string, AsyncEventMethodInfo> asyncEventDictionary = eventList.AsyncEvents;

        if (this is Page) {
            /* SYNCHRONOUS - Page */

            GetDelegateInformationFromSyncMethod(_pagePreInitEventName, syncEventDictionary);
            GetDelegateInformationFromSyncMethod(_pagePreLoadEventName, syncEventDictionary);
            GetDelegateInformationFromSyncMethod(_pageLoadCompleteEventName, syncEventDictionary);
            GetDelegateInformationFromSyncMethod(_pagePreRenderCompleteEventName, syncEventDictionary);
            GetDelegateInformationFromSyncMethod(_pageInitCompleteEventName, syncEventDictionary);
            GetDelegateInformationFromSyncMethod(_pageSaveStateCompleteEventName, syncEventDictionary);

            /* ASYNCHRONOUS - Page */

            GetDelegateInformationFromAsyncMethod(_pagePreRenderCompleteAsyncEventName, asyncEventDictionary);
        }

        /* SYNCHRONOUS - Control */

        GetDelegateInformationFromSyncMethod(_pageInitEventName, syncEventDictionary);
        GetDelegateInformationFromSyncMethod(_pageLoadEventName, syncEventDictionary);
        GetDelegateInformationFromSyncMethod(_pageDataBindEventName, syncEventDictionary);
        GetDelegateInformationFromSyncMethod(_pagePreRenderEventName, syncEventDictionary);
        GetDelegateInformationFromSyncMethod(_pageUnloadEventName, syncEventDictionary);
        GetDelegateInformationFromSyncMethod(_pageErrorEventName, syncEventDictionary);

        if (!GetDelegateInformationFromSyncMethod(_pageAbortTransactionEventName, syncEventDictionary)) {
            GetDelegateInformationFromSyncMethod(_onTransactionAbortEventName, syncEventDictionary);
        }

        if (!GetDelegateInformationFromSyncMethod(_pageCommitTransactionEventName, syncEventDictionary)) {
            GetDelegateInformationFromSyncMethod(_onTransactionCommitEventName, syncEventDictionary);
        }

        /* ASYNCHRONOUS - Control */

    }

    private bool GetDelegateInformationFromAsyncMethod(string methodName, IDictionary<string, AsyncEventMethodInfo> dictionary) {
        // First, try to get a delegate to the single-parameter handler
        MethodInfo parameterfulMethod = GetInstanceMethodInfo(typeof(Func<CancellationToken, Task>), methodName);
        if (parameterfulMethod != null) {
            dictionary[methodName] = new AsyncEventMethodInfo(parameterfulMethod, requiresCancellationToken: true);
            return true;
        }

        // If there isn't one, try the argless one
        MethodInfo parameterlessMethod = GetInstanceMethodInfo(typeof(Func<Task>), methodName);
        if (parameterlessMethod != null) {
            dictionary[methodName] = new AsyncEventMethodInfo(parameterlessMethod, requiresCancellationToken: false);
            return true;
        }

        return false;
    }

    private bool GetDelegateInformationFromSyncMethod(string methodName, IDictionary<string, SyncEventMethodInfo> dictionary) {
        // First, try to get a delegate to the two parameter handler
        MethodInfo parameterfulMethod = GetInstanceMethodInfo(typeof(EventHandler), methodName);
        if (parameterfulMethod != null) {
            dictionary[methodName] = new SyncEventMethodInfo(parameterfulMethod, isArgless: false);
            return true;
        }

        // If there isn't one, try the argless one
        MethodInfo parameterlessMethod = GetInstanceMethodInfo(typeof(VoidMethod), methodName);
        if (parameterlessMethod != null) {
            dictionary[methodName] = new SyncEventMethodInfo(parameterlessMethod, isArgless: true);
            return true;
        }

        return false;
    }

    private MethodInfo GetInstanceMethodInfo(Type delegateType, string methodName) {
        Delegate del = Delegate.CreateDelegate(
            type: delegateType,
            target: this,
            method: methodName,
            ignoreCase: true,
            throwOnBindFailure: false);

        return (del != null) ? del.Method : null;
    }


    /// <devdoc>
    /// <para>Obtains a <see cref='System.Web.UI.UserControl'/> object from a user control file.</para>
    /// </devdoc>
    public Control LoadControl(string virtualPath) {

        return LoadControl(VirtualPath.Create(virtualPath));
    }

    internal Control LoadControl(VirtualPath virtualPath) {

        // If it's relative, make it *app* relative.  Treat is as relative to this
        // user control (ASURT 55513)
        virtualPath = VirtualPath.Combine(this.TemplateControlVirtualDirectory, virtualPath);

        // Process the user control and get its BuildResult
        BuildResult result = BuildManager.GetVPathBuildResult(Context, virtualPath);

        return LoadControl((IWebObjectFactory)result, virtualPath, null /*Type*/, null /*parameters*/);
    }

    // Make sure we have reflection permission to use GetMethod below (ASURT 106196)
    [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
    private void AddStackContextToHashCode(HashCodeCombiner combinedHashCode) {

        StackTrace st = new StackTrace();

        // First, skip all the stack frames that are in the TemplateControl class, as
        // they are irrelevant to the hash.  Start the search at 2 since we know for sure
        // that this method and its caller are in TemplateControl.
        int startingUserFrame = 2;
        for (; ; startingUserFrame++) {
            StackFrame f = st.GetFrame(startingUserFrame);
            if (f.GetMethod().DeclaringType != typeof(TemplateControl)) {
                break;
            }
        }

        // Get a cache key based on the top two items of the caller's stack.
        // It's not guaranteed unique, but for all common cases, it will be
        for (int i = startingUserFrame; i < startingUserFrame + 2; i++) {
            StackFrame f = st.GetFrame(i);

            MethodBase m = f.GetMethod();
            combinedHashCode.AddObject(m.DeclaringType.AssemblyQualifiedName);
            combinedHashCode.AddObject(m.Name);
            combinedHashCode.AddObject(f.GetNativeOffset());
        }
    }


    public Control LoadControl(Type t, object[] parameters) {

        return LoadControl(null /*IWebObjectFactory*/, null /*virtualPath*/, t, parameters);
    }


    private Control LoadControl(IWebObjectFactory objectFactory, VirtualPath virtualPath, Type t, object[] parameters) {

        // Make sure we get an object factory or a type, but not both
        Debug.Assert((objectFactory == null) != (t == null));

        BuildResultCompiledType compiledUCResult = null;
        BuildResultNoCompileUserControl noCompileUCResult = null;

        if (objectFactory != null) {
            // It can be a compiled or no-compile user control
            compiledUCResult = objectFactory as BuildResultCompiledType;
            if (compiledUCResult != null) {
                t = compiledUCResult.ResultType;
                Debug.Assert(t != null);

                // Make sure it's a user control (VSWhidbey 428718)
                Util.CheckAssignableType(typeof(UserControl), t);
            }
            else {
                noCompileUCResult = (BuildResultNoCompileUserControl)objectFactory;
                Debug.Assert(noCompileUCResult != null);
            }
        }
        else {
            // Make sure the type has the correct base class (ASURT 123677)
            if (t != null)
                Util.CheckAssignableType(typeof(Control), t);
        }

        PartialCachingAttribute cacheAttrib;

        // Check if the user control has a PartialCachingAttribute attribute
        if (t != null) {
            cacheAttrib = (PartialCachingAttribute)
                TypeDescriptor.GetAttributes(t)[typeof(PartialCachingAttribute)];
        }
        else {
            cacheAttrib = noCompileUCResult.CachingAttribute;
        }

        if (cacheAttrib == null) {
            // The control is not cached.  Just create it.
            Control c;
            if (objectFactory != null) {
                c = (Control) objectFactory.CreateInstance();
            }
            else {
                c = (Control) HttpRuntime.CreatePublicInstance(t, parameters);
            }

            // If it's a user control, do some extra initialization
            UserControl uc = c as UserControl;
            if (uc != null) {
                Debug.Assert(virtualPath != null);
                if (virtualPath != null)
                    uc.TemplateControlVirtualPath = virtualPath;
                uc.InitializeAsUserControl(Page);
            }

            return c;
        }

        HashCodeCombiner combinedHashCode = new HashCodeCombiner();

        // Start by adding the type or object factory of the user control to the hash.
        // This guarantees that two unrelated user controls don't share the same cached data.
        if (objectFactory != null) {
            combinedHashCode.AddObject(objectFactory);
        }
        else {
            combinedHashCode.AddObject(t);
        }

        // If it's not shared, add some stack frames to the hash
        if (!cacheAttrib.Shared) {
            AddStackContextToHashCode(combinedHashCode);
        }

        string cacheKey = combinedHashCode.CombinedHashString;

        // Wrap it to allow it to be cached
        return new PartialCachingControl(objectFactory, t, cacheAttrib, "_" + cacheKey, parameters);
    }

    // Class that implements the templates returned by LoadTemplate (ASURT 94138)
    internal class SimpleTemplate : ITemplate {
        private IWebObjectFactory _objectFactory;

        internal SimpleTemplate(ITypedWebObjectFactory objectFactory) {

            // Make sure it's a user control (VSWhidbey 428718)
            Util.CheckAssignableType(typeof(UserControl), objectFactory.InstantiatedType);

            _objectFactory = objectFactory;
        }

        public virtual void InstantiateIn(Control control) {
            UserControl uc = (UserControl)_objectFactory.CreateInstance();

            uc.InitializeAsUserControl(control.Page);

            control.Controls.Add(uc);
        }
    }


    /// <devdoc>
    ///    <para>
    ///       Obtains an instance of the <see langword='ITemplate'/> interface from an
    ///       external file.
    ///    </para>
    /// </devdoc>
    public ITemplate LoadTemplate(string virtualPath) {
        return LoadTemplate(VirtualPath.Create(virtualPath));
    }

    internal ITemplate LoadTemplate(VirtualPath virtualPath) {

        // If it's relative, make it *app* relative.  Treat is as relative to this
        // user control (ASURT 55513)
        virtualPath = VirtualPath.Combine(TemplateControlVirtualDirectory, virtualPath);

        // Compile the declarative template and get its object factory
        ITypedWebObjectFactory objectFactory = (ITypedWebObjectFactory)BuildManager.GetVPathBuildResult(
            Context, virtualPath);

        return new SimpleTemplate(objectFactory);
    }


    /// <devdoc>
    ///    <para> Parse the input string into a Control.  Looks for the first control
    ///    in the input.  Returns null if none is found.</para>
    /// </devdoc>
    public Control ParseControl(string content) {
        return ParseControl(content, true);
    }

    public Control ParseControl(string content, bool ignoreParserFilter) {
        return TemplateParser.ParseControl(content, VirtualPath.Create(AppRelativeVirtualPath), ignoreParserFilter);
    }

#if NOTYET

    /// <devdoc>
    ///    <para> Parse the input string into an ITemplate.</para>
    /// </devdoc>
    internal ITemplate ParseTemplate(string content) {
        return TemplateParser.ParseTemplate(content, AppRelativeTemplateSourceDirectory);
    }
#endif

    /// <devdoc>
    /// Used by simplified databinding methods to ensure they can only be called when the control is on a page.
    /// </devdoc>
    private void CheckPageExists() {
        if (Page == null) {
            throw new InvalidOperationException(SR.GetString(SR.TemplateControl_DataBindingRequiresPage));
        }
    }


    /// <devdoc>
    /// Simplified databinding Eval() method. This method uses the current data item to evaluate an expression using DataBinder.Eval().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal object Eval(string expression) {
        CheckPageExists();
        return DataBinder.Eval(Page.GetDataItem(), expression);
    }


    /// <devdoc>
    /// Simplified databinding Eval() method with a format expression. This method uses the current data item to evaluate an expression using DataBinder.Eval().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal string Eval(string expression, string format) {
        CheckPageExists();
        return DataBinder.Eval(Page.GetDataItem(), expression, format);
    }


    /// <devdoc>
    /// Simplified databinding XPath() method. This method uses the current data item to evaluate an XPath expression using XPathBinder.Eval().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal object XPath(string xPathExpression) {
        CheckPageExists();
        return XPathBinder.Eval(Page.GetDataItem(), xPathExpression);
    }

    /// <devdoc>
    /// Simplified databinding XPath() method. This method uses the current data item and a namespace resolver
    /// to evaluate an XPath expression using XPathBinder.Eval().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal object XPath(string xPathExpression, IXmlNamespaceResolver resolver) {
        CheckPageExists();
        return XPathBinder.Eval(Page.GetDataItem(), xPathExpression, resolver);
    }
    

    /// <devdoc>
    /// Simplified databinding XPath() method with a format expression. This method uses the current data item to evaluate an XPath expression using XPathBinder.Eval().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal string XPath(string xPathExpression, string format) {
        CheckPageExists();
        return XPathBinder.Eval(Page.GetDataItem(), xPathExpression, format);
    }

    /// <devdoc>
    /// Simplified databinding XPath() method with a format expression. This method uses the current data item and a namespace resolver
    /// to evaluate an XPath expression using XPathBinder.Eval().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal string XPath(string xPathExpression, string format, IXmlNamespaceResolver resolver) {
        CheckPageExists();
        return XPathBinder.Eval(Page.GetDataItem(), xPathExpression, format, resolver);
    }
    

    /// <devdoc>
    /// Simplified databinding XPathSelect() method. This method uses the current data item to evaluate an XPath expression that returns a node list using XPathBinder.Select().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal IEnumerable XPathSelect(string xPathExpression) {
        CheckPageExists();
        return XPathBinder.Select(Page.GetDataItem(), xPathExpression);
    }

    /// <devdoc>
    /// Simplified databinding XPathSelect() method. This method uses the current data item and a namespace resolver
    /// to evaluate an XPath expression that returns a node list using XPathBinder.Select().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal IEnumerable XPathSelect(string xPathExpression, IXmlNamespaceResolver resolver) {
        CheckPageExists();
        return XPathBinder.Select(Page.GetDataItem(), xPathExpression, resolver);
    }


    /// <devdoc>
    /// Return a Page-level resource object
    /// </devdoc>
    protected object GetLocalResourceObject(string resourceKey) {

        // Cache the resource provider in the template control, so that if a Page needs to call
        // this multiple times, we don't need to call ResourceExpressionBuilder.GetLocalResourceProvider
        // every time.
        if (_resourceProvider == null)
            _resourceProvider = ResourceExpressionBuilder.GetLocalResourceProvider(this);

        return ResourceExpressionBuilder.GetResourceObject(_resourceProvider, resourceKey, null /*culture*/);
    }

    protected object GetLocalResourceObject(string resourceKey, Type objType, string propName) {

        // Cache the resource provider in the template control, so that if a Page needs to call
        // this multiple times, we don't need to call ResourceExpressionBuilder.GetLocalResourceProvider
        // every time.
        if (_resourceProvider == null)
            _resourceProvider = ResourceExpressionBuilder.GetLocalResourceProvider(this);

        return ResourceExpressionBuilder.GetResourceObject(_resourceProvider,
            resourceKey, null /*culture*/, objType, propName);
    }


    /// <devdoc>
    /// Return an App-level resource object
    /// </devdoc>
    protected object GetGlobalResourceObject(string className, string resourceKey) {
        return ResourceExpressionBuilder.GetGlobalResourceObject(className, resourceKey, null, null, null);
    }

    protected object GetGlobalResourceObject(string className, string resourceKey, Type objType, string propName) {
        return ResourceExpressionBuilder.GetGlobalResourceObject(className, resourceKey, objType, propName, null);
    }

    #region IFilterResolutionService

    /// <internalonly/>
    bool IFilterResolutionService.EvaluateFilter(string filterName) {
        return TestDeviceFilter(filterName);
    }


    /// <internalonly/>
    int IFilterResolutionService.CompareFilters(string filter1, string filter2) {
        return BrowserCapabilitiesCompiler.BrowserCapabilitiesFactory.CompareFilters(filter1, filter2);
    }
    #endregion

    private class EventList {
        internal readonly IDictionary<string, AsyncEventMethodInfo> AsyncEvents = new Dictionary<string, AsyncEventMethodInfo>(StringComparer.Ordinal);
        internal readonly IDictionary<string, SyncEventMethodInfo> SyncEvents = new Dictionary<string, SyncEventMethodInfo>(StringComparer.Ordinal);

        internal bool IsEmpty {
            get {
                return (AsyncEvents.Count == 0 && SyncEvents.Count == 0);
            }
        }
    }

    // Internal helper class for storing the event info
    private class SyncEventMethodInfo {
        internal SyncEventMethodInfo(MethodInfo methodInfo, bool isArgless){
            if (IsAsyncVoidMethod(methodInfo)) {
                SynchronizationContextUtil.ValidateModeForPageAsyncVoidMethods();
            }

            MethodInfo = methodInfo;
            IsArgless = isArgless;
        }

        internal bool IsArgless { get; private set; }
        internal MethodInfo MethodInfo { get; private set; }

        private static bool IsAsyncVoidMethod(MethodInfo methodInfo) {
            // When the C# / VB compilers generate an 'async void' method, they'll put
            // an [AsyncStateMachine] attribute on the entry point. This marker attribute
            // can be used to detect these methods. It's not 100% reliable, since it's
            // possible that a normal void method simply calls an async void method, and
            // the 'outer' method won't contain this attribute. But the heuristic is
            // good enough to help developers land in the pit of success re: async.

            return methodInfo.IsDefined(typeof(AsyncStateMachineAttribute), inherit: false);
        }
    }

    private class AsyncEventMethodInfo {
        internal AsyncEventMethodInfo(MethodInfo methodInfo, bool requiresCancellationToken) {
            MethodInfo = methodInfo;
            RequiresCancellationToken = requiresCancellationToken;
        }

        internal MethodInfo MethodInfo { get; private set; }
        internal bool RequiresCancellationToken { get; private set; }
    }
}
}
