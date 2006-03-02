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
public class Page : TemplateControl, IHttpHandler
{
	private bool _eventValidation = true;
	private bool _viewState = true;
	private bool _viewStateMac;
	private string _errorPage;
	private bool is_validated;
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
	ClientScriptManager scriptManager;
	bool allow_load; // true when the Form collection belongs to this page (GetTypeHashCode)

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
	internal const string CallbackArgumentID = "__CALLBACKARGUMENT";
	internal const string CallbackSourceID = "__CALLBACKTARGET";
	internal const string PreviousPageID = "__PREVIOUSPAGE";
	
	HtmlHead htmlHeader;
	
	MasterPage masterPage;
	string masterPageFile;
	
	Page previousPage;
	bool isCrossPagePostBack;
	ArrayList requireStateControls;
	Hashtable _validatorsByGroup;
	HtmlForm _form;

	string _title;
	string _theme;
	string _styleSheetTheme;
#endif

	#region Constructor
	public Page ()
	{
		scriptManager = new ClientScriptManager (this);
		Page = this;
		ID = "__Page";
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
		set { Thread.CurrentThread.CurrentCulture = new CultureInfo (value); }
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
		set { _eventValidation = value;}
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
			return _requestValueCollection != null;
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

			return ValidateCollection (_validators);
		}
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

#if NET_2_0
	public PageAdapter PageAdapter {
		get {
			return (PageAdapter)Adapter;
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
#endif

#if NET_2_0
	[Localizable (true)] 
	[Bindable (true)] 
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public string Title {
		get { return _title; }
		set { _title = value; }
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

#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public string UICulture
	{
		get { return Thread.CurrentThread.CurrentUICulture.Name; }
		set { Thread.CurrentThread.CurrentUICulture = new CultureInfo (value); }
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
		if (0 == String.Compare (Request.HttpMethod, "POST", true, CultureInfo.InvariantCulture)) {
			coll =	req.Form;
			WebROCollection c = (WebROCollection) coll;
			allow_load = !c.GotID;
			if (allow_load) {
				c.ID = GetTypeHashCode ();
			} else {
				allow_load = (c.ID == GetTypeHashCode ());
			}
		} else  {
			coll = req.QueryString;
		}

		
		if (coll == null || coll ["__VIEWSTATE"] == null)
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

	LosFormatter GetFormatter ()
	{
#if NET_2_0
		PagesSection config = (PagesSection) WebConfigurationManager.GetSection ("system.web/pages");
#else
		PagesConfiguration config = PagesConfiguration.GetInstance (_context);
#endif
		byte [] vkey = null;
		if (config.EnableViewStateMac) {
#if NET_2_0
			MachineKeySection mconfig = (MachineKeySection) WebConfigurationManager.GetSection ("system.web/machineKey");
			vkey = mconfig.ValidationKeyBytes;
#else
			MachineKeyConfig mconfig = HttpContext.GetAppConfig ("system.web/machineKey") as MachineKeyConfig;
			vkey = mconfig.ValidationKey;
#endif
		}

		return new LosFormatter (config.EnableViewStateMac, vkey);
	}

	string GetViewStateString ()
	{
		if (_savedViewState == null)
			return null;

		LosFormatter fmt = GetFormatter ();
		MemoryStream ms = new MemoryStream ();
		fmt.Serialize (ms, _savedViewState);
		return Convert.ToBase64String (ms.GetBuffer (), 0, (int) ms.Length);
	}

	internal object GetSavedViewState ()
	{
		return _savedViewState;
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
		}
		_requiresPostBack = list1;
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
#if NET_2_0 || TARGET_JVM
	public virtual void ProcessRequest (HttpContext context)
#else
	public void ProcessRequest (HttpContext context)
#endif
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
		} catch (ThreadAbortException) {
			// Do nothing, just ignore it by now.
		} catch (Exception e) {
			context.AddError (e); // OnError might access LastError
			OnError (EventArgs.Empty);
			context.ClearError (e);
			// We want to remove that error, as we're rethrowing to stop
			// further processing.
			throw;
		} finally {
			try {
				UnloadRecursive (true);
			} catch {}
			if (Thread.CurrentThread.CurrentCulture.Equals (culture) == false)
				Thread.CurrentThread.CurrentCulture = culture;

			if (Thread.CurrentThread.CurrentUICulture.Equals (uiculture) == false)
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
		
		ApplyMasterPage ();

		if (_title != null && htmlHeader != null)
			htmlHeader.Title = _title;
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
	
	void RaisePostBackEvents ()
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

	internal string GetSubmitStatements ()
	{
		return scriptManager.WriteSubmitStatements ();
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
		if (view_state == "")
			return null;

		LosFormatter fmt = GetFormatter ();
		try {
			_savedViewState = fmt.Deserialize (view_state);
		} catch (Exception e) {
			throw new HttpException ("Error restoring page viewstate.", e);
		}
		return _savedViewState;
	}

	internal void LoadPageViewState()
	{
		object sState = LoadPageStateFromPersistenceMedium ();
		if (sState != null) {
#if NET_2_0
			Triplet data = (Triplet) sState;
			if (allow_load) {
				LoadPageControlState (data.Third);
				LoadViewStateRecursive (data.First);
				_requiresPostBack = data.Second as ArrayList;
			}
#else
			Pair pair = (Pair) sState;
			if (allow_load) {
				LoadViewStateRecursive (pair.First);
				_requiresPostBack = pair.Second as ArrayList;
			}
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
		is_validated = true;
		ValidateCollection (_validators);
	}

	internal virtual bool AreValidatorsUplevel ()
	{
		bool uplevel = false;

		foreach (IValidator v in Validators) {
			BaseValidator bv = v as BaseValidator;
			if (bv == null) continue;

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

		if (!renderingForm)
			throw new HttpException ("Control '" +
						 control.ClientID +
						 "' of type '" +
						 control.GetType ().Name +
						 "' must be placed inside a form tag with runat=server.");
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
		get { return previousPage; }
	}

	
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[BrowsableAttribute (false)]
	public bool IsCallback {
		get { return _requestValueCollection != null && _requestValueCollection [CallbackArgumentID] != null; }
	}
	
	[BrowsableAttribute (false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public bool IsCrossPagePostBack {
		get { return _requestValueCollection != null && isCrossPagePostBack; }
	}
	
	string ProcessCallbackData ()
	{
		string callbackTarget = _requestValueCollection [CallbackSourceID];
		if (callbackTarget == null || callbackTarget.Length == 0)
			throw new HttpException ("Callback target not provided.");

		ICallbackEventHandler target = FindControl (callbackTarget) as ICallbackEventHandler;
		if (target == null)
			throw new HttpException (string.Format ("Invalid callback target '{0}'.", callbackTarget));

		string callbackArgument = _requestValueCollection [CallbackArgumentID];
		target.RaiseCallbackEvent (callbackArgument);
		return target.GetCallbackResult ();
	}

	[BrowsableAttribute (false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public HtmlHead Header {
		get { return htmlHeader; }
	}
	
	internal void SetHeader (HtmlHead header)
	{
		htmlHeader = header;
	}
	
	void ApplyMasterPage ()
	{
		if (masterPageFile != null) {
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
			if (masterPage == null)
				masterPage = MasterPage.CreateMasterPage (this, Context, masterPageFile, contentTemplates);

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
		is_validated = true;
		if (validationGroup == null || validationGroup == "")
			ValidateCollection (_validators);
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
			string prevPage = _requestValueCollection [PreviousPageID];
			if (prevPage != null) {
				previousPage = (Page) PageParser.GetCompiledPageInstance (prevPage, Server.MapPath (prevPage), Context);
				previousPage.ProcessCrossPagePostBack (_context);
			} else {
				previousPage = _context.LastPage;
			}
		}
		_context.LastPage = this;
	}


	Hashtable contentTemplates;
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected internal void AddContentTemplate (string templateName, ITemplate template)
	{
		if (contentTemplates == null)
			contentTemplates = new Hashtable ();
		contentTemplates [templateName] = template;
	}
		
	#endif
}
}
