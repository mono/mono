//
// System.Web.UI.Page
//
// Authors:
//	Duncan Mak  (duncan@ximian.com)
//	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Xml;

namespace System.Web.UI
{

public class Page : TemplateControl, IHttpHandler
{
	private string _culture;
	private bool _viewState = true;
	private bool _viewStateMac = false;
	private string _errorPage;
	private ArrayList _fileDependencies;
	private string _ID;
	private bool _isValid;
	private HttpServerUtility _server;
	private bool _smartNavigation = false;
	private TraceContext _trace;
	private bool _traceEnabled;
	private TraceMode _traceModeValue;
	private int _transactionMode;
	private string _UICulture;
	private HttpContext _context;
	private ValidatorCollection _validators;
	private bool _visible;
	private bool _renderingForm;
	private bool _hasForm;
	private object _savedViewState;
	private ArrayList _requiresPostBack;

	#region Fields
	 	protected const string postEventArgumentID = ""; //FIXME
		protected const string postEventSourceID = "";
	#endregion

	#region Constructor
	public Page ()
	{
		Page = this;
	}

	#endregion		

	#region Properties

	public HttpApplicationState Application
	{
		get { return _context.Application; }
	}

	bool AspCompatMode
	{
		set { throw new NotImplementedException (); }
	}

	bool Buffer
	{
		set { Response.BufferOutput = value; }
	}

	public Cache Cache
	{
		get { return _context.Cache; }
	}

	[MonoTODO]
	public string ClientTarget
	{
		get { throw new NotImplementedException (); }
		set { throw new NotImplementedException (); }
	}

	[MonoTODO]
	int CodePage
	{
		set { throw new NotImplementedException (); }
	}

	[MonoTODO]
	string ContentType
	{
		set { throw new NotImplementedException (); }
	}

	protected override HttpContext Context
	{
		get { return _context; }
	}

	string Culture
	{
		set { _culture = value; }
	}

	public override bool EnableViewState
	{
		get { return _viewState; }
		set { _viewState = value; }
	}

	protected bool EnableViewStateMac
	{
		get { return _viewStateMac; }
		set { _viewStateMac = value; }
	}

	public string ErrorPage
	{
		get { return _errorPage; }
		set { _errorPage = value; }
	}

	ArrayList FileDependencies
	{
		set { _fileDependencies = value; }
	}

	public override string ID
	{
		get { return _ID; }
		set { _ID = value; }
	}

	public bool IsPostBack
	{
		get {
			return (Request.HttpMethod == "POST");
		}
	}

	[MonoTODO]
	public bool IsReusable
	{
		get { throw new NotImplementedException (); }
	}

	public bool IsValid
	{
		get { return _isValid; }
	}

	[MonoTODO]
	int LCID {
		set { throw new NotImplementedException (); }
	}

	public HttpRequest Request
	{
		get { return _context.Request; }
	}

	public HttpResponse Response
	{
		get { return _context.Response; }
	}

	string ResponseEncoding
	{
		set { Response.ContentEncoding = Encoding.GetEncoding (value); }
	}

	public HttpServerUtility Server
	{
		get { return _server; }
	}

	public virtual HttpSessionState Session
	{
		get { return _context.Session; }
	}

	public bool SmartNavigation
	{
		get { return _smartNavigation; }
		set { _smartNavigation = value; }
	}

	public TraceContext Trace
	{
		get { return _trace; }
	}

	bool TraceEnabled
	{
		set { _traceEnabled = value; }
	}

	TraceMode TraceModeValue
	{
		set { _traceModeValue = value; }
	}

	int TransactionMode
	{
		set { _transactionMode = value; }
	}

	string UICulture
	{
		set { _UICulture = value; }
	}

	public IPrincipal User
	{
		get { return _context.User; }
	}

	public ValidatorCollection Validators
	{
		get { 
			if (_validators == null)
				_validators = new ValidatorCollection ();
			return _validators;
		}
	}

	public override bool Visible
	{
		get { return _visible; }
		set { _visible = value; }
	}

	#endregion

	#region Methods

	[MonoTODO]
	protected IAsyncResult AspCompatBeginProcessRequest (HttpContext context,
							     AsyncCallback cb, 
							     object extraData)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	protected void AspcompatEndProcessRequest (IAsyncResult result)
	{
		throw new NotImplementedException ();
	}
	
	protected virtual HtmlTextWriter CreateHtmlTextWriter (TextWriter tw)
	{
		return new HtmlTextWriter (tw);
	}

	[MonoTODO]
	public void DesignerInitialize ()
	{
		throw new NotImplementedException ();
	}

