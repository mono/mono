//
// System.Web.UI.Page.cs
//
// Authors:
//   Duncan Mak  (duncan@ximian.com)
//   Gonzalo Paniagua (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002,2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2003,2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.SessionState;
using System.Web.Util;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
#if NET_2_0
using System.Web.UI.Adapters;
using System.Collections.Generic;
using System.Reflection;
#endif

namespace System.Web.UI
{
// CAS
[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#if !NET_2_0
[RootDesignerSerializer ("Microsoft.VSDesigner.WebForms.RootCodeDomSerializer, " + Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design, true)]
#endif
[DefaultEvent ("Load"), DesignerCategory ("ASPXCodeBehind")]
[ToolboxItem (false)]
#if NET_2_0
[Designer ("Microsoft.VisualStudio.Web.WebForms.WebFormDesigner, " + Consts.AssemblyMicrosoft_VisualStudio_Web, typeof (IRootDesigner))]
#else
[Designer ("Microsoft.VSDesigner.WebForms.WebFormDesigner, " + Consts.AssemblyMicrosoft_VSDesigner, typeof (IRootDesigner))]
#endif
public partial class Page : TemplateControl, IHttpHandler
{
	static string machineKeyConfigPath = "system.web/machineKey";
#if NET_2_0
	private PageLifeCycle _lifeCycle = PageLifeCycle.Unknown;
	private bool _eventValidation = true;
	private object [] _savedControlState;
	private bool _doLoadPreviousPage;
	string _focusedControlID;
	bool _hasEnabledControlArray;
#endif
	private bool _viewState = true;
	private bool _viewStateMac;
	private string _errorPage;
	private bool is_validated;
	private bool _smartNavigation;
	private int _transactionMode;
	private HttpContext _context;
	private ValidatorCollection _validators;
	private bool renderingForm;
	private string _savedViewState;
	private ArrayList _requiresPostBack;
	private ArrayList _requiresPostBackCopy;
	private ArrayList requiresPostDataChanged;
	private IPostBackEventHandler requiresRaiseEvent;
	private NameValueCollection secondPostData;
	private bool requiresPostBackScript;
	private bool postBackScriptRendered;
	bool handleViewState;
	string viewStateUserKey;
	NameValueCollection _requestValueCollection;
	string clientTarget;
	ClientScriptManager scriptManager;
	bool allow_load; // true when the Form collection belongs to this page (GetTypeHashCode)
	PageStatePersister page_state_persister;

	[EditorBrowsable (EditorBrowsableState.Never)]
#if NET_2_0
	public
#else
	protected
#endif
	const string postEventArgumentID = "__EVENTARGUMENT";

	[EditorBrowsable (EditorBrowsableState.Never)]
#if NET_2_0
	public
#else
	protected
#endif
	const string postEventSourceID = "__EVENTTARGET";

#if NET_2_0
	const string ScrollPositionXID = "__SCROLLPOSITIONX";
	const string ScrollPositionYID = "__SCROLLPOSITIONY";
	const string EnabledControlArrayID = "__enabledControlArray";
#endif

#if NET_2_0
	internal const string LastFocusID = "__LASTFOCUS";
	internal const string CallbackArgumentID = "__CALLBACKARGUMENT";
	internal const string CallbackSourceID = "__CALLBACKTARGET";
	internal const string PreviousPageID = "__PREVIOUSPAGE";

	HtmlHead htmlHeader;
	
	MasterPage masterPage;
	string masterPageFile;
	
	Page previousPage;
	bool isCrossPagePostBack;
	bool isPostBack;
	bool isCallback;
	ArrayList requireStateControls;
	Hashtable _validatorsByGroup;
	HtmlForm _form;

	string _title;
	string _theme;
	string _styleSheetTheme;
	Hashtable items;

	bool _maintainScrollPositionOnPostBack;

	private bool asyncMode = false;
	private TimeSpan asyncTimeout;
	private const double DefaultAsyncTimeout = 45.0;
	private List<PageAsyncTask> parallelTasks;
	private List<PageAsyncTask> serialTasks;

	private ViewStateEncryptionMode viewStateEncryptionMode;
	private bool controlRegisteredForViewStateEncryption = false;
#endif

	#region Constructor
	public Page ()
	{
		scriptManager = new ClientScriptManager (this);
		Page = this;
		ID = "__Page";
#if NET_2_0
		PagesSection ps = WebConfigurationManager.GetSection ("system.web/pages") as PagesSection;
		if (ps != null) {
			asyncTimeout = ps.AsyncTimeout;
			viewStateEncryptionMode = ps.ViewStateEncryptionMode;
		} else {
			asyncTimeout = TimeSpan.FromSeconds (DefaultAsyncTimeout);
			viewStateEncryptionMode = ViewStateEncryptionMode.Auto;
		}
#endif
	}

	#endregion		

	#region Properties

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public HttpApplicationState Application
	{
		get {
			if (_context == null)
				return null;
			return _context.Application;
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected bool AspCompatMode
	{
#if NET_2_0
		get { return false; }
#endif
		set { throw new NotImplementedException (); }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
#if NET_2_0
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public bool Buffer
	{
		get { return Response.BufferOutput; }
		set { Response.BufferOutput = value; }
	}
#else
	protected bool Buffer
	{
		set { Response.BufferOutput = value; }
	}
#endif

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public Cache Cache
	{
		get {
			if (_context == null)
				throw new HttpException ("No cache available without a context.");
			return _context.Cache;
		}
	}

#if NET_2_0
	[EditorBrowsableAttribute (EditorBrowsableState.Advanced)]
#endif
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false), DefaultValue ("")]
	[WebSysDescription ("Value do override the automatic browser detection and force the page to use the specified browser.")]
	public string ClientTarget
	{
		get { return (clientTarget == null) ? "" : clientTarget; }
		set {
			clientTarget = value;
			if (value == "")
				clientTarget = null;
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
#if NET_2_0
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public int CodePage
	{
		get { return Response.ContentEncoding.CodePage; }
		set { Response.ContentEncoding = Encoding.GetEncoding (value); }
	}
#else
	protected int CodePage
	{
		set { Response.ContentEncoding = Encoding.GetEncoding (value); }
	}
#endif

	[EditorBrowsable (EditorBrowsableState.Never)]
#if NET_2_0
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public string ContentType
	{
		get { return Response.ContentType; }
		set { Response.ContentType = value; }
	}
#else
	protected string ContentType
	{
		set { Response.ContentType = value; }
	}
#endif

	protected override HttpContext Context
	{
		get {
			if (_context == null)
				return HttpContext.Current;

			return _context;
		}
	}

#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public string Culture
	{
		get { return Thread.CurrentThread.CurrentCulture.Name; }
		set { Thread.CurrentThread.CurrentCulture = GetPageCulture (value, Thread.CurrentThread.CurrentCulture); }
	}
#else
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected string Culture
	{
		set { Thread.CurrentThread.CurrentCulture = new CultureInfo (value); }
	}
#endif

#if NET_2_0
	public virtual bool EnableEventValidation {
		get { return _eventValidation; }
		set {
			if (_lifeCycle > PageLifeCycle.Init)
				throw new InvalidOperationException ("The 'EnableEventValidation' property can be set only in the Page_init, the Page directive or in the <pages> configuration section.");
			_eventValidation = value;
		}
	}

	internal PageLifeCycle LifeCycle {
		get { return _lifeCycle; }
	}
#endif

	[Browsable (false)]
	public override bool EnableViewState
	{
		get { return _viewState; }
		set { _viewState = value; }
	}

#if NET_2_0
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
#endif
	[EditorBrowsable (EditorBrowsableState.Never)]
#if NET_2_0
	public
#else
	protected
#endif
	bool EnableViewStateMac
	{
		get { return _viewStateMac; }
		set { _viewStateMac = value; }
	}

#if NET_1_1
	internal bool EnableViewStateMacInternal {
		get { return _viewStateMac; }
		set { _viewStateMac = value; }
	}
#endif
	
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false), DefaultValue ("")]
	[WebSysDescription ("The URL of a page used for error redirection.")]
	public string ErrorPage
	{
		get { return _errorPage; }
		set {
			_errorPage = value;
			if (_context != null)
				_context.ErrorPage = value;
		}
	}

#if NET_2_0
	[Obsolete]
#endif
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected ArrayList FileDependencies
	{
		set {
			if (Response != null)
				Response.AddFileDependencies (value);
		}
	}

	[Browsable (false)]
#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Never)]
#endif
	public override string ID
	{
		get { return base.ID; }
		set { base.ID = value; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public bool IsPostBack
	{
		get {
#if NET_2_0
			return isPostBack;
#else
			return _requestValueCollection != null;
#endif
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never), Browsable (false)]
	public bool IsReusable {
		get { return false; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public bool IsValid {
		get {
			if (!is_validated)
				throw new HttpException (Locale.GetText ("Page hasn't been validated."));

#if NET_2_0
			foreach (IValidator val in Validators)
				if (!val.IsValid)
					return false;
			return true;
#else
			return ValidateCollection (_validators);
#endif
		}
	}
#if NET_2_0
	public IDictionary Items {
		get {
			if (items == null)
				items = new Hashtable ();
			return items;
		}
	}
#endif

	[EditorBrowsable (EditorBrowsableState.Never)]
#if NET_2_0
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public int LCID {
		get { return Thread.CurrentThread.CurrentCulture.LCID; }
		set { Thread.CurrentThread.CurrentCulture = new CultureInfo (value); }
	}
#else
	protected int LCID {
		set { Thread.CurrentThread.CurrentCulture = new CultureInfo (value); }
	}
#endif

#if NET_2_0
	[Browsable (false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public bool MaintainScrollPositionOnPostBack {
		get { return _maintainScrollPositionOnPostBack; }
		set { _maintainScrollPositionOnPostBack = value; }
	}
#endif

#if NET_2_0
	public PageAdapter PageAdapter {
		get {
			return (PageAdapter)Adapter;
		}
	}
#endif

#if !TARGET_J2EE
	internal string theForm {
		get {
			return "theForm";
		}
	}
#endif

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public HttpRequest Request
	{
		get {
			if (_context != null)
				return _context.Request;

			throw new HttpException("Request is not available without context");
		}
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public HttpResponse Response
	{
		get {
			if (_context != null)
				return _context.Response;

			throw new HttpException ("Response is not available without context");
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
#if NET_2_0
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public string ResponseEncoding
	{
		get { return Response.ContentEncoding.WebName; }
		set { Response.ContentEncoding = Encoding.GetEncoding (value); }
	}
#else
	protected string ResponseEncoding
	{
		set { Response.ContentEncoding = Encoding.GetEncoding (value); }
	}
#endif

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public HttpServerUtility Server
	{
		get {
			return Context.Server;
		}
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public virtual HttpSessionState Session
	{
		get {
			if (_context == null)
				_context = HttpContext.Current;

			if (_context == null)
				throw new HttpException ("Session is not available without context");

			if (_context.Session == null)
				throw new HttpException ("Session state can only be used " +
						"when enableSessionState is set to true, either " +
						"in a configuration file or in the Page directive.");

			return _context.Session;
		}
	}

#if NET_2_0
	[FilterableAttribute (false)]
	[Obsolete]
#endif
	[Browsable (false)]
	public bool SmartNavigation
	{
		get { return _smartNavigation; }
		set { _smartNavigation = value; }
	}

#if NET_2_0
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Filterable (false)]
	[Browsable (false)]
	public virtual string StyleSheetTheme {
		get { return _styleSheetTheme; }
		set { _styleSheetTheme = value; }
	}

	[Browsable (false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public virtual string Theme {
		get { return _theme; }
		set { _theme = value; }
	}

	void InitializeStyleSheet ()
	{
		if (_styleSheetTheme == null) {
			PagesSection ps = WebConfigurationManager.GetSection ("system.web/pages") as PagesSection;
			if (ps != null)
				_styleSheetTheme = ps.StyleSheetTheme;
		}
		if (_styleSheetTheme != null && _styleSheetTheme != "")
			_styleSheetPageTheme = ThemeDirectoryCompiler.GetCompiledInstance (_styleSheetTheme, _context);
	}

	void InitializeTheme ()
	{
		if (_theme == null) {
			PagesSection ps = WebConfigurationManager.GetSection ("system.web/pages") as PagesSection;
			if (ps != null)
				_theme = ps.Theme;
		}
		if (_theme != null && _theme != "") {
			_pageTheme = ThemeDirectoryCompiler.GetCompiledInstance (_theme, _context);
			_pageTheme.SetPage (this);
		}
	}

#endif

#if NET_2_0
	[Localizable (true)] 
	[Bindable (true)] 
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public string Title {
		get {
			if (_title == null)
				return htmlHeader.Title;
			return _title;
		}
		set {
			if (htmlHeader != null)
				htmlHeader.Title = value;
			else
				_title = value;
		}
	}
#endif

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public TraceContext Trace
	{
		get { return Context.Trace; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
#if NET_2_0
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public bool TraceEnabled
	{
		get { return Trace.IsEnabled; }
		set { Trace.IsEnabled = value; }
	}
#else
	protected bool TraceEnabled
	{
		set { Trace.IsEnabled = value; }
	}
#endif

	[EditorBrowsable (EditorBrowsableState.Never)]
#if NET_2_0
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public TraceMode TraceModeValue
	{
		get { return Trace.TraceMode; }
		set { Trace.TraceMode = value; }
	}
#else
	protected TraceMode TraceModeValue
	{
		set { Trace.TraceMode = value; }
	}
#endif

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected int TransactionMode
	{
#if NET_2_0
		get { return _transactionMode; }
#endif
		set { _transactionMode = value; }
	}

#if !NET_2_0
	//
	// This method is here just to remove the warning about "_transactionMode" not being
	// used.  We probably will use it internally at some point.
	//
	internal int GetTransactionMode ()
	{
		return _transactionMode;
	}
#endif
	
#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public string UICulture
	{
		get { return Thread.CurrentThread.CurrentUICulture.Name; }
		set { Thread.CurrentThread.CurrentUICulture = GetPageCulture (value, Thread.CurrentThread.CurrentUICulture); }
	}
#else
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected string UICulture
	{
		set { Thread.CurrentThread.CurrentUICulture = new CultureInfo (value); }
	}
#endif

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public IPrincipal User
	{
		get { return Context.User; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public ValidatorCollection Validators
	{
		get { 
			if (_validators == null)
				_validators = new ValidatorCollection ();
			return _validators;
		}
	}

	[MonoTODO ("Use this when encrypting/decrypting ViewState")]
	[Browsable (false)]
	public string ViewStateUserKey {
		get { return viewStateUserKey; }
		set { viewStateUserKey = value; }
	}

	[Browsable (false)]
	public override bool Visible
	{
		get { return base.Visible; }
		set { base.Visible = value; }
	}

	#endregion

	#region Methods

#if NET_2_0
	CultureInfo GetPageCulture (string culture, CultureInfo deflt)
	{
		if (culture == null)
			return deflt;
		CultureInfo ret = null;
		if (culture.StartsWith ("auto", StringComparison.InvariantCultureIgnoreCase)) {
#if TARGET_J2EE
			if (Context.IsPortletRequest)
				return deflt;
#endif
			string[] languages = Request.UserLanguages;
			try {
				if (languages != null && languages.Length > 0)
					ret = CultureInfo.CreateSpecificCulture (languages[0]);
			} catch {
			}
			
			if (ret == null)
				ret = deflt;
		} else
			ret = CultureInfo.CreateSpecificCulture (culture);

		return ret;
	}
#endif

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected IAsyncResult AspCompatBeginProcessRequest (HttpContext context,
							     AsyncCallback cb, 
							     object extraData)
	{
		throw new NotImplementedException ();
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected void AspCompatEndProcessRequest (IAsyncResult result)
	{
		throw new NotImplementedException ();
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	protected virtual HtmlTextWriter CreateHtmlTextWriter (TextWriter tw)
	{
		return new HtmlTextWriter (tw);
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	public void DesignerInitialize ()
	{
		InitRecursive (null);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	protected virtual NameValueCollection DeterminePostBackMode ()
	{
		if (_context == null)
			return null;

		HttpRequest req = _context.Request;
		if (req == null)
			return null;

		NameValueCollection coll = null;
		if (0 == String.Compare (Request.HttpMethod, "POST", true, CultureInfo.InvariantCulture))
			coll = req.Form;
#if TARGET_J2EE
		else if (IsPortletRender && req.Form ["__VIEWSTATE"] != null)
			coll = req.Form;
#endif
		else
			coll = req.QueryString;

		WebROCollection c = (WebROCollection) coll;
		allow_load = !c.GotID;
		if (allow_load)
			c.ID = GetTypeHashCode ();
		else
			allow_load = (c.ID == GetTypeHashCode ());

		if (coll != null && coll ["__VIEWSTATE"] == null && coll ["__EVENTTARGET"] == null)
			return null;

		return coll;
	}

#if NET_2_0
	public override Control FindControl (string id) {
		if (id == ID)
			return this;
		else
			return base.FindControl (id);
	}
#endif

#if NET_2_0
	[Obsolete]
#endif
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackClientEvent (Control control, string argument)
	{
		return scriptManager.GetPostBackEventReference (control, argument);
	}

#if NET_2_0
	[Obsolete]
#endif
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackClientHyperlink (Control control, string argument)
	{
		return scriptManager.GetPostBackClientHyperlink (control, argument);
	}

#if NET_2_0
	[Obsolete]
#endif
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackEventReference (Control control)
	{
		return scriptManager.GetPostBackEventReference (control, "");
	}

#if NET_2_0
	[Obsolete]
#endif
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackEventReference (Control control, string argument)
	{
		return scriptManager.GetPostBackEventReference (control, argument);
	}

	internal void RequiresPostBackScript ()
	{
#if NET_2_0
		if (requiresPostBackScript)
			return;
		ClientScript.RegisterHiddenField (postEventSourceID, String.Empty);
		ClientScript.RegisterHiddenField (postEventArgumentID, String.Empty);
#endif
		requiresPostBackScript = true;
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	public virtual int GetTypeHashCode ()
	{
		return 0;
	}

#if NET_2_0
    [MonoTODO("The following properties of OutputCacheParameters are silently ignored: CacheProfile, NoStore, SqlDependency")]
    protected internal virtual void InitOutputCache(OutputCacheParameters cacheSettings)
    {
        if (cacheSettings.Enabled)
            InitOutputCache(cacheSettings.Duration,
                cacheSettings.VaryByHeader,
                cacheSettings.VaryByCustom,
                cacheSettings.Location,
                cacheSettings.VaryByParam);
    }
#endif

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected virtual void InitOutputCache (int duration,
						string varyByHeader,
						string varyByCustom,
						OutputCacheLocation location,
						string varyByParam)
	{
		HttpCachePolicy cache = _context.Response.Cache;
		bool set_vary = false;

		switch (location) {
		case OutputCacheLocation.Any:
			cache.SetCacheability (HttpCacheability.Public);
			cache.SetMaxAge (new TimeSpan (0, 0, duration));		
			cache.SetLastModified (_context.Timestamp);
			set_vary = true;
			break;
		case OutputCacheLocation.Client:
			cache.SetCacheability (HttpCacheability.Private);
			cache.SetMaxAge (new TimeSpan (0, 0, duration));		
			cache.SetLastModified (_context.Timestamp);
			break;
		case OutputCacheLocation.Downstream:
			cache.SetCacheability (HttpCacheability.Public);
			cache.SetMaxAge (new TimeSpan (0, 0, duration));		
			cache.SetLastModified (_context.Timestamp);
			break;
		case OutputCacheLocation.Server:			
			cache.SetCacheability (HttpCacheability.Server);
			set_vary = true;
			break;
		case OutputCacheLocation.None:
			break;
		}

		if (set_vary) {
			if (varyByCustom != null)
				cache.SetVaryByCustom (varyByCustom);

			if (varyByParam != null && varyByParam.Length > 0) {
				string[] prms = varyByParam.Split (';');
				foreach (string p in prms)
					cache.VaryByParams [p.Trim ()] = true;
				cache.VaryByParams.IgnoreParams = false;
			} else {
				cache.VaryByParams.IgnoreParams = true;
			}
			
			if (varyByHeader != null && varyByHeader.Length > 0) {
				string[] hdrs = varyByHeader.Split (';');
				foreach (string h in hdrs)
					cache.VaryByHeaders [h.Trim ()] = true;
			}
		}
			
		cache.Duration = duration;
		cache.SetExpires (_context.Timestamp.AddSeconds (duration));
	}

#if NET_2_0
	[Obsolete]
#else
	[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
	public bool IsClientScriptBlockRegistered (string key)
	{
		return scriptManager.IsClientScriptBlockRegistered (key);
	}

#if NET_2_0
	[Obsolete]
#else
	[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
	public bool IsStartupScriptRegistered (string key)
	{
		return scriptManager.IsStartupScriptRegistered (key);
	}

	public string MapPath (string virtualPath)
	{
		return Request.MapPath (virtualPath);
	}

#if NET_2_0
	protected internal override void Render (HtmlTextWriter writer) {
		if (MaintainScrollPositionOnPostBack) {
			ClientScript.RegisterWebFormClientScript ();

			ClientScript.RegisterHiddenField (ScrollPositionXID, Request [ScrollPositionXID]);
			ClientScript.RegisterHiddenField (ScrollPositionYID, Request [ScrollPositionYID]);
			
			StringBuilder script = new StringBuilder ();
			script.AppendLine ("<script type=\"text/javascript\">");
			script.AppendLine ("<!--");
			script.AppendLine (theForm + ".oldSubmit = " + theForm + ".submit;");
			script.AppendLine (theForm + ".submit = WebForm_SaveScrollPositionSubmit;");
			script.AppendLine (theForm + ".oldOnSubmit = " + theForm + ".onsubmit;");
			script.AppendLine (theForm + ".onsubmit = WebForm_SaveScrollPositionOnSubmit;");
			if (IsPostBack) {
				script.AppendLine (theForm + ".oldOnLoad = window.onload;");
				script.AppendLine ("window.onload = function () { WebForm_RestoreScrollPosition (" + theForm + "); };");
			}
			script.AppendLine ("// -->");
			script.AppendLine ("</script>");
			
			ClientScript.RegisterStartupScript (typeof (Page), "MaintainScrollPositionOnPostBackStartup", script.ToString());
		}
		base.Render (writer);
	}
#endif

	private void RenderPostBackScript (HtmlTextWriter writer, string formUniqueID)
	{
#if ONLY_1_1
		writer.WriteLine ("<input type=\"hidden\" name=\"{0}\" value=\"\" />", postEventSourceID);
		writer.WriteLine ("<input type=\"hidden\" name=\"{0}\" value=\"\" />", postEventArgumentID);
#endif
		writer.WriteLine ();
		
		ClientScript.WriteBeginScriptBlock (writer);

#if ONLY_1_1
		RenderClientScriptFormDeclaration (writer, formUniqueID);
#endif
		writer.WriteLine ("function __doPostBack(eventTarget, eventArgument) {");
		writer.WriteLine ("\tvar currForm = {0};", theForm);
#if NET_2_0
		writer.WriteLine ("\tcurrForm.__doPostBack(eventTarget, eventArgument);");
		writer.WriteLine ("}");
		writer.WriteLine ("{0}.__doPostBack = function (eventTarget, eventArgument) {{", theForm);
		writer.WriteLine ("\tvar currForm = this;");
		writer.WriteLine ("\tif(currForm.onsubmit && currForm.onsubmit() == false) return;");
#else
		writer.WriteLine ("\tif(document.ValidatorOnSubmit && !ValidatorOnSubmit()) return;");
#endif
		writer.WriteLine ("\tcurrForm.{0}.value = eventTarget;", postEventSourceID);
		writer.WriteLine ("\tcurrForm.{0}.value = eventArgument;", postEventArgumentID);
		writer.WriteLine ("\tcurrForm.submit();");
		writer.WriteLine ("}");
		
		ClientScript.WriteEndScriptBlock (writer);
	}

	void RenderClientScriptFormDeclaration (HtmlTextWriter writer, string formUniqueID)
	{
		writer.WriteLine ("\tvar {0};\n\tif (document.getElementById) {{ {0} = document.getElementById ('{1}'); }}", theForm, formUniqueID);
		writer.WriteLine ("\telse {{ {0} = document.{1}; }}", theForm, formUniqueID);
		writer.WriteLine ("\t{0}.isAspForm = true;", theForm);
#if TARGET_J2EE
		string serverUrl = Context.ServletResponse.encodeURL (Request.RawUrl);
		writer.WriteLine ("\t{0}.serverURL = {1};", theForm, ClientScriptManager.GetScriptLiteral (serverUrl));
#endif
	}

	internal void OnFormRender (HtmlTextWriter writer, string formUniqueID)
	{
		if (renderingForm)
			throw new HttpException ("Only 1 HtmlForm is allowed per page.");

		renderingForm = true;
		writer.WriteLine ();

#if NET_2_0
		ClientScript.WriteBeginScriptBlock (writer);
		RenderClientScriptFormDeclaration (writer, formUniqueID);
		ClientScript.WriteEndScriptBlock (writer);
#endif

		if (handleViewState)
			scriptManager.RegisterHiddenField ("__VIEWSTATE", _savedViewState);

		scriptManager.WriteHiddenFields (writer);
		if (requiresPostBackScript) {
			RenderPostBackScript (writer, formUniqueID);
			postBackScriptRendered = true;
		}
		scriptManager.WriteClientScriptIncludes (writer);
		scriptManager.WriteClientScriptBlocks (writer);
	}

	internal IStateFormatter GetFormatter ()
	{
		return new ObjectStateFormatter (this);
	}

	internal string GetSavedViewState ()
	{
		return _savedViewState;
	}

	internal void OnFormPostRender (HtmlTextWriter writer, string formUniqueID)
	{
		if (!postBackScriptRendered && requiresPostBackScript)
			RenderPostBackScript (writer, formUniqueID);

		scriptManager.WriteArrayDeclares (writer);
		
#if NET_2_0
		scriptManager.SaveEventValidationState ();
		scriptManager.WriteExpandoAttributes (writer);
#endif
		scriptManager.WriteHiddenFields (writer);
		scriptManager.WriteClientScriptIncludes (writer);
		scriptManager.WriteStartupScriptBlocks (writer);
		renderingForm = false;
		postBackScriptRendered = false;
	}

	private void ProcessPostData (NameValueCollection data, bool second)
	{
		if (data != null) {
			Hashtable used = new Hashtable ();
			foreach (string id in data.AllKeys){
				if (id == "__VIEWSTATE" || id == postEventSourceID || id == postEventArgumentID)
					continue;

				string real_id = id;
				int dot = real_id.IndexOf ('.');
				if (dot >= 1)
					real_id = real_id.Substring (0, dot);
			
				if (real_id == null || used.ContainsKey (real_id))
					continue;

				used.Add (real_id, real_id);

				Control ctrl = FindControl (real_id);
				if (ctrl != null){
					IPostBackDataHandler pbdh = ctrl as IPostBackDataHandler;
					IPostBackEventHandler pbeh = ctrl as IPostBackEventHandler;

					if (pbdh == null) {
						if (pbeh != null)
							RegisterRequiresRaiseEvent (pbeh);
						continue;
					}
		
					if (pbdh.LoadPostData (real_id, data) == true) {
						if (requiresPostDataChanged == null)
							requiresPostDataChanged = new ArrayList ();
						requiresPostDataChanged.Add (pbdh);
					}
				
					if (_requiresPostBackCopy != null)
						_requiresPostBackCopy.Remove (real_id);

				} else if (!second) {
					if (secondPostData == null)
						secondPostData = new NameValueCollection ();
					secondPostData.Add (real_id, data [id]);
				}
			}
		}

		ArrayList list1 = null;
		if (_requiresPostBackCopy != null && _requiresPostBackCopy.Count > 0) {
			string [] handlers = (string []) _requiresPostBackCopy.ToArray (typeof (string));
			foreach (string id in handlers) {
				IPostBackDataHandler pbdh = FindControl (id) as IPostBackDataHandler;
				if (pbdh != null) {			
					_requiresPostBackCopy.Remove (id);
					if (pbdh.LoadPostData (id, data)) {
						if (requiresPostDataChanged == null)
							requiresPostDataChanged = new ArrayList ();
	
						requiresPostDataChanged.Add (pbdh);
					}
				} else if (!second) {
					if (list1 == null)
						list1 = new ArrayList ();
					list1.Add (id);
				}
			}
		}
		_requiresPostBackCopy = second ? null : list1;
		if (second)
			secondPostData = null;
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
#if NET_2_0
	public virtual void ProcessRequest (HttpContext context)
#else
	public void ProcessRequest (HttpContext context)
#endif
	{
#if NET_2_0
		_lifeCycle = PageLifeCycle.Unknown;
#endif
		_context = context;
		if (clientTarget != null)
			Request.ClientTarget = clientTarget;

		WireupAutomaticEvents ();
		//-- Control execution lifecycle in the docs

		// Save culture information because it can be modified in FrameworkInitialize()
		CultureInfo culture = Thread.CurrentThread.CurrentCulture;
		CultureInfo uiculture = Thread.CurrentThread.CurrentUICulture;
		FrameworkInitialize ();
		context.ErrorPage = _errorPage;

		try {
			InternalProcessRequest ();
		} catch (ThreadAbortException) {
			// Do nothing, just ignore it by now.
		} catch (Exception e) {
			context.AddError (e); // OnError might access LastError
			OnError (EventArgs.Empty);
			context.ClearError (e);
			// We want to remove that error, as we're rethrowing to stop
			// further processing.
			Trace.Warn ("Unhandled Exception", e.ToString (), e);
			throw;
		} finally {
			try {
#if NET_2_0
				_lifeCycle = PageLifeCycle.Unload;
#endif
				RenderTrace ();
				UnloadRecursive (true);
#if NET_2_0
				_lifeCycle = PageLifeCycle.End;
#endif
			} catch {}
			if (Thread.CurrentThread.CurrentCulture.Equals (culture) == false)
				Thread.CurrentThread.CurrentCulture = culture;

			if (Thread.CurrentThread.CurrentUICulture.Equals (uiculture) == false)
				Thread.CurrentThread.CurrentUICulture = uiculture;
		}
	}
	
#if NET_2_0
	delegate void ProcessRequestDelegate (HttpContext context);

	private sealed class DummyAsyncResult : IAsyncResult
	{
		readonly object state;
		readonly WaitHandle asyncWaitHandle;
		readonly bool completedSynchronously;
		readonly bool isCompleted;

		public DummyAsyncResult (bool isCompleted, bool completedSynchronously, object state) 
		{
			this.isCompleted = isCompleted;
			this.completedSynchronously = completedSynchronously;
			this.state = state;
			if (isCompleted) {
				asyncWaitHandle = new ManualResetEvent (true);
			}
			else {
				asyncWaitHandle = new ManualResetEvent (false);
			}
		}

		#region IAsyncResult Members

		public object AsyncState {
			get { return state; }
		}

		public WaitHandle AsyncWaitHandle {
			get { return asyncWaitHandle; }
		}

		public bool CompletedSynchronously {
			get { return completedSynchronously; }
		}

		public bool IsCompleted {
			get { return isCompleted; }
		}

		#endregion
	}

	protected IAsyncResult AsyncPageBeginProcessRequest (HttpContext context, AsyncCallback callback, object extraData) 
	{
		ProcessRequest (context);
		DummyAsyncResult asyncResult = new DummyAsyncResult (true, true, extraData);

		if (callback != null) {
			callback (asyncResult);
		}
		
		return asyncResult;
	}

	protected void AsyncPageEndProcessRequest (IAsyncResult result) 
	{
	}

	internal void ProcessCrossPagePostBack (HttpContext context)
	{
		isCrossPagePostBack = true;
		ProcessRequest (context);
	}
#endif

	void InternalProcessRequest ()
	{
		_requestValueCollection = this.DeterminePostBackMode();

#if NET_2_0
		_lifeCycle = PageLifeCycle.Start;
		// http://msdn2.microsoft.com/en-us/library/ms178141.aspx
		if (_requestValueCollection != null) {
			if (!isCrossPagePostBack && _requestValueCollection [PreviousPageID] != null && _requestValueCollection [PreviousPageID] != Request.FilePath) {
				_doLoadPreviousPage = true;
			}
			else {
				isCallback = _requestValueCollection [CallbackArgumentID] != null;
				// LAMESPEC: on Callback IsPostBack is set to false, but true.
				//isPostBack = !isCallback;
				isPostBack = true;
			}
			string lastFocus = _requestValueCollection [LastFocusID];
			if (!String.IsNullOrEmpty (lastFocus)) {
				_focusedControlID = UniqueID2ClientID (lastFocus);
			}
		}
		
		// if request was transfered from other page - track Prev. Page
		previousPage = _context.LastPage;
		_context.LastPage = this;

		_lifeCycle = PageLifeCycle.PreInit;
		OnPreInit (EventArgs.Empty);

		InitializeTheme ();
		ApplyMasterPage ();
		_lifeCycle = PageLifeCycle.Init;
#endif
		Trace.Write ("aspx.page", "Begin Init");
		InitRecursive (null);
		Trace.Write ("aspx.page", "End Init");

#if NET_2_0
		_lifeCycle = PageLifeCycle.InitComplete;
		OnInitComplete (EventArgs.Empty);
#endif
			
		renderingForm = false;	
#if NET_2_0
		if (IsPostBack || IsCallback) {
			_lifeCycle = PageLifeCycle.PreLoad;
			if (_requestValueCollection != null)
				scriptManager.RestoreEventValidationState (_requestValueCollection [scriptManager.EventStateFieldName]);
#else
		if (IsPostBack) {
#endif
			Trace.Write ("aspx.page", "Begin LoadViewState");
			LoadPageViewState ();
			Trace.Write ("aspx.page", "End LoadViewState");
			Trace.Write ("aspx.page", "Begin ProcessPostData");
			ProcessPostData (_requestValueCollection, false);
			Trace.Write ("aspx.page", "End ProcessPostData");
		}

#if NET_2_0
		OnPreLoad (EventArgs.Empty);
		_lifeCycle = PageLifeCycle.Load;
#endif

		LoadRecursive ();
#if NET_2_0
		if (IsPostBack || IsCallback) {
			_lifeCycle = PageLifeCycle.ControlEvents;
#else
		if (IsPostBack) {
#endif
			Trace.Write ("aspx.page", "Begin ProcessPostData Second Try");
			ProcessPostData (secondPostData, true);
			Trace.Write ("aspx.page", "End ProcessPostData Second Try");
			Trace.Write ("aspx.page", "Begin Raise ChangedEvents");
			RaiseChangedEvents ();
			Trace.Write ("aspx.page", "End Raise ChangedEvents");
			Trace.Write ("aspx.page", "Begin Raise PostBackEvent");
			RaisePostBackEvents ();
			Trace.Write ("aspx.page", "End Raise PostBackEvent");
		}
		
#if NET_2_0
		_lifeCycle = PageLifeCycle.LoadComplete;
		OnLoadComplete (EventArgs.Empty);

		if (IsCrossPagePostBack)
			return;

		if (IsCallback) {
			string result = ProcessCallbackData ();
			HtmlTextWriter callbackOutput = new HtmlTextWriter (_context.Response.Output);
			callbackOutput.Write (result);
			callbackOutput.Flush ();
			return;
		}

		_lifeCycle = PageLifeCycle.PreRender;
#endif
		
		Trace.Write ("aspx.page", "Begin PreRender");
		PreRenderRecursiveInternal ();
		Trace.Write ("aspx.page", "End PreRender");
		
#if NET_2_0
		ExecuteRegisteredAsyncTasks ();

		_lifeCycle = PageLifeCycle.PreRenderComplete;
		OnPreRenderComplete (EventArgs.Empty);
#endif

		Trace.Write ("aspx.page", "Begin SaveViewState");
		SavePageViewState ();
		Trace.Write ("aspx.page", "End SaveViewState");
		
#if NET_2_0
		_lifeCycle = PageLifeCycle.SaveStateComplete;
		OnSaveStateComplete (EventArgs.Empty);
#if TARGET_J2EE
		if (OnSaveStateCompleteForPortlet ())
			return;
#endif // TARGET_J2EE
#endif // NET_2_0

#if NET_2_0
		_lifeCycle = PageLifeCycle.Render;
#endif
		
		//--
		Trace.Write ("aspx.page", "Begin Render");
		HtmlTextWriter output = new HtmlTextWriter (_context.Response.Output);
		RenderControl (output);
		Trace.Write ("aspx.page", "End Render");
	}

	private void RenderTrace ()
	{
		TraceManager traceManager = HttpRuntime.TraceManager;

		if (Trace.HaveTrace && !Trace.IsEnabled || !Trace.HaveTrace && !traceManager.Enabled)
			return;
		
		Trace.SaveData ();

		if (!Trace.HaveTrace && traceManager.Enabled && !traceManager.PageOutput) 
			return;

		if (!traceManager.LocalOnly || Context.Request.IsLocal) {
			HtmlTextWriter output = new HtmlTextWriter (_context.Response.Output);
			Trace.Render (output);
		}
	}
	
#if NET_2_0
	bool CheckForValidationSupport (Control targetControl)
	{
		if (targetControl == null)
			return false;
		Type type = targetControl.GetType ();
		object[] attributes = type.GetCustomAttributes (false);
		foreach (object attr in attributes)
			if (attr is SupportsEventValidationAttribute)
				return true;
		return false;
	}
#endif
	
	void RaisePostBackEvents ()
	{
#if NET_2_0
		Control targetControl;
#endif
		if (requiresRaiseEvent != null) {
			RaisePostBackEvent (requiresRaiseEvent, null);
			return;
		}

		NameValueCollection postdata = _requestValueCollection;
		if (postdata == null)
			return;

		string eventTarget = postdata [postEventSourceID];
		if (eventTarget == null || eventTarget.Length == 0) {
			Validate ();
			return;
                }

#if NET_2_0
		targetControl = FindControl (eventTarget);
		IPostBackEventHandler target = targetControl as IPostBackEventHandler;
#else
		IPostBackEventHandler target = FindControl (eventTarget) as IPostBackEventHandler;
#endif
			
		if (target == null)
			return;

		string eventArgument = postdata [postEventArgumentID];
		RaisePostBackEvent (target, eventArgument);
	}

	internal void RaiseChangedEvents ()
	{
		if (requiresPostDataChanged == null)
			return;

		foreach (IPostBackDataHandler ipdh in requiresPostDataChanged)
			ipdh.RaisePostDataChangedEvent ();

		requiresPostDataChanged = null;
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	protected virtual void RaisePostBackEvent (IPostBackEventHandler sourceControl, string eventArgument)
	{
#if NET_2_0
		Control targetControl = sourceControl as Control;
		if (targetControl != null && CheckForValidationSupport (targetControl))
			scriptManager.ValidateEvent (targetControl.UniqueID, eventArgument);
#endif
		sourceControl.RaisePostBackEvent (eventArgument);
	}
	
#if NET_2_0
	[Obsolete]
#endif
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void RegisterArrayDeclaration (string arrayName, string arrayValue)
	{
		scriptManager.RegisterArrayDeclaration (arrayName, arrayValue);
	}

#if NET_2_0
	[Obsolete]
#endif
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void RegisterClientScriptBlock (string key, string script)
	{
		scriptManager.RegisterClientScriptBlock (key, script);
	}

#if NET_2_0
	[Obsolete]
#endif
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void RegisterHiddenField (string hiddenFieldName, string hiddenFieldInitialValue)
	{
		scriptManager.RegisterHiddenField (hiddenFieldName, hiddenFieldInitialValue);
	}

	[MonoTODO("Not implemented, Used in HtmlForm")]
	internal void RegisterClientScriptFile (string a, string b, string c)
	{
		throw new NotImplementedException ();
	}

#if NET_2_0
	[Obsolete]
#endif
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void RegisterOnSubmitStatement (string key, string script)
	{
		scriptManager.RegisterOnSubmitStatement (key, script);
	}

	internal string GetSubmitStatements ()
	{
		return scriptManager.WriteSubmitStatements ();
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void RegisterRequiresPostBack (Control control)
	{
#if NET_2_0
		if (!(control is IPostBackDataHandler))
			throw new HttpException ("The control to register does not implement the IPostBackDataHandler interface.");
#endif
		
		if (_requiresPostBack == null)
			_requiresPostBack = new ArrayList ();

		if (_requiresPostBack.Contains (control.UniqueID))
			return;

		_requiresPostBack.Add (control.UniqueID);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void RegisterRequiresRaiseEvent (IPostBackEventHandler control)
	{
		requiresRaiseEvent = control;
	}

#if NET_2_0
	[Obsolete]
#endif
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void RegisterStartupScript (string key, string script)
	{
		scriptManager.RegisterStartupScript (key, script);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void RegisterViewStateHandler ()
	{
		handleViewState = true;
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	protected virtual void SavePageStateToPersistenceMedium (object viewState)
	{
		PageStatePersister persister = this.PageStatePersister;
		if (persister == null)
			return;
		Pair pair = viewState as Pair;
		if (pair != null) {
			persister.ViewState = pair.First;
			persister.ControlState = pair.Second;
		} else
			persister.ViewState = viewState;
		persister.Save ();
	}

	internal string RawViewState {
		get {
			NameValueCollection postdata = _requestValueCollection;
			string view_state;
			if (postdata == null || (view_state = postdata ["__VIEWSTATE"]) == null)
				return null;

			if (view_state == "")
				return null;
			return view_state;
		}
		set { _savedViewState = value; }
	}

#if NET_2_0
	protected virtual 
#else
	internal
#endif
	PageStatePersister PageStatePersister {
		get {
			if (page_state_persister == null)
				page_state_persister = new HiddenFieldPageStatePersister (this);
			return page_state_persister;
		}
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	protected virtual object LoadPageStateFromPersistenceMedium ()
	{
		PageStatePersister persister = this.PageStatePersister;
		if (persister == null)
			return null;
		persister.Load ();
		return new Pair (persister.ViewState, persister.ControlState);
	}

	internal void LoadPageViewState()
	{
		Pair sState = LoadPageStateFromPersistenceMedium () as Pair;
		if (sState != null) {
			if (allow_load) {
#if NET_2_0
				LoadPageControlState (sState.Second);
#endif
				Pair vsr = sState.First as Pair;
				if (vsr != null) {
					LoadViewStateRecursive (vsr.First);
					_requiresPostBackCopy = vsr.Second as ArrayList;
				}
			}
		}
	}

	internal void SavePageViewState ()
	{
		if (!handleViewState)
			return;

#if NET_2_0
		object controlState = SavePageControlState ();
#endif

		object viewState = SaveViewStateRecursive ();
		object reqPostback = (_requiresPostBack != null && _requiresPostBack.Count > 0) ? _requiresPostBack : null;
		Pair vsr = null;

		if (viewState != null || reqPostback != null)
			vsr = new Pair (viewState, reqPostback);
		Pair pair = new Pair ();

		pair.First = vsr;
#if NET_2_0
		pair.Second = controlState;
#else
		pair.Second = null;
#endif
		if (pair.First == null && pair.Second == null)
			SavePageStateToPersistenceMedium (null);
		else
			SavePageStateToPersistenceMedium (pair);		

	}

	public virtual void Validate ()
	{
		is_validated = true;
		ValidateCollection (_validators);
	}

#if NET_2_0
	internal bool AreValidatorsUplevel () {
		return AreValidatorsUplevel (String.Empty);
	}

	internal bool AreValidatorsUplevel (string valGroup)
#else
	internal virtual bool AreValidatorsUplevel ()
#endif
	{
		bool uplevel = false;

		foreach (IValidator v in Validators) {
			BaseValidator bv = v as BaseValidator;
			if (bv == null) continue;

#if NET_2_0
			if (valGroup != bv.ValidationGroup)
				continue;
#endif
			if (bv.GetRenderUplevel()) {
				uplevel = true;
				break;
			}
		}

		return uplevel;
	}

	bool ValidateCollection (ValidatorCollection validators)
	{
#if NET_2_0
		if (!_eventValidation)
			return true;
#endif

		if (validators == null || validators.Count == 0)
			return true;

		bool all_valid = true;
		foreach (IValidator v in validators){
			v.Validate ();
			if (v.IsValid == false)
				all_valid = false;
		}

		return all_valid;
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void VerifyRenderingInServerForm (Control control)
	{
		if (_context == null)
			return;
#if NET_2_0
		if (IsCallback)
			return;
#endif
		if (!renderingForm)
			throw new HttpException ("Control '" +
						 control.ClientID +
						 "' of type '" +
						 control.GetType ().Name +
						 "' must be placed inside a form tag with runat=server.");
	}

	protected override void FrameworkInitialize ()
	{
		base.FrameworkInitialize ();
#if NET_2_0
		InitializeStyleSheet ();
#endif
	}

	#endregion
	
	#if NET_2_0
	public
	#else
	internal
	#endif
		ClientScriptManager ClientScript {
		get { return scriptManager; }
	}
	
	#if NET_2_0
	
	static readonly object InitCompleteEvent = new object ();
	static readonly object LoadCompleteEvent = new object ();
	static readonly object PreInitEvent = new object ();
	static readonly object PreLoadEvent = new object ();
	static readonly object PreRenderCompleteEvent = new object ();
	static readonly object SaveStateCompleteEvent = new object ();
	int event_mask;
	const int initcomplete_mask = 1;
	const int loadcomplete_mask = 1 << 1;
	const int preinit_mask = 1 << 2;
	const int preload_mask = 1 << 3;
	const int prerendercomplete_mask = 1 << 4;
	const int savestatecomplete_mask = 1 << 5;
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public event EventHandler InitComplete {
		add {
			event_mask |= initcomplete_mask;
			Events.AddHandler (InitCompleteEvent, value);
		}
		remove { Events.RemoveHandler (InitCompleteEvent, value); }
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public event EventHandler LoadComplete {
		add {
			event_mask |= loadcomplete_mask;
			Events.AddHandler (LoadCompleteEvent, value);
		}
		remove { Events.RemoveHandler (LoadCompleteEvent, value); }
	}
	
	public event EventHandler PreInit {
		add {
			event_mask |= preinit_mask;
			Events.AddHandler (PreInitEvent, value);
		}
		remove { Events.RemoveHandler (PreInitEvent, value); }
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public event EventHandler PreLoad {
		add {
			event_mask |= preload_mask;
			Events.AddHandler (PreLoadEvent, value);
		}
		remove { Events.RemoveHandler (PreLoadEvent, value); }
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public event EventHandler PreRenderComplete {
		add {
			event_mask |= prerendercomplete_mask;
			Events.AddHandler (PreRenderCompleteEvent, value);
		}
		remove { Events.RemoveHandler (PreRenderCompleteEvent, value); }
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public event EventHandler SaveStateComplete {
		add {
			event_mask |= savestatecomplete_mask;
			Events.AddHandler (SaveStateCompleteEvent, value);
		}
		remove { Events.RemoveHandler (SaveStateCompleteEvent, value); }
	}
	
	protected virtual void OnInitComplete (EventArgs e)
	{
		if ((event_mask & initcomplete_mask) != 0) {
			EventHandler eh = (EventHandler) (Events [InitCompleteEvent]);
			if (eh != null) eh (this, e);
		}
	}
	
	protected virtual void OnLoadComplete (EventArgs e)
	{
		if ((event_mask & loadcomplete_mask) != 0) {
			EventHandler eh = (EventHandler) (Events [LoadCompleteEvent]);
			if (eh != null) eh (this, e);
		}
	}
	
	protected virtual void OnPreInit (EventArgs e)
	{
		if ((event_mask & preinit_mask) != 0) {
			EventHandler eh = (EventHandler) (Events [PreInitEvent]);
			if (eh != null) eh (this, e);
		}
	}
	
	protected virtual void OnPreLoad (EventArgs e)
	{
		if ((event_mask & preload_mask) != 0) {
			EventHandler eh = (EventHandler) (Events [PreLoadEvent]);
			if (eh != null) eh (this, e);
		}
	}
	
	protected virtual void OnPreRenderComplete (EventArgs e)
	{
		if ((event_mask & prerendercomplete_mask) != 0) {
			EventHandler eh = (EventHandler) (Events [PreRenderCompleteEvent]);
			if (eh != null) eh (this, e);
		}

		if (Form == null)
			return;
		if (!Form.DetermineRenderUplevel ())
			return;

		/* figure out if we have some control we're going to focus */
		if (String.IsNullOrEmpty (_focusedControlID)) {
			_focusedControlID = Form.DefaultFocus;
			if (String.IsNullOrEmpty (_focusedControlID))
				_focusedControlID = Form.DefaultButton;
		}

			if (!String.IsNullOrEmpty (_focusedControlID)) {
				ClientScript.RegisterWebFormClientScript ();
				ClientScript.RegisterStartupScript ("HtmlForm-DefaultButton-StartupScript",
									 String.Format ("<script type=\"text/javascript\">\n" +
											"<!--\n" +
											"WebForm_AutoFocus('{0}');// -->\n" +
											"</script>\n", _focusedControlID));
			}

			if (Form.SubmitDisabledControls && _hasEnabledControlArray) {
				ClientScript.RegisterWebFormClientScript ();
				ClientScript.RegisterOnSubmitStatement ("HtmlForm-SubmitDisabledControls-SubmitStatement",
										 "WebForm_ReEnableControls(this);");
			}
	}

	internal void RegisterEnabledControl (Control control)
	{
		if (Form == null || !Page.Form.SubmitDisabledControls || !Page.Form.DetermineRenderUplevel ())
			return;
		_hasEnabledControlArray = true;
		Page.ClientScript.RegisterArrayDeclaration (EnabledControlArrayID, String.Format ("'{0}'", control.ClientID));
	}
	
	protected virtual void OnSaveStateComplete (EventArgs e)
	{
		if ((event_mask & savestatecomplete_mask) != 0) {
			EventHandler eh = (EventHandler) (Events [SaveStateCompleteEvent]);
			if (eh != null) eh (this, e);
		}
	}
	
	public HtmlForm Form {
		get { return _form; }
	}
	
	internal void RegisterForm (HtmlForm form)
	{
		_form = form;
	}
	
	[BrowsableAttribute (false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public Page PreviousPage {
		get {
			if (_doLoadPreviousPage) {
				_doLoadPreviousPage = false;
				LoadPreviousPageReference ();
			}
			return previousPage;
		}
	}

	
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public bool IsCallback {
		get { return isCallback; }
	}
	
	[BrowsableAttribute (false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public bool IsCrossPagePostBack {
		get { return isCrossPagePostBack; }
	}

	public new virtual char IdSeparator {
		get {
			//TODO: why override?
			return base.IdSeparator;
		}
	}
	
	string ProcessCallbackData ()
	{
		string callbackTarget = _requestValueCollection [CallbackSourceID];
		if (callbackTarget == null || callbackTarget.Length == 0)
			throw new HttpException ("Callback target not provided.");

		Control targetControl = FindControl (callbackTarget);
		ICallbackEventHandler target = targetControl as ICallbackEventHandler;
		if (target == null)
			throw new HttpException (string.Format ("Invalid callback target '{0}'.", callbackTarget));

		string callbackEventError = String.Empty;
		string callBackResult;
		string callbackArgument = _requestValueCollection [CallbackArgumentID];

		try {
			target.RaiseCallbackEvent (callbackArgument);
		}
		catch (Exception ex) {
			callbackEventError = String.Format ("e{0}", ex.Message);
		}
		
		try {
			callBackResult = target.GetCallbackResult ();
		}
		catch (Exception ex) {
			return String.Format ("e{0}", ex.Message);
		}
		
		string eventValidation = ClientScript.GetEventValidationStateFormatted ();
		return String.Format ("{0}{1}|{2}{3}", callbackEventError, eventValidation == null ? 0 : eventValidation.Length, eventValidation, callBackResult);
	}

	[BrowsableAttribute (false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public HtmlHead Header {
		get { return htmlHeader; }
	}
	
	internal void SetHeader (HtmlHead header)
	{
		htmlHeader = header;
		if (_title != null) {
			htmlHeader.Title = _title;
			_title = null;
		}
	}

	protected bool AsyncMode {
		get { return asyncMode; }
		set { asyncMode = value; }
	}

	public TimeSpan AsyncTimeout {
		get { return asyncTimeout; }
		set { asyncTimeout = value; }
	}

	public bool IsAsync {
		get { return AsyncMode; }
	}
	
	[MonoTODO ("Not Implemented")]
	protected internal virtual string UniqueFilePathSuffix {
		get {
			throw new NotImplementedException ();
		}
	}

	[MonoTODO ("Not Implemented")]
	public int MaxPageStateFieldLength {
		get {
			throw new NotImplementedException ();
		}
		set {
			throw new NotImplementedException ();
		}
	}

	public void AddOnPreRenderCompleteAsync (BeginEventHandler beginHandler, EndEventHandler endHandler)
	{
		AddOnPreRenderCompleteAsync (beginHandler, endHandler, null);
	}

	public void AddOnPreRenderCompleteAsync (BeginEventHandler beginHandler, EndEventHandler endHandler, Object state)
	{
		if (!IsAsync) {
			throw new InvalidOperationException ("AddOnPreRenderCompleteAsync called and Page.IsAsync == false");
		}

		if (_lifeCycle >= PageLifeCycle.PreRender) {
			throw new InvalidOperationException ("AddOnPreRenderCompleteAsync can only be called before PreRender.");
		}

		if (beginHandler == null) {
			throw new ArgumentNullException ("beginHandler");
		}

		if (endHandler == null) {
			throw new ArgumentNullException ("endHandler");
		}

		RegisterAsyncTask (new PageAsyncTask (beginHandler, endHandler, null, state, false));
	}

	private List<PageAsyncTask> ParallelTasks {
		get
		{
			if (parallelTasks == null) {
				parallelTasks = new List<PageAsyncTask>();
			}
			return parallelTasks;
		}
	}

	private List<PageAsyncTask> SerialTasks {
		get {
			if (serialTasks == null) {
				serialTasks = new List<PageAsyncTask> ();
			}
			return serialTasks;
		}
	}

	public void RegisterAsyncTask (PageAsyncTask task) 
	{
		if (task == null) {
			throw new ArgumentNullException ("task");
		}

		if (task.ExecuteInParallel) {
			ParallelTasks.Add (task);
		}
		else {
			SerialTasks.Add (task);
		}
	}

	public void ExecuteRegisteredAsyncTasks ()
	{
		if ((parallelTasks == null || parallelTasks.Count == 0) &&
			(serialTasks == null || serialTasks.Count == 0)){
			return;
		}

		if (parallelTasks != null) {
			DateTime startExecution = DateTime.Now;
			List<PageAsyncTask> localParallelTasks = parallelTasks;
			parallelTasks = null; // Shouldn't execute tasks twice
			List<IAsyncResult> asyncResults = new List<IAsyncResult>();
			foreach (PageAsyncTask parallelTask in localParallelTasks) {
				IAsyncResult result = parallelTask.BeginHandler (this, EventArgs.Empty, new AsyncCallback (EndAsyncTaskCallback), parallelTask.State);
				if (result.CompletedSynchronously) {
					parallelTask.EndHandler (result);
				}
				else {
					asyncResults.Add (result);
				}
			}

			if (asyncResults.Count > 0) {
#if TARGET_JVM
				TimeSpan timeout = AsyncTimeout;
				long t1 = DateTime.Now.Ticks;
				bool signalled = true;
				for (int i = 0; i < asyncResults.Count; i++) {
					if (asyncResults [i].IsCompleted)
						continue;

					if (signalled)
						signalled = asyncResults [i].AsyncWaitHandle.WaitOne (timeout, false);

					if (signalled) {
						long t2 = DateTime.Now.Ticks;
						timeout = AsyncTimeout - TimeSpan.FromTicks (t2 - t1);
						if (timeout.Ticks <= 0)
							signalled = false;
					}
					else {
						localParallelTasks [i].TimeoutHandler (asyncResults [i]);
					}
				}
#else
				WaitHandle [] waitArray = new WaitHandle [asyncResults.Count];
				int i = 0;
				for (i = 0; i < asyncResults.Count; i++) {
					waitArray [i] = asyncResults [i].AsyncWaitHandle;
				}
				bool allSignalled = WaitHandle.WaitAll (waitArray, AsyncTimeout, false);
				if (!allSignalled) {
					for (i = 0; i < asyncResults.Count; i++) {
						if (!asyncResults [i].IsCompleted) {
							localParallelTasks [i].TimeoutHandler (asyncResults [i]);
						}
					}
				}
#endif
			}
			DateTime endWait = DateTime.Now;
			TimeSpan elapsed = endWait - startExecution;
			if (elapsed <= AsyncTimeout) {
				AsyncTimeout -= elapsed;
			}
			else {
				AsyncTimeout = TimeSpan.FromTicks(0);
			}
		}

		if (serialTasks != null) {
			List<PageAsyncTask> localSerialTasks = serialTasks;
			serialTasks = null; // Shouldn't execute tasks twice
			foreach (PageAsyncTask serialTask in localSerialTasks) {
				DateTime startExecution = DateTime.Now;

				IAsyncResult result = serialTask.BeginHandler (this, EventArgs.Empty, new AsyncCallback (EndAsyncTaskCallback), serialTask);
				if (result.CompletedSynchronously) {
					serialTask.EndHandler (result);
				}
				else {
					bool done = result.AsyncWaitHandle.WaitOne (AsyncTimeout, false);
					if (!done && !result.IsCompleted) {
						serialTask.TimeoutHandler (result);
					}
				}
				DateTime endWait = DateTime.Now;
				TimeSpan elapsed = endWait - startExecution;
				if (elapsed <= AsyncTimeout) {
					AsyncTimeout -= elapsed;
				}
				else {
					AsyncTimeout = TimeSpan.FromTicks (0);
				}
			}
		}
		AsyncTimeout = TimeSpan.FromSeconds (DefaultAsyncTimeout);
	}

	void EndAsyncTaskCallback (IAsyncResult result) 
	{
		PageAsyncTask task = (PageAsyncTask)result.AsyncState;
		task.EndHandler (result);
	}

	public static HtmlTextWriter CreateHtmlTextWriterFromType (TextWriter tw, Type writerType)
	{
		Type htmlTextWriterType = typeof (HtmlTextWriter);
		
		if (!htmlTextWriterType.IsAssignableFrom (writerType)) {
			throw new HttpException (String.Format ("Type '{0}' cannot be assigned to HtmlTextWriter", writerType.FullName));
		}

		ConstructorInfo constructor = writerType.GetConstructor (new Type [] { typeof (TextWriter) });
		if (constructor == null) {
			throw new HttpException (String.Format ("Type '{0}' does not have a consturctor that takes a TextWriter as parameter", writerType.FullName));
		}

		return (HtmlTextWriter) Activator.CreateInstance(writerType, tw);
	}

	public ViewStateEncryptionMode ViewStateEncryptionMode {
		get { return viewStateEncryptionMode; }
		set { viewStateEncryptionMode = value; }
	}

	public void RegisterRequiresViewStateEncryption ()
	{
		controlRegisteredForViewStateEncryption = true;
	}

	private static byte [] AES_IV = null;
	private static byte [] TripleDES_IV = null;
	private static object locker = new object ();
	private static bool isEncryptionInitialized = false;

	private static void InitializeEncryption () 
	{
		if (isEncryptionInitialized) {
			return;
		}

		lock (locker) {
			if (isEncryptionInitialized) {
				return;
			}

			string iv_string = "0BA48A9E-736D-40f8-954B-B2F62241F282";
			AES_IV = new byte [16];
			TripleDES_IV = new byte [8];

			int i;
			for (i = 0; i < AES_IV.Length; i++) {
				AES_IV [i] = (byte) iv_string [i];
			}

			for (i = 0; i < TripleDES_IV.Length; i++) {
				TripleDES_IV [i] = (byte) iv_string [i];
			}

			isEncryptionInitialized = true;
		}
	}

	internal ICryptoTransform GetCryptoTransform (CryptoStreamMode cryptoStreamMode) 
	{
		ICryptoTransform transform = null;
		MachineKeySection config = (MachineKeySection) WebConfigurationManager.GetSection (machineKeyConfigPath);
		byte [] vk = config.ValidationKeyBytes;

		switch (config.Validation) {
		case MachineKeyValidation.SHA1:
			transform = SHA1.Create ();
			break;

		case MachineKeyValidation.MD5:
			transform = MD5.Create ();
			break;

		case MachineKeyValidation.AES:
			if (cryptoStreamMode == CryptoStreamMode.Read){
				transform = Rijndael.Create().CreateDecryptor(vk, AES_IV);
			} else {
				transform = Rijndael.Create().CreateEncryptor(vk, AES_IV);
			}
			break;

		case MachineKeyValidation.TripleDES:
			if (cryptoStreamMode == CryptoStreamMode.Read){
				transform = TripleDES.Create().CreateDecryptor(vk, TripleDES_IV);
			} else {
				transform = TripleDES.Create().CreateEncryptor(vk, TripleDES_IV);
			}
			break;
		}

		return transform;
	}

	internal bool NeedViewStateEncryption {
		get {
			return (ViewStateEncryptionMode == ViewStateEncryptionMode.Always ||
					(ViewStateEncryptionMode == ViewStateEncryptionMode.Auto &&
					 controlRegisteredForViewStateEncryption));

		}
	}

	void ApplyMasterPage ()
	{
		if (masterPageFile != null && masterPageFile.Length > 0) {
			ArrayList appliedMasterPageFiles = new ArrayList ();

			if (Master != null) {
				MasterPage.ApplyMasterPageRecursive (Master, appliedMasterPageFiles);

				Master.Page = this;
				Controls.Clear ();
				Controls.Add (Master);
			}
		}
	}

	[DefaultValueAttribute ("")]
	public virtual string MasterPageFile {
		get { return masterPageFile; }
		set {
			masterPageFile = value;
			masterPage = null;
		}
	}
	
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public MasterPage Master {
		get {
			if (Context == null || String.IsNullOrEmpty (masterPageFile))
				return null;

			if (masterPage == null)
				masterPage = MasterPage.CreateMasterPage (this, Context, masterPageFile, contentTemplates);

			return masterPage;
		}
	}
	
	public void SetFocus (string clientID)
	{
		if (String.IsNullOrEmpty (clientID))
			throw new ArgumentNullException ("control");

		if (_lifeCycle > PageLifeCycle.PreRender)
			throw new InvalidOperationException ("SetFocus can only be called before and during PreRender.");

		if(Form==null)
			throw new InvalidOperationException ("A form tag with runat=server must exist on the Page to use SetFocus() or the Focus property.");

		_focusedControlID = clientID;
	}

	public void SetFocus (Control control)
	{
		if (control == null)
			throw new ArgumentNullException ("control");

		SetFocus (control.ClientID);
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void RegisterRequiresControlState (Control control)
	{
		if (control == null)
			throw new ArgumentNullException ("control");

		if (RequiresControlState (control))
			return;

		if (requireStateControls == null)
			requireStateControls = new ArrayList ();
		int n = requireStateControls.Add (control);

		if (_savedControlState == null || n >= _savedControlState.Length) 
			return;

		for (Control parent = control.Parent; parent != null; parent = parent.Parent)
			if (parent.IsChildControlStateCleared)
				return;

		object state = _savedControlState [n];
		if (state != null)
			control.LoadControlState (state);
	}
	
	public bool RequiresControlState (Control control)
	{
		if (requireStateControls == null) return false;
		return requireStateControls.Contains (control);
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void UnregisterRequiresControlState (Control control)
	{
		if (requireStateControls != null)
			requireStateControls.Remove (control);
	}
	
	public ValidatorCollection GetValidators (string validationGroup)
	{
		string valgr = validationGroup;
		if (valgr == null)
			valgr = String.Empty;

		if (_validatorsByGroup == null) _validatorsByGroup = new Hashtable ();
		ValidatorCollection col = _validatorsByGroup [valgr] as ValidatorCollection;
		if (col == null) {
			col = new ValidatorCollection ();
			_validatorsByGroup [valgr] = col;
		}
		return col;
	}
	
	public virtual void Validate (string validationGroup)
	{
		is_validated = true;
		if (validationGroup == null)
			ValidateCollection (_validatorsByGroup [String.Empty] as ValidatorCollection);
		else if (_validatorsByGroup != null) {
			ValidateCollection (_validatorsByGroup [validationGroup] as ValidatorCollection);
		}
	}

	object SavePageControlState ()
	{
		if (requireStateControls == null) return null;
		object[] state = new object [requireStateControls.Count];
		
		bool allNull = true;
		for (int n=0; n<state.Length; n++) {
			state [n] = ((Control) requireStateControls [n]).SaveControlState ();
			if (state [n] != null) allNull = false;
		}
		if (allNull) return null;
		else return state;
	}
	
	void LoadPageControlState (object data)
	{
		_savedControlState = (object []) data;
		
		if (requireStateControls == null) return;

		int max = Math.Min (requireStateControls.Count, _savedControlState != null ? _savedControlState.Length : requireStateControls.Count);
		for (int n=0; n < max; n++) {
			Control ctl = (Control) requireStateControls [n];
			ctl.LoadControlState (_savedControlState != null ? _savedControlState [n] : null);
		}
	}

	void LoadPreviousPageReference ()
	{
		if (_requestValueCollection != null) {
			string prevPage = _requestValueCollection [PreviousPageID];
			if (prevPage != null) {
				previousPage = (Page) PageParser.GetCompiledPageInstance (prevPage, Server.MapPath (prevPage), Context);
				previousPage.ProcessCrossPagePostBack (_context);
			} 
		}
	}


	Hashtable contentTemplates;
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected internal void AddContentTemplate (string templateName, ITemplate template)
	{
		if (contentTemplates == null)
			contentTemplates = new Hashtable ();
		contentTemplates [templateName] = template;
	}

	PageTheme _pageTheme;
	internal PageTheme PageTheme {
		get { return _pageTheme; }
	}

	PageTheme _styleSheetPageTheme;
	internal PageTheme StyleSheetPageTheme {
		get { return _styleSheetPageTheme; }
	}

	Stack dataItemCtx;
	
	internal void PushDataItemContext (object o) {
		if (dataItemCtx == null)
			dataItemCtx = new Stack ();
		
		dataItemCtx.Push (o);
	}
	
	internal void PopDataItemContext () {
		if (dataItemCtx == null)
			throw new InvalidOperationException ();
		
		dataItemCtx.Pop ();
	}
	
	public object GetDataItem() {
		if (dataItemCtx == null || dataItemCtx.Count == 0)
			throw new InvalidOperationException ("No data item");
		
		return dataItemCtx.Peek ();
	}

	protected internal override void OnInit (EventArgs e)
	{
		base.OnInit (e);

		ArrayList themes = new ArrayList();

		if (StyleSheetPageTheme != null && StyleSheetPageTheme.GetStyleSheets () != null)
			themes.AddRange (StyleSheetPageTheme.GetStyleSheets ());
		if (PageTheme != null && PageTheme.GetStyleSheets () != null)
			themes.AddRange (PageTheme.GetStyleSheets ());

		if (themes.Count > 0 && Header == null)
			throw new InvalidOperationException ("Using themed css files requires a header control on the page.");

		foreach (string lss in themes) {
			HtmlLink hl = new HtmlLink ();
			hl.Href = ResolveUrl (lss);
			hl.Attributes["type"] = "text/css";
			hl.Attributes["rel"] = "stylesheet";
			Header.Controls.Add (hl);
		}
	}

	#endif

#if NET_2_0
	[MonoTODO ("Not implemented.  Only used by .net aspx parser")]
	protected object GetWrappedFileDependencies (string [] list)
	{
		return list;
	}

	[MonoTODO ("Does nothing.  Used by .net aspx parser")]
	protected virtual void InitializeCulture ()
	{
	}

	[MonoTODO ("Does nothing. Used by .net aspx parser")]
	protected internal void AddWrappedFileDependencies (object virtualFileDependencies)
	{
	}
#endif
}
}
