//
// System.Web.UI.Page.cs
//
// Authors:
//   Duncan Mak  (duncan@ximian.com)
//   Gonzalo Paniagua (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002,2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Web.Util;

namespace System.Web.UI
{

// TODO FIXME missing the IRootDesigner Attribute
[DefaultEvent ("Load"), DesignerCategory ("ASPXCodeBehind")]
[ToolboxItem (false)]
[Designer ("System.Web.UI.Design.ControlDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
[RootDesignerSerializer ("Microsoft.VSDesigner.WebForms.RootCodeDomSerializer, " + Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design, true)]
public class Page : TemplateControl, IHttpHandler
{
	private string _culture;
	private bool _viewState = true;
	private bool _viewStateMac = false;
	private string _errorPage;
	private string _ID;
	private bool _isValid;
	private bool _smartNavigation = false;
	private TraceContext _trace;
	private bool _traceEnabled;
	private TraceMode _traceModeValue;
	private int _transactionMode;
	private string _UICulture;
	private HttpContext _context;
	private ValidatorCollection _validators;
	private bool renderingForm;
	private object _savedViewState;
	private ArrayList _requiresPostBack;
	private ArrayList requiresPostDataChanged;
	private IPostBackEventHandler requiresRaiseEvent;
	private NameValueCollection secondPostData;
	private bool requiresPostBackScript = false;
	private bool postBackScriptRendered = false;
	Hashtable clientScriptBlocks;
	Hashtable startupScriptBlocks;
	Hashtable hiddenFields;
	bool handleViewState;

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected const string postEventArgumentID = "__EVENTARGUMENT";
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected const string postEventSourceID = "__EVENTTARGET";

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
	protected bool AspCompatMode
	{
		set { throw new NotImplementedException (); }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected bool Buffer
	{
		set { Response.BufferOutput = value; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public Cache Cache
	{
		get { return _context.Cache; }
	}

	[MonoTODO]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false), DefaultValue ("")]
	[WebSysDescription ("Value do override the automatic browser detection and force the page to use the specified browser.")]
	public string ClientTarget
	{
		get { throw new NotImplementedException (); }
		set { throw new NotImplementedException (); }
	}

	[MonoTODO]
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected int CodePage
	{
		set { throw new NotImplementedException (); }
	}

	[MonoTODO]
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected string ContentType
	{
		set { throw new NotImplementedException (); }
	}

	protected override HttpContext Context
	{
		get { return _context; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected string Culture
	{
		set { _culture = value; }
	}

	[Browsable (false)]
	public override bool EnableViewState
	{
		get { return _viewState; }
		set { _viewState = value; }
	}

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
		set { _errorPage = value; }
	}

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
		get { return _ID; }
		set { _ID = value; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public bool IsPostBack
	{
		get {
			return (0 == String.Compare (Request.HttpMethod, "POST", true));
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

	[MonoTODO]
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected int LCID {
		set { throw new NotImplementedException (); }
	}

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
	protected string ResponseEncoding
	{
		set { Response.ContentEncoding = Encoding.GetEncoding (value); }
	}

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
		get { return _context.Session; }
	}

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
		get { return _trace; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected bool TraceEnabled
	{
		set { _traceEnabled = value; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected TraceMode TraceModeValue
	{
		set { _traceModeValue = value; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected int TransactionMode
	{
		set { _transactionMode = value; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected string UICulture
	{
		set { _UICulture = value; }
	}

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

	[Browsable (false)]
	public override bool Visible
	{
		get { return base.Visible; }
		set { base.Visible = value; }
	}

	#endregion

	#region Methods

	[MonoTODO]
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected IAsyncResult AspCompatBeginProcessRequest (HttpContext context,
							     AsyncCallback cb, 
							     object extraData)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
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

	[MonoTODO]
	[EditorBrowsable (EditorBrowsableState.Never)]
	public void DesignerInitialize ()
	{
		throw new NotImplementedException ();
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
		if (IsPostBack)
			coll =  req.Form;
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
		return String.Format ("__doPostBack ('{0}', '{1}')", control.UniqueID, argument);
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

	[MonoTODO]
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected virtual void InitOutputCache (int duration,
						string varyByHeader,
						string varyByCustom,
						OutputCacheLocation location,
						string varyByParam)
	{
		throw new NotImplementedException ();
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public bool IsClientScriptBlockRegistered (string key)
	{
		if (clientScriptBlocks == null)
			return false;

		return clientScriptBlocks.ContainsKey (key);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public bool IsStartupScriptRegistered (string key)
	{
		if (startupScriptBlocks == null)
			return false;

		return startupScriptBlocks.ContainsKey (key);
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
		writer.WriteLine ("\tfunction __doPostBack(eventTarget, eventArgument) {");
		writer.WriteLine ("\t\tvar theform = document.{0};", formUniqueID);
		writer.WriteLine ("\t\ttheform.{0}.value = eventTarget;", postEventSourceID);
		writer.WriteLine ("\t\ttheform.{0}.value = eventArgument;", postEventArgumentID);
		writer.WriteLine ("\t\ttheform.submit();");
		writer.WriteLine ("\t}");
		writer.WriteLine ("// -->");
		writer.WriteLine ("</script>");
	}

	static void WriteScripts (HtmlTextWriter writer, Hashtable scripts)
	{
		if (scripts == null)
			return;

		foreach (string key in scripts.Values)
			writer.WriteLine (key);
	}
	
	void WriteHiddenFields (HtmlTextWriter writer)
	{
		if (hiddenFields == null)
			return;

		foreach (string key in hiddenFields.Keys) {
			string value = hiddenFields [key] as string;
			writer.WriteLine ("\n<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", key, value);
		}

		hiddenFields = null;
	}

	internal void OnFormRender (HtmlTextWriter writer, string formUniqueID)
	{
		if (renderingForm)
			throw new HttpException ("Only 1 HtmlForm is allowed per page.");

		renderingForm = true;
		writer.WriteLine ();
		WriteHiddenFields (writer);
		if (requiresPostBackScript) {
			RenderPostBackScript (writer, formUniqueID);
			postBackScriptRendered = true;
		}

		if (handleViewState) {
			writer.Write ("<input type=\"hidden\" name=\"__VIEWSTATE\" ");
			writer.WriteLine ("value=\"{0}\" />", GetViewStateString ());
		}

		WriteScripts (writer, clientScriptBlocks);
	}

	internal string GetViewStateString ()
	{
		StringWriter sr = new StringWriter ();
		LosFormatter fmt = new LosFormatter ();
		fmt.Serialize (sr, _savedViewState);
		return sr.GetStringBuilder ().ToString ();
	}

	internal void OnFormPostRender (HtmlTextWriter writer, string formUniqueID)
	{
		if (!postBackScriptRendered && requiresPostBackScript)
			RenderPostBackScript (writer, formUniqueID);

		WriteHiddenFields (writer);
		WriteScripts (writer, startupScriptBlocks);
		renderingForm = false;
		postBackScriptRendered = false;
	}

	private void ProcessPostData (NameValueCollection data, bool second)
	{
		if (data == null)
			return;

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
			} else if (!second) {
				if (secondPostData == null)
					secondPostData = new NameValueCollection ();
				secondPostData.Add (real_id, data [id]);
			}
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	public void ProcessRequest (HttpContext context)
	{
		_context = context;
		WebTrace.PushContext ("Page.ProcessRequest ()");
		WebTrace.WriteLine ("Entering");
		WireupAutomaticEvents ();
		WebTrace.WriteLine ("Finished hookup");
		//-- Control execution lifecycle in the docs
		WebTrace.WriteLine ("FrameworkInitialize");
		FrameworkInitialize ();
		WebTrace.WriteLine ("InitRecursive");
		InitRecursive (null);
		renderingForm = false;	
		if (IsPostBack) {
			LoadPageViewState ();
			ProcessPostData (DeterminePostBackMode (), false);
		}

		WebTrace.WriteLine ("LoadRecursive");
		LoadRecursive ();
		if (IsPostBack) {
			ProcessPostData (secondPostData, true);
			RaiseChangedEvents ();
			RaisePostBackEvents ();
		}
		WebTrace.WriteLine ("PreRenderRecursiveInternal");
		PreRenderRecursiveInternal ();

		WebTrace.WriteLine ("SavePageViewState");
		SavePageViewState ();
		//--
		HtmlTextWriter output = new HtmlTextWriter (context.Response.Output);
		WebTrace.WriteLine ("RenderControl");
		RenderControl (output);
		_context = null;
		WebTrace.WriteLine ("UnloadRecursive");
		UnloadRecursive (true);
		WebTrace.WriteLine ("End");
		WebTrace.PopContext ();
	}
	
	internal void RaisePostBackEvents ()
	{
		if (requiresRaiseEvent != null) {
			RaisePostBackEvent (requiresRaiseEvent, null);
			return;
		}

		NameValueCollection postdata = DeterminePostBackMode ();
		if (postdata == null)
			return;

		string eventTarget = postdata [postEventSourceID];
		if (eventTarget == null || eventTarget.Length == 0)
			return;

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

		requiresPostDataChanged.Clear ();
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	protected virtual void RaisePostBackEvent (IPostBackEventHandler sourceControl, string eventArgument)
	{
		sourceControl.RaisePostBackEvent (eventArgument);
	}
	
	[MonoTODO]
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void RegisterArrayDeclaration (string arrayName, string arrayValue)
	{
		throw new NotImplementedException ();
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void RegisterClientScriptBlock (string key, string script)
	{
		if (IsClientScriptBlockRegistered (key))
			return;

		if (clientScriptBlocks == null)
			clientScriptBlocks = new Hashtable ();

		clientScriptBlocks.Add (key, script);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void RegisterHiddenField (string hiddenFieldName, string hiddenFieldInitialValue)
	{
		if (hiddenFields == null)
			hiddenFields = new Hashtable ();

		if (!hiddenFields.ContainsKey (hiddenFieldName))
			hiddenFields.Add (hiddenFieldName, hiddenFieldInitialValue);
	}
	
 	[MonoTODO]
	public void RegisterClientScriptFile (string a, string b, string c)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void RegisterOnSubmitStatement (string key, string script)
	{
		throw new NotImplementedException ();
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void RegisterRequiresPostBack (Control control)
	{
		if (_requiresPostBack == null)
			_requiresPostBack = new ArrayList ();

		_requiresPostBack.Add (control.ID);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void RegisterRequiresRaiseEvent (IPostBackEventHandler control)
	{
		requiresRaiseEvent = control;
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void RegisterStartupScript (string key, string script)
	{
		if (IsStartupScriptRegistered (key))
			return;

		if (startupScriptBlocks == null)
			startupScriptBlocks = new Hashtable ();

		startupScriptBlocks.Add (key, script);
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
		NameValueCollection postdata = DeterminePostBackMode ();
		string view_state;
		if (postdata == null || (view_state = postdata ["__VIEWSTATE"]) == null)
			return null;

		_savedViewState = null;
		LosFormatter fmt = new LosFormatter ();

		try { 
			_savedViewState = fmt.Deserialize (view_state);
		} catch (Exception e) {
			throw new HttpException ("Error restoring page viewstate.\n{0}", e);
		}

		return _savedViewState;
	}

	internal void LoadPageViewState()
	{
		WebTrace.PushContext ("LoadPageViewState");
		object sState = LoadPageStateFromPersistenceMedium ();
		WebTrace.WriteLine ("sState = '{0}'", sState);
		if (sState != null) {
			Pair pair = (Pair) sState;
			LoadViewStateRecursive (pair.First);
			_requiresPostBack = pair.Second as ArrayList;
		}
		WebTrace.PopContext ();
	}

	internal void SavePageViewState ()
	{
		if (!handleViewState)
			return;

		Pair pair = new Pair ();
		pair.First = SaveViewStateRecursive ();
		if (_requiresPostBack != null && _requiresPostBack.Count > 0)
			pair.Second = _requiresPostBack;
		SavePageStateToPersistenceMedium (pair);
	}

	public virtual void Validate ()
	{
		if (_validators == null || _validators.Count == 0){
			_isValid = true;
			return;
		}

		bool all_valid = true;
		foreach (IValidator v in _validators){
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
}
}
