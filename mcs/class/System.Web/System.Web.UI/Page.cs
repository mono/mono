//
// System.Web.UI.Page.cs
//
// Authors:
//   Duncan Mak  (duncan@ximian.com)
//   Gonzalo Paniagua (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002,2003 Ximian, Inc. (http://www.ximian.com)
// (c) 2003 Novell, Inc. (http://www.novell.com)
//

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
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Web.Util;
#if NET_2_0
using System.Web.UI.HtmlControls;
#endif

namespace System.Web.UI
{

#if !NET_2_0
[RootDesignerSerializer ("Microsoft.VSDesigner.WebForms.RootCodeDomSerializer, " + Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design, true)]
#endif
[DefaultEvent ("Load"), DesignerCategory ("ASPXCodeBehind")]
[ToolboxItem (false)]
[Designer ("System.Web.UI.Design.ControlDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
public class Page : TemplateControl, IHttpHandler
{
	private bool _viewState = true;
	private bool _viewStateMac;
	private string _errorPage;
	private bool _isValid;
	private bool _smartNavigation;
	private int _transactionMode;
	private HttpContext _context;
	private ValidatorCollection _validators;
	private bool renderingForm;
	private object _savedViewState;
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
	ClientScriptManager scriptManager = new ClientScriptManager ();

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected const string postEventArgumentID = "__EVENTARGUMENT";
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected const string postEventSourceID = "__EVENTTARGET";

#if NET_2_0
	private const string callbackArgumentID = "__CALLBACKARGUMENT";
	private const string callbackSourceID = "__CALLBACKTARGET";
	private const string previousPageID = "__PREVIOUSPAGE";
	
	IPageHeader htmlHeader;
	
	MasterPage masterPage;
	string masterPageFile;
	
	Page previousPage;
	bool isCrossPagePostBack;
	ArrayList requireStateControls;
	Hashtable _validatorsByGroup;
	HtmlForm _form;
#endif

	#region Constructor
	public Page ()
	{
		Page = this;
	}

	#endregion		

	#region Properties

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public HttpApplicationState Application
	{
		get { return _context.Application; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
#if NET_2_0
	public bool AspCompatMode
	{
		get { return false; }
		set { throw new NotImplementedException (); }
	}
#else
	protected bool AspCompatMode
	{
		set { throw new NotImplementedException (); }
	}
#endif

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
		get { return _context.Cache; }
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

	[EditorBrowsable (EditorBrowsableState.Never)]
#if NET_2_0
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    [BrowsableAttribute (false)]
	public string Culture
	{
		get { return Thread.CurrentThread.CurrentCulture.Name; }
		set { Thread.CurrentThread.CurrentCulture = new CultureInfo (value); }
	}
#else
	protected string Culture
	{
		set { Thread.CurrentThread.CurrentCulture = new CultureInfo (value); }
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
	protected bool EnableViewStateMac
	{
		get { return _viewStateMac; }
		set { _viewStateMac = value; }
	}

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
			return _requestValueCollection != null;
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never), Browsable (false)]
	public bool IsReusable {
		get { return false; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public bool IsValid
	{
		get { return _isValid; }
	}

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

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public HttpRequest Request
	{
		get { return _context.Request; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public HttpResponse Response
	{
		get { return _context.Response; }
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
			if (_context.Session == null)
				throw new HttpException ("Session state can only be used " +
						"when enableSessionState is set to true, either " +
						"in a configuration file or in the Page directive.");

			return _context.Session;
		}
	}

#if NET_2_0
    [FilterableAttribute (false)]
#endif
	[Browsable (false)]
	public bool SmartNavigation
	{
		get { return _smartNavigation; }
		set { _smartNavigation = value; }
	}

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
#if NET_2_0
	public int TransactionMode
	{
		get { return _transactionMode; }
		set { _transactionMode = value; }
	}
#else
	protected int TransactionMode
	{
		set { _transactionMode = value; }
	}
#endif

	[EditorBrowsable (EditorBrowsableState.Never)]
#if NET_2_0
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    [BrowsableAttribute (false)]
	public string UICulture
	{
		get { return Thread.CurrentThread.CurrentUICulture.Name; }
		set { Thread.CurrentThread.CurrentUICulture = new CultureInfo (value); }
	}
#else
	protected string UICulture
	{
		set { Thread.CurrentThread.CurrentUICulture = new CultureInfo (value); }
	}
#endif

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public IPrincipal User
	{
		get { return _context.User; }
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
		if (0 == String.Compare (Request.HttpMethod, "POST", true))
			coll =	req.Form;
		else 
			coll = req.QueryString;

		
		if (coll == null || coll ["__VIEWSTATE"] == null)
			return null;

		return coll;
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackClientEvent (Control control, string argument)
	{
		return GetPostBackEventReference (control, argument);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackClientHyperlink (Control control, string argument)
	{
		return "javascript:" + GetPostBackEventReference (control, argument);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackEventReference (Control control)
	{
		return GetPostBackEventReference (control, "");
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackEventReference (Control control, string argument)
	{
		RequiresPostBackScript ();
		return String.Format ("__doPostBack('{0}','{1}')", control.UniqueID, argument);
	}

	internal void RequiresPostBackScript ()
	{
		requiresPostBackScript = true;
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	public virtual int GetTypeHashCode ()
	{
		return 0;
	}

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
	
	private void RenderPostBackScript (HtmlTextWriter writer, string formUniqueID)
	{
		writer.WriteLine ("<input type=\"hidden\" name=\"{0}\" value=\"\" />", postEventSourceID);
		writer.WriteLine ("<input type=\"hidden\" name=\"{0}\" value=\"\" />", postEventArgumentID);
		writer.WriteLine ();
		writer.WriteLine ("<script language=\"javascript\">");
		writer.WriteLine ("<!--");

		if (Request.Browser.Browser == ("Netscape") && Request.Browser.MajorVersion == 4)
			writer.WriteLine ("\tvar theForm = document.{0};", formUniqueID);
		else
			writer.WriteLine ("\tvar theForm = document.getElementById ('{0}');", formUniqueID);

		writer.WriteLine ("\tfunction __doPostBack(eventTarget, eventArgument) {");
		writer.WriteLine ("\t\ttheForm.{0}.value = eventTarget;", postEventSourceID);
		writer.WriteLine ("\t\ttheForm.{0}.value = eventArgument;", postEventArgumentID);
		writer.WriteLine ("\t\ttheForm.submit();");
		writer.WriteLine ("\t}");
		writer.WriteLine ("// -->");
		writer.WriteLine ("</script>");
	}

	internal void OnFormRender (HtmlTextWriter writer, string formUniqueID)
	{
		if (renderingForm)
			throw new HttpException ("Only 1 HtmlForm is allowed per page.");

		renderingForm = true;
		writer.WriteLine ();
		scriptManager.WriteHiddenFields (writer);
		if (requiresPostBackScript) {
			RenderPostBackScript (writer, formUniqueID);
			postBackScriptRendered = true;
		}

		if (handleViewState) {
			string vs = GetViewStateString ();
			writer.Write ("<input type=\"hidden\" name=\"__VIEWSTATE\" ");
			writer.WriteLine ("value=\"{0}\" />", vs);
		}

		scriptManager.WriteClientScriptBlocks (writer);
	}

	internal string GetViewStateString ()
	{
		if (_savedViewState == null)
			return null;
		StringWriter sr = new StringWriter ();
		LosFormatter fmt = new LosFormatter ();
		fmt.Serialize (sr, _savedViewState);
		return sr.GetStringBuilder ().ToString ();
	}

	internal void OnFormPostRender (HtmlTextWriter writer, string formUniqueID)
	{
		scriptManager.WriteArrayDeclares (writer);

		if (!postBackScriptRendered && requiresPostBackScript)
			RenderPostBackScript (writer, formUniqueID);

		scriptManager.WriteHiddenFields (writer);
		scriptManager.WriteClientScriptIncludes (writer);
		scriptManager.WriteStartupScriptBlocks (writer);
		renderingForm = false;
		postBackScriptRendered = false;
	}

	private void ProcessPostData (NameValueCollection data, bool second)
	{
		if (data == null)
			return;

		if (_requiresPostBackCopy == null && _requiresPostBack != null)
			_requiresPostBackCopy = (ArrayList) _requiresPostBack.Clone ();

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
					if (_requiresPostBackCopy != null)
						_requiresPostBackCopy.Remove (ctrl.UniqueID);
				}
			} else if (!second) {
				if (secondPostData == null)
					secondPostData = new NameValueCollection ();
				secondPostData.Add (real_id, data [id]);
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
				} else if (second) {
					if (list1 == null)
						list1 = new ArrayList ();
					list1.Add (id);
				}
			}
			_requiresPostBack = list1;
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	public void ProcessRequest (HttpContext context)
	{
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
		} finally {
			try {
				UnloadRecursive (true);
			} catch {}
			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = uiculture;
		}
	}
	
#if NET_2_0
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
		if (!IsCrossPagePostBack)
			LoadPreviousPageReference ();
			
		OnPreInit (EventArgs.Empty);
#endif
		Trace.Write ("aspx.page", "Begin Init");
		InitRecursive (null);
		Trace.Write ("aspx.page", "End Init");

#if NET_2_0
		OnInitComplete (EventArgs.Empty);
		
		if (masterPageFile != null) {
			Controls.Add (Master);
			Master.FillPlaceHolders ();
		}
#endif
			
		renderingForm = false;	
		if (IsPostBack) {
			Trace.Write ("aspx.page", "Begin LoadViewState");
			LoadPageViewState ();
			Trace.Write ("aspx.page", "End LoadViewState");
			Trace.Write ("aspx.page", "Begin ProcessPostData");
			ProcessPostData (_requestValueCollection, false);
			Trace.Write ("aspx.page", "End ProcessPostData");
		}
		
#if NET_2_0
		if (IsCrossPagePostBack)
			return;

		OnPreLoad (EventArgs.Empty);
#endif

		LoadRecursive ();
		if (IsPostBack) {
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
		OnLoadComplete (EventArgs.Empty);

		if (IsCallback) {
			string result = ProcessCallbackData ();
			HtmlTextWriter callbackOutput = new HtmlTextWriter (_context.Response.Output);
			callbackOutput.Write (result);
			callbackOutput.Flush ();
			return;
		}
#endif
		
		Trace.Write ("aspx.page", "Begin PreRender");
		PreRenderRecursiveInternal ();
		Trace.Write ("aspx.page", "End PreRender");
		
#if NET_2_0
		OnPreRenderComplete (EventArgs.Empty);
#endif

		Trace.Write ("aspx.page", "Begin SaveViewState");
		SavePageViewState ();
		Trace.Write ("aspx.page", "End SaveViewState");
		
#if NET_2_0
		OnSaveStateComplete (EventArgs.Empty);
#endif
		
		//--
		Trace.Write ("aspx.page", "Begin Render");
		HtmlTextWriter output = new HtmlTextWriter (_context.Response.Output);
		RenderControl (output);
		Trace.Write ("aspx.page", "End Render");
		
		RenderTrace (output);
	}

	private void RenderTrace (HtmlTextWriter output)
	{
		TraceManager traceManager = HttpRuntime.TraceManager;

		if (Trace.HaveTrace && !Trace.IsEnabled || !Trace.HaveTrace && !traceManager.Enabled)
			return;
		
		Trace.SaveData ();

		if (!Trace.HaveTrace && traceManager.Enabled && !traceManager.PageOutput) 
			return;

		if (!traceManager.LocalOnly || Context.Request.IsLocal)
			Trace.Render (output);
	}
	
	internal void RaisePostBackEvents ()
	{
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

		IPostBackEventHandler target = FindControl (eventTarget) as IPostBackEventHandler;
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

	[MonoTODO("Used in HtmlForm")]
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

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void RegisterRequiresPostBack (Control control)
	{
		if (_requiresPostBack == null)
			_requiresPostBack = new ArrayList ();

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
		_savedViewState = viewState;
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	protected virtual object LoadPageStateFromPersistenceMedium ()
	{
		NameValueCollection postdata = _requestValueCollection;
		string view_state;
		if (postdata == null || (view_state = postdata ["__VIEWSTATE"]) == null)
			return null;

		_savedViewState = null;
		LosFormatter fmt = new LosFormatter ();

		try { 
			_savedViewState = fmt.Deserialize (view_state);
		} catch (Exception e) {
			throw new HttpException ("Error restoring page viewstate.\n", e);
		}

		return _savedViewState;
	}

	internal void LoadPageViewState()
	{
		object sState = LoadPageStateFromPersistenceMedium ();
		if (sState != null) {
#if NET_2_0
			Triplet data = (Triplet) sState;
			LoadPageControlState (data.Third);
			LoadViewStateRecursive (data.First);
			_requiresPostBack = data.Second as ArrayList;
#else
			Pair pair = (Pair) sState;
			LoadViewStateRecursive (pair.First);
			_requiresPostBack = pair.Second as ArrayList;
#endif
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

#if NET_2_0
		Triplet triplet = new Triplet ();
		triplet.First = viewState;
		triplet.Second = reqPostback;
		triplet.Third = controlState;

		if (triplet.First == null && triplet.Second == null && triplet.Third == null)
			triplet = null;
			
		SavePageStateToPersistenceMedium (triplet);
#else
		Pair pair = new Pair ();
		pair.First = viewState;
		pair.Second = reqPostback;

		if (pair.First == null && pair.Second == null)
			pair = null;
			
		SavePageStateToPersistenceMedium (pair);
#endif
	}

	public virtual void Validate ()
	{
		ValidateCollection (_validators);
	}
	
	void ValidateCollection (ValidatorCollection validators)
	{
		if (validators == null || validators.Count == 0){
			_isValid = true;
			return;
		}

		bool all_valid = true;
		foreach (IValidator v in validators){
			v.Validate ();
			if (v.IsValid == false)
				all_valid = false;
		}

		if (all_valid)
			_isValid = true;
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void VerifyRenderingInServerForm (Control control)
	{
		if (!renderingForm)
			throw new HttpException ("Control '" + control.ClientID + " " + control.GetType () + 
						 "' must be rendered within a HtmlForm");
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
	
	public event EventHandler InitComplete {
		add { Events.AddHandler (InitCompleteEvent, value); }
		remove { Events.RemoveHandler (InitCompleteEvent, value); }
	}
	
	public event EventHandler LoadComplete {
		add { Events.AddHandler (LoadCompleteEvent, value); }
		remove { Events.RemoveHandler (LoadCompleteEvent, value); }
	}
	
	public event EventHandler PreInit {
		add { Events.AddHandler (PreInitEvent, value); }
		remove { Events.RemoveHandler (PreInitEvent, value); }
	}
	
	public event EventHandler PreLoad {
		add { Events.AddHandler (PreLoadEvent, value); }
		remove { Events.RemoveHandler (PreLoadEvent, value); }
	}
	
	public event EventHandler PreRenderComplete {
		add { Events.AddHandler (PreRenderCompleteEvent, value); }
		remove { Events.RemoveHandler (PreRenderCompleteEvent, value); }
	}
	
	public event EventHandler SaveStateComplete {
		add { Events.AddHandler (SaveStateCompleteEvent, value); }
		remove { Events.RemoveHandler (SaveStateCompleteEvent, value); }
	}
	
	protected virtual void OnInitComplete (EventArgs e)
	{
		if (Events != null) {
			EventHandler eh = (EventHandler) (Events [InitCompleteEvent]);
			if (eh != null) eh (this, e);
		}
	}
	
	protected virtual void OnLoadComplete (EventArgs e)
	{
		if (Events != null) {
			EventHandler eh = (EventHandler) (Events [LoadCompleteEvent]);
			if (eh != null) eh (this, e);
		}
	}
	
	protected virtual void OnPreInit (EventArgs e)
	{
		if (Events != null) {
			EventHandler eh = (EventHandler) (Events [PreInitEvent]);
			if (eh != null) eh (this, e);
		}
	}
	
	protected virtual void OnPreLoad (EventArgs e)
	{
		if (Events != null) {
			EventHandler eh = (EventHandler) (Events [PreLoadEvent]);
			if (eh != null) eh (this, e);
		}
	}
	
	protected virtual void OnPreRenderComplete (EventArgs e)
	{
		if (Events != null) {
			EventHandler eh = (EventHandler) (Events [PreRenderCompleteEvent]);
			if (eh != null) eh (this, e);
		}
	}
	
	protected virtual void OnSaveStateComplete (EventArgs e)
	{
		if (Events != null) {
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
	
	public string GetWebResourceUrl(Type type, string resourceName)
	{
		if (type == null)
			throw new ArgumentNullException ("type");
	
		if (resourceName == null || resourceName.Length == 0)
			throw new ArgumentNullException ("type");
	
		return System.Web.Handlers.AssemblyResourceLoader.GetResourceUrl (type, resourceName); 
	}
	
	Stack dataItemCtx;
	
	internal void PushDataItemContext (object o)
	{
		if (dataItemCtx == null)
			dataItemCtx = new Stack ();
		
		dataItemCtx.Push (o);
	}
	
	internal void PopDataItemContext ()
	{
		if (dataItemCtx == null)
			throw new InvalidOperationException ();
		
		dataItemCtx.Pop ();
	}
	
	internal object CurrentDataItem {
		get {
			if (dataItemCtx == null)
				throw new InvalidOperationException ("No data item");
			
			return dataItemCtx.Peek ();
		}
	}
	
	protected object Eval (string expression)
	{
		return DataBinder.Eval (CurrentDataItem, expression);
	}
	
	protected object Eval (string expression, string format)
	{
		return DataBinder.Eval (CurrentDataItem, expression, format);
	}
	
	protected object XPath (string xpathexpression)
	{
		return XPathBinder.Eval (CurrentDataItem, xpathexpression);
	}
	
	protected object XPath (string xpathexpression, string format)
	{
		return XPathBinder.Eval (CurrentDataItem, xpathexpression, format);
	}
	
	protected IEnumerable XPathSelect (string xpathexpression)
	{
		return XPathBinder.Select (CurrentDataItem, xpathexpression);
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetCallbackEventReference (Control control, string argument, string clientCallback, string context)
	{
		return GetCallbackEventReference (control, argument, clientCallback, context, null);
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetCallbackEventReference (Control control, string argument, string clientCallback, string context, string clientErrorCallback)
	{
		if (!ClientScript.IsClientScriptIncludeRegistered (typeof(Page), "callback"))
			ClientScript.RegisterClientScriptInclude (typeof(Page), "callback", GetWebResourceUrl (typeof(Page), "callback.js"));
		
		return string.Format ("WebForm_DoCallback ('{0}', {1}, {2}, {3}, {4})", control.UniqueID, argument, clientCallback, context, clientErrorCallback);
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackEventReference (PostBackOptions options)
	{
		if (!ClientScript.IsClientScriptIncludeRegistered (typeof(Page), "webform")) {
			ClientScript.RegisterClientScriptInclude (typeof(Page), "webform", GetWebResourceUrl (typeof(Page), "webform.js"));
		}
		
		if (options.ActionUrl != null)
			ClientScript.RegisterHiddenField (previousPageID, _context.Request.FilePath);
		
		if (options.ClientSubmit || options.ActionUrl != null)
			RequiresPostBackScript ();
		
		return String.Format ("{0}WebForm_DoPostback({1},{2},{3},{4},{5},{6},{7},{8})", 
				options.RequiresJavaScriptProtocol ? "javascript:" : "",
				ClientScriptManager.GetScriptLiteral (options.TargetControl.UniqueID), 
				ClientScriptManager.GetScriptLiteral (options.Argument),
				ClientScriptManager.GetScriptLiteral (options.ActionUrl),
				ClientScriptManager.GetScriptLiteral (options.AutoPostBack),
				ClientScriptManager.GetScriptLiteral (options.PerformValidation),
				ClientScriptManager.GetScriptLiteral (options.TrackFocus),
				ClientScriptManager.GetScriptLiteral (options.ClientSubmit),
				ClientScriptManager.GetScriptLiteral (options.ValidationGroup)
			);
	}
	
    [BrowsableAttribute (false)]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public Page PreviousPage {
		get { return previousPage; }
	}

	
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    [BrowsableAttribute (false)]
	public bool IsCallback {
		get { return _requestValueCollection != null && _requestValueCollection [callbackArgumentID] != null; }
	}
	
    [BrowsableAttribute (false)]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public bool IsCrossPagePostBack {
		get { return _requestValueCollection != null && isCrossPagePostBack; }
	}
	
	string ProcessCallbackData ()
	{
		string callbackTarget = _requestValueCollection [callbackSourceID];
		if (callbackTarget == null || callbackTarget.Length == 0)
			throw new HttpException ("Callback target not provided.");

		ICallbackEventHandler target = FindControl (callbackTarget) as ICallbackEventHandler;
		if (target == null)
			throw new HttpException (string.Format ("Invalid callback target '{0}'.", callbackTarget));

		string callbackArgument = _requestValueCollection [callbackArgumentID];
		return target.RaiseCallbackEvent (callbackArgument);
	}

    [BrowsableAttribute (false)]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public IPageHeader Header {
		get { return htmlHeader; }
	}
	
	internal void SetHeader (IPageHeader header)
	{
		htmlHeader = header;
	}
	
    [DefaultValueAttribute ("")]
	public string MasterPageFile {
		get { return masterPageFile; }
		set { masterPageFile = value; masterPage = null; }
	}
	
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    [BrowsableAttribute (false)]
	public MasterPage Master {
		get {
			if (masterPage == null)
				masterPage = MasterPageParser.GetCompiledMasterInstance (masterPageFile, Server.MapPath (masterPageFile), Context);
			return masterPage;
		}
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void RegisterRequiresControlState (Control control)
	{
		if (requireStateControls == null) requireStateControls = new ArrayList ();
		requireStateControls.Add (control);
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
		if (validationGroup == null || validationGroup == "")
			return Validators;

		if (_validatorsByGroup == null) _validatorsByGroup = new Hashtable ();
		ValidatorCollection col = _validatorsByGroup [validationGroup] as ValidatorCollection;
		if (col == null) {
			col = new ValidatorCollection ();
			_validatorsByGroup [validationGroup] = col;
		}
		return col;
	}
	
	public virtual void Validate (string validationGroup)
	{
		if (validationGroup == null || validationGroup == "")
			ValidateCollection (_validators);
		else {
			if (_validatorsByGroup != null) {
				ValidateCollection (_validatorsByGroup [validationGroup] as ValidatorCollection);
			} else {
				_isValid = true;
			}
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
		if (requireStateControls == null) return;

		object[] state = (object[]) data;
		int max = Math.Min (requireStateControls.Count, state != null ? state.Length : requireStateControls.Count);
		for (int n=0; n < max; n++) {
			Control ctl = (Control) requireStateControls [n];
			ctl.LoadControlState (state != null ? state [n] : null);
		}
	}

	void LoadPreviousPageReference ()
	{
		if (_requestValueCollection != null) {
			string prevPage = _requestValueCollection [previousPageID];
			if (prevPage != null) {
				previousPage = (Page) PageParser.GetCompiledPageInstance (prevPage, Server.MapPath (prevPage), Context);
				previousPage.ProcessCrossPagePostBack (_context);
			} else {
				previousPage = _context.LastPage;
			}
		}
		_context.LastPage = this;
	}


	#endif
}
}