	protected virtual NameValueCollection DeterminePostBackMode ()
	{
		if (IsPostBack)
			return _context.Request.Form;

		return _context.Request.QueryString;
	}
	
	[MonoTODO]
	public string GetPostBackClientEvent (Control control, string argument)
	{
		// Don't throw the exception. keep going
		//throw new NotImplementedException ();
		StringBuilder result = new StringBuilder ();
		result.AppendFormat ("GetPostBackClientEvent ('{0}', '{1}')", control.ID, argument);
		return result.ToString ();
	}

	[MonoTODO]
	public string GetPostBackClientHyperlink (Control control, string argument)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public string GetPostBackEventReference (Control control)
	{
		// Don't throw the exception. keep going
		//throw new NotImplementedException ();
		return GetPostBackEventReference (control, "");
	}
	
	[MonoTODO]
	public string GetPostBackEventReference (Control control, string argument)
	{
		// Don't throw the exception. keep going
		//throw new NotImplementedException ();
		StringBuilder result = new StringBuilder ();
		result.AppendFormat ("GetPostBackEventReference ('{0}', '{1}')", control.ID, argument);
		return result.ToString ();
	}

	public virtual int GetTypeHashCode ()
	{
		return GetHashCode ();
	}

	[MonoTODO]
	protected virtual void InitOutputCache (int duration,
						string varyByHeader,
						string varyByCustom,
						OutputCacheLocation location,
						string varyByParam)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]
	public bool IsClientScriptBlockRegistered (string key)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]
	public bool IsStartupScriptRegistered (string key)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public string MapPath (string virtualPath)
	{
		throw new NotImplementedException ();
	}
	
	private void InvokeEventMethod (string m_name, object sender, EventArgs e)
	{
		Type [] types = new Type [] {typeof (object), typeof (EventArgs)};
		MethodInfo evt_method = GetType ().GetMethod (m_name, types);

		if (evt_method != null){
			object [] parms = new object [2];
			parms [0] = sender;
			parms [1] = e;
			evt_method.Invoke (this, parms);
		}
	}

	private bool _got_state = false;
	private int _random;
	internal void OnFormRender (HtmlTextWriter writer, string formUniqueID)
	{
		if (_hasForm)
			throw new HttpException ("Only 1 HtmlForm is allowed per page.");

		_renderingForm = true;
		_hasForm = true;
		writer.WriteLine();
		writer.Write("<input type=\"hidden\" name=\"__VIEWSTATE\" ");
		writer.WriteLine("value=\"{0}\" />", GetViewStateString ());
	}

	public string GetViewStateString ()
	{
		StringBuilder state_string = new StringBuilder ();
		state_string.AppendFormat ("{0:X}", GetTypeHashCode ());
		if (_context != null)
			state_string.Append (_context.Request.QueryString.GetHashCode ());

		if (!_got_state) {
			Random rnd = new Random ();
			_random = rnd.Next ();
			if (_random < 0)
				_random = -_random;
			_random++;
			_got_state = true;
		}

		state_string.AppendFormat ("{0:X}", _random);
		return state_string.ToString ();
	}

	internal void OnFormPostRender (HtmlTextWriter writer, string formUniqueID)
	{
		_renderingForm = false;
	}


	private void _Page_Init (object sender, EventArgs e)
	{
		InvokeEventMethod ("Page_Init", sender, e);
	}

	private void _Page_Load (object sender, EventArgs e)
	{
		InvokeEventMethod ("Page_Load", sender, e);
	}

	private void ProcessPostData ()
	{
		NameValueCollection data = DeterminePostBackMode ();
		ArrayList _raisePostBack = new ArrayList ();

		foreach (string id in data.AllKeys){
			string value = data [id];
			Control ctrl = FindControl (id);
			if (ctrl != null){
				IPostBackDataHandler pbdh = ctrl as IPostBackDataHandler;
				IPostBackEventHandler pbeh = ctrl as IPostBackEventHandler;
				if (pbdh != null) {
					if (pbdh.LoadPostData (id, data) == true) {
						pbdh.RaisePostDataChangedEvent ();
						if (pbeh == null)
							continue;
						if (_requiresPostBack != null &&
						    !(_requiresPostBack.Contains (ctrl)))
								_raisePostBack.Add (pbeh);
					}
					continue;
				}

				if (pbeh != null)
					pbeh.RaisePostBackEvent (null);
			}
		}

		foreach (IPostBackEventHandler e in _raisePostBack)
			e.RaisePostBackEvent (null);

		if (_requiresPostBack != null)
			foreach (IPostBackEventHandler e in _requiresPostBack)
				e.RaisePostBackEvent (null);
	}

	private bool init_done;
	public void ProcessRequest (HttpContext context)
	{
		if (!init_done){
			init_done = true;
			FrameworkInitialize ();
			// This 2 should depend on AutoEventWireUp in Page directive. Defaults to true.
			Init += new EventHandler (_Page_Init);
			Load += new EventHandler (_Page_Load);

			//-- Control execution lifecycle in the docs
			OnInit (EventArgs.Empty);
		}
		_got_state = false;
		_hasForm = false;
		_context = context;
		//LoadViewState ();
		//if (this is IPostBackDataHandler)
		//	LoadPostData ();
		if (IsPostBack)
			ProcessPostData ();

		OnLoad (EventArgs.Empty);
		//if (this is IPostBackDataHandler)
		//	RaisePostBackEvent ();
		OnPreRender (EventArgs.Empty);

		//--
		HtmlTextWriter output = new HtmlTextWriter (context.Response.Output);
		foreach (Control ctrl in Controls)
			ctrl.RenderControl (output);

		//SavePageViewState ();
	}

	protected virtual void RaisePostBackEvent (IPostBackEventHandler sourceControl, string eventArgument)
	{
		sourceControl.RaisePostBackEvent (eventArgument);
	}
	
	[MonoTODO]
	public void RegisterArrayDeclaration (string arrayName, string arrayValue)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]
	public virtual void RegisterClientScriptBlock (string key, string script)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]
	public virtual void RegisterHiddenField (string hiddenFieldName, string hiddenFieldInitialValue)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]
	public void RegisterClientScriptFile (string a, string b, string c)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public void RegisterOnSubmitStatement (string key, string script)
	{
		throw new NotImplementedException ();
	}
	
	public void RegisterRequiresPostBack (Control control)
	{
		if (_requiresPostBack == null)
			_requiresPostBack = new ArrayList ();

		_requiresPostBack.Add (control);
	}

	[MonoTODO]
	public virtual void RegisterRequiresRaiseEvent (IPostBackEventHandler control)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public void RegisterViewStateHandler ()
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	protected virtual void SavePageStatetoPersistenceMedium (object viewState)
	{
		throw new NotImplementedException ();
	}
	
	protected virtual object LoadPageStateFromPersistenceMedium ()
	{
		return _savedViewState;
	}

	internal void LoadPageViewState()
	{
		object sState = LoadPageStateFromPersistenceMedium ();
		if (sState != null)
			LoadViewStateRecursive (sState);
	}

	protected virtual void SavePageStateToPersistenceMedium (object viewState)
	{
		_savedViewState = viewState;
	}

	private void SaveControlState (Control ctrl, Triplet savedData, XmlTextWriter writer)
	{
		writer.WriteStartElement ("control");
		writer.WriteAttributeString ("id", ctrl.ID);
		writer.WriteAttributeString ("type", ctrl.GetType ().ToString ());
		StateBag state = savedData.First as StateBag;
		if (state != null){
			foreach (string key in state.Keys){
				object o = state [key];
				writer.WriteStartElement ("item");
				writer.WriteAttributeString ("key", key);
				if (o  == null)
					o = "";
				else if (o is string)
					writer.WriteAttributeString ("value", o as string);
				else
					//FIXME: add more conversions to string for other types.
					throw new NotSupportedException (o.GetType ().ToString ());

				writer.WriteEndElement ();
			}

			ArrayList controlList = savedData.Second as ArrayList;
			ArrayList stateList = savedData.Third as ArrayList;
			int idx = 0;
			foreach (Control child in controlList)
				SaveControlState (child, stateList [idx++] as Triplet, writer);
		}
		writer.WriteEndElement ();
	}

	internal void SavePageViewState ()
	{
		//SavePageStateToPersistenceMedium (SaveViewStateRecursive ());
		string outputFile = "page-" + this.ToString () + ".xml";
		XmlTextWriter xmlWriter = new XmlTextWriter (outputFile, Encoding.UTF8);
		xmlWriter.Formatting = Formatting.Indented;
		xmlWriter.Indentation = 4;
		xmlWriter.WriteStartDocument (true);
		xmlWriter.WriteStartElement ("viewstate");
		Triplet savedState = SaveViewStateRecursive () as Triplet;
		SaveControlState (this, savedState, xmlWriter);
		xmlWriter.WriteEndElement ();
		xmlWriter.Close ();
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

	public virtual void VerifyRenderingInServerForm (Control control)
	{
		if (!_renderingForm)
			throw new HttpException ("Control '" + control.ClientID + " " + control.GetType () + 
						 "' must be rendered within a HtmlForm");
	}

	#endregion
}
}
