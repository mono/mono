//
// System.Web.UI.Page.cs
//
// Authors:
//   Duncan Mak  (duncan@ximian.com)
//   Gonzalo Paniagua (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2002,2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2003-2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.SessionState;
using System.Web.Util;
using System.Web.UI.Adapters;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Web.Routing;

namespace System.Web.UI
{
// CAS
[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
[DefaultEvent ("Load"), DesignerCategory ("ASPXCodeBehind")]
[ToolboxItem (false)]
[Designer ("Microsoft.VisualStudio.Web.WebForms.WebFormDesigner, " + Consts.AssemblyMicrosoft_VisualStudio_Web, typeof (IRootDesigner))]
[DesignerSerializer ("Microsoft.VisualStudio.Web.WebForms.WebFormCodeDomSerializer, " + Consts.AssemblyMicrosoft_VisualStudio_Web, "System.ComponentModel.Design.Serialization.TypeCodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
public partial class Page : TemplateControl, IHttpHandler
{
//	static string machineKeyConfigPath = "system.web/machineKey";
	bool _eventValidation = true;
	object [] _savedControlState;
	bool _doLoadPreviousPage;
	string _focusedControlID;
	bool _hasEnabledControlArray;
	bool _viewState;
	bool _viewStateMac;
	string _errorPage;
	bool is_validated;
	bool _smartNavigation;
	int _transactionMode;
	ValidatorCollection _validators;
	bool renderingForm;
	string _savedViewState;
	List <string> _requiresPostBack;
	List <string> _requiresPostBackCopy;
	List <IPostBackDataHandler> requiresPostDataChanged;
	IPostBackEventHandler requiresRaiseEvent;
	IPostBackEventHandler formPostedRequiresRaiseEvent;
	NameValueCollection secondPostData;
	bool requiresPostBackScript;
	bool postBackScriptRendered;
	bool requiresFormScriptDeclaration;
	bool formScriptDeclarationRendered;
	bool handleViewState;
	string viewStateUserKey;
	NameValueCollection _requestValueCollection;
	string clientTarget;
	ClientScriptManager scriptManager;
	bool allow_load; // true when the Form collection belongs to this page (GetTypeHashCode)
	PageStatePersister page_state_persister;
	CultureInfo _appCulture;
	CultureInfo _appUICulture;

	// The initial context
	HttpContext _context;
	
	// cached from the initial context
	HttpApplicationState _application;
	HttpResponse _response;
	HttpRequest _request;
	Cache _cache;
	
	HttpSessionState _session;
	
	[EditorBrowsable (EditorBrowsableState.Never)]
	public const string postEventArgumentID = "__EVENTARGUMENT";

	[EditorBrowsable (EditorBrowsableState.Never)]
	public const string postEventSourceID = "__EVENTTARGET";

	const string ScrollPositionXID = "__SCROLLPOSITIONX";
	const string ScrollPositionYID = "__SCROLLPOSITIONY";
	const string EnabledControlArrayID = "__enabledControlArray";
	internal const string LastFocusID = "__LASTFOCUS";
	internal const string CallbackArgumentID = "__CALLBACKPARAM";
	internal const string CallbackSourceID = "__CALLBACKID";
	internal const string PreviousPageID = "__PREVIOUSPAGE";

	int maxPageStateFieldLength = -1;
	string uniqueFilePathSuffix;
	HtmlHead htmlHeader;
	
	MasterPage masterPage;
	string masterPageFile;
	
	Page previousPage;
	bool isCrossPagePostBack;
	bool isPostBack;
	bool isCallback;
	List <Control> requireStateControls;
	HtmlForm _form;

	string _title;
	string _theme;
	string _styleSheetTheme;
	string _metaDescription;
	string _metaKeywords;
	Control _autoPostBackControl;
	
	bool frameworkInitialized;
	Hashtable items;

	bool _maintainScrollPositionOnPostBack;

	bool asyncMode = false;
	TimeSpan asyncTimeout;
	const double DefaultAsyncTimeout = 45.0;
	List<PageAsyncTask> parallelTasks;
	List<PageAsyncTask> serialTasks;

	ViewStateEncryptionMode viewStateEncryptionMode;
	bool controlRegisteredForViewStateEncryption = false;

	#region Constructors	
	public Page ()
	{
		scriptManager = new ClientScriptManager (this);
		Page = this;
		ID = "__Page";
		PagesSection ps = WebConfigurationManager.GetSection ("system.web/pages") as PagesSection;
		if (ps != null) {
			asyncTimeout = ps.AsyncTimeout;
			viewStateEncryptionMode = ps.ViewStateEncryptionMode;
			_viewState = ps.EnableViewState;
			_viewStateMac = ps.EnableViewStateMac;
		} else {
			asyncTimeout = TimeSpan.FromSeconds (DefaultAsyncTimeout);
			viewStateEncryptionMode = ViewStateEncryptionMode.Auto;
			_viewState = true;
		}
		this.ViewStateMode = ViewStateMode.Enabled;
	}

	#endregion		

	#region Properties

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public HttpApplicationState Application {
		get { return _application; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected bool AspCompatMode {
		get { return false; }
		set {
			// nothing to do
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public bool Buffer {
		get { return Response.BufferOutput; }
		set { Response.BufferOutput = value; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public Cache Cache {
		get {
			if (_cache == null)
				throw new HttpException ("Cache is not available.");
			return _cache;
		}
	}

	[EditorBrowsableAttribute (EditorBrowsableState.Advanced)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false), DefaultValue ("")]
	[WebSysDescription ("Value do override the automatic browser detection and force the page to use the specified browser.")]
	public string ClientTarget {
		get { return (clientTarget == null) ? String.Empty : clientTarget; }
		set {
			clientTarget = value;
			if (value == String.Empty)
				clientTarget = null;
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public int CodePage {
		get { return Response.ContentEncoding.CodePage; }
		set { Response.ContentEncoding = Encoding.GetEncoding (value); }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public string ContentType {
		get { return Response.ContentType; }
		set { Response.ContentType = value; }
	}

	protected internal override HttpContext Context {
		get {
			if (_context == null)
				return HttpContext.Current;

			return _context;
		}
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public string Culture {
		get { return Thread.CurrentThread.CurrentCulture.Name; }
		set { Thread.CurrentThread.CurrentCulture = GetPageCulture (value, Thread.CurrentThread.CurrentCulture); }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[Browsable (false)]
	[DefaultValue ("true")]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public virtual bool EnableEventValidation {
		get { return _eventValidation; }
		set {
			if (IsInited)
				throw new InvalidOperationException ("The 'EnableEventValidation' property can be set only in the Page_init, the Page directive or in the <pages> configuration section.");
			_eventValidation = value;
		}
	}

	[Browsable (false)]
	public override bool EnableViewState {
		get { return _viewState; }
		set { _viewState = value; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	[EditorBrowsable (EditorBrowsableState.Never)]
	public bool EnableViewStateMac {
		get { return _viewStateMac; }
		set { _viewStateMac = value; }
	}

	internal bool EnableViewStateMacInternal {
		get { return _viewStateMac; }
		set { _viewStateMac = value; }
	}
	
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false), DefaultValue ("")]
	[WebSysDescription ("The URL of a page used for error redirection.")]
	public string ErrorPage {
		get { return _errorPage; }
		set {
			HttpContext ctx = Context;
			
			_errorPage = value;
			if (ctx != null)
				ctx.ErrorPage = value;
		}
	}

	[Obsolete ("The recommended alternative is HttpResponse.AddFileDependencies. http://go.microsoft.com/fwlink/?linkid=14202")]
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected ArrayList FileDependencies {
		set {
			if (Response != null)
				Response.AddFileDependencies (value);
		}
	}

	[Browsable (false)]
	[EditorBrowsable (EditorBrowsableState.Never)]
	public override string ID {
		get { return base.ID; }
		set { base.ID = value; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public bool IsPostBack {
		get { return isPostBack; }
	}

	public bool IsPostBackEventControlRegistered {
		get { return requiresRaiseEvent != null; }
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
				throw new HttpException (Locale.GetText ("Page.IsValid cannot be called before validation has taken place. It should be queried in the event handler for a control that has CausesValidation=True and initiated the postback, or after a call to Page.Validate."));

			foreach (IValidator val in Validators)
				if (!val.IsValid)
					return false;
			return true;
		}
	}

	[Browsable (false)]
	public IDictionary Items {
		get {
			if (items == null)
				items = new Hashtable ();
			return items;
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public int LCID {
		get { return Thread.CurrentThread.CurrentCulture.LCID; }
		set { Thread.CurrentThread.CurrentCulture = new CultureInfo (value); }
	}

	[Browsable (false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public bool MaintainScrollPositionOnPostBack {
		get { return _maintainScrollPositionOnPostBack; }
		set { _maintainScrollPositionOnPostBack = value; }
	}

	public PageAdapter PageAdapter {
		get {
			return Adapter as PageAdapter;
		}
	}

	string _validationStartupScript;
	string _validationOnSubmitStatement;
	string _validationInitializeScript;
	string _webFormScriptReference;

	internal string WebFormScriptReference {
		get {
			if (_webFormScriptReference == null)
				_webFormScriptReference = IsMultiForm ? theForm : "window";
			return _webFormScriptReference;
		}
	}

	internal string ValidationStartupScript {
		get {
			if (_validationStartupScript == null) {
				_validationStartupScript =
@"
" + WebFormScriptReference + @".Page_ValidationActive = false;
" + WebFormScriptReference + @".ValidatorOnLoad();
" + WebFormScriptReference + @".ValidatorOnSubmit = function () {
	if (this.Page_ValidationActive) {
		return this.ValidatorCommonOnSubmit();
	}
	return true;
};
";
			}
			return _validationStartupScript;
		}
	}

	internal string ValidationOnSubmitStatement {
		get {
			if (_validationOnSubmitStatement == null)
				_validationOnSubmitStatement = "if (!" + WebFormScriptReference + ".ValidatorOnSubmit()) return false;";
			return _validationOnSubmitStatement;
		}
	}

	internal string ValidationInitializeScript {
		get {
			if (_validationInitializeScript == null)
				_validationInitializeScript = "WebFormValidation_Initialize(" + WebFormScriptReference + ");";
			return _validationInitializeScript;
		}
	}

	internal IScriptManager ScriptManager {
		get { return (IScriptManager) Items [typeof (IScriptManager)]; }
	}
	internal string theForm {
		get {
			return "theForm";
		}
	}
	
	internal bool IsMultiForm {
		get { return false; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public HttpRequest Request {
		get {
			if (_request == null)
				throw new HttpException("Request is not available in this context.");
			return RequestInternal;
		}
	}

	internal HttpRequest RequestInternal {
		get { return _request; }
	}
	
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public HttpResponse Response {
		get {
			if (_response == null)
				throw new HttpException ("Response is not available in this context.");
			return _response;
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public string ResponseEncoding {
		get { return Response.ContentEncoding.WebName; }
		set { Response.ContentEncoding = Encoding.GetEncoding (value); }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public HttpServerUtility Server {
		get { return Context.Server; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public virtual HttpSessionState Session {
		get {
			if (_session != null)
				return _session;

			try {
				_session = Context.Session;
			} catch {
				// ignore, should not throw
			}
			
			if (_session == null)
				throw new HttpException ("Session state can only be used " +
						"when enableSessionState is set to true, either " +
						"in a configuration file or in the Page directive.");

			return _session;
		}
	}

	[Filterable (false)]
	[Obsolete ("The recommended alternative is Page.SetFocus and Page.MaintainScrollPositionOnPostBack. http://go.microsoft.com/fwlink/?linkid=14202")]
	[Browsable (false)]
	public bool SmartNavigation {
		get { return _smartNavigation; }
		set { _smartNavigation = value; }
	}

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

		if (!String.IsNullOrEmpty (_styleSheetTheme)) {
			string virtualPath = "~/App_Themes/" + _styleSheetTheme;
			_styleSheetPageTheme = BuildManager.CreateInstanceFromVirtualPath (virtualPath, typeof (PageTheme)) as PageTheme;
		}
	}

	void InitializeTheme ()
	{
		if (_theme == null) {
			PagesSection ps = WebConfigurationManager.GetSection ("system.web/pages") as PagesSection;
			if (ps != null)
				_theme = ps.Theme;
		}

		if (!String.IsNullOrEmpty (_theme)) {
			string virtualPath = "~/App_Themes/" + _theme;
			_pageTheme = BuildManager.CreateInstanceFromVirtualPath (virtualPath, typeof (PageTheme)) as PageTheme;
			if (_pageTheme != null)
				_pageTheme.SetPage (this);
		}
	}
	public Control AutoPostBackControl {
		get { return _autoPostBackControl; }
		set { _autoPostBackControl = value; }
	}
	
	[BrowsableAttribute(false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public RouteData RouteData {
		get {
			if (_request == null)
				return null;

			RequestContext reqctx = _request.RequestContext;
			if (reqctx == null)
				return null;

			return reqctx.RouteData;
		}
	}
	
	[Bindable (true)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Localizable (true)]
	public string MetaDescription {
		get {
			if (_metaDescription == null) {
				if (htmlHeader == null) {
					if (frameworkInitialized)
						throw new InvalidOperationException ("A server-side head element is required to set this property.");
					return String.Empty;
				} else
					return htmlHeader.Description;
			}

			return _metaDescription;
		}
		
		set {
			if (htmlHeader == null) {
				if (frameworkInitialized)
					throw new InvalidOperationException ("A server-side head element is required to set this property.");
				_metaDescription = value;
			} else
				htmlHeader.Description = value;
		}
	}

	[Bindable (true)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Localizable (true)]
	public string MetaKeywords {
		get {
			if (_metaKeywords == null) {
				if (htmlHeader == null) {
					if (frameworkInitialized)
						throw new InvalidOperationException ("A server-side head element is required to set this property.");
					return String.Empty;
				} else
					return htmlHeader.Keywords;
			}

			return _metaDescription;
		}
		
		set {
			if (htmlHeader == null) {
				if (frameworkInitialized)
					throw new InvalidOperationException ("A server-side head element is required to set this property.");
				_metaKeywords = value;
			} else
				htmlHeader.Keywords = value;
		}
	}
	[Localizable (true)] 
	[Bindable (true)] 
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public string Title {
		get {
			if (_title == null) {
				if (htmlHeader != null && htmlHeader.Title != null)
					return htmlHeader.Title;
				return String.Empty;
			}
			return _title;
		}

		set {
			if (htmlHeader != null)
				htmlHeader.Title = value;
			else
				_title = value;
		}
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public TraceContext Trace {
		get { return Context.Trace; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public bool TraceEnabled {
		get { return Trace.IsEnabled; }
		set { Trace.IsEnabled = value; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public TraceMode TraceModeValue {
		get { return Trace.TraceMode; }
		set { Trace.TraceMode = value; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected int TransactionMode {
		get { return _transactionMode; }
		set { _transactionMode = value; }
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public string UICulture {
		get { return Thread.CurrentThread.CurrentUICulture.Name; }
		set { Thread.CurrentThread.CurrentUICulture = GetPageCulture (value, Thread.CurrentThread.CurrentUICulture); }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public IPrincipal User {
		get { return Context.User; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public ValidatorCollection Validators {
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
	public override bool Visible {
		get { return base.Visible; }
		set { base.Visible = value; }
	}

	#endregion

	#region Methods

	CultureInfo GetPageCulture (string culture, CultureInfo deflt)
	{
		if (culture == null)
			return deflt;
		CultureInfo ret = null;
		if (culture.StartsWith ("auto", StringComparison.InvariantCultureIgnoreCase)) {
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

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected IAsyncResult AspCompatBeginProcessRequest (HttpContext context,
							     AsyncCallback cb, 
							     object extraData)
	{
		throw new NotImplementedException ();
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[MonoNotSupported ("Mono does not support classic ASP compatibility mode.")]
	protected void AspCompatEndProcessRequest (IAsyncResult result)
	{
		throw new NotImplementedException ();
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	protected virtual HtmlTextWriter CreateHtmlTextWriter (TextWriter tw)
	{
		if (Request.BrowserMightHaveSpecialWriter)
			return Request.Browser.CreateHtmlTextWriter(tw);
		else
			return new HtmlTextWriter (tw);
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	public void DesignerInitialize ()
	{
		InitRecursive (null);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	protected internal virtual NameValueCollection DeterminePostBackMode ()
	{
		// if request was transfered from other page such Transfer
		if (_context.IsProcessingInclude)
			return null;
		
		HttpRequest req = Request;
		if (req == null)
			return null;

		NameValueCollection coll = null;
		if (0 == String.Compare (Request.HttpMethod, "POST", true, Helpers.InvariantCulture))
			coll = req.Form;
		else {
			string query = Request.QueryStringRaw;
			if (query == null || query.Length == 0)
				return null;

			coll = req.QueryString;
		}

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

	public override Control FindControl (string id) {
		if (id == ID)
			return this;
		else
			return base.FindControl (id);
	}

	Control FindControl (string id, bool decode) {
		return FindControl (id);
	}

	[Obsolete ("The recommended alternative is ClientScript.GetPostBackEventReference. http://go.microsoft.com/fwlink/?linkid=14202")]
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackClientEvent (Control control, string argument)
	{
		return scriptManager.GetPostBackEventReference (control, argument);
	}

	[Obsolete ("The recommended alternative is ClientScript.GetPostBackClientHyperlink. http://go.microsoft.com/fwlink/?linkid=14202")]
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackClientHyperlink (Control control, string argument)
	{
		return scriptManager.GetPostBackClientHyperlink (control, argument);
	}

	[Obsolete ("The recommended alternative is ClientScript.GetPostBackEventReference. http://go.microsoft.com/fwlink/?linkid=14202")]
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackEventReference (Control control)
	{
		return scriptManager.GetPostBackEventReference (control, String.Empty);
	}

	[Obsolete ("The recommended alternative is ClientScript.GetPostBackEventReference. http://go.microsoft.com/fwlink/?linkid=14202")]
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackEventReference (Control control, string argument)
	{
		return scriptManager.GetPostBackEventReference (control, argument);
	}

	internal void RequiresFormScriptDeclaration ()
	{
		requiresFormScriptDeclaration = true;
	}
	
	internal void RequiresPostBackScript ()
	{
		if (requiresPostBackScript)
			return;
		ClientScript.RegisterHiddenField (postEventSourceID, String.Empty);
		ClientScript.RegisterHiddenField (postEventArgumentID, String.Empty);
		requiresPostBackScript = true;
		RequiresFormScriptDeclaration ();
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	public virtual int GetTypeHashCode ()
	{
		return 0;
	}

	[MonoTODO ("The following properties of OutputCacheParameters are silently ignored: CacheProfile, SqlDependency")]
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected internal virtual void InitOutputCache(OutputCacheParameters cacheSettings)
	{
		if (cacheSettings.Enabled) {
			InitOutputCache(cacheSettings.Duration,
					cacheSettings.VaryByContentEncoding,
					cacheSettings.VaryByHeader,
					cacheSettings.VaryByCustom,
					cacheSettings.Location,
					cacheSettings.VaryByParam);

			HttpResponse response = Response;
			HttpCachePolicy cache = response != null ? response.Cache : null;
			if (cache != null && cacheSettings.NoStore)
				cache.SetNoStore ();
		}
	}
	
	[MonoTODO ("varyByContentEncoding is not currently used")]
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected virtual void InitOutputCache(int duration,
					       string varyByContentEncoding,
					       string varyByHeader,
					       string varyByCustom,
					       OutputCacheLocation location,
					       string varyByParam)
	{
		if (duration <= 0)
			// No need to do anything, cache will be ineffective anyway
			return;
		
		HttpResponse response = Response;
		HttpCachePolicy cache = response.Cache;
		bool set_vary = false;
		HttpContext ctx = Context;
		DateTime timestamp = ctx != null ? ctx.Timestamp : DateTime.Now;
		
		switch (location) {
			case OutputCacheLocation.Any:
				cache.SetCacheability (HttpCacheability.Public);
				cache.SetMaxAge (new TimeSpan (0, 0, duration));
				cache.SetLastModified (timestamp);
				set_vary = true;
				break;
			case OutputCacheLocation.Client:
				cache.SetCacheability (HttpCacheability.Private);
				cache.SetMaxAge (new TimeSpan (0, 0, duration));
				cache.SetLastModified (timestamp);
				break;
			case OutputCacheLocation.Downstream:
				cache.SetCacheability (HttpCacheability.Public);
				cache.SetMaxAge (new TimeSpan (0, 0, duration));
				cache.SetLastModified (timestamp);
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

			if (PageAdapter != null) {
				if (PageAdapter.CacheVaryByParams != null) {
					foreach (string p in PageAdapter.CacheVaryByParams)
						cache.VaryByParams [p] = true;
				}
				if (PageAdapter.CacheVaryByHeaders != null) {
					foreach (string h in PageAdapter.CacheVaryByHeaders)
						cache.VaryByHeaders [h] = true;
				}
			}
		}

		response.IsCached = true;
		cache.Duration = duration;
		cache.SetExpires (timestamp.AddSeconds (duration));
	}
	

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected virtual void InitOutputCache (int duration,
						string varyByHeader,
						string varyByCustom,
						OutputCacheLocation location,
						string varyByParam)
	{
		InitOutputCache (duration, null, varyByHeader, varyByCustom, location, varyByParam);
	}

	[Obsolete ("The recommended alternative is ClientScript.IsClientScriptBlockRegistered(string key). http://go.microsoft.com/fwlink/?linkid=14202")]
	public bool IsClientScriptBlockRegistered (string key)
	{
		return scriptManager.IsClientScriptBlockRegistered (key);
	}

	[Obsolete ("The recommended alternative is ClientScript.IsStartupScriptRegistered(string key). http://go.microsoft.com/fwlink/?linkid=14202")]
	public bool IsStartupScriptRegistered (string key)
	{
		return scriptManager.IsStartupScriptRegistered (key);
	}

	public string MapPath (string virtualPath)
	{
		return Request.MapPath (virtualPath);
	}
	
	protected internal override void Render (HtmlTextWriter writer)
	{
		if (MaintainScrollPositionOnPostBack) {
			ClientScript.RegisterWebFormClientScript ();

			ClientScript.RegisterHiddenField (ScrollPositionXID, Request [ScrollPositionXID]);
			ClientScript.RegisterHiddenField (ScrollPositionYID, Request [ScrollPositionYID]);

			StringBuilder script = new StringBuilder ();
			script.AppendLine ("<script type=\"text/javascript\">");
			script.AppendLine (ClientScriptManager.SCRIPT_BLOCK_START);
			script.AppendLine (theForm + ".oldSubmit = " + theForm + ".submit;");
			script.AppendLine (theForm + ".submit = function () { " + WebFormScriptReference + ".WebForm_SaveScrollPositionSubmit(); }");
			script.AppendLine (theForm + ".oldOnSubmit = " + theForm + ".onsubmit;");
			script.AppendLine (theForm + ".onsubmit = function () { " + WebFormScriptReference + ".WebForm_SaveScrollPositionOnSubmit(); }");
			if (IsPostBack) {
				script.AppendLine (theForm + ".oldOnLoad = window.onload;");
				script.AppendLine ("window.onload = function () { " + WebFormScriptReference + ".WebForm_RestoreScrollPosition (); };");
			}
			script.AppendLine (ClientScriptManager.SCRIPT_BLOCK_END);
			script.AppendLine ("</script>");
			
			ClientScript.RegisterStartupScript (typeof (Page), "MaintainScrollPositionOnPostBackStartup", script.ToString());
		}
		base.Render (writer);
	}

	void RenderPostBackScript (HtmlTextWriter writer, string formUniqueID)
	{
		writer.WriteLine ();
		ClientScriptManager.WriteBeginScriptBlock (writer);
		RenderClientScriptFormDeclaration (writer, formUniqueID);
		writer.WriteLine (WebFormScriptReference + "._form = " + theForm + ";");
		writer.WriteLine (WebFormScriptReference + ".__doPostBack = function (eventTarget, eventArgument) {");
		writer.WriteLine ("\tif(" + theForm + ".onsubmit && " + theForm + ".onsubmit() == false) return;");
		writer.WriteLine ("\t" + theForm + "." + postEventSourceID + ".value = eventTarget;");
		writer.WriteLine ("\t" + theForm + "." + postEventArgumentID + ".value = eventArgument;");
		writer.WriteLine ("\t" + theForm + ".submit();");
		writer.WriteLine ("}");
		ClientScriptManager.WriteEndScriptBlock (writer);
	}

	void RenderClientScriptFormDeclaration (HtmlTextWriter writer, string formUniqueID)
	{
		if (formScriptDeclarationRendered)
			return;
		
		if (PageAdapter != null) {
 			writer.WriteLine ("\tvar {0} = {1};\n", theForm, PageAdapter.GetPostBackFormReference(formUniqueID));
		} else {
			writer.WriteLine ("\tvar {0};\n\tif (document.getElementById) {{ {0} = document.getElementById ('{1}'); }}", theForm, formUniqueID);
			writer.WriteLine ("\telse {{ {0} = document.{1}; }}", theForm, formUniqueID);
		}
		formScriptDeclarationRendered = true;
	}

	internal void OnFormRender (HtmlTextWriter writer, string formUniqueID)
	{
		if (renderingForm)
			throw new HttpException ("Only 1 HtmlForm is allowed per page.");

		renderingForm = true;
		writer.WriteLine ();

		if (requiresFormScriptDeclaration || (scriptManager != null && scriptManager.ScriptsPresent) || PageAdapter != null) {
			ClientScriptManager.WriteBeginScriptBlock (writer);
			RenderClientScriptFormDeclaration (writer, formUniqueID);
			ClientScriptManager.WriteEndScriptBlock (writer);
		}

		if (handleViewState)
			scriptManager.RegisterHiddenField ("__VIEWSTATE", _savedViewState);

		scriptManager.WriteHiddenFields (writer);
		if (requiresPostBackScript) {
			RenderPostBackScript (writer, formUniqueID);
			postBackScriptRendered = true;
		}

		scriptManager.WriteWebFormClientScript (writer);
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
		scriptManager.SaveEventValidationState ();
		scriptManager.WriteExpandoAttributes (writer);
		scriptManager.WriteHiddenFields (writer);
		if (!postBackScriptRendered && requiresPostBackScript)
			RenderPostBackScript (writer, formUniqueID);
		scriptManager.WriteWebFormClientScript (writer);
		scriptManager.WriteArrayDeclares (writer);
		scriptManager.WriteStartupScriptBlocks (writer);
		renderingForm = false;
		postBackScriptRendered = false;
	}

	void ProcessPostData (NameValueCollection data, bool second)
	{
		NameValueCollection requestValues = _requestValueCollection == null ? new NameValueCollection (SecureHashCodeProvider.DefaultInvariant, CaseInsensitiveComparer.DefaultInvariant) : _requestValueCollection;
		
		if (data != null && data.Count > 0) {
			var used = new Dictionary <string, string> (StringComparer.Ordinal);
			foreach (string id in data.AllKeys) {
				if (id == "__VIEWSTATE" || id == postEventSourceID || id == postEventArgumentID || id == ClientScriptManager.EventStateFieldName)
					continue;
			
				if (used.ContainsKey (id))
					continue;

				used.Add (id, id);

				Control ctrl = FindControl (id, true);
				if (ctrl != null) {
					IPostBackDataHandler pbdh = ctrl as IPostBackDataHandler;
					IPostBackEventHandler pbeh = ctrl as IPostBackEventHandler;

					if (pbdh == null) {
						if (pbeh != null)
							formPostedRequiresRaiseEvent = pbeh;
						continue;
					}
		
					if (pbdh.LoadPostData (id, requestValues) == true) {
						if (requiresPostDataChanged == null)
							requiresPostDataChanged = new List <IPostBackDataHandler> ();
						requiresPostDataChanged.Add (pbdh);
					}
				
					if (_requiresPostBackCopy != null)
						_requiresPostBackCopy.Remove (id);

				} else if (!second) {
					if (secondPostData == null)
						secondPostData = new NameValueCollection (SecureHashCodeProvider.DefaultInvariant, CaseInsensitiveComparer.DefaultInvariant);
					secondPostData.Add (id, data [id]);
				}
			}
		}

		List <string> list1 = null;
		if (_requiresPostBackCopy != null && _requiresPostBackCopy.Count > 0) {
			string [] handlers = (string []) _requiresPostBackCopy.ToArray ();
			foreach (string id in handlers) {
				IPostBackDataHandler pbdh = FindControl (id, true) as IPostBackDataHandler;
				if (pbdh != null) {			
					_requiresPostBackCopy.Remove (id);
					if (pbdh.LoadPostData (id, requestValues)) {
						if (requiresPostDataChanged == null)
							requiresPostDataChanged = new List <IPostBackDataHandler> ();
	
						requiresPostDataChanged.Add (pbdh);
					}
				} else if (!second) {
					if (list1 == null)
						list1 = new List <string> ();
					list1.Add (id);
				}
			}
		}
		_requiresPostBackCopy = second ? null : list1;
		if (second)
			secondPostData = null;
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	public virtual void ProcessRequest (HttpContext context)
	{
		SetContext (context);
		
		if (clientTarget != null)
			Request.ClientTarget = clientTarget;

		WireupAutomaticEvents ();
		//-- Control execution lifecycle in the docs

		// Save culture information because it can be modified in FrameworkInitialize()
		_appCulture = Thread.CurrentThread.CurrentCulture;
		_appUICulture = Thread.CurrentThread.CurrentUICulture;
		FrameworkInitialize ();
		frameworkInitialized = true;
		context.ErrorPage = _errorPage;

		try {
			InternalProcessRequest ();
		} catch (ThreadAbortException taex) {
			if (FlagEnd.Value == taex.ExceptionState)
				Thread.ResetAbort ();
			else
				throw;
		} catch (Exception e) {
			ProcessException (e);
		} finally {
			ProcessUnload ();
		}
	}

	void ProcessException (Exception e) {
		// We want to remove that error, as we're rethrowing to stop
		// further processing.
		Trace.Warn ("Unhandled Exception", e.ToString (), e);
		_context.AddError (e); // OnError might access LastError
		OnError (EventArgs.Empty);
		if (_context.HasError (e)) {
			_context.ClearError (e);
			throw new HttpUnhandledException (null, e);
		}
	}

	void ProcessUnload () {
			try {
				RenderTrace ();
				UnloadRecursive (true);
			} catch {}
			if (Thread.CurrentThread.CurrentCulture.Equals (_appCulture) == false)
				Thread.CurrentThread.CurrentCulture = _appCulture;

			if (Thread.CurrentThread.CurrentUICulture.Equals (_appUICulture) == false)
				Thread.CurrentThread.CurrentUICulture = _appUICulture;
			
			_appCulture = null;
			_appUICulture = null;
	}
	
	delegate void ProcessRequestDelegate (HttpContext context);

	sealed class DummyAsyncResult : IAsyncResult
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

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected IAsyncResult AsyncPageBeginProcessRequest (HttpContext context, AsyncCallback callback, object extraData) 
	{
		ProcessRequest (context);
		DummyAsyncResult asyncResult = new DummyAsyncResult (true, true, extraData);

		if (callback != null) {
			callback (asyncResult);
		}
		
		return asyncResult;
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected void AsyncPageEndProcessRequest (IAsyncResult result) 
	{
	}
	
	void InternalProcessRequest ()
	{
		if (PageAdapter != null)
			_requestValueCollection = PageAdapter.DeterminePostBackMode();
		else
			_requestValueCollection = this.DeterminePostBackMode();

		// http://msdn2.microsoft.com/en-us/library/ms178141.aspx
		if (_requestValueCollection != null) {
			if (!isCrossPagePostBack && _requestValueCollection [PreviousPageID] != null && _requestValueCollection [PreviousPageID] != Request.FilePath) {
				_doLoadPreviousPage = true;
			} else {
				isCallback = _requestValueCollection [CallbackArgumentID] != null;
				// LAMESPEC: on Callback IsPostBack is set to false, but true.
				//isPostBack = !isCallback;
				isPostBack = true;
			}
			
			string lastFocus = _requestValueCollection [LastFocusID];
			if (!String.IsNullOrEmpty (lastFocus))
				_focusedControlID = UniqueID2ClientID (lastFocus);
		}
		
		if (!isCrossPagePostBack) {
			if (_context.PreviousHandler is Page)
				previousPage = (Page) _context.PreviousHandler;
		}

		Trace.Write ("aspx.page", "Begin PreInit");
		OnPreInit (EventArgs.Empty);
		Trace.Write ("aspx.page", "End PreInit");

		InitializeTheme ();
		ApplyMasterPage ();
		Trace.Write ("aspx.page", "Begin Init");
		InitRecursive (null);
		Trace.Write ("aspx.page", "End Init");

		Trace.Write ("aspx.page", "Begin InitComplete");
		OnInitComplete (EventArgs.Empty);
		Trace.Write ("aspx.page", "End InitComplete");
			
		renderingForm = false;	


		RestorePageState ();
		ProcessPostData ();
		ProcessRaiseEvents ();
		if (ProcessLoadComplete ())
			return;
		RenderPage ();
	}

	void RestorePageState ()
	{
		if (IsPostBack || IsCallback) {
			if (_requestValueCollection != null)
				scriptManager.RestoreEventValidationState (
					_requestValueCollection [ClientScriptManager.EventStateFieldName]);
			Trace.Write ("aspx.page", "Begin LoadViewState");
			LoadPageViewState ();
			Trace.Write ("aspx.page", "End LoadViewState");
		}
	}

	void ProcessPostData ()
	{
		if (IsPostBack || IsCallback) {
			Trace.Write ("aspx.page", "Begin ProcessPostData");
			ProcessPostData (_requestValueCollection, false);
			Trace.Write ("aspx.page", "End ProcessPostData");
		}

		ProcessLoad ();
		if (IsPostBack || IsCallback) {
			Trace.Write ("aspx.page", "Begin ProcessPostData Second Try");
			ProcessPostData (secondPostData, true);
			Trace.Write ("aspx.page", "End ProcessPostData Second Try");
		}
	}

	void ProcessLoad ()
	{ 
		Trace.Write ("aspx.page", "Begin PreLoad");
		OnPreLoad (EventArgs.Empty);
		Trace.Write ("aspx.page", "End PreLoad");

		Trace.Write ("aspx.page", "Begin Load");
		LoadRecursive ();
		Trace.Write ("aspx.page", "End Load");
	}

	void ProcessRaiseEvents ()
	{
		if (IsPostBack || IsCallback) {
			Trace.Write ("aspx.page", "Begin Raise ChangedEvents");
			RaiseChangedEvents ();
			Trace.Write ("aspx.page", "End Raise ChangedEvents");
			Trace.Write ("aspx.page", "Begin Raise PostBackEvent");
			RaisePostBackEvents ();
			Trace.Write ("aspx.page", "End Raise PostBackEvent");
		}
	}

	bool ProcessLoadComplete ()
	{
		Trace.Write ("aspx.page", "Begin LoadComplete");
		OnLoadComplete (EventArgs.Empty);
		Trace.Write ("aspx.page", "End LoadComplete");

		if (IsCrossPagePostBack)
			return true;

		if (IsCallback) {
			string result = ProcessCallbackData ();
			HtmlTextWriter callbackOutput = new HtmlTextWriter (Response.Output);
			callbackOutput.Write (result);
			callbackOutput.Flush ();
			return true;
		}
		
		Trace.Write ("aspx.page", "Begin PreRender");
		PreRenderRecursiveInternal ();
		Trace.Write ("aspx.page", "End PreRender");
		
		ExecuteRegisteredAsyncTasks ();

		Trace.Write ("aspx.page", "Begin PreRenderComplete");
		OnPreRenderComplete (EventArgs.Empty);
		Trace.Write ("aspx.page", "End PreRenderComplete");

		Trace.Write ("aspx.page", "Begin SaveViewState");
		SavePageViewState ();
		Trace.Write ("aspx.page", "End SaveViewState");
		
		Trace.Write ("aspx.page", "Begin SaveStateComplete");
		OnSaveStateComplete (EventArgs.Empty);
		Trace.Write ("aspx.page", "End SaveStateComplete");
		return false;
	}

	internal void RenderPage ()
	{
		scriptManager.ResetEventValidationState ();
		
		//--
		Trace.Write ("aspx.page", "Begin Render");
 		HtmlTextWriter output = CreateHtmlTextWriter (Response.Output);
		RenderControl (output);
		Trace.Write ("aspx.page", "End Render");
	}

	internal void SetContext (HttpContext context)
	{
		_context = context;

		_application = context.Application;
		_response = context.Response;
		_request = context.Request;
		_cache = context.Cache;
	}

	void RenderTrace ()
	{
		TraceManager traceManager = HttpRuntime.TraceManager;

		if (Trace.HaveTrace && !Trace.IsEnabled || !Trace.HaveTrace && !traceManager.Enabled)
			return;
		
		Trace.SaveData ();

		if (!Trace.HaveTrace && traceManager.Enabled && !traceManager.PageOutput) 
			return;

		if (!traceManager.LocalOnly || Context.Request.IsLocal) {
			HtmlTextWriter output = new HtmlTextWriter (Response.Output);
			Trace.Render (output);
		}
	}
	
	void RaisePostBackEvents ()
	{
		if (requiresRaiseEvent != null) {
			RaisePostBackEvent (requiresRaiseEvent, null);
			return;
		}

		if (formPostedRequiresRaiseEvent != null) {
			RaisePostBackEvent (formPostedRequiresRaiseEvent, null);
			return;
		}
		
		NameValueCollection postdata = _requestValueCollection;
		if (postdata == null)
			return;

		string eventTarget = postdata [postEventSourceID];
		IPostBackEventHandler target;
		if (String.IsNullOrEmpty (eventTarget)) {
			target = AutoPostBackControl as IPostBackEventHandler;
			if (target != null)
				RaisePostBackEvent (target, null);
			else
			if (formPostedRequiresRaiseEvent != null)
				RaisePostBackEvent (formPostedRequiresRaiseEvent, null);
			else
				Validate ();
			return;
		}

		target = FindControl (eventTarget, true) as IPostBackEventHandler;
		if (target == null)
			target = AutoPostBackControl as IPostBackEventHandler;
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
		sourceControl.RaisePostBackEvent (eventArgument);
	}
	
	[Obsolete ("The recommended alternative is ClientScript.RegisterArrayDeclaration(string arrayName, string arrayValue). http://go.microsoft.com/fwlink/?linkid=14202")]
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void RegisterArrayDeclaration (string arrayName, string arrayValue)
	{
		scriptManager.RegisterArrayDeclaration (arrayName, arrayValue);
	}

	[Obsolete ("The recommended alternative is ClientScript.RegisterClientScriptBlock(Type type, string key, string script). http://go.microsoft.com/fwlink/?linkid=14202")]
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void RegisterClientScriptBlock (string key, string script)
	{
		scriptManager.RegisterClientScriptBlock (key, script);
	}

	[Obsolete]
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

	[Obsolete ("The recommended alternative is ClientScript.RegisterOnSubmitStatement(Type type, string key, string script). http://go.microsoft.com/fwlink/?linkid=14202")]
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
		if (!(control is IPostBackDataHandler))
			throw new HttpException ("The control to register does not implement the IPostBackDataHandler interface.");
		
		if (_requiresPostBack == null)
			_requiresPostBack = new List <string> ();

		string uniqueID = control.UniqueID;
		if (_requiresPostBack.Contains (uniqueID))
			return;

		_requiresPostBack.Add (uniqueID);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void RegisterRequiresRaiseEvent (IPostBackEventHandler control)
	{
		requiresRaiseEvent = control;
	}

	[Obsolete ("The recommended alternative is ClientScript.RegisterStartupScript(Type type, string key, string script). http://go.microsoft.com/fwlink/?linkid=14202")]
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
	protected virtual void SavePageStateToPersistenceMedium (object state)
	{
		PageStatePersister persister = this.PageStatePersister;
		if (persister == null)
			return;
		Pair pair = state as Pair;
		if (pair != null) {
			persister.ViewState = pair.First;
			persister.ControlState = pair.Second;
		} else
			persister.ViewState = state;
		persister.Save ();
	}

	internal string RawViewState {
		get {
			NameValueCollection postdata = _requestValueCollection;
			string view_state;
			if (postdata == null || (view_state = postdata ["__VIEWSTATE"]) == null)
				return null;

			if (view_state == String.Empty)
				return null;
			return view_state;
		}
		
		set { _savedViewState = value; }
	}


	protected virtual PageStatePersister PageStatePersister {
		get {
			if (page_state_persister == null && PageAdapter != null)
					page_state_persister = PageAdapter.GetStatePersister();					
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

	internal void LoadPageViewState ()
	{
		Pair sState = LoadPageStateFromPersistenceMedium () as Pair;
		if (sState != null) {
			if (allow_load || isCrossPagePostBack) {
				LoadPageControlState (sState.Second);

				Pair vsr = sState.First as Pair;
				if (vsr != null) {
					LoadViewStateRecursive (vsr.First);
					_requiresPostBackCopy = vsr.Second as List <string>;
				}
			}
		}
	}

	internal void SavePageViewState ()
	{
		if (!handleViewState)
			return;

		object controlState = SavePageControlState ();
		Pair vsr = null;
		object viewState = null;
		
		if (EnableViewState
		    && this.ViewStateMode == ViewStateMode.Enabled
		)
			viewState = SaveViewStateRecursive ();
		
		object reqPostback = (_requiresPostBack != null && _requiresPostBack.Count > 0) ? _requiresPostBack : null;
		if (viewState != null || reqPostback != null)
			vsr = new Pair (viewState, reqPostback);

		Pair pair = new Pair ();
		pair.First = vsr;
		pair.Second = controlState;
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

	internal bool AreValidatorsUplevel ()
	{
		return AreValidatorsUplevel (String.Empty);
	}

	internal bool AreValidatorsUplevel (string valGroup)
	{
		bool uplevel = false;

		foreach (IValidator v in Validators) {
			BaseValidator bv = v as BaseValidator;
			if (bv == null)
				continue;

			if (valGroup != bv.ValidationGroup)
				continue;
			if (bv.GetRenderUplevel()) {
				uplevel = true;
				break;
			}
		}

		return uplevel;
	}

	bool ValidateCollection (ValidatorCollection validators)
	{
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
		if (Context == null)
			return;
		if (IsCallback)
			return;
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
		InitializeStyleSheet ();
	}
#endregion
	
	public ClientScriptManager ClientScript {
		get { return scriptManager; }
	}

	internal static readonly object InitCompleteEvent = new object ();
	internal static readonly object LoadCompleteEvent = new object ();
	internal static readonly object PreInitEvent = new object ();
	internal static readonly object PreLoadEvent = new object ();
	internal static readonly object PreRenderCompleteEvent = new object ();
	internal static readonly object SaveStateCompleteEvent = new object ();
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

		string defaultButtonId = Form.DefaultButton;
		/* figure out if we have some control we're going to focus */
		if (String.IsNullOrEmpty (_focusedControlID)) {
			_focusedControlID = Form.DefaultFocus;
			if (String.IsNullOrEmpty (_focusedControlID))
				_focusedControlID = defaultButtonId;
		}

		if (!String.IsNullOrEmpty (_focusedControlID)) {
			ClientScript.RegisterWebFormClientScript ();
			
			ClientScript.RegisterStartupScript (
				typeof(Page),
				"HtmlForm-DefaultButton-StartupScript",
				"\n" + WebFormScriptReference + ".WebForm_AutoFocus('" + _focusedControlID + "');\n", true);
		}
		
		if (Form.SubmitDisabledControls && _hasEnabledControlArray) {
			ClientScript.RegisterWebFormClientScript ();

			ClientScript.RegisterOnSubmitStatement (
				typeof (Page),
				"HtmlForm-SubmitDisabledControls-SubmitStatement",
				WebFormScriptReference + ".WebForm_ReEnableControls();");
		}
	}

	internal void RegisterEnabledControl (Control control)
	{
		if (Form == null || !Page.Form.SubmitDisabledControls || !Page.Form.DetermineRenderUplevel ())
			return;
		_hasEnabledControlArray = true;
		Page.ClientScript.RegisterArrayDeclaration (EnabledControlArrayID, String.Concat ("'", control.ClientID, "'"));
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

	public string ClientQueryString {
		get { return Request.UrlComponents.Query; }
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

	[Browsable (false)]
	[EditorBrowsable (EditorBrowsableState.Never)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public new virtual char IdSeparator {
		get {
			//TODO: why override?
			return base.IdSeparator;
		}
	}
	
	string ProcessCallbackData ()
	{
		ICallbackEventHandler target = GetCallbackTarget ();
		string callbackEventError = String.Empty;
		ProcessRaiseCallbackEvent (target, ref callbackEventError);
		return ProcessGetCallbackResult (target, callbackEventError);
	}

	ICallbackEventHandler GetCallbackTarget ()
	{
		string callbackTarget = _requestValueCollection [CallbackSourceID];
		if (callbackTarget == null || callbackTarget.Length == 0)
			throw new HttpException ("Callback target not provided.");

		Control targetControl = FindControl (callbackTarget, true);
		ICallbackEventHandler target = targetControl as ICallbackEventHandler;
		if (target == null)
			throw new HttpException (string.Format ("Invalid callback target '{0}'.", callbackTarget));
		return target;
	}

	void ProcessRaiseCallbackEvent (ICallbackEventHandler target, ref string callbackEventError)
	{
		string callbackArgument = _requestValueCollection [CallbackArgumentID];

		try {
			target.RaiseCallbackEvent (callbackArgument);
		} catch (Exception ex) {
			callbackEventError = String.Concat ("e", RuntimeHelpers.DebuggingEnabled ? ex.ToString () : ex.Message);
		}
		
	}

	string ProcessGetCallbackResult (ICallbackEventHandler target, string callbackEventError)
	{
		string callBackResult;
		try {
			callBackResult = target.GetCallbackResult ();
		} catch (Exception ex) {
			return String.Concat ("e", RuntimeHelpers.DebuggingEnabled ? ex.ToString () : ex.Message);
		}
		
		string eventValidation = ClientScript.GetEventValidationStateFormatted ();
		return callbackEventError + (eventValidation == null ? "0" : eventValidation.Length.ToString ()) + "|" +
			eventValidation + callBackResult;
	}

	[BrowsableAttribute (false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public HtmlHead Header {
		get { return htmlHeader; }
	}
	
	internal void SetHeader (HtmlHead header)
	{
		htmlHeader = header;
		if (header == null)
			return;
		
		if (_title != null) {
			htmlHeader.Title = _title;
			_title = null;
		}
		if (_metaDescription != null) {
			htmlHeader.Description = _metaDescription;
			_metaDescription = null;
		}

		if (_metaKeywords != null) {
			htmlHeader.Keywords = _metaKeywords;
			_metaKeywords = null;
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected bool AsyncMode {
		get { return asyncMode; }
		set { asyncMode = value; }
	}

	[Browsable (false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public TimeSpan AsyncTimeout {
		get { return asyncTimeout; }
		set { asyncTimeout = value; }
	}

	public bool IsAsync {
		get { return AsyncMode; }
	}	

	protected internal virtual string UniqueFilePathSuffix {
		get {
			if (String.IsNullOrEmpty (uniqueFilePathSuffix))
				uniqueFilePathSuffix = "__ufps=" + AppRelativeVirtualPath.GetHashCode ().ToString ("x");
			return uniqueFilePathSuffix;
		}
	}

	[MonoTODO ("Actually use the value in code.")]
	[Browsable (false)]
	[EditorBrowsable (EditorBrowsableState.Never)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public int MaxPageStateFieldLength {
		get { return maxPageStateFieldLength; }
		set { maxPageStateFieldLength = value; }
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

		if (IsPrerendered) {
			throw new InvalidOperationException ("AddOnPreRenderCompleteAsync can only be called before and during PreRender.");
		}

		if (beginHandler == null) {
			throw new ArgumentNullException ("beginHandler");
		}

		if (endHandler == null) {
			throw new ArgumentNullException ("endHandler");
		}

		RegisterAsyncTask (new PageAsyncTask (beginHandler, endHandler, null, state, false));
	}

	List<PageAsyncTask> ParallelTasks {
		get {
			if (parallelTasks == null)
				parallelTasks = new List<PageAsyncTask>();
			return parallelTasks;
		}
	}

	List<PageAsyncTask> SerialTasks {
		get {
			if (serialTasks == null)
				serialTasks = new List<PageAsyncTask> ();
			return serialTasks;
		}
	}

	public void RegisterAsyncTask (PageAsyncTask task) 
	{
		if (task == null)
			throw new ArgumentNullException ("task");

		if (task.ExecuteInParallel)
			ParallelTasks.Add (task);
		else
			SerialTasks.Add (task);
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
				IAsyncResult result = parallelTask.BeginHandler (this, EventArgs.Empty, new AsyncCallback (EndAsyncTaskCallback), parallelTask);
				if (result.CompletedSynchronously)
					parallelTask.EndHandler (result);
				else
					asyncResults.Add (result);
			}

			if (asyncResults.Count > 0) {
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
			}
			DateTime endWait = DateTime.Now;
			TimeSpan elapsed = endWait - startExecution;
			if (elapsed <= AsyncTimeout)
				AsyncTimeout -= elapsed;
			else
				AsyncTimeout = TimeSpan.FromTicks(0);
		}

		if (serialTasks != null) {
			List<PageAsyncTask> localSerialTasks = serialTasks;
			serialTasks = null; // Shouldn't execute tasks twice
			foreach (PageAsyncTask serialTask in localSerialTasks) {
				DateTime startExecution = DateTime.Now;

				IAsyncResult result = serialTask.BeginHandler (this, EventArgs.Empty, new AsyncCallback (EndAsyncTaskCallback), serialTask);
				if (result.CompletedSynchronously)
					serialTask.EndHandler (result);
				else {
					bool done = result.AsyncWaitHandle.WaitOne (AsyncTimeout, false);
					if (!done && !result.IsCompleted) {
						serialTask.TimeoutHandler (result);
					}
				}
				DateTime endWait = DateTime.Now;
				TimeSpan elapsed = endWait - startExecution;
				if (elapsed <= AsyncTimeout)
					AsyncTimeout -= elapsed;
				else
					AsyncTimeout = TimeSpan.FromTicks (0);
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

	[Browsable (false)]
	[DefaultValue ("0")]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[EditorBrowsable (EditorBrowsableState.Never)]
	public ViewStateEncryptionMode ViewStateEncryptionMode {
		get { return viewStateEncryptionMode; }
		set { viewStateEncryptionMode = value; }
	}

	public void RegisterRequiresViewStateEncryption ()
	{
		controlRegisteredForViewStateEncryption = true;
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
			MasterPage master = Master;
			
			if (master != null) {
				var appliedMasterPageFiles = new Dictionary <string, bool> (StringComparer.Ordinal);
				MasterPage.ApplyMasterPageRecursive (Request.CurrentExecutionFilePath, HostingEnvironment.VirtualPathProvider, master, appliedMasterPageFiles);
				master.Page = this;
				Controls.Clear ();
				Controls.Add (master);
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

		if (IsPrerendered)
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
			requireStateControls = new List <Control> ();
		requireStateControls.Add (control);
		int n = requireStateControls.Count - 1;
		
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
		if (requireStateControls == null)
			return false;
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
		if (validationGroup == String.Empty)
			validationGroup = null;

		ValidatorCollection col = new ValidatorCollection ();
		if (_validators == null)
			return col;
		
		foreach (IValidator v in _validators)
			if (BelongsToGroup(v, validationGroup))
				col.Add(v);

		return col;
	}
	
	bool BelongsToGroup(IValidator v, string validationGroup)
	{
		BaseValidator validator = v as BaseValidator;
		if (validationGroup == null)
			return validator == null || String.IsNullOrEmpty (validator.ValidationGroup); 
		else
			return validator != null && validator.ValidationGroup == validationGroup; 			
	}
	
	public virtual void Validate (string validationGroup)
	{
		is_validated = true;
		ValidateCollection (GetValidators (validationGroup));
	}

	object SavePageControlState ()
	{
		int count = requireStateControls == null ? 0 : requireStateControls.Count;
		if (count == 0)
			return null;
		
		object state;
		object[] controlStates = new object [count];
		object[] adapterState = new object [count];
		Control control;
		ControlAdapter adapter;
		bool allNull = true;
		TraceContext trace = (Context != null && Context.Trace.IsEnabled) ? Context.Trace : null;
		
		for (int n = 0; n < count; n++) {
			control = requireStateControls [n];
			state = controlStates [n] = control.SaveControlState ();
			if (state != null)
				allNull = false;
			
			if (trace != null)
				trace.SaveControlState (control, state);

			adapter = control.Adapter;
			if (adapter != null) {
				adapterState [n] = adapter.SaveAdapterControlState ();
				if (adapterState [n] != null) allNull = false;
			}
		}
		
		if (allNull)
			return null;
		else
			return new Pair (controlStates, adapterState);
	}
	
	void LoadPageControlState (object data)
	{
		_savedControlState = null;
		if (data == null) return;
		Pair statePair = (Pair)data;
		_savedControlState = (object[]) statePair.First;
		object[] adapterState = (object[]) statePair.Second;

		if (requireStateControls == null) return;

		int min = Math.Min (requireStateControls.Count, _savedControlState != null ? _savedControlState.Length : requireStateControls.Count);
		for (int n=0; n < min; n++) {
			Control ctl = (Control) requireStateControls [n];
			ctl.LoadControlState (_savedControlState != null ? _savedControlState [n] : null);
			if (ctl.Adapter != null)
				ctl.Adapter.LoadAdapterControlState (adapterState != null ? adapterState [n] : null);
		}
	}

	void LoadPreviousPageReference ()
	{
		if (_requestValueCollection != null) {
			string prevPage = _requestValueCollection [PreviousPageID];
			if (prevPage != null) {
				IHttpHandler handler;
				handler = BuildManager.CreateInstanceFromVirtualPath (prevPage, typeof (IHttpHandler)) as IHttpHandler;
				previousPage = (Page) handler;
				previousPage.isCrossPagePostBack = true;
				Server.Execute (handler, null, true, _context.Request.CurrentExecutionFilePath, null, false, false);
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

	void AddStyleSheets (PageTheme theme, ref List <string> links)
	{
		if (theme == null)
			return;

		string[] tmpThemes = theme != null ? theme.GetStyleSheets () : null;
		if (tmpThemes == null || tmpThemes.Length == 0)
			return;

		if (links == null)
			links = new List <string> ();

		links.AddRange (tmpThemes);
	}
	
	protected internal override void OnInit (EventArgs e)
	{
		base.OnInit (e);

		List <string> themes = null;
		AddStyleSheets (StyleSheetPageTheme, ref themes);
		AddStyleSheets (PageTheme, ref themes);

		if (themes == null)
			return;
		
		HtmlHead header = Header;
		if (themes != null && header == null)
			throw new InvalidOperationException ("Using themed css files requires a header control on the page.");
		
		ControlCollection headerControls = header.Controls;
		string lss;
		for (int i = themes.Count - 1; i >= 0; i--) {
			lss = themes [i];
			HtmlLink hl = new HtmlLink ();
			hl.Href = lss;
			hl.Attributes["type"] = "text/css";
			hl.Attributes["rel"] = "stylesheet";
			headerControls.AddAt (0, hl);
		}
	}

	[MonoDocumentationNote ("Not implemented.  Only used by .net aspx parser")]
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected object GetWrappedFileDependencies (string [] virtualFileDependencies)
	{
		return virtualFileDependencies;
	}

	[MonoDocumentationNote ("Does nothing.  Used by .net aspx parser")]
	protected virtual void InitializeCulture ()
	{
	}

	[MonoDocumentationNote ("Does nothing. Used by .net aspx parser")]
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected internal void AddWrappedFileDependencies (object virtualFileDependencies)
	{
	}
}
}
